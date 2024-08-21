using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;

using EFS.ACommon;

namespace EFS.Spheres
{
    public partial class SiteMap : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        protected void OnItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            HtmlGenericControl ul = null;
            if (e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem)
            {
                SiteMapNode mapNode = (SiteMapNode)e.Item.DataItem;
                ul = new HtmlGenericControl("ul");
                ul.Attributes.Add("class", "site-map-menu");
                ul.Controls.Add(ConstructLink(mapNode, "first leaf"));

                List<SiteMapNode> lstChildMnu = mapNode.ChildNodes.Cast<SiteMapNode>().ToList();
                lstChildMnu.ForEach(childMnu =>
                {
                    ul.Controls.Add(ConstructMenu(childMnu, MapClass(childMnu)));
                });
                e.Item.Controls.Add(ul);
            }
        }

        private Control ConstructLink(SiteMapNode pSiteMapNode, string pClass)
        {
            HtmlGenericControl li = new HtmlGenericControl("li");
            li.Attributes.Add("class", pClass);
            HyperLink lnk = new HyperLink();
            lnk.Text = Ressource.GetString(pSiteMapNode.Title, true);
            lnk.NavigateUrl = pSiteMapNode.Url;
            li.Controls.Add(lnk);
            return li;
        }

        private string MapClass(SiteMapNode pSiteMapNode)
        {
            return pSiteMapNode.HasChildNodes ? "expanded" : "leaf"; ;
        }


        private Control ConstructMenu(SiteMapNode pSiteMapNode, string pClass)
        {
            Control ctrl = ConstructLink(pSiteMapNode, pClass);

            if (pSiteMapNode.HasChildNodes)
            {
                List<SiteMapNode> lstChildMnu = pSiteMapNode.ChildNodes.Cast<SiteMapNode>().ToList();
                lstChildMnu.ForEach(childMnu =>
                {
                    HtmlGenericControl ul = new HtmlGenericControl("ul");
                    ul.Attributes.Add("class", "site-map-menu");
                    ul.Controls.Add(ConstructMenu(childMnu, MapClass(childMnu)));
                    ctrl.Controls.Add(ul);
                });
            }
            return ctrl;
        }
    }
}