using System;
using System.Collections; 
using System.Xml;
using System.IO;
using System.Text;
using System.Threading;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml.XPath;
using System.Xml.Xsl;
using Microsoft.XmlDiffPatch;

using EFS.ACommon;
using EFS.Common;
using EFS.Common.Web;


namespace EFS
{
    public enum XMLDiffLegendEnum
    {
        Added,
        Changed,
        Ignored,
        MovedFrom,
        MovedTo,
        Removed,
    }
    public class XMLCompare
    {

        private readonly string m_Document1;
        private readonly bool m_Document1Specified;
        private readonly string m_Document2;
        private readonly bool m_Document2Specified;

        public bool isEquals = false;
        public string resultCompare;
        public string resultDisplay;
        public string resultTransform;


        public XMLCompare() { }
        public XMLCompare(string pDocument1, string pDocument2)
        {
            m_Document1 = pDocument1;
            m_Document1Specified = StrFunc.IsFilled(m_Document1);
            m_Document2 = pDocument2;
            m_Document2Specified = StrFunc.IsFilled(m_Document2);
        }

        public XMLCompare(string pDocument1, string pDocument2, ref Cst.ErrLevel pRet)
            : this(pDocument1, pDocument2)
        {
            Cst.ErrLevel ret = Compare();
            if (Cst.ErrLevel.SUCCESS == ret)
            {
                if (false == isEquals)
                {
                    ret = Display();
                    if (Cst.ErrLevel.SUCCESS == ret)
                        Transform();
                }
            }
            pRet = ret;
        }
        #region Compare
        // EG 20180423 Analyse du code Correction [CA2200]
        public Cst.ErrLevel Compare()
        {
            isEquals = false;
            Cst.ErrLevel ret = Cst.ErrLevel.ABORTED;
            try
            {
                if (m_Document1Specified && m_Document2Specified)
                {
                    //Create XmlDiff object for document comparison
                    XmlDiff diff = new XmlDiff
                    {
                        //Set comparisons that should be ignored
                        IgnoreComments = true,
                        IgnorePI = true,
                        IgnoreWhitespace = true,

                        //Choose most precise algorithm
                        //For large documents look at using the "Fast" algorithm
                        Algorithm = XmlDiffAlgorithm.Precise
                    };

                    //Compare documents and generate XDL diff document
                    StringWriter sw = new StringWriter();
                    XmlTextWriter writer = new XmlTextWriter(sw)
                    {
                        Formatting = Formatting.Indented
                    };

                    XmlTextReader reader1 = new XmlTextReader(new StringReader(m_Document1));
                    XmlTextReader reader2 = new XmlTextReader(new StringReader(m_Document2));
                    isEquals = diff.Compare(reader1, reader2, writer);
                    if (isEquals)
                        resultCompare = "No differences were found between the documents";
                    else
                        resultCompare = sw.ToString();
                    writer.Close();
                    reader1.Close();
                    reader2.Close();
                    ret = Cst.ErrLevel.SUCCESS;
                }
            }
            catch (Exception)
            {
                throw;
            }
            return ret;
        }
        #endregion Compare
        #region Display
        // EG 20180423 Analyse du code Correction [CA2200]
        public Cst.ErrLevel Display()
        {
            Cst.ErrLevel ret;
            try
            {
                if ((false == isEquals) && StrFunc.IsFilled(resultCompare) && m_Document1Specified)
                {
                    string displayResult = string.Empty;
                    XmlTextReader reader1 = new XmlTextReader(new StringReader(m_Document1));
                    XmlTextReader reader2 = new XmlTextReader(new StringReader(resultCompare));

                    XmlDiffView diffView = new XmlDiffView();
                    diffView.Load(reader1, reader2);

                    StringWriter sw = new StringWriter();
                    XmlTextWriter writer = new XmlTextWriter(sw)
                    {
                        Formatting = Formatting.Indented
                    };

                    sw.Write("<table>");
                    diffView.GetHtml(sw);
                    sw.Write("</table>");
                    sw.Close();
                    reader1.Close();
                    reader2.Close();
                    resultDisplay = sw.ToString();
                    diffView = null;
                }
                ret = Cst.ErrLevel.SUCCESS;
            }
            catch (Exception)
            {
                throw;
            }
            return ret;
        }
        #endregion Display
        #region PatchXML
        public string PatchXML(string pOriginal, string pPatchXml)
        {
            StringWriter sw = new StringWriter();
            XmlTextWriter writer = new XmlTextWriter(sw)
            {
                Formatting = Formatting.Indented
            };

            XmlDocument originalDoc = new XmlDocument();
            originalDoc.LoadXml(pOriginal);

            //Create XmlPatch object to perform patch operation
            XmlPatch patch = new XmlPatch();
            XmlTextReader reader = new XmlTextReader(new StringReader(pPatchXml));

            //Perform patch operation
            patch.Patch(originalDoc, reader);
            originalDoc.Save(writer);
            reader.Close();
            return sw.ToString();
        }
        #endregion PatchXML
        #region Transform
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pPage"></param>
        /// <param name="pCssFileName"></param>
        /// FI 20160804 [Migration TFS] Modify
        public void Transform()
        {
            string document = resultDisplay.Replace("&nbsp;", " ");
            StringBuilder sb = new StringBuilder();
            sb.Append(document);

            //string xslCompare = pPage.Request.MapPath(@"XML_Files/XSL_Files/Compare/Compare.xslt");
            //string xslCompare = FileTools.GetFile2(SessionTools.CS, pPage.Request.MapPath("~"), "XSLT", "Compare", "Compare");
            //string xslCompare = SessionTools.NewAppInstance().SearchFile(SessionTools.CS, @"~\Spheres\XSL_Files\Compare\Compare.xslt");

            // FI 20160804 [Migration TFS]
            string xslCompare = string.Empty;
            SessionTools.AppSession.AppInstance.SearchFile2(SessionTools.CS, @".\XMLCompare\Compare.xslt", ref xslCompare);

            Hashtable param = new Hashtable
            {
                { "pCurrentCulture", Thread.CurrentThread.CurrentCulture.Name }
            };

            resultTransform = XSLTTools.TransformXml(sb, xslCompare, param, null);

        }
        #endregion Transform
        #region CreatePlaceHolder
        public PlaceHolder CreatePlaceHolder() { return CreatePlaceHolder(0, String.Empty, String.Empty, String.Empty); }
        public PlaceHolder CreatePlaceHolder(string pTitle) { return CreatePlaceHolder(0, pTitle, String.Empty, String.Empty); }
        public PlaceHolder CreatePlaceHolder(string pTitleLeft, string pTitleRight)
        {
            return CreatePlaceHolder(0, String.Empty, pTitleLeft, pTitleRight);
        }
        public PlaceHolder CreatePlaceHolder(int pHeight, string pTitle)
        {
            return CreatePlaceHolder(pHeight, pTitle, String.Empty, String.Empty);
        }
        public PlaceHolder CreatePlaceHolder(int pHeight, string pTitleLeft, string pTitleRight)
        {
            return CreatePlaceHolder(pHeight, String.Empty, pTitleLeft, pTitleRight);
        }
        // EG 20200729 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc) Correctifs et compléments (Track, Banner, Tracker ...)
        public PlaceHolder CreatePlaceHolder(int pHeight, string pTitle, string pTitleLeft, string pTitleRight)
        {
            PlaceHolder plh = new PlaceHolder
            {
                ID = "__plhCompare"
            };
            Panel pnl = new Panel();

            #region Xml Difference Title
            if (StrFunc.IsFilled(pTitle))
            {
                Label lbl = new Label
                {
                    Text = pTitle
                };
                pnl.Controls.Add(lbl);
                plh.Controls.Add(pnl);
            }
            #endregion Xml Difference Title
            #region XmlDifference Body
            pnl = new Panel();
            if (0 < pHeight)
                pnl.Height = Unit.Pixel(pHeight);
            pnl.CssClass = "tableHolder";
            LiteralControl litCompare = new LiteralControl();
            if (isEquals)
                litCompare.Text = resultCompare;
            else
            {
                #region Xml Difference Header
                string resultTransformFinal = resultTransform;
                if (StrFunc.IsFilled(pTitleLeft) && StrFunc.IsFilled(pTitleRight))
                {

                    int rowIndex = resultTransformFinal.IndexOf("<tr");
                    if (-1 < rowIndex)
                    {
                        string header = @"<tr class=""DataGridGray_HeaderStyle"">";
                        header += @"<td>Line</td>";
                        header += "<td>" + pTitleLeft + "</td>";
                        header += @"<td>" + pTitleRight + "</td>";
                        header += @"</tr>";
                        resultTransformFinal = resultTransformFinal.Insert(rowIndex, header);
                    }
                }
                litCompare.Text = resultTransformFinal;
                #endregion Xml Difference Header
            }
            pnl.Controls.Add(litCompare);
            plh.Controls.Add(pnl);
            #endregion XmlDifference Body
            #region XmlDifference Footer
            if (false == isEquals)
            {
                pnl = new Panel();
                pnl.Controls.Add(DisplayLegend());
                plh.Controls.Add(pnl);
            }
            #endregion XmlDifference Footer
            return plh;
        }
        #endregion CreatePlaceHolder

        #region DisplayLegend
        private static Table DisplayLegend()
        {
            StringWriter sw = new StringWriter();
            _ = new XmlTextWriter(sw)
            {
                Formatting = Formatting.Indented
            };

            Table table = new Table
            {
                Width = Unit.Percentage(100)
            };
            TableRow tr = new TableRow();
            TableCell td = new TableCell
            {
                VerticalAlign = VerticalAlign.Middle,
                HorizontalAlign = HorizontalAlign.Left,
                Wrap = true
            };
            td.Font.Size = FontUnit.XXSmall;


            td.Controls.Add(AddResetCounter());
            td.Controls.Add(new LiteralControl(Cst.HTMLSpace));
            foreach (string xmlDiffLegend in Enum.GetNames(typeof(XMLDiffLegendEnum)))
                td.Controls.Add(AddLegend(xmlDiffLegend.ToString()));
            tr.Cells.Add(td);
            table.Rows.Add(tr);
            return table;
        }
        #endregion DisplayLegend

        // EG 20200729 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc) Correctifs et compléments (Track, Banner, Tracker ...)
        private static LinkButton AddResetCounter()
        {
            LinkButton btn = new LinkButton
            {
                ID = "Reset",
                Text = "<i class='fas fa-" +
                "eraser'></i>" + " Reset",
                CssClass = "fa-icon reset",
                CausesValidation = false,
                OnClientClick = "javascript:XMLDiff_ResetCounter();return false;"
            };
            return btn;
        }
        /// <summary>
        ///  Ajoute un Button 
        /// </summary>
        /// <param name="pName"></param>
        /// <returns></returns>
        // EG 20200729 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc) Correctifs et compléments (Track, Banner, Tracker ...)
        private static LinkButton AddLegend(string pName)
        {
            LinkButton btn = new LinkButton
            {
                ID = pName,
                Text = "<i class='fas fa-bookmark'></i>" + " " + pName,
                CssClass = "fa-icon " + pName,
                CausesValidation = false,
                OnClientClick = "javascript:XMLDiff_Anchor(this);return false;"
            };
            return btn;
        }
        // EG 20200729 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc) Correctifs et compléments (Track, Banner, Tracker ...)
        public static void RegisterXMLDiffAnchorFunction(Page pPage)
        {
            // EG 20160404 Migration vs2013
            //if (pPage.Request.Browser.JavaScript == true) 
            if (1 < pPage.Request.Browser.EcmaScriptVersion.Major)
            {
                pPage.ClientScript.RegisterHiddenField("__ANCHOR_COUNTER_ADDED", "0");
                pPage.ClientScript.RegisterHiddenField("__ANCHOR_COUNTER_CHANGED", "0");
                pPage.ClientScript.RegisterHiddenField("__ANCHOR_COUNTER_IGNORED", "0");
                pPage.ClientScript.RegisterHiddenField("__ANCHOR_COUNTER_MOVEDFROM", "0");
                pPage.ClientScript.RegisterHiddenField("__ANCHOR_COUNTER_MOVEDTO", "0");
                pPage.ClientScript.RegisterHiddenField("__ANCHOR_COUNTER_REMOVED", "0");

                string nameFunction = "XMLDiff_Anchor";
                // Create JavaScript
                StringBuilder sb = new StringBuilder();
                sb.Append("\n<script type='text/javascript'>\n");
                sb.Append("<!--\n");
                sb.Append("function " + nameFunction + "(elem)\n");
                sb.Append("{\n");
                sb.Append("var anchors = document.getElementsByName('_' + elem.id);\n");
                sb.Append("if (null != anchors)\n");
                sb.Append("{\n");
                sb.Append("   var txtAnchorCounter = document.getElementById('__ANCHOR_COUNTER_' + elem.id.toUpperCase());\n");
                sb.Append("   var currAnchor       = txtAnchorCounter.value;\n");
                sb.Append("   if (currAnchor >= anchors.length)\n");
                sb.Append("      currAnchor = 0;\n");
                sb.Append("   var anchor  = anchors[currAnchor];\n");
                sb.Append("   if (null != anchor)\n");
                sb.Append("   {\n");
                sb.Append("      location.href = '#' + anchor.id;\n");
                sb.Append("      currAnchor++;\n");
                sb.Append("   }\n");
                sb.Append("   txtAnchorCounter.value = currAnchor;\n");
                sb.Append("}\n");
                sb.Append("}\n");
                sb.Append("-->\n");
                sb.Append("</script>");

                // Register Client Script
                // EG 20160404 Migration vs2013
                //if (!pPage.IsStartupScriptRegistered(nameFunction))
                //    pPage.RegisterStartupScript(nameFunction, sb.ToString());
                Type csType = pPage.GetType();
                if (false == pPage.ClientScript.IsStartupScriptRegistered(csType, nameFunction))
                    pPage.ClientScript.RegisterStartupScript(csType, nameFunction, sb.ToString());

                nameFunction = "XMLDiff_ResetCounter";
                // Create JavaScript
                sb = new StringBuilder();
                sb.Append("\n<script type='text/javascript'>\n");
                sb.Append("<!--\n");
                sb.Append("function " + nameFunction + "()\n");
                sb.Append("{\n");
                sb.Append("   var txtAnchorCounter   = document.getElementById('__ANCHOR_COUNTER_ADDED');\n");
                sb.Append("   txtAnchorCounter.value = 0;\n");
                sb.Append("   txtAnchorCounter       = document.getElementById('__ANCHOR_COUNTER_CHANGED');\n");
                sb.Append("   txtAnchorCounter.value = 0;\n");
                sb.Append("   txtAnchorCounter       = document.getElementById('__ANCHOR_COUNTER_IGNORED');\n");
                sb.Append("   txtAnchorCounter.value = 0;\n");
                sb.Append("   txtAnchorCounter       = document.getElementById('__ANCHOR_COUNTER_MOVEDFROM');\n");
                sb.Append("   txtAnchorCounter.value = 0;\n");
                sb.Append("   txtAnchorCounter       = document.getElementById('__ANCHOR_COUNTER_MOVEDTO');\n");
                sb.Append("   txtAnchorCounter.value = 0;\n");
                sb.Append("   txtAnchorCounter       = document.getElementById('__ANCHOR_COUNTER_REMOVED');\n");
                sb.Append("   txtAnchorCounter.value = 0;\n");
                sb.Append("}\n");
                sb.Append("-->\n");
                sb.Append("</script>");

                // Register Client Script
                // EG 20160404 Migration vs2013
                //if (!pPage.IsStartupScriptRegistered(nameFunction))
                //    pPage.RegisterStartupScript(nameFunction, sb.ToString());
                if (false == pPage.ClientScript.IsStartupScriptRegistered(csType, nameFunction))
                    pPage.ClientScript.RegisterStartupScript(csType, nameFunction, sb.ToString());

            }
        }
    }
}