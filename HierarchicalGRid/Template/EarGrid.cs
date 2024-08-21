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
	#region PanelEarDet
	public class PanelEarDet : Panel,iChild
	{
		#region Variables
		public Panel          pnlEarDet;
		public EarDetGrid     dgEarDet;
		protected DataRowView m_ParentDataRowView;
		private bool          m_IsChildExist;
		#endregion Variables
		#region Membres de iChild
		public bool IsChildExist
		{
			get {return m_IsChildExist;}
			set {m_IsChildExist = value;}
		}
		#endregion Membres de iChild
		#region Constructors
		public PanelEarDet() : base()
		{
			pnlEarDet = new Panel();
			dgEarDet  = new EarDetGrid();
			this.ID  = "HG1";
			this.Controls.Add(pnlEarDet);
			this.Controls.Add(dgEarDet);

			this.DataBinding += new System.EventHandler(OnDataBinding);
		}
		#endregion Constructors
		#region Events
		#region OnDataBinding
		private void OnDataBinding(object sender, System.EventArgs e)
		{
			DataGridItem dgi = (DataGridItem) this.BindingContainer;
			DataGrid     dg  = (DataGrid) dgi.Parent.BindingContainer;
			DataView     dv  = (DataView) dg.DataSource;
			if (dv.Count > dgi.DataSetIndex)
			{
				m_ParentDataRowView = (DataRowView)dv[dgi.DataSetIndex];
				if (null != m_ParentDataRowView)
				{
					IsChildExist = false;
					foreach (DataRelation rel in m_ParentDataRowView.DataView.Table.DataSet.Relations)
					{
						if (0 < m_ParentDataRowView.Row.GetChildRows(rel.RelationName).Length)
						{
							IsChildExist = true;
							break;
						}
					}
				}
			}
		}
		#endregion OnDataBinding
		#endregion Events
	}
	#endregion PanelEarDet
	#region PanelEarDay
	public class PanelEarDay : Panel,iChild
	{
		#region Variables
		public Panel          pnlEarDay;
		public EarDayGrid     dgEarDay;
		protected DataRowView m_ParentDataRowView;
		private bool          m_IsChildExist;
		#endregion Variables
		#region Membres de iChild
		public bool IsChildExist
		{
			get {return m_IsChildExist;}
			set {m_IsChildExist = value;}
		}
		#endregion Membres de iChild
		#region Constructors
		public PanelEarDay() : base()
		{
			pnlEarDay = new Panel();
			dgEarDay  = new EarDayGrid();
			this.ID  = "HGDay";
			this.Controls.Add(pnlEarDay);
			this.Controls.Add(dgEarDay);

			this.DataBinding += new System.EventHandler(OnDataBinding);
		}
		#endregion Constructors
		#region Events
		#region OnDataBinding
		private void OnDataBinding(object sender, System.EventArgs e)
		{
			DataGridItem dgi = (DataGridItem) this.BindingContainer;
			DataGrid     dg  = (DataGrid) dgi.Parent.BindingContainer;
			DataView     dv  = (DataView) dg.DataSource;
			if (dv.Count > dgi.DataSetIndex)
			{
				m_ParentDataRowView = (DataRowView)dv[dgi.DataSetIndex];
				if (null != m_ParentDataRowView)
				{
					IsChildExist = false;
					foreach (DataRelation rel in m_ParentDataRowView.DataView.Table.DataSet.Relations)
					{
						if (0 < m_ParentDataRowView.Row.GetChildRows(rel.RelationName).Length)
						{
							IsChildExist = true;
							break;
						}
					}
				}
			}
		}
		#endregion OnDataBinding
		#endregion Events
	}
	#endregion PanelEarDay
	#region PanelEarCommon
	public class PanelEarCommon : Panel,iChild
	{
		#region Variables
		public Panel          pnlEarCommon;
		public EarCommonGrid  dgEarCommon;
		protected DataRowView m_ParentDataRowView;
		private bool          m_IsChildExist;
		#endregion Variables
		#region Membres de iChild
		public bool IsChildExist
		{
			get {return m_IsChildExist;}
			set {m_IsChildExist = value;}
		}
		#endregion Membres de iChild
		#region Constructors
		public PanelEarCommon() : base()
		{
			pnlEarCommon = new Panel();
			dgEarCommon  = new EarCommonGrid();
			this.ID  = "HGCommon";
			this.Controls.Add(pnlEarCommon);
			this.Controls.Add(dgEarCommon);

			this.DataBinding += new System.EventHandler(OnDataBinding);
		}
		#endregion Constructors
		#region Events
		#region OnDataBinding
		private void OnDataBinding(object sender, System.EventArgs e)
		{
			DataGridItem dgi = (DataGridItem) this.BindingContainer;
			DataGrid     dg  = (DataGrid) dgi.Parent.BindingContainer;
			DataView     dv  = (DataView) dg.DataSource;
			if (dv.Count > dgi.DataSetIndex)
			{
				m_ParentDataRowView = (DataRowView)dv[dgi.DataSetIndex];
				if (null != m_ParentDataRowView)
				{
					IsChildExist = false;
					foreach (DataRelation rel in m_ParentDataRowView.DataView.Table.DataSet.Relations)
					{
						if (0 < m_ParentDataRowView.Row.GetChildRows(rel.RelationName).Length)
						{
							IsChildExist = true;
							break;
						}
					}
				}
			}
		}
		#endregion OnDataBinding
		#endregion Events
	}
	#endregion PanelEarDay
	#region PanelEarDayAmount
	public class PanelEarDayAmount : HierarchicalPanel
	{
		#region Variables
		public EarDayAmountGrid dgEarDayAmount;
		#endregion Variables
		#region Constructors
		public PanelEarDayAmount() : base()
		{
			dgEarDayAmount = new EarDayAmountGrid();
			this.Controls.Add(dgEarDayAmount);
		}
		#endregion Constructors
		#region OnPreRender
		protected override void OnPreRender(EventArgs e)
		{
			if (2 < this.dgEarDayAmount.Items.Count)
			{
				this.Height            = Unit.Pixel(55);
				this.Style["overflow"] = "auto"; 
			}
		}
		#endregion OnPreRender
	}
	#endregion PanelEarDayAmount
	#region PanelEarCommonAmount
	public class PanelEarCommonAmount : HierarchicalPanel
	{
		#region Variables
		public EarCommonAmountGrid dgEarCommonAmount;
		#endregion Variables
		#region Constructors
		public PanelEarCommonAmount() : base()
		{
			dgEarCommonAmount = new EarCommonAmountGrid();
			this.Controls.Add(dgEarCommonAmount);
		}
		#endregion Constructors
		#region OnPreRender
		protected override void OnPreRender(EventArgs e)
		{
			if (2 < this.dgEarCommonAmount.Items.Count)
			{
				this.Height            = Unit.Pixel(55);
				this.Style["overflow"] = "auto"; 
			}
		}
		#endregion OnPreRender
	}
	#endregion PanelEarCommonAmount


	#region EarDetGrid
	public class EarDetGrid : HierarchicalGrid
	{
		#region Variables
		protected int       m_LastInstrumentNo = -1;
		protected int       m_LastStreamNo     = -1;
		protected string[,] m_ItemTitle;
		public  string xmlReferentialFileName = "Ear/XML_Files/XMLReferentialEARDET.xml";
		private ExtendEnum m_EventCodeEnum;
		private ExtendEnum m_EventTypeEnum;
		#endregion Variables
		#region Constructors
		public EarDetGrid() : base()
		{
			#region Grid properties
			this.ID                  = "hgEar";
			this.ShowHeader          = false;
			HeaderStyle.CssClass     = "DataGrid_AlternatingItemStyle";
			#endregion Grid properties

			#region Grid events
			this.TemplateSelection += new HierarchicalGridTemplateSelectionEventHandler(OnTemplateSelection);
			this.DataBinding       += new System.EventHandler(OnDataBinding);
			#endregion Grid events

			ExtendEnums extendEnums = ExtendEnumsTools.ListEnumsSchemes;
			m_EventCodeEnum         = extendEnums["EventCode"];
			m_EventTypeEnum         = extendEnums["EventType"];

			this.LoadReferential(xmlReferentialFileName);
		}
		#endregion Constructors
		#region Events
		#region OnItemDataBound
		override protected void OnItemDataBound(DataGridItemEventArgs e)
		{
			base.OnItemDataBound(e);		
			if (e.Item.ItemType == ListItemType.Item || 
				e.Item.ItemType == ListItemType.AlternatingItem || 
				e.Item.ItemType == ListItemType.SelectedItem)
			{

				m_ItemTitle[0,e.Item.ItemIndex] = string.Empty;
				m_ItemTitle[1,e.Item.ItemIndex] = "Ear_Title";

				DataGridItem dgi = (DataGridItem) e.Item;
				DataRowView  drv = (DataRowView)dgi.DataItem;

				int newInstrumentNo        = Convert.ToInt32(drv["INSTRUMENTNO"]);
				int newStreamNo            = Convert.ToInt32(drv["STREAMNO"]);
				bool isInstrumentNoChanged = (newInstrumentNo != m_LastInstrumentNo);
				bool isStreamNoChanged     = (newStreamNo != m_LastStreamNo);

				if (isInstrumentNoChanged || isStreamNoChanged)
				{
					string newEventCode          = drv["EVENTCODE"].ToString();
					string newEventType          = drv["EVENTTYPE"].ToString();
					string newEventCode_Resource = string.Empty;
					string newEventType_Resource = string.Empty;
					ExtendEnumValue extendEnumValue = m_EventCodeEnum.GetExtendEnumValueByValue(newEventCode);
					if (null != extendEnumValue)
						newEventCode_Resource = extendEnumValue.Documentation;
					extendEnumValue = m_EventTypeEnum.GetExtendEnumValueByValue(newEventType);
					if (null != extendEnumValue)
						newEventType_Resource = extendEnumValue.Documentation;

					if (0 == newInstrumentNo)
						m_ItemTitle[0,e.Item.ItemIndex] = newEventCode_Resource;
					else if (0 == newStreamNo)
						m_ItemTitle[0,e.Item.ItemIndex] = drv["INSTR_DISPLAYNAME"].ToString()+"["+drv["INSTR_IDENTIFIER"].ToString()+"]";
					else
						m_ItemTitle[0,e.Item.ItemIndex] = newEventCode_Resource + " - " + newEventType_Resource;
				}
				m_LastInstrumentNo = newInstrumentNo;
				m_LastStreamNo     = newStreamNo;
			}
		}
		#endregion OnItemDataBound
		#region OnDataBinding
		private void OnDataBinding(object sender, System.EventArgs e)
		{
			DataGridItem dgi  = (DataGridItem) this.BindingContainer;
			this.DataSource   = dgi.DataItem;
			((DataView)this.DataSource).Sort = ((DataView)dgi.DataItem).Table.DefaultView.Sort;
			this.DataMember   = "EarDay";
			this.RelationName = "EarDay";
			this.m_ItemTitle  = new string[2,((DataView)this.DataSource).Count];
		}
		#endregion OnDataBinding
		#region OnPreRender
		protected override void OnPreRender(EventArgs e)
		{
			if (0 < this.Controls.Count)
			{
				Table tbl = (Table)this.Controls[0];
				for (int i=0;i<m_ItemTitle.GetLength(1);i++)
				{
					if (StrFunc.IsFilled(m_ItemTitle[0,i]))
					{
						((TableRow)tbl.Controls[i+1]).CssClass           = m_ItemTitle[1,i];
						((TableCell)tbl.Controls[i+1].Controls[0]).Width = Unit.Percentage(100);
						tbl.Controls[i+1].Controls[0].Controls.Add(new LiteralControl(m_ItemTitle[0,i]));
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
				case "EarDay":
					e.TemplateFilename = "PanelEarDay";
					e.RelationName     = "EarDay";
					break;
				case "EarCommon":
					e.TemplateFilename = "PanelEarCommon";
					e.RelationName     = "EarCommon";
					break;
				default:
					throw new NotImplementedException("Unexpected child row in TemplateSelection ear");
			}
		}
		#endregion OnTemplateSelection
		#endregion Events
	}
	#endregion EarDetGrid
	#region EarDayCommonGrid
	public class EarDayCommonGrid : HierarchicalGrid
	{
		#region Members
		protected int       m_LastInstrumentNo = -1;
		protected int       m_LastStreamNo     = -1;
		protected string[,] m_ItemTitle;
		protected string    m_XmlReferentialFileName;
		protected string    m_ChildName;
		protected string    m_ParentName;
		protected ArrayList m_InfoColumn;

		private ExtendEnum m_EventCodeEnum;
		private ExtendEnum m_EventTypeEnum;
		private ExtendEnum m_EventClassEnum;
		#endregion Members
		#region Constructors
		public EarDayCommonGrid(string pXmlReferentialFileName,string pChildName) : base()
		{
			m_XmlReferentialFileName = pXmlReferentialFileName;
			m_ChildName              = pChildName;
			#region Grid properties
			this.ShowHeader           = true;
			this.HeaderStyle.CssClass = "DataGrid_AlternatingItemStyle";
			this.ContainerColumnName  = new HierarchicalColumnInfo(pChildName,pChildName,pChildName,-1);
			#endregion Grid properties
			ExtendEnums extendEnums = ExtendEnumsTools.ListEnumsSchemes;
			m_EventCodeEnum         = extendEnums["EventCode"];
			m_EventTypeEnum         = extendEnums["EventType"];
			m_EventClassEnum        = extendEnums["EventClass"];
			this.LoadReferential(m_XmlReferentialFileName);
			this.TemplateSelection += new HierarchicalGridTemplateSelectionEventHandler(OnTemplateSelection);
			this.DataBinding       += new System.EventHandler(OnDataBinding);
		}
		#endregion Constructors
		#region Events
		#region OnDataBinding
		protected void OnDataBinding(object sender, System.EventArgs e)
		{
			DataGridItem dgi  = (DataGridItem) this.BindingContainer;
			this.DataSource   = dgi.DataItem;
			((DataView)this.DataSource).Sort = ((DataView)dgi.DataItem).Table.DefaultView.Sort;
			this.DataMember   = m_ChildName;
			this.RelationName = m_ChildName;
			this.m_ItemTitle  = new string[2,((DataView)this.DataSource).Count];			

			DataGrid     dg  = (DataGrid) dgi.Parent.BindingContainer;
			DataView     dv  = (DataView) dg.DataSource;
			m_ParentName     = dv.Table.TableName;

		}
		#endregion OnDataBinding
		#region OnItemDataBound
		override protected void OnItemDataBound(DataGridItemEventArgs e)
		{
			base.OnItemDataBound(e);
			if (e.Item.ItemType == ListItemType.Item || 
				e.Item.ItemType == ListItemType.AlternatingItem || 
				e.Item.ItemType == ListItemType.SelectedItem)
			{
				this.SetToolTipInCell(e.Item,"EVENTCODE",m_EventCodeEnum,1);
				this.SetToolTipInCell(e.Item,"EVENTTYPE",m_EventTypeEnum,1);
				this.SetToolTipInCell(e.Item,"EVENTCLASS",m_EventClassEnum,1);

				m_ItemTitle[0,e.Item.ItemIndex] = string.Empty;
				m_ItemTitle[1,e.Item.ItemIndex] = "Event_Title";

				DataGridItem dgi = (DataGridItem) e.Item;
				DataRowView  drv = (DataRowView)dgi.DataItem;

				int newInstrumentNo        = Convert.ToInt32(drv["INSTRUMENTNO"]);
				int newStreamNo            = Convert.ToInt32(drv["STREAMNO"]);

				if ("Ear" == m_ParentName)
				{
					bool isInstrumentNoChanged = (newInstrumentNo != m_LastInstrumentNo);
					bool isStreamNoChanged     = (newStreamNo != m_LastStreamNo);

					if (isInstrumentNoChanged || isStreamNoChanged)
					{
						string newEventCode          = drv["EVENTCODELEVEL"].ToString();
						string newEventType          = drv["EVENTTYPELEVEL"].ToString();
						string newEventCode_Resource = string.Empty;
						string newEventType_Resource = string.Empty;
						ExtendEnumValue extendEnumValue = m_EventCodeEnum.GetExtendEnumValueByValue(newEventCode);
						if (null != extendEnumValue)
							newEventCode_Resource = extendEnumValue.Documentation;
						extendEnumValue = m_EventTypeEnum.GetExtendEnumValueByValue(newEventType);
						if (null != extendEnumValue)
							newEventType_Resource = extendEnumValue.Documentation;

						if (0 == newInstrumentNo)
							m_ItemTitle[0,e.Item.ItemIndex] = newEventCode_Resource;
						else if (0 == newStreamNo)
							m_ItemTitle[0,e.Item.ItemIndex] = drv["INSTR_DISPLAYNAME"].ToString()+"["+drv["INSTR_IDENTIFIER"].ToString()+"]";
						else
							m_ItemTitle[0,e.Item.ItemIndex] = newEventCode_Resource + " - " + newEventType_Resource;
					}
				}
				m_LastInstrumentNo = newInstrumentNo;
				m_LastStreamNo     = newStreamNo;

				if (m_ChildName == "EarDayAmount")
					e.Item.CssClass = "Ear_Day";
				else if (m_ChildName == "EarCommonAmount")
					e.Item.CssClass = "Ear_Common";
			}
		}
		#endregion OnItemDataBound
		#region OnPreRender
		protected override void OnPreRender(EventArgs e)
		{
			if (0 < this.Controls.Count)
			{
				Table tbl = (Table)this.Controls[0];
				DataGridItem dgi;
				int j=0;
				for (int i=0;i<m_ItemTitle.GetLength(1);i++)
				{
					if (StrFunc.IsFilled(m_ItemTitle[0,i]))
					{
						dgi = AddItemTitle(i);
						tbl.Controls.AddAt(i + j + 1,dgi);
						j++;
					}
				}
			}
			base.OnPreRender(e);
		}
		#endregion OnPreRender
		#region OnTemplateSelection
		protected void OnTemplateSelection(object sender, HierarchicalGridTemplateSelectionEventArgs e)
		{
			switch(e.Row.Table.TableName)
			{
				case "EarDayAmount":
				case "EarCommonAmount":
					e.TemplateFilename = "Panel" + m_ChildName;
					e.RelationName     = m_ChildName;
					break;
				default:
					throw new NotImplementedException("Unexpected child row in TemplateSelection ear");
			}

		}
		#endregion OnTemplateSelection
		#endregion Events
		#region Methods
		#region AddItemTitle
		private DataGridItem AddItemTitle(int pItemIndex)
		{
			DataGridItem dgi = new DataGridItem(0,0,ListItemType.Item);
			dgi.CssClass     = m_ItemTitle[1,pItemIndex];
			TableCell cell   = new TableCell();
			cell.ColumnSpan  = this.Columns.Count;
			cell.Text        = m_ItemTitle[0,pItemIndex];
			dgi.Cells.Add(cell);
			return dgi;
		}
		#endregion AddItemTitle
		#endregion Methods
	}
	#endregion EarAmountGrid
	#region EarDayGrid
	public class EarDayGrid : EarDayCommonGrid
	{
		#region Constructors
		public EarDayGrid() : base("Ear/XML_Files/XMLReferentialEARDAY.xml","EarDayAmount")
		{
			this.ID = "hgEarDay";
		}
		#endregion Constructors
	}
	#endregion EarDayGrid
	#region EarCommonGrid
	public class EarCommonGrid : EarDayCommonGrid
	{
		#region Constructors
		public EarCommonGrid() : base("Ear/XML_Files/XMLReferentialEARCOMMON.xml","EarCommonAmount")
		{
			this.ID = "hgEarCommon";
		}
		#endregion Constructors
	}
	#endregion EarCommonGrid
	#region EarAmountGrid
	public class EarAmountGrid : HierarchicalGrid
	{
		#region Variables
		protected string    m_XmlReferentialFileName;
		protected ArrayList m_InfoColumn;
		#endregion Variables
		#region Constructors
		public EarAmountGrid(string pXmlReferentialFileName) : base()
		{
			m_XmlReferentialFileName            = pXmlReferentialFileName;
			#region Grid properties
			this.HeaderStyle.CssClass           = "DataGrid_AlternatingItemStyle";
			this.ID                             = "hgEarAmount";
			this.GridLines                      = GridLines.Vertical;
			this.CellPadding                    = 0;
			this.CellSpacing                    = 0;
			this.BackColor                      = Color.Transparent;
			this.AlternatingItemStyle.BackColor = Color.Transparent;
			this.ItemStyle.BackColor            = Color.Transparent;
			this.ShowHeader                     = true;
			#endregion Grid properties
			this.LoadReferential(m_XmlReferentialFileName);
			this.DataBinding += new System.EventHandler(OnDataBinding);
		}
		#endregion Constructors
		#region Events
		#region OnDataBinding
		protected virtual void OnDataBinding(object sender, System.EventArgs e)
		{
			DataGridItem dgi  = (DataGridItem) this.BindingContainer;
			this.DataSource   = dgi.DataItem;
		}
		#endregion OnDataBinding
		#region OnPreRender
		protected override void OnPreRender(EventArgs e)
		{
			if (0 < this.Controls.Count)
			{
				Table tbl    = (Table)this.Controls[0];
				Control ctrl = tbl.NamingContainer.Parent;
				if (ctrl.GetType().BaseType.Equals(typeof(HierarchicalPanel)))
				{
					if (null != ((HierarchicalPanel) ctrl).Style["overflow"])
					{
						TableRow tr = (TableRow) tbl.Controls[0];
						tr.Attributes.Add("style","position:relative;top:expression(this.offsetParent.scrollTop);");
					}
				}
			}
			base.OnPreRender(e);
		}
		#endregion OnPreRender

		#endregion Events
	}
	#endregion EarAmountGrid
	#region EarDayAmountGrid
	public class EarDayAmountGrid : EarAmountGrid
	{
		#region Constructors
		public EarDayAmountGrid() : base("Ear/XML_Files/XMLReferentialEARDAYAMOUNT.xml"){}
		#endregion Constructors
		#region Events
		#region OnDataBinding
		override protected void OnDataBinding(object sender, System.EventArgs e)
		{
			base.OnDataBinding(sender,e);
			this.DataMember   = "EarDayAmount";
			this.RelationName = "EarDayAmount";
		}
		#endregion OnDataBinding
		#region OnItemDataBound
		override protected void OnItemDataBound(DataGridItemEventArgs e)
		{
			base.OnItemDataBound(e);
		}
		#endregion OnItemDataBound
		#endregion Events
	}
	#endregion EarDayAmountGrid
	#region EarCommonAmountGrid
	public class EarCommonAmountGrid : EarAmountGrid
	{
		#region Constructors
		public EarCommonAmountGrid() : base("Ear/XML_Files/XMLReferentialEARCOMMONAMOUNT.xml"){}
		#endregion Constructors
		#region Events
		#region OnDataBinding
		override protected void OnDataBinding(object sender, System.EventArgs e)
		{
			base.OnDataBinding(sender,e);
			this.DataMember   = "EarCommonAmount";
			this.RelationName = "EarCommonAmount";
		}
		#endregion OnDataBinding
		#region OnItemDataBound
		override protected void OnItemDataBound(DataGridItemEventArgs e)
		{
			base.OnItemDataBound(e);
		}
		#endregion OnItemDataBound
		#endregion Events
	}
	#endregion EarCommonAmountGrid

}
