using System;
using System.Drawing;
using System.IO;
using System.Web.UI;
using EFS.ACommon;
using EFS.Common;
using EFS.Common.Web;


namespace EFS.Spheres
{
    // EG 20221010 [XXXXX] Changement de nom de la page principale : mainDefault en default
    public partial class DefaultMaster : System.Web.UI.MasterPage
    {
        // EG 20220920 [XXXX][WIXXX] Gestion du Logo de banniere
        protected void Page_Load(object sender, EventArgs e)
        {
            SetLogo();
        }

        // EG 20220920 [XXXX][WIXXX] Gestion du Logo de banniere
        // EG 20230513 [WI922] Spheres Portal : EFS|BDX GUI Adaptation
        private void SetLogo()
        {
            #region Logo
            string fileName = null;
            bool isCustomize = SessionTools.License.IsLicFunctionalityAuthorised(LimitationFunctionalityEnum.CUSTOMIZEBANNER);
#if DEBUG
            isCustomize = true;
#endif
            //Set Entity logo 
            if (Software.IsSoftwarePortal())
            {
                fileName = @"~/Images/Logo_Entity/EuroFinanceSystems/LogoEFS.png";
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
                    //if (bitmapWidth > 150 || bitmapHeight > 50)
                    //{
                    //    imgLogoCompany.Width = 150;
                    //    imgLogoCompany.Height = 50;
                    //}
                    bitmap.Dispose();
                    bitmap = null;
                }
                catch { }
                imgLogoCompany.ToolTip = string.Empty;
                imgLogoCompany.Style.Add(HtmlTextWriterStyle.Cursor, "pointer");
                imgLogoCompany.Attributes.Add("onclick", "document.getElementById('main').src='Welcome.aspx';return false;");
            }
            else
            {
                imgLogoCompany.Visible = false;
            }
            #endregion Logo
        }
    }
}