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
	#region PanelEvent
	public class PanelEvent : HierarchicalPanel
	{
		#region Variables
		public EventGrid      dgEvent;
		#endregion Variables
		#region Constructors
		public PanelEvent() : base()
		{
			dgEvent  = new EventGrid();
			this.Controls.Add(dgEvent);
		}
		#endregion Constructors
	}
	#endregion PanelEvent
	#region EventGrid
	public class EventGrid : HierarchicalGrid
	{
		#region Variables
		public string xmlReferentialFileName = "Event/XML_Files/XMLReferentialEVENT.xml";
		#endregion Variables
		#region Constructors
		public EventGrid() : base()
		{
			#region Grid properties
			this.ID                  = "hgEvent";
			this.GridLines           = GridLines.None;
			this.BackColor           = Color.Transparent;
			this.AlternatingItemStyle.BackColor = Color.Transparent;
			this.ItemStyle.BackColor = Color.Transparent;
			this.ShowHeader          = false;
			#endregion Grid properties
			#region Grid events
			this.TemplateSelection         += new HierarchicalGridTemplateSelectionEventHandler(OnTemplateSelection);
			this.DataBinding               += new System.EventHandler(OnDataBinding);
			#endregion Grid events
			this.LoadReferential(xmlReferentialFileName);
		}
		#endregion Constructors
		#region Events
		#region OnDataBinding
		private void OnDataBinding(object sender, System.EventArgs e)
		{
			DataGridItem dgi  = (DataGridItem) this.BindingContainer;
			this.DataSource   = dgi.DataItem;
			this.DataMember   = "Event";
			this.RelationName = "Event";
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
				this.SetToolTipInCell(e.Item,"DATECODE");
			}
		}
		#endregion OnItemDataBound

		#region OnTemplateSelection
		private void OnTemplateSelection(object sender, HierarchicalGridTemplateSelectionEventArgs e)
		{
			switch(e.Row.Table.TableName)
			{
				default:
					throw new NotImplementedException("Unexpected child row in TemplateSelection event");
			}
		}
		#endregion OnTemplateSelection
		#endregion Events
	}
	#endregion EventGrid
}
