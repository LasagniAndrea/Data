using EFS.ACommon;
using EFS.Actor;
using EFS.ApplicationBlocks.Data;
using System;
using System.Collections;
using System.Data;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;

// EG 20231129 [WI756] Spheres Core : Refactoring Code Analysis with Intellisense

namespace EFS.Common.Web.Menu
{
    #region public class Menu
    public class Menu : ICloneable
    {
        #region Members
        private string id;
        private string idmenu;
        private string idmenu_parent;
        private int level;
        private string icon;
        private string displayname;
        private string description;
        private string url;
        private bool isexternal;
        private bool isaction;
        private bool isseparator;
        private bool ismovable;
        // CC 20100908 MENU.ISCHILDONLY and MENUOF.ISCHILDONLY replaced by column VISIBILITY cf Ticket 17143
        //private bool ischildonly;
        private string visibility;
        private string extllink;
        private bool ispublic;
        private bool isenabled;
        private bool isenabledSpecified;
        private string html;
        private string border;
        private PlaceHolder placeHolder;
        #endregion Members

        #region Accesseur(s)
        /// <summary>
        /// 
        /// </summary>
        public string MainMenuName
        {
            get
            {
                string mainMenuName = "Unknown";
                string[] mainMenu = idmenu.Split('_');
                if (ArrFunc.IsFilled(mainMenu) && (1 < mainMenu.Length))
                {
                    switch (mainMenu[1])
                    {
                        case "ABOUT":
                            mainMenuName = "About";
                            break;
                        case "ADM":
                            mainMenuName = "Admin";
                            break;
                        case "EXTL":
                            mainMenuName = "External";
                            break;
                        case "INP":
                            mainMenuName = "Input";
                            break;
                        case "INV":
                            mainMenuName = "Invoicing";
                            break;
                        case "PROCESS":
                            mainMenuName = "Process";
                            break;
                        case "REF":
                            mainMenuName = "Repository";
                            break;
                        case "VIEW":
                            mainMenuName = "Views";
                            break;
                        default:
                            // Cas PORTAIL
                            switch (mainMenu[1])
                            {
                                case "CLIENT":
                                    mainMenuName = "Views";
                                    break;
                                case "SERVICES":
                                    mainMenuName = "Process";
                                    break;
                                case "SUPPORT":
                                    mainMenuName = "About";
                                    break;
                                case "INTERNE":
                                    mainMenuName = "Input";
                                    break;
                                default:
                                    mainMenuName = "Unknown";
                                    break;
                            }
                            break;
                    }
                }
                return mainMenuName;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public string IdMenu
        {
            get { return idmenu; }
            set { idmenu = value; }
        }
        /// <summary>
        /// Retourne l'ID menu (System)
        /// </summary>
        // EG 20151019 [21465] New Id technique unique d'un menu
        public string Id
        {
            get { return id; }
            set { id = value; }
        }
        /// <summary>
        /// 
        /// </summary>
        public string IdMenu_Parent
        {
            get { return idmenu_parent; }
            set { idmenu_parent = value; }
        }
        /// <summary>
        /// 
        /// </summary>
        public int Level
        {
            get { return level; }
            set { level = value; }
        }
        /// <summary>
        /// 
        /// </summary>
        public string Icon
        {
            get { return icon; }
            set { icon = value; }
        }
        /// <summary>
        /// 
        /// </summary>
        public string Displayname
        {
            get { return displayname; }
            set { displayname = value; }
        }
        /// <summary>
        /// 
        /// </summary>
        public string Description
        {
            get { return description; }
            set { description = value; }
        }
        /// <summary>
        /// 
        /// </summary>
        public string Url
        {
            get { return url; }
            set { url = value; }
        }
        /// <summary>
        /// 
        /// </summary>
        public bool IsMovable
        {
            get { return ismovable; }
            set { ismovable = value; }
        }
        // CC 20100908 MENU.ISCHILDONLY and MENUOF.ISCHILDONLY replaced by column VISIBILITY cf Ticket 17143
        //public string IsChildOnly
        //{
        //    get { return ischildonly; }
        //    set { ischildonly = value; }
        //}
        public string Visibility
        {
            get { return visibility; }
            set { visibility = value; }
        }
        public bool IsExternal
        {
            get { return isexternal; }
            set { isexternal = value; }
        }
        public bool IsAction
        {
            get { return isaction; }
            set { isaction = value; }
        }
        public bool IsSeparator
        {
            get { return isseparator; }
            set { isseparator = value; }
        }
        public string ExtlLink
        {
            get { return extllink; }
            set { extllink = value; }
        }
        public bool IsPublic
        {
            get { return ispublic; }
            set { ispublic = value; }
        }
        public bool IsEnabled
        {
            get { return isenabled; }
            set { isenabled = value; }
        }
        public string Html
        {
            get { return html; }
            set { html = value; }
        }
        public PlaceHolder MnuPlaceHolder
        {
            get { return placeHolder; }
            set { placeHolder = value; }
        }
        #endregion Accesseur(s)

        #region Constructeur(s)
        public Menu()
        {
            id = string.Empty;
            idmenu = string.Empty;
            idmenu_parent = string.Empty;
            description = "unknown";
            isaction = false;
            isseparator = false;
            ismovable = false;
            // CC 20100908 MENU.ISCHILDONLY and MENUOF.ISCHILDONLY replaced by column VISIBILITY cf Ticket 17143
            //ischildonly = false;
            visibility = Cst.Visibility.SHOW.ToString();
            isexternal = false;
            ispublic = false;
            isenabled = false;
            isenabledSpecified = false;
            level = 0;
            html = string.Empty;
            placeHolder = null;
        }
        // EG 20151019 [21465] New Gestion remplacant %%IDMENUSYS%% par sa valeur (ID)
        // EG 20161122 AspTools.SetListOrListViewer
        // EG 20201116 [25556] Réécriture du lien sur Hyperlink (Pb self.Close) 
        public Menu(DataRow pDataRow, string pStyleDisplay, string pBorder, string pVisibility)
        {
            border = pBorder;
            StringBuilder sb = new StringBuilder(Cst.CrLf);
            _ = new StringBuilder(Cst.CrLf);
            int offsetimg;

            id = Convert.ToString(pDataRow["ID"]);
            idmenu = Convert.ToString(pDataRow["IDMENU"]);
            idmenu_parent = (pDataRow["IDMENU_MENU"] == DBNull.Value) ? string.Empty : Convert.ToString(pDataRow["IDMENU_MENU"]);
            level = Convert.ToInt32(pDataRow["LEVEL"]);
            icon = (pDataRow["ICON"] == DBNull.Value) ? string.Empty : pDataRow["ICON"].ToString();
            displayname = (pDataRow["DISPLAYNAME"] == DBNull.Value) ? string.Empty : pDataRow["DISPLAYNAME"].ToString();
            description = (pDataRow["DESCRIPTION"] == DBNull.Value) ? string.Empty : pDataRow["DESCRIPTION"].ToString();
            url = (pDataRow["URL"] == DBNull.Value) ? string.Empty : pDataRow["URL"].ToString();

            isaction = Convert.ToBoolean(pDataRow["ISACTION"]);
            isseparator = Convert.ToBoolean(pDataRow["ISSEPARATOR"]);
            ismovable = Convert.ToBoolean(pDataRow["ISMOVABLE"]);
            visibility = pVisibility;
            isexternal = Convert.ToBoolean(pDataRow["ISEXTERNAL"]);
            extllink = (pDataRow["EXTLLINK"] == DBNull.Value) ? string.Empty : pDataRow["EXTLLINK"].ToString();
            ispublic = Convert.ToBoolean(pDataRow["ISPUBLIC"]);
            isenabled = Convert.ToBoolean(pDataRow["ISENABLED"]);
            isenabledSpecified = Convert.ToBoolean(pDataRow["ISENABLEDSpecified"]);
            if (!isenabledSpecified)
                isenabled = ispublic;
            //
            string description_res = GetToolTip();
            //				
            offsetimg = ((level - 1) * 2) + ((level - 2) * 2);
            //Reswin += " " + level.ToString() + " " + Convert.ToString(offsetimg);//Used for debug (PL)
            //
            // CC 20100908 MENU.ISCHILDONLY and MENUOF.ISCHILDONLY replaced by column VISIBILITY cf Ticket 17143
            //if (!ischildonly)
            if (visibility != Cst.Visibility.HIDE.ToString())
            {
                string mainMenuName = MainMenuName;
                //string urlMenu = url.Replace("&", "&amp;");
                //string urlMenu = url;

                if (isaction || isseparator)
                {
                    placeHolder = SetActionMenuHTML(mainMenuName, description_res, offsetimg);
                }
                else
                {
                    if (level == 1)
                        placeHolder = SetMainMenuHTML(mainMenuName, pStyleDisplay);
                    else
                        placeHolder = SetSubMenuHTML(mainMenuName, description_res, pStyleDisplay);
                }
                html = sb.ToString();
            }
        }
        #endregion

        #region methods
        /// <summary>
        /// Construction du bloc de contrôle pour une ligne de menu avec ouverture de page
        /// Le lien Hyperlink possède un événement onclick qui fait appel à une fonction JS
        /// pour ouvrir la page via window.open, cette page pourra être refermée avec self.close 
        /// </summary>
        /// <param name="pMainMenuName"></param>
        /// <param name="pDescriptionRes"></param>
        /// <param name="pStyleDisplay"></param>
        /// <param name="pOffsetimg"></param>
        /// <returns></returns>
        // EG 20151220 Refactoring Diminution TAG HTML
        // EG 20201116 [25556] Réécriture du lien sur Hyperlink (Pb self.Close) + Modification id des contrôles Web de la ligne de menu
        // EG 20210226 [XXXXX] Réécriture du lien sur Hyperlink (Pb self.Close) Function Javascript OM(this,id)
        // EG 20210614 [25500] New Customer Portal Adaptation Image associée à un menu (en cas de connexion mode Customer Portal)
        // EG 20220311 [XXXXX] Pas de OnClick sur URL commençant par mailTo ou http
        private PlaceHolder SetActionMenuHTML(string pMainMenuName, string pDescriptionRes, int pOffsetimg)
        {
            PlaceHolder plh = new PlaceHolder();

            bool isUnavailable = (url.IndexOf("Unavailable.aspx") >= 0);
            bool isDirectURL = url.ToLower().StartsWith("mailto:") || url.StartsWith("http");
            string cssActionMenu = CstCSS.ActionMenu + pMainMenuName;
            if (isexternal && (-1 == pMainMenuName.IndexOf("External")))
                cssActionMenu = CstCSS.ActionMenu + "External";

            // EG 20151220 Refactoring Diminution TAG HTML
            if (isseparator)
                cssActionMenu = "mnusep-" + pMainMenuName;

            if (visibility == Cst.Visibility.MASK.ToString())
                cssActionMenu += " MaskMenu";


            Panel pnl = new Panel
            {
                ID = String.Format("mnu{0}", id),
                CssClass = cssActionMenu
            };

            if (false == isseparator)
            {
                if (false == Software.IsSoftwarePortal())
                {
                    Panel pnlImg = new Panel
                    {
                        CssClass = "button"
                    };
                    pnl.Controls.Add(pnlImg);
                }

                if (isUnavailable)
                {
                    Label lbl = new Label
                    {
                        Text = this.Displayname
                    };
                    pnl.Controls.Add(lbl);
                }
                else
                {
                    HyperLink lnk = new HyperLink();
                    if (Software.IsSoftwarePortal())
                        lnk.Text = "<i class='fas fa-chevron-right'></i> " + Displayname;
                    else
                        lnk.Text = Displayname;
                    if (isDirectURL)
                        lnk.NavigateUrl = url;
                    else
                        lnk.Attributes.Add("onclick", String.Format("OM(this, {0})", id));
                    if (StrFunc.IsFilled(pDescriptionRes))
                        lnk.Attributes.Add("alt", pDescriptionRes);
                    pnl.Controls.Add(lnk);
                }
            }
            plh.Controls.Add(pnl);

            pnl = new Panel
            {
                ID = String.Format("mnu{0}sub", id)
            };
            pnl.Style.Add(HtmlTextWriterStyle.PaddingLeft, pOffsetimg.ToString() + "pt");
            plh.Controls.Add(pnl);

            return plh;
        }
        /// <summary>
        /// Nouvelle méthode de chargement des menus action pour le sommaire dans la page Default.aspx 
        /// </summary>
        /// <returns></returns>
        /// EG 20220623 [XXXXX] New for Default.aspx
        public PlaceHolder AddActionMenuControl()
        {
            PlaceHolder plh = new PlaceHolder();
            bool isUnavailable = (url.IndexOf("Unavailable.aspx") >= 0);
            bool isMailTo = url.ToLower().StartsWith("mailto:");
            string cssActionMenu = CstCSS.Menu + MainMenuName;
            if (isexternal && (-1 == MainMenuName.IndexOf("External")))
                cssActionMenu = CstCSS.Menu + "External";

            if (isseparator)
                cssActionMenu = "mnu-sep";

            if (visibility == Cst.Visibility.MASK.ToString())
                cssActionMenu += " mask";


            Panel pnl = new Panel
            {
                ID = String.Format("amnu{0}", id),
                CssClass = cssActionMenu
            };

            if (false == isseparator)
            {
                if (isUnavailable)
                {
                    Label lbl = new Label
                    {
                        Text = this.Displayname
                    };
                    pnl.Controls.Add(lbl);
                }
                else
                {
                    HyperLink lnk = new HyperLink()
                    {
                        Text = Displayname
                    };


                    if (isMailTo)
                        lnk.NavigateUrl = url;
                    else
                        lnk.Attributes.Add("onclick", String.Format("OM(this, {0})", id));

                    string tooltip = GetToolTip();
                    if (StrFunc.IsFilled(tooltip))
                        lnk.Attributes.Add("alt", tooltip);
                    pnl.Controls.Add(lnk);
                }
            }
            plh.Controls.Add(pnl);

            pnl = new Panel
            {
                ID = String.Format("mnu{0}sub", id)
            };
            plh.Controls.Add(pnl);

            return plh;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pSb"></param>
        /// <param name="pMainMenuName"></param>
        /// <param name="pStyleDisplay"></param>
        // EG 20201116 [25556] Modification id des contrôles Web de la ligne de menu
        private PlaceHolder SetMainMenuHTML(string pMainMenuName, string pStyleDisplay)
        {
            PlaceHolder plh = new PlaceHolder();
            string cssMainMenu = CstCSS.MainMenu + pMainMenuName;
            if (visibility == Cst.Visibility.MASK.ToString())
                cssMainMenu += " MaskMenu";

            Panel pnl = new Panel
            {
                ID = String.Format("Mastermnu{0}", id),
                CssClass = cssMainMenu
            };
            pnl.Attributes.Add("onmouseout", String.Format("MenuOut(this,{0},'{1}','{2}');", level.ToString(), pMainMenuName, visibility));
            pnl.Attributes.Add("onmouseover", String.Format("MenuOver(this,{0},'{1}');", level.ToString(), pMainMenuName));
            pnl.Attributes.Add("onclick", String.Format("MainMenuDisplay('mnu{0}sub',this,{1},'{2}','{3}');", id, level.ToString(), pMainMenuName, visibility));
            Label lbl = new Label
            {
                Text = Displayname
            };
            pnl.Controls.Add(lbl);

            plh.Controls.Add(pnl);

            pnl = new Panel
            {
                ID = String.Format("mnu{0}sub", id),
                CssClass = String.Format("{0}-Bottom", CstCSS.MainMenu + pMainMenuName)
            };
            if (StrFunc.IsFilled(pStyleDisplay))
                pnl.Style.Add(HtmlTextWriterStyle.Display, "none");
            plh.Controls.Add(pnl);
            return plh;
        }
        /// <summary>
        /// Nouvelle méthode de chargement des menus principaux pour le sommaire dans la page Default.aspx 
        /// </summary>
        /// <returns></returns>
        /// EG 20220623 [XXXXX] New for Default.aspx
        /// EG 20221029 [XXXXX] Refactoring de Default.js (Suite lenteur sur PEEK des reponses de traitement sur MSSMQ)
        public PlaceHolder AddMainMenuControl()
        {
            PlaceHolder plh = new PlaceHolder();
            string cssMainMenu = CstCSS.Menu + MainMenuName;
            if (visibility == Cst.Visibility.MASK.ToString())
                cssMainMenu += " mask";

            Panel pnl = new Panel
            {
                ID = String.Format("mnu{0}", id),
                CssClass = cssMainMenu
            };
            pnl.Attributes.Add("onclick", String.Format("_MenuToggle('{0}',{1});", id, level));

            Label lbl = new Label
            {
                Text = Displayname
            };
            pnl.Controls.Add(lbl);

            plh.Controls.Add(pnl);

            pnl = new Panel
            {
                ID = String.Format("mnu{0}sub", id)
            };
            plh.Controls.Add(pnl);
            return plh;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pSb"></param>
        /// <param name="pMainMenuName"></param>
        /// <param name="pDescriptionRes"></param>
        /// <param name="pStyleDisplay"></param>
        // EG 20201116 [25556] Modification id des contrôles Web de la ligne de menu
        // EG 20210614 [25500] New Customer Portal Adaptation Image associée à un menu (en cas de connexion mode Customer Portal)
        private PlaceHolder SetSubMenuHTML(string pMainMenuName, string pDescriptionRes, string pStyleDisplay)
        {
            PlaceHolder plh = new PlaceHolder();

            string cssSubMenu = CstCSS.SubMenu + pMainMenuName;
            string cssSubMenuTxt = CstCSS.SubMenuTxt + pMainMenuName;
            if (isexternal && (-1 == pMainMenuName.IndexOf("External")))
            {
                cssSubMenu += "External";
                cssSubMenuTxt += "External";
            }

            if (visibility == Cst.Visibility.MASK.ToString())
                cssSubMenu += " MaskMenu";

            Panel pnl = new Panel
            {
                ID = String.Format("mnu{0}", id),
                CssClass = cssSubMenu
            };
            string mnu = pMainMenuName + (isexternal ? "External":string.Empty);
            pnl.Attributes.Add("onmouseout", String.Format("MenuOut(this,{0},'{1}','{2}');", level, mnu, visibility));
            pnl.Attributes.Add("onmouseover", String.Format("MenuOver(this,{0},'{1}');", level, mnu));
            pnl.Attributes.Add("onclick", String.Format("MainMenuDisplay('mnu{0}sub',this,{1},'{2}','{3}');", id, level, mnu, visibility));

            Panel pnlImg = new Panel
            {
                CssClass = "button"
            };
            pnl.Controls.Add(pnlImg);

            Label lbl = new Label();
            if (Software.IsSoftwarePortal())
                lbl.Text = "<i class='fas fa-angle-double-right'></i> " + Displayname;
            else
                lbl.Text = Displayname;

            lbl.ID = String.Format("lblmnu{0}", id);
            lbl.CssClass = cssSubMenuTxt;
            lbl.Attributes.Add("onmouseout", String.Format("this.className='{0}';", cssSubMenuTxt));
            lbl.Attributes.Add("onmouseover", String.Format("this.className='{0}';", cssSubMenuTxt));
            if (StrFunc.IsFilled(pDescriptionRes))
                lbl.Attributes.Add("alt", pDescriptionRes);
            pnl.Controls.Add(lbl);
            plh.Controls.Add(pnl);

            pnl = new Panel
            {
                ID = String.Format("mnu{0}sub", id)
            };
            if (StrFunc.IsFilled(pStyleDisplay))
                pnl.Style.Add(HtmlTextWriterStyle.Display, "none");
            //pnl.Style.Add(HtmlTextWriterStyle.PaddingLeft, "14pt");
            plh.Controls.Add(pnl);

            return plh;
        }
        /// <summary>
        /// Nouvelle méthode de chargement des sous-menus pour le sommaire dans la page Default.aspx 
        /// </summary>
        /// <returns></returns>
        /// EG 20220623 [XXXXX] New for Default.aspx
        public PlaceHolder AddSubMenuControl()
        {
            PlaceHolder plh = new PlaceHolder();

            string cssSubMenu = CstCSS.Menu + MainMenuName;
            if (isexternal && (-1 == MainMenuName.IndexOf("External")))
                cssSubMenu += "External";

            if (visibility == Cst.Visibility.MASK.ToString())
                cssSubMenu += " mask";

            Panel pnl = new Panel
            {
                ID = String.Format("mnu{0}", id),
                CssClass = cssSubMenu
            };
            _ = MainMenuName + (isexternal ? "External" : string.Empty);
            pnl.Attributes.Add("onclick", String.Format("_MenuToggle('{0}',{1});", id, level));

            Label lbl = new Label() {
                ID = String.Format("lblmnu{0}", id),
                Text = "<i class='fas fa-angle-right'></i> " + Displayname,
            };

            string tooltip = GetToolTip();
            if (StrFunc.IsFilled(tooltip))
                lbl.Attributes.Add("alt", GetToolTip());
            pnl.Controls.Add(lbl);
            plh.Controls.Add(pnl);

            pnl = new Panel
            {
                ID = String.Format("mnu{0}sub", id)
            };
            plh.Controls.Add(pnl);

            return plh;
        }

        /// <summary>
        /// Retourne un ToolTip
        /// <para>Les tooltips sont issus de la traduction de la propriété description</para>
        /// </summary>
        /// <returns></returns>
        public string GetToolTip()
        {
            string description_res = Ressource.GetString(description, true);
            if (!isexternal && (description_res == description))
                //Il n'existe aucune ressource
                description_res = string.Empty;
            return description_res;
        }

        /// <summary>
        /// Supprime l'éventuel paramètre inputMode d'une URL
        /// </summary>
        /// <param name="pURL">Représente l'URL</param>
        /// <returns></returns>
        public static string RemoveInputMode(string pURL)
        {
            string ret = pURL;
            //
            bool isFound = false;
            foreach (Cst.DataGridMode mode in Enum.GetValues(typeof(Cst.DataGridMode)))
            {
                for (int i = 0; i < 2; i++)
                {
                    string inputMode = (0 == i ? "&" : "?") + "inputMode=" + ((int)mode).ToString();
                    if (pURL.Contains(inputMode))
                    {
                        ret = pURL.Replace(inputMode, string.Empty);
                        isFound = true;
                        break;
                    }

                }
                if (isFound)
                    break;
            }
            //
            return ret;
        }
        /// <summary>
        /// Le menu est-il autorisé à être affiché
        /// </summary>
        /// EG 20230306 [26279] New
        public bool IsDisplaying
        {
            get
            {
                bool isDisplaying = IsEnabled && (Level > 0);
                Cst.Visibility mnuVisibility = (Cst.Visibility)Enum.Parse(typeof(Cst.Visibility), Visibility, true);
                if (isDisplaying)
                    isDisplaying = (mnuVisibility == Cst.Visibility.SHOW) || (mnuVisibility == Cst.Visibility.MASK);
                return isDisplaying;
            }
        }
        #endregion

        #region ICloneable Membres

        public object Clone()
        {
            Menu menu = new Menu
            {
                id = id,

                idmenu = idmenu,
                idmenu_parent = idmenu_parent,
                level = level,
                icon = icon,
                displayname = displayname,
                description = description,
                url = url,
                isexternal = isexternal,
                isaction = isaction,
                isseparator = isseparator,
                ismovable = ismovable,

                visibility = visibility,
                extllink = extllink,
                ispublic = ispublic,
                isenabled = isenabled,
                isenabledSpecified = isenabledSpecified,
                html = html,
                border = border,
                placeHolder = placeHolder
            };
            return menu;
        }

        #endregion
    }
    #endregion Menu

    #region public class Menus
    /// <summary>
    /// Représente la liste des menus de Spheres
    /// </summary>
    public class Menus : ArrayList
    {
        public static Menus operator +(Menus l, Menu c)
        {
            l.Add(c);
            return l;
        }
        public new Menu this[int i]
        {
            get { return (Menu)base[i]; }
            set { base[i] = value; }
        }

        /// <summary>
        /// Retourne true si le menu {pIDMenu} est parent d'un autre menu
        /// </summary>
        /// <param name="pIDMenu"></param>
        /// <returns></returns>
        public bool IsParent(string pIdMenu)
        {
            pIdMenu = TranslateMenu(pIdMenu);

            for (int i = 0; i < Count; i++)
            {
                if (((Menu)base[i]).IdMenu_Parent == pIdMenu)
                    return true;
            }

            return false;
        }
        //PL 20120606
        public string TranslateMenu(string pIdMenu)
        {
            string idMenu = pIdMenu;

            if (idMenu == "OTC_REF_ACT_ACT_PARENTLST" || idMenu == "OTC_REF_ACT_ACT_CHILDLST")
                //PL 20120606 Tip for Actor Parent/Child
                idMenu = "OTC_REF_ACT_ACT";
            else if (idMenu == "OTC_VIEW_IOTRACK" && Software.IsSoftwareVision())
                //PL 20101005 Tip for Vision
                idMenu = "VISION_ADM_SAF_LOG_IOTRACK";

            return idMenu;
        }

        /// <summary>
        /// Obtient true si le menu {pIDMenu} existe
        /// </summary>
        /// <param name="pIDMenu"></param>
        /// <returns></returns>
        public bool IsExistIDMenu(string pIdMenu)
        {
            bool IsOk;
            IsOk = false;
            for (int i = 0; i < Count; i++)
            {
                if (((Menu)base[i]).IdMenu == pIdMenu)
                    IsOk = true;
            }
            return IsOk;
        }

        /// <summary>
        /// Obtient true si le menu {pID} existe
        /// </summary>
        /// <param name="pIDMenu"></param>
        /// <returns></returns>
        /// EG 20151019 [21465] New
        public bool IsExistID(string pId)
        {
            bool IsOk;
            IsOk = false;
            for (int i = 0; i < Count; i++)
            {
                if (((Menu)base[i]).Id == pId)
                    IsOk = true;
            }
            return IsOk;
        }

        /// <summary>
        /// Retourne true si le menu {pIDMenu} à plusieurs parents
        /// </summary>
        /// <param name="pIDMenu"></param>
        /// <returns></returns>
        /// EG 20151019 [21465] New Un menu a-t-il plusieurs pères (ex: dénouementde masse)
        public bool HasSeveralParent(string pIdMenu)
        {
            int nbParent = 0;
            pIdMenu = TranslateMenu(pIdMenu);

            for (int i = 0; i < Count; i++)
            {
                if (((Menu)base[i]).IdMenu == pIdMenu)
                    nbParent++;
            }
            return (1 < nbParent);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            string liste = "(";
            int i;
            for (i = 0; i < Count; i++)
            {
                liste += "[" + base[i] + "]" + ",";
            }
            //20090226 PL Mise en commentaire du if() suivant car plantage ave cl adirective de page: Trace="true"
            //if (Count!=0) 
            //    liste+="["+base[i]+"]";
            liste += ")";
            return liste;
        }

        /// <summary>
        /// Retourne le menu dont l'identifiant est {pIdMenu}
        /// <para>Retourne null si aucun menu trouvé</para>
        /// </summary>
        /// <param name="pIDMenu"></param>
        /// <returns></returns>
        public Menu SelectByIDMenu(string pIdMenu)
        {
            Menu menu = null;
            for (int i = 0; i < Count; i++)
            {
                if (((Menu)base[i]).IdMenu == pIdMenu)
                {
                    menu = (Menu)base[i];
                    break;
                }
            }
            return menu;
        }

        /// <summary>
        /// Retourne le menu dont l'ID unique est {pId}
        /// <para>Retourne null si aucun menu trouvé</para>
        /// </summary>
        /// <param name="pIDMenu"></param>
        /// <returns></returns>
        /// EG 20151019 [21465] New 
        public Menu SelectByID(string pId)
        {
            Menu menu = null;
            for (int i = 0; i < Count; i++)
            {
                if (((Menu)base[i]).Id == pId)
                {
                    menu = (Menu)base[i];
                    break;
                }
            }
            return menu;
        }

        /// <summary>
        /// Retourne le menu dont l'URL contient {pURL}
        /// </summary>
        /// <param name="pURL"></param>
        /// <returns></returns>
        public Menu SelectByURL(string pURL)
        {
            Menu menu = null;
            for (int i = 0; i < Count; i++)
            {
                if (((Menu)base[i]).Url.IndexOf(pURL) > 0)
                {
                    menu = (Menu)base[i];
                    break;
                }
            }
            return menu;
        }

        /// <summary>
        /// Retourne le displayName du menu {pIdMenu}
        /// <para>Retourne null si le menu n'existe pas</para>
        /// </summary>
        /// <param name="pIdMenu"></param>
        /// <returns></returns>
        public string GetMenu_DisplayName(string pIdMenu)
        {
            string ret = null;
            if (IsExistIDMenu(pIdMenu))
                ret = SelectByIDMenu(pIdMenu).Displayname;
            return ret;
        }

        /// <summary>
        /// Retourne l'URL associé au menu {pIdMenu}
        /// <para>Retourne null si le menu n'existe pas</para>
        /// </summary>
        /// <param name="pIdMenu"></param>
        /// <returns></returns>
        public string GetMenu_Url(string pIdMenu)
        {
            string ret = null;
            if (IsExistIDMenu(pIdMenu))
                ret = SelectByIDMenu(pIdMenu).Url;
            return ret;
        }

        /// <summary>
        /// Retourne le menu parent via {pId} id unique
        /// <para>Retourne null si le menu n'existe pas</para>
        /// </summary>
        /// <param name="pIdMenu"></param>
        /// <returns></returns>
        /// EG 20151019 [21465] New
        public Menu GetMenuParentById(string pId)
        {
            Menu ret = null;
            if (IsExistID(pId))
            {
                string idParent = SelectByID(pId).IdMenu_Parent;
                if (StrFunc.IsFilled(idParent))
                    ret = SelectByIDMenu(idParent);
            }
            return ret;
        }

        /// <summary>
        /// Retourne l'URL asscociée à un menu après ajout des arguments PK, PKV et IDMENU
        /// </summary>
        /// <param name="pPKName"></param>
        /// <param name="pPKValue"></param>
        /// <param name="pIdMenu"></param>
        /// <returns></returns>
        public string GetMenu_AddPKArgument(string pPKName, string pPKValue, string pIdMenu)
        {
            string ret = GetMenu_Url(pIdMenu);
            
            if (StrFunc.IsFilled(ret))
            {
                ret = AspTools.AddIdMenuOnUrl(ret, pIdMenu);
                ret += "&PK=" + pPKName + "&PKV=" + pPKValue;
            }
            return ret;
        }

        /// <summary>
        ///  Copie les éléments vers un nouveau tableau
        /// </summary>
        /// <returns></returns>
        /// FI 20220503 [XXXXX] Add for linq usage
        public new Menu[] ToArray()
        {
            return (Menu[])this.ToArray(typeof(Menu));
        }
    }
    #endregion

    #region public class MenuChecking
    /// <summary>
    /// Gestion des menus.
    /// </summary>
    public class MenuChecking
    {
        #region Members
        private readonly string m_MenuRoot;
        private readonly int m_Ida;
        private readonly string m_Role;
        private readonly string m_CS;
        private DataTable m_dtMenus;
        private Menus m_menus;
        #endregion Members

        #region Accesseur(s)
        public Menus Menus
        {
            get { return m_menus; }
        }
        #endregion Accesseur(s)

        #region Constructeur(s)
        public MenuChecking(string pConnectionString, int pIda, ActorAncestor pActorAncestor, string Role, string pMenuRoot, string pStyleDisplay, string pBorder)
        {
            m_Ida = pIda;
            m_Role = Role;
            m_CS = pConnectionString;
            m_MenuRoot = pMenuRoot;
            m_menus = new Menus();
            LoadMenu(pActorAncestor, pStyleDisplay, pBorder);
        }
        #endregion Constructeur(s)

        #region private LoadMenu
        private void LoadMenu(ActorAncestor pActorAncestor, string pStyleDisplay, string pBorder)
        {

            //Constitution d'un datatable des menus depuis les tables SQL
            LoadMenuByLevel();

            #region Update column ISENABLED	from Filter
            //Maj successive de la colonne ISENABLED de la table (Rq.: Tjs enabled si SYSADMIN)
            if (!ActorTools.IsUserType_SysAdmin(m_Role))
            {
                SetMenuIsEnabledByActor(pActorAncestor);
            }
            #endregion

            #region Update column ISENABLED	from License
            string url, idmenu;
            //Note: Les restrictions sur les consultations sont applicable uniquement sur Spheres Vision (PL)
            bool isIOAuthorised = !Software.IsSoftwareVision();
            bool isGatewayBCSAuthorised = false;
            bool isGatewayFIXMLEurexAuthorised = false;
            bool isConsultationAuthorised = !Software.IsSoftwareVision();

            //PL 20120926 Gestion de isTradeAdminAuthorised et de isTradeCashBalanceInterestAuthorised 
            //bool isTradeAdminAuthorised = false;
            ////GLOP 20081105 PL En dur pour l'instant pour éviter une nouvelle licence...
            //#if DEBUG
            //            isTradeAdminAuthorised = true;
            //#else
            //            isTradeAdminAuthorised = (SessionTools.License.licensee == "EFSLIC") 
            //                            || (SessionTools.License.licensee == "OTCEX")
            //                            || (SessionTools.License.licensee == "SIGMA");
            //#endif
            bool isTradeAdminAuthorised = SessionTools.License.IsLicProductAuthorised_Add(LimitationProductEnum.invoice);
            bool isTradeCashBalanceInterestAuthorised = SessionTools.License.IsLicProductAuthorised_Add(LimitationProductEnum.cashBalanceInterest);

            //CC 20140110 
            //CC 20140110 
            #region User License (Functionality\PRISMA)
            bool isPrismaOnlyAuthorised = SessionTools.License.IsLicFunctionalityAuthorised(LimitationFunctionalityEnum.PRISMAONLY);
            #endregion

            #region User License (Functionality\CONSULTATION)
            if (!isConsultationAuthorised)
            {
                isConsultationAuthorised = SessionTools.License.IsLicFunctionalityAuthorised(LimitationFunctionalityEnum.CONSULTATION);
            }
            #endregion
            #region User License (Service\SpheresIO)
            if (!isIOAuthorised)
            {
                isIOAuthorised = SessionTools.License.IsLicServiceAuthorised(Cst.ServiceEnum.SpheresIO);
            }
            #endregion
            #region User License (Service\Gateway)
            if (!isGatewayBCSAuthorised)
            {
                isGatewayBCSAuthorised = SessionTools.License.IsLicServiceAuthorised(Cst.ServiceEnum.SpheresGateBCS);
            }
            if (!isGatewayFIXMLEurexAuthorised)
            {
                isGatewayFIXMLEurexAuthorised = SessionTools.License.IsLicServiceAuthorised(Cst.ServiceEnum.SpheresGateFIXMLEurex);
            }
            int i;
            #endregion

            for (i = 0; i < m_dtMenus.Rows.Count; i++)
            {
                url = m_dtMenus.Rows[i]["URL"].ToString();
                idmenu = m_dtMenus.Rows[i]["IDMENU"].ToString();

                #region LIST.ASPX
                if (url.ToUpper().StartsWith("LIST.ASPX"))
                {

                    if ((!isTradeCashBalanceInterestAuthorised) && ((url.IndexOf("=CASHINTEREST") >= 0) || (url.IndexOf("P1=CBI") >= 0)))
                    {
                        m_dtMenus.Rows[i]["ISENABLED"] = false;
                        m_dtMenus.Rows[i]["ISENABLEDSpecified"] = true;
                    }
                    // CC 20150602
                    if ((!isTradeAdminAuthorised) && ((url.IndexOf("ProcessBase=INV") >= 0) || (url.IndexOf("Invoicing=") >= 0) || (url.IndexOf("ProcessName=InvoicingGen") >= 0) || (url.IndexOf("ConfirmationMsg=INV_MCO") >= 0) || (url.IndexOf("P1=ADMIN") >= 0)))
                    {
                        m_dtMenus.Rows[i]["ISENABLED"] = false;
                        m_dtMenus.Rows[i]["ISENABLEDSpecified"] = true;
                    }

                    else if ((url.IndexOf("Consultation") >= 0))
                    {
                        if (!isConsultationAuthorised)
                        {
                            m_dtMenus.Rows[i]["ISENABLED"] = false;
                            m_dtMenus.Rows[i]["ISENABLEDSpecified"] = true;
                        }
                        else if ((!isTradeAdminAuthorised) && (url.IndexOf("Consultation=TRADEADMIN") >= 0))
                        {
                            m_dtMenus.Rows[i]["ISENABLED"] = false;
                            m_dtMenus.Rows[i]["ISENABLEDSpecified"] = true;
                        }
                    }
                    else if ((url.IndexOf("Referential=IO") >= 0))
                    {
                        if (!isIOAuthorised)
                        {
                            m_dtMenus.Rows[i]["ISENABLED"] = false;
                            m_dtMenus.Rows[i]["ISENABLEDSpecified"] = true;
                        }
                    }
                    //CC 20140110
                    else if (
                            (url.IndexOf("=ACTORACTOR") >= 0)
                            || (url.IndexOf("=ACTORAMOUNT") >= 0)
                            || (url.IndexOf("=ACTORGROUP") >= 0)
                            || (url.IndexOf("=ACTORHOST") >= 0)
                            || (url.IndexOf("=ACTORINSTRUMENT") >= 0)
                            || (url.IndexOf("=ACTORMARKET") >= 0)
                            || (url.IndexOf("=ADDRESSCOMPL") >= 0)
                            || (url.IndexOf("=BUSINESSCENTER") >= 0)
                            || (url.IndexOf("=CASHBALANCE") >= 0)
                            || (url.IndexOf("=CLEARINGCOMPART") >= 0)
                            || (url.IndexOf("=COUNTRY") >= 0)
                            || (url.IndexOf("=CSMID") >= 0)
                            || (url.IndexOf("=CSSLINK") >= 0)
                            || (url.IndexOf("=CURRENCY") >= 0)
                            || (url.IndexOf("=ENTITY") >= 0)
                            || (url.IndexOf("=EVENTENUM") >= 0)
                            || (url.IndexOf("=MARGINTRACK") >= 0)
                            || (url.IndexOf("=MARKET") >= 0)
                            || (url.IndexOf("=MATURITY") >= 0)
                            || (url.IndexOf("P1=MODELACTOR") >= 0)
                            || (url.IndexOf("P1=MODELINSTRUMENT") >= 0)
                            || (url.IndexOf("P1=MODELMARKET") >= 0)
                            || (url.IndexOf("=NCS") >= 0)
                            || (url.IndexOf("=POSREQUEST") >= 0)
                            || (url.IndexOf("=RATEINDEX") >= 0)
                            || (url.IndexOf("=RISKMARGIN") >= 0)
                            || (url.IndexOf("=ROLEG") >= 0)
                            || (url.IndexOf("=SSI") >= 0)
                            )
                    {
                        if (isPrismaOnlyAuthorised)
                        {
                            m_dtMenus.Rows[i]["ISENABLED"] = false;
                        }
                    }
                }
                #endregion
                //CC 20140110
                #region BUSINESSMONITORING.ASPX
                else if (url.ToUpper().StartsWith("BUSINESSMONITORING.ASPX"))
                {
                    if (isPrismaOnlyAuthorised)
                    {
                        m_dtMenus.Rows[i]["ISENABLED"] = false;
                    }
                }
                #endregion
                #region RUNIO.ASPX
                else if (url.ToUpper().StartsWith("RUNIO.ASPX"))
                {
                    if (!isIOAuthorised)
                    {
                        m_dtMenus.Rows[i]["ISENABLED"] = false;
                        m_dtMenus.Rows[i]["ISENABLEDSpecified"] = true;
                    }
                }
                #endregion 
                #region Gateway Menus
                else if (idmenu.ToUpper().StartsWith(Software.MenuRoot() + "_PROCESS_GATEWAY"))
                {
                    if (idmenu.ToUpper() == Software.MenuRoot() + "_PROCESS_GATEWAY" && !isGatewayBCSAuthorised && !isGatewayFIXMLEurexAuthorised)
                    {
                        m_dtMenus.Rows[i]["ISENABLED"] = false;
                        m_dtMenus.Rows[i]["ISENABLEDSpecified"] = true;
                    }
                    else if (idmenu.ToUpper().StartsWith(Software.MenuRoot() + "_PROCESS_GATEWAY_BCS") && !isGatewayBCSAuthorised)
                    {
                        m_dtMenus.Rows[i]["ISENABLED"] = false;
                        m_dtMenus.Rows[i]["ISENABLEDSpecified"] = true;
                    }
                    else if (idmenu.ToUpper().StartsWith(Software.MenuRoot() + "_PROCESS_GATEWAY_EUREX") && !isGatewayFIXMLEurexAuthorised)
                    {
                        m_dtMenus.Rows[i]["ISENABLED"] = false;
                        m_dtMenus.Rows[i]["ISENABLEDSpecified"] = true;
                    }
                }
                #endregion 
                #region TRADEADMINCAPTUREPAGE.ASPX
                else if (url.ToUpper().StartsWith("TRADEADMINCAPTUREPAGE.ASPX"))
                {
                    if (!isTradeAdminAuthorised)
                    {
                        m_dtMenus.Rows[i]["ISENABLED"] = false;
                        m_dtMenus.Rows[i]["ISENABLEDSpecified"] = true;
                    }
                }
                #endregion
                // CC 20150602
                #region TRADERISKCAPTUREPAGE.ASPX
                else if (url.ToUpper().StartsWith("TRADERISKCAPTUREPAGE.ASPX"))
                {
                    if (!isTradeCashBalanceInterestAuthorised)
                    {
                        m_dtMenus.Rows[i]["ISENABLED"] = false;
                        m_dtMenus.Rows[i]["ISENABLEDSpecified"] = true;
                    }
                }
                #endregion
                #region Group Menus (StrFunc.IsEmpty(url))
                else if (StrFunc.IsEmpty(url))
                {
                    if (idmenu.ToUpper() == Software.MenuRoot() + "_ADM_TOOL_IO")
                        if (!isIOAuthorised)
                        {
                            m_dtMenus.Rows[i]["ISENABLED"] = false;
                            m_dtMenus.Rows[i]["ISENABLEDSpecified"] = true;
                        }

                    //Code rajouté pour masquer les menus de VISION-Consul dans le cas ou on utilise VISION-IO
                    if ((idmenu.ToUpper() == Software.MenuRoot() + "_ADM_GROUP") || (idmenu.ToUpper() == Software.MenuRoot() + "_ADM_MNU") || (idmenu.ToUpper() == Software.MenuRoot() + "_ADM_PERM"))
                        if (!isConsultationAuthorised)
                        {
                            m_dtMenus.Rows[i]["ISENABLED"] = false;
                            m_dtMenus.Rows[i]["ISENABLEDSpecified"] = true;
                        }

                    //CC 20140110
                    if (
                        (idmenu.ToUpper() == Software.MenuRoot() + "_ADM_TUNING")
                        || (idmenu.ToUpper() == Software.MenuRoot() + "_INP")
                        || (idmenu.ToUpper() == Software.MenuRoot() + "_INV")
                        || (idmenu.ToUpper() == Software.MenuRoot() + "_PROCESS_ACCOUNTING")
                        // CC 20150910
                        //|| (idmenu.ToUpper() == Software.MenuRoot() + "_PROCESS_EXCHANGEDAY")
                        || (idmenu.ToUpper() == Software.MenuRoot() + "_PROCESS_FLOW_BALANCE")
                        || (idmenu.ToUpper() == Software.MenuRoot() + "_PROCESS_MAIL")
                        || (idmenu.ToUpper() == Software.MenuRoot() + "_PROCESS_POSITION_RISK")
                        || (idmenu.ToUpper() == Software.MenuRoot() + "_PROCESS_REGULATORY")
                        // CC 20150910
                        //|| (idmenu.ToUpper() == Software.MenuRoot() + "_PROCESS_VALUATION")
                        || (idmenu.ToUpper() == Software.MenuRoot() + "_PROCESS_VALUATION")
                        || (idmenu.ToUpper() == Software.MenuRoot() + "_REF_ACC")
                        || (idmenu.ToUpper() == Software.MenuRoot() + "_REF_ACT_ACT_RELATIONS")
                        || (idmenu.ToUpper() == Software.MenuRoot() + "_REF_ACT_BOOK_ACCOUNTS")
                        || (idmenu.ToUpper() == Software.MenuRoot() + "_REF_ACT_INSTRUCTIONS")
                        || (idmenu.ToUpper() == Software.MenuRoot() + "_REF_CHARGE")
                        || (idmenu.ToUpper() == Software.MenuRoot() + "_REF_DATA")
                        || (idmenu.ToUpper() == Software.MenuRoot() + "_REF_GRP")
                        || (idmenu.ToUpper() == Software.MenuRoot() + "_REF_MAIL")
                        || (idmenu.ToUpper() == Software.MenuRoot() + "_REF_PRD")
                        || (idmenu.ToUpper() == Software.MenuRoot() + "_VIEW_ACCOUNTING")
                        // CC 20150910 
                        || (idmenu.ToUpper() == Software.MenuRoot() + "_VIEW_DEPOSIT_CASHBALANCE")
                        || (idmenu.ToUpper() == Software.MenuRoot() + "_VIEW_OTC")
                        || (idmenu.ToUpper() == Software.MenuRoot() + "_VIEW_FO")
                        || (idmenu.ToUpper() == Software.MenuRoot() + "_VIEW_MAIL")
                        )
                        if (isPrismaOnlyAuthorised)
                        {
                            m_dtMenus.Rows[i]["ISENABLED"] = false;
                        }

                }
                #endregion
            }
            #endregion
            //	
            //Load list of menus
            DataRow[] rows;
            // FI 20240326 [WI884] remove if (i > 0)
            for (i = 0; i < m_dtMenus.Rows.Count; i++)
            {
                DataRow  dataRow = m_dtMenus.Rows[i];

                bool isMenuGroup = (dataRow["URL"] == DBNull.Value) || (StrFunc.IsEmpty(Convert.ToString(dataRow["URL"])));
                bool isPublic = Convert.ToBoolean(dataRow["ISPUBLIC"]);
                bool isEnabledSpecified = Convert.ToBoolean(dataRow["ISENABLEDSpecified"]);
                int id = Convert.ToInt32(dataRow["ID"]);
                
                string idMenu_Parent = Convert.ToString(dataRow["IDMENU_MENU"]);
                int level = Convert.ToInt32(dataRow["LEVEL"]);
                if (!isEnabledSpecified)
                {
                    int idBrother;
                    if (isMenuGroup && !isPublic)
                    {
                        //Note: Tout est basé sur l'utilisation de ID qui est croissant
                        //
                        //Menu Group non public: On le met enabled s'il contient au moins un menu Action enabled
                        //--------------------------------------------------------------------------------------
                        // a1/Recherche du prochain frère  
                        rows = m_dtMenus.Select("ID > " + id.ToString()
                                        + " And IDMENU_MENU = " + DataHelper.SQLString(idMenu_Parent)
                                        + " And LEVEL = " + level.ToString()
                                        , "ID");
                        if (rows.Length > 0)
                        {
                            idBrother = Convert.ToInt32(rows[0]["ID"]);
                        }
                        else
                        {
                            // a2/Recherche du prochain frère du père 
                            rows = m_dtMenus.Select("ID > " + id.ToString()
                                            + " And LEVEL = " + (level - 1).ToString()
                                            , "ID");
                            if (rows.Length > 0)
                            {
                                idBrother = Convert.ToInt32(rows[0]["ID"]);
                            }
                            else
                            {
                                idBrother = 99999;
                            }
                        }
                        // b/Recherche s'il existe un enfant de type menu Action enabled
                        rows = m_dtMenus.Select("ID > " + id.ToString()
                                        + " And ID < " + idBrother.ToString()
                                        + " And LEN(URL) > 0"
                                        + " And ((ISENABLEDSpecified=1 And ISENABLED=1) Or (ISENABLEDSpecified=0 And ISPUBLIC=1))"
                                        , "ID");
                        if (rows.Length > 0)
                        {
                            dataRow["ISENABLED"] = true;
                            dataRow["ISENABLEDSpecified"] = true;
                        }
                    }
                    else if (isMenuGroup && isPublic)
                    {
                        //Menu Group public: On le met disabled s'il ne contient aucun menu Action enabled.
                        //TODO
                    }
                }

                // EG 20100916 On affecte le même attribut VISIBILITY du parent vers les enfants (si <> SHOW)
                string visibility = Convert.ToString(dataRow["VISIBILITY"]);
                if (StrFunc.IsFilled(idMenu_Parent))
                {
                    Menu menuParent = m_menus.SelectByIDMenu(idMenu_Parent);
                    if (null != menuParent)
                    {
                        string visibilityParent = m_menus.SelectByIDMenu(idMenu_Parent).Visibility;
                        if ((visibilityParent == Cst.Visibility.HIDE.ToString()) ||
                            (visibilityParent == Cst.Visibility.MASK.ToString()) && (visibility == Cst.Visibility.SHOW.ToString()))
                            visibility = visibilityParent;
                    }
                }
                m_menus += new Menu(dataRow, pStyleDisplay, pBorder, visibility);
            }

        }
        #endregion

        #region private SetMenuIsEnabledByActor
        private void SetMenuIsEnabledByActor(ActorAncestor pActorAncestor)
        {

            //Récupération des users parents du current user
            ActorAncestor actorAncestor = pActorAncestor;
            //
            if (null != actorAncestor)
            {
                _ = new ArrayList();     //alAncestors
                ArrayList relations = actorAncestor.GetRelations();

                string sqlQuery = SQLCst.SELECT + "mnmd.IDMENU, mnmd.ISENABLED" + Cst.CrLf;
                sqlQuery += SQLCst.FROM_DBO + Cst.OTCml_TBL.ACTOR + " a" + Cst.CrLf;
                sqlQuery += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.ACTORMENU + " am on (am.IDA=a.IDA)" + Cst.CrLf;
                sqlQuery += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.MODELMENU + " mm on (mm.IDMODELMENU = am.IDMODELMENU)" + Cst.CrLf;
                sqlQuery += SQLCst.AND + OTCmlHelper.GetSQLDataDtEnabled(m_CS, "mm") + Cst.CrLf;
                sqlQuery += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.MENUMODEL + " mnmd on (mnmd.IDMODELMENU = mm.IDMODELMENU)" + Cst.CrLf;
                sqlQuery += SQLCst.WHERE + " a.IDA=@ida" + Cst.CrLf;
                //Rule: On traite d'abord les not IsEnabled afin de rendre prioritaire (par écrasement) les IsEnabled
                sqlQuery += SQLCst.ORDERBY + " mnmd.ISENABLED" + SQLCst.ASC;
                sqlQuery = DataHelper.TransformQuery2(m_CS, CommandType.Text, sqlQuery, null);
                //
                DataSet dsMnu;
                DataTable dtMnu;
                DataRow[] drMenu;

                ArrayList aActorRole;
                //	
                for (int level = relations.Count - 1; level >= 0; level--)    // On part des parents issus de la relation la plus élévés
                {
                    aActorRole = (ArrayList)relations[level];        // Liste des acteurs Ancestors du level en cours
                    for (int i = 0; i < aActorRole.Count; i++)
                    {
                        string sql = sqlQuery.Replace("@ida", ((ActorRelation)aActorRole[i]).ActorRole.IdA_Actor.ToString());
                        dsMnu = DataHelper.ExecuteDataset(m_CS, CommandType.Text, sql);
                        dtMnu = dsMnu.Tables[0];
                        for (int j = 0; j < dtMnu.Rows.Count; j++)
                        {
                            drMenu = m_dtMenus.Select("IDMENU=" + DataHelper.SQLString(dtMnu.Rows[j]["IDMENU"].ToString()));
                            if (drMenu.Length > 0)
                            {
                                // RD 20120605 [17862]
                                for (int k = 0; k < ArrFunc.Count(drMenu); k++)
                                {
                                    drMenu[k]["ISENABLED"] = dtMnu.Rows[j]["ISENABLED"];
                                    drMenu[k]["ISENABLEDSpecified"] = true;
                                }
                            }
                        }
                    }
                }
            }

        }
        #endregion

        #region private LoadMenuByLevel
        private void LoadMenuByLevel()
        {

            #region Init datatable m_dtMenus
            m_dtMenus = new DataTable();
            m_dtMenus.Columns.Add("ID", System.Type.GetType("System.Int32"));
            m_dtMenus.Columns.Add("IDMENU", System.Type.GetType("System.String"));
            m_dtMenus.Columns.Add("IDMENU_MENU", System.Type.GetType("System.String"));
            m_dtMenus.Columns.Add("LEVEL", System.Type.GetType("System.Int32"));
            m_dtMenus.Columns.Add("ICON", System.Type.GetType("System.String"));
            m_dtMenus.Columns.Add("DISPLAYNAME", System.Type.GetType("System.String"));
            m_dtMenus.Columns.Add("DESCRIPTION", System.Type.GetType("System.String"));
            m_dtMenus.Columns.Add("URL", System.Type.GetType("System.String"));
            m_dtMenus.Columns.Add("ISACTION", System.Type.GetType("System.Boolean"));
            m_dtMenus.Columns.Add("ISSEPARATOR", System.Type.GetType("System.Boolean"));
            m_dtMenus.Columns.Add("ISMOVABLE", System.Type.GetType("System.Boolean"));
            // CC 20100908 MENU.ISCHILDONLY and MENUOF.ISCHILDONLY replaced by column VISIBILITY cf Ticket 17143
            //m_dtMenus.Columns.Add("ISCHILDONLY", System.Type.GetType("System.Boolean"));
            m_dtMenus.Columns.Add("VISIBILITY", System.Type.GetType("System.String"));
            m_dtMenus.Columns.Add("ISEXTERNAL", System.Type.GetType("System.Boolean"));
            m_dtMenus.Columns.Add("EXTLLINK", System.Type.GetType("System.String"));
            m_dtMenus.Columns.Add("ISPUBLIC", System.Type.GetType("System.Boolean"));
            m_dtMenus.Columns.Add("ISENABLED", System.Type.GetType("System.Boolean"));
            m_dtMenus.Columns.Add("ISENABLEDSpecified", System.Type.GetType("System.Boolean"));
            m_dtMenus.Columns.Add("POSITION", System.Type.GetType("System.Int32"));
            #endregion
            //
            string sqlQuery = GetQueryMenu();
            DataSet dsMenu = DataHelper.ExecuteDataset(m_CS, CommandType.Text, sqlQuery);
            //
            #region Init variables
            DataTable dtMenu = dsMenu.Tables[0];
            DataRow[] rowXMenu;
            DataRow[] rowMenuOf;
            bool quit = false;
            // CC 20100908 MENU.ISCHILDONLY and MENUOF.ISCHILDONLY replaced by column VISIBILITY cf Ticket 17143
            //bool IsChildOnly = false;
            _ = Cst.Visibility.SHOW.ToString();
            int Id = 0;
            int Guard = 0;
            //PL 20110311 Add MaxGuard (Sur une idée de MF on admet qu'un menu pourrait être rattaché à 4 menus distincts)
            int MaxGuard = Math.Max(999, dtMenu.Rows.Count * 4);
            int Level = 1;
            int LastPosition = -1;
            string LastDisplayName = string.Empty;
            string IdMenu_Parent = m_MenuRoot;
            bool isCollaboratorSysAdmin = ActorTools.IsUserType_SysAdmin(m_Role);
            #endregion
            //
            #region Resource: Traduction du nom de chaque menu
            foreach (DataRow row in dtMenu.Select())
            {
                row["DISPLAYNAME"] = Ressource.GetMenu_Shortname(Convert.ToString(row["IDMENU"]), Convert.ToString(row["DISPLAYNAME"]));
            }
            #endregion
            //
            #region While:
            while (!quit)
            {
                Guard++;
                //Debug.WriteLine("Find    : " + IdMenu_Parent + " - " + DisplayName + "         (IdMenu:" + IdMenu + ")"); 

                #region Recherche dans MENUOF, d'un enfant
                //-------------------------------------------------------------------------------------------------
                //Recherche dans MENUOF, de l'enfant "suivant" mais de même "position" que le dernier enfant trouvé 
                //-------------------------------------------------------------------------------------------------
                rowMenuOf = dtMenu.Select("IDMENU_MENU=" + DataHelper.SQLString(IdMenu_Parent)
                                        + " And DISPLAYNAME > " + DataHelper.SQLString(LastDisplayName)
                                        + " And POSITION=" + LastPosition.ToString()
                                        , "POSITION, DISPLAYNAME");

                if (rowMenuOf.Length == 0)
                {
                    //----------------------------------------------------------------------------------------------
                    //Recherche dans MENUOF, de l'enfant "suivant" de "position" supérieure au dernier enfant trouvé 
                    //----------------------------------------------------------------------------------------------
                    rowMenuOf = dtMenu.Select("IDMENU_MENU=" + DataHelper.SQLString(IdMenu_Parent)
                                        + " And POSITION > " + LastPosition.ToString()
                                        , "POSITION, DISPLAYNAME");
                }
                bool IsOk = (rowMenuOf.Length > 0);
                #endregion

                //FI 20190606 [XXXXX] Ajout d'un garde-fou pour ne pas dupliquer un couple {Menu, Menu_Parent} 
                IsOk = IsOk && ArrFunc.IsEmpty(m_dtMenus.Select( 
                     StrFunc.AppendFormat("IDMENU={0} and IDMENU_MENU={1}",
                     DataHelper.SQLString(rowMenuOf[0]["IDMENU"].ToString()),  DataHelper.SQLString(rowMenuOf[0]["IDMENU_MENU"].ToString()) ))) ;

                 if (IsOk)
                 {
                     Id++;
                    string IdMenu = Convert.ToString(rowMenuOf[0]["IDMENU"]);
                    IdMenu_Parent = Convert.ToString(rowMenuOf[0]["IDMENU_MENU"]);
                    string Icon = Convert.ToString(rowMenuOf[0]["ICON"]);
                    string DisplayName = Convert.ToString(rowMenuOf[0]["DISPLAYNAME"]);
                    string Description = Convert.ToString(rowMenuOf[0]["DESCRIPTION"]);
                    string Url = Convert.ToString(rowMenuOf[0]["URL"]);
                    bool IsAction = (Url.Length != 0 && Url != "-");
                    bool IsSeparator = (Url == "-");
                    bool IsPublic = (isCollaboratorSysAdmin || Convert.ToBoolean(rowMenuOf[0]["ISENABLED"]));
                    bool IsEnabled = isCollaboratorSysAdmin;
                    bool IsEnabledSpecified = isCollaboratorSysAdmin;
                    bool IsMovable = Convert.ToBoolean(rowMenuOf[0]["ISMOVABLE"]);
                    // CC 20100908 MENU.ISCHILDONLY and MENUOF.ISCHILDONLY replaced by column VISIBILITY cf Ticket 17143
                    //IsChildOnly = Convert.ToBoolean(rowMenuOf[0]["ISCHILDONLY"]);
                    string Visibility = Convert.ToString(rowMenuOf[0]["VISIBILITY"]);
                    bool IsExternal = Convert.ToBoolean(rowMenuOf[0]["ISEXTERNAL"]);
                    string IsExtlLink = Convert.ToString(rowMenuOf[0]["EXTLLINK"]);
                    int Position = Convert.ToInt32(rowMenuOf[0]["POSITION"]);
                    //Debug.WriteLine("Found(Id:" + Id.ToString() + "): " + IdMenu_Parent + " - " + DisplayName + "         (IdMenu:" + IdMenu + ")");
                    m_dtMenus.Rows.Add(new object[] { Id, IdMenu, IdMenu_Parent, Level, Icon, DisplayName, Description, Url, IsAction, IsSeparator, 
                                            // CC 20100908 MENU.ISCHILDONLY and MENUOF.ISCHILDONLY replaced by column VISIBILITY cf Ticket 17143
                                            //IsMovable, IsChildOnly, IsExternal, IsExtlLink, IsPublic, IsEnabled, IsEnabledSpecified, Position });
                                            IsMovable, Visibility, IsExternal, IsExtlLink, IsPublic, IsEnabled, IsEnabledSpecified, Position });
                     //Debug.WriteLine("OK - Menu added - (IdMenu:" + IdMenu + ")(IdMenu_Parent:" + IdMenu_Parent + ")"); 

                     //------------------------------------------------------------------
                     //Init des variables pour recherche des menus enfants du menu traité
                     //------------------------------------------------------------------
                     Level++;
                     LastDisplayName = string.Empty;
                     LastPosition = -1;
                     IdMenu_Parent = IdMenu;
                 }
                 else
                 {
                     //Debug.WriteLine( "ERR");                         
                     #region Recherche du parent, du dernier enfant traité, afin de rechercher ensuite ses enfants "suivants" (via position et/ou displayname)
                     rowXMenu = m_dtMenus.Select("IDMENU=" + DataHelper.SQLString(IdMenu_Parent), "ID Desc");
                     if (rowXMenu.Length > 0)
                     {
                         LastDisplayName = rowXMenu[0]["DISPLAYNAME"].ToString();
                         LastPosition = Convert.ToInt32(rowXMenu[0]["POSITION"]);
                         IdMenu_Parent = Convert.ToString(rowXMenu[0]["IDMENU_MENU"]);
                         Level--;
                     }
                     else
                     {
                         //Debug.WriteLine("END - *****************************************"); 
                         quit = true;
                     }
                     #endregion
                 }
                if (Guard == MaxGuard)
                {
                    throw new Exception("More than " + MaxGuard + " iterations, please contact EFS");
                }
            }
            #endregion while

        }
        #endregion

        #region private GetQueryMenu
        private string GetQueryMenu()
        {
            string tmpQryRO = string.Empty;
            //PL 20120221 Add test on isUserReadOnly()
            //PL 20190304 Add test on IsDataArchive() and exclude: RunGateBCS.aspx and ProcessType=ProcessCSharp and IDMENU like '%PROCESS%'
            if (CSTools.IsUserReadOnly(m_CS) || SessionTools.IsDataArchive)
            {
                tmpQryRO = SQLCst.AND + @"(((m.URL is null) and ((not m.IDMENU like '%PROCESS%'))) or 
                    ((not m.URL like '%ProcessBase=%') 
                     and (not m.URL like '%RunIO%.aspx%') and (not m.URL like '%RunGate%.aspx%') 
                     and (not m.URL like '%ProcessType=ProcessCSharp%')))" + Cst.CrLf;
            }

            string tmpQry = Cst.OTCml_TBL.MENU + " m" + Cst.CrLf;
            tmpQry += SQLCst.WHERE + "(IDMENU!=" + DataHelper.SQLString(m_MenuRoot) + ")" + Cst.CrLf;
            tmpQry += SQLCst.AND + SQLCst.NOT_EXISTS_SELECT + Cst.OTCml_TBL.MENUOF + Cst.CrLf + SQLCst.WHERE + "IDMENU=m.IDMENU)" + Cst.CrLf;
            tmpQry += tmpQryRO;

            //Warning: SQL ANSI 
            string sqlQuery = SQLCst.SQL_ANSI + Cst.CrLf;
            //-------------------------------------------------------------------------
            //Query for retrieve all menus linked with table MENUOF
            //-------------------------------------------------------------------------
            //20050330 PL ne pas mettre ce commentaire: Bug sous Oracle quand une query débute par un commentaire
            //sqlQuery += DataHelper.SQLComment( "Query for retrieve all menus linked with table MENUOF")+ Cst.CrLf;
            sqlQuery += SQLCst.SELECT + "m.IDMENU, mo.IDMENU_MENU, ";
            sqlQuery += "m.ICON, m.DISPLAYNAME, m.DESCRIPTION, ";
            sqlQuery += DataHelper.SQLIsNullChar(m_CS, "m.URL", string.Empty, "URL") + ", ";
            //sqlQuery += "m.ISENABLED, m.ISMOVABLE, m.VISIBILITY, m.ISEXTERNAL, m.EXTLLINK, ";
            // CC 20100908 MENU.ISCHILDONLY and MENUOF.ISCHILDONLY replaced by column VISIBILITY cf Ticket 17143
            //sqlQuery += "m.ISENABLED, m.ISMOVABLE, case when m.ISCHILDONLY=0 then mo.ISCHILDONLY else 1 end as ISCHILDONLY, m.ISEXTERNAL, m.EXTLLINK, ";
            //sqlQuery += "m.ISENABLED, m.ISMOVABLE, case when m.VISIBILITY='SHOW' then mo.VISIBILITY else 'HIDE' end as VISIBILITY, m.ISEXTERNAL, m.EXTLLINK, ";
            sqlQuery += "m.ISENABLED, m.ISMOVABLE, m.VISIBILITY, m.ISEXTERNAL, m.EXTLLINK, ";
            //sqlQuery += DataHelper.SQLIsNull(m_CS, "mo.POSITION", "0", "POSITION") + Cst.CrLf;
            sqlQuery += "case when (mo.POSITION is null and m.URL is null) then -1 when (mo.POSITION is null and m.URL is not null) then 0 else mo.POSITION end as POSITION" + Cst.CrLf;
            //sqlQuery += ", case when m.URL is null then 0 else 1 end as ISACTION" + Cst.CrLf; 
            sqlQuery += ", case when nullif(m.URL,'-') is null then 0 else 1 end as ISACTION" + Cst.CrLf;
            sqlQuery += ", case when m.URL = '-' then 1 else 0 end as ISSEPARATOR" + Cst.CrLf; 
            sqlQuery += SQLCst.FROM_DBO + Cst.OTCml_TBL.MENU + " m," + SQLCst.DBO + Cst.OTCml_TBL.MENUOF + " mo" + Cst.CrLf;
            sqlQuery += SQLCst.WHERE + "(mo.IDMENU=m.IDMENU)";
            sqlQuery += SQLCst.AND + "(mo.IDMENU!=mo.IDMENU_MENU)" + Cst.CrLf;//pour éviter une boucle infinie plus bas dans le code (20050117 PL)
            sqlQuery += tmpQryRO;

            sqlQuery += SQLCst.UNIONALL + Cst.CrLf;

            //-------------------------------------------------------------------------
            //Query for create a fictive menu for not linked menus with table MENUOF
            //-------------------------------------------------------------------------
            sqlQuery += DataHelper.SQLComment("Query for retrieve a fictive menu for not linked menus with table MENUOF") + Cst.CrLf;
            sqlQuery += SQLCst.SELECT_DISTINCT + "' ' as IDMENU," + DataHelper.SQLString(m_MenuRoot) + "," + Cst.CrLf;
            if (Software.IsSoftwarePortal())
                sqlQuery += "'gif/Led/LedInternal.mnu.gif'" + Cst.CrLf;
            else
                sqlQuery += "'gif/Spheres-Black.gif'" + Cst.CrLf;
            sqlQuery += " as ICON, 'Miscellaneous' as DISPLAYNAME, 'Miscellaneous' as DESCRIPTION, null as URL," + Cst.CrLf;
            // CC 20100908 MENU.ISCHILDONLY and MENUOF.ISCHILDONLY replaced by column VISIBILITY cf Ticket 17143
            //sqlQuery += "1 as ISENABLED, 0 as ISMOVABLE, 0 as ISCHILDONLY, 1 as ISEXTERNAL, null as EXTLLINK" + Cst.CrLf;
            sqlQuery += "1 as ISENABLED, 0 as ISMOVABLE, 'SHOW' as VISIBILITY, 1 as ISEXTERNAL, null as EXTLLINK" + Cst.CrLf;
            sqlQuery += ", 99999 as POSITION" + Cst.CrLf;
            sqlQuery += ", 0 as ISACTION" + Cst.CrLf;
            sqlQuery += ", 0 as ISSEPARATOR" + Cst.CrLf;
            sqlQuery += SQLCst.FROM_DBO + Cst.OTCml_TBL.MENU + " m" + Cst.CrLf;
            sqlQuery += SQLCst.WHERE + SQLCst.EXISTS_SELECT + tmpQry + ")" + Cst.CrLf;

            sqlQuery += SQLCst.UNIONALL + Cst.CrLf;

            //-------------------------------------------------------------------------
            //Query for retrieve all menus not linked with table MENUOF
            //-------------------------------------------------------------------------
            //20050520 PL Mis en commentaire du code suivant
            //            if (Software.IsSoftwareEurosysFeed())
            //				sqlQuery += SQLCst.SELECT_DISTINCT + "' ' as IDMENU, 'FEED'," + Cst.CrLf;
            //            else if (Software.IsSoftwareEurosysWeb())
            //                sqlQuery += SQLCst.SELECT_DISTINCT + "' ' as IDMENU, 'WEB'," + Cst.CrLf;
            //            else
            sqlQuery += SQLCst.SELECT + "m.IDMENU, ' ' as IDMENU_MENU," + Cst.CrLf;
            sqlQuery += "m.ICON, m.DISPLAYNAME, m.DESCRIPTION, m.URL," + Cst.CrLf;
            // CC 20100908 MENU.ISCHILDONLY and MENUOF.ISCHILDONLY replaced by column VISIBILITY cf Ticket 17143
            //sqlQuery += "m.ISENABLED, m.ISMOVABLE, m.ISCHILDONLY, m.ISEXTERNAL, m.EXTLLINK" + Cst.CrLf;
            sqlQuery += "m.ISENABLED, m.ISMOVABLE, m.VISIBILITY, m.ISEXTERNAL, m.EXTLLINK" + Cst.CrLf;
            sqlQuery += ", 0 as POSITION" + Cst.CrLf;
            //sqlQuery += ", case when m.URL is null then 0 else 1 end as ISACTION" + Cst.CrLf;
            sqlQuery += ", case when nullif(m.URL,'-') is null then 0 else 1 end as ISACTION" + Cst.CrLf;
            sqlQuery += ", case when m.URL = '-' then 1 else 0 end as ISSEPARATOR" + Cst.CrLf; 
            sqlQuery += SQLCst.FROM_DBO + tmpQry;

            return sqlQuery;
        }
        #endregion
    }
    #endregion MenuChecking
}