#region Using Directives
using EFS.ACommon;
using EFS.Common.MQueue;
using EFS.LoggerClient;
using EFS.LoggerClient.LoggerService;
using EFS.SpheresRiskPerformance.CashBalance;
using System;
#endregion Using Directives

namespace EFS.Process
{
    /// <summary>
    /// Construction Messages de type : RiskPerformanceMQueue
    /// 
    /// DONNEES attendues en entrée : 
    ///<para>────────────────────────────────────────────────────────────────────────────────────────────────</para>
    ///<para>  ● processType : RISKPERFORMANCE, CASHBALANCE</para>
    ///<para>────────────────────────────────────────────────────────────────────────────────────────────────</para>
    ///
    /// PARAMETRES attendus en entrée (* = optionnel):
    ///<para>────────────────────────────────────────────────────────────────────────────────────────────────</para>
    ///<para>  RISKPERFORMANCE</para>
    ///<para>  ● ENTITY        : Identifiant de l'entité</para>
    ///<para>  ● TIMING        : Intraday ou EndOfDay</para>
    ///<para>  ● CLEARINGHOUSE : Identifiant de la chambre de compensation</para>
    ///<para>  ● DTBUSINESS*   : Si NON spécifiée = Plus petite DTENTITY de la table ENTITYMARKET</para>
    ///<para>                    pour l'entité et la chambre de compensation demandée</para>
    ///<para>  ● ISRESET*      : Reset des claculs précédents</para>
    ///<para>  ● ISSIMUL*      : Mode simulation</para>
    ///<para>  ● SOFTWARE*     : Software à l'origine de la demande calcul</para>

    ///<para>  CASHBALANCE</para>
    ///<para>  ● ENTITY        : Identifiant de l'entité</para>
    ///<para>  ● TIMING        : Intraday ou EndOfDay</para>
    ///<para>  ● DTBUSINESS*   : Si NON spécifiée = Plus petite DTENTITY de la table ENTITYMARKET pour l'entité</para>
    ///<para>  ● CTRL_EOD*     :                          </para>
    ///<para>  ● CTRL_EOD_LOGLEVEL*:                      </para>
    ///<para>  ● CTRL_EOD_CSSCUSTODIANLIST*:              </para>
    ///<para>────────────────────────────────────────────────────────────────────────────────────────────────</para> 
    ///
    /// Préparation du(des) message(s) normalisé(s) de type RiskPerformanceMQueue
    /// Calcul des déposits, Cash-balances
    ///<para>────────────────────────────────────────────────────────────────────────────────────────────────</para>
    ///<para>  ● Lecture, contrôle des paramètres présents, recherche de leurs ID</para>
    ///<para>  ● Construction des messages finaux</para>
    ///<para>────────────────────────────────────────────────────────────────────────────────────────────────</para> 
    ///</summary>
    // PM 20141216 [9700] Eurex Prisma for Eurosys Futures : Ajout du paramètre optionel SOFTWARE dans le message pour RISKPERFORMANCE
    public class NormMsgFactory_RISKPERFORMANCE : NormMsgFactoryBase
    {

        #region Constructors
        public NormMsgFactory_RISKPERFORMANCE(NormMsgFactoryProcess pNormMsgFactoryProcess)
            : base(pNormMsgFactoryProcess)
        {
        }
        #endregion Constructors

        #region Methods
        #region CreateIdInfos
        protected override Cst.ErrLevel CreateIdInfos()
        {
            switch (BuildingInfo.processType)
            {
                case Cst.ProcessTypeEnum.RISKPERFORMANCE:
                case Cst.ProcessTypeEnum.CASHBALANCE:

                    m_MQueueAttributes.id = m_MQueueAttributes.parameters.GetIntValueParameterById(NormMsgFactoryMQueue.PARAM_ENTITY);
                    m_MQueueAttributes.identifier = m_MQueueAttributes.parameters.GetExtendValueParameterById(NormMsgFactoryMQueue.PARAM_ENTITY);
                    break;
            }
            return Cst.ErrLevel.SUCCESS;
        }
        #endregion CreateIdInfos
        #region CreateParameters
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        /// FI 20150801 [XXXXX] Modify
        // EG 20190114 Add detail to ProcessLog Refactoring
        protected override Cst.ErrLevel CreateParameters()
        {
            Cst.ErrLevel codeReturn = Cst.ErrLevel.SUCCESS;
            if (false == BuildingInfo.parametersSpecified)
            {
                // FI 20200623 [XXXXX] SetErrorWarning
                m_NormMsgFactoryProcess.ProcessState.SetErrorWarning(ProcessStateTools.StatusErrorEnum);
                // ERROR = NO PARAMETERS
                
                Logger.Log(new LoggerData(LogLevelEnum.Error, new SysMsgCode(SysCodeEnum.SYS, 8000), 2,
                    new LogParam(NormMsgFactoryMQueue.PARAM_ENTITY + " / " + NormMsgFactoryMQueue.PARAM_TIMING),
                    new LogParam(m_NormMsgFactoryProcess.LogId),
                    new LogParam(m_NormMsgFactoryProcess.LogProcessType)));

                codeReturn = Cst.ErrLevel.FAILURE;
            }

            if (Cst.ErrLevel.SUCCESS == codeReturn)
            {
                switch (BuildingInfo.processType)
                {
                    case Cst.ProcessTypeEnum.RISKPERFORMANCE:
                        codeReturn = AddEntityParameter();
                        if (Cst.ErrLevel.SUCCESS == codeReturn)
                            codeReturn = AddTimingParameter();
                        
                        if (Cst.ErrLevel.SUCCESS == codeReturn)
                        {
                            // FI 20150801 [XXXXX] le traitement dépôt de garantie attend le paramètre CSSCUSTODIAN
                            codeReturn = AddClearingHouseParameter(RiskPerformanceMQueue.PARAM_CSSCUSTODIAN);
                        }
                        if (Cst.ErrLevel.SUCCESS == codeReturn)
                        {
                            // FI 20150801 [XXXXX] use RiskPerformanceMQueue.PARAM_CSSCUSTODIAN
                            codeReturn = AddDtBusinessParameter(NormMsgFactoryMQueue.PARAM_DTBUSINESS, NormMsgFactoryMQueue.PARAM_DTBUSINESS,
                                RiskPerformanceMQueue.PARAM_CSSCUSTODIAN);
                        }
                        if (Cst.ErrLevel.SUCCESS == codeReturn)
                            codeReturn = AddBooleanParameter(NormMsgFactoryMQueue.PARAM_ISRESET);
                        if (Cst.ErrLevel.SUCCESS == codeReturn)
                            codeReturn = AddBooleanParameter(NormMsgFactoryMQueue.PARAM_ISSIMUL);
                        // PM 20141216 [9700] Eurex Prisma for Eurosys Futures : Ajout du paramètre optionel SOFTWARE dans le message pour RISKPERFORMANCE
                        if (Cst.ErrLevel.SUCCESS == codeReturn)
                        {
                            codeReturn = AddStringParameter(NormMsgFactoryMQueue.PARAM_SOFTWARE,true);
                        }
                        break;
                    case Cst.ProcessTypeEnum.CASHBALANCE:
                        codeReturn = AddEntityParameter();

                        if (Cst.ErrLevel.SUCCESS == codeReturn)
                            codeReturn = AddTimingParameter();

                        if (Cst.ErrLevel.SUCCESS == codeReturn)
                            codeReturn = AddDtBusinessParameter();

                        //FI 20140422 New parameter
                        if (Cst.ErrLevel.SUCCESS == codeReturn)
                            codeReturn = AddBooleanParameter(RiskPerformanceMQueue.PARAM_ISEXTERNALGENEVENTS);

                        //FI 20141126 [20526] New parameter
                        if (Cst.ErrLevel.SUCCESS == codeReturn)
                            AddCtrlEndOfDayParameter();

                        //FI 20141126 [20526] New parameter
                        if (Cst.ErrLevel.SUCCESS == codeReturn)
                            AddCtrlEndOfDayLogLevelParameter();

                        //FI 20141126 [20526] New parameter
                        if (Cst.ErrLevel.SUCCESS == codeReturn)
                            codeReturn = AddStringParameter(RiskPerformanceMQueue.PARAM_CTRL_EOD_CSSCUSTODIANLIST, true);

                        break;
                    default:
                        // ERROR = Add LOG
                        codeReturn = Cst.ErrLevel.FAILURE;
                        break;
                }
            }
            return codeReturn;
        }
        #endregion CreateParameters
        
        #region ConstructSendingMQueue
        protected override Cst.ErrLevel ConstructSendingMQueue()
        {
            SetSendingMQueue(new RiskPerformanceMQueue(BuildingInfo.processType, m_MQueueAttributes));
            return Cst.ErrLevel.SUCCESS;
        }
        #endregion ConstructSendingMQueue

        /// <summary>
        /// Ajoute le paramètre CTRL_EOD si présent 
        /// <para>Génère une erreur si le paramètre est non valide</para>
        /// </summary>
        /// <param name="pParamKeyWrite"></param>
        /// <returns></returns>
        ///FI 20141126 [20526] Add Method
        // EG 20190114 Add detail to ProcessLog Refactoring
        private Cst.ErrLevel AddCtrlEndOfDayParameter()
        {
            Cst.ErrLevel codeReturn = Cst.ErrLevel.SUCCESS;
            if ((BuildingInfo.parameters[RiskPerformanceMQueue.PARAM_CTRL_EOD_MODE] is MQueueparameter param) && StrFunc.IsFilled(param.Value))
            {
                if (Enum.IsDefined(typeof(ControlEODMode), param.Value))
                {
                    MQueueparameter parameter = new MQueueparameter(RiskPerformanceMQueue.PARAM_CTRL_EOD_MODE, TypeData.TypeDataEnum.@string);
                    parameter.SetValue(param.Value);

                    if (null == m_MQueueAttributes.parameters)
                        m_MQueueAttributes.parameters = new MQueueparameters();
                    m_MQueueAttributes.parameters.Add(parameter);
                }
                else
                {
                    string[] array = ReflectionTools.EnumToStringArray(typeof(ControlEODMode));
                    string expectedValues = StrFunc.StringArrayList.StringArrayToStringList(array, false);

                    // FI 20200623 [XXXXX] SetErrorWarning
                    m_NormMsgFactoryProcess.ProcessState.SetErrorWarning(ProcessStateTools.StatusErrorEnum);

                    // Type de traitement incorrect
                    
                    Logger.Log(new LoggerData(LogLevelEnum.Error, new SysMsgCode(SysCodeEnum.SYS, 8012), 2,
                        new LogParam(param.Value + " (" + RiskPerformanceMQueue.PARAM_CTRL_EOD_MODE + ")"),
                        new LogParam(expectedValues),
                        new LogParam(m_NormMsgFactoryProcess.LogId),
                        new LogParam(m_NormMsgFactoryProcess.LogProcessType)));

                    codeReturn = Cst.ErrLevel.FAILURE;
                }
            }
            return codeReturn;
        }

        /// <summary>
        /// Ajoute le paramètre CTRL_EOD_LOGLEVEL si présent 
        /// <para>Génère une erreur si le paramètre est non valide</para>
        /// </summary>
        /// <param name="pParamKeyWrite"></param>
        /// <returns></returns>
        ///FI 20141126 [20526] Add Method
        private Cst.ErrLevel AddCtrlEndOfDayLogLevelParameter()
        {
            Cst.ErrLevel codeReturn = Cst.ErrLevel.SUCCESS;
            if ((BuildingInfo.parameters[RiskPerformanceMQueue.PARAM_CTRL_EOD_LOGSTATUS] is MQueueparameter param) && StrFunc.IsFilled(param.Value))
            {
                if (Enum.IsDefined(typeof(ControlEODLogStatus), param.Value))
                {
                    MQueueparameter parameter = new MQueueparameter(RiskPerformanceMQueue.PARAM_CTRL_EOD_LOGSTATUS, TypeData.TypeDataEnum.@string);
                    parameter.SetValue(param.Value);

                    if (null == m_MQueueAttributes.parameters)
                        m_MQueueAttributes.parameters = new MQueueparameters();
                    m_MQueueAttributes.parameters.Add(parameter);
                }
                else
                {
                    string[] array = ReflectionTools.EnumToStringArray(typeof(ControlEODLogStatus));
                    string expectedValues = StrFunc.StringArrayList.StringArrayToStringList(array, false);


                    // Type de traitement incorrect
                    // FI 20200623 [XXXXX] SetErrorWarning
                    m_NormMsgFactoryProcess.ProcessState.SetErrorWarning(ProcessStateTools.StatusErrorEnum);

                    
                    
                    Logger.Log(new LoggerData(LogLevelEnum.Error, new SysMsgCode(SysCodeEnum.SYS, 8012), 2,
                        new LogParam(param.Value + " (" + RiskPerformanceMQueue.PARAM_CTRL_EOD_LOGSTATUS + ")"),
                        new LogParam(expectedValues),
                        new LogParam(m_NormMsgFactoryProcess.LogId),
                        new LogParam(m_NormMsgFactoryProcess.LogProcessType)));

                    codeReturn = Cst.ErrLevel.FAILURE;
                }
            }
            return codeReturn;
        }



        #endregion Methods
    }
}
