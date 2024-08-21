#region Using Directives
using EFS.ACommon;
using EFS.Common.Web;
using EFS.TradeInformation;
using System;
using System.Data;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;
#endregion Using Directives
namespace EFS.Spheres
{
    /// <summary>
    /// Description résumée de FpMLCopyPaste.
    /// </summary>
    public partial class FpMLCopyPastePage : PageBase
	{
		#region Members
		private string               m_ParentGUID;
		private string               m_CopyPastePanelID;
		private TradeCommonInputGUI  m_TradeCommonInputGUI;
        private TradeCommonInput     m_TradeCommonInput;
		private string               m_ObjectName;
		private string               m_FieldName;
		private string               m_SubTitle;
		#endregion Members
		#region Accessors
		/// <summary>
		/// 
		/// </summary>
		/// FI 2020018 [XXXXX] Rename
		protected string ParentInputGUISessionID
		{
			get {return m_ParentGUID + "_GUI";}
		}
		/// <summary>
		/// 
		/// </summary>
		/// FI 2020018 [XXXXX] Rename
		protected string ParentInputSessionID
		{
			get {return m_ParentGUID + "_Input";}
		}
		
        #region TradeCommonInput
        public TradeCommonInput TradeCommonInput
        {
            set { m_TradeCommonInput = value; }
            get { return m_TradeCommonInput; }
        }
		#endregion TradeCommonInput

		public TradeCommonInputGUI TradeCommonInputGUI
		{
			set { m_TradeCommonInputGUI = value; }
			get { return m_TradeCommonInputGUI; }
		}
		
		#endregion Accessors

		#region Events
		#region OnInit
		protected override void OnInit(EventArgs e)
        {
            InitializeComponent();
            base.OnInit(e);
            //
            m_ParentGUID = Request.QueryString["GUID"];
            //if (StrFunc.IsEmpty(m_ParentGUID))
            //    throw new ArgumentException("Argument GUID expected");
            m_CopyPastePanelID = Request.QueryString["CopyPastePanelID"];

			// FI 20200518 [XXXXX] Utilisation de DataCache
			//m_TradeCommonInputGUI = (TradeCommonInputGUI)Session[InputGUISessionID];
			//m_TradeCommonInput = (TradeCommonInput)Session[InputSessionID];
			m_TradeCommonInputGUI = (TradeCommonInputGUI)DataCache.GetData<TradeCommonInputGUI>(ParentInputGUISessionID);
			m_TradeCommonInput = (TradeCommonInput)DataCache.GetData<TradeCommonInput>(ParentInputSessionID);
            
			
			m_ObjectName = Request.QueryString["ObjectName"];
            m_FieldName = Request.QueryString["FieldName"];
            m_SubTitle = Request.QueryString["SubTitle"];
            //
            PageConstruction();

        }
		#endregion OnInit
		#region OnLoad
		protected void OnLoad(object sender, System.EventArgs e)
		{
			// Placer ici le code utilisateur pour initialiser la page
		}
		#endregion OnLoad
		#region OnDelete
		private void OnDelete(object sender, CommandEventArgs e)
		{
			int idClipBoard = Convert.ToInt32(e.CommandArgument.ToString());
			if (Cst.ErrLevel.SUCCESS == SessionTools.SessionClipBoard.Delete(idClipBoard))
				RemoveTableRow(idClipBoard);
		}
		#endregion OnDelete
		#endregion Events
		#region Methods
		#region AddListCopy
		// EG 20200902 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc) Corrections et compléments
		private void AddListCopy()
		{
			#region Panel Container
			Panel pnlPaste = new Panel()
			{
				ID = "divDtg",
				Height = Unit.Pixel(185),
				Width = Unit.Percentage(100),
				CssClass = "input orange"
			};
			pnlPaste.Style[HtmlTextWriterStyle.Overflow] = "auto";
			pnlPaste.Style[HtmlTextWriterStyle.MarginTop] = "10px";
			#endregion Panel Container

			#region Table of copies
			Table table = new Table()
			{
				ID = "tblListCopy",
				CssClass = "DataGrid",
				CellPadding = 3,
				CellSpacing = 0,
				GridLines = GridLines.Both
			};

			SessionClipBoard sessionClipBoard = new SessionClipBoard(SessionTools.CS,SessionTools.AppSession);
			DataSet dsCopy = sessionClipBoard.Select(m_ObjectName);
			if (null != dsCopy)
			{
				#region Header
				TableRow tr  = new TableRow();
				TableCell td = new TableCell();
				tr.CssClass  = "DataGrid_HeaderStyle";
				tr.Cells.Add(TableTools.AddCell(Cst.HTMLSpace,false));
				tr.Cells.Add(TableTools.AddCell(Ressource.GetString("IdClipBoard"), false));
				tr.Cells.Add(TableTools.AddCell(Ressource.GetString("Date"),false));
				tr.Cells.Add(TableTools.AddCell(Ressource.GetString("Time"), false));
				tr.Cells.Add(TableTools.AddCell(Ressource.GetString("DisplayNameClipBoard"),false));
				tr.Cells.Add(TableTools.AddCell(Ressource.GetString("Field"), false));
				tr.Cells.Add(TableTools.AddCell(Ressource.GetString("IdObject"), false));
				tr.Cells.Add(TableTools.AddCell(Ressource.GetString("Extract"), false));
				table.Rows.Add(tr);
				#endregion Header
				int j = 0;
				int k = 0;
				string msgDelCopy = Ressource.GetString("Msg_DelCopy");
				foreach (DataRow row in dsCopy.Tables[0].Rows)
				{
                    tr = new TableRow
                    {
                        ID = row["IDCLIPBOARD"].ToString()
                    };
                    // Image to delete
                    WCToolTipLinkButton button = new WCToolTipLinkButton()
					{
						Text = "<i class='fa fa-trash-alt'></i>",
						CssClass = "fa-icon",
						ID = "Del" + tr.ID,
						CommandArgument = tr.ID
					};
					button.Command += new CommandEventHandler(this.OnDelete);

					StringBuilder sb = new StringBuilder();
					sb.AppendFormat("if (ConfirmDelCopy({0},{1},{2},{3},{4}))",JavaScript.JSString(msgDelCopy), 
						JavaScript.JSString(row["IDCLIPBOARD"].ToString()),
						JavaScript.JSString(row["DISPLAYNAME"].ToString()),
						JavaScript.JSString(row["FIELDNAME"].ToString()),
						JavaScript.JSString(row["OBJECTID"].ToString()));
					sb.Append("return true;");
					sb.Append("return false;");
					button.Attributes.Add("onclick", sb.ToString());


					td = new TableCell();
					td.Controls.Add(button);
					td.Style.Add(HtmlTextWriterStyle.Cursor,"pointer");
					td.Width = Unit.Pixel(30);
					tr.Cells.Add(td);
					//
					tr.Cells.Add(TableTools.AddCell(row["IDCLIPBOARD"].ToString(),HorizontalAlign.Right));
					tr.Cells.Add(TableTools.AddCell(Convert.ToDateTime(row["DTSYS"]).ToShortDateString(),HorizontalAlign.Center));
					tr.Cells.Add(TableTools.AddCell(Convert.ToDateTime(row["DTSYS"]).ToLongTimeString(),HorizontalAlign.Center));
				    tr.Cells.Add(TableTools.AddCell(row["DISPLAYNAME"].ToString(),HorizontalAlign.Left));
					tr.Cells.Add(TableTools.AddCell(row["FIELDNAME"].ToString(),HorizontalAlign.Left));
					tr.Cells.Add(TableTools.AddCell(row["OBJECTID"].ToString(),HorizontalAlign.Left));

					sb = new StringBuilder();
					sb.Append("window.opener.SetPasteChoice('" + m_CopyPastePanelID + "','" + row["IDCLIPBOARD"].ToString() + "');");
					sb.Append("self.close();");
					tr.Attributes.Add("ondblclick",sb.ToString());
                    tr.Style.Add(HtmlTextWriterStyle.Cursor, "pointer");

					int z = Math.DivRem(j,2,out k);
					tr.Attributes.Add("onmouseover","this.className='DataGrid_SelectedItemStyle';");
					tr.CssClass = "DataGrid_ItemStyle";
					tr.Attributes.Add("onmouseout", "this.className='DataGrid_ItemStyle';");

					#region XML Extrait
					string xml      = row["OBJECTXML"].ToString();
					int indexof     = xml.IndexOf("<" + m_FieldName);
					int start       = Math.Max(0,indexof);
					int length      = Math.Min(xml.Length -1 - start,Math.Min(200,xml.Length-1));
					string lightXML = xml.Substring(start,length) + "...";
					lightXML        = lightXML.Replace("<","&lt;").Replace(">","&gt;");
                    td = new TableCell
                    {
                        Text = lightXML,
                        ToolTip = xml.Substring(Math.Max(0, indexof)),
                        Width = Unit.Percentage(100),
                        Wrap = true
                    };
                    td.Style.Add(HtmlTextWriterStyle.Cursor, "pointer");
					tr.Cells.Add(td);
					#endregion XML Extrait
					table.Rows.Add(tr);
					j++;
				}
			}
			pnlPaste.Controls.Add(table);
			#endregion Table of copies
			CellForm.Controls.Add(pnlPaste);

		}
		#endregion AddListeCopy
		#region GenerateHtmlForm
		protected override void GenerateHtmlForm()
		{
			base.GenerateHtmlForm(); 
			Form.ID="frmPasteChoice";
			AddListCopy();
		}
		#endregion GenerateHtmlForm
		#region InitializeComponent
		private void InitializeComponent()
		{    
			this.Load += new System.EventHandler(this.OnLoad);
		}
		#endregion InitializeComponent
		#region PageConstruction
		// EG 20200828 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc) Ensemble des écrans de saisie au format FpML
		// EG 20200902 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc) Corrections et compléments
		protected override void PageConstruction()
		{
            AbortRessource = true;
            string title = Ressource.GetString("PasteChoice");
            HtmlPageTitle titleLeft = new HtmlPageTitle(title, m_SubTitle);
            GenerateHtmlForm();
            this.PageTitle = title;
            FormTools.AddBanniere(this, Form, titleLeft, new HtmlPageTitle(), null, IdMenu.GetIdMenu(IdMenu.Menu.Input));
            PageTools.BuildPage(this, Form, PageFullTitle, null, false, null, IdMenu.GetIdMenu(IdMenu.Menu.Input));
        }
        #endregion PageConstruction
        #region RemoveTableRow
        private void RemoveTableRow(int pIdClipBoard)
		{
			Table ctrl = (Table) FindControl("tblListCopy");
			if (null != ctrl)
			{
				TableRow row = (TableRow) ctrl.FindControl(pIdClipBoard.ToString());
				if (null != row)
					ctrl.Rows.Remove(row);
			}
		}
		#endregion RemoveTableRow
		#endregion Methods
	}
}
