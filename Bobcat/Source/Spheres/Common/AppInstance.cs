using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
//
using EFS.ACommon;
using EFS.ApplicationBlocks.Data;
using EFS.Common.Log;
using EFS.Process;
using System.Linq;

namespace EFS.Common
{

    /// <summary>
    /// Represente une Instance d'une application 
    /// </summary>
    public class AppInstance
    {
        // FI 20130318 add const versionIndicator 
        private const string versionIndicator = "v";

        /// <summary>
        /// 
        /// </summary>
        private static readonly Object s_lock = new Object();

        #region Properties

        /// <summary>
        ///  Obtient la l'AppInstance du service ou de l'application Web
        /// </summary>
        public static AppInstance MasterAppInstance
        {
            get;
            private set;
        }

        /// <summary>
        /// Equivalent à AppInstance.MasterAppInstance.AppTraceManager;
        /// </summary>
        public static SpheresTraceManager TraceManager { get => AppInstance.MasterAppInstance.AppTraceManager; }

        /// <summary>
        /// Gestion des traces 
        /// </summary>
        public SpheresTraceManager AppTraceManager { set; get; }

        /// <summary>
        /// Obtient ou définit la nom de machine qui exécute l'instance
        /// </summary>
        public string HostName { get; set; }

        /// <summary>
        /// Obtient ou définit le nom de l'application associé à l'instance
        /// </summary>
        public string AppName { get; set; }

        /// <summary>
        /// Obtient une le nom de l'application et sa version 
        /// <para>(ie SpheresIO v6.0.6060)</para>
        /// </summary>
        /// FI 20130318 utilisation de versionIndicator
        /// EG 20210614 [25500] New Customer Portal
        public string AppNameVersion
        {
            get
            {
                string tmpAppName = AppName;
                if (tmpAppName == "EFS")
                {
                    tmpAppName += " Customer Portal";
                }
                return StrFunc.AppendFormat("{0} {1}{2}", tmpAppName, versionIndicator, AppVersion);
            }
        }

        /// <summary>
        /// Obtient AppName
        /// </summary>
        /// FI 20190701 [XXXXX] Add
        public virtual string AppNameInstance
        {
            get
            {
                return AppName;
            }
        }


        /// <summary>
        /// Obtient ou définit le n° de version de l'application
        /// <para>Ex:6.0.6060</para>
        /// </summary>
        public string AppVersion { get; set; }



        /// <summary>
        /// Chemin physique du répertoire "racine" de l'application:
        /// <para>
        /// - Application Web ( ex.: C:\inetpub\wwwroot\Spheresv6.0.6219 )
        /// </para>
        /// <para>
        /// - Application Service ( ex.: C:\Program Files\EFS\Spheres Services 6.0.6219 )
        /// </para>
        /// </summary>
        public string AppRootFolder { get; set; }

        /// <summary>
        /// Chemin physique du répertoire de "travail" de l'application:
        /// <para>
        /// - Application Web: il s'agit du répertoire "racine" ( ex.: C:\inetpub\wwwroot\Spheresv6.0.6219 )
        /// </para>
        /// <para>
        /// - Application service: il s'agit d'un répertoire enfant du répertoire "racine" => {App Root Folder} \ {serviceEnum} ( ex.: C:\Program Files\EFS\Spheres Services 6.0.6219\SpheresEventsGen )
        /// </para>
        /// </summary>
        public virtual string AppWorkingFolder
        {
            get { return AppRootFolder; }
        }

        /// <summary>
        /// Obtient true si l'application est import/export 
        /// </summary>
        public virtual bool IsIO
        {
            get { return false; }
        }
        #endregion Properties

        #region Constructor
        /// <summary>
        /// 
        /// </summary>
        public AppInstance()
            : this("N/A", Software.Name, Software.VersionBuild, "N/A") { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pHostName">Application Host</param>
        /// <param name="pAppName">Application name</param>
        /// <param name="pAppVersion">Application name</param>
        public AppInstance(string pHostName, string pAppName, string pAppVersion)
            : this(pHostName, pAppName, pAppVersion, "N/A") { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pHostName">Application Host</param>
        /// <param name="pAppName">Application name</param>
        /// <param name="pAppVersion">Application name</param>
        /// <param name="pAppRootFolder">Application root folder</param>
        public AppInstance(string pHostName, string pAppName, string pAppVersion, string pAppRootFolder)
        {
            HostName = pHostName;
            AppName = pAppName;
            AppVersion = pAppVersion;

            AppRootFolder = pAppRootFolder;
            if ((null != AppRootFolder) && (AppRootFolder.EndsWith(@"\")))
                AppRootFolder = AppRootFolder.Substring(0, AppRootFolder.Length - 1);

            lock (s_lock)
            {
                if (null == MasterAppInstance)
                    MasterAppInstance = this;
            }
        }
        #endregion Constructor


        #region Method

        /// <summary>
        /// Initialisation de Diagnostics Trace
        /// </summary>
        public void InitilizeTraceManager()
        {
            if (null == TraceManager)
                AppTraceManager = new SpheresTraceManager(this.AppNameInstance, this.GetFilepath);

            AppTraceManager.NewTrace();
        }

        /// <summary>
        /// Retoure Le chemin {AppWorkingFolder}\{Temporary} ou {AppWorkingFolder}\{Temporary}\{sessionId}
        /// Le Folder est crée lorsque qu'il n'existe pas 
        /// </summary>
        /// <returns></returns>
        /// FI 20160804 [Migration TFS] Modify
        public string GetTemporaryDirectory()
        {
            // FI 20160804 [Migration TFS] MapPath n'est plus utilisé 
            //string ret = MapPath("Temporary");
            string ret = StrFunc.AppendFormat(@"{0}\{1}", AppWorkingFolder, "Temporary");

            if (false == Directory.Exists(ret))
                SystemIOTools.CreateDirectory(ret);

            return ret;
        }

        /// <summary>
        /// Suppression des fichiers présents dans le répertoirte {AppWorkingFolder}\{Temporary}
        /// </summary>
        // EG 20231030 [WI732] Delete Temporary Files and subdirectories
        public void CleanTemporaryDirectory()
        {
            CleanTemporaryDirectory(GetTemporaryDirectory());
        }
        // EG 20231030 [WI732] Delete Temporary Files and subdirectories
        public static void CleanTemporaryDirectory(string pPath)
        {
            DateTime _compareDate = DateTime.Now.AddDays(pPath.StartsWith("SVR-") ? -15 : -7);
            CleanTemporaryDirectoryFiles(pPath, _compareDate);
            CleanTemporaryDirectorySubDirectory(pPath, _compareDate);
        }
        /// <summary>
        /// Suppression des fichiers présents dans le répertoire {AppWorkingFolder}\{Temporary}
        /// de plus de n jours (CompareDate jalon)
        /// </summary>
        /// <param name="pPath">Path du répertoire racine</param>
        /// <param name="pCompareDate">Date jalon pour la suppression</param>
        // EG 20231030 [WI732] Delete Temporary Files and subdirectories
        private static void CleanTemporaryDirectoryFiles(string pPath, DateTime pCompareDate)
        {
            if (Directory.Exists(pPath))
            {
                List<string> excludeFile = new List<string> { ".scc", ".trc", ".txt" };
                Directory.GetFiles(pPath)
                    .Select(f => new FileInfo(f))
                    .Where(f => (f.LastAccessTime < pCompareDate) && (!excludeFile.Contains(f.Extension)))
                    .ToList()
                    .ForEach(f =>
                    {
                        try { f.Delete(); }
                        catch (Exception) { }
                    });
            }
        }

        /// <summary>
        /// Suppression des sous-répertoires de {AppWorkingFolder}\{Temporary} (avec récursivité) 
        /// de plus de n jours
        /// </summary>
        /// <param name="pPath">Path du répertoire racine</param>
        /// <param name="pCompareDate">Date jalon pour la suppression</param>
        // EG 20231030 [WI732] Delete Temporary Files and subdirectories
        public static void CleanTemporaryDirectorySubDirectory(string pPath, DateTime pCompareDate)
        {
            if (Directory.Exists(pPath))
            {
                Directory.GetDirectories(pPath)
                    .Select(d => new DirectoryInfo(d))
                    .Where(d => (d.LastWriteTime < pCompareDate))
                    .ToList()
                    .ForEach(d =>
                    {
                        try { d.Delete(true); }
                        catch (Exception) { }
                    });
            }
        }

        /// <summary>
        /// Retourne {AppRootFolder}\{pPath}
        /// </summary>
        /// <param name="pPath"><para>null ou vide autorisé</para>"~" pour obtenir AppRootFolder<para></para></param>
        /// <returns></returns>
        /// FI 20160804 [Migration TFS] Modify
        public string MapPath(string pPath)
        {
            //FI 20160804 [Migration TFS] La racine est désormais exeFolder 
            //=>le folder BusinessSchemas est désormais unique et se trouve physique au même endroit que l'exe des ser
            //string ret = string.Empty;
            //if (isValidInfo(m_workingFolder))
            //{
            //    ret = m_workingFolder;
            //    ret = this.ExeFolder;
            //    if (StrFunc.IsFilled(pPath))
            //        ret += @"\" + pPath;
            //}

            string ret = string.Empty;

            if (pPath == "~" || StrFunc.IsEmpty(pPath))
                ret = this.AppRootFolder;
            else
                ret += this.AppRootFolder + @"\" + pPath;

            return ret;
        }

        /// <summary>
        /// Fonction qui permet d'interpréter le chemin lorsqu'il existe un "."  ou "~"
        /// <para>. est emplacé par  {AppRootFolder}</para>
        /// <para>~ est remplacé par {AppWorkingFolder}</para>
        /// <para>
        /// </para>
        /// </summary>
        /// <param name="pPathFile">Chemin d'accès à un fichier ou un répertoire</param>
        /// <returns></returns>
        public string GetFilepath(string pPathFile)
        {
            string ret = pPathFile;

            if (false == File.Exists(pPathFile))
            {
                if (StrFunc.ContainsIn(pPathFile, @".\"))
                    ret = ret.Replace(@".\", AppRootFolder + @"\");

                if (StrFunc.ContainsIn(pPathFile, @"~\"))
                    ret = ret.Replace(@"~\", AppWorkingFolder + @"\");
            }
            return ret;
        }

        /// <summary>
        /// Recherche d'un fichier XML, XSL dans le cadre d'un processus de Messagerie, I/O, etc.
        ///<para>
        /// NB: Utilisation donnée en priorité aux fichiers stockés dans la table FILECONFIG (see also FileTools.GetFile2).
        /// </para>
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pSearchPathFile"></param>
        /// <returns></returns>
        // EG 20180205 [23769] Add dbTransaction  
        public bool SearchFile2(string pCS, string pSearchPathFile, ref string opPathFile)
        {
            return SearchFile2(pCS, null, pSearchPathFile, ref opPathFile);
        }
        // EG 20180205 [23769] Add dbTransaction  
        public bool SearchFile2(string pCS, IDbTransaction pDbTransaction, string pSearchPathFile, ref string opPathFile)
        {
            bool isFound = false;
            opPathFile = pSearchPathFile;

            if (StrFunc.IsFilled(pSearchPathFile))
            {
                opPathFile = GetFilepath(pSearchPathFile);

                //PL 20161024 WARNING: Add if()
                bool isStartWith_AppWorkingFolder = (opPathFile.IndexOf(AppWorkingFolder) >= 0);
                bool isStartWith_AppRootFolder = (opPathFile.IndexOf(AppRootFolder) >= 0);
                if (isStartWith_AppWorkingFolder || isStartWith_AppRootFolder)
                {
                    #region Interprétation...
                    #region v6.x
                    //PL 20170123 New GetFile2
                    //Interprétation lorsque fichier {WorkingFolder}[\*]\*.*
                    // ex.: C:\EFS\Services\v6.0.6219\Message\Report\XYZ.xsl
                    //      avec : 
                    //              [0] Category        ex. Message
                    //              [1] Sub-folder(s)   ex. Report 
                    //NB: pour plus d'info sur WorkingFolder voir les commentaires de cette propriété.  
                    string tmp_PathFile = string.Empty;
                    if (isStartWith_AppWorkingFolder)
                        tmp_PathFile = opPathFile.Replace(AppWorkingFolder + @"\", string.Empty);
                    else if (isStartWith_AppRootFolder)
                        tmp_PathFile = opPathFile.Replace(AppRootFolder + @"\", string.Empty);

                    string[] tmp_Data = tmp_PathFile.Split('\\');
                    string fileName = tmp_Data[tmp_Data.Length - 1];
                    string fileType = string.Empty;
                    string subFolder = string.Empty;
                    if (fileName.EndsWith(".xml", StringComparison.InvariantCultureIgnoreCase))
                    {
                        fileType = "XML";
                        fileName = fileName.Remove(fileName.Length - 4);
                    }
                    else if (fileName.EndsWith(".xsl", StringComparison.InvariantCultureIgnoreCase))
                    {
                        fileType = "XSL";
                        fileName = fileName.Remove(fileName.Length - 4);
                    }
                    else if (fileName.EndsWith(".xslt", StringComparison.InvariantCultureIgnoreCase))
                    {
                        fileType = "XSLT";
                        fileName = fileName.Remove(fileName.Length - 5);
                    }

                    if (StrFunc.IsFilled(fileType))
                    {
                        //if (fileType.StartsWith("XSL") && (ArrFunc.Count(tmp_Data) > 2))
                        if (ArrFunc.Count(tmp_Data) > 2)
                        {
                            //On reconstitue ici le chemin du sous-folder
                            for (int i = 1; i < tmp_Data.Length - 1; i++)
                            {
                                if (subFolder.Length > 0)
                                    subFolder += @"\";
                                subFolder += tmp_Data[i];
                            }
                        }

                        string mainFolder = tmp_Data[0]; ; //La catégorie représente le "Main Folder".
                        //+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-
                        //FI/PL 20170208 WARNING: Use "AppRootFolder" instead of "AppWorkingFolder" suite à pb en RELAESE sur la Messagerie.
                        //                        Voir peut-être pour utiliser l'un ou l'autre sur la base de isStartWith_AppWorkingFolder 
                        //                        et isStartWith_AppRootFolder !!!
                        //+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-
                        //isFound = FileTools.GetFile3(pCS, AppWorkingFolder, fileType, mainFolder, subFolder, fileName, ref opPathFile);
                        isFound = FileTools.GetFile3(pCS, pDbTransaction, AppRootFolder, fileType, mainFolder, subFolder, fileName, ref opPathFile);
                        //+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-
                    }
                    #endregion
                    #endregion Interprétation...
                }

                if (!isFound)
                    isFound = File.Exists(opPathFile);
            }

            return isFound;
        }


        /// <summary>
        /// Retourne true si {pAppNameVersion} correspond à une version de application Web de Spheres®
        /// <para>( ie l'application se nomme Spheresvx.x.xxxx)</para>
        /// </summary>
        /// <param name="pAppNameVersion">Nom de lapplication avec version (ie Spheresv3.1.5480)</param>
        /// FI 20130318 [] add IsSpheresWebApp
        public static bool IsSpheresWebApp(string pAppNameVersion)
        {
            if (StrFunc.IsEmpty(pAppNameVersion))
                throw new ArgumentException("AppNameVersion Argument is empty");
            string[] splitResult = pAppNameVersion.Split(versionIndicator.ToCharArray());

            bool ret = splitResult[0].Trim() == Software.SOFTWARE_Spheres;
            return ret;
        }

        #endregion
    }

    /// <summary>
    /// Represente une instance d'un service
    /// </summary>
    public class AppInstanceService : AppInstance
    {
        #region Members
        #endregion

        #region properties
        /// <summary>
        /// Collection des sémaphore d'interruption de traitement ouverte
        /// </summary>
        // EG 20180525 [23979] IRQ Processing
        public List<IRQSemaphore> LstIRQSemaphore
        {
            get;
            private set;
        }

        /// <summary>
        /// MOM PAth du service
        /// <para>(ie C:\Spheres\Process\Queue\EventsGen or "POSTE-059\private$\spheres_eventsgen")</para>
        /// </summary>
        public string MOMPath
        {
            get;
            private set;
        }

        /// <summary>
        /// Type de service
        /// </summary>
        public Cst.ServiceEnum ServiceEnum
        {
            get;
            private set;
        }

        /// <summary>
        /// Repertoire où se trouve les exe
        /// Ex C:\Spheres\Spheres Services 6.0.6060\SpheresEventsGen
        /// </summary>
        public override string AppWorkingFolder
        {
            get { return AppRootFolder + @"\" + ServiceEnum; }
        }
        /// <summary>
        /// Obtient ou définie le nom du service 
        /// <para>Nom complet du service (exemple SpheresGateFIXMLEurexv3.1.4748-Inst:CCLXV)</para>
        /// </summary>
        public string ServiceName
        {
            get;
            private set;
        }
        /// <summary>
        /// Obtient le nom du service sans l'instance
        /// </summary>
        public string ServiceName_WithoutInstance
        {
            get
            {
                int postInstance = ServiceName.LastIndexOf("-Inst:");
                if (postInstance > 0)
                {
                    return ServiceName.Substring(0, postInstance);
                }
                else
                {
                    return ServiceName;
                }
            }
        }
        /// <summary>
        /// Obtient true si le service est SpheresIO 
        /// </summary>
        public override bool IsIO
        {
            get
            {
                return (ServiceEnum == Cst.ServiceEnum.SpheresIO);
            }
        }
        /// <summary>
        /// Obtient le nom du service sans la version et avec l'instance  -Inst:xxx
        /// <para>(ie : SpheresIO-Inst:TEST)</para>
        /// <para></para>
        /// </summary>
        // FI 20131004 [] add property
        public override string AppNameInstance
        {
            get
            {

                string ret = base.AppNameInstance;

                int postInstance = ServiceName.LastIndexOf("-Inst:");
                if (postInstance > 0)
                    ret += ServiceName.Substring(postInstance, ServiceName.Length - postInstance);
                return ret;
            }
        }

        /// <summary>
        /// Obtient true si le service est une Gateway 
        /// </summary>
        /// PM 20210531 [XXXXX] Pas de Logger pour les Gateways
        public bool IsGateway
        {
            get
            {
                return ((ServiceEnum == Cst.ServiceEnum.SpheresGateBCS) || (ServiceEnum == Cst.ServiceEnum.SpheresGateFIXMLEurex));
            }
        }
        #endregion

        #region constructor
        /// <summary>
        ///  constructor
        /// </summary>
        /// <param name="pHostName"></param>
        /// <param name="pServiceEnum">type de service</param>
        /// <param name="pServiceName"></param>
        /// <param name="pAppVersion"></param>
        /// <param name="pAppRootFolder">Chemin physique où se trouve l'exe</param>
        /// <param name="pMOMPath"></param>
        public AppInstanceService(string pHostName,
                Cst.ServiceEnum pServiceEnum, string pServiceName, string pAppVersion, string pAppRootFolder, string pMOMPath)
            : base(pHostName, pServiceEnum.ToString(), pAppVersion, pAppRootFolder)
        {
            ServiceEnum = pServiceEnum;
            ServiceName = pServiceName;
            MOMPath = pMOMPath;

            LstIRQSemaphore = new List<IRQSemaphore>();
        }
        #endregion constructor
    }

    /// <summary>
    /// Représente une session 
    /// </summary>
    public class AppSession
    {

        /// <summary>
        /// 
        /// </summary>
        public enum AddFolderSessionId
        {
            True,
            False,
        }


        /// <summary>
        /// 
        /// </summary>
        public AppInstance AppInstance
        {
            get;
        }

        /// <summary>
        /// Obtient ou définit l'acteur qui lance l'instance
        /// </summary>
        public int IdA { get; set; }

        /// <summary>
        /// Obtient ou définit l'identifiant de l'acteur qui lance l'instance
        /// </summary>
        public string IdA_Identifier { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public int IdA_Entity { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string IdA_Identifier_Entity { get; set; }

        /// <summary>
        /// Obtient ou définit l'identificateur globale unique de l'instance
        /// </summary>
        public string SessionId { get; set; }

        /// <summary>
        /// Obtient ou définit les informations concernant le Browser
        /// </summary>
        public string BrowserInfo { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public AppSession(AppInstance appInstance)
        {
            this.AppInstance = appInstance;
        }


        /// <summary>
        /// Retourne un Id unique (s'appuie sur SessionId) qui doit utilisé lors de la construction de table de travail ("temporaires")
        /// Une table de travail doit par convention se terminer par "_{BuildTableId}_W" 
        /// <para>La méthode ProcessBase.Dispose() fait appel à la méthode DropWorkTable()</para>
        /// </summary>
        /// <returns></returns>
        /// FI 20170420 [23075] Add Method  
        public string BuildTableId()
        {
            if (SessionId.Length < 10)
                throw new Exception("Invalid SessionId, SessionId is too short");
            return SessionId.Substring(0, 10).Replace("-", "X").ToUpper();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCS"></param>
        /// FI 20170420 [23075] Add Method  
        /// FI 20170420 [23075] GLOP (cette méthode est un copier coller de ce qui existe de DropTemporaryTable (global.asax) 
        /// Il faudra supprimer ce qui existe dans global.asax
        // EG 20180425 Analyse du code Correction [CA2202]
        public void DropWorkTable(string pCS)
        {

            string tableId = BuildTableId();
            string cmd;
            if (DataHelper.IsDbSqlServer(pCS))
            {
                cmd = StrFunc.AppendFormat("select name from sysobjects where name like '%[_]{0}[_]W' and xtype='U'", tableId);
            }
            else if (DataHelper.IsDbOracle(pCS))
            {
                cmd = StrFunc.AppendFormat("select TABLE_NAME from USER_TABLES where TABLE_NAME like '%#_{0}#_W' escape '#'", tableId);
            }
            else
                throw new NotImplementedException("RDBMS not implemented");

            using (IDataReader dr = DataHelper.ExecuteReader(pCS, CommandType.Text, cmd))
            {
                while (dr.Read())
                {
                    string option = string.Empty;
                    if (DataHelper.IsDbOracle(pCS))
                        option = "purge";

                    cmd = StrFunc.AppendFormat("drop table {0} {1}", dr[0].ToString(), option);

                    DataHelper.ExecuteNonQuery(pCS, CommandType.Text, cmd);
                }
            }
        }

        /// <summary>
        /// Retoure Le chemin {AppWorkingFolder}\{Temporary} ou {AppWorkingFolder}\{Temporary}\{sessionId}
        /// Le Folder est crée lorsque qu'il n'existe pas 
        /// </summary>
        /// <returns></returns>
        /// FI 20160804 [Migration TFS] Modify
        public string GetTemporaryDirectory(AddFolderSessionId pAddFolderSession)
        {

            string ret = AppInstance.GetTemporaryDirectory();


            if (AddFolderSessionId.True == pAddFolderSession)
                ret += @"\" + SessionId;

            if (false == Directory.Exists(ret))
                SystemIOTools.CreateDirectory(ret);

            return ret;
        }

        /// <summary>
        ///  Retoure Le chemin {AppWorkingFolder}\{Temporary}\{pPath}
        /// </summary>
        /// <param name="pPath">null ou vide autorisé</param>
        /// <param name="pAddFolderSessionId"></param>
        /// <returns></returns>
        /// FI 20160804 [Migration TFS] Modify
        public string MapTemporaryPath(string pPath, AddFolderSessionId pAddFolderSessionId)
        {
            string ret = GetTemporaryDirectory(pAddFolderSessionId);
            if (StrFunc.IsFilled(pPath))
                ret += @"\" + pPath;
            return ret;
        }
    }
}
