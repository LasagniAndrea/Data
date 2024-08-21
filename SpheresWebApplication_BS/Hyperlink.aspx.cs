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
    // EG 20170125 [Refactoring URL] New     
    public partial class Hyperlink : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            string url = SpheresURL.PageToCall(Request.QueryString);
            url = AspTools.SetAspxPageName(url);
            if (StrFunc.IsFilled(url))
                Server.Transfer(url);
        }
    }
}