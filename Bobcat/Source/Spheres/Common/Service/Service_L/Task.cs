using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
//
using EFS.ACommon;
using EFS.ApplicationBlocks.Data;
using EFS.Common;
using EFS.Common.MQueue;
using EFS.Process;

namespace EFS.SpheresService
{

	/// <summary>
	/// Classe d'informations pour le traçage d'un service
	/// </summary>
	public class TaskService
	{
		#region Members
		private string m_ConnectionString;
		private bool m_IsServiceObserver;
		private DateTime m_DtHost;
		private DateTime m_DtStop;
		private MQueueCount m_CountQueue;
		private Cst.ProcessTypeEnum m_ProcessType;
		#endregion Members
		#region Accessors
		#region CS
		/// <summary>
		/// Connection String
		/// </summary>
		public string ConnectionString
		{
			get { return m_ConnectionString; }
			set { m_ConnectionString = value; }
		}
		#endregion CS
		#region IsServiceObserver
		public bool IsServiceObserver
		{
			get { return m_IsServiceObserver; }
			set { m_IsServiceObserver = value; }
		}
		#endregion IsServiceObserver
		#region DtHost
		/// <summary>
		/// Horaire machine dernière activité (heartbeat ou process)
		/// </summary>
		public DateTime DtHost
		{
			get { return m_DtHost; }
			set { m_DtHost = value; }
		}
		#endregion DtHost
		#region DtStop
		/// <summary>
		/// Horaire d'arrêt du service
		/// </summary>
		public DateTime DtStop
		{
			get { return m_DtStop; }
			set { m_DtStop = value; }
		}
		#endregion DtStop
		#region ProcessType
		public Cst.ProcessTypeEnum ProcessType
		{
			get { return m_ProcessType; }
			set { m_ProcessType = value; }
		}
		#endregion ProcessType
		#region CountQueue
		public MQueueCount CountQueue
		{
			get { return m_CountQueue; }
			set { m_CountQueue = value; }
		}
		#endregion CountQueue
		#endregion Accessors
		#region Constructors
		/// <summary>
		/// Constructeur
		/// </summary>
		public TaskService()
		{
			m_DtHost = DateTime.UtcNow;
			m_CountQueue = new MQueueCount();
		}
		#endregion Constructors
	}

	/// <summary>
	/// Classe d'informations pour le traçage d'un process
	/// </summary>
	public class TaskProcess : TaskService
	{
		#region Members
		private DateTime m_DtLast;
		private ProcessState m_ProcessState;
		private int m_IdProcess_L;
		private MQueueRequester m_Requester;
		#endregion Members
		#region Accessors
		#region DtLast
		/// <summary>
		/// Horaire exécution du process
		/// </summary>
		public DateTime DtLast
		{
			get { return m_DtLast; }
			set { m_DtLast = value; }
		}
		#endregion DtLast
		#region ProcessState
		public ProcessState ProcessState
		{
			get { return m_ProcessState; }
			set { m_ProcessState = value; }
		}
		#endregion ProcessState
		#region IdProcess_L
		public int IdProcess_L
		{
			get { return m_IdProcess_L; }
			set { m_IdProcess_L = value; }
		}
		#endregion IdProcess_L
		#region Requester
		public MQueueRequester Requester
		{
			get { return m_Requester; }
			set { m_Requester = value; }
		}
		#endregion Requester
		#endregion Accessors
		#region Constructors
		/// <summary>
		/// Constructeur
		/// </summary>
		public TaskProcess() : base()
		{
			m_DtLast = DtHost;
		}
		#endregion Constructors
	}

}
