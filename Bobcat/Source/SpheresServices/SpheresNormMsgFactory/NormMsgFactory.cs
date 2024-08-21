#region Using Directives
//
using EFS.ACommon;
using EFS.Actor;
using EFS.ApplicationBlocks.Data;
using EFS.Common;
using EFS.Common.Acknowledgment;
using EFS.Common.Log;
using EFS.Common.MQueue;
using EFS.LoggerClient;
using EFS.LoggerClient.LoggerService;
//
using EfsML.Business;
using System;
using System.Collections;
using System.Data;
#endregion Using Directives

namespace EFS.Process
{
    #region NormMsgFactoryBase
    /// <summary>
    /// Classe de base du traitement de normalisation
    /// </summary>
    public class NormMsgFactoryBase
    {
        #region Members
        protected NormMsgFactoryProcess m_NormMsgFactoryProcess;
        

        protected MQueueAttributes m_MQueueAttributes;
        /// <summary>
        /// Liste des Messages Queue qui seront postés au service cible
        /// </summary>
        protected MQueueBase[] m_SendingMQueue;

        

        #endregion Members

        #region Accessors
        /// <summary>
        /// Obtient la définition de l'accusé de réception présent dans le MsgQueue
        /// </summary>
        protected AcknowledgmentInfo Acknowledgment
        {
            get { return m_NormMsgFactoryProcess.NormMsgFactoryMQueue.acknowledgment; }
        }

        /// <summary>
        /// Obtient BuildingInfo présent dans le MsgQueue
        /// </summary>
        protected NormMsgBuildingInfo BuildingInfo
        {
            get { return m_NormMsgFactoryProcess.NormMsgFactoryMQueue.buildingInfo; }
        }

        #endregion Accessors

        #region Constructors
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pNormMsgFactoryProcess"></param>
        public NormMsgFactoryBase(NormMsgFactoryProcess pNormMsgFactoryProcess)
        {
            m_NormMsgFactoryProcess = pNormMsgFactoryProcess;
            m_MQueueAttributes = new MQueueAttributes()
            {
                connectionString = m_NormMsgFactoryProcess.Cs,
                id = BuildingInfo.id,
                identifier = BuildingInfo.identifier,
            };
            m_MQueueAttributes.timestamp = OTCmlHelper.GetDateSysUTC(m_MQueueAttributes.connectionString);


            
            
        }
        #endregion Constructors

        #region Methods
        /// <summary>
        ///  Construction des Message Queue qui seront postés 
        /// </summary>
        /// <returns></returns>
        // EG 20190114 Add detail to ProcessLog Refactoring
        public virtual Cst.ErrLevel Generate()
        {
            // Log
            
            Logger.Log(new LoggerData(LogLevelEnum.Debug, new SysMsgCode(SysCodeEnum.LOG, 8001), 1,
                new LogParam(m_NormMsgFactoryProcess.LogId),
                new LogParam(m_NormMsgFactoryProcess.LogProcessType)));

            Cst.ErrLevel codeReturn = CreateParameters();
            if (Cst.ErrLevel.SUCCESS == codeReturn)
                codeReturn = SetCriteria();
            if (Cst.ErrLevel.SUCCESS == codeReturn)
                codeReturn = CreateIdInfos();
            if (Cst.ErrLevel.SUCCESS == codeReturn)
            {
                // Log
                
                Logger.Log(new LoggerData(LogLevelEnum.Debug, new SysMsgCode(SysCodeEnum.LOG, 8002), 1,
                    new LogParam(m_NormMsgFactoryProcess.LogId),
                    new LogParam(m_NormMsgFactoryProcess.LogProcessType)));

                codeReturn = ConstructSendingMQueue();
            }
            return codeReturn;
        }

        /// <summary>
        /// Retourne un mQueue pour l'enregistrement {pDr}
        /// </summary>
        /// <param name="pDr"></param>
        /// <returns></returns>
        protected virtual MQueueBase BuildMessageQueue(DataRow pDr)
        {
            return null;
        }

        /// <summary>
        /// Retourne La requête SQL à l'origine des Mqueues générés
        /// </summary>
        /// <returns></returns>
        /// FI 20150618 [20945] Add
        protected virtual QueryParameters GetQueryParameters()
        {
            throw new NotImplementedException("Method GetQueryParameters is not implemented");
        }

        /// <summary>
        ///  Affectation de m_SendingMQueue via l'exécution d'une requête 
        ///  <para>Les méthodes GetQueryParameters, RowsCandidates, BuildMessageQueue doivent être overridées</para>
        ///  <para>Retourne NOTHINGTODO si le résultat de la requête retourne aucune ligne</para>
        /// </summary>
        /// <returns></returns>
        /// FI 20150618 [20945] Refactoring
        /// FI 20160225 [21953] Modify
        protected virtual Cst.ErrLevel ConstructSendingMQueue()
        {
            Cst.ErrLevel codeReturn = Cst.ErrLevel.SUCCESS;
            
            m_SendingMQueue = null;

            QueryParameters qryParameters = GetQueryParameters();

            DataSet ds = DataHelper.ExecuteDataset(m_NormMsgFactoryProcess.Cs, CommandType.Text,
                                                    qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter());

            DataRow[] rows = RowsCandidates(ds.Tables[0]);
            bool isFound = (0 < ArrFunc.Count(rows));
            if (isFound)
            {
                ArrayList aMQueue = new ArrayList();
                foreach (DataRow dr in rows)
                {
                    MQueueBase queue = BuildMessageQueue(dr);
                    MQueueTools.AddFactoryFlagParameter(queue);
                    // FI 20160225 [21953] Call Add AddData
                    AddData(queue);  
                    aMQueue.Add(queue);
                }
                m_SendingMQueue = (MQueueBase[])aMQueue.ToArray(typeof(MQueueBase));
            }
            else
            {
                // ERROR : No candidates
                codeReturn = Cst.ErrLevel.NOTHINGTODO;
            }

            return codeReturn;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected virtual Cst.ErrLevel SetCriteria()
        {
            return Cst.ErrLevel.SUCCESS;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected virtual Cst.ErrLevel CreateIdInfos()
        {
            return Cst.ErrLevel.SUCCESS;
        }

        /// <summary>
        /// Alimentation de m_MQueueAttributes.parameters
        /// </summary>
        /// <returns></returns>
        protected virtual Cst.ErrLevel CreateParameters()
        {
            return Cst.ErrLevel.SUCCESS;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected virtual DataRow[] RowsCandidates(DataTable pDt)
        {
            return pDt.Select(null, null, DataViewRowState.OriginalRows);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pQueue"></param>
        /// <param name="pDr"></param>
        protected void SetDefaultMessageQueue(MQueueBase pQueue, DataRow pDr)
        {
            pQueue.id = Convert.ToInt32(pDr["IDT"]);
            pQueue.idSpecified = true;

            pQueue.idInfo = new IdInfo() {id = pQueue.id };
            pQueue.idInfoSpecified = true;
            if (pDr.Table.Columns.Contains("GPRODUCT") && (false == Convert.IsDBNull(pDr["GPRODUCT"])))
                pQueue.idInfo.idInfos = new DictionaryEntry[] { new DictionaryEntry("GPRODUCT", pDr["GPRODUCT"].ToString()) };

            if (null != m_MQueueAttributes.parameters)
                pQueue.parameters = m_MQueueAttributes.parameters.Clone();
        }

        /// <summary>
        /// Affectation de m_SendingMQueue avec 
        /// </summary>
        /// <param name="pQueue"></param>
        /// FI 20160225 [21953] Modify
        protected void SetSendingMQueue(MQueueBase pQueue)
        {
            MQueueTools.AddFactoryFlagParameter(pQueue);
            // FI 20160225 [21953] Call Add AddData
            AddData(pQueue);
            m_SendingMQueue = new MQueueBase[1] { pQueue };
        }

        /// <summary>
        /// Postage des messages Queues générés 
        /// </summary>
        /// <returns></returns>
        /// FI 20170327 [23004] Méthode virtuelle
        // EG 20190114 Add detail to ProcessLog Refactoring
        public virtual Cst.ErrLevel SendFinalMessages()
        {
            if (null != m_SendingMQueue)
            {
                MQueueTaskInfo m_TaskInfo = new MQueueTaskInfo
                {
                    connectionString = m_NormMsgFactoryProcess.Cs,
                    Session = m_NormMsgFactoryProcess.Session,
                    process = BuildingInfo.processType,
                    trackerAttrib = new TrackerAttributes()
                    {
                        process = BuildingInfo.processType,
                        info = TrackerAttributes.BuildInfo(m_SendingMQueue[0])
                    },
                    mQueue = m_SendingMQueue,

                };

                // S'il n'existe pas de paramétrage dans le fichier config concernant MOMType et MOMPath NormMsgFactory se comporte comme les autres services   
                if (false == (StrFunc.IsFilled(SystemSettings.GetAppSettings("MOMType")) && StrFunc.IsFilled(SystemSettings.GetAppSettings("MOMPath"))))
                    m_TaskInfo.sendInfo = EFS.SpheresService.ServiceTools.GetMqueueSendInfo(BuildingInfo.processType, m_NormMsgFactoryProcess.AppInstance);

                switch (BuildingInfo.processType)
                {
                    case Cst.ProcessTypeEnum.IO:
                        m_TaskInfo.trackerAttrib.caller = m_SendingMQueue[0].GetStringValueIdInfoByKey("IN_OUT");
                        break;
                    case Cst.ProcessTypeEnum.RIMGEN:
                        //FI 20150618 [20945] il n'existe qu'un menu à partir de la 4.6
                        m_TaskInfo.trackerAttrib.caller = IdMenu.GetIdMenu(IdMenu.Menu.PROCESS_RIMGEN);
                        break;
                    default:
                        break;
                }

                if (null != Acknowledgment) // Acknowledgment peut être null (Exemple Interruption de traitement)
                {
                    m_TaskInfo.trackerAttrib.acknowledgment = new TrackerAcknowledgmentInfo
                    {
                        extlId = Acknowledgment.extlId,
                        schedules = Acknowledgment.schedules

                    };
                }

                MQueueTaskInfo.SendMultipleThreadPool(m_TaskInfo, false);

                
                Logger.Log(new LoggerData(LogLevelEnum.Debug, new SysMsgCode(SysCodeEnum.LOG, 8003), 1,
                    new LogParam(m_NormMsgFactoryProcess.LogId),
                    new LogParam(m_NormMsgFactoryProcess.LogProcessType),
                    new LogParam(m_SendingMQueue.Length)));
            }
            return Cst.ErrLevel.SUCCESS;
        }

       

        /// <summary>
        ///  Recopie les datas éventuellement présents dans le msg NormMsgFactoty dans {pQueue}
        /// </summary>
        /// <param name="pQueue"></param>
        /// FI 20160225 [21953] Add Method
        protected void AddData(MQueueBase pQueue)
        {
            pQueue.dataSpecified = this.m_NormMsgFactoryProcess.MQueue.dataSpecified;
            if (pQueue.dataSpecified)
                pQueue.data = this.m_NormMsgFactoryProcess.MQueue.data;
        }



        #region AddActorParameter
        /// <summary>
        ///  Ajoute le paramètre Acteur
        /// </summary>
        /// <returns></returns>
        protected Cst.ErrLevel AddActorParameter()
        {
            return AddActorParameter(NormMsgFactoryMQueue.PARAM_ACTOR, NormMsgFactoryMQueue.PARAM_ACTOR, null, null);
        }
        /// <summary>
        ///  Ajoute le paramètre Acteur
        /// </summary>
        /// <returns></returns>
        protected Cst.ErrLevel AddActorParameter(Nullable<int> pIdGActor)
        {
            return AddActorParameter(NormMsgFactoryMQueue.PARAM_ACTOR, NormMsgFactoryMQueue.PARAM_ACTOR, null, pIdGActor);
        }
        /// <summary>
        ///  Ajoute le paramètre Acteur
        /// <param name="pRoleActor">si renseigné, vérification que l'acteur a le rôle spécifié</param>
        /// </summary>
        /// <returns></returns>
        protected Cst.ErrLevel AddActorParameter(Nullable<RoleActor> pRoleActor)
        {
            return AddActorParameter(NormMsgFactoryMQueue.PARAM_ACTOR, NormMsgFactoryMQueue.PARAM_ACTOR, pRoleActor, null);
        }
        /// <summary>
        ///  Ajoute le paramètre Acteur
        /// <param name="pParamKeyRead">Nom du paramètre dans le message reçu</param>
        /// <param name="pRoleActor">si renseigné, vérification que l'acteur a le rôle spécifié</param>
        /// </summary>
        /// <returns></returns>
        protected Cst.ErrLevel AddActorParameter(string pParamKeyRead, Nullable<RoleActor> pRoleActor)
        {
            return AddActorParameter(pParamKeyRead, pParamKeyRead, pRoleActor, null);
        }
        /// <summary>
        ///  Ajoute le paramètre Acteur
        /// </summary>
        /// <returns></returns>
        /// <param name="pParamKeyRead">Nom du paramètre dans le message reçu</param>
        /// <param name="pParamKeyWrite">Nom du paramètre dans le message généré</param>
        /// <param name="pRoleActor">si renseigné, vérification que l'acteur a le rôle spécifié</param>
        /// <param name="pIdGActor">si renseigné, vérification que l'acteur appartient au groupe de acteur spécifié</param>
        /// <returns></returns>
        /// FI 20141230 [20616] Add parameter pIdGActor (pour faire similaire à AddMarketParameter)
        // EG 20190114 Add detail to ProcessLog Refactoring
        protected Cst.ErrLevel AddActorParameter(string pParamKeyRead, string pParamKeyWrite, Nullable<RoleActor> pRoleActor, Nullable<int> pIdGActor)
        {
            Cst.ErrLevel codeReturn = Cst.ErrLevel.SUCCESS;
            if ((BuildingInfo.parameters[pParamKeyRead] is MQueueparameter actor) && StrFunc.IsFilled(actor.Value))
            {
                SQL_Actor sqlActor = null;
                switch (actor.dataType)
                {
                    case TypeData.TypeDataEnum.@int:
                    case TypeData.TypeDataEnum.integer:
                        sqlActor = new SQL_Actor(m_NormMsgFactoryProcess.Cs, Convert.ToInt32(actor.Value));
                        break;
                    case TypeData.TypeDataEnum.@string:
                        sqlActor = new SQL_Actor(m_NormMsgFactoryProcess.Cs, actor.Value);
                        break;
                    default:
                        // ERROR Incorrect Datatype Parameter 
                        // FI 20200623 [XXXXX] SetErrorWarning
                        m_NormMsgFactoryProcess.ProcessState.SetErrorWarning(ProcessStateTools.StatusErrorEnum);

                        
                        Logger.Log(new LoggerData(LogLevelEnum.Error, new SysMsgCode(SysCodeEnum.SYS, 8003), 2,
                            new LogParam(actor.Value + " (" + pParamKeyRead + ")"),
                            new LogParam(actor.dataType),
                            new LogParam(m_NormMsgFactoryProcess.LogId),
                            new LogParam(m_NormMsgFactoryProcess.LogProcessType)));

                        codeReturn = Cst.ErrLevel.FAILURE;
                        break;
                }

                if (Cst.ErrLevel.SUCCESS == codeReturn)
                {
                    //FI 20141230 [20616]
                    sqlActor.IdGActor = pIdGActor;
                    if (pRoleActor.HasValue)
                        sqlActor.SetRole(pRoleActor.Value);

                    sqlActor.LoadTable(new string[] { "ACTOR.IDA, ACTOR.IDENTIFIER" });
                    if (sqlActor.IsLoaded)
                    {
                        if (pRoleActor.HasValue)
                        {
                            if (false == sqlActor.ContainsRole(pRoleActor.Value))
                            {
                                // ERROR = Add LOG
                                codeReturn = Cst.ErrLevel.FAILURE;
                            }
                        }
                        if (Cst.ErrLevel.SUCCESS == codeReturn)
                        {
                            MQueueparameter parameter;
                            if (actor.Value.Contains("%"))
                            {
                                parameter = new MQueueparameter(pParamKeyWrite, TypeData.TypeDataEnum.@string);
                                parameter.SetValue(actor.Value, actor.Value);
                            }
                            else
                            {
                                parameter = new MQueueparameter(pParamKeyWrite, TypeData.TypeDataEnum.integer);
                                parameter.SetValue(sqlActor.Id, sqlActor.Identifier);
                            }
                            AddMQueueAttributesParameter(parameter);
                        }
                    }
                    else
                    {
                        codeReturn = Cst.ErrLevel.FAILURE;
                    }

                    if (Cst.ErrLevel.FAILURE == codeReturn)
                    {
                        // FI 20200623 [XXXXX] SetErrorWarning
                        m_NormMsgFactoryProcess.ProcessState.SetErrorWarning(ProcessStateTools.StatusErrorEnum);

                        // ERROR ACTOR not found in repository
                        
                        Logger.Log(new LoggerData(LogLevelEnum.Error, new SysMsgCode(SysCodeEnum.SYS, 8002), 2,
                            new LogParam(actor.Value + " (" + pParamKeyRead + ")"),
                            new LogParam(m_NormMsgFactoryProcess.LogId),
                            new LogParam(m_NormMsgFactoryProcess.LogProcessType)));
                    }
                }
            }
            return codeReturn;
        }
        #endregion AddActorParameter

        #region AddBookParameter
        /// <summary>
        ///  Ajoute le paramètre book 
        /// </summary>
        /// <returns></returns>
        protected Cst.ErrLevel AddBookParameter()
        {
            return AddBookParameter(NormMsgFactoryMQueue.PARAM_BOOK, null);
        }

        /// <summary>
        ///  Ajoute le paramètre book  
        /// </summary>
        /// <param name="pIdGBook">si renseigné (vérification que le book appartient au groupe de book)</param>
        /// <returns></returns>
        /// FI 20141230 [20616] Modify Add parameter pGBook (pour faire similaire à AddMarketParameter)
        protected Cst.ErrLevel AddBookParameter(Nullable<int> pIdGBook)
        {
            return AddBookParameter(NormMsgFactoryMQueue.PARAM_BOOK, pIdGBook);
        }
        /// <summary>
        ///  Ajoute le paramètre book  
        /// </summary>
        /// <param name="pParamKeyWrite">Nom du paramètre dans le message généré</param>
        /// <param name="pIdGBook">si renseigné (vérification que le book appartient au groupe de book)</param>
        /// <returns></returns>
        /// FI 20141230 [20616] Modify Add parameter pGBook (pour faire similaire à AddMarketParameter)
        // EG 20190114 Add detail to ProcessLog Refactoring
        protected Cst.ErrLevel AddBookParameter(string pParamKeyWrite, Nullable<int> pIdGBook)
        {
            Cst.ErrLevel codeReturn = Cst.ErrLevel.SUCCESS;
            if ((BuildingInfo.parameters[NormMsgFactoryMQueue.PARAM_BOOK] is MQueueparameter book) && StrFunc.IsFilled(book.Value))
            {
                SQL_Book sqlBook = null;
                switch (book.dataType)
                {
                    case TypeData.TypeDataEnum.@int:
                    case TypeData.TypeDataEnum.integer:
                        sqlBook = new SQL_Book(m_NormMsgFactoryProcess.Cs, Convert.ToInt32(book.Value), SQL_Table.ScanDataDtEnabledEnum.Yes);
                        break;
                    case TypeData.TypeDataEnum.@string:
                        sqlBook = new SQL_Book(m_NormMsgFactoryProcess.Cs, SQL_TableWithID.IDType.Identifier, book.Value, SQL_Table.ScanDataDtEnabledEnum.Yes);
                        break;
                    default:
                        // ERROR Incorrect Datatype Parameter 
                        // FI 20200623 [XXXXX] SetErrorWarning
                        m_NormMsgFactoryProcess.ProcessState.SetErrorWarning(ProcessStateTools.StatusErrorEnum);

                        
                        Logger.Log(new LoggerData(LogLevelEnum.Error, new SysMsgCode(SysCodeEnum.SYS, 8003), 2,
                            new LogParam(book.Value + " (" + NormMsgFactoryMQueue.PARAM_BOOK.ToString() + ")"),
                            new LogParam(book.dataType),
                            new LogParam(m_NormMsgFactoryProcess.LogId),
                            new LogParam(m_NormMsgFactoryProcess.LogProcessType)));

                        codeReturn = Cst.ErrLevel.FAILURE;
                        break;
                }
                if (Cst.ErrLevel.SUCCESS == codeReturn)
                {
                    //FI 20141230 [20616]
                    sqlBook.IdGBook = pIdGBook;
                    sqlBook.IsUseTable = true;
                    sqlBook.LoadTable(new string[] { "BOOK.IDB, BOOK.IDENTIFIER" });

                    if (sqlBook.IsLoaded)
                    {
                        if (false == book.Value.Contains("%"))
                        {
                            MQueueparameter parameter = new MQueueparameter(pParamKeyWrite, TypeData.TypeDataEnum.integer);
                            parameter.SetValue(sqlBook.Id, sqlBook.Identifier);
                            AddMQueueAttributesParameter(parameter);
                        }
                    }
                    else
                    {
                        // ERROR BOOK not found in repository
                        // FI 20200623 [XXXXX] SetErrorWarning
                        m_NormMsgFactoryProcess.ProcessState.SetErrorWarning(ProcessStateTools.StatusErrorEnum);

                        
                        
                        Logger.Log(new LoggerData(LogLevelEnum.Error, new SysMsgCode(SysCodeEnum.SYS, 8002), 2,
                            new LogParam(book.Value + " (" + NormMsgFactoryMQueue.PARAM_BOOK.ToString() + ")"),
                            new LogParam(m_NormMsgFactoryProcess.LogId),
                            new LogParam(m_NormMsgFactoryProcess.LogProcessType)));

                        codeReturn = Cst.ErrLevel.FAILURE;
                    }
                }
            }
            return codeReturn;
        }
        #endregion AddBookParameter
        #region AddBooleanParameter
        /// <summary>
        /// Ajoute le paramètre {pParamKeyRead} à partir du paramètre {pParamKeyRead}
        /// <para>Affecte la valeur false si le paramètre {pParamKeyRead} n'existe pas </para>
        /// </summary>
        /// <param name="pParamKeyRead"></param>
        /// <returns></returns>
        protected Cst.ErrLevel AddBooleanParameter(string pParamKeyRead)
        {
            return AddBooleanParameter(pParamKeyRead, pParamKeyRead);
        }
        /// <summary>
        /// Ajoute le paramètre {pParamKeyWrite} à partir du paramètre {pParamKeyRead}
        /// <para>Affecte la valeur false si le paramètre {pParamKeyRead} n'existe pas </para>
        /// </summary>
        /// <param name="pParamKeyRead"></param>
        /// <param name="pParamKeyWrite"></param>
        /// <returns></returns>
        protected Cst.ErrLevel AddBooleanParameter(string pParamKeyRead, string pParamKeyWrite)
        {
            Cst.ErrLevel ret = Cst.ErrLevel.SUCCESS;
            bool isTrue = BuildingInfo.parameters.GetBoolValueParameterById(pParamKeyRead);
            MQueueparameter parameter = new MQueueparameter(pParamKeyWrite, TypeData.TypeDataEnum.@bool);
            parameter.SetValue(isTrue);

            AddMQueueAttributesParameter(parameter);

            return ret;
        }
        #endregion AddBooleanParameter
        #region AddStringParameter
        /// <summary>
        /// Récupère la valeur paramètre {pParamKeyRead} et ajoute un paramètre de même nom avec la valeur récupérée
        /// </summary>
        /// <param name="pParamKeyRead"></param>
        /// <param name="pParamKeyWrite"></param>
        /// <param name="pIsOptional">si true, il y a ajout du paramètre uniquement lorsque la valeur récupérée est enseignée</param>
        /// <returns></returns>
        /// <param name="pParamKeyRead"></param>
        /// <param name="pIsOptional"></param>
        /// <returns></returns>
        ///FI 20141126 [20526] Modify (add parameter pIsOptional)
        protected Cst.ErrLevel AddStringParameter(string pParamKeyRead, Boolean pIsOptional)
        {
            return AddStringParameter(pParamKeyRead, pParamKeyRead, pIsOptional);
        }
        /// <summary>
        /// Récupère la valeur paramètre {pParamKeyRead} et ajoute le paramètre {pParamKeyWrite} avec la valeur récupérée
        /// </summary>
        /// <param name="pParamKeyRead"></param>
        /// <param name="pParamKeyWrite"></param>
        /// <param name="pIsOptional">si true, il y a ajout du paramètre uniquement lorsque la valeur récupérée est enseignée</param>
        /// <returns></returns>
        ///FI 20141126 [20526] Modify (add parameter pIsOptional)
        protected Cst.ErrLevel AddStringParameter(string pParamKeyRead, string pParamKeyWrite, Boolean pIsOptional)
        {
            Cst.ErrLevel ret = Cst.ErrLevel.SUCCESS;
            string paramValue = BuildingInfo.parameters.GetStringValueParameterById(pParamKeyRead);

            Boolean isAdd = true;
            if (pIsOptional)
                isAdd = StrFunc.IsFilled(paramValue);

            if (isAdd)
            {
                MQueueparameter parameter = new MQueueparameter(pParamKeyWrite, TypeData.TypeDataEnum.@string);
                parameter.SetValue(paramValue);
                AddMQueueAttributesParameter(parameter);
            }
            return ret;
        }
        #endregion AddBooleanParameter

        #region AddClearingHouseParameter
        /// <summary>
        /// Lecture du paramètre PARAM_CLEARINGHOUSE
        /// <para>Retourne FAILURE si la clearingHouse en entrée est non renseignée ou inconnue</para>
        /// </summary>
        /// <returns></returns>
        protected Cst.ErrLevel AddClearingHouseParameter()
        {
            return AddClearingHouseParameter(NormMsgFactoryMQueue.PARAM_CLEARINGHOUSE);
        }
        /// <summary>
        /// Lecture du paramètre PARAM_CLEARINGHOUSE et alimentation du paramètre {pParamKeyWrite}
        /// <para>Retourne FAILURE si la clearingHouse en entrée est non renseignée ou inconnue</para>
        /// <param name="pParamKeyWrite"></param>
        /// </summary>
        // EG 20190114 Add detail to ProcessLog Refactoring
        protected Cst.ErrLevel AddClearingHouseParameter(string pParamKeyWrite)
        {
            Cst.ErrLevel codeReturn = Cst.ErrLevel.SUCCESS;
            if ((BuildingInfo.parameters[NormMsgFactoryMQueue.PARAM_CLEARINGHOUSE] is MQueueparameter clearingHouse) && StrFunc.IsFilled(clearingHouse.Value))
            {
                SQL_ActorRef sqlCss = null;
                switch (clearingHouse.dataType)
                {
                    case TypeData.TypeDataEnum.@int:
                    case TypeData.TypeDataEnum.integer:
                        sqlCss = new SQL_ActorRef(m_NormMsgFactoryProcess.Cs, Cst.OTCml_TBL.ACTOR, Convert.ToInt32(clearingHouse.Value));
                        break;
                    case TypeData.TypeDataEnum.@string:
                        sqlCss = new SQL_ActorRef(m_NormMsgFactoryProcess.Cs, Cst.OTCml_TBL.ACTOR, clearingHouse.Value);
                        break;
                    default:
                        // ERROR Incorrect Datatype Parameter 
                        // FI 20200623 [XXXXX] SetErrorWarning
                        m_NormMsgFactoryProcess.ProcessState.SetErrorWarning(ProcessStateTools.StatusErrorEnum);
                        
                        
                        Logger.Log(new LoggerData(LogLevelEnum.Error, new SysMsgCode(SysCodeEnum.SYS, 8003), 2,
                            new LogParam(clearingHouse.Value + " (" + NormMsgFactoryMQueue.PARAM_CLEARINGHOUSE.ToString() + ")"),
                            new LogParam(clearingHouse.dataType),
                            new LogParam(m_NormMsgFactoryProcess.LogId),
                            new LogParam(m_NormMsgFactoryProcess.LogProcessType)));

                        codeReturn = Cst.ErrLevel.FAILURE;
                        break;
                }
                if (Cst.ErrLevel.SUCCESS == codeReturn)
                {
                    sqlCss.LoadTable(new string[] { sqlCss.AliasActorTable + ".IDA" });
                    if (sqlCss.IsLoaded)
                    {
                        MQueueparameter parameter = new MQueueparameter(pParamKeyWrite, TypeData.TypeDataEnum.integer);
                        parameter.SetValue(sqlCss.Id, sqlCss.Identifier);
                        AddMQueueAttributesParameter(parameter);
                    }
                    else
                    {
                        // ERROR ClearingHouse not found in repository
                        // FI 20200623 [XXXXX] SetErrorWarning
                        m_NormMsgFactoryProcess.ProcessState.SetErrorWarning(ProcessStateTools.StatusErrorEnum);

                        
                        Logger.Log(new LoggerData(LogLevelEnum.Error, new SysMsgCode(SysCodeEnum.SYS, 8002), 2,
                            new LogParam(clearingHouse.Value + " (" + NormMsgFactoryMQueue.PARAM_CLEARINGHOUSE.ToString() + ")"),
                            new LogParam(m_NormMsgFactoryProcess.LogId),
                            new LogParam(m_NormMsgFactoryProcess.LogProcessType)));

                        codeReturn = Cst.ErrLevel.FAILURE;
                    }
                }
            }
            else
            {
                // ERROR Parameter CLEARINGHOUSE not found
                m_NormMsgFactoryProcess.ProcessState.SetErrorWarning(ProcessStateTools.StatusErrorEnum);
                
                
                Logger.Log(new LoggerData(LogLevelEnum.Error, new SysMsgCode(SysCodeEnum.SYS, 8001), 2,
                    new LogParam(NormMsgFactoryMQueue.PARAM_CLEARINGHOUSE),
                    new LogParam(m_NormMsgFactoryProcess.LogId),
                    new LogParam(m_NormMsgFactoryProcess.LogProcessType)));

                codeReturn = Cst.ErrLevel.FAILURE;
            }
            return codeReturn;
        }
        #endregion AddClearingHouseParameter

        #region AddCssCustodianParameter
        /// <summary>
        /// Lecture du paramètre CSSCUSTODIAN et alimentation du paramètre CSSCUSTODIAN
        /// <para>
        /// Retourne Cst.ErrLevel.FAILURE si le paramètre n'existe pas ou si valeur inconnue
        /// </para>
        /// <para>Les valeurs -1,-2,-3 sont acceptées</para>
        /// </summary>
        /// <param name="pParamKeyWrite"></param>
        /// FI 20150618 [20945] Add Method
        protected Cst.ErrLevel AddCssCustodianParameter()
        {
            return AddCssCustodianParameter(NormMsgFactoryMQueue.PARAM_CSSCUSTODIAN);
        }
        /// <summary>
        /// Lecture du paramètre PARAM_CSSCUSTODIAN et alimentation du paramètre {pParamKeyWrite}
        /// Retourne Cst.ErrLevel.FAILURE si le paramètre n'existe pas ou si valeur inconnue
        /// <para>Les valeurs -1,-2,-3 sont acceptées</para>
        /// </summary>
        /// <param name="pParamKeyWrite"></param>
        /// FI 20150618 [20945] Add Method
        // EG 20190114 Add detail to ProcessLog Refactoring
        protected Cst.ErrLevel AddCssCustodianParameter(string pParamKeyWrite)
        {
            Cst.ErrLevel codeReturn = Cst.ErrLevel.SUCCESS;

            if ((BuildingInfo.parameters[NormMsgFactoryMQueue.PARAM_CSSCUSTODIAN] is MQueueparameter cssCustodian) && StrFunc.IsFilled(cssCustodian.Value))
            {
                int idACssCustodian = 0;
                SQL_ActorRef sqlActor = null;
                switch (cssCustodian.dataType)
                {
                    case TypeData.TypeDataEnum.@int:
                    case TypeData.TypeDataEnum.integer:
                        idACssCustodian = Convert.ToInt32(cssCustodian.Value);
                        if ((idACssCustodian) > 0)
                            sqlActor = new SQL_ActorRef(m_NormMsgFactoryProcess.Cs, Cst.OTCml_TBL.ACTOR, Convert.ToInt32(cssCustodian.Value));
                        break;
                    case TypeData.TypeDataEnum.@string:
                        sqlActor = new SQL_ActorRef(m_NormMsgFactoryProcess.Cs, Cst.OTCml_TBL.ACTOR, cssCustodian.Value);
                        break;
                    default:
                        // ERROR Incorrect Datatype Parameter 
                        // FI 20200623 [XXXXX] SetErrorWarning
                        m_NormMsgFactoryProcess.ProcessState.SetErrorWarning(ProcessStateTools.StatusErrorEnum);
                        
                        
                        Logger.Log(new LoggerData(LogLevelEnum.Error, new SysMsgCode(SysCodeEnum.SYS, 8003), 2,
                            new LogParam(cssCustodian.Value + " (" + NormMsgFactoryMQueue.PARAM_CSSCUSTODIAN.ToString() + ")"),
                            new LogParam(cssCustodian.dataType),
                            new LogParam(m_NormMsgFactoryProcess.LogId),
                            new LogParam(m_NormMsgFactoryProcess.LogProcessType)));

                        codeReturn = Cst.ErrLevel.FAILURE;
                        break;
                }

                if (Cst.ErrLevel.SUCCESS == codeReturn)
                {
                    Boolean isError = false;
                    if (null != sqlActor)
                    {
                        sqlActor.LoadTable(new string[] { sqlActor.AliasActorTable + ".IDA" });
                        if (sqlActor.IsLoaded)
                        {
                            MQueueparameter parameter = new MQueueparameter(pParamKeyWrite, TypeData.TypeDataEnum.integer);
                            parameter.SetValue(sqlActor.Id, sqlActor.Identifier);
                            AddMQueueAttributesParameter(parameter);
                        }
                        else
                        {
                            isError = true;
                        }
                    }
                    else
                    {
                        switch (idACssCustodian)
                        {
                            case -1:
                            case -2:
                            case -3:
                                // FI 20180328 [23871] Alimentation de extValue=> Utilisation pour alimentation du tracker (Tooltip)
                                string extValue = string.Empty;
                                if (idACssCustodian == -1)
                                    extValue = Ressource.GetString("ActorCssCustodian_ALL");
                                else if (idACssCustodian == -2)
                                    extValue = Ressource.GetString("ActorCss_ALL");
                                else if (idACssCustodian == -3)
                                    extValue = Ressource.GetString("ActorCustodian_ALL");

                                MQueueparameter parameter = new MQueueparameter(pParamKeyWrite, TypeData.TypeDataEnum.integer);
                                parameter.SetValue(idACssCustodian, extValue);
                                AddMQueueAttributesParameter(parameter);
                                break;
                            default:
                                isError = true;
                                break;
                        }
                    }

                    if (isError)
                    {
                        // FI 20200623 [XXXXX] SetErrorWarning
                        m_NormMsgFactoryProcess.ProcessState.SetErrorWarning(ProcessStateTools.StatusErrorEnum);

                        // ERROR ClearingHouse not found in repository
                        
                        Logger.Log(new LoggerData(LogLevelEnum.Error, new SysMsgCode(SysCodeEnum.SYS, 8002), 2,
                            new LogParam(cssCustodian.Value + " (" + NormMsgFactoryMQueue.PARAM_CSSCUSTODIAN.ToString() + ")"),
                            new LogParam(m_NormMsgFactoryProcess.LogId),
                            new LogParam(m_NormMsgFactoryProcess.LogProcessType)));

                        codeReturn = Cst.ErrLevel.FAILURE;
                    }
                }
            }
            else
            {
                // ERROR Parameter PARAM_CSSCUSTODIAN not found
                // FI 20200623 [XXXXX] SetErrorWarning
                m_NormMsgFactoryProcess.ProcessState.SetErrorWarning(ProcessStateTools.StatusWarningEnum);

                
                Logger.Log(new LoggerData(LogLevelEnum.Error, new SysMsgCode(SysCodeEnum.SYS, 8001), 2,
                    new LogParam(NormMsgFactoryMQueue.PARAM_CSSCUSTODIAN),
                    new LogParam(m_NormMsgFactoryProcess.LogId),
                    new LogParam(m_NormMsgFactoryProcess.LogProcessType)));

                codeReturn = Cst.ErrLevel.FAILURE;
            }
            return codeReturn;
        }
        #endregion AddCssCustodianParameter

        #region AddCurrencyParameter
        protected Cst.ErrLevel AddCurrencyParameter(string pParamKeyRead)
        {
            return AddCurrencyParameter(pParamKeyRead, pParamKeyRead);
        }
        protected Cst.ErrLevel AddCurrencyParameter(string pParamKeyRead, string pParamKeyWrite)
        {
            Cst.ErrLevel ret = Cst.ErrLevel.SUCCESS;
            string currency = BuildingInfo.parameters.GetStringValueParameterById(pParamKeyRead);
            SQL_Currency sqlCurrency = new SQL_Currency(m_NormMsgFactoryProcess.Cs, SQL_Currency.IDType.IdC, currency);
            if (false == sqlCurrency.IsLoaded)
                sqlCurrency = new SQL_Currency(m_NormMsgFactoryProcess.Cs, SQL_Currency.IDType.Iso4217, currency);
            if (sqlCurrency.IsLoaded)
            {
                MQueueparameter parameter = new MQueueparameter(pParamKeyWrite, TypeData.TypeDataEnum.@string);
                parameter.SetValue(sqlCurrency.IdC);
                AddMQueueAttributesParameter(parameter);
            }
            else
            {
                // ERROR LOG Currency is not found
            }
            return ret;
        }
        #endregion AddCurrencyParameter

        #region AddDtBusinessParameter
        /// <summary>
        /// Ajoute un paramètre DTBUSINESS à partir du paramètre DTBUSINESS
        /// </summary>
        /// <returns></returns>
        protected Cst.ErrLevel AddDtBusinessParameter()
        {
            return AddDtBusinessParameter(NormMsgFactoryMQueue.PARAM_DTBUSINESS, NormMsgFactoryMQueue.PARAM_DTBUSINESS);
        }
        /// <summary>
        /// Ajoute un paramètre {pParamKeyWrite} à partir du paramètre {pParamKeyRead}
        /// <para>Lorsque le paramètre {pParamKeyRead} n'existe, recherche de la date BUSINESS à partir du paramètre ENTITY</para>
        /// </summary>
        /// <param name="pParamKeyRead"></param>
        /// <param name="pParamKeyWrite"></param>
        /// <returns></returns>
        protected Cst.ErrLevel AddDtBusinessParameter(string pParamKeyRead, string pParamKeyWrite)
        {
            return AddDtBusinessParameter(pParamKeyRead, pParamKeyWrite, null);
        }

        /// <summary>
        /// Ajoute un paramètre {pParamKeyWrite} à partir du paramètre {pParamKeyRead}
        /// <para>Lorsque le paramètre {pParamKeyRead} n'existe pas, recherche de la date BUSINESS</para>
        /// <para> - à partir des paramètre ENTITY et {pParamKeyWriteClearingHouse} (lorsque renseigné) ou </para>
        /// <para> - à partir des paramètre ENTITY  </para>
        /// </summary>
        /// <param name="pParamKeyRead">Nom du Paramètre en entrée qui contient la Date business</param>
        /// <param name="pParamKeyWrite">Paramètre en entrée qui contient la Date business</param>
        /// <param name="pParamKeyWriteCSSCustodian"></param>
        /// <returns></returns>
        // EG 20180307 [23769] Gestion dbTransaction
        // EG 20190114 Add detail to ProcessLog Refactoring
        protected Cst.ErrLevel AddDtBusinessParameter(string pParamKeyRead, string pParamKeyWrite, string pParamKeyWriteCSSCustodian)
        {
            Cst.ErrLevel codeReturn = Cst.ErrLevel.SUCCESS;
            DateTime dtProcess = DateTime.MinValue;
            if (null == BuildingInfo.parameters.GetObjectValueParameterById(pParamKeyRead))
            {
                if ((m_MQueueAttributes.parameters[MQueueBase.PARAM_ENTITY] is MQueueparameter entity) && StrFunc.IsFilled(entity.Value))
                {
                    Nullable<int> idEM = 0;
                    MQueueparameter cssCustodian = null;
                    if (StrFunc.IsFilled(pParamKeyWriteCSSCustodian))
                    {
                        cssCustodian = m_MQueueAttributes.parameters[pParamKeyWriteCSSCustodian] as MQueueparameter;
                        if ((null != cssCustodian) && StrFunc.IsFilled(cssCustodian.Value))
                        {
                            int[] idMarketForCss = MarketTools.CSSGetMarket(m_NormMsgFactoryProcess.Cs, null,
                                Convert.ToInt32(cssCustodian.Value), SQL_Table.ScanDataDtEnabledEnum.Yes);

                            if (0 < ArrFunc.Count(idMarketForCss))
                            {
                                //FI 20131016 [19062] appel à MarketTools.GetEntityMarket_MaxDtMarket pour compabilité ascendante
                                //idEM = MarketTools.GetEntityMarket_LastDtMarket(m_NormMsgFactoryProcess.Cs, Convert.ToInt32(entity.Value), idMarketForCss);
                                idEM = MarketTools.GetEntityMarket_MaxDtEntity(m_NormMsgFactoryProcess.Cs, null, Convert.ToInt32(entity.Value), idMarketForCss);
                            }
                            else
                            {
                                // FI 20200623 [XXXXX] SetErrorWarning
                                m_NormMsgFactoryProcess.ProcessState.SetErrorWarning(ProcessStateTools.StatusErrorEnum);

                                // ERROR MARKET not found for the specified CLEARINGHOUSE
                                
                                Logger.Log(new LoggerData(LogLevelEnum.Error, new SysMsgCode(SysCodeEnum.SYS, 8007), 2,
                                    new LogParam(LogTools.IdentifierAndId(cssCustodian.ExValue, Convert.ToInt32(cssCustodian.Value))),
                                    new LogParam(m_NormMsgFactoryProcess.LogId),
                                    new LogParam(m_NormMsgFactoryProcess.LogProcessType)));

                                codeReturn = Cst.ErrLevel.FAILURE;
                            }
                        }
                        else
                        {
                            m_NormMsgFactoryProcess.ProcessState.SetErrorWarning(ProcessStateTools.StatusErrorEnum);

                            // ERROR Parameter CLEARINGHOUSE not found
                            
                            Logger.Log(new LoggerData(LogLevelEnum.Error, new SysMsgCode(SysCodeEnum.SYS, 8001), 2,
                                new LogParam(pParamKeyWriteCSSCustodian),
                                new LogParam(m_NormMsgFactoryProcess.LogId),
                                new LogParam(m_NormMsgFactoryProcess.LogProcessType)));

                            codeReturn = Cst.ErrLevel.FAILURE;
                        }
                    }
                    else
                    {
                        //FI 20131016 [19062] cas particulier sur le traitement CASHBALANCE
                        if (BuildingInfo.processType == Cst.ProcessTypeEnum.CASHBALANCE)
                        {
                            idEM = GetEntityMarket_MaxDtEODProcess(m_NormMsgFactoryProcess.Cs, Convert.ToInt32(entity.Value));
                        }
                        else
                        {
                            //FI 20131016 [19062] appel à MarketTools.GetEntityMarket_MaxDtMarket pour compabilité ascendante
                            //idEM = MarketTools.GetEntityMarket_LastDtMarket(m_NormMsgFactoryProcess.Cs, Convert.ToInt32(entity.Value), null);
                            idEM = MarketTools.GetEntityMarket_MaxDtEntity(m_NormMsgFactoryProcess.Cs, null, Convert.ToInt32(entity.Value), null);
                        }
                    }


                    if (idEM.HasValue)
                    {
                        SQL_EntityMarket sqlEntityMarket = new SQL_EntityMarket(m_NormMsgFactoryProcess.Cs, idEM.Value);
                        //PM 20150512 [20575] Gestion DTENTITY
                        //sqlEntityMarket.LoadTable(new string[] { "ENTITYMARKET.DTMARKET" });
                        //dtProcess = sqlEntityMarket.dtMarket;
                        sqlEntityMarket.LoadTable(new string[] { "ENTITYMARKET.DTENTITY" });
                        dtProcess = sqlEntityMarket.DtEntity;
                    }
                    else
                    {
                        // FI 20200623 [XXXXX] SetErrorWarning
                        m_NormMsgFactoryProcess.ProcessState.SetErrorWarning(ProcessStateTools.StatusErrorEnum);

                        // ERROR DTMARKET not found for ENTITY/[CLEARINGHOUSE]
                        
                        Logger.Log(new LoggerData(LogLevelEnum.Error, new SysMsgCode(SysCodeEnum.SYS, 8008), 2,
                            new LogParam(LogTools.IdentifierAndId(entity.ExValue, Convert.ToInt32(entity.Value))),
                            new LogParam((null != cssCustodian) ? LogTools.IdentifierAndId(cssCustodian.ExValue, Convert.ToInt32(cssCustodian.Value)) : "-"),
                            new LogParam(m_NormMsgFactoryProcess.LogId),
                            new LogParam(m_NormMsgFactoryProcess.LogProcessType)));

                        codeReturn = Cst.ErrLevel.FAILURE;
                    }
                }
                else
                {
                    // FI 20200623 [XXXXX] SetErrorWarning
                    m_NormMsgFactoryProcess.ProcessState.SetErrorWarning(ProcessStateTools.StatusErrorEnum);

                    // ERROR Parameter ENTITY not found
                    
                    Logger.Log(new LoggerData(LogLevelEnum.Error, new SysMsgCode(SysCodeEnum.SYS, 8001), 2,
                        new LogParam(MQueueBase.PARAM_ENTITY),
                        new LogParam(m_NormMsgFactoryProcess.LogId),
                        new LogParam(m_NormMsgFactoryProcess.LogProcessType)));

                    codeReturn = Cst.ErrLevel.FAILURE;
                }
            }
            else
            {
                dtProcess = BuildingInfo.parameters.GetDateTimeValueParameterById(NormMsgFactoryMQueue.PARAM_DTBUSINESS);

            }
            //Ajoute le paramètre DTBUSINESS
            if (Cst.ErrLevel.SUCCESS == codeReturn)
            {
                MQueueparameter parameter = new MQueueparameter(pParamKeyWrite, TypeData.TypeDataEnum.date);
                parameter.SetValue(dtProcess);
                if (null == m_MQueueAttributes.parameters)
                    m_MQueueAttributes.parameters = new MQueueparameters();
                m_MQueueAttributes.parameters.Add(parameter);
            }
            return codeReturn;
        }
        #endregion AddDtBusinessParameter
        #region AddDtInvoicingParameter
        protected Cst.ErrLevel AddDtInvoicingParameter()
        {
            return AddDtInvoicingParameter(NormMsgFactoryMQueue.PARAM_DATE1);
        }
        protected Cst.ErrLevel AddDtInvoicingParameter(string pParamKeyWrite)
        {
            Cst.ErrLevel codeReturn = Cst.ErrLevel.SUCCESS;
            DateTime dt;
            if (null == BuildingInfo.parameters.GetObjectValueParameterById(NormMsgFactoryMQueue.PARAM_DTINVOICING))
                dt = new DtFunc().StringToDateTime("TODAY-1EOM");
            else
                dt = BuildingInfo.parameters.GetDateTimeValueParameterById(NormMsgFactoryMQueue.PARAM_DTINVOICING);

            if (Cst.ErrLevel.SUCCESS == codeReturn)
            {
                MQueueparameter parameter = new MQueueparameter(pParamKeyWrite, TypeData.TypeDataEnum.date);
                parameter.SetValue(dt);
                if (null == m_MQueueAttributes.parameters)
                    m_MQueueAttributes.parameters = new MQueueparameters();
                m_MQueueAttributes.parameters.Add(parameter);
            }
            return codeReturn;
        }
        #endregion AddDtInvoicingParameter
        #region AddDateParameter
        /// <summary>
        /// Ajoute un paramètre à partir du paramètre nommé {pParamKeyRead}
        /// <para>Si le paramètre {pParamKeyRead} n'existe pas, Spheres® utilise {pDefaultValue} comme valeur par défaut</para>
        /// </summary>
        /// <param name="pParamKeyRead"></param>
        /// <param name="pDefaultValue">
        /// <para>date au format ISO, ou TODAY, OU BUSINESS</para>
        /// <para>- Lorsque BUSINESS est renseigné, Spheres® récupère la date courante la plus récente vis à vis de l'entité (PARAM_ENTITY doit alors exister)</para>
        /// </param>
        /// <returns></returns>
        protected Cst.ErrLevel AddDateParameter(string pParamKeyRead, string pDefaultValue)
        {
            return AddDateParameter(pParamKeyRead, pParamKeyRead, pDefaultValue);
        }
        /// <summary>
        /// Ajoute un paramètre {pParamKeyWrite} à partir du paramètre nommé {pParamKeyRead}
        /// <para>- Si le paramètre {pParamKeyRead} n'existe pas, Spheres® utilise {pDefaultValue} comme valeur par défaut</para>
        /// </summary>
        /// <param name="pParamKeyRead"></param>
        /// <param name="pParamKeyWrite"></param>
        /// <param name="pDefaultValue">
        /// <para>date au format ISO, ou TODAY, OU BUSINESS</para>
        /// <para>- Lorsque BUSINESS est renseigné, Spheres® récupère la date courante la plus récente vis à vis de l'entité (PARAM_ENTITY doit alors exister)</para>
        /// </param>
        /// <returns></returns>
        protected Cst.ErrLevel AddDateParameter(string pParamKeyRead, string pParamKeyWrite, string pDefaultValue)
        {
            Cst.ErrLevel codeReturn = Cst.ErrLevel.SUCCESS;

            DateTime dt = DateTime.MinValue;
            if (null == BuildingInfo.parameters.GetObjectValueParameterById(pParamKeyRead))
            {
                if (StrFunc.IsFilled(pDefaultValue))
                {
                    if (pDefaultValue == DtFuncML.BUSINESS)
                        return AddDtBusinessParameter(pParamKeyRead, pParamKeyWrite);
                    else
                        dt = new DtFunc().StringToDateTime(pDefaultValue);
                }
            }
            else
                dt = BuildingInfo.parameters.GetDateTimeValueParameterById(pParamKeyRead);

            if (Cst.ErrLevel.SUCCESS == codeReturn)
            {
                MQueueparameter parameter = new MQueueparameter(pParamKeyWrite, TypeData.TypeDataEnum.date);
                parameter.SetValue(dt);

                AddMQueueAttributesParameter(parameter);
            }
            return codeReturn;
        }
        #endregion AddDateParameter

        #region AddAccountingRequestTypeParameter
        // EG 20190114 Add detail to ProcessLog Refactoring
        protected Cst.ErrLevel AddAccountingRequestTypeParameter()
        {
            Cst.ErrLevel codeReturn = Cst.ErrLevel.SUCCESS;
            string requestType = BuildingInfo.parameters.GetStringValueParameterById(NormMsgFactoryMQueue.PARAM_REQUESTTYPE);
            if (StrFunc.IsFilled(requestType))
            {
                if (("COMMON" != requestType) && ("ADMIN" != requestType) && ("CBI" != requestType))
                {
                    // FI 20200623 [XXXXX] SetErrorWarning
                    m_NormMsgFactoryProcess.ProcessState.SetErrorWarning(ProcessStateTools.StatusErrorEnum);

                    // Type de flux incorrect
                    
                    Logger.Log(new LoggerData(LogLevelEnum.Error, new SysMsgCode(SysCodeEnum.SYS, 8004), 2,
                        new LogParam(requestType + " (" + NormMsgFactoryMQueue.PARAM_REQUESTTYPE.ToString() + ")"),
                        new LogParam(m_NormMsgFactoryProcess.LogId),
                        new LogParam(m_NormMsgFactoryProcess.LogProcessType)));

                    codeReturn = Cst.ErrLevel.FAILURE;
                }
            }
            else
                requestType = "COMMON";

            if (Cst.ErrLevel.SUCCESS == codeReturn)
            {
                MQueueparameter parameter = new MQueueparameter(NormMsgFactoryMQueue.PARAM_REQUESTTYPE, TypeData.TypeDataEnum.@string);
                parameter.SetValue(requestType, requestType);
                AddMQueueAttributesParameter(parameter);
            }
            return codeReturn;
        }
        #endregion AddAccountingRequestTypeParameter
        #region AddEntityParameter
        /// <summary>
        /// Lecture du paramètre en entrée PARAM_ENTITY et génère le paramètre PARAM_ENTITY
        /// <para>Retourne Cst.ErrLevel.FAILURE si le paramètre PARAM_ENTITY n'existe pas ou si renseigné avec une donnée inconnue</para>
        /// </summary>
        /// <returns></returns>
        protected Cst.ErrLevel AddEntityParameter()
        {
            return AddEntityParameter(NormMsgFactoryMQueue.PARAM_ENTITY);
        }

        /// <summary>
        /// Lecture du paramètre en entrée PARAM_ENTITY et génère le paramètre {pParamKeyWrite}
        /// <para>Retourne Cst.ErrLevel.FAILURE si le paramètre PARAM_ENTITY n'existe pas ou si renseigné avec une donnée inconnue</para>
        /// </summary>
        /// <param name="pParamKeyWrite"></param>
        /// <returns></returns>
        // FI 20130502 [] appel à la méthode NewSqlEntity
        // EG 20190114 Add detail to ProcessLog Refactoring
        protected Cst.ErrLevel AddEntityParameter(string pParamKeyWrite)
        {
            Cst.ErrLevel codeReturn;
            if ((BuildingInfo.parameters[NormMsgFactoryMQueue.PARAM_ENTITY] is MQueueparameter entity) && StrFunc.IsFilled(entity.Value))
            {
                codeReturn = NewSqlEntity(entity, out SQL_Entity sqlEntity);

                if (Cst.ErrLevel.SUCCESS == codeReturn)
                {
                    sqlEntity.LoadTable(new string[] { sqlEntity.AliasActorTable + ".IDA" });
                    if (sqlEntity.IsLoaded)
                    {
                        MQueueparameter parameter = new MQueueparameter(pParamKeyWrite, TypeData.TypeDataEnum.integer);
                        parameter.SetValue(sqlEntity.Id, sqlEntity.Identifier);
                        AddMQueueAttributesParameter(parameter);
                    }
                    else
                    {
                        // FI 20200623 [XXXXX] SetErrorWarning
                        m_NormMsgFactoryProcess.ProcessState.SetErrorWarning(ProcessStateTools.StatusErrorEnum);

                        // ERROR Entity not found in repository
                        
                        Logger.Log(new LoggerData(LogLevelEnum.Error, new SysMsgCode(SysCodeEnum.SYS, 8002), 2,
                            new LogParam(entity.Value + " (" + NormMsgFactoryMQueue.PARAM_ENTITY.ToString() + ")"),
                            new LogParam(m_NormMsgFactoryProcess.LogId),
                            new LogParam(m_NormMsgFactoryProcess.LogProcessType)));

                        codeReturn = Cst.ErrLevel.FAILURE;
                    }
                }
            }
            else
            {
                // FI 20200623 [XXXXX] SetErrorWarning
                m_NormMsgFactoryProcess.ProcessState.SetErrorWarning(ProcessStateTools.StatusErrorEnum);

                // ERROR Parameter ENTITY not found
                
                Logger.Log(new LoggerData(LogLevelEnum.Error, new SysMsgCode(SysCodeEnum.SYS, 8001), 2,
                    new LogParam(NormMsgFactoryMQueue.PARAM_ENTITY),
                    new LogParam(m_NormMsgFactoryProcess.LogId),
                    new LogParam(m_NormMsgFactoryProcess.LogProcessType)));

                codeReturn = Cst.ErrLevel.FAILURE;

            }
            return codeReturn;
        }
        #endregion AddEntityParameter

        #region AddFlowTypeParameter
        protected Cst.ErrLevel AddFlowTypeParameter()
        {
            return AddFlowTypeParameter(NormMsgFactoryMQueue.PARAM_CLASS);
        }
        // EG 20190114 Add detail to ProcessLog Refactoring
        protected Cst.ErrLevel AddFlowTypeParameter(string pParamKeyWrite)
        {
            Cst.ErrLevel codeReturn = Cst.ErrLevel.SUCCESS;
            string flowType = BuildingInfo.parameters.GetStringValueParameterById(NormMsgFactoryMQueue.PARAM_CLASS);
            if (StrFunc.IsFilled(flowType))
            {
                if (false == Enum.IsDefined(typeof(Cst.FlowTypeEnum), flowType))
                {
                    // FI 20200623 [XXXXX] SetErrorWarning
                    m_NormMsgFactoryProcess.ProcessState.SetErrorWarning(ProcessStateTools.StatusErrorEnum);

                    // Type de flux incorrect
                    
                    Logger.Log(new LoggerData(LogLevelEnum.Error, new SysMsgCode(SysCodeEnum.SYS, 8004), 2,
                        new LogParam(flowType + " (" + NormMsgFactoryMQueue.PARAM_CLASS.ToString() + ")"),
                        new LogParam(m_NormMsgFactoryProcess.LogId),
                        new LogParam(m_NormMsgFactoryProcess.LogProcessType)));

                    codeReturn = Cst.ErrLevel.FAILURE;
                }
            }
            else
                flowType = Cst.FlowTypeEnum.ALL.ToString();

            if (Cst.ErrLevel.SUCCESS == codeReturn)
            {
                MQueueparameter parameter = new MQueueparameter(pParamKeyWrite, TypeData.TypeDataEnum.@string);
                parameter.SetValue(flowType, flowType);
                AddMQueueAttributesParameter(parameter);
            }
            return codeReturn;
        }
        #endregion AddFlowTypeParameter

        

        #region AddGroupParameter
        /// <summary>
        ///  Ajoute le paramètre qui représente un groupe {pParamKeyWrite} s'il existe dans le message reçu
        /// </summary>
        /// <param name="pParamKeyWrite"></param>
        /// <param name="pRole"></param>
        /// <returns></returns>
        /// FI 20141230 [20616] Add Method
        // EG 20190114 Add detail to ProcessLog Refactoring
        protected Cst.ErrLevel AddGroupParameter(string pParamKeyWrite, string pRole)
        {
            Cst.ErrLevel codeReturn = Cst.ErrLevel.SUCCESS;

            if ((BuildingInfo.parameters[pParamKeyWrite] is MQueueparameter g) && StrFunc.IsFilled(g.Value))
            {
                if (false == Enum.IsDefined(typeof(Cst.OTCml_TBL), pParamKeyWrite))
                    throw new Exception(StrFunc.AppendFormat("value: {0} is not allowed", pParamKeyWrite));

                Cst.OTCml_TBL table = (Cst.OTCml_TBL)Enum.Parse(typeof(Cst.OTCml_TBL), pParamKeyWrite);

                SQL_Group sqlG = null;
                switch (g.dataType)
                {
                    case TypeData.TypeDataEnum.@int:
                    case TypeData.TypeDataEnum.integer:
                        sqlG = new SQL_Group(m_NormMsgFactoryProcess.Cs, table, Convert.ToInt32(g.Value), SQL_Table.ScanDataDtEnabledEnum.Yes);
                        break;
                    case TypeData.TypeDataEnum.@string:
                        sqlG = new SQL_Group(m_NormMsgFactoryProcess.Cs, table, SQL_TableWithID.IDType.Identifier, g.Value, SQL_Table.ScanDataDtEnabledEnum.Yes);
                        break;
                    default:
                        // ERROR Incorrect Datatype Parameter 
                        // FI 20200623 [XXXXX] SetErrorWarning
                        m_NormMsgFactoryProcess.ProcessState.SetErrorWarning(ProcessStateTools.StatusErrorEnum);

                        
                        Logger.Log(new LoggerData(LogLevelEnum.Error, new SysMsgCode(SysCodeEnum.SYS, 8003), 2,
                            new LogParam(g.Value + " (" + pParamKeyWrite + ")"),
                            new LogParam(g.dataType),
                            new LogParam(m_NormMsgFactoryProcess.LogId),
                            new LogParam(m_NormMsgFactoryProcess.LogProcessType)));

                        codeReturn = Cst.ErrLevel.FAILURE;
                        break;
                }

                if (Cst.ErrLevel.SUCCESS == codeReturn)
                {
                    sqlG.Role = pRole;
                    string columId = StrFunc.AppendFormat("{0}.{1}", sqlG.SQLObject, OTCmlHelper.GetColunmID(sqlG.SQLObject));
                    string columIdentifier = StrFunc.AppendFormat("{0}.{1}", sqlG.SQLObject, "IDENTIFIER");
                    sqlG.LoadTable(new string[] { columId, columIdentifier });
                    if (sqlG.IsLoaded)
                    {
                        MQueueparameter parameter = new MQueueparameter(pParamKeyWrite, TypeData.TypeDataEnum.integer);
                        parameter.SetValue(sqlG.Id, sqlG.Identifier);
                        AddMQueueAttributesParameter(parameter);
                    }
                    else
                    {
                        // FI 20200623 [XXXXX] SetErrorWarning
                        m_NormMsgFactoryProcess.ProcessState.SetErrorWarning(ProcessStateTools.StatusErrorEnum);

                        // Groupe non trouvé ou avec rôle différent
                        
                        Logger.Log(new LoggerData(LogLevelEnum.Error, new SysMsgCode(SysCodeEnum.SYS, 8005), 2,
                            new LogParam(g.Value + " (" + pParamKeyWrite + ")"),
                            new LogParam(StrFunc.IsFilled(pRole) ? pRole : "-"),
                            new LogParam(m_NormMsgFactoryProcess.LogId),
                            new LogParam(m_NormMsgFactoryProcess.LogProcessType)));

                        codeReturn = Cst.ErrLevel.FAILURE;
                    }
                }
            }
            return codeReturn;
        }
        #endregion AddGroupParameter

        #region AddMarketParameter
        /// <summary>
        /// Ajoute le paramètre MARKET s'il existe dans le message reçu
        /// </summary>
        /// <param name="pIdGMarket"></param>
        /// <returns></returns>
        protected Cst.ErrLevel AddMarketParameter(Nullable<int> pIdGMarket)
        {
            return AddMarketParameter(NormMsgFactoryMQueue.PARAM_MARKET, pIdGMarket);
        }
        /// <summary>
        /// Ajoute le paramètre {pParamKeyWrite} s'il existe dans le message reçu le paramètre MARKET
        /// </summary>
        /// <param name="pParamKeyWrite"></param>
        /// <param name="pIdGMarket"></param>
        /// <returns></returns>
        /// FI 20170327 [23004] Modify 
        // EG 20190114 Add detail to ProcessLog Refactoring
        protected Cst.ErrLevel AddMarketParameter(string pParamKeyWrite, Nullable<int> pIdGMarket)
        {
            // FI 20170327 [23004]  Evolution (la valeur -1 (tous marchés) est acceptée)
            Cst.ErrLevel codeReturn = Cst.ErrLevel.SUCCESS;
            if ((BuildingInfo.parameters[NormMsgFactoryMQueue.PARAM_MARKET] is MQueueparameter market) && StrFunc.IsFilled(market.Value))
            {
                int idM = 0;
                SQL_Market sqlMarket = null;
                switch (market.dataType)
                {
                    case TypeData.TypeDataEnum.@int:
                    case TypeData.TypeDataEnum.integer:
                        idM = Convert.ToInt32(market.Value);
                        if (idM > 0)
                            sqlMarket = new SQL_Market(m_NormMsgFactoryProcess.Cs, idM, SQL_Table.ScanDataDtEnabledEnum.Yes);
                        break;
                    case TypeData.TypeDataEnum.@string:
                        sqlMarket = new SQL_Market(m_NormMsgFactoryProcess.Cs, market.Value, SQL_Table.ScanDataDtEnabledEnum.Yes);
                        break;
                    default:
                        // FI 20200623 [XXXXX] SetErrorWarning
                        m_NormMsgFactoryProcess.ProcessState.SetErrorWarning(ProcessStateTools.StatusWarningEnum);

                        // ERROR Incorrect Datatype Parameter 
                        
                        Logger.Log(new LoggerData(LogLevelEnum.Error, new SysMsgCode(SysCodeEnum.SYS, 8003), 2,
                            new LogParam(market.Value + " (" + NormMsgFactoryMQueue.PARAM_MARKET.ToString() + ")"),
                            new LogParam(market.dataType),
                            new LogParam(m_NormMsgFactoryProcess.LogId),
                            new LogParam(m_NormMsgFactoryProcess.LogProcessType)));

                        codeReturn = Cst.ErrLevel.FAILURE;
                        break;
                }
                if (Cst.ErrLevel.SUCCESS == codeReturn)
                {
                    Boolean isError = false;
                    if (null != sqlMarket)
                    {
                        sqlMarket.IdGMarket = pIdGMarket;
                        sqlMarket.LoadTable(new string[] { "MARKET.IDM, MARKET.IDENTIFIER" });
                        if (sqlMarket.IsLoaded)
                        {
                            MQueueparameter parameter = new MQueueparameter(pParamKeyWrite, TypeData.TypeDataEnum.integer);
                            parameter.SetValue(sqlMarket.Id, sqlMarket.Identifier);
                            AddMQueueAttributesParameter(parameter);
                        }
                        else
                        {
                            isError = true;
                        }
                    }
                    else
                    {
                        switch (idM)
                        {
                            case -1:
                                MQueueparameter parameter = new MQueueparameter(pParamKeyWrite, TypeData.TypeDataEnum.integer);
                                parameter.SetValue(idM);
                                AddMQueueAttributesParameter(parameter);
                                break;
                        }
                    }

                    if (isError)
                    {
                        // ERROR = Add LOG
                        // Marché non trouvé ou n'appartenant pas au groupe de marché spécifié
                        // FI 20200623 [XXXXX] SetErrorWarning
                        m_NormMsgFactoryProcess.ProcessState.SetErrorWarning(ProcessStateTools.StatusErrorEnum);

                        
                        Logger.Log(new LoggerData(LogLevelEnum.Error, new SysMsgCode(SysCodeEnum.SYS, 8006), 2,
                            new LogParam(market.Value + " (" + NormMsgFactoryMQueue.PARAM_MARKET.ToString() + ")"),
                            new LogParam(pIdGMarket.HasValue ? pIdGMarket.Value.ToString() : "-"),
                            new LogParam(m_NormMsgFactoryProcess.LogId),
                            new LogParam(m_NormMsgFactoryProcess.LogProcessType)));

                        codeReturn = Cst.ErrLevel.FAILURE;
                    }
                }
            }
            return codeReturn;
        }
        #endregion AddMarketParameter

        #region AddTimingParameter
        /// <summary>
        /// Ajoute le paramètre TIMING à partir du paramètre TIMING (Valeurs possibles ITD et EOD)
        /// </summary>
        /// <returns></returns>
        protected Cst.ErrLevel AddTimingParameter()
        {
            return AddTimingParameter(NormMsgFactoryMQueue.PARAM_TIMING);
        }
        /// <summary>
        /// Ajoute le paramètre pParamKeyWrite à partir du paramètre TIMING (Valeurs possibles ITD et EOD)
        /// </summary>
        /// <param name="pParamKeyWrite"></param>
        /// <returns></returns>
        // EG 20190114 Add detail to ProcessLog Refactoring
        protected Cst.ErrLevel AddTimingParameter(string pParamKeyWrite)
        {
            Cst.ErrLevel codeReturn = Cst.ErrLevel.SUCCESS;
            if ((BuildingInfo.parameters[NormMsgFactoryMQueue.PARAM_TIMING] is MQueueparameter timing) && StrFunc.IsFilled(timing.Value))
            {
                if (Enum.IsDefined(typeof(FixML.Enum.SettlSessIDEnum),
                    ReflectionTools.GetMemberNameByXmlEnumAttribute(typeof(FixML.Enum.SettlSessIDEnum), timing.Value, true)))
                {
                    MQueueparameter parameter = new MQueueparameter(pParamKeyWrite, TypeData.TypeDataEnum.@string);
                    parameter.SetValue(timing.Value);
                    AddMQueueAttributesParameter(parameter);
                }
                else
                {

                    // Type de traitement incorrect
                    // FI 20200623 [XXXXX] SetErrorWarning
                    m_NormMsgFactoryProcess.ProcessState.SetErrorWarning(ProcessStateTools.StatusErrorEnum);

                    
                    Logger.Log(new LoggerData(LogLevelEnum.Error, new SysMsgCode(SysCodeEnum.SYS, 8004), 2,
                        new LogParam(timing + " (" + NormMsgFactoryMQueue.PARAM_TIMING.ToString() + ")"),
                        new LogParam(m_NormMsgFactoryProcess.LogId),
                        new LogParam(m_NormMsgFactoryProcess.LogProcessType)));

                    codeReturn = Cst.ErrLevel.FAILURE;
                }
            }
            else
            {
                // FI 20200623 [XXXXX] SetErrorWarning
                m_NormMsgFactoryProcess.ProcessState.SetErrorWarning(ProcessStateTools.StatusErrorEnum);

                // ERROR Parameter Timing not found
                
                Logger.Log(new LoggerData(LogLevelEnum.Error, new SysMsgCode(SysCodeEnum.SYS, 8001), 2,
                    new LogParam(NormMsgFactoryMQueue.PARAM_TIMING),
                    new LogParam(m_NormMsgFactoryProcess.LogId),
                    new LogParam(m_NormMsgFactoryProcess.LogProcessType)));

                codeReturn = Cst.ErrLevel.FAILURE;

            }
            return codeReturn;
        }
        #endregion AddTimingParameter

        /// <summary>
        ///  Interprète la donnée {pData} et retourne la donnée au format ISO
        ///  <para>Cette méthode interprète les mots clefs TODAY, DTBUSINESS, etc..</para>
        /// </summary>
        /// <param name="pQueue"></param>
        /// <returns></returns>
        /// FI 20130430 [] add method
        protected string ConvertData(string pCS, string pData, TypeData.TypeDataEnum pDateDataType,
            int pIdAEntity, int pIdM, int pIdACss)
        {
            string ret = pData;

            DateTime date;
            switch (pDateDataType)
            {
                case TypeData.TypeDataEnum.datetime:
                    date = new DtFuncML(pCS, string.Empty, pIdAEntity, pIdM, pIdACss, null).StringToDateTime(pData, DtFunc.FmtISODateTime, true);
                    ret = ObjFunc.FmtToISo(date, pDateDataType);
                    break;
                case TypeData.TypeDataEnum.date:
                    date = new DtFuncML(pCS, string.Empty, pIdAEntity, pIdM, pIdACss, null).StringToDateTime(pData, DtFunc.FmtISODate, true);
                    ret = ObjFunc.FmtToISo(date, pDateDataType);
                    break;
                default:
                    break;
            }

            return ret;
        }

        /// <summary>
        ///  Retourne une nouvelle instance spécifié en tant que paramètre dans le message NormMsgFactoryMQueue
        /// </summary>
        /// <returns></returns>
        /// <param name="entity">Représente le paramètre</param>
        /// <param name="pEntity"></param>
        /// FI 20130502 [] add method NewSqlEntity 
        // EG 20190114 Add detail to ProcessLog Refactoring
        protected Cst.ErrLevel NewSqlEntity(MQueueparameter entity, out SQL_Entity pEntity)
        {
            Cst.ErrLevel codeReturn = Cst.ErrLevel.SUCCESS;
            pEntity = null;

            switch (entity.dataType)
            {
                case TypeData.TypeDataEnum.@int:
                case TypeData.TypeDataEnum.integer:
                    pEntity = new SQL_Entity(m_NormMsgFactoryProcess.Cs, Convert.ToInt32(entity.Value));
                    break;
                case TypeData.TypeDataEnum.@string:
                    pEntity = new SQL_Entity(m_NormMsgFactoryProcess.Cs, entity.Value);
                    break;
                default:
                    // FI 20200623 [XXXXX] SetErrorWarning
                    m_NormMsgFactoryProcess.ProcessState.SetErrorWarning(ProcessStateTools.StatusErrorEnum); 

                    
                    
                    Logger.Log(new LoggerData(LogLevelEnum.Error, new SysMsgCode(SysCodeEnum.SYS, 8003), 2,
                        new LogParam(entity.Value + " (" + NormMsgFactoryMQueue.PARAM_ENTITY.ToString() + ")"),
                        new LogParam(entity.dataType),
                        new LogParam(m_NormMsgFactoryProcess.LogId),
                        new LogParam(m_NormMsgFactoryProcess.LogProcessType)));

                    codeReturn = Cst.ErrLevel.FAILURE;
                    break;
            }
            return codeReturn;
        }

        /// <summary>
        /// Retourne SQL_Entity s'il existe le paramètre PARAM_ENTITY dans le NormMsgFactoryMQueue
        /// </summary>
        /// <returns></returns>
        /// 20130502 [] 
        protected SQL_Entity LoadParameterEntity()
        {
            SQL_Entity sqlEntity = null;
            //PL 20131008 Add test on BuildingInfo.parametersSpecified
            if (BuildingInfo.parametersSpecified)
            {
                if (BuildingInfo.parameters[NormMsgFactoryMQueue.PARAM_ENTITY] is MQueueparameter paramEntity)
                {
                    NewSqlEntity(paramEntity, out sqlEntity);
                    if (sqlEntity != null)
                        sqlEntity.LoadTable(new string[] { string.Concat(sqlEntity.AliasActorTable, ".IDA"),
                                                       string.Concat(sqlEntity.AliasActorTable, ".IDENTIFIER")});
                }
            }
            return sqlEntity;
        }

        /// <summary>
        /// Retourne l'enregistrement dans ENTITYMARKET (colonne IDEM) qui correspond traitement EOD le plus récent exécuté pour une entité (peu importe le statut final du traitement EOD)
        /// <para>retourne 0 s'il n'existe aucune ligne dans ENTITYMARKET</para>
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pIdAEntity">Représente l'entité</param>
        /// <returns></returns>
        /// FI 20131016 [19062] add Method
        /// PM 20150512 [20575] Gestion DTENTITY
        // EG 20180425 Analyse du code Correction [CA2202]
        private static int GetEntityMarket_MaxDtEODProcess(string pCS, int pIdAEntity)
        {
            int ret = 0;

            DataParameters parameters = new DataParameters();
            parameters.Add(new DataParameter(pCS, "IDA", DbType.Int32), pIdAEntity);

            StrBuilder sql = new StrBuilder(SQLCst.SELECT);
            sql += "em.IDEM" + Cst.CrLf;
            sql += SQLCst.FROM_DBO + Cst.OTCml_TBL.ENTITYMARKET + " em" + Cst.CrLf;
            //sql += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.POSREQUEST + " pr on pr.REQUESTTYPE='EOD' and pr.IDA_ENTITY = em.IDA and pr.DTBUSINESS = em.DTMARKET" + Cst.CrLf;
            sql += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.POSREQUEST + " pr on pr.REQUESTTYPE='EOD' and pr.IDA_ENTITY = em.IDA and pr.DTBUSINESS = em.DTENTITY" + Cst.CrLf;

            SQLWhere sqlwhere = new SQLWhere();
            sqlwhere.Append("em.IDA=@IDA");
            sql += sqlwhere.ToString();

            //sql += SQLCst.ORDERBY + "em.DTMARKET desc" + Cst.CrLf;
            sql += SQLCst.ORDERBY + "em.DTENTITY desc" + Cst.CrLf;

            QueryParameters qryParameters = new QueryParameters(pCS, sql.ToString(), parameters);

            using (IDataReader dr = DataHelper.ExecuteReader(pCS, CommandType.Text, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter()))
            {
                if (dr.Read())
                    ret = Convert.ToInt32(dr["IDEM"]);
            }
            return ret;
        }

        /// <summary>
        /// Ajoute  {parameter} dans m_MQueueAttributes.parameters
        /// </summary>
        /// <param name="parameter"></param>
        /// FI 20150618 [20945] Add Method
        protected void AddMQueueAttributesParameter(MQueueparameter parameter)
        {
            if (null == m_MQueueAttributes.parameters)
                m_MQueueAttributes.parameters = new MQueueparameters();

            m_MQueueAttributes.parameters.Add(parameter);
        }

        /// <summary>
        ///  Génère un paramètre d'entrée PARAM_CSSCUSTODIAN s'il est non présent et s'il existe un paramètre PARAM_CLEARINGHOUSE
        /// </summary>
        protected void AddBuildingInfoParameterCssCustodianParamerfromClearingHouse(Boolean pIsAddGPRODUCT)
        {
            if (BuildingInfo.parametersSpecified)
            {
                if ((BuildingInfo.parameters[NormMsgFactoryMQueue.PARAM_CLEARINGHOUSE] is MQueueparameter css) &&
                    (BuildingInfo.parameters[NormMsgFactoryMQueue.PARAM_CSSCUSTODIAN] is MQueueparameter))
                {
                    MQueueparameter cssCustodian = css.Clone();
                    cssCustodian.id = NormMsgFactoryMQueue.PARAM_CSSCUSTODIAN;
                    BuildingInfo.parameters.Add(cssCustodian);

                    if (pIsAddGPRODUCT)
                    {
                        MQueueparameter gproduct = new MQueueparameter(NormMsgFactoryMQueue.PARAM_GPRODUCT, TypeData.TypeDataEnum.@string);
                        gproduct.SetValue(Cst.ProductGProduct_FUT);
                        BuildingInfo.parameters.Add(gproduct);
                    }
                }
            }
        }



        #endregion Methods
    }
    #endregion NormMsgFactoryBase
}
