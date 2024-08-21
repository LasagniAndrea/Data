using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Optimization;
using System.Web.UI;
using AspNet.ScriptManager.jQuery;

namespace EFS.Spheres
{
    public class BundlesFormats
    {
        public const string PRINT = @"<link href='{0}' rel='stylesheet' media='print' />";
        public const string SCREEN = @"<link href='{0}' rel='stylesheet' media='screen' />";
        public const string ALL = @"<link href='{0}' rel='stylesheet' media='all' type='text/css' />";
    }
    public class BundleConfig
    {
        private static void AddMsAjaxMapping(string name, string loadSuccessExpression)
        {
            ScriptManager.ScriptResourceMapping.AddDefinition(name, new ScriptResourceDefinition
            {
                Path = "~/Scripts/WebForms/MsAjax/" + name,
                CdnPath = "http://ajax.aspnetcdn.com/ajax/4.5/6/" + name,
                LoadSuccessExpression = loadSuccessExpression,
                CdnSupportsSecureConnection = true
            });
        }


        // Pour plus d'informations sur le regroupement (Bundling), visitez le site http://go.microsoft.com/fwlink/?LinkID=303951
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                "~/Scripts/jquery-{version}.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryui").Include(
                "~/Scripts/jquery-ui-{version}.js"));

            bundles.Add(new ScriptBundle("~/bundles/BSDatePickerJS").Include(
                "~/Scripts/moment-with-locales.js",
                "~/Scripts/bootstrap-datetimepicker.js"));

            bundles.Add(new ScriptBundle("~/bundles/BSSelectJS").Include(
                "~/Scripts/bootstrap-select.js"));

            bundles.Add(new ScriptBundle("~/bundles/BSTypeahead").Include(
                "~/Scripts/bootstrap3-typeahead.js"));


            bundles.Add(new ScriptBundle("~/bundles/WebFormsJs").Include(
                "~/Scripts/WebForms/WebForms.js",
                "~/Scripts/WebForms/WebUIValidation.js",
                "~/Scripts/WebForms/MenuStandards.js",
                "~/Scripts/WebForms/Focus.js",
                "~/Scripts/WebForms/GridView.js",
                "~/Scripts/WebForms/DetailsView.js",
                "~/Scripts/WebForms/TreeView.js",
                "~/Scripts/WebForms/WebParts.js"));

            // L'ordre est très important pour que ces fichiers fonctionnent, ils ont des dépendances explicites
            bundles.Add(new ScriptBundle("~/bundles/MsAjaxJs").Include(
                "~/Scripts/WebForms/MsAjax/MicrosoftAjax.js",
                "~/Scripts/WebForms/MsAjax/MicrosoftAjaxApplicationServices.js",
                "~/Scripts/WebForms/MsAjax/MicrosoftAjaxTimer.js",
                "~/Scripts/WebForms/MsAjax/MicrosoftAjaxWebForms.js"));

            // Utilisez la version de développement de Modernizr pour développer et apprendre. Puis, une fois
            // prêt pour la production, utilisez l'outil de build sur http://modernizr.com pour sélectionner uniquement les tests dont vous avez besoin
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                "~/Scripts/modernizr-*"));

            ScriptManager.ScriptResourceMapping.AddDefinition("MsAjaxBundle", new ScriptResourceDefinition
            {
                Path = "~/bundles/MsAjaxJs",
                CdnPath = "http://ajax.aspnetcdn.com/ajax/4.5/6/MsAjaxBundle.js",
                LoadSuccessExpression = "window.Sys",
                CdnSupportsSecureConnection = true
            });

            AddMsAjaxMapping("MicrosoftAjax.js", "window.Sys && Sys._Application && Sys.Observer"); 
            AddMsAjaxMapping("MicrosoftAjaxCore.js", "window.Type && Sys.Observer"); 
            AddMsAjaxMapping("MicrosoftAjaxGlobalization.js", "window.Sys && Sys.CultureInfo"); 
            AddMsAjaxMapping("MicrosoftAjaxSerialization.js", "window.Sys && Sys.Serialization"); 
            AddMsAjaxMapping("MicrosoftAjaxComponentModel.js", "window.Sys && Sys.CommandEventArgs"); 
            AddMsAjaxMapping("MicrosoftAjaxNetwork.js", "window.Sys && Sys.Net && Sys.Net.WebRequestExecutor"); 
            AddMsAjaxMapping("MicrosoftAjaxHistory.js", "window.Sys && Sys.HistoryEventArgs");
            AddMsAjaxMapping("MicrosoftAjaxWebServices.js", "window.Sys && Sys.Net && Sys.Net.WebServiceProxy");
            AddMsAjaxMapping("MicrosoftAjaxTimer.js", "window.Sys && Sys.UI && Sys.UI._Timer");
            AddMsAjaxMapping("MicrosoftAjaxWebForms.js", "window.Sys && Sys.WebForms");
            AddMsAjaxMapping("MicrosoftAjaxApplicationServices.js", "window.Sys && Sys.Services");

            ScriptManager.ScriptResourceMapping.AddDefinition("WebFormsBundle", new ScriptResourceDefinition 
            { 
                Path = "~/bundles/WebFormsJs", 
                CdnPath = "http://ajax.aspnetcdn.com/ajax/4.5/6/WebFormsBundle.js", 
                LoadSuccessExpression = "window.WebForm_PostBackOptions", 
                CdnSupportsSecureConnection = true 
            });

            ScriptManager.ScriptResourceMapping.AddDefinition("respond", new ScriptResourceDefinition
            {
                Path = "~/Scripts/respond.min.js",
                DebugPath = "~/Scripts/respond.js",
            });

            ScriptManager.ScriptResourceMapping.AddDefinition("spheres", new ScriptResourceDefinition
            {
                Path = "~/Scripts/spheres.js",
                DebugPath = "~/Scripts/spheres.js",
            });

            ScriptManager.ScriptResourceMapping.AddDefinition("bootstrap-dialog", new ScriptResourceDefinition
            {
                Path = "~/Scripts/bootstrap-dialog.js",
                DebugPath = "~/Scripts/bootstrap-dialog.js",
            });

            ScriptManager.ScriptResourceMapping.AddDefinition("bootstrap-select", new ScriptResourceDefinition
            {
                Path = "~/Scripts/bootstrap-select.min.js",
                DebugPath = "~/Scripts/bootstrap-select.js",
            });

            ScriptManager.ScriptResourceMapping.AddDefinition("bootstrap3-typeahead", new ScriptResourceDefinition
            {
                Path = "~/Scripts/bootstrap3-typeahead.min.js",
                DebugPath = "~/Scripts/bootstrap3-typeahead.js",
            });

            ScriptManager.ScriptResourceMapping.AddDefinition("bootstrap-datetimepicker", new ScriptResourceDefinition
            {
                Path = "~/Scripts/bootstrap-datetimepicker.min.js",
                DebugPath = "~/Scripts/bootstrap-datetimepicker.js",
            });

            ScriptManager.ScriptResourceMapping.AddDefinition("tracker", new ScriptResourceDefinition
            {
                Path = "~/Scripts/tracker.js",
                DebugPath = "~/Scripts/tracker.js",
            });


            Bundle lessBundle = new Bundle("~/Content/Bootstrap/Less").IncludeDirectory("~/Content/Bootstrap", "Bootstrap.less");
            lessBundle.Transforms.Add(new LessTransform());
            lessBundle.Transforms.Add(new CssMinify());
            bundles.Add(lessBundle);

            lessBundle = new Bundle("~/Content/Less").IncludeDirectory("~/Content/Less", "Spheres.less");
            lessBundle.Transforms.Add(new LessTransform());
            lessBundle.Transforms.Add(new CssMinify());
            bundles.Add(lessBundle);

            lessBundle = new Bundle("~/Content/DateTimePicker/Less").IncludeDirectory("~/Content/DateTimePicker", "bootstrap-datetimepicker-build.less");
            lessBundle.Transforms.Add(new LessTransform());
            lessBundle.Transforms.Add(new CssMinify());
            bundles.Add(lessBundle);

            lessBundle = new Bundle("~/Content/bootstrap-select/Less").IncludeDirectory("~/Content/bootstrap-select", "bootstrap-select.less");
            lessBundle.Transforms.Add(new LessTransform());
            lessBundle.Transforms.Add(new CssMinify());
            bundles.Add(lessBundle);
        }
    }
}