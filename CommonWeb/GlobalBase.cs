using System;
using System.IO;
using System.Net;
using System.Web;
using System.Web.UI;

namespace Wilson.WebCompile
{
    public class GlobalBase : System.Web.HttpApplication
    {
        private static bool needsCompile = true;
        private static string applicationPath = "";
        private static string physicalPath = "";
        private static string applicationURL = "";
		
        private static System.Threading.Thread thread = null;

        // Override and Indicate Files to Skip with Semi-Colon Delimiter
        protected virtual string SkipFiles 
        {
            get { return @"ASPPage1.aspx;EFSBanniere.ascx"; }
        }

        // Override and Indicate Folders to Skip with Semi-Colon Delimiter
        protected virtual string SkipFolders 
        {
            get { return @"_vti_cnf"; }
        }
		
        public override void Init() 
        {
            if (GlobalBase.needsCompile) 
            {
                GlobalBase.needsCompile = false;

                applicationPath = HttpContext.Current.Request.ApplicationPath;
                if (!applicationPath.EndsWith("/")) { applicationPath += "/";	}

                string server = HttpContext.Current.Request.ServerVariables["SERVER_NAME"];
                bool https = HttpContext.Current.Request.ServerVariables["HTTPS"] != "off";
                applicationURL = (https ? "https://" : "http://") + server + applicationPath;

                physicalPath = HttpContext.Current.Request.PhysicalApplicationPath;
                thread = new System.Threading.Thread(new System.Threading.ThreadStart(CompileApp));
                thread.Start();
            }
        }

        private void CompileApp() 
        {
            CompileFolder(physicalPath);
        }

        private void CompileFolder(string Folder) 
        {
            foreach (string file in Directory.GetFiles(Folder, "*.as?x")) 
            {
                CompileFile(file);
            }

            foreach (string folder in Directory.GetDirectories(Folder)) 
            {
                bool skipFolder = false;
                foreach (string item in this.SkipFolders.Split(';')) 
                {
                    if (item != "" && folder.ToUpper().EndsWith(item.ToUpper())) 
                    {
                        skipFolder = true;
                        break;
                    }
                }
                if (!skipFolder) 
                {
                    CompileFolder(folder);
                }
            }
        }

        private void CompileFile(string File) 
        {
            bool skipFile = false;
            foreach (string item in this.SkipFiles.Split(';')) 
            {
                if (item != "" && File.ToUpper().EndsWith(item.ToUpper())) 
                {
                    skipFile = true;
                    break;
                }
            }

            if (!skipFile) 
            {
                string path = File.Remove(0, physicalPath.Length);
                if (File.ToLower().EndsWith(".ascx")) 
                {
                    string virtualPath = applicationPath + path.Replace(@"\", "/");
                    Page controlLoader = new Page();
                    try 
                    {
                        controlLoader.LoadControl(virtualPath);
                    }
                    catch (Exception e)
                    {
                        System.Diagnostics.Debug.WriteLine(e.Message);
                    }
                    finally
                    {
                        //System.Diagnostics.Debug.WriteLine(virtualPath, "Control");
                    }
                }
                else if (!File.ToLower().EndsWith(".asax")) 
                {
                    string url = applicationURL + path.Replace(@"\", "/");
                    WebRequest httpWebRequest;
                    try 
                    {
                        httpWebRequest = HttpWebRequest.Create(url);
                        WebResponse webResponse = httpWebRequest.GetResponse();
                        webResponse.Close();
                        webResponse =null;
                    }
                    catch (Exception e)
                    {
                        System.Diagnostics.Debug.WriteLine(e.Message);
                    }
                    System.Diagnostics.Debug.WriteLine(url, "Page");
                }
            }
        }
    }
}
