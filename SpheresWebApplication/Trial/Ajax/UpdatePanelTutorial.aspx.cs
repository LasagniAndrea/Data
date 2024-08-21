using System;
using System.Data;
using System.Configuration;
using System.Collections;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;

public partial class UpdatePanelTutorial : System.Web.UI.Page
{
    protected override void OnInit(EventArgs e)
    {
        ScriptManager sm =  ScriptManager.GetCurrent(this);
        sm.SupportsPartialRendering = true;
        
        base.OnInit(e);
    }

    protected void Page_Load(object sender, EventArgs e)
    {
        Button1.Click += new EventHandler(Button1_Click);
        Button2.Click += new EventHandler(Button2_Click);

        UpdatePanel1.Visible = true; 
    }


    protected void Button1_Click(object sender, EventArgs e)
    {
        Label1.Text = "Refreshed at " +
            DateTime.Now.ToString();
    }

    protected void Button2_Click(object sender, EventArgs e)
    {
        Label1.Text = "Refreshed at " +
            DateTime.Now.ToString();
    }
    
    protected override void OnPreRender(EventArgs e)
    {
        base.OnPreRender(e);
    }

    protected override void Render(HtmlTextWriter writer)
    {
        base.Render(writer);
    }


}


