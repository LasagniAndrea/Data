using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using EFS.ACommon;
using EFS.ApplicationBlocks.Data;
using EFS.ApplicationBlocks.Data.Extension;
using EFS.Common.IO.Interface;
using EFS.Common.Log;
using EFS.LoggerClient;
using EFS.LoggerClient.LoggerService;
using EFS.Process;

namespace EFS.Common.IO
{
    /// <summary>
    /// 
    /// </summary>
    public class IOTaskElement : IReaderRow
    {
        #region Members
        public int IdIOTaskDet { get; private set; }
        public int SequenceNo { get; private set; }
        public Cst.IOElementType ElementType { get; private set; }
        public string ElementId { get; private set; }
        public string LogLevel { get; private set; }
        public string CommitMode { get; private set; }
        public bool IsRunSuccess { get; private set; }
        public bool IsRunWarning { get; private set; }
        public bool IsRunError { get; private set; }
        public bool IsRunTimeOut { get; private set; }
        public bool IsRunDeadLock { get; private set; }
        public Cst.IOReturnCodeEnum RetCodeOnNoData { get; private set; }
        public Cst.IOReturnCodeEnum RetCodeOnNoDataModif { get; private set; }
        public Cst.RuleOnError RuleOnError { get; private set; }
        //
        public string ElementLog { get; private set; }
        //
        #region Members IReaderRow
        /// <summary>
        /// Data Reader permettant de lire les enregistrements
        /// </summary>
        public IDataReader Reader { get; set; }
        #endregion Members IReaderRow
        #endregion Members

        #region Constructor
        /// <summary>
        /// Constructeur vide pour IReaderRow
        /// </summary>
        public IOTaskElement() {}
        /// <summary>
        /// Constructeur avec mapping d'un DataRow
        /// </summary>
        /// <param name="pElement"></param>
        public IOTaskElement(DataRow pElement)
        {
            if (pElement != default(DataRow))
            {
                IdIOTaskDet = Convert.ToInt32(pElement["IDIOTASKDET"]);
                SequenceNo = Convert.ToInt32(pElement["SEQUENCENO"]);
                ElementId = Convert.ToString(pElement["IDIOELEMENT"]);
                ElementType = ReflectionTools.ConvertStringToEnumOrDefault<Cst.IOElementType>(pElement["ELEMENTTYPE"].ToString(), Cst.IOElementType.INPUT);
                LogLevel = pElement["LOGLEVEL"].ToString();
                CommitMode = pElement["COMMITMODE"].ToString();
                //
                ElementLog = LogTools.IdentifierAndId(ElementId, IdIOTaskDet);
                //
                IsRunSuccess = BoolFunc.IsTrue(pElement["ISRUNONSUCCESS"]);
                IsRunWarning = BoolFunc.IsTrue(pElement["ISRUNONWARNING"]);
                IsRunError = BoolFunc.IsTrue(pElement["ISRUNONERROR"]);
                IsRunTimeOut = BoolFunc.IsTrue(pElement["ISRUNONTIMEOUT"]);
                IsRunDeadLock = BoolFunc.IsTrue(pElement["ISRUNONDEADLOCK"]);
                //
                RetCodeOnNoData = ReflectionTools.ConvertStringToEnumOrDefault<Cst.IOReturnCodeEnum>(pElement["RETCODEONNODATA"].ToString(), Cst.IOReturnCodeEnum.NA);
                RetCodeOnNoDataModif = ReflectionTools.ConvertStringToEnumOrDefault<Cst.IOReturnCodeEnum>(pElement["RETCODEONNODATAM"].ToString(), Cst.IOReturnCodeEnum.NA);
                RuleOnError = ReflectionTools.ConvertStringToEnumOrDefault<Cst.RuleOnError>(pElement["RULEONERROR"].ToString(), Cst.RuleOnError.IGNORE);
            }
        }
        /// <summary>
        /// Constructeur avec mapping à partir d'un IDataReader
        /// </summary>
        /// <param name="pElement"></param>
        public IOTaskElement(IDataReader pElement)
        {
            if (pElement != default(IDataReader))
            {
                IdIOTaskDet = Convert.ToInt32(pElement["IDIOTASKDET"]);
                SequenceNo = Convert.ToInt32(pElement["SEQUENCENO"]);
                ElementId = Convert.ToString(pElement["IDIOELEMENT"]);
                ElementType = ReflectionTools.ConvertStringToEnumOrDefault<Cst.IOElementType>(pElement["ELEMENTTYPE"].ToString(), Cst.IOElementType.INPUT);
                LogLevel = pElement["LOGLEVEL"].ToString();
                CommitMode = pElement["COMMITMODE"].ToString();
                //
                ElementLog = LogTools.IdentifierAndId(ElementId, IdIOTaskDet);
                //
                IsRunSuccess = BoolFunc.IsTrue(pElement["ISRUNONSUCCESS"]);
                IsRunWarning = BoolFunc.IsTrue(pElement["ISRUNONWARNING"]);
                IsRunError = BoolFunc.IsTrue(pElement["ISRUNONERROR"]);
                IsRunTimeOut = BoolFunc.IsTrue(pElement["ISRUNONTIMEOUT"]);
                IsRunDeadLock = BoolFunc.IsTrue(pElement["ISRUNONDEADLOCK"]);
                //
                RetCodeOnNoData = ReflectionTools.ConvertStringToEnumOrDefault<Cst.IOReturnCodeEnum>(pElement["RETCODEONNODATA"].ToString(), Cst.IOReturnCodeEnum.NA);
                RetCodeOnNoDataModif = ReflectionTools.ConvertStringToEnumOrDefault<Cst.IOReturnCodeEnum>(pElement["RETCODEONNODATAM"].ToString(), Cst.IOReturnCodeEnum.NA);
                RuleOnError = ReflectionTools.ConvertStringToEnumOrDefault<Cst.RuleOnError>(pElement["RULEONERROR"].ToString(), Cst.RuleOnError.IGNORE);
            }
        }
        #endregion Constructor

        #region Method IReaderRow
        /// <summary>
        /// Lit un enregistrement à partir du IDataReader et le restitue sous forme d'objet (IOTaskElement)
        /// </summary>
        /// <returns>Un objet représentant l'enregistrement lu</returns>
        public object GetRowData()
        {
            IOTaskElement ret = default;
            if (null != Reader)
            {
                ret = new IOTaskElement(Reader);
            }
            return ret;
        }
        #endregion Method IReaderRow
    }

    /// <summary>
    /// 
    /// </summary>
    public class IOTaskInput
    {
        #region Members
        public string CommitMode { get; private set; }
        public string DataConnection { get; private set; }
        public string DataName { get; private set; }
        public string DataSectionEnd { get; private set; }
        public string DataSectionStart { get; private set; }
        public string DataStyle { get; private set; }
        public Cst.InputSourceDataStyle InputSourceDataStyle { get; private set; }
        public string DataTargetDescription { get; private set; }
        public string DisplayName { get; private set; }
        public bool IsHeterogenousSource { get; private set; }
        public bool IsVertical { get; private set; }
        public string LogLevel { get; private set; }
        public int VolumetryIndex { get; private set; }
        public int NbOmitRowEnd { get; private set; }
        public int NbOmitRowStart { get; private set; }
        public Cst.IOSerializeMode SerializeMode { get; private set; }
        public string XslMapping { get; private set; }
        #endregion Members

        #region Constructor
        /// <summary>
        /// Constructeur avec mapping à partir d'un IDataReader
        /// </summary>
        /// <param name="pElement"></param>
        public IOTaskInput(IDataReader pElement)
        {
            if (pElement != default(IDataReader))
            {
                CommitMode = pElement["COMMITMODE"].ToString();
                DataConnection = (Convert.IsDBNull(pElement["DATACONNECTION"]) ? null : pElement["DATACONNECTION"].ToString());
                DataName = pElement["DATANAME"].ToString();
                DataSectionEnd = (Convert.IsDBNull(pElement["DATASECTIONEND"]) ? null : pElement["DATASECTIONEND"].ToString());
                DataSectionStart = (Convert.IsDBNull(pElement["DATASECTIONSTART"]) ? null : pElement["DATASECTIONSTART"].ToString());
                DataStyle = pElement["DATASTYLE"].ToString();
                InputSourceDataStyle = ReflectionTools.ConvertStringToEnumOrDefault<Cst.InputSourceDataStyle>(DataStyle, Cst.InputSourceDataStyle.ANSIFILE);
                DataTargetDescription = (Convert.IsDBNull(pElement["DATATARGETDESC"]) ? string.Empty : pElement["DATATARGETDESC"].ToString());
                DisplayName = (Convert.IsDBNull(pElement["DISPLAYNAME"]) ? null : pElement["DISPLAYNAME"].ToString());
                IsHeterogenousSource = BoolFunc.IsTrue(pElement["ISHETEROGENOUS"]);
                IsVertical = BoolFunc.IsTrue(pElement["ISVERTICAL"]);
                LogLevel = pElement["LOGLEVEL"].ToString();
                VolumetryIndex = (Convert.IsDBNull(pElement["NBCOLUMNBYROWMAP"]) ? 0 : Convert.ToInt32(pElement["NBCOLUMNBYROWMAP"]));
                NbOmitRowEnd = (Convert.IsDBNull(pElement["NBOMITROWEND"]) ? 0 : Convert.ToInt32(pElement["NBOMITROWEND"]));
                NbOmitRowStart = (Convert.IsDBNull(pElement["NBOMITROWSTART"]) ? 0 : Convert.ToInt32(pElement["NBOMITROWSTART"]));
                SerializeMode = ReflectionTools.ConvertStringToEnumOrDefault<Cst.IOSerializeMode>(pElement["SERIALIZEMODE"].ToString(), Cst.IOSerializeMode.NORMAL);
                XslMapping = pElement["XSLMAPPING"].ToString();
            }
        }
        #endregion Constructor

        #region Methodes
        /// <summary>
        /// Interprétation du DataName
        /// </summary>
        /// <param name="pIOTaskLauncher"></param>
        // EG 20190114 Add detail to ProcessLog Refactoring
        public void SetFilePathForImport(IIOTaskLaunching pIOTaskLauncher)
        {
            DataName = IOCommonTools.CheckDynamicString(DataName, pIOTaskLauncher, false);
            //
            
            Logger.Log(new LoggerData(LogLevelEnum.None, new SysMsgCode(SysCodeEnum.LOG, 6034), 2, new LogParam(DataName)));
            //
            DataName = IOTaskTools.VerifyFilePathForImport(pIOTaskLauncher, DataName, default);
        }
        #endregion Methodes
    }

    /// <summary>
    /// 
    /// </summary>
    public static class IOTaskTools
    {
        #region Methods
        /// <summary>
        /// Lecture d'un SQL_IOTask à partir de son Id
        /// </summary>
        /// <param name="pCs"></param>
        /// <param name="pIOTaskId"></param>
        /// <returns></returns>
        /// <exception cref="Exception">Thrown when not found</exception>
        /// PM 20180219 [23824] Méthode déplacée à partir de Task (Task.cs) et rendue public static 
        public static SQL_IOTask CheckTaskEnabledExists(string pCs, int pIOTaskId)
        {
            // FI 20200916 [XXXXX] Ne jamais faire appel à un SQL_Table si l'Id vaut 0
            if (pIOTaskId == 0)
                throw new ArgumentException("Task id:0 doesn't exist");

            SQL_IOTask ioTask = new SQL_IOTask(pCs, pIOTaskId, SQL_IOTask.ScanDataDtEnabledEnum.Yes);
            if (false == ioTask.IsFound)
            {
                SQL_IOTask sql_IOTaskDisabled = new SQL_IOTask(pCs, pIOTaskId, SQL_IOTask.ScanDataDtEnabledEnum.No);
                if (sql_IOTaskDisabled.IsFound)
                {
                    throw new Exception("Task id:" + pIOTaskId.ToString() + " is disabled");
                }
                else
                {
                    throw new Exception("Task id:" + pIOTaskId.ToString() + " doesn't exist");
                }
            }
            return ioTask;
        }

        /// <summary>
        /// Requête de lecture des élements d'une tâche IO dans l'ordre d'exécution
        /// </summary>
        /// <param name="pCs"></param>
        /// <param name="pIOTaskId"></param>
        /// <returns></returns>
        public static QueryParameters TaskElementQuery(string pCs, int pIOTaskId)
        {
            DataParameters dataParameters = new DataParameters();
            dataParameters.Add(DataParameter.GetParameter(pCs, DataParameter.ParameterEnum.ID), pIOTaskId);
            //
            string sqlQuery = SQLCst.SQL_ANSI + @"
select IDIOTASKDET, SEQUENCENO, LOGLEVEL, COMMITMODE, ELEMENTTYPE, IDIOELEMENT,
       ISRUNONSUCCESS, ISRUNONWARNING, ISRUNONERROR, ISRUNONDEADLOCK, ISRUNONTIMEOUT,
       RETCODEONNODATA, RETCODEONNODATAM, RULEONERROR
  from dbo.IOTASKDET
 where (IDIOTASK = @ID)
   and " + OTCmlHelper.GetSQLDataDtEnabled(pCs, Cst.OTCml_TBL.IOTASKDET) + @"
 order by SEQUENCENO";
            //
            QueryParameters queryParameters = new QueryParameters(pCs, sqlQuery, dataParameters);
            return queryParameters;
        }

        /// <summary>
        /// Lecture des élements d'une tâche IO dans l'ordre d'exécution
        /// </summary>
        /// <param name="pCs"></param>
        /// <param name="pIOTaskId"></param>
        /// <returns></returns>
        /// PM 20180219 [23824] Méthode (SetTaskElementData()) déplacée à partir de Task (Task.cs) et rendue public static
        public static DataRow[] TaskElementDataRow(string pCs, int pIOTaskId)
        {
            DataRow[] row;
            if (pIOTaskId > 0)
            {
                QueryParameters queryParameters = TaskElementQuery(pCs, pIOTaskId);
                //
                DataSet dsData = DataHelper.ExecuteDataset(pCs, CommandType.Text, queryParameters.Query, queryParameters.Parameters.GetArrayDbParameter());
                if (dsData.Tables.Count > 0)
                {
                    row = dsData.Tables[0].Select();
                }
                else
                {
                    row = new DataRow[0];
                }
            }
            else
            {
                row = new DataRow[0];
            }
            return row;
        }

        /// <summary>
        /// DataReader de lecture des élements d'une tâche IO dans l'ordre d'exécution
        /// </summary>
        /// <param name="pCs"></param>
        /// <param name="pIOTaskId"></param>
        /// <returns></returns>
        public static IDataReader TaskElementDataReader(string pCs, int pIOTaskId)
        {
            IDataReader dataReader = default;
            if (pIOTaskId > 0)
            {
                QueryParameters queryParameters = TaskElementQuery(pCs, pIOTaskId);
                //
                dataReader = DataHelper.ExecuteReader(pCs, CommandType.Text, queryParameters.Query, queryParameters.Parameters.GetArrayDbParameter());
            }
            return dataReader;
        }

        /// <summary>
        /// Requête de lecture d'un élement d'Input d'une tâche IO
        /// </summary>
        /// <param name="pCs"></param>
        /// <param name="pIOElementId"></param>
        /// <returns></returns>
        public static QueryParameters TaskInputQuery(string pCs, string pIOElementId)
        {
            DataParameters dataParameters = new DataParameters();
            dataParameters.Add(DataParameter.GetParameter(pCs, DataParameter.ParameterEnum.IDENTIFIER), pIOElementId);
            //
            string sqlQuery = SQLCst.SQL_ANSI + @"
select DISPLAYNAME, DESCRIPTION, LOGLEVEL, COMMITMODE, DATASTYLE, DATACONNECTION, DATANAME, ISVERTICAL, ISHETEROGENOUS,
       DATASECTIONSTART, DATASECTIONEND, NBOMITROWSTART, NBOMITROWEND, XSLMAPPING, NBCOLUMNBYROWMAP, SERIALIZEMODE, DATATARGETDESC
  from dbo.IOINPUT
 where (IDIOINPUT = @IDENTIFIER)
   and " + OTCmlHelper.GetSQLDataDtEnabled(pCs, Cst.OTCml_TBL.IOINPUT);
            //
            QueryParameters queryParameters = new QueryParameters(pCs, sqlQuery, dataParameters);
            return queryParameters;
        }

        /// <summary>
        /// Requête de lecture d'un élement d'Output d'une tâche IO
        /// </summary>
        /// <param name="pCs"></param>
        /// <param name="pIOElementId"></param>
        /// <returns></returns>
        public static QueryParameters TaskOutputQuery(string pCs, string pIOElementId)
        {
            DataParameters dataParameters = new DataParameters();
            dataParameters.Add(DataParameter.GetParameter(pCs, DataParameter.ParameterEnum.IDENTIFIER), pIOElementId);
            //
            string sqlQuery = SQLCst.SQL_ANSI + @"
select DISPLAYNAME, DESCRIPTION, LOGLEVEL, COMMITMODE, DATASTYLE, WRITEMODE, DATACONNECTION,
       DATANAME, DATASECTIONSTART, DATASECTIONEND, ISHEADERCOLUMN, XSLMAPPING, NBCOLUMNBYROWMAP,
       OUT_DATASTYLE, OUT_DATACONNECTION, OUT_DATANAME, SERIALIZEMODE
  from dbo.IOOUTPUT
 where (IDIOOUTPUT = @IDENTIFIER)
   and " + OTCmlHelper.GetSQLDataDtEnabled(pCs, Cst.OTCml_TBL.IOOUTPUT);
            //
            QueryParameters queryParameters = new QueryParameters(pCs, sqlQuery, dataParameters);
            return queryParameters;
        }

        /// <summary>
        /// DataReader de lecture d'un élement d'Input d'une tâche IO
        /// </summary>
        /// <param name="pCs"></param>
        /// <param name="pIOElementId"></param>
        /// <returns></returns>
        public static IDataReader TaskInputDataReader(string pCs, string pIOElementId )
        {
            IDataReader dataReader = default;
            if (pIOElementId != default)
            {
                QueryParameters queryParameters = TaskInputQuery(pCs, pIOElementId);
                dataReader = DataHelper.ExecuteReader(pCs, CommandType.Text, queryParameters.Query, queryParameters.Parameters.GetArrayDbParameter());
            }
            return dataReader;
        }


        /// <summary>
        /// DataReader de lecture d'un élement d'Output d'une tâche IO
        /// </summary>
        /// <param name="pCs"></param>
        /// <param name="pIOElementId"></param>
        /// <returns></returns>
        public static IDataReader TaskOutputDataReader(string pCs, string pIOElementId)
        {
            IDataReader dataReader = default;
            if (pIOElementId != default)
            {
                QueryParameters queryParameters = TaskOutputQuery(pCs, pIOElementId);
                dataReader = DataHelper.ExecuteReader(pCs, CommandType.Text, queryParameters.Query, queryParameters.Parameters.GetArrayDbParameter());
            }
            return dataReader;
        }

        /// <summary>
        ///  Vérification de l'existence du fichier en entrée 
        ///  <para>- Au préalable interprétation de {pFilePath} s'il existe des wildcards(*,?) ou si {pFilePath} se termine par 9999_9999</para>
        /// </summary>
        /// <param name="pTask"></param>
        /// <param name="pFilePath"></param>
        /// <param name="pMsgLog"></param>
        /// FI 20131113 [19081]
        /// FI 20150917 [19081] Modify
        /// FI 20160503 [XXXXX] Modify
        // PM 20180219 [23824] Déplacée à partir de ProcessInput (IOProcessInput.cs) et rendue static par ajout de paramètres
        // EG 20190114 Add detail to ProcessLog Refactoring
        public static string VerifyFilePathForImport(IIOTaskLaunching pTask, string pFilePath, ArrayList pMsgLog)
        {
            //RD 20100719 Carriage return triming
            #region Carriage return triming
            if (StrFunc.IsFilled(pFilePath))
            {
                pFilePath = pFilePath.TrimEnd(Cst.CrLf.ToCharArray());
            }
            if (StrFunc.IsFilled(pFilePath))
            {
                pFilePath = pFilePath.TrimStart(Cst.CrLf.ToCharArray());
            }
            #endregion
            //
            #region Filename missing
            if (StrFunc.IsEmpty(pFilePath) || pFilePath.ToLower().Contains("please insert"))
            {
                throw new Exception(
                    @"<b>You forgot to specify the full pathname of the file to be imported</b>
                         - Verify it, and if necessary set either UNC (Universal Naming Convention) path or a full application-server-related path.
                         (eg: \\server\share\directory\sample.txt or S:\directory\sample.txt)");
            }
            #endregion
            //
            // PM 20180219 [23824] IOTools => IOCommonTools
            //if (!IOTools.IsHttp(pFilePath))
            if (!IOCommonTools.IsHttp(pFilePath))
            {
                Boolean isManagementOccurs = false;

                //FI 20101015 Contrôle existence du répertoire
                //RD 20101112 
                // Bug dans le cas des fichiers EXCEL & XML
                // Correction: ajout du test if (pVerifyFile)
                // EG 20130724 Passage du Timeout de lecture du fichier de 60 à 15
                // PM 20180219 [23824] Remplacement de Task par un paramètre implémentant l'interface IIOTaskLaunching
                //Task.process.CheckFolderFromFilePath(pFilePath, 15, 3);
                pTask.CheckFolderFromFilePath(pFilePath, 15, 3);
                //                        
                //PL 20100630 Wildcard management
                #region Wildcard management
                if ((false == isManagementOccurs) && FileTools.IsFilenameWithWildcards(pFilePath))
                {
                    FileTools.GetFilenameAndFoldername(pFilePath, out string fileName, out string folderPath);

                    DirectoryInfo directoryInfo = new DirectoryInfo(folderPath);
                    //PL 20131223 Refactoring
                    FileInfo[] fileInfos = null;

                    fileInfos = directoryInfo.GetFiles(fileName);
                    if (ArrFunc.IsFilled(fileInfos))
                    {
                        //PL 20100903 On retient ici le 1er fichier correspondant au searchPattern et trié par ordre alphabétique
                        fileInfos = FileTools.SortFilesByName(fileInfos);

                        bool isExitWhile = false;
                        int guard = 0;
                        while (!isExitWhile)
                        {
                            FileInfo fileInfo = fileInfos[0];

                            // PM 20180219 [23824] IOTools => IOCommonTools
                            //string bkpFolder = IOTools.CreateBkpFolder(folderPath);
                            string bkpFolder = IOCommonTools.CreateBkpFolder(folderPath);

                            //On déplace le fichier à traiter dans un sous-folder nommé "Bkp", et on importe par la suite le fichier depuis ce sous-folder.
                            // PM 20180219 [23824] IOTools => IOCommonTools
                            //FileTools.ErrLevel retMoveFileToFolder = IOTools.MoveFileToFolder(this, bkpFolder, fileInfo, ref pFilePath);
                            FileTools.ErrLevel retMoveFileToFolder = IOCommonTools.MoveFileToFolder(bkpFolder, fileInfo, ref pFilePath);
                            switch (retMoveFileToFolder)
                            {
                                case FileTools.ErrLevel.SUCCESS:
                                    // FI 20150917 [19081] add Log and use LOG-06037
                                    // PM 20180219 [23824] Remplacement de Task par un paramètre implémentant l'interface IIOTaskLaunching
                                    
                                    Logger.Log(new LoggerData(LogLevelEnum.None, new SysMsgCode(SysCodeEnum.LOG, 6037), 2, new LogParam(pFilePath)));

                                    isExitWhile = true;
                                    break;

                                case FileTools.ErrLevel.FILENOTFOUND:
                                case FileTools.ErrLevel.IOEXCEPTION: // FI 20160503 [XXXXX] add 
                                    //Fichier traité par une autre instance. On réinitie le cycle pour traiter un éventuel autre fichier.
                                    fileInfos = directoryInfo.GetFiles(fileName);
                                    if (ArrFunc.IsFilled(fileInfos))
                                    {
                                        fileInfos = FileTools.SortFilesByName(fileInfos);
                                    }
                                    else
                                    {
                                        isExitWhile = true;
                                    }
                                    break;
                            }

                            guard++;
                            if (guard >= 999)
                            {
                                isExitWhile = true;
                            }
                        }
                    }
                }
                #endregion Wildcard management

                #region concat management
                // FI 20150917 [19081] Fin de chantier 
                // Il s'agit de concaténer n fichiers présents en entrée lorsque le nom de fichier de l'élément d'importation se termine par 9999_9999
                // Ce code a été prévu initialement pour les fichiers Theoretical prices and instrument (importation PRISMA)
                // Principe : Spheres® procède à la génération d'un fichier résultat de la concaténation de n fichiers 
                // => Le fichier résultat est généré dans le répertoire temporary
                // => Les n fichiers sources sont conservés dans leur folder d'origine de manière à pourvoir être traité par une autre instance de IO
                string extention = Path.GetExtension(pFilePath);
                // PM 20170524 [22834][23078] Gestion 99_99 en plus de 9999_9999
                //if ((false == isManagementOccurs) && pFilePath.EndsWith(StrFunc.AppendFormat("9999_9999{0}", extention)))
                if ((false == isManagementOccurs)
                    && (pFilePath.EndsWith(StrFunc.AppendFormat("9999_9999{0}", extention))
                    || pFilePath.EndsWith(StrFunc.AppendFormat("99_99{0}", extention))))
                {
                    if (!File.Exists(pFilePath)) // Si le fichier 9999_9999 existe il est traité
                    {
                        FileTools.GetFilenameAndFoldername(pFilePath, out string fileName, out string folderPath);

                        DirectoryInfo directoryInfo = new DirectoryInfo(folderPath);
                        // PM 20170524 [22834][23078] Gestion 99_99 en plus de 9999_9999
                        //FileInfo[] fileInfos = directoryInfo.GetFiles(fileName.Replace("9999_9999", "????_?????"));
                        FileInfo[] fileInfos;
                        int numberLen;
                        string pattern;
                        string firstFileNumber;
                        if (pFilePath.EndsWith(StrFunc.AppendFormat("9999_9999{0}", extention)))
                        {
                            pattern = "9999_9999";
                            firstFileNumber = "0001_0001";
                            fileInfos = directoryInfo.GetFiles(fileName.Replace(pattern, "????_????"));
                            numberLen = 4;
                        }
                        else
                        {
                            pattern = "99_99";
                            firstFileNumber = "01_01";
                            fileInfos = directoryInfo.GetFiles(fileName.Replace(pattern, "??_??"));
                            numberLen = 2;
                        }
                        if (ArrFunc.IsFilled(fileInfos))
                        {
                            if (ArrFunc.Count(fileInfos) > 1)
                            {
                                //Génération d'un fichier résultat de la contaténation de n fichiers 
                                // PM 20170524 [22834][23078] Modif Regex
                                //Regex regEx = new Regex(@"\d{4}_\d{4}" + extention + "$", RegexOptions.IgnoreCase);
                                Regex regEx = new Regex(@"(\d{4}_\d{4})|(\d{2}_\d{2})" + extention + "$", RegexOptions.IgnoreCase);
                                Dictionary<string, string> srcfile = new Dictionary<string, string>();
                                // S'il y a plusieurs fichiers concaténation des fichiers
                                fileInfos = FileTools.SortFilesByName(fileInfos);
                                for (int i = 0; i < ArrFunc.Count(fileInfos); i++)
                                {
                                    if (regEx.IsMatch(fileInfos[i].Name))
                                        srcfile.Add(fileInfos[i].Name, fileInfos[i].FullName);
                                }

                                if (srcfile.Count > 0)
                                {
                                    #region Check des fichiers => génération d'exception lorsque certains fichiers sont manquants

                                    string firstFile = srcfile.Keys.First();
                                    // PM 20170524 [22834][23078] Modif Regex
                                    //regEx = new Regex(@"0001_\d{4}" + extention + "$", RegexOptions.IgnoreCase);
                                    regEx = new Regex(@"(0001_\d{4})|(01_\d{2})" + extention + "$", RegexOptions.IgnoreCase);
                                    if (false == regEx.IsMatch(firstFile))
                                    {
                                        // PM 20170524 [22834][23078] Modif exception message
                                        //throw new Exception(StrFunc.AppendFormat("First file: {0} is not expected. Expected file must contains 0001", firstFile));
                                        throw new Exception(StrFunc.AppendFormat("First file: {0} is not expected. Expected file must contains {1}01", firstFile, "".PadLeft(numberLen - 2, '0')));
                                    }

                                    int expectednbrFiles = IntFunc.IntValue(StrFunc.After(firstFile, "_").Replace(Path.GetExtension(firstFile), string.Empty));
                                    if (expectednbrFiles == 0)
                                    {
                                        throw new Exception(StrFunc.AppendFormat("File: {0} is not valid", firstFile));
                                    }

                                    List<string> usedFile = new List<string>();
                                    List<string> missingFile = new List<string>();
                                    for (int i = 1; i <= expectednbrFiles; i++)
                                    {
                                        // PM 20170524 [22834][23078] Gestion 99_99 en plus de 9999_9999
                                        //string formattedValue = i.ToString().PadLeft(4, '0');
                                        //string formattedExpectednbrFiles = expectednbrFiles.ToString().PadLeft(4, '0');
                                        string formattedValue = i.ToString().PadLeft(numberLen, '0');
                                        string formattedExpectednbrFiles = expectednbrFiles.ToString().PadLeft(numberLen, '0');
                                        string expectedEnd = StrFunc.AppendFormat("{0}_{1}{2}", formattedValue, formattedExpectednbrFiles, extention);

                                        string key = (from item in srcfile.Keys.Where(x => x.EndsWith(expectedEnd, true, null))
                                                      select item).FirstOrDefault();

                                        if (StrFunc.IsEmpty(key))
                                        {
                                            // PM 20170524 [22834][23078] Gestion 99_99 en plus de 9999_9999
                                            //missingFile.Add(fileName.Replace("9999_9999", formattedValue + "_" + formattedExpectednbrFiles));
                                            missingFile.Add(fileName.Replace(pattern, formattedValue + "_" + formattedExpectednbrFiles));
                                        }
                                        else
                                            usedFile.Add(srcfile[key]);
                                    }

                                    if (missingFile.Count > 0)
                                    {
                                        string list = StrFunc.StringArrayList.StringArrayToStringList(missingFile.ToArray(), false)
                                            .Replace(StrFunc.StringArrayList.LIST_SEPARATOR.ToString(), ",");

                                        throw new Exception(StrFunc.AppendFormat("Expected File(s) not found. File(s):{0}", list));
                                    }
                                    #endregion
                                    // PM 20180219 [23824] Remplacement de Task par un paramètre implémentant l'interface IIOTaskLaunching
                                    //string concatFolder = Task.process.appInstance.GetTemporaryDirectory(AppInstance.AddFolderSessionId.True);
                                    string concatFolder = pTask.GetTemporaryDirectory(AppSession.AddFolderSessionId.True);
                                    string finalFilePath = Path.Combine(concatFolder, fileName);

                                    string[] fullNamefiles = usedFile.ToArray();
                                    FileTools.FileConcat(fullNamefiles, ref finalFilePath, false);
                                    pFilePath = finalFilePath;

                                    #region  Ecriture ds Log
                                    string logFinalPath = pFilePath;
                                    if (logFinalPath.Length > 128)
                                    {
                                        logFinalPath = "... " + logFinalPath.Substring((logFinalPath.Length - 128) + 4);
                                    }


                                    
                                    Logger.Log(new LoggerData(LogLevelEnum.None, new SysMsgCode(SysCodeEnum.LOG, 6036), 2,
                                        new LogParam(logFinalPath),
                                        new LogParam(usedFile.Count),
                                        new LogParam(usedFile.First()),
                                        new LogParam(usedFile.Last())));

                                    #endregion  Ecriture ds Log
                                }
                            }
                            else // Il existe 1 seul fichier 
                            {
                                string finalFilePath = Path.Combine(folderPath, fileInfos[0].Name);

                                // FI 20150921 [19081] Case insensitive
                                // PM 20170524 [22834][23078] Gestion 99_99 en plus de 9999_9999
                                //if (false == finalFilePath.EndsWith("0001_0001" + extention, true, null))
                                //    throw new Exception(StrFunc.AppendFormat("Expected File: {0}. Existing file is {1}", fileName.Replace("9999_9999", "00001_0001"), finalFilePath));
                                if (false == finalFilePath.EndsWith(firstFileNumber + extention, true, null))
                                {
                                    string expectedFileName = fileName.Replace(pattern, firstFileNumber);
                                    throw new Exception(StrFunc.AppendFormat("Expected File: {0}. Existing file is {1}", expectedFileName, finalFilePath));
                                }

                                // PM 20180219 [23824] Remplacement de Task par un paramètre implémentant l'interface IIOTaskLaunching
                                
                                Logger.Log(new LoggerData(LogLevelEnum.None, new SysMsgCode(SysCodeEnum.LOG, 6037), 2, new LogParam(finalFilePath)));

                                pFilePath = finalFilePath;
                            }
                        }
                    }
                }
                #endregion concat management

                if ((!File.Exists(pFilePath)))
                {
                    // PM 20180219 [23824] Remplacement de Task par un paramètre implémentant l'interface IIOTaskLaunching
                    //pFilePath = Task.process.appInstance.GetFilepath(pFilePath);
                    pFilePath = pTask.GetFilepath(pFilePath);
                    //
                    if (!File.Exists(pFilePath))
                    {
                        throw new Exception(
                                @"<b>File to import does not exist</b>
                                 - Verify it, and if necessary set either UNC (Universal Naming Convention) path or a full application-server-related path.
                                 (eg: \\server\share\directory\sample.txt or S:\directory\sample.txt)" + Cst.CrLf +
                                "[File: " + pFilePath + "]" + Cst.CrLf + ArrFunc.GetStringList(pMsgLog, Cst.CrLf)
                                + Cst.CrLf + Cst.SYSTEM_EXCEPTION
                                );
                    }
                }
            }
            return pFilePath;
        }
        #endregion Methods
    }
}
