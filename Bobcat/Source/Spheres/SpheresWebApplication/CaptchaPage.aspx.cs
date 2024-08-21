using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using EFS.ACommon;
using EFS.Common.Web;
namespace EFS.Spheres
{
    /// EG 20240523 [WI941][26663] Security : New Captcha generator    
    public partial class CaptchaPage : PageBase
    {
        protected override void OnInit(EventArgs e)
        {
            InitializeComponent();
            Form = frmCaptcha;
            base.OnInit(e);
        }

        private void InitializeComponent()
        {
            this.ID = "frmCaptcha";
            AutoRefresh = SessionTools.UserPasswordReset;
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            PageTools.SetHead(this, "Captcha", null, null);
            ClientScript.RegisterHiddenField("__SOFTWARE", Software.Name);
            divCaptcha.CssClass = CSSMode;
            divbody.CssClass = CSSMode;

            if (false == IsPostBack)
                GenerateCaptcha();

            lblCaptchaTitle.Text = "Captcha security check";
            lblSpheresVersion.Text = Software.NameMajorMinorType;
            lblSpheresVersion.ToolTip = Software.CopyrightFull;
            lblCaptchaCode.Text = "Enter the code";
            lblCaptchaMessage.Text = "Security check";

            btnRegenerate.Text = "<i class='fas fa-redo-alt'></i> ";
            btnRegenerate.Style.Add(HtmlTextWriterStyle.Cursor, "pointer");
            btnValidate.Text = "<i class='fas fa-sign-in-alt'></i> ";
            btnValidate.Style.Add(HtmlTextWriterStyle.Cursor, "pointer");

            this.frmCaptcha.DefaultButton = btnValidate.ClientID;
            JavaScript.SetInitialFocus(txtCaptchaInput);

        }
        /// <summary>
        /// Génération du code et transformation en image avec déformation
        /// </summary>
        /// EG 20240523 [WI941][26663] New 
        private void GenerateCaptcha()
        {
            CaptchaGenerator captchaGenerator = new CaptchaGenerator();
            string captchaCode = captchaGenerator.GenerateCaptcha(8);
            byte[] captchaImage = captchaGenerator.GenerateCaptchaImage(captchaCode);
            imgCaptcha.ImageUrl = "data:image/png;base64," + Convert.ToBase64String(captchaImage);
            ViewState["CaptchaCode"] = captchaCode;
            Page.SetFocus(txtCaptchaInput);
        }

        /// EG 20240523 [WI941][26663] New 
        protected void OnReset(object sender, EventArgs e)
        {
            txtCaptchaInput.Text = string.Empty;
            GenerateCaptcha();   
        }

        /// <summary>
        /// Timer de raffraichissement automatique du code CAPTCHA
        /// </summary>
        // EG 20240523 [WI941][26663] New 

        protected override void SetAutoRefresh()
        {
            timerReset.Enabled = (AutoRefresh > 0);
            if (timerReset.Enabled)
                timerReset.Interval = AutoRefresh * 1000;
        }

        /// <summary>
        /// Validation de la saisie du code CAPTCHA
        /// </summary>
        /// <returns></returns>
        /// EG 20240523 [WI941][26663] New 
        private bool IsValidCaptcha()
        {
            // Récupérer le code CAPTCHA généré précédemment
            string captchaCode = ViewState["CaptchaCode"] as string;

            // Récupérer le code CAPTCHA saisi par l'utilisateur
            string userInput = txtCaptchaInput.Text.Trim();

            // Comparer les deux codes CAPTCHA
            return string.Equals(captchaCode, userInput, StringComparison.InvariantCulture);
        }
        /// <summary>
        /// Regénération d'un code CAPTCHA 
        /// demandée manuellement ou par activation du timer
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// EG 20240523 [WI941][26663] New 
        protected void OnRegenerate(object sender, EventArgs e)
        {
            GenerateCaptcha();
            divFooter.CssClass = string.Empty;
        }

        /// <summary>
        /// Validation du code saisie par comparaison avec le code sauvegardé
        /// dans  ViewState["CaptchaCode"] 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// EG 20240523 [WI941][26663] New 
        protected void OnValidate(object sender, EventArgs e)
        {
            // Valider le code CAPTCHA
            if (IsValidCaptcha())
            {
                lblCaptchaMessage.Text = "Security check : CAPTCHA code valide";
                divFooter.CssClass = "success";
                SessionTools.ConnectionState = Cst.ConnectionState.FAIL;
                Response.Redirect("default.aspx");
            }
            else
            {
                lblCaptchaMessage.Text = "Security check : Invalid CAPTCHA code !";
                divFooter.CssClass = "error";
                GenerateCaptcha();
            }
        }
    }
}