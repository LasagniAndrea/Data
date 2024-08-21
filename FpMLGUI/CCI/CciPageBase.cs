using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;

using System.Text.RegularExpressions;

using System.Xml;
//
using EFS.ACommon;
using EFS.Common;
using EFS.Common.Web;
using EFS.ApplicationBlocks.Data;
using System.Runtime.InteropServices;

namespace EFS.GUI.CCI
{
    /// <summary>
    /// Description résumée de CciPageBase.
    /// </summary>
    // EG 20180423 Analyse du code Correction [CA1405]
    [ComVisible(false)]
    public abstract class CciPageBase : PageBase
    {
        #region Enums
        /// <summary>
        /// 
        /// </summary>
        private enum NodeElementEnum
        {
            screen,
            @object,
        }

        /// <summary>
        /// 
        /// </summary>
        public enum ScreenNodeTypeEnum
        {
            screen,
            screenquickinput,
            control,
            objet,
            table,
            tradeextends,
            other,
        }

        /// <summary>
        /// Gestion du focus 
        /// </summary>
        public enum FocusModeEnum
        {
            /// <summary>
            /// Focus déterminé par les CCI
            /// </summary>
            Forced,
            /// <summary>
            /// Focus par défaut (géré par l'explorer web)
            /// </summary>
            Normal,
        }

        #endregion Enums
        //
        #region Members
        /// <summary>
        /// 
        /// </summary>
        private readonly Stack m_StackInstanceObject;
        /// <summary>
        /// 
        /// </summary>
        protected XmlNode m_AddRemoveNode;
        /// <summary>
        /// 
        /// </summary>
        protected Cst.OperatorType m_AddRemoveOperatorType;
        /// <summary>
        /// 
        /// </summary>
        protected string m_AddRemovePrefix;
        #endregion Members
        //
        #region Accessors
        /// <summary>
        /// Obtient le folder spécifique à la saisie en cours
        /// <para>Ce folder doit exister sous le répertoire Objects et Screens</para>
        /// </summary>
        protected virtual string FolderName
        {
            get { return "VirtualName"; }
        }

        /// <summary>
        /// 
        /// </summary>
        protected virtual InputGUI InputGUI
        {
            get { return null; }
        }

        /// <summary>
        /// Représente l'objet géré via les ccis
        /// </summary>
        protected virtual ICustomCaptureInfos Object
        {
            get { return null; }
        }

        /// <summary>
        /// 
        /// </summary>
        protected virtual string ScreenName
        {
            get
            {
                return "VirtualScreenName";
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// FI 20160804 [Migration TFS] Modify
        protected virtual string XML_FilesPath
        {
           // get { return @"XML_Files\CciPageBase"; }
            get { return @"CCIML\CciPageBase"; }
        }

        /// <summary>
        /// 
        /// </summary>
        public override bool IsDebugDesign
        {
            get
            {
                return BoolFunc.IsTrue(Request.QueryString["isDebugDesign"]);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public override bool IsDebugClientId
        {
            get
            {
                return BoolFunc.IsTrue(Request.QueryString["isDebugClientId"]);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        protected virtual bool IsExtend
        {
            get { return false; }
        }

        /// <summary>
        /// Retoure true si la gestion des objets array s'effectue dynamiquement
        /// <para></para>
        /// </summary>
        protected virtual bool IsNewModeForArray
        {
            get { return false; }
        }

        /// <summary>
        /// Retoure true lorsque les id des controls de la page sont générés dynamiquement, cela permet de garantir l'unicité 
        /// <para>
        /// Exemple si l'id d'un object Table dans le fichier descriptif vaut Tbl et que cette table se trouve dans un object graphique de party
        /// Alors les ids générés pour les 2 tables seront tradeHeader_party1_Tbl et tradeHeader_party2_Tbl
        /// 
        /// </para>
        /// </summary>
        ///FI 20100203 [] 
        protected virtual bool IsDynamicId
        {
            get { return false; }
        }

        /// <summary>
        /// Obtient le context [utilisé pour la recherche des objets de design]
        /// </summary>
        protected virtual string ObjectContext
        {
            get
            {
                //Petite astuce il est possible de placer objectContext dans lURL (pour le debug)
                return Request.QueryString["objectContext"];
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public FocusModeEnum FocusMode
        {
            get;
            private set;
        }

        /// <summary>
        /// Reourne true si la publication courante est la 2ème publication d'un contrôle autocomplete
        /// <para>Ce double publication se produit sous tout browser Chromium (Chrome ou Microsoft Edge), lorsque l'utilisateur sélectionne une donnée proposée par l'autocomplete en utilisant la souris</para>
        /// <para>L'usage de IsDblPostback permet de ne pas refaire les choses 2 fois</para>
        /// </summary>
        /// FI 20200402 [XXXXX] Add
        public Boolean IsDblPostbackFromAutocompleteControl
        {
            get;
            private set;
        }

        #endregion Accessors
        //
        #region Constructor
        public CciPageBase()
        {
            m_StackInstanceObject = new Stack();
            FocusMode = FocusModeEnum.Forced;
        }
        #endregion Constructor
        //
        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        /// FI 20200402 [XXXXX] Add override
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            SetIsDblPostbackFromAutocompleteControl();
        }
        #region methods
        /// <summary>
        /// Méthode qui permet de détecter que la publication courante est la 2nd publication d'un contrôle autocomplete 
        /// <para>La double publication n'a lieu que sur chrome lorsque l'uitlisateur utilise la souris</para>
        /// </summary>
        /// FI 20200402 [XXXXX] Add method
        // EG 20200828 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc) Control instead of WebControl
        private void SetIsDblPostbackFromAutocompleteControl()
        {
            if (StrFunc.IsFilled(PARAM_EVENTTARGET) && (null != Request.Form[PARAM_EVENTTARGET]))
            {
                bool isAutoComplete = false;
                if (FindControl(PARAM_EVENTTARGET) is Control control)
                {
                    if (control is WebControl control1)
                        isAutoComplete = control1.CssClass.Contains("autocomplete");
                    else if (control is HtmlControl control2)
                        isAutoComplete = control2.Attributes["class"].Contains("autocomplete");
                }
                if (isAutoComplete)
                {
                    Tuple<string, string, string> postbakcInfo = new Tuple<string, string, string>(PARAM_EVENTTARGET,
                        StrFunc.IsFilled(PARAM_EVENTTARGET) && (null != Request.Form[PARAM_EVENTTARGET]) ?
                        Request.Form[PARAM_EVENTTARGET] : string.Empty, PARAM_EVENTARGUMENT);
                    Tuple<string, string, string> lastpostbakcInfo = DataCache.GetData<Tuple<string, string, string>>(GUID + "PostBackInfo");
                    if (null != lastpostbakcInfo)
                        IsDblPostbackFromAutocompleteControl = lastpostbakcInfo.Equals(postbakcInfo);
                    if (IsDblPostbackFromAutocompleteControl)
                        System.Diagnostics.Debug.Print("IsDblPostback detected");
                    DataCache.SetData<Tuple<string, string, string>>(GUID + "PostBackInfo", postbakcInfo);
                }
            }
        }

        /// <summary>
        /// Retourne true si ScreenName est déclaré dans le fichier XML descriptif des écrans et objets
        /// </summary>
        /// <returns></returns>
        /// FI 201211 [18224] refactoring
        public bool IsScreenAvailable()
        {
            //Warning: This method is called by an InvokeMember
            if (null == InputGUI.DocXML)
                LoadDocXml();

            string searchScreen = GetXPath_Screen(ScreenName);
            XmlNode node = InputGUI.DocXML.SelectSingleNode(searchScreen);
            bool ret = (null != node);

            return ret;
        }

        /// <summary>
        /// Retourne le CustomObjectButtonReferential associé au cci identifié par pClientId
        /// <para>Retourne null s'il n'existe pas de bonton associé</para>
        /// </summary>
        /// <param name="pClientId"></param>
        /// <returns></returns>
        public CustomObjectButtonReferential GetCustomObjectButtonReferential(string pClientId)
        {
            CustomObjectButtonReferential ret = null;
            // FI 20180502 [23926] Instruction Link
            CustomObjectButtonReferential[] array = InputGUI.GetCustomObjectButtonReferential();
            // FI 20180614 [XXXXX] Add test (null != array)
            if (null != array)
                ret = array.Where(x => x.ClientId == pClientId).FirstOrDefault();
            return ret;
        }

        /// <summary>
        /// Obtient le contrôle server qui stocke les contrôles servers associés au ccis
        /// <para>Cette property doit obligatoirement overridée</para>
        /// </summary>
        public virtual PlaceHolder PlaceHolder
        {
            get { return null; }
        }

        /// <summary>
        /// 
        /// </summary>
        protected virtual void EraseTable() { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pTableSetting"></param>
        protected virtual void CreateTable(TableSettings pTableSetting) { }

        /// <summary>
        /// 
        /// </summary>
        protected virtual void WriteTable() { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCo"></param>
        /// <param name="pTypecontrol"></param>
        /// <param name="pParentOccurs"></param>
        /// <param name="pControl">Retourne le contrôle ajouté</param>
        /// FI 20121126 [18224] add parameter pControl 
        protected virtual void WriteControl(CustomObject pCo, CustomObject.ControlEnum pTypecontrol, int pParentOccurs, out Control pControl)
        {
            pControl = null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pRoot"></param>
        protected virtual void AddExtends(ref XmlNode pRoot)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCo"></param>
        /// <param name="pNode"></param>
        /// <param name="pParentClientId"></param>
        /// <param name="pParentOccurs"></param>
        /// <returns></returns>
        protected virtual int InitializeNodeObjectArray(CustomObject pCo, XmlNode pNode, string pParentClientId, int pParentOccurs)
        {
            return 1;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCo"></param>
        /// <param name="pNode"></param>
        /// <param name="pPrefix"></param>
        /// <returns></returns>
        protected virtual int InitializeNodeObjectArray2(CustomObject pCo, XmlNode pNode, string pPrefix)
        {
            return 1;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pClientId"></param>
        /// <returns></returns>
        protected virtual Array GetArrayElement(string pClientId)
        {
            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pClientId"></param>
        /// <param name="pParent"></param>
        /// <returns></returns>
        protected virtual Array GetArrayElement(string pClientId, out object pParent)
        {
            pParent = null;
            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pArray"></param>
        /// <param name="Parent"></param>
        /// <returns></returns>
        protected virtual bool RemoveLastItemInArrayElement(Array pArray, Object Parent)
        {
            return false;
        }

        /// <summary>
        /// Purge la collection customCaptureInfos, chargement de l'écran et alimentation de la collection en fonction du descriptif de l'écran
        /// </summary>
        /// <returns></returns>
        protected virtual bool LoadScreen()
        {
            AddAuditTimeStep("Start CciPageBase.LoadScreen");

            Object.CcisBase.Reset();

            bool isOk = (StrFunc.IsFilled(ScreenName));
            if (isOk)
            {
                LoadDocXml();
                isOk = IsScreenAvailable();
                if (isOk)
                    SearchObjectScreen();
            }

            AddAuditTimeStep("End CciPageBase.LoadScreen");
            return isOk;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        /// FI 201211 [18224] refactoring
        protected bool LoadSubScreen()
        {
            bool isOk = StrFunc.IsFilled(ScreenName);
            if (isOk)
                SearchObjectScreen();
            return isOk;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        /// FI 201211 [18224] refactoring
        protected virtual void SearchObjectScreen()
        {
            AddAuditTimeStep("Start CciPageBase.SearchObjectScreen");

            if ((null != InputGUI) && (null != InputGUI.DocXML))
            {
                string searchScreen = GetXPath_Screen(ScreenName);
                XmlNode nodeScreen = InputGUI.DocXML.SelectSingleNode(searchScreen);

                if (null != nodeScreen)
                {
                    if (XMLTools.ExistNodeAttribute(nodeScreen, "focus"))
                    {
                        string focus = XMLTools.GetNodeAttribute(nodeScreen, "focus");
                        FocusMode = (FocusModeEnum)Enum.Parse(typeof(FocusModeEnum), focus, true);
                    }

                    if (nodeScreen.HasChildNodes)
                        SearchObject(nodeScreen, string.Empty);
                }
            }

            AddAuditTimeStep("End CciPageBase.SearchObjectScreen");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pRootObject"></param>
        /// <param name="pQuickClientId"></param>
        protected void SearchObject(XmlNode pRootObject, string pQuickClientId)
        {
            SearchObject(pRootObject, pQuickClientId, string.Empty, 0);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pRootObject"></param>
        /// <param name="pQuickClientId"></param>
        /// <param name="pParentClientId"></param>
        /// <param name="pParentOccurs"></param>
        protected void SearchObject(XmlNode pRootObject, string pQuickClientId, string pParentClientId, int pParentOccurs)
        {
            bool isParentToDisplay = true;
            SearchObject(pRootObject, pQuickClientId, pParentClientId, pParentOccurs, ref isParentToDisplay);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pRootObject"></param>
        /// <param name="pQuickClientId"></param>
        /// <param name="pParentClientId"></param>
        /// <param name="pParentOccurs"></param>
        /// <param name="pIsParentToDisplay"></param>
        /// EG 20170918 [23342] New ControlEnum.timestamp
        // EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        protected void SearchObject(XmlNode pRootObject, string pQuickClientId, string pParentClientId, int pParentOccurs, ref bool pIsParentToDisplay)
        {
            //AddAuditTimeStep("Start CciPageBase.SearchObject - " + pRootObject.Name);

            XmlNode root = pRootObject;
            XmlNode nodeInstance = null;
            //ScreenNodeTypeEnum nodeType = ScreenNodeTypeEnum.other;
            //
            bool isInstanceObj = false;
            CustomObject co = null;
            int firstIndex = 1;
            int occurs = 1;
            int minOccurs = -1;
            bool isExistItemArray = false;
            //bool isFirstIndexSpecified = false;
            bool isDesignButtonItemArray = false;
            bool isRootObject_TableWithOnlyExtendsChild = false;
            bool isRootTable_ToDisplay = true;
            string xPathItemArray = string.Empty;
            bool bModeQuick = StrFunc.IsFilled(pQuickClientId);

            ScreenNodeTypeEnum nodeType  = GetNodeType(pRootObject.Name, out XmlNode nodeObject);

            switch (nodeType)
            {
                case ScreenNodeTypeEnum.table:
                    #region table
                    if (!bModeQuick)
                    {
                        if (root.HasChildNodes && root.ChildNodes.Count == 1)
                        {
                            if (null != root.SelectSingleNode("TradeExtends"))
                                isRootObject_TableWithOnlyExtendsChild = true;
                        }
                        //                            
                        TableSettings tblSettings = GetTableSettings(root, string.Empty, pParentOccurs);
                        CreateTable(tblSettings);
                    }
                    #endregion table
                    break;
                case ScreenNodeTypeEnum.objet:
                    #region Object
                    isInstanceObj = XMLTools.ExistNodeAttribute(root, "clientid");
                    // S'il n'y a pas de d'attribut clientId ds une balise  => Ds ce cas c'est juste une redirection vers La balise Object Correspondante
                    if (isInstanceObj)
                    {
                        nodeInstance = root;
                        co = new CustomObject();
                        InitializeCustomObjetInstanceObj(co, nodeInstance);
                        //
                        isExistItemArray = co.ContainsOccurs;
                        //20100115 [Chantier Strategy], Gestion dynamique des arrays via Reflection (voir InitializeNodeObjectArray2)
                        if (isExistItemArray && (false == IsNewModeForArray))
                        {
                            // 25052007 => Ajout test sur Firstindex pour remedier à un plantage sur ecran returnSwap
                            // Les ecrans ou ils existe Firstindex dans un array ne sont pas dynamiquement dessinés en consultation
                            bool isFirstIndexSpecified = StrFunc.IsFilled(XMLTools.GetNodeAttribute(nodeInstance, "firstindex"));
                            //
                            if (isFirstIndexSpecified)
                                occurs = int.Parse(XMLTools.GetNodeAttribute(nodeInstance, "occurs"));
                            else //if (false == this.isNewModeForArray)
                                occurs = InitializeNodeObjectArray(co, nodeInstance, pParentClientId, pParentOccurs);
                            //
                            isDesignButtonItemArray = XMLTools.ExistNodeAttribute(nodeInstance, "addsuboperator") && BoolFunc.IsTrue(XMLTools.GetNodeAttribute(nodeInstance, "addsuboperator"));
                            if (isDesignButtonItemArray)
                                xPathItemArray = CciPageBase.GetXpath(nodeInstance);
                            //
                            try
                            {
                                if (null != nodeInstance.Attributes.GetNamedItem("minoccurs"))
                                    minOccurs = int.Parse(XMLTools.GetNodeAttribute(nodeInstance, "minoccurs"));
                                if (null != nodeInstance.Attributes.GetNamedItem("firstindex"))
                                    firstIndex = int.Parse(XMLTools.GetNodeAttribute(nodeInstance, "firstindex"));
                                else
                                    firstIndex = 1;

                            }
                            catch
                            { firstIndex = 1; occurs = 1; isExistItemArray = false; isDesignButtonItemArray = false; }
                        }
                    }
                    // redirection vers ce Node
                    //root = GetNodeObject(root.Name);
                    //FI 20121203 [18224] use of variable nodeObject
                    root = nodeObject; 
                    #endregion Object
                    break;
                case ScreenNodeTypeEnum.screenquickinput:
                    #region ScreenQuickInput
                    string screen = XMLTools.GetNodeAttribute(root, "screen");
                    string xPath = XMLTools.GetNodeAttribute(root, "xpath");
                    string ClientIdQuick = screen + xPath;

                    // EG 20240123 [WI816] Trade input: Modification of periodic fees uninvoiced on a trade
                    CustomObjectTextBox coQuick = new CustomObjectTextBox
                    {
                        ClientId = ClientIdQuick,
                        PostBack = "true",
                        DataType = TypeData.TypeDataEnum.@string.ToString(),
                        Required = "true",
                        IsLockedModifyPostEvts = true,
                        IsLockedModifyFeesUninvoiced = true,
                        IsLockedAllocatedInvoice = true,
                        RegEx = EFSRegex.TypeRegex.None.ToString(),
                        Control = CustomObject.ControlEnum.quickinput
                    };

                    CustomCaptureInfo cciScreenQuickInput;
                    cciScreenQuickInput = new CustomCaptureInfo(coQuick.CtrlClientId, coQuick.IsMandatory, coQuick.DataTypeEnum, coQuick.IsEnabled, coQuick.RegexValue);
                    if (null != root.Attributes.GetNamedItem("separator"))
                        cciScreenQuickInput.QuickSeparator = XMLTools.GetNodeAttribute(root, "separator", false);
                    if (null != root.Attributes.GetNamedItem("format"))
                        cciScreenQuickInput.QuickFormat = XMLTools.GetNodeAttribute(root, "format");
                    //
                    Object.CcisBase.Add(cciScreenQuickInput);
                    //
                    // 20090819 RD / XML est case sensitive, la fonction XPATH "translate" sert à faire la transformation en lower 
                    string xpath2 = @"/Root/Screens/Screen[normalize-space(translate(@name,'ABCDEFGHIJKLMNOPQRSTUVWXYZ','abcdefghijklmnopqrstuvwxyz'))='" + screen.ToLower() + @"']";
                    //
                    if (StrFunc.IsFilled(xPath))
                        xpath2 = xpath2 + @"/" + xPath;
                    //
                    XmlNode ScreenNode = InputGUI.DocXML.SelectSingleNode(xpath2);  // redirection vers ce Node	
                    //
                    if (null != ScreenNode)
                        SearchObject(ScreenNode, coQuick.ClientId);
                    //
                    //FI 20121126 add controlQuickInput
                    WriteControl(coQuick, CustomObject.ControlEnum.quickinput, pParentOccurs, out _);

                    #endregion ScreenQuickInput
                    break;
                case ScreenNodeTypeEnum.control:
                    #region control
                    // EG 20170918 [23342] add CustomObject.ControlEnum.timestamp
                    CustomObject.ControlEnum typeControl = (CustomObject.ControlEnum)System.Enum.Parse(typeof(CustomObject.ControlEnum), root.Name, true);

                    CustomObject coCtrl;
                    switch (typeControl)
                    {
                        case CustomObject.ControlEnum.label:
                            coCtrl = new CustomObjectLabel();
                            break;
                        case CustomObject.ControlEnum.display:
                            coCtrl = new CustomObjectDisplay();
                            break;
                        case CustomObject.ControlEnum.textbox:
                        case CustomObject.ControlEnum.quickinput:
                            coCtrl = new CustomObjectTextBox();
                            break;
                        case CustomObject.ControlEnum.timestamp:
                            coCtrl = new CustomObjectTimestamp();
                            break;
                        case CustomObject.ControlEnum.button:
                            coCtrl = NewButton(root);
                            InitializeCusTomObjectButton(coCtrl, root);
                            break;
                        case CustomObject.ControlEnum.image:
                            coCtrl = new CustomObjectImage();
                            break;
                        case CustomObject.ControlEnum.dropdown:
                        case CustomObject.ControlEnum.optgroupdropdown: // 20090909 RD / DropDownList avec OptionGroup
                        case CustomObject.ControlEnum.htmlselect:
                            coCtrl = new CustomObjectDropDown();
                            break;
                        case CustomObject.ControlEnum.banner:
                            coCtrl = new CustomObjectBanner();
                            break;
                        case CustomObject.ControlEnum.buttonbanner:
                            coCtrl = new CustomObjectButtonBanner();
                            break;
                        case CustomObject.ControlEnum.ddlbanner:
                            coCtrl = new CustomObjectDDLBanner();
                            break;
                        case CustomObject.ControlEnum.openbanner:
                            coCtrl = new CustomObjectOpenBanner();
                            break;
                        case CustomObject.ControlEnum.rowheader:
                            coCtrl = new CustomObjectRowHeader();
                            break;
                        case CustomObject.ControlEnum.rowfooter:
                            coCtrl = new CustomObjectRowFooter();
                            break;
                        case CustomObject.ControlEnum.hyperlink:
                            coCtrl = new CustomObjectHyperLink();
                            break;
                        case CustomObject.ControlEnum.panel:
                            coCtrl = new CustomObjectPanel();
                            break;
                        default:
                            coCtrl = new CustomObject();
                            break;
                    }
                    //
                    InitializeCustomObjet(coCtrl, root, typeControl);

                    //FI 20121126 [18224] add variable control
                    Control control = null;
                    if (!bModeQuick)
                        WriteControl(coCtrl, typeControl, pParentOccurs, out control);

                    //FI 20121126 [18224] Appel à la méthode AddCci
                    if (coCtrl.IsControlData)
                        AddCci(coCtrl, typeControl, root, bModeQuick, pQuickClientId);
                    
                    //FI 20130926
                    if (control != null)
                    {
                        //FI 20121126 [18224] Appel à la méthode AddCciControl
                        AddCciControl(coCtrl, typeControl, control);
                    }

                    #endregion control
                    break;
                case ScreenNodeTypeEnum.tradeextends:
                    #region TradeExtends
                    if (IsExtend)
                    {
                        if (root.HasChildNodes)
                        {
                            for (int i = root.ChildNodes.Count - 1; i >= 0; i--)
                                root.RemoveChild(root.ChildNodes[i]);
                        }

                        AddExtends(ref root);

                        if (root.HasChildNodes)
                        {
                            isInstanceObj = XMLTools.ExistNodeAttribute(root, "clientid");
                            co = new CustomObject();
                            InitializeCustomObjetInstanceObj(co, root);
                            pParentClientId = co.ClientId;
                        }
                    }
                    pIsParentToDisplay = IsExtend && root.HasChildNodes;
                    #endregion
                    break;
                case ScreenNodeTypeEnum.screen:
                case ScreenNodeTypeEnum.other:
                    // Nothing Poursuite du traitement si  root.HasChildNodes
                    break;
            }
            //
            if (root.HasChildNodes)
            {
                //20100115 [Chantier Strategy], Gestion dynamique des arrays via Reflection (voir InitializeNodeObjectArray2)
                if (isExistItemArray && IsNewModeForArray)
                {
                    // 25052007 => Ajout test sur Firstindex pour remedier à un plantage sur ecran returnSwap
                    // Les ecrans ou ils existe Firstindex dans un array ne sont pas dynamiquement dessinés en consultation
                    bool isFirstIndexSpecified = StrFunc.IsFilled(XMLTools.GetNodeAttribute(nodeInstance, "firstindex"));
                    //
                    if (isFirstIndexSpecified)
                    {
                        occurs = int.Parse(XMLTools.GetNodeAttribute(nodeInstance, "occurs"));
                    }
                    else
                    {
                        string clientIdArray = GetCurrentIntancePrefix() + co.ClientId;
                        occurs = InitializeNodeObjectArray2(co, nodeInstance, clientIdArray);
                    }
                    //
                    isDesignButtonItemArray = XMLTools.ExistNodeAttribute(nodeInstance, "addsuboperator") && BoolFunc.IsTrue(XMLTools.GetNodeAttribute(nodeInstance, "addsuboperator"));
                    if (isDesignButtonItemArray)
                        xPathItemArray = GetXpath(nodeInstance);
                    //
                    try
                    {
                        if (null != nodeInstance.Attributes.GetNamedItem("minoccurs"))
                            minOccurs = int.Parse(XMLTools.GetNodeAttribute(nodeInstance, "minoccurs"));
                        //
                        if (null != nodeInstance.Attributes.GetNamedItem("firstindex"))
                            firstIndex = int.Parse(XMLTools.GetNodeAttribute(nodeInstance, "firstindex"));
                        else
                            firstIndex = 1;
                        //
                    }
                    catch
                    { firstIndex = 1; occurs = 1; isExistItemArray = false; isDesignButtonItemArray = false; }
                }
                //
                for (int i = firstIndex; i < (firstIndex + occurs); i++)
                {
                    int parentOccurs = pParentOccurs;
                    string parentClientId = pParentClientId;
                    //
                    if (isInstanceObj)
                    {
                        if (isExistItemArray)
                        {
                            //  Ex irs1_Toto => si plusieurs stream
                            bool isRequired = co.IsMandatory;
                            //au dela de minoccurs rien ne serait obligatoire
                            if ((minOccurs != -1) && (i > minOccurs))
                                isRequired = false;

                            // EG 20240123 [WI816] Trade input: Modification of periodic fees uninvoiced on a trade
                            CustomObject coItem = new CustomObject(co.ClientId + i.ToString(), isRequired.ToString(), co.PostBack, co.HelpSchema, co.HelpElement, co.HelpUrl, co.KeepInQuickInput)
                            {
                                IsLockedModifyPostEvts = co.IsLockedModifyPostEvts,
                                IsLockedModifyFeesUninvoiced = co.IsLockedModifyFeesUninvoiced,
                                IsLockedAllocatedInvoice = co.IsLockedAllocatedInvoice
                            };
                            m_StackInstanceObject.Push(coItem);

                            parentOccurs = i;
                            parentClientId = co.ClientId;
                        }
                        else
                            m_StackInstanceObject.Push(co);
                    }
                    //
                    foreach (XmlNode child in root.ChildNodes)
                    {
                        //FI 20120615 [17892] 
                        //Pas la peine de descendre sous des noeuds de type documentation ou text 
                        //Petit Tuning pour grapiller des millisecondes
                        if (child.Name != "documentation" & child.Name != "Documentation" & child.Name != "#comment" & child.Name != "#text")
                        {
                            if (isRootObject_TableWithOnlyExtendsChild)
                                SearchObject(child, pQuickClientId, parentClientId, parentOccurs, ref isRootTable_ToDisplay);
                            else
                                SearchObject(child, pQuickClientId, parentClientId, parentOccurs);
                        }
                    }
                    //
                    if (isInstanceObj)
                        m_StackInstanceObject.Pop();
                }
                #region isDesignButtonItemArray
                //
                if ((isDesignButtonItemArray) && (!bModeQuick))
                {
                    string nodeInstanceName_Del = Ressource.GetString("btnCciPageBaseRemoveEmpty_" + nodeInstance.Name, string.Empty);
                    string nodeInstanceName_Add = Ressource.GetString("btnCciPageBaseAdd_" + nodeInstance.Name, string.Empty);
                    //
                    TableSettings tblSettings = GetTableSettings(nodeInstance, "table", pParentOccurs);
                    if (StrFunc.IsEmpty(tblSettings.ControlAlignLeft))
                        tblSettings.ControlAlignLeft = "1";

                    //
                    CreateTable(tblSettings);
                    //Del
                    CustomObjectButtonItemArray CoButDel = new CustomObjectButtonItemArray();
                    InitializeCustomObjet(CoButDel, nodeInstance, CustomObject.ControlEnum.button);
                    CoButDel.Prefix = CoButDel.ClientId;
                    CoButDel.ClientId = CoButDel.Prefix + CustomObject.KEY_SEPARATOR + "Del";
                    CoButDel.XPath = xPathItemArray;
                    CoButDel.CssClass = "fas fa-caret-left";

                    if (StrFunc.IsFilled(nodeInstanceName_Del))
                        CoButDel.ToolTip = nodeInstanceName_Del;
                    else
                        CoButDel.ToolTip = Ressource.GetString("btnCciPageBaseRemoveEmpty");
                    //
                    CoButDel.OperatorType = Cst.OperatorType.substract.ToString();
                    if (occurs == 0)
                        CoButDel.Style = "display:none";
                    WriteControl(CoButDel, CustomObject.ControlEnum.button, pParentOccurs, out _);
                    //Addd
                    CustomObjectButtonItemArray CoButAdd = new CustomObjectButtonItemArray();
                    InitializeCustomObjet(CoButAdd, nodeInstance, CustomObject.ControlEnum.button);
                    CoButAdd.Prefix = CoButAdd.ClientId;
                    CoButAdd.ClientId = CoButAdd.Prefix + CustomObject.KEY_SEPARATOR + "Add";
                    CoButAdd.XPath = xPathItemArray;
                    CoButAdd.CssClass = "fas fa-caret-right";
                    //
                    if (StrFunc.IsFilled(nodeInstanceName_Add))
                        CoButAdd.ToolTip = nodeInstanceName_Add;
                    else
                        CoButAdd.ToolTip = Ressource.GetString("btnCciPageBaseAdd");
                    //
                    CoButAdd.OperatorType = Cst.OperatorType.add.ToString();
                    CoButAdd.Style = "text-align:left";
                    WriteControl(CoButAdd, CustomObject.ControlEnum.button, pParentOccurs, out _);

                    WriteTable();
                }
                #endregion isDesignButtonItemArray
            }
            //
            if ((ScreenNodeTypeEnum.table == nodeType) && (!bModeQuick))
            {
                if (isRootTable_ToDisplay)
                    WriteTable();
                else
                    EraseTable();
            }
            //AddAuditTimeStep("End CciPageBase.SearchObject - " + pRootObject.Name);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pAttrib"></param>
        /// <returns></returns>
        // EG 20240123 [WI816] Trade input: Modification of periodic fees uninvoiced on a trade
        private string GetDynamicAttributs(string pAttrib)
        {
            string ret = string.Empty;

            CustomObject co = null;
            bool isStopLoop = false;
            //	

            IEnumerator ListEnum = m_StackInstanceObject.GetEnumerator();
            while (ListEnum.MoveNext())
            {
                co = (CustomObject)ListEnum.Current;
                switch (pAttrib.ToLower())
                {
                    case "clientid":
                        ret = co.ClientId + CustomObject.KEY_SEPARATOR + ret;   //Ex Fra_notional_Amount
                        break;
                    case "accesskey":
                        isStopLoop = co.ContainsAccessKey; //=> Dès que l'on trouve une instance avec AccessKey => c'est cette info qui sera utilisée
                        break;
                    case "caption":
                        isStopLoop = co.ContainsCaption; //=> Dès que l'on trouve une instance avec Caption => c'est cette info qui sera affichée
                        break;
                    case "helpurl":
                        isStopLoop = co.ContainsHelpUrl; //=> Dès que l'on trouve une instance avec helpUrl => c'est cette aide qui sera affichée
                        break;
                    case "helpschema":
                        isStopLoop = co.ContainsHelpSchema; //=> Dès que l'on trouve une instance avec helpUrl => c'est cette aide qui sera affichée
                        break;
                    case "helpelement":
                        isStopLoop = co.ContainsHelpElement; //=> Dès que l'on trouve une instance avec helpUrl => c'est cette aide qui sera affichée
                        break;
                    case "required":
                        isStopLoop = (!co.IsMandatory); //=>  Dès que l'on trouve une instance non obligatoire => toute les zones sous cette instance sont non obligatoires
                        break;
                    case "islockedmodifypostevts":
                        isStopLoop = (!co.IsLockedModifyPostEvts); //=>  Dès que l'on trouve une instance non lockée => toute les zones sous cette instance sont non lockée
                        break;
                    case "islockedmodifyfeesuninvoiced":
                        isStopLoop = (!co.IsLockedModifyFeesUninvoiced); //=>  Dès que l'on trouve une instance non lockée => toute les zones sous cette instance sont non lockée
                        break;
                    case "islockedallocatedinvoice":
                        isStopLoop = (!co.IsLockedAllocatedInvoice); //=>  Dès que l'on trouve une instance non lockée => toute les zones sous cette instance sont non lockée
                        break;
                    case "postback":
                        isStopLoop = co.IsAutoPostBack; //=>  Dès que l'on trouve une instance postback => toute les zones sous cette instance sont postback
                        break;
                    case "keepinquickinput":
                        isStopLoop = (!co.IsInQuickInput); //=> Dès que l'on trouve une instance exclue d'un quickInput => toute les zones sous cette instance sont exclue du quickInput 
                        break;
                    case "match":
                        isStopLoop = co.ContainsMatch; //=> Dès que l'on trouve une instance avec match 
                        break;

                }
                if (isStopLoop)
                    break;
            }
            //
            if (null != co)
            {
                switch (pAttrib.ToLower())
                {
                    case "clientid":
                        ret = ret.Remove(ret.Length - 1, 1);
                        break;
                    case "accesskey":
                        ret = co.AccessKey;
                        break;
                    case "caption":
                        ret = co.Caption;
                        break;
                    case "helpurl":
                        ret = co.HelpUrl;
                        break;
                    case "helpschema":
                        ret = co.HelpSchema;
                        break;
                    case "helpelement":
                        ret = co.HelpElement;
                        break;
                    case "required":
                        ret = co.Required;
                        break;
                    case "islockedmodifypostevts":
                        ret = co.IsLockedModifyPostEvts.ToString();
                        break;
                    case "islockedmodifyfeesuninvoiced":
                        ret = co.IsLockedModifyFeesUninvoiced.ToString();
                        break;
                    case "islockedallocatedinvoice":
                        ret = co.IsLockedAllocatedInvoice.ToString();
                        break;
                    case "postback":
                        ret = co.PostBack;
                        break;
                    case "keepinquickinput":
                        ret = co.KeepInQuickInput;
                        break;
                    case "match":
                        ret = co.Match;
                        break;

                }
            }
            ret = ret.Trim();
            return ret;

        }

        /// <summary>
        /// Retourne le type de noeud associé à {pNodeName}
        /// </summary>
        /// <param name="pNodeName"></param>
        /// <returns></returns>
        /// FI 20121204 [18224] add 
        protected ScreenNodeTypeEnum GetNodeType(string pNodeName)
        {
            return GetNodeType(pNodeName, out _);
        }

        /// <summary>
        /// Retourne le type de noeud associé à {pNodeName}
        /// </summary>
        /// <param name="pNodeName"></param>
        /// <param name="pNodeObject">Retourne le noeud object s'il existe un noeud object nommé {pNodeName}</param>
        /// <returns></returns>
        /// FI 20121203 [18224] add parameter pNodeObject
        protected ScreenNodeTypeEnum GetNodeType(string pNodeName, out XmlNode pNodeObject)
        {
            pNodeObject = null;
            
            string rootName = pNodeName;
            //
            ScreenNodeTypeEnum ret = ScreenNodeTypeEnum.other;
            if (ScreenNodeTypeEnum.tradeextends.ToString() == rootName.ToLower())
                ret = ScreenNodeTypeEnum.tradeextends;
            else if (ScreenNodeTypeEnum.screen.ToString() == rootName.ToLower())
                ret = ScreenNodeTypeEnum.screen;
            else if (ScreenNodeTypeEnum.table.ToString() == rootName.ToLower())
                ret = ScreenNodeTypeEnum.table;
            else if (ScreenNodeTypeEnum.screenquickinput.ToString() == rootName.ToLower())
                ret = ScreenNodeTypeEnum.screenquickinput;
            //
            // Object ??
            if (ScreenNodeTypeEnum.other == ret)
            {
                XmlNode nodeObject = GetNodeObject(rootName);
                if (null != nodeObject)
                {
                    ret = ScreenNodeTypeEnum.objet;
                    pNodeObject = nodeObject;
                }
            }
            // Control ??
            if (ScreenNodeTypeEnum.other == ret)
            {
                if (Enum.IsDefined(typeof(CustomObject.ControlEnum), rootName.ToLower()))
                    ret = ScreenNodeTypeEnum.control;
            }
            //
            return ret;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private string GetPathObjectsSpecific()
        {
            return Server.MapPath(XML_FilesPath + @"\Objects\" + FolderName);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private string GetPathObjects()
        {
            return Server.MapPath(XML_FilesPath + @"\Objects\");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private string GetPathScreensSpecific()
        {
            return Server.MapPath(XML_FilesPath + @"\Screens\" + FolderName);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private static string GetXPath_Screen(string pScreenName)
        {
            // 20090819 RD / XML est case sensitive, la fonction XPATH "translate" sert à faire la transformation en lower 
            return @"/Root/Screens/Screen[normalize-space(translate(@name,'ABCDEFGHIJKLMNOPQRSTUVWXYZ','abcdefghijklmnopqrstuvwxyz'))='" + pScreenName.ToLower() + @"']";
        }

        /// <summary>
        /// Obtient la syntaxe xPath qui permet de récupérer le noeud Object dont l'attribut name = {pObjectName} et l'attribut context = {pContext}
        /// <para>Si {pContext} est vide, alors  Obtient le syntaxe xPath qui permet d'obtenir le noeud Object dont l'attribut name = {pObjectName} </para>
        /// </summary>
        /// <param name="pObjectName"></param>
        /// <param name="pContext"></param>
        /// <returns></returns>
        private static string GetXPath_Object(string pObjectName, string pContext)
        {
            string predicate = @"(normalize-space(@name)='" + pObjectName + @"')";
            if (StrFunc.IsFilled(pContext))
                predicate += @" and (normalize-space(@context)='" + pContext + @"')";
            else
                predicate += @" and not(@context)";
            return @"/Root/Objects/Object[" + predicate + "]";
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCo"></param>
        /// <param name="pNode"></param>
        /// FI 20140708 [20179] modify
        protected virtual void InitializeCustomObjetInstanceObj(CustomObject pCo, XmlNode pNode)
        {
            CustomObject co = pCo;
            co.ClientId = XMLTools.GetNodeAttribute(pNode, "clientid");
            if (co.ClientId.IndexOf("tradeDate") >= 0)
                co.AccessKey = XMLTools.GetNodeAttribute(pNode, "accesskey");
            co.Caption = XMLTools.GetNodeAttribute(pNode, "caption");
            co.Required = XMLTools.GetNodeAttribute(pNode, "required");
            co.Locked = XMLTools.GetNodeAttribute(pNode, "locked");
            co.PostBack = XMLTools.GetNodeAttribute(pNode, "postback");
            co.HelpSchema = XMLTools.GetNodeAttribute(pNode, "helpschema");
            co.HelpElement = XMLTools.GetNodeAttribute(pNode, "helpelement");
            co.HelpUrl = XMLTools.GetNodeAttribute(pNode, "helpurl");
            co.KeepInQuickInput = XMLTools.GetNodeAttribute(pNode, "keepinquickinput");
            co.Occurs = XMLTools.GetNodeAttribute(pNode, "occurs");

            // FI 20140708 [20179] valorisation de la propriété match uniquement si CaptureMode = Match
            if (Cst.Capture.IsModeMatch(InputGUI.CaptureMode))
                co.Match = XMLTools.GetNodeAttribute(pNode, "match");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pNode"></param>
        /// <returns></returns>
        private static string GetXpath(XmlNode pNode)
        {

            XmlNode node = pNode;
            string xpath = pNode.Name;

            //@"/Root/Objects/Object[normalize-space(@name)='" + pObjectName + @"']"
            while (null != node.ParentNode && node.Name != "Root")
            {
                string xPathParent = node.ParentNode.Name;
                if (("Screen" == node.ParentNode.Name))
                {
                    string nameAttribut = XMLTools.GetNodeAttribute(node.ParentNode, "name");
                    xPathParent = node.ParentNode.Name + "[normalize-space(@name)='" + nameAttribut + @"']";
                }
                else if ("Object" == node.ParentNode.Name)
                {
                    string nameAttribut = XMLTools.GetNodeAttribute(node.ParentNode, "name");
                    string contextAttribut = XMLTools.GetNodeAttribute(node.ParentNode, "context");
                    if (StrFunc.IsFilled(XMLTools.GetNodeAttribute(node.ParentNode, "context")))
                        xPathParent = node.ParentNode.Name + "[normalize-space(@name)='" + nameAttribut + @"' and normalize-space(@context)='" + contextAttribut + "']";
                    else
                        xPathParent = node.ParentNode.Name + "[normalize-space(@name)='" + nameAttribut + @"' and not(@context)]";
                }
                //
                xpath = xPathParent + @"/" + xpath;
                node = node.ParentNode;
            }
            xpath = xpath + "[normalize-space(@clientid)='" + XMLTools.GetNodeAttribute(pNode, "clientid") + @"']";
            return xpath;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCo"></param>
        /// <param name="pNode"></param>
        /// <param name="pTypeControl"></param>
        /// FI 20140708 [20179] modify
        /// EG 20170918 [23342] New CustomObjectTimestamp
        /// EG 20170926 [22374] Upd
        /// EG 20171003 [23452] Add FreeZone
        /// EG 20211220 [XXXXX] Nouvelle gestion des libellés d'un panel (Header) en fonction d'un contrôle dans la saisie
        /// EG 20211220 [XXXXX] Ajout d'un attribut "headlinkinfo" pour gérer l'affichage de la valeur du control dans un libellé présent sur un header de Panel
        // EG 20240123 [WI816] Trade input: Modification of periodic fees uninvoiced on a trade
        private void InitializeCustomObjet(CustomObject pCo, XmlNode pNode, CustomObject.ControlEnum pTypeControl)
        {
            CustomObject co = pCo;
            //			
            #region alimenation dynamique des customscontrols
            co.ClientId = GetDynamicAttributs("clientid");
            if (co.ClientId.IndexOf("tradeDate") >= 0)
                co.AccessKey = GetDynamicAttributs("accesskey");
            co.Caption = GetDynamicAttributs("caption");
            co.HelpSchema = GetDynamicAttributs("helpschema");
            co.HelpElement = GetDynamicAttributs("helpelement");
            co.HelpUrl = GetDynamicAttributs("helpurl");
            co.PostBack = GetDynamicAttributs("postback");
            co.Required = GetDynamicAttributs("required");
            // 20081222 RD 16099 : C'est pour garder la valeur par défaut
            if (m_StackInstanceObject.Count > 0)
            {
                co.IsLockedModifyPostEvts = BoolFunc.IsTrue(GetDynamicAttributs("IsLockedModifyPostEvts"));
                co.IsLockedModifyFeesUninvoiced= BoolFunc.IsTrue(GetDynamicAttributs("IsLockedModifyFeesUninvoiced"));
                co.IsLockedAllocatedInvoice = BoolFunc.IsTrue(GetDynamicAttributs("IsLockedAllocatedInvoice"));
            }
            co.KeepInQuickInput = GetDynamicAttributs("keepinquickinput");

            // FI 20140708 [20179] valorisation de la propriété match uniquement si CaptureMode = Match
            if (Cst.Capture.IsModeMatch(InputGUI.CaptureMode))
                co.Match = GetDynamicAttributs("match");
            #endregion alimentation dynamique des customscontrols
            //
            #region alimentation specifique depuis Node
            // ClienId
            string ClientIdControl = XMLTools.GetNodeAttribute(pNode, "clientid");
            if (StrFunc.IsFilled(ClientIdControl))
            {
                if (co.ContainsClientId)
                    co.ClientId = co.ClientId + CustomObject.KEY_SEPARATOR + ClientIdControl;
                else
                    co.ClientId = ClientIdControl;
            }
            string tmp;
            //
            tmp = XMLTools.GetNodeAttribute(pNode, "accesskey");
            if (StrFunc.IsFilled(tmp))
                co.AccessKey = tmp;
            //
            // Forçage de helpUrl si scecifique sur node object
            tmp = XMLTools.GetNodeAttribute(pNode, "helpschema");
            if (StrFunc.IsFilled(tmp))
            {
                if ("none" == tmp)
                    co.HelpSchema = "none";
                else
                    co.HelpSchema = tmp;
            }
            //
            tmp = XMLTools.GetNodeAttribute(pNode, "helpelement");
            if (StrFunc.IsFilled(tmp))
            {
                if ("none" == tmp)
                    co.HelpElement = "none";
                else
                    co.HelpElement = tmp;
            }
            tmp = XMLTools.GetNodeAttribute(pNode, "helpurl");
            if (StrFunc.IsFilled(tmp))
            {
                if ("none" == tmp)
                    co.HelpUrl = "none";
                else
                    co.HelpUrl = tmp;
            }
            //
            // 20081002 RD Forçage de postback si specifique sur node object
            tmp = XMLTools.GetNodeAttribute(pNode, "postback");
            if (StrFunc.IsFilled(tmp))
                co.PostBack = tmp;
            //
            tmp = XMLTools.GetNodeAttribute(pNode, "required");
            if (StrFunc.IsFilled(tmp))
                co.Required = tmp;
            //
            tmp = XMLTools.GetNodeAttribute(pNode, "locked");
            if (StrFunc.IsFilled(tmp))
                co.Locked = tmp;
            //
            // cssclass
            co.CssClass = XMLTools.GetNodeAttribute(pNode, "cssclass");
            // PostBackEvent
            co.PostBackEvent = XMLTools.GetNodeAttribute(pNode, "postbackevent");
            // PostBackArgument
            co.PostBackMethod = XMLTools.GetNodeAttribute(pNode, "postbackmethod");
            // PostBackArgument
            co.PostBackArgument = XMLTools.GetNodeAttribute(pNode, "postbackargument");
            // style
            co.Style = XMLTools.GetNodeAttribute(pNode, "style");
            //Enabled
            co.Enabled = XMLTools.GetNodeAttribute(pNode, "enabled");
            // RegEx
            co.RegEx = XMLTools.GetNodeAttribute(pNode, "regex");
            // Datatype
            co.DataType = XMLTools.GetNodeAttribute(pNode, "datatype");
            // width
            co.Width = XMLTools.GetNodeAttribute(pNode, "width");
            // RD 20100120 Add height property
            co.Height = XMLTools.GetNodeAttribute(pNode, "height");
            // Colspan
            co.Colspan = XMLTools.GetNodeAttribute(pNode, "colspan");
            // Caption
            tmp = XMLTools.GetNodeAttribute(pNode, "caption");
            if (StrFunc.IsFilled(tmp) && (false == tmp.Equals("undefined")))
                co.Caption = tmp;
            // EG 20091130 : Tooltip & ToolTipTitle ressource
            //ToolTip
            string tooltip = XMLTools.GetNodeAttribute(pNode, "tooltip");
            co.ToolTip = Ressource.GetString(tooltip, tooltip); // +Cst.GetAccessKey(co.AccessKey);
            //ToolTipTitle
            string tooltipTitle = XMLTools.GetNodeAttribute(pNode, "tooltiptitle");
            co.ToolTipTitle = Ressource.GetString(tooltipTitle);
            //Misc
            co.Misc = XMLTools.GetNodeAttribute(pNode, "misc");
            //Attributes
            co.Attributes = XMLTools.GetNodeAttribute(pNode, "attributes");
            //
            co.Control = pTypeControl;
            //
            if (co.Control == CustomObject.ControlEnum.label || co.Control == CustomObject.ControlEnum.display)
            {
                CustomObjectLabel coLabel = (CustomObjectLabel)co;
                coLabel.SpaceBefore = XMLTools.GetNodeAttribute(pNode, "spacebefore");
                coLabel.SpaceAfter = XMLTools.GetNodeAttribute(pNode, "spaceafter");
            }
            //
            if (co.Control == CustomObject.ControlEnum.htmlselect ||
                co.Control == CustomObject.ControlEnum.optgroupdropdown || // 20090909 RD / DropDownList avec OptionGroup
                co.Control == CustomObject.ControlEnum.dropdown)
            {
                CustomObjectDropDown coDropDown = (CustomObjectDropDown)co;
                coDropDown.ListRetrieval = XMLTools.GetNodeAttribute(pNode, "listretrieval");
                coDropDown.RelativeTo = XMLTools.GetNodeAttribute(pNode, "relativeto");
                coDropDown.ReadOnlyMode = XMLTools.GetNodeAttribute(pNode, "readonlymode");
                coDropDown.HeadLinkInfo = XMLTools.GetNodeAttribute(pNode, "headlinkinfo");
            }
            //
            if (co.Control == CustomObject.ControlEnum.image)
            {
                CustomObjectImage coImage = (CustomObjectImage)co;
                coImage.Source = XMLTools.GetNodeAttribute(pNode, "source");
            }
            //
            if (co.Control == CustomObject.ControlEnum.textbox)
            {
                CustomObjectTextBox coTextBox = (CustomObjectTextBox)co;
                coTextBox.TextMode = XMLTools.GetNodeAttribute(pNode, "textMode");
                coTextBox.Rows = XMLTools.GetNodeAttribute(pNode, "rows");
                // FI 20191227 [XXXXX] Alimentation de Autocomplete
                coTextBox.Autocomplete = XMLTools.GetNodeAttribute(pNode, "autocomplete");
                coTextBox.HeadLinkInfo = XMLTools.GetNodeAttribute(pNode, "headlinkinfo");
            }
            // EG 20170918 [23342] New
            if (co.Control == CustomObject.ControlEnum.timestamp)
            {
                CustomObjectTimestamp coTimestamp = (CustomObjectTimestamp)co;
                coTimestamp.AltTime = XMLTools.GetNodeAttribute(pNode, "alttime");
                coTimestamp.ToolTipZone = XMLTools.GetNodeAttribute(pNode, "ttipzone");
                coTimestamp.FreeZone = XMLTools.GetNodeAttribute(pNode, "freezone");
            }


            if (co.Control == CustomObject.ControlEnum.banner)
            {
                CustomObjectBanner coBanner = (CustomObjectBanner)co;
                coBanner.Level = XMLTools.GetNodeAttribute(pNode, "level");
            }
            if (co.Control == CustomObject.ControlEnum.openbanner ||
                co.Control == CustomObject.ControlEnum.buttonbanner ||
                co.Control == CustomObject.ControlEnum.ddlbanner
                )
            {
                ICustomObjectOpenBanner coBanner = (ICustomObjectOpenBanner)co;
                coBanner.Level = XMLTools.GetNodeAttribute(pNode, "level");
                coBanner.TableLinkId = XMLTools.GetNodeAttribute(pNode, "tablelinkid");
                coBanner.StartDisplay = XMLTools.GetNodeAttribute(pNode, "startdisplay");
                //
                if (co.Control == CustomObject.ControlEnum.ddlbanner)
                {
                    CustomObjectDDLBanner coDDLBanner = (CustomObjectDDLBanner)co;
                    coDDLBanner.ListRetrieval = XMLTools.GetNodeAttribute(pNode, "listretrieval");
                    coDDLBanner.RelativeTo = XMLTools.GetNodeAttribute(pNode, "relativeto");
                    coDDLBanner.ReadOnlyMode = XMLTools.GetNodeAttribute(pNode, "readonlymode");
                }
            }
            //
            if (co.Control == CustomObject.ControlEnum.hyperlink)
            {
                CustomObjectHyperLink coHyperLink = (CustomObjectHyperLink)co;
                coHyperLink.NavigateUrl = XMLTools.GetNodeAttribute(pNode, "navigateurl");
                coHyperLink.Arguments = XMLTools.GetNodeAttribute(pNode, "arguments");
                coHyperLink.ImageUrl = XMLTools.GetNodeAttribute(pNode, "imageurl");
            }
            #endregion
            //
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pNode"></param>
        /// <returns></returns>
        /// 20081226 EG Add CustomObjectButtonInputMenu
        /// 20081226 EG Add CustomObjectButton
        /// EG 20120620 invoicing attribute New
        private static CustomObject NewButton(XmlNode pNode)
        {

            if (StrFunc.IsFilled(XMLTools.GetNodeAttribute(pNode, "referential")) ||
                StrFunc.IsFilled(XMLTools.GetNodeAttribute(pNode, "invoicing")) ||
                StrFunc.IsFilled(XMLTools.GetNodeAttribute(pNode, "consultation")))
                return new CustomObjectButtonReferential();
            else if (StrFunc.IsFilled(XMLTools.GetNodeAttribute(pNode, "screenBox")))
                return new CustomObjectButtonScreenBox();
            else if (StrFunc.IsFilled(XMLTools.GetNodeAttribute(pNode, "menu")))
                return new CustomObjectButtonInputMenu();
            else if (StrFunc.IsFilled(XMLTools.GetNodeAttribute(pNode, "object")) ||
                     StrFunc.IsFilled(XMLTools.GetNodeAttribute(pNode, "element")))
                return new CustomObjectButtonFpmlObject();
            else
                return new CustomObjectButton();


        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCo"></param>
        /// <param name="pNode"></param>
        /// <returns></returns>
        /// EG 20120613 BlockUI New
        /// EG 20120620 DisplayMode attribute New
        /// EG 20120620 invoicing attribute New
        private static bool InitializeCusTomObjectButton(CustomObject pCo, XmlNode pNode)
        {
            bool isOk = false;
            try
            {
                CustomObjectButton coBase = (CustomObjectButton)pCo;
                coBase.Title = XMLTools.GetNodeAttribute(pNode, "title");
                coBase.DisplayMode = XMLTools.GetNodeAttribute(pNode, "displaymode");
                coBase.ImageUrl = XMLTools.GetNodeAttribute(pNode, "imageurl");

                //
                if (pCo.GetType().Equals(typeof(CustomObjectButtonReferential)))
                {
                    CustomObjectButtonReferential co = (CustomObjectButtonReferential)pCo;
                    co.Referential = XMLTools.GetNodeAttribute(pNode, "referential");
                    co.Invoicing = XMLTools.GetNodeAttribute(pNode, "invoicing");
                    co.Consultation = XMLTools.GetNodeAttribute(pNode, "consultation");
                    co.Condition = XMLTools.GetNodeAttribute(pNode, "condition");
                    co.SqlColumn = XMLTools.GetNodeAttribute(pNode, "sqlspecifiedfield");
                    co.ClientIdForSqlColumn = XMLTools.GetNodeAttribute(pNode, "idspecifiedfield");
                    // FI 20180502 [23926] Alimentation de FilteredWithSqlColumnOrKeyField
                    co.FilteredWithSqlColumnOrKeyField = XMLTools.GetNodeAttribute(pNode, "filteredwithsqlcolumnorkeyfield");
                    isOk = co.ContainsReferential;
                }
                else if (pCo.GetType().Equals(typeof(CustomObjectButtonScreenBox)))
                {
                    CustomObjectButtonScreenBox co = (CustomObjectButtonScreenBox)pCo;
                    co.ScreenBox = XMLTools.GetNodeAttribute(pNode, "screenBox");
                    co.DialogStyle = XMLTools.GetNodeAttribute(pNode, "dialogStyle");
                    isOk = (co.ContainsScreenBox);
                }
                else if (pCo.GetType().Equals(typeof(CustomObjectButtonFpmlObject)))
                {
                    CustomObjectButtonFpmlObject co = (CustomObjectButtonFpmlObject)pCo;
                    co.Object = XMLTools.GetNodeAttribute(pNode, "object");
                    co.Element = XMLTools.GetNodeAttribute(pNode, "element");
                    co.Occurence = XMLTools.GetNodeAttribute(pNode, "occurence");
                    co.CopyTo = XMLTools.GetNodeAttribute(pNode, "copyto");
                    co.IsZoomOnModeReadOnly = XMLTools.GetNodeAttribute(pNode, "readonly");
                    isOk = (co.ContainsObject || co.ContainsElement);
                }
                else if (pCo.GetType().Equals(typeof(CustomObjectButtonInputMenu)))
                {
                    CustomObjectButtonInputMenu co = (CustomObjectButtonInputMenu)pCo;
                    co.Menu = XMLTools.GetNodeAttribute(pNode, "menu");
                    isOk = co.ContainsMenu;
                }
                else if (pCo.GetType().Equals(typeof(CustomObjectButton)))
                {
                    coBase.OnClick = XMLTools.GetNodeAttribute(pNode, "onclick");
                    coBase.BlockUI = XMLTools.GetNodeAttribute(pNode, "blockUI");
                }

            }
            catch (Exception e)
            {
                isOk = false;
                System.Diagnostics.Debug.WriteLine("InitializeCusTomObjectButton: " + e.Message);
            }
            return isOk;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pPath"></param>
        /// <returns></returns>
        private void LoadObjectsNodes(string pPath)
        {
            AddAuditTimeStep("Start CciPageBase.LoadObjectsNodes");

            XmlNode eltObjects = InputGUI.DocXML.SelectSingleNode(@"/Root/Objects");
            if (null == eltObjects)
                throw new Exception(@"Node /Root/Objects doesn't exist");

            if (Directory.Exists(pPath))
            {
                string[] files = Directory.GetFiles(pPath, "*.xml");
                foreach (string xmlObjFile in files)
                {
                    XmlDocument docObjXml = new XmlDocument();
                    docObjXml.Load(xmlObjFile);
                    XmlNodeList nodeList = docObjXml.SelectNodes(@"/Objects/Object");
                    if (null != nodeList)
                    {
                        for (int i = 0; i < nodeList.Count; i++)
                        {
                            XmlNode impObjects = InputGUI.DocXML.ImportNode(nodeList[i], true);
                            eltObjects.AppendChild(impObjects);
                        }
                    }
                }
            }

            AddAuditTimeStep("End CciPageBase.LoadObjectsNodes");
        }

        /// <summary>
        /// Importe dans DocXML les nodes représentatifs de screen, parmi les fichier *.xml présents dans le répertoire {pPath}
        /// </summary>
        /// <param name="pPath"></param>
        /// <returns></returns>
        private void LoadScreensNodes(string pPath)
        {
            AddAuditTimeStep("Start CciPageBase.LoadScreensNodes");

            XmlNode eltObjects = InputGUI.DocXML.SelectSingleNode(@"/Root/Screens");
            if (null == eltObjects)
                throw new Exception(@"Node /Root/Screens doesn't exist");

            if (Directory.Exists(pPath))
            {
                string[] files = Directory.GetFiles(pPath, "*.xml");
                foreach (string xmlObjFile in files)
                {
                    XmlDocument docObjXml = new XmlDocument();
                    docObjXml.Load(xmlObjFile);
                    XmlNodeList nodeList = docObjXml.SelectNodes(@"/Screens/Screen");
                    if (null != nodeList)
                    {
                        for (int i = 0; i < nodeList.Count; i++)
                        {
                            XmlNode nodeScreen = nodeList[i];
                            XmlNode impObjects = InputGUI.DocXML.ImportNode(nodeScreen, true);
                            eltObjects.AppendChild(impObjects);
                        }
                    }
                }
            }

            AddAuditTimeStep("End CciPageBase.LoadScreensNodes");

        }

        /// <summary>
        /// Charge le Document XML descriptif de l'écran de saisie
        /// <para>Alimente InputGUI.DocXML</para>
        /// </summary>
        /// <returns></returns>
        private void LoadDocXml()
        {
            AddAuditTimeStep("Start CciPageBase.LoadDocXml");

            InputGUI.DocXML = null;
            InputGUI.DocXML = new XmlDocument();

            //Create a Object Root 
            XmlNode newElemRoot = InputGUI.DocXML.CreateNode(XmlNodeType.Element, "Root", "");
            InputGUI.DocXML.AppendChild(newElemRoot);

            //Create a Objects Screen 
            XmlNode newElemScreen = InputGUI.DocXML.CreateNode(XmlNodeType.Element, "Screens", "");
            InputGUI.DocXML.DocumentElement.AppendChild(newElemScreen);
            LoadScreensNodes(GetPathScreensSpecific());

            //Create a Objects element 
            XmlNode newElemObj = InputGUI.DocXML.CreateNode(XmlNodeType.Element, "Objects", "");
            InputGUI.DocXML.DocumentElement.AppendChild(newElemObj);
            // Add childs Element on Objects element
            LoadObjectsNodes(GetPathObjectsSpecific());
            LoadObjectsNodes(GetPathObjects());


            AddAuditTimeStep("End CciPageBase.LoadDocXml");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pNode"></param>
        /// <param name="pPrefix"></param>
        /// <param name="pParentOccurs"></param>
        /// <returns></returns>
        /// FI 20170928 [23452] Modify
        private TableSettings GetTableSettings(XmlNode pNode, string pPrefix, int pParentOccurs)
        {

            string id = XMLTools.GetNodeAttribute(pNode, pPrefix + "id");
            //20100203 FI Génération d'un prefix pour obtenir un id unique 
            //utilisé notamment sur les stratégies
            //if (StrFunc.IsFilled(id) && isDynamicId) 
            //    id = GetCurrentIntancePrefix() + id;
            //20100203 FI a mon avis on peut maintenant supprimer le pParentOccurs 
            //if ((0 < pParentOccurs) && StrFunc.IsFilled(id))
            //    id = id + pParentOccurs.ToString();
            //
            // EG 20100318 
            if (StrFunc.IsFilled(id))
            {
                if (IsDynamicId)
                    id = GetCurrentIntancePrefix() + id;
                else if (0 < pParentOccurs)
                    id += pParentOccurs.ToString();
            }
            TableSettings ret = new TableSettings
            {
                Id = id,
                ColSpan = XMLTools.GetNodeAttribute(pNode, pPrefix + "colspan"),
                RowSpan = XMLTools.GetNodeAttribute(pNode, pPrefix + "rowspan"),
                VerticalAlign = XMLTools.GetNodeAttribute(pNode, pPrefix + "verticalAlign"),
                Style = XMLTools.GetNodeAttribute(pNode, pPrefix + "style"),
                PanelOverFlow = XMLTools.GetNodeAttribute(pNode, pPrefix + "overflow"),
                ControlAlignLeft = XMLTools.GetNodeAttribute(pNode, pPrefix + "controlalignleft"),
                GridLines = XMLTools.GetNodeAttribute(pNode, pPrefix + "gridlines"),
                BorderColor = XMLTools.GetNodeAttribute(pNode, pPrefix + "bordercolor"),
                FixedCol = XMLTools.GetNodeAttribute(pNode, pPrefix + "fixedCol"),
                CellStyle = XMLTools.GetNodeAttribute(pNode, pPrefix + "cellstyle")
            };
            return ret;

        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected string GetCurrentIntancePrefix()
        {

            StrBuilder ret = new StrBuilder(string.Empty);
            //
            if (ArrFunc.IsFilled(m_StackInstanceObject))
            {
                CustomObject[] customObject = new CustomObject[ArrFunc.Count(m_StackInstanceObject)];
                m_StackInstanceObject.CopyTo(customObject, 0);
                for (int i = ArrFunc.Count(customObject) - 1; -1 < i; i--)
                    ret.Append(customObject[i].ClientId + CustomObject.KEY_SEPARATOR.ToString());
            }
            //
            return ret.ToString();

        }

        /// <summary>
        /// Obtient le noeud Object tel que l'attribut name = {pName} 
        /// <para>Prise en compte de objectContext</para>
        /// </summary>
        /// <returns></returns>
        protected XmlNode GetNodeObject(string pName)
        {

            //exemple objectContext= "aa,bb"
            //Spheres® recherche 
            //        Object tel que name = {pName} and context = {aa,bb} 
            //si NOK 
            //        Object tel que name = {pName} and context = {aa}
            //si NOK 
            //        Object tel que name = {pName} and context = {bb}
            //si NOK 
            //        Object tel que name = {pName} uniquement

            XmlNode ret = null;
            //
            if (StrFunc.IsFilled(this.ObjectContext))
            {
                string[] objectContextArray = StrFunc.StringArrayList.StringListToStringArray(this.ObjectContext);
                ret = InputGUI.DocXML.SelectSingleNode(GetXPath_Object(pName, this.ObjectContext));
                if ((null == ret) && ArrFunc.Count(objectContextArray) > 1)
                {
                    for (int i = 0; i < ArrFunc.Count(objectContextArray); i++)
                    {
                        ret = InputGUI.DocXML.SelectSingleNode(GetXPath_Object(pName, objectContextArray[i]));
                        if (null != ret)
                            break;
                    }
                }
            }
            //
            if (null == ret)
                ret = InputGUI.DocXML.SelectSingleNode(GetXPath_Object(pName, string.Empty));

#if DEBUG //FI 20170928 [23452] Add System.Diagnostics.Debug

            if (ret == null)
            {
            
                string pathContext = GetXPath_Object(pName, this.ObjectContext);
                string pathDefault = GetXPath_Object(pName, string.Empty);
                System.Diagnostics.Debug.Print(StrFunc.AppendFormat("Node (path:{0}) not Found  ", (pathContext != pathDefault) ? pathContext : pathDefault));
            }
#endif
    
            return ret;
        }

        /// <summary>
        /// Recherche le contrôle associé au Cci, et affecte l'attribut OnClick de manière à ouvrir le référentiel associé au cci
        /// </summary>
        /// <param name="pCci">CCi associé au contrôle surlequel sera afficher le link</param>
        /// <param name="pTable">Représente le referentiel à afficher</param>
        /// FI 20130228 [XXXXX] add  ASSET_INDEX, ASSET_COMMODITY et ASSET_EQUITY
        /// FI 20130312 [XXXXX] add  ASSET_EXTRDFUND
        /// FI 20140708 [20179] Modify
        /// FI 20140912 [XXXXX] Modify
        // EG 20170125 [Refactoring URL] Upd
        public void SetOpenFormReferential(CustomCaptureInfo pCci, Cst.OTCml_TBL pTable)
        {
            // EG 20170125 [Refactoring URL] Upd
            Cst.OTCml_TBL tableName = pTable;
            CustomCaptureInfo cci = pCci;
            //    
            SQL_TableWithID sqlTable = null;
            if (null != cci.Sql_Table)
                sqlTable = cci.Sql_Table as SQL_TableWithID;
            //
            if (null != sqlTable)
            {
                WebControl control = PlaceHolder.FindControl(pCci.ClientId_Prefix + pCci.ClientId_WithoutPrefix) as WebControl;
                if (pCci.ClientId_Prefix == Cst.DDL)
                {
                    if (control is WCDropDownList2)
                    {
                        WCDropDownList2 ddl = control as WCDropDownList2;
                        if (!ddl.HasViewer)
                            control = null;
                    }
                }
                else if (pCci.ClientId_Prefix == Cst.TXT)
                {
                    if (control is WCTextBox2)
                    {
                        WCTextBox2 txt = control as WCTextBox2;
                        if (false ==
                            (txt.CssClass.Contains(EFSCssClass.Consult) ||
                            (txt.CssClass == "txtCaptureBUY") ||
                            (txt.CssClass == "txtCaptureSELL")
                            ))
                            control = null;
                    }
                }
                //FI 20200124 [XXXXX] Pas de link si le contrôle est utilisé en mode matching
                if ((null != control) && (false == ControlsTools.HasCssMatch(control))) 
                {
                    // FI 20140708 [20179] Mise en place d'un lien sur le click uniquement s'il l'atribut est non renseigné
                    if (null == control.Attributes["onclick"])
                    {
                        // FI 20140912 Utilsation de la méthode partagée SpheresURL.GetURLHyperLink
                        // EG 20170125 [Refactoring URL] Upd
                        string column = OTCmlHelper.GetColunmID(tableName.ToString());

                        //string url = SpheresURL.GetURLHyperLink(new Pair<string, Nullable<Cst.OTCml_TBL>>(column, tableName), sqlTable.Id.ToString(), Cst.ConsultationMode.ReadOnly, Form.ID);

                        Nullable<IdMenu.Menu> idMenu;
                        switch (column)
                        {
                            case "IDT":
                                idMenu = SpheresURL.GetMenu_Trade(column, tableName.ToString());
                                break;
                            case "IDASSET":
                                idMenu = SpheresURL.GetMenu_Asset(column, tableName.ToString());
                                break;
                            case "IDIOELEMENT":
                                idMenu = SpheresURL.GetMenu_IO(tableName.ToString());
                                break;
                            default:
                                idMenu = SpheresURL.GetMenu_Repository(column, sqlTable.Id.ToString());
                                break;
                        }
                        // FI 20170728 [23341] Affichage en mode normal
                        string url = SpheresURL.GetURL(idMenu, sqlTable.Id.ToString(), Cst.ConsultationMode.Normal);
                        if (StrFunc.IsFilled(url))
                            // FI 20191128 [XXXXX] Appel à SetHyperLinkOpenFormReferential2
                            ControlsTools.SetHyperLinkOpenFormReferential2(control, url);
                    }
                }
            }

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="savedState"></param>
        protected override void LoadViewState(object savedState)
        {
            base.LoadViewState(savedState);
            // FI 20200518 [XXXXX] Utilisation de DataCache
            //if (null != Session[this.GUID + "FOCUSMODE"])
            //    focusMode = (FocusMode)Session[this.GUID + "FOCUSMODE"];
            FocusMode = DataCache.GetData<FocusModeEnum>(this.GUID + "FOCUSMODE");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected override object SaveViewState()
        {
            // FI 20200518 [XXXXX] Utilisation de DataCache
            //Session[this.GUID + "FOCUSMODE"] = focusMode;
            DataCache.SetData<FocusModeEnum>(BuildDataCacheKey("FOCUSMODE"), FocusMode);
            return base.SaveViewState();
        }


        /// <summary>
        /// Recherche le contrôle associé au Cci, et affecte l'attribut OnClick de manière à ouvrir le titre associé au cci
        /// </summary>
        /// <param name="pCci"></param>
        /// FI 20120625 Add SetOpenFormDebtSecurity
        /// FI 20140912 [XXXXX] Modify
        public void SetOpenFormDebtSecurity(CustomCaptureInfo pCci)
        {
            CustomCaptureInfo cci = pCci;
            //    
            SQL_TableWithID sqlTable = null;
            if (null != cci.Sql_Table)
                sqlTable = cci.Sql_Table as SQL_TableWithID;
            //
            if (null != sqlTable)
            {
                WebControl control = PlaceHolder.FindControl(pCci.ClientId_Prefix + pCci.ClientId_WithoutPrefix) as WebControl;
                if (pCci.ClientId_Prefix == Cst.DDL)
                {
                    if (control is WCDropDownList2)
                    {
                        WCDropDownList2 ddl = control as WCDropDownList2;
                        if (ddl.HasViewer)
                            control = ddl.LblViewer;
                        else
                            control = null;
                    }
                }
                else if (pCci.ClientId_Prefix == Cst.TXT)
                {
                    if (control is WCTextBox2)
                    {
                        WCTextBox2 txt = control as WCTextBox2;
                        if (false ==
                            (txt.CssClass.Contains(EFSCssClass.Consult)))
                            control = null;
                    }
                }
                //
                if ((null != control))
                {
                    if (null == control.Attributes["onclick"]) // FI 20140708 [20179] Mise en place d'un lien sur le click uniquement s'il l'atribut est non resnseigné
                    {
                        // EG 20170125 [Refactoring URL] Upd
                        //string url = SpheresURL.GetUrlFormTradeDebtSecurity("IDT", sqlTable.Id.ToString());
                        string url = SpheresURL.GetURL(IdMenu.Menu.InputDebtSec, sqlTable.Id.ToString());
                        // FI 20140912 [XXXXX] Call methode ControlsTools.SetHyperLinkOpenFormReferential
                        ControlsTools.SetHyperLinkOpenFormReferential(control, url);
                    }
                }
            }
        }

        /// <summary>
        /// Récupère le contrôle dont l'id vaut {pClientId}
        /// </summary>
        /// <param name="pClientId"></param>
        /// <returns></returns>
        /// FI 20121126 [18224] Récupère le contrôle dont l'id vaut {pClientId}
        public Control GetCciControl(string pClientId)
        {
            return InputGUI.GetCciControl(pClientId);
        }

        /// <summary>
        /// Récupère le customObject à l'origine du contrôle dont l'id vaut {pClientId}
        /// </summary>
        /// <param name="pClientId"></param>
        /// <returns></returns>
        public CustomObject GetCciCustomObject(string pClientId)
        {
            return InputGUI.GetCciCustomObject(pClientId);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="coCtrl"></param>
        /// <param name="typeControl"></param>
        /// <param name="control"></param>
        /// <param name="root"></param>
        /// <param name="bModeQuick"></param>
        /// <param name="pQuickClientId"></param>
        /// FI 20121126 [18224] add method
        /// EG 20170918 [23342] New CustomObject.ControlEnum.timestamp 
        private void AddCci(CustomObject coCtrl, CustomObject.ControlEnum typeControl, XmlNode root, bool bModeQuick, string pQuickClientId)
        {
            switch (typeControl)
            {
                case CustomObject.ControlEnum.textbox:
                    Object.CcisBase.Add(new CustomCaptureInfo(coCtrl.CtrlClientId, coCtrl.AccessKey, coCtrl.IsMandatory, coCtrl.DataTypeEnum, coCtrl.IsEnabled, coCtrl.RegexValue));
                    break;
                case CustomObject.ControlEnum.timestamp:
                    Object.CcisBase.Add(new CustomCaptureInfo(coCtrl.CtrlClientId, coCtrl.AccessKey, coCtrl.IsMandatory, coCtrl.DataTypeEnum, coCtrl.IsEnabled, coCtrl.RegexValue));
                    // Ajout Timezone
                    Object.CcisBase.Add(new CustomCaptureInfo(coCtrl.CtrlClientId.Replace(Cst.TMS, Cst.TMZ), coCtrl.IsMandatory, TypeData.TypeDataEnum.@string, coCtrl.IsEnabled, null));
                    break;
                case CustomObject.ControlEnum.checkbox:
                case CustomObject.ControlEnum.htmlcheckbox:
                    Object.CcisBase.Add(new CustomCaptureInfo(coCtrl.CtrlClientId, coCtrl.IsMandatory, TypeData.TypeDataEnum.@bool, coCtrl.IsEnabled));
                    break;
                case CustomObject.ControlEnum.htmlselect:
                case CustomObject.ControlEnum.dropdown:
                case CustomObject.ControlEnum.optgroupdropdown: // 20090909 RD / DropDownList avec OptionGroup
                case CustomObject.ControlEnum.ddlbanner:
                    CustomObjectDropDown coDropDown = (CustomObjectDropDown)coCtrl;
                    if (coDropDown.ContainsRelativeTo)
                        Object.CcisBase.Add(new CustomCaptureInfo(coDropDown.CtrlClientId, coDropDown.IsMandatory, coDropDown.DataTypeEnum, coCtrl.IsEnabled, string.Empty));
                    else
                        Object.CcisBase.Add(new CustomCaptureInfo(coDropDown.CtrlClientId, coDropDown.IsMandatory, coDropDown.DataTypeEnum, coCtrl.IsEnabled, coDropDown.ListRetrieval));
                    break;
                case CustomObject.ControlEnum.hyperlink:
                    Object.CcisBase.Add(new CustomCaptureInfo(coCtrl.CtrlClientId, coCtrl.IsMandatory, TypeData.TypeDataEnum.unknown, coCtrl.IsEnabled));
                    break;
                case CustomObject.ControlEnum.quickinput:
                    #region QuikInput => alimentation des ccis (parcours du node sur lequel pointe la quick sans interpretation du design)
                    if ((null != root.Attributes.GetNamedItem("object")))
                    {
                        string obj2 = XMLTools.GetNodeAttribute(root, "object");
                        //
                        CustomCaptureInfo cciQuickInput;
                        cciQuickInput = new CustomCaptureInfo(coCtrl.CtrlClientId, coCtrl.IsMandatory, TypeData.TypeDataEnum.@string, coCtrl.IsEnabled, coCtrl.RegexValue);
                        if (null != root.Attributes.GetNamedItem("separator"))
                            cciQuickInput.QuickSeparator = XMLTools.GetNodeAttribute(root, "separator", false);
                        if (null != root.Attributes.GetNamedItem("format"))
                            cciQuickInput.QuickFormat = XMLTools.GetNodeAttribute(root, "format");
                        //
                        Object.CcisBase.Add(cciQuickInput);
                        //
                        XmlNode objNode = GetNodeObject(obj2);
                        if (null != objNode)
                            SearchObject(objNode, coCtrl.ClientId);
                    }
                    #endregion QuikInput
                    break;
                default:
                    throw new NotImplementedException(typeControl.ToString() + "not implemented");
            }
            if (coCtrl.IsInQuickInput && bModeQuick)
                Object.CcisBase.SetQuickClientId(coCtrl.ClientId, pQuickClientId);
        }


        /// <summary>
        /// Ajoute éventuellement un item dans la liste des contrôles associés aux customCaptureInfos
        /// </summary>
        /// <param name="coCtrl"></param>
        /// <param name="typeControl"></param>
        /// <param name="control"></param>
        /// FI 20121126 [18224] add method
        /// EG 20170822 [23342] New CustomObject.ControlEnum.timestamp
        private void AddCciControl(CustomObject coCtrl, CustomObject.ControlEnum typeControl, Control control)
        {
            if (coCtrl.IsControlData)
            {
                InputGUI.AddCciControl(coCtrl, control);
            }
            else
            {
                switch (typeControl)
                {
                    // Les labels de type DSP sont ajoutés dans la collection des contrôles associés aux CCI
                    // 20130322 ajout des BUT
                    case CustomObject.ControlEnum.display:
                    case CustomObject.ControlEnum.button:
                        InputGUI.AddCciControl(coCtrl, control);
                        break;

                }
            }
        }

        /// <summary>
        ///  Attribut un état au contrôles {pClientId}
        /// </summary>
        /// <param name="pClientId"></param>
        /// <param name="pState"></param>
        /// <param name="pIsChange">retourne true si le nouvel état est différent de l'état précédent</param>
        /// FI 20121127 [18224] new method
        public void ControlSetState(string pClientId, string pState, out bool pIsChange)
        {
            InputGUI.ControlSetState(pClientId, pState, out pIsChange);
        }

        /// <summary>
        ///  Synchronisation du nouvel état et du précédent état du contrôle 
        /// </summary>
        /// FI 20121127 [18224] new method
        public void ControlSynchroState(string pClientId)
        {
            InputGUI.ControlSynchroState(pClientId);
        }


        /// <summary>
        ///  Recherche le control image associé au marché pour lui appliquer un drapeau qui correspond au pays du marché
        /// </summary>
        /// <param name="pCCi"></param>
        /// FI 20161214 [21916] Add
        /// FI 20180129 [XXXXX] Modify
        public void SetMarketCountryImage(CustomCaptureInfo pCci)
        {
            if (PlaceHolder.FindControl(Cst.BUT + pCci.ClientId_WithoutPrefix + "_img") is WCToolTipButton img)
            {
                img.Visible = (null != pCci.Sql_Table);
                if (null != pCci.Sql_Table)
                {
                    if (!(pCci.Sql_Table is SQL_Market sqlMarket))
                        throw new NotSupportedException(StrFunc.AppendFormat("cci (id:{0}) is not a market", pCci.ClientId_WithoutPrefix));

                    ControlsTools.RemoveStyleDisplay(img);
                    // FI 20180129 [XXXXX] Spheres doesn't display the Flag for specific country
                    if (StrFunc.IsFilled(sqlMarket.IdCountry))
                    {
                        Regex regex = new Regex(@"^([A-Z])([A-Z])$"); // 2 identical letters (i.e. ZZ ) => Spheres doesn't display the Flag 
                        Match match = regex.Match(sqlMarket.IdCountry.ToUpper());
                        if (match.Success && (match.Groups[1].Value == match.Groups[2].Value))
                        {
                            img.Visible = false;
                        }
                        else
                        {
                            img.Text = string.Empty;
                            img.CausesValidation = false;
                            img.Enabled = false;
                            img.CssClass = CSS.SetCssClassFlags(sqlMarket.IdCountry);
                        }
                    }
                    else
                    {
                        img.Visible = false;
                    }
                }
            }
            //FI 20191128 [XXXXX] inutile puisqu'il n'existe aucun lien
            //Label display = FindControl(Cst.DSP + pCci.ClientId_WithoutPrefix) as Label;
            //if (null != display)
            //{
            //    display.Attributes.Add("onmouseover", "this.style.textDecoration='underline'");
            //    display.Attributes.Add("onmouseout", "this.style.textDecoration='none'");
            //}
        }

        /// <summary>
        /// Affiche le contrôle associé à un WCLinkButtonOpenBanner
        /// </summary>
        /// <param name="pClientId">Control Id d'un WCLinkButtonOpenBanner</param>
        /// FI 20161214 [21916] Add
        /// FI 20200121 [25167] Rename
        // EG 20200914 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc) Correction et compléments
        public void ShowLinkControl(string pClientId)
        {
            if (PlaceHolder.FindControl(pClientId) is WCLinkButtonOpenBanner linkBanner)
                linkBanner.ShowLinkControl();
        }



        #endregion
    }

    /// <summary>
    /// 
    /// </summary>
    /// FI 20170928 [23452] Modify
    public class TableSettings
    {
        public string ColSpan
        { get; set; }
        public string RowSpan
        { get; set; }
        public string VerticalAlign
        { get; set; }
        public string Style
        { get; set; }
        public string PanelOverFlow
        { get; set; }
        public string Id
        { get; set; }
        public string ControlAlignLeft
        { get; set; }
        public string GridLines
        { get; set; }
        public string BorderColor
        { get; set; }
        public string FixedCol
        { get; set; }
        /// <summary>
        /// permet l'application d'un style sur la cellule qui contient la table
        /// </summary>
        /// FI 20170928 [23452] add
        public string CellStyle
        { get; set; }
    }

}
