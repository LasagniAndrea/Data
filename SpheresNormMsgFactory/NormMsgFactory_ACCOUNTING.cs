#region Using Directives
//
using EFS.ACommon;
using EFS.ApplicationBlocks.Data;
using EFS.Common.MQueue;
using EFS.LoggerClient;
using EFS.LoggerClient.LoggerService;
//
using EfsML.Business;
using System;
using System.Data;
#endregion Using Directives

namespace EFS.Process
{
    #region NormMsgFactory_ACCOUNTING
    /// <summary>
    /// Construction Messages de type : AccountGenMQueue
    /// 
    /// DONNEES attendues en entrée : 
    ///<para>────────────────────────────────────────────────────────────────────────────────────────────────</para>
    ///<para>  ● processType = ACCOUNTGEN</para>
    ///<para>────────────────────────────────────────────────────────────────────────────────────────────────</para>
    ///
    /// PARAMETRES attendus en entrée (* = optionnel):
    ///<para>────────────────────────────────────────────────────────────────────────────────────────────────</para>
    ///<para>  ● ENTITY     : Identifiant de l'entité</para>
    ///<para>  ● DTBUSINESS : Date de traitement</para>
    ///<para>────────────────────────────────────────────────────────────────────────────────────────────────</para> 
    ///</summary>
    public class NormMsgFactory_ACCOUNTING : NormMsgFactoryBase
    {
        #region Constructors
        /// <summary>
        /// Constructeur
        /// </summary>
        /// <param name="pNormMsgFactoryProcess"></param>
        public NormMsgFactory_ACCOUNTING(NormMsgFactoryProcess pNormMsgFactoryProcess)
            : base(pNormMsgFactoryProcess)
        {
        }
        #endregion Constructors

        #region Methods
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pDr"></param>
        /// <returns></returns>
        protected override MQueueBase BuildMessageQueue(DataRow pDr)
        {
            MQueueBase _queue = new AccountGenMQueue(m_MQueueAttributes);
            SetDefaultMessageQueue(_queue, pDr);
            return _queue;
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        // EG 20190114 Add detail to ProcessLog Refactoring
        protected override Cst.ErrLevel CreateParameters()
        {
            Cst.ErrLevel codeReturn = Cst.ErrLevel.SUCCESS;

            if (false == BuildingInfo.parametersSpecified)
            {
                // ERROR = NO PARAMETERS
                // FI 20200623 [XXXXX] SetErrorWarning
                m_NormMsgFactoryProcess.ProcessState.SetErrorWarning(ProcessStateTools.StatusErrorEnum);

                
                Logger.Log(new LoggerData(LogLevelEnum.Error, new SysMsgCode(SysCodeEnum.SYS, 8000), 2,
                    new LogParam(NormMsgFactoryMQueue.PARAM_ENTITY + " / " + NormMsgFactoryMQueue.PARAM_DATE1),
                    new LogParam(m_NormMsgFactoryProcess.LogId),
                    new LogParam(m_NormMsgFactoryProcess.LogProcessType)));

                codeReturn = Cst.ErrLevel.FAILURE;
            }

            if (Cst.ErrLevel.SUCCESS == codeReturn)
            {
                codeReturn = AddEntityParameter();
                if (Cst.ErrLevel.SUCCESS == codeReturn)
                {
                    // FI 20180605 [24001] PARAM_DATE1 est désormais prioritaire 
                    //Si non Reseigné, Spheres® récupère la date courante la plus récente vis à vis de l'entité 
                    //codeReturn = AddDtBusinessParameter(NormMsgFactoryMQueue.PARAM_DTBUSINESS, NormMsgFactoryMQueue.PARAM_DATE1, null);
                    codeReturn = AddDateParameter(NormMsgFactoryMQueue.PARAM_DATE1, string.Empty);
                    if (codeReturn == Cst.ErrLevel.SUCCESS)
                    {
                        // Si le parampètre PARAM_DATE1 n'existe pas en entrée alors lecture de l'éventuel paramètre DTBUSINESS (Compatibilité ascendante)
                        // Si le parampètre PARAM_DTBUSINESS n'existe pas alors création du paramètre PARAM_DATE1 avec valeur par défaut "BUSINESS"  
                        if (StrFunc.IsEmpty(m_MQueueAttributes.parameters[NormMsgFactoryMQueue.PARAM_DATE1].Value)
                            || (DtFunc.ParseDate(m_MQueueAttributes.parameters[NormMsgFactoryMQueue.PARAM_DATE1].Value, DtFunc.FmtISODate, null) == DateTime.MinValue))
                        {
                            m_MQueueAttributes.parameters.Remove(NormMsgFactoryMQueue.PARAM_DATE1);
                            codeReturn = AddDateParameter(NormMsgFactoryMQueue.PARAM_DTBUSINESS, NormMsgFactoryMQueue.PARAM_DATE1, DtFuncML.BUSINESS);
                        }
                    }
                }

                if (Cst.ErrLevel.SUCCESS == codeReturn)
                    codeReturn = AddAccountingRequestTypeParameter();
            }
            return codeReturn;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pDt"></param>
        /// <returns></returns>
        protected override DataRow[] RowsCandidates(DataTable pDt)
        {
            return pDt.Select(null, @"IDT", DataViewRowState.OriginalRows);
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        // EG 20191115 [25077] RDBMS : New version of Trades tables architecture (TRADESTSYS merge to TRADE, NEW TABLE TRADEXML)
        protected override QueryParameters GetQueryParameters()
        {
            DataParameters dp = new DataParameters(); 
            dp.Add(new DataParameter(m_NormMsgFactoryProcess.Cs, NormMsgFactoryMQueue.PARAM_ENTITY, DbType.Int32),
                    m_MQueueAttributes.parameters.GetIntValueParameterById(NormMsgFactoryMQueue.PARAM_ENTITY));

            dp.Add(new DataParameter(m_NormMsgFactoryProcess.Cs, NormMsgFactoryMQueue.PARAM_DATE1, DbType.Date),
                m_MQueueAttributes.parameters.GetDateTimeValueParameterById(NormMsgFactoryMQueue.PARAM_DATE1));


            #region Query
            string requestType = m_MQueueAttributes.parameters.GetStringValueParameterById(NormMsgFactoryMQueue.PARAM_REQUESTTYPE);
            StrBuilder sqlQuery = new StrBuilder();
            sqlQuery += @"select tblresult.IDT, tblresult.IDENTIFIER, tblresult.GPRODUCT
        from
        (        
          /* ----------------------------------------------------------------------------------------------------------- */
          /* Tous les EARs du jour non annules                                                                           */
          /* Tous les EARs passes non annules, non deja ignores par le parametrage et qui n''ont pas genere d''ecritures */
          /* ----------------------------------------------------------------------------------------------------------- */
          select tblmain.IDT, tblmain.IDENTIFIER, p.GPRODUCT
          from dbo.TRADE tblmain
          inner join dbo.INSTRUMENT i on (i.IDI=tblmain.IDI) and (i.ISACCOUNTING=1)
          inner join dbo.PRODUCT p on (p.IDP=i.IDP) and";

            switch (requestType)
            {
                case "ADMIN":
                    sqlQuery += @"(p.GPRODUCT ='ADM')";
                    break;
                case "CBI":
                    sqlQuery += @"(p.IDENTIFIER='cashBalanceInterest')";
                    break;
                case "COMMON":
                    sqlQuery += @"(p.GPRODUCT not in ('ADM','ASSET','RISK') or p.IDENTIFIER in ('cashBalance','cashPayment'))";
                    break;
            }

            sqlQuery += @"/* Ne pas considérer les événements des trades désactivés */
          inner join dbo.EAR ear on (ear.IDT=tblmain.IDT)
          left outer join dbo.EAR_ACCMODEL eam on (eam.IDEAR=ear.IDEAR)
          where 
          (
            (tblmain.IDSTENVIRONMENT='REGULAR') and (tblmain.IDSTACTIVATION='REGULAR') and

            /*EARs du jour non annulés*/
            ((ear.DTEVENT=@DATE1) and (ear.DTEVENTCANCEL is null))
            or
            (
              /*EARs passes non annulés*/
              (ear.DTEVENT<@DATE1)
              and (ear.DTEVENTCANCEL is null)
              /*non deja ignores par le parametrage*/
              and (eam.ISIGNORED is null or eam.ISIGNORED=0)
              /*pas d'écritures générées*/
              and not exists 
              (
                select 1
                from dbo.ACCDAYBOOK
                where (ACCDAYBOOK.IDEAR=ear.IDEAR)
              )
            )
          )
          
          union

          /* ----------------------------------------------------------------- */
          /* Tous les EARs annules du jour et passes qui n'ont pas ete traites */
          /* ----------------------------------------------------------------- */
          select tblmain.IDT, tblmain.IDENTIFIER, p.GPRODUCT
          from dbo.TRADE tblmain
          inner join dbo.INSTRUMENT i on (i.IDI=tblmain.IDI) and (i.ISACCOUNTING=1)
          inner join dbo.PRODUCT p on (p.IDP=i.IDP) and ";

            switch (requestType)
            {
                case "ADMIN":
                    sqlQuery += @"(p.GPRODUCT ='ADM')";
                    break;
                case "CBI":
                    sqlQuery += @"(p.IDENTIFIER='cashBalanceInterest')";
                    break;
                case "COMMON":
                    sqlQuery += @"(p.GPRODUCT not in ('ADM','ASSET','RISK') or p.IDENTIFIER in ('cashBalance','cashPayment'))";
                    break;
            }

            sqlQuery += @"inner join dbo.EAR ear on (ear.IDT=tblmain.IDT)
          where 
          (tblmain.IDSTENVIRONMENT='REGULAR') and 
          /*EARs annules du jour et passes*/
          (ear.DTEVENTCANCEL<=@DATE1)
          /*EARs annules NON traités*/
          and exists 
          (
            select 1
            from dbo.ACCDAYBOOK
            where (ACCDAYBOOK.IDEAR=ear.IDEAR)
            and (ACCDAYBOOK.DTACCDAYBOOKCANCEL is null)
          )
          
        ) tblresult        
        inner join dbo.TRADEACTOR tabuyer on (tabuyer.IDT=tblresult.IDT) and (tabuyer.IDROLEACTOR='COUNTERPARTY')  and (tabuyer.BUYER_SELLER='Buyer')
        inner join dbo.TRADEACTOR taseller on (taseller.IDT=tblresult.IDT) and (taseller.IDROLEACTOR='COUNTERPARTY') and (taseller.BUYER_SELLER='Seller')
        left outer join dbo.BOOK bbuyer on (bbuyer.IDB=tabuyer.IDB)
        left outer join dbo.BOOK bseller on (bseller.IDB=taseller.IDB)
        where (-1=@ENTITY or bbuyer.IDA_ENTITY=@ENTITY or bseller.IDA_ENTITY=@ENTITY)";
            #endregion SetQuery

            QueryParameters ret = new QueryParameters(m_NormMsgFactoryProcess.Cs, sqlQuery.ToString(), dp);
            return ret;
        }
        
        #endregion Methods
    }
    #endregion NormMsgFactory_ACCOUNTING
}