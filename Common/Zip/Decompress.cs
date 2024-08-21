using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Collections.ObjectModel;

using Ionic.Zip;

namespace EFS.Common
{
    /// <summary>
    /// Helper class to extract the contents of a Zip archive
    /// </summary>
    public sealed class Decompress
    {
        #region FieldsAndProperties

        private string m_zipFileName = String.Empty;

        /// <summary>
        /// Get the file name of the target Zip
        /// </summary>
        /// <value>
        /// test.zip
        /// </value>
        public string ZipFileName
        {
            get { return m_zipFileName; }
            private set { m_zipFileName = value; }
        }

        private string m_zipDirectory = String.Empty;

        /// <summary>
        /// Get the directory name of the target Zip
        /// </summary>
        /// <value>
        /// C:\Test;
        /// \\yoursvr\Test;
        /// </value>
        public string ZipDirectory
        {
            get { return m_zipDirectory; }
            private set { m_zipDirectory = value; }
        }

        private string m_zipFullPathName = String.Empty;

        /// <summary>
        /// Get the fullpath name of the curent Zip target
        /// </summary>
        ///  <value>
        /// C:\Test\test.zip;
        /// </value>
        public string ZipFullPathName
        {
            get { return m_zipFullPathName; }
            private set 
            {
                Initialised = ParsePath(value, out string error);
                LastError = error;
                
                m_zipFullPathName = value; 
            }
        }

        private bool m_Overwrite = false;

        /// <summary>
        /// Get or set the permission to overwrite existing datas
        /// </summary>
        /// <value>true, the datas may be overwritten</value>
        public bool Overwrite
        {
            get { return m_Overwrite; }
            set { m_Overwrite = value; }
        }

        private bool m_Initialised = false;

        /// <summary>
        /// Get the current initialization status of the instance
        /// </summary>
        /// <value>true, when we target a verified Zip file</value>
        public bool Initialised
        {
            get { return m_Initialised; }
            private set { m_Initialised = value; }
        }

        private string m_lastError = String.Empty;

        /// <summary>
        /// Get the last initialization error
        /// </summary>
        public string LastError
        {
            get { return m_lastError; }
            private set { m_lastError = value; }
        }

        #endregion FieldsAndProperties

        #region Constructors

        /// <summary>
        /// Build an instance referencing no zip file
        /// </summary>
        /// <remarks>
        /// Use ChangeZipFullPathName to set a valide zip file
        /// </remarks>
        public Decompress() :
            this(String.Empty)
        {
        }

        /// <summary>
        /// Build a  new instance referencing a zip file
        /// </summary>
        /// <param name="pFullpathname">the full path of the archive to decompress </param>
        public Decompress(string pFullpathname) :
            this(pFullpathname, false)
        { 
        }

        /// <summary>
        /// Build a new instance referencing a zip file
        /// </summary>
        /// <param name="pFullpathname">the full path of the archive we want to decompress</param>
        /// <param name="pOverwrite">set the permission to overwrite existing datas during extraction</param>
        public Decompress(string pFullpathname, bool pOverwrite)
        {
            ZipFullPathName = pFullpathname;
            Overwrite = pOverwrite;
        }

        #endregion Constructors

        #region Interface

        /// <summary>
        /// Extract the current zip file contents in the target directory
        /// </summary>
        /// <param name="pTargetDirectory">the target directory</param>
        /// <returns>true if the extraction is successfull otherwise false.
        /// The extraction can not succeed when the current instance is not correctly initialised or the target directory does not exist</returns>
        public bool Extract(string pTargetDirectory)
        {
            bool extracted = false;

            if (Initialised)
            {
                if (Directory.Exists(pTargetDirectory))
                {

                    using (ZipFile archive = ZipFile.Read(ZipFullPathName))
                    {

                        ExtractExistingFileAction existingFileAction = ExtractExistingFileAction.DoNotOverwrite;

                        if (Overwrite)
                            existingFileAction = ExtractExistingFileAction.OverwriteSilently;

                        foreach (ZipEntry innerFile in archive)
                        {
                            innerFile.Extract(pTargetDirectory, existingFileAction);
                        }
                    }

                    extracted = true;
                    LastError = String.Empty;
                }
                else
                    LastError = "Target directory does not exist";
            }

            return extracted;
        }

        /// <summary>
        /// Change the current zip file target  
        /// </summary>
        /// <param name="pFullpathname"></param>
        /// <param name="opError"></param>
        /// <returns></returns>
        public bool ChangeZipFullPathName(string pFullpathname, out string opError)
        {
            ZipFullPathName = pFullpathname;

            bool res = Initialised;
            opError = LastError;

            return res;
        }

        #endregion Interface

        #region Methods

        bool ParsePath(string pFullPathName, out string opError)
        {
            bool res = false;
            opError = string.Empty;

            if (pFullPathName != null && pFullPathName != String.Empty)
            {
                try
                {

                    ZipFileName = Path.GetFileName(pFullPathName);
                    ZipDirectory = Path.GetDirectoryName(pFullPathName);

                    res = File.Exists(pFullPathName);

                    if (res)
                    {
                        res = false;
                        ReadOnlyCollection<string> errors = null;

                        try
                        {
                            res = ZipFile.CheckZip(pFullPathName);
                        }
                        catch (ZipException ex)
                        {
                            opError = ex.Message;
                        }

                        if (errors != null && errors.Count > 0)
                            foreach (string error in errors)
                                String.Concat(opError, String.Format("{0}; ", error));
                    }
                    else
                        opError = "File does not exist";
                }
                catch (ArgumentException ex) { opError = ex.Message; }
                catch (PathTooLongException ex) { opError = ex.Message; }
                catch (NotSupportedException ex) { opError = ex.Message; }
            }
            else
                LastError = "Null arguments";

            return res;
        }

        #endregion Methods
    }
}
