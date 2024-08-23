using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EFS.ACommon
{
    /// <summary>
    /// 
    /// </summary>
    /// FI 20131106 [19139] Add ServiceAccount, ServiceUserName, ServicePassword
    // PM 20200601 [XXXXX] Déplacé à partir de Common/Service/Service.cs
    public enum ServiceKeyEnum
    {
        /// <summary>
        /// 
        /// </summary>
        ServiceEnum,
        /// <summary>
        /// Obtient ou définit le nom court utilisé pour identifier le service sur le sercice
        /// <para>Exemple :SpheresConfirmationMsgGenv5.1.6102-Inst:ConfirmationGen1 </para>
        /// </summary>
        ServiceName,
        /// <summary>
        /// 
        /// </summary>
        DisplayName,
        /// <summary>
        /// 
        /// </summary>
        Description,
        /// <summary>
        /// 
        /// </summary>
        Instance,
        /// <summary>
        /// Exemple : C:\Program Files\EFS\Spheres GateFIXMLEurex 3.0 RC\SpheresGateFIXMLEurexServicev3.0.exe -sSpheresGateFIXMLEurexv3.0.4737-Inst:Instance
        /// </summary>
        ImagePath,
        /// <summary>
        /// Exemple : SpheresGateFIXMLEurexServicev3.0
        /// </summary>
        ExeName,
        /// <summary>
        /// Exemple C:\Program Files\EFS\Spheres GateFIXMLEurex 3.0 RC\
        /// </summary>
        PathInstall,
        /* FI 20160804 [Migration TFS] Supression de Path 
        /// <summary>
        /// Exemple C:\Program Files\EFS\Spheres GateFIXMLEurex 3.0 RC\SpheresGateFIXMLEurex
        /// </summary>
        Path,
        */
        /* FI 20160804 [Migration TFS] Supression de PathXml (=> Non utilisé)
        ///// <summary>
        ///// Exemple C:\Program Files\EFS\Spheres GateFIXMLEurex 3.0 RC\SpheresGateFIXMLEurex\XML_Files
        ///// </summary>
        //PathXml,
        */
        /// <summary>
        /// Exemple C:\Program Files\EFS\Spheres GateFIXMLEurex 3.0 RC\SpheresGateFIXMLEurexServicev3.0.exe
        /// </summary>
        FullName,
        /// <summary>
        /// {FileWatcher | MSMQ | MQSerie}
        /// </summary>
        MOMType,
        /// <summary>
        /// 
        /// </summary>
        MOMPath,
        /// <summary>
        /// Obtient ou Définit la caractéristique "recoverable" d'un message
        /// <para>
        /// Un message « non recouvrable » est plus rapide à envoyer, car stocké uniquement en RAM. 
        /// <para>Attention: si le service MSMQ est redémarré tous les messages « non recouvrable » de la queue sont perdus !</para>
        /// <para>Default value: true</para>
        /// </summary>
        MOMRecoverable,
        /// <summary>
        /// 
        /// </summary>
        MOMEncrypt,
        /// <summary>
        /// Niveau de log au niveau des services (Pilote l'écriture dans le journal des évènements de Windows®)
        /// <para>Ne pas confondre avec le niveau de log des traitements</para>
        /// </summary>
        LogLevel,
        /// <summary>
        /// Timeout d'inaccessibilité à une queue de type MSMQ
        /// <para>Default value: 60 sec.</para>
        /// </summary>
        MSMQUnreachableTimeout,
        /// <summary>
        /// 
        /// </summary>
        Application,
        /// <summary>
        /// 
        /// </summary>
        AppDirectory,
        /// <summary>
        /// 
        /// </summary>
        ActivateObserver,
        /// <summary>
        /// Classe du service
        /// <para>Exemple EFS.SpheresService.SpheresIOService</para>
        /// </summary>
        ClassType,
        /// <summary>
        /// <para>
        /// Exemple SpheresGateFIXMLEurexv3.0.4737
        /// </para>
        /// </summary>
        Prefix,
        /// <summary>
        /// Activation oui/non du mode cache de connexions sur la gestion des queues.
        /// <para></para>
        /// </summary>
        MSMQEnableConnectionCache,
        /// <summary>
        /// Type d'utilisateur 
        /// <para>Valeurs possibles définies par l'enum System.ServiceProcess.ServiceAccount</para>
        /// </summary>
        ServiceAccount,
        /// <summary>
        /// Compte Windows® qui exécute le service 
        /// <para>Doit être renseigné lorsque ServiceUser = User</para>
        /// </summary>
        ServiceUserName,
        /// <summary>
        /// Mot de passe du compte Windows® qui exécute le service 
        /// <para>Doit être renseigné lorsque ServiceUser = User</para>
        /// </summary>
        ServicePassword
    }

    /// <summary>
    /// Class de methodes indiquant le type de service Spheres
    /// </summary>
    // PM 20200601 [XXXXX] New: créé à partir de méthodes de la classe ServiceTools (Common/Service/Service.cs)
    public static class SpheresServiceTools
    {
        /// <summary>
        /// Retourne si le nom du service commence par spheres (inclus les gateways)
        /// </summary>
        /// <param name="pServiceName"></param>
        /// <returns></returns>
        public static bool IsSpheresService(string pServiceName)
        {
            return pServiceName.ToLower().StartsWith("spheres");
        }
        /// <summary>
        /// Retourne si le nom du service commence par spheresgate
        /// </summary>
        /// <param name="pServiceName"></param>
        /// <returns></returns>
        public static bool IsSpheresGateService(string pServiceName)
        {
            return pServiceName.ToLower().StartsWith("spheresgate");
        }

        /// <summary>
        /// Retourne si le nom du service commence par sphereslogger
        /// </summary>
        /// <param name="pServiceName"></param>
        /// <returns></returns>
        // PM 20200102 [XXXXX] New Log
        public static bool IsSpheresLoggerService(string pServiceName)
        {
            return pServiceName.ToLower().StartsWith("sphereslogger");
        }
    }
}
