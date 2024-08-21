using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Threading;
//
using EFS.ACommon;
using EFS.Common;
using EFS.ApplicationBlocks.Data;
using EFS.ApplicationBlocks.Data.Extension;
using EFS.Common.Log;
using EFS.Common.MQueue;
using EFS.LoggerClient;
using EFS.LoggerClient.LoggerService;

namespace EFS.Process
{
    #region IRQBase
    /// <summary>
    /// Classe de stockage des informations d'interruption de traitement (IRQAnswerer et IRQRequester)
    /// </summary>
    // EG 20180525 [23979] IRQ Processing
    // EG 20190315 New version of Naming IRQ Semaphore IRQ{IDTRK_L}{SERVERNAME}{DATABASENAME}
    // EG 20190318 New version of Naming IRQ Semaphore IRQ{REQUESTTYPE.IDTRK_L}{SERVERNAME}{DATABASENAME}
    public abstract class IRQBase
    {
        #region Members
        protected string _cs;
        protected int _idTRK_L;
        protected string _database;
        protected Tuple<int, string, DateTime> _userRequester;
        protected Tuple<string, string, string> _applicationRequester;
        protected MapDataReaderRow _trkDataReader = null;
        protected MapDataReaderRow _logDataReader = null;
        #endregion Members

        #region Accessors
        // EG 20190315 New version of Naming IRQ Semaphore
        public string RequestDatabase { get { return _database; } }
        public int RequestId { get { return _idTRK_L; } }

        public Tuple<int, string, DateTime> UserRequester
        {
            get { return _userRequester; }
        }
        public Tuple<string, string, string> ApplicationRequester
        {
            get { return _applicationRequester; }
        }

        public ProcessStateTools.ReadyStateEnum ReadyState
        {
            get { return ProcessStateTools.ParseReadyState(TrackerInfo("READYSTATE")); }
        }
        public string AppName
        {
            get { return LogInfo("APPNAME"); }
        }
        public string AppVersion
        {
            get { return LogInfo("APPVERSION"); }
        }
        public string HostName
        {
            get { return LogInfo("HOSTNAME"); }
        }
        public string RequestedBy
        {
            get { return LogTools.IdentifierAndId(_userRequester.Item2, _userRequester.Item1); }
        }
        /// <summary>
        /// 
        /// </summary>
        public string RequestedAt
        {
            get
            {
                string ret = DtFunc.DateTimeToStringISO(_userRequester.Item3);
                // FI 20200819 [XXXXX] Si UTC Spheres ajoute " Etc/UTC"
                if (_userRequester.Item3.Kind == DateTimeKind.Utc)
                    ret += " Etc/UTC";
                return ret;
            }
        }
        public string Status { get { return TrackerInfo("STATUSTRACKER"); } }
        #endregion Accessors

        #region Constructors
        // EG 20190315 New version of Naming IRQ Semaphore
        public IRQBase(string pCS)
        {
            _cs = pCS;
            _database = IRQTools.IRQDatabase(pCS);
            _userRequester = new Tuple<int, string, DateTime>(0, string.Empty, DateTime.MinValue);
            _applicationRequester = new Tuple<string, string, string>(string.Empty, string.Empty, string.Empty);
        }
        #endregion Constructors

        #region Methods

        #region LogInfo
        private string LogInfo(string pInfoName)
        {
            string _value = string.Empty;
            if (null != _logDataReader)
                _value = _logDataReader[pInfoName].Value.ToString();
            return _value;

        }
        #endregion LogInfo
        #region TrackerInfo
        private string TrackerInfo(string pInfoName)
        {
            string _value = string.Empty;
            if (null != _trkDataReader)
                _value = _trkDataReader[pInfoName].Value.ToString();
            return _value;

        }
        #endregion LogInfo

        #region ReadProcessLogInfo
        /// <summary>
        /// Lecture des données du Log rattaché :
        /// 1. à la demande d'interruption  si IRQAnswerer
        /// 2. au traitement à interrompre  si IRQRequester
        /// </summary>
        // EG 20200929 [XXXXX] Correction Bug Interruption IRQ Lecture DataReader et UTC conversion
        protected void ReadProcessLogInfo()
        {
            #region Lecture des données PROCESS_L
            ProcessLogQuery logQry = new ProcessLogQuery(_cs);
            QueryParameters qry = logQry.GetQuerySelectPROCESS_L_ByTracker(_cs);
            qry.Parameters["IDTRK_L"].Value = _idTRK_L;

            using (IDataReader dr = DataHelper.ExecuteReader(_cs, CommandType.Text, qry.Query, qry.Parameters.GetArrayDbParameter()))
            {
                _logDataReader = DataReaderExtension.DataReaderMapToSingle(dr);
                
                if (null != _logDataReader)
                {
                    _logDataReader.Column.Where(x => (x.Name == "DTSTPROCESS")).ToList().ForEach(
                        item =>
                        {
                            item.Value = DateTime.SpecifyKind(Convert.ToDateTime(item.Value), DateTimeKind.Utc);
                        });

                    _applicationRequester = new Tuple<string, string, string>(AppName, AppVersion, HostName);
                }
            }
            #endregion Lecture des données PROCESS_L
        }
        #endregion ReadProcessLogInfo
        #region ReadTrackerInfo
        /// <summary>
        /// Chargement des données du tracker rattaché : 
        /// 1. à la demande d'interruption  si IRQAnswerer
        /// 2. au traitement à interrompre  si IRQRequester
        /// </summary>
        /// EG 20201016 Correction Test sur _trkDataReader not null
        protected void ReadTrackerInfo()
        {
            TrackerQuery trkQry = new TrackerQuery(_cs);
            QueryParameters qry = trkQry.GetQuerySelect();
            qry.Parameters["IDTRK_L"].Value = _idTRK_L;

            using (IDataReader dr = DataHelper.ExecuteReader(_cs, CommandType.Text, qry.Query, qry.Parameters.GetArrayDbParameter()))
            {
                _trkDataReader = DataReaderExtension.DataReaderMapToSingle(dr);
                // FI 20200819 [XXXXX]  SpecifyKind UTC on DTINS et DTUPD
                if (null != _trkDataReader)
                {
                    _trkDataReader.Column.Where(x => (x.Name == "DTINS" || x.Name == "DTUPD")).ToList().ForEach(
                    item =>
                    {
                        if (item.Value != Convert.DBNull)
                            item.Value = DateTime.SpecifyKind(Convert.ToDateTime(item.Value), DateTimeKind.Utc);
                    });

                    _userRequester = new Tuple<int, string, DateTime>(
                        Convert.ToInt32(_trkDataReader["IDAINS"].Value),
                        _trkDataReader["IDAINS_IDENTIFIER"].Value.ToString(),
                        Convert.ToDateTime(_trkDataReader["DTINS"].Value));
                }
            }
            
        }
        #endregion ReadTrackerInfo
        #endregion Methods
    }
    #endregion IRQBase

    #region IRQAnswerer
    /// <summary>
    /// Classe de stockage des informations d'interruption de traitement
    /// vue côté traitement à interrompre
    /// </summary>
    // EG 20180525 [23979] IRQ Processing
    public sealed class IRQAnswerer : IRQBase
    {
        #region Constructors
        public IRQAnswerer(string pCS, int pProcessIdTRK_L) : base(pCS)
        {
            GetTrackerRequester(pProcessIdTRK_L);
            ReadTrackerInfo();
            ReadProcessLogInfo();
        }
        #endregion Constructors
        #region Methods
        #region GetTrackerRequester
        private void GetTrackerRequester(int pProcessIdTRK_L)
        {
            string sqlQuery = @"select IDTRK_L
            from dbo.TRACKER_L
            where (IRQIDTRK_L = @IRQIDTRK_L) and (GROUPTRACKER = @GROUPTRACKER) and (READYSTATE = @READYSTATE)and (STATUSTRACKER = @STATUSTRACKER)" + Cst.CrLf;

            DataParameters parameters = new DataParameters();
            parameters.Add(new DataParameter(_cs, "IRQIDTRK_L", DbType.Int32), pProcessIdTRK_L);
            parameters.Add(new DataParameter(_cs, "GROUPTRACKER", DbType.AnsiString, SQLCst.UT_ENUM_MANDATORY_LEN), Cst.GroupTrackerEnum.EXT.ToString());
            parameters.Add(new DataParameter(_cs, "READYSTATE", DbType.AnsiString, SQLCst.UT_ENUM_MANDATORY_LEN), ProcessStateTools.ReadyStateTerminated);
            parameters.Add(new DataParameter(_cs, "STATUSTRACKER", DbType.AnsiString, SQLCst.UT_ENUM_MANDATORY_LEN), ProcessStateTools.StatusSuccess);

            QueryParameters qry = new QueryParameters(_cs, sqlQuery, parameters);
            object obj = DataHelper.ExecuteScalar(_cs, CommandType.Text, qry.Query, qry.Parameters.GetArrayDbParameter());
            if (null != obj)
                _idTRK_L = Convert.ToInt32(obj);
        }
        #endregion GetTrackerRequester
        #endregion Methods
    }
    #endregion IRQAnswerer
    #region IRQRequester
    /// <summary>
    /// <para>Classe de stockage des informations d'interruption de traitement
    /// vue côté demande de traitement à interrompre</para>
    /// <para>Construction d'un message IRQ pour NormMsgFactoryMQueue</para>
    /// </summary>
    // EG 20180525 [23979] IRQ Processing
    public sealed class IRQRequester : IRQBase
    {
        #region Members
        private readonly Pair<string, Cst.TrackerSystemMsgAttribute> _trackerSystemMsg;
        private NormMsgFactoryMQueue _normMsgFactoryMQueue;
        #endregion Members

        #region Accessors
        // EG 20190315 New version of Naming IRQ Semaphore IRQ{IDTRK_L}{SERVERNAME}{DATABASENAME}
        // EG 20190318 New version of Naming IRQ Semaphore IRQ{REQUESTTYPE.IDTRK_L}{SERVERNAME}{DATABASENAME}
        public string NamedSystemSemaphore
        {
            get
            {
                if (null != _trackerSystemMsg)
                    return Cst.ProcessTypeEnum.IRQ.ToString() + "{" + _trackerSystemMsg.First + "." + TrackerIdProcess + "}" + RequestDatabase;
                else
                    return Cst.ProcessTypeEnum.IRQ.ToString() + "{" + TrackerIdProcess + "}" + RequestDatabase;
            }
        }
        public int TrackerIdProcess { get { return _idTRK_L; } }
        public NormMsgFactoryMQueue NormMsgFactoryMQueue { get { return _normMsgFactoryMQueue; } }
        public string ProcessToInterrupt { get { return _normMsgFactoryMQueue.buildingInfo.parameters["PROCESS"].ExValue; } }
        private bool IsDefaultSysNumber_IRQRequest
        {
            get { return (null != _trackerSystemMsg) && (_trackerSystemMsg.Second.SysNumber_IRQRequest == 0); }
        }
        #endregion Accessors

        #region Constructors
        /// <summary>
        /// Recherche des éléments SYSTEMMSG pour constitution Message et Tooltip en fonction du process
        /// </summary>
        /// <param name="pCS">ConnectionString</param>
        /// <param name="pProcessIdTRK_L">Identifiant du tracker (du process à interrompre)</param>
        // EG 20180525 [23979] IRQ Processing
        // EG 20190318 New version of Naming IRQ Semaphore IRQ{REQUESTTYPE.IDTRK_L}{SERVERNAME}{DATABASENAME}
        public IRQRequester(string pCS, int pProcessIdTRK_L) : base(pCS)
        {
            _idTRK_L = pProcessIdTRK_L;
            ReadTrackerInfo();

            if (null != _trkDataReader)
            {
                int sysNumber = Convert.ToInt32(_trkDataReader["SYSNUMBER"].Value);
                Cst.GroupTrackerEnum _group = (Cst.GroupTrackerEnum)ReflectionTools.EnumParse(new Cst.GroupTrackerEnum(),
                    _trkDataReader["GROUPTRACKER"].Value.ToString());
                _trackerSystemMsg = IRQTools.GetTrackerMsgAttribute(_group, sysNumber);
            }

            ReadProcessLogInfo();
        }
        #endregion Constructors

        #region Methods
        #region AddDataParameters
        /// <summary>
        /// Gestion des paramètres DATA1..5 du traitement à interrompre 
        /// pour Tooltip de la ligne IRQ du tracker
        /// </summary>
        // EG 20180525 [23979] IRQ Processing
        // EG 20180606 [23979] IRQ (EARGEN)
        // EG 20231129 [WI762] End of Day processing : Possibility to request processing without initial margin (Cst.PosRequestTypeEnum.EndOfDayWithoutMR)
        private void AddDataParameters(MQueueparameters pParameters, string pProcess)
        {
            // RequestedBy|RequestedAt (IDAINS|DTINS)
            AddRequestedDataParameters(pParameters);
            if (IsDefaultSysNumber_IRQRequest)
            {
                // Message générique d'interruption de traitement
                MQueueparameter parameter = new MQueueparameter("DATA3", TypeData.TypeDataEnum.@string);
                parameter.SetValue(pProcess);
                pParameters.Add(parameter);
            }
            else
            {
                Nullable<Cst.PosRequestTypeEnum> posRequestType = ReflectionTools.ConvertStringToEnumOrNullable<Cst.PosRequestTypeEnum>(pProcess);
                if (posRequestType.HasValue)
                {
                    switch (posRequestType.Value)
                    {
                        case Cst.PosRequestTypeEnum.ClosingDay:
                        case Cst.PosRequestTypeEnum.EndOfDay:
                        case Cst.PosRequestTypeEnum.EndOfDayWithoutInitialMargin:
                        case Cst.PosRequestTypeEnum.CashBalance: // FI 20200819 [XXXXX] Add
                            AddParameter(pParameters, _trkDataReader, "DATA1", "DATA3");
                            AddParameter(pParameters, _trkDataReader, "DATA2", "DATA4");
                            AddParameter(pParameters, _trkDataReader, "DATA3", "DATA5");
                            break;
                    }
                }
                else
                {
                    Nullable<Cst.ProcessTypeEnum> processType = ReflectionTools.ConvertStringToEnumOrNullable<Cst.ProcessTypeEnum>(pProcess);
                    if (processType.HasValue)
                    {
                        switch (processType)
                        {
                            case Cst.ProcessTypeEnum.CASHBALANCE:
                                AddParameter(pParameters, _trkDataReader, "DATA1", "DATA3");
                                AddParameter(pParameters, _trkDataReader, "DATA2", "DATA4");
                                break;
                            case Cst.ProcessTypeEnum.RISKPERFORMANCE:
                            case Cst.ProcessTypeEnum.INVOICINGGEN:
                            case Cst.ProcessTypeEnum.RIMGEN:
                            case Cst.ProcessTypeEnum.EARGEN:
                                AddParameter(pParameters, _trkDataReader, "DATA1", "DATA3");
                                AddParameter(pParameters, _trkDataReader, "DATA2", "DATA4");
                                AddParameter(pParameters, _trkDataReader, "DATA3", "DATA5");
                                break;
                            case Cst.ProcessTypeEnum.IO:
                                AddParameter(pParameters, _trkDataReader, "DATA1", "DATA3");
                                break;
                        }
                    }
                }
            }
        }
        #endregion AddDataParameters
        #region AddParameter
        /// <summary>
        /// Ajout d'un paramètre
        /// </summary>
        // EG 20180525 [23979] IRQ Processing
        private void AddParameter(MQueueparameters pParameters, MapDataReaderRow pMapDr, string pColumnName,
            string pParameterName = null, TypeData.TypeDataEnum pDataType = TypeData.TypeDataEnum.@string)
        {
            string parameterName = pParameterName;
            if (StrFunc.IsEmpty(parameterName))
                parameterName = pColumnName;

            if (null == pParameters)
                pParameters = new MQueueparameters();

            if ((null != pMapDr[pColumnName]) && (false == Convert.IsDBNull(pMapDr[pColumnName].Value)))
            {
                MQueueparameter parameter = new MQueueparameter(parameterName, pDataType);
                object _value = pMapDr[pColumnName].Value;
                switch (pDataType)
                {
                    case TypeData.TypeDataEnum.@int:
                    case TypeData.TypeDataEnum.@integer:
                        parameter.SetValue(Convert.ToInt32(_value));
                        break;
                    case TypeData.TypeDataEnum.@decimal:
                    case TypeData.TypeDataEnum.dec:
                        parameter.SetValue(Convert.ToDecimal(_value));
                        break;
                    case TypeData.TypeDataEnum.@bool:
                    case TypeData.TypeDataEnum.@boolean:
                        parameter.SetValue(Convert.ToBoolean(_value));
                        break;
                    case TypeData.TypeDataEnum.date:
                    case TypeData.TypeDataEnum.datetime:
                    case TypeData.TypeDataEnum.time:
                        parameter.SetValue(Convert.ToDateTime(_value));
                        break;
                    default:
                        parameter.SetValue(_value.ToString());
                        break;
                }
                pParameters.Add(parameter);
            }
        }
        #endregion AddParameter
        #region AddRequestedDataParameters
        /// <summary>
        /// DATA1 et DATA2 = User Requester de traitement à interrompre
        /// </summary>
        /// <param name="pParameters"></param>
        // EG 20180525 [23979] IRQ Processing
        private void AddRequestedDataParameters(MQueueparameters pParameters)
        {
            //Add DATA1 from "IDAINS_IDENTIFIER" column
            AddParameter(pParameters, _trkDataReader, "IDAINS_IDENTIFIER", "DATA1", TypeData.TypeDataEnum.@string);
            pParameters["DATA1"].ExValue = _trkDataReader["IDAINS"].Value.ToString();
            pParameters["DATA1"].ExValueSpecified = true;

            //Add DATA2 form DTINS column DTINS
            AddParameter(pParameters, _trkDataReader, "DTINS", "DATA2", TypeData.TypeDataEnum.datetime);
        }
        #endregion AddRequestedDataParameters

        #region ConstructNormMsgFactoryMessage
        /// <summary>
        /// Création d'un message de Type NORMMSGFACTORY avec Cst.ProcessTypeEnum.IRQPROCESS
        /// destiné à l'intégration d'une demande d'interruption de process
        /// </summary>
        // EG 20180525 [23979] IRQ Processing
        public Cst.ErrLevel ConstructNormMsgFactoryMessage()
        {
            MQueueAttributes mQueueAttributes = new MQueueAttributes()
            {
                connectionString = _cs,
                id = _idTRK_L,
            };

            _normMsgFactoryMQueue = new NormMsgFactoryMQueue(mQueueAttributes)
            {
                buildingInfo = new NormMsgBuildingInfo()
                {
                    id = _idTRK_L,
                    idSpecified = true,
                    identifier = Cst.ProcessTypeEnum.IRQ.ToString(),
                    identifierSpecified = true,
                    processType = Cst.ProcessTypeEnum.IRQ,
                }
            };
            
             
            MQueueparameter[] parameters = GetNormMsgFactoryParameters();
            _normMsgFactoryMQueue.buildingInfo.parametersSpecified = ArrFunc.IsFilled(parameters);
            if (_normMsgFactoryMQueue.buildingInfo.parametersSpecified)
            {
                // FI 2021080 [XXXXX] set parameters = new MQueueparameters();
                _normMsgFactoryMQueue.buildingInfo.parameters = new MQueueparameters();
                _normMsgFactoryMQueue.buildingInfo.parameters.Add(parameters);
            }

            return Cst.ErrLevel.SUCCESS;
        }
        #endregion ConstructNormMsgFactoryMessage

        #region GetNormMsgFactoryParameters
        /// <summary>
        /// Création des paramètres du message NORMMSGFACTORY contenant l'ensemble des caractéristiques du
        /// couple TRACKER_L/PROCESS_L
        /// </summary>
        /// <returns>Array de paramètres</returns>
        public MQueueparameter[] GetNormMsgFactoryParameters()
        {
            MQueueparameters parameters = new MQueueparameters();

            if (null != _trkDataReader)
            {
                // Alimentation du type de process à interrompre
                MQueueparameter parameter = new MQueueparameter("PROCESS", TypeData.TypeDataEnum.integer);
                parameter.SetValue(_trackerSystemMsg.Second.SysNumber);
                parameter.ExValue = _trackerSystemMsg.First;
                parameter.ExValueSpecified = true;
                parameters.Add(parameter);
                // Alimentation des DATA (1..5)
                AddDataParameters(parameters, parameter.ExValue);
            }
            return parameters.parameter;
        }
        #endregion GetNormMsgFactoryParameters
        #endregion Methods

    }
    #endregion IRQRequester
    #region IRQSemaphore
    /// <summary>
    /// Classe de stockage des informations d'une sémaphore
    /// matérialisant une demande d'interruption de traitement
    /// </summary>
    // EG 20180525 [23979] IRQ Processing
    public class IRQSemaphore
    {
        #region Members
        /// <summary>
        /// Nom complet de la sémaphore "Global\" + IRQ{REQUESTTYPE.IDTRK_L}{SERVERNAME}{DATABASENAME}
        /// avec Server et Database liée au traitement à interrompre
        /// avec idTRK_L identifiant du TRACKER_L lié au traitement à interrompre
        /// </summary>
        // EG 20190315 New version of Naming IRQ Semaphore IRQ{IDTRK_L}{SERVERNAME}{DATABASENAME}
        // EG 20190318 New version of Naming IRQ Semaphore IRQ{REQUESTTYPE.IDTRK_L}{SERVERNAME}{DATABASENAME}
        public string name;
        /// <summary>
        /// Database concernée
        /// </summary>
        public string requestDatabase;
        /// <summary>
        /// idTRK_L du traitement à interrompre
        /// </summary>
        public int requestId;
        /// <summary>
        /// Utilisateur (id, identifier, date) :
        /// 1. du traitement à interrompre lorsque vue côté demande d'interruption (IRQResquester)
        /// 2. de la demande d'interruption lorsque vue côté traitement à interrompre (IRQAnswerer)
        /// </summary>
        public Tuple<int, string, DateTime> userRequester;
        /// <summary>
        /// Application (nom, version, hostname) :
        /// 1. du traitement à interrompre lorsque vue côté demande d'interruption (IRQResquester)
        /// 2. de la demande d'interruption lorsque vue côté traitement à interrompre (IRQAnswerer)
        /// </summary>
        public Tuple<string, string, string> applicationRequester;

        /// <summary>
        /// Sémaphore nommée créée
        /// </summary>
        public Semaphore semaphore;
        #endregion Members
        #region Constructors
        public IRQSemaphore(string pName, Semaphore pSemaphore)
        {
            name = pName;
            semaphore = pSemaphore;
        }
        // EG 20190315 New version of Naming IRQ Semaphore
        // EG 20190318 New version of Naming IRQ Semaphore IRQ{REQUESTTYPE.IDTRK_L}{SERVERNAME}{DATABASENAME}
        public IRQSemaphore(string pName, IRQBase pIRQBase, Semaphore pSemaphore)
        {
            name = pName;
            requestDatabase = pIRQBase.RequestDatabase;
            requestId = pIRQBase.RequestId;
            userRequester = pIRQBase.UserRequester;
            applicationRequester = pIRQBase.ApplicationRequester;
            semaphore = pSemaphore;
        }
        #endregion Constructors
    }
    #endregion IRQSemaphore

    #region IRQTools
    /// <summary>
    /// Classe de méthodes outils de gestion des interruptions de traitement
    /// </summary>
    // EG 20180525 [23979] IRQ Processing
    public static class IRQTools
    {
        #region UserName
        /// <summary>
        /// Utilisateur et son domaine 
        /// utilisé pour les permissions sur les sémaphores nommées en mode DEBUG
        /// </summary>
        // EG 20180525 [23979] IRQ Processing
        private static string UserName
        {
            get { return Environment.UserDomainName + "\\" + Environment.UserName; }
        }
        #endregion UserName
        #region GlobalNamedSemaphore
        /// <summary>
        /// Nom complet de la sémaphore d'interruption : "Global\"+ Name
        /// </summary>
        /// <param name="pNamedSemaphore">Nom de la sémaphore</param>
        /// <returns></returns>
        // EG 20180525 [23979] IRQ Processing
        private static string GlobalNamedSemaphore(string pNamedSemaphore)
        {
            string globalNamedSemaphore = pNamedSemaphore;
            if (false == globalNamedSemaphore.StartsWith("Global"))
                globalNamedSemaphore = "Global\\" + pNamedSemaphore;
            return globalNamedSemaphore;
        }
        #endregion GlobalNamedSemaphore

        /// <summary>
        /// 
        /// </summary>
        private static List<IRQSemaphore> LstIRQSemaphore { get => (AppInstance.MasterAppInstance as AppInstanceService).LstIRQSemaphore; }

        /// <summary>
        /// Test l'existence d'une sémaphore d'interruption de traitement
        /// dans la collection du service
        /// </summary>
        /// <param name="pNamedSemaphore">Nom de la sémaphore (avec prefixe Global)</param>
        /// <returns></returns>
        public static Boolean IsNamedSemaphoreAlreadyExist(string pNamedSemaphore)
        {
            return LstIRQSemaphore.Exists(item => item.name == pNamedSemaphore);
        }


        #region IRQDatabase
        // EG 20190315 New version of Naming IRQ Semaphore IRQ{IDTRK_L}{SERVERNAME}{DATABASENAME}
        // EG 20190318 New version of Naming IRQ Semaphore IRQ{REQUESTTYPE.IDTRK_L}{SERVERNAME}{DATABASENAME}
        public static string IRQDatabase(string pCS)
        {
            CSManager csManager = new CSManager(pCS);
            // AL 20240705 [WI996] Retrieves the full ServerName if available for the CS
            string serverName = csManager.GetSvrName(false);
            return "{" + (serverName == "." ? "Local" : serverName.Replace(@"\", ".")) + "}{" + csManager.GetDbName() + "}"; ;
        }
        #endregion IRQDatabase
        #region CreateNamedSemaphore
        /// <summary>
        /// Création d'une sémaphore nommée (Gestion Interruption de traitement)
        /// utilisé par le service NormMsgFactory sur un message IRQ
        /// </summary>
        /// <param name="tracker"></param>
        /// <param name="setErrorWarning"></param>
        /// <param name="pIRQRequester">Infos du demandeur</param>
        // EG 20180525 [23979] IRQ Processing
        // EG 20190114 Add detail to ProcessLog Refactoring
        // EG 20240108 [26624] Upd or Del ShowSecurity
        public static Cst.IRQLevel CreateNamedSemaphore(Tracker tracker, SetErrorWarning setErrorWarning, IRQRequester pIRQRequester)
        {
            Cst.IRQLevel ret = Cst.IRQLevel.REQUESTED;
            string globalNamedSemaphore = GlobalNamedSemaphore(pIRQRequester.NamedSystemSemaphore);

            if (IsNamedSemaphoreAlreadyExist(globalNamedSemaphore))
            {
                if (null != setErrorWarning)
                    // FI 20200623 [XXXXX] call SetErrorWarning
                    setErrorWarning.Invoke(ProcessStateTools.StatusWarningEnum);

                // La sémaphore est déjà présente dans la collection, cette  nouvelle demande d'interruption est donc rejetée.
                Logger.Log(new LoggerData(LogLevelEnum.Warning, new SysMsgCode(SysCodeEnum.LOG, 9906), 1));

                return Cst.IRQLevel.REJECTED;
            }

            Semaphore namedSemaphore = null;
            bool doesNotExist = false;
            bool unauthorized = false;
            try
            {
                // On essaie d'ouvrir la sémaphone nommée.
                namedSemaphore = Semaphore.OpenExisting(globalNamedSemaphore);
                AppInstance.TraceManager.TraceVerbose(nameof(IRQTools), $"NamedSemaphore {globalNamedSemaphore} exists.");
            }
            catch (WaitHandleCannotBeOpenedException)
            {
                // La sémaphore nommée n'existe pas;
                AppInstance.TraceManager.TraceVerbose(nameof(IRQTools), $"NamedSemaphore {globalNamedSemaphore} doesn't exists.");
                doesNotExist = true;
            }
            catch (UnauthorizedAccessException)
            {
                // La sémaphore nommée existe mais accès non autorisé;
                AppInstance.TraceManager.TraceVerbose(nameof(IRQTools), $"NamedSemaphore {globalNamedSemaphore} exists but unauthorized access.");
                unauthorized = true;
            }

            if (doesNotExist)
            {
                #region La sémaphore n'existe pas
                // Création de la sémaphore avec les règles d'accès
                namedSemaphore = new Semaphore(1, 1, globalNamedSemaphore, out bool semaphoreWasCreated, IRQTools.CreateAccessControlList());

                if (false == semaphoreWasCreated)
                    throw new InvalidOperationException($"Failed to create NamedSemaphore {globalNamedSemaphore}." );

                AppInstance.TraceManager.TraceInformation(nameof(IRQTools), $"NamedSemaphore {globalNamedSemaphore} created.");
                // Affichage des droits d'accès dans la trace
                ShowSecurity(namedSemaphore.GetAccessControl());
                ret = Cst.IRQLevel.EXECUTED;
                #endregion La sémaphore n'existe pas
            }
            else if (unauthorized)
            {
                #region La sémaphore existe mais pas de droit d'accès
                try
                {
                    // Ouverture de la sémaphore avec droit d'accès à lecture et changement de permission
                    namedSemaphore = Semaphore.OpenExisting(globalNamedSemaphore, SemaphoreRights.ReadPermissions | SemaphoreRights.ChangePermissions);
                    // Modification des permissions d'accès
                    IRQTools.ChangeAccessControlList(namedSemaphore);
                    // Réouverture de la sémaphore
                    namedSemaphore = Semaphore.OpenExisting(globalNamedSemaphore);
                    AppInstance.TraceManager.TraceVerbose(nameof(IRQTools), $"NamedSemaphore {globalNamedSemaphore} is opened.");
                    ret = Cst.IRQLevel.EXECUTED;
                }
                catch (UnauthorizedAccessException ex)
                {
                    AppInstance.TraceManager.TraceVerbose(nameof(IRQTools), $"NamedSemaphore {globalNamedSemaphore} Exception : UnauthorizedAccessException. Message {ExceptionTools.GetMessageExtended(ex)}.");
                    ret = Cst.IRQLevel.REJECTED;
                }
                catch (Exception ex)
                {
                    AppInstance.TraceManager.TraceVerbose(nameof(IRQTools), $"NamedSemaphore {globalNamedSemaphore} Exception. Message {ExceptionTools.GetMessageExtended(ex)}.");
                    ret = Cst.IRQLevel.REJECTED;
                }
                #endregion La sémaphore existe mais pas de droit d'accès
            }

            // La sémaphore est créée
            if ((null != namedSemaphore) &&
                (false == LstIRQSemaphore.Exists(item => item.name == globalNamedSemaphore)))
            {
                // Alimentation sur la ligne du Tracker NORMSGFACTORY (Message IRQ) de l'IDTRK_L du traitement à interrompre
                tracker.IrqIdTRK_L = pIRQRequester.TrackerIdProcess;
                // Ajout dans la collection de la classe IRQSemaphore (pour la sémaphore nouvellement créée)
                LstIRQSemaphore.Add(new IRQSemaphore(globalNamedSemaphore, pIRQRequester, namedSemaphore));
            }
            return ret;
        }
        #endregion CreateNamedSemaphore
        #region GetCurrentIRQSemaphore
        /// <summary>
        /// Récupère la classe IRQSemaphore dans la collection
        /// </summary>
        /// <param name="pProcess">ProcessBase appelant</param>
        /// <param name="pNamedSemaphore">Nom de base de la sémaphore d'interruption</param>
        /// <returns></returns>
        // EG 20180525 [23979] IRQ Processing
        public static IRQSemaphore GetCurrentIRQSemaphore(string pNamedSemaphore)
        {
            string globalNamedSemaphore = IRQTools.GlobalNamedSemaphore(pNamedSemaphore);
            return LstIRQSemaphore.FirstOrDefault(item => item.name == globalNamedSemaphore);
        }
        #endregion GetCurrentIRQSemaphore
        #region GetNamedSemaphore
        /// <summary>
        /// Recherche la présence d'une sémaphore de demande d'interruption
        /// </summary>
        /// <param name="cs"></param>
        /// <param name="idTRK_L"></param>
        /// <param name="pNamedSemaphore">Nom de base de la sémaphore d'interruption</param>
        /// <returns>Résultat de la recherche de type Cst.IRQLevel (NAMEDSEMAPHORE_NOTEXISTS|NAMEDSEMAPHORE_EXISTS|NAMEDSEMAPHORE_UNAUTHORIZED) </returns>
        // EG 20180525 [23979] IRQ Processing
        public static Cst.IRQLevel GetNamedSemaphore(string cs, int idTRK_L, string pNamedSemaphore)
        {
            Cst.IRQLevel ret = Cst.IRQLevel.NAMEDSEMAPHORE_NOTEXISTS;
            string globalNamedSemaphore = IRQTools.GlobalNamedSemaphore(pNamedSemaphore);

            // La sémaphore est déjà présente dans la collection, pas la peine de faire une nouvelle ouverture 
            if (IsNamedSemaphoreAlreadyExist(globalNamedSemaphore))
                return Cst.IRQLevel.NAMEDSEMAPHORE_EXISTS;

            Semaphore namedSemaphore = null;
            bool unauthorized = false;

            try
            {
                // On essaie d'ouvrir la sémaphone nommée.
                namedSemaphore = Semaphore.OpenExisting(globalNamedSemaphore);
                AppInstance.TraceManager.TraceInformation(nameof(IRQTools), $"NamedSemaphore {globalNamedSemaphore} exists.");
                ret = Cst.IRQLevel.NAMEDSEMAPHORE_EXISTS;
            }
            catch (WaitHandleCannotBeOpenedException ex)
            {
                // La sémaphore n'existe pas : RAS
                AppInstance.TraceManager.TraceVerbose(nameof(IRQTools), $"NamedSemaphore {globalNamedSemaphore} doesn't exists.");
                AppInstance.TraceManager.TraceVerbose(nameof(IRQTools), $"WaitHandleCannotBeOpenedException  {ex.Message}.");
            }
            catch (UnauthorizedAccessException)
            {
                // La seémaphore existe mais son accès est non autorisé;
                AppInstance.TraceManager.TraceVerbose(nameof(IRQTools), $"NamedSemaphore {globalNamedSemaphore} exists but UnauthorizedAccessException.");
                unauthorized = true;
            }

            if (unauthorized)
            {
                try
                {
                    // on ouvre la sémaphore avec les permissions de lecture et modification
                    namedSemaphore = Semaphore.OpenExisting(globalNamedSemaphore, SemaphoreRights.ReadPermissions | SemaphoreRights.ChangePermissions);
                    IRQTools.ChangeAccessControlList(namedSemaphore);
                    namedSemaphore = Semaphore.OpenExisting(globalNamedSemaphore);
                    AppInstance.TraceManager.TraceInformation(nameof(IRQTools), $"NamedSemaphore {globalNamedSemaphore} is opened.");
                    ret = Cst.IRQLevel.NAMEDSEMAPHORE_EXISTS;
                }
                catch (UnauthorizedAccessException ex)
                {
                    AppInstance.TraceManager.TraceVerbose(nameof(IRQTools), $"NamedSemaphore {globalNamedSemaphore} Exception : UnauthorizedAccessException. Message {ExceptionTools.GetMessageExtended(ex)}.");
                    ret = Cst.IRQLevel.NAMEDSEMAPHORE_UNAUTHORIZED;
                }
                catch (Exception ex)
                {
                    AppInstance.TraceManager.TraceVerbose(nameof(IRQTools), $"NamedSemaphore {globalNamedSemaphore} Exception. Message {ExceptionTools.GetMessageExtended(ex)}.");
                    ret = Cst.IRQLevel.NAMEDSEMAPHORE_UNAUTHORIZED;
                }
            }

            if ((Cst.IRQLevel.NAMEDSEMAPHORE_EXISTS == ret) && (false == IsNamedSemaphoreAlreadyExist(globalNamedSemaphore)))
            {
                // La sémaphore existe
                // Ajout dans la collection de la classe IRQSemaphore (pour la sémaphore nouvellement créée)
                // Vue côté answerer (soit traitement à interrompre)
                IRQAnswerer irqAnswerer = new IRQAnswerer(cs, idTRK_L);
                LstIRQSemaphore.Add(new IRQSemaphore(globalNamedSemaphore, irqAnswerer, namedSemaphore));
            }
            return ret;
        }
        #endregion GetNamedSemaphore

        /// <summary>
        /// Retourne l'utilisateur (avec Id) et la date de la demande d'interruption du traitement
        /// </summary>
        /// <param name="pProcess">traitement</param>
        /// <param name="pNamedSemaphore">Nom de la sémaphore liée au traitement</param>
        /// <returns></returns>
        // EG 20220221 [XXXXX] New
        public static Pair<string, string> GetRequestedCurrentIRQSemaphore(string pNamedSemaphore)
        {
            IRQSemaphore irqSemaphore = IRQTools.GetCurrentIRQSemaphore( pNamedSemaphore);
            string requestedBy = LogTools.IdentifierAndId(irqSemaphore.userRequester.Item2, irqSemaphore.userRequester.Item1);
            string requestedAt = DtFunc.DateTimeToStringISO(irqSemaphore.userRequester.Item3);
            if (irqSemaphore.userRequester.Item3.Kind == DateTimeKind.Utc)
                requestedAt += " Etc/UTC";
            return new Pair<string, string>(requestedBy,requestedAt);
        }
        #region IsIRQManaged
        /// <summary>
        /// Permet de savoir si une autorisation d'interruption de traitement 
        /// est opérationnelle et donc gérée par l'applicatif.
        /// S'appuie sur le Groupe du tracvker et le SysNumber du traitement à interrompre
        /// permet de lire la ligne d'attribut de Cst.TrackerSystemMsgAttribute pour le traitement (SysNumber) demandé
        /// afin de savoir sir l'interruption est gérée (attribute.IRQManaged)
        /// </summary>
        /// <param name="pGroupTracker">Groupe de la ligne TRACKER du traitement à interrompre</param>
        /// <param name="pSysNumber">Numéro de la ligne TRACKER du traitement à interrompre (TRK|SYSCODE => SYSTEMMSG)</param>
        // EG 20180525 [23979] IRQ Processing
        public static bool IsIRQManaged(string pGroupTracker, int pSysNumber)
        {
            bool isManaged = false;
            if (Enum.IsDefined(typeof(Cst.GroupTrackerEnum), pGroupTracker.ToUpper()))
            {
                Cst.GroupTrackerEnum groupTracker = (Cst.GroupTrackerEnum)Enum.Parse(typeof(Cst.GroupTrackerEnum), pGroupTracker, true);
                Pair<string, Cst.TrackerSystemMsgAttribute> systemMsg;
                if (groupTracker == Cst.GroupTrackerEnum.CLO)
                {
                    systemMsg = GetTrackerMsgAttribute<Cst.PosRequestTypeEnum>(pSysNumber);
                    if (null == systemMsg)
                        systemMsg = GetTrackerMsgAttribute<Cst.ProcessTypeEnum>(pSysNumber);
                }
                else if (groupTracker == Cst.GroupTrackerEnum.IO)
                    systemMsg = GetTrackerMsgAttribute<Cst.In_Out>(pSysNumber);
                else
                    systemMsg = GetTrackerMsgAttribute<Cst.ProcessTypeEnum>(pSysNumber);

                if (null != systemMsg)
                    isManaged = systemMsg.Second.IRQManaged;
            }
            return isManaged;
        }
        #endregion IsIRQManaged

        #region GetTrackerMsgAttribute
        /// <summary>
        /// Retourne les éléments SYSTEMMSG du traitement à interrompre
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="pSysNumber"></param>
        /// <returns></returns>
        // EG 20180525 [23979] IRQ Processing
        // EG 20190318 New version of Naming IRQ Semaphore IRQ{REQUESTTYPE.IDTRK_L}{SERVERNAME}{DATABASENAME}
        public static Pair<string, Cst.TrackerSystemMsgAttribute> GetTrackerMsgAttribute(Cst.GroupTrackerEnum pGroup, int pSysNumber)
        {
            Pair<string, Cst.TrackerSystemMsgAttribute> ret;
            if (pGroup == Cst.GroupTrackerEnum.CLO)
            {
                ret = IRQTools.GetTrackerMsgAttribute<Cst.PosRequestTypeEnum>(pSysNumber);
                if (null == ret)
                    ret = IRQTools.GetTrackerMsgAttribute<Cst.ProcessTypeEnum>(pSysNumber);
            }
            else if (pGroup == Cst.GroupTrackerEnum.IO)
                ret = IRQTools.GetTrackerMsgAttribute<Cst.In_Out>(pSysNumber);
            else
                ret = IRQTools.GetTrackerMsgAttribute<Cst.ProcessTypeEnum>(pSysNumber);

            return ret;
        }
        public static Pair<string, Cst.TrackerSystemMsgAttribute> GetTrackerMsgAttribute<T>(int pSysNumber) where T : struct
        {
            Pair<string, Cst.TrackerSystemMsgAttribute> ret = null;
            IEnumerable<T> lstEnum = from item in Enum.GetValues(typeof(T)).Cast<T>() select item;
            foreach (T @value in lstEnum)
            {
                Cst.TrackerSystemMsgAttribute attribute = ReflectionTools.GetAttribute<Cst.TrackerSystemMsgAttribute>(typeof(T), @value.ToString());
                if ((null != attribute) && (pSysNumber == attribute.SysNumber))
                {
                    if (ReflectionTools.ConvertStringToEnumOrNullable<Cst.IOElementType>(@value.ToString()).HasValue)
                        ret = new Pair<string, Cst.TrackerSystemMsgAttribute>(Cst.ProcessTypeEnum.IO.ToString(), attribute);
                    else
                        ret = new Pair<string, Cst.TrackerSystemMsgAttribute>(@value.ToString(), attribute);
                    break;
                }
            }
            return ret;
        }
        #endregion GetTrackerMsgAttribute

        #region IsIRQRequested
        /// <summary>
        /// Retourne si une demande d'interruption du traitement a été activée ou est en cours de traitement.
        /// Recherche existence d'une sémaphore avec pour nom : pNamedSemaphore
        /// </summary>
        /// <param name="pProcess">ProcessBase appelant</param>
        /// <param name="pNamedSemaphore">Nom de base de la sémaphore d'interruption</param>
        /// <param name="pCodeReturn">code retour = Cst.ErrLevel.IRQ_EXECUTED si true</param>
        // EG 20180525 [23979] IRQ Processing
        public static bool IsIRQRequested(ProcessBase pProcess, string pNamedSemaphore, ref Cst.ErrLevel pCodeReturn)
        {
            bool isIRQ = (Cst.ErrLevel.IRQ_EXECUTED == pCodeReturn);
            if (false == isIRQ)
            {
                isIRQ = IsIRQRequested(pProcess, pNamedSemaphore);
                if (isIRQ)
                    pCodeReturn = Cst.ErrLevel.IRQ_EXECUTED;

            }
            return isIRQ;
        }
        /// <summary>
        /// Retourne si une demande d'interruption du traitement a été activée.
        /// Recherche existence d'une sémaphore avec pour nom : pNamedSemaphore
        /// </summary>
        /// <param name="pProcess">ProcessBase appelant</param>
        /// <param name="pNamedSemaphore">Nom de base de la sémaphore d'interruption</param>
        // EG 20180525 [23979] IRQ Processing
        // EG 20240108 [26624] Del Test on NAMEDSEMAPHORE_UNAUTHORIZED
        public static bool IsIRQRequested(ProcessBase pProcess, string pNamedSemaphore)
        {
            Cst.IRQLevel irqLevel = IRQTools.GetNamedSemaphore(pProcess.Cs, pProcess.Tracker.IdTRK_L, pNamedSemaphore);
            return (irqLevel == Cst.IRQLevel.NAMEDSEMAPHORE_EXISTS);
        }
        // EG 20190308 New
        public static bool IsIRQRequested(ProcessBase pProcess, string pNamedSemaphore, ref ProcessState pProcessState)
        {
            bool isIRQ = (Cst.ErrLevel.IRQ_EXECUTED == pProcessState.CodeReturn);
            if (false == isIRQ)
            {
                isIRQ = IsIRQRequested(pProcess, pNamedSemaphore);
                if (isIRQ)
                    pProcessState.SetInterrupt();
            }
            return isIRQ;
        }
        /// <summary>
        /// Retourne True si une demande d'interruption a été initiée via NormMsgFactory
        /// Alimentation du Log si cette demande est traité pour la première fois (avec utilisateur et date)
        /// </summary>
        /// <param name="pProcess">traitement</param>
        /// <param name="pNamedSemaphore">Nom de la sémaphore liée au traitement</param>
        /// <param name="pCodeReturn">true/false</param>
        /// <returns></returns>
        // EG 20220221 [XXXXX] New
        public static bool IsIRQRequestedWithLog(ProcessBase pProcess, string pNamedSemaphore, ref Cst.ErrLevel pCodeReturn)
        {
            bool isIRQ = (Cst.ErrLevel.IRQ_EXECUTED == pCodeReturn);
            if (false == isIRQ)
            {
                isIRQ = IsIRQRequested(pProcess, pNamedSemaphore);
                if (isIRQ)
                {
                    Pair<string, string> requestedInfo = IRQTools.GetRequestedCurrentIRQSemaphore(pProcess.IRQNamedSystemSemaphore);

                    Logger.Log(new LoggerData(LogLevelEnum.Warning, new SysMsgCode(SysCodeEnum.LOG, 6043), 2,
                        new LogParam(requestedInfo.First),
                        new LogParam(requestedInfo.Second)));
                    pCodeReturn = Cst.ErrLevel.IRQ_EXECUTED;
                }
            }
            return isIRQ;
        }
        #endregion IsIRQRequested

        //-----------------------------------------------------
        // SECURITE : CONTRÔLES D'ACCES A UNE SEMAPHORE NOMMEE
        //-----------------------------------------------------

        #region AddAccessRule
        /// <summary>
        /// Ajout de règles d'accès à la sécurité des contrôles d'accès appliquée à une sémaphore IRQ 
        /// </summary>
        /// <param name="pSemSecurity">Sécurité des contrôles d'accès</param>
        /// <param name="pSemaphoreRights">Droits d'accès à la sémaphore</param>
        /// <param name="pAccessControlType">Contrôle d'accès à la sémaphore</param>
        // EG 20180525 [23979] IRQ Processing
        private static void AddAccessRule(SemaphoreSecurity pSemSecurity, SemaphoreRights pSemaphoreRights, AccessControlType pAccessControlType)
        {
            pSemSecurity.AddAccessRule(new SemaphoreAccessRule(new SecurityIdentifier(WellKnownSidType.WorldSid, null), pSemaphoreRights, pAccessControlType));
#if DEBUG
            pSemSecurity.AddAccessRule(new SemaphoreAccessRule(IRQTools.UserName, pSemaphoreRights, pAccessControlType));
#endif
        }
        #endregion AddAccessRule
        #region ChangeAccessControlList
        /// <summary>
        /// Changement des droit d'accès après ouverture de sémaphore (côté Traitement à interrompre)
        /// pour les SID de type : WellKnownSidType.LocalSystemSid
        /// et si Mode Debug     : IRQTools.UserName
        /// 
        /// Avant : SemaphoreRights.Synchronize | SemaphoreRights.Modify, AccessControlType.Deny
        /// Après : SemaphoreRights.Synchronize | SemaphoreRights.Modify, AccessControlType.Allow
        /// </summary>
        /// <param name="pSemaphore">Sémaphore ouverte mais sans les droits d'accès</param>
        // EG 20180525 [23979] IRQ Processing
        // EG 20240108 [26624] Upd ShowSecurity
        private static void ChangeAccessControlList(Semaphore pSemaphore)
        {
            AppInstance.TraceManager.TraceVerbose(nameof(IRQTools), String.Format("ChangeAccessControlList()"));
            AppInstance.TraceManager.TraceVerbose(nameof(IRQTools), String.Format("NamedSemaphore GetAccessControl()"));
            SemaphoreSecurity semSecurity = pSemaphore.GetAccessControl();
            if (null != semSecurity)
            {
                ShowSecurity(semSecurity);
                // Suppression des règles d'accès
                AppInstance.TraceManager.TraceVerbose(nameof(IRQTools), String.Format("NamedSemaphore RemoveAccessRule()"));
                IRQTools.RemoveAccessRule(semSecurity, SemaphoreRights.Synchronize | SemaphoreRights.Modify, AccessControlType.Deny);
                // Ajout des règles d'accès
                AppInstance.TraceManager.TraceVerbose(nameof(IRQTools), String.Format("NamedSemaphore AddAccessRule()"));
                IRQTools.AddAccessRule(semSecurity, SemaphoreRights.Synchronize | SemaphoreRights.Modify, AccessControlType.Allow);
                // Mise à jour des règles d'accès
                AppInstance.TraceManager.TraceVerbose(nameof(IRQTools), String.Format("NamedSemaphore SetAccessControl()"));
                pSemaphore.SetAccessControl(semSecurity);
                ShowSecurity(pSemaphore.GetAccessControl());
            }
        }
        #endregion ChangeAccessControlList
        #region CreateAccessControlList
        /// <summary>
        /// Construction des droits d'accès à la création d'une sémaphore (côté NormMsgFactory)
        /// La sémaphore est créée par NormMsgFactory avec :
        /// SemaphoreRights.Synchronize | SemaphoreRights.Modify, AccessControlType.Deny
        /// SemaphoreRights.ReadPermissions | SemaphoreRights.ChangePermissions, AccessControlType.Allow
        /// pour les SID de type : WellKnownSidType.LocalSystemSid
        /// et si Mode Debug     : IRQTools.UserName
        /// </summary>
        /// <returns></returns>
        // EG 20180525 [23979] IRQ Processing
        private static SemaphoreSecurity CreateAccessControlList()
        {
            SemaphoreSecurity semSecurity = new SemaphoreSecurity();
            IRQTools.AddAccessRule(semSecurity, SemaphoreRights.Synchronize | SemaphoreRights.Modify, AccessControlType.Deny);
            IRQTools.AddAccessRule(semSecurity, SemaphoreRights.ReadPermissions | SemaphoreRights.ChangePermissions, AccessControlType.Allow);
            return semSecurity;
        }
        #endregion CreateAccessControlList

        #region RemoveAccessRule
        /// <summary>
        /// Suppression de règles d'accès à la sécurité des contrôles d'accès appliquée à une sémaphore IRQ 
        /// </summary>
        /// <param name="pSemSecurity">Sécurité des contrôles d'accès</param>
        /// <param name="pSemaphoreRights">Droits d'accès à la sémaphore</param>
        /// <param name="pAccessControlType">Contrôle d'accès à la sémaphore</param>
        // EG 20180525 [23979] IRQ Processing
        private static void RemoveAccessRule(SemaphoreSecurity pSemSecurity, SemaphoreRights pSemaphoreRights, AccessControlType pAccessControlType)
        {
            pSemSecurity.RemoveAccessRule(new SemaphoreAccessRule(new SecurityIdentifier(WellKnownSidType.WorldSid, null), pSemaphoreRights, pAccessControlType));
#if DEBUG
            pSemSecurity.RemoveAccessRule(new SemaphoreAccessRule(IRQTools.UserName, pSemaphoreRights, pAccessControlType));
#endif

        }
        #endregion RemoveAccessRule
        #region ShowSecurity
        /// <summary>
        /// Affichage dans la trace des règles d'accès présente sur une sémaphore nommée
        /// </summary>
        /// <param name="pSemSecurity">Access control windows for semaphore</param>
        private static void ShowSecurity(SemaphoreSecurity pSemSecurity)
        {
            AppInstance.TraceManager.TraceVerbose(nameof(IRQTools), "Current access rules");

            foreach (SemaphoreAccessRule ar in pSemSecurity.GetAccessRules(true, true, typeof(SecurityIdentifier)))
            {
                AppInstance.TraceManager.TraceVerbose(nameof(IRQTools), $"User: {ar.IdentityReference}. Type: {ar.AccessControlType}. Rights: {ar.SemaphoreRights}");
            }
#if DEBUG
            foreach (SemaphoreAccessRule ar in pSemSecurity.GetAccessRules(true, true, typeof(NTAccount)))
            {
                AppInstance.TraceManager.TraceVerbose(nameof(IRQTools), $"User: {ar.IdentityReference}.Type: {ar.AccessControlType}.Rights: {ar.SemaphoreRights}");
            }
#endif
        }
        #endregion ShowSecurity
    }
    #endregion IRQTools
}
