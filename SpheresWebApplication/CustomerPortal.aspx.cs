using EFS.ACommon;
using EFS.Actor;
using EFS.ApplicationBlocks.Data;
using EFS.Authenticate;
using EFS.Common;
using EFS.Common.Web;
using EFS.Referential;
using System;
using System.Data;
using System.Threading;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using SpheresMenu = EFS.Common.Web.Menu;

namespace EFS.Spheres
{
    /// <summary>
    /// Page de démarrage du Portail réservé à nos clients
    /// </summary>
	/// EG 20210614 [25500] New Customer Portal
    public partial class CustomerPortal : SummaryCommon
    {
        protected override PlaceHolder CtrlMenu { get { return phMenu; } }
        protected override HtmlInputHidden CtrlMaskMenu { get { return hidMaskMenu; } }
        protected override HtmlInputHidden CtrlIdA { get { return hidIDA; } }
        protected override WCToolTipLinkButton BtnMaskMenu { get { return btnMaskMenu; } }

        protected override void OnInit(System.EventArgs e)
        {
            CtrlMenu.Controls.Clear();
            Form = frmCustomerPortal;
            base.OnInit(e);
            AddInputHiddenAutoPostback();
        }

        // EG 20210921 [XXXXX] La base base target est initialisée à _self
        protected void Page_Load(object sender, EventArgs e)
        {
            lnkHome.Text = "<i class='fas fa-home'></i> ";
            lnkHelp.Text = "<i class='fas fa-question-circle'></i>";
            lnkDisconnect.Text = Ressource.GetString("btnLogout"); 
            lnkProfile.Text = Ressource.GetString("imgBtnCulture");
            dduser.InnerHtml = String.Format("<i class='fa fa-user'> ({0}) </i><p id='user-name'></p><i class='fa fa-caret-down'></i>", Thread.CurrentThread.CurrentUICulture.TwoLetterISOLanguageName);
            lnkHome.Attributes.Add("onclick", "document.getElementById('main').src='CustomerPortal/Page/Welcome.aspx';return false;");
            lnkHelp.Attributes.Add("onclick", "window.open('/ApplicationHelp.htm','Help');return false;");
            lnkDisconnect.Attributes["href"] = "#";
            /// EG 20220623 [XXXXX] Implémentation Authentification via Shibboleth SP et IdP
            if (SessionTools.IsShibbolethAuthenticationType)
            {
                lnkDisconnect.Attributes.Add("onclick", "document.getElementById('main').src='FedAuthLogout.aspx';return false;");
            }
            else
            {
               lnkDisconnect.Attributes.Add("onclick", "javascript:" + Page.ClientScript.GetPostBackEventReference(lnkDisconnect, ""));
            }
            lnkProfile.Attributes.Add("onclick", "document.getElementById('main').src='Profile.aspx';return false;");
            lnkHome.ToolTip = Ressource.GetString("imgBtnHome");
            lnkHelp.ToolTip = Ressource.GetString("imgBtnHelp");

            lastLogin.Text = string.Empty;

            PageTools.SetHead(this, "Customer portal", null, null);

            LoadSummary();
            this.Header.Controls.Add(new LiteralControl(@"<base target=""_self""/>"));

            btnSlideMenu.Attributes.Add("onclick", "SlideMenu();return false;");
            btnSlideMenu.Attributes["title"] = Ressource.GetString("imgBtnSlideMenu");
        }

        // EG 20240619 [WI945] Security : Update outdated components (New QTip2)
        protected override void CreateChildControls()
        {
            // Override complet de PageBase (pas d'appel)
            JQuery.WriteEngineScript(this, JQuery.Engines.JQuery);
            JQuery.UI.WritePluginScript(this, JQuery.Engines.JQuery);
            JQuery.UI.WriteInitialisationScripts(this, new JQuery.Confirm());
            JQuery.UI.WriteInitialisationScripts2(this, new JQuery.Dialog());
            JQuery.UI.WriteInitialisationScripts(this, new JQuery.Toggle());
            JQuery.UI.WriteInitialisationScripts(this, new JQuery.QTip2());

            ScriptManager.Scripts.Add(new ScriptReference("~/Javascript/Summary.min.js"));
            ScriptManager.Scripts.Add(new ScriptReference("~/Javascript/PageBase.min.js"));
        }
    }
}