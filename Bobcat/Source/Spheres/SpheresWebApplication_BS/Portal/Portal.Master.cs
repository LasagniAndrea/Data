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
    public partial class PortalMaster : System.Web.UI.MasterPage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            //repeat_sm.ItemDataBound += new RepeaterItemEventHandler(OnItemDataBound);
        }

        protected void OnItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            if (e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem)
            {
                SiteMapNode mapNode = (SiteMapNode)e.Item.DataItem;
                if (mapNode.HasChildNodes)
                {
                    HtmlGenericControl li = new HtmlGenericControl("li");
                    HyperLink lnk = new HyperLink();
                    lnk.Text = "<span class='glyphicon glyphicon-home'></span>";
                    lnk.NavigateUrl = mapNode.Url;
                    li.Controls.Add(lnk);
                    e.Item.Controls.Add(li);

                    List<SiteMapNode> lstMainMnu = mapNode.ChildNodes.Cast<SiteMapNode>().ToList();
                    lstMainMnu.ForEach(mainMnu =>
                    {
                        Control ctrl = ConstructMenu(mainMnu);

                        if (mainMnu.HasChildNodes)
                        {
                            HtmlGenericControl ul = new HtmlGenericControl("ul");
                            ul.Attributes.Add("class", "dropdown-menu");
                            List<SiteMapNode> lstSubMnu = mainMnu.ChildNodes.Cast<SiteMapNode>().ToList();
                            lstSubMnu.ForEach(subMnu =>
                            {
                                ul.Controls.Add(ConstructMenu(subMnu));
                                if (subMnu != lstSubMnu.Last())
                                {
                                    li = new HtmlGenericControl("li");
                                    li.Attributes.Add("class", "divider");
                                    ul.Controls.Add(li);
                                }
                            });
                            ctrl.Controls.Add(ul);
                        }
                        e.Item.Controls.Add(ctrl);
                    });
                }
            }
        }

        private Control ConstructMenu(SiteMapNode pSiteMapNode)
        {
            HtmlGenericControl li = new HtmlGenericControl("li");
            HyperLink lnk = new HyperLink();
            lnk.Text = Ressource.GetString(pSiteMapNode.Title, true).ToUpper();
            lnk.NavigateUrl = pSiteMapNode.Url;

            if (pSiteMapNode.HasChildNodes)
            {
                li.Attributes.Add("class", "dropdown");
                //lnk.Attributes.Add("class", "dropdown-toggle");
                //lnk.Attributes.Add("data-toggle", "dropdown");
            }
            li.Controls.Add(lnk);
            return li;
        }
    }
}