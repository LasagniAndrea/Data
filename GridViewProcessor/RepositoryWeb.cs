#region using directives
using EFS.ACommon;
using EFS.ApplicationBlocks.Data;
using EFS.Common.Web;
using EFS.Rights;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
#endregion using directives

namespace EFS.GridViewProcessor
{
    /// <summary>
    /// Description résumée de ReferentialWeb.
    /// </summary>
    public sealed class RepositoryWeb
    {
        #region constructor
        public RepositoryWeb() { }
        #endregion

        public const string PrefixForReferential = "REF-";
        public const string TEMPORARYPREFIX = "*";
        public const int MinRefreshInterval = 20;
        //20091005 FI Session n'a pas de sens, Cookies est suffisant
        private const string Session_TemplateName = "Session_TemplateName";
        private const string Session_TemplateOwner = "Session_TemplateOwner";

        /// <summary>
        /// Obtient, pour une consultation donnée, un couple IdLstTemplate / IdA identfiant le template à utiliser.
        /// <para>La recherche s'effectue sur différents critères: Template spécifique défini dans l'url, dernier template utilisé au sein de la session courante, template par défaut...</para>
        /// <para>Un nouveau template est automatqiuement créé lorsque la recherche n'aboutit pas.</para>
        /// <para>Anciennement: GetLastUse()</para>
        /// </summary>
        /// <param name="pPage">Page appelante</param>
        /// <param name="pIdLstConsult">Nom de la consultation</param>
        /// <param name="pTitle"></param>
        /// <param name="pIsNewTemplate">Si true , création systématique d'un nouveau template lors de la 1ère publication</param>
        /// <param name="pValueForFilter"></param>
        /// <param name="pIndexForFilter"></param>
        /// <param name="pColumnForFilter"></param>
        /// <param name="pIsLoadOnStart"></param>
        /// <param name="opIdLstTemplate">Nom du template</param>
        /// <param name="opIdA">ID du propriétaire du template</param>
        public static void GetTemplate(PageBase pPage, string pIdLstConsult,
                                string pTitle, bool pIsNewTemplate,
                                string pValueForFilter, string pTableForFilter, string pColumnForFilter,
                                bool pIsLoadOnStart,
                                out string opIdLstTemplate, out int opIdA)
        {
            //---------------------------------------------------------------------------------------------------
            //NOTE:
            //- pIsNewTemplate = true lorsque lorsque l'on vient d'un bouton "..."
            //- pIsNewTemplate = true lorsque lorsque l'on vient d'un enfant (Exemple les books d'un acteur)
            //Lorsque pIsNewTemplate= true => on génère systématiquement un nouveau template
            //=> ce template est "temporaire" il n'a pas vocation a être stocké => pas de sauvegarde dans Cookie  
            //---------------------------------------------------------------------------------------------------
            string guid = string.Empty;

            HiddenField hiddenField = (HiddenField)pPage.MasterPage_ContentPlaceHolder.FindControl("__GUID");
            if (null != hiddenField)
                guid = hiddenField.Value;

            opIdA = SessionTools.Collaborator_IDA;
            opIdLstTemplate = string.Empty;

            //---------------------------------------------------------------------------------------------------
            //On retourne en priorité,s'il en existe un, le template courant stocké en Session.
            //NB: Lors de la 1ère publication opIdLstTemplate sera nécessairement null.
            //NB2:Dans la clé cookée en Session, il y a un GUID 
            //---------------------------------------------------------------------------------------------------
            ReadTemplateSession(pPage, pIdLstConsult, guid, ref opIdLstTemplate, ref opIdA);

            #region TF - Template forced (QueryString["TF"])
            //NB: Lorsque le Template forced défini est inexistant, un nouveau est automatiquement créé. (Utilisé par EG avec le Tracker)
            string idLstTemplateForced = pPage.Request.QueryString["TF"];
            int urlIdA = Convert.ToInt32(pPage.Request.QueryString["A"]);

            bool isNewTemplateForced = false;
            if (!String.IsNullOrEmpty(idLstTemplateForced))
            {
                if (String.IsNullOrEmpty(opIdLstTemplate)
                    || ((urlIdA == 0) && (opIdA != SessionTools.Collaborator_IDA)) || ((urlIdA > 0) && (opIdA != urlIdA))
                    || (opIdLstTemplate != idLstTemplateForced) || ((IsTemporary(opIdLstTemplate)) && (RemoveTemporaryPrefix(opIdLstTemplate) != (idLstTemplateForced))))
                {
                    //Template courant stocké en Session: 
                    //- Il n'en existe aucun 
                    //- ou il n'appartient pas à l'utilisateur courant ou présent dans l'URL
                    //- ou il est différent de celui décrit dans l'URL
                    //--> Utilisation du TemplateForced (TF) présent dasn l'URL
                    opIdLstTemplate = idLstTemplateForced;
                    if (urlIdA != 0)
                        opIdA = urlIdA;
                }

                if (ExistsTemplate2(pPage.CS, pIdLstConsult, ref opIdLstTemplate, opIdA))
                {
                    //Si le propriétaire du template est l'utilisateur lui même, pas de contrôle.
                    if (opIdA == SessionTools.Collaborator_IDA)
                    {
                        if (!HasUserRightForTemplate(pPage.CS, opIdA, pIdLstConsult, opIdLstTemplate, RightsTypeEnum.VIEW))
                        {
                            //Droits non présents --> On n'utilise pas ce TemplateForced (TF)
                            opIdA = SessionTools.Collaborator_IDA;
                            opIdLstTemplate = string.Empty;
                        }
                    }
                }
                else
                {
                    //TemplateForced (TF)  inexistant --> On le crée
                    isNewTemplateForced = true;
                }
            }
            #endregion TF - Template forced

            if (!(pIsNewTemplate || isNewTemplateForced))
            {
                #region Recherche du template à ouvrir
                //S'il existe, on considère l'IDLSTTEMPLATE présent dans QueryString["T"]
                if (String.IsNullOrEmpty(opIdLstTemplate))
                {
                    opIdLstTemplate = pPage.Request.QueryString["T"];
                    opIdA = Convert.ToInt32(pPage.Request.QueryString["A"]);
                    if (opIdA == 0)
                        opIdA = SessionTools.User.idA;
                }
                //Sinon, on considère celui défini en tant que Défaut (table LSTTEMPLATEDEF)
                if (String.IsNullOrEmpty(opIdLstTemplate))
                {
                    opIdLstTemplate = SearchTemplate(pPage.CS, pIdLstConsult, null);
                    if ((!String.IsNullOrEmpty(opIdLstTemplate)) && IsTemporary(opIdLstTemplate))
                    {
                        CleanDefault(pPage.CS, pIdLstConsult);
                        opIdLstTemplate = SearchTemplate(pPage.CS, pIdLstConsult, null);
                    }
                }
                //Sinon, on considère celui présent dans Cookie
                if (String.IsNullOrEmpty(opIdLstTemplate))
                {
                    ReadTemplateCookie(pIdLstConsult, out opIdLstTemplate, out opIdA);
                }
                //PL 20150601 GUEST New feature
                //Si un template est trouvé, on verifie sa validité (si invalide on le reset)
                CheckTemplate(pPage.CS, pIdLstConsult, ref opIdLstTemplate, ref opIdA);

                //Si aucun template trouvé et valide, et si GUEST, on considère celui défini en tant que Défaut pour son "Department" (table LSTTEMPLATEDEF)
                if (String.IsNullOrEmpty(opIdLstTemplate) && SessionTools.IsSessionGuest && (SessionTools.Collaborator.Department.Ida > 0))
                {
                    opIdLstTemplate = SearchTemplate(pPage.CS, pIdLstConsult, SessionTools.Collaborator.Department.Ida);
                    if (!String.IsNullOrEmpty(opIdLstTemplate))
                    {
                        opIdA = SessionTools.Collaborator.Department.Ida;
                        CheckTemplate(pPage.CS, pIdLstConsult, ref opIdLstTemplate, ref opIdA);
                    }
                }
                #endregion
            }

            //---------------------------------------------------------------------------------------------------
            //Lorsqu'aucun template n'est trouvé, on créé un nouveau template par défaut 
            //(Rq.: Ce qui est tjs le cas, lors d'un accès via le bouton "...")
            //(Rq.: Ce qui est tjs le cas, lorsque l'on consulte un référentiel Enfant)
            //---------------------------------------------------------------------------------------------------
            if (String.IsNullOrEmpty(opIdLstTemplate) || isNewTemplateForced)
            {
                #region Création d'un nouveau template
                bool isExistsValueForFilter = StrFunc.IsFilled(pValueForFilter);//Renseigné lors d'un accès via le bouton "..."

                //TF - Template forced: Si inexistant, un nouveau template est automatiquement créé. (Utilisé par EG avec le Tracker)
                string idLstTemplate = pPage.Request.QueryString["TF"];
                if (String.IsNullOrEmpty(idLstTemplate))
                    idLstTemplate = isExistsValueForFilter ? GetSelectName() : GetNewQueryName();

                opIdA = SessionTools.Collaborator_IDA;

                if (isExistsValueForFilter)
                {
                    //Création systématique d'un nouveau template nommé: *Select
                    opIdLstTemplate = CreateNewTemporaryTemplate(pPage.CS, pIdLstConsult, idLstTemplate, pValueForFilter, pTableForFilter, pColumnForFilter, pIsLoadOnStart, pTitle);
                }
                else
                {
                    //PL 20150601 GUEST New feature
                    //Si GUEST, on affiche un message d'alerte
                    if (SessionTools.IsSessionGuest)
                        pPage.MsgForAlertImmediate = Ressource.GetStringForJS("Msg_NoTemplateAvailable");
                    else
                        opIdLstTemplate = CreateNewTemporaryTemplate(pPage.CS, pIdLstConsult, idLstTemplate, pIsLoadOnStart, pTitle);
                }
                #endregion
            }

            #region Mise à jour du cookie (pour une prochaine consultation)
            if (!String.IsNullOrEmpty(opIdLstTemplate))
            {
                if (!(pIsNewTemplate || isNewTemplateForced))
                    WriteTemplateCookie(pIdLstConsult, opIdLstTemplate, opIdA);

                WriteTemplateSession(pPage, pIdLstConsult, opIdLstTemplate, opIdA, guid);
            }
            #endregion
        }
        /// <summary>
        /// Vérification de la validé d'un template.
        /// <para>NB: Si invalide, on le reset.</para>
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pIdLstConsult"></param>
        /// <param name="opIdLstTemplate"></param>
        /// <param name="opIdA"></param>
        private static void CheckTemplate(string pCS, string pIdLstConsult, ref string opIdLstTemplate, ref int opIdA)
        {
            #region Contrôle d'existence du Template trouvé, et des droits d'accès à son égard.
            bool isExistAndHasUserRight = false;
            if (!String.IsNullOrEmpty(opIdLstTemplate))
            {
                // RD 20130103 [18332]
                // Utilisation de la méthode ExistsTemplate3(), si le template temporaire n'existe pas alors rechercher un template final (non temporaire)
                isExistAndHasUserRight = ExistsTemplate3(pCS, pIdLstConsult, ref opIdLstTemplate, opIdA);
                if (isExistAndHasUserRight)
                {
                    //NB: Si le propriétaire du template est l'utilisateur lui même, pas de contrôle.
                    isExistAndHasUserRight = ((opIdA == SessionTools.Collaborator_IDA)
                        || HasUserRightForTemplate(pCS, opIdA, pIdLstConsult, opIdLstTemplate, RightsTypeEnum.VIEW));
                }
            }
            if (!isExistAndHasUserRight)
            {
                opIdA = SessionTools.User.idA;
                opIdLstTemplate = null;
            }
            #endregion
        }
        /// <summary>
        /// Retourne le nom par défaut (Select) d'un template destiné à un usage lors d'un accès via le bouton "..."
        /// </summary>
        /// <returns></returns>
        private static string GetSelectName()
        {
            //20090819 PL Suppression du GetString car les accents pose pb avec IE8 lorsqu'il passe dans l'URL
            //get { return Ressource.GetString("Select", true); }
            return "Select";
        }

        /// <summary>
        /// Retourne le nom par défaut (NewQueryName --> Nouveau en FR) d'un template 
        /// </summary>
        /// <returns></returns>
        public static string GetNewQueryName()
        {
            return Ressource.GetString("NewQueryName", true);
        }

        /// <summary>
        /// Renvoie la clé du template contenue dans la variable Session, pour l'instance courante de LSTCONSULT.
        /// </summary>
        /// <param name="pPage">Page appelante</param>
        /// <param name="pIdLstConsult">ID de la consultation</param>
        /// <param name="pGUID">GUID courant</param>
        /// <param name="opIdTemplate">REF: Id du template</param>
        /// <param name="opIdA">REF: ID du propriétaire du template</param>
        private static void ReadTemplateSession(Page pPage, string pIdLstConsult, string pGUID, ref string opIdLastTemplate, ref int opIdA)
        {
            try
            {
                if (pPage.Session[Session_TemplateName + pIdLstConsult + pGUID] != null)
                {
                    opIdLastTemplate = Convert.ToString(pPage.Session[Session_TemplateName + pIdLstConsult + pGUID]);

                    if (pPage.Session[Session_TemplateOwner + pIdLstConsult + pGUID] != null)
                    {
                        opIdA = Convert.ToInt32(pPage.Session[Session_TemplateOwner + pIdLstConsult + pGUID]);
                    }
                }
            }
            catch
            {
                opIdLastTemplate = string.Empty;
                opIdA = -1;
            }
        }

        /// <summary>         
        /// Sauvegarde la clé du template contenue dans la variable Session, pour l'instance courante de LSTCONSULT.
        /// </summary>
        /// <param name="pPage">Page: page appelante</param>
        /// <param name="pIdLstConsult">ID de la consultation</param>
        /// <param name="pIdTemplate">ID du template</param>
        /// <param name="pIdA">ID du propriétaire du template</param>
        /// <param name="pGUID">GUID courant</param>
        public static void WriteTemplateSession(Page pPage, string pIdLstConsult, string pIdTemplate, int pIdA, string pGUID)
        {
            pPage.Session[Session_TemplateName + pIdLstConsult + pGUID] = pIdTemplate;
            pPage.Session[Session_TemplateOwner + pIdLstConsult + pGUID] = pIdA.ToString();
        }

        #region Méthodes Cookie
        private static string CookieName(string pIdLstConsult)
        {
            return CookieNameWithoutUser(pIdLstConsult) + @"User=" + SessionTools.Collaborator_IDA + @";";
        }

        private static string CookieNameWithoutUser(string pIdLstConsult)
        {
            return @"Spheres Consult=" + pIdLstConsult + @";";
        }

        private static string CookieValue(string pIdTemplate, int pIdA)
        {
            return pIdTemplate + "|" + pIdA.ToString();
        }

        /// <summary>
        /// Renvoie la clé du template contenue dans le cookie pour l'instance actuelle de LSTCONSULT
        /// </summary>
        /// <param name="pPage">Page: page appelante</param>
        /// <param name="opIdTemplate">OUT string: Id du template </param>
        /// <param name="opIdA">OUT int: ID du propriétaire du template</param>
        private static void ReadTemplateCookie(string pIdLstConsult, out string opIdTemplate, out int opIdA)
        {
            string cookieValue = string.Empty;
            AspTools.ReadSQLCookie(CookieName(pIdLstConsult), out cookieValue);
            if (StrFunc.IsEmpty(cookieValue))
            {
                opIdTemplate = string.Empty;
                opIdA = -1;
            }
            else
            {
                string[] cookieValues;
                cookieValues = cookieValue.Split("|".ToCharArray(), 2);
                if (cookieValues.GetLength(0) == 2)
                {
                    opIdTemplate = cookieValues[0];
                    opIdA = Convert.ToInt32(cookieValues[1]);
                }
                else
                {
                    opIdTemplate = string.Empty;
                    opIdA = -1;
                }
            }
        }

        /// <summary>
        /// Ecrit la clé du template dans le cookie pour l'instance actuelle de LSTCONSULT
        /// </summary>
        /// <param name="pIdLstConsult">string: Id de la consultation</param>
        /// <param name="pIdTemplate">string: Id du template</param>
        /// <param name="pIdA">int: ID du propriétaire du template</param>
        private static void WriteTemplateCookie(string pIdLstConsult, string pIdTemplate, int pIdA)
        {
            AspTools.WriteSQLCookie(CookieName(pIdLstConsult), CookieValue(pIdTemplate, pIdA));
        }

        /// <summary>
        /// Mettre à jour la clé du template dans le cookie pour l'instance actuelle de LSTCONSULT
        /// </summary>
        /// <param name="pIdLstConsult">string: Id de la consultation</param>
        /// <param name="pIdTemplate">string: Id du template</param>
        /// <param name="pIdA">int: ID du propriétaire du template</param>
        public static void WriteTemplateCookieTemporary(string pIdLstConsult, string pIdTemplate, int pIdA)
        {
            AspTools.WriteSQLCookieTemporary(CookieNameWithoutUser(pIdLstConsult), CookieValue(pIdTemplate, pIdA), TEMPORARYPREFIX);
        }

        #endregion

        /// <summary>
        /// Vérification de l'existence d'un template sauvegardé ou modifié et non sauvegardé
        /// </summary>
        /// <param name="pCS">ConnectionString</param>
        /// <param name="pIdLstConsult">ID de la consultation</param>
        /// <param name="pIdLstTemplate">ID du template</param>
        /// <param name="pIdA">ID du propriétaire du template</param>
        /// <returns>boolean: true si le template existe</returns>        
        public static bool ExistsTemplate2(string pCS, string pIdLstConsult, ref string pIdLstTemplate, int pIdA)
        {
            bool ret = false;

            if (!IsTemporary(pIdLstTemplate))
            {
                //Tentative de recherche d'un template modifié et non sauvegardé
                ret = ExistsTemplate(pCS, pIdLstConsult, TEMPORARYPREFIX + pIdLstTemplate, pIdA);

                if (ret)
                    pIdLstTemplate = TEMPORARYPREFIX + pIdLstTemplate;
            }

            if (!ret)
            {
                ret = ExistsTemplate(pCS, pIdLstConsult, pIdLstTemplate, pIdA);
            }

            return ret;
        }

        /// <summary>
        /// Vérification de l'existence d'un template, sinon vérification de l'existence d'un template temporaire
        /// </summary>
        /// <param name="pCS">ConnectionString</param>
        /// <param name="pIdLstConsult">ID de la consultation</param>
        /// <param name="pIdLstTemplate">ID du template</param>
        /// <param name="pIdA">ID du propriétaire du template</param>
        /// <returns>boolean: true si le template existe</returns>        
        public static bool ExistsTemplate3(string pCS, string pIdLstConsult, ref string pIdLstTemplate, int pIdA)
        {
            bool ret = ExistsTemplate(pCS, pIdLstConsult, pIdLstTemplate, pIdA);

            if ((false == ret) && IsTemporary(pIdLstTemplate))
            {
                ret = ExistsTemplate(pCS, pIdLstConsult, RemoveTemporaryPrefix(pIdLstTemplate), pIdA);
                if (ret)
                    pIdLstTemplate = RemoveTemporaryPrefix(pIdLstTemplate);
            }

            return ret;
        }

        /// <summary>
        /// Vérification de l'existence d'un template
        /// </summary>
        /// <param name="pCS">ConnectionString</param>
        /// <param name="pIdLstConsult">ID de la consultation</param>
        /// <param name="pIdLstTemplate">ID du template</param>
        /// <param name="pIdA">ID du propriétaire du template</param>
        /// <returns>boolean: true si le template existe</returns>        
        public static bool ExistsTemplate(string pCS, string pIdLstConsult, string pIdLstTemplate, int pIdA)
        {
            bool ret = false;

            DataParameters dbParam = null;
            StrBuilder SQLSelect = new StrBuilder(SQLCst.SELECT);
            SQLSelect += "1" + Cst.CrLf;
            SQLSelect += SQLCst.FROM_DBO + Cst.OTCml_TBL.LSTTEMPLATE.ToString() + Cst.CrLf;
            SQLSelect += SQLCst.WHERE + GetSQLClause_PK_LSTTEMPLATE2(pCS, pIdLstConsult, pIdLstTemplate, pIdA, null, out dbParam);

            QueryParameters qryParameters = new QueryParameters(pCS, SQLSelect.ToString(), dbParam);
            object obj = DataHelper.ExecuteScalar(pCS, CommandType.Text, qryParameters.query, qryParameters.parameters.GetArrayDbParameter());
            ret = (null != obj);

            return ret;
        }

        /// <summary>
        /// Renvoie le template paramétré par défaut pour la consultation actuelle et pour un acteur ou l'acteur courant si non renseigné.
        /// <para>NB: Renvoie null si inexistant</para>
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pIdLstConsult"></param>
        /// <param name="pIdA"></param>
        /// <returns></returns>
        private static string SearchTemplate(string pCS, string pIdLstConsult, Nullable<int> pIdA)
        {
            string ret = null;

            string sql = SQLCst.SELECT + "lt.IDLSTTEMPLATE" + Cst.CrLf;
            sql += SQLCst.FROM_DBO + Cst.OTCml_TBL.LSTTEMPLATE.ToString() + " lt" + Cst.CrLf;
            sql += GetSQLInnerJoin_LST(Cst.OTCml_TBL.LSTTEMPLATEDEF);
            sql += SQLCst.WHERE + GetSQLClause_Consult(pIdLstConsult, "lt");
            sql += SQLCst.AND + "(lt.IDA=" + (pIdA == null ? SessionTools.User.idA : pIdA) + ")" + Cst.CrLf;

            if (pIdA != null)
            {
                //Warning: Lorsque l'on recherche un Template pour un autre acteur que l'acteur courant (ex. DEPARTMENT pour un GUEST),
                //         on prévilégie encore un template par défaut, mais pas seulement (Left Outer), et on priviligie également le nom 'Template'. 
                sql = sql.Replace(SQLCst.INNERJOIN_DBO, SQLCst.LEFTJOIN_DBO);

                sql += SQLCst.ORDERBY + "case when ltd.IDLSTCONSULT is null then 1 else 0 end,";
                sql += "case lt.IDLSTTEMPLATE when 'Template' then '_' else lt.IDLSTTEMPLATE end";
            }

            object obj = DataHelper.ExecuteScalar(pCS, CommandType.Text, sql.ToString());
            if (null != obj)
                ret = Convert.ToString(obj);

            return ret;
        }

        /// <summary>
        /// Retourne true si un utilisateur a un droit particulier sur un template
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pIdAOwner">D du propriétaire du template</param>
        /// <param name="pIdLstConsult"></param>
        /// <param name="pIdLstTemplate"></param>
        /// <param name="pRight">droit dont on veut verifier la validité</param>
        /// <returns></returns>
        public static bool HasUserRightForTemplate(string pCS, int pIdAOwner, string pIdLstConsult, string pIdLstTemplate, RightsTypeEnum pRight)
        {
            //Load Template
            LstTemplateData template = new LstTemplateData();
            template.Load(pCS, pIdLstConsult, pIdLstTemplate, pIdAOwner);

            return template.HasUserRight(pCS, SessionTools.User, pRight);
        }

        /// <summary>
        ///  Création et autoload d'un nouveau template temporaire par default
        /// </summary>
        /// <param name="pIdLstTemplate">string: valeur à utiliser pour l'affectation de l'ID</param>
        /// <param name="pCS"></param>
        /// <param name="pIdLstTemplate"></param>
        /// <param name="pIsLoadOnStart"></param>
        /// <param name="pTitle"></param>
        /// <param name="pIsNewTemplate"></param>
        /// <returns>ID du template créé</returns>
        public static string CreateNewTemporaryTemplate(string pCS, string pIdLstConsult, string pIdLstTemplate,
            bool pIsLoadOnStart, string pTitle)
        {
            return CreateNewTemporaryTemplate(pCS, pIdLstConsult, pIdLstTemplate,
                null, null, null,
                pIsLoadOnStart, pTitle);
        }

        /// <summary>
        ///  Création et autoload d'un nouveau template temporaire par default avec un filtre
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pIdLstTemplate">valeur à utiliser pour l'affectation de l'ID</param>
        /// <param name="pValueForFilter">valeur du filtre</param>
        /// <param name="pTableForFilter"></param>
        /// <param name="pColumnForFilter"></param>
        /// <param name="pIsLoadOnStart"></param>
        /// <param name="pTitle"></param>
        /// <param name="pIsNewTemplate"></param>
        /// <returns>ID du template créé</returns>
        public static string CreateNewTemporaryTemplate(string pCS, string pIdLstConsult, string pIdLstTemplate,
            string pValueForFilter, string pTableForFilter, string pColumnForFilter,
            bool pIsLoadOnStart, string pTitle)
        {
            //On créé un new LSTTEMPLATE que l'on renseigne par defaut
            LstTemplateData template = new LstTemplateData();

            template.IDLSTTEMPLATE = TEMPORARYPREFIX + pIdLstTemplate;
            template.IDLSTCONSULT = pIdLstConsult;
            template.DISPLAYNAME = pTitle;
            template.DESCRIPTION = string.Empty;
            template.CSSFILENAME = string.Empty;
            template.ROWBYPAGE = 0;
            template.RIGHTPUBLIC = RightsTypeEnum.NONE.ToString();
            template.RIGHTDESK = RightsTypeEnum.NONE.ToString();
            template.RIGHTDEPARTMENT = RightsTypeEnum.NONE.ToString();
            template.RIGHTENTITY = RightsTypeEnum.NONE.ToString();
            template.EXTLLINK = string.Empty;
            template.ISDEFAULT = false;
            template.IDA = SessionTools.Collaborator_IDA;
            template.ISLOADONSTART = pIsLoadOnStart;
            template.REFRESHINTERVAL = 0;
            //Insert SQL
            string idLstTemplate = CreateOverwrite(pCS, pIdLstConsult, template, SessionTools.Collaborator_IDA,
                                            pValueForFilter, pTableForFilter, pColumnForFilter);
            return idLstTemplate;
        }

        /// <summary>
        /// Fonction principale de création de template à l'aide de la classe LSTTEMPLATE
        /// </summary>
        /// <param name="pNewLSTTEMPLATE">LSTTEMPLATE: classe LSTTEMPLATE contenant les informations du template que l'on veut créer</param>
        /// <param name="pForcedIdA"></param>
        /// <returns>string: ID du template créé</returns>
        private static string CreateOverwrite(string pCS, string pIdLstConsult, LstTemplateData pTemplate, int pIdA,
            string pValueForFilter, string pTableForFilter, string pColumnForFilter)
        {
            //Si existe deja, on delete
            if (ExistsTemplate(pCS, pIdLstConsult, pTemplate.IDLSTTEMPLATE, pIdA))
            {
                //20091012 FI utilisation de la méthode static LstTemplate.Delete
                //Pourquoi faire un load pour delete ensuite ?
                //LstTemplate oldLSTTEMPLATE = new LstTemplate();
                //oldLSTTEMPLATE.Load(pCS, IdLstConsult, pLstTemplate.IDLSTTEMPLATE, forcedIdA);
                //oldLSTTEMPLATE.Delete(pCS, false);
                Delete(pCS, pIdLstConsult, pTemplate.IDLSTTEMPLATE, pIdA, false);
            }
            //Insert
            pTemplate.Insert(pCS, pIdA);
            //
            #region Insert into LSTWHERE
            //Rq. Utilisé pour insérer un critère dynamique sur l'ouverture d'un référentiel en tant qu'aide à la saisie (eg. bouton "..." sur la saisie des trades)
            if (pValueForFilter != null)//Warning: Ne pas tester IsEmpty() mais bien "null", car la donnée "empty" est possible
            {
                string sqlQuery = SQLCst.INSERT_INTO_DBO + Cst.OTCml_TBL.LSTWHERE.ToString();
                sqlQuery += "(IDA,IDLSTCONSULT,IDLSTTEMPLATE,IDLSTCOLUMN,TABLENAME,COLUMNNAME,ALIAS,POSITION,OPERATOR,LSTVALUE,LSTIDVALUE,ISENABLED,ISMANDATORY)" + Cst.CrLf;
                sqlQuery += " values (";
                sqlQuery += pIdA.ToString() + ",";
                sqlQuery += DataHelper.SQLString(pIdLstConsult) + ",";
                sqlQuery += DataHelper.SQLString(pTemplate.IDLSTTEMPLATE) + ",";
                if (StrFunc.IsFilled(pTableForFilter)) //=> on vient du référentiel
                {
                    sqlQuery += "null," + DataHelper.SQLString(pTableForFilter) + "," + DataHelper.SQLString(pColumnForFilter) + "," + DataHelper.SQLString(SQLCst.TBLMAIN) + ",";
                }
                else
                {
                    // PM 20091113 : Modif de ce qui a été fait en dur afin de gérer la consultation des Assets ETD
                    // Remplacement du nom de la table, de son alias et de nom de la colonne 'identifier' par des variables
                    string tableAlias = SQLCst.TBLMAIN;
                    string tableName = SQLCst.TBLMAIN;
                    string identifierName = "IDENTIFIER";
                    if ("*Select" == pTemplate.IDLSTTEMPLATE)
                    {
                        if ("ASSET_ETD_EXPANDED" == pTemplate.IDLSTCONSULT)
                        {
                            // FI 20180130 [23749] l'alias de la table est a_etd (avant c'était vw_a_etd) 
                            tableAlias = "a_etd";
                            tableName = "VW_ASSET_ETD_EXPANDED";
                        }
                        else if ("MATURITY" == pTemplate.IDLSTCONSULT)
                        {
                            tableAlias = "mat_etd";
                            tableName = "MATURITY";
                            identifierName = "MATURITYMONTHYEAR";
                            pColumnForFilter = "MATURITYMONTHYEAR";//GLOPL ??
                        }
                        else
                        {
                            //CC/PL 20120529 
                            tableAlias = "tasset";
                            tableName = "VW_TRADE_ASSET";
                        }
                    }
                    //20090626 PL TODO en dur pour l'instant...(IDLSTCOLUMN et "t")
                    // EG 20160308 Migration vs2013
                    //#warning 20090626 PL TODO
                    #region Get IDLSTCOLUMN
                    //string tmpSQLQuery = SQLCst.SELECT + "IDLSTCOLUMN" + Cst.CrLf;
                    //tmpSQLQuery += SQLCst.FROM_DBO + Cst.OTCml_TBL.LSTCOLUMN.ToString() + Cst.CrLf;
                    //tmpSQLQuery += SQLCst.WHERE + "TABLENAME='" + tableName + "' and COLUMNNAME='" + identifierName + "'";
                    ////IDataReader dr = DataHelper.ExecuteReader(source, CommandType.Text, tmpSQLQuery);
                    ////if (dr.Read())
                    ////    pIndexForFilter = Convert.ToInt32(dr.GetValue(0));
                    ////dr.Close();
                    ////dr.Dispose();
                    //object obj = DataHelper.ExecuteScalar(pCS, CommandType.Text, tmpSQLQuery);
                    //if (null != obj)
                    //    pIndexForFilter = Convert.ToInt32(obj);
                    #endregion Get IDLSTCOLUMN
                    sqlQuery += "-1," + DataHelper.SQLString(tableName) + "," + DataHelper.SQLString(identifierName) + "," + DataHelper.SQLString(tableAlias) + ",";//GLOPL??
                }
                sqlQuery += "0,";
                //FI 20111202 Mise en place de l'opérateur Contains à la place de like
                if (pValueForFilter == "%")
                    sqlQuery += DataHelper.SQLString(SQLCst.LIKE.Trim()) + ",";
                else
                    sqlQuery += DataHelper.SQLString("Contains") + ",";

                sqlQuery += "@LSTVALUE,"; //Note: Utilisation par sécurité d'un parameter car donnée saisie par le user
                sqlQuery += SQLCst.NULL + ",";
                sqlQuery += "1,";
                sqlQuery += "0)";

                IDbDataParameter paramLSTVALUE = new EFSParameter(pCS, "LSTVALUE", DbType.AnsiString, SQLCst.UT_NOTE_LEN).DataParameter;
                paramLSTVALUE.Value = pValueForFilter;

                DataHelper.ExecuteNonQuery(pCS, CommandType.Text, sqlQuery, paramLSTVALUE);
            }
            #endregion Insert into LSTWHERE

            return pTemplate.IDLSTTEMPLATE;
        }

        /// <summary>
        /// Mise à jour du ISDEFAULT de la classe LSTTEMPLATE
        /// </summary>
        public static void CleanDefault(string pCS, string pIdLstConsult)
        {
            StrBuilder sqlSelect = new StrBuilder(SQLCst.SELECT);
            sqlSelect += "lt.IDLSTTEMPLATE" + Cst.CrLf;
            sqlSelect += SQLCst.FROM_DBO + Cst.OTCml_TBL.LSTTEMPLATE.ToString() + " lt" + Cst.CrLf;
            sqlSelect += GetSQLInnerJoin_LST(Cst.OTCml_TBL.LSTTEMPLATEDEF);
            sqlSelect += SQLCst.WHERE + "(lt.IDA=" + SessionTools.Collaborator_IDA.ToString() + ")" + Cst.CrLf;
            sqlSelect += SQLCst.AND + "(lt.IDLSTCONSULT=" + DataHelper.SQLString(pIdLstConsult) + ")";//GLOP-PARAMETER

            object obj = DataHelper.ExecuteScalar(pCS, CommandType.Text, sqlSelect.ToString());
            if (null != obj)
            {
                string name = Convert.ToString(obj);

                if (IsTemporary(name))
                {
                    string name_WithoutPrefix = RemoveTemporaryPrefix(name);

                    LstTemplateData defaultTemplate = new LstTemplateData();
                    if (defaultTemplate.Load(pCS, pIdLstConsult, name_WithoutPrefix, SessionTools.Collaborator_IDA))
                    {
                        defaultTemplate.ISDEFAULT = true;
                        defaultTemplate.Update(pCS);
                    }
                    else
                    {
                        defaultTemplate.Load(pCS, pIdLstConsult, name, SessionTools.Collaborator_IDA);
                        defaultTemplate.ISDEFAULT = false;
                        defaultTemplate.Update(pCS);
                    }
                }
            }
        }
        private static string GetSQLInnerJoin_LST(Cst.OTCml_TBL pTbl)
        {
            string alias = string.Empty;
            switch (pTbl)
            {
                case Cst.OTCml_TBL.LSTTEMPLATEDEF:
                    alias = "ltd";
                    break;
                case Cst.OTCml_TBL.LSTSELECT:
                    alias = "ls";
                    break;
                case Cst.OTCml_TBL.LSTWHERE:
                    alias = "lw";
                    break;
                case Cst.OTCml_TBL.LSTORDERBY:
                    alias = "lo";
                    break;
            }
            string sql = SQLCst.INNERJOIN_DBO + pTbl.ToString() + " " + alias;
            sql += " on ((" + alias + ".IDLSTTEMPLATE=lt.IDLSTTEMPLATE) and (" + alias + ".IDLSTCONSULT=lt.IDLSTCONSULT) and (" + alias + ".IDA=lt.IDA))" + Cst.CrLf;

            return sql;
        }
        /// <summary>
        /// Retourne la clause where applicable à une consultation 
        /// </summary>
        /// <param name="pAlias">Alias de table qui contient la colonne IDLSTCONSULT</param>
        /// <returns>Clause where</returns>
        public static string GetSQLClause_Consult(string pIdLstConsult, string pAlias)
        {
            string alias = (String.IsNullOrEmpty(pAlias) ? string.Empty : pAlias + ".");
            //PL 20110923 Add Trim() pour alléger la lecture du SQL
            return "(" + alias + "IDLSTCONSULT=" + DataHelper.SQLString(pIdLstConsult.Trim()) + ")" + Cst.CrLf;
        }

        /// <summary>
        /// Défini si une consultation est de type Referential (descriptif via fichier XML)
        /// </summary>
        /// <param name="pIdLstConsult">string: ID de la consultation</param>
        /// <returns>bool: true si la consultation est de type referential sinon false</returns>
        public static bool IsReferential(string pIdLstConsult)
        {
            return (pIdLstConsult.StartsWith(PrefixForReferential));
        }
        #region public IsTemporary
        /// <summary>
        /// Retourne TRUE s'il s'agit d'un template de type "temporaire".
        /// <para>NB: Nom de template préfixé d'une étoile (*)</para>
        /// </summary>
        /// <param name="pIdLstTemplate">string: ID du template</param>
        /// <returns>bool</returns>
        public static bool IsTemporary(string pIdLstTemplate)
        {
            return ((pIdLstTemplate != null) && pIdLstTemplate.StartsWith(TEMPORARYPREFIX));
        }
        #endregion IsTemporary
        //
        #region public RemoveTemporaryPrefix
        /// <summary>
        /// Supprime le prefix "*" du nom du template passé en paramètre
        /// </summary>
        /// <param name="pIdLstTemplate">string: ID du template</param>
        /// <returns>bool</returns>
        public static string RemoveTemporaryPrefix(string pIdLstTemplate)
        {
            string ret = pIdLstTemplate;
            if (IsTemporary(ret))
                ret = ret.Remove(0, TEMPORARYPREFIX.Length);
            return ret;
        }
        #endregion

        #region public Delete
        /// <summary>
        /// Delete le template et ses childs associés
        /// </summary>
        /// <param name="pIDLSTTEMPLATE">string: ID du template</param>
        /// <param name="pIdA">int: ID du propriétaire du template</param>
        public static void Delete(string pCS, string pIdLstConsult, string pIdLstTemplate, int pIdA, bool pIsWithTemporary)
        {
            DataParameters dbParam = null;
            StrBuilder SQLDelete = new StrBuilder();
            SQLDelete += SQLCst.DELETE_DBO + Cst.OTCml_TBL.LSTTEMPLATE.ToString() + Cst.CrLf;
            SQLDelete += SQLCst.WHERE + GetSQLClause_PK_LSTTEMPLATE2(pCS, pIdLstConsult, pIdLstTemplate, pIdA, null, out dbParam);

            QueryParameters qryParameters = new QueryParameters(pCS, SQLDelete.ToString(), dbParam);
            DataHelper.ExecuteNonQuery(pCS, CommandType.Text, qryParameters.query, qryParameters.parameters.GetArrayDbParameter());

            if (pIsWithTemporary & !pIdLstTemplate.StartsWith(TEMPORARYPREFIX))
            {
                //Traitements du "temporary"
                SQLDelete = new StrBuilder();
                SQLDelete += SQLCst.DELETE_DBO + Cst.OTCml_TBL.LSTTEMPLATE.ToString() + Cst.CrLf;
                SQLDelete += SQLCst.WHERE + GetSQLClause_PK_LSTTEMPLATE2(pCS, pIdLstConsult, TEMPORARYPREFIX + pIdLstTemplate, pIdA, null, out dbParam);
                qryParameters = new QueryParameters(pCS, SQLDelete.ToString(), dbParam);
                DataHelper.ExecuteNonQuery(pCS, CommandType.Text, qryParameters.query, qryParameters.parameters.GetArrayDbParameter());
            }

            #region Delete des enfants associés
            Cst.OTCml_TBL[] childTableName = { Cst.OTCml_TBL.LSTSELECT, Cst.OTCml_TBL.LSTORDERBY, Cst.OTCml_TBL.LSTWHERE };
            for (int i = 0; i < childTableName.Length; i++)
            {
                DeleteChild(pCS, childTableName[i], pIdLstConsult, pIdLstTemplate, pIdA, pIsWithTemporary);
            }
            #endregion
        }
        #endregion

        #region DeleteChild
        /// <summary> 
        /// Supprime pour le template correspondant à la clé passée en arg, toutes les lignes de la table enfant passée en arg. 
        /// </summary>
        /// <param name="pTableName">string: nom de la table enfant</param>
        /// <param name="pIdLstTemplate">string: ID du template</param>
        /// <param name="pIdA">int: ID du propriétaire du template</param>
        public static void DeleteChild(string pCS, Cst.OTCml_TBL pTableName, string pIdLstConsult, string pIdLstTemplate, int pIdA, bool pIsWithTemporary)
        {
            DataParameters dbParam = null;
            StrBuilder SQLDelete = new StrBuilder();
            SQLDelete += SQLCst.DELETE_DBO + pTableName.ToString() + Cst.CrLf;
            SQLDelete += SQLCst.WHERE + GetSQLClause_PK_LSTTEMPLATE2(pCS, pIdLstConsult, pIdLstTemplate, pIdA, null, out dbParam);

            QueryParameters qryParameters = new QueryParameters(pCS, SQLDelete.ToString(), dbParam);
            DataHelper.ExecuteNonQuery(pCS, CommandType.Text, qryParameters.query, qryParameters.parameters.GetArrayDbParameter());

            if (pIsWithTemporary && !pIdLstTemplate.StartsWith(TEMPORARYPREFIX))
            {
                //Traitements des "temporaries"
                dbParam = null;
                SQLDelete = new StrBuilder();
                SQLDelete += SQLCst.DELETE_DBO + pTableName.ToString() + Cst.CrLf;
                SQLDelete += SQLCst.WHERE + GetSQLClause_PK_LSTTEMPLATE2(pCS, pIdLstConsult, TEMPORARYPREFIX + pIdLstTemplate, pIdA, null, out dbParam);

                qryParameters = new QueryParameters(pCS, SQLDelete.ToString(), dbParam);
                DataHelper.ExecuteNonQuery(pCS, CommandType.Text, qryParameters.query, qryParameters.parameters.GetArrayDbParameter());
            }
        }
        #endregion

        #region GetSQLClause_PK_LSTTEMPLATE2
        /// <summary>
        /// Obtient la clause where pour identifier un template par rapport à sa clé (IDLSTTEMPLATE et IDA)
        /// </summary>
        /// <param name="pIDLSTTEMPLATE">ID du template</param>
        /// <param name="pIDA">ID du propriétaire du template</param>
        /// <param name="pDBAlias">alias pour prefixer les colonnes</param>
        /// <param name="opDataParameters">OUT: Data Parameters</param>
        /// <returns>clause where</returns>
        public static string GetSQLClause_PK_LSTTEMPLATE2(string pCS, string pIdLstConsult, string pIDLstTemplate, int pIdA, string pDbAlias,
            out DataParameters opDataParameters)
        {
            string alias = (String.IsNullOrEmpty(pDbAlias) ? string.Empty : pDbAlias + ".");

            string SQLWhere = "(";
            SQLWhere += alias + "IDLSTTEMPLATE=@IDLSTTEMPLATE";
            SQLWhere += SQLCst.AND + alias + "IDLSTCONSULT=@IDLSTCONSULT";
            SQLWhere += SQLCst.AND + alias + "IDA=@IDA";
            SQLWhere += ")" + Cst.CrLf;

            opDataParameters = new DataParameters();
            opDataParameters.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.IDLSTTEMPLATE), pIDLstTemplate);
            opDataParameters.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.IDLSTCONSULT), pIdLstConsult);
            opDataParameters.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.IDA), pIdA);

            return SQLWhere;
        }
        #endregion

        /// <summary>
        /// Consultion sur la base d'un fichier XML
        /// Identification d'une colonne depuis la variable Session
        /// </summary>
        /// <param name="pPage"></param>
        /// <param name="pKeylstColumn"></param>
        /// <param name="pIndexColumn"></param>
        /// <returns></returns>
        //PL 20121129 Add pAlias
        public static int GetIndexInSession_LstColumn(Page pPage, string pKeylstColumn, int pIdLstColumn, string pAlias, string pTableName, string pColumnName)
        {
            int ret = -1;

            ArrayList[] alSelect = (ArrayList[])pPage.Session[pKeylstColumn];

            //finding position of column in the array
            if (ArrFunc.IsFilled(alSelect))
            {
                if (StrFunc.IsFilled(pTableName))
                {
                    string alias = null;
                    for (int i = 0; i < alSelect[0].Count; i++)
                    {
                        //PL 20121129 Bidouille en attendant mieux... On récupère l'ALIAS dans Datacol en 2ème position.
                        if (!String.IsNullOrEmpty(pAlias))
                        {
                            string[] dataCols_Item = Convert.ToString(alSelect[1][i]).Split(',');
                            alias = dataCols_Item[1];
                        }

                        if ((String.IsNullOrEmpty(pAlias) || alias == pAlias)
                            && (Convert.ToString(alSelect[3][i]) == pTableName)
                            && (Convert.ToString(alSelect[4][i]) == pColumnName))
                        {
                            ret = i;
                            break;
                        }
                    }
                }
                else //Compatibilié asc: idLstColumn maintenu pour compatibilié ascendante (à supprimer en v2.7)
                {
                    for (int i = 0; i < alSelect[0].Count; i++)
                    {
                        if (Convert.ToInt32(alSelect[2][i]) == pIdLstColumn)
                        {
                            ret = i;
                            break;
                        }
                    }
                }
            }
            return ret;
        }

        /// <summary>
        /// Consultion sur la base d'un fichier XML
        /// Sauvegarde dans une variable Session de la liste des colonnes (,type, ...) disponibles pour une consultion
        /// </summary>
        /// <param name="pPage">Page appelante</param>
        /// <param name="pIdLstConsult">ID de la consultation</param>
        /// <param name="pIdLstTemplate">ID du template</param>
        /// <param name="pIdA">ID du proprietaire du template</param>
        /// <param name="pReferential">Classe referential</param>
        /// <returns>Nom de de la variable Session dasn laquelle sont sauvegardés les données</returns>
        public static string SaveInSession_LstColumn(PageBase pPage, string pIdLstConsult, string pIdLstTemplate, int pIdA, Referential pReferential)
        {
            string previousRessource = string.Empty;

            string guid = string.Empty;
            HiddenField hiddenField = (HiddenField)pPage.MasterPage_ContentPlaceHolder.FindControl("__GUID");
            if (null != hiddenField)
                guid = hiddenField.Value;

            ArrayList[] alSelect = new ArrayList[7];
            alSelect[0] = new ArrayList();
            alSelect[1] = new ArrayList();
            alSelect[2] = new ArrayList();
            alSelect[3] = new ArrayList();
            alSelect[4] = new ArrayList();
            alSelect[5] = new ArrayList();
            alSelect[6] = new ArrayList();

            //PL 20120521 WARNING ligne suivante modifiée en -1
            //int indice = 0;
            int indice = -1;
            for (int index = 0; index < pReferential.Column.GetLength(0); index++)
            {
                ReferentialColumn rrc = pReferential.Column[index];
                if (
                    (rrc.ColumnName != Cst.OTCml_COL.ROWVERSION.ToString())
                    && (rrc.ColumnName != Cst.OTCml_COL.ROWATTRIBUT.ToString())
                    && !TypeData.IsTypeImage(rrc.DataType.value)
                    && ((rrc.IsHideSpecified && !rrc.IsHide) || (rrc.IsHideInDataGridSpecified && !rrc.IsHideInDataGrid) || (rrc.IsHideInCriteriaSpecified && !rrc.IsHideInCriteria))
                    && (rrc.Ressource != "Empty")
                    && (!rrc.IsRole) && (!rrc.IsItem)
                    )
                {
                    string displayCol = string.Empty;
                    string dataCol = string.Empty;
                    if (StrFunc.IsEmpty(rrc.Ressource))
                    {
                        displayCol = previousRessource;
                        //PL 20160218 Add SPACE
                        displayCol += " (" + Ressource.GetString(rrc.ColumnName) + ")";
                    }
                    else
                    {
                        //CC/PL 20130521
                        //displayCol = Ressource.GetMulti(rrc.Ressource);
                        displayCol = Ressource.GetMulti(rrc.Ressource, 2, 0);
                        previousRessource = displayCol;
                        if ((index + 1 < pReferential.Column.GetLength(0)) && (StrFunc.IsEmpty(pReferential.Column[index + 1].Ressource)))
                        {
                            //PL 20160218 Add SPACE
                            displayCol += " (" + Ressource.GetString(rrc.ColumnName) + ")";
                        }
                    }
                    dataCol = indice.ToString() + "," + rrc.AliasTableName + "," + rrc.DataType + "," + pReferential.TableName + "," + rrc.ColumnName;
                    //
                    OpenReferentialArguments arg = new OpenReferentialArguments();
                    //
                    arg.isHideInCriteriaSpecified = true;
                    arg.isHideInCriteria = (rrc.IsHideInCriteriaSpecified && rrc.IsHideInCriteria);
                    //
                    string relationTableName = string.Empty;
                    string relationColumnRelation = string.Empty;
                    string relationColumnSelect = string.Empty;
                    string relationListType = Cst.ListType.Repository.ToString();
                    if (ArrFunc.IsFilled(rrc.Relation))
                    {
                        if (ArrFunc.IsFilled(rrc.Relation[0].ColumnRelation) && ArrFunc.IsFilled(rrc.Relation[0].ColumnSelect))
                        {
                            relationTableName = rrc.Relation[0].TableName;
                            relationColumnRelation = rrc.Relation[0].ColumnRelation[0].ColumnName;
                            relationColumnSelect = rrc.Relation[0].ColumnSelect[0].ColumnName;
                            relationListType = rrc.Relation[0].ListType;
                            if (StrFunc.IsFilled(rrc.Relation[0].ColumnSelect[0].Ressource))
                            {
                                displayCol = Ressource.GetMulti(rrc.Relation[0].ColumnSelect[0].Ressource, 2, 0);
                            }
                            arg.referential = relationTableName;
                            arg.sqlColumn = relationColumnSelect;
                            arg.listType = (Cst.ListType)Enum.Parse(typeof(Cst.ListType), relationListType, true);
                        }
                        else if ((rrc.Relation[0].DDLType != null) && StrFunc.IsFilled((rrc.Relation[0].DDLType.Value)))
                        {
                            string ddlType = (rrc.Relation[0].DDLType.Value);
                            if (Cst.IsDDLTypeBusinessCenter(ddlType) || Cst.IsDDLTypeBusinessCenterId(ddlType))
                            {
                                arg.referential = Cst.OTCml_TBL.BUSINESSCENTER.ToString();
                                arg.sqlColumn = OTCmlHelper.GetColunmID(arg.referential);
                            }
                            else if (Cst.IsDDLAmountType(ddlType))
                            {
                                arg.referential = Cst.OTCml_TBL.VW_AMOUNTTYPE.ToString();
                                arg.sqlColumn = "VALUE";
                            }
                            else if (Cst.IsDDLTypeCountry(ddlType) || Cst.IsDDLTypeCountry_Country(ddlType))
                            {
                                arg.referential = Cst.OTCml_TBL.COUNTRY.ToString();
                                arg.sqlColumn = OTCmlHelper.GetColunmID(arg.referential);
                            }
                            else if (Cst.IsDDLTypeCSS(ddlType))
                            {
                                arg.referential = Cst.OTCml_TBL.ACTOR.ToString();
                                arg.sqlColumn = OTCmlHelper.GetColunmID(arg.referential);
                                arg.condition = ddlType;
                            }
                            else if (Cst.IsDDLTypeCurrency(ddlType))
                            {
                                arg.referential = Cst.OTCml_TBL.CURRENCY.ToString();
                                arg.sqlColumn = OTCmlHelper.GetColunmID(arg.referential);
                            }

                            else if (Cst.IsDDLTypeEnum(ddlType))
                            {
                                LoadEnumArguments loadEnum = LoadEnumArguments.GetArguments(ddlType, false);
                                arg.referential = Cst.OTCml_TBL.ENUM.ToString();
                                arg.fk = loadEnum.code;
                                arg.sqlColumn = "VALUE";
                            }
                            else if (Cst.IsDDLTypeMarketId(ddlType) || Cst.IsDDLTypeMarketETD(ddlType))
                            {
                                arg.referential = Cst.OTCml_TBL.MARKET.ToString();
                                arg.sqlColumn = OTCmlHelper.GetColunmID(arg.referential);
                            }
                            else if (Cst.IsDDLTypeClearingHouse(ddlType))
                            {
                                arg.referential = Cst.OTCml_TBL.ACTOR.ToString();
                                arg.sqlColumn = OTCmlHelper.GetColunmID(arg.referential);
                            }
                            else
                            {
                                //Nickel on continue
                            }
                        }
                    }

                    dataCol += "," + relationTableName;
                    dataCol += "," + relationColumnRelation;
                    dataCol += "," + relationColumnSelect;
                    dataCol += "," + relationListType;

                    alSelect[0].Add(displayCol);
                    alSelect[1].Add(dataCol);
                    alSelect[2].Add(indice.ToString());
                    alSelect[3].Add(pReferential.TableName);
                    alSelect[4].Add(rrc.ColumnName);
                    alSelect[5].Add(rrc.DataType);
                    alSelect[6].Add(arg);
                }
            }

            string name = pIdLstConsult + "-" + pIdLstTemplate + "-" + pIdA.ToString() + "-" + guid + "-" + "ListOfColumns2";
            pPage.Session[name] = alSelect;
            return name;
        }
        public static string SaveInSession_LstColumn2(PageBase pPage, string pIdLstConsult, string pIdLstTemplate, int pIdA, Referential pReferential)
        {
            string previousRessource = string.Empty;

            string guid = string.Empty;
            HiddenField hiddenField = (HiddenField)pPage.MasterPage_ContentPlaceHolder.FindControl("__GUID");
            if (null != hiddenField)
                guid = hiddenField.Value;

            List<ReferentialColumn> columns = pReferential.Column.ToList();
            List<ListFilterTools.ListColumn> lstColumns = new List<ListFilterTools.ListColumn>();
            columns.ForEach(column =>
            {
                bool isNoRowVersion = (column.ColumnName != Cst.OTCml_COL.ROWVERSION.ToString());
                bool isNoRowAttribut = (column.ColumnName != Cst.OTCml_COL.ROWATTRIBUT.ToString());
                bool isNoImage = (false == TypeData.IsTypeImage(column.DataType.value));


                string displayName = string.Empty;
                if (isNoRowVersion && isNoRowAttribut && isNoImage)
                {
                    ListFilterTools.ListColumn item = new ListFilterTools.ListColumn();
                    item.alias = "";
                    item.aliasDisplayName = "";
                    item.columnName = column.ColumnName;
                    item.dataType = column.DataType.value;

                    if (StrFunc.IsEmpty(column.Ressource))
                    {
                        displayName = previousRessource;
                        displayName += " (" + Ressource.GetString(column.ColumnName) + ")";
                    }
                    else
                    {
                        displayName = Ressource.GetMulti(column.Ressource, 2, 0);
                        previousRessource = displayName;

                        int index = columns.IndexOf(column);
                        //if ((-1 < index) && (index + 1 < columns.Count()))
                        if (index + 1 < columns.Count())
                        {
                            ReferentialColumn nextColumn = columns.ElementAt(index + 1);
                            if ((null != nextColumn) && StrFunc.IsEmpty(nextColumn.Ressource))
                                displayName += " (" + Ressource.GetString(nextColumn.ColumnName) + ")";
                        }
                    }
                    item.displayName = displayName;

                    item.group = "ALL";
                    item.isHideCriteria = column.IsHideInCriteriaSpecified && column.IsHideInCriteria;
                    item.isMandatory = column.IsMandatorySpecified && column.IsMandatory;
                    item.position = 0;
                    item.tableName = pReferential.TableName;

                    lstColumns.Add(item);


                    //&& ((column.IsHideSpecified && !column.IsHide) || (column.IsHideInDataGridSpecified && !column.IsHideInDataGrid) || (column.IsHideInCriteriaSpecified && !column.IsHideInCriteria))
                    //&& (column.Ressource != "Empty")
                    //&& (!column.IsRole) && (!column.IsItem)
                }
            });

            string name = pIdLstConsult + "-" + pIdLstTemplate + "-" + pIdA.ToString() + "-" + guid + "-" + "ListOfColumns";
            pPage.Session[name] = lstColumns;
            return name;
        }

        public void TEMP_PASCAL(string pCS, string pIdLstConsult, string pIdLstTemplate, int pIdA)
        {
            string idMenu = null;
            LstConsultData consult = new LstConsultData(pCS, pIdLstConsult, idMenu);
            consult.LoadTemplate(pCS, pIdLstTemplate, pIdA, false);

            bool isConsultWithDynamicArgs = false;
            consult.LoadLstDatatable(pCS, isConsultWithDynamicArgs);

            string[] parameters = null;
            string condApp = string.Empty;

            Referential referential = consult.GetReferentials(pCS, null, parameters, null).Items[0];
            referential.Initialize(true, null, condApp, parameters, null);
        }
    }
}
