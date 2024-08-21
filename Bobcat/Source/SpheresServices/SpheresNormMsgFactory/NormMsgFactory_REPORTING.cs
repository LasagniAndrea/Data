#region Using Directives
using EFS.ACommon;
using EFS.ApplicationBlocks.Data;
using EFS.Common.MQueue;
using EFS.LoggerClient;
using EFS.LoggerClient.LoggerService;
using EfsML.Business;
using EfsML.Enum;
using System;
using System.Data;
#endregion Using Directives

namespace EFS.Process
{

    /// <summary>
    /// Construction Messages de type : ReportInstrMsfgGenMQueue
    /// 
    /// DONNEES attendues en entrée : 
    ///<para>────────────────────────────────────────────────────────────────────────────────────────────────</para>
    ///<para>  ● processType    : RIMGEN</para>
    ///<para>────────────────────────────────────────────────────────────────────────────────────────────────</para>
    ///
    /// PARAMETRES attendus en entrée (* = optionnel):
    ///<para>────────────────────────────────────────────────────────────────────────────────────────────────</para>
    ///<para>  RIMGEN                                                                                        </para>
    ///<para>  ● ENTITY      : Identifiant de l'entité</para>
    ///<para>  ● DTBUSINESS* : Si NON spécifiée = Plus petite DTENTITY de la table ENTITYMARKET</para>
    ///<para>                  pour l'entité demandée si non trouvée = DATE JOUR </para>
    ///<para>  ● CNFCLASS*   : Si spécifié = MULTI-PARTIES, MULTI-TRADES</para>
    ///<para>  ● CNFTYPE*    : Si spécifié = ALLOCATION, FINANCIAL, POSACTION, POSITION, POSSYNTHETIC, SYNTHESIS</para>
    ///<para>                  sinon équivalent à TOUS</para>
    ///<para>  ● GMARKET*    : Identifiant du groupe de marché</para>
    ///<para>  ● MARKET*     : Identifiant du marché</para>
    ///<para>  ● GACTOR*     : Identifiant du groupe d'acteur</para>
    ///<para>  ● ACTOR*      : ID/IDENTIFIER d'un acteur</para>
    ///<para>  ● GBOOK*      : Identifiant du groupe de book</para>
    ///<para>  ● BOOK*       : ID/IDENTIFIER d'un book</para>
    ///<para>  ● ISSEND*     : Avec envoi du message après génération
    ///<para>                  si non renseigné équivalent à false (génération uniquement)</para>
    ///
    ///<para>  RIMGEN (FINANCIALPERIODIC)</para>
    ///<para>  ● ENTITY         : Identifiant de l'entité</para>
    ///<para>  ● DATE1 et DATE2 : Si NON spécifiée = Plus petite DTENTITY de la table ENTITYMARKET</para>
    ///<para>  ● CNFTYPE        : FINANCIALPERIODIC</para>
    ///<para>  ● BOOK*          : ID/IDENTIFIER d'un book</para>
    ///<para>────────────────────────────────────────────────────────────────────────────────────────────────</para> 
    ///
    /// Préparation du(des) message(s) normalisé(s) de type ReportInstrMsfgGenMQueue
    ///<para>────────────────────────────────────────────────────────────────────────────────────────────────</para>
    ///<para>  ● Lecture, contrôle des paramètres présents, recherche de leurs ID</para>
    ///<para>  ● Exécution de la requête de recherche des lignes candidates à envoi de message</para>
    ///<para>    requête presqu'identique à MCO_RIMGEN.xml</para>
    ///<para>  ● Construction des messages finaux</para>
    ///<para>────────────────────────────────────────────────────────────────────────────────────────────────</para> 
    ///</summary>
    public class NormMsgFactory_REPORTING : NormMsgFactoryBase
    {
        #region Members
        Nullable<NotificationTypeEnum> m_NotificationType;
        Nullable<NotificationClassEnum> m_NotificationClass;
        #endregion Members

        #region Constructors
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pNormMsgFactoryProcess"></param>
        ///FI 20150618 [20945] Modify
        public NormMsgFactory_REPORTING(NormMsgFactoryProcess pNormMsgFactoryProcess)
            : base(pNormMsgFactoryProcess)
        {
            
            
        }
        #endregion Constructors

        #region Methods
        
        /// <summary>
        ///  Retoune un Message Queue à partir de l'enregistrement {pDr}
        /// </summary>
        /// <param name="pDr"></param>
        /// <returns></returns>
        /// FI 20141230 [20616] Modify
        /// FI 20150522 [20987] Modify
        protected override MQueueBase BuildMessageQueue(DataRow pDr)
        {
            MQueueBase ret = new ReportInstrMsgGenMQueue(m_MQueueAttributes)
            {
                id = Convert.ToInt32(pDr["IDA"]),
                idSpecified = true,
                identifierSpecified = true,
                identifier = pDr["ACTOR_IDENTIFIER"].ToString()
            };

            if (null != m_MQueueAttributes.parameters)
                ret.parameters = m_MQueueAttributes.parameters.Clone();

            // CNFTYPE parameter
            // CNFTYPE existe dans le reader => Mis à jour du paramètre systématiquement
            if (!(ret.parameters[NormMsgFactoryMQueue.PARAM_CNFTYPE] is MQueueparameter parameter))
            {
                parameter = new MQueueparameter(NormMsgFactoryMQueue.PARAM_CNFTYPE, TypeData.TypeDataEnum.@string);
                ret.parameters.Add(parameter);
            }
            parameter.SetValue(pDr["CNFTYPE"].ToString());

            // CNFCLASS parameter
            parameter = ret.parameters[NormMsgFactoryMQueue.PARAM_CNFCLASS] as MQueueparameter;
            if (null == parameter)
            {
                parameter = new MQueueparameter(NormMsgFactoryMQueue.PARAM_CNFCLASS, TypeData.TypeDataEnum.@string);
                ret.parameters.Add(parameter);
            }
            parameter.SetValue(pDr["CNFCLASS"].ToString());

            // FI 20141230 [20616]
            // BOOK parameter est obligatoire uniquement si édition simple
            // Pas de valorisation si edition consolidée
            parameter = ret.parameters[NormMsgFactoryMQueue.PARAM_CNFCLASS] as MQueueparameter;
            string cnfClass = ReflectionTools.ConvertEnumToString<NotificationClassEnum>(NotificationClassEnum.MULTITRADES);
            if (parameter.Value == cnfClass)
            {
                parameter = ret.parameters[NormMsgFactoryMQueue.PARAM_BOOK] as MQueueparameter;
                if (null == parameter)
                {
                    parameter = new MQueueparameter(NormMsgFactoryMQueue.PARAM_BOOK, TypeData.TypeDataEnum.integer);
                    ret.parameters.Add(parameter);
                }
                parameter.SetValue(pDr["IDB"].ToString(), pDr["BOOK_IDENTIFIER"].ToString());
            }

            return ret;
        }
        
        /// <summary>
        /// Création des paramètres du message queue
        /// </summary>
        /// <returns></returns>
        /// FI 20130626 [18745] add SYNTHESIS
        /// FI 20141230 [20616] Refactoting 
        /// FI 20150220 [XXXXX] Modify 
        // EG 20190114 Add detail to ProcessLog Refactoring
        protected override Cst.ErrLevel CreateParameters()
        {
            Cst.ErrLevel codeReturn = Cst.ErrLevel.SUCCESS;

            if (false == BuildingInfo.parametersSpecified)
            {
                // FI 20200623 [XXXXX] SetErrorWarning
                m_NormMsgFactoryProcess.ProcessState.SetErrorWarning(ProcessStateTools.StatusErrorEnum);
                
                
                Logger.Log(new LoggerData(LogLevelEnum.Error, new SysMsgCode(SysCodeEnum.SYS, 8000), 2,
                    new LogParam(NormMsgFactoryMQueue.PARAM_ENTITY),
                    new LogParam(m_NormMsgFactoryProcess.LogId),
                    new LogParam(m_NormMsgFactoryProcess.LogProcessType)));

                codeReturn = Cst.ErrLevel.FAILURE;
            }

            if (Cst.ErrLevel.SUCCESS == codeReturn)
            {
                codeReturn = AddEntityParameter();
                
                //CNFCLASS et CNFTYPE
                if (codeReturn == Cst.ErrLevel.SUCCESS)
                    codeReturn = AddCnfClassCnfType(); 
                
                //DATE1 et DATE2
                if (codeReturn == Cst.ErrLevel.SUCCESS)
                    codeReturn = AddDateParameters();

                // GMARKET/MARKET
                if (Cst.ErrLevel.SUCCESS == codeReturn)
                {
                    if ((false == m_NotificationType.HasValue) ||
                         (m_NotificationType.Value == NotificationTypeEnum.ALLOCATION) ||
                         (m_NotificationType.Value == NotificationTypeEnum.POSACTION) ||
                         (m_NotificationType.Value == NotificationTypeEnum.POSITION) || (m_NotificationType.Value == NotificationTypeEnum.POSSYNTHETIC))
                    {
                        codeReturn = AddGroupParameter(NormMsgFactoryMQueue.PARAM_GMARKET, RoleGMarket.CNF.ToString());
                        if (Cst.ErrLevel.SUCCESS == codeReturn)
                        {
                            int idg = m_MQueueAttributes.parameters.GetIntValueParameterById(NormMsgFactoryMQueue.PARAM_GMARKET);
                            Nullable<int> idg2 = null;
                            if (idg > 0)
                                idg2 = idg;
                            codeReturn = AddMarketParameter(idg2);
                        }
                    }
                }
                // FI 20141230 [20616] Add  GACTOR/ACTOR à l'image de GMARKET/MARKET
                // GACTOR/ACTOR
                if (Cst.ErrLevel.SUCCESS == codeReturn)
                {
                    codeReturn = AddGroupParameter(NormMsgFactoryMQueue.PARAM_GACTOR, RoleGMarket.CNF.ToString());
                    if (Cst.ErrLevel.SUCCESS == codeReturn)
                    {
                        int idg = m_MQueueAttributes.parameters.GetIntValueParameterById(NormMsgFactoryMQueue.PARAM_GACTOR);
                        Nullable<int> idg2 = null;
                        if (idg > 0)
                            idg2 = idg;
                        codeReturn = AddActorParameter(idg2);
                    }
                }
                // FI 20141230 [20616] Add  GBOOK/BOOK à l'image de GMARKET/MARKET
                // GBOOK/BOOK
                if (Cst.ErrLevel.SUCCESS == codeReturn)
                {
                    codeReturn = AddGroupParameter(NormMsgFactoryMQueue.PARAM_GBOOK, RoleGMarket.CNF.ToString());
                    if (Cst.ErrLevel.SUCCESS == codeReturn)
                    {
                        Nullable<int> idg = m_MQueueAttributes.parameters.GetIntValueParameterById(NormMsgFactoryMQueue.PARAM_GBOOK);
                        Nullable<int> idg2 = null;
                        if (idg > 0)
                            idg2 = idg;
                        codeReturn = AddBookParameter(idg2);
                    }
                }

                // FI 20130604 [SHEDULING] permettre l'envoi des messages générés
                if (Cst.ErrLevel.SUCCESS == codeReturn)
                {
                    if (null != BuildingInfo.parameters.GetObjectValueParameterById(NormMsgFactoryMQueue.PARAM_SEND))
                    {
                        MQueueparameter parameter = new MQueueparameter(ReportInstrMsgGenMQueue.PARAM_ISWITHIO, TypeData.TypeDataEnum.@bool);
                        parameter.SetValue(BuildingInfo.parameters.GetBoolValueParameterById(NormMsgFactoryMQueue.PARAM_SEND));
                        AddMQueueAttributesParameter(parameter);
                    }
                }
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
            return pDt.Select(null, "ACTOR_IDENTIFIER, CNFTYPEORDER, BOOKORDER", DataViewRowState.OriginalRows);
        }

        //FI 20150618 [20945] Mise en commentaire de override of ConstructSendingMQueue
        /*  
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        /// FI 20150522 [20987] Modify
        protected override Cst.ErrLevel ConstructSendingMQueue()
        {
            switch (BuildingInfo.processType)
            {
                case Cst.ProcessTypeEnum.RIMGEN:
                    // FI 20150522 [20987] le menu PROCESS_FINPER_RIMGEN n'existe pas
                    //if (m_NotificationType.HasValue && (m_NotificationType.Value == NotificationTypeEnum.FINANCIALPERIODIC))
                    //    m_TaskInfo.tracker.caller = IdMenu.GetIdMenu(IdMenu.Menu.PROCESS_FINPER_RIMGEN);
                    //else
                    m_TaskInfo.tracker.caller = IdMenu.GetIdMenu(IdMenu.Menu.PROCESS_RIMGEN);
                    break;
            }

            return base.ConstructSendingMQueue();
        }*/

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected override QueryParameters GetQueryParameters()
        {
            DataParameters dp = CreateDataParameters();

            #region Query
            StrBuilder sqlQuery = new StrBuilder();
            sqlQuery += "select CNFTYPE, CNFCLASS, ACTOR_IDA as IDA, BOOK_IDB as IDB, BOOK_IDENTIFIER," + Cst.CrLf;
            sqlQuery += "ACTOR_IDENTIFIER, CNFTYPEORDER, isnull(BOOK_IDENTIFIER,'zzzzz') as BOOKORDER" + Cst.CrLf;
            sqlQuery += "from (" + Cst.CrLf;

            if (m_NotificationType.HasValue)
            {
                #region Une seule notification
                string cnfTypeOrder = "999";
                switch (m_NotificationType.Value)
                {
                    case NotificationTypeEnum.ALLOCATION:
                        cnfTypeOrder = "1";
                        break;
                    case NotificationTypeEnum.POSACTION:
                        cnfTypeOrder = "2";
                        break;
                    case NotificationTypeEnum.POSITION:
                        cnfTypeOrder = "3";
                        break;
                    case NotificationTypeEnum.POSSYNTHETIC:
                        cnfTypeOrder = "4";
                        break;
                    case NotificationTypeEnum.FINANCIAL:
                        cnfTypeOrder = "5";
                        break;
                    case NotificationTypeEnum.FINANCIALPERIODIC:
                        cnfTypeOrder = "6";
                        break;
                    case NotificationTypeEnum.SYNTHESIS:
                        cnfTypeOrder = "7";
                        break;
                }
                sqlQuery += GetQueryRIM(m_NotificationType.Value, cnfTypeOrder);
                #endregion Une seule notification
            }
            else
            {
                #region Toutes les notifications
                sqlQuery += GetQueryRIM(NotificationTypeEnum.ALLOCATION, "1");
                sqlQuery += SQLCst.UNIONALL + Cst.CrLf;
                sqlQuery += GetQueryRIM(NotificationTypeEnum.POSACTION, "2");
                sqlQuery += SQLCst.UNIONALL + Cst.CrLf;
                sqlQuery += GetQueryRIM(NotificationTypeEnum.POSITION, "3");
                sqlQuery += SQLCst.UNIONALL + Cst.CrLf;
                sqlQuery += GetQueryRIM(NotificationTypeEnum.POSSYNTHETIC, "4");
                sqlQuery += SQLCst.UNIONALL + Cst.CrLf;
                sqlQuery += GetQueryRIM(NotificationTypeEnum.FINANCIAL, "5");
                sqlQuery += SQLCst.UNIONALL + Cst.CrLf;
                sqlQuery += GetQueryRIM(NotificationTypeEnum.SYNTHESIS, "6");
                //FI 20150522 [20987] Add FINANCIALPERIODIC (FINANCIALPERIODIC rentre dans le même moule que SYNTHESIS)
                sqlQuery += SQLCst.UNIONALL + Cst.CrLf;
                sqlQuery += GetQueryRIM(NotificationTypeEnum.FINANCIALPERIODIC, "7");
                #endregion Totutes les notifications
            }

            sqlQuery += ") tblresult" + Cst.CrLf;
            sqlQuery += "inner join dbo.ENTITY e on (e.IDA = @ENTITY)" + Cst.CrLf;
            sqlQuery += "where exists (" + Cst.CrLf;
            sqlQuery += "select 1" + DataHelper.SQLFromDual(m_NormMsgFactoryProcess.Cs) + Cst.CrLf;
            sqlQuery += "where (e.ISSENDNCMSG_ENTITY = 1) and not exists (select 1 from dbo.ACTORROLE ar where (ar.IDROLEACTOR = 'CLIENT') and (ar.IDA = tblresult.ACTOR_IDA))" + Cst.CrLf;
            sqlQuery += "union" + Cst.CrLf;
            sqlQuery += "select 1" + DataHelper.SQLFromDual(m_NormMsgFactoryProcess.Cs) + Cst.CrLf;
            sqlQuery += "where (e.ISSENDNCMSG_CLIENT = 1) and exists (select 1 from dbo.ACTORROLE ar where (ar.IDROLEACTOR = 'CLIENT') and (ar.IDA = tblresult.ACTOR_IDA))" + Cst.CrLf;
            sqlQuery += ")" + Cst.CrLf;
            #endregion SetQuery

            QueryParameters ret = new QueryParameters(m_NormMsgFactoryProcess.Cs, sqlQuery.ToString(), dp);

            return ret;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        /// FI 20141230 [20616] Modify
        /// FI 20150527 [20987] Modify
        private DataParameters CreateDataParameters()
        {
            DataParameters dp = new DataParameters();

            if (BuildingInfo.parametersSpecified)
            {
                MQueueparameters parameters = m_MQueueAttributes.parameters;

                // ENTITY
                dp.Add(new DataParameter(m_NormMsgFactoryProcess.Cs, ReportInstrMsgGenMQueue.PARAM_ENTITY, DbType.Int32),
                    parameters.GetIntValueParameterById(ReportInstrMsgGenMQueue.PARAM_ENTITY));

                // FI 20150527 [20987] Alimentation des paramètres DATE1 et DATE2 
                // DATE1 et DAT2
                dp.Add(new DataParameter(m_NormMsgFactoryProcess.Cs, ReportInstrMsgGenMQueue.PARAM_DATE1, DbType.Date),
                            parameters.GetDateTimeValueParameterById(ReportInstrMsgGenMQueue.PARAM_DATE1));

                if (IsUseDate1AndDate2())
                    dp.Add(new DataParameter(m_NormMsgFactoryProcess.Cs, ReportInstrMsgGenMQueue.PARAM_DATE2, DbType.Date),
                            parameters.GetDateTimeValueParameterById(ReportInstrMsgGenMQueue.PARAM_DATE2));


                // MARKET & GMARKET
                if (null != BuildingInfo.parameters.GetObjectValueParameterById(ReportInstrMsgGenMQueue.PARAM_GMARKET))
                {
                    dp.Add(new DataParameter(m_NormMsgFactoryProcess.Cs, ReportInstrMsgGenMQueue.PARAM_GMARKET, DbType.Int32),
                        parameters.GetIntValueParameterById(ReportInstrMsgGenMQueue.PARAM_GMARKET));
                }
                if (null != m_MQueueAttributes.parameters.GetObjectValueParameterById(ReportInstrMsgGenMQueue.PARAM_MARKET))
                {
                    dp.Add(new DataParameter(m_NormMsgFactoryProcess.Cs, ReportInstrMsgGenMQueue.PARAM_MARKET, DbType.Int32),
                        parameters.GetIntValueParameterById(ReportInstrMsgGenMQueue.PARAM_MARKET));
                }

                // FI 20141230 [20616] BOOK & GBOOK (parallèle avec MARKET & GMARKET)
                // BOOK & GBOOK
                if (null != BuildingInfo.parameters.GetObjectValueParameterById(ReportInstrMsgGenMQueue.PARAM_GBOOK))
                {
                    dp.Add(new DataParameter(m_NormMsgFactoryProcess.Cs, ReportInstrMsgGenMQueue.PARAM_GBOOK, DbType.Int32),
                        parameters.GetIntValueParameterById(ReportInstrMsgGenMQueue.PARAM_GBOOK));
                }
                if (null != m_MQueueAttributes.parameters.GetObjectValueParameterById(ReportInstrMsgGenMQueue.PARAM_BOOK))
                {
                    dp.Add(new DataParameter(m_NormMsgFactoryProcess.Cs, ReportInstrMsgGenMQueue.PARAM_BOOK, DbType.Int32),
                        parameters.GetIntValueParameterById(ReportInstrMsgGenMQueue.PARAM_BOOK));
                }

                // FI 20141230 [20616] ACTOR & GACTOR (parallèle avec MARKET & GMARKET)
                // ACTOR & GACTOR
                if (null != BuildingInfo.parameters.GetObjectValueParameterById(ReportInstrMsgGenMQueue.PARAM_GACTOR))
                {
                    dp.Add(new DataParameter(m_NormMsgFactoryProcess.Cs, ReportInstrMsgGenMQueue.PARAM_GACTOR, DbType.Int32),
                        parameters.GetIntValueParameterById(ReportInstrMsgGenMQueue.PARAM_GACTOR));
                }
                if (null != m_MQueueAttributes.parameters.GetObjectValueParameterById(ReportInstrMsgGenMQueue.PARAM_ACTOR))
                {
                    dp.Add(new DataParameter(m_NormMsgFactoryProcess.Cs, ReportInstrMsgGenMQueue.PARAM_ACTOR, DbType.Int32),
                        parameters.GetIntValueParameterById(ReportInstrMsgGenMQueue.PARAM_ACTOR));
                }
            }
            return dp;
        }
        
        /// <summary>
        /// Construction de la requête principale (mode itératif en fonction du type de notification
        /// à l'exception de NotificationTypeEnum.FINANCIAL - lié au final par UNION ALL) 
        /// </summary>
        /// <param name="pNotificationType">Type de notification (ALLOCATION, FINANCIAL, POSACTION, POSITION, POSSYNTHETIC)</param>
        /// <param name="pCstOrder">Numéro d'ordre associé au type de notification (utilisé pour le tri)</param>
        /// <returns>Requête SQL pour un type de notification</returns>
        /// FI 20130627 [18745] add case SYNTHESIS
        /// FI 20141223 [XXXXX] Modify add restriction DTENABLED sur msg
        /// FI 20141230 [20616] Modify (gestion des paramètres GBOOK, BOOK, GACTOR, ACTOR)
        /// FI 20150522 [20987] Modify
        /// RD 20160912 [22447] Modify (gestion des reports SYNTHESIS MULTI-PARTIES)
        private string GetQueryRIM(NotificationTypeEnum pCnfType, string pCnfTypeOrder)
        {
            string cnfType = DataHelper.SQLString(pCnfType.ToString());
            string cnfTypeOrder = DataHelper.SQLString(pCnfTypeOrder);
            
			//Select
            StrBuilder sqlQuery = new StrBuilder();

            if ((false == m_NotificationClass.HasValue) || (m_NotificationClass.Value == NotificationClassEnum.MULTITRADES)
                || (pCnfType == NotificationTypeEnum.FINANCIAL || pCnfType == NotificationTypeEnum.FINANCIALPERIODIC))
			{
                string cnfClass = DataHelper.SQLString(ReflectionTools.ConvertEnumToString<NotificationClassEnum>(NotificationClassEnum.MULTITRADES));
				
                sqlQuery += "select distinct " + cnfType + @" as CNFTYPE, " + cnfTypeOrder + " as CNFTYPEORDER," + Cst.CrLf;
                sqlQuery += cnfClass + " as CNFCLASS, a.IDA as ACTOR_IDA, a.IDENTIFIER as ACTOR_IDENTIFIER, ";
                sqlQuery += "b.IDB as BOOK_IDB, b.IDENTIFIER as BOOK_IDENTIFIER" + Cst.CrLf;
                
                switch (pCnfType)
                {
                    case NotificationTypeEnum.FINANCIAL:
                    case NotificationTypeEnum.SYNTHESIS:
                    case NotificationTypeEnum.FINANCIALPERIODIC:
                        sqlQuery += GetSubQueryRIMCashBalance(pCnfType, NotificationClassEnum.MULTITRADES) + Cst.CrLf;
                        break;
                    default:
                        sqlQuery += GetSubQueryRIM(pCnfType, NotificationClassEnum.MULTITRADES) + Cst.CrLf;
                        break;
                }
            }

            if ((false == m_NotificationClass.HasValue) 
                && (pCnfType != NotificationTypeEnum.FINANCIAL && pCnfType != NotificationTypeEnum.FINANCIALPERIODIC))
                sqlQuery += "union all" + Cst.CrLf;

            if (((false == m_NotificationClass.HasValue) || (m_NotificationClass.Value == NotificationClassEnum.MULTIPARTIES))
                && (pCnfType != NotificationTypeEnum.FINANCIAL && pCnfType != NotificationTypeEnum.FINANCIALPERIODIC))
            {
                string cnfClass = DataHelper.SQLString(ReflectionTools.ConvertEnumToString<NotificationClassEnum>(NotificationClassEnum.MULTIPARTIES));

                sqlQuery += "select distinct " + cnfType + @" as CNFTYPE, " + cnfTypeOrder + " as CNFTYPEORDER," + Cst.CrLf;
                sqlQuery += cnfClass + " as CNFCLASS, a.IDA as ACTOR_IDA, a.IDENTIFIER as ACTOR_IDENTIFIER, ";
                sqlQuery += "null as BOOK_IDB, null  as BOOK_IDENTIFIER" + Cst.CrLf;
                sqlQuery += "from dbo.ACTOR a" + Cst.CrLf;
                sqlQuery += "inner join ( select distinct IDA, IDA_ACTOR, LEVELACTOR from dbo.BOOKACTOR_R where (ISPARTYCONSO = 1)) link on (link.IDA_ACTOR = a.IDA)" + Cst.CrLf;
                sqlQuery += "inner join (" + Cst.CrLf;
                sqlQuery += "select distinct b.IDA" + Cst.CrLf;

                switch (pCnfType)
                {
                    case NotificationTypeEnum.SYNTHESIS:
                        sqlQuery += GetSubQueryRIMCashBalance(pCnfType, NotificationClassEnum.MULTIPARTIES) + Cst.CrLf;
                        break;
                    default:
                        sqlQuery += GetSubQueryRIM(pCnfType, NotificationClassEnum.MULTIPARTIES) + Cst.CrLf;
                        break;
                }

                sqlQuery += ") at on (at.IDA = link.IDA)" + Cst.CrLf;
                sqlQuery += "where ((a.ISALL_CNF = 1) or (a.ISOWN_CNF = 1) or (a.ISCHILD_CNF = 1)) and" + Cst.CrLf;
                sqlQuery += "(link.LEVELACTOR>= case when (a.ISOWN_CNF = 1) or (a.ISALL_CNF = 1) then 1 else 2 end)" + Cst.CrLf;

            }
            return sqlQuery.ToString();
        }

        /// <summary>
        /// Construction de la sous-requête intégrée à la requête principale (pour chaque type de notification) 
        /// équivaut aux Clauses FROM et EXISTS communes à tous les types (ou presque)
        /// </summary>
        /// <param name="pCnfType">Type de notification</param>
        /// <param name="pCnfClass">Class de notification (MULTITRADES, MULTIPARTIES)</param>
        /// <returns>Sous Requête SQL pour un type de notification</returns>
        /// FI 20130418 [18602] modification de la requête spécifique à POSACTION
        /// FI 20130418 [18602] modification de la requête spécifique à POSACTION pour amélioration des performances
        /// La requête ne s'appuie plus sur EVENT,EVENTCLASS sur POSACTION
        /// FI 20141223 [XXXXX] Modify add restriction DTENABLED sur msg, suppression des jointures sur TRADEACTOR (Utilisation de TRADEINSTRUMENT)
        /// FI 20141230 [20616] Modify
        /// FI 20150525 [20987] Modify
        /// FI 20160225 [XXXXX] Modify
        /// FI 20170327 [23004] Modify
        // EG 20191115 [25077] RDBMS : New version of Trades tables architecture (TRADESTSYS merge to TRADE, NEW TABLE TRADEXML)
        // EG 20200226 Refactoring suite à à TRADEINSTRUMENT (INSTRUMENTNO=1) dans TRADE
        private string GetSubQueryRIM(NotificationTypeEnum pCnfType, NotificationClassEnum pCnfClass)
        {
            // CNFTYPE
            string cnfType = DataHelper.SQLString(pCnfType.ToString());

            // CNFCLASS
            string cnfClass = DataHelper.SQLString(ReflectionTools.ConvertEnumToString<NotificationClassEnum>(pCnfClass));

            //Select
            StrBuilder sqlQuery = new StrBuilder();
            sqlQuery += @"from dbo.TRADE t" + Cst.CrLf;
            sqlQuery += "inner join dbo.INSTRUMENT i on (i.IDI = t.IDI)" + Cst.CrLf;

            if (pCnfType == NotificationTypeEnum.ALLOCATION)
                // FI 20160225 [XXXXX] jointure sur TRADEINSTRUMENT identique au fichier XML (MCO_RIMGEN.xml) 
                sqlQuery += "and ((t.TRDTYPE is null) or (t.TRDTYPE != '1000') or (t.TRDTYPE = '1000' and t.TRDSUBTYPE is not null))" + Cst.CrLf;
            
            // FI 20170327 [23004] Utilisation de la méthode GetIntValueParameterById puisque la valeur -1 (tous marchés) est possible
            // if (null != m_MQueueAttributes.parameters.GetObjectValueParameterById(NormMsgFactoryMQueue.PARAM_MARKET))
            if (m_MQueueAttributes.parameters.GetIntValueParameterById(NormMsgFactoryMQueue.PARAM_MARKET)>0)
                sqlQuery += "and (t.IDM = @MARKET)" + Cst.CrLf;

            sqlQuery += Cst.CrLf;
            sqlQuery += "inner join dbo.ACTOR a on (a.IDA = t.IDA_DEALER)" + Cst.CrLf;
            sqlQuery += "inner join dbo.BOOK b on (b.IDB = t.IDB_DEALER) and (b.ISRECEIVENCMSG = 1) and (b.IDA_ENTITY = @ENTITY)" + Cst.CrLf;

            if (null != m_MQueueAttributes.parameters.GetObjectValueParameterById(NormMsgFactoryMQueue.PARAM_BOOK))
                sqlQuery += "and (b.IDB = @BOOK)" + Cst.CrLf;

            if (null != m_MQueueAttributes.parameters.GetObjectValueParameterById(NormMsgFactoryMQueue.PARAM_ACTOR))
                sqlQuery += "and (a.IDA = @ACTOR)" + Cst.CrLf;

            sqlQuery += "where (t.IDSTBUSINESS = 'ALLOC') and (t.IDSTENVIRONMENT = 'REGULAR')" + Cst.CrLf;
            if (pCnfType == NotificationTypeEnum.ALLOCATION)
                sqlQuery += "and (t.IDSTACTIVATION = 'REGULAR')";

            if (pCnfType == NotificationTypeEnum.POSACTION)
            {
                sqlQuery += StrFunc.AppendFormat(@"
                        and exists (
                                select 1  from dbo.CNFMESSAGE cnfMsg  
                                          where (cnfMsg.CNFTYPE='POSACTION') and (cnfMsg.MSGTYPE={0})
                                          and ({1}))", cnfClass, OTCmlHelper.GetSQLDataDtEnabled(m_NormMsgFactoryProcess.Cs, "cnfMsg"));
            }
            else
            {
                // FI 20150525 [20987] Refactoring 
                string arg;
                switch (pCnfType)
                {
                    case NotificationTypeEnum.FINANCIALPERIODIC:
                    case NotificationTypeEnum.SYNTHESIS:
                        // EG 20160404 Migration vs2013
                        arg = "ec.DTEVENT between @DATE1 and @DATE2";
                        break;
                    default:
                        arg = "ec.DTEVENT=@DATE1";
                        break;
                }

                sqlQuery += StrFunc.AppendFormat(@"
                        and exists (
                                select 1 from dbo.EVENT e
                                inner join dbo.EVENTCLASS ec on (ec.IDE = e.IDE)
                                inner join dbo.CNFMESSAGE cnf on (cnf.CNFTYPE = {0}) and (cnf.MSGTYPE = {1})
                                and (cnf.EVENTCODE = e.EVENTCODE) and (cnf.EVENTCLASS = ec.EVENTCLASS) 
                                and (isnull(cnf.EVENTTYPE, e.EVENTTYPE)=e.EVENTTYPE) 
                                and {2}
                                where (e.IDT = t.IDT) and ({3}))",
                                cnfType, cnfClass, OTCmlHelper.GetSQLDataDtEnabled(m_NormMsgFactoryProcess.Cs, "cnf"), arg);

            }


            //FI 20150525 [20987] Usage des paramètres DATE1
            if (pCnfType == NotificationTypeEnum.POSACTION)
            {
                //FI 20130418 [18602]
                //FI 20141223 [XXXXX] restriction exists à l'identique par rapport au fichier XML MCO_RIMGEN.xml
                sqlQuery +=
@"
and exists
    (
        select 1
        from dbo.POSACTIONDET pad 
        inner join dbo.POSACTION pa on pa.IDPA=pad.IDPA 
        where 
        ( 
          (pa.DTBUSINESS=@DATE1 and ((pad.DTCAN is null) or (pad.DTCAN > @DATE1)))
          or
          (pad.DTCAN=@DATE1 and pad.DTCAN>pa.DTBUSINESS and pad.CANDESCRIPTION='UnClearing')
        )
        and 
        (
          (t.IDT=pad.IDT_BUY or t.IDT=pad.IDT_SELL)         
        )
    )
";
                sqlQuery += Cst.CrLf;
            }
            else if (pCnfType == NotificationTypeEnum.ALLOCATION)
            {
                // FI 20181002 [24219] Add Restriction
                sqlQuery += "and t.DTTRADE >= @DATE1" + Cst.CrLf;
            }

            // Ajout restriction groupe de marché si spécifié
            // FI 20170327 [23004] Utilisation de la méthode GetIntValueParameterById puisque la valeur -1 (tous marchés) est possible
            //if (null != BuildingInfo.parameters.GetObjectValueParameterById(NormMsgFactoryMQueue.PARAM_GMARKET) &&
            //    (null == m_MQueueAttributes.parameters.GetObjectValueParameterById(NormMsgFactoryMQueue.PARAM_MARKET)))
            if (null != BuildingInfo.parameters.GetObjectValueParameterById(NormMsgFactoryMQueue.PARAM_GMARKET) &&
                (false == m_MQueueAttributes.parameters.GetIntValueParameterById(NormMsgFactoryMQueue.PARAM_MARKET)>0))
            {
                sqlQuery += GetSQLRestrictGMARKET("t.IDM") + Cst.CrLf;
            }

            // Ajout restriction groupe d'acteur si spécifié
            if (null != BuildingInfo.parameters.GetObjectValueParameterById(NormMsgFactoryMQueue.PARAM_GACTOR) &&
                (null == m_MQueueAttributes.parameters.GetObjectValueParameterById(NormMsgFactoryMQueue.PARAM_ACTOR)))
            {
                sqlQuery += GetSQLRestrictGACTOR("t.IDA_DEALER") + Cst.CrLf;
            }

            // Ajout restriction groupe de book si spécifié
            if (null != BuildingInfo.parameters.GetObjectValueParameterById(NormMsgFactoryMQueue.PARAM_GBOOK) &&
                (null == m_MQueueAttributes.parameters.GetObjectValueParameterById(NormMsgFactoryMQueue.PARAM_BOOK)))
            {
                sqlQuery += GetSQLRestrictGBOOK("t.IDB_DEALER") + Cst.CrLf;
            }

            return sqlQuery.ToString();
        }

        /// <summary>
        /// Construction de la sous-requête intégrée à la requête principale 
        /// (pour les notifications à base d'un ou plusieurs Trades CashBalance)
        /// </summary>
        /// <param name="pCnfType">Type de notification</param>
        /// <param name="pCnfClass">Class de notification (MULTITRADES, MULTIPARTIES)</param>
        /// <returns>Sous Requête SQL pour un type de notification</returns>
        /// RD 20160912 [22447] Add
        // EG 20191115 [25077] RDBMS : New version of Trades tables architecture (TRADESTSYS merge to TRADE, NEW TABLE TRADEXML)
        // EG 20200226 Refactoring suite à à TRADEINSTRUMENT (INSTRUMENTNO=1) dans TRADE
        private string GetSubQueryRIMCashBalance(NotificationTypeEnum pCnfType, NotificationClassEnum pCnfClass)
        {
            // CNFTYPE
            string cnfType = DataHelper.SQLString(pCnfType.ToString());

            // CNFCLASS
            string cnfClass = DataHelper.SQLString(ReflectionTools.ConvertEnumToString<NotificationClassEnum>(pCnfClass));

            //Select
            StrBuilder sqlQuery = new StrBuilder();
            sqlQuery += @"  from dbo.TRADE t
            inner join dbo.INSTRUMENT i on i.IDI=t.IDI 
            inner join dbo.PRODUCT p on p.IDP=i.IDP and p.IDENTIFIER='cashBalance'
            inner join dbo.ACTOR a on a.IDA=t.IDA_RISK
            inner join dbo.BOOK b on b.IDB=t.IDB_RISK and b.ISRECEIVENCMSG=1 and (b.IDA_ENTITY=@ENTITY)";

            if (null != m_MQueueAttributes.parameters.GetObjectValueParameterById(NormMsgFactoryMQueue.PARAM_BOOK))
                sqlQuery += "and (b.IDB = @BOOK)" + Cst.CrLf;
            if (null != m_MQueueAttributes.parameters.GetObjectValueParameterById(NormMsgFactoryMQueue.PARAM_ACTOR))
                sqlQuery += "and (a.IDA = @ACTOR)" + Cst.CrLf;

            sqlQuery += "where (t.IDSTENVIRONMENT='REGULAR') and (t.IDA_BUYER=t.IDA_ENTITY) and exists (" + Cst.CrLf;
            sqlQuery += "select 1 from dbo.EVENT e" + Cst.CrLf;
            sqlQuery += "inner join dbo.EVENTCLASS ec on (ec.IDE=e.IDE)" + Cst.CrLf;
            sqlQuery += "inner join dbo.CNFMESSAGE cnf on (cnf.EVENTCODE=e.EVENTCODE) and " + Cst.CrLf;
            sqlQuery += "(isnull(cnf.EVENTTYPE, e.EVENTTYPE)=e.EVENTTYPE) and (cnf.EVENTCLASS=ec.EVENTCLASS) and " + Cst.CrLf;
            sqlQuery += "(cnf.MSGTYPE=" + cnfClass + ") and (cnf.CNFTYPE = " + cnfType + ") and" + Cst.CrLf;
            sqlQuery += OTCmlHelper.GetSQLDataDtEnabled(m_NormMsgFactoryProcess.Cs, "cnf") + Cst.CrLf;
            sqlQuery += "where (e.IDT=t.IDT)" + Cst.CrLf;
            if ((NotificationTypeEnum.FINANCIALPERIODIC == pCnfType) || (NotificationTypeEnum.SYNTHESIS == pCnfType))
                sqlQuery += "and (ec.DTEVENT between @DATE1 and @DATE2)" + Cst.CrLf;
            else
                sqlQuery += "and (ec.DTEVENT = @DATE1)" + Cst.CrLf;
            // FI 20181002 [24219] Add Restriction
            sqlQuery += "and t.DTTRADE >= @DATE1"+ Cst.CrLf;
            sqlQuery += ")" + Cst.CrLf;

            if (null == m_MQueueAttributes.parameters.GetObjectValueParameterById(NormMsgFactoryMQueue.PARAM_ACTOR) &&
                null != m_MQueueAttributes.parameters.GetObjectValueParameterById(NormMsgFactoryMQueue.PARAM_GACTOR))
                sqlQuery += GetSQLRestrictGACTOR("t.IDA_RISK");

            if (null == m_MQueueAttributes.parameters.GetObjectValueParameterById(NormMsgFactoryMQueue.PARAM_BOOK) &&
                null != m_MQueueAttributes.parameters.GetObjectValueParameterById(NormMsgFactoryMQueue.PARAM_GBOOK))
                sqlQuery += GetSQLRestrictGBOOK("t.IDB_RISK");

            return sqlQuery.ToString();
        }

        /// <summary>
        ///  Retourne une restrcition SQL de type and exists sur GMARKET
        /// </summary>
        /// <returns></returns>
        private static string GetSQLRestrictGMARKET(string pColumRef)
        {
            StrBuilder sqlQuery = new StrBuilder();
            sqlQuery += "and exists (" + Cst.CrLf;
            sqlQuery += "  select 1 from dbo.MARKET m" + Cst.CrLf;
            sqlQuery += "  inner join dbo.MARKETG mg on (mg.IDM = m.IDM)" + Cst.CrLf;
            sqlQuery += "  inner join dbo.GMARKET gm on (gm.IDGMARKET = mg.IDGMARKET)" + Cst.CrLf;
            sqlQuery += "  inner join dbo.GMARKETROLE gmr on (gmr.IDGMARKET = gm.IDGMARKET) and (gmr.IDROLEGMARKET = 'CNF')" + Cst.CrLf;
            sqlQuery += "  where (m.IDM = #pColumRef#) and (gm.IDGMARKET  = @GMARKET)" + Cst.CrLf;
            sqlQuery += ")" + Cst.CrLf;

            string ret = sqlQuery.ToString();
            ret = ret.Replace("#pColumRef#", pColumRef);

            return ret;
        }

        /// <summary>
        ///  Retourne une restriction sql de type exist sur GACTOR 
        /// </summary>
        /// <returns></returns>
        /// FI 20141230 [20616] Add  Method
        private static string GetSQLRestrictGACTOR(string pColumRef)
        {
            StrBuilder sqlQuery = new StrBuilder();
            sqlQuery += "and exists (" + Cst.CrLf;
            sqlQuery += "  select 1 from dbo.ACTOR a" + Cst.CrLf;
            sqlQuery += "  inner join dbo.ACTORG ag on (ag.IDA = a.IDA)" + Cst.CrLf;
            sqlQuery += "  inner join dbo.GACTOR ga on (ga.IDGACTOR = ag.IDGACTOR)" + Cst.CrLf;
            sqlQuery += "  inner join dbo.GACTORROLE gar on (gar.IDGACTOR = ga.IDGACTOR) and (gar.IDROLEGACTOR = 'CNF')" + Cst.CrLf;
            sqlQuery += "  where (a.IDA = #pColumRef#) and (ga.IDGACTOR  = @GACTOR)" + Cst.CrLf;
            sqlQuery += ")" + Cst.CrLf;

            string ret = sqlQuery.ToString();
            ret = ret.Replace("#pColumRef#", pColumRef);

            return ret;
        }

        /// <summary>
        ///  Retourne une restriction sql de type exist sur GBOOK
        /// </summary>
        /// <returns></returns>
        private static string GetSQLRestrictGBOOK(string pColumRef)
        {
            StrBuilder sqlQuery = new StrBuilder();
            sqlQuery += "and exists (" + Cst.CrLf;
            sqlQuery += "  select 1 from dbo.BOOK b" + Cst.CrLf;
            sqlQuery += "  inner join dbo.BOOKG bg on (bg.IDB = b.IDB)" + Cst.CrLf;
            sqlQuery += "  inner join dbo.GBOOK gb on (gb.IDGBOOK = bg.IDGBOOK)" + Cst.CrLf;
            sqlQuery += "  inner join dbo.GBOOKROLE gbr on (gbr.IDGBOOK = gb.IDGBOOK) and (gbr.IDROLEGBOOK = 'CNF')" + Cst.CrLf;
            sqlQuery += "  where (b.IDB = #pColumRef#) and (gb.IDGBOOK  = @GBOOK)" + Cst.CrLf;
            sqlQuery += ")" + Cst.CrLf;

            string ret = sqlQuery.ToString();
            ret = ret.Replace("#pColumRef#", pColumRef);

            return ret;
        }

        /// <summary>
        ///  Création des paramètre DATE1 et DATE2
        /// </summary>
        /// <returns></returns>
        /// FI 20150525 [20987] Add Method
        private Cst.ErrLevel AddDateParameters()
        {
            Cst.ErrLevel codeReturn = AddDateParameter(MQueueBase.PARAM_DATE1, string.Empty);
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

            if (codeReturn == Cst.ErrLevel.SUCCESS && IsUseDate1AndDate2())
            {
                codeReturn = AddDateParameter(NormMsgFactoryMQueue.PARAM_DATE2, string.Empty);
                if (codeReturn == Cst.ErrLevel.SUCCESS)
                {
                    if (StrFunc.IsEmpty(m_MQueueAttributes.parameters[NormMsgFactoryMQueue.PARAM_DATE2].Value)
                       || (DtFunc.ParseDate(m_MQueueAttributes.parameters[NormMsgFactoryMQueue.PARAM_DATE2].Value, DtFunc.FmtISODate, null) == DateTime.MinValue))
                    {
                        // Si le parampètre PARAM_DATE2 n'existe pas en entrée alors ajout du paramètre PARAM_DATE2 et valeur identique à PARAM_DATE1
                        m_MQueueAttributes.parameters[NormMsgFactoryMQueue.PARAM_DATE2].Value = m_MQueueAttributes.parameters[NormMsgFactoryMQueue.PARAM_DATE1].Value;
                    }
                }
            }

            return codeReturn;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private Cst.ErrLevel AddCnfClassCnfType()
        {
            Cst.ErrLevel codeReturn = Cst.ErrLevel.SUCCESS;

            if (Cst.ErrLevel.SUCCESS == codeReturn)
            {
                #region CNFCLASS
                string cnfClass = BuildingInfo.parameters.GetStringValueParameterById(NormMsgFactoryMQueue.PARAM_CNFCLASS);
                if (StrFunc.IsFilled(cnfClass) && (cnfClass != Cst.DDLVALUE_ALL))
                {
                    if (Enum.IsDefined(typeof(NotificationClassEnum), cnfClass))
                    {
                        m_NotificationClass = (NotificationClassEnum)Enum.Parse(typeof(NotificationClassEnum), cnfClass, true);
                        switch (m_NotificationClass.Value)
                        {
                            case NotificationClassEnum.MULTITRADES:
                            case NotificationClassEnum.MULTIPARTIES:
                                break;
                            default:
                                // ERROR = Add LOG
                                // CNFCLASS incorrect
                                codeReturn = Cst.ErrLevel.FAILURE;
                                break;
                        }
                    }
                    else
                    {
                        // FI 20150220 [XXXXX] cas du MULTIPARTIES 
                        if (cnfClass == ReflectionTools.ConvertEnumToString<NotificationClassEnum>(NotificationClassEnum.MULTITRADES))
                            m_NotificationClass = NotificationClassEnum.MULTITRADES;
                        else if (cnfClass == ReflectionTools.ConvertEnumToString<NotificationClassEnum>(NotificationClassEnum.MULTIPARTIES))
                            m_NotificationClass = NotificationClassEnum.MULTIPARTIES;
                        else
                            codeReturn = Cst.ErrLevel.FAILURE;
                    }
                }
                #endregion CNFCLASS
            }

            if (Cst.ErrLevel.SUCCESS == codeReturn)
            {
                #region CNFTYPE
                string cnfType = BuildingInfo.parameters.GetStringValueParameterById(NormMsgFactoryMQueue.PARAM_CNFTYPE);
                if (StrFunc.IsFilled(cnfType) && (cnfType != Cst.DDLVALUE_ALL))
                {
                    if (Enum.IsDefined(typeof(NotificationTypeEnum), cnfType))
                    {
                        m_NotificationType = (NotificationTypeEnum)Enum.Parse(typeof(NotificationTypeEnum), cnfType, true);
                        switch (m_NotificationType.Value)
                        {
                            case NotificationTypeEnum.ALLOCATION:
                            case NotificationTypeEnum.FINANCIAL:
                            case NotificationTypeEnum.POSACTION:
                            case NotificationTypeEnum.POSITION:
                            case NotificationTypeEnum.POSSYNTHETIC:
                            case NotificationTypeEnum.FINANCIALPERIODIC:
                            case NotificationTypeEnum.SYNTHESIS:
                                // CNFTYPE
                                MQueueparameter parameter = new MQueueparameter(NormMsgFactoryMQueue.PARAM_CNFTYPE, TypeData.TypeDataEnum.@string);
                                parameter.SetValue(cnfType);
                                AddMQueueAttributesParameter(parameter);
                                break;
                            default:
                                // ERROR = Add LOG
                                // CNFTYPE incompatible
                                codeReturn = Cst.ErrLevel.FAILURE;
                                break;
                        }
                    }
                    else
                    {
                        // ERROR = Add LOG
                        // CNFTYPE incorrect
                        codeReturn = Cst.ErrLevel.FAILURE;
                    }
                }

                #endregion CNFTYPE
            }
            return codeReturn;
        }




        /// <summary>
        /// <para>Retourne true lorsque les paramètres DATE1 et DATE2 sont nécessaires</para>
        /// <para>Retourne false lorsque seul le paramètre DATE1</para>
        /// </summary>
        /// <returns></returns>
        /// FI 20150522 [20275] Add
        private Boolean IsUseDate1AndDate2()
        {
            Boolean ret = true;
            if (m_NotificationType.HasValue)
            {
                switch (m_NotificationType.Value)
                {
                    case NotificationTypeEnum.SYNTHESIS:
                    case NotificationTypeEnum.FINANCIALPERIODIC:
                        ret = true;
                        break;
                    default:
                        ret = false;
                        break;
                }
            }
            return ret;
        }

        #endregion Methods
    }
}
