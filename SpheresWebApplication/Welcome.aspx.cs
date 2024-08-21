using System;
using System.IO;
using System.Threading;
using System.Web.UI;
using EFS.ACommon;
using EFS.Common;
using EFS.Common.Web;

namespace EFS.Spheres
{
    /// <summary>
    /// Description résumée de Welcome.
    /// </summary>
    public partial class WelcomePage : PageBase
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        // EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        /// EG 20210614 [25500] New Customer Portal
        protected void Page_Load(object sender, System.EventArgs e)
        {
            string url = null;
            PageTools.SetHead(this, "Welcome", null, null);
            //
            if (Software.IsSoftwarePortal())
            {
                //Utilisation du welcome portail
                url = @"CustomerPortal/Page/Welcome.aspx";
            }


            if (SessionTools.IsConnected)
            {
                pnlWelcome.CssClass = this.CSSMode + " " + SessionTools.Company_CssColor;

                #region DefaultPage
                if (!Software.IsSoftwarePortal())
                {
                    try
                    {
                        if (StrFunc.IsFilled(SessionTools.DefaultPage))
                        {
                            TransferToDefaultUrl(SessionTools.DefaultPage);
                        }
                        else
                        {
                            bool isFound = false;
                            string urlDefaultPage = null;
                            //
                            string entity = SessionTools.Collaborator_ENTITY_IDENTIFIER;
                            string entity_dn = SessionTools.Collaborator_ENTITY_DISPLAYNAME.Replace(" ", "");
                            if (StrFunc.IsFilled(entity))
                            {
                                #region Find Default Page
                                string namePage;
                                string extPage;
                                for (int i = 0; i < 3; i++)
                                {
                                    switch (i)
                                    {
                                        case 0:
                                            namePage = "Welcome";
                                            break;
                                        case 1:
                                            namePage = "Default";
                                            break;
                                        default:
                                            namePage = "index";
                                            break;
                                    }
                                    for (int j = 0; j < 3; j++)
                                    {
                                        switch (j)
                                        {
                                            case 0:
                                                extPage = ".htm";
                                                break;
                                            case 1:
                                                extPage = ".asp";
                                                break;
                                            default:
                                                extPage = ".aspx";
                                                break;
                                        }
                                        urlDefaultPage = @"Welcome_Entity/" + entity + @"/" + namePage + Thread.CurrentThread.CurrentUICulture.Name + extPage;
                                        if (!File.Exists(Server.MapPath(urlDefaultPage)))
                                            urlDefaultPage = @"Welcome_Entity/" + entity_dn + @"/" + namePage + Thread.CurrentThread.CurrentUICulture.TwoLetterISOLanguageName + extPage;
                                        if (!File.Exists(Server.MapPath(urlDefaultPage)))
                                            urlDefaultPage = @"Welcome_Entity/" + entity + @"/" + namePage + Thread.CurrentThread.CurrentUICulture.TwoLetterISOLanguageName + extPage;
                                        if (!File.Exists(Server.MapPath(urlDefaultPage)))
                                            urlDefaultPage = @"Welcome_Entity/" + entity_dn + @"/" + namePage + Thread.CurrentThread.CurrentUICulture.TwoLetterISOLanguageName + extPage;
                                        if (!File.Exists(Server.MapPath(urlDefaultPage)))
                                            urlDefaultPage = @"Welcome_Entity/" + entity + @"/" + namePage + extPage;
                                        if (!File.Exists(Server.MapPath(urlDefaultPage)))
                                            urlDefaultPage = @"Welcome_Entity/" + entity_dn + @"/" + namePage + extPage;

                                        if (File.Exists(Server.MapPath(urlDefaultPage)))
                                        {
                                            isFound = true;
                                            break;
                                        }
                                    }
                                    if (isFound)
                                        break;
                                }
                                if (isFound)
                                    url = urlDefaultPage;
                                #endregion
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        string errMsg = ex.Message;
                        System.Diagnostics.Debug.WriteLine(errMsg);
                    }
                }
                #endregion DefaultPage
            }


            if (StrFunc.IsFilled(url))
            {
                string tmpUrl = url;
                if (tmpUrl.IndexOf("?") > 0)
                    tmpUrl = tmpUrl.Substring(0, tmpUrl.IndexOf("?"));
                if (File.Exists(Server.MapPath(tmpUrl)))
                {
                    //this.Server.Transfer(url);
                    this.Response.Redirect(url, true);
                }
            }
            else
            {
                //Utilisation de l'éventuelle page de "customization"
                if (SessionTools.IsConnected)
                {
                    url = SystemSettings.GetAppSettings("Spheres_DefaultHomePage");
                    if (StrFunc.IsEmpty(url))//Pour compatibilité ascendante
                        url = SystemSettings.GetAppSettings("Spheres_DefaultPage");
                }
                else
                {
                    //Fonctionnalitée volontairement non documentée (20060703 PL)
                    url = SystemSettings.GetAppSettings("Spheres_DefaultWelcomePage");
                }
                //
                if (StrFunc.IsFilled(url))
                {
                    this.Response.Redirect(url, true);
                }
                else
                {
                    string imageURL;

                    // FOOTER
                    ClientScript.RegisterHiddenField("__SOFTWARE", Software.Name);
                    hlkFooter.Text = Software.CopyrightFull;
                    hlkFooter.NavigateUrl = "http://www.euro-finance-systems.fr/";
                    bool isCustomize = SessionTools.License.IsLicFunctionalityAuthorised(EFS.Common.LimitationFunctionalityEnum.CUSTOMIZEWELCOME);
#if DEBUG
                    isCustomize = true;
#endif
                    imgLogoPub.Visible = (false == isCustomize);
                    imgWelcomeFooter.Visible = false;
                    if (isCustomize)
                    {
                        imageURL = SystemSettings.GetAppSettings("Spheres_WelcomeFooter");
                        if (StrFunc.IsFilled(imageURL))
                        {
                            imgWelcomeFooter.ImageUrl = Request.ApplicationPath + imageURL;
                            imgWelcomeFooter.Visible = true;
                        }
                    }
                    else
                    {
                        JavaScript.ExecuteImmediate(this, @"window.setTimeout(""Go()"",3500);", false);
                    }


                    // BODY

                    imageURL = SystemSettings.GetAppSettings(isCustomize ? "Spheres_WelcomeBody" : "Spheres_WelcomeDefaultBody");
                    if (StrFunc.IsEmpty(imageURL))
                        imageURL = $"/Images/Logo_Software/WelcomeBody_v{Software.YearCopyright}.png";
                    if (StrFunc.IsFilled(imageURL))
                        imageURL = Request.ApplicationPath + imageURL;
                    imgWelcomeBody.ImageUrl = imageURL;

                    // HEADER
                    imageURL = SystemSettings.GetAppSettings("Spheres_WelcomeHeader");
                    if (StrFunc.IsEmpty(imageURL)) //Pour compatibilité ascendante
                        imageURL = SystemSettings.GetAppSettings("Spheres_LogoWelcome");
                    if (StrFunc.IsEmpty(imageURL))
                        imageURL = $"/Images/Logo_Software/WelcomeHeader.png";
                    if (StrFunc.IsFilled(imageURL))
                        imageURL = Request.ApplicationPath + imageURL;
                    pnlWelcomeHeader.Style.Add(HtmlTextWriterStyle.BackgroundImage, imageURL);

                    this.DataBind();
                    this.Form = this.Welcome;
                }
            }

        }

        /// <summary>
        /// 
        /// </summary>
        /// 20100901 EG Inscription jQuery engine
        /// 20100901 EG Inscription et initialisation jQuery UI (Datepicker, timepiker, toggle)
        protected override void CreateChildControls()
        {
            base.CreateChildControls();
            //JQuery.UI.WriteInitialisationScripts(this, new JQuery.Dialog());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        // EG 20210212 [25661] New Appel Protection CSRF(Cross-Site Request Forgery)
        protected override void OnInit(EventArgs e)
        {
            //
            // CODEGEN : Cet appel est requis par le Concepteur Web Form ASP.NET.
            //
            InitializeComponent();
            base.OnInit(e);
            Form = Welcome;
            AntiForgeryControl();
        }

        /// <summary>
        /// Méthode requise pour la prise en charge du concepteur - ne modifiez pas
        /// le contenu de cette méthode avec l'éditeur de code.
        /// </summary>
        private void InitializeComponent()
        {

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pIdMenu"></param>
        private void TransferToDefaultUrl(string pIdMenu)
        {
            string url = SessionTools.Menus.GetMenu_Url(pIdMenu);
            if (StrFunc.IsFilled(url))
            {
                string idMenu = "IDMenu=" + pIdMenu;
                if (url.IndexOf(idMenu) <= 0)
                {
                    if (url.IndexOf("?") <= 0)
                        url += "?" + idMenu;
                    else
                        url += "&" + idMenu;
                }
                // FI 20240327 [WI869] call Response.Redirect instead of this.Server.Transfer
                // using Response.Redirect => PageBase.CheckURL works Fine 
                //Server.Transfer(url);
                Response.Redirect(url, true);
            }
        }
    }
}
