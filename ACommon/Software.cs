using System;
using System.Text.RegularExpressions;
using System.Configuration;
using System.Reflection;
using System.Linq;
//pl
namespace EFS.ACommon
{
    /// <summary>
    /// To insert here a summary
    /// </summary>
    public sealed class Software
    {
        #region Enum
        public enum ReleaseTypeEnum
        {
            NA,
            PreAlpha,
            Alpha,
            Beta,
            RC,
            RTM,
            //Gold
        }
        /* FI 20190822 [24861] Mise en commentaire (voir class AssemblyTools)
        public enum ComponentTypeEnum
        {
            System,
            Oracle,
            EFS,
            AddEFS,
            Other,
            Misc,
            Temporary,
        }
         */
        #endregion Enum
        #region Members
        public const string MEMUROOT_OTCml = "OTC"; //PL 20120201 A renommer prochainement
        private const string MEMUROOT_Vision = "VISION";
        private const string MEMUROOT_Portal = "EFS";
        //private const string MEMUROOT_Portal = "PORTAL";

        //PL 20120201 
        /// <summary>
        /// Spheres
        /// </summary>
        public const string SOFTWARE_Spheres = "Spheres";
        /// <summary>
        /// OTCml
        /// </summary>
        public const string SOFTWARE_OTCml = "OTCml";//PL 20120201 A supprimer prochainement...
        //public const string SOFTWARE_FnOml = "F&Oml";
        public const string SOFTWARE_Vision = "Vision";
        public const string SOFTWARE_Portal = "EFS";
        //public const string SOFTWARE_Portal = "PORTAL";

        // PM 20141216 [9700] Eurex Prisma for Eurosys Futures
        // Ajouté comme constante de la class Software, mais non géré dans cette class
        public const string SOFTWARE_EurosysFutures = "EurosysFutures";

        private static bool IsLoaded;// = false;

        private static string softwareName;
        private static int major;
        private static int minor;
        private static int revision;
        private static int build;
        private static ReleaseTypeEnum releaseType = ReleaseTypeEnum.NA;
        private static int releaseTypeNumber; // = 0;
        // EG 20160404 Migration vs2013
        private static readonly int servicePackNumber = 0;
        // EG 20160404 Migration vs2013
        private static readonly string servicePackLetter = string.Empty;
        private static string comment;
        private static string menuRoot;
        // FI 20210722 [XXXXX] add
        private static string _yearCopyright;

        #endregion Members
        #region Constructor
        public Software() : this(ConfigurationManager.AppSettings["Software"]) { }

        public Software(string pSoftware)
        {
            IsLoaded = true;
            switch (pSoftware)
            {
                case SOFTWARE_Vision:
                    #region Spheres Vision
                    softwareName = SOFTWARE_Vision;
                    releaseType = ReleaseTypeEnum.NA;
                    releaseTypeNumber = 1;
                    //servicePackNumber = 0;
                    //servicePackLetter = "";
                    comment = string.Empty;
                    menuRoot = MEMUROOT_Vision;
                    break;
                    #endregion
                case SOFTWARE_Portal:
                    #region Portal EFS
                    softwareName = SOFTWARE_Portal;
                    releaseType = ReleaseTypeEnum.NA;
                    releaseTypeNumber = 1;
                    //servicePackNumber = 0;
                    //servicePackLetter = "";
                    comment = string.Empty;
                    menuRoot = MEMUROOT_Portal;
                    break;
                    #endregion
                //case SOFTWARE_OTCml:
                //case SOFTWARE_FnOml:
                case SOFTWARE_Spheres:
                default:
                    //WARNING, default nécessaire actuellment pour les services OTCml 
                    #region Spheres OTCml/F&Oml
                    softwareName = (StrFunc.IsFilled(pSoftware) ? pSoftware : SOFTWARE_Spheres); //20100510 PL/CC/FI a finaliser... 
                    releaseType = ReleaseTypeEnum.NA;
                    releaseTypeNumber = 1;
                    //servicePackNumber = 0;
                    //servicePackLetter = "";
                    comment = string.Empty;
                    menuRoot = MEMUROOT_OTCml;
                    break;
                    #endregion
            }
            //20070724 PL
            Version version = Assembly.GetExecutingAssembly().GetName().Version;
            major = version.Major;
            minor = version.Minor;
            //Warning: Build Assembly = Revision OTCml et Revision Assembly = Build OTCml
            //         OTCml: Major.Minor.Revision.Build
            revision = version.Build;
            build = version.Revision;
        }
        #endregion Constructor
        #region General String Constant
        public static string Name
        {
            get
            {
                if (false == IsLoaded)
                {
                    new Software();
                }
                return softwareName;
            }
        }
        public static string LongName
        {
            get
            {
                string ret = Name;

                if (Name.StartsWith("Eurosys"))
                    ret = string.Empty;
                else if (IsSoftwareVision())
                    ret = "Spheres " + ret;

                return ret;
            }
        }
        public static string Major
        {
            get { return major.ToString(); }
        }
        public static string Minor
        {
            get { return minor.ToString(); }
        }
        public static string Revision
        {
            get { return revision.ToString(); }
        }
        /// <summary>
        /// Major Minor
        /// <para>(ie 3.1)</para>
        /// </summary>
        public static string MajorMinor
        {
            get { return major.ToString() + "." + minor.ToString(); }
        }
        /// <summary>
        /// Major Minor SPx {releaseType}
        /// </summary>
        public static string MajorMinorType
        {
            get { return MajorMinor + SPAndReleaseType; }
        }
        /// <summary>
        /// Major Minor Revision
        /// <para>(ie 3.6.5025)</para>
        /// </summary>
        public static string Version
        {
            get { return MajorMinor + "." + revision.ToString(); }
        }
        /// <summary>
        /// Major Minor Revision Build
        /// <para>(ie 3.6.5028.3354)</para>
        /// </summary>
        public static string VersionBuild
        {
            get { return Version + "." + build.ToString(); }
        }
        /// <summary>
        /// 
        /// </summary>
        public static string Build
        {
            get { return build.ToString(); }
        }
        public static string NameVersionBuild
        {
            get { return LongName + "© v" + VersionBuild; }
        }

        public static string VersionType
        {
            //PL 20121003
            //get { return Version + SPAndReleaseType; }
            get { return MajorMinorType + " (" + Version + ")"; }
        }
        public static string VersionBuildType
        {
            //PL 20121003
            //get { return Version + "." + build.ToString() + SPAndReleaseType + Comment; }//glop
            get { return MajorMinorType + Comment + " (" + VersionBuild + ")"; }
        }
        public static string NameMajorMinorType
        {
            get { return LongName + " v" + MajorMinorType; }
        }
        public static string NameVersionBuildType
        {
            get { return LongName + "© v" + VersionBuildType; }
        }
        /// <summary>
        /// 
        /// </summary>
        /// FI 20210722 [XXXXX] Refactoring (Lecture de AssemblyCopyright)
        public static string YearCopyright
        {
            //get { return "2021"; }
            get
            {
                if (StrFunc.IsEmpty(_yearCopyright))
                {
                    object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyCopyrightAttribute), false);
                    if (ArrFunc.IsFilled(attributes))
                    {
                        AssemblyCopyrightAttribute copyrightAttribute = attributes.OfType<AssemblyCopyrightAttribute>().Single();
                        Regex re = new Regex(@"\d{4}");
                        if (re.IsMatch(copyrightAttribute.Copyright))
                            _yearCopyright = re.Match(copyrightAttribute.Copyright).Value;
                    }
                }
                return _yearCopyright;
            }
        }
        // EG 20230513 [WI922] Spheres Portal : EFS|BDX GUI Adaptation
        public static string Title
        {
            get
            {
                if (IsSoftwarePortal())
                    return "EFS|BDX: Provider of Capital Markets Solutions © " + YearCopyright;
                else
                    return CopyrightWithoutYear;
            }
        }
        // EG 20230513 [WI922] Spheres Portal : EFS|BDX GUI Adaptation
        public static string TitleForDefaultAspx
        {
            get
            {
                if (IsSoftwarePortal())
                    return "EFS|BDX: Provider of Capital Markets Solutions © " + YearCopyright;
                else
                    return CopyrightWithYear;
            }
        }
        // EG 20230513 [WI922] Spheres Portal : EFS|BDX GUI Adaptation
        public static string CopyrightWithYear
        {
            get
            {
                string ret = string.Empty;
                if (!IsSoftwarePortal())
                    ret = LongName;
                return ret + " v" + MajorMinorType + Comment + " - © " + YearCopyright + " EFS|BDX.";
            }
        }
        // EG 20230513 [WI922] Spheres Portal : EFS|BDX GUI Adaptation
        public static string CopyrightWithoutYear
        {
            get
            {
                string ret = string.Empty;
                if (!IsSoftwarePortal())
                    ret = LongName;
                return ret + " v" + MajorMinorType + " © EFS|BDX.";
            }
        }
        // EG 20230513 [WI922] Spheres Portal : EFS|BDX GUI Adaptation
        public static string CopyrightFull
        {
            get
            {
                string ret = string.Empty;
                if (!IsSoftwarePortal())
                    ret = LongName;
                return ret + " v" + VersionBuildType + " - © " + YearCopyright + " EFS|BDX.";
            }
        }

        // EG 20230513 [WI922] Spheres Portal : EFS|BDX GUI Adaptation
        public static string CopyrightSmall
        {
            get
            {
                string ret = string.Empty;
                if (!IsSoftwarePortal())
                    ret = LongName + " ";   //PL 20180926 Add space
                ret += MajorMinorType;      //PL 20121003
                ret += " © " + YearCopyright + " EFS|BDX.";
                //ret += SPAndReleaseType;
                return ret;
            }
        }
        public static string Comment
        {
            get
            {
                if (StrFunc.IsFilled(comment))
                    return " " + comment;
                else
                    return string.Empty;
            }
        }
        public static string SPAndReleaseType
        {
            get
            {
                string rt = string.Empty;
                string sp = string.Empty;

                if (servicePackNumber > 0)
                    sp = "SP" + servicePackNumber.ToString() + servicePackLetter;

                switch (releaseType)
                {
                    case ReleaseTypeEnum.NA:
                        break;
                    case ReleaseTypeEnum.RTM:
                        if (servicePackNumber == 0)
                            rt = releaseType.ToString();
                        break;
                    default:
                        rt = releaseType.ToString();
                        if (releaseTypeNumber > 1)
                            rt += releaseTypeNumber.ToString();
                        break;
                }
                if (rt.Length > 0)
                    rt = " " + rt;
                if (sp.Length > 0)
                    sp = " " + sp;
                return rt + sp;
            }
        }
        public static bool IsSoftwareSpheres()
        {
            return (softwareName == SOFTWARE_Spheres);
        }
        public static bool IsSoftwareOTCmlOrFnOml()
        {
            return (IsSoftwareOTCml() || IsSoftwareFnOml());
        }
        public static bool IsSoftwareOTCml()
        {
            //return (softwareName == SOFTWARE_OTCml);
            return (softwareName == SOFTWARE_Spheres);
        }
        public static bool IsSoftwareFnOml()
        {
            //return (softwareName == SOFTWARE_FnOml);
            return (softwareName == SOFTWARE_Spheres);
        }
        public static bool IsSoftwareVision()
        {
            return (softwareName == SOFTWARE_Vision);
        }
        public static bool IsSoftwarePortal()
        {
            return (softwareName == SOFTWARE_Portal);
        }
        public static string MenuRoot()
        {
            return menuRoot;
        }

        public static string AddPrefixSoft(string pData)
        {
            return softwareName + "_" + pData;
        }

        /* FI 20190822 [24861] Mise en commenataire (voir class AssemblyTools)
        
        public static Dictionary<ComponentTypeEnum, string> GetAssemblies()
        {
            Dictionary<ComponentTypeEnum, string> ret = new Dictionary<ComponentTypeEnum, string>();

            #region GetAssemblies()
            System.Reflection.Assembly[] myAssemblies = System.Threading.Thread.GetDomain().GetAssemblies();
            string system = string.Empty;   //mscorlib, System, vjs, AjaxControlToolkit 
            string oracle = string.Empty;   //Oracle
            string efs = string.Empty;      //EFS, EfsML
            string addefs = string.Empty;   //FlyDocLibrary, apachefop.net, Ionic.Zip, itextsharp, WebPageSecurity, XmlDiffPatch
            string temporary = string.Empty;//Temporary ASP.NET Files
            string other = string.Empty;
            string misc = string.Empty;

            string fn = string.Empty;
            string cd = string.Empty;

            foreach (System.Reflection.Assembly a in myAssemblies)
            {
                try
                {
                    fn = string.Empty;
                    cd = string.Empty;
                    fn = a.FullName;
                    cd = a.CodeBase;

                    if (fn.StartsWith("mscorlib") || fn.StartsWith("System") || fn.StartsWith("vjs") || fn.StartsWith("AjaxControlToolkit"))
                    {
                        system += new string(' ', 0 * 2) + string.Format("Assembly Identity={0}", fn) + "\r\n";
                        system += new string(' ', 1 * 2) + string.Format("...Codebase={0}", cd) + "\r\n";
                    }
                    else if (fn.StartsWith("Oracle"))
                    {
                        oracle += new string(' ', 0 * 2) + string.Format("Assembly.Identity={0}", fn) + "\r\n";
                        oracle += new string(' ', 1 * 2) + string.Format("...Codebase={0}", cd) + "\r\n";
                    }
                    else if (fn.StartsWith("EFS") || fn.StartsWith("EfsML"))
                    {
                        efs += new string(' ', 0 * 2) + string.Format("Assembly.Identity={0}", fn) + "\r\n";
                        efs += new string(' ', 1 * 2) + string.Format("...Codebase={0}", cd) + "\r\n";
                    }
                    else if (fn.StartsWith("FlyDocLibrary") || fn.StartsWith("apachefop") || fn.StartsWith("Ionic") || fn.StartsWith("itextsharp")
                        || fn.StartsWith("WebPageSecurity") || fn.StartsWith("XmlDiffPatch"))
                    {
                        addefs += new string(' ', 0 * 2) + string.Format("Assembly Identity={0}", fn) + "\r\n";
                        addefs += new string(' ', 1 * 2) + string.Format("...Codebase={0}", cd) + "\r\n";
                    }
                    else if (cd.IndexOf("Temporary ASP.NET Files") > 0)
                    {
                        temporary += new string(' ', 0 * 2) + string.Format("Assembly Identity={0}", fn) + "\r\n";
                        temporary += new string(' ', 1 * 2) + string.Format("...Codebase={0}", cd) + "\r\n";
                    }
                    else
                    {
                        other += new string(' ', 0 * 2) + string.Format("Assembly Identity={0}", fn) + "\r\n";
                        other += new string(' ', 1 * 2) + string.Format("...Codebase={0}", cd) + "\r\n";
                    }
                }
                catch
                {
                    misc += new string(' ', 0 * 2) + string.Format("Assembly Identity={0}", fn) + "\r\n";
                    if (!String.IsNullOrEmpty(cd))
                    {
                        misc += new string(' ', 1 * 2) + string.Format("...Codebase={0}", cd) + "\r\n";
                    }
                }
            }
            #endregion

            if (!String.IsNullOrEmpty(system))
            {
                ret.Add(ComponentTypeEnum.System, system);
            }
            if (!String.IsNullOrEmpty(oracle))
            {
                ret.Add(ComponentTypeEnum.Oracle, oracle);
            }
            if (!String.IsNullOrEmpty(efs))
            {
                ret.Add(ComponentTypeEnum.EFS, efs);
            }
            if (!String.IsNullOrEmpty(addefs))
            {
                ret.Add(ComponentTypeEnum.AddEFS, addefs);
            }
            if (!String.IsNullOrEmpty(other))
            {
                ret.Add(ComponentTypeEnum.Other, other);
            }
            if (!String.IsNullOrEmpty(misc))
            {
                ret.Add(ComponentTypeEnum.Misc, misc);
            }
            if (!String.IsNullOrEmpty(temporary))
            {
                ret.Add(ComponentTypeEnum.Temporary, temporary);
            }

            return ret;
        }
        
        
        public static string GetAssembliesList()
        {
            string infos = string.Empty;

            System.Collections.Generic.Dictionary<Software.ComponentTypeEnum, string> assemblies = Software.GetAssemblies();
            
            string componentsList;
            if (assemblies.TryGetValue(Software.ComponentTypeEnum.System, out componentsList))
            {
                infos += "System:" + Cst.CrLf + componentsList + Cst.CrLf;
            }
            if (assemblies.TryGetValue(Software.ComponentTypeEnum.Oracle, out componentsList))
            {
                infos += "Oracle:" + Cst.CrLf + componentsList + Cst.CrLf;
            }
            if (assemblies.TryGetValue(Software.ComponentTypeEnum.EFS, out componentsList))
            {
                infos += "EFS:" + Cst.CrLf + componentsList + Cst.CrLf;
            }
            if (assemblies.TryGetValue(Software.ComponentTypeEnum.AddEFS, out componentsList))
            {
                infos += "External:" + Cst.CrLf + componentsList + Cst.CrLf;
            }
            if (assemblies.ContainsKey(Software.ComponentTypeEnum.Other) || assemblies.ContainsKey(Software.ComponentTypeEnum.Misc))
            {
                if (assemblies.TryGetValue(Software.ComponentTypeEnum.Other, out componentsList))
                {
                    infos += "Other:" + Cst.CrLf + componentsList + Cst.CrLf;
                }
                if (assemblies.TryGetValue(Software.ComponentTypeEnum.Misc, out componentsList))
                {
                    infos += componentsList + Cst.CrLf;
                }
            }
            if (assemblies.TryGetValue(Software.ComponentTypeEnum.Temporary, out componentsList))
            {
                infos += "Temporary:" + Cst.CrLf + componentsList + Cst.CrLf;
            }

            return infos;
        }
         */
        #endregion
    }
}