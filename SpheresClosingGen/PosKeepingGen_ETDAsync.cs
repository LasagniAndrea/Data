#region Using Directives
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
//
using EFS.ACommon;
using EFS.Actor;
using EFS.ApplicationBlocks.Data;
using EFS.ApplicationBlocks.Data.Extension;
using EFS.Common;
using EFS.Common.Log;
using EFS.Common.MQueue;
using EFS.GUI.Interface;
using EFS.LoggerClient;
using EFS.LoggerClient.LoggerService;
using EFS.Process.EventsGen;
using EFS.TradeInformation;
//
using EfsML.Business;
using EfsML.Enum;
using EfsML.Enum.Tools;
using EfsML.Interface;
using EfsML.v30.PosRequest;
//
using FixML.Enum;
using FixML.Interface;
using FixML.v50SP1.Enum;
//
using FpML.Enum;
using FpML.Interface;
#endregion Using Directives

namespace EFS.Process.PosKeeping
{
    #region MaturityOffsettingOptionData
    public class MaturityOffsettingOptionData
    {
        #region Members
        /// <summary>
        /// posRequest rattachée à la clé de position (MOO) 
        /// </summary>
        public IPosRequest PosRequest { set; get; }
        /// <summary>
        /// clé de position (Instrument - Dealer - Clearer - Actif) 
        /// </summary>
        public IPosKeepingKey PosKeepingKey { set; get; }
        /// <summary>
        /// Données nécessaire au traitement
        /// </summary>
        public IPosKeepingData PosKeepingData { set; get; }
        /// <summary>
        /// Cours de référence pour dénouement
        /// </summary>
        public Tuple<Cst.ErrLevel, SettlementPrice> SettlementPriceData { set; get; }
        /// <summary>
        /// Process 
        /// </summary>
        private PosKeepingGen_ETD Process { set; get; }
        /// <summary>
        /// Liste des PosRequest enfants (par trade) rattachée au posRequest (MOO)
        /// </summary>
        #endregion Members

        #region Accessors
        /// <summary>
        /// Actif dénoué
        /// </summary>
        public PosKeepingAsset_ETD Asset
        {
            get { return (PosKeepingAsset_ETD)PosKeepingData.Asset; }
        }
        /// <summary>
        /// Catégorie de l'actif
        /// </summary>
        public Cst.UnderlyingAsset AssetCategory
        {
            get
            {
                Cst.UnderlyingAsset assetCategory = Cst.UnderlyingAsset.Future;
                if (Asset.assetCategory_UnderlyerSpecified)
                    assetCategory = Asset.assetCategory_Underlyer;
                return assetCategory;
            }
        }
        /// <summary>
        /// Cours de référence
        /// </summary>
        public SettlementPrice SettlementPrice
        {
            get
            {
                return SettlementPriceData.Item2;
            }
        }
        #endregion Accessors

        #region Constructors
        public MaturityOffsettingOptionData(PosKeepingGen_ETD pProcess)
        {
            Process = pProcess;
        }
        #endregion Constructors

        #region GetRequestTypeAutomaticOption
        /// <summary>
        /// Détermination du type de dénouement en fonction de la règle suivante :
        ///<para> ► Call</para>
        ///<para>   ● Si STRIKE supérieur COURS(ASSET_UNL) alors : ABANDON (OTM)</para>
        ///<para>   ● Si STRIKE inférieur ou égal COURS(ASSET_UNL) alors : EXE (Si Achat) ASS (si Vente) (ITM)</para>
        ///   
        ///<para> ► Put</para>
        ///<para>   ● Si STRIKE supérieur ou égal COURS(ASSET_UNL) alors : EXE (Si Achat) ASS (si Vente) (ITM)</para>
        ///<para>   ● Si STRIKE inférieur COURS(ASSET_UNL)  alors : ABANDON (OTM)</para>
        /// <param name="pIsDealerBuyer">IFlag le donneur d'ordre est l'acheteur</param>
        /// <param name="pSettlementPrice">Cours de référence pour dénouement d'option</param>
        /// <returns></returns>
        public Cst.PosRequestTypeEnum GetRequestTypeAutomaticOption(bool pIsDealerBuyer, decimal pSettlementPrice)
        {
            Cst.PosRequestTypeEnum requestType = Cst.PosRequestTypeEnum.AutomaticOptionAbandon;


            // Pour les Options avec SSJ Future
            // Déterminer l'indicateur ITM/OTM en tenant compte de la base d'expression du prix de ce SSJ:
            // - opérer une conversion si la base diffère de 100 (ex. 32) 
            decimal strikePrice_UnderlyerBase = ExchangeTradedDerivativeTools.ConvertStrikeToUnderlyerBase(Asset.strikePrice, AssetCategory, Asset.instrumentNum_Underlyer, Asset.instrumentDen_Underlyer);

            // AutomaticOptionExercise or AutomaticOptionAssignment available when AtTheMoney
            Boolean isExeAss = false;
            MoneyPositionEnum moneyPosition = PosKeepingTools.GetMoneyPositionEnum(Asset.putCall, strikePrice_UnderlyerBase, pSettlementPrice);
            switch (moneyPosition)
            {
                case MoneyPositionEnum.OutOfTheMoney:
                    isExeAss = false;
                    break;
                case MoneyPositionEnum.InTheMoney:
                    isExeAss = true;
                    break;
                case MoneyPositionEnum.AtTheMoney:
                    switch (Asset.inTheMoneyCondition)
                    {
                        case ITMConditionEnum.OTM:
                            isExeAss = false;
                            break;
                        case ITMConditionEnum.ITM:
                            isExeAss = true;
                            break;
                        case ITMConditionEnum.ITMCall:
                            isExeAss = (Asset.putCall == PutOrCallEnum.Call);
                            break;
                        case ITMConditionEnum.ITMPut:
                            isExeAss = (Asset.putCall == PutOrCallEnum.Put);
                            break;
                    }
                    break;
                default:
                    break;
            }

            if (isExeAss)
                requestType = pIsDealerBuyer ? Cst.PosRequestTypeEnum.AutomaticOptionExercise : Cst.PosRequestTypeEnum.AutomaticOptionAssignment;
            return requestType;
        }
        #endregion GetRequestTypeAutomaticOption

        #region Initialize
        /// <summary>
        /// Initialisation des données utiles au traitement de
        /// dénouement automatique d'options à l'échéance
        /// </summary>
        /// <param name="pPosKeepingKey">Clé de position</param>
        /// <returns></returns>
        public Cst.ErrLevel Initialize(IPosKeepingKey pPosKeepingKey)
        {
            return Initialize(null, pPosKeepingKey);
        }
        /// <summary>
        /// Initialisation des données utiles au traitement de
        /// dénouement automatique d'options à l'échéance
        /// </summary>
        /// <param name="pDbTransaction">Transaction</param>
        /// <param name="pPosKeepingKey">Clé de position</param>
        /// <returns></returns>
        // EG 20190114 Add detail to ProcessLog Refactoring
        public Cst.ErrLevel Initialize(IDbTransaction pDbTransaction, IPosKeepingKey pPosKeepingKey)
        {
            Cst.ErrLevel ret = Cst.ErrLevel.SUCCESS;
            PosKeepingKey = pPosKeepingKey;

            PosKeepingData = Process.Product.CreatePosKeepingData();

            PosKeepingData.SetPosKey(Process.ProcessBase.Cs, pDbTransaction, pPosKeepingKey.IdI, pPosKeepingKey.UnderlyingAsset, pPosKeepingKey.IdAsset,
                                     pPosKeepingKey.IdA_Dealer, pPosKeepingKey.IdB_Dealer, pPosKeepingKey.IdA_Clearer, pPosKeepingKey.IdB_Clearer);

            PosKeepingData.SetAdditionalInfo(pPosKeepingKey.IdA_EntityDealer, pPosKeepingKey.IdA_EntityClearer);

            PosKeepingData.TradeSpecified = false;

            PosKeepingData.MarketSpecified = false;

            PosKeepingData.Asset = Process.GetAsset(pDbTransaction, pPosKeepingKey.UnderlyingAsset, pPosKeepingKey.IdAsset, Cst.PosRequestAssetQuoteEnum.UnderlyerAsset);
            if (null != PosKeepingData.Asset)
            {
                SetPosKeepingQuote(Process.Product.CreatePosRequestMaturityOffsetting(), Process);
                SetPosKeepingMarket();
                SetSettlementPriceData();
            }
            else
            {
                ret = Cst.ErrLevel.DATANOTFOUND;
                // FI 20200623 [XXXXX] call SetErrorWarning
                Process.ProcessBase.ProcessState.SetErrorWarning(ProcessStateTools.StatusErrorEnum);
                
                Logger.Log(new LoggerData(LogLevelEnum.Error, new SysMsgCode(SysCodeEnum.SYS, 5103), 0, new LogParam(pPosKeepingKey.IdAsset.ToString())));
            }
            return ret;
        }
        #endregion Initialize
        #region SetPosKeepingMarket
        /// <summary>
        /// Alimentation des données EntityMarket 
        /// </summary>
        private void SetPosKeepingMarket()
        {
            PosKeepingData.Market = Process.PKGenProcess.ProcessCacheContainer.GetEntityMarketLock(Process.MarketPosRequest.IdEM);
            PosKeepingData.MarketSpecified = (null != PosKeepingData.Market);
        }
        #endregion SetPosKeepingMarket
        #region SetPosKeepingQuote
        /// <summary>
        /// Lecture du sous-jacent exclusivement sur option
        /// </summary>
        /// <returns></returns>
        // EG 20190114 Add detail to ProcessLog Refactoring
        // EG 20190716 [VCL : New FixedIncome] Upd GetQuote
        // EG 20220627 [26082] Alimentation des données du trade pour logger
        protected void SetPosKeepingQuote(IPosRequest pPosRequest, PosKeepingGen_ETD pProcess)
        {
            Cst.ErrLevel errLevel = Cst.ErrLevel.SUCCESS;
            int idAsset = Asset.idAsset;
            string identifier = Asset.identifier;
            Cst.UnderlyingAsset assetCategory = Cst.UnderlyingAsset.ExchangeTradedContract;

            SystemMSGInfo errReadOfficialSettlement = null;
            SystemMSGInfo errReadOfficialClose = null;

            if ((pPosRequest is PosRequestMaturityOffsetting) ||
                (pPosRequest is PosRequestOption) ||
                (pPosRequest is PosRequestPhysicalPeriodicDelivery))
            {
                // Lecture du sous-jacent exclusivement sur option
                if (Asset.assetCategory_UnderlyerSpecified)
                    assetCategory = Asset.assetCategory_Underlyer;

                if (Asset.idAsset_UnderlyerSpecified && Asset.putCallSpecified)
                {
                    idAsset = Asset.idAsset_Underlyer;
                    identifier = Asset.identifier_Underlyer;
                }
                else if (Asset.putCallSpecified)
                {
                    // EG 20160302 (21969] Erreur Sous-jacent non défini sur le contrat
                    if (pPosRequest.IdTSpecified)
                    {
                        // EG 20220627 [26082] Alimentation des données du trade pour logger
                        string tradeIdentifier = Process.TradeForLog(PosKeepingData, pPosRequest);

                        // FI 20200623 [XXXXX] SetErrorWarning
                        pProcess.ProcessBase.ProcessState.SetErrorWarning(ProcessStateTools.StatusWarningEnum);
                        
                        
                        Logger.Log(new LoggerData(LogLevelEnum.Warning, new SysMsgCode(SysCodeEnum.SYS, 5109), 0,
                            new LogParam(LogTools.IdentifierAndId(identifier, idAsset)),
                            new LogParam(assetCategory),
                            new LogParam(tradeIdentifier)));
                    }
                    else
                    {
                        // FI 20200623 [XXXXX] SetErrorWarning
                        pProcess.ProcessBase.ProcessState.SetErrorWarning(ProcessStateTools.StatusWarningEnum);
                        
                        
                        Logger.Log(new LoggerData(LogLevelEnum.Warning, new SysMsgCode(SysCodeEnum.SYS, 5108), 0,
                            new LogParam(LogTools.IdentifierAndId(identifier, idAsset)),
                            new LogParam(assetCategory)));
                    }
                    errLevel = Cst.ErrLevel.DATANOTFOUND;
                }
            }

            if (Cst.ErrLevel.SUCCESS == errLevel)
            {
                bool isEOD = (pProcess.MasterPosRequest.RequestType == Cst.PosRequestTypeEnum.EndOfDay);

                Asset.SetFlagQuoteMandatory(pProcess.MasterPosRequest.DtBusiness, pPosRequest);

                DateTime dtQuote = Asset.GetSettlementQuoteTime(pProcess.MasterPosRequest.DtBusiness, pPosRequest);
                Quote quote = pProcess.PKGenProcess.ProcessCacheContainer.GetQuoteLock(idAsset, dtQuote, identifier, 
                    QuotationSideEnum.OfficialSettlement, assetCategory, new KeyQuoteAdditional(), ref errReadOfficialSettlement);
                if (null != quote)
                {
                    PosKeepingData.SetQuoteReference((Cst.UnderlyingAsset)Asset.assetCategory, quote, string.Empty);
                }

                quote = pProcess.PKGenProcess.ProcessCacheContainer.GetQuoteLock(idAsset, dtQuote, identifier, 
                    QuotationSideEnum.OfficialClose, assetCategory, new KeyQuoteAdditional(), ref errReadOfficialClose) as Quote;

                if (null != quote)
                {
                    PosKeepingData.SetQuote((Cst.UnderlyingAsset)Asset.assetCategory, quote, string.Empty);
                }

                // EG 20140120 Gestion status (ITD/EOD)
                if (Asset.isOfficialSettlementMandatory && (null != errReadOfficialSettlement))
                {
                    if (false == isEOD)
                        errReadOfficialSettlement.processState.Status = ProcessStateTools.StatusWarningEnum;

                    // FI 20200623 [XXXXX] SetErrorWarning
                    pProcess.PKGenProcess.ProcessState.SetErrorWarning(errReadOfficialSettlement.processState.Status);
                    
                    
                    Logger.Log(errReadOfficialSettlement.ToLoggerData(0));
                }
                else if (Asset.isOfficialCloseMandatory && (null != errReadOfficialClose))
                {
                    if (false == isEOD)
                    {
                        errReadOfficialClose.processState.Status = ProcessStateTools.StatusWarningEnum;
                    }
                    // FI 20200623 [XXXXX] SetErrorWarning
                    pProcess.PKGenProcess.ProcessState.SetErrorWarning(errReadOfficialSettlement.processState.Status);

                    
                    
                    Logger.Log(errReadOfficialClose.ToLoggerData(0));
                }
                else if ((null != errReadOfficialSettlement) && (null != errReadOfficialClose))
                {
                    if (false == isEOD)
                    {
                        errReadOfficialSettlement.processState.Status = ProcessStateTools.StatusWarningEnum;
                        errReadOfficialClose.processState.Status = ProcessStateTools.StatusWarningEnum;
                    }

                    // FI 20200623 [XXXXX] SetErrorWarning
                    pProcess.PKGenProcess.ProcessState.SetErrorWarning(errReadOfficialSettlement.processState.Status);
                    pProcess.PKGenProcess.ProcessState.SetErrorWarning(errReadOfficialClose.processState.Status);
                    
                    
                    Logger.Log(errReadOfficialSettlement.ToLoggerData(0));
                    Logger.Log(errReadOfficialClose.ToLoggerData(0));
                }
            }
        }
        #endregion SetPosKeepingQuote
        #region SetSettlementPriceData
        /// <summary>
        /// Lecture du cours de référence pour dénouement d'option
        /// </summary>
        protected void SetSettlementPriceData()
        {
            Cst.ErrLevel errLevel = Cst.ErrLevel.SUCCESS;
            SettlementPrice settlementPrice = new SettlementPrice(Asset.idAsset_Underlyer, Asset.identifier_Underlyer, DateTime.MinValue, 0, string.Empty,
                QuotationSideEnum.OfficialSettlement, QuoteTimingEnum.Close);

            IPosKeepingQuote quote = Asset.GetSettlementPrice(Process.MarketPosRequest.DtBusiness, ref errLevel);
            if (null != quote)
                settlementPrice = new SettlementPrice(quote);

            settlementPrice.price100 = PosKeepingData.ToBase100_UNL(settlementPrice.price);
            settlementPrice.price100 = PosKeepingData.VariableContractValue_UNL(settlementPrice.price100);

            SettlementPriceData = new Tuple<Cst.ErrLevel, SettlementPrice>(errLevel, settlementPrice);
        }
        #endregion SetSettlementPriceData
        #region UpdatePosRequest
        /// <summary>
        /// Mise à jour final du posRequest MOO
        /// </summary>
        public void UpdatePosRequest()
        {
            if (ProcessStateTools.IsStatusProgress(PosRequest.Status))
            {
                PosRequest.Status = Process.GetStatusGroupLevel(PosRequest.IdPR, ProcessStateTools.StatusUnknownEnum);
                PosRequest.StatusSpecified = true;
            }
            
            //PosKeepingTools.UpdatePosRequest(m_Process.ProcessBase.cs, null,
            //    posRequest.idPR, posRequest, m_Process.PKGenProcess.appInstance.IdA,
            //    m_Process.LogHeader.IdProcess, posRequest.idPR_PosRequest);
            PosKeepingTools.UpdatePosRequest(Process.ProcessBase.Cs, null,
                PosRequest.IdPR, PosRequest, Process.PKGenProcess.Session.IdA,
                Process.IdProcess, PosRequest.IdPR_PosRequest);
        }
        #endregion UpdatePosRequest
    }
    #endregion MaturityOffsettingOptionData
    #region SettlementPrice
    /// <summary>
    /// Cours de référence pour dénouement d'options
    /// </summary>
    public struct SettlementPrice
    {
        public int idAsset;
        public string assetIdentifier;
        public string source;
        public DateTime dtPrice;
        public decimal price;
        public decimal price100;
        public QuotationSideEnum quoteSide;
        public QuoteTimingEnum quoteTiming;

        public SettlementPrice(IPosKeepingQuote pQuote) :
            this(pQuote.IdAsset, pQuote.Identifier, pQuote.QuoteTime, pQuote.QuotePrice, pQuote.Source,
            pQuote.QuoteSide, pQuote.QuoteTiming) { }
        public SettlementPrice(int pIdAsset, string pAssetIdentifier, DateTime pDtPrice, decimal pPrice, string pSource,
            QuotationSideEnum pQuoteSide, QuoteTimingEnum pQuoteTiming)
        {
            idAsset = pIdAsset;
            assetIdentifier = pAssetIdentifier;
            dtPrice = pDtPrice;
            price = pPrice;
            price100 = pPrice;
            source = pSource;
            quoteSide = pQuoteSide;
            quoteTiming = pQuoteTiming;

        }
    }
    #endregion SettlementPrice

    #region TradeOptionData
    public class TradeOptionData : TradeData
    {
        #region Members
        private SettlementPrice m_SettlementPrice;
        #endregion Members
        #region Accessors
        #region Asset
        public PosKeepingAsset_ETD Asset
        {
            get { return (PosKeepingAsset_ETD)m_PosKeepingData.Asset; }
        }
        #endregion Asset
        #region ExchangeTradedDerivativeContainer
        // EG 20240115 [WI808] Update to public and add setter
        public ExchangeTradedDerivativeContainer ExchangeTradedDerivativeContainer
        {
            get { return m_RptSideProductContainer as ExchangeTradedDerivativeContainer; }
            set { m_RptSideProductContainer = value; }
        }
        #endregion ExchangeTradedDerivativeContainer
        #region GetDeliveryDate
        /// <summary>
        /// Insertion d'une Date DLY sur Exercise/Assignment
        /// DATE SOURCE (de BASE) : DTBUSINESS
        /// ─────────────────────────────────────────────────────────────────────────────────────────
        /// ► Cas 1 : Dénouement ou Liquidation à l'échéance (DTBUSINESS = MATURITY.MATURITYDATE)
        /// ─────────────────────────────────────────────────────────────────────────────────────────
        ///   ● EGALE A la date de livraison (DLY) de cette échéance (MATURITY.DELIVERYDATE) 
        ///   ou si celle-ci n'est pas renseignée
        ///   ● CALCULE PAR application de l'offset spécifié dans la règle d'échéance associée 
        ///     (maturityRule.PERIODMLTPDELIVERYDATEOFFSET, maturityRule.PERIODDELIVERYDATEOFFSET, 
        ///     maturityRule.DAYTYPEDELIVERYDATEOFFSET)
        ///   ou si celui-ci n'est pas renseigné
        ///   ● EGALE A la date d'échéance (MATURITY.MATURITYDATE ou DTBUSINESS)
        /// ─────────────────────────────────────────────────────────────────────────────────────────
        /// ► Cas 2 : Dénouement anticipé (manuel)(DTBUSINESS inférieur ou égale MATURITY.MATURITYDATE)
        /// ─────────────────────────────────────────────────────────────────────────────────────────
        ///   ● CALCULE PAR application de l'offset spécifié dans la règle d'échéance associée 
        ///     (maturityRule.PERIODMLTPDELIVERYDATEOFFSET, maturityRule.PERIODDELIVERYDATEOFFSET, 
        ///     maturityRule.DAYTYPEDELIVERYDATEOFFSET)
        ///   ou si celui-ci n'est pas renseigné
        ///   ● EGALE A la date de dénouement (DTBUSINESS)
        ///   
        /// </summary>
        /// <param name="pDbTransaction">Transaction</param>
        /// <returns></returns>
        private DateTime GetDeliveryDate(DateTime pDtBusiness)
        {
            DateTime deliveryDate = pDtBusiness;
            if (Asset.settlMethod == SettlMethodEnum.PhysicalSettlement)
            {
                if ((pDtBusiness == Asset.maturityDate) && Asset.deliveryDateSpecified)
                {
                    deliveryDate = Asset.deliveryDate;
                }
                else if (Asset.deliveryDelayOffsetSpecified)
                {
                    IBusinessDayAdjustments bda = m_Product.CreateBusinessDayAdjustments(BusinessDayConventionEnum.FOLLOWING, Asset.idBC);
                    deliveryDate = Tools.ApplyOffset(m_Cs, deliveryDate, Asset.deliveryDelayOffset, bda, null);
                }
            }
            return deliveryDate;
        }
        #endregion GetDeliveryDate
        #region IsRequestOptionAbandon
        // RD 20210906 [25803] PosRequestTypeEnum : Add NEX (OptionNotExercised) & NAS (OptionNotAssigned)
        protected bool IsRequestOptionAbandon
        {
            get
            {
                return (null != posRequest) &&
                       ((posRequest.RequestType == Cst.PosRequestTypeEnum.AutomaticOptionAbandon) ||
                        (posRequest.RequestType == Cst.PosRequestTypeEnum.OptionAbandon) ||
                        (posRequest.RequestType == Cst.PosRequestTypeEnum.OptionNotExercised) ||
                        (posRequest.RequestType == Cst.PosRequestTypeEnum.OptionNotAssigned));
            }
        }
        #endregion IsRequestOptionAbandon
        #region OptionEventCode
        // RD 20210906 [25803] PosRequestTypeEnum : Add NEX (OptionNotExercised) & NAS (OptionNotAssigned)
        private string OptionEventCode
        {
            get
            {
                string eventCode = string.Empty;
                switch (posRequest.RequestType)
                {
                    case Cst.PosRequestTypeEnum.OptionAbandon:
                    case Cst.PosRequestTypeEnum.OptionNotExercised:
                    case Cst.PosRequestTypeEnum.OptionNotAssigned:
                        eventCode = EventCodeFunc.Abandon;
                        break;
                    case Cst.PosRequestTypeEnum.OptionAssignment:
                        eventCode = EventCodeFunc.Assignment;
                        break;
                    case Cst.PosRequestTypeEnum.OptionExercise:
                        eventCode = EventCodeFunc.Exercise;
                        break;
                    case Cst.PosRequestTypeEnum.AutomaticOptionAbandon:
                        eventCode = EventCodeFunc.AutomaticAbandon;
                        break;
                    case Cst.PosRequestTypeEnum.AutomaticOptionAssignment:
                        eventCode = EventCodeFunc.AutomaticAssignment;
                        break;
                    case Cst.PosRequestTypeEnum.AutomaticOptionExercise:
                        eventCode = EventCodeFunc.AutomaticExercise;
                        break;
                }
                return eventCode;
            }
        }
        #endregion OptionEventCode
        #region Process
        public PosKeepingGen_ETD Process
        {
            get { return (PosKeepingGen_ETD)m_Process; }
        }
        #endregion Process
        #endregion Accessors
        #region Constructors
        // EG 20240115 [WI808] New constructor : Harmonisation et réunification des méthodes
        // EG 20240627 [OTRS1000274] Intraday Assignment |Exercise by position failled when quotation is missing
        public TradeOptionData(PosKeepingGen_ETD pProcess, IPosRequest pPosRequest, IPosKeepingData pPosKeepingData,
            (EFS_TradeLibrary pTradeLibrary, DataTable Position, DataTable PosActionDet) pData)
            : base(pProcess, pPosRequest, pPosKeepingData, pData)
        {
            Nullable<QuoteTimingEnum> quoteTiming = null;
            Nullable<QuotationSideEnum> quoteSide = null;
            Nullable<decimal> settlementPrice = null;
            Nullable<decimal> settlementPrice100 = null;
            Nullable<DateTime> dtQuote = null;

            pProcess.GetQuoteOfAsset(ref dtQuote, ref quoteTiming, ref quoteSide, ref settlementPrice, ref settlementPrice100);
            // EG 20240627 [OTRS1000274] Move IsQuoteOk here (Last version is before calling the Constructor)
            if (IsQuoteOk)
            {
                m_SettlementPrice = new SettlementPrice()
                {
                    dtPrice = dtQuote.Value,
                    quoteTiming = quoteTiming.Value,
                    quoteSide = quoteSide.Value,
                    price = settlementPrice.Value,
                    price100 = settlementPrice100.Value,
                };
            }
            else
            {
                // EG 20240627 [OTRS1000274] Initialisation de la date pour insertion du dénouEment même si prix non trouvé (:-()
                m_SettlementPrice = new SettlementPrice()
                {
                    dtPrice = MasterDtBusiness,
                };
            }
        }
        public TradeOptionData(PosKeepingGen_ETD pProcess, MaturityOffsettingOptionData pMaturityOffsettingOptionData, IPosRequest pPosRequest)
            : base(pProcess, pPosRequest, pMaturityOffsettingOptionData.PosKeepingData, pMaturityOffsettingOptionData.PosKeepingKey)
        {
            m_SettlementPrice = pMaturityOffsettingOptionData.SettlementPriceData.Item2;
        }
        #endregion Constructors
        #region Methods

        #region DeserializeTrade
        public override void DeserializeTrade(IDbTransaction pDbTransaction)
        {
            base.DeserializeTrade(pDbTransaction);
            IExchangeTradedDerivative etd = (IExchangeTradedDerivative)m_TradeLibrary.Product;
            m_RptSideProductContainer = new ExchangeTradedDerivativeContainer(Process.ProcessBase.Cs, pDbTransaction, etd);
        }
        #endregion DeserializeTrade
        #region GetDtSettlement
        /// <summary>
        /// Obtient la date de règlement (DtBusiness + 1JO)
        /// </summary>
        private DateTime GetDtSettlement()
        {
            return Tools.ApplyOffset(m_Cs, m_Product, MasterDtBusiness, 1, DayTypeEnum.ExchangeBusiness, Asset.idBC, null);
        }
        #endregion DtSettlement
        #region GetPositionTradeOption
        // EG 20201124 [XXXXX] Utilisation de la table temporaire construite auparavant en lieu et place de TRADE
        private DataSet GetPositionTradeOption(IDbTransaction pDbTransaction, DateTime pDate)
        {
            DataParameters parameters = new DataParameters();
            parameters.Add(new DataParameter(m_Cs, "IDT", DbType.Int32), idT);
            parameters.Add(DataParameter.GetParameter(m_Cs, DataParameter.ParameterEnum.DTBUSINESS), pDate);// FI 20201006 [XXXXX] DbType.Date
            Nullable<int> idPR = GetIdPR();
            if (idPR.HasValue)
                parameters.Add(new DataParameter(m_Cs, "IDPR", DbType.Int32), idPR.Value);

            string tableName = StrFunc.AppendFormat("TRADE_MOO_{0}_W", Process.PKGenProcess.Session.BuildTableId()).ToUpper();

            string sqlSelect = @"select tr.IDT, tr.IDENTIFIER, (tr.QTY - isnull(pab.QTY, 0) - isnull(pas.QTY, 0)) as QTY,
            tr.IDA_ENTITY, tr.IDEM, tr.IDA_CSSCUSTODIAN, tr.IDI, tr.IDASSET, tr.DTMARKET, tr.DTENTITY, tr.PRICE, tr.DTTRADE, tr.IDA_DEALER, tr.IDB_DEALER, tr.IDA_ENTITYDEALER, 
            tr.IDA_CLEARER, tr.IDB_CLEARER, tr.IDA_ENTITYCLEARER, tr.SIDE, tr.ISAUTOABN, ev.IDE_EVENT, ec.EVENTCLASS, ec.DTEVENT as EXPIRYDATE, tr.ASSETCATEGORY, 0 as ISCUSTODIAN
            from dbo.{0} tr
            inner join dbo.EVENT ev on (ev.IDT = tr.IDT) and (ev.EVENTCODE in ('ASD','EXD'))
            inner join dbo.EVENTCLASS ec on (ec.IDE = ev.IDE)
            left outer join (" + PosKeepingTools.GetQryPosAction_Trade_BySide(BuyerSellerEnum.BUYER, true, idPR.HasValue) + @") pab on (pab.IDT = tr.IDT)
            left outer join (" + PosKeepingTools.GetQryPosAction_Trade_BySide(BuyerSellerEnum.SELLER, true, idPR.HasValue) + @") pas on (pas.IDT = tr.IDT)
            where ((tr.QTY - isnull(pab.QTY, 0) - isnull(pas.QTY, 0)) >0) and (tr.IDT = @IDT)" + Cst.CrLf;
            sqlSelect += SQLCst.SEPARATOR_MULTISELECT;
            sqlSelect += @"select pa.IDPA, pad.IDPADET, pad.IDT_BUY, pad.IDT_SELL, pad.IDT_CLOSING, pad.QTY, pad.POSITIONEFFECT, tr.SIDE as SIDE_CLOSING
            from dbo.POSACTION pa 
            inner join dbo.POSACTIONDET pad on (pad.IDPA = pa.IDPA) and ((pad.DTCAN is null) or (pad.DTCAN > @DTBUSINESS))
            inner join dbo.{0} tr on (tr.IDT = pad.IDT_CLOSING)
            where (pad.IDT_CLOSING = @IDT)";
            if (idPR.HasValue)
                sqlSelect += "and (isnull(pa.IDPR,0) <> @IDPR)" + Cst.CrLf;

            sqlSelect = String.Format(sqlSelect, tableName);
            //string sqlSelect = Process.GetQueryPositionTradeOptionAndAction(idPR.HasValue)
            QueryParameters qryParam = new QueryParameters(m_Cs, sqlSelect, parameters);

            DataSet ds;
            if (null != pDbTransaction)
            {
                ds = DataHelper.ExecuteDataset(pDbTransaction, CommandType.Text, qryParam.Query, qryParam.Parameters.GetArrayDbParameter());
            }
            else
            {
                // PL 20180312 WARNING: Use Read Commited !
                //ds = OTCmlHelper.GetDataSetWithIsolationLevel(m_Cs, IsolationLevel.ReadUncommitted, qryParam, 480);
                ds = OTCmlHelper.GetDataSetWithIsolationLevel(m_Cs, IsolationLevel.ReadCommitted, qryParam, 480);
            }
            // ORIGINALQTY_CLOSED|ORIGINALQTY_CLOSING added after Dataset construction
            ds.Tables[1].Columns.Add("ORIGINALQTY_CLOSED", typeof(decimal));
            ds.Tables[1].Columns.Add("ORIGINALQTY_CLOSING", typeof(decimal));
            return ds;
        }
        #endregion GetPositionTradeOption

        #region InsertEventDet
        // EG 20190730 Add TypePrice parameter
        private Cst.ErrLevel InsertEventDet(IDbTransaction pDbTransaction, int pIdE, decimal pQty, decimal pContractMultiplier,
            Nullable<decimal> pPrice, Nullable<decimal> pClosingPrice)
        {
            // EG 20141128 [20520] Nullable<decimal>
            Nullable<decimal> price100 = null;
            Nullable<decimal> closingPrice100 = null;

            if (pPrice.HasValue)
            {
                price100 = m_PosKeepingData.ToBase100(pPrice.Value);
                price100 = m_PosKeepingData.VariableContractValue(price100);
            }
            if (pClosingPrice.HasValue)
            {
                closingPrice100 = m_PosKeepingData.ToBase100(pClosingPrice);
                closingPrice100 = m_PosKeepingData.VariableContractValue(closingPrice100);
            }
            Cst.ErrLevel codeReturn = EventQry.InsertEventDet_Closing(pDbTransaction, pIdE, pQty, pContractMultiplier, null, pPrice, price100, pClosingPrice, closingPrice100);
            return codeReturn;
        }
        #endregion InsertEventDet
        #region InsertOptionEvents
        /// <summary>
        /// <para>Insertion d'un package Evénement : Dénouement d'options</para>
        /// <para>Cas 1 : EXERCISE</para>
        /// <para>─────────────────────────────────────────────</para>
        /// <para>Evénement           Code    Type        Class</para>
        /// <para>─────────────────────────────────────────────</para>
        /// <para>TOTAL EXERCISE      EXE     TOT         GRP</para>
        /// <para>CASH SETTLEMENT     TER     SCU         REC/STL</para>
        /// <para>─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ </para>
        /// <para>PARTIAL EXERCISE    EXE     PAR         GRP</para>
        /// <para>CASH SETTLEMENT     INT     SCU         REC/STL</para>
        /// <para>PARTIAL ABANDON     ABN     PAR         GRP (si AbandonRemainingQty = true)</para>
        /// <para></para>
        /// <para>Cas 2 : ASSIGNMENT</para>
        /// <para>─────────────────────────────────────────────</para>
        /// <para>Evénement           Code    Type        Class</para>
        /// <para>─────────────────────────────────────────────</para>
        /// <para>TOTAL ASSIGNMENT    ASS     TOT         GRP</para>
        /// <para>CASH SETTLEMENT     TER     SCU         REC/STL</para>
        /// <para>─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ </para>
        /// <para>PARTIAL ASSIGNMENT  ASS     PAR         GRP</para>
        /// <para>CASH SETTLEMENT     INT     SCU         REC/STL</para>
        /// <para>PARTIAL ABANDON     ABN     PAR         GRP (si AbandonRemainingQty = true)</para>
        /// <para>Cas 3 : ABANDON</para>
        /// <para>─────────────────────────────────────────────</para>
        /// <para>Evénement           Code    Type        Class</para>
        /// <para>─────────────────────────────────────────────</para>
        /// <para>TOTAL ABANDON       ABN     TOT         GRP</para>
        /// <para>─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ </para>
        /// <para>PARTIAL ABANDON     ABN     PAR         GRP</para>
        /// </summary>
        /// <param name="pDbTransaction">Current Transaction</param>
        /// <param name="pRow">ClosingTrade</param>
        /// <param name="pActionQuantity"></param>
        /// <param name="pIdE"></param>
        /// <returns></returns>
        private Cst.ErrLevel InsertOptionEvents(IDbTransaction pDbTransaction, DataRow pRow, DataRow pRowPosActionDet, int pIdPADET, ref int pIdE)
        {

            #region Variables
            int idT = Convert.ToInt32(pRow["IDT"]);
            int idE = pIdE;
            int idE_Event = Convert.ToInt32(pRow["IDE_EVENT"]);
            decimal originalClosingQuantity = Convert.ToDecimal(pRowPosActionDet["ORIGINALQTY_CLOSING"]);
            decimal qty = Convert.ToDecimal(pRowPosActionDet["QTY"]);
            decimal remainQuantity = originalClosingQuantity - qty;
            bool isDealerBuyer = m_Process.IsTradeBuyer(pRow["SIDE"].ToString());
            string eventClass = pRow["EVENTCLASS"].ToString();
            decimal price = Convert.ToDecimal(pRow["PRICE"]);
            #endregion Variables

            #region EVENT ABANDON/ASSIGNMENT/EXERCISE
            IPosRequestDetOption detail = posRequest.DetailBase as IPosRequestDetOption;
            StatusCalculEnum idStCalc = IsQuoteOk ? StatusCalculEnum.CALC : StatusCalculEnum.TOCALC;
            string eventCode = OptionEventCode;
            string eventType = (remainQuantity == 0) ? EventTypeFunc.Total : EventTypeFunc.Partiel;

            Cst.ErrLevel codeReturn = EventQry.InsertEvent(pDbTransaction, idT, idE, idE_Event, null, 1, 1, null, null, null, null,
    eventCode, eventType, MasterDtBusiness, MasterDtBusiness, MasterDtBusiness, MasterDtBusiness, qty, null, UnitTypeEnum.Qty.ToString(), idStCalc, null);

            if (Cst.ErrLevel.SUCCESS == codeReturn)
                codeReturn = EventQry.InsertEventClass(pDbTransaction, pIdE, eventClass, MasterDtBusiness, false);

            // Insertion d'une Date DLY sur Exercise/Assignment
            if ((Cst.ErrLevel.SUCCESS == codeReturn) &&
                (false == EventCodeFunc.IsAbandon(eventCode)) &&
                (false == EventCodeFunc.IsAutomaticAbandon(eventCode)))
            {
                if (Asset.settlMethod == SettlMethodEnum.PhysicalSettlement)
                {
                    DateTime deliveryDate = GetDeliveryDate(MasterDtBusiness);
                    codeReturn = EventQry.InsertEventClass(pDbTransaction, idE, EventClassFunc.DeliveryDelay, deliveryDate, false);
                }
            }
            if (Cst.ErrLevel.SUCCESS == codeReturn)
            {
                // EG 20240627 [OTRS1000274] Insertion FXG même si prix non trouvé (:-()
//                if (IsQuoteOk)
//                {
                    if (m_SettlementPrice.dtPrice != DateTime.MinValue)
                        codeReturn = EventQry.InsertEventClass(pDbTransaction, idE, EventClassFunc.Fixing,
                            m_SettlementPrice.dtPrice, false);
//                }
                if (Cst.ErrLevel.SUCCESS == codeReturn)
                {
                    codeReturn = EventQry.InsertEventDet_Denouement(pDbTransaction, idE, qty, null,
                        detail.StrikePrice,
                        m_SettlementPrice.quoteSide, m_SettlementPrice.quoteTiming, m_SettlementPrice.dtPrice,
                        m_SettlementPrice.price, m_SettlementPrice.price100, posRequest.Notes);
                }
            }

            if (Cst.ErrLevel.SUCCESS == codeReturn)
            {
                #region Insertion EVENTASSET avec info du sous-jacent
                if ((Cst.ErrLevel.SUCCESS == codeReturn) && (detail.UnderlyerSpecified))
                {
                    if ((0 == detail.Underlyer.IdAsset) && Asset.idAsset_UnderlyerSpecified)
                    {
                        detail.Underlyer.IdAsset = Asset.idAsset_Underlyer;
                        detail.Underlyer.Identifier = Asset.identifier_Underlyer;
                        detail.Underlyer.AssetCategory = Asset.assetCategory_Underlyer;
                    }
                    EFS_Asset efs_Asset = detail.Underlyer.GetCharacteristics(m_Cs, pDbTransaction);
                    codeReturn = EventQuery.InsertEventAsset(pDbTransaction, idE, efs_Asset);
                }
                #endregion Insertion EVENTASSET avec info du sous-jacent
            }

            if (Cst.ErrLevel.SUCCESS == codeReturn)
                codeReturn = EventQuery.InsertEventPosActionDet(pDbTransaction, pIdPADET, idE);

            #endregion EVENT ABANDON/ASSIGNMENT/EXERCISE

            if (Cst.ErrLevel.SUCCESS == codeReturn)
            {
                #region EVENT NOM/QTY (Nominal/Quantité)
                idE_Event = idE;
                idE++;

                codeReturn = InsertNominalQuantityEvent(pDbTransaction, idT, ref idE, idE_Event, MasterDtBusiness, isDealerBuyer, qty, remainQuantity, pIdPADET);
                #endregion EVENT NOM/QTY (Nominal/Quantité)
            }


            // EG 20240115 [WI808] ExchangeTradedDerivativeContainer
            if ((Cst.ErrLevel.SUCCESS == codeReturn) && ExchangeTradedDerivativeContainer.IsFuturesStyleMarkToMarket)
            {
                #region VMG
                idE++;

                codeReturn = InsertVariationMarginEvent(pDbTransaction, idT, idE, idE_Event, isDealerBuyer, price, qty, m_RptSideProductContainer.ClearingBusinessDate);

                if (Cst.ErrLevel.SUCCESS == codeReturn)
                    codeReturn = EventQuery.InsertEventPosActionDet(pDbTransaction, pIdPADET, idE);
                #endregion VMG
            }

            if (Cst.ErrLevel.SUCCESS == codeReturn)
            {
                #region RMG
                idE++;
                // SI RMG >=0 Le payeur est l'acheteur de l'opération dénouée
                // SI RMG <0  Le payeur est le vendeur de l'opération dénouée
                codeReturn = InsertRealizedMarginEvent(pDbTransaction, idT, ref idE, idE_Event, isDealerBuyer, price, 0, qty, pIdPADET, true);
                #endregion RMG
            }


            if ((Cst.ErrLevel.SUCCESS == codeReturn) &&
                (false == EventCodeFunc.IsAbandon(eventCode)) &&
                (false == EventCodeFunc.IsAutomaticAbandon(eventCode)) &&
                EventClassFunc.IsCashSettlement(eventClass))
            {
                #region EVENT SCU (Cash Settlement sur Exercice/Assignation)
                /// EG 20140121 Homologuation
                idE++;
                codeReturn = InsertSettlementCurrencyEvent(pDbTransaction, idT, idE, idE_Event, isDealerBuyer, qty, remainQuantity, pIdPADET);
                #endregion EVENT SCU (Cash Settlement)
            }
            pIdE = idE;
            return codeReturn;
        }
        #endregion InsertOptionEvents
        #region InsertPosActionDetAndOptionLinkedEvent
        /// <summary>
        /// Insertion des éléments matérialisant la clôture dans POSACTIONDET/EVENTPOSACTIONDET/EVENT
        /// pour les dénouement d'options
        /// ATTENTION la colonne IDT_CLOSING = Négociation traitée soit:
        /// (IDT_CLOSING = IDT_BUY et IDT_SELL = null) ou (IDT_CLOSING = IDT_SELL et IDT_BUY = null)
        /// </summary>
        /// <param name="pDbTransaction">Transaction</param>
        /// <param name="pIdPA">Identifiant POSACTION</param>
        /// <param name="pIdPADET">Identifiant POSACTIONDET</param>
        /// <param name="pIdE">Identifiant EVENT</param>
        /// <param name="pRowPosActionDet">Nouvelles données de clôture</param>
        /// <param name="pRowClosing">Données de la négociation clôturante</param>
        /// <returns></returns>
        private Cst.ErrLevel InsertPosActionDetAndOptionLinkedEvent(IDbTransaction pDbTransaction, int pIdPA, int pIdPADET, ref int pIdE, 
            DataRow pRowPosActionDet, DataRow pRowClosing)
        {
            // POSACTIONDET
            Nullable<int> idT_Buy = null;
            if (false == Convert.IsDBNull(pRowPosActionDet["IDT_BUY"]))
                idT_Buy = Convert.ToInt32(pRowPosActionDet["IDT_BUY"]);
            Nullable<int> idT_Sell = null;
            if (false == Convert.IsDBNull(pRowPosActionDet["IDT_SELL"]))
                idT_Sell = Convert.ToInt32(pRowPosActionDet["IDT_SELL"]);
            int idT_Closing = Convert.ToInt32(pRowPosActionDet["IDT_CLOSING"]);
            // EG 20150920 [21374] Int (int32) to Long (Int64) 
            // EG 20170127 Qty Long To Decimal
            decimal qty = Convert.ToDecimal(pRowPosActionDet["QTY"]);
            string positionEffect = String.Empty;
            // EG 20141205 PositionEffect
            if (false == Convert.IsDBNull(pRowPosActionDet["POSITIONEFFECT"]))
                positionEffect = pRowPosActionDet["POSITIONEFFECT"].ToString();

            Cst.ErrLevel codeReturn = m_Process.InsertPosActionDet(pDbTransaction, pIdPA, pIdPADET, idT_Buy, idT_Sell, idT_Closing, qty, positionEffect);

            // EVENT CLOTURANTE
            if (Cst.ErrLevel.SUCCESS == codeReturn)
                codeReturn = InsertPosKeepingOptionEvents(pDbTransaction, pRowClosing, pRowPosActionDet, pIdPADET, ref pIdE);
            return codeReturn;
        }
        #endregion InsertPosActionDetAndOptionLinkedEvent
        #region InsertPosKeepingOptionEvents
        /// <summary>
        /// Application de la clôture sur les événements suite à dénouement d'options
        /// </summary>
        /// <returns></returns>
        private Cst.ErrLevel InsertPosKeepingOptionEvents(IDbTransaction pDbTransaction, DataRow pRow, DataRow pRowPosActionDet, int pIdPADET, ref int pIdE)
        {
            int idEParent = pIdE;
            #region EVENT (ABN/ASS/EXE)
            Cst.ErrLevel codeReturn = InsertOptionEvents(pDbTransaction, pRow, pRowPosActionDet, pIdPADET, ref pIdE);
            #endregion EVENT (ABN/ASS/EXE)

            //// Insertion des frais
            if (Cst.ErrLevel.SUCCESS == codeReturn)
            {
                #region EVENT (OPP)
                IPosRequestDetail detail = posRequest.DetailBase as IPosRequestDetail;
                if (detail.FeeCalculationSpecified && detail.FeeCalculation)
                {
                    Cst.Capture.ModeEnum mode = Process.ConvertExeAssAbnRequestTypeToCaptureMode(posRequest.RequestType);
                    User user = new User(Process.PKGenProcess.Session.IdA, null, RoleActor.SYSADMIN);
                    TradeInput tradeInput = new TradeInput();
                    tradeInput.SearchAndDeserializeShortForm(m_Cs, pDbTransaction, posRequest.IdT.ToString(), SQL_TableWithID.IDType.Id, user, Process.PKGenProcess.Session.SessionId);
                    tradeInput.tradeDenOption = new TradeDenOption();
                    tradeInput.tradeDenOption.InitShortForm(MasterDtBusiness, posRequest.Qty);
                    IPayment[] opp = PosKeepingGen_ETD.DenOptionCalcFee(CSTools.SetCacheOn(m_Cs), pDbTransaction,  tradeInput, mode, false);
                    detail.PaymentFeesSpecified = ArrFunc.IsFilled(opp);
                    if (ArrFunc.IsFilled(opp))
                    {
                        IExchangeTradedDerivative etd = (IExchangeTradedDerivative)tradeInput.CurrentTrade.Product;
                        ExchangeTradedDerivativeContainer etdContainer = new ExchangeTradedDerivativeContainer(etd, tradeInput.DataDocument);
                        etdContainer.Efs_ExchangeTradedDerivative = new EFS_ExchangeTradedDerivative(m_Cs, pDbTransaction, etd, etdContainer.DataDocument, Cst.StatusBusiness.ALLOC, null);
                        detail.PaymentFeesSpecified = true;
                        detail.PaymentFees = opp;
                        foreach (IPayment payment in detail.PaymentFees)
                        {
                            if (null == payment.Efs_Payment)
                                payment.Efs_Payment = new EFS_Payment(m_Cs, pDbTransaction,  payment, tradeInput.Product.Product, etdContainer.DataDocument);
                        }
                    }
                }
                if (detail.PaymentFeesSpecified)
                {
                    foreach (IPayment payment in detail.PaymentFees)
                    {
                        pIdE++;
                        int savIdE = pIdE;
                        codeReturn = EventQry.InsertPaymentEvents(pDbTransaction, m_TradeLibrary.DataDocument, Convert.ToInt32(pRow["IDT"]),
                        payment, MasterDtBusiness, 1, 1, ref pIdE, idEParent);

                        // Add EVENTPOSACTIONDET pour toutes les lignes de FRAIS
                        if (Cst.ErrLevel.SUCCESS == codeReturn)
                        {
                            for (int i = savIdE; i <= pIdE; i++)
                            {
                                codeReturn = EventQuery.InsertEventPosActionDet(pDbTransaction, pIdPADET, i);
                                if (Cst.ErrLevel.SUCCESS != codeReturn)
                                    break;
                            }
                        }
                        if (Cst.ErrLevel.SUCCESS != codeReturn)
                            break;
                    }
                }
                #endregion EVENT (OPP)
            }
            return codeReturn;
        }
        #endregion InsertPosKeepingOptionEvents
        #region InsertRealizedMarginEvent
        protected Cst.ErrLevel InsertRealizedMarginEvent(IDbTransaction pDbTransaction, int pIdT, ref int pIdE, int pIdE_Event,
            bool pIsDealerBuyer, Nullable<decimal> pPrice, Nullable<decimal> pClosingPrice, decimal pQty, int pIdPADET, bool pIsClosed)
        {

            #region EVENT RMG (Realized margin)
            Nullable<decimal> realizedMargin = m_PosKeepingData.RealizedMargin(pClosingPrice, pPrice, pQty);
            PayerReceiverAmountInfo _payrec = new PayerReceiverAmountInfo(m_PosKeepingData, EventTypeFunc.RealizedMargin, pIsDealerBuyer);
            _payrec.SetPayerReceiver(realizedMargin);
            Cst.ErrLevel codeReturn = EventQry.InsertEvent(pDbTransaction, pIdT, pIdE, pIdE_Event, null, 1, 1,
            _payrec.IdA_Payer, _payrec.IdB_Payer, _payrec.IdA_Receiver, _payrec.IdB_Receiver,
            EventCodeFunc.LinkedProductClosing, EventTypeFunc.RealizedMargin, MasterDtBusiness, MasterDtBusiness, MasterDtBusiness, MasterDtBusiness,
            realizedMargin, Asset.currency, UnitTypeEnum.Currency.ToString(), null, null);

            if (Cst.ErrLevel.SUCCESS == codeReturn)
                codeReturn = EventQry.InsertEventClass(pDbTransaction, pIdE, EventClassFunc.Recognition, MasterDtBusiness, false);

            // EG 20150616 [21124]
            if (Cst.ErrLevel.SUCCESS == codeReturn)
                codeReturn = EventQry.InsertEventClass(pDbTransaction, pIdE, EventClassFunc.ValueDate, MasterDtBusiness, false);

            if (Cst.ErrLevel.SUCCESS == codeReturn)
            {
                codeReturn = InsertEventDet(pDbTransaction, pIdE, pQty, Asset.contractMultiplier, pIsClosed ? pPrice : pClosingPrice, pIsClosed ? pClosingPrice : pPrice);
            }
            if (Cst.ErrLevel.SUCCESS == codeReturn)
                codeReturn = EventQuery.InsertEventPosActionDet(pDbTransaction, pIdPADET, pIdE);
            #endregion EVENT RMG (Realized margin)

            return codeReturn;
        }
        #endregion InsertRealizedMarginEvent
        #region InsertSettlementCurrencyEvent
        /// <summary>
        /// Le montant SCU suite à dénouement d'option est calculé directement avec le cours du sous-jacent
        /// Dans le cas d'un dénouement INTRADAY ce cours peut être absent. L'événement SCU et ses composants sera malgré tout
        /// créé avec un montant à zéro et sans Payer/Receiver
        /// A charge au traitement EOD final (via Calcul des Cash-Flows) de le compléter
        /// </summary>
        private Cst.ErrLevel InsertSettlementCurrencyEvent(IDbTransaction pDbTransaction, int pIdT, int pIdE, int pIdE_Event,
            bool pIsDealerBuyer, decimal pQty, decimal pRemainQty, int pIdPADET)
        {
            Cst.ErrLevel codeReturn = Cst.ErrLevel.UNDEFINED;

            IPosRequestDetOption _posRequestDetOption = posRequest.DetailBase as IPosRequestDetOption;
            #region EVENT SCU (Cash Settlement sur Exercice/Assignation)
            string eventCode = (pRemainQty == 0) ? EventCodeFunc.Termination : EventCodeFunc.Intermediary;
            string eventType = EventTypeFunc.SettlementCurrency;
            decimal strikePrice = _posRequestDetOption.StrikePrice;

            // EG 20140325 [19671][19766] 
            EFS_Asset efs_Asset = null;
            if (_posRequestDetOption.UnderlyerSpecified)
            {
                efs_Asset = _posRequestDetOption.Underlyer.GetCharacteristics(m_Cs, pDbTransaction);
                strikePrice = ExchangeTradedDerivativeTools.ConvertStrikeToUnderlyerBase(strikePrice, _posRequestDetOption.Underlyer.AssetCategory,
                    efs_Asset.instrumentNum, efs_Asset.instrumentDen);
            }

            #region Insertion EVENT / EVENTCLASS

            PayerReceiverAmountInfo _payrec = new PayerReceiverAmountInfo(m_PosKeepingData, EventTypeFunc.SettlementCurrency, pIsDealerBuyer);
            IPosKeepingQuote quote = Asset.GetSettlementPrice(MasterDtBusiness, ref codeReturn);

            Nullable<decimal> cashSettlementAmount = null;
            if (null != quote)
            {
                _payrec.SetSettlementInfo(m_PosKeepingData, quote);
                cashSettlementAmount = m_PosKeepingData.CashSettlement(strikePrice, quote.QuotePrice, pQty);
            }
            _payrec.SetPayerReceiver(cashSettlementAmount);

            codeReturn = EventQry.InsertEvent(pDbTransaction, pIdT, pIdE, pIdE_Event, null, 1, 1, _payrec.IdA_Payer, _payrec.IdB_Payer, _payrec.IdA_Receiver, _payrec.IdB_Receiver,
                eventCode, eventType, MasterDtBusiness, MasterDtBusiness, MasterDtBusiness, MasterDtBusiness, _payrec.Amount, Asset.currency,
                UnitTypeEnum.Currency.ToString(), _payrec.Amount.HasValue ? StatusCalculEnum.CALC : StatusCalculEnum.TOCALC, null);

            if (Cst.ErrLevel.SUCCESS == codeReturn)
                codeReturn = EventQry.InsertEventClass(pDbTransaction, pIdE, EventClassFunc.Recognition, MasterDtBusiness, false);

            if (Cst.ErrLevel.SUCCESS == codeReturn)
                codeReturn = EventQry.InsertEventClass(pDbTransaction, pIdE, EventClassFunc.ValueDate, MasterDtBusiness, false);

            if (Cst.ErrLevel.SUCCESS == codeReturn)
                codeReturn = EventQry.InsertEventClass(pDbTransaction, pIdE, EventClassFunc.Settlement, GetDtSettlement(), true);

            #endregion Insertion EVENT / EVENTCLASS
            #region Insertion EVENTASSET avec info du sous-jacent
            if ((Cst.ErrLevel.SUCCESS == codeReturn) && (_posRequestDetOption.UnderlyerSpecified))
            {
                if ((0 == _posRequestDetOption.Underlyer.IdAsset) && Asset.idAsset_UnderlyerSpecified)
                {
                    _posRequestDetOption.Underlyer.IdAsset = Asset.idAsset_Underlyer;
                    _posRequestDetOption.Underlyer.Identifier = Asset.identifier_Underlyer;
                    _posRequestDetOption.Underlyer.AssetCategory = Asset.assetCategory_Underlyer;
                }
                codeReturn = EventQuery.InsertEventAsset(pDbTransaction, pIdE, efs_Asset);
            }
            #endregion Insertion EVENTASSET avec info du sous-jacent

            if (Cst.ErrLevel.SUCCESS == codeReturn)
            {
                codeReturn = EventQry.InsertEventDet_Denouement(pDbTransaction, pIdE, pQty, Asset.contractMultiplier,
                    strikePrice, _payrec.QuoteSide, _payrec.QuoteTiming, _payrec.DtSettlementPrice, _payrec.SettlementPrice, _payrec.SettlementPrice100, null);
            }

            if ((Cst.ErrLevel.SUCCESS == codeReturn) && (0 < pIdPADET))
                codeReturn = EventQuery.InsertEventPosActionDet(pDbTransaction, pIdPADET, pIdE);

            #endregion EVENT SCU (Cash Settlement)

            return codeReturn;
        }
        #endregion InsertSettlementCurrencyEvent
        #region InsertVariationMarginEvent
        /// <summary>
        /// Calcul et insertion d'un VMG dans un traitement EOD
        /// ─────────────────────────────────────────────────────────────────────────────────────────
        /// ► Dénouement d'options
        ///   ● VMG = QTY DENOUEE * CONTRACTMULTIPLIER * COURS COMPENS[J]
        /// ► Correction de position / Transfert de position
        ///   ● VMG = QTY * CONTRACTMULTIPLIER * (COURS NEGO - COURS COMPENS[J-1])
        /// ► Décompensation
        ///   ● VMG = QTY * CONTRACTMULTIPLIER * (COURS OFFSETTING - COURS COMPENS[J-1])
        /// ─────────────────────────────────────────────────────────────────────────────────────────
        /// </summary>
        /// <param name="pDbTransaction">Transaction</param>
        /// <param name="pIdT">IdT</param>
        /// <param name="pIdE">IdE</param>
        /// <param name="pIdE_Event">IdE parent</param>
        /// <param name="pIsDealerBuyer">Le dealer est le buyer</param>
        /// <param name="pPrice">Prix de négo</param>
        /// <param name="pQty">Quantité</param>
        /// <param name="pDtControl">Date offsetting (cas unclearing) ou ClearingBusinessDate (cas mise à jour de cloture à l'échéance)</param>
        /// <returns></returns>
        // EG 20190114 Add detail to ProcessLog Refactoring
        // EG 20190716 [VCL : New FixedIncome] Upd GetQuote
        // EG 20190730 Add TypePrice parameter
        // EG 20210415 [25584][25702] Gestion de la validité des LPP/LPC VMG sur options marginées (suite à modification de Date de maturité)
        // EG 20220701 [26088] Appel à GetQuoteLock sur la méthode d'insertion VMG dans le dénouement d'option(multi-threading)
        private Cst.ErrLevel InsertVariationMarginEvent(IDbTransaction pDbTransaction, int pIdT, int pIdE, int pIdE_Event, bool pIsDealerBuyer,
            decimal pPrice, decimal pQty, DateTime pDtControl)
        {
            Quote_ETDAsset quote = null;
            SystemMSGInfo errReadOfficialClose = null;
            Quote_ETDAsset quoteVeil = null;

            bool isEOD = (m_Process.MasterPosRequest.RequestType == Cst.PosRequestTypeEnum.EndOfDay);

            // Détermination de la date de lecture du cours
            // Pour les DC dont l’échéance B.O. est décalée à JREAL_MATURITY = JREAL_MATURITY + N
            //  -	A partir de JREAL_MATURITY (inclus) jusqu’à JREAL_MATURITY (inclus) il n’y aura donc plus d’événement LPP\VMG de généré
            //  -	A JREAL_MATURITY il y aura un événement LPC\VMG généré, incluant dernier VMG et VMG correctif donc calculé entre ZERO et « cours veille », 
            //      avec cours veille = cours JREAL_MATURTY - 1
            // Pour les DC dont l’échéance B.O. n’est pas décalée à 
            //  -	A JREAL_MATURITY il y aura un événement LPC\VMG généré, incluant dernier VMG et VMG correctif donc calculé entre ZERO et « cours veille »
            // Cas particulier Négociation le jour de l'échéance (Cours négo se substitue à Cours veille)
            Nullable<decimal> quoteValue = null;
            decimal? variationMargin;
            if (((pDtControl.Date >= Asset.maturityDateSys)) && (Asset.maturityDateSys <= MasterDtBusiness))
            {
                // Cas particulier jour de négociation à l'échéance (Cours négo - 0)
                variationMargin = m_PosKeepingData.VariationMargin(pPrice, 0, pQty);
            }
            else
            {
                // EG 20140211 Pas de test sur trade jour
                DateTime dtQuote = Asset.GetOfficialCloseQuoteTime(MasterDtBusiness);
                // EG 20120605 [19807] New
                //if (m_ExchangeTradedDerivativeContainer.MaturityDateSys <= DtBusiness)
                if (Asset.maturityDateSys <= MasterDtBusiness)
                {
                    // Lecture en Date veille de dtQuote (MaturityDateSys)
                    dtQuote = Tools.ApplyOffset(m_Cs, m_Product, dtQuote, PeriodEnum.D, -1, DayTypeEnum.ExchangeBusiness, Asset.idBC, BusinessDayConventionEnum.PRECEDING, null);
                }
                // EG 20220630 [26088] Appel à la fonction qui lock le cache des cotations
                quote = m_Process.PKGenProcess.ProcessCacheContainer.GetQuoteLock(m_PosKeepingData.IdAsset, dtQuote, Asset.identifier,
                    QuotationSideEnum.OfficialClose, Cst.UnderlyingAsset.ExchangeTradedContract, new KeyQuoteAdditional(), ref errReadOfficialClose) as Quote_ETDAsset;
                if ((null == quote) || (false == quote.valueSpecified))
                {
                    if ((null != errReadOfficialClose) && isEOD)
                    {
                        // FI 20200623 [XXXXX] SetErrorWarning
                        m_Process.PKGenProcess.ProcessState.SetErrorWarning(ProcessStateTools.StatusWarningEnum);
                        
                        Logger.Log(new LoggerData(LogLevelEnum.Warning, errReadOfficialClose.SysMsgCode, 0, errReadOfficialClose.LogParamDatas));
                    }
                }
                else
                {
                    quoteValue = quote.value;
                }
                variationMargin = m_PosKeepingData.VariationMargin(quoteValue, 0, pQty);
            }

            PayerReceiverAmountInfo _payrec = new PayerReceiverAmountInfo(m_PosKeepingData, EventTypeFunc.VariationMargin, pIsDealerBuyer);
            _payrec.SetPayerReceiver(variationMargin);
            // RD 20150311 [20831] Remplacer le deuxième argument _payrec.IdA_Receiver par _payrec.IdB_Receiver
            Cst.ErrLevel codeReturn = EventQry.InsertEvent(pDbTransaction, pIdT, pIdE, pIdE_Event, null, 1, 1,
                _payrec.IdA_Payer, _payrec.IdB_Payer, _payrec.IdA_Receiver, _payrec.IdB_Receiver,
                EventCodeFunc.LinkedProductClosing, EventTypeFunc.VariationMargin, MasterDtBusiness, MasterDtBusiness, MasterDtBusiness, MasterDtBusiness,
                variationMargin, Asset.currency, UnitTypeEnum.Currency.ToString(),
                variationMargin.HasValue ? StatusCalculEnum.CALC : StatusCalculEnum.TOCALC, null);

            // EG 20210415 [25584][25702]Désactivation de l'éventuel événement LPP/VMG présent sous LPC/AMT à la date d'échéance
            // Cas rencontré possible après modification de la date d'échéance du contrat :
            // Avant (MATURITYDATE > DTBUSINESS EOD) Après (MATURITYDATE = DTBUSINESS EOD) 
            if (Cst.ErrLevel.SUCCESS == codeReturn)
                EventQry.DeactivEvents(m_Cs, pDbTransaction, pIdT,
                EventCodeFunc.LinkedProductPayment, EventTypeFunc.VariationMargin, EventClassFunc.ValueDate, MasterDtBusiness, m_Process.PKGenProcess.Session.IdA);

            if (Cst.ErrLevel.SUCCESS == codeReturn)
                codeReturn = EventQry.InsertEventClass(pDbTransaction, pIdE, EventClassFunc.Recognition, MasterDtBusiness, false);

            // EG 20150616 [21124]
            if (Cst.ErrLevel.SUCCESS == codeReturn)
                codeReturn = EventQry.InsertEventClass(pDbTransaction, pIdE, EventClassFunc.ValueDate, MasterDtBusiness, false);

            if (Cst.ErrLevel.SUCCESS == codeReturn)
                codeReturn = EventQry.InsertEventClass(pDbTransaction, pIdE, EventClassFunc.Settlement, GetDtSettlement(), true);
            // RD 20120821 [18087] Add QUOTEDELTA
            if (Cst.ErrLevel.SUCCESS == codeReturn)
            {
                Nullable<decimal> quoteDelta = null;
                if (null != quote)
                {
                    quoteValue = quote.value;
                    quoteDelta = quote.delta;
                }

                Nullable<decimal> quoteValueVeil = null;
                if (null != quoteVeil)
                    quoteValueVeil = quoteVeil.value;

                decimal price100 = m_PosKeepingData.ToBase100(pPrice);
                decimal? quoteValue100 = m_PosKeepingData.ToBase100(quoteValue);
                decimal? quoteValueVeil100 = m_PosKeepingData.ToBase100(quoteValueVeil);
                price100 = m_PosKeepingData.VariableContractValue(price100);
                quoteValue100 = m_PosKeepingData.VariableContractValue(quoteValue100);
                quoteValueVeil100 = m_PosKeepingData.VariableContractValue(quoteValueVeil100);

                codeReturn = EventQry.InsertEventDet_Closing(pDbTransaction, pIdE, pQty, Asset.contractMultiplier,
                    null, null, null, pPrice, price100, null, quoteValue, quoteValue100, quoteDelta, quoteValueVeil, quoteValueVeil100, null, null);
            }
            return codeReturn;
        }
        #endregion InsertVariationMarginEvent

        #region SetPaymentFeesForEvent
        /// <summary>
        /// Remplacement de la quantité d'origine du trade par la quantité dénouée ou corrigée (UTILISE pour la restitution de prime)
        /// </summary>
        /// <param name="pIdT">Id du trade clôturant</param>
        /// <param name="pQty">Quantité clôturée</param>
        /// <param name="pPaymentFees">Frais (pour restitution)</param>
        /// <param name="pDetail">PosRequestDetail</param>
        // EG 20140711 (New - Override)
        // EG 20150920 [21374] Int (int32) to Long (Int64) 
        // EG 20170127 Qty Long To Decimal
        // EG 20240115 [WI808] New : Harmonisation et réunification des méthodes
        public override void SetPaymentFeesForEvent(IDbTransaction pDbTransaction, int pIdT, decimal pQty)
        {

            if ((null != ExchangeTradedDerivativeContainer) && (null != ExchangeTradedDerivativeContainer.TradeCaptureReport))
            {
                base.SetPaymentFeesForEvent(pDbTransaction, pIdT, pQty);
            }
        }
        #endregion SetPaymentFeesForEvent
        #region SetPosKeepingData
        // EG 20180326 [23769] Use MapDataReaderRow
        protected override Cst.ErrLevel SetPosKeepingData(IDbTransaction pDbTransaction, MapDataReaderRow pMapDr, Cst.PosRequestAssetQuoteEnum pPosRequestAssetQuote, bool pIsQuoteCanBeReaded)
        {
            return base.SetPosKeepingTrade(pMapDr);
        }
        #endregion SetPosKeepingData


        #region TradeOptionGen
        public Cst.ErrLevel TradeOptionGen(IDbTransaction pDbTransaction)
        {
            DeserializeTrade(pDbTransaction);

            DataSet ds = GetPositionTradeOption(pDbTransaction, MasterDtBusiness);
            m_DtPosition = ds.Tables[0];
            m_DtPosActionDet = ds.Tables[1];
            m_DtPosition.PrimaryKey = new DataColumn[] { m_DtPosition.Columns["IDT"] };
            
            Cst.ErrLevel codeReturn = PosActionDetCalculation(pDbTransaction);

            if (Cst.ErrLevel.SUCCESS == codeReturn)
                codeReturn = TradeOptionUpdating(pDbTransaction);

            return codeReturn;
        }
        #endregion TradeOptionGen
        #region TradeOptionUpdating
        // PL 20180309 UP_GETID use Shared Sequence on POSACTION/POSACTIONDET
        // EG 20180906 PERF Add DTOUT (Alloc ETD only)
        // EG 20230929 [WI715][26497] Call PosKeepingTools.SetPosRequestUnderlyerDelivery instead of AddPosRequestUnderlyerDelivery
        // EG 20230929 [WI715][26497] Call PosKeepingTools.DeletePosRequestUnderlyerDelivery instead of DeletePosRequestUnderlyerDelivery
        // EG 20240115 [WI808] Upd to public
        public Cst.ErrLevel TradeOptionUpdating(IDbTransaction pDbTransaction)
        {
            Cst.ErrLevel ret = Cst.ErrLevel.SUCCESS;
            IDbTransaction dbTransaction = pDbTransaction;
            bool isDbTransactionSelfCommit = (null == pDbTransaction);
            bool isException = false;
            try
            {
                if (isDbTransactionSelfCommit)
                    dbTransaction = DataHelper.BeginTran(m_Cs);

                IPosRequestOption _posRequestOption = posRequest as IPosRequestOption;
                IPosRequestDetOption _posRequestOptionDetail = posRequest.DetailBase as IPosRequestDetOption;
                bool isUnderlyerDelivery = (_posRequestOptionDetail.UnderlyerSpecified && (false == IsRequestOptionAbandon) &&
                    ((_posRequestOptionDetail.Underlyer.AssetCategory == Cst.UnderlyingAsset.Future) ||
                    (_posRequestOptionDetail.Underlyer.AssetCategory == Cst.UnderlyingAsset.ExchangeTradedContract) ||
                    (_posRequestOptionDetail.Underlyer.AssetCategory == Cst.UnderlyingAsset.EquityAsset)));
                DataTable dtChanged = m_DtPosActionDet.GetChanges();
                if (null != dtChanged)
                {
                    m_DtPosition.PrimaryKey = new DataColumn[] { m_DtPosition.Columns["IDT"] };
                    DataRow rowTradeClosing = null;
                    #region GetId of POSACTION/POSACTIONDET/EVENT
                    int nbOfTokenIDPADET = dtChanged.Rows.Count;
                    // IDE = 9 EVTs potentiels
                    // . ABN-ASS-EXE/TOT-PAR
                    // . TER-INT/NOM
                    // . TER-INT/QTY
                    // . TER-INT/SCU
                    // . LPP/VMG
                    // . LPP/RMG
                    // . ABN/PAR si ASS-EXE/PAR et abandon de la quantité restante
                    // . TER/NOM si ASS-EXE/PAR et abandon de la quantité restante
                    // . TER/QTY si ASS-EXE/PAR et abandon de la quantité restante
                    int newIdPADET = 0;
                    int newIdE = 0;
                    ret = SQLUP.GetId(out int newIdPA, dbTransaction, SQLUP.IdGetId.POSACTION, SQLUP.PosRetGetId.First, 1);
                    if (Cst.ErrLevel.SUCCESS == ret)
                    {
                        newIdPADET = newIdPA;
                        ret = SQLUP.GetId(out newIdE, dbTransaction, SQLUP.IdGetId.EVENT, SQLUP.PosRetGetId.First, nbOfTokenIDPADET * posRequest.NbTokenIdE);
                    }
                    #endregion GetId of POSACTION/POSACTIONDET/EVENT

                    // EG 20151125 [20979] Refactoring
                    Nullable<int> idPR = GetIdPR();
                    if (idPR.HasValue)
                        ret = m_Process.DeleteAllPosActionAndLinkedEventByRequest(dbTransaction, idPR.Value);

                    #region Insertion dans POSACTION
                    if (Cst.ErrLevel.SUCCESS == ret)
                    {
                        //Asset.maturityDateSys
                        DateTime dtOut = Asset.maturityDateSys;
                        // FI 20190222 [24502] 63 jours à la place de 2 mois
                        // FI 20190320 [24594] add test != DateTime.MaxValue
                        // FI 20190510 [24594][24596] Correction d'une boulette (il y avait dtOut.AddMonths(63))
                        if (DtFunc.IsDateTimeFilled(dtOut) && dtOut != DateTime.MaxValue.Date)
                            dtOut = dtOut.AddDays(63);

                        m_Process.InsertPosAction(dbTransaction, newIdPA, MasterDtBusiness, posRequest.IdPR, dtOut);
                    }
                    #endregion Insertion dans POSACTION

                    if (Cst.ErrLevel.SUCCESS == ret)
                    {
                        #region Lecture des lignes de POSACTIONDET post traitement
                        foreach (DataRow row in dtChanged.Rows)
                        {
                            // On récupère le DataRow de la négociation clôturante
                            if (row.RowState == DataRowState.Added)
                            {
                                // Nouvelle clôture 
                                rowTradeClosing = m_DtPosition.Rows.Find(row["IDT_CLOSING", DataRowVersion.Current]);
                                ret = InsertPosActionDetAndOptionLinkedEvent(dbTransaction, newIdPA, newIdPADET, ref newIdE, row, rowTradeClosing);

                                // Insertion POSREQUEST pour INSERTION TRADE sur SOUS-JACENT
                                if (Cst.ErrLevel.SUCCESS == ret)
                                {
                                    // EG 20130513 Plus d'insertion si le sous-jacent n'est pas un future
                                    // PM 20150225 [POC] ou un EquityAsset
                                    if (isUnderlyerDelivery)
                                    {
                                        string eventClass = rowTradeClosing["EVENTCLASS"].ToString();
                                        if (EventClassFunc.IsPhysicalSettlement(eventClass))
                                        {
                                            if (_posRequestOptionDetail.UnderlyerSpecified)
                                            {
                                                SQL_Instrument sqlInstrument = new SQL_Instrument(m_Cs, _posRequestOptionDetail.Underlyer.IdI)
                                                {
                                                    DbTransaction = dbTransaction
                                                };
                                                if (sqlInstrument.IsLoaded && sqlInstrument.IsEnabled)
                                                {
                                                    // EG 20230929 [WI715][26497]
                                                    //ret = AddPosRequestUnderlyerDelivery(dbTransaction);
                                                    ret = PosKeepingTools.SetPosRequestUnderlyerDelivery(m_Cs, dbTransaction, m_Product, posRequest, m_Process.PKGenProcess.Session);
                                                }
                                            }
                                        }
                                    }
                                }

                            }
                            //newIdPADET++;
                            //newIdE++;
                            if (Cst.ErrLevel.SUCCESS == ret)
                            {
                                newIdE++;
                                ret = SQLUP.GetId(out newIdPADET, dbTransaction, SQLUP.IdGetId.POSACTION, SQLUP.PosRetGetId.First, 1);
                            }
                            if (Cst.ErrLevel.SUCCESS != ret)
                            {
                                isException = true;
                                break;
                            }
                        }
                        #endregion Lecture des lignes de POSACTIONDET post traitement
                    }
                }
                else
                {
                    // EG 20151125 [20979] Refactoring
                    Nullable<int> idPR = GetIdPR();
                    if (idPR.HasValue)
                        ret = m_Process.DeleteAllPosActionAndLinkedEventByRequest(dbTransaction, idPR.Value);

                    /// EG 20130613 [18751] Purge de l'éventuelle demande de LIVRAISON dans le cas où
                    // nous sommes en présence d'une annulation de dénouement manuel (Qty = 0)
                    if (isUnderlyerDelivery && (0 == _posRequestOption.Qty))
                    {
                        m_DtPosition.PrimaryKey = new DataColumn[] { m_DtPosition.Columns["IDT"] };
                        DataRow rowTradeClosing2 = m_DtPosition.Rows.Find(_posRequestOption.IdT);
                        if (null != rowTradeClosing2)
                        {
                            string eventClass = rowTradeClosing2["EVENTCLASS"].ToString();
                            if (EventClassFunc.IsPhysicalSettlement(eventClass))
                                // EG 20230929 [WI715][26497]
                                //ret = DeletePosRequestUnderlyerDelivery(dbTransaction);
                                ret = PosKeepingTools.DeletePosRequestUnderlyerDelivery(m_Cs, dbTransaction, m_Product, posRequest, m_Process.PKGenProcess.Session.IdA);
                        }
                    }
                }

                if (isDbTransactionSelfCommit & (false == isException))
                    DataHelper.CommitTran(dbTransaction);
            }
            catch (Exception)
            {
                isException = true;
                ret = Cst.ErrLevel.FAILURE;
                throw;
            }
            finally
            {
                if (isDbTransactionSelfCommit && (null != dbTransaction))
                {
                    if (isException)
                        DataHelper.RollbackTran(dbTransaction);
                    dbTransaction.Dispose();
                }

            }
            return ret;

        }
        #endregion TradeOptionUpdating
        #endregion Methods
    }
    #endregion TradeOptionData
    #region TradeUnderlyerDeliveryData
    public class TradeUnderlyerDeliveryData : TradeData
    {
        #region Members
        #endregion Members
        #region Accessors
        #region Asset
        protected PosKeepingAsset_ETD Asset
        {
            get { return (PosKeepingAsset_ETD)m_PosKeepingData.Asset; }
        }
        #endregion Asset
        #region Process
        public PosKeepingGen_ETD Process
        {
            get { return (PosKeepingGen_ETD)m_Process; }
        }
        #endregion Process
        #endregion Accessors
        #region Constructors
        public TradeUnderlyerDeliveryData(PosKeepingGen_ETD pProcess, IPosRequest pPosRequest, int pIdPRParent)
            : base(pProcess, pPosRequest)
        {
            Process.InitStatusPosRequest(pPosRequest, pPosRequest.IdPR, pIdPRParent, ProcessStateTools.StatusProgressEnum);
            // FI 20181219 [24399] AddSubPosRequest Method Static
            PosKeepingGenProcessBase.AddSubPosRequest(Process.LstSubPosRequest, posRequest);
        }
        #endregion Constructors
        #region Methods
        #region CreateUnderlyingTrade
        /// <summary>
        /// Création du trade matérialisant la livraison du sous-jacent suite à dénouement
        /// </summary>
        /// <param name="pRequestOption"></param>
        /// <returns></returns>
        public Cst.ErrLevel CreateUnderlyingTrade(IDbTransaction pDbTransaction)
        {
            // Creation et Alimentation du dataDocument du trade sous-jacent sur la base du template et de l'option
            Cst.ErrLevel codeReturn = SetDataDocument(pDbTransaction);
            // Enregistrement du trade
            if (Cst.ErrLevel.SUCCESS == codeReturn)
            {
                codeReturn = RecordUnderlyingTrade(pDbTransaction, out int idT);
                // Remplacement de l'IdT d'origine (Option) par celui du sous-jacent créé 
                if (Cst.ErrLevel.SUCCESS == codeReturn)
                    posRequest.IdT = idT;
            }
            return codeReturn;
        }
        #endregion CreateUnderlyingTrade
        #region DeserializeTrade
        public override void DeserializeTrade(IDbTransaction pDbTransaction)
        {
            base.DeserializeTrade(pDbTransaction);
            IExchangeTradedDerivative etd = (IExchangeTradedDerivative)m_TradeLibrary.Product;
            m_RptSideProductContainer = new ExchangeTradedDerivativeContainer(Process.ProcessBase.Cs, pDbTransaction, etd);
        }
        #endregion DeserializeTrade

        #region GetDeliveryDate
        private DateTime GetDeliveryDate(DateTime pDtBusiness)
        {
            DateTime deliveryDate = pDtBusiness;
            if (Asset.settlMethod == SettlMethodEnum.PhysicalSettlement)
            {
                if ((MasterDtBusiness == Asset.maturityDate) && Asset.deliveryDateSpecified)
                {
                    deliveryDate = Asset.deliveryDate;
                }
                else if (Asset.deliveryDelayOffsetSpecified)
                {
                    IBusinessDayAdjustments bda = m_Product.CreateBusinessDayAdjustments(BusinessDayConventionEnum.FOLLOWING, Asset.idBC);
                    deliveryDate = Tools.ApplyOffset(m_Cs, deliveryDate, Asset.deliveryDelayOffset, bda, null);
                }
            }
            return deliveryDate;
        }
        #endregion GetDeliveryDate

        #region RecordUnderlyingTrade
        /// <summary>
        ///  Création du trade matérialisant la livraison du sous-jacent
        /// </summary>
        /// <param name="pRequestOption"></param>
        /// <param name="pIdT">Retourne l'IdT du trade généré</param>
        /// <returns></returns>
        /// FI 20161206 [22092] Modify
        // EG 20190613 [24683] Add identifierDelivery
        public Cst.ErrLevel RecordUnderlyingTrade(IDbTransaction pDbTransaction, out int pIdTDelivery)
        {
            IPosRequestOption posRequestOption = posRequest as IPosRequestOption;
            IPosRequestDetOption detail = posRequestOption.Detail as IPosRequestDetOption;
            IPosRequestDetUnderlyer underlyer = detail.Underlyer;

            IPosRequestTradeAdditionalInfo additionalInfo = (IPosRequestTradeAdditionalInfo)m_Process.TemplateDataDocumentTrade[underlyer.IdI];

            // FI 20161206 [22092] Modify calcul des frais => Mise à jour de underlyer.dataDocument 
            TradeInput tradeInput = new TradeInput
            {
                SQLProduct = additionalInfo.SqlProduct,
                SQLTrade = additionalInfo.SqlTemplateTrade,
                DataDocument = underlyer.DataDocument
            };
            tradeInput.SQLProduct.DbTransaction = pDbTransaction;
            tradeInput.SQLTrade.DbTransaction = pDbTransaction;
            tradeInput.TradeStatus.Initialize(m_Cs, pDbTransaction, additionalInfo.SqlTemplateTrade.IdT);
            //PL 20200217 [25207] 
            //tradeInput.RecalculFeeAndTax(m_Cs, pDbTransaction, IdMenu.GetIdMenu(IdMenu.Menu.InputTrade_DLV));
            string action = "OTC_INP_TRD_OPTEXE_OPTASS";
            if (tradeInput.Product.GetTrdSubType() == TrdSubTypeEnum.TransactionFromExercise)
                action = "OTC_INP_TRD_OPTEXE";
            else if (tradeInput.Product.GetTrdSubType() == TrdSubTypeEnum.TransactionFromAssignment)
                action = "OTC_INP_TRD_OPTASS";
            tradeInput.RecalculFeeAndTax(m_Cs, pDbTransaction, action);

            List<Pair<int, string>> tradeLinkInfo = new List<Pair<int, string>>
            {
                new Pair<int, string>(this.idT, this.identifier)
            };

            Cst.ErrLevel codeReturn = m_Process.RecordTrade(pDbTransaction, underlyer.DataDocument, additionalInfo, underlyer.GetIdPRSource(),
            underlyer.RequestTypeSource, tradeLinkInfo, out int idTDelivery, out string _);

            pIdTDelivery = idTDelivery;
            return codeReturn;
        }
        #endregion RecordUnderlyingTrade

        #region SetDataDocument
        /// <summary>
        ///  Alimente pRequestOption.detail.underlyer.dataDocument (trade ss-jacent) 
        /// </summary>
        /// EG 20180114 New
        private Cst.ErrLevel SetDataDocument(IDbTransaction pDbTransaction)
        {
            IPosRequestOption posRequestOption = posRequest as IPosRequestOption;
            IPosRequestDetUnderlyer underlyer = posRequestOption.Detail.Underlyer;

            // Recherche et Désérialisation du template associé à l'instrument rattaché au sous-jacent.
            Cst.ErrLevel codeReturn = SetTemplateDataDocumentLock(pDbTransaction, underlyer);
            if (Cst.ErrLevel.SUCCESS == codeReturn)
            {
                IPosRequestTradeAdditionalInfo additionalInfo = (IPosRequestTradeAdditionalInfo)Process.TemplateDataDocumentTrade[underlyer.IdI];

                // Désérialisation du trade option à l'origine du trade sous-jacent à créer
                //DeserializeTrade(pDbTransaction, underlyer.idTSource);
                DeserializeTrade(pDbTransaction);

                // Recopie du dataDocument template du sous-jacent
                underlyer.DataDocument = (DataDocumentContainer)additionalInfo.TemplateDataDocument.Clone();

                DataDocumentContainer dataDocUnl = underlyer.DataDocument;
                decimal strikePrice_UnderlyerBase = ExchangeTradedDerivativeTools.ConvertStrikeToUnderlyerBase
                    (
                        Asset.strikePrice, Cst.UnderlyingAsset.Future, Asset.instrumentNum_Underlyer, Asset.instrumentDen_Underlyer
                    );

                if (dataDocUnl.CurrentProduct.IsExchangeTradedDerivative)
                    codeReturn = SetDataDocumentEtd(pDbTransaction, strikePrice_UnderlyerBase, dataDocUnl);
                else if (dataDocUnl.CurrentProduct.IsEquitySecurityTransaction)
                    codeReturn = SetDataDocumentEquityTransaction(pDbTransaction, strikePrice_UnderlyerBase, dataDocUnl);
                else
                    codeReturn = Cst.ErrLevel.DATAREJECTED;
            }
            return codeReturn;
        }
        #endregion SetDataDocument
        #region SetDataDocumentEtd
        /// <summary>
        ///  Complete le DataDocument dans le cas d'un ExchangeTradedDerivative
        /// </summary>
        // EG 20180114 New 
        private Cst.ErrLevel SetDataDocumentEtd(IDbTransaction pDbTransaction, decimal pPrice, DataDocumentContainer pDataDocument)
        {
            Cst.ErrLevel codeReturn = Cst.ErrLevel.SUCCESS;
            IPosRequestOption posRequestOption = posRequest as IPosRequestOption;
            // Désérialisation du trade option à l'origine du trade sous-jacent à créer
            DataDocumentContainer dataDocOption = m_TradeLibrary.DataDocument;
            IExchangeTradedDerivative etdOption = (IExchangeTradedDerivative)dataDocOption.CurrentProduct.Product;
            IFixTrdCapRptSideGrp rptSideOption = (IFixTrdCapRptSideGrp)etdOption.TradeCaptureReport.TrdCapRptSideGrp[0];

            IExchangeTradedDerivative etdFuture = (IExchangeTradedDerivative)pDataDocument.CurrentProduct.Product;
            IFixTradeCaptureReport trdCapRptUnl = (IFixTradeCaptureReport)etdFuture.TradeCaptureReport;
            IFixTrdCapRptSideGrp rptSideUnl = (IFixTrdCapRptSideGrp)etdFuture.TradeCaptureReport.TrdCapRptSideGrp[0];

            pDataDocument.PartyTradeIdentifier = dataDocOption.PartyTradeIdentifier;
            pDataDocument.PartyTradeInformation = dataDocOption.PartyTradeInformation;
            pDataDocument.PartyTradeInformationSpecified = dataDocOption.PartyTradeInformationSpecified;
            pDataDocument.Party = dataDocOption.Party;
            pDataDocument.BrokerPartyReferenceSpecified = dataDocOption.BrokerPartyReferenceSpecified;
            pDataDocument.BrokerPartyReference = dataDocOption.BrokerPartyReference;
            pDataDocument.GoverningLaw = dataDocOption.GoverningLaw;
            pDataDocument.GoverningLawSpecified = dataDocOption.GoverningLawSpecified;
            pDataDocument.CalculationAgent = dataDocOption.CalculationAgent;
            pDataDocument.CalculationAgentSpecified = dataDocOption.CalculationAgentSpecified;
            pDataDocument.GoverningLawSpecified = dataDocOption.GoverningLawSpecified;
            //
            pDataDocument.OtherPartyPaymentSpecified = false;

            // Mise à jour de l'asset sous jacent
            SQL_AssetETD assetETD = new SQL_AssetETD(m_Cs, posRequestOption.Detail.Underlyer.IdAsset)
            {
                DbTransaction = pDbTransaction
            };
            if (assetETD.IsLoaded)
            {
                ExchangeTradedDerivativeTools.SetFixInstrumentFromETDAsset(m_Cs, pDbTransaction, assetETD, CfiCodeCategoryEnum.Future,
                    trdCapRptUnl.Instrument, null);
            }
            // Mise à jour des données (Prix du sous-jacent, quantié, ...)
            trdCapRptUnl.LastPx = new EFS_Decimal(pPrice);
            trdCapRptUnl.LastPxSpecified = true;
            trdCapRptUnl.LastQty = new EFS_Decimal(posRequestOption.Qty);
            trdCapRptUnl.LastQtySpecified = true;

            trdCapRptUnl.TrdType = TrdTypeEnum.OptionExercise;
            trdCapRptUnl.TrdTypeSpecified = true;
            if ((posRequestOption.Detail.Underlyer.RequestTypeSource == Cst.PosRequestTypeEnum.OptionExercise) ||
                (posRequestOption.Detail.Underlyer.RequestTypeSource == Cst.PosRequestTypeEnum.AutomaticOptionExercise))
            {
                etdFuture.TradeCaptureReport.TrdSubType = TrdSubTypeEnum.TransactionFromExercise;
            }
            else
            {
                etdFuture.TradeCaptureReport.TrdSubType = TrdSubTypeEnum.TransactionFromAssignment;
            }
            trdCapRptUnl.TrdSubTypeSpecified = true;
            trdCapRptUnl.ExecType = ExecTypeEnum.TriggeredOrActivatedBySystem;

            rptSideUnl.Account = rptSideOption.Account;
            rptSideUnl.AccountSpecified = rptSideOption.AccountSpecified;
            rptSideUnl.AccountType = rptSideOption.AccountType;
            rptSideUnl.AccountTypeSpecified = rptSideOption.AccountTypeSpecified;
            rptSideUnl.AcctIdSource = AcctIDSourceEnum.Other;
            rptSideUnl.AcctIdSourceSpecified = rptSideOption.AcctIdSourceSpecified;
            rptSideUnl.ClrInstGrp = null;
            rptSideUnl.ExecRefIdSpecified = false;
            rptSideUnl.OrderIdSpecified = false;
            rptSideUnl.OrderInputDeviceSpecified = false;
            rptSideUnl.OrdTypeSpecified = rptSideOption.OrdTypeSpecified;
            rptSideUnl.Parties = rptSideOption.Parties;
            rptSideUnl.PositionEffect = FixML.v50SP1.Enum.PositionEffectEnum.Open;
            rptSideUnl.PositionEffectSpecified = true;
            // le dealer est Acheteur de CALL (exercice)  alors il est Acheteur du Future
            // le dealer est Vendeur de PUT (assignation) alors il est Acheteur du Future
            if (((etdOption.TradeCaptureReport.Instrument.PutOrCall == PutOrCallEnum.Call) &&
                (trdCapRptUnl.TrdSubType == TrdSubTypeEnum.TransactionFromExercise)) ||
                ((etdOption.TradeCaptureReport.Instrument.PutOrCall == PutOrCallEnum.Put) &&
                (trdCapRptUnl.TrdSubType == TrdSubTypeEnum.TransactionFromAssignment)))
            {
                rptSideUnl.Side = SideEnum.Buy;
            }
            else
            {
                rptSideUnl.Side = SideEnum.Sell;
            }
            rptSideUnl.SideSpecified = true;
            rptSideUnl.Text = "Delivery - Future";
            rptSideUnl.TextSpecified = true;

            DateTime tradeDate = posRequestOption.DtBusiness;
            // RD 20210106 [25627] Add test on LastTradingDay
            if ((DtFunc.IsDateTimeFilled(assetETD.Maturity_LastTradingDay)) && (0 < DateTime.Compare(tradeDate, assetETD.Maturity_LastTradingDay)))
                tradeDate = assetETD.Maturity_LastTradingDay;

            Process.SetMarketFacility(pDbTransaction, pDataDocument);

            // FI 20181023 [24259] Business date is pRequestOption.dtBusiness  
            // Il s'agit de reporter la correction [23671] en mode Async(multi-threading) 
            //Process.SetTradeDates(pDbTransaction, dataDocOption, pDataDocument, trdCapRptUnl, tradeDate, tradeDate);
            Process.SetTradeDates(pDbTransaction, pDataDocument, trdCapRptUnl, tradeDate, posRequestOption.DtBusiness);

            return codeReturn;
        }
        #endregion SetDataDocumentEtd
        #region SetDataDocumentEquityTransaction
        /// <summary>
        /// Complete le DataDocument dans le cas d'un EquitySecurityTransaction
        /// </summary>
        // EG 20180114 New
        // EG 20190114 Add detail to ProcessLog Refactoring
        private Cst.ErrLevel SetDataDocumentEquityTransaction(IDbTransaction pDbTransaction, decimal pPrice, DataDocumentContainer pDataDocument)
        {
            Cst.ErrLevel codeReturn = Cst.ErrLevel.SUCCESS;
            IPosRequestOption posRequestOption = posRequest as IPosRequestOption;
            if (pDataDocument.CurrentProduct.IsEquitySecurityTransaction)
            {
                DateTime dtBusiness = posRequestOption.DtBusiness;
                string CSCacheOn = CSTools.SetCacheOn(m_Cs);
                // Désérialisation du trade option à l'origine du trade sous-jacent à créer
                DataDocumentContainer dataDocOption = m_TradeLibrary.DataDocument;
                IExchangeTradedDerivative etdOption = (IExchangeTradedDerivative)dataDocOption.CurrentProduct.Product;
                IFixTradeCaptureReport trdCapRptOption = (IFixTradeCaptureReport)etdOption.TradeCaptureReport;
                IFixTrdCapRptSideGrp rptSideOption = (IFixTrdCapRptSideGrp)etdOption.TradeCaptureReport.TrdCapRptSideGrp[0];
                IFixParty fixPartyDealer = RptSideTools.GetParty(rptSideOption, PartyRoleEnum.BuyerSellerReceiverDeliverer);

                IEquitySecurityTransaction equityTransaction = (IEquitySecurityTransaction)pDataDocument.CurrentProduct.Product;
                IFixTradeCaptureReport trdCapRptUnl = (IFixTradeCaptureReport)equityTransaction.TradeCaptureReport;
                IFixTrdCapRptSideGrp rptSideUnl = (IFixTrdCapRptSideGrp)trdCapRptUnl.TrdCapRptSideGrp[0];

                IParty partyDealer = dataDocOption.Party.FirstOrDefault(p => p.Id == fixPartyDealer.PartyId.href);
                if (partyDealer != default(IParty))
                {
                    pDataDocument.RemoveParty();
                    SQL_Actor sqlActor = new SQL_Actor(CSCacheOn, partyDealer.PartyId)
                    {
                        DbTransaction = pDbTransaction
                    };
                    if (null != sqlActor)
                        pDataDocument.AddParty(sqlActor);
                    else
                        pDataDocument.AddParty(partyDealer);
                }

                IPartyTradeIdentifier partyIdentifierDealer = dataDocOption.PartyTradeIdentifier.FirstOrDefault(p => p.PartyReference.HRef == fixPartyDealer.PartyId.href);
                if (partyIdentifierDealer != default(IPartyTradeIdentifier))
                {
                    IPartyTradeIdentifier newPartyIdentifier = pDataDocument.AddPartyTradeIndentifier(fixPartyDealer.PartyId.href);
                    int posNewPartyIdentifier = Array.IndexOf(pDataDocument.PartyTradeIdentifier, newPartyIdentifier);
                    pDataDocument.PartyTradeIdentifier[posNewPartyIdentifier] = partyIdentifierDealer;
                }
                IPartyTradeInformation partyInformationDealer = dataDocOption.PartyTradeInformation.FirstOrDefault(p => p.PartyReference == fixPartyDealer.PartyId.href);
                if (partyInformationDealer != default(IPartyTradeInformation))
                {
                    IPartyTradeInformation newPartyInformation = pDataDocument.AddPartyTradeInformation(fixPartyDealer.PartyId.href);
                    int posNewPartyInformation = Array.IndexOf(pDataDocument.PartyTradeInformation, newPartyInformation);
                    pDataDocument.PartyTradeInformation[posNewPartyInformation] = partyInformationDealer;
                    pDataDocument.PartyTradeInformationSpecified = (pDataDocument.PartyTradeInformation.Count() != 0);
                }

                pDataDocument.BrokerPartyReferenceSpecified = dataDocOption.BrokerPartyReferenceSpecified;
                pDataDocument.BrokerPartyReference = dataDocOption.BrokerPartyReference;
                pDataDocument.GoverningLaw = dataDocOption.GoverningLaw;
                pDataDocument.GoverningLawSpecified = dataDocOption.GoverningLawSpecified;
                pDataDocument.CalculationAgent = dataDocOption.CalculationAgent;
                pDataDocument.CalculationAgentSpecified = dataDocOption.CalculationAgentSpecified;
                pDataDocument.GoverningLawSpecified = dataDocOption.GoverningLawSpecified;

                pDataDocument.OtherPartyPaymentSpecified = false;

                // Mise à jour de l'asset sous jacent
                SQL_AssetEquity assetUnl = new SQL_AssetEquity(CSCacheOn, posRequestOption.Detail.Underlyer.IdAsset)
                {
                    DbTransaction = pDbTransaction
                };
                if (assetUnl.IsLoaded)
                {
                    EquitySecurityTransactionTools.SetFixInstrumentFromEquityAsset(m_Cs, assetUnl, trdCapRptUnl.Instrument, trdCapRptUnl, pDataDocument);
                }
                // Mise à jour des données (prix, quantité, ...)
                trdCapRptUnl.LastPx = new EFS_Decimal(pPrice);
                trdCapRptUnl.LastPxSpecified = true;
                decimal quantity = posRequestOption.Qty;
                if ((Asset != null) && (Asset.factor != 0))
                {
                    quantity *= Asset.factor;
                }
                trdCapRptUnl.LastQty = new EFS_Decimal(quantity);
                trdCapRptUnl.LastQtySpecified = true;

                trdCapRptUnl.TrdType = TrdTypeEnum.OptionExercise;
                trdCapRptUnl.TrdTypeSpecified = true;
                if ((posRequestOption.Detail.Underlyer.RequestTypeSource == Cst.PosRequestTypeEnum.OptionExercise) ||
                    (posRequestOption.Detail.Underlyer.RequestTypeSource == Cst.PosRequestTypeEnum.AutomaticOptionExercise))
                {
                    trdCapRptUnl.TrdSubType = TrdSubTypeEnum.TransactionFromExercise;
                }
                else
                {
                    trdCapRptUnl.TrdSubType = TrdSubTypeEnum.TransactionFromAssignment;
                }
                trdCapRptUnl.TrdSubTypeSpecified = true;
                trdCapRptUnl.ExecType = ExecTypeEnum.TriggeredOrActivatedBySystem;

                rptSideUnl.Account = rptSideOption.Account;
                rptSideUnl.AccountSpecified = rptSideOption.AccountSpecified;
                rptSideUnl.AccountType = rptSideOption.AccountType;
                rptSideUnl.AccountTypeSpecified = rptSideOption.AccountTypeSpecified;
                rptSideUnl.AcctIdSource = AcctIDSourceEnum.Other;
                rptSideUnl.AcctIdSourceSpecified = rptSideOption.AcctIdSourceSpecified;
                rptSideUnl.ClrInstGrp = null;
                rptSideUnl.ExecRefIdSpecified = false;
                rptSideUnl.OrderIdSpecified = false;
                rptSideUnl.OrderInputDeviceSpecified = false;
                rptSideUnl.OrdTypeSpecified = rptSideOption.OrdTypeSpecified;
                // rptSideUnl.Parties
                if (rptSideUnl.Parties.Count() == 0)
                {
                    RptSideTools.AddParty(rptSideUnl);
                }
                rptSideUnl.Parties[0] = fixPartyDealer;
                IFixParty fixPartyCustodian = RptSideTools.AddParty(rptSideUnl);
                fixPartyCustodian.PartyRole = PartyRoleEnum.Custodian;
                fixPartyCustodian.PartyId.href = null;
                // rptSideUnl.PositionEffect
                SQL_Instrument sqlInstrument = new SQL_Instrument(CSCacheOn, posRequestOption.Detail.Underlyer.IdI)
                {
                    DbTransaction = pDbTransaction
                };
                if (sqlInstrument.FungibilityMode == FungibilityModeEnum.CLOSE)
                {
                    rptSideUnl.PositionEffect = FixML.v50SP1.Enum.PositionEffectEnum.Close;
                }
                else
                {
                    rptSideUnl.PositionEffect = FixML.v50SP1.Enum.PositionEffectEnum.Open;
                }
                rptSideUnl.PositionEffectSpecified = true;
                // le dealer est Acheteur de CALL (exercice)  alors il est Acheteur du sous-jacent
                // le dealer est Vendeur de PUT (assignation) alors il est Acheteur du sous-jacent
                if (((trdCapRptOption.Instrument.PutOrCall == PutOrCallEnum.Call) &&
                    (trdCapRptUnl.TrdSubType == TrdSubTypeEnum.TransactionFromExercise)) ||
                    ((trdCapRptOption.Instrument.PutOrCall == PutOrCallEnum.Put) &&
                    (trdCapRptUnl.TrdSubType == TrdSubTypeEnum.TransactionFromAssignment)))
                {
                    rptSideUnl.Side = SideEnum.Buy;
                }
                else
                {
                    rptSideUnl.Side = SideEnum.Sell;
                }
                rptSideUnl.SideSpecified = true;
                rptSideUnl.Text = "Delivery - EquitySecurityTransaction";
                rptSideUnl.TextSpecified = true;
                // Recherche du Custodian
                EquitySecurityTransactionContainer equityTransactionContainer = new EquitySecurityTransactionContainer(equityTransaction, pDataDocument);
                ClearingTemplates clearingTemplates = new ClearingTemplates();
                clearingTemplates.Load(CSCacheOn, pDataDocument, equityTransactionContainer, SQL_Table.ScanDataDtEnabledEnum.Yes);
                if (ArrFunc.IsFilled(clearingTemplates.clearingTemplate))
                {
                    ClearingTemplate clearingTemplateFind = clearingTemplates.clearingTemplate[0];
                    if (clearingTemplateFind.idAClearerSpecified)
                    {
                        string bookClearerIdentifier = clearingTemplateFind.bookClearerIdentifier;
                        SQL_Actor sqlActor = new SQL_Actor(CSCacheOn, clearingTemplateFind.idAClearer)
                        {
                            DbTransaction = pDbTransaction
                        };
                        string custodianXmlId = sqlActor.XmlId;
                        pDataDocument.AddParty(sqlActor);
                        IPartyTradeIdentifier partyIdentifierCustodian = pDataDocument.AddPartyTradeIndentifier(custodianXmlId);
                        if (StrFunc.IsFilled(bookClearerIdentifier))
                        {
                            partyIdentifierCustodian.BookId.Value = bookClearerIdentifier;
                            partyIdentifierCustodian.BookIdSpecified = true;
                            if (clearingTemplateFind.bookClearerDisplayNameSpecified)
                            {
                                partyIdentifierCustodian.BookId.BookName = clearingTemplateFind.bookClearerDisplayName;
                            }
                            if (clearingTemplateFind.idBClearerSpecified)
                            {
                                partyIdentifierCustodian.BookId.OTCmlId = clearingTemplateFind.idBClearer;
                            }
                        }
                        RptSideTools.SetParty(fixPartyCustodian, custodianXmlId, PartyRoleEnum.Custodian);

                        // EG 20150622 On prend la date Business courante de la table ENTITYMARKET
                        DateTime dtSysBusiness = Tools.GetDateBusiness(m_Cs, pDataDocument);
                        if (1 == dtSysBusiness.CompareTo(posRequestOption.DtBusiness))
                            dtBusiness = dtSysBusiness;
                    }
                }

                // EG 20171106 [23509]
                Process.SetMarketFacility(pDbTransaction, pDataDocument);
                Process.SetTradeDates(pDbTransaction, pDataDocument, trdCapRptUnl, posRequestOption.DtBusiness, dtBusiness);

                if (ArrFunc.IsEmpty(clearingTemplates.clearingTemplate) || (false == clearingTemplates.clearingTemplate[0].idAClearerSpecified))
                {
                    codeReturn = Cst.ErrLevel.FAILURE;
                    // FI 20200623 [XXXXX] SetErrorWarning
                    m_Process.PKGenProcess.ProcessState.SetErrorWarning(ProcessStateTools.StatusErrorEnum);

                    
                    Logger.Log(new LoggerData(LogLevelEnum.Error, new SysMsgCode(SysCodeEnum.SYS, 5165), 0,
                        new LogParam(LogTools.IdentifierAndId(assetUnl.Identifier, assetUnl.IdAsset)),
                        new LogParam(LogTools.IdentifierAndId(sqlInstrument.Identifier, sqlInstrument.IdI)),
                        new LogParam(LogTools.IdentifierAndId(partyDealer.PartyName, partyDealer.OTCmlId))));
                }

                if (Cst.ErrLevel.SUCCESS == codeReturn)
                {
                    #region GrossAmount
                    IPayment grossAmount = pDataDocument.CurrentProduct.ProductBase.CreatePayment();
                    //Payer\Receiver
                    if (rptSideUnl.Side == SideEnum.Buy)
                    {
                        grossAmount.PayerPartyReference.HRef = fixPartyDealer.PartyId.href;
                        grossAmount.ReceiverPartyReference.HRef = fixPartyCustodian.PartyId.href;
                    }
                    else
                    {
                        grossAmount.PayerPartyReference.HRef = fixPartyCustodian.PartyId.href;
                        grossAmount.ReceiverPartyReference.HRef = fixPartyDealer.PartyId.href;
                    }
                    //paymentAmount
                    Pair<Nullable<decimal>, string> priceGrossAmount = Tools.ConvertToQuotedCurrency(CSCacheOn, pDbTransaction, new Pair<Nullable<decimal>, string>(pPrice, assetUnl.IdC));

                    IMoney grossCalcAmount = pDataDocument.CurrentProduct.ProductBase.CreateMoney(quantity * priceGrossAmount.First.Value, priceGrossAmount.Second);
                    grossAmount.PaymentAmount.Amount.DecValue = grossCalcAmount.Amount.DecValue;
                    grossAmount.PaymentAmount.Currency = grossCalcAmount.Currency;
                    //Payement en date de livraison
                    // EG 20150622 Replace pRequestOption.dtBusiness by trdCapRptUnl.ClearingBusinessDate.DateValue
                    grossAmount.PaymentDate.UnadjustedDate.DateValue = GetDeliveryDate(trdCapRptUnl.ClearingBusinessDate.DateValue);
                    grossAmount.PaymentDate.DateAdjustments.BusinessDayConvention = BusinessDayConventionEnum.FOLLOWING;
                    grossAmount.PaymentDateSpecified = true;
                    equityTransaction.GrossAmount = grossAmount;
                    #endregion GrossAmount
                }
            }
            return codeReturn;
        }
        #endregion SetDataDocumentEquityTransaction
        #region SetPosKeepingData
        // EG 20180326 [23769] Use MapDataReaderRow
        protected override Cst.ErrLevel SetPosKeepingData(IDbTransaction pDbTransaction, MapDataReaderRow pMapDr, Cst.PosRequestAssetQuoteEnum pPosRequestAssetQuote, bool pIsQuoteCanBeReaded)
        {
            int idI = Convert.ToInt32(pMapDr["IDI"].Value);
            Nullable<Cst.UnderlyingAsset> _underlyingAsset = PosKeepingTools.GetUnderLyingAssetRelativeToInstrument(pMapDr);
            int idAsset = Convert.ToInt32(pMapDr["IDASSET"].Value);
            int idA_Dealer = Convert.ToInt32(pMapDr["IDA_DEALER"].Value);
            int idB_Dealer = Convert.ToInt32(pMapDr["IDB_DEALER"].Value);
            int idA_Clearer = Convert.ToInt32(pMapDr["IDA_CLEARER"].Value);
            int idB_Clearer = 0;
            if (false == Convert.IsDBNull(pMapDr["IDB_CLEARER"].Value))
            {
                idB_Clearer = Convert.ToInt32(pMapDr["IDB_CLEARER"].Value);
            }
            int idA_EntityDealer = 0;
            if (false == Convert.IsDBNull(pMapDr["IDA_ENTITYDEALER"].Value))
            {
                idA_EntityDealer = Convert.ToInt32(pMapDr["IDA_ENTITYDEALER"].Value);
            }
            int idA_EntityClearer = 0;
            if (false == Convert.IsDBNull(pMapDr["IDA_ENTITYCLEARER"].Value))
            {
                idA_EntityClearer = Convert.ToInt32(pMapDr["IDA_ENTITYCLEARER"].Value);
            }
            m_PosKeepingData = m_Product.CreatePosKeepingData();
            m_PosKeepingData.SetPosKey(m_Cs, null, idI, _underlyingAsset, idAsset, idA_Dealer, idB_Dealer, idA_Clearer, idB_Clearer);
            m_PosKeepingData.SetAdditionalInfo(idA_EntityDealer, idA_EntityClearer);

            Cst.PosRequestAssetQuoteEnum assetQuote = pPosRequestAssetQuote;
            if (null != pMapDr["IDASSET_UNDERLYER"])
                assetQuote = Cst.PosRequestAssetQuoteEnum.UnderlyerAsset;
            PosKeepingAsset asset = m_Process.CacheAssetFindLock(_underlyingAsset, idAsset, assetQuote);
            if (null == asset)
            {
                asset = m_Process.SetPosKeepingAsset(pDbTransaction, _underlyingAsset, pMapDr);
                m_Process.CacheAssetAddLock(_underlyingAsset, idAsset, assetQuote, asset);
            }
            m_PosKeepingData.Asset = asset;

            Cst.ErrLevel codeReturn = base.SetPosKeepingTrade(pMapDr);
            if (codeReturn == Cst.ErrLevel.SUCCESS)
            {
                if (pIsQuoteCanBeReaded)
                    SetPosKeepingQuote();

                base.SetPosKeepingMarket();
            }

            return codeReturn;
        }
        #endregion SetPosKeepingData
        #region SetPosKeepingQuote
        /// <summary>
        /// Lecture du sous-jacent exclusivement sur option
        /// </summary>
        /// <returns></returns>
        // EG 20190114 Add detail to ProcessLog Refactoring
        // EG 20190716 [VCL : New FixedIncome] Upd GetQuote
        private void SetPosKeepingQuote()
        {
            Cst.ErrLevel errLevel = Cst.ErrLevel.SUCCESS;
            int idAsset = Asset.idAsset;
            string identifier = Asset.identifier;
            Cst.UnderlyingAsset assetCategory = Cst.UnderlyingAsset.ExchangeTradedContract;

            SystemMSGInfo errReadOfficialSettlement = null;
            SystemMSGInfo errReadOfficialClose = null;

            if ((posRequest is PosRequestMaturityOffsetting) ||
                (posRequest is PosRequestOption) ||
                (posRequest is PosRequestPhysicalPeriodicDelivery))
            {
                // Lecture du sous-jacent exclusivement sur option
                if (Asset.assetCategory_UnderlyerSpecified)
                    assetCategory = Asset.assetCategory_Underlyer;

                if (Asset.idAsset_UnderlyerSpecified && Asset.putCallSpecified)
                {
                    idAsset = Asset.idAsset_Underlyer;
                    identifier = Asset.identifier_Underlyer;
                }
                else if (Asset.putCallSpecified)
                {
                    // Erreur Sous-jacent non défini sur le contrat
                    if (posRequest.IdTSpecified)
                    {
                        string tradeIdentifier = m_PosKeepingData.TradeSpecified ? LogTools.IdentifierAndId(m_PosKeepingData.Trade.Identifier, posRequest.IdT) : posRequest.IdT.ToString();

                        // FI 20200623 [XXXXX] SetErrorWarning
                        m_Process.ProcessBase.ProcessState.SetErrorWarning(ProcessStateTools.StatusWarningEnum);
                        
                        
                        Logger.Log(new LoggerData(LogLevelEnum.Warning, new SysMsgCode(SysCodeEnum.SYS, 5109), 0,
                            new LogParam(LogTools.IdentifierAndId(identifier, idAsset)),
                            new LogParam(assetCategory),
                            new LogParam(tradeIdentifier)));
                    }
                    else
                    {
                        // FI 20200623 [XXXXX] SetErrorWarning
                        m_Process.ProcessBase.ProcessState.SetErrorWarning(ProcessStateTools.StatusWarningEnum);
                        
                        
                        
                        Logger.Log(new LoggerData(LogLevelEnum.Warning, new SysMsgCode(SysCodeEnum.SYS, 5108), 0,
                            new LogParam(LogTools.IdentifierAndId(identifier, idAsset)),
                            new LogParam(assetCategory)));
                    }
                    errLevel = Cst.ErrLevel.DATANOTFOUND;
                }
            }

            if (Cst.ErrLevel.SUCCESS == errLevel)
            {
                bool isEOD = (m_Process.MasterPosRequest.RequestType == Cst.PosRequestTypeEnum.EndOfDay);

                Asset.SetFlagQuoteMandatory(m_Process.MasterPosRequest.DtBusiness, posRequest);

                DateTime dtQuote = Asset.GetSettlementQuoteTime(m_Process.MasterPosRequest.DtBusiness, posRequest);
                Quote quote = m_Process.PKGenProcess.ProcessCacheContainer.GetQuoteLock(idAsset, dtQuote, identifier, 
                    QuotationSideEnum.OfficialSettlement, assetCategory, new KeyQuoteAdditional(), ref errReadOfficialSettlement);
                if (null != quote)
                    m_PosKeepingData.SetQuoteReference((Cst.UnderlyingAsset)Asset.assetCategory, quote, string.Empty);

                quote = m_Process.PKGenProcess.ProcessCacheContainer.GetQuoteLock(idAsset, dtQuote, identifier, 
                    QuotationSideEnum.OfficialClose, assetCategory, new KeyQuoteAdditional(), ref errReadOfficialClose) as Quote;
                if (null != quote)
                    m_PosKeepingData.SetQuote((Cst.UnderlyingAsset)Asset.assetCategory, quote, string.Empty);

                // EG 20140120 Gestion status (ITD/EOD)
                if (Asset.isOfficialSettlementMandatory && (null != errReadOfficialSettlement))
                {
                    if (false == isEOD)
                    {
                        errReadOfficialSettlement.processState.Status = ProcessStateTools.StatusWarningEnum;
                    }
                    m_Process.PKGenProcess.ProcessState.SetErrorWarning(errReadOfficialSettlement.processState.Status);
                    
                    Logger.Log(errReadOfficialSettlement.ToLoggerData(0));
                }
                else if (Asset.isOfficialCloseMandatory && (null != errReadOfficialClose))
                {
                    if (false == isEOD)
                    {
                        errReadOfficialClose.processState.Status = ProcessStateTools.StatusWarningEnum;
                    }
                    m_Process.PKGenProcess.ProcessState.SetErrorWarning(errReadOfficialSettlement.processState.Status);

                    
                    Logger.Log(errReadOfficialClose.ToLoggerData(0));
                }
                else if ((null != errReadOfficialSettlement) && (null != errReadOfficialClose))
                {
                    if (false == isEOD)
                    {
                        errReadOfficialSettlement.processState.Status = ProcessStateTools.StatusWarningEnum;
                        errReadOfficialClose.processState.Status = ProcessStateTools.StatusWarningEnum;
                    }
                    m_Process.PKGenProcess.ProcessState.SetErrorWarning(errReadOfficialSettlement.processState.Status);
                    m_Process.PKGenProcess.ProcessState.SetErrorWarning(errReadOfficialClose.processState.Status);
                    

                    
                    Logger.Log(errReadOfficialSettlement.ToLoggerData(0));
                    Logger.Log(errReadOfficialClose.ToLoggerData(0));
                }
            }
        }
        #endregion SetPosKeepingQuote

        #region SetTemplateDataDocumentLock
        public Cst.ErrLevel SetTemplateDataDocumentLock(IDbTransaction pDbTransaction, IPosRequestDetUnderlyer pUnderlyer)
        {
            lock (Process.TemplateDataDocumentTradeLock)
            {
                return Process.SetTemplateDataDocument(pDbTransaction, pUnderlyer);
            }
        }
        #endregion SetTemplateDataDocumentLock

        #endregion Methods
    }
    #endregion TradeUnderlyerDeliveryData

    #region PosKeepingGen_ETD (ASYNCHRONE)
    public partial class PosKeepingGen_ETD
    {
        /*  ----------------------------------------------*/
        /*  DENOUEMENT AUTOMATIQUE D'OPTIONS A L'ECHEANCE */
        /*  ----------- ASYNCHRONE MODE ------------------*/
        /*  ----------------------------------------------*/
        #region EOD_AutomaticOptionGenThreading
        /// <summary>
        /// DENOUEMENT AUTOMATIQUE D'OPTIONS (EOD)
        /// ASYNCHRONE MODE
        ///<para>Retourne Cst.ErrLevel.IRQ_EXECUTED ou Cst.ErrLevel.SUCCESS</para>
        /// </summary>
        /// <returns>Cst.ErrLevel</returns>
        // EG 20180525 [23979] IRQ Processing
        // EG 20181127 PERF Post RC (Step 3)
        // EG 20190114 Add detail to ProcessLog Refactoring
        protected Cst.ErrLevel EOD_AutomaticOptionGenThreading()
        {
            Cst.ErrLevel codeReturn = Cst.ErrLevel.SUCCESS;


            try
            {
                m_PosRequest = RestoreMarketRequest;
                // INSERTION LOG
                
                Logger.Log(new LoggerData(LogLevelEnum.None, new SysMsgCode(SysCodeEnum.LOG, 5004), 0,
                    new LogParam(LogTools.IdentifierAndId(m_PosRequest.Identifiers.Entity, m_PosRequest.IdA_Entity)),
                    new LogParam(LogTools.IdentifierAndId(m_PosRequest.Identifiers.CssCustodian, m_PosRequest.IdA_CssCustodian)),
                    new LogParam(LogTools.IdentifierAndId(m_PosRequest.Identifiers.Market, m_PosRequest.IdM)),
                    new LogParam(m_PosRequest.GroupProductValue),
                    new LogParam(DtFunc.DateTimeToStringDateISO(m_PosRequest.DtBusiness)),
                    new LogParam("YES"),
                    new LogParam(Convert.ToString(m_PKGenProcess.GetHeapSize(ParallelProcess.AutomaticOption)) + "/" + Convert.ToString(m_PKGenProcess.GetMaxThreshold(ParallelProcess.AutomaticOption)))));
                Logger.Write();

                // PL 20180312 WARNING: Use Read Commited !
                //DataSet ds = GetDataRequestWithIsolationLevel(CS, IsolationLevel.ReadUncommitted, Cst.PosRequestTypeEnum.AutomaticOptionExercise,
                //    m_MasterPosRequest.dtBusiness, m_PosRequest.idEM, 480, null, null);
                DataSet ds = GetDataRequestWithIsolationLevel(CS, IsolationLevel.ReadCommitted, Cst.PosRequestTypeEnum.AutomaticOptionExercise,
                    m_MasterPosRequest.DtBusiness, m_PosRequest.IdEM, 480, null, null);

                if (null != ds)
                {
                    int nbRows = ds.Tables[0].Rows.Count;

                    IEnumerable<IGrouping<string, IGrouping<IPosKeepingKey, DataRow>>> groupTradesByPosKeepingKey = null;

                    if (0 < nbRows)
                    {
                        // Construction sur la base d'un DataTable (Trades candidats à dénouement) d'un énumérateur avec constitution d'une clé 
                        // de regroupement pour chaque trade (IPosKeepingKey = clé de position)
                        // Ex : <Key1, Trade1>, 
                        //      <Key2, Trade2>,
                        //      <Key2, Trade4>,
                        //      <Key3, Trade3>,
                        IEnumerable<IGrouping<IPosKeepingKey, DataRow>> posKeepingKeyGroups =
                            from row in ds.Tables[0].AsEnumerable()
                            group row by PosKeepingTools.SetPosKeepingKey(m_Product, row) into groupPosKeepingKey
                            select groupPosKeepingKey;

                        // Regroupement des trades constitués précédemment par clé de position 
                        // Ex : <Key1, <Key1, Trade1>>>, 
                        //      <Key2, <Key2, Trade2>,
                        //             <Key2, Trade4>>,
                        //      <Key3, <Key3, Trade3>>,
                        groupTradesByPosKeepingKey = posKeepingKeyGroups.GroupBy(posKeepingKeyGroup => posKeepingKeyGroup.Key.LockObjectId);
                        nbRows += groupTradesByPosKeepingKey.Count();
                    }

                    // Token GETID POSREQUEST = 1 (DENOUEMENT AUTOMATIQUE) + nbRows (NB TRADES A DENOUER + NB CLE DE POSITION)
                    int newIDPR = Initialize_IdPRAsync(1 + nbRows);

                    // Insert POSREQUEST REGROUPEMENT
                    ProcessStateTools.StatusEnum status = ((0 < nbRows) ? ProcessStateTools.StatusProgressEnum : ProcessStateTools.StatusNoneEnum);
                    IPosRequest posRequestGroupLevel = InsertPosRequestGroupLevel(Cst.PosRequestTypeEnum.EOD_AutomaticOptionGroupLevel, newIDPR, m_PosRequest, m_PosRequest.IdEM,
                        status, m_PosRequest.IdPR, m_LstSubPosRequest, m_MarketPosRequest.GroupProductEnum);


                    if (0 < nbRows)
                    {
                        
                        Logger.Write();

                        m_PosRequest = posRequestGroupLevel;

                        ProcessBase.InitializeMaxThreshold(ParallelProcess.AutomaticOption);
                        int heapSize = ProcessBase.GetHeapSize(ParallelProcess.AutomaticOption);
                        List<List<IGrouping<string, IGrouping<IPosKeepingKey, DataRow>>>> lstRows = ListExtensionsTools.ChunkBy(groupTradesByPosKeepingKey.ToList(), heapSize);

                        int counter = 1;
                        int totSubList = lstRows.Count();
                        if (0 < totSubList)
                        {
                            lstRows.ForEach(subLstRows =>
                            {
                                if ((Cst.ErrLevel.IRQ_EXECUTED != codeReturn) &&
                                    (false == IRQTools.IsIRQRequested(PKGenProcess, PKGenProcess.IRQNamedSystemSemaphore, ref codeReturn)))
                                {
                                    if ((1 < heapSize) || (1 == totSubList))
                                    {
                                        
                                        Logger.Log(new LoggerData(LogLevelEnum.Info, String.Format("Parallel Automatic Option generation {0}/{1}", counter, totSubList), 2));
                                        Logger.Write();
                                    }
                                    Cst.ErrLevel ret = AutomaticOptionGenAsync(subLstRows, newIDPR).Result;
                                    counter++;
                                }
                            });

                        }
                        if (Cst.ErrLevel.IRQ_EXECUTED == codeReturn)
                        {
                            
                            //PosKeepingTools.UpdateIRQPosRequestGroupLevel(CS, posRequestGroupLevel, m_PKGenProcess.appInstance, LogHeader.IdProcess);
                            PosKeepingTools.UpdateIRQPosRequestGroupLevel(CS, posRequestGroupLevel, m_PKGenProcess.Session, IdProcess);
                        }
                        else
                        {
                            UpdatePosRequestGroupLevel(posRequestGroupLevel);
                        }
                    }
                }
            }
            catch
            {
                throw;
            }
            finally
            {
                
                
                Logger.Write();
            }
            return codeReturn;
        }
        #endregion EOD_AutomaticOptionGenThreading

        #region AddPosRequestAutomaticOption
        /// <summary>
        /// Insertion d'un POSREQUEST pour le dénouement automatique d'option avec détermination du type de dénouement en fonction de la règle suivante :
        ///<para>────────────────────────────────────────────────────────────────────────────────────────────────</para>
        ///<para> 1 ► Column ISAUTOABN</para>
        ///<para>    ● Si = 1 dans la table ENTITYMARKET alors : ABANDON</para>
        ///<para>    ● Sinon Si = 1 dans la table MARKET alors : ABANDON</para>
        ///<para>────────────────────────────────────────────────────────────────────────────────────────────────</para>
        ///<para> Calcul ITM/OTM</para>
        ///<para> ► Recherche d’une cotation</para>
        ///<para>   ● SIDE   : OfficialSettlement</para>
        ///<para>   ● TIMING : Close</para>
        ///<para> ► Si cotation non trouvée, alors seconde recherche pour une cotation :</para>
        ///<para>   ● SIDE   : OfficialClose</para>
        ///<para>   ● TIMING : Close</para>
        /// 
        ///<para> ► Si cotation non trouvée, alors ABANDON</para>
        /// 
        ///<para> 2.1 ► Call</para>
        ///<para>       ● Si STRIKE supérieur COURS(ASSET_UNL) alors : ABANDON (OTM)</para>
        ///<para>       ● Si STRIKE inférieur ou égal COURS(ASSET_UNL) alors : EXE (Si Achat) ASS (si Vente) (ITM)</para>
        ///   
        ///<para> 2.2 ► Put</para>
        ///<para>       ● Si STRIKE supérieur ou égal COURS(ASSET_UNL) alors : EXE (Si Achat) ASS (si Vente) (ITM)</para>
        ///<para>       ● Si STRIKE inférieur COURS(ASSET_UNL)  alors : ABANDON (OTM)</para>
        ///<para>────────────────────────────────────────────────────────────────────────────────────────────────</para>
        /// </summary>
        /// <param name="pMaturityOffsettingOptionData">Données utiles au traitement (clé de postion, info marché, cours etc.)</param>
        /// <param name="pTrade">DataRow (Caractéristiques du trade)</param>
        /// <returns></returns>
        // EG 20190114 Add detail to ProcessLog Refactoring
        // EG 20220627 [26082] Alimentation des données du trade pour logger
        private IPosRequest AddPosRequestAutomaticOption(MaturityOffsettingOptionData pMaturityOffsettingOptionData, DataRow pTrade)
        {

            IPosRequest _posRequest = m_Product.CreatePosRequestOption(Cst.PosRequestTypeEnum.AutomaticOptionAbandon, SettlSessIDEnum.EndOfDay, m_MarketPosRequest.DtBusiness, 0);
            _posRequest.IdTSpecified = true;
            _posRequest.IdT = Convert.ToInt32(pTrade["IDT"]);

            IPosKeepingData posKeepingData = pMaturityOffsettingOptionData.PosKeepingData;
            PosKeepingAsset_ETD asset = pMaturityOffsettingOptionData.Asset;
            int idPR_Parent = pMaturityOffsettingOptionData.PosRequest.IdPR;
            SettlementPrice settlementPrice = pMaturityOffsettingOptionData.SettlementPriceData.Item2;

            int idT = Convert.ToInt32(pTrade["IDT"]);
            string identifier = pTrade["IDENTIFIER"].ToString();
            decimal qty = Convert.ToDecimal(pTrade["QTY"]);

            // Lecture du prix de règlement et Détermination ITM/OTM
            Cst.PosRequestTypeEnum requestType = Cst.PosRequestTypeEnum.AutomaticOptionAbandon;

            if (DtFunc.IsDateTimeFilled(settlementPrice.dtPrice) && (false == Convert.ToBoolean(pTrade["ISAUTOABN"])))
                requestType = pMaturityOffsettingOptionData.GetRequestTypeAutomaticOption(IsTradeBuyer(pTrade["SIDE"].ToString()), settlementPrice.price);

            IPosRequest posRequest = PosKeepingTools.CreatePosRequestOption(CS, m_Product, requestType, SettlSessIDEnum.EndOfDay,
                    idT, identifier, m_MarketPosRequest.IdA_Entity, m_MarketPosRequest.IdA_Css, m_MarketPosRequest.IdA_Custodian, m_MarketPosRequest.IdEM,
                    DtBusiness, qty, qty, asset.strikePrice,
                    pMaturityOffsettingOptionData.AssetCategory,
                    settlementPrice.idAsset, settlementPrice.assetIdentifier, QuoteTimingEnum.Close,
                    settlementPrice.price, settlementPrice.dtPrice, settlementPrice.source, null, null, null,
                    ProductTools.GroupProductEnum.ExchangeTradedDerivative);


            posRequest.GroupProductSpecified = (m_MarketPosRequest.GroupProductSpecified);
            if (posRequest.GroupProductSpecified)
                posRequest.GroupProduct = m_MarketPosRequest.GroupProduct;

            ProcessStateTools.StatusEnum status = ProcessStateTools.StatusProgressEnum;
            if (Cst.ErrLevel.DATAREJECTED == pMaturityOffsettingOptionData.SettlementPriceData.Item1)
            {
                status = ProcessStateTools.StatusWarningEnum;

                // FI 20200623 [XXXXX] SetErrorWarning
                m_PKGenProcess.ProcessState.SetErrorWarning(status);

                
                Logger.Log(new LoggerData(LoggerTools.StatusToLogLevelEnum(status), new SysMsgCode(SysCodeEnum.SYS, 5190), 0,
                    new LogParam(GetPosRequestLogValue(requestType)),
                    new LogParam(LogTools.IdentifierAndId(Queue.GetStringValueIdInfoByKey("ENTITY"), m_MasterPosRequest.IdA_Entity)),
                    new LogParam(LogTools.IdentifierAndId(Queue.GetStringValueIdInfoByKey("CSSCUSTODIAN"), m_MasterPosRequest.IdA_CssCustodian)),
                    new LogParam(LogTools.IdentifierAndId(m_MarketPosRequest.Identifiers.Market, m_MarketPosRequest.IdM)),
                    new LogParam(m_MarketPosRequest.GroupProductValue),
                    new LogParam(LogTools.IdentifierAndId(asset.identifier, asset.idAsset)),
                    new LogParam(DtFunc.DateTimeToStringISO(asset.maturityDate)),
                    new LogParam(DtFunc.DateTimeToStringDateISO(m_MasterPosRequest.DtBusiness)),
                    new LogParam(TradeForLog(posKeepingData, posRequest))));
            }

            int newIdPR = Next_IdPRAsync();
            InitStatusPosRequest(posRequest, newIdPR, idPR_Parent, status);
            AddSubPosRequest(m_LstSubPosRequest, posRequest);

            using (IDbTransaction dbTransaction = DataHelper.BeginTran(CS))
            {
                try
                {
                    // Calcul des frais associés au dénouement
                    IPayment[] oppFees = CalcMaturityOffsettingOptionsPayment(CS, dbTransaction, idT, requestType, qty, m_MasterPosRequest.DtBusiness, m_PKGenProcess.Session);

                    IPosRequestDetOption detail = posRequest.DetailBase as IPosRequestDetOption;
                    detail.PaymentFeesSpecified = ArrFunc.IsFilled(oppFees);
                    if (detail.PaymentFeesSpecified)
                        detail.PaymentFees = oppFees;
                    detail.FeeCalculationSpecified = false;

                    
                    //PosKeepingTools.InsertPosRequest(CS, dbTransaction, newIdPR, posRequest, m_PKGenProcess.appInstance, LogHeader.IdProcess, idPR_Parent);
                    PosKeepingTools.InsertPosRequest(CS, dbTransaction, newIdPR, posRequest, m_PKGenProcess.Session, IdProcess, idPR_Parent);

                    DataHelper.CommitTran(dbTransaction);
                }
                catch (Exception)
                {
                    DataHelper.RollbackTran(dbTransaction);
                }
            }
            return posRequest;

        }
        #endregion AddPosRequestAutomaticOption

        #region AutomaticOptionGenAsync
        private async Task<Cst.ErrLevel> AutomaticOptionGenAsync(List<IGrouping<string, IGrouping<IPosKeepingKey, DataRow>>> pLstPosKeepingKey, int pIdPRParent)
        {
            Cst.ErrLevel ret = Cst.ErrLevel.SUCCESS;
            CancellationTokenSource cts = new CancellationTokenSource();
            List<Task<Cst.ErrLevel>> getReturnTasks = null;

            try
            {
                // Création des tâches de traitement des dénouements pour chaque clé de position
                IEnumerable<Task<Cst.ErrLevel>> getReturnTasksQuery =
                    from posKeepingKey in pLstPosKeepingKey
                    select AutomaticOptionByPosKeepingKeyAsync(posKeepingKey, pIdPRParent, cts.Token);

                getReturnTasks = getReturnTasksQuery.ToList();

                // Boucle de traitement asynchrone des dénouements des trades (une tâche = une clé de position)
                while (0 < getReturnTasks.Count)
                {
                    Task<Cst.ErrLevel> firstFinishedTask = await Task.WhenAny(getReturnTasks);
                    getReturnTasks.Remove(firstFinishedTask);
                    // EG 20230929 [26506] Ajout du message d'erreur dans la trace pour les traitements en multithreading
                    //if (firstFinishedTask.IsFaulted)
                    //    throw firstFinishedTask.Exception.Flatten();
                    ProcessTools.AddTraceExceptionAndProcessStateFailure(this, firstFinishedTask, "AutomaticOptionGenAsync", null, true);
                }

            }
            catch (Exception)
            {
                ret = Cst.ErrLevel.FAILURE;
                throw;
            }
            return ret;
        }

        #endregion AutomaticOptionGenAsync
        #region AutomaticOptionByPosKeepingKeyAsync
        /// <summary>
        /// Traitement des dénouements automatiques d'options à l'échéance effectué pour une clé de position 
        /// </summary>
        /// <param name="pGroupTrades">Liste des trades </param>
        /// <param name="pIdPRParent">Identifiant du posRequest parent (EOD_AutomaticOptionGroupLevel)</param>
        /// <param name="pCt"></param>
        /// <returns></returns>
        private async Task<Cst.ErrLevel> AutomaticOptionByPosKeepingKeyAsync(IGrouping<string, IGrouping<IPosKeepingKey, DataRow>> pGroupTrades, int pIdPRParent, CancellationToken pCt)
        {
            Cst.ErrLevel ret = Cst.ErrLevel.SUCCESS;
            AppInstance.TraceManager.TraceInformation(this, String.Format("START AutomaticOptionByPosKeepingKeyAsync {0} / {1}",
                pGroupTrades.Key, pGroupTrades.Count()));

             
            MaturityOffsettingOptionData maturityOffsettingOptionData = null;


            try
            {
                IPosKeepingKey posKeepingKey = pGroupTrades.FirstOrDefault().Key;
                DataRow rowTrade = pGroupTrades.FirstOrDefault().FirstOrDefault();

                // Initialise les données essentielles au traitement : CLE DE POSITION / ASSET / COURS
                // Alimentation d'une nouvelle entrée dans POSREQUEST (MOO)
                maturityOffsettingOptionData = SetMaturityOffsettingOptionData(posKeepingKey, rowTrade, pIdPRParent);

                // Création des tâches de traitement des dénouements pour chaque trades de la clé de position
                IEnumerable<Task<Cst.ErrLevel>> getReturnTasksQuery =
                    from trades in pGroupTrades
                    select AutomaticOptionByTradeAsync(maturityOffsettingOptionData, trades.FirstOrDefault(), pCt);

                List<Task<Cst.ErrLevel>> getReturnTasks = getReturnTasksQuery.ToList();

                // Boucle de traitement asynchrone des dénouements des trades (une tâche = n trades d'une même clé de position)
                while (0 < getReturnTasks.Count)
                {
                    Task<Cst.ErrLevel> firstFinishedTask = await Task.WhenAny(getReturnTasks);
                    getReturnTasks.Remove(firstFinishedTask);
                    // EG 20230929 [26506] Ajout du message d'erreur dans la trace pour les traitements en multithreading
                    //if (firstFinishedTask.IsFaulted)
                    //    throw firstFinishedTask.Exception.Flatten();
                    ProcessTools.AddTraceExceptionAndProcessStateFailure(this, firstFinishedTask, "AutomaticOptionByPosKeepingKeyAsync", null, true);
                }

                return ret;
            }
            catch (Exception)
            {
                ret = Cst.ErrLevel.FAILURE;
                throw;
            }
            finally
            {
                if (null != maturityOffsettingOptionData)
                {
                    maturityOffsettingOptionData.UpdatePosRequest();
                }

                AppInstance.TraceManager.TraceInformation(this, String.Format("STOP AutomaticOptionByPosKeepingKeyAsync {0} / {1}",
                    pGroupTrades.Key, pGroupTrades.Count()));

            }
        }
        #endregion AutomaticOptionByPosKeepingKeyAsync
        #region AutomaticOptionByTradeAsync
        /// <summary>
        /// Mise en file d'attente de la tâche de dénouement d'un trade
        /// </summary>
        /// <param name="pMaturityOffsettingOptionData">Données utiles au traitement (clé de position, actif, cours, etc.)</param>
        /// <param name="pTrade">Données propres au trade à traiter</param>
        /// <param name="pCt">Token de notification d'annulation</param>
        /// <returns></returns>
        private async Task<Cst.ErrLevel> AutomaticOptionByTradeAsync(MaturityOffsettingOptionData pMaturityOffsettingOptionData, DataRow pTrade, CancellationToken pCt)
        {
            Cst.ErrLevel ret = Cst.ErrLevel.SUCCESS;
            string key = String.Format("(posKeepingKey: {0} )", pMaturityOffsettingOptionData.PosKeepingKey.LockObjectId);
            string wait = "START AutomaticOptionByTradeAsync Wait   : {0} " + key;
            string release = "STOP  AutomaticOptionByTradeAsync Release: {0} " + key;

            bool isSemaphoreSpecified = (null != ProcessBase.SemaphoreAsync);

            await Task.Run(() => 
                {
                    try
                    {
                        if (isSemaphoreSpecified)
                        {
                            ProcessBase.SemaphoreAsync.Wait();
                            AppInstance.TraceManager.TraceVerbose(this, String.Format(wait, ProcessBase.SemaphoreAsync.CurrentCount));
                        }
                        ret = AutomaticOptionByTrade(pMaturityOffsettingOptionData, pTrade);
                    }
                    catch (Exception) { throw; }
                    finally
                    {
                        if (isSemaphoreSpecified)
                        {
                            ProcessBase.SemaphoreAsync.Release();
                            AppInstance.TraceManager.TraceVerbose(this, String.Format(release, ProcessBase.SemaphoreAsync.CurrentCount));
                        }
                    }
                }
                , pCt);

            return ret;
        }
        #endregion AutomaticOptionByTradeAsync
        #region AutomaticOptionByTrade
        /// <summary>
        /// Traitement d'un dénouement
        /// </summary>
        /// <param name="pMaturityOffsettingOptionData">Données utiles au traitement (clé de position, actif, cours, etc.)</param>
        /// <param name="pTrade">Données propres au trade à traiter</param>
        /// <returns></returns>
        // EG 20180530 [23980] Gestion isException on TradeOptionGen
        // EG 20190114 Add detail to ProcessLog Refactoring
        private Cst.ErrLevel AutomaticOptionByTrade(MaturityOffsettingOptionData pMaturityOffsettingOptionData, DataRow pTrade)
        {
            Cst.ErrLevel ret = Cst.ErrLevel.SUCCESS;

            string logTradeIdent = LogTools.IdentifierAndId(pTrade["IDENTIFIER"].ToString(), Convert.ToInt32(pTrade["IDT"]));
            AppInstance.TraceManager.TraceInformation(this, String.Format("START AutomaticOptionByTrade Trade {0}", logTradeIdent));

            IPosRequest _posRequest = null;
            try
            {
                if (RequestCanBeExecuted(Cst.PosRequestTypeEnum.UpdateEntry))
                {
                    // Début de traitement d'une option : INSERT POSREQUEST (AUTOABN / AUTOASS / AUTOEXE)
                    _posRequest = AddPosRequestAutomaticOption(pMaturityOffsettingOptionData, pTrade);
                    // Initialise les données essentielles au traitement : TRADE
                    TradeOptionData tradeData = new TradeOptionData(this, pMaturityOffsettingOptionData, _posRequest);
                    ret = tradeData.Initialize(_posRequest.IdT, Cst.PosRequestAssetQuoteEnum.UnderlyerAsset, true);
                    if ((Cst.ErrLevel.SUCCESS == ret) && (false == ProcessStateTools.IsStatusErrorWarning(_posRequest.Status)))
                    {
                        if (tradeData.IsQuoteOk)
                        {
                            ExecAutomaticOptionByTrade2(tradeData);
                        }
                        else
                        {
                            ret = Cst.ErrLevel.QUOTENOTFOUND;
                            // FI 20200623 [XXXXX] SetErrorWarning
                            m_PKGenProcess.ProcessState.SetErrorWarning(ProcessStateTools.StatusErrorEnum);

                            
                            Logger.Log(new LoggerData(LogLevelEnum.Error, new SysMsgCode(SysCodeEnum.SYS, 5189), 0,
                                new LogParam(GetPosRequestLogValue(pMaturityOffsettingOptionData.PosRequest.RequestType)),
                                new LogParam(LogTools.IdentifierAndId(Queue.GetStringValueIdInfoByKey("ENTITY"), m_MasterPosRequest.IdA_Entity)),
                                new LogParam(LogTools.IdentifierAndId(Queue.GetStringValueIdInfoByKey("CSSCUSTODIAN"), m_MasterPosRequest.IdA_CssCustodian)),
                                new LogParam(LogTools.IdentifierAndId(m_MarketPosRequest.Identifiers.Market, m_MarketPosRequest.IdM)),
                                new LogParam(m_MarketPosRequest.GroupProductValue),
                                new LogParam(LogTools.IdentifierAndId(pMaturityOffsettingOptionData.Asset.identifier, pMaturityOffsettingOptionData.Asset.idAsset)),
                                new LogParam(DtFunc.DateTimeToStringDateISO(m_MasterPosRequest.DtBusiness)),
                                new LogParam(LogTools.IdentifierAndId(tradeData.identifier, tradeData.idT)),
                                new LogParam(tradeData.qty),
                                new LogParam(pMaturityOffsettingOptionData.Asset.GetLogQuoteInformationRelativeTo(pMaturityOffsettingOptionData.PosRequest))));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ret = Cst.ErrLevel.FAILURE;

                // FI 20200623 [XXXXX] SetErrorWarning
                m_PKGenProcess.ProcessState.SetErrorWarning(ProcessStateTools.StatusErrorEnum);

                // FI 20200623 [XXXXX] AddCriticalException
                m_PKGenProcess.ProcessState.AddCriticalException(ex);

                
                Logger.Log(new LoggerData(SpheresExceptionParser.GetSpheresException(null, ex)));
                Logger.Log(new LoggerData(LogLevelEnum.Error, new SysMsgCode(SysCodeEnum.SYS, 5101), 0,
                    new LogParam(GetPosRequestLogValue(m_PosRequest.RequestType)),
                    new LogParam(DtFunc.DateTimeToStringDateISO(m_PosRequest.DtBusiness))));
            }
            finally
            {
                if (null != _posRequest)
                {
                    if (0 < _posRequest.IdPR)
                        UpdatePosRequest(ret, _posRequest, pMaturityOffsettingOptionData.PosRequest.IdPR);
                }
                AppInstance.TraceManager.TraceInformation(this, String.Format("STOP AutomaticOptionByTrade Trade {0}", logTradeIdent));
            }
            return ret;
        }
        #endregion AutomaticOptionByTrade

        /// <summary>
        /// Usage de TryMultiple qui s'appuie uniquement sur le Logger
        /// </summary>
        /// <param name="pTradeOptionData"></param>
        /// <returns></returns>
        /// FI 20210127 [XXXXX] Add
        private Cst.ErrLevel ExecAutomaticOptionByTrade2(TradeOptionData pTradeOptionData)
        {
            Cst.ErrLevel ret = Cst.ErrLevel.SUCCESS;
            string logTradeIdent = LogTools.IdentifierAndId(pTradeOptionData.identifier, pTradeOptionData.idT);

            TryMultiple tryMultiple = new TryMultiple(CS, "AutomaticOptionByTrade", $"[PosRequest (Id:{pTradeOptionData.posRequest.IdPR}, trade:{logTradeIdent})]")
            {
                SetErrorWarning = m_PKGenProcess.ProcessState.SetErrorWarning,
                IsModeTransactional = true,
                ThreadSleep = 5 //blocage de 5 secondes entre chaque tentative
            };
            ret = tryMultiple.Exec<IDbTransaction, Cst.ErrLevel>(delegate (IDbTransaction arg1) { return pTradeOptionData.TradeOptionGen(arg1); }, null);
            return ret;
        }




        #region SetMaturityOffsettingOptionData
        /// <summary>
        /// Ininitalisation des données utiles au traitement
        /// </summary>
        /// <param name="pPosKeepingKey">Clé de position</param>
        /// <param name="pRow"></param>
        /// <param name="pIdPR_Parent">Identifiant du posRequest parent (EOD_AutomaticOptionGroupLevel)</param>
        /// <returns></returns>
        private MaturityOffsettingOptionData SetMaturityOffsettingOptionData(IPosKeepingKey pPosKeepingKey, DataRow pRow, int pIdPR_Parent)
        {
            // Initialise les données essentielles au traitement (la clé de position, l'asset avec sa cotation, le marché etc..)
            MaturityOffsettingOptionData maturityOffsettingOptionData = new MaturityOffsettingOptionData(this);
            maturityOffsettingOptionData.Initialize(pPosKeepingKey);

            // Recherche POSREQUEST existant
            IPosRequest posRequest = GetSubRequest(Cst.PosRequestTypeEnum.MaturityOffsettingOption, Convert.ToInt32(m_MarketPosRequest.IdEM), pPosKeepingKey);
            if (null == posRequest)
            {
                // Insertion POSREQUEST
                posRequest = PosKeepingTools.SetPosRequestMaturityOffsetting(m_Product, Cst.PosRequestTypeEnum.MaturityOffsettingOption, SettlSessIDEnum.EndOfDay, pRow);
                posRequest.IdTSpecified = false;
                posRequest.GroupProductSpecified = (m_MarketPosRequest.GroupProductSpecified);
                if (posRequest.GroupProductSpecified)
                    posRequest.GroupProduct = m_MarketPosRequest.GroupProduct;

                int newIdPR = Next_IdPRAsync();
                InitStatusPosRequest(posRequest, newIdPR, pIdPR_Parent, ProcessStateTools.StatusProgressEnum);
                AddSubPosRequest(m_LstSubPosRequest, posRequest);
                
                //PosKeepingTools.InsertPosRequest(CS, null, newIdPR, posRequest, m_PKGenProcess.appInstance, LogHeader.IdProcess, pIdPR_Parent);
                PosKeepingTools.InsertPosRequest(CS, null, newIdPR, posRequest, m_PKGenProcess.Session, IdProcess, pIdPR_Parent);
            }

            if (null == posRequest.Identifiers)
                posRequest.Identifiers = m_Product.CreatePosRequestKeyIdentifier();

            posRequest.IdentifiersSpecified = true;
            posRequest.Identifiers.Dealer = pRow["ACTORDEALER_IDENTIFIER"].ToString();
            posRequest.Identifiers.BookDealer = pRow["BOOKDEALER_IDENTIFIER"].ToString();
            maturityOffsettingOptionData.PosRequest = posRequest;
            return maturityOffsettingOptionData;
        }
        #endregion SetMaturityOffsettingOptionData

        /*  ----------------------------------------------*/
        /*  LIVRAISON SOUS-JACENT                         */
        /*  ----------- ASYNCHRONE MODE ------------------*/
        /*  ----------------------------------------------*/
        #region EOD_UnderlyerDeliveryGenThreading
        /// <summary>
        /// LIVRAISON SOUS-JACENT (EOD)
        /// ASYNCHRONE MODE
        /// </summary>
        /// <returns>Cst.ErrLevel</returns>
        // EG 20180525 [23979] IRQ Processing
        // EG 20181127 PERF Post RC (Step 3)
        // EG 20190114 Add detail to ProcessLog Refactoring
        protected Cst.ErrLevel EOD_UnderlyerDeliveryGenThreading()
        {
            Cst.ErrLevel codeReturn = Cst.ErrLevel.SUCCESS;
            IDbTransaction dbTransaction = null;
            bool isException = false;
            try
            {
                m_PosRequest = RestoreMarketRequest;
                
                Logger.Log(new LoggerData(LogLevelEnum.None, new SysMsgCode(SysCodeEnum.LOG, 5005), 0,
                    new LogParam(LogTools.IdentifierAndId(m_PosRequest.Identifiers.Entity, m_PosRequest.IdA_Entity)),
                    new LogParam(LogTools.IdentifierAndId(m_PosRequest.Identifiers.CssCustodian, m_PosRequest.IdA_CssCustodian)),
                    new LogParam(LogTools.IdentifierAndId(m_PosRequest.Identifiers.Market, m_PosRequest.IdM)),
                    new LogParam(m_PosRequest.GroupProductValue),
                    new LogParam(DtFunc.DateTimeToStringDateISO(m_PosRequest.DtBusiness)),
                    new LogParam("YES"),
                    new LogParam(Convert.ToString(m_PKGenProcess.GetHeapSize(ParallelProcess.UnderlyerDelivery)) + "/" + Convert.ToString(m_PKGenProcess.GetMaxThreshold(ParallelProcess.UnderlyerDelivery)))));
                Logger.Write();
                
                // PL 20180312 WARNING: Use Read Commited !
                //DataSet ds = GetDataRequestWithIsolationLevel(CS, IsolationLevel.ReadUncommitted, Cst.PosRequestTypeEnum.UnderlyerDelivery,
                //    m_MasterPosRequest.dtBusiness, m_PosRequest.idEM);
                DataSet ds = GetDataRequestWithIsolationLevel(CS, IsolationLevel.ReadCommitted, Cst.PosRequestTypeEnum.UnderlyerDelivery,
                    m_MasterPosRequest.DtBusiness, m_PosRequest.IdEM);

                if (null != ds)
                {
                    int nbRows = ds.Tables[0].Rows.Count;
                    // Insert POSREQUEST REGROUPEMENT
                    dbTransaction = DataHelper.BeginTran(CS);
                    SQLUP.GetId(out int newIDPR, dbTransaction, SQLUP.IdGetId.POSREQUEST, SQLUP.PosRetGetId.First, 1);
                    DataHelper.CommitTran(dbTransaction);

                    ProcessStateTools.StatusEnum status = ((0 < nbRows) ? ProcessStateTools.StatusProgressEnum : ProcessStateTools.StatusNoneEnum);
                    IPosRequest posRequestGroupLevel = InsertPosRequestGroupLevel(Cst.PosRequestTypeEnum.EOD_UnderlyerDeliveryGroupLevel, newIDPR, m_PosRequest, m_PosRequest.IdEM,
                        status, m_PosRequest.IdPR, m_LstSubPosRequest, m_MarketPosRequest.GroupProductEnum);

                    if (0 < nbRows)
                    {
                        
                        Logger.Write();

                        m_PosRequest = posRequestGroupLevel;
                        int heapSize = ProcessBase.GetHeapSize(ParallelProcess.UnderlyerDelivery);
                        List<List<DataRow>> lstRows = ListExtensionsTools.ChunkBy(ds.Tables[0].AsEnumerable().ToList(), heapSize);
                        int counter = 1;
                        int totSubList = lstRows.Count();
                        if (0 < totSubList)
                        {
                            lstRows.ForEach(subLstRows =>
                            {
                                if ((Cst.ErrLevel.IRQ_EXECUTED != codeReturn) &&
                                    (false == IRQTools.IsIRQRequested(PKGenProcess, PKGenProcess.IRQNamedSystemSemaphore, ref codeReturn)))
                                {
                                    ProcessBase.InitializeMaxThreshold(ParallelProcess.UnderlyerDelivery);
                                    if ((1 < heapSize) || (1 == totSubList))
                                    {
                                        
                                        Logger.Log(new LoggerData(LogLevelEnum.Info, String.Format("Parallel Underlyer Delivery generation {0}/{1}", counter, totSubList), 2));
                                        Logger.Write();
                                    }
                                    Cst.ErrLevel ret = UnderlyerDeliveryGenAsync(subLstRows, posRequestGroupLevel).Result;
                                    counter++;
                                }
                            });
                        }
                        if (Cst.ErrLevel.IRQ_EXECUTED == codeReturn)
                        {
                            
                            //PosKeepingTools.UpdateIRQPosRequestGroupLevel(CS, posRequestGroupLevel, m_PKGenProcess.appInstance, LogHeader.IdProcess);
                            PosKeepingTools.UpdateIRQPosRequestGroupLevel(CS, posRequestGroupLevel, m_PKGenProcess.Session, IdProcess);
                        }
                        else
                        {
                            UpdatePosRequestGroupLevel(posRequestGroupLevel);
                        }
                    }
                }
            }
            catch
            {
                isException = true;
                throw;
            }
            finally
            {
                if (null != dbTransaction)
                {
                    if (isException && (null != dbTransaction.Connection))
                        DataHelper.RollbackTran(dbTransaction);
                    dbTransaction.Dispose();
                }
                
                Logger.Write();

            }
            return codeReturn;
        }
        #endregion EOD_UnderlyerDeliveryGenThreading

        #region UnderlyerDeliveryGenAsync
        // EG 20210705 [XXXXX] Génération des événements déplacé dans le finally (pour pouvoir générer les événements de trades générés dans le cas où une erreur interviendrait ensuite)
        // EG 20210705 [XXXXX] Test existence sur les énumérateurs : tradesEquityAsset|tradesFutures pour génération événements
        // EG 20210707 [XXXXX] Test (null!=trade) sur construction des trades candidats à génération des événements : tradesEquityAsset|tradesFutures 
        private async Task<Cst.ErrLevel> UnderlyerDeliveryGenAsync(List<DataRow> pLstRows, IPosRequest pPosRequestGroupLevel)
        {
            Cst.ErrLevel ret = Cst.ErrLevel.SUCCESS;
            CancellationTokenSource cts = new CancellationTokenSource();
            List<Task<Tuple<int, string, Cst.UnderlyingAsset>>> getReturnTasks = null;

            List<Tuple<int, string, Cst.UnderlyingAsset>> lstTradesResults = new List<Tuple<int, string, Cst.UnderlyingAsset>>();

            try
            {
                // Création des tâches de traitement de livraison pour chaque trade candidat
                IEnumerable<Task<Tuple<int, string, Cst.UnderlyingAsset>>> getReturnTasksQuery =
                    from row in pLstRows.AsEnumerable()
                    select UnderlyerDeliveryGenByTradeAsync(row, pPosRequestGroupLevel, cts.Token);

                getReturnTasks = getReturnTasksQuery.ToList();

                // Boucle de traitement asynchrone des livraisons de sous-jacent (une tâche = un trade)
                while (0 < getReturnTasks.Count)
                {
                    Task<Tuple<int, string, Cst.UnderlyingAsset>> firstFinishedTask = await Task.WhenAny(getReturnTasks);
                    getReturnTasks.Remove(firstFinishedTask);
                    // EG 20230929 [26506] Ajout du message d'erreur dans la trace pour les traitements en multithreading
                    //if (firstFinishedTask.IsFaulted)
                    //    throw firstFinishedTask.Exception.Flatten();
                    ProcessTools.AddTraceExceptionAndProcessStateFailure(this, firstFinishedTask, "UnderlyerDeliveryGenAsync", null, true);

                    lstTradesResults.Add(firstFinishedTask.Result);
                }
            }
            catch (Exception)
            {
                ret = Cst.ErrLevel.FAILURE;
                throw;
            }
            finally
            {
                #region Génération des événements
                // EG 20210707 [XXXXX] Test (null!=trade) sur construction des trades candidats à génération des événements : tradesEquityAsset|tradesFutures 
                if (0 < lstTradesResults.Count)
                {
                    ProcessBase.InitializeMaxThresholdEvents(ParallelProcess.UnderlyerDelivery);

                    IEnumerable<Pair<int, string>> tradesEquityAsset =
                    from trade in lstTradesResults
                    where (null != trade) && (trade.Item3 == Cst.UnderlyingAsset.EquityAsset)
                    select new Pair<int, string>(trade.Item1, trade.Item2);

                    if (0 < tradesEquityAsset.Count())
                        New_EventsGenAPI.CreateEventsAsync(ProcessBase, tradesEquityAsset, ProductTools.GroupProductEnum.Security).Wait();

                    IEnumerable<Pair<int, string>> tradesFutures =
                    from trade in lstTradesResults
                    where (null != trade) && ((trade.Item3 == Cst.UnderlyingAsset.Future) || (trade.Item3 == Cst.UnderlyingAsset.ExchangeTradedContract))
                    select new Pair<int, string>(trade.Item1, trade.Item2);

                    if (0 < tradesFutures.Count())
                        New_EventsGenAPI.CreateEventsAsync(ProcessBase, tradesFutures, ProductTools.GroupProductEnum.ExchangeTradedDerivative).Wait();
                }
                #endregion Génération des événements
                
            }
            return ret;

        }
        #endregion UnderlyerDeliveryGenAsync
        #region UnderlyerDeliveryGenByTradeAsync
        /// <summary>
        /// Mise en file d'attente de la tâche de dénouement d'un trade
        /// </summary>
        /// <param name="pMaturityOffsettingOptionData">Données utiles au traitement (clé de position, actif, cours, etc.)</param>
        /// <param name="pTrade">Données propres au trade à traiter</param>
        /// <param name="pCt">Token de notification d'annulation</param>
        /// <returns></returns>
        private async Task<Tuple<int, string, Cst.UnderlyingAsset>> UnderlyerDeliveryGenByTradeAsync(DataRow pTrade, IPosRequest pPosRequestGroupLevel, CancellationToken pCt)
        {
            Tuple<int, string, Cst.UnderlyingAsset> result = null;
            string key = String.Format("(Trade: {0} )", pTrade["IDT"].ToString());
            string wait = "START UnderlyerDeliveryGenByTradeAsync Wait   : {0} " + key;
            string release = "STOP  UnderlyerDeliveryGenByTradeAsync Release: {0} " + key;

            bool isSemaphoreSpecified = (null != ProcessBase.SemaphoreAsync);

            await Task.Run(() =>
            {
                try
                {
                    if (isSemaphoreSpecified)
                    {
                        ProcessBase.SemaphoreAsync.Wait();
                        AppInstance.TraceManager.TraceVerbose(this, String.Format(wait, ProcessBase.SemaphoreAsync.CurrentCount));
                    }

                    result = UnderlyerDeliveryGenByTrade(pTrade, pPosRequestGroupLevel);
                }
                catch (Exception) { throw; }
                finally
                {
                    if (isSemaphoreSpecified)
                    {
                        ProcessBase.SemaphoreAsync.Release();
                        AppInstance.TraceManager.TraceVerbose(this, String.Format(release, ProcessBase.SemaphoreAsync.CurrentCount));
                    }
                }
            } , pCt);

            return result;
        }
        #endregion UnderlyerDeliveryGenByTradeAsync
        #region UnderlyerDeliveryGenByTrade
        // EG 20190114 Add detail to ProcessLog Refactoring
        // EG 20190926 Refactoring Cst.PosRequestTypeEnum
        private Tuple<int, string, Cst.UnderlyingAsset> UnderlyerDeliveryGenByTrade(DataRow pTrade, IPosRequest pPosRequestGroupLevel)
        {
            Tuple<int, string, Cst.UnderlyingAsset> result = null;
            IDbTransaction dbTransaction = null;
            bool isException = false;
            Cst.ErrLevel ret;
            try
            {
                dbTransaction = DataHelper.BeginTran(CS);
                IPosRequest _posRequest = PosKeepingTools.GetPosRequest(CS, dbTransaction, m_Product, Convert.ToInt32(pTrade["IDPR"]));
                _posRequest.IdAUpdSpecified = true;
                _posRequest.IdAUpd = m_PKGenProcess.Session.IdA;

                TradeUnderlyerDeliveryData tradeData = new TradeUnderlyerDeliveryData(this, _posRequest, pPosRequestGroupLevel.IdPR);

                ret = tradeData.Initialize(dbTransaction, _posRequest.IdT, Cst.PosRequestAssetQuoteEnum.UnderlyerAsset, false);
                if (Cst.ErrLevel.SUCCESS == ret)
                {
                    if (RequestCanBeExecuted(Cst.PosRequestTypeEnum.RequestTypeOption, _posRequest.IdT))
                        ret = tradeData.CreateUnderlyingTrade(dbTransaction);
                }


                if (Cst.ErrLevel.SUCCESS == ret)
                {
                    string tradeIdentifier = TradeRDBMSTools.GetTradeIdentifier(CS, dbTransaction, _posRequest.IdT);
                    if ((null != _posRequest) && (0 < _posRequest.IdPR))
                    {
                        _posRequest.DtIns = pPosRequestGroupLevel.DtIns; // Pour TRI (UNLDLVR sous EOD_UNLDLVR)
                        UpdatePosRequest(dbTransaction, ret, _posRequest, pPosRequestGroupLevel.IdPR);
                    }
                    DataHelper.CommitTran(dbTransaction);
                    result = new Tuple<int, string, Cst.UnderlyingAsset>(_posRequest.IdT, tradeIdentifier,
                        (_posRequest.DetailBase as IPosRequestDetOption).Underlyer.AssetCategory);
                }
                else
                {
                    isException = true;
                    ret = Cst.ErrLevel.DATAREJECTED;
                }
            }
            catch (Exception ex)
            {
                isException = true;
                ret = Cst.ErrLevel.FAILURE;
                // FI 20200623 [XXXXX] SetErrorWarning
                m_PKGenProcess.ProcessState.SetErrorWarning(ProcessStateTools.StatusErrorEnum);

                // FI 20200623 [XXXXX] AddCriticalException
                m_PKGenProcess.ProcessState.AddCriticalException(ex);

                
                Logger.Log(new LoggerData(SpheresExceptionParser.GetSpheresException(null, ex)));
                Logger.Log(new LoggerData(LogLevelEnum.Error, new SysMsgCode(SysCodeEnum.SYS, 5101), 0,
                    new LogParam(GetPosRequestLogValue(m_PosRequest.RequestType)),
                    new LogParam(DtFunc.DateTimeToStringDateISO(m_PosRequest.DtBusiness))));
            }
            finally
            {
                if (null != dbTransaction)
                {
                    if (isException && (null != dbTransaction.Connection))
                        DataHelper.RollbackTran(dbTransaction);
                    dbTransaction.Dispose();
                }

                //if ((null != _posRequest) && (0 < _posRequest.idPR))
                //{
                //    _posRequest.dtIns = pPosRequestGroupLevel.dtIns; // Pour TRI (UNLDLVR sous EOD_UNLDLVR)
                //    UpdatePosRequest(ret, _posRequest, pPosRequestGroupLevel.idPR);
                //}
            }
            return result;
        }
        #endregion UnderlyerDeliveryGenByTrade

    }
    #endregion PosKeepingGen_ETD (ASYNCHRONE)
    /// <summary>
    /// Constructeur pour Correction de position
    /// </summary>
    public class TradeCancellationData : TradeData
    {
        #region Constructors
        public TradeCancellationData(PosKeepingGenProcessBase pProcess, IPosRequest pPosRequest, IPosKeepingData pPosKeepingData,
            (EFS_TradeLibrary pTradeLibrary, DataTable Position, DataTable PosActionDet) pData) 
            : base(pProcess, pPosRequest, pPosKeepingData, pData)
        {
        }
        #endregion Constructors
    }

    /// <summary>
    /// Constructeur pour Transfert de postion
    /// </summary>
    public class TradeTransferData : TradeData
    {
        #region Constructors
        public TradeTransferData(PosKeepingGenProcessBase pProcess, IPosRequest pPosRequest, IPosKeepingData pPosKeepingData,
            (EFS_TradeLibrary pTradeLibrary, DataTable Position, DataTable PosActionDet) pData) : 
            base(pProcess, pPosRequest, pPosKeepingData, pData)
        {
            IPosRequestDetTransfer detail = posRequest.DetailBase as IPosRequestDetTransfer;
            IPayment[] _paymentFees = null;
            if (detail.IsReversalFeesSpecified)
            {
                SQL_TradeCommon sqlTrade = new SQL_TradeCommon(m_Cs, posRequest.IdT);
                SpheresIdentification identification = new SpheresIdentification(sqlTrade.Identifier, sqlTrade.DisplayName, sqlTrade.Description, sqlTrade.ExtlLink)
                {
                    OTCmlId = sqlTrade.Id
                };

                TradePositionTransfer positionTransfer = new TradePositionTransfer(identification, detail.InitialQty, posRequest.Qty, posRequest.DtBusiness, null);
                positionTransfer.isFeeRestitution.BoolValue = detail.IsReversalFees;
                positionTransfer.CalcFeeRestitution(m_Cs);

                detail.SetPaymentFees(positionTransfer.otherPartyPayment);

                _paymentFees = detail.PaymentFees;
            }
            else if (false == detail.IdTReplaceSpecified)
                detail.PaymentFees = null;
        }
        #endregion Constructors
    }
}
