using System;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using System.Linq;

using EFS.ApplicationBlocks.Data;

namespace EFS.Spheres.DataContracts
{
    
    /// <summary>
    /// Class representing a actor/role relationship.
    ///     The actor can be the relation owner, or the actor for which the relation is "with regard to" him.
    /// </summary>
    [DataContract(
        Name = DataHelper<ActorRelationship>.DATASETROWNAME,
        Namespace = DataHelper<ActorRelationship>.DATASETNAMESPACE)]
    [XmlRoot(ElementName = "Relation")]
    public sealed class ActorRelationship : IDataContractEnabled
    {
        string m_Actor;

        /// <summary>
        /// Actor Identifier
        /// </summary>
        [DataMember(Name = "ACTOR", Order = 1)]
        public string Actor
        {
            get { return m_Actor; }
            set { m_Actor = value; }
        }

        int m_ActorId;

        /// <summary>
        /// Actor internal ID
        /// </summary>
        [DataMember(Name = "ACTORID", Order = 2)]
        public int ActorId
        {
            get { return m_ActorId; }
            set { m_ActorId = value; }
        }

        string m_ActorName;

        /// <summary>
        /// Actor Display name
        /// </summary>
        [DataMember(Name = "ACTORNAME", Order = 3)]
        public string ActorName
        {
            get { return m_ActorName; }
            set { m_ActorName = value; }
        }

        DateTime m_ActorEnabledFrom;

        /// <summary>
        /// Starting actor validity date
        /// </summary>
        [DataMember(Name = "ACTOR_DTEN", Order = 4)]
        public DateTime ActorEnabledFrom
        {
            get { return m_ActorEnabledFrom; }
            set { m_ActorEnabledFrom = value; }
        }

        DateTime m_ActorDisabledFrom;

        /// <summary>
        /// Ending actor validity date
        /// </summary>
        [DataMember(Name = "ACTOR_DTDIS", Order = 5)]
        public DateTime ActorDisabledFrom
        {
            get { return m_ActorDisabledFrom; }
            set { m_ActorDisabledFrom = value; }
        }

        string m_RoleOwner;

        /// <summary>
        /// Identifier for the actor role owner
        /// </summary>
        [DataMember(Name = "ROLEOWNER", Order = 6)]
        public string RoleOwner
        {
            get { return m_RoleOwner; }
            set { m_RoleOwner = value; }
        }

        int m_RoleOwnerId;

        /// <summary>
        /// internal Id for the actor role owner
        /// </summary>
        [DataMember(Name = "ROLEOWNERID", Order = 7)]
        public int RoleOwnerId
        {
            get { return m_RoleOwnerId; }
            set { m_RoleOwnerId = value; }
        }

        string m_RoleOwnerName;

        /// <summary>
        /// Display name for the actor role owner
        /// </summary>
        [DataMember(Name = "ROLEOWNERNAME", Order = 8)]
        public string RoleOwnerName
        {
            get { return m_RoleOwnerName; }
            set { m_RoleOwnerName = value; }
        }

        DateTime m_RoleOwnerEnabledFrom;

        /// <summary>
        /// Starting validity date for the actor role owner
        /// </summary>
        [DataMember(Name = "ROLEOWNER_DTEN", Order = 9)]
        public DateTime RoleOwnerEnabledFrom
        {
            get { return m_RoleOwnerEnabledFrom; }
            set { m_RoleOwnerEnabledFrom = value; }
        }

        DateTime m_RoleOwnerDisabledFrom;

        /// <summary>
        /// Ending validity date for the actor role owner
        /// </summary>
        [DataMember(Name = "ROLEOWNER_DTDIS", Order = 10)]
        public DateTime RoleOwnerDisabledFrom
        {
            get { return m_RoleOwnerDisabledFrom;  }
            set { m_RoleOwnerDisabledFrom = value; }
        }

        string m_Role;

        /// <summary>
        /// Role identifier
        /// </summary>
        [DataMember(Name = "ROLE", Order = 11)]
        public string Role
        {
            get { return m_Role; }
            set 
            { 
                if (!String.IsNullOrEmpty(value))
                    m_Role = value.Trim(); 
            }
        }

        string m_RoleName;

        /// <summary>
        /// Role display name
        /// </summary>
        [DataMember(Name = "ROLENAME", Order = 12)]
        public string RoleName
        {
            get { return m_RoleName; }
            set { m_RoleName = value; }
        }

        DateTime m_RoleEnabledFrom;

        /// <summary>
        /// Role starting validity date
        /// </summary>
        [DataMember(Name = "ROLE_DTEN", Order = 13)]
        public DateTime RoleEnabledFrom
        {
            get { return m_RoleEnabledFrom; }
            set { m_RoleEnabledFrom = value; }
        }

        DateTime m_RoleDisabledFrom;

        /// <summary>
        /// Role ending validity date
        /// </summary>
        [DataMember(Name = "ROLE_DTDIS", Order = 14)]
        public DateTime RoleDisabledFrom
        {
            get { return m_RoleDisabledFrom; }
            set { m_RoleDisabledFrom = value; }
        }

        int m_RoleWithRegardToActorId;

        /// <summary>
        /// Actor internal, parent of the actor/role relation
        /// </summary>
        /// <remarks>
        /// The child of the actor role relation is returned from the RoleOwnerId property
        /// </remarks>
        [DataMember(Name = "ROLE_WITHREGARDTOID", Order = 15)]
        public int RoleWithRegardToActorId
        {
            get { return m_RoleWithRegardToActorId; }
            set { m_RoleWithRegardToActorId = value; }
        }


        #region IDataContractEnabled Membres

        /// <summary>
        /// get the minimum element among ActorEnabledFrom, RoleOwnerEnabledFrom, RoleEnabledFrom
        /// </summary>
        public DateTime ElementEnabledFrom
        {
            get
            {
                DateTime[] enabledFrom = {this.ActorEnabledFrom, this.RoleEnabledFrom, this.RoleOwnerEnabledFrom};

                return enabledFrom.Min();
            }
           
        }

        /// <summary>
        /// get the maximum element mong ActorDisabledFrom, RoleDisabledFrom, RoleOwnerDisabledFrom
        /// </summary>
        public DateTime ElementDisabledFrom
        {
            get
            {
                DateTime[] disabledFrom = { this.ActorDisabledFrom, this.RoleDisabledFrom, this.RoleOwnerDisabledFrom };

                return disabledFrom.Max();
            }
           
        }

        #endregion
    }

    /// <summary>
    /// Class representing an Entity/Market relationship including the Clearing House (if the market is connected to her, otherwise 
    ///     all the css fields will be null).
    /// Usually this class is used to load Clearing House informations which treat ordres on derivative contracts.
    /// </summary>
    /// PM 20170313 [22833] Ajout DTENTITYNEXT et IDBCENTITY
    /// PM 20180219 [23824] Ajout IDIOTASK_RISKDATA
    [DataContract(
        Name = DataHelper<EntityMarketWithCSS>.DATASETROWNAME,
        Namespace = DataHelper<EntityMarketWithCSS>.DATASETNAMESPACE)]
    [XmlRoot(ElementName = "EntityMarketRelation")]
    public sealed class EntityMarketWithCSS : IDataContractEnabled
    {
        string m_Market;

        /// <summary>
        /// market identfier
        /// </summary>
        [DataMember(Name = "MARKET", Order = 1)]
        public string Market
        {
            get { return m_Market; }
            set { m_Market = value; }
        }

        int m_MarketId;

        /// <summary>
        /// Market internal id
        /// </summary>
        [DataMember(Name = "MARKETID", Order = 2)]
        public int MarketId
        {
            get { return m_MarketId; }
            set { m_MarketId = value; }
        }

        string m_MarketName;

        /// <summary>
        /// Market display name
        /// </summary>
        [DataMember(Name = "MARKETNAME", Order = 3)]
        public string MarketName
        {
            get { return m_MarketName; }
            set { m_MarketName = value; }
        }

        DateTime m_MarketEnableFrom;

        /// <summary>
        /// Market activation date
        /// </summary>
        [DataMember(Name = "MARKET_DTEN", Order = 4)]
        public DateTime MarketEnableFrom
        {
            get { return m_MarketEnableFrom; }
            set { m_MarketEnableFrom = value; }
        }

        DateTime m_MarketDisabledFrom;

        /// <summary>
        /// Market deactivation date
        /// </summary>
        [DataMember(Name = "MARKET_DTDIS", Order = 5)]
        public DateTime MarketDisabledFrom
        {
            get { return m_MarketDisabledFrom; }
            set { m_MarketDisabledFrom = value; }
        }

        // TODO 20110404 MF transformer le type text du champ m_Market_AssignementMethod en un type énumération 

        string m_MarketAssignementMethod;

        /// <summary>
        /// Market assignement method
        /// </summary>
        [DataMember(Name = "MARKET_ASSMETH", Order = 6)]
        public string MarketAssignementMethod
        {
            get { return m_MarketAssignementMethod; }
            set { m_MarketAssignementMethod = value; }
        }

        // TODO MF définir le type en tant que énumération PosStockCoverEnum à peine elle sera déplacée vers Common
        /// <summary>
        /// Stock coverage string code 
        /// </summary>
        [DataMember(Name = "MARKET_STOCKCOVERAGE", Order = 7)]
        public string MarketStockCoverage
        {
            get;
            set;
        }

        bool m_MarketIsIntradayExeAssActivated;

        /// <summary>
        /// loading daily exercises/assignations during position risk evaluations
        /// </summary>
        [DataMember(Name = "MARKET_EXEASS", Order = 8)]
        public bool MarketIsIntradayExeAssActivated
        {
            get { return m_MarketIsIntradayExeAssActivated; }
            set { m_MarketIsIntradayExeAssActivated = value; }
        }

        /// <summary>
        /// Activate the cross margining (relative to the current market) during position risk evaluation
        /// </summary>
        [DataMember(Name = "MARKET_CROSSMARGIN", Order = 9)]
        public bool MarketCrossMarginActivated
        {
            get;
            set;
        }

        /// <summary>
        /// Business Center
        /// </summary>
        // PM 20130328 Ajouté pour l'AGREX
        [DataMember(Name = "MARKET_BC", Order = 10)]
        public string MarketBusinessCenter { get; set; }

        int m_EntityMarketId;

        /// <summary>
        /// Entity market internal id
        /// </summary>
        [DataMember(Name = "EM_IDEM", Order = 11)]
        public int EntityMarketId
        {
            get { return m_EntityMarketId; }
            set { m_EntityMarketId = value; }
        }

        DateTime m_DateBusiness;

        /// <summary>
        /// Business date set for the current entity/market couple
        /// </summary>
        [DataMember(Name = "EM_DTBUSINESS", Order = 12)]
        public DateTime DateBusiness
        {
            get { return m_DateBusiness; }
            set { m_DateBusiness = value; }
        }

        /// <summary>
        /// Prochaine Business date du couple entity/market
        /// </summary>
        /// PM 20170313 [22833] Ajout DTENTITYNEXT
        [DataMember(Name = "DTENTITYNEXT", Order = 13)]
        public DateTime DateBusinessNext  { get; set; }

        /// <summary>
        /// Business date set externally
        /// </summary>
        public DateTime ForcedDateBusiness
        {
            get;
            set;
        }

        bool m_CrossMarginActivated;


        DateTime m_DateMarket;

        /// <summary>
        /// Market date of the current entity/market couple
        /// </summary>
        /// PM 20150423 [20575] Add DateMarket
        [DataMember(Name = "DTMARKET", Order = 14)]
        public DateTime DateMarket
        {
            get { return m_DateMarket; }
            set { m_DateMarket = value; }
        }

        /// <summary>
        /// activate cross margining (relative to the current entity/market) during position risk evaluation
        /// </summary>
        [DataMember(Name = "EM_CROSSMARGIN", Order = 15)]
        public bool CrossMarginActivated
        {
            get { return m_CrossMarginActivated; }
            set { m_CrossMarginActivated = value; }
        }

        string m_Css;

        /// <summary>
        /// Clearing House identifier
        /// </summary>
        [DataMember(Name = "CSS", Order = 16)]
        public string Css
        {
            get { return m_Css; }
            set { m_Css = value; }
        }

        int m_CssId;

        /// <summary>
        /// Clearing house internal id
        /// </summary>
        [DataMember(Name = "CSSID", Order = 17)]
        public int CssId
        {
            get { return m_CssId; }
            set { m_CssId = value; }
        }

        string m_CssName;

        /// <summary>
        /// Clearing house display name
        /// </summary>
        [DataMember(Name = "CSSNAME", Order = 18)]
        public string CssName
        {
            get { return m_CssName; }
            set { m_CssName = value; }
        }

        string m_CssAcronym;

        /// <summary>
        /// Clearing house Acronyme
        /// </summary>
        // PM 20220111 [25617] Ajout CssAcronym
        [DataMember(Name = "CSSACRONYM", Order = 19)]
        public string CssAcronym
        {
            get { return m_CssAcronym; }
            set { m_CssAcronym = value; }
        }

        #region IDataContractEnabled Membres

        /// <summary>
        /// Clearing house activation date
        /// </summary>
        [DataMember(Name = "CSS_DTEN", Order = 20)]
        public DateTime ElementEnabledFrom
        {
            get;
            set;
        }

        /// <summary>
        /// Clearing house deactivation date
        /// </summary>
        [DataMember(Name = "CSS_DTDIS", Order = 21)]
        public DateTime ElementDisabledFrom
        {
            get;
            set;
        }

        #endregion

        // TODO 20110404 MF changer le type m_CssAssignementMethod en énumération 

        string m_CssAssignementMethod;

        /// <summary>
        /// Clearing House assignement lethod
        /// </summary>
        [DataMember(Name = "CSS_ASSMETH", Order = 22)]
        public string CssAssignementMethod
        {
            get { return m_CssAssignementMethod; }
            set { m_CssAssignementMethod = value; }
        }
        // PM 20160404 [22116] Données déplacée dans la table IMMETHOD
        //string m_CssMarginMethod;

        /// <summary>
        /// Clearing House risk evaluation method (SPAN, ...)
        /// </summary>
        // PM 20160404 [22116] Données déplacée dans la table IMMETHOD
        // [DataMember(Name = "CSS_MARGINMETH", Order = 23)]
        //public string CssMarginMethod
        //{
        //    get { return m_CssMarginMethod; }
        //    set { m_CssMarginMethod = value; }
        //}

        // TODO 20110404 MF changer le type m_CssMarginWeightedMethod en énumération 

        // PM 20160404 [22116] Données déplacée dans la table IMMETHOD
        //string m_CssMarginWeightedMethod;

        /// <summary>
        /// ??
        /// </summary>
        // PM 20160404 [22116] Données déplacée dans la table IMMETHOD
        //[DataMember(Name = "CSS_WEIGHTMETH", Order = 24)]
        //public string CssMarginWeightedMethod
        //{
        //    get { return m_CssMarginWeightedMethod; }
        //    set { m_CssMarginWeightedMethod = value; }
        //}

        // TODO 20110404 MF changer le type m_CssMarginWeightedMethod en énumération 

        // PM 20160404 [22116] Données déplacée dans la table IMMETHOD
        //string m_CssMarginRoundingDirection;

        /// <summary>
        /// Clearing House rounding method for risk evaluation results
        /// </summary>
        // PM 20160404 [22116] Données déplacée dans la table IMMETHOD
        //[DataMember(Name = "CSS_ROUNDDIR", Order = 25)]
        //public string CssMarginRoundingDirection
        //{
        //    get { return m_CssMarginRoundingDirection; }
        //    set { m_CssMarginRoundingDirection = value; }
        //}

        // PM 20160404 [22116] Données déplacée dans la table IMMETHOD
        //int m_CssMarginRoundingPrecision;

        /// <summary>
        /// Clearing House rounding precision for the current rounding method
        /// </summary>
        // PM 20160404 [22116] Données déplacée dans la table IMMETHOD
        //[DataMember(Name = "CSS_ROUNDPREC", Order = 26)]
        //public int CssMarginRoundingPrecision
        //{
        //    get { return m_CssMarginRoundingPrecision; }
        //    set { m_CssMarginRoundingPrecision = value; }
        //}

        // PM 20160404 [22116] Données déplacée dans la table IMMETHOD
        //bool m_CssMarginSpreadingEnabled;

        /// <summary>
        /// ??
        /// </summary>
        // PM 20160404 [22116] Données déplacée dans la table IMMETHOD
        //[DataMember(Name = "CSS_SPRD", Order = 27)]
        //public bool CssMarginSpreadingEnabled
        //{
        //    get { return m_CssMarginSpreadingEnabled; }
        //    set { m_CssMarginSpreadingEnabled = value; }
        //}

        /// <summary>
        /// Css currency, when not null the deposit currency will be converted in the css official currency, if the method
        /// allows it and the fx rate for the current business date exists
        /// </summary>
        [DataMember(Name = "CSS_CURRENCY", Order = 23)]
        public string CssCurrency { get; set; }

        /// <summary>
        /// Id interne des paramètres de la méthode de calcul
        /// </summary>
        /// PM 20160404 [22116] Ajout IDIMMETHODS
        [DataMember(Name = "IDIMMETHODS", Order = 24)]
        public int IdIMMethods { get; set; }

        /// <summary>
        /// Business Center de l'Entité
        /// </summary>
        /// PM 20170313 [22833] Ajout
        [DataMember(Name = "IDBCENTITY", Order = 25)]
        public string EntityBusinessCenter { get; set; }

        /// <summary>
        /// Id interne de la tâche de lecture des RiskDatas
        /// </summary>
        /// PM 20180219 [23824] Ajout IDIOTASK_RISKDATA
        [DataMember(Name = "IDIOTASK_RISKDATA", Order = 26)]
        public int IdIOTaskRiskData { get; set; }

        /// <summary>
        /// Indique s'il faut lire les données de risque pour les jours fériés marché
        /// </summary>
        /// PM 20180530 [23824] Ajout ISRISKDATAONHOLIDAY
        [DataMember(Name = "ISRISKDATAONHOLIDAY", Order = 27)]
        public bool IsRiskDataOnHoliday { get; set; }
    }

    /// <summary>
    /// Class representing a book, 
    /// </summary>
    //RD 20170420 [23092] Add class
    [DataContract(
        Name = DataHelper<Books>.DATASETROWNAME,
        Namespace = DataHelper<Books>.DATASETNAMESPACE)]
    [XmlRoot(ElementName = "Book")]
    public sealed class Books : IDataContractEnabled
    {
        int m_ActorId;

        /// <summary>
        /// Actor internal ID
        /// </summary>
        [DataMember(Name = "ACTORID", Order = 1)]
        public int ActorId
        {
            get { return m_ActorId; }
            set { m_ActorId = value; }
        }

        string m_Book;

        /// <summary>
        /// Book identifier
        /// </summary>
        [DataMember(Name = "BOOK", Order = 2)]
        public string Book
        {
            get { return m_Book; }
            set { m_Book = value; }
        }

        int m_BookId;

        /// <summary>
        /// Book internal id
        /// </summary>
        [DataMember(Name = "BOOKID", Order = 3)]
        public int BookId
        {
            get { return m_BookId; }
            set { m_BookId = value; }
        }

        string m_BookName;

        /// <summary>
        /// Book display name
        /// </summary>
        [DataMember(Name = "BOOKNAME", Order = 4)]
        public string BookName
        {
            get { return m_BookName; }
            set { m_BookName = value; }
        }

        #region IDataContractEnabled Membres

        /// <summary>
        /// Activation date of the book
        /// </summary>
        [DataMember(Name = "DTENABLED", Order = 9)]
        public DateTime ElementEnabledFrom
        {
            get;
            set;
        }

        /// <summary>
        /// Deactivation date of the book
        /// </summary>
        [DataMember(Name = "DTDISABLED", Order = 10)]
        public DateTime ElementDisabledFrom
        {
            get;
            set;
        }

        #endregion
    }
}