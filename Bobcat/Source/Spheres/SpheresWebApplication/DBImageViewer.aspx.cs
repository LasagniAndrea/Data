using System;
using System.Drawing;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
//
using EFS.ACommon;
using EFS.ApplicationBlocks.Data;
using EFS.Common;
using EFS.Common.Web;
using EFS.Referential;

namespace EFS.Spheres
{
    /// <summary>
    /// Summary description for DBImageViewerPage.
    /// </summary>
    /// EG 20220215 [26251][WI582] Upd : Vulnerability on unrestricted file upload : Enhancement
    public partial class DBImageViewerPage : PageBase
	{
		#region Constants
		private const int ThumbnailMaxWidth  = 600;        
		private const int ThumbnailMaxHeight = 275;             
		#endregion Constants
        
		#region Members
		protected string leftTitle, rightTitle;
		private string   qTableName;
		private string   qColumnName;
		private string   qColumnFileName;
		private string   qColumnFileType;
		private string   qColumnIDAUPD;
		private string   qColumnDTUPD;
		private string[] qKeyColumns;
		private string[] qKeyValues;
		private string[] qKeyDatatypes;
		private string   qDisplayName;
		protected        HtmlForm dummpy;

		#endregion Variables

		#region protected override OnInit
		// EG 20190125 DOCTYPE Conformity HTML5
		// EG 20200720 [25556] Nouvelle interface GUI v10 (Mode Noir ou blanc)
		// EG 20200820 [25556] Nouvelle interface GUI v10 (Mode Noir ou blanc) 
		// EG 20210204 [25590] Nouvelle gestion des mimeTypes autorisés dans l'upload d'un fichier
		// EG 20210212 [25661] New Appel Protection CSRF(Cross-Site Request Forgery)
		// EG 20220215 [26251][WI582] Upd : Vulnerability on unrestricted file upload : Enhancement
		protected override void OnInit(EventArgs e)
		{
            InitializeComponent();
            base.OnInit(e);

			Form = frmImageViewer;
			AntiForgeryControl();

			this.AbortRessource = true;

			#region QueryString
			_ = Request.QueryString["UploadTag"] ?? string.Empty;
			qTableName = Request.QueryString["TableName"] ?? string.Empty;
            qColumnName = Request.QueryString["ColumnName"] ?? string.Empty;
            qColumnFileName = Request.QueryString["ColumnFileName"] ?? string.Empty;
            qColumnFileType = Request.QueryString["ColumnFileType"] ?? string.Empty;
            qColumnIDAUPD = Request.QueryString["ColumnIDAUPD"] ?? string.Empty;
            qColumnDTUPD = Request.QueryString["ColumnDTUPD"] ?? string.Empty;
            qKeyColumns = Request.QueryString["KeyColumns"] != null ? StrFunc.StringArrayList.StringListToStringArray(Request.QueryString["KeyColumns"]) : null;
            qKeyValues = Request.QueryString["KeyValues"] != null ? StrFunc.StringArrayList.StringListToStringArray(Request.QueryString["KeyValues"]) : null;
            qKeyDatatypes = Request.QueryString["KeyDatatypes"] != null ? StrFunc.StringArrayList.StringListToStringArray(Request.QueryString["KeyDatatypes"]) : null;
            qDisplayName = Request.QueryString["DN"] ?? string.Empty;
            #endregion QueryString
            //
            if (StrFunc.IsFilled(qTableName))
                leftTitle = Ressource.GetString(Ressource.GetString(qTableName + "_", true) + " : " + qDisplayName, true);
            else
                leftTitle = Ressource.GetString("DBImageViewer", true);
            rightTitle = null;
            //
            PageTitle = leftTitle + " - " + Ressource.GetString("IMAGE");
            //
            HtmlPageTitle titleLeft = new HtmlPageTitle(leftTitle);
            Panel pnlHeader = new Panel
            {
                ID = "divHeader"
            };
            pnlHeader.Controls.Add(ControlsTools.GetBannerPage(this, titleLeft, null, IdMenu.GetIdMenu(IdMenu.Menu.Repository)));
			plhMain.Controls.Add(pnlHeader);

			AddToolBar();
			AddBody();
		}
		#endregion
		#region  protected override OnPreRender
		protected override void OnPreRender(EventArgs e)
		{
			this.DataBind();
			base.OnPreRender(e);
		}
		#endregion

		#region private InitializeComponent
		private void InitializeComponent()
		{
		}
		#endregion
		#region private CreateControls  

		// EG 20200820 [25556] Nouvelle interface GUI v10 (Mode Noir ou blanc) 
		// EG 20210204 [25590] Nouvelle gestion des mimeTypes autorisés dans l'upload d'un fichier
		// EG 20220215 [26251][WI582] New : Vulnerability on unrestricted file upload : Enhancement
		protected void AddToolBar()
		{
			Panel pnlToolBar = new Panel() { ID = "divalltoolbar", CssClass = CSSMode + " repository" };
			WCToolTipLinkButton btnUpload = ControlsTools.GetAwesomeButtonAction("btnModify", "fas fa-edit", true);

			WCToolTipLinkButton btnDelete = ControlsTools.GetAwesomeButtonAction("btnRemove", "fa fa-trash-alt", true);
			btnDelete.Click += new EventHandler(this.BtnDelete_Click);

			// PM 20240604 [XXXXX] Ajout gestion GUID pour les restrictions
			bool isReadOnly = false;
			string parentGUID = Request.QueryString["GUID"];
			if (StrFunc.IsFilled(parentGUID))
			{
				ReferentialsReferential referential = DataCache.GetData<ReferentialsReferential>(parentGUID + "_Referential");
				isReadOnly = (referential != default(ReferentialsReferential)) ? referential.isLookReadOnly || (referential.consultationMode == Cst.ConsultationMode.ReadOnly) : false;
			}

			if (SessionTools.IsUserWithLimitedRights() || isReadOnly)
			{
				btnUpload.Enabled = false;
				btnDelete.Enabled = false;
				btnUpload.Visible = false;
				btnDelete.Visible = false;
			}
			else
			{
				btnUpload.Attributes.Add("onclick", JavaScript.GetWindowOpenUpload(qTableName, qTableName, qColumnName, qKeyColumns, qKeyValues, qKeyDatatypes, qDisplayName, string.Empty, qColumnIDAUPD, qColumnDTUPD, parentGUID));
				btnDelete.Click += new EventHandler(this.BtnDelete_Click);
			}

			pnlToolBar.Controls.Add(btnUpload);
			pnlToolBar.Controls.Add(btnDelete);
			plhMain.Controls.Add(pnlToolBar);
		}

		// EG 20200820 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc) 
		protected void AddBody()
		{
			Panel pnlBody = new Panel() { ID = "divbody", CssClass = CSSMode + " repository" };

			if (qTableName != string.Empty && qColumnName != string.Empty)
			{
				LOFileColumn dbfc = new LOFileColumn(SessionTools.CS,
					qTableName, qColumnName, qColumnFileName, qColumnFileType, qKeyColumns, 
					qKeyValues, qKeyDatatypes, qColumnIDAUPD, qColumnDTUPD);
				
				LOFile loFile = dbfc.LoadFile();
				
				byte[] fileContent = loFile.FileContent;
				if (fileContent != null)
				{
					string fileName = string.Empty;
					fileName += qTableName;
					fileName += qColumnName;
					for (int i = 0; i < qKeyValues.Length; i++)
						fileName += qKeyValues[i];
					// EG 20100421 ".png" replace ".000" to see image on the page
					fileName += ".png";
					string localFilePath = SessionTools.TemporaryDirectory.ImagesPathMapped + fileName;
					if (Cst.ErrLevel.SUCCESS == FileTools.WriteBytesToFile(fileContent, localFilePath, FileTools.WriteFileOverrideMode.Override))
					{
                        int realImageWidth;
                        int realImageHeight;
                        int imageWidth = ThumbnailMaxWidth;
						int imageHeight = ThumbnailMaxHeight;

						System.Web.UI.WebControls.Image myLogo = new System.Web.UI.WebControls.Image();
						System.Drawing.Bitmap myImage = new Bitmap(SessionTools.TemporaryDirectory.ImagesPathMapped + fileName);
						System.Drawing.Size myImageSize = myImage.Size;
						realImageWidth = myImageSize.Width;
						realImageHeight = myImageSize.Height;
						myImage.Dispose();
						if (realImageWidth < imageWidth && realImageHeight < imageHeight)
						{
							imageWidth = realImageWidth;
							imageHeight = realImageHeight;
						}
						else
						{
							decimal rapportWidth = ((decimal)ThumbnailMaxWidth / (decimal)realImageWidth);
							decimal rapportHeight = ((decimal)ThumbnailMaxHeight / (decimal)realImageHeight);
							decimal rapport = (rapportWidth < rapportHeight ? rapportWidth : rapportHeight);
							imageWidth = (int)(realImageWidth * rapport);
							imageHeight = (int)(realImageHeight * rapport);
						}

						// 20100706 MF - added a random parameter to the image url (<fullpath>?<randomparameter>) 
						// to force the image content reload and turn the browser cache off. 
						myLogo.ImageUrl = SessionTools.TemporaryDirectory.ImagesPath + fileName +
							String.Format("?{0}", DateTime.UtcNow.Ticks);

						myLogo.Width = Unit.Pixel(imageWidth);
						myLogo.Height = Unit.Pixel(imageHeight);

						Panel pnlImage = new Panel()
						{
							ID = "divimgviewer",
							//Width = Unit.Pixel(imageWidth),
							//Height = Unit.Pixel(imageHeight)
						};
						pnlImage.Controls.Add(myLogo);
						pnlBody.Controls.Add(pnlImage);

                        Label imageLabelInfos = new Label
                        {
                            Text = Ressource.GetString("RealSize") + " : " + realImageWidth.ToString() + " x " + realImageHeight.ToString()
                        };
                        pnlBody.Controls.Add(imageLabelInfos);
					}
				}
				else
				{
                    Label lblNoImage = new Label
                    {
                        Text = "&lt;" + Ressource.GetString("lblNoImage") + "&gt;"
                    };
                    pnlBody.Controls.Add(lblNoImage);

                    if (FindControl("btnRemove") is LinkButton findButton)
                        findButton.Enabled = false;
                }
			}
			plhMain.Controls.Add(pnlBody);
		}
		#endregion
		#region private btnDelete_Click
		private void BtnDelete_Click(object sender, System.EventArgs e)
		{
			LOFileColumn dbfc = new LOFileColumn(SessionTools.CS, 
                qTableName, qColumnName, qColumnFileName, qColumnFileType, qKeyColumns, qKeyValues, qKeyDatatypes, qColumnIDAUPD, qColumnDTUPD);
			dbfc.DeleteFile();
			Server.Transfer(this.Page.Request.RawUrl);
		}
		#endregion
		#region private Page_Load
		protected void Page_Load(object sender, System.EventArgs e)
		{
            PageTools.SetHead(this, PageTitle, null, null);
		}
		#endregion private 
	}

}
