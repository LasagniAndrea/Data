using System;
using System.Collections.Generic;
using EFS.Common;
using System.IO;
using System.Web.UI;
using System.Web.UI.WebControls;
using EFS.Common.Web;
using EFS.ACommon;

namespace EFS.Spheres
{
    /// EG 20210614 [25500] New Customer Portal
    // EG 20230513 [WI922] Spheres Portal : EFS|BDX GUI Adaptation
    public partial class Welcome : PageBase
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            activeUser.Text = string.Empty;
            lastLogin.Text = string.Empty;
            if (StrFunc.IsFilled(SessionTools.Collaborator_DISPLAYNAME))
            {
                activeUser.Text = SessionTools.Collaborator_IDENTIFIER;
                if (DtFunc.IsDateTimeFilled(SessionTools.DTLASTLOGIN))
                {
                    lastLogin.Text = Ressource.GetString("DTLASTLOGIN") + Cst.HTMLSpace + DtFuncExtended.DisplayTimestampUTC(SessionTools.DTLASTLOGIN,
                        new AuditTimestampInfo()
                        {
                            TimestampZone = SessionTools.AuditTimestampZone,
                            Collaborator = SessionTools.Collaborator,
                            Precision = Cst.AuditTimestampPrecision.Second
                        });
                }
                lblWelcomeCustomerPortal.InnerText = Ressource.GetString(lblWelcomeCustomerPortal.ID);
                lblWelcomeCustomerPortal2.InnerText = Ressource.GetString(lblWelcomeCustomerPortal2.ID);
                infoCustomer.InnerText = SessionTools.Collaborator_ENTITY_DESCRIPTION;
                hidIsConnected.Value = "yes";
                if (!IsPostBack)
                    hidIDA.Value = SessionTools.Collaborator_IDA.ToString();
            }
            else
            {
                activeUser.Text = string.Empty;
                lastLogin.Text = string.Empty;
                hidIsConnected.Value = "no";
            }

            string fileName = Request.ApplicationPath + ControlsTools.GetLogoForEntity(@"/CustomerPortal/Image/LogoEFS-BDX.png"); 

            if (File.Exists(Server.MapPath(fileName)))
                logoCustomer.Style.Add(HtmlTextWriterStyle.BackgroundImage, String.Format(@"url({0})", fileName));
        }
    }
}