#region using directives
using System;
using System.Data;
using System.Collections;
using System.Collections.Generic;
using System.Text;

using EFS.ACommon;
using EFS.Common;

using EFS.Common.MQueue;
using EFS.GUI.Interface;

using EfsML.Business;
using EfsML.Notification;
using EfsML.Settlement;
using EfsML.Settlement.Message;
using EfsML.Repository;

using FixML.Enum;
using FixML.v50SP1.Enum;
using FixML.Interface;

using FpML.Enum;
using FpML.Interface;
using EfsML.Enum;
using EfsML.v30.PosRequest;
#endregion using directives


namespace EfsML.Interface
{
    #region IAbstractTransaction
    public interface IAbstractTransaction
    {
        #region Accessors
        IReference BuyerPartyReference { set; get; }
        IReference SellerPartyReference { set; get; }
        #endregion Accessors
    }
    #endregion IAbstractTransaction
    #region IAbstractUnitTransaction
    public interface IAbstractUnitTransaction : IAbstractTransaction
    {
        #region Accessors
        EFS_Decimal NumberOfUnits { set; get; }
        IMoney UnitPrice { set; get; }
        #endregion Accessors
    }
    #endregion IAbstractUnitTransaction
    #region IAccountNumber
    public interface IAccountNumber
    {
        #region Accessors
        ISpheresIdSchemeId Correspondant { set; get; }
        ICurrency Currency { set; get; }
        EFS_String NostroAccountNumber { set; get; }
        EFS_String AccountNumber { set; get; }
        EFS_String AccountName { set; get; }
        EFS_String JournalCode { set; get; }
        #endregion Accessors
    }
    #endregion IAccountNumber
    #region IAccruedInterestCalculationRules
    public interface IAccruedInterestCalculationRules
    {
        #region Accessors
        bool CalculationMethodSpecified { set; get; }
        AccruedInterestCalculationMethodEnum CalculationMethod { set; get; }
        bool RoundingSpecified { set; get; }
        IRounding Rounding { set; get; }
        bool ProrataDayCountFractionSpecified { set; get; }
        DayCountFractionEnum ProrataDayCountFraction { set; get; }
        #endregion Accessors
    }
    #endregion IAccruedInterestCalculationRules
    #region IActorId
    public interface IActorId : IScheme, ISpheresId
    {
        #region Accessors
        bool ActorNameSpecified { set; get; }
        string ActorName { set; get; }
        #endregion Accessors
    }
    #endregion IActorId
    #region IAdditionalInvoice
    public interface IAdditionalInvoice : IInvoiceSupplement
    {
    }
    #endregion IAdditionalInvoice
    #region IAllocatedInvoice
    // EG 20091125 Add allocatedEnterAmount 
    public interface IAllocatedInvoice : IInitialNetInvoiceAmounts
    {
        #region Accessors
        IAdjustedDate InvoiceDate { set; get; }
        INetInvoiceAmounts AllocatedAmounts { set; get; }
        INetInvoiceAmounts UnAllocatedAmounts { set; get; }
        bool FxGainOrLossAmountSpecified { set; get; }
        IMoney FxGainOrLossAmount { set; get; }
        IMoney AllocatedEnterAmount { set; get; }
        string Id { set; get; }
        #endregion Accessors
    }
    #endregion IAllocatedInvoice
    #region IAssetFxRateId
    public interface IAssetFxRateId : ISpheresId
    {
        #region Accessors
        string Value { set; get; }
        #endregion Accessors
    }
    #endregion IAssetFxRateId
    #region IAssetOrNothing
    public interface IAssetOrNothing
    {
        #region Accessors
        bool CurrencyReferenceSpecified { get; }
        string CurrencyReference { get; }
        bool GapSpecified { get; }
        decimal Gap { get; }
        #endregion Accessors
    }
    #endregion IAssetOrNothing
    #region IAttribution
    public interface IAttribution
    {
        #region Accessors
        IScheme Type { set; get; }
        bool SettlementAmountSpecified { set; get; }
        EFS_Decimal SettlementAmount { set; get; }
        bool BaseAmountSpecified { set; get; }
        EFS_Decimal BaseAmount { set; get; }
        bool UnderlyingAmountSpecified { set; get; }
        EFS_Decimal UnderlyingAmount { set; get; }
        #endregion Accessors
    }
    #endregion IAttribution
    #region IAttributions
    public interface IAttributions
    {
        #region Accessors
        ICurrency SettlementCurrency { set; get; }
        ICurrency BaseCurrency { set; get; }
        bool UnderlyingCurrencySpecified { set; get; }
        ICurrency UnderlyingCurrency { set; get; }
        bool AttributionSpecified { set; get; }
        IAttribution[] Attribution { set; get; }
        #endregion Accessors
    }
    #endregion IAttributions
    #region IAvailableInvoice
    public interface IAvailableInvoice : IInitialNetInvoiceAmounts
    {
        #region Accessors
        IAdjustedDate InvoiceDate { set; get; }
        INetInvoiceAmounts AvailableAmounts { set; get; }
        bool AllocatedAccountingAmountSpecified { set; get; }
        IMoney AllocatedAccountingAmount { set; get; }
        string Id { set; get; }
        #endregion Accessors
    }
    #endregion IAllocatedInvoice
    #region IAverageStrikeOption
    public interface IAverageStrikeOption
    {
        #region Accessors
        SettlementTypeEnum SettlementType { set; get; }
        #endregion Accessors
    }
    #endregion IAverageStrikeOption

    #region IBookId
    public interface IBookId : IScheme, ISpheresId
    {
        #region Accessors
        bool BookNameSpecified { set; get; }
        string BookName { set; get; }
        #endregion Accessors
    }
    #endregion IBookId
    #region IBondCollateral
    public interface IBondCollateral
    {
        #region Accessors
        IMoney NominalAmount { set; get; }
        bool CleanPriceSpecified { set; get; }
        EFS_Decimal CleanPrice { set; get; }
        bool AccrualsSpecified { set; get; }
        EFS_Decimal Accruals { set; get; }
        bool DirtyPriceSpecified { set; get; }
        EFS_Decimal DirtyPrice { set; get; }
        bool RelativePriceSpecified { set; get; }
        IRelativePrice RelativePrice { set; get; }
        bool YieldToMaturitySpecified { set; get; }
        EFS_Decimal YieldToMaturity { set; get; }
        bool InflationFactorSpecified { set; get; }
        EFS_Decimal InflationFactor { set; get; }
        bool InterestStartDateSpecified { set; get; }
        IAdjustableOrRelativeDate InterestStartDate { set; get; }
        bool PoolSpecified { set; get; }
        IAssetPool Pool { set; get; }
        #endregion Accessors
    }
    #endregion IBondCollateral
    #region IBondPrice
    public interface IBondPrice
    {
        #region Accessors
        EFS_Decimal CleanPrice { set; get; }
        EFS_Boolean CleanOfAccruedInterest { set; get; }
        bool AccrualsSpecified { set; get; }
        EFS_Decimal Accruals { set; get; }
        bool DirtyPriceSpecified { set; get; }
        EFS_Decimal DirtyPrice { set; get; }
        #endregion Accessors
    }
    #endregion IBondPrice
    #region IBondTransaction
    public interface IBondTransaction : IAbstractUnitTransaction
    {
        #region Accessors
        IBondPrice Price { set; get; }
        IBond Bond { set; get; }
        #endregion Accessors
    }
    #endregion IBondTransaction
    #region IBracket
    public interface IBracket
    {
        #region Accessors
        bool LowValueSpecified { set; get; }
        EFS_Decimal LowValue { set; get; }
        bool HighValueSpecified { set; get; }
        EFS_Decimal HighValue { set; get; }
        #endregion Accessors
        #region Methods
        bool IsBracketMatch(decimal pAmount);
        #endregion Methods
    }
    #endregion IBracket
    #region IBuyAndSellBack
    public interface IBuyAndSellBack : ISaleAndRepurchaseAgreement
    {
        #region Accessors
        #endregion Accessors
    }
    #endregion IBuyAndSellBack

    #region ICappedCallOrFlooredPut
    public interface ICappedCallOrFlooredPut
    {
        #region Accessors
        bool TypeFxCapBarrierSpecified { get; }
        bool TypeFxFloorBarrierSpecified { get; }
        PayoutEnum PayoutStyle { get; }
        #endregion Accessors
    }
    #endregion ICappedCallOrFlooredPut

    #region ICashBalanceInterest
    public interface ICashBalanceInterest
    {
        #region Accessors
        /// <summary>
        /// Type de montant sur lequel portent les intérêts
        /// </summary>
        Nullable<InterestAmountTypeEnum> InterestAmountType { set; get; }
        /// <summary>
        /// L'Entité
        /// </summary>
        IReference EntityPartyReference { get; set; }
        /// <summary>
        /// Eléments de calcul des intérêts
        /// </summary>
        IInterestRateStream[] Stream { get; }
        #endregion Accessors
    }
    #endregion ICashBalanceInterest

    #region ICashPayment
    // EG 20180608 New
    public interface ICashPayment
    {
        #region Accessors
        /// <summary>
        /// L'Entité
        /// </summary>
        bool EntityPartyReferenceSpecified { set; get; }
        IReference EntityPartyReference { get; set; }
        #endregion Accessors
    }
    #endregion ICashPayment


    #region ICashRepricingEvent
    public interface ICashRepricingEvent : IMidLifeEvent
    {
        #region Accessors
        bool CollateralSpecified { set; get; }
        ICollateralValuation Collateral { set; get; }
        EFS_Boolean CombinedInterestPayout { set; get; }
        bool TransferSpecified { set; get; }
        ITransfer Transfer { set; get; }
        #endregion Accessors
    }
    #endregion ICashRepricingEvent
    #region ICashStream
    public interface ICashStream : IInterestRateStream
    {
    }
    #endregion ICashStream
    #region ICashTransfer
    public interface ICashTransfer : IAtomicSettlementTransfer
    {
        #region Accessors
        IMoney TransferAmount { set; get; }
        IReference PayerPartyReference { set; get; }
        IReference ReceiverPartyReference { set; get; }
        bool AttributionsSpecified { set; get; }
        IAttributions[] Attributions { set; get; }
        #endregion Accessors
    }
    #endregion ICashTransfer


    #region ICFIIdentifier
    public interface ICFIIdentifier
    {
        #region Accessors
        string Value { set; get; }
        #endregion Accessors
    }
    #endregion ICFIIdentifier
    #region IClassification
    public interface IClassification
    {
        #region Accessors
        bool DebtSecurityClassSpecified { set; get; }
        IScheme DebtSecurityClass { set; get; }
        bool CfiCodeSpecified { set; get; }
        ICFIIdentifier CfiCode { set; get; }
        bool ProductTypeCodeSpecified { set; get; }
        ProductTypeCodeEnum ProductTypeCode { set; get; }
        bool FinancialInstrumentProductTypeCodeSpecified { set; get; }
        FinancialInstrumentProductTypeCodeEnum FinancialInstrumentProductTypeCode { set; get; }
        bool SymbolSpecified { set; get; }
        EFS_String Symbol { set; get; }
        bool SymbolSfxSpecified { set; get; }
        EFS_String SymbolSfx { set; get; }
        #endregion Accessors
    }
    #endregion IClassification
    #region ICommercialPaper
    public interface ICommercialPaper
    {
        #region Accessors
        bool ProgramSpecified { set; get; }
        CPProgramEnum Program { set; get; }
        bool RegTypeSpecified { set; get; }
        EFS_Integer RegType { set; get; }
        #endregion Accessors
    }
    #endregion ICommercialPaper
    #region ICollateralSubstitutionEvent
    public interface ICollateralSubstitutionEvent : IMidLifeEvent
    {
        #region Accessors
        ICollateralValuation PreviousCollateral { set; get; }
        ICollateralValuation NewCollateral { set; get; }
        bool SettlementTransferSpecified { set; get; }
        ISettlementTransfer SettlementTransfer { set; get; }
        #endregion Accessors
    }
    #endregion ICollateralSubstitutionEvent
    #region ICollateralValuation
    public interface ICollateralValuation
    {
        #region Accessors
        bool BondCollateralSpecified { set; get; }
        IBondCollateral BondCollateral { set; get; }
        bool UnitContractSpecified { set; get; }
        IUnitContract UnitContract { set; get; }
        IReference AssetReference { set; get; }
        #endregion Accessors
    }
    #endregion ICappedCallOrFlooredPut

    #region IMarginRatio
    public interface IMarginRatio : IActualPrice, ISpreadSchedule
    {
        #region Accessors
        bool SpreadScheduleSpecified { set; get; }
        ISpreadSchedule SpreadSchedule { set; get; }
        bool CrossMarginRatioSpecified { set; get; }
        IActualPrice CrossMarginRatio { set; get; }
        void CreateSpreadMarginRatio(Nullable<decimal> pValue);
        #endregion Accessors
    }
    #endregion IMarginRatio

    /// <summary>
    /// 
    /// </summary>
    /// FI 20140731 [20179] add tradeCciMatch
    /// FI 20150427 [20987] Modify
    /// FI 20150623 [21149] Modify
    /// FI 20160530 [21885] Modify
    /// FI 20170217 [22862] Modify
    public interface INotificationDocument : IEfsDocument
    {
        #region properties
        /// <summary>
        /// Représente l'en-tête du message
        /// </summary>
        INotificationHeader Header { set; get; }
        /// <summary>
        /// Représente le dataDocument inclus dans le message 
        /// <para>Ce document peut inclure plusieurs trades</para>
        /// </summary>
        IDataDocument DataDocument { set; get; }
        /// <summary>
        /// 
        /// </summary>
        bool TradeSortingSpecified { set; get; }
        /// <summary>
        /// Représente le tri des trades
        /// </summary>
        ReportTradeSort TradeSorting { set; get; }
        /// <summary>
        /// 
        /// </summary>
        bool NotepadSpecified { set; get; }
        /// <summary>
        /// Représente les notepad des trades du dataDocument
        /// </summary>
        CDATA[] Notepad { set; get; }
        /// <summary>
        /// 
        /// </summary>
        bool DetailsSpecified { set; get; }
        /// <summary>
        /// Représente le détail du dataDocument lorsque la confirmation est une confirmation de facture
        /// </summary>
        IConfirmationMessageDetails Details { set; get; }
        /// <summary>
        /// 
        /// </summary>
        bool EventsSpecified { set; get; }
        /// <summary>
        /// Représente des évènements des trades du dataDocument (présentés de ligne)
        /// </summary>
        Events Events { set; get; }
        /// <summary>
        /// 
        /// </summary>
        bool HierarchicalEventsSpecified { set; get; }
        /// <summary>
        /// Représente des évènements des trades du dataDocument (présentés de manière hiérarchique)
        /// </summary>
        HierarchicalEvents HierarchicalEvents { set; get; }

        /// <summary>
        /// 
        /// </summary>
        bool PosActionsSpecified { set; get; }
        /// <summary>
        /// Représente les actions sur positions - business date
        /// </summary>
        /// FI 20150427 [20987] Modify (posActions de type List)
        List<PosActions> PosActions { set; get; }

        /// <summary>
        /// 
        /// </summary>
        bool StlPosActionsSpecified { set; get; }
        /// <summary>
        /// Représente les actions sur positions - settlement date
        /// </summary>
        /// FI 20150623 [21149] Add
        List<PosActions> StlPosActions { set; get; }

        /// <summary>
        /// 
        /// </summary>
        bool PosSyntheticsSpecified { set; get; }
        /// <summary>
        /// Représente les positions synthétiques
        /// </summary>
        /// FI 20150427 [20987] Modify (posSynthetics de type List de PosSynthetics)
        List<PosSynthetics> PosSynthetics { set; get; }


        /// <summary>
        /// 
        /// </summary>
        bool StlPosSyntheticsSpecified { set; get; }
        /// <summary>
        /// Représente les positions synthétiques
        /// </summary>
        /// FI 20150809 [XXXXX] Add
        List<PosSynthetics> StlPosSynthetics { set; get; }


        /// <summary>
        /// 
        /// </summary>
        bool TradesSpecified { set; get; }
        /// <summary>
        /// Représente les trades négociés le jour de l'édition 
        /// </summary>
        /// FI 20150427 [20987] Modify (trades de type List de TradesOfDay)
        List<TradesReport> Trades { set; get; }


        /// <summary>
        /// 
        /// </summary>
        bool UnsettledTradesSpecified { set; get; }
        /// <summary>
        /// Représente les trades non encore réglés le jour de l'édition 
        /// <para>présent lorsqu'il existe des trades où la date de règlement est différente de la date business</para>
        /// </summary>
        /// FI 20150623 [21149] Add
        List<TradesReport> UnsettledTrades { set; get; }

        /// <summary>
        /// 
        /// </summary>
        bool SettledTradesSpecified { set; get; }
        /// <summary>
        /// Représente les trades réglés le jour de l'édition 
        /// <para>présent lorsqu'il existe des trades où la date de règlement est différente de la date business</para>
        /// </summary>
        /// FI 20150623 [21149] Add
        List<TradesReport> SettledTrades { set; get; }


        /// <summary>
        /// 
        /// </summary>
        bool PosTradesSpecified { set; get; }
        /// <summary>
        /// Représente les trades de la positions détaillée - business date
        /// </summary>
        /// FI 20130627 [18745] add
        /// FI 20150427 [20987] Modify (posTrades de type List)
        List<PosTrades> PosTrades { set; get; }

        /// <summary>
        /// 
        /// </summary>
        bool StlPosTradesSpecified { set; get; }
        /// <summary>
        /// Représente les trades de la positions détaillée  - settlement date
        /// </summary>
        /// FI 20150623 [21149] add
        List<PosTrades> StlPosTrades { set; get; }

        /// <summary>
        /// 
        /// </summary>
        bool DlvTradesSpecified { set; get; }
        /// <summary>
        /// 
        /// </summary>
        /// FI 20170217 [22862] Add
        List<DlvTrades> DlvTrades { set; get; }

        /// <summary>
        /// 
        /// </summary>
        bool CashPaymentsSpecified { set; get; }
        /// <summary>
        /// 
        /// </summary>
        /// FI 20150128 [20275] Add
        /// FI 20150427 [20987] cashPayments devient list de CashPayments
        List<CashPayments> CashPayments { set; get; }

        /// <summary>
        /// 
        /// </summary>
        bool CommonDataSpecified { set; get; }
        /// <summary>
        /// Représente des données communes à toutes les éditions:
        /// <para>- Données complémentaires sur les Trades (Format d'affichage des prix)</para>
        /// <para>- Données complémentaires sur les Assets (Format d'affichage des prix)</para>
        /// </summary>    
        /// RD 20130722 [18745] add
        CommonData CommonData { set; get; }

        /// <summary>
        /// 
        /// </summary>
        bool TradeCciMatchSpecified { set; get; }

        /// <summary>
        /// Liste des données soumises à matching
        /// </summary>
        List<TradeCciMatch> TradeCciMatch { set; get; }

        /// <summary>
        /// 
        /// </summary>
        bool CollateralsSpecified { set; get; }
        /// <summary>
        ///  Liste des dépôts de garantie (existe uniquement sur les éditions de synthèse)
        /// </summary>
        /// FI 20160530 [21885] Add
        List<CollateralsReport> Collaterals { set; get; }

        /// <summary>
        /// 
        /// </summary>
        bool UnderlyingStocksSpecified { set; get; }
        /// <summary>
        /// Liste des positions actions déposées et utilisées pour la réduction des postions ETD Short futures et Short call à une date donné
        /// </summary>
        /// FI 20160530 [21885] Add
        List<UnderlyingStocksReport> UnderlyingStocks { set; get; }
        

        #endregion properties

        #region Methods
        INotificationHeader CreateNotificationHeader();

        IRoutingCreateElement CreateRoutingCreateElement();

        INotificationRoutingCopyTo CreateNotificationRoutingCopyTo();

        INotificationRouting CreateNotificationRouting();

        IReference CreatePartyReference();

        IConfirmationTradeDetail CreateTradeDetail();

        IConfirmationMessageDetails CreateConfirmationMessageDetails();

        IEfsDocument CreateEfsDocument();

        #endregion Methods
    }

    /// <summary>
    /// 
    /// </summary>
    /// FI 20120829 [18048] add valueDate2
    public interface INotificationHeader : ISpheresId
    {
        /// <summary>
        /// identification du message
        /// </summary>
        NotificationId[] ConfirmationMessageId { set; get; }

        /// <summary>
        /// identification du système de communcation
        /// </summary>
        NotificationConfirmationSystemIds NotificationConfirmationSystemIds { set; get; }

        /// <summary>
        /// Date de création du message
        /// </summary>
        /// FI 20140808 [20549] creationTimestamp est de type EFS_DateTimeUTC
        EFS_DateTimeUTC CreationTimestamp { set; get; }



        /// <summary>
        /// 
        /// </summary>
        /// FI 20160624 [22286] Add
        bool PreviousCreationTimestampSpecified { set; get; }

        /// <summary>
        /// Précédentes dates d'un message déjà généré 
        /// </summary>
        /// FI 20160624 [22286] Add
        PreviousDateTimeUTC[] PreviousCreationTimestamp { set; get; }

        /// <summary>
        /// date valeur du message
        /// <para>Date From si le message est un extrait de compte</para>
        /// </summary>
        EFS_Date ValueDate { set; get; }

        /// <summary>
        /// 
        /// </summary>
        bool ValueDate2Specified { set; get; }

        /// <summary>
        /// <para>Date To si le message est un extrait de compte</para>
        /// </summary>
        EFS_Date ValueDate2 { set; get; }

        /// <summary>
        /// Représente l'acteur émeteur
        /// </summary>
        INotificationRouting SendBy { set; get; }

        /// <summary>
        /// Représente les destinataires 
        /// </summary>
        INotificationRouting[] SendTo { set; get; }

        /// <summary>
        /// 
        /// </summary>
        bool CopyToSpecified { set; get; }
        /// <summary>
        /// Représente les destinatiare en copie
        /// </summary>
        INotificationRoutingCopyTo[] CopyTo { set; get; }

        /// <summary>
        /// 
        /// </summary>
        bool SoftApplicationSpecified { set; get; }
        /// <summary>
        /// Représente la version de Spheres® à l'origine du message
        /// </summary>
        ISoftApplication SoftApplication { set; get; }

        bool ReportCurrencySpecified { set; get; }
        /// <summary>
        /// Représente la devise du message
        /// <para>Les contrevaleurs seront en cette devise </para>
        /// </summary>
        ICurrency ReportCurrency { set; get; }

        bool ReportFeeDisplaySpecified { set; get; }
        /// <summary>
        /// Pilote la présentation des frais
        /// </summary>
        string ReportFeeDisplay { set; get; }

        bool ReportSettingsSpecified { set; get; }
        /// <summary>
        /// Représente les paramètres concernant l'état
        /// </summary>
        ReportSettings ReportSettings { set; get; }
    }

    /// <summary>
    /// Représente le détail d'un message de facturation
    /// </summary>
    public interface IConfirmationMessageDetails
    {
        #region Members
        /// <summary>
        /// Représente les trades impliqués dans la facture
        /// </summary>
        IConfirmationTradeDetail[] TradeDetail { set; get; }
        #endregion Members
    }

    /// <summary>
    /// Représente un trade
    /// </summary>
    public interface IConfirmationTradeDetail
    {
        #region Members
        string OtcmlId { set; get; }
        bool TradeHeaderSpecified { set; get; }
        ITradeHeader TradeHeader { set; get; }
        bool ProductSpecified { set; get; }
        IConfirmationTradeProduct Product { set; get; }
        bool AdditionalPaymentSpecified { set; get; }
        IPayment[] AdditionalPayment { set; get; }
        bool OtherPartyPaymentSpecified { set; get; }
        IPayment[] OtherPartyPayment { set; get; }
        bool ExtendsSpecified { set; get; }
        ITradeExtends Extends { set; get; }
        bool ExternalLinkSpecified { set; get; }
        string ExternalLink { set; get; }
        IConfirmationTradeProduct CreateProduct();
        #endregion Members
    }

    /// <summary>
    /// Représente un product
    /// </summary>
    public interface IConfirmationTradeProduct
    {
        #region Members
        bool ProductSpecified { set; get; }
        IProductBase Product { set; get; }
        bool SubProductSpecified { set; get; }
        IConfirmationTradeProduct[] SubProduct { set; get; }
        bool StreamSpecified { set; get; }
        IInterestRateStream[] Stream { set; get; }
        bool LegSpecified { set; get; }
        IFxLeg[] Leg { set; get; }
        bool ExchangeTradedDerivativeSpecified { set; get; }
        IExchangeTradedDerivative ExchangeTradedDerivative { set; get; }
        #endregion Members
    }

    /// <summary>
    /// 
    /// </summary>
    public interface INotificationRouting : IRoutingPartyReference
    {
        /// <summary>
        /// 
        /// </summary>
        bool PartyReferenceSpecified { set; get; }
        /// <summary>
        /// 
        /// </summary>
        IReference PartyReference { set; get; }
    }

    /// <summary>
    /// 
    /// </summary>
    public interface INotificationRoutingCopyTo : INotificationRouting
    {
        bool IsBcc { set; get; }
    }



    #region ICouponEvent
    public interface ICouponEvent : IMidLifeEvent
    {
        #region Accessors
        EFS_Decimal CouponAmount { set; get; }
        EFS_Decimal ReinvestmentRate { set; get; }
        IReference AssetReference { set; get; }
        bool TransferSpecified { set; get; }
        EFS_Decimal Transfer { set; get; }
        #endregion Accessor
    }
    #endregion ICouponEvent
    #region ICreditNote
    public interface ICreditNote : IInvoiceSupplement
    {
    }
    #endregion ICreditNote
    #region ICss
    public interface ICss
    {
        #region Accessors
        string Value { set; get; }
        string OtcmlId { set; get; }
        int OTCmlId { set; get; }
        #endregion Accessors
    }
    #endregion ICss
    #region ICssCriteria
    public interface ICssCriteria
    {
        #region Accessors
        bool CssSpecified { set; get; }
        ICss Css { set; get; }
        bool CssInfoSpecified { set; get; }
        ICssInfo CssInfo { set; get; }
        #endregion Accessors
    }
    #endregion ICssCriteria
    #region ICssInfo
    public interface ICssInfo
    {
        #region Accessors
        bool CountrySpecified { get; }
        IScheme Country { get; }
        bool TypeSpecified { get; }
        IScheme Type { get; }
        bool SettlementTypeSpecified { get; }
        IScheme SettlementType { get; }
        bool PaymentTypeSpecified { get; }
        IScheme PaymentType { get; }
        bool SystemTypeSpecified { get; }
        IScheme SystemType { get; }
        #endregion Accessors
    }
    #endregion ICssInfo
    #region ICurrencyRepository
    public interface ICurrencyRepository : ICommonRepository
    {
        #region Accessors
        bool SymbolSpecified { set; get; }
        string Symbol { set; get; }
        bool SymbolalignSpecified { set; get; }
        string Symbolalign { set; get; }
        bool ISO4217_num3Specified { set; get; }
        string ISO4217_num3 { set; get; }
        bool FactorSpecified { set; get; }
        int Factor { set; get; }
        bool RounddirSpecified { set; get; }
        RoundingDirectionEnum Rounddir { set; get; }
        bool RoundprecSpecified { set; get; }
        int Roundprec { set; get; }
        bool FxrateSpecified { set; get; }
        // RD 20131015 [19067] Extrait de compte / un taux de change par jour        
        IFxRateRepository[] Fxrate { set; get; }
        #endregion Accessors
    }
    #endregion ICurrencyRepository
    #region ICurrencyRepository
    // EG 20240216 [WI850][26600] Ajout Request Date pour édition des confirmation sur facture Migration MAREX
    public interface IFxRateRepository : IFxRate
    {
        #region Accessors
        DateTime RequestDate { set; get; }
        EFS_Date FixingDate { set; get; }
        #endregion Accessors
    }
    #endregion ICurrencyRepository
    #region ICustomerSettlementPayment
    public interface ICustomerSettlementPayment
    {
        #region Accessors
        IExchangeRate Rate { get; }
        bool AmountSpecified { set; get; }
        IMoney GetMoney();
        EFS_Decimal Amount { set; get; }
        string Currency { set; get; }
        #endregion Accessors
    }
    #endregion ICustomerSettlementPayment

    #region IDebtSecurity
    // EG 20190823 [FIXEDINCOME] debtSecurityType|prevCouponDate
    public interface IDebtSecurity
    {
        #region Accessors
        DebtSecurityTypeEnum DebtSecurityType { set; get; }
        bool PrevCouponDateSpecified { set; get; }
        EFS_Date PrevCouponDate { set; get; }
        ISecurity Security { set; get; }
        IDebtSecurityStream[] Stream { set; get; }
        #endregion Accessors
    }
    #endregion IDebtSecurity
    #region IDebtSecurityTransaction
    /// <summary>
    /// 
    /// </summary>
    /// FI 20170116 [21916] Modify
    // EG 20171031 [23509] Upd 
    /// EG 20190730 New Interface ITradeTypeReport
    // EG 20190823 [FIXEDINCOME] CalcNextInterestPeriodDates (for Perpetual debSecurity)
    public interface IDebtSecurityTransaction : ITradeTypeReport
    {
        #region Accessors
        IReference BuyerPartyReference { set; get; }
        IReference SellerPartyReference { set; get; }
        IReference IssuerPartyReference { get; }
        bool SecurityAssetSpecified { set; get; }
        ISecurityAsset SecurityAsset { set; get; }
        bool SecurityAssetReferenceSpecified { set; get; }
        IReference SecurityAssetReference { set; get; }
        IOrderQuantity Quantity { set; get; }
        IOrderPrice Price { set; get; }
        IPayment GrossAmount { set; get; }
        IDebtSecurity DebtSecurity { get; }
        EFS_DebtSecurityTransactionAmounts Efs_DebtSecurityTransactionAmounts { set; get; }
        EFS_DebtSecurityTransactionStream Efs_DebtSecurityTransactionStream { set; get; }
        int SecurityAssetOTCmlId { get; }
        EFS_EventDate MaxTerminationDate { get; }
        /// <summary>
        /// 
        /// </summary>
        /// FI 20170116 [21916] RptSide (R majuscule)
        IFixTrdCapRptSideGrp[] RptSide { set; get; }
        EFS_Asset Efs_Asset(string pCS);
        // EG 20150624 (21151] New
        // EG 20150920 [21374] Int (int32) to Long (Int64) 
        // EG 20170127 Qty Long To Decimal
        void InitPositionTransfer(decimal pQuantity);
        void SetStreams(string pCS, DataDocumentContainer pDataDocument, Cst.StatusBusiness pStatus);
        /// <summary>
        /// Sur un <see cref="IDebtSecurityTransaction"/> l'asset peut être spécifié via une référence. Il s'agit ici de rechercher cet asset dans <paramref name="dataDocument"/> lorsque nécessaire afin d'utiliser ses propriétés (exemple <seealso cref="SecurityAssetOTCmlId"/>).
        /// </summary>
        /// <param name="dataDocument"></param>
        /// FI 20230615 [XXXX] Add
        void ResolveSecurityAsset(DataDocumentContainer dataDocument);

        ICalculationPeriodDates CalcNextInterestPeriodDates(string pCS, DebtSecurityTransactionContainer pDebtSecurityTransactionContainer, DateTime pPreviousCouponDate);
        ISpheresIdScheme ExchangeId { set; get; }
        bool ExchangeIdSpecified { set; get; }

        #endregion Accessors
    }
    #endregion IDebtSecurityTransaction
    #region IDebtSecurityStream
    // EG 20190823 [FIXEDINCOME] CalcNextInterestPeriodDates|SetRecordAndExDates (for Perpetual debSecurity)
    public interface IDebtSecurityStream : IInterestRateStream
    {
        #region Accessors
        bool SecurityExchangesSpecified { set; get; }
        ISecurityExchanges SecurityExchanges { get; }
        bool IsInitialSecurityExchange { get; }
        bool IsIntermediateSecurityExchange { get; }
        bool IsFinalSecurityExchange { get; }
        IMoney GetFirstParValueAmount { get; }
        #endregion Accessors
        ICalculationPeriodDates CalcNextInterestPeriodDates(string pCS, DebtSecurityTransactionContainer pDebtSecurityTransactionContainer, DateTime pPreviousCouponDate);
        void SetRecordAndExDates(string pCS, DataDocumentContainer pDataDocument, DividendDateReferenceEnum pTypeDate, IRelativeDateOffset pRelativeDateOffset);
    }
    #endregion IDebtSecurityStream

    #region IDerivativeContractRepository
    /// <summary>
    /// 
    /// </summary>
    public interface IDerivativeContractRepository : ICommonRepository
    {
        #region Accessors
        bool AssetCategorySpecified { set; get; }
        string AssetCategory { set; get; }
        bool IdDC_UnlSpecified { set; get; }
        string IdDC_Unl { set; get; }
        bool IdAsset_UnlSpecified { set; get; }
        string IdAsset_Unl { set; get; }
        bool IdC_PriceSpecified { set; get; }
        string IdC_Price { get; set; }
        bool IdC_NominalSpecified { set; get; }
        string IdC_Nominal { get; set; }
        bool CategorySpecified { set; get; }
        string Category { get; set; }
        bool ExerciseStyleSpecified { set; get; }
        string ExerciseStyle { set; get; }
        bool ContractSymbolSpecified { set; get; }
        string ContractSymbol { set; get; }
        bool FutValuationMethodSpecified { set; get; }
        string FutValuationMethod { set; get; }
        bool SettltMethodSpecified { set; get; }
        string SettltMethod { set; get; }
        //PL 20181001 [24212]
        bool RICCodeSpecified { set; get; }
        string RICCode { set; get; }
        bool BBGCodeSpecified { set; get; }
        string BBGCode { set; get; }
        bool ContractMultiplierSpecified { set; get; }
        decimal ContractMultiplier { set; get; }
        bool FactorSpecified { set; get; }
        decimal Factor { set; get; }
        int InstrumentDen { set; get; }

        bool IdMSpecified { set; get; }
        /// <summary>
        /// Marché
        /// </summary>
        int IdM { set; get; }

        bool PriceFmtStyleSpecified { set; get; }
        /// <summary>
        /// Format d'affichage des prix pour les éditions
        /// </summary>
        /// FI 20150218 [20275] Add
        string PriceFmtStyle { set; get; }


        bool StrikeFmtStyleSpecified { set; get; }
        /// <summary>
        /// Format d'affichage du strike pour les éditions
        /// </summary>
        /// FI 20150218 [20275] Add
        string StrikeFmtStyle { set; get; }


        bool ExtlDescSpecified { set; get; }
        /// <summary>
        /// 
        /// </summary>
        /// FI 20220906 [XXXXX] Add
        string ExtlDesc { set; get; }


        bool AttribSpecified { set; get; }
        /// <summary>
        /// 
        /// </summary>
        /// FI 20220908 [XXXXX] Add 
        string Attrib { set; get; }


        bool ETDIdentifierFormatSpecified { set; get; }
        /// <summary>
        /// 
        /// </summary>
        /// FI 20220912 [XXXXX] Add
        string ETDIdentifierFormat { set; get; }
        #endregion Accessors
    }
    #endregion IDerivativeContractRepository




    #region IEFS_AdjustableDate
    public interface IEFS_AdjustableDate
    {
        #region Accessors
        bool AdjustableDateSpecified { get; }
        /*
        string adjustedDate { get;}
        string unadjustedDate { get;}
        */
        DateTime AdjustedDate { get; }
        DateTime UnadjustedDate { get; }
        #endregion Accessors
    }
    #endregion IEFS_AdjustableDate
    #region IEFS_ExchangeRate
    public interface IEFS_ExchangeRate
    {
        #region Accessors
        IExchangeRate ExchangeRate { get; }
        bool ReferenceCurrencySpecified { get; }
        string ReferenceCurrency { get; }
        bool NotionalAmountSpecified { get; }
        EFS_Decimal NotionalAmount { get; }
        #endregion Accessors
    }
    #endregion IEFS_ExchangeRate
    #region IEFS_IntervalPeriods
    public interface IEFS_IntervalPeriods
    {
        #region Accessors
        #endregion Accessors
    }
    #endregion IEFS_IntervalPeriods

    #region IETDRepository
    /// <summary>
    /// 
    /// </summary>
    // PM 20140516 [19970][19259] Ajout de idC et idCSpecified
    // FI 20140903 [20275] hérite de IAssetRepository 
    // FI 20140218 [20275] strikePrice est de type RepositoryPrice
    // FI 20150522 [20275] Modify
    public interface IAssetETDRepository : IAssetRepository
    {
        #region Accessors
        int IdDC { set; get; }

        bool AssetSymbolSpecified { set; get; }
        string AssetSymbol { set; get; }
        bool CFICodeSpecified { set; get; }
        string CFICode { get; set; }
        bool ISINCodeSpecified { set; get; }
        string ISINCode { set; get; }
        //PL 20181001 [24213]
        bool RICCodeSpecified { set; get; }
        string RICCode { set; get; }
        bool BBGCodeSpecified { set; get; }
        string BBGCode { set; get; }
        bool AiiSpecified { set; get; }
        string Aii { set; get; }

        bool ContractMultiplierSpecified { set; get; }
        decimal ContractMultiplier { set; get; }

        bool StrikePriceSpecified { set; get; }
        RepositoryPrice StrikePrice { set; get; }

        bool PutCallSpecified { set; get; }
        string PutCall { set; get; }

        bool FactorSpecified { set; get; }
        decimal Factor { set; get; }

        bool MaturityDateSpecified { set; get; }
        /// <summary>
        /// Echéance 
        /// </summary>
        DateTime MaturityDate { set; get; }

        bool ExpIndSpecified { set; get; }
        // FI 20150522 [20275]
        /// <summary>
        /// Indicateur de proximité d'échéance
        /// </summary>
        int ExpInd { set; get; }

        bool MaturityMonthYearSpecified { set; get; }
        string MaturityMonthYear { set; get; }
        
        bool ConvertedPricesSpecified { set; get; }

        // 20120820 MF - Ticket 18073
        /// <summary>
        /// Converted values collection. The values conversion is performed according with a specific base and format style specified
        /// on the derivative contract referential related to the current asset.
        /// </summary>
        IConvertedPrices ConvertedPrices { set; get; }
        #endregion Accessors
    }

    // 20120820 MF - Ticket 18073
    /// <summary>
    /// Interface related to a converted values collection. The values conversion is performed according with a specific base and format style
    /// </summary>
    public interface IConvertedPrices
    {
        /// <summary>
        /// get/set when the strike price has been specified
        /// </summary>
        bool ConvertedStrikePriceSpecified { set; get; }

        /// <summary>
        /// Get/Set the value of the converted strike price. 
        /// The Strike has been converted from the default numerical base to the display numerical
        /// base of the derivative contract related to the current asset element. 
        /// Moreover a derivative contract specific style has been applied.
        /// </summary>
        string ConvertedStrikePrice { get; set; }

        /// <summary>
        /// get/set when the average long price has been specified
        /// </summary>
        bool ConvertedLongAveragePriceSpecified { set; get; }

        /// <summary>
        /// Get/Set the value of the converted weighted average long price of a trade subset (netted by asset). 
        /// The long average price has been converted from the default numerical base to the display numerical
        /// base of the derivative contract related to the current asset element. 
        /// Moreover a derivative contract specific style has been applied.
        /// </summary>
        string ConvertedLongAveragePrice { get; set; }

        /// <summary>
        /// get/set when the average short price has been specified
        /// </summary>
        bool ConvertedShortAveragePriceSpecified { set; get; }

        /// <summary>
        /// Get/Set the value of the converted weighted average short price of a trade subset (netted by asset). 
        /// The long average price has been converted from the default numerical base to the display numerical
        /// base of the derivative contract related to the current asset element. 
        /// Moreover a derivative contract specific style has been applied.
        /// </summary>
        string ConvertedShortAveragePrice { get; set; }

        /// <summary>
        /// Get/Set the value of the converted weighted average price of a synthetic position (netting by asset).
        /// The value has been converted from the numerical base 100 to the display numerical
        /// base of the derivative contract related to the current asset element. Moreover a derivative contract specific style has been applied.
        /// </summary>
        string ConvertedSynthPositionPrice { get; set; }

        /// <summary>
        /// Get/Set the value of the converted clearing (closing) price of a synthetic position (netting by asset).
        /// The value has been converted from the numerical base 100 to the display numerical
        /// base of the derivative contract related to the current asset element. Moreover a derivative contract specific style has been applied.
        /// </summary>
        string ConvertedClearingPrice { get; set; }

        /// <summary>
        /// Get the vector of prices of the trades, having the current asset (instance of ETDRepository) as underlying product. 
        /// All the prices are converted from the default numerical base into the display numerical base of the derivative contract 
        /// related to the current asset element. Moreover a derivative contract specific style has been applied.
        /// </summary>
        IConvertedTradePrice[] ConvertedTradePrices { get; }

        /// <summary>
        /// Add or update a new IConvertedTradePrice reference to the ConvertedTradePrices collection
        /// </summary>
        /// <param name="pHRef">trade identifier reference</param>
        /// <param name="pConvertedPrice">converted value</param>
        /// <returns>true if the reference is added, false if updated</returns>
        bool AddUpdateConvertedTradePrice(string pHRef, string pConvertedPrice);

        /// <summary>
        /// Evaluate the weighted average price of the current trade prices collection, adding the new given price.
        /// The average is evaluated in base100.
        /// </summary>
        /// <param name="pLastPx">trade price to add to the current average value</param>
        /// <param name="pLastQty">trade quantity used to multiply the input trade price</param>
        /// <param name="pCurrentBase">current numerical base of the trade price</param>
        /// <returns>the average value in base100</returns>
        decimal EvaluateWeightedAverage(decimal pLastPx, decimal pLastQty, int pCurrentBase, SideEnum pSide);
    }

    // 20120820 MF - Ticket 18073
    /// <summary>
    /// Interface related to a converted price value. The value conversion is performed according with a specific base and format style.
    /// </summary>
    public interface IConvertedTradePrice
    {
        /// <summary>
        /// reference to the OTCmlId of the element whose the price has been converted
        /// </summary>
        string HRef { get; set; }

        /// <summary>
        /// Converted price value
        /// </summary>
        string ConvertedPrice { get; set; }
    }

    #endregion IETDRepository

    #region IEventCodes
    public interface IEventCodes
    {
        #region Accessors
        bool ProductReferenceSpecified { get; }
        IReference ProductReference { get; }
        bool StreamIdSpecified { get; }
        EFS_NonNegativeInteger StreamId { get; }
        bool EventCodeSpecified { get; }
        IScheme EventCode { get; }
        bool EventTypeSpecified { get; }
        IScheme EventType { get; }
        bool SettlementDateSpecified { get; }
        EFS_Date SettlementDate { get; }
        #endregion Accessors
    }
    #endregion IEventCodes
    #region IEventCodesSchedule
    public interface IEventCodesSchedule
    {
        #region Accessors
        IEventCodes[] EventCodes { get; }
        #endregion Accessors
    }
    #endregion IEventCodesSchedule

    #region IEfsDocument
    /// <summary>
    /// 
    /// </summary>
    /// FI 20130626 [] add Herite de IRepositoryDocument
    public interface IEfsDocument : IRepositoryDocument
    {
        #region Accessors
        /// <summary>
        /// Représente la version du document
        /// </summary>
        EfsMLDocumentVersionEnum EfsMLversion { get; set; }
        #endregion
    }
    #endregion


    #region IRepositoryDocument
    /// <summary>
    /// 
    /// </summary>
    /// FI 20130626 [] add IRepositoryDocument
    /// FI 20151708 [XXXXX] Modify
    public interface IRepositoryDocument
    {
        #region Accessors
        bool RepositorySpecified { set; get; }
        IRepository Repository { get; set; }
        #endregion Accessors

        #region Methodes
        IRepository CreateRepository();
        /// <summary>
        /// Retourne tous les assets présents dans le repository
        /// </summary>
        /// <returns></returns>
        /// FI 20151708 [XXXXX] Add
        List<IAssetRepository> GetAllRepositoryAsset();
        #endregion Methodes
    }
    #endregion

    #region IEfsSettlementInformation
    public interface IEfsSettlementInformation
    {
        #region Accessors
        bool StandardSpecified { set; get; }
        StandardSettlementStyleEnum Standard { get; }
        bool InstructionSpecified { set; get; }
        IEfsSettlementInstruction Instruction { get; }
        #endregion Accessors
    }
    #endregion IEfsSettlementInformation
    #region IEfsSettlementInstruction
    public interface IEfsSettlementInstruction
    {
        #region Accessors
        bool SettlementMethodSpecified { set; get; }
        IScheme SettlementMethod { set; get; }
        bool BeneficiaryBankSpecified { set; get; }
        IRouting BeneficiaryBank { set; get; }
        IRouting Beneficiary { set; get; }
        bool CorrespondentInformationSpecified { set; get; }
        IRouting CorrespondentInformation { set; get; }
        bool IntermediaryInformationSpecified { set; get; }
        IIntermediaryInformation[] IntermediaryInformation { set; get; }
        bool SettlementMethodInformationSpecified { set; get; }
        IRouting SettlementMethodInformation { set; get; }
        bool InvestorInformationSpecified { set; get; }
        IRouting InvestorInformation { set; get; }
        bool OriginatorInformationSpecified { set; get; }
        IRouting OriginatorInformation { set; get; }
        int IdssiDb { set; get; }
        int IdIssi { set; get; }
        #endregion Accessors
        #region Methodes
        IScheme CreateSettlementMethod();
        IRouting CreateCorrespondentInformation();
        IIntermediaryInformation CreateIntermediaryInformation();
        IRouting CreateBeneficiary();
        IRouting CreateRouting();
        IRoutingCreateElement CreateRoutingCreateElement();
        #endregion
    }
    #endregion IEfsSettlementInstruction

    #region IDebtSecurityOption
    // EG 20150422 [20513] BANCAPERTA New
    public interface IDebtSecurityOption : IBondOption
    {
        #region Accessors
        IReference IssuerPartyReference { get; }
        bool SecurityAssetSpecified { set; get; }
        ISecurityAsset SecurityAsset { set; get; }
        IDebtSecurity DebtSecurity { get; }
        int SecurityAssetOTCmlId { get; }
        EFS_Asset Efs_Asset(string pCS);
        EFS_BondOption Efs_BondOption { set; get; }
        EFS_BondOptionPremium Efs_BondOptionPremium { set; get; }
        #endregion Accessors
    }
    #endregion IDebtSecurityOption

    #region IEquitySecurityTransaction
    /// EG 20150306 [POC-BERKELEY] : Add marginRatio
    public interface IEquitySecurityTransaction : IExchangeTradedBase
    {
        #region Accessors
        /// <summary>
        /// 
        /// </summary>
        EFS_EquitySecurityTransaction Efs_EquitySecurityTransaction { set; get; }
        /// <summary>
        /// 
        /// </summary>
        IPayment GrossAmount { set; get; }
        bool MarginRatioSpecified { set; get; }
        IMarginRatio MarginRatio { set; get; }
        IMarginRatio CreateMarginRatio { get; }
        #endregion Accessors
    }
    #endregion IEquitySecurityTransaction


    /// <summary>
    ///  Représente un asset de type Cash
    /// </summary>
    /// FI 20160530 [21885] Add
    public interface IAssetCashRepository : IAssetRepository
    {

    }

    /// <summary>
    ///  Représente un asset de type commodity
    /// </summary>
    /// FI 20161214 [21916] Add
    /// FI 20170116 [21916] Modify
    /// FI 20170201 [21916] Modify 
    public interface IAssetCommodityRepository : IAssetRepository
    {

        Boolean AssetSymbolSpecified { set; get; }
        /// <summary>
        /// Symbole
        /// </summary>
        string AssetSymbol { set; get; }


        Boolean ContractSymbolSpecified { set; get; }
        /// <summary>
        /// Symbole du contrat (Contrat Spot sur energie)
        /// </summary>
        string ContractSymbol { set; get; }


        Boolean ExchContractSymbolSpecified { set; get; }
        /// <summary>
        /// Symbole du contrat (Contrat Spot sur energie)
        /// </summary>
        string ExchContractSymbol { set; get; }


        Boolean DurationSpecified { set; get; }
        /// <summary>
        /// duration du contrat (Contrat Spot sur energie)
        /// <para>Ex 1Hour, 15Minutes</para>
        /// </summary>
        string Duration { set; get; }


        Boolean DeliveryPointSpecified { set; get; }
        /// <summary>
        /// Point de livraison (Contrat Spot sur energie) 
        /// </summary>
        string DeliveryPoint { set; get; }


        Boolean DeliveryTimezoneSpecified { set; get; }
        /// <summary>
        /// timezone pour les horaires de livraison (Contrat Spot sur energie) 
        /// </summary>
        string DeliveryTimezone { set; get; }



        Boolean QtyUnitSpecified { set; get; }
        /// <summary>
        /// Unité dans laquelle sont exprimées les quantités (Mwh par exemple) (Contrat Spot sur energie) 
        /// </summary>
        /// FI 20170116 [21916] Add
        string QtyUnit { set; get; }


        Boolean QtyScaleSpecified { set; get; }
        /// <summary>
        /// Nombre de digit utilisés pour la partie décimale
        /// </summary>
        ///  FI 20170201 [21916] Add 
        int QtyScale { set; get; }
    
    }



    /// <summary>
    /// 
    /// </summary>
    /// FI 20140818 [20275] add 
    /// FI 20140903 [20275] hérite de IAssetRepository
    public interface IAssetEquityRepository : IAssetRepository
    {

        Boolean AssetSymbolSpecified { set; get; }
        /// <summary>
        /// Symbole
        /// </summary>
        string AssetSymbol { set; get; }

        Boolean BBGCodeSpecified { set; get; }
        /// <summary>
        /// Bloomberg Code 
        /// </summary>
        string BBGCode { set; get; }


        Boolean RICCodeSpecified { set; get; }
        /// <summary>
        /// Reuters Code 
        /// </summary>
        string RICCode { set; get; }

        Boolean ISINCodeSpecified { set; get; }
        /// <summary>
        /// ISIN Code 
        /// </summary>
        string ISINCode { set; get; }

        Boolean CFICodeSpecified { set; get; }
        /// <summary>
        /// CFI Code 
        /// </summary>
        string CFICode { set; get; }

    }

    /// <summary>
    /// 
    /// </summary>
    /// FI 20140818 [20275] add 
    /// FI 20140903 [20275] hérite de IAssetRepository
    public interface IAssetIndexRepository : IAssetRepository
    {
        Boolean AssetSymbolSpecified { set; get; }
        /// <summary>
        /// Symbole
        /// </summary>
        string AssetSymbol { set; get; }

        Boolean BBGCodeSpecified { set; get; }
        /// <summary>
        /// Bloomberg Code 
        /// </summary>
        string BBGCode { set; get; }

        Boolean RICCodeSpecified { set; get; }
        /// <summary>
        /// Reuters Code 
        /// </summary>
        string RICCode { set; get; }

    }

    /// <summary>
    /// Représente un asset de change
    /// </summary>
    /// EG 20170222 Add Class
    /// FI 20150331 [XXPOC] Modify 
    public interface IAssetFxRateRepository : IAssetRepository
    {
        /// <summary>
        /// 
        /// </summary>
        /// FI 20150331 [XXPOC] Add
        IInformationSource PrimaryRateSrc { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// FI 20150331 [XXPOC] Add
        IQuotedCurrencyPair QuotedCurrencyPair { set; get; }

        /// <summary>
        /// 
        /// </summary>
        /// FI 20150331 [XXPOC] Add
        IBusinessCenterTime FixingTime { set; get; }
    }

    /// <summary>
    /// Représente un titre de rémunération
    /// </summary>
    /// FI 20151019 [21317] Add
    public interface IAssetDebtSecurityRepository : IAssetRepository
    {

        Boolean BBGCodeSpecified { set; get; }
        /// <summary>
        /// Bloomberg Code 
        /// </summary>
        string BBGCode { set; get; }


        Boolean RICCodeSpecified { set; get; }
        /// <summary>
        /// Reuters Code 
        /// </summary>
        string RICCode { set; get; }

        Boolean ISINCodeSpecified { set; get; }
        /// <summary>
        /// ISIN Code 
        /// </summary>
        string ISINCode { set; get; }

        Boolean CFICodeSpecified { set; get; }
        /// <summary>
        /// CFI Code 
        /// </summary>
        string CFICode { set; get; }

        Boolean SEDOLCodeSpecified { set; get; }
        /// <summary>
        /// SEDOL Code 
        /// </summary>
        string SEDOLCode { set; get; }


        Boolean ParValueSpecified { set; get; }
        /// <summary>
        /// Nominal du titre
        /// </summary>
        /// FI 20151019 [21317] GLOP (cela me gêne de faire appel à EfsML.Notification)
        ReportAmount ParValue { set; get; }
    }





    #region IExtendedBarrier
    public interface IExtendedBarrier
    {
        #region Accessors
        bool BarrierCapSpecified { set; get; }
        IExtendedTriggerEvent BarrierCap { set; get; }
        bool BarrierFloorSpecified { set; get; }
        IExtendedTriggerEvent BarrierFloor { set; get; }
        #endregion Accessors
    }
    #endregion IExtendedBarrier
    #region IExtendedTriggerEvent
    public interface IExtendedTriggerEvent : ITriggerEvent
    {
        #region Accessors
        bool TriggerPerComponentSpecified { set; get; }
        EFS_Boolean TriggerPerComponent { set; get; }
        bool TriggerHitOperatorSpecified { set; get; }
        TriggerHitOperatorEnum TriggerHitOperator { set; get; }
        #endregion Accessors
    }
    #endregion IExtendedTriggerEvent

    #region IExchangeTradedDerivative
    public interface IExchangeTradedDerivative : IExchangeTradedBase
    {
        #region Accessors
        /// <summary>
        /// 
        /// </summary>
        EFS_ExchangeTradedDerivative Efs_ExchangeTradedDerivative { set; get; }
        #endregion Accessors
    }
    #endregion IExchangeTradedDerivative

    #region IFlowContext
    public interface IFlowContext
    {
        #region Membres
        //
        bool CurrencySpecified { set; get; }
        string Currency { set; get; }
        //
        bool PartyContextSpecified { set; get; }
        IPartyPayerReceiverReference[] PartyContext { set; get; }
        //
        bool EventCodesScheduleSpecified { set; get; }
        IEventCodesSchedule EventCodesSchedule { set; get; }
        //
        bool CashSecuritiesSpecified { set; get; }
        CashSecuritiesEnum CashSecurities { set; get; }
        #endregion Membres
    }
    #endregion IFlowContext
    #region IForwardRepoTransactionLeg
    public interface IForwardRepoTransactionLeg : IRepoTransactionLeg
    {
        #region Accessors
        bool RepoInterestSpecified { set; get; }
        EFS_Decimal RepoInterest { set; get; }
        #endregion Accessors
    }
    #endregion IForwardRepoTransactionLeg
    #region IFullCouponCalculationRules
    // EG 20150907 [21317] Add recordDate|exDate
    public interface IFullCouponCalculationRules
    {
        #region Accessors
        bool CalculationMethodSpecified { set; get; }
        FullCouponCalculationMethodEnum CalculationMethod { set; get; }
        bool UnitCouponRoundingSpecified { set; get; }
        IRounding UnitCouponRounding { set; get; }
        bool RoundingSpecified { set; get; }
        IRounding Rounding { set; get; }
        bool RecordDateSpecified { set; get; }
        IRelativeDateOffset RecordDate { set; get; }
        bool ExDateSpecified { set; get; }
        IRelativeDateOffset ExDate { set; get; }
        #endregion Accessors
    }
    #endregion IFullCouponCalculationRules
    #region IFutureTransaction
    public interface IFutureTransaction : IAbstractUnitTransaction
    {
        #region Accessors
        EFS_Decimal DeliveryPrice { set; get; }
        IFuture Future { set; get; }
        #endregion Accessors
    }
    #endregion IFutureTransaction

    #region IImplicitProvision
    public interface IImplicitProvision
    {
        #region Accessors
        bool CancelableProvisionSpecified { set; get; }
        IEmpty CancelableProvision { set; get; }
        bool MandatoryEarlyTerminationProvisionSpecified { set; get; }
        IEmpty MandatoryEarlyTerminationProvision { set; get; }
        bool OptionalEarlyTerminationProvisionSpecified { set; get; }
        IEmpty OptionalEarlyTerminationProvision { set; get; }
        bool ExtendibleProvisionSpecified { set; get; }
        IEmpty ExtendibleProvision { set; get; }
        bool StepUpProvisionSpecified { set; get; }
        IEmpty StepUpProvision { set; get; }
        #endregion Accessors
    }
    #endregion IImplicitProvision
    #region IInterestPayoutEvent
    public interface IInterestPayoutEvent : IMidLifeEvent
    {
        #region Accessors
        IMoney Payment { set; get; }
        bool TransferSpecified { set; get; }
        EFS_Decimal Transfer { set; get; }
        #endregion Accessor
    }
    #endregion IInterestPayoutEvent
    #region IInitialInvoiceAmounts
    public interface IInitialInvoiceAmounts : IInvoiceAmounts
    {
        #region Accessors
        EFS_String Identifier { set; get; }
        string OtcmlId { set; get; }
        int OTCmlId { set; get; }
        #endregion Accessors
        #region Methods
        #endregion Methods
    }
    #endregion IInitialInvoiceAmounts
    #region IInitialNetInvoiceAmounts
    public interface IInitialNetInvoiceAmounts : INetInvoiceAmounts
    {
        #region Accessors
        EFS_String Identifier { set; get; }
        string OtcmlId { set; get; }
        int OTCmlId { set; get; }
        #endregion Accessors
        #region Methods
        INetInvoiceAmounts CreateNetInvoiceAmounts();
        #endregion Methods
    }
    #endregion IInitialNetInvoiceAmounts

    /// FI 20150218 [20275] Add
    public interface IInstrumentRepository : ICommonRepository
    {
        string Product { set; get; }
        string GProduct { set; get; }
        Boolean IsMargining { set; get; }
        Boolean IsFunding { set; get; }
    }


    #region IInvoice
    public interface IInvoice
    {
        #region Accessors
        IReference PayerPartyReference { set; get; }
        IReference ReceiverPartyReference { set; get; }
        IInvoicingScope Scope { set; get; }
        IAdjustedDate InvoiceDate { set; get; }
        IAdjustableOrRelativeAndAdjustedDate PaymentDate { set; get; }
        bool GrossTurnOverAmountSpecified { set; get; }
        IMoney GrossTurnOverAmount { set; get; }
        bool RebateIsInExcessSpecified { set; get; }
        EFS_Boolean RebateIsInExcess { set; get; }
        bool RebateAmountSpecified { set; get; }
        IMoney RebateAmount { set; get; }
        bool RebateConditionsSpecified { set; get; }
        IRebateConditions RebateConditions { set; get; }
        bool TaxSpecified { set; get; }
        IInvoiceTax Tax { set; get; }
        IMoney NetTurnOverAmount { set; get; }
        IMoney NetTurnOverIssueAmount { set; get; }
        bool NetTurnOverIssueRateSpecified { set; get; }
        EFS_Decimal NetTurnOverIssueRate { set; get; }
        bool IssueRateIsReverseSpecified { set; get; }
        EFS_Boolean IssueRateIsReverse { set; get; }
        bool IssueRateReadSpecified { set; get; }
        EFS_Decimal IssueRateRead { set; get; }
        bool InvoiceRateSourceSpecified { set; get; }
        IInvoiceRateSource InvoiceRateSource { set; get; }
        bool NetTurnOverAccountingAmountSpecified { set; get; }
        IMoney NetTurnOverAccountingAmount { set; get; }
        bool NetTurnOverAccountingRateSpecified { set; get; }
        EFS_Decimal NetTurnOverAccountingRate { set; get; }
        bool AccountingRateIsReverseSpecified { set; get; }
        EFS_Boolean AccountingRateIsReverse { set; get; }
        bool AccountingRateReadSpecified { set; get; }
        EFS_Decimal AccountingRateRead { set; get; }
        IInvoiceDetails InvoiceDetails { set; get; }
        EFS_Invoice Efs_Invoice { set; get; }
        EFS_BaseInvoice Efs_BaseInvoice { set; get; }
        EFS_InitialInvoice Efs_InitialInvoice { set; get; }
        EFS_TheoricInvoice Efs_TheoricInvoice { set; get; }

        #endregion Accessors
        #region Methods
        // EG 20240205 [WI640] New
        IReference CreatePartyReference(string pHref);
        IInvoicingScope CreateInvoicingScope(int pOTCmlId, string pIdentifier);
        IInvoiceDetails CreateInvoiceDetails(int pTradeLength);
        IRebateConditions CreateRebateConditions();
        IInvoiceRateSource CreateInvoiceRateSource(IInformationSource pRateSource, IBusinessCenterTime pBusinessCenterTime, IRelativeDateOffset pRelativeDateOffset);
        IInformationSource CreateInformationSource(int pOTCmlId, string pIdentifier);
        IInvoiceTax CreateInvoiceTax(int pNbTax);
        ISpheresSource CreateSpheresSource { get; }
        IInvoiceTradeSort CreateInvoiceTradeSort();
        IInvoiceTradeSortKeys CreateInvoiceTradeSortKeys(int pDim);
        IInvoiceTradeSortGroups CreateInvoiceTradeSortGroups(int pDim);
        ISchemeId CreateInvoiceTradeSortKey();
        IInvoiceTradeSortGroup CreateInvoiceTradeSortGroup();
        IInvoiceTradeSortKeyValue[] CreateInvoiceInvoiceTradeSortKeyValue(int pDim);
        IInvoiceTradeSortSum CreateInvoiceTradeSortSum();
        IReference[] CreateInvoiceTradeReference(int pDim);
        IReference CreateInvoiceTradeReference();
        ITripleInvoiceAmounts CreateTripleInvoiceAmounts();
        #endregion Methods
    }
    #endregion IInvoice
    #region IInvoiceAmounts
    public interface IInvoiceAmounts
    {
        #region Accessors
        IMoney GrossTurnOverAmount { set; get; }
        bool RebateAmountSpecified { set; get; }
        IMoney RebateAmount { set; get; }
        bool TaxSpecified { set; get; }
        IInvoiceTax Tax { set; get; }
        IMoney NetTurnOverAmount { set; get; }
        IMoney NetTurnOverIssueAmount { set; get; }
        bool NetTurnOverAccountingAmountSpecified { set; get; }
        IMoney NetTurnOverAccountingAmount { set; get; }
        #endregion Accessors
        #region Methods
        IMoney CreateMoney(decimal pAmount, string pCurrency);
        #endregion Methods
    }
    #endregion IInvoiceAmounts
    #region IInvoiceDetails
    public interface IInvoiceDetails
    {
        #region Accessors
        IInvoiceTrade[] InvoiceTrade { set; get; }
        bool InvoiceTradeSortSpecified { set; get; }
        IInvoiceTradeSort InvoiceTradeSort { set; get; }
        #endregion Accessors
        #region Methods
        //20090909 FI [Add Asset on InvoiceTrade] ajout du paramètre pSalesLength
        IInvoiceTrade CreateInvoiceTrade(string pCS, string pIdentifier, int pTradeOTCmlId,
            DateTime pTradeDate, DateTime pInDate, DateTime pOutDate, string idC, TradeSideEnum pSide, string pCounterpartyReference,
            string pInstrIdentifier, int pInstrOTCmlId, int pEventLength, int pTraderLength, int pSalesLength, DataDocumentContainer pDataDocument);
        void SetInvoiceTrade(int pIndex, IInvoiceTrade pInvoiceTrade);
        IInvoiceTrade GetInvoiceTrade(int pIdT);
        IInvoiceFee InvoiceFee(int pIdE);
        #endregion Methods
    }
    #endregion IInvoiceDetails
    #region IInvoiceFee
    public interface IInvoiceFee
    {
        #region Accessors
        EFS_String FeeType { set; get; }
        IMoney FeeAmount { set; get; }
        // EG 20091110
        IMoney FeeBaseAmount { set; get; }
        IMoney FeeInitialAmount { set; get; }
        bool FeeAccountingAmountSpecified { set; get; }
        IMoney FeeAccountingAmount { set; get; }
        // EG 20091110
        bool FeeBaseAccountingAmountSpecified { set; get; }
        IMoney FeeBaseAccountingAmount { set; get; }
        bool FeeInitialAccountingAmountSpecified { set; get; }
        IMoney FeeInitialAccountingAmount { set; get; }
        EFS_Date FeeDate { set; get; }
        EFS_Integer IdA_Pay { set; get; }
        bool IdB_PaySpecified { set; get; }
        EFS_Integer IdB_Pay { set; get; }
        bool FeeScheduleSpecified { set; get; }
        IInvoiceFeeSchedule FeeSchedule { set; get; }
        string OtcmlId { set; get; }
        int OTCmlId { set; get; }
        #endregion Accessor
    }
    #endregion IInvoiceFee
    #region IInvoiceFeeSchedule
    public interface IInvoiceFeeSchedule
    {
        #region Accessors
        string OtcmlId { set; get; }
        int OTCmlId { set; get; }
        bool IdentifierSpecified { set; get; }
        EFS_String Identifier { set; get; }
        bool FormulaDCFSpecified { set; get; }
        EFS_String FormulaDCF { set; get; }
        bool DurationSpecified { set; get; }
        // 20091110
        EFS_String Duration { set; get; }
        //PL 20141023
        //bool assessmentBasisValueSpecified { set; get; }
        //EFS_Decimal assessmentBasisValue { set; get; }
        bool AssessmentBasisValue1Specified { set; get; }
        EFS_Decimal AssessmentBasisValue1 { set; get; }
        bool AssessmentBasisValue2Specified { set; get; }
        EFS_Decimal AssessmentBasisValue2 { set; get; }
        #endregion Accessor
    }
    #endregion IInvoiceFeeSchedule
    #region IInvoiceFees
    public interface IInvoiceFees
    {
        #region Accessors
        IInvoiceFee[] InvoiceFee { set; get; }
        #endregion Accessor
        #region Methods
        IMoney CreateMoney(decimal pAmount, string pCurrency);
        // 20091110
        IInvoiceFee CreateInvoiceFee(int pOTCmlId, string pFeeType, string pCurrency, decimal pAmount, decimal pBaseAmount, decimal pInitialAmount, DateTime pFeeDate, int pIdA_Pay, int pIdB_Pay, string pIdCAccount);
        void SetInvoiceFee(int pIndex, IInvoiceFee pInvoiceFee);
        IInvoiceFeeSchedule CreateInvoiceFeeSchedule();
        #endregion Methods
    }
    #endregion IInvoiceFees
    #region IInvoiceRateSource
    public interface IInvoiceRateSource
    {
        #region Accessors
        IInformationSource RateSource { set; get; }
        IBusinessCenterTime FixingTime { set; get; }
        IRelativeDateOffset FixingDate { set; get; }
        IAdjustedDate AdjustedFixingDate { set; get; }
        bool AdjustedFixingDateSpecified { set; get; }
        object Clone();
        #endregion Accessor
    }
    #endregion IInvoiceRateSource
    #region IInvoiceSettlement
    public interface IInvoiceSettlement
    {
        #region Accessors
        IReference PayerPartyReference { get; set; }
        IReference ReceiverPartyReference { get; set; }
        IAdjustedDate ReceptionDate { set; get; }
        IAccountNumber AccountNumber { set; get; }
        IMoney SettlementAmount { set; get; }
        IMoney CashAmount { set; get; }
        bool BankChargesAmountSpecified { set; get; }
        IMoney BankChargesAmount { set; get; }
        bool VatBankChargesAmountSpecified { set; get; }
        IMoney VatBankChargesAmount { set; get; }
        IMoney NetCashAmount { set; get; }
        IMoney UnallocatedAmount { set; get; }
        bool FxGainOrLossAmountSpecified { set; get; }
        IMoney FxGainOrLossAmount { set; get; }
        bool AllocatedInvoiceSpecified { set; get; }
        IAllocatedInvoice[] AllocatedInvoice { set; get; }
        bool AvailableInvoiceSpecified { set; get; }
        IAvailableInvoice[] AvailableInvoice { set; get; }
        #endregion Accessors
        #region Methods
        IAvailableInvoice CreateAvailableInvoice();
        IAllocatedInvoice CreateAllocatedInvoice();
        bool IsInvoiceNotSelected(int pIdT);
        Type TypeofAvailableInvoice { get; }
        Type TypeofAllocatedInvoice { get; }
        EFS_InvoiceSettlement Efs_InvoiceSettlement { set; get; }
        #endregion Methods
    }
    #endregion IInvoiceSettlement
    #region IInvoiceSupplement
    public interface IInvoiceSupplement : IInvoice
    {
        #region Accessors
        IInitialInvoiceAmounts InitialInvoiceAmount { get; set; }
        INetInvoiceAmounts BaseNetInvoiceAmount { get; set; }
        IInvoiceAmounts TheoricInvoiceAmount { get; set; }
        #endregion Accessors
        #region Methods
        #endregion Methods
    }
    #endregion IInvoiceSupplement
    #region IInvoiceTax
    public interface IInvoiceTax : ITripleInvoiceAmounts
    {
        #region Accessors
        ITaxSchedule[] Details { set; get; }
        #endregion Accessors
        #region Methods
        decimal GetBaseAmountForTax(string pCs, decimal pNetTurnoverAmount, string pCurrency);
        decimal GetTotalTaxAmount(string pCs, decimal pBaseAmount, string pCurrency);
        #endregion Methods
    }
    #endregion IInvoiceTax

    #region IInvoiceTrade
    // 20090427 EG Add side (TradeSideEnum)
    // 20090427 EG Add counterpartyPartyReference (PartyReference)
    // EG 20220324 [XXXXX] Change type of element contract (IContractRepository) 
    public interface IInvoiceTrade
    {
        #region Accessors
        EFS_String Identifier { set; get; }
        ISpheresIdScheme Instrument { set; get; }
        ITradeDate TradeDate { set; get; }
        EFS_Date InDate { set; get; }
        EFS_Date OutDate { set; get; }
        //FI 20091223 [16471] Ajout de periodNumberOfDays 
        bool PeriodNumberOfDaysSpecified { set; get; }
        EFS_Integer PeriodNumberOfDays { set; get; }
        ICurrency Currency { set; get; }
        TradeSideEnum Side { set; get; }
        IReference CounterpartyPartyReference { set; get; }
        bool NotionalAmountSpecified { set; get; }
        IMoney NotionalAmount { set; get; }
        IInvoiceFees InvoiceFees { set; get; }
        bool TradersSpecified { set; get; }
        ITrader[] Traders { set; get; }
        //20091021 FI [add sales in invoice] add Sales
        bool SalesSpecified { set; get; }
        ITrader[] Sales { set; get; }
        bool AssetSpecified { set; get; }
        IShortAsset[] Asset { set; get; }
        //20100628 EG [add market/contract in invoice]
        bool MarketSpecified { set; get; }
        ICommonRepository Market { set; get; }
        bool ContractSpecified { set; get; }
        IContractRepository Contract { set; get; }
        string OtcmlId { set; get; }
        int OTCmlId { get; }
        string Id { set; get; }
        #endregion Accessor
        #region Methods
        IInvoiceFee InvoiceFee(int pIdE);
        ITrader CreateTrader();
        #endregion Methods
    }
    #endregion IInvoiceTrade

    #region IInvoiceTradeSort
    public interface IInvoiceTradeSort
    {
        #region Accessors
        IInvoiceTradeSortKeys Keys { set; get; }
        IInvoiceTradeSortGroups Groups { set; get; }
        #endregion
    }
    #endregion

    #region IInvoiceTradeSortGroups
    public interface IInvoiceTradeSortGroups
    {
        IInvoiceTradeSortGroup[] Group { set; get; }
    }
    #endregion

    #region IInvoiceTradeSortGroup
    public interface IInvoiceTradeSortGroup
    {
        IInvoiceTradeSortKeyValue[] KeyValue { set; get; }
        IReference[] InvoiceTradeReference { set; get; }
        IInvoiceTradeSortSum Sum { set; get; }
    }
    #endregion

    #region IInvoiceTradeSortKeyValue
    public interface IInvoiceTradeSortKeyValue : IReference
    {
        String Value { set; get; }
    }
    #endregion

    #region IInvoiceTradeSortSum
    public interface IInvoiceTradeSortSum
    {
        IMoney FeeAmount { set; get; }
        IMoney FeeAccountingAmount { set; get; }
        IMoney FeeInitialAmount { set; get; }
        IMoney FeeInitialAccountingAmount { set; get; }
    }
    #endregion

    #region IInvoiceTradeSortKeys
    /// <summary>
    /// liste des clefs de regroupements pour le tri des trades (alimentée avec INVRULESORDERBY)
    /// </summary>
    public interface IInvoiceTradeSortKeys
    {
        #region Accessors
        ISchemeId[] Key { set; get; }
        #endregion
    }
    #endregion

    #region IInvoicingScope
    public interface IInvoicingScope : IScheme
    {
        #region Accessors
        string OtcmlId { set; get; }
        int OTCmlId { set; get; }
        #endregion Accessors
    }
    #endregion IInvoicingScope
    #region IInvoicingScopeRepository
    public interface IInvoicingScopeRepository : ICommonRepository
    {
        #region Accessors
        bool PaymentTypeSpecified { set; get; }
        string PaymentType { get; set; }
        bool IdC_FeeSpecified { set; get; }
        string IdC_Fee { set; get; }
        bool EventTypeSpecified { set; get; }
        string EventType { set; get; }
        #endregion Accessors
    }
    #endregion IMarketRepository

    #region IIssiItems
    public interface IIssiItems
    {
        #region Accessors
        IEfsSettlementInstruction GetInstruction(PayerReceiverEnum pPayRec, bool pIsSecurityFlow);
        #endregion Accessors
    }
    #endregion IIssiItems
    #region IIssiItemsRoutingActorsInfo
    public interface IIssiItemsRoutingActorsInfo : IIssiItems
    {
        #region Accessors
        SettlementRoutingActorsBuilder IssiActorsInfo { get; }
        #endregion Accessors
    }
    #endregion IIssiItemsRoutingActorsInfo

    #region IKnownAmountSchedule
    public interface IKnownAmountSchedule : IAmountSchedule
    {
        #region Accessors
        bool NotionalValueSpecified { get; }
        IMoney NotionalValue { get; }
        bool DayCountFractionSpecified { get; set; }
        DayCountFractionEnum DayCountFraction { get; set; }
        #endregion Accessors
    }
    #endregion IKnownAmountSchedule

    #region IExchangeTradedBase
    public interface IExchangeTradedBase
    {
        #region Accessors
        IFixTradeCaptureReport TradeCaptureReport { set; get; }
        /// <summary>
        /// Obtient l'acheteur
        /// </summary>
        IReference BuyerPartyReference { get; }
        /// <summary>
        /// Obtient le vendeur
        /// </summary>
        IReference SellerPartyReference { get; }
        /// <summary>
        /// Obtient la clearingOrganization 
        /// </summary>
        IReference ClearingOrganization { get; }
        /// <summary>
        /// Obtient la clearingFirm
        /// </summary>
        IReference ClearingFirm { get; }
        // EG 20150624 (21151] New
        bool CategorySpecified { set; get; }

        /// <summary>
        /// 
        /// </summary>
        Nullable<CfiCodeCategoryEnum> Category { set; get; }
        /// <summary>
        /// 
        /// </summary>
        IFixInstrument CreateFixInstrument();
        // EG 20150624 (21151] New
        // EG 20150920 [21374] Int (int32) to Long (Int64) 
        // EG 20170127 Qty Long To Decimal
        void InitPositionTransfer(decimal pQuantity, DateTime pBusinessDate);
        #endregion Accessors
    }
    #endregion IExchangeTradedBase
    #region ILoanDeposit
    public interface ILoanDeposit
    {
        #region Accessors
        IInterestRateStream[] Stream { get; }
        bool CancelableProvisionSpecified { get; }
        ICancelableProvision CancelableProvision { get; }
        bool ExtendibleProvisionSpecified { get; }
        IExtendibleProvision ExtendibleProvision { get; }
        bool EarlyTerminationProvisionSpecified { get; }
        IEarlyTerminationProvision EarlyTerminationProvision { get; }
        bool ImplicitProvisionSpecified { get; }
        IImplicitProvision ImplicitProvision { get; }
        bool ImplicitCancelableProvisionSpecified { get; }
        bool ImplicitOptionalEarlyTerminationProvisionSpecified { get; }
        bool ImplicitMandatoryEarlyTerminationProvisionSpecified { get; }
        bool ImplicitExtendibleProvisionSpecified { get; }
        bool StepUpProvisionSpecified { get; }
        IStepUpProvision StepUpProvision { get; }
        bool AdditionalPaymentSpecified { set; get; }
        IPayment[] AdditionalPayment { get; }
        EFS_EventDate MaxTerminationDate { get; }
        #endregion Accessors
    }
    #endregion ILoanDeposit
    #region ILocalization
    public interface ILocalization
    {
        #region Accessors
        bool CountryOfIssueSpecified { set; get; }
        IScheme CountryOfIssue { set; get; }
        bool StateOrProvinceOfIssueSpecified { set; get; }
        IScheme StateOrProvinceOfIssue { set; get; }
        bool LocaleOfIssueSpecified { set; get; }
        IScheme LocaleOfIssue { set; get; }
        #endregion Accessors
    }
    #endregion ILocalization

    #region IMargin
    public interface IMargin
    {
        #region Accessors
        MarginTypeEnum MarginType { set; get; }
        EFS_Decimal MarginFactor { set; get; }
        #endregion Accessor
    }
    #endregion IMargin
    #region IMarginRequirement
    /// <summary>
    /// 
    /// </summary>
    /// PM 20140317 [17861] Ajout des Accessors : isGrossMargin, initialMarginMethodSpecified et initialMarginMethod
    /// FI 20160613 [22256] Modify
    public interface IMarginRequirement
    {
        #region Accessors
        bool IsGrossMarginSpecified { set; get; }
        EFS_Boolean IsGrossMargin { set; get; }
        bool InitialMarginMethodSpecified { set; get; }
        // PM 20160404 [22116] initialMarginMethod devient un array
        InitialMarginMethodEnum[] InitialMarginMethod { get; set; }
        IReference ClearingOrganizationPartyReference { get; set; }
        IReference MarginRequirementOfficePartyReference { get; set; }
        IReference EntityPartyReference { get; set; }
        SettlSessIDEnum Timing { get; set; }
        ISimplePayment[] Payment { get; set; }
        EFS_SimplePayment[] Efs_SimplePayment { get; set; }

        bool UnderlyingStockSpecified { set; get; }
        /// <summary>
        /// Positions actions déposées et utilisées pour réduire les positions Short Future et Short Call Future
        /// </summary>
        /// FI 20160613 [22256] Add
        EfsML.v30.MarginRequirement.UnderlyingStock[] UnderlyingStock { get; set; }
        #endregion Accessor
    }
    #endregion IMarginRequirement

    #region IMarketRepository
    public interface IMarketRepository : ICommonRepository
    {
        #region Accessors
        
        bool ISO10383_ALPHA4Specified { set; get; }
        /// <summary>
        /// Code MIC (http://www.iso15022.org/download/ISO10383_MIC.xls.)
        /// </summary>
        string ISO10383_ALPHA4 { get; set; }
        bool AcronymSpecified { set; get; }
        string Acronym { set; get; }
        bool CitySpecified { set; get; }
        string City { set; get; }
        bool ExchangeSymbolSpecified { set; get; }
        string ExchangeSymbol { set; get; }
        bool ShortIdentifierSpecified { set; get; }
        string ShortIdentifier { set; get; }
        bool Fixml_SecurityExchangeSpecified { set; get; }
        string Fixml_SecurityExchange { set; get; }
        //PL 20181001 [24211]
        bool RICCodeSpecified { set; get; }
        string RICCode { set; get; }
        bool BBGCodeSpecified { set; get; }
        string BBGCode { set; get; }

        bool ETDIdentifierFormatSpecified { set; get; }
        /// <summary>
        /// 
        /// </summary>
        /// FI 20220912 [XXXXX] Add
        string ETDIdentifierFormat { set; get; }

        #endregion Accessors
    }
    #endregion IMarketRepository
    
    /// <summary>
    /// 
    /// </summary>
    /// FI 20150603 [XXXXX] Modify
    public interface IBookRepository : ICommonRepository
    {
        #region Accessors
        bool IdCSpecified { set; get; }
        string IdC { get; set; }

        /// <summary>
        /// Représente le propriétaire du book
        /// </summary>
        // FI 20150603 [XXXXX] Add
        IBookOwnerRepository Owner { get; set; }

        #endregion Accessors
    }

    /// <summary>
    /// Représente le propriétaire d'un book
    /// </summary>
    /// FI 20150603 [XXXXX] Add
    /// FI 20150310 [XXXXX] Modify (suppression heritage de ISpheresId)
    public interface IBookOwnerRepository : IReference /*, ISpheresId*/
    {
    }

    /// <summary>
    /// Représente un acteur
    /// </summary>
    /// FI 20150603 [XXXXX] Add
    /// FI 20160530 [21885] Modify
    public interface IActorRepository : ICommonRepository
    {
        bool ISO10383_ALPHA4Specified { set; get; }
        /// <summary>
        /// Code MIC (http://www.iso15022.org/download/ISO10383_MIC.xls.)
        /// <para>Les Chambres de compensations possèdent un code MIC</para>
        /// </summary>
        /// FI 20160530 [21885] Add 
        string ISO10383_ALPHA4 { get; set; }

        bool ISO17442Specified { set; get; }
        /// <summary>
        /// Code LEI
        /// </summary>
        /// FI 20190515 [23912] Add
        string ISO17442 { get; set; }

        bool TelephoneNumberSpecified { set; get; }
        /// <summary>
        /// telephoneNumber
        /// </summary>
        /// FI 20190515 [23912] Add
        string TelephoneNumber { get; set; }

        bool MobileNumberSpecified { set; get; }
        /// <summary>
        /// mobileNumberNumber
        /// </summary>
        /// FI 20190515 [23912] Add
        string MobileNumber { get; set; }

        bool EmailSpecified { set; get; }
        /// <summary>
        /// email
        /// </summary>
        /// FI 20190515 [23912] Add
        string Email { get; set; }

        bool WebSpecified { set; get; }
        /// <summary>
        /// web
        /// </summary>
        /// FI 20190515 [23912] Add
        string Web { get; set; }

        bool AddressSpecified { set; get; }
        /// <summary>
        ///  Address de l'acteur 
        /// </summary>
        /// FI 20190515 [23912] Add
        IAddress Address { get; set; }
    }

    /// <summary>
    /// Représente un businessCenter
    /// </summary>
    /// FI 20150304 [XXPOC] Add
    public interface IBusinessCenterRepository : ICommonRepository
    {
    }

    #region IMarkToMarketEvent
    public interface IMarkToMarketEvent : IMidLifeEvent
    {
        #region Accessors
        ICollateralValuation Collateral { set; get; }
        EFS_Boolean CombinedInterestPayout { set; get; }
        bool TransferSpecified { set; get; }
        ITransfer Transfer { set; get; }
        #endregion Accessor
    }
    #endregion IMarkToMarketEvent
    #region IMidLifeEvent
    public interface IMidLifeEvent : IEvent
    {
        #region Accessors
        IAdjustedDate EventDate { set; get; }
        #endregion Accessors
    }
    #endregion IMidLifeEvent

    #region IBookRepository
    public interface ITradeLinkRepository : ICommonRepository
    {
        #region Accessors
        string Link { get; set; }
        int IdT { get; set; }
        bool ExecutionIdSpecified { set; get; }
        string ExecutionId { get; set; }
        int IdT_a { get; set; }
        string Identifier_a { get; set; }
        #endregion Accessors
    }
    #endregion IBookRepository

    #region INetInvoiceAmounts
    public interface INetInvoiceAmounts : ITripleInvoiceAmounts
    {
        #region Accessors
        bool TaxSpecified { set; get; }
        IInvoiceTax Tax { set; get; }
        #endregion Accessors
    }
    #endregion INetInvoiceAmounts
    #region INetTradeIdentifier
    public interface INetTradeIdentifier : IPartyTradeIdentifier
    {
        #region Accessors
        ITradeIdentifierList[] OriginalTradeIdentifier { set; get; }
        #endregion Accessors
    }
    #endregion INetTradeIdentifier

    #region IOrderPrice
    public interface IOrderPrice
    {
        #region Accessors
        IScheme PriceUnits { set; get; }
        //
        bool CleanPriceSpecified { set; get; }
        EFS_Decimal CleanPrice { set; get; }
        //
        bool DirtyPriceSpecified { set; get; }
        EFS_Decimal DirtyPrice { set; get; }
        //
        bool AccruedInterestRateSpecified { set; get; }
        /// <summary>
        /// Représente le taux du coupon couru
        /// </summary>
        EFS_Decimal AccruedInterestRate { set; get; }
        //
        bool AccruedInterestAmountSpecified { set; get; }
        /// <summary>
        /// Représente le montant du coupon couru
        /// </summary>
        IMoney AccruedInterestAmount { set; get; }
        #endregion
    }
    #endregion IOrderPrice
    #region IOrderQuantity
    public interface IOrderQuantity
    {
        #region Accessors
        OrderQuantityType3CodeEnum QuantityType { set; get; }
        //
        bool NumberOfUnitsSpecified { set; get; }
        EFS_Decimal NumberOfUnits { set; get; }
        //
        IMoney NotionalAmount { set; get; }
        #endregion
    }
    #endregion IOrderQuantity

    #region IPriceUnits
    public interface IPriceUnits : IScheme
    {
        #region Accessors
        bool ForcedSpecified { set; get; }
        bool Forced { set; get; }
        #endregion Accessor
    }
    #endregion IPriceUnits

    #region IPartyPayerReceiverReference
    public interface IPartyPayerReceiverReference
    {
        #region Accessors
        bool PayerPartyReferenceSpecified { set; get; }
        IReference PayerPartyReference { set; get; }
        bool ReceiverPartyReferenceSpecified { set; get; }
        IReference ReceiverPartyReference { set; get; }
        #endregion Accessors
    }
    #endregion IPartyPayerReceiverReference
    #region IPartySettlementTransferInformation
    public interface IPartySettlementTransferInformation
    {
        #region Accessors
        IReference PartyReference { set; get; }
        ISettlementTransferProcessingInformation ProcessingInformation { set; get; }
        #endregion Accessors
    }
    #endregion IPartySettlementTransferInformation
    #region IPayoutPeriod
    // EG 20150920 [21314] Int (int32) to Long (Int64)
    public interface IPayoutPeriod
    {
        #region Accessors
        PeriodEnum Period { get; }
        int PeriodMultiplier { get; }
        decimal Percentage { get; }
        #endregion Accessors
    }
    #endregion IPayoutPeriod

    #region IPosKeepingAsset
    public interface IPosKeepingAsset
    {
        #region Accessors
        int IdAsset { set; get; }
        string Identifier { set; get; }
        /// <summary>
        /// Représente la devise du nominal
        /// </summary>
        string NominalCurrency { set; get; }
        /// <summary>
        /// Représente la devise de prix de l'asset
        /// <para>Attention lorsque cette devise n'est pas cotée, car les flux découlant du prix doivent être en devise cotée</para>
        /// </summary>
        string PriceCurrency { set; get; }
        /// <summary>
        /// Représente la devise master de l'asset 
        /// <para>currency est équivalent à priceCurrency si priceCurrency est cotée,</para>
        /// <para>sinon est équivalent à la devise cotée de priceCurrency</para>
        /// </summary>
        string Currency { set; get; }
        decimal ContractMultiplier { set; get; }
        string IdBC { set; get; }
        IPosKeepingQuote Quote { set; get; }
        bool QuoteSpecified { set; get; }
        IPosKeepingQuote QuoteReference { set; get; }
        bool QuoteReferenceSpecified { set; get; }
        int IdM { set; get; }
        bool IdMSpecified { set; get; }
        #endregion Accessors

    }
    #endregion IPosKeepingAsset
    #region IPosKeepingAsset_ETD
    /// <summary>
    /// 
    /// </summary>
    /// FI 20110816 [17548] ajout de nominalCurrency, priceCurrency  
    /// EG 20130603 Ticket: 18721
    /// PM 20130807 [18876] Ajout priceQuoteMethod
    /// EG 20140120 Report 3.7
    /// PM 20140807 [20273][20106] Ajout de roundDir et roundPrec
    /// PM 20141120 [20508] Ajout de cashFlowCalculationMethod
    /// EG 20170206 [22787] Ajout colonnes (MATURITY|MATURITYRULE) pour gestion des périodes de livraison
    public interface IPosKeepingAsset_ETD : IPosKeepingAsset
    {
        #region Accessors
        DateTime MaturityDate { set; get; }
        DateTime MaturityDateSys { set; get; }
        bool PrecedingMaturityDateSysSpecified { set; get; }
        DateTime PrecedingMaturityDateSys { set; get; }
        DateTime LastTradingDay { set; get; }
        FinalSettlementPriceEnum FinalSettlementPrice { set; get; }
        CfiCodeCategoryEnum Category { set; get; }
        bool ExerciseStyleSpecified { set; get; }
        DerivativeExerciseStyleEnum ExerciseStyle { set; get; }
        /// <summary>
        /// Représente la valeur du sous jacent du DerivativeContract 
        /// </summary>
        decimal Nominal { set; get; }
        decimal StrikePrice { set; get; }
        int InstrumentNum { set; get; }
        int InstrumentDen { set; get; }
        SettlMethodEnum SettlMethod { set; get; }
        PutOrCallEnum PutCall { set; get; }
        bool PutCallSpecified { set; get; }
        Cst.UnderlyingAsset AssetCategory { set; get; }
        bool AssetCategorySpecified { set; get; }
        decimal Factor { set; get; }
        int IdAsset_Underlyer { set; get; }
        bool IdAsset_UnderlyerSpecified { set; get; }
        string Identifier_Underlyer { set; get; }
        bool Identifier_UnderlyerSpecified { set; get; }
        int IdDC_Underlyer { set; get; }
        bool IdDC_UnderlyerSpecified { set; get; }
        bool DeliveryDateSpecified { set; get; }
        DateTime DeliveryDate { set; get; }
        bool DeliveryDelayOffsetSpecified { set; get; }
        IOffset DeliveryDelayOffset { set; get; }
        int InstrumentNum_Underlyer { set; get; }
        int InstrumentDen_Underlyer { set; get; }
        PriceQuoteMethodEnum PriceQuoteMethod { set; get; }
        bool FinalSettlementSideSpecified { set; get; }
        QuotationSideEnum FinalSettlementSide { set; get; }
        bool FinalSettlementTimeSpecified { set; get; }
        string FinalSettlementTime { set; get; }
        FuturesValuationMethodEnum ValuationMethod { set; get; }
        string RoundDir { set; get; }
        int RoundPrec { set; get; }
        CashFlowCalculationMethodEnum CashFlowCalculationMethod { set; get; }
        bool UnitOfMeasureSpecified { set; get; }
        string UnitOfMeasure { set; get; }
        decimal UnitOfMeasureQty { set; get; }
        bool FirstDeliveryDateSpecified { set; get; }
        DateTime FirstDeliveryDate { set; get; }
        bool LastDeliveryDateSpecified { set; get; }
        DateTime LastDeliveryDate { set; get; }
        bool DeliveryTimeStartSpecified { set; get; }
        IPrevailingTime DeliveryTimeStart { set; get; }
        bool DeliveryTimeEndSpecified { set; get; }
        IPrevailingTime DeliveryTimeEnd { set; get; }
        bool IsApplySummertime { set; get; }
        bool DeliveryPeriodFrequencySpecified { set; get; }
        ICalculationPeriodFrequency DeliveryPeriodFrequency { set; get; }
        bool DayTypeDeliverySpecified { set; get; }
        DayTypeEnum DayTypeDelivery { set; get; }
        bool DeliverySettlementOffsetSpecified { set; get; }
        IOffset DeliverySettlementOffset { set; get; }
        bool SettlementOfHolidayDeliveryConventionSpecified { set; get; }
        SettlementOfHolidayDeliveryConventionEnum SettlementOfHolidayDeliveryConvention { set; get; }
        bool FirstDeliverySettlementDateSpecified { set; get; }
        DateTime FirstDeliverySettlementDate { set; get; }
        bool LastDeliverySettlementDateSpecified { set; get; }
        DateTime LastDeliverySettlementDate { set; get; }
        #endregion Accessors
        #region Method
        DateTime GetOfficialCloseQuoteTime(DateTime pDateReference);
        #endregion Method
    }
    #endregion IPosKeepingAsset_ETD
    #region IPosKeepingCommon
    public interface IPosKeepingCommon : IPosKeepingKey
    {
        #region Accessors
        PosKeepingAsset Asset { set; get; }
        bool TradeSpecified { set; get; }
        IPosKeepingTrade Trade { set; get; }
        bool MarketSpecified { set; get; }
        IPosKeepingMarket Market { set; get; }
        SQL_Product Product { set; get; }
        SQL_Instrument Instrument { set; get; }
        #endregion Accessors
    }
    #endregion IPosKeepingCommon
    #region IPosKeepingData
    // EG 20140120 Report 3.7
    // EG 20140311 [19734][19702] Add isClearingEOD_BookDealer
    // EG 20141128 [20520] Nullable<decimal>
    // EG 20150716 [21103] Add pDtSettlt to SetTrade method
    // EG 20180307 [23769] Gestion dbTransaction
    public interface IPosKeepingData : IPosKeepingCommon
    {
        #region Members
        string PositionEffect_BookDealer { set; get; }
        bool IsIgnorePositionEffect_BookDealer { set; get; }
        bool IsClearingEOD_BookDealer { set; get; }
        // EG 20240115 [WI808] Traitement EOD : Harmonisation et réunification des méthodes
        (int id, string identifier, Cst.UnderlyingAsset underlyer) AssetInfo { get; }
        #endregion Members

        #region Methods
        int IdA_Issuer { get; }
        int IdB_Issuer { get; }
        // EG 20150920 [21374] Int (int32) to Long (Int64) 
        // EG 20170127 Qty Long To Decimal
        decimal NominalValue(decimal pQuantity);
        // EG 20150920 [21374] Int (int32) to Long (Int64) 
        // EG 20151001 [21414] Add Nullable to ClosingPrice and return 
        //decimal CashSettlement(decimal pStrikePrice, decimal pClosingPrice, long pQuantity);
        // EG 20170127 Qty Long To Decimal
        Nullable<decimal> CashSettlement(decimal pStrikePrice, Nullable<decimal> pClosingPrice, decimal pQuantity);
        // EG 20141128 [20520] Nullable<decimal>
        // EG 20150920 [21374] Int (int32) to Long (Int64) 
        // EG 20170127 Qty Long To Decimal
        Nullable<decimal> RealizedMargin(Nullable<decimal> pClosedPrice, Nullable<decimal> pClosingPrice, decimal pQuantity);
        // EG 20141128 [20520] Nullable<decimal>
        // EG 20150920 [21374] Int (int32) to Long (Int64) 
        // EG 20170127 Qty Long To Decimal
        Nullable<decimal> VariationMargin(Nullable<decimal> pPrice, Nullable<decimal> pPriceVeil, decimal pQuantity);
        void Set(int pIdA_EntityDealer, int pIdA_EntityClearer);
        //PM 20150422 [20575] Add pDtEntityPrev, pDtEntity, pDtEntityNext
        void SetMarket(int pIDA_Entity, int pIDA_CSS, int pIdEM, int pIdM, string pIdBC, DateTime pDtMarketPrev, DateTime pDtMarket, DateTime pDtMarketNext, DateTime pDtEntityPrev, DateTime pDtEntity, DateTime pDtEntityNext);
        // EG 20150716 [21103] Add pDtSettlt
        // EG 20150920 [21374] Int (int32) to Long (Int64) 
        // EG 20170127 Qty Long To Decimal
        // EG 20171025 [23509] Upd dtExecution replace dtTimestamp
        void SetTrade(int pIdT, string pIdentifier, int pSide, decimal pQty, DateTime pDtBusiness, string pPositionEffect, DateTime pDtExecution, DateTime pDtSettlt);
        // EG 20180307 [23769] Gestion dbTransaction
        void SetPosKey(string pCS, IDbTransaction pDbTransaction, int pIdI, Nullable<Cst.UnderlyingAsset> pUnderlyingAsset, int pIdAsset, int pIdA_Dealer, int pIdB_Dealer, int pIdA_Clearer, int pIdB_Clearer);
        void SetAdditionalInfo(int pIdA_EntityDealer, int pIdA_EntityClearer);
        // EG 20180307 [23769] Gestion dbTransaction
        void SetBookDealerInfo(string pCS, IDbTransaction pDbTransaction, Nullable<Cst.UnderlyingAsset> pUnderlyingAsset, DateTime pDtEnabled);
        void SetQuote(Cst.UnderlyingAsset pCategory, Quote pQuote, string pSource);
        void SetQuoteReference(Cst.UnderlyingAsset pCategory, Quote pQuote, string pSource);
        decimal ToBase100(decimal pValue);
        Nullable<decimal> ToBase100(Nullable<decimal> pValue);
        // EG 20120503 new (Merde)
        decimal ToBase100_UNL(decimal pValue);
        Nullable<decimal> ToBase100_UNL(Nullable<decimal> pValue);
        // PM 20130807 [18876] alignement avec ToBase100 et Tobase100_UNL for Variable Tick Value
        decimal VariableContractValue(decimal pPrice);
        decimal? VariableContractValue(decimal? pPrice);
        decimal VariableContractValue_UNL(decimal pPrice);
        decimal? VariableContractValue_UNL(decimal? pPrice);

        string Message { get; }
        #endregion Methods
    }
    #endregion IPosKeepingData
    #region IPosKeepingKey
    public interface IPosKeepingKey
    {
        #region Accessors
        int IdI { set; get; }
        Nullable<Cst.UnderlyingAsset> UnderlyingAsset { set; get; }
        int IdAsset { set; get; }
        int IdA_Dealer { set; get; }
        int IdB_Dealer { set; get; }
        int IdA_Clearer { set; get; }
        int IdB_Clearer { set; get; }
        int IdA_EntityDealer { set; get; }
        int IdA_EntityClearer { set; get; }
        string LockObjectId { get; }
        #endregion Accessors
    }
    #endregion IPosKeepingKey
    #region IPosKeepingMarket
    public interface IPosKeepingMarket
    {
        #region Accessors
        int IdA_Entity { set; get; }
        int IdA_CSS { set; get; }
        int IdA_Custodian { set; get; }
        bool IdA_CustodianSpecified { set; get; }
        int IdEM { set; get; }
        int IdM { set; get; }
        string IdBC { set; get; }
        DateTime DtMarketPrev { set; get; }
        DateTime DtMarket { set; get; }
        DateTime DtMarketNext { set; get; }
        //PM 20150422 [20575] Add dtEntityPrev, dtEntity, dtEntityNext
        DateTime DtEntityPrev { set; get; }
        DateTime DtEntity { set; get; }
        DateTime DtEntityNext { set; get; }

        // EG 20240520 [WI930] New IdA_CSS_Identifier, IdM_Identifier, IdBCEntity
        string IdA_CSS_Identifier { set; get; }
        string IdM_Identifier { set; get; }
        string IdBCEntity { set; get; }
        #endregion Accessors
    }
    #endregion IPosKeepingMarket
    #region IPosKeepingQuote
    // EG 20140116 [19456]
    public interface IPosKeepingQuote
    {
        #region Accessors
        Cst.UnderlyingAsset Category { set; get; }
        int IdAsset { set; get; }
        string Identifier { set; get; }
        DateTime QuoteTime { set; get; }
        QuoteTimingEnum QuoteTiming { set; get; }
        QuotationSideEnum QuoteSide { set; get; }
        decimal QuotePrice { set; get; }
        string Source { set; get; }
        #endregion Accessors
    }
    #endregion IPosKeepingQuote

    #region IPosKeepingTrade
    // EG 20150716 [21103] Add dtSettlt
    // EG 20150920 [21374] Int (int32) to Long (Int64) 
    // EG 20170127 Qty Long To Decimal
    // EG 20171025 [23509] Upd dtExecution replace dtTimestamp
    public interface IPosKeepingTrade
    {
        #region Accessors
        int IdT { set; get; }
        string Identifier { set; get; }
        decimal Qty { set; get; }
        int Side { set; get; }
        DateTime DtBusiness { set; get; }
        DateTime DtSettlt { set; get; }
        DateTime DtExecution { set; get; }
        string PositionEffect { set; get; }
        string PositionEffect_BookDealer { set; get; }
        bool IsIgnorePositionEffect_BookDealer { set; get; }
        string UltimatelyPositionEffect { get; }
        #endregion Accessors
    }
    #endregion IPosKeepingTrade
    #region IPosKeepingClearingTrade
    // EG 20150920 [21374] Int (int32) to Long (Int64) 
    // EG 20170127 Qty Long To Decimal
    // EG 20171025 [23509] Upd dtExecution replace dtTimestamp
    public interface IPosKeepingClearingTrade
    {
        #region Accessors
        int IdT { set; get; }
        string Identifier { set; get; }
        decimal AvailableQty { set; get; }
        decimal ClosableQty { set; get; }
        DateTime DtBusiness { set; get; }
        DateTime DtExecution { set; get; }
        #endregion Accessors
    }
    #endregion IPosKeepingClearingTrade

    #region IPosRequest
    /// <summary>
    /// Représente une demande d'action sur postion
    /// </summary>
    /// FI 20130319 [18467] add source
    /// FI 20130327 [18467] add extlLink
    /// FI 20130327 [18467] add sourceIdProcessL
    /// EG 20150317 [POC] add gProduct and GetMQueueIdInfo without parameter
    // EG 20150920 [21374] Int (int32) to Long (Int64)  
    // EG 20170127 Qty Long To Decimal
    public interface IPosRequest
    {
        #region Accessors
        int IdPR { set; get; }
        int IdPR_PosRequest { set; get; }
        bool IdPR_PosRequestSpecified { set; get; }
        Cst.PosRequestTypeEnum RequestType { set; get; }
        SettlSessIDEnum RequestMode { set; get; }
        int IdA_Entity { set; get; }
        int IdA_CssCustodian { set; get; }
        int IdA_Css { set; get; }
        bool IdA_CssSpecified { set; get; }
        int IdA_Custodian { set; get; }
        bool IdA_CustodianSpecified { set; get; }
        int IdEM { set; get; }
        bool IdEMSpecified { set; get; }
        int IdM { set; get; }
        bool IdMSpecified { set; get; }
        int IdCE { set; get; }
        bool IdCESpecified { set; get; }
        DateTime DtBusiness { set; get; }
        IPosKeepingKey PosKeepingKey { set; get; }
        bool PosKeepingKeySpecified { set; get; }
        int IdT { set; get; }
        bool IdTSpecified { set; get; }
        decimal Qty { set; get; }
        bool QtySpecified { set; get; }
        string Notes { set; get; }
        bool NotesSpecified { set; get; }
        IPosRequestKeyIdentifier Identifiers { set; get; }
        bool IdentifiersSpecified { set; get; }
        object DetailBase { set; get; }
        int IdAIns { set; get; }
        DateTime DtIns { set; get; }
        int IdAUpd { set; get; }
        bool IdAUpdSpecified { set; get; }
        DateTime DtUpd { set; get; }
        bool DtUpdSpecified { set; get; }
        ProcessStateTools.StatusEnum Status { set; get; }
        bool StatusSpecified { set; get; }
        /// <summary>
        /// Application à l'origine de la demande
        /// </summary>
        string Source { set; get; }
        bool SourceSpecified { set; get; }
        /// <summary>
        /// Log IO à l'origine de la demande importée
        /// <para>Utilisé dans le cadre de l'importation des POSREQUEST</para>
        /// </summary>
        int SourceIdProcessL { set; get; }
        bool SourceIdProcessLSpecified { set; get; }
        /// <summary>
        /// Identifiant externe de la demande 
        /// <para>Utilisé dans le cadre de l'importation des POSREQUEST</para>
        /// </summary>
        string ExtlLink { set; get; }
        bool ExtlLinkSpecified { set; get; }
        /// <summary>
        /// Dernier process à avoir traité la demande POSREQUEST
        /// </summary>
        int IdProcessL { set; get; }
        bool IdProcessLSpecified { set; get; }

        Type DetailType { get; }
        int NbTokenIdE { get; }
        string RequestMessage { get; }
        // EG 20130523 New LockMode
        string LockModeEntityMarket { get; }
        string LockModeTrade { get; }
        /// EG 20150317 [POC] add gProduct
        string GProduct { get; }
        /// EG 20170109 GProductValue
        ProductTools.GroupProductEnum GroupProduct { set; get; }
        bool GroupProductSpecified { set; get; }
        string GroupProductValue { get; }
        Nullable<ProductTools.GroupProductEnum> GroupProductEnum { get; }
        // EG 20240115 [WI808] Traitement EOD : Harmonisation et réunification des méthodes
        (decimal qty, decimal availableQty, IPosRequestDetail detail) GetDetailForAction { get; }
        #endregion

        #region Method
        // EG 20171128 [23331]
        object CloneMain();
        void SetIdA_CssCustodian(int pIdA_CssCustodian, bool pIsCustodian);
        void SetIdA_CssCustodian(Nullable<int> pIdA_Css, Nullable<int> pIdA_Custodian);
        void SetPosKey(int pIdI, Nullable<Cst.UnderlyingAsset> pUnderlyingAsset, int pIdAsset, int pIdA_Dealer, int pIdB_Dealer, int pIdA_Clearer, int pIdB_Clearer);

        void SetAdditionalInfo(int pIdA_EntityDealer, int pIdA_EntityClearer);

        void SetIdentifiers(string pMarket, string pInstrument, string pAsset, string pDealer, string pBookDealer, string pClearer, string pBookClearer);
        void SetIdentifiers(string pEntity, string pCssCustodian, string pMarket,
            string pInstrument, string pAsset, string pDealer, string pBookDealer, string pClearer, string pBookClearer);
        void SetIdentifiers(string pEntity, string pCssCustodian);
        void SetIdentifiers(string pEntity, string pCssCustodian, string pMarket);
        void SetIdentifiers(string pTrade);
        // EG 20151102 [21465] New
        void SetSource(AppInstance pAppInstance);
        void SetNotes(string pNotes);
        /// EG 20150317 [POC] GetMQueueIdInfo without parameter
        IdInfo GetMQueueIdInfo();

        IPosRequestTradeAdditionalInfo CreateAdditionalInfoStandard(DataDocumentContainer pTemplateDataDocument, SQL_TradeCommon pSQLTrade,
            SQL_Product pSQLProduct, SQL_Instrument pSQLInstrument, string pScreenName);
        /// EG 20160302 (21969] New
        void SetStatus(Cst.ErrLevel pErrLevel);
        #endregion
    }
    #endregion IPosRequest
    #region IPosRequestClearingBLK
    public interface IPosRequestClearingBLK : IPosRequest
    {
        #region Accessors
        IPosRequestDetClearingBLK Detail { set; get; }
        void SetDetail(decimal pAvailableQtyBuy, decimal pAvailableQtySell);
        #endregion Accessors
    }
    #endregion IPosRequestClearingBLK
    #region IPosRequestClearingEOD
    public interface IPosRequestClearingEOD : IPosRequestClearingBLK
    {
    }
    #endregion IPosRequestClearingEOD
    #region IPosRequestClearingSPEC
    public interface IPosRequestClearingSPEC : IPosRequest
    {
        #region Accessors
        IPosRequestDetClearingSPEC Detail { set; get; }
        #endregion Accessors
    }
    #endregion IPosRequestClearingSPEC
    #region IPosRequestClosingDAY
    public interface IPosRequestClosingDAY : IPosRequest
    {
    }
    #endregion IPosRequestClosingDAY
    #region IPosRequestClosingDayControl
    public interface IPosRequestClosingDayControl : IPosRequest
    {
    }
    #endregion IPosRequestClosingDayControl
    #region IPosRequestCorporateAction
    // EG 20130417
    // EG 20171107 [23509] SetDetail change (DateTime pEffectiveDate)
    public interface IPosRequestCorporateAction : IPosRequest
    {
        #region Accessors
        object Clone();
        IPosRequestDetCorporateAction Detail { set; get; }
        void SetDetail(int pIdDCEx, string pIdentifierDCEx, string pContractSymbolDCEx, DateTime pEffectiveDate);
        #endregion Accessors
    }
    #endregion IPosRequestCorporateAction
    #region IPosRequestCorrection
    public interface IPosRequestCorrection : IPosRequest
    {
        #region Accessors
        IPosRequestDetCorrection Detail { set; get; }
        // EG 20150907 [21317] Add initialQty
        // EG 20150920 [21374] Int (int32) to Long (Int64) 
        // EG 20170127 Qty Long To Decimal
        void SetDetail(decimal pInitialQty, decimal pAvailableQty, IPayment[] pPaymentFees, EFS_Boolean pIsSafekeepingReversal);
        #endregion Accessors
    }
    #endregion IPosRequestCorrection

    #region IPosRequestPositionDocument
    /// <summary>
    ///  Représente un document qui contient les caractéristiques d'une demande d'action sur position
    ///  <para>la demande s'applique uniquement sur une position </para>
    ///  <para>la demande ne s'applique pas à un trade</para>
    /// </summary>
    public interface IPosRequestPositionDocument : IEfsDocument
    {
        /// <summary>
        /// Représente l'action
        /// </summary>
        Cst.PosRequestTypeEnum RequestType { set; get; }

        /// <summary>
        /// EOD,ITD
        /// </summary>
        SettlSessIDEnum RequestMode { set; get; }

        /// <summary>
        /// Représente la date de clearing de l'action
        /// </summary>
        EFS_Date ClearingBusinessDate { set; get; }

        /// <summary>
        /// Représente l'entité du dealer
        /// </summary>
        IActorId ActorEntity { set; get; }
        /// <summary>
        /// Représente le dealer
        /// <para>Le dealer n'est pas tjs renseigné (cas des assignations)</para>
        /// </summary>
        bool ActorDealerSpecified { set; get; }
        IActorId ActorDealer { set; get; }

        /// <summary>
        /// Représente le book dealer
        /// <para>Le dealer n'est pas tjs renseigné (cas des assignations)</para>
        /// </summary>
        bool BookDealerSpecified { set; get; }
        IBookId BookDealer { set; get; }

        /// <summary>
        /// Représente le clearer (une chambre ou un broker de compensation)
        /// </summary>
        IActorId ActorClearer { set; get; }
        /// <summary>
        /// Représente le book du clearer 
        /// <para>Si le clearer est une chambre, Représente un compartiment</para>
        /// </summary>
        IBookId BookClearer { set; get; }

        /// <summary>
        /// Représente l'asset
        /// </summary>
        IFixInstrument Instrmt { set; get; }

        /// <summary>
        /// Représente la quantité demandée
        /// </summary>
        // EG 20170127 Qty Long To Decimal
        EFS_Decimal Qty { set; get; }

        /// <summary>
        /// 
        /// </summary>
        bool NotesSpecified { set; get; }
        EFS_String Notes { set; get; }

        /// <summary>
        /// Flag qui permet l'exécution partielle de la damnde
        /// </summary>
        EFS_Boolean IsPartialExecutionAllowed { set; get; }
        /// <summary>
        /// Flag qui permet le calcul des frais sur les trades impliqués par l'action
        /// </summary>
        EFS_Boolean IsFeeCalculation { set; get; }
        /// <summary>
        /// Flag utilisée uniquement pour les exercices sur poitions options
        /// </summary>
        EFS_Boolean IsAbandonRemainingQty { set; get; }
        /// <summary>
        /// 
        /// </summary>
        IPayment[] PaymentFees { set; get; }

    }
    #endregion IPosRequestPositionDocument
    #region IPosRequestEntry
    public interface IPosRequestEntry : IPosRequest
    {
        #region Accessors
        IPosRequestDetEntry Detail { set; get; }
        void SetDetail(IPosKeepingTrade pTrade, PosKeepingEntryMQueue pMessage);
        #endregion Accessors
    }
    #endregion IPosRequestEntry
    #region IPosRequestEOD
    public interface IPosRequestEOD : IPosRequest
    {
    }
    #endregion IPosRequestEOD
    #region IPosRequestREMOVEEOD
    public interface IPosRequestREMOVEEOD : IPosRequest
    {
    }
    #endregion IPosRequestREMOVEEOD

    #region IPosRequestGroupLevel
    public interface IPosRequestGroupLevel : IPosRequest
    {
    }
    #endregion IPosRequestGroupLevel


    #region IPosRequestKeyIdentifier
    /// EG 20130607 [18740] Add corporateAction (RemoveCAExecuted)
    public interface IPosRequestKeyIdentifier
    {
        #region Accessors
        string Entity { set; get; }
        string CssCustodian { set; get; }
        string Market { set; get; }
        string Instrument { set; get; }
        string Asset { set; get; }
        string Trade { set; get; }
        string Dealer { set; get; }
        string BookDealer { set; get; }
        string Clearer { set; get; }
        string BookClearer { set; get; }
        string CorporateAction { set; get; }
        #endregion Accessors
    }
    #endregion IPosRequestKeyIdentifier
    #region IPosRequestMaturityOffsetting
    public interface IPosRequestMaturityOffsetting : IPosRequest
    {
    }
    #endregion IPosRequestMaturityOffsetting
    #region IPosRequestPhysicalPeriodicDelivery
    // EG 20170206 [22787] New
    public interface IPosRequestPhysicalPeriodicDelivery : IPosRequest
    {
    }
    #endregion IPosRequestPhysicalPeriodicDelivery


    #region IPosRequestCascadingShifting
    // PM 20130213 [18414] & PM 20130307 [18434]
    // EG 20171003 [23452] SetDetail change (DateTime pDtExecution)
    public interface IPosRequestCascadingShifting : IPosRequestMaturityOffsetting
    {
        #region Accessors
        object Clone();
        IPosRequestDetCascadingShifting Detail { set; get; }
        void SetDetail(int pIdDCDest, string pIdentifierDCDest, string pMaturityDest, DateTime pDtExecution);
        #endregion Accessors
    }
    #endregion IPosRequestCascadingShifting
    #region IPosRequestOption
    // EG 20160121 [21805] POC-MUREX
    public interface IPosRequestOption : IPosRequest
    {
        #region Accessors

        IPosRequestDetOption Detail { set; get; }
        int IdTOption { set; get; }
        bool IdTOptionSpecified { set; get; }
        object Clone();
        // EG 20150920 [21374] Int (int32) to Long (Int64) 
        // EG 20151019 [21465] Add Nullable on parameters pQuoteTiming|pPrice|pDtPrice, Add Nullable<bool> pIsFeeCalculation
        // EG 20170127 Qty Long To Decimal
        void SetDetail(string pCS, decimal pAvailableQty, decimal pStrikePrice,
            Cst.UnderlyingAsset pAssetCategory, int pIdAsset, string pIdentifier, Nullable<QuoteTimingEnum> pQuoteTiming,
            Nullable<decimal> pPrice, Nullable<DateTime> pDtPrice, string pSource, IPayment[] pPaymentFees, Nullable<bool> pIsFeeCalculation);
        #endregion Accessors
    }
    #endregion IPosRequestOption

    #region IPosRequestRemoveCAExecuted
    /// EG 20130607 [18740] Add RemoveCAExecuted
    public interface IPosRequestRemoveCAExecuted : IPosRequest
    {
    }
    #endregion IPosRequestRemoveCAExecuted


    #region IPosRequestPositionOption
    /// <summary>
    ///  Représente une requête POSREQUEST de denouement d'option sur position 
    /// </summary>
    // FI 20130315 [18467] add interface
    public interface IPosRequestPositionOption : IPosRequest
    {
        IPosRequestDetPositionOption Detail { set; get; }
        object Clone();
        void SetDetail(bool pIsPartialExecutionAllowed, bool pIsFeeCalculation, bool pIsAbandonRemainingQty, IPayment[] pPaymentFees);
    }
    #endregion IPosRequestPositionOption

    #region IPosRequestRemoveAlloc
    // EG 20170127 Qty Long To Decimal
    public interface IPosRequestRemoveAlloc : IPosRequest
    {
        #region Accessors
        object Clone();
        IPosRequestDetRemoveAlloc Detail { set; get; }
        // EG 20150920 [21374] Int (int32) to Long (Int64) 
        // EG 20170127 Qty Long To Decimal
        void SetDetail(string pCS, decimal pInitialQty, decimal pPositionQty);
        #endregion Accessors
    }
    #endregion IPosRequestRemoveAlloc

    #region IPosRequestSplit
    public interface IPosRequestSplit : IPosRequest
    {
        #region Accessors
        object Clone();
        IPosRequestDetSplit Detail { set; get; }
        void SetDetail(string pCS, ArrayList pNewTrades);
        #endregion Accessors
    }
    #endregion IPosRequestSplit

    #region IPosRequestTransfer
    /// EG 20141210 [20554] Add Nullable to pIdTReplace
    /// EG 20150716 (21103]
    public interface IPosRequestTransfer : IPosRequest
    {
        #region Accessors
        object Clone();
        IPosRequestDetTransfer Detail { set; get; }
        /// EG 20150716 (21103]
        // EG 20150920 [21374] Int (int32) to Long (Int64)  
        // EG 20170127 Qty Long To Decimal
        void SetDetail(string pCS, decimal pInitialQty, decimal pPositionQty, IPayment[] pPaymentFees, int pIdTReplace, EFS_Boolean pIsReversalSafekeeping);
        /// EG 20150716 (21103]
        // EG 20150920 [21374] Int (int32) to Long (Int64)  
        // EG 20170127 Qty Long To Decimal
        void SetDetail(string pCS, decimal pInitialQty, decimal pPositionQty, Pair<IParty, IBookId> pTransferDealer, Pair<IParty, IBookId> pTransferClearer,
            Pair<bool, bool> pTransferInfo, EFS_Boolean pIsReversalSafekeeping);

        #endregion Accessors
    }
    #endregion IPosRequestTransfer

    #region IPosRequestUnclearing
    public interface IPosRequestUnclearing : IPosRequest
    {
        #region Accessors
        IPosRequestDetUnclearing Detail { set; get; }
        #endregion Accessors
    }
    #endregion IPosRequestUnclearing

    #region IPosRequestUpdateEntry
    public interface IPosRequestUpdateEntry : IPosRequest
    {
        #region Accessors
        IPosRequestDetUpdateEntry Detail { set; get; }
        void SetDetail(IPosKeepingTrade pTrade);
        #endregion Accessors
    }
    #endregion IPosRequestUpdateEntry


    #region IPosRequestDetail
    // EG 20150716 [21103] Add isReversalSafekeeping
    // EG 20151019 [21465] Add feeCalculationSpecified|feeCalculation
    public interface IPosRequestDetail
    {
        #region Accessors
        /// <summary>
        /// Représente la version du document
        /// </summary>
        EfsMLDocumentVersionEnum EfsMLversion { set; get; }
        int NbAdditionalEvents { set; get; }

        bool PaymentFeesSpecified { set; get; }
        /// <summary>
        /// Représente les frais 
        /// </summary>
        IPayment[] PaymentFees { get; set; }

        bool IsReversalSafekeepingSpecified { set; get; }
        bool IsReversalSafekeeping { set; get; }

        bool FeeCalculationSpecified { set; get; }
        bool FeeCalculation { set; get; }


        #region Method
        /// <summary>
        /// Affectation des frais
        /// </summary>
        /// <param name="pPaymentFees"></param>
        void SetPaymentFees(IPayment[] pPaymentFees);
        #endregion Method

        #endregion Accessors
    }
    #endregion IPosRequestDetail

    #region IPosRequestDetCorporateAction
    // EG 20130417
    // EG 20140516 Add contractSymbolDCEx
    public interface IPosRequestDetCorporateAction : IPosRequestDetail
    {
        #region Accessors
        int IdDCEx { set; get; }
        string IdentifierDCEx { set; get; }
        string ContractSymbolDCEx { set; get; }
        DataDocumentContainer DataDocument { set; get; }
        DateTime BusinessDate { set; get; }
        IPosRequestTradeAdditionalInfo CreateAdditionalInfo(DataDocumentContainer pTemplateDataDocument, SQL_TradeCommon pSQLTrade,
            SQL_Product pSQLProduct, SQL_Instrument pSQLInstrument, string pScreenName);
        #endregion Accessors
    }
    #endregion IPosRequestDetCorporateAction

    #region IPosRequestDetCascadingShifting
    // PM 20130219 [18414] & PM 20130307 [18434]
    // EG 20171107 [23509] DateTime dtExecution
    public interface IPosRequestDetCascadingShifting : IPosRequestDetail
    {
        #region Accessors
        int IdDCDest { set; get; }
        string IdentifierDCDest { set; get; }
        string MaturityMonthYearDest { set; get; }
        DataDocumentContainer DataDocument { set; get; }
        DateTime DtExecution { set; get; }
        IPosRequestTradeAdditionalInfo CreateAdditionalInfo(DataDocumentContainer pTemplateDataDocument, SQL_TradeCommon pSQLTrade,
            SQL_Product pSQLProduct, SQL_Instrument pSQLInstrument, string pScreenName);
        #endregion Accessors
    }
    #endregion IPosRequestDetCascadingShifting
    #region IPosRequestDetClearingBLK
    // EG 20150920 [21374] Int (int32) to Long (Int64) 
    // EG 20170127 Qty Long To Decimal
    public interface IPosRequestDetClearingBLK : IPosRequestDetail
    {
        #region Accessors
        decimal AvailableQtyBuy { set; get; }
        decimal AvailableQtySell { set; get; }
        #endregion Accessors
    }
    #endregion IPosRequestDetClearingBLK
    #region IPosRequestDetClearingSPEC
    public interface IPosRequestDetClearingSPEC : IPosRequestDetail
    {
        #region Accessors
        IPosKeepingClearingTrade[] TradesTarget { set; get; }
        #endregion Accessors
    }
    #endregion IPosRequestDetClearingSPEC
    #region IPosRequestDetCorrection
    // EG 20150920 [21374] Int (int32) to Long (Int64) 
    // EG 20170127 Qty Long To Decimal
    public interface IPosRequestDetCorrection : IPosRequestDetail
    {
        #region Accessors
        decimal InitialQty { set; get; }
        decimal AvailableQty { set; get; }
        #endregion Accessors
    }
    #endregion IPosRequestDetCorrection
    #region IPosRequestDetEntry
    // EG 20150920 [21374] Int (int32) to Long (Int64)
    // EG 20170127 Qty Long To Decimal
    // EG 20171025 [23509] Upd dtExecution replace timestamp
    public interface IPosRequestDetEntry : IPosRequestDetail
    {
        #region Accessors
        DateTime DtBusiness { set; get; }
        int Side { set; get; }
        decimal Qty { set; get; }
        DateTime DtExecution { set; get; }
        string PositionEffect { set; get; }
        PosKeepingEntryMQueue Message { set; get; }
        #endregion Accessors
    }
    #endregion IPosRequestDetEntry
    #region IPosRequestDetOption
    /// <summary>
    /// Détail d'un denouement d'option sur trade
    /// </summary>
    // EG 20150920 [21374] Int (int32) to Long (Int64) 
    // EG 20170127 Qty Long To Decimal
    public interface IPosRequestDetOption : IPosRequestDetail
    {
        #region Accessors
        decimal AvailableQty { set; get; }
        decimal StrikePrice { set; get; }

        bool UnderlyerSpecified { set; get; }
        IPosRequestDetUnderlyer Underlyer { get; set; }
        decimal GetTotalQtySource();
        #endregion Accessors

        #region Methods
        // EG 20151019 [21465] Add Nullable on parameters pQuoteTiming|pPrice|pDtPrice
        void SetUnderlyer(string pCS, Cst.PosRequestTypeEnum pRequestTypeOption, int pIdTOption,
            Cst.UnderlyingAsset pAssetCategory, int pIdAsset, string pIdentifier, Nullable<QuoteTimingEnum> pQuoteTiming,
            Nullable<decimal> pPrice, Nullable<DateTime> pDtPrice, string pSource);
        #endregion Methods
    }
    #endregion IPosRequestDetOption

    #region IPosRequestDetPositionOption
    /// <summary>
    /// Détail d'un denouement d'option sur position
    /// <para>Contient les directives nécessaires pour mettre en place des denouments d'option par Trade</para>
    /// <para>Contient les frais applicablés globalement sur la position (FI 20130315 Aucune gestion de ces frais pour l'instant)</para>
    /// </summary>
    /// FI 20130315 [18467] add interface IPosRequestDetPositionOption
    public interface IPosRequestDetPositionOption : IPosRequestDetail
    {
        /// <summary>
        /// Obtient définit l'action type de la demande
        /// </summary>
        Cst.Capture.ModeEnum CaptureMode { set; get; }

        /// <summary>
        /// Obtient définit un flag qui autorise l'exécution partielle du denouement d'option
        /// </summary>
        Boolean PartialExecutionAllowed { set; get; }

        // EG 20160404 Migration vs2013
        //Boolean feeCalculation { set; get; }

        /// <summary>
        /// Obtient définit un flag qui définie si Spheres® doit abandonner les quantités restantes dans le cadre d'un exercice
        /// <para>L'abandon s'applique uniquement sur le dernier trade exercé</para>
        /// </summary>
        Boolean AbandonRemainingQty { set; get; }
    }
    #endregion IPosRequestDetPositionOption



    #region IPosRequestDetRemoveAlloc
    // EG 20150920 [21374] Int (int32) to Long (Int64) 
    // EG 20170127 Qty Long To Decimal
    public interface IPosRequestDetRemoveAlloc : IPosRequestDetail
    {
        #region Accessors
        decimal InitialQty { set; get; }
        decimal PositionQty { set; get; }
        #endregion Accessors
    }
    #endregion IPosRequestDetRemoveAlloc
    #region IPosRequestDetSplit
    public interface IPosRequestDetSplit : IPosRequestDetail
    {
        #region Accessors
        ArrayList NewTrades { set; get; }
        #endregion Accessors
    }
    #endregion IPosRequestDetSplit
    #region IPosRequestDetTransfer
    /// EG 20141210 [20554] Add idTReplaceSpecified and trade_IdentifierReplaceSpecified
    /// EG 20150716 [21103] Add isReversalSafekeeping
    // EG 20150920 [21374] Int (int32) to Long (Int64) 
    // EG 20170127 Qty Long To Decimal
    public interface IPosRequestDetTransfer : IPosRequestDetail
    {
        #region Accessors
        decimal InitialQty { set; get; }
        decimal PositionQty { set; get; }
        IParty DealerTarget { set; get; }
        bool DealerTargetSpecified { get; }
        IBookId DealerBookIdTarget { set; get; }
        bool DealerBookIdTargetSpecified { get; }
        IParty ClearerTarget { set; get; }
        bool ClearerTargetSpecified { get; }
        bool ClearerBookIdTargetSpecified { get; }
        IBookId ClearerBookIdTarget { set; get; }

        bool IdTReplaceSpecified { set; get; }
        int IdTReplace { set; get; }
        bool Trade_IdentifierReplaceSpecified { set; get; }
        string Trade_IdentifierReplace { set; get; }
        bool IsReversalFeesSpecified { set; get; }
        bool IsReversalFees { set; get; }
        bool IsCalcNewFeesSpecified { set; get; }
        bool IsCalcNewFees{ set; get; }
        /// EG 20150716 [21103]
        // EG 20160404 Migration vs2013
        //bool isReversalSafekeepingSpecified { set; get; }
        //bool isReversalSafekeeping { set; get; }

        #endregion Accessors

    }
    #endregion IPosRequestDetTransfer
    #region IPosRequestDetUnclearing
    // EG 20150920 [21374] Int (int32) to Long (Int64) 
    // EG 20170127 Qty Long To Decimal
    public interface IPosRequestDetUnclearing : IPosRequestDetail
    {
        #region Accessors
        Cst.PosRequestTypeEnum RequestType { set; get; }
        int IdPR { set; get; }
        int IdPADET { set; get; }
        int IdT_Closing { set; get; }
        string Closing_Identifier { set; get; }
        DateTime DtBusiness { set; get; }
        decimal ClosingQty { set; get; }
        bool IdT_DeliverySpecified { set; get; }
        int IdT_Delivery { set; get; }
        bool Delivery_IdentifierSpecified { set; get; }
        string Delivery_Identifier { set; get; }
        #endregion Accessors
        #region Methods
        #endregion Methods
    }
    #endregion IPosRequestDetUnclearing
    #region IPosRequestDetUnderlyer
    // EG 20180307 [23769] Gestion dbTransaction
    public interface IPosRequestDetUnderlyer
    {
        #region Accessors
        // EG 20151102 [21465] New
        PosRequestSource[] PosRequestSource { set; get; }
        //int idPRSource { set; get; }
        Cst.PosRequestTypeEnum RequestTypeSource { set; get; }
        int IdTSource { set; get; }
        Cst.UnderlyingAsset AssetCategory { set; get; }
        int IdAsset { set; get; }
        string Identifier { get; set; }
        QuoteTimingEnum QuoteTiming { get; set; }
        decimal Price { set; get; }
        DateTime DtPrice { set; get; }
        string Source { get; set; }
        // EG 20180307 [23769] Gestion dbTransaction
        EFS_Asset GetCharacteristics(string pCS, IDbTransaction pDbTransaction);
        int IdI { set; get; }
        /// <summary>
        /// Représente le trade ss-jacent
        /// </summary>
        DataDocumentContainer DataDocument { set; get; }
        IPosRequestTradeAdditionalInfo CreateAdditionalInfo(DataDocumentContainer pTemplateDataDocument, SQL_TradeCommon pSQLTrade,
            SQL_Product pSQLProduct, SQL_Instrument pSQLInstrument, string pScreenName);
        // EG 20151102 [21465] New
        // EG 20170127 Qty Long To Decimal
        void SetPosRequestSource(int pIdPR, decimal pQty);
        // EG 20151102 [21465] New
        void DeletePosRequestSource(int pIdPR);
        int[] GetIdPRSource();
        #endregion Accessors
    }
    #endregion IPosRequestDetUnderlyer
    #region IPosRequestTradeAdditionalInfo
    
    /// <summary>
    ///  Pilote l'enregistrement d'un trade né d'une action sur Position
    /// </summary>
    /// EG 20141230 [20587]
    public interface IPosRequestTradeAdditionalInfo
    {
        #region Accessors
        /// <summary>
        /// DataDocument Template
        /// </summary>
        DataDocumentContainer TemplateDataDocument { set; get; }
        /// <summary>
        /// Trade  Template
        /// </summary>
        SQL_TradeCommon SqlTemplateTrade { set; get; }
        /// <summary>
        /// Produit 
        /// </summary>
        SQL_Product SqlProduct { set; get; }
        /// <summary>
        ///  Instrument
        /// </summary>
        SQL_Instrument SqlInstrument { set; get; }
        /// <summary>
        /// Représente l'écran
        /// </summary>
        string ScreenName { set; get; }
        Cst.StatusActivation StActivation { set; get; }
        string DisplayName { set; get; }
        string Description { set; get; }
        string ExtlLink { set; get; }
        #endregion Accessors
    }
    #endregion IPosRequestTradeAdditionalInfo

    #region IPosRequestDetUpdateEntry
    // EG 20150920 [21374] Int (int32) to Long (Int64) 
    // EG 20170127 Qty Long To Decimal
    // EG 20171025 [23509] Upd dtExecution replace timestamp
    public interface IPosRequestDetUpdateEntry : IPosRequestDetail
    {
        #region Accessors
        DateTime DtBusiness { set; get; }
        int Side { set; get; }
        decimal Qty { set; get; }
        DateTime DtExecution { set; get; }
        string PositionEffect { set; get; }
        #endregion Accessors
    }
    #endregion IPosRequestDetEntry


    #region IRateEvent
    public interface IRateEvent : IMidLifeEvent
    {
        #region Accessors
        EFS_Decimal Rate { set; get; }
        #endregion Accessor
    }
    #endregion IRateEvent
    #region IRateIndexRepository
    public interface IAssetRateIndexRepository : IAssetRepository
    {
        #region Accessors
        bool InformationSourceSpecified { set; get; }
        IInformationSource InformationSource { get; set; }
        bool RateTypeSpecified { set; get; }
        string RateType { set; get; }
        bool CalculationRuleSpecified { set; get; }
        string CalculationRule { set; get; }
        #endregion Accessors
        #region Methodes
        IInformationSource CreateInformationSource();
        #endregion Methodes
    }
    #endregion IRateIndexRepository
    #region IRebateBracketCalculation
    public interface IRebateBracketCalculation
    {
        #region Accessors
        IBracket Bracket { set; get; }
        EFS_Decimal BracketRate { set; get; }
        bool BracketAmountSpecified { set; get; }
        IMoney BracketAmount { set; get; }
        #endregion Accessors
    }
    #endregion IRebateBracketCalculation
    #region IRebateBracketCalculations
    public interface IRebateBracketCalculations
    {
        #region Accessors
        IRebateBracketCalculation[] RebateBracketCalculation { set; get; }
        #region Methods
        decimal CalculRebateBracketAmount(string pConnectionString, BracketApplicationEnum pBracketApplication, decimal pAmount, CurrencyCashInfo pCurrencyCashInfo);
        #endregion Methods

        #endregion Accessors
    }
    #endregion IRebateBracketCalculations
    #region IRebateBracketConditions
    public interface IRebateBracketConditions
    {
        #region Accessors
        IRebateBracketParameters Parameters { set; get; }
        IRebateBracketResult Result { set; get; }
        #endregion Accessors
        #region Methods
        IRebateBracketParameters CreateParameters(BracketApplicationEnum pBracketApplication, PeriodEnum pPeriod, int pPeriodMultiplier, RollConventionEnum pRollConvention, int pBracketLength);
        IRebateBracketResult CreateResult(IRebateBracketParameter[] pParameters);
        #endregion Methods
    }
    #endregion IRebateBracketConditions
    #region IRebateBracketParameter
    public interface IRebateBracketParameter
    {
        #region Accessors
        IBracket Bracket { set; get; }
        EFS_Decimal Rate { set; get; }
        #endregion Accessors
    }
    #endregion IRebateBracketParameter
    #region IRebateBracketParameters
    public interface IRebateBracketParameters
    {
        #region Accessors
        ICalculationPeriodFrequency ApplicationPeriod { set; get; }
        BracketApplicationEnum ManagementType { set; get; }
        IRebateBracketParameter[] Parameter { set; get; }
        IRebateBracketParameter CreateParameter(decimal pLowValue, decimal pHighValue, decimal pDiscountRate);
        void SetParameter(int pIndex, IRebateBracketParameter pParameter);
        #endregion Accessors
    }
    #endregion IRebateBracketParameters
    #region IRebateBracketResult
    public interface IRebateBracketResult
    {
        #region Accessors
        bool SumOfGrossTurnOverPreviousPeriodAmountSpecified { set; get; }
        IMoney SumOfGrossTurnOverPreviousPeriodAmount { set; get; }
        bool SumOfNetTurnOverPreviousPeriodAmountSpecified { set; get; }
        IMoney SumOfNetTurnOverPreviousPeriodAmount { set; get; }
        IRebateBracketCalculations Calculations { set; get; }
        bool TotalRebateBracketAmountSpecified { set; get; }
        IMoney TotalRebateBracketAmount { set; get; }
        #endregion Accessors
    }
    #endregion IRebateBracketResult
    #region IRebateCapConditions
    public interface IRebateCapConditions
    {
        #region Accessors
        IRebateCapParameters Parameters { set; get; }
        IRebateCapResult Result { set; get; }
        #endregion Accessors
        #region Methods
        IRebateCapParameters CreateParameters(PeriodEnum pPeriod, int pPeriodMultiplier, RollConventionEnum pRollConvention, decimal pAmount, string pCurrency);
        IRebateCapResult CreateResult();
        #endregion Methods
    }
    #endregion IRebateCapConditions
    #region IRebateCapParameters
    public interface IRebateCapParameters
    {
        #region Accessors
        bool MaximumNetTurnOverAmountSpecified { set; get; }
        IMoney MaximumNetTurnOverAmount { set; get; }
        ICalculationPeriodFrequency ApplicationPeriod { set; get; }
        #endregion Accessors
    }
    #endregion IRebateCapParameters
    #region IRebateCapResult
    public interface IRebateCapResult
    {
        #region Accessors
        bool SumOfNetTurnOverPreviousPeriodAmountSpecified { set; get; }
        IMoney SumOfNetTurnOverPreviousPeriodAmount { set; get; }
        bool NetTurnOverInExcessAmountSpecified { set; get; }
        IMoney NetTurnOverInExcessAmount { set; get; }
        #endregion Accessors
    }
    #endregion IRebateCapResult
    #region IRebateConditions
    public interface IRebateConditions
    {
        #region Accessors
        bool BracketConditionsSpecified { set; get; }
        IRebateBracketConditions BracketConditions { set; get; }
        bool CapConditionsSpecified { set; get; }
        IRebateCapConditions CapConditions { set; get; }
        bool TotalRebateAmountSpecified { set; get; }
        IMoney TotalRebateAmount { set; get; }
        #endregion Accessors
        #region Methods
        void CreateBracketConditions();
        void CreateCapConditions();
        #endregion Methods
    }
    #endregion IRebateConditions
    #region IRelativeDateAndAdjustedDate
    public interface IRelativeDateAndAdjustedDate
    {
        #region Accessors
        bool RelativeDateSpecified { set; get; }
        IRelativeDateOffset RelativeDate { set; get; }
        IAdjustedDate AdjustedDate { set; get; }
        #endregion Accessors
    }
    #endregion IRelativeDateAndAdjustedDate
    #region IRelativePrice
    public interface IRelativePrice
    {
        #region Accessors
        EFS_Decimal Spread { set; get; }
        bool BondSpecified { set; get; }
        IBond Bond { set; get; }
        bool ConvertibleBondSpecified { set; get; }
        IConvertibleBond ConvertibleBond { set; get; }
        bool EquitySpecified { set; get; }
        IEquityAsset Equity { set; get; }
        #endregion Accessors
    }
    #endregion IRelativePrice
    #region IRepo
    public interface IRepo : ISaleAndRepurchaseAgreement
    {
        #region Accessors
        #endregion Accessors
    }
    #endregion IRepo
    #region IRepoCollateral
    public interface IRepoCollateral
    {
        #region Accessors
        bool BondSpecified { set; get; }
        IBond Bond { set; get; }
        bool ConvertibleBondSpecified { set; get; }
        IConvertibleBond ConvertibleBond { set; get; }
        bool EquitySpecified { set; get; }
        IEquityAsset Equity { set; get; }
        #endregion Accessors
    }
    #endregion IRepoCollateral

    #region IRepoTransactionLeg
    public interface IRepoTransactionLeg
    {
        #region Accessors
        bool IdentifierIdSpecified { set; get; }
        IScheme[] IdentifierId { set; get; }
        bool IdentifierVersionedIdSpecified { set; get; }
        IVersionedId[] IdentifierVersionedId { set; get; }
        bool IdentifierNoneSpecified { set; get; }
        object IdentifierNone { set; get; }
        IReference BuyerPartyReference { set; get; }
        IReference SellerPartyReference { set; get; }
        IAdjustableOrRelativeDate SettlementDate { set; get; }
        bool SettlementAmountSpecified { set; get; }
        IMoney SettlementAmount { set; get; }
        bool SettlementCurrencySpecified { set; get; }
        ICurrency SettlementCurrency { set; get; }

        bool CollateralValuationSpecified { set; get; }
        ICollateralValuation[] CollateralValuation { set; get; }
        #endregion Accessors
    }
    #endregion IRepoTransactionLeg
    #region IRoutingCreateElement
    /// <summary>
    /// 
    /// </summary>
    public interface IRoutingCreateElement
    {
        #region Methods
        IRouting CreateRouting();
        //
        IRoutingIds[] CreateRoutingIds(IRoutingId[] pRoutingId);
        IRoutingIdsAndExplicitDetails CreateRoutingIdsAndExplicitDetails();
        //
        IRoutingId CreateRoutingId();
        IAddress CreateAddress();
        IStreetAddress CreateStreetAddress();
        IScheme CreateCountry();
        #endregion Methods
    }
    #endregion IRoutingCreateElement
    #region IRoutingPartyReference
    public interface IRoutingPartyReference : IRouting
    {
        #region Accessors
        bool HRefSpecified { set; get; }
        string HRef { set; get; }
        #endregion Accessors
    }
    #endregion

    #region ISaleAndRepurchaseAgreement
    public interface ISaleAndRepurchaseAgreement
    {
        #region Accessors
        RepoDurationEnum Duration { set; get; }
        bool NoticePeriodSpecified { set; get; }
        IAdjustableOffset NoticePeriod { set; get; }
        ICashStream[] CashStream { set; get; }
        ISecurityLeg[] SpotLeg { set; get; }
        bool ForwardLegSpecified { set; get; }
        ISecurityLeg[] ForwardLeg { set; get; }
        EFS_SaleAndRepurchaseAgreement Efs_SaleAndRepurchaseAgreement { set; get; }
        EFS_EventDate MaxTerminationDate { get; }
        #endregion Accessors
    }
    #endregion ISaleAndRepurchaseAgreement
    #region ISecurity
    // EG 20190716 [VCL : New FixedIncome] Upd (GetRoundingAccruedRate, GetDayCountFractionForAccruedRate, GetPriceQuote, GetAssetMeasure
    public interface ISecurity : IExchangeTraded
    {
        #region Accessors
        bool ClassificationSpecified { set; get; }
        IClassification Classification { set; get; }
        bool CouponTypeSpecified { set; get; }
        IScheme CouponType { set; get; }
        bool PriceRateTypeSpecified { set; get; }
        PriceRateType3CodeEnum PriceRateType { set; get; }
        bool LocalizationSpecified { set; get; }
        ILocalization Localization { set; get; }
        bool InstructionRegistryCountrySpecified { set; get; }
        IScheme InstructionRegistryCountry { set; get; }
        bool InstructionRegistryReferenceSpecified { set; get; }
        IReference InstructionRegistryReference { set; get; }
        bool InstructionRegistryNoneSpecified { set; get; }
        IEmpty InstructionRegistryNone { set; get; }
        bool GuarantorPartyReferenceSpecified { set; get; }
        IReference GuarantorPartyReference { set; get; }
        bool ManagerPartyReferenceSpecified { set; get; }
        IReference ManagerPartyReference { set; get; }
        bool PurposeSpecified { set; get; }
        EFS_String Purpose { set; get; }
        bool SenioritySpecified { set; get; }
        IScheme Seniority { set; get; }
        bool NumberOfIssuedSecuritiesSpecified { set; get; }
        // EG 20170127 Qty Long To Decimal
        EFS_Decimal NumberOfIssuedSecurities { set; get; }
        bool FaceAmountSpecified { set; get; }
        IMoney FaceAmount { set; get; }
        bool PriceSpecified { set; get; }
        ISecurityPrice Price { set; get; }
        bool CommercialPaperSpecified { set; get; }
        ICommercialPaper CommercialPaper { set; get; }
        bool CalculationRulesSpecified { set; get; }
        ISecurityCalculationRules CalculationRules { set; get; }
        bool OrderRulesSpecified { set; get; }
        ISecurityOrderRules OrderRules { set; get; }
        bool QuoteRulesSpecified { set; get; }
        ISecurityQuoteRules QuoteRules { set; get; }
        bool IndicatorSpecified { set; get; }
        ISecurityIndicator Indicator { set; get; }
        bool YieldSpecified { set; get; }
        ISecurityYield Yield { set; get; }
        #endregion Accessors
        #region Methods
        ILocalization CreateLocalization();
        IClassification CreateClassification();
        IRounding GetRoundingAccruedRate(RoundingDirectionEnum pDirection, int pPrecision);
        DayCountFractionEnum GetDayCountFractionForAccruedRate(DayCountFractionEnum pDayCountFraction);
        Cst.PriceQuoteUnits GetPriceQuote(Cst.PriceQuoteUnits pPriceQuoteUnits);
        AssetMeasureEnum GetAssetMeasure(AssetMeasureEnum pAssetMeasure);
        #endregion Methods
    }
    #endregion ISecurity
    #region ISecurityAsset
    // EG 20150422 [20513] BANCAPERTA New issuerReference
    public interface ISecurityAsset
    {
        #region Members
        EFS_String SecurityId { set; get; }
        string Id { set; get; }
        string OtcmlId { set; get; }
        int OTCmlId { set; get; }
        bool SecurityNameSpecified { set; get; }
        EFS_String SecurityName { set; get; }
        bool SecurityDescriptionSpecified { set; get; }
        EFS_String SecurityDescription { set; get; }
        bool SecurityIssueDateSpecified { set; get; }
        EFS_Date SecurityIssueDate { set; get; }
        bool DebtSecuritySpecified { set; get; }
        IDebtSecurity DebtSecurity { set; get; }
        IParty Issuer { set; get; }
        bool IssuerReferenceSpecified { set; get; }
        IReference IssuerReference { set; get; }
        bool IdTTemplateSpecified { set; get; }
        int IdTTemplate { set; get; }
        bool IsNewAssetSpecified { set; get; }
        bool IsNewAsset { set; get; }
        #endregion
    }
    #endregion ISecurityAsset
    #region ISecurityCalculationRules
    public interface ISecurityCalculationRules
    {
        #region Accessors
        bool FullCouponCalculationRulesSpecified { set; get; }
        IFullCouponCalculationRules FullCouponCalculationRules { set; get; }
        
        bool AccruedInterestCalculationRulesSpecified { set; get; }
        IAccruedInterestCalculationRules AccruedInterestCalculationRules { set; get; }
        #endregion Accessors
    }
    #endregion ISecurityCalculationRules
    #region ISecurityIndicator
    public interface ISecurityIndicator
    {
        #region Accessors
        bool CertificatedSpecified { set; get; }
        EFS_Boolean Certificated { set; get; }
        bool DematerialisedSpecified { set; get; }
        EFS_Boolean Dematerialised { set; get; }
        bool FungibleSpecified { set; get; }
        EFS_Boolean Fungible { set; get; }
        bool ImmobilisedSpecified { set; get; }
        EFS_Boolean Immobilised { set; get; }
        bool AmortisedSpecified { set; get; }
        EFS_Boolean Amortised { set; get; }
        bool CallProtectionSpecified { set; get; }
        EFS_Boolean CallProtection { set; get; }
        bool CallableSpecified { set; get; }
        EFS_Boolean Callable { set; get; }
        bool PutableSpecified { set; get; }
        EFS_Boolean Putable { set; get; }
        bool ConvertibleSpecified { set; get; }
        EFS_Boolean Convertible { set; get; }
        bool EscrowedSpecified { set; get; }
        EFS_Boolean Escrowed { set; get; }
        bool PrefundedSpecified { set; get; }
        EFS_Boolean Prefunded { set; get; }
        bool PaymentDirectionSpecified { set; get; }
        EFS_Boolean PaymentDirection { set; get; }
        bool QuotedSpecified { set; get; }
        EFS_Boolean Quoted { set; get; }
        #endregion Accessors
    }
    #endregion ISecurityIndicator
    #region ISecurityLeg
    public interface ISecurityLeg
    {
        #region Accessors
        string Id { set; get; }
        bool SpotLegReferenceSpecified { set; get; }
        IReference SpotLegReference { set; get; }
        IDebtSecurityTransaction DebtSecurityTransaction { set; get; }
        bool MarginSpecified { set; get; }
        IMargin Margin { set; get; }
        #endregion Accessors
    }
    #endregion ISecurityLeg
    #region ISecurityLending
    public interface ISecurityLending : ISaleAndRepurchaseAgreement
    {
        #region Accessors
        #endregion Accessors
    }
    #endregion ISecurityLending

    #region ISecurityOrderRules
    public interface ISecurityOrderRules
    {
        #region Accessors
        bool PriceUnitsSpecified { set; get; }
        IPriceUnits PriceUnits { set; get; }
        bool AccruedInterestIndicatorSpecified { set; get; }
        /// <summary>
        /// <para>
        /// False: indique que les intérêt courus ne sont pas insclus (Prix Clean)
        /// </para>
        /// <para>
        /// True: indique que les intérêt courus sont insclus (Prix Dirty)
        /// </para>
        /// </summary>
        EFS_Boolean AccruedInterestIndicator { set; get; }
        bool PriceInPercentageRoundingSpecified { set; get; }
        IRounding PriceInPercentageRounding { set; get; }
        bool PriceInRateRoundingSpecified { set; get; }
        IRounding PriceInRateRounding { set; get; }
        bool QuantityTypeSpecified { set; get; }
        OrderQuantityType3CodeEnum QuantityType { set; get; }
        bool SettlementDaysOffsetSpecified { set; get; }
        IOffset SettlementDaysOffset { set; get; }
        #endregion Accessors
    }
    #endregion ISecurityOrderRules
    #region ISecurityPrice
    public interface ISecurityPrice
    {
        #region Accessors
        bool IssuePricePercentageSpecified { set; get; }
        EFS_Decimal IssuePricePercentage { set; get; }
        bool RedemptionPricePercentageSpecified { set; get; }
        EFS_Decimal RedemptionPricePercentage { set; get; }
        bool RedemptionPriceFormulaSpecified { set; get; }
        IFormula RedemptionPriceFormula { set; get; }
        bool RedemptionPriceNoneSpecified { set; get; }
        IEmpty RedemptionPriceNone { set; get; }
        #endregion Accessors
    }
    #endregion ISecurityPrice
    #region ISecurityQuoteRules
    public interface ISecurityQuoteRules
    {
        #region Accessors
        bool QuoteUnitsSpecified { set; get; }
        IScheme QuoteUnits { set; get; }
        bool AccruedInterestIndicatorSpecified { set; get; }
        EFS_Boolean AccruedInterestIndicator { set; get; }
        bool QuoteRoundingSpecified { set; get; }
        IRounding QuoteRounding { set; get; }
        #endregion Accessors
    }
    #endregion ISecurityQuoteRules
    #region ISecurityTransfer
    public interface ISecurityTransfer : IAtomicSettlementTransfer
    {
        #region Accessors
        EFS_Decimal Quantity { set; get; }
        IReference AssetReference { set; get; }
        IReference DelivererPartyReference { set; get; }
        IReference ReceiverPartyReference { set; get; }
        bool DaylightIndicatorSpecified { set; get; }
        EFS_Boolean DaylightIndicator { set; get; }
        #endregion Accessors
    }
    #endregion ISecurityTransfer
    #region ISecurityYield
    public interface ISecurityYield
    {
        #region Accessors
        bool YieldTypeSpecified { set; get; }
        EfsML.Enum.YieldTypeEnum YieldType { set; get; }
        bool YieldSpecified { set; get; }
        EFS_Decimal Yield { set; get; }
        bool YieldCalculationDateSpecified { set; get; }
        EFS_Date YieldCalculationDate { set; get; }
        #endregion Accessors
    }
    #endregion ISecurityYield
    #region ISettlementChain
    public interface ISettlementChain
    {
        #region Accessors
        ISettlementChainItem[] SettlementChainItem { get; }
        bool CssLinkSpecified { get; }
        int CssLink { set; get; }
        #endregion Accessors
        #region Indexors
        ISettlementChainItem this[PayerReceiverEnum pPayerReceiver] { get; }
        #endregion Indexors
        #region Methods
        IEfsSettlementInstruction[] CreateEfsSettlementInstructions();
        #endregion Methods

    }
    #endregion ISettlementChain
    #region ISettlementChainItem
    public interface ISettlementChainItem
    {
        #region Accessors
        SiModeEnum SiMode { set; get; }
        int IdIssi { get; }
        IEfsSettlementInstruction SettlementInstruction { set; get; }
        int IdASettlementOffice { set; get; }
        #endregion Accessors
    }
    #endregion ISettlementChainItem
    #region ISettlementInformationInput
    public interface ISettlementInformationInput
    {
        #region Accessors
        bool PartyReferencePayerSpecified { get; }
        IReference PartyReferencePayer { get; }
        bool PartyReferenceReceiverSpecified { get; }
        IReference PartyReferenceReceiver { get; }
        IEfsSettlementInformation SettlementInformation { get; }
        bool EventCodesScheduleSpecified { get; }
        IEventCodesSchedule EventCodesSchedule { get; }
        bool CssCriteriaSpecified { get; }
        ICssCriteria CssCriteria { get; }
        bool SsiCriteriaSpecified { get; }
        ISsiCriteria SsiCriteria { get; }
        #endregion Accessors
    }
    #endregion ISettlementInformationInput
    #region ISettlementInput
    public interface ISettlementInput
    {
        #region Accessors
        IFlowContext SettlementContext { get; }
        ISettlementInputInfo SettlementInputInfo { get; }
        #endregion Accessors
        //
        #region Methods
        object Clone();
        #endregion Methods
    }
    #endregion ISettlementInput
    #region ISettlementInputInfo
    public interface ISettlementInputInfo
    {
        #region Accessors
        IEfsSettlementInformation SettlementInformation { get; }
        bool CssCriteriaSpecified { set; get; }
        ICssCriteria CssCriteria { get; }
        bool SsiCriteriaSpecified { set; get; }
        ISsiCriteria SsiCriteria { get; }
        #endregion Accessors
        #region Methods
        IEfsSettlementInstruction[] CreateEfsSettlementInstructions(IEfsSettlementInstruction pEfsSettlementInstruction);
        IEfsSettlementInstruction[] CreateEfsSettlementInstructions();
        #endregion Methods

    }
    #endregion ISettlementInputInfo
    #region ISettlementMessageDocument
    public interface ISettlementMessageDocument
    {
        EfsMLDocumentVersionEnum EfsMLversion { set; get; }
        ISettlementMessageHeader Header { set; get; }
        ISettlementMessagePayment[] Payment { set; get; }
        ISettlementMessageHeader CreateSettlementMessageHeader();
        ISettlementMessagePayment[] CreateSettlementMessagePayment(int pNumber);
        ISettlementMessagePartyPayment CreateSettlementMessagePartyPayment();
        IMoney CreateMoney(decimal pAmount, string pCurrency);
        IRoutingCreateElement CreateRoutingCreateElement();
    }
    #endregion
    #region ISettlementMessageHeader
    public interface ISettlementMessageHeader : ISpheresId
    {
        SettlementMessageId[] SettlementMessageId { set; get; }
        EFS_DateTime CreationTimestamp { set; get; }
        EFS_Date ValueDate { set; get; }
        bool SumOfPaymentAmountsSpecified { set; get; }
        EFS_Decimal SumOfPaymentAmounts { set; get; }
        IRouting Sender { set; get; }
        IRouting Receiver { set; get; }
    }
    #endregion
    #region ISettlementMessagePartyPayment
    public interface ISettlementMessagePartyPayment : IRouting
    {
        #region Accessors
        bool TradeIdSpecified { set; get; }
        ISchemeId[] TradeId { set; get; }
        IEfsSettlementInstruction SettlementInstruction { set; get; }
        #endregion Accessors

        #region Methods
        void LoadTradeId(string pCs, int pIdT);
        #endregion Methods

    }
    #endregion ISettlementMessagePartyPayment
    #region ISettlementMessagePayment
    public interface ISettlementMessagePayment
    {
        PaymentId PaymentId { set; get; }
        ISettlementMessagePartyPayment Payer { set; get; }
        ISettlementMessagePartyPayment Receiver { set; get; }
        EFS_Date ValueDate { set; get; }
        IMoney PaymentAmount { set; get; }
        NettingInformation NettingInformation { set; get; }
        bool DataDocumentSpecified { set; get; }
        IDataDocument DataDocument { set; get; }
        bool EventsSpecified { set; get; }
        EventItems Events { set; get; }
    }
    #endregion ISettlementMessagePayment
    #region ISettlementTransfer
    public interface ISettlementTransfer : ISettlementTransferIdentifierBase
    {
        #region Accessors
        bool TransferInformationSpecified { set; get; }
        IPartySettlementTransferInformation[] TransferInformation { set; get; }
        ITransfer[] Transfer { set; get; }
        bool SettlementInstructionSpecified { set; get; }
        ISettlementInstruction[] SettlementInstruction { set; get; }
        #endregion Accessors
    }
    #endregion ISettlementTransfer
    #region ISettlementTransferIdentifier
    public interface ISettlementTransferIdentifier : ISettlementTransferIdentifierBase
    {
        #region Accessors
        #endregion Accessors
    }
    #endregion ISettlementTransferIdentifierBase
    #region ISettlementTransferIdentifierBase
    public interface ISettlementTransferIdentifierBase
    {
        #region Accessors
        bool IdentifierIdSpecified { set; get; }
        IScheme[] IdentifierId { set; get; }
        bool IdentifierVersionedIdSpecified { set; get; }
        IVersionedId[] IdentifierVersionedId { set; get; }
        bool TypeNoneSpecified { set; get; }
        IEmpty TypeNone { set; get; }
        bool TypeTypeSpecified { set; get; }
        IScheme[] TypeType { set; get; }
        bool TypeVersionedTypeSpecified { set; get; }
        IVersionedId[] TypeVersionedType { set; get; }
        #endregion Accessors
    }
    #endregion ISettlementTransferIdentifierBase
    #region ISettlementTransferProcessingInformation
    public interface ISettlementTransferProcessingInformation
    {
        #region Accessors
        bool OwnerSpecified { set; get; }
        EFS_Boolean Owner { set; get; }
        #endregion Accessors
    }
    #endregion ISettlementTransferProcessingInformation

    #region ISoftApplication
    public interface ISoftApplication
    {
        EFS_String Name { set; get; }
        EFS_String Version { set; get; }
    }
    #endregion ISoftApplication

    #region ISpheresId
    public interface ISpheresId
    {
        #region Accessors
        string OtcmlId { set; get; }
        int OTCmlId { set; get; }
        #endregion Accessors
    }
    #endregion ISpheresId
    #region ISpheresIdScheme
    public interface ISpheresIdScheme : ISpheresId, IScheme
    {

    }
    #endregion ISpheresIdScheme
    #region ISpheresIdSchemeId
    public interface ISpheresIdSchemeId : ISpheresId, ISchemeId
    {

    }
    #endregion ISpheresIdSchemeId

    #region ISpheresSource
    public interface ISpheresSource
    {
        #region Accessors
        bool StatusSpecified { set; get; }
        SpheresSourceStatusEnum Status { set; get; }
        ISpheresIdSchemeId[] SpheresId { set; get; }
        #endregion Accessors
        #region Methods
        ISpheresIdSchemeId GetSpheresIdFromScheme(string pScheme);
        ISpheresIdSchemeId[] GetSpheresIdLikeScheme(string pScheme);
        ISpheresIdSchemeId GetSpheresIdFromSchemeId(string pSchemeId);
        #endregion Methods
    }
    #endregion ISpheresSource
    #region ISsiCriteria
    public interface ISsiCriteria
    {
        #region Accessors
        bool CountrySpecified { set; get; }
        IScheme Country { set; get; }
        #endregion Accessors
    }
    #endregion ISsiCriteria
    #region IStepUpProvision
    public interface IStepUpProvision : IProvision
    {
        #region Accessors
        EFS_ExerciseDates Efs_ExerciseDates { set; get; }
        #endregion Accessors
    }
    #endregion IStepUpProvision
    
    #region ITax
    // EG 20100504 Ticket:16978
    public interface ITax
    {
        #region Accessors
        ISpheresSource TaxSource { set; get; }
        ITaxSchedule[] TaxDetail { set; get; }
        #endregion Accessors
        #region Methods
        #endregion Methods
    }
    #endregion ITaxSchedule
    #region ITaxSchedule
    // EG 20100504 Ticket:16978
    public interface ITaxSchedule
    {
        #region Accessors
        bool TaxAmountSpecified { set; get; }
        ITripleInvoiceAmounts TaxAmount { set; get; }
        ISpheresSource TaxSource { set; get; }
        ITripleInvoiceAmounts CreateTripleInvoiceAmounts { get; }
        IMoney CreateMoney(decimal pAmount, string pCurrency);
        object Clone();
        string GetEventType();
        decimal GetRate();
        decimal GetTaxAmount(string pCs, decimal pBaseAmount, string pCurrency);
        #endregion Accessors
    }
    #endregion ITaxSchedule

    #region ITradeExtends
    public interface ITradeExtends
    {
        #region Accessors
        ITradeExtend[] TradeExtend { set; get; }
        #endregion Accessors
        #region Methods
        ITradeExtend GetSpheresIdFromScheme(int pOTCmlId);
        ITradeExtend GetSpheresIdFromScheme2(string pScheme);
        #endregion Methods
    }
    #endregion ITradeExtends

    #region ITradeProcessingTimestamps
    // EG 20171025 [23509] New
    // EG 20171031 [23509] Upd
    public interface ITradeProcessingTimestamps
    {
        bool OrderEnteredSpecified { set; get; }
        String OrderEntered { set; get; }
        Nullable<DateTimeOffset> OrderEnteredDateTimeOffset { get; }
    }
    #endregion

    #region ITradeTypeReport
    /// EG 20190730 New 
    public interface ITradeTypeReport
    {
        #region Accessors
        bool TrdTypeSpecified { set; get; }
        TrdTypeEnum TrdType { set; get; }
        bool TrdSubTypeSpecified { set; get; }
        TrdSubTypeEnum TrdSubType { set; get; }
        bool SecondaryTrdTypeSpecified { set; get; }
        SecondaryTrdTypeEnum SecondaryTrdType { set; get; }
        #endregion Accessors
    }
    #endregion ITradeTypeReport


    #region ITradeAndComponentIdentifier
    public interface ITradeAndComponentIdentifier : ITradeIdentifier
    {
        #region Accessors
        ITradeComponentIdentifier TradeComponentIdentifier { set; get; }
        #endregion Accessors
    }
    #endregion ITradeAndComponentIdentifier
    #region ITradeComponentIdentification
    public interface ITradeComponentIdentification
    {
        #region Accessors
        bool IdSpecified { set; get; }
        IScheme[] Id { set; get; }
        bool VersionedIdSpecified { set; get; }
        IVersionedId[] VersionedId { set; get; }
        bool ReferenceSpecified { set; get; }
        IReference Reference { set; get; }
        #endregion Accessors
    }
    #endregion ITradeComponentIdentification
    #region ITradeComponentIdentifier
    public interface ITradeComponentIdentifier
    {
        #region Accessors
        bool IdentificationRepoLegSpecified { set; get; }
        ITradeComponentIdentification IdentificationRepoLeg { set; get; }
        bool IdentificationEventSpecified { set; get; }
        ITradeComponentIdentification IdentificationEvent { set; get; }
        bool IdentificationStreamSpecified { set; get; }
        ITradeComponentIdentification IdentificationStream { set; get; }
        #endregion Accessors
    }
    #endregion ITradeComponentIdentifier

    #region ITradeExtend
    public interface ITradeExtend : ISpheresIdScheme
    {
        #region Accessors
        bool HRefSpecified { set; get; }
        string HRef { set; get; }
        #endregion Accessors
    }
    #endregion ITradeExtend
    #region ITradeIdentifierList
    public interface ITradeIdentifierList
    {
        #region Accessors
        ITradeIdentifier[] TradeIdentifier { set; get; }
        #endregion Accessors
    }
    #endregion ITradeIdentifierList
    #region ITradeIntention
    public interface ITradeIntention
    {
        IReference[] Initiator { set; get; }
        bool ReactorSpecified { set; get; }
        IReference Reactor { set; get; }
        string Id { set; get; }
    }
    #endregion ITradeIntention
    
    #region ITransfer
    public interface ITransfer
    {
        #region Accessors
        bool TransferIdSpecified { set; get; }
        IScheme[] TransferId { set; get; }
        bool TransferVersionedIdSpecified { set; get; }
        IVersionedId[] TransferVersionedId { set; get; }
        bool TransferNoneSpecified { set; get; }
        IEmpty TransferNone { set; get; }
        bool IdentifierComponentSpecified { set; get; }
        ITradeComponentIdentifier IdentifierComponent { set; get; }
        bool IdentifierTradeAndComponentSpecified { set; get; }
        ITradeAndComponentIdentifier[] IdentifierTradeAndComponent { set; get; }
        bool IdentifierNetTradeSpecified { set; get; }
        INetTradeIdentifier IdentifierNetTrade { set; get; }
        bool IdentifierNoneSpecified { set; get; }
        IEmpty IdentifierNone { set; get; }
        DeliveryMethodEnum DeliveryMethod { set; get; }
        IAdjustedDate TransferDate { set; get; }
        bool CashTransferSpecified { set; get; }
        ICashTransfer CashTransfer { set; get; }
        bool SecurityTransferSpecified { set; get; }
        ISecurityTransfer SecurityTransfer { set; get; }
        bool SettlementInstructionReferenceSpecified { set; get; }
        IReference SettlementInstructionReference { set; get; }
        #endregion Accessors
    }
    #endregion ITransfer

    #region ITripleInvoiceAmounts
    public interface ITripleInvoiceAmounts
    {
        #region Accessors
        IMoney Amount { set; get; }
        bool IssueAmountSpecified { set; get; }
        IMoney IssueAmount { set; get; }
        bool AccountingAmountSpecified { set; get; }
        IMoney AccountingAmount { set; get; }
        #endregion Accessors
        #region Methods
        IMoney CreateMoney(decimal pAmount, string pCurrency);
        #endregion Methods
    }
    #endregion ITripleInvoiceAmounts
    #region IUnderlyerCappedFlooredPrice
    public interface IUnderlyerCappedFlooredPrice
    {
        #region Accessors
        bool PriceSpecified { set; get; }
        EFS_Decimal Price { set; get; }
        bool PriceRelativeToSpecified { set; get; }
        IReference PriceRelativeTo { set; get; }
        #endregion Accessors
    }
    #endregion IUnderlyerCappedFlooredPrice
    #region IUnitContract
    public interface IUnitContract
    {
        #region Accessors
        EFS_Decimal NumberOfUnits { set; get; }
        IMoney UnitPrice { set; get; }
        #endregion Accessors
    }
    #endregion IUnitContract

    #region IVersionedEventId
    public interface IVersionedEventId : IVersionedId
    {
        #region Accessors
        ISchemeId Id { set; get; }
        #endregion Accessors
    }
    #endregion IVersionedEventId
    #region IVersionedRepoLegId
    public interface IVersionedRepoLegId : IVersionedId
    {
        #region Accessors
        IScheme Id { set; get; }
        #endregion Accessors
    }
    #endregion IVersionedRepoLegId
    #region IVersionedSettlementTransferId
    public interface IVersionedSettlementTransferId : IVersionedId
    {
        #region Accessors
        IScheme Id { set; get; }
        #endregion Accessors
    }
    #endregion IVersionedSettlementTransferId
    #region IVersionedSettlementTransferType
    public interface IVersionedSettlementTransferType : IVersionedId
    {
        #region Accessors
        IScheme Type { set; get; }
        #endregion Accessors
    }
    #endregion IVersionedSettlementTransferType
    #region IVersionedStreamId
    public interface IVersionedStreamId : IVersionedId
    {
        #region Accessors
        IScheme Id { set; get; }
        #endregion Accessors
    }
    #endregion IVersionedStreamId
    #region IVersionedTransferId
    public interface IVersionedTransferId : IVersionedId
    {
        #region Accessors
        IScheme Id { set; get; }
        #endregion Accessors
    }
    #endregion IVersionedTransferId

    #region IVersionedId
    public interface IVersionedId
    {
        #region Accessors
        EFS_NonNegativeInteger Version { set; get; }
        bool EffectiveDateSpecified { set; get; }
        IAdjustedDate EffectiveDate { set; get; }
        #endregion Accessors
    }
    #endregion IVersionedId

    #region IZonedDateTime
    // EG 20171003 [23452] New
    // EG 20171031 [23509] Upd
    public interface IZonedDateTime : IScheme
    {
        #region Accessors
        Nullable<DateTimeOffset> DateTimeOffsetValue { get; }
        string Tzdbid { set; get; }
        string Efs_id { set; get; }
        #endregion Accessors
    }
    #endregion IZonedDateTime

    

    #region MarginRequirement

    /// <summary>
    /// interface of a generic risk evaluation parameter
    /// </summary>
    public interface IRiskParameter
    {
        /// <summary>
        /// sub-parameters collection
        /// </summary>
        IRiskParameter[] Parameters { get; }

        /// <summary>
        /// Positions included in the paramater
        /// </summary>
        IFixPositionReport[] Positions { get; }

        /// <summary>
        /// Amounts generated for the elements 
        /// </summary>
        IMoney MarginAmount { get; }

    }

    /// <summary>
    /// Risk method interface including the risk evaluation parameters 
    /// </summary>
    public interface IMarginCalculationMethod
    {
        /// <summary>
        /// main parameters collection 
        /// </summary>
        IRiskParameter[] Parameters { get; }

        /// <summary>
        /// Amount relative to the parameter collection
        /// </summary>
        IMoney[] MarginAmounts { get; set; }
    }

    /// <summary>
    /// Interface including the net positions for computation 
    /// </summary>
    public interface INetMargin
    {
        IFixPositionReport[] Positions { get; }

        // PM 20151116 [21561] marginCalculationMethod devient un tableau
        IMarginCalculationMethod[] MarginCalculationMethod { get; set; }

        IMarginCalculationMethod DeliveryCalculationMethod { get; set; }

        IMoney[] MarginAmounts { get; set; }
    }

    /// <summary>
    /// Interface including the risk evaluation details, whatever modality net/gross has been used
    /// </summary>
    public interface IMarginCalculation
    {
        IMarginRequirementOffice[] GrossMargin { get; set; }

        INetMargin NetMargin { get; set; }
    }

    /// <summary>
    /// Interface hosting the log elements of a risk evaluation 
    /// </summary>
    public interface IMarginRequirementOffice
    {
        IBookId BookId { get; set; }

        IReference PayerPartyReference { get; set; }

        IReference ReceiverPartyReference { get; set; }

        IMoney[] MarginAmounts { get; set; }

        IMarginCalculation MarginCalculation { get; set; }
    }

    /// <summary>
    /// Interface giving the signature of a risk (initial margin) evaluation log
    /// </summary>
    /// <remarks>a document </remarks>
    public interface IMarginDetailsDocument : IDataDocument, IEfsDocument
    {
        /// <summary>
        /// Interface hosting the log elements
        /// </summary>
        IMarginRequirementOffice MarginRequirementOffice { get; set; }
    }

    #endregion MarginRequirement
}
