#region using
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Net.Mail;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using System.Xml.XPath;
using System.Xml.Xsl;
//
using EFS.ACommon;
using EFS.Actor;
using EFS.ApplicationBlocks.Data;
using EFS.Authenticate;
using EFS.Common;
using EFS.Common.EfsSend;
using EFS.Common.IO;
using EFS.Common.Log;
using EFS.Common.MQueue;
using EFS.GUI.CCI;
using EFS.Import;
using EFS.LoggerClient;
using EFS.LoggerClient.LoggerService;
using EFS.PosRequestInformation;
using EFS.PosRequestInformation.Import;
using EFS.Process;
//
using EfsML.Business;
using EfsML.DynamicData;
using EfsML.Interface;
//
using FixML.Enum;
using FixML.Interface;
//
using FpML.Interface;
#endregion

namespace EFS.SpheresIO.PosRequest
{
    /// <summary>
    /// Classe de traitement de l'importation de POSREQUEST
    /// </summary>
    internal class ProcessPosRequestImport : IOProcessImportBase
    {

        /// <summary>
        /// Représente les données à importer
        /// </summary>
        private readonly PosRequestImport _posRequestImport;

        /// <summary>
        /// Représente le POSREQUEST alimenté avec les données importées
        /// </summary>
        private PosRequestPositionInput _posRequestInput;

        #region accessor
        /// <summary>
        /// Représente le type d'importation
        /// </summary>
        public override string Key
        {
            get
            {
                return "POSREQUEST";
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// FI 20180319 [XXXXX] Add
        protected override string KeyTraceTime
        {
            get
            {
                return String.Format("ExtlLink/Mode: {0}/{1}", GetParameter(PosRequestImportCst.extlLink), _captureMode);
            }
        }
        #endregion accessor

        #region constructor
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pTradeImport">Représente les données à imorter</param>
        /// <param name="pDbTransaction">la transaction courante (null si la tâche IO est en auto-commit)</param>
        /// <param name="pTask">Représente la tâche IO</param>
        public ProcessPosRequestImport(PosRequestImport pPosRequestImport, IDbTransaction pDbTransaction, Task pTask) :
            base(pDbTransaction, pTask, pPosRequestImport.settings)
        {
            _posRequestImport = pPosRequestImport;
            _posRequestInput = new PosRequestPositionInput();
            // FI 20181025 [24279] pas la peine d'affecter la CS
            //_posRequestInput.CcisBase.CS = _task.Cs;
        }
        #endregion

        /// <summary>
        /// Contrôle la présence des données à importer
        /// </summary>
        protected override void CheckInput()
        {
            CheckPosRequestInputInput();
        }

        /// <summary>
        /// 
        /// </summary>
        protected override void SetImportCustomCaptureInfos()
        {
            _importCustomCaptureInfos = _posRequestImport.posRequestInput.CustomCaptureInfos;
        }

        /// <summary>
        /// Traite l'enregistrement (Création ou Modification ou Annulation d'un POSREQUEST)
        /// </summary>
        protected override IOCommonTools.SqlAction ProcessExecute()
        {
            IOCommonTools.SqlAction retAction = IOCommonTools.SqlAction.NA;

            CheckParameter();

            IPosRequest posRequestSource = InitPosRequestInput();

            switch (_captureMode)
            {
                case Cst.Capture.ModeEnum.New:
                case Cst.Capture.ModeEnum.Update:
                    CalcCcisImport(_task.Cs, _dbTransaction);

                    LoadCcis();

                    DumpToDocument();

                    CheckCCiErr();

                    _posRequestInput.CustomCaptureInfos.CciContainer.CleanUp();

                    IPosRequest posRequest = NewPosRequest();

                    if (IsModeUpdate)
                        posRequest.IdPR = posRequestSource.IdPR;

                    SavePosRequest(posRequest);

                    break;

                case Cst.Capture.ModeEnum.RemoveOnly:
                    //Aucune mis à jour du datadocument, il s'agit d'un simple update de la table POSREQUEST
                    //Par conséquent en mode Full le datadocument qui sera inscrit dans le log reflète l'état initial du POSREQUEST
                    posRequestSource.Qty = 0;
                    posRequestSource.QtySpecified = true;
                    SavePosRequest(posRequestSource);
                    break;

                default:
                    throw new NotImplementedException(StrFunc.AppendFormat("Mode {0} is not implemented", _captureMode.ToString()));

            }

            if (IsModeNew)
                retAction = IOCommonTools.SqlAction.I;
            else if (IsModeUpdate)
                retAction = IOCommonTools.SqlAction.U;
            else if (IsModeRemoveOnlyAll)
                retAction = IOCommonTools.SqlAction.D;

            return retAction;
        }

        /// <summary>
        /// Détermine les paramètres de l'importation
        /// </summary>
        // EG 20190114 Add detail to ProcessLog Refactoring
        protected override void LoadParameter()
        {

            
            // FI 20210415 [XXXXX] Mise en place de LogLevelEnum.Debug à la place de warning
            Logger.Log(new LoggerData(LogLevelEnum.Debug, "Loading parameters...", 4));
            IScheme scheme = Tools.GetScheme(_settings.parameter, PosRequestImportCst.extlLink);
            if (null != scheme)
                SetParameter(scheme.Scheme, ((ImportParameter)scheme).GetDataValue(_task.Cs, _dbTransaction));


            scheme = Tools.GetScheme(_settings.parameter, PosRequestImportCst.id);
            if (null != scheme)
            {
                SetParameter(scheme.Scheme, ((ImportParameter)scheme).GetDataValue(_task.Cs, _dbTransaction));
            }
            if (IsModeUpdate || IsModeRemoveOnly)
            {
                
                Logger.Log(new LoggerData(LogLevelEnum.Info, StrFunc.AppendFormat(Key + ": <b>{0}</b>", GetParameter(PosRequestImportCst.id)), 4));
            }
        }

        /// <summary>
        /// Contrôle la présence des paramètres nécessaires
        /// </summary>
        private void CheckParameter()
        {
            if ((IsModeNew) && (StrFunc.IsEmpty(GetParameter(PosRequestImportCst.instrumentIdentifier))))
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendFormat("Instrument is not specified, parameter[scheme:{0}] is mandatory", PosRequestImportCst.instrumentIdentifier);
                throw new Exception(sb.ToString());
            }


            if ((IsModeUpdate || IsModeRemoveOnly) && (StrFunc.IsEmpty(GetParameter(PosRequestImportCst.id))))
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendFormat(Key + " id not specified, parameter[scheme:{0}] is mandatory", PosRequestImportCst.id);
                throw new Exception(sb.ToString());
            }
        }

        /// <summary>
        /// <para>Initialisation des ccis en fonction des données du POSREQUEST modifié</para>
        /// <para>Préparation du document en fonction de la présence des ccis (instanciation des objects nécessaires du fait de la présence de cci)</para>
        /// </summary>
        // EG 20190114 Add detail to ProcessLog Refactoring
        private void LoadCcis()
        {
            
            Logger.Log(new LoggerData(LogLevelEnum.Info, "Loading cci collection", 4));

            try
            {
                _posRequestInput.CustomCaptureInfos = new PosRequestCustomCaptureInfos();
                // FI 20181025 [24279] Appel à InitializeCustomCaptureInfos
                _posRequestInput.InitializeCustomCaptureInfos(_task.Cs, false); 

                PosRequestCustomCaptureInfos ccis = _posRequestInput.CustomCaptureInfos;
                ccis.IsModeIO = true;
                for (int i = 0; i < ArrFunc.Count(_importCustomCaptureInfos); i++)
                    ccis.Add((CustomCaptureInfo)_importCustomCaptureInfos[i].Clone(CustomCaptureInfo.CloneMode.CciAttribut));

                //Initialisation des ccis POSREQUEST à modifier
                ccis.LoadDocument(_captureMode);

            }
            catch (Exception ex)
            {
                throw new Exception("Error on loading customCaptureInfo", ex);
            }
        }

        /// <summary>
        /// Initialise _posRequestInput
        /// <para>En mode Création, génère une nouvelles instance</para>
        /// <para>En mode Modification, suppression charge le POSREQUEST source</para>
        /// <para>Retourne le POSREQUEST source en cas de modification ou d'annulation</para>
        /// </summary>
        /// <exception cref="lorsque POSREQUEST est non trouvé en cas de modification ou annulation"></exception>
        // EG 20190114 Add detail to ProcessLog Refactoring
        private IPosRequest InitPosRequestInput()
        {
            IPosRequest ret = null;

            if (IsModeNew)
            {
                
                Logger.Log(new LoggerData(LogLevelEnum.Info, string.Format("Adding new {0}", Key), 4));
            }
            else if (IsModeUpdate || IsModeRemoveOnlyAll)
            {
                
                Logger.Log(new LoggerData(LogLevelEnum.Info, string.Format("Loading {0} <b>{1}</b>", Key, GetParameter(PosRequestImportCst.id)), 4));
            }
            else
            {
                throw new Exception(StrFunc.AppendFormat("{0} is not implemented", _captureMode.ToString()));
            }

            if (IsModeNew)
            {
                _posRequestInput = new PosRequestPositionInput();
                string instrumentIdentifier = GetParameter(PosRequestImportCst.instrumentIdentifier);
                _posRequestInput.SetInstrument(_task.Cs, SQL_TableWithID.IDType.Identifier, instrumentIdentifier);
                
                // FI 20181025 [24279] Appel à InitializeCustomCaptureInfos
                _posRequestInput.InitializeCustomCaptureInfos(_task.Cs, true);
            }
            else
            {
                int idPR = IntFunc.IntValue2(GetParameter(PosRequestImportCst.id));
                try
                {
                    _posRequestInput = new PosRequestPositionInput();
                    ret = _posRequestInput.LoadDataDocument(_task.Cs, idPR);
                    if (null == ret)
                        throw new Exception(StrFunc.AppendFormat("<b>POSREQUEST (id:{0}) not found</b>", idPR));
                    // FI 20181025 [24279] Appel à InitializeCustomCaptureInfos
                    _posRequestInput.InitializeCustomCaptureInfos(_task.Cs, true);

                }
                catch (Exception ex)
                {
                    throw new Exception(StrFunc.AppendFormat("Error on loading POSREQUEST {0} ", idPR.ToString()), ex);
                }
            }
            return ret;
        }

        /// <summary>
        /// Alimente le document avec les valeurs des ccis de l'import
        /// </summary>
        // EG 20190114 Add detail to ProcessLog Refactoring
        private void DumpToDocument()
        {

            PosRequestCustomCaptureInfos ccis = this._posRequestInput.CustomCaptureInfos;


            
            Logger.Log(new LoggerData(LogLevelEnum.Info, "Dump data (Dump cci to posRequestDocument)", 4));

            try
            {
                try
                {
                    CustomCaptureInfo cci = ccis.CciPosRequest.CciFixInstrument.Cci(CciFixInstrument.CciEnum.Exch);
                    if (null == cci)
                    {
                        throw new Exception(
                            StrFunc.AppendFormat("customCaptureInfo (cliendId:{0}) does not exist",
                            ccis.CciPosRequest.CciFixInstrument.CciClientId(CciFixInstrument.CciEnum.Exch)));
                    }

                    SetCciFromCciImport(_task.Cs, _dbTransaction, cci, false);
                    ccis.Dump_ToDocument(0);
                    cci.IsLastInputByUser = false;
                }
                catch (Exception ex)
                {
                    //TODO FI 20130325 changer le chemin ci cci?
                    throw new Exception(
                        StrFunc.AppendFormat("Eror on dump cci {0} to posRequestDocument", "Instrmt_Exch"), ex);
                }


                DumpPosRequest(CciPosRequestPosition.CciEnum.actorEntityDealer);

                DumpPosRequest(CciPosRequestPosition.CciEnum.actorDealer);
                DumpPosRequest(CciPosRequestPosition.CciEnum.bookDealer);

                DumpPosRequest(CciPosRequestPosition.CciEnum.actorClearer);
                DumpPosRequest(CciPosRequestPosition.CciEnum.bookClearer);


                //Affectation des ccis avec les données issues de l'import puis maj du doc
                for (int i = 0; i < ArrFunc.Count(ccis); i++)
                {
                    CustomCaptureInfo currentCci = ccis[i];
                    try
                    {
                        SetCciFromCciImport(_task.Cs, _dbTransaction, currentCci, false);
                        ccis.Dump_ToDocument(0);
                        // RD 20100629/FI 20100629 / Pour ne PLUS considérer ce Cci comme étant le dernier renseigné
                        currentCci.IsLastInputByUser = false;
                    }
                    catch (Exception ex)
                    {
                        throw new Exception(StrFunc.AppendFormat("Error on dump cci {0} to Datadocument", currentCci.ClientId_WithoutPrefix), ex);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error on dump ccis to Datadocument", ex);
            }
        }

        /// <summary>
        /// Contrôle la présence d'un message d'erreur sur un cci
        /// </summary>
        /// <exception cref="s'il existe au minimum un message d'erreur"></exception>
        // EG 20190114 Add detail to ProcessLog Refactoring
        private void CheckCCiErr()
        {

            
            Logger.Log(new LoggerData(LogLevelEnum.Info, "Dump data (Dump cci to posRequestDocument)", 4));

            try
            {
                _posRequestInput.CustomCaptureInfos.FinaliseAll();
                string errMsg = _posRequestInput.CustomCaptureInfos.GetErrorMessage();

                if (StrFunc.IsFilled(errMsg))
                    throw new Exception(errMsg);
            }
            catch (Exception ex)
            {
                throw new Exception("Error on checking customCaptureInfo data", ex);
            }
        }

        /// <summary>
        /// Contrôle la présence de posRequestInput et posRequestInput.CustomCaptureInfos 
        /// </summary>
        private void CheckPosRequestInputInput()
        {
            try
            {
                if (false == _posRequestImport.posRequestInputSpecified)
                    throw new Exception("<b>PosRequestInput not specified</b>");

                if (ArrFunc.IsEmpty(_posRequestImport.posRequestInput.CustomCaptureInfos))
                    throw new Exception("<b>CustomCaptureInfos not specified</b>");
            }
            catch (Exception ex)
            {
                throw new Exception("<b>Error on initialize</b>", ex);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCciEnum"></param>
        /// FI 20130925 [18990] Alimentation de l'entité s'il est est non fournie (cas qui peut être possible si assignations où le book dealer est non renseigné)
        private void DumpPosRequest(CciPosRequestPosition.CciEnum pCciEnum)
        {
            PosRequestCustomCaptureInfos ccis = _posRequestInput.CustomCaptureInfos;
            try
            {
                CustomCaptureInfo cci = ccis.CciPosRequest.Cci(pCciEnum);
                if (null == cci)
                {
                    throw new Exception(
                        StrFunc.AppendFormat("customCaptureInfo (cliendId:{0}) does not exist",
                        ccis.CciPosRequest.CciClientId(pCciEnum)));
                }
                
                /// FI 20130925 [18990] call SetSingleEntity
                if (pCciEnum == CciPosRequestPosition.CciEnum.actorEntityDealer &&
                        StrFunc.IsEmpty(cci.NewValue))
                    SetSingleEntity(CSTools.SetCacheOn(_task.Cs), cci);

                SetCciFromCciImport(_task.Cs, _dbTransaction, cci, false);
                ccis.Dump_ToDocument(0);
                cci.IsLastInputByUser = false;

            }
            catch (Exception ex)
            {
                throw new Exception(
                    StrFunc.AppendFormat("Eror on dump cci {0} to posRequestDocument", ccis.CciPosRequest.CciClientId(pCciEnum)), ex);
            }
        }

        /// <summary>
        /// Sauvegarde dans POSREQUEST 
        /// </summary>
        /// <param name="pPosRequest"></param>
        /// FI 20130917 [18953] Envoie d'un message vers SpheresClosinggen si traitement ITD
        private void SavePosRequest(IPosRequest pPosRequest)
        {
            try
            {
                // EG 20151102 [21465] Refactoring
                if (0 == pPosRequest.IdPR)
                {
                    Nullable<int> idPR = PosKeepingTools.GetExistingKeyPosRequest(_task.Cs, _dbTransaction, pPosRequest);
                    if (idPR.HasValue)
                        pPosRequest.IdPR = idPR.Value;
                }

                bool isLockSuccessFull = false;
                Lock lockRecordExisting = null;
                LockObject lockPosRequest = null;
                if (0 < pPosRequest.IdPR)
                {
                    lockPosRequest = new LockObject(TypeLockEnum.POSREQUEST, pPosRequest.IdPR, pPosRequest.RequestType.ToString(), LockTools.Exclusive);
                    Lock lck = new Lock(_task.Cs, lockPosRequest, _task.Session, _captureMode.ToString());

                    int maxLoop = 5;
                    int count = 0;
                    while ((false == isLockSuccessFull) && count < maxLoop)
                    {
                        count++;
                        isLockSuccessFull = (false == LockTools.LockMode1(lck, out lockRecordExisting));
                    }
                    if (false == isLockSuccessFull)
                    {
                        throw new SpheresException2("LockRecord", lockRecordExisting.ToString());
                    }
                }

                CheckSavePosRequest(pPosRequest);
                PrepareSavePosRequest(pPosRequest);
                // EG 20151102 [21465] Refactoring Mise en commentaire
                /*
                //RD 20140805 [20104] 
                //En mode Intraday, dans le cas où une demande similaire (même action sur la même clé de position) est en cours de traitement, 
                //Spheres® marque un temps d'attente avant de lancer la demande en cours. 
                //C'est pour éviter de se retrouver avec deux demandes similaires, en cours de traitement simultanément, par deux instances du service
                if (pPosRequest.requestMode == SettlSessIDEnum.Intraday)
                {
                    // On réessaye 10 fois avec une attente d'une seconde à chaque fois
                    int maxLoop = 10;
                    int sleepTime = 1000;

                    int count = 0;
                    bool isPendingKeyPosRequest = true;

                    while (isPendingKeyPosRequest && count < maxLoop)
                    {
                        count++;
                        Nullable<int> idPR = PosKeepingTools.GetPendingKeyPosRequest(_task.Cs, _dbTransaction, pPosRequest);
                        isPendingKeyPosRequest = idPR.HasValue;

                        // Une demande similaire est en cours de traitement => Pause.
                        if (isPendingKeyPosRequest)
                            System.Threading.Thread.Sleep(sleepTime);
                    }
                }
                */

                try
                {
                    switch (_captureMode)
                    {
                        case Cst.Capture.ModeEnum.New:
                            // EG 20151102 [21465] Refactoring
                            /*
                            if (pPosRequest.extlLinkSpecified)
                            {
                                // lorsqu'il existe un identifiant externe Spheres® n'appelle pas ma méthode PosKeepingTools.FillPosRequest
                                // qui peut générer un update s'il existe un POSREQUEST semblable
                                int idPR = 0;
                                PosKeepingTools.AddNewPosRequest(_task.Cs, _dbTransaction, out idPR, pPosRequest, _task.appInstance, null, null);
                                pPosRequest.idPR = idPR;
                            }
                            else
                            {
                                PosKeepingTools.FillPosRequest(_task.Cs, _dbTransaction, pPosRequest, _task.appInstance, null, null);
                            }
                            */
                            int idPR = 0;
                            PosKeepingTools.AddNewPosRequest(_task.Cs, _dbTransaction, out idPR, pPosRequest, _task.Session, null, null);
                            pPosRequest.IdPR = idPR;
                            break;
                        case Cst.Capture.ModeEnum.Update:
                        case Cst.Capture.ModeEnum.RemoveOnly:
                            // EG 20151019 [21112] Add _dbTransaction
                            PosKeepingTools.UpdatePosRequest(_task.Cs,_dbTransaction, pPosRequest);
                            break;
                        default:
                            throw new NotImplementedException(StrFunc.AppendFormat("Mode {0} is not implemented", _captureMode.ToString()));
                    }
                    // FI 20130917 [18953] Mode Intraday, Spheres® génère un message Queue pour traitement immediat  
                    if (pPosRequest.RequestMode == SettlSessIDEnum.Intraday)
                    {
                        PosKeepingRequestMQueue requestMQueue = PosKeepingTools.BuildPosKeepingRequestMQueue(_task.Cs, pPosRequest, null);
                        IOTools.SendMQueue(_task, requestMQueue, Cst.ProcessTypeEnum.POSKEEPREQUEST);
                    }
                }
                catch (Exception)
                {
                    this.AddLogPosRequestInputXML(pPosRequest.IdPR, Cst.ErrLevel.FAILURE);
                    throw;
                }

                this.AddLogPosRequestInputXML(pPosRequest.IdPR, Cst.ErrLevel.SUCCESS);

                LogSavePosRequest(pPosRequest);

                // EG en attente
                //if (isLockSuccessFull)
                //    LockTools.UnLock(_task.Cs, lockPosRequest, _task.appInstance.SessionId);

            }
            catch (Exception ex)
            {
                throw new Exception("<b>Error on the recording of the " + Key + ".</b> The process is aborted.", ex);
            }
        }

        /// <summary>
        ///  Insère le posRequest dans le log (attachedDoc)
        ///  <para>Le posRequest est inséré si le niveau de log est full où s'il existe une erreur </para>
        /// </summary>
        /// <param name="idPR"></param>
        /// <param name="lRet"></param>
        // EG 20190114 Add detail to ProcessLog Refactoring
        private void AddLogPosRequestInputXML(int idPR, Cst.ErrLevel lRet)
        {
            bool isError = (Cst.ErrLevel.SUCCESS != lRet);
            if (isError || _task.Process.LogDetailEnum >= LogLevelDetail.LEVEL4)
            {
                try
                {
                    string filename = string.Empty;
                    string folder = string.Empty;
                    try
                    {
                        WritePosRequestInputXMLOnTemporary(idPR, out folder, out filename);
                    }
                    catch (Exception ex)
                    {
                        throw new Exception("<b>Error on writting posRequestXML on temporary folder</b>", ex);
                    }
                    //  
                    try
                    {
                        byte[] data = FileTools.ReadFileToBytes(folder + @"\" + filename);


                        //_task.process.processLog.AddAttachedDoc(_task.Cs, data, filename, "xml");
                        LogTools.AddAttachedDoc(_task.Cs, _task.Process.IdProcess, _task.Process.Session.IdA, data, filename, "xml");
                    }
                    catch (Exception ex)
                    {
                        throw new Exception("<b>Error on writting posRequestXML in AttachedDoc table</b>", ex);
                    }
                }
                catch (Exception ex)
                {

                    // FI 20200623 [XXXXX] AddCriticalException
                    _task.Process.ProcessState.AddCriticalException(ex);

                    // FI 20200623 [XXXXX] SetErrorWarning
                    // FI 20200706 [XXXXX] Ne pas appler SetErrorWarning. Le process ne peut être en erreur parce qu'il y a un pb dans l'écriture dans le log
                    //_task.process.ProcessState.SetErrorWarning(ProcessStateTools.StatusErrorEnum);

                    Logger.Log(new LoggerData(LogLevelEnum.Error, "<b>Error on saving posRequest</b>", 4));
                    Logger.Log(new LoggerData(SpheresExceptionParser.GetSpheresException(null, ex)));
                }
            }
        }

        /// <summary>
        /// Serialize le posrequest ds le folder Temporary de l'application
        /// </summary>
        /// <param name="pFolder">Folder généré</param>
        /// <param name="pFileName">Fichier généré</param>
        private void WritePosRequestInputXMLOnTemporary(int idPR, out string pFolder, out string pFileName)
        {
            pFolder = _task.Session.MapTemporaryPath("PosRequest_xml", AppSession.AddFolderSessionId.True);
            SystemIOTools.CreateDirectory(pFolder);
            //
            pFileName = FileTools.GetUniqueName("POSREQUEST", idPR.ToString()) + ".xml";
            //
            //Serialize
            EFS_SerializeInfoBase serializeInfo = new EFS_SerializeInfoBase(_posRequestInput.DataDocument.GetType(), _posRequestInput.DataDocument);
            serializeInfo.SetPosRequestTradeInfo(_posRequestInput.DataDocument.EfsMLversion);

            StringBuilder sb = CacheSerializer.Serialize(serializeInfo, Encoding.UTF8);

            FileTools.WriteStringToFile(sb.ToString(), pFolder + @"\" + pFileName);
        }

        /// <summary>
        /// Initialisation des élement PosRequest non saisisables avant d'entreprendre la sauvegarde 
        /// </summary>
        /// <param name="pPosRequest"></param>
        /// FI 20130917 [] gestion du mode RemoveOnly
        private void PrepareSavePosRequest(IPosRequest pPosRequest)
        {
            // FI 20140401 [19804] utilisation de la property serviceName
            pPosRequest.Source = _task.AppInstance.ServiceName;
            pPosRequest.SourceSpecified = true;

            int idProcess = GetIdProcess();
            pPosRequest.SourceIdProcessLSpecified = (idProcess > 0);
            if (pPosRequest.SourceIdProcessLSpecified)
                pPosRequest.SourceIdProcessL = idProcess;

            pPosRequest.StatusSpecified = true;
            pPosRequest.Status = ProcessStateTools.StatusPendingEnum;

            string extlLink = GetParameter(PosRequestImportCst.extlLink);
            pPosRequest.ExtlLinkSpecified = StrFunc.IsFilled(extlLink);
            if (pPosRequest.ExtlLinkSpecified)
                pPosRequest.ExtlLink = extlLink;

            // EG 20151019 [21112] New
            IPosRequestDetPositionOption detail = pPosRequest.DetailBase as IPosRequestDetPositionOption;
            detail.CaptureMode = CaptureMode;

            switch (_captureMode)
            {
                case Cst.Capture.ModeEnum.New:
                    pPosRequest.IdAIns = _task.Session.IdA;
                    break;
                /// FI 20130917 [] add case RemoveOnly
                case Cst.Capture.ModeEnum.Update:
                case Cst.Capture.ModeEnum.RemoveOnly:
                    pPosRequest.IdAUpdSpecified = true;
                    pPosRequest.IdAUpd = _task.Session.IdA;
                    break;

                default:
                    throw new NotImplementedException(StrFunc.AppendFormat("Mode {0} is not implemented", _captureMode.ToString()));
            }
        }

        /// <summary>
        /// Ecrit dans le log un message en cas de succès dans le traitement d'importation
        /// </summary>
        /// <param name="pPosRequest"></param>
        private void LogSavePosRequest(IPosRequest pPosRequest)
        {
            string msg = "PosRequest correctly {0}";

            switch (_captureMode)
            {
                case Cst.Capture.ModeEnum.New:
                    msg = StrFunc.AppendFormat(msg, "created");
                    break;
                case Cst.Capture.ModeEnum.Update:
                    msg = StrFunc.AppendFormat(msg, "modified");

                    break;
                case Cst.Capture.ModeEnum.RemoveOnly:
                    msg = StrFunc.AppendFormat(msg, "removed");
                    break;
                default:
                    throw new NotImplementedException(StrFunc.AppendFormat("Mode {0} is not implemented", _captureMode.ToString()));
            }
            msg += StrFunc.AppendFormat(" (IdPR:{0})", pPosRequest.IdPR);

            Logger.Log(new LoggerData(LogLevelEnum.Info, msg, 4, new LogParam(pPosRequest.IdPR, default, "POSREQUEST", Cst.LoggerParameterLink.IDDATA)));
        }

        /// <summary>
        /// Contrôle la cohérence des données avant la sauvegarde
        /// </summary>
        /// <param name="pPosRequest"></param>
        /// FI 20130917 [18953] gestion du mode Intraday
        private void CheckSavePosRequest(IPosRequest pPosRequest)
        {
            switch (_captureMode)
            {
                case Cst.Capture.ModeEnum.New:
                case Cst.Capture.ModeEnum.Update:
                    //FI 20130917 [18953] Mise en commentaire de la génération d'une exception en mode Intraday    
                    //FI 20130405 => Seul le mode EOD est possible pour l'instant
                    //if (pPosRequest.requestMode != SettlSessIDEnum.EndOfDay)
                    //{
                    //    string msgErr = StrFunc.AppendFormat("Request Mode {0} is not implemented. Only {1} is managed", pPosRequest.requestMode.ToString(), SettlSessIDEnum.EndOfDay.ToString());
                    //    throw new Exception(StrFunc.AppendFormat("PosRequest is not valid. {0}", msgErr));
                    //}

                    if (false == pPosRequest.PosKeepingKeySpecified)
                    {
                        string msgErr = "posKeeping Key is not specified" + Cst.CrLf;
                        throw new Exception(StrFunc.AppendFormat("PosRequest is not valid. {0}", msgErr));
                    }
                    else
                    {
                        string msgErr = string.Empty;
                        IPosKeepingKey key = pPosRequest.PosKeepingKey;
                        if (key.IdAsset == 0)
                            msgErr += "Asset is not specified" + Cst.CrLf;

                        if (key.IdA_EntityDealer == 0)
                            msgErr += "Entity is not specified" + Cst.CrLf;

                        if (key.IdA_Clearer == 0)
                            msgErr += "Clearer actor is not specified" + Cst.CrLf;

                        if (key.IdB_Clearer == 0)
                            msgErr += "Clearer book is not specified" + Cst.CrLf;

                        switch (pPosRequest.RequestType)
                        {
                            case Cst.PosRequestTypeEnum.OptionExercise:
                                if (key.IdA_Dealer == 0)
                                    msgErr += "Dealer actor is not specified" + Cst.CrLf;
                                if (key.IdB_Dealer == 0)
                                    msgErr += "Dealer book is not specified" + Cst.CrLf;
                                break;
                            default:
                                // NOTHING
                                break;
                        }

                        if (StrFunc.IsFilled(msgErr))
                            throw new Exception(StrFunc.AppendFormat("PosRequest is not valid. {0}", msgErr));
                    }
                    break;

                case Cst.Capture.ModeEnum.RemoveOnly:
                    // NOTHING
                    break;

                default:
                    throw new NotImplementedException(StrFunc.AppendFormat("Mode {0} is not implemented", _captureMode.ToString()));
            }
        }

        /// <summary>
        ///  Retourne une nouvelle instance de PosRequest à partir du dataDocument 
        /// </summary>
        /// <returns></returns>
        private IPosRequest NewPosRequest()
        {
            try
            {
                IPosRequest ret = _posRequestInput.NewPostRequest(CSTools.SetCacheOn(_task.Cs));
                return ret;
            }
            catch (Exception ex)
            {
                this.AddLogPosRequestInputXML(0, Cst.ErrLevel.FAILURE);
                throw new Exception("<b>Error on building posRequest from document</b>", ex);
            }
        }
        /// <summary>
        /// Alimente {pCci}.NewValue avec l'unique entité présente ds Spheres® 
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pCci"></param>
        ///FI 20130925 [18990] add method 
        private static void SetSingleEntity(string pCS, CustomCaptureInfo pCci)
        {
            Nullable<int> idA = Actor.ActorTools.GetSingleActorEntity(pCS);
            if (idA.HasValue)
            {
                SQL_Actor sqlActor = new SQL_Actor(pCS, idA.Value);
                sqlActor.LoadTable(new string[] { "IDENTIFIER" });
                pCci.NewValue = sqlActor.Identifier;
            }
        }
    }
}
