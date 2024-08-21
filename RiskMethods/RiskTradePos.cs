using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Xml.Serialization;

using EFS.ACommon;
using EFS.ApplicationBlocks.Data;
using EFS.Common;
using EFS.SpheresRiskPerformance.DataContracts;

using FpML.Interface;
using FpML.v44.Shared;

namespace EFS.SpheresRiskPerformance
{
    /// <summary>
    /// IEqualityComparer pour la class RiskActorBook
    /// </summary>
    public class RiskActorBookComparer : IEqualityComparer<RiskActorBook>
    {
        #region IEqualityComparer
        /// <summary>
        /// Les RiskActorBook sont egaux si tous leurs membres sont égaux
        /// </summary>
        /// <param name="pX">1er RiskActorBook à comparer</param>
        /// <param name="pY">2ème RiskActorBook à comparer</param>
        /// <returns>true si x Equals Y, sinon false</returns>
        public bool Equals(RiskActorBook pX, RiskActorBook pY)
        {
            return (pX == pY)
                || ((pX != default)
                    && (pY != default)
                    && (pX.IdA == pY.IdA)
                    && (pX.IdB == pY.IdB));
        }

        /// <summary>
        /// La méthode GetHashCode fournissant la même valeur pour des objets qui sont égaux.
        /// </summary>
        /// <param name="pObj">L'objet dont on veut le hash code</param>
        /// <returns>La valeur du hash code</returns>
        public int GetHashCode(RiskActorBook pObj)
        {
            //Vérifier si l'obet est null
            if (pObj is null) return 0;

            //Calcul du hash code pour l'objet.
            return (int)(pObj.IdA.GetHashCode() ^ pObj.IdB.GetHashCode());
        }
        #endregion IEqualityComparer
    }

    /// <summary>
    /// IEqualityComparer pour la class TradeValue
    /// </summary>
    /// PM 20170808 [23371] New
    public class TradeValueComparer : IEqualityComparer<TradeValue>
    {
        #region IEqualityComparer
        /// <summary>
        /// Les TradeValue sont egaux s'ils ont le même IDT
        /// </summary>
        /// <param name="pX">1er TradeValue à comparer</param>
        /// <param name="pY">2ème TradeValue à comparer</param>
        /// <returns>true si x Equals Y, sinon false</returns>
        public bool Equals(TradeValue pX, TradeValue pY)
        {
            return (pX == pY)
                || ((pX != default(TradeValue))
                    && (pY != default(TradeValue))
                    && (pX.IdT == pY.IdT));
        }

        /// <summary>
        /// La méthode GetHashCode fournissant la même valeur pour des objets qui sont égaux.
        /// </summary>
        /// <param name="pObj">L'objet dont on veut le hash code</param>
        /// <returns>La valeur du hash code</returns>
        public int GetHashCode(TradeValue pObj)
        {
            //Vérifier si l'obet est null
            if (pObj is null) return 0;

            //Calcul du hash code pour l'objet.
            return (int)(pObj.IdT.GetHashCode());
        }
        #endregion IEqualityComparer
    }

    /// <summary>
    /// Classe identifant un couple Actor/Book
    /// </summary>
    public class RiskActorBook
    {
        #region Members
        private readonly int m_IdA;
        private readonly int m_IdB;
        #endregion Members

        #region Accessors
        /// <summary>
        /// Identifiant interne de l'acteur
        /// </summary>
        public int IdA
        {
            get { return m_IdA; }
        }
        /// <summary>
        /// Identifiant interne du book
        /// </summary>
        public int IdB
        {
            get { return m_IdB; }
        }
        #endregion Accessors

        #region Constructor
        /// <summary>
        /// Constructeur vide nécessaire à la sérialisation en mode log Full
        /// </summary>
        // PM 20200129 ajout
        public RiskActorBook() { }

        /// <summary>
        /// Constructeur
        /// </summary>
        /// <param name="pIdA">Id de l'acteur</param>
        /// <param name="pIdB">Id du book</param>
        public RiskActorBook(int pIdA, int pIdB)
        {
            m_IdA = pIdA;
            m_IdB = pIdB;
        }
        #endregion Constructor

        #region override methods
        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(Object obj)
        {
            return ((obj != null) && (GetType() == obj.GetType()) && (this == (RiskActorBook)obj));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return (m_IdA.GetHashCode() ^ m_IdB.GetHashCode());
        }

        /// <summary>
        /// x == y
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public static bool operator ==(RiskActorBook x, RiskActorBook y)
        {
            if (ReferenceEquals(x, y)) return true;

            if (x is null || y is null) return false;

            return (x.m_IdA == y.m_IdA) && (x.m_IdB == y.m_IdB);
        }

        /// <summary>
        /// x != y
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public static bool operator !=(RiskActorBook x, RiskActorBook y)
        {
            return !(x == y);
        }
        #endregion override methods
    }

    /// <summary>
    /// IEqualityComparer pour la class RiskDataTradeKey
    /// </summary>
    public class RiskDataTradeKeyComparer : IEqualityComparer<RiskDataTradeKey>
    {
        #region IEqualityComparer
        /// <summary>
        /// Les RiskDataTradeKey sont egaux si tous leurs membres sont égaux
        /// </summary>
        /// <param name="pX">1er RiskDataTradeKey à comparer</param>
        /// <param name="pY">2ème RiskDataTradeKey à comparer</param>
        /// <returns>true si x Equals Y, sinon false</returns>
        // PM 20200910 [25482] ajout IdAsset 
        public bool Equals(RiskDataTradeKey pX, RiskDataTradeKey pY)
        {
            return (pX == pY)
                || ((pX != default(RiskDataTradeKey))
                    && (pY != default(RiskDataTradeKey))
                    && (pX.IdA_Dealer == pY.IdA_Dealer)
                    && (pX.IdB_Dealer == pY.IdB_Dealer)
                    && (pX.IdA_Clearer == pY.IdA_Clearer)
                    && (pX.IdB_Clearer == pY.IdB_Clearer)
                    && (pX.IdAsset == pY.IdAsset)
                    && (pX.DtBusiness == pY.DtBusiness));
        }

        /// <summary>
        /// La méthode GetHashCode fournissant la même valeur pour des objets qui sont égaux.
        /// </summary>
        /// <param name="pObj">L'objet dont on veut le hash code</param>
        /// <returns>La valeur du hash code</returns>
        // PM 20200910 [25482] ajout IdAsset 
        public int GetHashCode(RiskDataTradeKey pObj)
        {
            //Vérifier si l'obet est null
            if (pObj is null) return 0;

            //Calcul du hash code pour l'objet.
            return (int)(pObj.IdA_Dealer.GetHashCode()
                ^ pObj.IdB_Dealer.GetHashCode()
                ^ pObj.IdA_Clearer.GetHashCode()
                ^ pObj.IdB_Clearer.GetHashCode()
                ^ pObj.IdAsset.GetHashCode()
                ^ pObj.DtBusiness.GetHashCode());
        }
        #endregion IEqualityComparer
    }

    /// <summary>
    /// Classe utilisée comme clé pour les valeurs des trades sur lesquelles porte un calcul de déposit
    /// </summary>
    /// PM 20170313 [22833] New
    /// PM 20200910 [25482] Ajout IdAsset comme élément de la clé
    public class RiskDataTradeKey
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("idA_Dealer")]
        public int IdA_Dealer;
        [System.Xml.Serialization.XmlElementAttribute("idB_Dealer")]
        public int IdB_Dealer;
        [System.Xml.Serialization.XmlElementAttribute("idA_Clearer")]
        public int IdA_Clearer;
        [System.Xml.Serialization.XmlElementAttribute("idB_Clearer")]
        public int IdB_Clearer;
        [System.Xml.Serialization.XmlElementAttribute("idAsset")]
        public int IdAsset;
        [System.Xml.Serialization.XmlElementAttribute("dtBusiness")]
        public DateTime DtBusiness;
        #endregion Members

        #region Constructor
        /// <summary>
        /// Constructeur
        /// </summary>
        public RiskDataTradeKey() { }

        /// <summary>
        /// Constructeur
        /// </summary>
        /// <param name="pIdA_Dealer"></param>
        /// <param name="pIdB_Dealer"></param>
        /// <param name="pIdA_Clearer"></param>
        /// <param name="pIdB_Clearer"></param>
        /// <param name="pIdAsset"></param>
        /// <param name="pDtBusiness"></param>
        // PM 20200910 [25482] Ajout IdAsset en paramètre
        //public RiskDataTradeKey(int pIdA_Dealer, int pIdB_Dealer, int pIdA_Clearer, int pIdB_Clearer, DateTime pDtBusiness)
        public RiskDataTradeKey(int pIdA_Dealer, int pIdB_Dealer, int pIdA_Clearer, int pIdB_Clearer, int pIdAsset, DateTime pDtBusiness)
        {
            IdA_Dealer = pIdA_Dealer;
            IdB_Dealer = pIdB_Dealer;
            IdA_Clearer = pIdA_Clearer;
            IdB_Clearer = pIdB_Clearer;
            IdAsset = pIdAsset;
            DtBusiness = pDtBusiness;
        }
        #endregion Constructor
    }

    /// <summary>
    /// Classe des données sur lesquelles portera l'évaluation du risque
    /// </summary>
    /// PM 20170313 [22833] New
    public class RiskData
    {
        #region Members
        /// <summary>
        /// Position ETD sur laquelle porte un calcul de déposit
        /// </summary>
        private RiskDataPosition m_PositionETD;

        /// <summary>
        /// Valeurs des trades COM sur lesquelles porte un calcul de déposit
        /// </summary>
        private RiskDataTradeValue m_TradeValueCOM;

        /// <summary>
        /// Trades du jour pour lesquels ne pas calculer de déposit
        /// </summary>
        // PM 20221212 [XXXXX] New
        private RiskDataNoMarginTrade m_NoMarginTrade;
        #endregion Members

        #region Accessors
        /// <summary>
        /// Position ETD sur laquelle porte un calcul de déposit
        /// </summary>
        public RiskDataPosition PositionETD
        {
            get { return m_PositionETD; }
        }

        /// <summary>
        /// Asset ETD sql elements cache
        /// key = IdAsset, value = SQL_AssetETD
        /// </summary>
        [XmlIgnore]
        public Dictionary<int, SQL_AssetETD> AssetETDCache
        {
            get { return m_PositionETD.AssetETDCache; }
        }

        /// <summary>
        /// Cache pour les SQL_AssetCommodityContract
        /// key = IdCC, value = SQL_AssetCommodityContract
        /// </summary>
        // PM 20200910 [25481] ajout AssetCOMCache
        [XmlIgnore]
        public Dictionary<int, SQL_AssetCommodityContract> AssetCOMCache
        {
            get { return m_TradeValueCOM.AssetCache; }
        }

        /// <summary>
        /// Valeurs des trades COM sur lesquelles porte un calcul de déposit
        /// </summary>
        public RiskDataTradeValue TradeValueCOM
        {
            get { return m_TradeValueCOM; }
        }

        /// <summary>
        /// Trades du jour pour lesquels ne pas calculer de déposit
        /// </summary>
        // PM 20221212 [XXXXX] New
        public RiskDataNoMarginTrade TradeNoMargin
        {
            get { return m_NoMarginTrade; }
        }
        #endregion Accessors

        #region Constructor
        /// <summary>
        /// Constructeur
        /// </summary>
        // PM 20221212 [XXXXX] Ajout gestion RiskDataNoMarginTrade
        public RiskData()
        {
            m_PositionETD = new RiskDataPosition();
            m_TradeValueCOM = new RiskDataTradeValue();       
            m_NoMarginTrade = new RiskDataNoMarginTrade();
        }

        /// <summary>
        /// Constructeur
        /// </summary>
        /// <param name="pPosition"></param>
        /// <param name="pTradeValue"></param>
        /// <param name="pNoMarginTrade"></param>
        // PM 20221212 [XXXXX] Ajout gestion RiskDataNoMarginTrade
        public RiskData(RiskDataPosition pPosition, RiskDataTradeValue pTradeValue, RiskDataNoMarginTrade pNoMarginTrade)
        {
            m_PositionETD = ((pPosition == default(RiskDataPosition)) ? new RiskDataPosition() : pPosition);
            m_TradeValueCOM = ((pTradeValue == default(RiskDataTradeValue)) ? new RiskDataTradeValue() : pTradeValue);
            m_NoMarginTrade = ((pNoMarginTrade == default(RiskDataNoMarginTrade)) ? new RiskDataNoMarginTrade() : pNoMarginTrade);
        }
        #endregion Constructor

        #region Methods
        /// <summary>
        /// Ajout d'un element dans la position
        /// </summary>
        /// <param name="pKey"></param>
        /// <param name="pPosition"></param>
        /// <returns>True si l'élément a été ajouté</returns>
        public bool AddPosition(PosRiskMarginKey pKey, RiskMarginPosition pPosition)
        {
            if (m_PositionETD == default(RiskDataPosition))
            {
                m_PositionETD = new RiskDataPosition();
            }
            return (m_PositionETD.AddPosition(pKey, pPosition));
        }

        /// <summary>
        /// Effacer les données
        /// </summary>
        public void Clear()
        {
            if (m_PositionETD != default(RiskDataPosition))
            {
                m_PositionETD.Clear();
            }
        }

        /// <summary>
        /// Supprimer les données propres à la session
        /// </summary>
        /// <param name="pCS">Connection string</param>
        /// <param name="pSessionId">Session id de l'instance courante</param>
        public void ClearSessionData(string pCS)
        {
            if (m_PositionETD != default(RiskDataPosition))
            {
                m_PositionETD.ClearSessionData(pCS);
            }
        }

        /// <summary>
        /// Indique la présence de données (position ou valeur de trades)
        /// </summary>
        /// <returns></returns>
        // PM 20221212 [XXXXX] Ajout gestion RiskDataNoMarginTrade
        public bool ContainData()
        {
            return (((m_PositionETD != default(RiskDataPosition)) && (m_PositionETD.Count() > 0))
                || ((m_TradeValueCOM != default(RiskDataTradeValue)) && (m_TradeValueCOM.Count() > 0))
                || ((m_NoMarginTrade != default(RiskDataNoMarginTrade)) && (m_NoMarginTrade.Count() > 0)));
        }

        /// <summary>
        /// Obtient le nombre de données d'évaluation de risque
        /// </summary>
        /// <returns></returns>
        // PM 20221212 [XXXXX] Ajout gestion RiskDataNoMarginTrade
        public int Count()
        {
            int count = 0;
            if (m_PositionETD != default(RiskDataPosition))
            {
                count += m_PositionETD.Count();
            }
            if (m_TradeValueCOM != default(RiskDataTradeValue))
            {
                count += m_TradeValueCOM.Count();
            }
            if (m_NoMarginTrade != default(RiskDataNoMarginTrade))
            {
                count += m_NoMarginTrade.Count();
            }
            return count;
        }

        /// <summary>
        /// Obtient les IdA des différents clearer
        /// </summary>
        /// <returns></returns>
        // PM 20221212 [XXXXX] Ajout gestion RiskDataNoMarginTrade
        public int[] GetClearersIdA()
        {
            int[] clearerIdA = new int[0];
            if (m_PositionETD != default(RiskDataPosition))
            {
                clearerIdA = m_PositionETD.GetClearersIdA();
            }
            if (m_TradeValueCOM != default(RiskDataTradeValue))
            {
                clearerIdA = clearerIdA.Concat(m_TradeValueCOM.GetClearersIdA()).ToArray();
            }
            if (m_NoMarginTrade != default(RiskDataNoMarginTrade))
            {
                clearerIdA = clearerIdA.Concat(m_NoMarginTrade.GetClearersIdA()).ToArray();
            }
            return clearerIdA;
        }

        /// <summary>
        /// Obtient les IdA des différents dealer
        /// </summary>
        /// <returns></returns>
        // PM 20221212 [XXXXX] Ajout gestion RiskDataNoMarginTrade
        public int[] GetDealersIdA()
        {
            int[] dealerIdA = new int[0];
            if (m_PositionETD != default(RiskDataPosition))
            {
                dealerIdA = m_PositionETD.GetDealersIdA();
            }
            if (m_TradeValueCOM != default(RiskDataTradeValue))
            {
                dealerIdA = dealerIdA.Concat(m_TradeValueCOM.GetDealersIdA()).ToArray();
            }
            if (m_NoMarginTrade != default(RiskDataNoMarginTrade))
            {
                dealerIdA = dealerIdA.Concat(m_NoMarginTrade.GetDealersIdA()).ToArray();
            }
            return dealerIdA;
        }

        /// <summary>
        /// Obtient la position dans une collection de Pair
        /// </summary>
        /// <remarks>Pour un fonctionnement compatible avec l'ancienne version de stockage des positions</remarks>
        /// <returns>Une collection de Pair</returns>
        public IEnumerable<Pair<PosRiskMarginKey, RiskMarginPosition>> GetPositionAsEnumerablePair()
        {
            IEnumerable<Pair<PosRiskMarginKey, RiskMarginPosition>> position;
            if (m_PositionETD != default(RiskDataPosition))
            {
                position = m_PositionETD.GetPositionAsEnumerablePair();
            }
            else
            {
                position = new List<Pair<PosRiskMarginKey, RiskMarginPosition>>();
            }
            return position;
        }

        /// <summary>
        /// Obtient le premier couple Acteur/Book dealer qu'il soit en position en dans des trades
        /// </summary>
        /// <returns></returns>
        // PM 20221212 [XXXXX] Ajout gestion RiskDataNoMarginTrade
        public RiskActorBook GetFirstDealerActorBook()
        {
            RiskActorBook first = default;
            if ((m_PositionETD != default(RiskDataPosition)) && (m_PositionETD.Count() > 0))
            {
                first = m_PositionETD.GetFirstDealerActorBook();
            }
            else if ((m_TradeValueCOM != default(RiskDataTradeValue)) && (m_TradeValueCOM.Count() > 0))
            {
                first = m_TradeValueCOM.GetFirstDealerActorBook();
            }
            else if ((m_NoMarginTrade != default(RiskDataNoMarginTrade)) && (m_NoMarginTrade.Count() > 0))
            {
                first = m_NoMarginTrade.GetFirstDealerActorBook();
            }
            return first;
        }

        /// <summary>
        /// Obtient le premier couple Acteur/Book clearer qu'il soit en position en dans des trades
        /// </summary>
        /// <returns></returns>
        // PM 20221212 [XXXXX] Ajout gestion RiskDataNoMarginTrade
        public RiskActorBook GetFirstClearerActorBook()
        {
            RiskActorBook first = default;
            if ((m_PositionETD != default(RiskDataPosition)) && (m_PositionETD.Count() > 0))
            {
                first = m_PositionETD.GetFirstClearerActorBook();
            }
            else if ((m_TradeValueCOM != default(RiskDataTradeValue)) && (m_TradeValueCOM.Count() > 0))
            {
                first = m_TradeValueCOM.GetFirstClearerActorBook();
            }
            else if ((m_NoMarginTrade != default(RiskDataNoMarginTrade)) && (m_NoMarginTrade.Count() > 0))
            {
                first = m_NoMarginTrade.GetFirstClearerActorBook();
            }
            return first;
        }

        /// <summary>
        /// Prendre les données de risque d'une liste de books
        /// </summary>
        /// <param name="pBooksId">liste de books Id</param>
        /// <returns>Les données de risque d'une liste de books</returns>
        /// <remarks>Les données de risque sont des références des originaux</remarks>
        // PM 20221212 [XXXXX] Ajout gestion RiskDataNoMarginTrade
        public RiskData GetBooksRiskData(IEnumerable<int> pBooksId)
        {
            RiskData riskData = new RiskData();
            if (m_PositionETD != default(RiskDataPosition))
            {
                riskData.m_PositionETD = m_PositionETD.GetBooksRiskData(pBooksId);
            }
            if (m_TradeValueCOM != default(RiskDataTradeValue))
            {
                riskData.m_TradeValueCOM = m_TradeValueCOM.GetBooksRiskData(pBooksId);
            }
            if (m_NoMarginTrade != default(RiskDataNoMarginTrade))
            {
                riskData.m_NoMarginTrade = m_NoMarginTrade.GetBooksRiskData(pBooksId);
            }
            return riskData;
        }

        /// <summary>
        /// Construit les données de risque du clearer liés à l'ensemble des books données (pBooksId), 
        /// à partir des données de risque initiaux (données des dealers) 
        /// </summary>
        /// <param name="pBooksId">L'ensemble des books Id du CLEARER/MRO</param>
        /// <param name="pBooksOwnerId">L'Id de l'actor CLEARER/MRO propriétaire de l'ensemble des books. Cet Id sera utilisé comme propriétaire des données de risque retournées.</param>
        /// <returns>Toutes les données de risque (nettées) dont l'Id du book de clearer appartient à l'ensemble des Ids de book de clearer en paramètre. 
        /// Les Ids des actor/book dealers initiaux seront écrasés par les Id passés en argument.</returns>
        /// <remarks>Les données de risque sont des copies des originaux</remarks>
        // PM 20221212 [XXXXX] Ajout gestion RiskDataNoMarginTrade
        public RiskData BuildClearerRiskData(IEnumerable<int> pBooksId, int pBooksOwnerId)
        {
            RiskData riskData = new RiskData();
            if (m_PositionETD != default(RiskDataPosition))
            {
                riskData.m_PositionETD = m_PositionETD.BuildClearerRiskData(pBooksId, pBooksOwnerId);
            }
            if (m_TradeValueCOM != default(RiskDataTradeValue))
            {
                riskData.m_TradeValueCOM = m_TradeValueCOM.BuildClearerRiskData(pBooksId, pBooksOwnerId);
            }
            if (m_NoMarginTrade != default(RiskDataNoMarginTrade))
            {
                riskData.m_NoMarginTrade = m_NoMarginTrade.BuildClearerRiskData(pBooksId, pBooksOwnerId);
            }
            return riskData;
        }

        /// <summary>
        /// Prendre la position et les trades value filtrées sur un ensemble d'asset
        /// </summary>
        /// <param name="pAssetCache"></param>
        /// <returns></returns>
        // PM 20200910 [25481] Refactoring total pour gestion à la fois de la position et des trades value
        //public RiskData GetAssetRiskData(IEnumerable<int> pIdAsset)
        // PM 20221212 [XXXXX] Ajout gestion RiskDataNoMarginTrade
        public RiskData GetAssetRiskData(RiskAssetCache pAssetCache)
        {
            RiskDataPosition newPosition = new RiskDataPosition();
            RiskDataTradeValue newTradeValue = new RiskDataTradeValue();
            RiskDataNoMarginTrade newNoMarginTrade = new RiskDataNoMarginTrade();

            if (pAssetCache != default(RiskAssetCache))
            {
                if ((m_PositionETD != default(RiskDataPosition)) && (pAssetCache.AssetETDCache.Count > 0))
                {
                    newPosition = m_PositionETD.GetAssetRiskData(pAssetCache.AssetETDCache);
                }
                else if ((m_TradeValueCOM != default(RiskDataTradeValue)) && (pAssetCache.AssetCOMCache.Count > 0))
                {
                    newTradeValue = m_TradeValueCOM.GetAssetRiskData(pAssetCache.AssetCOMCache);
                }
                if ((m_NoMarginTrade != default(RiskDataNoMarginTrade)) && (pAssetCache.AssetCOMCache.Count > 0))
                {
                    newNoMarginTrade = m_NoMarginTrade.GetAssetRiskData(pAssetCache.AssetETDCache, pAssetCache.AssetCOMCache);
                }
            }
            RiskData newRiskData = new RiskData(newPosition, newTradeValue, newNoMarginTrade);

            return newRiskData;
        }

        /// <summary>
        /// Position la position Exe/Ass/Mof à zéro
        /// </summary>
        public void ClearPosExeAss()
        {
            if (m_PositionETD != default(RiskDataPosition))
            {
                m_PositionETD.ClearPosExeAss();
            }
        }

        /// <summary>
        /// Regroupe une collection de données de risque
        /// </summary>
        /// <param name="pRiskData">Un ensemble de données de risque</param>
        /// <returns>Un nouvel objet représentant l'ensemble des donneés de risque</returns>
        // PM 20221212 [XXXXX] Ajout gestion RiskDataNoMarginTrade 
        public static RiskData Aggregate(IEnumerable<RiskData> pRiskData)
        {
            RiskData agg;
            if (pRiskData != default(IEnumerable<RiskData>))
            {
                IEnumerable<RiskDataPosition> riskDataPos =
                    from data in pRiskData
                    select data.m_PositionETD;

                IEnumerable<RiskDataTradeValue> riskDataTrd =
                    from data in pRiskData
                    select data.m_TradeValueCOM;

                IEnumerable<RiskDataNoMarginTrade> riskDataNoMrg =
                    from data in pRiskData
                    select data.m_NoMarginTrade;

                RiskDataPosition aggPos = RiskDataPosition.Aggregate(riskDataPos);
                RiskDataTradeValue aggTrd = RiskDataTradeValue.Aggregate(riskDataTrd);
                RiskDataNoMarginTrade aggTrdNoMrg = RiskDataNoMarginTrade.Aggregate(riskDataNoMrg);

                agg = new RiskData(aggPos, aggTrd, aggTrdNoMrg);
            }
            else
            {
                agg = new RiskData();
            }
            return agg;
        }

        /// <summary>
        /// Regroupe les données de risque par actor/book
        /// </summary>
        /// <param name="pRiskData">Un ensemble de données de risque</param>
        /// <returns>Un dictionaire ayant un couple d'Ids actor/book comme clé et les données de risque comme value</returns>
        /// PM 20170313 [22833] Inspiré de RoleMarginReqOfficeAttribute.AggregatePositionsByActorBook(...)
        // PM 20221212 [XXXXX] Ajout gestion RiskDataNoMarginTrade 
        public static Dictionary<RiskActorBook, RiskData> AggregateByActorBook(IEnumerable<RiskData> pRiskData)
        {
            Dictionary<RiskActorBook, RiskData> aggregateRiskData = new Dictionary<RiskActorBook, RiskData>(new RiskActorBookComparer());
            if (pRiskData != default(IEnumerable<RiskData>))
            {
                IEnumerable<RiskDataPosition> riskDataETD = pRiskData.Select(r => r.m_PositionETD);
                IEnumerable<RiskDataTradeValue> riskDataCOM = pRiskData.Select(r => r.m_TradeValueCOM);
                IEnumerable<RiskDataNoMarginTrade> riskDataNoMrg = pRiskData.Select(r => r.m_NoMarginTrade);

                Dictionary<RiskActorBook, RiskDataPosition> aggregateRiskDataETD = RiskDataPosition.AggregateByActorBook(riskDataETD);
                Dictionary<RiskActorBook, RiskDataTradeValue> aggregateRiskDataCOM = RiskDataTradeValue.AggregateByActorBook(riskDataCOM);
                Dictionary<RiskActorBook, RiskDataNoMarginTrade> aggregateRiskDataNoMrg = RiskDataNoMarginTrade.AggregateByActorBook(riskDataNoMrg);

                // Prendre l'ensemble des différentes clés : Union
                RiskActorBookComparer actorBookComparer = new RiskActorBookComparer();
                IEnumerable<RiskActorBook> keys = aggregateRiskDataETD.Keys.Union(aggregateRiskDataCOM.Keys, actorBookComparer);
                keys = keys.Union(aggregateRiskDataNoMrg.Keys, actorBookComparer);

                foreach (RiskActorBook key in keys)
                {

                    if (false == aggregateRiskDataETD.TryGetValue(key, out RiskDataPosition aggETDValue))
                    {
                        aggETDValue = new RiskDataPosition();
                    }
                    if (false == aggregateRiskDataCOM.TryGetValue(key, out RiskDataTradeValue aggCOMValue))
                    {
                        aggCOMValue = new RiskDataTradeValue();
                    }
                    if (false == aggregateRiskDataNoMrg.TryGetValue(key, out RiskDataNoMarginTrade aggNoMrg))
                    {
                        aggNoMrg = new RiskDataNoMarginTrade();
                    }
                    aggregateRiskData.Add(key, new RiskData(aggETDValue, aggCOMValue, aggNoMrg));
                }
            }
            return aggregateRiskData;
        }

        /// <summary>
        /// Obtient uniquement la position sous la forme d'une collection d'objet Pair
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Pair<PosRiskMarginKey, RiskMarginPosition>> GetPosition()
        {
            IEnumerable<Pair<PosRiskMarginKey, RiskMarginPosition>> position;
            if (m_PositionETD != default(RiskDataPosition))
            {
                position = m_PositionETD.GetPositionAsEnumerablePair();
            }
            else
            {
                position = new List<Pair<PosRiskMarginKey, RiskMarginPosition>>();
            }
            return position;
        }
        #endregion Methods
    }

    /// <summary>
    /// Class représentant la valeur total d'un groupe de trades
    /// </summary>
    public class RiskMarginTradeValue
    {
        #region Members
        /// <summary>
        /// Collection des Id des trades 
        /// </summary>
        [XmlArray(ElementName = "trades")]
        public int[] TradeIds;

        /// <summary>
        /// Valeur total des trades
        /// </summary>
        public List<IMoney> Amount;

        /// <summary>
        /// Ensemble des trades
        /// </summary>
        public IEnumerable<TradeValue> Trades;
        #endregion Members

        #region Constructor
        /// <summary>
        /// Constructeur
        /// </summary>
        public RiskMarginTradeValue() { }

        /// <summary>
        /// Constructeur
        /// </summary>
        /// <param name="pIdTs"></param>
        /// <param name="pAmount"></param>
        /// PM 20170808 [23371] Modification de Value (decimal) en Amount ((Ensemble de IMoney)) pour tenir compte de la devise
        //public RiskMarginTradeValue(int[] pIdTs, decimal pValue)
        public RiskMarginTradeValue(int[] pIdTs, IEnumerable<IMoney> pAmount)
        {
            TradeIds = pIdTs;
            //Value = pValue;
            Amount = (pAmount != default(IEnumerable<IMoney>) ? pAmount.ToList() : new List<IMoney>());
        }

        /// <summary>
        /// Constructeur
        /// </summary>
        /// <param name="pIdTs"></param>
        /// <param name="pAmount"></param>
        // PM 20170808 [23371] Ajout
        public RiskMarginTradeValue(int[] pIdTs, IEnumerable<IMoney> pAmount, IEnumerable<TradeValue> pTrades)
        {
            TradeIds = pIdTs;
            Amount = (pAmount != default(IEnumerable<IMoney>) ? pAmount.ToList() : new List<IMoney>());
            Trades = pTrades;
        }
        #endregion Constructor
    }
    /// <summary>
    /// Classe représentant les données basées sur les trades sur lesquelles porte un calcul de déposit
    /// </summary>
    /// PM 20170313 [22833] New
    public abstract class RiskDataTradePos
    {
        #region Methods
        /// <summary>
        /// Vider les données
        /// </summary>
        public virtual void Clear() { }

        /// <summary>
        /// Obtient le nombre de clés de données présentes
        /// </summary>
        /// <returns></returns>
        public virtual int Count()
        {
            return 0;
        }

        /// <summary>
        /// Obtient les IdA des différents clearer
        /// </summary>
        /// <returns></returns>
        public virtual int[] GetClearersIdA()
        {
            return new int[0];
        }

        /// <summary>
        /// Obtient les IdA des différents dealer
        /// </summary>
        /// <returns></returns>
        public virtual int[] GetDealersIdA()
        {
            return new int[0];
        }

        /// <summary>
        /// Obtient l'ensemble des couples actor/book dealer
        /// </summary>
        /// <returns></returns>
        public virtual IEnumerable<RiskActorBook> GetDealersActorBook()
        {
            return new List<RiskActorBook>();
        }

        /// <summary>
        /// Obtient le premier couple Acteur/Book dealer
        /// </summary>
        /// <returns>null si aucun élément</returns>
        public virtual RiskActorBook GetFirstDealerActorBook()
        {
            return default;
        }

        /// <summary>
        /// Obtient le premier couple Acteur/Book clearer
        /// </summary>
        /// <returns>null si aucun élément</returns>
        public virtual RiskActorBook GetFirstClearerActorBook()
        {
            return default;
        }
        #endregion Methods
    }


    /// <summary>
    /// Classe de base représentant la période (Date/Time de début et Date/Time de fin) pour la selection des trades à inclure dans le calcul d'une Exposure à une date donnée
    /// </summary>
    /// PM 20170808 [23371] New
    public abstract class RiskDataExposureDates
    {
        #region Members
        /// <summary>
        /// Date de référence de la période d'Exposure
        /// </summary>
        protected DateTime m_DtAffect;
        /// <summary>
        /// Exposure Date/Time de début
        /// </summary>
        protected DateTime m_ExposureStartDateTime;
        /// <summary>
        /// Exposure Date/Time de fin
        /// </summary>
        protected DateTime m_ExposureEndDateTime;
        /// <summary>
        /// Heure début de la période
        /// </summary>
        protected TimeSpan m_FromTime;
        /// <summary>
        /// Heure Fin de la période
        /// </summary>
        protected TimeSpan m_ToTime;
        /// <summary>
        /// Nombre de jours à retirer pour trouver la date de début de période
        /// </summary>
        protected int m_DecStartDt;
        /// <summary>
        /// Nombre de jours à ajouter pour trouver la date de fin de période
        /// </summary>
        protected int m_IncEndDt;
        #endregion Members

        #region Accessors
        /// <summary>
        /// Exposure Date/Time de début
        /// </summary>
        public DateTime ExposureStartDateTime
        { get { return m_ExposureStartDateTime; } }
        /// <summary>
        /// Exposure Date/Time de fin
        /// </summary>
        public DateTime ExposureEndDateTime
        { get { return m_ExposureEndDateTime; } }
        #endregion Accessors

        #region Methods
        /// <summary>
        /// Calcul des dates/heures de début et de fin de période de l'Exposure
        /// </summary>
        protected void ComputeDates()
        {
            // Exposure Date/Time de début
            m_ExposureStartDateTime = m_DtAffect.AddDays(m_DecStartDt).Add(m_FromTime);
            // Exposure Date/Time de fin
            m_ExposureEndDateTime = m_DtAffect.AddDays(m_IncEndDt).Add(m_ToTime);
        }
        #endregion Methods
    }

    /// <summary>
    /// Classe représentant la période (Date/Time de début et Date/Time de fin) pour la selection des trades à inclure dans le calcul de la Current Exposure à une date donnée
    /// </summary>
    /// PM 20170808 [23371] New
    public class RiskDataCurrentExposureDates : RiskDataExposureDates
    {
        #region Constructor
        /// <summary>
        /// Constructeur
        /// </summary>
        public RiskDataCurrentExposureDates(DateTime pDtAffect)
        {
            // TODO rendre les deux valeurs ci-dessous (m_FromTime et m_ToTime) dynamique
            // La plage horaire pour le calcul de la Current Exposure: Jour même 16h00 --> Jour même 22h00
            //-----------------------------------------------------------------------------------
            // Heure de début (Cut-off)
            m_FromTime = new TimeSpan(16, 0, 0);
            // Heure de fin (Suspension)
            m_ToTime = new TimeSpan(22, 00, 0);
            //
            m_DtAffect = pDtAffect;
            //
            switch (m_DtAffect.DayOfWeek)
            {
                case DayOfWeek.Monday:
                    // Pour le lundi prendre le vendredi
                    m_DecStartDt = -3;
                    m_IncEndDt = -3;
                    break;
                default:
                    // Pour les autres jours de la semaine, prendre la veille
                    m_DecStartDt = -1;
                    m_IncEndDt = -1;
                    break;
            }
            base.ComputeDates();
        }
        #endregion Constructor
    }

    /// <summary>
    /// Classe représentant la période (Date/Time de début et Date/Time de fin) pour la selection des trades à inclure dans le calcul de la T0Exposure à une date donnée
    /// </summary>
    /// PM 20170808 [23371] New
    public class RiskDataT0ExposureDates : RiskDataExposureDates
    {
        #region Constructor
        /// <summary>
        /// Constructeur
        /// </summary>
        public RiskDataT0ExposureDates(DateTime pDtAffect)
        {
            // TODO rendre les deux valeurs ci-dessous (m_FromTime et m_ToTime) dynamique
            // La plage horaire pour le calcul des T0Exposure: Veille 16h00 --> Jour même 14h00
            //-----------------------------------------------------------------------------------
            // Heure de début (Cut-off)
            m_FromTime = new TimeSpan(16, 0, 0);
            // Heure de fin (Suspension)
            m_ToTime = new TimeSpan(14, 00, 0);
            //
            m_DtAffect = pDtAffect;
            //
            switch (m_DtAffect.DayOfWeek)
            {
                case DayOfWeek.Monday:
                    // Pour le lundi prendre du cut-off du vendredi à la suspension du jour (lundi)
                    m_DecStartDt = -3;
                    m_IncEndDt = 0;
                    break;
                case DayOfWeek.Friday:
                    // Pour le vendredi prendre du cut-off du jeudi à la suspension du jour (vendredi)
                    m_DecStartDt = -1;
                    m_IncEndDt = 0;
                    break;
                default:
                    // Pour les autres jours de la semaine, prendre du cut-off de la veille à la suspension du jour
                    m_DecStartDt = -1;
                    m_IncEndDt = 0;
                    break;
            }
            base.ComputeDates();
        }
        #endregion Constructor
    }

    /// <summary>
    /// Classe représentant la période (Date/Time de début et Date/Time de fin) pour la selection des trades à inclure dans le calcul de la Exposure Normal à une date donnée
    /// </summary>
    /// PM 20170808 [23371] New
    public class RiskDataNormalExposureDates : RiskDataExposureDates
    {
        #region Constructor
        /// <summary>
        /// Constructeur
        /// </summary>
        public RiskDataNormalExposureDates(DateTime pDtAffect)
        {
            // TODO rendre les deux valeurs ci-dessous (m_FromTime et m_ToTime) dynamique
            // La plage horaire pour le calcul des Exposure: Veille 16h00 --> Lendemain 12h00
            //---------------------------------------------------------------------------------
            // Heure de début (Cut-off)
            m_FromTime = new TimeSpan(16, 0, 0);
            // Heure de fin (Suspension)
            m_ToTime = new TimeSpan(12, 00, 0);
            //
            m_DtAffect = pDtAffect;
            //
            switch (m_DtAffect.DayOfWeek)
            {
                case DayOfWeek.Monday:
                    // Pour le lundi prendre du cut-off du vendredi à la suspension du mardi
                    m_DecStartDt = -3;
                    m_IncEndDt = 1;
                    break;
                case DayOfWeek.Friday:
                    // Pour le vendredi prendre du cut-off du jeudi à la suspension du lundi
                    m_DecStartDt = -1;
                    m_IncEndDt = 3;
                    break;
                default:
                    // Pour les autres jours de la semaine, prendre du cut-off de la veille à la suspension du lendemain
                    m_DecStartDt = -1;
                    m_IncEndDt = 1;
                    break;
            }
            base.ComputeDates();
        }
        #endregion Constructor
    }

    /// <summary>
    /// Classe représentant une position sur laquelle porte un calcul de déposit
    /// </summary>
    /// PM 20170313 [22833] New
    public class RiskDataPosition : RiskDataTradePos
    {
        #region Members
        /// <summary>
        /// Trades en position regroupé par clé de position
        /// </summary>
        private Dictionary<PosRiskMarginKey, RiskMarginPosition> m_Position;

        /// <summary>
        /// Asset ETD sql elements cache
        /// key = IdAsset, value = SQL_AssetETD
        /// </summary>
        private Dictionary<int, SQL_AssetETD> m_AssetETDCache = new Dictionary<int, SQL_AssetETD>();
        #endregion Members

        #region Accessors
        /// <summary>
        /// Asset ETD sql elements cache
        /// key = IdAsset, value = SQL_AssetETD
        /// </summary>
        internal Dictionary<int, SQL_AssetETD> AssetETDCache
        {
            get { return m_AssetETDCache; }
        }
        #endregion Accessors

        #region Constructor
        /// <summary>
        /// Constructeur
        /// </summary>
        public RiskDataPosition()
        {
            m_Position = new Dictionary<PosRiskMarginKey, RiskMarginPosition>();
        }

        /// <summary>
        /// Constructeur
        /// </summary>
        /// <param name="pPosition"></param>
        public RiskDataPosition(IEnumerable<KeyValuePair<PosRiskMarginKey, RiskMarginPosition>> pPosition)
        {
            if (pPosition != default(IEnumerable<KeyValuePair<PosRiskMarginKey, RiskMarginPosition>>))
            {
                m_Position = pPosition.ToDictionary(k => k.Key, v => v.Value);
            }
            else
            {
                m_Position = new Dictionary<PosRiskMarginKey, RiskMarginPosition>();
            }
        }
        #endregion Constructor

        #region Methods
        /// <summary>
        /// Construction de la position (nettée par asset, side dealer et clearer) à partir d'un ensemble de trades
        /// </summary>
        /// <param name="pIdEntity">Actor entity internal id, entité de laquelle descant l'ensemble de la position</param>
        /// <param name="pTrades">Ensemble de trades formant la position, les trades sont regroupés par asset, side dealer et clearer</param>
        public void BuildPosition(int pIdEntity, IEnumerable<TradeAllocation> pTrades)
        {
            if (pTrades != default(IEnumerable<TradeAllocation>))
            {
                m_Position =
                    // Regrouper les trades par Asset/Side/Actor/Book
                    (from trade in
                         pTrades.GroupBy(
                             key => new
                             {
                                 key.IdI,
                                 key.AssetId,
                                 key.Side,
                                 key.BookId,
                                 key.ActorId,
                                 key.ClearerId,
                                 key.BookClearerId
                             })
                     select trade).
                    // Transformer les collection de groupe de trades en un dictionaire de positions
                    ToDictionary(
                        // Definir la key de position
                        trades => new PosRiskMarginKey
                        {
                            idI = trades.Key.IdI,
                            idAsset = trades.Key.AssetId,
                            underlyingAsset = Cst.UnderlyingAsset.ExchangeTradedContract,
                            Side = trades.Key.Side,
                            idA_Dealer = trades.Key.ActorId,
                            idB_Dealer = trades.Key.BookId,
                            idA_Clearer = trades.Key.ClearerId,
                            idB_Clearer = trades.Key.BookClearerId,
                            idA_EntityDealer = pIdEntity,
                            idA_EntityClearer = pIdEntity
                        },
                        // Definir la position
                        trades => new RiskMarginPosition
                        {
                        // PM 20130905 [17949] Livraison : Ajout du distinct car le trade peut être présent 2 fois (1 pour ExeAss et 1 autre pour Delivery)
                        TradeIds = (from allocation in trades select allocation.TradeId).Distinct().ToArray(),
                        // EG 20150920 [21374] Int (int32) to Long (Int64) 
                        // EG 20170127 Qty Long To Decimal
                        Quantity = (decimal)(from allocation in trades select allocation.Quantity).Sum(),
                            ExeAssQuantity = (decimal)(from allocation in trades select allocation.ExeAssQuantity).Sum(),
                        // UNDONE MF Attention, on fait le netting des dénouements , et on prend la date la majeure de livraison, 
                        //  c'est faux, à corriger
                        DeliveryDate = (from allocation in trades select allocation.DeliveryDate).Max(),
                            SettlementDate = (from allocation in trades select allocation.SettlementDate).Max(),
                        // PM 20130905 [17949] Livraison (Attention position en livraison signée)
                        DeliveryQuantity = (from allocation in trades select allocation.DeliveryQuantity).Sum() * (trades.Key.Side == "2" ? -1 : 1),
                            DeliveryStep = (from allocation in trades where allocation.DeliveryQuantity != 0 select allocation.DeliveryStep).FirstOrDefault(),
                            DeliveryStepDate = (from allocation in trades where allocation.DeliveryQuantity != 0 select allocation.DeliveryStepDate).FirstOrDefault(),
                        // PM 20190401 [24625][24387] Ajout DeliveryExpressionType
                        DeliveryExpressionType = (from allocation in trades where allocation.DeliveryQuantity != 0 select allocation.DeliveryExpressionType).FirstOrDefault(),
                        },
                        // Definir le comparer pour la position key
                        new PosRiskMarginKeyComparer()
                    );
            }
            else
            {
                m_Position = new Dictionary<PosRiskMarginKey, RiskMarginPosition>();
            }
        }

        /// <summary>
        /// Ajout d'un element dans la position
        /// </summary>
        /// <param name="pKey"></param>
        /// <param name="pPosition"></param>
        /// <returns>True si l'élément a été ajouté</returns>
        public bool AddPosition(PosRiskMarginKey pKey, RiskMarginPosition pPosition)
        {
            bool added = (false == m_Position.ContainsKey(pKey));
            if (added)
            {
                m_Position.Add(pKey, pPosition);
            }
            return added;
        }

        /// <summary>
        /// Mise en cache dans AssetETDCache de tous les Asset ETD en position
        /// </summary>
        /// <param name="pCS"></param>
        public void BuildAssetETDCache(string pCS)
        {
            foreach (KeyValuePair<PosRiskMarginKey, RiskMarginPosition> keyValuePair in m_Position)
            {
                int idAsset = keyValuePair.Key.idAsset;

                if (!AssetETDCache.ContainsKey(idAsset))
                {
                    SQL_AssetETD asset = new SQL_AssetETD(pCS, idAsset);
                    m_AssetETDCache.Add(idAsset, asset);
                }
            }
        }

        /// <summary>
        /// Affectation de l'ensemble des SQL_AssetETD
        /// </summary>
        /// <param name="pAssetETDCache">Dictionnaire: key = idasset / value = SQL_AssetETD</param>
        // PM 20180918 [XXXXX] Ajout méthode suite test Prisma Eurosys
        public void SetAssetETDCacheExternal(Dictionary<int, SQL_AssetETD> pAssetETDCache)
        {
            if (pAssetETDCache != default(Dictionary<int, SQL_AssetETD>))
            {
                m_AssetETDCache = pAssetETDCache;
            }
        }

        /// <summary>
        /// Insert all the id asset in position or in delivery inside the IMASSET_ETD table
        /// </summary>
        /// <param name="pCS">Connection string</param>
        /// <param name="pTableId"></param>
        // EG 20180803 PERF Suppresion SESSIONID non utilisée avec IMASSET_ETD_{BuildTableId}_W
        // EG 20181119 PERF Correction post RC (Step 2)
        public void InsertImAssetEtd(string pCS, string pTableId)
        {
            if (m_AssetETDCache != null)
            {
                string queryInsert = DataContractHelper.GetQuery(DataContractResultSets.INSERTIMASSETETD);

                CommandType queryType = DataContractHelper.GetType(DataContractResultSets.INSERTIMASSETETD);

                Dictionary<string, object> dbParameterValues = new Dictionary<string, object>();

                using (IDbConnection connection = DataHelper.OpenConnection(pCS))
                {
                    foreach (KeyValuePair<int, SQL_AssetETD> keyValuePair in m_AssetETDCache)
                    {
                        int idAsset = keyValuePair.Key;
                        int idDC = keyValuePair.Value.IdDerivativeContract;
                        dbParameterValues.Add("IDASSET", idAsset);
                        dbParameterValues.Add("IDDC", idDC);

                        DataHelper.ExecuteNonQuery(connection, queryType, queryInsert, DataContractHelper.GetDbDataParameters(DataContractResultSets.INSERTIMASSETETD, dbParameterValues));

                        dbParameterValues.Remove("IDASSET");
                        dbParameterValues.Remove("IDDC");
                    }
                }

                if (DataHelper.GetSvrInfoConnection(pCS).IsOracle)
                    DataHelper.UpdateStatTable(pCS, String.Format("IMASSET_ETD_{0}_W", pTableId).ToUpper());

            }
        }

        /// <summary>
        /// Delete tous les id asset relatif à la session courante
        /// </summary>
        /// <param name="pCS">Connection string</param>
        // EG 20180803 PERF New
        private void TruncateImAssetEtd(string pCS)
        {
            string queryTruncate = DataContractHelper.GetQuery(DataContractResultSets.TRUNCATEIMASSETETD);
            if (false == String.IsNullOrEmpty(queryTruncate))
            {
                CommandType queryType = DataContractHelper.GetType(DataContractResultSets.TRUNCATEIMASSETETD);

                using (IDbConnection connection = DataHelper.OpenConnection(pCS))
                {
                    DataHelper.ExecuteNonQuery(connection, queryType, queryTruncate, null);
                }
            }
        }

        /// <summary>
        /// Vider la position
        /// </summary>
        public override void Clear()
        {
            m_Position.Clear();
            // Ne pas effacer asset cache, cette collection peut persiter jusqu'à ce qu'elle soit collectée par le GC
        }

        /// <summary>
        /// Supprimer les données propres à la session
        /// </summary>
        /// <param name="pCS">Connection string</param>
        /// <param name="pSessionId">Session id de l'instance courante</param>
        // EG 20180803 PERF Suppresion SESSIONID non utilisée avec IMASSET_ETD_{BuildTableId}_W
        public void ClearSessionData(string pCS)
        {
            TruncateImAssetEtd(pCS);
        }

        /// <summary>
        /// Copie les valeurs des membres de la classe dans celle passée en paramètre
        /// </summary>
        /// <param name="pDest">Classe de destination dans laquelle sont copié les valeurs des membres</param>
        /// <returns>True si les données ont été copiées</returns>
        public bool CopyMembersValue(RiskDataPosition pDest)
        {
            bool ret = pDest != default(RiskDataPosition);
            if (ret)
            {
                pDest.m_Position = m_Position;
                pDest.m_AssetETDCache = m_AssetETDCache;
            }
            return ret;
        }

        /// <summary>
        /// Obtient le nombre de clés de position
        /// </summary>
        /// <returns></returns>
        public override int Count()
        {
            return m_Position.Count();
        }

        /// <summary>
        /// Obtient les différents IdAsset de la position
        /// </summary>
        /// <returns></returns>
        public IEnumerable<int> GetAssetIdInPos()
        {
            IEnumerable<int> assetIds = (
                from pos in m_Position
                select pos.Key.idAsset).Distinct();

            return assetIds;
        }

        /// <summary>
        /// Obtient la position dans un dictionnaire
        /// </summary>
        public Dictionary<PosRiskMarginKey, RiskMarginPosition> GetPositionAsDictionary()
        {
            return m_Position;
        }

        /// <summary>
        /// Obtient la position dans une collection de Pair
        /// </summary>
        /// <remarks>Pour un fonctionnement compatible avec l'ancienne version de stockage des positions</remarks>
        /// <returns>Une collection de Pair</returns>
        public IEnumerable<Pair<PosRiskMarginKey, RiskMarginPosition>> GetPositionAsEnumerablePair()
        {
            IEnumerable<Pair<PosRiskMarginKey, RiskMarginPosition>> position =
                from pos in m_Position
                select new Pair<PosRiskMarginKey, RiskMarginPosition>(pos.Key, pos.Value);
            return position;
        }

        /// <summary>
        /// Obtient les IdA des différents clearer en position
        /// </summary>
        /// <returns></returns>
        public override int[] GetClearersIdA()
        {
            int[] clearerIdA = m_Position.Keys.Select(k => k.idA_Clearer).Distinct().ToArray();
            return clearerIdA;
        }

        /// <summary>
        /// Obtient les IdA des différents dealer en position
        /// </summary>
        /// <returns></returns>
        public override int[] GetDealersIdA()
        {
            int[] dealerIdA = m_Position.Keys.Select(k => k.idA_Dealer).Distinct().ToArray();
            return dealerIdA;
        }

        /// <summary>
        /// Obtient l'ensemble des couples actor/book dealer
        /// </summary>
        /// <returns></returns>
        public override IEnumerable<RiskActorBook> GetDealersActorBook()
        {
            return m_Position.Keys.Select(k => new RiskActorBook(k.idA_Dealer, k.idB_Dealer)).Distinct(new RiskActorBookComparer());
        }

        /// <summary>
        /// Obtient le premier couple Acteur/Book dealer
        /// </summary>
        /// <returns>null si aucun élément</returns>
        public override RiskActorBook GetFirstDealerActorBook()
        {
            RiskActorBook first = default;
            if (m_Position.Count() > 0)
            {
                PosRiskMarginKey dataKey = m_Position.First().Key;
                first = new RiskActorBook(dataKey.idA_Dealer, dataKey.idB_Dealer);
            }
            return first;
        }

        /// <summary>
        /// Obtient le premier couple Acteur/Book clearer
        /// </summary>
        /// <returns>null si aucun élément</returns>
        public override RiskActorBook GetFirstClearerActorBook()
        {
            RiskActorBook first = default;
            if (m_Position.Count() > 0)
            {
                PosRiskMarginKey dataKey = m_Position.First().Key;
                first = new RiskActorBook(dataKey.idA_Clearer, dataKey.idB_Clearer);
            }
            return first;
        }

        /// <summary>
        /// Prendre la position d'une liste de Books
        /// </summary>
        /// <param name="pBooksId">liste de Books Id</param>
        /// <returns>La position tenue par la liste de Books</returns>
        /// <remarks>Les données de risque sont des références des originaux</remarks>
        public RiskDataPosition GetBooksRiskData(IEnumerable<int> pBooksId)
        {
            RiskDataPosition actorRiskData = new RiskDataPosition();
            CopyMembersValue(actorRiskData);
            if (pBooksId != default(IEnumerable<int>))
            {
                actorRiskData.m_Position = m_Position.Where(p => pBooksId.Contains(p.Key.idB_Dealer)).ToDictionary(p => p.Key, p => p.Value);
            }
            return actorRiskData;
        }

        /// <summary>
        /// Construit les données de risque du clearer liés à l'ensemble des books données (pBooksId), 
        /// à partir des données de risque initiaux (données des dealers) 
        /// </summary>
        /// <param name="pBooksId">L'ensemble des books Id du CLEARER/MRO</param>
        /// <param name="pBooksOwnerId">L'Id de l'actor CLEARER/MRO propriétaire de l'ensemble des books. Cet Id sera utilisé comme propriétaire des données de risque retournées.</param>
        /// <returns>Toutes les données de risque (nettées par book/asset/side) dont l'Id du book de clearer appartient à l'ensemble des Ids de book de clearer en paramètre. 
        /// Les Ids des actor/book dealers initiaux seront écrasés par les Id passés en argument.</returns>
        /// <remarks>Les données de risque sont des copies des originaux</remarks>
        /// PM 20170313 [22833] Inspiré de PositionsExtractor.BuildClearerBookPositionsFromRepository(...)
        public RiskDataPosition BuildClearerRiskData(IEnumerable<int> pBooksId, int pBooksOwnerId)
        {
            RiskDataPosition clearerRiskData = new RiskDataPosition();
            CopyMembersValue(clearerRiskData);

            if (pBooksId != default(IEnumerable<int>))
            {
                clearerRiskData.m_Position = (
                    from positionsGroup in
                        ((from position in m_Position where pBooksId.Contains(position.Key.idB_Clearer) select position)
                            .GroupBy(elem => new
                            {
                                elem.Key.idI,
                                elem.Key.idAsset,
                                elem.Key.Side,
                                elem.Key.idB_Clearer,
                            })
                        )
                    select new KeyValuePair<PosRiskMarginKey, RiskMarginPosition>(
                        new PosRiskMarginKey
                        {
                            idI = positionsGroup.Key.idI,
                            idAsset = positionsGroup.Key.idAsset,
                            Side = positionsGroup.Key.Side,
                            idA_Dealer = pBooksOwnerId,
                            idB_Dealer = positionsGroup.Key.idB_Clearer,
                            idA_Clearer = pBooksOwnerId,
                            idB_Clearer = positionsGroup.Key.idB_Clearer,
                            idA_EntityDealer = 0,
                            idA_EntityClearer = 0
                        },
                        new RiskMarginPosition
                        {
                            TradeIds = (from position in positionsGroup from tradeId in position.Value.TradeIds select tradeId).ToArray(),
                            Quantity = (from position in positionsGroup select position.Value.Quantity).Sum(),
                            // EG 20150920 [21374] Int (int32) to Long (Int64) 
                            // EG 20170127 Qty Long To Decimal
                            ExeAssQuantity = (from position in positionsGroup select position.Value.ExeAssQuantity).Sum(),
                            DeliveryDate = (from position in positionsGroup select position.Value.DeliveryDate).Max(),
                            SettlementDate = (from position in positionsGroup select position.Value.SettlementDate).Max(),
                            // PM 20130905 [17949] Livraison
                            DeliveryQuantity = (from position in positionsGroup select position.Value.DeliveryQuantity).Sum(),
                            DeliveryStep = (from position in positionsGroup where position.Value.DeliveryQuantity != 0 select position.Value.DeliveryStep).FirstOrDefault(),
                            DeliveryStepDate = (from position in positionsGroup where position.Value.DeliveryQuantity != 0 select position.Value.DeliveryStepDate).FirstOrDefault(),
                        })).ToDictionary(k => k.Key, k => k.Value);
            }
            return clearerRiskData;
        }

        /// <summary>
        /// Prendre la position filtrée sur un ensemble d'asset
        /// </summary>
        /// <param name="pAsset"></param>
        /// <returns></returns>
        // PM 20200910 [25481] Refactoring
        //internal RiskDataPosition GetAssetRiskData(IEnumerable<int> pIdAsset)
        internal RiskDataPosition GetAssetRiskData(Dictionary<int, SQL_AssetETD> pAsset)
        {
            RiskDataPosition newRiskData;
            if ((pAsset != default(Dictionary<int, SQL_AssetETD>)) && (pAsset.Keys != default(IEnumerable<int>)) && (pAsset.Keys.Count() > 0))
            {
                Dictionary<PosRiskMarginKey, RiskMarginPosition> filteredposition = (
                    from position in m_Position
                    join idAsset in pAsset.Keys on position.Key.idAsset equals idAsset
                    select position).ToDictionary(k => k.Key, k => k.Value);

                newRiskData = new RiskDataPosition(filteredposition);
            }
            else
            {
                newRiskData = new RiskDataPosition();
            }
            return newRiskData;
        }

        /// <summary>
        /// Mets la position Exe/Ass/Mof à zéro
        /// </summary>
        // RD/PM 20170523 [23185] Modify
        public void ClearPosExeAss()
        {
            if (m_Position != default(Dictionary<PosRiskMarginKey, RiskMarginPosition>))
            {
                IEnumerable<PosRiskMarginKey> keys = m_Position.Keys.ToList();

                foreach (PosRiskMarginKey key in keys)
                {
                    RiskMarginPosition pos = m_Position[key];
                    pos.ExeAssQuantity = 0;
                    m_Position[key] = pos;
                }
            }
        }

        /// <summary>
        /// Regroupe une collection de données de risque
        /// </summary>
        /// <param name="pRiskData">Un ensemble de données de risque</param>
        /// <returns>Un nouvel objet représentant l'ensemble des donneés de risque</returns>
        public static RiskDataPosition Aggregate(IEnumerable<RiskDataPosition> pRiskData)
        {
            RiskDataPosition aggregate;
            if (pRiskData != default(IEnumerable<RiskDataPosition>))
            {
                Dictionary<PosRiskMarginKey, RiskMarginPosition> allData = (
                    from risk in pRiskData
                    from data in risk.m_Position
                    select data
                    ).ToDictionary(g => g.Key, g => g.Value, new PosRiskMarginKeyComparer());

                aggregate = new RiskDataPosition(allData);
            }
            else
            {
                aggregate = new RiskDataPosition();
            }
            return aggregate;
        }

        /// <summary>
        /// Regroupe les données de risque par actor/book
        /// </summary>
        /// <param name="pRiskData">un ensemble de données de risque</param>
        /// <returns>Un dictionaire ayant un couple d'Ids actor/book comme clé et les données de risque comme value</returns>
        public static Dictionary<RiskActorBook, RiskDataPosition> AggregateByActorBook(IEnumerable<RiskDataPosition> pRiskData)
        {
            Dictionary<RiskActorBook, RiskDataPosition> aggregate;
            if (pRiskData != default(IEnumerable<RiskDataPosition>))
            {
                aggregate = (
                    from risk in pRiskData
                    from position in risk.m_Position
                    select position
                    ).GroupBy(k => new RiskActorBook(k.Key.idA_Dealer, k.Key.idB_Dealer), new RiskActorBookComparer()
                    ).ToDictionary(g => g.Key, g => new RiskDataPosition(g.Select(p => p)), new RiskActorBookComparer());
            }
            else
            {
                aggregate = new Dictionary<RiskActorBook, RiskDataPosition>(new RiskActorBookComparer());
            }
            return aggregate;
        }
        #endregion Methods
    }

    /// <summary>
    /// Classe représentant les valeurs des trades sur lesquels porte un calcul de déposit
    /// </summary>
    /// PM 20170313 [22833] New
    public class RiskDataTradeValue : RiskDataTradePos
    {
        #region Members
        private Dictionary<RiskDataTradeKey, RiskMarginTradeValue> m_TradeValue;

        /// <summary>
        /// Cache pour les SQL_AssetCommodityContract
        /// key = IdCC, value = SQL_AssetCommodityContract
        /// </summary>
        // PM 20200910 [25481] ajout m_AssetCache
        private Dictionary<int, SQL_AssetCommodityContract> m_AssetCache = new Dictionary<int, SQL_AssetCommodityContract>();
        #endregion Members

        #region Accessors
        /// <summary>
        /// Cache pour les SQL_AssetCommodityContract
        /// key = IdCC, value = SQL_AssetCommodityContract
        /// </summary>
        // PM 20200910 [25481] ajout AssetCache
        internal Dictionary<int, SQL_AssetCommodityContract> AssetCache
        {
            get { return m_AssetCache; }
        }
        #endregion Accessors

        #region Constructor
        /// <summary>
        /// Constructeur
        /// </summary>
        public RiskDataTradeValue()
        {
            m_TradeValue = new Dictionary<RiskDataTradeKey, RiskMarginTradeValue>(new RiskDataTradeKeyComparer());
        }

        /// <summary>
        /// Constructeur
        /// </summary>
        /// <param name="pTradeValue"></param>
        public RiskDataTradeValue(IEnumerable<KeyValuePair<RiskDataTradeKey, RiskMarginTradeValue>> pTradeValue)
        {
            if (pTradeValue != default(IEnumerable<KeyValuePair<RiskDataTradeKey, RiskMarginTradeValue>>))
            {
                m_TradeValue = pTradeValue.ToDictionary(k => k.Key, v => v.Value, new RiskDataTradeKeyComparer());
            }
            else
            {
                m_TradeValue = new Dictionary<RiskDataTradeKey, RiskMarginTradeValue>(new RiskDataTradeKeyComparer());
            }
        }
        #endregion Constructor

        #region Methods
        /// <summary>
        /// Construction les valeurs de trade par date à partir d'un ensemble de trades
        /// </summary>
        /// <param name="pTradeValues"></param>
        /// <param name="pDtBusiness"></param>
        public void BuildTradeValueByDate(IEnumerable<TradeValue> pTradeValues, DateTime pDtBusiness, DateTime pDtBusinessNext)
        {
            if ((pTradeValues != default) && (pTradeValues.Count() > 0))
            {
                // Regrouper les trades par acteurs/books
                // PM 20200910 [25482] ajout IdAsset dans le regroupement
                var groupedTrades = pTradeValues.GroupBy(k => new
                {
                    k.IdADealer,
                    k.IdBDealer,
                    k.IdAClearer,
                    k.IdBClearer,
                    k.IdAsset
                });

                // Pour chaque acteurs/books
                foreach (var grpKey in groupedTrades)
                {
                    // 1- Prendre l'ensemble des différentes dates pour lesquelles il existe des trades jour (uniquement du lundi au vendredi)
                    List<DateTime> tradeDates = grpKey.Select(tr => tr.DtTimestamp.Date)
                        .Where(dt => (dt.DayOfWeek != DayOfWeek.Saturday) && (dt.DayOfWeek != DayOfWeek.Sunday))   // Ne pas prendre les samedi et dimanche
                        .Distinct().ToList();
                    //
                    // 2- Pour chaque date ci-dessus, prendre les dates veille
                    IEnumerable<DateTime> startDates = (from dt in tradeDates select new RiskDataNormalExposureDates(dt).ExposureStartDateTime.Date);
                    // 3- Pour chaque date ci-dessus, prendre les dates suivantes
                    // PM 20200908 [25476] Correction des dates de fin de période d'exposure
                    //IEnumerable<DateTime> endDates = (from dt in tradeDates select new RiskDataNormalExposureDates(dt).ExposureStartDateTime.Date);
                    IEnumerable<DateTime> endDates = (from dt in tradeDates select new RiskDataNormalExposureDates(dt).ExposureEndDateTime.Date);
                    // 4- Trier toutes les dates et supprimer les doublons
                    List<DateTime> allDates = (tradeDates.Union(startDates).Union(endDates)).Distinct().OrderBy(k => k).ToList();

                    // Pour chaque date recherche les trades à prendre en compte
                    foreach (DateTime dtAffect in allDates)
                    {
                        RiskDataExposureDates dtExposurePeriod;
                        if (dtAffect.Date == pDtBusiness.Date)
                        {
                            dtExposurePeriod = new RiskDataT0ExposureDates(dtAffect);
                        }
                        else if (dtAffect.Date == pDtBusinessNext.Date)
                        {
                            dtExposurePeriod = new RiskDataCurrentExposureDates(dtAffect);
                        }
                        else
                        {
                            dtExposurePeriod = new RiskDataNormalExposureDates(dtAffect);
                        }
                        // Prendre tous les trades concernés
                        IEnumerable<TradeValue> tradeOfDtExposure = grpKey.Where(tr => (tr.DtTimestamp > dtExposurePeriod.ExposureStartDateTime) && (tr.DtTimestamp <= dtExposurePeriod.ExposureEndDateTime));
                        // 
                        if (dtAffect.Date == pDtBusinessNext.Date)
                        {
                            // Pour la Current Exposure ajout des trades à règlement postérieur à Next Business Date
                            IEnumerable<TradeValue> tradeNotSettled = grpKey.Where(tr => (tr.DtSettlement > pDtBusinessNext.Date) && (tr.DtTimestamp <= dtExposurePeriod.ExposureStartDateTime));
                            tradeOfDtExposure = tradeOfDtExposure.Union(tradeNotSettled, new TradeValueComparer());
                        }
                        if (tradeOfDtExposure.Count() > 0)
                        {
                            // Constituer la clé
                            // PM 20200910 [25482] ajout IdAsset dans la clé
                            RiskDataTradeKey tradeKey = new RiskDataTradeKey(
                                grpKey.Key.IdADealer,
                                grpKey.Key.IdBDealer,
                                grpKey.Key.IdAClearer,
                                grpKey.Key.IdBClearer,
                                grpKey.Key.IdAsset,
                                dtAffect
                                );

                            // Constituer la value
                            RiskMarginTradeValue exposureValue;
                            IEnumerable<IMoney> amounts = from trade in tradeOfDtExposure
                                                          group trade by trade.Currency
                                                          into tradeByCurrency
                                                          select new Money(tradeByCurrency.Sum(tr => tr.Value), tradeByCurrency.Key);

                            if (dtAffect.Date == pDtBusinessNext.Date)
                            {
                                // Stocker les trades uniquement pour la Current Exposure (correspondant à pDtBusinessNext)
                                exposureValue = new RiskMarginTradeValue(tradeOfDtExposure.Select(tr => tr.IdT).ToArray(), amounts, tradeOfDtExposure);
                            }
                            else
                            {
                                exposureValue = new RiskMarginTradeValue(tradeOfDtExposure.Select(tr => tr.IdT).ToArray(), amounts, default);
                            }
                            // Stockage du nouvel élément
                            m_TradeValue.Add(tradeKey, exposureValue);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Construction de l'ensemble des assets des trades
        /// </summary>
        /// <param name="pCS"></param>
        // PM 20200910 [25481] New 
        public void BuildAssetCache(string pCS)
        {
            // Prendre les différents IdAsset
            IEnumerable<int> allIdAsset = m_TradeValue.Keys.Select(k => k.IdAsset).Distinct();
            // Construction du dictionnaire de SQL_AssetCommodityContract
            m_AssetCache = (
                from idAsset in allIdAsset
                select new SQL_AssetCommodityContract(pCS, idAsset)).ToDictionary(a => a.Id, a => a);
        }

        /// <summary>
        /// Copie les valeurs des membres de la classe dans celle passée en paramètre
        /// </summary>
        /// <param name="pDest">Classe de destination dans laquelle sont copié les valeurs des membres</param>
        /// <returns>True si les données ont été copiées</returns>
        public bool CopyMembersValue(RiskDataTradeValue pDest)
        {
            bool ret = pDest != default(RiskDataTradeValue);
            if (ret)
            {
                pDest.m_TradeValue = m_TradeValue;
                // PM 20200910 [25481] Ajout m_AssetCache
                pDest.m_AssetCache = m_AssetCache;
            }
            return ret;
        }

        /// <summary>
        /// Prendre la position filtrée sur un ensemble d'asset
        /// </summary>
        /// <param name="pAsset"></param>
        /// <returns></returns>
        // PM 20200910 [25481] New
        internal RiskDataTradeValue GetAssetRiskData(Dictionary<int, SQL_AssetCommodityContract> pAsset)
        {
            RiskDataTradeValue newRiskData;
            if ((pAsset != default)
                    && (pAsset.Keys != default(IEnumerable<int>)) && (pAsset.Keys.Count() > 0))
            {
                Dictionary<RiskDataTradeKey, RiskMarginTradeValue> filteredTradeValue = (
                    from tradeValue in m_TradeValue
                    join idAsset in pAsset.Keys on tradeValue.Key.IdAsset equals idAsset
                    select tradeValue).ToDictionary(k => k.Key, k => k.Value);

                // PM 20230104 [26181] Dans le cas où il n'y a pas de TradeValue, on ajoute les éventuels TradeValue fictifs provenant des MasterAgreements
                if (filteredTradeValue.Count == 0)
                {
                    // Les TradeValue fictifs provenant des MasterAgreements ont un IdAsset à 0
                    var tradeValueAggreement = m_TradeValue.Where(k => k.Key.IdAsset == 0);
                    filteredTradeValue = filteredTradeValue.Concat(tradeValueAggreement).ToDictionary(k => k.Key, k => k.Value);
                }

                newRiskData = new RiskDataTradeValue(filteredTradeValue)
                {
                    m_AssetCache = pAsset
                };
            }
            else
            {
                newRiskData = new RiskDataTradeValue();
            }
            return newRiskData;
        }

        /// <summary>
        /// Obtient le nombre de clé de valeur
        /// </summary>
        /// <returns></returns>
        public override int Count()
        {
            return m_TradeValue.Count();
        }

        /// <summary>
        /// Obtient les IdA des différents clearer
        /// </summary>
        /// <returns></returns>
        public override int[] GetClearersIdA()
        {
            int[] clearerIdA = m_TradeValue.Keys.Select(k => k.IdA_Clearer).Distinct().ToArray();
            return clearerIdA;
        }

        /// <summary>
        /// Obtient les IdA des différents dealer
        /// </summary>
        /// <returns></returns>
        public override int[] GetDealersIdA()
        {
            int[] dealerIdA = m_TradeValue.Keys.Select(k => k.IdA_Dealer).Distinct().ToArray();
            return dealerIdA;
        }

        /// <summary>
        /// Obtient l'ensemble des couples actor/book dealer
        /// </summary>
        /// <returns></returns>
        public override IEnumerable<RiskActorBook> GetDealersActorBook()
        {
            return m_TradeValue.Keys.Select(k => new RiskActorBook(k.IdA_Dealer, k.IdB_Dealer)).Distinct(new RiskActorBookComparer());
        }

        /// <summary>
        /// Obtient le premier couple Acteur/Book dealer
        /// </summary>
        /// <returns>null si aucun élément</returns>
        public override RiskActorBook GetFirstDealerActorBook()
        {
            RiskActorBook first = default;
            if (m_TradeValue.Count() > 0)
            {
                RiskDataTradeKey dataKey = m_TradeValue.First().Key;
                first = new RiskActorBook(dataKey.IdA_Dealer, dataKey.IdB_Dealer);
            }
            return first;
        }

        /// <summary>
        /// Obtient le premier couple Acteur/Book clearer
        /// </summary>
        /// <returns>null si aucun élément</returns>
        public override RiskActorBook GetFirstClearerActorBook()
        {
            RiskActorBook first = default;
            if (m_TradeValue.Count() > 0)
            {
                RiskDataTradeKey dataKey = m_TradeValue.First().Key;
                first = new RiskActorBook(dataKey.IdA_Clearer, dataKey.IdB_Clearer);
            }
            return first;
        }

        /// <summary>
        /// Prendre les données de risque d'une liste de books
        /// </summary>
        /// <param name="pBooksId">Liste de books</param>
        /// <returns>Les données de risque d'une collection de books</returns>
        /// <remarks>Les données de risque sont des références des originaux</remarks>
        public RiskDataTradeValue GetBooksRiskData(IEnumerable<int> pBooksId)
        {
            RiskDataTradeValue actorRiskData = new RiskDataTradeValue();
            if (pBooksId != default(IEnumerable<int>))
            {
                actorRiskData.m_TradeValue = m_TradeValue.Where(p => pBooksId.Contains(p.Key.IdB_Dealer)).ToDictionary(p => p.Key, p => p.Value);
            }
            return actorRiskData;
        }

        /// <summary>
        /// Construit les données de risque du clearer liés à l'ensemble des books données (pBooksId), 
        /// à partir des données de risque initiaux (données des dealers) 
        /// </summary>
        /// <param name="pBooksId">L'ensemble des books Id du CLEARER/MRO</param>
        /// <param name="pBooksOwnerId">L'Id de l'actor CLEARER/MRO propriétaire de l'ensemble des books. Cet Id sera utilisé comme propriétaire des données de risque retournées.</param>
        /// <returns>Toutes les données de risque (nettées par DtBusiness) dont l'Id du book de clearer appartient à l'ensemble des Ids de book de clearer en paramètre. 
        /// Les Ids des actor/book dealers initiaux seront écrasés par les Id passés en argument.</returns>
        /// <remarks>Les données de risque sont des copies des originaux</remarks>
        // PM 20200910 [25481] Ajout gestion idAsset et ensemble des trades
        public RiskDataTradeValue BuildClearerRiskData(IEnumerable<int> pBooksId, int pBooksOwnerId)
        {
            RiskDataTradeValue clearerRiskData = new RiskDataTradeValue();
            if ((pBooksId != default(IEnumerable<int>)) && (pBooksId.Count() > 0))
            {
                clearerRiskData.m_TradeValue = (
                    from tradeValueGroup in
                        ((from tradeValue in m_TradeValue where pBooksId.Contains(tradeValue.Key.IdB_Clearer) select tradeValue)
                            .GroupBy(elem => new
                            {
                                elem.Key.IdB_Clearer,
                                elem.Key.IdAsset,
                                elem.Key.DtBusiness,
                            })
                        )
                    select new KeyValuePair<RiskDataTradeKey, RiskMarginTradeValue>(
                        new RiskDataTradeKey
                        {
                            IdA_Dealer = pBooksOwnerId,
                            IdB_Dealer = tradeValueGroup.Key.IdB_Clearer,
                            IdA_Clearer = pBooksOwnerId,
                            IdB_Clearer = tradeValueGroup.Key.IdB_Clearer,
                            IdAsset = tradeValueGroup.Key.IdAsset,
                            DtBusiness = tradeValueGroup.Key.DtBusiness,
                        },
                        new RiskMarginTradeValue
                        {
                            TradeIds = (from tradeValue in tradeValueGroup from tradeId in tradeValue.Value.TradeIds select tradeId).ToArray(),
                            // PM 20170808 [23371] Modification de Value (decimal) en Amount (IEnumerable/IMoney) pour tenir compte de la devise
                            //Value = (from tradeValue in tradeValueGroup select tradeValue.Value.Value).Sum(),
                            Amount = (from trade in tradeValueGroup
                                      from moneys in trade.Value.Amount
                                      group moneys by moneys.Currency into amountByCur
                                      select new Money(amountByCur.Sum(a => a.Amount.DecValue), amountByCur.Key)
                                      ).Cast<IMoney>().ToList(),
                            Trades = from tradeValue in tradeValueGroup
                                     from trade in tradeValue.Value.Trades
                                     select trade,
                        })).ToDictionary(k => k.Key, k => k.Value, new RiskDataTradeKeyComparer());

                // Prendre les différents IdAsset
                IEnumerable<int> allIdAsset = clearerRiskData.m_TradeValue.Keys.Select(k => k.IdAsset).Distinct();
                // Construction du dictionnaire de SQL_AssetCommodityContract
                clearerRiskData.m_AssetCache = (
                    from asset in m_AssetCache
                    join idAsset in allIdAsset on asset.Key equals idAsset
                    select asset).ToDictionary(k => k.Key, k => k.Value);
            }
            return clearerRiskData;
        }

        /// <summary>
        /// Fournit la somme des valeurs de trades (Exposure) pour une date donnée par devise
        /// </summary>
        /// <param name="pDate"></param>
        /// <returns></returns>
        /// PM 20170808 [23371] Modification pour tenir compte de la devise
        public IEnumerable<IMoney> GetSumValueOfDate(DateTime pDate)
        {
            // Prendre tous les montants concernant la date
            IEnumerable<IMoney> allAmounts =
                from tradeValue in m_TradeValue
                where (tradeValue.Key.DtBusiness == pDate)
                from amount in tradeValue.Value.Amount
                select amount;

            // Sommer les montants par devise
            IEnumerable<IMoney> amounts =
                from amount in allAmounts
                group amount by amount.Currency into amountByCurrency
                select new Money(amountByCurrency.Sum(a => a.Amount.DecValue), amountByCurrency.Key);

            return amounts;
        }

        /// <summary>
        /// Fournit un dictionnaire contenant la somme des valeurs de trades (Exposure) par date et par devise
        /// </summary>
        /// <returns></returns>
        /// PM 20170808 [23371] Modification pour tenir compte de la devise
        public Dictionary<DateTime, IEnumerable<IMoney>> GetSumValueByDate()
        {
            Dictionary<DateTime, IEnumerable<IMoney>> valueDic = (
                from tradeValue in m_TradeValue
                group tradeValue.Value by tradeValue.Key.DtBusiness
                    into tradeValueDate
                select new
                {
                    Date = tradeValueDate.Key,
                    Amount = from trade in tradeValueDate
                             from amount in trade.Amount
                             group amount by amount.Currency into amountByCurrency
                             select new Money(amountByCurrency.Sum(a => a.Amount.DecValue), amountByCurrency.Key),
                }
                    ).ToDictionary(k => k.Date, v => v.Amount.Cast<IMoney>());

            return valueDic;
        }

        /// <summary>
        /// Fournit l'ensemble des objets RiskMarginTradeValue à une date donnée
        /// </summary>
        /// <param name="pDate"></param>
        /// <returns></returns>
        /// PM 20170808 [23371] New
        public IEnumerable<RiskMarginTradeValue> GetRiskMarginTradeValueOfDate(DateTime pDate)
        {

            IEnumerable<RiskMarginTradeValue> riskTradeValue =
                from tradeValue in m_TradeValue
                where (tradeValue.Key.DtBusiness == pDate)
                select tradeValue.Value;

            return riskTradeValue;
        }

        /// <summary>
        /// Regroupe une collection de données de risque
        /// </summary>
        /// <param name="pRiskData">Un ensemble de données de risque</param>
        /// <returns>Un nouvel objet représentant l'ensemble des donneés de risque</returns>
        public static RiskDataTradeValue Aggregate(IEnumerable<RiskDataTradeValue> pRiskData)
        {
            RiskDataTradeValue aggregate;
            if ((pRiskData != default(IEnumerable<RiskDataTradeValue>)) && (pRiskData.Count() > 0))
            {
                Dictionary<RiskDataTradeKey, RiskMarginTradeValue> allData = (
                    from risk in pRiskData
                    from data in risk.m_TradeValue
                    select data
                    ).ToDictionary(g => g.Key, g => g.Value, new RiskDataTradeKeyComparer());
                aggregate = new RiskDataTradeValue(allData);
            }
            else
            {
                aggregate = new RiskDataTradeValue();
            }
            return aggregate;
        }

        /// <summary>
        /// Regroupe les données de risque par actor/book
        /// </summary>
        /// <param name="pRiskData">un ensemble de données de risque</param>
        /// <returns>Un dictionaire ayant un couple d'Ids actor/book comme clé et les données de risque comme value</returns>
        public static Dictionary<RiskActorBook, RiskDataTradeValue> AggregateByActorBook(IEnumerable<RiskDataTradeValue> pRiskData)
        {
            Dictionary<RiskActorBook, RiskDataTradeValue> aggregate;
            if ((pRiskData != default(IEnumerable<RiskDataTradeValue>)) && (pRiskData.Count() > 0))
            {
                var groupedData = (
                from risk in pRiskData
                from data in risk.m_TradeValue
                select data
                    ).GroupBy(key => new RiskActorBook(key.Key.IdA_Dealer, key.Key.IdB_Dealer), new RiskActorBookComparer());

                aggregate = groupedData.ToDictionary(g => g.Key, g => new RiskDataTradeValue(g.Select(p => p)), new RiskActorBookComparer());
            }
            else
            {
                aggregate = new Dictionary<RiskActorBook, RiskDataTradeValue>(new RiskActorBookComparer());
            }
            return aggregate;
        }
        #endregion Methods
    }

    /// <summary>
    /// Classe représentant les trades du jour pour lesquels ne pas calculer de déposit
    /// </summary>
    // PM 20221212 [XXXXX] New
    public class RiskDataNoMarginTrade : RiskDataTradePos
    {
        #region Members
        /// <summary>
        /// Collection des trades du jour pour lesquels ne pas effectuer de calcul de déposit
        /// </summary>
        private Dictionary<RiskDataNoMarginTradeKey, RiskNoMarginTrade> m_NoMarginTrade;

        /// <summary>
        /// Cache pour elements Asset ETD sql
        /// key = IdAsset, value = SQL_AssetETD
        /// </summary>
        private Dictionary<int, SQL_AssetETD> m_AssetETDCache = new Dictionary<int, SQL_AssetETD>();

        /// <summary>
        /// Cache pour les SQL_AssetCommodityContract
        /// key = IdCC, value = SQL_AssetCommodityContract
        /// </summary>
        private Dictionary<int, SQL_AssetCommodityContract> m_AssetComodityCache = new Dictionary<int, SQL_AssetCommodityContract>();
        #endregion Members

        #region Accessors
        /// <summary>
        /// Trades du jours regroupés
        /// </summary>
        public Dictionary<RiskDataNoMarginTradeKey, RiskNoMarginTrade> NoMarginTrade
        {
            get
            {
                return m_NoMarginTrade;
            }
        }
        #endregion Accessors

        #region Constructor
        /// <summary>
        /// Constructeur
        /// </summary>
        public RiskDataNoMarginTrade()
        {
            m_NoMarginTrade = new Dictionary<RiskDataNoMarginTradeKey, RiskNoMarginTrade>(new RiskDataNoMarginTradeKeyComparer());
        }

        /// <summary>
        /// Constructeur
        /// </summary>
        /// <param name="pNoMarginTrade"></param>
        public RiskDataNoMarginTrade(IEnumerable<KeyValuePair<RiskDataNoMarginTradeKey, RiskNoMarginTrade>> pNoMarginTrade)
        {
            if (pNoMarginTrade != default(IEnumerable<KeyValuePair<RiskDataNoMarginTradeKey, RiskNoMarginTrade>>))
            {
                m_NoMarginTrade = pNoMarginTrade.ToDictionary(k => k.Key, v => v.Value, new RiskDataNoMarginTradeKeyComparer());
            }
            else
            {
                m_NoMarginTrade = new Dictionary<RiskDataNoMarginTradeKey, RiskNoMarginTrade>(new RiskDataNoMarginTradeKeyComparer());
            }
        }
        #endregion Constructor

        #region Methods
        /// <summary>
        /// Construction les données des trades
        /// </summary>
        /// <param name="pTradeValues"></param>
        /// <param name="pDtBusiness"></param>
        public void BuildTradeNoMargin(IEnumerable<TradeNoMargin> pTradeNoMargin)
        {
            if ((pTradeNoMargin != default(IEnumerable<TradeNoMargin>)) && (pTradeNoMargin.Count() > 0))
            {
                // Regrouper les trades par acteurs/books/IdAsset/AssetCategory
                var groupedTrades = pTradeNoMargin.GroupBy(k => new
                {
                    k.IdA_Dealer,
                    k.IdB_Dealer,
                    k.IdA_Clearer,
                    k.IdB_Clearer,
                    k.AssetCategory,
                    k.IdAsset
                });

                // Pour chaque groupes de trades
                foreach (var grpKey in groupedTrades)
                {
                    // Constituer la clé
                    RiskDataNoMarginTradeKey tradeKey = new RiskDataNoMarginTradeKey(
                                grpKey.Key.IdA_Dealer,
                                grpKey.Key.IdB_Dealer,
                                grpKey.Key.IdA_Clearer,
                                grpKey.Key.IdB_Clearer,
                                grpKey.Key.AssetCategory,
                                grpKey.Key.IdAsset
                                );

                    // Constituer les trades
                    IEnumerable<TradeNoMargin> tradesOfKey = grpKey;
                    RiskNoMarginTrade trades = new RiskNoMarginTrade(tradesOfKey);

                    // Stockage du nouvel élément
                    m_NoMarginTrade.Add(tradeKey, trades);
                }
            }
        }

        /// <summary>
        /// Construction de l'ensemble des assets des trades
        /// </summary>
        /// <param name="pCS"></param>
        public void BuildAssetCache(string pCS)
        {
            // Prendre les différents IdAsset commodity
            IEnumerable<int> allAssetComodity = m_NoMarginTrade.Keys.Where(k => k.AssetCategory == Cst.UnderlyingAsset.Commodity).Select(k => k.IdAsset).Distinct();
            // Construction du dictionnaire de SQL_AssetCommodityContract
            m_AssetComodityCache = (
                from idAsset in allAssetComodity
                select new SQL_AssetCommodityContract(pCS, idAsset)).ToDictionary(a => a.Id, a => a);

            // Prendre les différents IdAsset ETD
            IEnumerable<int> allAssetETD = m_NoMarginTrade.Keys.Where(k => k.AssetCategory == Cst.UnderlyingAsset.ExchangeTradedContract).Select(k => k.IdAsset).Distinct();
            // Construction du dictionnaire de SQL_AssetETD
            m_AssetETDCache = (
                from idAsset in allAssetETD
                select new SQL_AssetETD(pCS, idAsset)).ToDictionary(a => a.Id, a => a);
        }

        /// <summary>
        /// Prendre les trades filtrés sur un ensemble d'asset
        /// </summary>
        /// <param name="pAssetETD">Asset ETD</param>
        /// <param name="pAssetCommodity">Asset Commodity</param>
        /// <returns></returns>
        internal RiskDataNoMarginTrade GetAssetRiskData(Dictionary<int, SQL_AssetETD> pAssetETD, Dictionary<int, SQL_AssetCommodityContract> pAssetCommodity)
        {
            RiskDataNoMarginTrade newRiskData = new RiskDataNoMarginTrade();
            Dictionary<RiskDataNoMarginTradeKey, RiskNoMarginTrade> filteredTradeETD = new Dictionary<RiskDataNoMarginTradeKey, RiskNoMarginTrade>();
            Dictionary<RiskDataNoMarginTradeKey, RiskNoMarginTrade> filteredTradeCommodity = new Dictionary<RiskDataNoMarginTradeKey, RiskNoMarginTrade>();

            if ((pAssetETD != default(Dictionary<int, SQL_AssetETD>))
                    && (pAssetETD.Keys != default(IEnumerable<int>)) && (pAssetETD.Keys.Count() > 0))
            {
                filteredTradeETD = (
                    from tradeETD in m_NoMarginTrade
                    join idAsset in pAssetETD.Keys on tradeETD.Key.IdAsset equals idAsset
                    select tradeETD).ToDictionary(k => k.Key, k => k.Value);

                newRiskData.m_AssetETDCache = pAssetETD;
            }
            if ((pAssetCommodity != default)
                && (pAssetCommodity.Keys != default(IEnumerable<int>)) && (pAssetCommodity.Keys.Count() > 0))
            {
                filteredTradeCommodity = (
                    from tradeCommodity in m_NoMarginTrade
                    join idAsset in pAssetCommodity.Keys on tradeCommodity.Key.IdAsset equals idAsset
                    select tradeCommodity).ToDictionary(k => k.Key, k => k.Value);

                newRiskData.m_AssetComodityCache = pAssetCommodity;
            }
            if ((filteredTradeETD.Count > 0) || (filteredTradeCommodity.Count > 0))
            {
                newRiskData.m_NoMarginTrade = filteredTradeETD.Union(filteredTradeCommodity).ToDictionary(k => k.Key, k => k.Value);
            }

            return newRiskData;
        }

        /// <summary>
        /// Obtient le nombre de clé de valeur
        /// </summary>
        /// <returns></returns>
        public override int Count()
        {
            return m_NoMarginTrade.Count();
        }

        /// <summary>
        /// Obtient les IdA des différents clearer
        /// </summary>
        /// <returns></returns>
        public override int[] GetClearersIdA()
        {
            int[] clearerIdA = m_NoMarginTrade.Keys.Select(k => k.IdA_Clearer).Distinct().ToArray();
            return clearerIdA;
        }

        /// <summary>
        /// Obtient les IdA des différents dealer
        /// </summary>
        /// <returns></returns>
        public override int[] GetDealersIdA()
        {
            int[] dealerIdA = m_NoMarginTrade.Keys.Select(k => k.IdA_Dealer).Distinct().ToArray();
            return dealerIdA;
        }

        /// <summary>
        /// Obtient l'ensemble des couples actor/book dealer
        /// </summary>
        /// <returns></returns>
        public override IEnumerable<RiskActorBook> GetDealersActorBook()
        {
            return m_NoMarginTrade.Keys.Select(k => new RiskActorBook(k.IdA_Dealer, k.IdB_Dealer)).Distinct(new RiskActorBookComparer());
        }

        /// <summary>
        /// Obtient le premier couple Acteur/Book dealer
        /// </summary>
        /// <returns>null si aucun élément</returns>
        public override RiskActorBook GetFirstDealerActorBook()
        {
            RiskActorBook first = default;
            if (m_NoMarginTrade.Count() > 0)
            {
                RiskDataNoMarginTradeKey dataKey = m_NoMarginTrade.First().Key;
                first = new RiskActorBook(dataKey.IdA_Dealer, dataKey.IdB_Dealer);
            }
            return first;
        }

        /// <summary>
        /// Obtient le premier couple Acteur/Book clearer
        /// </summary>
        /// <returns>null si aucun élément</returns>
        public override RiskActorBook GetFirstClearerActorBook()
        {
            RiskActorBook first = default;
            if (m_NoMarginTrade.Count() > 0)
            {
                RiskDataNoMarginTradeKey dataKey = m_NoMarginTrade.First().Key;
                first = new RiskActorBook(dataKey.IdA_Clearer, dataKey.IdB_Clearer);
            }
            return first;
        }

        /// <summary>
        /// Prendre les trades d'une liste de books
        /// </summary>
        /// <param name="pBooksId">Liste de books</param>
        /// <returns>Les données de risque d'une collection de books</returns>
        /// <remarks>Les données de risque sont des références des originaux</remarks>
        public RiskDataNoMarginTrade GetBooksRiskData(IEnumerable<int> pBooksId)
        {
            RiskDataNoMarginTrade actorRiskData = new RiskDataNoMarginTrade();
            if (pBooksId != default(IEnumerable<int>))
            {
                actorRiskData.m_NoMarginTrade = m_NoMarginTrade.Where(p => pBooksId.Contains(p.Key.IdB_Dealer)).ToDictionary(p => p.Key, p => p.Value);
            }
            return actorRiskData;
        }

        /// <summary>
        /// Construit les données de risque du clearer liés à l'ensemble des books données (pBooksId), 
        /// à partir des données de risque initiaux (données des dealers) 
        /// </summary>
        /// <param name="pBooksId">L'ensemble des books Id du CLEARER/MRO</param>
        /// <param name="pBooksOwnerId">L'Id de l'actor CLEARER/MRO propriétaire de l'ensemble des books. Cet Id sera utilisé comme propriétaire des données de risque retournées.</param>
        /// <returns>Toutes les données de risque dont l'Id du book de clearer appartient à l'ensemble des Ids de book de clearer en paramètre. 
        /// Les Ids des actor/book dealers initiaux seront écrasés par les Id passés en argument.</returns>
        /// <remarks>Les données de risque sont des copies des originaux</remarks>
        public RiskDataNoMarginTrade BuildClearerRiskData(IEnumerable<int> pBooksId, int pBooksOwnerId)
        {
            RiskDataNoMarginTrade clearerRiskData = new RiskDataNoMarginTrade();
            if ((pBooksId != default(IEnumerable<int>)) && (pBooksId.Count() > 0))
            {
                clearerRiskData.m_NoMarginTrade = (
                    from tradeGroup in
                        ((from trade in m_NoMarginTrade where pBooksId.Contains(trade.Key.IdB_Clearer) select trade)
                            .GroupBy(elem => new
                            {
                                elem.Key.IdB_Clearer,
                                elem.Key.AssetCategory,
                                elem.Key.IdAsset,
                            })
                        )
                    select new KeyValuePair<RiskDataNoMarginTradeKey, RiskNoMarginTrade>(
                        new RiskDataNoMarginTradeKey
                        {
                            IdA_Dealer = pBooksOwnerId,
                            IdB_Dealer = tradeGroup.Key.IdB_Clearer,
                            IdA_Clearer = pBooksOwnerId,
                            IdB_Clearer = tradeGroup.Key.IdB_Clearer,
                            AssetCategory = tradeGroup.Key.AssetCategory,
                            IdAsset = tradeGroup.Key.IdAsset,
                        },
                        new RiskNoMarginTrade
                        {
                            Trades = (from trade in tradeGroup
                                     from trades in trade.Value.Trades
                                     select trades).ToList(),
                        })).ToDictionary(k => k.Key, k => k.Value, new RiskDataNoMarginTradeKeyComparer());

                // Prendre les différents IdAsset ETD
                IEnumerable<int> allIdAssetETD = clearerRiskData.m_NoMarginTrade.Keys.Where(k => k.AssetCategory == Cst.UnderlyingAsset.ExchangeTradedContract).Select(k => k.IdAsset).Distinct();
                // Construction du dictionnaire de SQL_AssetETD
                clearerRiskData.m_AssetETDCache = (
                    from asset in m_AssetETDCache
                    join idAsset in allIdAssetETD on asset.Key equals idAsset
                    select asset).ToDictionary(k => k.Key, k => k.Value);

                // Prendre les différents IdAsset Commodity
                IEnumerable<int> allIdAsset = clearerRiskData.m_NoMarginTrade.Keys.Where(k => k.AssetCategory == Cst.UnderlyingAsset.Commodity).Select(k => k.IdAsset).Distinct();
                // Construction du dictionnaire de SQL_AssetCommodityContract
                clearerRiskData.m_AssetComodityCache = (
                    from asset in m_AssetComodityCache
                    join idAsset in allIdAsset on asset.Key equals idAsset
                    select asset).ToDictionary(k => k.Key, k => k.Value);
            }
            return clearerRiskData;
        }

        /// <summary>
        /// Regroupe une collection de données de risque
        /// </summary>
        /// <param name="pRiskData">Un ensemble de données de risque</param>
        /// <returns>Un nouvel objet représentant l'ensemble des donneés de risque</returns>
        public static RiskDataNoMarginTrade Aggregate(IEnumerable<RiskDataNoMarginTrade> pRiskData)
        {
            RiskDataNoMarginTrade aggregate;
            if ((pRiskData != default(IEnumerable<RiskDataNoMarginTrade>)) && (pRiskData.Count() > 0))
            {
                Dictionary<RiskDataNoMarginTradeKey, RiskNoMarginTrade> allData = (
                    from risk in pRiskData
                    from data in risk.m_NoMarginTrade
                    select data
                    ).ToDictionary(g => g.Key, g => g.Value, new RiskDataNoMarginTradeKeyComparer());
                aggregate = new RiskDataNoMarginTrade(allData);
            }
            else
            {
                aggregate = new RiskDataNoMarginTrade();
            }
            return aggregate;
        }

        /// <summary>
        /// Regroupe les données de risque par actor/book
        /// </summary>
        /// <param name="pRiskData">un ensemble de données de risque</param>
        /// <returns>Un dictionaire ayant un couple d'Ids actor/book comme clé et les données de risque comme value</returns>
        public static Dictionary<RiskActorBook, RiskDataNoMarginTrade> AggregateByActorBook(IEnumerable<RiskDataNoMarginTrade> pRiskData)
        {
            Dictionary<RiskActorBook, RiskDataNoMarginTrade> aggregate;
            if ((pRiskData != default(IEnumerable<RiskDataNoMarginTrade>)) && (pRiskData.Count() > 0))
            {
                var groupedData = (
                from risk in pRiskData
                from data in risk.m_NoMarginTrade
                select data
                    ).GroupBy(key => new RiskActorBook(key.Key.IdA_Dealer, key.Key.IdB_Dealer), new RiskActorBookComparer());

                aggregate = groupedData.ToDictionary(g => g.Key, g => new RiskDataNoMarginTrade(g.Select(p => p)), new RiskActorBookComparer());
            }
            else
            {
                aggregate = new Dictionary<RiskActorBook, RiskDataNoMarginTrade>(new RiskActorBookComparer());
            }
            return aggregate;
        }
        #endregion Methods
    }

    /// <summary>
    /// Trades pour lesquels ne pas effectuer de calcul de déposit
    /// </summary>
    // PM 20221212 [XXXXX] New
    public class RiskNoMarginTrade
    {
        #region Members
        /// <summary>
        /// Collection des trades 
        /// </summary>
        private List<TradeNoMargin> m_Trades;
        #endregion Members

        #region Accessors
        // Collection des trades
        public List<TradeNoMargin> Trades
        {
            get { return m_Trades; }
            set
            {
                m_Trades = value;
                if (m_Trades == default(List<TradeNoMargin>))
                {
                    m_Trades = new List<TradeNoMargin>();
                }
            }
        }
        #endregion Accessors

        #region Constructor
        /// <summary>
        /// Constructeur
        /// </summary>
        public RiskNoMarginTrade()
        {
            m_Trades = new List<TradeNoMargin>();
        }

        /// <summary>
        /// Constructeur
        /// </summary>
        /// <param name="pTrades"></param>
        public RiskNoMarginTrade(IEnumerable<TradeNoMargin> pTrades)
        {
            if (pTrades != default(IEnumerable<TradeNoMargin>))
            {
                m_Trades = pTrades.ToList();
            }
            else
            {
                m_Trades = new List<TradeNoMargin>();
            }
        }

        #endregion Constructor
    }

    /// <summary>
    /// IEqualityComparer pour la class RiskDataNoMarginTradeKey
    /// </summary>
    // PM 20221212 [XXXXX] New
    public class RiskDataNoMarginTradeKeyComparer : IEqualityComparer<RiskDataNoMarginTradeKey>
    {
        #region IEqualityComparer
        /// <summary>
        /// Les RiskDataTradeNoMarginKey sont egaux si tous leurs membres sont égaux
        /// </summary>
        /// <param name="pX">1er RiskDataTradeNoMarginKey à comparer</param>
        /// <param name="pY">2ème RiskDataTradeNoMarginKey à comparer</param>
        /// <returns>true si x Equals Y, sinon false</returns>
        public bool Equals(RiskDataNoMarginTradeKey pX, RiskDataNoMarginTradeKey pY)
        {
            return (pX == pY)
                || ((pX != default(RiskDataNoMarginTradeKey))
                    && (pY != default(RiskDataNoMarginTradeKey))
                    && (pX.IdA_Dealer == pY.IdA_Dealer)
                    && (pX.IdB_Dealer == pY.IdB_Dealer)
                    && (pX.IdA_Clearer == pY.IdA_Clearer)
                    && (pX.IdB_Clearer == pY.IdB_Clearer)
                    && (pX.AssetCategory == pY.AssetCategory)
                    && (pX.IdAsset == pY.IdAsset));
        }

        /// <summary>
        /// La méthode GetHashCode fournissant la même valeur pour des objets qui sont égaux.
        /// </summary>
        /// <param name="pObj">L'objet dont on veut le hash code</param>
        /// <returns>La valeur du hash code</returns>
        public int GetHashCode(RiskDataNoMarginTradeKey pObj)
        {
            //Vérifier si l'obet est null
            if (pObj is null) return 0;

            //Calcul du hash code pour l'objet.
            return (int)(pObj.IdA_Dealer.GetHashCode()
                ^ pObj.IdB_Dealer.GetHashCode()
                ^ pObj.IdA_Clearer.GetHashCode()
                ^ pObj.IdB_Clearer.GetHashCode()
                ^ pObj.AssetCategory.GetHashCode()
                ^ pObj.IdAsset.GetHashCode());
        }
        #endregion IEqualityComparer
    }

    /// <summary>
    /// Classe utilisée comme clé pour les trades pour lesquels ne pas effectuer de calcul de déposit
    /// </summary>
    // PM 20221212 [XXXXX] New
    public class RiskDataNoMarginTradeKey
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("idA_Dealer")]
        public int IdA_Dealer;
        [System.Xml.Serialization.XmlElementAttribute("idB_Dealer")]
        public int IdB_Dealer;
        [System.Xml.Serialization.XmlElementAttribute("idA_Clearer")]
        public int IdA_Clearer;
        [System.Xml.Serialization.XmlElementAttribute("idB_Clearer")]
        public int IdB_Clearer;
        [System.Xml.Serialization.XmlElementAttribute("assetCategory")]
        public Cst.UnderlyingAsset AssetCategory;
        [System.Xml.Serialization.XmlElementAttribute("idAsset")]
        public int IdAsset;
        #endregion Members

        #region Constructor
        /// <summary>
        /// Constructeur
        /// </summary>
        public RiskDataNoMarginTradeKey() { }

        /// <summary>
        /// Constructeur
        /// </summary>
        /// <param name="pIdA_Dealer"></param>
        /// <param name="pIdB_Dealer"></param>
        /// <param name="pIdA_Clearer"></param>
        /// <param name="pIdB_Clearer"></param>
        /// <param name="pAssetCategory"></param>
        /// <param name="pIdAsset"></param>
        public RiskDataNoMarginTradeKey(int pIdA_Dealer, int pIdB_Dealer, int pIdA_Clearer, int pIdB_Clearer, Cst.UnderlyingAsset pAssetCategory, int pIdAsset)
        {
            IdA_Dealer = pIdA_Dealer;
            IdB_Dealer = pIdB_Dealer;
            IdA_Clearer = pIdA_Clearer;
            IdB_Clearer = pIdB_Clearer;
            IdAsset = pIdAsset;
            AssetCategory = pAssetCategory;
        }
        #endregion Constructor
    }

    /// <summary>
    /// 
    /// </summary>
    // PM 20200910 [25481] New
    public class RiskAssetCache
    {
        #region Members
        /// <summary>
        /// Cache pour les SQL_AssetETD
        /// key = IdAsset, value = SQL_AssetETD
        /// </summary>
        private readonly Dictionary<int, SQL_AssetETD> m_AssetETDCache = new Dictionary<int, SQL_AssetETD>();

        /// <summary>
        /// Cache pour les SQL_AssetCommodityContract
        /// key = IdCC, value = SQL_AssetCommodityContract
        /// </summary>

        private readonly Dictionary<int, SQL_AssetCommodityContract> m_AssetCOMCache = new Dictionary<int, SQL_AssetCommodityContract>();
        #endregion Members

        #region Accessors
        /// <summary>
        /// Cache pour les SQL_AssetETD
        /// key = IdAsset, value = SQL_AssetETD
        /// </summary>
        public Dictionary<int, SQL_AssetETD> AssetETDCache
        {
            get { return m_AssetETDCache; }
        }

        /// <summary>
        /// Cache pour les SQL_AssetCommodityContract
        /// key = IdCC, value = SQL_AssetCommodityContract
        /// </summary>
        internal Dictionary<int, SQL_AssetCommodityContract> AssetCOMCache
        {
            get { return m_AssetCOMCache; }
        }
        #endregion Accessors

        #region Constructor
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pAssetETDCache"></param>
        /// <param name="pAssetCOMCache"></param>
        public RiskAssetCache(Dictionary<int, SQL_AssetETD> pAssetETDCache, Dictionary<int, SQL_AssetCommodityContract> pAssetCOMCache)
        {
            m_AssetETDCache = pAssetETDCache;
            m_AssetCOMCache = pAssetCOMCache;
            if (m_AssetETDCache == default(Dictionary<int, SQL_AssetETD>))
            {
                m_AssetETDCache = new Dictionary<int, SQL_AssetETD>();
            }
            if (m_AssetCOMCache == default)
            {
                m_AssetCOMCache = new Dictionary<int, SQL_AssetCommodityContract>();
            }
        }
        #endregion Constructor

        #region Methods
        /// <summary>
        /// Construction des Asset concernés par la méthode
        /// </summary>
        /// <param name="pIdIMMethod"></param>
        /// <returns></returns>
        public RiskAssetCache AssetCacheFromMethod(int pIdIMMethod)
        {
            Dictionary<int, SQL_AssetETD> assetETD = (
                from asset in m_AssetETDCache
                where (asset.Value.IdImMethod == pIdIMMethod)
                select asset).ToDictionary(a => a.Key, a => a.Value);

            Dictionary<int, SQL_AssetCommodityContract> assetCOM = (
                from asset in m_AssetCOMCache
                where (asset.Value.IdImMethod == pIdIMMethod)
                select asset).ToDictionary(a => a.Key, a => a.Value);

            return (new RiskAssetCache(assetETD, assetCOM));
        }
        #endregion Methods
    }
}
