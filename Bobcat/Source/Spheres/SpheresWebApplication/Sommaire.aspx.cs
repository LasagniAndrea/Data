using System;
using System.Threading;
using System.Globalization;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;

using EFS.ACommon;
using EFS.Authenticate;
using EFS.Actor;
using EFS.ApplicationBlocks.Data;
using EFS.Common;
using EFS.Referential;

using EFS.Common.Web;
using EFS.Common.Log;


using EFS.Restriction;
//20071212 FI Ticket 16012 => Migration Asp2.0
using SpheresMenu = EFS.Common.Web.Menu;

namespace EFS.Spheres
{
    /// <summary>
    /// Description résumée de Sommaire
    /// </summary>
    // EG 20210120 [25556] Complement : New version of JQueryUI.1.12.1 (JS and CSS)
    // EG 20210120 [25556] Add summaryTtitle on cssClass
    /// EG 20210614 [25500] New Customer Portal
    public partial class SommairePage : SummaryCommon
    {
        #region members
        protected override PlaceHolder CtrlMenu { get { return phMenu; } }
        protected override HtmlInputHidden CtrlMaskMenu { get { return hidMaskMenu; } }
        protected override HtmlInputHidden CtrlIdA { get { return hidIDA; } }
        protected override WCToolTipLinkButton BtnMaskMenu { get { return btnMaskMenu; } }

        protected string Ressource_lnkBtnProfil = Ressource.GetString("btnProfil");
        #endregion members

        // EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        // EG 20210212 [25661] New Appel Protection CSRF(Cross-Site Request Forgery)
        // EG 20210222 [XXXXX] Suppression inscription function JavascriptCookieFunctions
        protected override void OnInit(EventArgs e)
        {
            TblLoginLogout.Attributes.Add("class",this.CSSMode + " " + SessionTools.Company_CssColor + " summarytitle" + (Software.IsSoftwarePortal()? " portal":""));

            InitializeComponent();
            base.OnInit(e);

            Form = Sommaire;
            AntiForgeryControl();

            //JavaScript.JavascriptCookieFunctions((PageBase)this);
        }

        /// <summary>
        /// Méthode requise pour la prise en charge du concepteur.
        /// Warning: Ne pas modifiez le contenu de cette méthode avec l'éditeur de code.
        /// </summary>
        private void InitializeComponent()
        {
            this.ID = "Sommaire";
        }

        // EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        // EG 20210222 [XXXXX] Suppression inscription function AutoTopSelfClose
        /// EG 20210614 [25500] New Customer Portal
        // EG 20210921 [XXXXX] La base base target est initialisée à main 
        protected void Page_Load(object sender, System.EventArgs e)
        {
            PageTools.SetHead(this, "Summary", null, null);

            //hidMaskMenu.Attributes["title"] = Ressource.GetString("imgBtnMaskMenu");

            if (this.Request.QueryString["Quit"] == "1")
                JavaScript.CallFunction(this, "top.self.close();");

            LoadSummary();
            this.Header.Controls.Add(new LiteralControl(@"<base target=""main""/>"));

            btnLogout.Pty.TooltipContent = Ressource.GetString("imgBtnLogout", true);
            btnLogout.Attributes.Add("onclick", "javascript:" + ClientScript.GetPostBackEventReference(btnLogout, null));
            btnLogout2.Text = Ressource.GetString("imgBtnLogout", true);
        }

        // EG 20210224 [XXXXX] suppresion PageBase.js déja appelé dans Render de PageBase
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

            // Force creation of script __doPostBack()
            HtmlInputHidden hdn = new HtmlInputHidden();
            hdn.Attributes.Add("onclick", "javascript:" + ClientScript.GetPostBackEventReference(this, null));
            this.Page.Controls.Add(hdn);

            ScriptManager.Scripts.Add(new ScriptReference("~/Javascript/Summary.min.js"));

        }

        protected void BtnLogout_Click(object sender, System.EventArgs e)
        {
            Disconnect.Run(this);
        }

        private void BtnQuit_Click(object sender, System.EventArgs e)
        {
            Disconnect.Run(this);
        }

    }
}