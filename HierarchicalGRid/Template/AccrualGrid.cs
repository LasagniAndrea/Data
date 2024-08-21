using System;
using System.Collections;
using System.Data;
using System.Drawing;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using EFS.ACommon;
using EFS;
using EFS.EFSTools;
using EFS.Referentiel;

namespace EFS
{
	#region PanelAccrual
	public class PanelAccrual : Panel
	{
		#region Variables
		Panel       pnlAccrual;
		AccrualGrid dgAccrual;
		#endregion Variables
		#region Constructor
		public PanelAccrual() : base()
		{
			this.ID                      = "HG5";
			this.Height                  = Unit.Pixel(120);
			pnlAccrual                   = new Panel();
			pnlAccrual.Height            = Unit.Pixel(80);
			pnlAccrual.Style["overflow"] = "auto"; 
			dgAccrual                    = new AccrualGrid();

			pnlAccrual.Controls.Add(dgAccrual);
			this.Controls.Add(pnlAccrual);

			this.DataBinding += new System.EventHandler(OnDataBinding);
		}
		#endregion Constructor
		#region Events
		#region OnDataBinding
		private void OnDataBinding(object sender, System.EventArgs e){}
		#endregion OnDataBinding
		#region OnPreRender
		protected override void OnPreRender(EventArgs e)
		{   
			if (0 < this.Controls.Count)
			{
				Table tbl        = new Table();
				tbl.CssClass     = "DataGrid";
				tbl.BorderStyle  = BorderStyle.Solid;
				tbl.BorderWidth  = Unit.Point(1);
				tbl.GridLines    = GridLines.Both;
				tbl.CellSpacing  = 0;
				tbl.CellPadding  = 2;
				tbl.Width        = Unit.Percentage(100);
				#region Title
				TableRow rowTitle  = this.dgAccrual.AddRowTitle(Ressource.GetString("Accruals"));
				rowTitle.CssClass  = "DataGrid_FooterStyle";
				tbl.Rows.Add(rowTitle);
				#endregion Title
				#region Header
				TableRow rowHeader = this.dgAccrual.CloneRowHeader;
				rowHeader.CssClass = "DataGrid_SubstituteHeaderStyle";
				tbl.Rows.Add(rowHeader);
				#endregion Header
				this.Controls.AddAt(0,tbl);
			}
			base.OnPreRender(e);
		}
		#endregion OnPreRender
		#endregion Events
	}
	#endregion PanelAccrual
	#region AccrualGrid
	public class AccrualGrid : HierarchicalGrid
	{
		#region Variables
		protected string xmlReferentialFileName = "Event/XML_Files/XMLReferentialACCRUAL.xml";
		#endregion Variables
		#region Constructor
		public AccrualGrid() : base()
		{
			#region Grid properties
			this.ID                  = "hgAccrual";
			this.TemplateCachingBase = CachingBases.Tablename;
			this.TemplateDataMode    = TemplateDataModes.Table;
			this.LoadControlMode     = LoadControlModes.WebControl;

			this.ShowHeader          = false;
			this.AllowPaging         = false;
			this.AllowSorting        = false;
			this.ShowFooter          = false;

			HeaderStyle.CssClass          = "DataGrid_SubstituteHeaderStyle";
			ItemStyle.CssClass            = "DataGrid_SubstituteItemStyle";
			AlternatingItemStyle.CssClass = "DataGrid_SubstituteItemStyle";
			#endregion Grid properties
			#region Grid events
			this.TemplateSelection         += new HierarchicalGridTemplateSelectionEventHandler(OnTemplateSelection);
			this.TemplateDataModeSelection += new HierarchicalGridTemplateDataModeSelectionEventHandler(OnTemplateDataSelection);
			this.DataBinding               += new System.EventHandler(OnDataBinding);
			#endregion Grid events
			this.LoadReferential(xmlReferentialFileName);
		}
		#endregion Constructor
		#region Events
		#region OnDataBinding
		private void OnDataBinding(object sender, System.EventArgs e)
		{
			DataGridItem dgi  = (DataGridItem) this.BindingContainer;
			this.DataSource   = dgi.DataItem;
			this.DataMember   = "Accrual";
			this.RelationName = "Accrual";
		}
		#endregion OnDataBinding
		#region OnPreRender
		protected override void OnPreRender(EventArgs e)
		{   
			DataGridItem item;
			DataGridItem dgi = null;
			Table table      = (Table)this.Controls[0];
			int j = 1;
			if (0 < this.Controls.Count)
			{
				for (int i=0;i<this.Items.Count;i++)
				{
					item = this.Items[i];
					if (item.ItemType == ListItemType.Item || 
						item.ItemType == ListItemType.AlternatingItem || 
						item.ItemType == ListItemType.SelectedItem)
					{
						dgi = AddItemDetail(item);
						table.Rows.AddAt(item.ItemIndex + 1 + j,dgi);
						j++;
					}
				}

			}
			base.OnPreRender(e);
		}
		#endregion OnPreRender
		#region OnTemplateSelection
		private void OnTemplateSelection(object sender, HierarchicalGridTemplateSelectionEventArgs e)
		{
			switch(e.Row.Table.TableName)
			{
				case "Event":
					e.TemplateFilename = this.GetType().Name;
					e.RelationName     = "Accrual";
					break;
				default:
					throw new NotImplementedException("Unexpected child row in TemplateSelection event");
			}
		}
		#endregion OnTemplateSelection
		#region OnTemplateDataSelection
		private void OnTemplateDataSelection(object sender, HierarchicalGridTemplateDataModeSelectionEventArgs e)
		{
			switch(e.Relation.RelationName)
			{
				case "Accrual":
					e.TemplateDataMode = TemplateDataModes.Table;
					break;
				default:
					throw new NotImplementedException("Unexpected Relation in TemplateDataSelection event");
			}
		}
		#endregion OnTemplateSelection
		#endregion Events
		#region Methods
		#region AddItemDetail
		private DataGridItem AddItemDetail(DataGridItem pItem)
		{
			DataView    dv   = (DataView) this.DataSource;
			DataRowView drv  = dv[pItem.ItemIndex];
			TableCell   td   = new TableCell();
			td.ColumnSpan    = pItem.Cells.Count;
			DataGridItem dgi = new DataGridItem(pItem.ItemIndex + 1,pItem.DataSetIndex,ListItemType.AlternatingItem);
			dgi.CssClass     = string.Empty;
			dgi.Cells.Add(td);

			WCRoundedTable roundedTable = new WCRoundedTable();
			td.Controls.Add(roundedTable);
			
			Table table = new Table();
			table.Width = Unit.Percentage(100);

			TableRow tr = new TableRow();
			tr.CssClass = "DataGrid_ToolTipStyle";
			SetInfoInRow(tr,GetInfoColumn(drv,"LINEARAMOUNT"));
			SetInfoInRow(tr,GetInfoColumn(drv,"ACTUARIALAMOUNT"));
			SetInfoInRow(tr,GetInfoColumn(drv,"DCF"));
			SetInfoInRow(tr,GetInfoColumn(drv,"DCFNUM"));
			SetInfoInRow(tr,GetInfoColumn(drv,"TOTALOFYEAR"));
			table.Rows.Add(tr);

			tr           = new TableRow();
			dgi.CssClass = "DataGrid_ToolTipStyle";
			SetInfoInRow(tr,GetInfoColumn(drv,"LINEARRATE"));
			SetInfoInRow(tr,GetInfoColumn(drv,"ACTUARIALRATE"));
			SetInfoInRow(tr,2);
			SetInfoInRow(tr,GetInfoColumn(drv,"DCFDEN"));
			SetInfoInRow(tr,GetInfoColumn(drv,"TOTALOFDAY"));
			table.Rows.Add(tr);
			roundedTable.AddContent(table);
			return dgi;
		}
		#endregion AddItemDetail
		#endregion Methods
	}
	#endregion AccrualGrid
}

