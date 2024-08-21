using EFS.ACommon;
using EFS.Common.IO.Interface;
using EFS.Common.Log;
using EFS.LoggerClient;
using EFS.LoggerClient.LoggerService;
using EfsML.DynamicData;
using System;
using System.Collections;
using System.Data;

namespace EFS.Common.IO
{
    /// <summary>
    /// 
    /// </summary>
    // PM 20180219 [23824] Déplacé dans CommonIO à partir de SpheresIO
    public class ProcessMapping
    {
        #region Members
        private static string _cs;
        private static int _userId;
        private static IOTask _xmlTask;
        private static IDbTransaction _dbTransaction;
        // PM 20180219 [23824] Remplacement du type Task par l'interface IIOTaskLaunching
        //private static Task _task;
        private static IIOTaskLaunching _task;
        //
        #endregion Members

        #region Methods

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pXMLTask"></param>
        /// <param name="pSetErrorWarning">Delegue pour inscrire un warning ou une erreur</param>
        /// <param name="pMappedData"></param>
        /// <param name="pDbTransaction"></param>
        /// <param name="pColumnValue"></param>
        /// RD 20100305 / Optimisation
        /// PM 20180219 [23824] Remplacement du type Task par l'interface IIOTaskLaunching
        /// FI 20201221 [XXXXX] Add pSetErrorWarning

        public static void GetMappedDataValue(IIOTaskLaunching pXMLTask, SetErrorWarning pSetErrorWarning, IOMappedData pMappedData, IDbTransaction pDbTransaction, ref string pColumnValue)
        {
            string resultAction = string.Empty;
            GetMappedDataValue(pXMLTask, pSetErrorWarning, pMappedData, pDbTransaction, IOCommonTools.SqlAction.NA, ref pColumnValue, ref resultAction, null);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pXMLTask"></param>
        /// <param name="pSetErrorWarning">Delegue pour inscrire un warning ou une erreur</param>
        /// <param name="pMappedData"></param>
        /// <param name="pDbTransaction"></param>
        /// <param name="pColumnValue"></param>
        /// <param name="pResultAction"></param>
        /// PM 20180219 [23824] Remplacement du type Task par l'interface IIOTaskLaunching
        /// FI 20201221 [XXXXX] Add pSetErrorWarning
        public static void GetMappedDataValue(IIOTaskLaunching pXMLTask, SetErrorWarning pSetErrorWarning, IOMappedData pMappedData, IDbTransaction pDbTransaction, ref string pColumnValue, ref string pResultAction)
        {
            GetMappedDataValue(pXMLTask, pSetErrorWarning, pMappedData, pDbTransaction, IOCommonTools.SqlAction.NA, ref pColumnValue, ref pResultAction, null);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pXMLTask"></param>
        /// <param name="pSetErrorWarning"></param>
        /// <param name="pMappedData"></param>
        /// <param name="pDbTransaction"></param>
        /// <param name="pActionId"></param>
        /// <param name="pColumnValue"></param>
        /// <param name="pResultAction"></param>
        /// PM 20180219 [23824] Remplacement du type Task par l'interface IIOTaskLaunching
        /// FI 20201221 [XXXXX] Add pSetErrorWarning
        public static void GetMappedDataValue(IIOTaskLaunching pXMLTask, SetErrorWarning pSetErrorWarning, IOMappedData pMappedData, IDbTransaction pDbTransaction, IOCommonTools.SqlAction pActionId, ref string pColumnValue, ref string pResultAction)
        {
            GetMappedDataValue(pXMLTask, pSetErrorWarning, pMappedData, pDbTransaction, pActionId, ref pColumnValue, ref pResultAction, null);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pXMLTask"></param>
        /// <param name="pSetErrorWarning">Delegue pour inscrire un warning ou une erreur</param>
        /// <param name="pMappedData"></param>
        /// <param name="pDbTransaction"></param>
        /// <param name="pColumnValue"></param>
        /// <param name="pMessage"></param>
        /// PM 20180219 [23824] Remplacement du type Task par l'interface IIOTaskLaunching
        /// FI 20201221 [XXXXX] Add pSetErrorWarning
        public static void GetMappedDataValue(IIOTaskLaunching pXMLTask, SetErrorWarning pSetErrorWarning, IOMappedData pMappedData, IDbTransaction pDbTransaction, ref string pColumnValue, ArrayList pMessage)
        {
            string resultAction = string.Empty;
            GetMappedDataValue(pXMLTask, pSetErrorWarning, pMappedData, pDbTransaction, IOCommonTools.SqlAction.NA, ref pColumnValue, ref resultAction, pMessage);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pXMLTask"></param>
        /// <param name="pSetErrorWarning">Delegue pour inscrire un warning ou une erreur</param>
        /// <param name="pMappedData"></param>
        /// <param name="pDbTransaction"></param>
        /// <param name="pActionId"></param>
        /// <param name="pColumnValue"></param>
        /// <param name="pResultAction"></param>
        /// <param name="pMessage"></param>
        /// PM 20180219 [23824] Remplacement du type Task par l'interface IIOTaskLaunching
        /// FI 20201221 [XXXXX] Add pSetErrorWarning
        public static void GetMappedDataValue(IIOTaskLaunching pXMLTask, SetErrorWarning pSetErrorWarning, IOMappedData pMappedData, IDbTransaction pDbTransaction, IOCommonTools.SqlAction pActionId, ref string pColumnValue, ref string pResultAction, ArrayList pMessage)
        {
            bool isInsertUpdateControl = false;
            GetMappedDataValue(pXMLTask, pSetErrorWarning, pMappedData, pDbTransaction, pActionId, ref pColumnValue, ref pResultAction, ref isInsertUpdateControl, false, pMessage);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pXMLTask"></param>
        /// <param name="pSetErrorWarning">Delegue pour inscrire un warning ou une erreur</param>
        /// <param name="pMappedData"></param>
        /// <param name="pDbTransaction"></param>
        /// <param name="pActionId"></param>
        /// <param name="pColumnValue"></param>
        /// <param name="pResultAction"></param>
        /// <param name="pIsInsertUpdateControl"></param>
        /// <param name="pMessage"></param>
        /// PM 20180219 [23824] Remplacement du type Task par l'interface IIOTaskLaunching
        /// FI 20201221 [XXXXX] Add pSetErrorWarning
        public static void GetMappedDataValue(IIOTaskLaunching pXMLTask, SetErrorWarning pSetErrorWarning, IOMappedData pMappedData, IDbTransaction pDbTransaction, IOCommonTools.SqlAction pActionId, ref string pColumnValue, ref string pResultAction, ref bool pIsInsertUpdateControl, ArrayList pMessage)
        {
            GetMappedDataValue(pXMLTask, pSetErrorWarning, pMappedData, pDbTransaction, pActionId, ref pColumnValue, ref pResultAction, ref pIsInsertUpdateControl, false, pMessage);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pXMLTask"></param>
        /// <param name="pSetErrorWarning">Delegue pour inscrire un warning ou une erreur</param>
        /// <param name="pMappedData"></param>
        /// <param name="pDbTransaction"></param>
        /// <param name="pActionId"></param>
        /// <param name="pColumnValue"></param>
        /// <param name="pResultAction"></param>
        /// <param name="pIsInsertUpdateControl"></param>
        /// <param name="pIsOnlyControl"></param>
        /// <param name="pMessage"></param>
        /// PM 20180219 [23824] Remplacement du type Task par l'interface IIOTaskLaunching
        /// FI 20201221 [XXXXX] Add pSetErrorWarning        
        public static void GetMappedDataValue(IIOTaskLaunching pXMLTask, SetErrorWarning pSetErrorWarning, IOMappedData pMappedData, IDbTransaction pDbTransaction,
            IOCommonTools.SqlAction pActionId, ref string pColumnValue, ref string pResultAction, ref bool pIsInsertUpdateControl,
            bool pIsOnlyControl, ArrayList pMessage)
        {
            try
            {
                _task = pXMLTask;
                _cs = _task.Cs;
                // PM 20180219 [23824] Utilisation de l'interface IIOTaskLaunching
                //_userId = _task.process.UserId;
                _userId = _task.UserId;
                _xmlTask = _task.IoTask;
                _dbTransaction = pDbTransaction;

                string controlValue = string.Empty;
                string defaultValue = string.Empty;
                bool isAction_IU = (pActionId == IOCommonTools.SqlAction.IU);
                //
                try
                {
                    #region GetDataValue
                    if (false == pIsOnlyControl)
                    {
                        pColumnValue = GetDataValue(pMappedData);
                        if (isAction_IU)
                            pMappedData.ValueVerified = pColumnValue;
                    }
                    else
                        pColumnValue = pMappedData.ValueVerified;
                    #endregion
                }
                catch (Exception ex)
                {
                    throw new Exception(ArrFunc.GetStringList(pMessage, Cst.Space) + Cst.CrLf + "<b>Valuation error</b>", ex);
                }
                //
                try
                {
                    #region Get Default Value
                    if (false == pIsOnlyControl)
                    {
                        if (null != pMappedData.Default)
                        {
                            defaultValue = GetDataValue(pMappedData.Default);
                            //
                            if (isAction_IU)
                                pMappedData.DefaultVerified = defaultValue;
                        }
                    }
                    else
                        defaultValue = pMappedData.DefaultVerified;
                    #endregion
                }
                catch (Exception ex)
                {
                    throw new Exception(ArrFunc.GetStringList(pMessage, Cst.Space) + Cst.CrLf + "<b>Valuation with default value error</b>", ex);
                }
                //
                try
                {
                    #region Apply Controls
                    if (null != pMappedData.Controls)
                    {
                        if (ArrFunc.IsFilled(pMappedData.Controls.Control))
                        {
                            bool isRowrejected = false;
                            //
                            for (int i = 0; i < pMappedData.Controls.Control.Length; i++)
                            {
                                if (isRowrejected)
                                {
                                    //20080310 PL Si "isRowrejected == true", on ne traite plus les éventuels contrôles suivants
                                    break;
                                }
                                //
                                string resultAction = pMappedData.Controls.Control[i].action.ToUpper();
                                //
                                if (false == pIsOnlyControl)
                                {
                                    if (null == pMappedData.Controls.Control[i].value)
                                    {
                                        pMappedData.Controls.Control[i].dataformat = pMappedData.dataformat;
                                        pMappedData.Controls.Control[i].datatype = pMappedData.datatype;
                                        pMappedData.Controls.Control[i].value = pColumnValue;
                                    }
                                    //
                                    bool isAction_IU_Verified = false;
                                    //
                                    if (isAction_IU && (pMappedData.Controls.Control[i].spheresLib != null))
                                    {
                                        IOMappedDataControl control = pMappedData.Controls.Control[i];
                                        //
                                        if (control.spheresLib.function.ToUpper() == "ISINSERT()" || control.spheresLib.function.ToUpper() == "ISUPDATE()")
                                            isAction_IU_Verified = true;
                                    }
                                    //
                                    if (isAction_IU_Verified)
                                    {
                                        pIsInsertUpdateControl = true;
                                        break;
                                    }
                                    //
                                    try
                                    {
                                        controlValue = GetDataValue(pMappedData.Controls.Control[i], pActionId);
                                        //
                                        if (isAction_IU)
                                            pMappedData.Controls.Control[i].valueVerified = controlValue;
                                    }
                                    catch (Exception ex)
                                    {
                                        throw new Exception(ArrFunc.GetStringList(pMessage, Cst.Space) + Cst.CrLf + "<b>Error to apply control" + Cst.CrLf + "[Action: " + resultAction + "]</b>", ex);
                                    }
                                }
                                else
                                {
                                    controlValue = pMappedData.Controls.Control[i].valueVerified;
                                    //
                                    if (null == controlValue)
                                        controlValue = GetDataValue(pMappedData.Controls.Control[i].spheresLib, pActionId);
                                }
                                //                                
                                if (controlValue == pMappedData.Controls.Control[i].@return)
                                {
                                    pResultAction = resultAction;
                                    //
                                    ArrayList defaultMsg = new ArrayList(pMessage);
                                    switch (pResultAction)
                                    {
                                        // GLOP FI IO => Revoir le copier coller 
                                        case "APPLYDEFAULT":
                                            if (StrFunc.IsFilled(defaultValue))
                                            {
                                                pColumnValue = defaultValue;
                                                defaultMsg.Insert(0, "LOG-06008");
                                                LogLogInfo(pSetErrorWarning, pMappedData.Controls.Control[i].logInfo, defaultMsg);
                                            }
                                            break;
                                        case "REJECTROW":
                                            isRowrejected = true;
                                            defaultMsg.Insert(0, "LOG-06009");
                                            LogLogInfo(pSetErrorWarning, pMappedData.Controls.Control[i].logInfo, defaultMsg);
                                            break;
                                        case "REJECTCOLUMN":
                                            if ((pActionId != IOCommonTools.SqlAction.S) && (pActionId != IOCommonTools.SqlAction.D))
                                            {
                                                defaultMsg.Insert(0, "LOG-06010");
                                                LogLogInfo(pSetErrorWarning, pMappedData.Controls.Control[i].logInfo, defaultMsg);
                                            }
                                            break;
                                        default: // Error
                                            {
                                                // FI 20200629 [XXXXX] SetErrorWarning
                                                // FI 20201221 [XXXXX] pSetErrorWarning
                                                pSetErrorWarning(ProcessStateTools.StatusErrorEnum);

                                                
                                                Logger.Log(new LoggerData(LogLevelEnum.Error, new SysMsgCode(SysCodeEnum.SYS, 6001), 3,
                                                    new LogParam(pMappedData.Controls.Control[i].action)));
                                                
                                                //Génération d'une exception pour stopper le traitement
                                                throw new Exception("action undefined");
                                            }
                                    }
                                }
                            }
                        }
                    }
                    #endregion
                }
                catch (Exception ex)
                {
                    throw new Exception(ArrFunc.GetStringList(pMessage, Cst.Space) + Cst.CrLf + "<b>Error to apply controls</b>", ex);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ArrFunc.GetStringList(pMessage, Cst.Space) + Cst.CrLf + "<b>Valuation error</b>", ex);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pSetErrorWarning">Delegue pour inscrire un warning ou une erreur</param>
        /// <param name="pLogInfo"></param>
        /// <param name="pDefaultMessage"></param>
        /// PM 20180219 [23824] Remplacement du type Task par l'interface IIOTaskLaunching
        /// FI 20201221 [XXXXX] Add pSetErrorWarning
        public static void LogLogInfo(SetErrorWarning pSetErrorWarning, IODetLogInfo pLogInfo, ArrayList pDefaultMessage)
        {
            string status = ProcessStateTools.StatusUnknown;

            if (null != pLogInfo)
            {
                // RD 20121203 / offrir des statuts exploitables avec le nouveau tracker 
                // et avec les nouveaux boutons «Error» et «Warning» sur les consultations du log.
                pLogInfo.TransformLogInfo();

                status = pLogInfo.status;
            }

            LogLogInfo(pSetErrorWarning, pLogInfo, status, pDefaultMessage);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pSetErrorWarning">Delegue pour inscrire un warning ou une erreur</param>
        /// <param name="pLogInfo"></param>
        /// <param name="pStatus"></param>
        /// <param name="pDefaultMessage"></param>
        /// PM 20180219 [23824] Remplacement du type Task par l'interface IIOTaskLaunching
        /// FI 20201221 [XXXXX] Add pSetErrorWarning
        public static void LogLogInfo(SetErrorWarning pSetErrorWarning, IODetLogInfo pLogInfo, string pStatus, ArrayList pDefaultMessage)
        {
            if (null != pLogInfo)
            {
                // RD 20121203 / offrir des statuts exploitables avec le nouveau tracker 
                // et avec les nouveaux boutons «Error» et «Warning» sur les consultations du log.
                pLogInfo.TransformLogInfo();
            }

            if ((null == pLogInfo) || (pLogInfo.status.ToUpper() != "NONE"))
            {
                // RD 20130201 correction du bug des messages accolés et simplification du code
                // 
                // Algorithme:
                // -----------
                // 1- S'il n'existe pas de message spécifié (pLogInfo == null) ET il n'existe pas de message par défaut (pDefaultMessage vide) 
                //      ALORS ne rien écrire dans le Log
                // 2- S'il n'existe pas de message spécifié (pLogInfo == null) ET il existe un message par défaut 
                //      ALORS ecrire le message par défaut.
                //
                // 3- S'il existe un message spécifié (pLogInfo != null) 
                //      ALORS écrire le message spécifié ET le message par défaut s'il existe
                //
                // 4- Si les deux messages sont au format SYSTEMMSG 
                //      ALORS écrire deux lignes dans le Log, une pour chaque message.
                // 5- Si l'un des deux messages est au format SYSTEMMSG et l'autre ne l'est pas
                //      ALORS écrire deux lignes dans le Log, une pour chaque message.
                // 6- Si les deux messages ne sont pas au format SYSTEMMSG
                //      ALORS écrire une seule ligne dans le Log, en accolant les deux messages.
                //
                bool isLogInfoSystemMsg = ((null != pLogInfo) && StrFunc.IsFilled(pLogInfo.Code) && (pLogInfo.Number != 0));
                IODetLogInfo defaultLogInfo = null;
                if (ArrFunc.IsFilled(pDefaultMessage))
                {
                    defaultLogInfo = new IODetLogInfo
                    {
                        status = ProcessStateTools.StatusUnknownEnum.ToString()
                    };
                    defaultLogInfo.SetMessageAndData((string[])pDefaultMessage.ToArray(typeof(string)));

                    if ((null != pLogInfo) && (false == isLogInfoSystemMsg) && StrFunc.IsFilled(pLogInfo.message))
                        defaultLogInfo.message = (StrFunc.IsFilled(defaultLogInfo.message) ? defaultLogInfo.message + Cst.CrLf2 : string.Empty) + pLogInfo.message;
                }

                bool isDefaultLogInfoSystemMsg = ((null != defaultLogInfo) && StrFunc.IsFilled(defaultLogInfo.Code) && (defaultLogInfo.Number != 0));
                if (null != pLogInfo)
                {
                    if ((isLogInfoSystemMsg) && (null != defaultLogInfo) && (false == isDefaultLogInfoSystemMsg) && StrFunc.IsFilled(defaultLogInfo.message))
                        pLogInfo.message = defaultLogInfo.message + (StrFunc.IsFilled(pLogInfo.message) ? Cst.CrLf2 + pLogInfo.message : string.Empty);
                }

                if (null != pLogInfo)
                {
                    pLogInfo.status = pStatus;
                    if (StrFunc.IsEmpty(pLogInfo.status))
                        pLogInfo.status = ProcessStateTools.StatusUnknown;
                }

                if (null != pLogInfo)
                    defaultLogInfo.status = pLogInfo.status;

                bool isException = ((null != pLogInfo) && pLogInfo.isException);

                if (isException)
                {
                    if (isLogInfoSystemMsg)
                    {
                        // FI 20200629 [XXXXX] SetErrorWarning
                        // FI 20201221 [XXXXX] pSetErrorWarning
                        pSetErrorWarning(ProcessStateTools.StatusErrorEnum);
                        pLogInfo.status = ProcessStateTools.StatusError;

                        
                        Logger.Log(LoggerConversionTools.ProcessLogInfoToLoggerData(pLogInfo));
                        // PM 20210915 [XXXXX] AzDO 201 : Le log peut avoir à la fois un SysMsgCode et un Message
                        if ((pLogInfo.SysMsgCode != default(SysMsgCode)) && (StrFunc.IsFilled(pLogInfo.message)))
                        {
                            Logger.Log(new LoggerData(LoggerConversionTools.StatusToLogLevelEnum(pLogInfo.status), pLogInfo.message, pLogInfo.levelOrder));
                        }
                    }

                    if (isDefaultLogInfoSystemMsg)
                    {
                        if (false == isLogInfoSystemMsg)
                            defaultLogInfo.status = ProcessStateTools.StatusError;

                        // FI 20200623 [XXXXX] SetErrorWarning
                        // FI 20201221 [XXXXX] pSetErrorWarning
                        pSetErrorWarning(ProcessStateTools.ParseStatus(defaultLogInfo.status));
                        
                        
                        Logger.Log(LoggerConversionTools.ProcessLogInfoToLoggerData(defaultLogInfo));
                        // PM 20210915 [XXXXX] AzDO 201 : Le log peut avoir à la fois un SysMsgCode et un Message
                        if ((defaultLogInfo.SysMsgCode != default(SysMsgCode)) && (StrFunc.IsFilled(defaultLogInfo.message)))
                        {
                            Logger.Log(new LoggerData(LoggerConversionTools.StatusToLogLevelEnum(defaultLogInfo.status), defaultLogInfo.message, defaultLogInfo.levelOrder));
                        }
                    }

                    //Génération d'une exception pour stopper le traitement
                    throw new Exception("An error occurs");
                }
                else
                {
                    if (isLogInfoSystemMsg)
                    {
                        // FI 20200623 [XXXXX] SetErrorWarning
                        // FI 20201221 [XXXXX] pSetErrorWarning
                        pSetErrorWarning(ProcessStateTools.ParseStatus(pLogInfo.status));
                        
                        
                        Logger.Log(LoggerConversionTools.ProcessLogInfoToLoggerData(pLogInfo));
                        // PM 20210915 [XXXXX] AzDO 201 : Le log peut avoir à la fois un SysMsgCode et un Message
                        if ((pLogInfo.SysMsgCode != default(SysMsgCode)) && (StrFunc.IsFilled(pLogInfo.message)))
                        {
                            Logger.Log(new LoggerData(LoggerConversionTools.StatusToLogLevelEnum(pLogInfo.status), pLogInfo.message, pLogInfo.levelOrder));
                        }
                    }

                    if (isDefaultLogInfoSystemMsg || (false == isLogInfoSystemMsg))
                    {
                        // FI 20200623 [XXXXX] SetErrorWarning
                        // FI 20201221 [XXXXX] pSetErrorWarning
                        pSetErrorWarning(ProcessStateTools.ParseStatus(defaultLogInfo.status));

                        
                        Logger.Log(LoggerConversionTools.ProcessLogInfoToLoggerData(defaultLogInfo));
                        // PM 20210915 [XXXXX] AzDO 201 : Le log peut avoir à la fois un SysMsgCode et un Message
                        if ((defaultLogInfo.SysMsgCode != default(SysMsgCode)) && (StrFunc.IsFilled(defaultLogInfo.message)))
                        {
                            Logger.Log(new LoggerData(LoggerConversionTools.StatusToLogLevelEnum(defaultLogInfo.status), defaultLogInfo.message, defaultLogInfo.levelOrder));
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pXMLTask"></param>
        /// <param name="pDynamicData"></param>
        /// <param name="pDbTransaction"></param>
        /// <param name="pDataValue"></param>
        /// <param name="pMessage"></param>
        // PM 20180219 [23824] Remplacement du type Task par l'interface IIOTaskLaunching
        //public static void GetDataValue(Task pXMLTask, StringDynamicData pDynamicData, IDbTransaction pDbTransaction, ref string pDataValue, ArrayList pMessage)
        public static void GetDataValue(IIOTaskLaunching pXMLTask, StringDynamicData pDynamicData, IDbTransaction pDbTransaction, ref string pDataValue, ArrayList pMessage)
        {
            _task = pXMLTask;
            _cs = _task.Cs;
            // PM 20180219 [23824] Remplacement du type Task par l'interface IIOTaskLaunching
            //_userId = _task.process.UserId;
            _userId = _task.UserId;
            _xmlTask = _task.IoTask;
            _dbTransaction = pDbTransaction;
            //
            try
            {
                pDataValue = GetDataValue(pDynamicData, IOCommonTools.SqlAction.NA);
            }
            catch (Exception ex)
            {
                throw new Exception("<b>Valuation error</b>" + Cst.CrLf + ArrFunc.GetStringList(pMessage, Cst.Space), ex);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pData"></param>
        /// <returns></returns>
        private static string GetDataValue(IOData pData)
        {
            return GetDataValue(pData, IOCommonTools.SqlAction.NA);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pData"></param>
        /// <param name="pActionId"></param>
        /// <returns></returns>
        /// RD 20100305 / Optimisation
        // RD 20160113 [21756] Modify
        private static string GetDataValue(IOData pData, IOCommonTools.SqlAction pActionId)
        {
            // RD 20160113 [21756] Use pActionId
            return GetDataValue((StringDynamicData)pData, pActionId);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pData"></param>
        /// <param name="pActionId"></param>
        /// <returns></returns>
        private static string GetDataValue(StringDynamicData pData, IOCommonTools.SqlAction pActionId)
        {
            string csTemp = _cs;

            // 20100826 MF - Spheres IO specific SpheresLib functions (e.g. GETPROCESSLOGID()) 
            // are not evaluated when they are referenced in a "SQL/Param" node - seealso Ticket 16023/EFSThread
            if (pData.sql != null)
                EvaluateSQLParams(pData.sql.param, pActionId);

            string retValue = pData.GetDataValue(csTemp, _dbTransaction);

            // RD 20110214 / Prise en charge du format
            if (null == retValue)
                retValue = GetDataValue(pData.spheresLib, pActionId, pData.dataformat);
            //
            return retValue;

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="paramsData"></param>
        /// <param name="pActionId"></param>
        private static void EvaluateSQLParams(ParamData[] paramsData, IOCommonTools.SqlAction pActionId)
        {
            // RD 20101019 Bug: if paramsData = null
            if (ArrFunc.IsFilled(paramsData))
            {
                foreach (ParamData paramData in paramsData)
                    if (paramData.spheresLib != null)
                    {
                        string ret = GetDataValue(paramData.spheresLib, pActionId);
                        paramData.value = ret;
                        paramData.spheresLib = null;
                    }
            }
        }

        /// <summary>
        /// Evaluation d'une Fonction SpheresLib propre à Spheres IO®
        /// </summary>
        /// <param name="pDataSpheresLib"></param>
        /// <param name="pActionId"></param>
        /// <returns></returns>
        private static string GetDataValue(DataSpheresLib pDataSpheresLib, IOCommonTools.SqlAction pActionId)
        {
            return GetDataValue(pDataSpheresLib, pActionId, string.Empty);
        }
        /// <summary>
        /// Evaluation et formatage du résultat d'une Fonction SpheresLib propre à Spheres IO®
        /// </summary>
        /// <param name="pDataSpheresLib"></param>
        /// <param name="pActionId"></param>
        /// <param name="pDataFormat"></param>
        /// <returns></returns>
        private static string GetDataValue(DataSpheresLib pDataSpheresLib, IOCommonTools.SqlAction pActionId, string pDataFormat)
        {

            string retValue = null;
            //
            if (pDataSpheresLib != null)
            {
                try
                {
                    #region SpheresLib
                    switch (pDataSpheresLib.function.ToUpper())
                    {
                        #region Spheres Functions for User
                        case "GETUSERID()":
                            retValue = _userId.ToString();
                            break;
                        #endregion

                        #region Spheres Functions for Requester
                        case "GETREQUESTER()":
                            // PM 20180219 [23824] Remplacement du type Task par l'interface IIOTaskLaunching
                            //retValue = _task.requester.hostName.ToString();
                            retValue = _task.RequesterHostName;
                            break;

                        case "GETREQUESTERID()":
                            // PM 20180219 [23824] Remplacement du type Task par l'interface IIOTaskLaunching
                            //retValue = _task.requester.idA.ToString();
                            retValue = _task.RequesterIdA.ToString();
                            break;

                        case "GETREQUESTERDATE()":
                            // RD 20110214 / Prise en charge du format
                            if (StrFunc.IsFilled(pDataFormat))
                            {
                                // PM 20180219 [23824] Remplacement du type Task par l'interface IIOTaskLaunching
                                //retValue = DtFunc.DateTimeToString(_task.requester.date, pDataFormat);
                                retValue = DtFunc.DateTimeToString(_task.RequesterDate, pDataFormat);
                            }
                            else
                            {
                                // RD 20190718 [] Manage FORMAT parameter
                                string format = string.Empty;

                                if (pDataSpheresLib.GetParam("FORMAT", out ParamData paramItem))
                                    format = paramItem.GetDataValue(_task.Cs, null);

                                if (StrFunc.IsFilled(format))
                                {
                                    retValue = DtFunc.DateTimeToString(_task.RequesterDate, format);
                                }
                                else
                                {

                                    // PM 20180219 [23824] Remplacement du type Task par l'interface IIOTaskLaunching
                                    //retValue = ObjFunc.FmtToISo(_task.requester.date, TypeData.TypeDataEnum.datetime);
                                    retValue = ObjFunc.FmtToISo(_task.RequesterDate, TypeData.TypeDataEnum.datetime);
                                }
                            }
                            break;

                        case "GETREQUESTERSESSIONID()":
                            // PM 20180219 [23824] Remplacement du type Task par l'interface IIOTaskLaunching
                            //retValue = _task.requester.sessionId.Trim(); ;
                            retValue = _task.RequesterSessionId.Trim();
                            break;
                        #endregion

                        #region Spheres Functions for Task
                        case "GETTASKID()":
                            // PM 20180219 [23824] Utilisation accessor
                            //retValue = _xmlTask.id.ToString();
                            retValue = _xmlTask.Id;
                            break;

                        case "GETTASKIDENTIFIER()":
                            // PM 20180219 [23824]Utilisation accessor
                            //retValue = _xmlTask.name.ToString();
                            retValue = _xmlTask.Name;
                            break;

                        case "GETTASKGUID()":
                            // PM 20180219 [23824] Utilisation accessor
                            //retValue = _xmlTask.taskguid;
                            retValue = _xmlTask.TaskGuid;
                            break;

                        case "GETTEMPORARYDIRECTORY()":
                            // PM 20180219 [23824] Remplacement du type Task par l'interface IIOTaskLaunching
                            //retValue = _task.appInstance.GetTemporaryDirectory(AppInstance.AddFolderSessionId.True);
                            retValue = _task.GetTemporaryDirectory(AppSession.AddFolderSessionId.True);
                            break;

                        case "GETPARAMETER(ID)":
                            // RD 20110214 / Prise en charge du format
                            retValue = GetParameter(pDataSpheresLib.param, "ID", pDataFormat);
                            break;

                        case "GETPARAMETER()":
                            // RD 20110214 / Prise en charge du format
                            retValue = GetParameter(pDataSpheresLib.param, string.Empty, pDataFormat);
                            break;
                        #endregion

                        #region Spheres Functions for Process
                        // RD 20190718 Add GETMOMPATH function
                        case "GETMOMPATH()":
                            retValue = (_task.Session.AppInstance as AppInstanceService).MOMPath;
                            break;
                        case "GETPROCESSLOGID()":
                            // PM 20180219 [23824] Remplacement du type Task par l'interface IIOTaskLaunching
                            //retValue = _task.process.processLog.header.IdProcess.ToString();
                            retValue = _task.IdProcess.ToString();
                            break;
                        // RD 20190718 Add GETPROCESSCS function
                        case "GETPROCESSCS()":
                            retValue = _task.Cs.ToString();
                            break;
                        // RD 20190718 Add GETPROCESSENCRYPTCS function
                        case "GETPROCESSENCRYPTCS()":
                            retValue = Cryptography.Encrypt(_task.Cs.ToString());
                            break;
                        #endregion

                        #region Spheres Functions for Insert/Update
                        case "ISINSERT()":
                            retValue = ObjFunc.FmtToISo((IOCommonTools.SqlAction.I == pActionId /*|| IOCommonTools.SqlAction.IU == pActionId*/), TypeData.TypeDataEnum.@bool);
                            break;
                        //
                        case "ISUPDATE()":
                            retValue = ObjFunc.FmtToISo((IOCommonTools.SqlAction.U == pActionId /*|| IOCommonTools.SqlAction.IU == pActionId*/), TypeData.TypeDataEnum.@bool);
                            break;
                        #endregion

                        default:
                            {
                                throw new Exception("Function: " + pDataSpheresLib.function.ToUpper() + " is not supported");
                            }
                    }
                    #endregion SpheresLib
                }
                catch (Exception ex)
                {
                    throw new Exception("Error with SpheresLib function: " + pDataSpheresLib.function.ToUpper(), ex);
                }
            }
            //	
            return retValue;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pParam"></param>
        /// <param name="pParamCommande"></param>
        /// <param name="pDataType"></param>
        /// <returns></returns>
        private static string GetSQLParamValue(ParamData[] pParam, string pParamCommande, out string pDataType)
        {
            string paramValue = string.Empty;
            pDataType = string.Empty;
            //
            if (StrFunc.IsFilled(pParamCommande))
            {
                for (int i = 0; i < pParam.Length; i++)
                {
                    if (pParam[i].name.Trim().ToUpper() == pParamCommande.Trim().ToUpper())
                    {
                        paramValue = pParam[i].value;
                        pDataType = pParam[i].datatype;
                        break;
                    }
                }
            }
            else
                paramValue = pParam[0].value;
            //
            return paramValue;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="pParam"></param>
        /// <param name="pParamName"></param>
        /// <param name="pDataFormat"></param>
        /// <returns></returns>
        private static string GetParameter(ParamData[] pParam, string pParamName, string pDataFormat)
        {
            string paramDataType;
            string paramId;

            if (pParam != null)
                paramId = GetSQLParamValue(pParam, pParamName, out paramDataType);
            else
                throw new Exception("Parameter ID is expected for function: GetTaskParameter(" + pParamName + ")");
            string paramValue;
            //			
            if (StrFunc.IsFilled(paramId))
            {
                if (_xmlTask.ExistTaskParam(paramId))
                {
                    if (StrFunc.IsFilled(paramDataType))
                    {
                        paramValue = _xmlTask.GetTaskParamValue(paramId);
                    }
                    else
                    {
                        paramValue = _xmlTask.GetTaskParamValue(paramId, out paramDataType);
                    }
                }
                else
                {
                    // PM 20180219 [23824]  Utilisation accessor
                    //throw new Exception("Parameter ID specified for function GetTaskParameter(" + pParamName + ") doesn't exist"
                    //    + " - None parameter specified for this Task: " + _xmlTask.name + "[" + _xmlTask.id + "]");
                    throw new Exception("Parameter ID specified for function GetTaskParameter(" + pParamName + ") doesn't exist"
                        + " - None parameter specified for this Task: " + _xmlTask.Name + "[" + _xmlTask.Id + "]");
                }
                //
                // RD 20110214 / Prise en charge du format
                if (StrFunc.IsFilled(pDataFormat))
                {
                    TypeData.TypeDataEnum dataType = TypeData.TypeDataEnum.unknown;
                    //
                    if (StrFunc.IsFilled(paramDataType))
                        dataType = TypeData.GetTypeDataEnum(paramDataType);
                    //
                    switch (dataType)
                    {
                        case TypeData.TypeDataEnum.datetime:
                        case TypeData.TypeDataEnum.date:
                        case TypeData.TypeDataEnum.time:
                            paramValue = new DtFunc().GetDateTimeString(paramValue, pDataFormat);
                            break;
                    }
                }
            }
            else
                throw new Exception("Parameter ID specified for function GetTaskParameter(" + pParamName + ") is empty");
            //
            return paramValue;
        }

        #endregion
    }
}