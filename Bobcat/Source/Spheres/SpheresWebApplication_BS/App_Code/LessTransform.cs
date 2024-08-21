using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Hosting;
using System.Web.Optimization;
using dotless.Core;
using dotless.Core.Input;
using dotless.Core.configuration;
using dotless.Core.Loggers;

using EFS.ACommon;

namespace EFS.Spheres
{
    internal sealed class VirtualFileReader : IFileReader
    {
        public byte[] GetBinaryFileContents(string fileName)
        {
            fileName = GetFullPathFileExists(fileName);
            return File.ReadAllBytes(fileName);
        }

        public string GetFileContents(string fileName)
        {
            fileName = GetFullPathFileExists(fileName);
            return File.ReadAllText(fileName);
        }

        public bool DoesFileExist(string pFileName)
        {
            return StrFunc.IsFilled(GetFullPathFileExists(pFileName));
        }

        private static string GetFullPathFileExists(string pFileName)
        {
            string fileName = GetFullPath(pFileName);
            if (false == File.Exists(fileName))
            {
                fileName = GetFullPath("Less/" + pFileName);
                if (false == File.Exists(fileName))
                {
                    fileName = GetFullPath("Bootstrap/" + pFileName);
                    if (false == File.Exists(fileName))
                    {
                        fileName = GetFullPath("Bootstrap/Mixins/" + pFileName);
                        if (false == File.Exists(fileName))
                            fileName = string.Empty;
                    }
                }
            }
            return fileName;
        }

        private static string GetFullPath(string path)
        {
            return HostingEnvironment.MapPath("~/Content/" + path);
        }

        bool IFileReader.UseCacheDependencies
        {
            get { throw new NotImplementedException(); }
        }
    }


    /// <summary>
    /// Description résumée de LessTransform
    /// </summary>
    public class LessTransform : IBundleTransform
    {
        public void Process(BundleContext context, BundleResponse response)
        {
            //response.Content = dotless.Core.Less.Parse(response.Content);
            //response.ContentType = "text/css";

            DotlessConfiguration config = new DotlessConfiguration();
            config.MinifyOutput = false;
            config.ImportAllFilesAsLess = false;
            config.CacheEnabled = false;
            config.LessSource = typeof(VirtualFileReader);
            #if DEBUG
                config.Logger = typeof(DiagnosticsLogger);
            #endif
            response.Content = Less.Parse(response.Content, config);         
            response.ContentType = "text/css";
        }
    }

}