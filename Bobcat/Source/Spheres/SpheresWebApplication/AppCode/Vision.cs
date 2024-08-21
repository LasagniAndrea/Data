using EFS.ACommon;
using EFS.Common;
using EFS.Common.Web;
using System;

namespace EFS.Spheres
{
    /// <summary>
    /// Description résumée de List.
    /// </summary>
    public class Vision
	{
		public Vision()
		{}
        //20081016 PL/FL Add pIdLstConsult
        public Cst.ErrLevel PrintPDF(PageBase pPage, string pTableName, int pIdA, string pIdLstTemplate, string pIdLstConsult) 
		{
            string executable = string.Empty;
            string msgOutput;
            System.Diagnostics.Process myProcess = new System.Diagnostics.Process();
            Cst.ErrLevel ret;
            try
            {
                string[] paramPB = new string[10];
                //RDBMS
                paramPB[0] = SessionTools.Data.RDBMS;
                // Administrateur LOGIN 
                paramPB[1] = string.Empty + SystemSettings.GetAppSettings_Software("_UserName");
                // Administrateur PASSWORD
                paramPB[2] = string.Empty + SystemSettings.GetAppSettings_Software("_Pwd");
                bool isPwdCrypted = (SystemSettings.GetAppSettings(Software.Name + "_" + "IsPwdCrypted") == "1");
                if (isPwdCrypted)
                    paramPB[2] = Cryptography.Decrypt(paramPB[2]);
                //paramPB[1] = "BIM_ERO6_C";
                //paramPB[2] = "efs98*";
                //DATABASE
                paramPB[3] = SessionTools.Data.Database;
                //SERVER
                paramPB[4] = SessionTools.Data.Server;
                //IDENTIFIER
                paramPB[5] = SessionTools.Collaborator_IDENTIFIER;
                //IDA
                paramPB[6] = pIdA.ToString();
                //IDLSTTEMPLATE
                paramPB[7] = pIdLstTemplate;
                //IDLSTCONSULT
                paramPB[8] = pIdLstConsult;
                //Chemin et nom du fichier PDF 
                string fileName = paramPB[6] + paramPB[7].Replace('*', '_') + paramPB[8].Replace('*', '_') + pTableName + ".PDF";
                paramPB[9] = SessionTools.TemporaryDirectory.PathMapped + fileName;
                FileTools.FileRenameToDotBak(paramPB[9]);//20090130 PL Add FileRenameToDotBak
                                                         //Recupération du chemin de l'exécutable PB ( efs_SendWeb ) sauvegarder dans Web.config
                string sWorkingDirectory = SystemSettings.GetAppSettings_Software("_PDFGenerator");
                if (StrFunc.IsEmpty(sWorkingDirectory))
                    sWorkingDirectory = SystemSettings.GetAppSettings_Software("_PDFGeneratorPath");
                executable = SystemSettings.GetAppSettings_Software("_PDFGeneratorExe");
                if (StrFunc.IsEmpty(executable))
                    executable = @"pdfgenerator.exe";
                //Shell Executable Powerbuilder avec passage des arguments
                myProcess.StartInfo.WorkingDirectory = sWorkingDirectory;
                if ((StrFunc.IsFilled(sWorkingDirectory)) && !sWorkingDirectory.EndsWith(@"\"))
                    myProcess.StartInfo.FileName = sWorkingDirectory + @"\" + executable;
                else
                    myProcess.StartInfo.FileName = sWorkingDirectory + executable;
                myProcess.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Maximized;
                myProcess.StartInfo.CreateNoWindow = false;
                myProcess.StartInfo.Arguments = paramPB[0] + ';' + paramPB[1] + ';' + paramPB[2] + ';' + paramPB[3] + ';' + paramPB[4] + ';' + paramPB[5] + ';' + paramPB[7] + ';' + paramPB[8] + ';' + paramPB[9] + ';' + sWorkingDirectory;
                //
                string sTimeoutSInSecond = SystemSettings.GetAppSettings_Software("_PDFGeneratorTimeout");
                int timeoutSInSecond = 300;//Default 5mn (300 sec.)
                if (StrFunc.IsFilled(sTimeoutSInSecond))
                    timeoutSInSecond = Convert.ToInt32(sTimeoutSInSecond);
                //if (timeoutSInSecond < 20)
                //    timeoutSInSecond = 300;
                int waiting = 1000 * timeoutSInSecond;
                bool isAborted = false;
                myProcess.Start();
                myProcess.WaitForExit(waiting);
                if (!myProcess.HasExited)
                {
                    isAborted = true;
                    myProcess.Kill();
                }
                if (isAborted)
                {
                    ret = Cst.ErrLevel.TIMEOUT;
                    msgOutput = "ERROR: Failed to execute PDFGenerator component ! \r\n\r\n";
                    msgOutput += @"Process aborted after " + (waiting / 60000).ToString() + " minutes !";
                    if (SessionTools.IsSessionSysAdmin && (myProcess.StartInfo != null) && (myProcess.StartInfo.FileName != null))
                    {
                        msgOutput += "\r\n\r\nDirectory: " + myProcess.StartInfo.WorkingDirectory;
                        msgOutput += "\r\nComponent: " + executable;
                    }
                    //JavaScript.AlertStartUpImmediate((PageBase)pPage, msgOutput, false);
                    JavaScript.DialogStartUpImmediate((PageBase)pPage, msgOutput, false);

                }
                else
                {
                    //Open ACROBAT PDF File
                    //20091029 FI [download File] add try cath
                    ret = Cst.ErrLevel.SUCCESS;
                    try
                    {
                        AspTools.OpenBinaryFile(pPage, paramPB[9], Cst.TypeMIME.Application.Pdf, true);
                    }
                    catch { ret = Cst.ErrLevel.ABORTED; }
                }
                myProcess.Close();
            }
            catch (Exception ex)
            {
                ret = Cst.ErrLevel.FILENOTFOUND;
                msgOutput = "WARNING: Failed to load PDFGenerator component ! \r\n\r\n";
                msgOutput += ex.Message;
                if (SessionTools.IsSessionSysAdmin && (myProcess.StartInfo != null) && (myProcess.StartInfo.FileName != null))
                {
                    msgOutput += "\r\n\r\nDirectory: " + myProcess.StartInfo.WorkingDirectory;
                    msgOutput += "\r\nComponent: " + executable;
                    //
                    if (StrFunc.IsEmpty(myProcess.StartInfo.WorkingDirectory))
                        msgOutput += "\r\n\r\nPlease verify that your web.config file is correct and that you have a key (PDFGenerator) with the path for PDFGenerator component.";
                    else
                        msgOutput += "\r\n\r\nPlease verify that your web.config file is correct and that you have the correct path for PDFGenerator component.";
                }
                //JavaScript.AlertStartUpImmediate((PageBase)pPage, msgOutput, false);
                JavaScript.DialogStartUpImmediate((PageBase)pPage, msgOutput, false);
            }
            return ret;
		}
	}
}
