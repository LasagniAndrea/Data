using EFS.ACommon;
using EFS.ApplicationBlocks.Data;
using EFS.Common.Web;
using System;
using System.Drawing;
using System.IO;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;


namespace EFS.Spheres
{
    /// <summary>
    /// Summary description for FileUploadPage.
    /// </summary>
    /// EG 20220215 [26251][WI582] Upd : Vulnerability on unrestricted file upload : Enhancement
    public partial class FileUploadPage : PageBase
    {
        #region Protected
        
        #region Members
        protected string leftTitle, rightTitle;
        private string   qUploadTag;
        private string   qTableName;
        private string   qColumnName;
        private string   qColumnFileName;
        private string   qColumnFileType;
        private string   qColumnIDAUPD;
        private string   qColumnDTUPD;
        private string[] qKeyColumns;
        private string[] qKeyValues;
        private string[] qKeyDatatypes;
        protected HtmlForm dummpy;
        #endregion



        #endregion Protected

        // EG 20210325 [25556] Correction GUI - Add SetHead sur FileUpload
        protected void Page_Load(object sender, System.EventArgs e)
        {
            PageTools.SetHead(this, PageTitle, null, null);
        }

        #region Web Form Designer generated code
        // EG 20190125 DOCTYPE Conformity HTML5
        // EG 20200720 [25556] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        // EG 20200820 [25556] Nouvelle interface GUI v10 (Mode Noir ou blanc) 
        // EG 20210204 [25590] Nouvelle gestion des mimeTypes autorisés dans l'upload d'un fichier
        // EG 20210212 [25661] New Appel Protection CSRF(Cross-Site Request Forgery)
        // EG 20220215 [26251][WI582] Upd : Vulnerability on unrestricted file upload : Enhancement
        override protected void OnInit(EventArgs e)
        {
            //
            // CODEGEN: This call is required by the ASP.NET Web Form Designer.
            //
            InitializeComponent();
            base.OnInit(e);

            Form = fileUpload;
            AntiForgeryControl();

            this.AbortRessource  = true;   //nécessaire pour l'astuce du bouton "..."

            #region QueryString
            qUploadTag = Request.QueryString["UploadTag"] ?? string.Empty;
            qTableName = Request.QueryString["TableName"] ?? string.Empty;
            qColumnName = Request.QueryString["ColumnName"] ?? string.Empty;
            qColumnFileName = Request.QueryString["ColumnFileName"] ?? string.Empty;
            qColumnFileType = Request.QueryString["ColumnFileType"] ?? string.Empty;
            qColumnIDAUPD = Request.QueryString["ColumnIDAUPD"] ?? string.Empty;
            qColumnDTUPD = Request.QueryString["ColumnDTUPD"] ?? string.Empty;
            qKeyColumns = Request.QueryString["KeyColumns"] != null ? StrFunc.StringArrayList.StringListToStringArray(Request.QueryString["KeyColumns"]) : null;
            qKeyValues = Request.QueryString["KeyValues"] != null ? StrFunc.StringArrayList.StringListToStringArray(Request.QueryString["KeyValues"]) : null;
            qKeyDatatypes = Request.QueryString["KeyDatatypes"] != null ? StrFunc.StringArrayList.StringListToStringArray(Request.QueryString["KeyDatatypes"]) : null;            
            #endregion QueryString

            leftTitle  = Ressource.GetString("FileUpload", true);
            rightTitle = null;

            #region Header           
            HtmlPageTitle titleLeft  = new HtmlPageTitle(leftTitle);
            Panel pnlHeader = new Panel
            {
                ID = "divHeader"
            };
            pnlHeader.Controls.Add(ControlsTools.GetBannerPage(this, titleLeft, null, string.Empty,string.Empty));
            plhMain.Controls.Add(pnlHeader);
            #endregion Header

            AddToolBar();
            AddBody();
            //CreateControls();
        }
		
        protected override void OnPreRender(EventArgs e)
        {
            this.DataBind();
            base.OnPreRender(e);
        }

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {    

        }
        #endregion

        #region Création des controls

        // EG 20200820 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc) 
        protected void AddToolBar()
        {
            Panel pnlToolBar = new Panel() { ID = "divalltoolbar", CssClass = CSSMode + " repository"};
            WCToolTipLinkButton btnUpload = ControlsTools.GetAwesomeButtonAction("btnUpload", "fas fa-upload", true);
            if (SessionTools.IsUserWithLimitedRights())
                btnUpload.Enabled = false;
            else
                btnUpload.Click += new EventHandler(this.BtnUpload_Click);

            pnlToolBar.Controls.Add(btnUpload);
            plhMain.Controls.Add(pnlToolBar);
        }
        // EG 20200820 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc) 
        protected void AddBody()
        {
            Panel pnlBody = new Panel() { ID = "divbody", CssClass = CSSMode + " repository"};
            WCTogglePanel togglePanel = new WCTogglePanel() { CssClass = CSSMode + " repository" };
            togglePanel.SetHeaderTitle(Ressource.GetString("PasteChoice"));

            Panel pnlFileUpload = new Panel();

            WCTooltipLabel lbl = new WCTooltipLabel()
            {
                ID = "lblFilename",
                Text = Ressource.GetString("lblFilename")
            };
            pnlFileUpload.Controls.Add(lbl);

            FileUpload fileUpload = new FileUpload()
            {
                ID = "fuFileUpload",
                Width = Unit.Percentage(100),
                CssClass = EFSCssClass.Capture
            };
            pnlFileUpload.Controls.Add(fileUpload);

            Panel pnlUploadStatus = new Panel();
            WCTooltipLabel lblStatus = new WCTooltipLabel(){ID = "lblStatus" };
            pnlUploadStatus.Controls.Add(lblStatus);


            togglePanel.AddContent(pnlFileUpload);
            togglePanel.AddContent(pnlUploadStatus);

            pnlBody.Controls.Add(togglePanel);
            plhMain.Controls.Add(pnlBody);
        }
        #endregion

        #region Event butRecord_Click()
        /// EG 20220215 [26251][WI582] Upd : Vulnerability on unrestricted file upload : Enhancement
        public void BtnUpload_Click(object sender, System.EventArgs e)
        {
            string statusMessage = string.Empty;
            FileUpload fileUpload = FindControl("fuFileUpload") as FileUpload;
            try
            {
                if (fileUpload.HasFile)
                {
                    string saveDirectory = Page.Request.PhysicalApplicationPath + GetUploadPartialPath(qUploadTag);
                    // Get the name of the file to upload.
                    String fileName = fileUpload.FileName;

                    // Append the name of the file to upload to the path.
                    string savePath = saveDirectory + @"\" + fileName;

                    string displayMessage = string.Empty;

                    // Initialisation des données du fichier à uploader avant vérification
                    UploadFileInfo uploadFileInfo = new UploadFileInfo()
                    {
                        Stream = fileUpload.PostedFile.InputStream,
                        Name = fileUpload.FileName,
                        MimeType = fileUpload.PostedFile.ContentType,
                        ContentLength = fileUpload.PostedFile.ContentLength,
                        Logo = (qColumnName == "LOLOGO"),
                    };
                    // Initialisation des paramètres (webCustom.config) pour vérification
                    uploadFileInfo.SetParameters();

                    // Appel à la vérification
                    FileTypeVerifyResult result = MimeMappingTools.FileTypeVerify(uploadFileInfo);
                    if (result.IsVerified)
                    {
                        Cst.ErrLevel errLevel = Cst.ErrLevel.UNDEFINED;
                        if (qTableName != string.Empty && qColumnName != string.Empty)
                        {
                            fileUpload.PostedFile.InputStream.Position = 0;
                            BinaryReader brFile = new BinaryReader(fileUpload.PostedFile.InputStream);
                            byte[] data = brFile.ReadBytes((int)fileUpload.PostedFile.InputStream.Length);

                            LOFile loFile = new LOFile(fileName, fileUpload.PostedFile.ContentType.ToString(), data);

                            LOFileColumn dbfc = new LOFileColumn(SessionTools.CS, qTableName, qColumnName, qColumnFileName,
                                qColumnFileType, qKeyColumns, qKeyValues, qKeyDatatypes, qColumnIDAUPD, qColumnDTUPD);

                            errLevel = dbfc.SaveFile(loFile, SessionTools.Collaborator_IDA, out Exception opException);

                            if (errLevel == Cst.ErrLevel.SUCCESS)
                                statusMessage = Ressource.GetString("lblFileUploadedSuccess");
                            else
                                throw (opException);
                        }
                        else if (Directory.Exists(saveDirectory))
                        {
                            fileUpload.SaveAs(savePath);
                            statusMessage = Ressource.GetString("lblFileUploadedSuccess");
                        }
                    }
                    else
                    {
                        throw new Exception(result.ErrMessage);
                    }
                    fileUpload.Dispose();
                }
                else
                {
                    // Notify the user that a file was not uploaded.
                    throw new Exception(Ressource.GetString("lblFileUploadedNotSpecified"));
                }
            }
            catch (Exception ex)
            {
                statusMessage = ex.Message;
            }
            finally
            {
                if (Page.FindControl("lblStatus") is Label lbl)
                {
                    lbl.ForeColor = Color.Red;
                    lbl.Text = statusMessage;
                }

                if (statusMessage == Ressource.GetString("lblFileUploadedSuccess"))
                {
                    // 20100706 MF - call to window.opener.location.reload() removed in order to avoid the confirmation window pop-up.
                    JavaScript.ExecuteImmediate(this, "window.opener.location.replace(window.opener.location.href);", true);
                }
            }  
        }		
        #endregion       

        private string GetUploadPartialPath( string pUploadTag )
        {
            string retValue = Cst.UploadPath; 
            retValue +=  pUploadTag;
            
            return retValue;
        }
       
    
      
    }
}
