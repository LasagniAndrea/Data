using System;
using System.Threading;
using System.Web.UI;
using EFS.Common;
using EFS.Common.Web;
using EFS.ApplicationBlocks.Data;
using System.Web.SessionState;

namespace EFS.Spheres.Errors
{
	/// EG 20210614 [25500] New Customer Portal
	public partial class MasterError : MasterPage
    {
		public Exception masterException;
		public AppInstance appInstance;

		protected void Page_Init(object sender, EventArgs e)
		{
			try
			{
				appInstance = SessionTools.AppInstance;
				masterException = null;
				if (Context.Handler is IRequiresSessionState || Context.Handler is IReadOnlySessionState)
					masterException = (Exception)Session["currentError"];

				if (masterException == null)
					masterException = Server.GetLastError();

				if (masterException == null)
					masterException = new Exception("Unhandled exception occurred");

				lblErrorMessage.InnerText = masterException.Message.ToString();


				lblErrorDate.Text = OTCmlHelper.GetDateSysUTC(SessionTools.CS).ToString("ddd, MMM d yyyy", Thread.CurrentThread.CurrentUICulture);
				lblErrorApplication.Text = appInstance.AppNameVersion;
				lblErrorUser.Text = SessionTools.Collaborator_ENTITY_IDENTIFIER + @"\" + SessionTools.Collaborator_IDENTIFIER + @" as " + SessionTools.Collaborator_ROLE;

				lblErrorBrowser.Text = SessionTools.AppSession.BrowserInfo;
				lblErrorCulture.Text = Thread.CurrentThread.CurrentUICulture.ToString();
			}
			catch
			{
				try
				{
					lblErrorMessage.InnerText = "Unhandled exception occurred.";
				}
				catch
				{ }
			}
		}
	}
}