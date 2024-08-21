using EFS.ACommon;
using Microsoft.Win32;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Text;
using System.Threading;

namespace EFS.Common.Log
{
    // EG 20180423 Analyse du code Correction [CA1060]
    internal static class NativeMethods
    {
        #region Advapi32 Methods
        /// <summary>
        /// Saves the specified event log to a backup file. The function does not clear the event log.
        /// </summary>
        /// <param name="pHandle"></param>
        /// <param name="pBackupName"></param>
        /// <returns>
        /// <para>If the function succeeds, the return value is nonzero.</para>
        /// <para>If the function fails, the return value is zero. To get extended error information, call GetLastError.</para>
        /// </returns>
        // EG 20180423 Analyse du code Correction [CA2101]
        [DllImport("Advapi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern int BackupEventLog(IntPtr pHandle, [MarshalAs(UnmanagedType.LPWStr)]string pBackupName);
        /// <summary>
        /// Clears the specified event log, and optionally saves the current copy of the log to a backup file.
        /// </summary>
        /// <param name="pHandle"></param>
        /// <param name="pBackupName"></param>
        /// <returns>
        /// <para>If the function succeeds, the return value is nonzero</para>
        /// <para>f the function fails, the return value is zero. To get extended error information, call GetLastError. The ClearEventLog function can fail if the event log is empty or the backup file already exists.</para>
        /// </returns>
        // EG 20180423 Analyse du code Correction [CA2101]
        [DllImport("Advapi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        internal static extern int ClearEventLog(IntPtr pHandle, [MarshalAs(UnmanagedType.LPWStr)] string pBackupName);
        /// <summary>
        /// Closes the specified event log.
        /// </summary>
        /// <param name="pHandle"></param>
        /// <returns>
        /// <para>If the function succeeds, the return value is nonzero.</para>
        /// <para>If the function fails, the return value is zero. To get extended error information, call GetLastError.</para>
        /// </returns>
        [DllImport("Advapi32.dll", SetLastError = true)]
        internal static extern int CloseEventLog(IntPtr pHandle);
        /// <summary>
        /// Enables an application to receive notification when an event is written to the specified event log. When the event is written to the log, the specified event object is set to the signaled state.
        /// </summary>
        /// <param name="pHandle"></param>
        /// <param name="pEventHandle"></param>
        /// <returns>
        /// <para>If the function succeeds, the return value is nonzero.</para>
        /// <para>If the function fails, the return value is zero. To get extended error information, call GetLastError.</para>
        /// </returns>
        [DllImport("Advapi32.dll", SetLastError = true)]
        internal static extern int NotifyChangeEventLog(IntPtr pHandle, IntPtr pEventHandle);
        /// <summary>
        /// Opens a handle to the specified event log.
        /// </summary>
        /// <param name="pMachineName"></param>
        /// <param name="pSourceName"></param>
        /// <returns>
        /// <para>If the function succeeds, the return value is the handle to an event log.</para>
        /// <para>If the function fails, the return value is NULL. To get extended error information, call GetLastError.</para>
        /// </returns>
        // EG 20180423 Analyse du code Correction [CA2101]
        [DllImport("Advapi32.dll", SetLastError = true, CharSet=CharSet.Unicode)]
        internal static extern IntPtr OpenEventLog([MarshalAs(UnmanagedType.LPWStr)] string pMachineName, [MarshalAs(UnmanagedType.LPWStr)] string pSourceName);

        /// <summary>
        /// Retrieves a registered handle to the specified event log.
        /// <para>If the function succeeds, the return value is a handle to the event log.</para>
        /// <para>If the function fails, the return value is NULL. To get extended error information, call GetLastError.</para>
        /// </summary>
        /// <param name="pMachineName"></param>
        /// <param name="pSourceName"></param>
        /// <returns></returns>
        // EG 20180423 Analyse du code Correction [CA2101]
        [DllImport("Advapi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        internal static extern IntPtr RegisterEventSource([MarshalAs(UnmanagedType.LPWStr)] string pMachineName, [MarshalAs(UnmanagedType.LPWStr)] string pSourceName);
        /// <summary>
        /// Writes an entry at the end of the specified event log.
        /// <para>If the function succeeds, the return value is nonzero, indicating that the entry was written to the log.</para>
        /// <para>If the function fails, the return value is zero. To get extended error information, call GetLastError, which returns one of the following extended error codes.</para>
        /// </summary>
        /// <param name="pHandle"></param>
        /// <param name="pEventLogEntry"></param>
        /// <param name="pCategory"></param>
        /// <param name="pEventID"></param>
        /// <param name="pSid"></param>
        /// <param name="pNumStrings"></param>
        /// <param name="pDataSize"></param>
        /// <param name="pLpStrings"></param>
        /// <param name="pLpRawData"></param>
        /// <returns></returns>
        [DllImport("Advapi32.dll", SetLastError = true)]
        internal static extern int ReportEvent(IntPtr pHandle, ushort pEventLogEntry, ushort pCategory, uint pEventID, IntPtr pSid,
            ushort pNumStrings, uint pDataSize, string[] pLpStrings, byte[] pLpRawData);
        /// <summary>
        /// Closes the specified event log.
        /// <para>If the function succeeds, the return value is nonzero.</para>
        /// <para>If the function fails, the return value is zero. To get extended error information, call GetLastError.</para>
        /// </summary>
        /// <param name="pHandle"></param>
        /// <returns></returns>
        [DllImport("Advapi32.dll", SetLastError = true)]
        internal static extern int DeregisterEventSource(IntPtr pHandle);

        /// <summary>
        /// The LookupAccountName function accepts the name of a system and an account as input. It retrieves a security identifier (SID) for the account and the name of the domain on which the account was found.
        /// </summary>
        /// <param name="pSystemName"></param>
        /// <param name="pAccountName"></param>
        /// <param name="pSid"></param>
        /// <param name="pSidSize"></param>
        /// <param name="pDomainName"></param>
        /// <param name="pDomainSize"></param>
        /// <param name="pNameUse"></param>
        /// <returns>
        /// <para> if the function succeeds, the function returns nonzero.</para>
        /// <para>if the function fails, it returns zero. For extended error information, call GetLastError.</para>
        /// </returns>
        // EG 20180423 Analyse du code Correction [CA2101]
        [DllImport("Advapi32.dll", SetLastError = true, CharSet=CharSet.Unicode, PreserveSig=true)]
        internal static extern int LookupAccountName([MarshalAs(UnmanagedType.LPWStr)] string pSystemName,
            [MarshalAs(UnmanagedType.LPWStr)] string pAccountName, IntPtr pSid, ref int pSidSize,
            [MarshalAs(UnmanagedType.LPWStr)] StringBuilder pDomainName, ref int pDomainSize, ref int pNameUse);

        #endregion Advapi32 Methods
    }

	#region Enums EventLog_Categories
	public enum EventLog_Categories
	{
		Success     = 0x1,
		Information = 0x2,
		Warning     = 0x3,
		Error       = 0x4,
	}
	#endregion Enums EventLog_Categories

	#region Enums EventLog_EventIds
	public enum EventLog_EventId
	{
		//SpheresWeb                        = 1000,
        SpheresServices_Info_Business       = 1000,

        SpheresServices_Info_System         = 1500,
        SpheresServices_Info_Start          = 1501,
        SpheresServices_Info_Initialize     = 1502,
        SpheresServices_Info_Connected      = 1503,
        SpheresServices_Info_Continue       = 1505,
        SpheresServices_Info_Pause          = 1508,
        SpheresServices_Info_Stop           = 1509,

        SpheresServices_Error_Business      = 2000,

        SpheresServices_Error_System        = 2500,
        SpheresServices_Error_MOMPath       = 2501,
        SpheresServices_Error_MOMInitialize = 2502,
        SpheresServices_Error_MOMUndefined  = 2509,
        SpheresServices_Error_ObjRefNotSet  = 2530,
        SpheresServices_Error_SQL           = 2550,
        SpheresServices_Error_SQLTimeout    = 2551,
        SpheresServices_Error_SQLDeadlock   = 2552,
        SpheresServices_Error_SQLConnection = 2553,
		SpheresServices_Error_CMECore       = 2570,
		SpheresServices_Error_Undefined     = 2599,

        SpheresServices_Warning_Business    = 3000,
        
        SpheresServices_Warning_System      = 3500,
	}
	#endregion Enums EventLog_EventId 

	#region Interface IRegistryPathProvider
	/// <summary>
	/// Simple interface that allows the EventLogRegistry to "callback" to the
	/// specified class and get the path that needs to be appended to the root
	/// when accessing the registry
	/// </summary>
	internal interface  IRegistryPathProvider
	{
		string RegistryPath {get;}
	}
	#endregion Interface IRegistryPathProvider

    #region PInvokeFailure
    /// <summary>
    /// Check pInvoke Failure using Marshal.GetLastWin32Error();
    /// </summary>
    internal class PInvokeFailure
    {
        #region Members
        /// <summary>
        /// System API GetLastError() values 
        /// </summary>
        /// <remarks>
        /// There are a lot of these so just add them as required!
        /// </remarks>
        public enum ErrorCodes
        {
            ERROR_SUCCESS = 0,
            ERROR_INSUFFICIENT_BUFFER = 122,
            ERROR_IO_PENDING = 997
        }
        #endregion Members
        #region Constructors
        private PInvokeFailure() { }
        #endregion Constructors
        #region Methods
        #region Check
        /// <summary>
        /// Check to see if a P/Invoke call failed for any reason
        /// </summary>
        /// <param name="pRc">The return code from the P/Invoke call</param>
        public static void Check()
        {
            Check(null);
        }
        #endregion Check
        #region Check
        /// <summary>
        /// Check to see if a P/Invoke call failed, ignoring an single expected error
        /// </summary>
        /// <param name="pExpectedError">The allowed failure error code</param>
        public static void Check(ErrorCodes pExpectedError)
        {
            Check(new ErrorCodes[] { pExpectedError });
        }
        /// <summary>
        /// Check to see if a P/Invoke call failed, ignoring expected errors
        /// </summary>
        /// <param name="pExpectedErrors">An array of allowed failure error codes</param>
        static public void Check(ErrorCodes[] pExpectedErrors)
        {
            //
            // NOTE: Some API's (e.g. LookupAccountName) don't set the last 
            // error to ERROR_SUCCESS when they work, so you can't use that 
            // as a reliable failure test -- you can only rely on the "BOOL" 
            // return code (0 == failed; anything else = success) and then 
            // check the GetLastError() value with a "true" failure

            // ... and see if it is a failure
            ErrorCodes lastError = (ErrorCodes)Marshal.GetLastWin32Error();

            bool isThrowException = true;
            // See if this is expected 
            if (null != pExpectedErrors)
            {
                foreach (ErrorCodes err in pExpectedErrors)
                {
                    // Get the last Win32 API error...

                    if (err == lastError)
                    {
                        isThrowException = false;
                        break;
                    }
                }
            }
            // If not, throw the exception
            if (isThrowException)
            {
#if THROW_HR_EXCEPTIONS
						Marshal.ThrowExceptionForHR( Marshal.GetHRForLastWin32Error() );
#else
                throw new Win32Exception((int)lastError);
#endif
            }

        }
        #endregion CheckFailedNot
        #endregion Methods
    }
    #endregion PInvokeFailure

    #region EventLogContent
    public class EventLogCharateristics
    {
        #region Constructors
        public EventLogCharateristics(string pHostName, string pEventLog, string pSource, EventLogEntryType pLevel, EventLog_EventId pEventId, string[] pData)
        {
            HostName = pHostName;
            IsLocalHost = true;
            EventLog = pEventLog;
            Source = pSource;

            Level = pLevel;
            EventId = pEventId;
            Data = pData;
        }
        public EventLogCharateristics(string pHostName, string pEventLog, string pSource, ProcessStateTools.StatusEnum pStatus, Cst.ErrLevel pReturnCode, EventLog_EventId pEventId)
        {
            HostName = pHostName;
            IsLocalHost = true; 
            EventLog = pEventLog;
            Source = pSource;

            #region Level: Déduction sur la base de EventId et le cas échéant de Status
            // Level: Error,Warning,Information,SuccessAudit or FailureAudit
            if ((pEventId == EventLog_EventId.SpheresServices_Error_System) || (pEventId == EventLog_EventId.SpheresServices_Error_Business))
            {
                Level = EventLogEntryType.Error;
            }
            else if (ProcessStateTools.IsStatusError(pStatus) || ProcessStateTools.IsStatusUnknown(pStatus))
            {
                Level = EventLogEntryType.Error;
            }
            else if (ProcessStateTools.IsStatusWarning(pStatus)) 
            {
                Level = EventLogEntryType.Warning;
            }
            else if (pEventId == EventLog_EventId.SpheresServices_Warning_Business)
            {
                Level = EventLogEntryType.Warning;
            }
            #endregion

            #region EventId: Si Undefined, déduction sur la base du ReturnCode
            m_ReturnCode = pReturnCode;

            if (pEventId != EventLog_EventId.SpheresServices_Error_Undefined)
            {
                EventId = pEventId;                    
            }
            else
            {
                switch (m_ReturnCode)
                {
                    case Cst.ErrLevel.SUCCESS:
                        EventId = EventLog_EventId.SpheresServices_Info_Business;
                        break;
                    case Cst.ErrLevel.EXECUTED:
                    case Cst.ErrLevel.STARTED:
                    case Cst.ErrLevel.CONNECTED:
                    case Cst.ErrLevel.INITIALIZE:
                        //Other Negative ReturnCode
                        EventId = EventLog_EventId.SpheresServices_Info_System;
                        break;
                    case Cst.ErrLevel.DATADISABLED:
                    case Cst.ErrLevel.NOBOOKMANAGED:
                    case Cst.ErrLevel.NOTHINGTODO:
                    case Cst.ErrLevel.QUOTENOTFOUND:
                    case Cst.ErrLevel.QUOTEDISABLED:
                    case Cst.ErrLevel.MULTIQUOTEFOUND:
                        //Business ReturnCode
                        EventId = EventLog_EventId.SpheresServices_Error_Business;
                        break;
                    default:
                        EventId = EventLog_EventId.SpheresServices_Error_System;
                        break;
                }
            }
            #endregion
        }
        #endregion Constructors
        #region Members
        private readonly Cst.ErrLevel m_ReturnCode;

        private string m_HostName = null;
        private bool m_IsLocalHost = true;
        private string m_EventLog = null;
        private EventLogEntryType m_Level = EventLogEntryType.Information;
        private string m_Source = null;
        private EventLog_EventId m_EventId = EventLog_EventId.SpheresServices_Error_Undefined;
        private EventLog_Categories m_TaskCategory;
        private bool m_TaskCategorySpecified = false;
        private string[] m_Data = null;
        #endregion Members
        #region Accessors
        public string HostName
        {
            get { return m_HostName; }
            set { m_HostName = value; }
        }
        public bool IsLocalHost
        {
            get { return m_IsLocalHost; }
            set { m_IsLocalHost = value; }
        }
        public string EventLog
        {
            get { return m_EventLog; }
            set { m_EventLog = value; }
        }
        public EventLogEntryType Level
        {
            get { return m_Level; }
            set { m_Level = value; }
        }
        public string Source
        {
            get { return m_Source; }
            set { m_Source = value; }
        }
        public EventLog_EventId EventId
        {
            get { return m_EventId; }
            set { m_EventId = value; }
        }
        public EventLog_Categories TaskCategory
        {
            get
            {
                if (m_TaskCategorySpecified)
                {
                    return m_TaskCategory;
                }
                else
                {
                    #region TaskCategory: Déduction sur la base du Level
                    if (EventLogEntryType.Information == Level)
                        return EventLog_Categories.Information;
                    else if (EventLogEntryType.Warning == Level)
                        return EventLog_Categories.Warning;
                    else if (EventLogEntryType.Error == Level)
                        return EventLog_Categories.Error;
                    else
                        return EventLog_Categories.Success;
                    #endregion
                }
            }
            set
            {
                m_TaskCategorySpecified = true;
                m_TaskCategory = value;
            }
        }
        public string[] Data
        {
            get { return m_Data; }
            set { m_Data = value; }
        }
        #endregion Accessors
    }
	#endregion EventLogContent

	#region EventLogEx
	/// <summary>
	/// 
	/// </summary>
	/// FI 20240624 [WI981] Refactoring
	public class EventLogEx : IDisposable
	{
		#region Members
		private readonly EventLogCharateristics m_EventLogCharateristics;
		private readonly string m_MachineName;
		private readonly Boolean m_IsLocaleMachine;
		private readonly string m_EventLogName;
		private readonly EventSource m_EventLogSource;
			


		private IntPtr m_HandleEventLog;
		private Thread m_Thread;
		private ManualResetEvent m_ManualResetEvent;
		private bool m_IsDisposed;
		private bool m_IsRaiseEvents;

		private const bool DEFAULT_RESTRICT = true;
		private const int DEFAULT_MAXSIZE = 0x10000;
		private const int DEFAULT_RETENTION = 0x093A80;

		public EntryWrittenEventHandler EntryWritten;


		#endregion Members
		#region Accessors
		private bool IsLocaleMachineName
		{
			get { return (m_MachineName == ".") || m_IsLocaleMachine; }
		}


		#region DisplayNameFile
		/// <summary>
		/// Get or set the path to the file that contains the friendly name for this log (as 
		/// displayed in the Event Viewer)
		/// </summary>
		/// <value>A full path to the resource file</value>
		/// <remarks>If this value is not specified, the name of the event log will be used
		/// <p/>Due to the restriction that you cannot create REG_SZ_EXPAND strings using
		/// the .NET framework <see cref="RegistryKey"/> classes, this path cannot include
		/// '%' escaped environment variables </remarks>
		public string DisplayNameFile
		{
			get { return (string)EventLogRegistry.Read("DisplayNameFile", LogName); }
			set { EventLogRegistry.Write("DisplayNameFile", value); }
		}
		#endregion DisplayNameFile
		#region DisplayNameId
		/// <summary>
		/// Get or set the identifier of the friendly name for this log (as 
		/// displayed in the Event Viewer) within the resource file specified by the 
		/// <see cref="DisplayNameFile"/> property 
		/// </summary>
		/// <value>The message identifier</value>
		/// <remarks>If this value is not specified, the name of the event log will be used</remarks>
		public int DisplayNameId
		{
			get { return (int)EventLogRegistry.Read("DisplayNameID", 0); }
			set { EventLogRegistry.Write("DisplayNameID", value); }
		}
		#endregion DisplayNameId
		#region EnableRaisingEvents

		/// <summary>
		/// 
		/// </summary>
		/// EG 20160404 Migration vs2013
		/// FI 20161021 [XXXXX] Modify 
		// EG 20180423 Analyse du code Correction [CA1060]
		public bool EnableRaisingEvents
		{
			get { return m_IsRaiseEvents; }
			set
			{
				//if (0 != m_Source.MachineName.CompareTo("."))
				if (false == IsLocaleMachineName)
					throw new ApplicationException("Events can only be raised on a local source");

				if (value != m_IsRaiseEvents)
				{
					lock (this)
					{
						if (((m_IsRaiseEvents = value) == true) && (null == m_ManualResetEvent))
						{
							Open();
							m_ManualResetEvent = new ManualResetEvent(false);

							// EG 20160404 Migration vs2013
							//ApiCall.CheckFailed(PInvoke.NotifyChangeEventLog(m_Handle, m_ManualResetEvent.Handle));
							int rc = NativeMethods.NotifyChangeEventLog(m_HandleEventLog, m_ManualResetEvent.SafeWaitHandle.DangerousGetHandle());
							if (rc == 0)
								PInvokeFailure.Check();
							m_Thread = new Thread(new ThreadStart(this.RunEventThread))
							{
								IsBackground = true
							};
							m_Thread.Start();
						}
					}
				}
			}
		}
		#endregion EnableRaisingEvents
		#region EventLogRegistry
		private EventLogRegistry EventLogRegistry
		{
			get
			{
				if (null == m_EventLogSource)
					throw new ApplicationException("Event source has not been set");

				return m_EventLogSource.EventLogRegistry;
			}
		}
		#endregion EventLogRegistry
		#region Exists
		/// <summary>
		/// Determines whether the associated log exists
		/// </summary>
		/// <value>A boolean that indicates if the logfile name is registered</value>
		public bool Exists
		{
			get
			{
				if (IsStandardLog)
					return (null != FileName);
				else
					return (null != EventLogRegistry);
			}
		}
		#endregion Exists
		#region FileName
		/// <summary>
		/// The full path to the physical file used to hold events for the log
		/// </summary>
		/// <value>A full path specification</value>
		/// <remarks>Due to the restriction that you cannot create REG_SZ_EXPAND strings using
		/// the .NET framework <see cref="RegistryKey"/> classes, this path cannot include
		/// '%' escaped environment variables 
		/// <p/>
		/// The filename for the <c>Application</c>, <c>System</c> or <c>Security</c>
		/// event logs cannot be changed using this property
		/// </remarks>
		public string FileName
		{
			get { return (string)EventLogRegistry.Read("File", null); }
			set
			{
				if (false == IsStandardLog)
					EventLogRegistry.Write("File", value);
			}
		}
		#endregion FileName
		#region IsStandardLog
		/// <summary>
		/// Private helper that checks if the current log is a system
		/// supplied one (Application, Security or System)
		/// </summary>
		private bool IsStandardLog
		{
			get
			{
				return ((0 == string.Compare(LogName, "Application", true)) ||
					(0 == string.Compare(LogName, "Security", true)) ||
					(0 == string.Compare(LogName, "System", true)));
			}
		}
		#endregion IsStandardLog
		#region LogName
		public string LogName { get { return m_EventLogName; } }
		#endregion LogName
		#region MaxSize
		/// <summary>
		/// Get or set the maximum allowed size of a log before the 
		/// overwritting of events occurs
		/// </summary>
		/// <value>
		/// The maximum allowed size of the log in bytes
		/// </value>
		/// <remarks>
		/// The default (and most common value) for this property is 64K
		/// </remarks>
		/// 
		public int MaxSize
		{
			get { return (int)EventLogRegistry.Read("MaxSize", DEFAULT_MAXSIZE); }
			set { EventLogRegistry.Write("MaxSize", value); }
		}
		#endregion MaxSize
		#region PrimaryModule
		/// <summary>
		/// Get the primary source for this event log
		/// </summary>
		/// <value>The name of the primary source</value>
		/// <remarks>This is usually the name of the first source registered with an event log</remarks>
		public string PrimaryModule
		{
			get { return (string)EventLogRegistry.Read("PrimaryModule", null); }
		}
		#endregion PrimaryModule
		
		#region Retention
		/// <summary>
		/// Get or set the logfile record retention period
		/// </summary>
		/// <value>The logfile retention period in seconds</value>
		/// <remarks>
		/// The most common values for this are 0 (never overwrite) and 0x93A80 (7 days)
		/// </remarks>
		public int Retention
		{
			get { return (int)EventLogRegistry.Read("Retention", DEFAULT_RETENTION); }
			set { EventLogRegistry.Write("Retention", value); }
		}
		#endregion Retention
		#region Restricted
		/// <summary>
		/// Get or set the flag that indicates if the Guest account is allowed access
		/// to the associated logfile
		/// </summary>
		/// <value>
		/// A boolean that indicates if access is allowed (true) or denied (false)
		/// </value>
		public bool Restricted
		{
			get { return (bool)EventLogRegistry.Read("RestrictGuestAccess", DEFAULT_RESTRICT); }
			set { EventLogRegistry.Write("RestrictGuestAccess", value); }
		}
		#endregion Restricted

		
		#endregion Accessors
		#region Constructors
		/// <summary>
		/// 
		/// </summary>
		/// <param name="pELC"></param>
		public EventLogEx(EventLogCharateristics pELC)
		{
			m_EventLogCharateristics = pELC;
			
			m_MachineName = pELC.HostName;
			m_IsLocaleMachine = pELC.IsLocalHost;
			m_EventLogName = pELC.EventLog;
			
			m_HandleEventLog = IntPtr.Zero;

			// FI 20240624 [WI981] Si la source n'existe pas => création de la source
			m_EventLogSource = new EventSource(pELC.Source, pELC.HostName, pELC.EventLog);
			if (false == m_EventLogSource.Exists)
			{
				m_EventLogSource.Close();
				m_EventLogSource.LoadUserSID();
				m_EventLogSource.NewEventLogRegistry();
				m_EventLogSource.RegisterSource();
				m_EventLogSource = new EventSource(pELC.Source, pELC.HostName, pELC.EventLog);
			}

		}
		#endregion Constructors
		#region Destructors
		~EventLogEx() { Dispose(false); }
		#endregion Destructors
		#region Methods
		#region Backup
		/// <summary>
		/// Backup the event log to the specified file
		/// </summary>
		/// <param name="backupName">The name of the backup file</param>
		/// <remarks>Caller must be a member of the BUILTIN\Administrators group</remarks>
		/// FI 20161021 [XXXXX] Modify 
		[PrincipalPermission(SecurityAction.Demand, Role = @"BUILTIN\Administrators")]
		// EG 20180423 Analyse du code Correction [CA1060]
		public void Backup(string pBackupName)
		{
			ValidateHandle();

			int rc = NativeMethods.BackupEventLog(m_HandleEventLog, pBackupName);

			if (rc == 0)// FI 20161021 [XXXXX] add If
				PInvokeFailure.Check();
		}
		#endregion Backup
		#region Clear
		/// <summary>
		/// Clear the event log without backing it up
		/// </summary>
		public void Clear()
		{
			Clear(null);
		}
		/// <summary>
		/// Clear the log, backing it up if required
		/// </summary>
		/// <param name="backupName">The name of the backup file</param>
		/// <remarks>Caller must be a member of the BUILTIN\Administrators group</remarks>
		/// FI 20161021 [XXXXX] Modify
		[PrincipalPermission(SecurityAction.Demand, Role = @"BUILTIN\Administrators")]
		// EG 20180423 Analyse du code Correction [CA1060]
		public void Clear(string pBackupName)
		{
			ValidateHandle();
			int rc = NativeMethods.ClearEventLog(m_HandleEventLog, pBackupName);
			// FI 20161021 [XXXXX] add if
			if (rc == 0)
				PInvokeFailure.Check();
		}
		#endregion Clear
		#region Close
		/// <summary>
		/// Close the event log (more natural than Dispose())
		/// </summary>
		public void Close() { Dispose(); }
		#endregion Close
		#region Delete
		/// <summary>
		/// Delete the log from the registry (the physical file is left as it is
		/// probably locked by the system)
		/// </summary>
		/// <remarks>Caller must be a member of the BUILTIN\Administrators group
		/// <p/>
		/// The entries for the <c>Application</c>, <c>System</c> or <c>Security</c>
		/// event logs cannot be deleted
		/// </remarks>
		/// 
		[PrincipalPermission(SecurityAction.Demand, Role = @"BUILTIN\Administrators")]
		public void Delete()
		{
			// Make sure that we are not trying to delete a standard log
			if (IsStandardLog)
			{
				throw new InvalidOperationException("You cannot delete a system provided log");
			}
			EventLogRegistry.Delete();
			Close();
		}
		#endregion Delete
		#region Dispose
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}
		// EG 20180423 Analyse du code Correction [CA1060]
		// EG 20180423 Analyse du code Correction [CA1063]
		protected virtual void Dispose(bool pIsDisposing)
		{
			if (false == m_IsDisposed)
			{
				try
				{
					if (pIsDisposing)
					{
						if (null != m_EventLogSource)
							m_EventLogSource.Dispose();

						if (null != m_ManualResetEvent)
							m_ManualResetEvent.Dispose();
					}

					// Release unmanaged resources
					if (IntPtr.Zero != m_HandleEventLog)
					{
						NativeMethods.CloseEventLog(m_HandleEventLog);
						m_HandleEventLog = IntPtr.Zero;
					}

					// Stop the event listener thread
					try
					{
						if (m_Thread != null)
							m_Thread.Abort();
					}
					catch { m_Thread = null; }

				}
				finally { m_IsDisposed = true; }
			}
		}

		#endregion Dispose
		#region GetSources
		/// <summary>
		/// Creates an array of the registered sources for the associated logfile
		/// </summary>
		/// <value>An array names of the registered event sources</value>
		public string[] GetSources()
		{
			return (string[])EventLogRegistry.Read("Sources", null);
		}
		#endregion GetSources

		#region Open
		/// <summary>
		/// Open the log on the machine associated with our current source without clearing it
		/// </summary>
		public void Open()
		{
			Open(false);
		}
		/// <summary>
		/// Open the log on the machine associated with our current source
		/// </summary>
		/// <param name="pIsClearOnSuccess">If true, clear the log if it can be opened</param>
		// EG 20180423 Analyse du code Correction [CA1060]
		public void Open(bool pIsClearOnSuccess)
		{
			if (IntPtr.Zero == m_HandleEventLog)
			{
				// TBD: Add "isBackup" flag and use PInvoke.OpenBackupEventLog()
				//      if it is set?
				m_HandleEventLog = NativeMethods.OpenEventLog(m_MachineName, m_EventLogName);
				// FI 20161021 [XXXXX] suspicion de valeurs retours > Int32.MaxValue (Machine sur OS 64 bits) => l'appel à ApiCall.CheckFailed est effectué uniquement si Error (eq 0). 
				//Rq : summary dit si erreur retourne la valeur null (Ce n'est pas constaté par mes tests ou l'on récupère IntPtr.Zero )  
				if ((m_HandleEventLog == null) || (m_HandleEventLog == IntPtr.Zero))
					PInvokeFailure.Check();

				if (pIsClearOnSuccess)
					Clear();
			}
		}
		#endregion Open
		#region RunEventThread
		private void RunEventThread()
		{
			try
			{
				while (true)
				{
					m_ManualResetEvent.WaitOne();

					if (m_IsRaiseEvents && (null != EntryWritten))
					{
						EntryWritten(this, (EntryWrittenEventArgs)EntryWrittenEventArgs.Empty);
					}
				}
			}
			catch { }
		}
		#endregion RunEventThread
		#region ValidateHandle
		/// <summary>
		/// Helper to validate the handle and throw if it is null
		/// </summary>
		private void ValidateHandle()
		{
			if (IntPtr.Zero == m_HandleEventLog)
				throw new ApplicationException("Event log has not been opened");
		}
		#endregion ValidateHandle

		#region OpenAndWrite
		/// <summary>
		/// Open the log on the machine associated with our current source without clearing it, then write.
		/// </summary>
		// EG 20180423 Analyse du code Correction [CA2200]
		public void ReportEvent()
		{
            if (m_EventLogSource.Exists)
            {
                byte[] rawData = null;
                m_EventLogSource.ReportEvent(m_EventLogCharateristics.Level, (uint)m_EventLogCharateristics.TaskCategory, (uint)m_EventLogCharateristics.EventId,
                    m_EventLogCharateristics.Data, rawData);
            }
        }
		#endregion OpenAndWrite
		#endregion Methods
	}
	#endregion EventLogEx
	#region EventLogRegistry
	/// <summary>
	/// Registry helper functions
	/// </summary>
	/// FI 0210629 [XXXXX] add IDisposable 
	internal class EventLogRegistry  : IDisposable
	{
		#region Members
		/// <summary>Well-known string for the root of the Eventlog entries</summary>
		const string LogRootRegistryPath = @"SYSTEM\CurrentControlSet\Services\EventLog\";

        /// <summary>Well-known string for the root of the system path entry</summary>
        //static private readonly string SystemRootPath = @"SOFTWARE\Microsoft\Windows NT\CurrentVersion";
        /// <summary>Instance root path</summary>
        readonly string m_RegPath;

        /// <summary>Active registry hive</summary>
        readonly RegistryKey m_Hive;

        private bool disposedValue;
        #endregion Members
        #region Accessors
        #endregion Accessors
        #region Constructors
        /// <summary>
        /// Open the HKLM hive on the specified machine
        /// </summary>
        /// <param name="provider">The RegistryPath provider</param>
        /// <param name="machineName">The machine to open the hive on (Use null or "." for the local machine)</param>
        public EventLogRegistry(IRegistryPathProvider pProvider,string pMachineName)
		{
			m_RegPath = LogRootRegistryPath + pProvider.RegistryPath;
			m_Hive = RegistryKey.OpenRemoteBaseKey(RegistryHive.LocalMachine, pMachineName ?? ".");
		}
		#endregion Constructors
		#region Methods
		#region Close
		/// <summary>
		/// Close the registry hive
		/// </summary>
		public void Close()
		{
			if (null != m_Hive)
				m_Hive.Close();
		}
		#endregion Close
		#region Delete
		/// <summary>
		/// Delete the current <see cref="m_RegPath"/>path from the hive
		/// </summary>
		public void Delete()
		{
			Delete(string.Empty);
		}
		/// <summary>
		/// Delete the specified subkey from the hive
		/// </summary>
		/// <param name="keyName">The subkey (from the current path) to delete</param>
		public void Delete(string pKeyName)
		{
			if (StrFunc.IsEmpty(pKeyName))
				m_Hive.DeleteSubKeyTree(m_RegPath);
			else
				m_Hive.DeleteSubKey(m_RegPath + @"\" + pKeyName,false);
			m_Hive.Flush();
		}
		#endregion Delete
		#region Read
		/// <summary>
		/// Read a value from the hive
		/// </summary>
		/// <param name="keyName">The key name of the value to be read</param>
		/// <param name="defaultValue">The default value returned if not found or on error</param>
		/// <returns></returns>
		public object Read(string pKeyName, object pDefaultValue)
		{
			return RegistryEx.ReadFromHive(m_Hive, m_RegPath, pKeyName, pDefaultValue);
		}
		#endregion Read
		#region Write
		/// <summary>
		/// Write a value to the hive and then flush the changes
		/// </summary>
		/// <param name="keyName">The key name of the value to be written</param>
		/// <param name="keyValue">The value to be written</param>
		public void Write(string pKeyName, object pKeyValue)
		{
			Write(pKeyName, pKeyValue, true);
		}
		/// <summary>
		/// Write a value to the hive, optionally flushing the changes on success
		/// </summary>
		/// <param name="keyName">The key name of the value to be written</param>
		/// <param name="keyValue">The value to be written</param>
		/// <param name="flushOnSuccess">If true, flush the hive on success</param>
		public void Write(string pKeyName,object pKeyValue,bool pIsFlushOnSuccess)
		{
			RegistryEx.WriteToHive(m_Hive,m_RegPath,pKeyName,pKeyValue);
			if (pIsFlushOnSuccess)
				m_Hive.Flush();
		}
		/// <summary>
		/// 
		/// </summary>
		/// <param name="disposing"></param>
		/// FI 20210629 [XXXXX] Add
		protected virtual void Dispose(bool disposing)
		{
			if (!disposedValue)
			{
				if (disposing)
				{
					// TODO: supprimer l'état managé (objets managés)
					if (null != m_Hive)
						m_Hive.Dispose();
				}

				// TODO: libérer les ressources non managées (objets non managés) et substituer le finaliseur
				// TODO: affecter aux grands champs une valeur null
				disposedValue = true;
			}
		}

        // // TODO: substituer le finaliseur uniquement si 'Dispose(bool disposing)' a du code pour libérer les ressources non managées
        // ~EventLogRegistry()
        // {
        //     // Ne changez pas ce code. Placez le code de nettoyage dans la méthode 'Dispose(bool disposing)'
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // Ne changez pas ce code. Placez le code de nettoyage dans la méthode 'Dispose(bool disposing)'
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
        #endregion Write
        #endregion Methods
    }
	#endregion EventLogRegistry
	#region EventSource
	public class EventSource : IDisposable, IRegistryPathProvider
	{
		#region Members
		readonly string m_MachineName;
		readonly string m_LogName;
		readonly string m_SourceName;

		private IntPtr m_HandleEventSource;
		private IntPtr m_UserSid;
		bool m_IsDisposed;
		#endregion Members

		#region Accessors
		/// <summary>
		/// 
		/// </summary>
		internal EventLogRegistry EventLogRegistry
		{
			get;
			private set;
		}

		/// <summary>
		/// Get or set the number of categories that are supported by this source
		/// </summary>
		/// <value>A count of the supported catgories</value>
		public int CategoryCount
		{
			get { return (int)EventLogRegistry.Read("CategoryCount", 0); }
			set { EventLogRegistry.Write("CategoryCount", value); }
		}
		
		/// <summary>
		/// Determines whether the associated source exists
		/// </summary>
		/// <value>A boolean value that indicates if the source has 
		/// been registered with the associated registry hive</value>
		public bool Exists
		{
			get
			{
				bool isExists = false;
				try { isExists = (EventLogRegistry.Read("TypesSupported", null) != null); }
				catch { }
				return isExists;
			}
		}
		
		
		/// <summary>
		/// 
		/// </summary>
		string IRegistryPathProvider.RegistryPath { get { return this.m_LogName + @"\" + this.m_SourceName; } }
		
		

		
		
		/// <summary>
		/// Get or set the types that are supported by this source
		/// </summary>
		/// <value>One or more of the standard <see cref="EventLogEntryType"/> 
		/// enumeration values logically ORed together</value>
		public int TypesSupported
		{
			get { return (int)EventLogRegistry.Read("TypesSupported", 0); }
			set { EventLogRegistry.Write("TypesSupported", value); }
		}
		#endregion Accessors

		#region Constructors
		/// <summary>
		/// Internal constructor
		/// </summary>
		/// <param name="source">The event source name</param>
		/// <param name="machineName">The machine that this source is to be registered on</param>
		/// <param name="logName">The name of the log that this source is to be associated with</param>
		/// FI 20160323 [XXXXX] Modify
		/// FI 20161021 [XXXXX] Modify 
		// EG 20180423 Analyse du code Correction [CA1060]
		// EG 20180423 Analyse du code Correction [CA1404]
		internal EventSource(string pSourceName, string pMachineName, string pLogName)
		{
			m_MachineName = pMachineName;
			m_LogName = pLogName;
			m_SourceName = pSourceName;

			LoadUserSID();

			NewEventLogRegistry();

			OpenEventSource();
		}
		#endregion Constructors
		#region Destructors
		/// <summary>
		/// Dispose of the instance
		/// </summary>
		~EventSource() { Dispose(false); }
		#endregion Destructors

		#region Methods
		/// <summary>
		/// 
		/// </summary>
		public void LoadUserSID()
		{
			// Get the users SID so that events can be logged with their 
			// user id (rather than just "Unknown")
			m_UserSid = IntPtr.Zero;
			try
			{

				int nameUse = 0;
				int sidSize = 0;
				int domainSize = 0;
				string userName = Environment.UserName;

				int rc = NativeMethods.LookupAccountName(null, userName, IntPtr.Zero, ref sidSize, null, ref domainSize, ref nameUse);
				if (rc == 0)
					PInvokeFailure.Check(PInvokeFailure.ErrorCodes.ERROR_INSUFFICIENT_BUFFER);


				m_UserSid = Marshal.AllocHGlobal((sidSize += 1));

				StringBuilder sbDomain = new StringBuilder((domainSize += 2));

				rc = NativeMethods.LookupAccountName(null, userName, m_UserSid, ref sidSize, sbDomain, ref domainSize, ref nameUse);
				// FI 20161021 [XXXXX] add if
				if (rc == 0)
					PInvokeFailure.Check();

			}
			catch (Win32Exception w32Ex) { throw new ApplicationException("Unable to obtain user SID", w32Ex); }
			catch (Exception ex)
			{
				int apiError = Marshal.GetLastWin32Error();
				string apiErrorText = apiError.ToString() + " (" + apiError.ToString("X8") + ")";
				throw new ApplicationException("Unable to obtain user SID: " + apiErrorText, ex);
			}
		}

		/// <summary>
		/// 
		/// </summary>
		private void OpenEventSource()
		{

			// Register as a source with NT (i.e. get a write handle)
			try
			{
				m_HandleEventSource = IntPtr.Zero;
				
				//FI 20160323 [XXXXX] 
				//sourceName must not contain characters prohibited in XML Attributes, with the exception of XML Escape sequences such as &lt &gl. (https://msdn.microsoft.com/en-us/library/aa363678.aspx)
				string sourceName = m_SourceName.Replace("&", "&amp;");
				m_HandleEventSource = NativeMethods.RegisterEventSource(m_MachineName, sourceName);
				// FI 20161021 [XXXXX] suspicion de valeurs retours > Int32.MaxValue (Machine sur OS 64 bits) => l'appel à ApiCall.CheckFailed est effectué uniquement si Error (eq 0). 
				//Rq : summary dit si erreur retourne la valeur null (Ce n'est pas constaté par mes test ou l'on récupère IntPtr.Zero )  
				if ((m_HandleEventSource == null) || (m_HandleEventSource == IntPtr.Zero))
					PInvokeFailure.Check();

			}
			catch (Exception ex) { throw new ApplicationException("Unable to register event source", ex); }
		}


		/// <summary>
		/// Dispose of the source and suppress any further finalisation
		/// </summary>
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}
		/// <summary>
		/// Dispose of the instance, optionally also disposing any managed resources
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed of,
		/// false otherwise</param>
		// EG 20180423 Analyse du code Correction [CA1060]
		// EG 20180423 Analyse du code Correction [CA1063]
		protected virtual void Dispose(bool pIsDisposing)
		{
			if (false == m_IsDisposed)
			{
				try
				{
					if (pIsDisposing)
					{
						// Release managed resources

						if (null != EventLogRegistry)
							EventLogRegistry.Dispose();
					}

					// Release unmanaged resources
					if (IntPtr.Zero != m_UserSid)
					{
						Marshal.FreeHGlobal(m_UserSid);
						m_UserSid = IntPtr.Zero;
					}

					if (IntPtr.Zero != m_HandleEventSource)
					{
						NativeMethods.DeregisterEventSource(m_HandleEventSource);
						m_HandleEventSource = IntPtr.Zero;
					}
				}
				finally { m_IsDisposed = true; }
			}
		}
		/// <summary>
		/// Enregistrement de la source dans la registry
		/// </summary>
		public void RegisterSource()
		{
			if (false == Exists)
			{
				CategoryCount = 1;
				TypesSupported = (int)EventLogEntryType.Error | (int)EventLogEntryType.Warning | (int)EventLogEntryType.Information;
			}
		}
		
		
		/// <summary>
		/// Close the source (more natural than Dispose())
		/// </summary>
		public void Close()
		{
			Dispose();
		}


		/// <summary>
		/// Delete the event source information from the associated logs registry hive
		/// </summary>
		/// <remarks>Caller must be a member of the BUILTIN\Administrators group</remarks>
		[PrincipalPermission(SecurityAction.Demand, Role = @"BUILTIN\Administrators")]
		public void Delete()
		{
			EventLogRegistry.Delete();
		}
		
		
		
		/// <summary>
		/// Report an event to the logfile
		/// </summary>
		/// <param name="pEventLogEntry">The <seealso cref="EventLogEntryType"/> of the event</param>
		/// <param name="pCategory">The application specific category of the event</param>
		/// <param name="pEventID">The ID (message number) of the event</param>
		/// <param name="pReplacementStrings">Array of replacement strings for the event</param>
		/// <param name="pRawData">Array of additional (opaque) data for the event</param>
		/// FI 20161021 [XXXXX] Modify
		// EG 20180423 Analyse du code Correction [CA1060]
		internal void ReportEvent(EventLogEntryType pEventLogEntry, uint pCategory, uint pEventID, string[] pReplacementStrings, byte[] pRawData)
		{
			try
			{
				if (IntPtr.Zero == m_HandleEventSource)
				{
					throw new ApplicationException("Event source failed to open");
				}

				uint extra = (uint)(pRawData != null ? pRawData.Length : 0);
				ushort strings = (ushort)(pReplacementStrings != null ? pReplacementStrings.Length : 0);

				int rc = NativeMethods.ReportEvent(m_HandleEventSource, (ushort)pEventLogEntry, (ushort)pCategory, pEventID,
					m_UserSid, strings, extra, pReplacementStrings, pRawData);

				// FI 20161021 [XXXXX] add if
				if (rc == 0)
				{
					PInvokeFailure.Check();
				}
			}
			catch (Exception ex)
			{
				throw new ApplicationException("ReportEvent error", ex);
			}
		}
		
		/// <summary>
		/// 
		/// </summary>
		public void NewEventLogRegistry()
		{
			EventLogRegistry = new EventLogRegistry(this, m_MachineName);
		}


		#endregion Methods

	}
	#endregion EventSource

	#region RegistryEx
	public class RegistryEx
	{
		#region Constructors
		private RegistryEx(){}
		#endregion Constructors

		#region Methods
		#region ReadUserSetting
		/// <summary>
		/// Read a value from the HKCU hive
		/// </summary>
		/// <param name="pRegPath">The path under HKCU to be read</param>
		/// <param name="pKeyName">The key of the entry to be returned</param>
		/// <param name="pDefaultValue">The default value to be returned on error</param>
		/// <returns>The read value or the defaultValue on error</returns>
		static public object ReadUserSetting(string pRegPath,string pKeyName,object pDefaultValue)
		{
			return ReadFromHive(Registry.CurrentUser,pRegPath,pKeyName,pDefaultValue);
		}
		#endregion ReadUserSetting
		#region ReadMachineSetting
		/// <summary>
		/// Read a value from the HKLM hive
		/// </summary>
		/// <param name="pRegPath">The path under HKLM to be read</param>
		/// <param name="pKeyName">The key of the entry to be returned</param>
		/// <param name="pDefaultValue">The default value to be returned on error</param>
		/// <returns>The read value or the defaultValue on error</returns>
		static public object ReadMachineSetting(string pRegPath,string pKeyName,object pDefaultValue)
		{
			return ReadFromHive(Registry.LocalMachine,pRegPath,pKeyName,pDefaultValue);
		}
		#endregion ReadMachineSetting
		#region ReadFromHive
		/// <summary>
		/// Read a value from the specified hive
		/// </summary>
		/// <param name="pRegHive">A local or remote registry hive</param>
		/// <param name="pRegPath">The path under the specified hive to be read</param>
		/// <param name="pKeyName">The key of the entry to be returned</param>
		/// <param name="pDefaultValue">The default value to be returned on error</param>
		/// <returns>The read value or the defaultValue on error</returns>
		static public object ReadFromHive(RegistryKey pRegHive, string pRegPath, string pKeyName, object pDefaultValue)
		{
			try
			{
				object ret = null;

				RegistryKey registryKey = pRegHive.OpenSubKey(pRegPath);
				if (null != registryKey)
					ret = registryKey.GetValue(pKeyName, pDefaultValue);

				return ret;
			}
			catch
			{
				return pDefaultValue;
			}
		}  
		#endregion ReadFromHive
		#region WriteUserSetting
		/// <summary>
		/// Write a value to the HKCU hive
		/// </summary>
		/// <param name="pRegPath">The path under HKCU to be written</param>
		/// <param name="pKeyName">The key of the entry to be created/updated</param>
		/// <param name="pKeyValue">The value to be written</param>
		static public void WriteUserSetting(string pRegPath,string pKeyName,object pKeyValue)
		{
			WriteToHive(Registry.CurrentUser,pRegPath,pKeyName,pKeyValue);
		}
		#endregion WriteUserSetting
		#region WriteUserSetting
		/// <summary>
		/// Write a value to the HKLM hive
		/// </summary>
		/// <param name="pRegPath">The path under HKLM to be written</param>
		/// <param name="pKeyName">The key of the entry to be created/updated</param>
		/// <param name="pKeyValue">The value to be written</param>
		static public void WriteMachineSetting(string pRegPath,string pKeyName,object pKeyValue)
		{
			WriteToHive(Registry.LocalMachine,pRegPath,pKeyName,pKeyValue);
		}
		#endregion WriteMachineSetting
		#region WriteToHive
		/// <summary>
		/// Write a value to the specified hive
		/// </summary>
		/// <param name="pRegHive">A local or remote registry hive</param>
		/// <param name="pRegPath">The path under the specified hive to be written</param>
		/// <param name="pKeyName">The key of the entry to be created/updated</param>
		/// <param name="pKeyValue">The value to be written</param>
		static public void WriteToHive(RegistryKey pRegHive, string pRegPath, string pKeyName, object pKeyValue)
		{
			try { pRegHive.CreateSubKey(pRegPath).SetValue(pKeyName, pKeyValue); }
			catch { }
		}
		#endregion WriteToHive
		#endregion Methods
	}
	#endregion RegistryEx
}
