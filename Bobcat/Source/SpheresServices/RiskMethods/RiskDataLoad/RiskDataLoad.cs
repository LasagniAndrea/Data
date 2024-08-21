using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
//
using EFS.ACommon;
using EFS.ApplicationBlocks.Data;
using EFS.ApplicationBlocks.Data.Extension;
using EFS.Common;
using EFS.Common.IO;
using EFS.Common.IO.Interface;
using EFS.Common.Log;
using EFS.LoggerClient;
using EFS.LoggerClient.LoggerService;
using EFS.Process;
//
using EfsML.Enum;
//
using FixML.Enum;
using FixML.v50SP1.Enum;

namespace EFS.SpheresRiskPerformance
{
    /// <summary>
    /// Classe de base des classes de chargement et de stockage des données RiskData spécifique à chaque type de fichier
    /// </summary>
    public abstract class RiskDataLoad
    {
        #region Members
        /// <summary>
        /// Assets ETD pour lesquels charger les données
        /// </summary>
        protected MarketDataAssetETD m_DataAsset; 
        // PM 20190222 [24326] Ahout m_MethodType
        protected InitialMarginMethodEnum m_MethodType; // Nom de la méthode de calcul pour laquelle charger les données
        #endregion Members
        #region Accessors
        /// <summary>
        ///  Nom de la méthode de calcul pour laquelle charger les données
        /// </summary>
        public InitialMarginMethodEnum MethodType { get { return m_MethodType; } }
        #endregion Accessors
        #region Constructors
        /// <summary>
        /// Constructeur
        /// </summary>
        /// <param name="pMethodType">Nom de la méthode de calcul pour laquelle charger les données</param>
        /// <param name="pAsset">Assets d'un  même marché pour lesquels charger les données</param>
        /// PM 20190222 [24326] Ajout m_MethodType
        ///public RiskDataLoad(RiskDataAsset pAsset)
        /// FI 20220321 [XXXXX] pAsset de type IEnumerable<IAssetETDFields>
        public RiskDataLoad(InitialMarginMethodEnum pMethodType, IEnumerable<IAssetETDIdent> pAsset)
        {
            m_MethodType = pMethodType;
            m_DataAsset = new MarketDataAssetETD(pAsset);
        }
        #endregion Constructors
        #region Methods
        /// <summary>
        /// Lecture d'un fichier via le RiskDataElementLoader
        /// </summary>
        /// <param name="pRiskDataLoader">Element d'import des RiskData</param>
        public abstract Cst.ErrLevel LoadFile(RiskDataElementLoader pRiskDataLoader);

        /// <summary>
        /// Lecture de la valeur d'une donnée parsée
        /// </summary>
        /// <param name="pParsingRow">Classe de parsing</param>
        /// <param name="pDataName">Nom de la donnée</param>
        /// <returns></returns>
        public static string GetRowDataValue(IOTaskDetInOutFileRow pParsingRow, string pDataName)
        {
            string value;
            if (pParsingRow != default)
            {
                value = pParsingRow.GetRowDataValue(null, pDataName);
            }
            else
            {
                value = string.Empty;
            }
            return value;
        }
        #endregion Methods
    }

    /// <summary>
    /// Classe de traitement d'une tâche d'import de RiskData
    /// </summary>
    // EG 20190114 ProcessLog delegate Refactoring
    public class RiskDataImportTask : IIOTaskLaunching
    {
        #region Delegates
        private delegate void CheckFolderFromFilePathDelegate(string pPathFileName, double pTimeout, int pLevelOrder);
        private delegate string GetFilepathDelegate(string pFilePath);
        private delegate string GetTemporaryDirectoryDelegate(AppSession.AddFolderSessionId pAddFolderSession);
        
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pStatusEnum"></param>
        /// FI 20200623 [XXXXX] Add
        private delegate void SetErrorWarningDelegate(ProcessStateTools.StatusEnum pStatusEnum);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pEx"></param>
        /// FI 20200623 [XXXXX] Add
        private delegate void AddCriticalExceptionDelegate(Exception pEx);

        /// <summary>
        ///  Délégué pour le contrôle d'existence du folder associé à un path de fichier
        /// </summary>
        private readonly CheckFolderFromFilePathDelegate m_CheckFolderFromFilePath;
        /// <summary>
        ///  Délégué pour l'interprétation d'un chemin lorsqu'il existe un "."  ou "~"
        /// </summary>
        private readonly GetFilepathDelegate m_GetFilepath;
        /// <summary>
        ///  Délégué pour l'obtention d'un répertoire temporraire
        /// </summary>
        private readonly GetTemporaryDirectoryDelegate m_GetTemporaryDirectory;
        
        /// <summary>
        ///  
        /// </summary>
        /// FI 20200623 [XXXXX] Add
        private readonly SetErrorWarningDelegate m_SetErrorWarningDelegate;
        /// <summary>
        ///  
        /// </summary>
        /// FI 20200623 [XXXXX] Add
        private readonly AddCriticalExceptionDelegate m_AddCriticalException;


        #endregion Delegates

        #region Members
        /// <summary>
        /// Process Id
        /// </summary>
        private readonly int m_IdProcess;
        /// <summary>
        /// Connection string
        /// </summary>
        private readonly string m_Cs;
        /// <summary>
        /// Identifiant non significatif de l'acteur requester ou de l'acteur qui lance l'instance
        /// </summary>
        private readonly int m_UserId;
        /// <summary>
        /// Requester Hostname
        /// </summary>
        private readonly string m_RequesterHostName;
        /// <summary>
        /// Requester Session ID
        /// </summary>
        private readonly string m_RequesterSessionId;
        /// <summary>
        /// Requester IdA
        /// </summary>
        private readonly int m_RequesterIdA;
        /// <summary>
        /// Requester IdATRK
        /// </summary>
        private readonly int m_RequesterIdTRK;
        /// <summary>
        /// Requester Date
        /// </summary>
        private readonly DateTime m_RequesterDate;
        /// <summary>
        /// Id non significatif de la tâche
        /// </summary>
        private readonly int m_IOTaskId;
        /// <summary>
        /// Tâche IO
        /// </summary>
        private IOTask m_IOTask = default;
        /// <summary>
        /// Données de la table IOTASK de la tâche
        /// </summary>
        private SQL_IOTask m_SqlIOTask;
        /// <summary>
        /// Liste des éléments de la tâche
        /// </summary>
        private List<IOTaskElement> m_Element;


        private readonly AppSession m_Session;
        #endregion Members

        #region Accessors interface IIOTaskLaunching
        /// <summary>
        /// Process Id
        /// </summary>
        public int IdProcess
        {
            get { return m_IdProcess; }
        }
        /// <summary>
        /// Connection string
        /// </summary>
        public string Cs
        {
            get { return m_Cs; }
        }
        /// <summary>
        /// Identifiant non significatif de l'acteur requester ou de l'acteur qui lance l'instance
        /// </summary>
        public int UserId
        {
            get { return m_UserId; }
        }
        
        /// <summary>
        /// Requester Hostname
        /// </summary>
        public string RequesterHostName
        {
            get { return m_RequesterHostName; }
        }
        /// <summary>
        /// Requester Session ID
        /// </summary>
        public string RequesterSessionId
        {
            get { return m_RequesterSessionId; }
        }
        /// <summary>
        /// Requester IdA
        /// </summary>
        public int RequesterIdA
        {
            get { return m_RequesterIdA; }
        }
        /// <summary>
        /// Requester Date
        /// </summary>
        public DateTime RequesterDate
        {
            get { return m_RequesterDate; }
        }
        /// <summary>
        /// Tâche IO
        /// </summary>
        public IOTask IoTask
        {
            get { return m_IOTask; }
            set { m_IOTask = value; }
        }

        /// <summary>
        /// App Instance
        /// </summary>
        public AppSession Session
        {
            get { return m_Session ; }
        }

        /// <summary>
        /// App Instance
        /// </summary>
        public AppInstanceService AppInstance
        {
            get { return m_Session.AppInstance as AppInstanceService; }
        }
        #endregion Accessors interface IIOTaskLaunching
        
        #region Constructor
        /// <summary>
        /// Constructeur
        /// </summary>
        /// <param name="pProcess"></param>
        /// <param name="pIOTaskId"></param>
        public RiskDataImportTask(ProcessBase pProcess, int pIOTaskId)
        {
            // Affectation des membres
            
            //m_IdProcess = pProcess.processLog.header.IdProcess;
            m_IdProcess = pProcess.IdProcess;
            m_Cs = pProcess.Cs;
            m_UserId = pProcess.UserId;
            m_Session = pProcess.Session;
            
            m_RequesterHostName =  pProcess.MQueue.header.requester.hostName;
            m_RequesterSessionId = pProcess.MQueue.header.requester.sessionId;
            m_RequesterIdA = pProcess.MQueue.header.requester.idA;
            m_RequesterIdTRK = pProcess.MQueue.header.requester.idTRK;
            m_RequesterDate =  pProcess.MQueue.header.requester.date;
            //
            m_IOTaskId = pIOTaskId;
            //
            // Affectation des délégués
            m_CheckFolderFromFilePath = pProcess.CheckFolderFromFilePath;
            m_GetFilepath = pProcess.AppInstance.GetFilepath;
            m_GetTemporaryDirectory = pProcess.Session.GetTemporaryDirectory;

            
            
            m_AddCriticalException = pProcess.ProcessState.AddCriticalException; 
            m_SetErrorWarningDelegate = pProcess.ProcessState.SetErrorWarning;
        }
        #endregion Constructor

        #region Methods interface IIOTaskLaunching
        /// <summary>
        /// Contrôle l'existence du folder associé à un path de fichier
        /// <para>Si plusieurs tentatives ont été nécessaires pour accéder au Folder, Spheres® injecte des lignes dans le log pour le notifier</para>
        /// <para>Attention ServiceBase s'appuie sur l'écriture dans le log pour restituer des informations dans le journal des évènements de windows</para>
        /// </summary>
        /// <exception cref="SpheresException2[FOLDERNOTFOUND] si folder non accessible, l'exception donne les informations suivantes: folder,nbr de tentatives,timeout en secondes "/>
        /// <param name="pPathFileName"></param>
        /// <param name="pTimeout"></param>
        /// <param name="pLevelOrder"></param>
        public void CheckFolderFromFilePath(string pPathFileName, double pTimeout, int pLevelOrder)
        {
            m_CheckFolderFromFilePath(pPathFileName, pTimeout, pLevelOrder);
        }

        /// <summary>
        /// Méthode qui permet d'interpréter le chemin lorsqu'il existe un "."  ou "~"
        /// <para>. est emplacé par  {AppRootFolder}</para>
        /// <para>~ est remplacé par {AppWorkingFolder}</para>
        /// <para>
        /// </para>
        /// </summary>
        /// <param name="pFilePath"></param>
        /// <returns></returns>
        public string GetFilepath(string pFilePath)
        {
            return m_GetFilepath(pFilePath);
        }
        /// <summary>
        /// Retoure Le chemin {AppWorkingFolder}\{Temporary} ou {AppWorkingFolder}\{Temporary}\{sessionId}
        /// Le Folder est crée lorsque qu'il nexiste pas 
        /// </summary>
        /// <param name="pAddFolderSession"></param>
        /// <returns></returns>
        public string GetTemporaryDirectory(AppSession.AddFolderSessionId pAddFolderSession)
        {
            return m_GetTemporaryDirectory(pAddFolderSession);
        }
        
        
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pStatus"></param>
        /// FI 20200623 [XXXXX] Add
        public void SetErrorWarning(ProcessStateTools.StatusEnum pStatus)
        {
            m_SetErrorWarningDelegate.Invoke(pStatus);
        }

        /// <summary>
        /// Ajout de l'exception dans le process appelant 
        /// </summary>
        /// <param name="exception"></param>
        /// FI 20200623 [XXXXX] Add
        public void AddCriticalException(Exception exception)
        {
            m_AddCriticalException.Invoke(exception);
        }
        #endregion Methods interface IIOTaskLaunching

        #region Methods
        /// <summary>
        /// Défini "en dur" les paramètres de la tâche d'import
        /// </summary>
        /// <param name="pDtBusiness"></param>
        private void SetIOTaskParameters(DateTime pDtBusiness)
        {
            if (m_IOTask != default)
            {
                m_IOTask.parameters = new IOTaskParams
                {
                    param = new IOTaskParamsParam[2]
                };
                //
                IOTaskParamsParam paramPriceOnly = new IOTaskParamsParam
                {
                    id = "PRICEONLY",
                    name = "Only import of the prices;fr:Importer uniquement les prix",
                    displayname = "Only import of the prices;fr:Importer uniquement les prix",
                    direction = "Input",
                    datatype = "bool",
                    returntype = null,
                    Value = "false",
                };
                IOTaskParamsParam paramDtBusiness = new IOTaskParamsParam
                {
                    id = "DTBUSINESS",
                    name = "Clearing date;fr:Date de compensation",
                    displayname = "Clearing date;fr:Date de compensation",
                    direction = "Input",
                    datatype = "Date",
                    returntype = null,
                    Value = DtFunc.DateTimeToStringDateISO(pDtBusiness),
                };
                m_IOTask.parameters.param[0] = paramPriceOnly;
                m_IOTask.parameters.param[1] = paramDtBusiness;
            }
        }

        /// <summary>
        /// Lecture des éléments de la tâche
        /// </summary>
        private void ReadIOTask()
        {
            m_SqlIOTask = IOTaskTools.CheckTaskEnabledExists(m_Cs, m_IOTaskId);
            //
            m_IOTask = new IOTask(m_IOTaskId.ToString(), m_RequesterSessionId, m_SqlIOTask);
            //
            IDataReader dr = default;
            try
            {
                dr = IOTaskTools.TaskElementDataReader(m_Cs, m_IOTaskId);
                if (dr != default(IDataReader))
                {
                    m_Element = (from row in dr.DataReaderEnumerator<IOTaskElement, IOTaskElement>()
                                 where (row != null)
                                 select row).ToList();
                }
            }
            finally
            {
                if (dr != default(IDataReader))
                {
                    dr.Close();
                    dr.Dispose();
                }
            }
        }

        /// <summary>
        /// Effectuer la tâche d'import des RiskData
        /// </summary>
        /// <param name="pDtBusiness"></param>
        /// <param name="pResultData">Données résultant de l'import</param>
        /// <returns></returns>
        // EG 20190114 Add detail to ProcessLog Refactoring
        public Cst.ErrLevel ProcessTask(DateTime pDtBusiness, RiskDataLoad pResultData)
        {
            Cst.ErrLevel retCode = Cst.ErrLevel.SUCCESS;
            //
            // PM 20190222 [24326] Ajout log
            // Log: Chargement des paramètres de risque pour la méthode de calcul
            
            Logger.Log(new LoggerData(LogLevelEnum.None, new SysMsgCode(SysCodeEnum.LOG, 1079), 1, new LogParam(pResultData.MethodType)));
            //
            // Lire les éléments de la tâche
            ReadIOTask();
            //
            
            Logger.Log(new LoggerData(LogLevelEnum.None, new SysMsgCode(SysCodeEnum.LOG, 6001), 1, new LogParam(m_IOTask.Name)));
            //
            // Définir "en dur" les paramètres de la tâche
            SetIOTaskParameters(pDtBusiness);
            //
            // Ne prendre que les éléments d'Input
            IEnumerable<IOTaskElement> elementToProcess = m_Element.Where(e => e.ElementType == Cst.IOElementType.INPUT);
            //
            if (elementToProcess.Count() == 0)
            {
                SetErrorWarning(ProcessStateTools.StatusErrorEnum);
                
                
                Logger.Log(new LoggerData(LogLevelEnum.Error, new SysMsgCode(SysCodeEnum.SYS, 6007), 0, new LogParam(m_IOTask.Name)));
                //
                throw new Exception("Task without element");
            }
            //
            // Construction d'un loader pour chaque élément
            List<RiskDataElementLoader> elementsLoader = m_Element.Select(e => new RiskDataElementLoader(this, e)).ToList();
            //
            // Lecture du détail de chaque élément d'input de la tâche
            foreach (RiskDataElementLoader element in elementsLoader)
            {
                element.PrepareElement();
            }
            //
            // Traiter chaque élément d'input de la tâche
            List<Task<Cst.ErrLevel>> allElementTask = new List<Task<Cst.ErrLevel>>();
            foreach (RiskDataElementLoader element in elementsLoader)
            {
                Task<Cst.ErrLevel> elementTask = Task.Run(() => element.ProcessElement(pResultData));
                allElementTask.Add(elementTask);
            }
            try
            {
                Task.WaitAll(allElementTask.ToArray());
            }
            catch (AggregateException ae)
            {
                throw ae.Flatten();
            }
            
            Logger.Log(new LoggerData(LogLevelEnum.None, new SysMsgCode(SysCodeEnum.LOG, 6002), 1, new LogParam(m_IOTask.Name)));
            
            return retCode;
        }
        #endregion Methods
    }

    /// <summary>
    /// Classe de traitement d'un élément d'input
    /// </summary>
    public class RiskDataElementLoader
    {
        #region Members
        private readonly IIOTaskLaunching m_IOTaskLauncher;
        private readonly IOTaskElement m_IOTaskElement;
        private IOTaskInput m_TaskInput;
        private InputParsing m_InputParsing;
        private Stream m_Stream;
        private StreamReader m_StreamReader;
        //
        private int m_NbParsingIgnoredLines;
        #endregion Members

        #region Accessors
        public IIOTaskLaunching IOTaskLauncher
        {
            get { return m_IOTaskLauncher; }
        }
        
        public IOTaskElement IOTaskElement
        {
            get { return m_IOTaskElement; }
        }

        public IOTaskInput TaskInput
        {
            get { return m_TaskInput; }
        }

        public InputParsing InputParsing
        {
            get { return m_InputParsing; }
        }

        public Stream Stream
        {
            get { return m_Stream; }
        }

        public StreamReader StreamReader
        {
            get { return m_StreamReader; }
        }
        #endregion Accessors

        #region Constructors
        /// <summary>
        /// Constructeur
        /// </summary>
        /// <param name="pIOTaskLauncher">Tâche d'import </param>
        /// <param name="pIOTaskElement">Elément d'import</param>
        public RiskDataElementLoader(IIOTaskLaunching pIOTaskLauncher, IOTaskElement pIOTaskElement)
        {
            m_IOTaskLauncher = pIOTaskLauncher;
            m_IOTaskElement = pIOTaskElement;
        }
        #endregion Constructors

        #region Methods
        /// <summary>
        /// Lecture du détail de l'élément d'input
        /// </summary>
        /// <returns></returns>
        public Cst.ErrLevel PrepareElement()
        {
            Cst.ErrLevel ret = Cst.ErrLevel.SUCCESS;
            IDataReader dr = null;
            try
            {
                dr = IOTaskTools.TaskInputDataReader(m_IOTaskLauncher.Cs, m_IOTaskElement.ElementId);
                if (!dr.Read())
                {
                    // FI 20200623 [XXXXX] SetErrorWarning
                    m_IOTaskLauncher.SetErrorWarning(ProcessStateTools.StatusErrorEnum);
                    
                    
                    
                    Logger.Log(new LoggerData(LogLevelEnum.Error, new SysMsgCode(SysCodeEnum.SYS, 6005), 1,
                        new LogParam(Cst.IOElementType.INPUT),
                        new LogParam(m_IOTaskElement.ElementId + " (id:<b>" + m_IOTaskLauncher.IoTask.taskDet.id + "</b>)")));
                    //
                    throw new Exception("Task Element not found");
                }

                m_InputParsing = new InputParsing(m_IOTaskElement.ElementId, m_IOTaskLauncher);
                m_TaskInput = new IOTaskInput(dr);
                m_TaskInput.SetFilePathForImport(m_IOTaskLauncher);
            }
            catch (Exception ex)
            {
                throw new Exception("Error to read datas of Input (" + m_IOTaskElement.ElementId + ")", ex);
            }
            finally
            {
                if (null != dr)
                {
                    dr.Close();
                    dr.Dispose();
                }
            }               
            return ret;
        }

        /// <summary>
        /// Effectuer la tâche d'import des données
        /// </summary>
        /// <param name="pResultData">Chargement et stockage des données de l'import</param>
        /// <returns></returns>
        // PM 20190222 [24326] Add NASDAQOMXCCTFILE, NASDAQOMXFMSFILE, NASDAQOMXRCTFILE, NASDAQOMXVCTFILE, NASDAQOMXYCTFILE
        // EG 20190114 Add detail to ProcessLog Refactoring
        // PM 20230622 [26091][WI390] Ajout EURONEXTVARFOREXFILE, EURONEXTVARSCENARIOSFILE, EURONEXTVARPARAMETERSFILE, EURONEXTVARPARAMETERSTXTFILE, EURONEXTVARSTATICDATAFILE
        public Cst.ErrLevel ProcessElement(RiskDataLoad pResultData)
        {
            Cst.ErrLevel ret;
            if (pResultData != default(RiskDataLoad))
            {
                string elementLog = LogTools.IdentifierAndId(m_IOTaskElement.ElementId, m_IOTaskElement.IdIOTaskDet);

                switch (m_TaskInput.InputSourceDataStyle)
                {
                    case Cst.InputSourceDataStyle.EUREXPRISMA_THEORETICALPRICEFILE:
                    case Cst.InputSourceDataStyle.EUREXPRISMA_RISKMEASUREFILE:
                    case Cst.InputSourceDataStyle.EUREXPRISMA_RISKMEASUREAGGREGATIONFILE:
                    case Cst.InputSourceDataStyle.EUREXPRISMA_LIQUIDITYFACTORSFILE:
                    case Cst.InputSourceDataStyle.EUREXPRISMA_MARKETCAPACITIESFILE:
                    case Cst.InputSourceDataStyle.EUREXPRISMA_FXRATESFILE:
                    case Cst.InputSourceDataStyle.EUREXPRISMA_STLPRICESFILE:
                    case Cst.InputSourceDataStyle.EURONEXTVARPARAMETERSTXTFILE:
                    case Cst.InputSourceDataStyle.EURONEXTVARSCENARIOSFILE:
                    case Cst.InputSourceDataStyle.EURONEXTVARFOREXFILE:
                    case Cst.InputSourceDataStyle.EURONEXTLEGACYEQYVARPARAMETERSFILE:
                    case Cst.InputSourceDataStyle.EURONEXTLEGACYCOMVARPARAMETERSFILE:
                    case Cst.InputSourceDataStyle.EURONEXTLEGACYCOMDLYVARPARAMETERSFILE:
                    case Cst.InputSourceDataStyle.EURONEXTLEGACYEQYVARSCENARIOSFILE:
                    case Cst.InputSourceDataStyle.EURONEXTLEGACYCOMVARSCENARIOSFILE:
                    case Cst.InputSourceDataStyle.EURONEXTLEGACYCOMDLYVARSCENARIOSFILE:
                    case Cst.InputSourceDataStyle.EURONEXTLEGACYEQYVARFOREXFILE:
                    case Cst.InputSourceDataStyle.EURONEXTLEGACYCOMVARFOREXFILE:
                    case Cst.InputSourceDataStyle.EURONEXTLEGACYCOMDLYVARFOREXFILE:
                    case Cst.InputSourceDataStyle.EURONEXTLEGACYEQYVARREFERENTIALDATAFILE:
                    case Cst.InputSourceDataStyle.EURONEXTLEGACYCOMVARREFERENTIALDATAFILE:
                    case Cst.InputSourceDataStyle.EURONEXTLEGACYEQYEXPIRYPRICESFILE:
                    case Cst.InputSourceDataStyle.EURONEXTLEGACYCOMEXPIRYPRICESFILE:
                    case Cst.InputSourceDataStyle.EURONEXTLEGACYSTOCKINDICESVALUESFILE:
                    case Cst.InputSourceDataStyle.EURONEXTLEGACYEQYOPTIONSDELTASFILE:
                    case Cst.InputSourceDataStyle.EURONEXTLEGACYCOMOPTIONSDELTASFILE:
                    case Cst.InputSourceDataStyle.EURONEXTLEGACYVARMARKETCALENDARFILE:
                    case Cst.InputSourceDataStyle.NASDAQOMXFMSFILE:
                    case Cst.InputSourceDataStyle.NASDAQOMXCCTFILE:
                    case Cst.InputSourceDataStyle.NASDAQOMXRCTFILE:
                    case Cst.InputSourceDataStyle.NASDAQOMXYCTFILE:
                    case Cst.InputSourceDataStyle.NASDAQOMXVCTFILE:
                        ret = LoadFile(pResultData, elementLog);
                        break;
                    default:
                        ret = Cst.ErrLevel.NOTHINGTODO;

                        // FI 20200623 [XXXXX] SetErrorWarning
                        m_IOTaskLauncher.SetErrorWarning(ProcessStateTools.StatusWarningEnum);

                        
                        Logger.Log(new LoggerData(LogLevelEnum.Warning, new SysMsgCode(SysCodeEnum.SYS, 6012), 2,
                            new LogParam(m_TaskInput.InputSourceDataStyle),
                            new LogParam(elementLog)));
                        //
                        break;
                }
            }
            else
            {
                ret = Cst.ErrLevel.INCORRECTPARAMETER;
            }
            return ret;
        }

        /// <summary>
        /// Parse une ligne
        /// </summary>
        /// <param name="pLine">Ligne en entrée</param>
        /// <returns>Résultat du parsing</returns>
        public IOTaskDetInOutFileRow ParseLine(string pLine)
        {
            IOTaskDetInOutFileRow row = default;
            if (null == m_InputParsing)
            {
                throw new Exception("inputParsing is null, Please call method InitLoadLine");
            }
            else
            {
                IOCommonTools.LoadLine(m_IOTaskLauncher, pLine, 1, 1, m_InputParsing, ref row, ref m_NbParsingIgnoredLines);
            }
            return row;
        }

        /// <summary>
        /// Import des données d'un element d'import
        /// </summary>
        /// <param name="pResultData">Chargement et stockage des données de l'import</param>
        /// <param name="pElementLog">Identifiant de l'element pour le log</param>
        /// <returns></returns>
        // PM 20190222 [24326] New
        private Cst.ErrLevel LoadFile(RiskDataLoad pResultData, string pElementLog)
        {
            
            
            Logger.Log(new LoggerData(LogLevelEnum.None, new SysMsgCode(SysCodeEnum.LOG, 6003), 2,
                new LogParam(m_IOTaskElement.ElementType),
                new LogParam(pElementLog)));
            
            IOCommonTools.OpenFile(m_TaskInput.DataName, Cst.InputSourceDataStyle.ANSIFILE.ToString(), ref m_Stream, ref m_StreamReader);
            
            Cst.ErrLevel ret = pResultData.LoadFile(this);
            
            CloseStream();
            
            
            Logger.Log(new LoggerData(LogLevelEnum.None, new SysMsgCode(SysCodeEnum.LOG, 6004), 2,
                new LogParam(m_IOTaskElement.ElementType),
                new LogParam(pElementLog)));
            
            return ret;
        }
            
        /// <summary>
        /// Fermeture des flux de lecture des données
        /// </summary>
        private void CloseStream()
        {
            if (m_Stream != null)
            {
                m_Stream.Close();
            }
            if (m_StreamReader != null)
            {
                m_StreamReader.Close();
            }
        }
        #endregion Methods
    }
}
