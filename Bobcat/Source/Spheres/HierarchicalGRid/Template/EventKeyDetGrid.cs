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

using EfsML.Enum;

namespace EFS
{
	#region PanelEventKeyAsset
	public class PanelEventKeyAsset : HierarchicalPanel
	{
		#region Variables
		public EventKeyAssetGrid dgEventKeyAsset;
		#endregion Variables
		#region Constructors
		public PanelEventKeyAsset() : base()
		{
			dgEventKeyAsset = new EventKeyAssetGrid();
			this.Controls.Add(dgEventKeyAsset);
		}
		#endregion Constructors
		#region OnPreRender
		protected override void OnPreRender(EventArgs e)
		{
			if (0 < this.dgEventKeyAsset.Items.Count)
			{
				this.Height                = Unit.Pixel(55);
				this.Style["overflow"]     = "auto"; 
			}
		}
		#endregion OnPreRender
	}
	#endregion PanelEventKeyAsset
	#region PanelEventKeyDet
	public class PanelEventKeyDet : HierarchicalPanel
	{
		#region Variables
		public EventKeyDetailGrid dgEventKeyDet;
		#endregion Variables
		#region Constructors
		public PanelEventKeyDet() : base()
		{
			dgEventKeyDet = new EventKeyDetailGrid();
			this.Controls.Add(dgEventKeyDet);
		}
		#endregion Constructors
		#region OnPreRender
		protected override void OnPreRender(EventArgs e)
		{
			if (0 < this.dgEventKeyDet.Items.Count)
			{
				this.Height                = Unit.Pixel(55);
				this.Style["overflow"]     = "auto"; 
			}
		}
		#endregion OnPreRender
	}
	#endregion PanelEventKeyDet
	#region EventKeyDetGrid
	public class EventKeyDetGrid : HierarchicalGrid
	{
		#region Variables
		protected string    m_XmlReferentialFileName;
		protected ArrayList m_InfoColumn;
		#endregion Variables
		#region Constructors
		public EventKeyDetGrid() : base()
		{
			#region Grid properties
			this.ID                  = "hgEventKeyDet";
			this.GridLines           = GridLines.None;
			this.BackColor           = Color.Transparent;
			this.AlternatingItemStyle.BackColor = Color.Transparent;
			this.ItemStyle.BackColor = Color.Transparent;
			this.ShowHeader          = false;
			#endregion Grid properties
			this.DataBinding += new System.EventHandler(OnDataBinding);
		}
		#endregion Constructors
		#region Events
		#region OnDataBinding
		protected virtual void OnDataBinding(object sender, System.EventArgs e)
		{
			DataGridItem dgi  = (DataGridItem) this.BindingContainer;
			this.DataSource   = dgi.DataItem;
			if (1 <= ((DataView)this.DataSource).Count)
				m_InfoColumn = new ArrayList();
		}
		#endregion OnDataBinding
		#region OnPreRender
		protected override void OnPreRender(EventArgs e)
		{
			if ((0 < this.Controls.Count) && (null != m_InfoColumn && 0 < m_InfoColumn.Count))
			{
				Table tbl = (Table)this.Controls[0];
				DataGridItem dgi  = new DataGridItem(0,0,ListItemType.Item);
				TableCell cell    = new TableCell();
				cell.ColumnSpan   = tbl.Rows[0].Cells.Count;
				Table table       = new Table();
				table.CssClass    = "DataGrid_ItemStyle";
				table.CellSpacing = 0;
				table.CellPadding = 2;
				table.BorderStyle = BorderStyle.None;
				table.GridLines   = GridLines.None;
				table.Width       = Unit.Percentage(100);

				foreach (object item in m_InfoColumn)
				{
					if (null != item)
					{
						if (item.GetType().IsArray)
						{
							TableRow tr = new TableRow();
							foreach (InfoColumn infoColumn in (InfoColumn[])item)
							{
								if (StrFunc.IsFilled(infoColumn.columnValue))
									SetInfoInRow(tr,infoColumn,infoColumn.columnSpan);
							}
							table.Controls.Add(tr);
						}
						else
						{
							InfoColumn infoColumn = (InfoColumn)item;
							if (StrFunc.IsFilled(infoColumn.columnValue))
							{
								TableRow tr = new TableRow();
								SetInfoInRow(tr,infoColumn);
								table.Controls.Add(tr);
							}
						}
					}
				}
				tbl.Rows.Clear();
				if (0 < table.Rows.Count)
				{
					cell.Controls.Add(table);
					dgi.Cells.Add(cell);
					tbl.Controls.AddAt(0,dgi);
				}
			}
			base.OnPreRender(e);
		}
		#endregion OnPreRender
		#endregion Events
	}
	#endregion EventKeyDetGrid
	#region EventKeyAssetGrid
	public class EventKeyAssetGrid : EventKeyDetGrid
	{
		#region Constructors
		public EventKeyAssetGrid() : base()
		{
			this.ID                  = "hgEventKeyAsset";
			this.ShowHeader          = true;
			m_XmlReferentialFileName = "Event/XML_Files/XMLReferentialEVENTKEYASSET.xml";
			this.LoadReferential(m_XmlReferentialFileName);
			this.DataBinding += new System.EventHandler(OnDataBinding);
		}
		#endregion Constructors
		#region Events
		#region OnDataBinding
		override protected void OnDataBinding(object sender, System.EventArgs e)
		{
			base.OnDataBinding(sender,e);
			this.DataMember   = "EventKeyAsset";
			this.RelationName = "EventKeyAsset";
		}
		#endregion OnDataBinding
		#region OnItemDataBound
		override protected void OnItemDataBound(DataGridItemEventArgs e)
		{
			base.OnItemDataBound(e);
			if (e.Item.ItemType == ListItemType.Item)
			{
				DataGridItem dgi  = (DataGridItem) this.BindingContainer;
				this.DataSource   = dgi.DataItem;
				DataView dv       = (DataView)this.DataSource;
				if (1 <= dv.Count)
				{
					foreach (DataRowView drv in dv)
					{
						m_InfoColumn.Add(GetInfoColumns(drv,"ID"));
						m_InfoColumn.Add(GetInfoColumns(drv,"PRIMARYRATESRC"));
						m_InfoColumn.Add(GetInfoColumns(drv,"PRIMARYRATESRCPAGE"));
						m_InfoColumn.Add(GetInfoColumns(drv,"PRIMARYRATESRCHEAD"));
						m_InfoColumn.Add(GetInfoColumns(drv,"QUOTESIDE"));
						m_InfoColumn.Add(GetInfoColumns(drv,"QUOTETIMING"));
					}
				}
			}
		}
		#endregion OnItemDataBound
		#endregion Events
	}
	#endregion EventKeyAssetGrid
	#region EventKeyDetailGrid
	public class EventKeyDetailGrid : EventKeyDetGrid
	{
		#region Constructors
		public EventKeyDetailGrid() : base()
		{
			this.ShowHeader          = true;
			m_XmlReferentialFileName = "Event/XML_Files/XMLReferentialEVENTKEYDET.xml";
			this.LoadReferential(m_XmlReferentialFileName);
			this.DataBinding += new System.EventHandler(OnDataBinding);
		}
		#endregion Constructors
		#region Events
		#region OnDataBinding
		override protected void OnDataBinding(object sender, System.EventArgs e)
		{
			base.OnDataBinding(sender,e);
			this.DataMember   = "EventKeyDet";
			this.RelationName = "EventKeyDet";
		}
		#endregion OnDataBinding
		#region OnItemDataBound
		override protected void OnItemDataBound(DataGridItemEventArgs e)
		{
			base.OnItemDataBound(e);
			if ((e.Item.ItemType == ListItemType.Item) && (null != m_InfoColumn))
			{
				DataRowView m_ParentDataRowView = ((HierarchicalPanel)this.Parent).ParentDataRowView;
				if (null != m_ParentDataRowView)
				{
					string familyProduct = m_ParentDataRowView["FAMILY"].ToString();
					string eventCode     = m_ParentDataRowView["EVENTCODE"].ToString();
					DataGridItem dgi     = (DataGridItem) e.Item;
					DataRowView  drv     = (DataRowView)dgi.DataItem;

					if (Cst.ProductFamily_IRD == familyProduct)
					{
						m_InfoColumn.Add(GetInfoColumns(drv,"RATE"));
						m_InfoColumn.Add(GetInfoColumns(drv,"STRIKE"));
						m_InfoColumn.Add(GetInfoColumns(drv,"DCF"));
						m_InfoColumn.Add(GetInfoColumns(drv,"DCFNUM"));
						m_InfoColumn.Add(GetInfoColumns(drv,"DCFDEN"));
						m_InfoColumn.Add(GetInfoColumns(drv,"TOTALOFDAY"));
						m_InfoColumn.Add(GetInfoColumns(drv,"TOTALOFYEAR"));
						m_InfoColumn.Add(GetInfoColumns(drv,"COMMENT"));

						m_InfoColumn.Add(GetInfoColumns(drv,"IDC_REF"));
						m_InfoColumn.Add(GetInfoColumns(drv,"NOTIONALAMOUNT"));
						m_InfoColumn.Add(GetInfoColumns(drv,"IDC1"));
						m_InfoColumn.Add(GetInfoColumns(drv,"IDC2"));
						m_InfoColumn.Add(GetInfoColumns(drv,"BASIS"));
						m_InfoColumn.Add(GetInfoColumns(drv,"DTFIXING"));
						m_InfoColumn.Add(GetInfoColumns(drv,"IDBC"));
						m_InfoColumn.Add(GetInfoColumns(drv,"COMMENT"));
					}
					else if (Cst.ProductFamily_FX == familyProduct)
					{
						if (EventCodeFunc.IsFxHasActivatedStatus(eventCode))
						{
							HierarchicalGrid hg = (HierarchicalGrid)this.Parent.NamingContainer.NamingContainer;
							m_InfoColumn.Add(hg.GetInfoColumn(m_ParentDataRowView,"IDSTTRIGGER"));
						}
						m_InfoColumn.Add(GetInfoColumns(drv,"FXTYPE"));
						m_InfoColumn.Add(GetInfoColumns(drv,"IDC_REF"));
						m_InfoColumn.Add(GetInfoColumns(drv,"IDC_BASE"));
						m_InfoColumn.Add(GetInfoColumns(drv,"IDC1"));
						m_InfoColumn.Add(GetInfoColumns(drv,"IDC2"));
						m_InfoColumn.Add(GetInfoColumns(drv,"BASIS"));
						m_InfoColumn.Add(GetInfoColumns(drv,"NOTIONALAMOUNT"));
						m_InfoColumn.Add(GetInfoColumns(drv,"RATE"));
						m_InfoColumn.Add(GetInfoColumns(drv,"SPOTRATE"));
						m_InfoColumn.Add(GetInfoColumns(drv,"FWDPOINTS"));
						m_InfoColumn.Add(GetInfoColumns(drv,"DTFIXING"));
						m_InfoColumn.Add(GetInfoColumns(drv,"IDBC"));
						m_InfoColumn.Add(GetInfoColumns(drv,"SETTLEMENTRATE"));
						m_InfoColumn.Add(GetInfoColumns(drv,"STRIKE"));
						m_InfoColumn.Add(GetInfoColumns(drv,"CONVERSIONRATE"));
						m_InfoColumn.Add(GetInfoColumns(drv,"TOTALPAYOUTAMOUNT"));
						m_InfoColumn.Add(GetInfoColumns(drv,"GAPRATE"));
						m_InfoColumn.Add(GetInfoColumns(drv,"PERIODPAYOUT"));
						m_InfoColumn.Add(GetInfoColumns(drv,"PCTPAYOUT"));
						m_InfoColumn.Add(GetInfoColumns(drv,"COMMENT"));
					}
					else
						this.ShowHeader = false;
				}
			}
		}
		#endregion OnItemDataBound
		#endregion Events
	}
	#endregion EventKeyDetailGrid
}
