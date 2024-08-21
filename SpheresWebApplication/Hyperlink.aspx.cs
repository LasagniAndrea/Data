using System;
using System.Web.UI;

using EFS.ACommon;
using EFS.Common;
using EFS.Common.Web;

namespace EFS.Spheres
{
    /// <summary>
    /// Page pour contruction et de transfert de l'URL finale 
    /// </summary>
    /// EG 20170125 [Refactoring URL] New     
    /// EG 20201116 [25556] New Réécriture du lien sur Hyperlink via Summary.aspx (Pb self.Close) 
    public partial class Hyperlink : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            string url;
            if (null != Request.QueryString["mnu"])
                url = SpheresURL.MenuToCall(Request.QueryString["mnu"]); // L'appel vient de summary
            else
                url = SpheresURL.PageToCall(Request.QueryString);
            
            if (StrFunc.IsFilled(url))
                Server.Transfer(url);
        }
    }
}