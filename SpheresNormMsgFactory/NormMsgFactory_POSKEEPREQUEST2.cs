#region Using Directives
using System;
using System.Data;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;

using EFS.ACommon;
using EFS.Actor;
using EFS.Common;
using EFS.Common.Log;
using EFS.Common.MQueue;
using EFS.ApplicationBlocks.Data;
using EFS.EFSTools;

using EFS.SpheresService;
using EFS.Tuning;

using EfsML;
using EfsML.Business;
using EfsML.Enum;
using EfsML.Enum.Tools;

#endregion Using Directives

namespace EFS.Process
{

    /// <summary>
    /// Construction Messages de type : PosKeepingRequestMQueue
    /// 
    /// DONNEES attendues en entrée : 
    ///<para>────────────────────────────────────────────────────────────────────────────────────────────────</para>
    ///<para>  ● processType    : POSKEEPREQUEST</para>
    ///<para>  ● posRequestType : Valeur de PosRequestTypeEnum (EOD, CLOSINGDAY...)</para>
    ///<para>────────────────────────────────────────────────────────────────────────────────────────────────</para>
    ///
    /// PARAMETRES attendus en entrée : 
    ///<para>────────────────────────────────────────────────────────────────────────────────────────────────</para>
    ///<para>  ● ENTITY         : Identifiant de l'entité</para>
    ///<para>  ● CSSCUSTODIAN   : Identifiant de la chambre de compensation ou du Custodian                  </para>
    ///<para>  ● GPRODUCT       : FUT                                                                        </para>
    ///<para>  ● CLEARINGHOUSE  : Identifiant de la chambre de compensation (Conserver pour raisson de compatibilté ascendante)</para>
    ///<para>────────────────────────────────────────────────────────────────────────────────────────────────</para> 
    ///</summary>
    /// FI 20150618 [20945] Add Classe (reecriture de NormMsgFactory_POSKEEPREQUEST)
    /// <para>Le comportement a totalement changé. désormais Spheres® execute la même requête que l'application web</para>
    public class NormMsgFactory_POSKEEPREQUEST2 : NormMsgFactoryBase
    {
        #region Constructors
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pNormMsgFactoryProcess"></param>
        public NormMsgFactory_POSKEEPREQUEST2(NormMsgFactoryProcess pNormMsgFactoryProcess)
            : base(pNormMsgFactoryProcess)
        {
            if (BuildingInfo.posRequestTypeSpecified)
                m_TaskInfo.tracker.caller = BuildingInfo.posRequestType.ToString();
        }
        #endregion Constructors

        #region method
        /// <summary>
        /// Alimentation de m_MQueueAttributes.parameters
        /// </summary>
        /// <returns></returns>
        protected override Cst.ErrLevel CreateParameters()
        {

            Cst.ErrLevel codeReturn = Cst.ErrLevel.SUCCESS;

            if (BuildingInfo.posRequestTypeSpecified)
            {
                switch (BuildingInfo.posRequestType)
                {
                    case Cst.PosRequestTypeEnum.ClosingDay:
                    case Cst.PosRequestTypeEnum.EndOfDay:
                        if (false == BuildingInfo.parametersSpecified)
                        {
                            // ERROR = Add LOG
                            m_NormMsgFactoryProcess.ProcessLogAddDetail(ProcessStateTools.StatusErrorEnum, ErrorManager.DetailEnum.NONE, 2, "SYS-08000",
                                false, null, new string[] { 
                            NormMsgFactoryMQueue.PARAM_ENTITY + " / " +
                            NormMsgFactoryMQueue.PARAM_CSSCUSTODIAN + " / " +
                            NormMsgFactoryMQueue.PARAM_GPRODUCT + " / " +
                            NormMsgFactoryMQueue.PARAM_DTBUSINESS,
                                m_NormMsgFactoryProcess.LogExtlId, m_NormMsgFactoryProcess.LogProcessType });
                            codeReturn = Cst.ErrLevel.FAILURE;
                        }

                        if (Cst.ErrLevel.SUCCESS == codeReturn)
                            codeReturn = AddEntityParameter();

                        if (Cst.ErrLevel.SUCCESS == codeReturn)
                        {
                            AddBuildingInfoParameterCssCustodianParamerfromClearingHouse(true);
                            codeReturn = AddCssCustodianParameter();
                        }

                        if (Cst.ErrLevel.SUCCESS == codeReturn)
                        {
                            MQueueparameter DtBusinness = BuildingInfo.parameters[NormMsgFactoryMQueue.PARAM_DTBUSINESS] as MQueueparameter;
                            if (null != DtBusinness)
                                AddMQueueAttributesParameter(DtBusinness);

                            MQueueparameter gProduct = BuildingInfo.parameters[NormMsgFactoryMQueue.PARAM_GPRODUCT] as MQueueparameter;
                            if (null != gProduct)
                                AddMQueueAttributesParameter(gProduct);
                        }

                        break;
                    default:
                        // ERROR = Add LOG
                        codeReturn = Cst.ErrLevel.FAILURE;
                        break;
                }
            }
            else
            {
                // ERROR = Add LOG
                m_NormMsgFactoryProcess.ProcessLogAddDetail(ProcessStateTools.StatusErrorEnum, ErrorManager.DetailEnum.NONE, 2, "SYS-08020",
                    false, null, new string[] { "posRequestType (EOD, CLOSINGDAY, ...)",
                            LogTools.IdentifierAndId(BuildingInfo.identifier, BuildingInfo.id), 
                                m_NormMsgFactoryProcess.LogExtlId, m_NormMsgFactoryProcess.LogProcessType });

                codeReturn = Cst.ErrLevel.FAILURE;
            }
            return codeReturn;
        }

        /// <summary>
        /// Retourne La requête SQL à l'origine des Mqueues générés
        /// </summary>
        /// <returns></returns>
        protected override QueryParameters GetQueryParameters()
        {
            
            
            string cs = m_NormMsgFactoryProcess.cs;
            MQueueparameters parameters = m_MQueueAttributes.parameters;

            DataParameters dp = new DataParameters();

            // ENTITY
            dp.Add(new DataParameter(cs, NormMsgFactoryMQueue.PARAM_ENTITY, DbType.Int32),
                parameters.GetIntValueParameterById(PosKeepingMQueue.PARAM_ENTITY));

            // CSSCUSTODIAN
            dp.Add(new DataParameter(cs, NormMsgFactoryMQueue.PARAM_CSSCUSTODIAN, DbType.Int32),
                    parameters.GetIntValueParameterById(NormMsgFactoryMQueue.PARAM_CSSCUSTODIAN));

            // GPRODUCT
            if (null != parameters[NormMsgFactoryMQueue.PARAM_GPRODUCT])
            {
                dp.Add(new DataParameter(cs, NormMsgFactoryMQueue.PARAM_GPRODUCT, DbType.String),
                       parameters.GetStringValueParameterById(NormMsgFactoryMQueue.PARAM_GPRODUCT));
            }

            // DTBUSINESS
            if (null != parameters[NormMsgFactoryMQueue.PARAM_DTBUSINESS])
            {
                dp.Add(new DataParameter(cs, NormMsgFactoryMQueue.PARAM_DTBUSINESS, DbType.Date),
                       parameters.GetDateTimeValueParameterById(NormMsgFactoryMQueue.PARAM_DTBUSINESS));
            }

            string query = string.Empty;
            switch (BuildingInfo.posRequestType)
            {
                case Cst.PosRequestTypeEnum.EndOfDay:
                    query = GetQueryEndOfDay();
                    break;
                case Cst.PosRequestTypeEnum.ClosingDay:
                    query = GetQueryClosingDay();
                    break;
                default:
                    throw new NotImplementedException(StrFunc.AppendFormat("posRequestType:{0} is not implemented"));
            }

            SQLWhere sqlwhere = new SQLWhere();
            if (dp.Contains("GPRODUCT"))
                sqlwhere.Append("result.GPRODUCT=@GPRODUCT");

            if (dp.Contains("DTBUSINESS"))
                sqlwhere.Append("result.DTENTITY=@DTBUSINESS");

            if (sqlwhere.Length() > 0)
                query += sqlwhere;

            QueryParameters ret = new QueryParameters(m_NormMsgFactoryProcess.cs, query, dp);

            return ret;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected override DataRow[] RowsCandidates(DataTable pDt)
        {
            return pDt.Select(null, "IDA_ENTITY, ENTITY_IDENTIFIER, IDA_CSSCUSTODIAN, CSSCUSTODIAN_IDENTIFIER, DTENTITY, GPRODUCT", DataViewRowState.OriginalRows);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pDr"></param>
        /// <returns></returns>
        protected override MQueueBase BuildMessageQueue(DataRow pDr)
        {
            PosKeepingRequestMQueue mqueue = new PosKeepingRequestMQueue(BuildingInfo.posRequestType, m_MQueueAttributes);

            DateTime DtBusiness = Convert.ToDateTime(pDr["DTENTITY"]);
            Pair<int, string> entity = new Pair<int, string>(Convert.ToInt32(pDr["IDA_ENTITY"]), Convert.ToString(pDr["ENTITY_IDENTIFIER"]));
            Pair<int, string> cssCustodian = new Pair<int, string>(Convert.ToInt32(pDr["IDA_CSSCUSTODIAN"]), Convert.ToString(pDr["CSSCUSTODIAN_IDENTIFIER"]));

            string gProduct = Convert.ToString(pDr["GPRODUCT"]);

            mqueue.idInfoSpecified = true;
            mqueue.idInfo = new MQueueIdInfo();
            mqueue.idInfo.idInfos =
                    new DictionaryEntry[] {
                                        new DictionaryEntry("GPRODUCT",  gProduct),
                                        new DictionaryEntry("REQUESTTYPE", BuildingInfo.posRequestType),
                                        new DictionaryEntry("DTBUSINESS", DtFunc.DateTimeToStringDateISO(DtBusiness)),
                                        new DictionaryEntry("ENTITY",entity.Second),
                                        new DictionaryEntry("CSSCUSTODIAN", cssCustodian.Second)};

            mqueue.parameters = m_MQueueAttributes.parameters.Clone();

            if (null == mqueue.parameters["DTBUSINESS"])
                mqueue.parameters.Add(new MQueueparameter("DTBUSINESS", TypeData.TypeDataEnum.date));
            mqueue.parameters["DTBUSINESS"].SetValue(DtBusiness);

            mqueue.parameters["ENTITY"].SetValue(entity.First, entity.Second);
            mqueue.parameters["CSSCUSTODIAN"].SetValue(cssCustodian.First, cssCustodian.Second);

            return mqueue;
        }

        /// <summary>
        /// Retourne la requête associée au traiyement EOD
        /// </summary>
        /// <returns></returns>
        private static string GetQueryEndOfDay()
        {
            // FI 20150618 [20945] GLOP => Prévoir le partage de la requête dans un fichier
            //Requête issue
            string sqlQuery = @"select distinct result.IDEM, 
        result.IDA_ENTITY, a_entity.IDENTIFIER as ENTITY_IDENTIFIER, a_entity.DISPLAYNAME as ENTITY_DISPLAYNAME, 
        result.IDA_CSSCUSTODIAN, a_csscust.IDENTIFIER as CSSCUSTODIAN_IDENTIFIER,  a_csscust.DISPLAYNAME as CSSCUSTODIAN_DISPLAYNAME, 
        result.DTENTITY, result.EMKEY,
        result.EOD_CURRENTSTATUS,
        result.EOD_TOTAL  as EOD_TOTALCOUNT,
        case when result.EOD_Success > 0 then 'SUCCESS'   else 'None' end as EOD_SUCCESSSTATUS, nullif(result.EOD_Success, 0) as EOD_SUCCESSCOUNT,
        case when result.EOD_Error   > 0 then 'ERROR'     else 'None' end as EOD_ERRORSTATUS,   nullif(result.EOD_Error, 0)   as EOD_ERRORCOUNT,
        result.GPRODUCT, result.SORTED
        from 
        (
	        select em.EMKEY, em.IDA_ENTITY, em.IDA_CSSCUSTODIAN, em.DTENTITY,
	            (select pr.STATUS from dbo.POSREQUEST pr 
	              where (pr.IDA_ENTITY= em.IDA_ENTITY) and (pr.IDA_CSSCUSTODIAN = em.IDA_CSSCUSTODIAN) and (pr.DTBUSINESS=em.DTENTITY) and (pr.REQUESTTYPE = 'EOD') 
                and (isnull(pr.IDEM,0)=isnull(em.IDEM,0)) 
                and (isnull(pr.DTUPD,pr.DTINS) = (select max(isnull(prmax.DTUPD,prmax.DTINS)) 
                                                  from dbo.POSREQUEST prmax 
                                                  where (prmax.IDA_ENTITY=pr.IDA_ENTITY) and (prmax.IDA_CSSCUSTODIAN=em.IDA_CSSCUSTODIAN) and 
                                                  (isnull(prmax.IDEM,0)=isnull(em.IDEM,0)) and 
                                                  (prmax.DTBUSINESS=pr.DTBUSINESS) and (prmax.REQUESTTYPE = 'EOD')))
	            ) as EOD_CURRENTSTATUS,              
	            (select count(*) from dbo.POSREQUEST pr 
	              where (pr.IDA_ENTITY = em.IDA_ENTITY) and (pr.IDA_CSSCUSTODIAN = em.IDA_CSSCUSTODIAN) and (isnull(pr.IDEM,0)=isnull(em.IDEM,0)) 
                and (pr.DTBUSINESS=em.DTENTITY) and (pr.REQUESTTYPE = 'EOD') 
	            ) as EOD_TOTAL,
	            (select count(*) from dbo.POSREQUEST pr 
	              where (pr.IDA_ENTITY = em.IDA_ENTITY) and (pr.IDA_CSSCUSTODIAN = em.IDA_CSSCUSTODIAN) and (isnull(pr.IDEM,0)=isnull(em.IDEM,0)) 
                and (pr.DTBUSINESS=em.DTENTITY) and (pr.REQUESTTYPE = 'EOD') 
                and (pr.STATUS in ('SUCCESS','NA'))
	            ) as EOD_SUCCESS,
	            (select count(*) from dbo.POSREQUEST pr 
	              where (pr.IDA_ENTITY = em.IDA_ENTITY) and (pr.IDA_CSSCUSTODIAN = em.IDA_CSSCUSTODIAN) and (isnull(pr.IDEM,0)=isnull(em.IDEM,0)) 
                and (pr.DTBUSINESS=em.DTENTITY) and (pr.REQUESTTYPE = 'EOD') 
                and (pr.STATUS = 'ERROR')
	            ) as EOD_ERROR,
          em.GPRODUCT, em.IDEM, case when em.IDA_CUSTODIAN is null then 0 else case when em.IDEM is null then 1 else 2 end end as SORTED  
	        from dbo.VW_ENTITY_CSSCUSTODIAN em
          where
          (
            (em.IDA_ENTITY=case when (@ENTITY=-1) then em.IDA_ENTITY else @ENTITY end) and 
            (em.CSSCUSTODIAN = case when (-2 < @CSSCUSTODIAN) then em.CSSCUSTODIAN else @CSSCUSTODIAN end) and
            (em.IDA_CSSCUSTODIAN = case when (-1 < @CSSCUSTODIAN) then @CSSCUSTODIAN else em.IDA_CSSCUSTODIAN end)
          )
          
        ) result        
        inner join dbo.ACTOR a_entity on (a_entity.IDA = result.IDA_ENTITY)
        inner join dbo.ACTOR a_csscust on (a_csscust.IDA = result.IDA_CSSCUSTODIAN)
        ";

            return sqlQuery;
        }

        /// <summary>
        /// Retourne la requête associée au traiyement CLOSINGDAY
        /// </summary>
        /// <returns></returns>
        private static string GetQueryClosingDay()
        {
            string query = @"    
        select distinct result.IDEM, 
        result.IDA_ENTITY, a_entity.IDENTIFIER as ENTITY_IDENTIFIER, a_entity.DISPLAYNAME as ENTITY_DISPLAYNAME, 
        result.IDA_CSSCUSTODIAN, a_csscust.IDENTIFIER as CSSCUSTODIAN_IDENTIFIER,  a_csscust.DISPLAYNAME as CSSCUSTODIAN_DISPLAYNAME, 
        result.DTENTITY, result.DTENTITYNEXT, result.EMKEY,
        result.CLOSINGDAY_CURRENTSTATUS,
        result.CLOSINGDAY_TOTAL  as CLOSINGDAY_TOTALCOUNT,
        case when result.CLOSINGDAY_Success > 0 then 'SUCCESS'   else 'None' end as CLOSINGDAY_SUCCESSSTATUS, nullif(result.CLOSINGDAY_Success, 0) as CLOSINGDAY_SUCCESSCOUNT,
        case when result.CLOSINGDAY_Error   > 0 then 'ERROR'     else 'None' end as CLOSINGDAY_ERRORSTATUS,   nullif(result.CLOSINGDAY_Error, 0)   as CLOSINGDAY_ERRORCOUNT,
        result.GPRODUCT, result.SORTED
        from 
        (
	        select em.EMKEY, em.IDA_ENTITY, em.IDA_CSSCUSTODIAN, em.DTENTITY, em.DTENTITYNEXT,
	            (select pr.STATUS from dbo.POSREQUEST pr 
	              where (pr.IDA_ENTITY= em.IDA_ENTITY) and (pr.IDA_CSSCUSTODIAN = em.IDA_CSSCUSTODIAN) and (pr.DTBUSINESS=em.DTENTITY) and (pr.REQUESTTYPE = 'CLOSINGDAY') 
                  and (isnull(pr.IDEM,0)=isnull(em.IDEM,0)) 
                  and (isnull(pr.DTUPD,pr.DTINS) = (select max(isnull(prmax.DTUPD,prmax.DTINS)) 
                                                    from dbo.POSREQUEST prmax 
                                                    where (prmax.IDA_ENTITY=pr.IDA_ENTITY) and (prmax.IDA_CSSCUSTODIAN=pr.IDA_CSSCUSTODIAN) AND (isnull(prMAX.IDEM,0)=isnull(em.IDEM,0)) 
                                                      and (prmax.DTBUSINESS=pr.DTBUSINESS) and (prmax.REQUESTTYPE = 'CLOSINGDAY')))
	            ) as CLOSINGDAY_CURRENTSTATUS,              
	            (select count(*) from dbo.POSREQUEST pr 
	              where (pr.IDA_ENTITY = em.IDA_ENTITY) and (pr.IDA_CSSCUSTODIAN = em.IDA_CSSCUSTODIAN) and (isnull(pr.IDEM,0)=isnull(em.IDEM,0)) 
                and (pr.DTBUSINESS=em.DTENTITY) and (pr.REQUESTTYPE = 'CLOSINGDAY') 
	            ) as CLOSINGDAY_TOTAL,
	            (select count(*) from dbo.POSREQUEST pr 
	              where (pr.IDA_ENTITY = em.IDA_ENTITY) and (pr.IDA_CSSCUSTODIAN = em.IDA_CSSCUSTODIAN) and (isnull(pr.IDEM,0)=isnull(em.IDEM,0)) 
                and (pr.DTBUSINESS=em.DTENTITY) and (pr.REQUESTTYPE = 'CLOSINGDAY') and
                (pr.STATUS in ('SUCCESS','NA'))
	            ) as CLOSINGDAY_SUCCESS,
	            (select count(*) from dbo.POSREQUEST pr 
	              where (pr.IDA_ENTITY = em.IDA_ENTITY) and (pr.IDA_CSSCUSTODIAN = em.IDA_CSSCUSTODIAN) and (isnull(pr.IDEM,0)=isnull(em.IDEM,0)) 
                and (pr.DTBUSINESS=em.DTENTITY) and (pr.REQUESTTYPE = 'CLOSINGDAY') and
                (pr.STATUS = 'ERROR')
	            ) as CLOSINGDAY_ERROR,
              em.GPRODUCT, em.IDEM, case when em.IDA_CUSTODIAN is null then 0 else case when em.IDEM is null then 1 else 2 end end as SORTED  
	        from dbo.VW_ENTITY_CSSCUSTODIAN em
          where
          (
            (em.IDA_ENTITY=case when (@ENTITY=-1) then em.IDA_ENTITY else @ENTITY end) and 
            (em.CSSCUSTODIAN = case when (-2 < @CSSCUSTODIAN) then em.CSSCUSTODIAN else @CSSCUSTODIAN end) and
            (em.IDA_CSSCUSTODIAN = case when (-1 < @CSSCUSTODIAN) then @CSSCUSTODIAN else em.IDA_CSSCUSTODIAN end)
          )
        ) result
        inner join dbo.ACTOR a_entity on (a_entity.IDA=result.IDA_ENTITY)
        inner join dbo.ACTOR a_csscust on (a_csscust.IDA=result.IDA_CSSCUSTODIAN)";


            return query;
        }
        #endregion
    }
}
