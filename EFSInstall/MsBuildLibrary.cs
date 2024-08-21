using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Build.Framework ; 
using Microsoft.Build.Utilities ;
using System.IO; 

namespace EFSInstall.MsBuildLibrary
{
    // EG 20180423 Analyse du code Correction [CA1405]
    [ComVisible(false)]
    public class ImportFolder : Task
    {
        #region Members
        private string _from;
        private string _to;
        #endregion Members

        #region accessor
        [Required]
        public string From
        {
            get
            {
                return _from;
            }
            set
            {
                _from = value;
            }
        }

        [Required]
        public string To
        {
            get
            {
                return _to;
            }
            set
            {
                _to = value;
            }
        }

        #endregion

        #region public override Execute
        // EG 20180423 Analyse du code Correction [CA2200]
        public override bool Execute()
        {
            try
            {
                FileCopy(_from, _to, true);
                return true;
            }
            catch (Exception)
            {
                throw;
            }
        }
        #endregion

        #region private static static
        private  void FileCopy(string srcdir, string destdir, bool recursive)
        {
            DirectoryInfo dir;
            FileInfo[] files;
            DirectoryInfo[] dirs;
            string tmppath;

            //determine if the destination directory exists, if not create it
            if (!Directory.Exists(destdir))
                Directory.CreateDirectory(destdir);

            dir = new DirectoryInfo(srcdir);

            //if the source dir doesn't exist, throw
            if (!dir.Exists)
                throw new ArgumentException("source dir doesn't exist -> " + srcdir);

            //get all files in the current dir
            files = dir.GetFiles();

            //loop through each file
            foreach (FileInfo file in files)
            {
                //create the path to where this file should be in destdir
                tmppath = Path.Combine(destdir, file.Name);

                //copy file to dest dir
                file.CopyTo(tmppath, false);
            }


            //if not recursive, all work is done
            if (!recursive)
            {
                return;
            }

            //otherwise, get dirs
            dirs = dir.GetDirectories();

            //loop through each sub directory in the current dir
            foreach (DirectoryInfo subdir in dirs)
            {
                //create the path to the directory in destdir
                tmppath = Path.Combine(destdir, subdir.Name);

                //recursively call this function over and over again
                //with each new dir.
                FileCopy(subdir.FullName, tmppath, recursive);
            }
        }
        #endregion

    }

}
