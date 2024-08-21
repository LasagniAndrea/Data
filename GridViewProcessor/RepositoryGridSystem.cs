#region using directives
using EFS.ACommon;
using EFS.Common.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Schema;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
#endregion using directives

namespace EFS.GridViewProcessor
{
    #region BSPanelTypeEnum
    public enum BSPanelTypeEnum
    {
        // Panel avec body seulement sans radius + titre de type <p class="title">
        basic,
        subbasic,
        // Panel avec body seulement
        body,
        // Panel avec header et body 
        heading,
        // Panel avec body et footer
        footer,
        // Panel avec header, body et footer
        full
    }
    #endregion BSPanelTypeEnum

    #region RepositoryForm
    /// <summary>
    /// Represente le template pour le formulaire Referential
    /// </summary>
    [Serializable]
    [System.Xml.Serialization.XmlRootAttribute("form", IsNullable = false)]
    public class RepositoryForm
    {
        #region Members
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool autogen;

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public bool inline;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool inlineSpecified;

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public bool horizontal;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool horizontalSpecified;

        [System.Xml.Serialization.XmlElementAttribute("col", typeof(BSGridCol), Form = XmlSchemaForm.Unqualified)]
        [System.Xml.Serialization.XmlElementAttribute("common", typeof(BSGridCommon), Form = XmlSchemaForm.Unqualified)]
        [System.Xml.Serialization.XmlElementAttribute("external", typeof(BSGridExternal), Form = XmlSchemaForm.Unqualified)]
        [System.Xml.Serialization.XmlElementAttribute("item", typeof(BSGridItem), Form = XmlSchemaForm.Unqualified)]
        [System.Xml.Serialization.XmlElementAttribute("role", typeof(BSGridRole), Form = XmlSchemaForm.Unqualified)]
        [System.Xml.Serialization.XmlElementAttribute("row", typeof(BSGridRow), Form = XmlSchemaForm.Unqualified)]
        [System.Xml.Serialization.XmlElementAttribute("tabs", typeof(BSGridTabs), Form = XmlSchemaForm.Unqualified)]
        public BSGridTag[] tags;

        [System.Xml.Serialization.XmlElementAttribute("value", typeof(BSGridValue), Form = XmlSchemaForm.Unqualified)]
        [System.Xml.Serialization.XmlElementAttribute("comment", typeof(BSGridComment), Form = XmlSchemaForm.Unqualified)]
        public BSGridValueBase[] values;

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string mnu;

        #endregion Members
        #region Constructors
        public RepositoryForm()
        {
            
        }

        public RepositoryForm(string pIdMenu)
        {
            mnu = ControlsTools.MainMenuName(pIdMenu);
        }
        #endregion Constructors
        #region Methods
        #region SetClass
        /// <summary>
        /// Ininitalisation de style du formulaire BootStrap
        /// </summary>
        /// <param name="pPnlGridSystem"></param>
        public void SetClass(Panel pPnlGridSystem)
        {
            pPnlGridSystem.CssClass = "col-sm-12";
            if (inlineSpecified && inline)
                pPnlGridSystem.CssClass += " form-inline";
            if (horizontalSpecified && horizontal)
                pPnlGridSystem.CssClass += " form-horizontal";

            // Suffix menu principal (aide aux couleurs des panels et titres)
            if (StrFunc.IsFilled(mnu))
                pPnlGridSystem.CssClass += " " + mnu;

            if (autogen)
                pPnlGridSystem.CssClass += " autogen";

        }
        #endregion SetClass
        #region CreateControls
        /// <summary>
        /// Construction des contrôles pour chaque colonne spécifiées dans le formulaire (GridSystem)
        /// </summary>
        /// <param name="pColumns">Colonnes du référentiel</param>
        /// <param name="plhGridSystem">Container principal du formulaire</param>
        public void CreateControls(RepositoryPageBase pPage, Control pPnlGridSystem)
        {
            bool isDisplayColumnName = pPage.isDisplayColumn;
            bool isShowAllData = pPage.IsShowAllData;

            List<ReferentialColumn> lstRrc = pPage.referential.Column.Where(column => column.IsHide == false).ToList();

            List<PlaceHolder> ctrls = ControlsTools.GetControls<PlaceHolder>(pPnlGridSystem.Controls);
            ctrls.ForEach(plh =>
            {
                ReferentialColumn rrc = lstRrc.Find(item => item.BSColumnName == plh.ID);
                // si IDENTIFIER n'est pas trouvé on recherche la 1ère colonne avec ISDATAKEYFIELD = true
                if (null == rrc) 
                {
                    switch (plh.ID)
                    {
                        case "FOREIGNKEY":
                            rrc = lstRrc.FirstOrDefault(item => item.IsForeignKeyFieldSpecified && item.IsForeignKeyField);
                            break;
                        case "IDENTIFIER":
                            rrc = lstRrc.FirstOrDefault(item => item.IsDataKeyFieldSpecified && item.IsDataKeyField && (false == item.IsForeignKeyField));
                            break;
                    }
                }
                if (null != rrc)
                {
                    plh.Controls.Add(RepositoryTools.CreateControl(pPage, rrc));
                }
                else if (StrFunc.IsFilled(plh.ID) && plh.ID.StartsWith("Msg_"))
                {
                    // Commentaire additionel
                    plh.Controls.Add(RepositoryTools.CreateControl_Comment(plh.ID));
                }
            });
        }
        #endregion CreateControls
        #region DisplayConditionalGridSystem
        /// <summary>
        /// Gestion de l'affichage des données (Cas spécifique)
        /// INSTRUMENT : Règles de validation
        /// ASSETENV : Règles de validation
        /// </summary>
        /// <param name="pPage"></param>
        public void DisplayConditionalGridSystem(RepositoryPageBase pPage)
        {
            if (pPage.referential.TableName == Cst.OTCml_TBL.INSTRUMENT.ToString())
            {
                DisplayConditionalGridSystem_INSTRUMENT(pPage.referential);
            }
            else if (pPage.referential.TableName == Cst.OTCml_TBL.ASSETENV.ToString())
            {
                DisplayConditionalGridSystem_ASSETENV(pPage.referential);
            }
        }
        #endregion DisplayConditionalGridSystem
        #region DisplayConditionalGridSystem_INSTRUMENT
        /// <summary>
        /// Affichages des données de règles de validation en fonction du couple GPRODUCT|FAMILY
        /// Repository : INSTRUMENT.XML
        /// </summary>
        /// <param name="pReferential"></param>
        private void DisplayConditionalGridSystem_INSTRUMENT(Referential pReferential)
        {
            Nullable<ProductTools.GroupProductEnum> groupProduct = pReferential.GroupProduct;
            Nullable<ProductTools.FamilyEnum> familyProduct = pReferential.FamilyProduct;
            string product = pReferential.dataRow["PRODUCT_IDENTIFIER"].ToString();

            // Recherche de la section avec id = ValidationRules_Title
            if (familyProduct.HasValue && groupProduct.HasValue)
            {
                object obj = GetElementById(tags, "ValidationRules_Title");
                if (null != obj)
                {
                    object ird = GetElementById(obj, "IRDValidationRules_Title");
                    if (null != ird)
                    {
                        (ird as BSGridTag).visibleSpecified = true;
                        (ird as BSGridTag).visible = (familyProduct.Value == ProductTools.FamilyEnum.InterestRateDerivative) ||
                            ((familyProduct.Value == ProductTools.FamilyEnum.DebtSecurity) && (groupProduct.Value == ProductTools.GroupProductEnum.Security));
                    }

                    object fx = GetElementById(obj, "FXValidationRules_Title");
                    if (null != fx)
                    {
                        (fx as BSGridTag).visibleSpecified = true;
                        (fx as BSGridTag).visible = (familyProduct.Value == ProductTools.FamilyEnum.ForeignExchange);
                    }
                    object option = GetElementById(obj, "OptionValidationRules_Title");
                    if (null != option)
                    {
                        (option as BSGridTag).visibleSpecified = true;
                        (option as BSGridTag).visible = product.EndsWith("Option") || (product == "swaption");
                    }

                    object dseAsset = GetElementById(obj, "DSEASSETValidationRules_Title");
                    if (null != dseAsset)
                    {
                        (dseAsset as BSGridTag).visibleSpecified = true;
                        (dseAsset as BSGridTag).visible = (familyProduct.Value == ProductTools.FamilyEnum.DebtSecurity) && (groupProduct.Value == ProductTools.GroupProductEnum.Asset);
                    }
                    object dseSec = GetElementById(obj, "DSESECValidationRules_Title");
                    if (null != dseSec)
                    {
                        (dseSec as BSGridTag).visibleSpecified = true;
                        (dseSec as BSGridTag).visible = (familyProduct.Value == ProductTools.FamilyEnum.DebtSecurity) && (groupProduct.Value == ProductTools.GroupProductEnum.Security);
                    }
                    object repo = GetElementById(obj, "RepoValidationRules_Title");
                    if (null != repo)
                    {
                        (repo as BSGridTag).visibleSpecified = true;
                        (repo as BSGridTag).visible = (product == "buyAndSellBack") || (product == "securityLending") || (product == "repo");
                    }
                }
            }
        }
        #endregion DisplayConditionalGridSystem_INSTRUMENT
        #region DisplayConditionalGridSystem_ASSETENV
        /// <summary>
        /// Affichages des données de règles de validation en fonction du couple GPRODUCT|FAMILY
        /// Repository : ASSETENV.XML 
        /// </summary>
        /// <param name="pReferential"></param>
        private void DisplayConditionalGridSystem_ASSETENV(Referential pReferential)
        {
            Nullable<ProductTools.GroupProductEnum> groupProduct = pReferential.GroupProduct;
            Nullable<ProductTools.FamilyEnum> familyProduct = pReferential.FamilyProduct;

            if (familyProduct.HasValue && groupProduct.HasValue)
            {
                // Recherche de la section avec id = Environment_Title
                object obj = GetElementById(tags, "Environment_Title");
                if (null != obj)
                {
                    object dse = GetElementById(obj, "DSEAssetEnv_Title");
                    if (null != dse)
                    {
                        (dse as BSGridTag).visibleSpecified = true;
                        (dse as BSGridTag).visible = (familyProduct.Value == ProductTools.FamilyEnum.DebtSecurity);
                    }

                }
            }
        }
        #endregion DisplayConditionalGridSystem_ASSETENV

        #region GetElementById
        /// <summary>
        /// Finds a Control recursively. Note finds the first match and exists
        /// </summary>
        /// <param name="pRoot">Racine de la recherche</param>
        /// <param name="pId">Id</param>
        /// <returns></returns>
        private object GetElementById <T>(T pSource, string pId)
        {
            object foundObject = null;
            BSGridTag[] tags = null;
            if (pSource is BSGridTag[])
            {
                foreach (BSGridTag tag in pSource as BSGridTag[])
                {
                    foundObject = GetElementById(tag, pId);
                    if (null != foundObject)
                        break;
                }
            }
            else if (pSource is BSGridTabs)
            {
                BSGridTabs tabs = pSource as BSGridTabs;
                if (tabs.id == pId)
                    return tabs;
                if (ArrFunc.IsFilled(tabs.sections))
                    foundObject = GetElementById(tabs.sections, pId);
            }

            else if (pSource is BSGridSection)
            {
                BSGridSection section = pSource as BSGridSection;
                if (section.id == pId)
                    return section;
                tags = section.tags;
            }
            else if (pSource is BSGridCol)
            {
                BSGridCol col = pSource as BSGridCol;
                if (col.id == pId)
                    return col;
                tags = col.tags;
            }
            else if (pSource is BSGridRow)
            {
                BSGridRow row = pSource as BSGridRow;
                if (row.id == pId)
                    return row;
                tags = row.tags;
            }

            if (ArrFunc.IsFilled(tags))
                foundObject = GetElementById(tags, pId);

            return foundObject;
        }
        #endregion GetElementById


        #endregion Methods
    }
    #endregion RepositoryForm

    #region BSGridTag
    /// <summary>
    /// Représente un grid (BOOTSTRAP) 
    /// </summary>
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(BSGridTabs))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(BSGridCol))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(BSGridCommon))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(BSGridRow))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(BSGridExternal))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(BSGridRole))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(BSGridItem))]
    public abstract class BSGridTag
    {
        #region Members
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string id;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string title;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string color;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool visibleSpecified;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public bool visible;

        [System.Xml.Serialization.XmlElementAttribute("pnl", typeof(BSGridPanel), Form = XmlSchemaForm.Unqualified)]
        public BSGridPanel pnl;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool pnlSpecified;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        protected HtmlGenericControl _control;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        private BSGridTag _bsgridTagParent;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        private List<ReferentialColumn> _lstColumns;
        #endregion Members

        #region Accessors
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool IsVisible
        {
            get { return (visibleSpecified && visible) || (false == visibleSpecified); }
        }

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public BSGridTag BsGridTagParent
        {
            get { return _bsgridTagParent; }
            set 
            { 
                _bsgridTagParent = value;
                if (null != _bsgridTagParent)
                    LstColumns = _bsgridTagParent.LstColumns;
            }
        }

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public List<ReferentialColumn> LstColumns
        {
            get { return _lstColumns; }
            set { _lstColumns = value;}
        }

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public BSGridTag BSGridTagParent
        {
            get { return _bsgridTagParent; }
        }
        public virtual Control Control
        {
            get
            {
                return _control;
            }
        }
        public virtual Control ControlContentBody
        {
            get
            {
                HtmlGenericControl content = _control;
                if (pnlSpecified)
                    content = pnl.PanelBody;
                return content;
            }
        }
        public virtual Control ControlContentFooter
        {
            get
            {
                HtmlGenericControl content = _control;
                if (pnlSpecified)
                    content = pnl.PanelFooter;
                return content;
            }
        }
        #endregion Accessors

        #region Constructors
        public BSGridTag()
        {
            _control = new HtmlGenericControl("div");
        }
        #endregion Constructors
        #region Methods
        public static List<string> RessourceTitles(string pTitle)
        {
            List<string> lstTitles = new List<string>();
            if (StrFunc.IsFilled(pTitle))
            {
                string[] titles = pTitle.Split('|');
                if (ArrFunc.IsFilled(titles))
                {
                    titles.ToList().ForEach(item =>
                    {
                        lstTitles.Add(Ressource.GetString(item,true));
                    });
                }
            }
            return lstTitles;
        }


        public void AddPanel(BSPanelTypeEnum pType, string pTitle)
        {
            AddPanel(null, pType, pTitle);
        }

        public void AddPanel(string pColor, BSPanelTypeEnum pType, string pTitle)
        {
            pnlSpecified = true;
            pnl = new BSGridPanel(pColor,pType, pTitle);
        }
        public virtual void CreateGridSystem()
        {
            CreateGridSystem(null);
        }
        public virtual void CreateGridSystem(BSGridTag pBSGridParent)
        {
        }
        protected List<BSGridValueBase> AddCommonValue()
        {
            List<BSGridValueBase> lstValues = new List<BSGridValueBase>();
            lstValues.Add(AddValue("FOREIGNKEY", 12));

            // Identifier, DisplayName et Description
            lstValues.Add(AddValue("IDENTIFIER", 4));
            lstValues.Add(AddValue("DISPLAYNAME", 8));
            lstValues.Add(AddValue("DESCRIPTION", 12));

            // Validity, External link
            lstValues.Add(AddValue("DTENABLED", 2));
            lstValues.Add(AddValue("EXTLLINK", 10));
            lstValues.Add(AddValue("DTDISABLED", 2));
            lstValues.Add(AddValue("EXTLLINK2", 10));
            return lstValues;

        }
        public BSGridTag AddCol(Nullable<int> pSize)
        {
            return AddCol(pSize, null);
        }
        public BSGridTag AddCol(Nullable<int> pSize, Nullable<int> pOffsetSize)
        {
            BSGridCol col = new BSGridCol();

            col.smSpecified = pSize.HasValue;
            if (pSize.HasValue)
                col.sm = pSize.Value.ToString();

            col.osmSpecified = pOffsetSize.HasValue;
            if (pOffsetSize.HasValue)
                col.osm = pOffsetSize.Value.ToString();
            return col;
        }
        public BSGridValueBase AddValue(string pColumnName)
        {
            return AddValue(pColumnName, null, null);
        }
        public BSGridValueBase AddValue(string pColumnName, Nullable<int> pSize)
        {
            return AddValue(pColumnName, pSize, null);
        }
        public BSGridValueBase AddValue(string pColumnName, Nullable<int> pSize, Nullable<int> pOffsetSize)
        {
            return new BSGridValue(pColumnName, pSize, pOffsetSize);
        }


        /// <summary>
        /// Création d'un grid (pour une liste de colonnes)
        /// </summary>
        public static BSGridTag CreateContent(List<ReferentialColumn> pListRrc)
        {
            return CreateContent(pListRrc, BSPanelTypeEnum.heading, null);
        }
        public static BSGridTag CreateContent(List<ReferentialColumn> pListRrc, BSPanelTypeEnum pHRPanelType, Nullable<int> pSize)
        {
            BSGridTag retBSGridTag = null;
            List<ReferentialColumn> lstRrcWithBlock = pListRrc.Where(column => ArrFunc.IsFilled(column.html_BLOCK)).ToList();

            List<BSGridTag> lstCols = new List<BSGridTag>();
            List<BSGridValueBase> lstValues = new List<BSGridValueBase>();
            // Construction sur la base des blocks
            if (null != lstRrcWithBlock)
            {
                BSGridTabs tabs = new BSGridTabs(false, false);
                List<BSGridSection> lstSections = new List<BSGridSection>();
                lstRrcWithBlock.ForEach(block =>
                {
                    lstValues = new List<BSGridValueBase>();
                    lstCols = new List<BSGridTag>();

                    // Détermination des colonnes appartenant à la section par Range
                    // Start = Index de la colonne avec le Block en cours
                    // End  = Index du prochain block ou index de la dernière colonne si plus de block
                    // Les colonnes candidates seront un Range(Start, End - Start)
                    int start = pListRrc.FindIndex(item => item.BSColumnName == block.BSColumnName);
                    int end = pListRrc.Count;
                    if ((lstRrcWithBlock.IndexOf(block) + 1) < lstRrcWithBlock.Count)
                        end = pListRrc.FindIndex(item => item.BSColumnName == lstRrcWithBlock[lstRrcWithBlock.IndexOf(block) + 1].BSColumnName);

                    List<ReferentialColumn> lstRrcCandidates = pListRrc.GetRange(start, end - start).ToList();

                    int nbCol = 12;
                    if (pSize.HasValue)
                    {
                        nbCol = pSize.Value;
                    }
                    else
                    {
                        if (block.html_BLOCK[0].columnbyrowSpecified)
                        {
                            nbCol = Convert.ToInt32(Math.Floor(Convert.ToDecimal(block.html_BLOCK[0].columnbyrow / 2)));
                            nbCol = 12 / Math.Max(1, nbCol);
                        }
                    }

                    List<BSGridTag> colHRb = null;
                    lstRrcCandidates.ForEach(item =>
                    {
                        // Lecture des Sous-titres qui se transformeront en panel
                        if (ArrFunc.IsFilled(item.html_HR))
                        {
                            // Rupture HR (= changement de Panel)
                            if ((null != colHRb) && (0 < colHRb.Count))
                            {
                                if (0 < lstValues.Count)
                                {
                                    // On transfert les colonnes dans le contrôle DIV
                                    BSGridCol  _last =  colHRb.Last() as BSGridCol;
                                    _last.values = lstValues.ToArray();
                                    lstValues.Clear();

                                    if (1 < colHRb.Count)
                                    {
                                        colHRb.Remove(_last);

                                        BSGridCol _newLast = colHRb.Last() as BSGridCol;
                                        List<BSGridTag> _savtags = new List<BSGridTag>();
                                        if (ArrFunc.IsFilled(_newLast.tags))
                                            _savtags.AddRange(_newLast.tags.ToList());
                                        _savtags.Add(_last);
                                        _newLast.tags = _savtags.ToArray();
                                    }

                                    if (null != item.html_HR.First())
                                    {
                                        // Imbrication des BSGridCol matérialisant les HR 
                                        lstCols.AddRange(colHRb);
                                        colHRb.Clear();
                                    }
                                }
                            }

                            int i = 0;
                            item.html_HR.ToList().ForEach(hr =>
                            {
                                if ((null != hr) && StrFunc.IsFilled(hr.title) && ("empty" != hr.title.ToLower()))
                                {
                                    if (null == colHRb)
                                        colHRb = new List<BSGridTag>();

                                    BSGridTag _col = tabs.AddCol(12);
                                    //_col.AddPanel("secondary", (0 == i) ? pHRPanelType : BSPanelTypeEnum.subbasic, hr.title);
                                    _col.AddPanel((0 == i) ? pHRPanelType : BSPanelTypeEnum.subbasic, hr.title);
                                    colHRb.Add(_col);
                                }
                                i++;
                            });
                        }

                        if ((false == item.IsHide) && (false == item.IsForeignKeyField))
                        {
                            lstValues.Add(tabs.AddValue(item.BSColumnName, nbCol));
                        }
                    });


                    if ((null != colHRb) && (0 < colHRb.Count) && (0 < lstValues.Count))
                    {
                        // On transfert les colonnes dans le contrôle DIV
                        BSGridCol _last = colHRb.Last() as BSGridCol;
                        _last.values = lstValues.ToArray();
                        lstValues.Clear();

                        if (1 < colHRb.Count)
                        {
                            colHRb.Remove(_last);

                            BSGridCol _newLast = colHRb.Last() as BSGridCol;
                            List<BSGridTag> _savtags = new List<BSGridTag>();
                            if (ArrFunc.IsFilled(_newLast.tags))
                                _savtags.AddRange(_newLast.tags.ToList());
                            _savtags.Add(_last);
                            _newLast.tags = _savtags.ToArray();
                        }
                        lstCols.AddRange(colHRb);
                        colHRb.Clear();
                    }

                    if ((0 < lstCols.Count) || (0 < lstValues.Count))
                    {
                        BSGridSection section = new BSGridSection("primary", block.html_BLOCK[0].title);
                        section.additionalData = lstRrcCandidates.First().IsAdditionalData;
                        if (0 < lstCols.Count)
                            section.tags = lstCols.ToArray();
                        if (0 < lstValues.Count)
                            section.values = lstValues.ToArray();
                        lstSections.Add(section);
                    }
                });

                if ((null != lstSections) && (0 < lstSections.Count))
                {
                    // On active la 1ere section
                    lstSections[0].activeSpecified = true;
                    lstSections[0].active = true;
                    tabs.sections = lstSections.ToArray();
                    retBSGridTag = tabs;
                }
            }
            else
            {
                BSGridCol colMain = new BSGridCol();
                colMain.AddPanel(BSPanelTypeEnum.heading, "Repository");
                retBSGridTag = colMain;
            }
            return retBSGridTag;
        }

        #endregion Methods

    }
    #endregion BSGridTag
    #region BSGridValueBase
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(BSGridValue))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(BSGridComment))]
    public abstract class BSGridValueBase
    {
        #region Members
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string sm;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool smSpecified;

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string osm;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool osmSpecified;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        protected PlaceHolder _plhContainer;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        protected HtmlGenericControl _colContainer;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        protected BSGridTag _bsgridTagParent;
        #endregion Members
        #region Accessors
        protected bool IsColSizeSpecified
        {
            get { return smSpecified || osmSpecified; }
        }
        protected string BSClass
        {
            get 
            { 
                string bsClass = string.Empty;
                if (IsColSizeSpecified)
                {
                    if (osmSpecified)
                        bsClass = " col-sm-offset-" + osm;
                    if (smSpecified)
                        bsClass += " col-sm-" + sm;
                }
                return bsClass.Trim(); 
            }
        }
        public Control Container
        {
            get 
            {
                if (IsColSizeSpecified) 
                    return _colContainer;
                else 
                    return _plhContainer;
            }
        }
        public BSGridTag BSgridTagParent
        {
            set { _bsgridTagParent = value; }
            get { return _bsgridTagParent; }
        }
        #endregion Accessors
        #region Constructors
        public BSGridValueBase()
        {
            _plhContainer = new PlaceHolder();
        }
        #endregion Constructors
        #region Methods
        public BSGridValueBase AddValue(string pValue)
        {
            return AddValue(pValue, null, null);
        }
        public BSGridValueBase AddValue(string pValue, Nullable<int> pSize)
        {
            return AddValue(pValue, pSize, null);
        }
        public BSGridValueBase AddValue(string pValue, Nullable<int> pSize, Nullable<int> pOffsetSize)
        {
            return new BSGridValue(pValue, pSize, pOffsetSize);
        }
        public virtual void CreateGridSystem()
        {
            CreateGridSystem(null);
        }
        public virtual void CreateGridSystem(BSGridTag pBSGridTagParent)
        {
        }
        #endregion Methods

    }
    #endregion BSGridValueBase

    #region BSGridCol
    /// <summary>
    /// Représente une colonne du Grid System
    /// Matérialisée par _control (BSGridTag) : <div class="col-smx col-sm-offset-x"></div>
    /// </summary>
    public class BSGridCol : BSGridTag
    {
        #region Members
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string sm;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool smSpecified;

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string osm;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool osmSpecified;

        [System.Xml.Serialization.XmlElementAttribute("col", typeof(BSGridCol), Form = XmlSchemaForm.Unqualified)]
        [System.Xml.Serialization.XmlElementAttribute("common", typeof(BSGridCommon), Form = XmlSchemaForm.Unqualified)]
        [System.Xml.Serialization.XmlElementAttribute("row", typeof(BSGridRow), Form = XmlSchemaForm.Unqualified)]
        [System.Xml.Serialization.XmlElementAttribute("tabs", typeof(BSGridTabs), Form = XmlSchemaForm.Unqualified)]
        public BSGridTag[] tags;

        [System.Xml.Serialization.XmlElementAttribute("value", typeof(BSGridValue), Form = XmlSchemaForm.Unqualified)]
        [System.Xml.Serialization.XmlElementAttribute("comment", typeof(BSGridComment), Form = XmlSchemaForm.Unqualified)]
        public BSGridValueBase[] values;
        #endregion Members
        #region Accessors
        protected bool IsAttributeClassSpecified
        {
            get { return smSpecified || osmSpecified; }
        }
        #endregion Accessors

        #region Constructors
        public BSGridCol() : base()
        {
        }
        public BSGridCol(Nullable<int> pSize, Nullable<int> pOffsetSize) : base()
        {
            smSpecified = pSize.HasValue;
            if (smSpecified)
                sm = pSize.Value.ToString();
            osmSpecified = pOffsetSize.HasValue;
            if (osmSpecified)
                osm = pOffsetSize.Value.ToString();
        }
        #endregion Constructors

        #region Methods
        private void SetColClass()
        {
            if (IsAttributeClassSpecified)
            {
                string cssClass = string.Empty;
                if (osmSpecified)
                    cssClass = "col-sm-offset-" + osm;
                if (smSpecified)
                    cssClass += " col-sm-" + sm;
                _control.Attributes.Add("class", cssClass.Trim());
            }
        }
        public override void CreateGridSystem(BSGridTag pGridTagParent)
        {
            if (IsVisible)
            {
                BsGridTagParent = pGridTagParent;

                if (pnlSpecified)
                {
                    pnl.CreatePanel();
                    _control.Controls.Add(pnl.Container);

                    if ((pnl.type == BSPanelTypeEnum.basic) || (pnl.type == BSPanelTypeEnum.subbasic))
                    {
                        _control.Style.Add(HtmlTextWriterStyle.MarginTop, "5px");

                    }
                }

                // Affectation de la class pour la colonne (size + offset)
                SetColClass();

                // HtmlTag
                if (ArrFunc.IsFilled(tags))
                {
                    List<BSGridTag> lst = tags.Cast<BSGridTag>().ToList();
                    lst.ForEach(tag =>
                    {
                        tag.CreateGridSystem(this);
                        ControlContentBody.Controls.Add(tag.Control);
                    });
                }

                // Values | Comments
                if (ArrFunc.IsFilled(values))
                {
                    List<BSGridValueBase> lst = values.Cast<BSGridValueBase>().ToList();
                    lst.ForEach(value =>
                    {
                        value.CreateGridSystem(this);
                        if ((BsGridTagParent is BSGridCommon) && (value is BSGridValue))
                        {
                            bool isContentFooter = ("DTENABLED|DTDISABLED|EXTLLINK|EXTLLINK2".Contains((value as BSGridValue).Value));
                            if (isContentFooter)
                                BsGridTagParent.ControlContentFooter.Controls.Add(value.Container);
                            else if (this.pnlSpecified)
                                ControlContentBody.Controls.Add(value.Container);
                            else
                                BsGridTagParent.ControlContentBody.Controls.Add(value.Container);
                        }
                        else
                        {
                            ControlContentBody.Controls.Add(value.Container);
                        }
                    });
                }
            }
        }

        #endregion Methods

    }
    #endregion BSGridCol

    #region BSGridPanel
    /// <summary>
    /// Représente un panel (Header, Body Footer) avec collapsing possible
    /// Matérialisée par _panel : Header et Footer sont optionnels
    /// <div class="panel">
    ///     <div class="panel-heading">
    ///     </div>
    ///     <div class="panel-body">
    ///     </div>
    ///     <div class="panel-footer">
    ///     </div>
    /// </div>
    /// </summary>
    public class BSGridPanel
    {
        #region Members
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public BSPanelTypeEnum type;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string color;

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string basiccolor;

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string title;

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public bool collapse;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool collapseSpecified;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        protected HtmlGenericControl _panel;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        protected PlaceHolder _container;
        #endregion Members

        #region Accessors
        private bool isCollapse
        {
            get { return collapseSpecified && collapse; }
        }
        public HtmlGenericControl Heading
        {
            get
            {
                HtmlGenericControl ctrl = null;
                switch (type)
                {
                    case BSPanelTypeEnum.heading:
                    case BSPanelTypeEnum.full:
                        ctrl = (HtmlGenericControl)_panel.Controls[0];
                        break;
                    default:
                        break;
                }
                return ctrl;
            }
        }
        public HtmlGenericControl PanelBody
        {
            get
            {
                HtmlGenericControl ctrl = null;
                switch (type)
                {
                    case BSPanelTypeEnum.basic:
                    case BSPanelTypeEnum.subbasic:
                    case BSPanelTypeEnum.body:
                    case BSPanelTypeEnum.footer:
                        ctrl = (HtmlGenericControl)_panel.Controls[0];
                        break;
                    case BSPanelTypeEnum.heading:
                    case BSPanelTypeEnum.full:
                        if (isCollapse)
                            ctrl = (HtmlGenericControl)_panel.Controls[1].Controls[0];
                        else
                            ctrl = (HtmlGenericControl)_panel.Controls[1];
                        break;
                    default:
                        break;
                }
                return ctrl;
            }
        }
        public HtmlGenericControl PanelFooter
        {
            get
            {
                HtmlGenericControl ctrl = null;
                switch (type)
                {
                    case BSPanelTypeEnum.footer:
                        ctrl = (HtmlGenericControl)_panel.Controls[1];
                        break;
                    case BSPanelTypeEnum.full:
                        ctrl = (HtmlGenericControl)_panel.Controls[2];
                        break;
                    default:
                        break;
                }
                return ctrl;
            }
        }
        public HtmlGenericControl Panel
        {
            get { return _panel; }
        }
        public PlaceHolder Container
        {
            get {return _container;}
        }
        #endregion Accessors

        public BSGridPanel()
        {
            //color = "primary";
            type = BSPanelTypeEnum.heading;
            title = "Title";
        }

        public BSGridPanel(BSPanelTypeEnum pType, string pTitle) : this(null, pType, pTitle) { }

        public BSGridPanel(string pColor, BSPanelTypeEnum pType, string pTitle)
        {
            color = pColor;
            type = pType;
            title = pTitle;
            _container = new PlaceHolder();
        }

        public void CreatePanel()
        {
            string finalTitle = string.Empty;
            if (StrFunc.IsFilled(title))
            {
                List<string> lstTitles = BSGridTag.RessourceTitles(title);
                lstTitles.ForEach(item =>
                {
                    finalTitle += item + " ";
                });
            }

            _container = new PlaceHolder();
            if ((type == BSPanelTypeEnum.basic) || (type == BSPanelTypeEnum.subbasic))
            {
                HtmlGenericControl span = new HtmlGenericControl("span");
                string spanClass = "reptitle";
                if (StrFunc.IsFilled(basiccolor))
                    spanClass += " " + basiccolor;

                if (type == BSPanelTypeEnum.basic) 
                    span.InnerHtml = "<span class='glyphicon glyphicon-list'></span> ";
                else
                    spanClass += " lvl2";

                span.InnerHtml += finalTitle;
                span.Attributes.Add("class", spanClass);

                _container.Controls.Add(span);
            }

            _panel = new HtmlGenericControl("div");
            _panel.Attributes.Add("class", "panel " + color);

            HtmlGenericControl pnlChild;

            if ((type == BSPanelTypeEnum.full) || (type == BSPanelTypeEnum.heading))
            {
                pnlChild = new HtmlGenericControl("div");
                pnlChild.Attributes.Add("class", "panel-heading");

                if (StrFunc.IsFilled(title))
                {
                    if (isCollapse)
                    {
                        HtmlAnchor a = new HtmlAnchor();
                        a.HRef = "#mc_" + title;
                        a.Attributes.Add("data-toggle", "collapse");
                        a.InnerText = finalTitle;
                        pnlChild.Controls.Add(a);
                    }
                    else
                    {
                        pnlChild.InnerText = finalTitle;
                    }
                }
                _panel.Controls.Add(pnlChild);
            }


            if (isCollapse)
            {
                HtmlGenericControl pnlCollapse = new HtmlGenericControl("div");
                pnlCollapse.ID = title.Replace("|",string.Empty);
                pnlCollapse.Attributes.Add("class", "panel-collapse collapse");

                pnlChild = new HtmlGenericControl("div");
                pnlChild.Attributes.Add("class", "panel-body");
                pnlCollapse.Controls.Add(pnlChild);


                if ((type == BSPanelTypeEnum.full) || (type == BSPanelTypeEnum.footer))
                {
                    pnlChild = new HtmlGenericControl("div");
                    pnlChild.Attributes.Add("class", "panel-footer clearfix");
                    pnlCollapse.Controls.Add(pnlChild);
                }

                _panel.Controls.Add(pnlCollapse);
            }
            else
            {
                pnlChild = new HtmlGenericControl("div");
                pnlChild.Attributes.Add("class", "panel-body");
                _panel.Controls.Add(pnlChild);

                if ((type == BSPanelTypeEnum.full) || (type == BSPanelTypeEnum.footer))
                {
                    pnlChild = new HtmlGenericControl("div");
                    pnlChild.Attributes.Add("class", "panel-footer clearfix");
                    _panel.Controls.Add(pnlChild);
                }
            }

            _container.Controls.Add(_panel);

        }
    }
    #endregion BSGridPanel
    #region BSGridRow
    /// <summary>
    /// Représente une rangée du Grid System
    /// Matérialisée par _control (BSGridTag) : <div class="row"></div>
    /// </summary>
    public class BSGridRow : BSGridTag
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("col", typeof(BSGridCol), Form = XmlSchemaForm.Unqualified)]
        [System.Xml.Serialization.XmlElementAttribute("common", typeof(BSGridCommon), Form = XmlSchemaForm.Unqualified)]
        public BSGridTag[] tags;

        [System.Xml.Serialization.XmlElementAttribute("value", typeof(BSGridValue), Form = XmlSchemaForm.Unqualified)]
        [System.Xml.Serialization.XmlElementAttribute("comment", typeof(BSGridComment), Form = XmlSchemaForm.Unqualified)]
        public BSGridValueBase[] values;

        #endregion Members
        #region Constructors
        public BSGridRow()
            : base()
        {
            _control.Attributes.Add("class", "row");
        }
        #endregion Constructors
        #region Methods
        public override void CreateGridSystem(BSGridTag pGridTagParent)
        {
            if (IsVisible)
            {
                BsGridTagParent = pGridTagParent;

                if (pnlSpecified)
                {
                    pnl.CreatePanel();
                    //_control.Controls.Add(pnl.Panel);
                    _control.Controls.Add(pnl.Container);
                }

                //HtmlTag
                if (ArrFunc.IsFilled(tags))
                {
                    List<BSGridTag> lst = tags.Cast<BSGridTag>().ToList();
                    lst.ForEach(tag =>
                    {
                        tag.CreateGridSystem(this);
                        ControlContentBody.Controls.Add(tag.Control);
                    });
                }
            }
        }

        #endregion Methods
    }
    #endregion BSGridRow
    #region BSGridSection
    /// <summary>
    /// Représente des onglets (Tabs) dynamiques
    /// </summary>
    public class BSGridSection : BSGridTag
    {
        #region Members
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public bool active;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool activeSpecified;

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public bool additionalData;

        [System.Xml.Serialization.XmlElementAttribute("col", typeof(BSGridCol), Form = XmlSchemaForm.Unqualified)]
        [System.Xml.Serialization.XmlElementAttribute("external", typeof(BSGridExternal), Form = XmlSchemaForm.Unqualified)]
        [System.Xml.Serialization.XmlElementAttribute("item", typeof(BSGridItem), Form = XmlSchemaForm.Unqualified)]
        [System.Xml.Serialization.XmlElementAttribute("role", typeof(BSGridRole), Form = XmlSchemaForm.Unqualified)]
        [System.Xml.Serialization.XmlElementAttribute("row", typeof(BSGridRow), Form = XmlSchemaForm.Unqualified)]
        [System.Xml.Serialization.XmlElementAttribute("tabs", typeof(BSGridTabs), Form = XmlSchemaForm.Unqualified)]
        public BSGridTag[] tags;

        [System.Xml.Serialization.XmlElementAttribute("value", typeof(BSGridValue), Form = XmlSchemaForm.Unqualified)]
        [System.Xml.Serialization.XmlElementAttribute("comment", typeof(BSGridComment), Form = XmlSchemaForm.Unqualified)]
        public BSGridValueBase[] values;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        protected HtmlGenericControl _controlBody;
        #endregion Members

        #region Accessors
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool IsActive
        {
            get { return activeSpecified && active; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool IsAdditionalData
        {
            get { return additionalData; }
        }

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public override Control ControlContentBody
        {
            get {return _controlBody;}
        }

        private string BodyClass
        {
            get {

                string bodyClass = "tab-pane fade";
                if (IsActive)
                    bodyClass += " active in";

                if (IsAdditionalData)
                    bodyClass += " ad-ovf";

                if ((BsGridTagParent as BSGridTabs).IsWithSeparator)
                    bodyClass += " sep";
                return bodyClass.Trim();
            }
        }

        #endregion Accessors

        public BSGridSection()
        {
            _control = new HtmlGenericControl("li");
            _controlBody = new HtmlGenericControl("div");
        }
        public BSGridSection(string pTitle): this()
        {
            title = pTitle;
            id = title.Replace(" ", "_");
        }

        public BSGridSection(string pColor, string pTitle):this()
        {
            color = pColor;
            title = pTitle;
            id = title.Replace(" ", "_");
        }
        private string SetSectionTitle()
        {
            string finalTitle = string.Empty;
            // Titres de la section en cours (le séparateur de titre utilisé est "|")
            List<string> lstCurrentTitles = BSGridTag.RessourceTitles(title);
            if (null != lstCurrentTitles)
            {
                lstCurrentTitles.ForEach(item =>
                {
                    if (pnlSpecified)
                        item = item.Replace(pnl.title, string.Empty);
                    else if (null != BsGridTagParent)
                    {
                        if (BsGridTagParent.pnlSpecified)
                            item = item.Replace(BsGridTagParent.pnl.title, string.Empty);
                        else if ((null != BsGridTagParent.BSGridTagParent) && (BsGridTagParent.BSGridTagParent is BSGridSection))
                        {
                            BSGridSection sectionParent = BsGridTagParent.BSGridTagParent as BSGridSection;
                            List<string> lstParentTitles = BSGridTag.RessourceTitles(sectionParent.title);
                            lstParentTitles.ForEach(itemParent =>
                            {
                                item = item.Replace(itemParent, string.Empty);
                            });
                        }
                    }
                    finalTitle += item + " - ";
                });
                finalTitle = finalTitle.Remove(finalTitle.Length - 3);
                finalTitle = finalTitle.Trim().First().ToString().ToUpper() + finalTitle.Trim().Substring(1);
            }
            return finalTitle;
        }
        public override void CreateGridSystem(BSGridTag pGridTagParent)
        {
            if (IsVisible)
            {
                BsGridTagParent = pGridTagParent;

                // Titre de la section
                if (IsActive)
                    _control.Attributes.Add("class", "active");

                HtmlAnchor anchor = new HtmlAnchor();
                anchor.Attributes.Add("data-toggle", "tab");
                anchor.HRef = "#mc_" + id;
                anchor.InnerText = SetSectionTitle();
                _control.Controls.Add(anchor);

                // Corps de la section
                _controlBody.ID = id;
                _controlBody.Attributes.Add("class", BodyClass);



                if (ArrFunc.IsFilled(tags))
                {
                    List<BSGridTag> lst = tags.Cast<BSGridTag>().ToList();
                    lst.ForEach(tag =>
                    {
                        tag.CreateGridSystem(this);
                        _controlBody.Controls.Add(tag.Control);
                    });
                }

                if (ArrFunc.IsFilled(values))
                {
                    List<BSGridValueBase> lst = values.Cast<BSGridValueBase>().ToList();
                    lst.ForEach(value =>
                    {
                        value.CreateGridSystem(this);
                        ControlContentBody.Controls.Add(value.Container);
                    });
                }
            }
        }
    }
    #endregion BSGridSection
    #region BSGridTabs
    /// <summary>
    /// Représente des onglets (Tabs) dynamiques
    /// </summary>
    public class BSGridTabs : BSGridTag
    {
        #region Members
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public bool justified;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool justifiedSpecified;

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public bool sep;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool sepSpecified;

        [System.Xml.Serialization.XmlElementAttribute("section", typeof(BSGridSection), Form = XmlSchemaForm.Unqualified)]
        public BSGridSection[] sections;

        [System.Xml.Serialization.XmlElementAttribute("item", typeof(BSGridItem), Form = XmlSchemaForm.Unqualified)]
        [System.Xml.Serialization.XmlElementAttribute("role", typeof(BSGridRole), Form = XmlSchemaForm.Unqualified)]
        [System.Xml.Serialization.XmlElementAttribute("external", typeof(BSGridExternal), Form = XmlSchemaForm.Unqualified)]
        public BSGridTemplate[] template;

        #endregion Members

        #region Accessors
        public bool IsWithSeparator
        {
            get { return sepSpecified && sep; }
        }
        private bool IsJustified
        {
            get { return justifiedSpecified && justified;}
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public override Control Control
        {
            get 
            { 
              
                return _control; 
            }
        }
        #endregion Accessors

        public BSGridTabs()
        {
            _control = new HtmlGenericControl("div");
        }

        public BSGridTabs(bool pIsJustified, bool pIsWithSeparator)
            : this(null, pIsJustified, pIsWithSeparator)
        {
        }

        public BSGridTabs(string pColor, bool pIsJustified, bool pIsWithSeparator):this()
        {
            color = pColor;
            justifiedSpecified = pIsJustified;
            justified = justifiedSpecified;
            sepSpecified = pIsWithSeparator;
            sep = sepSpecified;
        }

        #region Methods
        public override void CreateGridSystem(BSGridTag pGridTagParent)
        {
            if (IsVisible)
            {
                BsGridTagParent = pGridTagParent;

                // Heading contient les titres des sections Tabs
                HtmlGenericControl divHeading = new HtmlGenericControl("div");
                divHeading.Attributes.Add("class", "panel-heading");

                // Contenu des sections de Tabs
                HtmlGenericControl divBody = new HtmlGenericControl("div");
                divBody.Attributes.Add("class", "panel-body");

                HtmlGenericControl divContent = new HtmlGenericControl("div");
                divContent.Attributes.Add("class", "tab-content " + color);

                // Titre principal
                if (StrFunc.IsFilled(title))
                {
                    HtmlGenericControl span = new HtmlGenericControl("span");
                    span.Attributes.Add("class", "glyphicon glyphicon-th-list");
                    span.InnerText = " " + Ressource.GetString(title, true);
                    divHeading.Controls.Add(span);
                }
                HtmlGenericControl ul = new HtmlGenericControl("ul");
                ul.Attributes.Add("class", "nav nav-tabs" + (IsJustified ? " nav-justified" : string.Empty));

                if (null != template)
                {
                    template.Cast<BSGridTemplate>().ToList().ForEach(section =>
                    {
                        // Le template est composé de sections qui viendront s'ajouter à celles qui suivent (tag : sections)
                        section.CreateGridSystem(this);
                    });
                }

                // Titre et contenu des sections Tabs
                sections.Cast<BSGridSection>().ToList().ForEach(section =>
                {
                    // Section
                    section.CreateGridSystem(this);
                    ul.Controls.Add(section.Control);
                    divContent.Controls.Add(section.ControlContentBody);
                });


                divHeading.Controls.Add(ul);
                _control.Controls.Add(divHeading);
                divBody.Controls.Add(divContent);
                _control.Controls.Add(divBody);
                _control.Attributes.Add("class", "panel sph-nav-tabs " + color);
            }
        }
        #endregion Methods

    }
    #endregion BSGridTabs
    #region BSGridValue
    /// <summary>
    /// Représente une colonne du référenriel
    /// </summary>
    public class BSGridValue : BSGridValueBase
    {
        #region Members
        [System.Xml.Serialization.XmlTextAttribute(DataType = "normalizedString")]
        public string Value;
        #endregion Members

        #region Constructors
        public BSGridValue() : base()
        {
        }
        public BSGridValue(string pValue): this(pValue, null, null)
        {
        }
        public BSGridValue(string pValue, Nullable<int> pSize, Nullable<int> pOffsetSize)
            : this()
        {
            Value = pValue;
            smSpecified = pSize.HasValue;
            if (smSpecified)
                sm = pSize.Value.ToString();
            osmSpecified = pOffsetSize.HasValue;
            if (osmSpecified)
                osm = pOffsetSize.Value.ToString();
        }
        #endregion Constructors
        #region Methods
        public override void CreateGridSystem(BSGridTag pBSgridTagParent)
        {
            _bsgridTagParent = pBSgridTagParent;
            // PlaceHolder container des contrôles associés à la colonne
            _plhContainer.ID = Value;

            if (false == IsColSizeSpecified)
            {
                if (_bsgridTagParent is BSGridCol)
                {
                    BSGridCol col = _bsgridTagParent as BSGridCol;
                    smSpecified = true;
                    sm = "12";
                    if (col.smSpecified && ("12" != col.sm))
                    {
                        smSpecified = true;
                        sm = "12";
                    }
                }
            }
            if (IsColSizeSpecified)
            {
                _colContainer = new HtmlGenericControl("div");
                _colContainer.Attributes.Add("class", BSClass);
                _colContainer.Controls.Add(_plhContainer);
            }
        }
        #endregion Methods
    }
    #endregion BSGridValue
    #region BSGridComment
    /// <summary>
    /// Représente un commentaire
    /// </summary>
    public class BSGridComment : BSGridValueBase
    {
        #region Members
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string title;

        [System.Xml.Serialization.XmlAttributeAttribute("data-content")]
        public string dataContent;

        [System.Xml.Serialization.XmlTextAttribute(DataType = "normalizedString")]
        public string Comment;
        #endregion Members

        #region Constructors
        public BSGridComment()
            : base()
        {
        }
        public BSGridComment(string pComment) : this(pComment, null, null, null, null)
        {
        }
        public BSGridComment(string pComment, string pTitle, string pDataContent, Nullable<int> pSize, Nullable<int> pOffsetSize)
            : this()
        {
            Comment = pComment;
            title = pTitle;
            dataContent = pDataContent;
            smSpecified = pSize.HasValue;
            if (smSpecified)
                sm = pSize.Value.ToString();
            osmSpecified = pOffsetSize.HasValue;
            if (osmSpecified)
                osm = pOffsetSize.Value.ToString();
        }
        #endregion Constructors
        #region Methods
        public override void CreateGridSystem(BSGridTag pBSgridTagParent)
        {
            _bsgridTagParent = pBSgridTagParent;
            // PlaceHolder container des contrôles associés au commentaire
            _plhContainer.ID = Comment + ";" + dataContent + ";" + title;

            if (false == IsColSizeSpecified)
            {
                if (_bsgridTagParent is BSGridCol)
                {
                    BSGridCol col = _bsgridTagParent as BSGridCol;
                    smSpecified = true;
                    sm = "12";
                    if (col.smSpecified && ("12" != col.sm))
                    {
                        smSpecified = true;
                        sm = "12";
                    }
                }
            }
            if (IsColSizeSpecified)
            {
                _colContainer = new HtmlGenericControl("div");
                _colContainer.Attributes.Add("class", BSClass);
                _colContainer.Controls.Add(_plhContainer);
            }
        }
        #endregion Methods
    }
    #endregion BSGridComment

    #region BSGridCommon
    /// <summary>
    /// Représente la partie commune à tout référentiel
    /// Identifier, DisplayName, Description
    /// DtEnabled, DtDisabled
    /// ExtlLink1, ExtlLink2
    /// </summary>
    public class BSGridCommon : BSGridTag
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("col", typeof(BSGridCol), Form = XmlSchemaForm.Unqualified)]
        [System.Xml.Serialization.XmlElementAttribute("row", typeof(BSGridRow), Form = XmlSchemaForm.Unqualified)]
        [System.Xml.Serialization.XmlElementAttribute("tabs", typeof(BSGridTabs), Form = XmlSchemaForm.Unqualified)]
        public BSGridTag[] tags;

        [System.Xml.Serialization.XmlElementAttribute("value", typeof(BSGridValue), Form = XmlSchemaForm.Unqualified)]
        [System.Xml.Serialization.XmlElementAttribute("comment", typeof(BSGridComment), Form = XmlSchemaForm.Unqualified)]
        public BSGridValueBase[] values;
        #endregion Members
        #region Accessors
        private bool IsUseCommonTemplate
        {
            get { return ArrFunc.IsEmpty(tags) && ArrFunc.IsEmpty(values); }
        }
        #endregion Accessors
        #region Constructors
        public BSGridCommon()
        {
        }
        #endregion Constructors


        #region Methods
        public void CreateCommon(string pIdMenu)
        {
            
            //AddPanel(ControlsTools.MainMenuName(pIdMenu), BSPanelTypeEnum.full, "Characteristics");
            AddPanel(BSPanelTypeEnum.full, "Characteristics");
            List<BSGridValueBase> lst = AddCommonValue();
            values = lst.ToArray();
        }
        public override void CreateGridSystem(BSGridTag pGridTagParent)
        {
            if (IsVisible)
            {
                BsGridTagParent = pGridTagParent;


                if (IsUseCommonTemplate)
                {
                    // Création complète du panel commun
                    AddPanel(BSPanelTypeEnum.full, "Characteristics");
                    pnl.CreatePanel();
                    _control.Controls.Add(pnl.Container);

                    List<BSGridValueBase> lst = AddCommonValue();
                    lst.ForEach(value =>
                    {
                        value.CreateGridSystem(this);
                        BSGridValue val = value as BSGridValue;
                        if ("DTENABLED|DTDISABLED|EXTLLINK|EXTLLINK2".Contains(val.Value))
                            ControlContentFooter.Controls.Add(value.Container);
                        else
                            ControlContentBody.Controls.Add(value.Container);
                    });
                }
                else
                {
                    if (pnlSpecified)
                    {
                        pnl.CreatePanel();
                        _control.Controls.Add(pnl.Container);
                    }
                    if (ArrFunc.IsFilled(tags))
                    {
                        List<BSGridCol> lst = tags.Cast<BSGridCol>().ToList();

                        lst.ForEach(col =>
                        {
                            col.CreateGridSystem(this);
                            ControlContentBody.Controls.Add(col.Control);
                        });
                    }
                    if (ArrFunc.IsFilled(values))
                    {
                        List<BSGridValueBase> lst = values.Cast<BSGridValueBase>().ToList();

                        lst.ForEach(value =>
                        {
                            value.CreateGridSystem(this);
                            BSGridValue val = value as BSGridValue;
                            if ((null != val) && "DTENABLED|DTDISABLED|EXTLLINK|EXTLLINK2".Contains(val.Value))
                                ControlContentFooter.Controls.Add(value.Container);
                            else
                                ControlContentBody.Controls.Add(value.Container);
                        });
                    }
                }
            }
        }
        #endregion Methods

    }
    #endregion BSGridCommon

    #region BSGridTemplate
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(BSGridItem))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(BSGridRole))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(BSGridExternal))]
    public abstract class BSGridTemplate : BSGridTag
    {
        #region Members
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string sm;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool smSpecified;

        [System.Xml.Serialization.XmlElementAttribute("col", typeof(BSGridCol), Form = XmlSchemaForm.Unqualified)]
        [System.Xml.Serialization.XmlElementAttribute("row", typeof(BSGridRow), Form = XmlSchemaForm.Unqualified)]
        [System.Xml.Serialization.XmlElementAttribute("tabs", typeof(BSGridTabs), Form = XmlSchemaForm.Unqualified)]
        public BSGridTag[] tags;

        [System.Xml.Serialization.XmlElementAttribute("value", typeof(BSGridValue), Form = XmlSchemaForm.Unqualified)]
        [System.Xml.Serialization.XmlElementAttribute("comment", typeof(BSGridComment), Form = XmlSchemaForm.Unqualified)]
        public BSGridValueBase[] values;
        #endregion Members
        #region Accessors
        protected bool IsUseTemplate
        {
            get { return ArrFunc.IsEmpty(tags) && ArrFunc.IsEmpty(values); }
        }
        protected virtual List<ReferentialColumn> LstCandidates
        {
            get {return null;}
        }
        protected virtual string Title
        {
            get { return title; }
        }
        protected virtual int ColSize
        {
            get 
            {
                int size = 12;
                if (smSpecified)
                    size = Convert.ToInt32(sm);
                return size; 
            }
        }
        #endregion Accessors
        #region Constructors
        public BSGridTemplate()
        {
        }
        #endregion Constructors


        #region Methods
        public virtual void SetControl(List<BSGridValueBase> pLstValues)
        {
            pLstValues.ForEach(value =>
            {
                value.CreateGridSystem(this);
                BSGridValue val = value as BSGridValue;
                ControlContentBody.Controls.Add(value.Container);
            });
        }
        public virtual void CreateTemplate(List<BSGridTag> pLstBSGridTag, BSGridTag pBSGridTag)
        {
            if ((null != LstColumns) && (0 < LstColumns.Count))
            {
                List<ReferentialColumn> lst = LstCandidates;
                if ((null != lst) && (0 < lst.Count))
                {
                    BSGridTag _tag = BSGridTag.CreateContent(LstCandidates, BSPanelTypeEnum.basic, ColSize);

                    if (pBSGridTag is BSGridTabs)
                    {
                        BSGridTabs tabs = pBSGridTag as BSGridTabs;

                        List<BSGridSection> lstSections = new List<BSGridSection>();
                        if (null != tabs.sections)
                            lstSections.AddRange(tabs.sections.ToList());
                        if (_tag is BSGridTabs)
                        {
                            List<BSGridSection> lstNewSection = (_tag as BSGridTabs).sections.ToList();
                            if (0 < lstSections.Count)
                                lstNewSection.ForEach(section => section.active = false);
                            lstSections.AddRange(lstNewSection);
                        }
                        else
                        {
                            //BSGridSection section = new BSGridSection(lstSections.First().color, EFS.ACommon.Ressource.GetMultiEmpty(Title));
                            BSGridSection section = new BSGridSection(EFS.ACommon.Ressource.GetMultiEmpty(Title));
                            section.tags = new BSGridTag[1] { _tag };
                            lstSections.Add(section);
                        }

                        // Les sections pour les données additionnelles ont désormais pour container le tabs (pBSGridTabs) 
                        tabs.sections = lstSections.ToArray();
                    }
                    else if (null != pLstBSGridTag)
                    {
                        pLstBSGridTag.Add(_tag);
                    }
                    else
                    {
                        tags = new BSGridTag[1]{_tag};
                    }
                }

            }
        }
        public override void CreateGridSystem(BSGridTag pGridTagParent)
        {
            if (IsVisible)
            {
                BsGridTagParent = pGridTagParent;

                if (IsUseTemplate)
                {
                    // Création des valeurs
                    CreateTemplate(null, BsGridTagParent);
                }
                else
                {
                    if (pnlSpecified)
                    {
                        pnl.CreatePanel();
                        _control.Controls.Add(pnl.Container);
                    }
                }

                if (ArrFunc.IsFilled(tags))
                {
                    List<BSGridTag> lst = tags.Cast<BSGridTag>().ToList();

                    lst.ForEach(col =>
                    {
                        col.CreateGridSystem(this);
                        ControlContentBody.Controls.Add(col.Control);
                    });
                }

                if (ArrFunc.IsFilled(values))
                {
                    SetControl(values.Cast<BSGridValueBase>().ToList());
                }
            }
        }
        #endregion Methods
    }
    #endregion BSGridTemplate


    #region BSGridExternal
    public class BSGridExternal : BSGridTemplate
    {
        #region Accessors
        protected override List<ReferentialColumn> LstCandidates
        {
            get
            {
                List<ReferentialColumn> lst = null;
                if (null != LstColumns)
                    lst = LstColumns.Where(column => column.IsExternal).ToList();
                return lst;
            }
        }
        protected override string Title
        {
            get
            { 
                if (StrFunc.IsFilled(title))
                    return title;
                else
                    return "ExternalInfos"; }
        }
        #endregion Accessors

        #region Constructors
        public BSGridExternal(): base()
        {
        }
        #endregion Constructors
    }
    #endregion BSGridExternal
    #region BSGridItem
    public class BSGridItem : BSGridTemplate
    {
        #region Accessors
        protected override List<ReferentialColumn> LstCandidates
        {
            get
            {
                List<ReferentialColumn> lst = null;
                if (null != LstColumns)
                    lst = LstColumns.Where(column => column.IsItem).ToList();
                return lst;
            }
        }
        #endregion Accessors

        #region Constructors
        public BSGridItem(): base()
        {
        }
        #endregion Constructors
    }
    #endregion BSGridItem
    #region BSGridRole
    public class BSGridRole : BSGridTemplate
    {
        #region Accessors
        protected override List<ReferentialColumn> LstCandidates
        {
            get
            {
                List<ReferentialColumn> lst = null;
                if (null != LstColumns)
                    lst = LstColumns.Where(column => column.IsRole).ToList();
                return lst;
            }
        }
        protected override string Title
        {
            get
            {
                if (StrFunc.IsFilled(title))
                    return title;
                else
                    return "Role";
            }
        }
        #endregion Accessors

        #region Constructors
        public BSGridRole()
            : base()
        {
        }
        #endregion Constructors
    }
    #endregion BSGridRole

}