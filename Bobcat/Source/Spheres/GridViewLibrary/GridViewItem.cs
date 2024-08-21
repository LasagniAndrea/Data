#region using directives
using EFS.ACommon;
using EFS.Common.Web;
using EFS.GridViewProcessor;
using System;
using System.Collections;
using System.Data;
using System.Reflection;
using System.Web.UI;
using System.Web.UI.WebControls;
#endregion using directives

public abstract class GridViewItemTemplate : ITemplate
{
    protected string id;

    public GridViewItemTemplate(string pID)
    {
        id = pID;
    }

    public virtual void InstantiateIn(Control container)
    {

    }
}

#region GridViewItemTemplateLabel
public class GridViewItemTemplateLabel : GridViewItemTemplate
{
    protected string textAlign;

    public GridViewItemTemplateLabel(string pID) : this(pID, "left")
    {
    }
    public GridViewItemTemplateLabel(string pID, string pTextAlign)
        : base(pID)
    {
        id = pID;
        textAlign = pTextAlign;
    }
    public override void InstantiateIn(Control pControlContainer)
    {
        Label lbl = new Label();
        lbl.ID = id;
        lbl.Style.Add(HtmlTextWriterStyle.TextAlign, textAlign);
        pControlContainer.Controls.Add(lbl);
    }
}
#endregion GridViewItemTemplateLabel
#region GridViewItemTemplateAggregate
public class GridViewItemTemplateAggregate : GridViewItemTemplateLabel
{
    public GridViewItemTemplateAggregate(string pID)
        : base(pID, "right")
    {
    }
    public override void InstantiateIn(Control pControlContainer)
    {
        // Aggregate Label
        Label lblAgr = new Label();
        lblAgr.ID = id + RepositoryTools.SuffixAggregate;
        lblAgr.Style.Add(HtmlTextWriterStyle.TextAlign, "left");
        lblAgr.Style.Add(HtmlTextWriterStyle.FontWeight, "normal");
        pControlContainer.Controls.Add(lblAgr);

        base.InstantiateIn(pControlContainer);
    }
}
#endregion GridViewItemTemplateAggregate

#region GridViewTemplateCheckBox
public class GridViewItemTemplateCheckBox : GridViewItemTemplate
{
    public string dataField;
    public bool readOnly;
    public EventHandler CheckedChanged;
    private ArrayList attrKey;
    private ArrayList attrValue;
    private string toolTip;

    public GridViewItemTemplateCheckBox(string pID, bool pDefaultValue, bool pEditable, string pToolTip)
        : this(pID, string.Empty, pDefaultValue, pEditable, pToolTip) { }
    public GridViewItemTemplateCheckBox(string pID, string pDataField, bool pEditable, string pToolTip)
        : this(pID, pDataField, false, pEditable, pToolTip) { }
    public GridViewItemTemplateCheckBox(string pID, string pDataField, bool pDefaultValue, bool pEditable, string pToolTip)
        : base(pID)
    {
        dataField = pDataField;
        readOnly = (pEditable == false);
        attrKey = new ArrayList();
        attrValue = new ArrayList();
        toolTip = pToolTip;
    }
    public override void InstantiateIn(Control pControlContainer)
    {
        WCCheckBox2 checkBox = new WCCheckBox2();
        checkBox.ID = id;
        checkBox.isReadOnly = readOnly;
        checkBox.ToolTip = toolTip;
        checkBox.AutoPostBack = false;
        checkBox.CheckedChanged += CheckedChanged;

        if (ArrFunc.Count(attrKey) > 0)
        {
            for (int i = 0; i < ArrFunc.Count(attrKey); i++)
                checkBox.Attributes.Add(attrKey[i].ToString(), attrValue[i].ToString());
        }

        if (StrFunc.IsFilled(dataField))
            checkBox.DataBinding += new EventHandler(this.BindData);
        pControlContainer.Controls.Add(checkBox);
    }
    public void BindData(object sender, EventArgs e)
    {
        WCCheckBox2 checkBox = (WCCheckBox2)sender;
        GridViewRow row = (GridViewRow)checkBox.NamingContainer;
        DataRowView dv = (DataRowView)row.DataItem;
        if (null != dv && StrFunc.IsFilled(dataField))
        {
            string data = dv[dataField].ToString();
            checkBox.Checked = BoolFunc.IsTrue(data);
        }
    }
    public void AddAttribute(string pKey, string pValue)
    {
        attrKey.Add(pKey);
        attrValue.Add(pValue);
    }
}
#endregion GridViewTemplateCheckBox

#region GridViewEditItemTemplate
public class GridViewEditItemTemplate : ITemplate
{
    #region Members
    private Control ControlMain;
    private Control[] OthersControls;
    private Control[] InformationControls;
    #endregion Members

    #region Accessors
    public Control CloneControlMain
    {
        get 
        {
            if (null != ControlMain)
            {
                Type t = ControlMain.GetType();
                PropertyInfo[] properties = t.GetProperties();
                object clone = t.InvokeMember("", BindingFlags.CreateInstance, null, ControlMain, null);
                foreach (PropertyInfo pi in properties)
                {
                    if (pi.CanWrite)
                        pi.SetValue(clone, pi.GetValue(ControlMain, null), null);
                }
                return (Control)clone;
            }
            return null;
        }
    }
    #endregion Accessors

    #region constructor
    public GridViewEditItemTemplate(ReferentialColumn pRc, bool pIsRelation)
    {
        if (pRc.HtmlControlMainSpecified)
            ControlMain = pRc.HtmlControlMain;
        else
            ControlMain = pRc.ControlMain;

        OthersControls = pRc.OtherGridControls;
        InformationControls = pRc.InformationControls;

        if (null != ControlMain)
        {
            string editItemTemplateID = (pIsRelation ? pRc.IDForItemTemplateRelation : pRc.IDForItemTemplate);
            ControlMain.ID = editItemTemplateID + RepositoryTools.SuffixEdit;
        }
    }
    #endregion constructor

    #region InstantiateIn
    public void InstantiateIn(Control pControlContainer)
    {
        pControlContainer.Controls.Add(ControlMain);
        if (ArrFunc.IsFilled(OthersControls))
        {
            for (int i = 0; i < OthersControls.Length; i++)
                pControlContainer.Controls.Add(OthersControls[i]);
        }
        if (ArrFunc.IsFilled(InformationControls))
        {
            WebControl webCtrl = ControlMain as WebControl;
            Label lbl = InformationControls[1] as Label;
            if ((null != webCtrl) && (null != lbl))
                webCtrl.ToolTip = lbl.Text;
        }
    }
    #endregion
}
#endregion GridViewEditItemTemplate
