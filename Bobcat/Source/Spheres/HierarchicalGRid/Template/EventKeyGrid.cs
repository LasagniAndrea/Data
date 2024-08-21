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
	#region PanelEventKey
	public class PanelEventKey : Panel,iChild
	{
		#region Variables
		public Panel          pnlEventKey;
		public EventKeyGrid   dgEventKey;
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
		public PanelEventKey() : base()
		{
			pnlEventKey = new Panel();
			dgEventKey  = new EventKeyGrid();
			this.ID    = "HG1";
			this.Controls.Add(pnlEventKey);
			this.Controls.Add(dgEventKey);

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
					string eventCode = m_ParentDataRowView["EVENTCODE"].ToString();
					string relToCode = m_ParentDataRowView["RELTOCODE"].ToString();
					IsChildExist = true;
					if (EventCodeFunc.IsTradeDate(eventCode))
					{
						this.dgEventKey.ShowHeader = true;
						this.dgEventKey.HideBoundColumn(true,
							new ArrayList(new string[5]{"EVENTCODE","RELTOCODE","DTSTARTADJ","DTENDADJ","FILLER"}));
					}
					else if (EventCodeFunc.IsProductDate(eventCode) || EventCodeFunc.IsStreamDate(eventCode) ||
						     EventCodeFunc.IsNonDeliverableDate(eventCode) || EventCodeFunc.IsFxSingleLegDate(eventCode))
					{
						this.dgEventKey.ShowHeader = true;
						this.dgEventKey.HideBoundColumn(true,
							new ArrayList(new string[14]{"EVENTCODE","RELTOCODE","DTSTARTADJ","DTENDADJ","VALORISATION","UNIT",
															"IDA_PAY","ac1_IDENTIFIER","IDB_PAY","bk1_IDENTIFIER",
															"IDA_REC","ac2_IDENTIFIER","IDB_REC","bk2_IDENTIFIER"}));
					}
					else if (RelToCodeFunc.IsInterestPayment(relToCode))
					{
						if (0 < m_ParentDataRowView.Row.GetChildRows("EventKey").Length)
						{
							this.dgEventKey.ShowHeader = true;
							this.dgEventKey.HideBoundColumn(true,
								new ArrayList(new string[14]{"EVENTCODE","RELTOCODE","DTSTARTADJ","DTENDADJ","VALORISATION","UNIT",
																"IDA_PAY","ac1_IDENTIFIER","IDB_PAY","bk1_IDENTIFIER",
																"IDA_REC","ac2_IDENTIFIER","IDB_REC","bk2_IDENTIFIER"}));
						}
						else if (0 < m_ParentDataRowView.Row.GetChildRows("Accrual").Length)
							IsChildExist = true;
						else
							IsChildExist = false;
					}
					else 
					{
						IsChildExist = true;
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
		}
		#endregion OnDataBinding
		#region OnPreRender
		protected override void OnPreRender(EventArgs e)
		{
			if (null != m_ParentDataRowView)
			{
				if (EventCodeFunc.IsReset(m_ParentDataRowView["EVENTCODE"].ToString()))
				{
					#region ScrollPanel for SelfAverage Events
					if (0 < this.dgEventKey.Items.Count)
					{
						this.Height                = Unit.Pixel(100);
						this.Style["overflow"]     = "auto"; 
					}
					#endregion ScrollPanel for SelfAverage Events
				}
			}
		}
		#endregion OnPreRender
		#endregion Events
	}
	#endregion PanelEventKey
	#region EventKeyGrid
	public class EventKeyGrid : HierarchicalGrid
	{
		#region Variables
		public string lastRelToCode;
		public string lastEventCode;
		public string[,] itemTitle;
		public string xmlReferentialFileName = "Event/XML_Files/XMLReferentialEVENTKEY.xml";
		#endregion Variables
		#region Constructors
		public EventKeyGrid() : base()
		{
			#region Grid properties
			this.ID                  = "hgEventKey";
			this.ShowHeader          = false;
			HeaderStyle.CssClass     = "DataGrid_AlternatingItemStyle";
			this.ContainerColumnName = new HierarchicalColumnInfo("Event","EventDates","Event",11,50);
			this.ContainerColumnName = new HierarchicalColumnInfo("EventKeyAsset","btnDetail","EventKeyAsset",-1,100);
			this.ContainerColumnName = new HierarchicalColumnInfo("EventKeyDet","butBrowser","EventKeyDet",-1,100);
			#endregion Grid properties

			#region Grid events
			this.TemplateSelection         += new HierarchicalGridTemplateSelectionEventHandler(OnTemplateSelection);
			this.DataBinding               += new System.EventHandler(OnDataBinding);
			#endregion Grid events

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
				this.SetToolTipInCell(e.Item,"EVENTCODE");
				this.SetToolTipInCell(e.Item,"RELTOCODE");

				itemTitle[0,e.Item.ItemIndex] = string.Empty;
				itemTitle[1,e.Item.ItemIndex] = "DataGrid_FooterStyle";

				DataGridItem dgi    = (DataGridItem) e.Item;
				DataRowView  drv    = (DataRowView)dgi.DataItem;

				string newEventCode = drv["EVENTCODE"].ToString();
				string newRelToCode = drv["RELTOCODE"].ToString();
				//
				if (EventCodeFunc.IsProductDate(newEventCode))
					itemTitle[0,e.Item.ItemIndex] = drv["DISPLAYNAME"].ToString() + "[" + drv["IDENTIFIER"].ToString() + "]";
				else if (EventCodeFunc.IsStreamDate(newEventCode) || EventCodeFunc.IsFxSingleLegDate(newEventCode))
					itemTitle[0,e.Item.ItemIndex] = Ressource.GetString(newEventCode) + " n°" + drv["STREAMNO"].ToString();
				else if (EventCodeFunc.IsNonDeliverableDate(newEventCode))
					itemTitle[0,e.Item.ItemIndex] = Ressource.GetString(newEventCode);
				else if (RelToCodeFunc.IsProvision(newRelToCode))
					itemTitle[0,e.Item.ItemIndex] = Ressource.GetString(newRelToCode);
				else if (EventCodeFunc.IsFxOption(newEventCode))
					itemTitle[0,e.Item.ItemIndex] = Ressource.GetString(newEventCode);
				else if (EventCodeFunc.IsAdditionalPayment(newEventCode) || EventCodeFunc.IsOtherPartyPayment(newEventCode))
					itemTitle[0,e.Item.ItemIndex] = Ressource.GetString(newEventCode);
				else if (EventCodeFunc.IsAbandon(newEventCode) && (newEventCode != lastEventCode))
					itemTitle[0,e.Item.ItemIndex] = Ressource.GetString(newEventCode);
				else if (EventCodeFunc.IsExercise(newEventCode) && (newEventCode != lastEventCode))
					itemTitle[0,e.Item.ItemIndex] = Ressource.GetString(newEventCode);
				else if (EventCodeFunc.IsBarrierRate(newEventCode) && (newEventCode != lastEventCode))
					itemTitle[0,e.Item.ItemIndex] = Ressource.GetString(newEventCode);
				else if (RelToCodeFunc.IsCashSettlement(newRelToCode) && (newRelToCode != lastRelToCode))
					itemTitle[0,e.Item.ItemIndex] = Ressource.GetString(newRelToCode);
				else if (EventCodeFunc.IsTriggerRate(newEventCode) && (newEventCode != lastEventCode))
					itemTitle[0,e.Item.ItemIndex] = Ressource.GetString(newEventCode);
				else if (EventCodeFunc.IsFxExerciseDates(newEventCode) && (newEventCode != lastEventCode))
					itemTitle[0,e.Item.ItemIndex] = Ressource.GetString(newEventCode);
				else if (EventCodeFunc.IsFxExerciseProcedure(newEventCode) && (newEventCode != lastEventCode))
					itemTitle[0,e.Item.ItemIndex] = Ressource.GetString(newEventCode);
				else if ((newEventCode != lastEventCode) || (newRelToCode != lastRelToCode))
				{
					#region Event-RelToCode regroup
					if (EventAndRelToCodeFunc.IsNominalPeriodVariation(newEventCode,newRelToCode) && (newRelToCode != lastRelToCode))
						itemTitle[0,e.Item.ItemIndex] = Ressource.GetString("VaryingNotional");
					else if (RelToCodeFunc.IsNominal(newRelToCode) && 
						(false == EventAndRelToCodeFunc.IsNominalPeriodVariation(newEventCode,newRelToCode)))
						itemTitle[0,e.Item.ItemIndex] = Ressource.GetString(newRelToCode);
					else if (RelToCodeFunc.IsInterestPayment(newRelToCode) && (newRelToCode != lastRelToCode))
						itemTitle[0,e.Item.ItemIndex] = Ressource.GetString("Interest");
					else if (RelToCodeFunc.IsPremium(newRelToCode) && (newRelToCode != lastRelToCode))
						itemTitle[0,e.Item.ItemIndex] = Ressource.GetString(newRelToCode);
					else if (RelToCodeFunc.IsPayout(newRelToCode) && (newRelToCode != lastRelToCode))
						itemTitle[0,e.Item.ItemIndex] = Ressource.GetString(newRelToCode);
					#endregion Event-RelToCode regroup
				}
				if (RelToCodeFunc.IsInterestPayment(newRelToCode))
					e.Item.CssClass = "Event_INT";
				lastEventCode = newEventCode;
				lastRelToCode = newRelToCode;
			}
		}
		#endregion OnItemDataBound
		#region OnDataBinding
		private void OnDataBinding(object sender, System.EventArgs e)
		{
			DataGridItem dgi  = (DataGridItem) this.BindingContainer;
			this.DataSource   = dgi.DataItem;
			this.DataMember   = "EventKey";
			this.RelationName = "EventKey";
			this.itemTitle    = new string[2,((DataView)this.DataSource).Count];
		}
		#endregion OnDataBinding
		#region OnPreRender
		protected override void OnPreRender(EventArgs e)
		{
			if (0 < this.Controls.Count)
			{
				Table tbl = (Table)this.Controls[0];
				DataGridItem dgi;
				int j=0;
				for (int i=0;i<itemTitle.GetLength(1);i++)
				{
					if (StrFunc.IsFilled(itemTitle[0,i]))
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
		private void OnTemplateSelection(object sender, HierarchicalGridTemplateSelectionEventArgs e)
		{
			switch(e.Row.Table.TableName)
			{
				case "Accrual":
					e.TemplateFilename = "PanelAccrual";
					e.RelationName     = "Accrual";
					break;
				case "EventKey":
					e.TemplateFilename = "PanelEventKey";
					e.RelationName     = "EventKey";
					break;
				case "Event":
					e.TemplateFilename = "PanelEvent";
					e.RelationName     = "Event";
					break;
				case "EventKeyDet":
					e.TemplateFilename = "PanelEventKeyDet";
					e.RelationName     = "EventKeyDet";
					break;
				case "EventKeyAsset":
					e.TemplateFilename = "PanelEventKeyAsset";
					e.RelationName     = "EventKeyAsset";
					break;
				default:
					throw new NotImplementedException("Unexpected child row in TemplateSelection event");
			}
		}
		#endregion OnTemplateSelection
		#endregion Events
		#region Methods
		#region AddItemTitle
		private DataGridItem AddItemTitle(int pItemIndex)
		{
			DataGridItem dgi = new DataGridItem(0,0,ListItemType.Item);
			dgi.CssClass     = itemTitle[1,pItemIndex];
			TableCell cell   = new TableCell();
			cell.ColumnSpan  = this.Columns.Count;
			cell.Text        = itemTitle[0,pItemIndex];
			dgi.Cells.Add(cell);
			return dgi;
		}
		#endregion AddItemTitle
		#endregion Methods
	}
	#endregion EventKeyGrid
}
