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
    public partial class Welcome : ContentPageBase
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void Page_Load(object sender, System.EventArgs e)
        {
            string url = null;
            if (Software.IsSoftwarePortal())
            {
                //Utilisation du welcome portail
                url = @"Portal/NewPortal.aspx";
            }
            //
            if (SessionTools.IsConnected)
            {
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
                            string entity = SessionTools.Collaborator_PARENT_IDENTIFIER;
                            string entity_dn = SessionTools.Collaborator_PARENT_DISPLAYNAME.Replace(" ", "");
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
                    this.Server.Transfer(url);
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
                    welcomeHeaderContent.Visible = false;
                    welcomeFooterContent.Visible = false;

                    string imageURL = string.Empty;

                    // FOOTER
                    ClientScript.RegisterHiddenField("__SOFTWARE", Software.Name);
                    bool isCustomize = SessionTools.License.IsLicFunctionalityAuthorised(LimitationFunctionalityEnum.CUSTOMIZEWELCOME);

                    if (isCustomize)
                    {
                        // HEADER
                        imageURL = SystemSettings.GetAppSettings("Spheres_WelcomeHeader");
                        if (StrFunc.IsFilled(imageURL))
                        {
                            imageURL = Request.ApplicationPath + imageURL;
                            welcomeHeaderContent.Style.Add(HtmlTextWriterStyle.BackgroundImage, imageURL);
                            welcomeHeaderContent.Visible = true;
                        }

                        imageURL = SystemSettings.GetAppSettings("Spheres_WelcomeFooter");
                        if (StrFunc.IsFilled(imageURL))
                        {
                            imageURL = Request.ApplicationPath + imageURL;
                            welcomeFooterContent.Style.Add(HtmlTextWriterStyle.BackgroundImage, imageURL);
                            welcomeFooterContent.Visible = true;
                        }
                    }

                    // BODY
                    imageURL = SystemSettings.GetAppSettings(isCustomize ? "Spheres_WelcomeBody" : "Spheres_WelcomeDefaultBody");
                    if (StrFunc.IsEmpty(imageURL))
                        imageURL = "/Images/Logo_Software/WelcomeBody-v2020.png";
                    if (StrFunc.IsFilled(imageURL))
                        imageURL = Request.ApplicationPath + imageURL;
                    welcomeBodyContent.Style.Add(HtmlTextWriterStyle.BackgroundImage, imageURL);

                    this.DataBind();
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
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        protected override void OnInit(EventArgs e)
        {
            //
            // CODEGEN : Cet appel est requis par le Concepteur Web Form ASP.NET.
            //
            InitializeComponent();
            base.OnInit(e);
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
                this.Server.Transfer(url);
            }
        }

    }
}