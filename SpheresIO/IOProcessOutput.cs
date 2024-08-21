#region using
using System;
using System.Collections;
using System.Data;
using System.IO;
using System.Net.Mail;
using System.Text;
using System.Xml;
//
using EFS.ACommon;
using EFS.ApplicationBlocks.Data;
using EFS.Common;
using EFS.Common.EfsSend;
using EFS.Common.IO;
using EFS.Common.Log;
using EFS.LoggerClient;
using EFS.LoggerClient.LoggerService;
using EFS.Process;
//
using EfsML.Business;
using EfsML.Notification;
#endregion

namespace EFS.SpheresIO
{

    /// <summary>
    /// 
    /// </summary>
    public class ProcessOutput : ProcessInOut
    {
        #region Members
        private Cst.OutputSourceDataStyle outputSourceDataStyle;
        private string outputSessionId;
        //
        private bool m_IsXMLOutput;
        private bool m_IsPDFOutput;
        private bool m_IsBINOutput;
        private bool m_IsSendOutput;
        private bool m_IsXslOutput;
        private bool m_IsParsingOutput;
        //                    
        private OutputParsing m_OutputParsing;
        private string m_XslParsing;
        //
        private int m_NbReadRowsFromSource;
        //
        private int m_NbWrittenRows;
        private int m_NbRejectedRows;
        private int m_NbIgnoredRows;
        private int m_NbExportedFiles;
        //
        private ArrayList m_ElementRowDataList;
        private bool m_IsDynamicOutputDataName;
        //
        private Cst.WriteMode m_EnumWriteMode;
        private ArrayList m_FilePathList;
        private string m_DestFullPathForPDFFileExport;
        #endregion

        #region accessors
        protected override string ElementIdentifiersForMessage
        {
            get
            {
                if (m_IsPDFOutput)
                    return "PDF";
                else if (m_IsXMLOutput)
                    return "XML";
                else
                    return m_Out_dataStyle;// A copmléter par style en cas de besoins
            }
        }
        #endregion

        #region Constructor
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pTask"></param>
        /// <param name="pIdIoTaskDet"></param>
        /// <param name="pIdIOOutput"></param>
        /// <param name="pRetCodeOnNoData"></param>
        /// <param name="pRetCodeOnNoDataModif"></param>
        public ProcessOutput(Task pTask, int pIdIoTaskDet, string pIdIOOutput,
            Cst.IOReturnCodeEnum pRetCodeOnNoData, Cst.IOReturnCodeEnum pRetCodeOnNoDataModif)
            : base(pTask, pIdIoTaskDet, pIdIOOutput, false, pRetCodeOnNoData, pRetCodeOnNoDataModif) { }
        #endregion Constructor

        #region Methods
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        // EG 20180423 Analyse du code Correction [CA2200]
        // EG 20190114 Add detail to ProcessLog Refactoring
        /// EG 20220221 [XXXXX] Gestion IRQ
        protected override void ReadingFromSource()
        {
            Cst.ErrLevel ret = Cst.ErrLevel.SUCCESS;
            outputSessionId = SystemTools.GetNewGUID();
            //
            SqlOrder sqlOrder = null;
            //    
            try
            {
                DataParameters paramSQL = null;
                string messageValue = string.Empty;
                outputSourceDataStyle = (Cst.OutputSourceDataStyle)Enum.Parse(typeof(Cst.OutputSourceDataStyle), m_DataStyle, true);

                //
                ArrayList alInOutFileRow = new ArrayList();
                try
                {
                    #region Load Data from DB
                    try
                    {
                        switch (outputSourceDataStyle)
                        {
                            case Cst.OutputSourceDataStyle.RDBMSTABLE:
                            case Cst.OutputSourceDataStyle.RDBMSVIEW:
                                sqlOrder = new SqlOrder(m_Task.Cs, CommandType.TableDirect, m_DataName, TaskPostParsing);
                                break;
                            case Cst.OutputSourceDataStyle.RDBMSCOMMAND:
                                sqlOrder = new SqlOrder(m_Task.Cs, CommandType.Text, m_DataName, TaskPostParsing);
                                break;
                            case Cst.OutputSourceDataStyle.RDBMSSP:
                                // Les SP peuvent écrire dans le log, on met donc celui-ci à jour.
                                Logger.Write();

                                sqlOrder = new SqlOrder(TaskPostParsing, m_Task.Cs, m_DataName, out paramSQL);
                                break;
                            case Cst.OutputSourceDataStyle.RDBMSLST:
                                // PL 20121024 New features
                                #region Controle de la consultation paramétrée.
                                //La consultation paramétrée doit respecter le format: IDLSTCONSULT.ACTOR_IDENTIFIER.IDLSTTEMPLATE  (ex.: TRADEFnO_ALLOC.Pascal.MyTemplate)
                                string errMsg = null;
                                if (String.IsNullOrEmpty(m_DataName))
                                {
                                    errMsg = "Missing View";
                                }
                                else
                                {
                                    string[] split = m_DataName.Split('.');
                                    if ((split.Length != 3)
                                        || String.IsNullOrEmpty(split[0]) || String.IsNullOrEmpty(split[1]) || String.IsNullOrEmpty(split[2])
                                        || (split[2].StartsWith("*")))
                                    {
                                        errMsg = "Invalid View! [" + m_DataName + "]";
                                    }
                                }
                                #endregion
                                //TODO (A terminer et supprimer le throw ci-dessous)
                                throw new Exception(
                                    "<b>Output source data style (" + m_DataStyle + ") is not supported</b>" + Cst.CrLf + ArrFunc.GetStringList(m_MsgLogInOut, Cst.CrLf));
                            // EG 20160404 Migration vs2013
                            //break;
                            default:
                                throw new Exception(
                                    "<b>Output source data style (" + m_DataStyle + ") is not supported</b>" + Cst.CrLf + ArrFunc.GetStringList(m_MsgLogInOut, Cst.CrLf));
                        }
                    }
                    catch (Exception ex)
                    {
                        throw new Exception(
                            "<b>Error to Load Data from DB</b>" + Cst.CrLf + ArrFunc.GetStringList(m_MsgLogInOut, Cst.CrLf), ex);
                    }
                    #endregion
                    //
                    #region Create arraylist of Rows

                    try
                    {
                        int i = 0;
                        while (sqlOrder.DataReader.Read())
                        {
                            if (IRQTools.IsIRQRequested(m_Task.Process, m_Task.Process.IRQNamedSystemSemaphore, ref ret))
                            {
                                sqlOrder.CodeReturn = ret;
                                break;
                            }
                            int columnCount = sqlOrder.DataReader.FieldCount;

                            IOTaskDetInOutFileRow inOutFileRow = new IOTaskDetInOutFileRow
                            {
                                //FI 20131119[] le compteur commence par r1
                                // PM 20180219 [23824] PREFIX_XML_ROW_ID déplacée dans la class IOCommonTools
                                //inOutFileRow.id = PREFIX_XML_ROW_ID + (i+1).ToString();
                                id = IOCommonTools.PREFIX_XML_ROW_ID + (i + 1).ToString(),
                                //FI 20131119[] alimentation de src avec (i+1)
                                src = (i + 1).ToString(),
                                table = new IOTaskDetInOutFileRowTable[1]
                                {
                                    new IOTaskDetInOutFileRowTable
                                    {
                                        name = sqlOrder.DataReader.GetSchemaTable().TableName,
                                        column = new IOTaskDetInOutFileRowTableColumn[columnCount]
                                    }
                                }
                            };

                            string tmp;
                            TypeData.TypeDataEnum typeData;
                            for (int j = 0; j < columnCount; j++)
                            {

                                //20101026 PL Tuning/Refactoring
                                tmp = sqlOrder.DataReader.GetDataTypeName(j);
                                #region Data type on Oracle
                                if (DataHelper.IsDbOracle(m_Task.Cs))
                                {
                                    if ((tmp == "Char") || (tmp == "Varchar2"))
                                    {
                                        typeData = TypeData.TypeDataEnum.@string;
                                    }
                                    else if (tmp == "Date")
                                    {
                                        //20090703 PL WARNING Bug, sous Oracle GetDataTypeName() renvoie "Date" pour un "DateTime"
                                        typeData = TypeData.GetTypeFromSystemType(sqlOrder.DataReader.GetFieldType(j));
                                    }
                                    else if (tmp.StartsWith("Int"))
                                    {
                                        typeData = TypeData.TypeDataEnum.integer;
                                    }
                                    else if (tmp == "Clob")
                                    {
                                        typeData = TypeData.TypeDataEnum.text;
                                    }
                                    else if (tmp == "Blob")
                                    {
                                        typeData = TypeData.TypeDataEnum.image;
                                    }
                                    else
                                    {
                                        typeData = TypeData.GetTypeDataEnum(tmp);
                                        if (TypeData.IsTypeUnknown(typeData))
                                            typeData = TypeData.GetTypeFromSystemType(sqlOrder.DataReader.GetFieldType(j));
                                    }
                                }
                                #endregion
                                #region Data type on SQLServer
                                else
                                {
                                    if (tmp.EndsWith("char"))
                                    {
                                        typeData = TypeData.TypeDataEnum.@string;
                                    }
                                    else if (tmp == "bit")
                                    {
                                        typeData = TypeData.TypeDataEnum.@bool;
                                    }
                                    else if (tmp == "timestamp")
                                    {
                                        //PL Pour rester compatible avec le code plus bas...
                                        typeData = TypeData.TypeDataEnum.unknown;
                                    }
                                    else
                                    {
                                        typeData = TypeData.GetTypeDataEnum(tmp);
                                        if (TypeData.IsTypeUnknown(typeData))
                                            typeData = TypeData.GetTypeFromSystemType(sqlOrder.DataReader.GetFieldType(j));
                                    }
                                }
                                #endregion
                                //

                                bool isColumnMQueue = (TaskPostParsing.name == "Corporate Actions Output" &&
                                    inOutFileRow.table[0].column[j].name == "BUILDINFO");
                                inOutFileRow.table[0].column[j] = new IOTaskDetInOutFileRowTableColumn
                                {
                                    name = sqlOrder.DataReader.GetName(j),
                                    //FI 20131119[] pour l'instant SpheresI/O ne sait pas déterminer qu'une colonne contient un MQueue=> isColumnMQueue = false
                                    //Codage en dur lorsque la colonne se nomme BUILDINFO et que la tâche d'exportation est "Corporate Actions Output"
                                    isColumnMQueue = isColumnMQueue,
                                    //FI 20140120 [] alimenter isColumnMQueue uniquement lorsqu'il est spécifié pour ne pas surcherger le flux
                                    isColumnMQueueSpecified = isColumnMQueue
                                };

                                // RD 20110921 / déplacer cette ligne après le if
                                //inOutFileRow.table[0].column[j].datatype = typeData.ToString();
                                //// RD 20110401 [17379] Serialize mode : Pour le mode Light ne pas sérialiser le DataType s'il est = string
                                //inOutFileRow.table[0].column[j].datatypeSpecified = IOTools.XMLIsDataTypeSpecified(inOutFileRow.table[0].column[j].datatype, IsLightSerializeMode);
                                //
                                XmlDocument doc = new XmlDocument();
                                //
                                if (TypeData.IsTypeImage(typeData) && (DBNull.Value != sqlOrder.DataReader[j]))
                                {
                                    if (Cst.OutputTargetDataStyle.PDFFILE.ToString() == m_Out_dataStyle)
                                    {
                                        #region Type Image
                                        //200906004 PL Change AddFolderSessionId.False to True
                                        string fileFullPath = m_Task.Session.GetTemporaryDirectory(AppSession.AddFolderSessionId.True) + @"\";
                                        fileFullPath += m_IdIOInOut + "_" + inOutFileRow.id + "_" + inOutFileRow.table[0].name + "_" + inOutFileRow.table[0].column[j].name + "_" + outputSessionId + @".dat";
                                        //
                                        byte[] fileContent = (byte[])sqlOrder.DataReader[j];
                                        //
                                        if (Cst.ErrLevel.SUCCESS == FileTools.WriteBytesToFile(fileContent, fileFullPath, FileTools.WriteFileOverrideMode.Override))
                                            inOutFileRow.table[0].column[j].value = doc.CreateCDataSection(fileFullPath).Value;
                                        #endregion
                                    }
                                }
                                // ROWVERSION retourne unknown => Il est exclu
                                else if (false == TypeData.IsTypeUnknown(typeData))
                                {
                                    #region type standard
                                    string fmtIsoValue = null;
                                    //
                                    if (false == Convert.IsDBNull(sqlOrder.DataReader.GetValue(j)))
                                        fmtIsoValue = ObjFunc.FmtToISo(sqlOrder.DataReader.GetValue(j), typeData);
                                    //
                                    // RD 20110921 / Exporter dans un fichier les dates des prochain jours ouvrés 
                                    if (StrFunc.IsFilled(fmtIsoValue) && inOutFileRow.table[0].column[j].name == "DTHOLIDAYVALUE")
                                    {
                                        typeData = TypeData.TypeDataEnum.date;
                                        EFS_BusinessCenters efs_BusinessCenters = new EFS_BusinessCenters(m_Task.Cs, null);
                                        DateTime holidayDate = efs_BusinessCenters.GetDate(fmtIsoValue, null);
                                        //
                                        if (DtFunc.IsDateTimeFilled(holidayDate))
                                            fmtIsoValue = ObjFunc.FmtToISo(holidayDate, typeData);
                                        else
                                            fmtIsoValue = string.Empty;
                                    }
                                    //
                                    // RD 20100723 [17097] Input: XML Column 
                                    // Code transféré dans la méthode IOTools.SetInCDATA pour Harmonisation avec Import d'une donnée de type XML
                                    if (StrFunc.IsFilled(fmtIsoValue) && TypeData.IsTypeXml(typeData.ToString()))
                                    {
                                        // FI 20131119 [] La CS n'est jamais exportée 
                                        if (inOutFileRow.table[0].column[j].isColumnMQueue)
                                            fmtIsoValue = EraseCSInMqueue(fmtIsoValue);
                                        // PM 20180219 [23824] IOTools => IOCommonTools
                                        inOutFileRow.table[0].column[j].valueXML = IOCommonTools.SetInCDATA(fmtIsoValue, doc);
                                    }
                                    else
                                    {
                                        inOutFileRow.table[0].column[j].value = doc.CreateCDataSection(fmtIsoValue).Value;
                                    }
                                    #endregion
                                }
                                //
                                inOutFileRow.table[0].column[j].datatype = typeData.ToString();
                                // RD 20110401 [17379] Serialize mode : Pour le mode Light ne pas sérialiser le DataType s'il est = string
                                // PM 20180219 [23824] IOTools => IOCommonTools
                                inOutFileRow.table[0].column[j].datatypeSpecified = IOCommonTools.XMLIsDataTypeSpecified(inOutFileRow.table[0].column[j].datatype, IsLightSerializeMode);

                            }

                            alInOutFileRow.Add(inOutFileRow);
                            i++;
                        }
                    }
                    catch (Exception ex)
                    {
                        throw new Exception(
                            "<b>Error to create arraylist of Rows</b>" + Cst.CrLf + ArrFunc.GetStringList(m_MsgLogInOut, Cst.CrLf), ex);
                    }
                    #endregion
                }
                finally
                {
                    if (null != sqlOrder && null != sqlOrder.DataReader)
                    {
                        sqlOrder.DataReader.Close();
                        sqlOrder.DataReader.Dispose();
                    }
                }

                if (false == IRQTools.IsIRQRequested(m_Task.Process, m_Task.Process.IRQNamedSystemSemaphore, ref ret))
                {
                    //20081119 PL ******************************************************************
                    //            Code anciennement dans le constructeur de SqlOrder()
                    //
                    //  WARNING: Avec un DataReader les OUTPUT parameters ne peuvent être exploités qu'une fois le DataReader fermé !
                    //  See also: http://www.dominicpettifer.co.uk/displayBlog.aspx?id=8
                    if (outputSourceDataStyle == Cst.OutputSourceDataStyle.RDBMSSP)
                    {
                        sqlOrder.CodeReturn = Cst.ErrLevel.SUCCESS;
                        ProcessStateTools.StatusEnum status = ProcessStateTools.StatusSuccessEnum;
                        string paramName = string.Empty;
                        //
                        paramName = SQL_IOTaskParams.GetParamNameOfReturnSPParam(m_Task.Cs, TaskPostParsing.id.ToString(), Cst.ReturnSPParamTypeEnum.RETURNCODE);
                        if (paramSQL.Contains(paramName))
                        {
                            try
                            {
                                sqlOrder.CodeReturn = (Cst.ErrLevel)Enum.Parse(typeof(Cst.ErrLevel), paramSQL[paramName].Value.ToString(), true);
                            }
                            catch { sqlOrder.CodeReturn = Cst.ErrLevel.ABORTED; }
                        }
                        if (Cst.ErrLevel.SUCCESS != sqlOrder.CodeReturn)
                        {
                            status = ProcessStateTools.StatusErrorEnum;
                            // FI 20200706 [XXXXX] call SetErrorWarning
                            ProcessState.SetErrorWarning(ProcessStateTools.StatusErrorEnum);
                        }
                        //
                        paramName = SQL_IOTaskParams.GetParamNameOfReturnSPParam(m_Task.Cs, TaskPostParsing.id.ToString(), Cst.ReturnSPParamTypeEnum.RETURNMESSAGE);
                        string message = string.Empty;
                        if (paramSQL.Contains(paramName))
                            message = paramSQL[paramName].Value.ToString();
                        else
                            message = "Termination of the external stored procedure";
                        //
                        paramName = SQL_IOTaskParams.GetParamNameOfReturnSPParam(m_Task.Cs, TaskPostParsing.id.ToString(), Cst.ReturnSPParamTypeEnum.RETURNDATA);
                        if (paramSQL.Contains(paramName))
                            message += ";" + paramSQL[paramName].Value.ToString();
                        
                        string [] data = StrFunc.StringArrayList.StringListToStringArray(message);

                        ProcessLogInfo info = new ProcessLogInfo
                        {
                            status = status.ToString()
                        };
                        info.SetMessageAndData(data);


                        Logger.Log(LoggerConversionTools.ProcessLogInfoToLoggerData(info));

                        if (false == (Cst.ErrLevel.SUCCESS == sqlOrder.CodeReturn))
                        {
                            throw new Exception(
                                "<b>Error Executing external stored procedure</b>" + Cst.CrLf + ArrFunc.GetStringList(m_MsgLogInOut, Cst.CrLf));
                        }
                    }
                }


                if (false == IRQTools.IsIRQRequested(m_Task.Process, m_Task.Process.IRQNamedSystemSemaphore, ref ret))
                {
                    //20081119 PL ******************************************************************
                    #region Create postParsingFlow structure
                    try
                    {
                        IOTaskDetInOutFile xmlParsing = new IOTaskDetInOutFile
                        {
                            name = m_DataName,
                            row = (IOTaskDetInOutFileRow[])(alInOutFileRow.ToArray(typeof(IOTaskDetInOutFileRow)))
                        };

                        TaskPostParsing.taskDet.output.file = xmlParsing;

                        m_NbReadRowsFromSource = alInOutFileRow.Count;
                    }
                    catch (Exception ex)
                    {
                        throw new Exception(
                            "<b>Error to create postParsingFlow structure</b>" + Cst.CrLf + ArrFunc.GetStringList(m_MsgLogInOut, Cst.CrLf), ex);
                    }
                    #endregion
                    //
                    #region AddPostParsingFileInLog
                    bool isStatusError = (ProcessStateTools.StatusError.ToLower() == TaskPostParsing.taskDet.output.file.status);
                    // FI 20200706 [XXXXX] SetErrorWarning si Error
                    if (isStatusError)
                        ProcessState.SetErrorWarning(ProcessStateTools.StatusErrorEnum);

                    AddPostParsingFileInLog(isStatusError);
                    #endregion

                    if (0 == alInOutFileRow.Count)
                        PostParsingNoData();
                }
            }
            catch (Exception ex)
            {
                // FI 20200623 [XXXXX] SetErrorWarning
                // FI 20200706 [XXXXX] Mise en commentaire puisqu'il existe un throw
                //m_Task.process.ProcessState.SetErrorWarning(ProcessStateTools.StatusErrorEnum);

                // FI 20190829 [XXXXX] Ecriture dans le log Spheres du message d'erreur
                string msg = ExceptionTools.GetMessageExtended(ex);
                
                Logger.Log(new LoggerData(LogLevelEnum.Info, msg, 3));

                // FI 20190829 [XXXXX] Ecriture dans la trace du message Complet (avec pile des appels)
                AppInstance.TraceManager.TraceError(this, ExceptionTools.GetMessageAndStackExtended(ex));

                
                
                //m_ElementStatus = ProcessStateTools.StatusEnum.ERROR;
                // FI 20200706 [XXXXX] Mise en commentaire puisqu'il existe un throw
                Logger.Log(new LoggerData(LogLevelEnum.Error, new SysMsgCode(SysCodeEnum.SYS, 6003), 3, new LogParam("Reading")));

                // FI 20190829 [XXXXX] Generation de l'exception Error on Reading pour faire comme dans writing (voir "Error on writing")
                throw new Exception("Error on Reading");
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="pXmlDocumentPostMapping"></param>
        /// <param name="pXslParsingFile"></param>
        /// <param name="pDestinationFile"></param>
        /// <param name="pDestFileInfo"></param>
        /// <param name="pIsIOTrackWriten"></param>
        /// <returns></returns>
        // EG 20190114 Add detail to ProcessLog Refactoring
        protected Cst.ErrLevel WriteXmlMapToDestination(XmlDocument pXmlDocumentPostMapping, string pXslParsingFile, string pDestinationFile, ref FileInfo pDestFileInfo, ref bool pIsIOTrackWriten)
        {
            Cst.ErrLevel ret = Cst.ErrLevel.SUCCESS;
            //
            // RD 20101223 / Pouvoir écrire dans le même fichier plusieurs fois
            Cst.WriteMode newEnumWriteMode = m_EnumWriteMode;
            //            
            if (m_FilePathList.Contains(pDestinationFile))
            {
                if (m_IsXMLOutput || m_IsSendOutput)
                    throw new Exception(
                            @"<b>Risk of overwriting of data. Multiple " + m_Out_dataStyle + " files are to write to the same path (" + pDestinationFile + ")</b>");
                else
                    newEnumWriteMode = Cst.WriteMode.APPEND;
            }
            //
            #region Check Destination Directory
            FileInfo fInfo = new FileInfo(pDestinationFile);
            if (false == fInfo.Directory.Exists)
            {
                fInfo = new FileInfo(m_Task.Process.AppInstance.GetFilepath(pDestinationFile));
                if ((false == fInfo.Directory.Exists) && (false == m_IsSendOutput))
                    throw new Exception(
                            @"<b>Folder (" + fInfo.Directory.FullName + ") of Destination File (" + fInfo.FullName + ") specified for output " + m_IdIOInOut + " [" + m_Out_dataStyle + @"](" + m_DataName + " [" + m_DataStyle + @"]) does not exist.</b>
                            Verify it, and if necessary set either UNC (Universal Naming Convention) path or a full application-server-related path.
                            (eg: \\server\share\directory\sample.txt or S:\directory\sample.txt)");
            }
            //
            pDestFileInfo = fInfo;
            #endregion
            //              
            if (m_IsXMLOutput || m_IsSendOutput)
            {
                XmlDocument xmlDocFinal;

                #region XML FILE
                #region Transformation XSL dans un Document xmlDocFinal
                try
                {
                    xmlDocFinal = IOTools.XmlTransformInDoc(pXmlDocumentPostMapping, pXslParsingFile);
                }
                catch (Exception ex)
                {
                    throw new Exception(
                        "Error to Transform the result of mapping of Task : " + m_TaskPostMapping.name + " with XSL file: " + pXslParsingFile, ex);
                }
                #endregion Transformation XSL
                #region Sauvegarde de XMLFinal dans le fichier XML Destination
                string sendFileMsg = string.Empty;
                string sendDataName = "StreamToSend.xml";
                //
                if (fInfo.Directory.Exists)
                {
                    xmlDocFinal.Save(fInfo.FullName);
                    // PM 20180219 [23824] IOTools => IOCommonTools
                    //sendFileMsg = IOTools.GetFileMsg(fInfo);
                    sendFileMsg = IOCommonTools.GetFileMsg(fInfo);
                    sendDataName = fInfo.Name;
                }
                //
                if (m_IsSendOutput)
                {
                    // RD 20120314 
                    // Pour ne pas arrêter le traitement, on écrit l'exception tout de suite et on continu            
                    try
                    {
                        m_Task.AddLogAttachedDoc(sendDataName, xmlDocFinal);
                    }
                    catch (Exception ex)
                    {
                        
                        Logger.Log(new LoggerData(LogLevelEnum.None, ex.Message));
                    }

                    Send(xmlDocFinal, sendFileMsg, ref pIsIOTrackWriten);
                }
                #endregion Sauvegarde
                #endregion XML FILE
            }
            else
            {
                string stringFinal;

                #region Other Output file type
                #region Transformation XSL dans un StringBuilder sbFinal
                try
                {
                    stringFinal = IOTools.XmlTransformInString(pXmlDocumentPostMapping, pXslParsingFile);
                }
                catch (Exception ex)
                {
                    throw new Exception(
                        "Error to Transform the result of mapping of Task : " + m_TaskPostMapping.name + " with XSL file: " + pXslParsingFile, ex);
                }
                #endregion Transformation XSL
                #region Sauvegarde de stringFinal dans le fichier Destination
                //200906004 PL Change AddFolderSessionId.False to True
                // RD 20101223 / Pouvoir écrire dans le même fichier plusieurs fois / utilisation de newEnumWriteMode
                // PM 20180219 [23824] IOTools => IOCommonTools
                //IOTools.OpenFile(fInfo.FullName, newEnumWriteMode, m_Out_dataStyle, m_Task.appInstance.GetTemporaryDirectory(AppInstance.AddFolderSessionId.True));
                //IOTools.StreamWriter.Write(stringFinal);
                //IOTools.CloseFile();
                IOCommonTools.OpenFile(fInfo.FullName, newEnumWriteMode, m_Out_dataStyle, m_Task.Session.GetTemporaryDirectory(AppSession.AddFolderSessionId.True));
                IOCommonTools.StreamWriter.Write(stringFinal);
                IOCommonTools.CloseFile();
                #endregion Sauvegarde
                #endregion
            }
            //
            if (false == m_FilePathList.Contains(pDestinationFile))
                m_FilePathList.Add(pDestinationFile);
            //
            return ret;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pXmlSend"></param>
        /// <param name="pSendFileMsg"></param>
        /// <param name="pIsIOTrackWriten"></param>
        private void Send(XmlDocument pXmlSend, string pSendFileMsg, ref bool pIsIOTrackWriten)
        {
            Cst.OutputTargetDataStyle enumOut_dataStyle = (Cst.OutputTargetDataStyle)Enum.Parse(typeof(Cst.OutputTargetDataStyle), m_Out_dataStyle);
            //
            switch (enumOut_dataStyle)
            {
                case Cst.OutputTargetDataStyle.SENDSMTP:
                    SendSMTP(pXmlSend, pSendFileMsg, ref pIsIOTrackWriten);
                    break;
                case Cst.OutputTargetDataStyle.SENDFLYDOC:
                    SendFlyDoc(pXmlSend, pSendFileMsg, ref pIsIOTrackWriten);
                    break;
                default:
                    throw new Exception(
                        @"<b>Output data style " + m_Out_dataStyle + @" does not support Send method.</b>" + Cst.CrLf + ArrFunc.GetStringList(m_MsgLogInOut, Cst.CrLf));
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pXmlSendSmtp">Flux XML (serializable en EfsSendSMTP)</param>
        /// <param name="pSendFileMsg"></param>
        /// <param name="pIsIOTrackWriten"></param>
        /// FI 20150115 [XXXXX] Modify (Diverses modification pour éviter le message Objet null reference)
        // EG 20180423 Analyse du code Correction [CA2200]
        // EG 20190114 Add detail to ProcessLog Refactoring
        // RD 20190903 [24869] Modify
        private void SendSMTP(XmlDocument pXmlSendSmtp, string pSendFileMsg, ref bool pIsIOTrackWriten)
        {
            pIsIOTrackWriten = false;
            // RD 20190903 [24869] Log email source only in the current method log message.
            ArrayList logEmailSource = new ArrayList();
            // PL 20210421 Mise en commentaire de l'initialisation de idLog, car utilisée uniquement en fin de méthode
            //int idLog = m_Task.ProcessLog.header.IdProcess;

            try
            {
                if ((null == m_Task.IoTask.parameters) || ArrFunc.IsEmpty(m_Task.IoTask.parameters.param))
                    throw new NullReferenceException(StrFunc.AppendFormat("There is no parameter on IoTask (identifier:{0})", m_Task.IoTask.name));

                IOTaskParamsParam param1 = m_Task.IoTask.parameters.param[0];
                Pair<int, string> source = new Pair<int, string>(Convert.ToInt32(param1.Value), param1.name);

                logEmailSource.Add(StrFunc.AppendFormat("- Source: <b>{0}</b>", LogTools.IdentifierAndId(source.Second, source.First)));

                // RD 20190903 [24869] Log Sender and Recipient 
                if (source.Second == "IDMCO")
                {
                    int idMCO = source.First;

                    DataParameters dp = new DataParameters();
                    dp.Add(DataParameter.GetParameter(m_Task.Cs, DataParameter.ParameterEnum.ID), idMCO);
                    string query = @"select mco.IDMCO,
sendby.IDA as SENDBY_IDA,sendby.IDENTIFIER as SENDBY_IDENTIFIER,sendby.DISPLAYNAME as SENDBY_DISPLAYNAME,
sendto.IDA as SENDTO_IDA,sendto.IDENTIFIER as SENDTO_IDENTIFIER,sendto.DISPLAYNAME as SENDTO_DISPLAYNAME
from dbo.MCO mco
inner join dbo.ACTOR sendby on sendby.IDA = mco.IDA_SENDBYOFFICE
inner join dbo.ACTOR sendto on sendto.IDA = mco.IDA_SENDTOPARTY
where mco.IDMCO = @ID";
                    QueryParameters qryParameters = new QueryParameters(m_Task.Cs, query, dp);
                    using (IDataReader dr = DataHelper.ExecuteReader(m_Task.Cs, CommandType.Text, qryParameters.Query.ToString(), qryParameters.Parameters.GetArrayDbParameter()))
                    {
                        if (dr.Read())
                        {
                            Tuple<int, string, string> sendBy = new Tuple<int, string, string>(Convert.ToInt32(dr["SENDBY_IDA"]), dr["SENDBY_IDENTIFIER"].ToString(), dr["SENDBY_DISPLAYNAME"].ToString());
                            Tuple<int, string, string> sendTo = new Tuple<int, string, string>(Convert.ToInt32(dr["SENDTO_IDA"]), dr["SENDTO_IDENTIFIER"].ToString(), dr["SENDTO_DISPLAYNAME"].ToString());


                            logEmailSource.Add(StrFunc.AppendFormat("- Sender: <b>{0}</b> / {1}", LogTools.IdentifierAndId(sendBy.Item2, sendBy.Item1), sendBy.Item3));
                            logEmailSource.Add(StrFunc.AppendFormat("- Receiver: <b>{0}</b> / {1}", LogTools.IdentifierAndId(sendTo.Item2, sendTo.Item1), sendTo.Item3));
                        }
                        else
                            throw new InvalidProgramException(StrFunc.AppendFormat("IDCMO : {0} not found", idMCO));
                    }
                }

                EFS_SerializeInfoBase serializeInfo = new EFS_SerializeInfoBase(null, pXmlSendSmtp.OuterXml)
                {
                    TypeDocument = typeof(EfsSendSMTP)
                };
                EfsSendSMTP sendSmtp = (EfsSendSMTP)CacheSerializer.Deserialize(serializeInfo);

                // FI 20150115 [XXXXX] Il faut mieux un message qu'une erreur de type Objet null reference
                if (null == sendSmtp.SmtpClient)
                    throw new NullReferenceException("sendSmtp.SmtpClient is null");

                // FI 20150115 [XXXXX] Il faut mieux un message qu'une erreur de type Objet null reference
                if (null == sendSmtp.MailMessage)
                    throw new NullReferenceException("sendSmtp.MailMessage is null");
                // 
                // RD 20190903 [24869] Log Sender and Recipient email address missing
                if ((null == sendSmtp.MailMessage.From) || (StrFunc.IsEmpty(sendSmtp.MailMessage.From.MailAddress)))
                    throw new NullReferenceException("There is no sender address of the email");
                // 
                if (ArrFunc.IsEmpty(sendSmtp.MailMessage.To))
                    throw new NullReferenceException("There is no recipient address of the email");

                string IOTrackDescription = "Send email by SMTP";

                Pair<string, string> To = new Pair<string, string>();
                if (ArrFunc.IsFilled(sendSmtp.MailMessage.To))
                    To = new Pair<string, string>(sendSmtp.MailMessage.To[0].DisplayName, sendSmtp.MailMessage.To[0].MailAddress);

                string[] IOTrackDatasIdent = new string[5] { "Host", "To_DisplayName", "To_MailAddress", "Subject", "Number of attachments" };
                string[] IOTrackDatas = new string[5] { sendSmtp.SmtpClient.Host, To.First, To.Second, 
                            sendSmtp.MailMessage.Subject.Value, ArrFunc.Count(sendSmtp.MailMessage.Attachment).ToString()};
                
                Nullable<int> IOTrackIdDataSource = source.First;
                string IOTrackDataSourceIdent = source.Second;
                
                string IOTrackStatus = string.Empty;
                string IOTrackMessageValue = string.Empty;

                string exceptionMessage = string.Empty;
                SmtpStatusCode statusCode = SmtpStatusCode.Ok;
                Boolean isException = false;

                try
                {
                    // RD 20141202 [20531] 
                    // Erreur:  Service not available, closing transmission channel. The server response was: 4.4.1 Connection timed out            
                    bool isToLoop = true;
                    int nbLoop = 0;
                    int maxLoop = 10; //nbr de tentatives max pour envoyer un mail lorsque un Timeout est rencontré
                    int warningLoop = 5; //nbr de tentatives à partir du quel un warning sera écrit dans le Log
                    int sleepTime = 1000;


                    string msgError = string.Empty;
                    while (isToLoop)
                    {
                        try
                        {
                            sendSmtp.SendEmail();

                            // Test EFS pour simuler l'erreur du Timeout SMTP
                            //if (true)
                            //if (nbLoop < 8)
                            //    throw new Exception("EFS Test: Service not available, closing transmission channel. The server response was: 4.4.1 Connection timed out");

                            isToLoop = false;
                            ProcessStateTools.StatusEnum status = ProcessStateTools.StatusEnum.NONE;

                            // Lors de l’envoi avec succès de l’email
                            if (StrFunc.IsFilled(msgError))
                            {
                                msgError += Cst.CrLf + StrFunc.AppendFormat("<b>Failed attempts: {0}.</b>", nbLoop);
                                msgError += Cst.CrLf + StrFunc.AppendFormat("<b>Email posted successfully.</b>", nbLoop);

                                // Au delà de {warningLoop} Timeout on n’est plus en INFO mais en WARNING 
                                if (nbLoop > warningLoop)
                                {
                                    status = ProcessStateTools.StatusEnum.WARNING;
                                    msgError += Cst.CrLf + "<b>- contact your system administrator.</b>";
                                    // FI 20200706 [XXXXX] call SetErrorWarning 
                                    ProcessState.SetErrorWarning(ProcessStateTools.StatusWarningEnum);
                                }

                                // FI 20200623 [XXXXX] SetErrorWarning
                                // FI 20200607 [XXXXX] Mise en commenatire
                                //m_Task.process.ProcessState.SetErrorWarning(status);
                                
                                
                                // FI 20200706 [XXXXX] Mise en commentaire
                                //m_ElementStatus = status;
                                Logger.Log(new LoggerData(LoggerTools.StatusToLogLevelEnum(status), msgError + Cst.CrLf + ArrFunc.GetStringList(logEmailSource, Cst.CrLf), 3));
                            }
                            else
                            {
                                
                                // FI 20200706[XXXXX] Mise en commentaire
                                //m_ElementStatus = status;

                                Logger.Log(new LoggerData(LoggerTools.StatusToLogLevelEnum(status), "<b>Email posted successfully</b>" + Cst.CrLf + ArrFunc.GetStringList(logEmailSource, Cst.CrLf), 3));
                            }
                        }
                        catch (Exception ex)
                        {
                            if (ex.Message.ToLower().Contains("connection timed out"))
                            {
                                nbLoop++;
                                isToLoop = (nbLoop < maxLoop);

                                //Préparation du message
                                if (nbLoop == 1)
                                {
                                    msgError = StrFunc.AppendFormat("Smtp error occurred: <b>{0}</b>", ex.Message);
                                    msgError += Cst.CrLf + StrFunc.AppendFormat("<b>Spheres® {0} to send email again ...</b>", isToLoop ? "tries" : "doesn't try");
                                }

                                if (isToLoop)
                                {
                                    System.Threading.Thread.Sleep(sleepTime);
                                }
                                else
                                {
                                    // FI 20200623 [XXXXX] SetErrorWarning
                                    // FI 20200706 [XXXXX] Mise en commentaire
                                    //m_Task.process.ProcessState.SetErrorWarning(ProcessStateTools.StatusErrorEnum);

                                    msgError += Cst.CrLf + StrFunc.AppendFormat("<b>Failed attempts: {0}.</b>", nbLoop);
                                    msgError += Cst.CrLf + "<b>The email could not be sent!</b>";
                                    msgError += Cst.CrLf + "<b>- contact your system administrator.</b>";
                                    
                                    Logger.Log(new LoggerData(LogLevelEnum.Error, msgError, 3));
                                    throw;
                                }
                            }
                            else
                            {
                                if (StrFunc.IsFilled(msgError))
                                {
                                    // FI 20200623 [XXXXX] SetErrorWarning
                                    // FI 20200706 [XXXXX] Mise en commentaire puisqu'il y a un throw 
                                    //m_Task.process.ProcessState.SetErrorWarning(ProcessStateTools.StatusErrorEnum);
                                    msgError += Cst.CrLf + StrFunc.AppendFormat("<b>Failed attempts: {0}.</b>", nbLoop);
                                    msgError += Cst.CrLf + "<b>The email could not be sent!</b>";
                                    msgError += Cst.CrLf + "<b>- contact your system administrator.</b>";

                                    
                                    // FI 20200706 [XXXXX] Mise en commentaire puisqu'il y a un throw 
                                    // m_ElementStatus = ProcessStateTools.StatusEnum.ERROR;

                                    Logger.Log(new LoggerData(LogLevelEnum.Error, msgError, 3));
                                }
                                throw;
                            }
                        }
                    }

                    #region IOTrack
                    IOTrackDatas[4] = sendSmtp.CountAttachement.ToString();
                    IOTrackMessageValue = "<b>Email posted successfully</b>" + Cst.CrLf + pSendFileMsg;
                    IOTrackStatus = Cst.ErrLevel.SUCCESS.ToString();
                    #endregion
                }
                catch (SmtpException ex)
                {
                    exceptionMessage = ex.Message;
                    isException = true;
                    statusCode = ex.StatusCode;
                    IOTrackStatus = Cst.ErrLevel.FAILURE.ToString();
                    throw new Exception("<b>Error to send email by Smtp</b>", ex);
                }
                catch (Exception ex)
                {
                    exceptionMessage = ex.Message;
                    isException = true;
                    statusCode = SmtpStatusCode.GeneralFailure;
                    IOTrackStatus = Cst.ErrLevel.FAILURE.ToString();
                    throw new Exception("<b>Error to send email by Smtp</b>", ex);
                }
                finally
                {
                    if (isException)
                    {
                        IOTrackMessageValue = "<b>Warning! No posted email</b>" + Cst.CrLf;
                        IOTrackMessageValue += StrFunc.AppendFormat("Exception : {0}, Status : {1}", exceptionMessage, statusCode.ToString()) + Cst.CrLf;
                        // PL 20210421 Implémentation d'un Try/Catch temporaire car m_Task.ProcessLog.header.IdProcess non disponible.
                        //             A VALIDER/AMENDER par PM à son retour
                        string idLog = string.Empty;
                        try { idLog = m_Task.Process.IdProcess.ToString(); }
                        catch { idLog = Cst.NotAvailable; }
                        IOTrackMessageValue += StrFunc.AppendFormat("Refer to log detail for more information (id: {0})", idLog);
                    }

                    m_Task.AddIOTrackLog(IOTrackMessageValue, IOTrackIdDataSource, IOTrackDataSourceIdent, IOTrackDatas, IOTrackDatasIdent, IOTrackStatus, IOTrackDescription);
                    pIsIOTrackWriten = true;
                }
            }
            catch (Exception ex)
            {
                throw new Exception(
                        @"<b>Error to send email</b>" + Cst.CrLf + ArrFunc.GetStringList(logEmailSource, Cst.CrLf), ex);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pXmlSendFD"></param>
        /// <param name="pSendFileMsg"></param>
        /// <param name="pIsIOTrackWriten"></param>
        private void SendFlyDoc(XmlDocument pXmlSendFD, string pSendFileMsg, ref bool pIsIOTrackWriten)
        {
            pIsIOTrackWriten = false;

            try
            {
                EFS_SerializeInfoBase serializeInfo = new EFS_SerializeInfoBase(null, pXmlSendFD.OuterXml)
                {
                    TypeDocument = typeof(EfsSendFlyDoc)
                };
                EfsSendFlyDoc sendFD = (EfsSendFlyDoc)CacheSerializer.Deserialize(serializeInfo);

                int? transportID = null;
                int state = 0;
                string codeReturn = null;
                string date = null;
                string error = null;
                string url = null;
                //
                #region IOTrack
                string[] IOTrackDatas = new string[5] { sendFD.Login.UserName, sendFD.Message.To.Name, sendFD.Message.To.Address, 
                            sendFD.Message.Subject.Value, ArrFunc.Count(sendFD.Message.Attachment).ToString()};
                //
                string IOTrackDescription = "Send " + sendFD.TransportName.ToString() + " by FlyDoc®";
                Nullable<int> IOTrackIdDataSource = Convert.ToInt32(m_Task.IoTask.parameters.param[0].Value);
                string IOTrackDataSourceIdent = m_Task.IoTask.parameters.param[0].name;
                string[] IOTrackDatasIdent = new string[5] { "UserName", "To_DisplayName", "To_Address", "Subject", "Number of attachments" };
                string IOTrackStatus = string.Empty;
                string IOTrackMessageValue = string.Empty;
                string IOTrackDataTargetIdent = "FlyDoc ID";
                #endregion
                //
                try
                {
                    Cst.ErrLevel lRet = sendFD.SendFlyDoc(ref transportID, ref state, ref codeReturn, ref date, ref url, ref error);
                    //
                    if (Cst.ErrLevel.SUCCESS == lRet)
                    {
                        #region IOTrack
                        IOTrackMessageValue = "<b>" + sendFD.TransportName.ToString() + " posted successfully</b>" + Cst.CrLf + pSendFileMsg;
                        IOTrackDescription += (StrFunc.IsFilled(date) ? " [" + date + "]" : string.Empty);
                        IOTrackStatus = state.ToString();
                        #endregion
                    }
                    else
                        throw new Exception(error);

                }
                catch (Exception ex)
                {
                    #region IOTrack
                    IOTrackMessageValue = "<b>Warning! No posted " + sendFD.TransportName.ToString() + "</b>" + Cst.CrLf + "Refer to 'Log detail' for more information";
                    IOTrackStatus = state.ToString();
                    #endregion
                    //
                    string logMessage = "<b>Error to send " + sendFD.TransportName.ToString() + " by FlyDoc®</b>" + Cst.CrLf;
                    throw new Exception(
                        logMessage + Cst.CrLf + ArrFunc.GetStringList(m_MsgLogInOut, Cst.CrLf), ex);
                }
                finally
                {
                    #region IOTrack
                    IOTrackStatus = state.ToString();
                    //
                    m_Task.AddIOTrackLog(IOTrackMessageValue, IOTrackIdDataSource, IOTrackDataSourceIdent, IOTrackDatas, IOTrackDatasIdent,
                            IOTrackStatus, transportID, IOTrackDataTargetIdent, codeReturn, IOTrackDescription, url, null, null);
                    //
                    pIsIOTrackWriten = true;
                    #endregion
                }
            }
            catch (Exception ex)
            {
                throw new Exception(
                    @"<b>Error to send message by FlyDoc®</b>" + Cst.CrLf + ArrFunc.GetStringList(m_MsgLogInOut, Cst.CrLf), ex);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        protected override void PrepareWriting()
        {
            #region IOTrack
            m_IOTrackDataTargetIdent = m_Out_dataStyle;
            m_IOTrackDescription = "Export of data";
            //
            m_NbWrittenRows = 0;
            m_NbRejectedRows = 0;
            m_NbIgnoredRows = 0;
            m_NbExportedFiles = 0;
            #endregion
            //
            #region Check Output Type
            m_IsPDFOutput = (m_Out_dataStyle == Cst.OutputTargetDataStyle.PDFFILE.ToString());
            m_IsXMLOutput = (m_Out_dataStyle == Cst.OutputTargetDataStyle.XMLFILE.ToString());
            //
            m_IsSendOutput = (m_Out_dataStyle == Cst.OutputTargetDataStyle.SENDSMTP.ToString()) ||
                             (m_Out_dataStyle == Cst.OutputTargetDataStyle.SENDFLYDOC.ToString());
            //
            m_IsParsingOutput = (m_Out_dataStyle == Cst.OutputTargetDataStyle.ANSIFILE.ToString()) ||
                                (m_Out_dataStyle == Cst.OutputTargetDataStyle.UNICODEFILE.ToString());
            //
            m_IsBINOutput = m_IsPDFOutput; // || m_IsIMAGEOutput (TODO)
            //
            bool isOutputTypeOK = (m_IsBINOutput || m_IsParsingOutput || m_IsSendOutput || m_IsXMLOutput); // || m_IsHTMLOutput (TODO)
            if (false == isOutputTypeOK)
            {
                throw new Exception(
                        @"<b>Out Data Style " + m_Out_dataStyle + @" is not supported.</b>" + Cst.CrLf + ArrFunc.GetStringList(m_MsgLogInOut, Cst.CrLf));
            }
            #endregion
            //
            #region Load Parsings
            m_OutputParsing = new OutputParsing(m_IdIOInOut, m_Task);
            if ((m_OutputParsing.ParsingCount == 0) && (false == m_IsBINOutput))
                throw new Exception(
                    @"<b>No enabled parsing specified</b>" + Cst.CrLf + ArrFunc.GetStringList(m_MsgLogInOut, Cst.CrLf));
            #endregion
            //
            m_XslParsing = m_OutputParsing.GetXSLParsingFile();
            //
            m_IsXslOutput = (StrFunc.IsFilled(m_XslParsing) || m_IsXMLOutput || m_IsSendOutput);// || m_IsHTMLOutput (TODO)
            
            if (m_IsXslOutput)
            {
                #region XSL file for Parsing
                if (StrFunc.IsEmpty(m_XslParsing) || m_XslParsing.ToLower().Contains("please insert"))
                {
                    throw new Exception(
                        @"<b>You forgot to specify the XSL Parsing File</b>" + Cst.CrLf +
                            @" - Verify it, and if necessary set either UNC (Universal Naming Convention) path or a full application-server-related path.
                                (eg: \\server\share\directory\sample.xsl or S:\directory\sample.xsl)" + Cst.CrLf + ArrFunc.GetStringList(m_MsgLogInOut, Cst.CrLf));
                }
                //
                if (false == File.Exists(m_XslParsing))
                {
                    string tmp_xslParsing = string.Empty;
                    bool isFound;
                    try
                    {
                        isFound = m_Task.Process.AppInstance.SearchFile2(m_Task.Cs, m_XslParsing, ref tmp_xslParsing);
                    }
                    catch { isFound = false; }

                    if (isFound)
                    {
                        m_XslParsing = tmp_xslParsing;
                    }
                    else
                    {
                        //On tente au cas où GetFile() soit bugguées...                        
                        m_XslParsing = m_Task.Process.AppInstance.GetFilepath(m_XslParsing);
                        isFound = File.Exists(m_XslParsing);
                    }
                    
                    if (!isFound)
                    {
                        throw new Exception(
                                "<b>Specified XSL Parsing file does not exist</b>" + Cst.CrLf +
                                @"- Verify it, and if necessary set either UNC (Universal Naming Convention) path or a full application-server-related path.
                                        (eg: \\server\share\directory\sample.xsl or S:\directory\sample.xsl)" + Cst.CrLf +
                                 "[Xsl file: " + m_XslParsing + "]" + Cst.CrLf + ArrFunc.GetStringList(m_MsgLogInOut, Cst.CrLf));
                    }

                }
                #endregion Fichier XSL du parsing
            }
            //
            #region Destination file
            if ((StrFunc.IsEmpty(m_Out_dataName) && (false == m_IsSendOutput)) ||
                m_Out_dataName.ToLower().Contains("please insert"))
            {
                throw new Exception(
                        @"<b>You forgot to specify the full pathname of the file to be exported</b>
                                - Verify it, and if necessary set either UNC (Universal Naming Convention) path or a full application-server-related path.
                                (eg: \\server\share\directory\sample.txt or S:\directory\sample.txt)" + Cst.CrLf + ArrFunc.GetStringList(m_MsgLogInOut, Cst.CrLf));
            }
            #endregion Fichier Destination
            //
            #region Check dynamic Path file
            m_ElementRowDataList = new ArrayList();
            // PM 20180219 [23824] IOTools => IOCommonTools
            m_Out_dataName = IOCommonTools.CheckDynamicString(m_Out_dataName, m_Task, false, ref m_ElementRowDataList);
            m_IsDynamicOutputDataName = ArrFunc.IsFilled(m_ElementRowDataList);
            #endregion
            //
            m_FilePathList = new ArrayList();
            //
            if (m_IsParsingOutput || m_IsBINOutput)
            {
                #region Prepare Parsing && BIN Outputs
                m_EnumWriteMode = (Cst.WriteMode)Enum.Parse(typeof(Cst.WriteMode), m_WriteMode, true);
                m_DbConnection = DataHelper.OpenConnection(m_Task.Cs);
                m_DbTransaction = DataHelper.BeginTran(m_DbConnection);
                //		
                m_DestFullPathForPDFFileExport = string.Empty;
                //
                if (false == (m_IsDynamicOutputDataName))
                {
                    string fileName = m_Task.Process.AppInstance.GetFilepath(m_Out_dataName);
                    // EG 20130724 Passage du Timeout de lecture du fichier de 60 à 15
                    m_Task.Process.CheckFolderFromFilePath(fileName, 15, 3);
                    FileInfo fInfo = new FileInfo(fileName);
                    //
                    if (m_IsBINOutput)
                    {
                        m_DestFullPathForPDFFileExport = fInfo.FullName;
                    }
                    else
                    {
                        //200906004 PL Change AddFolderSessionId.False to True
                        // PM 20180219 [23824] IOTools => IOCommonTools
                        //IOTools.OpenFile(fInfo.FullName, m_EnumWriteMode, m_Out_dataStyle, m_Task.appInstance.GetTemporaryDirectory(AppInstance.AddFolderSessionId.True));
                        IOCommonTools.OpenFile(fInfo.FullName, m_EnumWriteMode, m_Out_dataStyle, m_Task.Session.GetTemporaryDirectory(AppSession.AddFolderSessionId.True));
                    }
                }
                #endregion
            }
            //
            base.PrepareWriting();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pIsOk"></param>
        /// <param name="pIsPostMappingRowFilled"></param>
        protected override Cst.ErrLevel FinalizeWriting(bool pIsOk, bool pIsPostMappingRowFilled)
        {
            Cst.ErrLevel ret = Cst.ErrLevel.SUCCESS;

            if (m_IsParsingOutput || m_IsBINOutput)
            {
                // PM 20180219 [23824] IOTools => IOCommonTools
                //IOTools.CloseFile();
                IOCommonTools.CloseFile();

                if (null != m_DbTransaction)
                {
                    if (m_IsWritingError)
                    {
                        //PL 20151229 Use DataHelper.RollbackTran()
                        //m_DbTransaction. Rollback();
                        DataHelper.RollbackTran(m_DbTransaction, false);
                    }
                    else
                    {
                        //PL 20151229 Use DataHelper.CommitTran()
                        //m_DbTransaction. Commit();
                        DataHelper.CommitTran(m_DbTransaction, false);
                    }
                }

                if (null != m_DbConnection)
                    DataHelper.CloseConnection(m_DbConnection);

                #region IOTrack
                if (false == m_IsIOTrackWritten)
                {
                    m_NbExportedFiles += m_FilePathList.Count;
                    //
                    if (m_FilePathList.Count == 1)
                    {
                        // PM 20180219 [23824] IOTools => IOCommonTools
                        //m_IOTrackFileMsg += IOTools.GetFileMsg(m_FilePathList[0].ToString(), m_Task) + Cst.CrLf;
                        m_IOTrackFileMsg += IOCommonTools.GetFileMsg(m_FilePathList[0].ToString(), m_Task) + Cst.CrLf;
                    }
                    else
                    {
                        for (int i = 0; i < m_FilePathList.Count; i++)
                        {
                            // PM 20180219 [23824] IOTools => IOCommonTools
                            //m_IOTrackFileMsg += IOTools.GetFileMsg(m_FilePathList[i].ToString(), m_Task, i + 1) + Cst.CrLf;
                            m_IOTrackFileMsg += IOCommonTools.GetFileMsg(m_FilePathList[i].ToString(), m_Task, i + 1) + Cst.CrLf;
                        }
                    }
                }
                #endregion
            }

            #region IOTrack
            if (false == m_IsIOTrackWritten)
            {
                if (m_IsXslOutput || m_IsBINOutput)
                {
                    m_IOTrackDatasIdent = new string[2] { "Read rows from database", "Exported files" };
                    m_IOTrackDatas = new string[2] { m_NbReadRowsFromSource.ToString(), m_NbExportedFiles.ToString() };
                }
                else
                {
                    m_IOTrackDatasIdent = new string[5] { "Read rows from database", "Rejected rows by data controls", "Ignored rows by setting", "Written rows to destination", "Exported files" };
                    m_IOTrackDatas = new string[5] { m_NbReadRowsFromSource.ToString(), m_NbRejectedRows.ToString(), m_NbIgnoredRows.ToString(), m_NbWrittenRows.ToString(), m_NbExportedFiles.ToString() };
                }
            }
            #endregion

            base.FinalizeWriting(pIsOk, pIsPostMappingRowFilled);

            return ret;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pPassNumber"></param>
        /// <param name="pPassMessage"></param>
        /// <returns></returns>
        // RD 20150526 [20614] Modify
        /// EG 20220221 [XXXXX] Gestion IRQ
        protected override bool Writing(int pPassNumber, string pPassMessage)
        {
            ArrayList messageValue = null;
            Cst.ErrLevel ret = Cst.ErrLevel.SUCCESS;
            //            
            try
            {
                if (m_IsXslOutput)
                {
                    #region XSL transformation Output
                    if (IRQTools.IsIRQRequested(m_Task.Process, m_Task.Process.IRQNamedSystemSemaphore, ref ret))
                        return false;

                    #region Valoriser les SphereLib, SQL, ...
                    if (null != m_TaskPostMapping.taskDet.output.file.row)
                    {
                        foreach (IOTaskDetInOutFileRow row in m_TaskPostMapping.taskDet.output.file.row)
                        {
                            foreach (IOTaskDetInOutFileRowData data in row.data)
                            {
                                messageValue = new ArrayList
                                {
                                    $"Row ({row.id })",
                                    $"Data ({data.name})"
                                };
                                string dataValue = data.value;
                                //
                                if (TypeData.IsTypeXml(data.datatype) && (null != data.valueXML))
                                {
                                    dataValue = data.valueXML.Value;
                                    // PM 20180219 [23824] IOTools => IOCommonTools
                                    dataValue = IOCommonTools.SetDataRowToDynamicString(dataValue, m_Task, row, true);
                                }
                                else
                                {
                                    ProcessMapping.GetMappedDataValue(m_Task, m_Task.SetErrorWarning, data, null, ref dataValue, messageValue);
                                    // PM 20180219 [23824] IOTools => IOCommonTools
                                    dataValue = IOCommonTools.SetDataRowToDynamicString(dataValue, m_Task, row, true);
                                }
                                //
                                if (dataValue == null)
                                    dataValue = string.Empty;
                                //
                                if (TypeData.IsTypeXml(data.datatype) && (null != data.valueXML))
                                    data.valueXML.Value = dataValue;
                                else
                                    data.value = dataValue;
                                //
                                data.sql = null;
                                data.spheresLib = null;
                                data.Default = null;
                                data.Controls = null;
                            }
                        }
                    }
                    #endregion

                    if (IRQTools.IsIRQRequested(m_Task.Process, m_Task.Process.IRQNamedSystemSemaphore, ref ret))
                        return false;

                    #region Serialiser l'instance (m_TaskPostMapping) dans un StringBuilder sbXMLMapping
                    StringBuilder sbXMLMapping = null;
                    try
                    {
                        EFS_SerializeInfoBase serializeInfo = new EFS_SerializeInfoBase(typeof(IOTask), m_TaskPostMapping);
                        sbXMLMapping = CacheSerializer.Serialize(serializeInfo);
                    }
                    catch (Exception ex)
                    {
                        throw new Exception(
                            @"<b>Error to Serialize the result of mapping</b>" + Cst.CrLf +
                            " - Check XSL file of mapping" + Cst.CrLf + ArrFunc.GetStringList(m_MsgLogInOut, Cst.Space), ex);
                    }
                    #endregion Serialiser

                    if (IRQTools.IsIRQRequested(m_Task.Process, m_Task.Process.IRQNamedSystemSemaphore, ref ret))
                        return false;

                    #region Charger le flux XML dans un Document XMLMapping
                    XmlDocument xmlDocumentPostMapping = new XmlDocument();
                    try
                    {
                        xmlDocumentPostMapping.LoadXml(sbXMLMapping.ToString());
                    }
                    catch (Exception ex)
                    {
                        throw new Exception(
                            @"<b>Error to Load the result of mapping in XML Document</b>" + Cst.CrLf +
                            ArrFunc.GetStringList(m_MsgLogInOut, Cst.CrLf), ex);
                    }
                    #endregion

                    if (IRQTools.IsIRQRequested(m_Task.Process, m_Task.Process.IRQNamedSystemSemaphore, ref ret))
                        return false;

                    #region Convert the ValueXml CDATA to xml stream
                    try
                    {
                        // RD 20100407 / [16931] No CData element in Output Post-Reading XML
                        // RD 20110401 / [17379] Serialize mode : En mode LIGHT, utiliser les attributs de substitution
                        IOTools.RemoveCDATAFromValueXml(xmlDocumentPostMapping, "data", m_SerializeOverrides);
                    }
                    catch (Exception ex)
                    {
                        throw new Exception(
                            @"<b>Error to Convert ValueXML CDATA to XML stream</b>" + Cst.CrLf +
                            ArrFunc.GetStringList(m_MsgLogInOut, Cst.CrLf), ex);
                    }
                    #endregion

                    if (IRQTools.IsIRQRequested(m_Task.Process, m_Task.Process.IRQNamedSystemSemaphore, ref ret))
                        return false;

                    #region Ecrire dans le fichier destination
                    FileInfo fileDest = null;

                    if (m_IsDynamicOutputDataName)
                    {
                        ArrayList elementRowDataValueList = new ArrayList();

                        #region process elementRowDataValueList like DATA;xxx;value;ROW;xxx;value
                        foreach (XmlNode nodeRow in xmlDocumentPostMapping.SelectNodes("iotask/iotaskdet/iooutput/file/row"))
                        {
                            #region temp
                            string elementRowDataWithValue = string.Empty;
                            //
                            for (int i = 0; i < m_ElementRowDataList.Count; i++)
                            {
                                elementRowDataWithValue += m_ElementRowDataList[i].ToString() + ";";
                                string[] rowData = m_ElementRowDataList[i].ToString().Split(';');
                                //
                                if (rowData[0] == "DATA")
                                {
                                    string filtre = "data[@name='" + rowData[1] + "']";
                                    XmlNode dataNode = nodeRow.SelectSingleNode(filtre);
                                    elementRowDataWithValue += dataNode.InnerText + ";";
                                }

                                if (rowData[0] == "ROW")
                                    elementRowDataWithValue += nodeRow.Attributes.GetNamedItem(rowData[1]).Value + ";";
                            }

                            if (StrFunc.IsFilled(elementRowDataWithValue))
                            {
                                //PL 20210310 Ne plus utiliser TrimEnd()
                                //   TrimEnd() ne supprime pas que le seul dernier caractère, ce qui pose un problème en cas de dernière donnée vide.
                                //   Ex. si 3ème donnée vide -->  "Data1;Data2;;" --> après TrimEnd() --> "Data1;Data2" --> la 3ème donnée a disparu !
                                //elementRowDataWithValue = elementRowDataWithValue.TrimEnd(';');
                                elementRowDataWithValue = elementRowDataWithValue.Remove(elementRowDataWithValue.Length - 1); 
                            }

                            if (false == elementRowDataValueList.Contains(elementRowDataWithValue))
                                elementRowDataValueList.Add(elementRowDataWithValue);
                            #endregion
                        }
                        #endregion

                        // RD 20101130 / Refactoring pour correction bug [8699]
                        #region xmlDocumentPostMappingNew and node "file"
                        XmlDocument xmlDocumentPostMappingNew = new XmlDocument();
                        xmlDocumentPostMappingNew.LoadXml(xmlDocumentPostMapping.OuterXml);
                        XmlNode nodeFile = xmlDocumentPostMappingNew.SelectNodes("iotask/iotaskdet/iooutput/file")[0];
                        #endregion

                        for (int i = 0; i < elementRowDataValueList.Count; i++)
                        {
                            #region calculer le nom du fichier destination et son contenu
                            string rowConditionNew = string.Empty;
                            string newDestinationFilePath = m_Out_dataName;
                            //
                            #region constituer la condition de séléction des ROW
                            string[] rowData = elementRowDataValueList[i].ToString().Split(';');
                            for (int iNew = 0; iNew < rowData.Length; iNew += 3)
                            {
                                // RD 20150526 [20614] Use XMLTools.EncaseXpathString
                                if (rowData[iNew] == "DATA")
                                    rowConditionNew += "(count(data[@name='" + rowData[iNew + 1] + "' and text() = " + XMLTools.EncaseXpathString(rowData[iNew + 2]) + "]) > 0) and ";
                                // RD 20150526 [20614] Use XMLTools.EncaseXpathString
                                if (rowData[iNew] == "ROW")
                                    rowConditionNew += "(@" + rowData[iNew + 1] + "=" + XMLTools.EncaseXpathString(rowData[iNew + 2]) + ") and ";
                                //
                                newDestinationFilePath = newDestinationFilePath.Replace("{" + rowData[iNew] + ";" + rowData[iNew + 1] + "}", rowData[iNew + 2]);
                            }
                            //
                            if (StrFunc.IsFilled(rowConditionNew))
                                rowConditionNew = "[" + rowConditionNew.TrimEnd(" and ".ToCharArray()) + "]";
                            #endregion
                            //
                            #region supprimer tous les anciens noeuds enfants ROW
                            IOTools.XMLRemoveChildNodes(nodeFile);
                            #endregion
                            //  
                            // Ajouter tous les "Row" qui passent le filtre dynamique
                            foreach (XmlNode nodeRowDataNew in xmlDocumentPostMapping.SelectNodes("iotask/iotaskdet/iooutput/file/row" + rowConditionNew))
                                nodeFile.AppendChild(xmlDocumentPostMappingNew.ImportNode(nodeRowDataNew, true));
                            #endregion

                            if (nodeFile.ChildNodes.Count > 0)
                            {
                                fileDest = null;
                                //
                                ret = WriteXmlMapToDestination(xmlDocumentPostMappingNew, m_XslParsing, newDestinationFilePath, ref fileDest, ref m_IsIOTrackWritten);
                                if (ret == Cst.ErrLevel.SUCCESS && null != fileDest)
                                {
                                    #region IOTrack
                                    m_NbExportedFiles++;

                                    // PM 20180219 [23824] IOTools => IOCommonTools
                                    if (elementRowDataValueList.Count == 1)
                                    {
                                        //m_IOTrackFileMsg += IOTools.GetFileMsg(fileDest) + Cst.CrLf;
                                        m_IOTrackFileMsg += IOCommonTools.GetFileMsg(fileDest) + Cst.CrLf;
                                    }
                                    else
                                    {
                                        //m_IOTrackFileMsg += IOTools.GetFileMsg(fileDest, m_NbExportedFiles) + Cst.CrLf;
                                        m_IOTrackFileMsg += IOCommonTools.GetFileMsg(fileDest, m_NbExportedFiles) + Cst.CrLf;
                                    }
                                    #endregion
                                }
                            }
                        }
                    }
                    else
                    {
                        ret = WriteXmlMapToDestination(xmlDocumentPostMapping, m_XslParsing, m_Out_dataName, ref fileDest, ref m_IsIOTrackWritten);
                        if (ret == Cst.ErrLevel.SUCCESS && null != fileDest)
                        {
                            #region IOTrack
                            m_NbExportedFiles++;
                            //
                            // PM 20180219 [23824] IOTools => IOCommonTools
                            //m_IOTrackFileMsg += IOTools.GetFileMsg(fileDest) + Cst.CrLf;
                            m_IOTrackFileMsg += IOCommonTools.GetFileMsg(fileDest) + Cst.CrLf;
                            #endregion
                        }
                    }
                    #endregion
                    #endregion
                }
                else if (m_IsParsingOutput || m_IsBINOutput)
                {
                    #region Parsing Output && BINary Output
                    //
                    try
                    {
                        string Line = string.Empty;
                        string titleLine = string.Empty;
                        bool isTitleLineProcessed = false;
                        bool isTitleLineWrote = false;
                        //
                        Cst.WriteMode newEnumWriteMode = m_EnumWriteMode;
                        string newFilepath = m_Out_dataName;
                        string oldFilepath = string.Empty;
                        //
                        if (null != m_TaskPostMapping.taskDet.output.file.row)
                        {
                            // RD 20110307 [17339] Output: XML to PDF                             
                            string temporaryDirectory = m_Task.Session.GetTemporaryDirectory(AppSession.AddFolderSessionId.True) + @"\";
                            XmlDocument doc = new XmlDocument();
                            //
                            foreach (IOTaskDetInOutFileRow row in m_TaskPostMapping.taskDet.output.file.row)
                            {
                                string rowLogInfoMsg = (null != row.logInfo ? "[" + row.logInfo.message + "]" : string.Empty);
                                rowLogInfoMsg = (StrFunc.IsFilled(rowLogInfoMsg) ? Cst.CrLf : string.Empty) + rowLogInfoMsg + Cst.CrLf + ArrFunc.GetStringList(m_MsgLogInOut, Cst.CrLf);

                                if (IRQTools.IsIRQRequested(m_Task.Process, m_Task.Process.IRQNamedSystemSemaphore, ref ret))
                                    break;

                                #region Vérifier si des données sont générées pour cette ligne
                                if (null == row.data)
                                {
                                    messageValue = new ArrayList
                                    {
                                        $"<b>No data generated for row :{row.id} in PostMapping file.</b>",
                                        rowLogInfoMsg
                                    };
                                    throw new Exception(ArrFunc.GetStringList(messageValue, Cst.CrLf));
                                }
                                #endregion Vérifier les données

                                if (m_IsDynamicOutputDataName)
                                {
                                    #region Ouvrir le bon fichier
                                    // PM 20180219 [23824] IOTools => IOCommonTools
                                    newFilepath = IOCommonTools.SetDataRowToDynamicString(m_Out_dataName, m_Task, row);
                                    //
                                    newEnumWriteMode = m_EnumWriteMode;
                                    //
                                    if (m_FilePathList.Contains(newFilepath))
                                    {
                                        if (m_IsBINOutput)
                                            throw new Exception(
                                                    @"<b>Risk of overwriting of data. Multiple " + m_Out_dataStyle + " files are to write to the same path (" + newFilepath + ")</b>" + rowLogInfoMsg);
                                        else
                                            newEnumWriteMode = Cst.WriteMode.APPEND;
                                    }
                                    //
                                    if (newFilepath != oldFilepath)
                                    {
                                        // PM 20180219 [23824] IOTools => IOCommonTools
                                        //IOTools.CloseFile();
                                        IOCommonTools.CloseFile();
                                        //
                                        oldFilepath = newFilepath;
                                        //
                                        FileInfo fInfo = new FileInfo(newFilepath);
                                        if (false == fInfo.Directory.Exists)
                                        {
                                            fInfo = new FileInfo(m_Task.Process.AppInstance.GetFilepath(newFilepath));
                                            if (false == fInfo.Directory.Exists)
                                                throw new Exception(
                                                        @"<b>Specified Folder (" + fInfo.Directory.FullName + ") of Destination File (" + fInfo.FullName + @") does not exist.</b>" + rowLogInfoMsg + @"
                                                Verify it, and if necessary set either UNC (Universal Naming Convention) path or a full application-server-related path.
                                                (eg: \\server\share\directory\sample.txt or S:\directory\sample.txt)");
                                        }
                                        //
                                        if (m_IsBINOutput)
                                            m_DestFullPathForPDFFileExport = fInfo.FullName;
                                        else
                                        {
                                            //200906004 PL Change AddFolderSessionId.False to True
                                            // PM 20180219 [23824] IOTools => IOCommonTools
                                            //IOTools.OpenFile(fInfo.FullName, newEnumWriteMode, m_Out_dataStyle, m_Task.appInstance.GetTemporaryDirectory(AppInstance.AddFolderSessionId.True));
                                            IOCommonTools.OpenFile(fInfo.FullName, newEnumWriteMode, m_Out_dataStyle, m_Task.Session.GetTemporaryDirectory(AppSession.AddFolderSessionId.True));
                                            //
                                            isTitleLineWrote = fInfo.Exists && (Cst.WriteMode.WRITE != newEnumWriteMode);
                                        }
                                    }
                                    #endregion
                                }
                                else
                                {
                                    if (m_FilePathList.Contains(newFilepath))
                                    {
                                        if (m_IsBINOutput)
                                            throw new Exception(
                                                    @"<b>Risk of overwriting of data. Multiple " + m_Out_dataStyle + " files are to write to the same path (" + newFilepath + ")</b>" + rowLogInfoMsg);
                                        else
                                        {
                                            FileInfo fInfo = new FileInfo(newFilepath);
                                            if (false == fInfo.Directory.Exists)
                                                fInfo = new FileInfo(m_Task.Process.AppInstance.GetFilepath(newFilepath));
                                            //
                                            // PM 20180219 [23824] IOTools => IOCommonTools
                                            //IOTools.CloseFile();
                                            ////200906004 PL Change AddFolderSessionId.False to True
                                            //IOTools.OpenFile(fInfo.FullName, Cst.WriteMode.APPEND, m_Out_dataStyle, m_Task.appInstance.GetTemporaryDirectory(AppInstance.AddFolderSessionId.True));
                                            IOCommonTools.CloseFile();
                                            IOCommonTools.OpenFile(fInfo.FullName, Cst.WriteMode.APPEND, m_Out_dataStyle, m_Task.Session.GetTemporaryDirectory(AppSession.AddFolderSessionId.True));
                                            //
                                            isTitleLineWrote = true;
                                        }
                                    }
                                }

                                if (false == m_IsBINOutput)
                                {
                                    #region Ligne Titre
                                    if (BoolFunc.IsTrue(m_IsHeaderColumn) && (false == isTitleLineProcessed))
                                    {
                                        #region Créer La ligne Titre
                                        ret = m_OutputParsing.MakeLine(row, ref titleLine, true);
                                        //
                                        if (ret != Cst.ErrLevel.SUCCESS)
                                        {
                                            m_IsWritingError = true;
                                            
                                            Logger.Log(new LoggerData(LogLevelEnum.Info, "<b>Error to create Title Line.</b>" + rowLogInfoMsg));
                                        }
                                        //
                                        isTitleLineProcessed = true;
                                        #endregion
                                    }
                                    //
                                    if (isTitleLineProcessed && (false == isTitleLineWrote))
                                    {
                                        #region Ecrire La ligne Titre
                                        //20091209 PL
                                        //IOTools.StreamWriter.WriteLine(titleLine);
                                        // PM 20180219 [23824] IOTools => IOCommonTools
                                        //IOTools.StreamWriter.Write(titleLine);
                                        IOCommonTools.StreamWriter.Write(titleLine);
                                        // 
                                        isTitleLineWrote = true;
                                        #endregion
                                    }
                                    #endregion
                                }

                                #region Ecrire La ligne
                                if (ret == Cst.ErrLevel.SUCCESS)
                                {
                                    if (m_IsBINOutput)
                                    {
                                        #region Write PDF
                                        bool isDestFileAlreadyWriten = false;
                                        //
                                        foreach (IOTaskDetInOutFileRowData data in row.data)
                                        {
                                            // RD 20110307 [17339] Output: XML to PDF 
                                            //•	créer un flux binaire en utilisant FopEngine, et l’écrire dans un fichier temporaire
                                            //•	On note le chemin du fichier temporaire dans la valeur de l’élément « DATA » 
                                            //•	Mettre datatype=IMAGE, pour pouvoir continuer avec l’étape suivante ci-dessous
                                            if (m_IsPDFOutput &&
                                                (data.datatype.ToLower() == (TypeData.TypeDataEnum.xml.ToString() + "fo")))
                                            {
                                                try
                                                {
                                                    string fileFullPath = temporaryDirectory + m_IdIOInOut + "_" + row.id + "_" + data.name + "_" + outputSessionId + @".dat";
                                                    // EG 20160404 Migration vs2013
                                                    //byte[] fileContent = FopEngine_V2.TransformToByte(m_Task.Cs, data.valueXML.Data, temporaryDirectory);
                                                    //if (Cst.ErrLevel.SUCCESS == FileTools.WriteBytesToFile(fileContent, fileFullPath, true))
                                                    //{
                                                    //    data.value = doc.CreateCDataSection(fileFullPath).Value;
                                                    //    data.datatype = TypeData.TypeDataEnum.image.ToString();
                                                    //}
                                                    if (Cst.ErrLevel.SUCCESS == FopEngine.WritePDF(m_Task.Cs, data.valueXML.Data, temporaryDirectory, fileFullPath))
                                                    {
                                                        data.value = doc.CreateCDataSection(fileFullPath).Value;
                                                        data.datatype = TypeData.TypeDataEnum.image.ToString();
                                                    }
                                                }
                                                catch (Exception ex)
                                                {
                                                    throw new Exception(
                                                            @"<b>Error to create " + ElementIdentifiersForMessage + " flow from XML flow.</b> " + Cst.CrLf +
                                                            @"[Data: " + data.name + ", Row: " + row.id + "]" + Cst.CrLf +
                                                            rowLogInfoMsg, ex);
                                                }
                                            }
                                            //
                                            // Le test sur Cst.OutputTargetDataStyle.PDFFILE est pour compatibilité ascendante avec les xsl de la version v2.2                                            
                                            if (TypeData.IsTypeImage(data.datatype) ||
                                                (data.name == Cst.OutputTargetDataStyle.PDFFILE.ToString()))
                                            {
                                                if (false == isDestFileAlreadyWriten)
                                                {
                                                    #region Move PDF
                                                    //
                                                    string sourceFullPathForPDFFileExport = data.value;
                                                    //
                                                    #region Verify path
                                                    if (StrFunc.IsEmpty(sourceFullPathForPDFFileExport))
                                                        throw new Exception(
                                                            @"<b>No " + ElementIdentifiersForMessage + " temporary file to export.</b>" + Cst.CrLf +
                                                            @"[Row: " + row.id + "]" + Cst.CrLf +
                                                            rowLogInfoMsg + Cst.CrLf + @"- <b>Please check Parsing parameters and XSL Mapping file.</b>");
                                                    //
                                                    if (false == File.Exists(sourceFullPathForPDFFileExport))
                                                    {
                                                        sourceFullPathForPDFFileExport = m_Task.Process.AppInstance.GetFilepath(sourceFullPathForPDFFileExport);
                                                        //
                                                        if (false == File.Exists(sourceFullPathForPDFFileExport))
                                                            throw new Exception(
                                                                @"<b>" + ElementIdentifiersForMessage + " temporary file does not exist.</b>" + Cst.CrLf +
                                                                @"[Row  : " + row.id + "]" + Cst.CrLf +
                                                                @"[File : " + sourceFullPathForPDFFileExport + "]" + Cst.CrLf +
                                                                rowLogInfoMsg + Cst.CrLf + @"- <b>Please check Parsing parameters and XSL Mapping file.</b>");
                                                    }
                                                    #endregion
                                                    //
                                                    try
                                                    {
                                                        // RD 20110630 [17339] Ecrasement du fichier exsitant
                                                        if (File.Exists(m_DestFullPathForPDFFileExport))
                                                        {
                                                            switch (m_EnumWriteMode)
                                                            {
                                                                case Cst.WriteMode.APPEND:
                                                                    throw new Exception(
                                                                        "<b>''Append'' mode not supported with a " + ElementIdentifiersForMessage + " file. Please modify the characteristics of the export to select the ''Write'' mode.</b>");

                                                                case Cst.WriteMode.WRITE:
                                                                    File.Delete(m_DestFullPathForPDFFileExport);
                                                                    break;
                                                            }
                                                        }
                                                        //
                                                        File.Move(sourceFullPathForPDFFileExport, m_DestFullPathForPDFFileExport);
                                                        File.Delete(sourceFullPathForPDFFileExport);
                                                    }
                                                    catch (Exception ex)
                                                    {
                                                        throw new Exception(
                                                            @"<b>Error to create " + ElementIdentifiersForMessage + " destination file.</b> " + Cst.CrLf +
                                                            @"[Source     : " + sourceFullPathForPDFFileExport + "]" + Cst.CrLf +
                                                            @"[Destination: " + m_DestFullPathForPDFFileExport + "]" +
                                                            rowLogInfoMsg, ex);
                                                    }
                                                    #endregion
                                                    //
                                                    isDestFileAlreadyWriten = true;
                                                }
                                                else
                                                {
                                                    
                                                    Logger.Log(new LoggerData(LogLevelEnum.Info, "<b>Multiple " + m_Out_dataStyle + " datas in PotsMapping XML stream. Only the first one is Exported</b>" + rowLogInfoMsg));
                                                }
                                            }
                                        }
                                        //
                                        if (isDestFileAlreadyWriten == false)
                                        {
                                            throw new Exception(
                                                @"<b>Error to create " + ElementIdentifiersForMessage + " destination file.</b> " + Cst.CrLf +
                                                @"[Destination: " + m_DestFullPathForPDFFileExport + "]" +
                                                rowLogInfoMsg + Cst.CrLf + @"- <b>None source file exists. Please check your XSL file of Mapping.</b>");
                                        }
                                        #endregion
                                    }
                                    else
                                    {
                                        ret = m_OutputParsing.MakeLine(row, ref Line);
                                        //
                                        if (ret == Cst.ErrLevel.SUCCESS)
                                        {
                                            //20091209 PL
                                            //IOTools.StreamWriter.WriteLine(currentLine);
                                            // PM 20180219 [23824] IOTools => IOCommonTools
                                            //IOTools.StreamWriter.Write(Line);
                                            IOCommonTools.StreamWriter.Write(Line);
                                        }
                                    }
                                    //
                                    m_NbWrittenRows++;
                                    //
                                    if (null != row.sql)
                                        row.sql.Exec(m_Task.Cs, m_DbTransaction);
                                }
                                else
                                {
                                    m_IsWritingError = true;
                                    messageValue = new ArrayList
                                    {
                                        $"<b>Row ({row.id}) : not exported</b>"
                                    };
                                    LogLevelEnum logLevel = LogLevelEnum.Info;
                                    if (ret == Cst.ErrLevel.DATAIGNORE)
                                    {
                                        m_NbIgnoredRows++;
                                        messageValue.Add("<b>[ignored by parsings]</b>");
                                    }
                                    else if (ret == Cst.ErrLevel.DATAREJECTED)
                                    {
                                        m_NbRejectedRows++;
                                        messageValue.Add("<b>[rejected by data controls]</b>");
                                    }
                                    else if (ret == Cst.ErrLevel.IRQ_EXECUTED)
                                    {
                                        messageValue.Add(@"<b>[interruption request executed]</b>");
                                        logLevel = LogLevelEnum.Warning;
                                    }

                                    messageValue.Add(rowLogInfoMsg);
                                    
                                    Logger.Log(new LoggerData(logLevel, ArrFunc.GetStringList(messageValue, Cst.Space), 3));
                                }
                                #endregion

                                if (m_IsWritingError)
                                    break;
                                else if (false == m_FilePathList.Contains(newFilepath))
                                    m_FilePathList.Add(newFilepath);
                            }
                        }
                    }
                    catch (Exception)
                    {
                        m_IsWritingError = true;
                        throw;
                    }
                    #endregion ANSI and UNICODE Files
                }
                return (Cst.ErrLevel.SUCCESS == ret);
            }
            catch (Exception ex)
            {
                // Remarque dans le FinalizeWriting une exeption est générée
                
                m_IsWritingError = true;

                // FI 20200623 [XXXXX] SetErrorWarning
                // FI 20200706 [XXXXX] Mise en commentaire puisque m_IsWritingError = true
                //m_Task.process.ProcessState.SetErrorWarning(ProcessStateTools.StatusErrorEnum);
                // FI 20190829 [XXXXX] Ecriture dans le log Spheres du message d'erreur
                string msg = ExceptionTools.GetMessageExtended(ex);

                
                // FI 20200706 [XXXXX] Mise en commentaire puisque m_IsWritingError = true
                //m_ElementStatus = ProcessStateTools.StatusEnum.ERROR;
                Logger.Log(new LoggerData(LogLevelEnum.Error, msg, 3));
                // FI 20190829 [XXXXX] Ecriture dans la trace du message Complet (avec pile des appels)
                AppInstance.TraceManager.TraceError(this, ExceptionTools.GetMessageAndStackExtended(ex));

                return false;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        /// RD 20110401 [17379] Serialize mode : Add column SERIALIZEMODE
        // EG 20180426 Analyse du code Correction [CA2202]
        protected override IDataReader GetInOutData()
        {
            //string sqlQuery = SQLCst.SQL_ANSI + Cst.CrLf;
            //sqlQuery += SQLCst.SELECT + "DISPLAYNAME, DESCRIPTION, LOGLEVEL, COMMITMODE, DATASTYLE, WRITEMODE, DATACONNECTION, ";
            //sqlQuery += "DATANAME, DATASECTIONSTART, DATASECTIONEND, ISHEADERCOLUMN, XSLMAPPING, NBCOLUMNBYROWMAP, " + Cst.CrLf;
            //sqlQuery += "OUT_DATASTYLE, OUT_DATACONNECTION, OUT_DATANAME, SERIALIZEMODE" + Cst.CrLf;
            //sqlQuery += SQLCst.FROM_DBO + Cst.OTCml_TBL.IOOUTPUT + Cst.CrLf;
            //sqlQuery += SQLCst.WHERE + " IDIOOUTPUT = " + DataHelper.SQLString(m_IdIOInOut) + Cst.CrLf;
            //sqlQuery += SQLCst.AND + OTCmlHelper.GetSQLDataDtEnabled(m_Task.Cs, Cst.OTCml_TBL.IOOUTPUT);
            //IDataReader drOutput = DataHelper.ExecuteReader(m_Task.Cs, CommandType.Text, sqlQuery);
            //return drOutput;
            return IOTaskTools.TaskOutputDataReader(m_Task.Cs, m_IdIOInOut);
        }

        // EG 20180426 Analyse du code Correction [CA2202]
        protected override QueryParameters GetInOutQuery()
        {
            //string sqlQuery = SQLCst.SQL_ANSI + Cst.CrLf;
            //sqlQuery += SQLCst.SELECT + "DISPLAYNAME, DESCRIPTION, LOGLEVEL, COMMITMODE, DATASTYLE, WRITEMODE, DATACONNECTION, ";
            //sqlQuery += "DATANAME, DATASECTIONSTART, DATASECTIONEND, ISHEADERCOLUMN, XSLMAPPING, NBCOLUMNBYROWMAP, " + Cst.CrLf;
            //sqlQuery += "OUT_DATASTYLE, OUT_DATACONNECTION, OUT_DATANAME, SERIALIZEMODE" + Cst.CrLf;
            //sqlQuery += SQLCst.FROM_DBO + Cst.OTCml_TBL.IOOUTPUT + Cst.CrLf;
            //sqlQuery += SQLCst.WHERE + " IDIOOUTPUT = " + DataHelper.SQLString(m_IdIOInOut) + Cst.CrLf;
            //sqlQuery += SQLCst.AND + OTCmlHelper.GetSQLDataDtEnabled(m_Task.Cs, Cst.OTCml_TBL.IOOUTPUT);
            //return sqlQuery;
            QueryParameters queryParameters = IOTaskTools.TaskOutputQuery(m_Task.Cs, m_IdIOInOut);
            return queryParameters;
        }

        /// <summary>
        /// Supprime la CS présente dans un flux XML qui représente un MQueue
        /// </summary>
        /// <param name="pData">Représente le msg Queue au format XML</param>
        /// <returns></returns>
        /// FI 20131119 [] Add Methode
        private static string EraseCSInMqueue(string pData)
        {
            // Load XML
            XmlDocument xmlData = new XmlDocument();
            xmlData.LoadXml(pData);

            XmlNode nodeHeader = xmlData.SelectSingleNode("//header");
            if (null != nodeHeader)
            {
                XmlNode nodeCS = nodeHeader.SelectSingleNode("connectionString");
                if ((null != nodeCS) && nodeCS.ChildNodes.Count > 0)
                    nodeCS.ChildNodes[0].Value = string.Empty;
            }
            string ret = xmlData.OuterXml;
            return ret;
        }

        #endregion Methods
    }
}
