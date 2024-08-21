using EFS.ACommon;
using EFS.Common.Web;
using System;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

#region Trial_CheckDisabled
public partial class Trial_CheckDisabled : System.Web.UI.Page
{
    protected override void OnInit(EventArgs e)
    {

        bool isReadOnly = true;
        bool isEnabled = true;
        bool enabledViewState = true ;

        Button but = new Button
        {
            ID = "but",
            Text = "post",
            EnableViewState = enabledViewState
        };

        WCHtmlCheckBox hck = new WCHtmlCheckBox(false, 0, isReadOnly)
        {
            ID = "hck",
            CssClass = EFSCssClass.CaptureCheck,
            Text = "hck",
            Enabled = isEnabled,
            Checked = true,
            EnableViewState = enabledViewState
        };

        WCCheckBox chk = new WCCheckBox(isReadOnly)
        {
            ID = "chk",
            CssClass = EFSCssClass.CaptureCheck,
            Text = "chk",
            Enabled = isEnabled,
            Checked = true,
            EnableViewState = enabledViewState
        };

        WCTextBox2 txt = new WCTextBox2(null, null)
        {
            ReadOnly = isReadOnly,
            AutoPostBack = true,
            ID = "txt",
            Enabled = isEnabled,
            EnableViewState = enabledViewState
        };

        form1.Controls.Add(but);
        form1.Controls.Add(new LiteralControl("</br>"));
        form1.Controls.Add(new LiteralControl("</br>"));
        form1.Controls.Add(hck);
        form1.Controls.Add(new LiteralControl("</br>"));
        form1.Controls.Add(new LiteralControl("</br>"));
        form1.Controls.Add(chk);
        form1.Controls.Add(new LiteralControl("</br>"));
        form1.Controls.Add(new LiteralControl("</br>"));
        form1.Controls.Add(txt);
        form1.Controls.Add(new LiteralControl("</br>"));
        form1.Controls.Add(new LiteralControl("</br>"));

        base.OnInit(e);
    }

    protected override void OnPreRender(EventArgs e)
    {
        if (false == IsPostBack)
        {
            WCCheckBox chk = form1.FindControl("chk") as WCCheckBox;
            chk.ForeColor = System.Drawing.Color.DarkBlue;
            //
            WCTextBox2 txt = form1.FindControl("txt") as WCTextBox2;
            txt.Text  = "voici du text";
        }
        base.OnPreRender(e);
    }

}
#endregion

#region class WCChekBox
/// <summary>
/// Obsolete use WCCheckBox2 a la place
/// </summary>
public class WCCheckBox : CheckBox
{
    #region Members
    private readonly bool _isReadOnly;
    #endregion

    #region WCCheckBox
    public WCCheckBox(bool pIsReadOnly)
    {
        _isReadOnly = pIsReadOnly;
    }
    #endregion

    #region protected override CreateChildControls
    protected override void CreateChildControls()
    {
        //
        //Attributes.Add("isReadOnly",_isReadOnly.ToString());
        if (_isReadOnly)
            Attributes.Add("readonly", "readonly");
        //
        base.CreateChildControls();
    }
    #endregion

    #region protected override Render
    protected override void Render(HtmlTextWriter writer)
    {
        Enabled = Enabled && (false == _isReadOnly);
        //
        base.Render(writer);
    }
    #endregion
    #region protected override LoadPostData
    protected override bool LoadPostData(string postDataKey, System.Collections.Specialized.NameValueCollection postCollection)
    {
        return base.LoadPostData(postDataKey, postCollection);
    }
    #endregion
    #region protected override LoadViewState
    protected override void LoadViewState(object savedState)
    {
        base.LoadViewState(savedState);
    }
    #endregion
}
#endregion class WCChekBox
#region Class WCHtmlCheckBox
/// <summary>
/// Obsolete use WCHtmlCheckBox2 a la place
/// </summary>
public class WCHtmlCheckBox : HtmlInputCheckBox
{
    #region Member
    private readonly Label _label;
    private readonly bool _isReadOnly;
    #endregion
    //
    #region Accessor
    public bool ExistLabel { get { return (null != _label); } }
    public override string ID
    {
        get
        {
            return base.ID;
        }
        set
        {
            base.ID = value;
            if (this.ExistLabel)
                _label.ID = value.Replace(Cst.HCK, Cst.LBL);
        }
    }

    public string Text
    {
        set { if (this.ExistLabel) _label.Text = value; }
        get { if (this.ExistLabel) return _label.Text; else	return string.Empty; }
    }

    public string CssClass
    {
        set { this.Attributes.Add("class", value); }
        get { return this.Attributes["class"].ToString(); }
    }

    public Boolean Enabled
    {
        set { this.Disabled = !value; }
        get { return (!this.Disabled); }
    }
    public string ToolTip
    {
        set { if (ExistLabel) _label.ToolTip = value; }
        get { if (ExistLabel) return (_label.ToolTip); else return string.Empty; }
    }

    #endregion
    //
    #region Constructor
    public WCHtmlCheckBox() : this(false, 0, false) { }
    public WCHtmlCheckBox(bool pWithLabel) : this(pWithLabel, 0, false) { }
    public WCHtmlCheckBox(bool pWithLabel, int pLblWidth, bool pIsReadOnly)
    {
        if (pWithLabel)
        {
            _label = new Label();
            if (0 != pLblWidth)
                _label.Width = Unit.Pixel(pLblWidth);
        }
        EnsureChildControls();
        //
        _isReadOnly = pIsReadOnly;
    }
    #endregion
    //	
    #region protected override CreateChildControls
    protected override void CreateChildControls()
    {
        if (this.ExistLabel)
        {
            _label.Text = this.Text + Cst.HTMLSpace;
            _label.Visible = true;
        }
        //
        //Attributes.Add("isReadOnly",_isReadOnly.ToString());
        if (_isReadOnly)
            Attributes.Add("readonly", "readonly");
        //
        base.CreateChildControls();
    }
    #endregion
    #region protected override Render
    protected override void Render(HtmlTextWriter writer)
    {
        if (this.ExistLabel)
            _label.RenderControl(writer);

        Enabled = Enabled && (false == _isReadOnly);
        //
        base.Render(writer);
    }
    #endregion
    #region protected override LoadPostData
    protected override bool LoadPostData(string postDataKey, System.Collections.Specialized.NameValueCollection postCollection)
    {
        return base.LoadPostData(postDataKey, postCollection);
    }
    #endregion
    #region protected override LoadViewState
    protected override void LoadViewState(object savedState)
    {
        base.LoadViewState(savedState);
    }
    #endregion
}
#endregion HtmlInputCheckBox

