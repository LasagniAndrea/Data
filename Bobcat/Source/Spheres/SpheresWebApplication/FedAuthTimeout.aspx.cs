using EFS.ACommon;
using EFS.Common.Web;
using System;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using EFS.Common;

namespace EFS.Spheres
{
    /// <summary>
    /// Description résumée de Welcome.
    /// </summary>
    public partial class FedAuthTimeout : PageBase
    {
        private FedAuthPageCommon commonForm;
        protected override void OnInit(EventArgs e)
        {
            InitializeComponent();
            Form = frmFedAuthTimeout;
            base.OnInit(e);
            AutoRefresh = 0;

            commonForm = new FedAuthPageCommon(this);
        }
        protected void Page_Load(object sender, System.EventArgs e)
        {
            commonForm.DisplayHeader(pnlHeader);
            DisplayFederateAuthInfo();
        }

        /// <summary>
        /// Méthode requise pour la prise en charge du concepteur - ne modifiez pas
        /// le contenu de cette méthode avec l'éditeur de code.
        /// </summary>
        private void InitializeComponent()
        {

        }

        /// <summary>
        /// Affichage du message de l'Authentification et du bouton de reprise de session
        /// </summary>
        private void DisplayFederateAuthInfo()
        {
            btnContinue.Visible = true;
            string msg = Ressource.GetString("MessageFedAuthTimeout");
            plhMessage.Controls.Add(new LiteralControl($"<h1>{commonForm.GetAuthenticateUser()}, {msg}</h1>"));
            btnContinue.Text = Ressource.GetString("FedAuthContinue");
            btnContinue.Click += new EventHandler(this.ContinueToSpheres);
        }
        /// <summary>
        /// Reprise de session par chargement de la page défaut
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        // EG 20221010 [XXXXX] Changement de nom de la page principale : mainDefault en default
        protected void ContinueToSpheres(object sender, System.EventArgs e)
        {
            JavaScript.ExecuteImmediate(this, "window.location.href='Default.aspx';", true);
        }

    }
}
