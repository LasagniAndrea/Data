using EFS.ACommon;
using EFS.ApplicationBlocks.Data;
using EFS.Common;
using EFS.Common.Web;
using System;
using System.Data;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Web;
using System.Web.UI;

namespace EFS.Spheres
{
    public partial class Banner : PageBase
    {
        protected int isConnected = 0;

        // EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        // EG 20210128 [XXXXX] Minification Banner.css
        // EG 20230513 [WI922] Spheres Portal : EFS|BDX GUI Adaptation
        protected void Page_Load(object sender, EventArgs e)
        {
            PageTools.SetHead(this, "Banner", null, null);
            Control head = PageTools.GetHeadTag(this);
            PageTools.SetHeaderLink(head, "linkCssBanner", "~/Includes/Banner.min.css");

            #region Logo
            string fileName = null;
            //PL 20180620 Use CUSTOMIZEBANNER instead of CUSTOMIZEWELCOME
            //bool isCustomize = SessionTools.License.IsLicFunctionalityAuthorised(LimitationFunctionalityEnum.CUSTOMIZEWELCOME);
            bool isCustomize = SessionTools.License.IsLicFunctionalityAuthorised(LimitationFunctionalityEnum.CUSTOMIZEBANNER);
#if DEBUG
            isCustomize = true;
#endif
            //Set Entity logo 
            if (Software.IsSoftwarePortal())
            {
                imgBanner.ImageUrl = "Images/Gif/BannerPortal3.gif";
                imgBanner.Visible = true;

                fileName = @"~/Images/Logo_Entity/EuroFinanceSystems/EuroFinanceSystems_Banner.gif";
            }
            else
            {
                if (isCustomize)
                {
                    fileName = SystemSettings.GetAppSettings("Spheres_BannerLeft");
                    if (string.IsNullOrWhiteSpace(fileName)) //Pour compatibilité ascendante
                        fileName = SystemSettings.GetAppSettings("Spheres_LogoRight");
                    fileName = this.Request.ApplicationPath + fileName;
                }
                if ((!isCustomize) || (!File.Exists(this.Server.MapPath(fileName))))
                    fileName = Request.ApplicationPath + ControlsTools.GetLogoForEntity(@"/Images/Logo_Software/Spheres_Banner_v" + Software.YearCopyright + ".png");
            }

            if (File.Exists(Server.MapPath(fileName)))
            {
                imgLogoCompany.Visible = true;
                imgLogoCompany.ImageUrl = fileName;
                try
                {
                    //20090305 PL
                    System.Drawing.Bitmap bitmap = new Bitmap(Server.MapPath(fileName));
                    int bitmapWidth = bitmap.Width;
                    int bitmapHeight = bitmap.Height;
                    if (bitmapWidth > 185 || bitmapHeight > 65)
                    {
                        imgLogoCompany.Width = 185;
                        imgLogoCompany.Height = 65;
                    }
                    bitmap.Dispose();
                    bitmap = null;
                }
                catch { }
                imgLogoCompany.ToolTip = string.Empty;
                imgLogoCompany.Style.Add(HtmlTextWriterStyle.Cursor, "pointer");
                imgLogoCompany.Attributes.Add("onclick", "parent.document.getElementById('main').src='Welcome.aspx';return false;");

            }
            else
            {
                imgLogoCompany.Visible = false;
            }
            //Set Software logo 
            fileName = null;
            if (!Software.IsSoftwarePortal())
            {
                if (isCustomize)
                {
                    fileName = SystemSettings.GetAppSettings("Spheres_BannerRight");
                    if (string.IsNullOrWhiteSpace(fileName)) //Pour compatibilité ascendante
                        fileName = SystemSettings.GetAppSettings("Spheres_LogoLeft");
                    fileName = this.Request.ApplicationPath + fileName;
                }
                if ((!isCustomize) || (!File.Exists(this.Server.MapPath(fileName))))
                {
                    fileName = Software.Name.Replace("&", "n");
                    if (fileName == "Spheres")
                        fileName = "OTCml";//PL 20120201 A terminer...
                    //fileName = @"~/Images/Logo_Software/Spheres_" + Software.Name.Replace("&","n") + @"_banner.gif";GLOPOLG
                    //PL 20100125 Pour le fun... Tester plus tard la licence pour savoir si seul les Product ETD sont dipso...
                    if (Software.IsSoftwareSpheres())
                    {
                        if ((SessionTools.License != null))
                        {
                            if (SessionTools.License.licensee == "DEGROOF")
                            {
                                fileName = "FnOml";//PL 20120201 A terminer...
                            }
                            else //(SessionTools.License.licensee == "OTCEX")
                            {
                                fileName = "OTCml";//PL 20120201 A terminer...
                            }
                        }
                        else if ((SessionTools.Data.Database != null))
                        {
                            string db = SessionTools.Data.Database.ToUpper();
                            if (db.StartsWith("FOML") || db.StartsWith("F&OML")
                                || (db.IndexOf("DEGROOF") >= 0) || (db.IndexOf("KYTE") >= 0) || (db.IndexOf("BNP") >= 0) || (db.IndexOf("KEDRIOS") >= 0)
                                || (db.IndexOf("PARALLEL") >= 0) || db.EndsWith("PASCAL"))
                            {
                                //fileName = @"~/Images/Logo_Software/Spheres_" + "FnOml" + @"_banner.gif";
                                fileName = "FnOml";//PL 20120201 A terminer...
                            }
                        }
                    }
                    fileName = $"~/Images/Logo_Software/Spheres_Banner_xs_v{Software.YearCopyright}.png";

                }
            }
            if (File.Exists(Server.MapPath(fileName)))
            {
                imgLogoSoftware.ImageUrl = fileName;
                imgLogoSoftware.Visible = true;
                imgLogoSoftware.Style.Add(HtmlTextWriterStyle.Cursor, "pointer");
                imgLogoSoftware.Style.Add("float", "right");
                imgLogoSoftware.Pty.TooltipContent =  Software.CopyrightFull;
                imgLogoSoftware.Attributes.Add("onclick", "parent.document.getElementById('main').src='Welcome.aspx';return false;");
            }
            else
            {
                imgLogoSoftware.Visible = false;
            }
            #endregion Logo

            #region XXX
            if (StrFunc.IsFilled(SessionTools.Collaborator_DISPLAYNAME))
            {
                hidIsConnected.Value = "yes";
                if (!IsPostBack)
                    hidIDA.Value = SessionTools.Collaborator_IDA.ToString();
            }
            else
            {
                hidIsConnected.Value = "no";
            }

            //btnCulture.CssClass = CSS.SetCssClass(CSS.Banner.flag, Thread.CurrentThread.CurrentUICulture.TwoLetterISOLanguageName);

            btnCulture.Text = @"<i class='fas fa-flag'></i> " + Thread.CurrentThread.CurrentUICulture.TwoLetterISOLanguageName;
            btnCulture.Attributes.Add("onclick", "parent.document.getElementById('main').src='Profile.aspx';return false;");


            if (SessionTools.IsConnected)
            {
                btnHelp.Attributes.Add("onclick", SystemSettings.GetWindowOpenForHelpApp(null) + ";return false;");
                btnHelp.Visible = true;
            }
            else
            {
                btnHelp.Attributes.Add("onclick", "return false;");
                btnHelp.Visible = false;
            }
            btnHelp.Style.Add(HtmlTextWriterStyle.VerticalAlign, "middle");

            #region Display Entity
            string parent_Identifier = SessionTools.Collaborator_ENTITY_IDENTIFIER;
            string parent_DisplayName = SessionTools.Collaborator_ENTITY_DISPLAYNAME;
            string parent_Description = SessionTools.Collaborator_ENTITY_DESCRIPTION;
            bool parent_IsEntity = SessionTools.Collaborator_ENTITY_ISENTITY;
            string displayNameCssClass = parent_IsEntity ? "entitydsn" : "supportdsn";
            string descriptionCssClass = parent_IsEntity ? "entitydesc" : "supportdesc";
            string guiCssClass = "labelGUITitle";

            btnUserName.Visible = SessionTools.IsConnected;
            if (SessionTools.IsConnected)
            {
                if (StrFunc.IsEmpty(parent_Identifier))
                {
                    //User non rattaché à une entité (ou à un support)
                    parent_DisplayName = Ressource.GetString2("Msg_Warning", true, string.Empty);
                    parent_Description = Ressource.GetString("Msg_UserWithoutEntity", true);
                    displayNameCssClass = "labelErrorDisplayname";
                    descriptionCssClass = "labelErrorDescription";
                    guiCssClass = "labelErrorGUITitle";
                }
                else
                {
                    if (StrFunc.IsEmpty(parent_DisplayName))
                        parent_DisplayName = parent_Identifier;
                    if (StrFunc.IsEmpty(parent_Description) || (parent_DisplayName == parent_Description))
                        parent_Description = string.Empty;
                    else if (parent_Description.StartsWith(parent_DisplayName))
                        parent_DisplayName = parent_Identifier;
                    else
                    {
                        //parent_Description = " - " + parent_Description;
                        parent_Description = Cst.HTMLSpace4 + parent_Description;
                    }
                }

                btnUserName.Text = @"<i class='fas fa-user'></i> " + HttpContext.Current.User.Identity.Name.ToString();
                string collaborator_ROLE = (string)SessionTools.Collaborator_ROLE;
                btnUserName.Pty.TooltipContent = SessionTools.Collaborator_DISPLAYNAME + " " + Cst.HTMLBold + Ressource.GetString(collaborator_ROLE) + Cst.HTMLEndBold;
                btnUserName.Pty.TooltipContent += " [" + Ressource.GetString(collaborator_ROLE) + "]";


                if (SessionTools.IsSessionSysAdmin)
                {
                    btnUserName.CssClass = String.Format("fa-icon {0}", "red");
                }
                else if (SessionTools.IsSessionSysOper)
                {
                    btnUserName.CssClass = String.Format("fa-icon {0}", "blue");
                }
                else if (SessionTools.IsSessionGuest)
                {
                    btnUserName.CssClass = String.Format("fa-icon {0}", "blue");
                }
                else
                {
                    btnUserName.CssClass = String.Format("fa-icon {0}", "gray");
                }

                if (DtFunc.IsDateTimeFilled(SessionTools.DTLASTLOGIN))
                {
                    // EG 20210406 [25556] Libellé dernière connexion
                    lblLastLogin.Text = Ressource.GetString("DTLASTLOGIN") + Cst.HTMLSpace + DtFuncExtended.DisplayTimestampUTC(SessionTools.DTLASTLOGIN,
                        new AuditTimestampInfo()
                        {
                            TimestampZone = SessionTools.AuditTimestampZone,
                            Collaborator = SessionTools.Collaborator,
                            Precision = Cst.AuditTimestampPrecision.Second
                        });
                }

                bool isUserWithLimitedRights = SessionTools.IsUserWithLimitedRights();
                if (isUserWithLimitedRights && (!SessionTools.IsSessionGuest) && (SessionTools.IsDataArchive))
                    isUserWithLimitedRights = false;

                //PL 20171201 Newness - ConnectionString_Banner
                if (isUserWithLimitedRights || BoolFunc.IsFalse(SystemSettings.GetAppSettings("ConnectionString_Banner", "true")))
                {
                    lblConnectionString.Visible = false;
                }
                else
                {
                    string servername = SessionTools.Data.Server;
                    // AL 20240705 [WI996] the following code is no more usefull with the changes in CSManager.GetSvrName(). Servername is already correct.
                    //if ((SessionTools.Data.RDBMS.IndexOf("SQLSRV") >= 0) && (servername == "."))
                    //{
                    //    //SQLServer: Serveur local
                    //    //servername = "(Local)";
                    //}
                    //else if ((SessionTools.Data.RDBMS.IndexOf("ORACL") >= 0) && (servername.IndexOf("SERVICE_NAME=") >= 0))
                    //{
                    //    //Oracle: CS au format EZConnect, on considère uniquement SERVICE_NAME
                    //    servername = servername.Substring(servername.IndexOf("SERVICE_NAME=") + 13);
                    //    if (servername.IndexOf(".") >= 0)
                    //        servername = servername.Substring(0, servername.IndexOf("."));
                    //    if (servername.IndexOf(")") >= 0)
                    //        servername = servername.Substring(0, servername.IndexOf(")"));
                    //}
                    lblConnectionString.Text = SessionTools.Data.RDBMS + @": \\" + servername + @"\" + SessionTools.Data.Database;
                    lblConnectionString.Style.Add(HtmlTextWriterStyle.Cursor, "pointer");
                    lblConnectionString.Pty.TooltipContent = Ressource.GetString("lblDatabase") + " " + Cst.HTMLBold + SessionTools.Data.DatabaseNameVersionBuild + Cst.HTMLEndBold;
                }

                imgLogoSoftware.Pty.TooltipContent += Cst.HTMLBreakLine + Ressource.GetString("RegisteredLicense") + " ";
                if (StrFunc.IsFilled(SessionTools.License.licensee))
                    imgLogoSoftware.Pty.TooltipContent += SessionTools.License.licensee;
                else
                    imgLogoSoftware.Pty.TooltipContent += "UNKNOWN!";
            }
            else
            {
                btnUserName.Text = string.Empty;
                lblLastLogin.Text = string.Empty;
                lblConnectionString.Text = string.Empty;

                if (Software.IsSoftwarePortal())
                {
                    parent_DisplayName = "EFS|BDX";
                    parent_Description = "Provider of Capital Markets Solutions";
                    btnUserName.Text = "Welcome to our portal";
                    displayNameCssClass = "entitydsn";
                    descriptionCssClass = "entitydesc";
                    guiCssClass = "labelGUITitle";
                }
            }

            lblCompanyDisplayname.CssClass = displayNameCssClass;
            lblCompanyDisplayname.Text = parent_DisplayName;
            lblCompanyDisplayname.Font.Bold = true;
            lblCompanyDescription.CssClass = descriptionCssClass;
            lblCompanyDescription.Text = parent_Description;
            lblCompanyDescription.Font.Bold = false;



            lblGUITitle.Text = string.Empty;
            lblGUITitle.Font.Bold = false;
            lblGUITitle.CssClass = guiCssClass;
            //
            string source = null;
            if (SessionTools.IsConnected)
                source = SessionTools.CS;
            else
                source = SystemSettings.GetLogConnectionString(false, System.Web.HttpContext.Current.Request.ApplicationPath);
            
            // FI 20140120 [] add test if (StrFunc.IsFilled (source))
            DbSvrType dbSvrType = DbSvrType.dbUNKNOWN;
            if (StrFunc.IsFilled(source))
                dbSvrType = DataHelper.GetDbSvrType(source);

            if (DbSvrType.dbUNKNOWN != dbSvrType)
            {
                string SQLSelect = SQLCst.SELECT + DataHelper.SQLIsNullChar(source, "GUITITLE", string.Empty, "GUITITLE");
                SQLSelect += "," + DataHelper.SQLIsNullChar(source, "GUITITLEFORECOLOR", string.Empty, "GUITITLEFORECOLOR");
                SQLSelect += "," + DataHelper.SQLIsNull(source, "GUITITLEFONTSIZE", "0", "GUITITLEFONTSIZE");
                SQLSelect += "," + DataHelper.SQLIsNullChar(source, "GUIMESSAGE", string.Empty, "GUIMESSAGE") + Cst.CrLf;
                SQLSelect += SQLCst.FROM_DBO + Cst.OTCml_TBL.LICENSEE.ToString();
                try
                {
                    using (IDataReader dr = DataHelper.ExecuteDataTable(source, SQLSelect).CreateDataReader())
                    {
                        if (dr.Read())
                        {
                            try
                            {
                                if (!Software.IsSoftwarePortal())
                                {
                                    lblArchive.Visible = SessionTools.IsDataArchive;
                                    if (lblArchive.Visible)
                                    {
                                        lblArchive.Text = "ARCHIVE " + SessionTools.DtArchive.ToString("d") + Cst.HTMLSpace4;
                                        lblArchive.Font.Bold = true;
                                    }

                                    lblGUITitle.Text = Translate(dr["GUITITLE"].ToString());
                                    lblGUITitle.Font.Bold = false;
                                    if (dr["GUITITLEFORECOLOR"].ToString().Length > 0)
                                        lblGUITitle.ForeColor = Color.FromName(dr[1].ToString());
                                    if (Convert.ToInt32(dr["GUITITLEFONTSIZE"]) > 0)
                                    {
                                        int size = Convert.ToInt32(dr["GUITITLEFONTSIZE"]);
                                        lblGUITitle.Font.Size = size;
                                    }
                                }
                            }
                            catch { }
                            try
                            {
                                if (SessionTools.IsConnected && (dr["GUIMESSAGE"].ToString().Length > 0) || SessionTools.IsDataArchive)
                                {
                                    // EG 20130925 HTMLBreakLine
                                    string msg = SessionTools.IsDataArchive ? "ARCHIVE Environment - Date: " + SessionTools.DtArchive.ToString("d") + Cst.HTMLBreakLine2 : string.Empty;
                                    msg += Translate(dr["GUIMESSAGE"].ToString());
                                    JQuery.OpenDialog openDialog = new JQuery.OpenDialog(lblGUITitle.Text, msg.Replace(Cst.CrLf, Cst.HTMLBreakLine), ProcessStateTools.StatusNoneEnum);
                                    JQuery.UI.WriteInitialisationScripts(this, openDialog);
                                }
                            }
                            catch { }
                        }
                    }
                    lblGUITitle.Visible = SessionTools.IsConnected;
                    imgRefresh.Visible = (SessionTools.IsConnected && Software.IsSoftwareSpheres());

                    this.DataBind();
                }
                catch (Exception ex)
                {
                    lblConnectionString.Text = "An error occurs : " + ex.Message;
                    lblConnectionString.CssClass = "errorOnBanner";
                    lblConnectionString.ToolTip = ex.Message;
                }
            }

            #endregion
            #endregion XXX
        }
        // EG 20200720 [25556] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        // EG 20200729 [25556] Nouvelle interface GUI v10 (Mode Noir ou blanc) Correctifs et compléments (Track, Banner, Tracker ...)
        // EG 20210128 [25556] Gestion affichage Bannière (cssClass) selon display EntityMarket ou pas
        // EG 20210212 [25661] New Appel Protection CSRF(Cross-Site Request Forgery)
        // EG 20210222 Suppression inscription script JavascriptCookieFunctions
        override protected void OnInit(EventArgs e)
        {
            this.AbortRessource = true;
            bm.Attributes.Add("class", CSSMode + " " + SessionTools.Company_CssColor);
            bm_company.Attributes.Add("class", CSSMode + " " + SessionTools.Company_CssColor);
            base.OnInit(e);

            Form = idBanner;
            AntiForgeryControl();

            bool displayEntityMarket = BoolFunc.IsTrue(SystemSettings.GetAppSettings("EntityMarket_Banner"));
            if (displayEntityMarket)
                bm_row2.Attributes.Add("class", "entitymarket");
            bm_entitymarket.Visible = displayEntityMarket;
            //JavaScript.JavascriptCookieFunctions(this);
        }

        #region protected override CreateChildControls
        // EG 20210224 [XXXXX] suppresion PageBase.js déja appelé dans Render de PageBase
        // EG 20240604 [WI945] Security : Update outdated components (New QTip2)
        protected override void CreateChildControls()
        {
            // Override complet de PageBase (pas d'appel)
            JQuery.WriteEngineScript(this, JQuery.Engines.JQuery);
            JQuery.UI.WritePluginScript(this, JQuery.Engines.JQuery);
            JQuery.UI.WriteInitialisationScripts(this, new JQuery.Dialog());
            JQuery.UI.WriteInitialisationScripts(this, new JQuery.Toggle());
            JQuery.UI.WriteInitialisationScripts(this, new JQuery.QTip2());
            //
            //ScriptManager.Scripts.Add(new ScriptReference("~/Javascript/PageBase.js"));
        }
        #endregion CreateChildControls (Scripts JavaScript)

        private string Translate(string pData)
        {
            pData = pData.Replace("%%RDBMS%%", SessionTools.Data.RDBMS);
            pData = pData.Replace("%%SERVER%%", SessionTools.Data.Server);
            pData = pData.Replace("%%SID%%", SessionTools.Data.Server);
            pData = pData.Replace("%%DATABASE%%", SessionTools.Data.Database);
            pData = pData.Replace("%%SCHEMA%%", SessionTools.Data.Database);
            //
            pData = pData.Replace("%%USER_IDENTIFIER%%", SessionTools.Collaborator_IDENTIFIER);
            pData = pData.Replace("%%USER_DISPLAYNAME%%", SessionTools.Collaborator_DISPLAYNAME);
            pData = pData.Replace("%%USERPARENT_IDENTIFIER%%", SessionTools.Collaborator_ENTITY_IDENTIFIER);
            pData = pData.Replace("%%USERPARENT_DISPLAYNAME%%", SessionTools.Collaborator_ENTITY_DISPLAYNAME);
            //
            pData = Ressource.GetString(pData, true);
            return pData;
        }
    }
}