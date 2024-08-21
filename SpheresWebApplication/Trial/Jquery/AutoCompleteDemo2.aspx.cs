using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.Services;
using System.Web.UI.WebControls;

namespace EFS.Spheres.Trial.Jquery
{
    public partial class AutoCompleteDemo2 : System.Web.UI.Page
    {
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            System.Diagnostics.Debug.WriteLine(TXT_autocomplete.Text);
        }


        /// <summary>
        /// Retourne une liste contenant les Deriv. Contracts dont l'Identifier débute par le paramètre "identifier"
        /// Nécessaire à l'autocomplete sur la TextBox txt_DcIdentifier
        /// </summary>
        [WebMethod]
        public static List<LodaDataItem> LodaData(string identifier)
        {
            List<LodaDataItem> lst = new List<LodaDataItem>
            {
                new LodaDataItem()
                {
                    id = "1",
                    @value = "c++",
                    label = "the best c++"
                },

                new LodaDataItem()
                {
                    id = "2",
                    @value = "java",
                    label = "the best java"
                },

                new LodaDataItem()
                {
                    id = "3",
                    @value = "php",
                    label = "the best php"
                },

                new LodaDataItem()
                {
                    id = "4",
                    @value = "coldfusion",
                    label = "the best coldfusion"
                }
            };


            List<LodaDataItem> lstSearch = new List<LodaDataItem>();
            lstSearch = (from item in lst.Where(y => y.@value.StartsWith(identifier))
                         select item).ToList();

            return lstSearch;
        }

    }

    public class LodaDataItem
    {
        public string id;
        public string @value;
        public string label;
    }

}