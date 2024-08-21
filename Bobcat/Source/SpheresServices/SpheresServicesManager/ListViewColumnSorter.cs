using System.Collections;
using System.Windows.Forms;

namespace EFS.SpheresService
{
	public class ListViewColumnSorter : IComparer
	{
		#region Members
		private int                     m_ColumnToSort;
		private SortOrder               m_OrderOfSort;
		private readonly CaseInsensitiveComparer m_ObjectCompare;
		#endregion Members
		#region Accessors
		#region SortColumn
		public int SortColumn
		{
			set {m_ColumnToSort = value;}
			get {return m_ColumnToSort;}
		}
		#endregion SortColumn
		#region Order
		public SortOrder Order
		{
			set {m_OrderOfSort = value;}
			get {return m_OrderOfSort;}
		}
		#endregion Order
		#endregion Accessors
		#region Constructors
		public ListViewColumnSorter()
		{
			//m_ColumnToSort  = 0;
			m_OrderOfSort   = SortOrder.None;
			m_ObjectCompare = new CaseInsensitiveComparer();
		}
		#endregion Constructors
		#region Methods
		#region Compare
		public int Compare(object pX,object pY)
		{
			ListViewItem listviewX = (ListViewItem)pX;
			ListViewItem listviewY = (ListViewItem)pY;

			int compareResult = m_ObjectCompare.Compare(listviewX.SubItems[m_ColumnToSort].Text,listviewY.SubItems[m_ColumnToSort].Text);
			if (SortOrder.Ascending == m_OrderOfSort)
				return compareResult;
            else if (SortOrder.Descending == m_OrderOfSort)
				return -compareResult;
			else
				return 0;
		}
		#endregion Compare
		#endregion Methods
	}
}
