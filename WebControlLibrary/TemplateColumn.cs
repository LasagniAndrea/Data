using System;
using System.Collections;
using System.Collections.Specialized;
using System.Data;

using System.Drawing;
using System.Reflection;

using System.Web.SessionState;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.ComponentModel;
using System.Diagnostics;
using System.Text;
using System.Xml;

using EFS.ACommon;
using EFS.Common;
using EFS.Common.Web;
using EFS.Referential;

using EFS.Common.MQueue;
using EFS.ApplicationBlocks.Data;

#region class CheckBoxItem_Referencial
public class CheckBoxItem_Referencial : ITemplate
{
    public string DataField;
    public bool ReadOnly;
    public EventHandler CheckedChanged;
    
    private readonly ArrayList attributeKey;
    private readonly ArrayList attributeValue;
    private readonly string checkBoxID;
    private readonly string checkBoxToolTip;
    
    public CheckBoxItem_Referencial(string pCheckBoxID, bool pEditable, string pCheckBoxToolTip)
        : this(pCheckBoxID, string.Empty, pEditable, pCheckBoxToolTip) { }
    public CheckBoxItem_Referencial(string pCheckBoxID, string pDataField, bool pEditable, string pCheckBoxToolTip)
    {
        DataField = pDataField;
        ReadOnly = !pEditable;
        
        attributeKey = new ArrayList();
        attributeValue = new ArrayList();
        checkBoxID = pCheckBoxID;
        checkBoxToolTip = pCheckBoxToolTip;
    }
    public void InstantiateIn(Control pControlContainer)
    {
        WCCheckBox2 checkBox;

        //PL 20180812 [24252]  +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-
        if (string.IsNullOrEmpty(DataField))
        {
            #region Création de la CheckBox de gauche (...OnAllPages) du header du DataGrid
            checkBox = new WCCheckBox2
            {
                ID = checkBoxID.Replace("OnCurrentPage", "OnAllPages"),
                IsReadOnly = ReadOnly,
                ToolTip = Ressource.GetString("SelectAllOnAllPages"),
                //########################################################################
                //PL 20190104 [24420/24421] Implémentation d'un AutoPostBack 
                //checkBox.AutoPostBack = false;
                //checkBox.Attributes.Add("onclick", "DataGridSelectAll(this, '" + "efsdtgRefeferentiel" + "', '" + "efsdtgRefeferentiel_cbSelect" + "');return true;");
                AutoPostBack = true
            };
            //########################################################################
            checkBox.CheckedChanged += CheckedChanged;

            pControlContainer.Controls.Add(checkBox);
            #endregion
        }
        //PL 20180812 [24252]  +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-

        #region Création de la CheckBox de droite (...OnCurrentPage) ou de l'unique CheckBox du header du DataGrid et des CheckBox de chaque ligne du DataGrid
        checkBox = new WCCheckBox2
        {
            ID = checkBoxID,
            //FI 20091106 [16722] checkBox.Enabled est valorisée avec ReadOnly
            IsReadOnly = ReadOnly,
            ToolTip = checkBoxToolTip,
            AutoPostBack = false
        };
        checkBox.CheckedChanged += CheckedChanged;
        if (ArrFunc.Count(attributeKey) > 0)
        {
            for (int i = 0; i < ArrFunc.Count(attributeKey); i++)
                checkBox.Attributes.Add(attributeKey[i].ToString(), attributeValue[i].ToString());
        }
        if (!string.IsNullOrEmpty(DataField))
        {
            checkBox.DataBinding += new EventHandler(this.BindData);
        }

        pControlContainer.Controls.Add(checkBox);
        #endregion
    }

    public void BindData(object sender, EventArgs e)
    {
        WCCheckBox2 checkBox = (WCCheckBox2)sender;
        DataGridItem container = (DataGridItem)checkBox.NamingContainer;
        DataRowView dv = (DataRowView)container.DataItem;
        if (null != dv && StrFunc.IsFilled(DataField))
        {
            string data = dv[DataField].ToString();
            checkBox.Checked = BoolFunc.IsTrue(data);
        }
    }
    public void AddAttribute(string pKey, string pValue)
    {
        attributeKey.Add(pKey);
        attributeValue.Add(pValue);
    }
}
#endregion class CheckBoxItem_Referencial
//
#region class ItemTemplate_Referencial
public class ItemTemplate_Referencial : ITemplate
{
    private readonly string ItemTemplateID;
    public ItemTemplate_Referencial(string pID)
    {
        ItemTemplateID = pID;
    }
    public void InstantiateIn(Control pControlContainer)
    {

        Label lbl = new Label
        {
            ID = ItemTemplateID
        };
        pControlContainer.Controls.Add(lbl);

    }
}

#endregion class ItemTemplate_Referencial
//
#region class ItemTemplateLabel_Referencial
public class ItemTemplateLabel_Referencial : ITemplate
{
    private readonly string ItemTemplateID;
    public ItemTemplateLabel_Referencial(string pID)
    {
        ItemTemplateID = pID;
    }
    public void InstantiateIn(Control pControlContainer)
    {
        // Aggregate Label
        Label labelAggregateLabel = new Label
        {
            ID = ItemTemplateID + ReferentialTools.SuffixAggregate
        };
        labelAggregateLabel.Style.Add(HtmlTextWriterStyle.TextAlign, "left");
        labelAggregateLabel.Style.Add(HtmlTextWriterStyle.FontWeight, "normal");
        pControlContainer.Controls.Add(labelAggregateLabel);
        //            
        Label lbl = new Label
        {
            ID = ItemTemplateID
        };
        lbl.Style.Add(HtmlTextWriterStyle.TextAlign, "right");
        pControlContainer.Controls.Add(lbl);
    }
}

#endregion class ItemTemplate_Referencial
//
#region class EditItemTemplate_Referencial
public class EditItemTemplate_Referencial : ITemplate
{
    #region Members
    private readonly Control ControlMain;
    private readonly Control[] OthersControls;
    private readonly Control[] InformationControls;
    #endregion Members

    #region Accessors
    public Control CloneControlMain
    {
        get 
        {
            Type t = ControlMain.GetType();
            PropertyInfo[] properties = t.GetProperties();
            object clone = t.InvokeMember("", BindingFlags.CreateInstance, null, ControlMain, null);
            foreach (PropertyInfo pi in properties)
            {
                if (pi.CanWrite)
                    pi.SetValue(clone, pi.GetValue(ControlMain, null), null);
            }
            return (Control) clone;
        }
    }
    #endregion Accessors

    #region constructor
    public EditItemTemplate_Referencial(ReferentialsReferentialColumn pRrc, bool pIsRelation)
    {
        if (pRrc.HtmlControlMainSpecified)
            ControlMain = pRrc.HtmlControlMain;
        else
            ControlMain = pRrc.ControlMain;

        // FI 20190318 [24568][24588] Message "ControlMain is null" plutôt que tomber sur nullreference exception
        if (null == ControlMain)
            throw new NullReferenceException("ControlMain is null");

        string editItemTemplateID = (pIsRelation ? pRrc.IDForItemTemplateRelation : pRrc.IDForItemTemplate);
        ControlMain.ID = editItemTemplateID + ReferentialTools.SuffixEdit;

        OthersControls = pRrc.OtherGridControls;
        InformationControls = pRrc.InformationControls;

    }
    #endregion constructor

    #region public InstantiateIn
    public void InstantiateIn(Control pControlContainer)
    {
        pControlContainer.Controls.Add(ControlMain);
        //
        if (ArrFunc.IsFilled(OthersControls))
        {
            for (int i = 0; i < OthersControls.Length; i++)
                pControlContainer.Controls.Add(OthersControls[i]);
        }
        //
        if (ArrFunc.IsFilled(InformationControls))
        {
            if ((ControlMain is WebControl webCtrl) && (InformationControls[1] is Label lbl))
                webCtrl.ToolTip = lbl.Text;
        }
    }
    #endregion
}
#endregion class EditItemTemplate_Referencial
