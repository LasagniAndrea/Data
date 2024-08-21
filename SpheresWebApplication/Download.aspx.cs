using EFS.ACommon;
using EFS.Common;
using EFS.Common.Web;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using SpheresMenu = EFS.Common.Web.Menu;

namespace EFS.Spheres.Download
{
    // EG 20200914 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc) Correction et compléments
    public partial class DownloadPage : PageBase
    {
        #region Declaration
        private Table _tblDwnldFile;
        private string _sortCol;
        private string _sortOrder;
        private string _mainMenuClassName;
        private string _idMenu;
        #endregion
        /// <summary>
        /// Constitution de l'URL de chargement d'une RN (HTML/PDF)
        /// </summary>
        /// EG 20230809 [XXXXX] Gestion des RN (nouvelle version HTML)
        private string RNFilePath
        {
            get
            {
                string requestedPath = Request.QueryString["P"].ToString();
                string subPath = string.Empty;
                if (!String.IsNullOrEmpty(Request.QueryString["D"]))
                    subPath = @"\" + Request.QueryString["D"].ToString().ToUpper();
                return requestedPath + subPath + @"\";
            }
        }
        /// <summary>
        /// Appel de le menu Spheres RN
        /// </summary>
        /// EG 20230809 [XXXXX] Gestion des RN (nouvelle version HTML)
        private bool IsRN
        {
            get {return (_idMenu == "OTC_ABOUT_RN");}
        }
        #region protected override OnInit
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            _idMenu = Request.QueryString["IdMenu"];
            _mainMenuClassName = ControlsTools.MainMenuName(_idMenu);
        }
        #endregion
        #region protected override PageConstruction
        // EG 20200902 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc) Corrections et compléments
        // EG 20200914 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc) Correction et compléments
        protected override void PageConstruction()
        {
            string _titleLeft = GetTitleFromMenu();

            HtmlPageTitle titleLeft = new HtmlPageTitle(_titleLeft);
            HtmlPageTitle titleRight = new HtmlPageTitle();
            PageTitle = _titleLeft;

            GenerateHtmlForm();

            FormTools.AddBanniere(this, Form, titleLeft, titleRight, string.Empty, _idMenu);
            PageTools.BuildPage(this, Form, PageFullTitle, string.Empty, false, string.Empty, _idMenu);

        }
        #endregion PageConstruction
        #region protected override GenerateHtmlForm
        // EG 20200914 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc) Correction et compléments
        protected override void GenerateHtmlForm()
        {
            base.GenerateHtmlForm();
            AddToolBar();
            AddBody();
        }
        #endregion
        // EG 20200914 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc) Correction et compléments
        protected void AddToolBar()
        {
            Panel pnlToolBar = new Panel() { ID = "divalltoolbar", CssClass = CSSMode + " " + _mainMenuClassName };
            WCToolTipLinkButton btnRefresh = ControlsTools.GetToolTipLinkButtonRefresh();
            btnRefresh.Attributes.Add("onclick", "__doPostBack('0','SELFRELOAD_');");
            pnlToolBar.Controls.Add(btnRefresh);
            CellForm.Controls.Add(pnlToolBar);
        }

        // EG 20200819 [XXXXX] Nouvelle interface GUI v10(Mode Noir ou blanc) 
        // EG 20200914 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc) Correction et compléments
        protected void AddBody()
        {
            Panel pnlBody = new Panel() { ID = "divbody", CssClass = CSSMode + " " + _mainMenuClassName };
            WCTogglePanel togglePanel = new WCTogglePanel() { CssClass = CSSMode + " " + _mainMenuClassName };
            togglePanel.AddContentHeader(CreateHeaderTitle());

            Panel pnlDivDtg = new Panel() { ID = "divDtg", CssClass = CSSMode + " " + _mainMenuClassName };

            Table table = new Table()
            {
                Width = Unit.Percentage(100),
                CellSpacing = 0,
                CellPadding = 3,
                ID = "tblDwnldFile",
                CssClass = "DataGrid",
                GridLines = GridLines.Vertical
            };
            pnlDivDtg.Controls.Add(table);
            togglePanel.AddContent(pnlDivDtg);

            pnlBody.Controls.Add(togglePanel);
            CellForm.Controls.Add(pnlBody);
        }

        #region private LoadData
        /// <summary>
        /// Retourne un DataTable alimenté avec le contenu du folder passé en paramètre.
        /// </summary>
        /// <param name="pFullPath"></param>
        /// <returns></returns>
        /// EG 20230809 [XXXXX] Upd : Gestion des RN (nouvelle version HTML)
        private DataTable LoadData(string pFullPath, string pRequestedPath)
        {
            // Create sample data for the DataList control.
            DataTable dt = new DataTable();
            DataRow dr;

            // Define the columns of the table.
            dt.Columns.Add(new DataColumn("Name", typeof(String)));
            dt.Columns.Add(new DataColumn("Length", typeof(Int32)));
            dt.Columns.Add(new DataColumn("CreationTime", typeof(DateTime)));
            dt.Columns.Add(new DataColumn("Extension", typeof(String)));

            if (Directory.Exists(pFullPath))
            {
                DirectoryInfo diDirectory = new DirectoryInfo(pFullPath);

                // Charger la liste des fichiers
                // Filtre si RN sur les fichiers exclusivement PDF ou HTML
                List<FileInfo> lstFileInfos = new List<FileInfo>();
                if (IsRN)
                    lstFileInfos.AddRange(diDirectory.GetFiles()
                        .Where(f => f.Extension.EndsWith(".pdf") || f.Extension.EndsWith(".html")));
                else
                    lstFileInfos.AddRange(diDirectory.GetFiles().ToList());

                lstFileInfos.ForEach(fi =>
                {
                    dr = dt.NewRow();

                    dr[0] = fi.Name;
                    dr[1] = fi.Length;
                    dr[2] = fi.LastWriteTime;
                    dr[3] = fi.Extension;

                    if (fi.Extension == ".lnk")
                    {
                        if (pRequestedPath == "PRIVATE")
                        {
                            //Warning: Les Shortcut (.lnk) sont gérés uniquement sur les environnements PRIVATE.
                            //         Ils ne doivent de plus avoir pour cible que des fichiers situés physiquement sous le folder "(COMMON)".
                            string targetPath = FileTools.GetShortcutTargetFile(fi.FullName);
                            FileInfo targetFile = new FileInfo(targetPath);

                            dr[0] = targetFile.Name.ToString();
                            dr[1] = targetFile.Length;
                            dr[2] = targetFile.LastWriteTime;
                            dr[3] = targetFile.Extension + ".lnk"; //Tip: ajout de cette extension pour identification ultérieure et gestion spécifique de ces items.
                            dt.Rows.Add(dr);
                        }
                    }
                    else if (fi.Name.ToLower().StartsWith("(shared)"))
                    {
                        if (pRequestedPath == "PRIVATE")
                        {
                            //Warning: Les Partages "(shared)" sont gérés uniquement sur les environnements PRIVATE.
                            //         Ils ne peuvent de plus avoir pour cible que des fichiers situés physiquement sous le folder "(COMMON)".
                            dr[3] += ".lnk"; //Tip: ajout de cette extension pour identification ultérieure et gestion spécifique de ces items.
                            dt.Rows.Add(dr);
                        }
                    }
                    else
                    {
                        dt.Rows.Add(dr);
                    }
                });


                // Charger la liste des répertoires
                // Si RN pas d'affichage du dossier "IMAGES" (images attachées à la RN HTML)
                List<DirectoryInfo> lstDirectories = new List<DirectoryInfo>();
                if (IsRN)
                    lstDirectories.AddRange(diDirectory.GetDirectories().Where(f => f.Name.ToLower() != "images"));
                else
                    lstDirectories.AddRange(diDirectory.GetDirectories().ToList());

                lstDirectories.ForEach(di =>
                {
                    dr = dt.NewRow();

                    dr[0] = di.Name.ToString();
                    dr[1] = -1;
                    dr[2] = di.LastWriteTime;

                    dt.Rows.Add(dr);
                });
            }
            return dt;
        }
        #endregion
        #region private DisplayData
        // EG 20200914 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc) Correction et compléments
        // EG 20230809 [XXXXX] Upd : Gestion des RN (nouvelle version HTML)
        private void DisplayData(DataView pDataview, string pFileUrl, string pParentUrl)
        {
            TableRow row;
            TableCell cell;
            TableHeaderCell headerCell;
            HyperLink hLinkFile;
            Unit SizeWidth = Unit.Point(90);
            Unit ModifWidth = Unit.Point(150);

            string navigateUrl;
            string awImageUrl;

            string pageUrl = Request.Url.ToString();
            int iIndexC = pageUrl.IndexOf("&C=", 1);
            int iIndexO = pageUrl.IndexOf("&O=", 1);

            #region Création de la ligne de titre (Nom, Taille, Date)
            row = new TableRow() { CssClass = "DataGrid_HeaderStyle" };
            _tblDwnldFile.Rows.Add(row);

            #region Cell Name
            headerCell = new TableHeaderCell(); 
            
            if (iIndexC > 0)
            {
                pageUrl = GetPageUrl(pageUrl, (_sortCol == "Name") && (_sortOrder == "ASC"), "N", iIndexC, iIndexO);
                navigateUrl = pageUrl;
            }
            else
            {
                navigateUrl = pageUrl + "&C=N" + "&O=A";
            }

            hLinkFile = new HyperLink() 
            {
                Text = " " + Ressource.GetString("Nom", "Name").ToString() + " ",
                NavigateUrl = navigateUrl
            };
            
            headerCell.Controls.Add(hLinkFile);
            row.Cells.Add(headerCell);
            #endregion
            #region Cell Size
            headerCell = new TableHeaderCell
            {
                Width = SizeWidth
            };

            if (pageUrl.IndexOf("&C=", 1) > 0)
            {
                pageUrl = GetPageUrl(pageUrl, (_sortCol == "Length") && (_sortOrder == "ASC"), "L", iIndexC, iIndexO);
                navigateUrl = pageUrl;
            }
            else
            {
                navigateUrl = pageUrl + "&C=L" + "&O=A";
            }

            hLinkFile = new HyperLink() 
            {
                Text = " " + Ressource.GetString("Taille", "Size").ToString() + " ",
                NavigateUrl = navigateUrl
            };

            headerCell.Controls.Add(hLinkFile);
            row.Cells.Add(headerCell);
            #endregion
            #region Cell Last Modif
            headerCell = new TableHeaderCell
            {
                Width = ModifWidth
            };

            if (pageUrl.IndexOf("&C=", 1) > 0)
            {
                pageUrl = GetPageUrl(pageUrl, (_sortCol == "CreationTime") && (_sortOrder == "ASC"), "T", iIndexC, iIndexO);
                navigateUrl = pageUrl;
            }
            else
            {
                navigateUrl = pageUrl + "&C=T" + "&O=A";
            }

            hLinkFile = new HyperLink()
            {
                Text = " " + Ressource.GetString("Dernière Modification", "Last Modification").ToString() + " ",
                NavigateUrl = navigateUrl
            };
            headerCell.Controls.Add(hLinkFile);
            row.Cells.Add(headerCell);
            #endregion

            #endregion Création de la ligne de titre (Nom, Taille, Date)

            #region Création des lignes de données
            
            foreach (DataRowView dr in pDataview)
            {
                Cst.HyperLinkTargetEnum target = Cst.HyperLinkTargetEnum._top;
                bool isFolder = ((Int32)dr[1] == -1);
                bool isShortcut = dr[3].ToString().ToLower().EndsWith(".lnk");
                bool isHtml = dr[3].ToString().ToLower().EndsWith(".html");
                if (isFolder)
                {
                    navigateUrl = pParentUrl + @"\" + dr[0].ToString();
                    target = Cst.HyperLinkTargetEnum._self;
                }
                else
                {
                    if (isShortcut)
                    {
                        string subFolder = string.Empty;
                        if (dr[0].ToString().StartsWith("(shared)"))
                        {
                            string fileName = dr[0].ToString().Substring(8); //Remove prefix "(shared)"
                            if (fileName.StartsWith("(") && fileName.IndexOf(")") > 0)
                            {
                                //ex. "(shared)(EUREX.PRISMA)00theoinstpubli20141013oiserieseodx0001_0001.zip"
                                //    --> fichier "00theoinstpubli20141013oiserieseodx0001_0001.zip" situé sous "(SHARED)\EUREX\PRISMA"
                                subFolder = (fileName.Substring(1, fileName.IndexOf(")") - 1)).Replace(@".", @"\") + @"\";
                                fileName = fileName.Substring(fileName.IndexOf(")") + 1);
                            }
                            dr[0] = fileName;
                        }
                        navigateUrl = pFileUrl.Replace("P=PRIVATE&", "P=SHARED&") + @"\" + subFolder + dr[0].ToString();
                    }
                    else if (IsRN)
                    {
                        // Pas de download mais ouverture directe sur le browser
                        navigateUrl = RNFilePath + dr[0].ToString();
                        target = Cst.HyperLinkTargetEnum._blank;
                    }
                    else
                    {
                        navigateUrl = pFileUrl + @"\" + dr[0].ToString();
                    }
                }
                    
                row = new TableRow() 
                { 
                    BorderStyle = (isFolder ? BorderStyle.Solid : BorderStyle.None),
                    BorderWidth = Unit.Point(1),
                    CssClass = "DataGrid_ItemStyle"
                };
                _tblDwnldFile.Rows.Add(row);

                #region Cell Name
                cell = new TableCell
                {
                    HorizontalAlign = HorizontalAlign.Left
                };

                #region Cell Icone
                #region ImageUrl
                if (isFolder)
                {
                    awImageUrl = "fas fa-folder-minus";
                }
                else
                {
                    string ext = dr[3].ToString().ToLower();
                    if (isShortcut)
                        ext = ext.Replace(".lnk", string.Empty);

                    switch (ext)
                    {
                        case ".xml":
                        case ".htm":
                        case ".html":
                            awImageUrl = "fas fa-file-code";
                            break;
                        case ".doc":
                        case ".docx":
                            awImageUrl = "fas fa-file-word";
                            break;
                        case ".xls":
                        case ".xlsx":
                            awImageUrl = "fas fa-file-excel";
                            break;
                        case ".pdf":
                            awImageUrl = "fas fa-file-pdf";
                            break;
                        case ".rar":
                        case ".zip":
                        case ".lzh":
                            awImageUrl = "fas fa-file-archive";
                            break;
                        case ".bmp":
                        case ".png":
                        case ".gif":
                        case ".jpg":
                            awImageUrl = "fas fa-file-image";
                            break;
                        case ".rtf":
                        default:
                            awImageUrl = "fas fa-file-alt";
                            break;
                    }
                }
                #endregion

                hLinkFile = new HyperLink()
                {
                    NavigateUrl = navigateUrl,
                    Text = String.Format("<i class='{0}'></i>", awImageUrl) + " " + dr[0].ToString(),
                    CssClass = "fa-icon" + (isFolder?" green":""),
                    Target = target.ToString()
                };
                cell.Controls.Add(hLinkFile);
                #endregion

                row.Cells.Add(cell);
                #endregion

                #region Cell Size
                string displaySize = string.Empty;
                if ((Int32)dr[1] != -1)
                {
                    string octet_byte = (SessionTools.Collaborator_Culture_ISOCHAR2 == "fr" ? "o" : "b");
                    int size = (Int32)dr[1];
                    if (size < (1024))
                        displaySize = "0 K";
                    else if (size < (1024 * 1024))
                        displaySize = (size / 1024).ToString("#,##") + " K";
                    else if (size < (1024 * 1024 * 1024))
                        displaySize = (size / (1024 * 1024)).ToString("#,##") + " M";
                    else
                        displaySize = (size / (1024 * 1024 * 1024)).ToString("#,##") + " G";
                    displaySize += octet_byte;
                }

                cell = new TableCell() 
                {
                    Text = displaySize,
                    Width = SizeWidth,
                    HorizontalAlign = HorizontalAlign.Right
                };
                row.Cells.Add(cell);
                #endregion

                #region Cell Last Modif
                cell = new TableCell
                {
                    Text = ((DateTime)dr[2]).ToString("g"),
                    HorizontalAlign = HorizontalAlign.Center,
                    Width = ModifWidth
                };
                row.Cells.Add(cell);
                #endregion
            }
            #endregion Création des lignes de données
        }
        #endregion

        // EG 20200914 [XXXXX] New
        private string GetPageUrl(string pPageUrl, bool pIsSortColAscendant, string pCol, int pIndexC, int pIndexO)
        {
            string pageUrl = pPageUrl.Substring(0, pIndexC) + String.Format("&C={0}", pCol) + pPageUrl.Substring(pIndexC + 4);
            return pageUrl.Substring(0, pIndexO) + String.Format("&O={0}", pIsSortColAscendant? "D":"A") + pageUrl.Substring(pIndexO + 4);
        }
        /// <summary>
        /// Contrôle si le path demandée est autorisée via le caractère wildcard "*"
        /// </summary>
        /// <param name="pDownloadAllowedPaths"></param>
        /// <param name="pRequestedFullPath"></param>
        /// <returns></returns>
        private bool IsWildcardAllowedPath(string pDownloadAllowedPaths, string pRequestedFullPath)
        {
            bool isAllowed = false;

            string[] allowedPaths = pDownloadAllowedPaths.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string allowedPath in allowedPaths)
            {
                if (allowedPath.EndsWith("*")
                    && (pRequestedFullPath.StartsWith(allowedPath.TrimEnd('*'), StringComparison.InvariantCultureIgnoreCase)))
                {
                    isAllowed = true;
                    break;
                }
            }

            return isAllowed;
        }

        #region private Page_Load
        // EG 20180423 Analyse du code Correction [CA2200]
        // EG 20200914 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc) Correction et compléments
        private void Page_Load(object sender, System.EventArgs e)
        {
            try
            {
                PageConstruction();

                #region Get Controls to update in this page
                _tblDwnldFile = (Table)this.FindControl("tblDwnldFile");
                Label lblCurrentPath = (Label)this.FindControl("lblCurrentPath");
                HyperLink hlNavigateParent = (HyperLink)this.FindControl("hlNavigateParent");
                #endregion
                
                #region Get Request Parameters
                InitializeSort();
                
                string requestedPath = Request.QueryString["P"].ToString();
                string subPath = string.Empty;
                if (!String.IsNullOrEmpty(Request.QueryString["D"]))
                    subPath = Request.QueryString["D"].ToString().ToUpper();
                #endregion

                #region Security (Newness from 2016-09-14)
                //PL 20160914 Nouveau: Contrôle du path 
                bool isForbidden = false;

                string requestedFullPath = requestedPath.ToUpper();
                char[] slashAntislash = { '/', '\\' };
                requestedFullPath = requestedFullPath.TrimEnd(slashAntislash);
                if (!String.IsNullOrEmpty(subPath))
                {
                    subPath = subPath.TrimStart(slashAntislash).TrimEnd(slashAntislash);
                    requestedFullPath += @"/" + subPath.ToUpper();
                }

                string downloadCmptLevel = (string)SystemSettings.GetAppSettings("DownloadCmptLevel", typeof(string), string.Empty);
                if ((!String.IsNullOrEmpty(downloadCmptLevel)) && (downloadCmptLevel == "5.0"))
                {
                    // Compatibilité ascendante: aucune restriction
                    isForbidden = false;
                }
                else
                {
                    string downloadAllowedPaths = (string)SystemSettings.GetAppSettings("DownloadAllowedPaths", typeof(string), string.Empty);
                    string downloadNotAllowedPaths = (string)SystemSettings.GetAppSettings("DownloadNotAllowedPaths", typeof(string), string.Empty);
                    #region
                    if (!String.IsNullOrEmpty(downloadAllowedPaths))
                    {
                        if (!downloadAllowedPaths.StartsWith(@";"))
                            downloadAllowedPaths = @";" + downloadAllowedPaths;
                        if (!downloadAllowedPaths.EndsWith(@";"))
                            downloadAllowedPaths += @";";
                    }
                    if (!String.IsNullOrEmpty(downloadNotAllowedPaths))
                    {
                        if (!downloadNotAllowedPaths.StartsWith(@";"))
                            downloadNotAllowedPaths = @";" + downloadNotAllowedPaths;
                        if (!downloadNotAllowedPaths.EndsWith(@";"))
                            downloadNotAllowedPaths += @";";
                    }
                    #endregion

                    if ((!String.IsNullOrEmpty(downloadNotAllowedPaths)) && (downloadNotAllowedPaths.ToUpper().IndexOf(@";" + requestedFullPath + @";") >= 0))
                    {
                        // Path non autorisé
                        isForbidden = true;
                    }
                    else if ((!String.IsNullOrEmpty(downloadAllowedPaths)) 
                            && (
                                 (downloadAllowedPaths.ToUpper().IndexOf(@";" + requestedFullPath + @";") >= 0)
                                 ||
                                 IsWildcardAllowedPath(downloadAllowedPaths, requestedFullPath) 
                                )
                             )
                    {
                        // Path autorisé
                        isForbidden = false;
                    }
                    else if (!requestedFullPath.StartsWith(@"~"))
                    {
                        //Seul les paths relatifs au root sont autorisés 
                        isForbidden = true;
                    }
                    else if ((requestedFullPath == @"~") || (requestedFullPath == @"~\."))
                    {
                        //Seul les paths enfants du root sont autorisés 
                        isForbidden = true;
                    }
                    else if ((requestedFullPath.IndexOf(@":") >= 0) || (requestedFullPath.IndexOf(@"..") >= 0))
                    {
                        //Sauf exception, unité logique (ex C:) et accès à un dossier parent ne sont pas autorisées
                        isForbidden = true;
                    }
                }
                if (isForbidden)
                {
                    throw new Exception("Forbitten: Acces is denied.");
                }
                #endregion 

                string physicalPath = DwnldTools.GetPath(this, requestedPath);
                DirectoryInfo diFinalPath = new DirectoryInfo(physicalPath + subPath);

                //Affichage du "Virtual requestedPath", préfixé par "Root\" et après effacement du "Physical requestedPath".
                lblCurrentPath.Text = @"Root\" + diFinalPath.FullName.Replace(physicalPath, string.Empty);

                if (String.IsNullOrEmpty(subPath))
                {
                    //On est ici sur le root --> interdiction de remonter dans l'arborescence physique.
                    hlNavigateParent.Visible = true;
                    hlNavigateParent.NavigateUrl = string.Empty;
                    lblCurrentPath.Text = "Root";
                }
                else
                {
                    //On est ici sur un folder enfant --> possibilité de remonter sur le folder parent.
                    hlNavigateParent.NavigateUrl = "Download.aspx?IdMenu=" + _idMenu + "&P=" + requestedPath;

                    string parentFinalPath = diFinalPath.Parent.FullName.ToUpper() + @"\";
                    parentFinalPath = parentFinalPath.Replace(physicalPath.ToUpper(), string.Empty);
                    if (!String.IsNullOrEmpty(parentFinalPath))
                        hlNavigateParent.NavigateUrl += "&D=" + parentFinalPath;
                }

                //Chargement du contenu du folder dans un DataView
                DataTable dt = LoadData(diFinalPath.FullName, requestedPath);
                DataView dv = new DataView(dt)
                {
                    Sort = _sortCol + " " + _sortOrder
                };

                //Affichage du DataView sur la page
                string fullPath = diFinalPath.FullName.Replace(physicalPath.ToUpper(), "");
                string fileUrl = "DownloadFile.aspx?P=" + requestedPath + "&F=" + fullPath;
                string parentUrl = "Download.aspx?IdMenu=" + _idMenu + "&P=" + requestedPath + "&D=" + fullPath;
                
                DisplayData(dv, fileUrl, parentUrl);
            }
            catch (Exception)
            {
                //PL 20160914 
                //Console.WriteLine("The process failed: {0}", exp.ToString());
                throw;
            }
        }
        #endregion
        #region private GetTitleFromMenu
        private string GetTitleFromMenu()
        {
            string sIdMenu = Request.QueryString["IdMenu"];
            string sTitle;
            if (StrFunc.IsEmpty(sIdMenu))
                sTitle = string.Empty;
            else
            {
                SpheresMenu.Menu menu = SessionTools.Menus.SelectByIDMenu(sIdMenu);
                sTitle = menu.Description;
            }

            sTitle = Ressource.GetString(sTitle, string.Empty, true);

            if (StrFunc.IsEmpty(sTitle))
                sTitle = Ressource.GetString(sIdMenu, sIdMenu, true);

            return sTitle;
        }
        #endregion
        #region private CreateHeaderTitle
        // EG 20200914 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc) Correction et compléments
        private Panel CreateHeaderTitle()
        {
            Panel pnlHeader = new Panel();
            pnlHeader.Style.Add(HtmlTextWriterStyle.Display, "flex");
            WCToolTipHyperlink hlLinkFolderImage = new WCToolTipHyperlink()
            {
                ID = "hlNavigateParent",
                CssClass = "fa-icon fas fa-folder-plus",
                Text = string.Empty
            };
            hlLinkFolderImage.Font.Size = FontUnit.Large;
            hlLinkFolderImage.Style.Add(HtmlTextWriterStyle.Padding, "0px 4px");
            hlLinkFolderImage.Pty.TooltipContent = Ressource.GetString("Parent folder", "Parent folder", true);
            pnlHeader.Controls.Add(hlLinkFolderImage);

            Label lblCurrentPath = new Label()
            {
                ID = "lblCurrentPath",
                CssClass = EFSCssClass.LabelDisplay,
                Width = Unit.Percentage(100)
            };
            lblCurrentPath.Font.Size = FontUnit.Point(11);
            pnlHeader.Controls.Add(lblCurrentPath);
            return pnlHeader;
        }
        #endregion
        #region private InitializeSort
        /// <summary>
        /// Initialisation des variables pilotant le tri des données pour l'affichage, via les éventuelles spécificatiosn présentes dasn l'URL (QueryString)
        /// </summary>
        private void InitializeSort()
        {
            _sortCol = "CreationTime";
            if (!String.IsNullOrEmpty(Request.QueryString["C"]))
            {
                switch (Request.QueryString["C"].ToString())
                {
                    case "N":
                        _sortCol = "Name";
                        break;
                    case "L":
                        _sortCol = "Length";
                        break;
                }
            }

            _sortOrder = "DESC";
            if (!String.IsNullOrEmpty(Request.QueryString["O"]))
            {
                if (Request.QueryString["O"].ToString() == "A")
                    _sortOrder = "ASC";
            }
        }
        #endregion
    }
}
