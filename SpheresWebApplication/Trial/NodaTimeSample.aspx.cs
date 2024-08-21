using EFS.Common.Web;
using NodaTime;
using NodaTime.TimeZones;
using NodaTime.TimeZones.Cldr;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Tz = EFS.TimeZone;

namespace Trial
{
    public partial class NodaTimeSample : PageBase
    {
        public class DtResult
        {
            public string WindowsID { get; set; }
            public string Territory { get; set; }
            public string TzdbID { get; set; }
            public string PatternCulture { get; set; }
            public string UtcDateTime { get; set; }
            public string OffsetDateTime { get; set; }
            public string ZonedDateTime { get; set; }

            public DtResult() { }
        }

        #region Members
        private WCZonedDateTime timestamp;
        private DropDownList ddlWindowsId;
        private DropDownList ddlTerritory;
        private ListBox lstTzdb;
        private GridView gvResult;
        private CheckBoxList chkCulture;
        #endregion Members
        // EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        // EG 20200930 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc) Correction et suppression de codes inutiles
        protected override void OnInit(EventArgs e)
        {
            AbortRessource = true;
            base.OnInit(e);

            GenerateHtmlForm();
            PageTools.BuildPage(this, Form, "Noda time tester", string.Empty, false, string.Empty);

            Panel divNodatime = new Panel
            {
                Height = Unit.Pixel(570)
            };
            divNodatime.Style.Add(HtmlTextWriterStyle.Margin, "50px");
            divNodatime.Style.Add(HtmlTextWriterStyle.BorderStyle, "solid");
            divNodatime.Style.Add(HtmlTextWriterStyle.BorderWidth, "1pt");
            divNodatime.Style.Add(HtmlTextWriterStyle.BorderColor, "#036AB5");
            divNodatime.Style.Add(HtmlTextWriterStyle.Display, "block");

            // TITRE
            Panel divHeader = new Panel
            {
                Height = Unit.Percentage(10)
            };
            divHeader.Style.Add(HtmlTextWriterStyle.BackgroundColor, "#036AB5");
            divHeader.Style.Add("border-top-left-radius", "7px");
            divHeader.Style.Add("border-top-right-radius", "7px");
            divHeader.Style.Add(HtmlTextWriterStyle.Display, "flex");

            Label lblHeader = new Label
            {
                Text = "Noda time samples",
                ForeColor = System.Drawing.Color.White
            };
            lblHeader.Font.Size = FontUnit.XLarge;
            lblHeader.Style.Add(HtmlTextWriterStyle.Padding, "3px 10px");
            divHeader.Controls.Add(lblHeader);

            // CRITERIA
            Panel divBody = new Panel
            {
                ID = "divbody",
                Height = Unit.Percentage(90)
            };
            divBody.Style.Add(HtmlTextWriterStyle.Display, "flex");

            // CRITERIA
            Panel divCriteria = new Panel
            {
                ID = "divCriteria",
                Width = Unit.Percentage(20),
                Height = Unit.Percentage(100)
            };
            //divCriteria.Style.Add(HtmlTextWriterStyle.Padding, "0px 10px");
            divCriteria.Style.Add(HtmlTextWriterStyle.BackgroundColor, "#e3ebf1");
            divCriteria.Style.Add("border-bottom-left-radius", "7px");
            divCriteria.Style.Add("border-right", "1pt solid #036AB5");

            HtmlGenericControl criteriaTitle = new HtmlGenericControl("h2")
            {
                InnerText = "Critères"
            };
            divCriteria.Controls.Add(criteriaTitle);

            Panel divDateInput = new Panel
            {
                Width = Unit.Percentage(100)
            };
            // Date
            timestamp = new WCZonedDateTime("tmsInput", "Date", true, "datetimeoffset", true, false, false, false, true, EFSCssClass.Capture, null);
            divDateInput.Controls.Add(timestamp);

            // Liste des MapZones
            Panel divMapZone = new Panel
            {
                ID = "divMapZone",
                Width = Unit.Percentage(100)
            };
            divMapZone.Style.Add(HtmlTextWriterStyle.Padding, "10px 0px");

            Label lblWindowsId = new Label
            {
                Width = Unit.Percentage(100),
                Text = "Windows Zones"
            };

            ddlWindowsId = new DropDownList
            {
                ID = "ddlWindowsId",
                Width = Unit.Percentage(100),
                AutoPostBack = true,
                CssClass = EFSCssClass.DropDownListCapture
            };
            ddlWindowsId.SelectedIndexChanged += new EventHandler(OnSelectedIndexChanged);

            Label lblTerritory = new Label
            {
                Width = Unit.Percentage(100),
                Text = "Territories"
            };

            ddlTerritory = new DropDownList
            {
                ID = "ddlTerritory",
                Width = Unit.Percentage(100),
                AutoPostBack = true,
                CssClass = EFSCssClass.DropDownListCapture
            };
            ddlTerritory.SelectedIndexChanged += new EventHandler(OnSelectedIndexChanged);

            Label lblTzdb = new Label
            {
                Text = "TzDb (IANA)",
                Width = Unit.Percentage(100)
            };

            lstTzdb = new ListBox
            {
                Rows = 10,
                ID = "lstTzDb",
                Width = Unit.Percentage(100),
                AutoPostBack = false,
                SelectionMode = ListSelectionMode.Multiple,
                CssClass = EFSCssClass.DropDownListCapture
            };

            chkCulture = new CheckBoxList
            {
                AutoPostBack = true
            };
            chkCulture.SelectedIndexChanged += new EventHandler(OnCheckedIndexChanged);

            divMapZone.Controls.Add(lblWindowsId);
            divMapZone.Controls.Add(ddlWindowsId);
            divMapZone.Controls.Add(lblTerritory);
            divMapZone.Controls.Add(ddlTerritory);

            divMapZone.Controls.Add(lblTzdb);
            divMapZone.Controls.Add(lstTzdb);

            divMapZone.Controls.Add(chkCulture);


            WCToolTipLinkButton btnProcess = ControlsTools.GetAwesomeButtonAction("btnOk", "fa fa-caret-square-right", true);
            btnProcess.Click += new EventHandler(this.OnProcessClick);

            divCriteria.Controls.Add(divDateInput);
            divCriteria.Controls.Add(divMapZone);
            divCriteria.Controls.Add(btnProcess);

            // RESULT
            Panel divResult = new Panel
            {
                Width = Unit.Percentage(80),
                Height = Unit.Percentage(100)
            };
            divResult.Style.Add(HtmlTextWriterStyle.Padding, "0px 10px");
            divResult.Style.Add(HtmlTextWriterStyle.BackgroundColor, "#f7e9e9");
            divResult.Style.Add("border-bottom-right-radius", "7px");

            HtmlGenericControl resultTitle = new HtmlGenericControl("h2")
            {
                InnerText = "Résultats"
            };

            Panel pnlGv = new Panel();
            pnlGv.Style.Add(HtmlTextWriterStyle.Overflow, "auto");
            pnlGv.Style.Add(HtmlTextWriterStyle.BorderStyle, "solid");
            pnlGv.Style.Add(HtmlTextWriterStyle.BorderWidth, "1pt");
            pnlGv.Style.Add(HtmlTextWriterStyle.BorderColor, "#C00303");
            pnlGv.Height = Unit.Pixel(460);

            gvResult = new GridView
            {
                CellSpacing = 0,
                CellPadding = 4,


                CssClass = "DataGrid"
            };
            gvResult.RowStyle.CssClass = "DataGrid_FlatHouse";
            gvResult.AlternatingRowStyle.Wrap = false;
            gvResult.AlternatingRowStyle.CssClass = "DataGrid_AlternatingItemStyle";
         
            gvResult.RowStyle.Wrap = false;
            gvResult.SelectedRowStyle.CssClass = "DataGrid_SelectedItemStyle";
            gvResult.SelectedRowStyle.Wrap = false;
            gvResult.HeaderStyle.CssClass = "DataGrid_HeaderStyle";
            gvResult.HeaderStyle.Wrap = false;
            gvResult.HeaderStyle.Font.Size = 8;
            gvResult.Width = Unit.Percentage(100);

            pnlGv.Controls.Add(gvResult);
            divResult.Controls.Add(resultTitle);
            divResult.Controls.Add(pnlGv);

            divNodatime.Controls.Add(divHeader);
            divBody.Controls.Add(divCriteria);
            divBody.Controls.Add(divResult);
            divNodatime.Controls.Add(divBody);
            CellForm.Controls.Add(divNodatime);

            if (false == IsPostBack)
            {
                Initialize();
            }
        }
        private void Initialize()
        {
            timestamp.Zdt.Parse = DateTimeOffset.Now.ToString();

            Tz.Web.LoadWindowsIdToListControl(ddlWindowsId, true);
            if (0 < ddlWindowsId.Items.Count)
            {
                ddlWindowsId.SelectedIndex = 0;
                Tz.Web.LoadTerritoryToListControl(ddlTerritory, ddlWindowsId.SelectedValue, true);
                if (0 < ddlTerritory.Items.Count)
                {
                    ddlTerritory.SelectedIndex = 0;
                    Tz.Web.LoadTzdbIdByTerritoryToListControl(lstTzdb, ddlTerritory.SelectedValue);
                }
            }

            List<CultureInfo> lstCultures = CultureInfo.GetCultures(CultureTypes.AllCultures).Where(culture =>
                (false == culture.IsNeutralCulture) && "en-GB|en-US|fr-BE|fr-FR|it-IT|es-ES".Contains(culture.Name)).OrderBy(o => o.EnglishName).ToList();
            lstCultures.ForEach(culture => chkCulture.Items.Add(new ListItem(culture.EnglishName, culture.LCID.ToString())));
        }
        #region OnSelectedIndexChanged
        public void OnSelectedIndexChanged(object sender, EventArgs e)
        {
            ListControl control = sender as ListControl;
            if (control.ID == "ddlWindowsId")
            {
                Tz.Web.LoadTerritoryToListControl(ddlTerritory, ddlWindowsId.SelectedValue, true);
                if (0 < ddlTerritory.Items.Count)
                    ddlTerritory.SelectedIndex = 0;
            }
            Tz.Web.LoadTzdbIdByTerritoryToListControl(lstTzdb, ddlTerritory.SelectedValue, ddlWindowsId.SelectedValue);
        }
        #endregion OnSelectedIndexChanged

        #region OnCheckedIndexChanged
        public void OnCheckedIndexChanged(object sender, EventArgs e)
        {
            gvResult.DataSource = null;
            gvResult.DataBind();
        }
        #endregion OnCheckedIndexChanged

        protected void Page_Load(object sender, System.EventArgs e)
        {
            timestamp.ApplyPostedValue();
        }

        #region OnProcessClick
        protected void OnProcessClick(object sender, System.EventArgs e)
        {
            ZoneLocalMappingResolver resolver = Resolvers.CreateMappingResolver(Resolvers.ReturnEarlier, Resolvers.ReturnStartOfIntervalAfter);


            OffsetDateTime odtSource = timestamp.Zdt.DtZoned.ToOffsetDateTime();

            IList<DtResult> lstDtResult = new List<DtResult>();

            List<ListItem> lstCulture = chkCulture.Items.Cast<ListItem>().Where(li => li.Selected).ToList();

            lstTzdb.GetSelectedIndices().ToList().ForEach(index =>
            {
                string tzdbID = lstTzdb.Items[index].Value;
                MapZone mapZone = Tz.Tools.MapZoneByTzDbId(tzdbID);
                ZonedDateTime zdt = Tz.Tools.ToZonedDateTime(odtSource, tzdbID);

                DtResult dtResult = null;
                lstCulture.ForEach(item =>
                {
                    CultureInfo culture = CultureInfo.GetCultureInfo(Convert.ToInt32(item.Value));
                    if (false == culture.LCID.Equals(CultureInfo.InvariantCulture.LCID))
                        Tz.Tools.SetCulturePatterns(culture);
                    dtResult = new DtResult
                    {
                        TzdbID = tzdbID,
                        Territory = mapZone.Territory,
                        WindowsID = mapZone.WindowsId,
                        PatternCulture = culture.EnglishName,
                        UtcDateTime = Tz.Tools.ToString(zdt.ToInstant(), Tz.Tools.cPatterns.iso8601UTCPattern.PatternText, null),
                        OffsetDateTime = Tz.Tools.ToString(zdt.ToOffsetDateTime(), culture),
                        ZonedDateTime = Tz.Tools.ToString(zdt, culture)
                    };
                    lstDtResult.Add(dtResult);
                });
            });
            gvResult.DataSource = lstDtResult;
            gvResult.DataBind();
        }
        #endregion OnProcessClick
    }
}