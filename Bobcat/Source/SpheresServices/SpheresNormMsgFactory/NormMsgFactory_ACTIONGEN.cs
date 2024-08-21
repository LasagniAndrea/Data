#region Using Directives
//
using EFS.ACommon;
using EFS.ApplicationBlocks.Data;
using EFS.Common;
using EFS.Common.MQueue;
using EFS.LoggerClient;
using EFS.LoggerClient.LoggerService;
//
using EfsML.Business;
using EfsML.DynamicData;
using System;
using System.Collections.Generic;
using System.Data;
#endregion Using Directives

namespace EFS.Process
{
    /// <summary>
    /// Construction Messages de type : TradeActionGenMQueue
    /// 
    /// DONNEES attendues en entrée : 
    ///<para>────────────────────────────────────────────────────────────────────────────────────────────────</para>
    ///<para>  ● processType = FEESCALCULATION</para>
    ///<para>────────────────────────────────────────────────────────────────────────────────────────────────</para>
    ///
    /// PARAMETRES attendus en entrée (* = optionnel):
    ///<para>────────────────────────────────────────────────────────────────────────────────────────────────  </para>
    ///<para>  ● ENTITY                           : Identifiant de l'entité</para>
    ///<para>  ● CSSCUSTODIAN (ou CLEARINGHOUSE)  : Identifiant de la chambre de compensation ou du custodian  </para>
    ///<para>  ● ISMANFEES_PRESERVED*             : Les frais calculés manuellement seront-ils conservés       </para>
    ///<para>  ● ISFORCEDFEES_PRESERVED*          : Les frais forcés seront-ils conservés                      </para>
    ///<para>────────────────────────────────────────────────────────────────────────────────────────────────  </para> 
    /// Préparation du(des) message(s) normalisé(s) de type TradeActionGenMQueue
    /// Recalcul des frais
    ///<para>────────────────────────────────────────────────────────────────────────────────────────────────</para>
    ///<para>  ● Lecture, contrôle des paramètres présents, recherche de leurs ID</para>
    ///<para>  ● Construction des messages finaux</para>
    ///<para>────────────────────────────────────────────────────────────────────────────────────────────────</para> 
    ///</summary>
    /// FI 20170314 [22225] Add
    public class NormMsgFactory_ACTIONGEN : NormMsgFactoryBase
    {


        /// <summary>
        /// 
        /// </summary>
        /// <param name="pNormMsgFactoryProcess"></param>
        public NormMsgFactory_ACTIONGEN(NormMsgFactoryProcess pNormMsgFactoryProcess)
            : base(pNormMsgFactoryProcess)
        {

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
                    new LogParam(NormMsgFactoryMQueue.PARAM_ENTITY + " / " + NormMsgFactoryMQueue.PARAM_CSSCUSTODIAN),
                    new LogParam(m_NormMsgFactoryProcess.LogId),
                    new LogParam(m_NormMsgFactoryProcess.LogProcessType)));

                codeReturn = Cst.ErrLevel.FAILURE;
            }

            if (Cst.ErrLevel.SUCCESS == codeReturn)
            {
                switch (BuildingInfo.processType)
                {
                    case Cst.ProcessTypeEnum.FEESCALCULATION:

                        codeReturn = AddEntityParameter();
                        if (Cst.ErrLevel.SUCCESS == codeReturn)
                        {
                            AddBuildingInfoParameterCssCustodianParamerfromClearingHouse(true);
                            codeReturn = AddCssCustodianParameter();
                        }

                        // FI 20180328 [23871] Add paramètres DATE1, DATE2, FEESCALCULATIONMODE etc..
                        if (Cst.ErrLevel.SUCCESS == codeReturn)
                            codeReturn = AddDateParameter(NormMsgFactoryMQueue.PARAM_DATE1, DtFuncML.BUSINESS);

                        if (Cst.ErrLevel.SUCCESS == codeReturn)
                            codeReturn = AddDateParameter(NormMsgFactoryMQueue.PARAM_DATE2, DtFuncML.BUSINESS);

                        if (Cst.ErrLevel.SUCCESS == codeReturn)
                            codeReturn = AddFeesCalculationModeParameter();

                        //NormsqgFactory ne permet pas de choisir un frais et/ou un barème et/ou une condition particulière 
                        foreach (string item in new string[] { "FEE", "FEESCHEDULE", "FEEMATRIX" })
                        {
                            MQueueparameter p = new MQueueparameter(item, TypeData.TypeDataEnum.integer)
                            {
                                Value = "-1",
                                ExValue = Ressource.GetString(StrFunc.AppendFormat("{0}_ALL", item)),
                                ExValueSpecified = true
                            };
                            AddMQueueAttributesParameter(p);
                        }

                        if (Cst.ErrLevel.SUCCESS == codeReturn)
                        {
                            codeReturn = AddBooleanParameter(NormMsgFactoryMQueue.PARAM_ISMANFEES_PRESERVED);
                            if (codeReturn == Cst.ErrLevel.SUCCESS)
                                codeReturn = AddBooleanParameter(NormMsgFactoryMQueue.PARAM_ISFORCEDFEES_PRESERVED);
                        }
                        break;
                    default:
                        // ERROR = Add LOG
                        codeReturn = Cst.ErrLevel.FAILURE;
                        break;
                }
            }
            return codeReturn;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected override QueryParameters GetQueryParameters()
        {
            QueryParameters ret;
            switch (BuildingInfo.processType)
            {
                case Cst.ProcessTypeEnum.FEESCALCULATION:
                    ret = GetQueryParametersFeesCalculation();
                    break;
                default:
                    throw new NotImplementedException(StrFunc.AppendFormat("processType (id:{0}) is not implemented", BuildingInfo.processType));
            }

            return ret;

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pDr"></param>
        /// <returns></returns>
        protected override MQueueBase BuildMessageQueue(DataRow pDr)
        {
            MQueueBase ret;
            switch (BuildingInfo.processType)
            {
                case Cst.ProcessTypeEnum.FEESCALCULATION:
                    ret = BuildMessageQueueFeesCalculation(pDr);
                    break;
                default:
                    throw new NotImplementedException(StrFunc.AppendFormat("processType (id:{0}) is not implemented", BuildingInfo.processType));
            }
            return ret;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        // EG 20191115 [25077] RDBMS : New version of Trades tables architecture (TRADESTSYS merge to TRADE, NEW TABLE TRADEXML)
        private QueryParameters GetQueryParametersFeesCalculation()
        {
            DataParameters dp = CreateDataParameters();
            // FI 20180328 [23871] Nouvelle requête
            string query = @"select 
          result.IDA_ENTITY, a_entity.IDENTIFIER as ENTITY_IDENTIFIER, a_entity.DISPLAYNAME as ENTITY_DISPLAYNAME, 
          result.IDA_CSSCUSTODIAN, csscust.IDENTIFIER as CSSCUSTODIAN_IDENTIFIER, 
          m.SHORT_ACRONYM as MARKET, result.DTENTITY, result.IDEM, result.IDI, result.IDASSET, result.ASSETCATEGORY, result.IDM,
          result.IDA_DEALER, result.IDB_DEALER, result.IDA_CLEARER, result.IDB_CLEARER, result.IDA_ENTITYCLEARER, 
          /*-- TRADE --*/
          result.IDT, result.IDENTIFIER, result.DTBUSINESS, result.DTEXECUTION, result.TZFACILITY, result.PRICE,
          result.QTY, result.SIDE, result.POSITIONEFFECT, result.EXECUTIONID, result.ORDERID, result.ORDERTYPE,
          /*--DEALER--*/
          dealer.IDENTIFIER as DEALER_IDENTIFIER,dealer.DISPLAYNAME as DEALER_DISPLAYNAME,
          bdealer.IDENTIFIER as DEALER_BOOKIDENTIFIER,bdealer.ISPOSKEEPING,
          /*--CLEARER--*/
          clearer.IDENTIFIER as CLEARER_IDENTIFIER, clearer.DISPLAYNAME as CLEARER_DISPLAYNAME,
          bclearer.IDENTIFIER as CLEARER_BOOKIDENTIFIER,
          /*--TRADE IDENTIFIER--*/
          null as TRADE_IDENTIFIER,
          /*--ASSET--*/          
          asset.ASSETCATEGORY as CATEGORY, asset.IDENTIFIER as ASSET_IDENTIFIER, asset.CHARACTERISTICS,
          /*--GPRODUCT--*/          
          result.GPRODUCT
          from
          (
	          <choose>
              <when test=""{FEESCALCULATIONMODE}='ALL' or {FEESCALCULATIONMODE}='STL'"">
                  select tr.IDM, tr.DTENTITY, tr.DTBUSINESS, tr.IDI, tr.IDASSET, tr.IDEM, tr.IDA_CSSCUSTODIAN, tr.IDA_ENTITYDEALER as IDA_ENTITY,
                  tr.IDA_DEALER, tr.IDB_DEALER, tr.IDA_CLEARER, tr.IDB_CLEARER, tr.IDA_ENTITYCLEARER, tr.ASSETCATEGORY,
                  tr.IDT, tr.IDENTIFIER, tr.GPRODUCT,
                  tr.DTEXECUTION, tr.TZFACILITY, tr.PRICE,
                  tr.QTY, tr.SIDE, tr.POSITIONEFFECT, tr.EXECUTIONID, tr.ORDERID, tr.ORDERTYPE
                  from dbo.VW_TRADE_ALLOC tr
                  where
                  (
                   (tr.DTBUSINESS = tr.DTENTITY) and (tr.DTENTITY between @DATE1 and @DATE2)
                   and (isnull(tr.TRDTYPE,0) not in (%%R:EXCLUDEDVALUESFORFEES%%))
                    <choose>
                        <when test=""{ENTITY}>-1""> and tr.IDA_ENTITY=@ENTITY </when>
                    </choose>
                    <choose>
                        <when test=""{CSSCUSTODIAN}>0""> and tr.IDA_CSSCUSTODIAN=@CSSCUSTODIAN </when>
                        <when test=""{CSSCUSTODIAN}=-1""> and (1=1) </when>
                        <otherwise> and tr.CSSCUSTODIAN=@CSSCUSTODIAN </otherwise>
                    </choose>
                  )
              </when>
            </choose>
            <choose>
              <when test=""{FEESCALCULATIONMODE}='ALL'"">
                union
              </when>
            </choose>  
            <choose>
              <when test=""{FEESCALCULATIONMODE}='ALL' or {FEESCALCULATIONMODE}='INV'"">
                  select tr.IDM, tr.DTENTITY, tr.DTBUSINESS, tr.IDI, tr.IDASSET, tr.IDEM, tr.IDA_CSSCUSTODIAN, tr.IDA_ENTITYDEALER as IDA_ENTITY,
                  tr.IDA_DEALER, tr.IDB_DEALER, tr.IDA_CLEARER, tr.IDB_CLEARER, tr.IDA_ENTITYCLEARER, tr.ASSETCATEGORY,
                  tr.IDT, tr.IDENTIFIER, tr.GPRODUCT,
                  tr.DTEXECUTION, tr.TZFACILITY, tr.PRICE,
                  tr.QTY, tr.SIDE, tr.POSITIONEFFECT, tr.EXECUTIONID, tr.ORDERID, tr.ORDERTYPE
                  from dbo.VW_TRADE_ALLOC tr
                  where
                  (
                   (tr.DTBUSINESS between @DATE1 and @DATE2)
                   and (isnull(tr.TRDTYPE,0) not in (%%R:EXCLUDEDVALUESFORFEES%%))
                   and not exists (select 1 from dbo.TRADELINK tl
				                                     inner join dbo.TRADE tr_A on  (tr_A.IDT = tl.IDT_A) and (tr_A.IDSTACTIVATION != 'DEACTIV')
				                                     where tl.IDT_B = tr.IDT and  tl.LINK = 'Invoice')
                    <choose>
                        <when test=""{ENTITY}>-1""> and tr.IDA_ENTITY=@ENTITY </when>
                    </choose>
                    <choose>
                        <when test=""{CSSCUSTODIAN}>0""> and tr.IDA_CSSCUSTODIAN=@CSSCUSTODIAN </when>
                        <when test=""{CSSCUSTODIAN}=-1""> and (1=1) </when>
                        <otherwise> and tr.CSSCUSTODIAN=@CSSCUSTODIAN </otherwise>
                    </choose>
                  )
              </when>
            </choose>  
          ) result         
          inner join dbo.VW_MARKET_IDENTIFIER m  on (m.IDM=result.IDM)        
          inner join dbo.ACTOR dealer            on (dealer.IDA = result.IDA_DEALER)
          inner join dbo.BOOK  bdealer           on (bdealer.IDB = result.IDB_DEALER)
          inner join dbo.ACTOR clearer           on (clearer.IDA = result.IDA_CLEARER)
          inner join dbo.BOOK  bclearer          on (bclearer.IDB = result.IDB_CLEARER)
          inner join dbo.VW_ASSET_CONSULT asset  on (asset.IDASSET = result.IDASSET) and (asset.ASSETCATEGORY = result.ASSETCATEGORY)
          inner join dbo.ACTOR a_entity          on (a_entity.IDA=result.IDA_ENTITY)
          inner join dbo.ACTOR csscust           on (csscust.IDA=result.IDA_CSSCUSTODIAN)";

            MQueueparameters parameters = m_MQueueAttributes.parameters;

            List<StringDynamicData> dynamicArgs = new List<StringDynamicData>
            {
                new StringDynamicData(TypeData.TypeDataEnum.integer.ToString(), "ENTITY", parameters["ENTITY"].Value),
                new StringDynamicData(TypeData.TypeDataEnum.integer.ToString(), "CSSCUSTODIAN", parameters["CSSCUSTODIAN"].Value),
                new StringDynamicData(TypeData.TypeDataEnum.date.ToString(), "DATE1", parameters["DATE1"].Value),
                new StringDynamicData(TypeData.TypeDataEnum.date.ToString(), "DATE2", parameters["DATE2"].Value),
                new StringDynamicData(TypeData.TypeDataEnum.@string.ToString(), "FEESCALCULATIONMODE", parameters["FEESCALCULATIONMODE"].Value)
            };

            TypeBuilder dynamicArgsType = new TypeBuilder(m_NormMsgFactoryProcess.Cs, dynamicArgs, "DynamicData", "ReferentialsReferential");
            query = StrFuncExtended.ReplaceChooseExpression2(query, dynamicArgsType.GetNewObject(), true);
            query = query.Replace(Cst.EXCLUDEDVALUESFORFEES, Cst.TrdType_ExcludedValuesForFees_ETD);

            QueryParameters ret = new QueryParameters(m_NormMsgFactoryProcess.Cs, query, dp);
            return ret;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private DataParameters CreateDataParameters()
        {
            DataParameters dp = new DataParameters();

            if (BuildingInfo.parametersSpecified)
            {
                MQueueparameters parameters = m_MQueueAttributes.parameters;

                dp.Add(new DataParameter(m_NormMsgFactoryProcess.Cs, NormMsgFactoryMQueue.PARAM_ENTITY, DbType.Int32),
                    parameters.GetIntValueParameterById(NormMsgFactoryMQueue.PARAM_ENTITY));

                dp.Add(new DataParameter(m_NormMsgFactoryProcess.Cs, NormMsgFactoryMQueue.PARAM_CSSCUSTODIAN, DbType.Int32),
                    parameters.GetIntValueParameterById(NormMsgFactoryMQueue.PARAM_CSSCUSTODIAN));
                
                // FI 20180328 [23871] Ajout des paramètres DATE1, DATE2, FEESCALCULATIONMODE nécessaires à la requête SQL
                dp.Add(new DataParameter(m_NormMsgFactoryProcess.Cs, NormMsgFactoryMQueue.PARAM_DATE1, DbType.Date), // FI 20201006 [XXXXX] DbType.Date
                    parameters.GetDateTimeValueParameterById(NormMsgFactoryMQueue.PARAM_DATE1));

                dp.Add(new DataParameter(m_NormMsgFactoryProcess.Cs, NormMsgFactoryMQueue.PARAM_DATE2, DbType.Date), // FI 20201006 [XXXXX] DbType.Date
                    parameters.GetDateTimeValueParameterById(NormMsgFactoryMQueue.PARAM_DATE2));

                dp.Add(new DataParameter(m_NormMsgFactoryProcess.Cs, NormMsgFactoryMQueue.PARAM_FEESCALCULATIONMODE, DbType.String),
                    parameters.GetStringValueParameterById(NormMsgFactoryMQueue.PARAM_FEESCALCULATIONMODE));
            }

            return dp;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pDr"></param>
        /// <returns></returns>
        /// FI 20180328 [23871] Mod
        private MQueueBase BuildMessageQueueFeesCalculation(DataRow pDr)
        {

            FeesCalculationSettingsMode1 feesCalculationSetting = new FeesCalculationSettingsMode1
            {
                actionDate = OTCmlHelper.GetDateSys(m_NormMsgFactoryProcess.Cs),
                noteSpecified = true,
                note = "Fees Calculation requested by NormMsgFactory",

                mode = (Cst.FeesCalculationMode) Enum.Parse(typeof(Cst.FeesCalculationMode), m_MQueueAttributes.parameters["FEESCALCULATIONMODE"].Value)
            };
            // Alimentation de feesCalculationSetting.fee, feesCalculationSetting.feeSchedule, feesCalculationSetting.feeMatrix
            // Code présent mais sans utilité puisque les paramètres FEE, FEESCHEDULE, FEEMATRIX ne sont pas disponibles côté NormMasgFactory
            MQueueparameter parameter = m_MQueueAttributes.parameters["FEE"];
            feesCalculationSetting.feeSpecified = (null != parameter) && Convert.ToInt32(parameter.Value) > 0;
            if (feesCalculationSetting.feeSpecified)
            {
                feesCalculationSetting.fee.otcmlId = parameter.Value;
                feesCalculationSetting.fee.identifier = parameter.ExValue;
            }

            parameter = m_MQueueAttributes.parameters["FEESCHEDULE"];
            feesCalculationSetting.feeSheduleSpecified = (null != parameter) && Convert.ToInt32(parameter.Value) > 0;
            if (feesCalculationSetting.feeSheduleSpecified)
            {
                feesCalculationSetting.feeShedule.otcmlId = parameter.Value;
                feesCalculationSetting.feeShedule.identifier = parameter.ExValue;
            }

            parameter = m_MQueueAttributes.parameters["FEEMATRIX"];
            feesCalculationSetting.feeMatrixSpecified = (null != parameter) && Convert.ToInt32(parameter.Value) > 0;
            if (feesCalculationSetting.feeMatrixSpecified)
            {
                feesCalculationSetting.feeMatrix.otcmlId = parameter.Value;
                feesCalculationSetting.feeMatrix.identifier = parameter.ExValue;
            }

            TradeActionGenMQueue queue = new TradeActionGenMQueue(m_MQueueAttributes)
            {
                item = new TradeActionMQueue[] 
                {
                    new TradeActionMQueue
                    {
                        tradeActionCode = TradeActionCode.TradeActionCodeEnum.FeesCalculation,
                        actionMsgs = new FeesCalculationSettingsMode1[1] { feesCalculationSetting }
                    }
                }
            };

            SetDefaultMessageQueue(queue, pDr);

            return queue;
        }

        /// <summary>
        /// Ajoute le paramètre FEESCALCULATIONMODE
        /// </summary>
        /// <returns></returns>
        /// FI 20180328 [23871] Add
        // EG 20190114 Add detail to ProcessLog Refactoring
        private Cst.ErrLevel AddFeesCalculationModeParameter()
        {
            Cst.ErrLevel codeReturn = Cst.ErrLevel.SUCCESS;

            string mode = BuildingInfo.parameters.GetStringValueParameterById(NormMsgFactoryMQueue.PARAM_FEESCALCULATIONMODE);
            if (StrFunc.IsFilled(mode))
            {
                if (false == Enum.IsDefined(typeof(Cst.FeesCalculationMode), mode))
                {
                    // FI 20200623 [XXXXX] SetErrorWarning
                    m_NormMsgFactoryProcess.ProcessState.SetErrorWarning(ProcessStateTools.StatusErrorEnum);

                    Logger.Log(new LoggerData(LogLevelEnum.Error, new SysMsgCode(SysCodeEnum.SYS, 8004), 2,
                        new LogParam(mode + " (" + NormMsgFactoryMQueue.PARAM_FEESCALCULATIONMODE.ToString() + ")"),
                        new LogParam(m_NormMsgFactoryProcess.LogId),
                        new LogParam(m_NormMsgFactoryProcess.LogProcessType)));

                    codeReturn = Cst.ErrLevel.FAILURE;
                }
                else
                {
                    mode = ((Cst.FeesCalculationMode)Enum.Parse(typeof(Cst.FeesCalculationMode), mode)).ToString();
                }
            }
            else
                mode = Cst.FeesCalculationMode.STL.ToString(); //Compatibilité ascendante => Si le paramètre n'existe pas en entrée

            if (Cst.ErrLevel.SUCCESS == codeReturn)
            {
                ResourceAttribut attribut = ReflectionTools.GetAttribute<ResourceAttribut>(typeof(Cst.FeesCalculationMode), mode.ToString());
                string extValue = Ressource.GetString(attribut.Resource);

                MQueueparameter parameter = new MQueueparameter(NormMsgFactoryMQueue.PARAM_FEESCALCULATIONMODE, TypeData.TypeDataEnum.@string);
                parameter.SetValue(mode, extValue);
                AddMQueueAttributesParameter(parameter);
            }

            return codeReturn;
        }
    }
}