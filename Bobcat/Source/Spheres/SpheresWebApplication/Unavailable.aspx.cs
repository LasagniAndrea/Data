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

using EFS.ACommon;
using EFS.Common; 
using EFS.Common.Web;

namespace EFS.Spheres
{
	/// <summary>
	/// Description r�sum�e de Nothing.
	/// </summary>
	public partial class UnavailablePage : PageBase
	{

        protected void Page_Load(object sender, System.EventArgs e)
        {

			try
			{
				PageTools.SetHead(this, "UnavailablePage", null, null);

				// RD 20200828 [25392]
				string errorMessage = this.Request.QueryString["ErrorMessage"];
				if (StrFunc.IsEmpty(errorMessage))
					errorMessage = "Page not found.";
								
				Exception ex = null;
				if (Context.Handler is IRequiresSessionState || Context.Handler is IReadOnlySessionState)
					ex = (Exception)Session["currentError"];
				if (ex == null)
					ex = Server.GetLastError();

				if (ex != null)
					errorMessage = errorMessage + " - " + ex.Message.ToString();

				lblErrorPage.Text   = string.Empty + this.Request.QueryString["aspxerrorpath"];
                lblErrorMessage.Text= errorMessage;
            }
			catch
			{
				try
				{
					lblErrorMessage.Text = "Page not found.";
					if (Context.Handler is IRequiresSessionState || Context.Handler is IReadOnlySessionState)
						Session["currentError"] = null;
				}
				catch
				{ }
			}
			try
			{
				this.ExceptionLastError = null;
			}
			catch
			{ }
		}

		#region Code g�n�r� par le Concepteur Web Form
		override protected void OnInit(EventArgs e)
		{
			//
			// CODEGEN�: Cet appel est requis par le Concepteur Web Form ASP.NET.
			//
			InitializeComponent();
			base.OnInit(e);
		}
		
		/// <summary>
		/// M�thode requise pour la prise en charge du concepteur - ne modifiez pas
		/// le contenu de cette m�thode avec l'�diteur de code.
		/// </summary>
		private void InitializeComponent()
		{    

        }
		#endregion
	}
}
