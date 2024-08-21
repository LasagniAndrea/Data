using System.Web;
using System.Web.UI;


// EG 20231129 [WI756] Spheres Core : Refactoring Code Analysis with Intellisense

namespace EFS.Common.Web
{
    /// <summary>
    /// Description résumée de JavaScript.
    /// </summary>
    public sealed partial class JavaScript
    {
        #region BootstrapDialog
        /// <summary>
        /// Javascript natif (utilisation de la méthode alert)
        /// </summary>
        public static void BootstrapDialog(PageBase pPage, string pMessage)
        {
            string script = @"
            var dialog = new BootstrapDialog({
                title: 'Dialog No.',
                message: '" + pMessage + @"',
                buttons: [{
                    label: 'Ok',
                    action: function(dialogRef){dialogRef.close();}
                }]
            });
            dialog.open();";

            //pPage.RegisterScript("BootstrapDialogImmediate", script, true);
            bool isOk = (null != HttpContext.Current) && (1 < HttpContext.Current.Request.Browser.EcmaScriptVersion.Major);
            if (isOk)
                isOk = (!pPage.ClientScript.IsStartupScriptRegistered("BootstrapDialogImmediate"));
            if (isOk)
                ScriptManager.RegisterStartupScript(pPage, pPage.GetType(), "BootstrapDialogImmediate", script, true);

        }
        #endregion BootstrapDialog

        #region public AlertImmediate
        /// <summary>
        /// Javascript natif (utilisation de la méthode alert)
        /// </summary>
        public static void AlertImmediate(ContentPageBase pPage, string pMessage)
        {
            AlertImmediate(pPage, pMessage, false);
        }
        /// <summary>
        /// Javascript natif (utilisation de la méthode alert)
        /// </summary>
        public static void AlertImmediate(ContentPageBase pPage, string pMessage, bool pIsClose)
        {
            string script = GetAlertScript(pMessage, pIsClose);
            pPage.RegisterScript("AlertImmediate", script, false);
        }
        #endregion

        #region ExecuteImmediate
        public static void ExecuteImmediate(ContentPageBase pPage, string pCommand, bool pIsClose)
        {
            ExecuteImmediate(pPage, pCommand, pIsClose, false);
        }
        public static void ExecuteImmediate(ContentPageBase pPage, string pCommand, bool pIsClose, bool pIsStartup)
        {
            string script;
            string nameFunction = "ExecuteImmediate";
            JSStringBuilder sb = new JSStringBuilder();
            //
            if (!pCommand.EndsWith(";"))
                pCommand += ";";
            //
            sb.AppendLine("function " + nameFunction + "()");
            sb.AppendLine("{");
            sb.AppendLine(pCommand);
            if (pIsClose)
                sb.AppendLine("self.close();");
            sb.AppendLine("}");
            sb.AppendLine(nameFunction + "();");
            script = sb.ToString();
            //
            pPage.RegisterScript(nameFunction, script, pIsStartup);
        }
        #endregion
    }
}
