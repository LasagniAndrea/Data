using EFS.ACommon;
using EFS.ApplicationBlocks.Data;
using EFS.Authenticate;
using EFS.Common;
using EFS.Restriction;
using EfsML.Enum.MiFIDII_Extended;
using FixML.Enum;
using FpML.Enum;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Xml.Serialization;

namespace EFS.Actor
{
    #region  Enum

    /// <summary>
    /// Type de Rôle pour les regroupements d'acteurs, de books, d'instruments etc... 
    /// </summary>
    public enum RoleType
    {
        /// <summary>
        /// Système
        /// </summary>
        SYSTEM,
        /// <summary>
        /// Accès
        /// </summary>
        ACCESS,
        /// <summary>
        /// Négociation
        /// </summary>
        TRADING,
        /// <summary>
        /// Analytique
        /// </summary>
        ANALYTIC,
        /// <summary>
        /// 
        /// </summary>
        COLLABORATION,
        /// <summary>
        /// Titres
        /// </summary>
        SECURITIES,
        /// <summary>
        /// Règlement
        /// </summary>
        SETTLEMENT,
        /// <summary>
        /// External
        /// </summary>
        EXTERNAL,
        //CC/PL 20170918 [23429] Add new type
        /// <summary>
        /// Algorithm
        /// </summary>
        ALGORITHM,
        //CC/PL 20170928 [23429] Analysis amended - Add new type
        /// <summary>
        /// Collaboration,Algorithm
        /// </summary>
        COLLABORATION_ALGORITHM,
    }
    

    /// <summary>
    /// 
    /// </summary>
    public enum RoleActor
    {
        NONE,
        /// <summary>
        /// Rôle autre que ceux pévus 
        /// <example>Rôle DOSSIER chez BIM</example>
        /// </summary>
        EXTERNAL,
        /// <summary>
        /// Utilisateur Admin de Spheres® 
        /// </summary>
        SYSADMIN,
        /// <summary>
        /// Utilisateur avec pouvoirs avancés de Spheres®
        /// </summary>
        SYSOPER,
        /// <summary>
        /// Utilisateur invité sur Spheres®
        /// </summary>
        GUEST,
        /// <summary>
        /// Utilisateur de Spheres®
        /// </summary>
        USER,
        /// <summary>
        /// Rôle Fictif 
        /// <para>Rôle utilisé lorsqu'un acteur n'a pas de rôle identifié et partipe au trade</para>
        /// </summary>
        PARTY,
        /// <summary>
        /// Contrepartie
        /// </summary>
        COUNTERPARTY,
        /// <summary>
        /// 
        /// </summary>
        CLIENT,
        /// <summary>
        /// 
        /// </summary>
        ACCOUNTCONV,
        /// <summary>
        /// 
        /// </summary>
        BROKER,
        /// <summary>
        /// Compensateur 
        /// </summary>
        CLEARER,
        /// <summary>
        /// 
        /// </summary>
        CALCULATIONAGENT,   // FI role qui n'existe pas ds la Table 
        /// <summary>
        /// Animateur de marché
        /// </summary>
        MARKETMAKER,
        /// <summary>
        /// Clearing or Settlement Systems
        /// </summary>
        CSS,
        /// <summary>
        /// Agent Corespondant (Utilisé par la messagerie de rglt côté payer)
        /// </summary>
        CORRESPONDENT,
        /// <summary>
        ///  Intermédaire (Utilisé par la messagerie de rglt côté receiver)
        /// </summary>
        INTERMEDIARY,
        /// <summary>
        /// Teneur de compte
        /// </summary>
        ACCOUNTSERVICER,
        /// <summary>
        /// Propriétaire du compte
        /// </summary>
        ACCOUNTOWNER,
        ORIGINATOR,
        INVESTOR,
        /// <summary>
        /// 
        /// </summary>
        DEPARTMENT,
        DESK,
        /// <summary>
        /// Office de messagerie de confirmation, notification 
        /// </summary>
        CONTACTOFFICE,
        /// <summary>
        /// Office de règlement 
        /// </summary>
        SETTLTOFFICE,
        /// <summary>
        /// Office de gestion des deposits
        /// </summary>
        MARGINREQOFFICE,
        /// <summary>
        /// Office de gestion des soldes espèces
        /// </summary>
        CSHBALANCEOFFICE,
        /// <summary>
        /// Entité
        /// </summary>
        ENTITY,
        LICENSEE,
        SUPPORT,
        /// <summary>
        /// 
        /// </summary>
        CONTACT,
        /// <summary>
        /// 
        /// </summary>
        SALES,
        /// <summary>
        /// 
        /// </summary>
        /// FI 20170928 [23452] Add FpmlRoleActorAttribute
        [FpmlRoleActorAttribute(value = PersonRoleEnum.Trader)]
        TRADER,
        /// <summary>
        /// Emetteur
        /// </summary>
        ISSUER,
        /// <summary>
        /// 
        /// </summary>
        GUARANTOR,
        /// <summary>
        /// 
        /// </summary>
        MANAGER,
        /// <summary>
        /// 
        /// </summary>
        CUSTODIAN,
        /// <summary>
        /// 
        /// </summary>
        INVOICINGOFFICE,
        /// <summary>
        /// Compartiment Client de compensation des Clearings Organization ou des Clearers 
        /// </summary>
        CCLEARINGCOMPART,
        /// <summary>
        /// Compartiment Maison de compensation des Clearings Organization ou des Clearers 
        /// </summary>
        HCLEARINGCOMPART,
        /// <summary>
        /// Compartiment Market de compensation des Clearing Organization
        /// </summary>
        MCLEARINGCOMPART,
        /// <summary>
        /// 
        /// </summary>
        /// FI 20140206 [19564] add REGULATORYOFFICE
        REGULATORYOFFICE,
        //CC/PL 20170918 [23429] Add EXECUTION and INVESTDECISION roles
        /// <summary>
        /// EXECUTION - Algorithm responsible for the execution of the transaction
        /// </summary>
        /// FI 20170928 [23452] Add FpmlRoleActorAttribute
        [FpmlRoleActorAttribute(value = PersonRoleEnum.ExecutionWithinFirm)]
        [FpmlRoleActorAttribute(value = AlgorithmRoleEnum.Execution)]
        EXECUTION,

        /// <summary>
        /// INVESTDECISION - Specifies a role of investment decision for the algorithm
        /// </summary>
        /// FI 20170928 [23452] Add FpmlRoleActorAttribute
        [FpmlRoleActorAttribute(value = PersonRoleEnum.InvestmentDecisionMaker)]
        [FpmlRoleActorAttribute(value = AlgorithmRoleEnum.InvestmentDecision)]
        INVESTDECISION,
        // EG 20210315 New (pour CRP)
        SUBOFFICE,
        /// <summary>
        /// Représente un acteur qui a mandat de représentation (généralement vis à vis d'un client)
        /// </summary>
        /// FI 20171003 [23464] Add
        DECISIONOFFICE,
    }


    /// <summary>
    /// Equivalence FpML
    /// </summary>
    /// FI 20170928 [23452] add
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    public sealed class FpmlRoleActorAttribute : Attribute
    {
        /// <summary>
        /// Représente l'équivalent FpML d'un rôle dans Spheres
        /// </summary>
        public object value;
    }

    /// <summary>
    ///  Classe tools
    /// </summary>
    /// FI 20170928 [23452]
    public class RoleActorTools
    {
        /// <summary>
        /// Conversion d'une RoleActor Spheres en FpML
        /// <para>Les équivalences FpML doivent être spécifiées en tant que attribut (FpmlRoleActorAttribute) dans l'enum RoleActor</para>
        /// </summary>
        /// <typeparam name="T">Type du role FpML (enum)</typeparam>
        /// <param name="pRoleActor">valeur d'un enum RoleActor</param>
        /// <returns></returns>
        /// FI 20170928 [23452] Add
        public static T ConvertToFpmL<T>(RoleActor pRoleActor)
        {
            T ret = default;

            System.Reflection.FieldInfo fieldInfo = pRoleActor.GetType().GetField(pRoleActor.ToString());

            FpmlRoleActorAttribute[] enumAttrs = (FpmlRoleActorAttribute[])fieldInfo.GetCustomAttributes(typeof(FpmlRoleActorAttribute), true);

            if (ArrFunc.IsEmpty(enumAttrs))
                throw new NullReferenceException("FpmlRoleActorAttribute doesn't exist");

            FpmlRoleActorAttribute findItem = enumAttrs.Where(x => x.value.GetType().Equals(typeof(T))).FirstOrDefault();
            if (null == findItem)
                throw new NullReferenceException(StrFunc.AppendFormat("FpmlRoleActorAttribute doesn't exist for {0}", typeof(T).ToString()));

            ret = (T)findItem.value;

            return ret;
        }

        /// <summary>
        ///  Conversion d'un role FpML en RoleActor
        /// <para>Les équivalences FpML doivent être spécifiées en tant que attribut (FpmlRoleActorAttribute) dans l'enum RoleActor</para>
        /// </summary>
        /// <typeparam name="T">Type du role FpML (enum)</typeparam>
        /// <param name="pFpMLEnum">valeur d'un enum FpmL</param>
        /// <returns></returns>
        /// FI 20170928 [23452] Add
        public static RoleActor ConvertToRoleActor<T>(T pFpMLEnum)
        {

            RoleActor ret = default;

            IEnumerable<System.Reflection.FieldInfo> fieldInfo = from item in new RoleActor().GetType().GetFields().Where(x => (null != x.CustomAttributes) && x.CustomAttributes.Count() > 0)
                                                                 from item2 in item.CustomAttributes.Where(x => x.AttributeType.Equals(typeof(FpmlRoleActorAttribute)))
                                                                 select item;

            foreach (System.Reflection.FieldInfo item in fieldInfo)
            {
                FpmlRoleActorAttribute[] enumAttrs = (FpmlRoleActorAttribute[])item.GetCustomAttributes(typeof(FpmlRoleActorAttribute), true);
                if (ArrFunc.IsEmpty(enumAttrs))
                    throw new NullReferenceException("FpmlRoleActorAttribute doesn't exist"); //Impossible en théorie

                FpmlRoleActorAttribute findItem = enumAttrs.Where(x => x.value.GetType().Equals(typeof(T))).FirstOrDefault();
                if (null != findItem)
                {
                    if (Enum.Parse(typeof(T), findItem.value.ToString()).Equals(pFpMLEnum))
                    {
                        ret = (RoleActor)item.GetValue(new RoleActor());
                        break;
                    }
                }
            }
            return ret;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public enum RoleActorSSI
    {
        CSS,
        CORRESPONDENT,
        INTERMEDIARY,
        ACCOUNTSERVICER,
        ACCOUNTOWNER,
        INVESTOR,
        ORIGINATOR,
    }
    #endregion Enum

    /// <summary>
    /// Classe représentant un rôle pour un acteur donné avec
    /// ses dates DTENABLED et DTDISABLED
    /// </summary>
    // EG 20230709 [XXXXX][WI551] New : ACTORROLE => DTENABLED/DTDISABLED are not considered    
    public class UserRoleType
    {
        private int _idA;
        private RoleActor _role;
        private DateTime _dtEnabled;
        private DateTime _dtDisabled;

        public int IdA
        {
            get { return _idA; }
            set { _idA = value; }
        }
        public RoleActor Role
        {
            get { return _role; }
            set { _role = value; }
        }
        public DateTime DtEnabled
        {
            get { return _dtEnabled; }
            set { _dtEnabled = value; }
        }
        public DateTime DtDisabled
        {
            get { return _dtDisabled; }
            set { _dtDisabled = value; }
        }

        public bool IsAvailable
        {
            get { return (_dtEnabled.CompareTo(DateTime.Now) <= 0) && (_dtDisabled.CompareTo(DateTime.Now) > 0); }
        }

        public UserRoleType(int pIdA, RoleActor pRole, DateTime pDtEnabled, DateTime pDtDisabled)
        {
            _idA = pIdA;
            _role = pRole;
            _dtEnabled = pDtEnabled;
            _dtDisabled = pDtDisabled;
        }
    }
    /// <summary>
    ///  Represente un utilisateur de Spheres® (User, SysAdmin, SysOper, etc...)
    ///  <para>Contient les méthodes utiles à l'authentification (voir ValidityEnum)</para>
    /// </summary>
    // PL 20171020 [23490] add Timezone
    // EG 20220623 [XXXXX] Shibboleth
    public class Collaborator
    {
        // EG 20220623 [XXXXX] Type de données utilisable pour faire la correspondance
        // entre Authentification Shibboleth et notre RDBMS
        // Liste non exhaustive
        public enum SAMLMappingClaimEnum
        {
            identifier,
            mail,
            displayName,
            givenName,
            sn,
            telephoneNumber,
        }
        #region public enum
        // EG 20220220 [26251] Validity User (SUPPRESSION de UnknownIdentifier et IncorrectPassword, on GARDE UnknownIdentifierOrIncorrectPassword) 
        public enum ValidityEnum
        {
            Succes = -1,
            MissingIdentifier = 0,
            MissingEntity,
            //UnknownIdentifier,
            UnknownIdentifierOrIncorrectPassword,
            //IncorrectPassword,
            MultipleIdentifier,
            RemovedAccount,
            DisabledAccountVsDtEnabled,
            DisabledAccountVsDtDisabled,
            ExpiredPwd,
            MissingModelSafety,
            UnauthorizedAcces,
            UnreachableDirectory, //PL 20160229 Newness
        }
        public enum AuthenticationTypeEnum
        {
            NA, DOMAINNT, EUROSYS, LDAP, RDBMS
        }
        #endregion

        #region Members
        private ValidityEnum _validity;
        private string _errorResource;
        private string _logMessage;

        //FI 20140519 [19923] add _parent
        //LP 20240723 [WI1001] Rename _parent to _entity
        private CollaboratorParent _entity;
        //FI 20140519 [19923] add _department
        private CollaboratorParent _department;

        private AuthenticationTypeEnum _authenticationType = AuthenticationTypeEnum.NA; 
        private int _ida;
        private ActorAncestor _actorAncestor;
        private string _identifier;
        private string _displayName;
        // EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        private string _cssColor;
        private string _culture;
        private string _timezone;
        private string _country;
        private string _extlLink;
        private string _pwd;
        Nullable<RoleActor> _userType;
        // EG 20230709 [XXXXX][WI551] New
        private DateTime _userTypeDtEnabled;
        private DateTime _userTypeDtDisabled;
        private string _telephoneNumber;
        private bool _isRdbmsUser;
        private bool _isDeleted;
        private bool _isPwdModNextLogOn;
        private Cst.ActionOnConnect _action;
        private bool _isPwdModPermitted;
        private bool _isPwdModPeriodic;
        // EG 20160404 Migration vs2013
        //private int _idModelSafety;
        private int _pwdRemainingGrace;
        private int _nbDayWarning;
        private int _nbDayPwdExpiration;
        private int _simultaneousLogin;
        // EG 20160404 Migration vs2013
        //private bool _dtExpirationSpecified;
        private DateTime _dtEnabled;
        private DateTime _dtDisabled;
        // EG 20151215 [21305] Nullable
        private Nullable<DateTime> _dtExpiration;

        private bool _isRDBMSTrace;
        private bool _isPwdChecked;
        // EG 20210209 [25660] New
        private bool _isPwdMustbeUpdated;
        // AL 20240607 [WI955] Impersonate mode
        private string _impersonateIdentifier;
        private string _impersonateDisplayName;
        #endregion members

        #region Accessors
        public ValidityEnum Validity
        {
            get { return _validity; }
            set
            {
                _validity = value;
                switch (_validity)
                {
                    case ValidityEnum.Succes:
                        ErrorResource = string.Empty;
                        break;
                    case ValidityEnum.DisabledAccountVsDtEnabled:
                    case ValidityEnum.DisabledAccountVsDtDisabled:
                        ErrorResource = "FailureConnect_DisabledAccount";
                        break;
                    default:
                        ErrorResource = "FailureConnect_" + Validity.ToString();
                        //Test d'existence d'une ressource
                        if (!Ressource.GetStringByRef(ErrorResource))
                            ErrorResource = "FailureConnect_Miscellaneous";
                        break;
                }
            }
        }
        public string ErrorResource
        {
            get { return _errorResource; }
            set { _errorResource = value; }
        }
        public string LogMessage
        {
            get { return _logMessage; }
            set { _logMessage = value; }
        }
        public AuthenticationTypeEnum AuthenticationType
        {
            get { return _authenticationType; }
            set { _authenticationType = value; }
        }
        public int Ida
        {
            get { return _ida; }
            set { _ida = value; }
        }
        public string Identifier
        {
            get { return _identifier; }
            set { _identifier = value; }
        }        
        public string DisplayName
        {
            get { return _displayName; }
            set { _displayName = value; }
        }
        /// <summary>
        /// 
        /// </summary>
        // EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        public string CssColor
        {
            get { return _cssColor; }
            set { _cssColor = value; }
        }
        public string Culture
        {
            get { return _culture; }
            set { _culture = value; }
        }
        public string Timezone
        {
            get { return _timezone; }
            set { _timezone = value; }
        }
        public string Country
        {
            get { return _country; }
            set { _country = value; }
        }
        public string ExtlLink
        {
            get { return _extlLink; }
            set { _extlLink = value; }
        }
        /// <summary>
        /// 
        /// </summary>
        public ActorAncestor ActorAncestor
        {
            get { return _actorAncestor; }
            set { _actorAncestor = value; }
        }
        /// <summary>
        /// Obtient ou définie l'acteur parent (1er acteur parent avec le rôle ENTITY ou SUPPORT s'il n'y a pas d'entité)
        /// </summary>
        //FI 20140519 [19923]
        //PL 20240724 Rename Parent to Entity [WI1001]
        public CollaboratorParent Entity
        {
            get { return _entity; }
            set { _entity = value; }
        }
        /// <summary>
        /// Obtient ou définie le département (1er acteur parent avec le rôle DEPARTMENT)
        /// </summary>
        public CollaboratorParent Department
        {
            get { return _department; }
            set { _department = value; }
        }

        public string Pwd
        {
            get { return _pwd; }
            set { _pwd = value; }
        }
        public Nullable<RoleActor> UserType
        {
            get { return _userType; }
            set { _userType = value; }
        }
        // EG 20230709 [XXXXX][WI551] New
        public DateTime UserTypeDtEnabled
        {
            get { return _userTypeDtEnabled; }
            set { _userTypeDtEnabled = value; }
        }
        // EG 20230709 [XXXXX][WI551] New
        public DateTime UserTypeDtDisabled
        {
            get { return _userTypeDtDisabled; }
            set { _userTypeDtDisabled = value; }
        }
        public string TelephoneNumber
        {
            get { return _telephoneNumber; }
            set { _telephoneNumber = value; }
        }
        public bool IsRdbmsUser
        {
            get { return _isRdbmsUser; }
            set { _isRdbmsUser = value; }
        }
        public bool IsDeleted
        {
            get { return _isDeleted; }
            set { _isDeleted = value; }
        }
        public bool IsPwdModNextLogOn
        {
            get { return _isPwdModNextLogOn; }
            set { _isPwdModNextLogOn = value; }
        }
        public Cst.ActionOnConnect ActionOnConnect
        {
            get { return _action; }
            set { _action = value; }
        }
        public bool IsPwdModPermitted
        {
            get { return _isPwdModPermitted; }
            set { _isPwdModPermitted = value; }
        }
        /// <summary>
        /// 
        /// </summary>
        public bool IsPwdModPeriodic
        {
            get { return _isPwdModPeriodic; }
            set { _isPwdModPeriodic = value; }
        }
        /// <summary>
        /// IdModelSafety peut être celui définit directement sur l'Actor, 
        /// dans le cas contraire, c'est celui définit sur un des ancetres de Actor                    
        /// </summary>
        public int IdModelSafety
        {
            get;
            set;
        }
        public int PwdRemainingGrace
        {
            get { return _pwdRemainingGrace; }
            set { _pwdRemainingGrace = value; }
        }
        public int NbDayWarning
        {
            get { return _nbDayWarning; }
            set { _nbDayWarning = value; }
        }
        public int NbDayPwdExpiration
        {
            get { return _nbDayPwdExpiration; }
            set { _nbDayPwdExpiration = value; }
        }
        public int SimultaneousLogin
        {
            get { return _simultaneousLogin; }
            set { _simultaneousLogin = value; }
        }
        /// <summary>
        /// journalisation des actions utilisateurs
        /// </summary>
        /// FI 20140519 [19923] add property
        public Cst.RequestTrackMode TrackMode
        {
            get;
            set;
        }
        /// <summary>
        /// Liste des mots de passe interdits
        /// </summary>
        /// EG 20151215 [21305] New
        public List<string> PwdForbiddenList
        {
            get;
            set;
        }

        public DateTime DtEnabled
        {
            get { return _dtEnabled; }
            set { _dtEnabled = value; }
        }
        public DateTime DtDisabled
        {
            get { return _dtDisabled; }
            set { _dtDisabled = value; }
        }
        // EG 20151215 [21305] Nullable
        public Nullable<DateTime> DtExpiration
        {
            get { return _dtExpiration; }
            set { _dtExpiration = value; }
        }
        public bool IsRDBMSTrace
        {
            get { return _isRDBMSTrace; }
            set { _isRDBMSTrace = value; }
        }
        public bool IsPwdChecked
        {
            get { return _isPwdChecked; }
            set { _isPwdChecked = value; }
        }
        // EG 20210209 [25660] New
        public bool IsPwdMustBeUpdateAfterNewHashAlgorithm
        {
            get { return _isPwdMustbeUpdated; }
            set { _isPwdMustbeUpdated = value; }
        }
        /// <summary>
        /// 
        /// </summary>
        public bool IsActorSysAdmin
        {
            get
            {
                Boolean ret = false;
                if (null != UserType)
                    ret = ActorTools.IsUserType_SysAdmin(UserType.Value);
                return ret;
            }
        }

        // AL 20240530 Impersonation mode
        public string ImpersonateIdentifier
        {
            get { return _impersonateIdentifier; }
            set { _impersonateIdentifier = value; }
        }
        public string ImpersonateDisplayName
        {
            get { return _impersonateDisplayName; }
            set { _impersonateDisplayName = value; }
        }
        #endregion

        #region constructor
        /// <summary>
        /// 
        /// </summary>
        /// FI 20140519 [19923] initilisation de _parent et _department
        public Collaborator()
        {
            _ida = 0;
            _entity = new CollaboratorParent();
            _department = new CollaboratorParent();
            Initialize();
        }
        #endregion constructor

        #region IsPwdValid
        /// <summary>
        /// Check if PWD is valid
        /// </summary>
        /// <param name="pCs"></param>
        /// <param name="pPwd"></param>
        /// <returns></returns>
        // EG 20151215 [21305] Add IsPwdForbidden
        // EG 20210209 [25660] Upd
        // RD 20220221 [25943] Add pCs and use CheckConnection.IsPwdForbidden
        public bool IsPwdValid(string pCs, string pPwd)
        {
            // RD 20220221 [25943] Use CheckConnection.IsPwdForbidden
            return HashDataCompare(Pwd, pPwd) && (false == CheckConnection.IsPwdForbidden(pCs, pPwd, PwdForbiddenList, Ida));
        }
        #endregion IsPwdValid
        // EG 20210209 [25660] New
        // EG 20210209 [25660] L'algorithme de hachage MD5 est déprécié. Méthode de transformation douce lors de la connexion
        private bool HashDataCompare(string pSourceData, string pData)
        {
            bool isOk = false;
            Tuple<string, string,  DateTime> ret = SystemSettings.GetAppSettings_HashAlgorithm;
            if (StrFunc.IsFilled(ret.Item1) && Enum.IsDefined(typeof(Cst.HashAlgorithm), ret.Item1))
            {
                Cst.HashAlgorithm defaultAlgorithm = (Cst.HashAlgorithm)Enum.Parse(typeof(Cst.HashAlgorithm), ret.Item1, true);
                string _hashData = StrFunc.HashData(pData, defaultAlgorithm);
                isOk = pSourceData == _hashData;
                if (false == isOk)
                {
                    if (StrFunc.IsFilled(ret.Item2) && DtFunc.IsDateTimeFilled(ret.Item3) && (DateTime.Compare(ret.Item3, DateTime.Today) > 0))
                    {
                        defaultAlgorithm = (Cst.HashAlgorithm)Enum.Parse(typeof(Cst.HashAlgorithm), ret.Item2, true);
                        isOk = pSourceData == StrFunc.HashData(pData, defaultAlgorithm);
                        if (isOk)
                            _isPwdMustbeUpdated = true;
                    }
                }
            }
            return isOk;
        }

        // EG 20210209 [25660] New
        // EG 20210209 [25660] L'algorithme de hachage MD5 est déprécié. Méthode de transformation douce lors de la connexion
        public void UpdatePwdWithNewHashAlgorithm(string pCS, string pInputPassword)
        {
            string sqlUpdate = @"update dbo.ACTOR set PWD = @PWD where (IDA = @IDA)";

            DataParameters parameters = new DataParameters();
            parameters.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.IDA), _ida);
            parameters.Add(new DataParameter(pCS, "PWD", DbType.AnsiString, 128), SystemSettings.HashData(pInputPassword));

            QueryParameters qry = new QueryParameters(pCS, sqlUpdate, parameters);
            int nbRow = DataHelper.ExecuteNonQuery(pCS, CommandType.Text, qry.Query, qry.Parameters.GetArrayDbParameter());
            if (nbRow == 1)
            {
                sqlUpdate = @"update dbo.PWDEXPIRED_H set PWD = @PWD where (IDA = @IDA)";
                qry = new QueryParameters(pCS, sqlUpdate, parameters);
                _ = DataHelper.ExecuteNonQuery(pCS, CommandType.Text, qry.Query, qry.Parameters.GetArrayDbParameter());
            }
        }

        #region  Methode
        // EG 20151215 [21305] DtExpiration nullable
        // EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        public void Initialize()
        {
            ActionOnConnect = Cst.ActionOnConnect.NONE;
            NbDayPwdExpiration = 0;
            //
            if (_ida == 0)
            {
                Validity = ValidityEnum.MissingIdentifier;
                AuthenticationType = AuthenticationTypeEnum.NA;
                Identifier = "Unknown";
                DisplayName = Identifier;
                CssColor = string.Empty;
                Culture = ConfigurationManager.AppSettings["Spheres_ReferentialDefault_culture"];
                Timezone = ConfigurationManager.AppSettings["Spheres_ReferentialDefault_timezone"];
                Country = ConfigurationManager.AppSettings["Spheres_ReferentialDefault_country"];
                DtDisabled = DateTime.Today.Add(TimeSpan.FromDays(1));
                DtExpiration = DateTime.Today.Add(TimeSpan.FromDays(1));
                TrackMode = Cst.RequestTrackMode.NONE   ;
            }
            else if (IsDeleted)
            {
                Validity = ValidityEnum.RemovedAccount;
            }
            else if (DtEnabled.CompareTo(DateTime.Now) > 0)
            {
                Validity = ValidityEnum.DisabledAccountVsDtEnabled;
            }
            else if (DtDisabled.CompareTo(DateTime.Now) <= 0)
            {
                Validity = ValidityEnum.DisabledAccountVsDtDisabled;
            }
            //EG 20151215 [21305] DtExpiration.HasValue
            //else if (IsPwdModPeriodic && DtExpiration.HasValue && (DtExpiration.Value.CompareTo(DateTime.Now) <= 0))
            //PL 20160315 Add and Use AuthenticationType member
            else if ((AuthenticationType != AuthenticationTypeEnum.LDAP) && IsPwdModPeriodic && DtExpiration.HasValue && (DtExpiration.Value.CompareTo(DateTime.Now) <= 0))
            {
                //Changement périodique activé et Mot de passe expiré :
                //S'il reste des graces à l'utilisateur et que l'utilisateur dispose du droit nécessaire
                //ou que le flag chgt du mot de passe est actif (ie: suite à une action de l'admin)
                // --> Connection autorisé pour changer le mot de passe
                if ((PwdRemainingGrace > 0 && IsPwdModPermitted) || (IsPwdModNextLogOn))
                {
                    Validity = ValidityEnum.Succes;
                    ActionOnConnect = Cst.ActionOnConnect.CHANGEPWD;
                }
                //Lock du compte
                else
                    Validity = ValidityEnum.ExpiredPwd;
            }
            else if (null == UserType)
            {
                Validity = ValidityEnum.UnauthorizedAcces;
            }
            else
            {
                // EG 20230709 [XXXXX][WI551] New
                if (UserTypeDtEnabled.CompareTo(DateTime.Now) > 0)
                {
                    Validity = ValidityEnum.DisabledAccountVsDtEnabled;
                }
                else if (UserTypeDtDisabled.CompareTo(DateTime.Now) <= 0)
                {
                    Validity = ValidityEnum.DisabledAccountVsDtDisabled;
                }
                else
                {
                    Validity = ValidityEnum.Succes;

                    //PL 20160315 Add and Use AuthenticationType member
                    if (AuthenticationType != AuthenticationTypeEnum.LDAP)
                    {
                        //Si le changement est obligatoire à la connection
                        if (IsPwdModNextLogOn)
                        {
                            ActionOnConnect = Cst.ActionOnConnect.CHANGEPWD;
                        }
                        //Sinon, si changement périodique activé et date d'expiration renseignée
                        // EG 20151215 [21305] DtExpiration.HasValue
                        else if (IsPwdModPeriodic && DtExpiration.HasValue)
                        {
                            //On regarde si un alert() d'expiration doit etre affiché
                            // EG 20151215 [21305] DtExpiration.Value
                            int nbDayPwdExpiration = (DtExpiration.Value.AddDays(-1 * NbDayWarning)).CompareTo(DateTime.Now);
                            if (IsPwdModPermitted && (nbDayPwdExpiration <= 0))
                            {
                                ActionOnConnect = Cst.ActionOnConnect.EXPIREDPWD;
                                NbDayPwdExpiration = nbDayPwdExpiration + NbDayWarning;
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Recherche du rôle de l'utilisateur: SYSADMIM, SYSOPER, USER, GUEST...
        /// </summary>
        /// <param name="pCs"></param>
        // EG 20230709 [XXXXX][WI551] Upd ACTORROLE => DTENABLED/DTDISABLED are not considered
        public void SetUserType(string pCs)
        {
            UserRoleType _userRoleType = ActorTools.GetUserType(pCs, Ida);
            if (null != _userRoleType)
            {
                UserType = _userRoleType.Role;
                UserTypeDtEnabled = _userRoleType.DtEnabled;
                UserTypeDtDisabled = _userRoleType.DtDisabled;
            }
        }

        /// <summary>
        /// Recherche des ancestors 
        /// </summary>
        /// <param name="pSource"></param>
        public void SetAncestor(string pCS)
        {
            ActorAncestor = new ActorAncestor(pCS, null, Ida, pRoleRestrict: null, pRoleTypeExclude: null, pIsPartyRelation: false);
        }

        /// <summary>
        /// Recherche du Parent de type ENTITY (ou à defaut de type SUPPORT)
        /// </summary>
        /// <param name="pCS"></param>
        // EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        public void SetEntity(string pCS)
        {
            _entity = new CollaboratorParent();
            
            if (null != ActorAncestor)
            {
                //Recherche d'un parent ENTITY (ou SUPPORT)
                RoleActor role = RoleActor.ENTITY;
                int idA = ActorAncestor.GetFirstRelation(role);
                if (idA > 0)
                {
                    _entity.IsEntity = true;
                }
                else
                {
                    //Aucun parent ENTITY --> Tentative de recherche d'un parent SUPPORT (ie: EFS)
                    role = RoleActor.SUPPORT;
                    idA = ActorAncestor.GetFirstRelation(role);
                }

                if (idA > 0)
                {
                    _entity.Set(pCS, idA, role);
                    if (StrFunc.IsFilled(_entity.CssColor))
                        CssColor = _entity.CssColor;
                }
            }
        }

        /// <summary>
        /// Recherche du Parent de type DEPARTMENT (ou DESK)
        /// </summary>
        /// <param name="pCS"></param>
        /// FI 20140519 [19923] gestion du departement
        public void SetDepartment(string pCS)
        {
            _department = new CollaboratorParent();

            if (null != ActorAncestor)
            {
                //Recherche d'un éventuel parent DEPARTMENT
                RoleActor role = RoleActor.DEPARTMENT;
                int idA = ActorAncestor.GetFirstRelation(role); 
                if (idA > 0)
                {
                    _department.IsDepartment = true;
                }
                else
                {
                    //Aucun parent DEPARTMENT --> Tentative de recherche d'un parent DESK
                    role = RoleActor.DESK;
                    idA = ActorAncestor.GetFirstRelation(role);
                }
                                
                if (idA > 0)
                {
                    _department.Set(pCS, idA, role);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pHostName"></param>
        public void SetRDBMSTrace(string pCS, string pHostName)
        {

            IsRDBMSTrace = false;
            //
            StrBuilder subSelect = new StrBuilder();
            subSelect += SQLCst.SELECT + "1 as NUM, ISTRACE" + Cst.CrLf;
            subSelect += SQLCst.FROM_DBO + Cst.OTCml_TBL.RDBMSTRACE.ToString() + Cst.CrLf;
            subSelect += SQLCst.WHERE + "IDENTIFIER = " + DataHelper.SQLString(Identifier) + Cst.CrLf;
            subSelect += SQLCst.AND + "HOSTNAME = " + DataHelper.SQLString(pHostName) + Cst.CrLf;
            subSelect += SQLCst.UNION + Cst.CrLf;
            subSelect += SQLCst.SELECT + "2 as NUM, ISTRACE" + Cst.CrLf;
            subSelect += SQLCst.FROM_DBO + Cst.OTCml_TBL.RDBMSTRACE.ToString() + Cst.CrLf;
            subSelect += SQLCst.WHERE + "IDENTIFIER = " + DataHelper.SQLString(Identifier) + Cst.CrLf;
            subSelect += SQLCst.AND + "(HOSTNAME = " + DataHelper.SQLString("*") + SQLCst.OR + "HOSTNAME is null)" + Cst.CrLf;
            subSelect += SQLCst.UNION + Cst.CrLf;
            subSelect += SQLCst.SELECT + "3 as NUM, ISTRACE" + Cst.CrLf;
            subSelect += SQLCst.FROM_DBO + Cst.OTCml_TBL.RDBMSTRACE.ToString() + Cst.CrLf;
            subSelect += SQLCst.WHERE + "IDENTIFIER = " + DataHelper.SQLString("*") + Cst.CrLf;
            subSelect += SQLCst.AND + "HOSTNAME = " + DataHelper.SQLString(pHostName) + Cst.CrLf;
            subSelect += SQLCst.UNION + Cst.CrLf;
            subSelect += SQLCst.SELECT + "4 as NUM, ISTRACE" + Cst.CrLf;
            subSelect += SQLCst.FROM_DBO + Cst.OTCml_TBL.RDBMSTRACE.ToString() + Cst.CrLf;
            subSelect += SQLCst.WHERE + "IDENTIFIER = " + DataHelper.SQLString("*") + Cst.CrLf;
            subSelect += SQLCst.AND + "(HOSTNAME = " + DataHelper.SQLString("*") + SQLCst.OR + "HOSTNAME is null)" + Cst.CrLf;
            //
            StrBuilder sqlSelect = new StrBuilder();
            sqlSelect += SQLCst.SELECT + "ISTRACE" + Cst.CrLf;
            sqlSelect += SQLCst.X_FROM + "(" + subSelect.ToString() + ") Trace" + Cst.CrLf;
            sqlSelect += SQLCst.ORDERBY + "NUM";
            //20070717 FI utilisation de ExecuteScalar
            object result = DataHelper.ExecuteScalar(pCS, CommandType.Text, sqlSelect.ToString());
            IsRDBMSTrace = BoolFunc.IsTrue(result);
            //
        }
        
        /// <summary>
        /// Retourne une nouvelle instance de class User à partir des propriétés de collaborator
        /// </summary>
        /// <returns></returns>
        public User NewUser()
        {
            return new User(this);
        }
        
        public string GetIdentifierDisplayNameId(CollaboratorParent pParent)
        {
            string ret = "N/A"; 
            if (pParent is null)
            {
                ret = FormatIdentifierDisplayNameId(this.Identifier, this.DisplayName, this.Ida, this.UserType);
            }
            else if (pParent.IsLoaded)
            {
                ret = FormatIdentifierDisplayNameId(pParent.Identifier, pParent.DisplayName, pParent.Ida, pParent.Role);
            }
            return ret;
        }
        private string FormatIdentifierDisplayNameId(string pIdentifier, string pDisplayName, int pId, RoleActor? pRole)
        {
            return pIdentifier + Cst.HTMLSpace + "/"
                   + ((pIdentifier == pDisplayName) ? string.Empty : Cst.HTMLSpace + pDisplayName + Cst.HTMLSpace + "/")
                   + Cst.HTMLSpace + Cst.HTMLSpan + "(Id: " + pId.ToString() + ") " + Cst.HTMLEndSpan
                   + ((pRole is null) ? string.Empty : Cst.HTMLSpace + pRole.ToString() + Cst.HTMLSpace);
        }
        #endregion
    }

    /// <summary>
    /// Représente un parent de Collaborator
    /// </summary>
    // FI 20140519 [19923] add class afin de gérer le departement de rattachement d'un utilisateur
    // PL 20171020 [23490] add Timezone
    // EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
    // EG 20210419 [XXXXX] Ajout BusinessCenter
    public class CollaboratorParent
    {
        public Boolean IsLoaded = false;
        public Boolean IsEntity = false;
        public Boolean IsDepartment = false;
        public int Ida = 0;
        public RoleActor Role = RoleActor.NONE;
        public string Identifier = string.Empty;
        public string DisplayName = string.Empty;
        public string Culture = string.Empty;
        public string Timezone = string.Empty;
        public string Country = string.Empty;
        public string Description = string.Empty;
        public string BIC = string.Empty;
        public string CssColor = string.Empty;
        public string BusinessCenter = string.Empty;

        //public string IdentifierDisplayNameId
        //{
        //    get 
        //    {
        //        string ret = "N/A";
        //        if (this.IsLoaded)
        //        {
        //            ret = Identifier + Cst.HTMLSpace + "/"
        //                  + ((Identifier == DisplayName) ? string.Empty : Cst.HTMLSpace + DisplayName + Cst.HTMLSpace + "/")
        //                  + Cst.HTMLSpace + Cst.HTMLSpan + "(Id: " + Ida.ToString() + ") " + Cst.HTMLEndSpan;
        //        }
        //        return ret; 
        //    }
        //}

        public CollaboratorParent() { }

        // EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        // EG 20210419 [XXXXX] Ajout BusinessCenter
        // PL 20240724 New signature / Add pRole
        public void Set(string pCS, int pId, RoleActor pRole)
        {
            DataParameters parameters = new DataParameters();
            parameters.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.IDA), pId);
            
            StrBuilder sqlSelect = new StrBuilder(SQLCst.SELECT);
            sqlSelect += "a.IDA, a.IDENTIFIER, a.DISPLAYNAME, a.DESCRIPTION, a.BIC, a.CSSFILENAME, a.CULTURE, a.TIMEZONE, a.IDCOUNTRYRESIDENCE, a.IDBC" + Cst.CrLf;
            sqlSelect += SQLCst.FROM_DBO + Cst.OTCml_TBL.ACTOR.ToString() + " a" + Cst.CrLf;
            sqlSelect += SQLCst.WHERE + "a.IDA=@IDA";
            
            QueryParameters qry = new QueryParameters(pCS, sqlSelect.ToString(), parameters);
            
            //20070717 FI utilisation de ExecuteDataTable pour le Cache
            IDataReader dr = null;
            try
            {
                dr = DataHelper.ExecuteDataTable(qry.Cs, qry.Query, qry.Parameters.GetArrayDbParameter()).CreateDataReader();
                if (dr.Read())
                {
                    IsLoaded = true;
                    Role = pRole;
                    Ida = Convert.ToInt32(dr["IDA"]);
                    Identifier = dr["IDENTIFIER"].ToString();
                    DisplayName = dr["DISPLAYNAME"].ToString();
                    Culture = dr["CULTURE"].ToString();
                    Timezone = dr["TIMEZONE"].ToString();
                    Country = dr["IDCOUNTRYRESIDENCE"].ToString();
                    Description = dr["DESCRIPTION"].ToString();
                    BIC = dr["BIC"].ToString();
                    if (0 < dr["CSSFILENAME"].ToString().Length)
                        CssColor = dr["CSSFILENAME"].ToString();
                    BusinessCenter = dr["IDBC"].ToString();
                }
            }
            finally
            {
                if (null != dr)
                    dr.Close();
            }
        }       
    }

    /// <summary>
    /// Classe stockant les parents d'un acteur et regroupant les methodes de chargement des parents
    /// </summary>
    public class ActorAncestor
    {
        #region Members
        /// <summary>
        /// Obtient ou définit la relation source (Parent-Enfant-Rôle)
        /// </summary>
        private readonly ActorRelation actorRelation;
        /// <summary>
        /// Obtient ou définit les relations Parent-Enfant-Role de 1er niveau associé à l'acteur parent la relation source
        /// </summary>
        private ActorAncestor[] directAncestors;
        /// <summary>
        /// Obtient ou définit les rôles génériques de l'acteur Parent de la relation source
        /// </summary>
        private ActorRole[] globalRelations;
        #endregion

        #region Constructor(s)
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pSource"></param>
        /// <param name="pDbTransaction"></param>
        /// <param name="pLevel">int : niveau actuel de progression recursive (sert a stopper la boucle recursive si on depasse le niveau 10)</param>
        /// <param name="pRoleRestrict">Lorsque renseigné considère uniquement les relations désignées</param>
        /// <param name="pRoleTypeExclude">Lorsque renseigné ignore les type de rôle désignés</param>
        /// <param name="pIsPartyRelation">Lorsque true , considère uniquement les relatiions avec ISPARTYRELATION</param>
        // EG 20180205 [23769] Add dbTransaction  
        // EG 20220519 [WI637] Ajout paramètre pRoleTypeExclude
        // FI 20240218 [WI838] ce constructeur devient privé
        private ActorAncestor(string pSource, IDbTransaction pDbTransaction, ActorRole pActorRole, int pLevel, RoleActor[] pRoleRestrict, RoleType[] pRoleTypeExclude, bool pIsPartyRelation)
        {
            actorRelation = new ActorRelation(pActorRole, pLevel);

            //Sécurité (afin d'éviter une potentielle boucle infinie)
            if (pLevel < 10)
                LoadAncestors(pSource, pDbTransaction, pRoleRestrict, pRoleTypeExclude, pIsPartyRelation);
        }

        /// <summary>
        /// Chargement des acteurs parents de l'acteur <paramref name="pIdA"/>
        /// <para> 
        /// La 1er relation obtenue est : l'acteur inconnu (id:0) a le rôle <see cref="RoleActor.NONE"/> vis à vis de l'acteur <paramref name="pIdA"/>
        /// </para>
        /// </summary>
        /// <param name="pSource"></param>
        /// <param name="pDbTransaction"></param>
        /// <param name="pIdA">Représente l'acteur initial</param>
        /// <param name="pRoleRestrict">Lorsque renseigné considère uniquement les relations désignées</param>
        /// <param name="pRoleTypeExclude">Lorsque renseigné ignore les type de rôle désignés</param>
        /// <param name="pIsPartyRelation">Lorsque true , considère uniquement les relatiions avec ISPARTYRELATION</param>
        /// EG 20220519 [WI637] Ajout paramètre pRoleTypeExclude
        /// FI 20240218 [WI838] seul ce constructeur est conservé
        public ActorAncestor(string pSource, IDbTransaction pDbTransaction, int pIdA, RoleActor[] pRoleRestrict, RoleType[] pRoleTypeExclude, bool pIsPartyRelation)
            : this(pSource, pDbTransaction, new ActorRole(0, RoleActor.NONE, pIdA), 0, pRoleRestrict, pRoleTypeExclude, pIsPartyRelation) { }

        #endregion Constructor(s)

        #region LoadAncestors
        /// <summary>
        /// Charge dans DirectAncestors les parents directs ( = niveau +1) du ActorAncestor actuel
        /// </summary>
        // EG 20180205 [23769] Add dbTransaction  
        // EG 20180425 Analyse du code Correction [CA2202]
        // EG 20190613 [24683] Use Datatable instead of DataReader
        // EG 20220519 [WI637] Ajout paramètre pRoleTypeExclude
        private void LoadAncestors(string pSource, IDbTransaction pDbTransaction, RoleActor[] pRoleRestrict, RoleType[] pRoleTypeExclude, bool pIsPartyRelation)
        {
            ArrayList tmpAncestors = new ArrayList();
            ArrayList tmpGlobalRelations = new ArrayList();
            ArrayList ListRole = null;
            ArrayList ListRoleType = null;
            //
            if (null != pRoleRestrict)
                ListRole = new ArrayList(pRoleRestrict);

            if (null != pRoleTypeExclude)
                ListRoleType = new ArrayList(pRoleTypeExclude);
            //
            string sqlSelect = $@"select ar.IDA_ACTOR,ar.IDROLEACTOR
            from dbo.ACTORROLE ar
            inner join ROLEACTOR ra on (ra.IDROLEACTOR = ar.IDROLEACTOR)  
            left outer join dbo.ACTOR anc on  (anc.IDA=ar.IDA_ACTOR) and {OTCmlHelper.GetSQLDataDtEnabled(pSource, "anc")}
            where (ar.IDA = @IDA) and {OTCmlHelper.GetSQLDataDtEnabled(pSource, "ar")}";

            if (ArrFunc.IsFilled(pRoleRestrict))
                sqlSelect += $" and (ar.IDROLEACTOR in ({DataHelper.SQLCollectionToSqlList(pSource, ListRole, TypeData.TypeDataEnum.@string)}))";

            if (ArrFunc.IsFilled(pRoleTypeExclude))
                sqlSelect += $" and (ra.ROLETYPE not in ({DataHelper.SQLCollectionToSqlList(pSource, ListRoleType, TypeData.TypeDataEnum.@string)}))";

            if (pIsPartyRelation)
                sqlSelect += " and (ra.ISPARTYRELATION=1)";


            //20070717 FI utilisation de ExecuteDataTable pour le Cache
            
            DataParameters dp = new DataParameters();
            dp.Add(DataParameter.GetParameter(pSource, DataParameter.ParameterEnum.IDA), actorRelation.ActorRole.IdA_Actor);
            QueryParameters qryParameters = new QueryParameters(pSource, sqlSelect, dp);
            DataTable dt = DataHelper.ExecuteDataTable(pSource, pDbTransaction, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter());
            if (null != dt)
            {
                foreach (DataRow row in dt.Rows)
                {
                    int ancIda = 0;
                    bool isExistsAncIda = (false == Convert.IsDBNull(row["IDA_ACTOR"]));
                    bool isAncIdaEqualIda = false;
                    if (isExistsAncIda)
                    {
                        ancIda = Convert.ToInt32(row["IDA_ACTOR"]);
                        isAncIdaEqualIda = (ancIda == actorRelation.ActorRole.IdA_Actor);
                    }

                    string ancIdRoleActor = Convert.ToString(row["IDROLEACTOR"]);
                    RoleActor roleActor;

                    // 20110512 MF supprimé le try catch (performance) et rajouté le trim avant le cast (résolution bug)
                    if (Enum.IsDefined(typeof(RoleActor), ancIdRoleActor.Trim()))
                    {
                        roleActor = (RoleActor)Enum.Parse(typeof(RoleActor), ancIdRoleActor, true);
                    }
                    else
                    {
                        roleActor = RoleActor.EXTERNAL;
                    }

                    //
                    if (isExistsAncIda && !isAncIdaEqualIda)
                    {
                        // add des parents du parent courant (==this.ActorRelation.ActorRole.IdA_Actor)
                        // EG 20220519 [WI637] Ajout paramètre pRoleTypeExclude
                        tmpAncestors.Add(new ActorAncestor(pSource, pDbTransaction, new ActorRole(actorRelation.ActorRole.IdA_Actor, roleActor, ancIda), actorRelation.Level + 1, pRoleRestrict, pRoleTypeExclude, pIsPartyRelation));
                    }
                    else
                    {
                        // Rôle du parent en cours vis à vis de lui même (ancIda = IdA_Actor) ou
                        // Rôle du parent en cours vis à vis de tous     (ancIda = 0) 
                        // ici ancIda est soit lui-même soit 0 (=null)
                        tmpGlobalRelations.Add(new ActorRole(actorRelation.ActorRole.IdA_Actor, roleActor, ancIda));
                        //tmpGlobalRelations.Add(new ActorRole(ancIda,roleActor,this.ActorRelation.ActorRole.IdA_Actor));
                    }
                }
            }

            if (tmpAncestors.Count > 0)
            {
                directAncestors = new ActorAncestor[tmpAncestors.Count];
                for (int index = 0; index < tmpAncestors.Count; index++)
                    directAncestors[index] = (ActorAncestor)tmpAncestors[index];
            }
            if (tmpGlobalRelations.Count > 0)
            {
                globalRelations = new ActorRole[tmpGlobalRelations.Count];
                for (int index = 0; index < tmpGlobalRelations.Count; index++)
                    globalRelations[index] = (ActorRole)tmpGlobalRelations[index];
            }
        }
        #endregion LoadAncestors
        //
        #region public GetDirectAncestorsListIda
        public ArrayList GetDirectAncestorsListIda()
        {
            // Retourne dans un array les parents (et la relation Parent-Enfant) de l'acteur Courant
            // Un enfant peut avoir plusieurs parents ou le même parent mais avec des liens différents (EX A CLIENT B et A DESK B retourne 2 lignes)  
            ArrayList list = new ArrayList();
            for (int i = 0; i < directAncestors.Length; i++)
            {
                ActorAncestor directAncestor = (ActorAncestor)directAncestors[i];
                list.Add(directAncestor.actorRelation.ActorRole.IdA_Actor);
            }
            return list;
        }
        #endregion GetDirectAncestorsListIda
        //	
        #region public GetRelationbyLevel
        public void GetRelationbyLevel(int plevel, ref ArrayList opList)
        {
            // Retourne dans un array toutes les relations Parent-Enfant pour un level Donnée 
            // si le niveau n'est pas celui de l'instance en cours => Redirection vers les parents
            // Permet de partir de l'instance de Level 0 (cas général)
            if (plevel == actorRelation.Level)
            {
                bool isCountains = false;
                for (int i = 0; i < opList.Count; i++)
                {
                    //une relation déjà existante ne pas ajoute pas d'entrée 
                    if (0 == actorRelation.CompareTo((ActorRelation)opList[i]))
                        isCountains = true;
                }
                if (!isCountains)
                {
                    // => Add only actorRole (represente une relation)
                    // le level de la relation est portée par index de l'array opList
                    // aucun doublon
                    opList.Add(actorRelation);
                }
            }
            else
            {
                if ((null != directAncestors) && (plevel > actorRelation.Level))
                {
                    for (int i = 0; i < directAncestors.Length; i++)
                        directAncestors[i].GetRelationbyLevel(plevel, ref opList);
                }
            }
        }
        #endregion public GetRelationbyLevel
        //
        #region public GetRelations
        public ArrayList GetRelations()
        {
            //Retourne array avec Les relations parents Enfants du niveau courant au niveau Max 
            //Chaque item correspond à un nivau, il contient un arraylist avec les relations de ce nivau
            ArrayList list = new ArrayList();
            //
            for (int i = actorRelation.Level; i < GetLevelLength(); i++)
            {
                ArrayList listLevel = new ArrayList();
                GetRelationbyLevel(i, ref listLevel);
                list.Add(listLevel);
            }
            //			
            return list;
        }
        #endregion GetRelations

        #region public GetFirstRelation/GetFirstRelation2
        /// <summary>
        /// Obtient le 1er Acteur qui a le rôle {pRoleActor}
        /// <para>Ce rôle peut être attribué vis à vis d'un autre acteur</para>
        /// <para>Ce rôle peut être attribué vis à vis d'aucun acteur</para> 
        /// </summary>
        /// <param name="pRoleActor"></param>
        /// <returns></returns>
        public int GetFirstRelation(RoleActor pRoleActor)
        {
            int index = 0;
            string type = null;
            return GetFirstRelation2(pRoleActor, ref index, ref type);
        }
        //PL 20110404 New GetFirstRelation2() & opIndex
        private int GetFirstRelation2(RoleActor pRoleActor, ref int opIndex, ref string opType)
        {

            bool isFound = false;
            opIndex++;
            opType = string.Empty;
            int retIda = 0;
            //
            if ((!isFound) && (actorRelation.ActorRole.Role == pRoleActor))
            {
                #region Actor
                isFound = true;
                opType = "Actor";
                retIda = actorRelation.ActorRole.IdA;
                #endregion
            }
            //
            if ((!isFound) && (null != globalRelations))
            {
                #region globalRelations
                //Recherche dans les rôles globals (vis à vis de tous ou de soit même: IDA_ACTOR = null|IDA)
                for (int i = 0; i < globalRelations.Length; i++)
                {
                    ActorRole actorRole = (ActorRole)globalRelations[i];
                    if (actorRole.Role == pRoleActor)
                    {
                        isFound = true;
                        opType = "globalRelations";
                        retIda = actorRole.IdA_Actor;
                        if (retIda == 0)//Si égal à zéro, on considère que l'acteur à le rôle vis à vis de lui même
                            retIda = actorRole.IdA;
                        break;
                    }
                }
                #endregion
            }
            //
            if ((!isFound) && (null != directAncestors))
            {
                #region directAncestors
                int lastIndexFound = -1;
                for (int i = 0; i < directAncestors.Length; i++)
                {
                    //tmpIda = directAncestors[i].GetFirstRelation(pRoleActor);
                    int indexFound = opIndex;
                    string type = null;
                    int tmpIda = directAncestors[i].GetFirstRelation2(pRoleActor, ref indexFound, ref type);
                    if (tmpIda > 0)
                    {
                        isFound = true;
                        if ((lastIndexFound == -1) || (indexFound < lastIndexFound) || ((indexFound == lastIndexFound) && (type == "Actor")))
                        {
                            lastIndexFound = indexFound;
                            opType = "directAncestors";
                            retIda = tmpIda;
                            //break;
                        }
                    }
                }
                if (isFound)
                    opIndex += lastIndexFound;
                #endregion
            }
            return retIda;

        }
        #endregion GetFirstRelation/GetFirstRelation2

        #region public GetListIdA_ActorByLevel
        public string GetListIdA_ActorByLevel(int pLevel)
        {
            return GetListIdA_ActorByLevel(pLevel, ",");
        }
        public string GetListIdA_ActorByLevel(int pLevel, string pSeparator)
        {
            // Retourne list  des IdA_Ancestor d'un level
            ArrayList listRelation = new ArrayList();
            GetRelationbyLevel(pLevel, ref listRelation);
            //
            StrBuilder list = new StrBuilder();
            for (int i = 0; i < listRelation.Count; i++)
            {
                string currentValue = ((ActorRelation)listRelation[i]).ActorRole.IdA_Actor.ToString();
                list += currentValue;
                if (i != listRelation.Count - 1)
                    list += pSeparator;
            }
            //
            return list.ToString();
        }
        #endregion public GetListIdA_ActorByLevel

        #region public GetListIdA_Ancestor
        public string GetListIdA_Ancestor()
        {
            string list = string.Empty;
            for (int i = actorRelation.Level; i < this.GetLevelLength(); i++)
            {
                string listLevel = GetListIdA_ActorByLevel(i);
                if (StrFunc.IsFilled(listLevel))
                {
                    if (StrFunc.IsFilled(list))
                        list += ",";
                    list += listLevel;
                }
            }
            //
            return list;
        }
        #endregion GetListIdA_Ancestor

        #region public GetLevelLength
        public int GetLevelLength()
        {
            int ret = actorRelation.Level;
            bool isContinue = true;
            //
            while (isContinue)
            {
                ret++;
                ArrayList list = new ArrayList();
                this.GetRelationbyLevel(ret, ref list);
                isContinue = (list.Count > 0);
            }
            return ret;
        }
        #endregion GetLevelLength
    }

    /// <summary>
    /// 
    /// </summary>
    public class ActorRole : IComparable
    {
        #region Membres
        /// <summary>
        /// Acteur 
        /// </summary>
        private int m_IdA;
        /// <summary>
        /// Acteur parent
        /// </summary>
        private int m_IdA_Actor;
        /// <summary>
        /// Rôle de l'acteur via à vis de l'acteur parent
        /// </summary>
        private RoleActor m_Role;
        #endregion Membres
        //
        #region properties
        /// <summary>
        /// Obtient l'acteur 
        /// </summary>
        [XmlAttribute(AttributeName = "ida")]
        public int IdA
        {
            get { return m_IdA; }
            set { m_IdA = value; }
        }
        /// <summary>
        /// Obtient le rôle de l'acteur
        /// </summary>
        [XmlAttribute(AttributeName = "role")]
        public RoleActor Role
        {
            get { return m_Role; }
            set { m_Role = value; }
        }
        /// <summary>
        /// Obtient l'acteur parent (Lorsque le rôle est attribué vis à vis d'un acteur parent)
        /// </summary>
        [XmlAttribute(AttributeName = "idaActor")]
        public int IdA_Actor
        {
            get { return m_IdA_Actor; }
            set { m_IdA_Actor = value; }
        }
        #endregion properties
        //
        #region constructor
        public ActorRole() { }
        public ActorRole(int pIdA, RoleActor pRole, int pIdA_Actor)
        {
            // m_ida est Role de m_IdA_Actor  Ex m_ida est CLIENT de m_IdA_Actor
            //								  Ex m_ida est ENTITY de m_IdA_Actor						
            m_IdA = pIdA;
            m_Role = pRole;
            m_IdA_Actor = pIdA_Actor;
        }

        public ActorRole(int pIdA, RoleActor pRole)
            :
            this(pIdA, pRole, 0) { }
        #endregion constructor
        //
        #region CompareTo
        public int CompareTo(object pObj)
        {
            if (pObj is ActorRole actorRole)
            {
                int ret = 0; //Like Equal
                if (actorRole.m_IdA != m_IdA)
                    ret = -1;
                if (actorRole.m_IdA_Actor != m_IdA_Actor)
                    ret = -1;
                if (actorRole.m_Role != m_Role)
                    ret = -1;
                return ret;
            }
            throw new ArgumentException("object is not a ActorRole");
        }
        #endregion CompareTo
    }

    /// <summary>
    /// 
    /// </summary>
    public class ActorRelation : IComparable
    {
        #region declaration
        // class utilisée dans le cades de actorAncestor 
        // permet de définir l'ensemble des roles des acteur vis à vis des autres à chaque niveau hiérarchique
        public ActorRole ActorRole;
        public int Level;
        #endregion declaration

        #region Constructor
        public ActorRelation(ActorRole pActorRole, int plLevel)
        {
            this.ActorRole = pActorRole;
            this.Level = plLevel;
        }
        #endregion Constructor

        #region CompareTo
        public int CompareTo(object pObj)
        {
            ActorRelation actorRelation = (ActorRelation)pObj;
            if (null != actorRelation)
            {
                int ret = actorRelation.ActorRole.CompareTo(this.ActorRole);
                if (0 == ret)
                {
                    if (actorRelation.Level != this.Level)
                        ret = -1;
                }
                return ret;
            }
            throw new ArgumentException("object is not a ActorRelation");
        }
        #endregion CompareTo

    }
    
    /// <summary>
    /// Liste ACTORROLE
    /// <para>Chaque item contient un acteur et un rôle</para>
    /// </summary>
    public class ActorRoleCollection : CollectionBase, IComparable
    {
        #region constructor
        public ActorRoleCollection() { }
        #endregion constructor

        #region indexor
        public ActorRole this[int pIndex]
        {
            get { return (ActorRole)this.List[pIndex]; }
        }
        #endregion indexor

        #region public Add
        /// <summary>
        /// Ajoute couple {Actor,Rôle} s'il n'existe pas
        /// </summary>
        /// <param name="pActorRole"></param>
        public void Add(ActorRole pActorRole)
        {
            if (false == IsExistActorRole(pActorRole))
                this.List.Add(pActorRole);
        }
        #endregion public Add

        #region public IsExistActorRole
        public bool IsExistActorRole(ActorRole pActorRole)
        {
            bool ret = false;
            for (int i = 0; i < List.Count; i++)
            {
                if (0 == pActorRole.CompareTo((ActorRole)List[i]))
                {
                    ret = true;
                    if (ret)
                        break;
                }
            }
            return ret;
        }
        #endregion

        #region public IsActorRole
        /// <summary>
        /// Retourne r
        /// true si l'acteur {pIdA} est présent et  possède le rôle {pRole}
        /// </summary>
        /// <param name="pIdA"></param>
        /// <param name="pRole"></param>
        /// <returns></returns>
        public bool IsActorRole(int pIdA, RoleActor pRole)
        {
            bool IsOk = false;
            for (int i = 0; i < this.Count; i++)
            {
                ActorRole actRole = (ActorRole)this[i];
                if ((pRole == actRole.Role) && (pIdA == actRole.IdA))
                {
                    IsOk = true;
                    break;
                }
            }
            return IsOk;
        }
        #endregion

        #region public GetListActor
        /// <summary>
        /// Retourne les acteurs présents
        /// </summary>
        /// <returns></returns>
        public int[] GetListActor()
        {
            return GetListActor(null);
        }
        /// <summary>
        /// Retourne les acteurs présents qui possèdent les rôles {pRole}
        /// </summary>
        /// <param name="pArrayRole">Si null, rerourne tous les acteurs</param>
        /// <returns></returns>
        public int[] GetListActor(RoleActor[] pRole)
        {
            ArrayList actorList = new ArrayList();
            int[] ret = null;
            for (int i = 0; i < Count; i++)
            {
                bool isOk = true;
                int ida = ((ActorRole)this[i]).IdA;
                if (null != pRole)
                {
                    isOk = false;
                    for (int j = 0; j < pRole.Length; j++)
                    {
                        isOk = IsActorRole(ida, pRole[j]);
                        if (isOk)
                            break;
                    }
                }

                if (isOk)
                {
                    if (!actorList.Contains(ida))
                        actorList.Add(ida);
                }
            }

            if (ArrFunc.IsFilled(actorList))
                ret = (int[])actorList.ToArray(typeof(int));

            return ret;
        }

        #endregion GetListActor

        #region public CompareTo
        public int CompareTo(object pObj)
        {
            if (pObj is ActorRoleCollection actorRoleCol)
            {
                int ret = 0;
                if (actorRoleCol.Count != this.Count)
                    ret = -1;
                //
                if (0 == ret)
                {
                    foreach (ActorRole actorRole in this)
                    {
                        foreach (ActorRole actorRole2 in actorRoleCol)
                        {
                            ret = actorRole.CompareTo(actorRole2);
                            if (0 == ret)
                                break;
                        }
                        if (-1 == ret)
                            break;
                    }
                }
                //
                return ret;
            }
            throw new ArgumentException("object is not a CollectionActorRole");
        }
        #endregion CompareTo
    }
    
    /// <summary>
    ///  Class de recherche des acteurs parents avec un rôle spécifique 
    /// </summary>
    // EG 20180205 [23769] Add dbTransaction  
    public class SearchAncestorRole
    {
        #region Members
        /// <summary>
        /// 
        /// </summary>
        private readonly int ida;
        /// <summary>
        /// 
        /// </summary>
        private readonly RoleActor roleActor;
        /// <summary>
        /// 
        /// </summary>
        private readonly string source;
        private readonly IDbTransaction dbTransaction;
        #endregion Members

        /// <summary>
        ///  Obtient le Rôle des acteurs parents recherchés
        /// </summary>
        /// FI 20240218 [WI838] add
        public RoleActor RoleActor => roleActor;

        /// <summary>
        ///  Obtient l'acteur inital
        /// </summary>
        /// FI 20240218 [WI838] add
        public int IdA => ida;


        #region constructor
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pDbTransaction"></param>
        /// <param name="pIdA">Représente l'acteur initial</param>
        /// <param name="pRole">Role recherché</param>
        public SearchAncestorRole(string pCS, IDbTransaction pDbTransaction, int pIdA, RoleActor pRole)
        {
            ida = pIdA;
            roleActor = pRole;
            source = pCS;
            dbTransaction = pDbTransaction;
        }
        #endregion constructor

        #region public GetActors
        /// <summary>
        ///  Retourne le list des acteurs avec le rôle <see cref="RoleActor"/> de manière ordonné (Parent,GrantParent, etc...)
        /// </summary>
        /// <param name="pRelationRole">Liste des rôles devant être utilisés dans la hierarchie. Null accepté si tous les rôles peuvent être considérés</param>
        /// <param name="pRoleTypeExclude">Exclusion de certains type de rôle pouvant être utilisés dans la hierarchie. Null accepté si aucune exclusion</param>
        // EG 20180205 [23769] Add dbTransaction  
        // EG 20220519 [WI637] Ajout paramètre pRoleTypeExclude
        public int[] GetActors(RoleActor[] pRelationRole, RoleType[] pRoleTypeExclude)
        {
            ArrayList actorList = new ArrayList();
            ActorAncestor ancestor = new ActorAncestor(source, dbTransaction, ida, pRelationRole, pRoleTypeExclude, false);

            

            for (int i = 0; i < ancestor.GetLevelLength(); i++)
            {
                ArrayList levelList = new ArrayList();
                ancestor.GetRelationbyLevel(i, ref levelList);
                if (null != levelList)
                {
                    for (int j = 0; j < levelList.Count; j++)
                    {
                        int currentIdA = ((ActorRelation)levelList[j]).ActorRole.IdA_Actor;
                        if (ActorTools.IsActorWithRole(source, dbTransaction, currentIdA, roleActor, 0))
                            actorList.Add(currentIdA);
                    }
                }
            }
            int[] ret = (int[])actorList.ToArray(typeof(int));
            return ret;
        }
        #endregion public GetActors
    }

    /// <summary>
    ///  Represente un utilisateur de Spheres® (User,SysAdmin, SysOper, etc...)
    ///  <para>Equivalent à Collaborator en plus light</para>
    /// </summary>
    /// EG 20210419 [XXXXX] New type ActorIdentification pour _entityIdentification, _departementIdentification
    public class User
    {
        #region Members
        /// <summary>
        /// Représente son identifiant 
        /// </summary>
        SpheresIdentification _identification;

        /// <summary>
        /// Représente les acteurs parents
        /// </summary>
        private ActorAncestor _actorAncestor;

        /// <summary>
        /// Représente le type d'utilisateur
        /// </summary>
        private RoleActor _userType;

        /// <summary>
        /// Représente son entité de rattachement
        /// </summary>
        readonly ActorIdentification _entityIdentification;

        /*
        /// <summary>
        /// Représente les entité enfants de 1er niveau de l'entité de rattachement 
        /// </summary>
        /// FI 20161114 [RATP] Add
        SpheresIdentification[] _childIdentityIdentification;
         */

        /// <summary>
        /// Représente son departement de rattachement
        /// </summary>
        /// FI 20140519 [19923] add member
        readonly ActorIdentification _departmentIdentification;
        #endregion Members

        #region Accessor
        /// <summary>
        /// Obtient l'id de l' utilisateur
        /// </summary>
        public int IdA
        {
            get {return _identification.OTCmlId;           }
        }

        /// <summary>
        /// Obtient l'identification de l'utilisateur
        /// </summary>
        public SpheresIdentification Identification
        {
            get {return _identification;}
            private set {_identification = value;}
        }

        /// <summary>
        /// Obtient tous les acteurs parents
        /// </summary>
        public ActorAncestor ActorAncestor
        {
            get {return _actorAncestor;}
            private set {_actorAncestor = value;}
        }

        /// <summary>
        /// Obtient le type de user (GUEST,SYSADM,SYSOPER,USER,...)
        /// </summary>
        public RoleActor UserType
        {
            get {return _userType;}
            private set {_userType = value;}
        }

        /// <summary>
        /// Obtient true si l'acteur est adminsitrateur
        /// </summary>
        public bool IsSessionSysAdmin
        {
            get {return ActorTools.IsUserType_SysAdmin(UserType);}
        }

        /// <summary>
        /// Obtient true si l'acteur est sys opérateur
        /// </summary>
        public bool IsSessionSysOper
        {
            get {return ActorTools.IsUserType_SysOper(UserType);}
        }

        /// <summary>
        /// Obtient true si l'acteur est invité
        /// </summary>
        public bool IsSessionGuest
        {
            get {return ActorTools.IsUserType_Guest(UserType);}
        }

        /// <summary>
        /// Obtient true si l'acteur a le rôle USER
        /// </summary>
        public bool IsSessionUser
        {
            get {return ActorTools.IsUserType_User(UserType);}
        }
        
        /// <summary>
        /// Obtient l'identification de l'entité de rattachement du user
        /// <para>Obtient null si l'utilisateur n'est pas rattaché à une entité</para>
        /// </summary>
        /// FI 20140519 [19923] add property
        public ActorIdentification EntityIdentification
        {
            get {return _entityIdentification;}
        }

        /// <summary>
        /// Obtient l'identification du departement de rattachement du user
        /// <para>Obtient null si l'utilisateur n'est pas rattaché à une entité</para>
        /// </summary>
        public ActorIdentification DepartmentIdentification
        {
            get { return _departmentIdentification; }
        }
        
        /// <summary>
        /// Obtient l'id de l'entité de rattachement de l'utilisateur
        /// <para>Obtient 0 si l'utilisateur n'est pas rattaché à une entité</para>
        /// </summary>
        public int Entity_IdA
        {
            get
            {
                int ret = 0;
                if (null != EntityIdentification)
                    ret = EntityIdentification.OTCmlId;
                return ret;
            }
        }
        /// EG 20210419 [XXXXX] New
        public string Entity_BusinessCenter
        {
            get
            {
                string ret = string.Empty;
                if (null != EntityIdentification)
                    ret = EntityIdentification.BusinessCenter;
                if (String.IsNullOrEmpty(ret))
                    ret = SystemSettings.GetAppSettings("Spheres_ReferentialDefault_businesscenter");
                return ret;
            }
        }
        /// <summary>
        /// Obtient l'id de l'acteur departement de rattachement de l'utilisateur
        /// <para>Obtient 0 si l'utilisateur n'est pas rattaché à un departement</para>
        /// </summary>
        /// FI 20140519 [19923] add property
        public int Department_IdA
        {
            get
            {
                int ret = 0;
                if (null != DepartmentIdentification)
                    ret = DepartmentIdentification.OTCmlId;
                return ret;
            }
        }
        #endregion

        #region constructor
        /// <summary>
        ///  Constructeur basique 
        ///  <para>Pas d'entité de rattachement</para>
        ///  <para>L'identification de l'acteur est alimentée avec IDA uniquement</para>
        /// </summary>
        /// <param name="pIda"></param>
        /// <param name="pActorAncestor"></param>
        /// <param name="pUserType"></param>
        /// EG 20210419 [XXXXX] Upd ActorIdentification
        public User(int pIda, ActorAncestor pActorAncestor, RoleActor pUserType)
        {
            _identification = new ActorIdentification
            {
                OTCmlId = pIda
            };
            _actorAncestor = pActorAncestor;
            _userType = pUserType;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCollaborator"></param>
        /// FI 20140519 [19923] alimentation de _departmentIdentification
        /// EG 20210419 [XXXXX] Upd ActorIdentification
        public User(Collaborator pCollaborator)
        {
            _identification = new SpheresIdentification
            {
                OTCmlId = pCollaborator.Ida,
                Identifier = pCollaborator.Identifier,
                Displayname = pCollaborator.DisplayName,
                Description = string.Empty,
                Timezone = pCollaborator.Timezone,
                Extllink = pCollaborator.ExtlLink
            };

            _actorAncestor = pCollaborator.ActorAncestor;

            _userType = RoleActor.USER;
            if (null != pCollaborator.UserType)
                _userType = pCollaborator.UserType.Value;

            if (pCollaborator.Entity.IsEntity)
                _entityIdentification = ConvertToActorIdentification(pCollaborator.Entity);

            if (pCollaborator.Entity.IsDepartment)
                _departmentIdentification = ConvertToActorIdentification(pCollaborator.Department);
        }
        
        /// <summary>
        ///  Retourne les entités de 1er niveau enfant de l'entité de rattachement de l'utilisateur 
        /// </summary>
        /// FI 20161114 [RATP] Add Method
        /// FI 20161114 [RATP] GLOP c'est totalement à revoir (utiliser pour SESSIONRESTRICT)
        public int[] GetChildEntity(string pCS)
        {
            int[] ret = null;

            if (null != _entityIdentification)
            {
                DataParameters dp = new DataParameters();
                dp.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.IDA_ENTITY), _entityIdentification.OTCmlId);
                string query = @"select IDA from dbo.ACTORROLE where IDA_ACTOR = @IDAENTITY and IDROLEACTOR = 'ENTITY'";

                QueryParameters qryParemeters = new QueryParameters(pCS, query, dp);

                DataTable dt = DataHelper.ExecuteDataTable(pCS, qryParemeters.Query, qryParemeters.Parameters.GetArrayDbParameter());
                if (dt.Rows.Count > 0)
                {
                    List<DataRow> rows = dt.Rows.Cast<DataRow>().ToList();
                    ret = (from item in rows
                           select Convert.ToInt32(item["IDA"])).ToArray();
                }
            }

            return ret;
        }


        /// <summary>
        /// Retourne un ConvertToActorIdentification à partir d'un  CollaboratorParent
        /// </summary>
        /// <param name="pColParent"></param>
        /// <returns></returns>
        // FI 20140519 [19923] add Method
        // PL 20171020 [23490] add Timezone
        /// EG 20210419 [XXXXX] Upd ActorIdentification
        private static ActorIdentification ConvertToActorIdentification(CollaboratorParent pColParent)
        {
            ActorIdentification ret = new ActorIdentification
            {
                OTCmlId = pColParent.Ida,
                Identifier = pColParent.Identifier,
                Displayname = pColParent.DisplayName,
                Description = pColParent.Description,
                Timezone = pColParent.Timezone,
                BusinessCenter = pColParent.BusinessCenter
            };
            ret.BusinessCenterSpecified = StrFunc.IsFilled(ret.BusinessCenter);
            return ret;
        }

        #endregion constructor

        #region Methods
        /// <summary>
        /// Retourne le desk de rattachement de l'utilisateur
        /// <para>Retoune 0 s'il n'existe pas</para>
        /// </summary>
        public int GetDesk()
        {
            return _actorAncestor.GetFirstRelation(RoleActor.DESK);
        }
        /// <summary>
        /// Retourne le département de rattachement de l'utilisateur
        /// <para>Retoune 0 s'il n'existe pas</para>
        /// </summary>
        public int GetDepartment()
        {
            return _actorAncestor.GetFirstRelation(RoleActor.DEPARTMENT);
        }

       

        /// <summary>
        /// Recherche des identifiants de l'acteur
        /// </summary>
        /// <param name="pCS"></param>
        public void SetActorIdentification(string pCS)
        {
            if (IdA == 0)
                throw new Exception("Actor IdA not defined");
            _identification = ActorTools.GetIdentification(pCS, IdA);
        }

        /// <summary>
        /// <para>Définit les acteurs parents</para>
        /// Affecte la property actorAncestor
        /// </summary>
        /// <param name="pCS"></param>
        // EG 20220519 [WI637] Ajout paramètre pRoleTypeExclude
        public void SetActorAncestor(string pCS)
        {
            if (IdA == 0)
                throw new Exception("Actor IdA not defined");

            ActorAncestor = new ActorAncestor(pCS, null, _identification.OTCmlId, pRoleRestrict: null, pRoleTypeExclude: null, pIsPartyRelation: false);
        }

        /// <summary>
        /// <para>Définit le type d'utilisateur</para>
        /// Affecte le property UserType
        /// </summary>
        /// <param name="pCS"></param>
        // EG 20230709 [XXXXX][WI551] Upd ACTORROLE => DTENABLED/DTDISABLED are not considered
        public void SetUsertType(string pCS)
        {
            if (IdA == 0)
                throw new Exception("Actor IdA not defined");
            //
            _userType = RoleActor.USER;
            UserRoleType _userRoleType = ActorTools.GetUserType(pCS, _identification.OTCmlId);
            if (null != _userRoleType)
                UserType = _userRoleType.Role;
        }

        /// <summary>
        ///  Retourne true si les sessions rattachées doivent appliquer SESSIONRESTRICT
        ///  <para>SESSIONRESTRICT est utilisé si user non admin et que la fonctionalité est activée via le fichier de config</para>
        /// </summary>
        /// FI 20141107 [20441] Add  
        public bool IsApplySessionRestrict()
        {
            Boolean ret = (false == IsSessionSysAdmin);
            if (ret)
                ret = BoolFunc.IsTrue(SystemSettings.GetAppSettings("DataRestrictionEnabled", "true"));
            return ret;
        }


        #endregion
    }

    /// <summary>
    /// 
    /// </summary>
    public sealed partial class ActorTools
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pRole"></param>
        /// <returns></returns>
        public static bool IsUserType_SysAdmin(RoleActor pRole)
        {
            return (pRole == RoleActor.SYSADMIN);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pRole"></param>
        /// <returns></returns>
        public static bool IsUserType_SysAdmin(string pRole)
        {
            if (StrFunc.IsFilled(pRole))
                return (pRole.TrimEnd() == RoleActor.SYSADMIN.ToString());
            else
                return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pRole"></param>
        /// <returns></returns>
        public static bool IsUserType_SysOper(RoleActor pRole)
        {
            return (pRole == RoleActor.SYSOPER);
        }
     

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pRole"></param>
        /// <returns></returns>
        public static bool IsUserType_User(RoleActor pRole)
        {
            return (pRole == RoleActor.USER);
        }
   

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pRole"></param>
        /// <returns></returns>
        public static bool IsUserType_Guest(RoleActor pRole)
        {
            return (pRole == RoleActor.GUEST);
        }


        // EG 20180205 [23769] Add dbTransaction  
        public static bool IsActorCSSorCLEARER(string pSource, IDbTransaction pDbTransaction, int pIdA)
        {
            return ActorTools.IsActorWithRole(pSource, pDbTransaction, pIdA, new RoleActor[] { RoleActor.CSS, RoleActor.CLEARER });
        }

        /// <summary>
        /// Retourne true si l'acteur a le rôle CLIENT
        /// </summary>
        /// <param name="pSource"></param>
        /// <param name="pIdA"></param>
        /// <returns></returns>
        // EG 20180205 [23769] Add dbTransaction  
        public static bool IsActorClient(string pSource, int pIdA)
        {
            return IsActorClient(pSource, null, pIdA);
        }
        // EG 20180205 [23769] Add dbTransaction  
        public static bool IsActorClient(string pSource, IDbTransaction pDbTransaction, int pIdA)
        {
            return ActorTools.IsActorWithRole(pSource, pDbTransaction, pIdA, RoleActor.CLIENT);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pIdA"></param>
        /// <returns></returns>
        // EG 20180205 [23769] Add dbTransaction  
        public static bool IsActorClientOrAccountConv(string pCS, int pIdA)
        {
            return IsActorClientOrAccountConv(pCS, null, pIdA);
        }
        // EG 20180205 [23769] Add dbTransaction  
        public static bool IsActorClientOrAccountConv(string pCS, IDbTransaction pDbTransaction, int pIdA)
        {
            return IsActorWithRole(pCS, pDbTransaction, pIdA, new RoleActor[] { RoleActor.CLIENT, RoleActor.ACCOUNTCONV });
        }

        /// <summary>
        /// Retourne true, si l'acteur a l'un des rôles suivant :{USER, GUEST, SYSADMIN, SYSOPER}
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pIdA"></param>
        /// <returns></returns>
        // EG 20180205 [23769] Add dbTransaction  
        public static bool IsActorUser(string pCS, int pIdA)
        {
            return IsActorUser(pCS, null, pIdA);
        }
        // EG 20180205 [23769] Add dbTransaction  
        public static bool IsActorUser(string pCS, IDbTransaction pDbTransaction, int pIdA)
        {
            RoleActor[] roleActor = new RoleActor[] { RoleActor.USER, RoleActor.GUEST, RoleActor.SYSADMIN, RoleActor.SYSOPER };
            return ActorTools.IsActorWithRole(pCS, pDbTransaction, pIdA, roleActor);
        }
        /// <summary>
        /// Retourne true si l'acteur possède l'un des rôles pRoleActor
        /// <para>Possibilité de vérifier que ces rôles sont attribués vis à vis d'un autre acteur</para>
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pDbTransaction">transaction</param>
        /// <param name="pIdA">Représente l'acteur</param>
        /// <param name="pRoleActor">Liste des rôles</param>
        /// <param name="pIdA_Actor">Représente l'acteur vis à vis duquel est attribué le rôle</param>
        /// <returns></returns>
        /// FI 20170404 [23039] Add pDbTransaction parameter 
        public static bool IsActorWithRole(string pCS, IDbTransaction pDbTransaction, int pIdA, RoleActor[] pRoleActor, int pIdA_Actor)
        {
            DataParameters dataParameters = new DataParameters();
            dataParameters.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.IDA), pIdA);
            if (pIdA_Actor > 0)
                dataParameters.Add(new DataParameter(pCS, "IDA_ACTOR", DbType.Int32), pIdA_Actor);

            ArrayList lstRole = new ArrayList(pRoleActor);
            string sqlLstRole = DataHelper.SQLCollectionToSqlList(pCS, lstRole, TypeData.TypeDataEnum.@string);

            StrBuilder sqlSelect = new StrBuilder(SQLCst.SELECT);
            sqlSelect += "IDROLEACTOR" + Cst.CrLf;
            sqlSelect += SQLCst.FROM_DBO + Cst.OTCml_TBL.ACTORROLE.ToString() + Cst.CrLf;
            sqlSelect += SQLCst.WHERE + "IDA=@IDA" + Cst.CrLf;
            if (pIdA_Actor > 0)
                sqlSelect += SQLCst.AND + "IDA_ACTOR=@IDA_ACTOR" + Cst.CrLf;
            sqlSelect += SQLCst.AND + "IDROLEACTOR in (" + sqlLstRole + ")";
            bool isOk = null != DataHelper.ExecuteScalar(pCS, pDbTransaction, CommandType.Text, sqlSelect.ToString(), dataParameters.GetArrayDbParameter());
            return isOk;
        }
        /// <summary>
        /// Recherche de l'acteur vis à vis de... pour un rôle
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pDbTransaction"></param>
        /// <param name="pIdA"></param>
        /// <param name="pRoleActor"></param>
        /// <returns></returns>
        // EG 20200311 [24683] New  
        public static KeyValuePair<int, bool> IsActorWithRoleRegardTo(string pCS, IDbTransaction pDbTransaction, int pIdA, RoleActor[] pRoleActor)
        {
            DataParameters dataParameters = new DataParameters();
            dataParameters.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.IDA), pIdA);
            ArrayList lstRole = new ArrayList(pRoleActor);
            string sqlLstRole = DataHelper.SQLCollectionToSqlList(pCS, lstRole, TypeData.TypeDataEnum.@string);

            string sqlSelect = @"select IDA_ACTOR
            from dbo.ACTORROLE
            where (IDA = @IDA) and (IDROLEACTOR in (" + sqlLstRole + "))";

            QueryParameters qryParameters = new QueryParameters(pCS, sqlSelect, dataParameters);
            object obj = DataHelper.ExecuteScalar(pCS, pDbTransaction, CommandType.Text, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter());
            KeyValuePair<int, bool> ret = new KeyValuePair<int, bool>((null != obj) ? Convert.ToInt32(obj) : 0, (null != obj));
            return ret;
        }

        /// <summary>
        /// Retourne true si l'acteur possède l'un des rôles pRoleActor
        /// <para>Possibilité de vérifier que ces rôles sont attribués vis à vis d'un autre acteur</para>
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pIdA">Représente l'acteur</param>
        /// <param name="pRoleActor">Liste des rôles</param>
        /// <param name="pIdA_Actor">Représente l'acteur vis à vis duquel est attribué le rôle</param>
        /// <returns></returns>
        /// FI 20170404 [23039] Modify
        public static bool IsActorWithRole(string pCS, int pIdA, RoleActor[] pRoleActor, int pIdA_Actor)
        {
            // FI 20170404 [23039] Appel à override avec paramètre transaction 
            return IsActorWithRole(pCS, null, pIdA, pRoleActor, pIdA_Actor);
        }


        /// <summary>
        /// Retourne true si l'acteur possède le rôle pRoleActor
        /// <para>Possibilité de vérifier que le  rôles est attribué vis à vis d'un autre acteur</para>
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pIdA"></param>
        /// <param name="pRoleActor"></param>
        /// <param name="pIdA_Actor"></param>
        /// <returns></returns>
        // EG 20180205 [23769] Add dbTransaction  
        public static bool IsActorWithRole(string pCS, int pIdA, RoleActor pRoleActor, int pIdA_Actor)
        {
            return IsActorWithRole(pCS, null, pIdA, pRoleActor, pIdA_Actor);
        }
        // EG 20180205 [23769] Add dbTransaction  
        public static bool IsActorWithRole(string pCS, IDbTransaction pDbTransaction, int pIdA, RoleActor pRoleActor, int pIdA_Actor)
        {
            return IsActorWithRole(pCS, pDbTransaction, pIdA, new RoleActor[] { pRoleActor }, pIdA_Actor);
        }
        /// <summary>
        /// Retourne true si l'acteur possède le rôle pRoleActor
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pIdA"></param>
        /// <param name="pRoleActor"></param>
        /// <returns></returns>
        // EG 20180205 [23769] Add dbTransaction  
        public static bool IsActorWithRole(string pCS, int pIdA, RoleActor pRoleActor)
        {
            return IsActorWithRole(pCS, null, pIdA, pRoleActor);
        }
        // EG 20180205 [23769] Add dbTransaction  
        public static bool IsActorWithRole(string pCS, IDbTransaction pDbTransaction, int pIdA, RoleActor pRoleActor)
        {
            return IsActorWithRole(pCS, pDbTransaction, pIdA, new RoleActor[] { pRoleActor }, 0);
        }
        /// <summary>
        /// Retourne true si l'acteur possède l'un des rôles pRoleActor
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pIdA"></param>
        /// <param name="pRoleActor"></param>
        /// <returns></returns>
        // EG 20180205 [23769] Add dbTransaction  
        public static bool IsActorWithRole(string pCS, int pIdA, RoleActor[] pRoleActor)
        {
            return IsActorWithRole(pCS, null, pIdA, pRoleActor);
        }
        // EG 20180205 [23769] Add dbTransaction  
        public static bool IsActorWithRole(string pCS, IDbTransaction pDbTransaction, int pIdA, RoleActor[] pRoleActor)
        {
            return IsActorWithRole(pCS, pDbTransaction, pIdA, pRoleActor, 0);
        }
        /// <summary>
        ///  Retourne l'Id de l'acteur en fonction d'un identifiant et du scheme qui lui est associé
        ///  <para>Exemple de sheme OTCml_ActorIdentifierScheme,OTCml_ActorBicScheme</para>
        /// </summary>
        /// <param name="pCs"></param>
        /// <param name="pValue"></param>
        /// <param name="pScheme"></param>
        /// <returns></returns>
        /// FI 20140206 [19564] add OTCml_ActorLEIScheme
        // EG 20180307 [23769] Gestion dbTransaction
        public static int GetIdAFromScheme(string pCS, IDbTransaction pDbTransaction, string pValue, string pScheme)
        {

            int ret = 0;
            //
            string column = string.Empty;
            DataParameter parameter = null;

            switch (pScheme)
            {
                case Cst.OTCml_ActorIdScheme:
                    column = "IDA";
                    parameter = new DataParameter(pCS, "PARAM", DbType.Int32)
                    {
                        Value = Convert.ToInt32(pValue)
                    };
                    break;
                case Cst.OTCml_ActorIdentifierScheme:
                    column = "IDENTIFIER";
                    parameter = new DataParameter(pCS, "PARAM", DbType.AnsiString, SQLCst.UT_IDENTIFIER_LEN)
                    {
                        Value = pValue
                    };
                    break;
                case Cst.OTCml_ActorBicScheme:
                    column = "BIC";
                    parameter = new DataParameter(pCS, "PARAM", DbType.AnsiString, 16)
                    {
                        Value = pValue
                    };
                    break;
                case Cst.OTCml_ActorLEIScheme:
                    column = "ISO17442";
                    parameter = new DataParameter(pCS, "PARAM", DbType.AnsiString, 20)
                    {
                        Value = pValue
                    };
                    break;
                default:
                    break;
            }
            //
            if (StrFunc.IsFilled(column))
            {
                StrBuilder query = new StrBuilder(SQLCst.SELECT + "IDA" + Cst.CrLf + SQLCst.FROM_DBO + "ACTOR");
                query += SQLCst.WHERE + column + "=@PARAM";
                //20070717 FI utilisation de ExecuteDataTable pour le Cache
                object obj = DataHelper.ExecuteScalar(pCS, pDbTransaction, CommandType.Text, query.ToString(), parameter.DbDataParameter);
                if (null != obj)
                    ret = Convert.ToInt32(obj);
                //
            }
            return ret;

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCodeBic"></param>
        /// <param name="pLTCode"></param>
        /// <returns></returns>
        public static string GetLTAdress(string pCodeBic, string pLTCode)
        {

            string LTCode = pLTCode;
            string ret = pCodeBic;
            //
            if (StrFunc.IsFilled(ret))
            {
                if (ret.Length >= 8)
                {
                    if (LTCode == null || LTCode.Trim().Length == 0)
                        LTCode = "X";
                }
                ret = ret.Substring(0, 8) + LTCode.Substring(0, 1) + ret.Substring(8);
            }
            ret += "XXXXXXXXXXXX";
            ret = ret.Substring(0, 12);
            return ret;

        }

        /// <summary>
        /// Retourne true s'il existe au minimum 1 acteur  avec le rôle {pRole}
        /// </summary>
        /// <param name="pCs"></param>
        /// <param name="pRole"></param>
        /// <returns></returns>
        public static bool ExistMultipleActorWithRole(string pCs, RoleActor pRole)
        {
            DataParameter param = DataParameter.GetParameter(pCs, DataParameter.ParameterEnum.IDROLEACTOR);
            param.Value = pRole.ToString();

            string sqlSelect = SQLCst.SELECT_DISTINCT + "IDA" + Cst.CrLf;
            sqlSelect += SQLCst.FROM_DBO + Cst.OTCml_TBL.ACTORROLE.ToString() + Cst.CrLf;
            sqlSelect += SQLCst.WHERE + "IDROLEACTOR = @IDROLEACTOR";
            //20070717 FI utilisation de ExecuteDataTable pour le Cache
            Object result = DataHelper.ExecuteScalar(pCs, CommandType.Text, sqlSelect, param.DbDataParameter);
            bool isOk = null != result;

            return isOk;
        }

        /// <summary>
        /// Retourne la liste des groupes d'acteur auxquels appartient un acteur
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pIdA"></param>
        /// <param name="pRole">Réduit la liste aux seuls groupes du rôle {pRole}, valeur null autorisé</param>
        /// <param name="pIsUseDataDtEnabled"></param>
        /// <returns></returns>
        // EG 20180205 [23769] Add dbTransaction  
        public static int[] GetGrpActor(string pCS, int pIdA, string pRole, bool pIsUseDataDtEnabled)
        {
            return GetGrpActor(pCS, null as IDbTransaction, pIdA, pRole, pIsUseDataDtEnabled);
        }
        // EG 20180205 [23769] Add dbTransaction  
        // EG 20180425 Analyse du code Correction [CA2202]
        public static int[] GetGrpActor(string pCS, IDbTransaction pDbTransaction, int pIdA, string pRole, bool pIsUseDataDtEnabled)
        {

            int[] ret = null;
            //
            DataParameters dp = new DataParameters();
            dp.Add(new DataParameter(pCS, "IDA", DbType.Int32), pIdA);
            if (StrFunc.IsFilled(pRole))
                dp.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.IDROLEGACTOR), pRole.ToString());
            //
            StrBuilder sqlSelect = new StrBuilder();
            sqlSelect += SQLCst.SELECT + "actorg.IDGACTOR" + Cst.CrLf;
            sqlSelect += SQLCst.FROM_DBO + Cst.OTCml_TBL.ACTORG.ToString() + " actorg" + Cst.CrLf;
            if (StrFunc.IsFilled(pRole))
            {
                sqlSelect += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.GACTORROLE.ToString() + " gActorRole on gActorRole.IDGACTOR = actorg.IDGACTOR" + Cst.CrLf;
                sqlSelect += SQLCst.AND + "gActorRole.IDROLEGACTOR = @IDROLEGACTOR" + Cst.CrLf;
            }
            sqlSelect += SQLCst.WHERE + "actorg.IDA=@IDA";
            //
            if (pIsUseDataDtEnabled)
            {
                sqlSelect += SQLCst.AND + OTCmlHelper.GetSQLDataDtEnabled(pCS, "actorg") + Cst.CrLf;
                if (StrFunc.IsFilled(pRole))
                    sqlSelect += SQLCst.AND + OTCmlHelper.GetSQLDataDtEnabled(pCS, "gActorRole") + Cst.CrLf;
            }
            //				
            QueryParameters qryParameters = new QueryParameters(pCS, sqlSelect.ToString(), dp);
            //				
            ArrayList al = new ArrayList();
            using (IDataReader dr =
                DataHelper.ExecuteDataTable(pCS, pDbTransaction, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter()).CreateDataReader())
            {
                while (dr.Read())
                    al.Add(Convert.ToInt32(dr[0]));
            }

            if (ArrFunc.IsFilled(al))
                ret = (int[])al.ToArray(typeof(int));

            return ret;
        }

        /// <summary>
        /// Retourne le rôle de l'acteur {pIdA}
        /// Ordre de traitement du plus restrictifs au plus extensif soit:
        /// GUEST > USER > SYSOPER > SYSADM
        /// Un contrôle de validity sur les rôle est opéré (DTENABLED|DTDISABLED) pour aurosier la connexion
        /// <para>Seules les valeurs SYSADMIN, SYSOPER, USER, GUEST sont considérés</para>
        /// </summary>
        /// <param name="pCs">Connexion</param>
        /// <param name="pIdA">Acteur concerné</param>
        /// EG 20230709 [XXXXX][WI551] ACTORROLE : DTENABLED/DTDISABLED are not considered
        public static UserRoleType GetUserType(string pCS, int pIdA)
        {
            UserRoleType userRoleType = null;

            string sqlSelect = $@"select ar.IDROLEACTOR, ar.DTENABLED, ar.DTDISABLED
            from dbo.ACTORROLE ar
            where (ar.IDA = @IDA) and 
                  (IDROLEACTOR in ('{RoleActor.SYSADMIN}','{RoleActor.SYSOPER}','{RoleActor.USER}','{RoleActor.GUEST}'))
            order by case ar.IDROLEACTOR 
            when '{RoleActor.GUEST}' then 0
            when '{RoleActor.USER}' then 1
            when '{RoleActor.SYSOPER}' then 2
            when '{RoleActor.SYSADMIN}' then 3 else 4 end";
            DataParameters dp = new DataParameters();
            dp.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.IDA), pIdA);

            QueryParameters qryParameters = new QueryParameters(pCS, sqlSelect, dp);

            using (IDataReader dr = DataHelper.ExecuteDataTable(pCS, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter()).CreateDataReader())
            {
                while (dr.Read())
                {
                    RoleActor roleActor = (RoleActor)ReflectionTools.ConvertStringToEnumOrNullable<RoleActor>(dr["IDROLEACTOR"].ToString().TrimEnd());
                    DateTime dtEnabled = Convert.ToDateTime(dr["DTENABLED"]);
                    DateTime dtDisabled = (dr["DTDISABLED"] is DBNull) ? DateTime.Today.Add(TimeSpan.FromDays(1)) : Convert.ToDateTime(dr["DTDISABLED"]);

                    if ((null == userRoleType) || (false == userRoleType.IsAvailable))
                    {
                        userRoleType = new UserRoleType(pIdA, roleActor, dtEnabled, dtDisabled);
                        if (userRoleType.IsAvailable)
                            break;
                    }
                }
            }

            return userRoleType;
        }

        /// <summary>
        /// Retourne l'identification d'un acteur
        /// <para>Retourne null si l'acteur n'existe pas</para>
        /// <para>Cette méthode ne vérifie pas si l'acteur est ENABLED</para>
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pIdA"></param>
        /// <returns></returns>
        // EG 20180425 Analyse du code Correction [CA2202]
        /// EG 20210419 [XXXXX] Upd ActorIdentification
        public static ActorIdentification GetIdentification(string pCS, int pIdA)
        {
            ActorIdentification ret = null;
            //
            DataParameters parameters = new DataParameters(new DataParameter[] { });
            parameters.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.IDA), pIdA);
            //
            StrBuilder sqlSelect = new StrBuilder(SQLCst.SELECT);
            sqlSelect += "a.IDA, a.IDENTIFIER, a.DISPLAYNAME, a.DESCRIPTION, a.IDBC, a.EXTLLINK " + Cst.CrLf;
            sqlSelect += SQLCst.FROM_DBO + Cst.OTCml_TBL.ACTOR.ToString() + " a" + Cst.CrLf;
            sqlSelect += SQLCst.WHERE + "a.IDA=@IDA";

            QueryParameters qry = new QueryParameters(pCS, sqlSelect.ToString(), parameters);

            using (IDataReader dr = DataHelper.ExecuteDataTable(qry.Cs, qry.Query, qry.Parameters.GetArrayDbParameter()).CreateDataReader())
            {
                if (dr.Read())
                {
                    ret = new ActorIdentification
                    {
                        OTCmlId = Convert.ToInt32(dr["IDA"]),
                        Identifier = dr["IDENTIFIER"].ToString(),
                        Displayname = dr["DISPLAYNAME"].ToString(),
                        Description = dr["DESCRIPTION"].ToString(),
                        Extllink = dr["EXTLLINK"].ToString(),
                        BusinessCenter = dr["IDBC"].ToString()
                    };
                    ret.BusinessCenterSpecified = StrFunc.IsFilled(ret.BusinessCenter);
                }
            }
            return ret;
        }
        /// <summary>
        /// Retourne l'acteur l'id non significatif de l'entité s'il en existe qu'1 seule paramétrée
        /// <para>Retourne null s'il existe n entités</para>
        /// </summary>
        /// <returns></returns>
        /// FI 20130925 [18990] Add method
        public static Nullable<int> GetSingleActorEntity(string pCS)
        {
            Nullable<int> ret = null;

            StrBuilder sqlSelect = new StrBuilder(SQLCst.SELECT);
            sqlSelect += "IDA" + Cst.CrLf;
            sqlSelect += SQLCst.FROM_DBO + Cst.OTCml_TBL.ENTITY.ToString();
            DataTable dt = DataHelper.ExecuteDataTable(pCS, sqlSelect.ToString());
            if (ArrFunc.Count(dt.Rows) == 1)
                ret = Convert.ToInt32(dt.Rows[0]["IDA"]);

            return ret;
        }

        /// <summary>
        /// Sales, trader, Algo
        /// </summary>
        private enum ActorRelativeToEnum
        {
            trader,
            algoExecution,
            sales,
            algoInvestmentDecision
        }

        /// <summary>
        ///  Retourne les liste des traders et algorithmes relatifs à l'acteur <paramref name="pIdA"/>
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pIdA">Lorsque non renseigné remonte tous traders et algorithmes</param>
        /// <param name="pUser"></param>
        /// <param name="pSessionID"></param>
        /// <returns>Tuple(IDA, IDENTIFIER, ROLEACTOR)</returns>
        /// FI 20170928 [23452] Add 
        public static IEnumerable<Tuple<int, string, RoleActor>> LoadTraderAlgo(string pCS, Nullable<int> pIdA, User pUser, string pSessionID)
        {
            IEnumerable<Tuple<int, string, RoleActor>> lst1 = SearchActorRelativeTo(pCS, pIdA, ActorRelativeToEnum.trader, pUser, pSessionID);
            IEnumerable<Tuple<int, string, RoleActor>> lst2 = SearchActorRelativeTo(pCS, pIdA, ActorRelativeToEnum.algoExecution, pUser, pSessionID);
            return lst1.Concat(lst2);
        }


        /// <summary>
        ///  Retourne les liste des sales et algoInvestmentDecision relatifs à l'acteur <paramref name="pIdA"/>
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pIdA">Lorsque non renseigné remonte tous les sales et algoInvestmentDecision</param>
        /// <param name="pUser">Représente l'utilisateur connecté</param>
        /// <param name="pSessionID">Représente la session Id de l'utilisateur connecté</param>
        /// <returns>Tuple(IDA, IDENTIFIER, ROLEACTOR)</returns>
        /// FI 20170928 [23452] Add 
        public static IEnumerable<Tuple<int, string, RoleActor>> LoadSalesAlgo(string pCS, Nullable<int> pIdA, User pUser, string pSessionID)
        {
            IEnumerable<Tuple<int, string, RoleActor>> lst1 = SearchActorRelativeTo(pCS, pIdA, ActorRelativeToEnum.sales, pUser, pSessionID);
            IEnumerable<Tuple<int, string, RoleActor>> lst2 = SearchActorRelativeTo(pCS, pIdA, ActorRelativeToEnum.algoInvestmentDecision, pUser, pSessionID);
            return lst1.Concat(lst2);
        }

        /// <summary>
        ///  Retourne les liste des acteurs de type <paramref name="pLoadTypeEnum"/> relatifs à l'acteur <paramref name="pIdA"/>
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pIdA">si null, charge tous les acteurs de de type</param>
        /// <param name="pLoadTypeEnum"></param>
        /// <param name="pUser">Représente l'utilisateur connecté</param>
        /// <param name="pSessionID">Représente la session Id de l'utilisateur connecté</param>
        /// <returns>Tuple(IDA, IDENTIFIER, ROLEACTOR)</returns>
        /// PL 20090612 Changement de type (pIdA est nullable)
        /// FI 20141107 [20441] Modification de signature 
        /// EG 20160404 Migration vs2013
        private static List<Tuple<int, string, RoleActor>> SearchActorRelativeTo(string pCS, Nullable<int> pIdA, ActorRelativeToEnum pLoadTypeEnum, User pUser, string pSessionID)
        {
            List<Tuple<int, string, RoleActor>> ret = new List<Tuple<int, string, RoleActor>>();

            if ((false == pIdA.HasValue) || pIdA.Value > 0)
            {
                bool isAll = (pIdA == null);

                DataParameters dataparameters = new DataParameters();
                if (ActorRelativeToEnum.trader == pLoadTypeEnum)
                    dataparameters.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.IDROLEACTOR), RoleActor.TRADER);
                else if (ActorRelativeToEnum.sales == pLoadTypeEnum)
                    dataparameters.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.IDROLEACTOR), RoleActor.SALES);
                else if (ActorRelativeToEnum.algoExecution == pLoadTypeEnum)
                    dataparameters.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.IDROLEACTOR), RoleActor.EXECUTION);
                else if (ActorRelativeToEnum.algoInvestmentDecision == pLoadTypeEnum)
                    dataparameters.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.IDROLEACTOR), RoleActor.INVESTDECISION);
                else
                    throw new NotImplementedException("ActorRelativeToEnum type NotImplemented");

                string sqlRestrict = string.Empty;
                if (pUser.IsApplySessionRestrict())
                {
                    SessionRestrictHelper sr = new SessionRestrictHelper(pUser, pSessionID, false);
                    sqlRestrict = sr.GetSQLActor(string.Empty, "a.IDA") + Cst.CrLf;
                }
                sqlRestrict += SQLCst.WHERE + OTCmlHelper.GetSQLDataDtEnabled(pCS, "a") + Cst.CrLf;

                if (pLoadTypeEnum == ActorRelativeToEnum.algoInvestmentDecision || pLoadTypeEnum == ActorRelativeToEnum.algoExecution)
                    sqlRestrict += "and a.ALGOTYPE is not null";
                else
                    sqlRestrict += "and a.ALGOTYPE is null";

                #region SQL Select
                string sqlSelect = SQLCst.SELECT + "a.IDA, a.IDENTIFIER as IDENTIFIER, ar.IDROLEACTOR" + Cst.CrLf;
                sqlSelect += SQLCst.FROM_DBO + Cst.OTCml_TBL.ACTOR + " a" + Cst.CrLf;
                sqlSelect += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.ACTORROLE + " ar on (ar.IDA=a.IDA)";
                sqlSelect += " and (ar.IDROLEACTOR in (@IDROLEACTOR))  and (ar.IDA_ACTOR=@IDA) and " + OTCmlHelper.GetSQLDataDtEnabled(pCS, "ar").ToString() + Cst.CrLf;
                sqlSelect += sqlRestrict;

                #region Add Sub Actors
                // EG 20160404 Migration vs2013
                //#warning 20050528 PL (non urgent) Finaliser la gestion des sous-acteurs (préproposition de tous les enfants, petits-enfants, ... )
                //20071114 PL Il faudrait utiliser ici une requête récursif avec le nouveau SQL et voir quel lien on doit contrôler...
                sqlSelect += SQLCst.UNION + Cst.CrLf;
                sqlSelect += SQLCst.SELECT + "a.IDA, a.IDENTIFIER as IDENTIFIER, ar.IDROLEACTOR" + Cst.CrLf;
                sqlSelect += SQLCst.FROM_DBO + Cst.OTCml_TBL.ACTOR + " a" + Cst.CrLf;
                sqlSelect += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.ACTORROLE + " ar on (ar.IDA=a.IDA) and (ar.IDROLEACTOR in (@IDROLEACTOR)) and " + OTCmlHelper.GetSQLDataDtEnabled(pCS, "ar").ToString() + Cst.CrLf;
                sqlSelect += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.ACTORROLE + " ar2 on (ar2.IDA=ar.IDA_ACTOR)";
                //20090619 PL Test
                //sqlSelect += SQLCst.AND + "ar2.IDROLEACTOR=@ROLE";
                sqlSelect += " and (ar2.IDROLEACTOR in (@IDROLEACTOR, 'DESK'))  and (ar2.IDA_ACTOR=@IDA) and " + OTCmlHelper.GetSQLDataDtEnabled(pCS, "ar2").ToString() + Cst.CrLf;
                sqlSelect += sqlRestrict;

                sqlSelect += SQLCst.UNION + Cst.CrLf;
                sqlSelect += SQLCst.SELECT + "a.IDA, a.IDENTIFIER as IDENTIFIER, ar.IDROLEACTOR" + Cst.CrLf;
                sqlSelect += SQLCst.FROM_DBO + Cst.OTCml_TBL.ACTOR + " a" + Cst.CrLf;
                sqlSelect += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.ACTORROLE + " ar on (ar.IDA=a.IDA) and (ar.IDROLEACTOR in (@IDROLEACTOR)) and " + OTCmlHelper.GetSQLDataDtEnabled(pCS, "ar").ToString() + Cst.CrLf;
                sqlSelect += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.ACTORROLE + " ar2 on (ar2.IDA=ar.IDA_ACTOR) and (ar2.IDROLEACTOR in (@IDROLEACTOR)) and " + OTCmlHelper.GetSQLDataDtEnabled(pCS, "ar2").ToString() + Cst.CrLf;
                sqlSelect += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.ACTORROLE + " ar3 on (ar3.IDA=ar2.IDA_ACTOR)";
                //20090619 PL Test
                //sqlSelect += SQLCst.AND + "ar3.IDROLEACTOR=@ROLE";
                sqlSelect += " and (ar3.IDROLEACTOR in (@IDROLEACTOR, 'DESK')) and (ar3.IDA_ACTOR=@IDA) and " + OTCmlHelper.GetSQLDataDtEnabled(pCS, "ar3").ToString() + Cst.CrLf;
                sqlSelect += sqlRestrict;
                #endregion

                sqlSelect += SQLCst.ORDERBY + "IDENTIFIER";
                #endregion sql Select

                if (isAll)
                    sqlSelect = sqlSelect.Replace(".IDA_ACTOR=@IDA)", ".IDA_ACTOR" + SQLCst.IS_NULL + ")");
                else
                    dataparameters.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.IDA), pIdA);

                QueryParameters qryParameters = new QueryParameters(pCS, sqlSelect, dataparameters);

                //FI 20090717 utilisation de ExecuteDataTable et CSTools.SetCacheOn pour optimisation (stockage dans le cache)
                using (DataTable dt = DataHelper.ExecuteDataTable(CSTools.SetCacheOn(pCS), qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter()))
                {
                    using (IDataReader dr = dt.CreateDataReader())
                    {
                        while (dr.Read())
                        {
                            ret.Add(new Tuple<int, string, RoleActor>(
                                Convert.ToInt32(dr["IDA"]),
                                dr["IDENTIFIER"].ToString(),
                                (RoleActor)Enum.Parse(typeof(RoleActor), dr["IDROLEACTOR"].ToString())
                            ));
                        }
                    };
                };
            }
            return ret;
        }
    }

    #region AmountPayerReceiverInfo
    /// <summary>
    /// Montant avec PayerReceiver complet (ID Spheres - Acteur et book)
    /// </summary>
    /// 20150515 EG [20513] New
    public class AmountPayerReceiverInfo : PayerReceiverInfo
    {
        #region Members
        public Nullable<PutOrCallEnum> putOrCall;
        public decimal amount;
        public string currency;
        #endregion Members
        #region Accessors
        public override decimal Amount
        {
            get { return amount; }
            set { amount = value; }
        }
        public override string Currency
        {
            get { return currency; }
            set { currency = value; }
        }
        #endregion Accessors

        #region Constructors
        public AmountPayerReceiverInfo() { }
        public AmountPayerReceiverInfo(Nullable<PutOrCallEnum> pPutOrCall, decimal pAmount, string pCurrency,
            int pIdA_Pay, string pActor_Pay, int pIdB_Pay, string pBook_Pay, int pIdA_Rec, string pActor_Rec, int pIdB_Rec, string pBook_Rec)
            : base(pIdA_Pay, pActor_Pay, pIdB_Pay, pBook_Pay, pIdA_Rec, pActor_Rec, pIdB_Rec, pBook_Rec)
        {
            putOrCall = pPutOrCall;
            amount = pAmount;
            currency = pCurrency;
        }
        // EG 20150706 [21021] Add pPutOrCall
        public AmountPayerReceiverInfo(Nullable<PutOrCallEnum> pPutOrCall, decimal pAmount, string pCurrency, PayerReceiverInfoDet pPayer, PayerReceiverInfoDet pReceiver)
            : base(pPayer, pReceiver)
        {
            putOrCall = pPutOrCall;
            amount = pAmount;
            currency = pCurrency;
        }
        #endregion Constructors
    }
    #endregion AmountPayerReceiverInfo
    #region QuantityPayerReceiverInfo
    /// <summary>
    /// Quantité avec PayerReceiver complet (ID Spheres - Acteur et book)
    /// </summary>
    /// 20150515 EG [20513] New
    // EG 20150920 [21374] Int (int32) to Long (Int64) 
    // EG 20170127 Qty Long To Decimal
    public class QuantityPayerReceiverInfo : PayerReceiverInfo
    {
        #region Members
        // EG 20150920 [21374] Int (int32) to Long (Int64) 
        public decimal quantity;
        #endregion Members
        #region Accessors
        // EG 20170127 Qty Long To Decimal
        public override decimal Quantity
        {
            get { return quantity; }
            set { quantity = value; }
        }
        #endregion Accessors

        #region Constructors
        public QuantityPayerReceiverInfo() { }
        // EG 20150920 [21374] Int (int32) to Long (Int64) 
        // EG 20170127 Qty Long To Decimal
        public QuantityPayerReceiverInfo(decimal pQuantity,
            int pIdA_Pay, string pActor_Pay, int pIdB_Pay, string pBook_Pay, int pIdA_Rec, string pActor_Rec, int pIdB_Rec, string pBook_Rec)
            : base(pIdA_Pay, pActor_Pay, pIdB_Pay, pBook_Pay, pIdA_Rec, pActor_Rec, pIdB_Rec, pBook_Rec)
        {
            quantity = pQuantity;
        }
        // EG 20150706 [20121] New
        // EG 20150920 [21374] Int (int32) to Long (Int64)
        // EG 20170127 Qty Long To Decimal
        public QuantityPayerReceiverInfo(decimal pQuantity, PayerReceiverInfoDet pPayer, PayerReceiverInfoDet pReceiver)
            : base(pPayer, pReceiver)
        {
            quantity = pQuantity;
        }
        #endregion Constructors
    }
    #endregion QuantityPayerReceiverInfo
    #region PayerReceiverInfoDet
    /// <summary>
    /// Info détail d'un payer|receiver (used by PayerReceiverInfo)
    /// </summary>
    /// 20150515 EG [20513] New
    // EG 20150706 [21021] Nullable<int> for actor|book
    public class PayerReceiverInfoDet
    {
        #region Members
        public PayerReceiverEnum payerReceiver;
        public Pair<Nullable<int>, string> actor;
        public Pair<Nullable<int>, string> book;
        #endregion Members
        #region Constructors
        public PayerReceiverInfoDet() { }
        // EG 20150706 [21021]
        public PayerReceiverInfoDet(PayerReceiverEnum pPayerReceiver, Nullable<int> pIdA, string pActor, Nullable<int> pIdB, string pBook)
        {
            payerReceiver = pPayerReceiver;
            actor = new Pair<Nullable<int>, string>(pIdA, pActor);
            book = new Pair<Nullable<int>, string>(pIdB, pBook);
        }
        // EG 20150706 [21021]
        public PayerReceiverInfoDet(PayerReceiverEnum pPayerReceiver, Pair<Pair<Nullable<int>, string>, Pair<Nullable<int>, string>> pActorBook)
            : this(pPayerReceiver, pActorBook.First, pActorBook.Second)
        {
        }
        // EG 20150706 [21021]
        public PayerReceiverInfoDet(PayerReceiverEnum pPayerReceiver, Pair<Nullable<int>, string> pActor, Pair<Nullable<int>, string> pBook)
        {
            payerReceiver = pPayerReceiver;
            actor = pActor;
            book = pBook;
        }
        #endregion Constructors
    }
    #endregion PayerReceiverInfoDet
    #region PayerReceiverInfo
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(QuantityPayerReceiverInfo))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(AmountPayerReceiverInfo))]
    /// <summary>
    /// Info d'un payer|receiver (used by QuantityPayerReceiverInfo|AmountPayerReceiverInfo)
    /// </summary>
    /// 20150515 EG [20513] New
    public abstract class PayerReceiverInfo
    {
        #region Members
        public PayerReceiverInfoDet payer;
        public PayerReceiverInfoDet receiver;
        #endregion Members
        #region Accessors
        // EG 20150920 [21374] Int (int32) to Long (Int64) 
        // EG 20170127 Qty Long To Decimal
        public virtual decimal Quantity { get { return 0; } set { ;} }
        public virtual decimal Amount { get { return 0; } set { ;} }
        public virtual string Currency { get { return string.Empty; } set { ;} }
        #endregion Accessors

        #region Constructors
        public PayerReceiverInfo() { }
        public PayerReceiverInfo(int pIdA_Pay, string pActor_Pay, int pIdB_Pay, string pBook_Pay, int pIdA_Rec, string pActor_Rec, int pIdB_Rec, string pBook_Rec)
        {
            payer = new PayerReceiverInfoDet(PayerReceiverEnum.Payer, pIdA_Pay, pActor_Pay, pIdB_Pay, pBook_Pay);
            receiver = new PayerReceiverInfoDet(PayerReceiverEnum.Receiver, pIdA_Rec, pActor_Rec, pIdB_Rec, pBook_Rec);
        }
        public PayerReceiverInfo(PayerReceiverInfoDet pPayer, PayerReceiverInfoDet pReceiver)
        {
            payer = pPayer;
            receiver = pReceiver;
        }
        #endregion Constructors
    }
    #endregion PayerReceiverInfo
}
