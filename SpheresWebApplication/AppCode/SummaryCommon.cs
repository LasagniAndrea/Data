using EFS.ACommon;
using EFS.Actor;
using EFS.ApplicationBlocks.Data;
using EFS.Authenticate;
using EFS.Common;
using EFS.Common.Log;
using EFS.Common.Web;
using EFS.Referential;

using System;
using System.Data;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using SpheresMenu = EFS.Common.Web.Menu;
using System.Linq;


namespace EFS.Spheres
{
    /// <summary>
    /// Classe de base pour la gestion du Sommaire 
    /// - le sommaire pour Spheres est dans la page Summary.aspx
    /// - le sommaire pour le portail client est dans une balise directement dans la page de démarrage CustomerPortal.aspx
    /// </summary>
    /// EG 20210614 [25500] New Customer Portal
    public class SummaryCommon : PageBase
    {
        protected SpheresMenu.Menus menus;
        // Représente le PlaceHolder container des menus Spheres
        protected virtual PlaceHolder CtrlMenu { get { return null; } }
        // Représente le contrôle caché indiquant le mode de filtrage en cours des menus affichés (Menu complet ou allégé)
        protected virtual HtmlInputHidden CtrlMaskMenu { get { return null; } }
        // Représente le contrôle caché indiquant l'IdA de l'utilisateur connecté
        protected virtual HtmlInputHidden CtrlIdA { get { return null; } }
        // Représente le bouton pour passer d'un menu allégé à un menu complet
        protected virtual WCToolTipLinkButton BtnMaskMenu { get { return null; } }

        /// <summary>
        /// Chargement du sommaire
        /// </summary>
        /// EG 20220623 [XXXXX] Implémentation Authentification via Shibboleth SP et IdP
        /// EG 20220123 [26235][WI543] Refactoring de la gestion des actions sur le PASSWORD liée aux règles de sécurité de l'acteur en cours de connexion
        protected void LoadSummary()
        {
            ClientScript.RegisterHiddenField("__SOFTWARE", Software.Name);
            ClientScript.RegisterHiddenField("__ROLE", SessionTools.Collaborator_ROLE);
            CtrlMaskMenu.Attributes["title"] = Ressource.GetString("imgBtnMaskMenu");
            bool isAuthenticated = false;
            if ((HttpContext.Current.User.Identity.AuthenticationType == "Forms") || SessionTools.IsShibbolethAuthenticationType)
            {
                isAuthenticated = Request.IsAuthenticated;
            }
            else if (
                (HttpContext.Current.User.Identity.AuthenticationType == "NTLM")
                ||
                (HttpContext.Current.User.Identity.AuthenticationType == "Negotiate"))
            {
                DateTime loginExpirationDateTime = new DateTime(9999, 1, 1);
                Collaborator clb = new Collaborator();

                string collaborator = HttpContext.Current.User.Identity.Name.ToString();
                collaborator = collaborator.Substring(collaborator.IndexOf(@"\") + 1);

                SessionTools.Initialize2(string.Empty, string.Empty, string.Empty, string.Empty, 0, 0, 0, 0, Cst.DBStatusEnum.NA.ToString(), clb, loginExpirationDateTime);

                string rdbmsName, serverName, databaseName, userName, pwd;
                rdbmsName = serverName = databaseName = userName = pwd = null;
                SystemSettings.GetDefaultConnectionCharacteristics(false, this.Request.ApplicationPath,
                                        ref rdbmsName, ref serverName, ref databaseName, ref userName, ref pwd);
                string source = SystemSettings.GetMainConnectionString(rdbmsName, serverName, databaseName, userName, pwd);

                //Windows authentification
                //ConnectionString => Définie sur WebConfig (même si le USER est défini comme RDBMS (car impossible de connaître son mot de passe)  
                bool isOk = true;
                try
                {
                    CheckConnection.CheckConnectionString(source);
                }
                catch (Exception) { isOk = false; }

                if (isOk)
                {
                    // FI 20200811 [XXXXX] => Appel à SynchroDatesysCol pour que le 1er appel à OTCmlHelper.GetDateSys(cs) ne provoque pas l'ouverture d'une nouvelle connexion s'il existe une transaction courante 
                    OTCmlHelper.SynchroDatesysCol(source);

                    Connection connection = new Connection();
                    // EG 20220623 [XXXXX] Implémentation Authentification via Shibboleth SP et IdP
                    isOk = connection.LoadCollaborator(source, collaborator, SessionTools.IsShibbolethAuthenticationType);
                    clb = connection.Collaborator;

                    int maxSimultaneousLoginAuthorized = 1;
                    int maxEntityAuthorized = 1;
                    string errResource = string.Empty;
                    if (isOk)
                    {
                        clb.SetRDBMSTrace(source, SessionTools.ServerAndUserHost);

                        string errMessage = string.Empty;
                        string debugStep = string.Empty;
                        isOk = CheckWebConnection.ReadRegistration(source, ref maxSimultaneousLoginAuthorized, ref maxEntityAuthorized,
                            ref errResource, ref errMessage, ref debugStep);
                    }

                    if (isOk && !clb.IsActorSysAdmin)
                    {
                        if (SessionTools.License.IsLicFunctionalityAuthorised(LimitationFunctionalityEnum.MODELDAYHOUR))
                            isOk = CheckConnection.CheckModelDayHour(source, clb, ref loginExpirationDateTime);
                    }
                    string errmsg = string.Empty;
                    if (isOk)
                        isOk = CheckWebConnection.CheckSimultaneousLogin(clb, maxSimultaneousLoginAuthorized, ref errmsg);

                    if (isOk)
                        isOk = SessionTools.Initialize2(clb, source, rdbmsName, serverName, databaseName, loginExpirationDateTime);

                    if (isOk)
                        isOk = CheckConnection.CheckMaxEntity(SessionTools.CS, SessionTools.Collaborator, maxEntityAuthorized, ref errmsg);

                    if (isOk)
                    {
                        SessionTools.AddCSHistory();

                        SessionTools.SetRestriction();

                        //Initialisation de la session pour les copier/coller
                        SessionTools.InitSessionClipBoard();

                        // FI 20240731 [XXXXX] Mise en commentaire => use DataEnabledEnum/DataEnabledEnumHelper. Les enums sont chargés lorsque nécessaire
                        //FI 20120124 Chargement des enums s'il ne l'ont pas été (ils ne sont pas chargés systématiquement)
                        //ExtendEnumsTools.LoadFpMLEnumsAndSchemes(SessionTools.CS);
                    }
                }
                isAuthenticated = isOk;
            }

            if (isAuthenticated)
            {

                if (!IsPostBack)
                {
                    CtrlIdA.Value = SessionTools.Collaborator_IDA.ToString();
                    JavaScript.ScriptOnStartUp(this, "DisplayMaskMenu();", "DisplayMaskMenu");
                }

                // EG 20220123 [26235][WI543] Refactoring de la gestion des actions sur le PASSWORD
                // ---------------------------------------------------------------------------------
                // 1. Cst.ActionOnConnect.VALIDPWDCHANGING    : Enregistrement d'un changement de PASSWORD appelée depuis PROFILE.ASPX ou à la connexion.
                //    Cst.ActionOnConnect.POSTPONEPWDCHANGING : Changement de PASSWORD remis à plus tard,  appelée à la connexion.
                //    Cst.ActionOnConnect.NONE                : RAS.
                //
                //    =>  Chargement des menus et tracker
                //
                //
                // 2. Cst.ActionOnConnect.CHANGEPWD           : l'utilisateur doit changer de PASSWORD à la connexion.
                // 3. Cst.ActionOnConnect.EXPIREDPWD          : le PASSWORD arrive bientôt à expiration, l'utilisateur peut le changer.
                // 
                //    =>  Les menus ne sont pas chargés, la page CHANGEPWD.ASPX est appelée via la méthode JAVASCRIPT (DEFAULT.JS)
                if ((SessionTools.ActionOnConnect == Cst.ActionOnConnect.VALIDPWDCHANGING) || (SessionTools.ActionOnConnect == Cst.ActionOnConnect.POSTPONEPWDCHANGING))
                {
                    SessionTools.ActionOnConnect = Cst.ActionOnConnect.NONE;
                }

                if (SessionTools.ActionOnConnect == Cst.ActionOnConnect.NONE)
                    OnConnectOk();
            }
            else
            {
                //PL 20210118 [25645] Use DistinguishUnknownIdentifierIncorrectPassword
                string idUnknown = Ressource.GetString((BoolFunc.IsTrue(SystemSettings.GetAppSettings("DistinguishUnknownIdentifierIncorrectPassword", "false")))
                                                       ? "FailureConnect_UnknownIdentifier" : "FailureConnect_UnknownIdentifierOrIncorrectPassword");
                CtrlMenu.Visible = false;
                string welcome = HttpContext.Current.User.Identity.Name.ToString();
                welcome += isAuthenticated.ToString();
                //JQuery.OpenDialog openDialog = new JQuery.OpenDialog(this.PageTitle, idUnknown + JavaScript.JS_CrLf + JavaScript.JS_CrLf + welcome.Replace(@"\", @"\\"), ProcessStateTools.StatusErrorEnum);
                //JQuery.UI.WriteInitialisationScripts(this, openDialog);
                // EG 20221121 Affichage message géré via Default.js (Méthode _DisplayMessageAfterOnConnectOk)
                SessionTools.MessageAfterOnConnectOk = new Tuple<string, ProcessStateTools.StatusEnum, string>(this.PageTitle, ProcessStateTools.StatusErrorEnum,
                    idUnknown + JavaScript.JS_CrLf + JavaScript.JS_CrLf + welcome.Replace(@"\", @"\\"));
            }
            BtnMaskMenu.Attributes.Add("onclick", "SwitchMaskMenu();return false;");

            this.DataBind();

        }

        // Affichage et construction des éléments du sommaire
        private void DisplayMenus()
        {
            int levelCurrentGroupMenu = 0;
            int levelLastGroupMenuEnabledDisabled = 0;
            string htmlMenu = string.Empty;
            bool isEnabled = true;
            MaskMenu();

            for (int i = 0; i < menus.Count; i++)
            {
                SpheresMenu.Menu menu = menus[i];
                //System.Diagnostics.Debug.WriteLine(menu.Displayname + " - " + menu.Level.ToString());

                Cst.Visibility mnuVisibility = (Cst.Visibility)Enum.Parse(typeof(Cst.Visibility), menu.Visibility, true);

                bool isSetLastGroupMenuEnabledDisabled;
                if (menu.Level <= levelLastGroupMenuEnabledDisabled)
                {
                    //Menu d'un niveau supérieur au dernier menu Enabled/Disabled 
                    //--> on tient compte de la disponibilité du menu courant
                    isEnabled = (menu.IsEnabled) && (menu.Level > 0);
                    if (isEnabled)
                        isEnabled = (mnuVisibility == Cst.Visibility.SHOW) || (mnuVisibility == Cst.Visibility.MASK);
                    isSetLastGroupMenuEnabledDisabled = true;
                }
                else if (isEnabled)
                {
                    //Menu d'un niveau inférieur au dernier menu Enabled/Disabled ce dernier étant Enabled 
                    //--> on tient compte de la disponibilité du menu courant
                    //NB: Les menus enfants d'un parent "disabled" sont systématiquement considérés "disabled"
                    isEnabled = (menu.IsEnabled) && (menu.Level > 0);
                    if (isEnabled)
                        isEnabled = (mnuVisibility == Cst.Visibility.SHOW) || (mnuVisibility == Cst.Visibility.MASK);
                    isSetLastGroupMenuEnabledDisabled = true;
                }
                else
                    isSetLastGroupMenuEnabledDisabled = false;

                if (isSetLastGroupMenuEnabledDisabled)
                    levelLastGroupMenuEnabledDisabled = menu.Level;

                if (isEnabled)
                {
                    if (menu.Level <= levelCurrentGroupMenu)
                    {
                        //Fermeture des tags DIV des différents groupes de menus précédemment ouverts & décrémentation de "levelCurrentGroupMenu"
                        for (int j = levelCurrentGroupMenu; j >= menu.Level; j--)
                        {
                            //System.Diagnostics.Debug.WriteLine("j:"+j.ToString() + "  levelCurrentGroupMenu:"+levelCurrentGroupMenu.ToString() + "  menu.Level:"+menu.Level.ToString());
                            htmlMenu += Cst.Tab + "</div>" + Cst.CrLf;
                            levelCurrentGroupMenu--;
                        }
                    }
                    levelCurrentGroupMenu = menu.Level;
                    int lastCtrl = 0;

                    Control ctrl = null;
                    for (int j = 1; j <= menu.Level; j++)
                    {
                        ctrl = (1 == j) ? CtrlMenu : ctrl.Controls[lastCtrl].Controls[1];
                        lastCtrl = ctrl.Controls.Count - 1;
                    }
                    if (null != ctrl)
                        ctrl.Controls.Add(menu.MnuPlaceHolder);
                    _ = menu.Level;
                }
            }
        }

        /// <summary>
        /// Affichage des menus principaux dans Default.aspx
        /// </summary>
        /// EG 20230306 [26279] Gestion non affichage des menus non autorisés dans le Summary
        private void DisplayMenusFed()
        {
            var qryMainMenu = from SpheresMenu.Menu item in menus
                        where item.Level == 1
                        select item;

            foreach (SpheresMenu.Menu mainMenu in qryMainMenu)
            {
                // Le menu est autorisé à être affiché
                if (mainMenu.IsDisplaying)
                {
                    Panel pnlContnrMenu = new Panel()
                    {
                        ID = String.Format("contnrmnu{0}", mainMenu.Id),
                        CssClass = (mainMenu.Visibility == Cst.Visibility.MASK.ToString() ? "mask" : "")
                    };
                    mainMenu.MnuPlaceHolder = mainMenu.AddMainMenuControl();

                    DisplaySubMenusFed(mainMenu);

                    pnlContnrMenu.Controls.Add(mainMenu.MnuPlaceHolder);
                    CtrlMenu.Controls.Add(pnlContnrMenu);
                }
            }
        }

        /// <summary>
        /// Affichage des sous-menus dans Default.aspx
        /// </summary>
        /// <param name="pParentMenu"></param>
        /// EG 20220112 [26227][WI537] Exclusion de la création des sous-menus lorsque le menu principal est déclaré comme EXTERNE
        /// EG 20230306 [26279] Gestion non affichage des menus non autorisés dans le Summary
        private void DisplaySubMenusFed(SpheresMenu.Menu pParentMenu)
        {
            var qrySubMenu = from SpheresMenu.Menu item in menus
                              where item.IdMenu_Parent == pParentMenu.IdMenu
                              select item;

            foreach (SpheresMenu.Menu subMenu in qrySubMenu)
            {
                if (subMenu.Visibility != Cst.Visibility.HIDE.ToString())
                {
                    System.Diagnostics.Debug.WriteLine(subMenu.MainMenuName);
                    // Le menu est autorisé à être affiché
                    if (subMenu.IsDisplaying)
                    {
                        if (subMenu.IsAction || subMenu.IsSeparator)
                        {
                            subMenu.MnuPlaceHolder = subMenu.AddActionMenuControl();
                        }
                        else
                        {
                            subMenu.MnuPlaceHolder = subMenu.AddSubMenuControl();
                        }


                        // EG 20220112 [26227][WI537] Add test
                        // EG 20230207 [26227][WI537] Add additional test on subMenu
                        if ((false == pParentMenu.IsExternal) || subMenu.IsExternal)
                            DisplaySubMenusFed(subMenu);

                        pParentMenu.MnuPlaceHolder.Controls[1].Controls.Add(subMenu.MnuPlaceHolder);
                    }
                }
            }
        }
        /// <summary>
        /// Lecture de Menus via SessionTools (pour éviter le rechargement du menu)
        /// </summary>
        /// EG 20210921 [XXXXX] Mise en commentaire de la base target, elle est gérée sur chaque page de login (classic et portail)
        /// EG 20220623 [XXXXX] Implémentation Meznu en fonction de la page par defaut
        /// EG 20221010 [XXXXX] Changement de nom de la page principale : mainDefault en default
        private void OnConnectOk()
        {
            // Les urls associées à cette page (issu de menu) sont ouvertes dans le container main (iFrame central de la page de démarrage par défaut)
            //this.Header.Controls.Add(new LiteralControl(@"<base target=""main""/>"));

            menus = SessionTools.Menus;
            if (String.IsNullOrEmpty(Request.QueryString["default"]))
                DisplayMenus();
            else
                DisplayMenusFed();
            CtrlMenu.Visible = true;

            #region Information message
            string infoMsg = string.Empty;
            string resInfoMsg = string.Empty;
            bool isAbort = false;
            bool isWarning = false;

            #region Check Database Status
            if (SessionTools.Data.DbStatus != Cst.DBStatusEnum.ONLINE.ToString())
            {
                bool isPostUpgrade = (SessionTools.Data.DbStatus == Cst.DBStatusEnum.POSTUPGRADE.ToString());
                bool isWarning_Save = isWarning;
                string infoMsg_Save = infoMsg;

                if (isPostUpgrade)
                {
                    #region POSTUPGRADE
                    LockObject lockObject = new LockObject(TypeLockEnum.SOFTWARE, 0, Cst.DBStatusEnum.POSTUPGRADE.ToString(), LockTools.Exclusive);
                    try
                    {
                        resInfoMsg = "Msg_RDBMSPostUpgrade";

                        bool isError = false;

                        Lock lck = new Lock(SessionTools.CS, lockObject, SessionTools.AppSession, Cst.DBStatusEnum.POSTUPGRADE.ToString());
                        if (LockTools.LockMode1(lck, out Lock lockEx))
                        {
                            isWarning |= true;

                            #region Step 1: Lecture de EFSSOFTWAREUPG

                            string[] dbVersion_Split = SessionTools.Data.DatabaseVersionBuild.Split('.');

                            string sqlSelectUPG = SQLCst.SELECT + "IDEFSSOFTWARE,MAJOR,MINOR,REVISION,BUILD,IDDATA,IDDATAIDENT,IDSTPROCESS,NOTE,DTINS,IDAINS,DTUPD,IDAUPD" + Cst.CrLf;
                            sqlSelectUPG += SQLCst.FROM_DBO + Cst.OTCml_TBL.EFSSOFTWAREUPG.ToString() + Cst.CrLf;

                            string sqlWhereUPG = SQLCst.WHERE + "(IDEFSSOFTWARE=" + DataHelper.SQLString(Software.Name) + ")";
                            sqlWhereUPG += SQLCst.AND + "(IDSTPROCESS=" + DataHelper.SQLString(ProcessStateTools.StatusPending) + ")" + Cst.CrLf;
                            sqlWhereUPG += SQLCst.AND + "( (MAJOR<" + dbVersion_Split[0] + ")" + Cst.CrLf;
                            sqlWhereUPG += "or (MAJOR=" + dbVersion_Split[0] + " and MINOR<" + dbVersion_Split[1] + ")" + Cst.CrLf;
                            sqlWhereUPG += "or (MAJOR=" + dbVersion_Split[0] + " and MINOR=" + dbVersion_Split[1] + " and REVISION<" + dbVersion_Split[2] + ")" + Cst.CrLf;
                            sqlWhereUPG += "or (MAJOR=" + dbVersion_Split[0] + " and MINOR=" + dbVersion_Split[1] + " and REVISION=" + dbVersion_Split[2] + " and BUILD<=" + dbVersion_Split[3] + ") )" + Cst.CrLf;

                            string sqlOrderUPG = SQLCst.ORDERBY + "MAJOR,MINOR,REVISION,BUILD,IDDATAIDENT,IDDATA";

                            DataSet dsUPG = DataHelper.ExecuteDataset(SessionTools.CS, CommandType.Text, sqlSelectUPG + sqlWhereUPG + sqlOrderUPG);
                            DataTable dtUPG = dsUPG.Tables[0];
                            dtUPG.TableName = Cst.OTCml_TBL.EFSSOFTWAREUPG.ToString();
                            #endregion

                            try
                            {
                                if (null != dtUPG.Rows && dtUPG.Rows.Count > 0)
                                {
                                    #region Step 2: Traitement des données
                                    
                                    for (int itemUPG = 0; itemUPG < dtUPG.Rows.Count; itemUPG++)
                                    {
                                        if (dtUPG.Rows[itemUPG]["IDSTPROCESS"].ToString() != ProcessStateTools.StatusEnum.PENDING.ToString())
                                        {
                                            //NB: Certains records, en double, peuvent ne plus être en PENDING, car déjà traités précédemment (cf plus bas dans cette méthode)
                                            continue;
                                        }

                                        try
                                        {
                                            int idData = Convert.ToInt32(dtUPG.Rows[itemUPG]["IDDATA"]);
                                            string idDataIdent = dtUPG.Rows[itemUPG]["IDDATAIDENT"].ToString();
                                            dtUPG.Rows[itemUPG].BeginEdit();
                                            dtUPG.Rows[itemUPG]["IDSTPROCESS"] = ProcessStateTools.StatusEnum.PROGRESS.ToString();
                                            ProcessLogHeader logHeader = null;
                                            switch (idDataIdent)
                                            {
                                                case "MATURITYRULE":
                                                    //new UpdateMaturityRule()
                                                    UpdateMaturityRuleProcess updateMaturityRuleProcess = new UpdateMaturityRuleProcess(SessionTools.CS, SessionTools.AppSession);
                                                    updateMaturityRuleProcess.UpdateMaturitiesAndEvents(idData, out logHeader);
                                                    dtUPG.Rows[itemUPG]["IDSTPROCESS"] = logHeader.Info.status;
                                                    string note = dtUPG.Rows[itemUPG]["NOTE"].ToString();
                                                    if (StrFunc.IsFilled(note) && (false == note.EndsWith(".")))
                                                        note += ".";
                                                    note += StrFunc.AppendFormat(" For more information, please see log (IdLog:{0})", logHeader.IdProcess.ToString());
                                                    dtUPG.Rows[itemUPG]["NOTE"] = note;
                                                    break;
                                            }

                                            dtUPG.Rows[itemUPG]["IDAUPD"] = SessionTools.Collaborator_IDA;
                                            // FI 20200820 [XXXXXX] dates systèmes en UTC
                                            dtUPG.Rows[itemUPG]["DTUPD"] = OTCmlHelper.GetDateSysUTC(SessionTools.CS);

                                            DataRow[] rowsUPG = dtUPG.Select("IDDATAIDENT='" + idDataIdent + "' and IDDATA=" + idData.ToString(), "MAJOR,MINOR,REVISION,BUILD");
                                            if (rowsUPG.Length > 1)
                                            {
                                                for (int r = 1; r < rowsUPG.Length; r++)
                                                {
                                                    rowsUPG[r]["IDSTPROCESS"] = ProcessStateTools.StatusEnum.NONE.ToString();
                                                    rowsUPG[r]["NOTE"] = "Already processed.";
                                                    rowsUPG[r]["IDAUPD"] = dtUPG.Rows[itemUPG]["IDAUPD"];
                                                    rowsUPG[r]["DTUPD"] = dtUPG.Rows[itemUPG]["DTUPD"];
                                                }
                                            }
                                        }
                                        catch (Exception ex)
                                        {
                                            string sNote = ex.Message;
                                            //PL 20240229 Gestion du cas particulier: "No Market Found"
                                            if (sNote == "No Market Found")
                                            {
                                                //MR non référencé par un DC, il est donc impossible de trouver un MARKET. On ne considère pas cela comme une erreur.
                                                dtUPG.Rows[itemUPG]["IDSTPROCESS"] = ProcessStateTools.StatusEnum.WARNING   .ToString();
                                            }
                                            else
                                            {
                                                isError = true;
                                                dtUPG.Rows[itemUPG]["IDSTPROCESS"] = ProcessStateTools.StatusEnum.ERROR.ToString();
                                                SpheresException2 sEx = SpheresExceptionParser.GetSpheresException(string.Empty, ex);
                                                sNote = dtUPG.Rows[itemUPG]["NOTE"].ToString() + StrFunc.AppendFormat($". An exception occurs: {sEx.MessageExtended}");
                                            }

                                            dtUPG.Rows[itemUPG]["NOTE"] = sNote.Substring(0, Math.Min(sNote.Length, SQLCst.UT_NOTE_LEN));
                                            //PL 20240131 Correction
                                            //dtUPG.Rows[itemUPG]["IDAUPD"] = dtUPG.Rows[itemUPG]["IDAUPD"];
                                            //dtUPG.Rows[itemUPG]["DTUPD"] = dtUPG.Rows[itemUPG]["DTUPD"];
                                            dtUPG.Rows[itemUPG]["IDAUPD"] = SessionTools.Collaborator_IDA;
                                            dtUPG.Rows[itemUPG]["DTUPD"] = OTCmlHelper.GetDateSysUTC(SessionTools.CS); 
                                        }
                                        finally
                                        {
                                            dtUPG.Rows[itemUPG].EndEdit();
                                        }
                                    }
                                    
                                    #endregion Step 2
                                }
                            }
                            finally
                            {
                                #region Step 3: Mise à jour de  EFSSOFTWAREUPG
                                int updatedRowsUPG = DataHelper.ExecuteDataAdapter(SessionTools.CS, sqlSelectUPG, dtUPG);
                                if (dtUPG.Rows.Count != updatedRowsUPG)
                                    isError = true;
                                #endregion

                                #region Step 4: Mise à jour de  EFSSOFTWARE
                                string sqlUpdate = SQLCst.UPDATE_DBO + Cst.OTCml_TBL.EFSSOFTWARE.ToString() + SQLCst.SET + "DBSTATUS=@DBSTATUS" + Cst.CrLf;
                                sqlUpdate += SQLCst.WHERE + "IDEFSSOFTWARE=" + DataHelper.SQLString(Software.Name);
                                DataParameters parameters = new DataParameters();
                                parameters.Add(new DataParameter(SessionTools.CS, "DBSTATUS", DbType.AnsiString, SQLCst.UT_ENUM_MANDATORY_LEN),
                                    isError ? Cst.DBStatusEnum.POSTCORRUPTED.ToString() : Cst.DBStatusEnum.ONLINE.ToString());
                                int nRow = DataHelper.ExecuteNonQuery(SessionTools.CS, CommandType.Text, sqlUpdate, parameters.GetArrayDbParameter());
                                #endregion
                            }
                        }
                        else
                        {
                            isAbort |= true;
                        }
                    }
                    finally
                    {
                        // FI 20180908 [XXXXX] Unlock systématique
                        // Il est arrivé dans le passé qu'un lock reste présent suite à une exception, empêchant toute autre excecution du PostUpgrade
                        if (null != lockObject)
                            LockTools.UnLock(SessionTools.CS, lockObject, SessionTools.AppSession.SessionId);
                    }
                    #endregion POSTUPGRADE
                }
                else
                {
                    resInfoMsg = "Msg_RDBMSStatus";
                    isAbort |= (SessionTools.Data.DbStatus != Cst.DBStatusEnum.NA.ToString());
                }

                //PL 20131022 Affichage uniquement dans le cas d'un utilisateur SYSADMIN
                if (isPostUpgrade && !SessionTools.IsSessionSysAdmin)
                {
                    isWarning = isWarning_Save;
                    infoMsg = infoMsg_Save;
                }
                else
                {
                    infoMsg = Ressource.GetString2(resInfoMsg, SessionTools.Data.DatabaseNameVersionBuild, SessionTools.Data.DbStatus);
                }
            }
            #endregion

            #region Check Version Software/Database
            if (SessionTools.Data.DatabaseMajorMinor != Software.MajorMinor)
            {
                //PL 20240209 Temporary - DB v14 compatible WEB v13
                if ((SessionTools.Data.DatabaseMajorMinor == "14.0") && (Software.MajorMinor.StartsWith("13.")))
                {
                    //PL 20240209 Temporary - DB v14 compatible WEB v13 (for HPC|MAREX)
                }
                else
                {
                    if (StrFunc.IsFilled(infoMsg))
                    {
                        infoMsg += Cst.CrLf2 + Cst.CrLf2 + Cst.HTMLBreakLine2 + Cst.HTMLBreakLine2;
                    }
                    infoMsg += Ressource.GetString2("Msg_AppWebAndRDBMSVersionMismatch", Software.NameVersionBuild, SessionTools.Data.DatabaseNameVersionBuild);
                    isAbort |= (SessionTools.Data.DatabaseMajorMinor != Software.MajorMinor);
                }
            }
            #endregion

            #region Check Version CLR, RDBMS, ...
            int CLRMajor, CLRMinor, CLRBuild, CLRRevision;
            CLRMajor = Environment.Version.Major;
            CLRMinor = Environment.Version.Minor;
            CLRBuild = Environment.Version.Build;
            CLRRevision = Environment.Version.Revision;
            bool isAvailableCLR = ((CLRMajor == 4 && CLRMinor == 0 && ((CLRBuild > 30319) || (CLRBuild == 30319 && CLRRevision >= 34209))));
            if (false == isAvailableCLR)
            {
                if (StrFunc.IsFilled(infoMsg))
                {
                    infoMsg += Cst.CrLf2 + Cst.CrLf2 + Cst.HTMLBreakLine2 + Cst.HTMLBreakLine2;
                }
                // EG 20130906
                infoMsg += Ressource.GetString2("Msg_DotNetNotAvailable", Software.NameVersionBuild,
                    CLRMajor.ToString() + "." + CLRMinor.ToString() + "." + CLRBuild.ToString() + "." + CLRRevision.ToString());
                isAbort |= (CLRMajor == 1);
            }
            #endregion

            #region Check Version IE (Memory leak)
            // EG 20120625 Ticket: 17901 - Memory Leakwith IE < 9 
            bool isIE_LT9 = (HttpContext.Current.Request.Browser.Browser == "IE") && (HttpContext.Current.Request.Browser.MajorVersion < 9);
            if (isIE_LT9)
            {
                string browserInfo = HttpContext.Current.Request.Browser.Browser;
                browserInfo += HttpContext.Current.Request.Browser.MajorVersion.ToString() + ".";
                browserInfo += HttpContext.Current.Request.Browser.MinorVersion.ToString() + " - ";
                infoMsg += Cst.CrLf2 + Cst.HTMLBreakLine2;
                infoMsg += Ressource.GetString2("Msg_AppWebIEVersionMemoryLeak", browserInfo);
                infoMsg += Cst.CrLf2 + Cst.HTMLBreakLine2;
                isWarning |= isIE_LT9;
            }
            #endregion Check Version IE (Memory leak)

            #region WelcomeMsg
            if (StrFunc.IsFilled(SessionTools.WelcomeMsg))
            {
                if (StrFunc.IsFilled(infoMsg))
                {
                    infoMsg += Cst.CrLf2 + Cst.CrLf2 + Cst.HTMLBreakLine2 + Cst.HTMLBreakLine2;
                }
                infoMsg += SessionTools.WelcomeMsg;
                //
                SessionTools.WelcomeMsg = string.Empty;
            }
            #endregion

            if (StrFunc.IsFilled(infoMsg))
            {
                //JQuery.OpenDialog openDialog = new JQuery.OpenDialog(this.PageFullTitle, infoMsg.Replace(Cst.CrLf, Cst.HTMLBreakLine), isAbort ? ProcessStateTools.StatusErrorEnum :
                //isWarning ? ProcessStateTools.StatusWarningEnum : ProcessStateTools.StatusNoneEnum);
                //openDialog.Height = "150";
                //openDialog.Width = "300";
                //JQuery.UI.WriteInitialisationScripts(this, openDialog);
                ProcessStateTools.StatusEnum status = isAbort ? ProcessStateTools.StatusErrorEnum : isWarning ? ProcessStateTools.StatusWarningEnum : ProcessStateTools.StatusNoneEnum;
                SessionTools.MessageAfterOnConnectOk = new Tuple<string, ProcessStateTools.StatusEnum, string>(this.PageFullTitle, status, infoMsg.Replace(Cst.CrLf, Cst.HTMLBreakLine));
            }
            #endregion Information message
        }

        protected void MaskMenu()
        {
            if (!IsPostBack)
            {
                AspTools.ReadSQLCookie("MaskMenu", out string cookieValue);
                if (StrFunc.IsFilled(cookieValue))
                    CtrlMaskMenu.Value = cookieValue;
                else
                {
                    CtrlMaskMenu.Value = "in";
                    AspTools.WriteSQLCookie("MaskMenu", CtrlMaskMenu.Value);
                }
            }
            else
            {
                AspTools.WriteSQLCookie("MaskMenu", CtrlMaskMenu.Value);
            }
        }
    }
}
