using EFS.ACommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace EFS.Common.Net
{
    public sealed class DnsHelper
    {

        /// <summary>
        /// Returns the IPv4 address of the specified host name or IP address.
        /// </summary>
        /// <param name="hostNameOrAddress">The host name or IP address to resolve.</param>
        /// <returns>The first IPv4 address associated with the specified host name, or null.</returns>
        public static string GetIPv4Address(string hostNameOrAddress)
        {
            // Get the list of IP addresses for the specified host
            IPAddress[] aIPHostAddresses = Dns.GetHostAddresses(hostNameOrAddress);

            // First try to find a real IPV4 address in the list
            foreach (IPAddress ipHost in aIPHostAddresses)
                if (ipHost.AddressFamily == AddressFamily.InterNetwork)
                    return ipHost.ToString();

            // If that didn't work, try to lookup the IPV4 addresses for IPV6 addresses in the list
            foreach (IPAddress ipHost in aIPHostAddresses)
                if (ipHost.AddressFamily == AddressFamily.InterNetworkV6)
                {
                    IPHostEntry ihe = Dns.GetHostEntry(ipHost);
                    foreach (IPAddress ipEntry in ihe.AddressList)
                        if (ipEntry.AddressFamily == AddressFamily.InterNetwork)
                            return ipEntry.ToString();
                }

            return hostNameOrAddress;
        }

        /// <summary>
        /// Returns the IPv4 address of the specified host name or IP address. Return <paramref name="hostNameOrAddress"/> if IP is not available
        ///  </summary>
        /// <param name="hostNameOrAddress"></param>
        /// <param name="ret"></param>
        /// <returns></returns>
        public static bool TryGetIPv4Address(string hostNameOrAddress, out string ret)
        {
            Boolean isOK = true;
            try
            {
                ret = GetIPv4Address(hostNameOrAddress);
            }
            catch
            {
                ret = hostNameOrAddress;
                isOK = false;
            }
            return isOK;
        }

        /// <summary>
        ///  Returns the Machine name of the specified host name or IP address.
        /// </summary>
        /// <param name="hostNameOrAddress"></param>
        /// <returns></returns>
        public static string GetMachineName(string hostNameOrAddress)
        {
            IPHostEntry hostEntryIP = Dns.GetHostEntry(hostNameOrAddress);
            string ret = hostEntryIP.HostName;
            
            string domain = IPGlobalProperties.GetIPGlobalProperties().DomainName;
            if (StrFunc.IsFilled(domain))
                ret = ret.Replace("." + domain, string.Empty);

            return ret;
        }
            
    }
}
