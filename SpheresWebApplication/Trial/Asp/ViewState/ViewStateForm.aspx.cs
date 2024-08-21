using System;
using System.Web.UI;

namespace EFS.Spheres.Trial.Asp
{
    public partial class ViewStateForm : System.Web.UI.Page
    {
        
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            
        }
        protected override void LoadViewState(object savedState)
        {
            base.LoadViewState(savedState);
        }


        protected void Page_Load(object sender, EventArgs e)
        {
            if (false == IsPostBack)
            {
                LBL_div1.Text = "Nom";
                LBL_div2.Text = "Prénom";
            }

            System.Diagnostics.Debug.WriteLine(string.Format("{0}: {1}", TXT_div1.ClientID, TXT_div1.Text));
            System.Diagnostics.Debug.WriteLine(string.Format("{0}: {1}", TXT_div2.ClientID, TXT_div2.Text));
        }


        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
        }

        protected override object SaveViewState()
        {
            return base.SaveViewState();
        }

        protected override void Render(HtmlTextWriter writer)
        {
            base.Render(writer);
        }

    }
}