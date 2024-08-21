#region Using Directives
using EFS.ACommon;
using EFS.Actor;
using EFS.Common;
using EFS.Common.MQueue;
using EFS.LoggerClient;
using EFS.LoggerClient.LoggerService;
#endregion Using Directives

namespace EFS.Process
{

    #region NormMsgFactory_INVOICINGGEN
    /// <summary>
    /// Construction Messages de type : InvoicingGenMQueue
    /// 
    /// DONNEES attendues en entrée : 
    ///<para>────────────────────────────────────────────────────────────────────────────────────────────────</para>
    ///<para>  ● processType : INVOICINGGEN</para>
    ///<para>────────────────────────────────────────────────────────────────────────────────────────────────</para>
    ///
    /// PARAMETRES attendus en entrée (* = optionnel):
    ///<para>────────────────────────────────────────────────────────────────────────────────────────────────</para>
    ///<para>  ● ENTITY        : Identifiant de l'entité</para>
    ///<para>  ● DTINVOICING   : Date de facturation</para>
    ///<para>  ● ISSIMUL*      : Mode simulation</para>
    ///<para>  ● PAYER*        : Payeur</para>
    ///<para>  ● CURRENCY*     : Devise de frais</para>
    ///
    ///<para>────────────────────────────────────────────────────────────────────────────────────────────────</para> 
    ///
    /// Préparation du(des) message(s) normalisé(s) de type InvoicingGenMQueue
    ///<para>────────────────────────────────────────────────────────────────────────────────────────────────</para>
    ///<para>  ● Lecture, contrôle des paramètres présents, recherche de leurs ID</para>
    ///<para>  ● Construction des messages finaux</para>
    ///<para>────────────────────────────────────────────────────────────────────────────────────────────────</para> 
    /// </summary>
    public class NormMsgFactory_INVOICINGGEN : NormMsgFactoryBase
    {
        #region Constructors
        /// <summary>
        /// Constructeur
        /// </summary>
        /// <param name="pNormMsgFactoryProcess">Process</param>
        public NormMsgFactory_INVOICINGGEN(NormMsgFactoryProcess pNormMsgFactoryProcess)
            : base(pNormMsgFactoryProcess)
        {
        }
        #endregion Constructors
        #region Methods
        #region CreateIdInfos
        protected override Cst.ErrLevel CreateIdInfos()
        {
            return Cst.ErrLevel.SUCCESS;
        }
        #endregion CreateIdInfos
        #region CreateParameters
        /// <summary>
        /// Création et contrôle des paramètres
        /// </summary>
        /// <returns></returns>
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
                    new LogParam(NormMsgFactoryMQueue.PARAM_ENTITY + " / " + NormMsgFactoryMQueue.PARAM_DTBUSINESS),
                    new LogParam(m_NormMsgFactoryProcess.LogId),
                    new LogParam(m_NormMsgFactoryProcess.LogProcessType)));

                codeReturn = Cst.ErrLevel.FAILURE;
            }

            if (Cst.ErrLevel.SUCCESS == codeReturn)
            {
                switch (BuildingInfo.processType)
                {
                    case Cst.ProcessTypeEnum.INVOICINGGEN:
                        codeReturn = AddEntityParameter();
                        if (Cst.ErrLevel.SUCCESS == codeReturn)
                            codeReturn = AddDtInvoicingParameter();
                        if (Cst.ErrLevel.SUCCESS == codeReturn)
                            codeReturn = AddBooleanParameter(NormMsgFactoryMQueue.PARAM_ISSIMUL);
                        if (Cst.ErrLevel.SUCCESS == codeReturn)
                            codeReturn = AddActorParameter(NormMsgFactoryMQueue.PARAM_PAYER, RoleActor.INVOICINGOFFICE);
                        if (Cst.ErrLevel.SUCCESS == codeReturn)
                            codeReturn = AddCurrencyParameter(NormMsgFactoryMQueue.PARAM_CURRENCY);
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
            SetSendingMQueue(new InvoicingGenMQueue(m_MQueueAttributes));
            return Cst.ErrLevel.SUCCESS;
        }
        #endregion ConstructSendingMQueue

        #region SetCriteria
        protected override Cst.ErrLevel SetCriteria()
        {
            string currency = m_MQueueAttributes.parameters.GetStringValueParameterById(NormMsgFactoryMQueue.PARAM_CURRENCY);
            string payer = m_MQueueAttributes.parameters.GetExtendValueParameterById(NormMsgFactoryMQueue.PARAM_PAYER);
            bool isCriteria = StrFunc.IsFilled(currency) || StrFunc.IsFilled(payer);
            if (isCriteria)
            {
                m_MQueueAttributes.criteria = new SQL_Criteria();
                string @operator;
                if (StrFunc.IsFilled(currency))
                {
                    @operator = (currency.Contains("%") ? "Contains" : "=");

                    SQL_ColumnCriteriaDataType datatype = new SQL_ColumnCriteriaDataType(TypeData.TypeDataEnum.@string);
                    SQL_ColumnCriteriaInput input = new SQL_ColumnCriteriaInput(currency.Replace("%", string.Empty));
                    m_MQueueAttributes.criteria.Add(new SQL_ColumnCriteria(datatype, "e.UNIT", "", @operator, input));
                    m_MQueueAttributes.parameters.Remove(NormMsgFactoryMQueue.PARAM_CURRENCY);
                }
                if (StrFunc.IsFilled(payer))
                {
                    @operator = (payer.Contains("%") ? "Contains" : "=");
                    SQL_ColumnCriteriaDataType datatype = new SQL_ColumnCriteriaDataType(TypeData.TypeDataEnum.@string);
                    SQL_ColumnCriteriaInput input = new SQL_ColumnCriteriaInput(payer.Replace("%", string.Empty));

                    m_MQueueAttributes.criteria.Add(new SQL_ColumnCriteria(datatype, "a_payer.IDENTIFIER", "", @operator, input));
                    m_MQueueAttributes.parameters.Remove(NormMsgFactoryMQueue.PARAM_PAYER);
                }
                m_MQueueAttributes.criteria.ColumnCriteriaSpecified = ArrFunc.IsFilled(m_MQueueAttributes.criteria.ColumnCriteria);
                m_MQueueAttributes.criteria.IsCaseInsensitive = true;
            }
            return Cst.ErrLevel.SUCCESS;
        }
        #endregion SetCriteria

        #endregion Methods
    }
    #endregion NormMsgFactory_INVOICINGGEN
}