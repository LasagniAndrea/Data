using System;
using System.Data;
using System.Reflection;
using System.Text;
using System.Data.Common;  
//
using EFS.ACommon;
using EFS.Common;
using EFS.ApplicationBlocks.Data;


namespace EFS.Common
{
    #region TypeLockObjectAttribute
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public sealed class TypeLockObject : Attribute
    {
        #region Members
        private string m_Category;
        #endregion Members
        //
        #region Accessors
        public string Category
        {
            get { return (m_Category); }
            set { m_Category = value; }
        }
        #endregion Accessors
    }
    #endregion TypeLockObjectAttribute

    #region public Enum TypeLockEnum
    /// <summary>
    /// Liste des Locks
    /// </summary>
    public enum TypeLockEnum
    {
        NA = -1,
        [TypeLockObject(Category = "TABLE")]
        EVENT = 1,
        [TypeLockObject(Category = "TABLE")]
        REPOSITORY = 4,
        SOFTWARE = 8,
        [TypeLockObject(Category = "TABLE")]
        TRADE = 16,
        [TypeLockObject(Category = "TABLE")]
        NETCONVENTION = 32,
        [TypeLockObject(Category = "TABLE")]
        NETDESIGNATION = 64,
        [TypeLockObject(Category = "TABLE")]
        IOTASK = 128,
        [TypeLockObject(Category = "TABLE")]
        STLMESSAGE = 256,
        [TypeLockObject(Category = "TABLE")]
        MCO = 512,
        [TypeLockObject(Category = "PROCESS")]
        OTC_INV_BROFEE_PROCESS_GENERATION = 1024,
        [TypeLockObject(Category = "PROCESS")]
        OTC_INV_BROFEE_PROCESS_VALIDATION = 2048,
        [TypeLockObject(Category = "PROCESS")]
        OTC_INV_BROFEE_PROCESS_CANCELLATION = 4096,
        [TypeLockObject(Category = "PROCESS")]
        OTC_PROCESS_POSKEEPING_CLEARINGBULK = 8192,
        [TypeLockObject(Category = "PROCESS")]
        OTC_PROCESS_POSKEEPING_UPDATEENTRY = 16384,
        [TypeLockObject(Category = "PROCESS")]
        OTC_PROCESS_POSKEEPING_EOD = 32768,
        [TypeLockObject(Category = "PROCESS")]
        OTC_PROCESS_POSKEEPING_UNCLEARING = 65536,
        [TypeLockObject(Category = "PROCESS")]
        OTC_PROCESS_POSKEEPING_POT = 131072,
        [TypeLockObject(Category = "PROCESS")]
        ENTITYMARKET = 262144,
        [TypeLockObject(Category = "PROCESS")]
        ENTITYCSS = 524288,
        [TypeLockObject(Category = "TABLE")]
        POSCOLLATERAL = 1048576,
        [TypeLockObject(Category = "TABLE")]
        ACTOR = 2097152,
        [TypeLockObject(Category = "PROCESS")]
        OTC_PROCESS_POSKEEPING_CLOSINGDAY = 4194304,
        [TypeLockObject(Category = "PROCESS")]
        OTC_PROCESS_POSKEEPING_CLEARINGSPEC = 8388608,
        [TypeLockObject(Category = "TABLE")]
        POSREQUEST = 16777216,
        POSKEEPINGKEY = 33554432,
        /// <summary>
        /// 
        /// </summary>
        /// FI 20201117 [24872] Add
        [TypeLockObject(Category = "TABLE")]
        POSACTIONDET = 67108864,
    }
	#endregion
	
	#region public class LockObject
	/// <summary>
	/// Class LockObject : Represente le type de lock (Trade,...) et son identifiant
	/// </summary>
    /// FI 20130527 [18662] m_ObjetId est de type string
    public class LockObject : IComparable 
    {
        #region Members
        /// <summary>
        /// Type PROCESS, TABLE ... 
        /// </summary>
        private TypeLockEnum m_ObjectType;	
        /// <summary>
        /// Id Object
        /// </summary>
        private string m_ObjetId;			    
        /// <summary>
        /// Identifier for display in Alert()
        /// </summary>
        private string m_ObjectIdentifier;	
        /// <summary>
        /// Mode de lock 
        /// Exclusive = X
        /// Shared = S9999
        /// </summary>
        private string m_LockMode;
        #endregion Members
        //
        #region Accessors
        /// <summary>
        /// Obtient ou définit le lock
        /// </summary>
        public TypeLockEnum ObjectType
        {
            get { return m_ObjectType; }
            set { m_ObjectType = value; }
        }
        /// <summary>
        /// Identifier Object
        /// </summary>
        public string ObjectIdentifier
        {
            get { return m_ObjectIdentifier; }
            set { m_ObjectIdentifier = value; }
        }
        /// <summary>
        /// Id Object
        /// </summary>
        /// FI 20130527 [18662] ObjectId est de type string
        public string ObjectId
        {
            get { return m_ObjetId; }
            set { m_ObjetId = value; }
        }
        public string LockMode
        {
            get { return m_LockMode; }
            set { m_LockMode = value; }
        }
        #endregion
        //
        #region Constructor
        /// <summary>
        /// Création d'un LockObject 
        /// </summary>
        /// <param name="pTypeLock"></param>
        /// FI 20130527 [18662] m_ObjetId est renseigné avec  string.Empty
        public LockObject(TypeLockEnum pTypeLock)
            : this(pTypeLock, string.Empty, string.Empty, LockTools.Exclusive) { }
        
        /// <summary>
        /// Création d'un LockObject 
        /// <para>OjectId est alimenté avec string.Empty</para>
        /// </summary>
        /// <param name="pTypeLock"></param>
        /// <param name="pIdentifier"></param>
        /// FI 20130527 [18662] m_ObjetId est renseigné avec  string.Empty
        public LockObject(TypeLockEnum pTypeLock, string pIdentifier) :
            this(pTypeLock, string.Empty, pIdentifier, LockTools.Exclusive) { }



        /// <summary>
        /// Création d'un LockObject 
        /// <para>OjectId est alimenté avec string.Empty</para>
        /// </summary>
        /// <param name="pTypeLock"></param>
        /// <param name="pIdentifier"></param>
        /// <param name="pLockMode"></param>
        /// FI 20130527 [18662] m_ObjetId est renseigné avec  string.Empty
        public LockObject(TypeLockEnum pTypeLock, string pIdentifier, string pLockMode) :
            this(pTypeLock, string.Empty , pIdentifier, pLockMode) { }



        /// <summary>
        /// Création d'un LockObject avec id de type Integer
        /// </summary>
        /// <param name="pTypeLock"></param>
        /// <param name="pObjectId"></param>
        /// <param name="pIdentifier"></param>
        /// <param name="pLockMode"></param>
        /// FI 20130527 [18662] m_ObjetId est renseigné avec string.Empty
        public LockObject(TypeLockEnum pTypeLock, int pObjectId, string pIdentifier, string pLockMode)
            : this(pTypeLock, pObjectId.ToString(), pIdentifier, pLockMode)
        {
        }

        /// <summary>
        /// Création d'un LockObject avec id de type string
        /// </summary>
        /// <param name="pTypeLock"></param>
        /// <param name="pObjectId"></param>
        /// <param name="pIdentifier"></param>
        /// <param name="pLockMode"></param>
        /// FI 20130527 [18662] modification de la signature, m_ObjetId est renseigné avec string
        public LockObject(TypeLockEnum pTypeLock, string pObjectId, string pIdentifier, string pLockMode)
        {
            m_ObjectType = pTypeLock;
            m_ObjetId = pObjectId;
            m_ObjectIdentifier = pIdentifier;
            m_LockMode = pLockMode;
        }
        #endregion
        //
        #region IComparable Membres
        /// <summary>
        /// Retourne 0 si identique et -1 si différent
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public int CompareTo(object obj)
        {

            int ret = -1;
            //
            LockObject lockObejct2 = (LockObject)obj;
            if ((lockObejct2.m_ObjetId == this.m_ObjetId) &&
                (lockObejct2.m_ObjectType == this.m_ObjectType))
                ret = 0;
            //
            return ret;
        }
        #endregion
    }
	#endregion
	
	#region public class Lock
	/// <summary>
	/// Detail d'un lock EFS (Table EFSLOCK)
	/// </summary>
    // EG 20180205 [23769] Add dbTransaction  
    public class Lock
    {
        #region Members
        /// <summary>
        /// Object sur lequel porte le lock
        /// </summary>
        private LockObject m_lockObject;	 
        /// <summary>
        /// Session qui pose le lock
        /// </summary>
        private AppSession m_Session;
        /// <summary>
        /// Connection String
        /// </summary>
        private string m_cs;				
        /// <summary>
        /// Action Origine du lock
        /// </summary>
        private string m_action;
        /// <summary>
        /// Description du lock
        /// </summary>
        private string m_note;		  	    
        /// <summary>
        /// Lien Externe
        /// </summary>
        private string m_extLink;	
        /// <summary>
        /// not use yet
        /// </summary>
        private string m_rowattribute;
        /// <summary>
        /// not use yet
        /// </summary>
        private string m_LockMode;
        /// <summary>
        /// Date du lock
	    /// </summary>
        private DateTime m_dt;

        /// <summary>
        /// Connection String
        /// </summary>
        private IDbTransaction m_DbTransaction;				
	
        #endregion Members
        //
        #region Accessors
        public string Action
        {
            get { return m_action; }
            set { m_action = value; }
        }
        public DateTime Dt
        {
            get { return m_dt; }
            set { m_dt = value; }
        }
        public string Cs
        {
            get { return m_cs; }
            set { m_cs = value; }
        }
        public LockObject LockObject
        {
            get { return m_lockObject; }
            set { m_lockObject = value; }
        }
        public AppSession Session
        {
            get { return m_Session; }
            set { m_Session = value; }
        }
        public string Note
        {
            get { return m_note; }
            set { m_note = value; }
        }

        public string ExtLink
        {
            get { return m_extLink; }
            set { m_extLink = value; }
        }

        public string Rowattribute
        {
            get { return m_rowattribute; }
            set { m_rowattribute = value; }
        }

        public string LockMode
        {
            get { return m_LockMode; }
            set { m_LockMode = value; }
        }
        public IDbTransaction DbTransaction
        {
            get { return m_DbTransaction; }
            set { m_DbTransaction = value; }
        }

        #endregion
        //
        #region Constructor
        public Lock(string pCs, LockObject pLockObject, AppSession pSession, string pAction)
            : this(pCs, null, pLockObject, pSession, pAction) { }

        public Lock(string pCs, IDbTransaction pDbTransaction, LockObject pLockObject, AppSession pSession, string pAction)
        {
            m_cs = pCs;
            m_DbTransaction = pDbTransaction;
            m_lockObject = pLockObject;
            m_Session = pSession;
            m_action = pAction;
            m_dt = OTCmlHelper.GetDateSysUTC(m_cs);
        }
        #endregion Constructor
        //
        #region Method
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {

            string ret;
            //
            StringBuilder objectIdentifier = new StringBuilder();
            
            if (StrFunc.IsFilled(m_lockObject.ObjectIdentifier))
                objectIdentifier.AppendFormat("{0} [Id:{1}]", m_lockObject.ObjectIdentifier, m_lockObject.ObjectId);
            else
                objectIdentifier.AppendFormat("Id: {0}", m_lockObject.ObjectId);
            //
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("Warning !!!" + Cst.CrLf + Cst.CrLf);
            sb.AppendFormat("{0} {1} is locked" + Cst.CrLf + Cst.CrLf, m_lockObject.ObjectType.ToString(), objectIdentifier.ToString() );
            if (StrFunc.IsFilled(m_LockMode))
                sb.AppendFormat("Mode: {0}" + Cst.CrLf, m_LockMode.Substring(0,1));
            sb.AppendFormat("Action: {0}" + Cst.CrLf, m_action);
            sb.AppendFormat("Session: User {0}, Hostname {1}, Process {2}, SessionId {3}" + Cst.CrLf, m_Session.IdA_Identifier, m_Session.AppInstance.HostName, m_Session.AppInstance.AppNameVersion, m_Session.SessionId);
            sb.AppendFormat("DateTime: {0} {1}" + Cst.CrLf, DtFunc.DateTimeToString(m_dt, DtFunc.FmtISODateTime2), "Etc/UTC");
            //
            ret = sb.ToString();
            //
            return ret;

        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        /// FI 20130527 [18662]
        // EG 20180307 [23769] Gestion dbTransaction
        public string Message()
        {
            string identifier = Ressource.GetString(m_lockObject.ObjectType.ToString());

            if (StrFunc.IsFilled(m_lockObject.ObjectIdentifier))
            {
                identifier = m_lockObject.ObjectIdentifier + "[Id:" + m_lockObject.ObjectId + "]";
            }
            else if (StrFunc.IsFilled(m_lockObject.ObjectId))
            {
                switch (m_lockObject.ObjectType)
                {
                    case TypeLockEnum.ENTITYMARKET:
                        SQL_EntityMarket _sqlEntityMarket = new SQL_EntityMarket(m_cs, Convert.ToInt32(this.LockObject.ObjectId))
                        {
                            DbTransaction = m_DbTransaction
                        };
                        if (_sqlEntityMarket.IsLoaded)
                            identifier = _sqlEntityMarket.Entity_IDENTIFIER + " - " + _sqlEntityMarket.Market_IDENTIFIER + " [Id:" + _sqlEntityMarket.IdEM + "]";
                        break;
                    default:
                        identifier = "Id: " + m_lockObject.ObjectId;
                        break;
                }
            }

            string category = GetCategoryLockObject();

            string resMessage;
            switch (category)
            {
                case "PROCESS":
                    resMessage = Ressource.GetString2("Msg_LOCK_" + category, Ressource.GetString(m_action, m_action, true).ToUpper(),
                        identifier.ToUpper(),
                        m_Session.IdA_Identifier,
                        m_Session.AppInstance.HostName,
                        m_Session.AppInstance.AppNameVersion,
                        m_Session.SessionId,
                        DtFunc.DateTimeToString(m_dt, DtFunc.FmtISODateTime2) + " Etc/UTC");
                    break;
                case "TABLE":
                default:
                    resMessage = Ressource.GetString2("Msg_LOCK_" + category, identifier, m_action,
                        m_Session.IdA_Identifier,
                        m_Session.AppInstance.HostName,
                        m_Session.AppInstance.AppNameVersion,
                        m_Session.SessionId,
                        DtFunc.DateTimeToString(m_dt, DtFunc.FmtISODateTime2) + " Etc/UTC");
                    break;
            }
            return resMessage;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private string GetCategoryLockObject()
        {
            string category = "NATIVE";
            TypeLockEnum typeLock = new TypeLockEnum();
            FieldInfo fld = typeLock.GetType().GetField(LockObject.ObjectType.ToString());
            object[] attributes = fld.GetCustomAttributes(typeof(TypeLockObject), true);
            if (0 != attributes.GetLength(0))
                category = ((TypeLockObject)attributes[0]).Category;
            return category;
        }
        #endregion

    }
	#endregion Lock

}


	

