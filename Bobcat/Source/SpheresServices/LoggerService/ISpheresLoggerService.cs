using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Threading;

namespace EFS.LoggerService
{
    /// <summary>
    /// Fonctionnalités du service de journal
    /// </summary>
    //[ServiceContract(SessionMode = SessionMode.Required)]
    [ServiceContract]
    public interface ISpheresLoggerService
    {
        /// <summary>
        /// Indique que le service est actif
        /// </summary>
        /// <returns></returns>
        [OperationContract]
        bool IsAlive();

        /// <summary>
        /// Commence un nouvel ensemble de journaux
        /// </summary>
        /// <returns></returns>
        [OperationContract]
        void BeginScope(LogScope pScope);

        /// <summary>
        /// Termine un ensemble de journaux
        /// </summary>
        /// <returns></returns>
        [OperationContract]
        void EndScope(LogScope pScope);

        /// <summary>
        /// Obtient les infos d'un ensemble de journaux à partir d'un IpProcess (de PROCESS_L) s'il existe déjà
        /// </summary>
        /// <param name="pConnexionString"></param>
        /// <param name="pIdProcess"></param>
        /// <returns></returns>
        [OperationContract]
        LogScope GetLogScope(string pConnexionString, int pIdProcess);

        /// <summary>
        /// Ajout un élément de journal
        /// </summary>
        /// <param name="pData"></param>
        /// <returns></returns>
        [OperationContract]
        void Log(LoggerData pData);

        /// <summary>
        /// Mettre à jour le LogLevel d'un élément de journal
        /// </summary>
        /// <param name="pData"></param>
        /// <param name="pNewLogLevel"></param>
        /// <returns></returns>
        [OperationContract]
        bool UpdateLogLevel(LoggerData pData, LogLevelEnum pNewLogLevel);

        /// <summary>
        /// Mettre à jour le LogLevel du premier et dernier élément de journal avec le pire LogLevel des autres éléments
        /// </summary>
        /// <param name="pDataFirst"></param>
        /// <param name="pDataLast"></param>
        /// <returns></returns>
        [OperationContract]
        bool UpdateWithWorstLogLevel(LoggerData pDataFirst, LoggerData pDataLast);

        /// <summary>
        /// Obtient le statut de l'écriture automatique du journal
        /// </summary>
        /// <param name="pScope"></param>
        /// <returns></returns>
        [OperationContract]
        bool GetAutoWrite(LogScope pScope);

        /// <summary>
        /// Activation ou désactivation de l'écriture automatique du journal
        /// </summary>
        /// <param name="pScope"></param>
        /// <param name="pAutoWrite"></param>
        /// <returns></returns>
        [OperationContract]
        bool SetAutoWrite(LogScope pScope, bool pAutoWrite);

        /// <summary>
        /// Ecrire tous les journaux d'un LogScope
        /// </summary>
        /// <param name="pScope"></param>
        /// <param name="pIsForced">Indique si l'écriture doit être forcée</param>
        /// <returns></returns>
        [OperationContract]
        void Write(LogScope pScope, bool pIsForced = false);

        /// <summary>
        /// Obtient tous les journaux en mémoire pour un LogScope
        /// </summary>
        /// <param name="pScope"></param>
        /// <returns></returns>
        [OperationContract]
        ResultLoggerData GetBufferLog(LogScope pScope);

        /// <summary>
        /// Obtient le dernier journal reçu pour un LogScope
        /// </summary>
        /// <param name="pScope"></param>
        /// <returns></returns>
        [OperationContract]
        ResultLoggerData GetLastLog(LogScope pScope);

        /// <summary>
        /// Obtient les journaux en fonction de critères
        /// </summary>
        /// <param name="pRequest"></param>
        /// <returns></returns>
        [OperationContract]
        ResultLoggerData GetLog(LoggerRequest pRequest);

        /// <summary>
        /// Obtient l'Id interne lors d'une écriture dans PROCESS_L en mode compatibilité ou 0 si non présent dans PROCESS_L
        /// </summary>
        /// <param name="pScope"></param>
        /// <returns></returns>
        [OperationContract]
        int GetIdProcess_L(LogScope pScope);

        /// <summary>
        /// Obtient le pire LogLevel du scope parmi Info, Warning, Error et Critical présent aprés la réception du journal pData (pour compatibilité avec IO)
        /// </summary>
        /// <param name="pScope"></param>
        /// <param name="pData"></param>
        /// <returns></returns>
        [OperationContract]
        LogLevelEnum GetWorstLogLevel(LogScope pScope, LoggerData pData);
    }

    /// <summary>
    /// Niveau de gravité du journal
    /// </summary>
    [DataContract(Name = "LogLevelEnum")]
    public enum LogLevelEnum
    {
        /// <summary>
        /// Tous les journaux
        /// </summary>
        [EnumMember]
        Trace = 0,
        /// <summary>
        /// Tous les journaux à partir de niveau Debug
        /// </summary>
        [EnumMember]
        Debug = 1,
        /// <summary>
        /// Tous les jounaux à partir du niveau Information
        /// </summary>
        [EnumMember]
        Info = 2,
        /// <summary>
        /// Warning et tous types d'erreurs
        /// </summary>
        [EnumMember]
        Warning = 3,
        /// <summary>
        /// Erreurs fonctionelles et systèmes
        /// </summary>
        [EnumMember]
        Error = 4,
        /// <summary>
        /// Uniquement les erreurs systèmes
        /// </summary>
        [EnumMember]
        Critical = 5,
        /// <summary>
        /// Auncun journal
        /// </summary>
        [EnumMember]
        None = 6,
    }

    /// <summary>
    /// Catégorie du journal
    /// </summary>
    [DataContract(Name = "LogCategoryEnum")]
    public enum LogCategoryEnum
    {
        [EnumMember]
        NA,
        [EnumMember]
        MissingPrice,
    }

    /// <summary>
    /// SYSCODE de SYSTEMMSG
    /// </summary>
    // PM 20220930 [XXXXX] Ajout valeur CME
    [DataContract(Name = "SysCodeEnum")]
    public enum SysCodeEnum
    {
        [EnumMember]
        LOG,
        [EnumMember]
        RES,
        [EnumMember]
        SYS,
        [EnumMember]
        TRK,
        [EnumMember]
        CME,
    }

    /// <summary>
    /// Données pour SYSTEMMSG
    /// </summary>
    [DataContract(Name = "SysMsgCode")]
    public class SysMsgCode : IEquatable<SysMsgCode>
    {
        #region Members
        private SysCodeEnum m_SysCode = SysCodeEnum.LOG;
        private int m_SysNumber = 0;
        #endregion Members

        #region Accessors
        /// <summary>
        /// SYSCODE
        /// </summary>
        [DataMember]
        public SysCodeEnum SysCode
        {
            get { return m_SysCode; }
            set { m_SysCode = value; }
        }

        /// <summary>
        /// SYSNUMBER
        /// </summary>
        [DataMember]
        public int SysNumber
        {
            get { return m_SysNumber; }
            set { m_SysNumber = value; }
        }
        #endregion Accessors

        #region Methods
        /// <summary>
        ///  Méthode Equals d'objet
        /// </summary>
        /// <param name="pObj"></param>
        /// <returns></returns>
        public override bool Equals(object pObj)
        {
            if ((pObj == null) || (GetType() != pObj.GetType()))
            {
                return false;
            }
            else
            {
                SysMsgCode objAsSysMsgCode = pObj as SysMsgCode;
                return Equals(objAsSysMsgCode);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return SysCode.GetHashCode() ^ SysNumber.GetHashCode();
        }

        /// <summary>
        /// Opérateur égal de SysMsgCode
        /// </summary>
        /// <param name="pObjA"></param>
        /// <param name="pObjB"></param>
        /// <returns></returns>
        public static bool operator ==(SysMsgCode pObjA, SysMsgCode pObjB)
        {
            if ((object)pObjA == default)
            {
                return ((object)pObjB == default);
            }
            else
            {
                return pObjA.Equals(pObjB);
            }
        }

        /// <summary>
        /// Opérateur différent de SysMsgCode
        /// </summary>
        /// <param name="pObjA"></param>
        /// <param name="pObjB"></param>
        /// <returns></returns>
        public static bool operator !=(SysMsgCode pObjA, SysMsgCode pObjB)
        {
            if ((object)pObjA == default)
            {
                return !((object)pObjB == default);
            }
            else
            {
                return !pObjA.Equals(pObjB);
            }
        }

        /// <summary>
        /// Méthode Equals de SysMsgCode
        /// </summary>
        /// <param name="pSysMsgCode"></param>
        /// <returns></returns>
        public bool Equals(SysMsgCode pSysMsgCode)
        {
            if ((object)pSysMsgCode == default)
            {
                return false;
            }
            else
            {
                return (SysNumber.Equals(pSysMsgCode.SysNumber) && SysCode.Equals(pSysMsgCode.SysCode));
            }
        }
        #endregion Methods
    }

    /// <summary>
    /// Données concernant les Identifiants internes de référentiels ou autres données
    /// </summary>
    [DataContract(Name = "LogParam")]
    public class LogParam
    {
        #region Members
        private string m_Identifier = string.Empty;
        private bool m_IsReferentialIdentifier = false;
        private int m_InternalId = 0;
        private string m_Link = string.Empty;
        private string m_Referential = string.Empty;
        private string m_Data = string.Empty;
        private bool m_DataSpecified = false;
        #endregion Members

        #region Accessors
        /// <summary>
        /// Indique s'il s'agit d'un Identifier de referentiel ou pas
        /// </summary>
        [DataMember]
        public bool IsReferentialIdentifier
        {
            get { return m_IsReferentialIdentifier; }
            set { m_IsReferentialIdentifier = value; }
        }
        
        /// <summary>
        /// Identifier
        /// </summary>
        [DataMember]
        public string Identifier
        {
            get { return m_Identifier; }
            set { m_Identifier = value; }
        }
        
        /// <summary>
        /// Id interne de la donnée
        /// </summary>
        [DataMember]
        public int InternalId
        {
            get { return m_InternalId; }
            set { m_InternalId = value; }
        }

        /// <summary>
        /// Lien vers la donnée
        /// </summary>
        [DataMember]
        public string Link
        {
            get { return m_Link; }
            set { m_Link = value; }
        }

        /// <summary>
        /// Nom du referentiel
        /// </summary>
        [DataMember]
        public string Referential
        {
            get { return m_Referential; }
            set { m_Referential = value; }
        }

        /// <summary>
        /// Indique si une autre donnée est renseignée
        /// </summary>
        [DataMember]
        public bool DataSpecified
        {
            get { return m_DataSpecified; }
            set { m_DataSpecified = value; }
        }
        
        /// <summary>
        /// Autre donnée
        /// </summary>
        [DataMember]
        public string Data
        {
            get { return m_Data; }
            set { m_Data = value; }
        }
        #endregion Accessors
    }

    /// <summary>
    /// Données d'une requête d'éléments du journal
    /// </summary>
    [DataContract(Name = "LoggerRequest")]
    public class LoggerRequest
    {
        #region Members
        private DateTime m_DtUtcLogStart;
        private DateTime m_DtUtcLogEnd;
        private LogLevelEnum m_LogLevel;
        private string m_HostName;
        private string m_ProcessName;
        private string m_ProcessType;
        #endregion Members

        #region Accessors
        /// <summary>
        /// Date/Heure de début du journal
        /// </summary>
        [DataMember]
        public DateTime DtUtcLogStart
        {
            get { return m_DtUtcLogStart; }
            internal set { m_DtUtcLogStart = value; }
        }

        /// <summary>
        /// Date/Heure de fin du journal
        /// </summary>
        [DataMember]
        public DateTime DtUtcLogEnd
        {
            get { return m_DtUtcLogEnd; }
            internal set { m_DtUtcLogEnd = value; }
        }

        /// <summary>
        /// Niveau de gravité
        /// </summary>
        [DataMember]
        public LogLevelEnum LogLevel
        {
            get { return m_LogLevel; }
            set { m_LogLevel = value; }
        }

        /// <summary>
        /// Nom de la machine
        /// </summary>
        [DataMember]
        public string HostName
        {
            get { return m_HostName; }
            set { m_HostName = value; }
        }

        /// <summary>
        /// Nom du processus
        /// </summary>
        [DataMember]
        public string ProcessName
        {
            get { return m_ProcessName; }
            set { m_ProcessName = value; }
        }    

        /// <summary>
        /// Type du processus
        /// </summary>
        [DataMember]
        public string ProcessType
        {
            get { return m_ProcessType; }
            set { m_ProcessType = value; }
        }
        #endregion Accessors
    }

    /// <summary>
    /// Données d'identification d'un ensemble de log
    /// </summary>
    [DataContract(Name = "LogScope")]
    public class LogScope
    {
        #region Members
        private Guid m_LogScopeId;
        private Guid m_SubLogScopeId;
        private string m_ConnectionString;
        private string m_ProcessType;
        private string m_InitialProcessType;
        private int m_IdPROCESS_L; // Pour compatibilité PROCESS_L
        private int m_IdTRK_L; // Pour compatibilité PROCESS_L
        private string m_IdStProcess; // Statut final du process
        private DateTime m_DtStProcess; // Horaire du statut final
        private DateTime m_DtProcessEnd; // Horaire de fin du process ou des process dépendants
        #endregion Members

        #region Accessors
        /// <summary>
        /// Id interne principal d'un ensemble de log
        /// </summary>
        [DataMember]
        public Guid LogScopeId
        {
            get { return m_LogScopeId; }
            internal set { m_LogScopeId = value; }
        }
        /// <summary>
        /// Id interne d'un sous ensemble de log (log d'un process faisant suite à une autre)
        /// </summary>
        [DataMember]
        public Guid SubLogScopeId
        {
            get { return m_SubLogScopeId; }
            internal set { m_SubLogScopeId = value; }
        }
        /// <summary>
        /// Connection String
        /// </summary>
        [DataMember]
        public string ConnectionString
        {
            get { return m_ConnectionString; }
            internal set { m_ConnectionString = value; }
        }
        /// <summary>
        /// Type de process
        /// </summary>
        [DataMember]
        public string ProcessType
        {
            get { return m_ProcessType; }
            set { m_ProcessType = value; }
        }
        /// <summary>
        /// Type de process d'origine de la création de l'ensemble de journaux
        /// </summary>
        [DataMember]
        public string InitialProcessType
        {
            get { return m_InitialProcessType; }
            set { m_InitialProcessType = value; }
        }
        /// <summary>
        /// Id interne de PROCESS_L pour compatibilité PROCESS_L
        /// </summary>
        [DataMember]
        public int IdPROCESS_L
        {
            get { return m_IdPROCESS_L; }
            set { m_IdPROCESS_L = value; }
        }
        /// <summary>
        /// Id interne du traker pour compatibilité PROCESS_L
        /// </summary>
        [DataMember]
        public int IdTRK_L
        {
            get { return m_IdTRK_L; }
            set { m_IdTRK_L = value; }
        }
        /// <summary>
        /// Statut final du process
        /// </summary>
        [DataMember]
        public string IdStProcess
        {
            get { return m_IdStProcess; }
            set { m_IdStProcess = value; }
        }
        /// <summary>
        /// Horaire du statut final du process
        /// </summary>
        [DataMember]
        public DateTime DtStProcess
        {
            get { return m_DtStProcess; }
            set { m_DtStProcess = value; }
        }
        /// <summary>
        /// Horaire de fin du process ou des process dépendants
        /// </summary>
        [DataMember]
        public DateTime DtProcessEnd
        {
            get { return m_DtProcessEnd; }
            set { m_DtProcessEnd = value; }
        }
        #endregion Accessors
    }

    /// <summary>
    /// Données d'un élément du journal
    /// </summary>
    [DataContract(Name = "LoggerData")]
    public class LoggerData
    {
        #region Members
        private LogScope m_LogScope;
        private DateTime m_DtUtcLog;
        private int m_SequenceNumber;
        private int m_GroupSequenceNumber;
        private LogLevelEnum m_LogLevel;
        private string m_HostName = string.Empty;
        private string m_InstanceName = string.Empty;
        private string m_InstanceVersion = string.Empty;
        private int m_InstanceIda = 0;
        private string m_ProcessName = string.Empty;
        private string m_ProcessType = string.Empty;
        private int m_OS_PID = 0;
        private int m_ThreadID = 0;
        private string m_CallingMethod = string.Empty;
        private string m_CallingClassType = string.Empty;
        //
        private int m_SQL_SPID = 0;
        //
        private LogCategoryEnum m_LogCategory;
        private SysMsgCode m_SysMsg = default;
        private string m_Message = string.Empty;
        private LogParam[] mParam;
        //
        private int m_RankOrder = 0;
        #endregion Members

        #region Accessors
        /// <summary>
        /// Id interne d'un ensemble de log
        /// </summary>
        [DataMember]
        public LogScope LogScope
        {
            get { return m_LogScope; }
            internal set { m_LogScope = value; }
        }
        
        /// <summary>
        /// Date/Heure de l'élément du journal
        /// </summary>
        [DataMember]
        public DateTime DtUtcLog
        {
            get { return m_DtUtcLog; }
            internal set { m_DtUtcLog = value; }
        }

        /// <summary>
        /// Numéro de séquence du journal
        /// </summary>
        [DataMember]
        public int SequenceNumber
        {
            get { return m_SequenceNumber; }
            set { m_SequenceNumber = value; }
        }

        /// <summary>
        /// Numéro de séquence apparié du journal 
        /// </summary>
        [DataMember]
        public int GroupSequenceNumber
        {
            get { return m_GroupSequenceNumber; }
            internal set { m_GroupSequenceNumber = value; }
        }

        /// <summary>
        /// Niveau de gravité
        /// </summary>
        [DataMember]
        public LogLevelEnum LogLevel
        {
            get { return m_LogLevel; }
            set { m_LogLevel = value; }
        }

        /// <summary>
        /// Nom de la machine
        /// </summary>
        [DataMember]
        public string HostName
        {
            get { return m_HostName; }
            set { m_HostName = value; }
        }

        /// <summary>
        /// Nom de l'instance
        /// </summary>
        [DataMember]
        public string InstanceName
        {
            get { return m_InstanceName; }
            set { m_InstanceName = value; }
        }

        /// <summary>
        /// Version de l'instance
        /// </summary>
        [DataMember]
        public string InstanceVersion
        {
            get { return m_InstanceVersion; }
            set { m_InstanceVersion = value; }
        }

        /// <summary>
        /// Id interne de l'acteur à l'origine de l'instance
        /// </summary>
        [DataMember]
        public int InstanceIda
        {
            get { return m_InstanceIda; }
            set { m_InstanceIda = value; }
        }
        
        /// <summary>
        /// Nom du processus
        /// </summary>
        [DataMember]
        public string ProcessName
        {
            get { return m_ProcessName; }
            set { m_ProcessName = value; }
        }

        /// <summary>
        /// Type du processus
        /// </summary>
        [DataMember]
        public string ProcessType
        {
            get { return m_ProcessType; }
            set { m_ProcessType = value; }
        }
        
        /// <summary>
        /// Id du processus de l'OS
        /// </summary>
        [DataMember]
        public int OS_PID
        {
            get { return m_OS_PID; }
            set { m_OS_PID = value; }
        }

        /// <summary>
        /// Id du Thread
        /// </summary>
        [DataMember]
        public int ThreadID
        {
            get { return m_ThreadID; }
            set { m_ThreadID = value; }
        }

        /// <summary>
        /// Nom de la méthode ayant envoyé l'élément du journal
        /// </summary>
        [DataMember]
        public string CallingMethod
        {
            get { return m_CallingMethod; }
            set { m_CallingMethod = value; }
        }

        /// <summary>
        /// Type de la classe de la méthode ayant envoyé l'élément du journal
        /// </summary>
        [DataMember]
        public string CallingClassType
        {
            get { return m_CallingClassType; }
            set { m_CallingClassType = value; }
        }

        /// <summary>
        /// Id de la session du processus SQL
        /// </summary>
        [DataMember]
        public int SQL_SPID
        {
            get { return m_SQL_SPID; }
            set { m_SQL_SPID = value; }
        }

        /// <summary>
        /// Catégorie de journal
        /// </summary>
        [DataMember]
        public LogCategoryEnum LogCategory
        {
            get { return m_LogCategory; }
            set { m_LogCategory = value; }
        }

        /// <summary>
        /// Message des SYSTEMMSG
        /// </summary>
        [DataMember]
        public SysMsgCode SysMsg
        {
            get { return m_SysMsg; }
            set { m_SysMsg = value; }
        }

        /// <summary>
        /// Message text
        /// </summary>
        [DataMember]
        public string Message
        {
            get { return m_Message; }
            set { m_Message = value; }
        }

        /// <summary>
        /// Parameters
        /// </summary>
        [DataMember]
        public LogParam[] Parameters
        {
            get { return mParam; }
            set { mParam = value; }
        }

        /// <summary>
        /// Niveau hierarchique
        /// </summary>
        [DataMember]
        public int RankOrder
        {
            get { return m_RankOrder; }
            set { m_RankOrder = value; }
        }
        #endregion Accessors
    }

    /// <summary>
    /// Données du journal en réponse à une solicitation
    /// </summary>
    [DataContract(Name = "ResultLoggerData")]
    public class ResultLoggerData
    {
        #region Members
        private readonly List<LoggerData> m_Log;
        #endregion Members

        #region Accessors
        /// <summary>
        /// Ensemble des éléments du journal en retour
        /// </summary>
        [DataMember]
        public List<LoggerData> Log
        {
            get { return m_Log; }
        }
        #endregion Accessors

        #region Constructors
        /// <summary>
        /// Constructeur pour un seul élément du journal
        /// </summary>
        /// <param name="pLogData"></param>
        internal ResultLoggerData(LoggerData pLogData)
        {
            m_Log = new List<LoggerData>();
            if (pLogData != default(LoggerData))
            {
                m_Log.Add(pLogData);
            }
        }

        /// <summary>
        /// Constructeur pour un ensemble d'éléments du journal
        /// </summary>
        /// <param name="pLogData"></param>
        internal ResultLoggerData(IEnumerable<LoggerData> pLogData)
        {
            m_Log = new List<LoggerData>();
            if (pLogData != default(IEnumerable<LoggerData>))
            {
                m_Log.AddRange(pLogData);
            }
        }
        #endregion Constructors
    }
}
