#region Using Directives
//
using EFS.ACommon;
using EFS.ApplicationBlocks.Data;
using EFS.Common;
using EFS.Common.Log;
using EFS.Common.MQueue;
using EFS.LoggerClient;
using EFS.LoggerClient.LoggerService;
//
using EfsML.CorporateActions;
using System;
using System.Collections.Generic;
using System.Data;
#endregion Using Directives

namespace EFS.Process
{
    #region NormMsgFactory_CORPOACTIONINTEGRATE
    /// <summary>
    /// Construction Messages de type : NORMMSGFACTORY
    /// 
    /// DONNEES attendues en entrée : 
    ///<para>────────────────────────────────────────────────────────────────────────────────────────────────</para>
    ///<para>  ● processType : CORPOACTIONINTEGRATE</para>
    ///<para>  ● id          : Id de la table CORPOACTIONISSUE (IDCAISSUE)</para>
    ///<para>  ● identifier  : Identifiant de la corporate action dans la table CORPOACTIONISSUE</para>
    ///<para>────────────────────────────────────────────────────────────────────────────────────────────────</para>
    ///
    /// PARAMETRES attendus en entrée : les caractéristiques de la corporate action publiée par le marché
    ///<para>────────────────────────────────────────────────────────────────────────────────────────────────</para>
    ///<para>  ● Données générale de la CA</para>
    ///<para>  ● Données liées au(x) sous-jacent(s) concerné(s)</para>
    ///<para>  ● Données liées à la méthode d'ajustement (composants statiques, règles d'arrondi...) </para>
    ///<para>────────────────────────────────────────────────────────────────────────────────────────────────</para> 
    ///
    /// Le service NormMsgFactory ne prépare pas un message pour un autre service applicatif,  
    /// il se charge directement de l'intégration ou de la modification d'une CA dans la table CORPOACTION.
    ///<para>────────────────────────────────────────────────────────────────────────────────────────────────</para>
    ///<para>  ● Recherche de la CA (table CORPOACTIONISSUE)</para>
    ///<para>  ● Contrôle exitence ou non de cette CA (table CORPOACTION)</para>
    ///<para>  ● recherche et implémentation des identifiants (MARCHE, SOUS-JACENTS ...)</para>
    ///<para>  ● Création/MAJ (table CORPOACTION)</para>
    ///<para>────────────────────────────────────────────────────────────────────────────────────────────────</para> 
    ///</summary>
    public class NormMsgFactory_CORPOACTIONINTEGRATE : NormMsgFactoryBase
    {
        #region Members
        private CorporateAction m_CorporateActionIssue;
        private CorporateAction m_CorporateActionEmbedded;
        private readonly string m_CATemplatePath;
        protected readonly string m_CATemplatePath_AI;

        private readonly CorporateActionReadyStateEnum _rsPublishedOrReserved = (CorporateActionReadyStateEnum.PUBLISHED | CorporateActionReadyStateEnum.RESERVED);


        #endregion Members
        #region Constructors
        public NormMsgFactory_CORPOACTIONINTEGRATE(NormMsgFactoryProcess pNormMsgFactoryProcess)
            : base(pNormMsgFactoryProcess)
        {
            m_CATemplatePath = m_NormMsgFactoryProcess.AppInstance.MapPath(@"CorporateActions\Templates\");
            m_CATemplatePath_AI = m_NormMsgFactoryProcess.AppInstance.MapPath(@"CorporateActions\Templates\Additionals\");
        }
        #endregion Constructors
        #region Methods
        #region Generate
        // EG 20140518 [19913]
        // EG 20190114 Add detail to ProcessLog Refactoring
        public override Cst.ErrLevel Generate()
        {
            // PM 20210121 [XXXXX] Passage du message au niveau de log None
            Logger.Log(new LoggerData(LogLevelEnum.None, new SysMsgCode(SysCodeEnum.LOG, 8020), 0,
                new LogParam(LogTools.IdentifierAndId(BuildingInfo.identifier, BuildingInfo.id))));

            // Log : Lectures de paramètres et Alimentation de la classe Corporate actions
            Logger.Log(new LoggerData(LogLevelEnum.Info, new SysMsgCode(SysCodeEnum.LOG, 8021), 1,
                new LogParam(LogTools.IdentifierAndId(BuildingInfo.identifier, BuildingInfo.id))));

            int idA = Convert.ToInt32(m_NormMsgFactoryProcess.MQueue.header.requester.idA);
            CAInfo _caInfoIssue = new CAInfo(idA, "IDCAISSUE", m_NormMsgFactoryProcess.MQueue.id.ToString());

            Cst.ErrLevel ret = SetCorporateActionClass();
            if (Cst.ErrLevel.SUCCESS == ret)
            {
                CAInfo _caInfoEmbedded = new CAInfo(idA, "IDCA", string.Empty);

                bool isExist = m_CorporateActionIssue.Exist(_caInfoEmbedded, CATools.CAWhereMode.NOTICE);
                if (isExist)
                {
                    // Une Corporate action existe déjà avec cette référence de notice.
                    // Un contrôle de cohérence est effectué entre les deux CAs
                    ret = CAEmbeddedLoadAndCompare(_caInfoEmbedded);

                    if (Cst.ErrLevel.SUCCESS == ret)
                    {
                        m_CorporateActionIssue.IdCA = m_CorporateActionEmbedded.IdCA;
                        // EG 20140518 [19913] lien IDCAISSUE entre CORPOACTION et CORPOACTIONISSUE
                        m_CorporateActionIssue.idCAIssue = _caInfoIssue.Id.Value;
                        m_CorporateActionIssue.IdA = idA;
                        m_CorporateActionIssue.corporateEvent[0].IdCE = m_CorporateActionEmbedded.corporateEvent[0].IdCE;
                        _caInfoEmbedded.Id = m_CorporateActionIssue.IdCA;
                        _caInfoEmbedded.Mode = Cst.Capture.ModeEnum.Update;
                    }
                }
                else
                {
                    Logger.Log(new LoggerData(LogLevelEnum.Info, new SysMsgCode(SysCodeEnum.LOG, 8023), 2,
                        new LogParam(LogTools.IdentifierAndId(BuildingInfo.identifier, BuildingInfo.id))));
                }
                if (Cst.ErrLevel.SUCCESS == ret)
                {
                    CorporateActionReadyStateEnum readystateIssue = m_CorporateActionIssue.readystate;

                    Logger.Log(new LoggerData(LogLevelEnum.Info, new SysMsgCode(SysCodeEnum.LOG, 8024), 2,
                        new LogParam(LogTools.IdentifierAndId(BuildingInfo.identifier, BuildingInfo.id))));

                    if ((_rsPublishedOrReserved & readystateIssue) == readystateIssue)
                        m_CorporateActionIssue.readystate = CorporateActionReadyStateEnum.EMBEDDED;

                    #region Script before embedding
                    // EG 20140518 [19913] Exécution du script avant intégration
                    if (m_CorporateActionIssue.corporateDocsSpecified)
                    {
                        
                        // EG 20211109 [XXXXX] Changement de signature (usage de ProcessBase)
                        ret = CATools.ExecCorporateActionSQLScript(m_NormMsgFactoryProcess, m_NormMsgFactoryProcess.LogProcessType, 
                            m_CorporateActionIssue, CATools.SQLRunTimeEnum.EMBEDDED, m_CATemplatePath_AI);
                    }
                    #endregion Script before embedding

                    // EG 20140518 [19913] Add _caInfoIssue.Id FK de CORPOACTION vers CORPOACTIONISSUE
                    if (Cst.ErrLevel.SUCCESS == ret)
                        ret = m_CorporateActionIssue.Write(_caInfoEmbedded, _caInfoIssue.Id);

                    if (CorporateActionReadyStateEnum.RESERVED == readystateIssue)
                        m_CorporateActionIssue.readystate = readystateIssue;
                }
            }
            // Mise à jour des statuts sur la table de publication
            if (null != m_CorporateActionIssue)
            {
                m_CorporateActionIssue.SetEmbeddedState(_caInfoIssue, ret);

                // FI 20200623 [XXXXX] SetErrorWarning
                m_NormMsgFactoryProcess.ProcessState.SetErrorWarning(m_CorporateActionIssue.GetStatus());
                
                Logger.Log(new LoggerData(LoggerTools.StatusToLogLevelEnum(m_CorporateActionIssue.GetStatus()), new SysMsgCode(SysCodeEnum.LOG, 8025), 1,
                    new LogParam(LogTools.IdentifierAndId(BuildingInfo.identifier, BuildingInfo.id)),
                    new LogParam(m_CorporateActionIssue.embeddedState)));

                m_CorporateActionIssue.UpdateStatus(_caInfoIssue);
            }

            return ret;
        }
        #endregion Generate
        #region IsExistEntityMarket
        /// <summary>
        /// Il existe une ligne ENTITYMARKET (ETD) pour le marché de la CA à intégrer
        /// </summary>
        /// <returns></returns>
        private bool IsExistEntityMarket(int pIdM)
        {
            bool isExist = false;
            DataParameters parameters = new DataParameters();
            parameters.Add(new DataParameter(m_NormMsgFactoryProcess.Cs, "IDM", DbType.Int32), pIdM);
            string sqlQuery = @"select 1 
            from dbo.ENTITYMARKET em 
            where (em.IDM = @IDM) and (em.IDA_CUSTODIAN is null)" + Cst.CrLf;
            QueryParameters query = new QueryParameters(m_NormMsgFactoryProcess.Cs, sqlQuery, parameters);
            object obj = DataHelper.ExecuteScalar(m_NormMsgFactoryProcess.Cs, CommandType.Text, query.Query, query.Parameters.GetArrayDbParameter());
            if (null != obj)
                isExist = Convert.ToBoolean(obj);
            return isExist;
        }
        #endregion IsExistEntityMarket
        #region CAEmbeddedLoadAndCompare
        /// <summary>
        ///<para>────────────────────────────────────────────────────────────────────────────────────────────────</para>
        ///<para>  ● Chargement de la corporate action déjà intégrée (présence dans la table CORPOACTION)</para>
        ///<para>  ● Autorisation de mise à jour</para>
        ///<para>  ● Comparaison des caractéristiques de la CA intégrée avec celles du nouveau message</para>
        ///<para>────────────────────────────────────────────────────────────────────────────────────────────────</para>
        /// </summary>
        /// <param name="pCAInfoTarget"></param>
        /// <returns></returns>
        // EG 20190114 Add detail to ProcessLog Refactoring
        private Cst.ErrLevel CAEmbeddedLoadAndCompare(CAInfo pCAInfoTarget)
        {
            m_CorporateActionEmbedded = new CorporateAction(m_NormMsgFactoryProcess.Cs)
            {
                cfiCodeSpecified = m_CorporateActionIssue.cfiCodeSpecified,
                cfiCode = m_CorporateActionIssue.cfiCode,
                market = new MarketIdentification()
                {
                    spheresid = m_CorporateActionIssue.market.spheresid
                },
                refNotice = new RefNoticeIdentification()
                {
                    value = m_CorporateActionIssue.refNotice.value
                }
            };
            Cst.ErrLevel ret = m_CorporateActionEmbedded.Load(pCAInfoTarget, CATools.CAWhereMode.NOTICE, m_CATemplatePath);
            if (Cst.ErrLevel.SUCCESS == ret)
            {
                
                Logger.Log(new LoggerData(LogLevelEnum.Info, new SysMsgCode(SysCodeEnum.LOG, 8022), 2,
                    new LogParam(LogTools.IdentifierAndId(BuildingInfo.identifier, BuildingInfo.id)),
                    new LogParam(LogTools.IdentifierAndId(m_CorporateActionEmbedded.identifier, m_CorporateActionEmbedded.IdCA))));

                #region Mise à jour des données du sous-jacent

                #endregion Mise à jour des données du sous-jacent
                #region Autorisation de mise à jour
                // La CA ne peut être modifiée si elle est déjà exécutée (Mode EOD ou SOD) 
                // Il existe au moins une ligne dans CORPOEVENTCONTRACT avec READYSTATE = EXECUTED
                bool isExist = false;
                DCQueryEMBEDDED _dcQry = new DCQueryEMBEDDED(m_NormMsgFactoryProcess.Cs);
                QueryParameters _dcQryParameters = _dcQry.GetQueryExist(CATools.DCWhereMode.IDCE_READYSTATE);
                _dcQryParameters.Parameters["IDCE"].Value = m_CorporateActionEmbedded.corporateEvent[0].IdCE;
                _dcQryParameters.Parameters["READYSTATE"].Value = CorporateEventReadyStateEnum.EXECUTED;
                object obj = DataHelper.ExecuteScalar(m_NormMsgFactoryProcess.Cs, CommandType.Text, _dcQryParameters.Query, _dcQryParameters.Parameters.GetArrayDbParameter());
                if (null != obj)
                    isExist = BoolFunc.IsTrue(obj);

                if (isExist)
                {
                    // FI 20200623 [XXXXX] SetErrorWarning
                    m_NormMsgFactoryProcess.ProcessState.SetErrorWarning(ProcessStateTools.StatusWarningEnum);

                    // Corporate action action déjà exécutée (partiellement ou non) donc mise à jour rejetée
                    Logger.Log(new LoggerData(LogLevelEnum.Warning, new SysMsgCode(SysCodeEnum.SYS, 8035), 2,
                        new LogParam(LogTools.IdentifierAndId(BuildingInfo.identifier, BuildingInfo.id)),
                        new LogParam(LogTools.IdentifierAndId(m_CorporateActionEmbedded.identifier, m_CorporateActionEmbedded.IdCA))));

                    // EG 20140107 [] NOTHINGTODO replace DATAREJECTED
                    //ret = Cst.ErrLevel.DATAREJECTED;
                    ret = Cst.ErrLevel.NOTHINGTODO;
                }

                #endregion Autorisation de mise à jour
            }
            return ret;
        }
        #endregion CAEmbeddedLoadAndCompare
        #region SetMarketClass
        // EG [33415/33420]
        // PL 20171006 [23469] Original MARKETTYPE deprecated
        private void SetMarketClass(MarketIdentification pMarket, SQL_Market pSQLMarket)
        {
            pMarket.acronymSpecified = StrFunc.IsFilled(pSQLMarket.Acronym);
            pMarket.acronym = pSQLMarket.Acronym;
            pMarket.descriptionSpecified = StrFunc.IsFilled(pSQLMarket.Description);
            pMarket.description = pSQLMarket.Description;
            pMarket.displaynameSpecified = StrFunc.IsFilled(pSQLMarket.DisplayName);
            pMarket.displayname = pSQLMarket.DisplayName;
            pMarket.identifierSpecified = StrFunc.IsFilled(pSQLMarket.Identifier);
            pMarket.identifier = pSQLMarket.Identifier;
            pMarket.exchangeSymbolSpecified = StrFunc.IsFilled(pSQLMarket.ExchangeSymbol);
            pMarket.exchangeSymbol = pSQLMarket.ExchangeSymbol;
            //pMarket.marketType = (Cst.MarketTypeEnum)Enum.Parse(typeof(Cst.MarketTypeEnum), pSQLMarket.MarketType);
            pMarket.marketType = pSQLMarket.MarketType;
            pMarket.shortIdentifierSpecified = StrFunc.IsFilled(pSQLMarket.ShortIdentifier);
            pMarket.shortIdentifier = pSQLMarket.ShortIdentifier;
            // EG [33415/33420]
            pMarket.IdM = pSQLMarket.Id;
        }
        #endregion SetMarketClass
        #region SetUnderlyerClass
        // EG 20190114 Add detail to ProcessLog Refactoring
        private Cst.ErrLevel SetUnderlyerClass(CorporateEventUnderlyer pUnderlyer, int pIdM_CA)
        {
            Cst.ErrLevel ret = Cst.ErrLevel.SUCCESS;
            if (pUnderlyer.marketSpecified)
            {
                // PL 20171006 [23469] Original MARKETTYPE deprecated
                SQL_Market sqlUNLMarket = new SQL_Market(m_NormMsgFactoryProcess.Cs, SQL_TableWithID.IDType.FIXML_SecurityExchange, pUnderlyer.market.FIXML_SecurityExchange, SQL_Table.ScanDataDtEnabledEnum.Yes);
                sqlUNLMarket.LoadTable(new string[] { "IDM, IDENTIFIER, DISPLAYNAME, DESCRIPTION, ACRONYM, IDMOPERATING, EXCHANGESYMBOL, SHORTIDENTIFIER" });

                if (sqlUNLMarket.IsLoaded)
                {
                    SetMarketClass(pUnderlyer.market, sqlUNLMarket);
                    SQL_AssetBase sqlAsset = null;

                    List<Pair<SQL_TableWithID.IDType, string>> _underlyerCode = pUnderlyer.UnderlyerCode;
                    if (null != _underlyerCode) 
                    {
                        bool _isFound = false;
                        _underlyerCode.ForEach(item =>
                        {
                            if (SQL_TableWithID.IDType.UNDEFINED != item.First)
                            {
                                switch (pUnderlyer.category)
                                {
                                    case Cst.UnderlyingAsset_ETD.EquityAsset:
                                        #region ASSET EQUITY
                                        sqlAsset = new SQL_AssetEquity(m_NormMsgFactoryProcess.Cs, item.First, item.Second);
                                        ((SQL_AssetEquity)sqlAsset).IdM_In = sqlUNLMarket.Id;
                                        if (false == sqlAsset.IsLoaded)
                                        {
                                            // Si non trouvé = Recherche avec ASSET_EQUITY_RDCMK (Future)
                                            ((SQL_AssetEquity)sqlAsset).IdM_In = 0;
                                            ((SQL_AssetEquity)sqlAsset).IdMRelated_In = pIdM_CA;
                                        }
                                        if (false == sqlAsset.IsLoaded)
                                        {
                                            // Si non trouvé = Recherche avec ASSET_EQUITY_RDCMK (Option)
                                            ((SQL_AssetEquity)sqlAsset).IdM_In = 0;
                                            ((SQL_AssetEquity)sqlAsset).IdMRelated_In = 0;
                                            ((SQL_AssetEquity)sqlAsset).IdMOption_In = pIdM_CA;
                                        }

                                        if (sqlAsset.IsLoaded)
                                        {
                                            pUnderlyer.identifierSpecified = StrFunc.IsFilled(sqlAsset.Identifier);
                                            pUnderlyer.identifier = sqlAsset.Identifier;
                                            pUnderlyer.displaynameSpecified = StrFunc.IsFilled(sqlAsset.DisplayName);
                                            pUnderlyer.displayname = sqlAsset.DisplayName;
                                            pUnderlyer.descriptionSpecified = StrFunc.IsFilled(sqlAsset.Description);
                                            pUnderlyer.description = sqlAsset.Description;
                                            pUnderlyer.spheresid = sqlAsset.Id.ToString();
                                            pUnderlyer.isinCodeSpecified = StrFunc.IsFilled(((SQL_AssetEquity)sqlAsset).ISINCode);
                                            pUnderlyer.isinCode = ((SQL_AssetEquity)sqlAsset).ISINCode;
                                            _isFound = true;
                                        }
                                        #endregion ASSET EQUITY
                                        break;
                                    default:
                                        // Catégorie de sous-jacent non gérée
                                        // FI 20200623 [XXXXX] SetErrorWarning
                                        m_NormMsgFactoryProcess.ProcessState.SetErrorWarning(ProcessStateTools.StatusErrorEnum);

                                        Logger.Log(new LoggerData(LogLevelEnum.Error, new SysMsgCode(SysCodeEnum.SYS, 8034), 2,
                                            new LogParam(pUnderlyer.category.ToString() + " (UNLCATEGORY)"),
                                            new LogParam(LogTools.IdentifierAndId(BuildingInfo.identifier, BuildingInfo.id))));

                                        ret = Cst.ErrLevel.DATANOTFOUND;
                                        break;
                                }
                            }

                        });

                        if (false == _isFound)
                        {
                            if (pUnderlyer.category == Cst.UnderlyingAsset_ETD.EquityAsset)
                            {
                                // Sous-jacent non trouvé!!!
                                // FI 20200623 [XXXXX] SetErrorWarning
                                m_NormMsgFactoryProcess.ProcessState.SetErrorWarning(ProcessStateTools.StatusErrorEnum);

                                Logger.Log(new LoggerData(LogLevelEnum.Error, new SysMsgCode(SysCodeEnum.SYS, 8032), 2,
                                    new LogParam(pUnderlyer.market.FIXML_SecurityExchange + "-" +
                                        LogTools.IdentifierAndId(pUnderlyer.caIssueCode, pUnderlyer.category.ToString())),
                                    new LogParam(LogTools.IdentifierAndId(BuildingInfo.identifier, BuildingInfo.id))));
                            }
                            else
                            {
                                // FI 20200623 [XXXXX] SetErrorWarning
                                m_NormMsgFactoryProcess.ProcessState.SetErrorWarning(ProcessStateTools.StatusErrorEnum);
                                // Catégorie de sous-jacent non gérée
                                Logger.Log(new LoggerData(LogLevelEnum.Error, new SysMsgCode(SysCodeEnum.SYS, 8034), 2,
                                    new LogParam(pUnderlyer.category.ToString() + " (UNLCATEGORY)"),
                                    new LogParam(LogTools.IdentifierAndId(BuildingInfo.identifier, BuildingInfo.id))));
                            }
                            ret = Cst.ErrLevel.DATANOTFOUND;
                        }
                    }
                    else
                    {
                        // FI 20200623 [XXXXX] SetErrorWarning
                        m_NormMsgFactoryProcess.ProcessState.SetErrorWarning(ProcessStateTools.StatusErrorEnum);

                        // Critère de recherche du sous-jacent incorrect!!!
                        Logger.Log(new LoggerData(LogLevelEnum.Error, new SysMsgCode(SysCodeEnum.SYS, 8034), 2,
                            new LogParam(pUnderlyer.market.FIXML_SecurityExchange + "-" +
                                LogTools.IdentifierAndId(pUnderlyer.caIssueCode, pUnderlyer.category.ToString())),
                            new LogParam(LogTools.IdentifierAndId(BuildingInfo.identifier, BuildingInfo.id))));

                        ret = Cst.ErrLevel.DATANOTFOUND;
                    }

                }
            }
            else
            {
                // Marché du sous-jacent non spécifié
                ret = Cst.ErrLevel.DATANOTFOUND;
            }
            return ret;
        }
        #endregion SetUnderlyerClass
        #region SetCorporateActionClass
        /// <summary>
        /// <para>────────────────────────────────────────────────────────────────────────────────────────────────</para>
        /// Alimentation de la classe CorporateAction via les paramètres présents dans le fichier de type NORMMSGFACTORY
        /// Compléments pour MARCHE et SOUS-JACENTS
        /// <para>────────────────────────────────────────────────────────────────────────────────────────────────</para>
        /// </summary>
        /// <returns></returns>
        // EG 20140518 [19913]
        // EG 20190114 Add detail to ProcessLog Refactoring
        protected Cst.ErrLevel SetCorporateActionClass()
        {
            Cst.ErrLevel ret = Cst.ErrLevel.SUCCESS;
            try
            {
                m_CorporateActionIssue = new CorporateAction(m_NormMsgFactoryProcess.Cs);

                #region SQL Scripts
                // EG 20140518 [19913] 
                m_CorporateActionIssue.corporateDocsSpecified = BuildingInfo.scriptsSpecified;
                if (m_CorporateActionIssue.corporateDocsSpecified)
                {
                    m_CorporateActionIssue.corporateDocs = new CorporateDocs();
                    MQueueScripts scripts = BuildingInfo.scripts;
                    for (int i = 0; i < scripts.script.Length; i++)
                    {
                        MQueueScript _script = scripts.script[i];
                        CorporateDoc _corporateDoc = new CorporateDoc()
                        {
                            docType = (CATools.DOCTypeEnum)CATools.CADocType(_script.docType),
                            runTime = (CATools.SQLRunTimeEnum)CATools.CADocRunTime(_script.runTime),
                            script = _script.script,
                            identifierSpecified = StrFunc.IsFilled(_script.docName)
                        };
                        if (_corporateDoc.identifierSpecified)
                            _corporateDoc.identifier = _script.docName;

                        Pair<CATools.DOCTypeEnum, CATools.SQLRunTimeEnum> _key = new Pair<CATools.DOCTypeEnum, CATools.SQLRunTimeEnum>(_corporateDoc.docType, _corporateDoc.runTime);
                        m_CorporateActionIssue.corporateDocs.Add(_key, _corporateDoc);
                    }
                }
                #endregion SQL Scripts

                #region Script before control embedding
                // EG 20141024 Exécution du script avant les contrôles d'intégration
                if (m_CorporateActionIssue.corporateDocsSpecified)
                {
                    
                    // EG 20211109 [XXXXX] Changement de signature (usage de ProcessBase)
                    ret = CATools.ExecCorporateActionSQLScript(m_NormMsgFactoryProcess, m_NormMsgFactoryProcess.LogProcessType,
                        m_CorporateActionIssue, CATools.SQLRunTimeEnum.CONTROL, m_CATemplatePath_AI, BuildingInfo.identifier, BuildingInfo.parameters);
                }
                #endregion Script before embedding

                if (Cst.ErrLevel.SUCCESS == ret)
                {
                    MQueueparameter parameter = BuildingInfo.parameters["CAMARKET"];
                    #region Parameters
                    if (null != parameter)
                    {
                        string _FIXML_SecurityExchange = parameter.Value;
                        SQL_Market sqlMarket = new SQL_Market(m_NormMsgFactoryProcess.Cs, SQL_TableWithID.IDType.FIXML_SecurityExchange, _FIXML_SecurityExchange, SQL_Table.ScanDataDtEnabledEnum.Yes);

                        // PL 20171006 [23469] Original MARKETTYPE deprecated
                        // RD 20180518 [33881] FIXML_SecurityExchange is missing
                        sqlMarket.LoadTable(new string[] { "IDM, IDENTIFIER, DISPLAYNAME, DESCRIPTION, ACRONYM, IDMOPERATING, EXCHANGESYMBOL, SHORTIDENTIFIER, ISO10383_ALPHA4, FIXML_SecurityExchange" });
                        if (sqlMarket.IsLoaded)
                        {
                            // EG 20141113 Contrôle Marché géré (présence dans ENTITYMARKET)
                            if (IsExistEntityMarket(sqlMarket.Id))
                            {
                                #region Corporate Action
                                List<string> _missingParameters = m_CorporateActionIssue.SetNormMsgFactoryParameters(sqlMarket.Id, BuildingInfo.parameters);
                                #endregion Corporate Action
                                if (0 == _missingParameters.Count)
                                {
                                    #region Compléments MARCHE
                                    SetMarketClass(m_CorporateActionIssue.market, sqlMarket);
                                    #endregion Compléments MARCHE
                                    #region Corporate Event
                                    m_CorporateActionIssue.corporateEvent = new CorporateEvent[1] { new CorporateEvent() };
                                    _missingParameters = m_CorporateActionIssue.corporateEvent[0].SetNormMsgFactoryParameters(BuildingInfo.parameters, m_CATemplatePath);
                                    if (0 == _missingParameters.Count)
                                    {
                                        CorporateEventProcedure procedure = m_CorporateActionIssue.corporateEvent[0].procedure;
                                        if (null != procedure)
                                        {
                                            #region Compléments SOUS-JACENTS
                                            if (ArrFunc.IsFilled(procedure.underlyers))
                                            {
                                                Cst.ErrLevel ret2 = ret;
                                                foreach (CorporateEventUnderlyer underlyer in procedure.underlyers)
                                                {
                                                    ret2 = SetUnderlyerClass(underlyer, sqlMarket.Id);
                                                    if (Cst.ErrLevel.SUCCESS != ret2)
                                                        ret = ret2;
                                                }
                                            }
                                            else
                                            {
                                                // Underlyer(s) non trouvé(s) 
                                                // FI 20200623 [XXXXX] SetErrorWarning
                                                m_NormMsgFactoryProcess.ProcessState.SetErrorWarning(ProcessStateTools.StatusErrorEnum);

                                                Logger.Log(new LoggerData(LogLevelEnum.Error, new SysMsgCode(SysCodeEnum.SYS, 8032), 2,
                                                    new LogParam(parameter.Value + " (UNLMARKET)"),
                                                    new LogParam(LogTools.IdentifierAndId(BuildingInfo.identifier, BuildingInfo.id))));

                                                ret = Cst.ErrLevel.DATANOTFOUND;
                                            }
                                            #endregion Compléments SOUS-JACENTS
                                        }
                                    }
                                    else
                                    {
                                        // Paramètre obligatoire manquant dans le message
                                        string _value = string.Empty;
                                        _missingParameters.ForEach(item => _value += item + " - ");
                                        
                                        m_NormMsgFactoryProcess.ProcessState.SetErrorWarning(ProcessStateTools.StatusErrorEnum);

                                        Logger.Log(new LoggerData(LogLevelEnum.Error, new SysMsgCode(SysCodeEnum.SYS, 8030), 2,
                                            new LogParam(_value + "(CorpoEvent)"),
                                            new LogParam(LogTools.IdentifierAndId(BuildingInfo.identifier, BuildingInfo.id))));

                                        ret = Cst.ErrLevel.DATANOTFOUND;
                                    }
                                    #endregion Corporate Event
                                }
                                else
                                {
                                    // Paramètre obligatoire manquant dans le message
                                    string _value = string.Empty;
                                    _missingParameters.ForEach(item => _value += item + " - ");
                                    // FI 20200623 [XXXXX] SetErrorWarning
                                    m_NormMsgFactoryProcess.ProcessState.SetErrorWarning(ProcessStateTools.StatusErrorEnum);

                                    Logger.Log(new LoggerData(LogLevelEnum.Error, new SysMsgCode(SysCodeEnum.SYS, 8030), 2,
                                        new LogParam(_value + "(CorpoAction)"),
                                        new LogParam(LogTools.IdentifierAndId(BuildingInfo.identifier, BuildingInfo.id))));

                                    ret = Cst.ErrLevel.DATANOTFOUND;
                                }
                            }
                            else
                            {
                                // EG 20141113 Marché non géré
                                Logger.Log(new LoggerData(LogLevelEnum.Info, new SysMsgCode(SysCodeEnum.LOG, 8026), 2,
                                    new LogParam(LogTools.IdentifierAndId(BuildingInfo.identifier, BuildingInfo.id)),
                                    new LogParam(LogTools.IdentifierAndId(sqlMarket.FIXML_SecurityExchange, sqlMarket.Id))));

                                ret = Cst.ErrLevel.ENTITYMARKET_UNMANAGED;
                            }
                        }
                        else
                        {
                            // FI 20200623 [XXXXX] SetErrorWarning
                            m_NormMsgFactoryProcess.ProcessState.SetErrorWarning(ProcessStateTools.StatusErrorEnum);

                            // Marché non trouvé dans le référentiel
                            Logger.Log(new LoggerData(LogLevelEnum.Error, new SysMsgCode(SysCodeEnum.SYS, 8032), 2,
                                new LogParam(parameter.Value + " (CAMARKET)"),
                                new LogParam(LogTools.IdentifierAndId(BuildingInfo.identifier, BuildingInfo.id))));

                            ret = Cst.ErrLevel.DATANOTFOUND;
                        }
                    }
                    else
                    {
                        m_NormMsgFactoryProcess.ProcessState.SetErrorWarning(ProcessStateTools.StatusErrorEnum);

                        // Paramètre CAMARKET non trouvé 
                        Logger.Log(new LoggerData(LogLevelEnum.Error, new SysMsgCode(SysCodeEnum.SYS, 8031), 2,
                            new LogParam("CAMARKET"),
                            new LogParam(LogTools.IdentifierAndId(BuildingInfo.identifier, BuildingInfo.id))));

                        ret = Cst.ErrLevel.DATANOTFOUND;
                    }
                }
                #endregion Parameters
            }
            catch (Exception ex)
            {
                // FI 20200623 [XXXXX] AddCriticalException
                m_NormMsgFactoryProcess.ProcessState.AddCriticalException(ex);
                
                // RD 20171013 [23506] Log exception                        
                Logger.Log(new LoggerData(SpheresExceptionParser.GetSpheresException(null, ex)));

                ret = Cst.ErrLevel.FAILURE;
            }
            return ret;
        }
        #endregion SetCorporateActionClass
        #endregion Methods
    }
    #endregion NormMsgFactory_CORPOACTIONINTEGRATE
}