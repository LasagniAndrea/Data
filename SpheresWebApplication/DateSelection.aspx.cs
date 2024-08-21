using System;
using System.Reflection;
using System.Collections;
using System.Data;
using System.Drawing;
using System.Web;
using System.Web.SessionState;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using System.Diagnostics;
using System.Globalization;
using System.Threading;
using System.Text;

using EFS.ACommon;
using EFS.Common;
using EFS.Common.Web; 

namespace EFS.Spheres
{
    /// <creation>
    ///     <version>???</version><date>???</date><author>???</author>
    /// </creation>
    /// <revision>
    ///     <version>1.0.4</version><date>20041202</date><author>OD</author>
    ///     <comment>
    ///     -Ajout de la gestion du DateTime (#region traitement du DateTime)
    ///     </comment>
    /// </revision>
    /// <revision>
    ///     <version>[X.X.X]</version><date>[YYYYMMDD]</date><author>[Initial]</author>
    ///     <comment>
    ///     [To insert here a description of the made modifications ]
    ///     </comment>
    /// </revision>
	/// <summary>
	/// 
	/// </summary>
	public partial class DateSelectionPage : PageBase
	{

		protected string m_PageTitle;
		protected string m_SelectedDate;
		protected string m_SelectedTime;
		private   string m_IdBound;
		private   bool   m_IsTypeDateTime;

		private const int MinYear = 1980;
		private const int MaxYear = 2099;
		
		#region Events
		#region OnCriteriaChange
		protected void OnCriteriaChange(object sender, System.EventArgs e)
		{
			int year  = Convert.ToInt32(lstYear.SelectedItem.Value);
			int month = Convert.ToInt32(lstMonth.SelectedItem.Value);
			
			if( (MinYear < year) && (0 < month))
			{
				calCapture.VisibleDate  = new DateTime(year,month , 1);
				calCapture.SelectedDate = calCapture.VisibleDate;
			}
			SelectCorrectValues();
		}
		#endregion OnCriteriaChange
		#region OnDateCalendarChange
		protected void OnDateCalendarChange(object sender, System.EventArgs e)
		{

			DateTime dtCalendar    = calCapture.SelectedDate;

			dtCalendar             = DtFunc.AddTimeToDate(dtCalendar,new DtFunc().StringToDateTime(m_SelectedTime,DtFunc.FmtLongTime));
			m_SelectedDate         = DtFunc.DateTimeToString(dtCalendar,m_IsTypeDateTime?DtFunc.FmtDateLongTime:DtFunc.FmtShortDate);
			calCapture.VisibleDate = dtCalendar;
			SelectCorrectValues();

			StringBuilder sb = new StringBuilder();
			sb.Append("\n<SCRIPT LANGUAGE='JAVASCRIPT' id='SetDateToOpener'>\n");
			sb.Append("window.opener.SetDate(\"" + m_IdBound + "\",\"" + m_SelectedDate + "\");");
			sb.Append("self.close();");
			sb.Append("</SCRIPT>\n");
            //20071212 FI Ticket 16012 => Migration Asp2.0
            if (!this.Page.ClientScript.IsClientScriptBlockRegistered("SetDateToOpener"))
                this.Page.ClientScript.RegisterClientScriptBlock(GetType(),"SetDateToOpener", sb.ToString());
		}

		#endregion OnDateCalendarChange
		#region OnInit
		protected override void OnInit(EventArgs e)
		{
			InitializeComponent();
			base.OnInit(e);
			//    
			#region QueryString reader
			m_PageTitle = Request.QueryString["Title"].ToString();
			m_IdBound = Request.QueryString["ID"].ToString();
			m_IsTypeDateTime = (Request.QueryString["MODE"].ToString() == "DT");
			m_SelectedDate = Request.QueryString["Value"].ToString();
			if (StrFunc.IsEmpty(m_PageTitle))
				m_PageTitle = Ressource.GetString("SelectADate");
			#endregion QueryString reader

		}
		#endregion OnInit
		#region OnLoad
		protected void OnLoad(object sender, System.EventArgs e)
		{
            PageTools.SetHead(this, m_PageTitle, null, null);
            #region GetSelectedDateTime
            DateTime dtSelected;
            if (false == IsPostBack)
            {
                dtSelected = new DtFunc().StringToDateTime(m_SelectedDate, m_IsTypeDateTime ? DtFunc.FmtDateLongTime : DtFunc.FmtShortDate);
                txtSelectedTime.Text = DtFunc.DateTimeToString(dtSelected, DtFunc.FmtLongTime);
            }
            else
                dtSelected = calCapture.SelectedDate;

            if (DtFunc.IsDateTimeEmpty(dtSelected))
			{
				dtSelected           = SystemTools.GetOSDateSys();			  
				txtSelectedTime.Text = DtFunc.DateTimeToString(dtSelected,DtFunc.FmtLongTime);
			}
			m_SelectedTime = txtSelectedTime.Text;
			#endregion GetSelectedDateTime

			#region Initialize SelectedDate to Calendar
			calCapture.SelectedDate = dtSelected;
			calCapture.VisibleDate  = calCapture.SelectedDate;
			#endregion Initialize SelectedDate to Calendar

			if (false == IsPostBack)
			{
				FillCalendarChoices();
				SelectCorrectValues();
			}
			this.DataBind();
            
			if (this.calCapture.ID == Page.Request.Params["__EVENTTARGET"])
			{
				try
				{
					if (null != Page.Request.Params["__EVENTARGUMENT"])
					{
						// ID du jour compteur incremental par rapport au 01/01/2000 (id=1)
						DateTime dtCompare = new DtFunc().StringToDateTime("01/01/2000",DtFunc.FmtShortDate);
						dtCompare = dtCompare.AddDays(Convert.ToInt32(Page.Request.Params["__EVENTARGUMENT"]));
						if (this.calCapture.SelectedDate.Equals(dtCompare))
							OnDateCalendarChange(this,null);
					}
				}
				catch {}
			}
		}
		#endregion OnLoad
		#region OnVisibleMonthChanged
		private void OnVisibleMonthChanged(object sender, System.Web.UI.WebControls.MonthChangedEventArgs e)
		{
			calCapture.SelectedDate = calCapture.VisibleDate;
			SelectCorrectValues();
		}
		#endregion OnVisibleMonthChanged
		#endregion Events
		#region Methods
		#region FillCalendarChoices
		private void FillCalendarChoices()
		{
			DateTime thisDate = new DateTime(DateTime.Today.Year, 1, 1);
			for (int x=0;x<12;x++)
			{
				ListItem li = new ListItem(thisDate.ToString("MMMM"),thisDate.Month.ToString());
				lstMonth.Items.Add(li);
				thisDate = thisDate.AddMonths(1);
			}
			//
			for (int y=MinYear;y<MaxYear;y++)
				lstYear.Items.Add(y.ToString());
		}

		#endregion FillCalendarChoices
		#region InitializeComponent
		private void InitializeComponent()
		{    
			this.calCapture.VisibleMonthChanged += new System.Web.UI.WebControls.MonthChangedEventHandler(this.OnVisibleMonthChanged);
			this.Load += new System.EventHandler(this.OnLoad);

		}
		#endregion InitializeComponent
		#region SelectCorrectValues
		private void SelectCorrectValues()
		{
			DateTime dtSelected = calCapture.SelectedDate;
			if (DtFunc.IsDateTimeFilled(dtSelected))   
			{
				lstMonth.SelectedIndex = lstMonth.Items.IndexOf(lstMonth.Items.FindByValue(dtSelected.Month.ToString()));
				lstYear.SelectedIndex  = lstYear.Items.IndexOf(lstYear.Items.FindByValue(dtSelected.Year.ToString()));
			}			
		}
		#endregion SelectCorrectValues
		#endregion Methods
	}
}
