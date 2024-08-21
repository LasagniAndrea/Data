#region Using Directives
using System;
using System.Collections.Generic;  
using System.Collections;
using System.Globalization;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Reflection;
using System.Xml.Serialization;
using System.Drawing;
using System.Linq;

using EFS.ACommon;
using EFS.Actor;
using EFS.Common;
using EFS.Common.Web;

using EFS.GUI.Attributes;
using EFS.GUI.Interface;

using Tz = EFS.TimeZone;

using EfsML.Enum.Tools;
using EfsML.Enum;

using FpML.Enum;
#endregion Using Directives


namespace EFS.GUI.CCI
{
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
        // EG 20160404 Migration vs2013
        //private bool m_IsSessionAdmin;
        private bool m_IsModeIO;
        //
        private Cst.Capture.ModeEnum m_CaptureMode;
        //use by LoadCapture (true during LoadCapture)
        /// <summary>
        /// 
        /// </summary>
        private bool m_IsLoading;
        //Use for ProcessInitialize
        private readonly Queue m_MyHigh;
        private readonly Queue m_MyLow;
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
        /// Représente l'identifiant de la session 
        /// </summary>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public string SessionId
        {
            set { m_SessionId = value; }
            get { return m_SessionId; }
        }

        /// <summary>
        /// Représente l'utilisateur de Spheres® (afin d'appliquer les rectictions aux données (SESSIONRESTRICT))
        /// <para>- Côté web, représente l'acteur connecté</para>
        /// <para>- Côté service, C'est l'acteur SYSTEM</para>
        /// </summary>
        /// FI 20141107 [20441] Add 
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public User User
        {
            set;
            get;
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
        public bool IsQueueEmpty
        {
            get
            {
                return ArrFunc.IsEmpty(m_MyHigh) && ArrFunc.IsEmpty(m_MyLow);
            }
        }

        /// <summary>
        ///  Obtient la liste des cci qui ont changé 
        /// </summary>
        /// FI 20140414 [19793] Tuning
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public List<String> ClientId_DumpToDocument
        {
            get;
            private set;
        }

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public string CS
        {
            set;
            get;
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
        /// FI 20161214 [21916] Modify
        public CustomCaptureInfo this[string pClientId_WithoutPrefix]
        {
            //get
            //{
            //    CustomCaptureInfo ret = null;
            //    if (0 < this.Count)
            //        ret = List.Cast<CustomCaptureInfo>().Where(x => x.ClientId_WithoutPrefix == pClientId_WithoutPrefix).FirstOrDefault();
            //    return ret;
            //}
            get
            {
                return this[pClientId_WithoutPrefix, true];
            }
        }
        public CustomCaptureInfo this[string pClientId, bool pIsWithoutPrefix]
        {
            get
            {
                CustomCaptureInfo ret = null;
                if (0 < this.Count)
                    ret = List.Cast<CustomCaptureInfo>().Where(x =>
                        (pIsWithoutPrefix ? x.ClientId_WithoutPrefix : x.ClientId) == pClientId).FirstOrDefault();
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
        /// <param name="pUser">Représente l'utilisateur afin d'appliquer d'éventuelles restrictions sur les données (SESSIONRESTRICT)</param>
        /// <param name="pSessionId"></param>
        /// <param name="pIsGetDefaultOnInitializeCci"></param>
        /// FI 20141107 [20441] Modification de la signature (add pUser)
        // EG 20180425 Analyse du code Correction [CA2214]
        public CustomCaptureInfosBase(string pCs, ICustomCaptureInfos pObj, User pUser, string pSessionId, bool pIsGetDefaultOnInitializeCci)
        {
            CS = pCs;

            User = pUser;
            if (null == User)
                User = new User(1, null, RoleActor.SYSADMIN);

            m_SessionId = pSessionId;

            m_IsGetDefaultOnInitializeCci = pIsGetDefaultOnInitializeCci;

            m_MyHigh = new Queue();
            m_MyLow = new Queue();
            m_Obj = pObj;

            //InitializeCciContainer();
        }
        #endregion Constructors

        #region Methods

        /// <summary>
        /// Ajoute un cci à la collection 
        /// <para>Ajout non effectué s'il existe déjà</para>
        /// </summary>
        /// <param name="pCci"></param>
        /// FI 20161214 [21916] Modify
        public void Add(CustomCaptureInfo pCci)
        {

            //bool isAlreadyRexist = false;
            //foreach (CustomCaptureInfo cci in this)
            //{
            //    if (cci.ClientId == (pCci.ClientId))
            //    {
            //        isAlreadyRexist = true;
            //        break;
            //    }
            //}
            //if (false == isAlreadyRexist)
            //    List.Add(pCci);

            // FI 20161214 [21916] use Linq instruction
            if (List.Cast<CustomCaptureInfo>().Where(x => x.ClientId == pCci.ClientId).Count() == 0)
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
                if ((this[i].QuickClientId == pQuickClientId) && (this[i].QuickDataPosition == pQuickPosition))
                {
                    cci = this[i];
                    break;
                }
            }
            return cci;
        }

        /// <summary>
        /// Nettoie la propriété ErrorMsg du cci 
        /// </summary>
        /// <param name="pClientId_WithoutPrefix"></param>
        public void ClearErrorMsg(string pClientId_WithoutPrefix)
        {
            SetErrorMsg(pClientId_WithoutPrefix, string.Empty);
        }


        /// <summary>
        /// Nettoie tous les ccis de la collection [cci.NewValue = null;cci.Sql_Table = null; cci.LastValue = null;cci.LastSql_Table = null]
        /// </summary>
        ///FI 20091130 [16770] Modify => ClearAllValue
        ///FI 20140708 [20179] Modify => gestion de newValueMatch 
        public void ClearAllValue()
        {
            foreach (CustomCaptureInfo cci in this)
            {
                cci.NewValue = null;
                cci.Sql_Table = null;
                cci.LastValue = null;
                cci.LastSql_Table = null;
                cci.NewValueMatch = null;
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
        ///  Obtient true s'il exite déjà le cci {pClientId_WithoutPrefix} dans la collection
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
                    string msg = cci.ClientId + ": ";
                    msg += cci.LastValue + " / " + cci.NewValue;
                    //                    msg += (cci.IsAutoPostback      ? " - AutoPostback":string.Empty);
                    //                    msg += (cci.HasChanged          ? " - Changed":string.Empty);
                    msg += (cci.IsInputByUser ? " - InputByUser" : string.Empty);
                    //                    msg += (cci.IsCalculateBySystem ? " - CalculateBySystem":string.Empty);
                    msg += (cci.HasError ? " - " + cci.ErrorMsg : string.Empty);

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
                System.Diagnostics.Debug.WriteLine(cci.ClientId);
            }
        }
        #endregion DebugClientId


        /// <summary>
        /// <para>1/ Déversement des données issues des CCI, dans les classes du Document XML</para>
        /// <para>2/ Gestion des incidences sur les autres données via 2 queues (High, Low)</para>
        /// <para>3/ Appel récursif afin de gérer les éventuels nouveaux déversement suite à la gestion des incidences</para>
        /// <para>4/ Purge des classes du Document XML</para>
        /// </summary>
        /// EG 20171009 [23452] Apply InitializeClientId_DumpToDocument
        /// FI 20170928 [23452] Modify
        public void Dump_ToDocument(int pGuard)
        {
            if (0 == pGuard)
                SynchronizeCcisFromQuickInput();
            
            if (CciContainerSpecified)
            {
                // FI 20140414 [19793] Tuning A mettre en place éventuellement plus tard
                //if (BoolFunc.IsTrue  ((SystemSettings.GetAppSettings("InializeClientId_DumpToDocument","true"))  
                InitializeClientId_DumpToDocument();

                // FI 20200429 [XXXXX] Add test sur count
                if (this.ClientId_DumpToDocument.Count > 0)
                {
                    CciContainer.Dump_ToDocument();

                    // RD 20120810 [18069] 
                    // Pour le mode Modification partielle (sans régénération des événements):
                    // - Ne pas modifier d'autres données 
                    // - Ne pas recalculer d'autres montants 
                    if (false == Cst.Capture.IsModeUpdatePostEvts(m_CaptureMode))
                    {
                        bool isRecursive = false;
                        bool isFound = true;
                        while (isFound && (++pGuard < 999))
                        {
                            isFound = false;

                            while ((m_MyHigh.Count > 0) && (++pGuard < 999))
                            {
                                isRecursive = true;
                                CustomCaptureInfo currentCci = (CustomCaptureInfo)m_MyHigh.Dequeue();
                                CciContainer.ProcessInitialize(currentCci);
                                // FI 20170928 [23452] Appel à Finalize (Avant cela était effectué dans ProcessInitialize (dernière instruction)
                                Finalize(currentCci.ClientId_WithoutPrefix, CustomCaptureInfosBase.ProcessQueueEnum.None);
                                //Debug();
                            }
                            if (m_MyLow.Count > 0)
                            {
                                isRecursive = true;
                                isFound = true;
                                CustomCaptureInfo currentCci = (CustomCaptureInfo)m_MyLow.Dequeue();
                                CciContainer.ProcessInitialize(currentCci);
                                // FI 20170928 [23452] Appel à Finalize (Avant cela était effectué dans ProcessInitialize (dernière instruction)
                                Finalize(currentCci.ClientId_WithoutPrefix, CustomCaptureInfosBase.ProcessQueueEnum.None);
                                //Debug();
                            }
                        }
                        if ((pGuard >= 999))
                        {
                            throw (new SpheresException2(MethodInfo.GetCurrentMethod().Name, "Infinite Loop"));
                        }

                        if (isRecursive)
                            Dump_ToDocument(pGuard);
                    }
                }
            }

            FinalizeQuickInput();
        }

        /// <summary>
        /// <para>1/ Déversement des CCI sur l'IHM</para>
        /// <para>2/ Mise à Disabled de certains contrôles</para>
        /// <para>3/ Reload de certaines DDL</para>
        /// </summary>
        /// <param name="pPage"></param>
        /// FI 20140708 [20179] Modify
        /// EG 20170822 [23342] Add Cst.TMS
        /// EG 20170926 [22374] Upd
        public virtual void Dump_ToGUI(CciPageBase pPage)
        {
            pPage.AddAuditTimeStep("Start CustomCaptureInfosBase.Dump_ToGUI");

            //FI 20140708 [20179] isModeConsult n'est plus utilisé
            //bool isModeConsult = MethodsGUI.IsModeConsult(pPage);
            //bool isModeConsult = Cst.Capture.IsModeConsult(this.CaptureMode);  

            if (null != m_CciContainer)
                m_CciContainer.RefreshCciEnabled();

            string warningMsg = string.Empty;
            foreach (CustomCaptureInfo cci in this)
            {
                bool isControlEnabled = cci.IsEnabled;

                // FI 20121126 [18224] appel à ma méthode GetCciControl
                //Control control = (Control)pPage.FindControl(cci.ClientId);
                Control control = GetCciControl(cci.ClientId, pPage);
                //
                if (null != control)
                {
                    switch (cci.ClientId_Prefix)
                    {
                        case Cst.TXT:
                        case Cst.QKI:
                            SetTextBox(control, cci, isControlEnabled, pPage);
                            break;
                        case Cst.TMS:
                            SetTimestamp(control, cci, Tz.Tools.UniversalTimeZone, isControlEnabled, pPage);
                            break;
                        case Cst.HSL:
                            SetHtmlSelect(control, cci, isControlEnabled);
                            break;
                        case Cst.DDL:
                            SetDropDown(control, cci, isControlEnabled, pPage);
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

            // FI 20140708 [20179] pas de mise en place de focus sur le prochain contrôle
            if ((false == Cst.Capture.IsModeMatch(CaptureMode)) && (pPage.FocusMode == CciPageBase.FocusModeEnum.Forced))
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
        /// FI 20140401 [19793] Tuning
        /// EG 20171009 [23452] Upd
        public void Finalize(string pClientId_WithoutPrefix, ProcessQueueEnum pProcessQueue)
        {
            Finalize(pClientId_WithoutPrefix, pProcessQueue, true);
        }
        /// EG 20171009 [23452] Upd Add test cci.ISTimestamp
        public void Finalize(string pClientId, ProcessQueueEnum pProcessQueue, bool pIsWithoutPrefix)
        {
            CustomCaptureInfo cci = this[pClientId, pIsWithoutPrefix];
            if (null != cci)
            {
                switch (pProcessQueue)
                {
                    case ProcessQueueEnum.High:
                    case ProcessQueueEnum.Low:
                        Enqueue(cci, pProcessQueue);
                        break;
                    default:
                        cci.Finalize(false == IsPreserveData);
                        if (cci.IsTimestamp)
                            Finalize(cci.ClientIdZone, pProcessQueue, false);
                        break;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private void FinalizeQuickInput()
        {
            // FI 20200429 [XXXXX] Usage instruction Linq
            //foreach (CustomCaptureInfo cci in this)
            //{
            //    if (cci.IsQuickInput)
            //    {

            //        SetNewValueQuickInput(cci); // Alimentation de NewValue du cci Quick (avec Formatage)
            //        cci.Finalize(false);
            //    }
            //}

            foreach (CustomCaptureInfo cci in this.List.Cast<CustomCaptureInfo>().Where(x => x.IsQuickInput))
            {
                SetNewValueQuickInput(cci); // Alimentation de NewValue du cci Quick (avec Formatage)
                cci.Finalize(false);
            }
        }

        /// <summary>
        /// Retourne la liste des ccis modifiés (tels que <see cref="CustomCaptureInfo.NewValue"/> != <see cref="CustomCaptureInfo.LastValue"/>).
        /// Les ccis <seealso cref="CustomCaptureInfo.IsTimeZone"/> sont exclus.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<CustomCaptureInfo> GetCciHasChanged()
        {
            // FI 20240307 [XXXXX] les ccis TimeZone sont exlus
            return from CustomCaptureInfo item in this.List.Cast<CustomCaptureInfo>().Where(x => (false == x.IsTimeZone) && x.HasChanged)
                   select item;
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
                string value = this[pClientId_WithoutPrefix].NewValue;
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
                if (cci.IsEnabled)
                {
                    ret = cci;
                    break;
                }
            }
            return ret;

        }

        /// <summary>
        /// Obtient un message d'erreur par concaténation des différentes propriétés ErrorMsg des ccis
        /// </summary>
        /// <returns></returns>
        public string GetErrorMessage()
        {
            string[] errorMessage = null;
            return GetErrorMessage(out _, ref errorMessage);

        }
        /// <summary>
        ///  Retourne le message d'erreur associés au cci
        ///  <para>Retourne un indicateur si toutes les erreurs s'appliquent uniquement sur des ccis tel isMissingMode == true</para> 
        ///  <para>Si oui le trade peut être sauvegarder en incomplet</para>
        ///  <para>Si non le trade contient des erreurs blocantes => le trade dne sera pas sauvegardé</para>
        /// </summary>
        /// <param name="pShortErrorMessage"></param>
        /// <param name="pIsAllCciMissingMode">Retourne true si tous les cci avec des messages d'erreur sont  isMissingMode == true</param>
        /// <returns></returns>
        /// FI 20131122 [19233] Appel à GetRessource
        /// FI 20131127 [19233] Ajout de l'erreur présente dans le cci
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
                if (StrFunc.IsFilled(cci.ErrorMsg))
                {
                    string resource = GetRessource(cci);

                    string msg = StrFunc.AppendFormat("{0} : {1}" + Cst.CrLf + "[" + Ressource.GetString("Detail") + " : {2}]", resource, cci.ErrorMsg, cci.ClientId_WithoutPrefix);
                    ret.Append(msg + Cst.CrLf);

                    // FI 20131127 [19233] add {2}
                    string msg2 = StrFunc.AppendFormat("- {0}: <b>{1}</b>, <b>{2}</b>, (id:{3})" + Cst.CrLf, resource, cci.NewValue, cci.ErrorMsg, cci.ClientId_WithoutPrefix);
                    if ((-1 < mesgNumber) && (mesgNumber < ArrFunc.Count(pShortErrorMessage)))
                    {
                        pShortErrorMessage[mesgNumber] = msg2;
                        mesgNumber++;
                    }
                    //
                    // EG 20240213 [WI756] Correctif Majuscule sur [xxx]Specified
                    if (cci.IsMissingModeSpecified && false == cci.IsMissingMode)
                        pIsAllCciMissingMode = false;
                }
            }
            return ret.ToString();
        }

 

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pClientId_WithoutPrefix"></param>
        /// <returns></returns>
        /// FI 20161214 [21916] Modify
        public string GetLastValue(string pClientId_WithoutPrefix)
        {
            string ret = string.Empty;

            // FI 20161214 [21916] tuning    
            //if (this.Contains(pClientId_WithoutPrefix))
            //    ret = this[pClientId_WithoutPrefix].LastValue;

            CustomCaptureInfo cci = this[pClientId_WithoutPrefix];
            if (null != cci)
                ret = cci.LastValue;

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
                if ((this[i].QuickClientId == pQuickClientId) && (this[i].QuickDataPosition > ret))
                {
                    ret = this[i].QuickDataPosition;
                }
            }
            return ret;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pClientId_WithoutPrefix"></param>
        /// <returns></returns>
        /// FI 20161214 [21916] Modify  
        public string GetNewValue(string pClientId_WithoutPrefix)
        {
            string ret = string.Empty;
            //if (this.Contains(pClientId_WithoutPrefix))
            //    ret = this[pClientId_WithoutPrefix].NewValue;

            // FI 20161214 [21916] Tuning
            CustomCaptureInfo cci = this[pClientId_WithoutPrefix];
            if (null != cci)
                ret = cci.NewValue;

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
        /// Alimentation des propriétés newValueMatch des ccis
        /// </summary>
        /// FI 20140708 [20179] add method
        protected virtual void Initialize_NewValueMatch()
        {

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
        /// FI 20121126 [18224] Modification de la signature de la méthode => pPage est de type CciPageBase
        /// FI 20140708 [20179] Modify => Gestion du mode Match
        /// EG 20170822 [23342] Add Cst.TMS 
        /// EG 20171003 [23452] Upd Cst.TMS|TMZ
        /// EG 20171009 [23452] Upd ISO8601DateTime
        /// EG 20171016 [23509] Upd
        public void Initialize_FromGUI(CciPageBase pPage)
        {
            pPage.AddAuditTimeStep("Start CustomCaptureInfosBase.Initialize_FromGUI");

            string eventTarget = string.Empty + pPage.PARAM_EVENTTARGET;

            //FI 20140708 [20179] Gestion du mode Match 
            if (Cst.Capture.IsModeMatch(this.CaptureMode))
            {
                foreach (CustomCaptureInfo cci in this)
                {
                    CustomObject co = GetCciCustomObject(cci.ClientId, pPage);
                    if ((null != co) && co.IsToMatch)
                    {
                        if (GetCciControl(cci.ClientId, pPage) is WebControl control)
                            cci.NewValueMatch = ControlsTools.GetMatchValue(control);
                    }
                }
            }
            else
            {
                foreach (CustomCaptureInfo cci in this)
                {
                    cci.IsLastInputByUser = false;
                    string clientId = cci.ClientId;
                    string data = string.Empty;
                    //Note: On traite ici les contrôles non AutoPostback et le contrôle qui a généré le Postback
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
                                // FI 20200828 [XXXXX] Ne pas faire de maj sur contrôle ReadOnly
                                // Ce contrôle peut avoir un contenu particulier (Exemple affichage d'une date UTC dont le format est fonction du profil)
                                // Il ne faut pas que contenu particulier provoque une modoification du CCI  
                                // Exemple cciEnum.denDate de la class CciDenOptionAction 
                                if (((TextBox)control).ReadOnly)
                                    isOk = false;
                                break;
                            case Cst.TMS:
                            case Cst.TMZ:
                                if (control is WCZonedDateTime ctrl)
                                {
                                    if ((null != ctrl.Zdt) && ctrl.Zdt.IsDateFilled)
                                    {
                                        // EG 20240531 [WI926] New 
                                        // Date vierge autorisée (TRADETEMPLATE only)
                                        if (ctrl.Zdt.DateIsReset)
                                            data = string.Empty;
                                        else
                                            data = (cci.ClientId_Prefix == Cst.TMS) ? ctrl.Zdt.ISO8601DateTime : ctrl.Zdt.ZoneId;
                                    }
                                    else
                                    {
                                        data = (cci.ClientId_Prefix == Cst.TMS) ? ctrl.OffsetDateTimeValue : ctrl.ZonedDateTimeValue;
                                    }
                                }
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
                            //Lorsque NewValue est alimentée avec un mot clef, on n'écrase pas ce mot clef par ce qui es affiché (data) correspond au libellé associé au mot clef
                            if (cci.IsNewValueKeyword && cci.Display == data)
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
                                        cci.NewValue = data;
                                    }
                                }
                                catch { cci.NewValue = string.Empty; } // Eviter de planter en saisie 
                            }
                            //
                            if ((cci.HasChanged) && (clientId == eventTarget))
                            {
                                //Warning: Ne pas écrire: cci.IsInputByUser = cci.HasChanged 
                                cci.IsInputByUser = true;
                                cci.IsLastInputByUser = true;
                            }
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
                                data += CciOfQuickInput(quickClientId, i).NewValueFmtToCurrentCulture + cci.QuickSeparator;
                            else
                                data += CciOfQuickInput(quickClientId, i).NewValue + cci.QuickSeparator;
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
        public void LoadDocument(Cst.Capture.ModeEnum pModeCapture)
        {
            LoadDocument(pModeCapture, null);
        }

        /// <summary>
        /// Chargement des ccis à partir du document
        /// <para>Synchronisation des contrôles de la page {pPage}</para>
        /// </summary>
        /// <param name="pModeCapture"></param>
        /// <param name="pPage">null est possible</param>
        /// FI 20140708 [20179] Modify
        public void LoadDocument(Cst.Capture.ModeEnum pModeCapture, CciPageBase pPage)
        {
            if (null != pPage)
                pPage.AddAuditTimeStep("Start CustomCaptureInfosBase.LoadCapture");

            m_IsLoading = true;

            m_CaptureMode = pModeCapture;


            //Alimentation des CCI à partir du document 
            Initialize_FromDocument();

            //FI 20140708 [20179] Appel Initialize_NewValueMatch
            if (m_CaptureMode == Cst.Capture.ModeEnum.Match)
                Initialize_NewValueMatch();

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
        ///  Retourne true s'il exist si le cci est présent dans une des queue (m_MyHigh ou m_MyLow)
        /// </summary>
        /// <param name="pClientId_WithoutPrefix"></param>
        /// <returns></returns>
        /// FI 20161214 [21916] Add
        public Boolean IsInQueue(string pClientId_WithoutPrefix)
        {
            Boolean ret = false;
            if (false == this.IsQueueEmpty)
            {

                CustomCaptureInfo cci = (m_MyHigh.Cast<CustomCaptureInfo>().Where(x => x.ClientId_WithoutPrefix == pClientId_WithoutPrefix)).FirstOrDefault();
                ret = (null != cci);

                if (false == ret)
                {
                    cci = (m_MyLow.Cast<CustomCaptureInfo>().Where(x => x.ClientId_WithoutPrefix == pClientId_WithoutPrefix)).FirstOrDefault();
                    ret = (null != cci);
                }
            }
            return ret;
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
            CustomCaptureInfo cci = this[pClientId];
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
                this[pClientId_WithoutPrefix].ErrorMsg = pErrorMsg;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pPage"></param>
        /// EG 20170822 [23342] Add Cst.TMS
        protected void SetFocus(CciPageBase pPage)
        {
            PlaceHolder plh = pPage.PlaceHolder;
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
                if (cci.ClientId == eventTarget)
                {
                    //Cci en liaison avec le control HTML à l'origine du post de la page
                    isFound_CciEventTarget = true;

                    if (plh.FindControl(Cst.BUT + cci.ClientId_WithoutPrefix) is ImageButton button && button.TabIndex != -1)
                        isFound_CciEventTarget_Next = true;

                    if (false == isFound_CciEventTarget_Next)
                    {
                        if (plh.FindControl(Cst.BUT + cci.ClientId_WithoutPrefix) is Button button2 && button2.TabIndex != -1)
                            isFound_CciEventTarget_Next = true;
                    }

                }
                else if (cci.ClientId == activeElement)
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
                    Control control = (Control)plh.FindControl(cci.ClientId);
                    if (null != control)
                    {
                        switch (cci.ClientId_Prefix)
                        {
                            case Cst.TXT:
                            case Cst.QKI:
                                TextBox txt = (TextBox)control;
                                isControlEnabled = txt.Enabled;
                                break;
                            case Cst.TMS:
                                TextBox tms = (TextBox)control;
                                isControlEnabled = tms.Enabled;
                                break;
                            case Cst.TMZ:
                                DropDownList tmz = (DropDownList)control;
                                isControlEnabled = tmz.Enabled;
                                break;
                            case Cst.HSL:
                                isControlEnabled = (false == ((WCHtmlSelect)control).Disabled);
                                break;
                            case Cst.DDL:
                                bool isUseControlBase = false;
                                try
                                {
                                    WCDropDownList2 ddl = (WCDropDownList2)control;
                                    isControlEnabled = ddl.Enabled && (!ddl.HasViewer);
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
                if (isFound_CciEventTarget && isControlEnabled && (!isFound_CciEventTarget_Next) && (cci.ClientId != eventTarget))
                {
                    //Cci en liaison avec le 1er control HTML "enabled" suivant le control HTML à l'origine du post de la page
                    isFound_CciEventTarget_Next = true;
                    pPage.ActiveElementForced = cci.ClientId;
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
        /// Mise à jour de la property NewValue du cci {pClientId}
        /// <para>Aucune mise à jour lorsque le CCI n'existe pas </para>
        /// </summary>
        /// <param name="pClientId">id du cci</param>
        /// <param name="pNewValue">Nouvelle valeur</param>
        public void SetNewValue(string pClientId, string pNewValue)
        {
            SetNewValue(pClientId, pNewValue, false);
        }
        /// <summary>
        /// Mise à jour de la property NewValue du cci {pClientId}
        /// <para>Aucune mise à jour lorsque le CCI n'existe pas </para>
        /// </summary>
        /// <param name="pClientId">id du cci</param>
        /// <param name="pNewValue">Nouvelle valeur</param>
        /// <param name="pbOnlyIfIsEmpty">si true la mise à jour s'effectue uniquement si le cci est non renseigné</param>
        /// FI 20140401 [19793] Tuning
        /// FI 20161214 [21916] Modify
        /// EG 20170926 [22374] New 
        public void SetNewValue(string pClientId, string pNewValue, bool pbOnlyIfIsEmpty)
        {
            SetNewValue(pClientId, true, pNewValue, pbOnlyIfIsEmpty);
        }
        //EG 20170919 [22374] Add pIsClientWithoutPrefix parameter
        /// <summary>
        /// Mise à jour de la property NewValue du cci {pClientId}
        /// <para>Aucune mise à jour lorsque le CCI n'existe pas </para>
        /// </summary>
        /// <param name="pClientId">id du cci</param>
        /// <param name="pIsClientWithoutPrefix">pClientId est avec ou sans préfixe</param>
        /// <param name="pNewValue">Nouvelle valeur</param>
        /// <param name="pbOnlyIfIsEmpty">si true la mise à jour s'effectue uniquement si le cci est non renseigné</param>
        public void SetNewValue(string pClientId, bool pIsClientWithoutPrefix, string pNewValue, bool pbOnlyIfIsEmpty)
        {
            CustomCaptureInfo cci = this[pClientId, pIsClientWithoutPrefix];

            if (null != cci)
            {
                if (pbOnlyIfIsEmpty) /// FI 20161214 [21916] correction lorsque pbOnlyIfIsEmpty = true (qui ne fonctionnait pas avant ??? bizarre) 
                {
                    if (StrFunc.IsEmpty(cci.NewValue))
                    {
                        cci.NewValue = pNewValue;
                    }
                    else
                    {
                        //Nothing TODO
                    }
                }
                else
                {
                    cci.NewValue = pNewValue;
                }
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
                            data += CciOfQuickInput(quickClientId, i).NewValueFmtToCurrentCulture + pCci.QuickSeparator;
                        else
                            data += CciOfQuickInput(quickClientId, i).NewValue + pCci.QuickSeparator;
                    }
                }
                pCci.NewValue = data;
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
                this[pClientId_WithoutPrefix].QuickClientId = pQuickClientId;
                this[pClientId_WithoutPrefix].QuickDataPosition = GetMaxQuickDataPosition(pQuickClientId) + 1;
            }
        }

        /// <summary>
        /// Affecte la propriété NewValue d'un cci, avec la nouvelle valeur saisie 
        /// <para>Ceci n'est effectué que si la propriété NewValue du cci est identique à la valeur précédente</para>
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
        /// FI 20140401 [19793] Tuning, Spheres n'utilise plus la méthode Contains pour éviter de balayer 2 fois la collection
        public void Synchronize(string pClientId_WithoutPrefix, string pLastValue, string pNewValue, bool pIsAlways)
        {
            //if (Contains(pClientId_WithoutPrefix))
            //{
            CustomCaptureInfo cci = this[pClientId_WithoutPrefix];
            if (null != cci)
            {
                if ((cci.NewValue == pLastValue) || (StrFunc.IsEmpty(cci.NewValue) && StrFunc.IsEmpty(pLastValue)))
                {
                    if (cci.IsMandatory || pIsAlways)
                        cci.NewValue = pNewValue;
                    else if (StrFunc.IsFilled(cci.NewValue))
                        cci.NewValue = pNewValue;
                }
            }
            //}
        }

        /// <summary>
        /// 
        /// </summary>
        private void SynchronizeCcisFromQuickInput()
        {
            // FI 20200429 [XXXXX] Usage instruction Linq
            foreach (CustomCaptureInfo cci in this.List.Cast<CustomCaptureInfo>().Where(x => x.IsQuickInput && x.HasChanged))
            {
                //=> Mise à jour des Ccis associés
                string[] aNewValue = cci.NewValue.Split(cci.QuickSeparator.ToCharArray());
                for (int i = 0; i < aNewValue.Length; i++)
                {
                    if (ConstainsCciOfQuickInput(cci.ClientId_WithoutPrefix, i + 1))
                    {
                        if (cci.IsQuickFormatOTCml)
                        {
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
                        }
                        else
                        {
                            CciOfQuickInput(cci.ClientId_WithoutPrefix, i + 1).NewValue = aNewValue[i];
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
        /// FI 20121126 [18224] La méthode FindControl n'est plus utilisée car non performante
        protected static Control GetCciControl(string pClientId, CciPageBase pPage)
        {
            Control ret = pPage.GetCciControl(pClientId);
            return ret;
        }

        /// <summary>
        /// Retourne le CustomObject à l'origine du contrôle dont l'id vaut {pClientId}
        /// </summary>
        /// <param name="pClientId"></param>
        /// <param name="pPage"></param>
        /// <returns></returns>
        /// FI 20140708 [20179] add method
        protected static CustomObject GetCciCustomObject(string pClientId, CciPageBase pPage)
        {
            CustomObject ret = pPage.GetCciCustomObject(pClientId);
            return ret;
        }


        /// <summary>
        /// Mise à jour du control DSP associé au cci
        /// </summary>
        /// <param name="pPage"></param>
        /// <param name="cci"></param>
        ///FI 20121126 [18224] Modification de la signature de la fonction
        // EG 20210929 [XXXXX]  Introduction d'une classe CSS (ccierror) pour gérer l'affichage de Cci Display si Erreur
        protected static void SetCciDisplay(CciPageBase pPage, CustomCaptureInfo cci)
        {
            //Existence d'un label associé (ie: affichage d'un libellé long)
            Control control = GetCciControl(Cst.DSP + cci.ClientId_WithoutPrefix, pPage);
            if (null != control)
            {
                string msg = string.Empty;
                string cssClass = string.Empty;

                if (cci.HasError)
                {
                    msg = cci.ErrorMsg;
                    cssClass = " ccierror";
                }
                else if (StrFunc.IsFilled(cci.Display) && (false == cci.IsNewValueKeyword))
                {
                        msg = cci.Display;
                }

                Label lbl = (Label)control;
                lbl.ToolTip = msg;
                lbl.Text = msg;
                lbl.CssClass = lbl.CssClass.Replace(" ccierror", string.Empty) + cssClass;
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
        // FI 20140708 [20179] Modify => Gestion du mode Match
        protected virtual void SetDropDown(Control control, CustomCaptureInfo cci, bool isControlEnabled, CciPageBase pPage)
        {
            if (!(control is DropDownList ddl))
                throw new Exception(StrFunc.AppendFormat("control {0} is not a  DropDown", cci.ClientId));

            string data = cci.NewValue;

            if (!(cci.IsMandatory))
                ControlsTools.DDLLoad_AddListItemEmptyEmpty(ddl);

            ddl.Enabled = isControlEnabled;

            bool isFound = ControlsTools.DDLSelectByValue(ddl, data);

            if ((!isFound) && (StrFunc.IsFilled(data)))
            {
                //FI 20121126 [18224] FindControl sur pPage.placeHolder 
                string msg_tmp = " " + Ressource.GetString("Msg_UnavailableOrRemoved", "[disabled or removed]");

                ListItem liUnavailable = new ListItem(data + msg_tmp, data);
                liUnavailable.Attributes.Add("style", "color:#FFFFFF;background-color:#AE0303");
                ddl.Items.Add(liUnavailable);
                _ = ControlsTools.DDLSelectByValue(ddl, data);
            }

            // FI 20140708 [20179] add IsModeMatch
            if (Cst.Capture.IsModeMatch(CaptureMode))
                SetLookMatchControl(ddl, cci, pPage);
        }

        /// <summary>
        /// Mise à jour du control {control} vis à vis {cci}
        /// </summary>
        /// <param name="control"></param>
        /// <param name="cci"></param>
        /// <param name="isControlEnabled"></param>
        /// FI 20140708 [20179] Modify => Gestion IsModeMatch 
        protected virtual void SetTextBox(Control control, CustomCaptureInfo cci, bool isControlEnabled, CciPageBase pPage)
        {
            Boolean isModeConsult = Cst.Capture.IsModeConsult(this.CaptureMode);

            string data;
            //20090427 FI Sur demande de PL les types de données numériques sont systématiquement interprétés (Mode REGULAR ou MODE TEMPLATE)
            //si l'on saisit 100% => on veut dans le template 1 et non pas 100
            if ((IsPreserveData) && (false == cci.IsTypeDecimal))
                data = cci.NewValue;
            else
                data = cci.NewValueFmtToCurrentCulture;

            if (cci.IsNewValueKeyword && (false == IsPreserveData))
                data = cci.Display;

            if (!(control is TextBox txt))
                throw new Exception(StrFunc.AppendFormat("control {0} is not a TextBox", cci.ClientId));

            txt.Text = data;

            ControlsTools.RemoveStyle(txt.Style, "color");
            if (StrFunc.IsFilled(cci.ErrorMsg))
                ControlsTools.SetStyleList(txt.Style, "color:#AE0303");
            else if (cci.IsNewValueKeywordAlert)
                ControlsTools.SetStyleList(txt.Style, "color:#AE0303");
            else if (cci.IsNewValueKeywordWarning)
                ControlsTools.SetStyleList(txt.Style, "color:#EF7A27");
            else if (cci.IsNewValueKeyword)
                ControlsTools.SetStyleList(txt.Style, "color:#51AD26");

            txt.Enabled = isControlEnabled;

            if (StrFunc.IsFilled(txt.CssClass))
            {
                string cssClassBase = EFSCssClass.GetCssClassBase(txt.CssClass);
                if (StrFunc.IsFilled(cssClassBase))
                {
                    bool isMultiline = StrFunc.ContainsIn(txt.CssClass, "Multiline");
                    txt.CssClass = EFSCssClass.GetCssClass(cci.IsTypeInt || cci.IsTypeDecimal, cci.IsMandatory, isMultiline, isModeConsult || txt.ReadOnly, cssClassBase);
                }
                // EG 20240123 [WI816] Trade input: Modification of periodic fees uninvoiced on a trade
                // Gestion du bouton associé au textBox de types Date... (le masquer si textBox est disabled)
                if (Cst.Capture.IsModeUpdateFeesUninvoiced(CaptureMode))
                {
                    if ((cci.IsTypeDate || cci.IsTypeDateTime || cci.IsTypeTime) && !txt.ReadOnly)
                    {
                        bool existClassPicker = txt.CssClass.Contains("DtPicker");
                        if ((!isControlEnabled) && existClassPicker)
                        {
                            txt.CssClass = txt.CssClass.Replace("DtPicker", string.Empty)
                                .Replace("DtTimePicker", string.Empty)
                                .Replace("TimePicker", string.Empty)
                                .Replace("DtTimeOffsetPicker", string.Empty)
                                .Replace("TimeOffsetPicker", string.Empty);
                        }
                        else if (isControlEnabled && !existClassPicker)
                        {
                            if (cci.IsTypeDate)
                                txt.CssClass = "DtPicker " + txt.CssClass;
                            else if (cci.IsTypeDateTime)
                                txt.CssClass = "DtTimePicker " + txt.CssClass;
                            else if (cci.IsTypeTime)
                                txt.CssClass = "TimePicker " + txt.CssClass;
                            else if (cci.IsTypeDateTimeOffset)
                                txt.CssClass = "DtTimeOffsetPicker " + txt.CssClass;
                            else if (cci.IsTypeTimeOffset)
                                txt.CssClass = "TimeOffsetPicker " + txt.CssClass;
                        }
                    }
                }
            }

            // FI 20140708 [20179]
            if (Cst.Capture.IsModeMatch(CaptureMode))
            {
                // En mode Match Spheres® rentre seulement 1 fois dans cette méthode pour chaque contôle
                // la méthode Dump_ToGUI est appelée une seule fois (qd Spheres® recharge le trade à l'initilisation de l'action matching)
                // Le look du contrôle peut déjà exister en fonction des valeurs par défaut définies sur le descriptif XML
                if (cci.NewValueMatch.HasValue)
                    SetLookMatchControl(txt, cci, pPage);
            }

        }

        /// <summary>
        /// Mise à jour du control {control} vis à vis {cci}
        /// </summary>
        /// <param name="control"></param>
        /// <param name="cci"></param>
        /// <param name="isControlEnabled"></param>
        /// EG 20170822 [23342] New
        /// EG 20170926 [22374] Upd SetZone
        /// EG 20171009 [23452] Upd data = pCci.NewValue
        protected virtual void SetTimestamp(Control pControl, CustomCaptureInfo pCci, string pZone, bool pIsControlEnabled, CciPageBase pPage)
        {
            Boolean isModeConsult = Cst.Capture.IsModeConsult(this.CaptureMode);

            string data = pCci.NewValue;

            if (!(pControl is WCZonedDateTime ctrl))
                throw new Exception(StrFunc.AppendFormat("control {0} is not a WCZonedDateTime", pCci.ClientId));

            if (StrFunc.IsFilled(data))
            {
                ctrl.Zdt.SetZone(pZone);
                ctrl.Zdt.Parse = data;
            }

            ControlsTools.RemoveStyle(ctrl.Style, "color");
            if (StrFunc.IsFilled(pCci.ErrorMsg))
                ControlsTools.SetStyleList(ctrl.Style, "color:#AE0303");

            ctrl.Enabled = pIsControlEnabled;

            if (StrFunc.IsFilled(ctrl.CssClass))
            {
                // FI 20240528 [XXXXX] use of CssClassEdit
                string cssClass = ctrl.CssClass;

                

                if (StrFunc.IsFilled(ctrl.CssClassEdit))
                    cssClass = cssClass.Replace($"{ctrl.CssClassEdit} ", string.Empty);

                string cssClassBase = EFSCssClass.GetCssClassBase(cssClass);
                if (StrFunc.IsFilled(cssClassBase))
                    ctrl.CssClass = EFSCssClass.GetCssClass(false, pCci.IsMandatory, false, isModeConsult || ctrl.ReadOnly, cssClassBase);

                if (StrFunc.IsFilled(ctrl.CssClassEdit))
                    ctrl.CssClass = $"{ctrl.CssClassEdit} {ctrl.CssClass}";
            }

            if (Cst.Capture.IsModeMatch(CaptureMode))
            {
                // En mode Match Spheres® rentre seulement 1 fois dans cette méthode pour chaque contôle
                // la méthode Dump_ToGUI est appelée une seule fois (qd Spheres® recharge le trade à l'initilisation de l'action matching)
                // Le look du contrôle peut déjà exister en fonction des valeurs par défaut définies sur le descriptif XML
                if (pCci.NewValueMatch.HasValue)
                    SetLookMatchControl(ctrl, pCci, pPage);
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
            string data = cci.NewValue;
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

                if (control is WebControl webCtrl)
                    webCtrl.Attributes.Remove("displaywhenempty");
            }
            //
            value = controlAttributAccessor.GetAttribute("isddlbanner");
            if (StrFunc.IsFilled(value))
            {
                if (control is WebControl webCtrl)
                    webCtrl.Attributes.Remove("isddlbanner");
            }
        }

        /// <summary>
        ///  Obtient la ressource associé au cci {pCci}
        /// </summary>
        /// <param name="pCci"></param>
        /// <returns></returns>
        /// FI 20131125 [19233] Add Methode
        public virtual string GetRessource(CustomCaptureInfo pCci)
        {
            //FI GLOP C'est super môche d'utiliser une classe qui appartient à l'espace de nom EFS.Common.Web
            CustomObject co = new CustomObject(pCci.ClientId_WithoutPrefix);
            string ret = co.Resource;
            return ret;
        }

        /// <summary>
        ///  Init de la property ClientId_DumpToDocument
        /// </summary>
        /// FI 20140414 [19793] Tuning, Add method
        /// EG 20171009 [23452] Upd Contains
        private void InitializeClientId_DumpToDocument()
        {
            this.ClientId_DumpToDocument = (from CustomCaptureInfo item in GetCciHasChanged()
                                            select item.ClientId_WithoutPrefix).ToList();
        }

        /// <summary>
        /// Mise en place du look du control {control} associé au {cci} si la données est à contrôlée (CaptureMode = Match)
        /// </summary>
        /// <param name="control"></param>
        /// <param name="cci"></param>
        /// <param name="pPage"></param>
        // FI 20140708 [20179] add Method
        protected void SetLookMatchControl(WebControl control, CustomCaptureInfo cci, CciPageBase pPage)
        {
            CustomObject customObject = GetCciCustomObject(cci.ClientId, pPage);
            if (customObject.IsToMatch)
                ControlsTools.SetLookMatch(control, cci.NewValueMatch);
        }

        /// <summary>
        /// Retourne la date/heure (dans le fuseau horaire associé) d'un cci de type datetimeoffset 
        /// <para>Retourne null si le cci n'existe pas, ou si le fuseau horaire associé n'existe pas</para>
        /// </summary>
        /// <param name="pClientId_WithoutPrefix"></param>
        /// <param name="pDefaultTimeZone">time zone par défaut lorsque le cci TMZ n'existe pas (cas possible si importation de trade)</param>
        /// <returns></returns>
        /// <exception cref="InvalidProgramException si le dataype n'est pas datetimeoffset"></exception>
        /// FI 20190625 [XXXXX] Add Method
        /// FI 20190924 [24952][24953] Chgt de signature add
        public Nullable<DateTime> GetLocalDateNewValue(string pClientId_WithoutPrefix, string pDefaultTimeZone)
        {
            Nullable<DateTime> ret = null;
            if (this.Contains(pClientId_WithoutPrefix))
            {
                CustomCaptureInfo cci = this[pClientId_WithoutPrefix];
                if (cci.DataType != TypeData.TypeDataEnum.datetimeoffset)
                    throw new InvalidProgramException(StrFunc.AppendFormat("DataType:{0} invalid", cci.DataType.ToString()));

                string timezone = string.Empty;
                CustomCaptureInfo cciZone = this[cci.ClientIdZone, false];
                if ((null != cciZone) && StrFunc.IsFilled(cciZone.NewValue))
                    timezone = cciZone.NewValue;
                else if (StrFunc.IsFilled(pDefaultTimeZone))
                    timezone = pDefaultTimeZone;

                if (StrFunc.IsFilled(cci.NewValue) && StrFunc.IsFilled(timezone))
                {
                    DateTimeOffset dtOffset = DateTimeOffset.Parse(cci.NewValue, CultureInfo.InvariantCulture);
                    TimeZoneInfo tzInfoTarget = Tz.Tools.GetTimeZoneInfoFromTzdbId(timezone);
                    ret = TimeZoneInfo.ConvertTimeFromUtc(dtOffset.DateTime, tzInfoTarget);
                }
            }
            return ret;
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
        private bool _isMandatory;
        private bool _isMissingMode;
        private bool _isEnabled;
        private TypeData.TypeDataEnum _dataType;
        private EFSRegex.TypeRegex _regex;
        private string _listRetrieval;
        private string _lastValue;
        private string _newValue;
        private bool _isInputByUser;
        private bool _isLastInputByUser;
        private string _errorMsg;
        private string _display;
        private SQL_Table _sql_Table;
        private SQL_Table _lastSql_Table;
        private string _quickClientId;
        private int _quickDataPosition;
        private string _quickSeparator;
        private string _quickFormat;
        
        
        
        #endregion Members

        #region property
        /// <summary>
        /// Le CCI est un timestamp
        /// </summary>
        /// EG 20171009 [23452] New
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool IsTimestamp
        {
            get { return (Cst.TMS == ClientId_Prefix); }
        }

        /// <summary>
        ///  Retourne true si CCI de type TimeZone (Cci qui ont la particularité ne ne pas être saisissable)
        /// </summary>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool IsTimeZone
        {
            get { return (Cst.TMZ == ClientId_Prefix); }
        }

        /// <summary>
        /// Retourne le clientId Zone associé
        /// </summary>
        /// EG 20171009 [23452] New
        public string ClientIdZone
        {
            get
            {
                string clientIdZone = string.Empty;
                if (IsTimestamp)
                    clientIdZone = ClientId.Replace(Cst.TMS, Cst.TMZ);
                return clientIdZone;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlAttributeAttribute("clientId", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string ClientId
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
        public string AccessKey
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
        /// Obtient ou définit true si cci de type system
        /// </summary>
        /// FI 20170116 [21916] Add
        /// FI 20170214 [XXXXX] Add XmlAttributeAttribute
        [System.Xml.Serialization.XmlAttributeAttribute("isSystem", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public Boolean IsSystem
        {
            get;
            set;
        }

        /// <summary>
        /// Obtient ou définit true si Spheres® autorise une valeur inconnue 
        /// <para>Dans ce cas elle sera automatiquement générée lors de l'enregistrement du trade</para>
        /// <para>Exemple : Auto-génération des traders</para>
        /// </summary>
        /// FI 20170404 [23039] Add 
        [System.Xml.Serialization.XmlAttributeAttribute("isAutoCreate", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public Boolean IsAutoCreate
        {
            get;
            set;
        }


        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool IsAccessKeyNextBlock
        {
            get
            {
                return (StrFunc.IsFilled(AccessKey)
                    && (AccessKey == SystemSettings.GetAppSettings("Spheres_NextBlock_AccessKey", "N")));
            }
        }

        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public string Display
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
        public string ErrorMsg
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
        public TypeData.TypeDataEnum DataType
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

        // EG 20160404 Migration vs2013
 
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public string ListRetrieval
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

    
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        // EG 20240213 [WI756] Correctif Majuscule sur [xxx]Specified
        public bool RegexSpecified;

        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlAttributeAttribute("regex", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        // EG 20240213 [WI756] Correctif Majuscule sur [xxx]Specified
        public EFSRegex.TypeRegex Regex
        {
            get
            {
                return _regex;
            }
            set
            {
                _regex = value;
                RegexSpecified = (EFSRegex.TypeRegex.None != _regex);
            }
        }


        /// <summary>
        /// 
        /// </summary>
        // RD 20100819 [] Pour l'Import
        [System.Xml.Serialization.XmlAttributeAttribute("mandatory", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public bool IsMandatory
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
                if (false == IsMandatory && StrFunc.IsFilled(ErrorMsg))
                    ErrorMsg = ErrorMsg.Replace(Ressource.GetString("ISMANDATORY"), string.Empty);
            }
        }



        /// <summary>
        /// 
        /// </summary>
        /// FI 20130702 [18798] Add
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        // EG 20240213 [WI756] Correctif Majuscule sur [xxx]Specified
        public bool IsMissingModeSpecified;


        /// <summary>
        /// Signifie que le cci accepte une valeur vide ou erronée
        /// </summary>
        /// RD 20120312 [] Pour l'Import
        [System.Xml.Serialization.XmlAttributeAttribute("missingMode", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public bool IsMissingMode
        {
            get { return _isMissingMode; }
            set { _isMissingMode = value; }
        }


        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool IsEnabled
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
        public bool IsInputByUser
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
        public bool IsLastInputByUser
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
        /// Valeur courante
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute("value", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string NewValue
        {
            get
            {
                return _newValue;
            }
            set
            {
                //PL 20091229 Add test value on boolean datatype [On Oracle® (Boolean field is NUMBER(1) and data is 0 or 1)]
                if ((this.DataType == TypeData.TypeDataEnum.@bool)
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
        public string LastValue
        {
            get
            {
                return _lastValue;
            }
            set
            {
                //PL 20091229 Add test value on boolean datatype [On Oracle® (Boolean field is NUMBER(1) and data is 0 or 1)]
                if ((this.DataType == TypeData.TypeDataEnum.@bool)
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
        public SQL_Table Sql_Table
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
        public SQL_Table LastSql_Table
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

        /// <summary>
        /// Obtient ou définie un flag qui indique que la donnée NewValue est contrôlée on pas (match)
        /// <para>Le matching des données est gérée via le mode de saisie Cst.Capture.ModeEnum.Match</para>
        /// <para>Obtient null si le cci est une donnée non contrôlée</para>
        /// </summary>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public Nullable<Cst.MatchEnum> NewValueMatch
        {
            get;
            set;
        }

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public string QuickSeparator
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
        public string QuickFormat
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
        public string QuickClientId
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
        public int QuickDataPosition
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
                return ClientId.Substring(lenPrefix);
            }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public string ClientId_Prefix
        {
            get
            {
                if (3 <= ClientId.Length)
                    return ClientId.Substring(0, 3);
                else
                    return string.Empty;
            }
        }
        /// <summary>
        /// Obtient true si la valeur représentée par LastValue est différente la valeur représentée par NewValue 
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
                    ret = BoolFunc.IsTrue(NewValue) != BoolFunc.IsTrue(LastValue);
                }
                else
                {
                    isDefaultTest = true;
                }
                //
                if (isDefaultTest)
                {
                    if (StrFunc.IsEmpty(NewValue))
                        ret = StrFunc.IsFilled(LastValue);
                    else
                        ret = (LastValue != NewValue);
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
            get { return StrFunc.IsFilled(ErrorMsg); }
        }
        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool IsFilled
        {
            get { return StrFunc.IsFilled(NewValue); }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool IsEmpty
        {
            get { return StrFunc.IsEmpty(NewValue); }
        }
        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool IsLastFilled
        {
            get { return StrFunc.IsFilled(LastValue); }
        }
        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool IsLastEmpty
        {
            get { return StrFunc.IsEmpty(LastValue); }
        }

        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool IsFilledValue
        {
            get
            {
                bool ret;
                try
                {

                    if (IsTypeDecimal)
                        ret = StrFunc.IsDecimalInvariantFilled(NewValue);
                    else if (IsTypeInt)
                        ret = StrFunc.IsIntegerInvariantFilled(NewValue);
                    else if (IsTypeDate)
                        ret = DtFunc.IsDateTimeFilled(new DtFunc().StringToDateTime(NewValue, DtFunc.FmtISODate));
                    else if (IsTypeBool)
                        ret = BoolFunc.IsTrue(NewValue);
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
            get { return (TypeData.IsTypeString(DataType)); }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool IsTypeBool
        {
            get { return (TypeData.IsTypeBool(DataType)); }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool IsTypeInt
        {
            get { return (TypeData.IsTypeInt(DataType)); }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool IsTypeDecimal
        {
            get { return (TypeData.IsTypeDec(DataType)); }
        }

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool IsTypeNumeric
        {
            get { return (IsTypeDecimal || IsTypeInt); }
        }

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool IsTypeDate
        {
            get { return (TypeData.IsTypeDate(DataType)); }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool IsTypeDateTime
        {
            get { return (TypeData.IsTypeDateTime(DataType)); }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        // EG 20170918 [23342] New
        public bool IsTypeDateTimeOffset
        {
            get { return (TypeData.IsTypeDateTimeOffset(DataType)); }
        }

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool IsTypeTime
        {
            get { return (TypeData.IsTypeTime(DataType)); }
        }
        // EG 20170918 [22374] New
        public bool IsTypeTimeOffset
        {
            get { return (TypeData.IsTypeTimeOffset(DataType)); }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool IsRegexFixedRate
        {
            get
            {
                return (EFSRegex.TypeRegex.RegexFixedRate == Regex) ||
                        (EFSRegex.TypeRegex.RegexFixedRateExtend == Regex ||
                        (EFSRegex.TypeRegex.RegexFixedRatePercent == Regex));
            }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool IsRegexPercent
        {
            get
            {
                return ((EFSRegex.TypeRegex.RegexPercent == Regex) ||
                          (EFSRegex.TypeRegex.RegexPercentExtend == Regex) ||
                          IsRegexPercentFraction
                        );
            }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool IsRegexPercentFraction
        {
            get { return (EFSRegex.TypeRegex.RegexPercentFraction == Regex); }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool IsRegexRate
        {
            // Cci FixedRate or FloatingRate
            get { return (EFSRegex.TypeRegex.RegexRate == Regex); }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool IsRegexFxRate
        {
            get
            {
                return (EFSRegex.TypeRegex.RegexFxRate == Regex) || (EFSRegex.TypeRegex.RegexFxRateExtend == Regex);
            }

        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool IsRegexAmount
        {
            get
            {
                return (EFSRegex.TypeRegex.RegexAmount == Regex) ||
                       (EFSRegex.TypeRegex.RegexAmountSigned == Regex) ||
                       (EFSRegex.TypeRegex.RegexAmountExtend == Regex) ||
                       (EFSRegex.TypeRegex.RegexAmountSignedExtend == Regex);
            }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool IsFixedRate
        {
            get
            {
                bool bRet = (IsRegexFixedRate);
                if (!bRet)
                    bRet = (IsRegexRate && RateTools.IsFixedRate(NewValue));
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
        /// Obtient true si NewValue contient un mot clef (ie comme par #Keyword:)  
        /// </summary>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool IsNewValueKeyword
        {
            get
            {
                return StrFunc.IsFilled(NewValue) && NewValue.StartsWith(Keyword);
            }
        }
        /// <summary>
        /// Obtient true si NewValue contient un mot clef (ie comme par #KeywordAlert:)  
        /// </summary>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool IsNewValueKeywordAlert
        {
            get
            {
                return StrFunc.IsFilled(NewValue) && NewValue.StartsWith(KeywordAlert);
            }
        }
        /// <summary>
        /// Obtient true si NewValue contient un mot clef (ie comme par #KeywordWarning:)  
        /// </summary>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool IsNewValueKeywordWarning
        {
            get
            {
                return StrFunc.IsFilled(NewValue) && NewValue.StartsWith(KeywordWarning);
            }
        }

        /// <summary>
        /// Onbient NewValue formattée selon la culture de la station
        /// </summary>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        /// EG 20170918 [23342] Add IsTypeDateTimeOffset
        /// EG 20171003 [23452] Upd IsTypeDateTimeOffset|IsTypeTimeOffset
        public string NewValueFmtToCurrentCulture
        {
            // Format NewValue With CurrentCulture
            get
            {
                string data = NewValue;
                if (StrFunc.IsFilled(data))
                {
                    #region isDecimal
                    if (IsFixedRate || IsRegexPercent)
                    {
                        if (false == IsRegexPercentFraction || (IsRegexPercentFraction && IsDataValidForFixedRate(data)))
                        {
                            FixedRate fixedRate = new FixedRate(data, CultureInfo.InvariantCulture);
                            data = fixedRate.ToString();
                        }
                        else if (IsRegexPercentFraction)
                        {
                            Fraction fraction = new Fraction(data, true);
                            data = fraction.ToString();
                        }
                    }
                    else if (IsRegexFxRate)
                    {
                        FxRate fxRate = new FxRate(data, CultureInfo.InvariantCulture);
                        data = fxRate.ToString();
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

                    #region IsTypeDateTimeOffset
                    // EG 20170918 [23342]
                    if (IsTypeDateTimeOffset)
                        data = Tz.Tools.GetDateTimeOffsetString(data);
                    #endregion

                    #region IsTypeTimeOffset
                    // EG 20170918 [22374]
                    if (IsTypeTimeOffset)
                        data = Tz.Tools.GetTimeOffsetString(data);
                    #endregion


                }
                return data;
            }
        }

        /// <summary>
        /// Définit NewValue à partir du donné formattée selon la culture de la station
        /// </summary>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        /// EG 20170918 [23342] Add IsTypeDateTimeOffset
        /// EG 20171016 [23509] Upd
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

                    #region IsDateTimeOffset
                    // EG 20170918 [23342]
                    // EG 20171031 [23509] Upd
                    if (IsTypeDateTimeOffset)
                    {
                        if (Tz.Tools.IsDateFilled(data))
                        {
                            DateTimeOffset dt = DateTimeOffset.Parse(data, CultureInfo.CurrentCulture.DateTimeFormat);
                            data = DtFunc.DateTimeOffsetUTCToStringTZ(dt);
                        }
                    }
                    #endregion

                    #region IsTypeTimeOffset
                    // EG 20170918 [22374]
                    // EG 20171031 [23509] Upd
                    if (IsTypeTimeOffset)
                    {
                        DateTimeOffset dt = DateTimeOffset.Parse(data, CultureInfo.CurrentCulture.DateTimeFormat);
                        data = DtFunc.DateTimeOffsetUTCToStringTZ(dt).Split('T')[1];
                    }
                    #endregion

                }
                NewValue = data;
            }
        }
        #endregion property

        #region constructors
        public CustomCaptureInfo(string pClientId, string pAccessKey,
            bool pIsMandatory, TypeData.TypeDataEnum pDataType, bool pIsEnabled, EFSRegex.TypeRegex pRegEx)
        {
            ClientId = pClientId;
            AccessKey = pAccessKey;
            IsMandatory = pIsMandatory;
            IsEnabled = pIsEnabled;
            DataType = pDataType;
            ErrorMsg = string.Empty;

            Regex = pRegEx;// EFSRegex.TypeRegex.None;
            ListRetrieval = string.Empty;
            QuickClientId = string.Empty;
            QuickDataPosition = 0;

            QuickSeparator = ";";
            QuickFormat = string.Empty;
        }

        public CustomCaptureInfo(string pClientId, bool pIsMandatory, TypeData.TypeDataEnum pDataType)
            : this(pClientId, null, pIsMandatory, pDataType, true, EFSRegex.TypeRegex.None) { }
        public CustomCaptureInfo(string pClientId, bool pIsMandatory, TypeData.TypeDataEnum pDataType, bool pIsEnabled)
            : this(pClientId, null, pIsMandatory, pDataType, pIsEnabled, EFSRegex.TypeRegex.None) { }
        public CustomCaptureInfo(string pClientId, bool pIsMandatory, TypeData.TypeDataEnum pDataType, bool pIsEnabled, string pListRetrieval/*, string pRelativeTo*/)
            : this(pClientId, null, pIsMandatory, pDataType, pIsEnabled, EFSRegex.TypeRegex.None)
        {
            ListRetrieval = pListRetrieval;
        }
        public CustomCaptureInfo(string pClientId, bool pIsMandatory, TypeData.TypeDataEnum pDataType, bool pIsEnabled, EFSRegex.TypeRegex pRegEx)
            : this(pClientId, null, pIsMandatory, pDataType, pIsEnabled, pRegEx) { }
        public CustomCaptureInfo()
            : this(string.Empty, null, false, TypeData.TypeDataEnum.unknown, false, EFSRegex.TypeRegex.None) { }
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
            NewValue = pDefaultData;
            ErrorMsg = string.Empty;
            Sql_Table = null;
            LastSql_Table = null;
        }

        // EG 20160404 Migration vs2013
        
        public bool IsListRetrieval(string plistRetrieval)
        {
            return (ListRetrieval == plistRetrieval.ToLower());
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
            NewValue = pData;
            Sql_Table = pSql_Table;
            Display = pDisplay;
            //
            //Warning: On teste seulement "null" et pas "empty"
            if (null == LastValue)
            {
                LastValue = NewValue;
                LastSql_Table = Sql_Table;
            }
        }

        /// <summary>
        /// <para>
        /// Synchronise les membres last avec les membres new (LastValue = NewValue , lastSql_Table = sql_Table)
        /// </para>
        /// <para>
        /// Alimente optionnellement le membre ErrorMsg si le cci est non reseigné alors q'il est obligatoire
        /// </para>
        /// 
        /// </summary>
        /// <param name="pbSetMsgErrorOnMandatory">si true aliement le membre ErrorMsg</param>
        public void Finalize(bool pbSetMsgErrorOnMandatory)
        {
            //Reset LastValue 
            LastValue = NewValue;
            LastSql_Table = Sql_Table;
            //
            if (pbSetMsgErrorOnMandatory)
            {
                if (IsMandatory)
                {
                    string errMsgIsMandatory = Ressource.GetString("ISMANDATORY");
                    if (StrFunc.IsEmpty(NewValue))
                    {
                        if (ErrorMsg.IndexOf(errMsgIsMandatory) < 0)
                            ErrorMsg += errMsgIsMandatory;
                    }
                    else
                    {
                        ErrorMsg = ErrorMsg.Replace(errMsgIsMandatory, string.Empty);
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
            string Value = (pbNew ? NewValue : LastValue);
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
            string Value = (pbNew ? NewValue : LastValue);
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
            string Value = (pbNew ? NewValue : LastValue);
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
        /// <summary>
        /// 
        /// </summary>
        /// <param name="plMode"></param>
        /// <returns></returns>
        /// FI 20140708 [20179] Modify
        /// FI 20170116 [21916] Modify
        /// FI 20170404 [23039] Modify
        // EG 20240213 [WI756] Correctif Majuscule sur [xxx]Specified
        public object Clone(CloneMode plMode)
        {
            CustomCaptureInfo clone = new CustomCaptureInfo();

            if ((CloneMode.CciAttribut == plMode) || (CloneMode.CciAll == plMode))
            {
                clone.ClientId = ClientId;
                clone.AccessKey = AccessKey;
                clone.IsMandatory = IsMandatory;
                clone.IsMissingModeSpecified = IsMissingModeSpecified;
                clone.IsMissingMode = IsMissingMode;
                clone.DataType = DataType;
                clone.Regex = Regex;
                clone.ListRetrieval = ListRetrieval;
                clone.QuickClientId = QuickClientId;
                clone.QuickDataPosition = QuickDataPosition;
                clone.QuickSeparator = QuickSeparator;
                clone.QuickFormat = QuickFormat;
                clone.IsSystem = IsSystem; // FI 20170116 [21916] add
                clone.IsAutoCreate = IsAutoCreate; // FI 20170404 [23039] Add               
            }

            if ((CloneMode.CciData == plMode) || (CloneMode.CciAll == plMode))
            {
                clone.LastValue = LastValue;
                clone.NewValue = NewValue;
                clone.IsInputByUser = IsInputByUser;
                clone.IsLastInputByUser = IsLastInputByUser;
                clone.ErrorMsg = ErrorMsg;
                clone.Display = Display;
                //FI 20140708 [20179] add
                clone.NewValueMatch = NewValueMatch;
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
        public string ClientId
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
        #region NewValue
        [System.Xml.Serialization.XmlElementAttribute("value", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public CDATA NewValue
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
        public string ErrorMsg
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
                return ClientId.Substring(lenPrefix);
            }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public string ClientId_Prefix
        {
            get
            {
                if (3 <= ClientId.Length)
                    return ClientId.Substring(0, 3);
                else
                    return string.Empty;
            }
        }
        //
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool HasError
        {
            get { return StrFunc.IsFilled(ErrorMsg); }
        }
        #endregion property
        //
        #region constructors
        public FullCustomCaptureInfo() { }
        public FullCustomCaptureInfo(string pClientId)
        {
            ClientId = pClientId;
        }
        #endregion constructors
        //
        #region Public Methodes
        #endregion Public Methodes
        //
        #region Private Methodes
        #endregion Private Methodes
    }

}
