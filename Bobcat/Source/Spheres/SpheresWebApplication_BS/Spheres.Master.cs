#region using directives
using EFS.ACommon;
using EFS.Common;
using EFS.Common.Web;
//using EFSSpheres;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using SpheresMenu = EFS.Common.Web.Menu;
#endregion using directives

namespace EFS.Spheres
{
    public partial class SpheresMaster : MasterPage
    {
        //private const string AntiXsrfTokenKey = "__AntiXsrfToken";
        //private const string AntiXsrfUserNameKey = "__AntiXsrfUserName";
        //private string _antiXsrfTokenValue;
        private List<SpheresMenu.Menu> lstMenus;
        private List<CounterMatriceMenu> mainMenus;
        private bool isDebug = false;

        private void Page_Error(object sender, EventArgs e)
        {
            // Get last error from the server
            Exception exc = Server.GetLastError();

            // Filter for a specific kind of exception
            if (exc is ArgumentOutOfRangeException)
            {
                // Give the user some information, but
                // stay on the default page
                Response.Write("<h2>Default Page Error</h2>\n");
                Response.Write("<p>ArgumentOutOfRange: Your click must have " + "been out of range?!</p>\n");
                Response.Write("Return to the <a href='Default.aspx'>" + "Default Page</a>\n");

                // Log the exception and notify system operators
                ExceptionUtility.LogException(exc, "DefaultPage");
                ExceptionUtility.NotifySystemOps(exc);

                // Clear the error from the server
                Server.ClearError();

            }
            // Filter for other kinds of exceptions
            else if (exc is InvalidOperationException)
            {
                // Pass the error on to the Generic Error page
                Server.Transfer("ErrorPage.aspx", true);
            }
            else
            {
                // Pass the error on to the default global handler
            }
        }
        protected void Page_Init(object sender, EventArgs e)
        {
            ////  Le code ci-dessous vous aide à vous protéger contre les attaques XSRF
            //var requestCookie = Request.Cookies[AntiXsrfTokenKey];
            //Guid requestCookieGuidValue;
            //if (requestCookie != null && Guid.TryParse(requestCookie.Value, out requestCookieGuidValue))
            //{
            //    // Utilisez les jetons anti-XSRF à partir du cookie
            //    _antiXsrfTokenValue = requestCookie.Value;
            //    Page.ViewStateUserKey = _antiXsrfTokenValue;
            //}
            //else
            //{
            //    // Générer un nouveau jeton anti-XSRF et enregistrer le cookie
            //    _antiXsrfTokenValue = Guid.NewGuid().ToString("N");
            //    Page.ViewStateUserKey = _antiXsrfTokenValue;

            //    var responseCookie = new HttpCookie(AntiXsrfTokenKey)
            //    {
            //        HttpOnly = true,
            //        Value = _antiXsrfTokenValue
            //    };
            //    if (FormsAuthentication.RequireSSL && Request.IsSecureConnection)
            //    {
            //        responseCookie.Secure = true;
            //    }
            //    Response.Cookies.Set(responseCookie);
            //}
            Page.PreLoad += master_Page_PreLoad;
        }

        protected void master_Page_PreLoad(object sender, EventArgs e)
        {
            //if (!IsPostBack)
            //{
            //    // Définir le jeton anti-XSRF
            //    ViewState[AntiXsrfTokenKey] = Page.ViewStateUserKey;
            //    ViewState[AntiXsrfUserNameKey] = Context.User.Identity.Name ?? String.Empty;
            //}
            //else
            //{
            //    // Valider le jeton anti-XSRF
            //    if ((string)ViewState[AntiXsrfTokenKey] != _antiXsrfTokenValue
            //        || (string)ViewState[AntiXsrfUserNameKey] != (Context.User.Identity.Name ?? String.Empty))
            //    {
            //        throw new InvalidOperationException("Échec de la validation du jeton anti-XSRF.");
            //    }
            //}
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            hidMaskMenu.Attributes["title"] = Ressource.GetString("imgBtnMaskMenu");
            Page.ClientScript.RegisterHiddenField("__SOFTWARE", Software.Name);
            Page.ClientScript.RegisterHiddenField("__ROLE", SessionTools.Collaborator_ROLE);

            bool isAuthenticated = false;
            if (HttpContext.Current.User.Identity.AuthenticationType == "Forms")
            {
                isAuthenticated = Request.IsAuthenticated;
            }
            else if ((HttpContext.Current.User.Identity.AuthenticationType == "NTLM") || (HttpContext.Current.User.Identity.AuthenticationType == "Negotiate"))
            {
            }

            if (isAuthenticated)
            {
                if (!IsPostBack)
                {
                    hidIDA.Value = SessionTools.Collaborator_IDA.ToString();
                    ScriptOnStartUp("DisplayMaskMenu();", "DisplayMaskMenu");
                }

                switch (SessionTools.ActionOnConnect)
                {
                    case Cst.ActionOnConnect.NONE:
                        OnConnectOk();
                        break;
                    case Cst.ActionOnConnect.CHANGEPWD:
#warning TODO : ChangePwd.aspx (CHANGEPWD)
                        SessionTools.ActionOnConnect = Cst.ActionOnConnect.NONE;
                        break;
                    case Cst.ActionOnConnect.EXPIREDPWD:
#warning TODO : ChangePwd.aspx (EXPIREDPWD)
                        SessionTools.ActionOnConnect = Cst.ActionOnConnect.NONE;
                        break;
                }
            }
            else
            {
                //string idUnknown = Ressource.GetString("FailureConnect_UnknownIdentifier");
                //string welcome = HttpContext.Current.User.Identity.Name.ToString();
                //JQuery.OpenDialog openDialog = new JQuery.OpenDialog(this.PageTitle, idUnknown + JavaScript.JS_CrLf + JavaScript.JS_CrLf + welcome.Replace(@"\", @"\\"), ProcessStateTools.StatusErrorEnum);
                //JQuery.UI.WriteInitialisationScripts(this, openDialog);
            }

            //this.DataBind();
        }

        // EG 20120207 Lecture de Menus via SessionTools (pour éviter le rechargement du menu)
        private void OnConnectOk()
        {
            DisplayMenus();

            #region Information message
            string infoMsg = string.Empty;
            string resInfoMsg = string.Empty;
            bool isAbort = false;

            #region Check Database Status
#warning Check Database Status
            #endregion

            #region Check Version Software/Database
            if (SessionTools.Data.DatabaseMajorMinor != Software.MajorMinor)
            {
                if (StrFunc.IsFilled(infoMsg))
                {
                    infoMsg += Cst.CrLf2 + Cst.CrLf2 + Cst.HTMLBreakLine2 + Cst.HTMLBreakLine2;
                }
                infoMsg += Ressource.GetString2("Msg_AppWebAndRDBMSVersionMismatch", Software.NameVersionBuild, SessionTools.Data.DatabaseNameVersionBuild);
                isAbort |= (SessionTools.Data.DatabaseMajorMinor != Software.MajorMinor);
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
                    infoMsg += Cst.CrLf2 + Cst.CrLf2 + Cst.HTMLBreakLine2 + Cst.HTMLBreakLine2;

                infoMsg += Ressource.GetString2("Msg_DotNetNotAvailable", Software.NameVersionBuild,
                    CLRMajor.ToString() + "." + CLRMinor.ToString() + "." + CLRBuild.ToString() + "." + CLRRevision.ToString());
                isAbort |= (CLRMajor == 1);
            }
            #endregion

            #region WelcomeMsg
            if (StrFunc.IsFilled(SessionTools.WelcomeMsg))
            {
                if (StrFunc.IsFilled(infoMsg))
                    infoMsg += Cst.CrLf2 + Cst.CrLf2 + Cst.HTMLBreakLine2 + Cst.HTMLBreakLine2;
                infoMsg += SessionTools.WelcomeMsg;
                SessionTools.WelcomeMsg = string.Empty;
            }
            #endregion

            if (StrFunc.IsFilled(infoMsg))
            {
                //JQuery.OpenDialog openDialog = new JQuery.OpenDialog(this.PageFullTitle, infoMsg.Replace(Cst.CrLf, Cst.HTMLBreakLine), isAbort ? ProcessStateTools.StatusErrorEnum :
                //    isWarning ? ProcessStateTools.StatusWarningEnum : ProcessStateTools.StatusNoneEnum);
                //openDialog.Height = "150";
                //openDialog.Width = "300";
                //JQuery.UI.WriteInitialisationScripts(this, openDialog);
            }
            #endregion Information message
        }


        protected void Logout(object sender, System.EventArgs e)
        {
            //EFS.Common.Web.Disconnect.Run(this.Page);
            if (SessionTools.IsSessionAvailable && StrFunc.IsFilled(SessionTools.CS))
            {
                //On rentre dans ce cas lorsque l'utilisateur se déconnecte via le bouton logOut         
                if (SessionTools.IsConnected)
                {
                    HttpConnectedUsers acu = new HttpConnectedUsers(HttpContext.Current.Application);
                    acu.RemoveConnectedUser(SessionTools.Collaborator_IDA);
                }

                SessionTools.Dispose();

                Login_Log.Logout(SessionTools.CS, SessionTools.Collaborator_IDA);
                SessionTools.ProcessLogAddLoginInfo(ProcessStateTools.StatusSuccessEnum, ActionLogin.Logout);

                SessionTools.LastConnectionString = SessionTools.CS;
                SessionTools.ResetLoginInfos();
                SessionTools.ConnectionState = Cst.ConnectionState.LOGOUT;
            }
            FormsAuthentication.SignOut();
            Page.Response.Redirect("default.aspx", true);
        }

        protected override void OnPreRender(System.EventArgs e)
        {
            base.OnPreRender(e); ;
            //if (SessionTools.IsConnected)
            //{
            //    DisplayMenus();
            //}
        }

        private void DisplayMenus()
        {
            SpheresMenu.Menus menus = SessionTools.Menus;
            // EG TO DO
            if ((null != menus) && (0 < menus.Count))
            {
                lstMenus = new System.Collections.Generic.List<SpheresMenu.Menu>(menus.Count);
                foreach (SpheresMenu.Menu item in menus)
                {
                    lstMenus.Add(item);
                }

                LoginView loginViewTop = (LoginView)FindControl("loginViewTop");
                PlaceHolder plhMainMenu = loginViewTop.FindControl("plhMnuSpheres") as PlaceHolder;

                if (null != plhMainMenu)
                {
                    MaskMenu();
                    // Construction Listes des menus hérarchisées pour interface
                    // Compteur Menu, Siblings...
                    List<SpheresMenu.Menu> lstMainMenus = lstMenus.FindAll(item => item.Level == 1 && ("HIDE" != item.Visibility));
                    mainMenus = new List<CounterMatriceMenu>();
                    lstMainMenus.ForEach(item =>
                    {
                        CounterMatriceMenu counterMatriceMenu = SetMatriceMenus(null, item);
                        counterMatriceMenu.SetSibling();
                        counterMatriceMenu.SetCssClass();
                        mainMenus.Add(counterMatriceMenu);
                    });

                    // Génération des contrôles HTML
                    if (null != mainMenus)
                    {
                        mainMenus.ForEach(
                            mainItem =>
                            {
                                SpheresMenu.Menu menu = lstMenus.Find(item => item.IdMenu == mainItem.IdMenu);

                                if (Cst.Visibility.HIDE != mainItem.Visibility)
                                {
                                    HtmlGenericControl ul = new HtmlGenericControl("ul");
                                    ul.ID = "mastermnu-" + menu.Id;

                                    ul.Attributes.Add("class", (Cst.Visibility.MASK == mainItem.Visibility) ? "maskmenu nav navbar-nav" : "nav navbar-nav");
                                    ul.Attributes.Add("idMenu", mainItem.IdMenu);
                                    ul.Attributes.Add("idMenuParent", mainItem.IdMenu_Parent);

                                    HtmlGenericControl li = new HtmlGenericControl("li");

                                    if (mainItem.IsEnabled)
                                        li.Attributes.Add("class", "dropdown sph-menu");
                                    else if (false == mainItem.IsEnabled)
                                        li.Attributes.Add("class", "disabled");

                                    ul.Controls.Add(li);

                                    HyperLink lnk = new HyperLink();
                                    lnk.Attributes.Add("class", "dropdown-toggle");
                                    lnk.Attributes.Add("data-toggle", "dropdown");


                                    lnk.Text = menu.Displayname;
                                    lnk.NavigateUrl = "#0";
                                    li.Controls.Add(lnk);

                                    ConstructMenu(mainItem, li);
                                    plhMainMenu.Controls.Add(ul);

                                }

                            });
                    }
                }
            }
        }

        private void ConstructMenu(CounterMatriceMenu pMainMenu, HtmlGenericControl pParentControl)
        {
            List<SpheresMenu.Menu> lstChildMenus = lstMenus.FindAll(item => (item.IdMenu_Parent == pMainMenu.IdMenu) && (false == item.IsSeparator) && ("HIDE" != item.Visibility));

            if ((null != lstChildMenus) && (0 < lstChildMenus.Count()))
            {
                switch (pMainMenu.Level)
                {
                    case 1:
                        #region Parent : Level 1 => Traitement des menus Level 2

                        HtmlGenericControl uldropdown = new HtmlGenericControl("ul");
                        uldropdown.Attributes.Add("class", "dropdown-menu sph-menu-item row");

                        lstChildMenus.ForEach(item =>
                        {

                            CounterMatriceMenu currentMatriceMenu = pMainMenu[item.IdMenu];

                            HtmlGenericControl lidropdown = new HtmlGenericControl("li");
                            lidropdown.Attributes.Add("class", (Cst.Visibility.MASK == currentMatriceMenu.Visibility ? "maskmenu " : "") + currentMatriceMenu.CssClass);
                            uldropdown.Controls.Add(lidropdown);

                            HtmlGenericControl ul = new HtmlGenericControl("ul");
                            lidropdown.Controls.Add(ul);

                            if (item.IsEnabled)
                            {
                                bool hasChildren = lstMenus.Exists(child => child.IdMenu_Parent == item.IdMenu &&
                                    ((Cst.Visibility)Enum.Parse(typeof(Cst.Visibility), child.Visibility, true) != Cst.Visibility.HIDE));

                                HtmlGenericControl li = new HtmlGenericControl("li");
                                ul.Controls.Add(li);

                                if (item.IsAction)
                                {
                                    HyperLink lnk = new HyperLink();
                                    if (currentMatriceMenu.IsExternal)
                                        lnk.Attributes.Add("class", "external");
                                    lnk.NavigateUrl = GetFullUrlMenu(item);
                                    lnk.Text = item.Displayname;
                                    if (isDebug)
                                    {
                                        lnk.Text = currentMatriceMenu.Level.ToString() + "-" + lnk.Text;
                                        if (currentMatriceMenu.Level < 4)
                                            lnk.Text += " (" + currentMatriceMenu.NbSubChildrens.ToString() + "-" + currentMatriceMenu.NbMenu.ToString() + ")";
                                    }
                                    li.Controls.Add(lnk);
                                }

                                if (hasChildren)
                                {
                                    li.Attributes.Add("class", "dropdown-header " + "sph-menu-lvl-" + item.Level.ToString() + (currentMatriceMenu.IsExternal ? " external" : ""));

                                    if (false == item.IsAction)
                                    {
                                        Label lbl = new Label();
                                        if (currentMatriceMenu.IsExternal)
                                            lbl.Attributes.Add("class", "external");

                                        lbl.Text = @"<span class='glyphicon glyphicon-" + (currentMatriceMenu.IsExternal ? "bookmark" : "th-list") + "'></span> " + item.Displayname;
                                        if (isDebug)
                                        {
                                            lbl.Text = currentMatriceMenu.Level.ToString() + "-" + lbl.Text;
                                            if (currentMatriceMenu.Level < 4)
                                                lbl.Text += " (" + currentMatriceMenu.NbSubChildrens.ToString() + "-" + currentMatriceMenu.NbMenu.ToString() + ")";
                                        }
                                        li.Controls.Add(lbl);
                                    }
                                    ConstructMenu(currentMatriceMenu, li);
                                }
                            }
                            else
                            {
                                HtmlGenericControl li = new HtmlGenericControl("li");
                                li.Attributes.Add("class", (Cst.Visibility.MASK == currentMatriceMenu.Visibility ? "maskmenu disabled" : "disabled"));
                                ul.Controls.Add(li);
                            }
                            pParentControl.Controls.Add(uldropdown);
                        });
                        #endregion Parent : Level 1 => Traitement des menus Level 2
                        break;
                    default:
                        #region Parent : Level 2,3,4 => Traitement des menus Level 3,4,5
                        HtmlGenericControl ul3 = new HtmlGenericControl("ul");
                        ul3.Attributes.Add("class", "row");

                        lstChildMenus.ForEach(item =>
                        {
                            CounterMatriceMenu currentMatriceMenu = pMainMenu[item.IdMenu];

                            HtmlGenericControl lidropdown = new HtmlGenericControl("li");
                            lidropdown.Attributes.Add("class", (Cst.Visibility.MASK == currentMatriceMenu.Visibility ? "maskmenu " : " " + item.IdMenu + " ") + currentMatriceMenu.CssClass);
                            ul3.Controls.Add(lidropdown);

                            HtmlGenericControl ul = new HtmlGenericControl("ul");
                            lidropdown.Controls.Add(ul);

                            if (item.IsEnabled)
                            {

                                bool hasChildren = lstMenus.Exists(child => child.IdMenu_Parent == item.IdMenu &&
                                    ((Cst.Visibility)Enum.Parse(typeof(Cst.Visibility), child.Visibility, true) != Cst.Visibility.HIDE));

                                HtmlGenericControl li = new HtmlGenericControl("li");
                                ul.Controls.Add(li);

                                if (item.IsAction)
                                {
                                    HyperLink lnk = new HyperLink();
                                    if (currentMatriceMenu.IsExternal)
                                        lnk.Attributes.Add("class", "external");
                                    lnk.NavigateUrl = GetFullUrlMenu(item);
                                    //lnk.NavigateUrl = HttpUtility.UrlEncode(GetFullUrlMenu(item), System.Text.Encoding.Default);
                                    lnk.Text = item.Displayname;
                                    if (isDebug)
                                    {
                                        lnk.Text = currentMatriceMenu.Level.ToString() + "-" + lnk.Text;
                                        if (currentMatriceMenu.Level < 4)
                                            lnk.Text += " (" + currentMatriceMenu.NbSubChildrens.ToString() + "-" + currentMatriceMenu.NbMenu.ToString() + ")";
                                    }

                                    li.Controls.Add(lnk);
                                }

                                if (hasChildren)
                                {
                                    li.Attributes.Add("class", "dropdown-header sph-menu-lvl-" + item.Level.ToString() + (currentMatriceMenu.IsExternal ? " external" : ""));
                                    if (false == item.IsAction)
                                    {
                                        Label lbl = new Label();
                                        lbl.Text = item.Displayname;
                                        if (isDebug)
                                        {
                                            lbl.Text = currentMatriceMenu.Level.ToString() + "-" + lbl.Text;
                                            if (currentMatriceMenu.Level < 4)
                                                lbl.Text += " (" + currentMatriceMenu.NbSubChildrens.ToString() + "-" + currentMatriceMenu.NbMenu.ToString() + ")";
                                        }
                                        li.Controls.Add(lbl);
                                    }
                                    ConstructMenu(currentMatriceMenu, li);
                                }
                            }
                            else
                            {
                                HtmlGenericControl li = new HtmlGenericControl("li");
                                li.Attributes.Add("class", (Cst.Visibility.MASK == currentMatriceMenu.Visibility ? "maskmenu disabled" : "disabled"));
                                ul.Controls.Add(li);
                            }

                            pParentControl.Controls.Add(ul3);
                        });
                        #endregion Parent : Level 1 => Traitement des menus Level 2
                        break;
                }
            }
        }

        private string GetFullUrlMenu(SpheresMenu.Menu pMenu)
        {
            string urlMenu = pMenu.Url;
            string addingForUrlMenu = string.Empty;
            bool isAddIDMenu = true;
            if (pMenu.IsExternal)
            {
                if (pMenu.Url.ToUpper().StartsWith("LISTVIEWER.ASPX"))
                {
                    addingForUrlMenu = "&TitleMenu=" + System.Web.HttpUtility.UrlEncode(pMenu.Description, System.Text.Encoding.UTF8);
                }
                else
                {
                    isAddIDMenu = false;
                }
            }
            if (isAddIDMenu)
            {
                urlMenu += ((urlMenu.IndexOf("?") < 0) ? "?" : "&") + "IDMenu=" + pMenu.IdMenu + addingForUrlMenu;
                if (urlMenu.Contains("%%IDMENUSYS%%"))
                    urlMenu = urlMenu.Replace("%%IDMENUSYS%%", pMenu.Id);
                urlMenu += "&IDMenuSys=" + pMenu.Id;
            }
            return urlMenu;
        }
        private void MaskMenu()
        {
            string maskValue = hidMaskMenu.Value;
            if (!IsPostBack)
            {
                string cookieValue = string.Empty;
                AspTools.ReadSQLCookie("MaskMenu", out cookieValue);
                if (StrFunc.IsFilled(cookieValue))
                    hidMaskMenu.Value = cookieValue;
                else
                {
                    hidMaskMenu.Value = "in";
                    AspTools.WriteSQLCookie("MaskMenu", hidMaskMenu.Value);
                }
            }
            else
            {
                AspTools.WriteSQLCookie("MaskMenu", hidMaskMenu.Value);
            }
        }

        private class CounterMatriceMenu
        {
            /// <summary>
            /// Id unique non signicatif du menu
            /// </summary>
            public string Id { get; set; }
            /// <summary>
            /// Id Spheres du menu
            /// </summary>
            public string IdMenu { get; set; }
            /// <summary>
            /// Id PARENT Spheres du menu
            /// </summary>
            public string IdMenu_Parent { get; set; }
            /// <summary>
            /// Niveau du menu
            /// </summary>
            public int Level { get; set; }
            /// <summary>
            /// Détermine si le menu possède des enfants
            /// </summary>
            public bool HasChildrens { get; set; }
            /// <summary>
            /// Liste des menus enfants
            /// </summary>
            public List<CounterMatriceMenu> counterMatriceMenu { get; set; }
            public string CssClass { get; set; }
            /// <summary>
            /// Détermine si le menu est externe
            /// </summary>
            public bool IsExternal { get; set; }
            /// <summary>
            /// Détermine si le menu est enabled
            /// </summary>
            public bool IsEnabled { get; set; }
            /// <summary>
            /// Détermine si la visibilité du menu
            /// </summary>
            public Cst.Visibility Visibility { get; set; }


            public int NbSubChildrens
            {
                get
                {
                    int total = 0;
                    if (null != counterMatriceMenu)
                        total = counterMatriceMenu.Count(item => item.HasChildrens);
                    return total;
                }
            }
            public int NbMenu
            {
                get
                {
                    int total = 0;
                    if (null != counterMatriceMenu)
                        total = CalcNbMenu(total);
                    return total;
                }
            }
            public CounterMatriceMenu PrevSibling { get; set; }
            public CounterMatriceMenu NextSibling { get; set; }
            public CounterMatriceMenu Parent { get; set; }

            /// <summary>
            /// Fonction récursive de calcul total de menus enfants (toutes descendance)
            /// </summary>
            /// <param name="pPreviousTotal"></param>
            /// <returns></returns>
            private int CalcNbMenu(int pPreviousTotal)
            {
                int total = pPreviousTotal;
                if (null != counterMatriceMenu)
                {
                    counterMatriceMenu.ForEach(item => total += item.CalcNbMenu(1));
                }
                return total;
            }

            /// <summary>
            /// Retourne le suffixe (Numérique) de la classe container des menus
            /// voir Bootstrap
            /// </summary>
            public int CssClassSuffix
            {
                get
                {
                    int suffix = 12;
                    if (StrFunc.IsFilled(CssClass))
                        suffix = Convert.ToInt32(CssClass.Replace("col-md-", string.Empty));
                    return suffix;
                }
            }

            /// <summary>
            /// Retourne le suffixe par defaut de la classe container des menus en fonction du nombre de sous-menus
            /// voir Bootstrap
            /// </summary>
            public int DefaultCssClass
            {
                get
                {
                    int suffix = 12;
                    switch (NbSubChildrens)
                    {
                        case 1:
                        case 2:
                        case 3:
                        case 4:
                        case 6:
                            suffix = 12 / NbSubChildrens;
                            break;
                        case 5:
                        case 7:
                        case 8:
                        case 9:
                        case 10:
                            suffix = 4;
                            break;
                    }
                    if (2 <= Level)
                    {
                        if ((12 / suffix) >= CssClassSuffix)
                            suffix = 6;
                    }

                    return suffix;
                }
            }




            public void SetSibling()
            {
                if (null != counterMatriceMenu)
                {
                    for (int i = 0; i < counterMatriceMenu.Count(); i++)
                    {
                        if (0 < i)
                            counterMatriceMenu[i].PrevSibling = counterMatriceMenu[i - 1];
                        if (i < counterMatriceMenu.Count() - 1)
                            counterMatriceMenu[i].NextSibling = counterMatriceMenu[i + 1];
                        counterMatriceMenu[i].SetSibling();
                    }
                }
            }

            public void SetCssClass()
            {
                string defaultCssClass = "col-md-12";
                if ((1 < Level) && (Level < 4) && (0 < NbMenu))
                    defaultCssClass = CalculCssClass();

                CssClass = defaultCssClass;
                if (null != counterMatriceMenu)
                    counterMatriceMenu.ForEach(item => { item.SetCssClass(); });
            }

            public string CalculCssClass()
            {
                int suffix = Parent.DefaultCssClass;
                if (12 != suffix)
                {
                    switch (Level)
                    {
                        case 1:
                            break;
                        case 2:
                            suffix = CorrectingCssClass(suffix);
                            break;
                        case 3:
                            break;
                        case 4:
                            break;
                        case 5:
                            break;
                        case 6:
                            break;
                    }
                }
                return "col-md-" + suffix.ToString();
            }

            private int CorrectingCssClass(int pSuffix)
            {
                int suffix = pSuffix;
                int index = Parent.counterMatriceMenu.FindIndex(item => item.IdMenu == IdMenu);
                int remainder;
                Math.DivRem(index, 12 / pSuffix, out remainder);
                switch (remainder)
                {
                    case 0:
                        suffix = CorrectingCssClassStart(suffix);
                        break;
                    case 1:
                        if (6 == suffix)
                            suffix = CorrectingCssClassEnd(suffix);
                        else
                            suffix = CorrectingCssClassIntermediary(suffix);
                        break;
                    case 2:
                        if (4 == suffix)
                            suffix = CorrectingCssClassEnd(suffix);
                        else
                            suffix = CorrectingCssClassIntermediary(suffix);
                        break;
                    case 3:
                        if (3 == suffix)
                            suffix = CorrectingCssClassEnd(suffix);
                        else
                            suffix = CorrectingCssClassIntermediary(suffix);
                        break;
                    case 4:
                        if (3 == suffix)
                            suffix = CorrectingCssClassEnd(suffix);
                        else
                            suffix = CorrectingCssClassIntermediary(suffix);
                        break;
                    case 5:
                        if (2 == suffix)
                            suffix = CorrectingCssClassEnd(suffix);
                        else
                            suffix = CorrectingCssClassIntermediary(suffix);
                        break;
                }
                return suffix;
            }

            private int CorrectingCssClassStart(int pSuffix)
            {
                int suffix = pSuffix;
                if (NbSubChildrens <= 1)
                    suffix = 2;
                return suffix;
            }
            private int CorrectingCssClassIntermediary(int pSuffix)
            {
                int suffix = pSuffix;
                if (NbSubChildrens <= 1)
                    suffix = 2;
                return suffix;
            }
            private int CorrectingCssClassEnd(int pSuffix)
            {
                int suffix = pSuffix;
                if (null != PrevSibling)
                {
                    int colReserved = 12 - PrevSibling.CssClassSuffix;
                    if ((null != PrevSibling.PrevSibling))
                        colReserved -= PrevSibling.PrevSibling.CssClassSuffix;
                    suffix = Math.Min(suffix, colReserved);
                }
                return suffix;
            }

            public CounterMatriceMenu() { }

            #region Indexors
            /// <summary>
            /// Recherche dans la matrice un menu par son Id
            /// </summary>
            /// <param name="pIndex"></param>
            /// <returns></returns>
            public CounterMatriceMenu this[string pIdMenu]
            {
                get
                {
                    CounterMatriceMenu matrice = null;
                    if (pIdMenu == IdMenu)
                    {
                        matrice = this;
                    }
                    else if (HasChildrens)
                    {
                        matrice = counterMatriceMenu.Find(item => item.IdMenu == pIdMenu);
                        if (null == matrice)
                        {
                            counterMatriceMenu.ForEach(item =>
                            {
                                if (null != item[pIdMenu])
                                    matrice = item[pIdMenu];

                            });
                        }
                    }
                    return matrice;
                }
            }
            #endregion Indexors
        }

        /// <summary>
        /// Construction des matrices de menus pour construction HTML
        /// </summary>
        /// <param name="pParentCounterMatriceMenu"></param>
        /// <param name="pParentMenu"></param>
        /// <returns></returns>
        private CounterMatriceMenu SetMatriceMenus(CounterMatriceMenu pParentCounterMatriceMenu, SpheresMenu.Menu pParentMenu)
        {

            CounterMatriceMenu matrice = new CounterMatriceMenu();
            // On séléctionne les menus enfants (on exclus les menus HIDE et les SEPARATEUR)
            List<SpheresMenu.Menu> lstChildMenus = lstMenus.FindAll(item => (item.IdMenu_Parent == pParentMenu.IdMenu) && (false == item.IsSeparator) && ("HIDE" != item.Visibility));
            matrice.HasChildrens = (null != lstChildMenus) && (0 < lstChildMenus.Count());
            matrice.Id = pParentMenu.Id;
            matrice.IdMenu = pParentMenu.IdMenu;
            matrice.IdMenu_Parent = pParentMenu.IdMenu_Parent;
            matrice.Level = pParentMenu.Level;
            matrice.Parent = pParentCounterMatriceMenu;
            matrice.IsExternal = pParentMenu.IsExternal;
            matrice.IsEnabled = pParentMenu.IsEnabled;
            matrice.Visibility = (Cst.Visibility)Enum.Parse(typeof(Cst.Visibility), pParentMenu.Visibility, true);

            if (matrice.HasChildrens)
            {
                matrice.counterMatriceMenu = new List<CounterMatriceMenu>();
                // Récursivité : Construction de la hiérarchie 
                lstChildMenus.ForEach(item => { matrice.counterMatriceMenu.Add(SetMatriceMenus(matrice, item)); });
            }
            return matrice;
        }

        // DEBUG
        private void DisplayCounter(CounterMatriceMenu pCounterMatriceMenu)
        {
            //System.Diagnostics.Debug.WriteLine(pCounterMatriceMenu.Id.ToString() + "*" + pCounterMatriceMenu.IdMenu.ToString() + "*" + pCounterMatriceMenu.IdMenu_Parent.ToString() + "*"
            //    + pCounterMatriceMenu.NbSubChildrens.ToString() + "*" + pCounterMatriceMenu.NbMenu.ToString() + "*"
            //    + ((null != pCounterMatriceMenu.PrevSibling) ? pCounterMatriceMenu.PrevSibling.IdMenu : "-") + "*"
            //    + ((null != pCounterMatriceMenu.NextSibling) ? pCounterMatriceMenu.NextSibling.IdMenu : "-") + "*"
            //    + pCounterMatriceMenu.CssClass);

            if (null != pCounterMatriceMenu.counterMatriceMenu)
            {
                pCounterMatriceMenu.counterMatriceMenu.ForEach(item => DisplayCounter(item));
            }
        }



#warning En provenance de PageBase Déterminer ou le mettre
        private void ScriptOnStartUp(string pscript, string pkey)
        {
            string script;
            JavaScript.JSStringBuilder sb = new JavaScript.JSStringBuilder();
            sb.AppendLine(pscript);
            script = sb.ToString();
            RegisterScript(pkey, script, true);
        }
        private void RegisterScript(string pKey, string pScript)
        {
            RegisterScript(pKey, pScript, false);
        }
        private void RegisterScript(string pKey, string pScript, bool pIsStartUp)
        {
            // EG 20160308 Migration vs2013
            //bool isOk = (Request.Browser.JavaScript);
            bool isOk = (null != HttpContext.Current) && (1 < HttpContext.Current.Request.Browser.EcmaScriptVersion.Major);
            if (isOk)
            {
                if (pIsStartUp)
                    isOk = (!Page.ClientScript.IsStartupScriptRegistered(pKey));
                else
                    isOk = (!Page.ClientScript.IsClientScriptBlockRegistered(pKey));
            }
            if (isOk)
            {
                if (pIsStartUp)
                    Page.ClientScript.RegisterStartupScript(GetType(), pKey, pScript);
                else
                    Page.ClientScript.RegisterClientScriptBlock(GetType(), pKey, pScript);
            }
        }

        #region ExecuteImmediate
        protected void ExecuteImmediate(string pCommand, bool pIsClose)
        {
            ExecuteImmediate(pCommand, pIsClose, false);
        }
        public void ExecuteImmediate(string pCommand, bool pIsClose, bool pIsStartup)
        {
            string script;
            string nameFunction = "ExecuteImmediate";
            JavaScript.JSStringBuilder sb = new JavaScript.JSStringBuilder();

            if (!pCommand.EndsWith(";"))
                pCommand += ";";

            sb.AppendLine("function " + nameFunction + "()");
            sb.AppendLine("{");
            sb.AppendLine(pCommand);
            if (pIsClose)
                sb.AppendLine("self.close();");
            sb.AppendLine("}");
            sb.AppendLine(nameFunction + "();");
            script = sb.ToString();
            RegisterScript(nameFunction, script, pIsStartup);
        }
        #endregion
    }
}