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
	/// Audit de l'activité d'un service (Alimentation de SERVICE_L)
	/// </summary>
	/// FI 20230309 [WI598] Implémentation de IDisposable
	public class ServiceTrace : IDisposable
	{
		#region Members
		/// <summary>
		///  Constante indiquant l'interval du Timer (En minutes)
		/// </summary>
		private const double TIMER_INTERVAL = 5D;

		/// <summary>
		/// Timer pour Hearbeat
		/// </summary>
		private readonly System.Threading.Timer m_TimerServiceTrace;

		/// <summary>
		/// 
		/// </summary>
		private readonly SemaphoreSlim m_ServiceLock = new SemaphoreSlim(1, 1);

		/// <summary>
		/// Liste en cache des process sur lesquels il y existe un heartbeat : Key = <seealso cref="Database(string)"/>; Value = Liste des process
		/// </summary>
		private readonly Dictionary<string, List<Service_L>> m_Services_L = new Dictionary<string, List<Service_L>>();


		private readonly DateTime m_DtStart;
		private DateTime m_DtLastUpd;

		private readonly AppInstanceService m_AppInstance;


		/// <summary>
		/// 
		/// </summary>
		private readonly string m_MOMType;
		private string m_MOMPath;

		private bool disposedValue;
		#endregion Members

		#region Accessors

		#region MOMPath
		/// <summary>
		/// Chemin du MOM
		/// </summary>
		public string MOMPath
		{
			get { return m_MOMPath; }
			set { m_MOMPath = value; }
		}
		#endregion MOMPath

		#endregion Accessors

		#region Constructors
		/// <summary>
		/// Constructeur
		/// </summary>
		/// <param name="pAppInstance"></param>
		/// <param name="pMOMType"></param>
		/// <param name="pMOMPath"></param>
		/// <param name="pOnTimer"></param>
		public ServiceTrace(AppInstanceService pAppInstance, string pMOMType, string pMOMPath, TimerCallback pOnTimer)
		{
			m_AppInstance = pAppInstance;
			m_MOMType = pMOMType;
			m_MOMPath = pMOMPath;
			m_DtStart = DateTime.UtcNow;
			m_DtLastUpd = m_DtStart;

			m_Services_L = new Dictionary<string, List<Service_L>>();
			m_TimerServiceTrace = new System.Threading.Timer(pOnTimer, null, Timeout.Infinite, Timeout.Infinite);

			AppInstanceService.TraceManager.TraceInformation(this, "Observer activated on " + pAppInstance.ServiceName);
		}
		#endregion Constructors

		#region Methods
		/// <summary>
		/// Démarrage du Timer de ServiceTrace
		/// </summary>
		public void StartTimer()
		{
			m_TimerServiceTrace.Change(TimeSpan.FromMinutes(TIMER_INTERVAL), TimeSpan.FromMinutes(TIMER_INTERVAL));
		}

		/// <summary>
		/// Arrêt du Timer de ServiceTrace
		/// </summary>
		public void StopTimer()
		{
			m_TimerServiceTrace.Change(Timeout.Infinite, Timeout.Infinite);
		}

		/// <summary>
		/// Ajout / Mise à jour d'une nouvelle tâche de process du service
		/// </summary>
		/// <param name="pProcess"></param>
		/// FI 20230309 [WI598] Refactoring
		public async Task TraceProcessAsync(ProcessBase pProcess, MQueueCount mQueueCount)
		{
			TaskProcess task = new TaskProcess
			{
				ConnectionString = pProcess.Cs,
				ProcessType = pProcess.MQueue.ProcessType,
				IsServiceObserver = pProcess.MQueue.IsMessageObserver,
				ProcessState = pProcess.ProcessState,
				IdProcess_L = pProcess.IdProcess,
				CountQueue = mQueueCount,
			};

			if (pProcess.MQueue.header.requesterSpecified)
				task.Requester = pProcess.MQueue.header.requester;

			m_DtLastUpd = task.DtHost;

			await UpdateAsync(task);
		}

		/// <summary>
		/// Mise à jour des informations sur le service
		/// </summary>
		/// <param name="pProcessType"></param>
		/// <param name="pMQueue">Mqueue éventuellement existant lorsque la trace se déclenche (timer se déclenche ou Mqueue de type observer)</param>
		/// <param name="pCountQueue"></param>
		public async Task TraceServiceAsync(Cst.ProcessTypeEnum pProcessType, MQueueBase pMQueue, MQueueCount pCountQueue)
		{
			TaskService task = new TaskService
			{
				ProcessType = pProcessType,
				CountQueue = pCountQueue
			};

			if (null != pMQueue) // Reseigné si Mqueue de type observer
			{
				task.ConnectionString = pMQueue.ConnectionString;
				task.IsServiceObserver = pMQueue.IsMessageObserver;
			}

			m_DtLastUpd = task.DtHost;

			await UpdateAsync(task);
		}

		/// <summary>
		/// Recherche le premier <see cref="Service_L"/>  en cache <seealso cref="m_Services_L"/> ayant pour type de process <paramref name="processType"/> sur la base attaquée par  <paramref name="cs"/>
		/// <para>Retourne null si non présent</para>
		/// </summary>
		/// <param name="cs"></param>
		/// <param name="processType"></param>
		/// <returns></returns>
		/// FI 20240319 [WI874] rafactoring
		public Service_L GetService(string cs, Cst.ProcessTypeEnum processType)
		{
			Service_L ret = null;
			if (ArrFunc.IsFilled(m_Services_L))
			{
				if (m_Services_L.TryGetValue(key: Database(cs), out List<Service_L> serviceOfDB))
					ret = serviceOfDB.FirstOrDefault(item => item.Process == processType);
			}
			return ret;
		}

		/// <summary>
		/// Ajoute <paramref name="service_L"/> dans le cache <seealso cref="m_Services_L"/>
		/// </summary>
		/// <param name="cs"></param>
		/// <param name="service_L"></param>
		/// FI 20240319 [WI874] add
		private void AddService(string cs, Service_L service_L)
		{
			if (false == m_Services_L.TryGetValue(key: Database(cs), out List<Service_L> serviceOfDB))
			{
				serviceOfDB = new List<Service_L>();
				m_Services_L.Add(key: Database(cs), serviceOfDB);
			}
			serviceOfDB.Add(service_L);
		}
		/// <summary>
		///  supprime <paramref name="service_L"/> du chache <seealso cref="m_Services_L"/>
		/// </summary>
		/// <param name="cs"></param>
		/// <param name="service_L"></param>
		/// FI 20240319 [WI874] add
		private Boolean RemoveService(string cs, Service_L service_L)
		{
			Boolean ret = false;
			if (m_Services_L.TryGetValue(key: Database(cs), out List<Service_L> serviceOfDB))
			{
				// Supprimer Service_L du dictionnaire 
				ret = serviceOfDB.Remove(service_L);
			}
			return ret;
		}


		/// <summary>
		/// Retourne la string SVR:{<see cref="CSManager.GetSvrName()"/>}-DB:{<see cref="CSManager.GetDbName()>"/>}
		/// </summary>
		/// <param name="pCS"></param>
		/// <returns></returns>
		/// FI 20240319 [WI874] add
		private static string Database(string pCS)
		{
			CSManager csManager = new CSManager(pCS);
			return $"SVR:{csManager.GetSvrName()}-DB:{@csManager.GetDbName()}";
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="pTask"></param>
		public async Task UpdateAsync(TaskService pTask)
		{
			// Autoriser une seule execution simultanée de la méthode
			await m_ServiceLock.WaitAsync();
			try
			{
				if (pTask.DtHost >= m_DtLastUpd)
				{
					Type tTask = pTask.GetType();
					if (tTask.Equals(typeof(TaskProcess)))
					{
						UpdateProcess(pTask as TaskProcess);
					}
					else if (tTask.Equals(typeof(TaskService)))
					{
						UpdateService(pTask as TaskService);
					}
				}
			}
			finally
			{
				m_ServiceLock.Release();
			}
		}

		/// <summary>
		/// Mise à jour / insertion d'une ligne de process dans SYSTEM_L
		/// </summary>
		/// <param name="pTaskProcess"></param>
		/// FI 20240319 [WI874] Refactoring
		private void UpdateProcess(TaskProcess pTaskProcess)
		{
			Service_L service_L = GetService(pTaskProcess.ConnectionString, pTaskProcess.ProcessType);
			Boolean isUpdate = (default(Service_L) != service_L);

			if (false == isUpdate)
			{
				// Process inexistant : le créer (sauf s'il existe déjà car inséra via une ConnectionString différente)
				service_L = new Service_L(pTaskProcess, m_AppInstance, m_DtStart, m_MOMType, m_MOMPath);
				isUpdate = service_L.Exists();
				if (false == isUpdate)
				{
					service_L.Insert();
					AddService(pTaskProcess.ConnectionString, service_L);
				}
			}

			if (isUpdate)
			{
				// Process existant : le mettre à jour
				int updated = service_L.UpdateProcess(pTaskProcess);

				if (0 == updated)
					RemoveService(pTaskProcess.ConnectionString, service_L);
			}
		}

		/// <summary>
		/// Mise à jour / insertion d'une ligne de service dans SYSTEM_L 
		/// </summary>
		/// <param name="pTaskService"></param>
		private void UpdateService(TaskService pTaskService)
		{
			Service_L service_L = default;

			if (StrFunc.IsFilled(pTaskService.ConnectionString)) // Renseigné lorsque traitement en cours (présence d'un message Queue)
			{
				service_L = GetService(pTaskService.ConnectionString, pTaskService.ProcessType);
				if (null != service_L)
				{
					int updated = service_L.UpdateService(pTaskService);
					if (0 == updated)
						RemoveService(pTaskService.ConnectionString, service_L);
				}
			}
			else
			{
				bool isExistRemove = false;

				// Heartbeat : mettre à jour tous les Services
				List<Service_L> serviceToRemove = new List<Service_L>();

				foreach (KeyValuePair<string, List<Service_L>> dicItem in m_Services_L)
				{
					foreach (Service_L item in dicItem.Value.Where(y => y.Process == pTaskService.ProcessType))
					{
						int updated = item.UpdateService(pTaskService);
						if (0 == updated)
						{
							// La ligne de SERVICE_L n'existe plus : ajout à la liste des service à supprimer
							serviceToRemove.Add(item);
							isExistRemove = true;
						}
					}
					if (serviceToRemove.Count > 0)
					{
						// Suppression des service n'ayant plus de ligne dans SERVICE_L
						foreach (Service_L service in serviceToRemove)
						{
							dicItem.Value.Remove(service);
						}
						serviceToRemove.Clear();
					}
				}
				if (isExistRemove)
				{
					// Supprimer les entrées m_Services_L n'ayant plus de Service_L
					List<string> keys = m_Services_L.Where(s => s.Value.Count == 0).Select(s => s.Key).ToList();

					foreach (string key in keys)
						m_Services_L.Remove(key);
				}
			}
		}
		#endregion Update

		/// <summary>
		/// Arret du Timer et mise à jour des informations d'arrêt du service
		/// </summary>
		/// <param name="pCountQueue"></param>
		public void Close(MQueueCount pCountQueue)
		{
			StopTimer();

			TaskService task = new TaskService
			{
				DtStop = DateTime.UtcNow,
				CountQueue = pCountQueue
			};

			foreach (List<Service_L> serviceDB in m_Services_L.Values)
			{
				foreach (Service_L item in serviceDB)
				{
					item.StopService(task);
				}
			}

			// Supprimer tous les Service_L
			m_Services_L.Clear();
			m_TimerServiceTrace.Dispose();
		}

		protected virtual void Dispose(bool disposing)
		{
			if (!disposedValue)
			{
				if (disposing)
				{
					m_TimerServiceTrace.Dispose();
				}

				// TODO: libérer les ressources non managées (objets non managés) et substituer le finaliseur
				// TODO: affecter aux grands champs une valeur null
				disposedValue = true;
			}
		}

		// // TODO: substituer le finaliseur uniquement si 'Dispose(bool disposing)' a du code pour libérer les ressources non managées
		// ~ServiceTrace()
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

	}

}
