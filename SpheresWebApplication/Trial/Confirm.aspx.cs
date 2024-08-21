using System;
using System.Data;
using System.Web;
using System.Text;
using System.Diagnostics;
using System.IO;
using System.Xml;
using System.Xml.Xsl;
using System.Xml.XPath;


using EFS.Common;
using EFS.Common.Web;

using EFS.ApplicationBlocks.Data;



namespace EFS.Spheres.Trial
{
	/// <summary>
	/// Description résumée de Confirm.
	/// </summary>
	public partial class ConfirmPage : System.Web.UI.Page
	{

		private void Page_Load(object sender, System.EventArgs e)
		{
			// Placer ici le code utilisateur pour initialiser la page
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
			this.btnHtml.Click += new System.EventHandler(this.BtnHtml_Click);
			this.btnPDF.Click += new System.EventHandler(this.BtnPDF_Click);
			this.Load += new System.EventHandler(this.Page_Load);

		}
		#endregion

		private void BtnHtml_Click(object sender, System.EventArgs e)
		{  
			StringReader sXMLTrade = null;
			try
			{
				string tradeIdentifier = txtTradeIdentifier.Text;
				string XslFile  = this.Request.MapPath(@"XML_Files/"+txtlXSLFile.Text);
				string HtmlFile = "IRD_HTML_OutPut.htm";

				string source = SessionTools.CS;
				string SQLQuery = "select IDENTIFIER,DEFINITION,FPML from dbo.TRADE where IDENTIFIER=" + tradeIdentifier;
				DataSet ds = DataHelper.ExecuteDataset(source, CommandType.Text, SQLQuery);
				DataTable dt = ds.Tables[0];
				//GlopPL verrue Replace() à revoir
				sXMLTrade = new StringReader(dt.Rows[0]["FPML"].ToString().Replace("xmlns=","xmln="));
				
				// Open XML file
				XmlDocument dataDoc = new XmlDocument();
				dataDoc.Load(sXMLTrade);

				// Create an XPathNavigator to use for the transform.
				XPathNavigator nav = dataDoc.CreateNavigator();
				// Transform the file.
                // EG 20160404 Migration vs2013
                //XslTransform xslt = new XslTransform();
                XslCompiledTransform xslt = new XslCompiledTransform();
				xslt.Load(XslFile);

				using (FileStream fs = new FileStream(this.Request.MapPath(HtmlFile), FileMode.Create))
				{
                    // EG 20160404 Migration vs2013
                    //xslt.Transform(nav, null, fs, null);
                    xslt.Transform(nav, null, fs);
				}
 				
				StringBuilder sb = new StringBuilder();
				sb.Append("\n<SCRIPT LANGUAGE='JAVASCRIPT' id='html'>\n");
				sb.Append("window.open(\""+HtmlFile+"\",\"htmlConfirmation\");");
				sb.Append("</SCRIPT>\n");
                // EG 20160404 Migration vs2013
                //this.Page.RegisterClientScriptBlock("html", sb.ToString());
                this.Page.ClientScript.RegisterClientScriptBlock(this.Page.GetType(), "html", sb.ToString());

			}
			catch
			{ 
				throw;
			}	
			finally
			{
				if (null != sXMLTrade)
					sXMLTrade.Close();
			}
		}

		private void BtnPDF_Click(object sender, System.EventArgs e)
		{
			try
			{
				string tradeIdentifier = txtTradeIdentifier.Text;
				string XslFile  = this.Request.MapPath(@"XML_Files/"+txtlXSLFile.Text);
				string PDFFile = "IRD_HTML_OutPut.pdf";

				string source = SessionTools.CS;
				string SQLQuery = "select IDENTIFIER,DEFINITION,FPML from dbo.TRADE where IDENTIFIER=" + tradeIdentifier;
				DataSet ds = DataHelper.ExecuteDataset(source, CommandType.Text, SQLQuery);
				DataTable dt = ds.Tables[0];
				//GlopPL verrue Replace() à revoir
				StringReader sXMLTrade = new StringReader(dt.Rows[0]["FPML"].ToString().Replace("xmlns=","xmln="));
			
				XslFile = XslFile.Replace("HTML","PDF"); //GlopPL
				//Transformer t = new Transformer(sXMLTrade, XslFile, Server.MapPath(PDFFile));
 			
				StringBuilder sb = new StringBuilder();
				sb.Append("\n<SCRIPT LANGUAGE='JAVASCRIPT' id='PDF'>\n");
				sb.Append("window.open(\""+PDFFile+"\",\"PDFConfirmation\");");
				sb.Append("</SCRIPT>\n");
                // EG 20160404 Migration vs2013
                //this.Page.RegisterClientScriptBlock("html", sb.ToString());
                this.Page.Page.ClientScript.RegisterClientScriptBlock(this.Page.GetType(), "html", sb.ToString());

			}
			catch
			{ 
				throw;
			}
		}
	}
}
