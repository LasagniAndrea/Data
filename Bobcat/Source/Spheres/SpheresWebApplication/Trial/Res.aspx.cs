using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using EFS.ACommon;
using EFS.Common; 

public partial class Trial_Res : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {

        ThreadTools.SetCurrentCulture(Cst.EnglishCulture);
        
        string culture = "fr-FR";
        System.Globalization.CultureInfo cultureInfo = System.Globalization.CultureInfo.CreateSpecificCulture(culture);
        string ret = Ressource.GetString("PERIODENUM_D", cultureInfo);
        lblfr.InnerText  =ret;  
        
    
        culture = "en-GB";
        cultureInfo = System.Globalization.CultureInfo.CreateSpecificCulture(culture);
        ret = Ressource.GetString("PERIODENUM_D", cultureInfo);
        lblen.InnerText  =ret;

        culture = "it-IT";
        cultureInfo = System.Globalization.CultureInfo.CreateSpecificCulture(culture);
        ret = Ressource.GetString("PERIODENUM_D", cultureInfo);
        lblit.InnerText = ret;


        ret = Ressource.GetString("PERIODENUM_D");
        lblNeutral.InnerText = ret;  

    }
}
