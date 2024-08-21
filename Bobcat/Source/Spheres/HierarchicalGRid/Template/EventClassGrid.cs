#region using directives
using EFS.ACommon;
using EFS.Common;
using EFS.Common.Web;
using EFS.Referential;
using EfsML.Enum.Tools;
using System;
using System.Data;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Web.UI;
using System.Web.UI.WebControls;
#endregion using directives

namespace EFS
{
    #region PanelEventClass
    // EG 20180423 Analyse du code Correction [CA1405]
    [ComVisible(false)] 
	public class PanelEventClass : HierarchicalPanel
	{
		#region Variables
		public EventClassGrid dgEventClass;
		#endregion Variables
		#region Constructors
        public PanelEventClass() : this(null) { }
        public PanelEventClass(object pProduct): base()
        {
            dgEventClass = new EventClassGrid(pProduct);
			this.Controls.Add(dgEventClass);
		}
		#endregion Constructors
	}
	#endregion PanelEventClass
	#region EventClassGrid
    // EG 20180423 Analyse du code Correction [CA1405]
    [ComVisible(false)] 
	public class EventClassGrid : HierarchicalGrid
	{
		#region Variables
		private readonly ExtendEnum m_EventClassEnum;
		#endregion Variables
		#region Constructors
        public EventClassGrid() : this(null){}
        public EventClassGrid(object pProduct) : base("Sub")
		{
            m_Product = pProduct;
			#region Grid properties
			this.ID                             = "hgEventClass";
			this.GridLines                      = GridLines.None;
			this.BackColor                      = Color.Transparent;
			this.AlternatingItemStyle.BackColor = Color.Transparent;
			this.ItemStyle.BackColor            = Color.Transparent;
			this.ShowHeader                     = false;
			#endregion Grid properties
			#region Grid events
			this.TemplateSelection += new HierarchicalGridTemplateSelectionEventHandler(OnTemplateSelection);
			this.DataBinding       += new System.EventHandler(OnDataBinding);
			#endregion Grid events
			// FI 20240731 [XXXXX] Mise en commentaire => use DataEnabledEnum/DataEnabledEnumHelper
			//ExtendEnums extendEnums = ExtendEnumsTools.ListEnumsSchemes;
			//m_EventClassEnum        = extendEnums["EventClass"];
			m_EventClassEnum = DataEnabledEnumHelper.GetDataEnum(SessionTools.CS, "EventClass");

			string fileName = ReferentialTools.GetObjectXMLFile(Cst.ListType.Event, "EVENTCLASS");
			this.LoadReferential(fileName);
		}
		#endregion Constructors
		#region Events
		#region OnDataBinding
		private void OnDataBinding(object sender, System.EventArgs e)
		{
			DataGridItem dgi  = (DataGridItem) this.BindingContainer;
			this.DataSource   = dgi.DataItem;
			this.DataMember   = "EventClass";
			this.RelationName = "EventClass";
		}
		#endregion OnDataBinding
		#region OnItemDataBound
		// EG 20200724 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
		// EG 20200914 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc) Correction et compléments
		override protected void OnItemDataBound(DataGridItemEventArgs e)
		{
            string cssClass = "evtitle gray";
			base.OnItemDataBound(e);				

			if (e.Item.ItemType == ListItemType.Item || 
				e.Item.ItemType == ListItemType.AlternatingItem || 
				e.Item.ItemType == ListItemType.SelectedItem)
			{
                Button btn = this.SetButtonInCell(e.Item, "EVENTCLASS", EventClassFunc.SettlementMessage, m_EventClassEnum, 1, cssClass);
				DataRowView m_ParentDataRowView = ((HierarchicalPanel)this.Parent).ParentDataRowView;
                if (null == btn)
                    btn = this.SetButtonInCell(e.Item, "EVENTCLASS", EventClassFunc.DeliveryMessage, m_EventClassEnum, 1, cssClass);

                if (null != btn)
                {
                    if (null != m_ParentDataRowView)
                    {
                        int idE = Convert.ToInt32(m_ParentDataRowView["IDE"].ToString());
                        btn.Attributes.Add("onclick", "OpenEventSi(" + idE + ",'settlementInformations');return false;");
                    }
                }
                else
                    this.SetToolTipInCell(e.Item, "EVENTCLASS", m_EventClassEnum, 1);

                string Img_Payment = "fas fa-coins";
				string Img_Payment_ToolTip = "ExchangeEvent";
                TableCell cell = this.GetDataGridItemCell(e.Item, "EVENTCLASS", 1);
                if (null != cell)
                {
                    if (EventClassFunc.IsInvoiced(cell.Text))
                    {
						Img_Payment = "fas fa-file-invoice";
						Img_Payment_ToolTip = "InvoiceEvent";
                    }
                    else if (EventTypeFunc.IsQuantity(m_ParentDataRowView["EVENTTYPE"].ToString()))
                    {
						Img_Payment = "fas fa-truck";
						Img_Payment_ToolTip = "DeliveryEvent";
                    }

                }
                this.AddPanelImageInCell(e.Item, "ISPAYMENT", Img_Payment, true, 1, Img_Payment_ToolTip);
                this.AddImageDtEventForced(e.Item, 1);
                e.Item.CssClass = "SubEventTransparent";
			}
		}
		#endregion OnItemDataBound

		#region OnTemplateSelection
		private void OnTemplateSelection(object sender, HierarchicalGridTemplateSelectionEventArgs e)
		{
			switch(e.Row.Table.TableName)
			{
				default:
					throw new NotImplementedException("Unexpected child row in TemplateSelection eventClass");
			}
		}
		#endregion OnTemplateSelection
		#endregion Events
		#region Methods
		#region AddImageDtEventForced
		// EG 20200914 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc) Correction et compléments
		private void AddImageDtEventForced(DataGridItem pDataGridItem,int pCellOffset)
		{
			int idxDTEVENT              = -1;
			string colNameDTEVENT       = "DTEVENT";
			string colNameDTEVENTFORCED = "DTEVENTFORCED";
			int idxDTEVENTFORCED        = -1;
			for (int i=0;i<aBoundColumnName.Count;i++)
			{
				if (colNameDTEVENT == aBoundColumnName[i].ToString())
					idxDTEVENT = i;
				if (colNameDTEVENTFORCED == aBoundColumnName[i].ToString())
					idxDTEVENTFORCED = i;
			}
			if ((-1 < idxDTEVENTFORCED) && (-1 < idxDTEVENT))
			{
				TableCell cellDTEVENT = pDataGridItem.Cells[idxDTEVENT+pCellOffset];
				TableCell cellDTEVENTFORCED = pDataGridItem.Cells[idxDTEVENTFORCED+pCellOffset];
				if (cellDTEVENT.Text != cellDTEVENTFORCED.Text)
				{
					Label lblForced = new Label() {
						ToolTip = Ressource.GetString("GenerateDate") + cellDTEVENTFORCED.Text,
						CssClass = "fa-icon fas fa-exclamation red",
					};
					lblForced.Font.Bold = true;
					lblForced.Style.Add(HtmlTextWriterStyle.Cursor, "pointer");
					cellDTEVENTFORCED.Controls.Add(lblForced);
				}
				else
				{
					cellDTEVENTFORCED.Visible = false;
				}
			}
		}
		#endregion AddImageDtEventForced
		#endregion Methods
	}
	#endregion EventClassGrid
}
