#region Using Directives
using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Reflection; 

using EFS.ACommon;
using EFS.Book;
using EFS.Common;
using EFS.Common.Log;
using EFS.Common.MQueue;
using EFS.ApplicationBlocks.Data;

using EfsML.Enum;

using FpML.Enum;
#endregion Using Directives

namespace EFS.Common
{

    /// <summary>
    /// Interface to implemented for class  KeyAsset[Type] (ie: KeyAssetFxRate)
    /// </summary>
    public interface IKeyAsset
    {
        //int GetIdAsset(string pSource);
        int GetIdAsset(string pSource, IDbTransaction pDbTransaction);
    }

    /// <summary>
    /// Représente la clef d'accès aux tables QUOTE_xxx_H
    /// <para>QUOTE_RATEINDEX_H,QUOTE_FXRATE_H etc... </para>
    /// </summary>
    // EG 20190716 [VCL : New FixedIncome] Del _assetMeasure and _cashFlowType _ Add _keyQuoteAdditional
    public class KeyQuote
    {
        #region Members
        /// <summary>
        /// Environnement de marché
        /// </summary>
        private String _idMarketEnv;
        /// <summary>
        /// Scénario
        /// </summary>
        private String _idValScenario;
        /// <summary>
        /// Unité de cotation
        /// </summary>
        private Nullable<Cst.PriceQuoteUnits> _quoteUnit;
        /// <summary>
        /// Type de cotation
        /// </summary>
        private Nullable<QuotationSideEnum> _quoteSide;
        /// <summary>
        /// Notion Temporelle de la cotation
        /// </summary>
        private Nullable<QuoteTimingEnum> _quoteTiming;
        /// <summary>
        /// Date de la cotation
        /// </summary>
        private DateTime _time;
        /// <summary>
        /// 
        /// </summary>
        private string _timeOperator;
        /// <summary>
        /// Données clés additionnelles (IDM, IDC, IDBC, ASSETMEASURE et CASHFLOWTYPE
        /// </summary>
        private KeyQuoteAdditional _keyQuoteAdditional;
        #endregion Members
        //
        #region Property
        /// <summary>
        /// TRUE = Pour ne pas écrire dans le log. Utilisé sur les debtSecurityTransaction 
        /// lorsque recherche d'un prix CleanPrice puis si inexistant DirtyPrice
        /// lorsque recherche d'un taux de coupon 
        /// </summary>
        /// EG 20190730 New
        public bool QuoteNotFoundIsLog
        {
            get { return (_keyQuoteAdditional == null) || _keyQuoteAdditional.quoteNotFoundIsLog; }
        }
        
        /// <summary>
        /// Obtient ou définit le CashFlowType
        /// </summary>
        public KeyQuoteAdditional KeyQuoteAdditional
        {
            get { return _keyQuoteAdditional; }
            set { _keyQuoteAdditional = value; }
        }
        /// <summary>
        /// Obtient ou définit le market Environment
        /// </summary>
        public string IdMarketEnv
        {
            get { return _idMarketEnv; }
            set { _idMarketEnv = value; }
        }
        /// <summary>
        /// Obtient ou définit le scenario de valorisation
        /// </summary>
        public string IdValScenario
        {
            get { return _idValScenario; }
            set { _idValScenario = value; }
        }
        /// <summary>
        /// Obtient ou définit l'unité de prix (ParValueDecimal,ParValueFraction,Price,Rate)
        /// </summary>
        public Nullable<Cst.PriceQuoteUnits> QuoteUnit
        {
            get { return _quoteUnit; }
            set { _quoteUnit = value; }
        }
        /// <summary>
        /// Obtient ou définit le type de prix (Bid,Mid,Ask,OfficialClose,OfficialSettlement)
        /// </summary>
        public Nullable<QuotationSideEnum> QuoteSide
        {
            get { return _quoteSide; }
            set { _quoteSide = value; }
        }
        /// <summary>
        /// Obtient ou définit le QuoteTiming (Close,High,Intraday,High,Low)
        /// </summary>
        public Nullable<QuoteTimingEnum> QuoteTiming
        {
            get { return _quoteTiming; }
            set { _quoteTiming = value; }
        }
        /// <summary>
        /// Obtient ou définit la date de cotation
        /// </summary>
        public DateTime Time
        {
            get { return _time; }
            set { _time = value; }
        }
        /// <summary>
        /// valeurs possibles sont "=" ou "&lt;" ou "&lt;="
        /// </summary>
        public string TimeOperator
        {
            get { return _timeOperator; }
            set { _timeOperator = value; }
        }
        #endregion Property

        #region Constructors
        /// <summary>
        /// Lecture d'un prix à une date
        /// <para>Spheres® utilise le marketEnvironnement par défaut, le valScenario par défaut</para>
        /// <para>Spheres® recherche le prix dont le type est celui par défaut (OfficialClose)</para>    
        /// <para>Spheres® recherche le prix dont le timing est celui par défaut (Close)</para>    
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pTime">date</param>
        public KeyQuote(string pCS, DateTime pTime)
            : this(pCS, pTime, null, null){}
        /// <summary>
        /// Lecture d'un prix à une date, pour un marketEnvironnement, pour un type (Bid,Mid,Ask,etc.)
        /// <para>Spheres® utilise le valscenario par défaut</para>
        /// <para>Spheres® recherche le prix dont le timing est celui par défaut (Close)</para>    
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pTime">date</param>
        /// <param name="pIdMarketEnv">Si null, Spheres® utilise la valeur par defaut</param>
        /// <param name="pQuoteSide">Si null, Spheres® utilise la valeur par defaut (OfficialClose)</param>
        public KeyQuote(string pCS, DateTime pTime, String pIdMarketEnv, Nullable<QuotationSideEnum> pQuoteSide)
            : this(pCS, pTime, pIdMarketEnv, null, pQuoteSide, null){}
        /// <summary>
        /// Lecture d'un prix à une date donné pour un market Environnement, un valscenario, pour un type et un Quotetiming
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pTime"></param>
        /// <param name="pIdMarketEnv">Si null,Spheres® utilise la valeur par defaut</param>
        /// <param name="pIdValScenario">Si null, Spheres® utilise la valeur par defaut</param>
        /// <param name="pQuoteSide">Si null, Spheres® utilise la valeur par defaut (OfficialClose)</param>
        /// <param name="pQuoteTiming">Si null, Spheres® utilise la valeur par defaut (Close)</param>
        // EG 20180205 [23769] Add dbTransaction  
        public KeyQuote(string pCS, DateTime pTime, String pIdMarketEnv, String pIdValScenario,
                    Nullable<QuotationSideEnum> pQuoteSide, Nullable<QuoteTimingEnum> pQuoteTiming)
            : this(pCS, null as IDbTransaction, pTime, pIdMarketEnv, pIdValScenario, pQuoteSide, pQuoteTiming) { }
        // EG 20180205 [23769] Add dbTransaction  
        public KeyQuote(string pCS, IDbTransaction pDbTransaction, DateTime pTime, String pIdMarketEnv, String pIdValScenario,
                    Nullable<QuotationSideEnum> pQuoteSide, Nullable<QuoteTimingEnum> pQuoteTiming)
        {
            _time = pTime;
            _idMarketEnv = pIdMarketEnv;
            _idValScenario = pIdValScenario;
            _quoteSide = pQuoteSide;
            _quoteTiming = pQuoteTiming;
            //
            _timeOperator = "=";
            //
            SetDefaultOnMarketEnv(pCS, pDbTransaction);
            SetDefaultOnValScenario(pCS, pDbTransaction);
        }


        // EG 20190716 [VCL : New FixedIncome] New constructors
        public KeyQuote(string pCS, DateTime pTime, String pIdMarketEnv, String pIdValScenario,
                    Nullable<QuotationSideEnum> pQuoteSide, Nullable<QuoteTimingEnum> pQuoteTiming, KeyQuoteAdditional pKeyQuoteAdditional)
            : this(pCS, null, pTime, pIdMarketEnv, pIdValScenario, pQuoteSide, pQuoteTiming, pKeyQuoteAdditional) { }

        public KeyQuote(string pCS, IDbTransaction pDbTransaction, DateTime pTime, String pIdMarketEnv, String pIdValScenario,
                    Nullable<QuotationSideEnum> pQuoteSide, Nullable<QuoteTimingEnum> pQuoteTiming, KeyQuoteAdditional pKeyQuoteAdditional)
        {
            _time = pTime;
            _idMarketEnv = pIdMarketEnv;
            _idValScenario = pIdValScenario;
            _quoteSide = pQuoteSide;
            _quoteTiming = pQuoteTiming;
            _timeOperator = "=";

            _keyQuoteAdditional = pKeyQuoteAdditional;

            SetDefaultOnMarketEnv(pCS, pDbTransaction);
            SetDefaultOnValScenario(pCS, pDbTransaction);
        }
        /// <summary>
        /// Lecture d'un prix à une date, pour un couple payer/receiver,
        /// <para>Avec le payer et le receiver, spheres® utilise l'environnement de marché adapté et recherche un type de prix (Bid,Ask ou Mid)</para>
        /// <para>Il ne faut pas utiliser ce constructor pour récupérer des prix OfficialClose et OfficialSettlement</para>
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pTime"></param>
        /// <param name="pIdA_Pay"></param>
        /// <param name="pIdB_Pay"></param>
        /// <param name="pIdA_Rec"></param>
        /// <param name="pIdB_Rec"></param>
        // EG 20150706 [21021] Nullable<int> for pIdA_Pay|pIdB_Pay|pIdA_Rec|pIdB_Rec
        public KeyQuote(string pCS, DateTime pTime, Nullable<int> pIdA_Pay, Nullable<int> pIdB_Pay, Nullable<int> pIdA_Rec, Nullable<int> pIdB_Rec)
            : this(pCS, pTime, pIdA_Pay, pIdB_Pay, pIdA_Rec, pIdB_Rec, null) { }
        
        /// <summary>
        /// Lecture d'un prix à une date, pour un couple payer/receiver, pour un QuoteTimingEnum
        /// <para>Avec le payer et le receiver,spheres® utilise l'environnement de marché adapté et recherche un type (Bid,Ask ou Mid)</para>
        /// <para>Il ne faut pas utiliser ce constructor pour récupérer des prix OfficialCloseclose et OfficialSettlement</para>
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pTime"></param>
        /// <param name="pIdA_Pay">Acteur payer</param>
        /// <param name="pIdB_Pay">Book payer</param>
        /// <param name="pIdA_Rec">Acteur Receiver</param>
        /// <param name="pIdB_Rec">Book Receiver</param>
        /// <param name="pQuoteTiming">Si null, Spheres® utilise la valeur par defaut (Close)</param>
        // EG 20150706 [21021] Nullable<int> for pIdA_Pay|pIdB_Pay|pIdA_Rec|pIdB_Rec
        public KeyQuote(string pCS, DateTime pTime,
                        Nullable<int> pIdA_Pay, Nullable<int> pIdB_Pay, Nullable<int> pIdA_Rec, Nullable<int> pIdB_Rec,
                        Nullable<QuoteTimingEnum> pQuoteTiming)
        {
            _time = pTime;
            
            KeyQuotePayerReceiver keyQuoteSide = new KeyQuotePayerReceiver(pCS, pIdA_Pay, pIdB_Pay, pIdA_Rec, pIdB_Rec);
            _idMarketEnv = keyQuoteSide.IdMarketEnv;
            _quoteSide = keyQuoteSide.QuoteSource;
            _idValScenario = string.Empty;
            _quoteTiming = pQuoteTiming;
            
            _timeOperator = "=";
            
            SetDefaultOnMarketEnv(pCS);
            SetDefaultOnValScenario(pCS);
        }
        #endregion Constructors

        #region Methods
        /// <summary>
        /// Alimente la propriété IdMarketEnv avec le MarketEnvironment par défaut si elle est non renseignée
        /// </summary>
        // EG 20180205 [23769] Add dbTransaction  
        private void SetDefaultOnMarketEnv(string pCS)
        {
            SetDefaultOnMarketEnv(pCS, null);
        }
        // EG 20180205 [23769] Add dbTransaction  
        private void SetDefaultOnMarketEnv(string pCS, IDbTransaction pDbTransaction)
        {
            if (StrFunc.IsEmpty(_idMarketEnv))
            {
                //Find default MarketEnvironment
                string SQLSelect = SQLCst.SELECT + "IDMARKETENV" + Cst.CrLf;
                SQLSelect += SQLCst.FROM_DBO + Cst.OTCml_TBL.MARKETENV + Cst.CrLf;
                SQLSelect += SQLCst.WHERE + "ISDEFAULT = 1";

                object obj = DataHelper.ExecuteScalar(pCS, pDbTransaction, CommandType.Text, SQLSelect);
                if (null != obj)
                    _idMarketEnv = Convert.ToString(obj);
            }
        }

        /// <summary>
        /// Alimente la propriété IdValScenario avec le valScenario par défaut si elle est non renseignée
        /// <para>Si IdMarketEnv est connu, Recherche du scenario associé à cet Environment de marché et tel que ISDEFAULT =1</para>
        /// <para>Si IdMarketEnv est inconnu, Recherche du scenario non associé à un environment de marché et tel que ISDEFAULT =1</para>
        /// </summary>
        // EG 20180205 [23769] Add dbTransaction  
        private void SetDefaultOnValScenario(string pCS)
        {
            SetDefaultOnValScenario(pCS, null);
        }
        // EG 20180205 [23769] Add dbTransaction  
        // EG 20190613 [24683] Use Datatable instead of DataReader
        private void SetDefaultOnValScenario(string pCS, IDbTransaction pDbTransaction)
        {
            if (StrFunc.IsEmpty(_idValScenario))
            {
                //Find default ValuationScenario
                string sqlSelect = String.Format(@"select IDVALSCENARIO, 1 as colorder
                from dbo.VALSCENARIO
                where ISDEFAULT = 1 and IDMARKETENV = {0}
                union
                select IDVALSCENARIO, 2 as colorder
                from dbo.VALSCENARIO
                where ISDEFAULT = 1 and IDMARKETENV is null
                order by colorder asc", DataHelper.SQLString(IdMarketEnv));

                object obj = DataHelper.ExecuteScalar(pCS, pDbTransaction, CommandType.Text, sqlSelect);
                if (null != obj)
                    _idValScenario = obj.ToString();
            }
        }
        #endregion
    }

    /// <summary>
    /// Complément d'informations pour une cotation (critères supplémentaires de filtres)
    /// avec comparateur d'égalité par valeurs
    /// 
    /// </summary>
    public sealed class KeyQuoteAdditional : IComparable, IEqualityComparer<KeyQuoteAdditional>
    {
        #region members
        /// <summary>
        /// Devise
        /// </summary>
        public string IdC = null;
        /// <summary>
        /// Marché de la cotation
        /// </summary>
        public Nullable<int> IdM = null;
        /// <summary>
        /// Mesure de la cotation
        /// </summary>
        public Nullable<AssetMeasureEnum> AssetMeasure = null;
        /// <summary>
        /// Source da la cotation
        /// </summary>
        public string Source = null;
        /// <summary>
        /// Business center
        /// </summary>
        public string IdBC = null;
        /// <summary>
        /// Type de paiment
        /// </summary>
        public Nullable<CashFlowTypeEnum> CashflowType = null;
        /// EG 20190730 New
        public bool quoteNotFoundIsLog = true;
        #endregion members
        #region Constructors
        public KeyQuoteAdditional()
        {
        }
        /// EG 20190730 New (Ne pas écrire dans le log si Prix non trouvé voir commentaires sur QuoteNotFoundIsLog)
        public KeyQuoteAdditional(Quote pQuoteSource) : this(pQuoteSource, true) { }
        public KeyQuoteAdditional(Quote pQuoteSource, bool pQuoteNotFoundIsLog)
        {
            AssetMeasure = pQuoteSource.AssetMeasure;
            IdC = pQuoteSource.idC;
            IdBC= pQuoteSource.idBC;
            IdM = pQuoteSource.idM;
            Source = pQuoteSource.source;
            CashflowType = ReflectionTools.ConvertStringToEnumOrNullable<CashFlowTypeEnum>(pQuoteSource.cashFlowType);
            quoteNotFoundIsLog = pQuoteNotFoundIsLog;
        }
        
        public KeyQuoteAdditional(AssetMeasureEnum pAssetMeasure)
        {
            AssetMeasure = pAssetMeasure;
        }
        /// EG 20190730 New (Ne pas écrire dans le log si Prix non trouvé voir commentaires sur QuoteNotFoundIsLog)
        public KeyQuoteAdditional(AssetMeasureEnum pAssetMeasure, bool pQuoteNotFoundIsLog)
        {
            AssetMeasure = pAssetMeasure;
            quoteNotFoundIsLog = pQuoteNotFoundIsLog;
        }
        #endregion Constructors

        #region IEqualityComparer
        /// <summary>
        /// Les éléments de cotation sont égaux si tous leurs membres sont égaux
        /// </summary>
        /// <param name="pContractX">1er KeyQuoteAdditional à comparer</param>
        /// <param name="pContractY">2ème KeyQuoteAdditional à comparer</param>
        /// <returns>true si X Equals Y, sinon false</returns>
        public bool Equals(KeyQuoteAdditional pKeyQuoteAdditionalX, KeyQuoteAdditional pKeyQuoteAdditionalY)
        {
            return (pKeyQuoteAdditionalX == pKeyQuoteAdditionalY)
                || ((pKeyQuoteAdditionalX != default(KeyQuoteAdditional))
                 && (pKeyQuoteAdditionalY != default(KeyQuoteAdditional))
                 && (pKeyQuoteAdditionalX.Source == pKeyQuoteAdditionalY.Source)
                 && (pKeyQuoteAdditionalX.IdC == pKeyQuoteAdditionalY.IdC)
                 && (pKeyQuoteAdditionalX.IdM == pKeyQuoteAdditionalY.IdM)
                 && (pKeyQuoteAdditionalX.AssetMeasure == pKeyQuoteAdditionalY.AssetMeasure)
                 && (pKeyQuoteAdditionalX.IdBC == pKeyQuoteAdditionalY.IdBC)
                 && (pKeyQuoteAdditionalX.CashflowType == pKeyQuoteAdditionalY.CashflowType));
        }

        /// <summary>
        /// La méthode GetHashCode fournissant la même valeur pour des objets QuoteAdditionalKey qui sont égaux.
        /// </summary>
        /// <param name="pMaturity">Le QuoteAdditionalKey dont on veut le hash code</param>
        /// <returns>La valeur du hash code</returns>
        public int GetHashCode(KeyQuoteAdditional pKeyQuoteAdditional)
        {
            //Vérifier si l'objet est null
            if (pKeyQuoteAdditional is null) return 0;

            //Obtenir le hash code de la mesure si non null.
            int hashAssetMeasure = pKeyQuoteAdditional.AssetMeasure.HasValue ? pKeyQuoteAdditional.AssetMeasure.GetHashCode() : 0;

            //Obtenir le hash code du type de paiment si non null.
            int hashCashFlowType = pKeyQuoteAdditional.CashflowType.HasValue ? pKeyQuoteAdditional.CashflowType.GetHashCode() : 0;

            //Obtenir le hash code du Business center si non null.
            int hashIdBC = StrFunc.IsEmpty(pKeyQuoteAdditional.IdBC) ? 0 : pKeyQuoteAdditional.IdBC.GetHashCode();

            //Obtenir le hash code de la devise si non null.
            int hashIdC = StrFunc.IsEmpty(pKeyQuoteAdditional.IdC) ? 0 : pKeyQuoteAdditional.IdC.GetHashCode();

            //Obtenir le hash code du marché si non null.
            int hashIdM = pKeyQuoteAdditional.IdM.HasValue ? pKeyQuoteAdditional.IdM.Value.GetHashCode() : 0;

            //Obtenir le hash code de la source si non null.
            int hashSource = StrFunc.IsEmpty(pKeyQuoteAdditional.Source) ? 0 : pKeyQuoteAdditional.Source.GetHashCode();

            //Calcul du hash code pour le contrat.
            return (int)(hashAssetMeasure ^ hashCashFlowType ^ hashIdBC ^ hashIdC ^ hashIdM ^ hashSource);
        }
        #endregion IEqualityComparer

        #region IComparable.CompareTo
        /// <summary>
        /// Compare l'instance courante à une autre
        /// </summary>
        /// <param name="pObj">Autre instance de MeffContract à comparer</param>
        /// <returns>0 si les objets sont égaux</returns>
        public int CompareTo(object pObj)
        {
            if (pObj is KeyQuoteAdditional keyQuoteAdditional)
            {
                int ret = -1;
                if (keyQuoteAdditional != default(KeyQuoteAdditional))
                {
                    ret = (AssetMeasure == keyQuoteAdditional.AssetMeasure ? 0 : (AssetMeasure.HasValue ? AssetMeasure.Value.CompareTo(keyQuoteAdditional.AssetMeasure) : -2));
                    if (0 == ret)
                    {
                        ret = (CashflowType == keyQuoteAdditional.CashflowType ? 0 : (CashflowType.HasValue ? CashflowType.Value.CompareTo(keyQuoteAdditional.CashflowType) : -3));
                        if (0 == ret)
                        {
                            ret = (IdBC == keyQuoteAdditional.IdBC ? 0 : (IdBC == null ? -4 : IdBC.CompareTo(keyQuoteAdditional.IdBC)));
                            if (0 == ret)
                            {
                                ret = (IdC == keyQuoteAdditional.IdC ? 0 : (IdC == null ? -5 : IdC.CompareTo(keyQuoteAdditional.IdC)));
                                if (0 == ret)
                                {
                                    ret = (IdM == keyQuoteAdditional.IdM ? 0 : (IdM.HasValue ? IdM.Value.CompareTo(keyQuoteAdditional.IdM) : -6));
                                    if (0 == ret)
                                    {
                                        ret = (Source == keyQuoteAdditional.Source ? 0 : (Source == null ? -7 : Source.CompareTo(keyQuoteAdditional.Source)));
                                    }
                                }
                            }
                        }
                    }
                }
                return ret;
            }
            throw new ArgumentException("object is not a MeffContract");
        }
        #endregion IComparable.CompareTo
    }
    /// <summary>
    /// 
    /// </summary>
    public struct KeyQuoteFxRate
    {
        public decimal QuoteValue;
        public QuoteBasisEnum QuoteBasis;
        public int IdQuote;
    }

    /// <summary>
    /// Représente le triplet {Acteur,Book,Entité} et le Market Environnement associé à un payer ou un receiver de flux 
    /// <para>Market Environnement est utilisé dans la recherche des prix des assets</para>
    /// </summary>
    // EG 20150706 [21021] Nullable<int> for m_IdB
    public class KeyQuotePayerReceiverItem
    {
        #region Members
        private readonly int m_IdA;
        private readonly int m_IdA_Entity;
        private readonly Nullable<int> m_IdB;
        private readonly bool m_IsBookClient;

        private readonly string m_IdMarketEnv;
        #endregion Members

        #region Property
        /// <summary>
        /// Obtient l'environnement de marché
        /// </summary>
        public string IdMarketEnv
        {
            get { return m_IdMarketEnv; }
        }

        /// <summary>
        /// Obtient true si le book est spécifié
        /// </summary>
        // EG 20150706 [21021]
        public bool IsBookSpecified
        {
            get { return m_IdB.HasValue; }
        }

        /// <summary>
        /// Obtient true si le l'acteur est un client (s'il a le rôle client)
        /// </summary>
        public bool IsBookClient
        {
            get { return m_IsBookClient; }
        }

        /// <summary>
        /// Obtient true si le book est managé
        /// </summary>
        // EG 20150706 [21021]
        public bool IsBookManaged
        {
            get { return (0 < m_IdA_Entity); }
        }
        #endregion Property

        #region Constructors
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pIdA"></param>
        /// <param name="pIdB"></param>
        // EG 20150706 [21021] Nullable<int> for pIdB
        public KeyQuotePayerReceiverItem(string pCS, int pIdA, Nullable<int> pIdB)
        {
            m_IdA = pIdA;
            m_IdB = pIdB;

            if (IsBookSpecified)
            {
                SQL_Book book = new SQL_Book(pCS, m_IdB.Value);
                if (book.IsLoaded)
                {
                    m_IdA_Entity = book.IdA_Entity;
                    if (IsBookManaged)
                    {
                        SQL_Actor actor = new SQL_Actor(pCS, m_IdA_Entity)
                        {
                            WithInfoEntity = true
                        };
                        if (actor.IsLoaded)
                        {
                            m_IsBookClient = BookTools.IsCounterPartyClient(pCS, m_IdA, m_IdB);
                            m_IdMarketEnv = (m_IsBookClient ? actor.IdMarketEnv_Client : actor.IdMarketEnv);
                        }
                    }
                }
            }
        }
        #endregion Constructors
    }

    /// <summary>
    /// Représente la source de la cotation {Bid,Mid,Ask},l'environment de marché associés à un couple {payer,receiver}
    /// <para>Market Environnement et le type de cotation sont utilisés dans la recherche des prix des assets</para>
    /// </summary>
    public class KeyQuotePayerReceiver
    {
        #region Members
        private bool _payerSpecified;
        private KeyQuotePayerReceiverItem _payer;
        //
        private bool _receiverSpecified;
        private KeyQuotePayerReceiverItem _receiver;
        //
        private readonly string _idMarketEnv;
        private readonly QuotationSideEnum _quoteSide;
        #endregion Members

        #region Property
        /// <summary>
        /// Obtient l'environnement de marché
        /// </summary>
        public string IdMarketEnv
        {
            get { return _idMarketEnv; }
        }

        /// <summary>
        /// Obtient la source de cotation Bid,Mid,Ask,OfficialClose,OfficialSettlement
        /// </summary>
        public QuotationSideEnum QuoteSource
        {
            get { return _quoteSide; }
        }
        #endregion Property

        #region Constructors
        /// <summary>
        ///  Constructeur 
        /// <para>Le type de cotation et le MarketEnvironnent sont déterminés en fonction des payers et des receivers</para>
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pIdA_Pay"></param>
        /// <param name="pIdB_Pay"></param>
        /// <param name="pIdA_Rec"></param>
        /// <param name="pIdB_Rec"></param>
        // EG 20150706 [21021] Nullable<int> for pIdA_Pay|pIdB_Pay|pIdA_Rec|pIdB_Rec
        public KeyQuotePayerReceiver(string pCS, Nullable<int> pIdA_Pay, Nullable<int> pIdB_Pay, Nullable<int> pIdA_Rec, Nullable<int> pIdB_Rec)
        {
            _quoteSide = QuotationSideEnum.Mid;

            SetKeyQuotePayer(pCS, pIdA_Pay, pIdB_Pay);
            SetKeyQuoteReceiver(pCS, pIdA_Rec, pIdB_Rec);

            if (_payerSpecified && _payer.IsBookClient)
            {
                _quoteSide = QuotationSideEnum.Ask;
                _idMarketEnv = _payer.IdMarketEnv ;
            }
            else if (_receiverSpecified && _receiver.IsBookClient)
            {
                _quoteSide = QuotationSideEnum.Bid;
                _idMarketEnv = _receiver.IdMarketEnv;
            }
            else if (_payerSpecified && _payer.IsBookManaged)
            {
                _quoteSide = QuotationSideEnum.Mid;
                _idMarketEnv = _payer.IdMarketEnv;
            }
            else if (_receiverSpecified && _receiver.IsBookManaged)
            {
                _quoteSide = QuotationSideEnum.Mid;
                _idMarketEnv = _receiver.IdMarketEnv;
            }
        }
        #endregion Constructors
       
        #region Methods
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pIdA"></param>
        /// <param name="pIdB"></param>
        // EG 20150706 [21021] Nullable<int> for pIdA|pIdB
        public void SetKeyQuotePayer(string pCS, Nullable<int> pIdA, Nullable<int> pIdB)
        {
            _payerSpecified = (pIdA.HasValue);
            if (_payerSpecified)
                _payer = new KeyQuotePayerReceiverItem(pCS, pIdA.Value, pIdB);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pIdA"></param>
        /// <param name="pIdB"></param>
        // EG 20150706 [21021] Nullable<int> for pIdA|pIdB 
        public void SetKeyQuoteReceiver(string pCS, Nullable<int> pIdA, Nullable<int> pIdB)
        {
            _receiverSpecified = pIdA.HasValue;
            if (_receiverSpecified)
                _receiver = new KeyQuotePayerReceiverItem(pCS, pIdA.Value, pIdB);
        }
        #endregion Methods
    }

    /// <summary>
    /// 
    /// </summary>
    public class KeyTenor
    {
        public int multiplier;
        public PeriodEnum period;
    }

    /// <summary>
    /// Clé d'accès aux tables ASSET_FXRATE
    /// </summary>
    public class KeyAssetFxRate : IKeyAsset
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("idC1")]
        public string IdC1;
        [System.Xml.Serialization.XmlElementAttribute("idC2")]
        public string IdC2;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool QuoteBasisSpecified;
        [System.Xml.Serialization.XmlElementAttribute("quoteBasis")]
        public QuoteBasisEnum QuoteBasis;
        [System.Xml.Serialization.XmlElementAttribute("idBCRateSrc")]
        public string IdBCRateSrc;
        [System.Xml.Serialization.XmlElementAttribute("rateSrc")]
        public string PrimaryRateSrc;
        [System.Xml.Serialization.XmlElementAttribute("rateSrcPage")]
        public string PrimaryRateSrcPage;
        [System.Xml.Serialization.XmlElementAttribute("rateSrcHead")]
        public string PrimaryRateSrcHead;
        [System.Xml.Serialization.XmlElementAttribute("isDefault")]
        public bool IsDefault;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool IsDefaultSpecified;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool QuoteBasisSourceSpecified;
        [System.Xml.Serialization.XmlElementAttribute("quoteBasisSource")]
        public QuoteBasisEnum QuoteBasisSource;
        #endregion

        #region Membres de IKeyAsset
        #region public GetIdAsset
        /// <summary>
        /// Recherche de l'asset 
        /// </summary>
        /// <param name="pSource"></param>
        /// <returns></returns>
        // EG 20180307 [23769] Gestion dbTransaction
        public int GetIdAsset(string pSource)
        {
            return GetIdAsset(pSource, null);
        }
        // EG 20180307 [23769] Gestion dbTransaction
        public int GetIdAsset(string pSource, IDbTransaction pDbTransaction)
        {
            // 20090609 RD Ajout de SQL Parameters
            int id = 0;
            
            SetQuoteBasis();
            
            StrBuilder sqlSelect = new StrBuilder();
            sqlSelect += SQLCst.SELECT + "a.IDASSET" + Cst.CrLf;
            //
            StrBuilder sqlFrom = new StrBuilder();
            sqlFrom += SQLCst.FROM_DBO + Cst.OTCml_TBL.ASSET_FXRATE.ToString() + " a" + Cst.CrLf;
            //
            StrBuilder sqlWhere = new StrBuilder();
            sqlWhere += SQLCst.WHERE;
            sqlWhere += OTCmlHelper.GetSQLDataDtEnabled(pSource, "a") + Cst.CrLf;
            sqlWhere += SQLCst.AND;
            //
            sqlWhere += "((a.QCP_IDC1 = @IDC1";
            sqlWhere += SQLCst.AND + "a.QCP_IDC2 = @IDC2";
            sqlWhere += SQLCst.AND + "a.QCP_QUOTEBASIS = @QUOTEBASIS";
            sqlWhere += ")" + SQLCst.OR + "(";
            sqlWhere += "a.QCP_IDC1 = @IDC2";
            sqlWhere += SQLCst.AND + "a.QCP_IDC2 = @IDC1";
            sqlWhere += SQLCst.AND + "a.QCP_QUOTEBASIS = @QUOTEBASIS2))";
            //
            DataParameters parametres = new DataParameters();
            //
            parametres.Add(new DataParameter(pSource, "IDC1", DbType.AnsiString, SQLCst.UT_CURR_LEN), IdC1);
            parametres.Add(new DataParameter(pSource, "IDC2", DbType.AnsiString, SQLCst.UT_CURR_LEN), IdC2);
            parametres.Add(new DataParameter(pSource, "QUOTEBASIS", DbType.AnsiString, SQLCst.UT_ENUM_MANDATORY_LEN), QuoteBasis.ToString());
            //
            if (QuoteBasisEnum.Currency1PerCurrency2 == QuoteBasis)
                parametres.Add(new DataParameter(pSource, "QUOTEBASIS2", DbType.AnsiString, SQLCst.UT_ENUM_MANDATORY_LEN), QuoteBasisEnum.Currency2PerCurrency1.ToString());
            else
                parametres.Add(new DataParameter(pSource, "QUOTEBASIS2", DbType.AnsiString, SQLCst.UT_ENUM_MANDATORY_LEN), QuoteBasisEnum.Currency1PerCurrency2.ToString());
            //            
            if (StrFunc.IsFilled(IdBCRateSrc))
            {
                sqlWhere += SQLCst.AND + "a.IDBCRATESRC = @IDBCRATESRC";
                parametres.Add(new DataParameter(pSource, "IDBCRATESRC", DbType.AnsiString, SQLCst.UT_ENUM_MANDATORY_LEN), IdBCRateSrc);
            }
            //
            if (StrFunc.IsFilled(PrimaryRateSrc))
            {
                sqlWhere += SQLCst.AND + "a.PRIMARYRATESRC = @PRIMARYRATESRC";
                parametres.Add(new DataParameter(pSource, "PRIMARYRATESRC", DbType.AnsiString, SQLCst.UT_UNC_LEN), PrimaryRateSrc);
            }
            //
            if (StrFunc.IsFilled(PrimaryRateSrcPage))
            {
                sqlWhere += SQLCst.AND + "a.PRIMARYRATESRCPAGE = @PRIMARYRATESRCPAGE";
                parametres.Add(new DataParameter(pSource, "PRIMARYRATESRCPAGE", DbType.AnsiString, SQLCst.UT_UNC_LEN), PrimaryRateSrcPage);
            }
            //
            if (StrFunc.IsFilled(PrimaryRateSrcHead))
            {
                sqlWhere += SQLCst.AND + "a.PRIMARYRATESRCHEAD = @PRIMARYRATESRCHEAD";
                parametres.Add(new DataParameter(pSource, "PRIMARYRATESRCHEAD", DbType.AnsiString, SQLCst.UT_UNC_LEN), PrimaryRateSrcHead);
            }
            //
            if (IsDefaultSpecified)
            {
                sqlWhere += SQLCst.AND + "a.ISDEFAULT = @ISDEFAULT";
                parametres.Add(new DataParameter(pSource, "ISDEFAULT", DbType.Boolean), DataHelper.SQLBoolean(IsDefault));
            }
            //
            StrBuilder sqlOrder = new StrBuilder();
            if (!IsDefaultSpecified)
            {
                //20070920 PL Add SQLCst.DESC
                sqlOrder += Cst.CrLf + SQLCst.ORDERBY + "a.ISDEFAULT" + SQLCst.DESC;
            }
            //
            //20070717 FI utilisation de ExecuteScalar pour le Cache
            object obj = DataHelper.ExecuteScalar(pSource, pDbTransaction, CommandType.Text, sqlSelect.ToString() + sqlFrom.ToString() + sqlWhere.ToString() + sqlOrder.ToString(), parametres.GetArrayDbParameter());
            if (null != obj)
                id = Convert.ToInt32(obj);
            //			
            return id;
        }
        #endregion
        #endregion

        #region Methods

        /// <summary>
        ///  Mise en place du QuoteBasis en fonction des devises
        /// <para>QuoteBasis est déterminé selon les conventions habituelles établies sur les marchés</para>
        /// <para>Par Exemple l'EURO est généralement côté au certain vis à vis de l'USD</para>
        /// </summary>
        public void SetQuoteBasis()
        {
            SetQuoteBasis(false);
        }

        /// <summary>
        ///  Mise en place du QuoteBasis en fonction des devises
        /// <para>QuoteBasis est déterminé selon les conventions habituelles établies sur les marchés</para>
        /// <para>Par Exemple l'EURO est généralement côté au certain vis à vis de l'USD</para>
        /// </summary>
        /// <param name="pIsForced"></param>
        /// PL 20100125 Add Test on USD and GBP (source EPL)
        public void SetQuoteBasis(bool pIsForced)
        {

            if ((!this.QuoteBasisSpecified) || pIsForced)
            {
                QuoteBasisSpecified = true;
                //
                if ((this.IdC1 == "XAU") || (this.IdC1 == "XAG") || (this.IdC1 == "XPD") || (this.IdC1 == "XPT"))
                    QuoteBasis = QuoteBasisEnum.Currency2PerCurrency1;
                else if ((this.IdC2 == "XAU") || (this.IdC2 == "XAG") || (this.IdC2 == "XPD") || (this.IdC2 == "XPT"))
                    QuoteBasis = QuoteBasisEnum.Currency1PerCurrency2;
                else if (this.IdC1 == "EUR")
                    QuoteBasis = QuoteBasisEnum.Currency2PerCurrency1;
                else if (this.IdC2 == "EUR")
                    QuoteBasis = QuoteBasisEnum.Currency1PerCurrency2;
                else if (this.IdC1 == "GBP")
                    QuoteBasis = QuoteBasisEnum.Currency2PerCurrency1;
                else if (this.IdC2 == "GBP")
                    QuoteBasis = QuoteBasisEnum.Currency1PerCurrency2;
                else if (this.IdC1 == "USD" && (this.IdC2 != "AUD" && this.IdC2 != "NZD"))
                    QuoteBasis = QuoteBasisEnum.Currency2PerCurrency1;
                else if (this.IdC2 == "USD" && (this.IdC1 != "AUD" && this.IdC1 != "NZD"))
                    QuoteBasis = QuoteBasisEnum.Currency1PerCurrency2;
                else
                    //Valeur subjective...
                    QuoteBasis = QuoteBasisEnum.Currency1PerCurrency2;
                //SQL_Currency() ...
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pQuoteBasis"></param>
        /// <param name="pCurrency1"></param>
        /// <param name="pCurrency2"></param>
        /// <returns></returns>
        public bool IsReverseQuotation(QuoteBasisEnum pQuoteBasis, string pCurrency1, string pCurrency2)
        {
            bool isReverse = false;
            if (QuoteBasisSpecified)
            {
                if (QuoteBasis == pQuoteBasis)
                {
                    if ((IdC1 != pCurrency1) || (IdC2 != pCurrency2))
                        isReverse = true;
                }
                else if ((IdC1 != pCurrency2) || (IdC2 != pCurrency1))
                    isReverse = true;
            }
            return isReverse;

        }
        #endregion
    }

    /// <summary>
    /// Clé d'accès à la table ASSET_RATEINDEX
    /// </summary>
    public class KeyAssetRateIndex : IKeyAsset
    {
        #region Membres
        public int idRX;
        public string rateIndex_Identifier;
        public string rateIndex_IdISDA;
        public int periodMultiplier;
        public PeriodEnum period;
        #endregion

        #region Membres de IKeyAsset
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCS"></param>
        /// <returns></returns>
        // EG 20180423 Analyse du code Correction [CA2200]
        int IKeyAsset.GetIdAsset(string pCS, IDbTransaction pDbTransaction)
        {
            try
            {
                int id = 0;
                //
                string SQLSelect = SQLCst.SELECT + "a.IDASSET" + Cst.CrLf;
                string SQLFrom = SQLCst.FROM_DBO + Cst.OTCml_TBL.ASSET_RATEINDEX.ToString() + " a" + Cst.CrLf;
                //
                string sqlFrom_RateIndex = SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.RATEINDEX + " r";
                sqlFrom_RateIndex += SQLCst.ON + "(r.IDRX = a.IDRX)" + Cst.CrLf;
                //
                string SQLWhere = SQLCst.WHERE;
                if (this.idRX > 0)
                {
                    SQLWhere += "(a.IDRX = " + idRX.ToString() + ")";
                }
                else if (StrFunc.IsFilled(this.rateIndex_Identifier))
                {
                    SQLFrom += sqlFrom_RateIndex;
                    SQLWhere += "(r.IDENTIFIER = " + DataHelper.SQLString(this.rateIndex_Identifier) + ")";
                }
                else if (StrFunc.IsFilled(this.rateIndex_IdISDA))
                {
                    SQLFrom += sqlFrom_RateIndex;
                    SQLWhere += "(r.IDISDA = " + DataHelper.SQLString(this.rateIndex_IdISDA) + ")";
                }
                //
                if (this.periodMultiplier > 0)
                {
                    SQLWhere += SQLCst.AND + "(r.PERIODMLTP = " + this.periodMultiplier.ToString() + ")";
                    SQLWhere += SQLCst.AND + "(r.PERIOD = " + DataHelper.SQLString(this.period.ToString()) + ")";
                }
                //
                //20070717 FI utilisation de ExecuteScalar pour le Cache
                Object obj = DataHelper.ExecuteScalar(pCS, pDbTransaction, CommandType.Text, SQLSelect + SQLFrom + SQLWhere);
                if (null != obj)
                    id = Convert.ToInt32(obj);
                //
                return id;

            }
            catch (Exception) { throw; }
        }
        #endregion
    }
}