using EFS.ACommon;
using EFS.Common;
using EFS.Common.Web;
using EFS.Referential;
using FpML.Interface;
using System;
using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;


namespace EFS
{
    #region interface iChild
    public interface IChild
	{
		bool IsChildExist
		{
			get;
		}
	}
	#endregion interface iChild
	#region InfoColumn
	public class InfoColumn
	{
		public string          columnName;
		public string          columnValue;
		public string          dataType;
		public int             scale;
		public string          ressource;
		public int             columnSpan;
		public HorizontalAlign hAlign;

		public InfoColumn(){}
        public InfoColumn(ReferentialsReferentialColumn pColumn) : this(pColumn, pColumn.Ressource) { }
        public InfoColumn(ReferentialsReferentialColumn pColumn, string pRessource)
		{
            ressource = pRessource;
			columnName = pColumn.ColumnName;
			dataType   = pColumn.DataType.value;
			scale      = pColumn.ScaleSpecified?pColumn.Scale:0;
			if (pColumn.ExistsRelation)
			{
				ReferentialsReferentialColumnRelation relation = pColumn.Relation[0];
				if (StrFunc.IsFilled(relation.AliasTableName))
					columnName = relation.AliasTableName + "_" + relation.ColumnSelect[0].ColumnName;
				else
					columnName = relation.ColumnSelect[0].ColumnName;
				if (StrFunc.IsFilled(relation.ColumnSelect[0].Ressource))
					ressource = relation.ColumnSelect[0].Ressource;

				dataType = relation.ColumnSelect[0].DataType;
			}
			SetCellHAlign();
		}
		public InfoColumn(string pColumnName,string pDataType,string pRessource,string pColumnValue,int pScale,HorizontalAlign pHAlign)
		{
			columnName  = pColumnName;
			dataType    = pDataType;
			ressource   = pRessource;
			columnValue = pColumnValue;
			scale       = pScale;
			hAlign      = pHAlign;
		}
		#region Methods
		#region SetCellDataType
		public void SetCellDataType(DataRowView pDrv)
		{
				if (Convert.IsDBNull(pDrv[columnName]))
					columnValue = string.Empty;
				else
				{
					if (TypeData.IsTypeDate(dataType))
						columnValue = String.Format("{0:d}",pDrv[columnName].ToString());
                    else if (TypeData.IsTypeDateTime(dataType))
                        columnValue = String.Format("{0:G}",pDrv[columnName].ToString());
					else if (TypeData.IsTypeDec(dataType))
					{
						columnValue = Convert.ToDecimal(pDrv[columnName]).ToString("N" + scale);
						//20071122 PL
						if (scale > 0)
						{
							while (columnValue.EndsWith("0"))
							{
								columnValue = columnValue.Remove(columnValue.Length-1, 1);
							}
							if (columnName == "DCFDEN")
							{
								if (columnValue.EndsWith(System.Threading.Thread.CurrentThread.CurrentCulture.NumberFormat.NumberDecimalSeparator))
								{
									columnValue = columnValue.Remove(columnValue.Length-1, 1);
								}
							}
							else
							{
								int posNumberDecimalSeparator = columnValue.IndexOf(System.Threading.Thread.CurrentThread.CurrentCulture.NumberFormat.NumberDecimalSeparator);
								if (posNumberDecimalSeparator == columnValue.Length-1)
									columnValue += "00";
								else if (posNumberDecimalSeparator == columnValue.Length-2)
									columnValue += "0";
							}
						}
					}
                    else if (TypeData.IsTypeInt(dataType))
                    {
                        // EG 20150920 [21314] Int (int32) to Long (Int64) 
                        columnValue = Convert.ToInt64(pDrv[columnName]).ToString("N0");
                    }
                    else
                        columnValue = pDrv[columnName].ToString();
				}
		}
		#endregion SetCellDataType
		#region SetCellHAlign
		public void SetCellHAlign()
		{
			if (TypeData.IsTypeDate(dataType))
				hAlign = HorizontalAlign.Center;
			else if (TypeData.IsTypeDec(dataType))
				hAlign = HorizontalAlign.Right;
			else if (TypeData.IsTypeInt(dataType))
				hAlign = HorizontalAlign.Right;
			else
				hAlign = HorizontalAlign.Left;
		}
		#endregion SetCellHAlign
		#endregion Methods

	}
	#endregion InfoColumn
	#region Enum
	#region CachingBases
	/// Specifies whether the template filename shall be cached based on the name of the table of the
	/// data of a specific column
	public enum CachingBases
	{
		None,      /// Don't cache the filename of the template
		Tablename, /// Cache the filename of the template based on the tablename
		Column,    /// Cache the filename of the template based on the value in the column (specify the TemplateCachingColumn)
	}
	#endregion Enum: CachingBases
	#region LoadControlModes
	/// Specifies whether the template shall be loaded as a template using Page.LoadTemplate 
	/// or as a UserControl using Page.LoadControl
	public enum LoadControlModes
	{
		UserControl,     /// Specifies that the template shall be loaded as a UserControl using Page.LoadControl
		WebControl,      /// Specifies that the template shall be loaded as a WebControl
	}
	#endregion LoadControlModes
	#region TemplateDataModes
	/// Specifies whether to load one instance of the template for all child rows of a relation or to 
	/// load one instance of the template for each child row
	public enum TemplateDataModes
	{
				
		Table,     /// one template per table shall be loaded
		SingleRow, /// one template per row shall be loaded
	}
	#endregion TemplateDataModes
	#endregion Enum

	/// <summary>
	/// 
	/// </summary>
	/// FI 20200904 [25468] Add class
	public class DateTimeTzBoundColumn : BoundColumn
	{
		private readonly string _tzdbId;
		private readonly Cst.DataTypeDisplayMode _displayMode;
		/// <summary>
		/// 
		/// </summary>
		/// <param name="columnDataType"></param>
		public DateTimeTzBoundColumn(ReferentialsReferentialColumnDataType columnDataType) : base()
		{
			if (false == TypeData.IsTypeDateTime(columnDataType.value))
				throw new InvalidProgramException($"type:{columnDataType.value} not expected");
			else if (false == columnDataType.datakindSpecified || columnDataType.datakind != Cst.DataKind.Timestamp)
				throw new InvalidProgramException($"datakind:Timestamp expected");

			_tzdbId = columnDataType.tzdbidSpecified ? columnDataType.tzdbid : "Etc/UTC";
			_displayMode = columnDataType.displaySpecified ? columnDataType.display : Cst.DataTypeDisplayMode.Audit;
		}
		/// <summary>
		/// Affichage d'un horodatage
		/// </summary>
		/// <param name="dataValue"></param>
		/// <returns></returns>
		protected override string FormatDataValue(object dataValue)
		{
			if (dataValue == Convert.DBNull)
			{
				return base.FormatDataValue(dataValue);
			}
			else
			{
				DateTimeTz dtTz = new DateTimeTz(Convert.ToDateTime(dataValue), _tzdbId);
				if (!(_displayMode == Cst.DataTypeDisplayMode.Audit))
					throw new NotImplementedException($"displayMode:{_displayMode} not implemented");

				return DtFuncExtended.DisplayTimestampAudit(dtTz, new AuditTimestampInfo()
				{
					Collaborator = SessionTools.Collaborator,
					TimestampZone = SessionTools.AuditTimestampZone,
					Precision = SessionTools.AuditTimestampPrecision
				});
			}
		}
	}
	#region public class HierarchicalGrid
    // EG 20180423 Analyse du code Correction [CA1405]
    [ComVisible(false)]
	public class HierarchicalGrid : DataGrid, IPostBackDataHandler
	{
		#region Constants
		/// Name of the Hidden field that stores the ClientIDs of the expanded rows
		private const string EXPAND_CLIENTIDS_HIDDENFIELDNAME = "HGrid_ExpandIDs_";
		/// String used as a separator between the different ClientIDs in the hidden text field
		/// Note: when changing this value please update the JavaScripts too (hardcoded)
		internal const string EXPAND_CLIENTIDS_SEP = ", ";
		protected const string REGROUPCOLUMN = "~hide&colspan";
		#endregion Constants
		#region Variables
		private int                m_ColumnID               = -1;              /// Index of the HierarchicalGridColumn
		protected int              m_ContainerColumnID      = -1;              /// Index of the HierarchicalContainerGridColumn
		protected ArrayList        m_ContainerColumnName    = new ArrayList(); /// 
		private readonly Hashtable m_CachingTable           = new Hashtable(); /// Stores the cached filenames of the templates
		private readonly Hashtable m_CachingRelation        = new Hashtable(); /// Stores the cached relation of the templates
        private object m_CachingProduct = new object();
		//private readonly Hashtable m_TdmCachingTable        = new Hashtable(); /// Stores the cached template data modes
		private RowStates          m_RowExpand;
		private string             m_ExpandClientIDs        = String.Empty;
		private TemplateDataModes  m_TemplateDataMode       = TemplateDataModes.Table;
		private LoadControlModes   m_LoadControlMode        = LoadControlModes.WebControl;
		private CachingBases       m_TemplateCachingBase    = CachingBases.Tablename;
		private string             m_TemplateCachingColumn;
		private string             m_RelationName;

		protected ReferentialsReferential m_Referential;
		protected ArrayList aBoundColumnName;
        protected object m_Product;

		#endregion Variables
		#region Variables Events
		public event HierarchicalGridTemplateSelectionEventHandler         TemplateSelection;
		public event HierarchicalGridTemplateDataModeSelectionEventHandler TemplateDataModeSelection;
		#endregion Variables Events

		#region Accessors

        #region CloneRowHeader
        public TableRow CloneRowHeader
		{
			get
			{
				TableRow  row       = new TableRow();
                #region FixedColumn with icon
                TableCell cell = new TableCell
                {
                    Width = Unit.Pixel(30),
                    Text = Cst.HTMLSpace
                };
                row.Cells.Add(cell);
				#endregion FixedColumn with icon
				for (int i=1;i<this.Columns.Count;i++)
				{
					if (this.Columns[i].Visible)
					{
						if (REGROUPCOLUMN == this.Columns[i].HeaderText)
						{

							cell             = row.Cells[row.Cells.Count-1];
							cell.ColumnSpan  += cell.ColumnSpan==0?2:1;
							cell.Width       = Unit.Pixel(Convert.ToInt32(cell.Width.Value) + 
								Convert.ToInt32(this.Columns[i].HeaderStyle.Width.Value));
						}
						else
						{
                            cell = new TableCell
                            {
                                Width = this.Columns[i].HeaderStyle.Width,
                                Text = this.Columns[i].HeaderText
                            };
                            if (StrFunc.IsEmpty(cell.Text))
								cell.Text = Cst.HTMLSpace;
							cell.Wrap  = true;
							row.Cells.Add(cell);
						}
					}
				}
				return row;
			}
		}
		#endregion CloneRowHeader
		#region ColumnID
		public int ColumnID
		{
			get {return m_ColumnID;}
		}
		#endregion ColumnID
		#region ContainerColumnID
		public int ContainerColumnID
		{
			get {return m_ContainerColumnID;}
		}
		#endregion ContainerColumnID
		#region ContainerColumnName
		public HierarchicalColumnInfo ContainerColumnName
		{
			set {m_ContainerColumnName.Add(value);}
		}
		#endregion ContainerColumnName
		#region DataRowViewParent
		public DataRowView DataRowViewParent
		{
			get
			{
				DataRowView dataRowView = null;
                if (this.BindingContainer.GetType().Equals(typeof(DataGridItem)))
                {
                    DataGridItem dgi = (DataGridItem)this.BindingContainer;
                    DataGrid dg = (DataGrid)dgi.Parent.BindingContainer;
                    DataView dv = (DataView)dg.DataSource;
                    if (dv.Count > dgi.DataSetIndex)
                        dataRowView = (DataRowView)dv[dgi.DataSetIndex];
                }
                else
                    dataRowView = null;
				return dataRowView;
			}
		}
		#endregion DataRowViewParent

		#region ExpandClientIDs
		/// Specifies the ClientIDs of the expanded rows
		internal string ExpandClientIDs
		{
			get { return m_ExpandClientIDs; }
			set
			{
				m_ExpandClientIDs = value;
				//remove unnecessary EXPAND_CLIENTIDS_SEP from the beginning, the end and doubles in the middle
				m_ExpandClientIDs = m_ExpandClientIDs.Replace(EXPAND_CLIENTIDS_SEP + EXPAND_CLIENTIDS_SEP,EXPAND_CLIENTIDS_SEP);
				if(m_ExpandClientIDs.StartsWith(EXPAND_CLIENTIDS_SEP))
					m_ExpandClientIDs = m_ExpandClientIDs.Remove(0, EXPAND_CLIENTIDS_SEP.Length);
				if(m_ExpandClientIDs.EndsWith(EXPAND_CLIENTIDS_SEP))
					m_ExpandClientIDs = m_ExpandClientIDs.Substring(0, m_ExpandClientIDs.Length - EXPAND_CLIENTIDS_SEP.Length);
			}
		}
		#endregion ExpandedClientIDs
		#region GetContainerColumnID
		public int GetContainerColumnID(string pRelation)
		{
			foreach (HierarchicalColumnInfo columnInfo in m_ContainerColumnName)
			{
				if (columnInfo.relationName.Contains(pRelation))
					return columnInfo.pos;
			}
			return -1;
		}
		public HierarchicalColumnInfo GetContainerColumnInfo(string pRelation)
		{
			foreach (HierarchicalColumnInfo columnInfo in m_ContainerColumnName)
			{
				if (columnInfo.relationName.Contains(pRelation))
					return columnInfo;
			}
			return null;
		}
		#endregion GetContainerColumnID
		#region IsUserControl
		public bool IsUserControl
		{
			get {return LoadControlModes.UserControl == LoadControlMode;}
		}
		#endregion IsUserControl
		#region IsWebControl
		public bool IsWebControl
		{
			get {return LoadControlModes.WebControl == LoadControlMode;}
		}
		#endregion IsWebControl
		#region LoadControlMode
		/// Specifies whether the template shall be loaded as a template using Page.LoadTemplate 
		/// or as a UserControl using Page.LoadControl
		[DefaultValue(LoadControlModes.WebControl), Category("HierarchicalGrid")]
		public LoadControlModes LoadControlMode
		{
			get { return m_LoadControlMode; }
			set { m_LoadControlMode = value; }
		}
		#endregion LoadControlMode
        #region RelationName
        public IProductBase ProductBase
        {
            get { return m_Product as IProductBase; }
        }
        #endregion ProductBase
		#region RelationName
		public string RelationName
		{
			get {return m_RelationName;}
			set {m_RelationName = value;}
		}
		#endregion RelationName
		#region Referential
		public ReferentialsReferential Referential
		{
			get {return m_Referential;}
			set {m_Referential = value;}
		}
		#endregion Referential
		#region RemoveContainerColumnName
		public void RemoveContainerColumnName(string pRelation)
		{
			HierarchicalColumnInfo columnInfo = GetContainerColumnInfo(pRelation);
			m_ContainerColumnName.Remove(columnInfo);
		}
		#endregion GetContainerColumnID

		#region RowExpand
		/// Specifies whether a Row is expanded or collapsed
		/// Please note that this property is only available after the Items collection is populated (usually after DataBind)
		[Category("HierarchicalGrid"), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public RowStates RowExpand
		{
			get
			{
				EnsureChildControls();
				if(m_RowExpand == null)
					m_RowExpand = new RowStates(this);
				return m_RowExpand;
			}
		}
		#endregion RowExpanded
		#region TemplateCachingBase
		/// Specifies whether the template filename shall be cached based on the name of the table or the
		/// data of a specific column
		[DefaultValue(CachingBases.None), Category("HierarchicalGrid")]
		public CachingBases TemplateCachingBase
		{
			get { return m_TemplateCachingBase; }
			set { m_TemplateCachingBase = value; }
		}
		#endregion TemplateCachingBase
		#region TemplateCachingColumn
		/// Specifies the column name that is the basis for template filename caching
		[Category("HierarchicalGrid")]
		public string TemplateCachingColumn
		{
			get { return m_TemplateCachingColumn; }
			set { m_TemplateCachingColumn = value; }
		}
		#endregion TemplateCachingColumn
		#region TemplateDataMode
		/// Specifies whether to load one instance of the template for all child rows of a relation or to 
		/// load one instance of the template for each child row or to defer the decision until run time.
		/// You can use TemplateDataModes.Table if you want to display a DataGrid for multiple child rows - the
		/// BindingContainer contains a DataSet
		/// You can use TemplateDataModes.SingleRow if you want to display the templates multiple times for each child row
		/// You can use TemplateDataModes.RunTime if you want to defer the decision of which template data mode to use
		/// until runtime and base it on the particular data relation that is currently being displayed.
		[DefaultValue(TemplateDataModes.SingleRow), Category("HierarchicalGrid")]
		public TemplateDataModes TemplateDataMode
		{
			get { return m_TemplateDataMode; }
			set { m_TemplateDataMode = value; }
		}
		#endregion TemplateDataMode
		#endregion Accessors
		#region Constructor
        public HierarchicalGrid():this(string.Empty){}

		// EG 20200724 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
		public HierarchicalGrid(string pCssPrefix)
		{
			#region DataGrid Properties
			#region PagerStyle
			PagerStyle.Mode               = PagerMode.NumericPages;
			PagerStyle.CssClass = "DataGrid_PagerStyle";
			PagerStyle.PageButtonCount    = 10;
			PagerStyle.Visible            = true;
			PagerStyle.HorizontalAlign    = HorizontalAlign.Left;
			#endregion PagerStyle
			#region HeaderStyle
			ShowHeader                    = true;
			HeaderStyle.CssClass = "DataGrid_HeaderStyle";
			HeaderStyle.Wrap              = true;
			#endregion HeaderStyle
			#region EditItemStyle
			EditItemStyle.CssClass = "DataGrid_SelectedItemStyle";
			EditItemStyle.CssClass        = EFSCssClass.Capture;
			#endregion EditItemStyle
			#region ItemStyle
			ItemStyle.CssClass = "DataGrid_ItemStyle";
			ItemStyle.Wrap                = true;
			#endregion ItemStyle
			#region AlternatingItemStyle
			AlternatingItemStyle.CssClass = "DataGrid_ItemStyle";
			AlternatingItemStyle.Wrap     = true;
			#endregion AlternatingItemStyle
			#region SelectedItemStyle
			SelectedItemStyle.CssClass = "DataGrid_SelectedItemStyle";
			SelectedItemStyle.Wrap        = true;
			#endregion SelectedItemStyle
			#region FooterStyle
			ShowFooter                    = false;
			FooterStyle.HorizontalAlign   = HorizontalAlign.Left;
			FooterStyle.CssClass = "DataGrid_FooterStyle";
			#endregion FooterStyle
			this.CreateControlStyle();
			#region Other settings
			AllowPaging                   = false;
			AllowSorting                  = false;
			CellSpacing                   = 0;
			CellPadding                   = 2;
            if (StrFunc.IsEmpty(pCssPrefix)) // C'est nul
			    Width                     = Unit.Percentage(100);
			CssClass = "DataGrid";
			AutoGenerateColumns           = false;

			this.ID = "divDtg";

			#endregion Other settings
			this.ItemCreated += new DataGridItemEventHandler(OnItemCreated);
			#endregion DataGrid Properties
		}
		#endregion Constructor
		#region Event
		#region OnItemCreated
		public void OnItemCreated(Object sender, DataGridItemEventArgs e)
		{
			#region Header
			ListItemType itemType = e.Item.ItemType;
			if (itemType == ListItemType.Header)
			{
				int removedCol          = 0;
				double removedColdWidth = 0;
				bool isColSpanColumn = false;
				for (int i=Columns.Count-1 ; i >= 1; i--)
				{
					TableCell cell = e.Item.Cells[i];
					if(true == AllowSorting)
					{
						if (0 < cell.Controls.Count)
						{
							Control ctrlToTest = cell.Controls[0];
							isColSpanColumn    = (((LinkButton)ctrlToTest).Text == REGROUPCOLUMN);
						}
					}
					else 
						isColSpanColumn    = (cell.Text == REGROUPCOLUMN);

					if (isColSpanColumn)
					{
						cell.Visible = false;
						if (Columns[i].Visible)
						{
							removedColdWidth += Columns[i].HeaderStyle.Width.Value;
							removedCol ++;
						}
					}
					else if (0 != removedCol)
					{
						cell.ColumnSpan     = 1 + removedCol;
						removedColdWidth   += Columns[i].HeaderStyle.Width.Value;
						cell.Width          = Unit.Pixel(Convert.ToInt32(removedColdWidth));
						removedCol          = 0;
						removedColdWidth    = 0;
					}
				}
				//e.Item.Cells[0].Width = Unit.Pixel(30);
			}  
			else if (itemType == ListItemType.Item || itemType == ListItemType.AlternatingItem || itemType == ListItemType.SelectedItem)
			{
				TableCell cell = e.Item.Cells[e.Item.Cells.Count-1];
				cell.Text = Cst.HTMLSpace;
			}


			#endregion Header
		}
		#endregion OnItemCreated
		#region OnItemDataBound
		/// Override handling to display the tables with a relation defined to the DataSource (aka child tables)
		protected override void OnItemDataBound(DataGridItemEventArgs e)
		{
            if (e.Item.ItemType == ListItemType.Header)
            {
                for (int i = Columns.Count - 1; i >= 0; i--)
                {
                    TableCell cell = e.Item.Cells[i];
                    string header = cell.Text;
                    if (header.IndexOf("<brnobold/>") > 0)
                    {
                        int pos_brnobold = header.IndexOf("<brnobold/>");
                        string text_nobold = cell.Text.Substring(pos_brnobold);
                        Label label_bold = new Label
                        {
                            Text = cell.Text.Substring(0, pos_brnobold)
                        };
                        label_bold.Style.Add(HtmlTextWriterStyle.FontWeight, "bold");
                        cell.Controls.Add(label_bold);

                        Label label_nobold = new Label
                        {
                            Text = Cst.HTMLBreakLine + text_nobold
                        };
                        label_nobold.Style.Add(HtmlTextWriterStyle.FontWeight, "normal");
                        cell.Controls.Add(label_nobold);
                    }
                }
            }

			if (e.Item.ItemType == ListItemType.Item || 
				e.Item.ItemType == ListItemType.AlternatingItem || 
				e.Item.ItemType == ListItemType.SelectedItem)
			{
				DataRowView drv = e.Item.DataItem as DataRowView;

				//works only with a DataTable
				if(drv != null)
					DisplayRelatedTables(e.Item, drv);

				//set back the original data item
				e.Item.DataItem = drv;


                #region Column RowState
                try
                {
                    string rowState = string.Empty;
                    if (drv.DataView.Table.Columns.Contains("ROWSTATE"))
                        rowState = (null != drv["ROWSTATE"] ? drv["ROWSTATE"].ToString() : string.Empty);
                    //
                    if (StrFunc.IsFilled(rowState))
                    {
                        // En supposant que la colonne "ROWSTATE" est en dérnière position des colonnes dites "ColumnAction" ( voir AddColumnAction())
                        TableCell cellRowState = e.Item.Cells[0];
                        string[] arrRowState = rowState.Split("+".ToCharArray());
                        for (int i = 0; i < arrRowState.Length; i++)
                        {
                            if (StrFunc.IsFilled(arrRowState[i]))
                            {
                                Control rowStateCtrl = null;
                                string rowStateCtrlAttributes = string.Empty;
                                //
                                bool isWebdings = arrRowState[i].StartsWith("webdings=");
                                bool isWingdings = arrRowState[i].StartsWith("wingdings=");
                                bool isImage = arrRowState[i].StartsWith("img=");
                                bool isLabelBackground = arrRowState[i].StartsWith("lblbkg=");
                                bool isButtonBackground = arrRowState[i].StartsWith("btnbkg=");
                                //
                                if (isWebdings)
                                {
                                    Label lblRowState = new Label();
                                    lblRowState.Font.Name = "webdings";
                                    rowStateCtrl = lblRowState;
                                    rowStateCtrlAttributes = arrRowState[i].TrimStart("webdings=".ToCharArray());
                                }
                                else if (isWingdings)
                                {
                                    Label lblRowState = new Label();
                                    lblRowState.Font.Name = "wingdings";
                                    rowStateCtrl = lblRowState;
                                    rowStateCtrlAttributes = arrRowState[i].TrimStart("wingdings=".ToCharArray());
                                }
                                else if (isImage)
                                {
                                    WCToolTipPanel imgRowState = new WCToolTipPanel();
                                    rowStateCtrl = imgRowState;
                                    rowStateCtrlAttributes = arrRowState[i].TrimStart("img=".ToCharArray());
                                }
                                else if (isLabelBackground)
                                {
                                    WCTooltipLabel lblRowState = new WCTooltipLabel();
                                    rowStateCtrl = lblRowState;
                                    rowStateCtrlAttributes = arrRowState[i].TrimStart("lblbkg=".ToCharArray());
                                }
                                else if (isButtonBackground)
                                {
                                    WCToolTipButton lblRowState = new WCToolTipButton();
                                    rowStateCtrl = lblRowState;
                                    rowStateCtrlAttributes = arrRowState[i].TrimStart("btnbkg=".ToCharArray());
                                }

                                if (null != rowStateCtrl)
                                {
                                    rowStateCtrlAttributes = rowStateCtrlAttributes.Trim(@"\''".ToCharArray());
                                    string[] arrRowStateCtrlAttributes = rowStateCtrlAttributes.Split(";".ToCharArray());
                                    for (int j = 0; j < arrRowStateCtrlAttributes.Length; j++)
                                    {
                                        if (StrFunc.IsFilled(arrRowStateCtrlAttributes[j]))
                                        {
                                            string[] arrAttributeKeyValue = arrRowStateCtrlAttributes[j].Split(":".ToCharArray());
                                            string attributeKey = arrAttributeKeyValue[0].Trim(Cst.Cr.ToCharArray()).Trim(@"\".ToCharArray());
                                            //
                                            if (StrFunc.IsFilled(attributeKey))
                                            {
                                                string attributeValue = arrAttributeKeyValue[1].Trim(Cst.Cr.ToCharArray()).Trim(@"\".ToCharArray());
                                                //
                                                if (isWebdings || isWingdings)
                                                {
                                                    Label lblRowState = (Label)rowStateCtrl;
                                                    if ("code" == attributeKey)
                                                        lblRowState.Text = attributeValue;
                                                    else if ("title" == attributeKey)
                                                        lblRowState.ToolTip = attributeValue;
                                                    else
                                                        lblRowState.Style.Value = lblRowState.Style.Value + ";" + arrRowStateCtrlAttributes[j];
                                                }
                                                else if (isImage)
                                                {
                                                    WCToolTipPanel imgRowState = rowStateCtrl as WCToolTipPanel;
                                                    if ("src" == attributeKey)
                                                    {
                                                        imgRowState.CssClass = CSS.SetCssClass(attributeValue);
                                                        imgRowState.Style.Add(HtmlTextWriterStyle.MarginLeft, "2px");
                                                        imgRowState.Style.Add(HtmlTextWriterStyle.VerticalAlign, "middle");
                                                    }
                                                    else
                                                    {
                                                        if ("title" == attributeKey)
                                                            attributeValue = ACommon.Ressource.GetString(attributeValue, true);
                                                        else if ("alt" == attributeKey)
                                                            attributeValue = ACommon.Ressource.GetString(attributeValue, true);
                                                        imgRowState.Attributes.Add(attributeKey, attributeValue);
                                                    }
                                                }
                                                else if (isLabelBackground)
                                                {
                                                    if (rowStateCtrl is WCTooltipLabel lblRowState)
                                                    {
                                                        if ("title" == attributeKey)
                                                            attributeValue = ACommon.Ressource.GetString(attributeValue, true);
                                                        else if ("alt" == attributeKey)
                                                            attributeValue = ACommon.Ressource.GetString(attributeValue, true);
                                                        lblRowState.Attributes.Add(attributeKey, attributeValue);
                                                    }
                                                }
                                                else if (isButtonBackground)
                                                {
                                                    if (rowStateCtrl is WCToolTipButton btnRowState)
                                                    {
                                                        if ("title" == attributeKey)
                                                            btnRowState.Pty.TooltipTitle = Ressource.GetString(attributeValue, true);
                                                        else if ("alt" == attributeKey)
                                                            btnRowState.Pty.TooltipContent = Ressource.GetString(attributeValue, true);
                                                        btnRowState.Attributes.Add(attributeKey, attributeValue);
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    cellRowState.Controls.Add(rowStateCtrl);
                                }
                            }

                        }
                    }
                }
                catch (Exception ex)
                {
                    throw new SpheresException2(MethodInfo.GetCurrentMethod().Name,
                        "Unexpected error for column Raw State", ex);
                }

                #endregion Column RowState

                #region Apply RowStyle
                try
                {
                    string rowStyle = string.Empty;
                    if (drv.DataView.Table.Columns.Contains("ROWSTYLE"))
                        rowStyle = (null != drv["ROWSTYLE"] ? drv["ROWSTYLE"].ToString() : string.Empty);
                    //
                    if (StrFunc.IsFilled(rowStyle))
                    {
                        if ("class" == Referential.SQLRowStyle.type)
                            e.Item.CssClass = rowStyle;
                        else if ("style" == Referential.SQLRowStyle.type)
                            e.Item.Style.Value = rowStyle;
                    }
                }
                catch (Exception ex)
                {
                    throw new SpheresException2(MethodInfo.GetCurrentMethod().Name, "Unexpected error when applying style", ex);
                }
                #endregion  Apply RowStyle
			}
			base.OnItemDataBound(e);
		}
		#endregion OnItemDataBound
		#region OnPreRender
		protected override void OnPreRender(EventArgs e)
		{
			base.OnPreRender(e);

			//register on the page that LoadPostData is being called
			Page.RegisterRequiresPostBack(this);

			//specifies the name of the hidden field that contains the ClientIDs of the expanded rows
			//adds an attribute to the HierarGrid itself
			//register the hidden field
			string hiddenFieldName = EXPAND_CLIENTIDS_HIDDENFIELDNAME + this.ClientID;
			this.Attributes.Add("Expand_ClientIDs", hiddenFieldName);
            // EG 20160404 Migration vs2013
            //Page.RegisterHiddenField(hiddenFieldName, ExpandClientIDs);
            Page.ClientScript.RegisterHiddenField(hiddenFieldName, ExpandClientIDs);
			//register the client script from the embedded resource file (once for all HierarchicalGrids on page)
			#region HierarchicalGrid Javascript registration
			JavaScript.RegisterManifestResource(this.Page,"Javascript.HierarchicalGrid.js","HierarchicalGrid",false);
			#endregion HierarchicalGrid Javascript registration

			#region Startup Javascript registration
			//register the startup client script from the embedded resource file (once per HierarchicalGrids)
			JavaScript.RegisterManifestResource(this.Page,"Javascript.Startup.js","Startup",true,"HiddenFieldName",hiddenFieldName);
			#endregion Startup Javascript registration
		}
		#endregion OnPreRender
		#region OnTemplateDataModeSelection
		/// Occurs when the TemplateDataMode is set to runtime and HierarchicalGrid finds a new DataRelation
		/// for which it needs to determine the TemplateDataMode to use.
		/// You can use HierarchicalGridTemplateDataModeSelectionEventArgs.Relation to define which data mode the Hierarchicalgrid
		/// will use to load the particular template for the DataRelation.
		/// return the template data mode in the HierarchicalGridTemplateDataModeSelectionEventArgs.TemplateDataMode property
		/// Raises the TemplateDataModeSelection event
		/// returns true if at least one event handler is attached
		protected virtual bool OnTemplateDataModeSelection(HierarchicalGridTemplateDataModeSelectionEventArgs e)
		{
			if(TemplateDataModeSelection != null)
			{
				TemplateDataModeSelection(this, e);
				return true;
			}
			else
				return false;
		}
		#endregion OnTemplateDataModeSelection
		#region OnTemplateSelection
		/// <summary>
		/// Occurs when the HierarchicalGrid finds a new child row for which there is no template filename in the cache table
		/// You can use HierarGridTemplateSelectionEventArgs.Row to define which template shall be loaded and
		/// return the filename in the HierarchicalGridTemplateSelectionEventArgs.TemplateFilename property
		/// Raises the TemplateSelection event
		/// returns true if at least one event handler is attached
		protected virtual bool OnTemplateSelection(HierarchicalGridTemplateSelectionEventArgs e)
		{
			if(TemplateSelection != null)
			{
				TemplateSelection(this, e);
				return true;
			}
			else
				return false;
		}
		#endregion OnTemplateSelection
		#endregion Event

		#region Methods
		#region public AddBoundColumn
		public static BoundColumn AddBoundColumn(ReferentialsReferentialColumn pColumn)
		{
			pColumn.Ressource = (StrFunc.IsFilled(pColumn.Ressource) ? pColumn.Ressource : string.Empty);

			BoundColumn bc;
			// FI 20200904 [25468]  new DateTimeTzBoundColumn si datakind timestamp
			if (TypeData.IsTypeDateTime(pColumn.DataType.value) && pColumn.DataType.datakindSpecified && pColumn.DataType.datakind == Cst.DataKind.Timestamp)
				bc = new DateTimeTzBoundColumn(pColumn.DataType);
			else
				bc = new BoundColumn();
			
            if (StrFunc.IsEmpty(pColumn.Ressource) && (false == pColumn.IsHideInDataGrid))
				bc.HeaderText = REGROUPCOLUMN;
			else
                //bc.HeaderText = Ressource.GetString(pColumn.Ressource,null,true);
                bc.HeaderText = Ressource.GetMulti(pColumn.Ressource,1);

			bc.DataField      = pColumn.ColumnName;
			bc.SortExpression = bc.DataField;
            bc.Visible = (false == pColumn.ExistsRelation) && (false == pColumn.IsHideInDataGrid);
			bc.ItemStyle.Wrap = true;
			bc.HeaderStyle.Wrap = true;

			if (pColumn.GridWidthSpecified)
			{
				//bc.HeaderStyle.Width = Unit.Pixel(pColumn.GridWidth);
				//bc.ItemStyle.Width   = Unit.Pixel(pColumn.GridWidth);

                if (pColumn.GridWidth < 0)
                {
                    bc.HeaderStyle.Width = Unit.Percentage(Math.Abs(pColumn.GridWidth));
                    bc.ItemStyle.Width = Unit.Percentage(Math.Abs(pColumn.GridWidth));
                }
                else
                {
                    bc.HeaderStyle.Width = Unit.Pixel(pColumn.GridWidth);
                    bc.ItemStyle.Width = Unit.Pixel(pColumn.GridWidth);
                }
			}
			return bc;
		}
		#endregion AddBoundColumn
		#region public AddBoundColumnRelation
		public static BoundColumn AddBoundColumnRelation(ReferentialsReferentialColumn pColumn)
		{
			BoundColumn bc    = new BoundColumn();
			ReferentialsReferentialColumnRelation relation = pColumn.Relation[0];
			if (StrFunc.IsFilled(relation.AliasTableName))
				bc.DataField  = relation.AliasTableName + "_" + relation.ColumnSelect[0].ColumnName;
			else
				bc.DataField  = relation.ColumnSelect[0].ColumnName;
			if (StrFunc.IsEmpty(relation.ColumnSelect[0].Ressource))
				bc.HeaderText = REGROUPCOLUMN;
			else
				bc.HeaderText = Ressource.GetString(relation.ColumnSelect[0].Ressource, true);
			bc.SortExpression = bc.DataField;
            bc.Visible = (false == pColumn.IsHideInDataGrid);
			if (pColumn.GridWidthSpecified)
			{
				bc.HeaderStyle.Width = Unit.Pixel(pColumn.GridWidth);
				bc.ItemStyle.Width   = Unit.Pixel(pColumn.GridWidth);
			}
			return bc;							
		}
		#endregion AddBoundColumnRelation
		#region public AddRowTitle
		public TableRow AddRowTitle(string pTitle)
		{
			TableRow  row       = new TableRow();
			TableCell cell      = new TableCell();
			int columnSpan      = 1;
			cell.Text           = pTitle;
			for (int i=1;i<this.Columns.Count;i++)
			{
				if (this.Columns[i].Visible)
					columnSpan  += 1;
			}
			cell.Wrap       = true;
			cell.ColumnSpan = columnSpan;
			row.Cells.Add(cell);
			return row;
		}
		#endregion AddRowTitle
		#region protected  CreateColumnSet
		/// Overriding the implementation of the DataGrid to add the additional HierarColumn 
		/// if not already existing
		protected override ArrayList CreateColumnSet(PagedDataSource pSource, bool pIsDataSourceUsed)
		{
			ArrayList aColumn = base.CreateColumnSet(pSource, pIsDataSourceUsed);
			#region ColumnID
			m_ColumnID   = -1;
			for(int i=0;i<aColumn.Count;i++)
			{
				if(aColumn[i].GetType().Equals(typeof(HierarchicalColumn)))
				{
					m_ColumnID = i;
					break;
				}
			}

			if(-1 == m_ColumnID)
			{
				aColumn.Insert(0, new HierarchicalColumn());
				if(null != HttpContext.Current)
					this.Columns.AddAt(0, new HierarchicalColumn());
				m_ColumnID = 0;
			}
			#endregion ColumnID
			#region ContainerColumnName
			int nbContainerColumn  = 0;
			if (0 < m_ContainerColumnName.Count)
			{
				for (int i=0;i<aColumn.Count;i++)
				{
					if (aColumn[i].GetType().Equals(typeof(HierarchicalContainerColumn)))
						nbContainerColumn++;
					if (nbContainerColumn == m_ContainerColumnName.Count)
						break;
				}


				while (nbContainerColumn < m_ContainerColumnName.Count)
				{
					HierarchicalColumnInfo hColumnInfo = (HierarchicalColumnInfo)m_ContainerColumnName[nbContainerColumn];
                    HierarchicalContainerColumn hContainerColumn = new HierarchicalContainerColumn
                    {
                        HeaderText = hColumnInfo.headerText
                    };

                    if (-1 != hColumnInfo.pos)
					{
						aColumn.Insert(hColumnInfo.pos,hContainerColumn);
					}
					else
					{
						aColumn.Add(hContainerColumn);
						hColumnInfo.pos = (aColumn.Count - 1);
					}
					if(null != HttpContext.Current)
					{
						this.Columns.AddAt(hColumnInfo.pos, hContainerColumn);
						if (hColumnInfo.widthSpecified)
						{
							DataGridColumn column = this.Columns[hColumnInfo.pos];
							column.HeaderStyle.Width = Unit.Pixel(hColumnInfo.width);
							column.ItemStyle.Width   = Unit.Pixel(hColumnInfo.width);
						}
					}
					nbContainerColumn++;
				}
			}
			#endregion ContainerColumnName
			return aColumn;
		}
		#endregion CreateColumnSet
		#region protected DisplayRelatedTables
		/// Loops over all the child relations of the current table and displays for each row in each child table
		/// the appropriate template
		/// <param name="item">DataGridItem that has just been databound</param>
		/// <param name="drv">DataRowView belonging to the DataGridItem item</param>
        // EG 20180423 Analyse du code Correction [CA2200]
        protected virtual void DisplayRelatedTables(DataGridItem pItem, DataRowView pDrv)
		{
			Panel pnl = null;
			try
			{
				DataRelationCollection dataRelations = pDrv.DataView.Table.ChildRelations;

				if (dataRelations.Count > 0)
				{
					//search the reference to the panel created by the HierarchicalColumn
					HierarchicalPlaceHolder hph = (HierarchicalPlaceHolder) pItem.Cells[m_ColumnID].FindControl("HPH");
					foreach (DataRelation dataRelation in dataRelations)
					{
						DataRow[] dataRows = pDrv.Row.GetChildRows(dataRelation);

						if (0 != dataRows.Length)
						{
							// create the panel that contains all the child templates
							string templateFilename = null;
							string relationName     = null;
                            object product = null;
                            GetTemplateFilename(dataRows[0], ref templateFilename, ref relationName, ref product);
							if(null == pnl)
							{
								pnl = new Panel();
								if (dataRelation.ExtendedProperties.ContainsKey("Container") && 
									Convert.ToBoolean(dataRelation.ExtendedProperties["Container"]))
								{
									pnl.ID = "_pnl" + relationName;
									m_ContainerColumnID = GetContainerColumnID(relationName);
                                    if (-1 < m_ContainerColumnID)
                                        pItem.Cells[m_ContainerColumnID].Controls.Add(pnl);
								}
								else
								{
									pnl.ID = "pnl";
									hph.Controls.Add(pnl);
									pnl.Attributes.Add("style", "display:none");
								}
								pnl.CssClass = "HierarchicalGrid_" + pItem.ItemType.ToString();
							}
							//either load one template for all child rows of a related table
							if (StrFunc.IsFilled(templateFilename))
							{
								if (TemplateDataModes.Table == TemplateDataMode)
									#region TemplateMode.Table
									LoadTemplateTable(pnl, templateFilename,relationName,product, pItem, dataRows);
									#endregion TemplateMode.Table
								else
								{
									#region TemplateMode.SingleRow
									for(int i=0;i<dataRows.Length;i++)
									{
										DataRow dataRow = dataRows[i];
										LoadTemplateSingleRow(i,pnl, templateFilename,relationName, pItem, dataRow);
									}
									#endregion TemplateMode.SingleRow
								}
							}
							else
								pnl = null;

							if (dataRelation.ExtendedProperties.ContainsKey("Container") && 
								Convert.ToBoolean(dataRelation.ExtendedProperties["Container"]))
								pnl = null;
						}
					}
				}
				//if no child rows exist, hide the plus-minus icon
                Control ctrlExpand = pItem.Cells[m_ColumnID].FindControl("Expand");
				if (null == pnl)
				{
                    if (null != ctrlExpand)
                        ctrlExpand.Visible = false;
				}
				else if (0 < pnl.Controls.Count)
				{
					bool isChildExist = true;
					if (0 < pnl.Controls[0].Controls.Count)
					{
						Type tInterface = pnl.Controls[0].Controls[0].GetType().GetInterface("iChild");
						if (null != tInterface)
						{
							PropertyInfo pty = tInterface.GetProperty("IsChildExist");
							if (null != pty)
								isChildExist = (bool) tInterface.InvokeMember(pty.Name,BindingFlags.GetProperty,null,pnl.Controls[0].Controls[0],null);
						}
					}
                    if (null != ctrlExpand)
                        ctrlExpand.Visible = isChildExist;
				}
			}
			catch (Exception)
			{
				throw;
			}
		}
		#endregion DisplayRelatedTables
		#region public GetInfoColumn
        public InfoColumn GetInfoColumn(DataRowView pDrv, string pColumnName, params string[] pOtherColumnName)
        {
            return GetInfoColumn(pDrv, 1, pColumnName, null, false, pOtherColumnName);
        }

        public InfoColumn GetInfoColumn(DataRowView pDrv, string pColumnName, string pRessource, bool pIsUserRessource, params string[] pOtherColumnName)
		{
            return GetInfoColumn(pDrv, 1, pColumnName, pRessource,pIsUserRessource, pOtherColumnName);
		}
        public InfoColumn GetInfoColumn(DataRowView pDrv, int pColumnSpan, string pColumnName, string pRessource, bool pIsUserRessource, params string[] pOtherColumnName)
		{
			InfoColumn infoColumn = null;
			ReferentialsReferentialColumn column = Referential[pColumnName];
			if (null != column)
			{
                if (pIsUserRessource && StrFunc.IsFilled(pRessource))
                    infoColumn = new InfoColumn(column,pRessource);
                else
				    infoColumn = new InfoColumn(column);
				infoColumn.columnSpan = pColumnSpan;
				infoColumn.SetCellDataType(pDrv);
				if (0 < pOtherColumnName.Length)
				{
					foreach (string col in pOtherColumnName)
					{
						if (StrFunc.IsFilled(pDrv[col].ToString()))
							infoColumn.columnValue += Cst.HTMLSpace + pDrv[col].ToString();
					}
				}
			}
			return infoColumn;
		}
        public InfoColumn[] GetInfoColumns(DataRowView pDrv,params string[] pOtherColumnName)
        {
            return GetInfoColumns(pDrv, 1, pOtherColumnName);
        }

        public InfoColumn[] GetInfoColumns(DataRowView pDrv, int pColumnSpan, params string[] pOtherColumnName)
		{
			ArrayList  aInfoColumn = new ArrayList();
            foreach (string col in pOtherColumnName)
			{
                ReferentialsReferentialColumn column = Referential[col];
                if (null != column)
				{
                    InfoColumn infoColumn = new InfoColumn(column)
                    {
                        columnSpan = pColumnSpan
                    };
                    infoColumn.SetCellDataType(pDrv);
					aInfoColumn.Add(infoColumn);
				}
			}
			InfoColumn[] infoColumns = null;
			if (0 < aInfoColumn.Count)
				infoColumns = (InfoColumn[])aInfoColumn.ToArray(aInfoColumn[0].GetType());
			return infoColumns;
		}
        public InfoColumn SplitInfoColumn(DataRowView pDrv, string pOtherColumnName, string pRessource, bool pIsUserRessource)
        {
            InfoColumn infoColumn = GetInfoColumn(pDrv, 1, pOtherColumnName, pRessource, pIsUserRessource);
            if (StrFunc.IsFilled(infoColumn.columnValue))
            {
                infoColumn.columnValue = infoColumn.columnValue.Replace("[", string.Empty);
                infoColumn.columnValue = infoColumn.columnValue.Replace("]", "<br/>");
            }
            return infoColumn;
        }
        public InfoColumn[] SplitInfoColumns(DataRowView pDrv, string pOtherColumnName)
        {
            InfoColumn[] infoColumns = GetInfoColumns(pDrv, 1, pOtherColumnName);
            foreach (InfoColumn infoColumn in infoColumns)
            {
                if (StrFunc.IsFilled(infoColumn.columnValue))
                {
                    infoColumn.columnValue = infoColumn.columnValue.Replace("[", string.Empty);
                    infoColumn.columnValue = infoColumn.columnValue.Replace("]", "<br/>");
                }
            }
            return infoColumns;
        }
		#endregion GetInfoColumn
		#region GetBoundColumnIndex
		public int GetBoundColumnIndex(string pColumnName)
		{
			for (int i=0;i<aBoundColumnName.Count;i++)
			{
				if (pColumnName == aBoundColumnName[i].ToString())
					return i;
			}
			return -1;
		}
		#endregion GetBoundColumnIndex
		#region protected GetTemplateFilename
		/// Returns the filename for the template that should be displayed from the cache or 
		/// via the TemplateSelection event
		/// DataRow of the child for which the template is loaded
		/// Filename of the template
        // EG 20180423 Analyse du code Correction [CA2200]
        protected virtual Cst.ErrLevel GetTemplateFilename(DataRow pDataRow, ref string pTemplateFileName, ref string pRelationName, ref object pProduct)
		{
			object cacheKey = null;
			try
			{

				if (CachingBases.Tablename == TemplateCachingBase)
					cacheKey = pDataRow.Table.TableName;
				else if (CachingBases.Column == TemplateCachingBase)
					cacheKey = pDataRow[TemplateCachingColumn];

				if ((null != cacheKey) && (m_CachingTable.Contains(cacheKey)))
				{
					pTemplateFileName = (string) m_CachingTable[cacheKey];
					pRelationName     = (string) m_CachingRelation[cacheKey];
                    pProduct= (object)m_CachingProduct;
				}
				else
				{
					HierarchicalGridTemplateSelectionEventArgs eventArgs = new HierarchicalGridTemplateSelectionEventArgs(pDataRow);
					bool isHandlerAttached = OnTemplateSelection(eventArgs);

					if(false == isHandlerAttached)
						throw new WarningException("Please use the event \"TemplateSelection\" to specify which " + 
							"template to load for a child table/row");

					pTemplateFileName = eventArgs.TemplateFilename;
                    pRelationName = eventArgs.RelationName;
                    pProduct = eventArgs.Product;


					if(null != cacheKey)
					{
                        m_CachingTable[cacheKey] = pTemplateFileName;
                        m_CachingRelation[cacheKey] = pRelationName;
                        m_CachingProduct = pProduct;
					}
				}		

				return Cst.ErrLevel.SUCCESS;
			}
			catch (Exception)
			{
				throw;
			}
		}
		#endregion GetTemplateFilename
		#region public HideBoundColumn
		public void HideBoundColumn(bool pIsExclude,ArrayList paColumnsName)
		{
            for (int i=0;i<aBoundColumnName.Count;i++)
			{
                bool isContains = paColumnsName.Contains(aBoundColumnName[i]);
                if ((isContains && (false == pIsExclude)) ||
					((false == isContains) && pIsExclude))
				{
					this.Columns[i].HeaderText = string.Empty;
				}
			}
		}
		#endregion HideBoundColumn
		#region public LoadReferential
        public void LoadReferential(string pFileName)
        {
			string columnName;
			int    totWidth    = 0;
			try
			{
                ReferentialTools.DeserializeXML_ForModeRO(pFileName, out m_Referential);
				if (null != Referential)
				{
                    aBoundColumnName = new ArrayList();

					foreach (ReferentialsReferentialColumn column in Referential.Column)
					{
						columnName     = column.ColumnName;
						BoundColumn bc = AddBoundColumn(column);
						SetBoundColumnDataType(bc,column);
						aBoundColumnName.Add(columnName);
						if (bc.Visible)
							totWidth += column.GridWidthSpecified?column.GridWidth:0;
						this.Columns.Add(bc);
						if (column.IsDataKeyField || column.IsKeyField)
							this.DataKeyField = bc.DataField;
        			
						#region ExistsRelation
						if (column.ExistsRelation)
						{
							bc = AddBoundColumnRelation(column);
							if (bc.Visible)
								totWidth += column.GridWidthSpecified?column.GridWidth:0;
							ReferentialsReferentialColumnRelation relation = column.Relation[0];
							if (StrFunc.IsFilled(relation.AliasTableName))
								columnName = relation.AliasTableName + "_" + relation.ColumnSelect[0].ColumnName;
							else
								columnName = relation.ColumnSelect[0].ColumnName;
							aBoundColumnName.Add(columnName);
							this.Columns.Add(bc); 
						}
						#endregion ExistsRelation
					}
				}
			}
			catch (Exception)
			{
				throw;
			}
		}

        #endregion LoadReferential
        #region protected LoadTemplateIntoPanel
        /// Creates a new panel and loads the Template from the ASCX file into the panel
        /// <param name="panel">Container in which the template is rendered</param>
        /// <param name="templateFilename">Filename of the template</param>
        /// <param name="panelName">ID of the Panel that is created</param>
        /// <param name="templateName">ID of the Template that is loaded</param>
        /// <returns>a reference to the newly created panel that contains the template</returns>
        protected virtual Panel LoadTemplateIntoPanel(Panel pPanel, string pTemplateFilename, string pPanelName, string pTemplateName, object pObject)
		{
            Panel pnl = new Panel
            {
                ID = pPanelName
            };
            pPanel.Controls.Add(pnl);
			Control template = null;


			if (IsUserControl)
			{
				#region UserControl
				template = Page.LoadControl(pTemplateFilename);
				template.ID      = pTemplateName;
				pnl.Controls.Add(template);
				#endregion UserControl
			}
			else if (IsWebControl)
			{
				#region WebControl
				Type tControl      = Type.GetType(this.GetType().Namespace + "." + pTemplateFilename,true,true);
                ConstructorInfo ci = null;
                if (null != pObject)
                {
                    Type[] argTypes = new Type[] { typeof(System.Object) };
                    object[] argValues = new object[] { pObject };
                    ci = tControl.GetConstructor(argTypes);
                    if (null != ci)
                        template = (Control)ci.Invoke(argValues);
                }
                if (null == ci)
                {
                    ci = tControl.GetConstructor(System.Type.EmptyTypes);
                    template = (Control)ci.Invoke(null);
                }
				template.ID = pTemplateName;
				pnl.Controls.Add(template);
				#endregion WebControl
			}
			return pnl;
		}
		#endregion LoadTemplateIntoPanel
		#region protected LoadTemplateSingleRow
		/// Loads one instance of the template for each child row
		/// <param name="index">index of the DataRow in the Table</param>
		/// <param name="panel">Container in which the template is rendered</param>
		/// <param name="templateFilename">Filename of the template</param>
		/// <param name="item">DataGridItem that has just been databound</param>
		/// <param name="dataRow">DataRow of the child that should be displayed</param>
		protected virtual void LoadTemplateSingleRow(int pIndex,Panel pPanel,string pTemplateFilename,string pRelationName, 
			DataGridItem pItem, DataRow pDataRow)
		{
			string rowName = pDataRow.Table.TableName + "_" + pIndex;

			Panel pnl = LoadTemplateIntoPanel(pPanel, pTemplateFilename, "Pnl_" + rowName, "Child_" + rowName,null);

			//setting the DataItem property of the current DataGridItem to a DataRowView of the current child's row
			DataView view = pDataRow.Table.DefaultView;
			IEnumerator enumerator = view.GetEnumerator();
			
			//Workaround:	found no way to create a DataRowView instance from a DataRow other than looping over the DefaultView
			while(enumerator.MoveNext())
			{
				if(pDataRow == ((DataRowView) enumerator.Current).Row)
					break;
			}

			pItem.DataItem = (DataRowView) enumerator.Current;
			pnl.DataBind();
		}

		#endregion LoadTemplateSingleRow
		#region protected LoadTemplateTable
		/// Loads one instance of the template for all child rows of a relation
		/// <param name="panel">Container in which the template is rendered</param>
		/// <param name="templateFilename">Filename of the template</param>
		/// <param name="item">DataGridItem that has just been databound</param>
		/// <param name="dataRows">DataRow array of the childs that should be displayed</param>
        // EG 20180423 Analyse du code Correction [CA2200]
        protected virtual void LoadTemplateTable(Panel pPanel, string pTemplateFilename, string pRelationName, object pProduct, DataGridItem pItem, DataRow[] pDataRows)
		{
			try
			{
				Panel pnl = LoadTemplateIntoPanel(pPanel, pTemplateFilename,"Pnl_" + pDataRows[0].Table.TableName,
                    "Child_" + pDataRows[0].Table.TableName, pProduct);

				DataView    dv   = null;
				DataRowView drv  = null;
				Type tDataSource = DataSource.GetType();
				//
				if (tDataSource.Equals(typeof(DataSet)))
					dv = new DataView(((DataSet) this.DataSource).Tables[this.DataMember]);
				else if (tDataSource.Equals(typeof(DataView)) || tDataSource.BaseType.Equals(typeof(DataView)))
					dv = (DataView) this.DataSource;
				//
				drv = dv[pItem.ItemIndex];	
				pItem.DataItem = drv.CreateChildView(pRelationName);
				pnl.DataBind();
			}
			catch (Exception)
			{
				throw;
			}
		}
		#endregion LoadTemplateTable
		#region public SetBoundColumnDataType
		public static void SetBoundColumnDataType(BoundColumn pBoundColumn,ReferentialsReferentialColumn pColumn)
		{
			if (TypeData.IsTypeDate(pColumn.DataType.value))
			{
				pBoundColumn.DataFormatString = "{0:d}";
				pBoundColumn.ItemStyle.HorizontalAlign = HorizontalAlign.Center;
			}
            else if (TypeData.IsTypeDateTime(pColumn.DataType.value))
            {
				pBoundColumn.DataFormatString = "{0:G}";
                pBoundColumn.ItemStyle.HorizontalAlign = HorizontalAlign.Center;
            }
            else if (TypeData.IsTypeDec(pColumn.DataType.value))
			{
				if (pColumn.ScaleSpecified)
					pBoundColumn.DataFormatString = "{0:N" + pColumn.Scale.ToString() + "}";
				else
					pBoundColumn.DataFormatString = "{0:N}";
				pBoundColumn.ItemStyle.HorizontalAlign = HorizontalAlign.Right;
			}
            else if (TypeData.IsTypeInt(pColumn.DataType.value))
			{
				pBoundColumn.DataFormatString = "{0:N0}";
				pBoundColumn.ItemStyle.HorizontalAlign = HorizontalAlign.Right;
			}
            else if (TypeData.IsTypeBool(pColumn.DataType.value))
			{
				pBoundColumn.DataFormatString = "{0:N0}";
				pBoundColumn.ItemStyle.HorizontalAlign = HorizontalAlign.Right;
			}
			else
			{
				pBoundColumn.DataFormatString = "{0:d}";
				pBoundColumn.ItemStyle.HorizontalAlign = HorizontalAlign.Left;
			}
		}
		#endregion SetBoundColumnDataType
		#region public SetButtonInCell
		// EG 20200724 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
		public WCToolTipButton SetButtonInCell(DataGridItem pDataGridItem, string pColumnName, string pStringToCompare)
		{
            return SetButtonInCell(pDataGridItem, pColumnName, pStringToCompare, 1, "evtitle orange");
		}
        public WCToolTipButton SetButtonInCell(DataGridItem pDataGridItem, string pColumnName, string pStringToCompare,
			ExtendEnum pExtendEnum,int pCellOffset,string pCssName)
		{

            WCToolTipButton btn = SetButtonInCell(pDataGridItem, pColumnName, pStringToCompare, pCellOffset, pCssName);
			if (null != btn)
			{
				ExtendEnumValue extendEnumValue = pExtendEnum.GetExtendEnumValueByValue(btn.Text);
				if (null != extendEnumValue)
					btn.Pty.TooltipContent = "See " + extendEnumValue.Documentation;
			}
			return btn;
		}
        public WCToolTipButton SetButtonInCell(DataGridItem pDataGridItem, string pColumnName, string pStringToCompare, int pCellOffset, string pCssName)
		{
			for (int i=0;i<aBoundColumnName.Count;i++)
			{
				if (pColumnName == aBoundColumnName[i].ToString())
				{
					TableCell cell  = pDataGridItem.Cells[i+pCellOffset];
					if (0 == cell.Text.CompareTo(pStringToCompare))
					{
                        WCToolTipButton btnDetail = new WCToolTipButton
                        {
                            CssClass = pCssName,
                            ID = "EventDetail",
                            Text = cell.Text
                        };
                        cell.Text = string.Empty;
						btnDetail.Pty.TooltipContent = cell.Text;
						cell.Controls.Add(btnDetail);
						return btnDetail;
					}
				}
			}
			return null;
		}
		#endregion SetButtonInCell
		#region public SetFormatDecimalCell
		public void SetFormatDecimalCell(DataGridItem pDataGridItem,string pColumnName,decimal pValue,int pScale)
		{
			SetFormatDecimalCell(pDataGridItem,pColumnName,pValue,pScale,1);
		}
		public void SetFormatDecimalCell(DataGridItem pDataGridItem,string pColumnName,decimal pValue,int pScale,int pCellOffset)
		{
			for (int i=0;i<aBoundColumnName.Count;i++)
			{
				if (pColumnName == aBoundColumnName[i].ToString())
				{
					TableCell cell  = pDataGridItem.Cells[i+pCellOffset];
					cell.Text       = StrFunc.FmtDecimalToCurrentCulture(pValue,pScale);
					break;
				}
			}
		}
		#endregion SetFormatDecimalCell
        #region public SetFormatPercentCell
        public void SetFormatPercentCell(DataGridItem pDataGridItem, string pColumnName, decimal pValue, int pScale)
        {
            SetFormatPercentCell(pDataGridItem, pColumnName, pValue, pScale, 1);
        }
        public void SetFormatPercentCell(DataGridItem pDataGridItem, string pColumnName, decimal pValue, int pScale, int pCellOffset)
        {
            for (int i = 0; i < aBoundColumnName.Count; i++)
            {
                if (pColumnName == aBoundColumnName[i].ToString())
                {
                    TableCell cell = pDataGridItem.Cells[i + pCellOffset];
                    cell.Text = pValue.ToString("p" + pScale.ToString());
                    break;
                }
            }
        }
        #endregion SetFormatPercentCell
        #region  AddPanelImageInCell
        public Panel AddPanelImageInCell(DataGridItem pDataGridItem, string pColumnName, string pImageName, object pCompareTo, int pCellOffset)
        {
            return AddPanelImageInCell(pDataGridItem, pColumnName, pImageName, pCompareTo, pCellOffset, null);
        }
		// EG 20200908 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc) Correction et compléments
		public Panel AddPanelImageInCell(DataGridItem pDataGridItem, string pColumnName, string pAwesomeImageName, object pCompareTo, int pCellOffset, string pResourceKey)
        {
            for (int i = 0; i < aBoundColumnName.Count; i++)
            {
                if (pColumnName == aBoundColumnName[i].ToString())
                {
                    ReferentialsReferentialColumn column = Referential[pColumnName];
                    if (TypeData.IsTypeBool(column.DataType.value))
                    {
                        TableCell cell = pDataGridItem.Cells[i + pCellOffset];
                        string cellValue = cell.Text;
                        cell.Text = string.Empty;
                        if ((bool)pCompareTo == BoolFunc.IsTrue(cellValue))
                        {
                            Panel pnl = new Panel
                            {
                                CssClass = String.Format("fa-icon {0}", pAwesomeImageName)
                            };
                            if (StrFunc.IsFilled(pResourceKey))
                            {
                                pnl.ToolTip = Ressource.GetString(pResourceKey);
                                pnl.Style.Add(HtmlTextWriterStyle.Cursor, "pointer");
                            }
                            cell.Controls.Add(pnl);
                            return pnl;
                        }
                    }
                }
            }
            return null;
        }
        #endregion AddPanelImageInCell
        #region public SetInfoInRow
        public static void SetInfoInRow(TableRow pTr, int pColumnspan)
		{
			SetInfoInRow(pTr,null,null,false,pColumnspan);
		}
        public static void SetInfoInRow(TableRow pTr, InfoColumn pInfoColumn, int pColumnspan)
		{
			SetInfoInRow(pTr,pInfoColumn,null,false,pColumnspan);
		}

		public static void SetInfoInRow(TableRow pTr,InfoColumn pInfoColumn)
		{
			SetInfoInRow(pTr,pInfoColumn,null,false,1);
		}
        public static void SetInfoInRow(TableRow pTr, InfoColumn pInfoColumn, string pRessourceSubstitute)
		{
			SetInfoInRow(pTr,pInfoColumn,pRessourceSubstitute,false,1);
		}
		public static void SetInfoInRow(TableRow pTr,InfoColumn pInfoColumn,string pRessourceSubstitute,bool pIsDisplayNullValue,int pColumnspan)
		{
			if (null != pInfoColumn)
			{
				if (pIsDisplayNullValue || StrFunc.IsFilled(pInfoColumn.columnValue.Trim()))
				{
					#region Label
					TableCell td       = new TableCell();
                    //CC/PL 20111206 Certaines ressources sont issues de ressources "Menu"
                    if (pInfoColumn.ressource.StartsWith(Software.MenuRoot()))
                        td.Text = Ressource.GetMenu_Shortname(pInfoColumn.ressource, pRessourceSubstitute);
                    else
                        td.Text = Ressource.GetString(pInfoColumn.ressource, pRessourceSubstitute, true);
					td.HorizontalAlign = HorizontalAlign.Left;
					td.Wrap            = false;
					td.CssClass        = "DataGrid_LabelToolTipStyle";
					pTr.Cells.Add(td);
                    #endregion Label
                    #region Value
                    td = new TableCell
                    {
                        Wrap = false,
                        HorizontalAlign = pInfoColumn.hAlign,
						Text = StrFunc.IsEmpty(pInfoColumn.columnValue)? Cst.HTMLSpace: pInfoColumn.columnValue
					};
					#endregion Value
					if (1 < pColumnspan)
						td.ColumnSpan = pColumnspan;
					td.CssClass        = "DataGrid_ToolTipStyle";
					pTr.Cells.Add(td);
				}
			}
		}
		#endregion SetInfoInRow
		#region public SetToolTipInCell
        // EG 20150318 [POC]
		public void SetToolTipInCell (DataGridItem pDataGridItem, string pColumnName, ExtendEnum pExtendEnum, int pCellOffset)
		{
			TableCell cell = GetDataGridItemCell(pDataGridItem, pColumnName, pCellOffset);
			if (null != cell)
			{
                // EG 20160404 Migration vs2013
                //if (cell.Text == "TRD")
                //{
                //    int i  = 0;
                //}
				ExtendEnumValue extendEnumValue = pExtendEnum.GetExtendEnumValueByValue(cell.Text);
				if (null != extendEnumValue)
				{
					cell.ToolTip = extendEnumValue.Documentation;
			        //Color foreColor = Color.Black;
                    try
                    {
                        bool isNewForeColor = StrFunc.IsFilled(extendEnumValue.ForeColor);
                        bool isNewBackColor = StrFunc.IsFilled(extendEnumValue.BackColor);
                        if (isNewBackColor || isNewForeColor)
                        {
                            Color newForeColor = isNewForeColor ? Color.FromName(extendEnumValue.ForeColor) : (cell.ForeColor.IsEmpty ? Color.Black : cell.ForeColor);
                            Color newBackColor = isNewBackColor ? Color.FromName(extendEnumValue.BackColor) : (cell.BackColor.IsEmpty ? Color.White : cell.BackColor);

                            if ((newForeColor != newBackColor) && newForeColor.IsKnownColor && newBackColor.IsKnownColor)
                            {
                                cell.ForeColor = newForeColor;
                                cell.BackColor = newBackColor;
                            }
                        }
                    }
                    catch{}
				}
				else
				{
					cell.ToolTip = Ressource.GetString(cell.Text, true);
				}
			}
		}
		public void SetColorInCell (DataGridItem pDataGridItem, string pColumnName, string pColor, int pCellOffset)
		{
			TableCell cell = GetDataGridItemCell(pDataGridItem, pColumnName, pCellOffset);
			if (null != cell)
			{
				switch(pColor)
				{
					case "Receiver":
						//PL 20110225 PASGLOP
                        //cell.ForeColor = Color.RoyalBlue;
                        cell.ForeColor = Color.Green;
                        //cell.CssClass = EFSCssClass.CssClassEnum.DataGrid_Receiver.ToString();
						if (cell.Text.Trim().Length > 0)
							cell.ToolTip = Ressource.GetString(pColor, true);
						break;
					case "Payer":
                        //PL 20110225 PASGLOP
                        cell.ForeColor = Color.DarkRed;
                        //cell.CssClass = EFSCssClass.CssClassEnum.DataGrid_Payer.ToString();
						if (cell.Text.Trim().Length > 0)
							cell.ToolTip = Ressource.GetString(pColor, true);
						break;
					default:
						try
						{
							Color color = Color.FromName(pColor);
							cell.ForeColor = color;
							if (color == Color.DarkGray)
								cell.Font.Bold = false;
						}
						catch{}
						break;
				}
			}
		}
//		public void SetToolTipInCell (DataGridItem pDataGridItem,string pColumnName)
//		{
//			SetToolTipInCell(pDataGridItem,pColumnName,1);
//		}
		public TableCell GetDataGridItemCell (DataGridItem pDataGridItem, string pColumnName, int pCellOffset)
		{
			for (int i=0;i<aBoundColumnName.Count;i++)
			{
				if (pColumnName == aBoundColumnName[i].ToString())
				{
					TableCell cell = pDataGridItem.Cells[i+pCellOffset];
					return cell;
				}
			}
			return null;
		}
		#endregion SetToolTipInCell
		#endregion Methods

		#region Implementation of IPostBackDataHandler
		#region RaisePostDataChangedEvent
		/// <summary>
		/// Unused in the implementation of the HierarGrid but needed according to IPostBackDataHandler
		/// </summary>
		public void RaisePostDataChangedEvent()
		{
		}
		#endregion RaisePostDataChangedEvent
		#region LoadPostData
		/// Loads the post data from the hidden field that contains the row state (expanded or collapsed)
		/// <param name="postDataKey">contains the control's UniqueID</param>
		/// <param name="postCollection">Postdata</param>
		/// <returns>always false</returns>
		public bool LoadPostData(string postDataKey, NameValueCollection postCollection)
		{
			string hiddenFieldName = EXPAND_CLIENTIDS_HIDDENFIELDNAME + this.ClientID;

			if(postCollection[hiddenFieldName] != null)
				ExpandClientIDs = postCollection[hiddenFieldName];
			
			return false;
		}
		#endregion LoadPostData
		#endregion Implementation of IPostBackDataHandler
	}
	#endregion HierarchicalGrid
	
	#region public class HierarchicalPanel
    // EG 20180423 Analyse du code Correction [CA1405]
    [ComVisible(false)]
	public class HierarchicalPanel : Panel
	{
		#region Variables
		protected DataRowView  m_ParentDataRowView;

		public DataRowView ParentDataRowView
		{
			get {return m_ParentDataRowView;}
		}
		#endregion Variables
		#region Constructors
		public HierarchicalPanel() : base()
		{
			this.ID = "HGContainer";
			this.DataBinding += new System.EventHandler(OnDataBinding);
		}
		#endregion Constructors
		#region Event
		#region OnDataBinding
		protected void OnDataBinding(object sender, System.EventArgs e)
		{
			DataGridItem dgi = (DataGridItem) this.BindingContainer;
            if (null != dgi)
            {
                DataGrid dg = (DataGrid)dgi.Parent.BindingContainer;
                DataView dv = (DataView)dg.DataSource;
                if (dv.Count > dgi.DataSetIndex)
                    m_ParentDataRowView = (DataRowView)dv[dgi.DataSetIndex];
            }
		}
		#endregion OnDataBinding
		#endregion Event
	}

	#endregion HierarchicalPanel
	
	#region public class RowStates
	/// Helper class that externally looks like a collection with an indexer but stores the RowState in the 
	/// owner's hidden text box "ExpandClientIDs"
	public class RowStates
	{
		#region Variable
		private readonly HierarchicalGrid m_HGrid;
		#endregion Variable
		#region Constructor
		/// Constructor that sets a reference to the parent HierarchicalGrid
		/// <param name="pHierarchicalGrid">Reference to the parent HierarchicalGrid</param>
		public RowStates(HierarchicalGrid pHierarchicalGrid)
		{
			m_HGrid = pHierarchicalGrid;
		}
		#endregion Constructor
		#region Indexor
		/// Specifies whether a Row is expanded or collapsed
        public bool this[int pIndex]
        {
            get
            {
                if (pIndex < m_HGrid.Items.Count)
                {
                    Panel pnl = (Panel)m_HGrid.Items[pIndex].FindControl("Expand");
                    if ((null != pnl) && StrFunc.IsFilled(m_HGrid.ExpandClientIDs))
                    {
                        if (0 <= m_HGrid.ExpandClientIDs.IndexOf(pnl.ClientID))
                            return true;
                    }
                }
                return false;
            }
            set
            {
                if (pIndex < m_HGrid.Items.Count)
                {
                    Panel pnl = (Panel)m_HGrid.Items[pIndex].FindControl("Expand");
                    if (null != pnl)
                    {
                        if (value)
                        {
                            //add the ClientID to the hidden text field
                            if (0 > m_HGrid.ExpandClientIDs.IndexOf(pnl.ClientID))
                                m_HGrid.ExpandClientIDs += HierarchicalGrid.EXPAND_CLIENTIDS_SEP + pnl.ClientID;
                        }
                        else
                            //remove the ClientID from the hidden text field
                            m_HGrid.ExpandClientIDs = m_HGrid.ExpandClientIDs.Replace(pnl.ClientID, String.Empty);
                    }
                }
            }
        }
		#endregion Indexor
		#region Methods
		#region CollapseAll
		public void CollapseAll()
		{
			SetAll(false);
		}
		#endregion CollapseAll
		#region ExpandAll
		public void ExpandAll()
		{
			SetAll(true);
		}
		#endregion ExpandAll
		#region SetAll
		/// Expands or collapses all the rows
		private void SetAll(bool pIsExpanded)
		{
			for(int i=0;i<m_HGrid.Items.Count;i++)
				this[i] = pIsExpanded;
		}
		#endregion SetAll
		#endregion Methods
	}
	#endregion Class: RowStates
	
	#region TemplateSelection event: Delegate and EventArgs
	/// Represents the method that will handle the TemplateSelection event.
	[Serializable]
	public delegate void HierarchicalGridTemplateSelectionEventHandler(object sender, HierarchicalGridTemplateSelectionEventArgs e);
	/// Provides data for the TemplateSelection event
	public class HierarchicalGridTemplateSelectionEventArgs : EventArgs
	{
		#region Variables
		private readonly DataRow m_Row;
        private string m_TemplateFilename;
        private string m_RelationName;
        private object m_Product;
		#endregion Variables
		#region Accessors
        #region Product
        public object Product
        {
            get { return m_Product; }
            set { m_Product = value; }
        }
        #endregion Product
        #region RelationName
		/// Set the Filename for the template that shall be loaded
		public string RelationName
		{
			get { return m_RelationName; }
			set { m_RelationName = value; }
		}
		#endregion RelationName
		#region Row
			/// Gets the row the event has been raised for
			public DataRow Row
			{
				get {return m_Row;}
			}
			#endregion Row
		#region TemplateFilename
			/// Set the Filename for the template that shall be loaded
			public string TemplateFilename
			{
				get { return m_TemplateFilename; }
				set { m_TemplateFilename = value; }
			}
			#endregion TemplateFilename
		#endregion Accessors
		#region Constructor
		/// Initializes a new instance of HierarGridTemplateSelectionEventArgs class.
		/// <param name="pRow">The row that the event was raised for</param>
		/// <param name="pTemplateFilename">The filename of the template that shall be loaded</param>
		public HierarchicalGridTemplateSelectionEventArgs(DataRow pRow) : this(pRow, String.Empty,String.Empty,null ){}
		public HierarchicalGridTemplateSelectionEventArgs(DataRow pRow, string pTemplateFilename) 
			: this(pRow, pTemplateFilename,String.Empty,null){}
		public HierarchicalGridTemplateSelectionEventArgs(DataRow pRow, string pTemplateFilename,string pRelationName,object pProduct)
		{
            m_Row = pRow;
            m_TemplateFilename = pTemplateFilename;
            m_RelationName = pRelationName;
            m_Product = pProduct;
		}
		#endregion Constructor
	}
	#endregion TemplateSelection event: Delegate and EventArgs
	
	#region TemplateDataModeSelection event: Delegate and EventArgs
	/// Represents the method that will handle the TemplateDataModeSelection event.
	[Serializable]
	public delegate void HierarchicalGridTemplateDataModeSelectionEventHandler(object sender,
	HierarchicalGridTemplateDataModeSelectionEventArgs e);

	/// Provides data for the TemplateDataModeSelection event
	public class HierarchicalGridTemplateDataModeSelectionEventArgs : EventArgs
	{
		#region Variables
		private readonly DataRelation      m_Relation;
		private TemplateDataModes m_TemplateDataMode;
		#endregion Variables
		#region Accessors
		#region Relation
		/// Gets the DataRelation the event has been raised for
		public DataRelation Relation
		{
			get { return m_Relation; }
		}
		#endregion Relation
		#region TemplateDataMode
		/// Set the TemplateDataMode that will be use to load child rows for the DataRelation
		public TemplateDataModes TemplateDataMode
		{
			get { return m_TemplateDataMode; }
			set { m_TemplateDataMode = value; }
		}
		#endregion TemplateDataMode
		#endregion Accessors
		#region Constructor
		/// Initializes a new instance of HierarGridTemplateDataModeSelectionEventArgs class.
		/// <param name="pDataRelation">The DataRelation that the event was raised for</param>
		/// <param name="pTemplateDataMode">The the TemplateDataMode that will be use to load child rows for the DataRelation</param>
		public HierarchicalGridTemplateDataModeSelectionEventArgs(DataRelation pDataRelation, TemplateDataModes pTemplateDataMode)
		{
			m_Relation         = pDataRelation;
			m_TemplateDataMode = pTemplateDataMode;
		}
		#endregion Constructor
	}
	#endregion TemplateDataModeSelection event: Delegate and EventArgs
}
