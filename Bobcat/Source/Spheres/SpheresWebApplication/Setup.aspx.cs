using EFS.ACommon;
using EFS.ApplicationBlocks.Data;
using EFS.Common;
using EFS.Common.Web;
using System;
using System.Web.UI.WebControls;
using System.Xml;


namespace EFS.Spheres
{
    /// <summary>
    /// Page utilisée pour la personnalisation du Web.Config (RDBMS, Server, Database, User, ...)
    /// </summary>
    // EG 20190125 DOCTYPE Conformity HTML5
    public partial class SetupPage : PageBase
	{
		protected string leftTitle, rightTitle;
		protected Label lblTitle;

		private readonly XmlDocument doc = new XmlDocument();
		private readonly string softwareLongName = Software.LongName;
		private readonly string softwareName     = Software.Name;
		protected System.Web.UI.WebControls.Label lblTitleUser;
		private string errorMsg = null;

		// EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
		protected void Page_Load(object sender, System.EventArgs e)
		{
			imglogosoftware.ImageUrl = @"Images\Logo_Software\Spheres_Banner_v" + Software.YearCopyright + ".png";

			leftTitle  = softwareLongName + " Setup";
			rightTitle = string.Empty;
			this.PageTitle = leftTitle;
			PageTools.SetHead(this, leftTitle, null, null);

			#region Header
			HtmlPageTitle titleLeft = new HtmlPageTitle(leftTitle);
			HtmlPageTitle titleRight = new HtmlPageTitle(rightTitle);
            Panel pnlHeader = new Panel
            {
                ID = "divHeader"
            };
            pnlHeader.Controls.Add(ControlsTools.GetBannerPage(this, titleLeft, titleRight, IdMenu.GetIdMenu(IdMenu.Menu.About), null));
			plhHeader.Controls.Add(pnlHeader);
			#endregion Header

			if (!this.IsPostBack)
			{
				#region Load DDL
                ddlSourceDisplay.Items.Add(new ListItem("HIDE", "HIDE"));
                ddlSourceDisplay.Items.Add(new ListItem("DISABLED", "DISABLED"));
                ddlSourceDisplay.Items.Add(new ListItem("ENABLED_DATABASENAME", "ENABLED_DATABASENAME"));
                ddlSourceDisplay.Items.Add(new ListItem("ENABLED", "ENABLED"));
                foreach (string hashAlgorithm in Enum.GetNames(typeof(Cst.HashAlgorithm)))
				{
					ddlOldHash.Items.Add(new ListItem(hashAlgorithm, hashAlgorithm));
					ddlHash.Items.Add(new ListItem(hashAlgorithm, hashAlgorithm));
				}
				foreach(string rdbms in Enum.GetNames(typeof(RdbmsEnum)))
				{
					if (rdbms.StartsWith("ORA") || rdbms.StartsWith("SQL"))
					{
						ddlRdbmsName.Items.Add(new ListItem(rdbms, rdbms));
						ddlRdbmsNameLog.Items.Add(new ListItem(rdbms, rdbms));
					}
				}
				#endregion Load DDL
				ControlsTools.DDLSelectByValue(ddlSourceDisplay, SystemSettings.GetAppSettings(softwareName +"_"+ ddlSourceDisplay.ID.Substring(3)));
				//ControlsTools.DDLSelectByValue(ddlHash, SystemSettings.GetAppSettings(softwareName +"_"+ ddlHash.ID.Substring(3)));

				string hashData = SystemSettings.GetAppSettings(softwareName + "_" + ddlHash.ID.Substring(3));
				string[] hashDataElements = hashData.Split(';');
				if (ArrFunc.IsFilled(hashDataElements))
				{
					if (1 == hashDataElements.Length)
                    {
						ControlsTools.DDLSelectByValue(ddlOldHash, hashDataElements[0]);
						ControlsTools.DDLSelectByValue(ddlHash, Cst.HashAlgorithm.SHA256.ToString());
						txtDtDeprecatedOldHash.Text = DateTime.Today.ToShortDateString();

					}
					else
                    {
						ControlsTools.DDLSelectByValue(ddlHash, hashDataElements[0]);
						ControlsTools.DDLSelectByValue(ddlOldHash, hashDataElements[1]);
						DateTime dtDeprecated = new DtFunc().StringDateISOToDateTime(hashDataElements[2]);
						txtDtDeprecatedOldHash.Text = dtDeprecated.Date.ToShortDateString();
					}

				}
				#region Load Data Access
				txtUserName.Text = SystemSettings.GetAppSettings(softwareName +"_"+ txtUserName.ID.Substring(3));
				string originalPwd = SystemSettings.GetAppSettings(softwareName +"_"+ txtPwd.ID.Substring(3));
				txtPwd.Attributes["value"] = originalPwd;
				txtPwd.Attributes.Add("OriginalPwd", originalPwd);
				ControlsTools.DDLSelectByValue(ddlRdbmsName, SystemSettings.GetAppSettings(softwareName +"_"+ ddlRdbmsName.ID.Substring(3)));
				txtServerName.Text	= SystemSettings.GetAppSettings(softwareName +"_"+ txtServerName.ID.Substring(3));
				txtDatabaseName.Text= SystemSettings.GetAppSettings(softwareName +"_"+ txtDatabaseName.ID.Substring(3));
				#endregion Load Data Access
				#region Load Log Access
				txtUserNameLog.Text = SystemSettings.GetAppSettings(softwareName +"_"+ txtUserNameLog.ID.Substring(3));
				originalPwd = SystemSettings.GetAppSettings(softwareName +"_"+ txtPwdLog.ID.Substring(3));
				txtPwdLog.Attributes["value"] = originalPwd;
				txtPwdLog.Attributes.Add("OriginalPwd", originalPwd);
				ControlsTools.DDLSelectByValue(ddlRdbmsNameLog, SystemSettings.GetAppSettings(softwareName +"_"+ ddlRdbmsNameLog.ID.Substring(3)));
				txtServerNameLog.Text	= SystemSettings.GetAppSettings(softwareName +"_"+ txtServerNameLog.ID.Substring(3));
				txtDatabaseNameLog.Text= SystemSettings.GetAppSettings(softwareName +"_"+ txtDatabaseNameLog.ID.Substring(3));
				#endregion Load Log Access
			}
		}

		#region Code généré par le Concepteur Web Form
		override protected void OnInit(EventArgs e)
		{
			InitializeComponent();
			base.OnInit(e);

			Form = Setup;
			AntiForgeryControl();

			divalltoolbar.CssClass = CSSMode + " about";
			divbody.Attributes.Add("class", divalltoolbar.CssClass);
			divSphereSystem.Attributes.Add("class", divalltoolbar.CssClass);
			divRdbms.Attributes.Add("class", divalltoolbar.CssClass);
			divData.Attributes.Add("class", divalltoolbar.CssClass);
			divLog.Attributes.Add("class", divalltoolbar.CssClass);

			btnOk.Text = @"<i class='fa fa-save'></i> Ok";
			btnCancel.Text = @"<i class='fa fa-times'></i> Cancel";
			btnApply.Text = @"<i class='fa fa-save'></i> Apply";

			AbortRessource = true;

			//lblSourceDisplay.Text = Ressource.GetString("lblData");
			lblSourceDisplay.ToolTip = "Select the possibility of consulting/modifying the characteristics of access to the data at the login.";

			//lblHash.Text = Ressource.GetString("lblHash");
			lblHash.ToolTip = "Select a hash algorithm for the storage of the passwords user";

			//lblRdbmsName.Text = Ressource.GetString("lblRdbmsName");
			lblRdbmsName.ToolTip = "Select a RDBMS into the list";
			//lblRdbmsNameLog.Text = lblRdbmsName.Text;
			lblRdbmsNameLog.ToolTip = lblRdbmsName.ToolTip;

			//lblServerName.Text = Ressource.GetString("lblServerName");
			lblServerName.ToolTip = "Enter a server name for MS SQLServer® or an Oracle® instance name";
			//lblServerNameLog.Text = lblServerName.Text;
			lblServerNameLog.ToolTip = lblServerName.ToolTip;

			//lblDatabaseName.Text = Ressource.GetString("lblDatabaseName");
			lblDatabaseName.ToolTip = "Enter a database name for MS SQLServer® or a schema name for Oracle®";
			//lblDatabaseNameLog.Text = lblDatabaseName.Text;
			lblDatabaseNameLog.ToolTip = lblDatabaseName.ToolTip;

			//lblUserName.Text = Ressource.GetString("lblUserName");
			lblUserName.ToolTip = "Enter the user name (usually data base owner)";
			//lblUserNameLog.Text = lblUserName.Text;
			lblUserNameLog.ToolTip = lblUserName.ToolTip;

			//lblPwd.Text = Ressource.GetString("lblPwd");
			lblPwd.ToolTip = "Enter the password";
			//lblPwdLog.Text = lblPwd.Text;
			lblPwdLog.ToolTip = lblPwd.ToolTip;

			//txtDtDeprecatedOldHash.CssClass = "DtPicker";
		}

		/// <summary>
		/// Méthode requise pour la prise en charge du concepteur - ne modifiez pas
		/// le contenu de cette méthode avec l'éditeur de code.
		/// </summary>
		private void InitializeComponent()
		{    

		}
		#endregion

		private void SetAttribute(string pKey, string pNewValue)
		{
			// Use an XPath query to look up the "add" element in this configuration having a matching "key" attribute
			string select = "/configuration/appSettings/add[@key='" + pKey + "']";
			XmlNode node = doc.SelectSingleNode(select);
			// Modify the element
			XmlElement element = (XmlElement) node;
            //PL 20120619 
            if (pKey.EndsWith("Log") && (element == null))
            {
                //Si element est null mais que la clé est uen clé de l'envir. de Log, on tolère
            }
            else
            {
			    //Si element est null cela entrainera uen erreur
                element.SetAttribute("value", pNewValue);
            }
		}
		private bool WriteInConfigFile() 
		{
			bool isRet = false;
			try
			{
				// Open web.config file
				string fileName = Server.MapPath("web.config"); 
				
				doc.Load(fileName);

				string key;
				string newValue;
				key = softwareName +"_"+ ddlSourceDisplay.ID.Substring(3);
				newValue = ddlSourceDisplay.SelectedValue;
				SetAttribute(key, newValue);

				key = softwareName +"_"+ ddlHash.ID.Substring(3);
				// EG 20210304 Gestion du MD5 depecrated
				//newValue = ddlHash.SelectedValue;
				DateTime dtDeprecated = new DtFunc().StringToDateTime(txtDtDeprecatedOldHash.Text);
				newValue = ddlHash.SelectedValue + ";" + ddlOldHash.SelectedValue + ";" + DtFunc.DateTimeToStringDateISO(dtDeprecated);

				SetAttribute(key, newValue);
				#region Data Access
				key = softwareName +"_"+ txtUserName.ID.Substring(3);
				newValue = txtUserName.Text;
				SetAttribute(key, newValue);

				#region Pwd
				if (txtPwd.Attributes["OriginalPwd"] != txtPwd.Text)
				{
					//Password modified
					key = softwareName +"_"+ txtPwd.ID.Substring(3);
					newValue = Cryptography.Encrypt(txtPwd.Text);
					SetAttribute(key, newValue);

					key = softwareName +"_"+ "IsPwdCrypted";
					newValue = (txtPwd.Text.Trim().Length == 0 ? "0":"1");
					SetAttribute(key, newValue);
				}
				#endregion Pwd

				key = softwareName +"_"+ ddlRdbmsName.ID.Substring(3);
				newValue = ddlRdbmsName.SelectedValue;
				SetAttribute(key, newValue);

				key = softwareName +"_"+ txtServerName.ID.Substring(3);
				newValue = txtServerName.Text;
				SetAttribute(key, newValue);

				key = softwareName +"_"+ txtDatabaseName.ID.Substring(3);
				newValue = txtDatabaseName.Text;
				SetAttribute(key, newValue);
				#endregion Data Access
				#region Log Access
				key = softwareName +"_"+ txtUserNameLog.ID.Substring(3);
				newValue = txtUserNameLog.Text;
				SetAttribute(key, newValue);

				#region Pwd
				if (txtPwdLog.Attributes["OriginalPwd"] != txtPwdLog.Text)
				{
					//Password modified
					key = softwareName +"_"+ txtPwdLog.ID.Substring(3);
					newValue = Cryptography.Encrypt(txtPwdLog.Text);
					SetAttribute(key, newValue);

					key = softwareName +"_"+ "IsPwdCrypted" +"Log";
					newValue = (txtPwdLog.Text.Trim().Length == 0 ? "0":"1");
					SetAttribute(key, newValue);
				}
				#endregion Pwd

				key = softwareName +"_"+ ddlRdbmsNameLog.ID.Substring(3);
				newValue = ddlRdbmsNameLog.SelectedValue;
				SetAttribute(key, newValue);

				key = softwareName +"_"+ txtServerNameLog.ID.Substring(3);
				newValue = txtServerNameLog.Text;
				SetAttribute(key, newValue);

				key = softwareName +"_"+ txtDatabaseNameLog.ID.Substring(3);
				newValue = txtDatabaseNameLog.Text;
				SetAttribute(key, newValue);
				#endregion Log Access

				// Save the configuration
				doc.Save(fileName);
				isRet = true;
			}
			catch (Exception ex)
			{
				errorMsg = ex.Message;
			}
			return isRet;
		}

		// EG 20210222 [XXXXX] Suppression inscription function AutoClose
		protected void BtnCancel_Click(object sender, System.EventArgs e)
		{
			JavaScript.CallFunction(this, "self.close();");
		}

		protected void BtnApply_Click(object sender, System.EventArgs e)
		{
			Save(false);
		}

		protected void BtnOk_Click(object sender, System.EventArgs e)
		{
			Save(true);
		}	
		private void Save(bool isClosing)
		{
			bool isRet = WriteInConfigFile();
			string msg;
			if (isRet)
				msg = "Data recorded successfully !";
			else
			{
				errorMsg = errorMsg.Replace(@"""", string.Empty);
				errorMsg = errorMsg.Replace(@"\", @"/");
				msg = "Not recorded data !" + Cst.CrLf + Cst.CrLf + errorMsg;
				errorMsg = null;
			}
			//JavaScript.AlertStartUpImmediate(this, msg, isClosing);
            JavaScript.DialogStartUpImmediate(this, msg, isClosing);
		}
	}
}
