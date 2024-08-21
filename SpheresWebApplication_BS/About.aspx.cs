using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;

using EFS.ACommon;
using EFS.Common;
using EFS.Common.Web;
using EFS.ApplicationBlocks.Data;
using System.Data;

namespace EFS.Spheres
{
    public partial class About : Page
    {
        protected string leftTitle, rightTitle;
        protected string displayFpMLInfo, displayFIXInfo;

        protected string ok = "<span class='glyphicon glyphicon-ok text-success'></span>";
        protected string remove = "<span class='glyphicon glyphicon-remove text-danger'></span>";
        protected string rowDefault = "<tr><td class='col-sm-5 '>{0}</td><td class='col-sm-7'>{1}</td></tr>";
        protected string rowBoolean = "<tr><td class='col-sm-7'>{0}</td><td class='text-center col-sm-5'>{1}</td></tr>";


        protected void Page_Load(object sender, EventArgs e)
        {
            lblSoftware.InnerText = Software.CopyrightFull;
            lblLicensee.Text = Ressource.GetString("RegisteredLicense") + " ";
            lblRdbms.Visible = SessionTools.IsConnected;
            lblRdbmsResult.Visible = SessionTools.IsConnected;
            lblDatabase.Visible = SessionTools.IsConnected;
            lblDatabaseResult.Visible = SessionTools.IsConnected;
            if (SessionTools.IsConnected)
            {
                lblLicenseeResult.Text = StrFunc.IsFilled(SessionTools.License.licensee) ? SessionTools.License.licensee : "UNKNOWN!";
                lblRdbmsResult.Text = DataHelper.GetServerVersion(SessionTools.CS);
                lblDatabase.Text = DataHelper.isDbOracle(SessionTools.CS) ? "Schema" : "Database";
                lblDatabaseResult.Text += SessionTools.Data.Server + @"\" + SessionTools.Data.Database + " (" + SessionTools.Data.DatabaseVersionBuild + ")";
            }
            else
            {
                lblLicenseeResult.Text = "NOT CONNECTED";
                lblLicenseeResult.CssClass = "label label-danger";
            }
            this.DataBind();

            DisplayBrowserInfo();
            DisplaySystemInfo();
            DisplayComponentInfo();
        }


        protected void DisplayBrowserInfo()
        {
            HttpBrowserCapabilities browser = Request.Browser;
            // Header : Brower and Platform
            lblBrowserType.InnerHtml = String.Format("{0} v{1}.{2} <small>{3}</small>", browser.Browser, browser.MajorVersion, browser.MinorVersion, browser.Platform); ;

            PlaceHolder plh = new PlaceHolder();
            plh.Controls.Add(new LiteralControl("<table class='about'>"));

            plh.Controls.Add(new LiteralControl(String.Format(rowBoolean, "AOL", browser.AOL ? ok : remove)));
            plh.Controls.Add(new LiteralControl(String.Format(rowBoolean, "Beta version", browser.Beta ? ok : remove)));
            plh.Controls.Add(new LiteralControl(String.Format(rowBoolean, "Crawler", browser.Crawler ? ok : remove)));
            plh.Controls.Add(new LiteralControl(String.Format(rowBoolean, "Win16", browser.Win16 ? ok : remove)));
            plh.Controls.Add(new LiteralControl(String.Format(rowBoolean, "Win32", browser.Win32 ? ok : remove)));

            plh.Controls.Add(new LiteralControl("<tr class='info'><th colspan='2'>Supports</th></tr>"));
            plh.Controls.Add(new LiteralControl(String.Format(rowBoolean, "ActiveX Controls", browser.ActiveXControls ? ok : remove)));
            plh.Controls.Add(new LiteralControl(String.Format(rowBoolean, "Cookies", browser.Cookies ? ok : remove)));
            plh.Controls.Add(new LiteralControl(String.Format(rowBoolean, "Frames", browser.Frames ? ok : remove)));
            plh.Controls.Add(new LiteralControl(String.Format(rowBoolean, "Java Applets", browser.JavaApplets ? ok : remove)));
            plh.Controls.Add(new LiteralControl(String.Format(rowBoolean, "Win32", browser.Win32 ? ok : remove)));

            bool isSupportJS = (1 <= browser.EcmaScriptVersion.Major);
            string supportJS = string.Empty;
            if (isSupportJS)
            {
                supportJS = " (EcmaScript v" + browser.EcmaScriptVersion.Major.ToString() + "." + browser.EcmaScriptVersion.Minor.ToString();
                if (0 <= browser.EcmaScriptVersion.Build)
                {
                    supportJS += "." + browser.EcmaScriptVersion.Build.ToString();
                    if (0 <= browser.EcmaScriptVersion.Revision)
                        supportJS += "." + browser.EcmaScriptVersion.Revision.ToString();
                }
                supportJS += ")";
            }
            plh.Controls.Add(new LiteralControl(String.Format(rowBoolean, "Win32" + supportJS, browser.Win32 ? ok : remove)));

            plh.Controls.Add(new LiteralControl(String.Format(rowBoolean, "VBScript", browser.VBScript ? ok : remove)));
            plh.Controls.Add(new LiteralControl(String.Format(rowBoolean, "Tables", browser.Tables ? ok : remove)));

            plh.Controls.Add(new LiteralControl("</table>"));

            browserContent.Controls.Add(plh);

        }
        protected void DisplaySystemInfo()
        {
            System.Web.HttpBrowserCapabilities browser = Request.Browser;

            PlaceHolder plh = new PlaceHolder();
            plh.Controls.Add(new LiteralControl("<table id='tblWeb' class='about table-fixed'>"));
            HtmlGenericControl tHead = new HtmlGenericControl("thead");
            tHead.Controls.Add(new LiteralControl("<tr><th class='col-sm-5'>Key</th><th class='col-sm-7'>Value</th></tr>"));
            plh.Controls.Add(tHead);
            HtmlGenericControl tBody = new HtmlGenericControl("tbody");
            plh.Controls.Add(tBody);

            tBody.Controls.Add(new LiteralControl(String.Format(rowDefault, "Software", SystemSettings.GetAppSettings("Software"))));
            tBody.Controls.Add(new LiteralControl(String.Format(rowDefault, "AuthenticateType", SystemSettings.GetAppSettings("AuthenticateType"))));
            tBody.Controls.Add(new LiteralControl(String.Format(rowDefault, "AutoLogin", (bool)SystemSettings.GetAppSettings("AutoLogin", typeof(Boolean), false) ? ok : remove)));
            tBody.Controls.Add(new LiteralControl(String.Format(rowDefault, "MOMEncrypt", (bool)SystemSettings.GetAppSettings("MOMEncrypt", typeof(Boolean), false) ? ok : remove)));
            tBody.Controls.Add(new LiteralControl(String.Format(rowDefault, "MOMRecoverable", (bool)SystemSettings.GetAppSettings("MOMRecoverable", typeof(Boolean), true) ? ok : remove)));
            tBody.Controls.Add(new LiteralControl(String.Format(rowDefault, "MOMType", SystemSettings.GetAppSettings("MOMType"))));
            tBody.Controls.Add(new LiteralControl(String.Format(rowDefault, "MOMPath", SystemSettings.GetAppSettings(Cst.MOM.MOMPath).Replace(@"\", @"\\"))));

            foreach (Cst.ServiceEnum s in Enum.GetValues(typeof(Cst.ServiceEnum)))
            {
                string suffix = EFS.SpheresService.ServiceTools.GetQueueSuffix(s);
                string MOMPath_X = SystemSettings.GetAppSettings(Cst.MOM.MOMPath + "_" + suffix);
                if (StrFunc.IsFilled(MOMPath_X))
                {
                    string MOMType_X = SystemSettings.GetAppSettings(Cst.MOM.MOMType + "_" + suffix, SystemSettings.GetAppSettings("MOMType"));
                    plh.Controls.Add(new LiteralControl(String.Format(rowDefault, "MOMType_" + suffix, MOMType_X)));
                    tBody.Controls.Add(new LiteralControl(String.Format(rowDefault, "MOMPath_" + suffix, MOMPath_X.Replace(@"\", @"\\"))));
                }
            }

            tBody.Controls.Add(new LiteralControl(String.Format(rowDefault, "EFSmLHelpSchemasVersion", SystemSettings.GetAppSettings("EFSmLHelpSchemasVersion"))));
            tBody.Controls.Add(new LiteralControl(String.Format(rowDefault, "EFSmLHelpSchemasUrl", SystemSettings.GetAppSettings("EFSmLHelpSchemasUrl"))));
            tBody.Controls.Add(new LiteralControl(String.Format(rowDefault, "FpMLHelpVersion", SystemSettings.GetAppSettings("FpMLHelpVersion"))));
            tBody.Controls.Add(new LiteralControl(String.Format(rowDefault, "FpMLHelpUrl", SystemSettings.GetAppSettings("FpMLHelpUrl"))));
            tBody.Controls.Add(new LiteralControl(String.Format(rowDefault, "FixMLHelpVersion", SystemSettings.GetAppSettings("FixMLHelpVersion"))));
            tBody.Controls.Add(new LiteralControl(String.Format(rowDefault, "FixMLHelpUrl", SystemSettings.GetAppSettings("FixMLHelpUrl"))));
            tBody.Controls.Add(new LiteralControl(String.Format(rowDefault, "Spheres_MinDate", SystemSettings.GetAppSettings("Spheres_MinDate"))));
            tBody.Controls.Add(new LiteralControl(String.Format(rowDefault, "Spheres_MaxDate", SystemSettings.GetAppSettings("Spheres_MaxDate"))));
            tBody.Controls.Add(new LiteralControl(String.Format(rowDefault, "Spheres_Collation", SystemSettings.GetAppSettings("Spheres_Collation"))));
            tBody.Controls.Add(new LiteralControl(String.Format(rowDefault, "Spheres_DataViewMaxRows", SystemSettings.GetAppSettings("Spheres_DataViewMaxRows"))));
            tBody.Controls.Add(new LiteralControl(String.Format(rowDefault, "Spheres_TraceRDBMS", SystemSettings.GetAppSettings("Spheres_TraceRDBMS"))));
            tBody.Controls.Add(new LiteralControl(String.Format(rowDefault, "Spheres_TraceRDBMS_Username", SystemSettings.GetAppSettings("Spheres_TraceRDBMS_Username"))));
            tBody.Controls.Add(new LiteralControl(String.Format(rowDefault, "Spheres_TraceRDBMS_Hostname", SystemSettings.GetAppSettings("Spheres_TraceRDBMS_Hostname"))));

            plh.Controls.Add(new LiteralControl("</table>"));
            webappsettingsContent.Controls.Add(plh);


            plh = new PlaceHolder();
            plh.Controls.Add(new LiteralControl("<table id='tblSystem' class='about table-fixed'>"));
            tHead = new HtmlGenericControl("thead");
            tHead.Controls.Add(new LiteralControl("<tr><th class='col-sm-5'>Key</th><th class='col-sm-7'>Value</th></tr>"));
            plh.Controls.Add(tHead);
            tBody = new HtmlGenericControl("tbody");
            plh.Controls.Add(tBody);
            tBody.Controls.Add(new LiteralControl(String.Format(rowDefault, "OS", Environment.OSVersion)));
            tBody.Controls.Add(new LiteralControl(String.Format(rowDefault, "CLR", Environment.Version)));
            tBody.Controls.Add(new LiteralControl(String.Format(rowDefault, "ApplicationInstance", Context.ApplicationInstance.GetType().FullName)));
            tBody.Controls.Add(new LiteralControl(String.Format(rowDefault, "SessionId", SessionTools.SessionID)));
            string userHostAdress = SessionTools.UserHostAddress;
            string clientMachineName = SessionTools.ClientMachineName;
            tBody.Controls.Add(new LiteralControl(String.Format(rowDefault, "UserHostAdress", StrFunc.IsFilled(userHostAdress) ? userHostAdress : Cst.HTMLSpace)));
            tBody.Controls.Add(new LiteralControl(String.Format(rowDefault, "ClientMachineName", StrFunc.IsFilled(clientMachineName) ? clientMachineName : Cst.HTMLSpace)));

            plh.Controls.Add(new LiteralControl("</table>"));
            systemsContent.Controls.Add(plh);
        }
        protected void DisplayComponentInfo()
        {
            List<Pair<ComponentTypeEnum, Assembly>> assemblies = GetAssemblies();
            string rowHeaderTitle = "<tr class='info'><th colspan='2'>{0} <span class='badge badge-primary pull-right'>{1}</span></th></tr>";
            string rowBody = "<tr><td class='col-sm-10'>{0}{1}</td><td class='col-sm-2'>{2}</td></tr>";

            string popoverClickTemplate = @"<span class=""pull-right text-info glyphicon glyphicon-option-horizontal"" data-toggle=""popover"" title=""{0}"" tabindex=""0"" data-content=""{1}""></span>";

            string popoverContentTemplate =
            @"<table class='table table-hover table-condensed small'>
                    <tr><td>Location</td><td>{0}</td></tr>
                    <tr><td>PublicTokenKey</td><td>{1}</td></tr>
                    <tr><td>Company</td><td>{2}</td></tr>
                    <tr><td>Product</td><td>{3}</td></tr>
                    <tr><td>Copyright</td><td>{4}</td></tr>
                </table>";

            if ((null != assemblies) && (0 < assemblies.Count()))
            {
                PlaceHolder plh = new PlaceHolder();
                plh.Controls.Add(new LiteralControl("<table id='tblComponent' class='about table-fixed'>"));
                HtmlGenericControl tHead = new HtmlGenericControl("thead");
                tHead.Controls.Add(new LiteralControl("<tr><th class='col-sm-10'>Assembly</th><th class='col-sm-2'>Version</th></tr>"));
                plh.Controls.Add(tHead);
                HtmlGenericControl tBody = new HtmlGenericControl("tbody");
                plh.Controls.Add(tBody);

                // System
                FieldInfo[] flds = new ComponentTypeEnum().GetType().GetFields();
                foreach (FieldInfo fld in flds)
                {
                    if (fld.FieldType.IsEnum)
                    {
                        ComponentTypeEnum componentType = (ComponentTypeEnum)Enum.Parse(typeof(ComponentTypeEnum), fld.Name, false);
                        if (assemblies.Exists(item => item.First == componentType && (false == item.Second.IsDynamic)))
                        {
                            List<Pair<ComponentTypeEnum, Assembly>> lstAssemblies =
                                assemblies.FindAll(item => item.First == componentType).OrderBy(orderby => orderby.Second.GetName().Name).ToList();
                            tBody.Controls.Add(new LiteralControl(String.Format(rowHeaderTitle, componentType.ToString(), lstAssemblies.Count().ToString())));
                            lstAssemblies.ForEach(item =>
                            {
                                if (false == item.Second.IsDynamic)
                                {
                                    AssemblyInfo assemblyInfo = new AssemblyInfo(item.Second);

                                    string popoverContent = String.Format(popoverContentTemplate, assemblyInfo.AssemblyPath,
                                        assemblyInfo.PublicTokenKey, assemblyInfo.Company, assemblyInfo.Product, assemblyInfo.Copyright);
                                    string popoverClick = String.Format(popoverClickTemplate, assemblyInfo.ProductTitle, popoverContent);

                                    tBody.Controls.Add(new LiteralControl(String.Format(rowBody, assemblyInfo.ProductTitle, popoverClick, assemblyInfo.Version)));
                                }
                            }); ;
                        }
                    }
                }
                plh.Controls.Add(new LiteralControl("</table>"));
                componentsContent.Controls.Add(plh);
            }
        }

        private string GetAttributeValue<TAttr>(Assembly pAssembly, Func<TAttr, string> resolveFunc, string defaultResult = null) where TAttr : Attribute
        {
            object[] attributes = pAssembly.GetCustomAttributes(typeof(TAttr), false);
            if (attributes.Length > 0)
                return resolveFunc((TAttr)attributes[0]);
            else
                return defaultResult;
        }

        private List<Pair<ComponentTypeEnum, Assembly>> GetAssemblies()
        {
            Assembly[] assemblies = Thread.GetDomain().GetAssemblies();
            List<Pair<ComponentTypeEnum, Assembly>> lstAssembly = new List<Pair<ComponentTypeEnum, Assembly>>();
            string fn = string.Empty;
            foreach (Assembly assembly in assemblies)
            {
                Pair<ComponentTypeEnum, Assembly> pair = new Pair<ComponentTypeEnum, Assembly>();
                pair.Second = assembly;

                try
                {
                    fn = assembly.FullName;
                    if (fn.StartsWith("mscorlib") || fn.StartsWith("System") || fn.StartsWith("vjs") || fn.StartsWith("AjaxControlToolkit"))
                        pair.First = ComponentTypeEnum.System;
                    else if (fn.StartsWith("Oracle"))
                        pair.First = ComponentTypeEnum.Oracle;
                    else if (fn.StartsWith("EFS") || fn.StartsWith("EfsML"))
                        pair.First = ComponentTypeEnum.EFS;
                    else if (fn.StartsWith("FlyDocLibrary") || fn.StartsWith("FONet") || fn.StartsWith("Ionic") || fn.StartsWith("itextsharp") ||
                             fn.StartsWith("WebPageSecurity") || fn.StartsWith("XmlDiffPatch"))
                        pair.First = ComponentTypeEnum.AddEFS;
                    else if (assembly.CodeBase.IndexOf("Temporary ASP.NET Files") > 0)
                        pair.First = ComponentTypeEnum.Temporary;
                    else
                        pair.First = ComponentTypeEnum.Other;
                }
                catch { pair.First = ComponentTypeEnum.Misc; }
                lstAssembly.Add(pair);
            }
            return lstAssembly;
        }


        private class AssemblyInfo
        {
            public AssemblyInfo(Assembly assembly)
            {
                if (assembly == null)
                    throw new ArgumentNullException("assembly");
                this.assembly = assembly;
            }

            private readonly Assembly assembly;

            /// <summary>
            /// Retourne le Titre de l'assembly
            /// </summary>
            public string ProductTitle
            {
                get
                {
                    return GetAttributeValue<AssemblyTitleAttribute>(a => a.Title, Path.GetFileNameWithoutExtension(assembly.CodeBase));
                }
            }

            /// <summary>
            /// Retourne le chemin de l'assembly
            /// </summary>
            public string AssemblyPath
            {
                get
                {
                    return Path.GetDirectoryName(assembly.CodeBase.Replace("file:///", "").Replace("/", "\\"));
                }
            }

            /// <summary>
            /// Retourne la PublicTokenKey de l'assembly
            /// </summary>
            public string PublicTokenKey
            {
                get
                {
                    return GetPublicTokenKeyFromAssembly();
                }
            }

            /// <summary>
            /// Retourne la version
            /// </summary>
            public string Version
            {
                get
                {
                    string result = "1.0.0.0";
                    Version version = assembly.GetName().Version;
                    if (version != null)
                        result = version.ToString();
                    return result;
                }
            }

            /// <summary>
            /// Retourne la Description.
            /// </summary>
            public string Description
            {
                get { return GetAttributeValue<AssemblyDescriptionAttribute>(a => a.Description); }
            }


            /// <summary>
            /// Retourne le Product.
            /// </summary>
            public string Product
            {
                get { return GetAttributeValue<AssemblyProductAttribute>(a => a.Product); }
            }

            /// <summary>
            /// Retourne le Copyright.
            /// </summary>
            public string Copyright
            {
                get { return GetAttributeValue<AssemblyCopyrightAttribute>(a => a.Copyright); }
            }

            /// <summary>
            /// Retourne la compagnie.
            /// </summary>
            public string Company
            {
                get { return GetAttributeValue<AssemblyCompanyAttribute>(a => a.Company); }
            }

            /// <summary>
            /// Méthode principale d'extraction d'un attribut donné
            /// </summary>
            /// <typeparam name="TAttr"></typeparam>
            /// <param name="resolveFunc"></param>
            /// <param name="defaultResult"></param>
            /// <returns></returns>
            protected string GetAttributeValue<TAttr>(Func<TAttr, string> resolveFunc, string defaultResult = null) where TAttr : Attribute
            {
                object[] attributes = assembly.GetCustomAttributes(typeof(TAttr), false);
                if (attributes.Length > 0)
                    return resolveFunc((TAttr)attributes[0]);
                else
                    return defaultResult;
            }

            protected string GetPublicTokenKeyFromAssembly()
            {
                string publicTokenKey = string.Empty;
                var bytes = assembly.GetName().GetPublicKeyToken();
                if ((null != bytes) && (0 <= bytes.Length))
                {
                    for (int i = 0; i < bytes.GetLength(0); i++)
                        publicTokenKey += string.Format("{0:x2}", bytes[i]);
                }
                else
                {
                    publicTokenKey = "None";
                }
                return publicTokenKey;
            }
        }
    }
}