using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Web;
using System.Web.Security;
using System.Web.SessionState;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
// EG 20160404 Migration vs2013
using EFS.ACommon;
namespace EFS.Spheres.Trial
{
	/// <summary>
	/// Description résumée de [!output SAFE_CLASS_NAME].
	/// </summary>
	public partial class HelpCryptagePage : System.Web.UI.Page
	{
	
		protected void Page_Load(object sender, System.EventArgs e)
		{
			// Placer ici le code utilisateur pour initialiser la page
		}

		#region Web Form Designer generated code
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

		protected void BtnCrypt_Click(object sender, System.EventArgs e)
		{
            // EG 20160404 Migration vs2013
            //TxtCrypt.Text=FormsAuthentication.HashPasswordForStoringInConfigFile(txtToCrypt.Text.ToString(),DdwTypeCrypt.SelectedItem.Text.ToString());
            TxtCrypt.Text = StrFunc.HashData(txtToCrypt.Text.ToString(), DdwTypeCrypt.SelectedItem.Text.ToString());
        }
	}
}
