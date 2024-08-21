using EFS.ACommon;
using EFS.ApplicationBlocks.Data;
using EFS.Common;
using EFS.Common.Web;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace EFS.Spheres
{
    /// <summary>
    /// Description résumée de About
    /// </summary>
    public partial class AboutPage : PageBase
    {
        protected string leftTitle, rightTitle;
        protected string displayFpMLInfo, displayFIXInfo;

        protected System.Web.UI.WebControls.Label Label1;
        protected System.Web.UI.WebControls.Label Label2;

        // EG 20190125 DOCTYPE Conformity HTML5
        // EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        // EG 20200902 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc) Corrections et compléments
        protected void Page_Load(object sender, System.EventArgs e)
        {
            Form = this.About;
            try
            {
                SetFocus(imgOk.ClientID);
            }
            catch { }

            imglogosoftware.ImageUrl = $"Images/Logo_Software/Spheres_Banner_v{Software.YearCopyright}.png";


            //HtmlContainerControl ctrl = (HtmlContainerControl)this.FindControl("lblSoftware");
            //if (null != ctrl)
            //    ctrl.InnerText = Software.CopyrightFull;
            lblSoftware.Text = Cst.HTMLSpace + Software.CopyrightFull;
            lblSoftware.ForeColor = System.Drawing.Color.White;
            lblSoftware.Font.Bold = true;
            lblSoftware.Font.Size = FontUnit.Large;
            
            lblLicensee1.Text = Ressource.GetString("RegisteredLicense") + " ";
            lblLicensee2.Text = StrFunc.IsFilled(SessionTools.License.licensee) ? SessionTools.License.licensee:"UNKNOWN!";
            lblLicensee2.Font.Bold = StrFunc.IsFilled(SessionTools.License.licensee);

            bool isUserWithLimitedRights = SessionTools.IsUserWithLimitedRights();
            if (isUserWithLimitedRights && (!SessionTools.IsSessionGuest) && (SessionTools.IsDataArchive))
                isUserWithLimitedRights = false;
            
            bool isDbOracle = DataHelper.IsDbOracle(SessionTools.CS);
            lblDatabase1.Text = isDbOracle ? "Schema: ":"Database: ";
            lblDatabase2.Text = isUserWithLimitedRights ? "N/A" : SessionTools.Data.Server + @"\" + SessionTools.Data.Database;
            lblDatabase2b.Text = isUserWithLimitedRights ? string.Empty : Cst.HTMLSpace + "(" + SessionTools.Data.DatabaseVersionBuild + ")";
            if (!isUserWithLimitedRights)
            {
                if (SessionTools.Data.DatabaseMajorMinor != Software.MajorMinor)
                    lblDatabase2b.ForeColor = System.Drawing.Color.Firebrick;
                else if (SessionTools.Data.DatabaseVersion != Software.Version)
                    lblDatabase2b.ForeColor = System.Drawing.Color.DarkOrange;
            }
            lblDatabase2.Font.Bold = !isUserWithLimitedRights;
            lblDatabase2b.Font.Bold = !isUserWithLimitedRights;
            lblRDBMS1.Text = "RDBMS: ";
            lblRDBMS2.Text = isUserWithLimitedRights ? "N/A" : DataHelper.GetServerVersion(SessionTools.CS);
            lblRDBMS2.Font.Bold = !isUserWithLimitedRights;
            if (!isUserWithLimitedRights)
                lblRDBMS2.ForeColor = isDbOracle ? System.Drawing.Color.Firebrick : System.Drawing.Color.RoyalBlue;
            lblProvider1.Text = "Provider: ";
            SvrInfoConnection svrInfoConnection = DataHelper.GetSvrInfoConnection(SessionTools.CS);
            if (isDbOracle)
            {
                lblProvider2.Text = "ODP.NET " + svrInfoConnection.ProviderVersion;
                if (svrInfoConnection.ProviderVersion.IndexOf("4.122") >= 0)
                    lblProvider2.ForeColor = System.Drawing.Color.Firebrick;
            }
            else
            {
                lblProvider2.Text = ".NET ";
            }
            lblArchive.Visible = false;
            if (SessionTools.IsDataArchive)
            {
                lblArchive.Visible = true;
                lblArchive.ForeColor = System.Drawing.Color.Red;
                lblArchive.Text = Cst.HTMLSpace4 + "[ Archive " + SessionTools.DtArchive.ToString("d") + " ]";
            }
            
            leftTitle = Ressource.GetString("About");
            rightTitle = string.Empty;
            this.PageTitle = leftTitle;
            PageTools.SetHead(this, leftTitle, null, null);

            #region Header
            HtmlPageTitle titleLeft = new HtmlPageTitle(leftTitle);
            HtmlPageTitle titleRight = new HtmlPageTitle(rightTitle);
            Panel pnlHeader = new Panel
            {
                ID = "divHeader"
            };
            pnlHeader.Controls.Add(ControlsTools.GetBannerPage(this, titleLeft, titleRight, IdMenu.GetIdMenu(IdMenu.Menu.About), null));
            plhHeader.Controls.Add(pnlHeader);

            displayFpMLInfo = (Software.IsSoftwareOTCmlOrFnOml() ? "display" : "none");
            displayFIXInfo = (Software.IsSoftwareOTCmlOrFnOml() ? "display" : "none");
            this.DataBind();

            #endregion Header
        }

        #region Code généré par le Concepteur Web Form
        // EG 20200720 [25556] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        // EG 20210212 [25661] New Appel Protection CSRF(Cross-Site Request Forgery)
        override protected void OnInit(EventArgs e)
        {
            InitializeComponent();
            base.OnInit(e);

            Form = About;
            AntiForgeryControl();

            bool isUserWithLimitedRights = SessionTools.IsUserWithLimitedRights();

            divalltoolbar.CssClass = CSSMode + " about";
            divbody.Attributes.Add("class", divalltoolbar.CssClass);
            divabout.Attributes.Add("class", divalltoolbar.CssClass);


            imgOk.Text = @"<i class='fa fa-check-square'></i>" + Ressource.GetString("btnOk");
            imgBrowserInfo.Text = @"<i class='fa fa-external-link-alt'></i>" + Ressource.GetString("butBrowserInfo");
            imgSystemInfo.Visible = !isUserWithLimitedRights;
            imgSystemInfo.Text = @"<i class='fa fa-external-link-alt'></i>" + Ressource.GetString("butSystemInfo");
            imgComponents.Visible = !isUserWithLimitedRights;
            imgComponents.Text = @"<i class='fa fa-external-link-alt'></i>" + Ressource.GetString("butComponents");

            this.AbortRessource = true;

        }
        /// <summary>
        /// Méthode requise pour la prise en charge du concepteur - ne modifiez pas
        /// le contenu de cette méthode avec l'éditeur de code.
        /// </summary>
        private void InitializeComponent()
        {
        }
        #endregion

        // EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        protected void OnOk_Click(object sender, System.EventArgs e)
        {
            //PL 20150722 TEST IN PROGRESS...
            #region
            //string root = @"D:\VSS\Spheres_NV\Code_Source\Spheres";
            ////System.Diagnostics.Debug.WriteLine(("DIR: " + root).PadRight(100,'.'));
            //Insert(root, null);
            //ListFile(root);
            //ListDir(root);

            //root = @"D:\VSS\Spheres_NV\Code_Source\SpheresServices";
            ////System.Diagnostics.Debug.WriteLine(("DIR: " + root).PadRight(100, '.'));
            //Insert(root, null);
            //ListFile(root);
            //ListDir(root);
            #endregion

            ClosePage();
        }
        private bool ListDir(string pData)
        {
            bool ret = false;
            foreach (string d in Directory.GetDirectories(pData))
            {
                ret = true;
                //System.Diagnostics.Debug.WriteLine(("DIR: " + d).PadRight(100, '.'));
                Insert(d, null);
                ListFile(d);
                ListDir(d);
            }
            return ret;
        }
        private bool ListFile(string pData)
        {
            bool ret = false;
            DirectoryInfo dir = new DirectoryInfo(pData);
            foreach (FileInfo f in dir.GetFiles())
            {
                if (!f.Name.StartsWith("ErrorLog"))
                {
                    ret = true;
                    //System.Diagnostics.Debug.WriteLine("FILE: " + f.Name + Cst.Tab + f.Extension + Cst.Tab + f.Length.ToString() + Cst.Tab + f.DirectoryName + Cst.Tab);
                    Insert(pData, f);
                }
            }
            return ret;
        }
        private void Insert(string pDir, FileInfo pFile)
        {
            string sqlInsert = @"insert into dbo.E_EFS_SPHERES_CODE ";
            if (pFile == null)
            {
                sqlInsert += "(DIRNAME) values (";
                sqlInsert += DataHelper.SQLString(pDir);
            }
            else
            {
                switch (pFile.Extension)
                {
                    case ".aspx":
                    case ".config":
                    case ".cs":
                    case ".css":
                    case ".htm":
                    case ".html":
                    case ".js":
                    case ".resx":
                    case ".sql":
                    case ".xml":
                    case ".xsd":
                    case ".xsl":
                    case ".xslt":
                        int TOTALROWS, CODEROWS, COMMENTROWS, ONECHARROWS, EMPTYROWS;
                        TOTALROWS = CODEROWS = COMMENTROWS = ONECHARROWS = EMPTYROWS = 0;
                        string comment1, comment2;
                        comment1 = comment2 = null;
                        switch (pFile.Extension)
                        {
                            case ".aspx":
                                comment1 = "<%--";
                                break;
                            case ".config":
                            case ".htm":
                            case ".html":
                            case ".resx":
                            case ".xml":
                            case ".xsd":
                            case ".xsl":
                            case ".xslt":
                                comment1 = "<!--";
                                break;
                            case ".cs":
                            case ".js":
                                comment1 = "/*";
                                comment2 = "//";
                                break;
                            case ".css":
                                comment1 = "/*";
                                break;
                            case ".sql":
                                comment1 = "/*";
                                comment2 = "--";
                                break;
                        }
                        if (!String.IsNullOrEmpty(comment1))
                            CountRows(pFile, comment1, comment2, out TOTALROWS, out CODEROWS, out COMMENTROWS, out ONECHARROWS, out EMPTYROWS);

                        sqlInsert += "(DIRNAME,FILENAME,FILEEXT,FILESIZE,TOTALROWS,CODEROWS,COMMENTROWS,ONECHARROWS,EMPTYROWS) values (";
                        sqlInsert += DataHelper.SQLString(pDir);
                        sqlInsert += "," + DataHelper.SQLString(pFile.Name);
                        sqlInsert += "," + DataHelper.SQLString(pFile.Extension);
                        sqlInsert += "," + pFile.Length.ToString();
                        sqlInsert += "," + TOTALROWS.ToString() + "," + CODEROWS.ToString() + "," + COMMENTROWS.ToString() + "," + ONECHARROWS.ToString() + "," + EMPTYROWS.ToString();
                        break;
                    default:
                        sqlInsert += "(DIRNAME,FILENAME,FILEEXT,FILESIZE) values (";
                        sqlInsert += DataHelper.SQLString(pDir);
                        sqlInsert += "," + DataHelper.SQLString(pFile.Name);
                        sqlInsert += "," + DataHelper.SQLString(pFile.Extension);
                        sqlInsert += "," + pFile.Length.ToString();
                        break;
                }
            }
            sqlInsert += ")";
            DataHelper.ExecuteNonQuery(SessionTools.CS, CommandType.Text, sqlInsert);
        }
        private void CountRows(FileInfo pFile, string pComment1, string pComment2, out int opTOTALROWS, out int opCODEROWS, out int opCOMMENTROWS, out int opONECHARROWS, out int opEMPTYROWS)
        {
            opTOTALROWS = opCODEROWS = opCOMMENTROWS = opONECHARROWS = opEMPTYROWS = 0;

            string comment3 = pComment1;
            if (pComment1 == "/*")
                comment3 = "*";
            if (String.IsNullOrEmpty(pComment2))
                pComment2 = pComment1;

            string line;
            System.IO.StreamReader file = new System.IO.StreamReader(pFile.FullName);
            while ((line = file.ReadLine()) != null)
            {
                opTOTALROWS++;
                line = line.Trim().Replace(Cst.Tab, string.Empty).Replace(Cst.CrLf, string.Empty);
                if (line.Length == 0)
                    opEMPTYROWS++;
                else if (line.Length == 1)
                    opONECHARROWS++;
                else if (line.StartsWith(pComment1) || line.StartsWith(pComment2) || line.StartsWith(comment3) || line.StartsWith("#"))
                    opCOMMENTROWS++;
                else
                    opCODEROWS++;
            }
            file.Close();
        }

        // EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        // EG 20221012 [XXXXX] Refactoring de l'affichage des caracatéristiques Browser depuis la fenêtre à propos.
        protected void OnBrowserInfo_Click(object sender, System.EventArgs e)
        {
            HttpBrowserCapabilities browser = Request.Browser;
            StringWriter stringWriter = new StringWriter();
            using (HtmlTextWriter writer = new HtmlTextWriter(stringWriter))
            {
                string tmp;

                writer.AddAttribute(HtmlTextWriterAttribute.Class, "about");
                writer.RenderBeginTag(HtmlTextWriterTag.Div);

                writer.RenderBeginTag(HtmlTextWriterTag.P);
                writer.Write("Type = " + browser.Type);
                writer.Write(Cst.HTMLSpace4);
                writer.Write("Name = " + browser.Browser);
                writer.Write(Cst.HTMLSpace4);
                writer.Write("Version = " + browser.Version);
                writer.Write(Cst.HTMLSpace4);
                writer.Write("(v" + browser.MajorVersion + "." + browser.MinorVersion + ")");
                writer.RenderEndTag();
                writer.RenderBeginTag(HtmlTextWriterTag.P);
                writer.Write("Platform = " + browser.Platform);
                writer.RenderBeginTag(HtmlTextWriterTag.Hr);
                writer.RenderEndTag();

                writer.RenderBeginTag(HtmlTextWriterTag.P);
                writer.Write("Is AOL = " + browser.AOL);
                writer.RenderEndTag();
                writer.RenderBeginTag(HtmlTextWriterTag.P);
                writer.Write("Is Beta = " + browser.Beta);
                writer.RenderEndTag();
                writer.RenderBeginTag(HtmlTextWriterTag.P);
                writer.Write("Is Crawler = " + browser.Crawler);
                writer.RenderEndTag();
                writer.RenderBeginTag(HtmlTextWriterTag.P);
                writer.Write("Is Win16 = " + browser.Win16);
                writer.RenderEndTag();
                writer.RenderBeginTag(HtmlTextWriterTag.P);
                writer.Write("Is Win32 = " + browser.Win32);
                writer.RenderBeginTag(HtmlTextWriterTag.Hr);
                writer.RenderEndTag();

                writer.RenderBeginTag(HtmlTextWriterTag.P);
                writer.Write("Supports ActiveX Controls = " + browser.ActiveXControls);
                writer.RenderEndTag();
                writer.RenderBeginTag(HtmlTextWriterTag.P);
                writer.Write("Supports Cookies = " + browser.Cookies);
                writer.RenderEndTag();
                writer.RenderBeginTag(HtmlTextWriterTag.P);
                writer.Write("Supports Frames = " + browser.Frames);
                writer.RenderEndTag();
                writer.RenderBeginTag(HtmlTextWriterTag.P);
                writer.Write("Supports Java Applets = " + browser.JavaApplets);
                writer.RenderEndTag();
                writer.RenderBeginTag(HtmlTextWriterTag.P);
                //writer.Write("Supports JavaScript = " + (browser.EcmaScriptVersion.Major<1?"false":"true"));
                //PL 20101126
                if (browser.EcmaScriptVersion.Major < 1)
                {
                    tmp = "False";
                }
                else
                {
                    tmp = "True" + Cst.HTMLSpace4 + "(EcmaScript v";
                    tmp += browser.EcmaScriptVersion.Major.ToString();
                    tmp += "." + browser.EcmaScriptVersion.Minor.ToString();
                    if (browser.EcmaScriptVersion.Build >= 0)
                    {
                        tmp += "." + browser.EcmaScriptVersion.Build.ToString();
                        if (browser.EcmaScriptVersion.Revision >= 0)
                            tmp += "." + browser.EcmaScriptVersion.Revision.ToString();
                    }
                    tmp += ")";
                }
                writer.Write("Supports JavaScript = " + tmp);
                writer.RenderEndTag();
                writer.RenderBeginTag(HtmlTextWriterTag.P);
                writer.Write("Supports VBScript = " + browser.VBScript);
                writer.RenderEndTag();
                writer.RenderBeginTag(HtmlTextWriterTag.P);
                writer.Write("Supports Tables = " + browser.Tables);
                writer.RenderEndTag();

                writer.RenderEndTag();
            }
            JavaScript.DialogStartUpImmediate(this, "Browser Capabilities", stringWriter.ToString(), false, ProcessStateTools.StatusNoneEnum, JavaScript.DefautHeight, JavaScript.DefautWidth);
        }
        // EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        // EG 20221012 [XXXXX] Refactoring de l'affichage des caractéristiques Système depuis la fenêtre à propos.
        protected void OnSystemInfo_Click(object sender, System.EventArgs e)
        {
            _ = Request.Browser;
            StringWriter stringWriter = new StringWriter();
            using (HtmlTextWriter writer = new HtmlTextWriter(stringWriter))
            {
                writer.AddAttribute(HtmlTextWriterAttribute.Class, "about");
                writer.RenderBeginTag(HtmlTextWriterTag.Div);

                writer.RenderBeginTag(HtmlTextWriterTag.P);
                writer.Write("Software = " + SystemSettings.GetAppSettings("Software"));
                writer.RenderBeginTag(HtmlTextWriterTag.Hr);
                writer.RenderEndTag();
                writer.RenderEndTag();
                writer.RenderBeginTag(HtmlTextWriterTag.P);
                writer.Write("AuthenticateType = " + SystemSettings.GetAppSettings("AuthenticateType"));
                writer.Write(Cst.HTMLSpace4);
                writer.Write("AutoLogin = " + SystemSettings.GetAppSettings("AutoLogin"));
                writer.RenderBeginTag(HtmlTextWriterTag.Hr);
                writer.RenderEndTag();
                //
                writer.RenderBeginTag(HtmlTextWriterTag.P);
                writer.Write("MOMEncrypt = " + SystemSettings.GetAppSettings(Cst.MOM.MOMEncrypt, false.ToString()));
                writer.Write(Cst.HTMLSpace4);
                writer.Write("MOMRecoverable = " + SystemSettings.GetAppSettings(Cst.MOM.MOMRecoverable, true.ToString()));
                string MOMType = SystemSettings.GetAppSettings(Cst.MOM.MOMType);
                writer.RenderBeginTag(HtmlTextWriterTag.P);
                writer.Write("MOMType = " + MOMType);
                writer.Write(Cst.HTMLSpace4);
                writer.Write("MOMPath = " + SystemSettings.GetAppSettings(Cst.MOM.MOMPath).Replace(@"\", @"\\"));
                writer.RenderEndTag();
                //
                foreach (Cst.ServiceEnum s in Enum.GetValues(typeof(Cst.ServiceEnum)))
                {
                    string suffix = EFS.SpheresService.ServiceTools.GetQueueSuffix(s);
                    string MOMPath_X = SystemSettings.GetAppSettings(Cst.MOM.MOMPath + "_" + suffix);
                    if (StrFunc.IsFilled(MOMPath_X))
                    {
                        writer.RenderBeginTag(HtmlTextWriterTag.P);
                        string MOMType_X = SystemSettings.GetAppSettings(Cst.MOM.MOMType + "_" + suffix, MOMType);
                        writer.Write("MOMType_" + suffix + " = " + MOMType_X);
                        writer.Write(Cst.HTMLSpace4);
                        writer.Write("MOMPath_" + suffix + " = " + MOMPath_X.Replace(@"\", @"\\"));
                        writer.RenderEndTag();
                    }
                }
                //
                writer.RenderBeginTag(HtmlTextWriterTag.P);
                writer.RenderBeginTag(HtmlTextWriterTag.Hr);
                writer.Write("EFSmLHelpSchemasVersion = " + SystemSettings.GetAppSettings("EFSmLHelpSchemasVersion"));
                writer.Write(Cst.HTMLSpace4);
                writer.Write("EFSmLHelpSchemasUrl = " + SystemSettings.GetAppSettings("EFSmLHelpSchemasUrl"));
                writer.RenderEndTag();
                writer.RenderBeginTag(HtmlTextWriterTag.P);
                writer.Write("FpMLHelpVersion = " + SystemSettings.GetAppSettings("FpMLHelpVersion"));
                writer.Write(Cst.HTMLSpace4);
                writer.Write("FpMLHelpUrl = " + SystemSettings.GetAppSettings("FpMLHelpUrl"));
                writer.RenderEndTag();
                writer.RenderBeginTag(HtmlTextWriterTag.P);
                writer.Write("FixMLHelpVersion = " + SystemSettings.GetAppSettings("FixMLHelpVersion"));
                writer.Write(Cst.HTMLSpace4);
                writer.Write("FixMLHelpUrl = " + SystemSettings.GetAppSettings("FixMLHelpUrl"));
                writer.RenderBeginTag(HtmlTextWriterTag.Hr);
                writer.RenderEndTag();
                writer.RenderBeginTag(HtmlTextWriterTag.P);
                writer.Write("Spheres_MinDate = " + SystemSettings.GetAppSettings("Spheres_MinDate"));
                writer.Write(Cst.HTMLSpace4);
                writer.Write("Spheres_MaxDate = " + SystemSettings.GetAppSettings("Spheres_MaxDate"));
                writer.RenderEndTag();
                writer.RenderBeginTag(HtmlTextWriterTag.P);
                writer.Write("Spheres_Collation = " + SystemSettings.GetAppSettings("Spheres_Collation"));
                writer.RenderEndTag();
                writer.RenderBeginTag(HtmlTextWriterTag.P);
                writer.Write("Spheres_DataViewMaxRows = " + SystemSettings.GetAppSettings("Spheres_DataViewMaxRows"));
                writer.RenderEndTag();
                writer.RenderBeginTag(HtmlTextWriterTag.P);
                writer.RenderBeginTag(HtmlTextWriterTag.Hr);
                writer.Write("Spheres_TraceRDBMS = " + SystemSettings.GetAppSettings("Spheres_TraceRDBMS"));
                writer.RenderEndTag();
                writer.RenderBeginTag(HtmlTextWriterTag.P);
                writer.Write("Spheres_TraceRDBMS_Username = " + SystemSettings.GetAppSettings("Spheres_TraceRDBMS_Username"));
                writer.RenderEndTag();
                writer.RenderBeginTag(HtmlTextWriterTag.P);
                writer.Write("Spheres_TraceRDBMS_Hostname = " + SystemSettings.GetAppSettings("Spheres_TraceRDBMS_Hostname"));
                writer.RenderEndTag();
                writer.RenderBeginTag(HtmlTextWriterTag.Hr);
                writer.RenderEndTag();
                //writer.RenderBeginTag(HtmlTextWriterTag.P);
                writer.Write("OS = " + Environment.OSVersion);
                //writer.RenderEndTag();
                writer.RenderBeginTag(HtmlTextWriterTag.P);
                writer.Write("CLR = " + Environment.Version);
                writer.RenderBeginTag(HtmlTextWriterTag.Hr);
                writer.RenderEndTag();
                writer.RenderEndTag();

                writer.RenderBeginTag(HtmlTextWriterTag.P);
                writer.Write("ApplicationInstance = " + Context.ApplicationInstance.GetType().FullName);
                writer.RenderEndTag();
                writer.RenderBeginTag(HtmlTextWriterTag.P);
                writer.Write("SessionId = " + SessionTools.SessionID);
                writer.RenderEndTag();
                writer.RenderBeginTag(HtmlTextWriterTag.Hr);
                writer.RenderEndTag();

                writer.RenderBeginTag(HtmlTextWriterTag.P);
                writer.Write("UserHostAdress = " + SessionTools.UserHostAddress);
                writer.RenderEndTag();
                writer.RenderBeginTag(HtmlTextWriterTag.P);
                writer.Write("ClientMachineName = " + SessionTools.ClientMachineName);
                writer.RenderEndTag();

                writer.RenderEndTag();

                writer.RenderEndTag();
            }

            JavaScript.DialogStartUpImmediate(this, "System Environment", stringWriter.ToString(), false, ProcessStateTools.StatusNoneEnum, JavaScript.DefautHeight, JavaScript.DefautWidth);
        }
        // EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        // EG 20221012 [XXXXX] Refactoring de l'affichage des composants depuis la fenêtre à propos.
        protected void OnComponents_Click(object sender, System.EventArgs e)
        {
            _ = Request.Browser;
            StringWriter stringWriter = new StringWriter();
            using (HtmlTextWriter writer = new HtmlTextWriter(stringWriter))
            {
                Dictionary<ComponentTypeEnum, string> assemblies = AssemblyTools.GetDomainAssemblies<Dictionary<ComponentTypeEnum, string>>();
                writer.AddAttribute(HtmlTextWriterAttribute.Class, "about");
                writer.RenderBeginTag(HtmlTextWriterTag.Div);

                foreach (ComponentTypeEnum componentType in Enum.GetValues(typeof(ComponentTypeEnum)))
                {
                    if (assemblies.TryGetValue(componentType, out string componentsList))
                    {
                        WriteComponentList(writer, componentType, componentsList);

                        if (componentType == ComponentTypeEnum.Oracle)
                        {
                            try
                            {
                                using (IDataReader dr = DataHelper.ExecuteReader(SessionTools.CS, CommandType.Text, "select PARAMETER, VALUE from sys.v_$nls_parameters"))
                                {
                                    writer.Write($"<div class='header'>Oracle NLS_PARAMETERS</div>");
                                    writer.RenderBeginTag(HtmlTextWriterTag.Div);
                                    while (dr.Read())
                                    {
                                        writer.WriteLine(dr["PARAMETER"].ToString() + ":    " + dr["VALUE"].ToString());
                                    }
                                    writer.RenderEndTag();
                                }
                            }
                            catch { }
                        }
                    }
                }

                writer.RenderEndTag();
            }

            //JavaScript.DialogStartUpImmediate(this, "Components", stringWriter.ToString(), false, ProcessStateTools.StatusNoneEnum, new Pair<int?, int?>(0, 500), new Pair<int?, int?>(0, 900));
            JavaScript.DialogStartUpImmediate(this, "Components", stringWriter.ToString(), false, ProcessStateTools.StatusNoneEnum, JavaScript.DefautHeight, JavaScript.DefautWidth);
        }

        private void WriteComponentList(HtmlTextWriter pWriter, ComponentTypeEnum pType, string pComponentList)
        {
            pWriter.Write($"<h2 id='about-{pType}' class='header'>{pType}</h2>");
            pWriter.RenderBeginTag(HtmlTextWriterTag.Div);
            pWriter.Write(pComponentList);
            pWriter.RenderEndTag();
            pWriter.WriteBreak();
        }
    }
}