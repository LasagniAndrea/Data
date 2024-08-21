using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EFS.SpheresIO.MarketData.T7RDF.v10
{

    #region AccruedInterestCalculationMethodEnum
    /* AssetSubType(1950) */
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
    [System.SerializableAttribute()]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fixprotocol.org/FIXML-5-0-SP2")]
    public enum AccruedInterestCalculationMethodEnum
    {

        /// <remarks/>
        [System.Xml.Serialization.XmlEnumAttribute("1")]
        Basis30360,

        /// <remarks/>
        [System.Xml.Serialization.XmlEnumAttribute("3")]
        M30360,

        /// <remarks/>
        [System.Xml.Serialization.XmlEnumAttribute("6")]
        Act360,

        /// <remarks/>
        [System.Xml.Serialization.XmlEnumAttribute("7")]
        Act365Fixed,

        /// <remarks/>
        [System.Xml.Serialization.XmlEnumAttribute("8")]
        ActActAFB,

        /// <remarks/>
        [System.Xml.Serialization.XmlEnumAttribute("9")]
        ActActIcma,

        /// <remarks/>
        [System.Xml.Serialization.XmlEnumAttribute("11")]
        ActActIsda,

        /// <remarks/>
        [System.Xml.Serialization.XmlEnumAttribute("14")]
        Act365L,
    }
    #endregion
    #region AssetSubTypeEnum
    /* AssetSubType(29831) */
    [System.SerializableAttribute()]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fixprotocol.org/FIXML-5-0-SP2")]
    public enum AssetSubTypeEnum
    {
        [System.Xml.Serialization.XmlEnumAttribute("0")]
        EUAE,
        [System.Xml.Serialization.XmlEnumAttribute("1")]
        CERE,
        [System.Xml.Serialization.XmlEnumAttribute("2")]
        ERUE,
        [System.Xml.Serialization.XmlEnumAttribute("3")]
        EUAA,
        [System.Xml.Serialization.XmlEnumAttribute("4")]
        Other,
    }
    #endregion AssetSubTypeEnum
    #region AssetTypeEnum
    /* AssetType(1940) */
    [System.SerializableAttribute()]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fixprotocol.org/FIXML-5-0-SP2")]
    public enum AssetTypeEnum
    {
        [System.Xml.Serialization.XmlEnumAttribute("0")]
        EmissionAllowances,
        [System.Xml.Serialization.XmlEnumAttribute("1")]
        Other,
    }
    #endregion AssetTypeEnum

    #region AuctionTypeEnum
    /* AuctionType(1803) */
    [System.SerializableAttribute()]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fixprotocol.org/FIXML-5-0-SP2")]
    public enum AuctionTypeEnum
    {
        [System.Xml.Serialization.XmlEnumAttribute("100")]
        AnyAuction,
    }
    #endregion AuctionTypeEnum

    #region BusinessDayTypeEnum
    /* BusinessDayType(2581) */
    [System.SerializableAttribute()]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fixprotocol.org/FIXML-5-0-SP2")]
    public enum BusinessDayTypeEnum
    {
        [System.Xml.Serialization.XmlEnumAttribute("5")]
        PrecedingDay,
        [System.Xml.Serialization.XmlEnumAttribute("1")]
        CurrentDay,
    }
    #endregion BusinessDayTypeEnum

    #region CalculationMethodEnum
    /* CalculationMethod(28866) */
    [System.SerializableAttribute()]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fixprotocol.org/FIXML-5-0-SP2")]
    public enum CalculationMethodEnum
    {
        [System.Xml.Serialization.XmlEnumAttribute("0")]
        Automatic,
        [System.Xml.Serialization.XmlEnumAttribute("1")]
        Manual,
    }
    #endregion CalculationMethodEnum

    #region CheckMarketOrderEnum
    /// <summary>
    /// Specifies if Market Orders will be validated against the available bid/ask price on the opposing side in specifc scenarios
    /// </summary>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
    [System.SerializableAttribute()]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fixprotocol.org/FIXML-5-0-SP2")]
    public enum CheckMarketOrderEnum
    {

        /// <summary>
        /// No
        /// </summary>
        N,

        /// <summary>
        /// Yes
        /// </summary>
        Y,
    }
    #endregion

    #region ContractCycleSubTypeEnum
    /* ContractCycleType (30865) */
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
    [System.SerializableAttribute()]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fixprotocol.org/FIXML-5-0-SP2")]
    public enum ContractCycleSubTypeEnum
    {

        /// <remarks/>
        [System.Xml.Serialization.XmlEnumAttribute("1")]
        EndOfMonth,
    }
    #endregion
    #region ContractCycleTypeEnum
    /* ContractCycleType (30865) */
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
    [System.SerializableAttribute()]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fixprotocol.org/FIXML-5-0-SP2")]
    public enum ContractCycleTypeEnum
    {

        /// <remarks/>
        [System.Xml.Serialization.XmlEnumAttribute("1")]
        Daily,

        /// <remarks/>
        [System.Xml.Serialization.XmlEnumAttribute("2")]
        Weekly,

        /// <remarks/>
        [System.Xml.Serialization.XmlEnumAttribute("3")]
        Monthly,

        /// <remarks/>
        [System.Xml.Serialization.XmlEnumAttribute("4")]
        Quarterly,

        /// <remarks/>
        [System.Xml.Serialization.XmlEnumAttribute("5")]
        SemiAnnually,

        /// <remarks/>
        [System.Xml.Serialization.XmlEnumAttribute("6")]
        Yearly,
    }
    #endregion
    #region ContractDisplayInstructionEnum
    /* ContractDisplayInstruction (25186) */
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
    [System.SerializableAttribute()]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fixprotocol.org/FIXML-5-0-SP2")]
    public enum ContractDisplayInstructionEnum
    {

        /// <remarks/>
        [System.Xml.Serialization.XmlEnumAttribute("0")]
        None,

        /// <remarks/>
        [System.Xml.Serialization.XmlEnumAttribute("1")]
        Date,

        /// <remarks/>
        [System.Xml.Serialization.XmlEnumAttribute("2")]
        Month,

        /// <remarks/>
        [System.Xml.Serialization.XmlEnumAttribute("3")]
        Permanent,

        /// <remarks/>
        [System.Xml.Serialization.XmlEnumAttribute("4")]
        Quarter,

        /// <remarks/>
        [System.Xml.Serialization.XmlEnumAttribute("5")]
        Season,

        /// <remarks/>
        [System.Xml.Serialization.XmlEnumAttribute("6")]
        WeekOfYear,

        /// <remarks/>
        [System.Xml.Serialization.XmlEnumAttribute("7")]
        Year,

        /// <remarks/>
        [System.Xml.Serialization.XmlEnumAttribute("8")]
        WeekOfMonth,

        /// <remarks/>
        [System.Xml.Serialization.XmlEnumAttribute("9")]
        WeekendOfYear,

        /// <remarks/>
        [System.Xml.Serialization.XmlEnumAttribute("10")]
        RelativeDay,

        /// <remarks/>
        [System.Xml.Serialization.XmlEnumAttribute("11")]
        EndOfMonth,
    }
    #endregion
    #region ContractIdentificationEligibilityEnum
    /* ContractIdentificationEligibility (31803) */
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
    [System.SerializableAttribute()]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fixprotocol.org/FIXML-5-0-SP2")]
    public enum ContractIdentificationEligibilityEnum
    {
        [System.Xml.Serialization.XmlEnumAttribute("1")]
        ContractMonthYear,
        [System.Xml.Serialization.XmlEnumAttribute("2")]
        ExpirationDate,
        [System.Xml.Serialization.XmlEnumAttribute("3")]
        ContractDate,
    }
    #endregion

    #region ContractFrequencyEnum
    /* ContractFrequency (30867) */
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
    [System.SerializableAttribute()]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fixprotocol.org/FIXML-5-0-SP2")]
    public enum ContractFrequencyEnum
    {
        /// <summary>
        /// Day
        /// </summary>
        D,
        /// <summary>
        /// Week
        /// </summary>
        Wk,
        /// <summary>
        /// Month
        /// </summary>
        Mo,
        /// <summary>
        /// Flex
        /// </summary>
        Flex,
        /// <summary>
        /// EndOfMonth
        /// </summary>
        EOM,
    }
    #endregion

    #region CouponTypeEnum
    /* CouponType (1946) */
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
    [System.SerializableAttribute()]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fixprotocol.org/FIXML-5-0-SP2")]
    public enum CouponTypeEnum
    {
        [System.Xml.Serialization.XmlEnumAttribute("1")]
        Zero,
        [System.Xml.Serialization.XmlEnumAttribute("2")]
        FixedRate,
        [System.Xml.Serialization.XmlEnumAttribute("3")]
        FloatingRate,
        [System.Xml.Serialization.XmlEnumAttribute("4")]
        Structured,
    }
    #endregion

    #region CoverIndicatorEnum
    /* CoverIndicator (25200) */
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
    [System.SerializableAttribute()]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fixprotocol.org/FIXML-5-0-SP2")]
    public enum CoverIndicatorEnum
    {

        /// <remarks/>
        [System.Xml.Serialization.XmlEnumAttribute("1")]
        Intraday,

        /// <remarks/>
        [System.Xml.Serialization.XmlEnumAttribute("2")]
        Longterm,

        /// <remarks/>
        [System.Xml.Serialization.XmlEnumAttribute("3")]
        Activated,
    }
    #endregion

    #region DecaySplitEnum
    /* DecaySplit (25144) */
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
    [System.SerializableAttribute()]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fixprotocol.org/FIXML-5-0-SP2")]
    public enum DecaySplitEnum
    {
        [System.Xml.Serialization.XmlEnumAttribute("0")]
        Month,
        [System.Xml.Serialization.XmlEnumAttribute("3")]
        Quarter,
        [System.Xml.Serialization.XmlEnumAttribute("6")]
        Season,
        [System.Xml.Serialization.XmlEnumAttribute("12")]
        Year,
    }
    #endregion

    #region DepositTypeEnum
    /* DepositType(28890) */
    [System.SerializableAttribute()]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fixprotocol.org/FIXML-5-0-SP2")]
    public enum DepositTypeEnum
    {
        [System.Xml.Serialization.XmlEnumAttribute("1")]
        Auslandskassenverein,
        [System.Xml.Serialization.XmlEnumAttribute("2")]
        Girosammelverwahrung,
        [System.Xml.Serialization.XmlEnumAttribute("3")]
        Streifbandverwahrung,
        [System.Xml.Serialization.XmlEnumAttribute("4")]
        Wertpapierrechnung,
        [System.Xml.Serialization.XmlEnumAttribute("5")]
        NCSCviaT2S_NCSCT,
    }
    #endregion DepositTypeEnum

    #region DisplaySeasonEnum
    /* DisplaySeason (25214) */
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
    [System.SerializableAttribute()]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fixprotocol.org/FIXML-5-0-SP2")]
    public enum DisplaySeasonEnum
    {
        /// <summary>
        /// Summer
        /// </summary>
        SUM,

        /// <summary>
        /// Winter
        /// </summary>
        WIN,
    }
    #endregion

    #region EventTypeEnum
    /* EventType(865) */
    [System.SerializableAttribute()]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fixprotocol.org/FIXML-5-0-SP2")]
    public enum EventTypeEnum
    {
        [System.Xml.Serialization.XmlEnumAttribute("1")]
        Put,
        [System.Xml.Serialization.XmlEnumAttribute("2")]
        Call,
        [System.Xml.Serialization.XmlEnumAttribute("3")]
        Tender,
        [System.Xml.Serialization.XmlEnumAttribute("4")]
        SinkingFundCall,
        [System.Xml.Serialization.XmlEnumAttribute("5")]
        Activation,
        [System.Xml.Serialization.XmlEnumAttribute("6")]
        Inactiviation,
        [System.Xml.Serialization.XmlEnumAttribute("7")]
        LastEligibleTradeDate,
        [System.Xml.Serialization.XmlEnumAttribute("8")]
        SwapStartDate,
        [System.Xml.Serialization.XmlEnumAttribute("9")]
        SwapEndDate,
        [System.Xml.Serialization.XmlEnumAttribute("10")]
        SwapRollDate,
        [System.Xml.Serialization.XmlEnumAttribute("11")]
        SwapNextStartDate,
        [System.Xml.Serialization.XmlEnumAttribute("12")]
        SwapNextRollDate,
        [System.Xml.Serialization.XmlEnumAttribute("13")]
        FirstDeliveryDate,
        [System.Xml.Serialization.XmlEnumAttribute("14")]
        LastDeliveryDate,
        [System.Xml.Serialization.XmlEnumAttribute("15")]
        InitialInventoryDueDate,
        [System.Xml.Serialization.XmlEnumAttribute("16")]
        FinalInventoryDueDate,
        [System.Xml.Serialization.XmlEnumAttribute("17")]
        FirstIntentDate,
        [System.Xml.Serialization.XmlEnumAttribute("18")]
        LastIntentDate,
        [System.Xml.Serialization.XmlEnumAttribute("19")]
        PositionRemovalDate,
        [System.Xml.Serialization.XmlEnumAttribute("28")]
        TotalTradingDuration,
        [System.Xml.Serialization.XmlEnumAttribute("29")]
        CurrentTradingDuration,
        [System.Xml.Serialization.XmlEnumAttribute("99")]
        Other,
        [System.Xml.Serialization.XmlEnumAttribute("100")]
        FirstEligibleTradeDate,
        [System.Xml.Serialization.XmlEnumAttribute("101")]
        CapitalAdjustmentDate,
        [System.Xml.Serialization.XmlEnumAttribute("102")]
        DividendPaymentDate,
        [System.Xml.Serialization.XmlEnumAttribute("115")]
        FinalSettlementReferenceDate,
    }
    #endregion EventTypeEnum
    #region ExerciseStyleEnum
    /* ExerciseStyle(1194) */
    [System.SerializableAttribute()]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fixprotocol.org/FIXML-5-0-SP2")]
    public enum ExerciseStyleEnum
    {
        [System.Xml.Serialization.XmlEnumAttribute("0")]
        European,
        [System.Xml.Serialization.XmlEnumAttribute("1")]
        American,
        [System.Xml.Serialization.XmlEnumAttribute("2")]
        Bermuda,
    }
    #endregion ExerciseStyleEnum

    #region FlatIndicatorEnum
    /* CouponType (25170) */
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
    [System.SerializableAttribute()]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fixprotocol.org/FIXML-5-0-SP2")]
    public enum FlatIndicatorEnum
    {
        [System.Xml.Serialization.XmlEnumAttribute("1")]
        NoFlat,
        [System.Xml.Serialization.XmlEnumAttribute("2")]
        Flat,
        [System.Xml.Serialization.XmlEnumAttribute("3")]
        XFlat,
    }
    #endregion

    #region ImpliedMarketIndicatorEnum
    /* ImpliedMarketIndicator(1144) */
    [System.SerializableAttribute()]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fixprotocol.org/FIXML-5-0-SP2")]
    public enum ImpliedMarketIndicatorEnum
    {
        [System.Xml.Serialization.XmlEnumAttribute("0")]
        NotImplied,
        [System.Xml.Serialization.XmlEnumAttribute("1")]
        ImpliedIn,
        [System.Xml.Serialization.XmlEnumAttribute("2")]
        ImpliedOut,
        [System.Xml.Serialization.XmlEnumAttribute("3")]
        BothImpliedInAndImpliedOut,
    }
    #endregion ImpliedMarketIndicatorEnum
    #region InstrAttribTypeEnum
    /* Typ(871) */
    [System.SerializableAttribute()]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fixprotocol.org/FIXML-5-0-SP2")]
    public enum InstrAttribTypeEnum
    {
        [System.Xml.Serialization.XmlEnumAttribute("100")]
        MinimumReserveOrderQuantity,
        [System.Xml.Serialization.XmlEnumAttribute("101")]
        MinimumDisplayQuantity,
        [System.Xml.Serialization.XmlEnumAttribute("102")]
        IssuerName,
        [System.Xml.Serialization.XmlEnumAttribute("103")]
        IssuerNumber,
        [System.Xml.Serialization.XmlEnumAttribute("104")]
        MarketType,
        [System.Xml.Serialization.XmlEnumAttribute("105")]
        MarketTypeSupplement,
        [System.Xml.Serialization.XmlEnumAttribute("106")]
        ReportingMarket,
        [System.Xml.Serialization.XmlEnumAttribute("107")]
        SettlementCalendar,
        [System.Xml.Serialization.XmlEnumAttribute("108")]
        ProductAssignmentGroup,
        [System.Xml.Serialization.XmlEnumAttribute("109")]
        ProductAssignmentGroupDescription,
        [System.Xml.Serialization.XmlEnumAttribute("110")]
        DomesticIndicator,
        [System.Xml.Serialization.XmlEnumAttribute("111")]
        VDOMinimumExecutionVolume,
        [System.Xml.Serialization.XmlEnumAttribute("112")]
        IlliquidAsDefinedByExchange,
        [System.Xml.Serialization.XmlEnumAttribute("113")]
        MarketMakingObligation,
        [System.Xml.Serialization.XmlEnumAttribute("114")]
        LiquidAsDefinedByRegulator,
        [System.Xml.Serialization.XmlEnumAttribute("115")]
        EligibleForStressedMarketConditions,
        [System.Xml.Serialization.XmlEnumAttribute("116")]
        EligibleForSystematicInternaliser,
        [System.Xml.Serialization.XmlEnumAttribute("117")]
        MultiCCPEligible,
        [System.Xml.Serialization.XmlEnumAttribute("118")]
        PoolFactor,
        [System.Xml.Serialization.XmlEnumAttribute("119")]
        IndexationCoefficient,
        [System.Xml.Serialization.XmlEnumAttribute("120")]
        TradedBeforeIssueDate,
        [System.Xml.Serialization.XmlEnumAttribute("121")]
        IssuerBusinessUnit,
        [System.Xml.Serialization.XmlEnumAttribute("122")]
        AllowKnockOut,
        [System.Xml.Serialization.XmlEnumAttribute("123")]
        HasPassiveLiquidityProtection,
        [System.Xml.Serialization.XmlEnumAttribute("124")]
        PassiveLiquidityProtectionDeferralTime,
        [System.Xml.Serialization.XmlEnumAttribute("125")]
        WarrantStrike,
        [System.Xml.Serialization.XmlEnumAttribute("126")]
        ReportingMarketTES,
        [System.Xml.Serialization.XmlEnumAttribute("127")]
        LiquidityProviderUserGroup,
        [System.Xml.Serialization.XmlEnumAttribute("128")]
        SpecialistUserGroup,
        [System.Xml.Serialization.XmlEnumAttribute("129")]
        LiquidityClass
    }
    #endregion InstrAttribTypeEnum

    #region InstrumentAuctionTypeEnum
    /* InstrumentAuctionType (31803) */
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
    [System.SerializableAttribute()]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fixprotocol.org/FIXML-5-0-SP2")]
    public enum InstrumentAuctionTypeEnum
    {
        [System.Xml.Serialization.XmlEnumAttribute("0")]
        Default,
        [System.Xml.Serialization.XmlEnumAttribute("1")]
        SingleAuction,
        [System.Xml.Serialization.XmlEnumAttribute("2")]
        SpecialAuction,
    }
    #endregion
    #region InstrumentPartyIDSourceEnum
    /* Src(1050) */
    [System.SerializableAttribute()]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fixprotocol.org/FIXML-5-0-SP2")]
    public enum InstrumentPartyIDSourceEnum
    {
        [System.Xml.Serialization.XmlEnumAttribute("D")]
        Proprietary,
    }
    #endregion InstrumentPartyIDSourceEnum
    #region InstrumentScopeOperatorEnum
    /* InstrumentScopeOperator(1535) */
    [System.SerializableAttribute()]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fixprotocol.org/FIXML-5-0-SP2")]
    public enum InstrumentScopeOperatorEnum
    {
        [System.Xml.Serialization.XmlEnumAttribute("1")]
        Include,
    }
    #endregion InstrumentScopeOperatorEnum

    #region IsPrimaryFlagEnum
    /* IsPrimaryFlag (25216) */
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
    [System.SerializableAttribute()]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fixprotocol.org/FIXML-5-0-SP2")]
    public enum IsPrimaryFlagEnum
    {

        /// <summary>
        /// No
        /// </summary>
        N,

        /// <summary>
        /// yes
        /// </summary>
        Y,
    }
    #endregion

    #region LegSecurityIDSourceEnum
    /* LegSecurityIDSource(603) */
    [System.SerializableAttribute()]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fixprotocol.org/FIXML-5-0-SP2")]
    public enum LegSecurityIDSourceEnum
    {
        [System.Xml.Serialization.XmlEnumAttribute("1")]
        CUSIP,
        [System.Xml.Serialization.XmlEnumAttribute("2")]
        SEDOL,
        [System.Xml.Serialization.XmlEnumAttribute("3")]
        QUIK,
        [System.Xml.Serialization.XmlEnumAttribute("4")]
        ISIN,
        [System.Xml.Serialization.XmlEnumAttribute("5")]
        RIC,
        [System.Xml.Serialization.XmlEnumAttribute("6")]
        ISOCurrencyCode,
        [System.Xml.Serialization.XmlEnumAttribute("7")]
        ISOCountryCode,
        [System.Xml.Serialization.XmlEnumAttribute("8")]
        ExchangeSymbol,
        [System.Xml.Serialization.XmlEnumAttribute("9")]
        CTASymbol,
        [System.Xml.Serialization.XmlEnumAttribute("A")]
        BloombergSymbol,
        [System.Xml.Serialization.XmlEnumAttribute("B")]
        Wertpapier,
        [System.Xml.Serialization.XmlEnumAttribute("C")]
        Dutch,
        [System.Xml.Serialization.XmlEnumAttribute("D")]
        Valoren,
        [System.Xml.Serialization.XmlEnumAttribute("E")]
        Sicovam,
        [System.Xml.Serialization.XmlEnumAttribute("F")]
        Belgian,
        [System.Xml.Serialization.XmlEnumAttribute("G")]
        ClearstreamEuroclear,
        [System.Xml.Serialization.XmlEnumAttribute("H")]
        ClearingHouseOrganization,
        [System.Xml.Serialization.XmlEnumAttribute("I")]
        FpMLSpecification,
        [System.Xml.Serialization.XmlEnumAttribute("J")]
        OptionPriceReportingAuthority,
        [System.Xml.Serialization.XmlEnumAttribute("K")]
        FpMLURL,
        [System.Xml.Serialization.XmlEnumAttribute("L")]
        LetterOfCredit,
        [System.Xml.Serialization.XmlEnumAttribute("M")]
        MarketplaceAssignedIdentifier,
    }
    #endregion LegSecurityIDSourceEnum
    #region LegSecurityTypeEnum
    /* LegSecurityType(609) */
    [System.SerializableAttribute()]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fixprotocol.org/FIXML-5-0-SP2")]
    public enum LegSecurityTypeEnum
    {
        /// <summary>
        /// Part of a multi-leg instrument
        /// </summary>
        MLEG,
        /// <summary>
        /// Underlying leg
        /// </summary>
        ULEG,
    }
    #endregion LegSecurityTypeEnum

    #region LegSideEnum
    /* LegSide(624) */
    [System.SerializableAttribute()]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fixprotocol.org/FIXML-5-0-SP2")]
    public enum LegSideEnum
    {
        [System.Xml.Serialization.XmlEnumAttribute("1")]
        Buy,
        [System.Xml.Serialization.XmlEnumAttribute("2")]
        Sell,
        [System.Xml.Serialization.XmlEnumAttribute("3")]
        BuyMin,
        [System.Xml.Serialization.XmlEnumAttribute("4")]
        SellPlus,
        [System.Xml.Serialization.XmlEnumAttribute("5")]
        SellSht,
        [System.Xml.Serialization.XmlEnumAttribute("6")]
        SellShtEx,
        [System.Xml.Serialization.XmlEnumAttribute("7")]
        Undisc,
        [System.Xml.Serialization.XmlEnumAttribute("8")]
        Cross,
        [System.Xml.Serialization.XmlEnumAttribute("9")]
        CrossShort,
        [System.Xml.Serialization.XmlEnumAttribute("A")]
        CrossShortEx,
        [System.Xml.Serialization.XmlEnumAttribute("B")]
        AsDefined,
        [System.Xml.Serialization.XmlEnumAttribute("C")]
        Opposite,
        [System.Xml.Serialization.XmlEnumAttribute("D")]
        Subscribe,
        [System.Xml.Serialization.XmlEnumAttribute("E")]
        Redeem,
        [System.Xml.Serialization.XmlEnumAttribute("F")]
        LendFinancing,
        [System.Xml.Serialization.XmlEnumAttribute("G")]
        BorrowFinancing,
    }
    #endregion LegSideEnum

    #region MarketSegmentRelationshipEnum
    /* MarketSegmentRelationship(2547) */
    [System.SerializableAttribute()]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fixprotocol.org/FIXML-5-0-SP2")]
    public enum MarketSegmentRelationshipEnum
    {
        [System.Xml.Serialization.XmlEnumAttribute("100")]
        CashLegForVolatilityStrategies,
        [System.Xml.Serialization.XmlEnumAttribute("101")]
        TargetProductForDecayingProduct,
        [System.Xml.Serialization.XmlEnumAttribute("102")]
        BTRFBucket,
        [System.Xml.Serialization.XmlEnumAttribute("103")]
        EBBBucket,
    }
    #endregion MarketSegmentRelationshipEnum
    #region MarketSegmentStatusEnum
    /// <summary>
    ///    Status of market segment.
    /// </summary>
    /* MarketSegmentStatus(2542) */
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
    [System.SerializableAttribute()]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fixprotocol.org/FIXML-5-0-SP2")]
    public enum MarketSegmentStatusEnum
    {
        [System.Xml.Serialization.XmlEnumAttribute("1")]
        Active,
        [System.Xml.Serialization.XmlEnumAttribute("3")]
        Published,
    }
    #endregion 
    #region MarketSegmentSubTypeEnum
    /* MarketSegmentSubType(2545) */
    [System.SerializableAttribute()]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fixprotocol.org/FIXML-5-0-SP2")]
    public enum MarketSegmentSubTypeEnum
    {
        [System.Xml.Serialization.XmlEnumAttribute("1")]
        InterProductSpread,
        [System.Xml.Serialization.XmlEnumAttribute("2")]
        BTRFBucket,
        [System.Xml.Serialization.XmlEnumAttribute("3")]
        EBBBucket,
    }
    #endregion MarketSegmentSubTypeEnum
    #region MarketSegmentTypeEnum
    /* MarketSegmentType(2543) */
    [System.SerializableAttribute()]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fixprotocol.org/FIXML-5-0-SP2")]
    public enum MarketSegmentTypeEnum
    {
        [System.Xml.Serialization.XmlEnumAttribute("1")]
        Pool,
    }
    #endregion MarketSegmentTypeEnum

    #region MatchTypeEnum
    /* MatchType(574) */
    [System.SerializableAttribute()]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fixprotocol.org/FIXML-5-0-SP2")]
    public enum MatchTypeEnum
    {
        [System.Xml.Serialization.XmlEnumAttribute("1")]
        OnePartyTradeReport,
        [System.Xml.Serialization.XmlEnumAttribute("2")]
        TwoPartyTradeReport,
        [System.Xml.Serialization.XmlEnumAttribute("3")]
        ConfirmedTradeReport,
        [System.Xml.Serialization.XmlEnumAttribute("4")]
        AutoMatch,
        [System.Xml.Serialization.XmlEnumAttribute("5")]
        CrossAuction,
        [System.Xml.Serialization.XmlEnumAttribute("6")]
        CounterOrderSelection,
        [System.Xml.Serialization.XmlEnumAttribute("7")]
        CallAuction,
        [System.Xml.Serialization.XmlEnumAttribute("8")]
        IssuingBuyBackAuction,
        [System.Xml.Serialization.XmlEnumAttribute("A1")]
        ExactMatchA1,
        [System.Xml.Serialization.XmlEnumAttribute("A2")]
        ExactMatchA2,
        [System.Xml.Serialization.XmlEnumAttribute("A3")]
        ExactMatchA3,
        [System.Xml.Serialization.XmlEnumAttribute("A4")]
        ExactMatchA4,
        [System.Xml.Serialization.XmlEnumAttribute("A5")]
        ExactMatchA5,
        [System.Xml.Serialization.XmlEnumAttribute("AQ")]
        AcceptsPairOffs,
        [System.Xml.Serialization.XmlEnumAttribute("M1")]
        ExactMatch,
        [System.Xml.Serialization.XmlEnumAttribute("M2")]
        ACTM2Match,
        [System.Xml.Serialization.XmlEnumAttribute("M3")]
        ACTAcceptedTrade,
        [System.Xml.Serialization.XmlEnumAttribute("M4")]
        ACTDefaultTrade,
        [System.Xml.Serialization.XmlEnumAttribute("M5")]
        ACTDefaultAfterM2,
        [System.Xml.Serialization.XmlEnumAttribute("M6")]
        ACTM6Match,
        [System.Xml.Serialization.XmlEnumAttribute("MT")]
        OCSLockedInNonACT,
        [System.Xml.Serialization.XmlEnumAttribute("S1")]
        SummarizedMatchUsingA1,
        [System.Xml.Serialization.XmlEnumAttribute("S2")]
        SummarizedMatchUsingA2,
        [System.Xml.Serialization.XmlEnumAttribute("S3")]
        SummarizedMatchUsingA3,
        [System.Xml.Serialization.XmlEnumAttribute("S4")]
        SummarizedMatchUsingA4,
        [System.Xml.Serialization.XmlEnumAttribute("S5")]
        SummarizedMatchUsingA5,
    }
    #endregion MatchTypeEnum

    #region MDBookTypeEnum
    /* MDBookType(1021) */
    [System.SerializableAttribute()]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fixprotocol.org/FIXML-5-0-SP2")]
    public enum MDBookTypeEnum
    {
        [System.Xml.Serialization.XmlEnumAttribute("2")]
        PriceDepth,
        [System.Xml.Serialization.XmlEnumAttribute("3")]
        OrderDepth,
    }
    #endregion MDBookTypeEnum

    #region MultilegModelEnum
    /* MultilegModel(1377) */
    [System.SerializableAttribute()]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fixprotocol.org/FIXML-5-0-SP2")]
    public enum MultilegModelEnum
    {
        [System.Xml.Serialization.XmlEnumAttribute("0")]
        PredefinedMultilegSecurity,
        [System.Xml.Serialization.XmlEnumAttribute("1")]
        UserdefinedMultilegSecurity,
        [System.Xml.Serialization.XmlEnumAttribute("2")]
        UserdefinedNonSecuritizedMultileg,
    }
    #endregion MultilegModelEnum

    #region PostTradeAnonymityEnum
    /* ImpliedMarketIndicator(28876) */
    [System.SerializableAttribute()]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fixprotocol.org/FIXML-5-0-SP2")]
    public enum PostTradeAnonymityEnum
    {
        [System.Xml.Serialization.XmlEnumAttribute("0")]
        Disabled,
        [System.Xml.Serialization.XmlEnumAttribute("1")]
        Enabled,
        [System.Xml.Serialization.XmlEnumAttribute("2")]
        CentralCounterparty,
    }
    #endregion PostTradeAnonymityEnum

    #region PriceTypeEnum
    /* PriceType(423) */
    [System.SerializableAttribute()]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fixprotocol.org/FIXML-5-0-SP2")]
    public enum PriceTypeEnum
    {
        [System.Xml.Serialization.XmlEnumAttribute("1")]
        Percent,
        [System.Xml.Serialization.XmlEnumAttribute("2")]
        Shares,
        [System.Xml.Serialization.XmlEnumAttribute("22")]
        Points,
    }
    #endregion PriceTypeEnum

    #region ProductComplexEnum
    /* ProductComplex(1227) */
    [System.SerializableAttribute()]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fixprotocol.org/FIXML-5-0-SP2")]
    public enum ProductComplexEnum
    {
        [System.Xml.Serialization.XmlEnumAttribute("1")]
        SimpleInstrument,
        [System.Xml.Serialization.XmlEnumAttribute("2")]
        StandardOptionStrategy,
        [System.Xml.Serialization.XmlEnumAttribute("3")]
        NonStandardOptionStrategy,
        [System.Xml.Serialization.XmlEnumAttribute("4")]
        VolatilityStrategy,
        [System.Xml.Serialization.XmlEnumAttribute("5")]
        FuturesSpread,
        [System.Xml.Serialization.XmlEnumAttribute("6")]
        InterProductSpread,
        [System.Xml.Serialization.XmlEnumAttribute("7")]
        StandardFuturesStrategy,
        [System.Xml.Serialization.XmlEnumAttribute("8")]
        PackAndBundle,
        [System.Xml.Serialization.XmlEnumAttribute("9")]
        Strip,
        [System.Xml.Serialization.XmlEnumAttribute("10")]
        FlexibleInstrument,
    }
    #endregion ProductComplexEnum

    #region PutOrCallEnum
    /* PutOrCall(201) */
    [System.SerializableAttribute()]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fixprotocol.org/FIXML-5-0-SP2")]
    public enum PutOrCallEnum
    {
        [System.Xml.Serialization.XmlEnumAttribute("0")]
        Put,
        [System.Xml.Serialization.XmlEnumAttribute("1")]
        Call,
    }
    #endregion PutOrCallEnum

    #region QuoteSideIndicatorEnum
    /* QuoteSideIndicator(2559) */
    [System.SerializableAttribute()]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fixprotocol.org/FIXML-5-0-SP2")]
    public enum QuoteSideIndicatorEnum
    {
        [System.Xml.Serialization.XmlEnumAttribute("0")]
        Item0,
        [System.Xml.Serialization.XmlEnumAttribute("1")]
        Item1,
    }
    #endregion QuoteSideIndicatorEnum
    #region QuoteSideModelTypeEnum
    /* QuoteSideModelType(28898) */
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
    [System.SerializableAttribute()]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fixprotocol.org/FIXML-5-0-SP2")]
    public enum QuoteSideModelTypeEnum
    {

        /// <summary>
        /// Single Sided Quote not supported
        /// </summary>
        [System.Xml.Serialization.XmlEnumAttribute("0")]
        SingleSidedQuoteNotSupported,

        /// <summary>
        /// Single Sided Quote supported
        /// </summary>
        [System.Xml.Serialization.XmlEnumAttribute("1")]
        SingleSidedQuoteSupported,
    }
    #endregion

    #region SecurityAltIDSourceEnum
    /* SecurityAltID(455) */
    [System.SerializableAttribute()]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fixprotocol.org/FIXML-5-0-SP2")]
    public enum SecurityAltIDSourceEnum
    {
        [System.Xml.Serialization.XmlEnumAttribute("1")]
        CUSIP,
        [System.Xml.Serialization.XmlEnumAttribute("2")]
        SEDOL,
        [System.Xml.Serialization.XmlEnumAttribute("3")]
        QUIK,
        [System.Xml.Serialization.XmlEnumAttribute("4")]
        ISIN,
        [System.Xml.Serialization.XmlEnumAttribute("5")]
        RIC,
        [System.Xml.Serialization.XmlEnumAttribute("6")]
        ISOCurrencyCode,
        [System.Xml.Serialization.XmlEnumAttribute("7")]
        ISOCountryCode,
        [System.Xml.Serialization.XmlEnumAttribute("8")]
        ExchangeSymbol,
        [System.Xml.Serialization.XmlEnumAttribute("9")]
        CTASymbol,
        [System.Xml.Serialization.XmlEnumAttribute("A")]
        BloombergSymbol,
        [System.Xml.Serialization.XmlEnumAttribute("B")]
        Wertpapier,
        [System.Xml.Serialization.XmlEnumAttribute("C")]
        Dutch,
        [System.Xml.Serialization.XmlEnumAttribute("D")]
        Valoren,
        [System.Xml.Serialization.XmlEnumAttribute("E")]
        Sicovam,
        [System.Xml.Serialization.XmlEnumAttribute("F")]
        Belgian,
        [System.Xml.Serialization.XmlEnumAttribute("G")]
        ClearstreamEuroclear,
        [System.Xml.Serialization.XmlEnumAttribute("H")]
        ClearingHouseOrganization,
        [System.Xml.Serialization.XmlEnumAttribute("I")]
        FpMLSpecification,
        [System.Xml.Serialization.XmlEnumAttribute("J")]
        OptionPriceReportingAuthority,
        [System.Xml.Serialization.XmlEnumAttribute("K")]
        FpMLURL,
        [System.Xml.Serialization.XmlEnumAttribute("L")]
        LetterOfCredit,
        [System.Xml.Serialization.XmlEnumAttribute("M")]
        MarketplaceAssignedIdentifier,
        [System.Xml.Serialization.XmlEnumAttribute("101")]
        DisplayNameClearingInstrumentName,
        [System.Xml.Serialization.XmlEnumAttribute("102")]
        RedundantToSecurityID,
    }
    #endregion SecurityAltIDSourceEnum
    #region SecurityIDSourceEnum
    /* SecurityIDSource(22) */
    [System.SerializableAttribute()]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fixprotocol.org/FIXML-5-0-SP2")]
    public enum SecurityIDSourceEnum
    {
        [System.Xml.Serialization.XmlEnumAttribute("1")]
        CUSIP,
        [System.Xml.Serialization.XmlEnumAttribute("2")]
        SEDOL,
        [System.Xml.Serialization.XmlEnumAttribute("3")]
        QUIK,
        [System.Xml.Serialization.XmlEnumAttribute("4")]
        ISIN,
        [System.Xml.Serialization.XmlEnumAttribute("5")]
        RIC,
        [System.Xml.Serialization.XmlEnumAttribute("6")]
        ISOCurrencyCode,
        [System.Xml.Serialization.XmlEnumAttribute("7")]
        ISOCountryCode,
        [System.Xml.Serialization.XmlEnumAttribute("8")]
        ExchangeSymbol,
        [System.Xml.Serialization.XmlEnumAttribute("9")]
        CTASymbol,
        [System.Xml.Serialization.XmlEnumAttribute("A")]
        BloombergSymbol,
        [System.Xml.Serialization.XmlEnumAttribute("B")]
        Wertpapier,
        [System.Xml.Serialization.XmlEnumAttribute("C")]
        Dutch,
        [System.Xml.Serialization.XmlEnumAttribute("D")]
        Valoren,
        [System.Xml.Serialization.XmlEnumAttribute("E")]
        Sicovam,
        [System.Xml.Serialization.XmlEnumAttribute("F")]
        Belgian,
        [System.Xml.Serialization.XmlEnumAttribute("G")]
        ClearstreamEuroclear,
        [System.Xml.Serialization.XmlEnumAttribute("H")]
        ClearingHouseOrganization,
        [System.Xml.Serialization.XmlEnumAttribute("I")]
        FpMLSpecification,
        [System.Xml.Serialization.XmlEnumAttribute("J")]
        OptionPriceReportingAuthority,
        [System.Xml.Serialization.XmlEnumAttribute("K")]
        FpMLURL,
        [System.Xml.Serialization.XmlEnumAttribute("L")]
        LetterOfCredit,
        [System.Xml.Serialization.XmlEnumAttribute("M")]
        MarketplaceAssignedIdentifier,
    }
    #endregion SecurityIDSourceEnum
    #region SecurityStatusEnum
    /* SecurityStatus(965) */
    [System.SerializableAttribute()]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fixprotocol.org/FIXML-5-0-SP2")]
    public enum SecurityStatusEnum
    {
        [System.Xml.Serialization.XmlEnumAttribute("1")]
        Active,
        [System.Xml.Serialization.XmlEnumAttribute("2")]
        Inactive,
        [System.Xml.Serialization.XmlEnumAttribute("10")]
        Published,
        [System.Xml.Serialization.XmlEnumAttribute("11")]
        PendingDeletion,
    }
    #endregion SecurityStatusEnum
    #region SecurityTypeEnum
    /* SecurityType(167) */
    [System.SerializableAttribute()]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fixprotocol.org/FIXML-5-0-SP2")]
    public enum SecurityTypeEnum
    {
        [System.Xml.Serialization.XmlEnumAttribute("?")]
        Wildcard,
        ABS,
        AMENDED,
        AN,
        BA,
        BDN,
        BN,
        BOX,
        BOND,
        BRADY,
        BRIDGE,
        BUYSELL,
        CAMM,
        CAN,
        CASH,
        CB,
        CD,
        CDS,
        CL,
        CMB,
        CMBS,
        CMO,
        COFO,
        COFP,
        CORP,
        CP,
        CPP,
        CS,
        CTB,
        DEFLTED,
        DINP,
        DN,
        DUAL,
        ETC,
        ETF,
        ETN,
        EUCD,
        EUCORP,
        EUCP,
        EUFRN,
        EUSOV,
        EUSUPRA,
        FAC,
        FADN,
        FOR,
        FORWARD,
        FRN,
        FUT,
        FXFWD,
        FXNDF,
        FXSPOT,
        FXSWAP,
        GO,
        IET,
        IRS,
        LOFC,
        LQN,
        MATURED,
        MBS,
        MF,
        MIO,
        MLEG,
        MPO,
        MPP,
        MPT,
        MT,
        MTN,
        NONE,
        ONITE,
        OOC,
        OOF,
        OOP,
        OPT,
        OTHER,
        PEF,
        PFAND,
        PN,
        PROV,
        PS,
        PZFJ,
        RAN,
        REPLACD,
        REPO,
        RETIRED,
        REV,
        RVLV,
        RVLVTRM,
        SECLOAN,
        SECPLEDGE,
        SLQN,
        SPCLA,
        SPCLO,
        SPCLT,
        STN,
        STRUCT,
        SUPRA,
        SWING,
        TAN,
        TARP,
        TAXA,
        TB,
        TBA,
        TBILL,
        TBOND,
        TCAL,
        TD,
        TECP,
        TERM,
        TINT,
        TIPS,
        TLQN,
        TMCP,
        TNOTE,
        TPRN,
        TRAN,
        TRF,
        UST,
        USTB,
        VAR,
        VRDN,
        WAR,
        WITHDRN,
        XCN,
        XLINKD,
        YANK,
        YCD,
    }
    #endregion SecurityTypeEnum
    #region SecurityUpdateActionEnum
    /* SecurityUpdateAction (980) */
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
    [System.SerializableAttribute()]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fixprotocol.org/FIXML-5-0-SP2")]
    public enum SecurityUpdateActionEnum
    {

        /// <summary>
        /// Add
        /// </summary>
        A,

        /// <remarks/>
        M,
    }
    #endregion

    #region TradingSessionIdEnum
    /* TradingSessionID(336) */
    [System.SerializableAttribute()]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fixprotocol.org/FIXML-5-0-SP2")]
    public enum TradingSessionIdEnum
    {
        [System.Xml.Serialization.XmlEnumAttribute("1")]
        Day,
    }
    #endregion TradingSessionIdEnum
    #region TradingSessionSubIdEnum
    /* TradingSessionSubID(625) */
    [System.SerializableAttribute()]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fixprotocol.org/FIXML-5-0-SP2")]
    public enum TradingSessionSubIdEnum
    {
        [System.Xml.Serialization.XmlEnumAttribute("3")]
        Continuous,

        [System.Xml.Serialization.XmlEnumAttribute("6")]
        ScheduledIntradayAuction,

        [System.Xml.Serialization.XmlEnumAttribute("8")]
        AnyAuction,
    }
    #endregion TradingSessionSubIdEnum

    #region TrdTypeEnum
    /// <summary>
    /// TrdType(828)
    /// </summary>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
    [System.SerializableAttribute()]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fixprotocol.org/FIXML-5-0-SP2")]
    public enum TrdTypeEnum
    {
        [System.Xml.Serialization.XmlEnumAttribute("0")]
        RegularTrade,
        [System.Xml.Serialization.XmlEnumAttribute("1")]
        Block,
        [System.Xml.Serialization.XmlEnumAttribute("12")]
        ExchangeForSwap,
        [System.Xml.Serialization.XmlEnumAttribute("50")]
        PortfolioTrade,
        [System.Xml.Serialization.XmlEnumAttribute("54")]
        OTC,
        [System.Xml.Serialization.XmlEnumAttribute("1000")]
        Vola,
        [System.Xml.Serialization.XmlEnumAttribute("1001")]
        EFPFin,
        [System.Xml.Serialization.XmlEnumAttribute("1002")]
        EFPIdx,
        [System.Xml.Serialization.XmlEnumAttribute("1004")]
        BlockTAM,
        [System.Xml.Serialization.XmlEnumAttribute("1005")]
        LIS,
        [System.Xml.Serialization.XmlEnumAttribute("1006")]
        XetraEurexEnlghtTriggeresTrade,
        [System.Xml.Serialization.XmlEnumAttribute("1007")]
        BlockQTPIP,
    }
    #endregion TrdTypeEnum

    #region UnderlyingInstrumentSecurityIDSourceEnum
    /// <summary>
    /// Identifies class or source of the SecurityID (48) value. Required if SecurityID is specified.
    /// 100+ are reserved for private security identifications
    /// </summary>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
    [System.SerializableAttribute()]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fixprotocol.org/FIXML-5-0-SP2")]
    public enum UnderlyingInstrumentSecurityIDSourceEnum
    {
        /// <summary>
        /// Marketplace-assigned Identifier
        /// </summary>
        M,
    }
    #endregion
    #region UnderlyingSecurityIDSourceEnum
    /* SecurityIDSource(30305) */
    [System.SerializableAttribute()]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fixprotocol.org/FIXML-5-0-SP2")]
    public enum UnderlyingSecurityIDSourceEnum
    {
        [System.Xml.Serialization.XmlEnumAttribute("1")]
        CUSIP,
        [System.Xml.Serialization.XmlEnumAttribute("2")]
        SEDOL,
        [System.Xml.Serialization.XmlEnumAttribute("3")]
        QUIK,
        [System.Xml.Serialization.XmlEnumAttribute("4")]
        ISIN,
        [System.Xml.Serialization.XmlEnumAttribute("5")]
        RIC,
        [System.Xml.Serialization.XmlEnumAttribute("6")]
        ISOCurrencyCode,
        [System.Xml.Serialization.XmlEnumAttribute("7")]
        ISOCountryCode,
        [System.Xml.Serialization.XmlEnumAttribute("8")]
        ExchangeSymbol,
        [System.Xml.Serialization.XmlEnumAttribute("9")]
        CTASymbol,
        [System.Xml.Serialization.XmlEnumAttribute("A")]
        BloombergSymbol,
        [System.Xml.Serialization.XmlEnumAttribute("B")]
        Wertpapier,
        [System.Xml.Serialization.XmlEnumAttribute("C")]
        Dutch,
        [System.Xml.Serialization.XmlEnumAttribute("D")]
        Valoren,
        [System.Xml.Serialization.XmlEnumAttribute("E")]
        Sicovam,
        [System.Xml.Serialization.XmlEnumAttribute("F")]
        Belgian,
        [System.Xml.Serialization.XmlEnumAttribute("G")]
        ClearstreamEuroclear,
        [System.Xml.Serialization.XmlEnumAttribute("H")]
        ClearingHouseOrganization,
        [System.Xml.Serialization.XmlEnumAttribute("I")]
        FpMLSpecification,
        [System.Xml.Serialization.XmlEnumAttribute("J")]
        OptionPriceReportingAuthority,
        [System.Xml.Serialization.XmlEnumAttribute("K")]
        FpMLURL,
        [System.Xml.Serialization.XmlEnumAttribute("L")]
        LetterOfCredit,
        [System.Xml.Serialization.XmlEnumAttribute("M")]
        MarketplaceAssignedIdentifier,
    }
    #endregion

    #region USApprovalEnum
    /* USApproval(9543) */
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
    [System.SerializableAttribute()]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fixprotocol.org/FIXML-5-0-SP2")]
    public enum USApprovalEnum
    {
        None,
        CFTC,
        SEC,
    }
    #endregion 

    #region WarrantTypeEnum
    /* WarrantType (30762) */
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
    [System.SerializableAttribute()]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fixprotocol.org/FIXML-5-0-SP2")]
    public enum WarrantTypeEnum
    {
        [System.Xml.Serialization.XmlEnumAttribute("1")]
        Call,
        [System.Xml.Serialization.XmlEnumAttribute("2")]
        Put,
        [System.Xml.Serialization.XmlEnumAttribute("3")]
        Range,
        [System.Xml.Serialization.XmlEnumAttribute("4")]
        Certificate,
        [System.Xml.Serialization.XmlEnumAttribute("5")]
        Other,
    }
    #endregion
}
