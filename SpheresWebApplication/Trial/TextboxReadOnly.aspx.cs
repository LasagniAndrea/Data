using System;
using System.Collections;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Xml.Linq;

using EFS.Common.Web;   

public partial class Trial_TextBoxReadOnly : PageBase 
{

    protected override void OnInit(EventArgs e)
    {
        AbortRessource = true; 
        base.OnInit(e);
        //
        GenerateHtmlForm();
        PageTools.BuildPage(this, Form, "eee", string.Empty, false, string.Empty);
        //
        WCTextBox txtRef = new WCTextBox("WCTextbox", string.Empty, @"([\w|\W]{1,64})", true, "dddd")
        {
            ReadOnly = false, //=> La valeur de .text du contrôle est stockée dans le viewstate
            Enabled = false //=> La valeur de .text du contrôle est stockée dans le viewstate
        };

        TextBox txtRef2 = new TextBox
        {
            ID = "Textbox",
            ReadOnly = txtRef.ReadOnly,
            Enabled = txtRef.Enabled
        };


        //Nok => on perd la valeur text après un postback (viewState non alimenté)
        if (false == IsPostBack)
        {
            txtRef.Text = "this is a text";
            txtRef2.Text = txtRef.Text;
        }
        
        CellForm.Controls.Add(txtRef);
        CellForm.Controls.Add(txtRef2);
        //
        //ok => on ne perd pas la valeur text après un postback
        //ok car l'appel à InitializeText est effectuer après création du contrôle 
        //=> Asp alimente le viewstate si des modifs sont opérés (ce qui est le cas)
        //InitializeText();
        //
        Button but = new Button
        {
            ID = "BUT"
        };
        CellForm.Controls.Add(but);  
        //
    }
 
    private void InitializeText()
    {
        if (false == IsPostBack)
        {
            TextBox txt = FindControl("Textbox") as TextBox;
            txt.Text = "this is a text";
            txt = (TextBox)FindControl("WCTextbox");
            txt.Text = "this is a text";
        }
    }
}
