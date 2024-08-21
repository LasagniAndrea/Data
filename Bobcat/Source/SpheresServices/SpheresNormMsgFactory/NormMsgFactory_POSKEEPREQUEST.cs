#region Using Directives
using EFS.ACommon;
using EFS.ApplicationBlocks.Data;
using EFS.Common;
using EFS.Common.Log;
using EFS.Common.MQueue;
using EFS.LoggerClient;
using EFS.LoggerClient.LoggerService;
using EfsML.Business;
using EfsML.DynamicData;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
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
    public class NormMsgFactory_POSKEEPREQUEST : NormMsgFactoryBase
    {
        #region Constructors
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pNormMsgFactoryProcess"></param>
        public NormMsgFactory_POSKEEPREQUEST(NormMsgFactoryProcess pNormMsgFactoryProcess)
            : base(pNormMsgFactoryProcess)
        {

        }
        #endregion Constructors

        #region method
        /// <summary>
        /// Alimentation de m_MQueueAttributes.parameters
        /// </summary>
        /// <returns></returns>
        /// FI 20170327 [23004] Modify
        // EG 20190114 Add detail to ProcessLog Refactoring
        // EG 20231129 [WI762] End of Day processing : Possibility to request processing without margin Requirement(Cst.PosRequestTypeEnum.EndOfDayWithoutMR)
        protected override Cst.ErrLevel CreateParameters()
        {

            Cst.ErrLevel codeReturn = Cst.ErrLevel.SUCCESS;

            if (BuildingInfo.posRequestTypeSpecified)
            {
                switch (BuildingInfo.posRequestType)
                {
                    case Cst.PosRequestTypeEnum.ClosingDay:
                    case Cst.PosRequestTypeEnum.EndOfDay:
                    case Cst.PosRequestTypeEnum.EndOfDayWithoutInitialMargin:
                    case Cst.PosRequestTypeEnum.UpdateEntry:  // FI 20170327 [23004] Add UpdateEntry   
                        if (false == BuildingInfo.parametersSpecified)
                        {
                            // FI 20200623 [XXXXX] SetErrorWarning
                            m_NormMsgFactoryProcess.ProcessState.SetErrorWarning(ProcessStateTools.StatusErrorEnum);
                            
                            // ERROR = Add LOG
                            // FI 20170327 [23004] Affichage des paramètres obligatoires UNIQUEMENT (PARAM_ENTITY et PARAM_CSSCUSTODIAN) 
                            
                            Logger.Log(new LoggerData(LogLevelEnum.Error, new SysMsgCode(SysCodeEnum.SYS, 8000), 2,
                                new LogParam(NormMsgFactoryMQueue.PARAM_ENTITY + " / " + NormMsgFactoryMQueue.PARAM_CSSCUSTODIAN),
                                new LogParam(m_NormMsgFactoryProcess.LogId),
                                new LogParam(m_NormMsgFactoryProcess.LogProcessType)));

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
                            if (BuildingInfo.parameters[NormMsgFactoryMQueue.PARAM_DTBUSINESS] is MQueueparameter DtBusinness)
                                AddMQueueAttributesParameter(DtBusinness);

                            if (BuildingInfo.parameters[NormMsgFactoryMQueue.PARAM_GPRODUCT] is MQueueparameter gProduct)
                                AddMQueueAttributesParameter(gProduct);
                        }


                        if (Cst.ErrLevel.SUCCESS == codeReturn)
                        {
                            if (BuildingInfo.posRequestType == Cst.PosRequestTypeEnum.UpdateEntry)
                            {
                                // FI 20170327 [23004] Valorisation du paramètre MARKET 
                                // Paramètre alimenté systématiquement parce que nécessaire à la requête (voir méthod GetQueryUpdateEntry)
                                if (BuildingInfo.parameters[NormMsgFactoryMQueue.PARAM_MARKET] is MQueueparameter)
                                {
                                    AddMarketParameter(null);
                                }
                                else
                                {
                                    MQueueparameter parameter = new MQueueparameter(NormMsgFactoryMQueue.PARAM_MARKET, TypeData.TypeDataEnum.integer);
                                    parameter.SetValue(-1);
                                    AddMQueueAttributesParameter(parameter);
                                }
                            }
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
                // FI 20200623 [XXXXX] SetErrorWarning
                m_NormMsgFactoryProcess.ProcessState.SetErrorWarning(ProcessStateTools.StatusErrorEnum);

                
                Logger.Log(new LoggerData(LogLevelEnum.Error, new SysMsgCode(SysCodeEnum.SYS, 8020), 2,
                    new LogParam("posRequestType (EOD, CLOSINGDAY, ...)"),
                    new LogParam(LogTools.IdentifierAndId(BuildingInfo.identifier, BuildingInfo.id)),
                    new LogParam(m_NormMsgFactoryProcess.LogId),
                    new LogParam(m_NormMsgFactoryProcess.LogProcessType)));

                codeReturn = Cst.ErrLevel.FAILURE;
            }
            return codeReturn;
        }

        /// <summary>
        /// Retourne La requête SQL à l'origine des Mqueues générés
        /// </summary>
        /// <returns></returns>
        // EG 20231129 [WI762] End of Day processing : Possibility to request processing without margin Requirement(Cst.PosRequestTypeEnum.EndOfDayWithoutMR)
        protected override QueryParameters GetQueryParameters()
        {
            string cs = m_NormMsgFactoryProcess.Cs;
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


            string query;
            switch (BuildingInfo.posRequestType)
            {
                case Cst.PosRequestTypeEnum.EndOfDay:
                case Cst.PosRequestTypeEnum.EndOfDayWithoutInitialMargin:
                    query = GetQueryEndOfDay();
                    break;
                case Cst.PosRequestTypeEnum.ClosingDay:
                    query = GetQueryClosingDay();
                    break;
                case Cst.PosRequestTypeEnum.UpdateEntry: // FI 20170327 [23004] add UpdateEntry
                    dp.Add(new DataParameter(cs, NormMsgFactoryMQueue.PARAM_MARKET, DbType.Int32),
                        parameters.GetIntValueParameterById(NormMsgFactoryMQueue.PARAM_MARKET));
                    query = GetQueryUpdateEntry();
                    break;
                default:
                    throw new NotImplementedException(StrFunc.AppendFormat("posRequestType:{0} is not implemented", BuildingInfo.posRequestType.ToString()));
            }

            SQLWhere sqlwhere = new SQLWhere();
            if (dp.Contains("GPRODUCT"))
                sqlwhere.Append("result.GPRODUCT=@GPRODUCT");
            if (dp.Contains("DTBUSINESS"))
                sqlwhere.Append("result.DTENTITY=@DTBUSINESS");
            if (sqlwhere.Length() > 0)
                query += sqlwhere;

            QueryParameters ret = new QueryParameters(m_NormMsgFactoryProcess.Cs, query, dp);

            return ret;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected override DataRow[] RowsCandidates(DataTable pDt)
        {
            DataRow[] ret = pDt.Select(null, "IDA_ENTITY, ENTITY_IDENTIFIER, IDA_CSSCUSTODIAN, CSSCUSTODIAN_IDENTIFIER, DTENTITY, GPRODUCT", DataViewRowState.OriginalRows);
            return ret;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pDr"></param>
        /// <returns></returns>
        /// FI 20170327 [23004] Modify
        // EG 20231129 [WI762] End of Day processing : Possibility to request processing without margin Requirement(Cst.PosRequestTypeEnum.EndOfDayWithoutInitialMargin)
        protected override MQueueBase BuildMessageQueue(DataRow pDr)
        {
            PosKeepingRequestMQueue mqueue = new PosKeepingRequestMQueue(BuildingInfo.posRequestType, m_MQueueAttributes);

            DateTime DtBusiness = Convert.ToDateTime(pDr["DTENTITY"]);
            Pair<int, string> entity = new Pair<int, string>(Convert.ToInt32(pDr["IDA_ENTITY"]), Convert.ToString(pDr["ENTITY_IDENTIFIER"]));
            Pair<int, string> cssCustodian = new Pair<int, string>(Convert.ToInt32(pDr["IDA_CSSCUSTODIAN"]), Convert.ToString(pDr["CSSCUSTODIAN_IDENTIFIER"]));

            string gProduct = Convert.ToString(pDr["GPRODUCT"]);

            DictionaryEntry[] dic = null;
            switch (BuildingInfo.posRequestType)
            {
                case Cst.PosRequestTypeEnum.ClosingDay:
                case Cst.PosRequestTypeEnum.EndOfDay:
                case Cst.PosRequestTypeEnum.EndOfDayWithoutInitialMargin:
                case Cst.PosRequestTypeEnum.UpdateEntry:
                    // FI 20170327 [23004] Ajout systématique de IDA_ENTITY et IDA_CSSCUSTODIAN
                    dic = new DictionaryEntry[] {
                                        new DictionaryEntry("GPRODUCT",  gProduct),
                                        new DictionaryEntry("REQUESTTYPE", BuildingInfo.posRequestType),
                                        new DictionaryEntry("DTBUSINESS", DtFunc.DateTimeToStringDateISO(DtBusiness)),
                                        new DictionaryEntry("IDA_ENTITY",entity.First),
                                        new DictionaryEntry("ENTITY",entity.Second),
                                        new DictionaryEntry("IDA_CSSCUSTODIAN",cssCustodian.First),
                                        new DictionaryEntry("CSSCUSTODIAN", cssCustodian.Second)};

                    if (BuildingInfo.posRequestType == Cst.PosRequestTypeEnum.UpdateEntry)
                    {
                        // FI 20170327 [23004] Ajout des entrées nécessaires à UpdateEntry
                        dic =
                              (from item in
                                   (from itemx in dic select itemx).Concat(
                                        new DictionaryEntry[] {
                                        new DictionaryEntry("IDEM",  Convert.ToInt32(pDr["IDEM"])),
                                        new DictionaryEntry("IDM",  Convert.ToInt32(pDr["IDM"])),
                                        new DictionaryEntry("MARKET",  Convert.ToString(pDr["MARKET"])),
                                        new DictionaryEntry("IDI",  Convert.ToInt32(pDr["IDI"])),
                                        new DictionaryEntry("IDASSET",  Convert.ToInt32(pDr["IDASSET"])),
                                        new DictionaryEntry("CONTRACTNAME", PosKeepingTools.GetContractName(pDr)),
                                        new DictionaryEntry("IDT",  Convert.ToInt32(pDr["IDT"])),
                                        new DictionaryEntry("TRADEIDENTIFIER",  Convert.ToString(pDr["TRADE_IDENTIFIER"])),
                                        new DictionaryEntry("IDA_DEALER", Convert.ToInt32(pDr["IDA_DEALER"])),
                                        new DictionaryEntry("DEALER_IDENTIFIER", Convert.ToString(pDr["DEALER_IDENTIFIER"])),
                                        new DictionaryEntry("IDB_DEALER", Convert.ToInt32(pDr["IDB_DEALER"])),
                                        new DictionaryEntry("DEALER_BOOKIDENTIFIER", Convert.ToString(pDr["DEALER_BOOKIDENTIFIER"])),
                                        new DictionaryEntry("IDA_CLEARER", Convert.ToInt32(pDr["IDA_CLEARER"])),
                                        new DictionaryEntry("CLEARER_IDENTIFIER", Convert.ToString(pDr["CLEARER_IDENTIFIER"])),
                                        new DictionaryEntry("IDB_CLEARER", Convert.ToInt32(pDr["IDB_CLEARER"])),
                                        new DictionaryEntry("CLEARER_BOOKIDENTIFIER", Convert.ToString(pDr["CLEARER_BOOKIDENTIFIER"])),
                                        new DictionaryEntry("IDA_ENTITYCLEARER", Convert.ToInt32(pDr["IDA_ENTITYCLEARER"]))})
                               select item).ToArray();
                    }

                    break;
                default:
                    throw new NotImplementedException(StrFunc.AppendFormat("posRequestType:{0} is not implemented", BuildingInfo.posRequestType.ToString()));
            }

            mqueue.idInfoSpecified = true;
            mqueue.idInfo = new IdInfo
            {
                idInfos = dic
            };

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
            string requestType = "('EOD','EOD_WOIM')";
            string sqlQuery = $@"select distinct result.IDEM, 
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
                                                      (prmax.DTBUSINESS=pr.DTBUSINESS) and (prmax.REQUESTTYPE in {requestType})))
	                ) as EOD_CURRENTSTATUS,              
	                (select count(*) from dbo.POSREQUEST pr 
	                  where (pr.IDA_ENTITY = em.IDA_ENTITY) and (pr.IDA_CSSCUSTODIAN = em.IDA_CSSCUSTODIAN) and (isnull(pr.IDEM,0)=isnull(em.IDEM,0)) 
                    and (pr.DTBUSINESS=em.DTENTITY) and (pr.REQUESTTYPE in {requestType}) 
	                ) as EOD_TOTAL,
	                (select count(*) from dbo.POSREQUEST pr 
	                  where (pr.IDA_ENTITY = em.IDA_ENTITY) and (pr.IDA_CSSCUSTODIAN = em.IDA_CSSCUSTODIAN) and (isnull(pr.IDEM,0)=isnull(em.IDEM,0)) 
                    and (pr.DTBUSINESS=em.DTENTITY) and (pr.REQUESTTYPE in {requestType}) 
                    and (pr.STATUS in ('SUCCESS','NA'))
	                ) as EOD_SUCCESS,
	                (select count(*) from dbo.POSREQUEST pr 
	                  where (pr.IDA_ENTITY = em.IDA_ENTITY) and (pr.IDA_CSSCUSTODIAN = em.IDA_CSSCUSTODIAN) and (isnull(pr.IDEM,0)=isnull(em.IDEM,0)) 
                    and (pr.DTBUSINESS=em.DTENTITY) and (pr.REQUESTTYPE in {requestType}) 
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
            inner join dbo.ACTOR a_csscust on (a_csscust.IDA = result.IDA_CSSCUSTODIAN)";
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

        /// <summary>
        ///  Requête de chargement du process de mise à jour des clôtures 
        /// </summary>
        /// <returns></returns>
        // FI 20170327 [23004] Modify 
        // PL 20170405 [23047] Remove TRADE table
        // EG 20171016 [23509] Upd DTEXECUTION remplace DTTIMESTAMP
        // EG 20200226 Refactoring suite à à TRADEINSTRUMENT (INSTRUMENTNO=1) dans TRADE
        // EG 20230102 [WI500] Refactoring complet de la requête pour ne plus tomber en Timeout(Usage de DTOUT et de clauses With)
        private string GetQueryUpdateEntry()
        {
            MQueueparameters parameters = m_MQueueAttributes.parameters;

            List<StringDynamicData> dynamicArgs = new List<StringDynamicData>
            {
                new StringDynamicData(TypeData.TypeDataEnum.integer.ToString(), "ENTITY", parameters.GetIntValueParameterById(NormMsgFactoryMQueue.PARAM_ENTITY).ToString()),
                new StringDynamicData(TypeData.TypeDataEnum.integer.ToString(), "CSSCUSTODIAN", parameters.GetIntValueParameterById(NormMsgFactoryMQueue.PARAM_CSSCUSTODIAN).ToString()),
                new StringDynamicData(TypeData.TypeDataEnum.integer.ToString(), "MARKET", parameters.GetIntValueParameterById(NormMsgFactoryMQueue.PARAM_MARKET).ToString())
            };

            TypeBuilder dynamicArgsType = new TypeBuilder(m_NormMsgFactoryProcess.Cs, dynamicArgs, "DynamicData", "ReferentialsReferential");

            //Cette requête est un copier/coller de la requête présente dans le fichier POSKEEPING_UPDATEENTRY
            //Ensuite 
            //- Il faut exclure les directives SESSIONRESTRICT
            //- Il faut ajouter une restriction sur la chambre
            string query = @"with TRADEOPEN_W as 
            (
	            select 
	            tr.IDT, tr.IDM, tr.IDASSET, tr.SIDE, tr.DTOUT, floor(tr.QTY) as QTY, em.DTENTITY, tr.DTBUSINESS, tr.DTEXECUTION,
	            tr.IDI, em.IDEM, tr.IDA_CSSCUSTODIAN, tr.IDA_ENTITY, tr.IDA_DEALER, tr.IDB_DEALER, tr.IDA_CLEARER, tr.IDB_CLEARER, 'FUT' as GPRODUCT
	            from dbo.TRADE tr
	            inner join VW_INSTR_PRODUCT pr on ( pr.IDI = tr.IDI) and (pr.FUNGIBILITYMODE != 'NONE') and (pr.GPRODUCT = 'FUT')
	            inner join ENTITYMARKET em on ( em.IDM = tr.IDM ) and (em.IDA = tr.IDA_ENTITY) and (em.IDA_CUSTODIAN is null)
	            where (tr.IDSTACTIVATION = 'REGULAR') and (tr.IDSTBUSINESS = 'ALLOC') and (tr.IDA_ENTITY = 116) and (tr.DTOUT is null or tr.DTOUT > em.DTENTITY)
                <choose><when test=""{ ENTITY}> -1""> and (tr.IDA_ENTITY = @ENTITY)</when></choose>   
                <choose><when test = ""{MARKET}>-1""> and (tr.IDM = @MARKET) </when></choose>
                <choose><when test=""{CSSCUSTODIAN}>-1""> and (tr.IDA_CSSCUSTODIAN = @CSSCUSTODIAN)</when></choose>
            ),
            TRADE_W as 
            (
                select
                tr.IDT, tr.IDM, tr.IDASSET, tr.SIDE, tr.DTOUT, floor(tr.QTY) as QTY, em.DTENTITY, tr.DTBUSINESS, tr.DTEXECUTION,
	            tr.IDI, em.IDEM, tr.IDA_CSSCUSTODIAN, tr.IDA_ENTITY, tr.IDA_DEALER, tr.IDB_DEALER, tr.IDA_CLEARER, tr.IDB_CLEARER, 'FUT' as GPRODUCT
                from dbo.TRADE tr
                inner join VW_INSTR_PRODUCT pr on (pr.IDI = tr.IDI) and(pr.FUNGIBILITYMODE != 'NONE') and(pr.GPRODUCT = 'FUT')
                inner join ENTITYMARKET em on(em.IDM = tr.IDM) and(em.IDA = tr.IDA_ENTITY) and(em.IDA_CUSTODIAN is null)
                where(tr.IDSTACTIVATION = 'REGULAR') and(tr.IDSTBUSINESS = 'ALLOC') and(tr.DTOUT is null or tr.DTOUT > em.DTENTITY) and (tr.DTBUSINESS = em.DTENTITY)
                <choose><when test = ""{ENTITY}>-1""> and(tr.IDA_ENTITY = @ENTITY) </when></choose>
                <choose><when test = ""{ MARKET}>-1""> and(tr.IDM = @MARKET) </when></choose>
                <choose><when test=""{CSSCUSTODIAN}>-1""> and (tr.IDA_CSSCUSTODIAN = @CSSCUSTODIAN)</when></choose>
            ),
            POS_W as 
            (
                select tw.IDT, tw.IDASSET, tw.DTBUSINESS, tw.QTY, tw.IDI, tw.SIDE, tw.DTEXECUTION, tw.IDM, tw.DTENTITY, tw.IDEM, tw.IDA_CSSCUSTODIAN,
	            tw.IDA_DEALER, tw.IDB_DEALER, tw.IDA_ENTITY, tw.IDA_CLEARER, tw.IDB_CLEARER,  
	            isnull(pos.QTY_SELL, 0) as QTY_SELL, isnull(pos.QTY_BUY, 0) as QTY_BUY
                from TRADE_W tw
                left outer join
                (
                    select pad.IDT_BUY as IDT, sum(isnull(pad.QTY, 0)) as QTY_BUY, 0 as QTY_SELL
                    from TRADE_W alloc
                    inner join dbo.POSACTIONDET pad on (pad.IDT_BUY = alloc.IDT)
                    inner join dbo.POSACTION pa on (pa.IDPA = pad.IDPA)
                    where (pa.DTOUT is null or pa.DTOUT > alloc.DTENTITY) and(pa.DTBUSINESS <= alloc.DTENTITY) and((pad.DTCAN is null) or(pad.DTCAN > alloc.DTENTITY))
                    group by pad.IDT_BUY

                    union all

                    select pad.IDT_SELL as IDT, 0 as QTY_BUY, sum(isnull(pad.QTY, 0)) as QTY_SELL
                    from TRADE_W alloc
                    inner join dbo.POSACTIONDET pad on (pad.IDT_SELL = alloc.IDT)
                    inner join dbo.POSACTION pa on (pa.IDPA = pad.IDPA)
                    where (pa.DTOUT is null or pa.DTOUT > alloc.DTENTITY) and(pa.DTBUSINESS <= alloc.DTENTITY) and((pad.DTCAN is null) or(pad.DTCAN > alloc.DTENTITY))
                    group by pad.IDT_SELL
	            ) pos on(pos.IDT = tw.IDT)
                where (tw.QTY -isnull(pos.QTY_SELL, 0) - isnull(pos.QTY_BUY, 0) > 0)
            )
            select result.IDT, result.IDA_ENTITY, a_entity.IDENTIFIER as ENTITY_IDENTIFIER, a_entity.DISPLAYNAME as ENTITY_DISPLAYNAME, 
            result.IDA_CSSCUSTODIAN, a_csscust.IDENTIFIER as CSSCUSTODIAN_IDENTIFIER, m.SHORT_ACRONYM as MARKET, result.DTENTITY, result.IDEM, result.IDM, 
            result.IDI, result.IDASSET, result.IDA_DEALER, result.IDB_DEALER, result.IDA_CLEARER, result.IDB_CLEARER,           
            /*--DEALER--*/
            dealer.IDENTIFIER as DEALER_IDENTIFIER,dealer.DISPLAYNAME as DEALER_DISPLAYNAME,
            bdealer.IDENTIFIER as DEALER_BOOKIDENTIFIER,
            case when arCLIENT.IDA is not null then 'Client' else 'House' end as ROLEDEALER,
            /*--CLEARER--*/
            clearer.IDENTIFIER as CLEARER_IDENTIFIER, clearer.DISPLAYNAME as CLEARER_DISPLAYNAME,
            bclearer.IDENTIFIER as CLEARER_BOOKIDENTIFIER,
            /*--TRADE IDENTIFIER--*/
            tr.IDENTIFIER as TRADE_IDENTIFIER,
            /*--CATEGORY--*/
            case asset.CATEGORY when 'F' then 'Future' else 'Option' end as CATEGORY,
            /*--ASSET IDENTIFIER--*/
            asset.IDENTIFIER as ASSET_IDENTIFIER,
            /*--DERIVATIVECONTRACT--*/
            asset.IDDC as ASSET_IDDC, asset.CONTRACTIDENTIFIER, asset.CONTRACTDISPLAYNAME,
            /*--PUTCALL--*/
            case asset.PUTCALL when '0' then 'Put' when '1' then 'Call' else '' end as PUTCALL,
            /*--STRIKE--*/
            asset.STRIKEPRICE as STRIKE,
            /*--MATURITY--*/
            asset.MATFMT_MMMYY, result.GPRODUCT
            from 
            (
                select max(posw.IDT) as IDT, posw.IDM, posw.DTENTITY, posw.IDI, posw.IDASSET, posw.IDEM, posw.IDA_CSSCUSTODIAN,
                posw.IDA_ENTITY, posw.IDA_DEALER, posw.IDB_DEALER, posw.IDA_CLEARER, posw.IDB_CLEARER, 'FUT' as GPRODUCT
                from POS_W posw
                where (posw.QTY - posw.QTY_SELL - posw.QTY_BUY > 0) and
                exists(
                    select 1
                    from TRADEOPEN_W tw
                    left outer join
                    (
                        select pad.IDT_BUY as IDT, sum(isnull(pad.QTY, 0)) as QTY_BUY, 0 as QTY_SELL
                        from TRADEOPEN_W alloc
                        inner join dbo.POSACTIONDET pad on (pad.IDT_BUY = alloc.IDT)
                        inner join dbo.POSACTION pa on (pa.IDPA = pad.IDPA)
                        where (pa.DTOUT is null or pa.DTOUT > alloc.DTENTITY) and(pa.DTBUSINESS <= alloc.DTENTITY) and((pad.DTCAN is null) or(pad.DTCAN > alloc.DTENTITY))
                        group by pad.IDT_BUY

                        union all

                        select pad.IDT_SELL as IDT, 0 as QTY_BUY, sum(isnull(pad.QTY, 0)) as QTY_SELL
                        from TRADEOPEN_W alloc
                        inner join dbo.POSACTIONDET pad on (pad.IDT_SELL = alloc.IDT)
                        inner join dbo.POSACTION pa on (pa.IDPA = pad.IDPA)
                        where (pa.DTOUT is null or pa.DTOUT > alloc.DTENTITY) and(pa.DTBUSINESS <= alloc.DTENTITY) and((pad.DTCAN is null) or(pad.DTCAN > alloc.DTENTITY))
                        group by pad.IDT_SELL
                    ) pos on(pos.IDT = tw.IDT)
                    where (tw.DTBUSINESS <= posw.DTBUSINESS) and
                          (tw.IDASSET = posw.IDASSET) and
                          (tw.IDI = posw.IDI) and
                          (tw.IDA_DEALER = posw.IDA_DEALER) and
                          (tw.IDB_DEALER = posw.IDB_DEALER) and
                          (tw.IDA_CLEARER = posw.IDA_CLEARER) and
                          (tw.IDB_CLEARER = posw.IDB_CLEARER) and
                          (tw.SIDE <> posw.SIDE) and
                          (tw.DTEXECUTION <= posw.DTEXECUTION) and
                          (tw.QTY - isnull(pos.QTY_SELL, 0) - isnull(pos.QTY_BUY, 0) > 0)
                )
                group by posw.IDM, posw.DTENTITY, posw.IDI, posw.IDASSET, posw.IDEM, posw.IDA_CSSCUSTODIAN, posw.DTBUSINESS,
                         posw.IDA_DEALER, posw.IDB_DEALER, posw.IDA_ENTITY, posw.IDA_CLEARER, posw.IDB_CLEARER
            ) result
            inner join dbo.ACTOR dealer                on(dealer.IDA = result.IDA_DEALER)
            inner join dbo.BOOK bdealer               on(bdealer.IDB = result.IDB_DEALER)
            inner join dbo.ACTOR clearer               on(clearer.IDA = result.IDA_CLEARER)
            inner join dbo.BOOK bclearer              on(bclearer.IDB = result.IDB_CLEARER)
            inner join dbo.VW_ASSET_ETD_EXPANDED asset on(asset.IDASSET = result.IDASSET)
            inner join dbo.VW_MARKET_IDENTIFIER m      on(m.IDM = result.IDM)
            inner join dbo.ACTOR a_entity              on(a_entity.IDA = result.IDA_ENTITY)
            inner join dbo.ACTOR a_csscust             on(a_csscust.IDA = result.IDA_CSSCUSTODIAN)
            inner join dbo.TRADE tr                    on(tr.IDT = result.IDT)
            left outer join(select IDA from dbo.ACTOR where exists(select 1 from dbo.ACTORROLE where IDA = ACTOR.IDA and IDROLEACTOR = 'CLIENT'))  arCLIENT on arCLIENT.IDA = dealer.IDA";

            query = StrFuncExtended.ReplaceChooseExpression2(query, dynamicArgsType.GetNewObject(), true);

            return query;

        }



        /// <summary>
        /// Postage des messages Queues générés 
        /// </summary>
        /// <returns></returns>
        /// FI 20170327 [23004] add override (1 ligne tracker par message)
        // EG 20190114 Add detail to ProcessLog Refactoring
        public override Cst.ErrLevel SendFinalMessages()
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
                        caller = BuildingInfo.posRequestTypeSpecified ? BuildingInfo.posRequestType.ToString() : string.Empty
                    },
                    sendInfo = EFS.SpheresService.ServiceTools.GetMqueueSendInfo(BuildingInfo.processType, m_NormMsgFactoryProcess.AppInstance)
                };

                if (null != Acknowledgment) // Acknowledgment peut être null (Exemple Interruption de traitement)
                {
                    m_TaskInfo.trackerAttrib.acknowledgment = new TrackerAcknowledgmentInfo
                    {
                        extlId = Acknowledgment.extlId,
                        schedules = Acknowledgment.schedules

                    };
                }

                foreach (MQueueBase mQueue in m_SendingMQueue) // (1 ligne tracker par message)
                {
                    m_TaskInfo.trackerAttrib.info = TrackerAttributes.BuildInfo(mQueue);
                    m_TaskInfo.mQueue = new MQueueBase[] { mQueue };

                    MQueueTaskInfo.SendMultiple(m_TaskInfo);
                }

                
                Logger.Log(new LoggerData(LogLevelEnum.Debug, new SysMsgCode(SysCodeEnum.LOG, 8003), 1,
                    new LogParam(m_NormMsgFactoryProcess.LogId),
                    new LogParam(m_NormMsgFactoryProcess.LogProcessType),
                    new LogParam(m_SendingMQueue.Length)));
            }
            return Cst.ErrLevel.SUCCESS;
        }
        #endregion
    }
}
