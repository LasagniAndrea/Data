#region Using Directives
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
//
using EFS.ACommon;
using EFS.ApplicationBlocks.Data;
using EFS.Common;
using EFS.Common.Log;
using EFS.Common.MQueue;
using EFS.LoggerClient;
using EFS.LoggerClient.LoggerService;
using EFS.Process;
using EFS.SpheresService;
using EFS.TradeLink;
using EFS.Tuning;
//
using EfsML;
using EfsML.Business;
using EfsML.Enum;
using EfsML.Enum.Tools;
using EfsML.Interface;
using EfsML.v30.PosRequest;
//
using FixML.Interface;
//
using FpML.Enum;
using FpML.Interface;
#endregion Using Directives

namespace EFS.Process
{

    public enum NextPreviousEnum
    {
        Next,
        Previous,
        None
    }

    #region ETDQuoteInfo
    /// <summary>
    /// Détermination de :
    /// 1. la date de traitement
    /// 2. la date de lecture du cours en fonction de la date de traitement
    /// 3. la date de lecture du cours veille en fonction de la date de traitement
    /// </summary>
    public class ETDQuoteInfo : QuoteInfo, ICloneable
    {
        #region Members
        #endregion Members
        #region Constructors
        public ETDQuoteInfo() { }
        public ETDQuoteInfo(EventsValProcessETD pProcess, NextPreviousEnum pType) : this(pProcess, pType, pProcess.ETDContainer.ClearingBusinessDate.Date) { }
        public ETDQuoteInfo(EventsValProcessETD pProcess, NextPreviousEnum pType, Nullable<DateTime> pDate):base(pProcess, pType, pDate)
        {
            // Détermination de la date de lecture du cours
            PosKeepingAsset_ETD posKeepingAsset = pProcess.ETDContainer.ProductBase.CreatePosKeepingAsset(Cst.UnderlyingAsset.ExchangeTradedContract) as PosKeepingAsset_ETD;
            posKeepingAsset.idAsset = pProcess.ETDContainer.AssetETD.IdAsset;
            posKeepingAsset.identifier = pProcess.ETDContainer.AssetETD.Identifier;
            posKeepingAsset.maturityDate = pProcess.ETDContainer.AssetETD.Maturity_MaturityDate;
            posKeepingAsset.maturityDateSys = pProcess.ETDContainer.AssetETD.Maturity_MaturityDateSys;
            dtQuote = posKeepingAsset.GetOfficialCloseQuoteTime(dtBusiness);
        }
        #endregion Constructors
        #region Methods
        #region Clone
        public object Clone()
        {
            ETDQuoteInfo clone = new ETDQuoteInfo
            {
                dtBusiness = this.dtBusiness,
                dtQuote = this.dtQuote,
                processCacheContainer = this.processCacheContainer,
                rowState = this.rowState
            };
            return clone;
        }
        #endregion Clone
        #endregion Methods
    }
    #endregion ETDQuoteInfo

    #region EventsValProcessETD
    /// <summary>
    /// 
    /// </summary>
    //PM 20140509 [19970][19259] Prise en compte du coefficient de la devise du prix du contrat dans la contract multiplier lorsque celle-ci n'est pas la devise cotée.
    // EG 20150706 [21021] Nullable<int> for _bookBuyer|_bookSeller
    public partial class EventsValProcessETD : EventsValProcessBase
    {
        #region Members
        private readonly CommonValParameters m_Parameters;
        private ExchangeTradedDerivativeContainer m_ExchangeTradedDerivative;

        // EG 20170324 use m_Buyer|m_BookBuyer|m_Seller|m_BookSeller|m_IsPosKeeping_BookDealer
        //private IParty _buyer;
        //private Nullable<int> _bookBuyer;
        //private IParty _seller;
        //private Nullable<int> m_BookSeller;
        //private bool _isPosKeeping_BookDealer;
        private bool _isCashFlowsVal;
        private bool _isAssetMaster;

        // EntityMarket
        // EG 20170324 use m_EntityMarketInfo
        //private IPosKeepingMarket _entityMarketInfo;
        //Représente la quantité disponible jour
        // EG 20150920 [21374] Int (int32) to Long (Int64) 
        // EG 20170127 Qty Long To Decimal Declare in EventsValProcessBase
        //private decimal _posAvailableQuantity;
        //Représente la quantité disponible veille
        // EG 20150920 [21374] Int (int32) to Long (Int64) 
        // EG 20170127 Qty Long To Decimal Declare  in EventsValProcessBase
        //private decimal _posAvailableQuantityPrev;
        //Représente la quantité disponible : 
        //quantité disponible veille +/- celles des actions de correction du jour (Correction - POC , Transfert - POT , Décompensation - UNCLEARING)
        // EG 20150920 [21374] Int (int32) to Long (Int64) 
        // EG 20170127 Qty Long To Decimal Declqre in EventsValProcessBase
        //private decimal _posQuantityPrevAndActionsDay;
        #endregion Members
        #region Accessors
        // EG 20170324 use m_EntityMarketInfo
        //public override IPosKeepingMarket EntityMarketInfo
        //{
        //    get { return _entityMarketInfo; }
        //}
        private Quote_ETDAsset Quote_ETDAsset
        {
            get { return m_Quote as Quote_ETDAsset; }
        }

        /// <summary>
        /// Obtient le ContractMultiplier en detant compte du factor lorsque la devise du prix n'est pas une devise cotée
        /// <para>Obtenu à partir des caractéristiques du prix s'il est renseigné ou obtenu à partir des caractéristiques de l'asset
        /// </summary>
        //PM 20140509 [19970][19259] Utilisation de ContractMultiplierQuotedCurrency au lieu de ContractMultiplier,
        // et appliquation du factor de la devise du prix du contrat lorsque cette devise n’est pas la devise cotée.
        private decimal ContractMultiplier
        {
            get
            {
                //PM 20140509 [19970][19259] Utilisation de ContractMultiplierQuotedCurrency au lieu de ContractMultiplier
                //decimal ret = m_ExchangeTradedDerivative.ContractMultiplier;
                decimal ret = m_ExchangeTradedDerivative.ContractMultiplierQuotedCurrency;
                if ((null != Quote_ETDAsset) && Quote_ETDAsset.contractMultiplierSpecified)
                {
                    ret = Quote_ETDAsset.contractMultiplier;
                    //PM 20140509 [19970][19259] Appliquation du factor de la devise du prix du contrat lorsque cette devise n’est pas la devise cotée.
                    if (m_ExchangeTradedDerivative.PriceCurrency != m_ExchangeTradedDerivative.PriceQuotedCurrency)
                    {
                        ret /= m_ExchangeTradedDerivative.PriceCurrencyFactor;
                    }
                }
                return ret;
            }
        }
        /// <summary>
        /// Obtient le ContractMultiplier 
        /// <para>Obtenu à partir des caractéristiques du prix s'il est renseigné ou obtenu à partir des caractéristiques de l'asset
        /// </summary>
        //PM 20140512 [19970][19259] Ajouté : correspond à l'ancien méthode "contractMultiplier" renommé en "originalContractMultiplier"
        private decimal OriginalContractMultiplier
        {
            get
            {
                decimal ret = m_ExchangeTradedDerivative.ContractMultiplier;
                if ((null != Quote_ETDAsset) && Quote_ETDAsset.contractMultiplierSpecified)
                {
                    ret = Quote_ETDAsset.contractMultiplier;
                }
                return ret;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public override CommonValParameters Parameters
        {
            get { return m_Parameters; }
        }

        /// <summary>
        /// Obtient la quantité disponible en date de cotation - 1 (Date veille)
        /// </summary>
        // EG 20150920 [21374] Int (int32) to Long (Int64)  
        // EG 20170127 Qty Long To Decimal Declare in EventsValProcessBase
        //private decimal PosAvailableQuantityPrev
        //{
        //    get { return m_PosAvailableQuantityPrev; }
        //}
        /// <summary>
        /// Obtient la quantité disponible en date de cotation - 1 +/- celles des actions de correction du jour 
        /// Correction de position (POC) (-)
        /// Transfert (POT) (-)
        /// Unclearing (UNCLEARING)(+)
        /// </summary>
        // EG 20150920 [21374] Int (int32) to Long (Int64) 
        // EG 20170127 Qty Long To Decimal Declare in EventsValProcessBase
        //private decimal PosQuantityPrevAndActionsDay
        //{
        //    get { return m_PosQuantityPrevAndActionsDay; }
        //}

        public ExchangeTradedDerivativeContainer ETDContainer
        {
            get { return m_ExchangeTradedDerivative; }
        }
        #endregion Accessors
        #region Constructors
        public EventsValProcessETD(EventsValProcess pProcess, DataSetTrade pDsTrade, EFS_TradeLibrary pTradeLibrary, IProduct pProduct)
            : base(pProcess, pDsTrade, pTradeLibrary, pProduct)
        {
            m_Parameters = new CommonValParametersETD();
            if (ProcessBase.ProcessCallEnum.Master == pProcess.ProcessCall)
                pProcess.ProcessCacheContainer = new ProcessCacheContainer(pProcess.Cs, (IProductBase)pProduct);
        }
        #endregion Constructors
        #region Methods
        #region EndOfInitialize
        // EG 20180502 Analyse du code Correction [CA2214]
        public override void EndOfInitialize()
        {
            if (false == m_tradeLibrary.DataDocument.CurrentProduct.IsStrategy)
            {
                Initialize();
                InitializeDataSetEvent();
            }
        }
        #endregion EndOfInitialize
        #region InitializeDataSetEvent
        /// <summary>
        /// Cette méthode override la méthode virtuelle pour le traitement EOD
        /// Dans ce cas 
        /// 1./ Le nombre de tables EVENTXXX chargées est réduit : EVENT|EVENTCLASS|EVENTDET|EVENTASSET
        /// 2./ La date DTBUSINESS est utilisé pour restreindre le nombre d'EVTS chargé
        /// tels que DtBusiness between DTSTARTADJ and DTENDADJ
        /// </summary>
        // EG 20150612 [20665] Refactoring : Chargement DataSetEventTrade
        // EG 20150617 [20665]
        public override void InitializeDataSetEvent()
        {
            m_DsEvents = new DataSetEventTrade(m_CS, SlaveDbTransaction,  ParamIdT);
            Nullable<DateTime> dtBusiness = null;
            if (IsEndOfDayProcess)
                dtBusiness = m_Quote.time;
            // EG 20150617 [20665]
            m_DsEvents.Load(EventTableEnum.Class | EventTableEnum.Detail | EventTableEnum.Asset, dtBusiness, null);
        }
        #endregion InitializeDataSetEvent

        /// <summary>
        /// Construction de la requête permettant d'obtenir les informations sur l'asset sur lequel à eu lieu un cascading
        /// à partir de l'asset résultant du cascading. Et execute cette requête pour retourner le DataReader correspondant.
        /// </summary>
        /// <param name="pIdAsset">Id de l'asset post cascading</param>
        /// <param name="pDtBusiness">Date de bourse du cascading</param>
        /// <returns>Le DataReader correspondant à l'execution de la requête (Attention, penser à le fermer)</returns>
        // PM 20130220 [18414]
        private IDataReader GetAssetBeforeCascading(DateTime pDtBusiness, int pIdT)
        {
            StrBuilder sqlSelect = new StrBuilder();
            DataParameters parameters = new DataParameters();

            parameters.Add(new DataParameter(this.Process.Cs, "DTBUSINESS", DbType.DateTime), pDtBusiness);
            parameters.Add(new DataParameter(this.Process.Cs, "IDT", DbType.Int32), pIdT);

            sqlSelect += SQLCst.SELECT + "ast.IDASSET as IDASSET, ast.IDENTIFIER as IDENTIFIER, ast.CONTRACTMULTIPLIER as CONTRACTMULTIPLIER" + Cst.CrLf;
            sqlSelect += SQLCst.FROM_DBO + Cst.OTCml_TBL.TRADELINK + " tl" + Cst.CrLf;
            sqlSelect += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.EVENT + " ev" + Cst.CrLf;
            sqlSelect += SQLCst.ON + "(ev.IDT = tl.IDT_B)" + Cst.CrLf;
            sqlSelect += SQLCst.AND + "(ev.EVENTCODE = " + DataHelper.SQLString(EventCodeFunc.Cascading) + ")" + Cst.CrLf;
            sqlSelect += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.EVENTCLASS + " ec" + Cst.CrLf;
            sqlSelect += SQLCst.ON + "(ec.IDE = ev.IDE)" + Cst.CrLf;
            sqlSelect += SQLCst.AND + "(ec.EVENTCLASS = " + DataHelper.SQLString(EventClassFunc.GroupLevel) + ")" + Cst.CrLf;
            sqlSelect += SQLCst.AND + "(ec.DTEVENT = @DTBUSINESS)" + Cst.CrLf;
            sqlSelect += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.EVENT + " evroot" + Cst.CrLf;
            sqlSelect += SQLCst.ON + "(evroot.IDE = ev.IDE_EVENT)" + Cst.CrLf;
            sqlSelect += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.EVENTASSET + " ea" + Cst.CrLf;
            sqlSelect += SQLCst.ON + "(ea.IDE = evroot.IDE)" + Cst.CrLf;
            sqlSelect += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.VW_ASSET_ETD_EXPANDED + " ast";
            sqlSelect += SQLCst.ON + "(ast.IDASSET = ea.IDASSET)" + Cst.CrLf;
            sqlSelect += SQLCst.WHERE + "(tl.IDT_A = @IDT)" + Cst.CrLf;
            sqlSelect += SQLCst.AND + "(tl.LINK = " + DataHelper.SQLString(TradeLinkType.PositionAfterCascading.ToString()) + ")";
            //
            IDataReader dr = DataHelper.ExecuteReader(Process.Cs, CommandType.Text, sqlSelect.ToString(), parameters.GetArrayDbParameter());
            return dr;
        }

        /// <summary>
        /// Recalcul la valeur de tous les Events {pEventCode}/{pEventType} du jour
        /// <para>Uniquement si le contractMultiplier a changé</para>
        /// </summary>
        /// <param name="pDbTransaction"></param>
        /// <param name="pEventCode"></param>
        /// <param name="pEventType"></param>
        private void CashFlowAmountGen(IDbTransaction pDbTransaction, string pEventCode, string pEventType)
        {
            decimal cashFlowAmount = 0;
            DataRow[] rowsEvent = GetTodayOrFutureEvent(pEventCode, pEventType);

            if (ArrFunc.IsFilled(rowsEvent))
            {
                foreach (DataRow rowEvent in rowsEvent)
                {
                    DataRow rowEventDet = GetRowEventDetail(Convert.ToInt32(rowEvent["IDE"]));
                    // 
                    decimal eventDetContractMultiplier = (Convert.IsDBNull(rowEventDet["CONTRACTMULTIPLIER"]) ? 0 : Convert.ToDecimal(rowEventDet["CONTRACTMULTIPLIER"]));
                    //
                    // Uniquemet si le contractMultiplier a changé
                    //PM 20140512 [19970][19259] La comparraison se fait avec le contract multiplier tel qu'il est paramètré sans modification dû à la devise du prix
                    //isEventToUpdate = (eventDetContractMultiplier != contractMultiplier);
                    bool isEventToUpdate = (eventDetContractMultiplier != OriginalContractMultiplier);
                    //
                    if (isEventToUpdate)
                    {
                        bool isTradeDay = (m_ExchangeTradedDerivative.ClearingBusinessDate.Date == Quote_ETDAsset.time.Date);
                        //
                        int idE = Convert.ToInt32(rowEvent["IDE"]);
                        decimal eventDetPrice100 = (Convert.IsDBNull(rowEventDet["PRICE100"]) ? 0 : Convert.ToDecimal(rowEventDet["PRICE100"]));
                        decimal eventDetQuote100 = (Convert.IsDBNull(rowEventDet["QUOTEPRICE100"]) ? 0 : Convert.ToDecimal(rowEventDet["QUOTEPRICE100"]));
                        // EG 20150920 [21374] Int (int32) to Long (Int64)
                        // EG 20170127 Qty Long To Decimal
                        decimal eventDetPosQuantityUSED = (Convert.IsDBNull(rowEventDet["DAILYQUANTITY"]) ? 0 : Convert.ToDecimal(rowEventDet["DAILYQUANTITY"]));
                        //        
                        if (EventTypeFunc.IsUnrealizedMargin(pEventType))
                        {
                            //PM 20140807 [20273][20106] Calcul unitaire arrondie avant de multiplier par la quantité
                            //cashFlowAmount = (eventDetQuote100 - eventDetPrice100) * eventDetPosQuantityUSED * contractMultiplier;
                            //PM 20150129 [20737][20754] Modification du calcul unitaire
                            //cashFlowAmount = m_ExchangeTradedDerivative.CashFlowValorization(eventDetQuote100 - eventDetPrice100, contractMultiplier, eventDetPosQuantityUSED);
                            cashFlowAmount = m_ExchangeTradedDerivative.CashFlowValorization(eventDetQuote100, eventDetPrice100, ContractMultiplier, eventDetPosQuantityUSED);
                        }
                        else if (EventTypeFunc.IsLiquidationOptionValue(pEventType))
                        {
                            //PM 20140807 [20273][20106] Calcul unitaire arrondie avant de multiplier par la quantité
                            //cashFlowAmount = eventDetQuote100 * eventDetPosQuantityUSED * contractMultiplier;
                            //PM 20150129 [20737][20754] Modification du calcul unitaire
                            //cashFlowAmount = m_ExchangeTradedDerivative.CashFlowValorization(eventDetQuote100, contractMultiplier, eventDetPosQuantityUSED);
                            cashFlowAmount = m_ExchangeTradedDerivative.CashFlowValorization(eventDetQuote100, 0, ContractMultiplier, eventDetPosQuantityUSED);
                        }
                        else if (EventTypeFunc.IsVariationMargin(pEventType))
                        {
                            // PM 20130221 [18414] Jamais avec le prix de négo pour les trades post cascading
                            // EG 20130610 Add Test m_ExchangeTradedDerivative.isTradeCAAdjusted
                            if (isTradeDay
                                && (false == m_ExchangeTradedDerivative.IsCascading)
                                && (false == m_ExchangeTradedDerivative.IsTradeCAAdjusted)
                                && (false == m_ExchangeTradedDerivative.IsPositionOpening))
                            {
                                //PM 20140807 [20273][20106] Calcul unitaire arrondie avant de multiplier par la quantité
                                //cashFlowAmount = (eventDetQuote100 - eventDetPrice100) * eventDetPosQuantityUSED * contractMultiplier;
                                //PM 20150129 [20737][20754] Modification du calcul unitaire
                                //cashFlowAmount = m_ExchangeTradedDerivative.CashFlowValorization(eventDetQuote100 - eventDetPrice100, contractMultiplier, eventDetPosQuantityUSED);
                                cashFlowAmount = m_ExchangeTradedDerivative.CashFlowValorization(eventDetQuote100, eventDetPrice100, ContractMultiplier, eventDetPosQuantityUSED);
                            }
                            else
                            {
                                decimal dailySettlementPriceVEILLE100 = Convert.ToDecimal(rowEventDet["QUOTEPRICEYEST100"]);
                                //PM 20140807 [20273][20106] Calcul unitaire arrondie avant de multiplier par la quantité
                                //cashFlowAmount = (eventDetQuote100 - dailySettlementPriceVEILLE100) * eventDetPosQuantityUSED * contractMultiplier;
                                //PM 20150129 [20737][20754] Modification du calcul unitaire
                                //cashFlowAmount = m_ExchangeTradedDerivative.CashFlowValorization(eventDetQuote100 - dailySettlementPriceVEILLE100, contractMultiplier, eventDetPosQuantityUSED);
                                cashFlowAmount = m_ExchangeTradedDerivative.CashFlowValorization(eventDetQuote100, dailySettlementPriceVEILLE100, ContractMultiplier, eventDetPosQuantityUSED);
                            }
                        }
                        else if (EventTypeFunc.IsRealizedMargin(pEventType))
                        {
                            decimal closingPrice100 = (Convert.IsDBNull(rowEventDet["CLOSINGPRICE100"]) ? 0 : Convert.ToDecimal(rowEventDet["CLOSINGPRICE100"]));
                            //PM 20140807 [20273][20106] Calcul unitaire arrondie avant de multiplier par la quantité
                            //cashFlowAmount = (eventDetPrice100 - closingPrice100) * eventDetPosQuantityUSED * contractMultiplier;
                            //PM 20150129 [20737][20754] Modification du calcul unitaire
                            //cashFlowAmount = m_ExchangeTradedDerivative.CashFlowValorization(eventDetPrice100 - closingPrice100, contractMultiplier, eventDetPosQuantityUSED);
                            cashFlowAmount = m_ExchangeTradedDerivative.CashFlowValorization(eventDetPrice100, closingPrice100, ContractMultiplier, eventDetPosQuantityUSED);
                        }
                        else if (EventTypeFunc.IsPremium(pEventType))
                        {
                            // Uniquement pour le Trade du jour
                            isEventToUpdate = isTradeDay;
                            //
                            if (isEventToUpdate)
                            {
                                //PM 20140807 [20273][20106] Calcul unitaire arrondie avant de multiplier par la quantité
                                //cashFlowAmount = eventDetPrice100 * eventDetPosQuantityUSED * contractMultiplier;
                                //PM 20150129 [20737][20754] Modification du calcul unitaire
                                //cashFlowAmount = m_ExchangeTradedDerivative.CashFlowValorization(eventDetPrice100, contractMultiplier, eventDetPosQuantityUSED);
                                // FI 20190801 [24821] Application de l'arrondi sur la prime
                                cashFlowAmount = m_ExchangeTradedDerivative.CashFlowValorization(eventDetPrice100, 0, ContractMultiplier, eventDetPosQuantityUSED, true);
                            }
                        }
                        //
                        if (isEventToUpdate)
                        {
                            rowEventDet["CONTRACTMULTIPLIER"] = ContractMultiplier;
                            rowEvent["VALORISATION"] = Math.Abs(cashFlowAmount);
                            rowEvent["VALORISATIONSYS"] = Math.Abs(cashFlowAmount);
                            //
                            Update(pDbTransaction, idE, false, IsEndOfDayProcess);
                        }
                    }
                }
            }

        }

        /// <summary>
        /// Retourne les lignes de la table EVENT de type  Denouement d'options tel que {EVENCODE = AEX/EXE/AAS/ASS/ABN/AAB}
        /// <para>Retourne null si les lignes n'existe pas</para>
        /// </summary>
        /// <returns></returns>
        protected DataRow[] GetRowDenouementGroup(DateTime pDate)
        {
            DataRow[] rows = null;
            if (null != DsEvents)
            {
                rows = DsEvents.DtEvent.Select(StrFunc.AppendFormat(@"IDT = {0} and INSTRUMENTNO = {1} and EVENTCODE in ('{2}','{3}','{4}','{5}','{6}','{7}') and DTSTARTADJ=#{8}#",
                     m_ParamIdT.Value, StrFunc.GetSuffixNumeric2(m_CurrentProduct.ProductBase.Id), 
                     EventCodeFunc.AutomaticAbandon, EventCodeFunc.AutomaticAssignment, EventCodeFunc.AutomaticExercise, 
                     EventCodeFunc.Abandon, EventCodeFunc.Assignment, EventCodeFunc.Exercise, 
                     DtFunc.DateTimeToStringDateISO(pDate.Date)),"IDE");
            }
            return rows;
        }
        /// <summary>
        /// Retourne les lignes de la table EVENT de type  Correction de Position (Position Cancelation) tel que {EVENCODE = POC}
        /// <para>Retourne null si les lignes n'existe pas</para>
        /// </summary>
        /// <returns></returns>
        protected DataRow[] GetRowPositionCancelationGroup()
        {
            DataRow[] rows = null;
            if (null != DsEvents)
            {
                rows = DsEvents.DtEvent.Select(StrFunc.AppendFormat(@"IDT = {0} and INSTRUMENTNO = {1} and EVENTCODE = '{2}' and DTSTARTADJ=#{3}#",
                     m_ParamIdT.Value, StrFunc.GetSuffixNumeric2(m_CurrentProduct.ProductBase.Id), EventCodeFunc.PositionCancelation, 
                     DtFunc.DateTimeToStringDateISO(m_Quote.time.Date)),"IDE");
            }
            return rows;
        }
        /// <summary>
        /// Retourne les lignes de la table EVENT de type  Correction de Position (Position Cancelation) tel que {EVENCODE = POC}
        /// <para>Retourne null si les lignes n'existe pas</para>
        /// </summary>
        /// <returns></returns>
        protected DataRow[] GetRowPositionTransferGroup()
        {
            DataRow[] rows = null;
            if (null != DsEvents)
            {
                rows = DsEvents.DtEvent.Select(StrFunc.AppendFormat(@"IDT = {0} and INSTRUMENTNO = {1} and EVENTCODE = '{2}' and DTSTARTADJ=#{3}#",
                     m_ParamIdT.Value, StrFunc.GetSuffixNumeric2(m_CurrentProduct.ProductBase.Id), EventCodeFunc.PositionTransfer,
                     DtFunc.DateTimeToStringDateISO(m_Quote.time.Date)), "IDE");
            }
            return rows;
        }

        /// <summary>
        /// Retourne les lignes de la table EVENT de type Liquidation de Future (Maturity Offsetting) tel que {EVENCODE = MOF}
        /// <para>Retourne null si les lignes n'existe pas</para>
        /// </summary>
        /// <returns></returns>
        // EG 20140110 [19470]
        protected DataRow[] GetRowMaturityOffsettingGroup()
        {
            DataRow[] rows = null;
            if (null != DsEvents)
            {
                rows = DsEvents.DtEvent.Select(StrFunc.AppendFormat(@"IDT = {0} and INSTRUMENTNO = {1} and EVENTCODE = '{2}' and DTSTARTADJ=#{3}#",
                     m_ParamIdT.Value, StrFunc.GetSuffixNumeric2(m_CurrentProduct.ProductBase.Id), EventCodeFunc.MaturityOffsettingFuture,
                     DtFunc.DateTimeToStringDateISO(m_Quote.time.Date)), "IDE");
            }
            return rows;
        }



        /// <summary>
        /// Retourne tous les Events {pEventCode}/{pEventType} du jour ou future
        /// <para>Retourne null si pas de lignes existantes</para>
        /// </summary>
        /// <returns></returns>
        protected DataRow[] GetTodayOrFutureEvent(string pEventCode, string pEventType)
        {
            DataRow[] rows = null;
            if (null != DsEvents)
            {
                rows = DsEvents.DtEvent.Select(StrFunc.AppendFormat(@"IDT = {0} and INSTRUMENTNO = {1} and EVENTCODE = '{2}' and EVENTTYPE = '{3}' and DTSTARTADJ>=#{4}#",
                     m_ParamIdT.Value, StrFunc.GetSuffixNumeric2(m_CurrentProduct.ProductBase.Id), pEventCode, pEventType, 
                     DtFunc.DateTimeToStringDateISO(Quote_ETDAsset.time.Date)), "IDE");
            }
            return rows;
        }

        /// <summary>
        /// Alimente les membres m_ExchangeTradedDerivative,_buyer,_bookBuyer,_seller,_bookSeller de la classe
        /// </summary>
        // EG 20180205 [23769] Use function with lock
        // EG 20190613 [24683] Use slaveDbTransaction
        private void Initialize()
        {
            m_ExchangeTradedDerivative = new ExchangeTradedDerivativeContainer((IExchangeTradedDerivative)m_CurrentProduct);
            m_EventsValProcess.ProcessCacheContainer.SetAssetETD(m_ExchangeTradedDerivative);

            // Buyer / Seller
            IFixParty buyer = m_ExchangeTradedDerivative.GetBuyer();
            IFixParty seller = m_ExchangeTradedDerivative.GetSeller();
            if (null == buyer)
                throw new NotSupportedException("buyer is not Found");
            if (null == seller)
                throw new NotSupportedException("seller is not Found");

            IReference buyerReference = buyer.PartyId;
            // EG 20170324 use m_Buyer|m_BookBuyer
            m_Buyer = TradeLibrary.DataDocument.GetParty(buyerReference.HRef);
            m_BookBuyer = TradeLibrary.DataDocument.GetOTCmlId_Book(m_Buyer.Id);

            IReference sellerReference = seller.PartyId;
            // EG 20170324 use m_Seller|m_BookSeller
            m_Seller = TradeLibrary.DataDocument.GetParty(sellerReference.HRef);
            m_BookSeller = TradeLibrary.DataDocument.GetOTCmlId_Book(m_Seller.Id);

            // EG 20170324 [22991] 
            int idEntity;
            if ((null != Quote_ETDAsset) && Quote_ETDAsset.eodComplementSpecified)
            {
                m_IsPosKeeping_BookDealer = Quote_ETDAsset.eodComplement.isPosKeeping_BookDealer;
                idEntity = Quote_ETDAsset.eodComplement.idAEntity;
            }
            else
            {
                m_IsPosKeeping_BookDealer = PosKeepingTools.IsPosKeepingBookDealer(m_CS, Process.SlaveDbTransaction , m_DsTrade.IdT, Cst.ProductGProduct_FUT);
                idEntity = TradeLibrary.DataDocument.GetFirstEntity(CSTools.SetCacheOn(m_CS));
            }

            // EG 20170324  use m_EntityMarketInfo
            m_EntityMarketInfo = m_EventsValProcess.ProcessCacheContainer.GetEntityMarketLock(idEntity, m_ExchangeTradedDerivative.AssetETD.IdM, null);
            _isCashFlowsVal = (null != Quote_ETDAsset) && (Quote_ETDAsset.isCashFlowsValSpecified) && (Quote_ETDAsset.isCashFlowsVal);
        }

        /// <summary>
        /// Valorisation des évènement existants Premium,RealizedMargin,VariationMargin,UnrealizedMargin et LiquidationOptionValue
        /// </summary>
        // EG 20111024 Test sur _isPosKeeping_BookDealer pour recalcul des montants
        // EG 20170412 [23081] Gestion  dbTransaction et SlaveDbTransaction
        // EG 20180502 Analyse du code Correction [CA2200]
        private void CashFlowValorisation()
        {
            IDbTransaction dbTransaction = null;
            if (null != SlaveDbTransaction)
                dbTransaction = SlaveDbTransaction;

            bool isException = false;
            try
            {
                if (null == SlaveDbTransaction)
                    dbTransaction = DataHelper.BeginTran(m_EventsValProcess.Cs);
                // EG 20170324 use m_IsPosKeeping_BookDealer
                if (m_IsPosKeeping_BookDealer)
                {
                    // VariationMargin
                    if (m_ExchangeTradedDerivative.IsFuturesStyleMarkToMarket)
                    {
                        CashFlowAmountGen(dbTransaction, EventCodeFunc.LinkedProductPayment, EventTypeFunc.VariationMargin);
                        CashFlowAmountGen(dbTransaction, EventCodeFunc.LinkedProductIntraday, EventTypeFunc.VariationMargin);
                    }
                    // UnrealizedMargin
                    CashFlowAmountGen(dbTransaction, EventCodeFunc.LinkedProductClosing, EventTypeFunc.UnrealizedMargin);
                    CashFlowAmountGen(dbTransaction, EventCodeFunc.LinkedProductIntraday, EventTypeFunc.UnrealizedMargin);
                    //
                    if (m_ExchangeTradedDerivative.IsOption)
                    {
                        // LiquidationOptionValue
                        CashFlowAmountGen(dbTransaction, EventCodeFunc.LinkedProductClosing, EventTypeFunc.LiquidationOptionValue);
                        CashFlowAmountGen(dbTransaction, EventCodeFunc.LinkedProductIntraday, EventTypeFunc.LiquidationOptionValue);
                        // Premium
                        // EG 20130723 (18754)
                        if (m_ExchangeTradedDerivative.IsPremiumStyle || m_ExchangeTradedDerivative.IsFuturesStyleMarkToMarket)
                            CashFlowAmountGen(dbTransaction, EventCodeFunc.LinkedProductPayment, EventTypeFunc.Premium);
                    }
                    // RealizedMargin
                    CashFlowAmountGen(dbTransaction, EventCodeFunc.LinkedProductClosing, EventTypeFunc.RealizedMargin);
                }
                // EG 20130723 (18754)
                else if (m_ExchangeTradedDerivative.IsOption && (m_ExchangeTradedDerivative.IsPremiumStyle || m_ExchangeTradedDerivative.IsFuturesStyleMarkToMarket))
                {
                    // Premium
                    CashFlowAmountGen(dbTransaction, EventCodeFunc.LinkedProductPayment, EventTypeFunc.Premium);
                }

                if (null == SlaveDbTransaction)
                {
                    DataHelper.CommitTran(dbTransaction);
                    dbTransaction = null;
                }
            }
            catch (Exception) { isException = true; throw; }
            finally
            {
                if ((null != dbTransaction) && (null == SlaveDbTransaction) && isException)
                {
                    try
                    {
                        DataHelper.RollbackTran(dbTransaction);
                    }
                    catch { }
                }
            }
        }


        /// <summary>
        /// CONTROLE DE L'ASSET à L'ORIGINE DE LA DEMANDE
        /// L'IdAsset est stocké dans la table EVENTASSET 
        /// 
        /// 1 ► Sur la ligne de type STREAM c'est à dire
        ///    ● FED/FUT/GRP
        ///    ● AED-EED/CAL-PUT/GRP
        /// 2 ► Sur la ligne de CashSettlement sur les options dénouées c'est à dire
        ///    ● INT-TER/SCU enfants de MOF-AAS-ASS-AEX-EXE/TOT-INT
        /// </summary>
        // EG 20180619 Add parameter pQuoteinfo
        // EG 20201009 [25504] Recherche de l'événement Group d'une option ETD (AED-EED) et mise à jour du code EVENTCODE de celui-ci si erroné avant valorisation des événements enfants.
        private bool IsAssetMaster(ETDQuoteInfo pQuoteInfo)
        {
            bool isAssetMaster = false;
            DataRow[] rowStream;
            if (m_ExchangeTradedDerivative.IsOption)
            {
                if (FixML.v50SP1.Enum.DerivativeExerciseStyleEnum.American == m_ExchangeTradedDerivative.ExerciseStyle)
                    rowStream = UpdateEventCodeExchangeTradedDerivativeOption(EventCodeFunc.AmericanExchangeTradedDerivativeOption, EventCodeFunc.EuropeanExchangeTradedDerivativeOption);
                else
                    rowStream = UpdateEventCodeExchangeTradedDerivativeOption(EventCodeFunc.EuropeanExchangeTradedDerivativeOption, EventCodeFunc.AmericanExchangeTradedDerivativeOption);
            }
            else
            {
                rowStream = GetRowEventByEventCode(EventCodeFunc.FutureExchangeTradedDerivative);
            }
            if (ArrFunc.IsFilled(rowStream))
            {
                int idE = Convert.ToInt32(rowStream[0]["IDE"]);
                DataRow[] rowAsset = GetRowAsset(idE);
                if (ArrFunc.IsFilled(rowAsset))
                {
                    //isAssetMaster = (null != m_Quote) && (m_Quote.idAsset == Convert.ToInt32(rowAsset[0]["IDASSET"]));
                    if (null != m_Quote)
                        isAssetMaster =  (m_Quote.idAsset == Convert.ToInt32(rowAsset[0]["IDASSET"]));
                    else if (null != pQuoteInfo.quote)
                        isAssetMaster = (pQuoteInfo.quote.idAsset == Convert.ToInt32(rowAsset[0]["IDASSET"]));
                }
            }
            return isAssetMaster;
        }

        /// <summary>
        /// Purge les closing Amount présents supérieur à la date donnée
        /// <para>Tous les montants sont mis à zéro</para>
        /// </summary>
        /// <param name="pDate"></param>
        // EG 20110708 Remove d'autres EVTs : VMG sur AAB/ASS/AEX/ABN/ASS/EXE et POC, RMG sur MOF
        // EG 20170412 [23081] Gestion  dbTransaction et SlaveDbTransaction
        private void RemoveClosingAmountGen(DateTime pDate)
        {

            IDbTransaction dbTransaction = null;
            if (null != SlaveDbTransaction)
                dbTransaction = SlaveDbTransaction;

            bool isException = false;
            try
            {
                if (null == SlaveDbTransaction)
                    dbTransaction = DataHelper.BeginTran(m_EventsValProcess.Cs);
                //
                DataRow rowEventAMT = GetRowAmountGroup();
                if (null != rowEventAMT)
                {
                    m_ParamInstrumentNo.Value = Convert.ToInt32(rowEventAMT["INSTRUMENTNO"]);
                    m_ParamStreamNo.Value = Convert.ToInt32(rowEventAMT["STREAMNO"]);
                    //
                    RemoveClosingAmountGen(dbTransaction, rowEventAMT, pDate, EventTypeFunc.VariationMargin);
                    RemoveClosingAmountGen(dbTransaction, rowEventAMT, pDate, EventTypeFunc.UnrealizedMargin);
                    RemoveClosingAmountGen(dbTransaction, rowEventAMT, pDate, EventTypeFunc.LiquidationOptionValue);
                }
                // Remove VMG sur Denouement d'options
                DataRow[] rowEventDEN = GetRowDenouementGroup(pDate);
                if (null != rowEventDEN)
                {
                    foreach (DataRow row in rowEventDEN)
                    {
                        m_ParamInstrumentNo.Value = Convert.ToInt32(row["INSTRUMENTNO"]);
                        m_ParamStreamNo.Value = Convert.ToInt32(row["STREAMNO"]);
                        RemoveClosingAmountGen(dbTransaction, row, pDate, EventTypeFunc.VariationMargin);
                    }
                }
                //
                // Remove VMG sur Correction de Position
                DataRow[] rowEventPOC = GetRowPositionCancelationGroup();
                if (null != rowEventPOC)
                {
                    foreach (DataRow row in rowEventPOC)
                    {
                        m_ParamInstrumentNo.Value = Convert.ToInt32(row["INSTRUMENTNO"]);
                        m_ParamStreamNo.Value = Convert.ToInt32(row["STREAMNO"]);
                        RemoveClosingAmountGen(dbTransaction, row, pDate, EventTypeFunc.VariationMargin);
                    }
                }
                // Remove VMG sur transfert de Position
                DataRow[] rowEventPOT = GetRowPositionTransferGroup();
                if (null != rowEventPOT)
                {
                    foreach (DataRow row in rowEventPOT)
                    {
                        m_ParamInstrumentNo.Value = Convert.ToInt32(row["INSTRUMENTNO"]);
                        m_ParamStreamNo.Value = Convert.ToInt32(row["STREAMNO"]);
                        RemoveClosingAmountGen(dbTransaction, row, pDate, EventTypeFunc.VariationMargin);
                    }
                }
                // Remove RMG sur Liquidation de future (MOF : Maturity Offsetting)
                DataRow[] rowEventMOF = GetRowMaturityOffsettingGroup();
                if (null != rowEventMOF)
                {
                    foreach (DataRow row in rowEventMOF)
                    {
                        // On exclue les lignes DEACTIVEES du [re]calcul
                        int idE = Convert.ToInt32(row["IDE"]);
                        if (null == GetRowEventClass(idE, EventClassFunc.RemoveEvent))
                            RemoveClosingAmountGen(dbTransaction, row, pDate, EventTypeFunc.RealizedMargin);
                    }
                }
                if (null == SlaveDbTransaction)
                {
                    DataHelper.CommitTran(dbTransaction);
                    dbTransaction = null;
                }
            }
            catch (Exception)
            {
                isException = true;
                throw;
            }
            finally
            {
                if ((null != dbTransaction) && (null == SlaveDbTransaction) && isException)
                {
                    try
                    {
                        DataHelper.RollbackTran(dbTransaction);
                    }
                    catch { }
                }
            }
        }

        /// <summary>
        /// Purge les closing Amount présents supérieur à la date donnée d'un même type
        /// <para>Tous les montants sont mis à zéro</para>
        /// </summary>
        /// <param name="pDbTransaction"></param>
        /// <param name="pRowAmountGroup"></param>
        /// <param name="pDate"></param>
        /// <param name="pEventType"></param>
        /// EG 20150113 [20501]
        private void RemoveClosingAmountGen(IDbTransaction pDbTransaction, DataRow pRowAmountGroup, DateTime pDate, string pEventType)
        {
            string eventCodeLink = EventCodeLink(pRowAmountGroup["EVENTTYPE"].ToString(), pEventType, m_Quote.QuoteTiming.Value);
            DataRow[] rowChilds = pRowAmountGroup.GetChildRows(m_DsEvents.ChildEvent);
            foreach (DataRow rowChild in rowChilds)
            {
                if ((eventCodeLink == rowChild["EVENTCODE"].ToString()) &&
                    (pEventType == rowChild["EVENTTYPE"].ToString()) &&
                    (pDate.Date <= Convert.ToDateTime(rowChild["DTSTARTUNADJ"])))
                {
                    int idE = Convert.ToInt32(rowChild["IDE"]);
                    // Clear amount
                    rowChild["VALORISATION"] = Convert.DBNull;
                    rowChild["IDA_PAY"] = Convert.DBNull;
                    rowChild["IDB_PAY"] = Convert.DBNull;
                    rowChild["IDA_REC"] = Convert.DBNull;
                    rowChild["IDB_REC"] = Convert.DBNull;
                    // Clear amount details + Modify NOTE = 
                    DataRow rowEventDet = GetRowEventDetail(idE);
                    rowEventDet["QUOTEPRICE"] = Convert.DBNull;
                    rowEventDet["QUOTEPRICE100"] = Convert.DBNull;
                    // RD 20120821 [18087] Add QUOTEDELTA
                    rowEventDet["QUOTEDELTA"] = Convert.DBNull;
                    rowEventDet["NOTE"] = "Quotation was deleted or deactivated";
                    //
                    Update(pDbTransaction, idE, false, IsEndOfDayProcess);
                }
            }
        }


        #region Calculation_LPC_AMT
        /// <summary>
        /// Calcul des marges (HVM|VMG|LOV|UMG)
        /// </summary>
        /// <param name="pQuote"></param>
        /// <param name="pQuotePrev"></param>
        /// EG 20150208 [POC-MUREX] Refactoring Alimentation List(VAL_Event)
        // EG 20190613 [24683] Upd Calling WriteClosingAmount
        // EG 20190925 Retour arrière CashFlows (Mise en veille de l'appel à )EndOfDayWriteClosingAmountGen
        private void Calculation_LPC_AMT(ETDQuoteInfo pQuote, ETDQuoteInfo pQuotePrev)
        {
            bool isClosePrice = (false == pQuote.quote.QuoteTiming.HasValue) || (QuoteTimingEnum.Close == pQuote.quote.QuoteTiming.Value);
            bool isIntradayPrice = pQuote.quote.QuoteTiming.HasValue && (QuoteTimingEnum.Intraday == pQuote.quote.QuoteTiming.Value);

            DataRow rowEventAMT = GetRowAmountGroup();

            List<VAL_Event> lstEvents = new List<VAL_Event>();

            #region Historical VariationMargin
            // EG 20130610 Add test m_ExchangeTradedDerivative.isTradeCAAdjusted
            if ((m_ExchangeTradedDerivative.IsPositionOpening || m_ExchangeTradedDerivative.IsTradeCAAdjusted) &&
                (isClosePrice && m_ExchangeTradedDerivative.IsFuturesStyleMarkToMarket))
                lstEvents.Add(PrepareClosingAmountGen(rowEventAMT, EventTypeFunc.HistoricalVariationMargin, pQuote, pQuotePrev));
            #endregion Historical VariationMargin

            #region VariationMargin
            if ((m_ExchangeTradedDerivative.IsFuturesStyleMarkToMarket) && (isClosePrice || isIntradayPrice))
                lstEvents.Add(PrepareClosingAmountGen(rowEventAMT, EventTypeFunc.VariationMargin, pQuote, pQuotePrev));
            #endregion VariationMargin

            #region UnrealizedMargin
            if (isClosePrice || isIntradayPrice)
                lstEvents.Add(PrepareClosingAmountGen(rowEventAMT, EventTypeFunc.UnrealizedMargin, pQuote, pQuotePrev));
            #endregion UnrealizedMargin

            #region LiquidationOptionValue
            if (m_ExchangeTradedDerivative.IsOption && (isClosePrice || isIntradayPrice))
                lstEvents.Add(PrepareClosingAmountGen(rowEventAMT, EventTypeFunc.LiquidationOptionValue, pQuote, pQuotePrev));
            #endregion LiquidationOptionValue

            //if (IsEndOfDayProcess)
            //    EndOfDayWriteClosingAmountGen(lstEvents);
            //else
            //    WriteClosingAmountGen(lstEvents);
            WriteClosingAmountGen(lstEvents, pQuote.dtBusiness);

        }
        #endregion Calculation_LPC_AMT

        #region PrepareClosingAmountGen
        /// <summary>
        /// Calcul des montants
        /// </summary>
        /// EG 20160208 [POC-MUREX] New
        /// RD 20160303 [21975] Modify        
        /// EG 20170510 [23153] SetRowEventClosingAmountGen parameters
        // EG 20180205 [23769] Use function with lock
        // EG 20190114 Add detail to ProcessLog Refactoring
        // EG 20190716 [VCL : New FixedIncome] Upd GetQuoteLock
        private VAL_Event PrepareClosingAmountGen(DataRow pRowEventAMT, string pEventType, ETDQuoteInfo pQuote, ETDQuoteInfo pQuotePrev)
        {
            VAL_Event @event = new VAL_Event();

            Quote_ETDAsset _quoteETDAsset = pQuote.quote as Quote_ETDAsset;
            string eventCodeLink = EventCodeLink(EventTypeFunc.Amounts, pEventType, _quoteETDAsset.QuoteTiming.Value);
            m_ParamInstrumentNo.Value = Convert.ToInt32(pRowEventAMT["INSTRUMENTNO"]);
            m_ParamStreamNo.Value = Convert.ToInt32(pRowEventAMT["STREAMNO"]);

            @event.Value = GetRowAmount(pEventType, _quoteETDAsset.QuoteTiming.Value, pQuote.dtBusiness);
            @event.IsNewRow = (null == @event.Value);

            if (@event.IsNewRow)
            {
                // EG 20160208 POC-MUREX Appel NewRowEvent2 (l'IdE sera mis à jour ultérieurement : via WriteClosingAmountGen)
                @event.Value = NewRowEvent2(pRowEventAMT, eventCodeLink, pEventType, pQuote.dtBusiness, pQuote.dtBusiness, m_EventsValProcess.AppInstance);
                @event.ClassREC = NewRowEventClass(-1, EventClassFunc.Recognition, pQuote.dtBusiness, false);

                // EG 20150616 [21124] pour VMG|UMG|LOV|HVM
                @event.ClassVAL = NewRowEventClass(-1, EventClassFunc.ValueDate, pQuote.dtBusiness, false);

                if (EventTypeFunc.IsVariationMargin(pEventType))
                {
                    // EG 20170324 use m_EntityMarketInfo
                    DateTime dtSTL = m_EventsValProcess.ProcessCacheContainer.GetBusinessDayLock(pQuote.dtBusiness, BusinessDayConventionEnum.FOLLOWING, m_EntityMarketInfo.IdBC);
                    @event.ClassSTL = NewRowEventClass(-1, EventClassFunc.Settlement, dtSTL, true);
                }
                @event.Detail = NewRowEventDet(@event.Value);
            }
            else
            {
                @event.Detail = GetRowEventDetail(Convert.ToInt32(@event.Value["IDE"]));
            }

            // EG 20150706 [21021]
            Nullable<int> idAPay = null;
            Nullable<int> idBPay = null;
            Nullable<int> idARec = null;
            Nullable<int> idBRec = null;

            // RD 20160303 [21975] Variable posQuantityUsed not necessary
            //decimal posQuantityUsed = decimal.Zero;
            // PM 20130801 [18876] Added for Contract with Variable tick Value
            decimal strikePrice = m_ExchangeTradedDerivative.StrikePrice;
            decimal strikePrice100 = m_ExchangeTradedDerivative.ToBase100(strikePrice);
            // EG 20141128 [20520] Nullable<decimal>
            Nullable<decimal> quote = null;
            Nullable<decimal> quote100 = null;
            if (pQuote.quote.valueSpecified)
                quote = pQuote.quote.value;

            if (quote.HasValue)
            {
                quote100 = m_ExchangeTradedDerivative.ToBase100(quote.Value);
                quote100 = m_ExchangeTradedDerivative.VariableContractValue(strikePrice100, quote100.Value);
            }

            @event.Detail["CONTRACTMULTIPLIER"] = ContractMultiplier;
            @event.Detail["DTACTION"] = pQuote.dtQuote;
            @event.Detail["QUOTETIMING"] = pQuote.quote.quoteTiming;
            @event.Detail["QUOTEPRICE"] = quote ?? Convert.DBNull;
            @event.Detail["QUOTEPRICE100"] = quote100 ?? Convert.DBNull;
            // RD 20120821 [18087] Add QUOTEDELTA
            if (_quoteETDAsset.deltaSpecified)
                @event.Detail["QUOTEDELTA"] = _quoteETDAsset.delta;
            else
                @event.Detail["QUOTEDELTA"] = DBNull.Value;

            if (EventTypeFunc.IsHistoricalVariationMargin(pEventType))
            {
                #region HistoricalVariationMargin
                // --------------------------------------------------------------------------------------------------------------------------
                // Sur un trade d'insertion de position 
                // => m_ExchangeTradedDerivative.isPositionOpening ou m_ExchangeTradedDerivative.isTradeCAAdjusted
                // Spheres® calcule un montant (de reprise) historique de variation de marge sur la base de: 
                // QTY(Init) * MULTIPLIER * (QUOTE(J-1) - PRIX(Nego))
                // --------------------------------------------------------------------------------------------------------------------------
                if (m_ExchangeTradedDerivative.ClearingBusinessDate.Date >= pQuote.dtBusiness)
                {
                    ETDQuoteInfo _quoteVeil = new ETDQuoteInfo(this, NextPreviousEnum.Previous, m_ExchangeTradedDerivative.ClearingBusinessDate.Date);

                    if ((m_ExchangeTradedDerivative.ClearingBusinessDate.Date == pQuote.dtBusiness) || (_quoteVeil.dtBusiness == pQuote.dtBusiness))
                    {
                        @event.Qty = m_ExchangeTradedDerivative.Qty;

                        _quoteVeil.SetQuote(this);
                        if (((null == _quoteVeil.quote) || (false == _quoteVeil.quote.valueSpecified)) && (0 < @event.Qty))
                        {
                            #region Log error message
                            // EG 20130620 Si Traitement EOD alors Message Warning + CodeErreur = QUOTENOTFOUND
                            ProcessState _processState = new ProcessState(ProcessStateTools.StatusErrorEnum);
                            if (m_Quote.isEOD)
                            {
                                _processState.Status = ProcessStateTools.StatusEnum.WARNING;
                                _processState.CodeReturn = ProcessStateTools.CodeReturnQuoteNotFoundEnum;
                                m_EventsValProcess.ProcessState.SetProcessState(_processState);
                            }

                            // FI 20200623 [XXXXX] SetErrorWarning
                            m_EventsValProcess.ProcessState.SetErrorWarning(_processState.Status);

                            Logger.Log(new LoggerData(LoggerTools.StatusToLogLevelEnum(_processState.Status), new SysMsgCode(SysCodeEnum.SYS, 5158), 2,
                                new LogParam(LogTools.IdentifierAndId(m_EventsValMQueue.GetStringValueIdInfoByKey("identifier"), m_DsTrade.IdT)),
                                new LogParam(LogTools.IdentifierAndId(m_Quote.idAsset_Identifier, m_Quote.idAsset)),
                                new LogParam(DtFunc.DateTimeToString(_quoteVeil.dtQuote, DtFunc.FmtDateTime)),
                                new LogParam(pEventType)));

                            throw new SpheresException2(_processState);
                            #endregion Log error message
                        }
                        decimal quoteVeil = _quoteVeil.quote.value;
                        decimal quoteVeil100 = m_ExchangeTradedDerivative.ToBase100(quoteVeil);
                        // PM 20130801 [18876] Added for Contract with Variable tick Value
                        quoteVeil100 = m_ExchangeTradedDerivative.VariableContractValue(strikePrice100, quoteVeil100);
                        //
                        //PM 20140807 [20273][20106] Calcul unitaire arrondie avant de multiplier par la quantité
                        //closingAmount = (quoteVeil100 - m_ExchangeTradedDerivative.Price100) * posQuantityUsed * contractMultiplier;
                        //PM 20150129 [20737][20754] Modification du calcul unitaire
                        //closingAmount = m_ExchangeTradedDerivative.CashFlowValorization(quoteVeil100 - m_ExchangeTradedDerivative.Price100, contractMultiplier, (int)posQuantityUsed);
                        // RD 20160303 [21975] Variable posQuantityUsed not valorized            
                        //@event.closingAmount = m_ExchangeTradedDerivative.CashFlowValorization(quoteVeil100, m_ExchangeTradedDerivative.Price100, contractMultiplier, (int)posQuantityUsed);
                        // EG 20170127 Qty Long To Decimal
                        @event.ClosingAmount = m_ExchangeTradedDerivative.CashFlowValorization(quoteVeil100, m_ExchangeTradedDerivative.Price100, ContractMultiplier, @event.Qty);

                        @event.Detail["PRICE"] = m_ExchangeTradedDerivative.Price;
                        @event.Detail["PRICE100"] = m_ExchangeTradedDerivative.Price100;
                        @event.Detail["QUOTEPRICEYEST"] = quoteVeil;
                        @event.Detail["QUOTEPRICEYEST100"] = quoteVeil100;
                        // EG 20170324 use m_Seller|m_BookSeller|m_Buyer|m_BookBuyer
                        idAPay = (0 < @event.ClosingAmount.Value) ? m_Seller.OTCmlId : m_Buyer.OTCmlId;
                        idBPay = (0 < @event.ClosingAmount.Value) ? m_BookSeller : m_BookBuyer;
                        idARec = (0 < @event.ClosingAmount.Value) ? m_Buyer.OTCmlId : m_Seller.OTCmlId;
                        idBRec = (0 < @event.ClosingAmount.Value) ? m_BookBuyer : m_BookSeller;
                        CommonValFunc.SetPayerReceiver(@event.Value, idAPay, idBPay, idARec, idBRec);
                    }
                }
                #endregion HistoricalVariationMargin
            }
            else if (EventTypeFunc.IsUnrealizedMargin(pEventType))
            {
                #region UnrealizedMargin
                // EG 20141128 [20520] Nullable<decimal>
                if (quote.HasValue)
                {
                    @event.Qty = PosAvailableQuantity;
                    //PM 20140807 [20273][20106] Calcul unitaire arrondie avant de multiplier par la quantité
                    //closingAmount = (quote100 - m_ExchangeTradedDerivative.Price100) * posQuantityUsed * contractMultiplier;
                    //PM 20150129 [20737][20754] Modification du calcul unitaire
                    //closingAmount = m_ExchangeTradedDerivative.CashFlowValorization(quote100.Value - m_ExchangeTradedDerivative.Price100, contractMultiplier, (int)posQuantityUsed);
                    @event.ClosingAmount = m_ExchangeTradedDerivative.CashFlowValorization(quote100.Value, m_ExchangeTradedDerivative.Price100, ContractMultiplier, (int)@event.Qty);
                    // EG 20170324 use m_Seller|m_BookSeller|m_Buyer|m_BookBuyer
                    idAPay = (0 < @event.ClosingAmount.Value) ? m_Seller.OTCmlId : m_Buyer.OTCmlId;
                    idBPay = (0 < @event.ClosingAmount.Value) ? m_BookSeller : m_BookBuyer;
                    idARec = (0 < @event.ClosingAmount.Value) ? m_Buyer.OTCmlId : m_Seller.OTCmlId;
                    idBRec = (0 < @event.ClosingAmount.Value) ? m_BookBuyer : m_BookSeller;
                }
                CommonValFunc.SetPayerReceiver(@event.Value, idAPay, idBPay, idARec, idBRec);
                @event.Detail["PRICE"] = m_ExchangeTradedDerivative.Price;
                @event.Detail["PRICE100"] = m_ExchangeTradedDerivative.Price100;
                #endregion UnrealizedMargin
            }
            else if (EventTypeFunc.IsLiquidationOptionValue(pEventType))
            {
                #region LiquidationOptionValue
                // EG 20141128 [20520] Nullable<decimal>
                if (quote.HasValue)
                {
                    @event.Qty = PosAvailableQuantity;
                    //PM 20140807 [20273][20106] Calcul unitaire arrondie avant de multiplier par la quantité
                    //closingAmount = quote100 * posQuantityUsed * contractMultiplier;
                    //PM 20150129 [20737][20754] Modification du calcul unitaire
                    //closingAmount = m_ExchangeTradedDerivative.CashFlowValorization(quote100.Value, contractMultiplier, (int)posQuantityUsed);
                    @event.ClosingAmount = m_ExchangeTradedDerivative.CashFlowValorization(quote100.Value, 0, ContractMultiplier, @event.Qty);
                    // EG 20170324 use m_Seller|m_BookSeller|m_Buyer|m_BookBuyer
                    idAPay = m_Seller.OTCmlId;
                    idBPay = m_BookSeller;
                    idARec = m_Buyer.OTCmlId;
                    idBRec = m_BookBuyer;
                }
                CommonValFunc.SetPayerReceiver(@event.Value, idAPay, idBPay, idARec, idBRec);
                #endregion LiquidationOptionValue
            }
            else if (EventTypeFunc.IsVariationMargin(pEventType))
            {
                #region VariationMargin
                // --------------------------------------------------------------------------------------------------------------------------
                // Rappel: 
                // --------------------------------------------------------------------------------------------------------------------------
                //            - Sur un trade "Jour", qui représente un "trade de marché", 
                //            Spheres® calcule la marge entre le prix négocié et le cours jour (OfficialClose), 
                //            sur la base de la quantité du "trade de marché" (donc de la quantité négociée).
                //
                //            - Sur un trade "Veille", quel qu'il soit, qui représente donc un "trade en position",  
                //            Spheres® calcule la marge entre le cours jour (OfficialClose) et le cours veille (OfficialClose), 
                //            sur la base de la quantité en position la veille (donc à l'ouverture de la journée).
                //
                //            - Sur un trade "Jour", qui ne représente pas un "trade de marché", mais un "trade ouverture de position", 
                //            Spheres® calcule la marge entre le cours jour (OfficialClose) et le cours veille (OfficialClose), 
                //            à l'image de ce qui est fait classiquement pour un "trade de marché Veille", 
                //            sur la base de la quantité du "trade ouverture de position" (donc de la quantité insérée en position).
                //
                //            - Sur un trade "Jour", qui représente un "trade post cascading", 
                //            Spheres® calcule la marge entre le cours jour (OfficialClose) de l'asset du trade
                //            et le cours jour (OfficialClose) de l'asset du trade avant cascading.
                // --------------------------------------------------------------------------------------------------------------------------
                bool isTradeDay = (m_ExchangeTradedDerivative.ClearingBusinessDate.Date == pQuote.dtBusiness);
                //
                if (isTradeDay)
                {
                    //"Trade d'ouverture de position" ou "Trade de marché" du jour
                    @event.Qty = m_ExchangeTradedDerivative.Qty;
                }
                else
                {
                    //"Trade de marché" négocié les jours précédents 
                    // Quantité en position veille +/- somme(quantité des actions du jour) (+ unclearing, - transfert et correction)
                    @event.Qty = PosQuantityPrevAndActionsDay;
                }
                //
                // PM 20130220 [18414] Ajout gestion Cascading
                // EG 20130607 Ajout gestion CorporateAction
                if (isTradeDay
                    && (false == m_ExchangeTradedDerivative.IsPositionOpening)
                    && (false == m_ExchangeTradedDerivative.IsTradeCAAdjusted)
                    && (false == m_ExchangeTradedDerivative.IsCascading))
                {
                    // EG 20141128 [20520] Nullable<decimal>
                    if (quote.HasValue)
                    {
                        //"Trade de marché" du Jour 
                        //PM 20140807 [20273][20106] Calcul unitaire arrondie avant de multiplier par la quantité
                        //closingAmount = (quote100 - m_ExchangeTradedDerivative.Price100) * posQuantityUsed * contractMultiplier;
                        //PM 20150129 [20737][20754] Modification du calcul unitaire
                        //closingAmount = m_ExchangeTradedDerivative.CashFlowValorization(quote100.Value - m_ExchangeTradedDerivative.Price100, contractMultiplier, (int)posQuantityUsed);
                        // EG 20170127 Qty Long To Decimal
                        @event.ClosingAmount = m_ExchangeTradedDerivative.CashFlowValorization(quote100.Value, m_ExchangeTradedDerivative.Price100, ContractMultiplier, @event.Qty);
                    }
                    @event.Detail["PRICE"] = m_ExchangeTradedDerivative.Price;
                    @event.Detail["PRICE100"] = m_ExchangeTradedDerivative.Price100;
                }
                else
                {
                    //Le cours veille est lu dans le cache
                    SystemMSGInfo errReadOfficialClose = null;
                    Quote_ETDAsset currentQuote = null;

                    // PM 20130220 [18414] Ajout gestion Cascading
                    // Si premier jour et trade post cascading alors lire le cours de l'asset avant cascading
                    if (isTradeDay && m_ExchangeTradedDerivative.IsCascading)
                    {
                        // Lecture du cours jour sur l'asset avant cascading
                        IDataReader dr = null;
                        try
                        {
                            dr = GetAssetBeforeCascading(pQuote.dtBusiness, m_EventsValProcess.CurrentId);
                            if ((null != dr) && (dr.Read()))
                            {
                                int idAssetPreCascading = Convert.ToInt32(dr["IDASSET"]);
                                string assetPreCascadingIdentifier = Convert.ToString(dr["IDENTIFIER"]);
                                // EG 20131231 [19419]
                                currentQuote = m_EventsValProcess.ProcessCacheContainer.GetQuoteLock(idAssetPreCascading, pQuote.dtQuote,
                                    assetPreCascadingIdentifier, QuotationSideEnum.OfficialClose, Cst.UnderlyingAsset.ExchangeTradedContract,
                                    new KeyQuoteAdditional(), ref errReadOfficialClose) as Quote_ETDAsset;
                            }
                        }
                        catch (Exception)
                        {
                            currentQuote = null;
                        }
                        finally
                        {
                            if (null != dr)
                            {
                                dr.Close();
                            }
                        }
                    }
                    else
                    {
                        pQuotePrev.SetQuote(this);
                        currentQuote = pQuotePrev.quote as Quote_ETDAsset;
                    }
                    // EG 20131218 [19359] Un cours a zéro est valid si quote est valide = plus de test (0 == quote.value)
                    // RD 20160303 [21975] Variable posQuantityUsed not valorized            
                    //if (((null == currentQuote) || (false == currentQuote.valueSpecified)) && (0 < posQuantityUsed))
                    if (((null == currentQuote) || (false == currentQuote.valueSpecified)) && (0 < @event.Qty))
                    {
                        // Previous VMG not found => throw exception
                        #region Log error message
                        // EG 20130620 Si Traitement EOD alors Message Warning + CodeErreur = QUOTENOTFOUND
                        ProcessState _processState = new ProcessState(ProcessStateTools.StatusErrorEnum);
                        if (m_Quote.isEOD)
                        {
                            _processState.Status = ProcessStateTools.StatusEnum.WARNING;
                            _processState.CodeReturn = ProcessStateTools.CodeReturnQuoteNotFoundEnum;
                            m_EventsValProcess.ProcessState.SetProcessState(_processState);
                        }
                        // FI 20200623 [XXXXX] SetErrorWarning
                        m_EventsValProcess.ProcessState.SetErrorWarning(_processState.Status);

                        throw new SpheresException2(_processState);
                        #endregion Log error message
                    }

                    decimal quoteVeil = currentQuote.value;
                    decimal quoteVeil100 = m_ExchangeTradedDerivative.ToBase100(quoteVeil);
                    // PM 20130801 [18876] Added for Contract with Variable tick Value
                    quoteVeil100 = m_ExchangeTradedDerivative.VariableContractValue(strikePrice100, quoteVeil100);
                    // EG 20141128 [20520] Nullable<decimal>
                    if (quote.HasValue)
                    {
                        //PM 20140807 [20273][20106] Calcul unitaire arrondie avant de multiplier par la quantité
                        //closingAmount = (quote100 - quoteVeil100) * posQuantityUsed * contractMultiplier;
                        //PM 20150129 [20737][20754] Modification du calcul unitaire
                        //closingAmount = m_ExchangeTradedDerivative.CashFlowValorization(quote100.Value - quoteVeil100, contractMultiplier, (int)posQuantityUsed);
                        // RD 20160303 [21975] Variable posQuantityUsed not valorized            
                        //@event.closingAmount = m_ExchangeTradedDerivative.CashFlowValorization(quote100.Value, quoteVeil100, contractMultiplier, (int)posQuantityUsed);
                        // EG 20170127 Qty Long To Decimal
                        @event.ClosingAmount = m_ExchangeTradedDerivative.CashFlowValorization(quote100.Value, quoteVeil100, ContractMultiplier, @event.Qty);
                    }
                    @event.Detail["QUOTEPRICEYEST"] = quoteVeil;
                    @event.Detail["QUOTEPRICEYEST100"] = quoteVeil100;
                }
                // EG 20141128 [20520] Nullable<decimal>
                if (@event.ClosingAmount.HasValue)
                {
                    // EG 20170324 use m_Seller|m_BookSeller|m_Buyer|m_BookBuyer
                    idAPay = (0 < @event.ClosingAmount.Value) ? m_Seller.OTCmlId : m_Buyer.OTCmlId;
                    idBPay = (0 < @event.ClosingAmount.Value) ? m_BookSeller : m_BookBuyer;
                    idARec = (0 < @event.ClosingAmount.Value) ? m_Buyer.OTCmlId : m_Seller.OTCmlId;
                    idBRec = (0 < @event.ClosingAmount.Value) ? m_BookBuyer : m_BookSeller;
                }
                CommonValFunc.SetPayerReceiver(@event.Value, idAPay, idBPay, idARec, idBRec);
                #endregion VariationMargin
            }
            @event.Currency = m_ExchangeTradedDerivative.PriceQuotedCurrency;
            // EG 20170510 [23153]
            @event.SetRowEventClosingAmountGen(m_DsEvents, 1, false);
            return @event;
        }
        #endregion PrepareClosingAmountGen

        #region Valorize
        // EG 20180502 Analyse du code Correction [CA2214]
        //  EG 20180614 [XXXXX] Use pDbTransaction (GetAvailableQuantity|GetPreviousAvailableQuantity)
        // RD 20200911 [25475] Add try catch in order to Log the Exception
        public override Cst.ErrLevel Valorize()
        {
            Cst.ErrLevel ret = Cst.ErrLevel.SUCCESS;
            //FI 20101221 => Pour les strategies Spheres® ne fait rien
            //D'ailleurs il n'existe pas dans ce cas l'évènement de regroupement Linked to the Product Closing 
            if (m_tradeLibrary.DataDocument.CurrentProduct.IsStrategy)
                return ret;

            try
            {
                //Initialize();
                // EG 20150617 [20665]
                //InitializeDataSetEvent();

                if (null != Quote_ETDAsset && Quote_ETDAsset.IsQuoteTable_ETD)
                {
                    // RD 20110629
                    // Les événements de CashFlow sont revalorisés dans le cas d'une indication explicite: isCashFlowsVal = true
                    // Les événements de Closing (VariationMargin,UnrealizedMargin et LiquidationOptionValue) ne seront pas calculés,  
                    // dans le cas d'une mise à jour du contractMultiplier sur le DC ou sur l'ASSET, car on ne dispose pas de cotation
                    if (_isCashFlowsVal)
                        CashFlowValorisation();
                    return ret;
                }
                // EG 20170324 use m_IsPosKeeping_BookDealer
                if (m_IsPosKeeping_BookDealer)
                {
                    ETDQuoteInfo _quoteInfo = new ETDQuoteInfo(this, NextPreviousEnum.None);
                    ETDQuoteInfo _quotePrevInfo = new ETDQuoteInfo(this, NextPreviousEnum.Previous);

                    //Suppression d'un prix
                    if ((null != _quoteInfo) && (_quoteInfo.rowState == DataRowState.Deleted))
                    {
                        _quoteInfo.SetQuote(this);
                        RemoveClosingAmountGen(_quoteInfo.dtBusiness);
                    }
                    //                
                    //Modification ou ajout d'un prix ou création de trade (pas de prix)
                    if (_quoteInfo.rowState != DataRowState.Deleted)
                    {
                        bool isFirstQuotationFound = true;
                        if (false == IsEndOfDayProcess)
                        {
                            //Spheres® vérifie que la date de cotation ne correspond pas à un jour férié
                            _quoteInfo.BusinessDayControl(this);
                            isFirstQuotationFound = _quoteInfo.SetQuote(this);
                        }
                        else
                        {
                            _quoteInfo.InitQuote(this);
                        }

                    // EG 20151102 [20979] Refactoring
                    // EG 20170214 
                    m_PosAvailableQuantity = PosKeepingTools.GetAvailableQuantity(m_EventsValProcess.Cs, SlaveDbTransaction, _quoteInfo.dtBusiness.Date, m_EventsValProcess.CurrentId);

                    // EG 20170324 [22991]
                    if ((null != Quote_ETDAsset) && Quote_ETDAsset.eodComplementSpecified)
                        m_PosQuantityPrevAndActionsDay = Quote_ETDAsset.eodComplement.posQuantityPrevAndActionsDay;
                    else
                        m_PosQuantityPrevAndActionsDay = PosKeepingTools.GetPreviousAvailableQuantity(m_EventsValProcess.Cs, SlaveDbTransaction, _quoteInfo.dtBusiness.Date, m_EventsValProcess.CurrentId);


                        _isAssetMaster = IsAssetMaster(_quoteInfo);
                        if (_isAssetMaster)
                        {
                            // EG 20160208 [POC-MUREX] Refactoring
                            Calculation_LPC_AMT(_quoteInfo, _quotePrevInfo);

                            #region Calculations at ClearingDate+n <= DtMarket (HORS EOD)
                            bool isQuotationToCalc = (false == IsEndOfDayProcess);
                            int guard = 0;

                            DateTime dtQuotation = _quoteInfo.dtQuote;
                            ETDQuoteInfo _savQuoteInfo = (ETDQuoteInfo)_quoteInfo.Clone();
                            ETDQuoteInfo _quoteNextInfo = null;
                            while (isQuotationToCalc)
                            {
                                guard++;
                                if (guard == 999)
                                {
                                    string msgException = "Incoherence during the calculation. Infinite loop detected" + Cst.CrLf;
                                    throw new Exception(msgException);
                                }

                                _quoteNextInfo = new ETDQuoteInfo(this, NextPreviousEnum.Next, dtQuotation);
                                // EG 20170324 use m_EntityMarketInfo
                                isQuotationToCalc = (_quoteNextInfo.dtBusiness <= m_EntityMarketInfo.DtMarket);
                                if (isQuotationToCalc)
                                    isQuotationToCalc = _quoteNextInfo.SetQuote(this);

                                if (isQuotationToCalc)
                                {
                                    m_PosAvailableQuantityPrev = m_PosAvailableQuantity;
                                    // EG 20151102 [20979] Refactoring
                                    m_PosAvailableQuantity = PosKeepingTools.GetAvailableQuantity(m_EventsValProcess.Cs, SlaveDbTransaction, _quoteNextInfo.dtBusiness, m_EventsValProcess.CurrentId);
                                    Calculation_LPC_AMT(_quoteNextInfo, _savQuoteInfo);
                                }
                                if ((DataRowState.Modified == _quoteNextInfo.rowState) || (false == isQuotationToCalc))
                                    break;

                                    _savQuoteInfo = (ETDQuoteInfo)_quoteNextInfo.Clone();
                                    // EG 20150623 SET _quoteNextInfo.dtQuote to dtQuotation
                                    dtQuotation = _quoteNextInfo.dtQuote;
                            }
                            #endregion Calculations at ClearingDate+n <= DtMarket (HORS EOD)
                        }
                    }
                }

                // RD 20110629
                // Les événements de CashFlow sont revalorisés dans le cas d'une indication explicite: isCashFlowsVal = true
                // Ici on ne revalorise que les Events sur lesquels le Multiplier utilisé pour le calcul est différent de celui en vigeur sur l'Asset/DC
                // Les Events qui viennent d'être calculés, suite à l'arrivée d'une cotation, ne seront pas revalorisés, car ils viennent d'être calculés avec le bon Multiplier
                if (_isCashFlowsVal)
                    CashFlowValorisation();
            }
            catch (SpheresException2 ex)
            {
                Logger.Log(new LoggerData(ex));
                throw ex;
            }
            catch (Exception ex)
            {
                SpheresException2 sphEx = new SpheresException2(MethodInfo.GetCurrentMethod().Name, ex);
                Logger.Log(new LoggerData(sphEx));
                throw sphEx;
            }

            return ret;
        }
        #endregion Valorize
        #endregion Methods
    }
    #endregion EventsValProcessETD
}
