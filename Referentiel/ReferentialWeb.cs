using System;
using System.Data;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Collections;
using EFS.ACommon;
using EFS.Common;
using EFS.Common.Web;
using EFS.Rights;
using EFS.ApplicationBlocks.Data;

namespace EFS.Referential
{
    public class ReferentialInformation
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string Type;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool TypeSpecified;

        [System.Xml.Serialization.XmlElementAttribute("ToolTipMessage", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public ReferentialInformationToolTipMessage ToolTipMessage;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool ToolTipMessageSpecified;

        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string LabelMessage;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool LabelMessageSpecified;

        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string PopupMessage;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool PopupMessageSpecified;

        //PL 20110324
        [System.Xml.Serialization.XmlElementAttribute("URL", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public ReferentialInformationURL URL;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool URLSpecified;
        #endregion Members

        #region Methods
        #region GetWebCtrlInformation
        // EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        // EG 20210304 [XXXXX] Relooking Referentiels  
        // EG 20210304 [XXXXX] Relooking Referentiels - Pas de 100% sur Tablecell Toolitip
        public TableRow GetWebCtrlInformation(int pColspan, string pID, string pTitle)
        {
            TableRow tr = new TableRow();
            WCToolTipPanel btnMessage = new WCToolTipPanel();
            Label lblMessage = null;

            if (GetWebCtrlInformation(pID, pTitle, ref btnMessage, ref lblMessage))
            {
                Panel pnlInfo = new Panel() {CssClass = "panel-" + lblMessage.CssClass};
                btnMessage.Style.Add(HtmlTextWriterStyle.MarginRight, "3px");
                btnMessage.CssClass = btnMessage.CssClass.Replace("-circle", string.Empty);
                Panel pnlMessage = new Panel();
                pnlMessage.Controls.Add(lblMessage);
                pnlInfo.Controls.Add(btnMessage);
                pnlInfo.Controls.Add(pnlMessage);
                TableCell td = new TableCell();
                td.Controls.Add(pnlInfo);
                td.Controls.Add(new LiteralControl(Cst.HTMLBreakLine));
                td.ColumnSpan = pColspan;
                tr.Cells.Add(td);
            }
            return tr;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pID"></param>
        /// <param name="pTitle"></param>
        /// <param name="opBtnMessage"></param>
        /// <param name="opLblMessage"></param>
        /// <returns></returns>
        /// FI 20160804 [Migration TFS] Modify
        // EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        /// EG 20201002 [XXXXX] Gestion des ouvertures via window.open (nouveau mode : opentab : mode par défaut)
        /// EG 20201005 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc) Homogenéisation de la couleur des tooltips en fonction du type de message
        // EG 20210304 [XXXXX] Relooking Referentiels
        // EG 20210304 [XXXXX] Relooking Referentiels - Si pas de LabelMessage alors le contrôle est null
        // EG 20220602 [XXXXX] Un lien vers la liste d'un enum (si la colonne est concernée) est intégré dans le tooltip
        public bool GetWebCtrlInformation(string pID, string pTitle, ref WCToolTipPanel opBtnMessage, ref Label opLblMessage)
        {
            bool ret = false;
            string labelMessage = string.Empty;
            if (ToolTipMessageSpecified || LabelMessageSpecified || (this.URLSpecified && (this.URL.Value != "None")))
            {
                ret = true;
                #region Type Message
                string cssClass = EFSCssClass.Msg_Information;
                if (TypeSpecified)
                {
                    switch (Type)
                    {
                        case Cst.TypeInformationMessage.Warning:
                            cssClass = EFSCssClass.Msg_Warning;
                            break;
                        case Cst.TypeInformationMessage.Alert:
                            cssClass = EFSCssClass.Msg_Alert;
                            break;
                        case Cst.TypeInformationMessage.Success:
                            cssClass = EFSCssClass.Msg_Success;
                            break;
                        case Cst.TypeInformationMessage.Information:
                        default:
                            cssClass = EFSCssClass.Msg_Information;
                            break;
                    }
                }
                #endregion Type Message

                WCToolTipPanel btnMessage = new WCToolTipPanel
                {
                    ID = ControlID.GetID(pID + "MSG", null, Cst.IMG)
                };
                if (URLSpecified && URL.enumSpecified)
                    btnMessage.Pty.TooltipContent = Ressource.GetString2("SeeEnumList", URL.@enum);

                // EG 20210304 [XXXXX] Relooking Referentiels - Si pas de LabelMessage alors le contrôle est null
                Label lblMessage = null;
                if (LabelMessageSpecified)
                {
                    lblMessage = new Label();
                    labelMessage = Ressource.GetString(LabelMessage, true);
                    lblMessage.ID = ControlID.GetID(pID + "MSG", null, Cst.LBL);
                    lblMessage.CssClass = cssClass;
                    lblMessage.Text = labelMessage;
                    btnMessage.CssClass = GetAwesomeClassForTooltipInfo(Type);
                }

                string url = string.Empty;
                // EG 20220602 [XXXXX] Construction du lien vers la liste des éléments de l'enum
                if (this.URLSpecified && (this.URL.Value != "None"))
                    url = String.Format("Referential.aspx?T=Repository&O=ENUMS&M=2&PK=CODE&PKV={0}&F=frmConsult&IDMenu={1}", this.URL.@enum, IdMenu.GetIdMenu(IdMenu.Menu.Enums));

                if (ToolTipMessageSpecified)
                {
                    string tooltipMessage = Ressource.GetString(ToolTipMessage.Value, true);
                    if (StrFunc.IsFilled(labelMessage) && tooltipMessage.StartsWith(Cst.CrLf))
                        tooltipMessage = labelMessage + tooltipMessage;

                    // EG 20220602 [XXXXX] Le lien vers la liste d'un enum est intégré au tooltip
                    if (StrFunc.IsFilled(url))
                        tooltipMessage += Cst.HTMLBreakLine2 + $"<a href='{url}' target='_blank' class='qtip-enum-link' tabindex='-1'><i class='fa-icon fas fa-list'></i>" + Ressource.GetString2("SeeEnumList", URL.@enum) + @"</a>";

                    if (ToolTipMessage.titleSpecified)
                        btnMessage.Pty.TooltipTitle = Ressource.GetString(ToolTipMessage.title, true);
                    else if (StrFunc.IsFilled(pTitle))
                        btnMessage.Pty.TooltipTitle = Ressource.GetString(pTitle, true);

                    btnMessage.Pty.TooltipContent = tooltipMessage.Replace(Cst.CrLf, Cst.HTMLBreakLine);
                    btnMessage.CssClass = GetAwesomeClassForTooltipInfo(Type);
                    btnMessage.Pty.TooltipStyle = GetTooltipStyle(Type, StrFunc.IsFilled(url));
                }
                else if (StrFunc.IsFilled(url))
                {
                    // EG 20220602 [XXXXX] Enum sans tooltip
                    btnMessage.Attributes.Add("onclick", JavaScript.GetWindowOpen(url, Cst.WindowOpenStyle.EfsML_ListReferential));
                }

                opBtnMessage = btnMessage;
                opLblMessage = lblMessage;
            }
            return ret;
        }
        #endregion GetWebCtrlInformation
        #region GetAwesomeClassForTooltipInfo
        // EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        private static string GetAwesomeClassForTooltipInfo(string pType)
        {
            string color;
            switch (pType.ToLower())
            {
                case Cst.TypeInformationMessage.Warning:
                    color = "orange";
                    break;
                case Cst.TypeInformationMessage.Alert:
                    color = "red";
                    break;
                case Cst.TypeInformationMessage.Success:
                    color = "green";
                    break;
                case Cst.TypeInformationMessage.Information:
                default:
                    color = "blue";
                    break;
            }
            return String.Format("fa-icon fas fa-info-circle {0}", color);
        }
        #endregion GetAwesomeClassForTooltipInfo
        /// <summary>
        /// Define style of Qtip
        /// </summary>
        /// <param name="pType"></param>
        /// <param name="pIsLinkToEnum"></param>
        /// <returns></returns>
        private string GetTooltipStyle(string pType, bool pIsLinkToEnum)
        {
            string _style = "blue";
            switch (pType.ToLower())
            {
                case Cst.TypeInformationMessage.Warning:
                    _style = "orange";
                    break;
                case Cst.TypeInformationMessage.Alert:
                    _style = "red";
                    break;
                case Cst.TypeInformationMessage.Success:
                    _style = "green";
                    break;
                case Cst.TypeInformationMessage.Information:
                default:
                    _style = pIsLinkToEnum ? "brown" : "blue";
                    break;
            }
            return $"qtip-{_style}";
        }
        #endregion Methods
    }

    /// <summary>
    /// Description résumée de ReferentialWeb.
    /// </summary>
    public sealed class ReferentialWeb
    {
        #region constructor
        public ReferentialWeb() { }
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
        /// <param name="pLstConsult">Représente la consultation</param>
        /// <param name="pIsCeateTemporaryTemplate">Si true, création systématique d'un nouveau template temporaire lors postback de la page (le template se nomme *{TemplateName} (Rq {TemplateName} est disponible dans la variable session)).</param>
        /// <param name="pIsNewTemporaryTemplate">Si true, création systématique d'un nouveau template temporaire lors de la 1ère publication (le template se nomme "*nouveau" ou "*select")</param>
        /// <param name="pIsExistFilter">paramètre utilisé uniquement si pIsNewTemporaryTemplate. Permet de de créer "*nouveau"  </param>
        /// <param name="opIdLstTemplate">Nom du template</param>
        /// <param name="opIdA">ID du propriétaire du template</param>
        /// FI 20200602 [25370] refactoring
        public static void GetTemplate(PageBase pPage, LstConsult pLstConsult,
                                bool pIsCeateTemporaryTemplate,
                                bool pIsNewTemporaryTemplate, bool pIsExistFilter,
                                out string opIdLstTemplate, out int opIdA)
        {

            //---------------------------------------------------------------------------------------------------
            //NOTE:
            //- pIsNewTemporaryTemplate = true lorsque l'on vient d'un bouton "..."
            //- pIsNewTemporaryTemplate = true lorsque l'on vient d'un enfant (Exemple les books d'un acteur)
            //- pIsNewTemporaryTemplate = true lorsque l'on vient du tracker
            //Lorsque pIsNewTemporaryTemplate = true => on génère systématiquement un nouveau template
            //=> ce template est "temporaire" il n'a pas vocation a être conservé => pas de sauvegarde dans Cookie  
            //---------------------------------------------------------------------------------------------------

            string guid = pPage.GUID;

            opIdA = SessionTools.Collaborator_IDA;
            opIdLstTemplate = string.Empty;

            //---------------------------------------------------------------------------------------------------
            //On retourne en priorité,s'il en existe un, le template courant stocké en Session.
            //NB: Lors de la 1ère publication opIdLstTemplate sera nécessairement null.
            //NB2:Dans la clé cookée en Session, il y a un GUID 
            //---------------------------------------------------------------------------------------------------
            // FI 20200602 [25370] Ajout de pPage.IsPostBack (Il est inutile d'appeler cette fonction qui n'a de sens que lors d'un postback)
            if (pPage.IsPostBack)
                ReadTemplateSession(pLstConsult.IdLstConsult, guid, ref opIdLstTemplate, ref opIdA);

            #region TF - Template forced (QueryString["TF"])
            //NB: Lorsque le Template forced défini est inexistant, un nouveau est automatiquement créé. (Utilisé par EG avec le Tracker)
            // FI 20200602 [25370] Lecture du TF et A uniquement si (!pPage.IsPostBack)
            // L'utilisateur peut ensuite changé de template si bon lui semble. Ce n'est pas vraiment un template forcé mais un templaté proposé 
            // Le jour où il y aura réelllement des template forcés, il faudra griser les boutons ouvrir et enregistrer sur List.aspx
            string idLstTemplateForced = (!pPage.IsPostBack) ? pPage.Request.QueryString["TF"] : null;
            int urlIdA = (!pPage.IsPostBack) ? Convert.ToInt32(pPage.Request.QueryString["A"]) : 0;

            bool isNewTemplateForced = false;
            if (StrFunc.IsFilled(idLstTemplateForced))
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
                
                if ((false == pIsNewTemporaryTemplate) && ExistsTemplate2(SessionTools.CS, pLstConsult.IdLstConsult, ref opIdLstTemplate, opIdA)) // FI 20200602 [25370] Ajout de (false == pIsNewTemporaryTemplate)
                {
                    //Si le propriétaire du template est l'utilisateur lui même, pas de contrôle.
                    if (opIdA == SessionTools.Collaborator_IDA)
                    {
                        if (!HasUserRightForTemplate(SessionTools.CS, opIdA, pLstConsult.IdLstConsult, opIdLstTemplate, RightsTypeEnum.VIEW))
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

            if (!(pIsNewTemporaryTemplate || isNewTemplateForced))
            {
                #region Recherche du template à ouvrir
                //S'il existe, on considère l'IDLSTTEMPLATE présent dans QueryString["T"]
                if (String.IsNullOrEmpty(opIdLstTemplate))
                {
                    opIdLstTemplate = pPage.Request.QueryString["T"];
                    opIdA = Convert.ToInt32(pPage.Request.QueryString["A"]);
                    if (opIdA == 0)
                        opIdA = SessionTools.User.IdA;
                }
                //Sinon, on considère celui défini en tant que Défaut (table LSTTEMPLATEDEF)
                if (String.IsNullOrEmpty(opIdLstTemplate))
                {
                    opIdLstTemplate = SearchTemplate(SessionTools.CS, pLstConsult.IdLstConsult, null);
                    if ((!String.IsNullOrEmpty(opIdLstTemplate)) && IsTemporary(opIdLstTemplate))
                    {
                        CleanDefault(SessionTools.CS, pLstConsult.IdLstConsult);
                        opIdLstTemplate = SearchTemplate(SessionTools.CS, pLstConsult.IdLstConsult, null);
                    }
                }
                //Sinon, on considère celui présent dans Cookie
                if (String.IsNullOrEmpty(opIdLstTemplate))
                {
                    ReadTemplateCookie(pLstConsult.IdLstConsult, out opIdLstTemplate, out opIdA);
                }
                //PL 20150601 GUEST New feature
                //Si un template est trouvé, on verifie sa validité (si invalide on le reset)
                CheckTemplate(SessionTools.CS, pLstConsult.IdLstConsult, ref opIdLstTemplate, ref opIdA);

                //Si aucun template trouvé et valide, et si GUEST, on considère celui défini en tant que Défaut pour son "Department" (table LSTTEMPLATEDEF)
                if (String.IsNullOrEmpty(opIdLstTemplate) && SessionTools.IsSessionGuest && (SessionTools.Collaborator.Department.Ida > 0))
                {
                    opIdLstTemplate = SearchTemplate(SessionTools.CS, pLstConsult.IdLstConsult, SessionTools.Collaborator.Department.Ida);
                    if (!String.IsNullOrEmpty(opIdLstTemplate))
                    {
                        opIdA = SessionTools.Collaborator.Department.Ida;
                        CheckTemplate(SessionTools.CS, pLstConsult.IdLstConsult, ref opIdLstTemplate, ref opIdA);
                    }
                }
                #endregion
            }

            //---------------------------------------------------------------------------------------------------
            // Lorsqu'aucun template n'est pas trouvé, on créé un nouveau template par défaut 
            //(Rq.: Ce qui est tjs le cas, lors d'un accès via le bouton "...")
            //(Rq.: Ce qui est tjs le cas, lorsque l'on consulte un référentiel Enfant)
            //---------------------------------------------------------------------------------------------------
            if (String.IsNullOrEmpty(opIdLstTemplate) || isNewTemplateForced)
            {
                #region Création d'un nouveau template


                //TF - Template forced: Si inexistant, un nouveau template est automatiquement créé. (Utilisé par EG avec le Tracker)
                string idLstTemplate = pPage.Request.QueryString["TF"];
                if (String.IsNullOrEmpty(idLstTemplate))
                    idLstTemplate = pIsExistFilter ? GetSelectName() : GetNewQueryName();

                opIdA = SessionTools.Collaborator_IDA;
                                //PL 20150601 GUEST New feature
                //Si GUEST, on affiche un message d'alerte
                if (SessionTools.IsSessionGuest)
                    pPage.MsgForAlertImmediate = Ressource.GetStringForJS("Msg_NoTemplateAvailable");
                else
                    opIdLstTemplate = CreateNewTemporaryTemplate(SessionTools.CS, pLstConsult.IdLstConsult, idLstTemplate, false, pLstConsult.Title);

                #endregion
            }
            else if (StrFunc.IsFilled(opIdLstTemplate) && pIsCeateTemporaryTemplate && !ReferentialWeb.IsTemporary(opIdLstTemplate)) // FI 20200602 [25370] nouveau cas
            {
                /* Creation d'un template temporaire */
                if (null == pLstConsult.template)
                {
                    /* On rentre ici lorsque l'utilisateur decoche un critère facultatif alors qu'il n'a pas encore fait de refreh du grid 
                       Le template est ici chargé pour générer dans la foulée un nouveau template temporaire */
                    pLstConsult.LoadTemplate(SessionTools.CS, opIdLstTemplate, opIdA);
                }

                Pair<string, int> retCopy = pLstConsult.CreateCopyTemporaryTemplate(SessionTools.CS);
                opIdLstTemplate = retCopy.First;
                opIdA = retCopy.Second;
            }

            #region Mise à jour du cookie (pour une prochaine consultation)
            if (!String.IsNullOrEmpty(opIdLstTemplate))
            {
                if (!(pIsNewTemporaryTemplate || isNewTemplateForced))
                    WriteTemplateCookie(pLstConsult.IdLstConsult, opIdLstTemplate, opIdA);

                WriteTemplateSession(pLstConsult.IdLstConsult, opIdLstTemplate, opIdA, guid);
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
                opIdA = SessionTools.User.IdA;
                opIdLstTemplate = null;
            }
            #endregion
        }
        /// <summary>
        /// Retourne le nom par défaut (Select) d'un template destiné à un usage lors d'un accès via le bouton "..."
        /// </summary>
        /// <returns></returns>
        public static string GetSelectName()
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
        /// <param name="pIdLstConsult">ID de la consultation</param>
        /// <param name="pGUID">GUID courant</param>
        /// <param name="opIdTemplate">REF: Id du template</param>
        /// <param name="opIdA">REF: ID du propriétaire du template</param>
        private static void ReadTemplateSession(string pIdLstConsult, string pGUID, ref string opIdLastTemplate, ref int opIdA)
        {
            try
            {
                // FI 20200518 [XXXXX] Utilisation de DataCache 
                if (DataCache.GetData<string>(Session_TemplateName + pIdLstConsult + pGUID) != null)
                {
                    opIdLastTemplate = DataCache.GetData<string>(Session_TemplateName + pIdLstConsult + pGUID);

                    if (DataCache.GetData<int>(Session_TemplateOwner + pIdLstConsult + pGUID) != 0)
                    {
                        opIdA = DataCache.GetData<int>(Session_TemplateOwner + pIdLstConsult + pGUID);
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
        public static void WriteTemplateSession(string pIdLstConsult, string pIdTemplate, int pIdA, string pGUID)
        {
            // FI 20200518 [XXXXX] Utilisation de DataCache 
            //pPage.Session[Session_TemplateName + pIdLstConsult + pGUID] = pIdTemplate;
            //pPage.Session[Session_TemplateOwner + pIdLstConsult + pGUID] = pIdA.ToString();

            DataCache.SetData<string>(Session_TemplateName + pIdLstConsult + pGUID, pIdTemplate);
            DataCache.SetData<int>(Session_TemplateOwner + pIdLstConsult + pGUID, pIdA);
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
            AspTools.ReadSQLCookie(CookieName(pIdLstConsult), out string cookieValue);
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
            StrBuilder SQLSelect = new StrBuilder(SQLCst.SELECT);
            SQLSelect += "1" + Cst.CrLf;
            SQLSelect += SQLCst.FROM_DBO + Cst.OTCml_TBL.LSTTEMPLATE.ToString() + Cst.CrLf;
            SQLSelect += SQLCst.WHERE + GetSQLClause_PK_LSTTEMPLATE2(pCS, pIdLstConsult, pIdLstTemplate, pIdA, null, out DataParameters dbParam);

            QueryParameters qryParameters = new QueryParameters(pCS, SQLSelect.ToString(), dbParam);
            object obj = DataHelper.ExecuteScalar(pCS, CommandType.Text, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter());
            return (null != obj);
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
            sql += SQLCst.AND + "(lt.IDA=" + (pIdA == null ? SessionTools.User.IdA : pIdA) + ")" + Cst.CrLf;

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
            LstConsult.LstTemplate template = new LstConsult.LstTemplate();
            template.Load(pCS, pIdLstConsult, pIdLstTemplate, pIdAOwner);

            return template.HasUserRight(pCS, SessionTools.User, pRight);
        }

        /// <summary>
        ///  Création d'un nouveau template temporaire avec un filtre éventuel. 
        ///  <para>Le proprétaire est l'utilisateur courant</para>
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pIdLstTemplate">valeur à utiliser pour l'affectation de l'ID</param>
        /// <param name="pIsLoadOnStart"></param>
        /// <param name="pTitle"></param>
        /// <param name="pIsNewTemplate"></param>
        /// <returns>ID du template créé</returns>
        // EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        // EG 20210505 [25700] FreezeGrid implementation 
        public static string CreateNewTemporaryTemplate(string pCS, string pIdLstConsult, string pIdLstTemplate, bool pIsLoadOnStart, string pTitle)
        {
            //On créé un new LSTTEMPLATE que l'on renseigne par defaut
            LstConsult.LstTemplate lstTemplate = new LstConsult.LstTemplate
            {
                IDLSTTEMPLATE = TEMPORARYPREFIX + pIdLstTemplate,
                IDLSTCONSULT = pIdLstConsult,
                DISPLAYNAME = pTitle,
                DESCRIPTION = string.Empty,
                CSSCOLOR = string.Empty,
                ROWBYPAGE = 0,
                RIGHTPUBLIC = RightsTypeEnum.NONE.ToString(),
                RIGHTDESK = RightsTypeEnum.NONE.ToString(),
                RIGHTDEPARTMENT = RightsTypeEnum.NONE.ToString(),
                RIGHTENTITY = RightsTypeEnum.NONE.ToString(),
                FREEZECOL = 0,
                EXTLLINK = string.Empty,
                ISDEFAULT = false,
                IDA = SessionTools.Collaborator_IDA,
                ISLOADONSTART = pIsLoadOnStart,
                REFRESHINTERVAL = 0
            };

            //Insert SQL
            CreateOverwrite(pCS, lstTemplate, lstTemplate.IDA);

            return lstTemplate.IDLSTTEMPLATE;
        }

        /// <summary>
        /// création d'un de template. 
        /// <para>
        /// Si le template exsite déjà il est supprimé 
        /// </para>
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pLstTemplate">représente le template que l'on veut créer</param>
        /// <param name="pIdA">représente le propiétaire du template</param>
        /// EG 20200312 [25077] RDBMS : Correction alias
        public static void CreateOverwrite(string pCS, LstConsult.LstTemplate pLstTemplate, int pIdA)
        {
            //Si existe deja, on delete
            if (ExistsTemplate(pCS, pLstTemplate.IDLSTCONSULT, pLstTemplate.IDLSTTEMPLATE, pIdA))
            {
                //20091012 FI utilisation de la méthode static LstTemplate.Delete
                //Pourquoi faire un load pour delete ensuite ?
                //LstTemplate oldLSTTEMPLATE = new LstTemplate();
                //oldLSTTEMPLATE.Load(pCS, IdLstConsult, pLstTemplate.IDLSTTEMPLATE, forcedIdA);
                //oldLSTTEMPLATE.Delete(pCS, false);
                Delete(pCS, pLstTemplate.IDLSTCONSULT, pLstTemplate.IDLSTTEMPLATE, pIdA, false);
            }

            //Insert
            pLstTemplate.Insert(pCS, pIdA);

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pIdLstConsult"></param>
        /// <param name="pIdLstTemplate"></param>
        /// <param name="pIdA"></param>
        /// <param name="pFilter"></param>
        /// FI 20200602 [XXXXX] Add 
        public static void InsertFilter(string pCS, string pIdLstConsult, String pIdLstTemplate, int pIdA, Pair<string, Pair<string, string>> pFilter)
        {

            string valueForFilter = pFilter.First;
            Pair<string, string> columnForFilter = pFilter.Second;


            string sqlQuery = SQLCst.INSERT_INTO_DBO + Cst.OTCml_TBL.LSTWHERE.ToString();
            sqlQuery += "(IDA,IDLSTCONSULT,IDLSTTEMPLATE,IDLSTCOLUMN,TABLENAME,COLUMNNAME,ALIAS,POSITION,OPERATOR,LSTVALUE,LSTIDVALUE,ISENABLED,ISMANDATORY)" + Cst.CrLf;
            sqlQuery += " values (";
            sqlQuery += pIdA.ToString() + ",";
            sqlQuery += DataHelper.SQLString(pIdLstConsult) + ",";
            sqlQuery += DataHelper.SQLString(pIdLstTemplate) + ",";
            if (null != columnForFilter) //=> on vient du référentiel
            {
                // IDLSTCOLUMN,TABLENAME,COLUMNNAME,ALIAS
                sqlQuery += "null," + DataHelper.SQLString(columnForFilter.First) + "," + DataHelper.SQLString(columnForFilter.Second) + "," + DataHelper.SQLString(SQLCst.TBLMAIN) + ",";
            }
            else
            {
                // PM 20091113 : Modif de ce qui a été fait en dur afin de gérer la consultation des Assets ETD
                // Remplacement du nom de la table, de son alias et de nom de la colonne 'identifier' par des variables
                string tableAlias = SQLCst.TBLMAIN;
                string tableName = SQLCst.TBLMAIN;
                string identifierName = "IDENTIFIER";
                if ("*Select" == pIdLstTemplate)
                {
                    if ("ASSET_ETD_EXPANDED" == pIdLstConsult)
                    {
                        // FI 20180130 [23749] l'alias de la table est a_etd (avant c'était vw_a_etd)
                        tableAlias = "vw_a_etd";
                        tableName = "VW_ASSET_ETD_EXPANDED";
                    }
                    else if ("MATURITY" == pIdLstConsult)
                    {
                        tableAlias = "ma";
                        tableName = "MATURITY";
                        identifierName = "MATURITYMONTHYEAR";
                    }
                    else
                    {
                        //CC/PL 20120529 
                        tableAlias = "tasset";
                        tableName = "VW_TRADE_ASSET";
                    }
                }
                //20090626 PL TODO en dur pour l'instant...(IDLSTCOLUMN et "t")
                // EG 20160404 Migration vs2013
                // #warning 20090626 PL TODO
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
            sqlQuery += "0,"; //POSITION
                              //FI 20111202 Mise en place de l'opérateur Contains à la place de like
            if (valueForFilter == "%")
                sqlQuery += DataHelper.SQLString(SQLCst.LIKE.Trim()) + ","; //OPERATOR
            else
                sqlQuery += DataHelper.SQLString("Contains") + ",";//OPERATOR

            sqlQuery += "@LSTVALUE,"; //Note: Utilisation par sécurité d'un parameter car donnée saisie par le user
            sqlQuery += SQLCst.NULL + ",";
            sqlQuery += "1,"; //ISENABLED
            sqlQuery += "0)"; //ISMANDATORY

            DataParameters dp = new DataParameters();
            dp.Add(new DataParameter(pCS, "LSTVALUE", DbType.AnsiString, SQLCst.UT_NOTE_LEN), valueForFilter);

            DataHelper.ExecuteNonQuery(pCS, CommandType.Text, sqlQuery, dp.GetArrayDbParameter());
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

                    LstConsult.LstTemplate defaultLstTemplate = new LstConsult.LstTemplate();
                    if (defaultLstTemplate.Load(pCS, pIdLstConsult, name_WithoutPrefix, SessionTools.Collaborator_IDA))
                    {
                        defaultLstTemplate.ISDEFAULT = true;
                        defaultLstTemplate.Update(pCS);
                    }
                    else
                    {
                        defaultLstTemplate.Load(pCS, pIdLstConsult, name, SessionTools.Collaborator_IDA);
                        defaultLstTemplate.ISDEFAULT = false;
                        defaultLstTemplate.Update(pCS);
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
        /// <param name="pIdLstTemplate">ID du template</param>
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
        /// <param name="pCS"></param>
        /// <param name="pIdLstConsult"></param>
        /// <param name="pIdLstTemplate">ID du template</param>
        /// <param name="pIdA">ID du propriétaire du template</param>
        /// <param name="pIsWithTemporary">Si true supprime l'éventuel template temporaire associé</param>
        public static void Delete(string pCS, string pIdLstConsult, string pIdLstTemplate, int pIdA, bool pIsWithTemporary)
        {
            StrBuilder SQLDelete = new StrBuilder();
            SQLDelete += SQLCst.DELETE_DBO + Cst.OTCml_TBL.LSTTEMPLATE.ToString() + Cst.CrLf;
            SQLDelete += SQLCst.WHERE + GetSQLClause_PK_LSTTEMPLATE2(pCS, pIdLstConsult, pIdLstTemplate, pIdA, null, out DataParameters dbParam);

            QueryParameters qryParameters = new QueryParameters(pCS, SQLDelete.ToString(), dbParam);
            DataHelper.ExecuteNonQuery(pCS, CommandType.Text, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter());

            if (pIsWithTemporary & !pIdLstTemplate.StartsWith(TEMPORARYPREFIX))
            {
                //Traitements du "temporary"
                SQLDelete = new StrBuilder();
                SQLDelete += SQLCst.DELETE_DBO + Cst.OTCml_TBL.LSTTEMPLATE.ToString() + Cst.CrLf;
                SQLDelete += SQLCst.WHERE + GetSQLClause_PK_LSTTEMPLATE2(pCS, pIdLstConsult, TEMPORARYPREFIX + pIdLstTemplate, pIdA, null, out dbParam);
                qryParameters = new QueryParameters(pCS, SQLDelete.ToString(), dbParam);
                DataHelper.ExecuteNonQuery(pCS, CommandType.Text, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter());
            }

            #region Delete des enfants associés
            Cst.OTCml_TBL[] childTableName = { Cst.OTCml_TBL.LSTSELECT, Cst.OTCml_TBL.LSTORDERBY, Cst.OTCml_TBL.LSTWHERE, Cst.OTCml_TBL.LSTPARAM };
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
        /// <param name="pTableName">nom de la table enfant</param>
        /// <param name="pIdLstTemplate">string: ID du template</param>
        /// <param name="pIdA">ID du propriétaire du template</param>
        /// <param name="pIsWithTemporary">Si true supprime l'éventuel template temporaire associé</param>
        public static void DeleteChild(string pCS, Cst.OTCml_TBL pTableName, string pIdLstConsult, string pIdLstTemplate, int pIdA, bool pIsWithTemporary)
        {
            StrBuilder SQLDelete = new StrBuilder();
            SQLDelete += SQLCst.DELETE_DBO + pTableName.ToString() + Cst.CrLf;
            SQLDelete += SQLCst.WHERE + GetSQLClause_PK_LSTTEMPLATE2(pCS, pIdLstConsult, pIdLstTemplate, pIdA, null, out DataParameters dbParam);

            QueryParameters qryParameters = new QueryParameters(pCS, SQLDelete.ToString(), dbParam);
            DataHelper.ExecuteNonQuery(pCS, CommandType.Text, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter());

            if (pIsWithTemporary && !pIdLstTemplate.StartsWith(TEMPORARYPREFIX))
            {
                //Traitements des "temporaries"
                SQLDelete = new StrBuilder();
                SQLDelete += SQLCst.DELETE_DBO + pTableName.ToString() + Cst.CrLf;
                SQLDelete += SQLCst.WHERE + GetSQLClause_PK_LSTTEMPLATE2(pCS, pIdLstConsult, TEMPORARYPREFIX + pIdLstTemplate, pIdA, null, out dbParam);

                qryParameters = new QueryParameters(pCS, SQLDelete.ToString(), dbParam);
                DataHelper.ExecuteNonQuery(pCS, CommandType.Text, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter());
            }
        }
        #endregion

        #region GetSQLClause_PK_LSTTEMPLATE2
        /// <summary>
        /// Obtient la clause where pour identifier un template par rapport à sa clé (IDLSTTEMPLATE et IDA)
        /// </summary>
        /// <param name="pIdLstConsult"></param>
        /// <param name="pIdLstTemplate">ID du template</param>
        /// <param name="pIdA">ID du propriétaire du template</param>
        /// <param name="pDbAlias">alias pour prefixer les colonnes</param>
        /// <param name="opDataParameters">OUT: Data Parameters</param>
        /// <returns>clause where</returns>
        public static string GetSQLClause_PK_LSTTEMPLATE2(string pCS, string pIdLstConsult, string pIdLstTemplate, int pIdA, string pDbAlias,
            out DataParameters opDataParameters)
        {
            opDataParameters = new DataParameters();
            opDataParameters.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.IDLSTTEMPLATE), pIdLstTemplate);
            opDataParameters.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.IDLSTCONSULT), pIdLstConsult);
            opDataParameters.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.IDA), pIdA);

            string alias = (String.IsNullOrEmpty(pDbAlias) ? string.Empty : pDbAlias + ".");
            string ret = StrFunc.AppendFormat(@"({0}IDLSTTEMPLATE = @IDLSTTEMPLATE and {0}IDLSTCONSULT = @IDLSTCONSULT and {0}IDA = @IDA)", alias) + Cst.CrLf;
            return ret;
        }
        #endregion

        /// <summary>
        /// Consultion sur la base d'un fichier XML
        /// Sauvegarde dans une variable Session de la liste des colonnes (,type, ...) disponibles pour une consultion
        /// </summary>
        /// <param name="pPage">Page appelante</param>
        /// <param name="pIdLstConsult">ID de la consultation</param>
        /// <param name="pIdLstTemplate">ID du template</param>
        /// <param name="pIdA">ID du proprietaire du template</param>
        /// <param name="pReferentialsReferential">Classe referential</param>
        /// <returns>Nom de de la variable Session dasn laquelle sont sauvegardés les données</returns>
        // EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        // EG 20210412 [XXXXX] Step1 : Utilisation Hyperlink pour Autocomplete(nouveaux attributs)
        public static string SaveInSession_LstColumn(PageBase pPage, string pIdLstConsult, string pIdLstTemplate, int pIdA, ReferentialsReferential pReferentialsReferential)
        {
            string previousRessource = string.Empty;

            // FI 20200217 [XXXXX] Lecture de pPage.GUID
            string guid = pPage.GUID;

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
            for (int index = 0; index < pReferentialsReferential.Column.GetLength(0); index++)
            {
                ReferentialsReferentialColumn rrc = pReferentialsReferential.Column[index];
                if (
                    (rrc.ColumnName != Cst.OTCml_COL.ROWVERSION.ToString())
                    && (rrc.ColumnName != Cst.OTCml_COL.ROWATTRIBUT.ToString())
                    && !TypeData.IsTypeImage(rrc.DataType.value)
                    && ((rrc.IsHideSpecified && !rrc.IsHide) || (rrc.IsHideInDataGridSpecified && !rrc.IsHideInDataGrid) || (rrc.IsHideInCriteriaSpecified && !rrc.IsHideInCriteria))
                    && (rrc.Ressource != "Empty")
                    && (!rrc.IsRole) && (!rrc.IsItem)
                    )
                {
                    string displayCol;
                    if (StrFunc.IsEmpty(rrc.Ressource))
                    {
                        displayCol = previousRessource;
                        displayCol += " (" + Ressource.GetString(rrc.ColumnName) + ")";
                    }
                    else
                    {
                        displayCol = Ressource.GetMulti(rrc.Ressource, 2, 0);
                        previousRessource = displayCol;
                        if ((index + 1 < pReferentialsReferential.Column.GetLength(0)) && (StrFunc.IsEmpty(pReferentialsReferential.Column[index + 1].Ressource)))
                        {
                            if (Ressource.GetStringByRef(rrc.ColumnName))
                                displayCol += " (" + Ressource.GetString(rrc.ColumnName) + ")";
                        }
                    }
                    string dataCol = indice.ToString() + "," + rrc.AliasTableName + "," + rrc.DataType.value + "," + pReferentialsReferential.TableName + "," + rrc.ColumnName;
                    //
                    OpenReferentialArguments arg = new OpenReferentialArguments
                    {
                        isHideInCriteriaSpecified = true,
                        isHideInCriteria = (rrc.IsHideInCriteriaSpecified && rrc.IsHideInCriteria)
                    };

                    string relationTableName = string.Empty;
                    string relationColumnRelation = string.Empty;
                    string relationColumnSelect = string.Empty;
                    // FI 20160804 [Migration TFS] Repository isntead of Referential
                    string relationListType = Cst.ListType.Repository.ToString();
                    // EG 20231114 [WI736] Appel à ExistsRelation
                    if (rrc.ExistsRelation)
                    {
                        if (ArrFunc.IsFilled(rrc.Relation[0].ColumnRelation) && ArrFunc.IsFilled(rrc.Relation[0].ColumnSelect))
                        {
                            relationTableName = rrc.Relation[0].TableName;
                            relationColumnRelation = rrc.Relation[0].ColumnRelation[0].ColumnName;
                            relationColumnSelect = rrc.Relation[0].ColumnSelect[0].ColumnName;
                            relationListType = rrc.Relation[0].ListType;
                            if (StrFunc.IsFilled(rrc.Relation[0].ColumnSelect[0].Ressource))
                            {
                                //CC/PL 20130521
                                //displayCol = Ressource.GetMulti(rrc.Relation[0].ColumnSelect[0].Ressource);
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
                            else if (Cst.IsDDLTypeCSSColor(ddlType))
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
                            //else if (Cst.IsDDLTypeMarketFull(ddlType) || Cst.IsDDLTypeMarketId(ddlType) || Cst.IsDDLTypeMarketETD(ddlType))
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
                    else if (rrc.IsHyperLinkSpecified && rrc.IsHyperLink.linktypeSpecified && rrc.IsHyperLink.linktype == "column" && 
                             rrc.IsHyperLink.typeSpecified && rrc.IsHyperLink.IsAutocompleteAuthorized)
                    {
                        // FI 20210804 [XXXXX]
                        // OpenReferentialArguments
                        // s'il n'existe pas de colonne Relation, Spheres® s'appuie sur les éventuelles propriétés HyperLink
                        //
                        // Défaut par rapport aux relations
                        // -  Il n'existe pas de Condition :
                        // Exemple1 : sur une colonne destinée à des acteurs CSS => Spheres® propose tous les acteurs 
                        // Exemple2 : sur une colonne destinée à des acteurs ENTITY => Spheres® propose tous les acteurs 

                        // -  Il est supposé que les colonnes où il existe l'hyperlink affiche des colonnes particulières  
                        // Exemple1  : sur une colonne Marché avec hyperlink => Spheres® suppose la colonne affiche la colonne SHORT_ACRONYM
                        // Exemple2  : sur une colonne Acteur avec hyperlink => Spheres® suppose la colonne affiche la colonne IDENTIFIER
                        if (rrc.IsHyperLink.linktableSpecified && rrc.IsHyperLink.nameSpecified)
                        {
                            arg.referential = rrc.IsHyperLink.linktable;
                            arg.sqlColumnSpecified = true;
                            arg.sqlColumn = rrc.IsHyperLink.name;
                            arg.conditionSpecified = rrc.IsHyperLink.conditionSpecified;
                            arg.condition = rrc.IsHyperLink.condition;
                        }
                        else
                        {
                            switch (rrc.IsHyperLink.type)
                            {
                                case "IDM":/* PDIML/Repository/VW_MARKET_IDENTIFIER.xml */
                                    arg.referential = Cst.OTCml_TBL.VW_MARKET_IDENTIFIER.ToString();
                                    arg.sqlColumn = "SHORT_ACRONYM"; // Il est supposé que la colonne affichée est obligatoirement SHORT_ACRONYM
                                    break;
                                case "IDA": /* PDIML/Repository/ACTOR.xml */
                                    arg.referential = Cst.OTCml_TBL.ACTOR.ToString();
                                    arg.sqlColumn = "IDENTIFIER";
                                    break;
                                case "IDB": /* PDIML/Repository/BOOK.xml */
                                    arg.referential = Cst.OTCml_TBL.BOOK.ToString();
                                    arg.sqlColumn = "IDENTIFIER";
                                    break;
                                case "IDDC": /* PDIML/Repository/DERIVATIVECONTRACT.xml */
                                    arg.referential = Cst.OTCml_TBL.DERIVATIVECONTRACT.ToString();
                                    arg.sqlColumn = "IDENTIFIER";
                                    break;
                                case "IDCC":
                                    arg.referential = Cst.OTCml_TBL.COMMODITYCONTRACT.ToString();
                                    arg.sqlColumn = "IDENTIFIER";
                                    break;
                                case "IDXC":
                                    arg.referential = Cst.OTCml_TBL.VW_CONTRACT.ToString();
                                    arg.sqlColumn = "IDENTIFIER";
                                    break;
                                case "IDASSET_ETD": /* PDIML/Repository/ASSET_ETD.xml */
                                    arg.referential = Cst.OTCml_TBL.ASSET_ETD.ToString();
                                    arg.sqlColumn = "IDENTIFIER";
                                    break;
                                case "IDASSET_COMMODITY": /* PDIML/Repository/ASSET_COMMODITY.xml */
                                    arg.referential = Cst.OTCml_TBL.ASSET_COMMODITY.ToString();
                                    arg.sqlColumn = "IDENTIFIER";
                                    break;
                                case "IDASSET": /* PDIML/Repository/VW_ASSET.xml */
                                    arg.referential = Cst.OTCml_TBL.VW_ASSET.ToString();
                                    arg.sqlColumn = "IDENTIFIER";
                                    break;
                                case "IDT": /* PDIML/Repository/TRADE.xml : Afin de pouvoir proposer des identifiants de trade de Marché */
                                    arg.referential = Cst.OTCml_TBL.TRADE.ToString();
                                    arg.sqlColumn = "IDENTIFIER";
                                    break;
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
                    alSelect[3].Add(pReferentialsReferential.TableName);
                    alSelect[4].Add(rrc.ColumnName);
                    alSelect[5].Add(rrc.DataType.value);
                    alSelect[6].Add(arg);
                }
                //PL 20120521 WARNING ligne suivante en commentaire
                //indice++;
            }

            string name = pIdLstConsult + "-" + pIdLstTemplate + "-" + pIdA.ToString() + "-" + guid + "-" + "ListOfColumns";

            // FI 20180518 [XXXXX] Utilisation de DataCache
            //pPage.Session[name] = ;
            DataCache.SetData<ArrayList[]>(name, alSelect);
            
            return name;
        }


        /// <summary>
        /// Ajoute les clauses Where d'un template à une classe referential
        /// </summary>
        /// <param name="pReferentialsReferential">REF ReferentialsReferential: classe referential sur laquelle ajouter les clauses Where</param>
        /// FI 20200220 [XXXXX] opReferentialsReferential n'est plus de type ref
        /// EG 20210419 [XXXXX] Upd Usage du businessCenter de l'entité
        public static void AddLSTWHEREToReferential(string pCS, string pIdLstConsult, string pIdLstTemplate, int pIdA, ReferentialsReferential pReferentialsReferential)
        {
            ArrayList alObjects = new ArrayList();
            ArrayList alNewJoins = new ArrayList();
            //
            #region Initialize
            if (pReferentialsReferential.SQLWhereSpecified)
            {
                for (int index = 0; index < pReferentialsReferential.SQLWhere.Length; index++)
                {
                    alObjects.Add(((Array)pReferentialsReferential.SQLWhere).GetValue(index));
                }
            }
            //
            if (pReferentialsReferential.SQLJoinSpecified)
            {
                for (int index = 0; index < pReferentialsReferential.SQLJoin.Length; index++)
                {
                    alNewJoins.Add(pReferentialsReferential.SQLJoin[index]);
                }
            }
            #endregion
            //
            #region select LSTWHERE
            //string SQLSelect = SQLCst.SELECT + @"lw.IDLSTCOLUMN, lw.OPERATOR, lw.LSTVALUE, lw.LSTIDVALUE" + Cst.CrLf;
            //PL 20120924 Debug Use ALIAS
            //string SQLSelect = SQLCst.SELECT + @"lw.IDLSTCOLUMN,lw.TABLENAME,lw.COLUMNNAME,lw.OPERATOR,lw.LSTVALUE,lw.LSTIDVALUE" + Cst.CrLf;
            string SQLSelect = SQLCst.SELECT + @"lw.IDLSTCOLUMN,lw.ALIAS,lw.TABLENAME,lw.COLUMNNAME,lw.OPERATOR,lw.LSTVALUE,lw.LSTIDVALUE" + Cst.CrLf;
            SQLSelect += SQLCst.FROM_DBO + Cst.OTCml_TBL.LSTWHERE.ToString() + " lw" + Cst.CrLf;
            SQLSelect += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.LSTTEMPLATE.ToString() + " lt on (lt.IDA=lw.IDA)";
            SQLSelect += SQLCst.AND + "(lt.IDLSTTEMPLATE=lw.IDLSTTEMPLATE)" + Cst.CrLf;
            SQLSelect += SQLCst.AND + "(lt.IDLSTCONSULT=lw.IDLSTCONSULT)" + Cst.CrLf;

            SQLWhere sqlWhere = new SQLWhere();
            sqlWhere.Append("lw.IDA=" + pIdA.ToString());
            sqlWhere.Append("lw.IDLSTTEMPLATE=" + DataHelper.SQLString(pIdLstTemplate));
            sqlWhere.Append("lw.IDLSTCONSULT=" + DataHelper.SQLString(pIdLstConsult));
            sqlWhere.Append("((lt.ISENABLEDLSTWHERE=" + DataHelper.SQLBoolean(true) + SQLCst.AND + "lw.ISENABLED=" + DataHelper.SQLBoolean(true) + ")" + SQLCst.OR + "(lw.ISMANDATORY=" + DataHelper.SQLBoolean(true) + "))");

            SQLSelect += sqlWhere.ToString();
            #endregion
            //
            DataSet ds = DataHelper.ExecuteDataset(pCS, CommandType.Text, SQLSelect);
            DataTable dt = ds.Tables[0];
            if (dt.Select().GetLength(0) > 0)
            {
                pReferentialsReferential.HasLstWhereClause = true;
                if (!pReferentialsReferential.SQLWhereSpecified)
                {
                    pReferentialsReferential.SQLWhere = new ReferentialsReferentialSQLWhere[dt.Select().GetLength(0)];
                    pReferentialsReferential.SQLWhereSpecified = true;
                }
                int countRelation = 0;
                foreach (DataRow row in dt.Select())
                {
                    ReferentialsReferentialColumn currentColumn = null;
                    //if (Convert.ToInt32(row["IDLSTCOLUMN"]) > -1)
                    if (StrFunc.IsFilled(Convert.ToString(row["COLUMNNAME"])))
                    {
                        //PL 20120924 Debug Use ALIAS
                        //currentColumn = opReferentialsReferential[Convert.ToString(row["COLUMNNAME"])];
                        currentColumn = pReferentialsReferential[Convert.ToString(row["COLUMNNAME"]), Convert.ToString(row["ALIAS"])];
                    }
                    else if (Convert.ToInt32(row["IDLSTCOLUMN"]) > -1)//Compatibilié asc: idLstColumn maintenu pour compatibilié ascendante (à supprimer en v2.7)
                    {
                        currentColumn = pReferentialsReferential.Column[Convert.ToInt32(row["IDLSTCOLUMN"])];
                    }
                    if (currentColumn != null)
                    {

                        ReferentialsReferentialSQLWhere rrSQLWhere = new ReferentialsReferentialSQLWhere
                        {
                            AliasTableName = currentColumn.AliasTableName,
                            AliasTableNameSpecified = true,
                            TableName = Convert.ToString(row["TABLENAME"]), // FI 20201127 [XXXXX] Alimentation de le property TABLENAME
                            TableNameSpecified = true,
                            ColumnName = currentColumn.ColumnName,
                            ColumnNameSpecified = true,
                            LstValue = LstConsult.FormatLstValue2(pCS, row["LSTVALUE"].ToString().Trim(), currentColumn.DataType, false, false, false, SessionTools.User.Entity_IdA, SessionTools.User.Entity_BusinessCenter),
                            LstValueSpecified = true,
                            DataType = currentColumn.DataType,
                            Operator = row["OPERATOR"].ToString().Trim(),
                            OperatorSpecified = true
                        };


                        if ((currentColumn.Relation != null)
                            && (currentColumn.Relation.Length > 0)
                            && (currentColumn.Relation[0].ColumnSelect != null)
                            && (currentColumn.Relation[0].ColumnSelect.Length > 0))
                        {
                            rrSQLWhere.AliasTableName = currentColumn.Relation[0].AliasTableName;
                            rrSQLWhere.AliasTableNameSpecified = true;
                            rrSQLWhere.TableName = currentColumn.Relation[0].TableName; // FI 20201127 [XXXXX] Alimentation de le property TABLENAME
                            rrSQLWhere.TableNameSpecified = true;

                            rrSQLWhere.ColumnName = currentColumn.Relation[0].ColumnSelect[0].ColumnName;
                            rrSQLWhere.ColumnNameSpecified = true;
                            // FI 20190327 [24603] rrSQLWhere.DataType est de type ReferentialsReferentialColumnDataType
                            rrSQLWhere.DataType = new ReferentialsReferentialColumnDataType
                            {
                                value = currentColumn.Relation[0].ColumnSelect[0].DataType
                            };
                        }
                        alObjects.Add(rrSQLWhere);
                    }
                }
                System.Type type = ((System.Array)pReferentialsReferential.SQLWhere).GetType().GetElementType();
                pReferentialsReferential.SQLWhere = (ReferentialsReferentialSQLWhere[])alObjects.ToArray(type);

                if (countRelation > 0)
                {
                    pReferentialsReferential.SQLJoin = new string[alNewJoins.Count];
                    pReferentialsReferential.SQLJoinSpecified = true;
                    for (int i = 0; i < alNewJoins.Count; i++)
                        pReferentialsReferential.SQLJoin[i] = alNewJoins[i].ToString();
                }
            }
        }


        /*
        public void TEMP_PASCAL(string pCS, string pIdLstConsult, string pIdLstTemplate, int pIdA)
        {
            string idMenu = null;
            LstConsult lstConsult = new LstConsult(pCS, pIdLstConsult, idMenu);
            lstConsult.LoadTemplate(pCS, pIdLstTemplate, pIdA, false);

            bool isConsultWithDynamicArgs = false;
            lstConsult.LoadLstDatatable(pCS, isConsultWithDynamicArgs);

            string[] parameters = null;
            string condApp = string.Empty;

            ReferentialsReferential rr = lstConsult.GetReferentials(pCS, null, parameters, null).Items[0];
            //rr.SetDynamicDatas(pDynamicDatas);        
            rr.Initialize(true, null, condApp, parameters, null);
            //ReferentialTools.InitializeID(ref referential);
        }
        */
    }
}