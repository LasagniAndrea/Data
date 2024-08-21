#region Using Directives
using EFS.ACommon;
using EFS.Common;
using EfsML.Enum;
using FixML.v50SP1.Enum;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
#endregion Using Directives


namespace EFS.Process.PosKeeping
{
    #region TradeKey
    /// <summary>
    /// Elements constituants la clé identique hors contexte afin de merger les trades
    /// </summary>
    public class TradeKey
    {
        #region Members
        public int IdA_Dealer { set; get; }
        public int IdB_Dealer { set; get; }
        public int IdA_Clearer { set; get; }
        public int IdB_Clearer { set; get; }
        public int IdAsset { set; get; }
        public DateTime DtBusiness { set; get; }
        public DateTime DtTrade { set; get; }
        public string Side { set; get; }
        public Nullable<int> IdA_ExecBroker { set; get; }
        // EG 20150723 IDDC Nullable
        public Nullable<int> IdDC { set; get; }
        #endregion Members
        #region Constructors
        public TradeKey() { }
        public TradeKey(TradeKey pTradeKey):this(pTradeKey.IdA_Dealer, pTradeKey.IdB_Dealer, pTradeKey.IdA_Clearer, pTradeKey.IdB_Clearer,
            pTradeKey.IdDC, pTradeKey.IdAsset, pTradeKey.DtBusiness, pTradeKey.DtTrade, pTradeKey.Side, pTradeKey.IdA_ExecBroker)
        {
        }
        // EG 20150723 IDDC Nullable
        public TradeKey(int pIdA_Dealer, int pIdB_Dealer, int pIdA_Clearer, int pIdB_Clearer, Nullable<int> pIdDC,
            int pIdAsset, DateTime pDtBusiness, DateTime pDtTrade, string pSide, Nullable<int> pIdA_ExecBroker)
        {
            IdA_Dealer = pIdA_Dealer;
            IdB_Dealer = pIdB_Dealer;
            IdA_Clearer = pIdA_Clearer;
            IdB_Clearer = pIdB_Clearer;
            IdDC = pIdDC;
            IdAsset = pIdAsset;
            DtBusiness = pDtBusiness;
            DtTrade = pDtTrade;
            Side = pSide;
            IdA_ExecBroker = pIdA_ExecBroker;
        }
        public TradeKey(DataRow pRowTrade)
        {
            IdA_Dealer = Convert.ToInt32(pRowTrade["IDA_DEALER"]);
            IdB_Dealer = Convert.ToInt32(pRowTrade["IDB_DEALER"]);
            IdA_Clearer = Convert.ToInt32(pRowTrade["IDA_CLEARER"]);
            IdB_Clearer = Convert.ToInt32(pRowTrade["IDB_CLEARER"]);
            IdAsset = Convert.ToInt32(pRowTrade["IDASSET"]);
            DtBusiness = Convert.ToDateTime(pRowTrade["DTBUSINESS"]);
            DtTrade = Convert.ToDateTime(pRowTrade["DTTRADE"]);
            Side = pRowTrade["SIDE"].ToString();
            if (false == Convert.IsDBNull(pRowTrade["IDA_EXECBROKER"]))
                IdA_ExecBroker = Convert.ToInt32(pRowTrade["IDA_EXECBROKER"]);
            // EG 20150723 IDDC Nullable
            if (pRowTrade.Table.Columns.Contains("IDDC") && (false == Convert.IsDBNull(pRowTrade["IDDC"])))
                IdDC = Convert.ToInt32(pRowTrade["IDDC"]);
        }
        #endregion Constructors
    }
    #endregion TradeKey
    #region TradeKeyComparer
    /// <summary>
    /// Comparer de trades par TradeKey
    /// </summary>
    internal class TradeKeyComparer : IEqualityComparer<TradeKey>
    {
        #region IEqualityComparer
        /// <summary>
        /// Les TradeKey sont égaux s'ils ont les même:
        /// DEALER  (ACTEUR/BOOK), CLEARER (ACTEUR/BOOK), IDASSET
        /// DTBUSINESS, DTTRADE, SIDE, EXECUTIVE BROKER (si existe)
        /// </summary>
        /// <param name="x">1er TradeKey à comparer</param>
        /// <param name="y">2ème TradeKey à comparer</param>
        /// <returns>true si x Equals Y, sinon false</returns>
        public bool Equals(TradeKey pTradeKey1, TradeKey pTradeKey2)
        {

            //Vérifier si les objets référencent les même données
            if (ReferenceEquals(pTradeKey1, pTradeKey2)) return true;

            //Vérifier si un des objets est null
            if (pTradeKey1 is null || pTradeKey2 is null)
                return false;

            // Vérifier qu'il s'agit des même TradeInfo
            return (pTradeKey1.IdA_Dealer == pTradeKey2.IdA_Dealer) &&
                   (pTradeKey1.IdB_Dealer == pTradeKey2.IdB_Dealer) &&
                   (pTradeKey1.IdA_Clearer == pTradeKey2.IdA_Clearer) &&
                   (pTradeKey1.IdB_Clearer == pTradeKey2.IdB_Clearer) &&
                   (pTradeKey1.IdDC == pTradeKey2.IdDC) &&
                   (pTradeKey1.IdAsset == pTradeKey2.IdAsset) &&
                   (pTradeKey1.DtBusiness == pTradeKey2.DtBusiness) &&
                   (pTradeKey1.DtTrade == pTradeKey2.DtTrade) &&
                   (pTradeKey1.Side == pTradeKey2.Side) &&
                   (pTradeKey1.IdA_ExecBroker == pTradeKey2.IdA_ExecBroker);
        }

        /// <summary>
        /// La méthode GetHashCode fournissant la même valeur pour des objets Tradeinfo qui sont égaux.
        /// </summary>
        /// <param name="pCombinedCommodity">Le paramètre TradeInfo dont on veut le hash code</param>
        /// <returns>La valeur du hash code</returns>
        public int GetHashCode(TradeKey pTradeKey)
        {
            //Vérifier si l'obet est null
            if (pTradeKey is null) return 0;

            int hashIdA_Dealer = pTradeKey.IdA_Dealer.GetHashCode();
            int hashIdB_Dealer = pTradeKey.IdB_Dealer.GetHashCode();
            int hashIdA_Clearer = pTradeKey.IdA_Clearer.GetHashCode();
            int hashIdB_Clearer = pTradeKey.IdB_Clearer.GetHashCode();
            int hashIdDC = pTradeKey.IdDC.GetHashCode();
            int hashIdAsset = pTradeKey.IdAsset.GetHashCode();
            int hashDtBusiness = pTradeKey.DtBusiness.GetHashCode();
            int hashDtTrade = pTradeKey.DtTrade.GetHashCode();
            int hashSide = pTradeKey.Side.GetHashCode();
            int hashIdA_ExecBroker = pTradeKey.IdA_ExecBroker.GetHashCode();

            //Calcul du hash code pour le TradeInfo.
            return (int)(hashIdA_Dealer ^ hashIdB_Dealer ^ hashIdA_Clearer ^ hashIdB_Clearer ^ hashIdDC ^ hashIdAsset
                ^ hashDtBusiness ^ hashDtTrade ^ hashSide ^ hashIdA_ExecBroker);
        }
        #endregion IEqualityComparer

    }
    #endregion TradeInfoComparer
    #region TradeCandidate
    /// <summary>
    /// Données d'un trade candidat
    /// </summary>
    /// EG 20171016 [23509] Add DtExecution
    public class TradeCandidate : TradeKey
    {
        #region Accessors
        public int IdT{ get; set; }
        public string TradeIdentifier { set; get; }
        public TradeContext Context {get;set;}
        public DateTime DtTimestamp {get;set;}
        public DateTime DtExecution { get; set; }
        public PositionEffectEnum PositionEffect { get; set; }
        public decimal Price {get;set; }
        // EG 20150907 [21317] Add Accruedinterest
        public decimal AccruedInterest { get; set; }
        // EG 20150920 [21374] Int (int32) to Long (Int64) 
        // EG 20170127 Qty Long To Decimal
        public decimal Qty { get; set; }
        public int RemainderPriceLength
        {
            get
            {
                int length = 0;
                decimal remainder = Price - Math.Floor(Price);
                if (0 != remainder)
                    length = remainder.ToString("0.###########").Remove(0, 2).Length;
                return length;
            }
        }

        #endregion Accessors
        #region Constructors
        // EG 20150907 [21317] Add Accruedinterest
        /// EG 20171016 [23509] Add DtExecution
        // EG 20190308 DTEXECUTION instead of DTEXCECUTION
        public TradeCandidate() : base()
        {
        }
        public TradeCandidate(DataRow pRowTrade, TradeContext pContext):base(pRowTrade)
        {
            Context = pContext;
            IdT = Convert.ToInt32(pRowTrade["IDT"]);
            DtTimestamp = Convert.ToDateTime(pRowTrade["DTTIMESTAMP"]);
            DtExecution = Convert.ToDateTime(pRowTrade["DTEXECUTION"]);
            Price = Convert.ToDecimal(pRowTrade["PRICE"]);
            // EG 20150920 [21374] Int (int32) to Long (Int64) 
            // EG 20170127 Qty Long To Decimal
            Qty = Convert.ToDecimal(pRowTrade["QTY"]);
            PositionEffect = pContext.positionEffect;
            TradeIdentifier = pRowTrade["IDENTIFIER"].ToString();
            if (false == Convert.IsDBNull(pRowTrade["ACCRUEDINTEREST"]))
                AccruedInterest = Convert.ToDecimal(pRowTrade["ACCRUEDINTEREST"]);
        }
        // EG 20150907 [21317] Add Accruedinterest
        // EG 20171016 [23509] Add DtExecution
        public TradeCandidate(TradeCandidate pTradeCandidate)
            : base(pTradeCandidate)
        {
            Context = pTradeCandidate.Context;
            IdT = pTradeCandidate.IdT;
            DtTimestamp = pTradeCandidate.DtTimestamp;
            DtExecution = pTradeCandidate.DtExecution;
            Price = pTradeCandidate.Price;
            Qty = pTradeCandidate.Qty;
            PositionEffect = pTradeCandidate.PositionEffect;
            TradeIdentifier = pTradeCandidate.TradeIdentifier;
            AccruedInterest = pTradeCandidate.AccruedInterest;

        }
        #endregion Constructors
    }
    #endregion TradeCandidate

    #region TradeMergeContext
    /// <summary>
    /// Eléments de contextes de TradeMergeRule
    /// </summary>
    public class TradeMergeContext
    {
        #region Accessors
        public Nullable<TypePartyEnum> TypeParty {set;get;}
        public Nullable<int> Party { set; get; }
        public Nullable<TypeInstrEnum> TypeInstr { set; get; }
        public Nullable<int> Instr { set; get; }
        public Nullable<TypeContractEnum> TypeContract { set; get; }
        public Nullable<int> Contract { set; get; }
        public Nullable<PositionEffectEnum> PositionEffect { set; get; }
        public Nullable<PriceValueEnum> PriceValue { set; get; }

        #region PartyHasValue
        public bool PartyHasValue
        {
            get { return TypeParty.HasValue && Party.HasValue; }
        }
        #endregion PartyHasValue
        #region ContractHasValue
        public bool ContractHasValue
        {
            get { return TypeContract.HasValue && Contract.HasValue; }
        }
        #endregion ContractHasValue
        #region InstrHasValue
        public bool InstrHasValue
        {
            get { return TypeInstr.HasValue && Instr.HasValue; }
        }
        #endregion InstrSpecified

        #endregion Accessors
        #region Constructors
        public TradeMergeContext(DataRow pRowMergeRule)
        {
            // Dealer du trade (Groupe acteur / Acteur / Groupe Book / Book)
            if ((false == Convert.IsDBNull(pRowMergeRule["TYPEPARTY"])) && 
                Enum.IsDefined(typeof(TypePartyEnum), pRowMergeRule["TYPEPARTY"].ToString()))
                TypeParty = (TypePartyEnum)Enum.Parse(typeof(TypePartyEnum), pRowMergeRule["TYPEPARTY"].ToString(), true);

            if (false == Convert.IsDBNull(pRowMergeRule["IDPARTY"]))
                Party = Convert.ToInt32(pRowMergeRule["IDPARTY"]);

            // Groupe d'instruments - Instrument
            if ((false == Convert.IsDBNull(pRowMergeRule["TYPEINSTR"])) &&
                 Enum.IsDefined(typeof(TypeInstrEnum), pRowMergeRule["TYPEINSTR"].ToString()))
                TypeInstr = (TypeInstrEnum)Enum.Parse(typeof(TypeInstrEnum), pRowMergeRule["TYPEINSTR"].ToString(), true);
            if (false == Convert.IsDBNull(pRowMergeRule["IDINSTR"]))
                Instr = Convert.ToInt32(pRowMergeRule["IDINSTR"]);

            // Groupe marché / Marché / Groupe Contrat / Contrat
            if ((false == Convert.IsDBNull(pRowMergeRule["TYPECONTRACT"])) &&
                 Enum.IsDefined(typeof(TypeContractEnum), pRowMergeRule["TYPECONTRACT"].ToString()))
                TypeContract = (TypeContractEnum)Enum.Parse(typeof(TypeContractEnum), pRowMergeRule["TYPECONTRACT"].ToString(), true);
            if (false == Convert.IsDBNull(pRowMergeRule["IDCONTRACT"]))
                Contract = Convert.ToInt32(pRowMergeRule["IDCONTRACT"]);

            // PosEffect
            if ((false == Convert.IsDBNull(pRowMergeRule["POSITIONEFFECT"])) &&
                 Enum.IsDefined(typeof(PositionEffectEnum), pRowMergeRule["POSITIONEFFECT"].ToString()))
                PositionEffect = (PositionEffectEnum)Enum.Parse(typeof(PositionEffectEnum), pRowMergeRule["POSITIONEFFECT"].ToString(), true);

            // PriceValue
            if ((false == Convert.IsDBNull(pRowMergeRule["PRICEVALUE"])) &&
                    Enum.IsDefined(typeof(PriceValueEnum), pRowMergeRule["PRICEVALUE"].ToString()))
                PriceValue = (PriceValueEnum)Enum.Parse(typeof(PriceValueEnum), pRowMergeRule["PRICEVALUE"].ToString(), true);
        }
        #endregion Constructors
        #region Methods
        #region IsMatch

        /// FI 20170908 [23409] Modify
        public MatchingEnum IsMatch(TradeMergeContextEnum pMergeContextEnum, TradeContext pTradeContext)
        {
            MatchingEnum match = MatchingEnum.Ignore;
            switch (pMergeContextEnum)
            {
                case TradeMergeContextEnum.IdGrpActor:
                    #region Groupe Actor
                    if (PartyHasValue)
                    {
                        if (TypePartyEnum.GrpActor == TypeParty)
                            match = (Party == pTradeContext.idGrpActor) ? MatchingEnum.HiMatch : MatchingEnum.UnMatch;
                    }
                    else
                        match = MatchingEnum.LoMatch;
                    #endregion Groupe Actor
                    break;
                case TradeMergeContextEnum.IdA:
                    #region Party
                    if (PartyHasValue)
                    {
                        if (TypePartyEnum.Actor == TypeParty)
                            match = (Party == pTradeContext.idA) ? MatchingEnum.HiMatch : MatchingEnum.UnMatch;
                    }
                    else
                        match = MatchingEnum.LoMatch;
                    #endregion Party
                    break;
                case TradeMergeContextEnum.IdGrpBook:
                    #region Groupe Book
                    if (PartyHasValue)
                    {
                        if (TypePartyEnum.GrpBook == TypeParty)
                            match = (Party == pTradeContext.idGrpBook) ? MatchingEnum.HiMatch : MatchingEnum.UnMatch;
                    }
                    else
                        match = MatchingEnum.LoMatch;
                    #endregion Groupe Book
                    break;
                case TradeMergeContextEnum.IdB:
                    #region Book
                    if (PartyHasValue)
                    {
                        if (TypePartyEnum.Book == TypeParty)
                            match = (Party == pTradeContext.idB) ? MatchingEnum.HiMatch : MatchingEnum.UnMatch;
                    }
                    else
                        match = MatchingEnum.LoMatch;
                    #endregion Book
                    break;
                case TradeMergeContextEnum.IdGrpInstr:
                    #region Groupe Instrument
                    if (InstrHasValue)
                    {
                        if (TypeInstrEnum.GrpInstr == TypeInstr)
                            match = (Instr == pTradeContext.idGrpInstr) ? MatchingEnum.HiMatch : MatchingEnum.UnMatch;
                    }
                    else
                        match = MatchingEnum.LoMatch;
                    #endregion Groupe Instrument
                    break;
                case TradeMergeContextEnum.IdI:
                    #region Instrument
                    if (InstrHasValue)
                    {
                        if (TypeInstrEnum.Instr == TypeInstr)
                            match = (Instr == pTradeContext.idI) ? MatchingEnum.HiMatch : MatchingEnum.UnMatch;
                    }
                    else
                        match = MatchingEnum.LoMatch;
                    #endregion Instrument
                    break;
                case TradeMergeContextEnum.IdGrpMarket:
                    #region Groupe Market
                    if (ContractHasValue)
                    {
                        if (TypeContractEnum.GrpMarket == TypeContract)
                            match = (Contract == pTradeContext.idGrpMarket) ? MatchingEnum.HiMatch : MatchingEnum.UnMatch;
                    }
                    else
                        match = MatchingEnum.LoMatch;
                    #endregion Groupe Market
                    break;
                case TradeMergeContextEnum.IdM:
                    #region Market
                    if (ContractHasValue)
                    {
                        if (TypeContractEnum.Market == TypeContract)
                            match = (Contract == pTradeContext.idM) ? MatchingEnum.HiMatch : MatchingEnum.UnMatch;
                    }
                    else
                        match = MatchingEnum.LoMatch;
                    #endregion Market
                    break;
                case TradeMergeContextEnum.IdGrpContract:
                    #region Groupe Contract
                    if (ContractHasValue)
                    {
                        if (TypeContractEnum.GrpContract == TypeContract)
                            match = (Contract == pTradeContext.idGrpContract) ? MatchingEnum.HiMatch : MatchingEnum.UnMatch;
                    }
                    else
                        match = MatchingEnum.LoMatch;
                    #endregion Groupe Contract
                    break;
                case TradeMergeContextEnum.IdContract:
                    #region Contract
                    if (ContractHasValue)
                    {
                        //if (TypeContractEnum.Contract == TypeContract) // FI 20170908 [23409] use DerivativeContract
                        if (TypeContractEnum.DerivativeContract == TypeContract)
                            match = (Contract == pTradeContext.idContract) ? MatchingEnum.HiMatch : MatchingEnum.UnMatch;
                    }
                    else
                        match = MatchingEnum.LoMatch;
                    #endregion Contract
                    break;
                case TradeMergeContextEnum.PositionEffect:
                    #region Position Effect
                    if (PositionEffect.HasValue)
                    {
                        if (PositionEffect.Value == pTradeContext.positionEffect)
                            match = MatchingEnum.HiMatch;
                        else if (PositionEffect.Value == PositionEffectEnum.Close)
                        {
                            switch (pTradeContext.positionEffect)
                            {
                                case PositionEffectEnum.FIFO:
                                case PositionEffectEnum.FIFO_ITD:
                                case PositionEffectEnum.HILO:
                                case PositionEffectEnum.LIFO:
                                    match = MatchingEnum.HiMatch;
                                    break;
                                default:
                                    match = MatchingEnum.UnMatch;
                                    break;
                            }
                        }
                    }
                    else
                        match = MatchingEnum.LoMatch;
                    #endregion PosEffect
                    break;
                case TradeMergeContextEnum.PriceValue:
                    #region Price
                    if (PriceValue.HasValue  && PriceValue.Value == PriceValueEnum.IDENTICAL)
                        match = MatchingEnum.HiMatch;
                    else
                        match = MatchingEnum.LoMatch;
                    #endregion Price
                    break;

            }
            return match;
        }
        #endregion IsMatch
        #endregion Methods
    }
    #endregion TradeMergeContext
    #region TradeMergeContextComparer
    /// <summary>
    /// Comparer de l'application des poids de contexte de merge (via ResultMatching)
    /// </summary>
    public class TradeMergeContextComparer : IComparer<TradeMergeRule>
    {
        public int Compare(TradeMergeRule pTradeMergeRule1, TradeMergeRule pTradeMergeRule2)
        {
            return pTradeMergeRule1.ResultMatching.CompareTo(pTradeMergeRule2.ResultMatching);
        }
    }
    #endregion TradeMergeContextComparer
    #region TradeMergeContextEnum
    /// <summary>
    /// Liste des éléments de trades pour marchage avec contexte
    /// </summary>
    public enum TradeMergeContextEnum
    {
        #region Members
        IdA,
        IdGrpActor,
        IdB,
        IdGrpBook,
        IdI,
        IdGrpInstr,
        IdM,
        IdGrpMarket,
        IdContract,
        IdGrpContract,
        PositionEffect,
        PriceValue,
        None,
        #endregion Members
    }
    #endregion TradeMergeContextEnum

    #region TradeContext
    /// <summary>
    /// Poids d'un contexte TRADEMERGE : Du plus fort au plus faible
    /// 12. BOOK
    /// 11. ACTOR
    /// 10. GRPBOOK
    /// 09. GRPACTOR
    /// 
    /// 08. CONTRACT
    /// 07. GRPCONTRACT
    /// 06. MARKET
    /// 05. GRPMARKET
    /// 04. INSTR
    /// 03. GRPINSTR
    /// 02. POSEFFECT
    /// 01. PRICE
    /// </summary>
    public class TradeContext
    {
        #region Members
        [TradeMergeContextWeight(Weight = 11, Name = TradeMergeContextEnum.IdA)]
        public int idA;
        [TradeMergeContextWeight(Weight = 9, Name = TradeMergeContextEnum.IdGrpActor)]
        public Nullable<int> idGrpActor;
        [TradeMergeContextWeight(Weight = 12, Name = TradeMergeContextEnum.IdB)]
        public int idB;
        [TradeMergeContextWeight(Weight = 10, Name = TradeMergeContextEnum.IdGrpBook)]
        public Nullable<int> idGrpBook;
        [TradeMergeContextWeight(Weight = 4, Name = TradeMergeContextEnum.IdI)]
        public int idI;
        [TradeMergeContextWeight(Weight = 3, Name = TradeMergeContextEnum.IdGrpInstr)]
        public Nullable<int> idGrpInstr;
        [TradeMergeContextWeight(Weight = 6, Name = TradeMergeContextEnum.IdM)]
        public int idM;
        [TradeMergeContextWeight(Weight = 5, Name = TradeMergeContextEnum.IdGrpMarket)]
        public Nullable<int> idGrpMarket;
        [TradeMergeContextWeight(Weight = 8, Name = TradeMergeContextEnum.IdContract)]
        public int idContract;
        [TradeMergeContextWeight(Weight = 7, Name = TradeMergeContextEnum.IdGrpContract)]
        public Nullable<int> idGrpContract;
        [TradeMergeContextWeight(Weight = 2, Name = TradeMergeContextEnum.PositionEffect)]
        public PositionEffectEnum positionEffect;
        [TradeMergeContextWeight(Weight = 1, Name = TradeMergeContextEnum.PriceValue)]
        public Nullable<PriceValueEnum> priceValue;
        public decimal price;
        #endregion Members
        #region Accessors
        #endregion Accessors
        #region Constructors
        public TradeContext(DataRow pRowTrade)
        {
            // idA_Dealer
            if (false == Convert.IsDBNull(pRowTrade["IDA_DEALER"]))
                idA = Convert.ToInt32(pRowTrade["IDA_DEALER"]);
            // Groupe Actor Dealer
            if (false == Convert.IsDBNull(pRowTrade["IDGACTOR"]))
                idGrpActor = Convert.ToInt32(pRowTrade["IDGACTOR"]);
            // idB_Dealer
            if (false == Convert.IsDBNull(pRowTrade["IDB_DEALER"]))
                idB = Convert.ToInt32(pRowTrade["IDB_DEALER"]);
            // Groupe Book Dealer
            if (false == Convert.IsDBNull(pRowTrade["IDGBOOK"]))
                idGrpBook = Convert.ToInt32(pRowTrade["IDGBOOK"]);
            // Instrument
            if (false == Convert.IsDBNull(pRowTrade["IDI"]))
                idI = Convert.ToInt32(pRowTrade["IDI"]);
            // Groupe Instrument
            if (false == Convert.IsDBNull(pRowTrade["IDGINSTR"]))
                idGrpInstr = Convert.ToInt32(pRowTrade["IDGINSTR"]);
            // Market
            if (false == Convert.IsDBNull(pRowTrade["IDM"]))
                idM = Convert.ToInt32(pRowTrade["IDM"].ToString());
            // Groupe Market
            if (false == Convert.IsDBNull(pRowTrade["IDGMARKET"]))
                idGrpMarket = Convert.ToInt32(pRowTrade["IDGMARKET"]);
            // Contract
            // EG 20150723 Merge OTC
            if (pRowTrade.Table.Columns.Contains("IDDC") && (false == Convert.IsDBNull(pRowTrade["IDDC"])))
                idContract = Convert.ToInt32(pRowTrade["IDDC"]);
            // Groupe Contract
            // EG 20150723 Merge OTC
            if (pRowTrade.Table.Columns.Contains("IDGCONTRACT") && (false == Convert.IsDBNull(pRowTrade["IDGCONTRACT"])))
                idGrpContract = Convert.ToInt32(pRowTrade["IDGCONTRACT"]);
            // PosEffect
            if (false == Convert.IsDBNull(pRowTrade["POSITIONEFFECT"]))
                positionEffect = (PositionEffectEnum)ReflectionTools.EnumParse(new PositionEffectEnum(), pRowTrade["POSITIONEFFECT"].ToString());
            // Price
            price = Convert.ToDecimal(pRowTrade["PRICE"]);
        }
        #endregion Constructors
    }
    #endregion TradeContext
    #region TradeMergeContextWeight
    /// <summary>
    /// Poids des contextes de merge
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Class)]
    public sealed class TradeMergeContextWeight : Attribute
    {
        #region Accessors
        public TradeMergeContextEnum Name {get;set;}
        public int Weight {get;set;}
        #endregion Accessors
    }
    #endregion TradeMergeContextWeight

    #region TradeMergeRule
    /// <summary>
    /// Données du référentiel TRADEMERGERULE : Contextes du MERGE
    /// </summary>
    public class TradeMergeRule
    {
        #region Accessors
        public int IdTradeMergeRule { get; set; }
        public string Identifier { get; set; }
        public string DisplayName { get; set; }

        // Contexte environnemental
        public TradeMergeContext Context { get; set; }
        // IDT des trades potentiellement candidats à MERGE
        public List<int> IdT_Candidate { get; set; }
        // IDT des trades potentiellement candidats à MERGE regroupé par Clé unique (voir classe TradeKey)
        // TradeKey = Clé unique par un merge
        // List<Pair<int,decimal>>> = Liste des trades de même clé avec <int,decimal> = <idT,price>
        public Dictionary<TradeKey, List<Pair<int, decimal>>> IdT_Mergeable { get; set; }
        // Résultat du matching (1er niveau, 2ème niveau) 
        public double ResultMatching {get;set;}
        #endregion Accessors
        #region Constructors
        public TradeMergeRule(DataRow pRowMergeRule)
        {
            IdTradeMergeRule = Convert.ToInt32(pRowMergeRule["IDTRADEMERGERULE"]);
            Identifier = pRowMergeRule["IDENTIFIER"].ToString();
            DisplayName = pRowMergeRule["DISPLAYNAME"].ToString();
            Context = new TradeMergeContext(pRowMergeRule);
        }
        #endregion Constructors
        #region Methods
        #region Match
        public MatchingEnum Match(TradeMergeContextEnum pContext, int pWeight, TradeContext pTradeContext)
        {
            MatchingEnum match = Context.IsMatch(pContext, pTradeContext);
            if (MatchingEnum.HiMatch == match)
                ResultMatching += Math.Pow(Convert.ToDouble(2), Convert.ToDouble(pWeight));
            else if (MatchingEnum.UnMatch == match)
                ResultMatching = 0;
            return match;
        }
        #endregion Match
        #region SetTradeMergable
        /// <summary>
        /// Assemblage des trades candidats entre eux en fonction de leur clé de merge
        /// Clé de merge = IDA_DEALER, IDB_DEALER, IDA_CLEARER, IDB_CLEARER, IDASSET, DTBUSINESS, DTTRADE, SIDE, IDA_EXECBROKER
        /// </summary>
        /// <param name="pDicTradeCandidate">Dictionnaire des trades candidats à merge (pour l'ensemble des contextes)</param>
        /// <param name="pLstTradeKeyGroup">Liste des "clé unique de merge" rencontrées dans les trade candidats</param>
        public void SetTradeMergable(Dictionary<int, TradeCandidate> pDicTradeCandidate, List<IGrouping<TradeKey, TradeKey>> pLstTradeKeyGroup)
        {
            IdT_Candidate.ForEach(trade => 
            {
                TradeCandidate tradeCandidate = pDicTradeCandidate[trade];
                // Pair (idt,price)
                Pair<int, decimal> _idT = new Pair<int, decimal>(trade, tradeCandidate.Price);
                pLstTradeKeyGroup.ForEach(tkg =>
                {
                    // On continue le traitement si il y a plus d'un trade qui compose la clé de merge (RESULTAT GROUP BY D'ORIGINE);
                    if (1 < tkg.Count())
                    {
                        // La clé de merge correspond
                        if (new TradeKeyComparer().Equals(tradeCandidate, tkg.Key))
                        {
                            if (null == IdT_Mergeable)
                                IdT_Mergeable = new Dictionary<TradeKey, List<Pair<int, decimal>>>();
                            if (false == IdT_Mergeable.ContainsKey(tkg.Key))
                            {
                                List<Pair<int, decimal>> _lst = new List<Pair<int, decimal>>
                                {
                                    _idT
                                };
                                IdT_Mergeable.Add(tkg.Key, _lst);
                            }
                            else 
                            {
                                IdT_Mergeable[tkg.Key].Add(_idT);
                            }
                        }
                    }
                });
            });
        }
        #endregion SetTradeMergable
        #region Weighting
        public void Weighting(TradeContext pTradeContext)
        {
            
            ResultMatching = 1;
            int _weight = 0;
            TradeMergeContextEnum _name = TradeMergeContextEnum.None;
            List<FieldInfo> flds = pTradeContext.GetType().GetFields().ToList();
            bool _isMatch = true;
            flds.ForEach(fld => {
                if (_isMatch)
                {
                    object[] attributes = fld.GetCustomAttributes(typeof(TradeMergeContextWeight), false);
                    if (0 != attributes.GetLength(0))
                    {
                        _weight = ((TradeMergeContextWeight)attributes[0]).Weight;
                        _name = ((TradeMergeContextWeight)attributes[0]).Name;
                        if (MatchingEnum.UnMatch == Match(_name, _weight, pTradeContext))
                            _isMatch = false;
                    }
                }
            });
        }
        #endregion Weighting
        #endregion Methods
    }
    #endregion TradeMergeRules

}
