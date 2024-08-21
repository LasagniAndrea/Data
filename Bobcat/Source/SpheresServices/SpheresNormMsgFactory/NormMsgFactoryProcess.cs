#region Using Directives
using EFS.ACommon;
using EFS.ApplicationBlocks.Data;
using EFS.Common;
using EFS.Common.Log;
using EFS.Common.MQueue;
using EFS.LoggerClient;
using EFS.LoggerClient.LoggerService;
using EFS.Tuning;
using EfsML.CorporateActions;
using System;
using System.Data;
#endregion Using Directives

namespace EFS.Process
{
    /// <summary>
    /// Process principal de traitement des messages génériques en vue de leurs normalisation 
    /// </summary>
    public class NormMsgFactoryProcess : ProcessBase
    {
        #region Members
        /// <summary>
        /// Message queue ayant sollicité le process NormMsgFactory
        /// </summary>
        private readonly NormMsgFactoryMQueue m_NormMsgFactoryMQueue;
        #endregion Members

        #region Accessors-
        /// <summary>
        /// Obtient le message Queue qui active le process
        /// </summary>
        public NormMsgFactoryMQueue NormMsgFactoryMQueue
        {
            get { return m_NormMsgFactoryMQueue; }
        }
        /// <summary>
        /// Obtient l'Id de la demande pour alimentation du Log
        /// <para>Lorsque la demande est externe contient l'identifiant externe de la demande</para>
        /// <para>Lorsque la demande est interne contient l'IDTRK de la demande</para>
        /// </summary>
        public string LogId
        {
            private set;
            get;
        }
        /// <summary>
        /// Obtient le libellé du process qui sera activé pour alimentation du Log
        /// </summary>
        public string LogProcessType
        {
            private set;
            get;
        }
        #endregion Accessors
        #region Constructor
        // EG 20190214 Correction messages Tracker pour NormMsgFactory
        public NormMsgFactoryProcess(MQueueBase pMQueue, AppInstanceService pAppInstance)
            : base(pMQueue, pAppInstance)
        {
            m_NormMsgFactoryMQueue = (NormMsgFactoryMQueue)pMQueue;
            if (false == IsProcessObserver)
            {
                LogProcessType = m_NormMsgFactoryMQueue.buildingInfo.processType.ToString();
                
                if (m_NormMsgFactoryMQueue.buildingInfo.processType == Cst.ProcessTypeEnum.POSKEEPREQUEST)
                {
                    if (m_NormMsgFactoryMQueue.buildingInfo.posRequestTypeSpecified &&
                        (Cst.PosRequestTypeEnum.None != m_NormMsgFactoryMQueue.buildingInfo.posRequestType))
                        LogProcessType = m_NormMsgFactoryMQueue.buildingInfo.posRequestType.ToString();
                }
                else if (m_NormMsgFactoryMQueue.buildingInfo.processType == Cst.ProcessTypeEnum.IO)
                {
                    // FI 20230109 [XXXXX] Add Identifier
                    if (m_NormMsgFactoryMQueue.buildingInfo.identifierSpecified)
                        LogProcessType += $"/Identifier: {m_NormMsgFactoryMQueue.buildingInfo.identifier}";
                    if (m_NormMsgFactoryMQueue.buildingInfo.idSpecified)
                        LogProcessType += $"/Id: {m_NormMsgFactoryMQueue.buildingInfo.id}";
                }
            }
            // FI 20180605 [24001] 
            if (pMQueue.header.requesterSpecified && pMQueue.header.requester.idTRKSpecified)
                //Demande générale d'un traitement sans sélection préalable des éléments candidats 
                LogId = LogTools.IdentifierAndId(Cst.ProcessTypeEnum.NORMMSGFACTORY.ToString(), pMQueue.header.requester.idTRK.ToString());
            else
                LogId = (null != m_NormMsgFactoryMQueue.acknowledgment) ? m_NormMsgFactoryMQueue.acknowledgment.extlId : "-";
        }
        #endregion Constructor
        #region Methods
        #region ProcessInitialize
        /// <summary>
        /// Instancie le Log, Tracker Etc.
        /// </summary>
        protected override void ProcessInitialize()
        {
            base.ProcessInitialize();

            if (false == IsProcessObserver)
            {
                if (!ProcessTuningSpecified)
                    ProcessTuning = new ProcessTuning(Cs, 0, MQueue.ProcessType, AppInstance.ServiceName, AppInstance.HostName);
                
                if (ProcessTuningSpecified)
                {
                    LogDetailEnum = ProcessTuning.LogDetailEnum;

                    
                    Logger.CurrentScope.SetLogLevel(LoggerConversionTools.DetailEnumToLogLevelEnum(LogDetailEnum));
                }
            }
        }
        #endregion ProcessInitialize

        
        #region ProcessExecuteSpecific
        /// <summary>
        /// Traitement de la demande de construction d'un message applicatif Spheres
        /// </summary>
        /// <returns></returns>
        /// FI 20150618 [20945] Modify
        /// FI 20170314 [22225] Modify
        /// EG 20180525 [23979] IRQ Processing
        // EG 20190114 Add detail to ProcessLog Refactoring
        // EG 20230901 [WI702] ClosingReopeningPosition - Delisting action - NormMsgFactory
        protected override Cst.ErrLevel ProcessExecuteSpecific()
        {
            Cst.ErrLevel codeReturn = Cst.ErrLevel.SUCCESS;
            // Log
            if ((m_NormMsgFactoryMQueue.buildingInfo.processType != Cst.ProcessTypeEnum.CORPOACTIONINTEGRATE) &&
                (m_NormMsgFactoryMQueue.buildingInfo.processType != Cst.ProcessTypeEnum.CLOSINGREOPENINGINTEGRATE) &&
               (m_NormMsgFactoryMQueue.buildingInfo.processType != Cst.ProcessTypeEnum.IRQ))
            {
                Logger.Log(new LoggerData(LogLevelEnum.None, new SysMsgCode(SysCodeEnum.LOG, 8000), 0,
                    new LogParam(LogId),
                    new LogParam(LogProcessType)));
                
                // FI 20230109 [XXXXX] Add Parameters
                if (m_NormMsgFactoryMQueue.buildingInfo.parametersSpecified)
                {
                    for (int i = 0; i < m_NormMsgFactoryMQueue.buildingInfo.parameters.Count; i++)
                    {
                        MQueueparameter mQueueparameter = m_NormMsgFactoryMQueue.buildingInfo.parameters[i];
                        Logger.Log(new LoggerData(LogLevelEnum.None, new SysMsgCode(SysCodeEnum.LOG, 6041), 2, new LogParam(mQueueparameter.id), new LogParam(mQueueparameter.Value)));
                    }
                }
            }

            NormMsgFactoryBase normMsgFactory = null;
            switch (m_NormMsgFactoryMQueue.buildingInfo.processType)
            {
                case Cst.ProcessTypeEnum.POSKEEPREQUEST:
                    normMsgFactory = new NormMsgFactory_POSKEEPREQUEST(this);
                    break;
                case Cst.ProcessTypeEnum.IO:
                    normMsgFactory = new NormMsgFactory_IO(this);
                    break;
                case Cst.ProcessTypeEnum.RISKPERFORMANCE:
                case Cst.ProcessTypeEnum.CASHBALANCE:
                    normMsgFactory = new NormMsgFactory_RISKPERFORMANCE(this);
                    break;
                case Cst.ProcessTypeEnum.ACCOUNTGEN:
                    normMsgFactory = new NormMsgFactory_ACCOUNTING(this);
                    break;
                case Cst.ProcessTypeEnum.EARGEN:
                    normMsgFactory = new NormMsgFactory_EAR(this);
                    break;
                case Cst.ProcessTypeEnum.RIMGEN:
                    normMsgFactory = new NormMsgFactory_REPORTING(this);
                    break;
                case Cst.ProcessTypeEnum.FEESCALCULATION:
                    // FI 20170314 [22225] Modify
                    //normMsgFactory = new NormMsgFactory_ACTIONGEN(this);
                    normMsgFactory = new NormMsgFactory_ACTIONGEN(this);
                    break;
                case Cst.ProcessTypeEnum.INVOICINGGEN:
                    normMsgFactory = new NormMsgFactory_INVOICINGGEN(this);
                    break;
                case Cst.ProcessTypeEnum.CORPOACTIONINTEGRATE:
                    normMsgFactory = new NormMsgFactory_CORPOACTIONINTEGRATE(this);
                    break;
                case Cst.ProcessTypeEnum.CLOSINGREOPENINGINTEGRATE:
                    normMsgFactory = new NormMsgFactory_CLOSINGREOPENINGINTEGRATE(this);
                    break;
                case Cst.ProcessTypeEnum.IRQ:
                    normMsgFactory = new NormMsgFactory_IRQ(this);
                    break;
            }
            if (null != normMsgFactory)
            {
                codeReturn = normMsgFactory.Generate();
                switch (codeReturn)
                {
                    case Cst.ErrLevel.SUCCESS:
                        codeReturn = normMsgFactory.SendFinalMessages();
                        break;
                    case Cst.ErrLevel.NOTHINGTODO:
                        Logger.Log(new LoggerData(LogLevelEnum.None, new SysMsgCode(SysCodeEnum.LOG, 8004), 1,
                            new LogParam(LogId),
                            new LogParam(LogProcessType)));
                        break;
                }

            }
            return codeReturn;
        }
        #endregion ProcessExecuteSpecific

        /// <summary>
        /// Initialize les données manquantes lorsqu'elles sont nécessaires au bon déroulement du process
        /// </summary>
        /// FI 20130503 [] add Method
        protected override void ProcessInitializeMqueue()
        {
            if (false == IsProcessObserver)
            {
                switch (m_NormMsgFactoryMQueue.buildingInfo.processType)
                {
                    case Cst.ProcessTypeEnum.CORPOACTIONINTEGRATE:
                        if (m_NormMsgFactoryMQueue.buildingInfo.id == 0)
                        {
                            int id = GetIdCorpoActionIssue(Cs, m_NormMsgFactoryMQueue.buildingInfo.parameters);
                            m_NormMsgFactoryMQueue.id = id;
                            m_NormMsgFactoryMQueue.buildingInfo.id = id;
                        }
                        break;
                    default:
                        break;
                }
            }
        }

        /// <summary>
        /// Retourne l'id de la publication d'un CA à partir de CAMARKET, REFNOTICE et CFICODE
        /// <para>Retourne 0 si aucune publication</para>
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pMqueueParameters"></param>
        /// <returns></returns>
        /// <exception cref="lorsque CAMARKET ou REFNOTICE ne sont pas présents dans le message"></exception>
        /// <exception cref="lorsque Spheres® ne détecte pas l'id"></exception>
        /// FI 20130503 [] add Method
        private static int GetIdCorpoActionIssue(string pCS, MQueueparameters pBuildingInfoParameters)
        {
            int ret = 0;

            CAQueryISSUE caQry = new CAQueryISSUE(pCS);
            if (null == pBuildingInfoParameters["CAMARKET"])
                throw new Exception("CAMARKET is mandatory");
            if (null == pBuildingInfoParameters["REFNOTICE"])
                throw new Exception("REFNOTICE is mandatory");


            string caMarket = pBuildingInfoParameters["CAMARKET"].Value;
            string refNotice = pBuildingInfoParameters["REFNOTICE"].Value;

            QueryParameters query = caQry.GetQueryExist(CATools.CAWhereMode.NOTICE);
            query.Parameters["CAMARKET"].Value = caMarket;
            query.Parameters["REFNOTICE"].Value = refNotice;

            // EG 20140103 Test cfiCodeSpecified
            string cfiCode = string.Empty;
            bool cfiCodeSpecified = (null != pBuildingInfoParameters["CFICODE"]);
            if (cfiCodeSpecified)
                cfiCode = pBuildingInfoParameters["CFICODE"].Value;
            query.Parameters["CFICODE"].Value = cfiCodeSpecified ? cfiCode : Convert.DBNull;

            object obj = DataHelper.ExecuteScalar(pCS, CommandType.Text, query.Query, query.Parameters.GetArrayDbParameter());
            if (null != obj)
                ret = Convert.ToInt32(obj);

            if (ret == 0)
                throw new Exception(StrFunc.AppendFormat(
                                                "Unable to find CA (id) using [CAMARKET: {0}, REFNOTICE :{1}, CFICODE {2})", caMarket, refNotice, cfiCode));

            return ret;
        }


        #endregion Methods
    }
}
