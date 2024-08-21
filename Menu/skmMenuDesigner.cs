using System;
using System.IO;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.Design;
using System.Runtime.InteropServices;

namespace skmMenu
{
	namespace Design 
	{
		/// <summary>
		/// MenuDesigner is the Designer class for the Menu control.  This class contains methods
		/// used by the VS.NET IDE to provide a rich experience in the VS.NET Designer.
		/// </summary>
        // EG 20180423 Analyse du code Correction [CA1405]
        [ComVisible(false)] 
		public class MenuDesigner : ControlDesigner
		{
			private Menu menuInstance;

			public MenuDesigner() : base()
			{				
			}

			public override void Initialize(System.ComponentModel.IComponent component)
			{
				this.menuInstance = (Menu) component;
				base.Initialize (component);
			}


			public override string GetDesignTimeHtml() 
			{
				StringWriter sw       = new StringWriter();
				HtmlTextWriter writer = new HtmlTextWriter(sw);

				Table menu = new Table();				
				menu.ApplyStyle(menuInstance.MenuStyle);
				
				// Display the Menu based on its specified Layout
				if (menuInstance.Layout == MenuLayout.Vertical)
				{
					for (int i = 1; i <= 5; i++)
					{
						TableRow tr = new TableRow();
						TableCell td = new TableCell();
						td.ApplyStyle(menuInstance.UnselectedMenuItemStyle);
						td.Text = "Menu Item " + i.ToString();
						tr.Cells.Add(td);
						menu.Rows.Add(tr);
					}
				}
				else
				{
					TableRow tr = new TableRow();
					for (int i = 1; i <= 5; i++)
					{						
						TableCell td = new TableCell();
						td.ApplyStyle(menuInstance.UnselectedMenuItemStyle);
						td.Text = "Menu Item " + i.ToString();
						tr.Cells.Add(td);						
					}
					menu.Rows.Add(tr);
				}

				menu.RenderControl(writer);
				return sw.ToString();
			}

		}
	}
}
