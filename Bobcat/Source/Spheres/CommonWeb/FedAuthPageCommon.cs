#region Using Directives
using EFS.ACommon;
using System;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
#endregion Using Directives

// EG 20231129 [WI756] Spheres Core : Refactoring Code Analysis with Intellisense

namespace EFS.Common.Web
{
    /// <summary>
    /// Méthode communes aux 3 formes utilisées en Authentification fédérée.
    /// FedAuthLogin, FedAuthLogout et FedAuthTimeout. 
    /// </summary>
    public partial class FedAuthPageCommon
    {
        private readonly PageBase currentPage;
        protected string title;
        public string federateUser;
        private Control ctrlBody;

        public FedAuthPageCommon(PageBase pPage)
        {
            currentPage = pPage;
            // Récupération du REMOTE_USER de l'assertion
            federateUser = currentPage.Request["REMOTE_USER"];
            title = Ressource.GetString(currentPage.Page.Form.Name);
        }

        // EG [XXXXX] Look Header de la fenêtre en fonction du résultat de l'authentification (couleur)
        public void DisplayHeader(WebControl pCtrlHeader)
        {
            string subTitle = Ressource.GetString("lblFedAuth");
            if (StrFunc.IsFilled(federateUser))
                pCtrlHeader.CssClass = "authentified";
            pCtrlHeader.Controls.Add(new Label { Text = subTitle });
            pCtrlHeader.Controls.Add(new Label { Text = title });
            currentPage.PageTitle = subTitle + "-" + title;

            if ((null == currentPage.Header) && (null == PageTools.SearchHeadControl(currentPage)))
                throw new NotSupportedException("This page is missing a head runat server. Please add <head runat=\"server\" />.");

            Control head = (Control)currentPage.Header;
            PageTools.SetHeaderTitle(head, "titlePage", currentPage.PageTitle);
            PageTools.SetMetaTag(head);

            //Add linkCssCommon
            PageTools.SetHeaderLink(head, "linkCssAwesome", currentPage.VirtualPath("Includes/fontawesome-all.min.css"));
            PageTools.SetHeaderLink(head, "linkCssCommon", currentPage.VirtualPath("Includes/EFSThemeCommon.min.css"));
            PageTools.SetHeaderLink(head, "linkCssCustomCommon", currentPage.VirtualPath("Includes/CustomThemeCommon.css"));
            PageTools.SetHeaderLink(head, "linkCssUISprites", currentPage.VirtualPath("Includes/EFSUISprites.min.css"));
            PageTools.SetHeaderLink(head, "linkCssUISprites", currentPage.VirtualPath("Includes/Tracker.min.css"));

            string cssColor = string.Empty;
            AspTools.CheckCssColor(ref cssColor);
            currentPage.CSSColor = cssColor;

            string cssMode = null;
            AspTools.CheckCssMode(ref cssMode);
            currentPage.CSSMode = cssMode;

            string linkCss = currentPage.VirtualPath(String.Format("Includes/EFSTheme-{0}.min.css", cssMode));
            PageTools.SetHeaderLink(head, "linkCss", linkCss);

            JQuery.UI.WriteHeaderLink(head, JQuery.Engines.JQuery);
            HtmlGenericControl style = new HtmlGenericControl("style");
            style.Attributes.Add("type", "text/css");
            style.InnerText = "THEAD { DISPLAY: table-header-group }  TFOOT { DISPLAY: table-footer-group }";
            head.Controls.Add(style);
        }

        public void DisplayFederateAuthInfo(Control pCtrlBody)
        {
            ctrlBody = pCtrlBody;
            // Affichage des résultats si spécifié dans le fichier de Configuration
            bool isDisplayResults = BoolFunc.IsTrue(SystemSettings.GetAppSettings("FederateAuthResults"));
            if (isDisplayResults)
            {
                DisplayTitle("<H3>Federate user</H3>");
                DisplayDetail("AUTH_USER", currentPage.Request["AUTH_USER"]);
                DisplayDetail("LOGON_USER", currentPage.Request["LOGON_USER"]);
                DisplayDetail("REMOTE_USER", currentPage.Request["REMOTE_USER"]);

                DisplayTitle("<H3>Federate user attributes</H3>");
                DisplayDetail("displayName", currentPage.Request["displayName"]);
                DisplayDetail("givenName", currentPage.Request["givenName"]);
                DisplayDetail("mail", currentPage.Request["mail"]);
                DisplayDetail("email", currentPage.Request["email"]);
                DisplayDetail("role", currentPage.Request["role"]);
                DisplayDetail("surName", currentPage.Request["surName"]);
                DisplayDetail("telephoneNumber", currentPage.Request["telephoneNumber"]);
                DisplayDetail("department", currentPage.Request["employeeNumber"]);
                DisplayDetail("uid", currentPage.Request["uid"]);
                DisplayDetail("oneLoginId", currentPage.Request["oneLoginId"]);

                DisplayTitle("<H3>HttpContext.Current.User Identity</H3>");
                DisplayDetail("Name", HttpContext.Current.User.Identity.Name);
                DisplayDetail("Authenticated", HttpContext.Current.User.Identity.IsAuthenticated);
                DisplayDetail("Authentication type", HttpContext.Current.User.Identity.AuthenticationType);

            }
        }

        public string GetAuthenticateUser()
        {
            string ret = currentPage.Request["displayName"];
            if (String.IsNullOrEmpty(ret))
                ret = currentPage.Request["surName"];
            if (String.IsNullOrEmpty(ret))
                ret = currentPage.Request["givenName"];
            if (String.IsNullOrEmpty(ret))
                ret = currentPage.Request["uid"];

            if (String.IsNullOrEmpty(ret))
                ret = federateUser;
            else
                ret += " (" + federateUser + ")";
            return ret;
        }

        private void DisplayTitle(string pTitle)
        {
            DisplayElement(pTitle, "title");
        }

        private void DisplayDetail(string pName, object pValue)
        {
            if (null != pValue)
            {
                DisplayElement(pName, "label");
                DisplayElement(pValue.ToString(), "value");
            }
        }
        private void DisplayElement(string pValue, string pClass)
        {
            Panel pnlContainer = new Panel() { CssClass = pClass };
            pnlContainer.Controls.Add(new Label() { Text = pValue });
            ctrlBody.Controls.Add(pnlContainer);
        }
    }
}

