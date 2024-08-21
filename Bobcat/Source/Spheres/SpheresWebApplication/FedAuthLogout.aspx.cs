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
    /// Description r�sum�e de la fin de session Shibboleth
    /// </summary>
    public partial class FedAuthLogout : PageBase //LoginBase
    {
        private FedAuthPageCommon commonForm;
        protected override void OnInit(EventArgs e)
        {
            InitializeComponent();
            Form = frmFedAuthLogout;
            base.OnInit(e);
            AutoRefresh = 0;

            commonForm = new FedAuthPageCommon(this);

        }
        /// Alimentation de la page de fin de session 
        protected void Page_Load(object sender, System.EventArgs e)
        {
            commonForm.DisplayHeader(pnlHeader);
            DisplayFederateAuthInfo();
        }

        /// <summary>
        /// M�thode requise pour la prise en charge du concepteur - ne modifiez pas
        /// le contenu de cette m�thode avec l'�diteur de code.
        /// </summary>
        private void InitializeComponent()
        {

        }
        /// <summary>
        /// Affichages du bouton de d�connexion si l'utilisateur est authentifi�
        /// </summary>
        protected void DisplayFederateAuthInfo()
        {
            // Contr�le d'existence d'un REMOTE_USER renseign�
            string msg = string.Empty;
            if (StrFunc.IsFilled(commonForm.federateUser))
            {
                // Succ�s : D�connexion possible
                btnLogout.Visible = true;
                msg = Ressource.GetString("SuccessFedAuthLogin");
                plhMessage.Controls.Add(new LiteralControl($"<h1>{commonForm.GetAuthenticateUser()}, {msg}</h1>"));
                btnLogout.Text = Ressource.GetString("FedAuthEnd");
                btnLogout.Click += new EventHandler(this.LogoutToSpheres);

            }
            else
            {
                // Erreur
                btnLogout.Visible = false;
                msg = Ressource.GetString("ErrorFedAuthLogout");
                plhMessage.Controls.Add(new LiteralControl($"<h1>{commonForm.GetAuthenticateUser()}, {msg}</h1>"));
            }

            commonForm.DisplayFederateAuthInfo(plhDetail);
        }
        /// <summary>
        /// D�connexion Spheres puis appel � la m�thode Logout de Shibboleth
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void LogoutToSpheres(object sender, System.EventArgs e)
        {
            Disconnect.Run(this, SessionTools.IsShibbolethAuthenticationType ? "None" : "");
            if (SessionTools.IsShibbolethAuthenticationType)
                JavaScript.ExecuteImmediate(this, "window.parent.location.href='https://efsshib.test.local/Shibboleth.sso/Logout';", true);
        }

    }
}
