using EFS.ACommon;
using EFS.Common;
using EFS.Common.EfsSend;
using System;
using System.IO;
using System.Web.UI.HtmlControls;

namespace OTC
{
    /// <summary>
    /// Description résumée de Pascal_Test.
    /// </summary>
    public partial class Pascal_Test : System.Web.UI.Page
	{
		protected System.Web.UI.WebControls.Button Button3;
		protected HtmlGenericControl lnkCss;
	
		protected void Page_Load(object sender, System.EventArgs e)
		{
            // Create a reference to the current directory.
            string s = this.Request.ApplicationPath;
            /*
            s = this.Request.FilePath;
            s = this.Request.MapPath("XXX");
            s = this.Request.Path;
            s = this.Request.PathInfo;
            s = this.Request.PhysicalApplicationPath;
            s = this.Request.PhysicalPath;
			s = @"\x20";
            */

			string s3 = "\x20";
            _ = s.Replace(@"\x20", s3);

            
            DirectoryInfo dir = new DirectoryInfo(this.Request.PhysicalApplicationPath + "Includes");
            FileInfo[] filesInDir = dir.GetFiles();
            foreach(FileInfo file in filesInDir)
            {
                if(file.Extension == ".css")
                {
                    _ = file.Name.Substring(8, file.Name.Length - 8 - 4);
                    //ddl1.Items.Add(new ListItem(file.Name, sValue));
                }
            }
            //DDLColor.Attributes.Add("disabled", "true");
        }


		#region Code généré par le Concepteur Web Form
		override protected void OnInit(EventArgs e)
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
		#endregion

		private void TextBox1_TextChanged(object sender, System.EventArgs e)
		{
//            HtmlSelect   ddlRef2 = new HtmlSelect();
//            ddlRef2.ID = ControlID.GetID(rrc.ColumnName,null,Cst.DDL)+"HTML";
//            ddlRef2.Disabled = !isEnabled;
//            ddlRef2.Attributes.Add("class", "ddlCapture");
//            ddlRef2.Attributes.Add("runat", "server");
//            ddlRef2.EnableViewState = true;
//            ddlRef2.Items.Add(new ListItem(Ressource.GetString("AAAAAA"), Cst.PriceCurve_Forward));
//            ddlRef2.Items.Add(new ListItem(Ressource.GetString("BBBBBB"), Cst.PriceCurve_Spot));
//            ddlRef2.Items.Insert(0,new ListItem(string.Empty, string.Empty));
//            ddlRef2.Items[0].Attributes.Add("style", "color:red;background-color:white");
//            ddlRef2.Items[1].Attributes.Add("style", "color:white;background-color:red");
//            ddlRef2.Items[2].Attributes.Add("style", "color:blue;background-color:yellow");
//
//
//            Control ctrRef2;
//            ctrRef2 = this.FindControl(ControlID.GetID(rrc.ColumnName,rrc.DataType,(IsDataForDDL(rrc)?Cst.DDL:null))+"HTML");
//            string x = ((HtmlSelect) ctrRef2).Value;
//            Debug.WriteLine(x);

		}

        private void Button4_Click(object sender, System.EventArgs e)
        {
            //EFS.Common.JavaScript.AlertOnLoad2(this,"Serveur: " + HiddenValue.Value, false);
            //Color.FromName(DDLColor.SelectedColor);
            //EFS.Common.JavaScript.AlertOnLoad2(this,"Serveur: " + DDLColor.SelectedColor, false);
            //DDLColor.Enabled = false;
        }

		private void PL_TestSendEmail()
		{
//            Email email = new Email( SystemSettings.GetAppSettings("Spheres_ErrorEmailSmtpServer") );
			
//            email.From = "ploubat@euro-finance-systems.fr";
//            email.To   = "ploubat@euro-finance-systems.fr";
//            email.Cc   = "ploubat@euro-finance-systems.fr";
			
//            email.MailPriority = System.Web.Mail.MailPriority.Low;
//            email.Subject      = "Test Mail from OTCml";

////			email.MailFormat   = System.Web.Mail.MailFormat.Text;            
////			email.Body    = "Row1" + Cst.CrLf + "Row2";

//            email.MailFormat   = System.Web.Mail.MailFormat.Html;            
//            email.Body    = @"<html>
//<body>
//    <table>
//        <tr>
//            <td bgcolor=""red"">
//                Row1
//            </td>
//        </tr>
//        <tr>
//            <td style=""color:blue;"">
//                Row2
//            </td>
//        </tr>
//    </table>
//</body>
//</html>";

//            lRet = email.SendEmail();
//            email = null;
            //
            string smtpserver = SystemSettings.GetAppSettings("Spheres_ErrorEmailSmtpServer");
            string strPort = SystemSettings.GetAppSettings("Spheres_ErrorEmailSmtpPort");
            int port = (StrFunc.IsFilled(strPort) ? Convert.ToInt32(strPort) : 25);
            //
            string pdw = SystemSettings.GetAppSettings("Spheres_ErrorEmailSmtpUserNamePwd");
            bool isPwdCrypted = Convert.ToBoolean(SystemSettings.GetAppSettings("Spheres_ErrorEmailSmtpIsPwdCrypted"));
            if (isPwdCrypted)
                pdw = Cryptography.Decrypt(pdw);
            //
            string smtpUser = SystemSettings.GetAppSettings("Spheres_ErrorEmailSmtpUserName");
            //
            EfsSendSMTP sendSmtp = new EfsSendSMTP(smtpserver, port, smtpUser, pdw);
            //
            sendSmtp.MailMessage.From = new EfsSendSMTPContact("ploubat@euro-finance-systems.fr");
            sendSmtp.MailMessage.To = new EfsSendSMTPContact[1] { new EfsSendSMTPContact("ploubat@euro-finance-systems.fr") };
            sendSmtp.MailMessage.Cc = new EfsSendSMTPContact[1] { new EfsSendSMTPContact("ploubat@euro-finance-systems.fr") };
            sendSmtp.MailMessage.Priority = EfsSendSMTPPriorityEnum.Low;
            sendSmtp.MailMessage.Subject.Value = "Test Mail from OTCml";
            sendSmtp.MailMessage.Body.format = EfsSendFormatEnum.Html;
            sendSmtp.MailMessage.Body.Value = @"<html>
<body>
    <table>
        <tr>
            <td bgcolor=""red"">
                Row1
            </td>
        </tr>
        <tr>
            <td style=""color:blue;"">
                Row2
            </td>
        </tr>
    </table>
</body>
</html>";
            //
            sendSmtp.SendEmail();
		}

		protected void Button2_Click(object sender, System.EventArgs e)
		{
			PL_TestSendEmail();
		}


        protected void Button1_Click(object sender, EventArgs e)
        {
            FlyDocLibrary.SendFax sendFax = new FlyDocLibrary.SendFax();

            bool isTracking = false;
            EFS.Common.EfsSend.EfsSendFlyDoc sendFlyDoc = new EFS.Common.EfsSend.EfsSendFlyDoc
            {
                Login = new EFS.Common.EfsSend.EfsSendFDLogin
                {
                    UserName = "ploubat@euro-finance-systems.fr",
                    Password = new EFS.Common.EfsSend.EfsSendFDPassword
                    {
                        Value = "YS5kGEgk",
                        Crypted = EFS.Common.EfsSend.EfsSendFDCryptedEnum.no
                    }
                },
                Name = "FlyDoc",
                TransportName = EFS.Common.EfsSend.EfsSendFDTransportNameEnum.Fax,
                Message = new EFS.Common.EfsSend.EfsSendFDMessage
                {
                    From = new EFS.Common.EfsSend.EfsSendFDContact
                    {
                        Address = "+33148714248",
                        Company = "EFS",
                        Name = "Pascal Loubat"
                    },
                    To = new EFS.Common.EfsSend.EfsSendFDContact
                    {
                        Address = "+33148715966",
                        Company = "EFS",
                        Name = "Loubat Pascal"
                    },
                    Subject = new EFS.Common.EfsSend.EfsSendSubject
                    {
                        Value = "Sample fax"
                    },
                    Body = new EFS.Common.EfsSend.EfsSendBody
                    {
                        Value = "This is a sample fax, including two attachments"
                    },
                    Attachment = new EFS.Common.EfsSend.EfsSendStream[1]
                }
            };
            sendFlyDoc.Message.Attachment[0] = new EFS.Common.EfsSend.EfsSendStream
            {
                Value = @"C:\inetpub\wwwroot\SpheresWebSite\ErrorLogsFiles\Confirm.pdf"
            };
            _ = sendFax.Run(sendFlyDoc, isTracking, out _, out _, out _, out _, out _);
        }
}
}
