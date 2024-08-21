#region using
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Messaging;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using System.Xml.XPath;
using System.Xml.Xsl;

//
using EFS.ACommon;
using EFS.ApplicationBlocks.Data;
using EFS.Common;
using EFS.Common.IO;
using EFS.Common.Log;
using EFS.Common.MQueue;
using EFS.LoggerClient;
using EFS.LoggerClient.LoggerService;
using EFS.Process;
using static EFS.ACommon.Cst;
#endregion


namespace EFS.SpheresIO
{
    /// <summary>
    /// 
    /// </summary>
    public class ProcessInOut 
    {
        #region Members
        #region Constantes
        protected const int DEFAULT_COUNTCOLUMNBYSTEP = 180000;
        protected const int DEFAULT_COUNTROWBYSTEP = 10000;
        //
        protected const string IOTASK = "IOTASK";
        protected const string IOTASKDET = "IOTASKDET";
        protected const string IOINPUT = "IOINPUT";
        protected const string IOOUTPUT = "IOOUTPUT";
        protected const string FILE = "FILE";
        #endregion
        //
        protected Task m_Task;
        protected bool m_IsIn;
        //
        protected bool m_IsHeterogenousSource;
        //
        protected Cst.IOSerializeMode m_SerializeMode;
        protected IOXmlOverrides m_SerializeOverrides;
        //
        protected int m_IdIoTaskDet;
        protected string m_IdIOInOut;
        protected ArrayList m_MsgLogInOut;
        //
        protected IOTask m_TaskPostMapping;
        //
        protected string m_DataStyle;
        protected string m_DataConnection;
        protected string m_DataName;
        // RD 20100111 [16818] MS Excel® file import
        protected bool m_IsVertical;
        protected string m_DataSectionStart;
        protected string m_DataSectionEnd;
        protected string m_XslMapping;
        protected int m_VolumetryIndex;
        protected Cst.IOReturnCodeEnum m_RetCodeOnNoData;
        protected Cst.IOReturnCodeEnum m_RetCodeOnNoDataModif;
        //
        protected Cst.CommitMode m_CommitMode;
        protected int m_NbOmitRowStart;
        protected int m_NbOmitRowEnd;
        //
        protected string m_WriteMode;
        protected int m_IsHeaderColumn;
        protected string m_Out_dataStyle;
        protected string m_Out_dataConnection;
        protected string m_Out_dataName;
        protected string m_DataTargetDescription;
        //
        protected int m_CountRowForPass;
        #region Objets volumineux pour le Mapping
        protected XmlSerializer m_SerializerIOTask;
        protected XmlSerializer m_SerializerIOTaskDet;
        protected XmlSerializer m_SerializerIOTaskDetInOut;
        protected XmlSerializer m_SerializerIOTaskDetInOutFile;
        //
        protected XmlDocument m_XmlForTransform;
        protected XslCompiledTransform m_XslForTransform;
        protected XsltSettings m_SettingsForTransform;
        //
        protected StringBuilder m_SbXMLPostParsing;
        //
        protected XmlDocument m_XmlDocPostMapping;
        #endregion
        
        /// <summary>
        ///  transaction courante
        /// </summary>
        protected IDbTransaction m_DbTransaction;
        
        /// <summary>
        ///  connexion courante (ouverte dès que mode est différent de autocommit)
        /// </summary>
        protected IDbConnection m_DbConnection;
        
        /// <summary>
        /// 
        /// </summary>
        protected bool m_IsWritingError;

        // PM 20200102 [XXXXX] New Log : ajout m_ElementStatus
        // FI 20200706 [XXXXX] Mise en commentaire (remplacé par ProcessState)
        //protected ProcessStateTools.StatusEnum m_ElementStatus;

        //
        // test MF
        //byte[] m_XmlStringToArray;
        XmlWriterSettings m_WriterSett;
        //XmlReaderSettings m_ReaderSett;
        // test MF
        #region IOTrack
        protected string m_IOTrackMessageValue;
        protected Nullable<int> m_IOTrackIdDataSource;
        protected string m_IOTrackDataSourceIdent;
        protected string[] m_IOTrackDatas;
        protected string[] m_IOTrackDatasIdent;
        protected string m_IOTrackStatus;
        protected Nullable<int> m_IOTrackIdDataTarget;
        protected string m_IOTrackDataTargetIdent;
        protected string m_IOTrackDescription;
        protected string m_IOTrackLotxt;
        //
        protected string m_IOTrackFileMsg;
        protected bool m_IsIOTrackWritten;
        #endregion
        // EG 20160404 Migration vs2013
#if DEBUG
        //bool m_IsDebug = false;
        readonly bool m_IsDebugDeadlock = false;
#endif
        #endregion Members

        #region Accessors
        protected IOTask TaskPostParsing
        {
            get { return m_Task.IoTask; }
            set { m_Task.IoTask = value; }
        }
        protected string XmlParFile
        {
            get { return LabelInOut + "Post" + (m_IsIn ? "Par_" : "Read_") + m_IdIOInOut.ToUpper() + ".xml"; }
        }
        protected string XmlMapFile
        {
            get { return LabelInOut + "PostMap_" + m_IdIOInOut.ToUpper() + ".xml"; }
        }
        /// <summary>
        /// <para>Représente les données source</para>
        /// </summary>
        public string DataName
        {
            get { return m_DataName; }
            set { m_DataName = value; }
        }
        /// <summary>
        /// 
        /// </summary>
        protected virtual bool IsIOXmlOverridesExtended
        {
            get { return false; }
        }
        protected virtual string ElementIdentifiersForMessage
        {
            get { return "N/A"; }
        }
        //
        public bool IsLightSerializeMode { get { return (m_SerializeMode == Cst.IOSerializeMode.LIGHT); } }
        public string DataStyle { get { return m_DataStyle; } }
        public string DataConnection { get { return m_DataConnection; } }
        public string IdIOInOut { get { return m_IdIOInOut; } }
        public string LabelInOut { get { return (m_IsIn ? "Input" : "Output"); } }
        public Task Task { get { return m_Task; } }
        public ArrayList MsgLog { get { return m_MsgLogInOut; } }
        //
        public IOXmlOverrides SerializeOverrides
        {
            get { return m_SerializeOverrides; }
        }




        /// <summary>
        /// 
        /// </summary>
        /// PM 20200102 [XXXXX] New Log
        /// FI 20200706 [XXXXX] Add (Remplace ElementStatus)
        public ProcessState ProcessState
        {
            get;
            private set;
        }

        #endregion

        #region Constructor
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pTask"></param>
        /// <param name="pIdIoTaskDet"></param>
        /// <param name="pIdInOut"></param>
        /// <param name="pIsInOut"></param>
        /// <param name="pRetCodeOnNoData"></param>
        /// <param name="pRetCodeOnNoDataModif"></param>
        public ProcessInOut(Task pTask, int pIdIoTaskDet, string pIdInOut, bool pIsInOut,
            Cst.IOReturnCodeEnum pRetCodeOnNoData, Cst.IOReturnCodeEnum pRetCodeOnNoDataModif)
        {
            m_IdIoTaskDet = pIdIoTaskDet;
            m_IdIOInOut = pIdInOut;
            m_IsIn = pIsInOut;
            m_Task = pTask;
            m_RetCodeOnNoData = pRetCodeOnNoData;
            m_RetCodeOnNoDataModif = pRetCodeOnNoDataModif;
            // FI 20200706 [XXXXX] use ProcessState
            //m_ElementStatus = ProcessStateTools.StatusEnum.SUCCESS;
            ProcessState = new ProcessState(ProcessStateTools.StatusSuccessEnum);
        }
        #endregion Constructor

        #region methods
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        /// FI 20170116 [21916] Modify
        /// EG 20190114 Add detail to ProcessLog Refactoring
        /// EG 20220221 [XXXXX] Gestion IRQ
        public Cst.ErrLevel Process()
        {
            const int MAXSTEP = 3;
            Cst.ErrLevel ret = Cst.ErrLevel.SUCCESS;

            try
            {

                Prepare();

                
                LogLevelEnum processLogLevelEnum = Logger.CurrentScope.LogLevel;
                if ((GetLogLevel() >= LogLevelDetail.LEVEL4) && (processLogLevelEnum > LogLevelEnum.Debug))
                {
                    Logger.CurrentScope.SetLogLevel(LogLevelEnum.Debug);
                }

                Cst.InputSourceDataStyle inputSourceDataStyle = Cst.InputSourceDataStyle.ANSIFILE;
                if (m_IsIn)
                {
                    inputSourceDataStyle = (Cst.InputSourceDataStyle)Enum.Parse(typeof(Cst.InputSourceDataStyle), m_DataStyle, true);
                }

                #region Reading/Parsing
                // FI 20131021 [17861] call IsFileDirectImport Method
                if (IsFileDirectImport(inputSourceDataStyle))
                {

                    
                    Logger.Log(new LoggerData(LogLevelEnum.None, new SysMsgCode(SysCodeEnum.LOG, 6011), 2,
                        new LogParam("1"),
                        new LogParam("1"),
                        new LogParam("Input")));
                }
                else
                {

                    
                    Logger.Log(new LoggerData(LogLevelEnum.None, new SysMsgCode(SysCodeEnum.LOG, 6011), 2,
                        new LogParam("1"),
                        new LogParam(MAXSTEP),
                        new LogParam(m_IsIn ? "Parsing" : "Reading")));
                }
                

                SetDataName(inputSourceDataStyle);
                Logger.Write();

                if (IRQTools.IsIRQRequestedWithLog(m_Task.Process, m_Task.Process.IRQNamedSystemSemaphore, ref ret))
                    return ret;
                ReadingFromSource();
                #endregion

#if DEBUG
                //*********************************************************************************************************
                // EG 20130723 Use for DEADLOCK TEST
                // EG 20160404 Migration vs2013
                if (m_IsDebugDeadlock)
                {
                    DeadLockGen deadLock = new DeadLockGen(m_Task.Cs);
                    deadLock.Generate();
                }
                //*********************************************************************************************************
# endif

                if (IRQTools.IsIRQRequestedWithLog(m_Task.Process, m_Task.Process.IRQNamedSystemSemaphore, ref ret))
                    return ret;

                if (IsFileDirectImport(inputSourceDataStyle))
                {
                    //Warning: No Mapping and no Writing
                    //Appel de PrepareWriting() pour initialisation des données nécessaires à IOTRACK
                    PrepareWriting();
                    //Appel de FinilizeWriting() pour alimentation de IOTRACK
                    if (IRQTools.IsIRQRequestedWithLog(m_Task.Process, m_Task.Process.IRQNamedSystemSemaphore, ref ret))
                        return ret;

                    FinalizeWriting(true, false);
                }
                else
                {
                    #region Pour le Debug =>  Charger un fichier XML PostParsing spécifique  dans la classe m_TaskPostParsing
                    // FI 20170116 [21916] Utilisation du paramètre ISDEBUG
                    /* 
                     * Il faut déposer le fichier "Debug_xxx"  sous Tempoary
                    */

                    if (m_Task.m_IOMQueue.parametersSpecified &&
                        (null != m_Task.m_IOMQueue.parameters["ISDEBUG"]) &&
                        BoolFunc.IsTrue(m_Task.m_IOMQueue.parameters["ISDEBUG"].Value))
                    {
                        string fileName = m_Task.Session.MapTemporaryPath(@"Debug_" + XmlParFile, AppSession.AddFolderSessionId.False);
                        if (File.Exists(fileName))
                        {
                            try
                            {
                                object debugXMLTask = IOTools.XmlDeserialize(typeof(IOTask), fileName);
                                if (debugXMLTask != null)
                                    TaskPostParsing = (IOTask)debugXMLTask;
                            }
                            catch (Exception ex)
                            {
                                throw new Exception("Error to load: Debug_" + XmlParFile + " (for debug)" + Cst.CrLf +
                                    m_MsgLogInOut + Cst.CrLf + Cst.CrLf + ex.Message);
                            }
                        }
                    }
                    #endregion Pour le Debug

                    #region MappingAndWriting, FinalizeWriting
                    bool isOk = true;

                    bool isTaskOk = false;
                    bool isTaskDetOk = false;
                    bool isTaskDetInOutOk = false;
                    bool isTaskDetInOutFileOk = false;
                    bool isTaskDetInOutFileRowOk = false;

                    bool isPassPostMappingRowOk = false;

                    #region PrepareMapping

                    if (IRQTools.IsIRQRequestedWithLog(m_Task.Process, m_Task.Process.IRQNamedSystemSemaphore, ref ret))
                        return ret;

                    PrepareMapping();

                    #region Initialize Step
                    int countRowsProcessed = 0;
                    int countRowsToProcessForPass = 0;
                    //
                    IOTaskDetInOutFileRow[] rowsAll = (m_IsIn ? TaskPostParsing.taskDet.input.file.row : TaskPostParsing.taskDet.output.file.row);
                    IOTaskDetInOutFileRow[] rowsLevel0 = (m_IsIn ? TaskPostParsing.taskDet.input.file.rowsLevel0 : null);
                    IOTaskDetInOutFileRow[] rowsWithoutLevel0 = (m_IsIn ? TaskPostParsing.taskDet.input.file.rowsWithoutLevel0 : null);
                    //
                    IOTaskDetInOutFileRow[] rowsToProcess = rowsAll;
                    //
                    // En mode hiérarchique, on ne traite que les lignes à la racine avec level <=1 et level !=0
                    // En mode héterogène, on ne considère toutes les lignes sans distinction de level 
                    if ((false == m_IsHeterogenousSource) && ArrFunc.IsFilled(rowsLevel0))
                        rowsToProcess = rowsWithoutLevel0;
                    //
                    int countRowsAll = ArrFunc.Count(rowsAll);
                    int countRowsToProcess = ArrFunc.Count(rowsToProcess);
                    int countRowsLevel0 = ArrFunc.Count(rowsLevel0);
                    // 
                    // passNumber = 0 ==> Traitement du fichier en une seule passe, donc sans découpage:
                    // - Si le nombre de ligne du fichier est trop petit par rapport au nombre de lignes par passe
                    // - Ou le fichier est spécifié comme étant Hétérogène sans indice de volumétrie
                    int passNumber = ((countRowsToProcess < m_CountRowForPass) || (m_IsHeterogenousSource && m_VolumetryIndex == 0) ? 0 : 1);
                    //
                    string passMessage = string.Empty;
                    string xmlMapPath = XmlMapFile;
                    #endregion

                    #region Initialiser les objets larges
                    //
                    // On instancie une seule fois m_SbXMLPostParsing si passNumber = 0 (pas de découpage du fichier)
                    // sinon m_SbXMLPostParsing sera instancié pour chaque passe
                    m_SbXMLPostParsing = (passNumber == 0 ? new StringBuilder() : null);
                    //
                    // Si le fichier est à traiter AVEC découpage ET s'il est HOMOGENE
                    // - On allou un tableau vide de dimension égale au nombre de lignes par passe, qui va être chargé par les lignes à passer à l'XSL de Mapping 
                    // Si le fichier est à traiter SANS découpage
                    // - Toutes les lignes seront passées à l'XSL de Mapping
                    // Si le fichier est HETEROGENE
                    // - Toutes les lignes seront passées à l'XSL de Mapping, en plus de nombre de lignes à traiter pour chaque passe
                    IOTaskDetInOutFileRow[] rowsToProcessForPass = ((passNumber > 0) && (false == m_IsHeterogenousSource) ? new IOTaskDetInOutFileRow[m_CountRowForPass + countRowsLevel0] : null);
                    #endregion

                    #endregion

                    if (IRQTools.IsIRQRequestedWithLog(m_Task.Process, m_Task.Process.IRQNamedSystemSemaphore, ref ret))
                        return ret;

                    PrepareWriting();

                    try
                    {
                        #region while
                        // RD 20100721 S'il y'a aucune ligne dans le flux PostParsing/PostReading, rentrer quand même une fois dans l'étape de Mapping
                        while ((countRowsProcessed < countRowsToProcess) || (countRowsToProcess == 0))
                        {
                            #region Get rows to process
                            // Le fichier est à traiter AVEC découpage, et on est à une nieme passe
                            if (passNumber > 0)
                            {
                                if (m_IsHeterogenousSource)
                                {
                                    // En fichier Hétérogène:
                                    // - on initialise juste le nombre (countRowsToProcessForPass) de lignes à traiter pour la passe en cours
                                    // - passer countRowsToProcessForPass comme paramétre à l'XSL de Mapping
                                    // - en plus de lui passer toutes les lignes
                                    if ((passNumber * m_CountRowForPass) < countRowsAll)
                                        countRowsToProcessForPass = m_CountRowForPass;
                                    else
                                        countRowsToProcessForPass = countRowsAll - ((passNumber - 1) * m_CountRowForPass);
                                }
                                else
                                {
                                    // Ajouter tous les row avec level=0, uniquement à la première passe
                                    if (passNumber == 1)
                                    {
                                        for (int i = 0; i < countRowsLevel0; i++)
                                            rowsToProcessForPass[i] = rowsLevel0[i];
                                    }
                                    //
                                    // En fichier Homogène:
                                    // - En plus d'éventuelles lignes de level 0
                                    // - on passe à l'XSL de Mapping uniquement les lignes correspondant à la passe en cours                                    
                                    GetRowToProcessForPass(rowsToProcess, passNumber, countRowsLevel0, ref rowsToProcessForPass, ref countRowsToProcessForPass);
                                    //
                                    if (m_IsIn)
                                        TaskPostParsing.taskDet.input.file.row = rowsToProcessForPass;
                                    else
                                        TaskPostParsing.taskDet.output.file.row = rowsToProcessForPass;
                                }
                            }
                            // Le fichier est à traiter en une seule passe
                            else
                            {
                                // Toutes les lignes seront traitées en une seule traite
                                countRowsToProcessForPass = countRowsAll;
                                rowsAll = null;
                            }
                            //
                            countRowsProcessed += countRowsToProcessForPass;
                            #endregion
                            //
                            isPassPostMappingRowOk = false;
                            //
                            if (passNumber > 0)
                            {
                                passMessage = countRowsProcessed.ToString() + "/" + countRowsAll.ToString();
                                
                                
                                Logger.Log(new LoggerData(LogLevelEnum.None, new SysMsgCode(SysCodeEnum.LOG, 6012), 2,
                                    new LogParam("2"),
                                    new LogParam(MAXSTEP),
                                    new LogParam("Mapping"),
                                    new LogParam(passNumber),
                                    new LogParam(passMessage)));
                            }
                            else
                            {

                                
                                Logger.Log(new LoggerData(LogLevelEnum.None, new SysMsgCode(SysCodeEnum.LOG, 6011), 2,
                                    new LogParam("2"),
                                    new LogParam(MAXSTEP),
                                    new LogParam("Mapping")));
                            }

                            
                            
                            Logger.Write();

                            if (IRQTools.IsIRQRequestedWithLog(m_Task.Process, m_Task.Process.IRQNamedSystemSemaphore, ref ret))
                                return ret;

                            Mapping(passNumber, passMessage);

                            #region Pour le Debug =>  Charger un fichier XML PostParsing spécifique  dans la classe m_TaskPostParsing
                            // FI 20170116 [21916] Utilisation du paramètre ISDEBUG
                            /*                              
                            Il faut déposer le fichier "Debug_xxx"  sous Tempoary
                            */
                            if (m_Task.m_IOMQueue.parametersSpecified &&
                                (null != m_Task.m_IOMQueue.parameters["ISDEBUG"]) &&
                                BoolFunc.IsTrue(m_Task.m_IOMQueue.parameters["ISDEBUG"].Value))
                            {
                                string fileName = m_Task.Session.MapTemporaryPath(@"\Debug_" + xmlMapPath, AppSession.AddFolderSessionId.False);
                                if (File.Exists(fileName))
                                {
                                    try
                                    {
                                        object debugXMLTask = IOTools.XmlDeserialize(typeof(IOTask), fileName);
                                        //
                                        if (debugXMLTask != null)
                                            m_TaskPostMapping = (IOTask)debugXMLTask;
                                    }
                                    catch (Exception ex)
                                    {
                                        throw new SpheresException2(MethodInfo.GetCurrentMethod().Name, "Error to load: Debug_" + xmlMapPath + " (for debug)" + Cst.CrLf +
                                            m_MsgLogInOut + Cst.CrLf + Cst.CrLf + ex.Message);
                                    }
                                }
                            }
                            #endregion Pour le Debug

                            if (IRQTools.IsIRQRequestedWithLog(m_Task.Process, m_Task.Process.IRQNamedSystemSemaphore, ref ret))
                                return ret;

                            #region Vérifier le résultat du Mapping (m_TaskPostMapping)
                            if (null != m_TaskPostMapping)
                            {
                                isTaskOk = true;
                                //
                                if (null != m_TaskPostMapping.taskDet)
                                {
                                    isTaskDetOk = true;
                                    //
                                    if (m_IsIn)
                                    {
                                        if (null != m_TaskPostMapping.taskDet.input)
                                        {
                                            isTaskDetInOutOk = true;
                                            //
                                            if (null != m_TaskPostMapping.taskDet.input.file)
                                            {
                                                isTaskDetInOutFileOk = true;
                                                //
                                                if (ArrFunc.IsFilled(m_TaskPostMapping.taskDet.input.file.row))
                                                {
                                                    isTaskDetInOutFileRowOk = true;
                                                    isPassPostMappingRowOk = true;
                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        if (null != m_TaskPostMapping.taskDet.output)
                                        {
                                            isTaskDetInOutOk = true;
                                            //
                                            if (null != m_TaskPostMapping.taskDet.output.file)
                                            {
                                                isTaskDetInOutFileOk = true;
                                                //
                                                if (ArrFunc.IsFilled(m_TaskPostMapping.taskDet.output.file.row))
                                                {
                                                    isTaskDetInOutFileRowOk = true;
                                                    isPassPostMappingRowOk = true;
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            #endregion

                            if (isPassPostMappingRowOk)
                            {
                                if (passNumber > 0)
                                {
                                    
                                    Logger.Log(new LoggerData(LogLevelEnum.None, new SysMsgCode(SysCodeEnum.LOG, 6012), 2,
                                        new LogParam("3"),
                                        new LogParam(MAXSTEP),
                                        new LogParam("Writing"),
                                        new LogParam(passNumber),
                                        new LogParam(passMessage)));
                                }
                                else
                                {
                                    
                                    Logger.Log(new LoggerData(LogLevelEnum.None, new SysMsgCode(SysCodeEnum.LOG, 6011), 2,
                                        new LogParam("3"),
                                        new LogParam(MAXSTEP),
                                        new LogParam("Writing")));
                                }

                                Logger.Write();

                                if (IRQTools.IsIRQRequestedWithLog(m_Task.Process, m_Task.Process.IRQNamedSystemSemaphore, ref ret))
                                    return ret;

                                isOk = Writing(passNumber, passMessage);
                                if (false == isOk)
                                    break;
                            }
                            else
                            {
                                if (passNumber > 0)
                                {
                                    
                                    Logger.Log(new LoggerData(LogLevelEnum.Debug,
                                        "<b>No row generated in " + LabelInOut + " PostMapping</b>" + Cst.CrLf + passMessage + Cst.CrLf +
                                        "[Xsl file: " + m_XslMapping + "]" + Cst.CrLf + ArrFunc.GetStringList(m_MsgLogInOut, Cst.CrLf), 3));
                                }
                                else
                                {
                                    // PM 20210511 [XXXXX] Afficher systématiquement l'étape 3/3 de writing même si rien à faire
                                    
                                    Logger.Log(new LoggerData(LogLevelEnum.None, new SysMsgCode(SysCodeEnum.LOG, 6011), 2,
                                        new LogParam("3"),
                                        new LogParam(MAXSTEP),
                                        new LogParam("Writing")));
                                }
                            }

                            passNumber++;

                            // RD 20100721, S'il y'a aucune ligne dans le flux PostParsing/PostReading, sortir de la boucle
                            if (countRowsToProcess == 0)
                                break;
                        }
                        #endregion while

                        if (IRQTools.IsIRQRequestedWithLog(m_Task.Process, m_Task.Process.IRQNamedSystemSemaphore, ref ret))
                            return ret;

                        #region Vérifier le résultat du Mapping (m_TaskPostMapping)
                        string postMapLogMessage1 = "Please, check if the data source is empty. If it is not empty check the parsing parameters and the XSL mapping file or contact your administrator.";
                        string postMapLogMessage2 = LabelInOut + " PostMapping</b>" + Cst.CrLf +
                            " - " + postMapLogMessage1 + Cst.CrLf +
                            "[Xsl file: " + m_XslMapping + "]" + Cst.CrLf + ArrFunc.GetStringList(m_MsgLogInOut, Cst.CrLf);
                        //
                        if (false == isTaskOk)
                        {
                            throw new Exception("<b>Error to generate " + postMapLogMessage2);
                        }
                        else if (false == isTaskDetOk)
                        {
                            throw new Exception("<b>No 'taskdet' element generated in " + postMapLogMessage2);
                        }
                        else if (false == isTaskDetInOutOk)
                        {
                            throw new Exception("<b>No " + (m_IsIn ? "'input'" : "'output'") + " element generated in " + postMapLogMessage2);
                        }
                        else if (false == isTaskDetInOutFileOk)
                        {
                            throw new Exception("<b>No 'file' element generated in " + postMapLogMessage2);
                        }
                        else if (false == isTaskDetInOutFileRowOk)
                        {
                            string messageValue = @"<b>No row generated in " + postMapLogMessage2;

                            ret = Cst.ErrLevel.SUCCESS;

                            if (Cst.IOReturnCodeEnum.SUCCESS != m_RetCodeOnNoData)
                            {
                                switch (m_RetCodeOnNoData)
                                {
                                    case Cst.IOReturnCodeEnum.ERROR:
                                        throw new Exception(messageValue);

                                    case Cst.IOReturnCodeEnum.WARNING:
                                        
                                        // FI 20200706 [XXXXX] use ProcessState
                                        //m_ElementStatus = ProcessStateTools.StatusEnum.WARNING;
                                        ProcessState.SetErrorWarning(ProcessStateTools.StatusWarningEnum);

                                        Logger.Log(new LoggerData(LogLevelEnum.Warning, messageValue, 4));
                                        break;
                                }
                            }
                            else
                            {

                                
                                Logger.Log(new LoggerData(LogLevelEnum.Debug, messageValue, 4));
                            }
                        }
                        #endregion Vérifier m_TaskPostMapping
                    }
                    catch (Exception)
                    {
                        isOk = false;
                        throw;
                    }
                    finally
                    {
                        if (ret == Cst.ErrLevel.SUCCESS)
                        {
                            ret = FinalizeWriting(isOk, isTaskDetInOutFileOk);
                        }
                        else
                        {
                            FinalizeWriting(isOk, isTaskDetInOutFileOk);
                        }

                        
                        Logger.CurrentScope.SetLogLevel(processLogLevelEnum);
                    }
                    #endregion
                }

                return ret;
            }
            catch (Exception ex)
            {
                // AL 20240709 [WI999] Attach input file
                try
                {
                    List<string> XmlData = new List<string> {
                        Cst.InputSourceDataStyle.ANSIDATA.ToString(),
                        Cst.InputSourceDataStyle.UNICODEDATA.ToString(),
                        Cst.InputSourceDataStyle.XMLDATA.ToString()
                    };

                    if (!XmlData.Contains(m_DataStyle) && File.Exists(DataName)) // The ìnput is in a dedicated and existing file => attach the file
                    {
                        string fileName = Path.GetFileName(DataName);
                        string ext = Path.GetExtension(DataName);
                        if (!string.IsNullOrWhiteSpace(ext) && ext.StartsWith("."))
                            ext = ext.Substring(1);
                        m_Task.AddLogAttachedDoc(fileName, DataName, ext);
                    }
                    else // the input is located directly in the XML message, or the expected input file doesn't exists => attach the XML message
                    {
                        string fileName = "Input_" + m_IdIOInOut + ".xml";
                        string fileNameFull = m_Task.Process.Session.MapTemporaryPath(fileName, AppSession.AddFolderSessionId.True);
                        SerializeMQInput(fileNameFull);
                        m_Task.AddLogAttachedDoc(fileName, fileNameFull);
                    }
                }
                catch (Exception innerEx)
                {
                    Logger.Log(new LoggerData(LogLevelEnum.Error,
                          "<b>Cannot attach input file</b>" + Cst.CrLf + innerEx.Message));
                }

                // FI 20190829 [XXXXX] Génération d'une exception a la place d'une SpheresException
                // Dans une SpheresException le message d'erreur est bien lourd puisqu'il comporte la stack des Appels
                // Dans la méthode ProcessTaskElement, Il y a appel à la méthode ExceptionTools.GetMessageExtended(ex) 
                throw new Exception("<b>Error to process element</b>" + Cst.CrLf + ArrFunc.GetStringList(m_MsgLogInOut, Cst.CrLf), ex);
            }
        }

        // AL 20240709 [WI999] Serialize the current MQueueBase object
        private void SerializeMQInput(string fileNameFull)
        {
            using (FileStream fs = new FileStream(fileNameFull, FileMode.CreateNew, FileAccess.ReadWrite, FileShare.None))
            {
                using (StreamWriter sw = new StreamWriter(fs))
                {                    
                    MQueueFormatter mqFormatter = new MQueueFormatter
                    {
                        isEncrypt = false
                    };

                    Message msg = new Message();
                    mqFormatter.Write(msg, m_Task.Process.MQueue);

                    using (StreamReader sr = new StreamReader(msg.BodyStream))
                    {                 
                        sw.Write(sr.ReadToEnd());
                    }
                }
            }
        }

        /// <summary>
        /// Chargement
        /// </summary>
        /// <returns></returns>
        /// RD 20110401 [17379] Serialize mode : Add column SERIALIZEMODE
        /// EG 20180426 Analyse du code Correction [CA2202]
        /// EG 20190114 Add detail to ProcessLog Refactoring
        protected void Prepare()
        {
            try
            {
                QueryParameters queryparameters =  GetInOutQuery();

                using (IDataReader drData = DataHelper.ExecuteReader(m_Task.Cs, CommandType.Text, queryparameters.Query, queryparameters.Parameters.GetArrayDbParameter()))
                {
                    if (!drData.Read())
                    {
                        Logger.Log(new LoggerData(LogLevelEnum.Error, new SysMsgCode(SysCodeEnum.SYS, 6005), 0,
                            new LogParam(m_IsIn ? Cst.IOElementType.INPUT.ToString() : Cst.IOElementType.OUTPUT.ToString()),
                            new LogParam(m_IdIOInOut + " (id:<b>" + TaskPostParsing.taskDet.id + "</b>)")));

                        throw new Exception("Task Element not found");
                    }

                    #region Préparer les données de m_IdIOInOut
                    m_DataStyle = drData["DATASTYLE"].ToString();
                    m_DataConnection = (Convert.IsDBNull(drData["DATACONNECTION"]) ? null : drData["DATACONNECTION"].ToString());
                    m_DataName = drData["DATANAME"].ToString();
                    m_DataSectionStart = (Convert.IsDBNull(drData["DATASECTIONSTART"]) ? null : drData["DATASECTIONSTART"].ToString());
                    m_DataSectionEnd = (Convert.IsDBNull(drData["DATASECTIONEND"]) ? null : drData["DATASECTIONEND"].ToString());
                    m_VolumetryIndex = (Convert.IsDBNull(drData["NBCOLUMNBYROWMAP"]) ? 0 : Convert.ToInt32(drData["NBCOLUMNBYROWMAP"]));
                    m_SerializeMode = (Cst.IOSerializeMode)Enum.Parse(typeof(Cst.IOSerializeMode), drData["SERIALIZEMODE"].ToString(), true);

                    IOTaskDetInOut detInOut = new IOTaskDetInOut
                    {
                        id = m_IdIOInOut.ToString(),
                        name = m_IdIOInOut.ToString(),
                        displayname = (Convert.IsDBNull(drData["DISPLAYNAME"]) ? null : drData["DISPLAYNAME"].ToString()),
                        loglevel = drData["LOGLEVEL"].ToString(),
                        commitmode = drData["COMMITMODE"].ToString()
                    };

                    if (m_IsIn)
                    {
                        m_XslMapping = drData["XSLMAPPING"].ToString();
                        // RD 20100111 [16818] MS Excel® file import
                        m_IsVertical = BoolFunc.IsTrue(drData["ISVERTICAL"]);
                        m_NbOmitRowStart = (Convert.IsDBNull(drData["NBOMITROWSTART"]) ? 0 : Convert.ToInt32(drData["NBOMITROWSTART"]));
                        m_NbOmitRowEnd = (Convert.IsDBNull(drData["NBOMITROWEND"]) ? 0 : Convert.ToInt32(drData["NBOMITROWEND"]));
                        m_IsHeterogenousSource = BoolFunc.IsTrue(drData["ISHETEROGENOUS"]);
                        m_DataTargetDescription = (Convert.IsDBNull(drData["DATATARGETDESC"]) ? string.Empty : drData["DATATARGETDESC"].ToString());

                        TaskPostParsing.taskDet.input = detInOut;

                        m_MsgLogInOut = new ArrayList
                        {
                            $"[Task: {TaskPostParsing.name}]",
                            $"[Input: {m_IdIOInOut}({m_DataName} [{m_DataStyle}])]"
                        };
                    }
                    else
                    {
                        if (Convert.IsDBNull(drData["XSLMAPPING"]))
                        {
                            //FI 20160804 [Migration TFS] Mofification
                            m_XslMapping = (@".\IOOutput\default-output-map.xsl");
                            
                            Logger.Log(new LoggerData(LogLevelEnum.None, new SysMsgCode(SysCodeEnum.LOG, 6007), 0, new LogParam(m_XslMapping)));
                        }
                        else
                        {
                            m_XslMapping = drData["XSLMAPPING"].ToString();
                        }

                        m_WriteMode = drData["WRITEMODE"].ToString();
                        m_IsHeaderColumn = Convert.ToInt32(drData["ISHEADERCOLUMN"]);
                        m_Out_dataStyle = drData["OUT_DATASTYLE"].ToString();
                        m_Out_dataConnection = (Convert.IsDBNull(drData["OUT_DATACONNECTION"]) ? null : drData["OUT_DATACONNECTION"].ToString());
                        m_Out_dataName = drData["OUT_DATANAME"].ToString();

                        TaskPostParsing.taskDet.output = detInOut;

                        m_MsgLogInOut = new ArrayList
                        {
                            $"[Task: {TaskPostParsing.name}]",
                            $"[Output: {m_IdIOInOut} ({m_DataName} [{m_DataStyle}][{m_Out_dataStyle}])]"
                        };
                    }

                    m_CommitMode = CheckCommitMode();
                    // Niveau de validation par défaut est: RECORDCOMMIT (Ligne)
                    if (m_CommitMode == Cst.CommitMode.INHERITED)
                        m_CommitMode = Cst.CommitMode.RECORDCOMMIT;

                    //// Niveau de détail de log par défaut est: EXPANDED (Détaillé), voir ProcessBase.IsLevelToLog()
                    //m_Task.process.LogDetailEnum = (LogLevelDetail)Enum.Parse(typeof(LogLevelDetail), CheckLogLevel(), true);
                    m_Task.Process.LogDetailEnum = GetLogLevel();
                    // PM 20200910 [XXXXX] New Log: gestion niveau de log
                    Logger.CurrentScope.SetLogLevel(LoggerConversionTools.DetailEnumToLogLevelEnum(m_Task.Process.LogDetailEnum));


                    
                    Logger.Log(new LoggerData(LogLevelEnum.None, new SysMsgCode(SysCodeEnum.LOG, 6005), 2, 
                        //new LogParam(m_Task.process.LogDetailEnum == LogLevelDetail.DEFAULT ? Ressource.GetString(LogLevelDetail.EXPANDED.ToString()) : Ressource.GetString(m_Task.process.LogDetailEnum.ToString()))));
                        new LogParam(Ressource.GetString(m_Task.Process.LogDetailEnum.ToString()))));

                    
                    Logger.Log(new LoggerData(LogLevelEnum.None, new SysMsgCode(SysCodeEnum.LOG, 6027), 2, new LogParam(Ressource.GetString($"COMMITMODE_{m_CommitMode}"))));

                    #endregion Préparer les données de m_IdIOInOut

                }

                // RD 20110401 [17379] Serialize mode : on utilise les la substitution d'attributs, uniquement en mode LIGHT
                if (Cst.IOSerializeMode.LIGHT == m_SerializeMode)
                {
                    m_SerializeOverrides = new IOXmlOverrides(IsIOXmlOverridesExtended);
                    m_SerializerIOTask = new XmlSerializer(typeof(IOTask), m_SerializeOverrides.XmlAttributeOverrides);
                }
                else
                {
                    m_SerializerIOTask = new XmlSerializer(typeof(IOTask));
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error to read datas of " + (m_IsIn ? "Input" : "Output") + " (" + m_IdIOInOut + ")", ex);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected virtual IDataReader GetInOutData()
        {
            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected virtual QueryParameters GetInOutQuery()
        {
            return null;
        }

        /// <summary>
        /// Reading/Parsing
        /// </summary>
        /// <returns></returns>
        protected virtual void ReadingFromSource()
        {

        }

        /// <summary>
        /// Retourne la collection de lignes à traiter pour la passe pPassNumber, avec sa taille
        /// </summary>
        /// <param name="pRowsToProcess">Collection de toutes les Row à traiter</param>
        /// <param name="pPassNumber">Numéro de la passe</param>
        /// <param name="pCountRowsLevel0">Taille de la collection des Row avec level = 0</param>
        /// <param name="pRowToProcessForPass">(taskRetCode) Collection des Row à traiter pour cette passe</param>
        /// <param name="pCountRowToProcessForPass">(taskRetCode) Taille de la collection des Row à traiter pour cette passe</param>
        protected void GetRowToProcessForPass(IOTaskDetInOutFileRow[] pRowsToProcess, int pPassNumber, int pCountRowsLevel0, ref IOTaskDetInOutFileRow[] pRowToProcessForPass, ref int pCountRowToProcessForPass)
        {
            if ((pPassNumber * m_CountRowForPass) < ArrFunc.Count(pRowsToProcess))
                pCountRowToProcessForPass = m_CountRowForPass; // ici on prend un paquet complet
            else
                pCountRowToProcessForPass = ArrFunc.Count(pRowsToProcess) - ((pPassNumber - 1) * m_CountRowForPass);// ici on prend ce qui reste

            // Ajouter tous les row avec level!=0, inclus dans cette passe
            for (int i = 0; i < pCountRowToProcessForPass; i++)
                pRowToProcessForPass[pCountRowsLevel0 + i] = pRowsToProcess[(pPassNumber - 1) * m_CountRowForPass + i];

            // Compléter les éventuelles row restants
            for (int i = (pCountRowsLevel0 + pCountRowToProcessForPass); i < pRowToProcessForPass.Length; i++)
                pRowToProcessForPass[i] = null;
        }

        protected void PrepareMapping()
        {
            #region Test d'existence du Fichier XSL du mapping
            if (StrFunc.IsEmpty(m_XslMapping) || m_XslMapping.ToLower().Contains("please insert"))
                throw new Exception(
                        @"<b>You forgot to specify the XSL Mapping File for " + (m_IsIn ? "input" : "output") + "</b>" + Cst.CrLf +
                         @" - Verify it, and if necessary set either UNC (Universal Naming Convention) path or a full application-server-related path.
                                (eg: \\server\share\directory\mapping_sample.xsl or S:\directory\mapping_sample.xsl)" + Cst.CrLf + ArrFunc.GetStringList(m_MsgLogInOut, Cst.CrLf));
            
            if (false == File.Exists(m_XslMapping))
            {
                string tmp_xslMapping = string.Empty;
                bool isFound;
                try
                {
                    isFound = m_Task.Process.AppInstance.SearchFile2(m_Task.Cs, m_XslMapping, ref tmp_xslMapping);
                }
                catch { isFound = false; }

                if (isFound)
                {
                    m_XslMapping = tmp_xslMapping;
                }
                else
                {
                    //On tente au cas où GetFile() soit bugguées...                        
                    m_XslMapping = m_Task.Process.AppInstance.GetFilepath(m_XslMapping);
                    isFound = File.Exists(m_XslMapping);
                }
                
                if (!isFound)
                {
                    throw new Exception(
                            @"<b>Specified XSL Mapping File does not exist.</b>
                                    - Verify it, and if necessary set either UNC (Universal Naming Convention) path or a full application-server-related path.
                                    (eg: \\server\share\directory\mapping_sample.xsl or S:\directory\mapping_sample.xsl)" + Cst.CrLf +
                             "[Xsl file : " + m_XslMapping + "]" + Cst.CrLf + ArrFunc.GetStringList(m_MsgLogInOut, Cst.CrLf));
                }
            }
            #endregion Test d'existence du Fichier XSL du mapping
            
            m_CountRowForPass = (m_VolumetryIndex > 0 ? (int)decimal.Round(DEFAULT_COUNTCOLUMNBYSTEP / m_VolumetryIndex, 0) : DEFAULT_COUNTROWBYSTEP);
            m_CountRowForPass = (m_CountRowForPass < 1 ? 1 : m_CountRowForPass);
            
            #region Initialiser les objets larges
            //
            // RD 20110401 [17379] Serialize mode
            // - on utilise les la substitution d'attributs, uniquement en mode LIGHT
            // - m_SerializerIOTask déplacé dans la méthode PrepareElement() pour pouvoir l'utiliser dans la sérialisation PostParsing
            if (Cst.IOSerializeMode.LIGHT == m_SerializeMode)
            {
                m_SerializerIOTaskDet = new XmlSerializer(typeof(IOTaskDet), m_SerializeOverrides.XmlAttributeOverrides);
                m_SerializerIOTaskDetInOut = new XmlSerializer(typeof(IOTaskDetInOut), m_SerializeOverrides.XmlAttributeOverrides);
                m_SerializerIOTaskDetInOutFile = new XmlSerializer(typeof(IOTaskDetInOutFile), m_SerializeOverrides.XmlAttributeOverrides);
            }
            else
            {
                m_SerializerIOTaskDet = new XmlSerializer(typeof(IOTaskDet));
                m_SerializerIOTaskDetInOut = new XmlSerializer(typeof(IOTaskDetInOut));
                m_SerializerIOTaskDetInOutFile = new XmlSerializer(typeof(IOTaskDetInOutFile));
            }
            
            m_XmlForTransform = new XmlDocument();
            m_SettingsForTransform = new XsltSettings(true, true);
            m_XslForTransform = new XslCompiledTransform();
            // RD 20140127 [19530] Use IOTools.XslTransformLoad method
            IOTools.XslTransformLoad(m_XslForTransform, m_XslMapping, m_SettingsForTransform, new XmlUrlResolver());
            
            m_XmlDocPostMapping = new XmlDocument();

            //// test MF
            //m_ReaderSett = new XmlReaderSettings();
            //m_ReaderSett.IgnoreComments = true;
            ////m_ReaderSett.ValidationType = ValidationType.None;
            //m_ReaderSett.CloseInput = true;
            ////
            m_WriterSett = new XmlWriterSettings
            {
                CloseOutput = true,
                //m_WriterSett.Indent = false;
                NewLineHandling = NewLineHandling.None,
                //m_WriterSett.NewLineOnAttributes = false;
                OmitXmlDeclaration = true
            };
            //// test MF
            #endregion
            // RD 20110323
            //#region Supprimer les Row vide ( null) du flux PostParsing
            //if (ArrFunc.IsFilled(m_TaskPostParsing.elementRow.input.file.row))
            //{
            //    ArrayList newRowList = new ArrayList();
            //    //
            //    foreach (IOTaskDetInOutFileRow row in m_TaskPostParsing.elementRow.input.file.row)
            //    {
            //        if (null != row)
            //            newRowList.Add(row);
            //    }
            //    //
            //    m_TaskPostParsing.elementRow.input.file.row = (IOTaskDetInOutFileRow[])newRowList.ToArray(typeof(IOTaskDetInOutFileRow));
            //}
            //#endregion
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pPassNumber"></param>
        /// <param name="pPassMessage"></param>
        /// <returns></returns>
        /// FI 20170116 [21916] Modify
        // EG 20180423 Analyse du code Correction [CA2200]
        // EG 20180426 Analyse du code Correction [CA2202]
        // EG 20190114 Add detail to ProcessLog Refactoring
        protected Cst.ErrLevel Mapping(int pPassNumber, string pPassMessage)
        {
            try
            {
                Cst.ErrLevel ret = Cst.ErrLevel.SUCCESS;
                // 
                string messageValue = string.Empty;
                //
                #region Serialiser l'instance (m_TaskPostParsing) dans un StringBuilder sbXMLPostParsing
                StringWriter swForSerialize = null;
                StringWriter swStep1 = null;
                StringBuilder sbStep1 = null;
                //
                if (pPassNumber <= 1)
                {
                    sbStep1 = new StringBuilder();
                    swStep1 = new StringWriter(sbStep1);
                }
                //                
                try
                {
                    if (pPassNumber <= 1)
                    {
                        bool isDebug = false;
                        //
                        #region Pour le Debug interne : Charger un fichier XML PostParsing dans la classe m_TaskPostParsing

                        // EG 20160404 Migration vs2013
                        // FI 20170116 [21916] Utilisation du paramètre ISDEBUG
                        if (m_Task.m_IOMQueue.parametersSpecified &&
                            (null != m_Task.m_IOMQueue.parameters["ISDEBUG"]) &&
                            BoolFunc.IsTrue(m_Task.m_IOMQueue.parameters["ISDEBUG"].Value))
                        {
                            string fileName = m_Task.Session.MapTemporaryPath(@"Debug_" + XmlParFile, AppSession.AddFolderSessionId.False);
                            if (System.IO.File.Exists(fileName))
                            {
                                try
                                {
                                    isDebug = true;
                                    XmlDocument xmlDoc = new XmlDocument();
                                    xmlDoc.Load(fileName);
                                    sbStep1 = new StringBuilder(xmlDoc.OuterXml);
                                }
                                catch (Exception ex)
                                {
                                    throw new SpheresException2(MethodInfo.GetCurrentMethod().Name, "Error to load: Debug_" + XmlParFile + " (for debug)" + Cst.CrLf +
                                        m_MsgLogInOut + Cst.CrLf + Cst.CrLf + ex.Message);
                                }
                            }
                        }

                        #endregion
                        //
                        if (false == isDebug)
                            m_SerializerIOTask.Serialize(swStep1, TaskPostParsing);
                        //
                        if (false == m_IsHeterogenousSource)
                            m_SbXMLPostParsing = new StringBuilder(sbStep1.Length * 2);
                    }
                    else if (false == m_IsHeterogenousSource)
                    {
                        swForSerialize = new StringWriter(m_SbXMLPostParsing);
                        m_SerializerIOTask.Serialize(swForSerialize, TaskPostParsing);
                    }
                    //
                    if (false == m_IsHeterogenousSource)
                    {
                        if (m_IsIn)
                            TaskPostParsing.taskDet.input.file.row = null;
                        else
                            TaskPostParsing.taskDet.output.file.row = null;
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception(
                        "<b>Error to Serialize to StringBuilder the result of " + (m_IsIn ? "parsing" : "reading") + "</b>" +
                        pPassMessage + Cst.CrLf + ArrFunc.GetStringList(m_MsgLogInOut, Cst.CrLf), ex);
                }
                finally
                {
                    #region Nettoyer
                    if (swForSerialize != null)
                    {
                        swForSerialize.Close();
                        swForSerialize.Dispose();
                        GC.SuppressFinalize(swForSerialize);
                        //
                        swForSerialize = null;
                    }
                    //
                    if (swStep1 != null)
                    {
                        swStep1.Close();
                        swStep1.Dispose();
                        GC.SuppressFinalize(swStep1);
                        //
                        swStep1 = null;
                    }
                    #endregion
                }
                #endregion Serialiser

                #region Transformation XSL dans une classe XmlDocument xmlDocPostMapping
                MemoryStream msForTransformResult = null;
                // test MF
                //MemoryStream msForTransformSource = null;
                //XmlReader xmlrForTransformSource = null;
                XmlWriter xmlwForTransformResult = null;
                // test MF
                //
                try
                {
                    #region Load data to transform
                    try
                    {
                        if (pPassNumber <= 1)
                        {
                            m_XmlForTransform.LoadXml(sbStep1.ToString().Trim());
                            // test MF
                            //m_XmlStringToArray = System.Text.Encoding.Unicode.GetBytes(sbStep1.ToString().Trim());
                            // test MF
                        }
                        else if (false == m_IsHeterogenousSource)
                        {
                            m_XmlForTransform.LoadXml(m_SbXMLPostParsing.ToString().Trim());
                            // test MF
                            //m_XmlStringToArray = System.Text.Encoding.Unicode.GetBytes(m_SbXMLPostParsing.ToString().Trim());
                            // test MF
                        }
                    }
                    catch (Exception ex)
                    {
                        throw new Exception(
                            @"<b>Error to load data to transform</b>" +
                            pPassMessage + Cst.CrLf + ArrFunc.GetStringList(m_MsgLogInOut, Cst.CrLf), ex);
                    }
                    #endregion
                    // RD 20110331 / [17097] Serialize mode : Substitu les DataType ("date" -->"d", "decimal" --> "dc", ...)
                    IOTools.XMLSetData(m_XmlForTransform, m_SerializeOverrides, true);
                    //
                    // RD 20100406 / [16931] No CData element in Output Post-Reading XML
                    #region Convert the ValueXml CDATA to xml stream
                    try
                    {
                        // RD 20100723 [17097] Input: XML Column 
                        // Enlever le CData sur des éventuelles "data" de type XML
                        // RD 20110401 / [17379] Serialize mode : En mode LIGHT, utiliser les attributs de substitution
                        if (m_IsIn)
                            IOTools.RemoveCDATAFromValueXml(m_XmlForTransform, "data", m_SerializeOverrides);
                        else
                            IOTools.RemoveCDATAFromValueXml(m_XmlForTransform, "column", m_SerializeOverrides);
                    }
                    catch (Exception ex)
                    {
                        throw new Exception(
                            @"<b>Error to Convert ValueXML CDATA to XML stream</b>" +
                            pPassMessage + Cst.CrLf + ArrFunc.GetStringList(m_MsgLogInOut, Cst.CrLf), ex);
                    }
                    #endregion
                    //
                    #region Transformer
                    try
                    {
                        XPathNavigator xmlFileNav = m_XmlForTransform.CreateNavigator();
                        //
                        XsltArgumentList args = new XsltArgumentList();
                        args.AddParam("pPassNumber", "", (pPassNumber == 0 ? 1 : pPassNumber));
                        args.AddParam("pPassRowsToProcess", "", m_CountRowForPass);
                        //
                        // test MF
                        //msForTransformSource = new MemoryStream(m_XmlStringToArray);
                        //xmlrForTransformSource = XmlReader.Create(msForTransformSource, m_ReaderSett);
                        // test MF
                        msForTransformResult = new MemoryStream();
                        // test MF
                        xmlwForTransformResult = XmlWriter.Create(msForTransformResult, m_WriterSett);
                        // test MF

#if DEBUG
                        DateTime startTrans = DateTime.Now;
                        System.Diagnostics.Debug.WriteLine(String.Format("Transformation starting DateTime : {0}", startTrans));
#endif
                        m_XslForTransform.Transform(xmlFileNav, args, xmlwForTransformResult);
                        // test MF
                        //m_XslForTransform.Transform(xmlrForTransformSource, args, xmlwForTransformResult);
                        // test MF
#if DEBUG
                        DateTime endTrans = DateTime.Now;
                        TimeSpan spanTrans = endTrans - startTrans;
                        System.Diagnostics.Debug.WriteLine(String.Format("Transformation ending DateTime : {0}", endTrans));
                        System.Diagnostics.Debug.WriteLine(String.Format("Transformation time : {0} s", spanTrans.TotalSeconds));
#endif
                        // test MF

                        //
                        msForTransformResult.Seek(0, SeekOrigin.Begin);
                        XmlTextReader xtReader = new XmlTextReader(msForTransformResult);
                        m_XmlDocPostMapping.Load(xtReader);
                    }
                    catch (Exception ex)
                    {
                        throw new Exception(
                            "<b>Error to Transform the result of " + (m_IsIn ? "Parsing" : "Reading") + "</b>" + Cst.CrLf +
                            " - Probably it's a very big volume, check Indication of volume of your " + (m_IsIn ? "Input" : "Output") +
                            pPassMessage + Cst.CrLf + "[Xsl file: " + m_XslMapping + "]" + Cst.CrLf +
                            ArrFunc.GetStringList(m_MsgLogInOut, Cst.CrLf), ex);
                    }

                    #endregion
                }
                catch (Exception) { throw; }
                finally
                {
                    #region nettoyer

                    // test MF
                    if (xmlwForTransformResult != null)
                    {
                        xmlwForTransformResult.Close();
                        xmlwForTransformResult = null;
                    }
                    // test MF

                    if (msForTransformResult != null)
                    {
                        msForTransformResult.Close();
                        msForTransformResult.Dispose();
                        GC.SuppressFinalize(msForTransformResult);
                        //
                        msForTransformResult = null;
                    }
                    //
                    if (false == m_IsHeterogenousSource)
                    {
                        //m_XmlForTransform.RemoveAll();
                        m_SbXMLPostParsing.Remove(0, m_SbXMLPostParsing.Length);
                    }

                    // test MF
                    //if (xmlrForTransformSource != null)
                    //{
                    //    xmlrForTransformSource.Close();
                    //    xmlrForTransformSource = null;
                    //}
                    //if (msForTransformSource != null)
                    //{
                    //    msForTransformSource.Close();
                    //    msForTransformSource = null;
                    //}                    
                    // test MF

                    #endregion
                }
                #endregion Transformation XSL
                // 
                #region AddPostMappingFileInLog
                try
                {
                    AddPostMappingFileInLog(pPassNumber, m_XmlDocPostMapping, m_SerializerIOTask);
                }
                catch (Exception ex)
                {
                    throw new Exception("<b>Error to add PostMapping file in log</b>" +
                        pPassMessage + Cst.CrLf + "[File: " + XmlMapFile + "]" + Cst.CrLf +
                        ArrFunc.GetStringList(m_MsgLogInOut, Cst.CrLf), ex);
                }
                #endregion
                //
                #region Désérialiser le résultat de la transformation dans une instance IOTask (m_TaskPostMapping)
                try
                {
                    #region Remove XmlDeclaration Node
                    XmlNode xmlNode = m_XmlDocPostMapping.FirstChild;
                    XmlNodeType xmlNodeType = (XmlNodeType)xmlNode.NodeType;
                    if (xmlNodeType == XmlNodeType.XmlDeclaration)
                    {
                        m_XmlDocPostMapping.RemoveChild(xmlNode);
                        xmlNode = m_XmlDocPostMapping.FirstChild;
                    }
                    #endregion
                    //
                    #region Mettre les colonnes XML dans un CDATA
                    // RD 20100407 / [16931] No CData element in Output Post-Reading XML
                    // RD 20110401 / [17379] Serialize mode : En mode LIGHT, utiliser les attributs de substitution
                    if (m_IsIn)
                        // RD 20100723 [17097] Input: XML Column 
                        // Mettre le contenu des éventuelles colonnes de type XML dans CData
                        IOTools.SetValueXmlInCDATA(m_XmlDocPostMapping, "column", m_SerializeOverrides);
                    else
                        IOTools.SetValueXmlInCDATA(m_XmlDocPostMapping, "data", m_SerializeOverrides);
                    #endregion
                    //
                    #region Désérialiser
                    // RD 20110331 / [17097] Serialize mode : Corriger les DataType avec les vrais valeurs ("d" --> "date", "dc" --> "decimal", ...)
                    IOTools.XMLSetData(m_XmlDocPostMapping, m_SerializeOverrides, false);
                    //                    
                    using (StringReader srForSerialize = new StringReader(m_XmlDocPostMapping.OuterXml))
                    {
                        string xmlNodeName = xmlNode.Name.Trim().ToUpper();
                        xmlNode = null;
                        //
                        if (xmlNodeName == IOTASK)
                        {
                            m_TaskPostMapping = (IOTask)m_SerializerIOTask.Deserialize(srForSerialize);
                        }
                        else if (xmlNodeName == IOTASKDET)
                        {
                            m_TaskPostMapping = new IOTask
                            {
                                taskDet = (IOTaskDet)m_SerializerIOTaskDet.Deserialize(srForSerialize)
                            };
                        }
                        else if (m_IsIn && (xmlNodeName == IOINPUT))
                        {
                            m_TaskPostMapping = new IOTask
                            {
                                taskDet = new IOTaskDet
                                {
                                    input = (IOTaskDetInOut)m_SerializerIOTaskDetInOut.Deserialize(srForSerialize)
                                }
                            };
                        }
                        else if (!m_IsIn && (xmlNodeName == IOOUTPUT))
                        {
                            m_TaskPostMapping = new IOTask
                            {
                                taskDet = new IOTaskDet
                                {
                                    output = (IOTaskDetInOut)m_SerializerIOTaskDetInOut.Deserialize(srForSerialize)
                                }
                            };
                        }
                        else if (m_IsIn && (xmlNodeName == FILE))
                        {
                            m_TaskPostMapping = new IOTask
                            {
                                taskDet = new IOTaskDet
                                {
                                    input = new IOTaskDetInOut
                                    {
                                        file = (IOTaskDetInOutFile)m_SerializerIOTaskDetInOutFile.Deserialize(srForSerialize)
                                    }
                                }
                            };
                        }
                        else if (!m_IsIn && (xmlNodeName == FILE))
                        {
                            m_TaskPostMapping = new IOTask
                            {
                                taskDet = new IOTaskDet
                                {
                                    output = new IOTaskDetInOut
                                    {
                                        file = (IOTaskDetInOutFile)m_SerializerIOTaskDetInOutFile.Deserialize(srForSerialize)
                                    }
                                }
                            };
                        }
                        else
                        {
                            throw new Exception(
                                "<b>Error to generate PostMapping file</b> - Check the XSL mapping file" +
                                pPassMessage + Cst.CrLf + "[File: " + XmlMapFile + "]" + Cst.CrLf +
                                ArrFunc.GetStringList(m_MsgLogInOut, Cst.CrLf));
                        }
                    }
                    m_XmlDocPostMapping.RemoveAll();
                    #endregion
                }
                catch (Exception ex)
                {
                    throw new Exception(
                        "<b>Error to Deserialize the result of mapping</b>" + Cst.CrLf +
                        " - Perhaps it's a very big volume, check Indication of volume of your Input or Output" +
                        pPassMessage + Cst.CrLf + ArrFunc.GetStringList(m_MsgLogInOut, Cst.CrLf), ex);
                }
                #endregion Désérialiser le résultat

                return ret;
            }
            catch
            {
                //Message d'info pour indiquer la présence d'erreur
                Logger.Log(new LoggerData(LogLevelEnum.Error, new SysMsgCode(SysCodeEnum.SYS, 6003), 3, new LogParam("Mapping")));
                throw;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        protected virtual void PrepareWriting()
        {
            #region IOTrack
            m_IOTrackMessageValue = string.Empty;
            m_IOTrackIdDataSource = null;
            m_IOTrackDataSourceIdent = m_DataName + " [" + m_DataStyle + "]";
            m_IOTrackDatas = null;
            m_IOTrackDatasIdent = null;
            m_IOTrackStatus = string.Empty;
            m_IOTrackIdDataTarget = null;
            m_IOTrackDataTargetIdent = string.Empty;
            m_IOTrackDescription = string.Empty;
            //
            m_IOTrackFileMsg = string.Empty;
            m_IsIOTrackWritten = false;
            #endregion
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pPassNumber"></param>
        /// <param name="pPassMessage"></param>
        /// <returns></returns>
        protected virtual bool Writing(int pPassNumber, string pPassMessage)
        {
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pIsOk"></param>
        /// <param name="pIsPostMappingRowFilled"></param>
        // EG 20190114 Add detail to ProcessLog Refactoring
        protected virtual Cst.ErrLevel FinalizeWriting(bool pIsOk, bool pIsPostMappingRowFilled)
        {
            #region IOTrack
            if (false == m_IsIOTrackWritten)
            {
                if (pIsOk && (false == m_IsWritingError))
                {
                    m_IOTrackMessageValue = "<b>Data " + (m_IsIn ? "imported" : "exported") + " successfully</b>" + Cst.CrLf + m_IOTrackFileMsg;
                    m_IOTrackStatus = Cst.ErrLevel.SUCCESS.ToString();
                }
                else
                {
                    m_IOTrackMessageValue = "<b>Warning! One or several errors occurred during data " + (m_IsIn ? "import" : "export") + "</b>" + Cst.CrLf +
                        "Refer to 'Log detail' for more information" + Cst.CrLf +
                        m_IOTrackFileMsg;
                    m_IOTrackStatus = ProcessStateTools.StatusError;
                }

                // RD 20120830 [18102] Gestion des compteurs IOTRACK
                m_Task.AddIOTrackLog(m_IOTrackMessageValue,
                    m_IOTrackIdDataSource, m_IOTrackDataSourceIdent,
                    m_IOTrackDatas, m_IOTrackDatasIdent,
                    m_IOTrackStatus,
                    m_IOTrackIdDataTarget, m_IOTrackDataTargetIdent,
                    m_IOTrackDescription,
                    m_IOTrackLotxt);
            }
            #endregion

            if (m_IsWritingError)
            {
                // FI 20200623 [XXXXX] SetErrorWarning
                // FI 20200706 [XXXXX] Mise en commentaire puisque un throw est généré
                //Task.process.ProcessState.SetErrorWarning(ProcessStateTools.StatusErrorEnum);

                //Message d'information pour avertir la présence d'erreurs
                
                // FI 20200706 [XXXXX] Mise en commentaire puisque un throw est généré
                //m_ElementStatus = ProcessStateTools.StatusEnum.ERROR;
                Logger.Log(new LoggerData(LogLevelEnum.Error, new SysMsgCode(SysCodeEnum.SYS, 6003), 3, new LogParam("Writing")));

                //Génération d'une exception pour stopper le process
                throw new Exception("Error on writing");
            }

            return Cst.ErrLevel.SUCCESS;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        // PL 20200710 New Name and new return type
        //protected string CheckLogLevel()
        protected LogLevelDetail GetLogLevel()
        {
            string parentLogLevel = IOCommonTools.CheckDefaultValue(TaskPostParsing.taskDet.loglevel, TaskPostParsing.loglevel);
            // PM 20180219 [23824] IOTools => IOCommonTools
            //string parantLogLevel = IOTools.CheckDefaultValue(m_TaskPostParsing.taskDet.loglevel, m_TaskPostParsing.loglevel);

            //if (m_IsIn)
            //    return IOTools.CheckDefaultValue(m_TaskPostParsing.taskDet.input.loglevel, parantLogLevel);
            //else
            //    return IOTools.CheckDefaultValue(m_TaskPostParsing.taskDet.output.loglevel, parantLogLevel);
            string finalLogLevel;
            if (m_IsIn)
            {
                finalLogLevel = IOCommonTools.CheckDefaultValue(TaskPostParsing.taskDet.input.loglevel, parentLogLevel);
            }
            else
            {
                finalLogLevel = IOCommonTools.CheckDefaultValue(TaskPostParsing.taskDet.output.loglevel, parentLogLevel);
            }

            LogLevelDetail ret;
            if (Enum.IsDefined(typeof(LogLevelDetail), finalLogLevel))
            {
                ret = (LogLevelDetail)Enum.Parse(typeof(LogLevelDetail), finalLogLevel, true);
            }
            else
            {
                //Pour compatibilité ascendante, si besoin... 
                switch (finalLogLevel)
                {
                    case "FULL":
                        ret = LogLevelDetail.LEVEL4;
                        break;
                    case "NONE":
                        ret = LogLevelDetail.LEVEL2; 
                        break;
                    default:
                        ret = LogLevelDetail.LEVEL3;
                        break;
                }
            }
            return ret;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected Cst.CommitMode CheckCommitMode()
        {
            // PM 20180219 [23824] IOTools => IOCommonTools
            //string parantCommitMode = IOTools.CheckDefaultValue(m_TaskPostParsing.taskDet.commitmode, m_TaskPostParsing.commitmode);

            //if (m_IsIn)
            //    return IOTools.CheckDefaultValue(m_TaskPostParsing.taskDet.input.commitmode, parantCommitMode);
            //else
            //    return IOTools.CheckDefaultValue(m_TaskPostParsing.taskDet.output.commitmode, parantCommitMode);
            string parentCommitMode = IOCommonTools.CheckDefaultValue(TaskPostParsing.taskDet.commitmode, TaskPostParsing.commitmode);
            
            string commitMode;
            if (m_IsIn)
            {
                commitMode = IOCommonTools.CheckDefaultValue(TaskPostParsing.taskDet.input.commitmode, parentCommitMode);
            }
            else
            {
                commitMode = IOCommonTools.CheckDefaultValue(TaskPostParsing.taskDet.output.commitmode, parentCommitMode);
            }

            Cst.CommitMode ret = Cst.CommitMode.RECORDCOMMIT;
            if (Enum.IsDefined(typeof(Cst.CommitMode), commitMode))
            {
                ret = (Cst.CommitMode)Enum.Parse(typeof(Cst.CommitMode), commitMode, true);
            }
            return ret;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pIsError"></param>
        // EG 20190114 Add detail to ProcessLog Refactoring
        protected void AddPostParsingFileInLog(bool pIsError)
        {
            if (pIsError || (GetLogLevel() >= LogLevelDetail.LEVEL4))
            {
                // RD 20120314 
                // Ecrire dans le log le message ci-dessous, uniquement si l'insertion du fichier XML dans AttachDoc s'est bien passée            
                Cst.ErrLevel errLevel;

                if (false == IsLightSerializeMode)
                {
                    errLevel = m_Task.AddXmlFileInLog(XmlParFile, typeof(IOTask), TaskPostParsing, null);
                }
                else
                {
                    // RD 20110401 [17379] Pour le mode Light, Substitution des DataType ("date" -->"d", "decimal" --> "dc", ...)
                    StringBuilder sb = new StringBuilder();
                    using (StringWriter sw = new StringWriter(sb))
                    {
                        m_SerializerIOTask.Serialize(sw, TaskPostParsing);
                    };

                    XmlDocument xmlForFileLog = new XmlDocument();
                    xmlForFileLog.LoadXml(sb.ToString().Trim());
                    //
                    // Substitu les DataType ("date" -->"d", "decimal" --> "dc", ...)
                    IOTools.XMLSetData(xmlForFileLog, m_SerializeOverrides, true);
                    // 
                    // Enlever le CData sur des éventuelles "data" de type XML
                    // RD 20110401 / [17379] Serialize mode : En mode LIGHT, utiliser les attributs de substitution
                    if (m_IsIn)
                        IOTools.RemoveCDATAFromValueXml(xmlForFileLog, "data", m_SerializeOverrides);
                    else
                        IOTools.RemoveCDATAFromValueXml(xmlForFileLog, "column", m_SerializeOverrides);
                    //
                    errLevel = m_Task.AddXmlFileInLog(XmlParFile, typeof(IOTask), TaskPostParsing, xmlForFileLog, null);
                }

                //Add Log
                if (errLevel == Cst.ErrLevel.SUCCESS)
                {

                    // FI 20200623 [XXXXX] SetErrorWarning if error
                    // FI 20200706 [XXXXX] Ne pas être en erreur si pb d'alimentation du fichier dans le log
                    /*
                    if (pIsError)
                        m_Task.process.ProcessState.SetErrorWarning(ProcessStateTools.StatusErrorEnum);
                    */


                    
                    if (pIsError)
                    {
                        Logger.Log(new LoggerData(LogLevelEnum.Error, new SysMsgCode(SysCodeEnum.SYS, 6002), 3,
                            new LogParam(m_IsIn ? "Parsing" : "Reading"),
                            new LogParam(XmlParFile)));
                    }
                    else
                    {
                        Logger.Log(new LoggerData(LogLevelEnum.Debug, new SysMsgCode(SysCodeEnum.LOG, 6006), 3,
                            new LogParam(m_IsIn ? "Parsing" : "Reading"),
                            new LogParam(XmlParFile)));
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pStep"></param>
        /// <param name="pDomXml"></param>
        /// <param name="pXmlSerializer"></param>
        // EG 20190114 Add detail to ProcessLog Refactoring
        protected void AddPostMappingFileInLog(int pStep, XmlDocument pDomXml, XmlSerializer pXmlSerializer)
        {
            string fileName = XmlMapFile;
            if (pStep > 0)
            {
                //PL 20100910 Add PadLeft()
                fileName = fileName.Replace(".xml", "_" + pStep.ToString().PadLeft(4, '0') + ".xml");
            }

            if (GetLogLevel() >= LogLevelDetail.LEVEL4)
            {
                // RD 20120314 
                // Ecrire dans le log le message ci-dessous, uniquement si l'insertion du fichier XML dans AttachDoc s'est bien passée            
                if (Cst.ErrLevel.SUCCESS == m_Task.AddXmlFileInLog(fileName, typeof(IOTask), TaskPostParsing, pDomXml, pXmlSerializer))
                {
                    // RD 20121105 Bug d'affichage du nom du fichier PostMapping dans le Log (du à un copier/coller)
                    // Correction: Remplacer xmlParFile par fileName
                    
                    Logger.Log(new LoggerData(LogLevelEnum.Debug, new SysMsgCode(SysCodeEnum.LOG, 6006), 3,
                        new LogParam("Mapping"),
                        new LogParam(fileName)));
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        // EG 20190114 Add detail to ProcessLog Refactoring
        public void PostParsingNoData()
        {
            string logMessage1 = "Please, check if the data source is empty. If it is not empty contact your administrator.";
            string logMessage2 = LabelInOut + " PostParsing</b>" + Cst.CrLf +
                " - " + logMessage1 + Cst.CrLf + ArrFunc.GetStringList(MsgLog, Cst.CrLf);
            // 
            string msgRetCode = "<b>No row generated in " + logMessage2;

            if (Cst.IOReturnCodeEnum.SUCCESS != m_RetCodeOnNoData)
            {
                switch (m_RetCodeOnNoData)
                {
                    case Cst.IOReturnCodeEnum.ERROR:
                        
                        ProcessState.Status = ProcessStateTools.StatusEnum.ERROR;

                        throw new Exception(msgRetCode);

                    case Cst.IOReturnCodeEnum.WARNING:
                        m_Task.Process.ProcessState.SetErrorWarning(ProcessStateTools.StatusWarningEnum);
                        
                        ProcessState.Status = ProcessStateTools.StatusEnum.WARNING;
                        Logger.Log(new LoggerData(LogLevelEnum.Warning, msgRetCode));

                        break;
                }
            }
            else
            {
                Logger.Log(new LoggerData(LogLevelEnum.Info, msgRetCode));
            }
        }

        /// <summary>
        ///  Retourne true si l'importation de type File {inputSourceDataStyle} s'effectue en dur
        ///  <para>SpheresIO n'applique pas les étapes classique de Parsing/Mapping</para>
        /// </summary>
        /// <param name="inputSourceDataStyle"></param>
        /// <returns></returns>
        protected static bool IsFileDirectImport(Cst.InputSourceDataStyle inputSourceDataStyle)
        {
            Boolean ret = false;
            // 20220325 [XXXXX] usage de InputSourceDataStyleAttribute
            InputSourceDataStyleAttribute inputSourceAttribut = ReflectionTools.GetAttribute<InputSourceDataStyleAttribute>(inputSourceDataStyle);
            if (null != inputSourceAttribut)
            {
                ret = inputSourceAttribut.IsDirectImport;
            }
            return ret;
        }

        /// <summary>
        /// Interprétation de m_DataName 
        /// </summary>
        /// <param name="inputSourceDataStyle"></param>
        /// FI 20131021 [17861] add method
        protected virtual void SetDataName(Cst.InputSourceDataStyle inputSourceDataStyle)
        {
        }
        #endregion
    }
}
