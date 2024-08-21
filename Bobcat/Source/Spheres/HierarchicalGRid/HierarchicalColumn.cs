using System;
using System.Collections;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Runtime.InteropServices;
using EFS.ACommon;


namespace EFS
{
	#region public class HierarchicalColumnInfo
	public class HierarchicalColumnInfo
	{

		public string name;
		public ArrayList relationName;

		public string resource;
		public string headerText;
		public int    pos;
		public bool   widthSpecified;
		public int    width;

		public HierarchicalColumnInfo(string pName,string pResource,string pRelationName)
			:this(pName,pResource,pRelationName,-1,0,null){} 
		public HierarchicalColumnInfo(string pName,string pResource,string pRelationName,int pPosition)
			:this(pName,pResource,pRelationName,pPosition,0,null){} 
		public HierarchicalColumnInfo(string pName,string pResource,string pRelationName,int pPosition,int pWidth,params string[] pRelationNames)
		{
			name         = pName;
			resource     = pResource;
			pos          = pPosition;
            relationName = new ArrayList
            {
                pRelationName
            };
            if (null != pRelationNames)
			{
				foreach (string relation in pRelationNames)
					relationName.Add(relation);
			}
			if (StrFunc.IsFilled(resource))
				headerText = Ressource.GetString(resource);

			widthSpecified = (pWidth != 0);
			width          = pWidth;
		}
	}
	#endregion 
	#region public class HierarchicalColumn
	/// <summary>
	/// The HierarColumn is derived from the DataGridColumn and contains an image with a plus/minus icon 
	/// and a HierarchicalPlaceHolder that takes the dynamically loaded templates
	/// </summary>
    // EG 20180423 Analyse du code Correction [CA1405]
    [ComVisible(false)] 
	public class HierarchicalColumn : DataGridColumn
	{
		#region Constructor
		public HierarchicalColumn() : base(){}
		#endregion Constructor
		#region Methods
		#region AddControls
		/// Adds a plus image and a HierarchicalPlaceHolder to the child collection
		// EG 20200728 [XXXXX] Nouvelle interface GUI v10(Mode Noir ou blanc) Correctifs et compléments
		protected virtual void AddControls(TableCell pCell, ListItemType pItemType)
        {
            Panel pnl = new Panel
            {
                ID = "Expand",
				CssClass = "fa-icon fa fa-plus-square"
			};
            pnl.Attributes.Add("onclick", "javascript:HierarchicalGrid_toggleRow(this);");
			pCell.Controls.Add(pnl);

            HierarchicalPlaceHolder hph = new HierarchicalPlaceHolder
            {
                ID = "HPH"
            };
            pCell.Style.Add(HtmlTextWriterStyle.Cursor, "pointer");
            pCell.Controls.Add(hph);
        }
        #endregion AddControls
		#region InitializeCell
		/// On initialization the HierarchicalGridColumn adds a plus image and a HierarchicalPlaceHolder 
		/// that is later filled with the templates
		public override void InitializeCell(TableCell pCell, int pColumnIndex, ListItemType pItemType)
		{
			base.InitializeCell(pCell, pColumnIndex, pItemType);
			switch (pItemType)
			{
				case ListItemType.Item:
				case ListItemType.AlternatingItem:
				case ListItemType.SelectedItem:
				{
					AddControls(pCell, pItemType);
					break;
				}
				case ListItemType.EditItem:
					break;
			}
		}
		#endregion InitializeCell
		#endregion Methods
	}
	#endregion HierarchicalColumn
	#region public class HierarchicalContainerColumn
	/// <summary>
	/// The HierarchicalContainerColumn is derived from the DataGridColumn and contains a placeHolder
	/// that takes the dynamically loaded templates
	/// </summary>
    // EG 20180423 Analyse du code Correction [CA1405]
    [ComVisible(false)] 
	public class HierarchicalContainerColumn : DataGridColumn
	{
		#region Constructor
		public HierarchicalContainerColumn() : base(){}
		#endregion Constructor
	}
	#endregion HierarchicalContainerColumn

}
