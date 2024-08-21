using System;
using System.Web.UI;

namespace EFS.Spheres.Trial
{
    public partial class Postback : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            //On post la page sur le onblur et le focus appliqué au contrôle suivant => Nickel

            // Create a new PostBackOptions object and set its properties.
            PostBackOptions myPostBackOptions = new PostBackOptions(this)
            {
                TrackFocus = true,
                AutoPostBack = true
            };

            txt1.Attributes.Add("onblur", Page.ClientScript.GetPostBackEventReference(myPostBackOptions));
            txt2.Attributes.Add("onblur", Page.ClientScript.GetPostBackEventReference(myPostBackOptions));

            DDL1.Attributes.Add("onblur", Page.ClientScript.GetPostBackEventReference(myPostBackOptions));
            DDL2.Attributes.Add("onblur", Page.ClientScript.GetPostBackEventReference(myPostBackOptions));

        }
    }
}