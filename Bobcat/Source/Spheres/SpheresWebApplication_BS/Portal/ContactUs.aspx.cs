using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using EFS.ACommon;
using EFS.Common.Web;


namespace EFS.Spheres
{
    public partial class ContactUs : ContentPageBase
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            lblFirstName.Text = Ressource.GetString("FIRSTNAME", true);
            lblSurName.Text = Ressource.GetString("SURNAME", true);
            lblEmail.Text = Ressource.GetString("MAIL", true);
            lblFunction.Text = Ressource.GetString("FUNCTIONNAME", true);
            lblCompanyName.Text = Ressource.GetString("CompanyName", true);
            lblPhoneNumber.Text = Ressource.GetString("TELEPHONENUMBER", true);
            lblSector.Text = Ressource.GetString("CORPORATESECTOR", true);
            lblRequest.Text = Ressource.GetString("Request", true);
            lblMessage.Text = Ressource.GetString("MESSAGE", true);

            ddlSector.Items.Add(new ListItem("Assurance", "insurance"));
            ddlSector.Items.Add(new ListItem("Banque", "bank"));
            ddlSector.Items.Add(new ListItem("Corporate", "corporate"));
            ddlSector.Items.Add(new ListItem("Etablissement de crédit", "credit"));
            ddlSector.Items.Add(new ListItem("Intermédiaire financier", "intermediary"));
            ddlSector.Items.Add(new ListItem("Société de Gestion", "management"));
            ddlSector.Items.Add(new ListItem("Autres", "others"));

            ddlRequest.Items.Add(new ListItem("Consulting", "consulting"));
            ddlRequest.Items.Add(new ListItem("Périmètre", "perimeter"));
            ddlRequest.Items.Add(new ListItem("Services", "services"));
            ddlRequest.Items.Add(new ListItem("Solution", "solution"));
            ddlRequest.Items.Add(new ListItem("Autres", "others"));

            btnSend.Text = Ressource.GetString("btnSend", true);
        }
    }
}