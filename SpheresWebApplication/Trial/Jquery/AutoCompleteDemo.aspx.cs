using EFS.Common;
using EFS.Common.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Services;
using System.Web.UI;
using System.Web.UI.WebControls;



namespace EFS.Spheres.Trial.Jquery
{
    public partial class AutoCompleteDemo : PageBase
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        protected override void OnInit(EventArgs e)
        {
            AbortRessource = true;
            base.OnInit(e);
            
            GenerateHtmlForm();
            PageTools.BuildPage(this, Form, "eee", string.Empty, false, string.Empty);

            Label lbl = new Label
            {
                CssClass = EFSCssClass.Label,
                Text = "Saisir un language (java,c++ etc..)"
            };
            ;

            WCTextBox txtRef;
            txtRef = (WCTextBox)new WCTextBox("WCTextbox", string.Empty, @"([\w|\W]{1,64})", true, "dddd");
            txtRef.CssClass = EFSCssClass.Capture;
            txtRef.ID = "autocomplete";

            Style myStyle = new Style
            {
                BorderStyle = BorderStyle.Solid
            };
            txtRef.ApplyStyle(myStyle);

            Panel div = new Panel();
            div.Controls.Add(lbl);
            div.Controls.Add(txtRef);
            div.BackColor = System.Drawing.Color.Aqua;

            CellForm.Controls.Add(new LiteralControl("<h1> Demo Jquery AutoComplete</h1>")); 

            CellForm.Controls.Add(div);
        }
        protected override void CreateChildControls()
        {
            JavaScript.JSStringBuilder builder = new JavaScript.JSStringBuilder();

            //string script = @"$(""#autocomplete"" ).autocomplete({source: [""c++"", ""java"", ""php"", ""coldfusion""]});";

            string script = @"
   $(document).ready(function() {
            $(""#autocomplete"").autocomplete({
                source: function(request, response) {
                    $.ajax({
                        url: ""AutoCompleteDemo.aspx/LodaData"",
                        data: ""{ 'identifier': '"" + request.term + ""' }"",
                        dataType: ""json"",
                        type: ""POST"",
                        contentType: ""application/json; charset=utf-8"",
                        dataFilter: function(data) { return data; },
                        success: function(data) {
                        console.log(data) 
                        response($.map(data.d, function(item) {
                            return {
                            value: item.First
                                }
                        }))
                        },
                        error: function(XMLHttpRequest, textStatus, errorThrown) {
                        alert(textStatus);
                    }
                });
            },
                minLength: 1
            });
        });
";

            builder.Append(script);

            base.CreateChildControls();
            this.RegisterScript("autocomplete", builder.ToString(), true);
        }


        /// <summary>
        /// Retourne une liste contenant les Deriv. Contracts dont l'Identifier débute par le paramètre "identifier"
        /// Nécessaire à l'autocomplete sur la TextBox txt_DcIdentifier
        /// </summary>
        [WebMethod]
        public static List<Pair<string, string>> LodaData(string identifier)
        {
            List<Pair<string, string>> lst = new List<Pair<string, string>>
            {
                new Pair<string, string>("c++", "the best c++"),
                new Pair<string, string>("java", "the best java"),
                new Pair<string, string>("php", "the best php"),
                new Pair<string, string>("coldfusion", "the best coldfusion")
            };

            List<Pair<string, string>> lstSearch = new List<Pair<string, string>>();
            lstSearch = (from item in lst.Where(y => y.First.StartsWith(identifier))
                       select item).ToList() ;

            return lstSearch;
        }
    }
}