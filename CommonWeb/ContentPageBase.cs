#region Using Directives
using EFS.ACommon;
using System;
using System.Collections;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
#endregion Using Directives

// EG 20231129 [WI756] Spheres Core : Refactoring Code Analysis with Intellisense

namespace EFS.Common.Web
{
    /// <summary>
    /// Description résumée de PageBase. 
    /// </summary>
    // EG 20180423 Analyse du code Correction [CA1405]
    [ComVisible(false)] 
    public partial class ContentPageBase : PageBase
    {
        #region Accessors
        public override ContentPlaceHolder MasterPage_ContentPlaceHolder
        {
            get
            {

                ContentPlaceHolder cph = null;
                if (null != Master)
                    cph = Master.FindControl("mc") as ContentPlaceHolder;
                return cph;
            }
        }
        public override string ContentPlaceHolder_ClientID
        {
            get
            {
                string cphID = string.Empty;
                ContentPlaceHolder cph = MasterPage_ContentPlaceHolder;
                if (null != cph)
                    cphID = cph.ClientID + "_";
                return cphID;
            }
        }
        public override string ContentPlaceHolder_UniqueID
        {
            get
            {
                string cphID = string.Empty;
                ContentPlaceHolder cph = MasterPage_ContentPlaceHolder;
                if (null != cph)
                    cphID = cph.UniqueID + "$";
                return cphID;
            }
        }
        #endregion Accessors

        #region Constructor
        public ContentPageBase() : base()
        {
        }
        #endregion

        #region Methods
        public virtual void SetFormClass(string pClass)
        {
            if (this.Master.FindControl("mainForm") is HtmlForm form)
                form.Attributes.Add("class", pClass);
        }
        protected override void InitializeScriptManager()
        {
        }
        protected override void PageRender(string pActiveElement)
        {
        }
        protected override void RequestTrackGen()
        {
        }
        protected override void DialogImmediate()
        {
        }
        protected override void CreateChildControls()
        {
        }
        protected override void PageRender(HtmlTextWriter writer)
        {
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="indexEnumerator"></param>
        protected override void ApplyResourcesForControls(IEnumerator indexEnumerator)
        {
            Type ty;
            string styname;

            while (indexEnumerator.MoveNext())
            {
                ty = indexEnumerator.Current.GetType();
                styname = ty.Name;
                switch (styname)
                {
                    case "ResourceBasedLiteralControl":
                    case "LiteralControl":
                        break;
                    case "HtmlForm":
                    case "HtmlTable":
                    case "Table":
                    case "HtmlTableRow":
                    case "TableRow":
                    case "HtmlTableCell":
                    case "TableCell":
                    case "PlaceHolder":
                    case "ContentPlaceHolder":
                    case "Panel":
                        Control frm = (Control)indexEnumerator.Current;
                        ApplyResourcesForControls(frm.Controls.GetEnumerator());
                        break;
                    case "HtmlGenericControl":
                        HtmlGenericControl htmlGeneric = (HtmlGenericControl)indexEnumerator.Current;
                        if ("label" == htmlGeneric.TagName)
                        {
                            htmlGeneric.InnerText = Ressource.GetString(htmlGeneric.ID.ToString());
                        }
                        else
                        {
                            Control gen = (Control)indexEnumerator.Current;
                            ApplyResourcesForControls(gen.Controls.GetEnumerator());
                        }
                        break;
                    case "HtmlButton":
                        HtmlButton htmlBtn = (HtmlButton)indexEnumerator.Current;
                        if (htmlBtn.ID != null)
                            htmlBtn.InnerText = Ressource.GetString(htmlBtn.ID.ToString());
                        break;
                    case "Label":
                    case "WCTooltipLabel":
                        Label lbl = (Label)indexEnumerator.Current;
                        if (lbl.ID != null)
                            lbl.Text = Ressource.GetString(lbl.ID.ToString());
                        break;
                    case "ImageButton":
                        ImageButton imgBtn = (ImageButton)indexEnumerator.Current;
                        imgBtn.ToolTip = Ressource.GetString(imgBtn.ID.ToString());
                        break;
                    case "LinkButton":
                        LinkButton lbtn = (LinkButton)indexEnumerator.Current;
                        lbtn.Text = Ressource.GetString(lbtn.ID.ToString());
                        break;
                    case "Button":
                        Button btn = (Button)indexEnumerator.Current;
                        btn.Text = Ressource.GetString(btn.ID.ToString());
                        break;
                    case "CheckBox":
                        CheckBox chk = (CheckBox)indexEnumerator.Current;
                        chk.Text = Ressource.GetString(chk.ID.ToString());
                        break;
                    default:
                        if (styname.IndexOf("_ascx") > 0 || styname.StartsWith("WC") || styname.IndexOf("master") > 0)
                        {
                            Control uctrl = (Control)indexEnumerator.Current;
                            ApplyResourcesForControls(uctrl.Controls.GetEnumerator());
                        }
                        break;
                }
            }

        }

        public string AddPostBackReference(Control pControl)
        {
            return AddPostBackReference(pControl, string.Empty);
        }
        public string AddPostBackReference(Control pControl, string pArgument)
        {
            return String.Format("javascript:setTimeout('__doPostBack(\\'{0}\\',\\'{1}\\')',0)", pControl.UniqueID, pArgument);
        }


        /// <summary>
        ///  Génère une httpReponse qui ouvre un fichier excel avec le contenu du control source
        ///  <para>Le rendu HTML du control doit être constitué d'élément HMTL simples (table, Tr, Td,..)</para>
        /// </summary>
        /// <param name="pControlSource">Contrôle source</param>
        /// <param name="pFileName">Nom du fichier</param>
        protected void ExportControlToExcel(Control pControlSource, string pFileName)
        {
            //Spheres® récupère le rendu du gatagrid et ecrit la reponse avec le rendu du controle DataGrid
            //Spheres® envoie ensuite au client la sortie actuelle de la page et ferme le socket 

            Response.Clear();
            Response.AddHeader("content-disposition", StrFunc.AppendFormat("attachment;filename={0}.xls", pFileName));
            Response.ContentEncoding = Encoding.Default;
            Response.Charset = string.Empty;
            Response.Cache.SetCacheability(HttpCacheability.NoCache);
            Response.ContentType = Cst.TypeMIME.Vnd.MsExcel;

            StringWriter stringWrite = new StringWriter();

            HtmlTextWriter htmlWrite = new HtmlTextWriter(stringWrite);
            pControlSource.RenderControl(htmlWrite);
            pControlSource.EnableViewState = false;

            // Le NonBreakSpace (Alt 0160) n'est pas reconnu par Excel comme étant un séparateur de groupe
            // => Remplacer NonBreakSpace par un vrai espace. 
            string stringResponse = stringWrite.ToString();
            stringResponse = stringResponse.Replace(Cst.NonBreakSpace, Cst.Space);

            // add Content-Length (stringResponse.Length.ToString() = taille en octet du flux du fait de l'usage Encoding.Default)
            // Encoding.Default retourne Windows-1252 (sur un poste Européen)
            // Windows-1252 utilisée sur tous les continents, en Europe de l’Ouest, en Amérique, et dans une grande partie de l'Afrique ou de l’Océanie ainsi que certains pays d’Asie du Sud-Est
            Response.AddHeader("Content-Length", stringResponse.Length.ToString());

            Response.Write(stringResponse);
            Response.Flush();
            Response.Close();

        }


        /// <summary>
        /// Initialisation et Inscription pour affichae de la 
        /// </summary>
        /// <param name="pMessage"></param>
        /// <param name="pStatus"></param>
        public void SetDialogImmediate(string pFunctionName, string pMessage, ProcessStateTools.StatusEnum pStatus)
        {
            Control ctrl = this.MasterPage_ContentPlaceHolder.FindControl("divConfirmHeader");
            if (null != ctrl)
            {
                // Header de la dialogBox modale dialogBox modale
                HtmlContainerControl divHeader = ctrl as HtmlContainerControl;
                switch (pStatus)
                {
                    case ProcessStateTools.StatusEnum.ERROR:
                        divHeader.Attributes.Add("class", "modal-header btn-danger");
                        break;
                    case ProcessStateTools.StatusEnum.WARNING:
                        divHeader.Attributes.Add("class", "modal-header btn-warning");
                        break;
                    case ProcessStateTools.StatusEnum.SUCCESS:
                        divHeader.Attributes.Add("class", "modal-header btn-success");
                        break;
                    case ProcessStateTools.StatusEnum.NA:
                        divHeader.Attributes.Add("class", "modal-header btn-primary");
                        break;
                    case ProcessStateTools.StatusEnum.NONE:
                        divHeader.Attributes.Add("class", "modal-header btn-info");
                        break;
                }
                
            }
            // Titre de la dialogBox modale
            ctrl = this.MasterPage_ContentPlaceHolder.FindControl("confirmTitle");
            if (null != ctrl)
            {
                HtmlGenericControl spanTitle = ctrl as HtmlGenericControl;
                spanTitle.InnerText = this.Header.Title;
            }
            
            // Message d'information de la dialogBox modale
            ctrl = this.MasterPage_ContentPlaceHolder.FindControl("confirmMsg");
            if (null != ctrl)
            {
                HtmlGenericControl spanMsg = ctrl as HtmlGenericControl;
                spanMsg.InnerHtml = pMessage;
            }
            // Désactivation du bouton Submit de la dialogBox modale
            ctrl = this.MasterPage_ContentPlaceHolder.FindControl("btnConfirmSubmit");
            if (null != ctrl)
            {
                HtmlButton btnSubmit = ctrl as HtmlButton;
                btnSubmit.Attributes.Remove("onclick");
                btnSubmit.Visible = false;
            }
            // Transformation du bouton Cancel de la dialogBox modale
            ctrl = this.MasterPage_ContentPlaceHolder.FindControl("btnConfirmCancel");
            if (null != ctrl)
            {
                HtmlButton btnCancel = ctrl as HtmlButton;
                btnCancel.Attributes.Add("class", "btn btn-xs btn-apply");
                btnCancel.InnerText = Ressource.GetString("btnOk");
            }
            ScriptManager.RegisterStartupScript(this, this.GetType(), pFunctionName, "DisplayMessage();", true);
        }
        #endregion Methods
    }
}

