using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
//
using Microsoft.Win32;

namespace EFS.ACommon
{
    /// <summary>
    /// Class pour les constantes de la registry
    /// </summary>
    // PM 20200601 [XXXXX] New: créé à partir des constantes de la classe ServiceTools (Common/Service/Service.cs)
    public static class RegistryConst
    {
        #region public const
        /// <summary>
        /// System\\CurrentControlSet\\Services\\
        /// </summary>
        public const string RegistryKey = "System\\CurrentControlSet\\Services\\";
        /// <summary>
        /// Parameters
        /// </summary>
        public const string RegistrySubKeyParameters = "Parameters";
        /// <summary>
        /// Eventlog
        /// </summary>
        public const string RegistrySubKeyLog = "Eventlog";
        /// EG 20130619 Replace "_" by "-Inst:"
        public const string DelimiterInstance = "-Inst:";
        #endregion public const
    }

    /// <summary>
    /// Class d'accés à la registry
    /// </summary>
    // PM 20200601 [XXXXX] New: créé à partir de méthodes de la classe ServiceTools (Common/Service/Service.cs)
    public static class RegistryTools
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pServiceName"></param>
        /// <returns></returns>
        public static void AddServiceNameToImagePath(string pServiceName)
        {
            RegistryKey service = null;
            try
            {
                //PL 20120319 Add pIsWritable = true
                service = RegistryTools.GetRegistryKeyService(true, pServiceName);
                if (null != service)
                {
                    string key = ServiceKeyEnum.ImagePath.ToString();
                    string realImagePath = (string)service.GetValue(key);
                    if (StrFunc.IsFilled(realImagePath))
                    {
                        realImagePath = realImagePath.Replace("\"", string.Empty);
                    }
                    int i = realImagePath.IndexOf("-s");
                    if (-1 < i)
                    {
                        realImagePath = realImagePath.Substring(0, i);
                    }
                    service.SetValue(ServiceKeyEnum.ImagePath.ToString(), realImagePath + " -s" + pServiceName);
                }
            }
            finally
            {
                if (null != service)
                    service.Close();
            }
        }

        /// <summary>
        /// /
        /// </summary>
        /// <param name="pIsWritable"></param>
        /// <param name="pServiceName"></param>
        /// <param name="pAdditionalParameters"></param>
        /// <returns></returns>
        /// PL 20120319 Add pIsWritable
        /// EG 20130619 Passage Public -> Private
        /// PM 20200601 [XXXXX] Déplacé à partir de la classe ServiceTools (Common/Service/Service.cs) et passage Private -> Public
        public static RegistryKey GetRegistryKeyService(bool pIsWritable, string pServiceName, params string[] pAdditionalParameters)
        {
            if (pAdditionalParameters == null || pAdditionalParameters.Length == 0 || pAdditionalParameters.Length == 3)
            {
                return Registry.LocalMachine.OpenSubKey(RegistryConst.RegistryKey + pServiceName, pIsWritable);
            }
            else if (pAdditionalParameters.Length == 4)
            {
                return Registry.LocalMachine.OpenSubKey(pAdditionalParameters[3], pIsWritable);
            }
            else
            {
                throw new ArgumentOutOfRangeException("pAdditionalParameters", "pAdditionalParameters length not equals 3 or 4");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pIsWritable"></param>
        /// <param name="pServiceName"></param>
        /// <param name="pAdditionalParameters"></param>
        /// <returns></returns>
        /// PL 20120319 Add pIsWritable
        /// EG 20130619 Passage Public -> Private
        /// PM 20200601 [XXXXX] Déplacé à partir de la classe ServiceTools (Common/Service/Service.cs) et passage Private -> Public
        public static RegistryKey GetRegistryKeyServiceParameters(bool pIsWritable, string pServiceName, params string[] pAdditionalParameters)
        {
            RegistryKey service = GetRegistryKeyService(pIsWritable, pServiceName, pAdditionalParameters);
            if (null != service)
            {
                return service.OpenSubKey(RegistryConst.RegistrySubKeyParameters, pIsWritable);
            }
            return null;
        }

        #region EventLog
        /// <summary>
        /// Retourne le nom du journal des évènements de windows utilisé par un Service Spheres 
        /// <para>- soit SpheresGateways</para>
        /// <para>- soit Spheres</para>
        /// </summary>
        /// <param name="pServiceName"></param>
        /// <returns></returns>
        /// FI 20161026 [XXXXX] Add
        public static string GetEventLog(string pServiceName)
        {
            string ret;
            if (SpheresServiceTools.IsSpheresGateService(pServiceName))
                ret = Cst.SpheresGatewayEventLog;
            else if (SpheresServiceTools.IsSpheresService(pServiceName))
                ret = Cst.SpheresEventLog;
            else
                throw new NotImplementedException(StrFunc.AppendFormat("Service (name:{0}) is not valid", pServiceName));
            return ret;
        }

        /// <summary>
        /// Retourne le nom de la source pour écriture dans le journal des évènements de windows 
        /// </summary>
        /// <param name="pServiceName"></param>
        /// <returns></returns>
        public static string GetEventLogSource(string pServiceName)
        {
            return StrFunc.AppendFormat("{0}{1}", pServiceName, Cst.EventLogSourceExtension);
        }
        
        /// <summary>
        /// Alimentation de System\\CurrentControlSet\\Services\\eventLog
        /// </summary>
        /// <param name="pServiceName"></param>
        /// FI 20161026 [XXXXX] Modify
        public static void SetEventLogService(string pServiceName)
        {
            RegistryKey sourceLog = null;
            RegistryKey serviceLog = null;
            RegistryKey service = null;

            try
            {
                //PL 20120319 Use pIsWritable = false
                service = RegistryTools.GetRegistryKeyService(false, pServiceName);
                string eventMessageFile = (string)service.GetValue("ImagePath");
                int i = eventMessageFile.LastIndexOf("\\");
                if (-1 < i)
                {
                    eventMessageFile = eventMessageFile.Substring(0, i + 1) + "SpheresServicesMessage.dll";
                }

                //PL 20120319 Add pIsWritable = true
                service = RegistryTools.GetRegistryKeyService(true, RegistryConst.RegistrySubKeyLog);

                // FI 20161026 [XXXXX]  call GetEventLog(pServiceName)
                // EventLog 
                serviceLog = service.CreateSubKey(GetEventLog(pServiceName));

                string sourceLogName = RegistryTools.GetEventLogSource(pServiceName);
                sourceLog = serviceLog.CreateSubKey(sourceLogName);
                sourceLog.SetValue("EventMessageFile", eventMessageFile);
                sourceLog.SetValue("CategoryMessageFile", eventMessageFile);
                sourceLog.SetValue("CategoryCount", 4);
                sourceLog.SetValue("TypesSupported", 7);
            }
            finally
            {
                if (null != sourceLog)
                {
                    sourceLog.Close();
                }
                if (null != serviceLog)
                {
                    serviceLog.Close();
                }
                if (null != service)
                {
                    service.Close();
                }
            }
        }

        /// <summary>
        /// Suprresion des entrées utilisées pour écriture dans le journal  des événements de windows®
        /// </summary>
        /// FI 20161026 [XXXXX] Modify
        public static void DeleteEventLogService(string pServiceName)
        {
            RegistryKey serviceLog = null;
            RegistryKey service = null;
            try
            {
                string logName = RegistryTools.GetEventLog(pServiceName);
                string logSource = RegistryTools.GetEventLogSource(pServiceName);
                //PL 20120319 Add pIsWritable = true
                service = RegistryTools.GetRegistryKeyService(true, RegistryConst.RegistrySubKeyLog);
                if (null != service)
                {
                    serviceLog = service.OpenSubKey(logName, true);
                    if (serviceLog.OpenSubKey(logSource) != null)
                    {
                        serviceLog.DeleteSubKeyTree(logSource);
                    }
                }
            }
            finally
            {
                if (null != serviceLog)
                {
                    serviceLog.Close();
                }
                if (null != service)
                {
                    service.Close();
                }
            }
        }
        #endregion EventLog
    }
}
