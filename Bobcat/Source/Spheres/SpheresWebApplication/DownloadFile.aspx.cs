#region Using Directives
//
using EFS.ACommon;
using EFS.Common;
using EFS.Common.Web;
using EFS.LoggerClient;
using EFS.LoggerClient.LoggerService;
using System;
using System.IO;
using System.Web;
//
#endregion Using Directives

namespace EFS.Spheres.Download
{
    public partial class DownloadFilePage : PageBase
    {
        #region protected Page_Load
        protected void Page_Load(object sender, System.EventArgs e)
        {
            string path = Request.QueryString["P"].ToString(); //Exemple de valeur "~/RN"
            string file = Request.QueryString["F"].ToString(); //Exemple de valeur "v5.x\English\Release notes - Spheres© v5.0 EN.pdf"

            // FI 20211215 [25904] add ArgumentNullException
            if (StrFunc.IsEmpty(path))
                throw new ArgumentNullException("parameter P(Path) is null or Empty in QueryString");
            if (StrFunc.IsEmpty(file))
                throw new ArgumentNullException("parameter F(File) is null or Empty in QueryString");

            string physicalPath = DwnldTools.GetPath(this, path);

            // FI 20211215 [25904] Appel à Path.GetFullPath pour interprétation des éventuels \.. ou \. 
            string fullFilePath = Path.GetFullPath(physicalPath + file);

            // FI 20211215 [25904] Appel à CheckFullFilePath
            CheckFullFilePath(fullFilePath);

            // FI 20211215 [25904] Appel à CheckExtension
            CheckExtension(fullFilePath);

            DownloadFile(fullFilePath);
        }
        #endregion


        #region private DownloadLog
        private void DownloadLog(string psFileName, string psDirName, string psError, bool pbIsOk)
        {
            try
            {
                if (false == Software.IsSoftwarePortal())
                    return;

                #region Décomposition du message d'erreur

                string sError1, sError2, sError3;

                if (psError.Length > 64)
                {
                    sError1 = psError.Substring(0, 64);
                    if (psError.Length - 64 > 64)
                    {
                        sError2 = psError.Substring(64, 64);
                        if (psError.Length - 128 > 64)
                        {
                            sError3 = psError.Substring(128, 64);
                        }
                        else
                        {
                            sError3 = psError.Substring(128, psError.Length - 128);
                        }
                    }
                    else
                    {
                        sError2 = psError.Substring(64, psError.Length - 64);
                        sError3 = "";
                    }
                }
                else
                {
                    sError1 = psError;
                    sError2 = "";
                    sError3 = "";
                }
                #endregion


                LogLevelEnum logLevel = (pbIsOk ? LogLevelEnum.Info : LogLevelEnum.Error);
                SessionTools.LogAddInfo(logLevel, "Download", new string[] { psFileName, psDirName, sError1, sError2, sError3 });
            }
            catch (Exception exp)
            {
                Console.WriteLine("The process failed: {0}", exp.ToString());
            }
        }
        #endregion

        #region private DownloadFile
        /// <summary>
        /// 
        /// </summary>
        /// <param name="psFullPath"></param>
        // FI 20211215 [25904] Refactoring
        /// EG 20220215 [26251][WI582] Upd : Vulnerability on unrestricted file upload : Enhancement
        private void DownloadFile(string psFullPath)
        {
            string sDirName = Path.GetDirectoryName(psFullPath);
            string sFileName = Path.GetFileName(psFullPath);
            try
            {
                if (Directory.Exists(sDirName))
                {
                    Response.BufferOutput = true;
                    Response.Clear();
                    Response.AppendHeader("content-disposition", "attachment; filename=" + sFileName);
                    Response.ContentType = MimeMapping.GetMimeMapping(sFileName);

                    try
                    {
                        // Open the file.
                        using (FileStream iStream = new FileStream(psFullPath, FileMode.Open, FileAccess.Read, FileShare.Read))
                        {
                            // Buffer to read 10K bytes in chunk:
                            byte[] buffer = new Byte[10000];

                            // Total bytes to read:
                            long dataToRead = iStream.Length;

                            // Read the bytes.
                            while (dataToRead > 0)
                            {
                                // Verify that the client is connected.
                                if (Response.IsClientConnected)
                                {
                                    // Read the data in buffer.
                                    int length = iStream.Read(buffer, 0, 10000);

                                    // Write the data to the current output stream.
                                    Response.OutputStream.Write(buffer, 0, length);

                                    // Flush the data to the HTML output.
                                    Response.Flush();

                                    buffer = new Byte[10000];
                                    dataToRead -= length;
                                }
                                else
                                {
                                    //prevent infinite loop if user disconnects
                                    dataToRead = -1;
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        // Trap the error, if any.
                        Response.Write("Error : " + ex.Message);
                        DownloadLog(sFileName, sDirName, ex.Message, false);
                    }

                    DownloadLog(sFileName, sDirName, "", true);
                }
            }
            catch (Exception exp)
            {
                Console.WriteLine("The process failed: {0}", exp.ToString());
                DownloadLog(sFileName, sDirName, exp.ToString(), false);
            }
            finally
            {
                Response.End();
            }
        }
        #endregion

        /// <summary>
        /// Contrôle que le fichier se trouve dans un dossier enfant de l'application Web (Ex : Temporary ou RN)
        /// </summary>
        /// <param name="fullFilePath">fichier avec chemin complet</param>
        /// <exception cref="Exception"></exception>
        /// FI 20211215 [25904] Add Method
        private void CheckFullFilePath(string fullFilePath)
        {
            //Seuls les dossiers enfants du root sont autorisés
            string rootPath = MapPath("~"); //Server.MapPath("~") returns the physical path to the root of the application
            if ((!fullFilePath.ToUpper().StartsWith(rootPath.ToUpper())) || (fullFilePath.ToUpper() == rootPath.ToUpper()))
                throw new Exception($"unauthorized location for {fullFilePath}");
        }

        /// <summary>
        /// Contrôle que l'extension du fichier est acceptée. Les extensions acceptées peuvent être définies via le fichier de config
        /// </summary>
        /// <param name="fullFilePath">fichier avec chemin complet</param>
        /// <exception cref="Exception"></exception>
        /// FI 20211215 [25904] Add Method
        private void CheckExtension(string fullFilePath)
        {
            string[] allowedExtension = new string[] { ".log", ".trc", ".txt", ".csv", ".html", ".xml", ".pdf", ".doc", ".docx", ".xsl", ".xslx", ".zip" };
            string allowedExtensionSetting = (string)SystemSettings.GetAppSettings("DownloadAllowedExtensions", typeof(string), string.Empty);
            if (StrFunc.IsFilled(allowedExtensionSetting))
                allowedExtension = StrFunc.StringArrayList.StringListToStringArray(allowedExtensionSetting);

            string fileExtension = Path.GetExtension(fullFilePath);
            if (false == ArrFunc.ExistInArray(allowedExtension, fileExtension.ToLower()))
                throw new Exception($"extension:{fileExtension} not allowed");
        }
    }
}
