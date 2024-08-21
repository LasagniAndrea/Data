using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace EFS.Spheres.Trial.Jquery
{
    public partial class WebForm1 : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }


        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);


            DDLResource.Items.Add(new ListItem("", ""));
            DDLResource.Items.Add(new ListItem("ExpiryDate", "ExpiryDate"));
            DDLResource.Items.Add(new ListItem("SortBy", "SortedBy"));
            DDLResource.Items.Add(new ListItem("TradeStatus", "TradeStatus"));
        }
    }
}