#region Using Directives
using EFS.ACommon;
using EFS.ApplicationBlocks.Data;
using EFS.Common;
using EFS.Common.Log;
using EFS.Common.MQueue;
using EFS.LoggerClient;
using EFS.LoggerClient.LoggerService;
using EfsML.Business;
using System;
using System.Collections;
using System.Data;
#endregion Using Directives

namespace EFS.Process
{
    /// <summary>
    /// Construction Messages de type : IOMQueue
    /// 
    /// DONNEES attendues en entrée : 
    ///<para>────────────────────────────────────────────────────────────────────────────────────────────────</para>
    ///<para>  ● processType : IO</para>
    ///<para>  ● id          : Id de la tâche IO</para>
    ///<para>  ● identifier  : Identifiant de la tâche IO</para>
    ///<para>────────────────────────────────────────────────────────────────────────────────────────────────</para>
    ///
    /// PARAMETRES attendus en entrée : les valeurs des paramètres de la tâche concernée
    ///<para>────────────────────────────────────────────────────────────────────────────────────────────────</para>
    ///<para>  ● Les valeurs des paramètres de la tâche concernée</para>
    ///<para>  ● Peuvent être omis les paramètres optionnels ou dôtés de valeurs par défaut</para>
    ///<para>────────────────────────────────────────────────────────────────────────────────────────────────</para> 
    ///
    /// Préparation du(des) message(s) normalisé(s) de type IOMQueue
    ///<para>────────────────────────────────────────────────────────────────────────────────────────────────</para>
    ///<para>  ● Recherche de la tâche demandée</para>
    ///<para>  ● Lecture et valorisation des paramètres de la tâche avec ceux présents dans le message d'origine</para>
    ///<para>  ● Construction des messages finaux</para>
    ///<para>────────────────────────────────────────────────────────────────────────────────────────────────</para> 
    ///</summary>
    public class NormMsgFactory_IO : NormMsgFactoryBase
    {
        #region Members
        private SQL_IOTask m_SqlIOTask;
        private bool m_IsTaskLoaded;
        #endregion Members
        #region Constructors
        public NormMsgFactory_IO(NormMsgFactoryProcess pNormMsgFactoryProcess)
            : base(pNormMsgFactoryProcess)
        {
        }
        #endregion Constructors
        #region Methods
        #region CreateIdInfos
        protected override Cst.ErrLevel CreateIdInfos()
        {
            if (m_IsTaskLoaded)
            {
                m_MQueueAttributes.idInfo = new IdInfo
                {
                    id = m_SqlIOTask.Id,
                    idInfos = new DictionaryEntry[]{
                                                        new DictionaryEntry("ident", "IOTASK"),
                                                        new DictionaryEntry("identifier", m_SqlIOTask.Identifier),
                                                        new DictionaryEntry("displayName", m_SqlIOTask.DisplayName),
                                                        new DictionaryEntry("IN_OUT", m_SqlIOTask.InOut)}
                };
            }
            return Cst.ErrLevel.SUCCESS;
        }
        #endregion CreateIdInfos
        #region CreateParameters
        // EG 20190114 Add detail to ProcessLog Refactoring
        protected override Cst.ErrLevel CreateParameters()
        {
            Cst.ErrLevel codeReturn = Cst.ErrLevel.SUCCESS;

            // Recherche de la tâche à partir de source""
            SQL_Actor css = null;
            if (BuildingInfo.identifierSpecified && BuildingInfo.identifier.StartsWith("source="))
                codeReturn = SetTaskIdentifierFromSource(out css);


            // Recherche de la tâche
            if (0 < BuildingInfo.id)
            {
                m_SqlIOTask = new SQL_IOTask(m_NormMsgFactoryProcess.Cs, BuildingInfo.id);
            }
            else if (BuildingInfo.identifierSpecified)
            {
                m_SqlIOTask = new SQL_IOTask(m_NormMsgFactoryProcess.Cs, SQL_TableWithID.IDType.Identifier, BuildingInfo.identifier, SQL_IOTask.ScanDataDtEnabledEnum.Yes);
            }
            else
            {
                // FI 20200623 [XXXXX] SetErrorWarning
                m_NormMsgFactoryProcess.ProcessState.SetErrorWarning(ProcessStateTools.StatusErrorEnum);

                
                Logger.Log(new LoggerData(LogLevelEnum.Error, new SysMsgCode(SysCodeEnum.SYS, 8009), 2,
                    new LogParam(m_NormMsgFactoryProcess.LogId),
                    new LogParam(m_NormMsgFactoryProcess.LogProcessType)));

                codeReturn = Cst.ErrLevel.FAILURE;
            }

            if (Cst.ErrLevel.SUCCESS == codeReturn)
            {
                m_SqlIOTask.LoadTable(new string[] { "IDIOTASK", "IDENTIFIER", "DISPLAYNAME", "IN_OUT" });
                m_IsTaskLoaded = m_SqlIOTask.IsLoaded;
                if (m_IsTaskLoaded)
                {
                    m_MQueueAttributes = new MQueueAttributes()
                    {
                        connectionString = m_NormMsgFactoryProcess.Cs,
                        id = m_SqlIOTask.Id,
                        identifier = m_SqlIOTask.Identifier
                    };

                    SQL_Entity sqlEntity = LoadParameterEntity();
                    codeReturn = SetMqueueParameters(css, sqlEntity);
                }
                else
                {
                    // FI 20200623 [XXXXX] SetErrorWarning   
                    m_NormMsgFactoryProcess.ProcessState.SetErrorWarning(ProcessStateTools.StatusErrorEnum);

                    // ERROR = Add LOG
                    
                    Logger.Log(new LoggerData(LogLevelEnum.Error, new SysMsgCode(SysCodeEnum.SYS, 8010), 2,
                        new LogParam(LogTools.IdentifierAndId(BuildingInfo.identifier, BuildingInfo.id)),
                        new LogParam(m_NormMsgFactoryProcess.LogId),
                        new LogParam(m_NormMsgFactoryProcess.LogProcessType)));

                    codeReturn = Cst.ErrLevel.FAILURE;
                }
            }
            return codeReturn;
        }
        #endregion CreateParameters
        #region ConstructSendingMQueue
        protected override Cst.ErrLevel ConstructSendingMQueue()
        {
            SetSendingMQueue(new IOMQueue(m_MQueueAttributes));
            
            return Cst.ErrLevel.SUCCESS;
        }
        #endregion ConstructSendingMQueue

        /// <summary>
        /// Retourne la donnée associée à {pCode}
        /// </summary>
        /// <param name="pData"></param>
        /// <returns></returns>
        /// FI 20130429 [] add Method
        private static String GetDataFromListData(string pList, string pCode)
        {
            if (StrFunc.IsEmpty(pCode))
            {
                throw new ArgumentException("pCode Argument is not valid. Empty value is not allowed");
            }

            string ret = null;
            string[] list = pList.Split(";".ToCharArray());
            if (ArrFunc.IsFilled(list))
            {
                foreach (string s in list)
                {
                    string[] listValue = s.Split("=".ToCharArray());
                    if (listValue[0].ToUpper() == pCode.ToUpper())
                    {
                        ret = listValue[1];
                        break;
                    }
                }
            }
            return ret;
        }

        /// <summary>
        /// Retourne la chambre de compensation 
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pCSS">identification de la chambre (Identifier, ou Id)</param>
        /// <param name="pColumn">Liste des colonnes à charger</param>
        /// <returns></returns>
        /// <exception cref="Exception lorsque Spheres® ne parvient pas à identifier la chambre"></exception>
        /// FI 20130429 [] add Method
        private static SQL_Actor LoadCSS(string pCS, string pCSS, string[] pColumn)
        {
            SQL_Actor ret = null;

            SQL_Actor sqlActor = new SQL_Actor(CSTools.SetCacheOn(pCS), pCSS, SQL_Table.RestrictEnum.No,
                SQL_Table.ScanDataDtEnabledEnum.Yes, null, string.Empty );

            sqlActor.LoadTable(pColumn);

            if (sqlActor.IsLoaded)
            {
                ret = sqlActor;
            }
            else if (IntFunc.IsPositiveInteger(pCSS))
            {
                sqlActor = new SQL_Actor(CSTools.SetCacheOn(pCS), SQL_TableWithID.IDType.Id, pCSS, SQL_Table.RestrictEnum.No,
                SQL_Table.ScanDataDtEnabledEnum.Yes, null, string.Empty);
                sqlActor.LoadTable(pColumn);
                if (sqlActor.IsLoaded)
                    ret = sqlActor;
            }

            if (null == ret)
                throw new Exception(StrFunc.AppendFormat("CSS (:{0}) not found", pCSS));

            return ret;
        }

        /// <summary>
        /// Retourne true si la date {pDate} est un jour ouvré sur les Marchés {pIdM} 
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pDate">Représente la date</param>
        /// <param name="pIdM">Représente le marché</param>
        /// <returns></returns>
        /// FI 20130429 [] add Method
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        private static Boolean IsExchangeBusinessDay(string pCS, DateTime pDate, int[] pIdM)
        {
            string[] stringArray = Array.ConvertAll(pIdM, i => i.ToString());

            bool ret = true;
            FpML.Interface.IProductBase product = Tools.GetNewProductBase();
            FpML.Interface.IBusinessCenters bcs = product.LoadBusinessCenters(pCS, null, null, null, stringArray);

            EFS_BusinessCenters efs_bc = new EFS_BusinessCenters(pCS, bcs, null);
            if (efs_bc.businessCentersSpecified)
            {
                //Utiliser DayTypeEnum.ExchangeBusiness => on est en présence d'un BC de marché
                ret = (false == (efs_bc.IsHoliday(pDate, FpML.Enum.DayTypeEnum.ExchangeBusiness)));
            }

            return ret;
        }

        /// <summary>
        /// Retourne la tâche I/O d'importation IO associé à une chambre 
        /// <para>Rertourne  string.Empty s'il n'existe pas de tâche</para>
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pIdACSS">Représente la chambre</param>
        /// <param name="pMode">Type d'importation</param>
        /// <returns></returns>
        /// FI 20130429 [] add Method
        // EG 20180425 Analyse du code Correction [CA2202]
        private static string LoadCssTaskIdentifier(string pCS, int pIdACSS, string pMode)
        {
            string taskIdentifier = string.Empty;
            // PM 20180219 [23824] Ajout tacheExtension
            string tacheExtension = pMode;
            // PM 20180219 [23824] Lorsque pMode vaut "RISKDATA" c'était la tâche "IDIOTASK_RISKDATA" qui était lancé, maintenant ce sera la tâche "IDIOTASK_PRICE"
            if (tacheExtension == "RISKDATA")
            {
                tacheExtension = "PRICE";
            }
            StrBuilder sqlSelect = new StrBuilder(SQLCst.SELECT);
            sqlSelect += "iotask.IDENTIFIER,css.IDIOTASK_" + tacheExtension + Cst.CrLf;
            sqlSelect += SQLCst.FROM_DBO + Cst.OTCml_TBL.ACTOR.ToString() + " a" + Cst.CrLf;
            sqlSelect += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.CSS.ToString() + " css on (css.IDA=a.IDA)" + Cst.CrLf;
            sqlSelect += SQLCst.AND + OTCmlHelper.GetSQLDataDtEnabled(pCS, "css") + Cst.CrLf;
            sqlSelect += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.IOTASK.ToString() + " iotask on (iotask.IDIOTASK=css.IDIOTASK_" + tacheExtension + ")" + Cst.CrLf;
            sqlSelect += SQLCst.AND + OTCmlHelper.GetSQLDataDtEnabled(pCS, "iotask") + Cst.CrLf;
            sqlSelect += SQLCst.WHERE + OTCmlHelper.GetSQLDataDtEnabled(pCS, "a");
            sqlSelect += SQLCst.AND + "(a.IDA=@IDA)";

            DataParameters parameters = new DataParameters();
            parameters.Add(new DataParameter(pCS, "IDA", DbType.Int32), pIdACSS);

            QueryParameters queryParameters = new QueryParameters(pCS, sqlSelect.ToString(), parameters);

            using (IDataReader dr = DataHelper.ExecuteReader(pCS, CommandType.Text, queryParameters.Query, queryParameters.Parameters.GetArrayDbParameter()))
            {
                if (dr.Read())
                    taskIdentifier = Convert.ToString(dr["IDENTIFIER"]);
            }
            return taskIdentifier;
        }

        /// <summary>
        /// Indique si l'on doit importer les données de risque pour les jours fériés sur les marchés d'une chambre
        /// </summary>
        /// <param name="pCS">Chaine de connexion</param>
        /// <param name="pIdACSS">Id de la chambre</param>
        /// <returns>True si les données de risque doivent être importées sinon False</returns>
        /// PM 20150515 [20575] New
        // EG 20180425 Analyse du code Correction [CA2202]
        private static bool IsRiskdataOnMarketHoliday(string pCS, int pIdACSS)
        {
            bool isRiskdataOnHoliday = false;
            StrBuilder sqlSelect = new StrBuilder(SQLCst.SELECT);
            sqlSelect += "css.ISRISKDATAONHOLIDAY" + Cst.CrLf;
            sqlSelect += SQLCst.FROM_DBO + Cst.OTCml_TBL.ACTOR.ToString() + " a" + Cst.CrLf;
            sqlSelect += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.CSS.ToString() + " css on (css.IDA=a.IDA)" + Cst.CrLf;
            sqlSelect += SQLCst.AND + OTCmlHelper.GetSQLDataDtEnabled(pCS, "css") + Cst.CrLf;
            sqlSelect += SQLCst.WHERE + OTCmlHelper.GetSQLDataDtEnabled(pCS, "a");
            sqlSelect += SQLCst.AND + "(a.IDA=@IDA)";

            DataParameters parameters = new DataParameters();
            parameters.Add(new DataParameter(pCS, "IDA", DbType.Int32), pIdACSS);

            QueryParameters queryParameters = new QueryParameters(pCS, sqlSelect.ToString(), parameters);

            using (IDataReader dr = DataHelper.ExecuteReader(pCS, CommandType.Text, queryParameters.Query, queryParameters.Parameters.GetArrayDbParameter()))
            {
                if (dr.Read())
                    isRiskdataOnHoliday = Convert.ToBoolean(dr["ISRISKDATAONHOLIDAY"]);
            }
            return isRiskdataOnHoliday;
        }

        /// <summary>
        ///  Recherce la tâche IO en fonction des éléments présents dans Source 
        ///  <para>Alimente BuildingInfo.identifier</para>
        ///  <param name="pCss">Retourne potentiellement la chambre à laquelle s'applique la tâche</param>
        /// </summary>
        /// FI 20130429 [] add Method
        /// FI 20150108 [XXXXX] Modify
        // EG 20180307 [23769] Gestion dbTransaction
        // EG 20190114 Add detail to ProcessLog Refactoring
        private Cst.ErrLevel SetTaskIdentifierFromSource(out SQL_Actor pCss)
        {
            Cst.ErrLevel errLevel = Cst.ErrLevel.SUCCESS;

            pCss = null;

            if (false == BuildingInfo.identifierSpecified)
                throw new Exception("Argument pBuildingInfoIdentifier does not Starts With 'source='");

            if (false == BuildingInfo.identifier.StartsWith("source="))
                throw new Exception("Argument pBuildingInfoIdentifier does not Starts With 'source='");

            string buildingInfoIdentifier = RetrieveSourceInfo(BuildingInfo.identifier);

            string cs = m_NormMsgFactoryProcess.Cs;
            string csCacheOn = CSTools.SetCacheOn(cs);

            bool isRiskdata = buildingInfoIdentifier.ToUpper().Contains("SRC=RISKDATA");
            bool isRepository = buildingInfoIdentifier.ToUpper().Contains("SRC=REPOSITORY");
            if (isRiskdata || isRepository)
            {
                string mode = GetDataFromListData(buildingInfoIdentifier, "SRC");
                if (StrFunc.IsEmpty(mode))
                    throw new Exception("SRC is not specified in source");

                string css = GetDataFromListData(buildingInfoIdentifier, "CSS");
                if (StrFunc.IsEmpty(css))
                    throw new Exception("CSS is not specified in source");

                string bizdt = GetDataFromListData(buildingInfoIdentifier, "BIZDT");
                if (StrFunc.IsEmpty(bizdt))
                    throw new Exception("BIZDT is not specified in source");

                pCss = LoadCSS(csCacheOn, css, new string[] { "IDA", "IDENTIFIER" });

                string taskIdentifier = LoadCssTaskIdentifier(cs, pCss.Id, mode);
                if (!String.IsNullOrEmpty(taskIdentifier))
                    BuildingInfo.identifier = taskIdentifier;

                //FI 20130430 - PL 20140414
                //Besoin EFS: La date est contrôler uniquement s'il existe des enregistrements dans ENTITYMARKET
                //            de manière à renseigner les Market data d'une base vierge, ou si environnement SPHERES_MARKETDATA. 
                bool isEntityMarketNotFilled_Or_SMD = (!MarketTools.IsEntityMarketFilled(csCacheOn))
                                                   || ((cs.IndexOf("SVR-DB01") > 0) && (cs.IndexOf("SPHERES_MARKETDATA") > 0));
                if (!isEntityMarketNotFilled_Or_SMD)
                {
                    int idAEntity = 0;
                    SQL_Entity sqlEntity = LoadParameterEntity();
                    if (null != sqlEntity)
                        idAEntity = sqlEntity.Id;

                    //Interprétation de la date bizdt
                    string sbusinessDate = ConvertData(cs, bizdt, TypeData.TypeDataEnum.date, idAEntity, 0, pCss.Id);
                    DateTime businessDate = DtFunc.ParseDate(sbusinessDate, DtFunc.FmtISODate, System.Globalization.CultureInfo.InvariantCulture);

                    //FI 20150108 Test existence Marché enabled suite à pb rencontrée sur base sigma où tous les marchés sont disabled
                    //Spheres® plantait avec un message non interprétable
                    int[] idM = MarketTools.CSSGetMarket(csCacheOn, null, pCss.Id, SQL_Table.ScanDataDtEnabledEnum.Yes);
                    if (ArrFunc.IsEmpty(idM))
                    {
                        // FI 20200623 [XXXXX] SetErrorWarning
                        m_NormMsgFactoryProcess.ProcessState.SetErrorWarning(ProcessStateTools.StatusWarningEnum);
                        AddLodNoActivity(BuildingInfo.identifier, pCss.Identifier, businessDate);
                        errLevel = Cst.ErrLevel.NOTHINGTODO;
                    }

                    if (errLevel == Cst.ErrLevel.SUCCESS)
                    {
                        bool isOK = MarketTools.IsCurrentDateBusiness(csCacheOn, businessDate, pCss.Id);
                        //Si cette date est une date bourse en vigueur sur un marché de la chambre 
                        //--> OK: on effectue l'importation.

                        // PM 20150513 [20575]
                        if (isOK)
                        {
                            // Regarder si la date est également en vigeur sur un marché indépendamment des jours traités par les entités
                            isOK = MarketTools.IsCurrentDateMarket(csCacheOn, businessDate, pCss.Id);
                            if (isRiskdata && (isOK == false))
                            {
                                // Regarder s'il faut charger les données de risque pour les jours fériés marchés
                                isOK = IsRiskdataOnMarketHoliday(csCacheOn, pCss.Id);
                            }
                        }

                        if (false == isOK)
                        {
                            //Mise en place d'un message d'erreur ou d'un warning
                            bool isExchangeBusinessDay = IsExchangeBusinessDay(csCacheOn, businessDate, idM);
                            // PM 20150604 [20575] Ajout test sur date en cours sur le marché
                            //if (isExchangeBusinessDay)
                            bool isMarketDay = MarketTools.IsCurrentDateMarket(csCacheOn, businessDate, pCss.Id);
                            if ((false == isMarketDay) && isExchangeBusinessDay)
                            {
                                // EG 20130704
                                // Contrôle existence d'une ligne dans ENTITYMARKET pour la chambre (si aucune = WARNING, sinon ERROR)
                                bool isEntityMarketForCSSFilled = MarketTools.IsEntityMarketForCSSFilled(csCacheOn, pCss.Id);
                                if (isEntityMarketForCSSFilled)
                                {
                                    // Si la journée est ouvrée
                                    //--> ERROR: on n'effectue pas l'importation et on génère une erreur
                                    //<b>Tâche IO <b>{1}</b> non exécutée</b>  
                                    //<b>Détails:</b>  
                                    //- <b>{2}</b> n’est pas une date bourse en vigueur pour la chambre <b>{3}</b>  
                                    //- Identifiant externe de la demande: <b>{4}</b>  
                                    //- Traitement demandé: <b>{5}</b>
                                    // FI 20200623 [XXXXX] SetErrorWarning
                                    m_NormMsgFactoryProcess.ProcessState.SetErrorWarning(ProcessStateTools.StatusErrorEnum);

                                    
                                    Logger.Log(new LoggerData(LogLevelEnum.Error, new SysMsgCode(SysCodeEnum.SYS, 8011), 1,
                                        new LogParam(BuildingInfo.identifier),
                                        new LogParam(DtFunc.DateTimeToStringDateISO(businessDate)),
                                        new LogParam(pCss.Identifier),
                                        new LogParam(m_NormMsgFactoryProcess.LogId),
                                        new LogParam(m_NormMsgFactoryProcess.LogProcessType)));

                                    errLevel = Cst.ErrLevel.FAILURE;
                                }
                                else
                                {
                                    // FI 20200623 [XXXXX] SetErrorWarning
                                    m_NormMsgFactoryProcess.ProcessState.SetErrorWarning(ProcessStateTools.StatusWarningEnum);
                                    AddLodNoActivity(BuildingInfo.identifier, pCss.Identifier, businessDate);
                                    errLevel = Cst.ErrLevel.NOTHINGTODO;
                                }
                            }
                            else
                            {
                                // FI 20200623 [XXXXX] SetErrorWarning
                                m_NormMsgFactoryProcess.ProcessState.SetErrorWarning(ProcessStateTools.StatusWarningEnum);

                                // Si cette date est une date fériée sur un marché de la chambre 
                                //--> WARNING: on n'effectue pas l'importation et on génère un simple warning 
                                //<b>Tâche IO <b>{1}</b> non exécutée</b>  
                                //<b>Détails:</b>  
                                //- <b>{2}</b> n’est pas une date ouvrée pour la chambre <b>{3}</b>  
                                //- Identifiant externe de la demande: <b>{4}</b>  - Traitement demandé: <b>{5}</b>
                                
                                Logger.Log(new LoggerData(LogLevelEnum.Warning, new SysMsgCode(SysCodeEnum.LOG, 8010), 1,
                                    new LogParam(BuildingInfo.identifier),
                                    new LogParam(DtFunc.DateTimeToStringDateISO(businessDate)),
                                    new LogParam(pCss.Identifier),
                                    new LogParam(m_NormMsgFactoryProcess.LogId),
                                    new LogParam(m_NormMsgFactoryProcess.LogProcessType)));

                                errLevel = Cst.ErrLevel.NOTHINGTODO;
                            }
                        }
                    }
                }
            }
            return errLevel;
        }
        
        /// <summary>
        /// Alimente le log avec un warning =>  Aucune activité pour la chambre
        /// </summary>
        /// <param name="pTaskIdentifier"></param>
        /// <param name="pCssIdentifier"></param>
        /// <param name="pBusinessDate"></param>
        /// FI 20150108 [XXXXX] Add Method
        // EG 20190114 Add detail to ProcessLog Refactoring
        private void AddLodNoActivity(string pTaskIdentifier, string pCssIdentifier, DateTime pBusinessDate)
        {

            //<b>Tâche IO <b>{1}</b> non exécutée</b>  
            //<b>Détails:</b>  
            //- Aucune activité pour la chambre <b>{2}</b>  
            //- Journée de bourse demandée: <b>{3}</b>  
            //- Identifiant externe de la demande: <b>{4}</b>  
            //- Traitement demandé: <b>{5}</b>
            
            Logger.Log(new LoggerData(LogLevelEnum.Warning, new SysMsgCode(SysCodeEnum.LOG, 8011), 1,
                new LogParam(pTaskIdentifier),
                new LogParam(pCssIdentifier),
                new LogParam(DtFunc.DateTimeToStringDateISO(pBusinessDate)),
                new LogParam(m_NormMsgFactoryProcess.LogId),
                new LogParam(m_NormMsgFactoryProcess.LogProcessType)));
        }


        /// <summary>
        /// Recupère ce qui se trouve à l'intérieur de source=""
        /// </summary>
        /// <param name="pSourceInfo"></param>
        /// FI 20130429 [] add Method
        private static string RetrieveSourceInfo(string pSourceInfo)
        {
            string ret = pSourceInfo;

            if (false == pSourceInfo.StartsWith(@"source="""))
                throw new Exception(@"Argument pBuildingInfoIdentifier does not Starts With 'source=""'");
            if (false == pSourceInfo.EndsWith(@""""))
                throw new Exception(@"Argument pBuildingInfoIdentifier does not end With '""'");

            ret = ret.Replace(@"source=""", string.Empty);
            ret = ret.Remove(ret.LastIndexOf(@""""));

            return ret;
        }

        /// <summary>
        /// Valorisation des paramètres de la tâche
        /// <para>Les paramètres sont chargés à partir du paramétrage de la tâche</para>
        /// <para>Les paramètres sont valorisés avec le contenu des paramètres présents dans le msg NormMsgFactory</para>
        /// </summary>
        /// <param name="pCSS">Représente une chambre de compensation (valeur null possible)</param>
        /// <param name="pEntity">Représente une entité (valeur null possible)</param>
        /// FI 20130430 [] Add Method
        // EG 20190114 Add detail to ProcessLog Refactoring
        private Cst.ErrLevel SetMqueueParameters(SQL_Actor pCSS, SQL_Entity pEntity)
        {
            Cst.ErrLevel ret = Cst.ErrLevel.SUCCESS;

            int idACss = 0;
            if (null != pCSS)
                idACss = pCSS.Id;

            int idAEntity = 0;
            if (null != pEntity)
                idAEntity = pEntity.Id;

            SQL_IOTaskParams taskParams = new SQL_IOTaskParams(m_NormMsgFactoryProcess.Cs, m_SqlIOTask.Id);
            DataRow[] drParams = taskParams.Select();

            m_MQueueAttributes.parameters = GetIOMQueueParameters(drParams);

            if (null != m_MQueueAttributes.parameters)
            {
                foreach (MQueueparameter parameter in m_MQueueAttributes.parameters.parameter)
                {
                    // Un paramètre est considéré véritablement comme obligatoire: s'il est obligatoire et s'il n'a pas de valeur par défault
                    if (BuildingInfo.parametersSpecified)
                    {
                        if (BuildingInfo.parameters[parameter.id] is MQueueparameter buildingInfo_parameter)
                        {
                            //Il existe un paramètre de même nom dans le message source: on retient sa valeur
                            string value = base.ConvertData(m_NormMsgFactoryProcess.Cs, buildingInfo_parameter.Value, buildingInfo_parameter.dataType, idAEntity, 0, idACss);

                            parameter.SetValue(value);
                            parameter.ExValueSpecified = StrFunc.IsFilled(buildingInfo_parameter.ExValue);
                            if (parameter.ExValueSpecified)
                                parameter.ExValue = buildingInfo_parameter.ExValue;
                        }
                        else if (parameter.isMandatory)
                        {
                            //Il n'existe pas de paramètre de même nom dans le message source, et ce paramètre est obligatoire et sans valeur par défaut (voir GetIOMQueueParameters): on génère une erreur
                            // FI 20200623 [XXXXX] SetErrorWarning
                            m_NormMsgFactoryProcess.ProcessState.SetErrorWarning(ProcessStateTools.StatusErrorEnum);

                            
                            Logger.Log(new LoggerData(LogLevelEnum.Error, new SysMsgCode(SysCodeEnum.SYS, 8001), 2,
                                new LogParam(parameter.id),
                                new LogParam(m_NormMsgFactoryProcess.LogId),
                                new LogParam(m_NormMsgFactoryProcess.LogProcessType)));

                            ret = Cst.ErrLevel.FAILURE;
                        }
                    }
                }
            }
            return ret;
        }




        #region GetIOMQueueParameters
        private static MQueueparameters GetIOMQueueParameters(DataRow[] pDrParams)
        {
            MQueueparameters parameters = null;
            if (ArrFunc.IsFilled(pDrParams))
            {
                parameters = new MQueueparameters();
                foreach (DataRow drParam in pDrParams)
                {
                    MQueueparameter parameter = new MQueueparameter(drParam["IDIOPARAMDET"].ToString(),
                                                    drParam["DISPLAYNAME"].ToString(),
                                                    drParam["DISPLAYNAME"].ToString(),
                                                    (TypeData.TypeDataEnum)Enum.Parse(typeof(TypeData.TypeDataEnum),
                                                    drParam["DATATYPE"].ToString(), true))
                    {
                        direction = drParam["DIRECTION"].ToString()
                    };
                    if (DataHelper.IsParamDirectionOutput(parameter.direction) ||
                        DataHelper.IsParamDirectionInputOutput(parameter.direction))
                    {
                        if (StrFunc.IsFilled(drParam["RETURNTYPE"].ToString()))
                            parameter.ReturnType = (Cst.ReturnSPParamTypeEnum)Enum.Parse(typeof(Cst.ReturnSPParamTypeEnum),
                                drParam["RETURNTYPE"].ToString(), true);
                    }
                    parameter.isMandatory = Convert.ToBoolean(drParam["ISMANDATORY"]);
                    #region Set parameter Value
                    string _value = Convert.ToString(drParam["DEFAULTVALUE"]);
                    parameter.isMandatory &= StrFunc.IsEmpty(_value);
                    try
                    {
                        switch (parameter.dataType)
                        {
                            case TypeData.TypeDataEnum.@bool:
                            case TypeData.TypeDataEnum.boolean:
                                parameter.SetValue(BoolFunc.IsTrue(_value));
                                break;
                            case TypeData.TypeDataEnum.date:
                            case TypeData.TypeDataEnum.datetime:
                            case TypeData.TypeDataEnum.time:
                                DateTime dtValue = new DtFunc().StringToDateTime(_value);
                                if (DtFunc.IsDateTimeFilled(dtValue))
                                    parameter.SetValue(dtValue);
                                break;
                            case TypeData.TypeDataEnum.integer:
                                parameter.SetValue(Convert.ToInt32(_value));
                                break;
                            case TypeData.TypeDataEnum.@decimal:
                                parameter.SetValue(Convert.ToDecimal(_value));
                                break;
                            default:
                                parameter.SetValue(_value);
                                break;
                        }
                    }
                    catch (Exception)
                    {
                        // Fonctions qui seront interprétés dans les process
                        if (StrFunc.ContainsIn(_value.ToUpper(), "SPHERESLIB") || StrFunc.ContainsIn(_value.ToUpper(), "SQL"))
                            parameter.Value = _value;
                    }
                    #endregion Set parameter Value
                    parameters.Add(parameter);
                }
            }
            return parameters;
        }
        #endregion GetIOMQueueParameters





        #endregion Methods
    }
}
