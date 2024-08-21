using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.SessionState;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.ServiceModel.Diagnostics;

//using Spheres;
using EFS.Common;
using EFS.Common.Web;

namespace EFS.Spheres
{
    public partial class ErrorPage : Page
    {
        protected Exception ex = null;
        public string OriginalUrl { get; private set; }

        protected void Page_Load(object sender, EventArgs e)
        {
            AppInstance appInstance = SessionTools.NewAppInstance();

            string generalErrorMsg = "A problem has occurred on this web site. Please try again. " + "If this error continues, please contact support.";
            string httpErrorMsg = "An HTTP error occurred. Page Not found.";
            string unhandledErrorMsg = "The error was unhandled by application code.";

            innerTrace.InnerText = string.Empty;
            innerTarget.Text = string.Empty;
            innerMessage.Text = string.Empty;
            string _errorTarget = string.Empty;

            friendlyErrorMsg.Text = generalErrorMsg; // Request.QueryString["aspxerrorpath"];

            // Get the last error from the server.
            Exception ex = null;
            if (Context.Handler is IRequiresSessionState || Context.Handler is IReadOnlySessionState)
                ex = Session["currentError"] as Exception;

            // Get the error number passed as a querystring value.
            string statusCode = Request.QueryString["StatusCode"];
            if (statusCode == "404")
            {
                ex = new HttpException(404, httpErrorMsg, ex);
                innerTarget.Text = Request.QueryString["requestedUrl"];
                friendlyErrorMsg.Text = ex.Message;
            }

            // If the exception no longer exists, create a generic exception.
            if (ex == null)
            {
                ex = new Exception(unhandledErrorMsg);
            }

            // Show error details to only you (developer). LOCAL ACCESS ONLY.
            if (Request.IsLocal)
            {
                // Detailed Error Message.
                errorDetailedMsg.Text = ex.Message;

                // Show local access details.
                detailedErrorPanel.Visible = true;
                innerTrace.Visible = (null != ex.InnerException) || (null != ex.StackTrace);
                if (null != ex.InnerException)
                {
                    innerMessage.Text = ex.GetType().ToString() + "<br/>" +
                        ex.InnerException.Message;
                    innerTrace.InnerText = ex.InnerException.StackTrace;
                    if (null != ex.TargetSite)
                        innerTarget.Text = ex.InnerException.TargetSite.Name;
                }
                else
                {
                    innerMessage.Text = ex.GetType().ToString();
                    if (null != ex.StackTrace)
                    {
                        innerTrace.InnerText = ex.StackTrace.ToString().TrimStart();
                    }
                    if (null != ex.TargetSite)
                        innerTarget.Text = ex.TargetSite.Name;
                }
            }

            // Log the exception and notify system operators
            ExceptionUtility.LogException(ex, "Spheres Error Page");
            ExceptionUtility.NotifySystemOps(ex);

            // Clear the error from the server
            //Server.ClearError();        

        }
    }
}