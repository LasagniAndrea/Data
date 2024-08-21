using EFS.ACommon;
using EFS.Common;
using EFS.Common.Web;
using System;
using System.Web.UI;

namespace EFS.Spheres
{
    /// <summary>
    /// Description résumée de l'accès à Spheresaffichage des résultats .
    /// </summary>
    public partial class FedAuthLogin : LoginBase
    {
        private FedAuthPageCommon commonForm;

        protected override void OnInit(EventArgs e)
        {
            InitializeComponent();
            Form = frmFedAuthLogin;
            base.OnInit(e);
            AutoRefresh = 0;

            commonForm = new FedAuthPageCommon(this);

        }
        /// <summary>
        /// Chargement de la page retour pour lecture des assertions Shibboleth
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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
        /// Connexion automatique à Spheres après authetification Shibboleth
        /// Utilisation de la donnée retournée dans le REMOTE_USER pour recherche automatique de
        /// l'utilisateur dans la base SPHERES
        /// => Si succès alors redirection vers Default.aspx
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        // EG 20221010 [XXXXX] Changement de nom de la page principale : mainDefault en default
        protected void LoginToSpheres(object sender, System.EventArgs e)
        {
            LoginData loginData = new LoginData
            {
                Identifier = commonForm.federateUser
            };

            string rdbmsName, serverName, databaseName, userName, pwd;
            rdbmsName = serverName = databaseName = userName = pwd = null;
            SystemSettings.GetDefaultConnectionCharacteristics(false, this.Request.ApplicationPath, ref rdbmsName, ref serverName, ref databaseName, ref userName, ref pwd);

            if (rdbmsName.Length > 0)
                loginData.Rdbms = rdbmsName;
            if (serverName.Length > 0)
                loginData.ServerName = serverName;
            if (databaseName.Length > 0)
                loginData.DatabaseName = databaseName;
            if (Cst.ErrLevel.SUCCESS != this.LoginProcess(loginData))
            {
                // Erreur d'identification
                lblLoginMsg.Visible = (StrFunc.IsFilled(loginData.LoginMessage.Key) || StrFunc.IsFilled(loginData.LockoutMessage));
                lblLoginMsg.Text = loginData.LoginMessage.Key;
                lblLoginMsg.ToolTip = loginData.LoginMessage.Value;
                if (StrFunc.IsFilled(loginData.LockoutMessage))
                    lblLoginMsg.Text += loginData.LockoutMessage;

            }
            else
            {
                // Succès redirection vers la page d'accueil
                Response.Redirect("Default.aspx", true);
            }
        }

        /// <summary>
        /// Résultats de l'authentification Shibboleth
        /// </summary>
        private void DisplayFederateAuthInfo()
        {
            string msg = string.Empty;
            lblLoginMsg.Visible = false;
            // Contrôle d'existence d'un REMOTE_USER renseigné
            if (StrFunc.IsFilled(commonForm.federateUser))
            {
                // Succès
                msg = Ressource.GetString("SuccessFedAuthLogin");
                plhMessage.Controls.Add(new LiteralControl($"<h1>{commonForm.GetAuthenticateUser()}, {msg}</h1>"));
                btnAccess.Text = Ressource.GetString("FedAuthAccess");
                btnAccess.Click += new EventHandler(this.LoginToSpheres);
            }
            else
            {
                // Erreur
                msg = Ressource.GetString("ErrorFedAuthLogin"); // Authentication aborted or incomplete
                btnAccess.Visible = false;
                plhMessage.Controls.Add(new LiteralControl($"<h1>{msg}</h1>"));
            }

            commonForm.DisplayFederateAuthInfo(plhDetail);
        }
    }
}
