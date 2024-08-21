#region using directives
using EFS.ACommon;
using EFS.Common;
using EFS.Common.Web;
using EFS.Referential;
using EfsML.Business;
using EfsML.Enum;
using EfsML.Enum.Tools;
using System;
using System.Collections;
using System.Data;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Web.UI;
using System.Web.UI.WebControls;
#endregion using directives

namespace EFS
{
    #region PanelEvent
    // EG 20180423 Analyse du code Correction [CA1405]
    [ComVisible(false)] 
	public class PanelEvent : Panel, IChild
	{
		#region Members
		public Panel          pnlEvent;
		public EventGrid      dgEvent;
		protected DataRowView m_ParentDataRowView;
		private bool          m_IsChildExist;
		#endregion Members
		#region Accessors
		public string EventCode {get {return m_ParentDataRowView["EVENTCODE"].ToString();}}
		public string EventType {get {return m_ParentDataRowView["EVENTTYPE"].ToString();}}
		public string Family    {get {return m_ParentDataRowView["FAMILY"].ToString();}}
		#endregion Accessors

		#region Membres de IChild
		public bool IsChildExist
		{
			get {return m_IsChildExist;}
			set {m_IsChildExist = value;}
		}
		#endregion Membres de iChild
		#region Constructors
        public PanelEvent() : this(null) { }
        // EG 20200724 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        public PanelEvent(object pProduct): base()
        {
            dgEvent = new EventGrid(pProduct);
            this.ID = "divDtg";
            this.Controls.Add(dgEvent);

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
                    string eventType = m_ParentDataRowView["EVENTTYPE"].ToString();
                    IsChildExist = true;
                    if (EventCodeFunc.IsTrade(eventCode))
                    {
                        this.dgEvent.ShowHeader = true;
                        this.dgEvent.HideBoundColumn(true,
                            new ArrayList(new string[6] { "IDE", "EVENTCODE", "EVENTTYPE", "DTSTARTADJ", "DTENDADJ", "FILLER" }));
                    }
                    else if (EventCodeFunc.IsProduct(eventCode) || EventCodeFunc.IsStreamGroup(eventCode))
                    {
                        this.dgEvent.ShowHeader = true;
                        this.dgEvent.HideBoundColumn(true,
                            new ArrayList(new string[15]{"IDE","EVENTCODE","EVENTTYPE","DTSTARTADJ","DTENDADJ","VALORISATION","UNIT",
															"IDA_PAY","ac1_IDENTIFIER","IDB_PAY","bk1_IDENTIFIER",
															"IDA_REC","ac2_IDENTIFIER","IDB_REC","bk2_IDENTIFIER"}));
                    }
                    else if (EventTypeFunc.IsInterest(eventType))
                    {
                        if (0 < m_ParentDataRowView.Row.GetChildRows("Event").Length)
                        {
                            this.dgEvent.ShowHeader = true;
                            this.dgEvent.HideBoundColumn(true,
                                new ArrayList(new string[15]{"IDE","EVENTCODE","EVENTTYPE","DTSTARTADJ","DTENDADJ","VALORISATION","UNIT",
																"IDA_PAY","ac1_IDENTIFIER","IDB_PAY","bk1_IDENTIFIER",
																"IDA_REC","ac2_IDENTIFIER","IDB_REC","bk2_IDENTIFIER"}));
                        }
                        else
                            IsChildExist = false;
                    }
                    else
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
					if (1 < this.dgEvent.Items.Count)
					{
						this.Height            = Unit.Pixel(500);
						this.Style[HtmlTextWriterStyle.Overflow] = "auto"; 
					}
					#endregion ScrollPanel for SelfAverage Events
				}
                else if ((EventCodeFunc.IsLinkedProductClosing(m_ParentDataRowView["EVENTCODE"].ToString()) &&
                          EventTypeFunc.IsAmounts(m_ParentDataRowView["EVENTTYPE"].ToString()))) 
                {
                    // EG 20100412 Add Overflow for LPC amounts and OFS list (ETD)
                    #region ScrollPanel for LinkedProductClosing and OffSetting  Events
                    if (1 < this.dgEvent.Items.Count)
                    {
                        this.Height = Unit.Pixel(600);
                        this.Style[HtmlTextWriterStyle.Overflow] = "auto";
                    }
                    #endregion ScrollPanel for LinkedProductClosing and OffSetting  Events
                }
                else if (EventCodeFunc.IsOffsetting(m_ParentDataRowView["EVENTCODE"].ToString()))
                {
                    // EG 20110311 Add Overflow for OFS list
                    #region ScrollPanel for OffSetting  Events
                    if (1 < this.dgEvent.Items.Count)
                    {
                        if (dgEvent.ProductBase.IsDebtSecurityTransaction)
                            this.Height = Unit.Pixel(100);
                        else
                            this.Height = Unit.Pixel(160);
                        this.Style[HtmlTextWriterStyle.Overflow] = "auto";
                    }
                    #endregion ScrollPanel for OffSetting  Events
                }
                else if (EventTypeFunc.IsCashAvailable(m_ParentDataRowView["EVENTTYPE"].ToString()))
                {
                    // EG 20110902 Add Overflow for Cash Available events
                    #region ScrollPanel for CashAvailable Event
                    if (1 < this.dgEvent.Items.Count)
                    {
                        this.Height = Unit.Pixel(600);
                        this.Style[HtmlTextWriterStyle.Overflow] = "auto";
                    }
                    #endregion ScrollPanel for CashAvailable Event
                }
                else if (EventCodeFunc.IsDebtSecurityStream(m_ParentDataRowView["EVENTCODE"].ToString()))
                {
                    // EG 20140925 New
                    #region ScrollPanel for Stream event on DebtSecurity /DebtSecurityTransaction
                    if ((1 < this.dgEvent.Items.Count) && (dgEvent.ProductBase.IsDebtSecurity || dgEvent.ProductBase.IsDebtSecurityTransaction))
                    {
                        this.Height = Unit.Pixel(400);
                        this.Style[HtmlTextWriterStyle.Overflow] = "auto";
                    }
                    #endregion ScrollPanel for Stream event on DebtSecurityTransaction
                }
                else if ((EventCodeFunc.IsInterestLeg(m_ParentDataRowView["EVENTCODE"].ToString()) &&
                    EventTypeFunc.IsFloatingRate(m_ParentDataRowView["EVENTTYPE"].ToString())))
                {
                    // EG 20150320 (POC] 
                    #region ScrollPanel for InterestLeg and FloatingRate  Events
                    if (1 < this.dgEvent.Items.Count)
                    {
                        this.Height = Unit.Pixel(600);
                        this.Style[HtmlTextWriterStyle.Overflow] = "auto";
                    }
                    #endregion ScrollPanel for InterestLeg and FloatingRate  Events
                }

            }
		}
		#endregion OnPreRender
		#endregion Events
	}
    #endregion PanelEvent
    #region EventGrid
    // EG 20180423 Analyse du code Correction [CA1405]
    // EG 20201117 [25556] Corrections diverses - CSS
    [ComVisible(false)] 
	public class EventGrid : HierarchicalGrid
	{
		#region Members
		public  string             lastEventType;
		public  string             lastEventCode;
		public  string[,]          itemTitle;
		private readonly ExtendEnum         m_EventCodeEnum;
		private readonly ExtendEnum m_EventTypeEnum;
		private readonly ExtendEnum m_EventClassEnum;
		private bool               m_IsWithComplementaryInfo;
		private readonly Hashtable m_CurrencyInfos;
		#endregion Members
		#region Constructors
        public EventGrid(): this(null){}
        public EventGrid(object pProduct): base()
        {
            m_Product = pProduct;
            #region Grid properties
            this.ID = "hgEvent";
            this.ShowHeader = false;
            HeaderStyle.CssClass = "DataGrid_HeaderStyle";
            this.ContainerColumnName = new HierarchicalColumnInfo("EventClass", "EventDates", "EventClass", 10, 50);
            #endregion Grid properties

            #region Grid events
            this.TemplateSelection += new HierarchicalGridTemplateSelectionEventHandler(OnTemplateSelection);
            this.DataBinding += new System.EventHandler(OnDataBinding);
            #endregion Grid events

            /* FI 20240731 [XXXXX] Mise en commentaire => use DataEnabledEnum/DataEnabledEnumHelper
            ExtendEnums extendEnums = ExtendEnumsTools.ListEnumsSchemes;
            m_EventCodeEnum = extendEnums["EventCode"];
            m_EventTypeEnum = extendEnums["EventType"];
            m_EventClassEnum = extendEnums["EventClass"];
            */

            m_EventCodeEnum = DataEnabledEnumHelper.GetDataEnum(SessionTools.CS, "EventCode");
            m_EventTypeEnum = DataEnabledEnumHelper.GetDataEnum(SessionTools.CS, "EventType");
            m_EventClassEnum = DataEnabledEnumHelper.GetDataEnum(SessionTools.CS, "EventClass");

            m_CurrencyInfos = new Hashtable();
            string fileName = ReferentialTools.GetObjectXMLFile(Cst.ListType.Event, "VW_EVENT");
            this.LoadReferential(fileName);
        }
        #endregion Constructors
        #region Events
        #region OnItemDataBound
        // EG 20200930 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc) Correction et suppression de codes inutiles
        override protected void OnItemDataBound(DataGridItemEventArgs e)
		{
			base.OnItemDataBound(e);				

			if (e.Item.ItemType == ListItemType.Item || 
				e.Item.ItemType == ListItemType.AlternatingItem || 
				e.Item.ItemType == ListItemType.SelectedItem)
			{
				DataGridItem dgi = (DataGridItem) e.Item;
				this.SetToolTipInCell(dgi, "EVENTCODE", m_EventCodeEnum, 2);
				this.SetToolTipInCell(dgi, "EVENTTYPE", m_EventTypeEnum, 2);

				//20071114 PL
				this.SetColorInCell(dgi, "IDE", "DarkGray", 1);
				this.SetColorInCell(dgi, "IDA_PAY", "Payer", 2);
                this.SetColorInCell(dgi, "IDB_PAY", "Payer", 2);
				this.SetColorInCell(dgi, "ac1_IDENTIFIER", "Payer", 2);
				this.SetColorInCell(dgi, "bk1_IDENTIFIER", "Payer", 2);
				this.SetColorInCell(dgi, "IDA_REC", "Receiver", 2);
				this.SetColorInCell(dgi, "IDB_REC", "Receiver", 2);
				this.SetColorInCell(dgi, "ac2_IDENTIFIER", "Receiver", 2);
				this.SetColorInCell(dgi, "bk2_IDENTIFIER", "Receiver", 2);

				itemTitle[0,dgi.ItemIndex] = string.Empty;
                itemTitle[1, dgi.ItemIndex] = "subevtitle black";

                SetButtonZoom(dgi);
				SetEventActionAttribute(dgi);
				SetRoundingValorisation(dgi);
				SetCssClass(dgi);
				SetTitleBeforeItem(dgi, lastEventCode, lastEventType);

				DataRowView drv = (DataRowView)dgi.DataItem;
				lastEventCode	= drv["EVENTCODE"].ToString();
				lastEventType	= drv["EVENTTYPE"].ToString();

                //#region Event is dead
                //bool isItemLocked		= (Cst.StatusActivation.LOCKED.ToString() == drv["IDSTACTIVATION"].ToString());
                //bool isItemDeactivated	= (Cst.StatusActivation.DEACTIV.ToString() == drv["IDSTACTIVATION"].ToString());
                //dgi.Font.Italic			= isItemLocked;
                //dgi.Font.Strikeout		= isItemDeactivated;
                //#endregion Event is dead

			}
		}
		#endregion OnItemDataBound
		#region OnDataBinding
		private void OnDataBinding(object sender, System.EventArgs e)
		{
            if (this.BindingContainer.GetType().Equals(typeof(DataGridItem)))
            {
                DataGridItem dgi = (DataGridItem)this.BindingContainer;
                this.DataSource = dgi.DataItem;
                this.DataMember = "Event";
                this.RelationName = "Event";
            }
            else
            {
                this.ShowHeader = true;
                this.HideBoundColumn(true, new ArrayList(new string[15]{"IDE","EVENTCODE","EVENTTYPE","DTSTARTADJ","DTENDADJ","VALORISATION","UNIT",
                                                                                "IDA_PAY","ac1_IDENTIFIER","IDB_PAY","bk1_IDENTIFIER",
                                                                                "IDA_REC","ac2_IDENTIFIER","IDB_REC","bk2_IDENTIFIER"}));
            }
            object obj = this.BindingContainer.Page;
            if (null != obj)
            {
                Type tObj = obj.GetType();
                PropertyInfo pty = tObj.GetProperty("IsWithComplementaryInfo", typeof(System.Boolean));
                if (null != pty)
                    m_IsWithComplementaryInfo = (bool)tObj.InvokeMember(pty.Name, BindingFlags.GetProperty, null, obj, null);
            }
            if (m_IsWithComplementaryInfo)
            {
                this.ContainerColumnName = new HierarchicalColumnInfo("EventAsset", "btnDetail", "EventAsset", -1, 100);
                this.ContainerColumnName = new HierarchicalColumnInfo("EventDet", "Zoom", "EventDet", -1, 100);
            }
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
        // 20081030 EG Newness EventFee
		private void OnTemplateSelection(object sender, HierarchicalGridTemplateSelectionEventArgs e)
		{
			switch(e.Row.Table.TableName)
			{
				case "Event":
                    e.TemplateFilename = "PanelEvent";
                    e.RelationName = "Event";
                    e.Product = m_Product;
					break;
				case "EventClass":
                    e.TemplateFilename = "PanelEventClass";
                    e.RelationName = "EventClass";
                    e.Product = m_Product;
					break;
				case "EventDet":
                    e.TemplateFilename = "PanelEventDet";
                    e.RelationName = "EventDet";
                    e.Product = m_Product;
					//20071221 PL/EG/FI 
					if (-1 == this.GetContainerColumnID(e.RelationName))
					{
						int i = this.GetContainerColumnID("EventPricing");
                        this.RemoveContainerColumnName("EventPricing");
						this.ContainerColumnName = new HierarchicalColumnInfo(e.RelationName,"Zoom",e.RelationName,i,100);
					}
					break;
				case "EventAsset":
					e.TemplateFilename = "PanelEventAsset";
					e.RelationName     = "EventAsset";
                    e.Product = m_Product;
					break;
                case "EventFee":
                    e.TemplateFilename = "PanelEventFee";
                    e.RelationName = "EventFee";
                    e.Product = m_Product;
                    if (-1 == this.GetContainerColumnID(e.RelationName))
                    {
                        int i = this.GetContainerColumnID("EventDet");
                        this.RemoveContainerColumnName("EventPricing");
                        this.ContainerColumnName = new HierarchicalColumnInfo(e.RelationName, "Fee", e.RelationName, i, 100);
                    }
                    break;
				case "EventPricing":
					e.TemplateFilename = "PanelEventPricing";
					e.RelationName     = "EventPricing";
                    e.Product = m_Product;
					if (-1 == this.GetContainerColumnID(e.RelationName))
					{
						int i = this.GetContainerColumnID("EventDet");
                        // RD 20200601 [25364] Events consultation error
                        //this.RemoveContainerColumnName("EventDet");
                        this.ContainerColumnName = new HierarchicalColumnInfo(e.RelationName,"Pricing",e.RelationName,i,100);
					}
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
            DataGridItem dgi = new DataGridItem(0, 0, ListItemType.Item)
            {
                CssClass = SetCssClassSubTitle(pItemIndex)
            };
            TableCell cell = new TableCell
            {
                ColumnSpan = this.Columns.Count,
                Text = itemTitle[0, pItemIndex]
            };
            dgi.Cells.Add(cell);
			return dgi;
		}
		#endregion AddItemTitle
		#region GetEventEnumRessourceValue
        private static string GetEventEnumRessourceValue(ExtendEnum pExtendEnum, string pCode)
		{
			string ressource = pCode;
			ExtendEnumValue extendEnumValue = pExtendEnum.GetExtendEnumValueByValue(pCode);
			if (null != extendEnumValue)
				ressource = extendEnumValue.Documentation;
			return ressource;
		}
        #endregion GetEventEnumRessourceValue
        #region SetButtonZoom
        // EG 20200724 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        private void SetButtonZoom(DataGridItem pDataGridItem)
        {

            DataRowView parentDataRowView = DataRowViewParent;
            if (null != parentDataRowView)
            {
                #region DataGrid Current item
                DataRowView drv = (DataRowView)pDataGridItem.DataItem;
                string zoom = drv["ZOOM"].ToString();
                #endregion DataGrid Current item

                if (StrFunc.IsFilled(zoom))
                {
                    int idE = Convert.ToInt32(drv["IDE"].ToString());
                    string code = string.Empty;
                    ExtendEnum codeEnum = null;

                    if ("C" == zoom)
                    {
                        code = "EVENTCODE";
                        codeEnum = m_EventCodeEnum;
                    }
                    else if ("T" == zoom)
                    {
                        code = "EVENTTYPE";
                        codeEnum = m_EventTypeEnum;
                    }
                    string onClickValue = "OpenSubTradeEvents";
                    string cssClass = "evtitle orange";

                    if (ProductBase.IsADM)
                    {
                        onClickValue = "OpenSubTradeAdminEvents";
                        if (EventCodeFunc.IsInvoicingDates(parentDataRowView[code].ToString()))
                            cssClass = "evtitle green";
                    }
                    else if (ProductBase.IsRISK)
                    {
                        onClickValue = "OpenSubTradeRiskEvents";
                    }
                    else if (ProductBase.IsDebtSecurity)
                    {
                        onClickValue = "OpenSubDebtSecEvents";
                    }

                    string resCode = GetEventEnumRessourceValue(codeEnum, drv[code].ToString());
                    string resParentCode = GetEventEnumRessourceValue(codeEnum, parentDataRowView[code].ToString());
                    WCToolTipButton btn = this.SetButtonInCell(pDataGridItem, code, drv[code].ToString(), m_EventTypeEnum, 2, cssClass);
                    if (null != btn)
                        btn.Attributes.Add("onclick",
                            onClickValue + "(" + idE + ",'" + resParentCode + "-" + resCode + "');return false;");
                }
            }
        }
        #endregion SetButtonZoom
        #region SetCssClass
        // EG 20200914 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc) Correction et compléments
        // EG 20210329 [25556] Modification icon Awesome pour événement désactivé (Rond pointé rouge)
        private static void SetCssClass(DataGridItem pDataGridItem)
		{
			DataRowView drv			= (DataRowView)pDataGridItem.DataItem;
            _ = drv["EVENTCODE"].ToString();
            _ = drv["EVENTTYPE"].ToString();
            #region Event is dead
            bool isItemLocked = (Cst.StatusActivation.LOCKED.ToString() == drv["IDSTACTIVATION"].ToString());
            if (isItemLocked)
            {
                pDataGridItem.CssClass += EFSCssClass.Event_Locked;
                Label lbl = new Label()
                {
                    CssClass = "fa-icon fa fa-lock",
                    Text = " "
                };
                pDataGridItem.Cells[1].Controls.Add(lbl);
                lbl = new Label() { Text = " Locked" };
                pDataGridItem.Cells[1].Controls.Add(lbl);
            }
            bool isItemDeactivated = (Cst.StatusActivation.DEACTIV.ToString() == drv["IDSTACTIVATION"].ToString());
            if (isItemDeactivated)
            {
                pDataGridItem.CssClass += EFSCssClass.Event_Deactiv;
                Label lbl = new Label()
                {
                    CssClass = "fa-icon fa fa-dot-circle red",
                    Text = " ",
                    ToolTip = Ressource.GetString("DEACTIV"),
                };
                pDataGridItem.Cells[1].Controls.Add(lbl);
                lbl = new Label() { Text = " " + pDataGridItem.Cells[1].Text };
                pDataGridItem.Cells[1].Controls.Add(lbl);
            }
            #endregion Event is dead
        }
        #endregion SetCssClass
        #region SetCssClassSubTitle
        // EG 20200724 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        // EG 20200930 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc) Correction et suppression de codes inutiles
        private string SetCssClassSubTitle(int pItemIndex)
        {
            string cssClass = itemTitle[1, pItemIndex];
            if ("subevtitle black" == itemTitle[1, pItemIndex])
            {
                string family = string.Empty;

                string eventCode;
                string eventType;
                if (this.BindingContainer.GetType().Equals(typeof(DataGridItem)))
                {
                    DataGridItem dgiContainer = (DataGridItem)this.BindingContainer;
                    DataGrid dg = (DataGrid)dgiContainer.Parent.BindingContainer;
                    DataView dv = (DataView)dg.DataSource;
                    DataRowView drv = (DataRowView)dv[dgiContainer.DataSetIndex];
                    eventCode = drv["EVENTCODE"].ToString();
                    eventType = drv["EVENTTYPE"].ToString();
                    family = drv["FAMILY"].ToString();
                }
                else
                {
                    eventCode = EventCodeFunc.Trade;
                    eventType = EventTypeFunc.Date;
                }
                if (EventCodeFunc.IsTrade(eventCode))
                    cssClass = "subevtitle orange"; 
                else if (EventCodeFunc.IsProduct(eventCode))
                    cssClass = "subevtitle blue";
                else if (EventCodeFunc.IsLegGroup(eventCode))
                    cssClass = "subevtitle green";
                else if (EventCodeFunc.IsAdditionalPayment(eventCode))
                    cssClass = "subevtitle violet";
                else if (EventCodeFunc.IsOtherPartyPayment(eventCode))
                    cssClass = "subevtitle violet";
                else if (Cst.ProductFamily_EQD == family)
                    cssClass = itemTitle[1, pItemIndex];
                else if (Cst.ProductFamily_RTS == family)
                {
                    if (EventCodeAndEventTypeFunc.IsLinkedProductClosing(eventCode, eventType))
                        cssClass = "subevtitle orange";
                    else if (EventCodeFunc.IsPositionTransfer(eventCode))
                        cssClass = "subevtitle red";
                    else if (EventCodeFunc.IsPositionCancelation(eventCode))
                        cssClass = "subevtitle red";
                    else if (EventCodeFunc.IsOffsetting(eventCode))
                        cssClass = "subevtitle red";
                }
                else if (Cst.ProductFamily_FX == family)
                    cssClass = itemTitle[1, pItemIndex];
                else if (Cst.ProductFamily_IRD == family)
                {
                    if (EventCodeFunc.IsProductUnderlyer(eventCode))
                        cssClass = "subevtitle orange";
                }
                else if (Cst.ProductFamily_INV == family)
                {
                    if (EventCodeFunc.IsAllocatedInvoiceDates(eventCode))
                        cssClass = "subevtitle orange";
                    else if (EventCodeFunc.IsInvoiceMaster(eventCode))
                        cssClass = "subevtitle orange";
                    else if (EventCodeFunc.IsInvoiceMasterBase(eventCode))
                        cssClass = "subevtitle orange";
                    else if (EventCodeFunc.IsInvoiceAmended(eventCode))
                        cssClass = "subevtitle orange";
                    else if (EventTypeFunc.IsGrossTurnOverAmount(eventType))
                        cssClass = "subevtitle orange";
                }
                else if (Cst.ProductFamily_DSE == family)
                    cssClass = itemTitle[1, pItemIndex];
                else if (Cst.ProductFamily_LSD == family)
                {
                    if (EventCodeAndEventTypeFunc.IsLinkedProductClosing(eventCode, eventType))
                        cssClass = "subevtitle orange";
                    // EG 20170206 [22787] New
                    else if (EventCodeAndEventTypeFunc.IsLinkedPhysicalDelivery(eventCode, eventType))
                        cssClass = "subevtitle orange";
                    else if (EventCodeFunc.IsPositionTransfer(eventCode))
                        cssClass = "subevtitle red";
                    else if (EventCodeFunc.IsPositionCancelation(eventCode))
                        cssClass = "subevtitle red";
                    else if (EventCodeFunc.IsOffsetting(eventCode))
                        cssClass = "subevtitle red";
                }
            }
            return cssClass;
        }
        #endregion SetCssClassSubTitle
        #region SetEventActionAttribute
        /// <summary>
        /// Ajoute une action sur ondblclick
        /// </summary>
        /// <param name="pDataGridItem"></param>
		private static void SetEventActionAttribute(DataGridItem pDataGridItem)
		{
			DataRowView drv	= (DataRowView)pDataGridItem.DataItem;
			int idT			= Convert.ToInt32(drv["IDT"].ToString());
			int idE			= Convert.ToInt32(drv["IDE"].ToString());
            string url      = SessionTools.Menus.GetMenu_Url(IdMenu.GetIdMenu(IdMenu.Menu.InputEvent));

            
            if (StrFunc.IsFilled(url))
            {
                string idMenu = "IDMenu=" + IdMenu.GetIdMenu(IdMenu.Menu.InputEvent);
                if (url.IndexOf(idMenu) <= 0)
                {
                    if (url.IndexOf("?") <= 0)
                        url += "?" + idMenu;
                    else
                        url += "&" + idMenu;
                }
                if (null != pDataGridItem.Page)
                {
                    if (StrFunc.IsFilled(pDataGridItem.Page.Request["GUID"]))
                        url += "&GUID=" + pDataGridItem.Page.Request["GUID"];
                }
                url += "&PKV=" + idE + "&FKV=" + idT;
            }

            pDataGridItem.Attributes.Add("ondblclick", "window.open('" + url + "','_blank')");
		}
		#endregion SetEventActionAttribute
		#region SetRoundingValorisation
		private void SetRoundingValorisation(DataGridItem pDataGridItem)
		{
			DataRowView drv	= (DataRowView)pDataGridItem.DataItem;
            string family = drv["FAMILY"].ToString();
            string gProduct = drv["GPRODUCT"].ToString();
            if ((Cst.ProductGProduct_ASSET != gProduct) || (Cst.ProductFamily_DSE != family))
            {
                #region RoundPrec application by Currency for Amount/Qty
                if ((false == Convert.IsDBNull(drv["UNITTYPE"])) && (false == Convert.IsDBNull(drv["VALORISATION"])) )
                {
                    decimal valorisation = Convert.ToDecimal(drv["VALORISATION"]);
                    UnitTypeEnum unitType = (UnitTypeEnum) ReflectionTools.EnumParse(new UnitTypeEnum(), drv["UNITTYPE"].ToString());

                    switch (unitType)
                    {
                        case UnitTypeEnum.Currency:
                            string currency = drv["UNIT"].ToString();
                            CurrencyCashInfo currencyInfo;
                            if (false == m_CurrencyInfos.ContainsKey(currency))
                            {
                                currencyInfo = new CurrencyCashInfo(SessionTools.CS, currency);
                                if (null != currencyInfo)
                                    m_CurrencyInfos.Add(currency, currencyInfo);
                            }
                            if (m_CurrencyInfos.ContainsKey(currency))
                            {
                                currencyInfo = (CurrencyCashInfo)m_CurrencyInfos[currency];
                                this.SetFormatDecimalCell(pDataGridItem, "VALORISATION", valorisation, currencyInfo.RoundPrec, 2);
                            }
                            break;
                        case UnitTypeEnum.Qty:
                            // EG 20170127 Qty Long To Decimal
                            // FI 20191025 [XXXXX] 4 decimales pour la gestion des GILTS
                            if (false == Convert.IsDBNull(drv["UNIT"]) || ProductBase.IsDebtSecurityTransaction)
                                this.SetFormatDecimalCell(pDataGridItem, "VALORISATION", valorisation, 4, 2);
                            else
                                this.SetFormatDecimalCell(pDataGridItem, "VALORISATION", valorisation, 0, 2);
                            break;
                        case UnitTypeEnum.Percentage:
                            this.SetFormatPercentCell(pDataGridItem, "VALORISATION", valorisation, 2, 2);
                            break;
                        case UnitTypeEnum.Rate:
                        case UnitTypeEnum.None:
                            break;

                    }
                }

                #endregion RoundPrec application by Currency for Amount/Qty
            }
		}
        #endregion SetRoundingValorisation
        #region SetTitleBeforeItem
        // EG 20150624 [21151] OPP restitution
        // EG 20180514 [23812] Report
        // EG 20200724 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        // EG 20200930 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc) Correction et suppression de codes inutiles
        private void SetTitleBeforeItem(DataGridItem pDataGridItem, string pPreviousEventCode, string pPreviousEventType)
		{
			#region DataGrid Current item
			DataRowView drv		= (DataRowView)pDataGridItem.DataItem;
			string eventCode	= drv["EVENTCODE"].ToString();
			string eventType	= drv["EVENTTYPE"].ToString();
            _ = drv["FAMILY"].ToString();
            string product      = drv["IDENTIFIER_PRODUCT"].ToString();
			string resEventCode = GetEventEnumRessourceValue(m_EventCodeEnum, eventCode);
			string resEventType = GetEventEnumRessourceValue(m_EventTypeEnum, eventType);
			int itemIndex		= pDataGridItem.ItemIndex;

            DataRowView parentDrv = DataRowViewParent;
            string eventParentCode = string.Empty;
            string eventParentType = string.Empty;
            if (null != parentDrv)
            {
                eventParentCode = parentDrv["EVENTCODE"].ToString();
                eventParentType = parentDrv["EVENTTYPE"].ToString();
            }
			#endregion DataGrid Current item

			#region Title
			bool isEventCodeChanged = (eventCode != pPreviousEventCode);
            bool isEventCodeParentChanged = (eventCode != eventParentCode);
			bool isEventTypeChanged = (eventType != pPreviousEventType);
            bool isEventTypeParentChanged = (eventType != eventParentType);

            if (EventCodeFunc.IsProduct(eventCode))
            {
                itemTitle[0, itemIndex] = drv["DISPLAYNAME_INSTR"].ToString() + "[" + drv["IDENTIFIER_INSTR"].ToString() + "]";
                itemTitle[1, itemIndex] = "evtitle blue";
            }
            //PM 20140924 [20066][20185] Sauf si parent = CashBalanceStream
            //else if (EventCodeFunc.IsLegGroup(eventCode))
            else if (EventCodeFunc.IsLegGroup(eventCode) && (false == EventCodeFunc.IsCashBalanceStream(eventParentCode)))
            {
                itemTitle[0, itemIndex] = resEventCode;
                itemTitle[1, itemIndex] = "evtitle green";

                if (EventCodeFunc.IsPhysicalLeg(eventCode))
                    itemTitle[0, itemIndex] = resEventType + " " + itemTitle[0, itemIndex];
                else if (false == EventCodeFunc.IsRepoLeg(eventCode) && (false == EventCodeFunc.IsFinancialLeg(eventCode)))
                    itemTitle[0, itemIndex] += " - " + resEventType;

                if ((Cst.ProductBuyAndSellBack == product) || (Cst.ProductRepo == product))
                {
                    if (EventCodeFunc.IsDebtSecurityTransaction(eventCode) || EventCodeFunc.IsDebtSecurityStream(eventCode))
                        itemTitle[1, itemIndex] = "subevtitle black";
                }
                else if (Cst.ProductCashBalance == product)
                {
                    if (EventCodeFunc.IsCashBalanceStream(eventCode))
                        itemTitle[0, itemIndex] += " (" + drv["UNIT"].ToString() + ")";
                }
            }
            else if (EventCodeFunc.IsCashFlowConstituent(eventCode))
            {
                itemTitle[0, itemIndex] = resEventCode;
                itemTitle[1, itemIndex] = "subevtitle orange";
            }
            else if (EventCodeFunc.IsAdditionalPayment(eventCode) && isEventCodeChanged && isEventCodeParentChanged)
            {
                itemTitle[0, itemIndex] = resEventCode;
                itemTitle[1, itemIndex] = "evtitle violet";
            }
            else if (EventCodeFunc.IsSafeKeepingPayment(eventCode) &&
                (false == (ProductBase.IsCashBalance || ProductBase.IsCashBalanceInterest || ProductBase.IsCashPayment)) && isEventCodeChanged && isEventCodeParentChanged)
            {
                itemTitle[0, itemIndex] = resEventCode;
                itemTitle[1, itemIndex] = "subevtitle black";
            }
            else if (EventCodeFunc.IsOtherPartyPayment(eventCode) && isEventCodeChanged && isEventCodeParentChanged)
            {
                itemTitle[0, itemIndex] = resEventCode;
                if (EventCodeFunc.IsDenouement(eventParentCode) || EventCodeFunc.IsAutomaticAbandonAssignmentExercise(eventParentCode))
                    itemTitle[1, itemIndex] = "subevtitle violet";
                else if (EventCodeFunc.IsCashFlowConstituent(eventParentCode))
                    itemTitle[1, itemIndex] = "subevtitle cyan";
                else
                {
                    // EG 20150624 [21151] OPP restitution
                    if (EventCodeFunc.IsOffsetting(eventParentCode) ||
                        EventCodeFunc.IsPositionCancelation(eventParentCode) ||
                        EventCodeFunc.IsUnclearingOffsetting(eventParentCode) ||
                        EventCodeFunc.IsPositionTransfer(eventParentCode) && (isEventCodeChanged || isEventTypeChanged))
                        itemTitle[1, itemIndex] = "subevtitle black"; 
                    else
                        itemTitle[1, itemIndex] = "evtitle violet"; 
                }
            }
            else if (EventCodeFunc.IsDailyClosing(eventCode) && isEventCodeChanged)
                itemTitle[0, itemIndex] = resEventCode;
            else if (EventCodeFunc.IsRemoveTrade(eventCode) && isEventCodeChanged)
                itemTitle[0, itemIndex] = resEventCode;
            else if (EventCodeFunc.IsAbandonAssignmentExercise(eventCode) && isEventCodeChanged)
            {
                itemTitle[0, itemIndex] = resEventCode;
                itemTitle[1, itemIndex] = "evtitle orange";
            }
            else if (EventCodeFunc.IsAutomaticAbandonAssignmentExercise(eventCode) && isEventCodeChanged)
            {
                itemTitle[0, itemIndex] = resEventCode;
                itemTitle[1, itemIndex] = "evtitle violet";
            }
            else if (EventCodeFunc.IsAssignmentExerciseDate(eventCode) && isEventCodeChanged)
            {
                itemTitle[0, itemIndex] = resEventCode;
                itemTitle[1, itemIndex] = "subevtitle gray";
            }
            else if (EventCodeFunc.IsAutomaticExerciseDates(eventCode) && isEventCodeChanged)
            {
                itemTitle[0, itemIndex] = resEventCode;
                itemTitle[1, itemIndex] = "evtitle cyan";
            }
            else if (EventCodeFunc.IsOut(eventCode) && isEventCodeChanged)
            {
                itemTitle[0, itemIndex] = resEventCode;
            }
            else
            {
                if (ProductBase.IsEQD)
                    SetItemTitle_EQD(itemIndex, eventCode, resEventCode, isEventCodeChanged, eventType, resEventType, isEventTypeChanged, pPreviousEventType);
                else if (ProductBase.IsBondOption)
                    SetItemTitle_BO(product, itemIndex, eventCode, resEventCode, isEventCodeChanged, eventType, resEventType, isEventTypeChanged, pPreviousEventType);
                else if (ProductBase.IsCommoditySpot)
                    SetItemTitle_COMD(itemIndex, eventCode, isEventCodeChanged, eventType, resEventType, isEventTypeChanged);
                else if (ProductBase.IsFx)
                    SetItemTitle_FX(itemIndex, eventCode, resEventCode, isEventCodeChanged, eventType, resEventType, isEventTypeChanged, isEventTypeParentChanged);
                else if (ProductBase.IsIRD)
                    SetItemTitle_IRD(itemIndex, eventCode, resEventCode, isEventCodeChanged, eventType, resEventType, isEventTypeChanged);
                else if (ProductBase.IsADM)
                    SetItemTitle_INV(itemIndex, eventCode, resEventCode, isEventCodeChanged, eventType, resEventType, isEventTypeChanged, pPreviousEventCode);
                else if (ProductBase.IsDSE)
                    SetItemTitle_DSE(product, itemIndex, eventCode, resEventCode, isEventCodeChanged, eventType, resEventType, isEventTypeChanged, eventParentCode);
                else if (ProductBase.IsLSD)
                    SetItemTitle_ETD(itemIndex, eventCode, resEventCode, isEventCodeChanged, eventType, resEventType, isEventTypeChanged, eventParentCode);
                else if (ProductBase.IsCashBalance || ProductBase.IsCashBalanceInterest || ProductBase.IsCashPayment)
                    SetItemTitle_CSB(itemIndex, eventCode, resEventCode, eventType, resEventType, isEventTypeChanged, eventParentCode);
                else if (ProductBase.IsESE)
                    SetItemTitle_ESE(itemIndex, eventCode, resEventCode, isEventCodeChanged, eventType, resEventType, isEventTypeChanged, eventParentCode);
                else if (ProductBase.IsReturnSwap)
                    SetItemTitle_RTS(itemIndex, eventCode, resEventCode, isEventCodeChanged, eventType, resEventType, isEventTypeChanged, pPreviousEventType);
            }
			#endregion Title		
		}
        #endregion SetTitleBeforeItem
        #region SetItemTitle_BO
        /// EG 20150513 [20513] New
        // EG 20200724 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        private void SetItemTitle_BO(string _, int pIndex, string pCode, string pResCode, bool pIsCodeChanged, string pType,
            string pResType, bool pIsTypeChanged, string _2)
        {
            if (EventCodeFunc.IsInitialValuation(pCode))
                itemTitle[0, pIndex] = pResCode + " - " + pResType;
            else if (EventTypeFunc.IsNominal(pType))
                itemTitle[0, pIndex] = pResType;
            else if (EventTypeFunc.IsQuantity(pType))
                itemTitle[0, pIndex] = Ressource.GetString("nbOfOptions");
            else if ((EventCodeFunc.IsStart(pCode) || EventCodeFunc.IsIntermediary(pCode) || EventCodeFunc.IsTermination(pCode)) && EventTypeFunc.IsUnderlyer(pType))
                itemTitle[0, pIndex] = Ressource.GetString("bondUnits");
            else if (EventTypeFunc.IsBondPayment(pType))
                itemTitle[0, pIndex] = pResType;
            else if (EventCodeFunc.IsAsian(pCode) && pIsCodeChanged)
                itemTitle[0, pIndex] = pResCode;
            else if (EventTypeFunc.IsPremium(pType) && pIsTypeChanged)
            {
                itemTitle[0, pIndex] = pResType;
                itemTitle[1, pIndex] = "evtitle orange";
            }
            else if (EventCodeFunc.IsBarrier(pCode))
            {
                if (EventTypeFunc.IsDownUpTouch(pType) &&
                    (pIsCodeChanged || (pIsTypeChanged && EventTypeFunc.IsDownUpInOut(lastEventType))))
                    itemTitle[0, pIndex] = pResCode + " " + GetEventEnumRessourceValue(m_EventTypeEnum, EventTypeFunc.Features);
                else if (EventTypeFunc.IsDownUpInOut(pType) &&
                    (pIsCodeChanged || (pIsTypeChanged && EventTypeFunc.IsDownUpTouch(lastEventType))))
                    itemTitle[0, pIndex] = GetEventEnumRessourceValue(m_EventClassEnum, EventClassFunc.BarrierKnock) + " " +
                        GetEventEnumRessourceValue(m_EventTypeEnum, EventTypeFunc.Features);
            }
        }
        #endregion SetItemTitle_BO
        #region SetItemTitle_COMD
        /// EG 20161122 New Commodity Derivative
        // EG 20200724 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        private void SetItemTitle_COMD(int pIndex, string pCode,  bool pIsCodeChanged, string pType, string pResType, bool pIsTypeChanged)
        {
            if (EventCodeAndEventTypeFunc.IsQuantityPeriodVariation(pCode, pType) && pIsTypeChanged)
                itemTitle[0, pIndex] = pResType;
            else if (EventCodeFunc.IsLinkedProductPayment(pCode) && EventTypeFunc.IsAmounts(pType) && (pIsCodeChanged || pIsTypeChanged))
            {
                itemTitle[0, pIndex] = "Commodity payment";
                itemTitle[1, pIndex] = "subevtitle red";
            }
        }
        #endregion SetItemTitle_COMD
        #region SetItemTitle_CSB
        // EG 20200724 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        private void SetItemTitle_CSB(int pIndex, string pCode, string pResCode, string pType, string pResType,
            bool pIsTypeChanged, string pEventParentCode)
        {
            if ((EventTypeFunc.IsBorrowingAmount(pType) ||
                 EventTypeFunc.IsCashBalance(pType) ||
                 EventTypeFunc.IsCashBalanceAmounts(pType)||
                 EventTypeFunc.IsCashDeposit(pType) ||
                 EventTypeFunc.IsCashWithdrawal(pType) ||
                 EventTypeFunc.IsEquityBalance(pType) ||
                 EventTypeFunc.IsExcessDeficit(pType) ||
                 EventTypeFunc.IsForwardCashPayment(pType) ||
                 EventTypeFunc.IsFundingAmount(pType) ||
                 EventTypeFunc.IsLongOptionValue(pType) ||
                 EventTypeFunc.IsMarginCall(pType) ||
                 EventTypeFunc.IsMarginRequirement(pType) ||
                 EventTypeFunc.IsMarketValue(pType) ||
                 EventTypeFunc.IsRealizedMargin(pType) ||
                 EventTypeFunc.IsShortOptionValue(pType) ||
                 EventTypeFunc.IsTotalAccountValue(pType) ||
                 EventTypeFunc.IsUncoveredMarginRequirement(pType)||
                 EventTypeFunc.IsUnrealizedMargin(pType) ||
                 EventCodeFunc.IsSafeKeepingPayment(pCode) ||
                 EventTypeFunc.IsUnsettledTransaction(pType))
                 && (false == EventCodeFunc.IsLinkedFuture(pCode))
                 && (false == EventCodeFunc.IsLinkedOption(pCode))
                 && StrFunc.IsFilled(pEventParentCode) && pIsTypeChanged)
            {
                itemTitle[0, pIndex] = pResType;
                itemTitle[1, pIndex] = "subevtitle green";
            }
            else if (EventTypeFunc.IsCashAvailable(pType) && pIsTypeChanged)
            {
                itemTitle[0, pIndex] = pResType;
                itemTitle[1, pIndex] = "evtitle green";
            }
            else if ((EventTypeFunc.IsPreviousCashBalance(pType) ||
                EventTypeFunc.IsCashBalancePayment(pType) ) && pIsTypeChanged)
            {
                itemTitle[0, pIndex] = pResType;
                itemTitle[1, pIndex] = "evtitle orange";
            }
            else if ( EventCodeFunc.IsETDFuture(pCode) ||
                EventCodeFunc.IsFuturesStyleOption(pCode) ||
                EventCodeFunc.IsPremiumStyleOption(pCode))
            {
                itemTitle[0, pIndex] = pResCode + " - " + pResType;
                itemTitle[1, pIndex] = "evtitle orange";
            }
        }
        #endregion SetItemTitle_CSB
        #region SetItemTitle_DSE
        // EG 20150624 [21151] AIN restitution
        // FI 20151228 [21660] PAM restitution
        // EG 20190613 [24683] OCP 
        // EG 20190716 [VCL : New FixedIncome] Upd
        // EG 20190730 Add IsHistoricalGrossAmount
        // EG 20190926 [Maturity Redemption] Upd
        // EG 20200724 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        // EG 20200930 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc) Correction et suppression de codes inutiles
        // EG 20210318 [XXXXXX] Gestion du sous-titre EqualizationPayment
        private void SetItemTitle_DSE(string pProduct, int pIndex, string pCode, string pResCode, bool pIsCodeChanged, string pType, string pResType, 
            bool pIsTypeChanged, string pEventParentCode)
        {
            #region DSE / DST
            if (EventTypeFunc.IsDebtSecurityTransactionAmounts(pType) && pIsTypeChanged)
            {
                itemTitle[0, pIndex] = pResType;
                itemTitle[1, pIndex] = "evtitle orange";
                if ((Cst.ProductBuyAndSellBack == pProduct) || (Cst.ProductRepo == pProduct))
                    itemTitle[1, pIndex] = "subevtitle black";
            }
            else if ((EventTypeFunc.IsGrossAmount(pType) ||
                EventTypeFunc.IsHistoricalGrossAmount(pType) ||
                EventCodeAndEventTypeFunc.IsQuantityPeriodVariation(pCode, pType) ||
                EventCodeAndEventTypeFunc.IsNominalPeriodVariation(pCode, pType) ||
                EventTypeFunc.IsAccruedInterestAmount(pType) ||
                EventTypeFunc.IsPrincipalAmount(pType) ||
                EventTypeFunc.IsMarketValue(pType) ||
                EventTypeFunc.IsEqualizationPayment(pType) ||
                EventTypeFunc.IsRedemptionAmount(pType)) && pIsTypeChanged)
            {
                itemTitle[0, pIndex] = pResType;
                if (EventCodeFunc.IsOffsetting(pEventParentCode) || (EventCodeFunc.IsOffSettingCorporateAction(pEventParentCode)))
                    itemTitle[1, pIndex] = "subevtitle red";
            }
            else if (EventCodeAndEventTypeFunc.IsLinkedProductClosing(pCode, pType))
            {
                itemTitle[0, pIndex] = pResCode;
                itemTitle[1, pIndex] = "evtitle orange";
            }
            else if (EventCodeFunc.IsOffsetting(pCode) && pIsTypeChanged)
            {
                itemTitle[0, pIndex] = pResCode + " - " + pResType;
                itemTitle[1, pIndex] = "evtitle red";
            }
            else if (EventCodeFunc.IsUnclearingOffsetting(pCode))
            {
                itemTitle[0, pIndex] = pResCode + " - " + pResType;
                itemTitle[1, pIndex] = "evtitle red";
            }
            // EG 20130418 Add IsOffSettingCorporateAction
            else if (EventCodeFunc.IsOffSettingCorporateAction(pCode))
            {
                itemTitle[0, pIndex] = pResCode + " - " + pResType;
                itemTitle[1, pIndex] = "evtitle red";
            }
            else if (EventCodeFunc.IsMaturityOffsetting(pCode))
            {
                itemTitle[0, pIndex] = pResCode + " - " + pResType;
                itemTitle[1, pIndex] = "evtitle red";
            }
            else if (EventCodeFunc.IsPositionCancelation(pCode) && (pIsCodeChanged || pIsTypeChanged))
            {
                itemTitle[0, pIndex] = pResCode + " - " + pResType;
                itemTitle[1, pIndex] = "evtitle red";
            }
            else if (EventCodeFunc.IsPositionTransfer(pCode) && (pIsCodeChanged || pIsTypeChanged))
            {
                itemTitle[0, pIndex] = pResCode + " - " + pResType;
                itemTitle[1, pIndex] = "evtitle red";
            }
            else if ((EventCodeAndEventTypeFunc.IsVariationMargin(pCode, pType) ||
                EventCodeAndEventTypeFunc.IsHistoricalVariationMargin(pCode, pType) ||
                EventCodeAndEventTypeFunc.IsRealizedMargin(pCode, pType) ||
                EventCodeAndEventTypeFunc.IsUnrealizedMargin(pCode, pType)) && (pIsCodeChanged || pIsTypeChanged))
            {
                itemTitle[0, pIndex] = pResType;
                if (EventCodeFunc.IsOffsetting(pEventParentCode) ||
                    EventCodeFunc.IsPositionCancelation(pEventParentCode) ||
                    EventCodeFunc.IsUnclearingOffsetting(pEventParentCode) ||
                    EventCodeFunc.IsPositionTransfer(pEventParentCode) ||
                    EventCodeFunc.IsOffSettingClosingPosition(pEventParentCode) && (pIsCodeChanged || pIsTypeChanged))
                    itemTitle[1, pIndex] = "subevtitle red";
            }
            else if (EventTypeFunc.IsEqualizationPayment(pType))
            {
                if (EventCodeFunc.IsOffSettingCorporateAction(pEventParentCode))
                {
                    itemTitle[0, pIndex] = pResType;
                    itemTitle[1, pIndex] = "subevtitle red";
                }
            }
            #endregion DSE / DST
        }
        #endregion SetItemTitle_DSE
        #region SetItemTitle_ESE
        /// EG 20150306 [POC-BERKELEY] : Refactoring
        // EG 20190613 [24683] OCP 
        // EG 20190730 Add IsHistoricalGrossAmount
        // EG 20200724 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        // EG 20210318 [XXXXXX] Gestion du sous-titre EqualizationPayment
        private void SetItemTitle_ESE(int pIndex, string pCode, string pResCode, bool pIsCodeChanged, string pType, string pResType, 
            bool pIsTypeChanged, string pEventParentCode)
        {
            #region ESE
            if (EventCodeFunc.IsEquitySecurityTransaction(pCode))
                itemTitle[0, pIndex] = pResCode + "(" + pResType + ")";


            if ((EventTypeFunc.IsGrossAmount(pType) ||
                 EventTypeFunc.IsHistoricalGrossAmount(pType) ||
                 EventCodeAndEventTypeFunc.IsQuantityPeriodVariation(pCode, pType) ||
                 EventTypeFunc.IsInitialMargin(pType) || EventCodeAndEventTypeFunc.IsMarginRequirementRatio(pCode, pType)) && pIsTypeChanged)
            {
                itemTitle[0, pIndex] = pResType;
                if (EventCodeFunc.IsOffsetting(pEventParentCode) ||
                    EventCodeFunc.IsOffSettingClosingPosition(pEventParentCode) ||
                    EventCodeFunc.IsOffSettingCorporateAction(pEventParentCode))
                    itemTitle[1, pIndex] = "subevtitle red";
            }
            else if ((EventTypeFunc.IsInitialMargin(pType) || EventTypeFunc.IsMarginRequirement(pType) ||
                EventTypeFunc.IsMarketValue(pType) || EventTypeFunc.IsTotalMargin(pType)) && pIsTypeChanged)
                itemTitle[0, pIndex] = pResType;

            else if (EventCodeAndEventTypeFunc.IsLinkedProductClosing(pCode, pType))
            {
                itemTitle[0, pIndex] = pResCode;
                itemTitle[1, pIndex] = "evtitle orange";
            }
            else if (EventCodeFunc.IsOffsetting(pCode) && pIsTypeChanged)
            {
                itemTitle[0, pIndex] = pResCode + " - " + pResType;
                itemTitle[1, pIndex] = "evtitle red";
            }
            else if (EventCodeFunc.IsUnclearingOffsetting(pCode))
            {
                itemTitle[0, pIndex] = pResCode + " - " + pResType;
                itemTitle[1, pIndex] = "evtitle red";
            }
            // EG 20130418 Add IsOffSettingCorporateAction
            else if (EventCodeFunc.IsOffSettingCorporateAction(pCode))
            {
                itemTitle[0, pIndex] = pResCode + " - " + pResType;
                itemTitle[1, pIndex] = "evtitle red";
            }
            else if (EventCodeFunc.IsOffSettingClosingPosition(pCode))
            {
                itemTitle[0, pIndex] = pResCode + " - " + pResType;
                itemTitle[1, pIndex] = "evtitle red";
            }
            else if (EventCodeFunc.IsPositionCancelation(pCode) && (pIsCodeChanged || pIsTypeChanged))
            {
                itemTitle[0, pIndex] = pResCode + " - " + pResType;
                itemTitle[1, pIndex] = "evtitle red";
            }
            else if (EventCodeFunc.IsPositionTransfer(pCode) && (pIsCodeChanged || pIsTypeChanged))
            {
                itemTitle[0, pIndex] = pResCode + " - " + pResType;
                itemTitle[1, pIndex] = "evtitle red";
            }
            else if ((EventCodeAndEventTypeFunc.IsRealizedMargin(pCode, pType) || EventCodeAndEventTypeFunc.IsUnrealizedMargin(pCode, pType)) && (pIsCodeChanged || pIsTypeChanged))
            {
                itemTitle[0, pIndex] = pResType;
                if (EventCodeFunc.IsOffsetting(pEventParentCode) ||
                    EventCodeFunc.IsPositionCancelation(pEventParentCode) ||
                    EventCodeFunc.IsUnclearingOffsetting(pEventParentCode) ||
                    EventCodeFunc.IsOffSettingClosingPosition(pEventParentCode) ||
                    EventCodeFunc.IsPositionTransfer(pEventParentCode) ||
                    EventCodeFunc.IsOffSettingClosingPosition(pEventParentCode) && (pIsCodeChanged || pIsTypeChanged))
                    itemTitle[1, pIndex] = "subevtitle red";
            }
            else if (EventTypeFunc.IsEqualizationPayment(pType))
            {
                if (EventCodeFunc.IsOffSettingCorporateAction(pEventParentCode))
                {
                    itemTitle[0, pIndex] = pResType;
                    itemTitle[1, pIndex] = "subevtitle red";
                }
            }
            #endregion ESE
        }
        #endregion SetItemTitle_ESE
        #region SetItemTitle_EQD
        // EG 20200724 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        private void SetItemTitle_EQD(int pIndex, string pCode, string pResCode, bool pIsCodeChanged, string pType, 
            string pResType, bool pIsTypeChanged, string pPreviousType)
        {
            if (EventTypeFunc.IsUnderlyerComponent(pType) && EventTypeFunc.IsNominal(pPreviousType) && pIsTypeChanged)
                itemTitle[0, pIndex] = GetEventEnumRessourceValue(m_EventTypeEnum, EventTypeFunc.Underlyer);
            else if (EventCodeFunc.IsInitialValuation(pCode))
                itemTitle[0, pIndex] = pResCode + " - " + pResType;
            else if (EventTypeFunc.IsNominal(pType))
                itemTitle[0, pIndex] = pResType;
            else if (EventCodeFunc.IsAsian(pCode) && pIsCodeChanged)
                itemTitle[0, pIndex] = pResCode;
            else if (EventCodeFunc.IsLookBackOption(pCode) && pIsCodeChanged)
                itemTitle[0, pIndex] = pResCode;
            else if (EventCodeFunc.IsBarrier(pCode))
            {
                if (EventTypeFunc.IsDownUpTouch(pType) &&
                    (pIsCodeChanged || (pIsTypeChanged && EventTypeFunc.IsDownUpInOut(lastEventType))))
                    itemTitle[0, pIndex] = pResCode + " " + GetEventEnumRessourceValue(m_EventTypeEnum, EventTypeFunc.Features);
                else if (EventTypeFunc.IsDownUpInOut(pType) &&
                    (pIsCodeChanged || (pIsTypeChanged && EventTypeFunc.IsDownUpTouch(lastEventType))))
                    itemTitle[0, pIndex] = GetEventEnumRessourceValue(m_EventClassEnum, EventClassFunc.BarrierKnock) + " " +
                        GetEventEnumRessourceValue(m_EventTypeEnum, EventTypeFunc.Features);
            }
        }
        #endregion SetItemTitle_EQD
        #region SetItemTitle_ETD
        // EG 20190613 [24683] OCP 
        // EG 20200724 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        // EG 20210318 [XXXXXX] Gestion du sous-titre EqualizationPayment
        private void SetItemTitle_ETD(int pIndex, string pCode, string pResCode, bool pIsCodeChanged, string pType, string pResType,
            bool pIsTypeChanged, string pEventParentCode)
        {
            if ((EventCodeAndEventTypeFunc.IsNominalPeriodVariation(pCode, pType) ||
                EventCodeAndEventTypeFunc.IsQuantityPeriodVariation(pCode, pType)) && pIsTypeChanged)
            {
                itemTitle[0, pIndex] = pResType;
                // PM 20130213 Ticket 18414 Add IsCascading
                // PM 20130308 Ticket 18434 Add IsShifting
                // EG 20130418 Add IsOffSettingCorporateAction
                if (EventCodeFunc.IsMaturityOffsetting(pEventParentCode) ||
                    EventCodeFunc.IsCascading(pEventParentCode) ||
                    EventCodeFunc.IsShifting(pEventParentCode))
                    itemTitle[1, pIndex] = "subevtitle red";
                else if (EventCodeFunc.IsAbandonAssignmentExercise(pEventParentCode))
                    itemTitle[1, pIndex] = "subevtitle orange";
                else if (EventCodeFunc.IsAutomaticAbandonAssignmentExercise(pEventParentCode))
                    itemTitle[1, pIndex] = "subevtitle violet";
                else if (EventCodeFunc.IsOffsetting(pEventParentCode) ||
                        EventCodeFunc.IsOffSettingCorporateAction(pEventParentCode) ||
                        EventCodeFunc.IsOffSettingClosingPosition(pEventParentCode))
                    itemTitle[1, pIndex] = "subevtitle red";
            }
            else if (EventCodeFunc.IsNominalQuantityStep(pCode) && pIsCodeChanged)
                itemTitle[0, pIndex] = pResCode;
            else if (EventCodeAndEventTypeFunc.IsLinkedProductClosing(pCode, pType))
            {
                itemTitle[0, pIndex] = pResCode;
                itemTitle[1, pIndex] = "evtitle orange";
            }
            // EG 20170206 [22787] New
            else if (EventCodeAndEventTypeFunc.IsLinkedPhysicalDelivery(pCode, pType))
            {
                itemTitle[0, pIndex] = pResCode;
                itemTitle[1, pIndex] = "evtitle orange";
            }
            else if (EventCodeFunc.IsOffsetting(pCode) && pIsTypeChanged)
            {
                itemTitle[0, pIndex] = pResCode + " - " + pResType;
                itemTitle[1, pIndex] = "evtitle red";
            }
            else if (EventCodeFunc.IsUnclearingOffsetting(pCode) && (pIsTypeChanged || (pCode != pEventParentCode)))
            {
                itemTitle[0, pIndex] = pResCode + " - " + pResType;
                itemTitle[1, pIndex] = "evtitle red";
            }
            else if (EventCodeFunc.IsMaturityOffsetting(pCode))
            {
                itemTitle[0, pIndex] = pResCode + " - " + pResType;
                itemTitle[1, pIndex] = "evtitle red";
            }
            // EG 20130418 Add IsOffSettingCorporateAction
            else if (EventCodeFunc.IsOffSettingCorporateAction(pCode))
            {
                itemTitle[0, pIndex] = pResCode + " - " + pResType;
                itemTitle[1, pIndex] = "evtitle red";
            }
            else if (EventCodeFunc.IsOffSettingClosingPosition(pCode))
            {
                itemTitle[0, pIndex] = pResCode + " - " + pResType;
                itemTitle[1, pIndex] = "evtitle red";
            }
            // PM 20130213 Ticket 18414 Add IsCascading
            else if (EventCodeFunc.IsCascading(pCode))
            {
                itemTitle[0, pIndex] = pResCode + " - " + pResType;
                itemTitle[1, pIndex] = "evtitle blue";
            }
            // PM 20130308 Ticket 18434 Add IsShifting
            else if (EventCodeFunc.IsShifting(pCode))
            {
                itemTitle[0, pIndex] = pResCode + " - " + pResType;
                itemTitle[1, pIndex] = "evtitle rose";
            }
            else if (EventCodeFunc.IsPositionCancelation(pCode) && (pIsCodeChanged || pIsTypeChanged))
            {
                itemTitle[0, pIndex] = pResCode + " - " + pResType;
                itemTitle[1, pIndex] = "evtitle red";
            }
            else if (EventCodeFunc.IsPositionTransfer(pCode) && (pIsCodeChanged || pIsTypeChanged))
            {
                itemTitle[0, pIndex] = pResCode + " - " + pResType;
                itemTitle[1, pIndex] = "evtitle red";
            }
            else if ((EventCodeAndEventTypeFunc.IsVariationMargin(pCode, pType) ||
                EventCodeAndEventTypeFunc.IsHistoricalVariationMargin(pCode, pType) ||
                EventCodeAndEventTypeFunc.IsRealizedMargin(pCode, pType) ||
                EventCodeAndEventTypeFunc.IsLiquidationOptionValue(pCode, pType) ||
                EventCodeAndEventTypeFunc.IsUnrealizedMargin(pCode, pType)) && (pIsCodeChanged || pIsTypeChanged))
            {
                itemTitle[0, pIndex] = pResType;
                // PM 20130213 Ticket 18414 Add IsCascading
                // PM 20130308 Ticket 18434 Add IsShifting
                if (EventCodeFunc.IsMaturityOffsetting(pEventParentCode) ||
                    EventCodeFunc.IsCascading(pEventParentCode) ||
                    EventCodeFunc.IsShifting(pEventParentCode))
                    itemTitle[1, pIndex] = "subevtitle red";
                else if (EventCodeFunc.IsOffsetting(pEventParentCode) ||
                    EventCodeFunc.IsPositionCancelation(pEventParentCode) ||
                    EventCodeFunc.IsUnclearingOffsetting(pEventParentCode) ||
                    EventCodeFunc.IsOffSettingClosingPosition(pEventParentCode) ||
                    EventCodeFunc.IsPositionTransfer(pEventParentCode) && (pIsCodeChanged || pIsTypeChanged))
                    itemTitle[1, pIndex] = "subevtitle red";
                else if (EventCodeFunc.IsAbandonAssignmentExercise(pEventParentCode))
                    itemTitle[1, pIndex] = "subevtitle red";
                else if (EventCodeFunc.IsAutomaticAbandonAssignmentExercise(pEventParentCode))
                    itemTitle[1, pIndex] = "subevtitle violet";
            }
            else if ((EventCodeAndEventTypeFunc.IsIntradayVariationMargin(pCode, pType) ||
                EventCodeAndEventTypeFunc.IsIntradayLiquidationOptionValue(pCode, pType) ||
                EventCodeAndEventTypeFunc.IsIntradayUnrealizedMargin(pCode, pType)) && (pIsCodeChanged || pIsTypeChanged))
                itemTitle[0, pIndex] = "Intraday " + pResType;
            else if (EventTypeFunc.IsPremium(pType) && pIsTypeChanged)
            {
                itemTitle[0, pIndex] = pResType;
                if (EventCodeFunc.IsPositionCancelation(pEventParentCode) ||
                    EventCodeFunc.IsUnclearingOffsetting(pEventParentCode) || 
                    EventCodeFunc.IsPositionTransfer(pEventParentCode))
                    itemTitle[1, pIndex] = "subevtitle red";
                else
                    itemTitle[1, pIndex] = "evtitle orange";
            }
            else if (EventTypeFunc.IsHistoricalPremium(pType) && pIsTypeChanged)
            {
                itemTitle[0, pIndex] = pResType;
                itemTitle[1, pIndex] = "evtitle orange";
            }
            else if (EventTypeFunc.IsSettlementCurrency(pType) && pIsTypeChanged)
            {
                itemTitle[0, pIndex] = pResType;
                if (EventCodeFunc.IsAbandonAssignmentExercise(pEventParentCode))
                    itemTitle[1, pIndex] = "subevtitle orange";
                else if (EventCodeFunc.IsAutomaticAbandonAssignmentExercise(pEventParentCode))
                    itemTitle[1, pIndex] = "subevtitle violet";
            }
            else if (EventTypeFunc.IsEqualizationPayment(pType))
            {
                if (EventCodeFunc.IsOffSettingCorporateAction(pEventParentCode))
                {
                    itemTitle[0, pIndex] = pResType;
                    itemTitle[1, pIndex] = "subevtitle red";
                }
            }
        }
        #endregion SetItemTitle_ETD
        #region SetItemTitle_FX
        // EG 20180514 [23812] Report
        // EG 20200724 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        private void SetItemTitle_FX(int pIndex, string pCode, string pResCode, bool pIsCodeChanged, string pType, 
            string pResType, bool pIsTypeChanged, bool pIsTypeParentChanged)
        {
            if (EventTypeFunc.IsProvision(pType))
            {
                itemTitle[0, pIndex] = pResType;
                itemTitle[1, pIndex] = "evtitle gray";
            }
            else if (EventCodeFunc.IsBarrier(pCode) && pIsCodeChanged)
                itemTitle[0, pIndex] = pResCode;
            else if (EventTypeFunc.IsCashSettlement(pType) && pIsTypeChanged)
                itemTitle[0, pIndex] = pResType;
            else if (EventCodeFunc.IsTrigger(pCode) && pIsCodeChanged)
                itemTitle[0, pIndex] = pResCode;
            else if (EventCodeFunc.IsFxExerciseProcedure(pCode) && pIsCodeChanged)
                itemTitle[0, pIndex] = pResCode;
            else if (EventTypeFunc.IsForwardPoints(pType) && pIsTypeChanged)
                itemTitle[0, pIndex] = pResType;
            else if (EventTypeFunc.IsPayout(pType) && pIsTypeChanged && pIsTypeParentChanged)
                itemTitle[0, pIndex] = pResType;
            else if (EventTypeFunc.IsRebate(pType) && pIsTypeChanged && pIsTypeParentChanged)
                itemTitle[0, pIndex] = pResType;
            else if (EventCodeAndEventTypeFunc.IsMarginRequirementRatio(pCode, pType))
                itemTitle[0, pIndex] = pResType;
            else if (EventTypeFunc.IsInitialMargin(pType))
                itemTitle[0, pIndex] = pResType;
            else if ((EventTypeFunc.IsInitialMargin(pType) || EventTypeFunc.IsMarginRequirement(pType)) && pIsTypeChanged)
                itemTitle[0, pIndex] = pResType;
            else if ((EventCodeAndEventTypeFunc.IsLiquidationOptionValue(pCode, pType) || EventCodeAndEventTypeFunc.IsUnrealizedMargin(pCode, pType)) 
                && (pIsCodeChanged || pIsTypeChanged))
                itemTitle[0, pIndex] = pResType;
            else if (EventCodeAndEventTypeFunc.IsLinkedProductClosing(pCode, pType))
            {
                itemTitle[0, pIndex] = pResCode;
                itemTitle[1, pIndex] = "evtitle orange";
            }
        }
        #endregion SetItemTitle_FX
        #region SetItemTitle_INV
        // EG 20200724 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        private void SetItemTitle_INV(int pIndex, string pCode, string pResCode, bool pIsCodeChanged, string pType, 
            string pResType, bool pIsTypeChanged, string pPreviousCode)
        {
            if ((EventCodeFunc.IsInvoiceAmended(pCode) || EventCodeFunc.IsInvoiceMaster(pCode) || EventCodeFunc.IsInvoiceMasterBase(pCode)) &&
                (pIsCodeChanged && StrFunc.IsFilled(pPreviousCode)))
            {
                itemTitle[0, pIndex] = pResCode;
                itemTitle[1, pIndex] = "evtitle orange";
            }
            else if (EventTypeFunc.IsGrossTurnOverAmount(pType))
                itemTitle[0, pIndex] = pResType;
            else if (EventTypeFunc.IsGlobalRebate(pType) && pIsTypeChanged)
                itemTitle[0, pIndex] = pResType;
            else if (EventTypeFunc.IsBracketRebate(pType) || EventTypeFunc.IsCapRebate(pType))
                itemTitle[0, pIndex] = pResType;
            else if (EventTypeFunc.IsNetTurnOverAmount(pType) && pIsTypeChanged)
                itemTitle[0, pIndex] = pResType;
            else if (EventCodeFunc.IsAdditionalInvoiceDates(pCode) && pIsCodeChanged)
                itemTitle[0, pIndex] = pResCode;
            else if (EventCodeFunc.IsCreditNoteDates(pCode) && pIsCodeChanged)
                itemTitle[0, pIndex] = pResCode;
            else if (EventCodeFunc.IsAllocatedInvoiceDates(pCode) && pIsCodeChanged)
            {
                itemTitle[0, pIndex] = pResCode;
                itemTitle[1, pIndex] = "evtitle orange";
            }
            else if (EventTypeFunc.IsForeignExchangeProfit(pType) || EventTypeFunc.IsForeignExchangeLoss(pType))
                itemTitle[0, pIndex] = pResType;
        }
        #endregion SetItemTitle_INV
        #region SetItemTitle_IRD
        // EG 20190419 Update Titre sur Nominal step
        // EG 20200724 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        private void SetItemTitle_IRD(int pIndex, string pCode, string pResCode, bool pIsCodeChanged, string pType, string pResType, bool pIsTypeChanged)
        {
            if (EventTypeFunc.IsProvision(pType))
            {
                itemTitle[0, pIndex] = pResType;
                itemTitle[1, pIndex] = "evtitle gray";
            }
            else if (EventCodeAndEventTypeFunc.IsNominalPeriodVariation(pCode, pType) && (pIsTypeChanged))
                itemTitle[0, pIndex] = pResType;
            else if (EventCodeFunc.IsNominalStep(pCode) && pIsCodeChanged)
                itemTitle[0, pIndex] = pResCode;
            else if (EventTypeFunc.IsNominal(pType) && 
                (false == EventCodeAndEventTypeFunc.IsNominalPeriodVariation(pCode, pType)) && 
                (false == EventCodeFunc.IsNominalStep(pCode)))
                itemTitle[0, pIndex] = pResCode;
            else if (EventTypeFunc.IsInterest(pType) && (pIsTypeChanged))
                itemTitle[0, pIndex] = pResType;
            else if (EventCodeFunc.IsProductUnderlyer(pCode) && pIsCodeChanged)
            {
                itemTitle[0, pIndex] = pResCode;
                itemTitle[1, pIndex] = "evtitle orange";
            }
        }
        #endregion SetItemTitle_IRD
        #region SetItemTitle_RTS
        /// EG 20150302 Add BaseCurrency|QuotedCurrency (CFD Forex)
        // EG 20200724 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        // EG 20210318 [XXXXXX] Gestion du sous-titre EqualizationPayment
        // EG 20231127 [WI754] Implementation Return Swap : Add Test NominalStep
        private void SetItemTitle_RTS(int pIndex, string pCode, string pResCode, bool pIsCodeChanged,
            string pType, string pResType, bool pIsTypeChanged, string pEventParentCode)
        {
            if (EventCodeFunc.IsSingleUnderlyer(pCode) || EventCodeFunc.IsBasket(pCode))
            {
                itemTitle[0, pIndex] = pResCode + " - " + pResType;
                itemTitle[1, pIndex] = "evtitle orange";
            }
            else if (EventCodeFunc.IsNominalStep(pCode) && pIsCodeChanged)
                itemTitle[0, pIndex] = pResCode;
            else if (EventTypeFunc.IsNominal(pType) && pIsTypeChanged)
                itemTitle[0, pIndex] = pResType;
            else if (EventCodeAndEventTypeFunc.IsLinkedProductClosing(pCode, pType) || EventCodeAndEventTypeFunc.IsLinkedProductPayment(pCode, pType))
            {
                itemTitle[0, pIndex] = pResCode;
                itemTitle[1, pIndex] = "evtitle orange";
            }
            else if ((EventTypeFunc.IsBaseCurrency(pType) || EventTypeFunc.IsQuotedCurrency(pType) || EventCodeAndEventTypeFunc.IsQuantityPeriodVariation(pCode, pType) ||
                EventTypeFunc.IsInitialMargin(pType) || EventCodeAndEventTypeFunc.IsMarginRequirementRatio(pCode, pType)) && pIsTypeChanged)
                itemTitle[0, pIndex] = pResType;
                else if ((EventTypeFunc.IsInitialMargin(pType) || EventTypeFunc.IsMarginRequirement(pType) ||
                    EventTypeFunc.IsMarketValue(pType) || EventTypeFunc.IsTotalMargin(pType)) && pIsTypeChanged)
                    itemTitle[0, pIndex] = pResType;
            else if (EventCodeFunc.IsOffsetting(pCode) && pIsTypeChanged)
            {
                itemTitle[0, pIndex] = pResCode + " - " + pResType;
                itemTitle[1, pIndex] = "evtitle red";
            }
            else if (EventCodeFunc.IsUnclearingOffsetting(pCode))
            {
                itemTitle[0, pIndex] = pResCode + " - " + pResType;
                itemTitle[1, pIndex] = "evtitle red";
            }
            else if (EventCodeFunc.IsOffSettingCorporateAction(pCode))
            {
                itemTitle[0, pIndex] = pResCode + " - " + pResType;
                itemTitle[1, pIndex] = "evtitle red";
            }
            else if (EventCodeFunc.IsPositionCancelation(pCode) && (pIsCodeChanged || pIsTypeChanged))
            {
                itemTitle[0, pIndex] = pResCode + " - " + pResType;
                itemTitle[1, pIndex] = "evtitle red";
            }
            else if (EventCodeFunc.IsPositionTransfer(pCode) && (pIsCodeChanged || pIsTypeChanged))
            {
                itemTitle[0, pIndex] = pResCode + " - " + pResType;
                itemTitle[1, pIndex] = "evtitle red";
            }
            else if ((EventCodeAndEventTypeFunc.IsRealizedMargin(pCode, pType) || EventCodeAndEventTypeFunc.IsUnrealizedMargin(pCode, pType)) && (pIsCodeChanged || pIsTypeChanged))
            {
                itemTitle[0, pIndex] = pResType;
                if (EventCodeFunc.IsOffsetting(pEventParentCode) ||
                    EventCodeFunc.IsPositionCancelation(pEventParentCode) ||
                    EventCodeFunc.IsUnclearingOffsetting(pEventParentCode) ||
                    EventCodeFunc.IsPositionTransfer(pEventParentCode) && (pIsCodeChanged || pIsTypeChanged))
                    itemTitle[1, pIndex] = "subevtitle red";
            }
            else if ((EventTypeFunc.IsFundingAmount(pType) || EventTypeFunc.IsBorrowingAmount(pType)) && pIsTypeChanged)
                itemTitle[0, pIndex] = pResType;
            else if (EventTypeFunc.IsEqualizationPayment(pType))
            {
                if (EventCodeFunc.IsOffSettingCorporateAction(pEventParentCode))
                {
                    itemTitle[0, pIndex] = pResType;
                    itemTitle[1, pIndex] = "subevtitle red";
                }
            }

        }
        #endregion SetItemTitle_RTS
        #endregion Methods
    }
	#endregion EventGrid
}
