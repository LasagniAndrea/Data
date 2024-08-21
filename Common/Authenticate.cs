using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.Reflection;
using ActiveDs;
//
using EFS.ACommon;
using EFS.Actor;
using EFS.ApplicationBlocks.Data;
using EFS.Common;
using EFS.LoggerClient;
using EFS.LoggerClient.LoggerService;

namespace EFS.Authenticate
{
    #region IUserFactory
    /// <summary>
    /// La fabrique d’utilisateurs, l’interface la plus importante
    /// </summary>
    public interface IUserFactory
    {
        int CreateCollaborator(Collaborator pClb);
        Collaborator SearchCollaborator(string pIdentifier, string pPassword, string pEntity, bool pIgnoreCase);
        ICollection SearchAllCollaborator(string pEntity);
    }
    #endregion

    #region public class AuthenticateLDAP
    /// <summary>
    // Cet objet sera à ré-écrire en cas de changement du type d'annuaire
    // Remarquez que l'ensemble des traitements spécifiques sont confinés dans cette classe
    /// </summary>
    /// EG 20220623 [XXXXX] Implémentation Authentification via Shibboleth SP et IdP
    public class AuthenticateLDAP : IUserFactory
    {
        readonly string source;
        readonly LogAddDetail log; // FI 20240111 [WI793] usage de LogAddDetail
        readonly int LDAP_Trace;
        private readonly bool isShibbolethAuthenticateType;
        public AuthenticateLDAP(string pSource, int pLDAP_Trace, LogAddDetail pLog, bool pIsShibbolethAuthenticationType)
        {
            log = pLog;
            source = pSource;
            LDAP_Trace = pLDAP_Trace;
            isShibbolethAuthenticateType = pIsShibbolethAuthenticationType;
        }
        public int CreateCollaborator(Collaborator pClb)
        {
            // Création d'un utilisateur LDAP
            int lRet = 0;
            DirectoryEntry DirEntry = null;
            try
            {
                // Construction d'un Directory Searcher.
                DirEntry = new DirectoryEntry(source);
                System.DirectoryServices.PropertyCollection pcoll = DirEntry.Properties;
                // Mettre ici les valeurs des différents attributs
                pcoll["UserName"].Value = pClb.Identifier;
                pcoll["Tel"].Value = pClb.TelephoneNumber;

                DirEntry.CommitChanges();
                lRet = -1;
            }
            catch
            {
                // throw an exception
                lRet = 99;
                throw;
            }
            finally
            {
                if (null != DirEntry)
                    DirEntry.Close();
            }
            return lRet;
        }

        /// <summary>
        /// Fonction de recherche/chargement d'un utilisateur (acteur) 
        /// <para>
        /// Principe:
        /// - Step1 / Connexion à l'annuaire: connexion anomyne ou authentifiée, avec pour cette dernière un compte admin ou, depuis la v5.1, le Login de l'utilisateur 
        /// - Step2 / Recherche dans l'annuaire de l'utilisateur sur la base, par défaut, d'une équivalence entre le Login et un ANR (Ambiguous Name Resolution)
        ///           et si la recherche aboutit lecture de son UPN (User Principal Name)  
        /// - Step3 / Connexion à l'annuaire: connexion authentifiée avec l'UPN de l'utilisateur 
        /// - Step4 / Recherche dans l'annuaire de l'utilisateur et lecture de diverses données dont: cn, displayName, givenName, ...
        ///           et si la recherche aboutit lecture des données pour initialisation des membres de la classe Collaborator 
        /// </para>
        /// </summary>
        /// <param name="pIdentifier">string: Identifiant de l'acteur</param>
        /// <param name="pPassword">string: Mot de passe de l'acteur</param>
        /// <param name="pEntity">string: Entité (inutilisé)</param>
        /// <param name="pIgnoreCase">bool: Identifiant sensible/insensible à la case (inutilisé)</param>
        /// <returns>Collaborator: Caractéristiques de l'acteur authentifié</returns>
        // EG 20190114 Add detail to ProcessLog Refactoring
        // EG 20220220 [26251] Validity User (SUPPRESSION de UnknownIdentifier et IncorrectPassword, on GARDE UnknownIdentifierOrIncorrectPassword) 
        public Collaborator SearchCollaborator(string pIdentifier, string pPassword, string pEntity, bool pIgnoreCase)
        {
            Collaborator collaborator = new Collaborator
            {
                AuthenticationType = Collaborator.AuthenticationTypeEnum.LDAP,
                Validity = Collaborator.ValidityEnum.UnknownIdentifierOrIncorrectPassword
            };

            DirectoryEntry root = null;
            string traceMsg = string.Empty;

            collaborator.Identifier = pIdentifier;
            if (pIdentifier.IndexOf("@") > 0)
            {
                //NB: Il s'agit ici d'un identifiant du type: username@entityname. On ne conserve que username.
                string[] arrayArobase = pIdentifier.Split('@');
                int i = 0;
                collaborator.Identifier = arrayArobase[i];
                for (i = 1; i < arrayArobase.Length - 1; i++)
                {
                    collaborator.Identifier += "@" + arrayArobase[i];
                }
                collaborator.Entity.Identifier = arrayArobase[i];
            }

            //PL 20160122 New features: il peut exister plusieurs sources (domaines), seule contrainte, celles-ci doivent être de même type (ex. LDAP ou LDAPS ou ...)
            string[] aSource;
            string sourcePrefix = source.StartsWith("LDAP:") ? "LDAP:" : source.StartsWith("LDAPS:") ? "LDAPS:" : source.StartsWith("GC:") ? "GC:" : string.Empty;
            if (String.IsNullOrEmpty(sourcePrefix))
            {
                aSource = new string[] { source };
            }
            else
            {
                string[] stringSeparators = new string[] { sourcePrefix, ";" + sourcePrefix };
                aSource = source.Split(stringSeparators, StringSplitOptions.RemoveEmptyEntries);
            }

            //Test sur chaque source (domaine) dans l'ordre d'apparition.
            foreach (string src in aSource)
            {
                if (collaborator.Validity == Collaborator.ValidityEnum.Succes)
                {
                    //Collaborateur identifié avec succès --> On stoppe les recherches sur les éventuelles autres sources
                    break;
                }

                string sourceItem = sourcePrefix + src;

                string upn = null; //upn -> User Principal Name
                try
                {
                    //PL 20160121 New 
                    AddTrace(2, "LDAP_Trace: DirectoryEntry - Source is " + sourceItem, ref collaborator);

                    string LDAP_Username = SystemSettings.GetAppSettings("LDAP_Username", string.Empty);
                    string LDAP_Password = SystemSettings.GetAppSettings("LDAP_Password", string.Empty);
                    if (StrFunc.IsFilled(LDAP_Username) && (LDAP_Username.Contains("{Login}")))
                    {
                        if (LDAP_Username != "{Login}")
                        {
                            //Initialisation du statut de retour en cas d'échec de connexion à l'annuaire
                            collaborator.Validity = Collaborator.ValidityEnum.UnreachableDirectory;
                        }

                        LDAP_Username = LDAP_Username.Replace("{Login}", collaborator.Identifier);
                        if (StrFunc.IsFilled(LDAP_Username) && StrFunc.IsEmpty(LDAP_Password))
                            LDAP_Password = pPassword;
                    }
                    AddTrace(2, "LDAP_Trace: DirectoryEntry - Opening (" + (StrFunc.IsFilled(LDAP_Username) ? LDAP_Username + (StrFunc.IsFilled(LDAP_Password) ? "/*****" : "") : "Anonymous") + ")", ref collaborator);

                    //---------------------------------------------------------------------------
                    //Connexion à l'annuaire LDAP
                    //---------------------------------------------------------------------------
                    if (StrFunc.IsEmpty(LDAP_Username))
                    {
                        //Connexion anonyme 
                        root = new DirectoryEntry(sourceItem);

                        //PL 20160229 Newness: 
                        #region Si connexion anonyme en échec, tentative d'établissement d'une connexion authentifiée sur la base du Login/Password saisie
                        if (root != null)
                        {
                            try
                            {
                                //Lecture du Guid afin de vérifier le bon fonctionnement de la connexion à l'annuaire.
                                Guid guid = root.Guid;
                            }
                            catch
                            {
                                if (StrFunc.IsFilled(collaborator.Identifier) && StrFunc.IsFilled(pPassword))
                                {
                                    AddTrace(2, "LDAP_Trace: DirectoryEntry - Closing", ref collaborator);
                                    AddTrace(2, "LDAP_Trace: DirectoryEntry - Opening (" + collaborator.Identifier + "/*****)", ref collaborator);
                                    root = new DirectoryEntry(sourceItem, collaborator.Identifier, pPassword);
                                }
                            }
                        }
                        #endregion
                    }
                    else
                    {
                        //Connexion authentifiée 
                        root = new DirectoryEntry(sourceItem, LDAP_Username, LDAP_Password);
                    }

                    //***************************************************************************
                    #region PL Test LDAP
                    //using (System.Web.Hosting.HostingEnvironment.Impersonate())
                    //{
                    //    // This code runs as the application pool user
                    //    DirectoryEntry root2 = new DirectoryEntry(sourceItem);
                    //}
                    #endregion 
                    //***************************************************************************

                    AddTrace(2, "LDAP_Trace: DirectorySearcher - Initialize", ref collaborator);
                    //---------------------------------------------------------------------------
                    //Recherche dans l'annuaire LDAP de l'utilisateur (sur la base, par défaut, de son ANR)
                    //---------------------------------------------------------------------------
                    DirectorySearcher searcher = new DirectorySearcher(root);
                    //Filtre LDAP: Par défault anr -> Ambiguous Name Resolution
                    string filter = SystemSettings.GetAppSettings("LDAP_Filter", "(anr={Login})");
                    #region Compatiblité ascendante
                    //PL 20160302 Jusqu'à la v5.0, le mot clé {Login} n'était pas disponible. On utilisait le mot clé {0}.
                    if (filter.Contains("{0}"))
                        filter = string.Format(filter, "{Login}"); //Replace de {0} par {Login}
                    #endregion 
                    searcher.Filter = filter.Replace("{Login}", collaborator.Identifier);
                    //PL 20160122 Use ReferralChasingOption.All instead of default ReferralChasingOption.External
                    searcher.ReferralChasing = ReferralChasingOption.All;
                    // RD 20200312 [22240] Use pagination to increase the performances of Active Directory search
                    searcher.PageSize = 10;
                    AddTrace(2, "LDAP_Trace: Filter is " + searcher.Filter, ref collaborator);
                    #region Ajout de la propriété à lire...
                    searcher.PropertiesToLoad.Add("userPrincipalName");
                    #endregion Ajout de la propriété à lire...

                    AddTrace(2, "LDAP_Trace: Search in progress...", ref collaborator);
                    SearchResultCollection results = searcher.FindAll();
                    AddTrace(2, "LDAP_Trace: Item(s) found " + results.Count.ToString(), ref collaborator);

                    #region DEBUG
#if DEBUG
                    if ((results.Count == 0) && (collaborator.Identifier.IndexOf("-") == (collaborator.Identifier.Length - 2)))
                    {
                        //Login de type: XXXX-U --> Find sans le -U
                        searcher.Filter = filter.Replace("{Login}", collaborator.Identifier.Substring(0, collaborator.Identifier.Length - 2));
                        results = searcher.FindAll();
                    }
                    else if ((results.Count == 1) && (results[0].Properties["userPrincipalName"].Count == 1))
                    {
                        collaborator.UserType = Actor.RoleActor.SYSADMIN;
                        if (StrFunc.IsFilled(collaborator.Entity.Identifier) && (collaborator.Entity.Identifier.IndexOf(" as ") > 0))
                        {
                            //Login de type: "identifier@entity as ROLE". Parent_Identifier contient ici "entity as ROLE"
                            int posAs = collaborator.Entity.Identifier.IndexOf(" as ");
                            string role = collaborator.Entity.Identifier.Substring(posAs + 4);
                            if (Enum.IsDefined(typeof(Actor.RoleActor), role))
                                collaborator.UserType = (Actor.RoleActor)Enum.Parse(typeof(Actor.RoleActor), role, true);
                            else
                                collaborator.UserType = Actor.RoleActor.USER;

                            //A suffix
                            collaborator.Identifier += "-" + collaborator.UserType.ToString().Replace("SYS", string.Empty).Substring(0, 1);
                        }
                    }
#endif
                    #endregion DEBUG

                    if (results.Count == 0)
                    {
                        collaborator.Validity = Collaborator.ValidityEnum.UnknownIdentifierOrIncorrectPassword;
                    }
                    else if (results.Count > 1)
                    {
                        collaborator.Validity = Collaborator.ValidityEnum.MultipleIdentifier;
                        AddTrace(1, "LDAP_Trace: Several found", ref collaborator);
                    }
                    else if ((results.Count == 1) && (results[0].Properties["userPrincipalName"].Count == 1))
                    {
                        collaborator.Validity = Collaborator.ValidityEnum.UnknownIdentifierOrIncorrectPassword;

                        upn = results[0].Properties["userPrincipalName"][0].ToString();
                        AddTrace(2, "LDAP_Trace: userPrincipalName is " + upn, ref collaborator);

                        #region PWD checking
                        AddTrace(2, "LDAP_Trace: DirectoryEntry - Closing", ref collaborator);
                        root.Close();

                        //AL 20240503 [WI907] Check account and password expiration
                        DirectoryEntry de = results[0].GetDirectoryEntry();
                        IADsUser no = (IADsUser)de.NativeObject;
                        AddTrace(1, "LDAP_Trace: PasswordExpirationDate: " + no.PasswordExpirationDate, ref collaborator);
                        AddTrace(1, "LDAP_Trace: AccountDisabled: " + no.AccountDisabled, ref collaborator);
                        AddTrace(1, "LDAP_Trace: AccountExpirationDate: " + no.AccountExpirationDate, ref collaborator);
                        AddTrace(1, "LDAP_Trace: IsAccountLocked: " + no.IsAccountLocked, ref collaborator);
                        DateTime neverExpiresDate = new DateTime(1970, 1, 1); //Default value for account/pwd never expires

                        if (no.AccountDisabled || no.IsAccountLocked)
                            collaborator.Validity = Collaborator.ValidityEnum.UnauthorizedAcces;
                        else {
                            //AccountExpirationDate represents the start of the account expiration
                            if (no.AccountExpirationDate.CompareTo(neverExpiresDate) != 0 &&
                                no.AccountExpirationDate.Date.CompareTo(DateTime.Now.Date) < 0) 
                                    collaborator.Validity = Collaborator.ValidityEnum.ExpiredPwd;
                            //PasswordExpirationDate represents date and time of the password expiration
                            if (no.PasswordExpirationDate.CompareTo(neverExpiresDate) != 0 &&
                                no.PasswordExpirationDate.ToLocalTime().CompareTo(DateTime.Now) < 0) 
                                    collaborator.Validity = Collaborator.ValidityEnum.ExpiredPwd;                            
                        }

                        if (collaborator.Validity == Collaborator.ValidityEnum.UnknownIdentifierOrIncorrectPassword)
                        {                            
                            AddTrace(2, "LDAP_Trace: DirectoryEntry - Opening (Credentials)", ref collaborator);
                            //---------------------------------------------------------------------------
                            //Connexion à l'annuaire LDAP via UPN (et son PASSWORD)
                            //---------------------------------------------------------------------------
                            root = new DirectoryEntry(sourceItem, upn, pPassword);
                            AddTrace(2, "LDAP_Trace: DirectorySearcher - Initialize", ref collaborator);
                            //---------------------------------------------------------------------------
                            //Recherche dans l'annuaire LDAP de l'utilisateur (sur la base, par défaut, de son UPN)
                            //---------------------------------------------------------------------------
                            searcher = new DirectorySearcher(root);
                            filter = SystemSettings.GetAppSettings("LDAP_FilterUPN", "(&(objectClass=user)(userPrincipalName={UPN}))");
                            #region Compatiblité ascendante
                            //PL 20160302 Jusqu'à la v5.0, le mot clé {UPN} n'était pas disponible. On utilisait le mot clé {0}.
                            if (filter.Contains("{0}"))
                                filter = string.Format(filter, "{UPN}"); //Replace de {0} par {UPN}
                            #endregion
                            searcher.Filter = filter.Replace("{UPN}", upn);
                            //PL 20160229 Use ReferralChasingOption.All
                            searcher.ReferralChasing = ReferralChasingOption.All;
                            // RD 20200312 [22240] Use pagination to increase the performances of Active Directory search
                            searcher.PageSize = 10;
                            AddTrace(2, "LDAP_Trace: Filter is " + searcher.Filter, ref collaborator);

                            #region Ajout des propriétés à lire... 
                            searcher.PropertiesToLoad.Add("cn");
                            searcher.PropertiesToLoad.Add("displayName");
                            searcher.PropertiesToLoad.Add("givenname");

                            //searcher.PropertiesToLoad.Add("sn");
                            //searcher.PropertiesToLoad.Add("c");
                            //searcher.PropertiesToLoad.Add("title");
                            //searcher.PropertiesToLoad.Add("company");
                            //searcher.PropertiesToLoad.Add("department");
                            //searcher.PropertiesToLoad.Add("physicalDeliveryOfficeName");
                            //searcher.PropertiesToLoad.Add("telephoneNumber");
                            //searcher.PropertiesToLoad.Add("sAMAccountName");
                            //searcher.PropertiesToLoad.Add("phone");
                            //searcher.PropertiesToLoad.Add("mail");
                            //searcher.PropertiesToLoad.Add("*"); //STAR for all properties
                            string ldapAttribut_for_ActorIdentifier = SystemSettings.GetAppSettings("LDAP_Identifier");
                            if (StrFunc.IsFilled(ldapAttribut_for_ActorIdentifier))
                            {
                                AddTrace(2, "LDAP_Trace: Attribut for Spheres identifier is " + ldapAttribut_for_ActorIdentifier, ref collaborator);
                                searcher.PropertiesToLoad.Add(ldapAttribut_for_ActorIdentifier);
                            }
                            string ldapAttribut_for_ActorParentIdentifier = SystemSettings.GetAppSettings("LDAP_ParentIdentifier");
                            if (StrFunc.IsFilled(ldapAttribut_for_ActorParentIdentifier))
                            {
                                AddTrace(2, "LDAP_Trace: Attribut for Spheres parent identifier is " + ldapAttribut_for_ActorParentIdentifier, ref collaborator);
                                searcher.PropertiesToLoad.Add(ldapAttribut_for_ActorParentIdentifier);
                            }
                            #endregion Ajout des propriétés à lire...

                            AddTrace(2, "LDAP_Trace: Search in progress...", ref collaborator);
                            SearchResult result = searcher.FindOne();
                            #endregion

                            if (result == null)
                            {
                                AddTrace(1, "LDAP_Trace: Not found", ref collaborator);
                            }
                            else
                            {
                                AddTrace(2, "LDAP_Trace: Found", ref collaborator);
                                collaborator.Validity = Collaborator.ValidityEnum.Succes;

                                if (StrFunc.IsFilled(ldapAttribut_for_ActorIdentifier))
                                    collaborator.Identifier = result.Properties[ldapAttribut_for_ActorIdentifier][0].ToString();
                                if (StrFunc.IsFilled(ldapAttribut_for_ActorParentIdentifier))
                                    collaborator.Entity.Identifier = result.Properties[ldapAttribut_for_ActorParentIdentifier][0].ToString();

                                if (ObjFunc.IsNotNull(result.Properties["displayName"]))
                                    collaborator.DisplayName = result.Properties["displayName"][0].ToString();
                                else if (ObjFunc.IsNotNull(result.Properties["cn"]))
                                    collaborator.DisplayName = result.Properties["cn"][0].ToString();
                                else if (ObjFunc.IsNotNull(result.Properties["givenname"]))
                                    collaborator.DisplayName = result.Properties["givenname"][0].ToString();
                                else
                                    collaborator.DisplayName = pIdentifier;
                                //collaborator.TelephoneNumber = result.Properties["telephoneNumber"][0].ToString();
                                AddTrace(2, "LDAP_Trace: Identifier is " + collaborator.Identifier, ref collaborator);
                                AddTrace(2, "LDAP_Trace: DisplayName is " + collaborator.DisplayName, ref collaborator);
                                AddTrace(2, "LDAP_Trace: Entity is " + collaborator.Entity.Identifier, ref collaborator);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    if (LDAP_Trace > 0)
                    {
                        traceMsg = "LDAP_Trace: ERROR - " + ex.Message;
                        traceMsg += " (Src: " + ex.Source + ")";
                        //PL 20160121 New 
                        string error_traceMsg = string.Empty;
                        if (ex.InnerException != null)
                        {
                            error_traceMsg += Cst.CrLf;
                            error_traceMsg += " - InnerException: " + ex.InnerException.Message;
                            error_traceMsg += " (Src: " + ex.InnerException.Source + ")";
                        }
                        if (ex.StackTrace != null)
                        {
                            error_traceMsg += Cst.CrLf;
                            error_traceMsg += " - StackTrace: " + ex.StackTrace;
                        }
                        if (LDAP_Trace >= 3)
                            collaborator.LogMessage += Cst.CrLf + traceMsg;
                        if (LDAP_Trace > 4)
                            collaborator.LogMessage += error_traceMsg;
                        traceMsg += error_traceMsg;

                        if (null != log)
                            log.Invoke(LogLevelEnum.Error , traceMsg.Substring(0, Math.Min(traceMsg.Length, 3900)));
                    }
                }
                finally
                {
                    if (null != root)
                    {
                        AddTrace(2, "LDAP_Trace: DirectoryEntry - Closing", ref collaborator);
                        root.Close();
                    }
                }
            }
            return collaborator;
        }

        public System.Collections.ICollection SearchAllCollaborator(string pEntity)
        {
            //On retourne une collection de tous les utilisateurs de l'annuaire
            //TBD
            return null;
        }

        // EG 20190114 Add detail to ProcessLog Refactoring
        private void AddTrace(int pLevel, string pTraceMsg, ref Collaborator pCollaborator)
        {
            if (LDAP_Trace >= pLevel)
            {
                if (LDAP_Trace >= (pLevel + 2))
                {
                    bool addCrLf = pTraceMsg.EndsWith("Closing");
                    pCollaborator.LogMessage += Cst.CrLf + pTraceMsg + (addCrLf ? Cst.CrLf : string.Empty);
                }
                if (null != log)
                    log.Invoke(LogLevelEnum.Error, pTraceMsg);
            }
        }
    }
    #endregion

    #region public class AuthenticateRDBMS
    /// <summary>
    // Autre implémentation de la fabrique d’utilisateurs
    // La méthode de recherche et de création est différente de la précédente (LDAP) mais le client
    // ne doit en aucun cas être impacté par ce choix d’implémentation interne
    /// </summary>
    /// EG 20220623 [XXXXX] Implémentation Authentification via Shibboleth SP et IdP
    public class AuthenticateRDBMS : IUserFactory
    {
        private readonly string source;
        private readonly bool isLoadSafety;
        private readonly bool isShibbolethAuthenticateType;

        public AuthenticateRDBMS(string pSource, bool pIsLoadSafety, bool pIsShibbolethAuthenticationType)
        {
            source = pSource;
            isLoadSafety = pIsLoadSafety;
            isShibbolethAuthenticateType = pIsShibbolethAuthenticationType;
        }
        #region public CreateCollaborator
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pClb"></param>
        /// <returns></returns>
        public int CreateCollaborator(Collaborator pClb)
        {
            int lRet = 0;

            IDbConnection dbConnection = DataHelper.OpenConnection(source);
            IDbTransaction dbTransaction = DataHelper.BeginTran(dbConnection);
            
            // FI 20200820 [25468] Date systèmes en UTC Utilisation des paramètres DTINS et DTENABLED
            DataParameters dp = new DataParameters();
            dp.Add(DataParameter.GetParameter(source, DataParameter.ParameterEnum.IDENTIFIER), pClb.Identifier);
            dp.Add(DataParameter.GetParameter(source, DataParameter.ParameterEnum.DISPLAYNAME), pClb.DisplayName);
            dp.Add(DataParameter.GetParameter(source, DataParameter.ParameterEnum.DESCRIPTION), "automatic creation from the directory (LDAP identification)");
            dp.Add(DataParameter.GetParameter(source, DataParameter.ParameterEnum.IDAINS), 1);
            dp.Add(DataParameter.GetParameter(source, DataParameter.ParameterEnum.DTINS), OTCmlHelper.GetDateSysUTC(source));
            dp.Add(DataParameter.GetParameter(source, DataParameter.ParameterEnum.DTENABLED), OTCmlHelper.GetDateSys(source));

            string sqlInsert = SQLCst.INSERT_INTO_DBO + Cst.OTCml_TBL.ACTOR.ToString();
            sqlInsert += "(IDENTIFIER,DISPLAYNAME,DESCRIPTION,IDAINS,DTINS,DTENABLED)";
            sqlInsert += Cst.CrLf + SQLCst.VALUES;
            sqlInsert += "(@IDENTIFIER,@DISPLAYNAME,@DESCRIPTION,@IDAINS,@DTINS,@DTENABLED)";            try
            {
                lRet = DataHelper.ExecuteNonQuery(dbTransaction, CommandType.Text, sqlInsert, dp.GetArrayDbParameter());
                if (lRet == 1)
                {
                    dp = new DataParameters();
                    dp.Add(DataParameter.GetParameter(source, DataParameter.ParameterEnum.IDROLEACTOR), pClb.UserType);
                    dp.Add(DataParameter.GetParameter(source, DataParameter.ParameterEnum.IDENTIFIER), pClb.Identifier);
                    dp.Add(new DataParameter(source, "IDENTIFIER_ENT", DbType.AnsiString, SQLCst.UT_IDENTIFIER_LEN), pClb.Entity.Identifier);

                    sqlInsert = SQLCst.INSERT_INTO_DBO + Cst.OTCml_TBL.ACTORROLE.ToString();
                    sqlInsert += "(IDROLEACTOR,IDA,IDA_ACTOR,DTENABLED,DTINS,IDAINS)" + Cst.CrLf;
                    sqlInsert += SQLCst.SELECT + "@IDROLEACTOR,a.IDA,a_ent.IDA,a.DTENABLED,a.DTINS,a.IDAINS" + Cst.CrLf;
                    sqlInsert += SQLCst.FROM_DBO + Cst.OTCml_TBL.ACTOR.ToString() + " a," + SQLCst.DBO + Cst.OTCml_TBL.ACTOR.ToString() + " a_ent" + Cst.CrLf;
                    sqlInsert += SQLCst.WHERE + "a.IDENTIFIER=@IDENTIFIER" + Cst.CrLf;
                    sqlInsert += SQLCst.AND + "a_ent.IDENTIFIER=@IDENTIFIER_ENT" + Cst.CrLf;

                    lRet = DataHelper.ExecuteNonQuery(dbTransaction, CommandType.Text, sqlInsert, dp.GetArrayDbParameter());

                    if (lRet == 1)
                        DataHelper.CommitTran(dbTransaction, false);
                    else
                        DataHelper.RollbackTran(dbTransaction);
                }
            }
            catch
            {
                lRet = -1;
                if (null != dbTransaction)
                    DataHelper.RollbackTran(dbTransaction);
                throw;
            }
            finally
            {
                if (null != dbConnection)
                    DataHelper.CloseConnection(dbConnection);
            }
            return lRet;
        }
        #endregion public CreateCollaborator

        #region public SearchCollaborator
        /// <summary>
        /// Fonction de recherche/chargement d'un utilisateur (acteur) 
        /// </summary>
        /// <param name="pIdentifier">string: Identifiant de l'acteur</param>
        /// <param name="pPassword">string: Mot de passe de l'acteur (inutilisé)</param>
        /// <param name="pEntity">string: Entité (inutilisé)</param>
        /// <param name="pIgnoreCase">bool: Identifiant sensible/insensible à la case</param>
        /// <returns>Collaborator: Caractéristiques de l'acteur authentifié</returns>
        // FI 20140613 [19923] add TRACKMODE
        // EG 20151215 [21305] Add PWDFORBIDDENLIST
        // PL 20171020 [23490] add Timezone
        // EG 20180425 Analyse du code Correction [CA2202]
        // EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        // EG 20220623 [XXXXX] Implémentation pour Shibboleth
        // EG 20220220 [26251] Validity User (SUPPRESSION de UnknownIdentifier et IncorrectPassword, on GARDE UnknownIdentifierOrIncorrectPassword) 
        public Collaborator SearchCollaborator(string pIdentifier, string pPassword, string pEntity, bool pIgnoreCase)
        {
            try
            {
                Collaborator collaborator = new Collaborator
                {
                    AuthenticationType = Collaborator.AuthenticationTypeEnum.RDBMS
                };
                //20160315 Tip: see also call method below
                //Recherche de l'utilisateur Spheres relatif un un compte LDAP ou DomainNT identifié avec succès.
                if (pIdentifier.StartsWith("LDAP:\\->"))
                {
                    collaborator.AuthenticationType = Collaborator.AuthenticationTypeEnum.LDAP;
                    pIdentifier = pIdentifier.Substring(8);
                }
                else if (pIdentifier.StartsWith("DomainNT:\\->"))
                {
                    collaborator.AuthenticationType = Collaborator.AuthenticationTypeEnum.DOMAINNT;
                    pIdentifier = pIdentifier.Substring(12);
                }
                collaborator.Validity = Collaborator.ValidityEnum.UnknownIdentifierOrIncorrectPassword;

                bool isExistModelSafety = false;
                int nbDayValid = 0;
                int allowedGrace = 0;
                int idA = 0;

                #region sqlSelect
                //20081010 PL Add Parameters and left outer on MODELSAFETY
                // EG 20151215 [21305] Add ms.PWDFORBIDDENLIST
                string sqlSelect = @"select a.IDA, a.IDENTIFIER, a.DISPLAYNAME, a.PWD, a.DTENABLED, a.DTDISABLED, a.DTPWDEXPIRATION, a.ISRDBMSUSER, 
                a.ISRDBMSUSER, a.CSSFILENAME, a.EXTLLINK, a.ISPWDMODNEXTLOGON, a.PWDREMAININGGRACE, a.CULTURE, a.TIMEZONE, a.IDCOUNTRYRESIDENCE,
                isnull(a.SIMULTANEOUSLOGIN, ms.SIMULTANEOUSLOGIN) as SIMULTANEOUSLOGIN, a.ROWATTRIBUT, 
                ms.IDMODELSAFETY, ms.NBDAYWARNING, ms.NBDAYVALID, ms.ALLOWEDGRACE, ms.PWDMODPERMITTED, ms.PWDMODPERIODIC, ms.TRACKMODE, ms.PWDFORBIDDENLIST
                from dbo.ACTOR a
                left outer join dbo.MODELSAFETY ms on (ms.IDMODELSAFETY = a.IDMODELSAFETY)
                where" + Cst.CrLf;

                DataParameters dp = new DataParameters();

                /// EG 20220623 [XXXXX] Implémentation Authentification via Shibboleth SP et IdP
                if (isShibbolethAuthenticateType)
                {
                    // Lecture dans le fichier de configuration du type de donnée retourné par Le Shibboleth SP utilisé pour faire une "auto-connexion" sur Spheres.
                    Collaborator.SAMLMappingClaimEnum claim = 
                        ReflectionTools.ConvertStringToEnumOrDefault<Collaborator.SAMLMappingClaimEnum>((string)SystemSettings.GetAppSettings("SAML_MappingClaim"), 
                        Collaborator.SAMLMappingClaimEnum.identifier);

                    string column = "IDENTIFIER";
                    DataParameter param = new DataParameter(source, column, DbType.AnsiString, SQLCst.UT_IDENTIFIER_LEN);

                    switch (claim)
                    {
                        case Collaborator.SAMLMappingClaimEnum.displayName:
                            column = "DISPLAYNAME";
                            param = new DataParameter(source, column, DbType.AnsiString, SQLCst.UT_DISPLAYNAME_LEN);
                            break;
                        case Collaborator.SAMLMappingClaimEnum.mail:
                            column = "MAIL";
                            param = new DataParameter(source, column, DbType.AnsiString, 256);
                            break;
                        case Collaborator.SAMLMappingClaimEnum.telephoneNumber:
                            column = "TELEPHONENUMBER";
                            param = new DataParameter(source, column, DbType.AnsiString, 32);
                            break;
                    }

                    if (pIgnoreCase)
                        sqlSelect += DataHelper.SQLUpper(source, $"a.{column}") + " = " + DataHelper.SQLUpper(source, $"@{column}");
                    else
                        sqlSelect += $"a.{column} = @{column}";

                    param.Value = pIdentifier;
                    dp.Add(param);
                }
                else
                {
                    if ((pIdentifier.IndexOf("@") >= 0) && (pIdentifier.IndexOf(".") >= 0))
                    {
                        //Identification par EMAIL
                        if (pIgnoreCase)
                            sqlSelect += DataHelper.SQLUpper(source, "a.MAIL") + "=" + DataHelper.SQLUpper(source, "@MAIL");
                        else
                            sqlSelect += "a.MAIL=@MAIL";
                        dp.Add(new DataParameter(source, "MAIL", DbType.AnsiString, 256), pIdentifier);
                    }
                    else
                    {
                        //Identification par IDENTIFIER
                        if (pIgnoreCase)
                            sqlSelect += DataHelper.SQLUpper(source, "a.IDENTIFIER") + "=" + DataHelper.SQLUpper(source, "@IDENTIFIER");
                        else
                            sqlSelect += "a.IDENTIFIER=@IDENTIFIER";
                        dp.Add(new DataParameter(source, "IDENTIFIER", DbType.AnsiString, SQLCst.UT_IDENTIFIER_LEN), pIdentifier);
                    }
                }
                #endregion SQLSelect

                // EG 20151215 [21305]
                QueryParameters qryParameters = new QueryParameters(source, sqlSelect, dp);

                using (IDataReader dr = DataHelper.ExecuteDataTable(source, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter()).CreateDataReader())
                {
                    if (dr.Read())
                    {
                        idA = Convert.ToInt32(dr["IDA"]);

                        #region Lecture des caractéristiques de l'acteur (utilisateur)
                        //collaborator.Identifier = pIdentifier;
                        collaborator.Identifier = dr["IDENTIFIER"].ToString(); ;
                        collaborator.DisplayName = dr["DISPLAYNAME"].ToString();
                        collaborator.CssColor = dr["CSSFILENAME"].ToString();
                        collaborator.Culture = dr["CULTURE"].ToString();
                        collaborator.Timezone = dr["TIMEZONE"].ToString();
                        collaborator.Country = dr["IDCOUNTRYRESIDENCE"].ToString();
                        collaborator.ExtlLink = dr["EXTLLINK"].ToString();
                        collaborator.Pwd = dr["PWD"].ToString();
                        collaborator.IsRdbmsUser = Convert.ToBoolean(dr["ISRDBMSUSER"]);
                        collaborator.IsDeleted = (!(dr["ROWATTRIBUT"] is DBNull) && (dr["ROWATTRIBUT"].ToString() == Cst.RowAttribut_Deleted));
                        collaborator.IsPwdModNextLogOn = Convert.ToBoolean(dr["ISPWDMODNEXTLOGON"]);
                        collaborator.DtEnabled = Convert.ToDateTime(dr["DTENABLED"]);

                        if (!(dr["DTDISABLED"] is DBNull))
                            collaborator.DtDisabled = Convert.ToDateTime(dr["DTDISABLED"]);

                        // EG 20151215 [21305] DtExpiration nullable
                        if (false == (dr["DTPWDEXPIRATION"] is DBNull))
                            collaborator.DtExpiration = Convert.ToDateTime(dr["DTPWDEXPIRATION"]);

                        collaborator.PwdRemainingGrace = -1;
                        if (!(dr["PWDREMAININGGRACE"] is DBNull))
                            collaborator.PwdRemainingGrace = Convert.ToInt32(dr["PWDREMAININGGRACE"].ToString());

                        collaborator.SimultaneousLogin = -1;
                        if (!(dr["SIMULTANEOUSLOGIN"] is DBNull))
                            collaborator.SimultaneousLogin = Convert.ToInt32(dr["SIMULTANEOUSLOGIN"].ToString());
                        #endregion

                        #region Lecture des caractéristiques de sécurité appliquées "directement" à l'acteur (Table MODELSAFETY)
                        if (isLoadSafety)
                        {
                            isExistModelSafety = !(dr["IDMODELSAFETY"] is DBNull);
                            if (isExistModelSafety)
                            {
                                // RD 20110223 [16324]
                                collaborator.IdModelSafety = Convert.ToInt32(dr["IDMODELSAFETY"].ToString());

                                if (!(dr["NBDAYWARNING"] is DBNull))
                                    collaborator.NbDayWarning = Convert.ToInt32(dr["NBDAYWARNING"].ToString());
                                if (!(dr["NBDAYVALID"] is DBNull))
                                    nbDayValid = Convert.ToInt32(dr["NBDAYVALID"].ToString());
                                if (!(dr["ALLOWEDGRACE"] is DBNull))
                                    allowedGrace = Convert.ToInt32(dr["ALLOWEDGRACE"].ToString());

                                collaborator.IsPwdModPermitted = BoolFunc.IsTrue((dr["PWDMODPERMITTED"]));
                                collaborator.IsPwdModPeriodic = BoolFunc.IsTrue((dr["PWDMODPERIODIC"]));
                                collaborator.TrackMode = (Cst.RequestTrackMode)Enum.Parse(typeof(Cst.RequestTrackMode), dr["TRACKMODE"].ToString());
                                // EG 20151215 [21305] New
                                if (false == (dr["PWDFORBIDDENLIST"] is DBNull))
                                {
                                    collaborator.PwdForbiddenList = new List<string>(Convert.ToString(dr["PWDFORBIDDENLIST"]).Split(new char[] { ';' },
                                        StringSplitOptions.RemoveEmptyEntries));
                                }
                            }
                        }
                        #endregion

                        if (dr.Read())
                        {
                            //ERROR: Il existe plus d'un record 
                            idA = 0;
                            collaborator.Validity = Collaborator.ValidityEnum.MultipleIdentifier;
                        }
                    }
                }

                if (idA != 0)
                {
                    collaborator.Ida = idA;
                    
                    collaborator.SetUserType(source);
                    					
                    collaborator.Initialize();
                    
                    collaborator.SetAncestor(source);
                    
                    collaborator.SetEntity(source);

                    collaborator.SetDepartment(source);

                    #region Vérification des caractéristiques de sécurité appliquées à l'acteur (Table MODELSAFETY)
                    if (isLoadSafety)
                    {
                        #region Lecture éventuelle des caractéristiques de sécurité appliquées "à un parent" de l'acteur (Table MODELSAFETY)
                        if (!isExistModelSafety)
                        {
                            for (int level = 1; level <= collaborator.ActorAncestor.GetLevelLength(); level++)
                            {
                                #region Search MODELSAFETY of Ancestors : Level by Level
                                string listOfAncestors = collaborator.ActorAncestor.GetListIdA_ActorByLevel(level);
                                if (StrFunc.IsFilled(listOfAncestors))
                                {
                                    // EG 20151215 [21305] Add ms.TRACKMODE, ms.PWDFORBIDDENLIST
                                    sqlSelect = @"select isnull(a.SIMULTANEOUSLOGIN, ms.SIMULTANEOUSLOGIN) as SIMULTANEOUSLOGIN, 
                                    ms.SIMULTANEOUSLOGIN,ms.IDMODELSAFETY, ms.NBDAYWARNING, ms.NBDAYVALID, ms.ALLOWEDGRACE, ms.PWDMODPERMITTED, 
                                    ms.TRACKMODE, ms.PWDMODPERIODIC, ms.PWDFORBIDDENLIST
                                    from dbo.ACTOR a
                                    left outer join dbo.MODELSAFETY ms on (ms.IDMODELSAFETY=a.IDMODELSAFETY)
                                    where (a.IDA = @IDA)";
                                    
                                    dp = new DataParameters();
                                    dp.Add(DataParameter.GetParameter(source, DataParameter.ParameterEnum.IDA));
                                    
                                    string[] arrayOfAncestors = listOfAncestors.Split(',');
                                    
                                    for (int i = 0; i < arrayOfAncestors.Length; i++)
                                    {
                                        #region Search MODELSAFETY of Ancestors : for each Level
                                        int idaAncestor = IntFunc.IntValue(arrayOfAncestors[i]);
                                        if (idaAncestor > 0 && idaAncestor != collaborator.Ida)
                                        {
                                            dp["IDA"].Value = idaAncestor;
                                            // EG 20151215 [21305]
                                            qryParameters = new QueryParameters(source, sqlSelect, dp);

                                            using (IDataReader dr = DataHelper.ExecuteDataTable(source, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter()).CreateDataReader())
                                            {
                                                if (dr.Read())
                                                {
                                                    if (!(dr["IDMODELSAFETY"] is DBNull))
                                                    {
                                                        isExistModelSafety = true;

                                                        // RD 20110223 [16324]
                                                        collaborator.IdModelSafety = Convert.ToInt32(dr["IDMODELSAFETY"].ToString());

                                                        if (dr["NBDAYWARNING"] is DBNull)
                                                            collaborator.NbDayWarning = 0;
                                                        else
                                                            collaborator.NbDayWarning = Convert.ToInt32(dr["NBDAYWARNING"].ToString());

                                                        if (!(dr["NBDAYVALID"] is DBNull))
                                                            nbDayValid = Convert.ToInt32(dr["NBDAYVALID"].ToString());
                                                        if (!(dr["ALLOWEDGRACE"] is DBNull))
                                                            allowedGrace = Convert.ToInt32(dr["ALLOWEDGRACE"].ToString());

                                                        collaborator.IsPwdModPermitted = Convert.ToBoolean(dr["PWDMODPERMITTED"]);
                                                        collaborator.IsPwdModPeriodic = Convert.ToBoolean(dr["PWDMODPERIODIC"]);

                                                        if (collaborator.SimultaneousLogin < 0)
                                                        {
                                                            if (!(dr["SIMULTANEOUSLOGIN"] is DBNull))
                                                                collaborator.SimultaneousLogin = Convert.ToInt32(dr["SIMULTANEOUSLOGIN"].ToString());
                                                        }

                                                        // EG 20151215 [21305] New TrackMode and PwdForbiddenList
                                                        collaborator.TrackMode = (Cst.RequestTrackMode)Enum.Parse(typeof(Cst.RequestTrackMode), dr["TRACKMODE"].ToString());
                                                        if (false == (dr["PWDFORBIDDENLIST"] is DBNull))
                                                        {
                                                            collaborator.PwdForbiddenList = new List<string>(Convert.ToString(dr["PWDFORBIDDENLIST"]).Split(new char[] { ';' },
                                                                StringSplitOptions.RemoveEmptyEntries));
                                                        }
                                                        // Break dès qu'on trouve un MODELSAFETY sur au moins un Parent du même niveau
                                                        break;
                                                    }
                                                }
                                            }
                                        }
                                        #endregion
                                    }
                                    
                                    // Break dès qu'on trouve un MODELSAFETY
                                    if (isExistModelSafety)
                                        break;
                                }
                                #endregion
                            }
                        }
                        #endregion

                        if (isExistModelSafety)
                        {
                            if (collaborator.Validity == Collaborator.ValidityEnum.Succes)
                            {
                                #region Maj DTPWDEXPIRATION & PWDREMAININGGRACE
                                // EG 20151215 [21305] DtExpiration.HasValue
                                if (collaborator.IsPwdModPeriodic && (false == collaborator.DtExpiration.HasValue))
                                {
                                    dp = new DataParameters();
                                    dp.Add(new DataParameter(source, "DTPWDEXPIRATION", DbType.Date), OTCmlHelper.GetDateSys(source).Date.AddDays(nbDayValid)); // FI 20201006 [XXXXX] DbType.Date
                                    dp.Add(new DataParameter(source, "PWDREMAININGGRACE", DbType.Int32), allowedGrace);
                                    dp.Add(DataParameter.GetParameter(source, DataParameter.ParameterEnum.IDA), idA);

                                    string sqlQuery = SQLCst.UPDATE_DBO + Cst.OTCml_TBL.ACTOR.ToString() + Cst.CrLf;
                                    sqlQuery += SQLCst.SET + @"DTPWDEXPIRATION=@DTPWDEXPIRATION, PWDREMAININGGRACE=@PWDREMAININGGRACE" + Cst.CrLf;
                                    sqlQuery += SQLCst.WHERE + "IDA=@IDA";

                                    DataHelper.ExecuteNonQuery(source, CommandType.Text, sqlQuery, dp.GetArrayDbParameter());
                                }
                                #endregion
                            }
                        }
                        else
                        {
                            collaborator.Validity = Collaborator.ValidityEnum.MissingModelSafety;
                        }
                    }
                    #endregion
                }
                return collaborator;
            }
            catch 
            {
                throw;
            }
        }
        public ICollection SearchAllCollaborator(string pEntity)
        {
            //TBD
            return null;
        }
        #endregion SearchCollaborator
    }
    #endregion
    #region public class AuthenticateEUROSYS
    // EG 20220623 [XXXXX] Implémentation Authentification via Shibboleth SP et IdP
    public class AuthenticateEUROSYS : IUserFactory
    {
        private readonly string source;
        private readonly bool isShibbolethAuthenticateType;

        public AuthenticateEUROSYS(string pSource, bool pIsShibbolethAuthenticationType)
        {
            source = pSource;
            isShibbolethAuthenticateType = pIsShibbolethAuthenticationType;
        }
        #region public CreateCollaborator
        public int CreateCollaborator(Collaborator pClb)
        {
            int lRet = 0;
            //TODO
            return lRet;
        }
        #endregion public CreateCollaborator
        #region public SearchCollaborator
        /// <summary>
        /// Fonction de recherche/chargement d'un utilisateur (acteur) 
        /// </summary>
        /// <param name="pIdentifier">string: Identifier de l'acteur</param>
        /// <param name="pEntity">string: Entité (inutilisé)</param>
        /// <returns>Collaborator: Caractéristiques de l'acteur authentifié</returns>
        public Collaborator SearchCollaborator(string pIdentifier, string pPassword, string pEntity, bool pIgnoreCase)
        {
            //Nombre d'information n'existe pas dans Eurosys. Elles sont donc valorisées ici en dur (eg. DTENABLED, ROLE, ...)
            Collaborator collaborator = new Collaborator
            {
                AuthenticationType = Collaborator.AuthenticationTypeEnum.EUROSYS
            };

            string nomAppli = Software.Name.ToUpper();
            string SQLSelect = SQLCst.SELECT;
            SQLSelect += "u.ID_USER, u.DESCRIPTION, u.ID_CHANGEMENT_PWD, ua.LANGUE_APPLICATION" + Cst.CrLf;
            SQLSelect += SQLCst.FROM_DBO + "USERS u, USER_APPLI ua" + Cst.CrLf;
            SQLSelect += SQLCst.WHERE;
            SQLSelect += "u.NOM_USER=" + DataHelper.SQLString(pIdentifier);
            SQLSelect += SQLCst.AND + "ua.ID_USER=u.ID_USER";
            SQLSelect += SQLCst.AND + "ua.NOM_APPLICATION=" + DataHelper.SQLString(nomAppli);
            
            IDataReader dr1 = null;
            IDataReader dr2 = null;
            try
            {
                //20070717 FI utilisation de ExecuteDataTable
                dr1 = DataHelper.ExecuteDataTable(source, SQLSelect).CreateDataReader();
                if (dr1.Read())
                {
                    //collaborator.Source	     = source;
                    collaborator.Identifier = pIdentifier;
                    collaborator.DisplayName = dr1["DESCRIPTION"].ToString();
                    string defaultCulture = collaborator.Culture;
                    switch (dr1["LANGUE_APPLICATION"].ToString())
                    {
                        case "F":
                            collaborator.Culture = "fr-FR";
                            break;
                        case "I":
                            collaborator.Culture = "it-IT";
                            break;
                        case "E":
                            collaborator.Culture = "en-GB";
                            break;
                    }
                    collaborator.IsRdbmsUser = true;
                    collaborator.IsDeleted = false;
                    collaborator.IsPwdModNextLogOn = BoolFunc.IsTrue(dr1["ID_CHANGEMENT_PWD"].ToString());
                    collaborator.DtEnabled = DateTime.MinValue;
                    // EG 20151215 [21305] New
                    collaborator.DtExpiration = null;

                    #region Recherche du rôle du User
                    //On considère ici en dur chaque utilisateur comme ayant le rôle USER
                    collaborator.UserType = RoleActor.USER;
                    #endregion Recherche du role du User

                    collaborator.PwdRemainingGrace = -1;
                    collaborator.SimultaneousLogin = -1;
                    collaborator.IsPwdModPermitted = false;
                    collaborator.IsPwdModPeriodic = false;
                    collaborator.TrackMode = Cst.RequestTrackMode.NONE;

                    collaborator.Ida = Convert.ToInt32(dr1["ID_USER"]);
                    collaborator.Initialize();

                    if (dr1.Read())
                        //Il existe plus d'un record --> error
                        collaborator.Validity = Collaborator.ValidityEnum.MultipleIdentifier;
                    else
                    {
                        #region Scan ENTITY
                        collaborator.Entity = new CollaboratorParent();
                        //	
                        SQLSelect = SQLCst.SELECT;
                        SQLSelect += "a.OBSERVATION1, a.LANG_APPLI " + Cst.CrLf;
                        SQLSelect += SQLCst.FROM_DBO + "APPLI a" + Cst.CrLf;
                        //20070717 FI utilisation de ExecuteDataTable 
                        dr2 = DataHelper.ExecuteDataTable(source, SQLSelect).CreateDataReader();
                        if (dr2.Read())
                        {
                            collaborator.Entity.Identifier = dr2["OBSERVATION1"].ToString();
                            collaborator.Entity.DisplayName = dr2["OBSERVATION1"].ToString();
                            switch (dr2["LANG_APPLI"].ToString())
                            {
                                case "F":
                                    collaborator.Entity.Culture = "fr-FR";
                                    break;
                                case "I":
                                    collaborator.Entity.Culture = "it-IT";
                                    break;
                                case "E":
                                    collaborator.Entity.Culture = "en-GB";
                                    break;
                            }

                            if (StrFunc.IsEmpty(collaborator.Culture))
                                collaborator.Culture = collaborator.Entity.Culture;
                        }
                        #endregion
                        //
                        if (StrFunc.IsEmpty(collaborator.Culture))
                            collaborator.Culture = defaultCulture;
                        if (StrFunc.IsEmpty(collaborator.Culture))
                            collaborator.Culture = Cst.EnglishCulture;
                    }
                }
                else
                {
                    //Il existe aucun record
                    collaborator.Validity = Collaborator.ValidityEnum.UnknownIdentifierOrIncorrectPassword;
                }
            }
            catch
            {
                throw;
            }
            finally
            {
                if (null != dr1)
                    dr1.Close();
                if (null != dr2)
                    dr2.Close();
            }
            return collaborator;
        }
        public System.Collections.ICollection SearchAllCollaborator(string pEntity)
        {
            //TBD
            return null;
        }
        #endregion SearchCollaborator
    }
    #endregion
    #region public class AuthenticateDOMAINNT
    // EG 20220623 [XXXXX] Implémentation Authentification via Shibboleth SP et IdP
    public class AuthenticateDOMAINNT : IUserFactory
    {
        private readonly string source;
        private readonly bool isShibbolethAuthenticateType;

        public AuthenticateDOMAINNT(string pSource, bool pIsShibbolethAuthenticationType)
        {
            source = "WinNT://" + pSource;
            isShibbolethAuthenticateType = pIsShibbolethAuthenticationType;
        }
        public int CreateCollaborator(Collaborator pClb)
        {
            int lRet = 0;
            DirectoryEntry obDirEntry = null;
            DirectoryEntry obUser = null;
            try
            {
                obDirEntry = new DirectoryEntry(source);
                DirectoryEntries entries = obDirEntry.Children;
                obUser = entries.Add(pClb.Identifier, "User");
                obUser.Properties["FullName"].Add(pClb.DisplayName);
                obUser.Invoke("SetPassword", pClb.Pwd);
                obUser.CommitChanges();
                lRet = -1;
            }
            catch
            {
                // throw an exception
                lRet = 99;
                throw;
            }
            finally
            {
                if (null != obDirEntry)
                    obDirEntry.Close();
                if (null != obUser)
                    obUser.Close();
            }
            return lRet;
        }
        public Collaborator SearchCollaborator(string pIdentifier, string pPassword, string pEntity, bool pIgnoreCase)
        {
            Collaborator collaborator = new Collaborator
            {
                AuthenticationType = Collaborator.AuthenticationTypeEnum.DOMAINNT
            };
            //A faire...
            return collaborator;
        }

        public ICollection SearchAllCollaborator(string pEntity)
        {
            DirectoryEntry obDirEntry = null;
            try
            {
                obDirEntry = new DirectoryEntry(source);
                foreach (DirectoryEntry domain in obDirEntry.Children)
                {
                    Console.WriteLine(domain.Name);
                }
            }
            catch
            {
                throw;
            }
            finally
            {
                if (null != obDirEntry)
                    obDirEntry.Close();
            }
            return null;
        }
    }
    #endregion

    #region public class Authenticate
    /// EG 20220623 [XXXXX] Implémentation Authentification via Shibboleth SP et IdP
    public class Authenticate
    {
        #region Members
        readonly IUserFactory userfactory;
        string _logMessage = string.Empty;
        public string LogMessage
        {
            get { return _logMessage; }
            set { _logMessage = value; }
        }
        string _authenticationType = string.Empty;
        public string AuthenticationType
        {
            get { return _authenticationType; }
            set { _authenticationType = value; }
        }

        #endregion Members
        #region Constructors
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pType">Lorsque pType est empty => AuthenticateRDBMS</param>
        /// <param name="pCS"></param>
        /// <param name="pIsLoadSafety"
        /// <param name="pLog"></param>
        /// <param name="pIsShibbolethAuthenticationType"></param>
        /// EG 20160404 Migration vs2013
        /// EG 20190114 Add detail to ProcessLog Refactoring
        /// FI 20240111 [WI793] usage de LogAddDetail
        public Authenticate(string pType, string pCS, bool pIsLoadSafety, LogAddDetail pLog, bool pIsShibbolethAuthenticationType)
        {
            switch (pType)
            {
                case "LDAP":
                    int LDAP_Trace = (int)SystemSettings.GetAppSettings("LDAP_Trace", typeof(Int32), 0);
                    string traceMsg;
                    try
                    {
                        //***************************************************************************
                        #region PL Test LDAP
                        //System.Security.Principal.IIdentity id = System.Security.Principal.WindowsIdentity.GetCurrent();
                        //System.Security.Principal.WindowsIdentity winId = id as System.Security.Principal.WindowsIdentity;
                        //if (id == null)
                        //    return;
                        //string userInQuestion = winId.Name.Split('\\')[1];
                        //string myDomain = winId.Name.Split('\\')[0]; // this is the domain that the user is in the account that this program runs in should be authenticated in there 
                        #endregion
                        //***************************************************************************

                        if (LDAP_Trace >= 2)
                        {
                            traceMsg = "LDAP_Trace: Trace is ON";
                            if (LDAP_Trace >= 4)
                            {
                                _logMessage = traceMsg;
                            }

                            if (null != pLog)
                                pLog.Invoke(LogLevelEnum.None, traceMsg);
                        }

                        //Step 1: Initialisation du Domaine LDAP à utiliser (myRootDN)
                        //Step 2: Authentification de l'utilisateur (AuthenticateLDAP)
                        string myRootDN = SystemSettings.GetAppSettings("LDAP_Domain", string.Empty);

                        #region CurrentForest / CurrentDomain / ComputerDomain
                        //PL 20160122 New features: Identification et replace des éventuels Keywords ( ex. {CurrentForest} ).
                        string key_CurrentForest = "{CurrentForest}";
                        string key_CurrentDomain = "{CurrentDomain}";
                        string key_ComputerDomain = "{ComputerDomain}";
                        string forestName = string.Empty, domainName = string.Empty;
                        if (myRootDN.Contains(key_CurrentDomain) || myRootDN.Contains(key_CurrentForest))
                        {
                            // EG 20160404 Migration vs2013
                            System.DirectoryServices.ActiveDirectory.Domain domain;
                            try
                            {
                                domain = System.DirectoryServices.ActiveDirectory.Domain.GetCurrentDomain();
                                domainName = domain.Name;
                                forestName = domain.Forest.Name;
                            }
                            catch (Exception) { }
                        }
                        if (myRootDN.Contains(key_ComputerDomain) || myRootDN.Contains(key_CurrentForest))
                        {
                            // EG 20160404 Migration vs2013
                            System.DirectoryServices.ActiveDirectory.Domain domain;
                            try
                            {
                                domain = System.DirectoryServices.ActiveDirectory.Domain.GetComputerDomain();
                                if (String.IsNullOrEmpty(domainName))
                                    domainName = domain.Name;
                                if (String.IsNullOrEmpty(forestName))
                                    forestName = domain.Forest.Name;
                            }
                            catch (Exception) { }
                        }
                        if (myRootDN.Contains(key_CurrentForest))
                        {
                            // EG 20160404 Migration vs2013
                            System.DirectoryServices.ActiveDirectory.Forest forest;
                            try
                            {
                                forest = System.DirectoryServices.ActiveDirectory.Forest.GetCurrentForest();
                                forestName = forest.Name;
                            }
                            catch (Exception) { }
                        }
                        if (!String.IsNullOrEmpty(forestName))
                            myRootDN = myRootDN.Replace(key_CurrentForest, forestName);
                        if (!String.IsNullOrEmpty(domainName))
                            myRootDN = myRootDN.Replace(key_CurrentDomain, domainName).Replace(key_ComputerDomain, domainName);
                        #endregion CurrentForest / CurrentDomain / ComputerDomain

                        //PL 20160121 Suppression du contrôle d'existance de LDAP/LDAPS et de l'ajout automatique de LDAP (pour notamment permettre l'usage de GC)
                        //PL 20160121 New feature: Keyword {RootDSE}
                        #region RootDSE (DSA-Specific Entry)
                        string key_RootDSE = "{RootDSE}";
                        if (StrFunc.IsEmpty(myRootDN) || (myRootDN.Contains(key_RootDSE)))
                        {
                            if (LDAP_Trace >= 2)
                            {
                                traceMsg = "LDAP_Trace: Domain search RootDSE";
                                if (LDAP_Trace >= 4)
                                {
                                    _logMessage = traceMsg;
                                }

                                if (null != pLog)
                                    pLog.Invoke(LogLevelEnum.None, traceMsg);

                            }
                            //UserFactory = (IUserFactory) Activator.CreateInstance(null,"WindowsApplication_Login.UserLDAP");
                            DirectoryEntry myRootDSE = new DirectoryEntry("LDAP://RootDSE");
                            if (null != myRootDSE)
                            {
                                if (StrFunc.IsEmpty(myRootDN))
                                    myRootDN = "LDAP://" + myRootDSE.Properties["defaultNamingContext"][0].ToString();
                                else
                                    myRootDN = myRootDN.Replace(key_RootDSE, myRootDSE.Properties["defaultNamingContext"][0].ToString());
                                //***************************************************************************
                                #region PL Test LDAP
                                //System.Diagnostics.Debug.WriteLine(pc.Count);
                                //foreach (System.DirectoryServices.PropertyValueCollection pc in myRootDSE.Properties)
                                //{
                                //    System.Diagnostics.Debug.WriteLine(pc.PropertyName);
                                //    System.Diagnostics.Debug.WriteLine(pc.Value);
                                //}
                                #endregion
                                //***************************************************************************
                                myRootDSE.Close();
                            }
                        }
                        #endregion RootDSE

                        if (LDAP_Trace >= 2)
                        {
                            traceMsg = "LDAP_Trace: Domain is " + myRootDN;
                            if (LDAP_Trace >= 4)
                            {
                                _logMessage = traceMsg;
                            }

                            if (null != pLog)
                                pLog.Invoke(LogLevelEnum.None, traceMsg);
                        }

                        // EG 20220623 [XXXXX]  upd for Shibboleth SP
                        userfactory = new AuthenticateLDAP(myRootDN, LDAP_Trace, pLog, pIsShibbolethAuthenticationType);
                    }
                    catch (Exception ex)
                    {
                        if (LDAP_Trace > 0)
                        {
                            string errMsg = ex.Message;
                            errMsg += " (Src: " + ex.Source + ")";
                            if (ex.InnerException != null)
                            {
                                errMsg += Cst.CrLf;
                                errMsg += " - InnerException: " + ex.InnerException.Message;
                                errMsg += " (Src: " + ex.InnerException.Source + ")";
                            }
                            if (ex.StackTrace != null)
                            {
                                errMsg += Cst.CrLf;
                                errMsg += " - StackTrace: " + ex.StackTrace;
                            }
                            traceMsg = "LDAP_Trace: ERROR - " + errMsg.Substring(0, Math.Min(errMsg.Length, 3900));
                            if (LDAP_Trace >= 3)
                            {
                                _logMessage = traceMsg;
                            }

                            if (null != pLog)
                                pLog.Invoke(LogLevelEnum.None, traceMsg);
                        }

                        if (LDAP_Trace == 0)
                            throw;
                    }
                    break;
                case "DomainNT":
                    // EG 20220623 [XXXXX]  upd for Shibboleth SP
                    userfactory = new AuthenticateDOMAINNT("EFS", pIsShibbolethAuthenticationType);
                    break;
                case "EUROSYS":
                    // EG 20220623 [XXXXX]  upd for Shibboleth SP
                    userfactory = new AuthenticateEUROSYS(pCS, pIsShibbolethAuthenticationType);
                    break;
                case "RDBMS":
                default:
                    // EG 20220623 [XXXXX]  upd for Shibboleth SP
                    userfactory = new AuthenticateRDBMS(pCS, pIsLoadSafety, pIsShibbolethAuthenticationType);
                    break;
            }
        }
        #endregion Constructors
        #region Methods
        #region ReverseDNS
        public static string ReverseDNS(string pIP)
        {
            System.Net.IPAddress adresseIP = System.Net.IPAddress.Parse(pIP);
            try
            {
                //System.Net.IPHostEntry GetIpHOST = System.Net.Dns.GetHostByAddress(adresseIP);
                System.Net.IPHostEntry GetIpHOST = System.Net.Dns.GetHostEntry(adresseIP);
                return GetIpHOST.HostName;
            }
            catch
            {
                //Si ca marche pas, on retourne l'adresse IP 
                return adresseIP.ToString();
            }
        }
        #endregion ReverseDNS
        #region CreateCollaborator
        public int CreateCollaborator(Collaborator pClb)
        {
            return userfactory.CreateCollaborator(pClb);
        }
        #endregion SearchCollaborator
        #region SearchCollaborator
        public Collaborator SearchCollaborator(string pIdentifier, bool pIgnoreCase)
        {
            return userfactory.SearchCollaborator(pIdentifier, null, null, pIgnoreCase);
        }
        public Collaborator SearchCollaborator(string pIdentifier, string pPassword, bool pIgnoreCase)
        {
            return userfactory.SearchCollaborator(pIdentifier, pPassword, null, pIgnoreCase);
        }
        public Collaborator SearchCollaborator(string pIdentifier, string pPassword, string pEntity, bool pIgnoreCase)
        {
            return userfactory.SearchCollaborator(pIdentifier, pPassword, pEntity, pIgnoreCase);
        }
        #endregion SearchCollaborator
        #endregion Methods
    }
    #endregion

    #region public class Connection
    public class Connection
    {
        #region  Members
        private string _authenticateType;
        private Collaborator _collaborator;
        #endregion

        #region Accessors
        public Collaborator Collaborator
        {
            get { return _collaborator; }
        }
        private string AuthenticateType
        {
            set { _authenticateType = value; }
            get  
            {
                if (StrFunc.IsEmpty(_authenticateType))
                    return "RDBMS"; 
                else
                    return _authenticateType; 
            }
        }
        #endregion

        #region Constructor
        /// <summary>
        /// Connection vers le système d'identification (RDBMS, LDAP...) défini dans le fichier de config (web.config)
        /// </summary>
        public Connection()
        {
            AuthenticateType = SystemSettings.GetAppSettings("AuthenticateType");
        }
        /// <summary>
        /// Connection vers un système d'identification spécifique (RDBMS, LDAP...) 
        /// </summary>
        /// <param name="pAuthenticateType"></param>
        public Connection(string pAuthenticateType)
        {
            if (StrFunc.IsFilled(pAuthenticateType))
                AuthenticateType = pAuthenticateType;
            else
                AuthenticateType = SystemSettings.GetAppSettings("AuthenticateType");
        }
        #endregion constructor

        #region public LoadCollaborator
        /// <summary>
        /// Load of an User: Characteristics, Ancestors, Model safety...
        /// <para>Warning: This constructor, withpout password, can not work on LDAP</para>
        /// </summary>
        /// <param name="pCs"></param>
        /// <param name="pLogin"></param>
        /// <returns></returns>
        // EG 20220623 [XXXXX]  upd for Shibboleth SP
        // AL 20240607 [WI955] Impersonate mode
        public bool LoadCollaboratorWithoutSafety(string pCs, string pLogin, bool pIsShibbolethAuthenticationType)
        {
            return LoadUser(pCs, pLogin, null, false, null, pIsShibbolethAuthenticationType, null);
        }
        /// <summary>
        /// Load of an User: Characteristics, Ancestors, Model safety...
        /// <para>Warning: This constructor, withpout password, can not work on LDAP</para>
        /// </summary>
        /// <param name="pCs"></param>
        /// <param name="pLogin"></param>
        /// <returns></returns>
        // EG 20220623 [XXXXX]  upd for Shibboleth SP
        // AL 20240607 [WI955] Impersonate mode
        public bool LoadCollaborator(string pCs, string pLogin, bool pIsShibbolethAuthenticationType)
        {
            return LoadUser(pCs, pLogin, null, true, null, pIsShibbolethAuthenticationType, null);
        }
        /// <summary>
        /// Load of an User: Characteristics, Ancestors, Model safety...
        /// </summary>
        /// <param name="pCs"></param>
        /// <param name="pLogin"></param>
        /// <param name="pPassword"></param>
        /// <param name="pLog"></param>
        /// <param name="pIsShibbolethAuthenticationType"></param>
        /// <returns></returns>
        /// EG 20220623 [XXXXX]  upd for Shibboleth SP
        /// FI 20240111 [WI793] usage de LogAddDetail
        // AL 20240607 [WI955] Impersonate mode
        public bool LoadCollaborator(string pCs, string pLogin, string pPassword, LogAddDetail pLog, bool pIsShibbolethAuthenticationType, string pImpersonateUserId)
        {
            return LoadUser(pCs, pLogin, pPassword, true, pLog, pIsShibbolethAuthenticationType, pImpersonateUserId);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCs"></param>
        /// <param name="pLogin"></param>
        /// <param name="pPassword"></param>
        /// <param name="pIsLoadSafety"></param>
        /// <param name="pLog"></param>
        /// <param name="pIsShibbolethAuthenticationType"></param>
        // FI 20240111 [WI793] usage de LogAddDetail
        /// <returns></returns>
        // EG 20220623 [XXXXX]  upd for Shibboleth SP
        // EG 20220220 [26251] Validity User (SUPPRESSION de UnknownIdentifier et IncorrectPassword, on GARDE UnknownIdentifierOrIncorrectPassword) 
        // AL 20240607 [WI955] Impersonate mode
        private bool LoadUser(string pCs, string pLogin, string pPassword, bool pIsLoadSafety, LogAddDetail pLog, bool pIsShibbolethAuthenticationType, string pImpersonateUserId)
        {
            Collaborator lastSearchCollaborator = null;
            string originalLogin = pLogin;

            string[] authenticateTypes = AuthenticateType.Split(new char[] { ';' });
            foreach (string tmp_authenticateType in authenticateTypes)
            {
                int LDAP_Trace = (int)SystemSettings.GetAppSettings("LDAP_Trace", typeof(Int32), 0);                //Trace LDAP
                // EG 20220623 [XXXXX]  upd for Shibboleth SP
                bool LDAP_AutoAddUser = BoolFunc.IsTrue(SystemSettings.GetAppSettings("LDAP_AutoAddUser", "true")) && (false == pIsShibbolethAuthenticationType); //Création automatique des user en tant qu'ACTOR
                string LDAP_AllowedParents = SystemSettings.GetAppSettings("LDAP_AllowedParents");                  //Liste des Parents (Entités, Desk...) autorisés
                string LDAP_DefaultParent = SystemSettings.GetAppSettings("LDAP_DefaultParent");                    //Identifiant LDAP du Parent par défaut (Entité, Desk...)

                if (tmp_authenticateType == "LDAP")
                {
                    if (LDAP_AutoAddUser)
                    {
                        if (StrFunc.IsFilled(LDAP_DefaultParent) && (!pLogin.Contains("@")))
                        {
                            //NB: On est ici sur :
                            //    - une authentification LDAP avec autorisation de création auto des users 
                            //    - un parent (Entity, Desk...) qui sera issu de l'annuaire LDAP
                            //    --> On considère donc automatiquement ce parent (suffixage auto du login par @LDAP_DefaultParent)
                            pLogin += "@" + LDAP_DefaultParent; //Tip
                        }
                        else if (StrFunc.IsFilled(LDAP_AllowedParents))
                        {
                            if (pLogin.Contains("@"))
                            {
                                //NB: On est ici sur :
                                //    - une authentification LDAP avec autorisation de création auto des users 
                                //    - un seul ou plusieurs parents (Entity, Desk...) d'autorisés
                                //    - un identifiant login avec un entityname (username@entityname)
                                //    --> On vérifié donc que l'entityname est autorisé
                                string[] arrayArobase = pLogin.Split('@');
                                int i = 0;
                                for (i = 1; i < arrayArobase.Length - 1; i++) { }
                                string parent_Identifier = arrayArobase[i];                                                                
                                if ((";" + LDAP_AllowedParents + ";").IndexOf(";" + parent_Identifier + ";") < 0)
                                    LDAP_AutoAddUser = false; //Tip
                            }
                            else if (!LDAP_AllowedParents.Contains(";"))
                            {
                                //NB: On est ici sur :
                                //    - une authentification LDAP avec autorisation de création auto des users 
                                //    - un seul et unique parent (Entity, Desk...) d'autorisé
                                //    - un identifiant login sans entityname (différent de username@entityname)
                                //    --> On considère donc automatiquement ce parent (suffixage auto du login par @AllowedParent)
                                pLogin += "@" + LDAP_AllowedParents;
                            }
                        }
                    }
                }
                else
                {
                    //Réutilisation du login d'origine, celui-ci ayant pu être enrichi dans le cas d'une authentification LDAP
                    pLogin = originalLogin;
                }

                // EG 20220623 [XXXXX]  upd for Shibboleth SP
                Authenticate auth = new Authenticate(tmp_authenticateType, pCs, pIsLoadSafety, pLog, pIsShibbolethAuthenticationType);
                _collaborator = auth.SearchCollaborator(pLogin, pPassword, false);                
                if (tmp_authenticateType == "LDAP")
                {
                    auth.LogMessage = "LDAP_TraceLevel: " + LDAP_Trace.ToString() + Cst.CrLf;
                    auth.LogMessage += "LDAP_AutoAddUser: " + LDAP_AutoAddUser.ToString() + Cst.CrLf;
                    auth.LogMessage += "LDAP_ShibbolethAutheticationType: " + pIsShibbolethAuthenticationType.ToString() + Cst.CrLf;
                    auth.LogMessage += "LDAP_AllowedParents: " + (StrFunc.IsFilled(LDAP_AllowedParents) ? LDAP_AllowedParents : "N/A") + Cst.CrLf;
                    auth.LogMessage += "LDAP_DefaultParent: " + (StrFunc.IsFilled(LDAP_DefaultParent) ? LDAP_DefaultParent : "N/A") + Cst.CrLf;

                    //PL 20160308 Add test on LDAP_Trace > 0
                    if (LDAP_Trace > 0)
                        _collaborator.LogMessage = auth.LogMessage + _collaborator.LogMessage;
                }

                if (_collaborator.Validity == Collaborator.ValidityEnum.Succes)
                {
                    #region Identification non RDBMS: on vérifie l'existance de l'utilisateur dans la table ACTOR
                    switch (tmp_authenticateType)
                    {
                        case "LDAP":
                        case "DomainNT":
                            // EG 20220623 [XXXXX]  upd for Shibboleth SP
                            Authenticate auth_RDBMS = new Authenticate("RDBMS", pCs, pIsLoadSafety, pLog, pIsShibbolethAuthenticationType);
                            string identifier_RDBMS = tmp_authenticateType + ":\\->" + _collaborator.Identifier;//Tip see SearchCollaborator() method on RDBMS class
                            Collaborator clb_RDBMS = auth_RDBMS.SearchCollaborator(identifier_RDBMS, true);
                            if (clb_RDBMS.Validity == Collaborator.ValidityEnum.Succes)
                            {
                                _collaborator = clb_RDBMS;                                
                                //TBD Voir ultérieurement pour synchroniser certaines données: dn,tel,mail... (update ACTOR from "LDAP")
                            }
                            else if (clb_RDBMS.Validity == Collaborator.ValidityEnum.MissingModelSafety)
                            {
                                _collaborator = clb_RDBMS;
                            }
                            else if (LDAP_AutoAddUser && StrFunc.IsFilled(_collaborator.Entity.Identifier))
                            {
                                #region Create a new user in ACTOR table

                                //Rôle par défaut
                                _collaborator.UserType = RoleActor.USER;
                                
                                #region DEBUG
                                #if DEBUG
                                //NB: Le rôle est initialisé lors de l'identification LDAP
                                if (_collaborator.Entity.Identifier.Contains(" as "))
                                {
                                    //Login de type: "identifier@entity as ROLE". Parent_Identifier contient ici "entity as ROLE"
                                    int posAs = _collaborator.Entity.Identifier.IndexOf(" as ");
                                    int posRole = posAs + 4;
                                    //
                                    string role = _collaborator.Entity.Identifier.Substring(posRole);
                                    if (StrFunc.IsFilled(role) && Enum.IsDefined(typeof(RoleActor), role))
                                        _collaborator.UserType = (RoleActor)Enum.Parse(typeof(RoleActor), role);

                                    //Remove "as ROLE"
                                    _collaborator.Entity.Identifier = _collaborator.Entity.Identifier.Substring(0, posAs);
                                }
                                #endif
                                #endregion DEBUG

                                auth_RDBMS.CreateCollaborator(_collaborator);
                                //Load user characteristics from ACTOR table
                                _collaborator = auth_RDBMS.SearchCollaborator(_collaborator.Identifier, false);

                                #endregion
                            }
                            else if (!LDAP_AutoAddUser)
                            {
                                //Utilisateur authentifié via LDAP, mais inexistant en tant qu'ACTOR et autorisation auto d'ACTOR non permisse
                                _collaborator.Validity = Collaborator.ValidityEnum.UnauthorizedAcces;
                            }
                            else
                            {
                                //Utilisateur authentifié via LDAP, mais inexistant en tant qu'ACTOR et Login sans Entité (username@entityname) --> Création d'un nouvel acteur impossible
                                _collaborator.Validity = Collaborator.ValidityEnum.MissingEntity;
                            }

                            //NB: Le contrôle du PWD est implictement opéré lors de la connexion à l'annuaire.
                            _collaborator.IsPwdChecked = (_collaborator.Validity == Collaborator.ValidityEnum.Succes);
                            break;
                    }

                    // AL 20240607 [WI955] Impersonate mode
                    if (StrFunc.IsFilled(pImpersonateUserId))
                    {
                        Authenticate auth_RDBMS = new Authenticate("RDBMS", pCs, pIsLoadSafety, pLog, pIsShibbolethAuthenticationType);
                        Collaborator imp_RDBMS = auth_RDBMS.SearchCollaborator(pImpersonateUserId, true);
                        if (imp_RDBMS.Validity == Collaborator.ValidityEnum.Succes)
                        {
                            bool isImpersonateEnabled = _collaborator.IsActorSysAdmin;
                            if (!_collaborator.IsActorSysAdmin)
                            {
                                isImpersonateEnabled = false;
                                StrBuilder sqlSelect = new StrBuilder(SQLCst.SELECT);
                                sqlSelect += "IDIMPERSONATE" + Cst.CrLf;
                                sqlSelect += SQLCst.FROM_DBO + Cst.OTCml_TBL.ACTORIMPERSONATE.ToString() + SQLCst.AS + "imp" + Cst.CrLf;
                                sqlSelect += SQLCst.WHERE + OTCmlHelper.GetSQLDataDtEnabled(pCs, "imp").ToString() + Cst.CrLf;
                                sqlSelect += SQLCst.AND + "imp.IDA=@IDA" + Cst.CrLf;
                                sqlSelect += SQLCst.AND + "imp.IDA_ACTORIMP=@IDA_ACTORIMP" + Cst.CrLf;

                                DataParameters sqlParam = new DataParameters();
                                sqlParam.Add(new DataParameter(pCs, "IDA", DbType.Int32), _collaborator.Ida);
                                sqlParam.Add(new DataParameter(pCs, "IDA_ACTORIMP", DbType.Int32), imp_RDBMS.Ida);

                                QueryParameters qryParameters = new QueryParameters(pCs, sqlSelect.ToString(), sqlParam);                                

                                using (IDataReader dr = DataHelper.ExecuteReader(pCs, CommandType.Text, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter()))
                                {
                                    if (dr.Read())
                                        isImpersonateEnabled = true;
                                }
                            }
                            if (isImpersonateEnabled)
                            {
                                _collaborator.ImpersonateDisplayName = imp_RDBMS.DisplayName;
                                _collaborator.ImpersonateIdentifier = pImpersonateUserId;
                                _collaborator.UserType = imp_RDBMS.UserType;
                            }
                            else
                            {
                                _collaborator.LogMessage += "Impersonation of " + pImpersonateUserId + " not enabled" + Cst.CrLf;
                                _collaborator.Validity = Collaborator.ValidityEnum.UnauthorizedAcces;
                            }
                        }
                        else
                        {
                            _collaborator.LogMessage += "Not found " + pImpersonateUserId + Cst.CrLf;
                            _collaborator.Validity = Collaborator.ValidityEnum.UnknownIdentifierOrIncorrectPassword;
                        }
                    }

                    #endregion

                    break;
                }
                else //if (_collaborator.Validity != Collaborator.ValidityEnum.UnknownIdentifier)
                {
                    if (lastSearchCollaborator != null)
                        _collaborator.LogMessage += lastSearchCollaborator.LogMessage;

                    lastSearchCollaborator = _collaborator;
                }
            }
            // EG 20220220 [26251] Validity User (SUPPRESSION de UnknownIdentifier et IncorrectPassword, on GARDE UnknownIdentifierOrIncorrectPassword) 
            if (
                (_collaborator.Validity == Collaborator.ValidityEnum.UnknownIdentifierOrIncorrectPassword || 
                _collaborator.Validity == Collaborator.ValidityEnum.MissingIdentifier || 
                _collaborator.Validity == Collaborator.ValidityEnum.UnreachableDirectory)
                && (lastSearchCollaborator != null))
            {
                //Restauration de la recherche précédente, afin de disposer de ses informations (cas d'une authentification multiple)
                _collaborator = lastSearchCollaborator;
            }
            else if (lastSearchCollaborator != null && StrFunc.IsFilled(lastSearchCollaborator.LogMessage) && (_collaborator.LogMessage != lastSearchCollaborator.LogMessage))
            {
                //Restauration du log (trace) de la recherche précédente, afin de disposer de ses informations (cas d'une authentification multiple)
                _collaborator.LogMessage += lastSearchCollaborator.LogMessage;
            }
            return (_collaborator.Validity == Collaborator.ValidityEnum.Succes);
        }
        #endregion 
    }
    #endregion class Connection


    /// <summary>
    /// 
    /// </summary>
    public static class CheckConnection
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCs"></param>
        /// <param name="pCollaborator"></param>
        /// <param name="pLoginExpirationDateTime"></param>
        /// <returns></returns>
        /// FI 20120713 [] usage de parameters et application du cache SQL
        public static bool CheckModelDayHour(string pCs, Collaborator pCollaborator, ref DateTime pLoginExpirationDateTime)
        {

            bool isOk = false;
            //
            if (!isOk)
            {
                //Aucun contrôle pour un administrateur
                isOk = pCollaborator.IsActorSysAdmin;
            }
            //
            if (!isOk)
            {
                // RD 20110223 [16324] Utilisation de pCollaborator.IdModelSafety chargé dans AuthenticateRDBMS.SearchCollaborator()
                // IdModelSafety peut être celui définit directement sur l'Actor, 
                // dans le cas contraire, c'est celui définit sur un des ancetres de Actor
                bool existsModelDayHour = (pCollaborator.IdModelSafety > 0);
                //
                if (existsModelDayHour)
                {
                    #region Check if a MODELDAYHOUR exists for IDA
                    // RD 20110223 [16324] Utilisation de pCollaborator.IdModelSafety
                    StrBuilder sqlQuery = new StrBuilder(SQLCst.SELECT);
                    sqlQuery += " mdh.MODELNAME as MODELNAME " + Cst.CrLf;
                    sqlQuery += SQLCst.FROM_DBO + Cst.OTCml_TBL.MODELSAFETY + " ms" + Cst.CrLf;
                    sqlQuery += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.MODELDAYHOUR + " mdh on (mdh.IDMODELDAYHOUR = ms.IDMODELDAYHOUR)" + Cst.CrLf;
                    sqlQuery += SQLCst.WHERE + "ms.IDMODELSAFETY=@ID";
                    //
                    DataParameters dp = new DataParameters();
                    dp.Add(DataParameter.GetParameter(pCs, DataParameter.ParameterEnum.ID), pCollaborator.IdModelSafety);
                    QueryParameters qryParameters = new QueryParameters(pCs, sqlQuery.ToString(), dp);
                    //
                    IDataReader drCheck = DataHelper.ExecuteDataTable(CSTools.SetCacheOn(pCs), qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter()).CreateDataReader();
                    //
                    existsModelDayHour = drCheck.Read();
                    drCheck.Close();
                    #endregion
                    //
                    if (existsModelDayHour)
                    {
                        #region Verif des heures de connexions
                        pLoginExpirationDateTime = new DateTime(1, 1, 1);
                        DateTime today = OTCmlHelper.GetDateSys(pCs);
                        
                        // RD 20110223 [16324] Utilisation de pCollaborator.IdModelSafety
                        StrBuilder sqlQuery2 = new StrBuilder(SQLCst.SELECT);
                        sqlQuery2 += "h.TIMESTART, h.TIMEEND " + Cst.CrLf;
                        sqlQuery2 += SQLCst.FROM_DBO + Cst.OTCml_TBL.MODELSAFETY + " ms" + Cst.CrLf;
                        sqlQuery2 += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.MODELDAYHOUR + " mdh on (mdh.IDMODELDAYHOUR = ms.IDMODELDAYHOUR)" + Cst.CrLf;
                        sqlQuery2 += SQLCst.AND + OTCmlHelper.GetSQLDataDtEnabled(pCs, "mdh") + Cst.CrLf;
                        sqlQuery2 += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.DAYHOURMODEL + " dhm on (dhm.IDMODELDAYHOUR = mdh.IDMODELDAYHOUR)" + Cst.CrLf;
                        sqlQuery2 += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.DAYHOUR + " dh on (dh.IDDAYHOUR = dhm.IDDAYHOUR and dh.DDD =@DDD)" + Cst.CrLf;
                        sqlQuery2 += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.HOUR + " h on (h.IDHOUR = dh.IDHOUR) " + Cst.CrLf;
                        sqlQuery2 += SQLCst.WHERE + "ms.IDMODELSAFETY=@ID";
                        sqlQuery2 += SQLCst.ORDERBY + "h.TIMESTART";

                        DataParameters dp2 = new DataParameters();
                        dp2.Add(DataParameter.GetParameter(pCs, DataParameter.ParameterEnum.ID), pCollaborator.IdModelSafety);
                        dp2.Add(new DataParameter(pCs, "DDD", DbType.AnsiStringFixedLength, 3), today.DayOfWeek.ToString().ToUpper().Substring(0, 3));
                        QueryParameters qryParameters2 = new QueryParameters(pCs, sqlQuery2.ToString(), dp2);

                        //20070717 FI utilisation de ExecuteDataTable pour le Cache
                        using (IDataReader dr = DataHelper.ExecuteDataTable(CSTools.SetCacheOn(pCs), qryParameters2.Query, qryParameters2.Parameters.GetArrayDbParameter()).CreateDataReader())
                        {
                            while (dr.Read())
                            {
                                string timeString = dr["TIMEEND"].ToString();
                                int[] timer = StrFunc.StringToIntTime(timeString);
                                DateTime dtEnd = new DateTime(today.Year, today.Month, today.Day, timer[0], timer[1], 0);
                                if (today <= dtEnd)
                                {
                                    timeString = dr["TIMESTART"].ToString();
                                    timer = StrFunc.StringToIntTime(timeString);
                                    DateTime dtStart;
                                    dtStart = new DateTime(today.Year, today.Month, today.Day, timer[0], timer[1], 0);

                                    if (today >= dtStart)
                                    {
                                        pLoginExpirationDateTime = dtEnd;
                                        break;
                                    }
                                }
                            }
                        }
                        //
                        isOk = (today < pLoginExpirationDateTime);
                        #endregion
                    }
                }
            }
            //
            return isOk;
        }

        /// <summary>
        /// Contrôle le nombre d'acteurs disposant du rôle entité par rapport au nombre autorisé dans la licence d'utilisation.
        /// <para>NB: Volontairement popur l'instant (Piratage et autre...), on exclut l'exploitation des colonnes DTENABLED/DTDISABLED</para>
        /// </summary>
        /// <param name="pCs"></param>
        /// <param name="pCollaborator"></param>
        /// <param name="pMaxEntityAuthorized"></param>
        /// <param name="opErrMessage"></param>
        /// <returns></returns>
        /// FI 20120713 [] usage de parameters 
        public static bool CheckMaxEntity(string pCs, Collaborator pCollaborator, int pMaxEntityAuthorized, ref string opErrMessage)
        {
            int countEntity = 0;
            StrBuilder SQLSelect = new StrBuilder(SQLCst.SELECT);
            SQLSelect += "count(*)" + Cst.CrLf;
            SQLSelect += SQLCst.FROM_DBO + Cst.OTCml_TBL.ACTORROLE.ToString() + Cst.CrLf;
            SQLSelect += SQLCst.WHERE + "IDROLEACTOR=@IDROLEACTOR";

            DataParameters dp = new DataParameters();
            dp.Add(DataParameter.GetParameter(pCs, DataParameter.ParameterEnum.IDROLEACTOR), RoleActor.ENTITY);

            QueryParameters qry = new QueryParameters(pCs, SQLSelect.ToString(), dp);

            object obj = DataHelper.ExecuteScalar(pCs, CommandType.Text, qry.Query, qry.Parameters.GetArrayDbParameter());
            if (null != obj)
                countEntity = Convert.ToInt32(obj);

            bool isOk = countEntity <= pMaxEntityAuthorized;
            if (!isOk)
            {
                if (pCollaborator.IsActorSysAdmin)
                    opErrMessage = "FailureEntity_MaxAuthorized_Det";//L'application a atteint ses {0} entités autorisées, contactez votre administrateur !
                else
                    opErrMessage = "FailureEntity_MaxAuthorized";    //L'application a atteint son maximum d'entités autorisées, contactez votre administrateur !
            }

            return isOk;
        }

        /// <summary>
        /// Contrôle la connection à la base de donnée
        /// <para>Charge le DAL puis vérifie que la connexion s'opère à la base de donnée </para>
        /// </summary>
        /// <param name="pCS"></param>
        /// <returns></returns>
        public static void CheckConnectionString(string pCs)
        {
            DataHelper.CheckConnection(pCs);
        }

        #region IsPwdLikeValueIntercalate
        /// <summary>
        /// Vérifier si le Mot de passe (pPassword) ne contient pas la Valeur de contrôle (pCheckValue), dont les lettres sont séparées par le même caractère 
        /// Exemple: Valeur de contrôle Abcd --> interdire A_B_C_D, xAxBxCxDx, a*b*c*d*,...)
        /// </summary>
        /// <param name="pPwd"></param>
        /// <param name="pCheckValue"></param>
        /// <returns></returns>
        /// RD 20220215 [25943] New
        private static bool IsPwdLikeValueIntercalate(string pPwd, string pCheckValue)
        {
            // Mot de passe moins de 3 caractères ou Valeur de contrôle moins de deux caractères
            if (pPwd.Length < 3 || (pCheckValue.Length < 2))
                return false;

            string sPassword = pPwd.ToUpper();
            string sCheckValue = pCheckValue.ToUpper();

            string sFirstChar = sCheckValue.Substring(0, 1);
            int iIndexFirstChar = sPassword.IndexOf(sFirstChar);

            while (iIndexFirstChar > -1)
            {
                string sSecondChar = sCheckValue.Substring(1, 1);
                int iIndexSecondChar = sPassword.IndexOf(sSecondChar);

                if ((iIndexSecondChar > -1) && (iIndexSecondChar == iIndexFirstChar + 2))
                {
                    char cInterlayer = Convert.ToChar(sPassword.Substring(iIndexFirstChar + 1, 1));

                    if (sPassword.Contains(StrFunc.IntercalateString(sCheckValue, cInterlayer)))
                        return true;
                }

                iIndexFirstChar = sPassword.IndexOf(sFirstChar, iIndexFirstChar + 1);
            }

            return false;
        }
        #endregion
        #region IsPwdLikeValue
        /// <summary>
        /// Vérifier si le Mot de passe (pPassword) n'est pas constitué de la Valeur de contrôle (pCheckValue) ou d'une partie ou de son Reverse
        /// </summary>
        /// <param name="pPwd"></param>
        /// <param name="pCheckValue">Valeur de contrôle</param>
        /// <returns></returns>
        /// RD 20220211 [25943] New
        public static bool IsPwdLikeValue(string pPwd, string pCheckValue)
        {
            // Mot de passe ou Valeur de contrôle vides
            if (StrFunc.IsEmpty(pPwd) || (StrFunc.IsEmpty(pCheckValue)))
                return false;

            string sPassword = pPwd.ToUpper();
            string sCheckValue = pCheckValue.ToUpper();

            // PWD différent de la Valeur de contrôle ou de son Reverse
            if ((sPassword == sCheckValue) || (sPassword == StrFunc.ReverseString(sCheckValue)))
                return true;
            // PWD ne soit pas constitué à partir de la Valeur de contrôle ou de son Reverse
            else if (sPassword.Contains(sCheckValue) || sPassword.Contains(StrFunc.ReverseString(sCheckValue)))
                return true;
            // PWD ne soit pas constitué de la Valeur de contrôle moins le premier caractère ou de son Reverse
            else if (sPassword.Contains(sCheckValue.Substring(1)) || sPassword.Contains(StrFunc.ReverseString(sCheckValue.Substring(1))))
                return true;
            // PWD ne soit pas constitué de la Valeur de contrôle moins le dernier caractère ou de son Reverse
            else if (sPassword.Contains(sCheckValue.Substring(0, sCheckValue.Length - 1)) || sPassword.Contains(StrFunc.ReverseString(sCheckValue.Substring(0, sCheckValue.Length - 1))))
                return true;
            // PWD ne contient pas la Valeur de contrôle ou de son Reverse, dont les lettres sont séparées par le même caractère
            // Ex. Identifiant Abcd --> interdire A_B_C_D, xAxBxCxDx, a*b*c*d*, D_C_B_A, xDxCxBxAx, d*c*b*a*,...)
            else if (IsPwdLikeValueIntercalate(sPassword, sCheckValue) || IsPwdLikeValueIntercalate(sPassword, StrFunc.ReverseString(sCheckValue)))
                return true;

            return false;
        }
        #endregion IsPwdLikeValue
        #region IsPwdForbidden
        /// <summary>
        /// Vérifie si le PWD (pPwd) est présent dans la liste des valeurs des PWD interdits (PWDFORBIDDENLIST)
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pPwd"></param>
        /// <param name="pPwdForbiddenList"></param>
        /// <param name="pIdA"></param>
        /// <returns></returns>
        /// RD 20220211 [25943] New
        // EG 20220519 [WI637] Ajout Signature pRoleTypeExclude (ActorAncestor)
        public static bool IsPwdForbidden(string pCS, string pPwd, List<string> pPwdForbiddenList, int pIdA)
        {
            if (null == pPwdForbiddenList)
                return false;

            if (pPwdForbiddenList.Exists(item => item == pPwd))
                return true;

            if (pPwdForbiddenList.Contains(Cst.PWDFORBIDDENLIST_USER_ID) ||
                pPwdForbiddenList.Contains(Cst.PWDFORBIDDENLIST_PERSONAL_ID) ||
                pPwdForbiddenList.Contains(Cst.PWDFORBIDDENLIST_ADDRESS_ID) ||
                pPwdForbiddenList.Contains(Cst.PWDFORBIDDENLIST_ALL_ID))
            {
                IDataReader dr = null;

                try
                {
                    string sqlSelect = @"select a.DISPLAYNAME,a.DESCRIPTION,a.EXTLLINK,a.EXTLLINK2,
                    a.SURNAME,a.FIRSTNAME,a.MIDDLENAME,a.BIRTHPLACE,a.DTBIRTH,
                    a.ADDRESS1,a.ADDRESS2,a.ADDRESS3,a.ADDRESS4,a.ADDRESSPOSTALCODE,a.ADDRESSCITY,a.ADDRESSSTATE,a.ADDRESSCOUNTRY,a.TELEPHONENUMBER,a.MOBILEPHONENUMBER,a.FAXNUMBER,a.MAIL
                    from dbo.ACTOR a
                    inner join dbo.MODELSAFETY ms on (ms.IDMODELSAFETY=a.IDMODELSAFETY)
                    where (a.IDA=@IDA)" + Cst.CrLf;

                    DataParameters parameters = new DataParameters();
                    parameters.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.IDA), pIdA);
                    QueryParameters qry = new QueryParameters(pCS, sqlSelect, parameters);
                    dr = DataHelper.ExecuteReader(pCS, CommandType.Text, qry.Query, qry.Parameters.GetArrayDbParameter());

                    if (dr.Read())
                    {
                        // Add details for %%USER_ID%%
                        if (pPwdForbiddenList.Contains(Cst.PWDFORBIDDENLIST_USER_ID) || pPwdForbiddenList.Contains(Cst.PWDFORBIDDENLIST_ALL_ID))
                        {
                            if (IsPwdLikeValue(pPwd, Convert.ToString(dr["DISPLAYNAME"])))
                                return true;
                            if (!(dr["DESCRIPTION"] is DBNull) && IsPwdLikeValue(pPwd, Convert.ToString(dr["DESCRIPTION"])))
                                return true;
                            if (!(dr["EXTLLINK"] is DBNull) && IsPwdLikeValue(pPwd, Convert.ToString(dr["EXTLLINK"])))
                                return true;
                            if (!(dr["EXTLLINK2"] is DBNull) && IsPwdLikeValue(pPwd, Convert.ToString(dr["EXTLLINK2"])))
                                return true;
                        }
                        // Add details for %%PERSONAL_ID%%
                        if (pPwdForbiddenList.Contains(Cst.PWDFORBIDDENLIST_PERSONAL_ID) || pPwdForbiddenList.Contains(Cst.PWDFORBIDDENLIST_ALL_ID))
                        {
                            if (!(dr["SURNAME"] is DBNull) && IsPwdLikeValue(pPwd, Convert.ToString(dr["SURNAME"])))
                                return true;
                            if (!(dr["FIRSTNAME"] is DBNull) && IsPwdLikeValue(pPwd, Convert.ToString(dr["FIRSTNAME"])))
                                return true;
                            if (!(dr["MIDDLENAME"] is DBNull) && IsPwdLikeValue(pPwd, Convert.ToString(dr["MIDDLENAME"])))
                                return true;
                            if (!(dr["BIRTHPLACE"] is DBNull) && IsPwdLikeValue(pPwd, Convert.ToString(dr["BIRTHPLACE"])))
                                return true;

                            if (!(dr["DTBIRTH"] is DBNull))
                            {
                                DateTime dtBirth = Convert.ToDateTime(dr["DTBIRTH"]);

                                if (IsPwdLikeValue(pPwd, DtFunc.DateTimeToString(dtBirth, "yyyyMMdd")))
                                    return true;
                                if (IsPwdLikeValue(pPwd, DtFunc.DateTimeToString(dtBirth, "ddMMyyyy")))
                                    return true;
                                if (IsPwdLikeValue(pPwd, DtFunc.DateTimeToString(dtBirth, "yyyy/MM/dd")))
                                    return true;
                                if (IsPwdLikeValue(pPwd, DtFunc.DateTimeToString(dtBirth, "dd/MM/yyyy")))
                                    return true;
                                if (IsPwdLikeValue(pPwd, DtFunc.DateTimeToString(dtBirth, "yyyy-MM-dd")))
                                    return true;
                                if (IsPwdLikeValue(pPwd, DtFunc.DateTimeToString(dtBirth, "dd-MM-yyyy")))
                                    return true;
                            }
                        }
                        // Add details for %%ADDRESS_ID%%
                        if (pPwdForbiddenList.Contains(Cst.PWDFORBIDDENLIST_ADDRESS_ID) || pPwdForbiddenList.Contains(Cst.PWDFORBIDDENLIST_ALL_ID))
                        {
                            if (!(dr["ADDRESS1"] is DBNull) && IsPwdLikeValue(pPwd, Convert.ToString(dr["ADDRESS1"])))
                                return true;
                            if (!(dr["ADDRESS2"] is DBNull) && IsPwdLikeValue(pPwd, Convert.ToString(dr["ADDRESS2"])))
                                return true;
                            if (!(dr["ADDRESS3"] is DBNull) && IsPwdLikeValue(pPwd, Convert.ToString(dr["ADDRESS3"])))
                                return true;
                            if (!(dr["ADDRESS4"] is DBNull) && IsPwdLikeValue(pPwd, Convert.ToString(dr["ADDRESS4"])))
                                return true;
                            if (!(dr["ADDRESSPOSTALCODE"] is DBNull) && IsPwdLikeValue(pPwd, Convert.ToString(dr["ADDRESSPOSTALCODE"])))
                                return true;
                            if (!(dr["ADDRESSCITY"] is DBNull) && IsPwdLikeValue(pPwd, Convert.ToString(dr["ADDRESSCITY"])))
                                return true;
                            if (!(dr["ADDRESSSTATE"] is DBNull) && IsPwdLikeValue(pPwd, Convert.ToString(dr["ADDRESSSTATE"])))
                                return true;
                            if (!(dr["ADDRESSCOUNTRY"] is DBNull) && IsPwdLikeValue(pPwd, Convert.ToString(dr["ADDRESSCOUNTRY"])))
                                return true;
                            if (!(dr["TELEPHONENUMBER"] is DBNull) && IsPwdLikeValue(pPwd, Convert.ToString(dr["TELEPHONENUMBER"])))
                                return true;
                            if (!(dr["MOBILEPHONENUMBER"] is DBNull) && IsPwdLikeValue(pPwd, Convert.ToString(dr["MOBILEPHONENUMBER"])))
                                return true;
                            if (!(dr["FAXNUMBER"] is DBNull) && IsPwdLikeValue(pPwd, Convert.ToString(dr["FAXNUMBER"])))
                                return true;
                            if (!(dr["MAIL"] is DBNull) && IsPwdLikeValue(pPwd, Convert.ToString(dr["MAIL"])))
                                return true;
                        }
                    }
                }
                catch (Exception) { throw; }
                finally
                {
                    if (null != dr)
                        dr.Dispose();
                }
            }

            // RD 20220215[25943] Add details for %%ENTITY_ID%% and %%PARENTS_ID%%
            if (pPwdForbiddenList.Contains(Cst.PWDFORBIDDENLIST_ENTITY_ID) ||
                pPwdForbiddenList.Contains(Cst.PWDFORBIDDENLIST_PARENTS_ID) ||
                pPwdForbiddenList.Contains(Cst.PWDFORBIDDENLIST_ALL_ID))
            {
                ActorAncestor actorAncestor = new ActorAncestor(pCS,null, pIdA, pRoleRestrict: null, pRoleTypeExclude: null, pIsPartyRelation: false);
                List<string> idParent_List = new List<string>();

                if (pPwdForbiddenList.Contains(Cst.PWDFORBIDDENLIST_PARENTS_ID) || pPwdForbiddenList.Contains(Cst.PWDFORBIDDENLIST_ALL_ID))
                {   
                    int max = actorAncestor.GetLevelLength();

                    for (int i = 1; i < max; i++)
                    {
                        string lst = actorAncestor.GetListIdA_ActorByLevel(i, ";");
                        idParent_List.AddRange(new List<string>(lst.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries)));
                    }
                }
                else if (pPwdForbiddenList.Contains(Cst.PWDFORBIDDENLIST_ENTITY_ID))
                {
                    //Recherche d'un parent ENTITY ou SUPPORT
                    int idAParent = actorAncestor.GetFirstRelation(RoleActor.ENTITY);

                    if (idAParent > 0)
                        idParent_List.Add(idAParent.ToString());
                    else
                    {
                        //Aucun parent ENTITY --> Tentative de recherche d'un parent SUPPORT (ie: EFS)
                        idAParent = actorAncestor.GetFirstRelation(RoleActor.SUPPORT);
                        if (idAParent > 0)
                            idParent_List.Add(idAParent.ToString());
                    }
                };

                SQL_Actor sql_Actor = null;

                foreach (var idAParent in idParent_List)
                {
                    sql_Actor = new SQL_Actor(pCS, Convert.ToInt32(idAParent));

                    if (sql_Actor.IsLoaded)
                    {
                        if (IsPwdLikeValue(pPwd, sql_Actor.Identifier))
                            return true;
                        if (IsPwdLikeValue(pPwd, sql_Actor.DisplayName))
                            return true;
                        if (IsPwdLikeValue(pPwd, sql_Actor.Description))
                            return true;
                        if (IsPwdLikeValue(pPwd, sql_Actor.ExtlLink))
                            return true;
                        if (IsPwdLikeValue(pPwd, sql_Actor.ExtlLink2))
                            return true;
                    }
                }
            }

            return false;
        }
        #endregion IsPwdForbidden
    }

}