using System;
using System.Text.RegularExpressions;
using System.Linq;

using EFS.ACommon;
using EFS.SpheresService;
using EFS.Common.MQueue;

namespace EFS.Common
{

    /// <summary>
    ///  Représente les caractéristiques d'une queue MOM 
    /// </summary>
    [System.Xml.Serialization.XmlRoot("MOM")]
    [Serializable]
    public class MOMSettings
    {
        #region Members
        /// <summary>
        /// Obtient ou définit le type de MOM 
        /// </summary>
        [System.Xml.Serialization.XmlElement("MOMType")]
        public Cst.MOM.MOMEnum MOMType;
        /// <summary>
        /// Obtient ou définit le chemin d'accès au MOM
        /// </summary>
        [System.Xml.Serialization.XmlElement("MOMPath")]
        public string MOMPath;
        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlElement("MOMEncrypt")]
        public bool MOMEncrypt;
        /// <summary>
        /// Obtient ou Définit la caractéristique "recoverable" d'un message
        /// <para>
        /// Un message « non recouvrable » est plus rapide à envoyer, car… stocké uniquement en RAM. 
        /// <para>(Attention si le service MSMQ est redémarré tous les messages de la queue sont perdus</para>
        /// </para>
        /// </summary>
        [System.Xml.Serialization.XmlElement("MOMRecoverable")]
        public bool MOMRecoverable;
        #endregion
        //
        #region accessor
        /// <summary>
        /// Obtient si MOMType != Unkwown et MOMPath est renseigné 
        /// </summary>
        public bool IsInfoValid
        {
            get { return ((Cst.MOM.MOMEnum.Unknown != MOMType) && StrFunc.IsFilled(MOMPath)); }
        }
        #endregion
        //
        #region Method

        /// <summary>
        /// Retourne les informations spécifiques au MOM présentes dans le fichier de configuration
        /// <para>Lecture de AppSettingsSection</para>
        /// </summary>
        /// <param name="pProcessType">si "NA" lecture des clés MOMType, MOMPath, MOMEncrypt, MOMRecoverable. Si différent de "NA", lecture en priorité des clé MOMType_<paramref name="pProcessType"/> etc.</param>
        /// <returns></returns>
        public static MOMSettings LoadMOMSettings(Cst.ProcessTypeEnum pProcessType)
        {
            MOMSettings ret = new MOMSettings();
            string suffix = string.Empty;
            if (pProcessType != Cst.ProcessTypeEnum.NA)
            {
                suffix = @"_" + pProcessType.ToString();
            }

            //Step 1a: Recherche d'un Type spécifique (ex. MOMType_IO)
            ret.MOMType = Cst.MOM.GetMOMEnum(SystemSettings.GetAppSettings(Cst.MOM.MOMType + suffix));
            if ((ret.MOMType == Cst.MOM.MOMEnum.Unknown) && (StrFunc.IsFilled(suffix)))
            {
                //Step 1b: Recherche d'un Type commun (ex. MOMType)
                ret.MOMType = Cst.MOM.GetMOMEnum(SystemSettings.GetAppSettings(Cst.MOM.MOMType));
            }

            //Step 2a: Recherche d'un Path spécifique (ex. MOMPath_IO)
            ret.MOMPath = SystemSettings.GetAppSettings(Cst.MOM.MOMPath + suffix);
            if (StrFunc.IsEmpty(ret.MOMPath) && (StrFunc.IsFilled(suffix)))
            {
                //Step 2b: Recherche d'un Path commun (ex. MOMPath)
                ret.MOMPath = SystemSettings.GetAppSettings(Cst.MOM.MOMPath);
            }
            ret.MOMPath = TranslateMOMPath(ret.MOMPath, Software.VersionBuild);
#if DEBUG
            //PL for debug +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-
            if (Environment.MachineName == "DWS-136")
            {
                // ret.MOMPath = @"C:\SpheresServices\Queue";
                //PL 20220614 Test in progress... Unavailable MOMPath
                //ret.MOMPath = @"A:\SpheresServices\Queue";
            }
            //PL for debug +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-
#endif

            ret.MOMEncrypt = (bool)SystemSettings.GetAppSettings(Cst.MOM.MOMEncrypt, typeof(System.Boolean), false);
            ret.MOMRecoverable = (bool)SystemSettings.GetAppSettings(Cst.MOM.MOMRecoverable, typeof(System.Boolean), true);

            return ret;
        }

        /// <summary>
        ///  retourne le path du MOM après interprétation des mots clés MAJOR(), MINOR(), REVISON(), BUILD() lorsque présents 
        /// </summary>
        /// <param name="pMOMPathInitial">Path initial</param>
        /// <param name="pVersion">{Major}.{Minor}.{Revision} ou {Major}.{Minor}.{Revision}.{Build}</param>
        /// FI 20210804 [XXXXX] Add Method
        public static string TranslateMOMPath(string pMOMPathInitial, string pVersion)
        {
            if (StrFunc.IsEmpty(pVersion))
                throw new ArgumentException("Empty value not allowed", "pVersion");

            Regex regex = new Regex(@"^\d+.\d+.\d+(?:.\d+){0,1}$", RegexOptions.IgnoreCase);
            if (false == regex.IsMatch(pVersion))
                throw new ArgumentException(StrFunc.AppendFormat("version format :{0} is not valid", pVersion));

            string ret = pMOMPathInitial;

            // EG 20131014 Test IsFilled
            if (StrFunc.IsFilled(ret) && ret.IndexOf("()") > 0)
            {
                string[] version = pVersion.Split('.');
                switch (ArrFunc.Count(version))
                {
                    case 3:
                        ret = ret.Replace("MAJOR()", version[0]).Replace("MINOR()", version[1]).
                            Replace("REVISION()", version[2]);
                        break;
                    case 4:
                        ret = ret.Replace("MAJOR()", version[0]).Replace("MINOR()", version[1]).
                            Replace("REVISION()", version[2]).Replace("BUILD()", version[3]);
                        break;

                }
            }
            return ret;
        }

        /// <summary>
        /// Vérifie l'existence et la disponibilité de la Queue (MOMType+MOMPath).
        /// <para>Génère une erreur en cas d'indisponibilité de la Queue.</para>
        /// </summary>
        //PL 20220614 New method
        //PL 20221025 New signature of CheckMOMSettings() with 3 parameters
        public void CheckMOMSettings(string pCS, Cst.ProcessTypeEnum pProcess, int pIdA_Entity)
        {
            //PL 20221025 Le commentaire ci-dessous n'a plus lieu d'être, on recherche maintenant ici le Suffix à partir des 3 parameters.
            //----------------------------------------------------------------------------------------------------------------------------------------
            //ATTENTION: MOMPath doit être ici "complet". Dans le cas d'une Gateway BCS il doit par exemple disposer de l'éventuel suffix "MemberCode"
            //           Pour plus de détail, voir la surcharge GetQueueSuffix(string pConnectionString, Cst.ServiceEnum pService, int pIdA_Entity)
            //----------------------------------------------------------------------------------------------------------------------------------------
            string suffix = ServiceTools.GetQueueSuffix(pCS, Cst.Process.GetService(pProcess), pIdA_Entity);

            switch (MOMType)
            {
                case Cst.MOM.MOMEnum.FileWatcher:
                    FileTools.CheckFolder(StrFunc.AppendFormat(@"{0}\{1}", MOMPath, suffix), 1, out _);
                    break;
                case Cst.MOM.MOMEnum.MSMQ:
                    MQueueTools.GetMsMQueue(StrFunc.AppendFormat(@"{0}{1}", MOMPath, suffix), 1, out _);
                    break;
                default:
                    throw new NotImplementedException(StrFunc.AppendFormat("{0} is not implemented", MOMType.ToString()));
            }
        }
        #endregion
    }
}