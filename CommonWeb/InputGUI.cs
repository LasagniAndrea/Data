
#region Using Directives
//
using EFS.ACommon;
using EFS.Actor;
using System;
using System.Collections;
using System.Web.UI;
using System.Xml;

#endregion Using Directives

// EG 20231129 [WI756] Spheres Core : Refactoring Code Analysis with Intellisense

namespace EFS.Common.Web
{

    /// <summary>
    /// Représente l'environnement rattaché à un utilisateur lorsqu'il ouvre un menu
    /// <para>Par environnement, Spheres® considère le menu, le type de saisie(création, modification), actionTuning , les objets de design </para>
    /// </summary>
    // EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
    public class InputGUI : InputUser
    {
        #region Members
        /// <summary>
        /// Représente le nom de l'écran de saisie
        /// </summary>
        private string _currentIdScreen;
        /// <summary>
        /// Représente l'image associée à m_captureMode
        /// </summary>
        /// EG 20150923 Unused
        //private string _captureModeImage;
        /// <summary>
        /// Représente les contrôles de la page
        /// </summary>
        private ControlCollection _controls;
        /// <summary>
        /// Représente le document XML qui contient les objets de design 
        /// <para>
        /// Membre obtenu à partir des fichiers XML de description des écrans et des objets
        /// </para>
        /// </summary>
        private XmlDocument _docXML;
        /// <summary>
        /// Représente le répertoire où se trouve les fichiers XML de description des écrans et des objets
        /// </summary>
        private string _xmlFilePath;
        /// <summary>
        /// Représente le Cascade style sheet appliqué sur la page de saisie
        /// </summary>
        private string _cssMode;
        private string _cssColor;
        /// <summary>
        /// Représente la liste des customObjects associés aux boutons d'ouverture d'un référentiel existants sur la page de saisie 
        /// </summary>
        private Hashtable _customObjectButtonReferential;
        /// <summary>
        /// Représente la liste des control Web de type WCLinkButtonOpenBanner existants sur la page de saisie 
        /// </summary>
        private Hashtable _linkButtonOpenBanner;
        /// <summary>
        /// Représente la liste des control Web associé aux cci existants sur la page de saisie (chaque contrôle est une zone de saisi)
        /// <para>Les contrôles présents sont des dropDowns, des textboxs, des checkboxs</para>
        /// </summary>
        /// FI 20121126 [18224] add _cciControls
        private Hashtable _cciControls;
        /// <summary>
        /// Représente une liste avec l'état des DDL
        /// </summary>
        /// FI 20121126 [18224] add _ddlState
        private Hashtable _controlState;
        private string _mainMenuClassName;
        #endregion Members

        #region Accessors
        /// <summary>
        /// Obtient ou définit l'image qui représente 
        /// </summary>
        // EG 20150923 Unused
        //public string CaptureModeImage
        //{
        //    get { return _captureModeImage; }
        //    set { _captureModeImage = value; }
        //}

        /// <summary>
        /// Obtient le type d'écran (Full ou Customised) 
        /// <para>voir Cst.Capture.TypeEnum</para>
        /// </summary>
        public Cst.Capture.TypeEnum CaptureType
        {
            get
            {
                if (Cst.FpML_ScreenFullCapture == _currentIdScreen)
                    return Cst.Capture.TypeEnum.Full;
                else
                    return Cst.Capture.TypeEnum.Customised;
            }
        }

        /// <summary>
        /// Obtient ou définit les contrôles de la page de saisie
        /// </summary>
        public ControlCollection Controls
        {
            get { return _controls; }
            set { _controls = value; }
        }

        /// <summary>
        /// Obtient ou définit le nom de l'écran 
        /// </summary>
        public string CurrentIdScreen
        {
            get { return _currentIdScreen; }
            set { _currentIdScreen = value; }
        }

        /// <summary>
        /// Obtient ou dénit la couleur de style appliquée sur la feuille de saisie
        /// </summary>
        // EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        public string CssColor
        {
            get { return _cssColor; }
            set { _cssColor = value; }
        }
        /// <summary>
        /// Obtient le GUI mode appliquée sur la saisie (Noir ou blanc)
        /// 
        /// </summary>
        // EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        public string CssMode
        {
            get { return _cssMode; }
            set { _cssMode = value; }
        }

        /// <summary>
        /// Obtient ou définit le document XML qui contient les objets de design 
        /// </summary>
        public XmlDocument DocXML
        {
            get { return _docXML; }
            set { _docXML = value; }
        }

        /// <summary>
        /// Obtient ou dénit le répertoire les fichiers XML de descriptions des écrans et objets
        /// </summary>
        public string XMLFilePath
        {
            get { return _xmlFilePath; }
            set { _xmlFilePath = value; }
        }
        /// <summary>
        /// Référence le type de menu parent associé (Input)  
        /// </summary>
        // EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        public string MainMenuClassName
        {
            get { return _mainMenuClassName; }
            set { _mainMenuClassName = value; }
        }

        #endregion Accessors

        #region Constructors
        // EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        public InputGUI(User pUser, string pIdMenu, string pXMLFilePath)
            : base(pIdMenu, pUser)
        {
            XMLFilePath = pXMLFilePath;
            _mainMenuClassName = ControlsTools.MainMenuName(IdMenu);
        }
        #endregion Constructors

        #region Methods
        /// <summary>
        /// <para>Initialise les permissions rattachées au menu</para>
        /// <para>Initialise le type de saisie (notamment en fonction des permissions)</para>
        /// <para>Initialise l'image associée au type de saisie (New ou Consult)</para>
        /// </summary>
        /// <param name="pCs"></param>
        public override void InitializeFromMenu(string pCs)
        {
            base.InitializeFromMenu(pCs);
            InitCaptureMode();
        }

        /// <summary>
        /// Initialise de type de saisie (Création ou consultation) et l'image associée
        /// </summary>
        public override void InitCaptureMode()
        {
            base.InitCaptureMode();
        }

        /// <summary>
        /// Ajoute un item dans la liste des customObjects associés aux boutons d'ouverture d'un référentiel existants sur la page de saisie 
        /// </summary>
        /// <param name="pCustomObjectButtonReferential"></param>
        public void AddCustomObjectButtonReferential(CustomObjectButtonReferential pCustomObjectButtonReferential)
        {
            if (null == _customObjectButtonReferential)
                _customObjectButtonReferential = new Hashtable();

            if (false == _customObjectButtonReferential.ContainsKey(pCustomObjectButtonReferential.ClientId))
                _customObjectButtonReferential.Add(pCustomObjectButtonReferential.ClientId, pCustomObjectButtonReferential);
        }

        /// <summary>
        /// Retourne la liste des objects de gestion des boutons d'ouverture de référentiel/consultations 
        /// </summary>
        public CustomObjectButtonReferential[] GetCustomObjectButtonReferential()
        {
            CustomObjectButtonReferential[] ret = null;
            if (null != _customObjectButtonReferential)
            {
                ret = new CustomObjectButtonReferential[_customObjectButtonReferential.Count];
                _customObjectButtonReferential.Values.CopyTo((CustomObjectButtonReferential[])ret, 0);
            }
            return ret;
        }

        /// <summary>
        /// Ajoute un item dans la liste des control Web de type WCLinkButtonOpenBanner 
        /// </summary>
        /// <param name="pLinkButtonOpenBanner"></param>
        // EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        public void AddLinkButtonOpenBanner(WCLinkButtonOpenBanner pLinkButtonOpenBanner)
        {
            if (null == _linkButtonOpenBanner)
                _linkButtonOpenBanner = new Hashtable();
            _linkButtonOpenBanner.Add(pLinkButtonOpenBanner.ID, pLinkButtonOpenBanner);
        }

        /// <summary>
        /// Retourne la liste des controles web de type WCLinkButtonOpenBanner existants sur la page de saisie
        /// </summary>
        // EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        public WCLinkButtonOpenBanner[] GetLinkButtonOpenBanner()
        {
            WCLinkButtonOpenBanner[] ret = null;
            if (null != _linkButtonOpenBanner)
            {
                ret = new WCLinkButtonOpenBanner[_linkButtonOpenBanner.Count];
                _linkButtonOpenBanner.Values.CopyTo((WCLinkButtonOpenBanner[])ret, 0);
            }
            return ret;
        }

        /// <summary>
        /// Ajoute un item dans la liste des contrôles associés aux customCaptureInfos
        /// </summary>
        /// <param name="pCustomObject"></param>
        /// <param name="pControl"></param>
        /// FI 20121126 [18224] add method
        public void AddCciControl(CustomObject pCustomObject, Control pControl)
        {
            if (null == _cciControls)
                _cciControls = new Hashtable();

            if (false == _cciControls.ContainsKey(pCustomObject.CtrlClientId))
                _cciControls.Add(pCustomObject.CtrlClientId, new Pair<Control, CustomObject>(pControl, pCustomObject));

            if (pCustomObject is CustomObjectTimestamp)
            {
                string _clientId = pCustomObject.CtrlClientId.Replace(Cst.TMS, Cst.TMZ);
                if (false == _cciControls.ContainsKey(_clientId))
                    _cciControls.Add(_clientId, new Pair<Control, CustomObject>(pControl, pCustomObject));
            }

        }

        /// <summary>
        /// Retourne la liste des contrôles associés aux customCaptureInfos
        /// </summary>
        /// FI 20121126 [18224] add method
        public Control[] GetCciControl()
        {
            Control[] ret = null;
            if (null != _cciControls)
            {
                ret = new Control[_cciControls.Count];

                int i = 0;
                foreach (Pair<Control, CustomObject> item in _cciControls)
                {
                    ret[i] = item.First;
                    i++;
                }
            }
            return ret;
        }
        /// <summary>
        /// Retourne le contrôle {pClientId} parmi liste des contrôles associés aux customCaptureInfos
        /// <para>Retourne null si le contrôle n'existe pas</para>
        /// </summary>
        /// <param name="pClientId"></param>
        /// <returns></returns>
        public Control GetCciControl(string pClientId)
        {
            Control ret = null;
            if (_cciControls.Contains(pClientId))
            {
                Pair<Control, CustomObject> pair = (Pair<Control, CustomObject>)_cciControls[pClientId] as Pair<Control, CustomObject>;
                ret = pair.First;
            }
            return ret;
        }
        /// <summary>
        /// Retourne le CustomObject associé au contrôle {pClientId} parmi liste des contrôles associés aux customCaptureInfos
        /// </summary>
        /// <param name="pClientId"></param>
        /// <returns></returns>
        public CustomObject GetCciCustomObject(string pClientId)
        {
            CustomObject ret = null;
            if (_cciControls.Contains(pClientId))
            {
                Pair<Control, CustomObject> pair = (Pair<Control, CustomObject>)_cciControls[pClientId] as Pair<Control, CustomObject>;
                ret = pair.Second;
            }
            return ret;
        }

        /// <summary>
        ///  Attribut un nouvel état au contrôles {pClientId}, l'état précédent est conservé
        /// </summary>
        /// <param name="pClientId"></param>
        /// <param name="pState"></param>
        /// <param name="pIsChange">retourne true si le nouvel état est différent de l'état précédent</param>
        /// FI 20121127 [18224] new method
        public void ControlSetState(string pClientId, string pState, out bool pIsChange)
        {
            if (null == _controlState)
                _controlState = new Hashtable();

            if (false == _controlState.ContainsKey(pClientId))
            {
                _controlState.Add(pClientId, new Pair<string, string>(pState, string.Empty));
            }
            else
            {
                Pair<string, string> state = _controlState[pClientId] as Pair<string, string>;
                state.First = pState;
            }

            pIsChange = ControlIsStateChange(pClientId);
        }
        
        /// <summary>
        /// Retourne true si l'état du contrôle est différent de l'état précédent
        /// </summary>
        /// <param name="pClientId"></param>
        /// <returns></returns>
        /// FI 20121127 [18224] new method
        public Boolean ControlIsStateChange(string pClientId)
        {
            if (false ==  _controlState.ContainsKey(pClientId))
                throw new Exception (StrFunc.AppendFormat("clientId {0} doesn't exist in ddlState"));

            Pair<string, string> state = _controlState[pClientId] as Pair<string, string>;
            bool ret = state.First != state.Second;
            return ret;
        }

        /// <summary>
        ///  Synchronisation du nouvel état et du précédent état du contrôle 
        /// </summary>
        /// FI 20121127 [18224] new method
        public void ControlSynchroState(string pClientId)
        {
            if (false == _controlState.ContainsKey(pClientId))
                throw new Exception(StrFunc.AppendFormat("clientId {0} doesn't exist in ddlState"));

            Pair<string, string> state = _controlState[pClientId] as Pair<string, string>;
            state.Second = state.First;
        }

        /// <summary>
        /// Purge de la collection qui contient tous les controles
        /// </summary>
        // EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        public void ClearGUI()
        {
            _customObjectButtonReferential = null;
            _controls = null;
            _linkButtonOpenBanner = null;
            //FI 20121126 [18224] add _cciControls = null
            _cciControls = null;
            //FI 20121126 [18224] add _ddlState = null
            _controlState = null;
        }

        #endregion
    }

}
