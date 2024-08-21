using System;
using System.Reflection;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;

using System.IO;

using System.Web;
using System.Web.SessionState;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;

using EFS.ACommon;
using EFS.Common; 
using EFS.Common.Web;

namespace EFS.Spheres
{
    /// <summary>
    /// Description résumée de MSExcel.
    /// </summary>
    //public partial class MSExcelPage : PageBase
    //20081208 PL Suppression de l'héritage sur PageBase car erreur sous .Net 2.0
    //FI 20120212 [] add filename in URL (le nom de sessionDvMSExcel est trop abstrait pour donner lieu à un nom de fichier)
    public partial class MSExcelPage : Page
    {
        string sessionDvMSExcel;
        string filename;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void Page_Load(object sender, System.EventArgs e)
        {
            sessionDvMSExcel = this.Request.QueryString["dvMSExcel"];
            filename = this.Request.QueryString["filename"];

            LoadMSExcel();
        }

        #region Code généré par le Concepteur Web Form
        override protected void OnInit(EventArgs e)
        {
            InitializeComponent();
            base.OnInit(e);
            //20081208 PL Suppression de l'héritage sur PageBase 
            //AbortNoCache = true;//Warning: Mise en cache nécessaire au bon fonctionnment (20050902 PL)

        }

        /// <summary>
        /// Méthode requise pour la prise en charge du concepteur - ne modifiez pas
        /// le contenu de cette méthode avec l'éditeur de code.
        /// </summary>
        private void InitializeComponent()
        {

        }
        #endregion
        /// <summary>
        /// 
        /// 
        /// </summary>
        private void LoadMSExcel()
        {
            Response.Clear();

            //On construit la reponse en spcifiant 
            //que le contenu est du type fichier excel
            Response.ContentType = "application/vnd.ms-excel";
            Response.ContentEncoding = Encoding.Default;
            Response.Charset = string.Empty;
            Response.AppendHeader("Content-Disposition", "attachment; filename=" + filename + ".xls");
            //
            this.EnableViewState = false;
            // FI 20200518 [XXXXX] Utilisation de DataCache
            DataSet ds = DataCache.GetData<DataSet>(sessionDvMSExcel);
            if (null != ds)
            {
                dgMSExcel.DataSource = ds.Tables[0].DefaultView;
                dgMSExcel.DataBind();
                //On inititalise un objet de type HtmlTextWriter avec le rendu du controle serveur DataGrid
                StringWriter sw = new StringWriter();
                HtmlTextWriter tw = new HtmlTextWriter(sw);
                dgMSExcel.RenderControl(tw);
                //Et on ecrit la reponse avec le rendu du controle DataGrid
                Response.Write(sw.ToString());
            }
            //Response.End();   
            Response.Flush();
            Response.Close();
        }



        

        private void TestPascal_ReadPdf()
        {
            int FileSize;
            string sFile = @"c:\lerepertoire\lefichier.pdf";
            System.IO.FileStream MyFileStream = new System.IO.FileStream(sFile, System.IO.FileMode.Open);
            FileSize = (int)MyFileStream.Length;

            byte[] Buffer = new byte[FileSize];

            MyFileStream.Read(Buffer, 0, FileSize);
            MyFileStream.Close();

            Response.ContentType = Cst.TypeMIME.Application.Pdf;
            // Est utilisé si on veut avoir un popup "sauvegarder sous..."
            // Response.AddHeader("content-disposition", "attachment; filename=lefichier.pdf");

            Response.BinaryWrite(Buffer);
            Response.End();
        }
    }
}
