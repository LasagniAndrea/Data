using System;
using System.Collections;
using System.Data;
using System.Drawing;
using System.Web;
using System.Web.SessionState;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;

using EFS.Common;
using EFS.ACommon;
using EFS.ApplicationBlocks.Data;
using EFS.Common.Web; 

namespace EFS.Spheres
{
	/// <summary>
	/// Description résumée de Nothing.
	/// </summary>
	public partial class ErrorPage : PageBase
	{

		protected void Page_Load(object sender, System.EventArgs e)
		{
			try
			{
				PageTools.SetHead(this, "Error", null, null);
				
				//
				// RD 20200828 [25392]
				Exception ex = null;
				if (Context.Handler is IRequiresSessionState || Context.Handler is IReadOnlySessionState)
					ex = (Exception)Session["currentError"];

				if (ex == null)
					ex = Server.GetLastError();

				if (ex == null)
					ex = new Exception("Unhandled exception occurred");

				lblErrorPage.Text = string.Empty + this.Request.QueryString["aspxerrorpath"];
				lblErrorMessage.Text = this.ExceptionLastError.Message.ToString();
				lblErrorSource.Text = this.ExceptionLastError.Source.ToString();
				lblErrorDetail.Text = this.ExceptionLastError.StackTrace.ToString();
				// EG 20160308 Migration vs2013
				//lblErrorDate.Text = OTCmlHelper.GetRDBMSDateSys(SessionTools.CS).ToUniversalTime().ToString();
				// FI 20200811 [XXXXX] use OTCmlHelper.GetDateSys puisque GetRDBMSDateSys est obsolete
				//DateTime dtTmp;
				//OTCmlHelper.GetRDBMSDateSys(SessionTools.CS, null, true, false, out dtTmp);
				//lblErrorDate.Text = dtTmp.ToUniversalTime().ToString();
				// FI 20200820 [25468] dates systemes en UTC
				lblErrorDate.Text = OTCmlHelper.GetDateSysUTC(SessionTools.CS).ToString();
				lblErrorApplication.Text = SessionTools.AppSession.AppInstance.AppNameVersion;
				if (SessionTools.IsSourceVisible)
					lblErrorRDBMS.Text = @"\\" + SessionTools.Data.Server + @"\" + SessionTools.Data.DatabaseNameVersionBuild + @" as ";
				else
					lblErrorRDBMS.Text = string.Empty;
				lblErrorRDBMS.Text += SessionTools.Data.RDBMS;
				lblErrorBrowser.Text = SessionTools.AppSession.BrowserInfo;
				lblErrorCulture.Text = System.Threading.Thread.CurrentThread.CurrentUICulture.ToString();
				lblErrorUser.Text = SessionTools.Collaborator_ENTITY_IDENTIFIER + @"\" + SessionTools.Collaborator_IDENTIFIER + @" as " + SessionTools.Collaborator_ROLE;
			}
			catch
			{
				try
				{
					lblErrorMessage.Text = "Unhandled exception occurred.";
					lblErrorSource.Text = string.Empty;
					lblErrorDetail.Text = string.Empty;
				}
				catch
				{ }
			}
			try
			{
				this.ExceptionLastError = null;
				if (Context.Handler is IRequiresSessionState || Context.Handler is IReadOnlySessionState)
					Session["currentError"] = null;
			}
			catch
			{ }
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
