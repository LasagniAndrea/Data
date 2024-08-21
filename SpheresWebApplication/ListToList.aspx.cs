using EFS.Common.Web;
using System;
using System.Web.UI.WebControls;

namespace EFS.Spheres
{
    public partial class ListToListPage : PageBase
	{
        protected string leftTitle, rightTitle;
        //protected System.Web.UI.HtmlControls.HtmlInputHidden hidSelection;

        protected void Page_Load(object sender, System.EventArgs e)
        {          
            TextBox txtfound = (TextBox) plhMain.FindControl("txtDisabled");
            if(txtfound != null)
            {
                //txtfound.Enabled = false;
                if (false == IsPostBack)
                {
                    txtfound.Text = "disabled";
                }            
            }

            TextBox txtfound2 = (TextBox) plhMain.FindControl("txtEnabled");
            if(txtfound2 != null)
            {
                //txtfound2.Enabled = true;
                if (false == IsPostBack)
                {
                    txtfound2.Text = "enabled";
                }            
            }

            if (false == IsPostBack)
            {
                TextBox1.Text = "toto";
            }

        }

        #region Code généré par le Concepteur Web Form
        // EG 20210212 [25661] New Appel Protection CSRF(Cross-Site Request Forgery)
        override protected void OnInit(EventArgs e)
        {
            InitializeComponent();
            base.OnInit(e);

            Form = frmListToList;
            AntiForgeryControl();
            //
            //TextBox en 'dur'
            if (false == IsPostBack)
            {
                TextBox1.Enabled = false;
                TextBox1.EnableViewState = true;
            }
            //
            //TextBox créé en dynamique
            TextBox txtEnabled = new TextBox
            {
                Enabled = true,
                //txtEnabled.EnableViewState = true;
                ID = "txtEnabled"
            };
            //
            TextBox txtDisabled = new TextBox
            {
                Enabled = false,
                //txtDisabled.EnableViewState = true;
                ID = "txtDisabled"
            };
            //
            if (false == IsPostBack)
            {
                txtEnabled.Text = "enabled";
                txtDisabled.Text = "disabled";
            }
            //
            plhMain.Controls.Add(txtDisabled);
            plhMain.Controls.Add(txtEnabled);

        }
		
        /// <summary>
        /// Méthode requise pour la prise en charge du concepteur - ne modifiez pas
        /// le contenu de cette méthode avec l'éditeur de code.
        /// </summary>
        private void InitializeComponent()
        {    

        }
        #endregion

        protected void BtnPostBack_Click(object sender, System.EventArgs e)
        {

            //JavaScript.AlertStartUpImmediate((PageBase)this,"post back done",false);
            JavaScript.DialogStartUpImmediate(this, "post back done", false);   
        
        }
    }
}
