using EFS.ACommon;
using EFS.ApplicationBlocks.Data;
using EFS.ApplicationBlocks.Data.Extension;
using EFS.Common;
using EFS.Common.Web;
using EfsML.Business;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

namespace EFS.Spheres
{
    #region TrackerDataReader
    /// <summary>
    /// 
    /// </summary>
    // EG 20180525 [23979] IRQ Processing
    public class TrackerDataReader
    {
        #region Members
        public MapDataReaderRow mapDr;
        #endregion
        #region Accessors
        // EG 20180525 [23979] IRQ Processing
        public bool IsIRQ
        {
            get
            {
                return Convert.ToBoolean(mapDr["IRQ"]);
            }
        }
        public bool IsRowUpdated
        {
            get
            {
                DateTime dtIns = Convert.ToDateTime(mapDr["DTINS"].Value);
                DateTime dtUpd = Convert.IsDBNull(mapDr["DTUPD"].Value) ? Convert.ToDateTime(null) : Convert.ToDateTime(mapDr["DTUPD"].Value);
                return (dtUpd.CompareTo(dtIns) >= 0);
            }
        }
        #endregion  Accessors
        #region Constructor
        public TrackerDataReader()
        {
        }
        public TrackerDataReader(MapDataReaderRow pMapDr)
        {
            mapDr = pMapDr;
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
            return mapDr["READYSTATE"].Value.ToString();
        }
        #endregion GetReadyState
        #region GetStatus
        public string GetStatus()
        {
            return mapDr["STATUSTRACKER"].Value.ToString();
        }
        #endregion GetStatus
        #endregion Methods
    }
    #endregion TrackerDataReader

    #region TrackerContent
    // EG 20170125 [Refactoring URL] Upd
    public partial class TrackerContent : PageBase
    {
        // EG 20200818 [XXXXX] Réduction Accès ReadSQLCookie (TrackerHistoric) sur Tracker
        protected void Page_Load(object sender, EventArgs e)
        {
            string mode = Request.QueryString["mode"];
            //string[] temp = null;
            switch (mode)
            {
                case "GRPLOAD":
                    string group = Request.QueryString["group"];
                    string readyState = Request.QueryString["readystate"];
                    string histo = Request.QueryString["histo"];
                    int top = SessionTools.TrackerNbRowPerGroup;
                    Cst.GroupTrackerEnum groupTracker = Cst.GroupTrackerEnum.ALL;
                    if (Enum.IsDefined(typeof(Cst.GroupTrackerEnum), group.ToUpper()))
                        groupTracker = (Cst.GroupTrackerEnum)Enum.Parse(typeof(Cst.GroupTrackerEnum), group, true);
                    LoadTrackerData(groupTracker, readyState, histo, top);
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
        // EG 20170125 [Refactoring URL] Upd
        // EG 20180525 [23979] IRQ Processing
        // EG 20180619 Add (ti.INSTRUMENTNO = 1) for join TRADEINSTRUMENT
        // EG/PL 20181126 Refactoring ORDER BY 
        // EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        // EG 20200731 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc) Contournement ROWNUM/ORDER en mode FAVORIS
        // EG 20200818 [XXXXX] Réduction Accès ReadSQLCookie (TrackerHistoric) sur Tracker via valeur passée en paramètre
        // EG 20200831 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc) Contournement ROWNUM/ORDER en mode FAVORIS/GROUP pour Oracle
        // EG 20200922 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc) Mise en gras des traitements du jour
        // EG 20210419 [XXXXX] Calcul date histo pour 1D (Décalage d'1 jour ouvré sur la base de la date système et du BC de l'entité en vigueur)
        protected void LoadTrackerData(Cst.GroupTrackerEnum pGroupTracker, string pReadyState, string pHisto, int pTop)
        {
            IDbTransaction dbTransaction = null;
            HtmlGenericControl ul = new HtmlGenericControl("ul");
            string resDetail = Ressource.GetString("Detail");
            HtmlGenericControl li = null;
            string cs = SessionTools.CS;
            try 
            {
                DataParameters parameters = new DataParameters();
                parameters.Add(new DataParameter(cs, "ISMARKED", DbType.Boolean), false);

                string sqlSelect = @"select {0} tk.SYSCODE, tk.SYSNUMBER, tk.IDTRK_L, tk.GROUPTRACKER, tk.READYSTATE, tk.STATUSTRACKER, 
                tk.IDDATA, tk.IDDATAIDENT, tk.IDDATAIDENTIFIER, tk.DATA1, tk.DATA2, tk.DATA3, tk.DATA4, tk.DATA5, tk.DATA1IDENT, 
                tk.DTINS, tk.DTUPD, isnull(tk.IRQ, 0) as IRQ, 
                case tk.STATUSTRACKER 
                    when 'ERROR' then 0 
                    when 'IRQ' then 1 
                    when 'WARNING' then 2 
                    when 'PENDING' then 3 
                    when 'PROGRESS' then 4 
                    when 'NA' then 5 
                    when 'NONE' then 6 
                    when 'SUCCESS' then 7 end as STATUSORDER
                from dbo.TRACKER_L tk
                where (tk.ISMARKED = @ISMARKED)" + Cst.CrLf;

                if (pGroupTracker != Cst.GroupTrackerEnum.ALL)
                {
                    parameters.Add(new DataParameter(cs, "GROUPTRACKER", DbType.AnsiString, SQLCst.UT_ENUM_MANDATORY_LEN), pGroupTracker.ToString());
                    sqlSelect += " and (tk.GROUPTRACKER = @GROUPTRACKER)";
                }
                else
                {
                    parameters.Add(new DataParameter(cs, "GROUPFAV", DbType.Int32), SessionTools.TrackerGroupFav);

                    sqlSelect += @" and (0 < " + DataHelper.SQLBitand(cs, "@GROUPFAV", 
                       @"case tk.GROUPTRACKER when 'TRD' then 1 
                                              when 'IO'  then 2 
                                              when 'MSG' then 4 
                                              when 'ACC' then 8
                                              when 'CLO' then 16 
                                              when 'INV' then 32 
                                              when 'EXT' then 64 end") + ")" ;


                    //sqlSelect += @" and (0 < @GROUPFAV & case tk.GROUPTRACKER when 'TRD' then 1 when 'IO'  then 2 when 'MSG' then 4 when 'ACC' then 8
                    //when 'CLO' then 16 when 'INV' then 32 when 'EXT' then 64 end)";
                    pTop *= 2;
                }

                if (StrFunc.IsFilled(pReadyState))
                {
                    parameters.Add(new DataParameter(cs, "READYSTATE", DbType.AnsiString, SQLCst.UT_ENUM_MANDATORY_LEN), pReadyState);
                    sqlSelect += " and (tk.READYSTATE = @READYSTATE)";
                }

                //string histo = SessionTools.TrackerHistoric;
                if (StrFunc.IsFilled(pHisto) && ("Beyond" != pHisto))
                {
                    // FI 20200810 [XXXXX] Convertion de la date en UTC
                    //DateTime dtReference = new DtFunc().StringToDateTime("-" + pHisto);
                    //if (dtReference != DateTime.MinValue)
                    //    dtReference = DtFuncExtended.ConvertTimeToTz(new DateTimeTz(dtReference, SessionTools.GetCriteriaTimeZone()), "Etc/UTC").Date;
                    // FI 20201028 [XXXXX] usage du paramètre DTINSDATETIME2 de type datetime2 Evite à Oracle l'usage de INTERNAL_FUNCTION
                    DateTime dtReference = Tools.GetTrackerDtHisto(SessionTools.CS, SessionTools.User.Entity_IdA, SessionTools.User.Entity_BusinessCenter, pHisto, SessionTools.GetCriteriaTimeZone());
                    parameters.Add(DataParameter.GetParameter(cs, DataParameter.ParameterEnum.DTINSDATETIME2), dtReference);
                    sqlSelect += " and (isnull(tk.DTUPD, tk.DTINS) >= @DTINS)";
                }

                //PL 20130703 Newness TEST
                bool isTrackerApplyRestrict = BoolFunc.IsTrue(SystemSettings.GetAppSettings("Spheres_TrackerApplyRestrict"));
                if ((!SessionTools.User.IsSessionSysAdmin) && isTrackerApplyRestrict)
                {
                    parameters.Add(DataParameter.GetParameter(cs, DataParameter.ParameterEnum.IDAINS), SessionTools.User.IdA);
                    sqlSelect += " and (tk.IDAINS = @IDAINS)";
                }

                string sqlWhere = string.Empty;

                string sqlRowNumber = @"ROW_NUMBER() over (order by  isnull(tk.DTUPD,tk.DTINS) desc,
                            case tk.STATUSTRACKER 
                            when 'ERROR' then 0 
                            when 'IRQ' then 1 
                            when 'WARNING' then 2 
                            when 'PENDING' then 3 
                            when 'PROGRESS' then 4 
                            when 'NA' then 5 
                            when 'NONE' then 6 
                            when 'SUCCESS' then 7 end) as RANK_COL,";

                string sqlOrder = @"order by RANK_COL";

                string sqlTop = string.Empty;
                if (0 < pTop)
                    sqlTop = String.Format("offset 0 row fetch next {0} rows only", pTop);

                sqlSelect = String.Format(sqlSelect, sqlRowNumber);
                
                string messageTracker = @"replace(replace(replace(replace(replace(isnull(smd.MESSAGE,smd_gb.MESSAGE),
                '{1}',isnull(tk.DATA1,'{1}')),
                '{2}',isnull(tk.DATA2,'{2}')),
                '{3}',isnull(tk.DATA3,'{3}')),
                '{4}',isnull(tk.DATA4,'{4}')),
                '{5}',isnull(tk.DATA5,'{5}'))";


                // PL 20190207 Newness 
                // EG 20190214 Messages Tracker 
                // PL 20190227 Newness Add tk.DATA2, tk.DATA3 and Remove Case When Else End
                //     ||
                //     case when tk.GROUPTRACKER = 'IO'  then ' ' || isnull(tk.DATA1,'') else
                //     case when tk.GROUPTRACKER = 'MSG' and tk.SYSNUMBER in (2020, 2021) then ' ' || isnull(tk.DATA3,'') || ' ' || isnull(tk.DATA1,'') else
                //     case when tk.GROUPTRACKER = 'CLO' and tk.SYSNUMBER in (4100,4105,4110,4115,4060) then ' ' || isnull(tk.DATA2,'') || ' ' || isnull(tk.DATA3,'') else '' end end
                //end 
                // FI 20210330 [XXXXX] Utilisation de la table TRADE à la place de TRADEINSTRUMENT
                string sqlQuery = String.Format(@"select tk.IDTRK_L, tk.SYSCODE, tk.SYSNUMBER, t.IDT, pr.FAMILY, pr.GPRODUCT, 
                    tk.GROUPTRACKER, tk.READYSTATE, tk.STATUSTRACKER, 
                    tk.IDDATA, tk.IDDATAIDENT, tk.IDDATAIDENTIFIER, tk.DATA1, tk.DATA1IDENT, tk.DATA2, tk.DATA3,
                    tk.DTINS, tk.DTUPD, tk.IRQ, tk.STATUSORDER, 
                    case when smd.SYSCODE is null and smd_gb.SYSCODE is null then 'Request' 
                         else isnull(smd.SHORTMESSAGE, smd_gb.SHORTMESSAGE) end as SHORTMESSAGETRACKER, 
                    case when smd.SYSCODE is null and smd_gb.SYSCODE is null then 'No Message' 
                    else {0} end as MESSAGETRACKER 
                    from ({1}) tk
                    left outer join dbo.SYSTEMMSG sm on (sm.SYSCODE = tk.SYSCODE) and (sm.SYSNUMBER = tk.SYSNUMBER)
                    left outer join dbo.SYSTEMMSGDET smd on (smd.SYSCODE = sm.SYSCODE) and (smd.SYSNUMBER = sm.SYSNUMBER) and (smd.CULTURE = '{2}')
                    left outer join dbo.SYSTEMMSGDET smd_gb on (smd_gb.SYSCODE = sm.SYSCODE) and (smd_gb.SYSNUMBER = sm.SYSNUMBER) and (smd_gb.CULTURE = 'en')
                    left outer join dbo.TRADE t on (t.IDT = tk.IDDATA) and (substring(tk.IDDATAIDENT,1,5) = 'TRADE')
                    left outer join dbo.INSTRUMENT ns on (ns.IDI = t.IDI)
                    left outer join dbo.PRODUCT pr on (pr.IDP = ns.IDP)",
                    messageTracker, Cst.CrLf + sqlSelect, SessionTools.Collaborator_Culture_ISOCHAR2);

                QueryParameters qryParameters = new QueryParameters(cs, sqlQuery + Cst.CrLf + sqlWhere + Cst.CrLf + sqlOrder + Cst.CrLf + sqlTop, parameters);
                List<MapDataReaderRow> mapDrList = null;

                //PL 20200806 Do not use dbTransaction on Oracle, in order not to cause the opening of a new physical connection in the connection pool ADO.NET
                if (DataHelper.IsDbOracle(SessionTools.CS))
                {
                    using (IDataReader dr = DataHelper.ExecuteReader(cs, CommandType.Text, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter()))
                    {
                        DataHelper.SetFetchSize(cs, dr, (0 < pTop ? pTop : 100));
                        mapDrList = DataReaderExtension.DataReaderMapToList(dr);
                    }
                }
                else
                {
                    dbTransaction = DataHelper.BeginTran(SessionTools.CS, IsolationLevel.ReadUncommitted);
                    using (IDataReader dr = DataHelper.ExecuteReader(dbTransaction, CommandType.Text, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter()))
                    {
                        DataHelper.SetFetchSize(cs, dr, (0 < pTop ? pTop : 100));
                        mapDrList = DataReaderExtension.DataReaderMapToList(dr);
                    }
                }

                // FI 20200820 [25468] DateTimeKind.Utc for columns DTINS and DTINS
                mapDrList.ForEach(item =>
                {
                    item.Column.Where(x => (x.Name == "DTINS" || x.Name == "DTUPD")).ToList().ForEach(
                    itemCol =>
                    {
                        if (itemCol.Value != Convert.DBNull)
                            itemCol.Value = DateTime.SpecifyKind(Convert.ToDateTime(itemCol.Value), DateTimeKind.Utc);
                    });
                });


                //PL 20200806 Reste à élucider... Sous SQLServer seulement, le second passage dans le code ci-dessous entraine l'ouverture d'une seconde connexion physqiue dans le Pool de connexion ADO.NET
                //                                Autre mystère, cela n'ouvre qu'une connexion supplémentaire et non pas 5  comme cela devrait être le cas avec la Default value de "Inc Pool Size" qui vaut théoriquement 5
                mapDrList.ForEach(item =>
                {
                    
                    WCTooltipLabel lbl = null;
                    WCToolTipPanel pnl = null;
                    TrackerDataReader tdr = new TrackerDataReader(item);
                    ul.Attributes.Add("class", "content");

                    li = new HtmlGenericControl("li");
                    li.Attributes.Add("class", tdr.GetCssForeColor());

                    lbl = GetLabelDate(tdr);
                    if (StrFunc.IsFilled(lbl.Pty.TooltipTitle))
                    {
                        lbl.Pty.TooltipTitle = string.Empty;
                        li.Attributes["class"] += " today";
                    }

                    pnl = new WCToolTipPanel();
                    pnl.Controls.Add(lbl);
                    li.Controls.Add(pnl);

                    pnl = new WCToolTipPanel();
                    pnl.Controls.Add(GetLinkMessage(tdr));
                    li.Controls.Add(pnl);

                    lbl = GetLinkIdData(tdr, resDetail);
                    if (StrFunc.IsFilled(lbl.Text))
                    {
                        pnl = new WCToolTipPanel();
                        pnl.Controls.Add(lbl);
                        li.Controls.Add(pnl);
                    }
                    ul.Controls.Add(li);
                });
                if (0 == ul.Controls.Count)
                {
                    ul.Attributes.Add("class", "content empty");
                    li = new HtmlGenericControl("li");
                    li.Controls.Add(new LiteralControl(Ressource.GetString("Msg_TradesNoneSelected")));
                    ul.Controls.Add(li);
                }
                this.Page.Controls.Add(ul);
            }
            catch (Exception)
            {
                ul.Attributes.Add("class", "content TrackerBlue");
                li = new HtmlGenericControl("li");
                li.Controls.Add(new LiteralControl("Error"));
                ul.Controls.Add(li);
                this.Page.Controls.Add(ul);
            }
            finally
            {
                if (null != dbTransaction)
                    DataHelper.RollbackTran(dbTransaction);
            }
        }
        /// <summary>
        /// Retourne la date d'une ligne du Tracker (DTUPD ou DTINS)
        /// </summary>
        /// FI 20200720 [XXXXX] Refactoring Convertion de la date UTC du tracker selon le fuseau horaire du profi utilisateur
        // EG 20200922 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc) Mise en gras des traitements du jour
        // EG 20210916 [XXXXX] Add IsTrackerToDay(dtsys), suite à traitement des dates UTC/TimeZone)
        // EG 20220503 [XXXXX] La date (DD/MM) est affichée à la place de l'heure sur une ligne en dehors de la journée en cours
        protected static WCTooltipLabel GetLabelDate(TrackerDataReader pTrackerDR)
        {
            DateTime dtsys = Convert.ToDateTime(pTrackerDR.mapDr[pTrackerDR.IsRowUpdated ? "DTUPD" : "DTINS"].Value);

            //Affichage text
            AuditTimestampInfo auditTimestampInfo = new AuditTimestampInfo()
            {
                TimestampZone = SessionTools.AuditTimestampZone,
                Collaborator = SessionTools.Collaborator,
                Precision = Cst.AuditTimestampPrecision.Minute,
                Type = "time",
                WithTzdbId = false
            };

            WCTooltipLabel lbl = new WCTooltipLabel()
            {
                Text = DtFuncExtended.DisplayTimestampUTC(dtsys, auditTimestampInfo)
            };

            // EG 20210916 [XXXXX] New
            if (IsTrackerToDay(dtsys))
            {
                lbl.Pty.TooltipTitle += "today";
            }
            else
            {
                auditTimestampInfo.Type = "date";
                auditTimestampInfo.Precision = Cst.AuditTimestampPrecision.DDMM;
                lbl.Text = DtFuncExtended.DisplayTimestampUTC(dtsys, auditTimestampInfo);
            }

            //Affichage Tooltip
            auditTimestampInfo.Type = "datetime";
            auditTimestampInfo.WithTzdbId = true;
            auditTimestampInfo.Precision = Cst.AuditTimestampPrecision.Minute;

            string tooltip = DtFuncExtended.DisplayTimestampUTC(dtsys, auditTimestampInfo);
            // FI 20200731 [XXXXX] Ajout de la date de demande
            if (ProcessStateTools.IsReadyStateTerminated(pTrackerDR.GetReadyState()))
            {
                DateTime dtsysStart = Convert.ToDateTime(pTrackerDR.mapDr["DTINS"].Value);
                tooltip += Cst.HTMLBreakLine + $"{Ressource.GetString("REQUESTED")}: {DtFuncExtended.DisplayTimestampUTC(dtsysStart, auditTimestampInfo)}";
            }
            lbl.Pty.TooltipContent = tooltip;

            return lbl;
        }

        /// <summary>
        /// La ligne de tracker est-elle une ligne du jour ?
        /// Compare la date de la ligne du Tracker et la date jour dans le même timezone (celui spécifié dans Spheres)
        /// Par ordre de priorité User, Département et Entité
        /// </summary>
        /// <param name="pDtItemTracker"></param>
        /// <returns></returns>
        /// EG 20210916 [XXXXX] New (suite à traitement des dates UTC/TimeZone)
        /// EG 20230606 [XXXXX] DateTime.UTCNow instead of DateTime.Now
        private static bool IsTrackerToDay(DateTime pDtItemTracker)
        {
            string tz = SessionTools.GetCriteriaTimeZone();
            DateTime dtToDay = new DateTimeTz(DateTime.UtcNow, "Etc/UTC").ConvertTimeToTz(tz).Date;
            DateTime dtItem = new DateTimeTz(pDtItemTracker, "Etc/UTC").ConvertTimeToTz(tz).Date;
            return (0 <= dtItem.Date.CompareTo(dtToDay.Date));
        }
        /// <summary>
        /// Construction Plage de dates pour message court dans Tracker
        /// </summary>
        //EG 20190904 New
        private string GetBetweenDatesMessage(string pDates)
        {
            string data1 = string.Empty;
            string[] tmp_Data1 = pDates.Replace("[", string.Empty).Replace("]", string.Empty).Split(',');
            foreach (string _data1 in tmp_Data1)
            {
                if (!String.IsNullOrEmpty(_data1) && (_data1.Length >= 10) && IntFunc.IsPositiveInteger(_data1.Substring(0, 4)))
                {
                    try { data1 += DtFunc.DateTimeToString(new DtFunc().StringToDateTime(_data1.Substring(0, 10), DtFunc.FmtISODate), DtFunc.FmtShortDate) + " - "; }
                    catch { data1 = string.Empty; }
                }
            }
            return data1.Remove(data1.Length - 3);
        }
        /// <summary>
        /// Retourne la date d'une ligne du Tracker (DTUPD ou DTINS)
        /// </summary>
        // EG 20170125 [Refactoring URL] Upd
        // PL 20190227 Newness - Complete Short Message
        // EG 20190904 Harmonisation plages de dates sur ShortMessage du Tracker
        // EG 20200918 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc) Corrections et Compléments
        // EG 20201014 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc) Gestion Image IRQ
        // EG 20240619 [WI945] Security : Update outdated components (New QTip2)
        protected Label GetLinkMessage(TrackerDataReader pTrackerDR)
        {
            Label lbl = new Label
            {
                CssClass = "message",
                Text = pTrackerDR.mapDr["SHORTMESSAGETRACKER"].Value.ToString()
            };
            if (ProcessStateTools.IsStatusInterrupt(pTrackerDR.GetStatus()))
                lbl.CssClass += " StatusInterrupt";

            #region Complete Short Message
            string data1, data2, data3; 
            string grpTracker = pTrackerDR.mapDr["GROUPTRACKER"].Value.ToString();
            string sysnumber = pTrackerDR.mapDr["SYSNUMBER"].Value.ToString();
            switch (grpTracker)
            {
                case "ACC":
                    if (sysnumber == "3000")
                    {
                        data1 = GetBetweenDatesMessage(pTrackerDR.mapDr["DATA1"].Value.ToString());
                        lbl.Text += Cst.HTMLSpace + data1;
                    }
                    break;
                case "IO":
                    data1 = pTrackerDR.mapDr["DATA1"].Value.ToString();
                    if (!String.IsNullOrEmpty(data1))
                        lbl.Text += Cst.HTMLSpace + data1;
                    break;
                case "MSG":
                    // 2020:Generation of reports ... - 2021:<b>Regeneration and/or send of reports ...
                    if (sysnumber == "2020" || sysnumber == "2021")
                    {
                        data1 = GetBetweenDatesMessage(pTrackerDR.mapDr["DATA1"].Value.ToString());

                        data3 = pTrackerDR.mapDr["DATA3"].Value.ToString();
                        if (data3.Length > 16)
                        {
                            if (data3.IndexOf(' ') <= 16)
                                data3 = data3.Substring(0, data3.IndexOf(' '));
                            else
                                data3 = data3.Substring(0, 16 - 3) + "...";
                        }

                        lbl.Text += Cst.HTMLSpace + data3 + Cst.HTMLSpace + data1;
                    }
                    break;
                case "CLO":
                    // 4100/4105:EOD - 4110/4115:Closing Day - 4060:Cash-Balance
                    if (sysnumber == "4100" || sysnumber == "4105" || sysnumber == "4110" || sysnumber == "4115" || sysnumber == "4060")
                    {
                        data2 = pTrackerDR.mapDr["DATA2"].Value.ToString();
                        data3 = pTrackerDR.mapDr["DATA3"].Value.ToString();
                        if (!String.IsNullOrEmpty(data2) && (data2.Length >= 10) && IntFunc.IsPositiveInteger(data2.Substring(0, 4)))
                        {
                            try { data2 = DtFunc.DateTimeToString(new DtFunc().StringToDateTime(data2.Substring(0, 10), DtFunc.FmtISODate), DtFunc.FmtShortDate); }
                            catch { }
                        }
                        if (!String.IsNullOrEmpty(data3) && (data3.Length >= 10) && IntFunc.IsPositiveInteger(data3.Substring(0, 4)))
                        {
                            try { data3 = DtFunc.DateTimeToString(new DtFunc().StringToDateTime(data3.Substring(0, 10), DtFunc.FmtISODate), DtFunc.FmtShortDate); }
                            catch { }
                        }

                        lbl.Text += Cst.HTMLSpace + data2 + Cst.HTMLSpace + data3;
                    }
                    else if (sysnumber == "4030")
                    {
                        data1 = GetBetweenDatesMessage(pTrackerDR.mapDr["DATA1"].Value.ToString());
                        lbl.Text += Cst.HTMLSpace + data1;
                    }
                    break;
                case "EXT":
                    sysnumber = pTrackerDR.mapDr["SYSNUMBER"].Value.ToString();
                    if (sysnumber == "1011" || sysnumber == "2027" || sysnumber == "4062" || sysnumber == "4106" || sysnumber == "4116")
                    {
                        data1 = pTrackerDR.mapDr["DATA1"].Value.ToString();
                        data2 = pTrackerDR.mapDr["DATA2"].Value.ToString();
                        if ((!String.IsNullOrEmpty(data2)) && (data2.Length >= 10) && IntFunc.IsPositiveInteger(data2.Substring(0, 4)))
                        {
                            try { data2 = DtFunc.DateTimeToString(new DtFunc().StringToDateTime(data2.Substring(0, 10), DtFunc.FmtISODate), DtFunc.FmtShortDate); }
                            catch { }
                        }

                        lbl.Text += Cst.HTMLSpace + data1 + Cst.HTMLSpace + data2;
                    }
                    else if (IsIOInGroupEXT(sysnumber))
                    {
                        data1 = pTrackerDR.mapDr["DATA1"].Value.ToString();
                        if (!String.IsNullOrEmpty(data1))
                            lbl.Text += Cst.HTMLSpace + data1;
                    }
                    break;
            }
            #endregion Complete Short Message

            string tooltip = JavaScript.HTMLStringCrLf(pTrackerDR.mapDr["MESSAGETRACKER"].Value.ToString());
            if (lbl.Text != tooltip)
                lbl.Attributes.Add("qtip-alt", tooltip);


            string idData = pTrackerDR.mapDr["IDTRK_L"].Value.ToString();
            string args = SpheresURL.GetURL(IdMenu.Menu.TRACKER_L, idData).Replace("hyperlink.aspx?args=", string.Empty);
            
            lbl.Attributes.Add("args", args + "|" + idData);
            lbl.Attributes.Add("target", "_blank");
            lbl.Attributes.Add("onclick", "HL(this)");
            return lbl;
        }

        private static Boolean IsIOInGroupEXT(string sysNumber)
        {
            return StrFunc.IsFilled(sysNumber) && ((sysNumber == "1001" || sysNumber == "1002"));
        }

        /// <summary>
        /// 
        /// </summary>
        // EG 20170125 [Refactoring URL] Upd
        // EG 20190212 Tracker ShortMessage
        protected WCTooltipLabel GetLinkIdData(TrackerDataReader pTrackerDR, string pResDetail)
        {
            WCTooltipLabel lbl = new WCTooltipLabel
            {
                CssClass = "iddata"
            };

            Cst.GroupTrackerEnum group = (Cst.GroupTrackerEnum)Enum.Parse(typeof(Cst.GroupTrackerEnum), pTrackerDR.mapDr["GROUPTRACKER"].Value.ToString(), true);
            
            string idData = string.Empty;
            if (false == Convert.IsDBNull(pTrackerDR.mapDr["IDDATA"].Value))
            {
                idData = pTrackerDR.mapDr["IDDATA"].Value.ToString();
                lbl.Text = pResDetail;
            }


            string sysnumber = pTrackerDR.mapDr["SYSNUMBER"].Value.ToString();
            if (!(Cst.GroupTrackerEnum.IO == group) && !(Cst.GroupTrackerEnum.EXT == group && IsIOInGroupEXT(sysnumber)))
            {
                if ((false == Convert.IsDBNull(pTrackerDR.mapDr["IDDATAIDENTIFIER"].Value)))
                    lbl.Text = StrFunc.ReplaceNoBreakSpaceByWhiteSpace(pTrackerDR.mapDr["IDDATAIDENTIFIER"].Value.ToString());
            }

            if (StrFunc.IsFilled(idData))
            {
                string idDataIdent = pTrackerDR.mapDr["IDDATAIDENT"].Value.ToString();
                string data1 = Convert.IsDBNull(pTrackerDR.mapDr["DATA1"].Value) ? string.Empty : pTrackerDR.mapDr["DATA1"].Value.ToString();
                // EG 20151102 [21465] IDTRK_L est passé à la place de IDPR
                // pour palier au cas où le tracker regroupe plusieurs IDPR
                if ("POSREQUEST" == idDataIdent)
                    lbl.Attributes.Add("args", GetArgs(pTrackerDR, pTrackerDR.mapDr["IDTRK_L"].Value.ToString()) + "|" + idData + "|" + data1);
                else
                    lbl.Attributes.Add("args", GetArgs(pTrackerDR, idData) + "|" + idData + "|" + data1);
                lbl.Attributes.Add("target", "_blank");
                lbl.Attributes.Add("onclick", "HL(this)");
                if (pResDetail != lbl.Text)
                    lbl.Pty.TooltipContent = lbl.Text;
            }

            return lbl;
        }

        // EG 20170125 [Refactoring URL] New
        // FI 20170313 [22225] Modify
        private string GetArgs(TrackerDataReader pTrackerDR, string pIdData)
        {
            string args = string.Empty;
            string idDataIdent = pTrackerDR.mapDr["IDDATAIDENT"].Value.ToString();
            Cst.OTCml_TBL tableName = Cst.OTCml_TBL.PROCESS_L;
            if (Enum.IsDefined(typeof(Cst.OTCml_TBL), idDataIdent))
                tableName = (Cst.OTCml_TBL)System.Enum.Parse(typeof(Cst.OTCml_TBL), idDataIdent, true);


            IdMenu.Menu? idMenu;
            switch (tableName)
            {
                case Cst.OTCml_TBL.PROCESS_L:
                    idMenu = IdMenu.Menu.PROCESS_L;
                    break;
                case Cst.OTCml_TBL.TRADE:
                    idMenu = IdMenu.Menu.InputTrade;
                    break;
                case Cst.OTCml_TBL.TRADEADMIN:
                    idMenu = IdMenu.Menu.InputTradeAdmin;
                    break;
                case Cst.OTCml_TBL.TRADEDEBTSEC:
                case Cst.OTCml_TBL.DEBTSECURITY:
                    idMenu = IdMenu.Menu.InputDebtSec;
                    break;
                case Cst.OTCml_TBL.TRADERISK:
                    idMenu = IdMenu.Menu.InputTradeRisk;
                    if (false == Convert.IsDBNull(pTrackerDR.mapDr["FAMILY"].Value))
                    {
                        ProductTools.FamilyEnum family = (ProductTools.FamilyEnum)
                            ReflectionTools.EnumParse(new ProductTools.FamilyEnum(), pTrackerDR.mapDr["FAMILY"].Value.ToString());
                        switch (family)
                        {
                            case ProductTools.FamilyEnum.CashBalance:
                                idMenu = IdMenu.Menu.InputTradeRisk_CashBalance;
                                break;
                            case ProductTools.FamilyEnum.CashInterest:
                                idMenu = IdMenu.Menu.InputTradeRisk_CashInterest;
                                break;
                            case ProductTools.FamilyEnum.CashPayment:
                                idMenu = IdMenu.Menu.InputTradeRisk_CashPayment;
                                break;
                            case ProductTools.FamilyEnum.Margin:
                                idMenu = IdMenu.Menu.InputTradeRisk_InitialMargin;
                                break;
                        }
                    }
                    break;
                case Cst.OTCml_TBL.EVENT:
                    idMenu = IdMenu.Menu.InputEvent;
                    break;
                case Cst.OTCml_TBL.CORPOACTIONISSUE:
                    idMenu = IdMenu.Menu.InputCorporateActionIssue;
                    break;
                case Cst.OTCml_TBL.CORPOACTION:
                    idMenu = IdMenu.Menu.InputCorporateActionEmbedded;
                    break;
                case Cst.OTCml_TBL.IOTRACK:
                    idMenu = IdMenu.Menu.IOTRACK;
                    break;
                case Cst.OTCml_TBL.POSREQUEST:
                    idMenu = IdMenu.Menu.TrackerPosRequest;
                    break;
                case Cst.OTCml_TBL.SERVICE_L:
                    idMenu = IdMenu.Menu.SERVICE_L;
                    break;
                case Cst.OTCml_TBL.TRACKER_L:
                    idMenu = IdMenu.Menu.TRACKER_L;
                    break;
                case Cst.OTCml_TBL.QUOTE_BOND_H:
                    idMenu = IdMenu.Menu.QUOTE_BOND_H;
                    break;
                case Cst.OTCml_TBL.QUOTE_DEBTSEC_H:
                    idMenu = IdMenu.Menu.QUOTE_DEBTSEC_H;
                    break;
                case Cst.OTCml_TBL.QUOTE_EQUITY_H:
                    idMenu = IdMenu.Menu.QUOTE_EQUITY_H;
                    break;
                case Cst.OTCml_TBL.QUOTE_ETD_H:
                    idMenu = IdMenu.Menu.QUOTE_ETD_H;
                    break;
                case Cst.OTCml_TBL.QUOTE_INDEX_H:
                    idMenu = IdMenu.Menu.QUOTE_INDEX_H;
                    break;
                case Cst.OTCml_TBL.QUOTE_FXRATE_H:
                    idMenu = IdMenu.Menu.QUOTE_FXRATE_H;
                    break;
                case Cst.OTCml_TBL.QUOTE_RATEINDEX_H:
                    idMenu = IdMenu.Menu.QUOTE_RATEINDEX_H;
                    break;
                case Cst.OTCml_TBL.QUOTE_EXTRDFUND_H:
                    idMenu = IdMenu.Menu.QUOTE_EXTRDFUND_H;
                    break;
                case Cst.OTCml_TBL.ASSET_ETD: // FI 20170313 [22225] add
                    idMenu = IdMenu.Menu.AssetETD; 
                    break;
                default:
                    string column = OTCmlHelper.GetColunmID(idDataIdent);
                    idMenu = SpheresURL.GetMenu_Repository(column, pIdData);
                    break;
            }
            if (idMenu.HasValue)
            {
                args = SpheresURL.GetURL(idMenu, pIdData).Replace("hyperlink.aspx?args=", string.Empty);
            }
            return args;
        }
        #endregion Methods

    }
    #endregion TrackerContent
}
