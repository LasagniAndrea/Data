using System;
using System.Runtime.InteropServices;
using System.Collections;

namespace NetworkManagement
{
    // EG 20180423 Analyse du code Correction [CA1060]
    internal static class UnsafeNativeMethods
    {
        [DllImport("netapi32.dll")]
        internal static extern uint NetServerEnum([MarshalAs(UnmanagedType.LPWStr)] string ServerName,
            // 20100406 MF - Use System.IntPtr to have a good pointer where the result is a long pointer
        uint level, /*uint* bufptr*/ out IntPtr bufptr, uint prefmaxlen, ref uint entriesread, ref uint totalentries,
            uint servertype, [MarshalAs(UnmanagedType.LPWStr)] string domain, UIntPtr resume_handle);
    }

	public enum ServerTypeEnum : uint
	{
		None					= 0,
		ALL						= 0xFFFFFFFF,
		WORKSTATION          	= 0x00000001,
		SERVER               	= 0x00000002,
		SQLSERVER            	= 0x00000004,
		DOMAIN_CTRL          	= 0x00000008,
		DOMAIN_BAKCTRL       	= 0x00000010,
		TIME_SOURCE          	= 0x00000020,
		AFP                  	= 0x00000040,
		NOVELL               	= 0x00000080,
		DOMAIN_MEMBER        	= 0x00000100,
		PRINTQ_SERVER        	= 0x00000200,
		DIALIN_SERVER        	= 0x00000400,
		XENIX_SERVER         	= 0x00000800,
		NT                   	= 0x00001000,
		WFW                  	= 0x00002000,
		SERVER_MFPN          	= 0x00004000,
		SERVER_NT            	= 0x00008000,
		POTENTIAL_BROWSER    	= 0x00010000,
		BACKUP_BROWSER       	= 0x00020000,
		MASTER_BROWSER       	= 0x00040000,
		DOMAIN_MASTER        	= 0x00080000,
		SERVER_OSF           	= 0x00100000,
		SERVER_VMS           	= 0x00200000,
		WINDOWS              	= 0x00400000,  /* Windows95 and above */
		DFS                  	= 0x00800000,  /* Root of a DFS tree */
		CLUSTER_NT           	= 0x01000000,  /* NT Cluster */
		DCE                  	= 0x10000000,  /* IBM DSS (Directory and Security Services) or equivalent */
		ALTERNATE_XPORT      	= 0x20000000,  /* return list for alternate transport */
		LOCAL_LIST_ONLY      	= 0x40000000,  /* Return local list only */
		DOMAIN_ENUM          	= 0x80000000
	}



	public class NetServers
	{
        #region Members

        [System.Runtime.InteropServices.StructLayoutAttribute(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        private struct SERVER_INFO_101
        {
            public int dwPlatformID;
            public System.IntPtr lpszServerName;
            public int dwVersionMajor;
            public int dwVersionMinor;
            public int dwType;
            // 20100406 MF - Use System.IntPtr (or string ) to have a good pointer where the result is a null-terminated string (sz)
            //public int lpszComment;
            public System.IntPtr lpszComment;
        }

		private static ServerTypeEnum m_ServerType;
        #endregion Members

		#region Constructors
		public NetServers():this(ServerTypeEnum.None){}
		public NetServers(ServerTypeEnum pServerType) {m_ServerType = pServerType;}
		#endregion Constructors

        #region Methods
        #region GetServers
        public static string[] GetServers()
		{
			return GetServers(null);
		}
        // EG 20180423 Analyse du code Correction [CA2101]
        // EG 20180423 Analyse du code Correction [CA1060]
        public static string[] GetServers(string domain)
        {
            string[] servers;
            string serverName = null;
            string domainName = null;
            uint level = 101;
            uint prefmaxlen = 0xFFFFFFFF;
            uint entriesread = 0;
            uint totalentries = 0;
            uint resume_handle = 0;

            try
            {
                unsafe
                {
                    //get a pointer to the server info structure
                    // 20100406 MF - Use System.IntPtr to have a good pointer where the result is a long pointer
                    // SERVER_INFO_101* si = null;
                    System.IntPtr si = new IntPtr();
                    //temp pointer for use when looping through returned (pointer) array
                    //SERVER_INFO_101* tmpPointer;
                    System.IntPtr tmpPointer;
                    //this api requires a pointer to a byte array...which is actually a pointer to an array
                    //of SERVER_INFO_101 structures
                    //uint result = NetServerEnum(serverName, level, (uint*)&si, prefmaxlen, ref entriesread, ref totalentries,
                    //    (uint)m_ServerType, domainName, resume_handle);
                    uint result = UnsafeNativeMethods.NetServerEnum(serverName, level, out si, prefmaxlen, ref entriesread, ref totalentries,
                        (uint)m_ServerType, domainName, new UIntPtr(resume_handle));

                    servers = new string[entriesread];

                    SERVER_INFO_101 server;

                    if (0 == result)
                    {
                        if (null != (tmpPointer = si)) //assign the temp pointer 
                        {
                            for (int i = 0; i < entriesread; i++)	//loop through the entries
                            {
                                try
                                {
                                    server = (SERVER_INFO_101)Marshal.PtrToStructure(tmpPointer, typeof(SERVER_INFO_101));
                                    servers[i] = Marshal.PtrToStringAuto(server.lpszServerName);
                                }
                                catch (Exception) { throw; }
                                // 20100406 MF - cast the pointer to a ulong so the "pointer addiction" will work either 32-bit and 64-bit systems (but it could work anyway)
                                //tmpPointer++;		//increment the pointer...essentially move to the next structure in the array
                                tmpPointer = (IntPtr)((ulong)tmpPointer + (ulong)Marshal.SizeOf(server));
                            }
                        }
                    }
                }
            }
            catch { throw; }
            return servers;
        }
		#endregion GetServers
        #endregion Methods
    }
}
