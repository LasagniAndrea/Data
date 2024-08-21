using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using ThreadingTasks = System.Threading.Tasks;

using System.Data;
using System.Globalization;
using System.Text;
using System.Xml.Serialization;
//
using EFS.ACommon;
using EFS.Actor;
using EFS.ApplicationBlocks.Data;
using EFS.Common;
using EFS.Common.Log;
using EFS.Common.IO;
using EFS.GUI.CCI;
using EFS.Import;
using EFS.LoggerClient;
using EFS.LoggerClient.LoggerService;
using EFS.Process;
//
using EfsML.Business;
using EfsML.DynamicData;
//
using FpML.Interface;

namespace EFS.SpheresIO
{
    /// <summary>
    /// Classe de base pour un process d'importation 
    /// Importation des Trades, des debtSecurity, et des factures, des POSREQUEST
    /// </summary>
    internal class IOProcessImportBase
    {
        /// <summary>
        ///  valeurs de retour pour le lock du record
        /// </summary>
        /// FI 20130528 [18662] add enum
        protected enum LockRecordReturn
        {
            /// <summary>
            /// Lock de record posé, le record peut être importé.
            /// </summary>
            LockRecordSucces,
            /// <summary>
            /// Lock de record de même type déjà posé, le record doit être ignoré.  
            /// <para>Le record est nécessairement déjà en cours d'intégration pour une autre instance de IO</para>
            /// <para>Exemple importation d'une création de trade, et l'importation de la création du trade est déjà en cours</para>
            /// </summary>
            LockRecordIgnore,
            /// <summary>
            /// Lock de record déjà posé et de type différent, le record ne peut être intégré. Spheres® génère une erreur.
            /// </summary>
            LockRecordError,
        }

        #region Members
        /// <summary>
        /// Représente la transaction courante (null si la tâche est auto-commit)
        /// </summary>
        protected IDbTransaction _dbTransaction;
        /// <summary>
        /// Représente la tâche IO d'importation 
        /// </summary>
        protected Task _task;


        /// <summary>
        /// Représente le Mode opératoire dans Spheres® (création, modification, annulation, etc...)
        /// <para> la valeur de _captureMode est fonction de action type</para>
        /// </summary>
        protected Cst.Capture.ModeEnum _captureMode;
        /// <summary>
        /// liste des paramètres de l'importation
        /// </summary>
        protected Hashtable _importParameter;
        /// <summary>
        /// Représente la configuation de l'import
        /// </summary>
        protected ImportSettings _settings;
        /// <summary>
        /// Représente la collections de cci 
        /// <para>la collection contient les informations à importer</para>
        /// </summary>
        protected CustomCaptureInfosBase _importCustomCaptureInfos;
        #endregion

        #region accessor
        /// <summary>
        /// Représente de type d'importation 
        /// </summary>
        public virtual string Key
        {
            get { return "Trade"; }
        }

        /// <summary>
        /// 
        /// </summary>
        public Cst.Capture.ModeEnum CaptureMode
        {
            get
            {
                return _captureMode;
            }
        }


        /// <summary>
        /// Obtient true en création
        /// </summary>
        public bool IsModeNew
        {
            get
            {
                return Cst.Capture.IsModeNew(_captureMode);
            }
        }

        /// <summary>
        /// Obtient true en modification
        /// </summary>
        public bool IsModeUpdate
        {
            get
            {
                return Cst.Capture.IsModeUpdate(_captureMode);
            }
        }

        /// <summary>
        /// Obtient true en annulation d'une d'un Trade de marché (RemoveOnly)
        /// </summary>
        public bool IsModeRemoveOnly
        {
            get
            {
                return Cst.Capture.IsModeRemoveOnly(_captureMode);
            }
        }

        /// <summary>
        /// Obtient true en annulation d'une Allocation (RemoveAllocation)
        /// </summary>
        public bool IsModeRemoveAllocation
        {
            get
            {
                return Cst.Capture.IsModeRemoveAllocation(_captureMode);
            }
        }

        /// <summary>
        /// Obtient true en annulation d'une Allocation ou d'un Trade de marché (RemoveOnly/RemoveAllocation)
        /// </summary>
        public bool IsModeRemoveOnlyAll
        {
            get
            {
                return Cst.Capture.IsModeRemoveOnlyAll(_captureMode);
            }
        }
        /// <summary>
        ///  Obient ou définie un header qui sera présent dans les messages du log de l'importation
        /// </summary>
        /// FI 20131122 [19233] Add property
        public string LogHeader
        {
            get;
            set;
        }
        /// <summary>
        ///  Obtient  logHeader sans tag HTML 
        /// </summary>
        /// FI 20200409 [XXXXX] Add 
        public string LogHeaderNoHTMLTag
        {
            get
            {
                return LogHeader.Replace("<b>", string.Empty).Replace("</b>", string.Empty);
            }
        }

        /// <summary>
        ///  clé pour écriture dans la trace des durée de traitement
        /// </summary>
        /// FI 20180319 [XXXXX] Modify 
        protected virtual string KeyTraceTime
        {
            get
            {
                throw new InvalidOperationException("property KeyTraceTime must be override");
            }
        }

        /// <summary>
        /// Permet la mise à jour du statut du processus appelant
        /// </summary>
        /// FI 20200706 [XXXXX] Add
        protected SetErrorWarning SetErrorWarning
        {
            private set;
            get;
        }

        #endregion accessor

        #region constructor
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pDbTransaction"></param>
        /// <param name="pTask">Représenet la tâche IO</param>
        /// <param name="pSettings">Représente la configutation de l'importation</param>
        public IOProcessImportBase(IDbTransaction pDbTransaction, Task pTask, ImportSettings pSettings)
        {
            _dbTransaction = pDbTransaction;
            _task = pTask;
            _settings = pSettings;
            _importParameter = new Hashtable();
        }
        #endregion constructor

        #region Methods
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pSetErrorWarning"></param>
        /// FI 20200706 [XXXXX] Add
        public void InitDelegate(SetErrorWarning pSetErrorWarning )
        {
            this.SetErrorWarning = pSetErrorWarning;
        }


        /// <summary>
        /// Ajoute un paramètre 
        /// </summary>
        /// <param name="pKey">clé d'accès au paramètre</param>
        /// <param name="pValue">Valeur du paramètre</param>
        public void SetParameter(string pKey, string pValue)
        {
            if (_importParameter.ContainsKey(pKey))
                _importParameter[pKey] = pValue;
            else
                _importParameter.Add(pKey, pValue);
        }

        /// <summary>
        /// Retourne la valeur d'un paramètre 
        /// <para>Retourne null si le paramètre n'existe pas</para>
        /// </summary>
        /// <param name="pParameterkey">clé d'accès au paramètre</param>
        /// <returns></returns>
        public string GetParameter(string pParameterkey)
        {
            string ret = null;
            if (null != _importParameter)
            {
                if (_importParameter.ContainsKey(pParameterkey) && (null != _importParameter[pParameterkey]))
                    ret = _importParameter[pParameterkey].ToString();
            }
            return ret;
        }

        /// <summary>
        /// Détermine le type d'action (New, Update, etc...) à partir de settings.importMode 
        /// <para>les valeurs possibles sont définies par Cst.Capture.ModeEnum</para>
        /// </summary>
        protected void SetCaptureMode()
        {

            string importMode = _settings.importMode.GetDataValue(_task.Cs, _dbTransaction);
            if (Enum.IsDefined(typeof(Cst.Capture.ModeEnum), importMode))
            {
                _captureMode = (Cst.Capture.ModeEnum)Enum.Parse(typeof(Cst.Capture.ModeEnum), importMode);
            }
            else
            {
                throw new NotImplementedException(StrFunc.AppendFormat("<b>CaptureMode {0} is not implemented</b>", importMode));
            }
        }

        /// <summary>
        /// Retourne true si les conditions sont vérifiées, dans ce cas Spheres IO traite l'importation de la ligne
        /// </summary>
        /// <returns></returns>
        /// FI 20131122 [19233] 
        /// EG 20190114 Add detail to ProcessLog Refactoring
        protected bool IsConditionOk()
        {
            bool isOk = true;
            

            try
            {
                if (_settings.conditionSpecified)
                {
                    
                    Logger.Log(new LoggerData(LogLevelEnum.Debug, "Conditions checking...", 4));

                    //FI 20131122 [19233] add logHeader
                    isOk = _settings.IsConditionOk(_task.Cs, _dbTransaction, LogHeader, out ProcessLogInfo msgNotOkCondition);
                    if (false == isOk)
                    {
                        
                        Logger.Log(LoggerConversionTools.ProcessLogInfoToLoggerData(msgNotOkCondition));
                        
                        // FI 20210212 [XXXX] Appel à SetErrorWarning
                        if (ProcessStateTools.IsStatusErrorWarning(msgNotOkCondition.status))
                            SetErrorWarning((ProcessStateTools.StatusEnum)Enum.Parse(typeof(ProcessStateTools.StatusEnum), msgNotOkCondition.status));

                    }
                }
            }
            catch (Exception ex)
            {
                FireException("<b>Error on conditions checking</b>", ex);
            }

            return isOk;
        }

        /// <summary>
        /// Retourne le user sous lequel l'importation est enregistrée
        /// <para>Le user est celui spécifié dans la configation associé à l'importation (settings) si'il est erenseigné</para>
        /// <para>Le user est celui rattaché à la tâche (issu du message queue)</para>
        /// </summary>
        /// <exception cref="Exception lorsque Spheres® ne parvient pas à déterminer un user"></exception>
        protected User GetUser()
        {
            try
            {
                int userIdA = 0;
                if (_settings.userSpecified)
                {
                    SQL_Actor sqlActor = new SQL_Actor(CSTools.SetCacheOn(_task.Cs), _settings.user, SQL_Table.RestrictEnum.No, SQL_Table.ScanDataDtEnabledEnum.Yes, null, string.Empty);
                    if (sqlActor.LoadTable(new string[] { "IDA" }))
                        userIdA = sqlActor.Id;
                    // 
                    if (userIdA == 0)
                        throw new Exception("<b>User is incorrect</b>. Please define another user from section settings.user");

                }
                else if (_task.Process.UserId > 0)
                {
                    userIdA = _task.Process.UserId;
                }

                if (userIdA == 0)
                    throw new Exception("User not defined");

                User user = LoadCacheUser(_task.Cs, userIdA);
                if (user == null)
                    throw new Exception("User is null");

                return user;
            }
            catch (Exception ex)
            {
                throw new Exception("<b>Error on Loading user</b>", ex);
            }
        }

        /// <summary>
        ///  Retourne le User rattaché à {pIdA} 
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pIdA"></param>
        /// <returns></returns>
        /// FI 20130204 [] Mise en cache de certaines ds l'importation
        private User LoadCacheUser(string pCS, int pIdA)
        {
            string key = IOProcessImportCache.KeyBuilder(pCS, pIdA.ToString());

            User user;
            if (IOProcessImportCache.cacheUser.Contains(key))
            {
                user = (User)IOProcessImportCache.cacheUser[key];
            }
            else
            {
                user = new User(pIdA, null, RoleActor.USER);
                user.SetActorIdentification(CSTools.SetCacheOn(pCS));
                user.SetUsertType(CSTools.SetCacheOn(pCS));
                user.SetActorAncestor(CSTools.SetCacheOn(pCS));
                IOProcessImportCache.cacheUser.Add(key, user);
            }
            return user;
        }

        /// <summary>
        /// Affecte le membre _importCustomCaptureInfos
        /// </summary>
        protected virtual void SetImportCustomCaptureInfos()
        {
            throw new Exception("SetImportCustomCaptureInfos must be Override");
        }

        /// <summary>
        /// Lance le process d'importation
        /// <para>Retourne l'action résultante (Nouvelle insertion, Mise à jour, etc..)</para>
        /// </summary>
        /// FI 20130528 [18662] Lock sur record
        /// FI 20180207 [XXXXX] Modification de signature (suppression paramètre out pour faciliter le multi-threading)
        public IOCommonTools.SqlAction Process()
        {
            IOCommonTools.SqlAction ret = IOCommonTools.SqlAction.NA;

            LockRecordReturn locRecordReturn = LockRecord(out LockObject lockObject);
            if (locRecordReturn == LockRecordReturn.LockRecordSucces)
            {
                try
                {
                    ret = ProcessDetail();
                }
                finally
                {
                    if (null != lockObject)
                        LockTools.UnLock(_task.Cs, lockObject, _task.Session.SessionId);
                }
            }
            return ret;
        }

        /// <summary>
        /// Contrôle la présence des données à importer
        /// </summary>
        protected virtual void CheckInput()
        {
        }

        /// <summary>
        /// Détermine les paramètres de l'importation  définie dans la partie configuration
        /// </summary>
        protected virtual void LoadParameter()
        {
        }

        /// <summary>
        ///  Traitement de l'importation
        /// <para>Retourne l'action résultante (Nouvelle insertion, Mise à jour, etc..)</para>
        /// </summary>
        protected virtual IOCommonTools.SqlAction ProcessExecute()
        {
            throw new Exception("ProcessExecute must be Override");
        }

        /// <summary>
        /// Evaluation des cci qui contiennent les données à importer
        /// <para>Pour chaque cci, alimentation de NewValue</para>
        /// </summary>
        /// FI 20180319 [XXXXX] Modify
        protected void CalcCcisImport(string pCS, IDbTransaction pDbTransaction)
        {

            try
            {
                // FI 20180319 [XXXXX] Mise en place de TraceTime 
                AppInstance.TraceManager.TraceTimeBegin("CalcCcisImport", KeyTraceTime);

                CustomCaptureInfosBase ccisImport = this._importCustomCaptureInfos;
                // FI 20211118 [XXXXX] Usage de task (Mise en parallèle des évaluations de ccis.NewValue). 
                IEnumerable<CustomCaptureInfoDynamicData> iccis = ccisImport.Cast<CustomCaptureInfoDynamicData>();
                List<ThreadingTasks.Task> tasks = new List<ThreadingTasks.Task>();
                foreach (CustomCaptureInfoDynamicData cci in iccis)
                {
                    tasks.Add(ThreadingTasks.Task.Run(() =>
                    {

                        string errCii = StrFunc.AppendFormat("Error on cci[{0}]", cci.ClientId);
                        try
                        {

                            // les différents attributs du cci à partir de la liste cci.dynamicAttribs
                            cci.SetAttribute(pCS, pDbTransaction);

                            // FI 20131213 [19337] add if
                            if (false == cci.ClientId.StartsWith("IMP"))
                                cci.ClientId = "IMP" + cci.ClientId;

                            cci.SetNewValue(pCS, pDbTransaction);

                        }
                        catch (Exception ex)
                        {
                            throw new Exception(errCii, ex);
                        }
                    }));
                }
                ThreadingTasks.Task.WaitAll(tasks.ToArray());
            }
            catch (Exception ex)
            {
                throw new Exception("Error on set newValue from dynamic Value", ex);
            }
            finally
            {
                // FI 20200409 [XXXXX] WriteCciImportTraceDebug est emplacé par WriteTradeInputCcis
                //WriteCciImportTraceDebug();
                WriteTradeInputCcis();
                // FI 20180319 [XXXXX] Mise en place de TraceTime 
                AppInstance.TraceManager.TraceTimeEnd("CalcCcisImport", KeyTraceTime);
            }
        }

        /// <summary>
        /// Affecte le Cci {pCci} de la saisie à partir du Cci de l'import 
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pDbTransaction"></param>
        /// <param name="pCci">Représente le cci de la saisie en cours</param>
        /// <param name="pCciIsPayerOrReceiver"></param>
        /// FI 20140815 [XXXXX] Modify 
        /// FI 20171103 [23326] Modify (virtual méthode)
        protected virtual void SetCciFromCciImport(string pCS, IDbTransaction pDbTransaction, CustomCaptureInfo pCci, Boolean pCciIsPayerOrReceiver)
        {
            CustomCaptureInfoDynamicData cciImport = (CustomCaptureInfoDynamicData)_importCustomCaptureInfos[pCci.ClientId_WithoutPrefix];
            if (null != cciImport)
            {
                if (pCciIsPayerOrReceiver)
                {
                    ParamData param = new ParamData("ACTOR_IDENTIFIER", cciImport.NewValue)
                    {
                        datatype = TypeData.TypeDataEnum.@string.ToString()
                    };

                    cciImport.NewValueDynamicData = new StringDynamicData
                    {
                        spheresLib = new DataSpheresLib
                        {
                            function = "GETPARTYXMLID()",
                            param = new ParamData[] { param }
                        }
                    };
                    cciImport.SetNewValue(pCS, pDbTransaction);
                }
                // FI 20171103 [23326] Appel à SetCci
                SetCci(pCci, cciImport.NewValue);
            }
        }
        /// <summary>
        /// Alimentation d'un cci de saisie {pCci} avec {pNewValue}
        /// </summary>
        /// <param name="pCci">Alimentation </param>
        /// <param name="pNewValue"></param>
        /// FI 20171103 [23326] Add
        protected static void SetCci(CustomCaptureInfo pCci, string pNewValue)
        {
            pCci.NewValueFromLiteral = pNewValue;
            pCci.IsInputByUser = true;
            pCci.IsLastInputByUser = true;
            // FI 20140815 [XXXXX] Alimentation de accessKey 
            // => (si accessKey est renseigné cela signifie que le cci de la saisie a déjà été valorisé avec la donnée présente ds le cci de l'import)
            pCci.AccessKey = "Initialized by cciImport";

        }

        /// <summary>
        /// Retourne IdProcessL en cours
        /// </summary>
        /// <returns></returns>
        protected int GetIdProcess()
        {
            
            //return _task.process.processLog.header.IdProcess;
            return _task.Process.IdProcess;
        }


        /// <summary>
        /// Ecrit dans AttachedDoc les ccis de l'importation afin de vérifier les valeurs obtenues après évaluation (évaluation consiste à alimenter NewValue)
        /// <para>Cette méthode remplace WriteCciImportTraceDebug qui n'était appelée que lorsque IsTraceDebug</para>
        /// </summary>
        /// FI 20200409 [XXXXX] Add Method
        private void WriteTradeInputCcis()
        {
            if (_task.Process.LogDetailEnum >= LogLevelDetail.LEVEL4)
            {
                IOProcessImportCustomCaptureInfos ccis = new IOProcessImportCustomCaptureInfos()
                {
                    ccis = this._importCustomCaptureInfos
                };

                foreach (CustomCaptureInfo item in ccis.ccis)
                    item.ClientId = item.ClientId_WithoutPrefix;

                string folder = _task.Session.MapTemporaryPath("InputCcis_xml", AppSession.AddFolderSessionId.True);
                SystemIOTools.CreateDirectory(folder);

                string fileName = FileTools.ReplaceFilenameInvalidChar(LogHeaderNoHTMLTag + "_InputCcis");
                string fileXml = fileName + ".xml";
                try
                {

                    XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
                    ns.Add(string.Empty, string.Empty);
                    XmlSerializer xmlSerializer = new XmlSerializer(ccis.GetType(), new XmlRootAttribute("InputCcis"));
                    StringBuilder sb = new StringBuilder();
                    xmlSerializer.Serialize(new StringWriterWithEncoding(sb, Encoding.UTF8), ccis, ns);
                    string xmlStream = sb.ToString();

                    //Write xml
                    System.Xml.XmlDocument doc = new System.Xml.XmlDocument();
                    using (System.Xml.XmlWriter xmlWritter = System.Xml.XmlTextWriter.Create(folder + @"\" + fileXml,
                        new System.Xml.XmlWriterSettings() { Indent = true }))
                    {
                        doc.LoadXml(xmlStream);
                        doc.Save(xmlWritter);
                        xmlWritter.Close();
                    }
                }
                catch (Exception ex)
                {
                    FireException("<b>Error on writting InputCcis on temporary folder</b>.", ex);
                }

                try
                {
                    byte[] data = FileTools.ReadFileToBytes(folder + @"\" + fileXml);
                    LogTools.AddAttachedDoc(_task.Cs, _task.Process.IdProcess, _task.Process.Session.IdA, data, fileXml, "xml");
                }
                catch (Exception ex)
                {
                    FireException("<b>Error on writting InputCcis in AttachedDoc table</b>.", ex);
                }

                foreach (CustomCaptureInfo item in ccis.ccis)
                    item.ClientId = "IMP" + item.ClientId;
            }
        }

        /// <summary>
        /// Mise en place d'un lock de manière à ce que l'enregistrement en entrée soit traité exclusivement par une seule instance de SpheresIO
        /// <para>Cette méthode doit être overridé. Par défaut aucun lock n'est posé</para>
        /// <param name="pLockObject">Retourne l'objet locké</param>
        /// </summary>
        /// FI 20130528 [18662] 
        protected virtual LockRecordReturn LockRecord(out LockObject pLockObject)
        {
            pLockObject = null;
            return LockRecordReturn.LockRecordSucces;
        }

        /// <summary>
        ///  Retourne la valeur de l'énumration ActionTypeEnum qui correspond  à _settings.importMode.actionType
        ///  <para>Retourne null si _settings.importMode.actionType non renseigné</para>
        /// </summary>
        /// <returns></returns>
        /// FI 20130528 [18662] ParseActionType
        protected Nullable<ActionTypeEnum> GetActionType()
        {
            Nullable<ActionTypeEnum> ret = null;
            if (_settings.importMode.actionTypeSpecified)
            {
                string actionType = _settings.importMode.actionType;
                if (Enum.IsDefined(typeof(ActionTypeEnum), actionType))
                {
                    ret = (ActionTypeEnum)Enum.Parse(typeof(ActionTypeEnum), actionType);
                }
                else
                {
                    FireException(new NotImplementedException(StrFunc.AppendFormat("<b>Action Type {0} is not implemented</b>", actionType)));
                }
            }
            return ret;
        }


        /// <summary>
        /// Lance le process d'importation
        /// <para>Retourne l'action résultante (Nouvelle insertion, Mise à jour, etc..)</para>
        /// <para>>Retourne true s'il existe au moins une condition non respectée dont le statut est StatusEnum.ERROR</para>
        /// </summary>
        /// FI 20180207 [XXXXX] Modification de signature (suppression paramètre out pour faciliter le multi-threading)
        // EG 20190114 Add detail to ProcessLog Refactoring
        private IOCommonTools.SqlAction ProcessDetail()
        {
            SetCaptureMode();

            if (IsModeNew)
            {
                
                Logger.Log(new LoggerData(LogLevelEnum.Debug, "Process of insertion of a new " + Key + "...", 4));
            }
            else if (IsModeUpdate)
            {
                
                Logger.Log(new LoggerData(LogLevelEnum.Debug, "Process of modification of an existing " + Key + "...", 4));
            }
            else if (IsModeRemoveOnlyAll)
            {
                
                Logger.Log(new LoggerData(LogLevelEnum.Debug, "Process of cancellation of an existing " + Key + "...", 4));
            }
            else
                throw new NotImplementedException(StrFunc.AppendFormat("<b>CaptureMode {0} is not implemented</b>", _captureMode.ToString()));

            CheckInput();

            SetImportCustomCaptureInfos();

            LoadParameter();

            //FI 20120124 [18363] Désormais les conditions sont exploitées avant le contrôle des parameters 
            //(le contrôle des parameters est effectué dans la méthode Voir méthode CheckParameter)
            //=> Cela offre plus de flexibilité
            //Exemple en cas d'annulation 
            //Cela permet de ne pas tenter d'annuler un trade qui n'existe pas et d'afficher un simple warning ds ce cas 
            //le traitement est en warning ou en erreur en fonction du statut associé à la condition
            //Avec l'ancien comportement, Spheres® était obligatoirement en erreur puisque le parameter identifier est non renseigné
            //
            //Ce besoin a été identifié dans le cadre de la gateway bcs ou la suppression peut être demandée alors que le trade n'existe pas
            //=> si on démarre une gateway BCS à 16h00 alors que le trade a subit un split auparavant, alors Il est demandé à IO ds supprimer un trade qui n'existe pas
            //=> Il ne faut être en erreur dans ce cas.
            IOCommonTools.SqlAction retAction = IOCommonTools.SqlAction.NA;

            if (IsConditionOk())
                retAction = ProcessExecute();

            return retAction;
        }

        /// <summary>
        /// 
        /// Déclenche (throw) l'exception {pEx}
        /// </summary>
        /// <param name="pEx"></param>
        internal void FireException(Exception pEx)
        {
            throw pEx;
        }
        /// <summary>
        ///  Générère une nouvelle exception (de type Exception) et la déclenche
        /// </summary>
        /// <param name="pMessage"></param>
        /// <param name="pInnerException"></param>
        internal void FireException(string pMessage)
        {
            FireException(pMessage, null);
        }

        /// <summary>
        ///  Générère une nouvelle excepion (de type Exception) et la déclenche
        /// </summary>
        /// <param name="pMessage"></param>
        /// <param name="pInnerException"></param>
        internal void FireException(string pMessage, Exception pInnerException)
        {
            if (null != pInnerException)
                FireException(new Exception(pMessage, pInnerException));
            else
                FireException(new Exception(pMessage));
        }
        #endregion Methods
    }

    /// <summary>
    /// Classe qui gère les données stockées dans un cache 
    /// </summary>
    /// FI 20130204 [] Mise en cache de certaines donnée ds l'importation
    internal class IOProcessImportCache
    {
        /// <summary>
        /// 
        /// </summary>
        internal static Hashtable cacheUser = new Hashtable();

        /// <summary>
        /// 
        /// </summary>
        internal static Hashtable cacheInputUser = new Hashtable();

        /// <summary>
        /// Supprime tous les éléments du cache
        /// </summary>
        internal static void Clear()
        {
            cacheUser.Clear();
            cacheInputUser.Clear();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pcs"></param>
        /// <param name="pKey"></param>
        /// <returns></returns>
        public static string KeyBuilder(string pCs, string pKey)
        {
            return pCs + pKey;
        }
    }


    /// <summary>
    /// 
    /// </summary>
    /// FI 20200409 [XXXXX] Add 
    public class IOProcessImportCustomCaptureInfos
    {
        #region Members
        [System.Xml.Serialization.XmlArray("customCaptureInfos")]
        [System.Xml.Serialization.XmlArrayItem("customCaptureInfo", typeof(CustomCaptureInfoDynamicData))]
        public CustomCaptureInfosBase ccis;
        #endregion
    }
}
