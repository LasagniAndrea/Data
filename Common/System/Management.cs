using System;
using System.Collections.Generic;
using System.Text;
using System.Management;

namespace EFS.Common
{
    #region public class ManagementObjectSearcher
    public class ManagementObjectSearcher
    {
        public static int CpuInfo(out string pCpuName, out string pCpuID)
        {
            pCpuName = string.Empty;
            pCpuID = string.Empty;
            System.Management.ManagementObjectSearcher Win32Processor = new System.Management.ManagementObjectSearcher("select * from win32_processor");
            foreach (ManagementObject Source in Win32Processor.Get())
            {
                pCpuName = Source["Name"].ToString();
                pCpuID = Source["ProcessorID"].ToString();
            }

            return 0;
        }
        public static int AccountServiceInfo(string pServiceName, out string pAccountService)
        {
            pAccountService = string.Empty;
            string qry = "select * from win32_service where name='" + pServiceName + "'";
            System.Management.ManagementObjectSearcher win32Service = new System.Management.ManagementObjectSearcher(qry);
            foreach (ManagementObject service in win32Service.Get())
                pAccountService = service["StartName"].ToString();
            return 0;
        }
    }
    #endregion

    #region public class SIDClass
    public class SIDClass
    {
        private System.Management.ManagementObjectSearcher query;
        private System.Management.ManagementObjectCollection queryCollection;

        public string ShowUserSID(string username)
        {
            // local scope 
            ConnectionOptions co = new ConnectionOptions
            {
                Username = username
            };
            ManagementScope msc = new ManagementScope("\\root\\cimv2", co);
            string queryString = "SELECT * FROM Win32_UserAccount where name='" + co.Username + "'";
            SelectQuery q = new SelectQuery(queryString);
            query = new System.Management.ManagementObjectSearcher(msc, q);
            queryCollection = query.Get();
            string res = String.Empty;
            foreach (ManagementObject mo in queryCollection)
            {
                // there should be only one here! 
                res += mo["SID"].ToString();
            }
            return res;
        }
    }
    #endregion
}
