using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Web;
using System.Web.SessionState;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using System.IO;
using EFS.ACommon;
using EFS.Common.Web;

namespace OTC
{
    /// <summary>
    /// Description résumée de Pascal_Color.
    /// </summary>
    public partial class Pascal_Colorv2 : PageBase
    {

        protected void Page_Load(object sender, System.EventArgs e)
        {
            // Placer ici le code utilisateur pour initialiser la page
        }

        #region Code généré par le Concepteur Web Form
        override protected void OnInit(EventArgs e)
        {
            //
            // CODEGEN : Cet appel est requis par le Concepteur Web Form ASP.NET.
            //
            this.AbortRessource = true;
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

        protected void ButProcess_Click(object sender, EventArgs e)
        {
            string r_line, w_line, grayColor, targetColor, result;
            TextBox txtGray, txtTarget;
            string includesPath = SessionTools.AppInstance.MapPath(@"..\Includes") + @"\";
            string color = null;
            string gray_filename = includesPath + "EFSTheme-vlight.min.css";
            //
            result = Cst.CrLf2;
            for (int css = 1; css <= 11; css++)
            {
                switch (css)
                {
                    case 1:
                        color = "Green";
                        break;
                    case 2:
                        color = "Red";
                        break;
                    case 3:
                        color = "Brown";
                        break;
                    case 4:
                        color = "Blue";
                        break;
                    case 5:
                        color = "Cyan";
                        break;
                    case 6:
                        color = "Olive";
                        break;
                    case 7:
                        color = "Orange";
                        break;
                    case 8:
                        color = "Violet";
                        break;
                    case 9:
                        color = "Kaki";
                        break;
                    case 10:
                        color = "Rose";
                        break;
                    case 11:
                        color = "Yellow";
                        break;
                }
                string target_filename = "EFSTheme" + color + ".css";
                result += target_filename;
                target_filename = includesPath + target_filename;

                StreamReader r = File.OpenText(gray_filename);
                try
                {
                    StreamWriter w = File.CreateText(target_filename);
                    while ((r_line = r.ReadLine()) != null)
                    {
                        w_line = r_line;
                        for (int i = 1; i <= 9; i++)
                        {
                            //***********************************************************
                            //Color
                            //***********************************************************
                            txtGray = (TextBox)this.FindControl("txtGray" + i.ToString());
                            grayColor = ColorTranslator.ToHtml(Color.FromArgb(txtGray.BackColor.ToArgb()));
                            txtTarget = (TextBox)this.FindControl("txt" + color + i.ToString());
                            targetColor = ColorTranslator.ToHtml(Color.FromArgb(txtTarget.BackColor.ToArgb()));

                            w_line = w_line.Replace(grayColor, targetColor);
                            w_line = w_line.Replace(grayColor.ToUpper(), targetColor);
                            w_line = w_line.Replace(grayColor.ToLower(), targetColor);
                            //***********************************************************
                            //Image Name
                            //***********************************************************
                            grayColor = @"/gray/";
                            //'/blue/',		'/green/', 		'/red/',		'/yellow/', '/blue2/', '/green2/', '/black2/'
                            switch (color)
                            {
                                case "Green":
                                    targetColor = @"/green/";
                                    break;
                                case "Red":
                                    targetColor = @"/red/";
                                    break;
                                case "Brown":
                                    targetColor = @"/brown/";
                                    break;
                                case "Blue":
                                    targetColor = @"/blue/";
                                    break;
                                case "Cyan":
                                    targetColor = @"/cyan/";
                                    break;
                                case "Olive":
                                    targetColor = @"/olive/";
                                    break;
                                case "Orange":
                                    targetColor = @"/orange/";
                                    break;
                                case "Violet":
                                    targetColor = @"/violet/";
                                    break;
                                case "Kaki":
                                    targetColor = @"/kaki/";
                                    break;
                                case "Rose":
                                    targetColor = @"/rose/";
                                    break;
                                case "Yellow":
                                    targetColor = @"/yellow/";
                                    break;
                                default:
                                    targetColor = @"/unknown/";
                                    break;
                            }
                            w_line = w_line.Replace(grayColor, targetColor);
                            w_line = w_line.Replace(grayColor.ToUpper(), targetColor);
                            w_line = w_line.Replace(grayColor.ToLower(), targetColor);

                            grayColor = @"Gray.";
                            //'Blue.',			'Green.', 		'Red.',			'Yellow.', 'Blue2.', 'Green2.', 'Black.'
                            switch (color)
                            {
                                case "Green":
                                case "Olive":
                                    targetColor = @"Green.";
                                    break;
                                case "Red":
                                    targetColor = @"Red.";
                                    break;
                                case "Rose":
                                    targetColor = @"Rose.";
                                    break;
                                case "Violet":
                                    targetColor = @"Violet.";
                                    break;
                                case "Brown":
                                    targetColor = @"Marron.";
                                    break;
                                case "Kaki":
                                case "Yellow":
                                    targetColor = @"Yellow.";
                                    break;
                                case "Orange":
                                    targetColor = @"Orange.";
                                    break;
                                case "Black":
                                    targetColor = @"Black.";
                                    break;
                                case "Blue":
                                    targetColor = @"Blue.";
                                    break;
                                case "Cyan":
                                    targetColor = @"Cyan.";
                                    break;
                                default:
                                    targetColor = @"Unknown.";
                                    break;
                            }
                            w_line = w_line.Replace(grayColor, targetColor);
                            w_line = w_line.Replace(grayColor.ToUpper(), targetColor);
                            w_line = w_line.Replace(grayColor.ToLower(), targetColor);
                            grayColor = @"Black_Over";
                            //'Blue_Over',		'Green_Over', 		'Red_Over',		'Yellow_Over', 'Blue2_Over', 'Green2_Over', 'Black_Over'
                            switch (color)
                            {
                                case "Green":
                                case "Olive":
                                    targetColor = @"Green_Over";
                                    break;
                                case "Red":
                                case "Violet":
                                case "Rose":
                                    targetColor = @"Red_Over";
                                    break;
                                case "Brown":
                                case "Kaki":
                                case "Yellow":
                                case "Orange":
                                    targetColor = @"Yellow_Over";
                                    break;
                                case "Black":
                                    targetColor = @"Black_Over";
                                    break;
                                case "Blue":
                                case "Cyan":
                                    targetColor = @"Blue_Over";
                                    break;
                                default:
                                    targetColor = @"Unknown_Over";
                                    break;
                            }
                            w_line = w_line.Replace(grayColor, targetColor);
                            w_line = w_line.Replace(grayColor.ToUpper(), targetColor);
                            w_line = w_line.Replace(grayColor.ToLower(), targetColor);
                        }
                        w.WriteLine(w_line);
                    }
                    r.Close();
                    w.Close();
                    //
                    result += " [Success]" + Cst.CrLf;
                }
                catch (Exception ex)
                {
                    result += " [ERROR:" + ex.Message + "]" + Cst.CrLf;
                }
            }
            JavaScript.AlertImmediate(this, "Feuilles CSS générées:" + result);
        }

    }
}
