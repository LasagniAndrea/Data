using EFS.ACommon;
using EFS.ApplicationBlocks.Data;
using EFS.Common;
using EFS.Common.Log;
using EFS.Common.MQueue;
using EFS.GUI.Interface;
using EFS.LoggerClient;
using EFS.LoggerClient.LoggerService;
using EFS.Process;
using EfsML.Business;
using EfsML.Enum;
using EfsML.Interface;
using FixML.v50SP1.Enum;
using FpML.Enum;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Xml.Serialization;

namespace EfsML.CorporateActions
{
    #region Enumerators

    #region CorporateActionReadyStateEnum
    /// <summary>
    /// Etat de publication d'une Corporate action
    /// </summary>
    [SerializableAttribute()]
    [XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public enum CorporateActionReadyStateEnum
    {
        /// <summary>Publiée</summary>
        PUBLISHED,
        /// <summary>Intégrée</summary>
        EMBEDDED,
        /// <summary>Dépréciée</summary>
        DEPRECATED,
        /// <summary>Expirée</summary>
        EXPIRED,
        /// <summary>Reservée</summary>
        RESERVED,
        /// <summary>Non managée (Marché non géré)</summary>
        UNMANAGED,
    }
    #endregion CorporateActionReadyStateEnum
    #region CorporateActionEmbeddedStateEnum
    /// <summary>
    /// Etat d'intégration d'une Corporate action publiée
    /// </summary>
    [SerializableAttribute()]
    [XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public enum CorporateActionEmbeddedStateEnum
    {
        /// <summary>Inconnu</summary>
        UNKNOWN,
        /// <summary>Donnée non trouvée</summary>
        DATANOTFOUND,
        /// <summary>Erreur inconnue</summary>
        ERROR,
        /// <summary>Nouvelle publication</summary>
        NEWEST,
        /// <summary>Reservé donc non intégrable</summary>
        RESERVED,
        /// <summary>En attente d'intégration</summary>
        REQUESTED,
        /// <summary>Intégrée avec succés</summary>
        SUCCESS,
        /// <summary>Non gérée (Pas de prsénce du marché sur ENTITYMARKET</summary>
        UNMANAGED,
    }
    #endregion CorporateActionEmbeddedStateEnum
    #region CorporateEventReadyStateEnum
    /// <summary>
    /// Etat d'évaluation et de traitement d'une Corporate action (contract et asset)
    /// </summary>
    [SerializableAttribute()]
    [XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public enum CorporateEventReadyStateEnum
    {
        REQUESTED,
        DEPRECATED,
        EVALUATED,
        EXECUTED,
    }
    #endregion CorporateEventReadyStateEnum
    #region CorpoEventRenamingContractMethodEnum
    /// <summary>
    /// Méthode utilisée pour les contrats (Nouveaux et mise à jour suite à CA)
    /// </summary>
    [SerializableAttribute()]
    [XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public enum CorpoEventRenamingContractMethodEnum
    {
        None,
        ContractAttribute,
        SymbolSuffix,
    }
    #endregion CorpoEventRenamingContractMethodEnum
    #region CorporateEventGroupEnum
    /// <summary>
    /// Groupe (Classification d'événements de Corporate actions)
    /// </summary>
    [SerializableAttribute()]
    [XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public enum CorporateEventGroupEnum
    {
        All, // unused on DDL (used for RatioCertified template)
        Distribution,
        Reorganization,
        Structure,
        Combination,
        Others,
    }
    #endregion CorporateEventGroupEnum
    #region CorporateEventTypeEnum
    /// <summary>
    /// Type d'événement de Corporate actions
    /// </summary>
    [SerializableAttribute()]
    [XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public enum CorporateEventTypeEnum
    {
        All, // unused on DDL (used for RatioCertified template)
        BonusIssue,
        ExtraDividend,
        SpecialDividend,
        StockDividend,
        RightsIssue,
        SpinOff,
        KRepayment,
        Split,
        ReverseSplit,
        TakeOver,
        TenderOffer,
        Merger,
        DeMerger,
        Delisting,
        Liquidation,
        Renaming,
        ISINChange,
        Conversion,
        Others,
    }
    #endregion CorporateEventTypeEnum

    #region AdjustmentElementEnum
    /// <summary>
    /// Eléments d'ajustement.
    /// </summary>
    [SerializableAttribute()]
    [XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public enum AdjustmentElementEnum
    {
        RFactor,
        ContractSize,
        ContractMultiplier,
        StrikePrice,
        Price,
        EqualisationPayment,
    }
    #endregion AdjustmentElementEnum

    #region AdjustmentMethodOfDerivContractEnum
    /// <summary>
    /// Méthode utilisée pour l'ajustement des contrats suite à événement de Corporate actions.
    /// </summary>
    [SerializableAttribute()]
    [XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public enum AdjustmentMethodOfDerivContractEnum
    {
        Ratio,
        FairValue,
        Package,
        None,
    }
    #endregion AdjustmentMethodOfDerivContractEnum
    #region ConditionEnum
    /// <summary>
    /// Condition d'application de l'ajustement des contrats suite à événement de Corporate actions.
    /// </summary>
    [SerializableAttribute()]
    [XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public enum ConditionEnum
    {
        Always,
        NonPositiveValue,
        NonNegativeValue,
        PositiveValue,
        LessThanOne,
        Zero,
    }
    #endregion ConditionEnum
    #region CombinationOperandEnum
    /// <summary>
    /// operateur de lien pour les événements combinés de Corporate actions.
    /// </summary>
    [SerializableAttribute()]
    [XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public enum CombinationOperandEnum
    {
        FollowedBy,
        Together,
    }
    #endregion CombinationOperandEnum
    #region ComponentType
    /// <summary>
    /// Type de composant (utilisé dans la sérialisation sur un XSD:CHOICE)
    /// </summary>
    [XmlTypeAttribute(IncludeInSchema = false)]
    public enum ComponentType
    {
        component,
        componentFormula,
        componentMethod,
        componentProperty,
        componentReference,
    }
    #endregion ComponentType
    #region ResultType
    /// <summary>
    /// Type de résultat (utilisé dans la sérialisation sur un XSD:CHOICE)
    /// </summary>
    // EG [33415/33420] Nouveau type résultat de composant (info = string)
    // EG 20140317 [19722]
    [XmlTypeAttribute(IncludeInSchema = false)]
    public enum ResultType
    {
        amount,
        unit,
        info,
        check,
    }
    #endregion ResultType


    #endregion Enumerators

    /* -------------------------------------------------------- */
    /* ----- CLASSES GENERALES QUERIES :                  ----- */
    /* ----- CORPOACTIONISSUE                             ----- */
    /* ----- CORPOACTION/CORPOEVENT/CORPOACTIONNOTICE     ----- */
    /* -------------------------------------------------------- */

    #region CAQueryBase
    /// <summary>
    /// Classe de base des Queries et paramètres des Corporate actions (insert, update, select, delete)
    /// </summary>
    /// EG 20211020 [XXXXX] Nouvelle gestion des notices (USEURLNOTICE et URLNOTICE)
    public abstract partial class CAQueryBase
    {
        #region Members
        protected DataParameter paramID;
        protected DataParameter paramIdentifier;
        protected DataParameter paramReadyState;
        protected DataParameter paramCfiCode;
        protected DataParameter paramUrlNotice;
        protected DataParameter paramUseUrlNotice;
        protected DataParameter paramRefNotice;
        protected DataParameter paramNoticeFileName;
        protected DataParameter paramPubDate;
        protected DataParameter paramDtIns;
        protected DataParameter paramIdAIns;
        protected DataParameter paramDtUpd;
        protected DataParameter paramIdAUpd;
        protected DataParameter paramExtlLink;
        protected DataParameter paramRowAttribut;

        protected string _CS;
        #endregion Members
        #region Constructor
        public CAQueryBase(string pCS)
        {
            _CS = pCS;
            InitParameter();
        }
        #endregion
        #region Methods
        #region AddParameters
        /// <summary>
        /// Ajout des paramètres commun pour l'insertion et la mise à jour
        /// </summary>
        /// <param name="pParameters"></param>
        /// EG 20211020 [XXXXX] Nouvelle gestion des notices (USEURLNOTICE et URLNOTICE)
        private void AddParameters(DataParameters pParameters)
        {
            pParameters.Add(paramID);
            pParameters.Add(paramIdentifier);
            pParameters.Add(paramReadyState);
            pParameters.Add(paramCfiCode);
            pParameters.Add(paramRefNotice);
            pParameters.Add(paramNoticeFileName);
            pParameters.Add(paramPubDate);
            pParameters.Add(paramUrlNotice);
            pParameters.Add(paramUseUrlNotice);
        }
        #endregion AddParametersInsert
        #region AddParametersInsert
        /// <summary>
        /// Ajout des commun paramètres pour l'insertion
        /// </summary>
        /// <param name="pParameters"></param>
        public virtual void AddParametersInsert(DataParameters pParameters)
        {
            AddParameters(pParameters);
            pParameters.Add(paramDtIns);
            pParameters.Add(paramIdAIns);
            pParameters.Add(paramExtlLink);
            pParameters.Add(paramRowAttribut);
        }
        #endregion AddParametersInsert
        #region AddParametersUpdate
        /// <summary>
        /// Ajout des paramètres commun pour la mise à jour
        /// </summary>
        /// <param name="pParameters"></param>
        public virtual void AddParametersUpdate(DataParameters pParameters)
        {
            AddParameters(pParameters);
            pParameters.Add(paramDtUpd);
            pParameters.Add(paramIdAUpd);
        }
        #endregion AddParametersUpdate
        #region AddParametersUpdateStatus
        /// <summary>
        /// Ajout des paramètres pour la mise à jour du status d'intégration
        /// </summary>
        /// <param name="pParameters"></param>
        public virtual void AddParametersUpdateStatus(DataParameters pParameters)
        {
            pParameters.Add(paramID);
            pParameters.Add(paramReadyState);
            pParameters.Add(paramDtUpd);
            pParameters.Add(paramIdAUpd);
        }
        #endregion AddParametersUpdateStatus
        #region GetQueryUpdateStatus
        public virtual QueryParameters GetQueryUpdateStatus()
        {
            return null;
        }
        #endregion GetQueryUpdateStatus

        #region GetQueryExist
        public virtual QueryParameters GetQueryExist(CATools.CAWhereMode pCAWhereMode)
        {
            return null;
        }
        #endregion GetQueryExist
        #region GetQueryDelete
        public virtual QueryParameters GetQueryDelete()
        {
            return null;
        }
        #endregion GetQueryDelete
        #region GetQuerySelect
        public virtual QueryParameters GetQuerySelect(CATools.CAWhereMode pCAWhereMode)
        {
            return null;
        }
        #endregion GetQuerySelect
        #region GetQuerySelectCandidate
        public virtual QueryParameters GetQuerySelectCandidate()
        {
            return null;
        }
        #endregion GetQuerySelectCandidate

        #region GetQueryInsert
        public virtual QueryParameters GetQueryInsert()
        {
            return null;
        }
        #endregion GetQueryInsert
        #region GetQueryUpdate
        public virtual QueryParameters GetQueryUpdate()
        {
            return null;
        }
        #endregion GetQueryUpdate

        #region InitParameter
        /// <summary>
        /// Initialisation des paramètres communs (Intégrées et publiées)
        /// </summary>
        // EG [33415/33420]
        /// EG 20211020 [XXXXX] Nouvelle gestion des notices (USEURLNOTICE et URLNOTICE)
        private void InitParameter()
        {
            paramID = new DataParameter(_CS, "ID", DbType.Int32);
            paramIdentifier = new DataParameter(_CS, "IDENTIFIER", DbType.AnsiString, SQLCst.UT_IDENTIFIER_LEN);
            paramReadyState = new DataParameter(_CS, "READYSTATE", DbType.AnsiString, SQLCst.UT_ENUM_MANDATORY_LEN);
            paramCfiCode = new DataParameter(_CS, "CFICODE", DbType.AnsiString, SQLCst.UT_ENUMCHAR_OPTIONAL_LEN);
            paramRefNotice = new DataParameter(_CS, "REFNOTICE", DbType.AnsiString, SQLCst.UT_IDENTIFIER_LEN);
            paramNoticeFileName = new DataParameter(_CS, "NOTICEFILENAME", DbType.AnsiString, SQLCst.UT_UNC_LEN);
            paramPubDate = new DataParameter(_CS, "PUBDATE", DbType.Date);
            paramDtIns =  DataParameter.GetParameter(_CS, DataParameter.ParameterEnum.DTINS);
            paramIdAIns = DataParameter.GetParameter(_CS, DataParameter.ParameterEnum.IDAINS);
            paramDtUpd = DataParameter.GetParameter(_CS, DataParameter.ParameterEnum.DTUPD);
            paramIdAUpd = DataParameter.GetParameter(_CS, DataParameter.ParameterEnum.IDAUPD);
            paramExtlLink = new DataParameter(_CS, "EXTLLINK", DbType.AnsiString, SQLCst.UT_EXTLINK_LEN);
            paramRowAttribut = new DataParameter(_CS, "ROWATTRIBUT", DbType.AnsiString, SQLCst.UT_ROWATTRIBUT_LEN);
            paramUrlNotice = new DataParameter(_CS, "URLNOTICE", DbType.AnsiString, SQLCst.UT_UNC_LEN);
            paramUseUrlNotice = new DataParameter(_CS, "USEURLNOTICE", DbType.Boolean);
        }
        #endregion

        #region SetParameters
        /// <summary>
        /// Valorisation des paramètres communs INSERTION/MISE A JOUR (Intégrées et publiées)
        /// </summary>
        /// <param name="pCorporateAction">Corporate action</param>
        /// <param name="pParameters">Paramètres</param>
        /// EG 20211020 [XXXXX] Nouvelle gestion des notices (USEURLNOTICE et URLNOTICE)
        private void SetParameters(CorporateAction pCorporateAction, DataParameters pParameters)
        {
            pParameters["ID"].Value = pCorporateAction.IdCA;
            pParameters["IDENTIFIER"].Value = pCorporateAction.identifier;
            pParameters["REFNOTICE"].Value = pCorporateAction.refNotice.value;
            pParameters["CFICODE"].Value = pCorporateAction.cfiCodeSpecified? ReflectionTools.ConvertEnumToString<CfiCodeCategoryEnum>(pCorporateAction.cfiCode):Convert.DBNull;
            pParameters["NOTICEFILENAME"].Value = pCorporateAction.refNotice.fileName;
            pParameters["PUBDATE"].Value = pCorporateAction.pubDate;
            pParameters["URLNOTICE"].Value = pCorporateAction.urlnotice;
            pParameters["USEURLNOTICE"].Value = pCorporateAction.refNotice.useurlnoticeSpecified && pCorporateAction.refNotice.useurlnotice;

        }
        #endregion SetParametersInsert

        #region SetParametersInsert
        /// <summary>
        /// Valorisation des paramètres communs INSERTION (Intégrées et publiées)
        /// </summary>
        /// <param name="pCorporateAction">Corporate action</param>
        /// <param name="pParameters">Paramètres</param>
        public virtual void SetParametersInsert(CorporateAction pCorporateAction, DataParameters pParameters)
        {
            SetParameters(pCorporateAction, pParameters);

            pParameters["IDAINS"].Value = pCorporateAction.IdA;
            // FI 20200820 [25468] Dates systemes en UTC
            pParameters["DTINS"].Value = OTCmlHelper.GetDateSysUTC(_CS);
            pParameters["EXTLLINK"].Value = string.Empty;
            pParameters["ROWATTRIBUT"].Value = Cst.RowAttribut_System;
        }
        #endregion SetParametersInsert
        #region SetParametersUpdate
        /// <summary>
        /// Valorisation des paramètres communs MISE A JOUR (Intégrées et publiées)
        /// </summary>
        /// <param name="pCorporateAction">Corporate action</param>
        /// <param name="pParameters">Paramètres</param>
        public virtual void SetParametersUpdate(CorporateAction pCorporateAction, DataParameters pParameters)
        {
            SetParameters(pCorporateAction, pParameters);

            pParameters["IDAUPD"].Value = pCorporateAction.IdA;
            // FI 20200820 [25468] Dates systemes en UTC
            pParameters["DTUPD"].Value = OTCmlHelper.GetDateSysUTC(_CS);
        }
        #endregion SetParametersUpdate
        #endregion Methods
    }
    #endregion CAQueryBase

    #region CAQueryEMBEDDED
    /// <summary>
    /// Classe des Queries et paramètres des Corporate actions INTEGREES (insert, update, select, delete)
    /// </summary>
    /// EG 20140518 [19913] New 
    public class CAQueryEMBEDDED : CAQueryBase
    {
        #region Members
        /// EG 20140518 [19913] New 
        private DataParameter paramIdCAIssue;
        private DataParameter paramIdM;
        private DataParameter paramEffectiveDate; // use By GetQuerySelectCandidate only
        private DataParameter paramDisplayName;
        private DataParameter paramDocumentation;
        #endregion Members
        #region Constructor
        public CAQueryEMBEDDED(string pCS): base(pCS)
        {
            InitParameter();
        }
        #endregion
        #region Methods
        #region AddParameters
        /// <summary>
        /// Paramètres communs à l'INSERTION / MISE A JOUR
        /// </summary>
        /// <param name="pParameters">Paramèetres</param>
        public void AddParameters(DataParameters pParameters)
        {
            /// EG 20140518 [19913] New 
            pParameters.Add(paramIdCAIssue);
            pParameters.Add(paramIdM);
            pParameters.Add(paramDisplayName);
            pParameters.Add(paramDocumentation);
        }
        #endregion AddParameters
        #region AddParametersInsert
        /// <summary>
        /// Paramètres pour INSERTION
        /// </summary>
        /// <param name="pParameters">Paramètres</param>
        public override void AddParametersInsert(DataParameters pParameters)
        {
            base.AddParametersInsert(pParameters);
            AddParameters(pParameters);
        }
        #endregion AddParametersInsert
        #region AddParametersUpdate
        /// <summary>
        /// Paramètres pour MISE A JOUR
        /// </summary>
        /// <param name="pParameters">Paramètres</param>
        public override void AddParametersUpdate(DataParameters pParameters)
        {
            base.AddParametersUpdate(pParameters);
            AddParameters(pParameters);
        }
        #endregion AddParametersUpdate

        #region GetQueryExist
        /// <summary>
        /// Requête de contrôle existence d'une CA INTEGREE
        /// Recherche par :
        /// ● son ID
        /// ● sa référence de notice (MARCHE + REFNOTICE (Principale ou additionnelles))
        /// </summary>
        /// <param name="pCAWhereMode">Mode (Critère) de recherche</param>
        /// <returns></returns>
        public override QueryParameters GetQueryExist(CATools.CAWhereMode pCAWhereMode)
        {
            DataParameters parameters = new DataParameters();
            StrBuilder sqlQuery = new StrBuilder();
            sqlQuery += SQLCst.SELECT + "1" + Cst.CrLf;
            sqlQuery += SQLCst.FROM_DBO + Cst.OTCml_TBL.CORPOACTION.ToString() + " ca" + Cst.CrLf;

            switch (pCAWhereMode)
            {
                case CATools.CAWhereMode.ID:
                    parameters.Add(paramID);
                    sqlQuery += SQLCst.WHERE + "(ca.IDCA = @ID)" + Cst.CrLf;
                    break;
                case CATools.CAWhereMode.NOTICE:
                    parameters.Add(paramIdM);
                    parameters.Add(paramRefNotice);
                    parameters.Add(paramCfiCode);

                    sqlQuery += SQLCst.LEFTJOIN_DBO + Cst.OTCml_TBL.CORPOACTIONNOTICE.ToString() + " cn on (cn.IDCA = ca.IDCA)" + Cst.CrLf;
                    sqlQuery += SQLCst.WHERE + "(ca.IDM = @IDM) and ((ca.REFNOTICE = @REFNOTICE) or (cn.REFNOTICE = @REFNOTICE))" + Cst.CrLf;
                    sqlQuery += SQLCst.AND + "((ca.CFICODE is null) or (ca.CFICODE = @CFICODE))" + Cst.CrLf;
                    break;
            }
            QueryParameters ret = new QueryParameters(_CS, sqlQuery.ToString(), parameters);
            return ret;
        }
        #endregion GetQueryExist
        #region GetQueryDelete
        /// <summary>
        /// Requête de suppression d'une CA PUBLIEE
        /// </summary>
        /// <returns></returns>
        public override QueryParameters GetQueryDelete()
        {
            DataParameters parameters = new DataParameters();
            parameters.Add(paramID);
            StrBuilder sqlQuery = new StrBuilder();
            sqlQuery += SQLCst.DELETE_DBO + Cst.OTCml_TBL.CORPOACTION.ToString() + Cst.CrLf;
            sqlQuery += SQLCst.WHERE + "(IDCA = @ID)" + Cst.CrLf;
            QueryParameters ret = new QueryParameters(_CS, sqlQuery.ToString(), parameters);
            return ret;
        }
        #endregion GetQueryDelete
        #region GetQuerySelect
        /// <summary>
        /// Requête de sélection d'une CA INTEGREE
        /// Recherche par :
        /// ● son ID
        /// ● sa référence de notice (MARCHE (IDM) + REFNOTICE (Principale ou additionnelles))
        /// Chargement de CORPOACTION / CORPOEVENT et CORPOEVENTNOTICE
        /// </summary>
        /// <param name="pCAWhereMode">Mode (Critère) de recherche</param>
        /// <returns></returns>
        /// EG 20140518 [19913] New Lecture CORPODOCS
        /// EG 20211020 [XXXXX] Nouvelle gestion des notices (USEURLNOTICE et URLNOTICE)
        public override QueryParameters GetQuerySelect(CATools.CAWhereMode pCAWhereMode)
        {
            DataParameters parameters = new DataParameters();

            string sqlAddNoticeWhere = string.Empty;
            string sqlWhere = string.Empty;
            
            switch (pCAWhereMode)
            {
                case CATools.CAWhereMode.ID:
                    parameters.Add(paramID);
                    sqlWhere = SQLCst.WHERE + "(ca.IDCA = @ID);" + Cst.CrLf;
                    sqlAddNoticeWhere = sqlWhere;
                    break;
                case CATools.CAWhereMode.NOTICE:
                    parameters.Add(paramIdM);
                    parameters.Add(paramRefNotice);
                    parameters.Add(paramCfiCode);
                    sqlAddNoticeWhere = SQLCst.WHERE + @"(ca.IDM = @IDM) and ((ca.CFICODE is null) or (ca.CFICODE = @CFICODE)) 
                    and ((ca.REFNOTICE = @REFNOTICE) or (cn.REFNOTICE = @REFNOTICE))" + Cst.CrLf;

                    sqlWhere = SQLCst.WHERE + @"(ca.IDM = @IDM) and ((ca.CFICODE is null) or (ca.CFICODE = @CFICODE)) and 
                    ((ca.REFNOTICE = @REFNOTICE) or  exists (select 1 from dbo.CORPOACTIONNOTICE cn where (cn.IDCA=ca.IDCA) and (cn.REFNOTICE = @REFNOTICE)))" + Cst.CrLf;
                    break;
            }

            StrBuilder sqlQuery = new StrBuilder();
            sqlQuery += SQLCst.SELECT + @"ca.IDCA as ID, ca.IDCAISSUE, ca.IDM, ca.IDENTIFIER, ca.DISPLAYNAME, ca.DOCUMENTATION,
            ca.READYSTATE, ca.CFICODE, ca.URLNOTICE, ca.USEURLNOTICE, ca.REFNOTICE, ca.NOTICEFILENAME, ca.PUBDATE, ca.DTINS, ca.IDAINS, ca.DTUPD, ca.IDAUPD, ca.EXTLLINK, ca.ROWATTRIBUT
            from dbo.CORPOACTION ca" + Cst.CrLf;
            sqlQuery += sqlWhere + SQLCst.SEPARATOR_MULTIDML;

            sqlQuery += SQLCst.SELECT + @"ce.IDCE, ce.IDCA, ce.IDENTIFIER as CEIDENTIFIER, ce.REQUESTMODE, ce.EXECORDER,
            ce.CEGROUP, ce.CETYPE, ce.CECOMBINEDOPER, ce.CECOMBINEDTYPE, ce.EXDATE, ce.EFFECTIVEDATE, ce.ADJMETHOD, ce.ADJPROCEDURES
            from dbo.CORPOEVENT ce
            inner join dbo.CORPOACTION ca on (ca.IDCA = ce.IDCA)" + Cst.CrLf;
            sqlQuery += sqlWhere + SQLCst.SEPARATOR_MULTIDML;

            sqlQuery += SQLCst.SELECT + @"cn.IDCA, cn.REFNOTICE, cn.PUBDATE, cn.NOTICEFILENAME, cn.USEURLNOTICE
            from dbo.CORPOACTIONNOTICE cn
            inner join dbo.CORPOACTION ca on (ca.IDCA = cn.IDCA)" + Cst.CrLf;
            sqlQuery += sqlAddNoticeWhere + SQLCst.SEPARATOR_MULTIDML;

            /// EG 20140518 [19913] New 
            sqlQuery += SQLCst.SELECT + @"cd.IDCA, cd.DOCNAME, cd.DOCTYPE, cd.RUNTIME, cd.LODOC, cd.DTUPD, cd.IDAUPD
            from dbo.CORPODOCS cd
            inner join dbo.CORPOACTION ca on (ca.IDCA = cd.IDCA)" + Cst.CrLf;
            sqlQuery += sqlWhere + SQLCst.SEPARATOR_MULTIDML;

            QueryParameters ret = new QueryParameters(_CS, sqlQuery.ToString(), parameters);
            return ret;
        }
        #endregion GetQuerySelect
        #region GetQuerySelectCandidate
        /// <summary>
        /// Requête de sélection des CA INTEGREES candidates à traitement 
        /// ● pour un marché 
        /// ● à une date donnée
        /// ● avec statut = 
        /// </summary>
        /// <returns></returns>
        public override QueryParameters GetQuerySelectCandidate()
        {
            DataParameters parameters = new DataParameters();
            parameters.Add(paramIdM);
            parameters.Add(paramEffectiveDate);
            parameters.Add(paramReadyState);
            StrBuilder sqlQuery = new StrBuilder();
            sqlQuery += SQLCst.SELECT + "ce.IDCA as ID, ce.IDCE, ca.IDENTIFIER, ca.REFNOTICE, ca.CFICODE" + Cst.CrLf;
            sqlQuery += SQLCst.FROM_DBO + Cst.OTCml_TBL.CORPOEVENT.ToString() + " ce" + Cst.CrLf;
            sqlQuery += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.CORPOACTION.ToString() + " ca on (ca.IDCA = ce.IDCA)" + Cst.CrLf;
            sqlQuery += SQLCst.WHERE + "(ca.IDM = @IDM) and (ce.EFFECTIVEDATE = @EFFECTIVEDATE) and (ca.READYSTATE = @READYSTATE)";
            QueryParameters ret = new QueryParameters(_CS, sqlQuery.ToString(), parameters);
            return ret;
        }
        #endregion GetQuerySelectCandidate

        #region GetQueryInsert
        /// <summary>
        /// Requête d'insertion d'un CA INTEGREE
        /// </summary>
        /// <returns></returns>
        /// EG 20140518 [19913] New Add IDCAISSUE
        /// EG 20211020 [XXXXX] Nouvelle gestion des notices (USEURLNOTICE et URLNOTICE)
        public override QueryParameters GetQueryInsert()
        {
            DataParameters parameters = new DataParameters();
            AddParametersInsert(parameters);
            StrBuilder sqlQuery = new StrBuilder();
            sqlQuery += SQLCst.INSERT_INTO_DBO + Cst.OTCml_TBL.CORPOACTION.ToString() + Cst.CrLf;
            sqlQuery += @"(IDCA, IDCAISSUE, IDM, IDENTIFIER, DISPLAYNAME, DOCUMENTATION, READYSTATE, CFICODE, URLNOTICE, USEURLNOTICE, REFNOTICE, NOTICEFILENAME, " + Cst.CrLf;
            sqlQuery += @"PUBDATE, DTINS, IDAINS, EXTLLINK, ROWATTRIBUT)" + Cst.CrLf;
            sqlQuery += @"values" + Cst.CrLf;
            sqlQuery += @"(@ID, @IDCAISSUE, @IDM, @IDENTIFIER, @DISPLAYNAME, @DOCUMENTATION, @READYSTATE, @CFICODE, @URLNOTICE, @USEURLNOTICE, @REFNOTICE, @NOTICEFILENAME, " + Cst.CrLf;
            sqlQuery += @"@PUBDATE, @DTINS, @IDAINS, @EXTLLINK, @ROWATTRIBUT)" + Cst.CrLf;
            QueryParameters ret = new QueryParameters(_CS, sqlQuery.ToString(), parameters);
            return ret;
        }
        #endregion GetQueryInsert
        #region GetQueryUpdate
        /// <summary>
        /// Requête de MISE A JOUR d'un CA INTEGREE
        /// </summary>
        /// <returns></returns>
        /// EG 20140518 [19913] New Add IDCAISSUE
        /// EG 20211020 [XXXXX] Nouvelle gestion des notices (USEURLNOTICE et URLNOTICE)
        public override QueryParameters GetQueryUpdate()
        {
            DataParameters parameters = new DataParameters();
            AddParametersUpdate(parameters);
            StrBuilder sqlQuery = new StrBuilder();
            sqlQuery += SQLCst.UPDATE_DBO + Cst.OTCml_TBL.CORPOACTION.ToString() + Cst.CrLf;
            sqlQuery += SQLCst.SET + "IDCAISSUE=@IDCAISSUE, IDM=@IDM, IDENTIFIER=@IDENTIFIER, DISPLAYNAME=@DISPLAYNAME, DOCUMENTATION=@DOCUMENTATION, " + Cst.CrLf;
            sqlQuery += "READYSTATE=@READYSTATE, CFICODE=@CFICODE, URLNOTICE=@URLNOTICE, USEURLNOTICE=@USEURLNOTICE, REFNOTICE=@REFNOTICE, NOTICEFILENAME=@NOTICEFILENAME, PUBDATE=@PUBDATE, DTUPD=@DTUPD, IDAUPD = @IDAUPD" + Cst.CrLf;
            sqlQuery += SQLCst.WHERE + "(IDCA=@ID)";
            QueryParameters ret = new QueryParameters(_CS, sqlQuery.ToString(), parameters);
            return ret;
        }
        #endregion GetQueryUpdate

        #region InitParameter
        /// <summary>
        /// Initialisation des paramètres
        /// </summary>
        /// EG 20140518 [19913] New Add IDCAISSUE
        private void InitParameter()
        {
            paramIdCAIssue = new DataParameter(_CS, "IDCAISSUE", DbType.Int32);
            paramIdM = new DataParameter(_CS, "IDM", DbType.Int32);
            paramDisplayName = new DataParameter(_CS, "DISPLAYNAME", DbType.AnsiString, SQLCst.UT_DISPLAYNAME_LEN);
            paramDocumentation = new DataParameter(_CS, "DOCUMENTATION", DbType.AnsiString, SQLCst.UT_NOTE_LEN);
            paramEffectiveDate = new DataParameter(_CS, "EFFECTIVEDATE", DbType.Date); // use By GetQuerySelect only
        }
        #endregion

        #region SetParameters
        /// <summary>
        /// Valorisation des paramètres (INSERTION / MISE A JOUR)
        /// </summary>
        /// EG 20140518 [19913] New Add IDCAISSUE
        private void SetParameters(CorporateAction pCorporateAction, DataParameters pParameters)
        {
            pParameters["IDCAISSUE"].Value = pCorporateAction.idCAIssue;
            pParameters["IDM"].Value = Convert.ToInt32(pCorporateAction.market.spheresid);
            pParameters["DISPLAYNAME"].Value = (pCorporateAction.displaynameSpecified ? pCorporateAction.displayname : Convert.DBNull);
            pParameters["DOCUMENTATION"].Value = (pCorporateAction.descriptionSpecified ? pCorporateAction.description : Convert.DBNull);
            pParameters["READYSTATE"].Value = pCorporateAction.readystate.ToString();
        }
        #endregion SetParameters
        #region SetParametersInsert
        /// <summary>
        /// Valorisation des paramètres (INSERTION)
        /// </summary>
        public override void SetParametersInsert(CorporateAction pCorporateAction, DataParameters pParameters)
        {
            base.SetParametersInsert(pCorporateAction, pParameters);
            SetParameters(pCorporateAction, pParameters);
        }
        #endregion SetParametersInsert
        #region SetParametersUpdate
        /// <summary>
        /// Valorisation des paramètres (MISE A JOUR)
        /// </summary>
        public override void SetParametersUpdate(CorporateAction pCorporateAction, DataParameters pParameters)
        {
            base.SetParametersUpdate(pCorporateAction, pParameters);
            SetParameters(pCorporateAction, pParameters);
        }
        #endregion SetParametersUpdate
        #endregion Methods
    }
    #endregion CAQueryEMBEDDED
    #region CEQueryEMBEDDED
    /// <summary>
    /// Classe des Queries et paramètres des Corporate events INTEGREES (insert, update, select, delete)
    /// </summary>
    public class CEQueryEMBEDDED
    {
        #region Members
        private readonly string _CS;
        private DataParameter paramID;
        private DataParameter paramIdCA;
        private DataParameter paramIdM;
        private DataParameter paramIdentifier;
        private DataParameter paramExecOrder;
        private DataParameter paramRequestMode;
        private DataParameter paramCEGroup;
        private DataParameter paramCEType;
        private DataParameter paramCECombinedOperand;
        private DataParameter paramCECombinedType;
        private DataParameter paramExDate;
        private DataParameter paramEffectiveDate;
        private DataParameter paramAdjMethod;
        private DataParameter paramAdjProcedures;
        #endregion Members
        #region Constructor
        public CEQueryEMBEDDED(string pCS)
        {
            _CS = pCS;
            InitParameter();
        }
        #endregion
        #region Methods
        #region AddParamterInsert
        /// <summary>
        /// Paramètres pour INSERTION
        /// </summary>
        /// <param name="pParameters">Paramètres</param>
        public void AddParametersInsert(DataParameters pParameters)
        {
            pParameters.Add(paramIdCA);
            AddParametersUpdate(pParameters);
        }
        #endregion AddParamterInsert
        #region AddParametersUpdate
        /// <summary>
        /// Paramètres pour MISE A JOUR
        /// </summary>
        /// <param name="pParameters">Paramètres</param>
        public void AddParametersUpdate(DataParameters pParameters)
        {
            pParameters.Add(paramID);
            pParameters.Add(paramIdentifier);
            pParameters.Add(paramRequestMode);
            pParameters.Add(paramExecOrder);
            pParameters.Add(paramCEGroup);
            pParameters.Add(paramCEType);
            pParameters.Add(paramCECombinedOperand);
            pParameters.Add(paramCECombinedType);
            pParameters.Add(paramExDate);
            pParameters.Add(paramEffectiveDate);
            pParameters.Add(paramAdjMethod);
            pParameters.Add(paramAdjProcedures);
        }
        #endregion AddParametersUpdate

        #region GetQueryExist
        /// <summary>
        /// Requête d'existence d'un CE
        /// </summary>
        /// <param name="pID">ID</param>
        public QueryParameters GetQueryExist()
        {
            StrBuilder sqlQuery = new StrBuilder();
            sqlQuery += SQLCst.SELECT + "1" + Cst.CrLf;
            sqlQuery += SQLCst.FROM_DBO + Cst.OTCml_TBL.CORPOEVENT.ToString() + " ce" + Cst.CrLf;
            sqlQuery += SQLCst.WHERE + Cst.CrLf;

            DataParameters parameters = new DataParameters();
            parameters.Add(paramID);
            sqlQuery += "(IDCE = @ID)" + Cst.CrLf;
            QueryParameters ret = new QueryParameters(_CS, sqlQuery.ToString(), parameters);
            return ret;
        }
        #endregion GetQueryExist
        #region GetQueryDelete
        /// <summary>
        /// Requête de suppression d'un CE
        /// </summary>
        public QueryParameters GetQueryDelete()
        {
            DataParameters parameters = new DataParameters();
            parameters.Add(paramID);
            StrBuilder sqlQuery = new StrBuilder();
            sqlQuery += SQLCst.DELETE_DBO + Cst.OTCml_TBL.CORPOEVENT.ToString() + Cst.CrLf;
            sqlQuery += SQLCst.WHERE + "(IDCE = @ID)" + Cst.CrLf;
            QueryParameters ret = new QueryParameters(_CS, sqlQuery.ToString(), parameters);
            return ret;
        }
        #endregion GetQueryDelete
        #region GetQuerySelect
        /// <summary>
        /// Requête de sélection d'un CE
        /// </summary>
        // EG [33415/33420]
        public QueryParameters GetQuerySelect(CATools.CAWhereMode pCAWhereMode)
        {
            DataParameters parameters = new DataParameters();

            string sqlJoinWhere = string.Empty;
            string sqlWhere = string.Empty;
            
            switch (pCAWhereMode)
            {
                case CATools.CAWhereMode.ID:
                    parameters.Add(paramID);
                    sqlWhere = SQLCst.WHERE + "(ce.IDCE = @ID);" + Cst.CrLf;
                    break;
                case CATools.CAWhereMode.EFFECTIVEDATE:
                    parameters.Add(paramEffectiveDate);
                    parameters.Add(paramIdM);
                    sqlJoinWhere = SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.CORPOACTION.ToString() + " ca on (ca.IDCA = cn.IDCA) and (ca.IDM = @IDM)" + Cst.CrLf;
                    sqlWhere = SQLCst.WHERE + "(ca.IDM = @IDM) and (cn.EFFECTIVEDATE = @EFFECTIVEDATE)";
                    break;
            }

            StrBuilder sqlQuery = new StrBuilder();
            sqlQuery += SQLCst.SELECT + "ce.IDCE, ce.IDCA, ce.IDENTIFIER, ce.REQUESTMODE, ce.EXECORDER, " + Cst.CrLf;
            sqlQuery += "ce.CEGROUP, ce.CETYPE, ce.CECOMBINEDOPER, ce.CECOMBINEDTYPE, ce.EXDATE, ce.EFFECTIVEDATE, " + Cst.CrLf;
            sqlQuery += "ce.ADJMETHOD, ce.CFICODE, ce.ADJPROCEDURES" + Cst.CrLf;
            sqlQuery += SQLCst.FROM_DBO + Cst.OTCml_TBL.CORPOEVENT.ToString() + " ce" + Cst.CrLf;
            sqlQuery += sqlJoinWhere + sqlWhere;
            QueryParameters ret = new QueryParameters(_CS, sqlQuery.ToString(), parameters);
            return ret;
        }
        #endregion GetQuerySelect
        #region GetQueryInsert
        /// <summary>
        /// Requête d'insertion d'un CE
        /// </summary>
        // EG [33415/33420]
        public QueryParameters GetQueryInsert()
        {
            DataParameters parameters = new DataParameters();
            AddParametersInsert(parameters);
            StrBuilder sqlQuery = new StrBuilder();
            sqlQuery += SQLCst.INSERT_INTO_DBO + Cst.OTCml_TBL.CORPOEVENT.ToString() + Cst.CrLf;
            sqlQuery += @"(IDCE, IDCA, IDENTIFIER, REQUESTMODE, EXECORDER, " + Cst.CrLf;
            sqlQuery += @"CEGROUP, CETYPE, CECOMBINEDOPER, CECOMBINEDTYPE, EXDATE, EFFECTIVEDATE, " + Cst.CrLf;
            sqlQuery += @"ADJMETHOD, ADJPROCEDURES)" + Cst.CrLf;
            sqlQuery += @"values" + Cst.CrLf;
            sqlQuery += @"(@ID, @IDCA, @IDENTIFIER, @REQUESTMODE, @EXECORDER, " + Cst.CrLf;
            sqlQuery += @"@CEGROUP, @CETYPE, @CECOMBINEDOPER, @CECOMBINEDTYPE, @EXDATE, @EFFECTIVEDATE, " + Cst.CrLf;
            sqlQuery += @"@ADJMETHOD, @ADJPROCEDURES)" + Cst.CrLf;
            QueryParameters ret = new QueryParameters(_CS, sqlQuery.ToString(), parameters);
            return ret;
        }
        #endregion GetQueryInsert
        #region GetQueryUpdate
        /// <summary>
        /// Requête de mise à jour d'un CE
        /// </summary>
        // EG [33415/33420]
        public QueryParameters GetQueryUpdate()
        {
            DataParameters parameters = new DataParameters();
            AddParametersUpdate(parameters);
            StrBuilder sqlQuery = new StrBuilder();
            sqlQuery += SQLCst.UPDATE_DBO + Cst.OTCml_TBL.CORPOEVENT.ToString() + Cst.CrLf;
            sqlQuery += SQLCst.SET + "IDENTIFIER=@IDENTIFIER, EXECORDER=@EXECORDER, REQUESTMODE=@REQUESTMODE, " + Cst.CrLf;
            sqlQuery += "CEGROUP=@CEGROUP, CETYPE=@CETYPE, CECOMBINEDOPER=@CECOMBINEDOPER, CECOMBINEDTYPE=@CECOMBINEDTYPE," + Cst.CrLf;
            sqlQuery += "EXDATE=@EXDATE, EFFECTIVEDATE=@EFFECTIVEDATE, ADJMETHOD=@ADJMETHOD, ADJPROCEDURES=@ADJPROCEDURES" + Cst.CrLf;
            sqlQuery += SQLCst.WHERE + "(IDCE=@ID)";
            QueryParameters ret = new QueryParameters(_CS, sqlQuery.ToString(), parameters);
            return ret;
        }
        #endregion GetQueryUpdate

        #region InitParameter
        /// <summary>
        /// Initialisation des paramètres
        /// </summary>
        private void InitParameter()
        {
            paramID = new DataParameter(_CS, "ID", DbType.Int32);
            paramIdCA = new DataParameter(_CS, "IDCA", DbType.Int32);
            paramIdM = new DataParameter(_CS, "IDM", DbType.Int32);
            paramIdentifier = new DataParameter(_CS, "IDENTIFIER", DbType.AnsiString,SQLCst.UT_IDENTIFIER_LEN);
            paramExecOrder = new DataParameter(_CS, "EXECORDER", DbType.Int32);
            paramRequestMode = new DataParameter(_CS, "REQUESTMODE", DbType.AnsiString, SQLCst.UT_ENUM_MANDATORY_LEN);
            paramCEGroup = new DataParameter(_CS, "CEGROUP", DbType.AnsiString, SQLCst.UT_ENUM_MANDATORY_LEN);
            paramCEType = new DataParameter(_CS, "CETYPE", DbType.AnsiString, SQLCst.UT_ENUM_MANDATORY_LEN);
            paramCECombinedOperand = new DataParameter(_CS, "CECOMBINEDOPER", DbType.AnsiString, SQLCst.UT_ENUM_OPTIONAL_LEN);
            paramCECombinedType = new DataParameter(_CS, "CECOMBINEDTYPE", DbType.AnsiString, SQLCst.UT_ENUM_OPTIONAL_LEN);
            paramExDate = new DataParameter(_CS, "EXDATE", DbType.Date);
            paramEffectiveDate = new DataParameter(_CS, "EFFECTIVEDATE", DbType.Date);
            paramAdjMethod = new DataParameter(_CS, "ADJMETHOD", DbType.AnsiString, SQLCst.UT_ENUM_MANDATORY_LEN);
            paramAdjProcedures = new DataParameter(_CS, "ADJPROCEDURES", DbType.Xml);
        }
        #endregion
        #region SetParameters
        /// <summary>
        /// Valorisation des paramètres (INSERT / MISE A JOUR)
        /// </summary>
        /// <param name="pCorporateEvent"></param>
        /// <param name="pParameters"></param>
        // EG [33415/33420] 
        public void SetParameters(CorporateEvent pCorporateEvent, DataParameters pParameters)
        {
            pParameters["IDENTIFIER"].Value = pCorporateEvent.identifier;
            pParameters["EXECORDER"].Value = pCorporateEvent.execOrder;
            pParameters["REQUESTMODE"].Value = pCorporateEvent.mode.ToString();
            pParameters["CEGROUP"].Value = pCorporateEvent.group.ToString();
            pParameters["CETYPE"].Value = pCorporateEvent.type.ToString();
            pParameters["CECOMBINEDOPER"].Value = (pCorporateEvent.operandSpecified ? pCorporateEvent.operand.ToString() : Convert.DBNull);
            pParameters["CECOMBINEDTYPE"].Value = (pCorporateEvent.combinedTypeSpecified ? pCorporateEvent.combinedType.ToString() : Convert.DBNull);
            pParameters["EXDATE"].Value = (pCorporateEvent.exDateSpecified ? pCorporateEvent.exDate : Convert.DBNull);
            pParameters["EXECORDER"].Value = pCorporateEvent.execOrder;
            pParameters["EFFECTIVEDATE"].Value = (pCorporateEvent.effectiveDateSpecified ? pCorporateEvent.effectiveDate : Convert.DBNull);
            pParameters["ADJMETHOD"].Value = pCorporateEvent.adjMethod.ToString();

            EFS_SerializeInfoBase serializeInfo = new EFS_SerializeInfoBase(pCorporateEvent.procedure.GetType(), pCorporateEvent.procedure);
            // FI 20230103 [26204] Encoding.Unicode (puisque accepté par Oracle et sqlServer)
            StringBuilder sb = CacheSerializer.Serialize(serializeInfo, new UnicodeEncoding());

            pParameters["ADJPROCEDURES"].Value = sb.ToString();
        }
        #endregion SetParameters

        #region SetParametersInsert
        /// <summary>
        /// Valorisation des paramètres (INSERT)
        /// </summary>
        /// <param name="pID">ID CE</param>
        /// <param name="pIdCA">ID CA</param>
        /// <param name="pCorporateEvent">Corporate event</param>
        /// <param name="pParameters">Paramètres</param>
        public void SetParametersInsert(int pID, int pIdCA, CorporateEvent pCorporateEvent, DataParameters pParameters)
        {
            pParameters["ID"].Value = pID;
            pParameters["IDCA"].Value = pIdCA;
            SetParameters(pCorporateEvent, pParameters);
        }
        #endregion SetParametersInsert
        #region SetParametersUpdate
        /// <summary>
        /// Valorisation des paramètres (MISE A JOUR)
        /// </summary>
        /// <param name="pCorporateEvent">Corporate event</param>
        /// <param name="pParameters">Paramètres</param>
        public void SetParametersUpdate(CorporateEvent pCorporateEvent, DataParameters pParameters)
        {
            pParameters["ID"].Value = Convert.ToInt32(pCorporateEvent.spheresid);
            SetParameters(pCorporateEvent, pParameters);
        }
        #endregion SetParametersUpdate
        #endregion Methods
    }
    #endregion CEQueryEMBEDDED
    #region CNQueryEMBEDDED
    /// <summary>
    /// Classe des Queries et paramètres des notices additionnelles INTEGREES (insert, update, select, delete)
    /// </summary>
    /// EG 20211020 [XXXXX] Nouvelle gestion des notices (USEURLNOTICE)
    public class CNQueryEMBEDDED
    {
        #region Members
        private readonly string _CS;
        private DataParameter paramID;
        private DataParameter paramRefNotice;
        private DataParameter paramPubDate;
        private DataParameter paramNoticeFileName;
        private DataParameter paramUseUrlNotice;
        #endregion Members
        #region Constructor
        public CNQueryEMBEDDED(string pCS)
        {
            _CS = pCS;
            InitParameter();
        }
        #endregion
        #region Methods
        #region AddParameters
        /// <summary>
        /// Paramètres pour INSERTION / MISE A JOUR
        /// </summary>
        /// <param name="pParameters">Paramètres</param>
        /// EG 20211020 [XXXXX] Nouvelle gestion des notices (USEURLNOTICE)
        public void AddParameters(DataParameters pParameters)
        {
            pParameters.Add(paramID);
            pParameters.Add(paramRefNotice);
            pParameters.Add(paramPubDate);
            pParameters.Add(paramNoticeFileName);
            pParameters.Add(paramUseUrlNotice);
        }
        #endregion AddParameters

        #region GetQueryExist
        /// <summary>
        /// Requête existence d'une notice
        /// Recherche par :
        /// ● son ID (IDCA)
        /// ● sa référence de notice (IDCA + REFNOTICE)
        /// </summary>
        /// <param name="pCAWhereMode">Mode (Critère) de recherche</param>
        /// <returns></returns>
        public QueryParameters GetQueryExist(CATools.CAWhereMode pCAWhereMode)
        {
            DataParameters parameters = new DataParameters();
            StrBuilder sqlQuery = new StrBuilder();
            sqlQuery += SQLCst.SELECT + "1" + Cst.CrLf;
            sqlQuery += SQLCst.FROM_DBO + Cst.OTCml_TBL.CORPOACTIONNOTICE.ToString() + " cn" + Cst.CrLf;

            switch (pCAWhereMode)
            {
                case CATools.CAWhereMode.ID:
                    parameters.Add(paramID);
                    sqlQuery += SQLCst.WHERE + "(cn.IDCA = @ID)" + Cst.CrLf;
                    break;
                case CATools.CAWhereMode.NOTICE:
                    parameters.Add(paramID);
                    parameters.Add(paramRefNotice);
                    sqlQuery += SQLCst.WHERE + "(cn.IDCA = @ID) and (cn.REFNOTICE = @REFNOTICE)";
                    break;
            }
            QueryParameters ret = new QueryParameters(_CS, sqlQuery.ToString(), parameters);
            return ret;
        }
        #endregion GetQueryExist

        #region GetQueryDelete
        /// <summary>
        /// Requête de suppression d'une notice
        /// </summary>
        /// <returns></returns>
        public QueryParameters GetQueryDelete()
        {
            DataParameters parameters = new DataParameters();
            parameters.Add(paramID);
            StrBuilder sqlQuery = new StrBuilder();
            sqlQuery += SQLCst.DELETE_DBO + Cst.OTCml_TBL.CORPOACTIONNOTICE.ToString() + Cst.CrLf;
            sqlQuery += SQLCst.WHERE + "(IDCA = @ID)" + Cst.CrLf;
            QueryParameters ret = new QueryParameters(_CS, sqlQuery.ToString(), parameters);
            return ret;
        }
        #endregion GetQueryDelete
        #region GetQuerySelect
        /// <summary>
        /// Requête de sélection des notices
        /// </summary>
        /// <returns></returns>
        /// EG 20211020 [XXXXX] Nouvelle gestion des notices (USEURLNOTICE)
        public QueryParameters GetQuerySelect()
        {
            DataParameters parameters = new DataParameters();
            parameters.Add(paramID);

            StrBuilder sqlQuery = new StrBuilder();
            sqlQuery += SQLCst.SELECT + "cn.IDCA, cn.REFNOTICE, cn.PUBDATE, cn.NOTICEFILENAME, cn.USEURLNOTICE" + Cst.CrLf;
            sqlQuery += SQLCst.FROM_DBO + Cst.OTCml_TBL.CORPOACTIONNOTICE.ToString() + " cn" + Cst.CrLf;
            sqlQuery += SQLCst.WHERE + "(IDCA = @ID)" + Cst.CrLf;
            QueryParameters ret = new QueryParameters(_CS, sqlQuery.ToString(), parameters);
            return ret;
        }
        #endregion GetQuerySelect
        #region GetQueryInsert
        /// <summary>
        /// Requête d'insertion d'une notice
        /// </summary>
        /// <returns></returns>
        /// EG 20211020 [XXXXX] Nouvelle gestion des notices (USEURLNOTICE)
        public QueryParameters GetQueryInsert()
        {
            DataParameters parameters = new DataParameters();
            AddParameters(parameters);
            StrBuilder sqlQuery = new StrBuilder();
            sqlQuery += SQLCst.INSERT_INTO_DBO + Cst.OTCml_TBL.CORPOACTIONNOTICE.ToString() + Cst.CrLf;
            sqlQuery += @"(IDCA, REFNOTICE, PUBDATE, NOTICEFILENAME, USEURLNOTICE)" + Cst.CrLf;
            sqlQuery += @"values" + Cst.CrLf;
            sqlQuery += @"(@ID, @REFNOTICE, @PUBDATE, @NOTICEFILENAME, @USEURLNOTICE)" + Cst.CrLf;
            QueryParameters ret = new QueryParameters(_CS, sqlQuery.ToString(), parameters);
            return ret;
        }
        #endregion GetQueryInsert
        #region GetQueryUpdate
        /// <summary>
        /// Requête de mise à jour d'une notice
        /// </summary>
        /// <returns></returns>
        /// EG 20211020 [XXXXX] Nouvelle gestion des notices (USEURLNOTICE)
        public QueryParameters GetQueryUpdate()
        {
            DataParameters parameters = new DataParameters();
            AddParameters(parameters);
            StrBuilder sqlQuery = new StrBuilder();
            sqlQuery += SQLCst.UPDATE_DBO + Cst.OTCml_TBL.CORPOACTIONNOTICE.ToString() + Cst.CrLf;
            sqlQuery += SQLCst.SET + "REFNOTICE=@REFNOTICE, PUBDATE=@PUBDATE, NOTICEFILENAME=@NOTICEFILENAME, USEURLNOTICE=@USEURLNOTICE" + Cst.CrLf;
            sqlQuery += SQLCst.WHERE + "(IDCA=@ID)";
            QueryParameters ret = new QueryParameters(_CS, sqlQuery.ToString(), parameters);
            return ret;
        }
        #endregion GetQueryUpdate

        #region InitParameter
        /// <summary>
        /// Initialisation des paramètres
        /// </summary>
        /// EG 20211020 [XXXXX] Nouvelle gestion des notices (USEURLNOTICE)
        private void InitParameter()
        {
            paramID = new DataParameter(_CS, "ID", DbType.Int32);
            paramRefNotice = new DataParameter(_CS, "REFNOTICE", DbType.AnsiString, SQLCst.UT_IDENTIFIER_LEN);
            paramPubDate = new DataParameter(_CS, "PUBDATE", DbType.Date);
            paramNoticeFileName = new DataParameter(_CS, "NOTICEFILENAME", DbType.AnsiString, SQLCst.UT_ENUM_MANDATORY_LEN);
            paramUseUrlNotice = new DataParameter(_CS, "USEURLNOTICE", DbType.Boolean);
        }
        #endregion
        #region SetParameters
        /// <summary>
        /// Valorisation des paramètres
        /// </summary>
        /// <param name="pID"></param>
        /// <param name="pRefNotice"></param>
        /// <param name="pParameters"></param>
        /// EG 20211020 [XXXXX] Nouvelle gestion des notices (USEURLNOTICE)
        public void SetParameters(int pID, RefNoticeIdentification pRefNotice, DataParameters pParameters)
        {
            pParameters["ID"].Value = pID;
            pParameters["REFNOTICE"].Value = pRefNotice.value;
            pParameters["PUBDATE"].Value = pRefNotice.pubDate;
            pParameters["NOTICEFILENAME"].Value = pRefNotice.fileName.ToString();
            pParameters["USEURLNOTICE"].Value = pRefNotice.useurlnoticeSpecified && pRefNotice.useurlnotice;
        }
        #endregion SetParameters
        #endregion Methods
    }
    #endregion CNQueryEMBEDDED
    #region DAQueryEMBEDDED
    /// <summary>
    /// Classe des Queries et paramètres des DC liés à une CA INTEGREES (insert, update, select, delete)
    /// </summary>
    public class DAQueryEMBEDDED
    {
        #region Members
        private readonly string _CS;

        private DataParameter paramIdCEC;
        private DataParameter paramEffectiveDate; // Use only for GetQuerySelectCandidate

        private DataParameter paramIdCE;
        private DataParameter paramIdA_Entity;
        private DataParameter paramIdDC;
        private DataParameter paramIdDA;

        private DataParameter paramCategory;
        private DataParameter paramReadyState;
        private DataParameter paramAdjStatus;
        private DataParameter paramAdjMethod;
        private DataParameter paramRFactor;
        private DataParameter paramRFactor_Rnd;
        private DataParameter paramRFactor_Retained;


        private DataParameter paramContractSize;
        private DataParameter paramContractMultiplier;

        private DataParameter paramExContractSize;
        private DataParameter paramExContractSize_Rnd;
        private DataParameter paramExContractMultiplier;
        private DataParameter paramExContractMultiplier_Rnd;

        #endregion Members
        #region Constructor
        public DAQueryEMBEDDED(string pCS)
        {
            _CS = pCS;
            InitParameter();
        }
        #endregion
        #region Methods
        #region AddParameters
        /// <summary>
        /// Paramètres communs à l'INSERTION / MISE A JOUR
        /// </summary>
        /// <param name="pParameters">Paramèetres</param>
        private void AddParameters(DataParameters pParameters)
        {
            pParameters.Add(paramIdCE);
            pParameters.Add(paramIdA_Entity);
            pParameters.Add(paramIdDA);

            pParameters.Add(paramReadyState);
            pParameters.Add(paramAdjStatus);
            pParameters.Add(paramAdjMethod);
            pParameters.Add(paramRFactor);
            pParameters.Add(paramRFactor_Rnd);
            pParameters.Add(paramRFactor_Retained);

            pParameters.Add(paramContractSize);
            pParameters.Add(paramContractMultiplier);
            pParameters.Add(paramExContractSize);
            pParameters.Add(paramExContractSize_Rnd);
            pParameters.Add(paramExContractMultiplier);
            pParameters.Add(paramExContractMultiplier_Rnd);
        }
        #endregion AddParameters
        #region AddParamterInsert
        /// <summary>
        /// Paramètres pour INSERTION
        /// </summary>
        /// <param name="pParameters">Paramètres</param>
        public void AddParametersInsert(DataParameters pParameters)
        {
            pParameters.Add(paramIdCEC);
            pParameters.Add(paramIdDC);
            pParameters.Add(paramCategory);
            AddParameters(pParameters);
        }
        #endregion AddParamterInsert
        #region AddParametersUpdate
        /// <summary>
        /// Paramètres pour MISE A JOUR
        /// </summary>
        /// <param name="pParameters">Paramètres</param>
        public void AddParametersUpdate(DataParameters pParameters)
        {
            AddParameters(pParameters);
        }
        #endregion AddParametersUpdate

        #region GetQueryDelete
        /// <summary>
        /// Requête de suppression d'une CA PUBLIEE
        /// </summary>
        /// <returns></returns>
        public QueryParameters GetQueryDelete()
        {
            DataParameters parameters = new DataParameters();
            parameters.Add(paramIdCE);
            parameters.Add(paramIdA_Entity);
            parameters.Add(paramIdDA);
            StrBuilder sqlQuery = new StrBuilder();
            sqlQuery += SQLCst.DELETE_DBO + Cst.OTCml_TBL.CORPOEVENTDATTRIB.ToString() + Cst.CrLf;
            sqlQuery += SQLCst.WHERE + "(IDCE=@IDCE) and (IDA_ENTITY=@IDA_ENTITY) and (IDDA=@IDDA)" + Cst.CrLf;
            QueryParameters ret = new QueryParameters(_CS, sqlQuery.ToString(), parameters);
            return ret;
        }
        #endregion GetQueryDelete
        #region GetQueryExist
        /// <summary>
        /// Requête de contrôle d'existence des DA rattachés à une CA (CE)
        /// </summary>
        /// <returns></returns>
        public QueryParameters GetQueryExist()
        {
            DataParameters parameters = new DataParameters();
            parameters.Add(paramIdCE);
            StrBuilder sqlQuery = new StrBuilder();
            sqlQuery += SQLCst.SELECT + "1" + Cst.CrLf;
            sqlQuery += SQLCst.FROM_DBO + Cst.OTCml_TBL.CORPOEVENTDATTRIB.ToString() + " ceda" + Cst.CrLf;
            sqlQuery += SQLCst.WHERE + "(ceda.IDCE = @IDCE)" + Cst.CrLf;
            QueryParameters ret = new QueryParameters(_CS, sqlQuery.ToString(), parameters);
            return ret;
        }
        #endregion GetQueryExist

        #region GetQuerySelectCandidate
        /// <summary>
        /// Requête de sélection des DAs candidates à traitement de CA 
        /// ● pour un DERIVATIVE ATTRIB donné
        /// </summary>
        /// <returns></returns>
        public QueryParameters GetQuerySelectCandidate()
        {
            DataParameters parameters = new DataParameters();
            parameters.Add(paramIdCE);
            parameters.Add(paramIdDC);
            parameters.Add(paramIdA_Entity);
            parameters.Add(paramEffectiveDate);

            // FI 20220524 [XXXXX] Suppression de la jointure sur MATURITYRULE puisque inutile
            string sqlQryDA = @"select  da.IDDERIVATIVEATTRIB as IDDA
            from dbo.DERIVATIVEATTRIB da 
            inner join dbo.MATURITY ma on (ma.IDMATURITY = da.IDMATURITY)
            where (da.IDDC = @IDDC) and (isnull(ma.MATURITYDATE,@EFFECTIVEDATE) >= @EFFECTIVEDATE)" + Cst.CrLf;

            string sqlQryCorpo = @"select da.IDDA
            from dbo.CORPOEVENTDATTRIB da 
            where (da.IDCE = @IDCE) and (da.IDA_ENTITY = @IDA_ENTITY) and (da.IDDC = @IDDC)" + Cst.CrLf;

            // EG 20160105 [34091] Remove 'As' to SubQuery (NewDA|DelDA|UpdDA) 
            // RD 20180614 [24025] Si CONTRACTMULTIPLIER est NULL, prendre FACTOR
            // FI 20220524 [XXXXX] Suppression de la jointure sur MATURITYRULE puisque inutile
            // EG 20240722 [XXXXX] Add Restriction on DTENABLED for DERIVATIVEATTRIB CANDIDATES
            string sqlQuery = @"select da.IDDERIVATIVEATTRIB as IDDA, da.IDDC, da.IDMATURITY, ma.MATURITYDATE, ma.MATURITYMONTHYEAR, 
            case when da.CONTRACTMULTIPLIER is null then (case when dc.CONTRACTMULTIPLIER is null then isnull(da.FACTOR,dc.FACTOR) else dc.CONTRACTMULTIPLIER end ) else da.CONTRACTMULTIPLIER end as CONTRACTMULTIPLIER,
            isnull(da.FACTOR,dc.FACTOR) as CONTRACTSIZE, main.ROWSTATE 
            from dbo.DERIVATIVEATTRIB da 
            inner join dbo.DERIVATIVECONTRACT dc on (dc.IDDC = da.IDDC)
            inner join dbo.MATURITY ma on (ma.IDMATURITY = da.IDMATURITY) 
            inner join
            (
                select distinct IDDA, 'Added' as ROWSTATE from (" + sqlQryDA + SQLCst.EXCEPT + Cst.CrLf + sqlQryCorpo + @") NewDA
                union
                select distinct IDDA, 'Deleted' as ROWSTATE from (" + sqlQryCorpo + SQLCst.EXCEPT + Cst.CrLf + sqlQryDA + @") DelDA
                union
                select distinct IDDA, 'Modified' as ROWSTATE from (" + sqlQryDA + SQLCst.INTERSECT + Cst.CrLf + sqlQryCorpo + @") UpdDA
            ) main on (main.IDDA = da.IDDERIVATIVEATTRIB)
            where(da.DTENABLED < @EFFECTIVEDATE) and(isnull(da.DTDISABLED, @EFFECTIVEDATE) >= @EFFECTIVEDATE)" + Cst.CrLf;
            QueryParameters ret = new QueryParameters(_CS, sqlQuery, parameters);
            return ret;
        }
        #endregion GetQuerySelectCandidate

        #region GetQueryInsert
        /// <summary>
        /// Requête d'insertion d'un CA INTEGREE
        /// </summary>
        /// <returns></returns>
        public QueryParameters GetQueryInsert()
        {
            DataParameters parameters = new DataParameters();
            AddParametersInsert(parameters);
            StrBuilder sqlQuery = new StrBuilder();
            sqlQuery += SQLCst.INSERT_INTO_DBO + Cst.OTCml_TBL.CORPOEVENTDATTRIB.ToString() + Cst.CrLf;
            sqlQuery += "(IDCEC, IDCE, IDA_ENTITY, IDDC, CATEGORY, IDDA, READYSTATE, ADJSTATUS, ADJMETHOD, " + Cst.CrLf;
            sqlQuery += "RFACTOR, RFACTOR_RND, RFACTOR_RETAINED, CMULTIPLIER, CSIZE, " + Cst.CrLf;
            sqlQuery += "EXCMULTIPLIER, EXCMULTIPLIER_RND, EXCSIZE, EXCSIZE_RND)" + Cst.CrLf;
            sqlQuery += "values" + Cst.CrLf;
            sqlQuery += "(@IDCEC, @IDCE, @IDA_ENTITY, @IDDC, @CATEGORY, @IDDA, @READYSTATE, @ADJSTATUS, @ADJMETHOD, " + Cst.CrLf;
            sqlQuery += "@RFACTOR, @RFACTOR_RND, @RFACTOR_RETAINED, @CMULTIPLIER, @CSIZE, " + Cst.CrLf;
            sqlQuery += "@EXCMULTIPLIER, @EXCMULTIPLIER_RND, @EXCSIZE, @EXCSIZE_RND)" + Cst.CrLf;
            QueryParameters ret = new QueryParameters(_CS, sqlQuery.ToString(), parameters);
            return ret;
        }
        #endregion GetQueryInsert
        #region GetQueryUpdate
        /// <summary>
        /// Requête de MISE A JOUR d'un CA INTEGREE
        /// </summary>
        /// <returns></returns>
        public QueryParameters GetQueryUpdate()
        {
            DataParameters parameters = new DataParameters();
            AddParametersUpdate(parameters);
            StrBuilder sqlQuery = new StrBuilder();
            sqlQuery += SQLCst.UPDATE_DBO + Cst.OTCml_TBL.CORPOEVENTDATTRIB.ToString() + Cst.CrLf;
            sqlQuery += SQLCst.SET + "READYSTATE=@READYSTATE, ADJSTATUS=@ADJSTATUS, ADJMETHOD=@ADJMETHOD, " + Cst.CrLf;
            sqlQuery += "RFACTOR=@RFACTOR, RFACTOR_RND=@RFACTOR_RND, RFACTOR_RETAINED=@RFACTOR_RETAINED, CMULTIPLIER=@CMULTIPLIER, CSIZE=@CSIZE, " + Cst.CrLf;
            sqlQuery += "EXCMULTIPLIER=@EXCMULTIPLIER, EXCMULTIPLIER_RND=@EXCMULTIPLIER_RND, EXCSIZE=@EXCSIZE, EXCSIZE_RND=@EXCSIZE_RND" + Cst.CrLf;
            sqlQuery += SQLCst.WHERE + "(IDCE=@IDCE) and (IDA_ENTITY=@IDA_ENTITY) and (IDDA=@IDDA)" + Cst.CrLf;
            QueryParameters ret = new QueryParameters(_CS, sqlQuery.ToString(), parameters);
            return ret;
        }
        #endregion GetQueryUpdate

        #region InitParameter
        /// <summary>
        /// Initialisation des paramètres
        /// </summary>
        private void InitParameter()
        {
            paramIdCEC = new DataParameter(_CS, "IDCEC", DbType.Int32);

            paramIdA_Entity = new DataParameter(_CS, "IDA_ENTITY", DbType.Int32);
            paramIdCE = new DataParameter(_CS, "IDCE", DbType.Int32);
            paramIdDC = new DataParameter(_CS, "IDDC", DbType.Int32);
            paramEffectiveDate = new DataParameter(_CS, "EFFECTIVEDATE", DbType.Date);

            paramIdDA = new DataParameter(_CS, "IDDA", DbType.Int32);
            paramCategory = new DataParameter(_CS, "CATEGORY", DbType.AnsiString, SQLCst.UT_CFICODE_LEN);
            paramReadyState = new DataParameter(_CS, "READYSTATE", DbType.AnsiString, SQLCst.UT_ENUM_MANDATORY_LEN);
            paramAdjStatus = new DataParameter(_CS, "ADJSTATUS", DbType.AnsiString, SQLCst.UT_ENUM_MANDATORY_LEN);
            paramAdjMethod = new DataParameter(_CS, "ADJMETHOD", DbType.AnsiString, SQLCst.UT_ENUM_MANDATORY_LEN);

            paramRFactor = new DataParameter(_CS, "RFACTOR", DbType.Decimal);
            paramRFactor_Rnd = new DataParameter(_CS, "RFACTOR_RND", DbType.Decimal);
            paramRFactor_Retained = new DataParameter(_CS, "RFACTOR_RETAINED", DbType.Decimal);

            paramContractSize = new DataParameter(_CS, "CSIZE", DbType.Decimal);
            paramContractMultiplier = new DataParameter(_CS, "CMULTIPLIER", DbType.Decimal);

            paramExContractSize = new DataParameter(_CS, "EXCSIZE", DbType.Decimal);
            paramExContractSize_Rnd = new DataParameter(_CS, "EXCSIZE_RND", DbType.Decimal);
            paramExContractMultiplier = new DataParameter(_CS, "EXCMULTIPLIER", DbType.Decimal);
            paramExContractMultiplier_Rnd = new DataParameter(_CS, "EXCMULTIPLIER_RND", DbType.Decimal);
        }
        #endregion InitParameter

        #region SetParameters
        /// <summary>
        /// Valorisation des paramètres (INSERTION / MISE A JOUR)
        /// </summary>
        private void SetParameters(CorporateEventDAttrib pCorporateEventDAttrib, DataParameters pParameters)
        {
            pParameters["READYSTATE"].Value = pCorporateEventDAttrib.readyState;
            pParameters["ADJSTATUS"].Value = pCorporateEventDAttrib.adjStatus;
            pParameters["ADJMETHOD"].Value = pCorporateEventDAttrib.adjMethod;

            SetSimpleUnitParameters("RFACTOR", pCorporateEventDAttrib.rFactorSpecified, pCorporateEventDAttrib.rFactor, pParameters);
            pParameters["RFACTOR_RETAINED"].Value = pCorporateEventDAttrib.rFactorRetained;

            CalculationCumData cumData = pCorporateEventDAttrib.cumData;
            pParameters["CSIZE"].Value = (cumData.contractSizeSpecified ? cumData.contractSize.DecValue : Convert.DBNull);
            pParameters["CMULTIPLIER"].Value = (cumData.contractMultiplierSpecified ? cumData.contractMultiplier.DecValue : Convert.DBNull);

            CalculationExData exData = pCorporateEventDAttrib.exData;
            if (null == exData)
                exData = new CalculationExData();
            SetSimpleUnitParameters("EXCSIZE", exData.contractSizeSpecified, exData.contractSize, pParameters);
            SetSimpleUnitParameters("EXCMULTIPLIER", exData.contractMultiplierSpecified, exData.contractMultiplier, pParameters);
        }
        #endregion SetParameters
        #region SetParametersDelete
        /// <summary>
        /// Valorisation des paramètres (DELETE)
        /// </summary>
        // EG 20151002 pParameters["IDDC"]
        public void SetParametersDelete(CorporateEventDAttrib pCorporateEventDAttrib, DataParameters pParameters)
        {
            pParameters["IDCE"].Value = pCorporateEventDAttrib.idCE;
            pParameters["IDA_ENTITY"].Value = pCorporateEventDAttrib.idA_Entity;
            //pParameters["IDDC"].Value = pCorporateEventDAttrib.idDC;
            pParameters["IDDA"].Value = pCorporateEventDAttrib.idDA;
        }
        #endregion SetParametersDelete
        #region SetParametersInsert
        /// <summary>
        /// Valorisation des paramètres (INSERTION)
        /// </summary>
        public void SetParametersInsert(CorporateEventDAttrib pCorporateEventDAttrib, DataParameters pParameters)
        {
            pParameters["IDCEC"].Value = pCorporateEventDAttrib.idCEC;
            pParameters["IDCE"].Value = pCorporateEventDAttrib.idCE;
            pParameters["IDA_ENTITY"].Value = pCorporateEventDAttrib.idA_Entity;
            pParameters["IDDC"].Value = pCorporateEventDAttrib.idDC;
            pParameters["IDDA"].Value = pCorporateEventDAttrib.idDA;
            pParameters["CATEGORY"].Value = ReflectionTools.ConvertEnumToString<CfiCodeCategoryEnum>(pCorporateEventDAttrib.category);
            SetParameters(pCorporateEventDAttrib, pParameters);
        }
        #endregion SetParametersInsert
        #region SetParametersUpdate
        /// <summary>
        /// Valorisation des paramètres (MISE A JOUR)
        /// </summary>
        public void SetParametersUpdate(CorporateEventDAttrib pCorporateEventDAttrib, DataParameters pParameters)
        {
            pParameters["IDCE"].Value = pCorporateEventDAttrib.idCE;
            pParameters["IDA_ENTITY"].Value = pCorporateEventDAttrib.idA_Entity;
            pParameters["IDDA"].Value = pCorporateEventDAttrib.idDA;
            SetParameters(pCorporateEventDAttrib, pParameters);
        }
        #endregion SetParametersUpdate
        #region SetSimpleUnitParameters
        private void SetSimpleUnitParameters(string pElement, bool pIsSpecified, SimpleUnit pSimpleUnit, DataParameters pParameters)
        {
            if (pIsSpecified)
            {
                pParameters[pElement].Value = pSimpleUnit.value.DecValue;
                pParameters[pElement + "_RND"].Value = (pSimpleUnit.valueRoundedSpecified ? pSimpleUnit.valueRounded.DecValue : pSimpleUnit.value.DecValue);
            }
            else
            {
                pParameters[pElement].Value = Convert.DBNull;
                pParameters[pElement + "_RND"].Value = Convert.DBNull;
            }
        }
        #endregion SetSimpleUnitParameters
        #endregion Methods
    }
    #endregion DAQueryEMBEDDED
    #region DCQueryEMBEDDED
    /// <summary>
    /// Classe des Queries et paramètres des DC liés à une CA INTEGREES (insert, update, select, delete)
    /// </summary>
    public class DCQueryEMBEDDED
    {
        #region Members
        private readonly string _CS;
        private DataParameter paramIdCEC;

        private DataParameter paramIdM;                // use By GetQuerySelectCandidate only
        private DataParameter paramEffectiveDate;       // use By GetQuerySelectCandidate only
        private DataParameter paramAssetCategory_UNL;
        private DataParameter paramIdAsset_UNL;

        private DataParameter paramIdA_Entity;
        private DataParameter paramIdCE;
        private DataParameter paramIdDC;

        private DataParameter paramCategory;
        private DataParameter paramReadyState;
        private DataParameter paramAdjStatus;
        private DataParameter paramAdjMethod;
        private DataParameter paramRFactor;
        private DataParameter paramRFactor_Rnd;
        private DataParameter paramRFactor_Retained;

        private DataParameter paramContractSize;
        private DataParameter paramContractMultiplier;

        private DataParameter paramExContractSize;
        private DataParameter paramExContractSize_Rnd;
        private DataParameter paramExContractMultiplier;
        private DataParameter paramExContractMultiplier_Rnd;

        private DataParameter paramRenamingMethod;
        private DataParameter paramRenamingValue;
        private DataParameter paramExRenamingValue;

        #endregion Members
        #region Constructor
        public DCQueryEMBEDDED(string pCS)
        {
            _CS = pCS;
            InitParameter();
        }
        #endregion
        #region Methods
        #region AddParameters
        /// <summary>
        /// Paramètres communs à l'INSERTION / MISE A JOUR
        /// </summary>
        /// <param name="pParameters">Paramèetres</param>
        private void AddParameters(DataParameters pParameters)
        {
            pParameters.Add(paramIdCE);
            pParameters.Add(paramIdA_Entity);
            pParameters.Add(paramIdDC);

            pParameters.Add(paramReadyState);
            pParameters.Add(paramAdjStatus);
            pParameters.Add(paramAdjMethod);
            pParameters.Add(paramRFactor);
            pParameters.Add(paramRFactor_Rnd);
            pParameters.Add(paramRFactor_Retained);

            pParameters.Add(paramRenamingMethod);
            pParameters.Add(paramRenamingValue);
            pParameters.Add(paramExRenamingValue);

            pParameters.Add(paramContractMultiplier);
            pParameters.Add(paramContractSize);

            pParameters.Add(paramExContractMultiplier);
            pParameters.Add(paramExContractMultiplier_Rnd);
            pParameters.Add(paramExContractSize);
            pParameters.Add(paramExContractSize_Rnd);

        }
        #endregion AddParameters
        #region AddParamterInsert
        /// <summary>
        /// Paramètres pour INSERTION
        /// </summary>
        /// <param name="pParameters">Paramètres</param>
        public void AddParametersInsert(DataParameters pParameters)
        {
            pParameters.Add(paramIdCEC);
            pParameters.Add(paramAssetCategory_UNL);
            pParameters.Add(paramIdAsset_UNL);
            pParameters.Add(paramCategory);
            AddParameters(pParameters);
        }
        #endregion AddParamterInsert
        #region AddParametersUpdate
        /// <summary>
        /// Paramètres pour MISE A JOUR
        /// </summary>
        /// <param name="pParameters">Paramètres</param>
        public void AddParametersUpdate(DataParameters pParameters)
        {
            AddParameters(pParameters);
        }
        #endregion AddParametersUpdate

        #region GetQueryDelete
        /// <summary>
        /// Requête de suppression d'une CA PUBLIEE
        /// </summary>
        /// <returns></returns>
        public QueryParameters GetQueryDelete()
        {
            DataParameters parameters = new DataParameters();
            parameters.Add(paramIdCE);
            parameters.Add(paramIdA_Entity);
            parameters.Add(paramIdDC);
            StrBuilder sqlQuery = new StrBuilder();
            sqlQuery += SQLCst.DELETE_DBO + Cst.OTCml_TBL.CORPOEVENTCONTRACT.ToString() + Cst.CrLf;
            sqlQuery += SQLCst.WHERE + "(IDCE=@IDCE) and (IDA_ENTITY=@IDA_ENTITY) and (IDDC=@IDDC)";
            QueryParameters ret = new QueryParameters(_CS, sqlQuery.ToString(), parameters);
            return ret;
        }
        #endregion GetQueryDelete
        #region GetQueryExist
        /// <summary>
        /// Requête de contrôle d'existence des contrats rattachés à une CA (CE)
        /// </summary>
        /// <returns></returns>
        public QueryParameters GetQueryExist(CATools.DCWhereMode pDCWhereMode)
        {
            DataParameters parameters = new DataParameters();

            StrBuilder sqlQuery = new StrBuilder();
            sqlQuery += SQLCst.SELECT + "1" + Cst.CrLf;
            sqlQuery += SQLCst.FROM_DBO + Cst.OTCml_TBL.CORPOEVENTCONTRACT.ToString() + " cec" + Cst.CrLf;


            switch (pDCWhereMode)
            {
                case CATools.DCWhereMode.IDCE_READYSTATE:
                    parameters.Add(paramIdCE);
                    parameters.Add(paramReadyState);
                    sqlQuery += SQLCst.WHERE + "(cec.IDCE = @IDCE) and (cec.READYSTATE = @READYSTATE)" + Cst.CrLf;
                    break;
                case CATools.DCWhereMode.IDCE:
                    parameters.Add(paramIdCE);
                    sqlQuery += SQLCst.WHERE + "(cec.IDCE = @IDCE)" + Cst.CrLf;
                    break;
            }
            QueryParameters ret = new QueryParameters(_CS, sqlQuery.ToString(), parameters);
            return ret;
        }
        #endregion GetQueryExist
        #region GetQueryInsert
        /// <summary>
        /// Requête d'insertion d'un CA INTEGREE
        /// </summary>
        /// <returns></returns>
        public QueryParameters GetQueryInsert()
        {
            DataParameters parameters = new DataParameters();
            AddParametersInsert(parameters);
            StrBuilder sqlQuery = new StrBuilder();
            sqlQuery += SQLCst.INSERT_INTO_DBO + Cst.OTCml_TBL.CORPOEVENTCONTRACT.ToString() + Cst.CrLf;
            sqlQuery += "(IDCEC, IDCE, IDA_ENTITY, ASSETCATEGORY_UNL, IDASSET_UNL, IDDC, CATEGORY, READYSTATE, ADJSTATUS, ADJMETHOD, " + Cst.CrLf;
            sqlQuery += "RENAMINGMETHOD, RENAMINGVALUE, EXRENAMINGVALUE, RFACTOR, RFACTOR_RND, RFACTOR_RETAINED, CMULTIPLIER, CSIZE, " + Cst.CrLf;
            sqlQuery += "EXCMULTIPLIER, EXCMULTIPLIER_RND, EXCSIZE, EXCSIZE_RND)" + Cst.CrLf;
            sqlQuery += "values" + Cst.CrLf;
            sqlQuery += "(@IDCEC, @IDCE, @IDA_ENTITY, @ASSETCATEGORY_UNL, @IDASSET_UNL, @IDDC, @CATEGORY, @READYSTATE, @ADJSTATUS, @ADJMETHOD, " + Cst.CrLf;
            sqlQuery += "@RENAMINGMETHOD, @RENAMINGVALUE, @EXRENAMINGVALUE, @RFACTOR, @RFACTOR_RND, @RFACTOR_RETAINED, @CMULTIPLIER, @CSIZE, " + Cst.CrLf;
            sqlQuery += "@EXCMULTIPLIER, @EXCMULTIPLIER_RND, @EXCSIZE, @EXCSIZE_RND)" + Cst.CrLf;
            QueryParameters ret = new QueryParameters(_CS, sqlQuery.ToString(), parameters);
            return ret;
        }
        #endregion GetQueryInsert
        #region GetQueryUpdate
        /// <summary>
        /// Requête de MISE A JOUR d'un CA INTEGREE
        /// </summary>
        /// <returns></returns>
        public QueryParameters GetQueryUpdate()
        {
            DataParameters parameters = new DataParameters();
            AddParametersUpdate(parameters);
            StrBuilder sqlQuery = new StrBuilder();
            sqlQuery += SQLCst.UPDATE_DBO + Cst.OTCml_TBL.CORPOEVENTCONTRACT.ToString() + Cst.CrLf;
            sqlQuery += SQLCst.SET + "READYSTATE=@READYSTATE, ADJSTATUS=@ADJSTATUS, ADJMETHOD=@ADJMETHOD, " + Cst.CrLf;
            sqlQuery += "RENAMINGMETHOD=@RENAMINGMETHOD, RENAMINGVALUE=@RENAMINGVALUE, EXRENAMINGVALUE=@EXRENAMINGVALUE, " + Cst.CrLf;
            sqlQuery += "RFACTOR=@RFACTOR, RFACTOR_RND=@RFACTOR_RND, RFACTOR_RETAINED=@RFACTOR_RETAINED, CMULTIPLIER=@CMULTIPLIER, CSIZE=@CSIZE, " + Cst.CrLf;
            sqlQuery += "EXCMULTIPLIER=@EXCMULTIPLIER, EXCMULTIPLIER_RND=@EXCMULTIPLIER_RND, EXCSIZE=@EXCSIZE, EXCSIZE_RND=@EXCSIZE_RND" + Cst.CrLf;
            sqlQuery += SQLCst.WHERE + "(IDCE=@IDCE) and (IDA_ENTITY=@IDA_ENTITY) and (IDDC=@IDDC)";
            QueryParameters ret = new QueryParameters(_CS, sqlQuery.ToString(), parameters);
            return ret;
        }
        #endregion GetQueryUpdate

        #region GetQuerySelect
        /// <summary>
        /// Requête de sélection des DC liée à la CA INTEGREE
        /// </summary>
        /// <param name="pDCWhereMode">Mode (Critère) de recherche</param>
        /// <returns></returns>
        public QueryParameters GetQuerySelect(CATools.DCWhereMode pDCWhereMode)
        {
            DataParameters parameters = new DataParameters();

            string sqlQuery = string.Empty;

            switch (pDCWhereMode)
            {
                case CATools.DCWhereMode.IDDC:
                    parameters.Add(paramIdCE);
                    parameters.Add(paramIdA_Entity);
                    parameters.Add(paramIdDC);
                    sqlQuery += SQLCst.SELECT + "cec.IDCEC" + Cst.CrLf;
                    sqlQuery += SQLCst.FROM_DBO + Cst.OTCml_TBL.CORPOEVENTCONTRACT.ToString() + " cec" + Cst.CrLf;
                    sqlQuery += SQLCst.WHERE + "(IDCE=@IDCE) and (IDA_ENTITY=@IDA_ENTITY) and (IDDC=@IDDC)";
                    break;
                case CATools.DCWhereMode.ID:
                    parameters.Add(paramIdCEC);
                    sqlQuery += SQLCst.SELECT + "ce.IDCE, ce.IDA_ENTITY, ce.IDDC" + Cst.CrLf;
                    sqlQuery += SQLCst.FROM_DBO + Cst.OTCml_TBL.CORPOEVENTCONTRACT.ToString() + " cec" + Cst.CrLf;
                    sqlQuery += SQLCst.WHERE + "(cec.ICEC = @IDCEC)";
                    break;
            }
            QueryParameters ret = new QueryParameters(_CS, sqlQuery.ToString(), parameters);
            return ret;
        }
        #endregion GetQuerySelect
        #region GetQuerySelectCandidate
        /// <summary>
        /// Requête de sélection des DERIVATECONTRACT candidats à traitement
        /// ● pour un marché 
        /// ● pour un sous-jacent donné (Catégorie + ID)
        /// ● pour dates de maturité >= date d'effet de la CA
        /// </summary>
        /// <returns></returns>
        /// EG 20130716 Jointure externe sur DERIVATIVECONTRAXCT et MATURITY
        // EG 20150114 [20676] Add FUTVALUATIONMETHOD 
        // EG 20220621 [34623] Modifications des queries pour CA sur multi-entité (Maj Restriction DTDISABLED)
        public QueryParameters GetQuerySelectCandidate()
        {
            DataParameters parameters = new DataParameters();
            parameters.Add(paramIdCE);
            parameters.Add(paramIdM);
            parameters.Add(paramIdA_Entity);
            parameters.Add(paramAssetCategory_UNL);
            parameters.Add(paramIdAsset_UNL);
            parameters.Add(paramEffectiveDate);
            parameters.Add(paramCategory);

            string sqlQryDC = @"select dc.IDDC
            from dbo.DERIVATIVECONTRACT dc
            inner join dbo.ENTITYMARKET em on (em.IDM = dc.IDM) and (em.IDA = @IDA_ENTITY)
            left outer join dbo.DERIVATIVEATTRIB da on (da.IDDC = dc.IDDC)
            left outer join dbo.MATURITY ma on (ma.IDMATURITY = da.IDMATURITY) and (isnull(ma.MATURITYDATE , @EFFECTIVEDATE) >= @EFFECTIVEDATE)
            where (dc.IDM = @IDM) and (dc.IDASSET_UNL = @IDASSET_UNL) and (dc.ASSETCATEGORY = @ASSETCATEGORY_UNL) and 
            (dc.DTENABLED < @EFFECTIVEDATE) and (isnull(dc.DTDISABLED , @EFFECTIVEDATE)>= @EFFECTIVEDATE) and ((dc.CATEGORY = @CATEGORY) or (@CATEGORY is null))" + Cst.CrLf;

            string sqlQryCorpo = @"select cec.IDDC
            from dbo.CORPOEVENTCONTRACT cec
            inner join dbo.CORPOEVENT ce on (ce.IDCE = cec.IDCE)
            inner join dbo.CORPOACTION ca on (ca.IDCA = ce.IDCA) and (ca.IDM = @IDM)
            where (cec.IDA_ENTITY = @IDA_ENTITY) and (cec.IDASSET_UNL = @IDASSET_UNL) and (cec.ASSETCATEGORY_UNL = @ASSETCATEGORY_UNL) and (cec.IDCE = @IDCE)" + Cst.CrLf;
            
            // EG 20150114 [20676] Add FUTVALUATIONMETHOD
            // Tous les nouveaux DCs non encore présents dans CORPOEVENTCONTRACT (Insert)
            // Tous les DCs présents dans CORPOEVENTCONTRACT et non candidats (Delete)
            // Tous les DCs présents dans CORPOEVENTCONTRACT (Update)
            // EG 20160105 [34091] Remove 'As' to SubQuery (NewDC|DelDC|UpdDC)
            // RD 20180614 [24025] Si CONTRACTMULTIPLIER est NULL, prendre FACTOR
            // EG 20190121 [23249] Add CASHFLOWCALCMETHOD pour Application arrondi sur EQP avant|après application de la quantité en position sur le trade
            // FI 20190409 [23249] Add dbo. sur MARKET
            string sqlQuery = @"select dc.IDDC, dc.IDENTIFIER, isnull(dc.CONTRACTMULTIPLIER,dc.FACTOR) as CONTRACTMULTIPLIER, dc.FACTOR as CONTRACTSIZE, 
            dc.CONTRACTSYMBOL, dc.CONTRACTATTRIBUTE, dc.CATEGORY, dc.PRICEDECLOCATOR, dc.STRIKEDECLOCATOR, main.ROWSTATE, dc.FUTVALUATIONMETHOD,
            isnull(dc.CASHFLOWCALCMETHOD, mk.CASHFLOWCALCMETHOD) as CASHFLOWCALCMETHOD
            from dbo.DERIVATIVECONTRACT dc
            inner join dbo.MARKET mk on (mk.IDM = dc.IDM)
            inner join
            (
                select distinct IDDC, 'Added' as ROWSTATE from (" + sqlQryDC + SQLCst.EXCEPT + Cst.CrLf + sqlQryCorpo + @") NewDC
                union
                select distinct IDDC, 'Deleted' as ROWSTATE from (" + sqlQryCorpo + SQLCst.EXCEPT + Cst.CrLf + sqlQryDC + @") DelDC
                union
                select distinct IDDC, 'Modified' as ROWSTATE from (" + sqlQryDC + SQLCst.INTERSECT + Cst.CrLf + sqlQryCorpo + @") UpdDC
            ) main on (main.IDDC = dc.IDDC) and (dc.DTENABLED < @EFFECTIVEDATE) and (isnull(dc.DTDISABLED,@EFFECTIVEDATE) >= @EFFECTIVEDATE)" + Cst.CrLf;

            QueryParameters ret = new QueryParameters(_CS, sqlQuery, parameters);

            return ret;
        }
        #endregion GetQuerySelectCandidate

        #region InitParameter
        /// <summary>
        /// Initialisation des paramètres
        /// </summary>
        private void InitParameter()
        {
            paramIdCEC = new DataParameter(_CS, "IDCEC", DbType.Int32);

            paramIdM = new DataParameter(_CS, "IDM", DbType.Int32);
            paramIdCE = new DataParameter(_CS, "IDCE", DbType.Int32);
            paramIdA_Entity = new DataParameter(_CS, "IDA_ENTITY", DbType.Int32);
            paramIdDC = new DataParameter(_CS, "IDDC", DbType.Int32);

            paramAssetCategory_UNL = new DataParameter(_CS, "ASSETCATEGORY_UNL", DbType.AnsiString, SQLCst.UT_ENUM_MANDATORY_LEN);
            paramEffectiveDate = new DataParameter(_CS, "EFFECTIVEDATE", DbType.Date);
            paramIdAsset_UNL = new DataParameter(_CS, "IDASSET_UNL", DbType.Int32);
            paramCategory = new DataParameter(_CS, "CATEGORY", DbType.AnsiString, SQLCst.UT_CFICODE_LEN);
            paramReadyState = new DataParameter(_CS, "READYSTATE", DbType.AnsiString, SQLCst.UT_ENUM_MANDATORY_LEN);
            paramAdjStatus = new DataParameter(_CS, "ADJSTATUS", DbType.AnsiString, SQLCst.UT_ENUM_MANDATORY_LEN);
            paramAdjMethod = new DataParameter(_CS, "ADJMETHOD", DbType.AnsiString, SQLCst.UT_ENUM_MANDATORY_LEN);
            paramRenamingMethod = new DataParameter(_CS, "RENAMINGMETHOD", DbType.AnsiString, SQLCst.UT_ENUM_MANDATORY_LEN);
            paramRenamingValue = new DataParameter(_CS, "RENAMINGVALUE", DbType.AnsiString, SQLCst.UT_LINKCODE_LEN);
            paramExRenamingValue = new DataParameter(_CS, "EXRENAMINGVALUE", DbType.AnsiString, SQLCst.UT_LINKCODE_LEN);

            paramRFactor = new DataParameter(_CS, "RFACTOR", DbType.Decimal);
            paramRFactor_Rnd = new DataParameter(_CS, "RFACTOR_RND", DbType.Decimal);
            paramRFactor_Retained = new DataParameter(_CS, "RFACTOR_RETAINED", DbType.Decimal);

            paramContractSize = new DataParameter(_CS, "CSIZE", DbType.Decimal);
            paramContractMultiplier = new DataParameter(_CS, "CMULTIPLIER", DbType.Decimal);

            paramExContractSize = new DataParameter(_CS, "EXCSIZE", DbType.Decimal);
            paramExContractSize_Rnd = new DataParameter(_CS, "EXCSIZE_RND", DbType.Decimal);
            paramExContractMultiplier = new DataParameter(_CS, "EXCMULTIPLIER", DbType.Decimal);
            paramExContractMultiplier_Rnd = new DataParameter(_CS, "EXCMULTIPLIER_RND", DbType.Decimal);
        }
        #endregion

        #region SetParameters
        /// <summary>
        /// Valorisation des paramètres (INSERTION / MISE A JOUR)
        /// </summary>
        private void SetParameters(CorporateEventContract pCorporateEventContract, DataParameters pParameters)
        {
            pParameters["READYSTATE"].Value = pCorporateEventContract.readyState;
            pParameters["ADJSTATUS"].Value = pCorporateEventContract.adjStatus;
            pParameters["ADJMETHOD"].Value = pCorporateEventContract.adjMethod;
            pParameters["RENAMINGMETHOD"].Value = pCorporateEventContract.renamingMethod;

            SetSimpleUnitParameters("RFACTOR", pCorporateEventContract.rFactorSpecified, pCorporateEventContract.rFactor, pParameters);
            pParameters["RFACTOR_RETAINED"].Value = pCorporateEventContract.rFactorRetained;

            CalculationCumData cumData = pCorporateEventContract.cumData;
            pParameters["RENAMINGVALUE"].Value = (cumData.renamingValueSpecified ? cumData.renamingValue : Convert.DBNull);
            pParameters["CSIZE"].Value = (cumData.contractSizeSpecified ? cumData.contractSize.DecValue : Convert.DBNull);
            pParameters["CMULTIPLIER"].Value = (cumData.contractMultiplierSpecified ? cumData.contractMultiplier.DecValue : Convert.DBNull);
            CalculationExData exData = pCorporateEventContract.exData;
            if (null == exData)
                exData = new CalculationExData();
            pParameters["EXRENAMINGVALUE"].Value = (exData.renamingValueSpecified ? exData.renamingValue : Convert.DBNull);
            SetSimpleUnitParameters("EXCSIZE", exData.contractSizeSpecified, exData.contractSize, pParameters);
            SetSimpleUnitParameters("EXCMULTIPLIER", exData.contractMultiplierSpecified, exData.contractMultiplier, pParameters);
        }
        #endregion SetParameters
        #region SetParametersDelete
        /// <summary>
        /// Valorisation des paramètres (MISE A JOUR)
        /// </summary>
        public void SetParametersDelete(CorporateEventContract pCorporateEventContract, DataParameters pParameters)
        {
            pParameters["IDCE"].Value = pCorporateEventContract.idCE;
            pParameters["IDA_ENTITY"].Value = pCorporateEventContract.idA_Entity;
            pParameters["IDDC"].Value = pCorporateEventContract.idDC;
        }
        #endregion SetParametersDelete
        #region SetParametersInsert
        /// <summary>
        /// Valorisation des paramètres (INSERTION)
        /// </summary>
        public void SetParametersInsert(CorporateEventContract pCorporateEventContract, DataParameters pParameters)
        {
            pParameters["IDCEC"].Value = pCorporateEventContract.IdCEC;
            pParameters["IDCE"].Value = pCorporateEventContract.idCE;
            pParameters["IDA_ENTITY"].Value = pCorporateEventContract.idA_Entity;
            pParameters["IDDC"].Value = pCorporateEventContract.idDC;
            pParameters["ASSETCATEGORY_UNL"].Value = pCorporateEventContract.assetCategory_UNL;
            pParameters["IDASSET_UNL"].Value = pCorporateEventContract.idAsset_UNL;
            pParameters["CATEGORY"].Value = ReflectionTools.ConvertEnumToString<CfiCodeCategoryEnum>(pCorporateEventContract.category);
            SetParameters(pCorporateEventContract, pParameters);
        }
        #endregion SetParametersInsert
        #region SetParametersUpdate
        /// <summary>
        /// Valorisation des paramètres (MISE A JOUR)
        /// </summary>
        public void SetParametersUpdate(CorporateEventContract pCorporateEventContract, DataParameters pParameters)
        {
            pParameters["IDCE"].Value = pCorporateEventContract.idCE;
            pParameters["IDA_ENTITY"].Value = pCorporateEventContract.idA_Entity;
            pParameters["IDDC"].Value = pCorporateEventContract.idDC;
            SetParameters(pCorporateEventContract, pParameters);
        }
        #endregion SetParametersUpdate
        #region SetSimpleUnitParameters
        private void SetSimpleUnitParameters(string pElement, bool pIsSpecified, SimpleUnit pSimpleUnit, DataParameters pParameters)
        {
            if (pIsSpecified)
            {
                pParameters[pElement].Value = pSimpleUnit.value.DecValue;
                pParameters[pElement + "_RND"].Value = (pSimpleUnit.valueRoundedSpecified ? pSimpleUnit.valueRounded.DecValue : pSimpleUnit.value.DecValue);
            }
            else
            {
                pParameters[pElement].Value = Convert.DBNull;
                pParameters[pElement + "_RND"].Value = Convert.DBNull;
            }
        }
        #endregion SetSimpleUnitParameters
        #endregion Methods
    }
    #endregion DCQueryEMBEDDED
    #region ETDQueryEMBEDDED
    /// <summary>
    /// Classe des Queries et paramètres des DC liés à une CA INTEGREES (insert, update, select, delete)
    /// </summary>
    public class ETDQueryEMBEDDED
    {
        #region Members
        private readonly string _CS;

        private DataParameter paramIdCEC;
        private DataParameter paramEffectiveDate; // Use only for GetQuerySelectCandidate

        private DataParameter paramIdCE;
        private DataParameter paramIdA_Entity;
        private DataParameter paramIdDA;
        private DataParameter paramIdDC;      
        private DataParameter paramIdAsset;

        private DataParameter paramCategory;
        private DataParameter paramReadyState;
        private DataParameter paramAdjStatus;
        private DataParameter paramAdjMethod;
        private DataParameter paramRFactor;
        private DataParameter paramRFactor_Rnd;
        private DataParameter paramRFactor_Retained;


        private DataParameter paramContractSize;
        private DataParameter paramContractMultiplier;
        private DataParameter paramStrikePrice;
        private DataParameter paramDailyClosingPrice;

        private DataParameter paramExContractSize;
        private DataParameter paramExContractSize_Rnd;
        private DataParameter paramExContractMultiplier;
        private DataParameter paramExContractMultiplier_Rnd;
        private DataParameter paramExStrikePrice;
        private DataParameter paramExStrikePrice_Rnd;
        private DataParameter paramExDailyClosingPrice;
        private DataParameter paramExDailyClosingPrice_Rnd;

        private DataParameter paramEqualisationPayment;
        private DataParameter paramEqualisationPayment_Rnd;

        #endregion Members
        #region Constructor
        public ETDQueryEMBEDDED(string pCS)
        {
            _CS = pCS;
            InitParameter();
        }
        #endregion
        #region Methods
        #region AddParameters
        /// <summary>
        /// Paramètres communs à l'INSERTION / MISE A JOUR
        /// </summary>
        /// <param name="pParameters">Paramèetres</param>
        private void AddParameters(DataParameters pParameters)
        {
            pParameters.Add(paramIdCE);
            pParameters.Add(paramIdA_Entity);
            pParameters.Add(paramIdAsset);

            pParameters.Add(paramReadyState);
            pParameters.Add(paramAdjStatus);
            pParameters.Add(paramAdjMethod);
            pParameters.Add(paramRFactor);
            pParameters.Add(paramRFactor_Rnd);
            pParameters.Add(paramRFactor_Retained);

            pParameters.Add(paramContractSize);
            pParameters.Add(paramContractMultiplier);
            pParameters.Add(paramStrikePrice);
            pParameters.Add(paramDailyClosingPrice);

            pParameters.Add(paramExContractSize);
            pParameters.Add(paramExContractSize_Rnd);
            pParameters.Add(paramExContractMultiplier);
            pParameters.Add(paramExContractMultiplier_Rnd);
            pParameters.Add(paramExStrikePrice);
            pParameters.Add(paramExStrikePrice_Rnd);
            pParameters.Add(paramExDailyClosingPrice);
            pParameters.Add(paramExDailyClosingPrice_Rnd);

            pParameters.Add(paramEqualisationPayment);
            pParameters.Add(paramEqualisationPayment_Rnd);
        }
        #endregion AddParameters
        #region AddParamterInsert
        /// <summary>
        /// Paramètres pour INSERTION
        /// </summary>
        /// <param name="pParameters">Paramètres</param>
        public void AddParametersInsert(DataParameters pParameters)
        {
            pParameters.Add(paramIdCEC);
            pParameters.Add(paramIdDA);
            pParameters.Add(paramIdDC);
            pParameters.Add(paramCategory);
            AddParameters(pParameters);
        }
        #endregion AddParamterInsert
        #region AddParametersUpdate
        /// <summary>
        /// Paramètres pour MISE A JOUR
        /// </summary>
        /// <param name="pParameters">Paramètres</param>
        public void AddParametersUpdate(DataParameters pParameters)
        {
            AddParameters(pParameters);
        }
        #endregion AddParametersUpdate

        #region GetQueryDelete
        /// <summary>
        /// Requête de suppression d'une CA PUBLIEE
        /// </summary>
        /// <returns></returns>
        public QueryParameters GetQueryDelete()
        {
            DataParameters parameters = new DataParameters();
            parameters.Add(paramIdCE);
            parameters.Add(paramIdA_Entity);
            parameters.Add(paramIdAsset);
            StrBuilder sqlQuery = new StrBuilder();
            sqlQuery += SQLCst.DELETE_DBO + Cst.OTCml_TBL.CORPOEVENTASSET.ToString() + Cst.CrLf;
            sqlQuery += SQLCst.WHERE + "(IDCE=@IDCE) and (IDA_ENTITY=@IDA_ENTITY) and (IDASSET=@IDASSET)" + Cst.CrLf;
            QueryParameters ret = new QueryParameters(_CS, sqlQuery.ToString(), parameters);
            return ret;
        }
        #endregion GetQueryDelete
        #region GetQueryExist
        /// <summary>
        /// Requête de contrôle d'existence d'actifs rattachés à une CA (CE)
        /// </summary>
        /// <returns></returns>
        public QueryParameters GetQueryExist()
        {
            DataParameters parameters = new DataParameters();
            parameters.Add(paramIdCE);
            StrBuilder sqlQuery = new StrBuilder();
            sqlQuery += SQLCst.SELECT + "1" + Cst.CrLf;
            sqlQuery += SQLCst.FROM_DBO + Cst.OTCml_TBL.CORPOEVENTASSET.ToString() + " cea" + Cst.CrLf;
            sqlQuery += SQLCst.WHERE + "(cea.IDCE = @IDCE)" + Cst.CrLf;
            QueryParameters ret = new QueryParameters(_CS, sqlQuery.ToString(), parameters);
            return ret;
        }
        #endregion GetQueryExist

        #region GetQuerySelectCandidate
        /// <summary>
        /// Requête de sélection des ASSETs candidates à traitement de CA 
        /// ● pour un DERIVATIVE CONTRACT donné
        /// </summary>
        /// <returns></returns>
        // EG 20240722 [XXXXX] Add Restriction on DTENABLED for ASSET CANDIDATES
        public QueryParameters GetQuerySelectCandidate()
        {
            DataParameters parameters = new DataParameters();
            parameters.Add(paramIdCE);
            parameters.Add(paramIdDA);
            parameters.Add(paramIdA_Entity);
            parameters.Add(paramEffectiveDate);

            string sqlQryAsset = @"select etd.IDASSET
            from dbo.VW_ASSET_ETD_EXPANDED etd 
            where (etd.IDDERIVATIVEATTRIB = @IDDA) and (isnull(etd.MATURITYDATE,@EFFECTIVEDATE) >= @EFFECTIVEDATE)" + Cst.CrLf;

            string sqlQryCorpo = @"select cea.IDASSET
            from dbo.CORPOEVENTASSET cea
            where (cea.IDCE = @IDCE) and (cea.IDA_ENTITY = @IDA_ENTITY) and (cea.IDDA = @IDDA)" + Cst.CrLf;

            // EG 20160105 [34091] Remove 'As' to SubQuery (NewAsset|DelAsset|UpdAsset)
            // RD 20180614 [24025] Si CONTRACTMULTIPLIER est NULL, prendre FACTOR
            string sqlQuery = @"select etd.IDASSET, etd.IDENTIFIER, isnull(etd.ORIGINALCONTRACTMULTIPLIER,etd.FACTOR) as CONTRACTMULTIPLIER, 
            etd.FACTOR as CONTRACTSIZE, etd.STRIKEPRICE, etd.CATEGORY, main.ROWSTATE 
            from dbo.VW_ASSET_ETD_EXPANDED etd
            inner join
            (
                select distinct IDASSET, 'Added' as ROWSTATE from (" + sqlQryAsset + SQLCst.EXCEPT + Cst.CrLf + sqlQryCorpo + @") NewAsset
                union
                select distinct IDASSET, 'Deleted' as ROWSTATE from (" + sqlQryCorpo + SQLCst.EXCEPT + Cst.CrLf + sqlQryAsset + @") DelAsset
                union
                select distinct IDASSET, 'Modified' as ROWSTATE from (" + sqlQryAsset + SQLCst.INTERSECT + Cst.CrLf + sqlQryCorpo + @") UpdAsset
            ) main on(main.IDASSET = etd.IDASSET)
            where (etd.DTENABLED < @EFFECTIVEDATE) and(isnull(etd.DTDISABLED, @EFFECTIVEDATE) >= @EFFECTIVEDATE)" + Cst.CrLf;
            QueryParameters ret = new QueryParameters(_CS, sqlQuery, parameters);
            return ret;
        }
        #endregion GetQuerySelectCandidate

        #region GetQueryInsert
        /// <summary>
        /// Requête d'insertion d'un CA INTEGREE
        /// </summary>
        /// <returns></returns>
        public QueryParameters GetQueryInsert()
        {
            DataParameters parameters = new DataParameters();
            AddParametersInsert(parameters);
            StrBuilder sqlQuery = new StrBuilder();
            sqlQuery += SQLCst.INSERT_INTO_DBO + Cst.OTCml_TBL.CORPOEVENTASSET.ToString() + Cst.CrLf;
            sqlQuery += "(IDCEC, IDCE, IDA_ENTITY, IDDC, IDDA, CATEGORY, IDASSET, READYSTATE, ADJSTATUS, ADJMETHOD, " + Cst.CrLf;
            sqlQuery += "RFACTOR, RFACTOR_RND, RFACTOR_RETAINED, CMULTIPLIER, CSIZE, STRIKEPRICE, DCLOSINGPRICE, " + Cst.CrLf;
            sqlQuery += "EXCMULTIPLIER, EXCMULTIPLIER_RND, EXCSIZE, EXCSIZE_RND, " + Cst.CrLf;
            sqlQuery += "EXSTRIKEPRICE, EXSTRIKEPRICE_RND, EXDCLOSINGPRICE, EXDCLOSINGPRICE_RND, EQUALPAYMENT, EQUALPAYMENT_RND)" + Cst.CrLf;
            sqlQuery += "values" + Cst.CrLf;
            sqlQuery += "(@IDCEC, @IDCE, @IDA_ENTITY, @IDDC, @IDDA, @CATEGORY, @IDASSET, @READYSTATE, @ADJSTATUS, @ADJMETHOD, " + Cst.CrLf;
            sqlQuery += "@RFACTOR, @RFACTOR_RND, @RFACTOR_RETAINED, @CMULTIPLIER, @CSIZE, @STRIKEPRICE, @DCLOSINGPRICE, " + Cst.CrLf;
            sqlQuery += "@EXCMULTIPLIER, @EXCMULTIPLIER_RND, @EXCSIZE, @EXCSIZE_RND, " + Cst.CrLf;
            sqlQuery += "@EXSTRIKEPRICE, @EXSTRIKEPRICE_RND, @EXDCLOSINGPRICE, @EXDCLOSINGPRICE_RND, @EQUALPAYMENT, @EQUALPAYMENT_RND)" + Cst.CrLf;
            QueryParameters ret = new QueryParameters(_CS, sqlQuery.ToString(), parameters);
            return ret;
        }
        #endregion GetQueryInsert
        #region GetQueryUpdate
        /// <summary>
        /// Requête de MISE A JOUR d'un CA INTEGREE
        /// </summary>
        /// <returns></returns>
        public QueryParameters GetQueryUpdate()
        {
            DataParameters parameters = new DataParameters();
            AddParametersUpdate(parameters);
            StrBuilder sqlQuery = new StrBuilder();
            sqlQuery += SQLCst.UPDATE_DBO + Cst.OTCml_TBL.CORPOEVENTASSET.ToString() + Cst.CrLf;
            sqlQuery += SQLCst.SET + "READYSTATE=@READYSTATE, ADJSTATUS=@ADJSTATUS, ADJMETHOD=@ADJMETHOD, " + Cst.CrLf;
            sqlQuery += "RFACTOR=@RFACTOR, RFACTOR_RND=@RFACTOR_RND, RFACTOR_RETAINED=@RFACTOR_RETAINED, CMULTIPLIER=@CMULTIPLIER, CSIZE=@CSIZE, " + Cst.CrLf;
            sqlQuery += "STRIKEPRICE=@STRIKEPRICE,  DCLOSINGPRICE=@DCLOSINGPRICE, " + Cst.CrLf;
            sqlQuery += "EXCMULTIPLIER=@EXCMULTIPLIER, EXCMULTIPLIER_RND=@EXCMULTIPLIER_RND, EXCSIZE=@EXCSIZE, EXCSIZE_RND=@EXCSIZE_RND, "  + Cst.CrLf;
            sqlQuery += "EXSTRIKEPRICE=@EXSTRIKEPRICE,  EXSTRIKEPRICE_RND=@EXSTRIKEPRICE_RND, " + Cst.CrLf;
            sqlQuery += "EXDCLOSINGPRICE=@EXDCLOSINGPRICE, EXDCLOSINGPRICE_RND=@EXDCLOSINGPRICE_RND, EQUALPAYMENT=@EQUALPAYMENT, EQUALPAYMENT_RND=@EQUALPAYMENT_RND" + Cst.CrLf;
            sqlQuery += SQLCst.WHERE + "(IDCE=@IDCE) and (IDA_ENTITY=@IDA_ENTITY) and (IDASSET=@IDASSET)" + Cst.CrLf;
            QueryParameters ret = new QueryParameters(_CS, sqlQuery.ToString(), parameters);
            return ret;
        }
        #endregion GetQueryUpdate

        #region InitParameter
        /// <summary>
        /// Initialisation des paramètres
        /// </summary>
        private void InitParameter()
        {
            paramIdCEC = new DataParameter(_CS, "IDCEC", DbType.Int32);

            paramIdA_Entity = new DataParameter(_CS, "IDA_ENTITY", DbType.Int32);
            paramIdCE = new DataParameter(_CS, "IDCE", DbType.Int32);
            paramIdDA = new DataParameter(_CS, "IDDA", DbType.Int32);
            paramIdDC = new DataParameter(_CS, "IDDC", DbType.Int32);
            paramEffectiveDate = new DataParameter(_CS, "EFFECTIVEDATE", DbType.Date);


            paramIdAsset = new DataParameter(_CS, "IDASSET", DbType.Int32);
            paramCategory = new DataParameter(_CS, "CATEGORY", DbType.AnsiString, SQLCst.UT_CFICODE_LEN);
            paramReadyState = new DataParameter(_CS, "READYSTATE", DbType.AnsiString, SQLCst.UT_ENUM_MANDATORY_LEN);
            paramAdjStatus = new DataParameter(_CS, "ADJSTATUS", DbType.AnsiString, SQLCst.UT_ENUM_MANDATORY_LEN);
            paramAdjMethod = new DataParameter(_CS, "ADJMETHOD", DbType.AnsiString, SQLCst.UT_ENUM_MANDATORY_LEN);

            paramRFactor = new DataParameter(_CS, "RFACTOR", DbType.Decimal);
            paramRFactor_Rnd = new DataParameter(_CS, "RFACTOR_RND", DbType.Decimal);
            paramRFactor_Retained = new DataParameter(_CS, "RFACTOR_RETAINED", DbType.Decimal);

            paramContractSize = new DataParameter(_CS, "CSIZE", DbType.Decimal);
            paramContractMultiplier = new DataParameter(_CS, "CMULTIPLIER", DbType.Decimal);
            paramStrikePrice = new DataParameter(_CS, "STRIKEPRICE", DbType.Decimal);
            paramDailyClosingPrice = new DataParameter(_CS, "DCLOSINGPRICE", DbType.Decimal);

            paramExContractSize = new DataParameter(_CS, "EXCSIZE", DbType.Decimal);
            paramExContractSize_Rnd = new DataParameter(_CS, "EXCSIZE_RND", DbType.Decimal);
            paramExContractMultiplier = new DataParameter(_CS, "EXCMULTIPLIER", DbType.Decimal);
            paramExContractMultiplier_Rnd = new DataParameter(_CS, "EXCMULTIPLIER_RND", DbType.Decimal);
            paramExStrikePrice = new DataParameter(_CS, "EXSTRIKEPRICE", DbType.Decimal);
            paramExStrikePrice_Rnd = new DataParameter(_CS, "EXSTRIKEPRICE_RND", DbType.Decimal);
            paramExDailyClosingPrice = new DataParameter(_CS, "EXDCLOSINGPRICE", DbType.Decimal);
            paramExDailyClosingPrice_Rnd = new DataParameter(_CS, "EXDCLOSINGPRICE_RND", DbType.Decimal);

            paramEqualisationPayment = new DataParameter(_CS, "EQUALPAYMENT", DbType.Decimal);
            paramEqualisationPayment_Rnd = new DataParameter(_CS, "EQUALPAYMENT_RND", DbType.Decimal);
        }
        #endregion InitParameter

        #region SetParameters
        /// <summary>
        /// Valorisation des paramètres (INSERTION / MISE A JOUR)
        /// </summary>
        private void SetParameters(CorporateEventAsset pCorporateEventAsset, DataParameters pParameters)
        {
            pParameters["READYSTATE"].Value = pCorporateEventAsset.readyState;
            pParameters["ADJSTATUS"].Value = pCorporateEventAsset.adjStatus;
            pParameters["ADJMETHOD"].Value = pCorporateEventAsset.adjMethod;

            SetSimpleUnitParameters("RFACTOR", pCorporateEventAsset.rFactorSpecified, pCorporateEventAsset.rFactor, pParameters);
            pParameters["RFACTOR_RETAINED"].Value = pCorporateEventAsset.rFactorRetained;

            CalculationCumData cumData = pCorporateEventAsset.cumData;
            pParameters["CSIZE"].Value = (cumData.contractSizeSpecified ? cumData.contractSize.DecValue : Convert.DBNull);
            pParameters["CMULTIPLIER"].Value = (cumData.contractMultiplierSpecified ? cumData.contractMultiplier.DecValue : Convert.DBNull);
            pParameters["STRIKEPRICE"].Value = (cumData.strikePriceSpecified ? cumData.strikePrice.DecValue : Convert.DBNull);
            pParameters["DCLOSINGPRICE"].Value = (cumData.dailyClosingPriceSpecified ? cumData.dailyClosingPrice.DecValue : Convert.DBNull);

            CalculationExData exData = pCorporateEventAsset.exData;
            if (null == exData)
                exData = new CalculationExData();
            SetSimpleUnitParameters("EXCSIZE", exData.contractSizeSpecified, exData.contractSize, pParameters);
            SetSimpleUnitParameters("EXCMULTIPLIER", exData.contractMultiplierSpecified, exData.contractMultiplier, pParameters);
            SetSimpleUnitParameters("EXSTRIKEPRICE", exData.strikePriceSpecified, exData.strikePrice, pParameters);
            SetSimpleUnitParameters("EXDCLOSINGPRICE", exData.dailyClosingPriceSpecified, exData.dailyClosingPrice, pParameters);
            SetSimpleUnitParameters("EQUALPAYMENT", exData.equalizationPaymentSpecified, exData.equalizationPayment, pParameters);

        }
        #endregion SetParameters
        #region SetParametersDelete
        /// <summary>
        /// Valorisation des paramètres (DELETE)
        /// </summary>
        public void SetParametersDelete(CorporateEventAsset pCorporateEventAsset, DataParameters pParameters)
        {
            pParameters["IDCE"].Value = pCorporateEventAsset.idCE;
            pParameters["IDA_ENTITY"].Value = pCorporateEventAsset.idA_Entity;
            pParameters["IDASSET"].Value = pCorporateEventAsset.idASSET;
        }
        #endregion SetParametersDelete
        #region SetParametersInsert
        /// <summary>
        /// Valorisation des paramètres (INSERTION)
        /// </summary>
        public void SetParametersInsert(CorporateEventAsset pCorporateEventAsset, DataParameters pParameters)
        {
            pParameters["IDCEC"].Value = pCorporateEventAsset.idCEC;
            pParameters["IDCE"].Value = pCorporateEventAsset.idCE;
            pParameters["IDA_ENTITY"].Value = pCorporateEventAsset.idA_Entity;
            pParameters["IDDC"].Value = pCorporateEventAsset.idDC;
            pParameters["IDDA"].Value = pCorporateEventAsset.idDA;
            pParameters["IDASSET"].Value = pCorporateEventAsset.idASSET;
            pParameters["CATEGORY"].Value = ReflectionTools.ConvertEnumToString<CfiCodeCategoryEnum>(pCorporateEventAsset.category);
            SetParameters(pCorporateEventAsset, pParameters); 
        }
        #endregion SetParametersInsert
        #region SetParametersUpdate
        /// <summary>
        /// Valorisation des paramètres (MISE A JOUR)
        /// </summary>
        public void SetParametersUpdate(CorporateEventAsset pCorporateEventAsset, DataParameters pParameters)
        {
            pParameters["IDCE"].Value = pCorporateEventAsset.idCE;
            pParameters["IDA_ENTITY"].Value = pCorporateEventAsset.idA_Entity;
            pParameters["IDASSET"].Value = pCorporateEventAsset.idASSET;
            SetParameters(pCorporateEventAsset, pParameters); 
        }
        #endregion SetParametersUpdate
        #region SetSimpleUnitParameters
        private void SetSimpleUnitParameters(string pElement, bool pIsSpecified, SimpleUnit pSimpleUnit, DataParameters pParameters)
        {
            if (pIsSpecified)
            {
                pParameters[pElement].Value = pSimpleUnit.value.DecValue;
                pParameters[pElement + "_RND"].Value = (pSimpleUnit.valueRoundedSpecified ? pSimpleUnit.valueRounded.DecValue : pSimpleUnit.value.DecValue);
            }
            else
            {
                pParameters[pElement].Value = Convert.DBNull;
                pParameters[pElement + "_RND"].Value = Convert.DBNull;
            }
        }
        #endregion SetSimpleUnitParameters
        #endregion Methods
    }
    #endregion ETDQueryEMBEDDED
    #region CAQueryISSUE
    /// <summary>
    /// Classe des Queries et paramètres des Corporate actions PUBLIEES (insert, update, select, delete)
    /// </summary>
    public class CAQueryISSUE : CAQueryBase
    {
        #region Members
        private DataParameter paramCAMarket;
        private DataParameter paramEmbeddedState;
        private DataParameter paramEffectiveDate;
        private DataParameter paramCEGroup;
        private DataParameter paramCEType;
        private DataParameter paramCECombinedOperand;
        private DataParameter paramCECombinedType;
        private DataParameter paramAdjMethod;
        private DataParameter paramBuildInfo;
        #endregion Members
        #region Constructor
        public CAQueryISSUE(string pCS):base(pCS)
        {
            InitParameter();
        }
        #endregion
        #region Methods
        #region AddParameters
        /// <summary>
        /// Paramètres communs à l'INSERTION / MISE A JOUR
        /// </summary>
        /// <param name="pParameters">Paramètres</param>
        private void AddParameters(DataParameters pParameters)
        {
            pParameters.Add(paramCAMarket);
            pParameters.Add(paramEffectiveDate);
            pParameters.Add(paramEmbeddedState);
            pParameters.Add(paramCEGroup);
            pParameters.Add(paramCEType);
            pParameters.Add(paramCECombinedOperand);
            pParameters.Add(paramCECombinedType);
            pParameters.Add(paramAdjMethod);
            pParameters.Add(paramBuildInfo);
        }
        #endregion AddParameters
        #region AddParamterInsert
        /// <summary>
        /// Paramètres pour INSERTION
        /// </summary>
        /// <param name="pParameters">Paramètres</param>
        public override void AddParametersInsert(DataParameters pParameters)
        {
            base.AddParametersInsert(pParameters);
            AddParameters(pParameters);
        }
        #endregion AddParametersInsert
        #region AddParametersUpdate
        /// <summary>
        /// Paramètres pour MISE A JOUR
        /// </summary>
        /// <param name="pParameters">Paramètres</param>
        public override void AddParametersUpdate(DataParameters pParameters)
        {
            base.AddParametersUpdate(pParameters);
            AddParameters(pParameters);
        }
        #endregion AddParametersUpdate

        #region GetQueryExist
        /// <summary>
        /// Requête de contrôle existence d'une CA PUBLIEE
        /// Recherche par :
        /// ● son ID
        /// ● sa référence de notice (MARCHE + REFNOTICE principale)
        /// </summary>
        /// <param name="pCAWhereMode">Mode (Critère) de recherche</param>
        /// <returns></returns>
        /// FI 20130503[] la méthode retourne IDCAISSUE
        public override QueryParameters GetQueryExist(CATools.CAWhereMode pCAWhereMode)
        {
            DataParameters parameters = new DataParameters();
            StrBuilder sqlQuery = new StrBuilder();
            sqlQuery += SQLCst.SELECT + "ca.IDCAISSUE" + Cst.CrLf;
            sqlQuery += SQLCst.FROM_DBO + Cst.OTCml_TBL.CORPOACTIONISSUE.ToString() + " ca" + Cst.CrLf;

            switch (pCAWhereMode)
            {
                case CATools.CAWhereMode.ID:
                    parameters.Add(paramID);
                    sqlQuery += SQLCst.WHERE + "(ca.IDCAISSUE = @ID)" + Cst.CrLf;
                    break;
                case CATools.CAWhereMode.NOTICE:
                    parameters.Add(paramCAMarket);
                    parameters.Add(paramRefNotice);
                    parameters.Add(paramCfiCode);
                    sqlQuery += SQLCst.WHERE + "(ca.CAMARKET = @CAMARKET) and (ca.REFNOTICE = @REFNOTICE)" + Cst.CrLf;
                    sqlQuery += SQLCst.AND + "((ca.CFICODE is null) or (ca.CFICODE = @CFICODE))";
                    break;
            }
            QueryParameters ret = new QueryParameters(_CS, sqlQuery.ToString(), parameters);
            return ret;
        }
        #endregion GetQueryExist
        #region GetQueryDelete
        /// <summary>
        /// Requête de suppression d'une CA PUBLIEE
        /// </summary>
        /// <returns></returns>
        public override QueryParameters GetQueryDelete()
        {
            DataParameters parameters = new DataParameters();
            parameters.Add(paramID);
            StrBuilder sqlQuery = new StrBuilder();
            sqlQuery += SQLCst.DELETE_DBO + Cst.OTCml_TBL.CORPOACTIONISSUE.ToString() + Cst.CrLf;
            sqlQuery += SQLCst.WHERE + "(IDCAISSUE = @ID)" + Cst.CrLf;
            QueryParameters ret = new QueryParameters(_CS, sqlQuery.ToString(), parameters);
            return ret;
        }
        #endregion GetQueryDelete
        #region GetQuerySelect
        /// <summary>
        /// Requête de sélection d'une CA PUBLIEE
        /// Recherche par :
        /// ● son ID
        /// ● sa référence de notice (MARCHE (ISO) + REFNOTICE (Principale ou additionnelles))
        /// Chargement de CORPOACTIONISSUE
        /// </summary>
        /// <param name="pCAWhereMode">Mode (Critère) de recherche</param>
        /// <returns></returns>
        /// EG 20211020 [XXXXX] Nouvelle gestion des notices (USEURLNOTICE et URLNOTICE)
        public override QueryParameters GetQuerySelect(CATools.CAWhereMode pCAWhereMode)
        {
            DataParameters parameters = new DataParameters();
            StrBuilder sqlQuery = new StrBuilder();
            string sqlWhere = string.Empty;

            switch (pCAWhereMode)
            {
                case CATools.CAWhereMode.ID:
                    parameters.Add(paramID);
                    sqlWhere += SQLCst.WHERE + "(ca.IDCAISSUE = @ID)" + Cst.CrLf;
                    break;
                case CATools.CAWhereMode.NOTICE:
                    parameters.Add(paramCAMarket);
                    parameters.Add(paramRefNotice);
                    parameters.Add(paramCfiCode);
                    sqlWhere += SQLCst.WHERE + "(ca.CAMARKET = @CAMARKET) and (ca.REFNOTICE = @REFNOTICE) and ((ca.CFICODE is null) or (ca.CFICODE = @CFICODE))" + Cst.CrLf;
                    break;
            }

            sqlQuery += SQLCst.SELECT + @"ca.IDCAISSUE as ID, mk.IDM, ca.CAMARKET, ca.CEGROUP, ca.CETYPE, ca.CECOMBINEDOPER, ca.CECOMBINEDTYPE, ca.IDENTIFIER,
            ca.READYSTATE, ca.CFICODE, ca.EMBEDDEDSTATE, ca.REFNOTICE, ca.URLNOTICE, ca.USEURLNOTICE, ca.NOTICEFILENAME, ca.PUBDATE, ca.EFFECTIVEDATE, ca.ADJMETHOD, ca.CFICODE, ca.BUILDINFO,
            ca.DTINS, ca.IDAINS, ca.DTUPD, ca.IDAUPD, ca.EXTLLINK, ca.ROWATTRIBUT
            from dbo.CORPOACTIONISSUE ca
            inner join dbo.VW_MARKET_IDENTIFIER mk on (mk.FIXML_SECURITYEXCHANGE = ca.CAMARKET)" + Cst.CrLf;
            sqlQuery += sqlWhere + Cst.CrLf;

            QueryParameters ret = new QueryParameters(_CS, sqlQuery.ToString(), parameters);
            return ret;
        }
        #endregion GetQuerySelect
        #region GetQueryInsert
        /// <summary>
        /// Requête d'insertion d'un CA PUBLIEE
        /// </summary>
        /// <returns></returns>
        /// EG 20211020 [XXXXX] Nouvelle gestion des notices (USEURLNOTICE et URLNOTICE)
        public override QueryParameters GetQueryInsert()
        {
            DataParameters parameters = new DataParameters();
            AddParametersInsert(parameters);
            StrBuilder sqlQuery = new StrBuilder();
            sqlQuery += SQLCst.INSERT_INTO_DBO + Cst.OTCml_TBL.CORPOACTIONISSUE.ToString() + Cst.CrLf;
            sqlQuery += @"(IDCAISSUE, CAMARKET, CEGROUP, CETYPE, CECOMBINEDOPER, CECOMBINEDTYPE, IDENTIFIER, READYSTATE, EMBEDDEDSTATE, REFNOTICE, NOTICEFILENAME, URLNOTICE, USEURLNOTICE," + Cst.CrLf;
            sqlQuery += @"PUBDATE, EFFECTIVEDATE, ADJMETHOD, CFICODE, BUILDINFO, DTINS, IDAINS, EXTLLINK, ROWATTRIBUT)" + Cst.CrLf;
            sqlQuery += @"values" + Cst.CrLf;
            sqlQuery += @"(@ID, @CAMARKET, @CEGROUP, @CETYPE, @CECOMBINEDOPER, @CECOMBINEDTYPE, @IDENTIFIER, @READYSTATE, @EMBEDDEDSTATE, @REFNOTICE, @NOTICEFILENAME, @URLNOTICE, @USEURLNOTICE," + Cst.CrLf;
            sqlQuery += @"@PUBDATE, @EFFECTIVEDATE, @ADJMETHOD, @CFICODE, @BUILDINFO, @DTINS, @IDAINS, @EXTLLINK, @ROWATTRIBUT)" + Cst.CrLf;
            QueryParameters ret = new QueryParameters(_CS, sqlQuery.ToString(), parameters);
            return ret;
        }
        #endregion GetQueryInsert
        #region GetQueryUpdateStatus
        /// <summary>
        /// Requête de mise à jour du STATUS D'INTEGRATION d'un CA PUBLIEE dans la table des CA INTEGREE
        /// </summary>
        /// <returns></returns>
        public override QueryParameters GetQueryUpdateStatus()
        {
            DataParameters parameters = new DataParameters();
            AddParametersUpdateStatus(parameters);
            parameters.Add(paramEmbeddedState);
            StrBuilder sqlQuery = new StrBuilder();
            sqlQuery += SQLCst.UPDATE_DBO + Cst.OTCml_TBL.CORPOACTIONISSUE.ToString() + Cst.CrLf;
            sqlQuery += SQLCst.SET + "READYSTATE=@READYSTATE, EMBEDDEDSTATE=@EMBEDDEDSTATE, DTUPD=@DTUPD, IDAUPD=@IDAUPD" + Cst.CrLf;
            sqlQuery += SQLCst.WHERE + "(IDCAISSUE=@ID)";
            QueryParameters ret = new QueryParameters(_CS, sqlQuery.ToString(), parameters);
            return ret;
        }
        #endregion GetQueryUpdateStatus

        #region GetQueryUpdate
        /// <summary>
        /// Requête de MISE A JOUR d'un CA PUBLIEE
        /// </summary>
        /// <returns></returns>
        /// EG 20211020 [XXXXX] Nouvelle gestion des notices (USEURLNOTICE et URLNOTICE)
        public override QueryParameters GetQueryUpdate()
        {
            DataParameters parameters = new DataParameters();
            AddParametersUpdate(parameters);
            StrBuilder sqlQuery = new StrBuilder();
            sqlQuery += SQLCst.UPDATE_DBO + Cst.OTCml_TBL.CORPOACTIONISSUE.ToString() + Cst.CrLf;
            sqlQuery += SQLCst.SET + "CAMARKET=@CAMARKET, CEGROUP=@CEGROUP, CETYPE=@CETYPE, CECOMBINEDOPER=@CECOMBINEDOPER, CECOMBINEDTYPE=@CECOMBINEDTYPE," + Cst.CrLf;
            sqlQuery += "IDENTIFIER=@IDENTIFIER, READYSTATE=@READYSTATE, EMBEDDEDSTATE=@EMBEDDEDSTATE, REFNOTICE=@REFNOTICE, NOTICEFILENAME=@NOTICEFILENAME, URLNOTICE=@URLNOTICE, USEURLNOTICE=@USEURLNOTICE," + Cst.CrLf;
            sqlQuery += "PUBDATE=@PUBDATE, EFFECTIVEDATE=@EFFECTIVEDATE, ADJMETHOD=@ADJMETHOD, CFICODE=@CFICODE, BUILDINFO=@BUILDINFO," + Cst.CrLf;
            sqlQuery += "DTUPD=@DTUPD, IDAUPD = @IDAUPD" + Cst.CrLf;
            sqlQuery += SQLCst.WHERE + "(IDCAISSUE=@ID)";
            QueryParameters ret = new QueryParameters(_CS, sqlQuery.ToString(), parameters);
            return ret;
        }
        #endregion GetQueryUpdate

        #region InitParameter
        /// <summary>
        /// Initialisation des paramètres
        /// </summary>
        private void InitParameter()
        {
            paramCAMarket = new DataParameter(_CS, "CAMARKET", DbType.AnsiString, SQLCst.UT_ENUM_MANDATORY_LEN);
            paramEmbeddedState = new DataParameter(_CS, "EMBEDDEDSTATE", DbType.AnsiString, SQLCst.UT_ENUM_MANDATORY_LEN);
            paramCEGroup = new DataParameter(_CS, "CEGROUP", DbType.AnsiString, SQLCst.UT_ENUM_MANDATORY_LEN);
            paramCEType = new DataParameter(_CS, "CETYPE", DbType.AnsiString, SQLCst.UT_ENUM_MANDATORY_LEN);
            paramCECombinedOperand = new DataParameter(_CS, "CECOMBINEDOPER", DbType.AnsiString, SQLCst.UT_ENUM_OPTIONAL_LEN);
            paramCECombinedType = new DataParameter(_CS, "CECOMBINEDTYPE", DbType.AnsiString, SQLCst.UT_ENUM_OPTIONAL_LEN);
            paramEffectiveDate = new DataParameter(_CS, "EFFECTIVEDATE", DbType.Date);
            paramAdjMethod = new DataParameter(_CS, "ADJMETHOD", DbType.AnsiString, SQLCst.UT_ENUM_MANDATORY_LEN);
            paramBuildInfo = new DataParameter(_CS, "BUILDINFO", DbType.Xml);
        }
        #endregion

        #region SetParameters
        /// <summary>
        /// Valorisation des paramètres (INSERTION / MISE A JOUR)
        /// </summary>
        private void SetParameters(CorporateAction pCorporateAction, DataParameters pParameters)
        {
            pParameters["CAMARKET"].Value = pCorporateAction.market.FIXML_SecurityExchange;
            if (CorporateActionReadyStateEnum.EMBEDDED == pCorporateAction.readystate)
                pCorporateAction.readystate = CorporateActionReadyStateEnum.PUBLISHED;
            pParameters["READYSTATE"].Value = pCorporateAction.readystate.ToString();
            pParameters["EMBEDDEDSTATE"].Value = pCorporateAction.embeddedState.ToString();
            CorporateEvent corporateEvent = pCorporateAction.corporateEvent[0];
            pParameters["CEGROUP"].Value = corporateEvent.group.ToString();
            pParameters["CETYPE"].Value = corporateEvent.type.ToString();
            pParameters["CECOMBINEDOPER"].Value = (corporateEvent.operandSpecified ? corporateEvent.operand.ToString() : Convert.DBNull);
            pParameters["CECOMBINEDTYPE"].Value = (corporateEvent.combinedTypeSpecified ? corporateEvent.combinedType.ToString(): Convert.DBNull);
            pParameters["EFFECTIVEDATE"].Value = (corporateEvent.effectiveDateSpecified ? corporateEvent.effectiveDate : Convert.DBNull);
            pParameters["ADJMETHOD"].Value = corporateEvent.adjMethod.ToString();

            EFS_SerializeInfoBase serializeInfo =
                new EFS_SerializeInfoBase(pCorporateAction.normMsgFactoryMQueue.GetType(), pCorporateAction.normMsgFactoryMQueue);
            // FI 20230103 [26204] Encoding.Unicode (puisque accepté par Oracle et sqlServer)
            StringBuilder sb = CacheSerializer.Serialize(serializeInfo, new UnicodeEncoding());
            pParameters["BUILDINFO"].Value = sb.ToString();
        }
        #endregion SetParameters
        #region SetParametersInsert
        /// <summary>
        /// Valorisation des paramètres (INSERTION)
        /// </summary>
        public override void SetParametersInsert(CorporateAction pCorporateAction, DataParameters pParameters)
        {
            base.SetParametersInsert(pCorporateAction, pParameters);
            SetParameters(pCorporateAction, pParameters);
        }
        #endregion SetParametersInsert
        #region SetParametersUpdate
        /// <summary>
        /// Valorisation des paramètres (MISE A JOUR)
        /// </summary>
        public override void SetParametersUpdate(CorporateAction pCorporateAction, DataParameters pParameters)
        {
            base.SetParametersUpdate(pCorporateAction, pParameters);
            SetParameters(pCorporateAction, pParameters);
        }
        #endregion SetParametersUpdate
        #endregion Methods
    }
    #endregion CAQueryISSUE


    #region CAQueryDOCS
    /// <summary>
    /// Classe des Queries et paramètres des Corporate actions DOCS (insert, update, select, delete)
    /// </summary>
    /// EG 20140518 [19913] New 
    public class CAQueryDOCS 
    {
        #region Members
        private DataParameter paramID;
        private DataParameter paramDocName;
        private DataParameter paramDocType;
        private DataParameter paramRunTime;
        private DataParameter paramLoDoc;
        private DataParameter paramDtUpd;
        private DataParameter paramIdAUpd;

        private readonly string _CS;
        #endregion Members
        #region Constructor
        public CAQueryDOCS(string pCS)
        {
            _CS = pCS;
            InitParameter();
        }
        #endregion
        #region Methods
        #region AddParameters
        /// <summary>
        /// Paramètres communs à l'INSERTION / MISE A JOUR
        /// </summary>
        /// <param name="pParameters">Paramètres</param>
        private void AddParameters(DataParameters pParameters)
        {
            pParameters.Add(paramID);
            pParameters.Add(paramDocName);
            pParameters.Add(paramDocType);
            pParameters.Add(paramRunTime);
            pParameters.Add(paramLoDoc);
            pParameters.Add(paramDtUpd);
            pParameters.Add(paramIdAUpd);
        }
        #endregion AddParameters
        #region AddParamterInsert
        /// <summary>
        /// Paramètres pour INSERTION
        /// </summary>
        /// <param name="pParameters">Paramètres</param>
        public void AddParametersInsert(DataParameters pParameters)
        {
            AddParameters(pParameters);
        }
        #endregion AddParametersInsert
        #region AddParametersUpdate
        /// <summary>
        /// Paramètres pour MISE A JOUR
        /// </summary>
        /// <param name="pParameters">Paramètres</param>
        public void AddParametersUpdate(DataParameters pParameters)
        {
            AddParameters(pParameters);
        }
        #endregion AddParametersUpdate

        #region GetQueryExist
        // EG 20140506 [19913] New
        public QueryParameters GetQueryExist()
        {
            DataParameters parameters = new DataParameters();
            parameters.Add(paramID);
            StrBuilder sqlQuery = new StrBuilder();
            sqlQuery += SQLCst.SELECT + @"cd.IDCA from dbo.CORPODOCS cd where (cd.IDCA = @ID)" + Cst.CrLf;
            QueryParameters ret = new QueryParameters(_CS, sqlQuery.ToString(), parameters);
            return ret;
        }
        #endregion GetQueryExist
        #region GetQueryDelete
        // EG 20140506 [19913] New
        public QueryParameters GetQueryDelete()
        {
            DataParameters parameters = new DataParameters();
            parameters.Add(paramID);
            StrBuilder sqlQuery = new StrBuilder();
            sqlQuery += SQLCst.DELETE_DBO + @"CORPODOCS where (IDCA = @ID)" + Cst.CrLf;
            QueryParameters ret = new QueryParameters(_CS, sqlQuery.ToString(), parameters);
            return ret;
        }
        #endregion GetQueryDelete
        #region GetQuerySelect
        // EG 20140506 [19913] New
        public QueryParameters GetQuerySelect()
        {
            DataParameters parameters = new DataParameters();
            parameters.Add(paramID);
            StrBuilder sqlQuery = new StrBuilder();
            sqlQuery += SQLCst.SELECT + @"cd.IDCA, cd.DOCNAME, cd.DOCTYPE, cd.RUNTIME, cd.LODOC, cd.DTUPD, cd.IDAUPD from dbo.CORPODOCS cd where (cd.IDCA = @ID)" + Cst.CrLf;
            QueryParameters ret = new QueryParameters(_CS, sqlQuery.ToString(), parameters);
            return ret;
        }
        #endregion GetQuerySelect
        #region GetQueryInsert
        public QueryParameters GetQueryInsert()
        {
            DataParameters parameters = new DataParameters();
            AddParametersInsert(parameters);
            StrBuilder sqlQuery = new StrBuilder();
            sqlQuery += SQLCst.INSERT_INTO_DBO + @"CORPODOCS (IDCA, DOCNAME, DOCTYPE, RUNTIME, LODOC, DTUPD, IDAUPD)
            values (@ID, @DOCNAME, @DOCTYPE, @RUNTIME, @LODOC, @DTUPD, @IDAUPD)" + Cst.CrLf;
            QueryParameters ret = new QueryParameters(_CS, sqlQuery.ToString(), parameters);
            return ret;
        }
        #endregion GetQueryInsert
        #region GetQueryUpdate
        public QueryParameters GetQueryUpdate()
        {
            DataParameters parameters = new DataParameters();
            AddParametersUpdate(parameters);

            StrBuilder sqlQuery = new StrBuilder();
            sqlQuery += SQLCst.UPDATE_DBO + @"CORPODOCS SET DOCNAME=@DOCNAME, DOCTYPE=@DOCTYPE, RUNTIME=@RUNTIME, LODOC=@LODOC, DTUPD=@DTUPD, IDAUPD=@IDAUPD
            where (IDCA=@ID)" + Cst.CrLf;
            QueryParameters ret = new QueryParameters(_CS, sqlQuery.ToString(), parameters);
            return ret;
        }
        #endregion GetQueryUpdate

        #region InitParameter
        /// <summary>
        /// Initialisation des paramètres
        /// </summary>
        private void InitParameter()
        {
            paramID = new DataParameter(_CS, "ID", DbType.Int32);
            paramDocName = new DataParameter(_CS, "DOCNAME", DbType.AnsiString, SQLCst.UT_UNC_LEN);
            paramDocType = new DataParameter(_CS, "DOCTYPE", DbType.AnsiString, SQLCst.UT_ENUM_MANDATORY_LEN);
            paramRunTime = new DataParameter(_CS, "RUNTIME", DbType.AnsiString, SQLCst.UT_ENUM_OPTIONAL_LEN);
            paramLoDoc = new DataParameter(_CS, "LODOC", DbType.Binary);
            paramDtUpd = DataParameter.GetParameter(_CS, DataParameter.ParameterEnum.DTUPD);
            paramIdAUpd = DataParameter.GetParameter(_CS,DataParameter.ParameterEnum.IDAUPD);
        }
        #endregion

        #region SetParameters
        /// <summary>
        /// Valorisation des paramètres (INSERTION / MISE A JOUR)
        /// </summary>
        public void SetParameters(int pIdCA, CorporateDoc pCorporateDoc, DataParameters pParameters)
        {
            pParameters["ID"].Value = pIdCA;
            pParameters["DOCNAME"].Value = pCorporateDoc.identifier;
            pParameters["DOCTYPE"].Value = pCorporateDoc.docType;
            pParameters["RUNTIME"].Value = pCorporateDoc.runTime;
            pParameters["LODOC"].Value = pCorporateDoc.script;
            pParameters["IDAUPD"].Value = 1;
            // FI 20200820 [XXXXXX] Dates systemes en UTC
            pParameters["DTUPD"].Value = OTCmlHelper.GetDateSysUTC(_CS);
        }
        #endregion SetParameters
        #endregion Methods
    }
    #endregion CAQueryDOCS

    #region CAInfo
    [Serializable]
    public class CAInfo
    {
        #region Members
        private Nullable<int> m_IdM;
        private Cst.Capture.ModeEnum m_Mode;
        private Nullable<int> m_Id;
        private Cst.OTCml_TBL m_Table;
        private int m_IdA;
        #endregion Members
        #region Accessors
        #region Mode
        public Cst.Capture.ModeEnum Mode
        {
            set { m_Mode = value; }
            get { return m_Mode; }
        }
        #endregion Mode
        #region CATable
        public Cst.OTCml_TBL CATable
        {
            get { return m_Table; }
            set { m_Table = value; }
        }
        #endregion CATable
        #region IdName
        public string IdName
        {
            get
            {
                string idName = "IDCA";
                if (m_Table == Cst.OTCml_TBL.CORPOACTIONISSUE)
                    idName += "ISSUE";
                return idName;
            }
        }
        #endregion IdName
        #region IdA
        public int IdA
        {
            get { return m_IdA; }
            set { m_IdA = value; }
        }
        #endregion IdA
        #region IdM
        public Nullable<int> IdM
        {
            get { return m_IdM; }
            set { m_IdM = value; }
        }
        #endregion IdM
        #region Id
        public Nullable<int> Id
        {
            get { return m_Id; }
            set { m_Id = value; }
        }
        #endregion IdSource
        #endregion Accessors
        #region Constructors
        public CAInfo(int pIdA)
        {
            m_IdA = pIdA;
            m_IdM = null;
            m_Mode = Cst.Capture.ModeEnum.New;
            m_Id = null;
            m_Table = Cst.OTCml_TBL.CORPOACTIONISSUE;
        }
        public CAInfo(int pIdA, string pPK, string pPKV)
            : this(pIdA)
        {
            if (StrFunc.IsFilled(pPK) && (pPK == "IDCA"))
            {
                m_Table = Cst.OTCml_TBL.CORPOACTION;
            }

            if (StrFunc.IsFilled(pPKV))
            {
                m_Mode = Cst.Capture.ModeEnum.Update;
                m_Id = Convert.ToInt32(pPKV);
            }
        }
        #endregion Constructors
    }
    #endregion CAInfo

    /* -------------------------------------------------------- */
    /* ----- CLASSES GENERALES CORPORATE ACTION/EVENEMENT ----- */
    /* -------------------------------------------------------- */

    #region CorporateAction
    /// <summary>
    /// Contient les caractéristiques générales d'une Corporate Action
    /// </summary>
    [SerializableAttribute()]
    [XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    [XmlRootAttribute("corporateAction", Namespace = "http://www.efs.org/2007/EFSmL-3-0", IsNullable = false)]
    /// EG 20140518 [19913] New 
    /// EG 20211020 [XXXXX] Nouvelle gestion des notices (URLNOTICE)
    public class CorporateAction : SpheresCommonIdentification
    {
        #region Members
        [XmlElementAttribute("market", Order = 1)]
        public MarketIdentification market;
        [XmlArrayItemAttribute("corporateEvent", IsNullable = false)]
        public CorporateEvent[] corporateEvent;
        [XmlElementAttribute("refNotice", Order = 2)]
        public RefNoticeIdentification refNotice;
        [XmlElementAttribute("pubDate", Order = 3)]
        public DateTime pubDate;
        [XmlIgnoreAttribute()]
        public bool refNoticeAddSpecified;
        [XmlArrayAttribute("refNoticeAdds", Order = 4)]
        [XmlArrayItemAttribute("refNoticeAdd", IsNullable = false)]
        public RefNoticeIdentification[] refNoticeAdd;
        [XmlElementAttribute("readystate", Order = 5)]
        public CorporateActionReadyStateEnum readystate;
        [XmlElementAttribute("embeddedstate", Order = 6)]
        public CorporateActionEmbeddedStateEnum embeddedState;
        [XmlIgnoreAttribute()]
        public bool cfiCodeSpecified;
        [XmlElementAttribute("cfiCode", Order = 7)]
        public CfiCodeCategoryEnum cfiCode;
        [XmlElementAttribute("idCAIssue", Order = 8)]
        public int idCAIssue;
        /// EG 20140518 [19913] New 
        [XmlIgnoreAttribute()]
        public bool corporateDocsSpecified;
        [XmlElementAttribute("corporateDocs", Order = 9)]
        public CorporateDocs corporateDocs;
        [XmlElementAttribute("urlnotice", Order = 10)]
        public string urlnotice;

        // Variables de travail (non sérialisées)
        [XmlIgnoreAttribute()]
        public NormMsgFactoryMQueue normMsgFactoryMQueue;
        [XmlIgnoreAttribute()]
        private int _idA;
        [XmlIgnoreAttribute()]
        private readonly string _CS;
        [XmlIgnoreAttribute()]
        private Cst.OTCml_TBL _CATable;
        #endregion Members
        #region Accessors
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public int IdCA
        {
            get { return Convert.ToInt32(spheresid); }
            set { spheresid = value.ToString(); }
        }
        #region IdA
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public int IdA
        {
            get { return _idA; }
            set { _idA = value; }
        }
        #endregion IdA
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public Cst.OTCml_TBL CATable
        {
            get { return _CATable; }
            set { _CATable = value; }
        }
        #endregion Accessors
        #region Constructors
        public CorporateAction() { }
        public CorporateAction(string pCS) 
        {
            _CS = pCS;
        }
        #endregion Constructors
        #region Methods
        //* --------------------------------------------------------------- *//
        // COMMON CORPOACTIONISSUE/CORPOACTION (Publiées/Intégrées) 
        //* --------------------------------------------------------------- *//

        #region Delete
        /// <summary>
        /// Suppression d'un Corporate action
        /// </summary>
        public Cst.ErrLevel Delete()
        {
            Cst.ErrLevel ret = Cst.ErrLevel.UNDEFINED;
            IDbTransaction dbTransaction = null;
            try
            {
                CAQueryBase caQry = null;
                switch (_CATable)
                {
                    case Cst.OTCml_TBL.CORPOACTIONISSUE:
                        caQry = new CAQueryISSUE(_CS);
                        break;
                    case Cst.OTCml_TBL.CORPOACTION:
                        caQry = new CAQueryEMBEDDED(_CS);
                        break;
                }
                if (null != caQry)
                {
                    dbTransaction = DataHelper.BeginTran(_CS);
                    QueryParameters qry = caQry.GetQueryDelete();
                    qry.Parameters["ID"].Value = IdCA;
                    DataHelper.ExecuteNonQuery(dbTransaction, CommandType.Text, qry.Query, qry.Parameters.GetArrayDbParameter());
                    DataHelper.CommitTran(dbTransaction);
                    ret = Cst.ErrLevel.SUCCESS;
                }
                else
                    ret = Cst.ErrLevel.FAILURE;
                return ret;
            }
            catch (Exception) 
            {
                ret= Cst.ErrLevel.FAILURE; 
                throw; 
            }
            finally
            {
                if (ret == Cst.ErrLevel.FAILURE)
                {
                    if (null != dbTransaction)
                        DataHelper.RollbackTran(dbTransaction);
                }
            }
        }
        #endregion Delete
        #region Load
        /// <summary>
        /// Recherche et chargement dans la classe d'une Corporate action publiée ou intégrée
        /// </summary>
        /// <param name="pCAInfo">Table source (CA intégrées/publiées)</param>
        /// <param name="pCAWhereMode">Mode (Critère) de recherche</param>
        /// <param name="pTemplatePath">Path des templates de la procédure d'ajustement</param>
        /// <returns></returns>
        /// EG 20211020 [XXXXX] Nouvelle gestion des notices (USEURLNOTICE)
        public Cst.ErrLevel Load(CAInfo pCAInfo, CATools.CAWhereMode pCAWhereMode, string pTemplatePath)
        {
            CAQueryBase caQry = null;
            switch (pCAInfo.CATable)
            {
                case Cst.OTCml_TBL.CORPOACTIONISSUE:
                    caQry = new CAQueryISSUE(_CS);
                    break;
                case Cst.OTCml_TBL.CORPOACTION:
                    caQry = new CAQueryEMBEDDED(_CS);
                    break;
            }
            if (null != caQry)
            {
                IdA = pCAInfo.IdA;
                CATable = pCAInfo.CATable;

                QueryParameters qryParameters = caQry.GetQuerySelect(pCAWhereMode);
                DataParameters parameters = qryParameters.Parameters;
                SetWhereParameters(parameters, pCAInfo, pCAWhereMode);
                DataSet ds = DataHelper.ExecuteDataset(_CS, CommandType.Text, qryParameters.Query, parameters.GetArrayDbParameter());
                if (null != ds)
                {
                    if (1 == ds.Tables[0].Rows.Count)
                    {
                        ArrayList aCorporateEvents = new ArrayList();
                        CorporateEvent _corporateEvent = new CorporateEvent();

                        DataRow rowCA = ds.Tables[0].Rows[0];
                        spheresid = rowCA["ID"].ToString();

                        switch (pCAInfo.CATable)
                        {
                            case Cst.OTCml_TBL.CORPOACTIONISSUE:
                                EFS_SerializeInfoBase serializeInfo = new EFS_SerializeInfoBase(typeof(NormMsgFactoryMQueue), rowCA["BUILDINFO"].ToString());
                                normMsgFactoryMQueue = (NormMsgFactoryMQueue)CacheSerializer.Deserialize(serializeInfo);
                                if (normMsgFactoryMQueue.buildingInfo.parametersSpecified)
                                {
                                    Nullable<CorporateActionReadyStateEnum> _readyState = null;
                                    if (false == Convert.IsDBNull(rowCA["READYSTATE"]))
                                        _readyState = CATools.CAReadyState(rowCA["READYSTATE"].ToString());
                                    if (false == Convert.IsDBNull(rowCA["CFICODE"]))
                                        _ = CATools.CACfiCodeCategory(rowCA["CFICODE"].ToString());

                                    SetNormMsgFactoryParameters(Convert.ToInt32(rowCA["IDM"]), normMsgFactoryMQueue.buildingInfo.parameters, _readyState);
                                    if (false == Convert.IsDBNull(rowCA["EMBEDDEDSTATE"]))
                                        embeddedState = CATools.CAEmbeddedState(rowCA["EMBEDDEDSTATE"].ToString()).Value;
                                    else
                                        embeddedState = CorporateActionEmbeddedStateEnum.UNKNOWN;
                                    _corporateEvent.SetNormMsgFactoryParameters(normMsgFactoryMQueue.buildingInfo.parameters, pTemplatePath);
                                    aCorporateEvents.Add(_corporateEvent);
                                }
                                // EG 20140518 [19913] New 
                                if (normMsgFactoryMQueue.buildingInfo.scriptsSpecified)
                                    SetNormMsgFactoryScripts(normMsgFactoryMQueue.buildingInfo.scripts);

                                break;
                            case Cst.OTCml_TBL.CORPOACTION:
                                // Corporate Actions
                                SetCorporateActionEmbedded(rowCA);
                                // Corporate Events
                                foreach (DataRow rowCE in ds.Tables[1].Rows)
                                {
                                    _corporateEvent = new CorporateEvent();
                                    _corporateEvent.SetCorporateEventEmbedded(rowCE);
                                    aCorporateEvents.Add(_corporateEvent);
                                }
                                // Corporate notices (additionnelles) 
                                ArrayList aRefNotices = new ArrayList();
                                foreach (DataRow rowCN in ds.Tables[2].Rows)
                                {
                                    RefNoticeIdentification refNotice = new RefNoticeIdentification
                                    {
                                        value = rowCN["REFNOTICE"].ToString(),
                                        pubDate = Convert.ToDateTime(rowCN["PUBDATE"]),
                                        fileName = rowCN["NOTICEFILENAME"].ToString(),
                                        useurlnoticeSpecified = (false == Convert.IsDBNull(rowCN["USEURLNOTICE"]))
                                    };
                                    if (refNotice.useurlnoticeSpecified)
                                        refNotice.useurlnotice = Convert.ToBoolean(rowCN["USEURLNOTICE"]);
                                    aRefNotices.Add(refNotice);
                                }
                                refNoticeAddSpecified = (0 < aRefNotices.Count);
                                refNoticeAdd = (RefNoticeIdentification[])aRefNotices.ToArray(typeof(RefNoticeIdentification));

                                // EG 20140518 [19913] New 
                                corporateDocs = new CorporateDocs(ds.Tables[3].Rows);
                                break;
                        }
                        // EG 20140518 [19913] New 
                        corporateDocsSpecified = (null != corporateDocs) && (0 < corporateDocs.Count);

                        corporateEvent = (CorporateEvent[])aCorporateEvents.ToArray(typeof(CorporateEvent));
                    }
                }
            }
            return Cst.ErrLevel.SUCCESS;
        }
        #endregion Load

        #region Exist
        /// <summary>
        /// Contrôle existence d'une corporate action dans la table CORPOACTIONISSUE ou CORPOACTION
        /// Clé : Marché + Référence de notice principale
        /// </summary>
        /// <param name="pCAInfo">Table concernée</param>
        /// <param name="pCAWhereMode">Mode (Critère) de recherche</param>
        /// <returns></returns>
        public bool Exist(CAInfo pCAInfo, CATools.CAWhereMode pCAWhereMode)
        {
            bool isExist = false;
            CAQueryBase caQry = null;
            switch (pCAInfo.CATable)
            {
                case Cst.OTCml_TBL.CORPOACTIONISSUE:
                    caQry = new CAQueryISSUE(_CS);
                    break;
                case Cst.OTCml_TBL.CORPOACTION:
                    caQry = new CAQueryEMBEDDED(_CS);
                    break;
            }
            QueryParameters qryParameters = caQry.GetQueryExist(pCAWhereMode);
            DataParameters parameters = qryParameters.Parameters;
            SetWhereParameters(parameters, pCAInfo, pCAWhereMode);
            object obj = DataHelper.ExecuteScalar(_CS, CommandType.Text, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter());
            if (null != obj)
                isExist = BoolFunc.IsTrue(obj);
            return isExist;
        }
        #endregion Exist
        #region Write
        /// <summary>
        /// Insertion/Mise à jour d'une CA PUBLIEE ou INTEGREE
        /// </summary>
        /// <param name="pCAInfo">Info liée au type de CA (ID, TABLE, etc)</param>
        /// <returns></returns>
        /// EG 20140518 [19913] Add pIdCAIssue
        public Cst.ErrLevel Write(CAInfo pCAInfo, Nullable<int> pIdCAIssue)
        {
            Cst.ErrLevel ret = Cst.ErrLevel.UNDEFINED;
            switch (pCAInfo.Mode)
            {
                case Cst.Capture.ModeEnum.New:
                    ret = Insert(pCAInfo, pIdCAIssue);
                    break;
                case Cst.Capture.ModeEnum.Update:
                    ret = Update(pCAInfo, pIdCAIssue);
                    break;
            }
            return ret;
        }
        #endregion Write
        #region Insert
        /// <summary>
        /// Insertion d'une CA PUBLIEE ou INTEGREE
        /// </summary>
        /// <param name="pCAInfo">Info liée au type de CA (ID, TABLE, etc)</param>
        /// <returns></returns>
        /// EG 20140518 [19913] Add pIdCAIssue
        public Cst.ErrLevel Insert(CAInfo pCAInfo, Nullable<int> pIdCAIssue)
        {
            Cst.ErrLevel ret = Cst.ErrLevel.UNDEFINED;
            CAQueryBase caQry = null;
            SQLUP.IdGetId idGetId = SQLUP.IdGetId.CORPOACTIONISSUE;
            switch (pCAInfo.CATable)
            {
                case Cst.OTCml_TBL.CORPOACTION:
                    caQry = new CAQueryEMBEDDED(_CS);
                    idGetId = SQLUP.IdGetId.CORPOACTION;
                    break;
                case Cst.OTCml_TBL.CORPOACTIONISSUE:
                    caQry = new CAQueryISSUE(_CS);
                    idGetId = SQLUP.IdGetId.CORPOACTIONISSUE;
                    break;
            }
            if (null != caQry)
            {
                IDbTransaction dbTransaction = null;
                bool isOk = true;
                try
                {
                    dbTransaction = DataHelper.BeginTran(_CS);
                    QueryParameters qryParameters = null;
                    qryParameters = caQry.GetQueryInsert();
                    SQLUP.GetId(out int id, dbTransaction, idGetId);
                    IdCA = id;
                    IdA = pCAInfo.IdA;
                    caQry.SetParametersInsert(this, qryParameters.Parameters);
                    // EG 20140518 [19913] Add pIdCAIssue
                    if (pIdCAIssue.HasValue)
                        qryParameters.Parameters["IDCAISSUE"].Value = pIdCAIssue.Value;
                    DataHelper.ExecuteNonQuery(dbTransaction, CommandType.Text, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter());
                    if (pCAInfo.CATable == Cst.OTCml_TBL.CORPOACTION)
                    {
                        CEQueryEMBEDDED ceQry = new CEQueryEMBEDDED(_CS);
                        qryParameters = ceQry.GetQueryInsert();
                        SQLUP.GetId(out id, dbTransaction, SQLUP.IdGetId.CORPOEVENT, SQLUP.PosRetGetId.First, corporateEvent.Length);
                        foreach (CorporateEvent _corporateEvent in corporateEvent)
                        {
                            ceQry.SetParametersInsert(id, IdCA, _corporateEvent, qryParameters.Parameters);
                            DataHelper.ExecuteNonQuery(dbTransaction, CommandType.Text, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter());
                        }

                        if (refNoticeAddSpecified)
                        {
                            CNQueryEMBEDDED cnQry = new CNQueryEMBEDDED(_CS);
                            qryParameters = cnQry.GetQueryInsert();
                            foreach (RefNoticeIdentification _refNotice in refNoticeAdd)
                            {
                                cnQry.SetParameters(IdCA, _refNotice, qryParameters.Parameters);
                                DataHelper.ExecuteNonQuery(dbTransaction, CommandType.Text, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter());
                            }
                        }

                        if (corporateDocsSpecified)
                        {
                            CAQueryDOCS cadocQry = new CAQueryDOCS(_CS);
                            qryParameters = cadocQry.GetQueryInsert();
                            foreach (CorporateDoc _corporateDoc in corporateDocs.Values)
                            {
                                cadocQry.SetParameters(IdCA, _corporateDoc, qryParameters.Parameters);
                                DataHelper.ExecuteNonQuery(dbTransaction, CommandType.Text, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter());
                            }
                        }

                    }
                    DataHelper.CommitTran(dbTransaction);
                    ret = Cst.ErrLevel.SUCCESS;
                }
                catch (Exception) { isOk = false; throw; }
                finally
                {
                    if (false == isOk)
                    {
                        if (null != dbTransaction)
                            DataHelper.RollbackTran(dbTransaction);
                    }
                }
            }
            return ret;
        }
        #endregion Insert
        #region IsContractSizeAdjusted
        /// <summary>
        /// La corporate action nécessite-t-elle un ajustement du contract size ?
        /// </summary>
        /// <param name="pCategory"></param>
        /// <returns></returns>
        public bool IsContractSizeAdjusted(CfiCodeCategoryEnum pCategory)
        {
            bool _isContractSizeAdjusted = false;
            Adjustment _adjustment = corporateEvent[0].procedure.adjustment;
            if (_adjustment is AdjustmentRatio)
            {
                AdjustmentRatio _ratio = _adjustment as AdjustmentRatio;
                switch (pCategory)
                {
                    case CfiCodeCategoryEnum.Future:
                        _isContractSizeAdjusted = (_ratio.contract.futureSpecified && _ratio.contract.future.contractSizeSpecified);
                        break;
                    case CfiCodeCategoryEnum.Option:
                        _isContractSizeAdjusted = (_ratio.contract.optionSpecified && _ratio.contract.option.contractSizeSpecified);
                        break;
                }
            }
            return _isContractSizeAdjusted;
        }
        #endregion IsContractSizeAdjusted
        #region IsEqualisationPaymentSpecified
        /// <summary>
        /// La corporate action nécessite-t-elle un calcul d'EqualisationPayment ?
        /// </summary>
        /// <param name="pCategory"></param>
        /// <returns></returns>
        // EG 20141106 [20253] Equalisation payment
        public bool IsEqualisationPaymentSpecified(CfiCodeCategoryEnum pCategory)
        {
            bool _isEqualisationPaymentSpecified = false;
            Adjustment _adjustment = corporateEvent[0].procedure.adjustment;
            if (_adjustment is AdjustmentRatio)
            {
                AdjustmentRatio _ratio = _adjustment as AdjustmentRatio;
                switch (pCategory)
                {
                    case CfiCodeCategoryEnum.Future:
                        _isEqualisationPaymentSpecified = (_ratio.contract.futureSpecified && _ratio.contract.future.equalisationPaymentSpecified);
                        break;
                    case CfiCodeCategoryEnum.Option:
                        _isEqualisationPaymentSpecified = (_ratio.contract.optionSpecified && _ratio.contract.option.equalisationPaymentSpecified);
                        break;
                }
            }
            return _isEqualisationPaymentSpecified;
        }
        #endregion IsEqualisationPaymentSpecified


        #region Update
        /// <summary>
        /// Mise à jour d'une CA PUBLIEE ou INTEGREE
        /// </summary>
        /// <param name="pCAInfo">Info liée au type de CA (ID, TABLE, etc)</param>
        /// <returns></returns>
        public Cst.ErrLevel Update(CAInfo pCAInfo, Nullable<int> pIdCAIssue)
        {
            Cst.ErrLevel ret = Cst.ErrLevel.UNDEFINED;
            CAQueryBase caQry = null;
            switch (pCAInfo.CATable)
            {
                case Cst.OTCml_TBL.CORPOACTION:
                    caQry = new CAQueryEMBEDDED(_CS);
                    break;
                case Cst.OTCml_TBL.CORPOACTIONISSUE:
                    caQry = new CAQueryISSUE(_CS);
                    break;
            }
            if (null != caQry)
            {
                IDbTransaction dbTransaction = null;
                bool isOk = true;
                try
                {
                    dbTransaction = DataHelper.BeginTran(_CS);
                    QueryParameters qryParameters = null;
                    qryParameters = caQry.GetQueryUpdate();
                    caQry.SetParametersUpdate(this, qryParameters.Parameters);
                    if (pIdCAIssue.HasValue)
                        qryParameters.Parameters["IDCAISSUE"].Value = pIdCAIssue.Value;
                    DataHelper.ExecuteNonQuery(dbTransaction, CommandType.Text, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter());

                    bool isExist = false;
                    QueryParameters qryParametersExist = null;
                    DataParameters parameters = null;
                    object obj = null;
                    if (pCAInfo.CATable == Cst.OTCml_TBL.CORPOACTION)
                    {
                        #region CORPOEVENT
                        CEQueryEMBEDDED ceQry = new CEQueryEMBEDDED(_CS);
                        qryParameters = ceQry.GetQueryUpdate();
                        foreach (CorporateEvent _corporateEvent in corporateEvent)
                        {
                            ceQry.SetParametersUpdate(_corporateEvent, qryParameters.Parameters);
                            DataHelper.ExecuteNonQuery(dbTransaction, CommandType.Text, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter());
                        }
                        #endregion CORPOEVENT
                        #region CORPONOTICE
                        CNQueryEMBEDDED cnQry = new CNQueryEMBEDDED(_CS);
                        qryParametersExist = cnQry.GetQueryExist(CATools.CAWhereMode.ID);
                        qryParametersExist.Parameters["ID"].Value = IdCA;
                        parameters = qryParametersExist.Parameters;
                        obj = DataHelper.ExecuteScalar(dbTransaction, CommandType.Text, qryParametersExist.Query, qryParametersExist.Parameters.GetArrayDbParameter());
                        if (null != obj)
                            isExist = BoolFunc.IsTrue(obj);
                        if (isExist)
                        {
                            qryParameters = cnQry.GetQueryDelete();
                            qryParameters.Parameters["ID"].Value = IdCA;
                            DataHelper.ExecuteNonQuery(dbTransaction, CommandType.Text, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter());
                        }

                        if (refNoticeAddSpecified)
                        {
                            qryParameters = cnQry.GetQueryInsert();
                            foreach (RefNoticeIdentification _refNotice in refNoticeAdd)
                            {
                                cnQry.SetParameters(IdCA, _refNotice, qryParameters.Parameters);
                                DataHelper.ExecuteNonQuery(dbTransaction, CommandType.Text, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter());
                            }
                        }
                        #endregion CORPONOTICE
                        #region CORPODOCS
                        /// EG 20140518 [19913]
                        isExist = false;
                        CAQueryDOCS cadocQry = new CAQueryDOCS(_CS);
                        qryParametersExist = cadocQry.GetQueryExist();
                        qryParametersExist.Parameters["ID"].Value = IdCA;
                        parameters = qryParametersExist.Parameters;
                        obj = DataHelper.ExecuteScalar(dbTransaction, CommandType.Text, qryParametersExist.Query, qryParametersExist.Parameters.GetArrayDbParameter());
                        if (null != obj)
                            isExist = BoolFunc.IsTrue(obj);
                        if (isExist)
                        {
                            qryParameters = cadocQry.GetQueryDelete();
                            qryParameters.Parameters["ID"].Value = IdCA;
                            DataHelper.ExecuteNonQuery(dbTransaction, CommandType.Text, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter());
                        }

                        if (corporateDocsSpecified)
                        {
                            qryParameters = cadocQry.GetQueryInsert();
                            foreach (CorporateDoc _corporateDoc in corporateDocs.Values)
                            {
                                cadocQry.SetParameters(IdCA, _corporateDoc, qryParameters.Parameters);
                                qryParameters.Parameters["IDAUPD"].Value = pCAInfo.IdA;
                                // FI 20200820 [25468] Dates systemes en UTC
                                qryParameters.Parameters["DTUPD"].Value = OTCmlHelper.GetDateSysUTC(_CS);
                                DataHelper.ExecuteNonQuery(dbTransaction, CommandType.Text, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter());
                            }
                        }
                        #endregion CORPODOCS
                    }
                    ret = Cst.ErrLevel.SUCCESS;
                    DataHelper.CommitTran(dbTransaction);
                }
                catch (Exception) { isOk = false; throw; }
                finally
                {
                    if (false == isOk)
                    {
                        if (null != dbTransaction)
                            DataHelper.RollbackTran(dbTransaction);
                    }
                }
            }
            return ret;
        }
        #endregion Update
        #region SetEmbeddedState
        /// <summary>
        /// Mise à jour du statut d'intégration d'une CA PUBLIEE
        /// </summary>
        public void SetEmbeddedState(CAInfo pCAInfo, Cst.ErrLevel pErrLevel)
        {
            if (pCAInfo.CATable == Cst.OTCml_TBL.CORPOACTIONISSUE)
            {
                switch (pErrLevel)
                {
                    // EG 20140107 [19448] Add NOTHINGTODO
                    case Cst.ErrLevel.NOTHINGTODO:
                    case Cst.ErrLevel.SUCCESS:
                        embeddedState = CorporateActionEmbeddedStateEnum.SUCCESS;
                        break;
                    case Cst.ErrLevel.DATANOTFOUND:
                        embeddedState = CorporateActionEmbeddedStateEnum.DATANOTFOUND;
                        break;
                    case Cst.ErrLevel.ENTITYMARKET_UNMANAGED:
                        embeddedState = CorporateActionEmbeddedStateEnum.UNMANAGED;
                        break;
                    case Cst.ErrLevel.FAILURE:
                    default:
                        embeddedState = CorporateActionEmbeddedStateEnum.ERROR;
                        break;
                }
            }
        }
        #endregion SetEmbeddedState
        #region SetEmbeddedState
        /// <summary>
        /// Mise à jour du statut d'intégration d'une CA PUBLIEE
        /// </summary>
        public ProcessStateTools.StatusEnum GetStatus()
        {
            ProcessStateTools.StatusEnum _status = ProcessStateTools.StatusEnum.NA;
            switch (embeddedState)
            {
                case CorporateActionEmbeddedStateEnum.DATANOTFOUND:
                    _status = ProcessStateTools.StatusWarningEnum;
                    break;
                case CorporateActionEmbeddedStateEnum.ERROR:
                    _status = ProcessStateTools.StatusErrorEnum;
                    break;
                case CorporateActionEmbeddedStateEnum.NEWEST:
                case CorporateActionEmbeddedStateEnum.UNMANAGED:
                    _status = ProcessStateTools.StatusNoneEnum;
                    break;
                case CorporateActionEmbeddedStateEnum.REQUESTED:
                    _status = ProcessStateTools.StatusProgressEnum;
                    break;
                case CorporateActionEmbeddedStateEnum.UNKNOWN:
                    _status = ProcessStateTools.StatusUnknownEnum;
                    break;
                case CorporateActionEmbeddedStateEnum.SUCCESS:
                    _status = ProcessStateTools.StatusSuccessEnum;
                    // EG 20160404 Migration vs2013
                    //break;
                    break;
            }
            return _status;
        }
        #endregion SetEmbeddedState
        #region UpdateStatus
        /// <summary>
        /// Mise à jour du statut d'un CA PUBLIEE/INTEGREE
        /// </summary>
        /// <param name="pCAInfo">Info liée au type de CA (ID, TABLE, etc)</param>
        public Cst.ErrLevel UpdateStatus(CAInfo pCAInfo)
        {
            Cst.ErrLevel ret = Cst.ErrLevel.UNDEFINED;
            CAQueryBase caQry = null;
            switch (pCAInfo.CATable)
            {
                case Cst.OTCml_TBL.CORPOACTION:
                    caQry = new CAQueryEMBEDDED(_CS);
                    break;
                case Cst.OTCml_TBL.CORPOACTIONISSUE:
                    caQry = new CAQueryISSUE(_CS);
                    break;
            }
            if (null != caQry)
            {
                IDbTransaction dbTransaction = null;
                bool isOk = true;
                try
                {
                    dbTransaction = DataHelper.BeginTran(_CS);
                    // EG 20160404 Migration vs2013
                    //int id = 0;
                    QueryParameters qryParameters = null;
                    qryParameters = caQry.GetQueryUpdateStatus();
                    qryParameters.Parameters["ID"].Value = pCAInfo.Id;
                    qryParameters.Parameters["READYSTATE"].Value = readystate.ToString();
                    if (pCAInfo.CATable == Cst.OTCml_TBL.CORPOACTIONISSUE)
                        qryParameters.Parameters["EMBEDDEDSTATE"].Value = embeddedState.ToString();
                    qryParameters.Parameters["IDAUPD"].Value = pCAInfo.IdA;
                    // FI 20200820 [25468] Dates systemes en UTC
                    qryParameters.Parameters["DTUPD"].Value = OTCmlHelper.GetDateSysUTC(_CS);

                    DataHelper.ExecuteNonQuery(dbTransaction, CommandType.Text, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter());
                    ret = Cst.ErrLevel.SUCCESS;
                    DataHelper.CommitTran(dbTransaction);
                }
                catch (Exception) { isOk = false; throw; }
                finally
                {
                    if (false == isOk)
                    {
                        if (null != dbTransaction)
                            DataHelper.RollbackTran(dbTransaction);
                    }
                }
            }
            return ret;
        }
        #endregion UpdateStatus


        #region SetWhereParameters
        /// <summary>
        /// Valorisation des paramètres de recherche d'une CA INTEGREE ou PUBLIEE (clause Where) 
        /// En fonction du mode et de la table, recherche par :
        /// ● son ID
        /// ● sa référence de notice (MARCHE + REFNOTICE)
        /// </summary>
        /// <param name="pParameters">Paramètres</param>
        /// <param name="pCAInfo">Info liée au type de CA (ID, TABLE, etc)</param>
        /// <param name="pCAWhereMode">Mode (Critère) de recherche</param>
        private void SetWhereParameters(DataParameters pParameters, CAInfo pCAInfo, CATools.CAWhereMode pCAWhereMode)
        {
            switch (pCAWhereMode)
            {
                case CATools.CAWhereMode.ID:
                    pParameters["ID"].Value = pCAInfo.Id;
                    break;
                case CATools.CAWhereMode.NOTICE:
                    pParameters["REFNOTICE"].Value = refNotice.value;
                    pParameters["CFICODE"].Value = cfiCodeSpecified?ReflectionTools.ConvertEnumToString<CfiCodeCategoryEnum>(cfiCode):Convert.DBNull;
                    switch (pCAInfo.CATable)
                    {
                        case Cst.OTCml_TBL.CORPOACTIONISSUE:
                            pParameters["CAMARKET"].Value = market.FIXML_SecurityExchange;
                            break;
                        case Cst.OTCml_TBL.CORPOACTION:
                            pParameters["IDM"].Value = Convert.ToInt32(market.spheresid);
                            break;
                    }
                    break;
            }
        }
        #endregion SetWhereParameters

        //* --------------------------------------------------------------- *//
        // COTE CORPOACTIONISSUE (Publiées) 
        //* --------------------------------------------------------------- *//

        #region ConstructNormMsgFactoryMessage
        /// <summary>
        /// Création d'un message de Type NORMMSGFACTORY avec Cst.ProcessTypeEnum.CORPOACTIONINTEGRATE
        /// destiné à l'intégration d'un Corporate action publiée
        /// </summary>
        /// EG 20140518 [19913]
        public Cst.ErrLevel ConstructNormMsgFactoryMessage()
        {
            MQueueAttributes mQueueAttributes = new MQueueAttributes()
            {
                connectionString = _CS,
                id = IdCA,
                identifier = identifier
            };

            normMsgFactoryMQueue = new NormMsgFactoryMQueue(mQueueAttributes)
            {
                buildingInfo = new NormMsgBuildingInfo()
                {
                    id = IdCA,
                    idSpecified = true,
                    identifierSpecified = true,
                    identifier = identifier,
                    processType = Cst.ProcessTypeEnum.CORPOACTIONINTEGRATE
                }
            };

            normMsgFactoryMQueue.buildingInfo.parametersSpecified = true;
            normMsgFactoryMQueue.buildingInfo.parameters = new MQueueparameters();
            normMsgFactoryMQueue.buildingInfo.parameters.Add(GetNormMsgFactoryParameters());
            
            normMsgFactoryMQueue.buildingInfo.scriptsSpecified = corporateDocsSpecified;
            if (corporateDocsSpecified)
            {
                normMsgFactoryMQueue.buildingInfo.scripts = new MQueueScripts();
                ArrayList aScripts = new ArrayList();
                foreach (CorporateDoc corporateDoc in corporateDocs.Values)
                {
                    MQueueScript _script = new MQueueScript
                    {
                        docName = corporateDoc.identifier,
                        docType = corporateDoc.docType.ToString(),
                        runTime = corporateDoc.runTime.ToString(),
                        script = corporateDoc.script
                    };
                    aScripts.Add(_script);
                }
                if (0 < aScripts.Count)
                    normMsgFactoryMQueue.buildingInfo.scripts.script = (MQueueScript[])aScripts.ToArray(typeof(MQueueScript));
            }
            return Cst.ErrLevel.SUCCESS;
        }
        #endregion ConstructNormMsgFactoryMessage
        #region GetNormMsgFactoryParameters
        /// <summary>
        /// Création des paramètres du message NORMMSGFACTORY contenant l'ensemble des caractéristiques de
        /// la Corporate action (CA, CE, CE Procedure (Sous-jacents, Composants, Règles d'arrondi)
        /// </summary>
        /// <returns>Array de paramètres</returns>
        /// EG 20211020 [XXXXX] Nouvelle gestion des notices (USEURLNOTICE et URLNOTICE)
        public MQueueparameter[] GetNormMsgFactoryParameters()
        {
            MQueueparameters parameters = new MQueueparameters();
            #region Paramètres CA
            MQueueparameter parameter = new MQueueparameter("CAMARKET", TypeData.TypeDataEnum.@string);
            parameter.SetValue(market.FIXML_SecurityExchange);
            parameters.Add(parameter);

            parameter = new MQueueparameter("PUBDATE", TypeData.TypeDataEnum.@date);
            parameter.SetValue(pubDate);
            parameters.Add(parameter);

            if (cfiCodeSpecified)
            {
                parameter = new MQueueparameter("CFICODE", TypeData.TypeDataEnum.@string);
                parameter.SetValue(ReflectionTools.ConvertEnumToString<CfiCodeCategoryEnum>(cfiCode));
                parameters.Add(parameter);
            }

            parameter = new MQueueparameter("URLNOTICE", TypeData.TypeDataEnum.@string);
            parameter.SetValue(urlnotice);
            parameters.Add(parameter);

            if (refNotice.useurlnoticeSpecified)
            {
                parameter = new MQueueparameter("USEURLNOTICE", TypeData.TypeDataEnum.@bool);
                parameter.SetValue(refNotice.useurlnotice);
                parameters.Add(parameter);
            }
            parameter = new MQueueparameter("REFNOTICE", TypeData.TypeDataEnum.@string);
            parameter.SetValue(refNotice.value);
            parameter.displayNameSpecified = displaynameSpecified;
            parameter.displayName = displayname;
            parameter.nameSpecified = identifierSpecified;
            parameter.name = identifier;
            parameter.ExValueSpecified = true;
            parameter.ExValue = refNotice.fileName;
            parameters.Add(parameter);

            parameter = new MQueueparameter("READYSTATE", TypeData.TypeDataEnum.@string);
            parameter.SetValue(readystate.ToString());
            parameters.Add(parameter);

            if (refNoticeAddSpecified)
            {
                int i = 1;
                foreach (RefNoticeIdentification noticeAdd in refNoticeAdd)
                {
                    parameter = new MQueueparameter("REFNOTICEADD" + i.ToString(), TypeData.TypeDataEnum.@string)
                    {
                        Value = noticeAdd.value,
                        ExValueSpecified = true,
                        ExValue = noticeAdd.fileName
                    };
                    parameters.Add(parameter);
                    parameter = new MQueueparameter("REFNOTICEADDPUBDATE" + i.ToString(), TypeData.TypeDataEnum.@date);
                    parameter.SetValue(noticeAdd.pubDate);
                    parameters.Add(parameter);
                    i++;
                }
            }

            if (descriptionSpecified)
            {
                parameter = new MQueueparameter("DOCUMENTATION", TypeData.TypeDataEnum.@string);
                parameter.SetValue(description);
                parameters.Add(parameter);
            }

            #endregion Paramètres CA
            #region Paramètres CE
            parameters.Add(corporateEvent[0].GetNormMsgFactoryParameters());
            #endregion Paramètres CE

            return parameters.parameter;
        }
        #endregion GetNormMsgFactoryParameters
        #region SetNormMsgFactoryParameters
        /// <summary>
        /// Initialisation des éléments de la classe à l'aide de ceux du message (de type NORMMSGFACTORY)
        /// </summary>
        /// <param name="pIdM">MARCHE</param>
        /// <param name="pMQueueParameters">Paramètres du messages</param>
        /// <param name="pReadyState">Statut d'intégration</param>
        public List<string> SetNormMsgFactoryParameters(int pIdM, MQueueparameters pMQueueParameters)
        {
            return SetNormMsgFactoryParameters(pIdM, pMQueueParameters, null);
        }
        /// EG 20211020 [XXXXX] Nouvelle gestion des notices (USEURLNOTICE et URLNOTICE)
        public List<string> SetNormMsgFactoryParameters(int pIdM, MQueueparameters pMQueueParameters, Nullable<CorporateActionReadyStateEnum> pReadyState)
        {
            List<string> _missingParameters = new List<string>();
            MQueueparameter _parameter = pMQueueParameters["CAMARKET"];
            if (null != _parameter)
            {
                market = new MarketIdentification();
                MQueueparameter _parameter2 = pMQueueParameters["MARKETTYPE"];
                if (null != _parameter2)
                {
                    market.ISO10383_ALPHA4 = _parameter.Value;
                    market.ISO10383_ALPHA4Specified = true;
                    market.exchangeSymbolSpecified = _parameter.ExValueSpecified;
                    market.exchangeSymbol = _parameter.ExValue;
                }
                else
                {
                    market.FIXML_SecurityExchange = _parameter.Value;
                    market.FIXML_SecurityExchangeSpecified = true;
                }
                market.spheresid = pIdM.ToString();
            }
            else if (0 < pIdM)
            {
                market = new MarketIdentification
                {
                    spheresid = pIdM.ToString()
                };
            }
            else
                _missingParameters.Add("CAMARKET");

            if (pReadyState.HasValue)
                readystate = pReadyState.Value;
            else
            {
                _parameter = pMQueueParameters["READYSTATE"];
                if (null != _parameter)
                    readystate = CATools.CAReadyState(_parameter.Value).Value;
                else
                    readystate = CorporateActionReadyStateEnum.PUBLISHED;
            }

            _parameter = pMQueueParameters["CFICODE"];
            cfiCodeSpecified = (null != _parameter);
            if (cfiCodeSpecified)
                cfiCode = CATools.CACfiCodeCategory(_parameter.Value).Value;

            _parameter = pMQueueParameters["PUBDATE"];
            if (null != _parameter)
                pubDate = new EFS_Date(_parameter.Value).DateValue;
            else
                _missingParameters.Add("PUBDATE");

            _parameter = pMQueueParameters["URLNOTICE"];
            if (null != _parameter)
                urlnotice = _parameter.Value;
            else
                _missingParameters.Add("URLNOTICE");

            _parameter = pMQueueParameters["REFNOTICE"];
            if (null != _parameter)
            {
                identifierSpecified = _parameter.nameSpecified;
                identifier = _parameter.name;
                displaynameSpecified = _parameter.displayNameSpecified;
                displayname = _parameter.displayName;
                refNotice = new RefNoticeIdentification
                {
                    value = _parameter.Value,
                    fileName = _parameter.ExValue,
                    pubDate = pubDate,
                    useurlnoticeSpecified = (null != pMQueueParameters["USEURLNOTICE"])
                };
                if (refNotice.useurlnoticeSpecified)
                    refNotice.useurlnotice = Convert.ToBoolean(pMQueueParameters["USEURLNOTICE"].Value);
            }
            else
                _missingParameters.Add("PUBDATE");


            ArrayList aRefNoticeAdd = new ArrayList();
            for (int i = 1; i < 10; i++)
            {
                _parameter = pMQueueParameters["REFNOTICEADD" + i.ToString()];
                if (null != _parameter)
                {
                    RefNoticeIdentification _refNoticeadd = new RefNoticeIdentification
                    {
                        value = _parameter.Value,
                        fileName = _parameter.ExValue,
                        useurlnoticeSpecified = refNotice.useurlnoticeSpecified,
                        useurlnotice = refNotice.useurlnotice
                    };

                    _parameter = pMQueueParameters["REFNOTICEADDPUBDATE" + i.ToString()];
                    if (null != _parameter)
                        _refNoticeadd.pubDate = new EFS_Date(_parameter.Value).DateValue;
                    else
                        _refNoticeadd.pubDate = pubDate;

                    aRefNoticeAdd.Add(_refNoticeadd);
                }
            }
            refNoticeAddSpecified = (0 < aRefNoticeAdd.Count);
            refNoticeAdd = (RefNoticeIdentification[])aRefNoticeAdd.ToArray(typeof(RefNoticeIdentification));

            _parameter = pMQueueParameters["DOCUMENTATION"];
            if (null != _parameter)
            {
                descriptionSpecified = true;
                description = _parameter.Value;
            }
            return _missingParameters;
        }
        #endregion SetNormMsgFactoryParameters

        #region SetNormMsgFactoryScripts
        /// EG 20140518 [19913]
        public void SetNormMsgFactoryScripts(MQueueScripts pMQueueScripts)
        {
            corporateDocsSpecified = true;
            corporateDocs = new CorporateDocs(); 
            for (int i = 0; i < pMQueueScripts.script.Length; i++)
            {
                MQueueScript _script = pMQueueScripts.script[i];
                CorporateDoc _corporateDoc = new CorporateDoc()
                {
                    docType = (CATools.DOCTypeEnum)CATools.CADocType(_script.docType),
                    runTime = (CATools.SQLRunTimeEnum)CATools.CADocRunTime(_script.runTime),
                    script = _script.script
                };
                _corporateDoc.SetFileName(_script.docName, market.FIXML_SecurityExchange, refNotice.value);

                Pair<CATools.DOCTypeEnum, CATools.SQLRunTimeEnum> _key = new Pair<CATools.DOCTypeEnum, CATools.SQLRunTimeEnum>(_corporateDoc.docType, _corporateDoc.runTime);
                corporateDocs.Add(_key, _corporateDoc);
            }
        }
        #endregion SetNormMsgFactoryScripts

        //* --------------------------------------------------------------- *//
        // COTE CORPOACTION (Intégrées) 
        //* --------------------------------------------------------------- *//

        #region SetCorporateActionEmbedded
        /// <summary>
        /// Initialisation des éléments de la classe à l'aide des données d'une Corporate action intégrée
        /// </summary>
        /// <param name="pRow">DataRow de la CA</param>
        /// EG 20140518 [19913] Add IDCAISSUE
        /// EG 20211020 [XXXXX] Nouvelle gestion des notices (USEURLNOTICE et URLNOTICE)
        public void SetCorporateActionEmbedded(DataRow pRow)
        {
            if (false == Convert.IsDBNull(pRow["IDCAISSUE"]))
                idCAIssue = Convert.ToInt32(pRow["IDCAISSUE"]);
            identifierSpecified = (false == Convert.IsDBNull(pRow["IDENTIFIER"]));
            if (identifierSpecified)
                identifier = pRow["IDENTIFIER"].ToString();
            displaynameSpecified = (false == Convert.IsDBNull(pRow["DISPLAYNAME"]));
            if (displaynameSpecified)
                displayname = pRow["DISPLAYNAME"].ToString();
            descriptionSpecified = (false == Convert.IsDBNull(pRow["DOCUMENTATION"]));
            if (descriptionSpecified)
                description = pRow["DOCUMENTATION"].ToString();

            market = new MarketIdentification
            {
                spheresid = pRow["IDM"].ToString()
            };

            pubDate = Convert.ToDateTime(pRow["PUBDATE"]);

            cfiCodeSpecified = (false == Convert.IsDBNull(pRow["CFICODE"]));
            if (cfiCodeSpecified)
                cfiCode = CATools.CACfiCodeCategory(pRow["CFICODE"].ToString()).Value;
            

            readystate = CATools.CAReadyState(pRow["READYSTATE"].ToString()).Value;

            refNotice = new RefNoticeIdentification
            {
                value = pRow["REFNOTICE"].ToString(),
                pubDate = pubDate,
                useurlnoticeSpecified = (false == Convert.IsDBNull(pRow["USEURLNOTICE"]))
            };
            if (false == Convert.IsDBNull(pRow["NOTICEFILENAME"]))
                refNotice.fileName = pRow["NOTICEFILENAME"].ToString();
            urlnotice = pRow["URLNOTICE"].ToString();
            if (refNotice.useurlnoticeSpecified)
                refNotice.useurlnotice = Convert.ToBoolean(pRow["USEURLNOTICE"]);
        }
        #endregion SetCorporateActionEmbedded

        #region GetCorporateDoc
        public CorporateDoc GetCorporateDoc(Pair<CATools.DOCTypeEnum, CATools.SQLRunTimeEnum> pKey)
        {
            CorporateDoc _corporateDoc = null;
            if (corporateDocsSpecified)
                corporateDocs.GetCorporateDoc(pKey);
            return _corporateDoc;
        }
        #endregion GetCorporateDoc
        #region DeleteCorporateDoc
        public void DeleteCorporateDoc(Pair<CATools.DOCTypeEnum, CATools.SQLRunTimeEnum> pKey)
        {
            if (corporateDocsSpecified)
                corporateDocs.DeleteCorporateDoc(pKey);
        }
        #endregion DeleteCorporateDoc
        #endregion Methods
    }
    #endregion CorporateAction

    #region CorporateDoc
    /// <summary>
    /// Contient les caractéristiques des scripts SQL associés à la Corporate action
    /// </summary>
    [SerializableAttribute()]
    [XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    /// EG 20140518 [19913]
    public class CorporateDoc  : SpheresCommonIdentification
    {
        #region Members
        [XmlElementAttribute("docType", Order = 1)]
        public CATools.DOCTypeEnum docType;
        [XmlElementAttribute("runTime", Order = 2)]
        public CATools.SQLRunTimeEnum runTime;
        [XmlElementAttribute("script", Order = 3)]
        public byte[] script;

        [XmlIgnoreAttribute()]
        private int _idA;

        #endregion Members
        #region Accessors
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public int IdCA
        {
            get { return Convert.ToInt32(spheresid); }
            set { spheresid = value.ToString(); }
        }
        #region IdA
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public int IdA
        {
            get { return _idA; }
            set { _idA = value; }
        }
        #endregion IdA
        #region FullFileName
        public string FullFileName
        {
            get
            {
                string _fullFileName = identifier;
                string _suffix;
                switch (docType)
                {
                    case CATools.DOCTypeEnum.MSSQL:
                    case CATools.DOCTypeEnum.ORA:
                        _suffix = "sql";
                        break;
                    default:
                        _suffix = docType.ToString().ToLower();
                        break;
                }
                if (StrFunc.IsFilled(_suffix))
                    _fullFileName += "." + _suffix;
                return _fullFileName;
            }
        }
        #endregion FullFileName

        #endregion Accessors
        #region Constructors
        public CorporateDoc() { }
        #endregion Constructors
        #region Methods
        #region FullPathFileName
        public string FullPathFileName(string pTemporaryPath)
        {
            return pTemporaryPath + ((false == pTemporaryPath.EndsWith(@"\")) ? @"\" : string.Empty) + FullFileName;
        }
        #endregion FullPathFileName
        #region SaveToTemporaryDirectory
        /// <summary>
        /// Sauvegarde du script dans le répertoire temporaire
        /// </summary>
        /// <param name="pTemporaryPath"></param>
        /// <returns></returns>
        public string SaveToTemporaryDirectory(string pTemporaryPath)
        {
            
            string fullPath = string.Empty;
            if (null != script)
            {
                fullPath = FullPathFileName(pTemporaryPath);
                FileTools.WriteBytesToFile(script, fullPath, FileTools.WriteFileOverrideMode.Override);
            }
            return fullPath;
        }
        #endregion SaveToTemporaryDirectory

        #region SetCorporateDoc
        public void SetCorporateDoc(DataRow pRow)
        {
            spheresid = pRow["IDCA"].ToString();
            identifierSpecified = (false == Convert.IsDBNull(pRow["DOCNAME"]));
            if (identifierSpecified)
                identifier = pRow["DOCNAME"].ToString();

            docType = CATools.CADocType(pRow["DOCTYPE"].ToString()).Value;
            runTime = CATools.CADocRunTime(pRow["RUNTIME"].ToString()).Value;

            if (false == Convert.IsDBNull(pRow["LODOC"]))
            {
                if (pRow["LODOC"].GetType().Equals(typeof(System.String)))
                {
                    string content = pRow["LODOC"].ToString();
                    if (CATools.DOCTypeEnum.XML == docType)
                    {
                        if (content.IndexOf(@"<?mso-application") < 0)
                        {
                            content = content.Replace(@"<?xml version=""1.0"" encoding=""utf-16""?>", string.Empty);
                            content = content.Replace(@"<?xml version=""1.0"" encoding=""UTF-16""?>", string.Empty);
                        }
                    }
                    script = Encoding.UTF8.GetBytes(content);
                }
                else
                {
                    script = (byte[])pRow["LODOC"];
                }
            }
        }
        #endregion SetCorporateEventEmbedded

        #region FileName
        public void SetFileName(string pFileName, string pFIXML_SecurityExchange,string pRefNotice)
        {
            identifierSpecified = true;
            if (StrFunc.IsFilled(pFileName))
                identifier = pFileName;
            else
                identifier = docType.ToString() + "-" + pFIXML_SecurityExchange + "-" + pRefNotice.Replace("/", "-") + "(" + runTime.ToString().ToLower() + ")";
        }
        #endregion FileName

        #endregion Methods
    }
    #endregion CorporateDoc
    #region CorporateDocs
    /// <summary>
    /// Dictionaire des scripts SQL associés à la Corporate action
    /// </summary>
    /// EG 20140518 [19913]
    // EG 20180423 Analyse du code Correction [CA1405]
    [SerializableAttribute()]
    [ComVisible(false)]
    public class CorporateDocs : Dictionary<Pair<CATools.DOCTypeEnum, CATools.SQLRunTimeEnum>, CorporateDoc>
    {
        #region Members
        #endregion Members
        #region Constructors
        public CorporateDocs(): base(new PairComparer<CATools.DOCTypeEnum, CATools.SQLRunTimeEnum>())
        {
            
        }
        public CorporateDocs(DataRowCollection pRows): base(new PairComparer<CATools.DOCTypeEnum, CATools.SQLRunTimeEnum>())
        {
            foreach (DataRow row in pRows)
            {
                CorporateDoc _corporateDoc = new CorporateDoc();
                _corporateDoc.SetCorporateDoc(row);
                CATools.DOCTypeEnum _docType = CATools.CADocType(row["DOCTYPE"].ToString()).Value;
                CATools.SQLRunTimeEnum _runTime = CATools.CADocRunTime(row["RUNTIME"].ToString()).Value;
                Pair<CATools.DOCTypeEnum, CATools.SQLRunTimeEnum> key = new Pair<CATools.DOCTypeEnum, CATools.SQLRunTimeEnum>(_docType, _runTime);
                this.Add(key, _corporateDoc);
            }
        }

        #endregion Constructors
        #region Methods
        #region SetCorporateDoc
        public void SetCorporateDoc(Pair<CATools.DOCTypeEnum, CATools.SQLRunTimeEnum> pKey, 
            string pFileName, string pFIXML_SecurityEchange, string pRefNotice, Byte[] FileBytes)
        {
            if (false == this.ContainsKey(pKey))
                this.Add(pKey, new CorporateDoc());

            try
            {
                CorporateDoc _corporateDoc = this[pKey];
                _corporateDoc.docType = pKey.First;
                _corporateDoc.runTime = pKey.Second;
                _corporateDoc.SetFileName(pFileName, pFIXML_SecurityEchange, pRefNotice);
                _corporateDoc.script = FileBytes;
            }
            catch (KeyNotFoundException) { }
        }
        #endregion SetCorporateDoc
        #region GetCorporateDoc
        public CorporateDoc GetCorporateDoc(Pair<CATools.DOCTypeEnum, CATools.SQLRunTimeEnum> pKey)
        {
            CorporateDoc _corporateDoc = null;
            try { _corporateDoc = this[pKey]; }
            catch (KeyNotFoundException) { }
            return _corporateDoc;
        }
        #endregion GetCorporateDoc
        #region DeleteCorporateDoc
        public void DeleteCorporateDoc(Pair<CATools.DOCTypeEnum, CATools.SQLRunTimeEnum> pKey)
        {
            if (this.ContainsKey(pKey))
                this.Remove(pKey);
        }
        #endregion DeleteCorporateDoc
        #endregion Methods
    }
    #endregion CorporateDocs
    #region CorporateEvent
    /// <summary>
    /// Contient les caractéristiques d'un événement de Corporate action
    /// Group / Type(s) / Date(s) / Procédures
    /// </summary>
    [SerializableAttribute()]
    [XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public class CorporateEvent : SpheresCommonIdentification
    {
        #region Members
        [XmlElementAttribute("group", Order = 1)]
        public CorporateEventGroupEnum group;
        [XmlElementAttribute("type", Order = 2)]
        public CorporateEventTypeEnum @type;
        [XmlIgnoreAttribute()]
        public bool operandSpecified;
        [XmlElementAttribute("operand", Order = 3)]
        public CombinationOperandEnum operand;
        [XmlIgnoreAttribute()]
        public bool combinedTypeSpecified;
        [XmlElementAttribute("combinedType", Order = 4)]
        public CorporateEventTypeEnum combinedType;
        [XmlElementAttribute("mode", Order = 5)]
        public FixML.Enum.SettlSessIDEnum mode;
        [XmlIgnoreAttribute()]
        public bool exDateSpecified;
        [XmlElementAttribute("exDate", Order = 6)]
        public DateTime exDate;
        [XmlIgnoreAttribute()]
        public bool effectiveDateSpecified;
        [XmlElementAttribute("effectiveDate", Order = 7)]
        public DateTime effectiveDate;
        [XmlElementAttribute("execOrder", Order = 8)]
        public int execOrder;
        [XmlElementAttribute("adjMethod", Order = 9)]
        public AdjustmentMethodOfDerivContractEnum adjMethod;
        [XmlIgnoreAttribute()]
        public bool procedureSpecified;
        [XmlElementAttribute("procedure", Order = 10)]
        public CorporateEventProcedure procedure;
        [XmlIgnoreAttribute()]
        public bool contractsSpecified;
        [XmlArrayAttribute("derivativeContract", Order = 11)]
        [XmlArrayItemAttribute("derivativeContracts")]
        public CorporateEventContract[] contracts;
        #endregion Members
        #region Accessors
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public int IdCE
        {
            get { return Convert.ToInt32(spheresid); }
            set { spheresid = value.ToString(); }
        }
        #endregion Accessors
        #region Constructors
        public CorporateEvent() { }
        #endregion Constructors
        #region Methods
        #region GetNormMsgFactoryParameters
        /// <summary>
        /// Création des paramètres du message NORMMSGFACTORY contenant l'ensemble des caractéristiques du
        /// Corporate event (CE, CE Procedure (Sous-jacents, Composants, Règles d'arrondi)
        /// </summary>
        /// <returns>Array de paramètres</returns>
        // EG [33415/33420]
        public MQueueparameter[] GetNormMsgFactoryParameters()
        {
            MQueueparameters parameters = new MQueueparameters();
            #region Paramètres CE
            MQueueparameter parameter = new MQueueparameter("CEGROUP", TypeData.TypeDataEnum.@string);
            parameter.SetValue(group.ToString());
            parameters.Add(parameter);
            parameter = new MQueueparameter("CETYPE", TypeData.TypeDataEnum.@string);
            parameter.SetValue(type.ToString());
            parameters.Add(parameter);

            if (combinedTypeSpecified)
            {
                parameter = new MQueueparameter("CECOMBINEDOPER", TypeData.TypeDataEnum.@string);
                parameter.SetValue(operand.ToString());
                parameters.Add(parameter);
                parameter = new MQueueparameter("CECOMBINEDTYPE", TypeData.TypeDataEnum.@string);
                parameter.SetValue(combinedType.ToString());
                parameters.Add(parameter);
            }

            parameter = new MQueueparameter("CEIDENTIFIER", TypeData.TypeDataEnum.@string);
            parameter.SetValue(identifier);
            parameters.Add(parameter);

            parameter = new MQueueparameter("CEMODE", TypeData.TypeDataEnum.@string);
            parameter.SetValue(mode.ToString());
            parameters.Add(parameter);

            if (exDateSpecified)
            {
                parameter = new MQueueparameter("EXDATE", TypeData.TypeDataEnum.@date);
                parameter.SetValue(exDate);
                parameters.Add(parameter);
            }
            if (effectiveDateSpecified)
            {
                parameter = new MQueueparameter("EFFECTIVEDATE", TypeData.TypeDataEnum.@date);
                parameter.SetValue(effectiveDate);
                parameters.Add(parameter);
            }

            #region Paramètres Procédure
            if (null != procedure)
                parameters.Add(procedure.GetNormMsgFactoryParameters());
            #endregion Paramètres Procédure

            return parameters.parameter;
            #endregion Paramètres CE
        }
        #endregion GetNormMsgFactoryParameters
        #region SetNormMsgFactoryParameters
        /// <summary>
        /// Initialisation des éléments de la classe à l'aide des paramètres du message (de type NORMMSGFACTORY)
        /// </summary>
        /// <param name="pMQueueParameters">Paramètres du message</param>
        /// <param name="pTemplatePath">Nom du fichier template</param>
        // EG [33415/33420]
        // EG 20180426 Analyse du code Correction [CA2202]
        public List<string> SetNormMsgFactoryParameters(MQueueparameters pMQueueParameters, string pTemplatePath)
        {
            List<string> _missingParameters = new List<string>();
            #region CORPORATE EVENT
            MQueueparameter _parameter = pMQueueParameters["CEGROUP"];
            if (null != _parameter)
                group = CATools.CEGroup(_parameter.Value).Value;
            else
                _missingParameters.Add("CEGROUP");

            _parameter = pMQueueParameters["CETYPE"];
            if (null != _parameter)
                type = CATools.CEType(_parameter.Value).Value;
            else
                _missingParameters.Add("CETYPE");

            _parameter = pMQueueParameters["CECOMBINEDOPER"];
            if (null != _parameter)
            {
                Nullable<CombinationOperandEnum> _operand = CATools.CEOperand(_parameter.Value);
                operandSpecified = _operand.HasValue;
                if (_operand.HasValue)
                    operand = _operand.Value;
                else
                    _missingParameters.Add("CECOMBINEDOPER");
            }
            _parameter = pMQueueParameters["CECOMBINEDTYPE"];
            if (null != _parameter)
            {
                Nullable<CorporateEventTypeEnum> _type = CATools.CEType(_parameter.Value);
                combinedTypeSpecified = _type.HasValue;
                if (_type.HasValue)
                    combinedType = _type.Value;
                else
                    _missingParameters.Add("CECOMBINEDOPER");

            }
            _parameter = pMQueueParameters["CEMODE"];
            if (null != _parameter)
                mode = CATools.CEMode(_parameter.Value).Value;
            else
                _missingParameters.Add("CEMODE");

            execOrder = 1;
            _parameter = pMQueueParameters["EXDATE"];
            if (null != _parameter)
            {
                exDateSpecified = true;
                exDate = new EFS_Date(_parameter.Value).DateValue;
            }
            _parameter = pMQueueParameters["EFFECTIVEDATE"];
            if (null != _parameter)
            {
                effectiveDateSpecified = true;
                effectiveDate = new EFS_Date(_parameter.Value).DateValue;
            }
            _parameter = pMQueueParameters["CEIDENTIFIER"];
            if (null != _parameter)
            {
                identifierSpecified = true;
                identifier = _parameter.Value;
            }
            else
                _missingParameters.Add("CEIDENTIFIER");

            _parameter = pMQueueParameters["ADJMETHOD"];
            if (null != _parameter)
                adjMethod = CATools.CEMethod(_parameter.Value).Value;
            else
                _missingParameters.Add("ADJMETHOD");

            _parameter = pMQueueParameters["ADJTEMPLATEFILENAME"];
            if (null != _parameter)
            {
                #region CORPORATE EVENT PROCEDURE
                string fileName = SystemIOTools.AddFileNameSuffixe(_parameter.Value, ".xml");
                FileInfo fileInfo = new FileInfo(pTemplatePath + fileName);
                if (null != fileInfo)
                {
                    using (FileStream XMLTemplate = FileTools.OpenFile(fileInfo, FileMode.Open, FileAccess.Read))
                    {
                        procedure = SerializationHelper.LoadObjectFromFileStream<CorporateEventProcedure>(XMLTemplate);
                        procedureSpecified = true;
                        procedure.adjustment.templateFileName = _parameter.Value;
                        _missingParameters.AddRange(procedure.SetNormMsgFactoryParameters(pMQueueParameters, pTemplatePath));
                    }
                }
                #endregion CORPORATE EVENT PROCEDURE
            }
            else
            {
                _missingParameters.Add("ADJTEMPLATEFILENAME");
            }

            return _missingParameters;
            #endregion CORPORATE EVENT
        }
        #endregion SetNormMsgFactoryParameters

        #region SetCorporateEventEmbedded
        /// <summary>
        /// Initialisation des éléments de la classe à l'aide des paramètres d'une Corporate action intégrée
        /// </summary>
        /// <param name="pRow">DataRow du CE</param>
        // EG [33415/33420]
        // EG 20210318 [XXXXXX] Set procedureSpecified
        public void SetCorporateEventEmbedded(DataRow pRow)
        {
            spheresid = pRow["IDCE"].ToString();
            identifierSpecified = (false == Convert.IsDBNull(pRow["CEIDENTIFIER"]));
            if (identifierSpecified)
                identifier = pRow["CEIDENTIFIER"].ToString();

            mode = CATools.CEMode(pRow["REQUESTMODE"].ToString()).Value;
            group = CATools.CEGroup(pRow["CEGROUP"].ToString()).Value;
            type = CATools.CEType(pRow["CETYPE"].ToString()).Value;
            operandSpecified = (false == Convert.IsDBNull(pRow["CECOMBINEDOPER"]));
            if (operandSpecified)
                operand = CATools.CEOperand(pRow["CECOMBINEDOPER"].ToString()).Value;
            combinedTypeSpecified = (false == Convert.IsDBNull(pRow["CECOMBINEDTYPE"]));
            if (combinedTypeSpecified)
                combinedType = CATools.CEType(pRow["CECOMBINEDTYPE"].ToString()).Value;

            adjMethod = CATools.CEMethod(pRow["ADJMETHOD"].ToString()).Value;

            execOrder = Convert.ToInt32(pRow["EXECORDER"]);

            exDateSpecified = (false == Convert.IsDBNull(pRow["EXDATE"]));
            if (exDateSpecified)
                exDate = Convert.ToDateTime(pRow["EXDATE"]);

            effectiveDateSpecified = (false == Convert.IsDBNull(pRow["EFFECTIVEDATE"]));
            if (effectiveDateSpecified)
                effectiveDate = Convert.ToDateTime(pRow["EFFECTIVEDATE"]);

            procedureSpecified = (false == Convert.IsDBNull(pRow["ADJPROCEDURES"]));
            EFS_SerializeInfoBase serializeInfo = new EFS_SerializeInfoBase(typeof(CorporateEventProcedure), pRow["ADJPROCEDURES"].ToString());
            procedure = (CorporateEventProcedure)CacheSerializer.Deserialize(serializeInfo);
        }
        #endregion SetCorporateEventEmbedded
        #endregion Methods

        #region Indexors
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public CorporateEventContract this[int pIdDC]
        {
            get
            {
                CorporateEventContract _contract = null;
                if (contractsSpecified)
                {
                    foreach (CorporateEventContract item in contracts)
                    {
                        if (item.idDC == pIdDC)
                        {
                            _contract = item;
                            break;
                        }
                    }
                }
                return _contract;
            }
        }
        #endregion Indexors

    }
    #endregion CorporateEvent
    #region CorporateEventProcedure
    /// <summary>
    /// Contient les caractéristiques des procédures d'ajustement d'un événement de Corporate action
    /// Sous-jacents concernés.
    /// Procédures d'ajustement (NoAdjustment, Ratio, Future, Option)
    /// </summary>
    [SerializableAttribute()]
    [XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    [XmlRootAttribute("procedures", Namespace = "http://www.efs.org/2007/EFSmL-3-0", IsNullable = false)]
    // EG 20140317 [19722]
    public class CorporateEventProcedure
    {
        #region Members
        [XmlArrayAttribute("underlyers", Order = 1)]
        [XmlArrayItemAttribute("underlyer")]
        public CorporateEventUnderlyer[] underlyers;
        [XmlElementAttribute("noAdjustment", typeof(AdjustmentNone), Order = 2)]
        [XmlElementAttribute("ratio", typeof(AdjustmentRatio), Order = 2)]
        [XmlElementAttribute("fairValue", typeof(AdjustmentFairValue), Order = 2)]
        [XmlElementAttribute("package", typeof(AdjustmentPackage), Order = 2)]
        public Adjustment adjustment;
        #endregion Members
        #region Constructors
        public CorporateEventProcedure()
        {
        }
        #endregion Constructors
        #region Methods
        public void AddUnderlyer(CorporateEventUnderlyer pUnderlyer)
        {
            ArrayList aUnderlyers = new ArrayList(); 
            aUnderlyers.AddRange(underlyers);
            aUnderlyers.Add(pUnderlyer);
            underlyers = (CorporateEventUnderlyer[])aUnderlyers.ToArray(typeof(CorporateEventUnderlyer));
        }
        // EG [33415/33420] Nouveau type résultat de composant (info = string)
        // EG 20140317 [19722] Nouveau type résultat de composant (check = bool)
        // EG 20141106 [20253] Equalisation payment
        public MQueueparameter[] GetNormMsgFactoryParameters()
        {
            MQueueparameters parameters = new MQueueparameters();
            MQueueparameter parameter = null;

            if (null != adjustment)
            {
                #region Paramètres SOUS-JACENTs
                string suffix = string.Empty;
                int i = 1;
                foreach (CorporateEventUnderlyer underlyer in underlyers)
                {
                    parameter = new MQueueparameter("UNLCATEGORY" + suffix, TypeData.TypeDataEnum.@string);
                    parameter.SetValue(underlyer.category.ToString());
                    parameters.Add(parameter);

                    if (underlyer.marketSpecified)
                    {
                        parameter = new MQueueparameter("UNLMARKET" + suffix, TypeData.TypeDataEnum.@string);
                        parameter.SetValue(underlyer.market.ISO10383_ALPHA4);
                        parameters.Add(parameter);
                    }

                    if (underlyer.caIssueCodeSpecified)
                    {
                        parameter = new MQueueparameter("UNLCODE" + suffix, TypeData.TypeDataEnum.@string);
                        parameter.SetValue(underlyer.caIssueCode);
                        parameters.Add(parameter);
                    }
                    i++;
                    suffix = i.ToString();
                }
                #endregion Paramètres SOUS-JACENTs

                #region PARAMETRES D'AJUSTEMENT
                parameter = new MQueueparameter("ADJMETHOD", TypeData.TypeDataEnum.@string);
                parameter.SetValue(adjustment.method.ToString());
                parameters.Add(parameter);

                if (StrFunc.IsFilled(adjustment.templateFileName))
                {
                    parameter = new MQueueparameter("ADJTEMPLATEFILENAME", TypeData.TypeDataEnum.@string);
                    parameter.SetValue(adjustment.templateFileName);
                    parameters.Add(parameter);
                }

                if (StrFunc.IsFilled(adjustment.templateFileName_AI))
                {
                    parameter = new MQueueparameter("TEMPLATEFILENAME_AI", TypeData.TypeDataEnum.@string);
                    parameter.SetValue(adjustment.templateFileName_AI);
                    parameters.Add(parameter);
                }

                if (StrFunc.IsFilled(adjustment.templateFileName_CT))
                {
                    parameter = new MQueueparameter("TEMPLATEFILENAME_CT", TypeData.TypeDataEnum.@string);
                    parameter.SetValue(adjustment.templateFileName_CT);
                    parameters.Add(parameter);
                }

                #region Mise à jour des COMPOSANTS SIMPLES
                List<ComponentSimple> _list = CATools.AllComponentSimples(this);
                if (this.adjustment.additionalInfoSpecified)
                    _list.AddRange(CATools.AllComponentSimples(this.adjustment.additionalInfo));
                _list.ForEach(item =>
                {
                    if (item.resultSpecified)
                    {
                        parameter = new MQueueparameter(item.Id, item.name, item.description, TypeData.TypeDataEnum.@decimal);
                        switch (item.result.itemsElementName)
                        {
                            case ResultType.unit:
                                SimpleUnit _simpleUnit = (SimpleUnit)item.result.result;
                                parameter.SetValue(_simpleUnit.value.DecValue.ToString());
                                break;
                            case ResultType.amount:
                                Money _money = (Money)item.result.result;
                                parameter.SetValue(_money.amount.DecValue.ToString());
                                parameter.ExValueSpecified = true;
                                parameter.ExValue = _money.currency.value;
                                break;
                            // EG [33415/33420]
                            case ResultType.info:
                                parameter.dataType = TypeData.TypeDataEnum.@string;
                                parameter.SetValue((string)item.result.result);
                                break;
                            // EG 20140317 [19722]
                            case ResultType.check:
                                parameter.dataType = TypeData.TypeDataEnum.@bool;
                                parameter.SetValue((bool)item.result.result);
                                break;

                        }
                        parameters.Add(parameter);
                    }
                });

                if (adjustment.method == AdjustmentMethodOfDerivContractEnum.Ratio)
                {
                    #region Ratio Method
                    #region Nouvelle spécification d'arrondi pour RFACTOR
                    AdjustmentRatio ratio = (AdjustmentRatio)adjustment;
                    if (ratio.rFactor.roundingSpecified)
                    {
                        parameter = new MQueueparameter("RFACTORROUNDING", TypeData.TypeDataEnum.integer);
                        parameter.SetValue(ratio.rFactor.rounding.precision);
                        parameter.ExValue = ratio.rFactor.rounding.direction.ToString();
                        parameter.ExValueSpecified = true;
                        parameters.Add(parameter);
                    }
                    #endregion Nouvelle spécification d'arrondi pour RFACTOR
                    #region RFACTOR Certifié
                    if (ratio.rFactor.rFactorCertifiedSpecified)
                    {
                        parameter = parameters[CATools.RFactorCertified_Id];
                        if (null == parameter)
                        {
                            parameter = new MQueueparameter(CATools.RFactorCertified_Id,
                                CATools.RFactorCertified_Name, CATools.RFactorCertified_Description, TypeData.TypeDataEnum.@decimal);
                            parameters.Add(parameter);
                        }
                        SimpleUnit _simpleUnit = ratio.rFactor.rFactorCertified.result.result as SimpleUnit;
                        parameter.SetValue(_simpleUnit.valueRounded.DecValue.ToString());
                    }
                    #endregion RFACTOR Certifié

                    #region Ajustement des élements
                    if (false == ratio.contract.futureSpecified)
                    {
                        parameter = new MQueueparameter("ADJFUTURE", TypeData.TypeDataEnum.@bool);
                        parameter.SetValue(ratio.contract.futureSpecified);
                        parameters.Add(parameter);
                    }
                    else if (false == ratio.contract.future.contractSizeSpecified)
                    {
                        parameter = new MQueueparameter("ADJFUTURECSIZE", TypeData.TypeDataEnum.@bool);
                        parameter.SetValue(ratio.contract.future.contractSizeSpecified);
                        parameters.Add(parameter);
                    }
                    else if (false == ratio.contract.future.priceSpecified)
                    {
                        parameter = new MQueueparameter("ADJFUTUREPRICE", TypeData.TypeDataEnum.@bool);
                        parameter.SetValue(ratio.contract.future.priceSpecified);
                        parameters.Add(parameter);
                    }

                    if (false == ratio.contract.optionSpecified)
                    {
                        parameter = new MQueueparameter("ADJOPTION", TypeData.TypeDataEnum.@bool);
                        parameter.SetValue(ratio.contract.optionSpecified);
                        parameters.Add(parameter);
                    }
                    else if (false == ratio.contract.option.contractSizeSpecified)
                    {
                        parameter = new MQueueparameter("ADJOPTIONCSIZE", TypeData.TypeDataEnum.@bool);
                        parameter.SetValue(ratio.contract.option.contractSizeSpecified);
                        parameters.Add(parameter);
                    }
                    else if (false == ratio.contract.option.strikePriceSpecified)
                    {
                        parameter = new MQueueparameter("ADJOPTIONSTRIKEPRICE", TypeData.TypeDataEnum.@bool);
                        parameter.SetValue(ratio.contract.option.strikePriceSpecified);
                        parameters.Add(parameter);
                    }
                    else if (false == ratio.contract.option.priceSpecified)
                    {
                        parameter = new MQueueparameter("ADJOPTIONPRICE", TypeData.TypeDataEnum.@bool);
                        parameter.SetValue(ratio.contract.option.priceSpecified);
                        parameters.Add(parameter);
                    }
                    #endregion Ajustement des élements

                    #region Nouvelle spécification d'arrondi pour CONTRACTSIZE
                    if (ratio.contract.futureSpecified && ratio.contract.future.contractSizeSpecified && ratio.contract.future.contractSize.roundingSpecified)
                    {
                        AdjustmentContractSize contractSize = ratio.contract.future.contractSize;
                        parameter = new MQueueparameter("CONTRACTSIZEROUNDING", TypeData.TypeDataEnum.integer);
                        parameter.SetValue(contractSize.rounding.precision);
                        parameter.ExValue = contractSize.rounding.direction.ToString();
                        parameter.ExValueSpecified = true;
                        parameters.Add(parameter);
                    }
                    else if (ratio.contract.optionSpecified && ratio.contract.option.contractSizeSpecified && ratio.contract.option.contractSize.roundingSpecified)
                    {
                        AdjustmentContractSize contractSize = ratio.contract.option.contractSize;
                        parameter = new MQueueparameter("CONTRACTSIZEROUNDING", TypeData.TypeDataEnum.integer);
                        parameter.SetValue(contractSize.rounding.precision);
                        parameter.ExValue = contractSize.rounding.direction.ToString();
                        parameter.ExValueSpecified = true;
                        parameters.Add(parameter);
                    }
                    #endregion Nouvelle spécification d'arrondi pour CONTRACTSIZE
                    #region Nouvelle spécification d'arrondi pour CONTRACTMULTIPLIER
                    if (ratio.contract.futureSpecified && ratio.contract.future.contractMultiplierSpecified && ratio.contract.future.contractMultiplier.roundingSpecified)
                    {
                        AdjustmentContractMultiplier contractMultiplier = ratio.contract.future.contractMultiplier;
                        parameter = new MQueueparameter("CONTRACTMULTIPLIERROUNDING", TypeData.TypeDataEnum.integer);
                        parameter.SetValue(contractMultiplier.rounding.precision);
                        parameter.ExValue = contractMultiplier.rounding.direction.ToString();
                        parameter.ExValueSpecified = true;
                        parameters.Add(parameter);
                    }
                    else if (ratio.contract.optionSpecified && ratio.contract.option.contractMultiplierSpecified && ratio.contract.option.contractMultiplier.roundingSpecified)
                    {
                        // EG 20160222 Change future to option
                        AdjustmentContractMultiplier contractMultiplier = ratio.contract.option.contractMultiplier;
                        parameter = new MQueueparameter("CONTRACTMULTIPLIERROUNDING", TypeData.TypeDataEnum.integer);
                        parameter.SetValue(contractMultiplier.rounding.precision);
                        parameter.ExValue = contractMultiplier.rounding.direction.ToString();
                        parameter.ExValueSpecified = true;
                        parameters.Add(parameter);
                    }
                    #endregion Nouvelle spécification d'arrondi pour CONTRACTMULTIPLIER
                    #region Nouvelle spécification d'arrondi pour PRICE
                    if (ratio.contract.futureSpecified && ratio.contract.future.priceSpecified && ratio.contract.future.price.roundingSpecified)
                    {
                        AdjustmentPrice price = ratio.contract.future.price;
                        parameter = new MQueueparameter("PRICEROUNDING", TypeData.TypeDataEnum.integer);
                        parameter.SetValue(price.rounding.precision);
                        parameter.ExValue = price.rounding.direction.ToString();
                        parameter.ExValueSpecified = true;
                        parameters.Add(parameter);
                    }
                    #endregion Nouvelle spécification d'arrondi pour PRICE
                    #region SOULTE FUTURE
                    if (ratio.contract.futureSpecified && ratio.contract.future.equalisationPaymentSpecified)
                    {
                        parameter = new MQueueparameter("ADJFUTUREEQPAYMENT", TypeData.TypeDataEnum.@bool);
                        parameter.SetValue(true);
                        parameters.Add(parameter);
                        #region Nouvelle spécification d'arrondi pour SOULTE
                        EqualisationPayment eqPayment = ratio.contract.future.equalisationPayment;
                        if (eqPayment.roundingSpecified)
                        {
                            parameter = new MQueueparameter("EQPAYMENTROUNDING", TypeData.TypeDataEnum.integer);
                            parameter.SetValue(eqPayment.rounding.precision);
                            parameter.ExValue = eqPayment.rounding.direction.ToString();
                            parameter.ExValueSpecified = true;
                            parameters.Add(parameter);
                        }
                        #endregion Nouvelle spécification d'arrondi pour SOULTE
                    }
                    #endregion SOULTE FUTURE

                    #region Nouvelle spécification d'arrondi pour STRIKEPRICE
                    if (ratio.contract.optionSpecified && ratio.contract.option.strikePriceSpecified && ratio.contract.option.strikePrice.roundingSpecified)
                    {
                        AdjustmentStrikePrice strikePrice = ratio.contract.option.strikePrice;
                        parameter = new MQueueparameter("STRIKEPRICEROUNDING", TypeData.TypeDataEnum.integer);
                        parameter.SetValue(strikePrice.rounding.precision);
                        parameter.ExValue = strikePrice.rounding.direction.ToString();
                        parameter.ExValueSpecified = true;
                        parameters.Add(parameter);
                    }
                    #endregion Nouvelle spécification d'arrondi pour STRIKEPRICE
                    #region SOULTE OPTION
                    if (ratio.contract.optionSpecified && ratio.contract.option.equalisationPaymentSpecified)
                    {
                        parameter = new MQueueparameter("ADJOPTIONEQPAYMENT", TypeData.TypeDataEnum.@bool);
                        parameter.SetValue(true);
                        parameters.Add(parameter);
                        #region Nouvelle spécification d'arrondi pour SOULTE
                        EqualisationPayment eqPayment = ratio.contract.option.equalisationPayment;
                        if (eqPayment.roundingSpecified)
                        {
                            parameter = new MQueueparameter("EQPAYMENTROUNDING", TypeData.TypeDataEnum.integer);
                            parameter.SetValue(eqPayment.rounding.precision);
                            parameter.ExValue = eqPayment.rounding.direction.ToString();
                            parameter.ExValueSpecified = true;
                            parameters.Add(parameter);
                        }
                        #endregion Nouvelle spécification d'arrondi pour SOULTE
                    }
                    #endregion SOULTE OPTION
                    #endregion Ratio Method
                }
                else if (adjustment is AdjustmentFairValue)
                {
                    // TBD : A développer
                }
                else if (adjustment is AdjustmentPackage)
                {
                    // TBD : A développer
                }
                else if (adjustment is AdjustmentNone)
                {
                    // NOTHING TO DO
                }
                #endregion Mise à jour des COMPOSANTS SIMPLES
                #endregion PARAMETRES D'AJUSTEMENT
            }
            return parameters.parameter;
        }
        // EG [33415/33420] Gestion AdjustmentNone + Nouveau type résultat de composant (info = string)
        public List<string> SetNormMsgFactoryParameters(MQueueparameters pMQueueParameters, string pTemplatePath)
        {
            List<string> _missingParameters = new List<string>();

            adjustment.SetAdditionalInfos(pMQueueParameters, pTemplatePath);
            adjustment.SetAdjustmentContract(pMQueueParameters, pTemplatePath);
            #region MAJ DES SOUS-JACENTS
            MQueueparameter _parameter = pMQueueParameters["UNLCATEGORY"];
            if (null != _parameter)
                _missingParameters.AddRange(underlyers[0].SetNormMsgFactoryParameters(pMQueueParameters));
            else
                _missingParameters.Add("UNLCATEGORY");

            _parameter = pMQueueParameters["UNLCATEGORY2"];
            if (null != _parameter)
            {
                if (1 == underlyers.Length)
                {
                    CorporateEventUnderlyer _underlyer = new CorporateEventUnderlyer();
                    _missingParameters.AddRange(_underlyer.SetNormMsgFactoryParameters(pMQueueParameters, "2"));
                    AddUnderlyer(_underlyer);
                }
                else
                    _missingParameters.AddRange(underlyers[1].SetNormMsgFactoryParameters(pMQueueParameters, "2"));
            }
            #endregion MAJ DES SOUS-JACENTS
            #region AJUSTEMENT DES ELEMENTS
            if (this.adjustment is AdjustmentRatio)
            {
                AdjustmentRatio ratio = adjustment as AdjustmentRatio;
                ratio.SetNormMsgFactoryParameters(pMQueueParameters);

            }
            else if (this.adjustment is AdjustmentNone)
            {
                AdjustmentNone none = adjustment as AdjustmentNone;
                none.SetNormMsgFactoryParameters(pMQueueParameters);

            }
            #endregion AJUSTEMENT DES ELEMENTS
            #region MAJ DES VALEURS des COMPOSANTS SIMPLES
            foreach (MQueueparameter item in pMQueueParameters.parameter)
            {
                if (ReflectionTools.GetObjectById(this, item.id) is ComponentSimple component)
                {
                    component.resultSpecified = StrFunc.IsFilled(item.Value);
                    if (component.resultSpecified)
                    {
                        if (null == component.result)
                        {
                            component.result = new Result { itemsElementName = component.resulttype };
                        }
                        switch (component.result.itemsElementName)
                        {
                            case ResultType.amount:
                                component.result.result = new Money(item.Value, item.ExValue);
                                break;
                            case ResultType.unit:
                                component.result.result = new SimpleUnit(item.Value);
                                break;
                            case ResultType.info:
                                component.result.result = item.Value;
                                break;
                            case ResultType.check:
                                component.result.result = Convert.ToBoolean(item.Value);
                                break;
                        }
                    }
                }
            }
            #endregion MAJ DES VALEURS des COMPOSANTS SIMPLES
            #region MAJ DU RFACTOR CERTIFIE
            if (adjustment is AdjustmentRatio)
            {
                AdjustmentRatio ratio = (AdjustmentRatio)adjustment;
                _parameter = pMQueueParameters[CATools.RFactorCertified_Id];
                ratio.rFactor.rFactorCertifiedSpecified = (null != _parameter);
                if (ratio.rFactor.rFactorCertifiedSpecified)
                {
                    ratio.rFactor.rFactorCertified = new ComponentSimple();
                    ComponentSimple _rFactorCertified = ratio.rFactor.rFactorCertified;
                    _rFactorCertified.Id = _parameter.id;
                    _rFactorCertified.name = _parameter.name;
                    _rFactorCertified.resultSpecified = true;
                    _rFactorCertified.result = new Result
                    {
                        itemsElementName = ResultType.unit,
                        result = new SimpleUnit(_parameter.Value, _parameter.Value)
                    };
                }
            }
            #endregion MAJ DU RFACTOR CERTIFIE
            return _missingParameters;
        }
        #endregion Methods

    }
    #endregion CorporateEventProcedure
    #region CorporateEventUnderlyer
    /// <summary>
    /// Sous-jacent concerné par l'événement
    /// </summary>
    [SerializableAttribute()]
    [XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    [XmlRootAttribute("underlyer", Namespace = "http://www.efs.org/2007/EFSmL-3-0", IsNullable = false)]
    public class CorporateEventUnderlyer : SpheresCommonIdentification
    {
        #region Members
        [XmlElementAttribute("category", Order = 1)]
        public Cst.UnderlyingAsset_ETD category;
        [XmlIgnoreAttribute()]
        public bool marketSpecified;
        [XmlElementAttribute("market", Order = 2)]
        public MarketIdentification market;
        [XmlIgnoreAttribute()]
        public bool isinCodeSpecified;
        [XmlElementAttribute("isinCode", Order = 3)]
        public string isinCode;
        [XmlIgnoreAttribute()]
        public bool caIssueCodeSpecified;
        [XmlElementAttribute("caIssueCode", Order = 4)]
        public string caIssueCode;
        [XmlIgnoreAttribute()]
        public bool componentSpecified;
        [XmlElementAttribute("component", typeof(ComponentSimple), Order = 5)]
        [XmlElementAttribute("componentFormula", typeof(ComponentFormula), Order = 5)]
        [XmlElementAttribute("componentMethod", typeof(ComponentMethod), Order = 5)]
        [XmlElementAttribute("componentProperty", typeof(ComponentProperty), Order = 5)]
        [XmlElementAttribute("componentReference", typeof(ComponentReference), Order = 5)]
        public ComponentBase[] component;
        [XmlIgnoreAttribute()]
        protected CAWorkingDataContainer m_WorkingDataContainer;
        #endregion Members
        #region Accessors
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public int SpheresID
        {
            get { return Convert.ToInt32(spheresid); }
            set { spheresid = value.ToString(); }
        }
        [XmlIgnoreAttribute()]
        public CAWorkingDataContainer WorkingDataContainer
        {
            get { return m_WorkingDataContainer; }
            set { m_WorkingDataContainer = value; }
        }

        #region UnderlyerCode
        // EG 20141024 Détermination de la clé de recherche du sous-jacent
        // Le code d'identification du sous-jacent est par défaut le code ISIN mais il peut être l'identifiant : 
        // UNLCODE = 'identifier=XXXXXXXXXXUS38259P7069;isin=US38259P7069;nsin=38259P508;ric=GOOGL.OQ;bbg=GOOG:US'
        [XmlIgnoreAttribute()]
        public List<Pair<SQL_TableWithID.IDType, string>> UnderlyerCode
        {
            get
            {
                List<Pair<SQL_TableWithID.IDType, string>> _lstUnderlyerCode = new List<Pair<SQL_TableWithID.IDType, string>>();
                string[] sep = new string[] { ";" };
                string[] _expr = caIssueCode.Split(sep, StringSplitOptions.RemoveEmptyEntries);

                // _expr[0]  = isin OU Qualification du code
                // _expr[1]  = Valeur du code qualifié

                if (ArrFunc.IsFilled(_expr))
                {
                    sep = new string[] { "=" };
                    foreach (string _subExpr in _expr)
                    {
                        string[] _expr2 = _subExpr.Split(sep, StringSplitOptions.RemoveEmptyEntries);
                        Pair<SQL_TableWithID.IDType, string> _underlyerCode;
                        if (1 == _expr2.Length)
                        {
                            // C'est obligatoirement un code ISIN
                            _underlyerCode = new Pair<SQL_TableWithID.IDType, string>(SQL_TableWithID.IDType.IsinCode, _expr2[0]);
                        }
                        else
                        {
                            // Ce sont des autres codes
                            switch (_expr2[0])
                            {
                                case "identifier":
                                    _underlyerCode = new Pair<SQL_TableWithID.IDType, string>(SQL_TableWithID.IDType.Identifier, _expr2[1]);
                                    break;
                                case "isin":
                                    _underlyerCode = new Pair<SQL_TableWithID.IDType, string>(SQL_TableWithID.IDType.IsinCode, _expr2[1]);
                                    break;
                                case "nsin":
                                    _underlyerCode = new Pair<SQL_TableWithID.IDType, string>(SQL_TableWithID.IDType.NSINCode, _expr2[1]);
                                    break;
                                case "ric":
                                    _underlyerCode = new Pair<SQL_TableWithID.IDType, string>(SQL_TableWithID.IDType.RICCode, _expr2[1]);
                                    break;
                                case "bbg":
                                    _underlyerCode = new Pair<SQL_TableWithID.IDType, string>(SQL_TableWithID.IDType.BBGCode, _expr2[1]);
                                    break;
                                case "symbol":
                                    _underlyerCode = new Pair<SQL_TableWithID.IDType, string>(SQL_TableWithID.IDType.AssetSymbol, _expr2[1]);
                                    break;
                                default:
                                    _underlyerCode = new Pair<SQL_TableWithID.IDType, string>(SQL_TableWithID.IDType.UNDEFINED, null);
                                    break;
                            }
                        }
                        if (null != _underlyerCode)
                            _lstUnderlyerCode.Add(_underlyerCode);
                    }
                }
                return _lstUnderlyerCode;
            }
        }
        #endregion UnderlyerCode
        #endregion Accessors
        #region Constructors
        public CorporateEventUnderlyer()
        {
        }
        public List<string> SetNormMsgFactoryParameters(MQueueparameters pMQueueParameters )
        {
            return SetNormMsgFactoryParameters(pMQueueParameters, string.Empty);
        }
        public List<string> SetNormMsgFactoryParameters(MQueueparameters pMQueueParameters, string pSuffix)
        {
            List<string> _missingParameters = new List<string>();
            MQueueparameter _parameter = pMQueueParameters["UNLCATEGORY" + pSuffix];
            if (null != _parameter)
            {
                category = (Cst.UnderlyingAsset_ETD)System.Enum.Parse(typeof(Cst.UnderlyingAsset_ETD), _parameter.Value, false);

                _parameter = pMQueueParameters["UNLMARKET" + pSuffix];
                if (null != _parameter)
                {
                    marketSpecified = true;
                    market = new MarketIdentification();
                    if (null != pMQueueParameters["MARKETTYPE"])
                    {
                        market.ISO10383_ALPHA4Specified = true;
                        market.ISO10383_ALPHA4 = _parameter.Value;
                    }
                    else
                    {
                        market.FIXML_SecurityExchangeSpecified = true;
                        market.FIXML_SecurityExchange = _parameter.Value;
                    }
                }
                else
                    _missingParameters.Add("UNLMARKET");


                _parameter = pMQueueParameters["UNLIDENTIFIER" + pSuffix];
                if (null != _parameter)
                {
                    identifierSpecified = true;
                    identifier = _parameter.Value;
                }
                _parameter = pMQueueParameters["UNLCODE" + pSuffix];
                if (null != _parameter)
                {
                    caIssueCodeSpecified = StrFunc.IsFilled(_parameter.Value);
                    if (caIssueCodeSpecified)
                        caIssueCode = _parameter.Value;
                }
                if (false == caIssueCodeSpecified)
                    _missingParameters.Add("UNLCODE");
            }
            else
                _missingParameters.Add("UNLCATEGORY");

            return _missingParameters;
        }
        #endregion Constructors

        #region Methods
        #region CumPriceTradingDay
        /// <summary>
        /// Retourne le prix de clôture (CUM). 
        /// Méthode invoquée par réflection par l'automate de calcul des formules
        /// </summary>
        /// <returns>Résultat</returns>
        /// EG 20130528 Change Key int to Pair(Cst.UnderlyingAsset, int)
        // EG 20190114 Add detail to ProcessLog Refactoring
        // EG 20190716 [VCL : New FixedIncome] Add (KeyQuoteAdditional parameter)
        // EG 20231129 [WI762] End of Day processing : Possibility to request processing without initial margin (IsEODRequest)
        public Nullable<decimal> CumPriceTradingDay()
        {
            bool isEOD = m_WorkingDataContainer.IsEODRequest; 
            Nullable<decimal> ret = null;
            SystemMSGInfo errReadOfficialClose = null;
            IPosKeepingMarket _entityMarketInfo = m_WorkingDataContainer.EntityMarketInfo;
            Cst.UnderlyingAsset _category = (Cst.UnderlyingAsset)ReflectionTools.EnumParse(new Cst.UnderlyingAsset(), this.category.ToString());
            DateTime _cumDate = m_WorkingDataContainer.ProcessCacheContainer.CACumDate;
            Quote quote = m_WorkingDataContainer.ProcessCacheContainer.GetQuote(SpheresID, _cumDate,
                this.identifier, QuotationSideEnum.OfficialClose, _category, new KeyQuoteAdditional(), ref errReadOfficialClose);
            if ((null != quote) && quote.valueSpecified)
                ret = quote.value;
            else
            {
                if (null != errReadOfficialClose)
                {
                    // EG 201411 [20484]
                    //m_WorkingDataContainer.ProcessCacheContainer.LogWithData.Invoke(isEOD ? ProcessStateTools.StatusWarningEnum : errReadOfficialClose.processState.Status,
                    //    LogLevel.DEFAULT, 2, errReadOfficialClose.identifier, false, null, errReadOfficialClose.datas);
                    // FI 20200623 [XXXXX] call SetErrorWarning
                    m_WorkingDataContainer.ProcessCacheContainer.SetErrorWarning(ProcessStateTools.StatusWarningEnum);
                    

                    
                    Logger.Log(new LoggerData(LogLevelEnum.Warning, errReadOfficialClose.SysMsgCode, 2, errReadOfficialClose.LogParamDatas));
                }
            }
            return ret;
        }
        #endregion CumPriceTradingDay
        #endregion Methods
    }
    #endregion CorporateEventUnderlyer
    #region CorporateEventContract
    /// <summary>
    /// DC concerné par l'événement
    /// </summary>
    [SerializableAttribute()]
    [XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    [XmlRootAttribute("derivativeContract", Namespace = "http://www.efs.org/2007/EFSmL-3-0", IsNullable = false)]
    // EG 20150114 [20676] Add FutureValuationMethodEnum
    public class CorporateEventContract : ContractIdentification
    {
        #region Members
        [XmlElementAttribute("idCE", Order = 1)]
        public int idCE;
        [XmlElementAttribute("idA_Entity", Order = 2)]
        public int idA_Entity;
        [XmlElementAttribute("idAsset_UNL", Order = 3)]
        public Cst.UnderlyingAsset_ETD assetCategory_UNL;
        [XmlElementAttribute("idAsset_UNL", Order = 4)]
        public int idAsset_UNL;

        [XmlElementAttribute("idDC", Order = 5)]
        public int idDC;

        [XmlElementAttribute("category", Order = 6)]
        public CfiCodeCategoryEnum category;
        [XmlElementAttribute("readyState", Order = 7)]
        public CorporateEventReadyStateEnum readyState;
        [XmlElementAttribute("adjStatus", Order = 8)]
        public CATools.AdjStatusEnum adjStatus;
        [XmlElementAttribute("adjMethod", Order = 9)]
        public AdjustmentMethodOfDerivContractEnum adjMethod;
        [XmlIgnoreAttribute()]
        public bool rFactorSpecified;
        [XmlElementAttribute("rFactor", Order = 10)]
        public SimpleUnit rFactor;

        [XmlElementAttribute("cumData", Order = 11)]
        public CalculationCumData cumData;
        [XmlIgnoreAttribute()]
        public bool exDataSpecified;
        [XmlElementAttribute("exData", Order = 12)]
        public CalculationExData exData;
        [XmlIgnoreAttribute()]
        public bool dAttribsSpecified;
        [XmlArrayAttribute("dAttribs", Order = 13)]
        [XmlArrayItemAttribute("dAttrib")]
        public CorporateEventDAttrib[] dAttribs;

        [XmlElementAttribute("idDCEX", Order = 14)]
        public int idDCEX;
        [XmlElementAttribute("idDCEXADJ", Order = 15)]
        public Nullable<int> idDCEXADJ;

        [XmlElementAttribute("idDDCRecycled", Order = 16)]
        public Nullable<int> idDDCRecycled;

        [XmlIgnoreAttribute()]
        public bool priceDecLocatorSpecified;
        [XmlElementAttribute("priceDecLocator", Order = 17)]
        public EFS_Integer priceDecLocator;
        [XmlIgnoreAttribute()]
        public bool strikeDecLocatorSpecified;
        [XmlElementAttribute("strikeDecLocator", Order = 18)]
        public EFS_Integer strikeDecLocator;

        [XmlIgnoreAttribute()]
        public decimal rFactorRetained;

        [XmlIgnoreAttribute()]
        public DataRowState rowState;

        // EG 20150114 [20676] 
        [XmlIgnoreAttribute()]
        public FuturesValuationMethodEnum futValuationMethod;

        // EG 20190121 [23249] New pour Application arrondi sur EQP avant|après application de la quantité en position sur le trade
        [XmlIgnoreAttribute()]
        public CashFlowCalculationMethodEnum cashFlowCalcMethod;

        #endregion Members
        #region Accessors
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public int IdCEC
        {
            get { return Convert.ToInt32(spheresid); }
            set { spheresid = value.ToString(); }
        }
        #endregion Accessors
        #region Methods
        /// <summary>
        /// Mise à jour du résultat du ratio
        /// </summary>
        /// <param name="pRatio"></param>
        public void SetRatio(AdjustmentRatio pRatio, Cst.ErrLevel pErrLevel)
        {
            Nullable<decimal> _rFactor = pRatio.rFactor.component[0].Result;
            Nullable<decimal> _rFactorRounded = pRatio.rFactor.component[0].ResultRounded;
            rFactorSpecified = _rFactor.HasValue && _rFactorRounded.HasValue;
            if (rFactorSpecified)
                rFactor = new SimpleUnit(_rFactor.Value, _rFactorRounded.Value);
            CATools.SetAdjStatus(pRatio.rFactor, this, pErrLevel);
        }
        /// <summary>
        /// Mise à jour de IDCEC
        /// </summary>
        /// <param name="pRatio"></param>
        public void SetID(IDbTransaction pDbTransaction)
        {
            DCQueryEMBEDDED dcQry = new DCQueryEMBEDDED(pDbTransaction.Connection.ConnectionString);
            QueryParameters dcQryParameters = dcQry.GetQuerySelect(CATools.DCWhereMode.IDDC);
            dcQryParameters.Parameters["IDCE"].Value = idCE;
            dcQryParameters.Parameters["IDA_ENTITY"].Value = idA_Entity;
            dcQryParameters.Parameters["IDDC"].Value = idDC;
            object obj = DataHelper.ExecuteScalar(pDbTransaction, CommandType.Text, dcQryParameters.Query, dcQryParameters.Parameters.GetArrayDbParameter());
            if (null != obj)
                IdCEC = Convert.ToInt32(obj);
        }
        #endregion Methods
    }
    #endregion CorporateEventContract
    #region CorporateEventDAttrib
    /// <summary>
    /// DerivativeAttrib concerné par l'événement
    /// </summary>
    [SerializableAttribute()]
    [XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    [XmlRootAttribute("dAttrib", Namespace = "http://www.efs.org/2007/EFSmL-3-0", IsNullable = false)]
    // EG 20150114 [20676] Add FutureValuationMethodEnum
    public class CorporateEventDAttrib : SpheresCommonIdentification
    {
        #region Members
        [XmlElementAttribute("idCEC", Order = 1)]
        public int idCEC;
        [XmlElementAttribute("idCE", Order = 2)]
        public int idCE;
        [XmlElementAttribute("idA_Entity", Order = 3)]
        public int idA_Entity;
        [XmlElementAttribute("idDC", Order = 4)]
        public int idDC;
        [XmlElementAttribute("idCEDA", Order = 5)]
        public int idDA;
        [XmlElementAttribute("category", Order = 6)]
        public CfiCodeCategoryEnum category;
        [XmlElementAttribute("readyState", Order = 7)]
        public CorporateEventReadyStateEnum readyState;
        [XmlElementAttribute("adjStatus", Order = 8)]
        public CATools.AdjStatusEnum adjStatus;
        [XmlElementAttribute("method", Order = 9)]
        public AdjustmentMethodOfDerivContractEnum adjMethod;
        [XmlIgnoreAttribute()]
        public bool rFactorSpecified;
        [XmlElementAttribute("rFactor", Order = 10)]
        public SimpleUnit rFactor;

        [XmlElementAttribute("cumData", Order = 11)]
        public CalculationCumData cumData;
        [XmlIgnoreAttribute()]
        public bool exDataSpecified;
        [XmlElementAttribute("exData", Order = 12)]
        public CalculationExData exData;

        [XmlIgnoreAttribute()]
        public bool assetsSpecified;
        [XmlArrayAttribute("assets", Order = 13)]
        [XmlArrayItemAttribute("asset")]
        public CorporateEventAsset[] assets;

        [XmlElementAttribute("idDAEX", Order = 14)]
        public int idDAEX;
        [XmlElementAttribute("idDAEXADJ", Order = 15)]
        public int idDAEXADJ;

        [XmlIgnoreAttribute()]
        public bool priceDecLocatorSpecified;
        [XmlElementAttribute("priceDecLocator", Order = 16)]
        public EFS_Integer priceDecLocator;
        [XmlIgnoreAttribute()]
        public bool strikeDecLocatorSpecified;
        [XmlElementAttribute("strikeDecLocator", Order = 17)]
        public EFS_Integer strikeDecLocator;

        [XmlIgnoreAttribute()]
        public DataRowState rowState;

        [XmlIgnoreAttribute()]
        public decimal rFactorRetained;

        // EG 20150114 [20676] 
        [XmlIgnoreAttribute()]
        public FuturesValuationMethodEnum futValuationMethod;

        // EG 20190121 [23249] New pour Application arrondi sur EQP avant|après application de la quantité en position sur le trade
        [XmlIgnoreAttribute()]
        public CashFlowCalculationMethodEnum cashFlowCalcMethod;
        #endregion Members
        #region Accessors
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public int IdCEDA
        {
            get { return Convert.ToInt32(spheresid); }
            set { spheresid = value.ToString(); }
        }
        #endregion Accessors
    }
    #endregion CorporateEventDAttrib
    #region CorporateEventAsset
    /// <summary>
    /// Asset concerné par l'événement
    /// </summary>
    [SerializableAttribute()]
    [XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    [XmlRootAttribute("asset", Namespace = "http://www.efs.org/2007/EFSmL-3-0", IsNullable = false)]
    // EG 20150114 [20676] Add FutureValuationMethodEnum
    public class CorporateEventAsset : SpheresCommonIdentification
    {
        #region Members
        [XmlElementAttribute("idCEC", Order = 1)]
        public int idCEC;
        [XmlElementAttribute("idCE", Order = 2)]
        public int idCE;
        [XmlElementAttribute("idA_Entity", Order = 3)]
        public int idA_Entity;
        [XmlElementAttribute("idASSET", Order = 4)]
        public int idASSET;
        [XmlElementAttribute("idDA", Order = 5)]
        public int idDA;
        [XmlElementAttribute("idDC", Order = 6)]
        public int idDC;
        [XmlElementAttribute("category", Order = 7)]
        public CfiCodeCategoryEnum category;
        [XmlElementAttribute("readyState", Order = 8)]
        public CorporateEventReadyStateEnum readyState;
        [XmlElementAttribute("adjStatus", Order = 9)]
        public CATools.AdjStatusEnum adjStatus;
        [XmlElementAttribute("method", Order = 10)]
        public AdjustmentMethodOfDerivContractEnum adjMethod;
        [XmlIgnoreAttribute()]
        public bool rFactorSpecified;
        [XmlElementAttribute("rFactor", Order = 11)]
        public SimpleUnit rFactor;

        [XmlElementAttribute("cumData", Order = 12)]
        public CalculationCumData cumData;
        [XmlIgnoreAttribute()]
        public bool exDataSpecified;
        [XmlElementAttribute("exData", Order = 13)]
        public CalculationExData exData;

        [XmlElementAttribute("idASSETEX", Order = 14)]
        public int idASSETEX;
        [XmlElementAttribute("idASSETEXADJ", Order = 15)]
        public int idASSETEXADJ;

        [XmlIgnoreAttribute()]
        public bool priceDecLocatorSpecified;
        [XmlElementAttribute("priceDecLocator", Order = 16)]
        public EFS_Integer priceDecLocator;
        [XmlIgnoreAttribute()]
        public bool strikeDecLocatorSpecified;
        [XmlElementAttribute("strikeDecLocator", Order = 17)]
        public EFS_Integer strikeDecLocator;


        [XmlIgnoreAttribute()]
        public DataRowState rowState;

        [XmlIgnoreAttribute()]
        public decimal rFactorRetained;

        // EG 20150114 [20676] 
        [XmlIgnoreAttribute()]
        public FuturesValuationMethodEnum futValuationMethod;

        // EG 20190121 [23249] New pour Application arrondi sur EQP avant|après application de la quantité en position sur le trade
        [XmlIgnoreAttribute()]
        public CashFlowCalculationMethodEnum cashFlowCalcMethod;

        #endregion Members
        #region Accessors
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public int IdCEA
        {
            get { return Convert.ToInt32(spheresid); }
            set { spheresid = value.ToString(); }
        }
        /// <summary>
        /// Un montant d'Equalisation payment (Soulte) doit être calculé sur l'option ou future
        /// </summary>
        // EG 20141106 [20253] Equalisation payment
        public bool IsEqualPaymentMustBeCalculated
        {
            get
            {
                bool _ret = false;
                //if ((this.category== CfiCodeCategoryEnum.Option) &&  exDataSpecified && exData.contractSizeSpecified)
                if (exDataSpecified && exData.contractSizeSpecified)
                {
                    decimal _result = exData.contractSize.value.DecValue;
                    decimal _resultRounded = exData.contractSize.value.DecValue;
                    if (exData.contractSize.valueRoundedSpecified)
                        _resultRounded = exData.contractSize.valueRounded.DecValue;
                    _ret = (_result != _resultRounded);
                }
                return _ret;
            }
        }
        #endregion Accessors
    }
    #endregion CorporateEventAsset
    #region CorporateEventMktRules
    /// <summary>
    /// Règles de marché pour les Corporate actions
    /// </summary>
    [SerializableAttribute()]
    [XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    [XmlRootAttribute("mktRules", Namespace = "http://www.efs.org/2007/EFSmL-3-0", IsNullable = false)]
    // EG 20140317 [19722]
    // EG 20141106 [20253] Equalisation payment
    // PM 20170908 [23408] Suppression du paramètrage des EventCode/EventType pour les Equalisation payment
    public class CorporateEventMktRules 
    {
        #region Members
        [XmlElementAttribute("idM", Order = 1)]
        public int idM;
        [XmlElementAttribute("renamingContractMethod", Order = 2)]
        public CorpoEventRenamingContractMethodEnum renamingContractMethod;
        [XmlIgnoreAttribute()]
        public bool renamingCategorySpecified;
        [XmlElementAttribute("renamingCategory", Order = 3)]
        public CfiCodeCategoryEnum renamingCategory;
        [XmlElementAttribute("isEqualPaymentFutureAuthorized", Order = 4)]
        public bool isEqualPaymentFutureAuthorized;
        // PM 20170911 [23408] Le paramétrage des codes événements n'est plus possible
        //public bool equalPaymentFutureEventCodeSpecified;
        //[XmlElementAttribute("equalPaymentEventCode", Order = 5)]
        //public string equalPaymentFutureEventCode;
        //public bool equalPaymentFutureEventTypeSpecified;
        //[XmlElementAttribute("equalPaymentFutureEventType", Order = 6)]
        //public string equalPaymentFutureEventType;
        [XmlElementAttribute("isEqualPaymentOptionAuthorized", Order = 5)]
        public bool isEqualPaymentOptionAuthorized;
        // PM 20170911 [23408] Le paramétrage des codes événements n'est plus possible
        //public bool equalPaymentOptionEventCodeSpecified;
        //[XmlElementAttribute("equalPaymentOptionEventCode", Order = 8)]
        //public string equalPaymentOptionEventCode;
        //public bool equalPaymentOptionEventTypeSpecified;
        //[XmlElementAttribute("equalPaymentOptionEventType", Order = 9)]
        //public string equalPaymentOptionEventType;
        [XmlElementAttribute("rounding", Order = 6)]
        public List<Pair<AdjustmentElementEnum, Rounding>> rounding;
        #endregion Members
        #region Accessors
        #endregion Accessors
        #region Constructors
        public CorporateEventMktRules()
        {
        }
        #endregion Constructors
        #region Methods
        #endregion Methods
    }
    #endregion CorporateEventMktRules


    /* -------------------------------------------------------- */
    /* ----- CLASSES METHODES D'AJUSTEMENT                ----- */
    /* -------------------------------------------------------- */


    #region CAWorkingDataContainer
    /// <summary>
    /// Classe de travail pour l'automate de calcul des composants d'ajustement pour un élément d'ajustement donné.
    /// </summary>
    [SerializableAttribute()]
    public class CAWorkingDataContainer
    {
        #region Members
        /// <summary>
        /// Class CSS pour les messages informatifs générés pendant l'évaluation des composants
        /// </summary>
        private enum CssMessage
        {
            formula,
            result,
            resultCertified,
            subtitle,
            title,
        }
        /// <summary>
        /// Entité courante
        /// </summary>
        private readonly int m_CurrentIdA_Entity;
        /// <summary>
        /// Marché courant
        /// </summary>
        private readonly int m_CurrentIdM;
        /// <summary>
        /// CumDate
        /// </summary>
        // EG 20160404 Migration vs2013
        //private DateTime m_CumDate;
        /// <summary>
        /// Dictionnaire des composants utilisé pour l'évaluation des composants
        /// </summary>
        private Dictionary<Pair<string, string>, ComponentBase> m_DicComponents;
        /// <summary>
        /// Indicateur spécifiant si une trace par message doit être alimentée pendant le calcul 
        /// à remplacer par la suite par un énumérateur (Message ou Log)
        /// </summary>
        private bool m_IsMessageSupplied;
        /// <summary>
        /// Message généré
        /// </summary>
        private string m_Message;
        /// <summary>
        /// Règle d'arrondide l'élément en cours d'ajustement
        /// </summary>
        //private Rounding m_Rounding;

        private ProcessCacheContainer m_ProcessCacheContainer;
        #endregion Members
        #region Accessors
        /// <summary>
        /// Données communes en provenance du process
        /// </summary>
        public ProcessCacheContainer ProcessCacheContainer
        {
            get { return m_ProcessCacheContainer; }
            set { m_ProcessCacheContainer = value; }
        }
        /// <summary>
        /// Données de l'entité courante
        /// </summary>
        public IPosKeepingMarket EntityMarketInfo
        {
            get { return m_ProcessCacheContainer.GetEntityMarket(m_CurrentIdA_Entity, m_CurrentIdM, null); }
        }
        /// <summary>
        /// Dictionnaires des composants
        /// </summary>
        public Dictionary<Pair<string, string>, ComponentBase> DicComponents
        {
            get { return m_DicComponents; }
            set { m_DicComponents = value; }
        }
        /// <summary>
        /// Indicateur spécifiant si une trace par message doit être alimentée pendant le calcul 
        /// </summary>
        public bool IsMessageSupplied
        {
            get { return m_IsMessageSupplied; }
            set { m_IsMessageSupplied = value; }
        }
        /// <summary>
        /// Message généré
        /// </summary>
        public string Message
        {
            get { return m_Message; }
            set { m_Message = value; }
        }
        /// <summary>
        /// Règle d'arrondi du marché courant
        /// </summary>
        public List<Pair<AdjustmentElementEnum, Rounding>> MktRulesRounding
        {
            get
            {
                return m_ProcessCacheContainer.GetMktRules(m_CurrentIdM).rounding; 
            }
        }
        /// <summary>
        /// Message généré
        /// </summary>
        /// EG 20130719 PosRequestType appelant (use by ReadQuote)
        public Nullable<Cst.PosRequestTypeEnum> PosRequestType
        {
            get { return m_ProcessCacheContainer.PosRequestType; }
        }
        #endregion Accessors
        #region Constructors
        /// <summary>
        /// Initialisation
        /// </summary>
        public CAWorkingDataContainer(Adjustment pAdjustment, int pIdA_Entity, int pIdM, ProcessCacheContainer pProcessCacheContainer, bool pIsMessageSupplied)
        {
            m_CurrentIdA_Entity = pIdA_Entity;
            m_CurrentIdM = pIdM;
            m_ProcessCacheContainer = pProcessCacheContainer;
            m_IsMessageSupplied = pIsMessageSupplied;
            // Chargement des composants
            m_DicComponents = new Dictionary<Pair<string, string>, ComponentBase>();
            CATools.AllComponents(pAdjustment, ref m_DicComponents);
        }
        #endregion Constructors
        #region Methods
        #region AddDictionaryComponent
        /// <summary>
        /// Ajout d'autres composants dans le dictionnaires pour une source donné
        /// </summary>
        /// <typeparam name="T">Type de la source</typeparam>
        /// <param name="pSource">Source</param>
        /// <returns></returns>
        public void AddDictionaryComponent<T>(T pSource)
        {
            CATools.AllComponents(pSource, ref m_DicComponents);
        }
        #endregion AddDictionaryComponent
        #region SetMessageTitle
        /// <summary>
        /// Message : Titre  (Ajustement, Mise en place, Array (sous-jacents), Formule et autres...)
        /// </summary>
        /// <typeparam name="T">Type de la source</typeparam>
        /// <param name="pSource">Source</param>
        // EG 20200914 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc) Correction et compléments
        // EG 20200930 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc) Correction et suppression de codes inutiles
        public void SetMessageTitle<T>(T pSource)
        {
            if (m_IsMessageSupplied)
            {
                string _pattern = "<span class='{1}'>{0}</span>";
                if (pSource is Array)
                {
                    Array _source = pSource as Array;
                    Type _type = _source.GetType().GetElementType();
                    if (_type == typeof(CorporateEventUnderlyer))
                        m_Message += String.Format(_pattern, Ressource.GetString(CATools.pfxLBLComponent + _type.Name, true), CssMessage.title + " blue");
                }
                else if (pSource is Adjustment)
                {
                    Adjustment _source = pSource as Adjustment;
                    m_Message += String.Format(_pattern, _source.description, CssMessage.title + " blue");
                }
                else if (pSource is TakePlace)
                {
                    m_Message += String.Format("<span class='{1}'><i class='fas fa-info-circle'></i>{0}</span>", Ressource.GetString(CATools.pfxLBLComponent + "takePlace", true), CssMessage.subtitle);
                }
                else if (pSource is CorporateEventUnderlyer)
                {
                    CorporateEventUnderlyer _source = pSource as CorporateEventUnderlyer;
                    string _pattern2 = "<span class='{2}'>{0} <b>{1}</b></span>";
                    m_Message += String.Format("<span class='{1}'><i class='fas fa-info-circle'></i>{0}</span>", _source.identifier, CssMessage.subtitle);
                    m_Message += String.Format(_pattern2, Ressource.GetString(CATools.pfxLBLComponent + "UNLIsinCode", true),
                        _source.isinCode, CssMessage.result) + Cst.HTMLBreakLine;
                    m_Message += String.Format(_pattern2, Ressource.GetString(CATools.pfxLBLComponent + "UNLCategory", true),
                        _source.category, CssMessage.result) + Cst.HTMLBreakLine2;

                }
                else if (pSource is ComponentFormula)
                {
                    ComponentFormula _source = pSource as ComponentFormula;
                    m_Message += String.Format("<span class='{1}'><i class='fas fa-info-circle'></i>{0}</span>", Ressource.GetString(CATools.pfxLBLComponent + _source.Id, true), CssMessage.subtitle);
                    m_Message += String.Format("<span class='{3}'>{0} <b>{1} = {2}</b></span>",
                        Ressource.GetString(CATools.pfxLBLComponent + "Formula", true), _source.name,
                        _source.formula.mathExpression, CssMessage.formula);

                }
            }
        }
        #endregion SetMessageTitle
        #region SetMessageResult
        /// <summary>
        /// Message : Résultat de l'évaluation d'un composant
        /// </summary>
        /// <param name="pComponent">Composant évalué</param>
        /// <param name="pErrLevel">ErrLevel</param>
        public void SetMessageResult(ComponentBase pComponent, Cst.ErrLevel pErrLevel)
        {
            if (m_IsMessageSupplied)
            {
                string _id = pComponent.Id;
                string _description = pComponent.descriptionSpecified?pComponent.description:pComponent.Id;
                string _pattern = "<span class='{4}'>{0} (<i>{1}</i>) <b{2}>{3}</b></span>";

                if (pComponent is ComponentReference reference)
                    _id = reference.href;
                string _arg2 = string.Empty;
                string _arg3 = string.Empty;
                if (Cst.ErrLevel.SUCCESS == pErrLevel)
                {
                    switch (pComponent.result.itemsElementName)
                    {
                        case ResultType.amount:
                        case ResultType.unit:
                            _arg3 = new EFS_Decimal(pComponent.Result.Value).CultureValue;
                            break;
                        case ResultType.check:
                            _arg3 = Convert.ToBoolean(pComponent.result.result)?"true":"false";
                            break;
                        case ResultType.info:
                            _arg3 = pComponent.result.result as string;
                            break;
                    }
                }
                else
                {
                    _arg2 = " class='" + Cst.ErrLevel.FAILURE.ToString() + "'";
                    _arg3 = "N/A";
                }

                if (StrFunc.IsEmpty(pComponent.name))
                    _pattern = _pattern.Replace("(", string.Empty).Replace(")", String.Empty);
                m_Message += String.Format(_pattern, Ressource.GetString(CATools.pfxLBLComponent + _id, _description, false), pComponent.name,
                    _arg2, _arg3, CssMessage.result) + Cst.HTMLBreakLine;
            }
        }
        /// <summary>
        /// Message : Résultat final de l'évaluation de l'élément d'ajustement 
        /// Après application de la règle d'arrondi
        /// </summary>
        // EG 20200914 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc) Correction et compléments
        public void SetMessageResult(AdjustmentElement pAdjustmentElement, Cst.ErrLevel pErrLevel)
        {
            if (m_IsMessageSupplied)
            {
                // EG 20160404 Migration vs2013
                //string _pattern = "<span class='{1}'>{0}</span>";
                m_Message += String.Format("<span class='{1}'><i class='fas fa-info-circle'></i>{0}</span>", Ressource.GetString(CATools.pfxLBLComponent + "finalResult", true), CssMessage.subtitle);

                Rounding _rounding = pAdjustmentElement.Rounding;
                RoundingDirectionEnum roundingDirection = RoundingDirectionEnum.Nearest;
                switch (_rounding.direction)
                {
                    case Cst.RoundingDirectionSQL.D:
                        roundingDirection = RoundingDirectionEnum.Down;
                        break;
                    case Cst.RoundingDirectionSQL.N:
                        roundingDirection = RoundingDirectionEnum.Nearest;
                        break;
                    case Cst.RoundingDirectionSQL.U:
                        roundingDirection = RoundingDirectionEnum.Up;
                        break;
                }
                m_Message += String.Format("<span class='{2}'>{0}<b>{1}</b></span>",
                    Ressource.GetString(CATools.pfxLBLComponent + "RoundingRules", true),
                    roundingDirection.ToString() + "-" + _rounding.precision.ToString(), CssMessage.result) + Cst.HTMLBreakLine; ;

                ComponentBase _component = pAdjustmentElement.component[0] as ComponentBase;
                Nullable<decimal> _result = _component.ResultRounded;
                string _value = "N/A";
                if (_result.HasValue)
                    _value = new EFS_Decimal(_result.Value).CultureValue;

                string _arg3 = pErrLevel.ToString();
                if (Cst.ErrLevel.SUCCESS != pErrLevel)
                    _arg3 = Cst.ErrLevel.FAILURE.ToString();

                m_Message += String.Format("<span class='{2} {3}'>{0}<b>{1}</b></span>",
                    Ressource.GetString(CATools.pfxLBLComponent + _component.Id, true), _value, CssMessage.result, _arg3) + Cst.HTMLBreakLine; ;
            }
        }
        /// <summary>
        /// Message : Résultat de l'élément de condition de mise en place de l'ajustement
        /// </summary>
        /// <param name="pTakePlace">Mise en place</param>
        public void SetMessageResult(TakePlace pTakePlace)
        {
            if (m_IsMessageSupplied)
            {
                // Condition
                m_Message += String.Format("<span class='{2}'>{0}<b>{1}</b></span>", Ressource.GetString(CATools.pfxLBLComponent + "condition"),
                    pTakePlace.condition.ToString(), CssMessage.result) + Cst.HTMLBreakLine; ;
                // Resultat de la condition
                m_Message += String.Format("<span class='{2}'>{0}<b>{1}</b></span>", Ressource.GetString(CATools.pfxLBLComponent + "response",true),
                    pTakePlace.response.ToString(), CssMessage.result) + Cst.HTMLBreakLine; ;
            }
        }
        /// <summary>
        /// Message : Valeur d'un composant simple (R-Factor certifié)
        /// </summary>
        /// <param name="pComponent"></param>
        public void SetMessageResult(ComponentSimple pComponent)
        {
            if (m_IsMessageSupplied)
            {
                string _value = "N/A";
                if (pComponent.Result.HasValue)
                    _value = new EFS_Decimal(pComponent.Result.Value).CultureValue;
                m_Message += String.Format("<span class='{2}'>{0} = <b>{1}</b></span>",
                Ressource.GetString(CATools.pfxLBLComponent + pComponent.Id, true), _value, CssMessage.resultCertified);
            }
        }

        #endregion SetMessageResult
        // EG 20231129 [WI762] End of Day processing : Possibility to request processing without initial margin (Cst.PosRequestTypeEnum.EndOfDayWithoutInitialMargin)
        public bool IsEODRequest
        {
            get 
            {
                return PosRequestType.HasValue &&
                    ((PosRequestType.Value == Cst.PosRequestTypeEnum.EndOfDay) || (PosRequestType.Value == Cst.PosRequestTypeEnum.EndOfDayWithoutInitialMargin));
            }
        }
        #endregion Methods
    }
    #endregion CAWorkingDataContainer
    #region Ajustment
    /// <summary>
    /// Classe de base étendue aux différentes méthodes d'ajustement
    /// Méthodes:
    /// ● Ratio (R-Factor)
    /// ● Fair-Value (TBD)
    /// ● Package (TBD)
    /// </summary>
    // EG [33415/33420] Intégration de nouveaux composants additionnels
    // EG 20141106 [20253] Equalisation payment
    /// EG 20240105 [WI756] Spheres Core : Refactoring Code Analysis - Correctifs après tests (property Id - Attribute name)
    [SerializableAttribute()]
    [XmlIncludeAttribute(typeof(AdjustmentNone))]
    [XmlIncludeAttribute(typeof(AdjustmentRatio))]
    [XmlIncludeAttribute(typeof(AdjustmentPackage))]
    [XmlIncludeAttribute(typeof(AdjustmentFairValue))]
    [XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public abstract partial class Adjustment
    {
        #region Members
        [XmlIgnoreAttribute()]
        public bool descriptionSpecified;
        [XmlElementAttribute("description", Order = 1)]
        public string  description;
        [XmlElementAttribute("methodType", Order = 2)]
        public AdjustmentMethodOfDerivContractEnum method;
        [XmlIgnoreAttribute()]
        public bool takePlaceSpecified;
        [XmlElementAttribute("takePlace", Order = 3)]
        public TakePlace takePlace;
        // EG [33415/33420]
        [XmlIgnoreAttribute()]
        public bool additionalInfoSpecified;
        [XmlElementAttribute("additionalInfo", Order = 4)]
        public AdditionalInfo additionalInfo;
        [XmlElementAttribute("contract", Order = 5)]
        public AdjustmentContract contract;


        [XmlElementAttribute("template", Order = 6)]
        public string templateFileName;
        [XmlElementAttribute("template_AI", Order = 7)]
        public string templateFileName_AI;
        [XmlElementAttribute("templateFileName_CT", Order = 8)]
        public string templateFileName_CT;

        [XmlAttributeAttribute("id", DataType = "ID")]
        public string id;
        #endregion Members

        [XmlIgnoreAttribute()]
        protected CorporateEventContract _currentContract;
        [XmlIgnoreAttribute()]
        protected CorporateEventAsset _currentAsset;
        [XmlIgnoreAttribute()]
        protected CorporateEventDAttrib _currentDAttrib;


        #region Accessors
        [XmlIgnoreAttribute()]
        public List<string> LstUnderlyerIsinCode{get;set;}

        [XmlIgnoreAttribute()]
        public List<AIUnderlyer> LstEx_AIUnderlyer { get; set; }
        [XmlIgnoreAttribute()]
        public List<AIUnderlyer> LstExAdj_AIUnderlyer { get; set; }
        [XmlIgnoreAttribute()]
        public List<AIUnderlyer> LstRecycled_AIUnderlyer { get; set; }

        [XmlIgnoreAttribute()]
        public List<string> LstCum_AIContractNoAdjusted { get; set; }
        [XmlIgnoreAttribute()]
        public List<AIContract> LstEx_AIContract { get; set; }
        [XmlIgnoreAttribute()]
        public List<AIContract> LstExAdj_AIContract { get; set; }
        [XmlIgnoreAttribute()]
        public List<AIContract> LstExRecycled_AIContract { get; set; }
        #endregion Accessors

        #region Methods
        #region SetAdditionalInfos
        // EG 20180426 Analyse du code Correction [CA2202]
        public void SetAdditionalInfos(MQueueparameters pMQueueParameters, string pTemplatePath)
        {
            MQueueparameter _parameter = pMQueueParameters["TEMPLATEFILENAME_AI"];
            if (null != _parameter)
            {
                string fileName = SystemIOTools.AddFileNameSuffixe(_parameter.Value, ".xml");
                FileInfo fileInfo = new FileInfo(pTemplatePath + @"Additionals\" + fileName);
                if (null != fileInfo)
                {
                    using (FileStream XMLTemplate = FileTools.OpenFile(fileInfo, FileMode.Open, FileAccess.Read))
                    {
                        AdditionalInfos ai = SerializationHelper.LoadObjectFromFileStream<AdditionalInfos>(XMLTemplate);
                        templateFileName_AI = _parameter.Value;
                        additionalInfoSpecified = (null != ai);
                        if (additionalInfoSpecified)
                            additionalInfo = ai.additionalInfo;
                    }
                }
            }
        }
        #endregion SetAdditionalInfos
        #region SetAdjustmentContract
        // EG 20180426 Analyse du code Correction [CA2202]
        public void SetAdjustmentContract(MQueueparameters pMQueueParameters, string pTemplatePath)
        {
            string fileName = string.Empty;
            MQueueparameter _parameter = pMQueueParameters["TEMPLATEFILENAME_CT"];
            if (null != _parameter)
                fileName = _parameter.Value;
            else
            {
                if (this is AdjustmentRatio)
                    fileName = "CT_Ratio";
                //else if (this is AdjustmentPackage)
                //    fileName = "CT_Package";
                //else if (this is AdjustmentFairValue)
                //    fileName = "CT_FairValue";
            }
            if (StrFunc.IsFilled(fileName))
            {
                FileInfo fileInfo = new FileInfo(pTemplatePath + @"Additionals\" + SystemIOTools.AddFileNameSuffixe(fileName, ".xml"));
                if (null != fileInfo)
                {
                    using (FileStream XMLTemplate = FileTools.OpenFile(fileInfo, FileMode.Open, FileAccess.Read))
                    {
                        contract = SerializationHelper.LoadObjectFromFileStream<AdjustmentContract>(XMLTemplate);
                        templateFileName_CT = fileName;
                    }
                }
            }
        }
        #endregion SetAdjustmentContract
        #region SetCumContractMultiplierToEx
        public virtual void SetCumContractMultiplierToEx<T>(T pTarget)
        {
        }
        #endregion SetCumContractMultiplierToEx
        #region SetCumContractSizeToEx
        public virtual void SetCumContractSizeToEx<T>(T pTarget)
        {
        }
        #endregion SetCumContractSizeToEx

        #region GetAdditionalInfo
        /// <summary>
        /// Donne la valeur d'un composant additionnel
        /// </summary>
        /// <param name="pComponent">id du composant</param>
        /// <returns></returns>
        // EG [33415/33420] New
        public object GetAdditionalInfo(string pComponent)
        {
            object _info = null;
            if (additionalInfoSpecified)
            {
                foreach (ComponentBase component in additionalInfo.component)
                {
                    if (pComponent.ToLower() == component.Id.ToLower())
                    {
                        if (component.resultSpecified)
                        {
                            switch (component.result.itemsElementName)
                            {
                                case ResultType.info:
                                    _info = component.result.result.ToString();
                                    break;
                                case ResultType.unit:
                                    SimpleUnit _unit = component.result.result as SimpleUnit;
                                    _info = (_unit.valueRoundedSpecified ? _unit.valueRounded.DecValue : _unit.value.DecValue);
                                    break;
                                case ResultType.amount:
                                    Money _money = component.result.result as Money;
                                    _info = (_money.amountRoundedSpecified ? _money.amountRounded.DecValue : _money.amount.DecValue);
                                    break;
                                case ResultType.check:
                                    _info = BoolFunc.IsTrue(component.result.result);
                                    break;
                            }
                        }
                        break;
                    }
                }
            }
            return _info;
        }
        #endregion GetAdditionalInfo

        #region GetResetDerivativeIsinCode
        public Nullable<bool> GetResetDerivativeIsinCode() { return GetBooleanAdditionalInfo("resetDerivativeIsinCode"); }
        #endregion GetResetDerivativeIsinCode
        #region GetLstContractSymbol
        public string GetLstContractSymbol(CATools.CAElementTypeEnum pEltType)  
        {
            return GetStringAdditionalInfo(pEltType.ToString() + "ContractSymbol"); 
        }
        #endregion GetLstContractSymbol

        #region GetUnderlyerIsinCode
        public string GetUnderlyerIsinCode(CATools.CAElementTypeEnum pEltType) 
        {
            return GetStringAdditionalInfo(pEltType.ToString() + "UnderlyerIsinCode"); 
        }
        #endregion GetUnderlyerIsinCode

        #region InitAIContract
        public void InitAIContract(List<CorporateEventContract> pCorpoEventContract)
        {
            LstCum_AIContractNoAdjusted = InitAIContractNoAdjusted("cumContractNoAdjusted");
            LstEx_AIContract = InitAIContract(CATools.CAElementTypeEnum.Ex, pCorpoEventContract);
            AddLstContractUnderlyer(CATools.CAElementTypeEnum.Ex);
            LstExRecycled_AIContract = InitAIContract(CATools.CAElementTypeEnum.ExRecycled, pCorpoEventContract);
            AddLstContractUnderlyer(CATools.CAElementTypeEnum.ExRecycled );
            LstExAdj_AIContract = InitAIContract(CATools.CAElementTypeEnum.ExAdj, pCorpoEventContract);
            AddLstContractUnderlyer(CATools.CAElementTypeEnum.ExAdj);
        }
        /// <summary>
        /// Initialisation des informations additionnelles pour la CA
        /// Exemple:
        /// En entrée (_value)         : sym=AAA|BBB;dsn=AAA - Class A|BBB - Class C;isin=US0000000001|US0000000002;csize=1000|100;cmul=1000|100 etc...
        /// Intermédiaire (_lstInput)  : List('sym=AAA|BBB', 'dsn=AAA - Class A|BBB - Class C', 'isin=US0000000001|US0000000002', 'csize=1000|100', 'cmul=1000|100') etc...
        ///               (_lstOutput) : List(Pair('sym', ['AAA','BBB']), Pair('dsn', ['AAA - Class A','BBB - Class C']),Pair('isin', ['US0000000001','US0000000002']),
        ///                              Pair('csize', ['1000','100']), Pair('cmul', ['1000','100']) etc...
        /// En sortie                  : List (AIContract)
        /// </summary>
        /// <param name="pElementType"></param>
        /// <param name="pCorpoEventContract"></param>
        /// <returns></returns>
        public List<AIContract> InitAIContract(CATools.CAElementTypeEnum pElementType, List<CorporateEventContract> pCorpoEventContract)
        {
            List<AIContract> _lstAIContract = new List<AIContract>();

            string _value = string.Empty;
            switch (pElementType)
            {
                case CATools.CAElementTypeEnum.ExAdj:
                    _value = GetStringAdditionalInfo("exAdjContract");
                    break;
                case CATools.CAElementTypeEnum.Ex:
                    _value = GetStringAdditionalInfo("exContract");
                    break;
                case CATools.CAElementTypeEnum.ExRecycled:
                    _value = GetStringAdditionalInfo("exRecycledContract");
                    break;
            }
            
            if (StrFunc.IsFilled(_value))
            {
                List<string> _lstInput = new List<string>(_value.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries));
                List<Pair<CATools.CAElementEnum, string[]>> _lstOutPut = new List<Pair<CATools.CAElementEnum, string[]>>();

                if (0 < _lstInput.Count)
                {
                    _lstInput.ForEach(item =>
                        {
                            string[] data = item.Split(new char[] { '=' }, StringSplitOptions.None);
                            for (int i = 0; i < data.Length; i+=2)
                            {
                                CATools.CAElementEnum _enum = (CATools.CAElementEnum) ReflectionTools.EnumParse(new CATools.CAElementEnum(), data[i]);
                                Pair<CATools.CAElementEnum, string[]> _element = 
                                    new Pair<CATools.CAElementEnum, string[]>(_enum, data[i + 1].Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries));
                                _lstOutPut.Add(_element);
                            }
                        });
                }

                Pair<CATools.CAElementEnum, string[]> _lstSymbol = _lstOutPut.Find(item => item.First == CATools.CAElementEnum.sym);
                if (null == _lstSymbol)
                {
                    Pair<CATools.CAElementEnum, string[]> _element = new Pair<CATools.CAElementEnum, string[]>
                    {
                        First = CATools.CAElementEnum.sym
                    };
                    _lstOutPut.Add(_element);
                    _lstSymbol = _lstOutPut.Find(item => item.First == CATools.CAElementEnum.sym);
                }
                
                if ((null != _lstSymbol) && ArrFunc.IsEmpty(_lstSymbol.Second))
                {
                    ArrayList _lstContract = new ArrayList();
                    string _lstContractSymbol = GetLstContractSymbol(pElementType);
                    pCorpoEventContract.ForEach(item => 
                        {
                            string _symbol = item.contractSymbol;
                            if (StrFunc.IsFilled(_lstContractSymbol))
                                _symbol = ReplaceOldValueByNewValue(_symbol, _lstContractSymbol, false).ToString();
                            if (false == _lstContract.Contains(_symbol))
                                _lstContract.Add(_symbol);

                        });
                    _lstSymbol.Second = (string[])_lstContract.ToArray(typeof(string));
                }
                if (null != _lstSymbol)
                {
                    for (int i = 0; i < _lstSymbol.Second.Length; i++)
                    {
                        AIContract _aiContract = new AIContract(_lstSymbol.Second[i]);

                        foreach (CATools.CAElementEnum _enum in (CATools.CAElementEnum[])System.Enum.GetValues(typeof(CATools.CAElementEnum)))
                        {
                            if (CATools.CAElementEnum.sym != _enum)
                            {
                                Pair<CATools.CAElementEnum, string[]> _lstElement = _lstOutPut.Find(item => item.First == _enum);
                                if ((null != _lstElement) && ArrFunc.IsFilled(_lstElement.Second))
                                {
                                    string _tmp = _lstElement.Second[Math.Max(0, Math.Min(i, _lstElement.Second.Length - 1))];
                                    if (StrFunc.IsFilled(_tmp))
                                    {
                                        switch (_enum)
                                        {
                                            case CATools.CAElementEnum.cmul:
                                                if (DecFunc.IsDecimal(_tmp))
                                                    _aiContract.ContractMultiplier = DecFunc.DecValueFromInvariantCulture(_tmp);
                                                break;
                                            case CATools.CAElementEnum.csize:
                                                if (DecFunc.IsDecimal(_tmp))
                                                    _aiContract.ContractSize = DecFunc.DecValueFromInvariantCulture(_tmp);
                                                break;
                                            case CATools.CAElementEnum.desc:
                                                _aiContract.Description = _tmp;
                                                break;
                                            case CATools.CAElementEnum.dsn:
                                                _aiContract.DisplayName = _tmp;
                                                break;
                                            case CATools.CAElementEnum.isin:
                                                _aiContract.IsinCode = _tmp;
                                                break;
                                            case CATools.CAElementEnum.qtymul:
                                                if (DecFunc.IsDecimal(_tmp))
                                                    _aiContract.QtyMultiplier = DecFunc.DecValueFromInvariantCulture(_tmp);
                                                break;
                                            case CATools.CAElementEnum.unlisin:
                                                _aiContract.UnlIsinCode = _tmp;
                                                break;
                                        }
                                    }
                                }
                            }
                        }
                        _lstAIContract.Add(_aiContract);
                    }
                }
            }
            return _lstAIContract;
        }
        #endregion InitAIContract
        #region InitAIContractNoAdjusted
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pId">AdditionalInfo id = cumContractNoAdjusted</param>
        public List<string> InitAIContractNoAdjusted(string pId)
        {
            List<string> _aiContract = new List<string>();
            // AAA|BBB
            string _value = GetStringAdditionalInfo(pId);
            if (StrFunc.IsFilled(_value))
            {
                string[] sep = new string[] { "|"};
                string[] _expr = _value.Split(sep , StringSplitOptions.RemoveEmptyEntries);

                // _expr[0]  = AAA
                // _expr[1]  = BBB

                if (ArrFunc.IsFilled(_expr))
                {
                    for (int i = 0; i < _expr.Length; i++)
                    {
                        if (false == _aiContract.Contains(_expr[i]))
                            _aiContract.Add(_expr[i]);
                    }
                }
            }
            return _aiContract;
        }
        #endregion InitAIContractNoAdjusted

        #region IsContractAdjustable
        public bool IsContractAdjustable(CorporateEventContract pContract)
        {
            return (ArrFunc.IsEmpty(LstCum_AIContractNoAdjusted) || (false == LstCum_AIContractNoAdjusted.Contains(pContract.contractSymbol)));
        }
        #endregion IsContractAdjustable

        #region GetAIContract
        public AIContract GetAIContract<T>(CATools.CAElementTypeEnum pEltType, T pSource)
        {
            AIContract _aiContract = null;
            if (pSource is CorporateEventContract)
            {
                CorporateEventContract _contract = pSource as CorporateEventContract;
                string _lstContractSymbol = GetLstContractSymbol(pEltType);
                object _newContractSymbol = _contract.contractSymbol;
                if (StrFunc.IsFilled(_lstContractSymbol))
                    _newContractSymbol = ReplaceOldValueByNewValue(_contract.contractSymbol, _lstContractSymbol, false);

                if (null != _newContractSymbol)
                {
                    if (CATools.AI_Undo == _newContractSymbol.ToString())
                        _aiContract = new AIContract(CATools.AI_Undo);
                    else
                        _aiContract = GetAIContract(pEltType, _newContractSymbol.ToString());
                }
            }
            else if (pSource is string)
            {
                List<AIContract> _lst = null;
                if (pEltType == CATools.CAElementTypeEnum.Ex)
                    _lst = LstEx_AIContract;
                else if (pEltType == CATools.CAElementTypeEnum.ExAdj)
                    _lst = LstExAdj_AIContract;
                else if (pEltType == CATools.CAElementTypeEnum.ExRecycled)
                    _lst = LstExRecycled_AIContract;
                if (null != _lst)
                    _aiContract = _lst.Find(match => match.Symbol == pSource.ToString() || StrFunc.IsEmpty(match.Symbol) );
            }
            return _aiContract;
        }
        #endregion GetAIContract
        #region GetStringAIContractValue
        public string GetStringAIContractValue<T>(CATools.CAElementTypeEnum pElementType, CATools.CAElementEnum pElement, T pSource)
        {
            string _value = null;
            AIContract _aiContract = GetAIContract(pElementType, pSource);
            if (null != _aiContract)
            {
                switch (pElement)
                {
                    case CATools.CAElementEnum.dsn:
                        _value = _aiContract.DisplayName;
                        break;
                    case CATools.CAElementEnum.isin:
                        _value = _aiContract.IsinCode;
                        break;
                    case CATools.CAElementEnum.sym:
                        _value = _aiContract.Symbol;
                        break;
                    case CATools.CAElementEnum.unlisin:
                        _value = _aiContract.UnlIsinCode;
                        break;
                }
            }
            return _value;
        }
        #endregion GetStringAIContractValue
        #region GetDecimalAIContractValue
        public Nullable<decimal> GetDecimalAIContractValue<T>(CATools.CAElementTypeEnum pElementType, CATools.CAElementEnum pElement, T pSource)
        {
            Nullable<decimal> _value = null;
            AIContract _aiContract = GetAIContract(pElementType, pSource);
            if (null != _aiContract)
            {
                switch (pElement)
                {
                    case CATools.CAElementEnum.cmul:
                        _value = _aiContract.ContractMultiplier;
                        break;
                    case CATools.CAElementEnum.csize:
                        _value = _aiContract.ContractSize;
                        break;
                    case CATools.CAElementEnum.qtymul:
                        _value = _aiContract.QtyMultiplier;
                        break;
                }
            }
            return _value;
        }
        #endregion GetDecimalAIContractValue

        #region AddLstContractUnderlyer
        private void AddLstContractUnderlyer(CATools.CAElementTypeEnum pEltType)
        {
            List<string> _lstUnlIsinCode = new List<string>();
            string _unlIsinCode = string.Empty;
            List<AIContract> _lst = null;
            if (pEltType == CATools.CAElementTypeEnum.Ex)
                _lst = LstEx_AIContract;
            else if (pEltType == CATools.CAElementTypeEnum.ExAdj)
                _lst = LstExAdj_AIContract;
            else if (pEltType == CATools.CAElementTypeEnum.ExRecycled)
                _lst = LstExRecycled_AIContract;
            if (null != _lst)
            {
                _lst.ForEach(item =>
                    {
                        // EG 20160126 [34091] Add StrFunc.IsFilled(item.unlIsinCode)
                        if ((false == LstUnderlyerIsinCode.Contains(item.UnlIsinCode)) && StrFunc.IsFilled(item.UnlIsinCode))
                            LstUnderlyerIsinCode.Add(item.UnlIsinCode);
                    });
            }
        }
        #endregion AddLstContractUnderlyer

        #region InitAIUnderlyer
        public void InitAIUnderlyer()
        {
            LstEx_AIUnderlyer = InitAIUnderlyer(CATools.CAElementTypeEnum.Ex);
            LstExAdj_AIUnderlyer = InitAIUnderlyer(CATools.CAElementTypeEnum.ExAdj);
            LstRecycled_AIUnderlyer = InitAIUnderlyer(CATools.CAElementTypeEnum.ExRecycled);
        }
        /// <summary>
        /// Initialisation des informations additionnelles pour la CA ( côté sous-jacents)
        /// Exemple:
        /// En entrée (_value)         : isin=US0000000001|US0000000002;dsn=AAA shares - Class A|BBB shares - Class C;sym=AAA|BBB;rsym=YYY|ZZZ
        /// Intermédiaire (_lstInput)  : List('isin=US0000000001|US0000000002', 'dsn=AAA shares - Class A|BBB shares - Class C', 'sym=AAA|BBB', 'rsym=YYY|ZZZ' etc...
        ///               (_lstOutput) : List(Pair('isin', ['US0000000001','US0000000002']), Pair('dsn', ['AAA shares - Class A','BBB shares - Class C']),
        ///                              Pair('sym', ['AAA','BBB']), Pair('rsym', ['YYY','ZZZ']) etc...
        /// En sortie                  : List (AIUnderlyer)
        /// </summary>
        /// <param name="pElementType"></param>
        /// <returns></returns>
        public List<AIUnderlyer> InitAIUnderlyer(CATools.CAElementTypeEnum pElementType)
        {
            List<AIUnderlyer> _lstAIUnderlyer = new List<AIUnderlyer>();

            string _value = string.Empty;
            switch (pElementType)
            {
                case CATools.CAElementTypeEnum.ExAdj:
                    _value = GetStringAdditionalInfo("exAdjUnderlyer");
                    break;
                case CATools.CAElementTypeEnum.Ex:
                    _value = GetStringAdditionalInfo("exUnderlyer");
                    break;
                case CATools.CAElementTypeEnum.ExRecycled:
                    _value = GetStringAdditionalInfo("exRecycledUnderlyer");
                    break;
            }
            
            if (StrFunc.IsFilled(_value))
            {
                List<string> _lstInput = new List<string>(_value.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries));
                List<Pair<CATools.CAElementEnum, string[]>> _lstOutPut = new List<Pair<CATools.CAElementEnum, string[]>>();

                if (0 < _lstInput.Count)
                {
                    _lstInput.ForEach(item =>
                    {
                        string[] data = item.Split(new char[] { '=' }, StringSplitOptions.None);
                        for (int i = 0; i < data.Length; i += 2)
                        {
                            CATools.CAElementEnum _enum = (CATools.CAElementEnum)ReflectionTools.EnumParse(new CATools.CAElementEnum(), data[i]);
                            Pair<CATools.CAElementEnum, string[]> _element =
                                new Pair<CATools.CAElementEnum, string[]>(_enum, data[i + 1].Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries));
                            _lstOutPut.Add(_element);
                        }
                    });
                }

                Pair<CATools.CAElementEnum, string[]> _lstIsin = _lstOutPut.Find(item => item.First == CATools.CAElementEnum.isin);
                if (null != _lstIsin)
                {
                    for (int i = 0; i < _lstIsin.Second.Length; i++)
                    {
                        AIUnderlyer _aiUnderlyer = new AIUnderlyer(_lstIsin.Second[i]);

                        foreach (CATools.CAElementEnum _enum in (CATools.CAElementEnum[])System.Enum.GetValues(typeof(CATools.CAElementEnum)))
                        {
                            if (CATools.CAElementEnum.isin != _enum)
                            {
                                Pair<CATools.CAElementEnum, string[]> _lstElement = _lstOutPut.Find(item => item.First == _enum);
                                if ((null != _lstElement) && ArrFunc.IsFilled(_lstElement.Second))
                                {
                                    string _tmp = _lstElement.Second[Math.Max(0, Math.Min(i, _lstElement.Second.Length - 1))];
                                    if (StrFunc.IsFilled(_tmp))
                                    {
                                        switch (_enum)
                                        {
                                            case CATools.CAElementEnum.desc:
                                                _aiUnderlyer.Description = _tmp;
                                                break;
                                            case CATools.CAElementEnum.dsn:
                                                _aiUnderlyer.DisplayName = _tmp;
                                                break;
                                            case CATools.CAElementEnum.isin:
                                                _aiUnderlyer.IsinCode = _tmp;
                                                break;
                                            case CATools.CAElementEnum.sym:
                                                _aiUnderlyer.Symbol = _tmp;
                                                break;
                                            case CATools.CAElementEnum.rsym:
                                                _aiUnderlyer.RelatedSymbol = _tmp;
                                                break;
                                        }
                                    }
                                }
                            }
                        }
                        _lstAIUnderlyer.Add(_aiUnderlyer);
                    }


                    _lstAIUnderlyer.ForEach(item =>
                    {
                        if (false == LstUnderlyerIsinCode.Contains(item.IsinCode))
                            LstUnderlyerIsinCode.Add(item.IsinCode);
                    });

                }
            }
            return _lstAIUnderlyer;
        }
        #endregion InitAIUnderlyer
        #region GetAIUnderlyer
        public AIUnderlyer GetAIUnderlyer(CATools.CAElementTypeEnum pElementType, string pIsinCode)
        {
            AIUnderlyer _aiUnderlyer = null;
            List<AIUnderlyer> _lst = null;
            switch (pElementType)
            {
                case CATools.CAElementTypeEnum.Ex:
                    _lst = LstEx_AIUnderlyer;
                    break;
                case CATools.CAElementTypeEnum.ExAdj:
                    _lst = LstExAdj_AIUnderlyer;
                    break;
                case CATools.CAElementTypeEnum.ExRecycled:
                    _lst = LstRecycled_AIUnderlyer;
                    break;
            }
            if (null != _lst)
                _aiUnderlyer = _lst.Find(match => match.IsinCode == pIsinCode);
            return _aiUnderlyer;
        }
        #endregion GetAIUnderlyer
        #region GetStringAIUnderlyerValue
        public string GetStringAIUnderlyerValue(CATools.CAElementTypeEnum pElementType, CATools.CAElementEnum pElement, string pIsinCode)
        {
            string _value = String.Empty;
            AIUnderlyer _aiUnderlyer = GetAIUnderlyer(pElementType, pIsinCode);
            if (null != _aiUnderlyer)
            {
                switch (pElement)
                {
                    case CATools.CAElementEnum.dsn:
                        _value = _aiUnderlyer.DisplayName;
                        break;
                    case CATools.CAElementEnum.isin:
                        _value = _aiUnderlyer.IsinCode;
                        break;
                    case CATools.CAElementEnum.rsym:
                        _value = _aiUnderlyer.RelatedSymbol;
                        break;
                    case CATools.CAElementEnum.sym:
                        _value = _aiUnderlyer.Symbol;
                        break;
                }
            }
            return _value;
        }
        #endregion GetStringAIUnderlyerValue

        #region GetStringAdditionalInfo
        /// <summary>
        /// Donne la valeur d'un composant additionnel
        /// </summary>
        /// <returns></returns>
        // EG [33415/33420] New
        private string GetStringAdditionalInfo(string pId)
        {
            string _info = string.Empty;
            object _obj = GetAdditionalInfo(pId);
            if (null != _obj)
                _info = _obj.ToString();
            return _info;
        }
        #endregion GetStringAdditionalInfo
        #region GetBooleanAdditionalInfo
        private Nullable<bool> GetBooleanAdditionalInfo(string pId)
        {
            Nullable<bool> _info = null;
            object _obj = GetAdditionalInfo(pId);
            if ((null != _obj) && (_obj is bool x))
                _info = x;
            return _info;
        }
        #endregion GetBooleanAdditionalInfo

        #region ReplaceOldValueByNewValue
        /// <summary>
        /// Donne la valeur du composant matérialisant le contractSymbol  (EX) 
        /// id = exContractSymbol
        /// </summary>
        /// <returns></returns>
        // EG [33415/33420] New a étoffer pour multi-remplacement
        public object ReplaceOldValueByNewValue(string pOldValue, string pNewValue, bool isResetOldValue)
        {
            object newValue = isResetOldValue ? Convert.DBNull : pOldValue;
            if (StrFunc.IsFilled(pNewValue))
            {
                // old=FSA|BPV;new=US|XX
                string[] _expr = pNewValue.Split(';');
                if (ArrFunc.IsFilled(_expr) && (2 == _expr.Length))
                {
                    string[] _oldExpr = _expr[0].Split('=');
                    string[] _newExpr = _expr[1].Split('=');

                    if (ArrFunc.IsFilled(_oldExpr) && (2 == _oldExpr.Length) && 
                        ArrFunc.IsFilled(_newExpr) && (2 == _newExpr.Length))
                    {
                        string[] _oldExpr_item = _oldExpr[1].Split('|');
                        string[] _newExpr_item = _newExpr[1].Split('|');

                        if (ArrFunc.IsFilled(_oldExpr_item) && ArrFunc.IsFilled(_newExpr_item) && 
                            (_oldExpr_item.Length == _newExpr_item.Length))
                        {
                            for (int i = 0; i < _oldExpr_item.Length; i++)
                            {
                                if (pOldValue == _oldExpr_item[i])
                                {
                                    newValue = _newExpr_item[i];
                                    break;
                                }
                            }

                        }

                    }
                }
            }
            return newValue;
        }
        #endregion ReplaceOldValueByNewValue

        #region SetNormMsgFactoryParameters
        public virtual void SetNormMsgFactoryParameters(MQueueparameters pMQueueParameters)
        {
        }
        #endregion SetNormMsgFactoryParameters
        #region SetCumDataToExData
        /// <summary>
        /// Deverse les valeurs CUM dans EX pour DC - DA et ASSET
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="pTarget"></param>
        /// <returns></returns>
        // EG [33415/33420] New
        public virtual Cst.ErrLevel SetCumDataToExData<T>(T pTarget, int pIdA_Entity, int pIdM, ProcessCacheContainer pProcessCacheContainer)
        {
            Cst.ErrLevel ret = Cst.ErrLevel.SUCCESS;

            if (pTarget is CorporateEventContract)
            {
                _currentContract = pTarget as CorporateEventContract;
                _currentContract.exData = new CalculationExData();
                _currentContract.exDataSpecified = true;
                SetCumDataToExData(_currentContract.cumData, _currentContract.exData);

                if (_currentContract.dAttribsSpecified)
                {
                    foreach (CorporateEventDAttrib _dAttrib in _currentContract.dAttribs)
                        SetCumDataToExData(_dAttrib, pIdA_Entity, pIdM, pProcessCacheContainer);
                }
            }
            else if (pTarget is CorporateEventDAttrib)
            {
                _currentDAttrib = pTarget as CorporateEventDAttrib;
                _currentDAttrib.exData = new CalculationExData();
                _currentDAttrib.exDataSpecified = true;
                SetCumDataToExData(_currentDAttrib.cumData, _currentDAttrib.exData);
                if (_currentDAttrib.assetsSpecified)
                {
                    foreach (CorporateEventAsset _asset in _currentDAttrib.assets)
                        SetCumDataToExData(_asset, pIdA_Entity, pIdM, pProcessCacheContainer);
                }
            }
            else if (pTarget is CorporateEventAsset)
            {
                _currentAsset = pTarget as CorporateEventAsset;
                _currentAsset.exData = new CalculationExData();
                _currentAsset.exDataSpecified = true;

                // EG 20131008 On lit le cours de clôture de l'asset CUM pour ensuite le recopier sur EX.
                // GLOP DSP
                if (false == _currentAsset.cumData.dailyClosingPriceSpecified)
                {
                    AdjustmentPrice price = new AdjustmentPrice();
                    price.SetCurrentTarget(_currentAsset);
                    price.SetWorkingDataContainer(this, pIdA_Entity, pIdM, pProcessCacheContainer, false);
                    Nullable<decimal> _dcp = price.CumDailyClosingPrice();
                    _currentAsset.cumData.dailyClosingPriceSpecified = _dcp.HasValue;
                    // EG 20140103 Test dailyClosingPriceSpecified
                    if (_currentAsset.cumData.dailyClosingPriceSpecified)
                        _currentAsset.cumData.dailyClosingPrice = new EFS_Decimal(_dcp.Value);
                }
                SetCumDataToExData(_currentAsset.cumData, _currentAsset.exData);
            }
            return ret;
        }
        /// <summary>
        /// Deverse les valeurs CUM dans EX 
        /// </summary>
        /// <param name="pCumData">Données CUM</param>
        /// <param name="pExData">Données EX</param>
        /// <returns></returns>
        public Cst.ErrLevel SetCumDataToExData(CalculationCumData pCumData, CalculationExData pExData)
        {
            pExData.contractSizeSpecified = pCumData.contractSizeSpecified;
            if (pExData.contractSizeSpecified)
                pExData.contractSize = new SimpleUnit(pCumData.contractSize.DecValue, pCumData.contractSize.DecValue);

            pExData.contractMultiplierSpecified = pCumData.contractMultiplierSpecified;
            if (pExData.contractMultiplierSpecified)
                pExData.contractMultiplier = new SimpleUnit(pCumData.contractMultiplier.DecValue, pCumData.contractMultiplier.DecValue);

            pExData.dailyClosingPriceSpecified = pCumData.dailyClosingPriceSpecified;
            if (pExData.dailyClosingPriceSpecified)
                pExData.dailyClosingPrice = new SimpleUnit(pCumData.dailyClosingPrice.DecValue, pCumData.dailyClosingPrice.DecValue);

            pExData.equalizationPaymentSpecified = false;

            pExData.renamingValueSpecified = pCumData.renamingValueSpecified;
            if (pExData.renamingValueSpecified)
                pExData.renamingValue = pCumData.renamingValue;

            pExData.strikePriceSpecified = pCumData.strikePriceSpecified;
            if (pExData.strikePriceSpecified)
                pExData.strikePrice = new SimpleUnit(pCumData.strikePrice.DecValue, pCumData.strikePrice.DecValue);

            return Cst.ErrLevel.SUCCESS;
        }
        #endregion SetCumDataToExData

        #region EqualisationPayment
        /// <summary>
        /// La position théorique doît être préservée après ajustement de tel sorte que:
        /// ExCSize x ExStrikePrice = CSize x StrikePrice
        /// Si ce n'est pas le cas alors calcul d'un EQUALISATION PAYMENT(lié à l'arrondi du CSize)
        /// Si Payment est négatif alors le détenteur d’une position acheteuse d’options le recevra. 
        /// Si Payment est positif alors le détenteur d’une position vendeuse d’options le recevra.
        /// </summary>
        /// <param name="pCorporateEventAsset"></param>
        // EG 20141106 [20253] Equalisation payment
        // EG 20150114 [20676] Change Test EqualisationPayment cumData replace exData
        // EG 20190121 [23249] Sauvegarde de la règle d'arrondi
        // EG 20211028 [XXXXX] Correction formule finale application du dénominateur 100 car "_varPosition" est exprimé en pourcentage
        public void EqualisationPayment<T>(T pSource,CorporateEventAsset pCorporateEventAsset, ProcessCacheContainer pProcessCacheContainer, int pIdM)
        {
            if (pCorporateEventAsset.IsEqualPaymentMustBeCalculated)
            {
                CalculationCumData cumData = pCorporateEventAsset.cumData;
                CalculationExData exData = pCorporateEventAsset.exData;

                decimal _cumTheoreticalPosition = 0;
                decimal _exTheoreticalPosition = 0;
                Rounding _rounding = null;

                if (pSource is AdjustmentFuture)
                {
                    AdjustmentFuture future = pSource as AdjustmentFuture;
                    _cumTheoreticalPosition = cumData.contractSize.DecValue * cumData.dailyClosingPrice.DecValue;
                    _exTheoreticalPosition = exData.contractSize.valueRounded.DecValue * exData.dailyClosingPrice.valueRounded.DecValue;
                    if (future.equalisationPayment.roundingSpecified)
                        _rounding = future.equalisationPayment.rounding;
                }
                else if (pSource is AdjustmentOption)
                {
                    AdjustmentOption option = pSource as AdjustmentOption;
                    _cumTheoreticalPosition = cumData.contractSize.DecValue * cumData.strikePrice.DecValue;
                    _exTheoreticalPosition = exData.contractSize.valueRounded.DecValue * exData.strikePrice.valueRounded.DecValue;
                    if (option.equalisationPayment.roundingSpecified)
                        _rounding = option.equalisationPayment.rounding;
                }
                if (_cumTheoreticalPosition != _exTheoreticalPosition)
                {
                    // EG 20150114 [20676] Change Test EqualisationPayment cumData replace exData
                    if (pCorporateEventAsset.cumData.dailyClosingPriceSpecified)
                    {
                        CorporateEventMktRules _mktRules = pProcessCacheContainer.GetMktRules(pIdM);

                        decimal rFactor = pCorporateEventAsset.rFactor.valueRounded.DecValue;
                        decimal _cumDailyClosingPrice = pCorporateEventAsset.cumData.dailyClosingPrice.DecValue;
                        decimal _cumCSize = cumData.contractSize.DecValue;
                        decimal _varPosition = ((exData.contractSize.valueRounded.DecValue * rFactor) - _cumCSize) / _cumCSize;

                        // EG 20240530 [WI948][OTRS1000085][26698] Corporate Action: Wrong Equalization Payment
                        // 1. A division by 100 was wrongly applied in the calculation of the value of the position (V)
                        // 2. Rounding to 6 decimal was not applied to this same intermediate calculation(Position valuation = V)
                        _rounding = _mktRules.rounding.Find(item => item.First == AdjustmentElementEnum.RFactor).Second;
                        EFS_Round _roundVarPosition = new EFS_Round(_rounding.direction, _rounding.precision, _varPosition);
                        decimal _equaPayment = (_roundVarPosition.AmountRounded * _cumDailyClosingPrice * _cumCSize);

                        _rounding = _mktRules.rounding.Find(item => item.First == AdjustmentElementEnum.EqualisationPayment).Second;
                        EFS_Round _round = new EFS_Round(_rounding.direction, _rounding.precision, _equaPayment);
                        // Mise à jour Asset
                        pCorporateEventAsset.exData.equalizationPaymentSpecified = true;
                        pCorporateEventAsset.exData.equalizationPayment = new SimpleUnit(_equaPayment, _round.AmountRounded);
                        // EG 20190121 [23249] New
                        pCorporateEventAsset.exData.equalizationPayment.SaveRounding(_rounding);
                    }
                    else
                        pCorporateEventAsset.adjStatus = CATools.AdjStatusEnum.EQUALPAYMENT_DCP_NOTFOUND;
                }
            }
        }
        #endregion EqualisationPayment
        #endregion Methods
    }
    #endregion Ajustment
    #region AdjustmentRatio
    /// <summary>
    /// Caractéristiques des éléments pour la détermination d'un ajustement par la méthode du ratio
    /// ● Eléments de calcul du R-Factor
    /// ● Eléments de calcul des ajustements sur contrat Future
    /// ● Eléments de calcul des ajustements sur contrat Option
    /// </summary>
    [SerializableAttribute()]
    [XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    [XmlRootAttribute("ratio", Namespace = "http://www.efs.org/2007/EFSmL-3-0", IsNullable = false)]
    public class AdjustmentRatio : Adjustment
    {
        #region Members
        [XmlElementAttribute("rFactor", Order = 1)]
        public RFactor rFactor;

        //[XmlIgnoreAttribute()]
        //public bool futureSpecified;
        //[XmlElementAttribute("future", Order = 2)]
        //public AdjustmentFuture future;
        //[XmlIgnoreAttribute()]
        //public bool optionSpecified;
        //[XmlElementAttribute("option", Order = 3)]
        //public AdjustmentOption option;
        #endregion Members
        #region Constructors
        public AdjustmentRatio()
        {
            contract = new AdjustmentContract();
        }
        #endregion Constructors
        #region Methods
        #region SetNormMsgFactoryParameters
        // EG 20141106 [20253] Equalisation payment
        public override void SetNormMsgFactoryParameters(MQueueparameters pMQueueParameters)
        {
            if (null != pMQueueParameters["ADJFUTURE"])
                contract.futureSpecified = pMQueueParameters.GetBoolValueParameterById("ADJFUTURE");
            if (contract.futureSpecified)
            {
                if (null != pMQueueParameters["ADJFUTURECSIZE"])
                {
                    contract.future.contractSizeSpecified = pMQueueParameters.GetBoolValueParameterById("ADJFUTURECSIZE");
                    contract.future.contractMultiplierSpecified = contract.future.contractSizeSpecified;
                }

                if (contract.future.contractSizeSpecified)
                    SetSpecificRounding(contract.future.contractSize, pMQueueParameters, "CONTRACTSIZEROUNDING");

                if (contract.future.contractMultiplierSpecified)
                    SetSpecificRounding(contract.future.contractMultiplier, pMQueueParameters, "CONTRACTMULTIPLIERROUNDING");


                if (null != pMQueueParameters["ADJFUTUREPRICE"])
                    contract.future.priceSpecified = pMQueueParameters.GetBoolValueParameterById("ADJFUTUREPRICE");

                if (contract.future.priceSpecified)
                    SetSpecificRounding(contract.future.price, pMQueueParameters, "PRICEROUNDING");

                contract.future.equalisationPaymentSpecified = (null != pMQueueParameters["ADJFUTUREEQPAYMENT"]);
                if (contract.future.equalisationPaymentSpecified)
                {
                    contract.future.equalisationPayment = new EqualisationPayment();
                    SetSpecificRounding(contract.future.equalisationPayment, pMQueueParameters, "EQPAYMENTROUNDING");
                }

            }

            if (null != pMQueueParameters["ADJOPTION"])
                contract.optionSpecified = pMQueueParameters.GetBoolValueParameterById("ADJOPTION");
            if (contract.optionSpecified)
            {
                if (null != pMQueueParameters["ADJOPTIONCSIZE"])
                {
                    contract.option.contractSizeSpecified = pMQueueParameters.GetBoolValueParameterById("ADJOPTIONCSIZE");
                    contract.option.contractMultiplierSpecified = contract.option.contractSizeSpecified;
                }

                if (contract.option.contractSizeSpecified)
                    SetSpecificRounding(contract.option.contractSize, pMQueueParameters, "CONTRACTSIZEROUNDING");

                // EG 20160222
                if (contract.option.contractMultiplierSpecified)
                    SetSpecificRounding(contract.option.contractMultiplier, pMQueueParameters, "CONTRACTMULTIPLIERROUNDING");

                if (null != pMQueueParameters["ADJOPTIONSTRIKEPRICE"])
                    contract.option.strikePriceSpecified = pMQueueParameters.GetBoolValueParameterById("ADJOPTIONSTRIKEPRICE");

                if (contract.option.strikePriceSpecified)
                    SetSpecificRounding(contract.option.strikePrice, pMQueueParameters, "STRIKEPRICEROUNDING");

                if (null != pMQueueParameters["ADJOPTIONPRICE"])
                    contract.option.priceSpecified = pMQueueParameters.GetBoolValueParameterById("ADJOPTIONPRICE");

                if (contract.option.priceSpecified)
                    SetSpecificRounding(contract.option.price, pMQueueParameters, "PRICEROUNDING");

                contract.option.equalisationPaymentSpecified = (null != pMQueueParameters["ADJOPTIONEQPAYMENT"]);
                if (contract.option.equalisationPaymentSpecified)
                {
                    contract.option.equalisationPayment = new EqualisationPayment();
                    SetSpecificRounding(contract.option.equalisationPayment, pMQueueParameters, "EQPAYMENTROUNDING");
                }
            }
        }
        #endregion SetNormMsgFactoryParameters
        #region GetCumAndExData
        /// <summary>
        /// Copie le ContractSize/ContractMultiplier CUM dans 
        /// // ContractSize/ContractMultiplier (EX) si non ajustement de cet éléement spécifié sur la notice
        /// </summary>
        /// <typeparam name="T">Type de la source (CorpoEventContract / CorpoEventDAttrib / CorpoEventAsset</typeparam>
        /// <param name="pTarget">source</param>
        private void GetCumAndExData<T>(T pTarget, ref CalculationCumData pCumData, ref CalculationExData pExData)
        {
            CalculationCumData _cumData = null;
            CalculationExData _exData = null;
            if (pTarget is CorporateEventContract)
            {
                CorporateEventContract _target = pTarget as CorporateEventContract;
                _cumData = _target.cumData;
                if (false == _target.exDataSpecified)
                {
                    _target.exData = new CalculationExData();
                    _target.exDataSpecified = true;
                }
                _exData = _target.exData;
            }
            else if (pTarget is CorporateEventDAttrib)
            {
                CorporateEventDAttrib _target = pTarget as CorporateEventDAttrib;
                _cumData = _target.cumData;
                if (false == _target.exDataSpecified)
                {
                    _target.exData = new CalculationExData();
                    _target.exDataSpecified = true;
                }
                _exData = _target.exData;
            }
            else if (pTarget is CorporateEventAsset)
            {
                CorporateEventAsset _target = pTarget as CorporateEventAsset;
                _cumData = _target.cumData;
                if (false == _target.exDataSpecified)
                {
                    _target.exData = new CalculationExData();
                    _target.exDataSpecified = true;
                }
                _exData = _target.exData;
            }
            pCumData = _cumData;
            pExData = _exData;
        }
        #endregion GetCumAndExData
        #region SetCumContractMultiplierToEx
        /// <summary>
        /// Copie le ContractSize/ContractMultiplier CUM dans 
        /// // ContractSize/ContractMultiplier (EX) si non ajustement de cet éléement spécifié sur la notice
        /// </summary>
        /// <typeparam name="T">Type de la source (CorpoEventContract / CorpoEventDAttrib / CorpoEventAsset</typeparam>
        /// <param name="pTarget">source</param>
        public override void SetCumContractMultiplierToEx<T>(T pTarget)
        {
            CalculationCumData _cumData = null;
            CalculationExData _exData = null;
            GetCumAndExData(pTarget, ref _cumData, ref _exData);
            if ((null != _exData) && (null != _cumData))
            {
                _exData.contractMultiplierSpecified = _cumData.contractMultiplierSpecified;
                if (_exData.contractMultiplierSpecified)
                {
                    decimal _contractMultiplier = _cumData.contractMultiplier.DecValue;
                    _exData.contractMultiplier = new SimpleUnit(_contractMultiplier, _contractMultiplier);
                }
            }
        }
        #endregion SetCumContractMultiplierToEx
        #region SetCumContractSizeToEx
        /// <summary>
        /// Copie le ContractSize/ContractMultiplier CUM dans 
        /// // ContractSize/ContractMultiplier (EX) si non ajustement de cet éléement spécifié sur la notice
        /// </summary>
        /// <typeparam name="T">Type de la source (CorpoEventContract / CorpoEventDAttrib / CorpoEventAsset</typeparam>
        /// <param name="pTarget">source</param>
        public override void SetCumContractSizeToEx<T>(T pTarget)
        {
            CalculationCumData _cumData = null;
            CalculationExData _exData = null;
            GetCumAndExData(pTarget, ref _cumData, ref _exData);
            if ((null != _exData) && (null != _cumData))
            {
                _exData.contractSizeSpecified = _cumData.contractSizeSpecified;
                if (_exData.contractSizeSpecified)
                {
                    decimal _contractSize = _cumData.contractSize.DecValue;
                    _exData.contractSize = new SimpleUnit(_contractSize, _contractSize);
                }
            }
        }
        #endregion SetCumContractSizeToEx
        #region SetSpecificRounding
        private void SetSpecificRounding<T>(T pTarget, MQueueparameters pMQueueParameters, string pParameterName)
        {
            Rounding _rounding = null;
            if (pTarget is AdjustmentElement _adjustedElement)
            {
                _adjustedElement.roundingSpecified = (null != pMQueueParameters[pParameterName]);
                if (_adjustedElement.roundingSpecified)
                    _adjustedElement.rounding = new Rounding();
                _rounding = _adjustedElement.rounding;
            }
            else if (pTarget is EqualisationPayment _equalisationPayment)
            {
                _equalisationPayment.roundingSpecified = (null != pMQueueParameters[pParameterName]);
                if (_equalisationPayment.roundingSpecified)
                    _equalisationPayment.rounding = new Rounding();
                _rounding = _equalisationPayment.rounding;
            }

            if (null != _rounding)
            {
                MQueueparameter _parameter = pMQueueParameters[pParameterName];
                _rounding.precision = Convert.ToInt32(_parameter.Value);
                if (_parameter.ExValueSpecified)
                    _rounding.direction = CATools.CERoundingDirection(_parameter.ExValue);
            }
        }
        #endregion SetSpecificRounding

        #endregion Methods
    }
    #endregion AdjustmentRatio
    #region AdjustmentFairValue
    /// <summary>
    /// Caractéristiques des éléments pour la détermination d'un ajustement par la méthode de fair value (TBD)
    /// </summary>
    [SerializableAttribute()]
    [XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    [XmlRootAttribute("fairValue", Namespace = "http://www.efs.org/2007/EFSmL-3-0", IsNullable = false)]
    public class AdjustmentFairValue : Adjustment
    {
        #region Constructors
        public AdjustmentFairValue()
        {
            contract = new AdjustmentContract();
        }
        #endregion Constructors
    }
    #endregion AdjustmentFairValue
    #region AdjustmentPackage
    /// <summary>
    /// Caractéristiques des éléments pour la détermination d'un ajustement par la méthode du package, Panier (TBD)
    /// </summary>
    [SerializableAttribute()]
    [XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    [XmlRootAttribute("package", Namespace = "http://www.efs.org/2007/EFSmL-3-0", IsNullable = false)]
    public class AdjustmentPackage : Adjustment
    {
        #region Constructors
        public AdjustmentPackage()
        {
            contract = new AdjustmentContract();
        }
        #endregion Constructors
    }
    #endregion AdjustmentPackage
    #region AdjustmentNone
    /// <summary>
    /// Pas d'ajustement
    /// </summary>
    [SerializableAttribute()]
    [XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    [XmlRootAttribute("noAdjustment", Namespace = "http://www.efs.org/2007/EFSmL-3-0", IsNullable = false)]
    public class AdjustmentNone : Adjustment
    {
        #region Constructors
        public AdjustmentNone()
        {
            contract = new AdjustmentContract();
        }
        #endregion Constructors
    }
    #endregion AdjustmentNone

    /* -------------------------------------------------------- */
    /* ----- CLASSES ELEMENTS AJUSTES                     ----- */
    /* -------------------------------------------------------- */

    #region AdjustmentFuture
    /// <summary>
    /// Caractéristiques des éléments de détermination des ajustements pour les contrats Future
    /// ● Ajustement du Contract Size
    /// ● Ajustement du cours
    /// </summary>
    [SerializableAttribute()]
    [XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public class AdjustmentFuture
    {
        #region Members
        [XmlIgnoreAttribute()]
        public bool contractSizeSpecified;
        [XmlElementAttribute("contractSize", Order = 1)]
        public AdjustmentContractSize contractSize;
        [XmlIgnoreAttribute()]
        public bool contractMultiplierSpecified;
        [XmlElementAttribute("contractMultiplier", Order = 2)]
        public AdjustmentContractMultiplier contractMultiplier;
        [XmlIgnoreAttribute()]
        public bool priceSpecified;
        [XmlElementAttribute("price", Order = 3)]
        public AdjustmentPrice price;
        [XmlIgnoreAttribute()]
        public bool equalisationPaymentSpecified;
        [XmlElementAttribute("equalisationPayment", Order = 4)]
        public EqualisationPayment equalisationPayment;
        #endregion Members

        #region Methods
        #region Evaluate
        /// <summary>
        /// Evaluation des éléments futures (ContractSize, DailySettlementPrice)
        /// </summary>
        // EG 20141106 [20253] Equalisation payment
        public void Evaluate(Adjustment pAdjustment, CorporateEventContract pContract, int pIdA_Entity, int pIdM, ProcessCacheContainer pProcessCacheContainer, bool pIsMessageSupplied)
        {
            if (contractSizeSpecified)
                contractSize.Evaluate(pAdjustment, pContract, pIdA_Entity, pIdM, pProcessCacheContainer, pIsMessageSupplied);
            else
                pAdjustment.SetCumContractSizeToEx(pContract);

            if (contractMultiplierSpecified)
                contractMultiplier.Evaluate(pAdjustment, pContract, pIdA_Entity, pIdM, pProcessCacheContainer, pIsMessageSupplied);
            else
                pAdjustment.SetCumContractMultiplierToEx(pContract);

            if (pContract.dAttribsSpecified && CATools.IsAdjStatusOK(pContract.adjStatus))
            {
                foreach (CorporateEventDAttrib _dAttrib in pContract.dAttribs)
                {
                    if (pAdjustment is AdjustmentRatio)
                    {
                        _dAttrib.rFactorSpecified = pContract.rFactorSpecified;
                        _dAttrib.rFactor = pContract.rFactor;
                        _dAttrib.rFactorRetained = pContract.rFactorRetained;
                    }

                    if (contractSizeSpecified)
                        contractSize.Evaluate(pAdjustment, _dAttrib, pIdA_Entity, pIdM, pProcessCacheContainer, pIsMessageSupplied);
                    else
                        pAdjustment.SetCumContractSizeToEx(_dAttrib);


                    if (contractMultiplierSpecified)
                        contractMultiplier.Evaluate(pAdjustment, _dAttrib, pIdA_Entity, pIdM, pProcessCacheContainer, pIsMessageSupplied);
                    else
                        pAdjustment.SetCumContractMultiplierToEx(_dAttrib);

                    if (_dAttrib.assetsSpecified)
                    {
                        foreach (CorporateEventAsset _asset in _dAttrib.assets)
                        {
                            if (pAdjustment is AdjustmentRatio)
                            {
                                _asset.rFactorSpecified = pContract.rFactorSpecified;
                                _asset.rFactor = pContract.rFactor;
                                _asset.rFactorRetained = pContract.rFactorRetained;
                            }

                            if (contractSizeSpecified)
                                contractSize.Evaluate(pAdjustment, _asset, pIdA_Entity, pIdM, pProcessCacheContainer, pIsMessageSupplied);
                            else
                                pAdjustment.SetCumContractSizeToEx(_asset);

                            if (contractMultiplierSpecified)
                                contractMultiplier.Evaluate(pAdjustment, _asset, pIdA_Entity, pIdM, pProcessCacheContainer, pIsMessageSupplied);
                            else
                                pAdjustment.SetCumContractMultiplierToEx(_asset);

                            if (priceSpecified && CATools.IsAdjStatusOK(_asset.adjStatus))
                                price.Evaluate(pAdjustment, _asset, pIdA_Entity, pIdM, pProcessCacheContainer, pIsMessageSupplied);

                            if (CATools.IsAdjStatusOK(_asset.adjStatus) && equalisationPaymentSpecified)
                                pAdjustment.EqualisationPayment(this, _asset, pProcessCacheContainer, pIdM);

                        }
                    }
                }
            }
        }
        #endregion Evaluate
        #endregion Methods
    }
    #endregion AdjustmentFuture
    #region AdjustmentOption
    /// <summary>
    /// Caractéristiques des éléments de détermination des ajustements pour les contrats Option
    /// ● Contract Size
    /// ● Strike
    /// </summary>
    [SerializableAttribute()]
    [XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public class AdjustmentOption
    {
        #region Members
        [XmlIgnoreAttribute()]
        public bool contractSizeSpecified;
        [XmlElementAttribute("contractSize", Order = 1)]
        public AdjustmentContractSize contractSize;
        [XmlIgnoreAttribute()]
        public bool contractMultiplierSpecified;
        [XmlElementAttribute("contractMultiplier", Order = 2)]
        public AdjustmentContractMultiplier contractMultiplier;
        [XmlIgnoreAttribute()]
        public bool strikePriceSpecified;
        [XmlElementAttribute("strikePrice", Order = 3)]
        public AdjustmentStrikePrice strikePrice;
        [XmlIgnoreAttribute()]
        public bool priceSpecified;
        [XmlElementAttribute("price", Order = 4)]
        public AdjustmentPrice price;
        [XmlIgnoreAttribute()]
        public bool equalisationPaymentSpecified;
        [XmlElementAttribute("equalisationPayment", Order = 5)]
        public EqualisationPayment equalisationPayment;
        
        #endregion Members
        #region Methods
        #region Evaluate
        /// <summary>
        /// Evaluation des éléments futures (ContractSize, DailySettlementPrice)
        /// </summary>
        // EG 20141106 [20253] Equalisation payment
        public void Evaluate(Adjustment pAdjustment, CorporateEventContract pContract, int pIdA_Entity, int pIdM, ProcessCacheContainer pProcessCacheContainer, bool pIsMessageSupplied)
        {
            if (contractSizeSpecified)
                contractSize.Evaluate(pAdjustment, pContract, pIdA_Entity, pIdM, pProcessCacheContainer, pIsMessageSupplied);
            else
                pAdjustment.SetCumContractSizeToEx(pContract);

            if (contractMultiplierSpecified)
                contractMultiplier.Evaluate(pAdjustment, pContract, pIdA_Entity, pIdM, pProcessCacheContainer, pIsMessageSupplied);
            else
                pAdjustment.SetCumContractMultiplierToEx(pContract);

            if (pContract.dAttribsSpecified && CATools.IsAdjStatusOK(pContract.adjStatus))
            {
                foreach (CorporateEventDAttrib _dAttrib in pContract.dAttribs)
                {
                    if (pAdjustment is AdjustmentRatio)
                    {
                        _dAttrib.rFactorSpecified = pContract.rFactorSpecified;
                        _dAttrib.rFactor = pContract.rFactor;
                        _dAttrib.rFactorRetained = pContract.rFactorRetained;
                    }

                    if (contractSizeSpecified)
                        contractSize.Evaluate(pAdjustment, _dAttrib, pIdA_Entity, pIdM, pProcessCacheContainer, pIsMessageSupplied);
                    else
                        pAdjustment.SetCumContractSizeToEx(_dAttrib);

                    if (contractMultiplierSpecified)
                        contractMultiplier.Evaluate(pAdjustment, _dAttrib, pIdA_Entity, pIdM, pProcessCacheContainer, pIsMessageSupplied);
                    else
                        pAdjustment.SetCumContractMultiplierToEx(_dAttrib);

                    if (_dAttrib.assetsSpecified && CATools.IsAdjStatusOK(_dAttrib.adjStatus))
                    {
                        foreach (CorporateEventAsset _asset in _dAttrib.assets)
                        {
                            if (pAdjustment is AdjustmentRatio)
                            {
                                _asset.rFactorSpecified = pContract.rFactorSpecified;
                                _asset.rFactor = pContract.rFactor;
                                _asset.rFactorRetained = pContract.rFactorRetained;
                            }

                            if (contractSizeSpecified)
                                contractSize.Evaluate(pAdjustment, _asset, pIdA_Entity, pIdM, pProcessCacheContainer, pIsMessageSupplied);
                            else
                                pAdjustment.SetCumContractSizeToEx(_asset);

                            if (contractMultiplierSpecified)
                                contractMultiplier.Evaluate(pAdjustment, _asset, pIdA_Entity, pIdM, pProcessCacheContainer, pIsMessageSupplied);
                            else
                                pAdjustment.SetCumContractMultiplierToEx(_asset);

                            if (strikePriceSpecified && CATools.IsAdjStatusOK(_asset.adjStatus))
                                strikePrice.Evaluate(pAdjustment, _asset, pIdA_Entity, pIdM, pProcessCacheContainer, pIsMessageSupplied);

                            // EG 20150114 [20676] Add Test FuturesStyleMarkToMarket
                            // EG 20150217 [20775] Add Test equalisationPaymentSpecified
                            if (priceSpecified && CATools.IsAdjStatusOK(_asset.adjStatus) && 
                                ((_asset.futValuationMethod == FuturesValuationMethodEnum.FuturesStyleMarkToMarket) || equalisationPaymentSpecified))
                                price.Evaluate(pAdjustment, _asset, pIdA_Entity, pIdM, pProcessCacheContainer, pIsMessageSupplied);

                            if (CATools.IsAdjStatusOK(_asset.adjStatus) && equalisationPaymentSpecified)
                                pAdjustment.EqualisationPayment(this, _asset, pProcessCacheContainer, pIdM);
                        }
                    }
                }
            }

        }

        #endregion Evaluate
        #endregion Methods

    }
    #endregion AdjustmentOption

    #region AdjustmentElement
    /// <summary>
    /// Classe de base étendue aux différents éléments d'ajustement
    /// Eléments:
    /// ● R-Factor
    /// ● Contract Size
    /// ● Strike
    /// ● Price
    /// </summary>
    /// EG 20240105 [WI756] Spheres Core : Refactoring Code Analysis - Correctifs après tests (property Id - Attribute name)
    [SerializableAttribute()]
    [XmlIncludeAttribute(typeof(RFactor))]
    [XmlIncludeAttribute(typeof(AdjustmentStrikePrice))]
    [XmlIncludeAttribute(typeof(AdjustmentPrice))]
    [XmlIncludeAttribute(typeof(AdjustmentContractSize))]
    [XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public abstract partial class AdjustmentElement : Component
    {
        #region Members
        [XmlAttributeAttribute(DataType = "normalizedString")]
        public string name;
        [XmlAttributeAttribute("id", DataType = "ID")]
        public string id;
        [XmlIgnoreAttribute()]
        public bool roundingSpecified;
        [XmlElementAttribute("rounding", Order = 1)]
        public Rounding rounding;

        [XmlIgnoreAttribute()]
        protected CorporateEventContract _currentContract;
        [XmlIgnoreAttribute()]
        protected CorporateEventAsset _currentAsset;
        [XmlIgnoreAttribute()]
        protected CorporateEventDAttrib _currentDAttrib;
        [XmlIgnoreAttribute()]
        protected object _target;
        [XmlIgnoreAttribute()]
        protected CAWorkingDataContainer _workingDataContainer;
        #endregion Members

        #region Accessors
        [XmlIgnoreAttribute()]
        public CAWorkingDataContainer WorkingDataContainer
        {
            get { return _workingDataContainer; }
            set { _workingDataContainer = value; }
        }
        #region Message
        [XmlIgnoreAttribute()]
        public string Message
        {
            get 
            { 
                return _workingDataContainer.Message; 
            }
        }
        #endregion Message
        #region Result
        /// <summary>
        /// Résultat final
        /// </summary>
        [XmlIgnoreAttribute()]
        public Nullable<decimal> Result
        {
            get { return component[0].Result; }
        }
        #endregion Result
        #region Rounding
        /// <summary>
        /// Arrondi utilisé qui est soit celui:
        /// ● spécifié directement sur les procédures de la CA 
        /// ● spécifié pour le marché (CORPOMKTRULES)
        /// ● sinon la valeur par défault (CATools.DefaultMarketRules)
        /// </summary>
        [XmlIgnoreAttribute()]
        public Rounding Rounding
        {
            get
            {
                Rounding _rounding = rounding;
                // Si pas de règle d'arrondi spécifique alors on prend celle par défaut spécifiée sur le marché
                if (false == roundingSpecified)
                {
                    List<Pair<AdjustmentElementEnum, Rounding>> _mktRulesdRounding = _workingDataContainer.MktRulesRounding;
                    if (null != _mktRulesdRounding)
                    {
                        if (this is RFactor)
                            _rounding = _mktRulesdRounding.Find(item => item.First == AdjustmentElementEnum.RFactor).Second;
                        else if (this is AdjustmentContractSize)
                            _rounding = _mktRulesdRounding.Find(item => item.First == AdjustmentElementEnum.ContractSize).Second;
                        else if (this is AdjustmentContractMultiplier)
                            _rounding = _mktRulesdRounding.Find(item => item.First == AdjustmentElementEnum.ContractMultiplier).Second;
                        else if (this is AdjustmentPrice)
                            _rounding = _mktRulesdRounding.Find(item => item.First == AdjustmentElementEnum.Price).Second;
                        else if (this is AdjustmentStrikePrice)
                            _rounding = _mktRulesdRounding.Find(item => item.First == AdjustmentElementEnum.StrikePrice).Second;
                    }
                }
                return _rounding;
            }
        }
        #endregion Rounding
        #region ResultRounded
        /// <summary>
        /// Résultat final
        /// </summary>
        [XmlIgnoreAttribute()]
        public Nullable<decimal> ResultRounded
        {
            get  {return component[0].ResultRounded;}
        }
        #endregion ResultRounded
        #endregion Accessors
        #region Methods
        #region CumContractSize
        /// <summary>
        /// Retourne la taille du contrat (CUM). 
        /// Méthode invoquée avec REFLECTION par l'automate de calcul des formules
        /// </summary>
        /// <returns>Résultat</returns>
        public Nullable<decimal> CumContractSize()
        {
            Nullable<decimal> ret = null;
            if (_target is CorporateEventContract && _currentContract.cumData.contractSizeSpecified)
                ret = _currentContract.cumData.contractSize.DecValue;
            else if (_target is CorporateEventAsset && _currentAsset.cumData.contractSizeSpecified)
                ret = _currentAsset.cumData.contractSize.DecValue;
            else if (_target is CorporateEventDAttrib && _currentDAttrib.cumData.contractSizeSpecified)
                ret = _currentDAttrib.cumData.contractSize.DecValue;
            return ret;
        }
        #endregion CumContractSize
        #region CumContractMultiplier
        /// <summary>
        /// Retourne le contractmultiplier (CUM). 
        /// Méthode invoquée avec REFLECTION par l'automate de calcul des formules
        /// </summary>
        /// <returns>Résultat</returns>
        public Nullable<decimal> CumContractMultiplier()
        {
            Nullable<decimal> ret = null;
            if (_target is CorporateEventContract && _currentContract.cumData.contractMultiplierSpecified)
                ret = _currentContract.cumData.contractMultiplier.DecValue;
            else if (_target is CorporateEventAsset && _currentAsset.cumData.contractMultiplierSpecified)
                ret = _currentAsset.cumData.contractMultiplier.DecValue;
            else if (_target is CorporateEventDAttrib && _currentDAttrib.cumData.contractMultiplierSpecified)
                ret = _currentDAttrib.cumData.contractMultiplier.DecValue;
            return ret;
        }
        #endregion CumContractMultiplier
        #region CumDailyClosingPrice
        /// <summary>
        /// Retourne le prix d'exercice (CUM). 
        /// Méthode invoquée avec REFLECTION par l'automate de calcul des formules
        /// </summary>
        /// <returns>Résultat</returns>
        public Nullable<decimal> CumDailyClosingPrice()
        {
            Nullable<decimal> ret = null;
            if (_target is CorporateEventAsset)
            {
                if (_currentAsset.cumData.dailyClosingPriceSpecified)
                    ret = _currentAsset.cumData.dailyClosingPrice.DecValue;
                else
                {
                    // Lecture du Daily Closing Price
                    ret = ReadQuote();
                    _currentAsset.cumData.dailyClosingPriceSpecified = ret.HasValue;
                    if (ret.HasValue)
                        _currentAsset.cumData.dailyClosingPrice = new EFS_Decimal(ret.Value);
                }
            }
            return ret;
        }
        #endregion CumDailyClosingPrice
        #region CumStrikePrice
        /// <summary>
        /// Retourne le prix d'exercice (CUM). 
        /// Méthode invoquée avec REFLECTION par l'automate de calcul des formules
        /// </summary>
        /// <returns>Résultat</returns>
        public Nullable<decimal> CumStrikePrice()
        {
            Nullable<decimal> ret = null;
            if (_target is CorporateEventAsset)
            {
                if (_currentAsset.cumData.strikePriceSpecified)
                    ret = _currentAsset.cumData.strikePrice.DecValue;
            }
            return ret;
        }
        #endregion CumStrikePrice
        #region ReadQuote
        // EG 20130719 Log avec Warning si EOD
        // EG 20140422 Log avec Warning si CLOSINGDAY
        // EG 20190114 Add detail to ProcessLog Refactoring
        // EG 20190716 [VCL : New FixedIncome] Add (KeyQuoteAdditional parameter)
        private Nullable<decimal> ReadQuote()
        {
            Nullable<decimal> ret = null;
            if (_target is CorporateEventAsset)
            {
                // Lecture du prix 
                //bool isEOD = (_workingDataContainer.PosRequestType.HasValue &&  _workingDataContainer.PosRequestType.Value == Cst.PosRequestTypeEnum.EndOfDay);
                SystemMSGInfo errReadOfficialClose = null;
                IPosKeepingMarket _entityMarketInfo = _workingDataContainer.EntityMarketInfo;
                DateTime _cumDate = _workingDataContainer.ProcessCacheContainer.CACumDate;
                Quote quote = _workingDataContainer.ProcessCacheContainer.GetQuote(_currentAsset.idASSET, _cumDate, _currentAsset.identifier,
                    QuotationSideEnum.OfficialClose, Cst.UnderlyingAsset.ExchangeTradedContract, new KeyQuoteAdditional(), ref errReadOfficialClose);
                if ((null != quote) && quote.valueSpecified)
                    ret = quote.value;
                else
                {
                    if (null != errReadOfficialClose)
                    {
                        //_workingDataContainer.ProcessCacheContainer.LogWithData.Invoke(isEOD?ProcessStateTools.StatusWarningEnum:errReadOfficialClose.processState.Status,
                        //    LogLevel.DEFAULT, 2, errReadOfficialClose.identifier, false, null, errReadOfficialClose.datas);
                        // FI 20200623 [XXXXX] call SetErrorWarning
                        _workingDataContainer.ProcessCacheContainer.SetErrorWarning(ProcessStateTools.StatusWarningEnum);
                        

                        
                        Logger.Log(new LoggerData(LogLevelEnum.Warning, errReadOfficialClose.SysMsgCode, 2, errReadOfficialClose.LogParamDatas));
                    }
                }
            }
            return ret;
        }
        #endregion ReadQuote

        #region SetCurrentTarget
        public void SetCurrentTarget<T>(T pTarget)
        {
            _target = pTarget;
            if (_target is CorporateEventContract)
                _currentContract = pTarget as CorporateEventContract;
            else if (_target is CorporateEventDAttrib)
                _currentDAttrib = pTarget as CorporateEventDAttrib;
            if (_target is CorporateEventAsset)
                _currentAsset = pTarget as CorporateEventAsset;
        }
        #endregion SetCurrentTarget

        #region Evaluate
        /// <summary>
        /// Evaluation des éléments futures (ContractSize, StrikePrice, DailySettlementPrice)
        /// </summary>
        public Cst.ErrLevel Evaluate<T>(Adjustment pAdjustment, T pTarget, int pIdA_Entity, int pIdM, ProcessCacheContainer pProcessCacheContainer, bool pIsMessageSupplied)
        {
            Cst.ErrLevel ret = Cst.ErrLevel.SUCCESS;
            _target = pTarget;

            CalculationExData exData = null;
            Nullable<int> _contractRoundingPrec = null;
            if (_target is CorporateEventContract)
            {
                _currentContract = pTarget as CorporateEventContract;
                if (false == _currentContract.exDataSpecified)
                {
                    _currentContract.exData = new CalculationExData();
                    _currentContract.exDataSpecified = true;
                }
                exData = _currentContract.exData;
                _ = _currentContract.cumData;
            }
            else if (_target is CorporateEventAsset)
            {
                _currentAsset = pTarget as CorporateEventAsset;
                if (false == _currentAsset.exDataSpecified)
                {
                    _currentAsset.exData = new CalculationExData();
                    _currentAsset.exDataSpecified = true;
                }
                exData = _currentAsset.exData;
                _ = _currentAsset.cumData;
            }
            else if (_target is CorporateEventDAttrib)
            {
                _currentDAttrib = pTarget as CorporateEventDAttrib;
                if (false == _currentDAttrib.exDataSpecified)
                {
                    _currentDAttrib.exData = new CalculationExData();
                    _currentDAttrib.exDataSpecified = true;
                }
                exData = _currentDAttrib.exData;
                _ = _currentDAttrib.cumData;
            }

            SetWorkingDataContainer(pAdjustment, pIdA_Entity, pIdM, pProcessCacheContainer, pIsMessageSupplied);

            if (_target is CorporateEventUnderlyer[])
            {
                _workingDataContainer.AddDictionaryComponent(pTarget);
                ret = CATools.EvaluateComponent(_target, _workingDataContainer);
            }


            if (Cst.ErrLevel.SUCCESS == ret)
            {
                // Evaluation des composants
                _workingDataContainer.SetMessageTitle(pAdjustment);
                ret = CATools.EvaluateComponent(this, _workingDataContainer);
                CATools.SetAdjStatus(this, _target, ret);
            }

            // Résultat final + Arrondi 
            CATools.ApplyRounding(this, _contractRoundingPrec);
            Nullable<decimal> _result = Result;
            Nullable<decimal> _resultRounded = ResultRounded;
            if (this is AdjustmentContractSize)
            {
                exData.contractSizeSpecified = _result.HasValue;
                if (exData.contractSizeSpecified)
                    exData.contractSize = new SimpleUnit(_result.Value, (_resultRounded ?? _result.Value));
            }
            else if (this is AdjustmentContractMultiplier)
            {
                exData.contractMultiplierSpecified = _result.HasValue;
                if (exData.contractMultiplierSpecified)
                    exData.contractMultiplier = new SimpleUnit(_result.Value, (_resultRounded ?? _result.Value));
            }
            else if (this is AdjustmentStrikePrice)
            {
                exData.strikePriceSpecified = _result.HasValue;
                if (exData.strikePriceSpecified)
                    exData.strikePrice = new SimpleUnit(_result.Value, (_resultRounded ?? _result.Value));
            }
            else if (this is AdjustmentPrice)
            {
                exData.dailyClosingPriceSpecified = _result.HasValue;
                if (exData.dailyClosingPriceSpecified)
                    exData.dailyClosingPrice = new SimpleUnit(_result.Value, (_resultRounded ?? _result.Value));
            }
            else if (this is RFactor)
            {
                RFactor rFactor = this as RFactor;
                ret = rFactor.SetRatioRetained(ret);

                _workingDataContainer.SetMessageResult(rFactor, ret);
                // Evaluation des composants pour la mise en place du R-Factor
                if (pAdjustment.takePlaceSpecified)
                {
                    ret = CATools.EvaluateComponent(pAdjustment.takePlace, _workingDataContainer);
                    CATools.SetAdjStatus(pAdjustment.takePlace, _target, ret);
                    // Résultat condition de mise en place
                    pAdjustment.takePlace.SetResponse();
                    _workingDataContainer.SetMessageResult(pAdjustment.takePlace);
                }
                // Affichage du ratio certifié
                if (rFactor.rFactorCertifiedSpecified)
                    _workingDataContainer.SetMessageResult(rFactor.rFactorCertified);
            }
            else
                ret = Cst.ErrLevel.UNDEFINED;

            return ret;
        }
        #endregion Evaluate

        #region SetWorkingDataContainer
        /// <summary>
        /// Initialisation
        /// </summary>
        public void SetWorkingDataContainer(Adjustment pAdjustment, int pIdA_Entity, int pIdM, ProcessCacheContainer pProcessCacheContainer, bool pIsMessageSupplied)
        {
            _workingDataContainer = new CAWorkingDataContainer(pAdjustment, pIdA_Entity, pIdM, pProcessCacheContainer, pIsMessageSupplied);
        }
        #endregion SetWorkingDataContainer
        #endregion Methods

    }
    #endregion AdjustmentElement
    #region AdjustmentContractSize
    /// <summary>
    /// Caractéristiques des éléments d'ajustement du contractSize
    /// </summary>
    [SerializableAttribute()]
    [XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    [XmlRootAttribute("contractSize", Namespace = "http://www.efs.org/2007/EFSmL-3-0", IsNullable = false)]
    public class AdjustmentContractSize : AdjustmentElement
    {
    }
    #endregion AdjustmentContractSize
    #region AdjustmentContractMultiplier
    /// <summary>
    /// Caractéristiques des éléments d'ajustement du contractMultiplier
    /// </summary>
    [SerializableAttribute()]
    [XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    [XmlRootAttribute("contractMultiplier", Namespace = "http://www.efs.org/2007/EFSmL-3-0", IsNullable = false)]
    public class AdjustmentContractMultiplier : AdjustmentElement
    {
    }
    #endregion AdjustmentContractMultiplier
    #region AdjustmentPrice
    /// <summary>
    /// Caractéristiques des éléments d'ajustement du cours
    /// </summary>
    [SerializableAttribute()]
    [XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    [XmlRootAttribute("price", Namespace = "http://www.efs.org/2007/EFSmL-3-0", IsNullable = false)]
    public class AdjustmentPrice : AdjustmentElement
    {
    }
    #endregion AdjustmentPrice
    #region AdjustmentStrikePrice
    /// <summary>
    /// Caractéristiques des éléments d'ajustement du prix d'exercice
    /// </summary>
    [SerializableAttribute()]
    [XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    [XmlRootAttribute("strikePrice", Namespace = "http://www.efs.org/2007/EFSmL-3-0", IsNullable = false)]
    public class AdjustmentStrikePrice : AdjustmentElement
    {
    }
    #endregion AdjustmentStrikePrice
    #region RFactor
    /// <summary>
    /// Caractéristiques du R-Factor
    /// </summary>
    [SerializableAttribute()]
    [XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    [XmlRootAttribute("rFactor", Namespace = "http://www.efs.org/2007/EFSmL-3-0", IsNullable = false)]
    public class RFactor : AdjustmentElement
    {
        #region Members
        [XmlIgnoreAttribute()]
        public bool rFactorCertifiedSpecified;
        [XmlElementAttribute("rFactorCertified", Order = 1)]
        public ComponentSimple rFactorCertified;
        [XmlIgnoreAttribute()]
        public CATools.AdjStatusEnum adjStatus;
        [XmlIgnoreAttribute()]
        public decimal rFactorRetained; 
        #endregion Members
        #region Methods
        #region FillRatio
        /// <summary>
        /// Mise à jour du résultat du ratio
        /// </summary>
        /// <param name="pRatio"></param>
        public void FillRatio<T>(T pTarget, Cst.ErrLevel pErrLevel)
        {
            Nullable<decimal> _result = component[0].Result;
            Nullable<decimal> _resultRounded = component[0].ResultRounded;
            if (pTarget is CorporateEventContract)
            {
                CorporateEventContract _target = pTarget as CorporateEventContract;
                _target.rFactorSpecified = _result.HasValue;
                if (_target.rFactorSpecified)
                    _target.rFactor = new SimpleUnit(_result.Value, (_resultRounded ?? _result.Value));

                _target.rFactorRetained = rFactorRetained;
            }
            else if (pTarget is CorporateEventAsset)
            {
                CorporateEventAsset _target = pTarget as CorporateEventAsset;
                _target.rFactorSpecified = _result.HasValue;
                if (_target.rFactorSpecified)
                    _target.rFactor = new SimpleUnit(_result.Value, (_resultRounded ?? _result.Value));
                _target.rFactorRetained = rFactorRetained;
            }
            CATools.SetAdjStatus(this, pTarget, pErrLevel);
        }
        #endregion FillRatio
        #region SetRatioRetained
        public Cst.ErrLevel SetRatioRetained(Cst.ErrLevel pErrLevel)
        {
            Cst.ErrLevel ret = pErrLevel;
            Nullable<decimal> _resultRounded = ResultRounded;
            adjStatus = (Cst.ErrLevel.SUCCESS != pErrLevel) ? CATools.AdjStatusEnum.RFACTOR_UNEVALUATED : CATools.AdjStatusEnum.EVALUATED;
            if (_resultRounded.HasValue)
            {
                rFactorRetained = _resultRounded.Value;
                if (rFactorCertifiedSpecified && _resultRounded.HasValue && (_resultRounded.Value != rFactorCertified.ResultRounded.Value))
                {
                    adjStatus = CATools.AdjStatusEnum.RFACTORCERTIFIED_RETAINED;
                    rFactorRetained = rFactorCertified.ResultRounded.Value;
                    ReverseId_RFactor_RFactorCertified();
                    ret = Cst.ErrLevel.RFACTOR_NOTCONFORM;
                }
            }
            else
            {
                if (rFactorCertifiedSpecified && rFactorCertified.ResultRounded.HasValue)
                {
                    adjStatus = CATools.AdjStatusEnum.RFACTORCERTIFIED_RETAINED;
                    rFactorRetained = rFactorCertified.ResultRounded.Value;
                    ReverseId_RFactor_RFactorCertified();
                    ret = Cst.ErrLevel.RFACTOR_NOTCONFORM;
                }
            }
            return ret;
        }
        #endregion SetRatioRetained
        #region ReverseId_RFactor_RFactorCertified
        private void ReverseId_RFactor_RFactorCertified()
        {
            ComponentBase component = this.component[0] as ComponentBase;
            if (null != component)
            {
                string id = component.Id.Replace("_DEPRECATED", string.Empty);
                rFactorCertified.Id = id;
                component.Id = id + "_DEPRECATED";
            }
        }
        #endregion ReverseId_RFactor_RFactorCertified

        #region FxRate
        /// <summary>
        /// Lecture d'un taux de change pour conversion dans une formule de calcul de ratio
        /// Codage en dur du GBX (Lecture GBP et multiplier de 100)
        /// </summary>
        /// <param name="pIdC1">Devise 1</param>
        /// <param name="pIdC2">Devise 2</param>
        /// <param name="pIdC_Pivot">Devise pivot</param>
        /// <returns></returns>
        /// EG 20140418 New 
            // EG 20231129 [WI762] End of Day processing : Possibility to request processing without initial margin (Cst.PosRequestTypeEnum.EndOfDayWithoutInitialMargin)
        public Nullable<decimal> FxRate(string pIdC1, string pIdC2, string pIdC_Pivot)
        {
            Nullable<decimal> ret = null;
            DateTime _cumDate = _workingDataContainer.ProcessCacheContainer.CACumDate;
            int multiplier = ((pIdC1 == "GBX") || (pIdC2 == "GBX")) ? 100:1;
            KeyAssetFxRate keyAssetFXRate = new KeyAssetFxRate
            {
                IdC1 = (pIdC1 == "GBX" ? "GBP" : pIdC1),
                IdC2 = (pIdC2 == "GBX" ? "GBP" : pIdC2),
                QuoteBasisSpecified = true,
                QuoteBasis = QuoteBasisEnum.Currency2PerCurrency1
            };
            keyAssetFXRate.SetQuoteBasis();
            KeyQuote keyQuote = new KeyQuote(_workingDataContainer.ProcessCacheContainer.CS, _cumDate);
            SQL_Quote quote = new SQL_Quote(_workingDataContainer.ProcessCacheContainer.CS, QuoteEnum.FXRATE, AvailabilityEnum.Enabled,
                _workingDataContainer.ProcessCacheContainer.ProductBase, keyQuote, keyAssetFXRate);
            if (StrFunc.IsFilled(pIdC_Pivot))
                quote.GetQuoteByPivotCurrency(pIdC_Pivot);
            if (quote.IsLoaded)
            {
                ret = quote.QuoteValue * multiplier;

                SQL_Currency currency = new SQL_Currency(_workingDataContainer.ProcessCacheContainer.CS, keyAssetFXRate.IdC2);
                if (currency.LoadTable(new string[] { "ROUNDDIR", "ROUNDPREC" }))
                {
                    EFS_Round round = new EFS_Round(currency.RoundDir, currency.RoundPrec, ret.Value);
                    ret = round.AmountRounded;
                }
            }
            return ret;
        }
        #endregion FxRate

        #endregion Methods
    }
    #endregion RFactor

    /* -------------------------------------------------------- */
    /* ----- FORMULES ET COMPOSANTS D'AJUSTEMENT          ----- */
    /* -------------------------------------------------------- */

    #region AdditionalInfos
    /// <summary>
    /// Caractéristiques complémentaires ( Composants additionnels )
    /// </summary>
    [SerializableAttribute()]
    [XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    [XmlRootAttribute("procedures", Namespace = "http://www.efs.org/2007/EFSmL-3-0", IsNullable = false)]
    // EG [33415/33420] New
    public class AdditionalInfos
    {
        #region Members
        [XmlElementAttribute("additionalInfo", Order = 1)]
        public AdditionalInfo additionalInfo;
        [XmlElementAttribute("template", Order=2)]
        public string templateFileName;

        #endregion Members
        #region Constructors
        public AdditionalInfos() { }
        #endregion Constructors
    }
    #endregion AdditionalInfos

    #region AdditionalInfo
    /// <summary>
    /// Caractéristiques complémentaires ( Composants additionnels)
    /// </summary>
    [SerializableAttribute()]
    [XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    [XmlRootAttribute("additionalInfo", Namespace = "http://www.efs.org/2007/EFSmL-3-0", IsNullable = false)]
    // EG [33415/33420] New
    public class AdditionalInfo
    {
        #region Members
        [XmlElementAttribute("component", typeof(ComponentSimple), Order = 1)]
        [XmlElementAttribute("componentFormula", typeof(ComponentFormula), Order = 1)]
        [XmlElementAttribute("componentMethod", typeof(ComponentMethod), Order = 1)]
        [XmlElementAttribute("componentProperty", typeof(ComponentProperty), Order = 1)]
        [XmlElementAttribute("componentReference", typeof(ComponentReference), Order = 1)]
        public ComponentBase[] component;
        #endregion Members
        #region Constructors
        #endregion Constructors
    }
    #endregion AdditionalInfo

    #region AdjustmentContract
    [SerializableAttribute()]
    [XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    [XmlRootAttribute("contract", Namespace = "http://www.efs.org/2007/EFSmL-3-0", IsNullable = false)]
    // EG [33415/33420] New
    public class AdjustmentContract
    {
        #region Members
        [XmlIgnoreAttribute()]
        public bool futureSpecified;
        [XmlElementAttribute("future", Order = 2)]
        public AdjustmentFuture future;
        [XmlIgnoreAttribute()]
        public bool optionSpecified;
        [XmlElementAttribute("option", Order = 3)]
        public AdjustmentOption option;
        #endregion Members
        #region Constructors
        #endregion Constructors
    }
    #endregion AdjustmentContract


    #region Formula
    /// <summary>
    /// Caractéristiques d'une formule : Nom - Expression mathématique - Composants
    /// </summary>
    [SerializableAttribute()]
    [XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public class Formula
    {
        #region Members
        [XmlIgnoreAttribute()]
        public bool descriptionSpecified;
        [XmlElementAttribute("description", Order = 1)]
        public string description;
        [XmlElementAttribute("mathExpression", Order = 2)]
        public string mathExpression;
        [XmlIgnoreAttribute()]
        public bool componentSpecified;
        [XmlElementAttribute("component", typeof(ComponentSimple), Order = 3)]
        [XmlElementAttribute("componentFormula", typeof(ComponentFormula), Order = 3)]
        [XmlElementAttribute("componentMethod", typeof(ComponentMethod), Order = 3)]
        [XmlElementAttribute("componentProperty", typeof(ComponentProperty), Order = 3)]
        [XmlElementAttribute("componentReference", typeof(ComponentReference), Order = 3)]
        public ComponentBase[] component;
        #endregion Members
    }
    #endregion Formula

    #region ComponentBase
    /// <summary>
    /// Classe de base étendue aux composants : SIMPLE, FORMULA, METHOD, PROPERTY, REFERENCE
    /// </summary>
    [SerializableAttribute()]
    [XmlIncludeAttribute(typeof(ComponentSimple))]
    [XmlIncludeAttribute(typeof(ComponentFormula))]
    [XmlIncludeAttribute(typeof(ComponentProperty))]
    [XmlIncludeAttribute(typeof(ComponentMethod))]
    [XmlIncludeAttribute(typeof(ComponentReference))]
    [XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    // EG [33415/33420] Gestion nouveau type Result
    public abstract partial class ComponentBase
    {
        #region Members
        [XmlIgnoreAttribute()]
        public bool descriptionSpecified;
        [XmlElementAttribute("description", Order = 1)]
        public string description;
        [XmlIgnoreAttribute()]
        public bool resultSpecified;
        [XmlElementAttribute("result", Order = 2)]
        public Result result;
        [XmlIgnoreAttribute()]
        public bool noticeSpecified;
        [XmlElementAttribute("notice", Order = 3)]
        public string notice;

        [XmlAttributeAttribute(DataType = "normalizedString")]
        public string name;
        [XmlAttributeAttribute("id", Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "ID")]
        public string Id
        {
            set;
            get;
        }
        #endregion Members
        #region Accessors
        #region ResultParameterMethod
        public object ResultParameterMethod
        {
            get
            {
                object ret = null;
                switch (result.itemsElementName)
                {
                    case ResultType.amount:
                        ret = (result.result as Money).amount.DecValue;
                        break;
                    case ResultType.unit:
                        ret = (result.result as SimpleUnit).value.DecValue;
                        break;
                    case ResultType.info:
                        ret = result.result;
                        break;
                }
                return ret;
            }
        }
        #endregion ResultParameterMethod
        #region Result
        /// <summary>
        /// Résultat d'évaluation (Valeur numérique)
        /// </summary>
        [XmlIgnoreAttribute()]
        public Nullable<decimal> Result
        {
            get
            {
                Nullable<decimal> ret = null;
                if (resultSpecified)
                    ret = CATools.Result(result,CATools.ResultValueEnum.Gross);
                return ret;
            }
        }
        #endregion Result
        #region ResultRounded
        [XmlIgnoreAttribute()]
        public Nullable<decimal> ResultRounded
        {
            get
            {
                Nullable<decimal> ret = null;
                if (resultSpecified)
                    ret = CATools.Result(result,CATools.ResultValueEnum.Rounded);
                return ret;
            }
        }
        #endregion ResultRounded
        #endregion Accessors
        #region Methods
        public void SetResult<T>(T pResult)
        {
            if (pResult is Nullable<decimal> || (null == pResult))
            {
                Nullable<decimal> _result = pResult as Nullable<decimal>;
                resultSpecified = _result.HasValue;
                if (resultSpecified)
                {
                    result = new Result
                    {
                        itemsElementName = ResultType.unit,
                        result = new SimpleUnit(_result.Value)
                    };
                }
            }
            else if (pResult is Result)
            {
                Result _result = pResult as Result;
                switch (_result.itemsElementName)
                {
                    case ResultType.amount:
                        Money _money = _result.result as Money;
                        resultSpecified = (null != _money);
                        if (resultSpecified)
                        {
                            result = new Result
                            {
                                itemsElementName = ResultType.amount
                            };
                            if (_money.amountRoundedSpecified)
                                result.result = new Money(_money.amount.DecValue, _money.amountRounded.DecValue, _money.currency.value);
                            else
                                result.result = new Money(_money.amount.DecValue, _money.currency.value);
                        }
                        break;
                    case ResultType.unit:
                        SimpleUnit _simpleUnit = _result.result as SimpleUnit;
                        resultSpecified = (null != _simpleUnit);
                        if (resultSpecified)
                        {
                            result = new Result
                            {
                                itemsElementName = ResultType.unit
                            };
                            if (_simpleUnit.valueRoundedSpecified)
                                result.result = new SimpleUnit(_simpleUnit.value.DecValue, _simpleUnit.valueRounded.DecValue);
                            else
                                result.result = new SimpleUnit(_simpleUnit.value.DecValue);
                        }
                        break;
                    // EG [33415/33420]
                    case ResultType.info:
                        string _info = _result.result as string;
                        resultSpecified = StrFunc.IsFilled(_info);
                        if (resultSpecified)
                        {
                            result = new Result
                            {
                                itemsElementName = ResultType.info,
                                result = _info
                            };
                        }
                        break;
                    // EG 20140317 [19722]
                    case ResultType.check:
                        resultSpecified = BoolFunc.IsTrue(_result.result);
                        if (resultSpecified)
                        {
                            result = new Result
                            {
                                itemsElementName = ResultType.check,
                                result = true
                            };
                        }
                        break;
                }
            }
        }
        #endregion Methods
    }
    #endregion ComponentBase
    #region Component
    /// <summary>
    /// Choix des élements (composants) d'un élément d'ajustement ou d'un de ses composants
    /// </summary>
    [SerializableAttribute()]
    [XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public class Component
    {
        [XmlElementAttribute("component", typeof(ComponentSimple),Order=1)]
        [XmlElementAttribute("componentFormula", typeof(ComponentFormula), Order = 1)]
        [XmlElementAttribute("componentMethod", typeof(ComponentMethod), Order = 1)]
        [XmlElementAttribute("componentProperty", typeof(ComponentProperty), Order = 1)]
        [XmlElementAttribute("componentReference", typeof(ComponentReference), Order = 1)]
        [XmlChoiceIdentifierAttribute("itemsElementName")]
        public ComponentBase[] component;
        [XmlElementAttribute(IsNullable = false)]
        [XmlIgnoreAttribute()]
        public ComponentType[] itemsElementName;
    }
    #endregion Component

    #region ComponentProperty
    /// <summary>
    /// Caractéristiques d'un composant évalué via un accesseur C#
    /// </summary>
    [SerializableAttribute()]
    [XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public class ComponentProperty : ComponentBase
    {
        [XmlElementAttribute("property", Order = 1)]
        public AssemblyInformation property;
    }
    #endregion ComponentProperty
    #region ComponentMethod
    /// <summary>
    /// Caractéristiques d'un composant évalué via une méthode C#
    /// </summary>
    [SerializableAttribute()]
    [XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public class ComponentMethod : ComponentBase
    {
        [XmlElementAttribute("method", Order = 1)]
        public Method method;
    }
    #endregion ComponentMethod
    #region ComponentFormula
    /// <summary>
    /// Caractéristiques d'un composant évalué via une formule
    /// </summary>
    [SerializableAttribute()]
    [XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public class ComponentFormula : ComponentBase
    {
        [XmlElementAttribute("formula", Order = 1)]
        public Formula formula;
    }
    #endregion ComponentFormula
    #region ComponentSimple
    /// <summary>
    /// Caractéristiques d'un composant simple connu à priori (valeur renseignée à la saisie d'une CA)
    /// </summary>
    [SerializableAttribute()]
    [XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public class ComponentSimple : ComponentBase
    {
        #region Members
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public bool required;
        [System.Xml.Serialization.XmlAttributeAttribute(AttributeName="datatype")]
        public ResultType resulttype;
        #endregion Members
        #region Constructors
        public ComponentSimple()
        {
            required = true;
            resulttype = ResultType.unit;
        }
        #endregion Constructors
        #region Members
        public void InitializeResult()
        {
            result = new Result
            {
                itemsElementName = resulttype
            };
            switch (result.itemsElementName)
            {
                case ResultType.amount:
                    result.result = new Money();
                    break;
                case ResultType.check:
                    result.result = new Boolean();
                    break;
                case ResultType.info:
                    result.result = string.Empty;
                    break;
                case ResultType.unit:
                    result.result = new SimpleUnit();
                    break;
            }
            resultSpecified = true;
        }
        #endregion Members
    }
    #endregion ComponentSimple
    #region ComponentReference
    /// <summary>
    /// Référence vers un composant existant ailleurs dans le document
    /// </summary>
    [SerializableAttribute()]
    [XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    // EG 20210818 [XXXXX] Ajout valeur par défaut d'un composant en référence lorsque celui ci n'existe pas -->
    public class ComponentReference : ComponentBase
    {
        #region Members
        [System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "IDREF")]
        public string href;
        [XmlIgnoreAttribute()]
        public bool defaultResultSpecified;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string defaultResult;
        #endregion Members
    }
    #endregion ComponentReference


    #region AssemblyInformation
    /// <summary>
    /// Descriptif d'accès à un composant METHOD ou PROPERTY (Assembly, Classe, nom de la méthode ou de l'accesseur)
    /// </summary>
    [SerializableAttribute()]
    [XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public class AssemblyInformation
    {
        #region Members
        [XmlAttributeAttribute("assembly", DataType = "normalizedString")]
        public string @assembly;
        [XmlAttributeAttribute("class", DataType = "normalizedString")]
        public string @class;
        [XmlTextAttribute()]
        public string @value;
        #endregion Members
        #region Constructors
        public AssemblyInformation()
        {
        }
        #endregion Constructors
    }
    #endregion AssemblyInformation
    #region Currency
    /// <summary>
    /// Devise associable à un résultat d'évaluation (informatif)
    /// </summary>
    [SerializableAttribute()]
    [XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    [XmlRootAttribute("currency", Namespace = "http://www.efs.org/2007/EFSmL-3-0", IsNullable = false)]
    public class Currency
    {
        public string currencyScheme;
        public string value;
        public Currency()
        {
            this.currencyScheme = "http://www.fpml.org/ext/iso4217-2001-08-15";
        }
    }
    #endregion Currency
    #region EqualisationPayment
    /// <summary>
    /// Devise associable à un résultat d'évaluation (informatif)
    /// </summary>
    [SerializableAttribute()]
    [XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    [XmlRootAttribute("equalisationPayment", Namespace = "http://www.efs.org/2007/EFSmL-3-0", IsNullable = false)]
    public class EqualisationPayment
    {
        [XmlIgnoreAttribute()]
        public bool roundingSpecified;
        [XmlElementAttribute("rounding", Order = 1)]
        public Rounding rounding;

        public EqualisationPayment()
        {
        }
    }
    #endregion EqualisationPayment
    #region Method
    /// <summary>
    /// Descriptif d'accès à une méthode C# (Assembly, Classe, Méthode, Paramètres)
    /// </summary>
    [SerializableAttribute()]
    [XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public class Method
    {
        [XmlElementAttribute("name", Order = 1)]
        public AssemblyInformation name;
        [XmlIgnoreAttribute()]
        public bool componentSpecified;
        [XmlArrayAttribute("components", Order = 2)]
        [XmlArrayItemAttribute("component", typeof(ComponentSimple))]
        [XmlArrayItemAttribute("componentFormula", typeof(ComponentFormula))]
        [XmlArrayItemAttribute("componentMethod", typeof(ComponentMethod))]
        [XmlArrayItemAttribute("componentProperty", typeof(ComponentProperty))]
        [XmlArrayItemAttribute("componentReference", typeof(ComponentReference))]
        public ComponentBase[] component;

    }
    #endregion Method
    #region Money
    /// <summary>
    /// Résultat d'évaluation de type MONTANT-DEVISE d'un composant, d'un élement d'ajustement ...
    /// </summary>
    [SerializableAttribute()]
    [XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    // EG 20190121 [23249] Add rounding pour sauvergarde des règles d'arrondi
    public class Money
    {
        #region Members
        [XmlElementAttribute("currency", Order = 1)]
        public Currency currency;
        [XmlElementAttribute("amount", Order = 2)]
        public EFS_Decimal amount;
        [XmlIgnoreAttribute()]
        public bool amountRoundedSpecified;
        [XmlElementAttribute("amountRounded", Order = 3)]
        public EFS_Decimal amountRounded;
        [XmlIgnoreAttribute()]
        public bool roundingSpecified;
        [XmlElementAttribute("rounding", Order = 4)]
        public Rounding rounding;
        #endregion Members
        #region Constructors
        public Money(){}
        public Money(string pValue, string pCurrency)
        {
            amount = new EFS_Decimal(pValue);
            amountRoundedSpecified = false;
            currency = new Currency
            {
                value = pCurrency
            };
        }
        public Money(string pValue, string pValueRounded, string pCurrency)
        {
            amount = new EFS_Decimal(pValue);
            amountRoundedSpecified = true;
            amountRounded = new EFS_Decimal(pValueRounded);
            currency = new Currency
            {
                value = pCurrency
            };
        }
        public Money(decimal pValue, string pCurrency)
        {
            amount = new EFS_Decimal(pValue);
            amountRoundedSpecified = false;
            currency = new Currency
            {
                value = pCurrency
            };
        }
        public Money(decimal pValue, decimal pValueRounded, string pCurrency)
        {
            amount = new EFS_Decimal(pValue);
            amountRoundedSpecified = true;
            amountRounded = new EFS_Decimal(pValueRounded);
            currency = new Currency
            {
                value = pCurrency
            };
        }
        #endregion Constructors
        #region Methods
        // EG 20190121 [23249] New
        public void SaveRounding(Rounding pRounding)
        {
            SaveRounding(pRounding, null);
        }
        // EG 20190121 [23249] New
        public void SaveRounding(Rounding pRounding, Nullable<int> pPrecision)
        {
            roundingSpecified = (null != pRounding);
            rounding = new Rounding(pRounding.direction, pPrecision ?? pRounding.precision);
        }
        #endregion Methods
    }
    #endregion Money
    #region Result
    /// <summary>
    /// Résultat d'évaluation d'un composant, d'un élement d'ajustement ...
    /// </summary>
    [SerializableAttribute()]
    [XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    // EG [33415/33420] New type result = info
    // EG 20140317 [19722]
    public class Result
    {
        #region Members
        [XmlElementAttribute("amount", typeof(Money),Order = 1)]
        [XmlElementAttribute("unit", typeof(SimpleUnit), Order = 1)]
        [XmlElementAttribute("info", typeof(string), Order = 1)]
        [XmlElementAttribute("check", typeof(bool), Order = 1)]
        [XmlChoiceIdentifierAttribute("itemsElementName")]
        public object result;
        [XmlElementAttribute(IsNullable = false)]
        [XmlIgnoreAttribute()]
        public ResultType itemsElementName;
        #endregion Members
        #region Methods
        #endregion Methods
    }
    #endregion Result
    #region Rounding 
    /// <summary>
    /// Règles d'arrondi du résultat d'un ajustement ou d'une évaluation
    /// </summary>
    [SerializableAttribute()]
    [XmlTypeAttribute(Namespace="http://www.efs.org/2007/EFSmL-3-0")]
    public class Rounding
    {
        #region Members
        [XmlElementAttribute("direction", Order = 1)]
        public Cst.RoundingDirectionSQL direction;
        [XmlIgnoreAttribute()]
        public bool isUpdDirection;
        [XmlElementAttribute("precision", Order = 2)]
        public int precision;
        [XmlIgnoreAttribute()]
        public bool isUpdPrecision;
        #endregion Members
        #region Constructors
        public Rounding()
        {
        }
        public Rounding(Cst.RoundingDirectionSQL pDirection, int pPrecision)
        {
            direction = pDirection;
            isUpdDirection = true;
            precision = pPrecision;
            isUpdPrecision = true;
        }
        public Rounding(string pDirection,bool pIsUpdDirection, int pPrecision, bool pIsUpdPrecision)
        {
            direction = (Cst.RoundingDirectionSQL)System.Enum.Parse(typeof(Cst.RoundingDirectionSQL), pDirection);
            isUpdDirection = pIsUpdDirection;
            precision = pPrecision;
            isUpdPrecision = pIsUpdPrecision;
        }
        #endregion Constructors
    }
    #endregion Rounding 
    #region TakePlace
    /// <summary>
    /// Caractéristiques des conditions d'application d'un ajustement
    /// </summary>
    [SerializableAttribute()]
    [XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public class TakePlace
    {
        #region Members
        [XmlElementAttribute("component", typeof(ComponentSimple), Order = 1)]
        [XmlElementAttribute("componentFormula", typeof(ComponentFormula), Order = 1)]
        [XmlElementAttribute("componentMethod", typeof(ComponentMethod), Order = 1)]
        [XmlElementAttribute("componentProperty", typeof(ComponentProperty), Order = 1)]
        [XmlElementAttribute("componentReference", typeof(ComponentReference), Order = 1)]
        public ComponentBase component;

        [XmlElementAttribute("condition", Order = 2)]
        public ConditionEnum condition;
        [XmlIgnoreAttribute()]
        public bool responseSpecified;
        [XmlElementAttribute("response", Order = 3)]
        public bool response;
        #endregion Members
        #region Constructors
        public TakePlace()
        {
            condition = ConditionEnum.Always;
        }
        #endregion Constructors
        #region Methods
        #region Response
        /// <summary>
        /// Valorisation de la réponse à la condition de mise en place
        /// </summary>
        public void SetResponse()
        {
            bool isTakePlace = false;
            responseSpecified = component.resultSpecified;
            if (responseSpecified)
            {
                Nullable<decimal> _result = component.Result;
                #region Application de la condition
                switch (condition)
                {
                    case ConditionEnum.LessThanOne:
                        isTakePlace = (_result.Value < 1);
                        break;
                    case ConditionEnum.NonNegativeValue:
                        isTakePlace = (_result.Value >= 0);
                        break;
                    case ConditionEnum.NonPositiveValue:
                        isTakePlace = (_result.Value <= 0);
                        break;
                    case ConditionEnum.PositiveValue:
                        isTakePlace = (_result.Value > 0);
                        break;
                    case ConditionEnum.Always:
                        isTakePlace = true;
                        break;
                    default:
                        isTakePlace = false;
                        break;
                }
                #endregion Application de la condition
            }
            response = isTakePlace;
        }
        #endregion Response
        #endregion Methods
    }
    #endregion TakePlace
    #region SimpleUnit
    [SerializableAttribute()]
    [XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    // EG 20190121 [23249] Add rounding pour sauvergarde des règles d'arrondi
    public class SimpleUnit
    {
        [XmlElementAttribute("value", Order = 1)]
        public EFS_Decimal @value;
        [XmlIgnoreAttribute()]
        public bool valueRoundedSpecified;
        [XmlElementAttribute("valueRounded", Order = 2)]
        public EFS_Decimal valueRounded;
        [XmlIgnoreAttribute()]
        public bool roundingSpecified;
        [XmlElementAttribute("rounding", Order = 3)]
        public Rounding rounding;

        #region Constructors
        public SimpleUnit()
        {
        }
        public SimpleUnit(string pValue)
        {
            @value = new EFS_Decimal(pValue);
            valueRoundedSpecified = false;
        }
        public SimpleUnit(string pValue, string pValueRounded)
        {
            @value = new EFS_Decimal(pValue);
            valueRoundedSpecified = true;
            valueRounded = new EFS_Decimal(pValueRounded);
        }
        public SimpleUnit(decimal pValue)
        {
            @value = new EFS_Decimal(pValue);
            valueRoundedSpecified = false;
        }

        public SimpleUnit(decimal pValue, decimal pValueRounded)
        {
            @value = new EFS_Decimal(pValue);
            valueRoundedSpecified = true;
            valueRounded = new EFS_Decimal(pValueRounded);
        }
        #endregion Constructors
        #region Methods
        // EG 20190121 [23249] New
        public void SaveRounding(Rounding pRounding)
        {
            SaveRounding(pRounding, null);
        }
        // EG 20190121 [23249] New
        public void SaveRounding(Rounding pRounding, Nullable<int> pPrecision)
        {
            roundingSpecified = (null != pRounding);
            rounding = new Rounding(pRounding.direction, pPrecision ?? pRounding.precision);
        }
        #endregion Methods
    }
    #endregion SimpleUnit

    /* -------------------------------------------------------- */
    /* ----- CLASSES DE CALCUL DC et ASSET                ----- */
    /* -------------------------------------------------------- */

    #region CalculationData
    /// <summary>
    /// Elements de calcul des DC et ASSET (utilisé en CUM et EX)
    /// </summary>
    [SerializableAttribute()]
    [XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public class CalculationData
    {
        #region Members
        [XmlIgnoreAttribute()]
        public bool contractAttributeSpecified;
        [XmlElementAttribute("version", Order = 1)]
        public string contractAttribute;
        [XmlIgnoreAttribute()]
        public bool contractSizeSpecified;
        [XmlElementAttribute("contractSize", Order = 2)]
        public EFS_Decimal contractSize;
        [XmlIgnoreAttribute()]
        public bool contractMultiplierSpecified;
        [XmlElementAttribute("contractMultiplier", Order = 3)]
        public EFS_Decimal contractMultiplier;
        [XmlIgnoreAttribute()]
        public bool strikePriceSpecified;
        [XmlElementAttribute("strikePrice", Order = 4)]
        public EFS_Decimal strikePrice;
        [XmlIgnoreAttribute()]
        public bool dailySettlementPriceSpecified;
        [XmlElementAttribute("dailySettlementPrice", Order = 5)]
        public EFS_Decimal dailySettlementPrice;
        [XmlIgnoreAttribute()]
        public bool unitEqPaymentSpecified;
        [XmlElementAttribute("unitEqualizationPayment", Order = 6)]
        public EFS_Decimal unitEqPayment;
        #endregion Members
        #region Constructors
        public CalculationData()
        {
        }
        #endregion Constructors
    }
    #endregion CalculationData
    #region CalculationCumData
    /// <summary>
    /// Elements de calcul des DC et ASSET (CUM)
    /// </summary>
    [SerializableAttribute()]
    [XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public class CalculationCumData
    {
        #region Members
        [XmlIgnoreAttribute()]
        public bool renamingValueSpecified;
        [XmlElementAttribute("renamingValue", Order = 1)]
        public string renamingValue;
        [XmlIgnoreAttribute()]
        public bool contractSizeSpecified;
        [XmlElementAttribute("contractSize", Order = 2)]
        public EFS_Decimal contractSize;
        [XmlIgnoreAttribute()]
        public bool contractMultiplierSpecified;
        [XmlElementAttribute("contractMultiplier", Order = 3)]
        public EFS_Decimal contractMultiplier;
        [XmlIgnoreAttribute()]
        public bool strikePriceSpecified;
        [XmlElementAttribute("strikePrice", Order = 4)]
        public EFS_Decimal strikePrice;
        [XmlIgnoreAttribute()]
        public bool dailyClosingPriceSpecified;
        [XmlElementAttribute("dailyClosingPrice", Order = 5)]
        public EFS_Decimal dailyClosingPrice;
        #endregion Members
        #region Constructors
        public CalculationCumData()
        {
        }
        #endregion Constructors
    }
    #endregion CalculationCumData
    #region CalculationExData
    /// <summary>
    /// Elements de calcul des DC et ASSET (EX)
    /// </summary>
    [SerializableAttribute()]
    [XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public class CalculationExData
    {
        #region Members
        [XmlIgnoreAttribute()]
        public bool renamingValueSpecified;
        [XmlElementAttribute("renamingValue", Order = 1)]
        public string renamingValue;
        [XmlIgnoreAttribute()]
        public bool contractSizeSpecified;
        [XmlElementAttribute("contractSize", Order = 2)]
        public SimpleUnit contractSize;
        [XmlIgnoreAttribute()]
        public bool contractMultiplierSpecified;
        [XmlElementAttribute("contractMultiplier", Order = 3)]
        public SimpleUnit contractMultiplier;
        [XmlIgnoreAttribute()]
        public bool strikePriceSpecified;
        [XmlElementAttribute("strikePrice", Order = 4)]
        public SimpleUnit strikePrice;
        [XmlIgnoreAttribute()]
        public bool dailyClosingPriceSpecified;
        [XmlElementAttribute("dailyClosingPrice", Order = 5)]
        public SimpleUnit dailyClosingPrice;
        [XmlIgnoreAttribute()]
        public bool equalizationPaymentSpecified;
        [XmlElementAttribute("equalizationPayment", Order = 6)]
        public SimpleUnit equalizationPayment;
        #endregion Members
        #region Constructors
        public CalculationExData()
        {
        }
        #endregion Constructors
    }
    #endregion CalculationExData

    /* -------------------------------------------------------- */
    /* ----- CLASSES D'IDENTIFICATION REPOSITORY          ----- */
    /* -------------------------------------------------------- */

    #region SpheresCommonIdentification
    /// <summary>
    /// Classe générique d'identification d'un élement de la SGBD/R Spheres
    /// </summary>
    [SerializableAttribute()]
    [XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public class SpheresCommonIdentification
    {
        #region Members
        [XmlAttributeAttribute("spheresid", DataType = "normalizedString")]
        public string spheresid;
        [XmlAttributeAttribute("id", DataType = "ID")]
        public string id;
        [XmlIgnoreAttribute()]
        public bool identifierSpecified;
        [XmlElementAttribute("identifier", Order = 1)]
        public string identifier;
        [XmlIgnoreAttribute()]
        public bool displaynameSpecified;
        [XmlElementAttribute("displayname", Order = 2)]
        public string displayname;
        [XmlIgnoreAttribute()]
        public bool descriptionSpecified;
        [XmlElementAttribute("description", Order = 3)]
        public string description;
        [XmlIgnoreAttribute()]
        public bool extllinkSpecified;
        [XmlElementAttribute("extllink", Order = 4)]
        public string extllink;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public int SpheresId
        {
            get { return Convert.ToInt32(spheresid); }
            set { spheresid = value.ToString(); }
        }

        #endregion Members
    }
    #endregion SpheresCommonIdentification
    #region MarketIdentification
    /// <summary>
    /// Classe générique d'identification d'un élement MARKET dans Spheres
    /// </summary>
    [SerializableAttribute()]
    [XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    /// EG 20211020 [XXXXX] Nouvelle gestion des notices (USEUCANOTICE)
    public class MarketIdentification : SpheresCommonIdentification
    {
        #region Members
        [XmlIgnoreAttribute()]
        public bool FIXML_SecurityExchangeSpecified;
        [XmlElementAttribute("FIXML_SecurityExchange", Order = 1)]
        public string FIXML_SecurityExchange;
        [XmlIgnoreAttribute()]
        public bool ISO10383_ALPHA4Specified;
        [XmlElementAttribute("ISO10383_ALPHA4", Order = 2)]
        public string ISO10383_ALPHA4;
        [XmlElementAttribute("marketType", Order = 3)]
        public Cst.MarketTypeEnum marketType;
        [XmlIgnoreAttribute()]
        public bool acronymSpecified;
        [XmlElementAttribute("acronym", Order = 4)]
        public string acronym;
        [XmlIgnoreAttribute()]
        public bool exchangeSymbolSpecified;
        [XmlElementAttribute("exchangeSymbol", Order = 5)]
        public string exchangeSymbol;
        [XmlIgnoreAttribute()]
        public bool shortIdentifierSpecified;
        [XmlElementAttribute("shortIdentifier", Order = 6)]
        public string shortIdentifier;
        [XmlIgnoreAttribute()]
        public bool shortAcronymSpecified;
        [XmlElementAttribute("shortAcronym", Order = 7)]
        public string shortAcronym;
        [XmlIgnoreAttribute()]
        public bool idBCSpecified;
        [XmlElementAttribute("businessCenter", Order = 8)]
        public string idBC;
        [XmlIgnoreAttribute()]
        public bool urlCANoticeSpecified;
        [XmlElementAttribute("urlCANotice", Order = 9)]
        public string urlCANotice;
        #endregion Members
        #region Accessors
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public int IdM
        {
            get { return Convert.ToInt32(spheresid); }
            set { spheresid = value.ToString(); }
        }
        #endregion Accessors
    }
    #endregion MarketIdentification
    #region MaturityIdentification
    /// <summary>
    /// Classe générique d'identification d'une échéance de contrat dans Spheres
    /// </summary>
    [SerializableAttribute()]
    [XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public class MaturityIdentification : SpheresCommonIdentification
    {
        #region Members
        [XmlIgnoreAttribute()]
        public bool maturityMonthYearSpecified;
        [System.Xml.Serialization.XmlElementAttribute("maturityMonthYear", Order = 1)]
        public string maturityMonthYear;
        #endregion Members
    }
    #endregion MaturityIdentification
    #region RefNoticeIdentification
    /// <summary>
    /// Identification Notice de corporate action
    /// </summary>
    [SerializableAttribute()]
    [XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    [XmlRootAttribute("refNotice", Namespace = "http://www.efs.org/2007/EFSmL-3-0", IsNullable = false)]
    /// EG 20211020 [XXXXX] Nouvelle gestion des notices (USEURLNOTICE)
    public class RefNoticeIdentification
    {
        [XmlAttributeAttribute("fileName")]
        public string fileName;
        [XmlAttributeAttribute("pubDate")]
        public DateTime pubDate;
        [XmlIgnoreAttribute()]
        public bool useurlnoticeSpecified;
        [XmlAttributeAttribute("useurlnotice")]
        public bool useurlnotice;
        public string value;
        public RefNoticeIdentification()
        {
        }
    }
    #endregion RefNoticeIdentification

    #region ContractIdentification
    /// </summary>
    [SerializableAttribute()]
    [XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public class ContractIdentification : SpheresCommonIdentification
    {
        #region Members
        [XmlElementAttribute("renamingMethod", Order = 1)]
        public CorpoEventRenamingContractMethodEnum renamingMethod;
        [XmlIgnoreAttribute()]
        public bool contractSymbolSpecified;
        [XmlElementAttribute("contractSymbol", Order = 2)]
        public string contractSymbol;
        [XmlIgnoreAttribute()]
        public bool contractAttributeSpecified;
        [XmlElementAttribute("contractAttribute", Order = 3)]
        public string contractAttribute;
        #endregion Members
    }
    #endregion ContractIdentification

    #region AIRecycledContract
    public class AIRecycledContract
    {
        public string CumContractSymbol { get; set; }
        public string CumUnderlyerIsinCode { get; set; }
        public string NewContractSymbol { get; set; }
        public string NewUnderlyerIsinCode { get; set; }

        #region Constructors
        public AIRecycledContract() { }
        public AIRecycledContract(string pCumContractSymbol, string pCumUnderlyerIsinCode, string pNewContractSymbol, string pNewUnderlyerIsinCode)
        {
            CumContractSymbol = pCumContractSymbol;
            CumUnderlyerIsinCode = pCumUnderlyerIsinCode;
            NewContractSymbol = pNewContractSymbol;
            NewUnderlyerIsinCode = pNewUnderlyerIsinCode;
        }
        #endregion Constructors
    }
    #endregion RecycledContract

    #region AIContract
    public class AIContract
    {
        public string Symbol { get; set; }
        public string UnlIsinCode { get; set; }
        public string Description { get; set; }
        public string DisplayName { get; set; }
        public Nullable<decimal> ContractSize { get; set; }
        public Nullable<decimal> ContractMultiplier { get; set; }
        public Nullable<decimal> QtyMultiplier { get; set; }
        public string IsinCode { get; set; }

        #region Constructors
        public AIContract() { }
        public AIContract(string pSymbol)
        {
            Symbol = pSymbol;
        }
        #endregion Constructors
    }
    #endregion AIContract


    #region AIUnderlyer
    public class AIUnderlyer
    {
        public string IsinCode { get; set; }
        public string Description { get; set; }
        public string DisplayName { get; set; }
        public string Symbol { get; set; }
        public string RelatedSymbol { get; set; }

        #region Constructors
        public AIUnderlyer() {}
        public AIUnderlyer(string pIsinCode)
        {
            IsinCode = pIsinCode;
        }
        #endregion Constructors
    }
    #endregion AIUnderlyer
}








