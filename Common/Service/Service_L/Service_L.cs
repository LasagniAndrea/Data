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
	/// Classe de gestion des écritures dans SERVICE_L
	/// </summary>
	public class Service_L
	{
		#region Members
		private readonly string m_CS;
		private int m_IdService_L;
		private readonly Cst.ProcessTypeEnum m_Process;
		private readonly Cst.ServiceEnum m_Service;
		private readonly string m_InstanceName;
		private readonly string m_Version;
		private readonly string m_HostName;
		private readonly string m_Account;
		private readonly DateTime m_DtStart;
		private DateTime m_DtStop;
		private DateTime m_DtHost;
		private readonly string m_MOMType;
		private readonly string m_MOMPath;

		private int m_IdProcess_L;
		private DateTime m_DtLast;
		private MQueueRequester m_Requester;
		private bool m_RequesterSpecified;
		private bool m_IsServiceObserver;

		private int m_ProcessedSuccess;
		private int m_ProcessedError;
		private int m_CountQueueHigh;
		private int m_CountQueueNormal;
		private int m_CountQueueLow;
		#endregion Members
		#region Accessors
		#region DtStart
		/// <summary>
		/// Horaire de démarrage du service
		/// </summary>
		public DateTime DtStart
		{
			get { return m_DtStart; }
		}
		#endregion DtStart
		#region DtStop
		/// <summary>
		/// Horaire d'arrêt du service
		/// </summary>
		public DateTime DtStop
		{
			get { return m_DtStop; }
		}
		#endregion DtStop
		#region IdService
		public int IdService
		{
			get { return m_IdService_L; }
		}
		#endregion IdService
		#region HostName
		public string HostName
		{
			get { return m_HostName; }
		}
		#endregion HostName
		#region Process
		public Cst.ProcessTypeEnum Process
		{
			get { return m_Process; }
		}
		#endregion Process
		#region Service
		/// <summary>
		/// Type de service
		/// </summary>
		public Cst.ServiceEnum Service
		{
			get { return m_Service; }
		}
		#endregion Service		
		#region InstanceName
		/// <summary>
		/// Nom de l'instance du service
		/// </summary>
		public string InstanceName
		{
			get { return m_InstanceName; }
		}
		#endregion Service	

		#region Version
		/// <summary>
		/// Version du service
		/// </summary>
		public string Version
		{
			get { return m_Version; }
		}
		#endregion Version
		#region OSType
		public string OSType
		{
			get { return Environment.OSVersion.Platform.ToString(); }
		}
		#endregion OSType
		#region OSVersion
		public string OSVersion
		{
			get
			{
				string version = Environment.OSVersion.Version.Major.ToString() + ".";
				version += Environment.OSVersion.Version.Minor.ToString() + ".";
				version += Environment.OSVersion.Version.Build.ToString() + ".";
				version += Environment.OSVersion.Version.Revision.ToString();
				return version;
			}
		}
		#endregion OSVersion
		#endregion Accessors
		#region Constructors
		/// <summary>
		/// Constructeur
		/// </summary>
		/// <param name="pTask"></param>
		/// <param name="pAppInstanceService"></param>
		/// <param name="pDtStart"></param>
		/// <param name="pMOMType"></param>
		/// <param name="pMOMPath"></param>
		public Service_L(TaskProcess pTask, AppInstanceService pAppInstanceService, DateTime pDtStart, string pMOMType, string pMOMPath)
		{
			m_Account = string.Empty;
			m_CS = pTask.ConnectionString;

			m_Process = pTask.ProcessType;
			m_Service = pAppInstanceService.ServiceEnum;
			m_InstanceName = pAppInstanceService.AppNameInstance;
			m_Version = pAppInstanceService.AppVersion;
			m_HostName = pAppInstanceService.HostName;
			m_DtStart = pDtStart;

			m_MOMType = pMOMType;
			m_MOMPath = pMOMPath;
			ManagementObjectSearcher.AccountServiceInfo(pAppInstanceService.ServiceName, out m_Account);

			m_IdProcess_L = pTask.IdProcess_L;
			m_DtLast = pTask.DtLast;
			m_DtHost = pTask.DtHost;
			m_Requester = pTask.Requester;
			m_RequesterSpecified = (null != m_Requester);
			m_IsServiceObserver = pTask.IsServiceObserver;
			bool isSuccess = ProcessStateTools.IsStatusSuccess(pTask.ProcessState.Status);
			bool isError = ProcessStateTools.IsStatusError(pTask.ProcessState.Status);
			m_ProcessedSuccess = isSuccess ? 1 : 0;
			m_ProcessedError = isError ? 1 : 0;

			if (pTask.CountQueue != default(MQueueCount))
			{
				m_CountQueueHigh = pTask.CountQueue.CountQueueHigh;
				m_CountQueueNormal = pTask.CountQueue.CountQueueNormal;
				m_CountQueueLow = pTask.CountQueue.CountQueueLow;
			}
		}
		#endregion Constructors
		#region Methods
		/// <summary>
		/// 
		/// </summary>
		/// <param name="pQueryParameters"></param>
		private void SetCommonParameters(QueryParameters pQueryParameters)
		{
			pQueryParameters.Parameters["PROCESS"].Value = Process.ToString();
			pQueryParameters.Parameters["SERVICE"].Value = InstanceName;
			pQueryParameters.Parameters["VERSION"].Value = Version;
			pQueryParameters.Parameters["HOSTNAME"].Value = HostName;
			pQueryParameters.Parameters["DTSTART"].Value = DtStart;
			pQueryParameters.Parameters["DTSYS"].Value = OTCmlHelper.GetDateSysUTC(m_CS);
			pQueryParameters.Parameters["DTHOST"].Value = m_DtHost;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="pQueryParameters"></param>
		private void SetCounterQueue(QueryParameters pQueryParameters)
		{
			pQueryParameters.Parameters["COUNTQUEUEHIGH"].Value = m_CountQueueHigh;
			pQueryParameters.Parameters["COUNTQUEUENORMAL"].Value = m_CountQueueNormal;
			pQueryParameters.Parameters["COUNTQUEUELOW"].Value = m_CountQueueLow;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="pQueryParameters"></param>
		private void SetProcess(QueryParameters pQueryParameters)
		{
			pQueryParameters.Parameters["IDPROCESS_L"].Value = m_IdProcess_L;
			pQueryParameters.Parameters["DTLAST"].Value = m_DtLast;
			pQueryParameters.Parameters["PROCESSEDSUCCESS"].Value = m_ProcessedSuccess;
			pQueryParameters.Parameters["PROCESSEDERROR"].Value = m_ProcessedError;
			pQueryParameters.Parameters["OCCURREDERROR"].Value = 0;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="pQueryParameters"></param>
		private void SetProcessRequester(QueryParameters pQueryParameters)
		{
			pQueryParameters.Parameters["IDAREQUESTER"].Value = (m_RequesterSpecified && m_Requester.idASpecified) ? m_Requester.idA : Convert.DBNull;
			pQueryParameters.Parameters["DTREQUESTER"].Value = (m_RequesterSpecified) ? m_Requester.date : Convert.DBNull;
			pQueryParameters.Parameters["HOSTNAMEREQUESTER"].Value = (m_RequesterSpecified) ? m_Requester.hostName : Convert.DBNull;
			pQueryParameters.Parameters["SESSIONIDREQUESTER"].Value = (m_RequesterSpecified && m_Requester.sessionIdSpecified) ? m_Requester.sessionId : Convert.DBNull;
		}

		/// <summary>
		/// Insertion d'une ligne dans SERVICE_L. Retourne Cst.ErrLevel.IRVIOLATION si Erreur SQL
		/// </summary>
		/// <returns></returns>
		public void Insert()
		{
			if (StrFunc.IsFilled(m_CS))
			{
				Service_L_Query service_L_Query = new Service_L_Query(m_CS);

				QueryParameters queryParameters = service_L_Query.SQLInsert();

				SQLUP.GetId(out int idService_L, m_CS, SQLUP.IdGetId.SERVICE_L, SQLUP.PosRetGetId.First, 1);

				m_IdService_L = idService_L;
				queryParameters.Parameters["IDSERVICE_L"].Value = m_IdService_L;
				queryParameters.Parameters["ISSERVICEOBSERVER"].Value = m_IsServiceObserver;

				SetCommonParameters(queryParameters);
				SetProcess(queryParameters);
				SetProcessRequester(queryParameters);
				SetCounterQueue(queryParameters);

				queryParameters.Parameters["OSTYPE"].Value = OSType;
				queryParameters.Parameters["OSVERSION"].Value = OSVersion;
				queryParameters.Parameters["MOMTYPE"].Value = m_MOMType;
				queryParameters.Parameters["MOMPATH"].Value = m_MOMPath;
				queryParameters.Parameters["ACCOUNT"].Value = m_Account;
				queryParameters.Parameters["ROWATTRIBUT"].Value = Cst.RowAttribut_System;

				DataHelper.ExecuteNonQuery(queryParameters.Cs, CommandType.Text, queryParameters.Query, queryParameters.Parameters.GetArrayDbParameter());
			}
		}

		/// <summary>
		/// Mise à jour des informations sur le dernier process
		/// </summary>
		/// <param name="pTask"></param>
		/// <returns></returns>
		public int UpdateProcess(TaskProcess pTask)
		{
			int updated = 0;

			m_IdProcess_L = pTask.IdProcess_L;
			m_DtLast = pTask.DtLast;
			m_DtHost = pTask.DtHost;
			m_Requester = pTask.Requester;
			m_RequesterSpecified = (null != m_Requester);

			m_IsServiceObserver = pTask.IsServiceObserver;
			bool isSuccess = ProcessStateTools.IsStatusSuccess(pTask.ProcessState.Status);
			bool isError = ProcessStateTools.IsStatusError(pTask.ProcessState.Status);
			m_ProcessedSuccess += isSuccess ? 1 : 0;
			m_ProcessedError += isError ? 1 : 0;

			m_CountQueueHigh = pTask.CountQueue.CountQueueHigh;
			m_CountQueueNormal = pTask.CountQueue.CountQueueNormal;
			m_CountQueueLow = pTask.CountQueue.CountQueueLow;

			Service_L_Query query = new Service_L_Query(m_CS);

			QueryParameters queryParameters = query.SQLUpdateProcess();
			if (null != queryParameters)
			{
				queryParameters.Parameters["ISSERVICEOBSERVER"].Value = m_IsServiceObserver;

				SetCommonParameters(queryParameters);
				SetProcess(queryParameters);
				SetProcessRequester(queryParameters);
				SetCounterQueue(queryParameters);

				updated = DataHelper.ExecuteNonQuery(queryParameters.Cs, CommandType.Text, queryParameters.Query, queryParameters.Parameters.GetArrayDbParameter());
			}


			return updated;
		}

		/// <summary>
		/// Mise à jour de l'horaire du heartbeat du service
		/// </summary>
		/// <param name="pTask"></param>
		/// <returns></returns>
		public int UpdateService(TaskService pTask)
		{

			m_DtHost = pTask.DtHost;

			Service_L_Query service_L_Query = new Service_L_Query(m_CS);

			QueryParameters queryParameters = service_L_Query.SQLUpdateService();

			SetCommonParameters(queryParameters);
			SetCounterQueue(queryParameters);

			int updated = DataHelper.ExecuteNonQuery(queryParameters.Cs, CommandType.Text, queryParameters.Query, queryParameters.Parameters.GetArrayDbParameter());

			return updated;
		}

		/// <summary>
		/// Mise à jour de l'horaire d'arrêt du service
		/// </summary>
		/// <param name="pTask"></param>
		/// <returns></returns>
		public Cst.ErrLevel StopService(TaskService pTask)
		{
			Cst.ErrLevel ret = Cst.ErrLevel.SUCCESS;
			if (StrFunc.IsFilled(m_CS) && (null != pTask) && DtFunc.IsDateTimeFilled(pTask.DtStop))
			{
				m_DtHost = pTask.DtStop;
				m_DtStop = pTask.DtStop;

				Service_L_Query service_L_Query = new Service_L_Query(m_CS);
				if (null != service_L_Query)
				{
					QueryParameters queryParameters = service_L_Query.SQLStopService();
					if (null != queryParameters)
					{
						SetCommonParameters(queryParameters);
						SetCounterQueue(queryParameters);
						queryParameters.Parameters["DTSTOP"].Value = m_DtStop;
						//
						DataHelper.ExecuteNonQuery(queryParameters.Cs, CommandType.Text, queryParameters.Query, queryParameters.Parameters.GetArrayDbParameter());
					}
				}
			}
			return ret;
		}
		/// <summary>
		///  Retourne True s'il existe déjà l'enregistrement dans la table SERVICE_L. Aliemente m_IdService_L si l'enregistrement existe déjà.
		/// </summary>
		/// <returns></returns>
		/// FI 20240319 [WI874] Add
		public bool Exists()
		{
			bool ret = false;

			Service_L_Query service_L_Query = new Service_L_Query(m_CS);

			QueryParameters queryParameters = service_L_Query.SQLLoadIdService_L();
			queryParameters.Parameters["PROCESS"].Value = Process.ToString();
			queryParameters.Parameters["SERVICE"].Value = InstanceName;
			queryParameters.Parameters["VERSION"].Value = Version;
			queryParameters.Parameters["HOSTNAME"].Value = HostName;
			queryParameters.Parameters["DTSTART"].Value = DtStart;

			using (IDataReader dr = DataHelper.ExecuteReader(m_CS, CommandType.Text, queryParameters.Query, queryParameters.Parameters.GetArrayDbParameter()))
			{
				ret = dr.Read();
				if (ret)
				{
					m_IdService_L = Convert.ToInt32(dr["IDSERVICE_L"]);
				}
			}
			return ret;
		}


		#endregion Methods
	}


	/// <summary>
	/// Classe des requêtes d'écriture dans SERVICE_L
	/// <remark>
	/// PK de SERVICE_L : PROCESS, SERVICE, VERSION, HOSTNAME, DTSTART.
	/// Cette PK a d'utilité que le fait d'être unique. Elle n'est pas référencée par une table enfant.
	/// Son unicité doit être utile aux ordres UPDATESERVICE 
	/// - VERSION:  c'est pour identifier de manière unique un service
	/// - HOSTNAME: vient se rajouter car une même BdD peut être utilisé par différent serveur
	/// - PROCESS: vient se rajouter car un même service peut avoir plusieurs fonctionnalité business (ex. SettlMsgGen qui calcule les chaînes de Rglt et qui produit les messages de Rglt)
	/// - DTSTART: pour gérer le cas d'un service qui est stoppé (ou killed) et redémarré, afin de produire un nouveau record et ainsi conservé l'ancien pour audit.
	/// </remark>
	/// </summary>
	public class Service_L_Query
	{


		#region Members
		private readonly string m_Cs;
		private DataParameter m_ParamIdService_L;
		private DataParameter m_ParamService;
		private DataParameter m_ParamProcess;
		private DataParameter m_ParamVersion;
		private DataParameter m_ParamHostName;
		private DataParameter m_ParamAccount;
		private DataParameter m_ParamDtStart;
		private DataParameter m_ParamDtStop;
		private DataParameter m_ParamOSType;
		private DataParameter m_ParamOSVersion;
		private DataParameter m_ParamMOMType;
		private DataParameter m_ParamMOMPath;
		private DataParameter m_ParamDtSys;
		private DataParameter m_ParamDtHost;
		private DataParameter m_ParamIdProcess_L;
		private DataParameter m_ParamDtLast;
		private DataParameter m_ParamIdARequester;
		private DataParameter m_ParamDtRequester;
		private DataParameter m_ParamHostNameRequester;
		private DataParameter m_ParamSessionIdRequester;
		private DataParameter m_ParamIsServiceObserver;


		private DataParameter m_ParamProcessedSuccess;
		private DataParameter m_ParamProcessedError;
		private DataParameter m_ParamOccurredError;
		private DataParameter m_ParamCountQueueHigh;
		private DataParameter m_ParamCountQueueNormal;
		private DataParameter m_ParamCountQueueLow;

		private DataParameter m_ParamRowAttribut;
		#endregion Members
		#region Accessors
		#endregion Accessors
		#region Constructors
		public Service_L_Query(string pCs)
		{
			m_Cs = pCs;
			InitParameter();
		}
		#endregion Constructors
		#region Methods
		#region InitParameter
		private void InitParameter()
		{
			m_ParamIdService_L = new DataParameter(m_Cs, "IDSERVICE_L", DbType.Int32);
			m_ParamProcess = new DataParameter(m_Cs, "PROCESS", DbType.AnsiString, SQLCst.UT_ENUM_MANDATORY_LEN);
			m_ParamService = new DataParameter(m_Cs, "SERVICE", DbType.AnsiString, SQLCst.UT_ENUM_MANDATORY_LEN);
			m_ParamVersion = new DataParameter(m_Cs, "VERSION", DbType.AnsiString, SQLCst.UT_ENUM_MANDATORY_LEN);
			m_ParamHostName = new DataParameter(m_Cs, "HOSTNAME", DbType.AnsiString, SQLCst.UT_HOST_LEN);
			m_ParamAccount = new DataParameter(m_Cs, "ACCOUNT", DbType.AnsiString, SQLCst.UT_ENUM_OPTIONAL_LEN);
			m_ParamDtStart = new DataParameter(m_Cs, "DTSTART", DbType.DateTime2);
			m_ParamDtStop = new DataParameter(m_Cs, "DTSTOP", DbType.DateTime2);
			m_ParamDtHost = new DataParameter(m_Cs, "DTHOST", DbType.DateTime2);
			m_ParamDtSys = new DataParameter(m_Cs, "DTSYS", DbType.DateTime2);
			m_ParamOSType = new DataParameter(m_Cs, "OSTYPE", DbType.AnsiString, SQLCst.UT_ENUM_OPTIONAL_LEN);
			m_ParamOSVersion = new DataParameter(m_Cs, "OSVERSION", DbType.AnsiString, SQLCst.UT_ENUM_OPTIONAL_LEN);
			m_ParamMOMType = new DataParameter(m_Cs, "MOMTYPE", DbType.AnsiString, SQLCst.UT_ENUM_OPTIONAL_LEN);
			m_ParamMOMPath = new DataParameter(m_Cs, "MOMPATH", DbType.AnsiString, SQLCst.UT_ENUM_OPTIONAL_LEN);

			m_ParamIdProcess_L = new DataParameter(m_Cs, "IDPROCESS_L", DbType.Int32);
			m_ParamDtLast = new DataParameter(m_Cs, "DTLAST", DbType.DateTime2);
			m_ParamIdARequester = new DataParameter(m_Cs, "IDAREQUESTER", DbType.Int32);
			m_ParamDtRequester = new DataParameter(m_Cs, "DTREQUESTER", DbType.DateTime2);
			m_ParamHostNameRequester = new DataParameter(m_Cs, "HOSTNAMEREQUESTER", DbType.AnsiString, SQLCst.UT_HOST_LEN);
			m_ParamSessionIdRequester = new DataParameter(m_Cs, "SESSIONIDREQUESTER", DbType.AnsiString, 64);
			m_ParamIsServiceObserver = new DataParameter(m_Cs, "ISSERVICEOBSERVER", DbType.Boolean);

			m_ParamProcessedSuccess = new DataParameter(m_Cs, "PROCESSEDSUCCESS", DbType.Int32);
			m_ParamProcessedError = new DataParameter(m_Cs, "PROCESSEDERROR", DbType.Int32);
			m_ParamOccurredError = new DataParameter(m_Cs, "OCCURREDERROR", DbType.Int32);

			m_ParamCountQueueHigh = new DataParameter(m_Cs, "COUNTQUEUEHIGH", DbType.Int32);
			m_ParamCountQueueNormal = new DataParameter(m_Cs, "COUNTQUEUENORMAL", DbType.Int32);
			m_ParamCountQueueLow = new DataParameter(m_Cs, "COUNTQUEUELOW", DbType.Int32);

			m_ParamRowAttribut = new DataParameter(m_Cs, "ROWATTRIBUT", DbType.AnsiString, SQLCst.UT_ROWATTRIBUT_LEN);
		}
		#endregion InitParameter
		#region SQLInsert
		public QueryParameters SQLInsert()
		{
			#region parameters
			DataParameters parameters = new DataParameters();
			parameters.Add(m_ParamIdService_L);
			parameters.Add(m_ParamProcess);
			parameters.Add(m_ParamService);
			parameters.Add(m_ParamVersion);
			parameters.Add(m_ParamHostName);
			parameters.Add(m_ParamAccount);
			parameters.Add(m_ParamDtStart);
			parameters.Add(m_ParamOSType);
			parameters.Add(m_ParamOSVersion);
			parameters.Add(m_ParamMOMType);
			parameters.Add(m_ParamMOMPath);
			parameters.Add(m_ParamDtSys);
			parameters.Add(m_ParamDtHost);
			parameters.Add(m_ParamIdProcess_L);
			parameters.Add(m_ParamDtLast);
			parameters.Add(m_ParamIdARequester);
			parameters.Add(m_ParamDtRequester);
			parameters.Add(m_ParamHostNameRequester);
			parameters.Add(m_ParamSessionIdRequester);
			parameters.Add(m_ParamIsServiceObserver);
			parameters.Add(m_ParamProcessedSuccess);
			parameters.Add(m_ParamProcessedError);
			parameters.Add(m_ParamOccurredError);
			parameters.Add(m_ParamCountQueueHigh);
			parameters.Add(m_ParamCountQueueNormal);
			parameters.Add(m_ParamCountQueueLow);
			parameters.Add(m_ParamRowAttribut);
			#endregion parameters
			#region Query
			StrBuilder sqlQuery = new StrBuilder();
			sqlQuery += SQLCst.INSERT_INTO_DBO + Cst.OTCml_TBL.SERVICE_L.ToString() + Cst.CrLf;
			sqlQuery += @"(IDSERVICE_L, PROCESS, SERVICE, VERSION, HOSTNAME, ACCOUNT," + Cst.CrLf;
			sqlQuery += @"DTSTART, OSTYPE, OSVERSION, MOMTYPE, MOMPATH," + Cst.CrLf;
			sqlQuery += @"DTSYS, DTHOST, IDPROCESS_L, DTLAST," + Cst.CrLf;
			sqlQuery += @"IDAREQUESTER, DTREQUESTER, HOSTNAMEREQUESTER, SESSIONIDREQUESTER, ISSERVICEOBSERVER," + Cst.CrLf;
			sqlQuery += @"PROCESSEDSUCCESS, PROCESSEDERROR, OCCURREDERROR," + Cst.CrLf;
			sqlQuery += @"COUNTQUEUEHIGH, COUNTQUEUENORMAL, COUNTQUEUELOW," + Cst.CrLf;
			sqlQuery += @"ROWATTRIBUT)" + Cst.CrLf;
			sqlQuery += @"values" + Cst.CrLf;
			sqlQuery += @"(@IDSERVICE_L, @PROCESS, @SERVICE, @VERSION, @HOSTNAME, @ACCOUNT," + Cst.CrLf;
			sqlQuery += @"@DTSTART, @OSTYPE, @OSVERSION, @MOMTYPE, @MOMPATH," + Cst.CrLf;
			sqlQuery += @"@DTSYS, @DTHOST, @IDPROCESS_L, @DTLAST," + Cst.CrLf;
			sqlQuery += @"@IDAREQUESTER, @DTREQUESTER, @HOSTNAMEREQUESTER, @SESSIONIDREQUESTER, @ISSERVICEOBSERVER," + Cst.CrLf;
			sqlQuery += @"@PROCESSEDSUCCESS, @PROCESSEDERROR, @OCCURREDERROR," + Cst.CrLf;
			sqlQuery += @"@COUNTQUEUEHIGH, @COUNTQUEUENORMAL, @COUNTQUEUELOW," + Cst.CrLf;
			sqlQuery += @"@ROWATTRIBUT)" + Cst.CrLf;
			#endregion Query
			QueryParameters ret = new QueryParameters(m_Cs, sqlQuery.ToString(), parameters);
			return ret;
		}
		#endregion SQLInsert
		#region SQLStopService
		public QueryParameters SQLStopService()
		{
			#region parameters
			DataParameters parameters = new DataParameters();
			parameters.Add(m_ParamService);
			parameters.Add(m_ParamProcess);
			parameters.Add(m_ParamVersion);
			parameters.Add(m_ParamHostName);
			parameters.Add(m_ParamDtStart);

			parameters.Add(m_ParamDtHost);
			parameters.Add(m_ParamDtSys);
			parameters.Add(m_ParamDtStop);
			parameters.Add(m_ParamCountQueueHigh);
			parameters.Add(m_ParamCountQueueNormal);
			parameters.Add(m_ParamCountQueueLow);
			#endregion parameters
			#region Query
			StrBuilder sqlQuery = new StrBuilder();
			sqlQuery += SQLCst.UPDATE_DBO + Cst.OTCml_TBL.SERVICE_L.ToString() + Cst.CrLf;
			sqlQuery += SQLCst.SET + "DTHOST=@DTHOST,DTSYS=@DTSYS,DTSTOP=@DTSTOP," + Cst.CrLf;
			sqlQuery += @"COUNTQUEUEHIGH=@COUNTQUEUEHIGH," + Cst.CrLf;
			sqlQuery += @"COUNTQUEUENORMAL=@COUNTQUEUENORMAL," + Cst.CrLf;
			sqlQuery += @"COUNTQUEUELOW=@COUNTQUEUELOW" + Cst.CrLf;
			sqlQuery += SQLCst.WHERE + "SERVICE=@SERVICE" + SQLCst.AND + "PROCESS=@PROCESS" + SQLCst.AND;
			sqlQuery += "VERSION=@VERSION" + SQLCst.AND + "HOSTNAME=@HOSTNAME" + Cst.CrLf;
			sqlQuery += SQLCst.AND + "DTSTART=@DTSTART" + Cst.CrLf;
			#endregion Query
			QueryParameters ret = new QueryParameters(m_Cs, sqlQuery.ToString(), parameters);
			return ret;
		}
		#endregion SQLStopService		
		#region SQLUpdateService
		public QueryParameters SQLUpdateService()
		{

			DataParameters parameters = new DataParameters();
			parameters.Add(m_ParamService);
			parameters.Add(m_ParamProcess);
			parameters.Add(m_ParamVersion);
			parameters.Add(m_ParamHostName);
			parameters.Add(m_ParamDtStart);
			parameters.Add(m_ParamDtHost);
			parameters.Add(m_ParamDtSys);
			parameters.Add(m_ParamCountQueueHigh);
			parameters.Add(m_ParamCountQueueNormal);
			parameters.Add(m_ParamCountQueueLow);


			StrBuilder sqlQuery = new StrBuilder();
			sqlQuery += SQLCst.UPDATE_DBO + Cst.OTCml_TBL.SERVICE_L.ToString() + Cst.CrLf;
			sqlQuery += SQLCst.SET + "DTHOST=@DTHOST,DTSYS=@DTSYS," + Cst.CrLf;
			sqlQuery += @"COUNTQUEUEHIGH=@COUNTQUEUEHIGH," + Cst.CrLf;
			sqlQuery += @"COUNTQUEUENORMAL=@COUNTQUEUENORMAL," + Cst.CrLf;
			sqlQuery += @"COUNTQUEUELOW=@COUNTQUEUELOW" + Cst.CrLf;
			sqlQuery += SQLCst.WHERE + "SERVICE=@SERVICE" + SQLCst.AND + "PROCESS=@PROCESS" + SQLCst.AND;
			sqlQuery += "VERSION=@VERSION" + SQLCst.AND + "HOSTNAME=@HOSTNAME" + Cst.CrLf;
			sqlQuery += SQLCst.AND + "DTSTART=@DTSTART" + Cst.CrLf;

			QueryParameters ret = new QueryParameters(m_Cs, sqlQuery.ToString(), parameters);
			return ret;
		}
		#endregion SQLUpdateService
		#region SQLUpdateProcess
		public QueryParameters SQLUpdateProcess()
		{

			DataParameters parameters = new DataParameters();
			parameters.Add(m_ParamService);
			parameters.Add(m_ParamProcess);
			parameters.Add(m_ParamVersion);
			parameters.Add(m_ParamHostName);
			parameters.Add(m_ParamDtStart);
			parameters.Add(m_ParamDtHost);

			parameters.Add(m_ParamIdProcess_L);
			parameters.Add(m_ParamDtLast);
			parameters.Add(m_ParamIdARequester);
			parameters.Add(m_ParamDtRequester);
			parameters.Add(m_ParamHostNameRequester);
			parameters.Add(m_ParamSessionIdRequester);
			parameters.Add(m_ParamIsServiceObserver);

			parameters.Add(m_ParamDtSys);
			parameters.Add(m_ParamProcessedSuccess);
			parameters.Add(m_ParamProcessedError);
			parameters.Add(m_ParamOccurredError);

			parameters.Add(m_ParamCountQueueHigh);
			parameters.Add(m_ParamCountQueueNormal);
			parameters.Add(m_ParamCountQueueLow);



			StrBuilder sqlQuery = new StrBuilder();
			sqlQuery += SQLCst.UPDATE_DBO + Cst.OTCml_TBL.SERVICE_L.ToString() + Cst.CrLf;
			sqlQuery += SQLCst.SET + "IDPROCESS_L=@IDPROCESS_L,DTLAST=@DTLAST,DTHOST=@DTHOST,DTSYS=@DTSYS," + Cst.CrLf;
			sqlQuery += @"IDAREQUESTER=@IDAREQUESTER, DTREQUESTER=@DTREQUESTER," + Cst.CrLf;
			sqlQuery += @"HOSTNAMEREQUESTER=@HOSTNAMEREQUESTER, SESSIONIDREQUESTER=@SESSIONIDREQUESTER," + Cst.CrLf;
			sqlQuery += @"ISSERVICEOBSERVER=@ISSERVICEOBSERVER," + Cst.CrLf;
			sqlQuery += @"PROCESSEDSUCCESS=@PROCESSEDSUCCESS," + Cst.CrLf;
			sqlQuery += @"PROCESSEDERROR=@PROCESSEDERROR," + Cst.CrLf;
			sqlQuery += @"OCCURREDERROR=@OCCURREDERROR," + Cst.CrLf;
			sqlQuery += @"COUNTQUEUEHIGH=@COUNTQUEUEHIGH," + Cst.CrLf;
			sqlQuery += @"COUNTQUEUENORMAL=@COUNTQUEUENORMAL," + Cst.CrLf;
			sqlQuery += @"COUNTQUEUELOW=@COUNTQUEUELOW" + Cst.CrLf;
			sqlQuery += SQLCst.WHERE + "SERVICE=@SERVICE" + SQLCst.AND + "PROCESS=@PROCESS" + SQLCst.AND;
			sqlQuery += "VERSION=@VERSION" + SQLCst.AND + "HOSTNAME=@HOSTNAME" + Cst.CrLf;
			sqlQuery += SQLCst.AND + "DTSTART=@DTSTART" + Cst.CrLf;

			QueryParameters ret = new QueryParameters(m_Cs, sqlQuery.ToString(), parameters);

			return ret;
		}
		#endregion SQLUpdateProcess

		/// <summary>
		///  Requête qui permet d'obtenir la valeur de la colonne IDSERVICE_L sur un enregistrement
		/// </summary>
		/// <returns></returns>
		/// FI 20240319 [WI874] Add
		public QueryParameters SQLLoadIdService_L()
		{
			DataParameters parameters = new DataParameters();
			parameters.Add(m_ParamService);
			parameters.Add(m_ParamProcess);
			parameters.Add(m_ParamVersion);
			parameters.Add(m_ParamHostName);
			parameters.Add(m_ParamDtStart);

			StrBuilder sqlQuery = new StrBuilder();
			sqlQuery += SQLCst.SELECT;
			sqlQuery += "IDSERVICE_L" + Cst.CrLf;
			sqlQuery += SQLCst.FROM_DBO + Cst.OTCml_TBL.SERVICE_L.ToString() + Cst.CrLf;
			sqlQuery += SQLCst.WHERE + "SERVICE=@SERVICE" + SQLCst.AND + "PROCESS=@PROCESS" + SQLCst.AND;
			sqlQuery += "VERSION=@VERSION" + SQLCst.AND + "HOSTNAME=@HOSTNAME" + Cst.CrLf;
			sqlQuery += SQLCst.AND + "DTSTART=@DTSTART" + Cst.CrLf;

			QueryParameters ret = new QueryParameters(m_Cs, sqlQuery.ToString(), parameters);
			return ret;
		}

		#endregion Methods
	}

}
