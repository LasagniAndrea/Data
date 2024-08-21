#region using directives
using EFS.ACommon;
using EFS.Common.Web;
using EFS.Referential;
using EfsML.Enum.Tools;
using FpML.Interface;
using System;
using System.Collections;
using System.Data;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Web.UI;
using System.Web.UI.WebControls;
#endregion using directives

namespace EFS
{
    #region PanelEventAsset
    // EG 20180423 Analyse du code Correction [CA1405]
    [ComVisible(false)] 
	public class PanelEventAsset : HierarchicalPanel
	{
		#region Variables
		public EventAssetGrid dgEventAsset;
		#endregion Variables
		#region Constructors
        public PanelEventAsset() : this(null) { }
        public PanelEventAsset(object pProduct): base()
		{
            dgEventAsset = new EventAssetGrid(pProduct);
			this.Controls.Add(dgEventAsset);
		}
		#endregion Constructors
		#region OnPreRender
		protected override void OnPreRender(EventArgs e)
		{
            if (1 < this.dgEventAsset.InfoColumnLength)
			{
                this.Width = Unit.Percentage(100);
			}
        }
		#endregion OnPreRender
	}
	#endregion PanelEventAsset
	#region PanelEventDet
    // EG 20180423 Analyse du code Correction [CA1405]
    [ComVisible(false)] 
	public class PanelEventDet : HierarchicalPanel
	{
		#region Variables
		public EventDetailGrid dgEventDet;
		#endregion Variables
		#region Constructors
        public PanelEventDet() : this(null) { }
        public PanelEventDet(object pProduct): base()
        {
            dgEventDet = new EventDetailGrid(pProduct);
            this.Controls.Add(dgEventDet);
            this.Style[HtmlTextWriterStyle.Display] = "none";
        }
        #endregion Constructors
		#region OnPreRender
		protected override void OnPreRender(EventArgs e)
		{
            if (1 < this.dgEventDet.InfoColumnLength)
			{
                this.Width = Unit.Percentage(100);
			}
		}
		#endregion OnPreRender
	}
	#endregion PanelEventDet
    #region PanelEventFee
    // 20081030 EG Newness EventFee
    // EG 20180423 Analyse du code Correction [CA1405]
    [ComVisible(false)] 
    public class PanelEventFee : HierarchicalPanel
    {
        #region Variables
        public EventFeeGrid dgEventFee;
        #endregion Variables
        #region Constructors
        public PanelEventFee() : this(null) { }
        public PanelEventFee(object pProduct): base()
        {
            dgEventFee = new EventFeeGrid(pProduct);
            this.Controls.Add(dgEventFee);
            this.Style[HtmlTextWriterStyle.Display] = "none";
        }
        #endregion Constructors
        #region OnPreRender
        protected override void OnPreRender(EventArgs e)
        {
            if (1 < this.dgEventFee.InfoColumnLength)
            {
                this.Width = Unit.Percentage(100);
            }
        }
        #endregion OnPreRender
    }
    #endregion PanelEventFee
    #region PanelEventPricing
    // EG 20180423 Analyse du code Correction [CA1405]
    [ComVisible(false)] 
    public class PanelEventPricing : HierarchicalPanel
	{
		#region Variables
		public EventPricingGrid dgEventPricing;
		#endregion Variables
		#region Constructors
        public PanelEventPricing() : this(null) { }
        public PanelEventPricing(object pProduct): base()
		{
            dgEventPricing = new EventPricingGrid(pProduct);
			this.Controls.Add(dgEventPricing);

		}
		#endregion Constructors
		#region OnPreRender
		protected override void OnPreRender(EventArgs e)
		{
            if (1 < this.dgEventPricing.InfoColumnLength)
            {
                this.Width = Unit.Percentage(100);
            }
		}
		#endregion OnPreRender
	}
	#endregion PanelEventPricing
	#region EventDetGrid
    // EG 20180423 Analyse du code Correction [CA1405]
    // EG 20190716 [VCL : New FixedIncome] Upd
    [ComVisible(false)] 
	public class EventDetGrid : HierarchicalGrid
	{
		#region Members
		protected string      m_XmlFileName;
		protected ArrayList   m_InfoColumn;
		protected string      m_FamilyProduct;
		protected string      m_EventCode;
        protected string      m_EventType;
		protected DataRowView m_ParentDataRowView;
		#endregion Members
        #region Accessors
        public int InfoColumnLength
        {
            get 
            { 
                int nb = 0;
                if (null != m_InfoColumn)
                {
                    foreach (object item in m_InfoColumn)
                    {
                        if (null != item)
                        {
                            if (item.GetType().IsArray)
                            {
                                foreach (InfoColumn infoColumn in (InfoColumn[])item)
                                {
                                    if (StrFunc.IsFilled(infoColumn.columnValue))
                                        nb++;
                                }
                            }
                            else
                            {
                                InfoColumn infoColumn = (InfoColumn)item;
                                if (StrFunc.IsFilled(infoColumn.columnValue))
                                    nb++;
                            }
                        }
                    }
                }
                return nb; 
            }
        }
        public bool IsInfoColumnExist
        {
            get {return (0<InfoColumnLength);}
        }
        #endregion Accessors
        #region Constructors
        public EventDetGrid() : this(null){}
        public EventDetGrid(object pProduct) : base("Sub")
		{
            m_Product = pProduct;
			#region Grid properties
			this.ID                             = "hgEventDet";
			this.GridLines                      = GridLines.None;
			this.BackColor                      = Color.Transparent;
			this.AlternatingItemStyle.BackColor = Color.Transparent;
			this.ItemStyle.BackColor            = Color.Transparent;
			this.ShowHeader                     = false;
			#endregion Grid properties
			this.DataBinding += new System.EventHandler(OnDataBinding);
            this.Style[HtmlTextWriterStyle.Display] = "none";
		}
		#endregion Constructors
		#region Events
		#region OnDataBinding
		protected virtual void OnDataBinding(object sender, System.EventArgs e)
		{
			DataGridItem dgi    = (DataGridItem) this.BindingContainer;
			this.DataSource     = dgi.DataItem;
			m_ParentDataRowView = ((HierarchicalPanel)this.Parent).ParentDataRowView;
			if (null != m_ParentDataRowView)
			{
				m_FamilyProduct = m_ParentDataRowView["FAMILY"].ToString();
				m_EventCode		= m_ParentDataRowView["EVENTCODE"].ToString();
                m_EventType     = m_ParentDataRowView["EVENTTYPE"].ToString();
			}
            if (1 <= ((DataView)this.DataSource).Count)
            {
                m_InfoColumn = new ArrayList();
            }
		}
		#endregion OnDataBinding
		#region OnPreRender
		protected override void OnPreRender(EventArgs e)
		{
			if ((0 < this.Controls.Count) && (null != m_InfoColumn && 0 < m_InfoColumn.Count))
			{
				Table tbl         = (Table)this.Controls[0];
				DataGridItem dgi  = new DataGridItem(0,0,ListItemType.Item);
                TableCell cell = new TableCell
                {
                    ColumnSpan = tbl.Rows[0].Cells.Count
                };
                Table table = new Table
                {
                    CellSpacing = 0,
                    CellPadding = 2,
                    BorderStyle = BorderStyle.None,
                    GridLines = GridLines.None
                };
                foreach (object item in m_InfoColumn)
				{
					if (null != item)
					{
						if (item.GetType().IsArray)
						{
                            TableRow tr = new TableRow
                            {
                                CssClass = EFSCssClass.Event_Subevent
                            };
                            foreach (InfoColumn infoColumn in (InfoColumn[])item)
							{
								if (StrFunc.IsFilled(infoColumn.columnValue))
									SetInfoInRow(tr,infoColumn,infoColumn.columnSpan);
							}
                            if (0 < tr.Controls.Count)
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
                    table.Width = Unit.Percentage(100);
                    this.Style[HtmlTextWriterStyle.Display] = "block";
                    ((Panel)this.Parent).Style[HtmlTextWriterStyle.Display] = "block";
				}
			}
			base.OnPreRender(e);
		}
		#endregion OnPreRender
		#endregion Events
		#region Methods
        #endregion Methods
    }
	#endregion EventDetGrid
	#region EventAssetGrid
    // EG 20180423 Analyse du code Correction [CA1405]
    [ComVisible(false)] 
	public class EventAssetGrid : EventDetGrid
	{
		#region Constructors
        public EventAssetGrid() : this(null) { }
        public EventAssetGrid(object pProduct): base(pProduct)
		{
			this.ID                  = "hgEventAsset";
			this.ShowHeader          = true;
            m_XmlFileName = ReferentialTools.GetObjectXMLFile(Cst.ListType.Event, "EVENTASSET");
			this.LoadReferential(m_XmlFileName);
			this.DataBinding += new System.EventHandler(OnDataBinding);
		}
		#endregion Constructors
		#region Events
		#region OnDataBinding
		override protected void OnDataBinding(object sender, System.EventArgs e)
		{
			base.OnDataBinding(sender,e);
			this.DataMember   = "EventAsset";
			this.RelationName = "EventAsset";
		}
		#endregion OnDataBinding
		#region OnItemDataBound
        /// EG 20140120 Report v3.7
        /// EG 20150302 Add IDBC
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
                        m_InfoColumn.Add(GetInfoColumns(drv, "ASSETCATEGORY"));
                        m_InfoColumn.Add(GetInfoColumns(drv, "ID"));
                        m_InfoColumn.Add(GetInfoColumns(drv, "ISINCODE"));
                        m_InfoColumn.Add(GetInfoColumns(drv, "IDM"));
                        m_InfoColumn.Add(GetInfoColumn(drv, "IDBC", "BusinessCenter", true));
                        m_InfoColumn.Add(GetInfoColumns(drv, "PRIMARYRATESRC"));
                        m_InfoColumn.Add(GetInfoColumns(drv, "PRIMARYRATESRCPAGE"));
                        m_InfoColumn.Add(GetInfoColumns(drv, "PRIMARYRATESRCHEAD"));
                        m_InfoColumn.Add(GetInfoColumns(drv, "IDMARKETENV"));
                        m_InfoColumn.Add(GetInfoColumns(drv, "IDVALSCENARIO"));
                        m_InfoColumn.Add(GetInfoColumns(drv, "QUOTESIDE"));
                        m_InfoColumn.Add(GetInfoColumns(drv, "QUOTETIMING"));
                        m_InfoColumn.Add(GetInfoColumns(drv, "CLEARANCESYSTEM"));

                        m_InfoColumn.Add(GetInfoColumn(drv, "CONTRACTSYMBOL", "CONTRACT", true, "CATEGORY", "PUTORCALL", "STRIKEPRICE"));
                        m_InfoColumn.Add(GetInfoColumns(drv, "MATURITYDATE"));
                        m_InfoColumn.Add(GetInfoColumns(drv, "MATURITYDATESYS"));
                        m_InfoColumn.Add(GetInfoColumns(drv, "DELIVERYDATE"));
                        m_InfoColumn.Add(GetInfoColumn(drv, "NOMINAL", "IDC"));
                        m_InfoColumn.Add(GetInfoColumns(drv, "IDC"));
                        m_InfoColumn.Add(GetInfoColumns(drv, "WEIGHT", "UNITTYPEWEIGHT"));
                    }
				}
			}
		}
		#endregion OnItemDataBound
		#endregion Events
	}
	#endregion EventAssetGrid
	#region EventDetailGrid
    // EG 20180423 Analyse du code Correction [CA1405]
    [ComVisible(false)] 
	public class EventDetailGrid : EventDetGrid
	{
		#region Constructors
        public EventDetailGrid() : this(null) { }
        public EventDetailGrid(object pProduct)
            : base(pProduct)
        {
            this.ShowHeader = true;
            m_XmlFileName = ReferentialTools.GetObjectXMLFile(Cst.ListType.Event, "EVENTDET");
            this.LoadReferential(m_XmlFileName);
            this.DataBinding += new System.EventHandler(OnDataBinding);
        }
        #endregion Constructors
        #region Accessors
        #region IsFungible
        protected bool IsFungible
        {
            get
            {
                IProductBase product = m_Product as IProductBase;
                return product.IsFungible(SessionTools.CS);
            }
        }
        #endregion IsFungible

        #endregion Accessors
        #region Events
        #region OnDataBinding
        override protected void OnDataBinding(object sender, System.EventArgs e)
		{
			base.OnDataBinding(sender,e);
			this.DataMember   = "EventDet";
			this.RelationName = "EventDet";
		}
		#endregion OnDataBinding
		#region OnItemDataBound
        // EG 20171025 [23509] Add TZDLVY
        // EG 20190716 [VCL : New FixedIncome] Upd
        override protected void OnItemDataBound(DataGridItemEventArgs e)
        {
            base.OnItemDataBound(e);
            if ((e.Item.ItemType == ListItemType.Item) && (null != m_InfoColumn))
            {
                if (null != m_ParentDataRowView)
                {
                    DataGridItem dgi = (DataGridItem)e.Item;
                    DataRowView drv = (DataRowView)dgi.DataItem;

                    if (ProductBase.IsADM)
                    {
                        m_InfoColumn.Add(GetInfoColumns(drv, "RATE"));
                        m_InfoColumn.Add(GetInfoColumns(drv, "IDC_REF"));
                        m_InfoColumn.Add(GetInfoColumns(drv, "NOTIONALAMOUNT"));
                        m_InfoColumn.Add(GetInfoColumns(drv, "IDC1"));
                        m_InfoColumn.Add(GetInfoColumns(drv, "IDC2"));
                        m_InfoColumn.Add(GetInfoColumns(drv, "BASIS"));
                        m_InfoColumn.Add(GetInfoColumns(drv, "DTFIXING"));
                        m_InfoColumn.Add(GetInfoColumns(drv, "IDBC"));
                    }
                    else
                    {
                        m_InfoColumn.Add(GetInfoColumns(drv, "PCTRATE"));
                        m_InfoColumn.Add(GetInfoColumns(drv, "DTACTION"));
                        m_InfoColumn.Add(GetInfoColumns(drv, "NOTE"));

                        if (false == EventTypeFunc.IsAllMarketValue(m_EventType))
                        {
                            m_InfoColumn.Add(GetInfoColumns(drv, "NOTIONALREFERENCE"));
                            m_InfoColumn.Add(GetInfoColumns(drv, "INTEREST"));
                        }
                        m_InfoColumn.Add(GetInfoColumns(drv, "SPREAD"));
                        m_InfoColumn.Add(GetInfoColumns(drv, "MULTIPLIER"));
                        m_InfoColumn.Add(GetInfoColumns(drv, "DCF"));
                        m_InfoColumn.Add(GetInfoColumns(drv, "DCFNUM"));
                        m_InfoColumn.Add(GetInfoColumns(drv, "DCFDEN"));
                        m_InfoColumn.Add(GetInfoColumns(drv, "TOTALOFDAY"));
                        m_InfoColumn.Add(GetInfoColumns(drv, "TOTALOFYEAR"));

                        m_InfoColumn.Add(GetInfoColumns(drv, "FXTYPE"));
                        m_InfoColumn.Add(GetInfoColumns(drv, "IDC_REF"));
                        m_InfoColumn.Add(GetInfoColumns(drv, "IDC_BASE"));
                        m_InfoColumn.Add(GetInfoColumns(drv, "IDC1"));
                        m_InfoColumn.Add(GetInfoColumns(drv, "IDC2"));
                        m_InfoColumn.Add(GetInfoColumns(drv, "BASIS"));
                        if (false == EventTypeFunc.IsAllMarketValue(m_EventType))
                        {
                            m_InfoColumn.Add(GetInfoColumns(drv, "NOTIONALAMOUNT"));
                            m_InfoColumn.Add(GetInfoColumns(drv, "RATE"));
                        }
                        m_InfoColumn.Add(GetInfoColumns(drv, "SPOTRATE"));
                        m_InfoColumn.Add(GetInfoColumns(drv, "FWDPOINTS"));
                        m_InfoColumn.Add(GetInfoColumns(drv, "DTFIXING"));
                        m_InfoColumn.Add(GetInfoColumns(drv, "IDBC"));
                        m_InfoColumn.Add(GetInfoColumns(drv, "SETTLEMENTRATE"));
                        m_InfoColumn.Add(GetInfoColumns(drv, "STRIKEPRICE"));
                        m_InfoColumn.Add(GetInfoColumns(drv, "CONVERSIONRATE"));
                        m_InfoColumn.Add(GetInfoColumns(drv, "TOTALPAYOUTAMOUNT"));
                        m_InfoColumn.Add(GetInfoColumns(drv, "GAPRATE"));
                        m_InfoColumn.Add(GetInfoColumns(drv, "PERIODPAYOUT"));
                        m_InfoColumn.Add(GetInfoColumns(drv, "PCTPAYOUT"));

                        m_InfoColumn.Add(GetInfoColumn(drv, "CONTRACTMULTIPLIER", "CONTRACTSIZE", true));
                        m_InfoColumn.Add(GetInfoColumn(drv, "FACTOR", "ETDFactor", true));
                        m_InfoColumn.Add(GetInfoColumn(drv, "PRICE", "ETDPrice", true));
                        m_InfoColumn.Add(GetInfoColumn(drv, "CLOSINGPRICE", "ETDClosingPrice", true));
                        m_InfoColumn.Add(GetInfoColumn(drv, "QUOTETIMING"));
                        m_InfoColumn.Add(GetInfoColumn(drv, "ASSETMEASURE"));
                        if (ProductBase.IsDebtSecurityTransaction)
                        {
                            if (EventTypeFunc.IsAllMarketValue(m_EventType))
                            {
                                m_InfoColumn.Add(GetInfoColumn(drv, "QUOTEPRICE", "CleanPrice", true));
                                m_InfoColumn.Add(GetInfoColumn(drv, "QUOTEPRICE100", "DirtyPrice", true));
                                m_InfoColumn.Add(GetInfoColumn(drv, "RATE", "debtSecurityTransaction_CouponTitle", true));
                                m_InfoColumn.Add(GetInfoColumn(drv, "NOTIONALREFERENCE", "NOTIONALAMOUNT", true));

                                if (EventTypeFunc.IsMarketValue(m_EventType))
                                {
                                    m_InfoColumn.Add(GetInfoColumn(drv, "NOTIONALAMOUNT", "Report-MKPrincipalAmount", true));
                                    m_InfoColumn.Add(GetInfoColumn(drv, "INTEREST", "Report-MKAccruedInterestAmount", true));
                                }
                            }
                            else if (EventTypeFunc.IsUnrealizedMargin(m_EventType))
                            {
                                m_InfoColumn.Add(GetInfoColumn(drv, "NOTIONALAMOUNT", "Report-MKPrincipalAmount", true));
                                m_InfoColumn.Add(GetInfoColumn(drv, "INTEREST", "Report-MKAccruedInterestAmount", true));
                            }
                        }
                        else
                        {
                            m_InfoColumn.Add(GetInfoColumn(drv, "QUOTEPRICE", "ETDQuotePrice", true));
                        }
                        m_InfoColumn.Add(GetInfoColumn(drv, "QUOTEPRICEYEST", "ETDQuotePriceYest", true));
                        m_InfoColumn.Add(GetInfoColumn(drv, "SETTLTQUOTESIDE", "ETDSettlementQuoteSide", true));
                        m_InfoColumn.Add(GetInfoColumn(drv, "SETTLTQUOTETIMING", "ETDSettlementQuoteTiming", true));
                        m_InfoColumn.Add(GetInfoColumn(drv, "DTSETTLTPRICE", "ETDDtSettlementPrice", true));
                        m_InfoColumn.Add(GetInfoColumn(drv, "SETTLTPRICE", "ETDSettlementPrice", true));
                        m_InfoColumn.Add(GetInfoColumn(drv, "DAILYQUANTITY", "QUANTITY", true, "UNITDAILYQUANTITY"));
                        m_InfoColumn.Add(GetInfoColumn(drv, "PIP"));

                        /// EG 20161122 New Commodity Derivative
                        m_InfoColumn.Add(GetInfoColumn(drv, "DTDLVYSTART","DeliveryStart", true));
                        m_InfoColumn.Add(GetInfoColumn(drv, "DTDLVYEND", "DeliveryEnd", true));
                        m_InfoColumn.Add(GetInfoColumn(drv, "TZDLVY", "Timezone", true));

                        if (EventCodeFunc.IsFxHasActivatedStatus(m_EventCode))
                        {
                            HierarchicalGrid hg = (HierarchicalGrid)this.Parent.NamingContainer.NamingContainer;
                            m_InfoColumn.Add(hg.GetInfoColumn(m_ParentDataRowView, "IDSTTRIGGER"));
                        }
                    }
                }
            }
        }
		#endregion OnItemDataBound
		#endregion Events
	}
	#endregion EventDetailGrid
    #region EventFeeGrid
    // 20081030 EG Newness EventFee
    // EG 20180423 Analyse du code Correction [CA1405]
    [ComVisible(false)] 
    public class EventFeeGrid : EventDetGrid
    {
        #region Constructors
        public EventFeeGrid() : this(null) { }
        public EventFeeGrid(object pProduct)
            : base(pProduct)
        {
            this.ID = "hgEventFee";
            this.ShowHeader = true;
            m_XmlFileName = ReferentialTools.GetObjectXMLFile(Cst.ListType.Event, "EVENTFEE");
            this.LoadReferential(m_XmlFileName);
            this.DataBinding += new System.EventHandler(OnDataBinding);
        }
        #endregion Constructors
        #region Events
        #region OnDataBinding
        override protected void OnDataBinding(object sender, System.EventArgs e)
        {
            base.OnDataBinding(sender, e);
            this.DataMember = "EventFee";
            this.RelationName = "EventFee";
        }
        #endregion OnDataBinding
        #region OnItemDataBound
        override protected void OnItemDataBound(DataGridItemEventArgs e)
        {
            base.OnItemDataBound(e);
            if ((e.Item.ItemType == ListItemType.Item) && (null != m_InfoColumn))
            {
                DataGridItem dgi = (DataGridItem)e.Item;
                DataRowView drv = (DataRowView)dgi.DataItem;
                m_InfoColumn.Add(GetInfoColumns(drv, "STATUS"));
                m_InfoColumn.Add(GetInfoColumns(drv, "IDFEEMATRIX"));
                m_InfoColumn.Add(GetInfoColumns(drv, "IDFEE"));
                m_InfoColumn.Add(GetInfoColumns(drv, "IDFEESCHEDULE"));
                m_InfoColumn.Add(GetInfoColumns(drv, "FORMULA"));
                //PL 20141023
                //m_InfoColumn.Add(GetInfoColumns(drv, "ASSESSMENTBASISVALUE"));
                m_InfoColumn.Add(GetInfoColumns(drv, "ASSESSMENTBASISVALUE1"));
                m_InfoColumn.Add(GetInfoColumns(drv, "ASSESSMENTBASISVALUE2"));
                //EG [18076] Add
                m_InfoColumn.Add(SplitInfoColumns(drv, "ASSESSMENTBASISDET"));

                m_InfoColumn.Add(GetInfoColumns(drv, "BRACKET1"));
                m_InfoColumn.Add(GetInfoColumns(drv, "BRACKET2"));
                m_InfoColumn.Add(SplitInfoColumn(drv, "FORMULAVALUE1", "Parameters_Title", true));
                m_InfoColumn.Add(SplitInfoColumn(drv, "FORMULAVALUE2", "Parameters_Title", true));
                m_InfoColumn.Add(SplitInfoColumns(drv, "FORMULAVALUEBRACKET"));
                m_InfoColumn.Add(GetInfoColumns(drv, "FORMULADCF"));
                m_InfoColumn.Add(SplitInfoColumns(drv, "FORMULAMIN"));
                m_InfoColumn.Add(SplitInfoColumns(drv, "FORMULAMAX"));
                m_InfoColumn.Add(SplitInfoColumns(drv, "FEEPAYMENTFREQUENCY"));

                m_InfoColumn.Add(SplitInfoColumns(drv, "IDTAX"));
                m_InfoColumn.Add(SplitInfoColumns(drv, "IDTAXDET"));
                m_InfoColumn.Add(SplitInfoColumns(drv, "TAXTYPE"));
                m_InfoColumn.Add(SplitInfoColumns(drv, "TAXRATE"));
                m_InfoColumn.Add(SplitInfoColumns(drv, "TAXCOUNTRY"));
            }
        }
        #endregion OnItemDataBound
        #endregion Events
    }
    #endregion EventFeeGrid
	#region EventPricingGrid
    // EG 20180423 Analyse du code Correction [CA1405]
    [ComVisible(false)] 
	public class EventPricingGrid : EventDetGrid
	{
		#region Constructors
        public EventPricingGrid() : this(null) { }
        public EventPricingGrid(object pProduct)
            : base(pProduct)
        {
            this.ID = "hgEventPricing";
            this.ShowHeader = true;
            m_XmlFileName = ReferentialTools.GetObjectXMLFile(Cst.ListType.Event, "EVENTPRICING");
            this.LoadReferential(m_XmlFileName);
            this.DataBinding += new System.EventHandler(OnDataBinding);
        }
		#endregion Constructors
		#region Events
		#region OnDataBinding
		override protected void OnDataBinding(object sender, System.EventArgs e)
		{
			base.OnDataBinding(sender,e);
			this.DataMember   = "EventPricing";
			this.RelationName = "EventPricing";
		}
		#endregion OnDataBinding
		#region OnItemDataBound
		override protected void OnItemDataBound(DataGridItemEventArgs e)
		{
			base.OnItemDataBound(e);
			if ((e.Item.ItemType == ListItemType.Item) && (null != m_InfoColumn))
			{
				DataGridItem dgi = (DataGridItem)e.Item;
				DataRowView drv = (DataRowView)dgi.DataItem;

				foreach (ReferentialsReferentialColumn column in this.Referential.Column)
				{
                    if (false == column.IsHideInDataGrid)
					{
						m_InfoColumn.Add(GetInfoColumns(drv, column.ColumnName));
					}
				}
			}
		}
		#endregion OnItemDataBound
		#endregion Events
	}
	#endregion EventPricingGrid
}
