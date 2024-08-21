using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Web;
using System.Web.SessionState;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;

using EFS.Common; 
using EFS.Common.Web;   
using EFS.ACommon;


namespace EFS.Spheres
{
	/// <summary>
	/// Description résumée de Nothing.
	/// </summary>
	public partial class UnderConstructionPage : PageBase
	{

        protected void Page_Load(object sender, System.EventArgs e)
        {
            try
            {
                PageTools.SetHead(this, "UnderConstruction", null, null);
                lblTitlePage.Text     = string.Empty + this.Request.QueryString["IdMenu"];
                lblTitlePage.Text     = Ressource.GetString(lblTitlePage.Text,lblTitlePage.Text);
                
                lblUnderConstruction.Text   = Ressource.GetString("LBL_UNDER_CONSTRUCTION","Sorry, the page you requested is under construction !");
                
                lblUnderConstructionContact.Text   = Ressource.GetString("LBL_UNDER_CONSTRUCTION_CONTACT","For additional assistance, contact the editor");               
            }
            catch
            {
                try
                {
                    string errorMessage = string.Empty + this.Request.QueryString["ErrorMessage"];
					lblUnderConstruction.Text= (errorMessage.Trim().Length==0 ? "Error undefined.":errorMessage);                    
                }
                catch
                {}
            }
            try
            {
                this.ExceptionLastError = null;
            }
            catch
            {}
        }

		#region Code généré par le Concepteur Web Form
		override protected void OnInit(EventArgs e)
		{
			//
			// CODEGEN : Cet appel est requis par le Concepteur Web Form ASP.NET.
			//
			InitializeComponent();
			base.OnInit(e);
		}
		
		/// <summary>
		/// Méthode requise pour la prise en charge du concepteur - ne modifiez pas
		/// le contenu de cette méthode avec l'éditeur de code.
		/// </summary>
		private void InitializeComponent()
		{    

        }
		#endregion
	}
}
