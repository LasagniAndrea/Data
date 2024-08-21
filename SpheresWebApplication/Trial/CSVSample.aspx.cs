using CsvHelper;
using CsvHelper.TypeConversion;
using EFS.ACommon;
using EFS.ApplicationBlocks.Data;
using EFS.ApplicationBlocks.Data.Extension;
using EFS.Common;
using EFS.Common.Web;
using EFS.CommonCSV;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using CsvConfig = CsvHelper.Configuration;

namespace EFS.Spheres.Trial
{
    public partial class CSVSample : PageBase
    {
        protected string leftTitle, rightTitle;

        protected override void OnInit(EventArgs e)
        {
            InitializeComponent();
            base.OnInit(e);
        }

        /// <summary>
        /// Méthode requise pour la prise en charge du concepteur - ne modifiez pas
        /// le contenu de cette méthode avec l'éditeur de code.
        /// </summary>
        private void InitializeComponent()
        {
        }

        // EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        protected void Page_Load(object sender, System.EventArgs e)
        {
            leftTitle = "CSV Helper";
            rightTitle = "Tests";
            PageTools.SetHead(this, "CSW Tester", null, null);

            Panel pnlCulture = (Panel)this.FindControl("pnlCulture");
            pnlCulture.Visible = false;
            Panel pnlResults = (Panel)this.FindControl("pnlResults");

            HtmlContainerControl ctrl = (HtmlContainerControl)this.FindControl("lblCultureTitle");
            if (null != ctrl)
                ctrl.InnerText = Ressource.GetString("lblCulture");
            ctrl = (HtmlContainerControl)this.FindControl("lblResults");
            if (null != ctrl)
                ctrl.InnerText = Ressource.GetString("lblResultsType");

            lblPatternDto.Text = "Format (DateTimeOffset)";
            lblPatternDt.Text = "Format (DateTime)";
            lblPatternDec.Text = "Format (Decimal)";
            chkHeader.Text = "Header";
            lblDelimiter.Text = "Delimiter";

            if (!IsPostBack)
            {
                ControlsTools.DDLLoad_Culture(ddlCulture, true);
                ControlsTools.DDLSelectByValue(ddlCulture, Thread.CurrentThread.CurrentUICulture.Name);
                ddlQuery.Items.Add(new ListItem("Tracker"));
                ddlQuery.Items.Add(new ListItem("Trade"));
                ddlQuery.Items.Add(new ListItem("Event"));

                CsvTools.GetCsvPatternEnum<CsvPattern>("datetime").ToList().ForEach(item => ddlPatternDt.Items.Add(new ListItem(item.ToString())));
                CsvTools.GetCsvPatternEnum<CsvPattern>("datetimeoffset").ToList().ForEach(item => ddlPatternDto.Items.Add(new ListItem(item.ToString())));
                CsvTools.GetCsvPatternEnum<CsvPattern>("decimal").ToList().ForEach(item => ddlPatternDec.Items.Add(new ListItem(item.ToString())));

                ControlsTools.DDLSelectByValue(ddlPatternDto, CsvPattern.isoDatetimeOffset.ToString());
                ControlsTools.DDLSelectByValue(ddlPatternDt, CsvPattern.isoDatetime.ToString());
                ControlsTools.DDLSelectByValue(ddlPatternDec, CsvPattern.inherit.ToString());
            }
            else
            {
                // EG 20130725 Timeout sur Block
                JQuery.Block block = new JQuery.Block("CSV", "Msg_WaitingRefresh", true)
                {
                    Timeout = SystemSettings.GetTimeoutJQueryBlock("BM")
                };
                JQuery.UI.WriteInitialisationScripts(this, block);
            }

            SetLblFormat(lblFormatDto, ddlPatternDto.SelectedValue);
            SetLblFormat(lblFormatDt, ddlPatternDt.SelectedValue);
            SetLblFormat(lblFormatDec, ddlPatternDec.SelectedValue);


            #region Header
            HtmlPageTitle titleLeft = new HtmlPageTitle(leftTitle);
            Panel pnlHeader = new Panel
            {
                ID = "divHeader"
            };
            pnlHeader.Controls.Add(ControlsTools.GetBannerPage(this, titleLeft, null, IdMenu.GetIdMenu(IdMenu.Menu.Admin)));
            plhHeader.Controls.Add(pnlHeader);
            #endregion Header
        }

        #region OnWriteClick
        // EG 20210308 [XXXXX] Mise à jour package CsvHelper 12.3.2 vers 13.0.0
        // EG 20210308 [XXXXX] CsvConfiguration devient OverrideCsvConfiguration
        protected void OnWriteClick(object sender, System.EventArgs e)
        {
            byte[] result = null;
            OverrideCsvConfiguration csvConfiguration = GetCsvConfiguration();
            switch (ddlQuery.SelectedValue)
            {
                case "Event":
                    result = CsvTools.ExportToCSV(GetQueryEvent(), csvConfiguration);
                    break;
                case "Tracker":
                    result = CsvTools.ExportToCSV(GetQueryTracker(), csvConfiguration);
                    break;
                case "Trade":
                    result = CsvTools.ExportToCSV(GetQueryTrade(), csvConfiguration);
                    break;
            }
            if (ArrFunc.IsFilled(result))
                ExportCsvResponse(result, ddlQuery.SelectedValue, string.Empty);
        }
        #endregion OnWriteClick
        #region OnConfigClick
        // EG 20210308 [XXXXX] Mise à jour package CsvHelper 12.3.2 vers 13.0.0
        // EG 20210308 [XXXXX] CsvConfiguration devient OverrideCsvConfiguration
        // EG 20210309 [XXXXX] Mise à jour package CsvHelper 19.0.0 vers 20.0.0
        protected void OnConfigClick(object sender, System.EventArgs e)
        {
            OverrideCsvConfiguration csvConfiguration = GetCsvConfiguration();
            try
            {
                Pair<CsvConfig.IWriterConfiguration, CsvContext> wc = GetWriterConfiguration();
                txtResults.Text  = "AllowComments            :" + wc.First.AllowComments.ToString() + Cst.CrLf;
                txtResults.Text += "Comments                 :" + wc.First.Comment.ToString() + Cst.CrLf;
                txtResults.Text += "CultureInfo              :" + wc.First.CultureInfo.DisplayName + Cst.CrLf;
                txtResults.Text += "Delimiter                :" + wc.First.Delimiter + Cst.CrLf;
                txtResults.Text += "DoubleQuoteString        :" + new String(wc.First.Quote, 2) + Cst.CrLf;
                txtResults.Text += "Escape                   :" + wc.First.Escape.ToString() + Cst.CrLf;
                txtResults.Text += "HasHeaderRecord          :" + wc.First.HasHeaderRecord.ToString() + Cst.CrLf;
                txtResults.Text += "InjectionCharacters      :";
                foreach (char item in wc.First.InjectionCharacters) { txtResults.Text += item.ToString(); }
                txtResults.Text += Cst.CrLf;
                txtResults.Text += "InjectionEscapeCharacter :" + wc.First.InjectionEscapeCharacter.ToString() + Cst.CrLf;
                txtResults.Text += "Quote                    :" + wc.First.Quote.ToString() + Cst.CrLf;
                txtResults.Text += "QuoteString              :" + wc.First.Quote.ToString() + Cst.CrLf;
                txtResults.Text += "SanitizeForInjection     :" + wc.First.SanitizeForInjection.ToString() + Cst.CrLf;
                txtResults.Text += "TrimOptions              :" + wc.First.TrimOptions.ToString() + Cst.CrLf;
                txtResults.Text += "Timestamp format      :" + csvConfiguration.patternTms + Cst.CrLf;

                txtConverterOptions.Text = String.Empty;
                TypeConverterOptions tco = wc.Second.TypeConverterOptionsCache.GetOptions<DateTimeOffset>();
                DisplayConverterOptions(tco, "DateTimeOffset");
                tco = wc.Second.TypeConverterOptionsCache.GetOptions<DateTime>();
                DisplayConverterOptions(tco, "DateTime");
                tco = wc.Second.TypeConverterOptionsCache.GetOptions<Decimal>();
                DisplayConverterOptions(tco, "Decimal");
            }
            catch (IOException ex)
            {
                this.MsgForAlertImmediate = ex.Message;
            }
        }
        // EG 20210308 [XXXXX] Mise à jour package CsvHelper 12.3.2 vers 13.0.0
        // EG 20210308 [XXXXX] CsvConfiguration devient OverrideCsvConfiguration
        private OverrideCsvConfiguration GetCsvConfiguration()
        {
            return new OverrideCsvConfiguration(GetCsvConfigSetting(ddlQuery.SelectedValue));
        }

        // EG 20210308 [XXXXX] Mise à jour package CsvHelper 12.3.2 vers 13.0.0
        // EG 20210308 [XXXXX] CsvConfiguration devient OverrideCsvConfiguration
        // EG 20210309 [XXXXX] Mise à jour package CsvHelper 19.0.0 vers 20.0.0
        // EG 20210309 [XXXXX] CsvConfig.CsvConfiguration devient un record (ajout member configuration)
        private Pair<CsvConfig.IWriterConfiguration, CsvContext> GetWriterConfiguration()
        {
            OverrideCsvConfiguration csvConfiguration = GetCsvConfiguration();
            Pair<CsvConfig.IWriterConfiguration, CsvContext> ret = null;
            using (var memoryStream = new MemoryStream())
            using (var streamWriter = new StreamWriter(memoryStream))
            {
                CsvWriter wc = new CsvWriter(streamWriter, csvConfiguration.configuration);
                ret = new Pair<CsvConfig.IWriterConfiguration, CsvContext>(wc.Configuration, wc.Context);
            }
            return ret;
        }

        private void DisplayConverterOptions(TypeConverterOptions pTco, string pCast)
        {
            if (null != pTco)
            {
                txtConverterOptions.Text += "Type          :" + pCast + Cst.CrLf;
                if (pTco.DateTimeStyle.HasValue)
                    txtConverterOptions.Text += "DateTimeStyle :" + pTco.DateTimeStyle.Value.ToString() + Cst.CrLf;
                if (ArrFunc.IsFilled(pTco.Formats))
                {
                    txtConverterOptions.Text += "Format        :";
                    pTco.Formats.ToList().ForEach(format => {txtConverterOptions.Text += format + " ,";});
                    txtConverterOptions.Text += Cst.CrLf;
                }
            }
        }
        #endregion OnConfigClick

        #region GetQueryEvent
        private List<MapDataReaderRow> GetQueryEvent()
        {
            List<MapDataReaderRow> ret = null;
            DataParameters parameters = new DataParameters();
            parameters.Add(new DataParameter(SessionTools.CS, "ASSETCATEGORY", DbType.AnsiString, SQLCst.UT_ENUM_MANDATORY_LEN), Cst.UnderlyingAsset.ExchangeTradedContract.ToString());

            string sqlQuery = @"select ev.IDT, tr.IDENTIFIER, tr.DTBUSINESS, tr.DTEXECUTION, tr.TZFACILITY, ev.EVENTCODE, ev.EVENTTYPE, ec.EVENTCLASS, ec.DTEVENT, ev.VALORISATION, ev.UNIT, ec.ISPAYMENT,
			pay.IDENTIFIER as ACTOR_PAYER, bpay.IDENTIFIER as BOOK_PAYER,
			rec.IDENTIFIER as ACTOR_RECEIVER, brec.IDENTIFIER as BOOK_RECEIVER
            from dbo.EVENT ev
            inner join dbo.EVENTCLASS ec on (ec.IDE = ev.IDE)
			inner join dbo.TRADE tr on (tr.IDT = ev.IDT)
			inner join dbo.ENTITYMARKET em on (em.IDA = tr.IDA_ENTITY) and (em.IDM = tr.IDM)
            inner join dbo.ACTOR pay on (pay.IDA = ev.IDA_PAY)
            inner join dbo.ACTOR rec on (rec.IDA = ev.IDA_REC)
            inner join dbo.BOOK bpay on (bpay.IDB = ev.IDB_PAY)
            inner join dbo.BOOK brec on (brec.IDB = ev.IDB_REC)
			where (ec.EVENTCLASS = 'VAL') and (tr.DTBUSINESS = em.DTENTITY)
            order by ev.IDT, ev.IDE" + Cst.CrLf;
            QueryParameters qryParameters = new QueryParameters(SessionTools.CS, sqlQuery, parameters);
            using (IDataReader dr = DataHelper.ExecuteReader(SessionTools.CS, CommandType.Text, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter()))
            {
                ret = DataReaderExtension.DataReaderMapToList(dr, DataReaderExtension.DataReaderMapColumnType(dr));
            }
            return ret;
        }
        #endregion GetQueryTradeInstrument
        #region GetQueryTrade
        private List<MapDataReaderRow> GetQueryTrade()
        {
            List<MapDataReaderRow> ret = null;
            DataParameters parameters = new DataParameters();
            parameters.Add(new DataParameter(SessionTools.CS, "ASSETCATEGORY", DbType.AnsiString, SQLCst.UT_ENUM_MANDATORY_LEN), Cst.UnderlyingAsset.ExchangeTradedContract.ToString());

            string sqlQuery = @"select tr.DTBUSINESS, tr.IDT, tr.IDENTIFIER, tr.DTORDERENTERED, tr.DTEXECUTION, tr.TZFACILITY, 
            pr.IDENTIFIER as PRODUCT, ns.IDENTIFIER as INSTRUMENT, tr.IDM, mk.SHORT_ACRONYM,
            tr.IDASSET, ast.CONTRACTDESCRIPTION as CONTRACT, ast.CONTRACTSYMBOL, ast.IDENTIFIER as ASSET,
            tr.SIDE, tr.QTY, tr.PRICE, tr.STRIKEPRICE, tr.TRDTYPE, 
            deal.IDENTIFIER as ACTOR_DEALER, bdeal.IDENTIFIER as BOOK_DEALER,
            clear.IDENTIFIER as ACTOR_CLEARER, bclear.IDENTIFIER as BOOK_CLEARER, tr.DTOUT, tr.DTSYS 
            from dbo.TRADE tr
            inner join dbo.INSTRUMENT ns on (ns.IDI = tr.IDI)
            inner join dbo.PRODUCT pr on (pr.IDP = ns.IDP)
            inner join dbo.VW_MARKET_IDENTIFIER mk on (mk.IDM = tr.IDM)
            inner join dbo.VW_ASSET_ETD_EXPANDED ast on (ast.IDASSET = tr.IDASSET)
            inner join dbo.ACTOR deal on (deal.IDA = tr.IDA_DEALER)
            inner join dbo.ACTOR clear on (clear.IDA = tr.IDA_CLEARER)
            inner join dbo.BOOK bdeal on (bdeal.IDB = tr.IDB_DEALER)
            inner join dbo.BOOK bclear on (bclear.IDB = tr.IDB_CLEARER)
            where (tr.ASSETCATEGORY = @ASSETCATEGORY)
            order by tr.IDT" + Cst.CrLf;
            QueryParameters qryParameters = new QueryParameters(SessionTools.CS, sqlQuery, parameters);
            using (IDataReader dr = DataHelper.ExecuteReader(SessionTools.CS, CommandType.Text, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter()))
            {
                ret = DataReaderExtension.DataReaderMapToList(dr, DataReaderExtension.DataReaderMapColumnType(dr));
            }
            return ret;
        }
        #endregion GetQueryTrade
        #region GetQueryTracker
        private DataTable GetQueryTracker()
        {
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
            from dbo.TRACKER_L tk" + Cst.CrLf;

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
            sqlSelect = String.Format(sqlSelect, sqlRowNumber);

            string messageTracker = @"replace(replace(replace(replace(replace(isnull(smd.MESSAGE,smd_gb.MESSAGE),
            '{1}',isnull(tk.DATA1,'{1}')),
            '{2}',isnull(tk.DATA2,'{2}')),
            '{3}',isnull(tk.DATA3,'{3}')),
            '{4}',isnull(tk.DATA4,'{4}')),
            '{5}',isnull(tk.DATA5,'{5}'))";

            string sqlQuery = String.Format(@"select tk.IDTRK_L, tk.SYSCODE, tk.SYSNUMBER, ti.IDT, pr.FAMILY, pr.GPRODUCT, 
                tk.GROUPTRACKER, tk.READYSTATE, tk.STATUSTRACKER, 
                tk.IDDATA, tk.IDDATAIDENT, tk.IDDATAIDENTIFIER, tk.DATA1, tk.DATA1IDENT, 
                tk.DTINS, tk.DTUPD, tk.IRQ, tk.STATUSORDER, 
                case when smd.SYSCODE is null and smd_gb.SYSCODE is null then 'Request' 

                        else isnull(smd.SHORTMESSAGE, smd_gb.SHORTMESSAGE) +

                            case when tk.GROUPTRACKER = 'IO'  then ' ' + isnull(tk.DATA1,'') else
                            case when tk.GROUPTRACKER = 'MSG' and tk.SYSNUMBER in (2020, 2021) then ' ' + isnull(tk.DATA3,'') + ' ' + isnull(tk.DATA1,'') else
							case when tk.GROUPTRACKER = 'CLO' and tk.SYSNUMBER in (4100,4105,4110,4115,4060) then ' ' + isnull(tk.DATA2,'') + ' ' + isnull(tk.DATA3,'') else 
                            case when tk.GROUPTRACKER = 'EXT' and tk.SYSNUMBER in (1011,2027,4062,4106,4116) then ' ' + isnull(tk.DATA1,'') + ' ' + isnull(tk.DATA2,'') else '' end end end
                        end end as SHORTMESSAGETRACKER, 

                case when smd.SYSCODE is null and smd_gb.SYSCODE is null then 'No Message' 
                else {0} end as MESSAGETRACKER 
                from ({1}) tk
                left outer join dbo.SYSTEMMSG sm on (sm.SYSCODE = tk.SYSCODE) and (sm.SYSNUMBER = tk.SYSNUMBER)
                left outer join dbo.SYSTEMMSGDET smd on (smd.SYSCODE = sm.SYSCODE) and (smd.SYSNUMBER = sm.SYSNUMBER) and (smd.CULTURE = '{2}')
                left outer join dbo.SYSTEMMSGDET smd_gb on (smd_gb.SYSCODE = sm.SYSCODE) and (smd_gb.SYSNUMBER = sm.SYSNUMBER) and (smd_gb.CULTURE = 'en')
                left outer join dbo.TRADEINSTRUMENT ti on (ti.IDT = tk.IDDATA) and (ti.INSTRUMENTNO = 1) and (substring(tk.IDDATAIDENT,1,5) = 'TRADE')
                left outer join dbo.INSTRUMENT ns on (ns.IDI = ti.IDI)
                left outer join dbo.PRODUCT pr on (pr.IDP = ns.IDP)",
                messageTracker, Cst.CrLf + sqlSelect, SessionTools.Collaborator_Culture_ISOCHAR2);

            return DataHelper.ExecuteDataTable(SessionTools.CS, sqlQuery + Cst.CrLf + sqlWhere + Cst.CrLf + sqlOrder, null);
        }
        #endregion GetQueryTracker

        protected void OnPatternChanged(object sender, EventArgs e)
        {
            DropDownList ddl = sender as DropDownList;
            if (Page.FindControl(ddl.ID.Replace("ddlPattern", "lblFormat")) is Label lbl)
                SetLblFormat(lbl, ddl.SelectedValue);
        }

        private void SetLblFormat(Label pLabel, string pPattern)
        {
            CultureInfo culture = CultureInfo.CreateSpecificCulture(ddlCulture.SelectedValue);
            CsvPattern pattern = ReflectionTools.ConvertStringToEnum<CsvPattern>(pPattern);
            string format = CsvTools.GetFinalPatternFormat(culture, pattern, "$1.ffffff");
            pLabel.Text = format;
        }
    }
}