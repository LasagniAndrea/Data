#region Using Directives
using System;
using System.Collections;
using System.Globalization;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Reflection;
using System.Xml.Serialization;
using System.Drawing;

using EFS.ACommon;
using EFS.Common;
using EFS.Common.Web;
using EFS.EFSTools;
using EFS.GUI.Attributes;
using EFS.GUI.Interface;

using EfsML.Enum.Tools;
using EfsML.Enum;



using FpML.Enum;
#endregion Using Directives

namespace EFS.GUI.CCI
{
    /// <summary>
    /// 
    /// </summary>
    public interface ICustomCaptureInfos
    {
        CustomCaptureInfosBase CcisBase { get; }
    }
    
    /// <summary>
    /// Interface to implemented for class Trade[product] (ie: TradeFra)
    /// Warning: This methods are "virtual method" of the class "TradeBase"
    /// </summary>
    public interface IContainerCciFactory
    {

        /// <summary>
        /// Instanciations des objets du dataDocument en fonction de la présence des CCIs
        /// <para>Exemple s'il existe des ccis appartenent à un 3ème otherPartyPayments, on génère ici un 3ème otherPartyPayments dans le dataDocument</para>
        /// </summary>
        void Initialize_FromCci();


        /// <summary>
        /// Ajout des ccis dits "Systems" car systématiquement nécessaires 
        /// (ie: Buyer et Seller du fra)
        /// <remarks>Par défaut, seuls sont injectés les ccis qui sont déclarés sur les fichiers descriptifs de l'écran</remarks>
        /// </summary>
        void AddCciSystem();


        /// <summary>
        /// Initialisation des ccis à partir des données présentes dans le dataDocument
        /// </summary>
        void Initialize_FromDocument();

        /// <summary>
        /// Alimentation du dataDocument avec les données présentes dans les ccis
        /// </summary>
        void Dump_ToDocument();


        /// <summary>
        /// Préproposition de ccis en fonction du cci en entrée
        /// </summary>
        /// <param name="pProcessQueue"></param>
        /// <param name="pCci"></param>
        void ProcessInitialize(CustomCaptureInfo pCci);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCci"></param>
        void ProcessExecute(CustomCaptureInfo pCci);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCci"></param>
        void ProcessExecuteAfterSynchronize(CustomCaptureInfo pCci);


        /// <summary>
        /// retourne true si le cci ne peut être alimenté qu'avec les contreparties
        /// </summary>
        bool IsClientId_PayerOrReceiver(CustomCaptureInfo pCci);


        /// <summary>
        /// CleanUp => Procedure appelée afin de nettoyer le document Fpml avant validation ou avant chgt d'écran 
        /// Ex OtherPartyPayment 
        /// s'il existe 3 OtherPartyPayment sur le screen objects => IL y aura 3 instances de Trade.otherpartyPayment
        /// Les instances non alimentées seront supprimées pour conformité  vis à vis de l'xsd fpml
        /// </summary>
        void CleanUp();


        /// <summary>
        /// Affecte la propriétié Enabled des ccis
        /// </summary>
        void RefreshCciEnabled();


        /// <summary>
        /// Affecte la propriétié Display d'un cci
        /// </summary>
        void SetDisplay(CustomCaptureInfo pCci);


        /// <summary>
        ///  Initialisation du document pour que la saisie light soit operationnelle
        ///  Exemple En Création, il faut absolument que les payers receivers qui font référence aux COUNTERPARTY ne soit pas identiques
        ///  sinon Pb de synchronisation des payers/receivers
        /// </summary>
        void Initialize_Document();

    }
    
    /// <summary>
    /// 
    /// </summary>
    public interface IContainerCci
    {
        
        /// <summary>
        /// retourne le nom complet du cci (sans le prefix du control)  s'il appartient à la classe trade en cours ( Ex TradeFra ou TradeFxLeg......)
        /// Ex Fra => TradeFra.CciClientId( "toto") => Retourne le cci tel que ClientId_WithoutPrefix = fra_toto  
        /// </summary>
        string CciClientId(string pClientId_Key);
       
        /// <summary>
        /// retourne le cci s'il appartient à la classe trade en cours
        /// Ex Fra => TradeFra.CciTradeIdentifier("Toto") => Retourne le ccis[fra_toto] 
        /// </summary>
        CustomCaptureInfo Cci(string pClientId_Key);
      
        /// <summary>
        /// retourne si le cci accessible  depuis l'instance trade en cours
        /// parametre = nom complet (sans le prefix control)
        /// </summary>
        bool IsCciOfContainer(string pClientId_WithoutPrefix);
       
        /// <summary>
        /// retourne l'élement à partir du nom complet (sans le prefix control)
        /// Ex Fra => TradeFra.CciContainerKey("fra_Toto") => Retourne "Toto"
        /// </summary>
        string CciContainerKey(string pClientId_WithoutPrefix);
    }

    /// <summary>
    /// 
    /// </summary>
    public interface IContainerArray
    {
        #region RemoveLastItemInArray
        void RemoveLastItemInArray(string pPrefix);
        #endregion RemoveLastItemInArray
    }

    /// <summary>
    /// Interface dedicated to CCiControl with special needs at the presentation level
    /// </summary>
    public interface ICciPresentation
    {
        void DumpSpecific_ToGUI(CciPageBase pPage);
    }

    /// <summary>
    /// CustomCaptureInfos: Contient une collection de classe CustomCaptureInfo.
    /// </summary>
    public abstract class CustomCaptureInfosBase : CollectionBase
    {
        #region Enums
        #region ProcessQueueEnum
        public enum ProcessQueueEnum
        {
            High,
            Low,
            None,
        }
        #endregion ProcessQueueEnum
        #endregion Enums

        #region Members
        protected bool m_IsGetDefaultOnInitializeCci;
        /// <summary>
        /// Obj With Cci
        /// </summary>
        private ICustomCaptureInfos m_Obj;
        /// <summary>
        /// cciContainer 
        /// </summary>
        private IContainerCciFactory m_CciContainer;
        //
        //stokage Session en cours (for restriction au données)
        private string m_SessionId;      // doit être valorisé dans ds un contexte Web (et pas ds des services par Ex)
        private bool m_IsSessionAdmin;
        private bool m_IsModeIO;
        //
        private Cst.Capture.ModeEnum m_CaptureMode;
        //use by LoadCapture (true during LoadCapture)
        /// <summary>
        /// 
        /// </summary>
        private bool m_IsLoading;
        //Use for ProcessInitialize
        private Queue m_MyHigh;
        private Queue m_MyLow;
        #endregion Members

        #region Accessors
        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public IContainerCciFactory CciContainer
        {
            set { m_CciContainer = value; }
            get { return m_CciContainer; }
        }

        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public Cst.Capture.ModeEnum CaptureMode
        {
            set { m_CaptureMode = value; }
            get { return m_CaptureMode; }
        }


        /// <summary>
        /// Obtient ou définit un indicateur pour activer l'initialisation des données clefs
        /// <para>1er Exemple Si true alors EntityOfUser sera remplacé par l'entité rattaché à l'utilisateur</para>
        /// <para>2nd Exemple Si true alors un CCI destininé a rececoir une devise sera initialisé avec la devise par défaut</para>
        /// </summary>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public bool IsGetDefaultOnInitializeCci
        {
            get { return m_IsGetDefaultOnInitializeCci; }
            set { m_IsGetDefaultOnInitializeCci = value; }
        }

        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool CciContainerSpecified
        {
            get { return (null != m_CciContainer); }
        }

        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public string SessionId
        {
            set { m_SessionId = value; }
            get { return m_SessionId; }
        }

        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool IsSessionAdmin
        {
            set { m_IsSessionAdmin = value; }
            get { return m_IsSessionAdmin; }
        }


        /// <summary>
        /// Mode input with Spheres IO
        /// </summary>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool IsModeIO
        {
            set { m_IsModeIO = value; }
            get { return m_IsModeIO; }
        }

        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public ICustomCaptureInfos Obj
        {
            set { m_Obj = value; }
            get { return m_Obj; }
        }


        /// <summary>
        ///  Obtient ou définit un drapeaux afin d'indiquer que les ccis ne sont plus en phase avec le document
        /// </summary>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public Boolean IsToSynchronizeWithDocument
        {
            get;
            set;
        }


        /// <summary>
        /// Obtient lorsqu'il n'y a pas interprétation des données saisies
        /// Exemple : Today ne doit pas être interprété 
        /// <para>Ce mode de fonctionnement s'applique sur les templates </para>
        /// </summary>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public virtual bool IsPreserveData
        {
            get { return false; }
        }

        /// <summary>
        /// Ontient true si, au minimum, 1 cci est HasChanged == true
        /// </summary>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool HasChanged
        {
            get
            {
                foreach (CustomCaptureInfo cci in this)
                {
                    if (cci.HasChanged)
                        return true;
                }
                return false;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public virtual bool IsUseProcessInitialize
        {
            get { return true; }
        }

        /// <summary>
        /// Obtient true durant la période d'exécution de la méthode LoadCapture
        /// </summary>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool IsLoading
        {
            get { return m_IsLoading; }
        }

        /// <summary>
        /// 
        /// </summary>
        public bool isQueueEmpty
        {
            get
            {
                return ArrFunc.IsEmpty(m_MyHigh) && ArrFunc.IsEmpty(m_MyLow);
            }
        }
        #endregion Accessors

        #region Indexors
        /// <summary>
        /// </summary>
        /// <param name="pIndex"></param>
        /// <returns></returns>
        public CustomCaptureInfo this[int pIndex]
        {
            get { return (CustomCaptureInfo)this.List[pIndex]; }
        }
        /// <summary>
        /// Obtient le cci de la collection tel cci.ClientId_WithoutPrefix == {pClientId_WithoutPrefix}
        /// <para>
        /// Obtient null si inexistant
        /// </para>
        /// </summary>
        /// <param name="pClientId_WithoutPrefix"></param>
        /// <returns></returns>
        public CustomCaptureInfo this[string pClientId_WithoutPrefix]
        {
            get
            {
                CustomCaptureInfo ret = null;
                if (0 < this.Count)
                {
                    foreach (CustomCaptureInfo cci in this)
                    {
                        if (cci.ClientId_WithoutPrefix == pClientId_WithoutPrefix)
                        {
                            ret = cci;
                            break;
                        }
                    }
                }
                return ret;
            }
        }
        #endregion Indexors

        #region Constructor
        /// <summary>
        /// 
        /// </summary>
        public CustomCaptureInfosBase()
        {
            m_MyHigh = new Queue();
            m_MyLow = new Queue();
            //m_IsGetDefaultOnInitializeCci = false;
            //m_Obj = null;
        }
        ///// <summary>
        ///// 
        ///// </summary>
        ///// <param name="pObj"></param>
        //public CustomCaptureInfosBase(ICustomCaptureInfos pObj)
        //    : this(pObj, null, true, true)
        //{ }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pObj"></param>
        /// <param name="pSessionId"></param>
        /// <param name="pIsSessionAdmin"></param>
        /// <param name="pIsGetDefaultOnInitializeCci"></param>
        public CustomCaptureInfosBase(ICustomCaptureInfos pObj, string pSessionId, bool pIsSessionAdmin, bool pIsGetDefaultOnInitializeCci)
        {
            m_SessionId = pSessionId;
            m_IsSessionAdmin = pIsSessionAdmin;
            m_IsGetDefaultOnInitializeCci = pIsGetDefaultOnInitializeCci;
            //
            m_MyHigh = new Queue();
            m_MyLow = new Queue();
            m_Obj = pObj;
            //
            InitializeCciContainer();
            //
        }
        #endregion Constructors

        #region Methods

        /// <summary>
        /// Ajoute un cci à la collection 
        /// <para>L'ajout n'est pas effectué s'il n'existe déjà un cci avec même clientId</para>
        /// </summary>
        /// <param name="pCci"></param>
        public void Add(CustomCaptureInfo pCci)
        {

            bool isAlreadyRexist = false;
            foreach (CustomCaptureInfo cci in this)
            {
                if (cci.clientId == (pCci.clientId))
                {
                    isAlreadyRexist = true;
                    break;
                }
            }
            //
            if (false == isAlreadyRexist)
                List.Add(pCci);

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pQuickClientId"></param>
        /// <param name="pQuickPosition"></param>
        /// <returns></returns>
        private CustomCaptureInfo CciOfQuickInput(string pQuickClientId, int pQuickPosition)
        {
            // Retounre Cci Tel que Cci.QuickClientId = pQuickClientId et  Cci.QuickDataPosition= pQuickPosition
            CustomCaptureInfo cci = null;
            for (int i = 0; i < this.Count; i++)
            {
                if ((this[i].quickClientId == pQuickClientId) && (this[i].quickDataPosition == pQuickPosition))
                {
                    cci = this[i];
                    break;
                }
            }
            return cci;
        }

        /// <summary>
        /// Nettoie la propriété errorMsg du cci 
        /// </summary>
        /// <param name="pClientId_WithoutPrefix"></param>
        public void ClearErrorMsg(string pClientId_WithoutPrefix)
        {
            SetErrorMsg(pClientId_WithoutPrefix, string.Empty);
        }


        /// <summary>
        /// Nettoie tous les ccis de la collection [cci.newValue = null;cci.sql_Table = null; cci.lastValue = null;cci.lastSql_Table = null]
        /// </summary>
        ///FI 20091130 [16770] => ClearAllValue
        public void ClearAllValue()
        {
            foreach (CustomCaptureInfo cci in this)
            {
                cci.newValue = null;
                cci.sql_Table = null;
                cci.lastValue = null;
                cci.lastSql_Table = null;
            }

        }

        /// <summary>
        /// Remove empty item, set specified flag and set default party before Validation
        /// </summary>
        public void CleanUp()
        {
            if (CciContainerSpecified)
                CciContainer.CleanUp();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pClientId_WithoutPrefix"></param>
        /// <returns></returns>
        public bool Contains(string pClientId_WithoutPrefix)
        {
            return (null != this[pClientId_WithoutPrefix]);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pQuickClientId"></param>
        /// <param name="pQuickPosition"></param>
        /// <returns></returns>
        private bool ConstainsCciOfQuickInput(string pQuickClientId, int pQuickPosition)
        {
            // Test Existence d'un Cci Tel que Cci.QuickClientId = pQuickClientId et  Cci.QuickDataPosition= pQuickPosition
            return (null != CciOfQuickInput(pQuickClientId, pQuickPosition));
        }


        #region Debug
        public void Debug()
        {
            this.Debug(string.Empty, false);
        }
        public void Debug(string pClienIdFilter, bool pOnlyCiiHasChanged)
        {

            string[] filter = StrFunc.StringArrayList.StringListToStringArray(pClienIdFilter);
            bool isAll = ArrFunc.IsEmpty(filter);

            System.Diagnostics.Debug.WriteLine("--------------------------------------------------");
            foreach (CustomCaptureInfo cci in this)
            {
                bool isAdd = isAll;
                //
                if (false == isAdd)
                {
                    for (int i = 0; i < ArrFunc.Count(filter); i++)
                    {
                        isAdd = StrFunc.ContainsIn(cci.ClientId_WithoutPrefix, filter[i]);
                        if (isAdd)
                            break;
                    }
                }
                //
                if ((isAdd) && pOnlyCiiHasChanged)
                    isAdd = cci.HasChanged;
                //        
                if (isAdd)
                {
                    string msg = cci.clientId + ": ";
                    msg += cci.lastValue + " / " + cci.newValue;
                    //                    msg += (cci.IsAutoPostback      ? " - AutoPostback":string.Empty);
                    //                    msg += (cci.HasChanged          ? " - Changed":string.Empty);
                    msg += (cci.isInputByUser ? " - InputByUser" : string.Empty);
                    //                    msg += (cci.IsCalculateBySystem ? " - CalculateBySystem":string.Empty);
                    msg += (cci.HasError ? " - " + cci.errorMsg : string.Empty);

                    System.Diagnostics.Debug.WriteLine(msg);
                }

            }
        }
        #endregion Debug
        #region Debug_Change
        public void Debug_Change()
        {
            this.Debug(string.Empty, true);
        }
        #endregion Debug_Change
        #region DebugClientId
        public void DebugClientId()
        {
            foreach (CustomCaptureInfo cci in this)
            {
                System.Diagnostics.Debug.WriteLine(cci.clientId);
            }
        }
        #endregion DebugClientId


        /// <summary>
        /// 1/ Déversement des données issues des CCI, dans les classes du Document XML
        /// 2/ Gestion des incidences sur les autres données via 2 queues (High, Low)
        /// 3/ Appel récursif afin de gérer les éventuels nouveaux déversement suite à la gestion des incidences
        /// 4/ Purge des classes du Document XML
        /// </summary>
        public void Dump_ToDocument(int pGuard)
        {
            bool isRecursive = false;
            if (0 == pGuard)
                SynchronizeCcisFromQuickInput();
            //
            if (CciContainerSpecified)
            {
                CciContainer.Dump_ToDocument();

                // RD 20120810 [18069] 
                // Pour le mode Modification partielle (sans régénération des événements):
                // - Ne pas modifier d'autres données 
                // - Ne pas recalculer d'autres montants 
                if (false == Cst.Capture.IsModeUpdatePostEvts(m_CaptureMode))
                {
                    //Debug();
                    //
                    #region Management of queues

                    CustomCaptureInfo currentCci;
                    bool isFound = true;
                    while (isFound && (++pGuard < 999))
                    {
                        isFound = false;

                        while ((m_MyHigh.Count > 0) && (++pGuard < 999))
                        {
                            isRecursive = true;
                            currentCci = (CustomCaptureInfo)m_MyHigh.Dequeue();
                            //System.Diagnostics.Debug.WriteLine("High: " + currentCci.ClientId);
                            CciContainer.ProcessInitialize(currentCci);
                            //Debug();
                        }
                        if (m_MyLow.Count > 0)
                        {
                            isRecursive = true;
                            isFound = true;
                            currentCci = (CustomCaptureInfo)m_MyLow.Dequeue();
                            //System.Diagnostics.Debug.WriteLine("Low: " + currentCci.ClientId);
                            CciContainer.ProcessInitialize(currentCci);
                            //Debug();
                        }
                    }
                    if ((pGuard >= 999))
                    {
                        throw (new SpheresException(MethodInfo.GetCurrentMethod().Name, "Infinite Loop"));
                    }
                    #endregion Management of queues
                    if (isRecursive)
                        Dump_ToDocument(pGuard);
                }
            }
            //
            FinalizeQuickInput();

        }

        /// <summary>
        /// 1/ Déversement des CCI sur l'IHM
        /// 2/ Mise à Disabled de certains contrôles
        /// 3/ Reload de certaines DDL
        /// </summary>
        /// <param name="pPage"></param>
        public virtual void Dump_ToGUI(CciPageBase pPage)
        {
            pPage.AddAuditTimeStep("Start CustomCaptureInfosBase.Dump_ToGUI");

            bool isModeConsult = MethodsGUI.IsModeConsult(pPage);

            if (null != m_CciContainer)
                m_CciContainer.RefreshCciEnabled();

            string warningMsg = string.Empty;
            foreach (CustomCaptureInfo cci in this)
            {
                bool isControlEnabled = cci.isEnabled;

                // FI 20121126 [18224] appel à ma méthode GetCciControl
                //Control control = (Control)pPage.FindControl(cci.clientId);
                Control control = GetCciControl(cci.clientId, pPage);
                //
                if (null != control)
                {
                    switch (cci.ClientId_Prefix)
                    {
                        case Cst.TXT:
                        case Cst.QKI:
                            SetTextBox(control, cci, isModeConsult, isControlEnabled);
                            break;
                        case Cst.HSL:
                            SetHtmlSelect(control, cci, isControlEnabled);
                            break;
                        case Cst.DDL:
                            SetDropDown(control, cci, isModeConsult, isControlEnabled, pPage);
                            break;
                        case Cst.CHK:
                        case Cst.HCK:
                            SetCheckBox(control, cci, isControlEnabled);
                            break;
                    }

                    SetControlAttributs(control, cci, pPage);

                    // FI 20121126 [18224] appel à la méthode
                    SetCciDisplay(pPage, cci);
                }
            }

            if (pPage.focusMode == CciPageBase.FocusMode.Forced)
                SetFocus(pPage);

            if (StrFunc.IsFilled(warningMsg))
                JavaScript.DialogImmediate(pPage, warningMsg);


            pPage.AddAuditTimeStep("End CustomCaptureInfosBase.Dump_ToGUI");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCci"></param>
        /// <param name="pProcessQueue"></param>
        private void Enqueue(CustomCaptureInfo pCci, ProcessQueueEnum pProcessQueue)
        {
            if (IsUseProcessInitialize)
            {
                switch (pProcessQueue)
                {
                    case ProcessQueueEnum.High:
                        m_MyHigh.Enqueue(pCci);
                        break;
                    case ProcessQueueEnum.Low:
                        m_MyLow.Enqueue(pCci);
                        break;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pClientId_WithoutPrefix"></param>
        /// <param name="pProcessQueue"></param>
        public void Finalize(string pClientId_WithoutPrefix, ProcessQueueEnum pProcessQueue)
        {
            if (Contains(pClientId_WithoutPrefix))
            {
                CustomCaptureInfo cci = this[pClientId_WithoutPrefix];
                switch (pProcessQueue)
                {
                    case ProcessQueueEnum.High:
                    case ProcessQueueEnum.Low:
                        Enqueue(cci, pProcessQueue);
                        break;
                    default:
                        cci.Finalize(false == IsPreserveData);
                        break;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private void FinalizeQuickInput()
        {
            foreach (CustomCaptureInfo cci in this)
            {
                if (cci.IsQuickInput)
                {
                    SetNewValueQuickInput(cci); // Alimentation de NewValue du cci Quick (avec Formatage)
                    cci.Finalize(false);
                }
            }

        }

        /// <summary>
        /// Obtient la liste des ccis modifiés (lastValue!=newValue)
        /// </summary>
        /// <returns></returns>
        public CustomCaptureInfo[] GetArrayCciHasChanged()
        {
            CustomCaptureInfo[] ret = null;
            ArrayList al = new ArrayList();
            foreach (CustomCaptureInfo cci in this)
            {
                if (cci.HasChanged)
                    al.Add(cci);
            }
            //
            if (al.Count > 0)
                ret = (CustomCaptureInfo[])al.ToArray(typeof(CustomCaptureInfo));
            //
            return ret;

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pClientId_WithoutPrefix"></param>
        /// <returns></returns>
        public decimal GetDecimalNewValue(string pClientId_WithoutPrefix)
        {
            decimal ret = 0;
            if (this.Contains(pClientId_WithoutPrefix))
            {
                string value = this[pClientId_WithoutPrefix].newValue;
                if (StrFunc.IsEmpty(value))
                    ret = decimal.Zero;
                else
                    ret = DecFunc.DecValue(value, CultureInfo.InvariantCulture);
            }
            return ret;

        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public CustomCaptureInfo GetFirstCciEnabled()
        {
            CustomCaptureInfo ret = null;
            foreach (CustomCaptureInfo cci in this)
            {
                if (cci.isEnabled)
                {
                    ret = cci;
                    break;
                }
            }
            return ret;

        }

        /// <summary>
        /// Obtient un message d'erreur par concaténation des différentes propriétés errorMsg des ccis
        /// </summary>
        /// <returns></returns>
        public string GetErrorMessage()
        {
            bool isAllCciMissingMode = true;
            string[] errorMessage = null;
            return GetErrorMessage(out isAllCciMissingMode, ref errorMessage);

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pIsAllCciMissingMode"></param>
        /// <returns></returns>
        public string GetErrorMessage(out bool pIsAllCciMissingMode, ref string[] pShortErrorMessage)
        {
            pIsAllCciMissingMode = true;
            StrBuilder ret = new StrBuilder();
            int mesgNumber = -1;
            //
            // Get fisrt empty Item
            for (int i = 0; i < ArrFunc.Count(pShortErrorMessage); i++)
            {
                if (StrFunc.IsEmpty(pShortErrorMessage[i]))
                {
                    mesgNumber = i;
                    break;
                }
            }

            foreach (CustomCaptureInfo cci in this)
            {
                if (StrFunc.IsFilled(cci.errorMsg))
                {
                    CustomObject co = new CustomObject(cci.ClientId_WithoutPrefix);
                    //StrBuilder currentMsg = new StrBuilder().AppendFormat("{0}[{1}] : {2}", co.Resource, co.ClientId, cci.errorMsg);
                    StrBuilder currentMsg = new StrBuilder().AppendFormat("{0} : {1}" + Cst.CrLf + "[" + Ressource.GetString("Detail") + " : {2}]", co.Resource, cci.errorMsg, co.ClientId);
                    ret.Append(currentMsg.ToString() + Cst.CrLf);
                    //
                    currentMsg = new StrBuilder().AppendFormat("- {0}: <b>{1}</b> (id:{2})" + Cst.CrLf, co.Resource, cci.newValue, co.ClientId);
                    if ((-1 < mesgNumber) && (mesgNumber < ArrFunc.Count(pShortErrorMessage)))
                    {
                        pShortErrorMessage[mesgNumber] = currentMsg.ToString();
                        mesgNumber++;
                    }
                    //
                    if (false == cci.isMissingMode)
                        pIsAllCciMissingMode = false;
                }
            }
            return ret.ToString();

        }

        /// <summary>
        /// Retourne l'index de l'élément pClientId_WithoutPrefix dans la collection
        /// <para>Retoune -1 si l'élément est inexistant</para>
        /// </summary>
        /// <param name="pClientId_WithoutPrefix"></param>
        /// <returns></returns>
        public int GetIndex(string pClientId_WithoutPrefix)
        {
            int ret = -1;
            //
            if (0 < this.Count)
            {
                foreach (CustomCaptureInfo cci in this)
                {
                    ret++;
                    if (cci.ClientId_WithoutPrefix == pClientId_WithoutPrefix)
                        break;
                }
            }
            //
            return ret;

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pClientId_WithoutPrefix"></param>
        /// <returns></returns>
        public string GetLastValue(string pClientId_WithoutPrefix)
        {
            string ret = string.Empty;
            if (this.Contains(pClientId_WithoutPrefix))
                ret = this[pClientId_WithoutPrefix].lastValue;
            return ret;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pQuickClientId"></param>
        /// <returns></returns>
        private int GetMaxQuickDataPosition(string pQuickClientId)
        {
            int ret = 0;
            for (int i = 0; i < this.Count; i++)
            {
                if ((this[i].quickClientId == pQuickClientId) && (this[i].quickDataPosition > ret))
                {
                    ret = this[i].quickDataPosition;
                }
            }
            return ret;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pClientId_WithoutPrefix"></param>
        /// <returns></returns>
        public string GetNewValue(string pClientId_WithoutPrefix)
        {
            string ret = string.Empty;
            if (this.Contains(pClientId_WithoutPrefix))
                ret = this[pClientId_WithoutPrefix].newValue;
            return ret;
        }

        /// <summary>
        /// Initialisation des CCI à partir des données présentes dans le dataDocument
        /// </summary>
        /// <param name="pPage"></param>
        private void Initialize_FromDocument()
        {
            if (CciContainerSpecified)
            {
                //
                InitializeDocument_FromCci();
                CciContainer.AddCciSystem();
                CciContainer.Initialize_Document();
                CciContainer.Initialize_FromDocument();
            }
            InitializeQuickInput();
        }
        
        /// <summary>
        /// Instanciations des objets du dataDocument en fonction de la présence des CCIs
        /// </summary>
        public void InitializeDocument_FromCci()
        {
            if (CciContainerSpecified)
                CciContainer.Initialize_FromCci();
        }


        /// <summary>
        /// Initialisation des CCI à partir des données présentes dans les contrôles IHM
        /// </summary>
        /// <param name="pPage"></param>
        /// FI 20121126 [18224] modification de la signature de la méthode => pPage est de type CciPageBase
        public void Initialize_FromGUI(CciPageBase pPage)
        {
            pPage.AddAuditTimeStep("Start CustomCaptureInfosBase.Initialize_FromGUI");

            string eventTarget = string.Empty + pPage.Request.Params["__EVENTTARGET"];

            //Note: isProcessAllObject=true si la page est postée par autre chose qu'un contrôle de saisie (ie: menu consult, F5, ...)
            //bool isProcessAllObject = (false == eventTarget.StartsWith(Cst.DDL)) &&
            //                          (false == eventTarget.StartsWith(Cst.HSL)) &&
            //                          (false == eventTarget.StartsWith(Cst.CHK)) &&
            //                          (false == eventTarget.StartsWith(Cst.HCK)) &&
            //                          (false == eventTarget.StartsWith(Cst.TXT)) &&
            //                          (false == eventTarget.StartsWith(Cst.QKI)) &&
            //                          (false == eventTarget.StartsWith(Cst.BUT));

            foreach (CustomCaptureInfo cci in this)
            {
                cci.isLastInputByUser = false;
                string clientId = cci.clientId;
                string data = string.Empty;
                //Note: On traite ici les contrôles non AutoPostback et le contrôle qui a généré le Postback
                //if (isProcessAllObject || (!cci.isAutoPostback) || (clientId == eventTarget))
                //20060503 Mise en commentaire
                //On met à jour systématiquement, aucune incidence particulière car les dump dans le dataDocument, les calculs etc  sont effectués que lorsque Last!= New

                //FI 20121126 [18224] appel à la méthode GetCciControl
                //Control control = (Control)pPage.FindControl(clientId);
                Control control = GetCciControl(clientId, pPage);  
                
                if (null != control)
                {
                    //20090601 FI Add IsOK
                    bool isOk = true;
                    switch (cci.ClientId_Prefix)
                    {
                        case Cst.TXT:
                        case Cst.QKI:
                            data = ((TextBox)control).Text.Trim();
                            break;
                        case Cst.DDL:
                            data = ((DropDownList)control).SelectedValue;
                            break;
                        case Cst.HSL:
                            data = ((HtmlSelect)control).Value;
                            break;
                        case Cst.CHK:
                        case Cst.HCK:
                            PropertyInfo pty = control.GetType().GetProperty("Checked"); ;
                            data = (bool)pty.GetValue(control, null) ? Cst.FpML_Boolean_True : Cst.FpML_Boolean_False;
                            break;
                        default: // //20090601 FI exemple les boutons isolés (sans zone texte associé) passent ici => on affecte pas le cci
                            isOk = false;
                            break;
                    }
                    //
                    if (isOk)
                    {
                        //Lorsque newValue est alimentée avec un mot clef, on n'écrase pas ce mot clef par ce qui es affiché (data) correspond au libellé associé au mot clef
                        if (cci.IsNewValueKeyword && cci.display == data)
                            isOk = false;
                        //
                        if (isOk)
                        {
                            try
                            {
                                //Si preserve on conserve les données saisie (Ex pour Date : Today ne doit pas être interprété)
                                //20090427 FI Sur demande de PL les types de données numériques sont systématiquement interprétés (Mode REGULAR ou MODE TEMPLATE)
                                //si l'on saisit 100% => on veut dans le template 1 et non pas 100
                                if ((false == IsPreserveData) || (cci.IsTypeNumeric))
                                {
                                    //La saisie doit-elle être interprétée fonctionellement 
                                    //avant le traitement classique opérée ds cci.NewValueFromLiteral
                                    data = InterceptInput(cci, data);
                                    cci.NewValueFromLiteral = data;
                                }
                                else
                                {
                                    cci.newValue = data;
                                }
                            }
                            catch { cci.newValue = string.Empty; } // Eviter de planter en saisie 
                        }
                        //
                        if ((cci.HasChanged) && (clientId == eventTarget))
                        {
                            //Warning: Ne pas écrire: cci.IsInputByUser = cci.HasChanged 
                            cci.isInputByUser = true;
                            cci.isLastInputByUser = true;
                        }
                    }
                }
            }
            pPage.AddAuditTimeStep("End CustomCaptureInfosBase.Initialize_FromGUI");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCci"></param>
        /// <param name="pSql_Table"></param>
        /// <param name="pData"></param>
        public virtual void InitializeCci(CustomCaptureInfo pCci, SQL_Table pSql_Table, string pData)
        {
            InitializeCci(pCci, pSql_Table, pData, null);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCci"></param>
        /// <param name="pSql_Table"></param>
        /// <param name="pData"></param>
        /// <param name="pDisplay"></param>
        public virtual void InitializeCci(CustomCaptureInfo pCci, SQL_Table pSql_Table, string pData, string pDisplay)
        {
            pCci.Initialize(pSql_Table, pData, pDisplay);

        }

        /// <summary>
        /// Synchronize les différents pointeurs des cciContainers  avec le dataDocument
        /// </summary>
        public virtual void InitializeCciContainer()
        {
            m_CciContainer = null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pModeCapture"></param>
        public virtual void InitializeDocument(Cst.Capture.ModeEnum pModeCapture)
        {

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCci"></param>
        /// <param name="pDataInput"></param>
        /// <returns></returns>
        protected virtual string InterceptInput(CustomCaptureInfo pCci, string pDataInput)
        {
            return pDataInput;
        }

        /// <summary>
        /// 
        /// </summary>
        private void InitializeQuickInput()
        {

            // Initialisation  des Cci QuickInput à partir des Ccis 
            foreach (CustomCaptureInfo cci in this)
            {
                if (cci.IsQuickInput)
                {
                    string data = string.Empty;
                    string quickClientId = cci.ClientId_WithoutPrefix;
                    //
                    for (int i = 1; i < this.Count; i++)
                    {
                        if (ConstainsCciOfQuickInput(quickClientId, i))
                        {
                            if (cci.IsQuickFormatOTCml)
                                data += CciOfQuickInput(quickClientId, i).NewValueFmtToCurrentCulture + cci.quickSeparator;
                            else
                                data += CciOfQuickInput(quickClientId, i).newValue + cci.quickSeparator;
                        }
                    }
                    //
                    cci.Initialize(null, data);
                }
            }

        }

        /// <summary>
        /// Retourne l'élément associé au ClientId, cet élément est nécessairement un array
        /// </summary>
        public virtual Array GetArrayElement(string pClientId)
        {
            return null;

        }

        /// <summary>
        /// Retourne l'élément associé au ClientId, cet élément est nécessairement un array, retourne également l'object parent de l'array retoruné
        /// </summary>
        public virtual Array GetArrayElement(string pClientId, out object pParent)
        {

            pParent = null;
            return null;

        }

        /// <summary>
        /// Chargement des ccis à partir du document
        /// </summary>
        /// <param name="pPage"></param>
        public void LoadCapture(Cst.Capture.ModeEnum pModeCapture)
        {
            LoadCapture(pModeCapture, null);
        }

        /// <summary>
        /// Chargement des ccis à partir du document
        /// <para>Synchronisation des contrôles de la page {pPage}</para>
        /// </summary>
        /// <param name="pModeCapture"></param>
        /// <param name="pPage">null est possible</param>
        public void LoadCapture(Cst.Capture.ModeEnum pModeCapture, CciPageBase pPage)
        {
            if (null != pPage)
                pPage.AddAuditTimeStep("Start CustomCaptureInfosBase.LoadCapture");

            m_IsLoading = true;

            m_CaptureMode = pModeCapture;

            

            //Alimentation des CCI à partir du document 
            Initialize_FromDocument();

            //FI 12/04/07 => L'étape Dump_ToDocument ne doit pas être fait en mode consultation
            //FI 24/05/07 => oui il faut pour mettre à jours les BCs
            //if (pModeCapture != Cst.Capture.ModeEnum.Consult) 
            Dump_ToDocument(0);//20050314 PL pour Initialisation des mots clefs (Ex EntityOfUSer)

            if (null != pPage)
                Dump_ToGUI(pPage);

            m_IsLoading = false;

            if (null != pPage)
                pPage.AddAuditTimeStep("End CustomCaptureInfosBase.LoadCapture");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pClientId"></param>
        /// <returns></returns>
        public virtual string ShiftClientIdToDocumentElement(string pClientId)
        {
            return pClientId;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pClientId_WithoutPrefix"></param>
        public void Remove(string pClientId_WithoutPrefix)
        {
            if (Contains(pClientId_WithoutPrefix))
            {
                for (int i = this.Count - 1; -1 < i; i--)
                {
                    CustomCaptureInfo cci = this[i];
                    if (pClientId_WithoutPrefix == cci.ClientId_WithoutPrefix)
                    {
                        this.RemoveAt(i);
                        break;
                    }
                }
            }

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pTradeCci"></param>
        public void RemoveCciOf(IContainerCci pTradeCci)
        {
            for (int i = this.Count - 1; -1 < i; i--)
            {
                CustomCaptureInfo cci = this[i];
                if (pTradeCci.IsCciOfContainer(cci.ClientId_WithoutPrefix))
                    this.RemoveAt(i);
            }
        }

        /// <summary>
        /// Remove empty item, set specified flag and set default party before Validation
        /// </summary>
        public void RemoveLastItemInArray(string pPrefix)
        {
            if (CciContainerSpecified)
                ((IContainerArray)CciContainer).RemoveLastItemInArray(pPrefix);
        }

        /// <summary>
        /// Supprime tous la collection CCI, supprime les queues de unitilisée pour processInitialise
        /// </summary>
        public void Reset()
        {
            base.Clear();
            m_MyHigh.Clear();
            m_MyLow.Clear();
        }

        /// <summary>
        ///  Initialize from GUI, Dump To Document, clean UP
        /// </summary>
        /// <param name="pPage"></param>
        public void SaveCapture(Page pPage)
        {
            //Mise a jour Document Fpml
            UpdCapture(pPage);
            //
            //Nettoyage du Document Fpml => Ex supp des OtherPartyPayment non alimenté
            if ((CciContainerSpecified) && (false == IsPreserveData))  //Pas de cleanup sur un template par ex 
                CciContainer.CleanUp();
        }

        /// <summary>
        /// Affecte la propertie nommée {pPropertyName} avec {pValue}  
        /// </summary>
        /// <param name="pClientId"></param>
        /// <param name="pProperty"></param>
        /// <param name="pValue"></param>
        public void Set(string pClientId, string pPropertyName, object pValue)
        {

            CustomCaptureInfo cci = null;
            //FI 20110418 [17405] ccis.Contains en commentaire => Tuning
            //if (this.Contains(pClientId))
            cci = this[pClientId];
            //				
            if (null != cci)
            {
                PropertyInfo fld = cci.GetType().GetProperty(pPropertyName);
                if (null != fld)
                    fld.SetValue(cci, pValue, null);
            }

        }

        /// <summary>
        /// Finalize tous les ccis
        /// <para>Synchronize pour chaque cci la valeur last avec la valeur new</para>
        /// <para>Alimente le message d'erreur du cci si les données obligatoires sont non renseignées [uniquement si false == IsPreserveData]</para>
        /// </summary>
        public void FinaliseAll()
        {
            for (int i = 0; i < ArrFunc.Count(this); i++)
                this[i].Finalize(false == IsPreserveData);

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pClientId_WithoutPrefix"></param>
        /// <param name="pErrorMsg"></param>
        public void SetErrorMsg(string pClientId_WithoutPrefix, string pErrorMsg)
        {
            if (this.Contains(pClientId_WithoutPrefix))
                this[pClientId_WithoutPrefix].errorMsg = pErrorMsg;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pPage"></param>
        protected void SetFocus(CciPageBase pPage)
        {
            PlaceHolder plh = pPage.placeHolder;
            //20090622 PL Focus: gestion dynamique

            //eventTarget => Control à l'origine du post
            string eventTarget = string.Empty + pPage.Request.Params["__EVENTTARGET"];
            //activeElement => Control ayant le focus
            string activeElement = string.Empty + pPage.Request.Params["__ACTIVEELEMENT"];
            //
            bool isFound_CciEventTarget = false;
            bool isFound_CciEventTarget_Next = false;
            bool isFound_CciActiveElement = false;
            bool isActiveElement_AccessKeyNextBlock = false;
            //
            foreach (CustomCaptureInfo cci in this)
            {
                if (cci.clientId == eventTarget)
                {
                    //Cci en liaison avec le control HTML à l'origine du post de la page
                    isFound_CciEventTarget = true;

                    ImageButton button = plh.FindControl(Cst.BUT + cci.ClientId_WithoutPrefix) as ImageButton;
                    if (null != button && button.TabIndex != -1)
                        isFound_CciEventTarget_Next = true;

                    if (false == isFound_CciEventTarget_Next)
                    {
                        Button button2 = plh.FindControl(Cst.BUT + cci.ClientId_WithoutPrefix) as Button;
                        if (null != button2 && button2.TabIndex != -1)
                            isFound_CciEventTarget_Next = true;
                    }

                }
                else if (cci.clientId == activeElement)
                {
                    //Cci en liaison avec le control HTML actif lors du post de la page
                    //
                    //Soit le control ActiveElement est situé avant le control Eventarget -> Cela indique une remonté du circuit tabulaire (Shift-Tab)
                    //Soit le control ActiveElement dispose d'un AccessKey "NextBlock" -> Cela indique un (éventuel) déplacement vai AccessKey (Alt-*)
                    //NB: On laissera le focus tel qu'il est...
                    isFound_CciActiveElement = true;
                    isActiveElement_AccessKeyNextBlock = cci.IsAccessKeyNextBlock;
                }
                //
                #region isControlEnabled
                bool isControlEnabled = false;
                if (isFound_CciEventTarget && !isFound_CciEventTarget_Next)
                {
                    Control control = (Control)plh.FindControl(cci.clientId);
                    if (null != control)
                    {
                        switch (cci.ClientId_Prefix)
                        {
                            case Cst.TXT:
                            case Cst.QKI:
                                TextBox txt = (TextBox)control;
                                isControlEnabled = txt.Enabled;
                                break;
                            case Cst.HSL:
                                isControlEnabled = (false == ((WCHtmlSelect)control).Disabled);
                                break;
                            case Cst.DDL:
                                bool isUseControlBase = false;
                                try
                                {
                                    WCDropDownList2 ddl = (WCDropDownList2)control;
                                    isControlEnabled = ddl.Enabled && (!ddl.hasViewer);
                                }
                                catch { isUseControlBase = true; }
                                //
                                if (isUseControlBase)
                                {
                                    DropDownList ddl = (DropDownList)control;
                                    isControlEnabled = ddl.Enabled;
                                }
                                break;
                            case Cst.CHK:
                            case Cst.HCK:
                                PropertyInfo pty = control.GetType().GetProperty("Enabled");
                                isControlEnabled = (bool)pty.GetValue(control, null);
                                break;
                        }
                    }
                }
                #endregion
                //
                if (isFound_CciEventTarget && isControlEnabled && (!isFound_CciEventTarget_Next) && (cci.clientId != eventTarget))
                {
                    //Cci en liaison avec le 1er control HTML "enabled" suivant le control HTML à l'origine du post de la page
                    isFound_CciEventTarget_Next = true;
                    pPage.ActiveElementForced = cci.clientId;
                }
                else if (isFound_CciActiveElement && ((!isFound_CciEventTarget) || isActiveElement_AccessKeyNextBlock))
                {
                    pPage.ActiveElementForced = null;
                }
                //
                if (isFound_CciActiveElement)
                    break;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pClientId"></param>
        /// <param name="pNewValue"></param>
        public void SetNewValue(string pClientId, string pNewValue)
        {
            SetNewValue(pClientId, pNewValue, false);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pClientId"></param>
        /// <param name="pNewValue"></param>
        /// <param name="pbOnlyIfIsEmpty"></param>
        public void SetNewValue(string pClientId, string pNewValue, bool pbOnlyIfIsEmpty)
        {

            //pbOnlyIfIsEmpty => permet d'initialiser la new Value lorsqu'elle est vide
            CustomCaptureInfo cci = null;
            //
            if (this.Contains(pClientId))
                cci = this[pClientId];
            //
            if (null != cci)
            {
                if (pbOnlyIfIsEmpty && StrFunc.IsEmpty(cci.newValue))
                    cci.newValue = pNewValue;
                else
                    cci.newValue = pNewValue;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCci"></param>
        private void SetNewValueQuickInput(CustomCaptureInfo pCci)
        {

            if (pCci.IsQuickInput)
            {
                string data = string.Empty;
                string quickClientId = pCci.ClientId_WithoutPrefix;
                //
                for (int i = 1; i < Count; i++)
                {
                    if (ConstainsCciOfQuickInput(quickClientId, i))
                    {
                        if (pCci.IsQuickFormatOTCml)
                            data += CciOfQuickInput(quickClientId, i).NewValueFmtToCurrentCulture + pCci.quickSeparator;
                        else
                            data += CciOfQuickInput(quickClientId, i).newValue + pCci.quickSeparator;
                    }
                }
                pCci.newValue = data;
            }

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pClientId_WithoutPrefix"></param>
        /// <param name="pQuickClientId"></param>
        public void SetQuickClientId(string pClientId_WithoutPrefix, string pQuickClientId)
        {
            if (this.Contains(pClientId_WithoutPrefix))
            {
                this[pClientId_WithoutPrefix].quickClientId = pQuickClientId;
                this[pClientId_WithoutPrefix].quickDataPosition = GetMaxQuickDataPosition(pQuickClientId) + 1;
            }
        }

        /// <summary>
        /// Affecte la propriété newValue d'un cci, avec la nouvelle valeur saisie 
        /// <para>Ceci n'est effectué que si la propriété newValue du cci est identique à la valeur précédente</para>
        /// </summary>
        /// <param name="pClientId_WithoutPrefix">Représente le cci à synchroniser</param>
        /// <param name="pLastValue">valeur précédente</param>
        /// <param name="pNewValue">nouvelle précédente</param>
        public void Synchronize(string pClientId_WithoutPrefix, string pLastValue, string pNewValue)
        {
            Synchronize(pClientId_WithoutPrefix, pLastValue, pNewValue, false);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pClientId_WithoutPrefix"></param>
        /// <param name="pLastValue"></param>
        /// <param name="pNewValue"></param>
        /// <param name="pIsAlways"></param>
        public void Synchronize(string pClientId_WithoutPrefix, string pLastValue, string pNewValue, bool pIsAlways)
        {
            if (Contains(pClientId_WithoutPrefix))
            {
                CustomCaptureInfo cci = this[pClientId_WithoutPrefix];
                if ((cci.newValue == pLastValue) || (StrFunc.IsEmpty(cci.newValue) && StrFunc.IsEmpty(pLastValue)))
                {
                    if (cci.isMandatory || pIsAlways)
                        cci.newValue = pNewValue;
                    else if (StrFunc.IsFilled(cci.newValue))
                        cci.newValue = pNewValue;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private void SynchronizeCcisFromQuickInput()
        {

            foreach (CustomCaptureInfo cci in this)
            {
                if (cci.IsQuickInput && cci.HasChanged)
                {
                    //=> Mise à jour des Ccis associés
                    string[] aNewValue = cci.newValue.Split(cci.quickSeparator.ToCharArray());
                    for (int i = 0; i < aNewValue.Length; i++)
                    {
                        if (ConstainsCciOfQuickInput(cci.ClientId_WithoutPrefix, i + 1))
                        {
                            if (cci.IsQuickFormatOTCml)
                                try
                                {
                                    string data = aNewValue[i];
                                    data = InterceptInput(CciOfQuickInput(cci.ClientId_WithoutPrefix, i + 1), data);
                                    CciOfQuickInput(cci.ClientId_WithoutPrefix, i + 1).NewValueFromLiteral = data;
                                }
                                catch
                                {
                                    CciOfQuickInput(cci.ClientId_WithoutPrefix, i + 1).NewValueFromLiteral = string.Empty;// Eviter de planter en saisie 
                                }
                            else
                                CciOfQuickInput(cci.ClientId_WithoutPrefix, i + 1).newValue = aNewValue[i];
                        }
                    }
                }
            }

        }

        /// <summary>
        ///  Mise à jour du datadocument en fonction de CaptureMode, a voir non utilisé pour l'instant
        /// </summary>
        public virtual void UpdCaptureFromCaptureMode()
        {
        }

        /// <summary>
        /// Initialize from GUI And Dump To Document
        /// </summary>
        /// <param name="pPage"></param>
        public virtual void UpdCapture(Page pPage)
        {
            //Mise a jour From GUI
            Initialize_FromGUI((CciPageBase)pPage);
            //Mise a jour From Document Fmpl
            Dump_ToDocument(0);
        }

        /// <summary>
        /// Initialize from GUI, Dump To Document, Display  In GUI 
        /// </summary>
        /// <param name="pPage"></param>
        public void UpdCaptureAndDisplay(CciPageBase pPage)
        {
            UpdCapture(pPage);
            //
            Dump_ToGUI(pPage);
        }
        /// <summary>
        /// Retourne le control dont l'id vaut {pClientId}
        /// </summary>
        /// <param name="pClientId"></param>
        /// <param name="pPage"></param>
        /// <returns></returns>
        /// FI 20121126 [18224] La méthode FindControl n'est plus utilisée car pas performante
        protected static Control GetCciControl(string pClientId, CciPageBase pPage)
        {
            Control ret = pPage.GetCciControl(pClientId);
            return ret;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="plh"></param>
        /// <param name="cci"></param>
        ///FI 20121126 [18224] Modification de la signature de la fonction
        protected static void SetCciDisplay(CciPageBase pPage, CustomCaptureInfo cci)
        {
            //Existence d'un label associé (ie: affichage d'un libellé long)
            //Control control = (Control)plh.FindControl(Cst.DSP + cci.ClientId_WithoutPrefix);
            // FI 20121126 [18224] appel à la méthode GetCciControl
            Control control = GetCciControl(Cst.DSP + cci.ClientId_WithoutPrefix, pPage);
            if (null != control)
            {
                string msg = string.Empty;
                Color color = Color.Empty;
                Color backColor = Color.Empty;
                //
                if (cci.HasError)
                {
                    msg = cci.errorMsg;
                    color = Color.Red;
                    // EG 20100823 Reverse LBL if cci.HasError
                    backColor = Color.White;
                }
                else
                {
                    if (StrFunc.IsFilled(cci.display) && (false == cci.IsNewValueKeyword))
                        msg = cci.display;
                }
                //
                Label lbl = (Label)control;
                lbl.ToolTip = msg;
                lbl.Text = msg;
                lbl.ForeColor = color;
                lbl.BackColor = backColor;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="control"></param>
        /// <param name="cci"></param>
        /// <param name="isModeConsult"></param>
        /// <param name="isControlEnabled"></param>
        /// <param name="pPage"></param>
        protected virtual void SetDropDown(Control control, CustomCaptureInfo cci, bool isModeConsult, bool isControlEnabled, CciPageBase pPage)
        {
            DropDownList ddl = control as DropDownList;
            if (null == ddl)
                throw new Exception(StrFunc.AppendFormat("control {0} is not a  DropDown", cci.clientId));

            string data = cci.newValue;

            if (!(cci.isMandatory))
                ControlsTools.DDLLoad_AddListItemEmptyEmpty(ddl);

            ddl.Enabled = isControlEnabled;

            bool isFound = ControlsTools.DDLSelectByValue(ddl, data);

            if ((!isFound) && (StrFunc.IsFilled(data)))
            {
                string warningMsg = string.Empty;

                if (StrFunc.IsEmpty(warningMsg))
                    warningMsg = Ressource.GetString("Msg_DataUnavailableOrRemoved", "Warning: Data disabled or removed !") + Cst.CrLf;
                //
                //FI 20121126 [18224] FindControl sur pPage.placeHolder 
                Label label = pPage.placeHolder.FindControl(Cst.LBL + cci.ClientId_WithoutPrefix) as Label;
                string caption = cci.ClientId_WithoutPrefix;
                if (null != label)
                    caption = label.Text;
                warningMsg += Cst.CrLf + caption + ": " + data;
                //
                string msg_tmp = " " + Ressource.GetString("Msg_UnavailableOrRemoved", "[disabled or removed]");
                //
                ListItem liUnavailable = new ListItem(data + msg_tmp, data);
                liUnavailable.Attributes.Add("style", "color:#FFFFFF;background-color:#AE0303");
                ddl.Items.Add(liUnavailable);
                isFound = ControlsTools.DDLSelectByValue(ddl, data);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="control"></param>
        /// <param name="cci"></param>
        /// <param name="isControlEnabled"></param>
        protected virtual void SetTextBox(Control control, CustomCaptureInfo cci, bool isModeConsult, bool isControlEnabled)
        {
            string data = string.Empty;
            //20090427 FI Sur demande de PL les types de données numériques sont systématiquement interprétés (Mode REGULAR ou MODE TEMPLATE)
            //si l'on saisit 100% => on veut dans le template 1 et non pas 100
            if ((IsPreserveData) && (false == cci.IsTypeDecimal))
                data = cci.newValue;
            else
                data = cci.NewValueFmtToCurrentCulture;
            //
            if (cci.IsNewValueKeyword && (false == IsPreserveData))
                data = cci.display;
            //
            TextBox txt = control as TextBox;
            if (null == txt)
                throw new Exception(StrFunc.AppendFormat("control {0} is not a TextBox", cci.clientId));

            txt.Text = data;
            //
            ControlsTools.RemoveStyle(txt.Style, "color");
            if (StrFunc.IsFilled(cci.errorMsg))
                ControlsTools.SetStyleList(txt.Style, "color:#AE0303");
            else if (cci.IsNewValueKeywordAlert)
                ControlsTools.SetStyleList(txt.Style, "color:#AE0303");
            else if (cci.IsNewValueKeywordWarning)
                ControlsTools.SetStyleList(txt.Style, "color:#EF7A27");
            else if (cci.IsNewValueKeyword)
                ControlsTools.SetStyleList(txt.Style, "color:#51AD26");
            //
            txt.Enabled = isControlEnabled;
            //
            if (StrFunc.IsFilled(txt.CssClass))
            {
                string cssClassBase = EFSCssClass.GetCssClassBase(txt.CssClass);
                if (StrFunc.IsFilled(cssClassBase))
                {
                    bool isMultiline = StrFunc.ContainsIn(txt.CssClass, "Multiline");
                    txt.CssClass = EFSCssClass.GetCssClass(cci.IsTypeInt || cci.IsTypeDecimal, cci.isMandatory, isMultiline, isModeConsult || txt.ReadOnly, cssClassBase);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="control"></param>
        /// <param name="cci"></param>
        /// <param name="isControlEnabled"></param>
        protected virtual void SetHtmlSelect(Control control, CustomCaptureInfo cci, bool isControlEnabled)
        {
            string data = cci.newValue;
            ControlsTools.DDLSelectByValue((HtmlSelect)control, data);
            ((WCHtmlSelect)control).Disabled = (false == isControlEnabled);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="control"></param>
        /// <param name="cci"></param>
        /// <param name="isControlEnabled"></param>
        protected virtual void SetCheckBox(Control control, CustomCaptureInfo cci, bool isControlEnabled)
        {
            control.GetType().GetProperty("Enabled").SetValue(control, isControlEnabled, null);
            control.GetType().GetProperty("Checked").SetValue(control, cci.IsFilledValue, null);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="control"></param>
        /// <param name="cci"></param>
        /// <param name="pPage"></param>
        protected virtual void SetControlAttributs(Control control, CustomCaptureInfo cci, PageBase pPage)
        {

            IAttributeAccessor controlAttributAccessor = (IAttributeAccessor)control;
            string value = controlAttributAccessor.GetAttribute("displaywhenempty");
            //
            if (StrFunc.IsFilled(value) && BoolFunc.IsFalse(value))
            {
                pPage.SetStyleDisplay(control, "display:none");
                if (cci.IsFilled)
                    pPage.RemoveStyleDisplay(control);

                WebControl webCtrl = control as WebControl;
                if (null != webCtrl)
                    webCtrl.Attributes.Remove("displaywhenempty");
            }
            //
            value = controlAttributAccessor.GetAttribute("isddlbanner");
            if (StrFunc.IsFilled(value))
            {
                WebControl webCtrl = control as WebControl;
                if (null != webCtrl)
                    webCtrl.Attributes.Remove("isddlbanner");
            }
        }


        #endregion Methods
    }

    /// <summary>
    /// CustomCaptureInfo: Contient les informations d'une zone de saisie sur un écran personnalisé. 
    /// Attention, toute nouveau member doit être ajouté dans CustomCaptureInfoDynamicData
    /// </summary>
    public class CustomCaptureInfo : ICloneable
    {
        #region constantes
        /// <summary>
        /// Constante utilisée pour identifier des mots clefs 
        /// </summary>
        public const string Keyword = "#Keyword#:";
        public const string KeywordAlert = Keyword + "#Alert#:";
        public const string KeywordWarning = Keyword + "#Warning#:";
        #endregion

        #region public Enum
        public enum CloneMode
        {
            CciAttribut,
            CciData,
            CciAll
        }
        #endregion

        #region  Members
        private string _clientId;
        private string _accessKey;
        /// <summary>
        /// 
        /// </summary>
        [Obsolete("do not used", false)] 
        private bool _isAutoPostback;
        private bool _isMandatory;
        private bool _isMissingMode;
        private bool _isEnabled;
        private TypeData.TypeDataEnum _dataType;
        private EFSRegex.TypeRegex _regex;
        /// <summary>
        /// 
        /// </summary>
        [Obsolete("do not used", false)] 
        private string _listRetrieval;
        private string _lastValue;
        private string _newValue;
        private bool _isInputByUser;
        private bool _isLastInputByUser;
        private bool _isCalculateBySystem;
        private string _errorMsg;
        private string _display;
        private SQL_Table _sql_Table;
        private SQL_Table _lastSql_Table;
        private string _quickClientId;
        private int _quickDataPosition;
        private string _quickSeparator;
        private string _quickFormat;
        /// <summary>
        /// 
        /// </summary>
        [Obsolete("do not used", false)] 
        private string _typeRelativeTo;
        #endregion Members

        #region property
        
        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlAttributeAttribute("clientId", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string clientId
        {
            get
            {
                return _clientId;
            }
            set
            {
                _clientId = value;
            }
        }
        
        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlAttributeAttribute("accessKey", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string accessKey
        {
            get
            {
                return _accessKey;
            }
            set
            {
                _accessKey = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool IsAccessKeyNextBlock
        {
            get
            {
                return (StrFunc.IsFilled(accessKey)
                    && (accessKey == SystemSettings.GetAppSettings("Spheres_NextBlock_AccessKey", "N")));
            }
        }
        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public string display
        {
            get
            {
                return _display;
            }
            set
            {
                _display = value;
            }
        }
        
        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public string errorMsg
        {
            get
            {
                return _errorMsg;
            }
            set
            {
                _errorMsg = value;
            }
        }
        
        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlAttributeAttribute("dataType", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public TypeData.TypeDataEnum dataType
        {
            get
            {
                return _dataType;
            }
            set
            {
                _dataType = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        [Obsolete("do not used",false)]  
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public string listRetrieval
        {
            get
            {
                return _listRetrieval;
            }
            set
            {
                _listRetrieval = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        [Obsolete("do not used", false)]  
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool IsDynamicListRetrieval
        {
            get
            {
                return (StrFunc.IsFilled(listRetrieval) && listRetrieval.Contains(Cst.DA_START2));
            }
        }

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool regexSpecified;
        
        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlAttributeAttribute("regex", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public EFSRegex.TypeRegex regex
        {
            get
            {
                return _regex;
            }
            set
            {
                _regex = value;
                regexSpecified = (EFSRegex.TypeRegex.None != _regex);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        [Obsolete("Do not used",false)] 
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public string typeRelaTiveTo
        {
            get
            {
                return _typeRelativeTo;
            }
            set
            {
                _typeRelativeTo = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        [Obsolete("do not used", false)]
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool isAutoPostback
        {
            get
            {
                return _isAutoPostback;
            }
            set
            {
                _isAutoPostback = value;
            }
        }
        
        
        /// <summary>
        /// 
        /// </summary>
        // RD 20100819 [] Pour l'Import
        [System.Xml.Serialization.XmlAttributeAttribute("mandatory", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public bool isMandatory
        {
            get
            {
                return _isMandatory;
            }
            set
            {
                _isMandatory = value;
                //
                //FI 20100403 Spheres supprime le message d'erreur ISMANDATORY lorsque le cci est optionel
                if (false == isMandatory && StrFunc.IsFilled(errorMsg))
                    errorMsg = errorMsg.Replace(Ressource.GetString("ISMANDATORY"), string.Empty);
            }
        }
        
        
        
        /// <summary>
        /// 
        /// </summary>
        /// RD 20120312 [] Pour l'Import
        [System.Xml.Serialization.XmlAttributeAttribute("missingMode", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public bool isMissingMode
        {
            get { return _isMissingMode; }
            set { _isMissingMode = value; }
        }
        
        
        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool isEnabled
        {
            get
            {
                return _isEnabled;
            }
            set
            {
                _isEnabled = value;
            }
        }
        
        
        /// <summary>
        /// Retourne true lorsque le cci a été saisi au moins 1 fois
        /// <para>Lorsque la collection ccis est valorisée depuis IHM, retourne true uniquement lorsque le control associé génère un post sur la page</para>
        /// </summary>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool isInputByUser
        {
            get
            {
                return _isInputByUser;
            }
            set
            {
                _isInputByUser = value;
            }
        }
        
        
        /// <summary>
        /// Retourne true lorsque le cci est le dernier a être saisi (cci à l'origine du dump)
        /// <para>Propriété à utiliser lorsque la collection ccis est valorisée depuis IHM (voir CustomCaptureInfosBase.Initialize_FromGUI)</para>
        /// <para>Lorsque la collection ccis est valorisée depuis IHM, retourne true uniquement lorsque le control associé est celui qui vient de poster la page</para>
        /// </summary>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool isLastInputByUser
        {
            get
            {
                return _isLastInputByUser;
            }
            set
            {
                _isLastInputByUser = value;
            }
        }
        


        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool isCalculateBySystem
        {
            get
            {
                return _isCalculateBySystem;
            }
            set
            {
                _isCalculateBySystem = value;
            }
        }
        

        /// <summary>
        /// Valeur courante
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute("value", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string newValue
        {
            get
            {
                return _newValue;
            }
            set
            {
                //PL 20091229 Add test value on boolean datatype [On Oracle® (Boolean field is NUMBER(1) and data is 0 or 1)]
                if ((this.dataType == TypeData.TypeDataEnum.@bool)
                    && (value != Boolean.TrueString.ToLower()) && (value != Boolean.FalseString.ToLower()))
                {
                    if (BoolFunc.IsTrue(value))
                        _newValue = Boolean.TrueString.ToLower();
                    else
                        _newValue = Boolean.FalseString.ToLower();
                }
                else
                {
                    _newValue = value;
                }
            }
        }

       /// <summary>
        /// Valeur précédente
        /// </summary>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public string lastValue
        {
            get
            {
                return _lastValue;
            }
            set
            {
                //PL 20091229 Add test value on boolean datatype [On Oracle® (Boolean field is NUMBER(1) and data is 0 or 1)]
                if ((this.dataType == TypeData.TypeDataEnum.@bool)
                    && (value != Boolean.TrueString.ToLower()) && (value != Boolean.FalseString.ToLower()))
                {
                    if (BoolFunc.IsTrue(value))
                        _lastValue = Boolean.TrueString.ToLower();
                    else
                        _lastValue = Boolean.FalseString.ToLower();
                }
                else
                {
                    _lastValue = value;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public SQL_Table sql_Table
        {
            get
            {
                return _sql_Table;
            }
            set
            {
                _sql_Table = value;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public SQL_Table lastSql_Table
        {
            get
            {
                return _lastSql_Table;
            }
            set
            {
                _lastSql_Table = value;
            }
        }
        //
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public string quickSeparator
        {
            set
            {
                _quickSeparator = string.Empty;
                if (this.IsQuickInput)
                    _quickSeparator = value;
            }
            get
            {
                return _quickSeparator;
            }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public string quickFormat
        {
            set
            {
                _quickFormat = string.Empty;
                if (IsQuickInput)
                {
                    _quickFormat = "OTCml"; //default
                    if (StrFunc.IsFilled(value))
                        _quickFormat = value;
                }
            }
            get { return _quickFormat; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public string quickClientId
        {
            get
            {
                return _quickClientId;
            }
            set
            {
                _quickClientId = value;
            }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public int quickDataPosition
        {
            get
            {
                return _quickDataPosition;
            }
            set
            {
                _quickDataPosition = value;
            }
        }
        //
        //		
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public string ClientId_WithoutPrefix
        {
            get
            {
                int lenPrefix = ClientId_Prefix.Length;
                return clientId.Substring(lenPrefix);
            }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public string ClientId_Prefix
        {
            get
            {
                if (3 <= clientId.Length)
                    return clientId.Substring(0, 3);
                else
                    return string.Empty;
            }
        }
        /// <summary>
        /// Obtient true si la valeur représentée par lastValue est différente la valeur représentée par newValue 
        /// </summary>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool HasChanged
        {
            get
            {
                bool isDefaultTest = false;
                bool ret = false;
                if (IsTypeDecimal)
                {
                    ret = (DecValue(false) != DecValue(true));
                    if ((false == ret) && (DecValue(true) == decimal.Zero))
                        isDefaultTest = true;
                }
                else if (IsTypeDate)
                {
                    ret = (DateValue(false).Date != DateValue(true).Date);
                }
                else if (IsTypeTime)
                {
                    ret = (TimeValue(false) != TimeValue(true));
                }
                else if (IsTypeBool)
                {
                    ret = BoolFunc.IsTrue(newValue) != BoolFunc.IsTrue(lastValue);
                }
                else
                {
                    isDefaultTest = true;
                }
                //
                if (isDefaultTest)
                {
                    if (StrFunc.IsEmpty(newValue))
                        ret = StrFunc.IsFilled(lastValue);
                    else
                        ret = (lastValue != newValue);
                }
                return ret;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool HasError
        {
            get { return StrFunc.IsFilled(errorMsg); }
        }
        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool IsFilled
        {
            get { return StrFunc.IsFilled(newValue); }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool IsEmpty
        {
            get { return StrFunc.IsEmpty(newValue); }
        }
        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool IsLastFilled
        {
            get { return StrFunc.IsFilled(lastValue); }
        }
        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool IsLastEmpty
        {
            get { return StrFunc.IsEmpty(lastValue); }
        }
        
        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool IsFilledValue
        {
            get
            {
                bool ret = false;
                try
                {

                    if (IsTypeDecimal)
                        ret = StrFunc.IsDecimalInvariantFilled(newValue);
                    else if (IsTypeInt)
                        ret = StrFunc.IsIntegerInvariantFilled(newValue);
                    else if (IsTypeDate)
                        ret = DtFunc.IsDateTimeFilled(new DtFunc().StringToDateTime(newValue, DtFunc.FmtISODate));
                    else if (IsTypeBool)
                        ret = BoolFunc.IsTrue(newValue);
                    else
                        ret = IsFilled;
                }
                catch { ret = false; }
                return ret;
            }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool IsEmptyValue
        {
            get
            {
                return !IsFilledValue;
            }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool IsTypeString
        {
            get { return (TypeData.IsTypeString(dataType)); }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool IsTypeBool
        {
            get { return (TypeData.IsTypeBool(dataType)); }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool IsTypeInt
        {
            get { return (TypeData.IsTypeInt(dataType)); }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool IsTypeDecimal
        {
            get { return (TypeData.IsTypeDec(dataType)); }
        }

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool IsTypeNumeric
        {
            get { return (IsTypeDecimal || IsTypeInt); }
        }

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool IsTypeDate
        {
            get { return (TypeData.IsTypeDate(dataType)); }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool IsTypeDateTime
        {
            get { return (TypeData.IsTypeDateTime(dataType)); }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool IsTypeTime
        {
            get { return (TypeData.IsTypeTime(dataType)); }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool IsRegexFixedRate
        {
            get
            {
                return (EFSRegex.TypeRegex.RegexFixedRate == regex) ||
                        (EFSRegex.TypeRegex.RegexFixedRateExtend == regex ||
                        (EFSRegex.TypeRegex.RegexFixedRatePercent == regex));
            }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool IsRegexPercent
        {
            get
            {
                return ((EFSRegex.TypeRegex.RegexPercent == regex) ||
                          (EFSRegex.TypeRegex.RegexPercentExtend == regex) ||
                          IsRegexPercentFraction
                        );
            }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool IsRegexPercentFraction
        {
            get { return (EFSRegex.TypeRegex.RegexPercentFraction == regex); }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool IsRegexRate
        {
            // Cci FixedRate or FloatingRate
            get { return (EFSRegex.TypeRegex.RegexRate == regex); }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool IsRegexFxRate
        {
            get
            {
                return (EFSRegex.TypeRegex.RegexFxRate == regex) || (EFSRegex.TypeRegex.RegexFxRateExtend == regex);
            }

        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool IsRegexAmount
        {
            get
            {
                return (EFSRegex.TypeRegex.RegexAmount == regex) ||
                       (EFSRegex.TypeRegex.RegexAmountSigned == regex) ||
                       (EFSRegex.TypeRegex.RegexAmountExtend == regex) ||
                       (EFSRegex.TypeRegex.RegexAmountSignedExtend == regex);
            }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool IsFixedRate
        {
            get
            {
                bool bRet = (IsRegexFixedRate);
                if (!bRet)
                    bRet = (IsRegexRate && RateTools.IsFixedRate(newValue));
                return bRet;
            }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool IsQuickInput
        {
            get { return (Cst.QKI == this.ClientId_Prefix); }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool IsQuickFormatOTCml
        {
            get { return (_quickFormat == "OTCml"); }
        }
        /// <summary>
        /// 
        /// </summary>
        [Obsolete("Do not used", false)] 
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool IsRelaTiveTo
        {
            get
            {
                return StrFunc.IsFilled(_typeRelativeTo);
            }
        }
        /// <summary>
        /// Obtient true si newValue contient un mot clef (ie comme par #Keyword:)  
        /// </summary>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool IsNewValueKeyword
        {
            get
            {
                bool ret = false;
                ret = StrFunc.IsFilled(newValue) && newValue.StartsWith(Keyword);
                return ret;
            }
        }
        /// <summary>
        /// Obtient true si newValue contient un mot clef (ie comme par #KeywordAlert:)  
        /// </summary>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool IsNewValueKeywordAlert
        {
            get
            {
                bool ret = false;
                ret = StrFunc.IsFilled(newValue) && newValue.StartsWith(KeywordAlert);
                return ret;
            }
        }
        /// <summary>
        /// Obtient true si newValue contient un mot clef (ie comme par #KeywordWarning:)  
        /// </summary>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool IsNewValueKeywordWarning
        {
            get
            {
                bool ret = false;
                ret = StrFunc.IsFilled(newValue) && newValue.StartsWith(KeywordWarning);
                return ret;
            }
        }

        /// <summary>
        /// Onbient newValue formattée selon la culture de la station
        /// </summary>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public string NewValueFmtToCurrentCulture
        {
            // Format NewValue With CurrentCulture
            get
            {
                string data = newValue;
                if (StrFunc.IsFilled(data))
                {
                    #region isDecimal
                    if (IsFixedRate || IsRegexPercent)
                    {
                        if (false == IsRegexPercentFraction || (IsRegexPercentFraction && IsDataValidForFixedRate(data)))
                        {
                            FixedRate fixedRate = new FixedRate(data, CultureInfo.InvariantCulture);
                            data = fixedRate.ToString();
                            fixedRate = null;
                        }
                        else if (IsRegexPercentFraction)
                        {
                            Fraction fraction = new Fraction(data, true);
                            data = fraction.ToString();
                        }
                    }
                    else if (IsRegexFxRate)
                    {
                        EFSTools.FxRate fxRate = new FxRate(data, CultureInfo.InvariantCulture);
                        data = fxRate.ToString();
                        fxRate = null;
                    }
                    else if (IsTypeDecimal)
                        data = StrFunc.FmtDecimalToCurrentCulture(data);
                    #endregion
                    //
                    #region IsTypeInt
                    // 20090511 RD
                    // Pour afficher un Int avec séparateur de milliers
                    if (IsTypeInt)
                        data = StrFunc.FmtIntegerToCurrentCulture(data);
                    #endregion
                    //
                    #region IsTypeDate
                    if (IsTypeDate)
                        data = new DtFunc().GetDateString(data);
                    //
                    if (IsTypeDateTime)
                        data = new DtFunc().GetDateTimeString(data);
                    #endregion
                    //
                    #region IsTypeTime
                    if (IsTypeTime)
                        data = new DtFunc().GetLongTimeString(data);
                    #endregion
                }
                return data;
            }
        }

        /// <summary>
        /// Définit newValue à partir du donné formattée selon la culture de la station
        /// </summary>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public string NewValueFromLiteral
        {
            //Set New Value With FmplFormat (Invariant Culture)
            set
            {
                string data = value;
                //
                if (StrFunc.IsFilled(data))
                {
                    #region isDecimal
                    if (IsRegexFixedRate || IsDataValidForFixedRate(data) || IsRegexPercent)
                    {
                        //
                        //data = CciToolsBase.ReplaceDecimalCorrectValue(data);
                        //
                        if (false == IsRegexPercentFraction || (IsRegexPercentFraction && IsDataValidForFixedRate(data)))
                        {
                            if (!data.EndsWith("%"))
                                data += " %";
                            //
                            FixedRate fixedRate = new FixedRate(data);
                            data = StrFunc.FmtDecimalToInvariantCulture(fixedRate.Value);
                        }
                        else
                        {
                            try
                            {
                                if (EFSRegex.IsMatch(data, EFSRegex.TypeRegex.RegexPercentFraction))
                                {
                                    string dataValue = data.Replace(" ", String.Empty);
                                    dataValue = data.Replace("%", String.Empty);
                                    data = StrFunc.FmtFractionToInvariantCulture(dataValue);
                                }
                                else
                                    data = string.Empty;
                            }
                            catch { data = string.Empty; }
                        }
                    }
                    else if (IsRegexFxRate)
                    {
                        FxRate fxRate = new FxRate(data);
                        data = StrFunc.FmtDecimalToInvariantCulture(fxRate.Value);
                    }
                    else if (IsRegexAmount)
                    {
                        data = StrFunc.FmtDecimalToInvariantCulture2(data);
                    }
                    else if (IsTypeDecimal)
                    {
                        data = StrFunc.FmtDecimalToInvariantCulture(data);
                    }
                    #endregion
                    //
                    #region IsTypeInt
                    if (IsTypeInt)
                    {
                        // 20090511 RD
                        // Pour afficher un Int avec séparateur de milliers                    
                        data = StrFunc.FmtIntegerToInvariantCulture2(data);
                    }
                    #endregion
                    //
                    #region isDate
                    if (IsTypeDate)
                        data = new DtFunc().GetDateTimeString(data, DtFunc.FmtISODate);
                    #endregion
                    //
                    #region isDateTime
                    if (IsTypeDateTime)
                        data = new DtFunc().GetDateTimeString(data, DtFunc.FmtISODateTime);
                    #endregion
                    //						
                    #region isTime
                    if (IsTypeTime)
                        //data = DtFunc.GetDateTimeString(data,DtFunc.FmtISOTime);
                        data = new DtFunc().GetTimeString(data, DtFunc.FmtISOTime);
                    #endregion
                    //
                    #region isbool
                    if (IsTypeBool)
                        try
                        {
                            bool isOk = BoolFunc.IsTrue(data);
                            data = isOk ? Cst.FpML_Boolean_True : Cst.FpML_Boolean_False;
                        }
                        catch { data = "false"; }
                    #endregion
                }
                newValue = data;
            }
        }
        #endregion property

        #region constructors
        public CustomCaptureInfo(string pClientId, string pAccessKey,
            bool pIsMandatory, TypeData.TypeDataEnum pDataType,
            bool pIsAutoPostback, bool pIsEnabled,
            EFSRegex.TypeRegex pRegEx)
        {
            clientId = pClientId;
            accessKey = pAccessKey;
            isMandatory = pIsMandatory;
            isEnabled = pIsEnabled;
            dataType = pDataType;
            isAutoPostback = pIsAutoPostback;
            errorMsg = string.Empty;

            regex = pRegEx;// EFSRegex.TypeRegex.None;
            listRetrieval = string.Empty;
            quickClientId = string.Empty;
            quickDataPosition = 0;

            quickSeparator = ";";
            quickFormat = string.Empty;
            typeRelaTiveTo = string.Empty;
        }

        public CustomCaptureInfo(string pClientId, bool pIsMandatory, TypeData.TypeDataEnum pDataType)
            : this(pClientId, null, pIsMandatory, pDataType, false, true, EFSRegex.TypeRegex.None) { }
        public CustomCaptureInfo(string pClientId, bool pIsMandatory, TypeData.TypeDataEnum pDataType, bool pIsEnabled)
            : this(pClientId, null, pIsMandatory, pDataType, false, pIsEnabled, EFSRegex.TypeRegex.None) { }
        public CustomCaptureInfo(string pClientId, bool pIsMandatory, TypeData.TypeDataEnum pDataType, bool pIsAutoPostback, bool pIsEnabled, string pListRetrieval, string pRelativeTo)
            : this(pClientId, null, pIsMandatory, pDataType, pIsAutoPostback, pIsEnabled, EFSRegex.TypeRegex.None)
        {
            listRetrieval = pListRetrieval;
            typeRelaTiveTo = pRelativeTo;
        }
        public CustomCaptureInfo(string pClientId, bool pIsMandatory, TypeData.TypeDataEnum pDataType, bool pIsAutoPostback, bool pIsEnabled)
            : this(pClientId, null, pIsMandatory, pDataType, pIsAutoPostback, pIsEnabled, EFSRegex.TypeRegex.None) { }
        public CustomCaptureInfo(string pClientId, bool pIsMandatory, TypeData.TypeDataEnum pDataType, bool pIsAutoPostback, bool pIsEnabled, EFSRegex.TypeRegex pRegEx)
            : this(pClientId, null, pIsMandatory, pDataType, pIsAutoPostback, pIsEnabled, pRegEx) { }
        public CustomCaptureInfo()
            : this(string.Empty, null, false, TypeData.TypeDataEnum.unknown, false, false, EFSRegex.TypeRegex.None) { }
        #endregion constructors

        #region Methodes
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pData"></param>
        /// <returns></returns>
        public bool IsDataValidForFixedRate(string pData)
        {
            bool ret = false;
            if (IsRegexRate || IsRegexPercentFraction)
            {
                if (!pData.EndsWith("%"))
                    pData += " %";
                ret = EFSRegex.IsMatch(pData, EFSRegex.TypeRegex.RegexFixedRatePercent);
            }
            return ret;
        }

        /// <summary>
        /// 
        /// </summary>
        public void Reset()
        {
            // RD 20091228 [16809] Confirmation indicators for each party
            Reset(string.Empty);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pDefaultData"></param>
        public void Reset(string pDefaultData)
        {
            newValue = pDefaultData;
            errorMsg = string.Empty;
            sql_Table = null;
            lastSql_Table = null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="plistRetrieval"></param>
        /// <returns></returns>
        [Obsolete("do not used",false)] 
        public bool IsListRetrieval(string plistRetrieval)
        {
            return (listRetrieval == plistRetrieval.ToLower());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pSql_Table"></param>
        /// <param name="pData"></param>
        public void Initialize(SQL_Table pSql_Table, string pData)
        {
            Initialize(pSql_Table, pData, null);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pSql_Table"></param>
        /// <param name="pData"></param>
        /// <param name="pDisplay"></param>
        public void Initialize(SQL_Table pSql_Table, string pData, string pDisplay)
        {
            newValue = pData;
            sql_Table = pSql_Table;
            display = pDisplay;
            //
            //Warning: On teste seulement "null" et pas "empty"
            if (null == lastValue)
            {
                lastValue = newValue;
                lastSql_Table = sql_Table;
            }
        }

        /// <summary>
        /// <para>
        /// Synchronise les membres last avec les membres new (lastValue = newValue , lastSql_Table = sql_Table)
        /// </para>
        /// <para>
        /// Alimente optionnellement le membre errorMsg si le cci est non reseigné alors q'il est obligatoire
        /// </para>
        /// 
        /// </summary>
        /// <param name="pbSetMsgErrorOnMandatory">si true aliement le membre errorMsg</param>
        public void Finalize(bool pbSetMsgErrorOnMandatory)
        {
            //Reset LastValue 
            lastValue = newValue;
            lastSql_Table = sql_Table;
            //
            if (pbSetMsgErrorOnMandatory)
            {
                if (isMandatory)
                {
                    string errMsgIsMandatory = Ressource.GetString("ISMANDATORY");
                    if (StrFunc.IsEmpty(newValue))
                    {
                        if (errorMsg.IndexOf(errMsgIsMandatory) < 0)
                            errorMsg += errMsgIsMandatory;
                    }
                    else
                    {
                        errorMsg = errorMsg.Replace(errMsgIsMandatory, string.Empty);
                    }
                }
            }

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pbNew"></param>
        /// <returns></returns>
        private decimal DecValue(bool pbNew)
        {
            string Value = (pbNew ? newValue : lastValue);
            //
            if (IsTypeDecimal)
            {
                if (StrFunc.IsEmpty(Value))
                    return decimal.Zero;
                else
                    try { return decimal.Parse(Value, CultureInfo.InvariantCulture); }
                    catch { return decimal.Zero; }
            }
            else
                return Decimal.Zero;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pbNew"></param>
        /// <returns></returns>
        private DateTime DateValue(bool pbNew)
        {
            string Value = (pbNew ? newValue : lastValue);
            //
            if (IsTypeDate)
            {
                if (StrFunc.IsEmpty(Value))
                    return Convert.ToDateTime(null);
                else
                    try { return new DtFunc().StringToDateTime(Value); }
                    catch { return Convert.ToDateTime(null); }
            }
            else
                return Convert.ToDateTime(null);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pbNew"></param>
        /// <returns></returns>
        private TimeSpan TimeValue(bool pbNew)
        {
            string Value = (pbNew ? newValue : lastValue);
            DateTime result = Convert.ToDateTime(null);
            //
            if (IsTypeTime)
            {
                if (StrFunc.IsFilled(Value))
                    try { result = new DtFunc().StringToDateTime(Value); }
                    catch { }
            }
            TimeSpan time = new TimeSpan(result.Hour, result.Minute, result.Second);
            return time;
        }
        #endregion

        #region Membres de ICloneable
        public object Clone()
        {
            return this.Clone(CloneMode.CciAll);
        }
        public object Clone(CloneMode plMode)
        {
            CustomCaptureInfo clone = new CustomCaptureInfo();

            if ((CloneMode.CciAttribut == plMode) || (CloneMode.CciAll == plMode))
            {
                clone.clientId = clientId;
                clone.accessKey = accessKey;
                clone.isAutoPostback = isAutoPostback;
                clone.isMandatory = isMandatory;
                clone.isMissingMode = isMissingMode;
                clone.dataType = dataType;
                clone.regex = regex;
                clone.listRetrieval = listRetrieval;
                clone.typeRelaTiveTo = typeRelaTiveTo;
                clone.quickClientId = quickClientId;
                clone.quickDataPosition = quickDataPosition;
                clone.quickSeparator = quickSeparator;
                clone.quickFormat = quickFormat;
            }

            if ((CloneMode.CciData == plMode) || (CloneMode.CciAll == plMode))
            {
                clone.lastValue = lastValue;
                clone.newValue = newValue;
                clone.isInputByUser = isInputByUser;
                clone.isLastInputByUser = isLastInputByUser;
                clone.isCalculateBySystem = isCalculateBySystem;
                clone.errorMsg = errorMsg;
                clone.display = display;
            }
            return clone;
        }
        #endregion
    }
    
    /// <summary>
    /// FullCustomCaptureInfo: Contient les informations d'un objet Efsml à Importer.
    /// </summary>
    public class FullCustomCaptureInfo
    {
        #region constantes
        #endregion
        //
        #region public Enum
        #endregion
        //
        #region  Members
        private string _clientId;
        private string _errorMsg;
        private CDATA _newValue;
        #endregion Members
        //
        #region property
        [System.Xml.Serialization.XmlAttributeAttribute("clientId", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string clientId
        {
            get
            {
                return _clientId;
            }
            set
            {
                _clientId = value;
            }
        }
        #region newValue
        [System.Xml.Serialization.XmlElementAttribute("value", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public CDATA newValue
        {
            get
            {
                return _newValue;
            }
            set
            {
                _newValue = value;
            }
        }
        #endregion
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public string errorMsg
        {
            get
            {
                return _errorMsg;
            }
            set
            {
                _errorMsg = value;
            }
        }
        //		
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public string ClientId_WithoutPrefix
        {
            get
            {
                int lenPrefix = ClientId_Prefix.Length;
                return clientId.Substring(lenPrefix);
            }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public string ClientId_Prefix
        {
            get
            {
                if (3 <= clientId.Length)
                    return clientId.Substring(0, 3);
                else
                    return string.Empty;
            }
        }
        //
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool HasError
        {
            get { return StrFunc.IsFilled(errorMsg); }
        }
        #endregion property
        //
        #region constructors
        public FullCustomCaptureInfo() { }
        public FullCustomCaptureInfo(string pClientId)
        {
            clientId = pClientId;
        }
        #endregion constructors
        //
        #region Public Methodes
        #endregion Public Methodes
        //
        #region Private Methodes
        #endregion Private Methodes
    }
    
    /// <summary>
    /// 
    /// </summary>
    public class CciCompare : IComparable
    {
        #region Members
        /// <summary>
        /// Nom de baptême du cci  
        /// </summary>
        public string key;
        /// <summary>
        /// alimenté avec cci.NewValue  
        /// </summary>
        private string sValue;
        /// <summary>
        /// alimenté avec cci.isInputByUser   
        /// </summary>
        private bool isInputByUser;
        /// <summary>
        /// niveau de priorité, le cciCompare de niveau le plus faible est prioritaire
        /// </summary>
        private int order;
        /// <summary>
        /// alimenté avec cci.dataType   
        /// </summary>
        private TypeData.TypeDataEnum dataType;
        #endregion Members
        #region accessor
        public bool IsFilledValue
        {
            get
            {
                bool ret = false;
                try
                {
                    //
                    if (TypeData.IsTypeDec(dataType))
                        ret = StrFunc.IsDecimalInvariantFilled(sValue);
                    else if (TypeData.IsTypeInt(dataType))
                        ret = StrFunc.IsIntegerInvariantFilled(sValue);
                    else if (TypeData.IsTypeDate(sValue))
                        ret = DtFunc.IsDateTimeFilled(new DtFunc().StringToDateTime(sValue, DtFunc.FmtISODate));
                    else
                        ret = StrFunc.IsFilled(sValue);
                }
                catch { ret = false; }
                return ret;
            }
        }
        public bool IsEmptyValue
        {
            get
            {
                return (false == IsFilledValue);
            }
        }
        #endregion
        #region Constructor
        public CciCompare(string pkey, CustomCaptureInfo pCci, int pOrder)
            : this(pkey, pCci.dataType, pCci.newValue, pCci.isInputByUser, pOrder)
        {
        }
        public CciCompare(string pkey, TypeData.TypeDataEnum pdataType, string pValue, bool pIsInputByUser, int pOrder)
        {
            key = pkey;
            dataType = pdataType;
            sValue = pValue;
            isInputByUser = pIsInputByUser;
            order = pOrder;
        }
        #endregion cosntructor

        #region Members de IComparable
        /// <summary>
        /// 
        /// </summary>
        /// <remarks>
        /// 20090609 FI
        /// Est prioritaire 
        /// la donnée non renseignée (pour une donnée numerique, on considère que 0 est une donnée non renseignée)  
        /// si les 2 données sont non renseignées 
        ///     alors est prioritaire celle spécifié comme prioritaire (order de poids le plus faible)
        /// sinon est prioritaire la donnée non saisie par l'utilisateur. 
        ///  Si aucune donnée n'est saisie alors  est prioritaire celle spécifiée comme prioritaire (order de poids le plus faible)
        /// 
        /// </remarks>
        /// <param name="pobj"></param>
        /// <returns></returns>
        public int CompareTo(object pObj)
        {
            CciCompare cci2 = pObj as CciCompare;
            if (null != cci2)
            //if (pobj is CciCompare)
            {
                int ret = 0; //Like Equal
                //CciCompare cci2 = (CciCompare)pobj;
                //20090609 FI On ne considère plus que IsFilledValue
                //if (StrFunc.IsEmpty(sValue) && StrFunc.IsFilled(cci2.sValue))
                //    ret = -1; // cette instance est inférieure à pObj
                //if (StrFunc.IsFilled(sValue) && StrFunc.IsEmpty(cci2.sValue))
                //    ret = 1; // cette instance est supérieur à pObj
                //
                if ((ret == 0) && (dataType == cci2.dataType))
                {
                    if ((dataType == cci2.dataType))
                    {
                        if ((false == IsFilledValue) && cci2.IsFilledValue)
                            ret = -1; // cette instance est inférieure à pObj
                        if (IsFilledValue && (false == cci2.IsFilledValue))
                            ret = 1; // cette instance est supérieur à pObj
                    }
                    else
                    {
                        //20090609 FI On ne devrait jamais passé ici
                        //Comparer des données qui ne sont pas de même type, cela parait incohérent
                        //
                        if (StrFunc.IsEmpty(sValue) && StrFunc.IsFilled(cci2.sValue))
                            ret = -1; // cette instance est inférieure à pObj
                        if (StrFunc.IsFilled(sValue) && StrFunc.IsEmpty(cci2.sValue))
                            ret = 1; // cette instance est supérieur à pObj
                    }
                }
                //
                if ((IsEmptyValue) && (cci2.IsEmptyValue))
                {
                    if (ret == 0)
                        ret = order.CompareTo(cci2.order);   // cette instance est inférieure si son order et inférieur
                }
                //
                if (ret == 0)
                    ret = isInputByUser.CompareTo(cci2.isInputByUser);   // cette instance est inférieure si IsInputByUser =false 
                //
                if (ret == 0)
                    ret = order.CompareTo(cci2.order);   // cette instance est inférieure si son order et inférieur
                return ret;
            }
            throw new ArgumentException("object is not a CciCompare");
        }
        #endregion Members de IComparable
    }
    
    /// <summary>
    /// Description résumée de RateTools.
    /// </summary>
    public sealed class RateTools
    {
        public const string
            FLOATING_RATE_TENOR_SEPERATOR = "/";

        public enum RateTypeEnum
        {
            RATE_FIXED,
            RATE_FLOATING,
        }
        #region constructor
        public RateTools() { }
        #endregion constructor
        #region GetTypeRate()
        //static RateTypeEnum GetTypeRate(string pRate)
        //{
        //    if (IsFixedRate(pRate))
        //        return RateTypeEnum.RATE_FIXED;
        //    else
        //        return RateTypeEnum.RATE_FLOATING;
        //}
        #endregion
        #region GetFloatingRateWithTenor() methods
        public static string GetFloatingRateWithTenor(string pFloatingRate, string pPeriodMultiplier, string pPeriod)
        {
            string ret = pFloatingRate;
            if (StrFunc.IsFilled(pPeriodMultiplier) && StrFunc.IsFilled(pPeriod))
            {
                ret += FLOATING_RATE_TENOR_SEPERATOR + pPeriodMultiplier + pPeriod;
            }
            return ret;
        }
        public static string GetFloatingRateWithTenor(string pFloatingRate, int pPeriodMultiplier, PeriodEnum pPeriod)
        {
            return GetFloatingRateWithTenor(pFloatingRate, pPeriodMultiplier.ToString(), pPeriod.ToString());
        }
        #endregion
        #region GetFloatingRateWithoutTenor()
        /// <summary>
        /// Return a floating string with tenor if exist (ie: EUR-EURIBOR-Telerate 3m, EUR-EONIA)
        /// </summary>
        /// <param name="pFloatingRateWithTenor"></param>
        /// <returns></returns>
        public static string GetFloatingRateWithoutTenor(string pFloatingRateWithTenor)
        {
            string ret = pFloatingRateWithTenor;
            int pos = pFloatingRateWithTenor.IndexOf(FLOATING_RATE_TENOR_SEPERATOR);
            if (pos > 0)
                ret = pFloatingRateWithTenor.Substring(0, pos);
            return ret;
        }
        #endregion

        #region IsFloatingRateWithTenor() methods
        /// <summary>
        /// Return true if the rate is a floating rate with tenor (ie: EUR-EURIBOR-Telerate 3M)
        /// </summary>
        /// <param name="pRate"></param>
        /// <returns></returns>
        public static bool IsFloatingRateWithTenor(string pFloatingRate)
        {
            return pFloatingRate.IndexOf(FLOATING_RATE_TENOR_SEPERATOR) > 0;
        }
        /// <summary>
        /// Return true if the rate is a floating rate with tenor (ie: EUR-EURIBOR-Telerate 3M)
        /// </summary>
        /// <param name="pFloatingRate"></param>
        /// <param name="opTenor">Return the string tenor (ie: 3M)</param>
        /// <returns></returns>
        public static bool IsFloatingRateWithTenor(string pFloatingRate, out string opTenor)
        {
            bool ret = IsFloatingRateWithTenor(pFloatingRate);
            if (ret)
                opTenor = pFloatingRate.Substring(pFloatingRate.IndexOf(FLOATING_RATE_TENOR_SEPERATOR) + 1).Trim();
            else
                opTenor = string.Empty;
            return ret;
        }
        public static bool IsFloatingRateWithTenor(string pFloatingRate, out int opTenor_periodMultiplier, out PeriodEnum opTenor_period)
        {
            string tenor = string.Empty;
            bool ret = IsFloatingRateWithTenor(pFloatingRate, out tenor);
            if (ret)
            {
                System.Text.RegularExpressions.Regex re = new System.Text.RegularExpressions.Regex("\\d+");
                opTenor_periodMultiplier = Int32.Parse(re.Match(tenor).Value);
                string period = tenor.Substring(opTenor_periodMultiplier.ToString().Length).Trim();
                opTenor_period = StringToEnum.Period(period);
            }
            else
            {
                opTenor_periodMultiplier = 0;
                opTenor_period = PeriodEnum.D;
            }
            return ret;
        }
        #endregion IsFloatingRateWithTenor()
        #region IsFloatingRate()
        /// <summary>
        /// Return true if the rate is a floating rate
        /// </summary>
        /// <param name="pRate"></param>
        /// <returns></returns>
        public static bool IsFloatingRate(string pRate)
        {
            return (StrFunc.IsFilled(pRate)) && (!IsFixedRate(pRate));
        }
        #endregion
        #region IsFixedRate()
        public static bool IsFixedRate(string pRate)
        {
            bool isFixedRate = EFSRegex.IsMatch(pRate, EFSRegex.TypeRegex.RegexFixedRateExtend);
            return isFixedRate;
        }
        #endregion
    }

    /// <summary>
    /// 
    /// </summary>
    public class CciToolsBase
    {
        #region public static Getccis(IContainerCci pContainerCci)
        public static CustomCaptureInfosBase Getccis(IContainerCci pContainerCci)
        {

            CustomCaptureInfosBase ret = null;
            Object ccis = null;
            Type tContainerCci = pContainerCci.GetType();
            MemberInfo[] mbrsCcis = tContainerCci.GetMember("Ccis");
            if (ArrFunc.IsEmpty(mbrsCcis))
                mbrsCcis = tContainerCci.GetMember("ccis");
            if (ArrFunc.IsEmpty(mbrsCcis) && (null != tContainerCci.BaseType))
            {
                tContainerCci = tContainerCci.BaseType;
                mbrsCcis = tContainerCci.GetMember("Ccis");
                if (ArrFunc.IsEmpty(mbrsCcis))
                    mbrsCcis = tContainerCci.GetMember("ccis");
            }
            if (ArrFunc.IsFilled(mbrsCcis))
            {
                if (mbrsCcis[0].MemberType == MemberTypes.Property)
                    ccis = tContainerCci.InvokeMember(mbrsCcis[0].Name, BindingFlags.GetProperty, null, pContainerCci, null);
                else if (mbrsCcis[0].MemberType == MemberTypes.Field)
                    ccis = tContainerCci.InvokeMember(mbrsCcis[0].Name, BindingFlags.GetField, null, pContainerCci, null);
            }
            //
            if (null != ccis)
                ret = (CustomCaptureInfosBase)ccis;

            return ret;

        }
        #endregion

        #region public static SetCciContainer
        public static void SetCciContainer(IContainerCci pContainerCci, string pField, object pValue)
        {
            SetCciContainer(pContainerCci, "CciEnum", pField, pValue);
        }
        public static void SetCciContainer(IContainerCci pContainerCci, string pCciEnumsMethodName, string pField, object pValue)
        {
            SetCciContainer(pContainerCci, null, pCciEnumsMethodName, pField, pValue);
        }
        public static void SetCciContainer(IContainerCci pContainerCci, Type pClassOwnerType, string pCciEnumsMethodName, string pField, object pValue)
        {
            //
            CustomCaptureInfosBase ccis = CciToolsBase.Getccis(pContainerCci);
            //
            if (null == ccis)
                throw new NullReferenceException(StrFunc.AppendFormat("ccis is not defined on object {0}", pContainerCci.GetType().ToString()));
            //
            if (null != ccis)
            {
                Type tContainerCci = pContainerCci.GetType();
                if (null != pClassOwnerType)
                    tContainerCci = pClassOwnerType;
                //
                // Get CciEnum
                MemberInfo[] mbrsCciEnums = tContainerCci.GetMember(pCciEnumsMethodName);
                if (ArrFunc.IsFilled(mbrsCciEnums))
                {
                    FieldInfo[] fldCciEnums = ((Type)mbrsCciEnums[0]).GetFields();
                    for (int i = 1; i < fldCciEnums.Length; i++)
                        ccis.Set(pContainerCci.CciClientId(fldCciEnums[i].Name), pField, pValue);
                }
            }

        }
        #endregion
        #region public static CreateInstance
        public static void CreateInstance(IContainerCci pContainerCci, Object pObjectOwner)
        {
            CreateInstance(pContainerCci, pObjectOwner, "CciEnum");
        }
        public static void CreateInstance(IContainerCci pContainerCci, Object pObjectOwner, string pCciEnumsMethodName)
        {
            CreateInstance(pContainerCci, null, pObjectOwner, pCciEnumsMethodName);
        }
        public static void CreateInstance(IContainerCci pContainerCci, Type pClassOwnerType, Object pObjectOwner, string pCciEnumsMethodName)
        {
            CustomCaptureInfosBase ccis = CciToolsBase.Getccis(pContainerCci);
            //
            if (null == ccis)
                throw new NullReferenceException(StrFunc.AppendFormat("ccis is not defined on object {0}", pContainerCci.GetType().ToString()));
            //
            if (null != ccis)
            {
                Type tClassOwner = pContainerCci.GetType();
                if (null != pClassOwnerType)
                    tClassOwner = pClassOwnerType;
                // Get CciEnum
                MemberInfo[] mbrsCciEnums = tClassOwner.GetMember(pCciEnumsMethodName);
                if ((null != mbrsCciEnums) && (0 < mbrsCciEnums.Length))
                {
                    FieldInfo[] fldCciEnums = ((Type)mbrsCciEnums[0]).GetFields();
                    foreach (FieldInfo fldEnum in fldCciEnums)
                    {
                        object[] attributes = fldEnum.GetCustomAttributes(typeof(XmlEnumAttribute), true);
                        if (0 < attributes.Length)
                        {
                            XmlEnumAttribute enumAttribute = (XmlEnumAttribute)attributes[0];

                            // Get CciClientId
                            Type[] typeArray = new Type[1];
                            typeArray.SetValue(typeof(string), 0);
                            MethodInfo method = tClassOwner.GetMethod("CciClientId", typeArray);
                            string cciClientIdValue = string.Empty;
                            if (null != method)
                            {
                                object[] argValues = new object[] { fldEnum.Name };
                                String[] argNames = new String[] { method.GetParameters()[0].Name };
                                cciClientIdValue = (string)tClassOwner.InvokeMember(method.Name, BindingFlags.InvokeMethod,
                                    null, pContainerCci, argValues, null, null, argNames);
                            }

                            // Test if ccis contains CciClientId
                            if (((CustomCaptureInfosBase)ccis).Contains(cciClientIdValue))
                            {
                                String[] cciElts = enumAttribute.Name.Split(".".ToCharArray());
                                object obj = pObjectOwner;
                                foreach (string cciElt in cciElts)
                                {
                                    if (null != obj)
                                    {
                                        Type tObj = obj.GetType();
                                        FieldInfo fld = tObj.GetField(cciElt);
                                        if (null != fld)
                                        {
                                            object obj2 = fld.GetValue(obj);
                                            if ((null == obj2) && (false == fld.FieldType.IsArray))
                                            {
                                                obj2 = fld.FieldType.InvokeMember(null, BindingFlags.CreateInstance, null, null, null);
                                                fld.SetValue(obj, obj2);
                                            }
                                            obj = obj2;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

        }
        #endregion CreateInstance
        //
        #region public NewCreateInstance
#warning TO BE DEFINE and REPLACE CreateInstance
        public static void CreateInstance2(Object pClassOwner, Object pObjectOwner)
        {
            CreateInstance2(pClassOwner, pObjectOwner, "CciEnum");
        }
        public static void CreateInstance2(Object pClassOwner, Object pObjectOwner, string pCciEnumsMethodName)
        {
            try
            {
                #region Get ccis
                Type tClassOwner = pClassOwner.GetType();
                object ccis = null;
                MemberInfo[] mbrsCcis = tClassOwner.GetMember("ccis");
                if ((null != mbrsCcis) && (0 < mbrsCcis.Length))
                {
                    if (mbrsCcis[0].MemberType == MemberTypes.Property)
                        ccis = tClassOwner.InvokeMember(mbrsCcis[0].Name, BindingFlags.GetProperty, null, pClassOwner, null);
                    else if (mbrsCcis[0].MemberType == MemberTypes.Field)
                        ccis = tClassOwner.InvokeMember(mbrsCcis[0].Name, BindingFlags.GetField, null, pClassOwner, null);
                }
                #endregion Get ccis
                //
                if (null == ccis)
                    throw new NullReferenceException(StrFunc.AppendFormat("ccis is not defined on object {0}", tClassOwner.ToString()));
                //                
                if (null != ccis)
                {
                    // Get CciEnum
                    MemberInfo[] mbrsCciEnums = tClassOwner.GetMember(pCciEnumsMethodName);
                    if ((null != mbrsCciEnums) && (0 < mbrsCciEnums.Length))
                    {
                        FieldInfo[] fldCciEnums = ((Type)mbrsCciEnums[0]).GetFields();
                        foreach (FieldInfo fldEnum in fldCciEnums)
                        {
                            object[] attributes = fldEnum.GetCustomAttributes(typeof(CciEnumInstance), true);
                            if (0 < attributes.Length)
                            {
                                // Get CciClientId
                                Type[] typeArray = new Type[1];
                                typeArray.SetValue(typeof(string), 0);
                                MethodInfo method = tClassOwner.GetMethod("CciClientId", typeArray);
                                string cciClientIdValue = string.Empty;
                                if (null != method)
                                {
                                    object[] argValues = new object[] { fldEnum.Name };
                                    String[] argNames = new String[] { method.GetParameters()[0].Name };
                                    cciClientIdValue = (string)tClassOwner.InvokeMember(method.Name, BindingFlags.InvokeMethod,
                                        null, pClassOwner, argValues, null, null, argNames);
                                }
                                if (((CustomCaptureInfosBase)ccis).Contains(cciClientIdValue))
                                {
                                    foreach (object attribute in attributes)
                                    {
                                        CciEnumInstance cciEnumInstance = (CciEnumInstance)attribute;
                                        String[] cciElts = cciEnumInstance.Name.Split(".".ToCharArray());
                                        object obj = pObjectOwner;
                                        foreach (string cciElt in cciElts)
                                        {
                                            Type tObj = obj.GetType();
                                            FieldInfo fld = tObj.GetField(cciElt);
                                            if (null != fld)
                                            {
                                                object obj2 = fld.GetValue(obj);
                                                if (null == obj2)
                                                {
                                                    obj2 = fld.FieldType.InvokeMember(null, BindingFlags.CreateInstance, null, null, null);
                                                    fld.SetValue(obj, obj2);
                                                }
                                                obj = obj2;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex) { throw ex; }
        }
        #endregion NewCreateInstance

        #region public Dump_IsCciContainerArraySpecified
        public static bool Dump_IsCciContainerArraySpecified(bool pIsSpeficied, IContainerCciSpecified[] pObjArray)
        {
            //pIsSpeficied = false; => On n'afffecte pas le specified à false  Ex si Ecran n'affiche pas la totalité des données du trade 
            //EX Trade avec 4 frais => si Ecran n'affiche que 3 frais et qu'il ne sont pas renseignés => il reste cependant 1 frais => Specified doit rester à true
            bool isSpecified = pIsSpeficied;
            if (null != pObjArray)
            {
                foreach (IContainerCciSpecified pObj in pObjArray)
                {
                    if (pObj.IsSpecified)
                    {
                        isSpecified = true;
                        break;
                    }
                }
            }
            return isSpecified;
        }
        #endregion
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="coDDL"></param>
        /// <param name="pddl"></param>
        /// <param name="pCur1"></param>
        /// <param name="pCur2"></param>
        public static void DDL_LoadQuoteBasis(CustomObjectDropDown coDDL, DropDownList pddl, string pCur1, string pCur2)
        {
            string valueCurrency1PerCurrency2 = string.Empty;
            string valueCurrency2PerCurrency1 = string.Empty;

            switch (coDDL.ListRetrieval.ToLower())
            {
                case "predef:quotebasis":
                    valueCurrency1PerCurrency2 = QuoteBasisEnum.Currency1PerCurrency2.ToString();
                    valueCurrency2PerCurrency1 = QuoteBasisEnum.Currency2PerCurrency1.ToString();
                    break;
                case "predef:strikequotebasis":
                    valueCurrency1PerCurrency2 = StrikeQuoteBasisEnum.CallCurrencyPerPutCurrency.ToString();
                    valueCurrency2PerCurrency1 = StrikeQuoteBasisEnum.PutCurrencyPerCallCurrency.ToString();
                    break;
            }

            bool isUseIsdaRepresentation = BoolFunc.IsTrue(pddl.Attributes["useIsDaRepresentation"]);
            pddl.Items.Clear();
            if (isUseIsdaRepresentation)
            {
                // Ex USD/EUR = 1,2  1,2 USD pour 1 Eur 
                // L'affichage DEV1/DEV2 signifie The amount of currency1 for 1 unit of currency2
                pddl.Items.Add(new ListItem(pCur1 + " / ." + pCur2, valueCurrency1PerCurrency2));
                pddl.Items.Add(new ListItem(pCur2 + " / ." + pCur1, valueCurrency2PerCurrency1));
            }
            else
            {
                // Ex EUR/USD = 1,2  1 Eur donne 1,2 USD
                // L'affichage DEV1/DEV2 signifie The amount of currency2 for 1 unit of currency1
                pddl.Items.Add(new ListItem(pCur2 + ". / " + pCur1, valueCurrency1PerCurrency2));
                pddl.Items.Add(new ListItem(pCur1 + ". / " + pCur2, valueCurrency2PerCurrency1));
            }

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="coDDL"></param>
        /// <param name="pddl"></param>
        /// <param name="pCur1"></param>
        /// <param name="pCur2"></param>
        public static void DDL_LoadSideRateBasis(CustomObjectDropDown coDDL, DropDownList pddl, string pCur1, string pCur2)
        {
            string valueBaseCurrencyPerCurrency1 = string.Empty;
            string valueCurrency1PerBaseCurrency = string.Empty;
            string valueBaseCurrencyPerCurrency2 = string.Empty;
            string valueCurrency2PerBaseCurrency = string.Empty;

            switch (coDDL.ListRetrieval.ToLower())
            {
                case "predef:sideratebasis":
                    valueCurrency1PerBaseCurrency = SideRateBasisEnum.Currency1PerBaseCurrency.ToString();
                    valueBaseCurrencyPerCurrency1 = SideRateBasisEnum.BaseCurrencyPerCurrency1.ToString();
                    valueCurrency2PerBaseCurrency = SideRateBasisEnum.Currency2PerBaseCurrency.ToString();
                    valueBaseCurrencyPerCurrency2 = SideRateBasisEnum.BaseCurrencyPerCurrency2.ToString();
                    break;
            }
            //
            if (null != pddl)
            {
                bool isUseIsdaRepresentation = BoolFunc.IsTrue(pddl.Attributes["useIsDaRepresentation"]);
                pddl.Items.Clear();
                if (isUseIsdaRepresentation)
                {
                    // Ex USD/EUR = 1,2  1,2 USD pour 1 Eur 
                    // L'affichage DEV1/DEV2 signifie The amount of currency1 for 1 unit of currency2
                    pddl.Items.Add(new ListItem(pCur1 + " / ." + pCur2, valueCurrency1PerBaseCurrency));
                    pddl.Items.Add(new ListItem(pCur2 + " / ." + pCur1, valueBaseCurrencyPerCurrency1));
                    pddl.Items.Add(new ListItem(pCur1 + " / ." + pCur2, valueBaseCurrencyPerCurrency2));
                    pddl.Items.Add(new ListItem(pCur2 + " / ." + pCur1, valueCurrency2PerBaseCurrency));
                }
                else
                {
                    // Ex EUR/USD = 1,2  1 Eur donne 1,2 USD
                    // L'affichage DEV1/DEV2 signifie The amount of currency2 for 1 unit of currency1
                    pddl.Items.Add(new ListItem(pCur2 + ". / " + pCur1, valueCurrency1PerBaseCurrency));
                    pddl.Items.Add(new ListItem(pCur1 + ". / " + pCur2, valueBaseCurrencyPerCurrency1));
                    pddl.Items.Add(new ListItem(pCur2 + ". / " + pCur1, valueBaseCurrencyPerCurrency2));
                    pddl.Items.Add(new ListItem(pCur1 + ". / " + pCur2, valueCurrency2PerBaseCurrency));
                }
            }
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCci"></param>
        /// <param name="pddl"></param>
        public static void DDL_LoadInvoiceRateSourceFixingDateRelativeTo(CustomObjectDropDown coDDL, DropDownList pddl)
        {
            if (null != pddl)
            {
                pddl.Items.Clear();
                switch (coDDL.ListRetrieval.ToLower())
                {
                    case "predef:invoicefixingdaterelativeto":
                        pddl.Items.Add(new ListItem("StartPeriod", "StartPeriod"));
                        pddl.Items.Add(new ListItem("InvoiceDate", "InvoiceDate"));
                        pddl.Items.Add(new ListItem("InvoicePaymentDate", "InvoiceDate"));
                        break;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pMsg"></param>
        /// <param name="pNewValue"></param>
        /// <returns></returns>
        public static string BuildCciErrMsg(string pMsg, string pNewValue)
        {
            return StrFunc.AppendFormat("{0} [{1}]", pMsg, pNewValue);
        }


        #region Cci Reflection Tools
        /// <summary>
        /// Get the list of the public fields of the given type
        /// </summary>
        /// <param name="pType">type we want to inspect</param>
        /// <exception cref="ArgumentNullException">when type is null</exception>
        /// <returns>the fields list</returns>
        public static string[] GetCciKeys(Type pType)
        {
            if (pType == null)
                throw new ArgumentNullException("pType");

            return GetCciKeys(BindingFlags.Public | BindingFlags.Instance, pType);
        }

        /// <summary>
        /// Get the list of the fields of the given type
        /// </summary>
        /// <param name="flags">field filter</param>
        /// <param name="pType">type we want to inspect</param>
        /// <exception cref="ArgumentNullException">when type is null</exception>
        /// <returns>the fields list</returns>
        public static string[] GetCciKeys(BindingFlags flags, Type pType)
        {
            if (pType == null)
                throw new ArgumentNullException("pType");

            string[] fieldNames = null;

            FieldInfo[] fInfos = pType.GetFields(flags);

            if (!ArrFunc.IsEmpty(fInfos))
            {
                fieldNames = new string[fInfos.Length];

                for (int idxField = 0; idxField < fInfos.Length; idxField++)
                    fieldNames[idxField] = fInfos[idxField].Name;
            }

            return fieldNames;
        }

        /// <summary>
        /// Get the value of the field 
        /// </summary>
        /// <param name="pFieldName">the field name we want get the value from</param>
        /// <param name="pBusinessStruct">the object reference we want to extract the value from</param>
        /// <param name="pType">type we want to inspect</param>
        /// <remarks>the field we want to obtain the value from, it must be type of {EFS_Date, EFS_DateTime, EFS_Decimal, EFS_Integer, EFS_Boolean} </remarks>
        /// <returns>the value of the field converted to string if pFieldName exists, 
        /// otherwise the empty string</returns>
        public static string GetStringValue(string pFieldName, object pBusinessStruct, Type pType)
        {
            if (pType == null)
                throw new ArgumentNullException("pType");

            string stringValue = null;

            FieldInfo fInfo = pType.GetField(pFieldName);

            if (fInfo != null)
            {
                object value = fInfo.GetValue(pBusinessStruct);

                if (value is EFS_Date)
                    stringValue = ((EFS_Date)value).Value;
                else if (value is EFS_DateTime)
                    stringValue = ((EFS_DateTime)value).Value;
                else if (value is EFS_Decimal)
                    stringValue = ((EFS_Decimal)value).Value;
                else if (value is EFS_Integer)
                    stringValue = ((EFS_Integer)value).Value;
                else if (value is EFS_Boolean)
                    stringValue = ((EFS_Boolean)value).Value;
                else if (value is EFS_String)
                    stringValue = ((EFS_String)value).Value;
                else
                    // default string case
                    // UNDONE can raise exceptions?
                    stringValue = Convert.ToString(value);
            }

            return stringValue;
        }

        /// <summary>
        /// Set the value of a field
        /// </summary>
        /// <param name="pFieldName">the field name we want put the value in</param>
        /// <param name="pBusinessStruct">the object reference we want to extract the value from</param>
        /// <param name="pType">type we want to inspect</param>
        /// <param name="pValue">value to set
        /// </param>
        /// <remarks>the field to set must be type of {EFS_Date, EFS_DateTime, EFS_Decimal, EFS_Integer, EFS_Boolean} </remarks>
        /// <returns></returns>
        public static bool SetStringValue(string pFieldName, object pBusinessStruct, Type pType, string pValue)
        {
            if (pType == null)
                throw new ArgumentNullException("pType");

            bool set = false;

            FieldInfo fInfo = pType.GetField(pFieldName);

            if (fInfo != null)
            {
                object convertedValue = null;

                if (fInfo.FieldType == typeof(EFS_Date))
                    convertedValue = new EFS_Date(pValue);
                else if (fInfo.FieldType == typeof(EFS_DateTime))
                    convertedValue = new EFS_DateTime(pValue);
                else if (fInfo.FieldType == typeof(EFS_Decimal))
                    convertedValue = new EFS_Decimal(pValue);
                else if (fInfo.FieldType == typeof(EFS_Integer))
                    convertedValue = new EFS_Integer(pValue);
                else if (fInfo.FieldType == typeof(EFS_Boolean))
                    convertedValue = new EFS_Boolean(bool.Parse(pValue));
                else if (fInfo.FieldType == typeof(EFS_String))
                    convertedValue = new EFS_String(pValue);
                else
                    // default string case
                    convertedValue = pValue;

                set = true;

                try
                {
                    fInfo.SetValue(pBusinessStruct, convertedValue);
                }
                // UNDONE treat exception?
                catch (ArgumentException) { }

            }

            return set;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T">type of the variable, pass always an anonymous type</typeparam>
        /// <param name="variable">the variable we want to know the name</param>
        /// <param name="type">the type of the class where the field is defined, null for methode scope variable</param>
        /// <returns></returns>
        public static string GetFieldVariableName<T>(T variable, Type type) where T : class
        {
            string ret = String.Empty;

            var properties = typeof(T).GetProperties();

            if (properties.Length == 1)
            {
                // get the name of the variable passed as input parameter
                string variableName = properties[0].Name;

                if (type != null)
                {

                    // verify that the name is relative to one field of the class
                    FieldInfo fInfo = type.GetField(variableName);

                    if (fInfo != null)
                        ret = fInfo.Name;
                }
                else
                    ret = variableName;

            }

            return ret;
        }

        #endregion Cci Reflection Tools

    }

    /// <summary>
    /// 
    /// </summary>
    public class CaptureToolsBase
    {
        #region public IsDocumentElementValid
        public static bool IsDocumentElementValid(EFS_Decimal pData)
        {
            bool isOk = false;
            if (null != pData)
                isOk = StrFunc.IsFilled(pData.Value);
            return isOk;
        }
        public static bool IsDocumentElementValid(EFS_Date pData)
        {
            bool isOk = false;
            if (null != pData)
                isOk = !DtFunc.IsDateTimeEmpty(pData.DateValue);
            return isOk;
        }
        public static bool IsDocumentElementValid(string pData)
        {
            bool isOk = false;
            if (null != pData)
                isOk = StrFunc.IsFilled(pData);
            return isOk;
        }
        #endregion IsFpmlDocElementValid
    }
   
}
