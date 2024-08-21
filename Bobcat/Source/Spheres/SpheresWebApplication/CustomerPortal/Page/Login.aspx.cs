using EFS.ACommon;
using EFS.Common;
using EFS.Common.Web;
using System;
using System.Globalization;
using System.Linq;
using System.Threading;

namespace EFS.Spheres
{
    /// EG 20210614 [25500] New Customer Portal
    public partial class Login : LoginBase
    {
        /// <summary>
        /// Méthode requise pour la prise en charge du concepteur - ne modifiez pas
        /// le contenu de cette méthode avec l'éditeur de code.
        /// </summary>
        private void InitializeComponent()
        {
            this.ID = "portalLogin";
        }

        // EG 20210921 [XXXXX] Sur la page Login on charge la ressource du poste
        protected override void OnInit(EventArgs e)
        {
            InitializeComponent();
            Form = portalLogin;
            base.OnInit(e);
            // On initialise la culture avec celle par de la machine
            if ((null != Request.UserLanguages) && (0 < Request.UserLanguages.Count()))
            {
                string culture = Request.UserLanguages[0].Trim();
                Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture(culture);
            }
        }

        // EG 20210921 [25290] Application du reset comme sur la page Login de base
        protected void OnReset(object sender, EventArgs e)
        {
            txtIdentifier.Text = string.Empty;
            txtPassword.Text = string.Empty;
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            // EG 20210908 [XXXXX] Add message Msg_WaitingConnection
            JQuery.Block block = new JQuery.Block((PageBase)this.Page)
            {
                Width = "150px",
                Timeout = SystemSettings.GetTimeoutJQueryBlock("LG")
            };
            JQuery.UI.WriteInitialisationScripts(this, block);
            btnSignIn.OnClientClick = $"Block({JavaScript.HTMLString(Ressource.GetString("Msg_WaitingConnection"))});stopResetTimer();return true;";

            ClientScript.RegisterHiddenField("__SOFTWARE", Software.Name);

            string returnUrl = Request.QueryString["ReturnUrl"];
            if (String.IsNullOrEmpty(returnUrl) || (false == returnUrl.ToLower().Contains("customerportal"))
                || returnUrl.ToLower().Contains("welcome"))
            {
                hdrLogin.Attributes.Add("class", "hide");
                ftrLogin.Attributes.Add("class", "hide");
            }

            txtIdentifier.ToolTip = Ressource.GetString("InputIdentifier");
            txtPassword.ToolTip = Ressource.GetString("InputPwd");
            loginTitle.InnerText = Ressource.GetString("LoginCustomerPortal");
            lblLoginMsg.Text = string.Empty;
            lblLockoutMsg.Text = string.Empty;

            // - Mise en place d'un DefaultButton sur le formulaire
            this.portalLogin.DefaultButton = btnSignIn.ClientID;
            JavaScript.SetInitialFocus(txtIdentifier);

        }
        protected void BtnLogin_Click(object sender, EventArgs e)
        {
            SessionTools.ConnectionState = Cst.ConnectionState.LOGINREQUEST;
            SetAutoRefresh();
            divErrLockout.Visible = false;
            LoginData data = SetData(txtPassword.Text);
            LoginProcess(data);
            divErrLockout.Visible = StrFunc.IsFilled(data.LockoutMessage);
            lblLockoutMsg.Text = data.LockoutMessage;
            lblLoginMsg.Visible = (StrFunc.IsFilled(data.LoginMessage.Key));
            lblLoginMsg.Text = data.LoginMessage.Key;
            lblLoginMsg.ToolTip = data.LoginMessage.Value;
            SetAutoRefresh();
        }

        // EG 20210921 [25290] Mécanisme d'ignorement du couple user/pwd comme la page Login de base
        protected override void SetAutoRefresh()
        {
            timerReset.Enabled = (AutoRefresh > 0);
            if (timerReset.Enabled)
                timerReset.Interval = AutoRefresh * 1000;
        }

        protected override LoginData SetData(string pPassword)
        {
            LoginData data = new LoginData
            {
                LblIdentifier = lblIdentifier.Text,
                LblPassword = lblPassword.Text,
                Identifier = txtIdentifier.Text,
                Password = pPassword
            };

            string rdbmsName, serverName, databaseName, userName, pwd;
            rdbmsName = serverName = databaseName = userName = pwd = null;
            SystemSettings.GetDefaultConnectionCharacteristics(false, this.Request.ApplicationPath, ref rdbmsName, ref serverName, ref databaseName, ref userName, ref pwd);

            if (rdbmsName.Length > 0)
                data.Rdbms = rdbmsName;
            if (serverName.Length > 0)
                data.ServerName = serverName;
            if (databaseName.Length > 0)
                data.DatabaseName = databaseName;

            return data;
        }

    }
}