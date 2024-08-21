using EFS.ACommon;
using EFS.ApplicationBlocks.Data;
using EFS.Common.Web;
using EFS.Restriction;
using System;
using System.Collections;
using System.Data;
using System.Drawing;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;


namespace EFS.Spheres
{
    #region SelCommonProduct
    public partial class SelCommonProduct
    {
        #region Members
        private readonly string m_PrefixControl = string.Empty;
        private int m_NumberOfSections = 0;
        private bool m_IsBindParents = false;
        private bool m_IsDescChecked = true;
        private readonly bool m_IsAllItemIncluded = false;
        private DataSet m_DsProduct;
        private string[] m_ElementValue;        // Holds value of drop down list
        private string[] m_HiddenElementValue;  // Holds values to be saved in cookie. Needed for Product and Instrument
        private readonly Cst.SQLCookieGrpElement m_GrpElement;
        private readonly PageBase m_CurrentPage;
        private Hashtable m_ClassList;
        private readonly string m_SessionId;
        private readonly bool m_isInstrumentEnabled;
        private readonly bool m_isBindDDL;
        #endregion Members

        #region Accessors
        public bool IsDescChecked
        {
            get { return m_IsDescChecked; }
            set { m_IsDescChecked = value; }
        }
        public int NumberOfSections
        {
            get { return m_NumberOfSections; }
            set { m_NumberOfSections = value; }
        }
        #endregion accessors

        #region Constructors
        public SelCommonProduct(PageBase pPage, Cst.SQLCookieGrpElement pGrpElement, string pPrefixControl, string pSessionId)
            : this(pPage, pGrpElement, pPrefixControl, pSessionId, false, Cst.Capture.ModeEnum.New, false) { }

        public SelCommonProduct(PageBase pPage, Cst.SQLCookieGrpElement pGrpElement, string pPrefixControl, string pSessionId, bool pIsAllItemIncluded, Cst.Capture.ModeEnum pModeEnum, bool pIsBindDDL)
        {
            m_SessionId = pSessionId;
            m_CurrentPage = pPage;
            m_IsAllItemIncluded = pIsAllItemIncluded;
            m_GrpElement = pGrpElement;
            m_PrefixControl = pPrefixControl;
            m_isInstrumentEnabled = Cst.Capture.IsModeNewOrDuplicateOrReflect(pModeEnum);
            m_isBindDDL = pIsBindDDL;
        }
        #endregion Constructors

        #region Events
        #region OnSelectClick
        // EG 20200825 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        public void OnSelectClick(object sender, EventArgs e)
        {
            // EG 20091110
            SaveLastProduct();
            
            m_CurrentPage.Session["TemplateInfo"] = m_HiddenElementValue;

            //20090720 PL Add if() pour éviter error JS
            if ((null != sender) && sender.GetType().Equals(typeof(WCToolTipLinkButton)))
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("\n<SCRIPT LANGUAGE='JAVASCRIPT' id='SetProductToOpener'>\n");
                sb.Append("window.opener.SetProduct();");
                sb.Append("self.close();");
                sb.Append("</SCRIPT>\n");
                //20071212 FI Ticket 16012 => Migration Asp2.0
                if (!m_CurrentPage.ClientScript.IsClientScriptBlockRegistered("SetProductToOpener"))
                    m_CurrentPage.ClientScript.RegisterClientScriptBlock(GetType(), "SetProductToOpener", sb.ToString());
            }
        }
        #endregion OnSelectClick
        #region OnSelectedElementChanged
        public void OnSelectedElementChanged(object sender, EventArgs e)
        {

            string id = ((DropDownList)sender).ID.Replace("lst", string.Empty);
            Cst.EnumElement enumElement = (Cst.EnumElement)Enum.Parse(typeof(Cst.EnumElement), id);
            //
            DisplayInfo(enumElement);
            //
            if (enumElement != Cst.EnumElement.Screen)
            {
                string product = string.Empty;
                Control ctrlFound = m_CurrentPage.FindControl(m_PrefixControl + "lst" + Cst.EnumElement.Product.ToString());
                if (ctrlFound != null)
                    product = ((DropDownList)ctrlFound).SelectedValue;
                string instrt = string.Empty;
                ctrlFound = m_CurrentPage.FindControl(m_PrefixControl + "lst" + Cst.EnumElement.Instrument.ToString());
                if (ctrlFound != null)
                    instrt = ((DropDownList)ctrlFound).SelectedValue;

                // isBindParents is used in SetDataViewFilter only when Tous Produits and Tous Instruments are chosen 
                //   and a change from the Template DropDown is the origin of the execution
                // isBindParents allows the product and instrument to stay at Tous Produits and Tous instruments
                // It also allows the values of product and instrument to be saved to a temporary cookie.
                if ((enumElement == Cst.EnumElement.Template) && (StrFunc.IsEmpty(product) && StrFunc.IsEmpty(instrt)))
                {
                    // Cas particulier d'une sélection d'un Template avec "Tous Produits" & "Tous instruments" --> Load des screens à l'aide de l'IDI du template
                    // ici, on a changé le dropdown template et le produit et l'instrument est vide, je charge les écrans pour ce template.
                    // Anciennement, ça alignait Tous Produits et Tous Instruments en fonction du template chosi. 
                    m_IsBindParents = true;
                    BindChildren(Cst.EnumElement.Product, Cst.EnumElement.Product + 1);
                }
                else
                {
                    // en fonction du dropdown qui a changé, il faut mettre à jour les autres dropdowns 
                    m_IsBindParents = false;
                    BindChildren(enumElement + 1, GetParents(enumElement + 1));
                }
            }
            //
            m_CurrentPage.Session["TemplateInfo"] = m_HiddenElementValue;

        }
        #endregion OnSelectedElementChanged
        #endregion Events
        
        #region Methods
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pElement"></param>
        public void Bind(Cst.EnumElement pElement)
        {
            m_DsProduct = (DataSet)LoadListProductInstrumentTemplate();
            //
            GetLastProduct();
            CheckBox chk = m_CurrentPage.FindControl("chkDescription") as CheckBox;
            //
            if ((false == m_CurrentPage.IsPostBack) || m_isBindDDL)
            {
                m_HiddenElementValue = m_ElementValue;
                //
                BindChildren(pElement);
                if (null != chk)
                    chk.Checked = m_IsDescChecked;
            }
            else
            {
                RestoreColor(pElement);
                //
                m_IsDescChecked = false;
                if (null != chk)
                    m_IsDescChecked = chk.Checked;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pElement"></param>
        /// <param name="pParentElement"></param>
        private void BindChildren(Cst.EnumElement pElement, params Cst.EnumElement[] pParentElement)
        {
            int idxElement = int.Parse(Enum.Format(typeof(Cst.EnumElement), pElement, "d"));

            if (m_CurrentPage.FindControl(m_PrefixControl + "lst" + pElement.ToString()) is DropDownList ddlElement)
            {
                if (m_DsProduct == null)
                    this.Bind(pElement);

                try
                {
                    if (pParentElement.Length >= 1)
                    {
                        m_DsProduct.Tables[pElement.ToString()].DefaultView.RowFilter = "";
                        for (int i = 0; i < pParentElement.Length; i++)
                        {
                            if (m_CurrentPage.FindControl(m_PrefixControl + "lst" + pParentElement[i].ToString()) is DropDownList ctrlParent)
                                SetDataViewFilter(ref m_DsProduct, pElement, pParentElement[i], ctrlParent.SelectedValue);
                        }
                    }

                    ddlElement.DataSource = m_DsProduct.Tables[pElement.ToString()];
                    ddlElement.DataTextField = "IDENTIFIER";
                    ddlElement.DataValueField = "IDKEY";
                    ddlElement.DataBind();
                }
                catch (Exception ex) { throw new Exception("DataBind", ex); }

                ListItem item;
                try
                {
                    // on ajoute les items particuliers
                    if (pElement.Equals(Cst.EnumElement.Product))
                    {
                        if (m_IsAllItemIncluded)
                        {
                            item = new ListItem("[" + Ressource.GetString("AllProducts") + "]", "");
                            ddlElement.Items.Insert(0, item);
                        }
                        //
                        Colorize(Cst.EnumElement.Product);
                    }
                    else if (pElement.Equals(Cst.EnumElement.Instrument))
                    {
                        //item = new ListItem("[" + Ressource.GetString("AllInstruments") + "]------------------------------", "");
                        //item.Attributes.Add("style", "background-color:darkgray");
                        //((DropDownList)ctrl).Items.Insert(0, item);
                        if (m_IsAllItemIncluded)
                        {
                            item = new ListItem("[" + Ressource.GetString("AllInstruments") + "]", "");
                            ddlElement.Items.Insert(0, item);
                        }
                        //
                        Colorize(Cst.EnumElement.Instrument);
                    }
                    else if (pElement.Equals(Cst.EnumElement.Screen))
                    {
                        //20060530 PL Ajout du If() pour éviter de commencer par la full qui pose pb avec les payer/receiver quand on passe sur la "light"
                        //20090624 EG Positionnement permanent de Full en queue de liste
                        //if (((DropDownList)ctrl).Items.Count == 0)
                        //    ((DropDownList)ctrl).Items.Insert(0, new ListItem(Cst.Capture.TypeEnum.Full.ToString(), Cst.Capture.TypeEnum.Full.ToString()));
                        //((DropDownList)ctrl).Items.Insert(((DropDownList)ctrl).Items.Count, new ListItem(Cst.Capture.TypeEnum.Full.ToString(), Cst.Capture.TypeEnum.Full.ToString()));

                        ListItem fullItem = new ListItem(Cst.Capture.TypeEnum.Full.ToString(), Cst.Capture.TypeEnum.Full.ToString());
                        if (false == ddlElement.Items.Contains(fullItem))
                            ddlElement.Items.Insert(ddlElement.Items.Count, fullItem);
                    }
                }
                catch (Exception ex) { throw new Exception("Colorize", ex); }
                //
                item = null;
                // FI 20190726 [XXXXX] Pré-proposition du template avec le template spécifié dans par l'écran par défaut
                if (pElement == Cst.EnumElement.Template)
                {
                    try
                    {
                        // If a screen had been selected previous and set to the cookie, bring it back up
                        if (null != m_ElementValue[idxElement])
                            item = ddlElement.Items.FindByValue(m_ElementValue[idxElement]);
                        //
                        if (null == item)
                        {
                            // si le template est renseigné, on recherche un screen par defaut                    
                            if (m_CurrentPage.FindControl(m_PrefixControl + "lst" + Cst.EnumElement.Instrument.ToString()) is DropDownList ctrlParent &&
                                !StrFunc.IsEmpty(ctrlParent.SelectedValue))
                            {
                                // Otherwise, Search template in default Screen
                                // La colonne IDPARENT1 = IDI de l'instrument
                                foreach (DataRow row in m_DsProduct.Tables[Cst.EnumElement.Screen.ToString()].Select("IDPARENT1=" + ctrlParent.SelectedValue))
                                {
                                    if (true == row["ISDEFAULT"].ToString().ToUpper().Equals("TRUE"))
                                    {
                                        // La colonne IDPARENT = IDT du template
                                        item = ddlElement.Items.FindByValue(row["IDPARENT"].ToString());
                                        break;
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception ex) { throw new Exception("DefaultTemplate", ex); }
                }

                else if (pElement == Cst.EnumElement.Screen)
                {
                    try
                    {
                        // If a screen had been selected previous and set to the cookie, bring it back up
                        if (null != m_ElementValue[idxElement])
                            item = ddlElement.Items.FindByValue(m_ElementValue[idxElement]);

                        if (null == item)
                        {
                            // si le template est renseigné, on recherche un screen par defaut                    
                            if (m_CurrentPage.FindControl(m_PrefixControl + "lst" + Cst.EnumElement.Template.ToString()) is DropDownList ctrlParent && 
                                !StrFunc.IsEmpty(ctrlParent.SelectedValue))
                            {
                                // Otherwise, show default screen
                                foreach (DataRow row in m_DsProduct.Tables[pElement.ToString()].Select(m_DsProduct.Tables[pElement.ToString()].DefaultView.RowFilter))
                                {
                                    if (true == row["ISDEFAULT"].ToString().ToUpper().Equals("TRUE"))
                                    {
                                        item = ddlElement.Items.FindByValue(row["IDKEY"].ToString());
                                        break;
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception ex) { throw new Exception("DefaultScreen", ex); }
                }
                else
                {
                    //on selectionne l'item correspondant à elementValue[idx]
                    if (!StrFunc.IsEmpty(m_ElementValue[idxElement]))
                        item = ddlElement.Items.FindByValue(m_ElementValue[idxElement]);
                }


                if (item == null && (ddlElement.Items.Count > 0))
                {
                    item = ddlElement.Items[0];
                }

                //Si on pré-selectionne un item , on rebind ses enfants
                if (item != null)
                {
                    item.Selected = true;
                    if (pElement != Cst.EnumElement.Screen)
                        BindChildren(pElement + 1, GetParents(pElement + 1));
                }
                else if (pElement == Cst.EnumElement.Template) //cas Particulier du template: si aucun, on reset les screens
                {
                    try
                    {
                        DropDownList ddlScreen = m_CurrentPage.FindControl(m_PrefixControl + "lst" + Cst.EnumElement.Screen.ToString()) as DropDownList;
                        ddlScreen.DataSource = m_DsProduct.Tables[pElement.ToString()];
                        ddlScreen.DataTextField = "IDENTIFIER";
                        ddlScreen.DataValueField = "IDKEY";
                        ddlScreen.DataBind();
                    }
                    catch (Exception ex) { throw new Exception("ResetScreen", ex); }
                }

                DisplayInfo(pElement);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pElement"></param>
        /// EG 20210126 [25556] Minification des fichiers JQuery et des CSS
        public void Colorize(Cst.EnumElement pElement)
        {
            m_ClassList = new Hashtable();
            Control ctrl = m_CurrentPage.FindControl(m_PrefixControl + "lst" + pElement.ToString());

            //foreach (ListItem li in ((DropDownList)ctrl).Items)
            foreach (ExtendedListItem li in ((OptionGroupDropDownList)ctrl).ExtendedItems)
            {
                int posSemiColon = 0, posSemiColon2 = 0;
                string @class = "ddlCaptureItem";
                string text = li.Text;
                string color;
                if (0 != IntFunc.IntValue(li.Value))
                {
                    posSemiColon = text.IndexOf(";");
                    posSemiColon2 = posSemiColon;
                    if (Cst.EnumElement.Instrument == pElement)
                        posSemiColon2 = text.IndexOf(";", posSemiColon + 1);
                    color = text.Substring(posSemiColon2 + 1).Replace("EFSTheme", string.Empty).Replace(".min.css", string.Empty);
                    li.Text = text.Substring(0, posSemiColon);
                    @class = "ddlCapture_" + color.ToLower();
                    li.GroupingType = ListItemGroupingTypeEnum.Inherit;
                }
                if (0 >= IntFunc.IntValue(li.Value))
                {
                    if (0 != IntFunc.IntValue(li.Value))
                    {
                        if (Cst.EnumElement.Instrument == pElement) 
                            li.Text += " : " + text.Substring(posSemiColon + 1, posSemiColon2 - posSemiColon - 1);
                        li.GroupingText = li.Text;
                        li.GroupingType = ListItemGroupingTypeEnum.New;
                        li.GroupCssClass = EFSCssClass.DropDownListOptionGroup;
                    }
                    @class = "ddlCaptureTitle";
                }
                li.Attributes.Add("class", @class);
                m_ClassList.Add(li.Value, @class);
            }
            m_CurrentPage.Session["Class" + pElement.ToString() + "_" + m_SessionId] = m_ClassList;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pElement"></param>
		// EG 20200825 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        private void DisplayInfo(Cst.EnumElement pElement)
        {
            DataRow dr = null;
            Control ctrl = m_CurrentPage.FindControl(m_PrefixControl + "lst" + pElement.ToString());
            string id = string.Empty;
            string @value = string.Empty;
            if (ctrl != null)
            {
                id = ((DropDownList)ctrl).SelectedValue;
            }
            if (StrFunc.IsFilled(id) && (m_DsProduct != null))
            {
                @value = ((DropDownList)ctrl).SelectedItem.Text;
                if (Cst.EnumElement.Screen == pElement)
                {
                    // Need to search for 2 keys with Screen
                    Control ctrl2 = m_CurrentPage.FindControl(m_PrefixControl + "lst" + Cst.EnumElement.Template.ToString());
                    if (null != ctrl2)
                    {
                        string selectedTemplate = ((DropDownList)ctrl2).SelectedValue;
                        string instrumentOfTemplate = m_DsProduct.Tables[Cst.EnumElement.Template.ToString()].Rows.Find(selectedTemplate)["IDPARENT"].ToString();
                        string[] keys = { id, instrumentOfTemplate };
                        dr = m_DsProduct.Tables[pElement.ToString()].Rows.Find(keys);
                    }
                }
                else
                {
                    dr = m_DsProduct.Tables[pElement.ToString()].Rows.Find(id);
                }
            }
            //
            bool isDisplayData = (dr != null);
            ctrl = m_CurrentPage.FindControl(m_PrefixControl + "lbl" + pElement.ToString() + "-displayname");
            if (ctrl != null) 
                ((Label)ctrl).Text = (isDisplayData ? dr["DISPLAYNAME"].ToString() : string.Empty);
            ctrl = m_CurrentPage.FindControl(m_PrefixControl + "lbl" + pElement.ToString() + "-description");
            if (ctrl != null) 
                ((TextBox)ctrl).Text = (isDisplayData ? dr["DESCRIPTION"].ToString() : string.Empty);
            ctrl = m_CurrentPage.FindControl(m_PrefixControl + "img" + pElement.ToString());
            if (ctrl != null)
            {
                ((WCToolTipPanel)ctrl).Pty.TooltipContent = @value + Cst.HTMLBreakLine;
                ((WCToolTipPanel)ctrl).Pty.TooltipContent += (isDisplayData ? dr["DISPLAYNAME"].ToString() + Cst.HTMLBreakLine : string.Empty);
                ((WCToolTipPanel)ctrl).Pty.TooltipContent += (isDisplayData ? dr["DESCRIPTION"].ToString() + Cst.HTMLBreakLine : string.Empty);

            }
        }
        /// <summary>
        /// 
        /// </summary>
        private void GetLastProduct()
        {
            m_HiddenElementValue = new string[4] { null, null, null, null };
            m_ElementValue = new string[4] { null, null, null, null };
            
            string s;
             
            for (int i = 0; i < m_DsProduct.Tables.Count; i++)
            {
                object element = Enum.ToObject(typeof(Cst.EnumElement), i).ToString();
                
                AspTools.ReadSQLCookie(m_GrpElement, element.ToString(), out s);
                if (StrFunc.IsFilled(s))
                    m_ElementValue[i] = s;
            }
            
            if (null != m_CurrentPage.Session["TemplateInfo"])
                m_HiddenElementValue = (string[])m_CurrentPage.Session["TemplateInfo"];
            
            AspTools.ReadSQLCookie(m_GrpElement, Cst.SQLCookieElement.IsDisplayDescriptive.ToString(), out s);
            m_IsDescChecked = BoolFunc.IsTrue(s);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pElement"></param>
        /// <returns></returns>
        private Cst.EnumElement[] GetParents(Cst.EnumElement pElement)
        {
            // Non- logical parents
            Cst.EnumElement[] elems;
            if (Cst.EnumElement.Template == pElement)
                elems = new Cst.EnumElement[3] { Cst.EnumElement.Template, Cst.EnumElement.Instrument, Cst.EnumElement.Product };
            else if (Cst.EnumElement.Instrument == pElement)
                elems = new Cst.EnumElement[1] { Cst.EnumElement.Product };
            else if (Cst.EnumElement.Screen == pElement)
                elems = new Cst.EnumElement[2] { Cst.EnumElement.Template, Cst.EnumElement.Product };
            else if (Cst.EnumElement.Product == pElement)
                elems = new Cst.EnumElement[1] { pElement };
            else
                throw new NotImplementedException(StrFunc.AppendFormat("Value: {0} is not impleted", pElement.ToString()));

            return elems;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        // FI 20140930 [XXXXX] Modify
        // EG 20150222 Refactoring Queries
        // EG 20180423 Analyse du code Correction [CA2200]
        private DataSet LoadListProductInstrumentTemplate()
        {
            //WARNING: Utilisation de "select distinct" car les jointures issues de "GetSQLJoin_ModelInstrument()"
            //         peuvent renvoyer des doublons.
            
            string source = SessionTools.CS;
            
            SessionRestrictHelper srh = new SessionRestrictHelper(SessionTools.User, SessionTools.SessionID, false);

            // FI 20140930 [XXXXX] call method GetSqlProductRestriction
            string productRestriction = GetSQLRestrictProduct(source); 

            string instrumentSession = string.Empty;  
            if (false == SessionTools.IsSessionSysAdmin)
                instrumentSession = srh.GetSQLInstr(string.Empty, "i.IDI");
            
            string instrumentEnabled = OTCmlHelper.GetSQLDataDtEnabled(source, "i") + Cst.CrLf;
            
            #region Product
            string sqlQuery = @"select distinct 0 as GRP, p.IDP as IDKEY, p.IDENTIFIER || ';' || p.CSSFILENAME as IDENTIFIER, 
            p.DISPLAYNAME as DISPLAYNAME, p.DESCRIPTION, p.FAMILY
            from dbo.PRODUCT p
            inner join dbo.INSTRUMENT i on (i.IDP = p.IDP) 
            /* InstrumentEnabled */ {0} 
            /* InstrumentSession */ {1} 
            where 
            /* ProductRestriction */ {2} 
            union all 
            select distinct 1 as GRP, -p.IDP as IDKEY, p.FAMILY || ';' || p.CSSFILENAME as IDENTIFIER, 
            null as DISPLAYNAME, null as DESCRIPTION, p.FAMILY
            from dbo.PRODUCT p
            inner join dbo.INSTRUMENT i on (i.IDP = p.IDP) 
            /* InstrumentEnabled  */ {0} 
            /* InstrumentSession  */ {1} 
            where 
            /* ProductRestriction */ {2} 
            and (p.IDP in ( select MIN(p2.IDP) 
                            from dbo.PRODUCT p2
                            inner join dbo.INSTRUMENT i on (i.IDP = p2.IDP) {0}
                            where (p2.FAMILY = p.FAMILY) and {3})
            );" + Cst.CrLf;

            sqlQuery = String.Format(sqlQuery, m_isInstrumentEnabled ? " and " + instrumentEnabled : string.Empty,
                instrumentSession, productRestriction, productRestriction.Replace("p.", "p2."));
            #endregion Product
            #region Instrument
            sqlQuery += @"select distinct 0 as GRP, i.IDI as IDKEY, i.IDP as IDPARENT, 
            i.IDENTIFIER || ';' || p.IDENTIFIER || ';' || p.CSSFILENAME as IDENTIFIER, i.DISPLAYNAME, i.DESCRIPTION, p.FAMILY, 
            p.IDENTIFIER as PRODUCT_IDENTIFIER
            from dbo.INSTRUMENT i
            inner join dbo.PRODUCT p on (p.IDP = i.IDP) and 
            /* ProductRestriction */ {1}
            /* InstrumentSession  */ {0} 
            /* InstrumentEnabled  */ {2}
            union all
            select distinct 1 as GRP, -i.IDI as IDKEY, i.IDP as IDPARENT, 
            p.FAMILY || ';' || p.IDENTIFIER || ';' || p.CSSFILENAME as IDENTIFIER,
            null as DISPLAYNAME, null as DESCRIPTION, p.FAMILY, p.IDENTIFIER as PRODUCT_IDENTIFIER
            from dbo.INSTRUMENT i
            inner join dbo.PRODUCT p on (p.IDP = i.IDP) and {1}
            where (i.IDI in ( select MIN(i2.IDI) 
                            from dbo.INSTRUMENT i2
                            inner join dbo.PRODUCT p2 on (p2.IDP = i2.IDP) and 
                            /* ProductRestriction */ {3}
                            /* InstrumentSession  */ {4}
                            where (p2.IDENTIFIER = p.IDENTIFIER) 
                            /* InstrumentEnabled  */ {5}
                          ));" + Cst.CrLf;

            sqlQuery = String.Format(sqlQuery, instrumentSession, productRestriction, 
                m_isInstrumentEnabled ? " where " + instrumentEnabled : string.Empty,
                productRestriction.Replace("p.", "p2."), instrumentSession.Replace("i.", "i2."),
                m_isInstrumentEnabled ? " and " + instrumentEnabled.Replace("i.", "i2.") : string.Empty);
            #endregion Instrument
            #region Template
            sqlQuery += @"select distinct vti.IDKEY, vti.IDPARENT, vti.IDPARENT1, vti.IDENTIFIER, vti.DISPLAYNAME, vti.DESCRIPTION, vti.FILLER 
            from dbo.VW_TRADE_INSTR_TEMPLATE vti
            /* InstrumentSession  */ {0} 
            inner join dbo.PRODUCT p on (p.IDP = vti.IDPARENT1) and 
            /* ProductRestriction */ {1}
            /* InstrumentEnabled  */ {2};" + Cst.CrLf;

            sqlQuery = String.Format(sqlQuery, instrumentSession.Replace("i.IDI", "vti.IDPARENT"), productRestriction,
                m_isInstrumentEnabled ? " where " + OTCmlHelper.GetSQLDataDtEnabled(source, "vti") : string.Empty);
            #endregion Template
            #region Screen
            sqlQuery += @"select distinct ig.SCREENNAME  as IDKEY, ig.SCREENNAME as IDENTIFIER, t.IDKEY as IDPARENT, t.IDPARENT as IDPARENT1, 
            ig.SCREENNAME as DISPLAYNAME, ig.DESCRIPTION, ig.ISDEFAULT, t.FILLER
            from dbo.INSTRUMENTGUI ig 
            inner join dbo.VW_TRADE_INSTR_TEMPLATE t on (t.IDPARENT = ig.IDI)
            /* InstrumentSession  */ {0} 
            inner join dbo.PRODUCT p on (p.IDP = t.IDPARENT1) and 
            /* ProductRestriction */ {1}
            /* InstrumentEnabled  */ {2};" + Cst.CrLf;

            sqlQuery = String.Format(sqlQuery, instrumentSession.Replace("i.IDI", "t.IDPARENT"), productRestriction,
                m_isInstrumentEnabled ? " and " + OTCmlHelper.GetSQLDataDtEnabled(source, "t") : string.Empty);

            #endregion Screen

            DataSet ds;
            try
            {
                ds = DataHelper.ExecuteDataset(CSTools.SetCacheOn(source), CommandType.Text, sqlQuery);

                for (int i = 0; i < ds.Tables.Count; i++)
                {
                    //if (i == (int)Cst.EnumElement.Screen && (0 == ds.Tables[i].Rows.Count))
                    //{
                    //    // Get Data Directly from XML if empty recordset
                    //    //ControlTools.DDLLoad_CustomisedInput(null);
                    //}

                    object element = Enum.ToObject(typeof(Cst.EnumElement), i).ToString();
                    ds.Tables[i].TableName = element.ToString();
                    //20090512 PL/Test 
                    if (i == (int)Cst.EnumElement.Product)
                        ds.Tables[i].DefaultView.Sort = "FAMILY, GRP desc, IDENTIFIER";
                    else if (i == (int)Cst.EnumElement.Instrument)
                        ds.Tables[i].DefaultView.Sort = "FAMILY, PRODUCT_IDENTIFIER, GRP desc, IDENTIFIER";
                    else if (i == (int)Cst.EnumElement.Template)
                        ds.Tables[i].DefaultView.Sort = "IDENTIFIER";
                    else if (i == (int)Cst.EnumElement.Screen)
                        ds.Tables[i].DefaultView.Sort = "IDENTIFIER";
                    else
                        throw new NotImplementedException(StrFunc.AppendFormat("Value: {0} is not impleted", i.ToString()));

                    DataColumn[] columnPK;
                    if (i == (int)Cst.EnumElement.Screen)
                    {
                        // Screens need to keys
                        columnPK = new DataColumn[2] { ds.Tables[i].Columns["IDKEY"], ds.Tables[i].Columns["IDPARENT"] };
                    }
                    else
                        columnPK = new DataColumn[1] { ds.Tables[i].Columns["IDKEY"] };

                    ds.Tables[i].PrimaryKey = columnPK;
                }
            }
            catch (Exception)
            {
                throw;
            }
            return ds;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pElement"></param>
        public void RestoreColor(Cst.EnumElement pElement)
        {
            if (this.m_DsProduct != null)
            {
                Control ctrl;
                switch (pElement)
                {
                    case Cst.EnumElement.Product:
                    case Cst.EnumElement.Instrument:
                        m_ClassList = (Hashtable)m_CurrentPage.Session["Class" + pElement.ToString() + "_" + m_SessionId];
                        ctrl = m_CurrentPage.FindControl(m_PrefixControl + "lst" + pElement.ToString());
                        foreach (ListItem li in ((DropDownList)ctrl).Items)
                        {
                            if (m_ClassList.ContainsKey(li.Value))
                                li.Attributes.Add("class", m_ClassList[li.Value].ToString());
                        }
                        break;
                    default:
                        ctrl = m_CurrentPage.FindControl(m_PrefixControl + "lst" + Cst.EnumElement.Instrument.ToString());
                        if (null != ctrl)
                        {
                            ListItem liInstrument = ((DropDownList)ctrl).SelectedItem;
                            if (null != liInstrument)
                            {
                                Control ctrlChild = m_CurrentPage.FindControl(m_PrefixControl + "lst" + pElement.ToString());
                                foreach (ListItem li in ((DropDownList)ctrlChild).Items)
                                {
                                    li.Attributes.Add("class", liInstrument.Attributes["class"].ToString());
                                }
                            }
                        }
                        break;
                }
                DisplayInfo(pElement);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public void SaveLastProduct()
        {
            CookieData[] cookiedata = new CookieData[m_NumberOfSections + 1];
            if ((m_CurrentPage.FindControl(m_PrefixControl + "lst" + Cst.EnumElement.Template.ToString()) is DropDownList ddl) && StrFunc.IsFilled(ddl.SelectedValue))
            {
                if (null != m_DsProduct)
                {
                    DataRow dr = m_DsProduct.Tables[Cst.EnumElement.Template.ToString()].Rows.Find(ddl.SelectedValue);
                    if (null != dr)
                    {
                        SetCookieValue(cookiedata, Cst.EnumElement.Product, dr["IDPARENT1"].ToString());
                        SetCookieValue(cookiedata, Cst.EnumElement.Instrument, dr["IDPARENT"].ToString());
                        SetCookieValue(cookiedata, Cst.EnumElement.Template, ddl.SelectedItem.Text);
                    }
                }
            }
            else
            {
                SetCookieValue(cookiedata, Cst.EnumElement.Template, string.Empty);
            }

            if (m_CurrentPage.FindControl(m_PrefixControl + "lst" + Cst.EnumElement.Screen.ToString()) is DropDownList ddlScreen)
                SetCookieValue(cookiedata, Cst.EnumElement.Screen, ddlScreen.SelectedValue == string.Empty ? Cst.FpML_ScreenFullCapture : ddlScreen.SelectedValue);

            if (m_CurrentPage.FindControl(m_PrefixControl + "chkDescription") is CheckBox chk)
                cookiedata[m_NumberOfSections] = new CookieData(m_GrpElement, Cst.SQLCookieElement.IsDisplayDescriptive.ToString(), (chk.Checked ? "1" : "0"));

            AspTools.WriteSQLCookie(cookiedata);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCookiedata"></param>
        /// <param name="pElement"></param>
        /// <param name="pValue"></param>
        /// FI 20140930 [XXXXX] private method
        private void SetCookieValue(CookieData[] pCookiedata, Cst.EnumElement pElement, string pValue)
        {
            int idx = int.Parse(Enum.Format(typeof(Cst.EnumElement), pElement, "d"));
            pCookiedata[idx] = new CookieData(m_GrpElement, pElement.ToString(), pValue);
        }
        /// <summary>
        /// Used to realign DropDownLists from a change on screen
        /// </summary>
        /// <param name="pDs">DataSet of pElement</param>
        /// <param name="pElement">Element whose filter needs to be changed</param>
        /// <param name="pParentElement">Elements used to as criteria of filtered table of pElement</param>
        /// <param name="pValue">Selected value of DropDown pParentElement, if "" => Tous xxx</param>
        /// <returns>void</returns>
        private void SetDataViewFilter(ref DataSet pDs, Cst.EnumElement pElement, Cst.EnumElement pParentElement, string pValue)
        {

            bool isNotFirstTime = false;
            string rowFilter = pDs.Tables[pElement.ToString()].DefaultView.RowFilter;
            if (0 < rowFilter.Length)
                isNotFirstTime = true;

            if (StrFunc.IsEmpty(pValue) && (!m_IsBindParents))
                return;

            int idx;

            // The first switch = table to be filtered. Can be filtered by + than one criteria. Ex. A template can filtered by an instrument and/or product
            // The second switch/if is the element the gives a filter. An instrument is filtered with respect to a product, etc.
            // isBindParents is used only when Tous Produits and Tous Instruments are chosen and a change 
            //   from the Template DropDown is the origin of the execution
            // isBindParents allows the product and instrument to stay at Tous Produits and Tous instruments
            // It also allows the values of product and instrument to be saved to a temporary cookie.
            switch (pElement)
            {
                case Cst.EnumElement.Screen:
                    // Update Screen with respect to Template chosen
                    if (Cst.EnumElement.Template == pParentElement)
                        pDs.Tables[pElement.ToString()].DefaultView.RowFilter = (isNotFirstTime ? rowFilter + " AND " : "") + "IDPARENT = " + pValue;
                    break;
                case Cst.EnumElement.Template:

                    idx = int.Parse(Enum.Format(typeof(Cst.EnumElement), Cst.EnumElement.Template, "d"));
                    switch (pParentElement)
                    {
                        // Filter template with respect to Instrument
                        case Cst.EnumElement.Instrument:
                            if (false == m_IsBindParents) // false because case executed both when true and when false
                                pDs.Tables[pElement.ToString()].DefaultView.RowFilter = (isNotFirstTime ? rowFilter + " AND " : "") + "IDPARENT = " + pValue.Replace("-", string.Empty);
                            break;
                        // Filter template with respect to Product
                        case Cst.EnumElement.Product:
                            if (false == m_IsBindParents) // false because case executed both when true and when false
                                pDs.Tables[pElement.ToString()].DefaultView.RowFilter = (isNotFirstTime ? rowFilter + " AND " : "") + "IDPARENT1 = " + pValue.Replace("-", string.Empty);
                            break;
                        case Cst.EnumElement.Template:
                            // Gets currently select value and set to elementValue which wil be used to 
                            // reselect value. Otherswise, reverts to last saved value in cookie.
                            if (m_IsBindParents)
                            {
                                if (m_CurrentPage.FindControl(m_PrefixControl + "lst" + Cst.EnumElement.Template.ToString()) is DropDownList ctrl)
                                {
                                    m_HiddenElementValue[idx] = ctrl.SelectedValue;
                                    m_ElementValue[idx] = ctrl.SelectedValue;
                                }
                            }
                            else
                                // 20090922 EG
                                m_ElementValue[idx] = "";
                            break;
                    }
                    break;

                case Cst.EnumElement.Product:
                    if (Cst.EnumElement.Instrument == pParentElement)
                    {
                        if (m_IsBindParents)
                        {
                            // Find product of selected template and save to cookie.
                            idx = int.Parse(Enum.Format(typeof(Cst.EnumElement), pElement, "d"));
                            DataRow dr = null;
                            if (m_CurrentPage.FindControl(m_PrefixControl + "lst" + Cst.EnumElement.Template.ToString()) is DropDownList ctrlTemplate)
                            {
                                if (false == ctrlTemplate.SelectedValue.Equals(string.Empty))
                                    dr = m_DsProduct.Tables[Cst.EnumElement.Template.ToString()].Rows.Find(ctrlTemplate.SelectedValue);
                            }
                            if (null != dr)
                            {
                                m_HiddenElementValue[idx] = dr["IDPARENT1"].ToString();
                            }
                            // Resets Product to Tous Produits
                            m_ElementValue[idx] = "";
                        }
                    }
                    break;
                case Cst.EnumElement.Instrument:
                    if (Cst.EnumElement.Product == pParentElement)
                    {
                        if (m_IsBindParents)
                        {
                            // Find instrument of selected template and save to cookie.
                            DataRow dr = null;

                            idx = int.Parse(Enum.Format(typeof(Cst.EnumElement), pElement, "d"));
                            Control ctrlTemplate = m_CurrentPage.FindControl(m_PrefixControl + "lst" + Cst.EnumElement.Template.ToString());
                            if (null != ctrlTemplate)
                            {
                                if (false == ((DropDownList)ctrlTemplate).SelectedValue.Equals(string.Empty))
                                    dr = m_DsProduct.Tables[Cst.EnumElement.Template.ToString()].Rows.Find(((DropDownList)ctrlTemplate).SelectedValue);

                                if (null != dr)
                                    m_HiddenElementValue[idx] = dr["IDPARENT"].ToString();
                            }

                            // Resets Instrument to Tous Instruments
                            m_ElementValue[idx] = "";
                        }
                        else
                        {
                            // Filter template with respect to Product
                            pDs.Tables[pElement.ToString()].DefaultView.RowFilter = (isNotFirstTime ? rowFilter + " AND " : "") + "IDPARENT = " + pValue.Replace("-", string.Empty);
                        }

                    }
                    break;
            }
        }
        /// <summary>
        ///  Retourne la restriction SQL sur la table PRODUCT (alias p)
        /// </summary>
        /// <returns></returns>
        /// FI 20140930 [XXXXX] Add method
        private string GetSQLRestrictProduct(string pCS)
        {

            StrBuilder ret = new StrBuilder();

            switch (m_GrpElement)
            {
                case Cst.SQLCookieGrpElement.SelADMProduct:
                    ret += StrFunc.AppendFormat("p.GPRODUCT = '{0}'", Cst.ProductGProduct_ADM);
                    ret += SQLCst.AND + StrFunc.AppendFormat("p.FAMILY = '{0}'", Cst.ProductFamily_INV);
                    break;

                case Cst.SQLCookieGrpElement.SelDebtSecProduct:
                    ret += StrFunc.AppendFormat("p.GPRODUCT = '{0}'", Cst.ProductGProduct_ASSET);
                    ret += SQLCst.AND + StrFunc.AppendFormat("p.FAMILY = '{0}'", Cst.ProductFamily_DSE);
                    break;

                case Cst.SQLCookieGrpElement.SelRiskProduct:
                    ret += StrFunc.AppendFormat("p.GPRODUCT = '{0}'", Cst.ProductGProduct_RISK);
                    // PM 20120809 [18058] Add ProductFamily_CASHINTEREST
                    // FI 20140930 [XXXXX] add ProductFamily_CASHPAYMENT
                    ret += SQLCst.AND + DataHelper.SQLColumnIn(pCS, "p.FAMILY",
                        new string[] { Cst.ProductFamily_MARGIN.ToString(), 
                                        Cst.ProductFamily_CASHBALANCE.ToString(), 
                                        Cst.ProductFamily_CASHINTEREST.ToString(), 
                                        Cst.ProductFamily_CASHPAYMENT.ToString(), 
                                            }, TypeData.TypeDataEnum.@string);
                    break;

                case Cst.SQLCookieGrpElement.SelRiskProductCashBalance:
                    ret += StrFunc.AppendFormat("p.GPRODUCT = '{0}'", Cst.ProductGProduct_RISK);
                    ret += SQLCst.AND + StrFunc.AppendFormat("p.FAMILY = '{0}'", Cst.ProductFamily_CASHBALANCE);
                    break;

                case Cst.SQLCookieGrpElement.SelRiskProductMargin:
                    ret += StrFunc.AppendFormat("p.GPRODUCT = '{0}'", Cst.ProductGProduct_RISK);
                    ret += SQLCst.AND + StrFunc.AppendFormat("p.FAMILY = '{0}'", Cst.ProductFamily_MARGIN);
                    break;

                case Cst.SQLCookieGrpElement.SelRiskProductCashPayment:
                    ret += StrFunc.AppendFormat("p.GPRODUCT = '{0}'", Cst.ProductGProduct_RISK);
                    ret += SQLCst.AND + StrFunc.AppendFormat("p.FAMILY = '{0}'", Cst.ProductFamily_CASHPAYMENT);
                    break;

                case Cst.SQLCookieGrpElement.SelRiskProductCashInterest:
                    ret += StrFunc.AppendFormat("p.GPRODUCT = '{0}'", Cst.ProductGProduct_RISK);
                    ret += SQLCst.AND + StrFunc.AppendFormat("p.FAMILY = '{0}'", Cst.ProductFamily_CASHINTEREST);
                    break;


                case Cst.SQLCookieGrpElement.SelProduct:
                default:
                    ret += "p.GPRODUCT" + StrFunc.AppendFormat(" not in ({0},{1},{2})",
                        DataHelper.SQLString(Cst.ProductGProduct_ADM),
                        DataHelper.SQLString(Cst.ProductGProduct_ASSET),
                        DataHelper.SQLString(Cst.ProductGProduct_RISK));
                    break;
            }

            return ret.ToString();
        }

        #endregion Methods
    }
    #endregion
}

