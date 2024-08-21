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
using EfsML.Ear;
using System;
using System.Collections.Generic;
using System.Data;
#endregion Using Directives

namespace EFS.Process
{
    #region NormMsgFactory_EAR
    /// <summary>
    /// Construction Messages de type : EarGenMQueue 
    /// 
    /// DONNEES attendues en entrée : 
    ///<para>────────────────────────────────────────────────────────────────────────────────────────────────</para>
    ///<para>  ● processType = EARGEN</para>
    ///<para>────────────────────────────────────────────────────────────────────────────────────────────────</para>
    ///
    /// PARAMETRES attendus en entrée (* = optionnel):
    ///<para>────────────────────────────────────────────────────────────────────────────────────────────────</para>
    ///<para>  ● ENTITY      : Identifiant de l'entité</para>
    ///<para>  ● DTBUSINESS* : Si NON spécifiée = Lecture de ENTITYMARKET pour l'entité demandée</para>
    ///<para>  ● CLASS*      : Type de flux, si spécifié = CASH_FLOWS, CLOSING</para>
    ///<para>                  sinon équivalent à TOUS</para>
    ///<para>  ● REQUESTYPE* : Type de trade, si spécifié = ADMIN, CBI, COMMON</para>
    ///<para>                  sinon équivalent à COMMON</para>
    ///<para>────────────────────────────────────────────────────────────────────────────────────────────────</para> 
    ///</summary>
    public class NormMsgFactory_EAR : NormMsgFactoryBase
    {
        #region Constructors
        public NormMsgFactory_EAR(NormMsgFactoryProcess pNormMsgFactoryProcess)
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
            MQueueBase ret = new EarGenMQueue(m_MQueueAttributes);
            SetDefaultMessageQueue(ret, pDr);
            return ret;
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
                    // Lorsque DTBUSINESS est non renseigné, Spheres® récupère la date courante la plus récente vis à vis de l'entité
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
                    codeReturn = AddFlowTypeParameter();
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
        /// FI 20170531 [23206] Refactoring
        /// FI 20170607 [23221] Modify
        /// FI 20180907 [24160] Mise en place du @STSTART
        // EG 20191115 [25077] RDBMS : New version of Trades tables architecture (TRADESTSYS merge to TRADE, NEW TABLE TRADEXML)
        protected override QueryParameters GetQueryParameters()
        {
            #region SQLPRESELECT
            // FI 20170607 [23221] Prise en considération de GPRODUCT= 'COM'
            string sqlPreselect = @"insert into EARGEN_%%SHORTSESSIONID%%_W (IDT, IDEC, IDE,  RMV_EAR_DAY, RECORDTYPE)
            <choose>
            <when test=""'%%PARAM1%%'='COMMON'"">
              select  t.IDT,ec.IDEC, e.IDE, enumrmv.EAR_DAY_FUT as RMV_EAR_DAY, '0' as RECORDTYPE
              from dbo.TRADE t
              inner join dbo.INSTRUMENT i on i.IDI=t.IDI and i.ISEAR=1
              inner join dbo.PRODUCT p on p.IDP=i.IDP and p.GPRODUCT='FUT'
              inner join dbo.EVENT e on e.IDT=t.IDT
              inner join dbo.EVENTCLASS ec on ec.IDE=e.IDE and ec.DTEVENT between @DTSTART and @DATE1              
              inner join dbo.VW_EAREVENTENUM enumcode on enumcode.VALUE=e.EVENTCODE and enumcode.CODE='EventCode' and enumcode.EAR_DAY_FUT=1
              inner join dbo.VW_EAREVENTENUM enumtype on enumtype.VALUE=e.EVENTTYPE and enumtype.CODE='EventType' and  enumtype.EAR_DAY_FUT=1
              inner join dbo.VW_EAREVENTENUM enumclass on enumclass.VALUE=ec.EVENTCLASS and enumclass.CODE='EventClass' and enumclass.EAR_DAY_FUT=1
              inner join dbo.VW_EAREVENTENUM enumrmv on enumrmv.VALUE='RMV' and enumrmv.CODE='EventClass'
              where (t.IDSTENVIRONMENT='REGULAR') and (t.IDSTACTIVATION='REGULAR')
              <choose>
                <when test=""{CLASS}='CLOSING'""> and e.EVENTCODE='CLO'</when> 
                <when test=""{CLASS}='CASH_FLOWS'""> and e.EVENTCODE!='CLO'</when> 
              </choose>
 
              union all

              select  t.IDT,ec.IDEC, e.IDE, enumrmv.EAR_DAY_COM as RMV_EAR_DAY, '0' as RECORDTYPE
              from dbo.TRADE t
              inner join dbo.INSTRUMENT i on i.IDI=t.IDI and i.ISEAR=1
              inner join dbo.PRODUCT p on p.IDP=i.IDP and p.GPRODUCT='COM'
              inner join dbo.EVENT e on e.IDT=t.IDT
              inner join dbo.EVENTCLASS ec on ec.IDE=e.IDE and ec.DTEVENT between @DTSTART and @DATE1              
              inner join dbo.VW_EAREVENTENUM enumcode on enumcode.VALUE=e.EVENTCODE and enumcode.CODE='EventCode' and enumcode.EAR_DAY_COM=1
              inner join dbo.VW_EAREVENTENUM enumtype on enumtype.VALUE=e.EVENTTYPE and enumtype.CODE='EventType' and  enumtype.EAR_DAY_COM=1
              inner join dbo.VW_EAREVENTENUM enumclass on enumclass.VALUE=ec.EVENTCLASS and enumclass.CODE='EventClass' and enumclass.EAR_DAY_COM=1
              inner join dbo.VW_EAREVENTENUM enumrmv on enumrmv.VALUE='RMV' and enumrmv.CODE='EventClass'
              where (t.IDSTENVIRONMENT='REGULAR') and (t.IDSTACTIVATION='REGULAR')
              <choose>
                <when test=""{CLASS}='CLOSING'""> and e.EVENTCODE='CLO'</when> 
                <when test=""{CLASS}='CASH_FLOWS'""> and e.EVENTCODE!='CLO'</when> 
              </choose> 

              union all

              select t.IDT, ec.IDEC, e.IDE, enumrmv.EAR_DAY_FX as RMV_EAR_DAY, '0' as RECORDTYPE
              from dbo.TRADE t
              inner join dbo.INSTRUMENT i on i.IDI=t.IDI and i.ISEAR=1
              inner join dbo.PRODUCT p on p.IDP=i.IDP and p.GPRODUCT='FX'
              inner join dbo.EVENT e on e.IDT=t.IDT
              inner join dbo.EVENTCLASS ec on ec.IDE=e.IDE and ec.DTEVENT between @DTSTART and @DATE1               
              inner join dbo.VW_EAREVENTENUM enumcode on enumcode.VALUE=e.EVENTCODE and enumcode.CODE='EventCode' and enumcode.EAR_DAY_FX=1
              inner join dbo.VW_EAREVENTENUM enumtype on enumtype.VALUE=e.EVENTTYPE and enumtype.CODE='EventType' and  enumtype.EAR_DAY_FX=1
              inner join dbo.VW_EAREVENTENUM enumclass on enumclass.VALUE=ec.EVENTCLASS and enumclass.CODE='EventClass' and enumclass.EAR_DAY_FX=1
              inner join dbo.VW_EAREVENTENUM enumrmv on enumrmv.VALUE='RMV' and enumrmv.CODE='EventClass'
              where (t.IDSTENVIRONMENT='REGULAR') and (t.IDSTACTIVATION='REGULAR')
              <choose>
                <when test=""{CLASS}='CLOSING'""> and e.EVENTCODE='CLO'</when> 
                <when test=""{CLASS}='CASH_FLOWS'""> and e.EVENTCODE!='CLO'</when> 
              </choose> 

              union all

              select t.IDT, ec.IDEC, e.IDE, enumrmv.EAR_DAY_OTC as RMV_EAR_DAY, '0' as RECORDTYPE
              from dbo.TRADE t
              inner join dbo.INSTRUMENT i on i.IDI=t.IDI and i.ISEAR=1
              inner join dbo.PRODUCT p on p.IDP=i.IDP and p.GPRODUCT='OTC'
              inner join dbo.EVENT e on e.IDT=t.IDT
              inner join dbo.EVENTCLASS ec on ec.IDE=e.IDE and ec.DTEVENT between @DTSTART and @DATE1
              inner join dbo.VW_EAREVENTENUM enumcode on enumcode.VALUE=e.EVENTCODE and enumcode.CODE='EventCode' and enumcode.EAR_DAY_OTC=1 
              inner join dbo.VW_EAREVENTENUM enumtype on enumtype.VALUE=e.EVENTTYPE and enumtype.CODE='EventType' and  enumtype.EAR_DAY_OTC=1
              inner join dbo.VW_EAREVENTENUM enumclass on enumclass.VALUE=ec.EVENTCLASS and enumclass.CODE='EventClass' and enumclass.EAR_DAY_OTC=1
              inner join dbo.VW_EAREVENTENUM enumrmv on enumrmv.VALUE='RMV' and enumrmv.CODE='EventClass'
              where (t.IDSTENVIRONMENT='REGULAR') and (t.IDSTACTIVATION='REGULAR')
              <choose>
                <when test=""{CLASS}='CLOSING'""> and e.EVENTCODE='CLO'</when> 
                <when test=""{CLASS}='CASH_FLOWS'""> and e.EVENTCODE!='CLO'</when> 
              </choose> 

              union all

              select t.IDT, ec.IDEC, e.IDE, enumrmv.EAR_DAY_SEC as RMV_EAR_DAY, '0' as RECORDTYPE
              from dbo.TRADE t
              inner join dbo.INSTRUMENT i on i.IDI=t.IDI and i.ISEAR=1
              inner join dbo.PRODUCT p on p.IDP=i.IDP and p.GPRODUCT='SEC'
              inner join dbo.EVENT e on e.IDT=t.IDT
              inner join dbo.EVENTCLASS ec on ec.IDE=e.IDE and ec.DTEVENT between @DTSTART and @DATE1               
              inner join dbo.VW_EAREVENTENUM enumcode on enumcode.VALUE=e.EVENTCODE and enumcode.CODE='EventCode' and enumcode.EAR_DAY_SEC=1 
              inner join dbo.VW_EAREVENTENUM enumtype on enumtype.VALUE=e.EVENTTYPE and enumtype.CODE='EventType' and  enumtype.EAR_DAY_SEC=1
              inner join dbo.VW_EAREVENTENUM enumclass on enumclass.VALUE=ec.EVENTCLASS and enumclass.CODE='EventClass' and enumclass.EAR_DAY_SEC=1
              inner join dbo.VW_EAREVENTENUM enumrmv on enumrmv.VALUE='RMV' and enumrmv.CODE='EventClass'
              where (t.IDSTENVIRONMENT='REGULAR') and (t.IDSTACTIVATION='REGULAR')
              <choose>
                <when test=""{CLASS}='CLOSING'""> and e.EVENTCODE='CLO'</when> 
                <when test=""{CLASS}='CASH_FLOWS'""> and e.EVENTCODE!='CLO'</when> 
              </choose> 

              union all

              select t.IDT, ec.IDEC, e.IDE, enumrmv.EAR_DAY_RISK as RMV_EAR_DAY, '0' as RECORDTYPE
              from TRADE t
              inner join dbo.INSTRUMENT i on i.IDI=t.IDI and i.ISEAR=1
              inner join dbo.PRODUCT p on p.IDP=i.IDP and p.GPRODUCT='RISK' and p.IDENTIFIER in ('cashBalance','cashPayment')
              inner join dbo.EVENT e on e.IDT=t.IDT
              inner join dbo.EVENTCLASS ec on ec.IDE=e.IDE and ec.DTEVENT between @DTSTART and @DATE1
              inner join dbo.VW_EAREVENTENUM enumcode on enumcode.VALUE=e.EVENTCODE and enumcode.CODE='EventCode' and enumcode.EAR_DAY_RISK=1
              inner join dbo.VW_EAREVENTENUM enumtype on enumtype.VALUE=e.EVENTTYPE and enumtype.CODE='EventType' and  enumtype.EAR_DAY_RISK=1
              inner join dbo.VW_EAREVENTENUM enumclass on enumclass.VALUE=ec.EVENTCLASS and enumclass.CODE='EventClass' and enumclass.EAR_DAY_RISK=1
              inner join dbo.VW_EAREVENTENUM enumrmv on enumrmv.VALUE='RMV' and enumrmv.CODE='EventClass'
              where (t.IDSTENVIRONMENT='REGULAR') and (t.IDSTACTIVATION='REGULAR') and not (e.EVENTCODE='OPP' and e.EVENTTYPE='CSH')   
              <choose>
                <when test=""{CLASS}='CLOSING'""> and e.EVENTCODE='CLO'</when> 
                <when test=""{CLASS}='CASH_FLOWS'""> and e.EVENTCODE!='CLO'</when> 
              </choose>  

              union all

              select t.IDT, ec.IDEC, e.IDE, 0 as RMV_EAR_DAY, '1' as RECORDTYPE 
              from dbo.TRADE t
              inner join dbo.INSTRUMENT i on i.IDI=t.IDI and i.ISEAR=1
              inner join dbo.PRODUCT p on p.IDP=i.IDP and (p.GPRODUCT not in ('ADM','ASSET','RISK') or p.IDENTIFIER in ('cashBalance','cashPayment'))
              inner join dbo.EVENT e on e.IDT=t.IDT and e.EVENTCODE='RMV' and e.EVENTTYPE='DAT'
              inner join dbo.EVENTCLASS ec on ec.IDE=e.IDE and ec.EVENTCLASS='GRP' and ec.DTEVENT between @DTSTART and @DATE1 
              where (t.IDSTENVIRONMENT='REGULAR')
              <choose>
                <when test=""{CLASS}='CLOSING'""> and e.EVENTCODE='CLO'</when> 
                <when test=""{CLASS}='CASH_FLOWS'""> and e.EVENTCODE!='CLO'</when> 
              </choose> 
            </when> 
            <when test=""'%%PARAM1%%'='ADMIN'"">
                select  t.IDT,ec.IDEC, e.IDE, enumrmv.EAR_DAY_ADM as RMV_EAR_DAY, '0' as RECORDTYPE
                from dbo.TRADE t
                inner join dbo.INSTRUMENT i on i.IDI=t.IDI and i.ISEAR=1
                inner join dbo.PRODUCT p on p.IDP=i.IDP and p.GPRODUCT='ADM'
                inner join dbo.EVENT e on e.IDT=t.IDT
                inner join dbo.EVENTCLASS ec on ec.IDE=e.IDE and ec.DTEVENT between @DTSTART and @DATE1               
                inner join dbo.VW_EAREVENTENUM enumcode on enumcode.VALUE=e.EVENTCODE and enumcode.CODE='EventCode' and enumcode.EAR_DAY_ADM=1
                inner join dbo.VW_EAREVENTENUM enumtype on enumtype.VALUE=e.EVENTTYPE and enumtype.CODE='EventType' and  enumtype.EAR_DAY_ADM=1
                inner join dbo.VW_EAREVENTENUM enumclass on enumclass.VALUE=ec.EVENTCLASS and enumclass.CODE='EventClass' and enumclass.EAR_DAY_ADM=1
                inner join dbo.VW_EAREVENTENUM enumrmv on enumrmv.VALUE='RMV' and enumrmv.CODE='EventClass'
                where (t.IDSTENVIRONMENT='REGULAR') and (t.IDSTACTIVATION='REGULAR')
                <choose>
                  <when test=""{CLASS}='CLOSING'""> and e.EVENTCODE='CLO'</when> 
                  <when test=""{CLASS}='CASH_FLOWS'""> and e.EVENTCODE!='CLO'</when> 
                </choose> 

                union all

                select t.IDT, ec.IDEC, e.IDE, 0 as RMV_EAR_DAY, '1' as RECORDTYPE 
                from dbo.TRADE t
                inner join dbo.INSTRUMENT i on i.IDI=t.IDI and i.ISEAR=1
                inner join dbo.PRODUCT p on p.IDP=i.IDP and p.GPRODUCT='ADM'
                inner join dbo.EVENT e on e.IDT=t.IDT and e.EVENTCODE='RMV' and e.EVENTTYPE='DAT'
                inner join dbo.EVENTCLASS ec on ec.IDE=e.IDE and ec.EVENTCLASS='GRP' and ec.DTEVENT between @DTSTART and @DATE1 
                where (t.IDSTENVIRONMENT='REGULAR')
                <choose>
                  <when test=""{CLASS}='CLOSING'""> and e.EVENTCODE='CLO'</when> 
                  <when test=""{CLASS}='CASH_FLOWS'""> and e.EVENTCODE!='CLO'</when> 
                </choose> 
            </when> 
            <when test=""'%%PARAM1%%'='CBI'"">
                select  t.IDT,ec.IDEC, e.IDE, enumrmv.EAR_DAY_RISK as RMV_EAR_DAY, '0' as RECORDTYPE
                from dbo.TRADE t
                inner join dbo.INSTRUMENT i on i.IDI=t.IDI and i.ISEAR=1
                inner join dbo.PRODUCT p on p.IDP=i.IDP and p.IDENTIFIER='cashBalanceInterest'
                inner join dbo.EVENT e on e.IDT=t.IDT
                inner join dbo.EVENTCLASS ec on ec.IDE=e.IDE and ec.DTEVENT between @DTSTART and @DATE1               
                inner join dbo.VW_EAREVENTENUM enumcode on enumcode.VALUE=e.EVENTCODE and enumcode.CODE='EventCode' and enumcode.EAR_DAY_RISK=1
                inner join dbo.VW_EAREVENTENUM enumtype on enumtype.VALUE=e.EVENTTYPE and enumtype.CODE='EventType' and  enumtype.EAR_DAY_RISK=1
                inner join dbo.VW_EAREVENTENUM enumclass on enumclass.VALUE=ec.EVENTCLASS and enumclass.CODE='EventClass' and enumclass.EAR_DAY_RISK=1
                inner join dbo.VW_EAREVENTENUM enumrmv on enumrmv.VALUE='RMV' and enumrmv.CODE='EventClass'
                where (t.IDSTENVIRONMENT='REGULAR') and (t.IDSTACTIVATION='REGULAR')
                <choose>
                  <when test=""{CLASS}='CLOSING'""> and e.EVENTCODE='CLO'</when> 
                  <when test=""{CLASS}='CASH_FLOWS'""> and e.EVENTCODE!='CLO'</when> 
                </choose> 

                union all

                select t.IDT, ec.IDEC, e.IDE, 0 as RMV_EAR_DAY, '1' as RECORDTYPE 
                from dbo.TRADE t
                inner join dbo.INSTRUMENT i on i.IDI=t.IDI and i.ISEAR=1
                inner join dbo.PRODUCT p on p.IDP=i.IDP and p.IDENTIFIER='cashBalanceInterest'
                inner join dbo.EVENT e on e.IDT=t.IDT and e.EVENTCODE='RMV' and e.EVENTTYPE='DAT'
                inner join dbo.EVENTCLASS ec on ec.IDE=e.IDE and ec.EVENTCLASS='GRP' and ec.DTEVENT between @DTSTART and @DATE1 
                where (t.IDSTENVIRONMENT='REGULAR')
                <choose>
                  <when test=""{CLASS}='CLOSING'""> and e.EVENTCODE='CLO'</when> 
                  <when test=""{CLASS}='CASH_FLOWS'""> and e.EVENTCODE!='CLO'</when> 
                </choose> 
            </when> 
         </choose>";
            #endregion SQLPRESELECT

            #region SQLSELECT
            string sqlSelect = @"select 
            t.IDT, t.IDENTIFIER, t.DISPLAYNAME, t.DESCRIPTION, t.IDI, p.GPRODUCT, t.DTTRADE, t.DTSYS,
            t.IDSTENVIRONMENT, t.IDSTBUSINESS, t.IDSTACTIVATION, t.IDSTPRIORITY, t.SOURCE, t.EXTLLINK, 1 as ISSELECTED
            from
            (
                /* event jour non annulé => l'EAR du jour sera (re)généré */
                select tblmain.IDT
                from EARGEN_%%SHORTSESSIONID%%_W tblmain
                inner join dbo.EVENT e on e.IDE=tblmain.IDE
                inner join dbo.EVENTCLASS ec on ec.IDEC=tblmain.IDEC
                where 
                tblmain.RECORDTYPE='0' and ec.DTEVENT=@DATE1 and e.IDSTACTIVATION='REGULAR'             

                union 

                /* event jour annulé avec EAR => l'EAR du jour sera supprimé et ne sera pas re-généré */
                select tblmain.IDT
                from EARGEN_%%SHORTSESSIONID%%_W tblmain
                inner join dbo.EVENT e on e.IDE=tblmain.IDE 
                inner join dbo.EVENTCLASS ec on ec.IDEC=tblmain.IDEC 
                where tblmain.RECORDTYPE='0' and ec.DTEVENT=@DATE1 and e.IDSTACTIVATION='DEACTIV'
                and exists 
                (
                    select 1 
                    from dbo.EARDAY ed11 
                    inner join dbo.EAR ear11 on ear11.IDEAR=ed11.IDEAR
                    where ear11.IDT=tblmain.IDT and ed11.IDEC=tblmain.IDEC
                )

                union

                /* event du passé non annulé sans EAR */
                select tblmain.IDT
                from EARGEN_%%SHORTSESSIONID%%_W tblmain
                inner join dbo.EVENT e on e.IDE=tblmain.IDE 
                inner join dbo.EVENTCLASS ec on ec.IDEC=tblmain.IDEC 
                where tblmain.RECORDTYPE='0' and ec.DTEVENT<@DATE1 and e.IDSTACTIVATION='REGULAR'
                and not exists 
                (
                    select 1 
                    from dbo.EARDAY ed21 
                    inner join dbo.EAR ear21 on ear21.IDEAR=ed21.IDEAR
                    where ear21.IDT=tblmain.IDT and ed21.IDEC=tblmain.IDEC and ear21.IDSTACTIVATION='REGULAR'
                )

                union

                /* event du passé annulé */
                select tblmain.IDT
                from EARGEN_%%SHORTSESSIONID%%_W tblmain
                inner join dbo.EVENT e on e.IDE=tblmain.IDE 
                inner join dbo.EVENTCLASS ec on ec.IDEC=tblmain.IDEC 
                where tblmain.RECORDTYPE='0' and ec.DTEVENT<@DATE1 and e.IDSTACTIVATION='DEACTIV' 
                /* mode d'annulation native (autre que l'EventClass d'annulation) */
                and tblmain.RMV_EAR_DAY= 0
                /* avec un EAR NON annulé généré */
                and exists 
                (
                    select 1 
                    from dbo.EARDAY ed31 
                    inner join dbo.EAR ear31 on ear31.IDEAR=ed31.IDEAR
                    where ear31.IDT=tblmain.IDT and ed31.IDEC=ec.IDEC and ear31.IDSTACTIVATION='REGULAR'
                )
                /* annulation non traitée nativement */
                and not exists 
                (
                    select 1 
                    from dbo.EARDAY ed32 
                    inner join dbo.EAR ear32 on ear32.IDEAR=ed32.IDEAR 
                    where ear32.IDT=tblmain.IDT and ed32.IDEC=ec.IDEC and ear32.IDSTACTIVATION='REMOVED' and ear32.DTREMOVED is not null
                )
                /* annulation non traitée avec le mode externe */
                and not exists 
                (
                    select 1 
                    from dbo.EARDAY ed33 
                    inner join dbo.EAR ear33 on ear33.IDEAR=ed33.IDEAR
                    inner join dbo.EVENTCLASS ecrmv33 on ecrmv33.EVENTCLASS='RMV'
                    where ear33.IDT=tblmain.IDT and ecrmv33.IDE=ec.IDE and ed33.IDEC=ecrmv33.IDEC
                )

                union

                /* event du passé annulé */
                select tblmain.IDT 
                from EARGEN_%%SHORTSESSIONID%%_W tblmain
                inner join dbo.EVENT e on e.IDE=tblmain.IDE 
                inner join dbo.EVENTCLASS ec on ec.IDEC=tblmain.IDEC 
                where tblmain.RECORDTYPE = '0' and ec.DTEVENT<@DATE1
                /* EventClass d'annulation, donc event annulé sans gestion native de l'annulation des évenements */
                and ec.EVENTCLASS='RMV'
                /* EARDAY frères générés */
                and exists 
                (
                    select 1 
                    from dbo.EARDAY ed41 
                    inner join dbo.EAR ear41 on ear41.IDEAR=ed41.IDEAR
                    inner join dbo.EVENTCLASS ec41 on ec41.EVENTCLASS!='RMV'
                    where ec41.IDE=ec.IDE and ear41.IDT=tblmain.IDT and ed41.IDEC=ec41.IDEC
                )
                /* sans d'EAR généré, donc annulation non traitée avec le mode externe */
                and not exists 
                (
                    select 1 
                    from dbo.EARDAY ed42 
                    inner join dbo.EAR ear42 on ear42.IDEAR=ed42.IDEAR
                    where ear42.IDT=tblmain.IDT and ear42.IDSTACTIVATION='REGULAR' and ed42.IDEC=ec.IDEC
                )
                /* Annulation non traitée nativement */
                and not exists 
                (
                    select 1 
                    from dbo.EARDAY ed43 
                    inner join dbo.EAR ear43 on ear43.IDEAR=ed43.IDEAR
                    inner join dbo.EVENTCLASS ec43 on ec43.EVENTCLASS!='RMV'
                    where ec43.IDE=ec.IDE and ear43.IDT=tblmain.IDT and ear43.IDSTACTIVATION='REMOVED' and ear43.DTREMOVED is not null and ed43.IDEC=ec43.IDEC
                )

                union

                /*event d'annulation (RMV/DAT/GRP) du jour et passés NON traités */
                select tblmain.IDT
                from EARGEN_%%SHORTSESSIONID%%_W tblmain
                where tblmain.RECORDTYPE = '1' 
                /* NON traités */
                and exists                    
                (
                    select 1 
                    from dbo.EAR ear
                    where ear.IDT=tblmain.IDT and ear.DTEARCANCEL is null
                ) 
            ) tblmain
            inner join dbo.TRADE t on t.IDT=tblmain.IDT
            inner join dbo.INSTRUMENT i on i.IDI=t.IDI
            inner join dbo.PRODUCT p on p.IDP=i.IDP
            <choose>
                <when test=""{ENTITY}>-1"">
                    inner join dbo.TRADEACTOR tabuyer on tabuyer.IDT=tblmain.IDT and tabuyer.IDROLEACTOR='COUNTERPARTY'  and tabuyer.BUYER_SELLER='Buyer'
                    inner join dbo.TRADEACTOR taseller on taseller.IDT=tblmain.IDT and taseller.IDROLEACTOR ='COUNTERPARTY' and taseller.BUYER_SELLER='Seller'
                    left outer join dbo.BOOK bbuyer on bbuyer.IDB=tabuyer.IDB
                    left outer join dbo.BOOK bseller on bseller.IDB=taseller.IDB
                </when>
            </choose>  
            <choose>
            <when test=""{ENTITY}>-1"">
                and (bbuyer.IDA_ENTITY=@ENTITY or bseller.IDA_ENTITY=@ENTITY)
            </when>
            </choose>";
            #endregion SQLSELECT

            string requestType = m_MQueueAttributes.parameters.GetStringValueParameterById(NormMsgFactoryMQueue.PARAM_REQUESTTYPE);
            sqlPreselect = sqlPreselect.Replace("%%PARAM1%%", requestType);

            string buildTableId = this.m_NormMsgFactoryProcess.Session.BuildTableId();
            string tableName = StrFunc.AppendFormat("EARGEN_{0}_W", buildTableId).ToUpper();
            sqlPreselect = sqlPreselect.Replace("EARGEN_%%SHORTSESSIONID%%_W", tableName);
            sqlSelect = sqlSelect.Replace("EARGEN_%%SHORTSESSIONID%%_W", tableName);

            MQueueparameters parameters = m_MQueueAttributes.parameters;
            List<StringDynamicData> dynamicArgs = new List<StringDynamicData>
            {
                new StringDynamicData(TypeData.TypeDataEnum.integer.ToString(), "ENTITY",
                parameters[NormMsgFactoryMQueue.PARAM_ENTITY].Value),
                new StringDynamicData(TypeData.TypeDataEnum.date.ToString(), "DATE1",
                parameters[NormMsgFactoryMQueue.PARAM_DATE1].Value),
                new StringDynamicData(TypeData.TypeDataEnum.@string.ToString(), "CLASS",
                parameters[NormMsgFactoryMQueue.PARAM_CLASS] != null ?
                parameters[NormMsgFactoryMQueue.PARAM_CLASS].ToString() : Cst.DDLVALUE_ALL)
            };

            // FI 20180907 [24160] Mise en place du @DTSTART
            DateTime date1 = new DtFunc().StringDateISOToDateTime(parameters[NormMsgFactoryMQueue.PARAM_DATE1].Value);
            DateTime dtStart = EARTools.LaunchProcessGetStartDate(m_NormMsgFactoryProcess.Cs, date1);
            dynamicArgs.Add(new StringDynamicData(TypeData.TypeDataEnum.date.ToString(), "DTSTART", DtFunc.DateTimeToStringDateISO(dtStart)));

            TypeBuilder dynamicArgsType = new TypeBuilder(m_NormMsgFactoryProcess.Cs, dynamicArgs, "DynamicData", "ReferentialsReferential");
            sqlPreselect = StrFuncExtended.ReplaceChooseExpression2(sqlPreselect, dynamicArgsType.GetNewObject(), true);
            sqlSelect = StrFuncExtended.ReplaceChooseExpression2(sqlSelect, dynamicArgsType.GetNewObject(), true);

            //Creation Table temporaire 
            if (DataHelper.IsExistTable(m_NormMsgFactoryProcess.Cs, tableName))
                DataHelper.ExecuteNonQuery(m_NormMsgFactoryProcess.Cs, CommandType.Text, StrFunc.AppendFormat("drop table {0} purge", tableName));

            DataHelper.CreateTableAsSelect(m_NormMsgFactoryProcess.Cs, "EARGEN_MODEL", tableName);

            //Execution de la requête PRESELECT => Alimentation de la table temporaire 
            DataParameters dp = new DataParameters();
            dp.Add(new DataParameter(m_NormMsgFactoryProcess.Cs, NormMsgFactoryMQueue.PARAM_ENTITY, DbType.Int32),
                m_MQueueAttributes.parameters.GetIntValueParameterById(NormMsgFactoryMQueue.PARAM_ENTITY));

            dp.Add(new DataParameter(m_NormMsgFactoryProcess.Cs, NormMsgFactoryMQueue.PARAM_DATE1, DbType.Date),
                m_MQueueAttributes.parameters.GetDateTimeValueParameterById(NormMsgFactoryMQueue.PARAM_DATE1));

            dp.Add(new DataParameter(m_NormMsgFactoryProcess.Cs, NormMsgFactoryMQueue.PARAM_CLASS, DbType.String));
            if (null != BuildingInfo.parameters.GetObjectValueParameterById(NormMsgFactoryMQueue.PARAM_CLASS))
                dp[NormMsgFactoryMQueue.PARAM_CLASS].Value =
                    m_MQueueAttributes.parameters.GetStringValueParameterById(NormMsgFactoryMQueue.PARAM_CLASS);
            else
                dp[NormMsgFactoryMQueue.PARAM_CLASS].Value = Cst.DDLVALUE_ALL;
            
            // FI 20180907 [24160] Mise en place du @DTSTART
            dp.Add(new DataParameter(m_NormMsgFactoryProcess.Cs, "DTSTART", DbType.Date), dtStart);

            QueryParameters qryPreselect = new QueryParameters(m_NormMsgFactoryProcess.Cs, sqlPreselect.ToString(), dp);

            //+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-
            // PL 20200710 Mise en place d'une transaction, pour harmonisation du code avec l'usage de CSTools.SetMaxTimeOut() 
            //             et ainsi éviter l'éventuelle ouverture d'une nouvelle connexion du fait d'une CS différente (présence d'un TimeOut de 300)
            //+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-
            //string cs = CSTools.SetMaxTimeOut(m_NormMsgFactoryProcess.Cs, 300);
            //DataHelper.ExecuteNonQuery(cs, CommandType.Text, qryPreselect.query, qryPreselect.parameters.GetArrayDbParameter());
            IDbTransaction dbTransaction = null;
            try
            {
                string cs = CSTools.SetMaxTimeOut(m_NormMsgFactoryProcess.Cs, 300);
                dbTransaction = DataHelper.BeginTran(cs);
                DataHelper.ExecuteNonQuery(cs, dbTransaction, CommandType.Text, qryPreselect.Query, qryPreselect.Parameters.GetArrayDbParameter());
                DataHelper.CommitTran(dbTransaction);
            }
            catch (Exception)
            {
                if (null != dbTransaction)
                    DataHelper.RollbackTran(dbTransaction);
                throw;
            }
            finally
            {
                if (null != dbTransaction)
                    dbTransaction.Dispose();
            }
            //+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-

            /* Constitution de la requête finale */
            QueryParameters ret = new QueryParameters(m_NormMsgFactoryProcess.Cs, sqlSelect.ToString(), dp);

            return ret;
        }

        #endregion Methods
    }
    #endregion NormMsgFactory_EAR
}