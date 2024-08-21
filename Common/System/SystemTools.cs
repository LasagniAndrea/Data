using System;
using System.Globalization;
using System.Collections.Generic;
using System.Text;
using System.Security.Principal;
using System.Configuration;
using System.Threading;

using EFS.ApplicationBlocks.Data;
using EFS.ACommon;


namespace EFS.Common
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class SystemTools
    {
        /// <summary>
        /// Identite sous windows de l'utilisateur se lequel tourne l'application Courante 
        /// </summary>
        /// <returns></returns>
        public static string PrincipalWindowsIdentityGetcurrent()
        {
            return WindowsIdentity.GetCurrent().Name;
        }


        /// <summary>
        /// Retourne un nouvei identificateur global unique
        /// </summary>
        /// <returns></returns>
        public static string GetNewGUID()
        {
            Guid guid = Guid.NewGuid();
            return guid.ToString();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static bool IsFramework1()
        {
            return Environment.Version.Major == 1;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static bool IsFramework2()
        {
            return Environment.Version.Major == 2;
        }

        /// <summary>
        /// Retourne l'horodatage système dans le fuseau horaire local
        /// </summary>
        /// <returns></returns>
        public static DateTime GetOSDateSys()
        {
            return GetOSDateSys(false);
        }

        /// <summary>
        /// Retourne l'horodatage système selon le fuseau horaire local ou UTC
        /// </summary>
        /// <returns></returns>
        /// FI 20200901 [25468] Add
        public static DateTime GetOSDateSys(bool pIsUTC)
        {
            if (pIsUTC)
                return DateTime.Now.ToUniversalTime();
            else
                return DateTime.Now;
        }

        
    }
}
