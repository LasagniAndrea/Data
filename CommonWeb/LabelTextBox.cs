using System;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Drawing;
using System.Runtime.InteropServices;

namespace EFS.Common.Web
{
	/// <summary>
	/// Description résumée de LabelTextBox.
	/// </summary>
    // EG 20180423 Analyse du code Correction [CA1405]
    [ComVisible(false)] 
	public class LabelTextBox : TextBox
	{
		
		public String Label = "";
		public String LabelCssClass = "";
		public Boolean LabelOnTop = false;
		public string BorderType = "border-bottom";
		//
		public Boolean NotNull = false;
		//
		private Color _bordercolor =Color.Black;
		private Color _forecolor =Color.Black;
		
		public LabelTextBox()
		{
		}

		public LabelTextBox(String strLabel, String strText)
		{
			Label = strLabel;
			Text = strText;
		}

		
		//
		public override bool ReadOnly 
		{
			get {return base.ReadOnly;}
			set
			{
				base.ReadOnly=value;
				if (value)
				{
					_forecolor=Color.FromArgb(0,102,153);
					_bordercolor=Color.FromArgb(0,102,153);
				}
			}
		}

		private Label _label;

		protected override void CreateChildControls()
		{
			_label = new Label();
			if (LabelOnTop)
				_label.Text = Label + "<br/>";
			else
				_label.Text = Label + "&nbsp;:&nbsp;";
			_label.CssClass = LabelCssClass;			
			//
			// Set standard styles for the textbox
			this.Font.Name = "verdana";
			this.Font.Size = FontUnit.XSmall;
			this.Font.Bold = NotNull;
			//
			this.ForeColor=_forecolor;
			this.BorderColor=_bordercolor;
			//
			if (BorderType=="border-bottom")
			{
				this.BorderStyle=BorderStyle.None;
				this.Style["border-bottom"] = "solid";
				this.BackColor=Color.Transparent;
			}
			else
			{
				this.BorderStyle=BorderStyle.Solid;
				this.BackColor=Color.White;

			}
			if (NotNull)
				this.BorderWidth = (Unit) 2;
			else
				this.BorderWidth = (Unit) 1;

			//
		}

		protected override void Render(HtmlTextWriter writer)
		{
			_label.RenderControl(writer); 
			base.Render(writer);
		}

	}
}
