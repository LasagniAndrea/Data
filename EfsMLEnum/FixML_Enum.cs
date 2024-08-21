#region using directives
using System;
using System.Xml.Serialization;
using System.ComponentModel;
#endregion using directives

namespace FixML.Enum
{
    #region Common Enums 4.4 & 5.0 SP1
    #region AcctIDSourceEnum
    [System.Xml.Serialization.XmlTypeAttribute()]
    public enum AcctIDSourceEnum
    {
        [System.Xml.Serialization.XmlEnumAttribute("1")]
        BIC,
        [System.Xml.Serialization.XmlEnumAttribute("2")]
        SID,
        [System.Xml.Serialization.XmlEnumAttribute("3")]
        TFM,
        [System.Xml.Serialization.XmlEnumAttribute("4")]
        OMGEO,
        [System.Xml.Serialization.XmlEnumAttribute("5")]
        DTCC,
        [System.Xml.Serialization.XmlEnumAttribute("99")]
        Other,
    }
    #endregion AcctIDSourceEnum
    #region AccountTypeEnum
    /* AccountType(581) */
    [System.SerializableAttribute()]
    public enum AccountTypeEnum
    {
        [System.Xml.Serialization.XmlEnumAttribute("1")]
        AccountCustomer,
        [System.Xml.Serialization.XmlEnumAttribute("2")]
        AccountNonCustomer,
        [System.Xml.Serialization.XmlEnumAttribute("3")]
        HouseTrader,
        [System.Xml.Serialization.XmlEnumAttribute("4")]
        FloorTrader,
        [System.Xml.Serialization.XmlEnumAttribute("6")]
        AccountNonCustomerCross,
        [System.Xml.Serialization.XmlEnumAttribute("7")]
        HouseTraderCross,
        [System.Xml.Serialization.XmlEnumAttribute("8")]
        JointBOAcct,
    }
    #endregion AccountTypeEnum
    #region ClearingFeeIndicatorEnum
    /* ClearingFeeIndicator(635) */
    [System.SerializableAttribute()]
    public enum ClearingFeeIndicatorEnum
    {
        [System.Xml.Serialization.XmlEnumAttribute("1")]
        FirstYearDelegate,
        [System.Xml.Serialization.XmlEnumAttribute("2")]
        SecondYearDelegate,
        [System.Xml.Serialization.XmlEnumAttribute("3")]
        ThirdYearDelegate,
        [System.Xml.Serialization.XmlEnumAttribute("4")]
        FourthYearDelegate,
        [System.Xml.Serialization.XmlEnumAttribute("5")]
        FifthYearDelegate,
        [System.Xml.Serialization.XmlEnumAttribute("9")]
        SixthYearDelegate,
        [System.Xml.Serialization.XmlEnumAttribute("B")]
        CBOEMember,
        [System.Xml.Serialization.XmlEnumAttribute("C")]
        NonMemberCustomer,
        [System.Xml.Serialization.XmlEnumAttribute("E")]
        EquityClearingMember,
        [System.Xml.Serialization.XmlEnumAttribute("F")]
        FullAssociateMember,
        [System.Xml.Serialization.XmlEnumAttribute("H")]
        Firms106H106J,
        [System.Xml.Serialization.XmlEnumAttribute("I")]
        GIMIDEMCOMMembership,
        [System.Xml.Serialization.XmlEnumAttribute("L")]
        Lessee106F,
        [System.Xml.Serialization.XmlEnumAttribute("M")]
        AllOthers,
    }
    #endregion ClearingFeeIndicatorEnum
    #region CommTypeEnum
    /* CommType(13) */
    [System.SerializableAttribute()]
    public enum CommTypeEnum
    {
        [System.Xml.Serialization.XmlEnumAttribute("1")]
        PerUnit,
        [System.Xml.Serialization.XmlEnumAttribute("2")]
        Percent,
        [System.Xml.Serialization.XmlEnumAttribute("3")]
        Absolute,
        [System.Xml.Serialization.XmlEnumAttribute("4")]
        PctWaivedCshDisc,
        [System.Xml.Serialization.XmlEnumAttribute("5")]
        PctWaivedEnUnits,
        [System.Xml.Serialization.XmlEnumAttribute("6")]
        PerBond,
    }
    #endregion CommTypeEnum
    #region ContAmtTypeEnum
    /* ContAmtType(519) */
    [System.SerializableAttribute()]
    public enum ContAmtTypeEnum
    {
        [System.Xml.Serialization.XmlEnumAttribute("1")]
        CommissionAmt,
        [System.Xml.Serialization.XmlEnumAttribute("2")]
        CommissionPct,
        [System.Xml.Serialization.XmlEnumAttribute("3")]
        InitialChargeAmt,
        [System.Xml.Serialization.XmlEnumAttribute("4")]
        InitialChargePct,
        [System.Xml.Serialization.XmlEnumAttribute("5")]
        DiscountAmt,
        [System.Xml.Serialization.XmlEnumAttribute("6")]
        DiscountPct,
        [System.Xml.Serialization.XmlEnumAttribute("7")]
        DilutionLevyAmt,
        [System.Xml.Serialization.XmlEnumAttribute("8")]
        DilutionLevyPct,
        [System.Xml.Serialization.XmlEnumAttribute("9")]
        ExitChargeAmt,
        [System.Xml.Serialization.XmlEnumAttribute("10")]
        ExitChargePct,
        [System.Xml.Serialization.XmlEnumAttribute("11")]
        FundBasedRenewalComm,
        [System.Xml.Serialization.XmlEnumAttribute("12")]
        ProjectedFundValue,
        [System.Xml.Serialization.XmlEnumAttribute("13")]
        FundBasedRenewalCommAmtOrd,
        [System.Xml.Serialization.XmlEnumAttribute("14")]
        FundBasedRenewalCommAmtProj,
        [System.Xml.Serialization.XmlEnumAttribute("15")]
        NetSettlementAmount,
    }
    #endregion ContAmtTypeEnum
    #region CPProgramEnum
/* 
 * Supprimé car Existant dans EfsML.Enum
 * 
    [System.Xml.Serialization.XmlTypeAttribute()]
    public enum CPProgramEnum
    {
        [System.Xml.Serialization.XmlEnumAttribute("1")]
        Program3a3,
        [System.Xml.Serialization.XmlEnumAttribute("2")]
        Program42,
        [System.Xml.Serialization.XmlEnumAttribute("99")]
        Other,
    }
*/
    #endregion CPProgramEnum
    #region CustOrderCapacityEnum
    /* CustOrderCapacity(582) */
    [System.SerializableAttribute()]
    public enum CustOrderCapacityEnum
    {
        [System.Xml.Serialization.XmlEnumAttribute("1")]
        MemberTradingForTheirOwnAccount,
        [System.Xml.Serialization.XmlEnumAttribute("2")]
        ClearingFirmTradingForItsProprietaryAccount,
        [System.Xml.Serialization.XmlEnumAttribute("3")]
        MemberTradingForAnotherMember,
        [System.Xml.Serialization.XmlEnumAttribute("4")]
        AllOther,
    }
    #endregion CustOrderCapacityEnum
    #region DeliveryTypeEnum
    /* DeliveryType(919) */
    [System.SerializableAttribute()]
    public enum DeliveryTypeEnum
    {
        [System.Xml.Serialization.XmlEnumAttribute("0")]
        VersusPayment,
        [System.Xml.Serialization.XmlEnumAttribute("1")]
        Free,
        [System.Xml.Serialization.XmlEnumAttribute("2")]
        TriParty,
        [System.Xml.Serialization.XmlEnumAttribute("3")]
        HoldInCustody,
    }
    #endregion DeliveryTypeEnum
    #region InstrRegistryEnum
    [System.Xml.Serialization.XmlTypeAttribute()]
    public enum InstrRegistryEnum
    {
        [System.Xml.Serialization.XmlEnumAttribute("BIC")]
        Custodian,
        [System.Xml.Serialization.XmlEnumAttribute("ISO")]
        Country,
        [System.Xml.Serialization.XmlEnumAttribute("ZZ")]
        Physical,
    }
    #endregion InstrRegistryEnum
    #region LegCoveredOrUncoveredEnum
    /* LegCoveredOrUncovered(565) */
    [System.SerializableAttribute()]
    public enum LegCoveredOrUncoveredEnum
    {
        [System.Xml.Serialization.XmlEnumAttribute("0")]
        Covered,
        [System.Xml.Serialization.XmlEnumAttribute("1")]
        Uncovered,
    }
    #endregion LegCoveredOrUncoveredEnum
    #region LegSwapTypeEnum
    /* LegSwapType(690) */
    [System.SerializableAttribute()]
    [System.Xml.Serialization.XmlTypeAttribute()]
    public enum LegSwapTypeEnum
    {
        [System.Xml.Serialization.XmlEnumAttribute("1")]
        ParForPar,
        [System.Xml.Serialization.XmlEnumAttribute("2")]
        ModifiedDuration,
        [System.Xml.Serialization.XmlEnumAttribute("4")]
        Risk,
        [System.Xml.Serialization.XmlEnumAttribute("5")]
        Proceeds,
    }
    #endregion LegSwapTypeEnum
    #region MiscFeeBasisEnum
    /* MiscFeeBasis(891) */
    [System.SerializableAttribute()]
    public enum MiscFeeBasisEnum
    {
        [System.Xml.Serialization.XmlEnumAttribute("0")]
        Absolute,
        [System.Xml.Serialization.XmlEnumAttribute("1")]
        PerUnit,
        [System.Xml.Serialization.XmlEnumAttribute("2")]
        Percentage,
    }
    #endregion MiscFeeBasisEnum
    #region MoneyPositionEnum
    public enum MoneyPositionEnum
    {
        Unknown = 0,
        InTheMoney,
        AtTheMoney,
        OutOfTheMoney
    }
    #endregion MoneyPositionEnum
    #region MultiLegReportingTypeEnum
    /* MultiLegReportingType(442) */
    [System.SerializableAttribute()]
    public enum MultiLegReportingTypeEnum
    {
        [System.Xml.Serialization.XmlEnumAttribute("1")]
        Single,
        [System.Xml.Serialization.XmlEnumAttribute("2")]
        IndivLeg,
        [System.Xml.Serialization.XmlEnumAttribute("3")]
        MultiLeg,
    }
    #endregion MultiLegReportingTypeEnum
    #region OrderCapacityEnum
    /* OrderCapacity(528) */
    [System.SerializableAttribute()]
    public enum OrderCapacityEnum
    {
        [System.Xml.Serialization.XmlEnumAttribute("A")]
        Agency,
        [System.Xml.Serialization.XmlEnumAttribute("G")]
        Proprietary,
        [System.Xml.Serialization.XmlEnumAttribute("I")]
        Individual,
        [System.Xml.Serialization.XmlEnumAttribute("P")]
        Principal,
        [System.Xml.Serialization.XmlEnumAttribute("R")]
        RisklessPrincipal,
        [System.Xml.Serialization.XmlEnumAttribute("W")]
        AgentOtherMember,
    }
    #endregion OrderCapacityEnum
    #region OrdStatusEnum
    /* OrdStatus(39) */
    [System.SerializableAttribute()]
    [System.Xml.Serialization.XmlTypeAttribute()]
    public enum OrdStatusEnum
    {
        [System.Xml.Serialization.XmlEnumAttribute("0")]
        New,
        [System.Xml.Serialization.XmlEnumAttribute("1")]
        Partial,
        [System.Xml.Serialization.XmlEnumAttribute("2")]
        Filled,
        [System.Xml.Serialization.XmlEnumAttribute("3")]
        Done,
        [System.Xml.Serialization.XmlEnumAttribute("4")]
        Canceled,
        [System.Xml.Serialization.XmlEnumAttribute("5")]
        Replaced,
        [System.Xml.Serialization.XmlEnumAttribute("6")]
        PendingCR,
        [System.Xml.Serialization.XmlEnumAttribute("7")]
        Stopped,
        [System.Xml.Serialization.XmlEnumAttribute("8")]
        Rejected,
        [System.Xml.Serialization.XmlEnumAttribute("9")]
        Suspended,
        [System.Xml.Serialization.XmlEnumAttribute("A")]
        PendingNew,
        [System.Xml.Serialization.XmlEnumAttribute("B")]
        Calculated,
        [System.Xml.Serialization.XmlEnumAttribute("C")]
        Expired,
        [System.Xml.Serialization.XmlEnumAttribute("D")]
        AcceptBidding,
        [System.Xml.Serialization.XmlEnumAttribute("E")]
        PendingRep,
    }
    #endregion OrdStatusEnum
    #region SettlMethodEnum
    /* SettlMethod(1193) */
    [System.SerializableAttribute()]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fixprotocol.org/FIXML-5-0-SP1")]
    public enum SettlMethodEnum
    {
        [System.Xml.Serialization.XmlEnumAttribute("C")]
        CashSettlement,
        [System.Xml.Serialization.XmlEnumAttribute("P")]
        PhysicalSettlement,
    }
    #endregion SettlMethodEnum
    #region PossDupFlagEnum
    /* PossDupFlag(43) */
    [System.SerializableAttribute()]
    public enum PossDupFlagEnum
    {
        [System.Xml.Serialization.XmlEnumAttribute("N")]
        OriginalTransmission,
        [System.Xml.Serialization.XmlEnumAttribute("Y")]
        PossibleDuplicate,
    }
    #endregion PossDupFlagEnum
    #region PossResendEnum
    /* PossResend(97) */
    [System.SerializableAttribute()]
    public enum PossResendEnum
    {
        [System.Xml.Serialization.XmlEnumAttribute("N")]
        OriginalTransmission,
        [System.Xml.Serialization.XmlEnumAttribute("Y")]
        PossibleResend,
    }
    #endregion PossResendEnum
    #region PreallocMethodEnum
    /* PreallocMethod(591) */
    [System.SerializableAttribute()]
    public enum PreallocMethodEnum
    {
        [System.Xml.Serialization.XmlEnumAttribute("0")]
        ProRata,
        [System.Xml.Serialization.XmlEnumAttribute("1")]
        NotProRata_DiscussFirst,
    }
    #endregion PreallocMethodEnum
    #region ProductEnum
    /* Product(460) */
    [System.SerializableAttribute()]
    public enum ProductEnum
    {
        [System.Xml.Serialization.XmlEnumAttribute("1")]
        AGENCY,
        [System.Xml.Serialization.XmlEnumAttribute("2")]
        COMMODITY,
        [System.Xml.Serialization.XmlEnumAttribute("3")]
        CORPORATE,
        [System.Xml.Serialization.XmlEnumAttribute("4")]
        CURRENCY,
        [System.Xml.Serialization.XmlEnumAttribute("5")]
        EQUITY,
        [System.Xml.Serialization.XmlEnumAttribute("6")]
        GOVERNMENT,
        [System.Xml.Serialization.XmlEnumAttribute("7")]
        INDEX,
        [System.Xml.Serialization.XmlEnumAttribute("8")]
        LOAN,
        [System.Xml.Serialization.XmlEnumAttribute("9")]
        MONEYMARKET,
        [System.Xml.Serialization.XmlEnumAttribute("10")]
        MORTGAGE,
        [System.Xml.Serialization.XmlEnumAttribute("11")]
        MUNICIPAL,
        [System.Xml.Serialization.XmlEnumAttribute("12")]
        OTHER,
        [System.Xml.Serialization.XmlEnumAttribute("13")]
        FINANCING,
    }
    #endregion ProductEnum
    #region SettlCurrFxRateCalcEnum
    /* SettlCurrFxRateCalc(156) */
    [System.SerializableAttribute()]
    public enum SettlCurrFxRateCalcEnum
    {
        [System.Xml.Serialization.XmlEnumAttribute("M")]
        Multiply,
        [System.Xml.Serialization.XmlEnumAttribute("D")]
        Divide,
    }
    #endregion SettlCurrFxRateCalcEnum
    #region SettlSessIDEnum
    /// <summary>
    /// Intraday,EndOfDay, StartOfDay
    /// </summary>
    [System.SerializableAttribute()]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fixprotocol.org/FIXML-5-0-SP1")]
    // EG 20240520 [WI930] Add EndOfDayPlusStartOfDay
    public enum SettlSessIDEnum
    {
        [System.Xml.Serialization.XmlEnumAttribute("NONE")]
        None,
        [System.Xml.Serialization.XmlEnumAttribute("ITD")]
        Intraday,
        [System.Xml.Serialization.XmlEnumAttribute("RTH")]
        RegularTradingHours,
        [System.Xml.Serialization.XmlEnumAttribute("ETH")]
        ElectronicTradingHours,
        [System.Xml.Serialization.XmlEnumAttribute("EOD")]
        EndOfDay,
        [System.Xml.Serialization.XmlEnumAttribute("SOD")]
        StartOfDay,
        [System.Xml.Serialization.XmlEnumAttribute("EODSOD")]
        EndOfDayPlusStartOfDay,
    }
    #endregion SettlSessIDEnum
    #region SideEnum
    /* Side(54) */
    [System.Xml.Serialization.XmlTypeAttribute()]
    public enum SideEnum
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
    #endregion SideEnum
    #region SymbolSfxEnum
    /* SymbolSfx(65) */
    [System.SerializableAttribute()]
    public enum SymbolSfxEnum
    {
        [System.Xml.Serialization.XmlEnumAttribute("CD")]
        EUCPLumpsumInterest,
        [System.Xml.Serialization.XmlEnumAttribute("WI")]
        WhenIssued,
    }
    #endregion SymbolSfxEnum
    #region TerminationTypeEnum
    /* TerminationType(788) */
    [System.SerializableAttribute()]
    public enum TerminationTypeEnum
    {
        [System.Xml.Serialization.XmlEnumAttribute("1")]
        Overnight,
        [System.Xml.Serialization.XmlEnumAttribute("2")]
        Term,
        [System.Xml.Serialization.XmlEnumAttribute("3")]
        Flexible,
        [System.Xml.Serialization.XmlEnumAttribute("4")]
        Open,
    }
    #endregion TerminationTypeEnum
    #region YieldTypeEnum
    /* YieldType(235) */
    [System.SerializableAttribute()]
    [System.Xml.Serialization.XmlTypeAttribute()]
    public enum YieldTypeEnum
    {
        [System.Xml.Serialization.XmlEnumAttribute("AFTERTAX")]
        AfterTaxYield,
        [System.Xml.Serialization.XmlEnumAttribute("ANNUAL")]
        AnnualYield,
        [System.Xml.Serialization.XmlEnumAttribute("ATISSUE")]
        YieldAtIssue,
        [System.Xml.Serialization.XmlEnumAttribute("AVGMATURITY")]
        YieldToAvgMaturity,
        [System.Xml.Serialization.XmlEnumAttribute("BOOK")]
        BookYield,
        [System.Xml.Serialization.XmlEnumAttribute("CALL")]
        YieldToNextCall,
        [System.Xml.Serialization.XmlEnumAttribute("CHANGE")]
        YieldChangeSinceClose,
        [System.Xml.Serialization.XmlEnumAttribute("CLOSE")]
        ClosingYield,
        [System.Xml.Serialization.XmlEnumAttribute("COMPOUND")]
        CompoundYield,
        [System.Xml.Serialization.XmlEnumAttribute("CURRENT")]
        CurrentYield,
        [System.Xml.Serialization.XmlEnumAttribute("GOVTEQUIV")]
        GvntEquivalentYield,
        [System.Xml.Serialization.XmlEnumAttribute("GROSS")]
        TrueGrossYield,
        [System.Xml.Serialization.XmlEnumAttribute("INFLATION")]
        YieldInflationAssumption,
        [System.Xml.Serialization.XmlEnumAttribute("INVERSEFLOATER")]
        InvFloaterBondYield,
        [System.Xml.Serialization.XmlEnumAttribute("LASTCLOSE")]
        MostRecentClosingYield,
        [System.Xml.Serialization.XmlEnumAttribute("LASTMONTH")]
        ClosingYieldMostRecentMonth,
        [System.Xml.Serialization.XmlEnumAttribute("LASTQUARTER")]
        ClosingYieldMostRecentQuarter,
        [System.Xml.Serialization.XmlEnumAttribute("LASTYEAR")]
        ClosingYieldMostRecentYear,
        [System.Xml.Serialization.XmlEnumAttribute("LONGAVGLIFE")]
        YieldToLongestAverageLife,
        [System.Xml.Serialization.XmlEnumAttribute("MARK")]
        MarkToMarketYield,
        [System.Xml.Serialization.XmlEnumAttribute("MATURITY")]
        YieldToMaturity,
        [System.Xml.Serialization.XmlEnumAttribute("NEXTREFUND")]
        YieldToNextRefundSinking,
        [System.Xml.Serialization.XmlEnumAttribute("OPENAVG")]
        OpenAverageYield,
        [System.Xml.Serialization.XmlEnumAttribute("PREVCLOSE")]
        PreviousCloseYield,
        [System.Xml.Serialization.XmlEnumAttribute("PROCEEDS")]
        ProceedsYield,
        [System.Xml.Serialization.XmlEnumAttribute("PUT")]
        YieldToNextPut,
        [System.Xml.Serialization.XmlEnumAttribute("SEMIANNUAL")]
        SemiAnnualYield,
        [System.Xml.Serialization.XmlEnumAttribute("SHORTAVGLIFE")]
        YieldToShortestAverageLife,
        [System.Xml.Serialization.XmlEnumAttribute("SIMPLE")]
        SimpleYield,
        [System.Xml.Serialization.XmlEnumAttribute("TAXEQUIV")]
        TaxEquivalentYield,
        [System.Xml.Serialization.XmlEnumAttribute("TENDER")]
        YieldtoTenderDate,
        [System.Xml.Serialization.XmlEnumAttribute("TRUE")]
        TrueYield,
        [System.Xml.Serialization.XmlEnumAttribute("VALUE1_32")]
        YieldValueOf1_32,
        [System.Xml.Serialization.XmlEnumAttribute("WORST")]
        YieldToWorst,
    }
    #endregion YieldTypeEnum
    #region YesNoEnum
    [System.Xml.Serialization.XmlTypeAttribute()]
    public enum YesNoEnum
    {
        [System.Xml.Serialization.XmlEnumAttribute("Y")]
        Yes,
        [System.Xml.Serialization.XmlEnumAttribute("N")]
        No,
    }
    #endregion YesNoEnum

    #region ExecTypeEnum
    /* ExecType(150) */
    [System.SerializableAttribute()]
    [System.Xml.Serialization.XmlTypeAttribute()]
    public enum ExecTypeEnum
    {
        [System.Xml.Serialization.XmlEnumAttribute("0")]
        New,
        [System.Xml.Serialization.XmlEnumAttribute("3")]
        Done,
        [System.Xml.Serialization.XmlEnumAttribute("4")]
        Canceled,
        [System.Xml.Serialization.XmlEnumAttribute("5")]
        Replaced,
        [System.Xml.Serialization.XmlEnumAttribute("6")]
        PendingCancel,
        [System.Xml.Serialization.XmlEnumAttribute("7")]
        Stopped,
        [System.Xml.Serialization.XmlEnumAttribute("8")]
        Rejected,
        [System.Xml.Serialization.XmlEnumAttribute("9")]
        Suspended,
        [System.Xml.Serialization.XmlEnumAttribute("A")]
        PendingNew,
        [System.Xml.Serialization.XmlEnumAttribute("B")]
        Calculated,
        [System.Xml.Serialization.XmlEnumAttribute("C")]
        Expired,
        [System.Xml.Serialization.XmlEnumAttribute("D")]
        Restated,
        [System.Xml.Serialization.XmlEnumAttribute("E")]
        PendingReplace,
        [System.Xml.Serialization.XmlEnumAttribute("F")]
        Trade,
        [System.Xml.Serialization.XmlEnumAttribute("G")]
        TradeCorrect,
        [System.Xml.Serialization.XmlEnumAttribute("H")]
        TradeCancel,
        [System.Xml.Serialization.XmlEnumAttribute("I")]
        OrderStatus,
        [System.Xml.Serialization.XmlEnumAttribute("J")]
        TradeInAClearingHold,
        [System.Xml.Serialization.XmlEnumAttribute("K")]
        TradeHasBeenReleasedToClearing,
        [System.Xml.Serialization.XmlEnumAttribute("L")]
        TriggeredOrActivatedBySystem,
    }
    #endregion ExecTypeEnum

    #region PutOrCallEnum
    /* PutOrCall(201) */
    [System.SerializableAttribute()]
    [System.Xml.Serialization.XmlTypeAttribute()]
    public enum PutOrCallEnum
    {
        [System.Xml.Serialization.XmlEnumAttribute("0")]
        Put,
        [System.Xml.Serialization.XmlEnumAttribute("1")]
        Call,
    }
    #endregion PutOrCallEnum
    #region TrdSubTypeEnum
    /* TrdSubType(829)) */
    [System.SerializableAttribute()]
    [System.Xml.Serialization.XmlTypeAttribute()]
    public enum TrdSubTypeEnum
    {
        [System.Xml.Serialization.XmlEnumAttribute("0")]
        CMTA,
        [System.Xml.Serialization.XmlEnumAttribute("1")]
        InternalTransferOrAdjustment,
        [System.Xml.Serialization.XmlEnumAttribute("2")]
        ExternalTransferOrTransferOfAccount,
        [System.Xml.Serialization.XmlEnumAttribute("3")]
        RejectForSubmittingSide,
        [System.Xml.Serialization.XmlEnumAttribute("4")]
        AdvisoryForContraSide,
        [System.Xml.Serialization.XmlEnumAttribute("5")]
        Offset,
        [System.Xml.Serialization.XmlEnumAttribute("6")]
        Onset,
        [System.Xml.Serialization.XmlEnumAttribute("7")]
        DifferentialSpread,
        [System.Xml.Serialization.XmlEnumAttribute("8")]
        ImpliedSpreadLegExecutedAgainstAnOutright,
        [System.Xml.Serialization.XmlEnumAttribute("9")]
        TransactionFromExercise,
        [System.Xml.Serialization.XmlEnumAttribute("10")]
        TransactionFromAssignment,
        [System.Xml.Serialization.XmlEnumAttribute("11")]
        ACATS,
        [System.Xml.Serialization.XmlEnumAttribute("14")]
        AI,
        [System.Xml.Serialization.XmlEnumAttribute("15")]
        B,
        [System.Xml.Serialization.XmlEnumAttribute("16")]
        K,
        [System.Xml.Serialization.XmlEnumAttribute("17")]
        LC,
        [System.Xml.Serialization.XmlEnumAttribute("18")]
        M,
        [System.Xml.Serialization.XmlEnumAttribute("19")]
        N,
        [System.Xml.Serialization.XmlEnumAttribute("20")]
        NM,
        [System.Xml.Serialization.XmlEnumAttribute("21")]
        NR,
        [System.Xml.Serialization.XmlEnumAttribute("22")]
        P,
        [System.Xml.Serialization.XmlEnumAttribute("23")]
        PA,
        [System.Xml.Serialization.XmlEnumAttribute("24")]
        PC,
        [System.Xml.Serialization.XmlEnumAttribute("25")]
        PN,
        [System.Xml.Serialization.XmlEnumAttribute("26")]
        R,
        [System.Xml.Serialization.XmlEnumAttribute("27")]
        RO,
        [System.Xml.Serialization.XmlEnumAttribute("28")]
        RT,
        [System.Xml.Serialization.XmlEnumAttribute("29")]
        SW,
        [System.Xml.Serialization.XmlEnumAttribute("30")]
        T,
        [System.Xml.Serialization.XmlEnumAttribute("31")]
        WN,
        [System.Xml.Serialization.XmlEnumAttribute("32")]
        WT,
        [System.Xml.Serialization.XmlEnumAttribute("33")]
        OffHoursTrade,
        [System.Xml.Serialization.XmlEnumAttribute("34")]
        OnHoursTrade,
        [System.Xml.Serialization.XmlEnumAttribute("35")]
        OTCQuote,
        [System.Xml.Serialization.XmlEnumAttribute("36")]
        ConvertedSWAP,
        [System.Xml.Serialization.XmlEnumAttribute("37")]
        CrossedTrade,
        [System.Xml.Serialization.XmlEnumAttribute("38")]
        InterimProtectedTrade,
        [System.Xml.Serialization.XmlEnumAttribute("39")]
        LargeInScale
    }
    #endregion TrdSubTypeEnum
    #region TrdTypeEnum
    /// <summary>
    /// TrdType(828)
    /// </summary>
    /// FI 20140505 [19851] add volaTrade, EFPFinTrade, EFPIndexFuturesTrade
    // EG 20190613 [24683] Add Technical trade
    // PM 20231113 [26490][WI740] Add Eurex C7 10.0 Values: VBAPOnExchBuysideNonDisclosed, VBAPTES1BuysideNonDisclosed, VBAPTES2BuysideNonDisclosed, VBAPOnExchBuysideDisclosed, VBAPTES1BuysideDisclosed, VBAPTES2BuysideDisclosed, ProductDeListing
    // PM 20231207 [26610][WI771] Add Eurex C7 10.1 Value: FlexibleEFPIndexFuturesTrade
    [System.SerializableAttribute()]
    [System.Xml.Serialization.XmlTypeAttribute()]
    public enum TrdTypeEnum
    {
        [System.Xml.Serialization.XmlEnumAttribute("0")]
        RegularTrade,
        [System.Xml.Serialization.XmlEnumAttribute("1")]
        BlockTrade,
        // 20120531 MF Ticket 17769
        /// <summary>
        /// Exchange for Physical, available on the Spheres ENUMs 'SecondaryTrdTypeEnum', 'TrdTypeEnum'
        /// </summary>
        [System.Xml.Serialization.XmlEnumAttribute("2")]
        EFP,
        [System.Xml.Serialization.XmlEnumAttribute("3")]
        Transfer,
        [System.Xml.Serialization.XmlEnumAttribute("4")]
        LateTrade,
        [System.Xml.Serialization.XmlEnumAttribute("5")]
        T_Trade,
        [System.Xml.Serialization.XmlEnumAttribute("6")]
        WeightedAveragePriceTrade,
        [System.Xml.Serialization.XmlEnumAttribute("7")]
        BunchedTrade,
        [System.Xml.Serialization.XmlEnumAttribute("8")]
        LateBunchedTrade,
        [System.Xml.Serialization.XmlEnumAttribute("9")]
        PriorReferencePriceTrade,
        [System.Xml.Serialization.XmlEnumAttribute("10")]
        AfterHoursTrade,
        [System.Xml.Serialization.XmlEnumAttribute("11")]
        ExchangeForRisk,
        /// <summary>
        /// Exchange for Swap, available on the Spheres ENUMs 'SecondaryTrdTypeEnum', 'TrdTypeEnum'
        /// </summary>
        [System.Xml.Serialization.XmlEnumAttribute("12")]
        ExchangeForSwap,
        [System.Xml.Serialization.XmlEnumAttribute("13")]
        ExchangeOfFuturesForFuturesInMarket,
        [System.Xml.Serialization.XmlEnumAttribute("14")]
        ExchangeOfOptionsForOptions,
        [System.Xml.Serialization.XmlEnumAttribute("15")]
        TradingAtSettlement,
        [System.Xml.Serialization.XmlEnumAttribute("16")]
        AllOrNone,
        [System.Xml.Serialization.XmlEnumAttribute("17")]
        FuturesLargeOrderExecution,
        [System.Xml.Serialization.XmlEnumAttribute("18")]
        ExchangeOfFuturesForFuturesExternalMarket,
        [System.Xml.Serialization.XmlEnumAttribute("19")]
        OptionInterimTrade,
        [System.Xml.Serialization.XmlEnumAttribute("20")]
        OptionCabinetTrade,
        [System.Xml.Serialization.XmlEnumAttribute("22")]
        PrivatelyNegotiatedTrade,
        [System.Xml.Serialization.XmlEnumAttribute("23")]
        SubstitutionOfFuturesForForwards,
        [System.Xml.Serialization.XmlEnumAttribute("24")]
        ErrorTrade,
        [System.Xml.Serialization.XmlEnumAttribute("25")]
        SpecialCumDividend,
        [System.Xml.Serialization.XmlEnumAttribute("26")]
        SpecialExdividend,
        [System.Xml.Serialization.XmlEnumAttribute("27")]
        SpecialCumCoupon,
        [System.Xml.Serialization.XmlEnumAttribute("28")]
        SpecialExCoupon,
        [System.Xml.Serialization.XmlEnumAttribute("29")]
        CashSettlement,
        [System.Xml.Serialization.XmlEnumAttribute("30")]
        SpecialPrice,
        [System.Xml.Serialization.XmlEnumAttribute("31")]
        GuaranteedDelivery,
        [System.Xml.Serialization.XmlEnumAttribute("32")]
        SpecialCumRights,
        [System.Xml.Serialization.XmlEnumAttribute("33")]
        SpecialExRights,
        [System.Xml.Serialization.XmlEnumAttribute("34")]
        SpecialCumCapitalRepayments,
        [System.Xml.Serialization.XmlEnumAttribute("35")]
        SpecialExCapitalRepayments,
        [System.Xml.Serialization.XmlEnumAttribute("36")]
        SpecialCumBonus,
        [System.Xml.Serialization.XmlEnumAttribute("37")]
        SpecialExBonus,
        [System.Xml.Serialization.XmlEnumAttribute("38")]
        LargeTrade,
        [System.Xml.Serialization.XmlEnumAttribute("39")]
        WorkedPrincipalTrade,
        [System.Xml.Serialization.XmlEnumAttribute("40")]
        BlockTradeAfterMarket,
        [System.Xml.Serialization.XmlEnumAttribute("41")]
        NameChange,
        [System.Xml.Serialization.XmlEnumAttribute("42")]
        PortfolioTransfer,
        [System.Xml.Serialization.XmlEnumAttribute("43")]
        ProrogationBuy,
        [System.Xml.Serialization.XmlEnumAttribute("44")]
        ProrogationSell,
        [System.Xml.Serialization.XmlEnumAttribute("45")]
        OptionExercise,
        [System.Xml.Serialization.XmlEnumAttribute("46")]
        DeltaNeutralTransaction,
        [System.Xml.Serialization.XmlEnumAttribute("47")]
        FinancingTransaction,
        [System.Xml.Serialization.XmlEnumAttribute("48")]
        NonStandardSettlement,
        [System.Xml.Serialization.XmlEnumAttribute("49")]
        DerivativeRelatedTransaction,
        [System.Xml.Serialization.XmlEnumAttribute("50")]
        PortfolioTrade,
        [System.Xml.Serialization.XmlEnumAttribute("51")]
        VolumeWeightedAverageTrade,
        [System.Xml.Serialization.XmlEnumAttribute("52")]
        ExchangeGrantedTrade,
        [System.Xml.Serialization.XmlEnumAttribute("53")]
        RepurchaseAgreement,
        [System.Xml.Serialization.XmlEnumAttribute("54")]
        OTC,
        [System.Xml.Serialization.XmlEnumAttribute("55")]
        ExchangeBasisFacility,
        [System.Xml.Serialization.XmlEnumAttribute("63")]
        TechnicalTrade,
        //
        [System.Xml.Serialization.XmlEnumAttribute("1000")]
        PositionOpening,
        /// <summary>
        /// Position comming from a cascading of position
        /// </summary>
        //  PM 20130216 [18414]
        [System.Xml.Serialization.XmlEnumAttribute("1001")]
        Cascading,
        /// <summary>
        /// Position comming from a shifting of position
        /// </summary>
        //  PM 20130216 [18414]
        [System.Xml.Serialization.XmlEnumAttribute("1002")]
        Shifting,
        // EG 20130607
        [System.Xml.Serialization.XmlEnumAttribute("1003")]
        CorporateAction,
        [System.Xml.Serialization.XmlEnumAttribute("1004")]
        MergedTrade,
        [System.Xml.Serialization.XmlEnumAttribute("1005")]
        SplitTrade,
        /// <summary>
        /// Vola Trade
        /// <para>Eurex Trade Type</para>
        /// </summary>
        /// FI 20140505 [19851]
        [System.Xml.Serialization.XmlEnumAttribute("1100")]
        VolaTrade,
        /// <summary>
        /// Exchange for physical
        /// <para>Eurex Trade Type</para>
        /// </summary>
        /// FI 20140505 [19851]
        [System.Xml.Serialization.XmlEnumAttribute("1101")]
        EFPFinTrade,
        /// <summary>
        /// Exchange for physical
        /// <para>Eurex Trade Type</para>
        /// </summary>
        /// FI 20140505 [19851]
        [System.Xml.Serialization.XmlEnumAttribute("1102")]
        EFPIndexFuturesTrade,
        /// <summary>
        /// Transaction based Settlement
        /// <para>Eurex Trade Type</para>
        /// </summary>
        // PM 20231113 [26490][WI740]
        [System.Xml.Serialization.XmlEnumAttribute("1104")]
        TransactionBasedSettlement,
        /// <summary>
        /// Enlight Triggered Trade
        /// <para>Eurex Trade Type</para>
        /// </summary>
        /// RD 20201221 [25603]
        [System.Xml.Serialization.XmlEnumAttribute("1106")]
        EnlightTriggeredTrade,
        /// <summary>
        /// Block QTPIP Trade
        /// <para>Eurex Trade Type</para>
        /// </summary>
        // PM 20231113 [26490][WI740]
        [System.Xml.Serialization.XmlEnumAttribute("1107")]
        BlockQTPIPTrade,
        /// <summary>
        /// Compression Trade
        /// <para>Eurex Trade Type</para>
        /// </summary>
        // PM 20231113 [26490][WI740]
        // PM 20231207 [26610][WI771] Valeur 1108 Supprimée à partir de C7 10.1
        [Obsolete("Deleted on C7 10.1", false)]
        [System.Xml.Serialization.XmlEnumAttribute("1108")]
        CompressionTrade,
        /// <summary>
        /// Flexible EFP-Index Futures Trade
        /// </summary>
        // PM 20231207 [26610][WI771]
        [System.Xml.Serialization.XmlEnumAttribute("1116")]
        FlexibleEFPIndexFuturesTrade,
        /// <summary>
        /// VBAP On-Exch Buyside non-disclosed
        /// <para>Eurex Trade Type</para>
        /// </summary>
        // PM 20231113 [26490][WI740]
        [System.Xml.Serialization.XmlEnumAttribute("1150")]
        VBAPOnExchBuysideNonDisclosed,
        /// <summary>
        /// VBAP TES1 Buyside non-disclosed
        /// <para>Eurex Trade Type</para>
        /// </summary>
        // PM 20231113 [26490][WI740]
        [System.Xml.Serialization.XmlEnumAttribute("1151")]
        VBAPTES1BuysideNonDisclosed,
        /// <summary>
        /// VBAP TES2 Buyside non-disclosed
        /// <para>Eurex Trade Type</para>
        /// </summary>
        // PM 20231113 [26490][WI740]
        [System.Xml.Serialization.XmlEnumAttribute("1152")]
        VBAPTES2BuysideNonDisclosed,
        /// <summary>
        /// VBAP On-Exch Buyside disclosed
        /// <para>Eurex Trade Type</para>
        /// </summary>
        // PM 20231113 [26490][WI740]
        [System.Xml.Serialization.XmlEnumAttribute("1153")]
        VBAPOnExchBuysideDisclosed,
        /// <summary>
        /// VBAP TES1 Buyside disclosed
        /// <para>Eurex Trade Type</para>
        /// </summary>
        // PM 20231113 [26490][WI740]
        [System.Xml.Serialization.XmlEnumAttribute("1154")]
        VBAPTES1BuysideDisclosed,
        /// <summary>
        /// VBAP TES2 Buyside disclosed
        /// <para>Eurex Trade Type</para>
        /// </summary>
        // PM 20231113 [26490][WI740]
        [System.Xml.Serialization.XmlEnumAttribute("1155")]
        VBAPTES2BuysideDisclosed,
        /// <summary>
        /// Product De-listing
        /// <para>Eurex Trade Type</para>
        /// </summary>
        // PM 20231113 [26490][WI740]
        [System.Xml.Serialization.XmlEnumAttribute("1160")]
        ProductDeListing,
    }
    #endregion TrdTypeEnum

    #region PosType
    /// <summary>
    /// Used to identify the type of quantity that is being returned
    /// </summary>
    /// <seealso cref="Spheres\EfsML\Schemas\FIXML44\fixml-fields-base-4-4.xsd"/>
    /// <remarks>http://www.fixprotocol.org/FIXimate3.0/en/FIX.5.0/tag703.html</remarks>
    public enum PosType
    {
        /// <summary>
        /// Allocation Trade 
        /// </summary>
        [XmlEnum("ALC")]
        ALC = 0,
        /// <summary>
        /// Option Assignment
        /// </summary>
        [XmlEnum("AS")]
        AS = 1,
        /// <summary>
        /// Option Exercise
        /// </summary>
        [XmlEnum("EX")]
        EX = 5,
        /// <summary>
        /// Intra-spread Qty
        /// </summary>
        [XmlEnum("IAS")]
        IAS = 7,
        /// <summary>
        /// Inter-spread Qty
        /// </summary>
        [XmlEnum("IAS")]
        IES = 8,
        /// <summary>
        /// Adjustement (used for stock coverage)
        /// </summary>
        [XmlEnum("PA")]
        PA = 9,
        /// <summary>
        /// Cross Margin Qty
        /// </summary>
        [XmlEnum("XM")]
        XM = 18,
         /// <summary>
        /// Delivery Notice
        /// </summary>
        [XmlEnum("DN")]
        DN = 21,
    }

    #endregion PosType


    #endregion Common Enums 4.4 & 5.0 SP1
}

namespace FixML.Enum
{
    #region Eurex FI 20220209 [25699] Add
    #region ContractFrequencyEnum
    /* ContractFrequencyEnum(30867) */
    [System.SerializableAttribute()]
    [System.Xml.Serialization.XmlTypeAttribute()]
    public enum ContractFrequencyEnum
    {
        [System.Xml.Serialization.XmlEnumAttribute("D")]
        Day,
        [System.Xml.Serialization.XmlEnumAttribute("Wk")]
        Week,
        [System.Xml.Serialization.XmlEnumAttribute("Mo")]
        Month,
        [System.Xml.Serialization.XmlEnumAttribute("Flex")]
        Flexible,
        [System.Xml.Serialization.XmlEnumAttribute("EOM")]
        EndOfMonth,
    }
    #endregion ContractFrequencyEnum
    #endregion
}

namespace FixML.v44.Enum
{
    #region Enums Version 4.4
    #region BookingTypeEnum
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fixprotocol.org/FIXML-4-4")]
    public enum BookingTypeEnum
    {
        [System.Xml.Serialization.XmlEnumAttribute("0")]
        RegularBooking,
        [System.Xml.Serialization.XmlEnumAttribute("1")]
        CFDContractForDifference,
        [System.Xml.Serialization.XmlEnumAttribute("2")]
        TotalReturnSwap,
    }
    #endregion BookingTypeEnum
    #region BookingUnitEnum
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fixprotocol.org/FIXML-4-4")]
    public enum BookingUnitEnum
    {
        [System.Xml.Serialization.XmlEnumAttribute("0")]
        EachPartialExecution,
        [System.Xml.Serialization.XmlEnumAttribute("1")]
        AggregatePartialExecutions,
        [System.Xml.Serialization.XmlEnumAttribute("2")]
        AggregateExecutions_Side_SettlementDate,
    }
    #endregion BookingUnitEnum
    #region CancellationRightsEnum
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fixprotocol.org/FIXML-4-4")]
    public enum CancellationRightsEnum
    {
        [System.Xml.Serialization.XmlEnumAttribute("Y")]
        Yes,
        [System.Xml.Serialization.XmlEnumAttribute("N")]
        NoExecO0nly,
        [System.Xml.Serialization.XmlEnumAttribute("M")]
        NoWaiver,
        [System.Xml.Serialization.XmlEnumAttribute("O")]
        NoInstit,
    }
    #endregion CancellationRightsEnum
    #region CashMarginEnum
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fixprotocol.org/FIXML-4-4")]
    public enum CashMarginEnum
    {
        [System.Xml.Serialization.XmlEnumAttribute("1")]
        Cash,
        [System.Xml.Serialization.XmlEnumAttribute("2")]
        MarginOpen,
        [System.Xml.Serialization.XmlEnumAttribute("3")]
        MarginClose,
    }
    #endregion CashMarginEnum
    #region CrossTypeEnum
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fixprotocol.org/FIXML-4-4")]
    public enum CrossTypeEnum
    {
        [System.Xml.Serialization.XmlEnumAttribute("1")]
        CrossAON,
        [System.Xml.Serialization.XmlEnumAttribute("2")]
        CrossIOC,
        [System.Xml.Serialization.XmlEnumAttribute("3")]
        CrossOneSide,
        [System.Xml.Serialization.XmlEnumAttribute("4")]
        CrossSamePrice,
    }
    #endregion CrossTypeEnum
    #region CurveNameEnum
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fixprotocol.org/FIXML-4-4")]
    public enum CurveNameEnum
    {
        [System.Xml.Serialization.XmlEnumAttribute("MuniAAA")]
        MuniAAA,
        [System.Xml.Serialization.XmlEnumAttribute("FutureSWAP")]
        FutureSWAP,
        [System.Xml.Serialization.XmlEnumAttribute("LIBID")]
        LIBID,
        [System.Xml.Serialization.XmlEnumAttribute("LIBOR")]
        LIBOR,
        [System.Xml.Serialization.XmlEnumAttribute("OTHER")]
        OTHER,
        [System.Xml.Serialization.XmlEnumAttribute("SWAP")]
        SWAP,
        [System.Xml.Serialization.XmlEnumAttribute("Treasury")]
        Treasury,
        [System.Xml.Serialization.XmlEnumAttribute("Euribor")]
        Euribor,
        [System.Xml.Serialization.XmlEnumAttribute("Pfandbriefe")]
        Pfandbriefe,
        [System.Xml.Serialization.XmlEnumAttribute("EONIA")]
        EONIA,
        [System.Xml.Serialization.XmlEnumAttribute("SONIA")]
        SONIA,
        [System.Xml.Serialization.XmlEnumAttribute("EUREPO")]
        EUREPO,
    }
    #endregion CurveNameEnum
    #region DayBookingInstEnum
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fixprotocol.org/FIXML-4-4")]
    public enum DayBookingInstEnum
    {
        [System.Xml.Serialization.XmlEnumAttribute("0")]
        Auto,
        [System.Xml.Serialization.XmlEnumAttribute("1")]
        SpeakFirst,
        [System.Xml.Serialization.XmlEnumAttribute("2")]
        Accumulate,
    }
    #endregion DayBookingInstEnum
    #region DiscretionInstEnum
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fixprotocol.org/FIXML-4-4")]
    public enum DiscretionInstEnum
    {
        [System.Xml.Serialization.XmlEnumAttribute("0")]
        DispPrice,
        [System.Xml.Serialization.XmlEnumAttribute("1")]
        MarketPrice,
        [System.Xml.Serialization.XmlEnumAttribute("2")]
        PrimaryPrice,
        [System.Xml.Serialization.XmlEnumAttribute("3")]
        LocalPrimaryPrice,
        [System.Xml.Serialization.XmlEnumAttribute("4")]
        MidpointPrice,
        [System.Xml.Serialization.XmlEnumAttribute("5")]
        LastTradePrice,
        [System.Xml.Serialization.XmlEnumAttribute("6")]
        VWAP,
    }
    #endregion DiscretionInstEnum
    #region EventTypeEnum
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fixprotocol.org/FIXML-4-4")]
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
        [System.Xml.Serialization.XmlEnumAttribute("99")]
        Other,
    }
    #endregion EventTypeEnum
    #region ExecInstEnum
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fixprotocol.org/FIXML-4-4")]
    public enum ExecInstEnum
    {
        [System.Xml.Serialization.XmlEnumAttribute("1")]
        NotHeld,
        [System.Xml.Serialization.XmlEnumAttribute("2")]
        Work,
        [System.Xml.Serialization.XmlEnumAttribute("3")]
        GoAlong,
        [System.Xml.Serialization.XmlEnumAttribute("4")]
        OverDay,
        [System.Xml.Serialization.XmlEnumAttribute("5")]
        Held,
        [System.Xml.Serialization.XmlEnumAttribute("6")]
        PartNotInit,
        [System.Xml.Serialization.XmlEnumAttribute("7")]
        StrictScale,
        [System.Xml.Serialization.XmlEnumAttribute("8")]
        TryToScale,
        [System.Xml.Serialization.XmlEnumAttribute("9")]
        StayBid,
        [System.Xml.Serialization.XmlEnumAttribute("0")]
        StayOffer,
        [System.Xml.Serialization.XmlEnumAttribute("A")]
        NoCross,
        [System.Xml.Serialization.XmlEnumAttribute("B")]
        OkCross,
        [System.Xml.Serialization.XmlEnumAttribute("C")]
        CallFirst,
        [System.Xml.Serialization.XmlEnumAttribute("D")]
        PercVol,
        [System.Xml.Serialization.XmlEnumAttribute("E")]
        DNI,
        [System.Xml.Serialization.XmlEnumAttribute("F")]
        DNR,
        [System.Xml.Serialization.XmlEnumAttribute("G")]
        AllOrNone,
        [System.Xml.Serialization.XmlEnumAttribute("H")]
        RestateOnSysFail,
        [System.Xml.Serialization.XmlEnumAttribute("I")]
        InstitOnly,
        [System.Xml.Serialization.XmlEnumAttribute("J")]
        RestateOnTradingHalt,
        [System.Xml.Serialization.XmlEnumAttribute("K")]
        CancelOnTradingHalt,
        [System.Xml.Serialization.XmlEnumAttribute("L")]
        LastPeg,
        [System.Xml.Serialization.XmlEnumAttribute("M")]
        MidPrcPeg,
        [System.Xml.Serialization.XmlEnumAttribute("N")]
        NonNego,
        [System.Xml.Serialization.XmlEnumAttribute("O")]
        OpenPeg,
        [System.Xml.Serialization.XmlEnumAttribute("P")]
        MarkPeg,
        [System.Xml.Serialization.XmlEnumAttribute("Q")]
        CancelOnSysFail,
        [System.Xml.Serialization.XmlEnumAttribute("R")]
        PrimPeg,
        [System.Xml.Serialization.XmlEnumAttribute("S")]
        Suspend,
        [System.Xml.Serialization.XmlEnumAttribute("T")]
        FixedPeg,
        [System.Xml.Serialization.XmlEnumAttribute("U")]
        CustDispInst,
        [System.Xml.Serialization.XmlEnumAttribute("V")]
        Netting,
        [System.Xml.Serialization.XmlEnumAttribute("W")]
        PegVWAP,
        [System.Xml.Serialization.XmlEnumAttribute("X")]
        TradeAlong,
        [System.Xml.Serialization.XmlEnumAttribute("Y")]
        TryToStop,
        [System.Xml.Serialization.XmlEnumAttribute("Z")]
        CxlifNotBest,
        [System.Xml.Serialization.XmlEnumAttribute("a")]
        TrailStopPeg,
        [System.Xml.Serialization.XmlEnumAttribute("b")]
        StrictLimit,
        [System.Xml.Serialization.XmlEnumAttribute("c")]
        IgnorePriceChk,
        [System.Xml.Serialization.XmlEnumAttribute("d")]
        PegToLimit,
        [System.Xml.Serialization.XmlEnumAttribute("e")]
        WorkToStrategy,
    }
    #endregion ExecInstEnum
    #region ExecPriceTypeEnum
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fixprotocol.org/FIXML-4-4")]
    public enum ExecPriceTypeEnum
    {
        [System.Xml.Serialization.XmlEnumAttribute("B")]
        BidPrice,
        [System.Xml.Serialization.XmlEnumAttribute("C")]
        CreationPrice,
        [System.Xml.Serialization.XmlEnumAttribute("D")]
        CreationPriceAdjPct,
        [System.Xml.Serialization.XmlEnumAttribute("E")]
        CreationPriceAdjAmt,
        [System.Xml.Serialization.XmlEnumAttribute("O")]
        OfferPrice,
        [System.Xml.Serialization.XmlEnumAttribute("P")]
        OfferPriceMinusAdjPct,
        [System.Xml.Serialization.XmlEnumAttribute("Q")]
        OfferPriceMinusAdjAmt,
        [System.Xml.Serialization.XmlEnumAttribute("S")]
        SinglePrice,
    }
    #endregion ExecPriceTypeEnum
    #region ExecRestatementReasonEnum
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fixprotocol.org/FIXML-4-4")]
    public enum ExecRestatementReasonEnum
    {
        [System.Xml.Serialization.XmlEnumAttribute("0")]
        GTCorpAct,
        [System.Xml.Serialization.XmlEnumAttribute("1")]
        GTRenew,
        [System.Xml.Serialization.XmlEnumAttribute("2")]
        Verbal,
        [System.Xml.Serialization.XmlEnumAttribute("3")]
        RePx,
        [System.Xml.Serialization.XmlEnumAttribute("4")]
        BrkrOpt,
        [System.Xml.Serialization.XmlEnumAttribute("5")]
        PartDec,
        [System.Xml.Serialization.XmlEnumAttribute("6")]
        CxlTradingHalt,
        [System.Xml.Serialization.XmlEnumAttribute("7")]
        CxlSystemFailure,
        [System.Xml.Serialization.XmlEnumAttribute("8")]
        MrktOption,
        [System.Xml.Serialization.XmlEnumAttribute("9")]
        CanceledNotBest,
        [System.Xml.Serialization.XmlEnumAttribute("10")]
        WarehouseRecap,
        [System.Xml.Serialization.XmlEnumAttribute("99")]
        Other,
    }
    #endregion ExecRestatementReasonEnum
    #region GTBookingInstEnum
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fixprotocol.org/FIXML-4-4")]
    public enum GTBookingInstEnum
    {
        [System.Xml.Serialization.XmlEnumAttribute("0")]
        BookAll,
        [System.Xml.Serialization.XmlEnumAttribute("1")]
        AccumUntilFill,
        [System.Xml.Serialization.XmlEnumAttribute("2")]
        AccumUntilNotify,
    }
    #endregion GTBookingInstEnum
    #region HandlInstEnum
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fixprotocol.org/FIXML-4-4")]
    public enum HandlInstEnum
    {
        [System.Xml.Serialization.XmlEnumAttribute("1")]
        AutoExecPriv,
        [System.Xml.Serialization.XmlEnumAttribute("2")]
        AutoExecPub,
        [System.Xml.Serialization.XmlEnumAttribute("3")]
        Manual,
    }
    #endregion HandlInstEnum
    #region LastCapacityEnum
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fixprotocol.org/FIXML-4-4")]
    public enum LastCapacityEnum
    {
        [System.Xml.Serialization.XmlEnumAttribute("1")]
        Agent,
        [System.Xml.Serialization.XmlEnumAttribute("2")]
        CrossAsAgent,
        [System.Xml.Serialization.XmlEnumAttribute("3")]
        CrossAsPrincipal,
        [System.Xml.Serialization.XmlEnumAttribute("4")]
        Principal,
    }
    #endregion LastCapacityEnum
    #region LastLiquidityIndEnum
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fixprotocol.org/FIXML-4-4")]
    public enum LastLiquidityIndEnum
    {
        [System.Xml.Serialization.XmlEnumAttribute("1")]
        AddedLiquidity,
        [System.Xml.Serialization.XmlEnumAttribute("2")]
        RemovedLiquidity,
        [System.Xml.Serialization.XmlEnumAttribute("3")]
        LiquidityRoutedOut,
    }
    #endregion LastLiquidityIndEnum
    #region LimitTypeEnum
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fixprotocol.org/FIXML-4-4")]
    public enum LimitTypeEnum
    {
        [System.Xml.Serialization.XmlEnumAttribute("0")]
        OrBetter,
        [System.Xml.Serialization.XmlEnumAttribute("1")]
        Strict,
        [System.Xml.Serialization.XmlEnumAttribute("2")]
        OrWorse,
    }
    #endregion LimitTypeEnum
    #region MiscFeeTypeEnum
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fixprotocol.org/FIXML-4-4")]
    public enum MiscFeeTypeEnum
    {
        [System.Xml.Serialization.XmlEnumAttribute("1")]
        Reg,
        [System.Xml.Serialization.XmlEnumAttribute("2")]
        Tax,
        [System.Xml.Serialization.XmlEnumAttribute("3")]
        LocalComm,
        [System.Xml.Serialization.XmlEnumAttribute("4")]
        ExchFee,
        [System.Xml.Serialization.XmlEnumAttribute("5")]
        Stamp,
        [System.Xml.Serialization.XmlEnumAttribute("6")]
        Levy,
        [System.Xml.Serialization.XmlEnumAttribute("7")]
        Other,
        [System.Xml.Serialization.XmlEnumAttribute("8")]
        Markup,
        [System.Xml.Serialization.XmlEnumAttribute("9")]
        Consumption,
        [System.Xml.Serialization.XmlEnumAttribute("10")]
        Transaction,
        [System.Xml.Serialization.XmlEnumAttribute("11")]
        Conversion,
        [System.Xml.Serialization.XmlEnumAttribute("12")]
        Agent,
        [System.Xml.Serialization.XmlEnumAttribute("13")]
        SecLending,
    }
    #endregion MiscFeeTypeEnum
    #region MoneyLaunderingStatusEnum
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fixprotocol.org/FIXML-4-4")]
    public enum MoneyLaunderingStatusEnum
    {
        [System.Xml.Serialization.XmlEnumAttribute("Y")]
        Passed,
        [System.Xml.Serialization.XmlEnumAttribute("N")]
        NotChecked,
        [System.Xml.Serialization.XmlEnumAttribute("1")]
        ExBelowLim,
        [System.Xml.Serialization.XmlEnumAttribute("2")]
        ExClientMoneyType,
        [System.Xml.Serialization.XmlEnumAttribute("3")]
        ExAuthCredit,
    }
    #endregion MoneyLaunderingStatusEnum
    #region OffsetTypeEnum
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fixprotocol.org/FIXML-4-4")]
    public enum OffsetTypeEnum
    {
        [System.Xml.Serialization.XmlEnumAttribute("0")]
        Price,
        [System.Xml.Serialization.XmlEnumAttribute("1")]
        BasisPoints,
        [System.Xml.Serialization.XmlEnumAttribute("2")]
        Ticks,
        [System.Xml.Serialization.XmlEnumAttribute("3")]
        PriceTierLevel,
    }
    #endregion OffsetTypeEnum
    #region OrderRestrictionsEnum
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fixprotocol.org/FIXML-4-4")]
    public enum OrderRestrictionsEnum
    {
        [System.Xml.Serialization.XmlEnumAttribute("1")]
        ProgramTrade,
        [System.Xml.Serialization.XmlEnumAttribute("2")]
        IndexArbitrage,
        [System.Xml.Serialization.XmlEnumAttribute("3")]
        NonIndexArbitrage,
        [System.Xml.Serialization.XmlEnumAttribute("4")]
        CompetingMarketMaker,
        [System.Xml.Serialization.XmlEnumAttribute("5")]
        ActMM,
        [System.Xml.Serialization.XmlEnumAttribute("6")]
        ActMMDeriv,
        [System.Xml.Serialization.XmlEnumAttribute("7")]
        ForeignEntity,
        [System.Xml.Serialization.XmlEnumAttribute("8")]
        ExMrktPart,
        [System.Xml.Serialization.XmlEnumAttribute("9")]
        ExIntMrktLink,
        [System.Xml.Serialization.XmlEnumAttribute("A")]
        RiskArb,
    }
    #endregion OrderRestrictionsEnum
    #region OrdRejReasonEnum
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fixprotocol.org/FIXML-4-4")]
    public enum OrdRejReasonEnum
    {
        [System.Xml.Serialization.XmlEnumAttribute("0")]
        BrokerOpt,
        [System.Xml.Serialization.XmlEnumAttribute("1")]
        UnknownSym,
        [System.Xml.Serialization.XmlEnumAttribute("2")]
        ExchClosed,
        [System.Xml.Serialization.XmlEnumAttribute("3")]
        ExceedsLim,
        [System.Xml.Serialization.XmlEnumAttribute("4")]
        TooLate,
        [System.Xml.Serialization.XmlEnumAttribute("5")]
        Unknown,
        [System.Xml.Serialization.XmlEnumAttribute("6")]
        Duplicate,
        [System.Xml.Serialization.XmlEnumAttribute("7")]
        DuplicateVerbal,
        [System.Xml.Serialization.XmlEnumAttribute("8")]
        Stale,
        [System.Xml.Serialization.XmlEnumAttribute("9")]
        TradeAlongReq,
        [System.Xml.Serialization.XmlEnumAttribute("10")]
        InvInvID,
        [System.Xml.Serialization.XmlEnumAttribute("11")]
        UnsuppOrderChar,
        [System.Xml.Serialization.XmlEnumAttribute("12")]
        Surveillence,
        [System.Xml.Serialization.XmlEnumAttribute("13")]
        IncorrectQuantity,
        [System.Xml.Serialization.XmlEnumAttribute("14")]
        IncorrectAllocatedQuantity,
        [System.Xml.Serialization.XmlEnumAttribute("15")]
        UnknownAccounts,
        [System.Xml.Serialization.XmlEnumAttribute("99")]
        Other,
    }
    #endregion OrdRejReasonEnum
    #region OrdTypeEnum
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fixprotocol.org/FIXML-4-4")]
    public enum OrdTypeEnum
    {
        [System.Xml.Serialization.XmlEnumAttribute("1")]
        Market,
        [System.Xml.Serialization.XmlEnumAttribute("2")]
        Limit,
        [System.Xml.Serialization.XmlEnumAttribute("3")]
        Stop,
        [System.Xml.Serialization.XmlEnumAttribute("4")]
        StopLimit,
        [System.Xml.Serialization.XmlEnumAttribute("5")]
        MarketOnClose,
        [System.Xml.Serialization.XmlEnumAttribute("6")]
        WithOrWithout,
        [System.Xml.Serialization.XmlEnumAttribute("7")]
        LimitOrBetter,
        [System.Xml.Serialization.XmlEnumAttribute("8")]
        LimitWithOrWithout,
        [System.Xml.Serialization.XmlEnumAttribute("9")]
        OnBasis,
        [System.Xml.Serialization.XmlEnumAttribute("A")]
        OnClose,
        [System.Xml.Serialization.XmlEnumAttribute("B")]
        LimitOnClose,
        [System.Xml.Serialization.XmlEnumAttribute("C")]
        ForexMarket,
        [System.Xml.Serialization.XmlEnumAttribute("D")]
        PreviouslyQuoted,
        [System.Xml.Serialization.XmlEnumAttribute("E")]
        PreviouslyIndicated,
        [System.Xml.Serialization.XmlEnumAttribute("F")]
        ForexLimit,
        [System.Xml.Serialization.XmlEnumAttribute("G")]
        ForexSwap,
        [System.Xml.Serialization.XmlEnumAttribute("H")]
        ForexPreviouslyQuoted,
        [System.Xml.Serialization.XmlEnumAttribute("I")]
        Funari,
        [System.Xml.Serialization.XmlEnumAttribute("J")]
        MarketIfTouched,
        [System.Xml.Serialization.XmlEnumAttribute("K")]
        MarketWithLeftOverLimit,
        [System.Xml.Serialization.XmlEnumAttribute("L")]
        PreviousFundValuationPoint,
        [System.Xml.Serialization.XmlEnumAttribute("M")]
        NextFundValuationPoint,
        [System.Xml.Serialization.XmlEnumAttribute("P")]
        Pegged,
    }
    #endregion OrdTypeEnum
    #region PartyRoleEnum
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fixprotocol.org/FIXML-4-4")]
    public enum PartyRoleEnum
    {
        [System.Xml.Serialization.XmlEnumAttribute("1")]
        ExecutingFirm,
        [System.Xml.Serialization.XmlEnumAttribute("2")]
        BrokerofCredit,
        [System.Xml.Serialization.XmlEnumAttribute("3")]
        ClientID,
        [System.Xml.Serialization.XmlEnumAttribute("4")]
        ClearingFirm,
        [System.Xml.Serialization.XmlEnumAttribute("5")]
        InvestorID,
        [System.Xml.Serialization.XmlEnumAttribute("6")]
        IntroducingFirm,
        [System.Xml.Serialization.XmlEnumAttribute("7")]
        EnteringFirm,
        [System.Xml.Serialization.XmlEnumAttribute("8")]
        LocateLendingFirm,
        [System.Xml.Serialization.XmlEnumAttribute("9")]
        FundManager,
        [System.Xml.Serialization.XmlEnumAttribute("10")]
        SettlementLocation,
        [System.Xml.Serialization.XmlEnumAttribute("11")]
        InitiatingTrader,
        [System.Xml.Serialization.XmlEnumAttribute("12")]
        ExecutingTrader,
        [System.Xml.Serialization.XmlEnumAttribute("13")]
        OrderOriginator,
        [System.Xml.Serialization.XmlEnumAttribute("14")]
        GiveupClearingFirm,
        [System.Xml.Serialization.XmlEnumAttribute("15")]
        CorrespondantClearingFirm,
        [System.Xml.Serialization.XmlEnumAttribute("16")]
        ExecutingSystem,
        [System.Xml.Serialization.XmlEnumAttribute("17")]
        ContraFirm,
        [System.Xml.Serialization.XmlEnumAttribute("18")]
        ContraClearingFirm,
        [System.Xml.Serialization.XmlEnumAttribute("19")]
        SponsoringFirm,
        [System.Xml.Serialization.XmlEnumAttribute("20")]
        UndrContraFirm,
        [System.Xml.Serialization.XmlEnumAttribute("21")]
        ClearingOrganization,
        [System.Xml.Serialization.XmlEnumAttribute("22")]
        Exchange,
        [System.Xml.Serialization.XmlEnumAttribute("24")]
        CustomerAccount,
        [System.Xml.Serialization.XmlEnumAttribute("25")]
        CorrespondentClearingOrganization,
        [System.Xml.Serialization.XmlEnumAttribute("26")]
        CorrespondentBroker,
        [System.Xml.Serialization.XmlEnumAttribute("27")]
        BuyerSellerReceiverDeliverer,
        [System.Xml.Serialization.XmlEnumAttribute("28")]
        Custodian,
        [System.Xml.Serialization.XmlEnumAttribute("29")]
        Intermediary,
        [System.Xml.Serialization.XmlEnumAttribute("30")]
        Agent,
        [System.Xml.Serialization.XmlEnumAttribute("31")]
        SubCustodian,
        [System.Xml.Serialization.XmlEnumAttribute("32")]
        Beneficiary,
        [System.Xml.Serialization.XmlEnumAttribute("33")]
        InterestedParty,
        [System.Xml.Serialization.XmlEnumAttribute("34")]
        RegulatoryBody,
        [System.Xml.Serialization.XmlEnumAttribute("35")]
        LiquidityProvider,
        [System.Xml.Serialization.XmlEnumAttribute("36")]
        EnteringTrader,
        [System.Xml.Serialization.XmlEnumAttribute("37")]
        ContraTrader,
        [System.Xml.Serialization.XmlEnumAttribute("38")]
        PositionAccount,
        [System.Xml.Serialization.XmlEnumAttribute("39")]
        AllocEntity,
    }
    #endregion PartyRoleEnum
    #region PartySourceEnum
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fixprotocol.org/FIXML-4-4")]
    public enum PartySourceEnum
    {
        [System.Xml.Serialization.XmlEnumAttribute("A")]
        AustralianTaxFileNumber,
        [System.Xml.Serialization.XmlEnumAttribute("B")]
        BIC,
        [System.Xml.Serialization.XmlEnumAttribute("C")]
        AccptMarketPart,
        [System.Xml.Serialization.XmlEnumAttribute("D")]
        PropCode,
        [System.Xml.Serialization.XmlEnumAttribute("E")]
        ISOCode,
        [System.Xml.Serialization.XmlEnumAttribute("F")]
        SettlEntLoc,
        [System.Xml.Serialization.XmlEnumAttribute("G")]
        MIC,
        [System.Xml.Serialization.XmlEnumAttribute("H")]
        CSDPartCode,
        [System.Xml.Serialization.XmlEnumAttribute("I")]
        DirectedDefinedISITC,
        [System.Xml.Serialization.XmlEnumAttribute("1")]
        KoreanInvestorID,
        [System.Xml.Serialization.XmlEnumAttribute("2")]
        TaiwaneseQualified,
        [System.Xml.Serialization.XmlEnumAttribute("3")]
        TaiwaneseTradingAcct,
        [System.Xml.Serialization.XmlEnumAttribute("4")]
        MCDnumber,
        [System.Xml.Serialization.XmlEnumAttribute("5")]
        ChineseBShare,
        [System.Xml.Serialization.XmlEnumAttribute("6")]
        UKNationalInsPenNumber,
        [System.Xml.Serialization.XmlEnumAttribute("7")]
        USSocialSecurity,
        [System.Xml.Serialization.XmlEnumAttribute("8")]
        USEmployerIDNumber,
        [System.Xml.Serialization.XmlEnumAttribute("9")]
        AustralianBusinessNumber,
    }
    #endregion PartySourceEnum
    #region PartySubIDTypeEnum
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fixprotocol.org/FIXML-4-4")]
    public enum PartySubIDTypeEnum
    {
        [System.Xml.Serialization.XmlEnumAttribute("1")]
        Firm,
        [System.Xml.Serialization.XmlEnumAttribute("2")]
        Person,
        [System.Xml.Serialization.XmlEnumAttribute("3")]
        System,
        [System.Xml.Serialization.XmlEnumAttribute("4")]
        Application,
        [System.Xml.Serialization.XmlEnumAttribute("5")]
        FullLegalNameOfFirm,
        [System.Xml.Serialization.XmlEnumAttribute("6")]
        PostalAddress,
        [System.Xml.Serialization.XmlEnumAttribute("7")]
        PhoneNumber,
        [System.Xml.Serialization.XmlEnumAttribute("8")]
        EmailAddress,
        [System.Xml.Serialization.XmlEnumAttribute("9")]
        ContactName,
        [System.Xml.Serialization.XmlEnumAttribute("10")]
        SecuritiesAccountNumber,
        [System.Xml.Serialization.XmlEnumAttribute("11")]
        RegistrationNumber,
        [System.Xml.Serialization.XmlEnumAttribute("12")]
        RegisteredAddressForConfirmationPurposes,
        [System.Xml.Serialization.XmlEnumAttribute("13")]
        RegulatoryStatus,
        [System.Xml.Serialization.XmlEnumAttribute("14")]
        RegistrationName,
        [System.Xml.Serialization.XmlEnumAttribute("15")]
        CashAccount,
        [System.Xml.Serialization.XmlEnumAttribute("16")]
        BIC,
        [System.Xml.Serialization.XmlEnumAttribute("17")]
        CSDParticipantmemberCode,
        [System.Xml.Serialization.XmlEnumAttribute("18")]
        RegisteredAddress,
        [System.Xml.Serialization.XmlEnumAttribute("19")]
        FundaccountName,
        [System.Xml.Serialization.XmlEnumAttribute("20")]
        TelexNumber,
        [System.Xml.Serialization.XmlEnumAttribute("21")]
        FaxNumber,
        [System.Xml.Serialization.XmlEnumAttribute("22")]
        SecuritiesAccountName,
        [System.Xml.Serialization.XmlEnumAttribute("23")]
        CashAccountName,
        [System.Xml.Serialization.XmlEnumAttribute("24")]
        Department,
        [System.Xml.Serialization.XmlEnumAttribute("25")]
        LocationDesk,
        [System.Xml.Serialization.XmlEnumAttribute("26")]
        PositionAccountType,
        [System.Xml.Serialization.XmlEnumAttribute("4000")]
        ReservedAndAvailableForBilaterallyAgreedUponUserDefinedValues,
    }
    #endregion PartySubIDTypeEnum
    #region PegRoundDirectionEnum
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fixprotocol.org/FIXML-4-4")]
    public enum PegRoundDirectionEnum
    {
        [System.Xml.Serialization.XmlEnumAttribute("1")]
        MoreAggressive,
        [System.Xml.Serialization.XmlEnumAttribute("2")]
        MorePassive,
    }
    #endregion PegRoundDirectionEnum
    #region MoveTypeEnum
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fixprotocol.org/FIXML-4-4")]
    public enum MoveTypeEnum
    {
        [System.Xml.Serialization.XmlEnumAttribute("0")]
        Floating,
        [System.Xml.Serialization.XmlEnumAttribute("1")]
        Fixed,
    }
    #endregion MoveTypeEnum
    #region PositionEffectEnum
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fixprotocol.org/FIXML-4-4")]
    public enum PositionEffectEnum
    {
        [System.Xml.Serialization.XmlEnumAttribute("O")]
        Open,
        [System.Xml.Serialization.XmlEnumAttribute("C")]
        Close,
        [System.Xml.Serialization.XmlEnumAttribute("R")]
        Rolled,
        [System.Xml.Serialization.XmlEnumAttribute("F")]
        FIFO,
    }
    #endregion PositionEffectEnum
    #region PriceTypeEnum
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fixprotocol.org/FIXML-4-4")]
    public enum PriceTypeEnum
    {
        [System.Xml.Serialization.XmlEnumAttribute("1")]
        Pct,
        [System.Xml.Serialization.XmlEnumAttribute("2")]
        Cps,
        [System.Xml.Serialization.XmlEnumAttribute("3")]
        Abs,
        [System.Xml.Serialization.XmlEnumAttribute("4")]
        Discount,
        [System.Xml.Serialization.XmlEnumAttribute("5")]
        Premium,
        [System.Xml.Serialization.XmlEnumAttribute("6")]
        BpsBenchmark,
        [System.Xml.Serialization.XmlEnumAttribute("7")]
        TEDPrice,
        [System.Xml.Serialization.XmlEnumAttribute("8")]
        TEDYield,
        [System.Xml.Serialization.XmlEnumAttribute("9")]
        Yield,
        [System.Xml.Serialization.XmlEnumAttribute("10")]
        FixedCabinetTradePrice,
        [System.Xml.Serialization.XmlEnumAttribute("11")]
        VariableCabinetTradePrice,
    }
    #endregion PriceTypeEnum
    #region PriorityIndicatorEnum
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fixprotocol.org/FIXML-4-4")]
    public enum PriorityIndicatorEnum
    {
        [System.Xml.Serialization.XmlEnumAttribute("0")]
        PriorityUnchanged,
        [System.Xml.Serialization.XmlEnumAttribute("1")]
        LostPriority,
    }
    #endregion PriorityIndicatorEnum
    #region ReportToExchEnum
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fixprotocol.org/FIXML-4-4")]
    public enum ReportToExchEnum
    {
        [System.Xml.Serialization.XmlEnumAttribute("Y")]
        PartyMustRpt,
        [System.Xml.Serialization.XmlEnumAttribute("N")]
        PartySendingWillRpt,
    }
    #endregion ReportToExchEnum
    #region RoundDirectionEnum
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fixprotocol.org/FIXML-4-4")]
    public enum RoundDirectionEnum
    {
        [System.Xml.Serialization.XmlEnumAttribute("0")]
        Nearest,
        [System.Xml.Serialization.XmlEnumAttribute("1")]
        Down,
        [System.Xml.Serialization.XmlEnumAttribute("2")]
        Up,
    }
    #endregion RoundDirectionEnum
    #region QtyTypeEnum
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fixprotocol.org/FIXML-4-4")]
    public enum QtyTypeEnum
    {
        [System.Xml.Serialization.XmlEnumAttribute("0")]
        Units,
        [System.Xml.Serialization.XmlEnumAttribute("1")]
        Contracts,
    }
    #endregion QtyTypeEnum
    #region ScopeEnum
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fixprotocol.org/FIXML-4-4")]
    public enum ScopeEnum
    {
        [System.Xml.Serialization.XmlEnumAttribute("1")]
        Local,
        [System.Xml.Serialization.XmlEnumAttribute("2")]
        National,
        [System.Xml.Serialization.XmlEnumAttribute("3")]
        Global,
        [System.Xml.Serialization.XmlEnumAttribute("4")]
        NationalExcludingLocal,
    }
    #endregion ScopeEnum
    #region SecurityIDSourceEnum
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fixprotocol.org/FIXML-4-4")]
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
        ISOCurr,
        [System.Xml.Serialization.XmlEnumAttribute("7")]
        ISOCountry,
        [System.Xml.Serialization.XmlEnumAttribute("8")]
        ExchSymb,
        [System.Xml.Serialization.XmlEnumAttribute("9")]
        CTA,
        [System.Xml.Serialization.XmlEnumAttribute("A")]
        Blmbrg,
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
        Common,
        [System.Xml.Serialization.XmlEnumAttribute("H")]
        ClearingHouse,
        [System.Xml.Serialization.XmlEnumAttribute("I")]
        FpML,
        [System.Xml.Serialization.XmlEnumAttribute("J")]
        OptionPriceReportingAuthority,
        [System.Xml.Serialization.XmlEnumAttribute("100")]
        Proprietary,
    }
    #endregion SecurityIDSourceEnum
    #region SecurityTypeEnum
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fixprotocol.org/FIXML-4-4")]
    public enum SecurityTypeEnum
    {
        [System.Xml.Serialization.XmlEnumAttribute("EUSUPRA")]
        EuroSupranationalCoupons,
        [System.Xml.Serialization.XmlEnumAttribute("FAC")]
        FederalAgencyCoupon,
        [System.Xml.Serialization.XmlEnumAttribute("FADN")]
        FederalAgencyDiscountNote,
        [System.Xml.Serialization.XmlEnumAttribute("PEF")]
        PrivateExportFunding,
        [System.Xml.Serialization.XmlEnumAttribute("SUPRA")]
        USDSupranationalCoupons,
        [System.Xml.Serialization.XmlEnumAttribute("FUT")]
        Future,
        [System.Xml.Serialization.XmlEnumAttribute("OPT")]
        Option,
        [System.Xml.Serialization.XmlEnumAttribute("CORP")]
        CorporateBond,
        [System.Xml.Serialization.XmlEnumAttribute("CPP")]
        CorporatePrivatePlacement,
        [System.Xml.Serialization.XmlEnumAttribute("CB")]
        ConvertibleBond,
        [System.Xml.Serialization.XmlEnumAttribute("DUAL")]
        DualCurrency,
        [System.Xml.Serialization.XmlEnumAttribute("EUCORP")]
        EuroCorporateBond,
        [System.Xml.Serialization.XmlEnumAttribute("XLINKD")]
        IndexedLinked,
        [System.Xml.Serialization.XmlEnumAttribute("STRUCT")]
        StructuredNotes,
        [System.Xml.Serialization.XmlEnumAttribute("YANK")]
        YankeeCorporateBond,
        [System.Xml.Serialization.XmlEnumAttribute("FOR")]
        ForeignExchangeContract,
        [System.Xml.Serialization.XmlEnumAttribute("CS")]
        CommonStock,
        [System.Xml.Serialization.XmlEnumAttribute("PS")]
        PreferredStock,
        [System.Xml.Serialization.XmlEnumAttribute("BRADY")]
        BradyBond,
        [System.Xml.Serialization.XmlEnumAttribute("EUSOV")]
        EuroSovereigns,
        [System.Xml.Serialization.XmlEnumAttribute("TBOND")]
        USTreasuryBond,
        [System.Xml.Serialization.XmlEnumAttribute("TINT")]
        InterestStripFromAnyBondOrNote,
        [System.Xml.Serialization.XmlEnumAttribute("TIPS")]
        TreasuryInflationProtectedSecurities,
        [System.Xml.Serialization.XmlEnumAttribute("TCAL")]
        PrincipalStripOfACallableBondOrNote,
        [System.Xml.Serialization.XmlEnumAttribute("TPRN")]
        PrincipalStripFromANoncallableBondOrNote,
        [System.Xml.Serialization.XmlEnumAttribute("UST")]
        USTreasuryNoteDeprecatedValueUseTNOTE,
        [System.Xml.Serialization.XmlEnumAttribute("USTB")]
        USTreasuryBillDeprecatedValueUseTBILL,
        [System.Xml.Serialization.XmlEnumAttribute("TNOTE")]
        USTreasuryNote,
        [System.Xml.Serialization.XmlEnumAttribute("TBILL")]
        USTreasuryBill,
        [System.Xml.Serialization.XmlEnumAttribute("REPO")]
        Repurchase,
        [System.Xml.Serialization.XmlEnumAttribute("FORWARD")]
        Forward,
        [System.Xml.Serialization.XmlEnumAttribute("BUYSELL")]
        BuySellback,
        [System.Xml.Serialization.XmlEnumAttribute("SECLOAN")]
        SecuritiesLoan,
        [System.Xml.Serialization.XmlEnumAttribute("SECPLEDGE")]
        SecuritiesPledge,
        [System.Xml.Serialization.XmlEnumAttribute("TERM")]
        TermLoan,
        [System.Xml.Serialization.XmlEnumAttribute("RVLV")]
        RevolverLoan,
        [System.Xml.Serialization.XmlEnumAttribute("RVLVTRM")]
        RevolverTermLoan,
        [System.Xml.Serialization.XmlEnumAttribute("BRIDGE")]
        BridgeLoan,
        [System.Xml.Serialization.XmlEnumAttribute("LOFC")]
        LetterOfCredit,
        [System.Xml.Serialization.XmlEnumAttribute("SWING")]
        SwingLineFacility,
        [System.Xml.Serialization.XmlEnumAttribute("DINP")]
        DebtorInPossession,
        [System.Xml.Serialization.XmlEnumAttribute("DEFLTED")]
        Defaulted,
        [System.Xml.Serialization.XmlEnumAttribute("WITHDRN")]
        Withdrawn,
        [System.Xml.Serialization.XmlEnumAttribute("REPLACD")]
        Replaced,
        [System.Xml.Serialization.XmlEnumAttribute("MATURED")]
        Matured,
        [System.Xml.Serialization.XmlEnumAttribute("AMENDED")]
        AmendedRestated,
        [System.Xml.Serialization.XmlEnumAttribute("RETIRED")]
        Retired,
        [System.Xml.Serialization.XmlEnumAttribute("BA")]
        BankersAcceptance,
        [System.Xml.Serialization.XmlEnumAttribute("BN")]
        BankNotes,
        [System.Xml.Serialization.XmlEnumAttribute("BOX")]
        BillOfExchanges,
        [System.Xml.Serialization.XmlEnumAttribute("CD")]
        CertificateOfDeposit,
        [System.Xml.Serialization.XmlEnumAttribute("CL")]
        CallLoans,
        [System.Xml.Serialization.XmlEnumAttribute("CP")]
        CommercialPaper,
        [System.Xml.Serialization.XmlEnumAttribute("DN")]
        DepositNotes,
        [System.Xml.Serialization.XmlEnumAttribute("EUCD")]
        EuroCertificateOfDeposit,
        [System.Xml.Serialization.XmlEnumAttribute("EUCP")]
        EuroCommercialPaper,
        [System.Xml.Serialization.XmlEnumAttribute("LQN")]
        LiquidityNote,
        [System.Xml.Serialization.XmlEnumAttribute("MTN")]
        MediumTermNotes,
        [System.Xml.Serialization.XmlEnumAttribute("ONITE")]
        Overnight,
        [System.Xml.Serialization.XmlEnumAttribute("PN")]
        PromissoryNote,
        [System.Xml.Serialization.XmlEnumAttribute("PZFJ")]
        PlazosFijos,
        [System.Xml.Serialization.XmlEnumAttribute("STN")]
        ShortTermLoanNote,
        [System.Xml.Serialization.XmlEnumAttribute("TD")]
        TimeDeposit,
        [System.Xml.Serialization.XmlEnumAttribute("XCN")]
        ExtendedCommNote,
        [System.Xml.Serialization.XmlEnumAttribute("YCD")]
        YankeeCertificateOfDeposit,
        [System.Xml.Serialization.XmlEnumAttribute("ABS")]
        AssetbackedSecurities,
        [System.Xml.Serialization.XmlEnumAttribute("CMBS")]
        CorpMortgagebackedSecurities,
        [System.Xml.Serialization.XmlEnumAttribute("CMO")]
        CollateralizedMortgageObligation,
        [System.Xml.Serialization.XmlEnumAttribute("IET")]
        IOETTEMortgage,
        [System.Xml.Serialization.XmlEnumAttribute("MBS")]
        MortgagebackedSecurities,
        [System.Xml.Serialization.XmlEnumAttribute("MIO")]
        MortgageInterestOnly,
        [System.Xml.Serialization.XmlEnumAttribute("MPO")]
        MortgagePrincipalOnly,
        [System.Xml.Serialization.XmlEnumAttribute("MPP")]
        MortgagePrivatePlacement,
        [System.Xml.Serialization.XmlEnumAttribute("MPT")]
        MiscellaneousPassthrough,
        [System.Xml.Serialization.XmlEnumAttribute("PFAND")]
        Pfandbriefe,
        [System.Xml.Serialization.XmlEnumAttribute("TBA")]
        ToBeAnnounced,
        [System.Xml.Serialization.XmlEnumAttribute("AN")]
        OtherAnticipationNotesBANGANEtc,
        [System.Xml.Serialization.XmlEnumAttribute("COFO")]
        CertificateOfObligation,
        [System.Xml.Serialization.XmlEnumAttribute("COFP")]
        CertificateOfParticipation,
        [System.Xml.Serialization.XmlEnumAttribute("GO")]
        GeneralObligationBonds,
        [System.Xml.Serialization.XmlEnumAttribute("MT")]
        MandatoryTender,
        [System.Xml.Serialization.XmlEnumAttribute("RAN")]
        RevenueAnticipationNote,
        [System.Xml.Serialization.XmlEnumAttribute("REV")]
        RevenueBonds,
        [System.Xml.Serialization.XmlEnumAttribute("SPCLA")]
        SpecialAssessment,
        [System.Xml.Serialization.XmlEnumAttribute("SPCLO")]
        SpecialObligation,
        [System.Xml.Serialization.XmlEnumAttribute("SPCLT")]
        SpecialTax,
        [System.Xml.Serialization.XmlEnumAttribute("TAN")]
        TaxAnticipationNote,
        [System.Xml.Serialization.XmlEnumAttribute("TAXA")]
        TaxAllocation,
        [System.Xml.Serialization.XmlEnumAttribute("TECP")]
        TaxExemptCommercialPaper,
        [System.Xml.Serialization.XmlEnumAttribute("TRAN")]
        TaxRevenueAnticipationNote,
        [System.Xml.Serialization.XmlEnumAttribute("VRDN")]
        VariableRateDemandNote,
        [System.Xml.Serialization.XmlEnumAttribute("WAR")]
        Warrant,
        [System.Xml.Serialization.XmlEnumAttribute("MF")]
        MutualFund,
        [System.Xml.Serialization.XmlEnumAttribute("MLEG")]
        MultilegInstrument,
        [System.Xml.Serialization.XmlEnumAttribute("NONE")]
        NoSecurityType,
        [System.Xml.Serialization.XmlEnumAttribute("WLD")]
        WildcardEntry,
    }
    #endregion SecurityTypeEnum
    #region SettlTypeEnum
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fixprotocol.org/FIXML-4-4")]
    public enum SettlTypeEnum
    {
        [System.Xml.Serialization.XmlEnumAttribute("0")]
        Regular,
        [System.Xml.Serialization.XmlEnumAttribute("1")]
        Cash,
        [System.Xml.Serialization.XmlEnumAttribute("2")]
        NextDay,
        [System.Xml.Serialization.XmlEnumAttribute("3")]
        T2,
        [System.Xml.Serialization.XmlEnumAttribute("4")]
        T3,
        [System.Xml.Serialization.XmlEnumAttribute("5")]
        T4,
        [System.Xml.Serialization.XmlEnumAttribute("6")]
        Future,
        [System.Xml.Serialization.XmlEnumAttribute("7")]
        WhenIssued,
        [System.Xml.Serialization.XmlEnumAttribute("8")]
        SellersOption,
        [System.Xml.Serialization.XmlEnumAttribute("9")]
        T5,
        [System.Xml.Serialization.XmlEnumAttribute("A")]
        T1,
    }
    #endregion SettlTypeEnum
    #region StipulationTypeEnum
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fixprotocol.org/FIXML-4-4")]
    public enum StipulationTypeEnum
    {
        [System.Xml.Serialization.XmlEnumAttribute("AMT")]
        AMT,
        [System.Xml.Serialization.XmlEnumAttribute("AUTOREINV")]
        AutoReinvestmentAtRateOrBetter,
        [System.Xml.Serialization.XmlEnumAttribute("BANKQUAL")]
        BankQualified,
        [System.Xml.Serialization.XmlEnumAttribute("BGNCON")]
        BargainConditions,
        [System.Xml.Serialization.XmlEnumAttribute("COUPON")]
        CouponRange,
        [System.Xml.Serialization.XmlEnumAttribute("CURRENCY")]
        ISOCurrencyCode,
        [System.Xml.Serialization.XmlEnumAttribute("CUSTOMDATE")]
        CustomStartendDate,
        [System.Xml.Serialization.XmlEnumAttribute("GEOG")]
        GeographicsAndRange,
        [System.Xml.Serialization.XmlEnumAttribute("HAIRCUT")]
        ValuationDiscount,
        [System.Xml.Serialization.XmlEnumAttribute("INSURED")]
        Insured,
        [System.Xml.Serialization.XmlEnumAttribute("ISSUE")]
        YearOrYearMonthOfIssue,
        [System.Xml.Serialization.XmlEnumAttribute("ISSUER")]
        IssuersTicker,
        [System.Xml.Serialization.XmlEnumAttribute("ISSUESIZE")]
        IssueSizeRange,
        [System.Xml.Serialization.XmlEnumAttribute("LOOKBACK")]
        LookbackDays,
        [System.Xml.Serialization.XmlEnumAttribute("LOT")]
        ExplicitLotIdentifier,
        [System.Xml.Serialization.XmlEnumAttribute("LOTVAR")]
        LotVarianceValueInPercentMaximumOverOrUnderallocationAllowed,
        [System.Xml.Serialization.XmlEnumAttribute("MAT")]
        MaturityYearAndMonth,
        [System.Xml.Serialization.XmlEnumAttribute("MATURITY")]
        MaturityRange,
        [System.Xml.Serialization.XmlEnumAttribute("MAXSUBS")]
        MaximumSubstitutionsRepo,
        [System.Xml.Serialization.XmlEnumAttribute("MINQTY")]
        MinimumQuantity,
        [System.Xml.Serialization.XmlEnumAttribute("MININCR")]
        MinimumIncrement,
        [System.Xml.Serialization.XmlEnumAttribute("MINDNOM")]
        MinimumDenomination,
        [System.Xml.Serialization.XmlEnumAttribute("MAXDNOM")]
        MaximumDenomination,
        [System.Xml.Serialization.XmlEnumAttribute("PAYFREQ")]
        PaymentFrequencyCalendar,
        [System.Xml.Serialization.XmlEnumAttribute("PIECES")]
        NumberOfPieces,
        [System.Xml.Serialization.XmlEnumAttribute("PMIN")]
        PoolsMinimum,
        [System.Xml.Serialization.XmlEnumAttribute("PMAX")]
        PoolsMaximum,
        [System.Xml.Serialization.XmlEnumAttribute("PPM")]
        PoolsPerMillion,
        [System.Xml.Serialization.XmlEnumAttribute("PPL")]
        PoolsPerLot,
        [System.Xml.Serialization.XmlEnumAttribute("PPT")]
        PoolsPerTrade,
        [System.Xml.Serialization.XmlEnumAttribute("PRICE")]
        PriceRange,
        [System.Xml.Serialization.XmlEnumAttribute("PRICEFREQ")]
        PricingFrequency,
        [System.Xml.Serialization.XmlEnumAttribute("PROD")]
        ProductionYear,
        [System.Xml.Serialization.XmlEnumAttribute("PROTECT")]
        CallProtection,
        [System.Xml.Serialization.XmlEnumAttribute("PURPOSE")]
        Purpose,
        [System.Xml.Serialization.XmlEnumAttribute("PXSOURCE")]
        BenchmarkPriceSource,
        [System.Xml.Serialization.XmlEnumAttribute("RATING")]
        RatingSourceAndRange,
        [System.Xml.Serialization.XmlEnumAttribute("REDEMPTION")]
        TypeOfRedemptionValuesAre,
        [System.Xml.Serialization.XmlEnumAttribute("RESTRICTED")]
        Restricted,
        [System.Xml.Serialization.XmlEnumAttribute("SECTOR")]
        MarketSector,
        [System.Xml.Serialization.XmlEnumAttribute("SECTYPE")]
        SecurityTypeIncludedOrExcluded,
        [System.Xml.Serialization.XmlEnumAttribute("STRUCT")]
        Structure,
        [System.Xml.Serialization.XmlEnumAttribute("SUBSFREQ")]
        SubstitutionsFrequencyRepo,
        [System.Xml.Serialization.XmlEnumAttribute("SUBSLEFT")]
        SubstitutionsLeftRepo,
        [System.Xml.Serialization.XmlEnumAttribute("TEXT")]
        FreeformText,
        [System.Xml.Serialization.XmlEnumAttribute("TRDVAR")]
        TradeVarianceValueInPercentMaximumOverOrUnderallocationAllowed,
        [System.Xml.Serialization.XmlEnumAttribute("WAC")]
        WeightedAverageCoupon,
        [System.Xml.Serialization.XmlEnumAttribute("WAL")]
        WeightedAverageLifeCoupon,
        [System.Xml.Serialization.XmlEnumAttribute("WALA")]
        WeightedAverageLoanAge,
        [System.Xml.Serialization.XmlEnumAttribute("WAM")]
        WeightedAverageMaturity,
        [System.Xml.Serialization.XmlEnumAttribute("WHOLE")]
        WholePool,
        [System.Xml.Serialization.XmlEnumAttribute("YIELD")]
        YieldRange,
        [System.Xml.Serialization.XmlEnumAttribute("SMM")]
        SingleMonthlyMortality,
        [System.Xml.Serialization.XmlEnumAttribute("CPR")]
        ConstantPrepaymentRate,
        [System.Xml.Serialization.XmlEnumAttribute("CPY")]
        ConstantPrepaymentYield,
        [System.Xml.Serialization.XmlEnumAttribute("CPP")]
        ConstantPrepaymentPenalty,
        [System.Xml.Serialization.XmlEnumAttribute("ABS")]
        AbsolutePrepaymentSpeed,
        [System.Xml.Serialization.XmlEnumAttribute("MPR")]
        MonthlyPrepaymentRate,
        [System.Xml.Serialization.XmlEnumAttribute("PSA")]
        PercentOfBMAPrepaymentCurve,
        [System.Xml.Serialization.XmlEnumAttribute("PPC")]
        PercentOfProspectusPrepaymentCurve,
        [System.Xml.Serialization.XmlEnumAttribute("MHP")]
        PercentOfManufacturedHousingPrepaymentCurve,
        [System.Xml.Serialization.XmlEnumAttribute("HEP")]
        finalCPROfHomeEquityPrepaymentCurve,
    }
    #endregion StipulationTypeEnum
    #region TargetStrategyEnum
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fixprotocol.org/FIXML-4-4")]
    public enum TargetStrategyEnum
    {
        [System.Xml.Serialization.XmlEnumAttribute("1")]
        VWAP,
        [System.Xml.Serialization.XmlEnumAttribute("2")]
        Participate,
        [System.Xml.Serialization.XmlEnumAttribute("3")]
        MininizeMarketImpact,
        [System.Xml.Serialization.XmlEnumAttribute("1000")]
        ReservedAndAvailableForBilaterallyAgreedUponUserDefinedValues,
    }
    #endregion TargetStrategyEnum
    #region TimeInForceEnum
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fixprotocol.org/FIXML-4-4")]
    public enum TimeInForceEnum
    {
        [System.Xml.Serialization.XmlEnumAttribute("0")]
        Day,
        [System.Xml.Serialization.XmlEnumAttribute("1")]
        GoodTillCancel,
        [System.Xml.Serialization.XmlEnumAttribute("2")]
        AtTheOpening,
        [System.Xml.Serialization.XmlEnumAttribute("3")]
        ImmediateOrCancel,
        [System.Xml.Serialization.XmlEnumAttribute("4")]
        FillOrKill,
        [System.Xml.Serialization.XmlEnumAttribute("5")]
        GoodTillCrossing,
        [System.Xml.Serialization.XmlEnumAttribute("6")]
        GoodTillDate,
        [System.Xml.Serialization.XmlEnumAttribute("7")]
        AtTheClose,
    }
    #endregion TimeInForceEnum
    #endregion Enums Version 4.4
}

namespace FixML.v50SP1.Enum
{
    #region Enums Version 5.0 SP1
    #region AggressorIndicatorEnum
    /* AggressorIndicator(1057) */
    [System.SerializableAttribute()]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fixprotocol.org/FIXML-5-0-SP1")]
    public enum AggressorIndicatorEnum
    {
        [System.Xml.Serialization.XmlEnumAttribute("Y")]
        Aggressor,
        [System.Xml.Serialization.XmlEnumAttribute("N")]
        Passive,
    }
    #endregion AggressorIndicatorEnum
    #region AllocAcctIDSourceEnum
    /* AllocAcctIDSource(661) */
    [System.SerializableAttribute()]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fixprotocol.org/FIXML-5-0-SP1")]
    public enum AllocAcctIDSourceEnum
    {
        [System.Xml.Serialization.XmlEnumAttribute("1")]
        BIC,
        [System.Xml.Serialization.XmlEnumAttribute("2")]
        SID,
        [System.Xml.Serialization.XmlEnumAttribute("3")]
        TFM,
        [System.Xml.Serialization.XmlEnumAttribute("4")]
        OMGEO,
        [System.Xml.Serialization.XmlEnumAttribute("5")]
        DTCC,
        [System.Xml.Serialization.XmlEnumAttribute("99")]
        Other,
    }
    #endregion AllocAcctIDSourceEnum
    #region AllocMethodEnum
    /* AllocMethod(1002) */
    [System.SerializableAttribute()]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fixprotocol.org/FIXML-5-0-SP1")]
    public enum AllocMethodEnum
    {
        [System.Xml.Serialization.XmlEnumAttribute("1")]
        Automatic,
        [System.Xml.Serialization.XmlEnumAttribute("2")]
        Guarantor,
        [System.Xml.Serialization.XmlEnumAttribute("3")]
        Manual,
    }
    #endregion AllocMethodEnum
    #region AsOfIndicatorEnum
    /* AsOfIndicator(1015) */
    [System.SerializableAttribute()]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fixprotocol.org/FIXML-5-0-SP1")]
    public enum AsOfIndicatorEnum
    {
        [System.Xml.Serialization.XmlEnumAttribute("0")]
        NotAnAsOfTrade,
        [System.Xml.Serialization.XmlEnumAttribute("1")]
        AsOfTrade,
    }
    #endregion AsOfIndicatorEnum
    #region AssignmentMethodEnum
    /* AssignmentMethod(744) */
    [System.SerializableAttribute()]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fixprotocol.org/FIXML-5-0-SP1")]
    public enum AssignmentMethodEnum
    {
        [System.Xml.Serialization.XmlEnumAttribute("P")]
        ProRata,
        [System.Xml.Serialization.XmlEnumAttribute("R")]
        Random,
        [System.Xml.Serialization.XmlEnumAttribute("F")]
        Fifo,
    }
    #endregion AssignmentMethodEnum
    #region AvgPxIndicatorEnum
    /* AvgPxIndicator(819) */
    [System.SerializableAttribute()]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fixprotocol.org/FIXML-5-0-SP1")]
    public enum AvgPxIndicatorEnum
    {
        [System.Xml.Serialization.XmlEnumAttribute("0")]
        NoAveragePricing,
        [System.Xml.Serialization.XmlEnumAttribute("1")]
        PartOfAnAveragePriceGroup,
        [System.Xml.Serialization.XmlEnumAttribute("2")]
        LastTradeIsTheAveragePriceGroup,
    }
    #endregion AvgPxIndicatorEnum
    #region BenchmarkCurveNameEnum
    /* BenchmarkCurveName(221) */
    [System.SerializableAttribute()]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fixprotocol.org/FIXML-5-0-SP1")]
    public enum BenchmarkCurveNameEnum
    {
        EONIA,
        EUREPO,
        Euribor,
        FutureSWAP,
        LIBID,
        LIBOR,
        MuniAAA,
        OTHER,
        Pfandbriefe,
        SONIA,
        SWAP,
        Treasury,
    }
    #endregion BenchmarkCurveNameEnum
    #region ClearingInstructionEnum
    /* ClearingInstruction(577) */
    [System.SerializableAttribute()]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fixprotocol.org/FIXML-5-0-SP1")]
    public enum ClearingInstructionEnum
    {
        [System.Xml.Serialization.XmlEnumAttribute("0")]
        ProcessNormally,
        [System.Xml.Serialization.XmlEnumAttribute("1")]
        ExcludeFromAllNetting,
        [System.Xml.Serialization.XmlEnumAttribute("2")]
        BilateralNettingOnly,
        [System.Xml.Serialization.XmlEnumAttribute("3")]
        ExClearing,
        [System.Xml.Serialization.XmlEnumAttribute("4")]
        SpecialTrade,
        [System.Xml.Serialization.XmlEnumAttribute("5")]
        MultilateralNetting,
        [System.Xml.Serialization.XmlEnumAttribute("6")]
        ClearAgainstCentralCounterparty,
        [System.Xml.Serialization.XmlEnumAttribute("7")]
        ExcludeFromCentralCounterparty,
        [System.Xml.Serialization.XmlEnumAttribute("8")]
        ManualMode,
        [System.Xml.Serialization.XmlEnumAttribute("9")]
        AutomaticPostingMode,
        [System.Xml.Serialization.XmlEnumAttribute("10")]
        AutomaticGiveUpMode,
        [System.Xml.Serialization.XmlEnumAttribute("11")]
        QualifiedServiceRepresentative,
        [System.Xml.Serialization.XmlEnumAttribute("12")]
        CustomerTrade,
        [System.Xml.Serialization.XmlEnumAttribute("13")]
        SelfClearing,
    }
    #endregion ClearingInstructionEnum
    #region DeliveryFormEnum
    /* DeliveryForm(668) */
    [System.SerializableAttribute()]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fixprotocol.org/FIXML-5-0-SP1")]
    public enum DeliveryFormEnum
    {
        [System.Xml.Serialization.XmlEnumAttribute("1")]
        BookEntry,
        [System.Xml.Serialization.XmlEnumAttribute("2")]
        Bearer,
    }
    #endregion DeliveryFormEnum
    #region DerivativeExerciseStyleEnum
    /* ExerciseStyle(1194) */
    [System.SerializableAttribute()]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fixprotocol.org/FIXML-5-0-SP1")]
    public enum DerivativeExerciseStyleEnum
    {
        [System.Xml.Serialization.XmlEnumAttribute("0")]
        European,
        [System.Xml.Serialization.XmlEnumAttribute("1")]
        American,
        [System.Xml.Serialization.XmlEnumAttribute("2")]
        Bermuda,
    }
    #endregion DerivativeExerciseStyleEnum
    #region DerivativeRoundingDirectionEnum
    /* RoundingDirection(468) */
    [System.SerializableAttribute()]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fixprotocol.org/FIXML-5-0-SP1")]
    public enum DerivativeRoundingDirectionEnum
    {
        [System.Xml.Serialization.XmlEnumAttribute("0")]
        ToNearest,
        [System.Xml.Serialization.XmlEnumAttribute("1")]
        Down,
        [System.Xml.Serialization.XmlEnumAttribute("2")]
        Up,
    }
    #endregion DerivativeRoundingDirectionEnum
    #region DeskOrderHandlingInstEnum
    /* DeskOrderHandlingInst(1035) */
    [System.SerializableAttribute()]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fixprotocol.org/FIXML-5-0-SP1")]
    public enum DeskOrderHandlingInstEnum
    {
        [System.Xml.Serialization.XmlEnumAttribute("ADD")]
        AddOnOrder,
        [System.Xml.Serialization.XmlEnumAttribute("AON")]
        AllOrNone,
        [System.Xml.Serialization.XmlEnumAttribute("CNH")]
        CashNotHeld,
        [System.Xml.Serialization.XmlEnumAttribute("DIR")]
        DirectedOrder,
        [System.Xml.Serialization.XmlEnumAttribute("E.W")]
        ExchangeForPhysicalTransaction,
        [System.Xml.Serialization.XmlEnumAttribute("FOK")]
        FillOrKill,
        [System.Xml.Serialization.XmlEnumAttribute("IO")]
        ImbalanceOnly,
        [System.Xml.Serialization.XmlEnumAttribute("IOC")]
        ImmediateOrCancel,
        [System.Xml.Serialization.XmlEnumAttribute("LOO")]
        LimitOnOpen,
        [System.Xml.Serialization.XmlEnumAttribute("LOC")]
        LimitOnClose,
        [System.Xml.Serialization.XmlEnumAttribute("MAO")]
        MarketAtOpen,
        [System.Xml.Serialization.XmlEnumAttribute("MAC")]
        MarketAtClose,
        [System.Xml.Serialization.XmlEnumAttribute("MOO")]
        MarketOnOpen,
        [System.Xml.Serialization.XmlEnumAttribute("MOC")]
        MarketOnClose,
        [System.Xml.Serialization.XmlEnumAttribute("MQT")]
        MinimumQuantity,
        [System.Xml.Serialization.XmlEnumAttribute("NH")]
        NotHeld,
        [System.Xml.Serialization.XmlEnumAttribute("OVD")]
        OverTheDay,
        [System.Xml.Serialization.XmlEnumAttribute("PEG")]
        Pegged,
        [System.Xml.Serialization.XmlEnumAttribute("RSV")]
        ReserveSizeOrder,
        [System.Xml.Serialization.XmlEnumAttribute("S.W")]
        StopStockTransaction,
        [System.Xml.Serialization.XmlEnumAttribute("SCL")]
        Scale,
        [System.Xml.Serialization.XmlEnumAttribute("TMO")]
        TimeOrder,
        [System.Xml.Serialization.XmlEnumAttribute("TS")]
        TrailingStop,
        [System.Xml.Serialization.XmlEnumAttribute("WRK")]
        Work,
    }
    #endregion DeskOrderHandlingInstEnum
    #region DeskTypeEnum
    /* DeskType(1033) */
    [System.SerializableAttribute()]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fixprotocol.org/FIXML-5-0-SP1")]
    public enum DeskTypeEnum
    {
        [System.Xml.Serialization.XmlEnumAttribute("A")]
        Agency,
        [System.Xml.Serialization.XmlEnumAttribute("AR")]
        Arbitrage,
        [System.Xml.Serialization.XmlEnumAttribute("D")]
        Derivatives,
        [System.Xml.Serialization.XmlEnumAttribute("IN")]
        International,
        [System.Xml.Serialization.XmlEnumAttribute("IS")]
        Institutional,
        [System.Xml.Serialization.XmlEnumAttribute("O")]
        Other,
        [System.Xml.Serialization.XmlEnumAttribute("PF")]
        PreferredTrading,
        [System.Xml.Serialization.XmlEnumAttribute("PR")]
        Proprietary,
        [System.Xml.Serialization.XmlEnumAttribute("PT")]
        ProgramTrading,
        [System.Xml.Serialization.XmlEnumAttribute("S")]
        Sales,
        [System.Xml.Serialization.XmlEnumAttribute("T")]
        Trading,
    }
    #endregion DeskTypeEnum
    #region DeskTypeSourceEnum
    /* DeskTypeSource(1034) */
    [System.SerializableAttribute()]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fixprotocol.org/FIXML-5-0-SP1")]
    public enum DeskTypeSourceEnum
    {
        [System.Xml.Serialization.XmlEnumAttribute("1")]
        NASD_OATS,
    }
    #endregion DeskTypeSourceEnum
    #region EventTypeEnum
    /* EventType(865) */
    [System.SerializableAttribute()]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fixprotocol.org/FIXML-5-0-SP1")]
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
        [System.Xml.Serialization.XmlEnumAttribute("99")]
        Other,
    }
    #endregion EventTypeEnum
    #region ExecInstEnum
    /* ExecInst(18) */
    [System.SerializableAttribute()]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fixprotocol.org/FIXML-5-0-SP1")]
    public enum ExecInstEnum
    {
        [System.Xml.Serialization.XmlEnumAttribute("0")]
        StayOnOfferSide,
        [System.Xml.Serialization.XmlEnumAttribute("1")]
        NotHeld,
        [System.Xml.Serialization.XmlEnumAttribute("2")]
        Work,
        [System.Xml.Serialization.XmlEnumAttribute("3")]
        GoAlong,
        [System.Xml.Serialization.XmlEnumAttribute("4")]
        OverTheDay,
        [System.Xml.Serialization.XmlEnumAttribute("5")]
        Held,
        [System.Xml.Serialization.XmlEnumAttribute("6")]
        ParticipantDontInitiate,
        [System.Xml.Serialization.XmlEnumAttribute("7")]
        StrictScale,
        [System.Xml.Serialization.XmlEnumAttribute("8")]
        TryToScale,
        [System.Xml.Serialization.XmlEnumAttribute("9")]
        StayOnBidSide,
        [System.Xml.Serialization.XmlEnumAttribute("A")]
        NoCross,
        [System.Xml.Serialization.XmlEnumAttribute("B")]
        OKToCross,
        [System.Xml.Serialization.XmlEnumAttribute("C")]
        CallFirst,
        [System.Xml.Serialization.XmlEnumAttribute("D")]
        PercentOfVolume,
        [System.Xml.Serialization.XmlEnumAttribute("E")]
        DoNotIncrease,
        [System.Xml.Serialization.XmlEnumAttribute("F")]
        DoNotReduce,
        [System.Xml.Serialization.XmlEnumAttribute("G")]
        AllOrNone,
        [System.Xml.Serialization.XmlEnumAttribute("H")]
        ReinstateOnSystemFailure,
        [System.Xml.Serialization.XmlEnumAttribute("I")]
        InstitutionsOnly,
        [System.Xml.Serialization.XmlEnumAttribute("J")]
        ReinstateOnTradingHalt,
        [System.Xml.Serialization.XmlEnumAttribute("K")]
        CancelOnTradingHalt,
        [System.Xml.Serialization.XmlEnumAttribute("L")]
        LastPeg,
        [System.Xml.Serialization.XmlEnumAttribute("M")]
        MidPricePeg,
        [System.Xml.Serialization.XmlEnumAttribute("N")]
        NonNegotiable,
        [System.Xml.Serialization.XmlEnumAttribute("O")]
        OpeningPeg,
        [System.Xml.Serialization.XmlEnumAttribute("P")]
        MarketPeg,
        [System.Xml.Serialization.XmlEnumAttribute("Q")]
        CancelOnSystemFailure,
        [System.Xml.Serialization.XmlEnumAttribute("R")]
        PrimaryPeg,
        [System.Xml.Serialization.XmlEnumAttribute("S")]
        Suspend,
        [System.Xml.Serialization.XmlEnumAttribute("T")]
        FixedPegToLocalBest,
        [System.Xml.Serialization.XmlEnumAttribute("U")]
        CustomerDisplayInstruction,
        [System.Xml.Serialization.XmlEnumAttribute("V")]
        Netting,
        [System.Xml.Serialization.XmlEnumAttribute("W")]
        PegToVWAP,
        [System.Xml.Serialization.XmlEnumAttribute("X")]
        TradeAlong,
        [System.Xml.Serialization.XmlEnumAttribute("Y")]
        TryToStop,
        [System.Xml.Serialization.XmlEnumAttribute("Z")]
        CancelIfNotBest,
        [System.Xml.Serialization.XmlEnumAttribute("a")]
        TrailingStopPeg,
        [System.Xml.Serialization.XmlEnumAttribute("b")]
        StrictLimit,
        [System.Xml.Serialization.XmlEnumAttribute("c")]
        IgnorePriceValidityChecks,
        [System.Xml.Serialization.XmlEnumAttribute("d")]
        PegToLimitPrice,
        [System.Xml.Serialization.XmlEnumAttribute("e")]
        WorkToTargetStrategy,
        [System.Xml.Serialization.XmlEnumAttribute("f")]
        IntermarketSweep,
        [System.Xml.Serialization.XmlEnumAttribute("g")]
        ExternalRoutingAllowed,
        [System.Xml.Serialization.XmlEnumAttribute("h")]
        ExternalRoutingNotAllowed,
        [System.Xml.Serialization.XmlEnumAttribute("i")]
        ImbalanceOnly,
        [System.Xml.Serialization.XmlEnumAttribute("j")]
        SingleExecutionRequestedForBlockTrade,
        [System.Xml.Serialization.XmlEnumAttribute("k")]
        BestExecution,
        [System.Xml.Serialization.XmlEnumAttribute("l")]
        SuspendOnSystemFailure,
        [System.Xml.Serialization.XmlEnumAttribute("m")]
        SuspendOnTradingHalt,
        [System.Xml.Serialization.XmlEnumAttribute("n")]
        ReinstateOnConnectionLoss,
        [System.Xml.Serialization.XmlEnumAttribute("o")]
        CancelOnConnectionLoss,
        [System.Xml.Serialization.XmlEnumAttribute("p")]
        SuspendOnConnectionLoss,
        [System.Xml.Serialization.XmlEnumAttribute("q")]
        ReleaseFromSuspension,
        [System.Xml.Serialization.XmlEnumAttribute("r")]
        ExecuteAsDeltaNeutral,
        [System.Xml.Serialization.XmlEnumAttribute("s")]
        ExecuteAsDurationNeutral,
        [System.Xml.Serialization.XmlEnumAttribute("t")]
        ExecuteAsFXNeutral,
    }
    #endregion ExecInstEnum
    #region FundRenewWaivEnum
    /* FundRenewWaiv(497) */
    [System.SerializableAttribute()]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fixprotocol.org/FIXML-5-0-SP1")]
    public enum FundRenewWaivEnum
    {
        [System.Xml.Serialization.XmlEnumAttribute("N")]
        No,
        [System.Xml.Serialization.XmlEnumAttribute("Y")]
        Yes,
    }
    #endregion FundRenewWaivEnum
    #region FuturesValuationMethodEnum
    /* FuturesValuationMethod(1197) */
    [System.SerializableAttribute()]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fixprotocol.org/FIXML-5-0-SP1")]
    public enum FuturesValuationMethodEnum
    {
        [System.Xml.Serialization.XmlEnumAttribute("EQTY")]
        PremiumStyle,
        [System.Xml.Serialization.XmlEnumAttribute("FUT")]
        FuturesStyleMarkToMarket,
        [System.Xml.Serialization.XmlEnumAttribute("FUTDA")]
        FuturesStyleWithAnAttachedCashAdjustment,
    }
    #endregion FuturesValuationMethodEnum
    #region LastRptRequestedEnum
    /* LastRptRequested(912) */
    [System.SerializableAttribute()]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fixprotocol.org/FIXML-5-0-SP1")]
    public enum LastRptRequestedEnum
    {
        [System.Xml.Serialization.XmlEnumAttribute("N")]
        NotLastMessage,
        [System.Xml.Serialization.XmlEnumAttribute("Y")]
        LastMessage,
    }
    #endregion LastRptRequestedEnum
    #region ListMethodEnum
    /* ListMethod(1198) */
    [System.SerializableAttribute()]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fixprotocol.org/FIXML-5-0-SP1")]
    public enum ListMethodEnum
    {
        [System.Xml.Serialization.XmlEnumAttribute("0")]
        PreListedOnly,
        [System.Xml.Serialization.XmlEnumAttribute("1")]
        UserRequested,
    }
    #endregion ListMethodEnum
    #region LotTypeEnum
    /* LotType(1093) */
    [System.SerializableAttribute()]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fixprotocol.org/FIXML-5-0-SP1")]
    public enum LotTypeEnum
    {
        [System.Xml.Serialization.XmlEnumAttribute("1")]
        Odd,
        [System.Xml.Serialization.XmlEnumAttribute("2")]
        Round,
        [System.Xml.Serialization.XmlEnumAttribute("3")]
        Block,
    }
    #endregion LotTypeEnum
    #region MatchStatusEnum
    /* MatchStatus(573) */
    [System.SerializableAttribute()]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fixprotocol.org/FIXML-5-0-SP1")]
    public enum MatchStatusEnum
    {
        [System.Xml.Serialization.XmlEnumAttribute("0")]
        ComparedMatchedOrAffirmed,
        [System.Xml.Serialization.XmlEnumAttribute("1")]
        UncomparedUnmatchedOrUnaffired,
        [System.Xml.Serialization.XmlEnumAttribute("2")]
        AdvisoryOrAlert,
    }
    #endregion MatchStatusEnum
    #region MatchTypeEnum
    /* MatchType(574) */
    [System.SerializableAttribute()]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fixprotocol.org/FIXML-5-0-SP1")]
    public enum MatchTypeEnum
    {
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
        [System.Xml.Serialization.XmlEnumAttribute("M3")]
        ACTAcceptedTrade,
        [System.Xml.Serialization.XmlEnumAttribute("M4")]
        ACTDefaultTrade,
        [System.Xml.Serialization.XmlEnumAttribute("M5")]
        ACTDefaultAfterM2,
        [System.Xml.Serialization.XmlEnumAttribute("M6")]
        ACTM6Match,
        [System.Xml.Serialization.XmlEnumAttribute("AQ")]
        ComparedRecordsResultingFromStampedAdvisories,
        [System.Xml.Serialization.XmlEnumAttribute("M1")]
        ACTM1match,
        [System.Xml.Serialization.XmlEnumAttribute("M2")]
        ACTM2Match,
        [System.Xml.Serialization.XmlEnumAttribute("MT")]
        NonACT,
        [System.Xml.Serialization.XmlEnumAttribute("S1")]
        SummarizedMatchS1,
        [System.Xml.Serialization.XmlEnumAttribute("S2")]
        SummarizedMatchS2,
        [System.Xml.Serialization.XmlEnumAttribute("S3")]
        SummarizedMatchS3,
        [System.Xml.Serialization.XmlEnumAttribute("S4")]
        SummarizedMatchS4,
        [System.Xml.Serialization.XmlEnumAttribute("S5")]
        SummarizedMatchS5,
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
    }
    #endregion MatchTypeEnum
    #region MiscFeeTypeEnum
    /* MiscFeeType(139) */
    [System.SerializableAttribute()]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fixprotocol.org/FIXML-5-0-SP1")]
    public enum MiscFeeTypeEnum
    {
        [System.Xml.Serialization.XmlEnumAttribute("1")]
        Reg,
        [System.Xml.Serialization.XmlEnumAttribute("2")]
        Tax,
        [System.Xml.Serialization.XmlEnumAttribute("3")]
        LocalComm,
        [System.Xml.Serialization.XmlEnumAttribute("4")]
        ExchFee,
        [System.Xml.Serialization.XmlEnumAttribute("5")]
        Stamp,
        [System.Xml.Serialization.XmlEnumAttribute("6")]
        Levy,
        [System.Xml.Serialization.XmlEnumAttribute("7")]
        Other,
        [System.Xml.Serialization.XmlEnumAttribute("8")]
        Markup,
        [System.Xml.Serialization.XmlEnumAttribute("9")]
        ConsumptionTax,
        [System.Xml.Serialization.XmlEnumAttribute("10")]
        Transaction,
        [System.Xml.Serialization.XmlEnumAttribute("11")]
        Conversion,
        [System.Xml.Serialization.XmlEnumAttribute("12")]
        Agent,
        [System.Xml.Serialization.XmlEnumAttribute("13")]
        TransferFee,
        [System.Xml.Serialization.XmlEnumAttribute("14")]
        SecLending,
    }
    #endregion MiscFeeTypeEnum
    #region NetGrossIndEnum
    /* NetGrossInd(430) */
    [System.SerializableAttribute()]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fixprotocol.org/FIXML-5-0-SP1")]
    public enum NetGrossIndEnum
    {
        [System.Xml.Serialization.XmlEnumAttribute("1")]
        Net,
        [System.Xml.Serialization.XmlEnumAttribute("2")]
        Gross,
    }
    #endregion NetGrossIndEnum
    #region OddLotEnum
    /* OddLot(575) */
    [System.SerializableAttribute()]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fixprotocol.org/FIXML-5-0-SP1")]
    public enum OddLotEnum
    {
        [System.Xml.Serialization.XmlEnumAttribute("N")]
        Round,
        [System.Xml.Serialization.XmlEnumAttribute("Y")]
        Odd,
    }
    #endregion OddLotEnum
    #region OrderCategoryEnum
    /* OrderCategory(1115) */
    [System.SerializableAttribute()]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fixprotocol.org/FIXML-5-0-SP1")]
    public enum OrderCategoryEnum
    {
        [System.Xml.Serialization.XmlEnumAttribute("1")]
        Order,
        [System.Xml.Serialization.XmlEnumAttribute("2")]
        Quote,
        [System.Xml.Serialization.XmlEnumAttribute("3")]
        PrivatelyNegotiatedTrade,
        [System.Xml.Serialization.XmlEnumAttribute("4")]
        MultilegOrder,
        [System.Xml.Serialization.XmlEnumAttribute("5")]
        LinkedOrder,
        [System.Xml.Serialization.XmlEnumAttribute("6")]
        QuoteRequest,
        [System.Xml.Serialization.XmlEnumAttribute("7")]
        ImpliedOrder,
        [System.Xml.Serialization.XmlEnumAttribute("8")]
        CrossOrder,
        [System.Xml.Serialization.XmlEnumAttribute("9")]
        StreamingPrice,
    }
    #endregion OrderCategoryEnum
    #region OrderRestrictionsEnum
    /* OrderRestrictions(529) */
    [System.SerializableAttribute()]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fixprotocol.org/FIXML-5-0-SP1")]
    public enum OrderRestrictionsEnum
    {
        [System.Xml.Serialization.XmlEnumAttribute("1")]
        ProgramTrade,
        [System.Xml.Serialization.XmlEnumAttribute("2")]
        IndexArbitrage,
        [System.Xml.Serialization.XmlEnumAttribute("3")]
        NonIndexArbitrage,
        [System.Xml.Serialization.XmlEnumAttribute("4")]
        CompetingMarketMaker,
        [System.Xml.Serialization.XmlEnumAttribute("5")]
        ActMM,
        [System.Xml.Serialization.XmlEnumAttribute("6")]
        ActMMDeriv,
        [System.Xml.Serialization.XmlEnumAttribute("7")]
        ForeignEntity,
        [System.Xml.Serialization.XmlEnumAttribute("8")]
        ExMrktPart,
        [System.Xml.Serialization.XmlEnumAttribute("9")]
        ExIntMrktLink,
        [System.Xml.Serialization.XmlEnumAttribute("A")]
        RiskArb,
        [System.Xml.Serialization.XmlEnumAttribute("B")]
        IssuerHolding,
        [System.Xml.Serialization.XmlEnumAttribute("C")]
        IssuePriceStabilization,
        [System.Xml.Serialization.XmlEnumAttribute("D")]
        NonAlgorithmic,
        [System.Xml.Serialization.XmlEnumAttribute("E")]
        Algorithmic,
    }
    #endregion OrderRestrictionsEnum
    #region OrdTypeEnum
    /* OrdType(40) */
    [System.SerializableAttribute()]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fixprotocol.org/FIXML-5-0-SP1")]
    public enum OrdTypeEnum
    {
        [System.Xml.Serialization.XmlEnumAttribute("1")]
        Market,
        [System.Xml.Serialization.XmlEnumAttribute("2")]
        Limit,
        [System.Xml.Serialization.XmlEnumAttribute("3")]
        Stop,
        [System.Xml.Serialization.XmlEnumAttribute("4")]
        StopLimit,
        [System.Xml.Serialization.XmlEnumAttribute("5")]
        MarketOnClose,
        [System.Xml.Serialization.XmlEnumAttribute("6")]
        WithOrWithout,
        [System.Xml.Serialization.XmlEnumAttribute("7")]
        LimitOrBetter,
        [System.Xml.Serialization.XmlEnumAttribute("8")]
        LimitWithOrWithout,
        [System.Xml.Serialization.XmlEnumAttribute("9")]
        OnBasis,
        [System.Xml.Serialization.XmlEnumAttribute("A")]
        OnClose,
        [System.Xml.Serialization.XmlEnumAttribute("B")]
        LimitOnClose,
        [System.Xml.Serialization.XmlEnumAttribute("C")]
        ForexMarket,
        [System.Xml.Serialization.XmlEnumAttribute("D")]
        PreviouslyQuoted,
        [System.Xml.Serialization.XmlEnumAttribute("E")]
        PreviouslyIndicated,
        [System.Xml.Serialization.XmlEnumAttribute("F")]
        ForexLimit,
        [System.Xml.Serialization.XmlEnumAttribute("G")]
        ForexSwap,
        [System.Xml.Serialization.XmlEnumAttribute("H")]
        ForexPreviouslyQuoted,
        [System.Xml.Serialization.XmlEnumAttribute("I")]
        Funari,
        [System.Xml.Serialization.XmlEnumAttribute("J")]
        MarketIfTouched,
        [System.Xml.Serialization.XmlEnumAttribute("K")]
        MarketWithLeftOverLimit,
        [System.Xml.Serialization.XmlEnumAttribute("L")]
        PreviousFundValuationPoint,
        [System.Xml.Serialization.XmlEnumAttribute("M")]
        NextFundValuationPoint,
        [System.Xml.Serialization.XmlEnumAttribute("P")]
        Pegged,
        [System.Xml.Serialization.XmlEnumAttribute("Q")]
        CounterOrderSelection,
    }
    #endregion OrdTypeEnum
    #region PartyIDSourceEnum
    /* PartyIDSource(447) */
    [System.SerializableAttribute()]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fixprotocol.org/FIXML-5-0-SP1")]
    public enum PartyIDSourceEnum
    {
        [System.Xml.Serialization.XmlEnumAttribute("1")]
        KoreanInvestorID,
        [System.Xml.Serialization.XmlEnumAttribute("2")]
        TaiwaneseQualified,
        [System.Xml.Serialization.XmlEnumAttribute("3")]
        TaiwaneseTradingAcct,
        [System.Xml.Serialization.XmlEnumAttribute("4")]
        MCDnumber,
        [System.Xml.Serialization.XmlEnumAttribute("5")]
        ChineseBShare,
        [System.Xml.Serialization.XmlEnumAttribute("6")]
        UKNationalInsPenNumber,
        [System.Xml.Serialization.XmlEnumAttribute("7")]
        USSocialSecurity,
        [System.Xml.Serialization.XmlEnumAttribute("8")]
        USEmployerIDNumber,
        [System.Xml.Serialization.XmlEnumAttribute("9")]
        AustralianBusinessNumber,
        [System.Xml.Serialization.XmlEnumAttribute("A")]
        AustralianTaxFileNumber,
        [System.Xml.Serialization.XmlEnumAttribute("B")]
        BIC,
        [System.Xml.Serialization.XmlEnumAttribute("C")]
        AccptMarketPart,
        [System.Xml.Serialization.XmlEnumAttribute("D")]
        PropCode,
        [System.Xml.Serialization.XmlEnumAttribute("E")]
        ISOCode,
        [System.Xml.Serialization.XmlEnumAttribute("F")]
        SettlEntLoc,
        [System.Xml.Serialization.XmlEnumAttribute("G")]
        MIC,
        [System.Xml.Serialization.XmlEnumAttribute("H")]
        CSDPartCode,
        [System.Xml.Serialization.XmlEnumAttribute("I")]
        DirectedDefinedISITC,
    }
    #endregion PartyIDSourceEnum
    #region PartyRoleEnum
    /* PartyRole(452) */
    [System.SerializableAttribute()]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fixprotocol.org/FIXML-5-0-SP1")]
    public enum PartyRoleEnum
    {
        /// <summary>
        /// Executing Firm (formerly FIX 4.2 ExecBroker)
        /// </summary>
        [System.Xml.Serialization.XmlEnumAttribute("1")]
        ExecutingFirm,
        [System.Xml.Serialization.XmlEnumAttribute("2")]
        BrokerofCredit,
        /// <summary>
        /// Client ID (formerly FIX 4.2 ClientID)
        /// </summary>
        [System.Xml.Serialization.XmlEnumAttribute("3")]
        ClientID,
        /// <summary>
        /// Clearing Firm (formerly FIX 4.2 ClearingFirm)
        /// </summary>
        [System.Xml.Serialization.XmlEnumAttribute("4")]
        ClearingFirm,
        [System.Xml.Serialization.XmlEnumAttribute("5")]
        InvestorID,
        [System.Xml.Serialization.XmlEnumAttribute("6")]
        IntroducingFirm,
        /// <summary>
        /// Entering Firm
        /// </summary>
        [System.Xml.Serialization.XmlEnumAttribute("7")]
        EnteringFirm,
        [System.Xml.Serialization.XmlEnumAttribute("8")]
        LocateLendingFirm,
        [System.Xml.Serialization.XmlEnumAttribute("9")]
        FundManager,
        [System.Xml.Serialization.XmlEnumAttribute("10")]
        SettlementLocation,
        /// <summary>
        /// Order Origination Trader (associated with Order Origination Firm - i.e. trader who initiates/submits the order)
        /// </summary>
        [System.Xml.Serialization.XmlEnumAttribute("11")]
        InitiatingTrader,
        /// <summary>
        /// Executing Trader (associated with Executing Firm - actually executes)
        /// </summary>
        [System.Xml.Serialization.XmlEnumAttribute("12")]
        ExecutingTrader,
        [System.Xml.Serialization.XmlEnumAttribute("13")]
        OrderOriginator,
        /// <summary>
        /// Giveup Clearing Firm (firm to which trade is given up)
        /// </summary>
        [System.Xml.Serialization.XmlEnumAttribute("14")]
        GiveupClearingFirm,
        /// <summary>
        /// Correspondant Clearing Firm
        /// </summary>
        [System.Xml.Serialization.XmlEnumAttribute("15")]
        CorrespondantClearingFirm,
        /// <summary>
        /// Executing System
        /// </summary>
        [System.Xml.Serialization.XmlEnumAttribute("16")]
        ExecutingSystem,
        [System.Xml.Serialization.XmlEnumAttribute("17")]
        ContraFirm,
        [System.Xml.Serialization.XmlEnumAttribute("18")]
        ContraClearingFirm,
        [System.Xml.Serialization.XmlEnumAttribute("19")]
        SponsoringFirm,
        [System.Xml.Serialization.XmlEnumAttribute("20")]
        UndrContraFirm,
        /// <summary>
        /// code 21
        /// </summary>
        [System.Xml.Serialization.XmlEnumAttribute("21")]
        ClearingOrganization,
        [System.Xml.Serialization.XmlEnumAttribute("22")]
        Exchange,
        [System.Xml.Serialization.XmlEnumAttribute("24")]
        CustomerAccount,
        [System.Xml.Serialization.XmlEnumAttribute("25")]
        CorrespondentClearingOrganization,
        [System.Xml.Serialization.XmlEnumAttribute("26")]
        CorrespondentBroker,
        /// <summary>
        /// code 27
        /// </summary>
        [System.Xml.Serialization.XmlEnumAttribute("27")]
        BuyerSellerReceiverDeliverer,
        /// <summary>
        /// Code 28
        /// </summary>
        [System.Xml.Serialization.XmlEnumAttribute("28")]
        Custodian,
        [System.Xml.Serialization.XmlEnumAttribute("29")]
        Intermediary,
        [System.Xml.Serialization.XmlEnumAttribute("30")]
        Agent,
        [System.Xml.Serialization.XmlEnumAttribute("31")]
        SubCustodian,
        [System.Xml.Serialization.XmlEnumAttribute("32")]
        Beneficiary,
        [System.Xml.Serialization.XmlEnumAttribute("33")]
        InterestedParty,
        [System.Xml.Serialization.XmlEnumAttribute("34")]
        RegulatoryBody,
        [System.Xml.Serialization.XmlEnumAttribute("35")]
        LiquidityProvider,
        [System.Xml.Serialization.XmlEnumAttribute("36")]
        EnteringTrader,
        [System.Xml.Serialization.XmlEnumAttribute("37")]
        ContraTrader,
        [System.Xml.Serialization.XmlEnumAttribute("38")]
        PositionAccount,
        [System.Xml.Serialization.XmlEnumAttribute("39")]
        ContraInvestorID,
        [System.Xml.Serialization.XmlEnumAttribute("40")]
        TransferFirm,
        [System.Xml.Serialization.XmlEnumAttribute("41")]
        ContraPositionAccount,
        [System.Xml.Serialization.XmlEnumAttribute("42")]
        ContraExchange,
        [System.Xml.Serialization.XmlEnumAttribute("43")]
        InternalCarryAccount,
        [System.Xml.Serialization.XmlEnumAttribute("44")]
        OrderEntryOperatorID,
        [System.Xml.Serialization.XmlEnumAttribute("45")]
        SecondaryAccountNumber,
        [System.Xml.Serialization.XmlEnumAttribute("46")]
        ForeignFirm,
        [System.Xml.Serialization.XmlEnumAttribute("47")]
        ThirdPartyAllocationFirm,
        [System.Xml.Serialization.XmlEnumAttribute("48")]
        ClaimingAccount,
        [System.Xml.Serialization.XmlEnumAttribute("49")]
        AssetManager,
        [System.Xml.Serialization.XmlEnumAttribute("50")]
        PledgorAccount,
        [System.Xml.Serialization.XmlEnumAttribute("51")]
        PledgeeAccount,
        [System.Xml.Serialization.XmlEnumAttribute("52")]
        LargeTraderReportableAccount,
        [System.Xml.Serialization.XmlEnumAttribute("53")]
        TraderMnemonic,
        [System.Xml.Serialization.XmlEnumAttribute("54")]
        SenderLocation,
        [System.Xml.Serialization.XmlEnumAttribute("55")]
        SessionID,
        [System.Xml.Serialization.XmlEnumAttribute("56")]
        AcceptableCounterparty,
        [System.Xml.Serialization.XmlEnumAttribute("57")]
        UnacceptableCounterparty,
        [System.Xml.Serialization.XmlEnumAttribute("58")]
        EnteringUnit,
        [System.Xml.Serialization.XmlEnumAttribute("59")]
        ExecutingUnit,
        [System.Xml.Serialization.XmlEnumAttribute("60")]
        IntroducingBroker,
        [System.Xml.Serialization.XmlEnumAttribute("61")]
        QuoteOriginator,
        [System.Xml.Serialization.XmlEnumAttribute("62")]
        ReportOriginator,
        [System.Xml.Serialization.XmlEnumAttribute("63")]
        SystematicInternaliser,
        [System.Xml.Serialization.XmlEnumAttribute("64")]
        MultilateralTradingFacility,
        [System.Xml.Serialization.XmlEnumAttribute("65")]
        RegulatedMarket,
        [System.Xml.Serialization.XmlEnumAttribute("66")]
        MarketMaker,
        [System.Xml.Serialization.XmlEnumAttribute("67")]
        InvestmentFirm,
        [System.Xml.Serialization.XmlEnumAttribute("68")]
        HostCompetentAuthority,
        [System.Xml.Serialization.XmlEnumAttribute("69")]
        HomeCompetentAuthority,
        [System.Xml.Serialization.XmlEnumAttribute("70")]
        CompetentAuthorityLiquidity,
        [System.Xml.Serialization.XmlEnumAttribute("71")]
        CompetentAuthorityTransactionVenue,
        [System.Xml.Serialization.XmlEnumAttribute("72")]
        ReportingIntermediary,
        [System.Xml.Serialization.XmlEnumAttribute("73")]
        ExecutionVenue,
        [System.Xml.Serialization.XmlEnumAttribute("74")]
        MarketDataEntryOriginator,
        [System.Xml.Serialization.XmlEnumAttribute("75")]
        LocationID,
        [System.Xml.Serialization.XmlEnumAttribute("76")]
        DeskID,
        [System.Xml.Serialization.XmlEnumAttribute("77")]
        MarketDataMarket,
        [System.Xml.Serialization.XmlEnumAttribute("78")]
        AllocationEntity,
        [System.Xml.Serialization.XmlEnumAttribute("79")]
        PrimeBroker,
        [System.Xml.Serialization.XmlEnumAttribute("80")]
        StepOutFirm,
        [System.Xml.Serialization.XmlEnumAttribute("81")]
        BrokerClearingID,
    }
    #endregion PartyRoleEnum
    #region PartySubIDTypeEnum
    /* PartySubIDType(803) */
    [System.SerializableAttribute()]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fixprotocol.org/FIXML-5-0-SP1")]
    public enum PartySubIDTypeEnum
    {
        [System.Xml.Serialization.XmlEnumAttribute("1")]
        Firm,
        [System.Xml.Serialization.XmlEnumAttribute("2")]
        Person,
        [System.Xml.Serialization.XmlEnumAttribute("3")]
        System,
        [System.Xml.Serialization.XmlEnumAttribute("4")]
        Application,
        [System.Xml.Serialization.XmlEnumAttribute("5")]
        FullLegalNameOfFirm,
        [System.Xml.Serialization.XmlEnumAttribute("6")]
        PostalAddress,
        [System.Xml.Serialization.XmlEnumAttribute("7")]
        PhoneNumber,
        [System.Xml.Serialization.XmlEnumAttribute("8")]
        EmailAddress,
        [System.Xml.Serialization.XmlEnumAttribute("9")]
        ContactName,
        [System.Xml.Serialization.XmlEnumAttribute("10")]
        SecurityAccountNumber,
        [System.Xml.Serialization.XmlEnumAttribute("11")]
        RegistrationNumber,
        [System.Xml.Serialization.XmlEnumAttribute("12")]
        ConfirmationRegisteredAddress,
        [System.Xml.Serialization.XmlEnumAttribute("13")]
        ConfirmationRegulatoryStatus,
        [System.Xml.Serialization.XmlEnumAttribute("14")]
        RegistrationName,
        [System.Xml.Serialization.XmlEnumAttribute("15")]
        CashAccountNumber,
        [System.Xml.Serialization.XmlEnumAttribute("16")]
        BIC,
        [System.Xml.Serialization.XmlEnumAttribute("17")]
        CSD,
        [System.Xml.Serialization.XmlEnumAttribute("18")]
        RegisteredAddress,
        [System.Xml.Serialization.XmlEnumAttribute("19")]
        FundAccountName,
        [System.Xml.Serialization.XmlEnumAttribute("20")]
        TelexNumber,
        [System.Xml.Serialization.XmlEnumAttribute("21")]
        FaxNumber,
        [System.Xml.Serialization.XmlEnumAttribute("22")]
        SecurityAccountName,
        [System.Xml.Serialization.XmlEnumAttribute("23")]
        CashAccountName,
        [System.Xml.Serialization.XmlEnumAttribute("24")]
        Department,
        [System.Xml.Serialization.XmlEnumAttribute("25")]
        LocationDesk,
        [System.Xml.Serialization.XmlEnumAttribute("26")]
        PositionAccountType,
        [System.Xml.Serialization.XmlEnumAttribute("27")]
        SecurityLocateID,
        [System.Xml.Serialization.XmlEnumAttribute("28")]
        MarketMaker,
        [System.Xml.Serialization.XmlEnumAttribute("29")]
        EligibleCounterparty,
        [System.Xml.Serialization.XmlEnumAttribute("30")]
        ProfessionalClient,
        [System.Xml.Serialization.XmlEnumAttribute("31")]
        Location,
        [System.Xml.Serialization.XmlEnumAttribute("32")]
        ExecutionVenue,
        [System.Xml.Serialization.XmlEnumAttribute("33")]
        CurrencyDeliveryIdentifier,
        [System.Xml.Serialization.XmlEnumAttribute("4000")]
        ReservedAndAvailableForBilaterallyAgreedUponUserDefinedValues,
    }
    #endregion PartySubIDTypeEnum
    #region PosAmtTypeEnum
    /* PosAmtType(707) */
    [System.SerializableAttribute()]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fixprotocol.org/FIXML-5-0-SP1")]
    public enum PosAmtTypeEnum
    {
        [System.Xml.Serialization.XmlEnumAttribute("CASH")]
        CashAmountCorporateEvent,
        [System.Xml.Serialization.XmlEnumAttribute("CRES")]
        CashResidualAmount,
        [System.Xml.Serialization.XmlEnumAttribute("FMTM")]
        FinalMarkToMarketAmount,
        [System.Xml.Serialization.XmlEnumAttribute("IMTM")]
        IncrementalMarkToMarketAmount,
        [System.Xml.Serialization.XmlEnumAttribute("PREM")]
        PremiumAmount,
        [System.Xml.Serialization.XmlEnumAttribute("SMTM")]
        StartOfDayMarkToMarketAmount,
        [System.Xml.Serialization.XmlEnumAttribute("TVAR")]
        TradeVariationAmount,
        [System.Xml.Serialization.XmlEnumAttribute("VADJ")]
        ValueAdjustedAmount,
        [System.Xml.Serialization.XmlEnumAttribute("SETL")]
        SettlementValue,
    }
    #endregion PosAmtTypeEnum
    #region PositionEffectEnum
    /* PositionEffect(77) */
    [System.SerializableAttribute()]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fixprotocol.org/FIXML-5-0-SP1")]
    public enum PositionEffectEnum
    {
        [System.Xml.Serialization.XmlEnumAttribute("C")]
        Close,
        [System.Xml.Serialization.XmlEnumAttribute("D")]
        Default,
        [System.Xml.Serialization.XmlEnumAttribute("F")]
        FIFO,
        [System.Xml.Serialization.XmlEnumAttribute("N")]
        CloseButNotifyOnOpen,
        [System.Xml.Serialization.XmlEnumAttribute("O")]
        Open,
        [System.Xml.Serialization.XmlEnumAttribute("R")]
        Rolled,
        //PL 20130318 Additional values
        [System.Xml.Serialization.XmlEnumAttribute("H")]
        HILO,
        [System.Xml.Serialization.XmlEnumAttribute("I")]
        FIFO_ITD,
        [System.Xml.Serialization.XmlEnumAttribute("L")]
        LIFO,
        [System.Xml.Serialization.XmlEnumAttribute("W")]
        WACP,
    }
    #endregion PositionEffectEnum
    #region PreviouslyReportedEnum
    /* PreviouslyReported(570) */
    [System.SerializableAttribute()]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fixprotocol.org/FIXML-5-0-SP1")]
    public enum PreviouslyReportedEnum
    {
        [System.Xml.Serialization.XmlEnumAttribute("N")]
        NotReported,
        [System.Xml.Serialization.XmlEnumAttribute("Y")]
        PerviouslyReported,
    }
    #endregion PreviouslyReportedEnum
    #region PriceQuoteMethodEnum
    /* PriceQuoteMethod(1196) */
    [System.SerializableAttribute()]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fixprotocol.org/FIXML-5-0-SP1")]
    public enum PriceQuoteMethodEnum
    {
        [System.Xml.Serialization.XmlEnumAttribute("STD")]
        Standard,
        [System.Xml.Serialization.XmlEnumAttribute("INX")]
        Index,
        [System.Xml.Serialization.XmlEnumAttribute("INT")]
        InterestRateIndex,
        // PM 20130801 [18876] Added for Contract with Variable tick Value
        [System.Xml.Serialization.XmlEnumAttribute("ASX_IR")]
        ASX90DayBankBill,
        [System.Xml.Serialization.XmlEnumAttribute("ASX_YT")]
        ASX3YearTreasuryBond,
        [System.Xml.Serialization.XmlEnumAttribute("ASX_XT")]
        ASX10YearTreasuryBond,
        [System.Xml.Serialization.XmlEnumAttribute("ASX_YS")]
        ASX3YearInterestRateSwap,
        [System.Xml.Serialization.XmlEnumAttribute("ASX_XS")]
        ASX10YearInterestRateSwap,
        // PM 20181016 [24261] Add OMX Swedish Bonds with Variable tick Value
        [System.Xml.Serialization.XmlEnumAttribute("XSTO_SGB2Y")]
        Swedish2YearGovernmentBond,
        [System.Xml.Serialization.XmlEnumAttribute("XSTO_SGB5Y")]
        Swedish5YearGovernmentBond,
        [System.Xml.Serialization.XmlEnumAttribute("XSTO_SGB10Y")]
        Swedish10YearGovernmentBond,
    }
    #endregion PriceQuoteMethodEnum
    #region PriceTypeEnum
    /* PriceType(423) */
    [System.SerializableAttribute()]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fixprotocol.org/FIXML-5-0-SP1")]
    public enum PriceTypeEnum
    {
        [System.Xml.Serialization.XmlEnumAttribute("1")]
        Percentage,
        [System.Xml.Serialization.XmlEnumAttribute("2")]
        PerUnit,
        [System.Xml.Serialization.XmlEnumAttribute("3")]
        FixedAmount,
        [System.Xml.Serialization.XmlEnumAttribute("4")]
        Discount,
        [System.Xml.Serialization.XmlEnumAttribute("5")]
        Premium,
        [System.Xml.Serialization.XmlEnumAttribute("6")]
        Spread,
        [System.Xml.Serialization.XmlEnumAttribute("7")]
        TEDPrice,
        [System.Xml.Serialization.XmlEnumAttribute("8")]
        TEDYield,
        [System.Xml.Serialization.XmlEnumAttribute("9")]
        Yield,
        [System.Xml.Serialization.XmlEnumAttribute("10")]
        FixedCabinetTradePrice,
        [System.Xml.Serialization.XmlEnumAttribute("11")]
        VariableCabinetTradePrice,
        [System.Xml.Serialization.XmlEnumAttribute("13")]
        ProductTicksInHalfs,
        [System.Xml.Serialization.XmlEnumAttribute("14")]
        ProductTicksInFourths,
        [System.Xml.Serialization.XmlEnumAttribute("15")]
        ProductTicksInEights,
        [System.Xml.Serialization.XmlEnumAttribute("16")]
        ProductTicksInSixteenths,
        [System.Xml.Serialization.XmlEnumAttribute("17")]
        ProductTicksInThirtySeconds,
        [System.Xml.Serialization.XmlEnumAttribute("18")]
        ProductTicksInSixtyForths,
        [System.Xml.Serialization.XmlEnumAttribute("19")]
        ProductTicksInOneTwentyEights,
    }
    #endregion PriceTypeEnum
    #region ProcessCodeEnum
    /* ProcessCode(81) */
    [System.SerializableAttribute()]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fixprotocol.org/FIXML-5-0-SP1")]
    public enum ProcessCodeEnum
    {
        [System.Xml.Serialization.XmlEnumAttribute("0")]
        Regular,
        [System.Xml.Serialization.XmlEnumAttribute("1")]
        SoftDollar,
        [System.Xml.Serialization.XmlEnumAttribute("2")]
        StepIn,
        [System.Xml.Serialization.XmlEnumAttribute("3")]
        StepOut,
        [System.Xml.Serialization.XmlEnumAttribute("4")]
        SoftDollarStepIn,
        [System.Xml.Serialization.XmlEnumAttribute("5")]
        SoftDollarStepOut,
        [System.Xml.Serialization.XmlEnumAttribute("6")]
        PlanSponsor,
    }
    #endregion ProcessCodeEnum
    #region PublishTrdIndicatorEnum
    /* PublishTrdIndicator(852) */
    [System.SerializableAttribute()]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fixprotocol.org/FIXML-5-0-SP1")]
    public enum PublishTrdIndicatorEnum
    {
        [System.Xml.Serialization.XmlEnumAttribute("N")]
        DoNotReportTrade,
        [System.Xml.Serialization.XmlEnumAttribute("Y")]
        ReportTrade,
    }
    #endregion PublishTrdIndicatorEnum
    #region QtyTypeEnum
    /* QtyType(854) */
    [System.SerializableAttribute()]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fixprotocol.org/FIXML-5-0-SP1")]
    public enum QtyTypeEnum
    {
        [System.Xml.Serialization.XmlEnumAttribute("0")]
        Units,
        [System.Xml.Serialization.XmlEnumAttribute("1")]
        Contracts,
        [System.Xml.Serialization.XmlEnumAttribute("2")]
        UnitsOfMeasurePerTimeUnit,
    }
    #endregion QtyTypeEnum
    #region RelatedPositionIDSourceEnum
    // PM 20160428 [22107] Ajout à partir des Extension Pack: FIX.5.0SP2 EP142 & EP199
    [System.SerializableAttribute()]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fixprotocol.org/FIXML-5-0-SP1")]
    public enum RelatedPositionIDSourceEnum
    {

        /// <remarks/>
        [System.Xml.Serialization.XmlEnumAttribute("1")]
        PosMaintRptID,

        /// <remarks/>
        [System.Xml.Serialization.XmlEnumAttribute("2")]
        TransferID,

        /// <remarks/>
        [System.Xml.Serialization.XmlEnumAttribute("3")]
        PositionID,
    }
    #endregion RelatedPositionIDSourceEnum

    /// <summary>
    /// Identifies the reporting entity that originated the value in RegulatoryTradeID (1903). 
    /// The reporting entity identifier may be assigned by a regulator.
    /// 
    /// Ajout  partir de l'Extension Pack: FIX.5.0SP2 EP275
    /// </summary>
    // EG 20240227 [WI855] Trade input : New data TVTIC (Trading Venue Transaction Identification Code)
    [System.SerializableAttribute()]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fixprotocol.org/FIXML-5-0-SP1")]
    public enum RegulatoryTradeIDSourceEnum
    {
        [System.Xml.Serialization.XmlEnumAttribute("1")]
        UniqueTransactionIdentifier,
    }
    /// <summary>
    /// Identifies the event which caused origination of the identifier in RegulatoryTradeID (1903)
    /// 
    /// Ajout  partir de l'Extension Pack: FIX.5.0SP2 EP275
    /// </summary>
    // EG 20240227 [WI855] Trade input : New data TVTIC (Trading Venue Transaction Identification Code)
    [System.SerializableAttribute()]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fixprotocol.org/FIXML-5-0-SP1")]
    public enum RegulatoryTradeIDEventEnum
    {
        [System.Xml.Serialization.XmlEnumAttribute("0")]
        InitialBlockTrade,
        [System.Xml.Serialization.XmlEnumAttribute("1")]
        Allocation,
        [System.Xml.Serialization.XmlEnumAttribute("2")]
        Clearing,
        [System.Xml.Serialization.XmlEnumAttribute("3")]
        Compression,
        [System.Xml.Serialization.XmlEnumAttribute("4")]
        Novation,
        [System.Xml.Serialization.XmlEnumAttribute("5")]
        Termination,
        [System.Xml.Serialization.XmlEnumAttribute("6")]
        PostTradeValuation,
    }
    /// <summary>
    /// Specifies the type of trade identifier provided in RegulatoryTradeID (1903).
    /// Contextual hierarchy of events for the same trade or transaction maybe captured 
    /// through use of the different RegulatoryTradeIDType (1906) values using multiple 
    /// instances of the repeating group as needed for regulatory reporting.
    /// 
    /// Ajout  partir de l'Extension Pack: FIX.5.0SP2 EP275
    /// </summary>
    // EG 20240227 [WI855] Trade input : New data TVTIC (Trading Venue Transaction Identification Code)
    [System.SerializableAttribute()]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fixprotocol.org/FIXML-5-0-SP1")]
    public enum RegulatoryTradeIDTypeEnum
    {
        [System.Xml.Serialization.XmlEnumAttribute("0")]
        Current,
        [System.Xml.Serialization.XmlEnumAttribute("1")]
        Previous,
        [System.Xml.Serialization.XmlEnumAttribute("2")]
        Block,
        [System.Xml.Serialization.XmlEnumAttribute("3")]
        Related,
        [System.Xml.Serialization.XmlEnumAttribute("4")]
        ClearedBlockTrade,
        [System.Xml.Serialization.XmlEnumAttribute("5")]
        TradingVenueTransactionIdentifier,
    }
    /// <summary>
    /// Specifies the scope to which the RegulatoryTradeID(1903) applies. 
    /// Used when a trade must be assigned more than one identifier,
    /// e.g. one for the clearing member and another for the client on a cleared 
    /// trade as with the principal model in Europe.
    /// 
    /// Ajout  partir de l'Extension Pack: FIX.5.0SP2 EP275
    /// </summary>
    // EG 20240227 [WI855] Trade input : New data TVTIC (Trading Venue Transaction Identification Code)
    [System.SerializableAttribute()]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fixprotocol.org/FIXML-5-0-SP1")]
    public enum RegulatoryTradeIDScopeEnum
    {
        [System.Xml.Serialization.XmlEnumAttribute("1")]
        ClearingMember,
        [System.Xml.Serialization.XmlEnumAttribute("2")]
        Client,
    }
    #region ResponseTransportTypeEnum
    /* ResponseTransportType(725) */
    [System.SerializableAttribute()]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fixprotocol.org/FIXML-5-0-SP1")]
    public enum ResponseTransportTypeEnum
    {
        [System.Xml.Serialization.XmlEnumAttribute("0")]
        Inband,
        [System.Xml.Serialization.XmlEnumAttribute("1")]
        OutOfBand,
    }
    #endregion ResponseTransportTypeEnum
    #region SecondaryTrdTypeEnum
    /// <summary>
    /// SecondaryTrdType(855) 
    /// </summary>
    /// FI 20140505 [19851] add volaTrade, EFPFinTrade, EFPIndexFuturesTrade  
    [System.SerializableAttribute()]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fixprotocol.org/FIXML-5-0-SP1")]
    public enum SecondaryTrdTypeEnum
    {
        [System.Xml.Serialization.XmlEnumAttribute("0")]
        RegularTrade,
        [System.Xml.Serialization.XmlEnumAttribute("1")]
        BlockTrade,
        // 20120531 MF Ticket 17769
        /// <summary>
        /// Exchange for Physical, available on the Spheres ENUMs 'SecondaryTrdTypeEnum', 'TrdTypeEnum'
        /// </summary>
        [System.Xml.Serialization.XmlEnumAttribute("2")]
        EFP,
        [System.Xml.Serialization.XmlEnumAttribute("3")]
        Transfer,
        [System.Xml.Serialization.XmlEnumAttribute("4")]
        LateTrade,
        [System.Xml.Serialization.XmlEnumAttribute("5")]
        T_Trade,
        [System.Xml.Serialization.XmlEnumAttribute("6")]
        WeightedAveragePriceTrade,
        [System.Xml.Serialization.XmlEnumAttribute("7")]
        BunchedTrade,
        [System.Xml.Serialization.XmlEnumAttribute("8")]
        LateBunchedTrade,
        [System.Xml.Serialization.XmlEnumAttribute("9")]
        PriorReferencePriceTrade,
        [System.Xml.Serialization.XmlEnumAttribute("10")]
        AfterHoursTrade,
        [System.Xml.Serialization.XmlEnumAttribute("11")]
        ExchangeForRisk,
        [System.Xml.Serialization.XmlEnumAttribute("12")]
        ExchangeForSwap,
        [System.Xml.Serialization.XmlEnumAttribute("13")]
        ExchangeOfFuturesForFuturesInMarket,
        [System.Xml.Serialization.XmlEnumAttribute("14")]
        ExchangeOfOptionsForOptions,
        [System.Xml.Serialization.XmlEnumAttribute("15")]
        TradingAtSettlement,
        [System.Xml.Serialization.XmlEnumAttribute("16")]
        AllOrNone,
        [System.Xml.Serialization.XmlEnumAttribute("17")]
        FuturesLargeOrderExecution,
        [System.Xml.Serialization.XmlEnumAttribute("18")]
        ExchangeOfFuturesForFuturesExternalMarket,
        [System.Xml.Serialization.XmlEnumAttribute("19")]
        OptionInterimTrade,
        [System.Xml.Serialization.XmlEnumAttribute("20")]
        OptionCabinetTrade,
        [System.Xml.Serialization.XmlEnumAttribute("22")]
        PrivatelyNegotiatedTrades,
        [System.Xml.Serialization.XmlEnumAttribute("23")]
        SubstitutionOfFuturesForForwards,
        [System.Xml.Serialization.XmlEnumAttribute("24")]
        ErrorTrade,
        [System.Xml.Serialization.XmlEnumAttribute("25")]
        SpecialCumDividend,
        [System.Xml.Serialization.XmlEnumAttribute("26")]
        SpecialExDividend,
        [System.Xml.Serialization.XmlEnumAttribute("27")]
        SpecialCumCoupon,
        [System.Xml.Serialization.XmlEnumAttribute("28")]
        SpecialExCoupon,
        [System.Xml.Serialization.XmlEnumAttribute("29")]
        CashSettlement,
        [System.Xml.Serialization.XmlEnumAttribute("30")]
        SpecialPrice,
        [System.Xml.Serialization.XmlEnumAttribute("31")]
        GuaranteedDelivery,
        [System.Xml.Serialization.XmlEnumAttribute("32")]
        SpecialCumRights,
        [System.Xml.Serialization.XmlEnumAttribute("33")]
        SpecialExRights,
        [System.Xml.Serialization.XmlEnumAttribute("34")]
        SpecialCumCapitalRepayments,
        [System.Xml.Serialization.XmlEnumAttribute("35")]
        SpecialExCapitalRepayments,
        [System.Xml.Serialization.XmlEnumAttribute("36")]
        SpecialCumBonus,
        [System.Xml.Serialization.XmlEnumAttribute("37")]
        SpecialExBonus,
        [System.Xml.Serialization.XmlEnumAttribute("38")]
        BlockTradeSameAsLargeTrade,
        [System.Xml.Serialization.XmlEnumAttribute("39")]
        WorkedPrincipalTrade,
        [System.Xml.Serialization.XmlEnumAttribute("40")]
        BlockTradesAfterMarket,
        [System.Xml.Serialization.XmlEnumAttribute("41")]
        NameChange,
        [System.Xml.Serialization.XmlEnumAttribute("42")]
        PortfolioTransfer,
        [System.Xml.Serialization.XmlEnumAttribute("43")]
        ProrogationBuy,
        [System.Xml.Serialization.XmlEnumAttribute("44")]
        ProrogationSell,
        [System.Xml.Serialization.XmlEnumAttribute("45")]
        OptionExercise,
        [System.Xml.Serialization.XmlEnumAttribute("46")]
        DeltaNeutralTransaction,
        [System.Xml.Serialization.XmlEnumAttribute("47")]
        FinancingTransaction,
        [System.Xml.Serialization.XmlEnumAttribute("48")]
        NonStandardSettlement,
        [System.Xml.Serialization.XmlEnumAttribute("49")]
        DerivativeRelatedTransaction,
        [System.Xml.Serialization.XmlEnumAttribute("50")]
        PortfolioTrade,
        [System.Xml.Serialization.XmlEnumAttribute("51")]
        VolumeWeightedAverageTrade,
        [System.Xml.Serialization.XmlEnumAttribute("52")]
        ExchangeGrantedTrade,
        [System.Xml.Serialization.XmlEnumAttribute("53")]
        RepurchaseAgreement,
        [System.Xml.Serialization.XmlEnumAttribute("54")]
        OTC,
        [System.Xml.Serialization.XmlEnumAttribute("55")]
        ExchangeBasisFacility,

        /*
        FI 20140507 [XXXXX] Mise en commentaire puisque SecondaryTrdTypeEnum n'accepte pas de valeur étendu
                            Les valeurs étendue ont été conservées dans l'enum dans la v4.0 pour éviter toute dégradation éventuelle
         
        // 20120711 MF Ticket 18006 
        /// <summary>
        ///  Opening position trade
        /// </summary>
        [System.Xml.Serialization.XmlEnumAttribute("1000")]
        PositionOpening,
        /// <summary>
        /// Position comming from a cascading of position
        /// </summary>
        //  PM 20130216 [18414]
        [System.Xml.Serialization.XmlEnumAttribute("1001")]
        Cascading,
        /// <summary>
        /// Position comming from a shifting of position
        /// </summary>
        //  PM 20130216 [18414]
        [System.Xml.Serialization.XmlEnumAttribute("1002")]
        Shifting,
        // EG 20130607
        [System.Xml.Serialization.XmlEnumAttribute("1003")]
        CorporateAction,
        [System.Xml.Serialization.XmlEnumAttribute("1004")]
        MergedTrade,
        [System.Xml.Serialization.XmlEnumAttribute("1005")]
        SplitTrade,
        /// <summary>
        /// Vola Trade
        /// <para>Eurex Trade Type</para>
        /// </summary>
        /// FI 20140505 [19851]
        [System.Xml.Serialization.XmlEnumAttribute("1100")]
        VolaTrade,
        /// <summary>
        /// Exchange for physical
        /// <para>Eurex Trade Type</para>
        /// </summary>
        /// FI 20140505 [19851]
        [System.Xml.Serialization.XmlEnumAttribute("1101")]
        EFPFinTrade,
        /// <summary>
        /// Exchange for physical
        /// <para>Eurex Trade Type</para>
        /// </summary>
        /// FI 20140505 [19851]
        [System.Xml.Serialization.XmlEnumAttribute("1102")]
        EFPIndexFuturesTrade
         */
    }
    #endregion SecondaryTrdTypeEnum
    #region SecurityIDSourceEnum
    /// <summary>
    /// Identifies class or source of the SecurityID (48) value. Required if SecurityID is specified.
    /// 100+ are reserved for private security identifications
    /// </summary>
    /* SecurityIDSource(22) */
    [System.SerializableAttribute()]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fixprotocol.org/FIXML-5-0-SP1")]
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
        RICCode,
        [System.Xml.Serialization.XmlEnumAttribute("6")]
        ISOCurrencyCode,
        [System.Xml.Serialization.XmlEnumAttribute("7")]
        ISOCountryCode,
        [System.Xml.Serialization.XmlEnumAttribute("8")]
        ExchangeSymbol,
        [System.Xml.Serialization.XmlEnumAttribute("9")]
        ConsolidatedTapeAssociationSymbol,
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
        Common,
        [System.Xml.Serialization.XmlEnumAttribute("H")]
        ClearingOrganization,
        [System.Xml.Serialization.XmlEnumAttribute("I")]
        ISDAFpMLProductSpecification,
        [System.Xml.Serialization.XmlEnumAttribute("J")]
        OptionPriceReportingAuthority,
        [System.Xml.Serialization.XmlEnumAttribute("K")]
        ISDAFpMLProductURL,
        [System.Xml.Serialization.XmlEnumAttribute("L")]
        LetterOfCredit,
        [System.Xml.Serialization.XmlEnumAttribute("M")]
        MarketplaceAssignedIdentifier,
        [System.Xml.Serialization.XmlEnumAttribute("100")]
        Proprietary,
        /// <summary>
        /// Eurex Instrument Identifier (T7)
        /// </summary>
        /// FI 20220201 [25699] Add
        [System.Xml.Serialization.XmlEnumAttribute("1100")]
        EurexInstrmtID,
        /// <summary>
        /// Eurex Contract Identifier
        /// </summary>
        /// FI 20220201 [25699] Add
        [System.Xml.Serialization.XmlEnumAttribute("1101")]
        EurexCntrctID,
    }


    #endregion SecurityIDSourceEnum
    #region SecurityStatusEnum
    /* SecurityStatus(965) */
    [System.SerializableAttribute()]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fixprotocol.org/FIXML-5-0-SP1")]
    public enum SecurityStatusEnum
    {
        [System.Xml.Serialization.XmlEnumAttribute("1")]
        Active,
        [System.Xml.Serialization.XmlEnumAttribute("2")]
        Inactive,
    }
    #endregion SecurityStatusEnum
    #region SecurityTypeEnum
    /* SecurityType(167) */
    [System.SerializableAttribute()]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fixprotocol.org/FIXML-5-0-SP1")]
    public enum SecurityTypeEnum
    {
        [System.Xml.Serialization.XmlEnumAttribute("ABS")]
        AssetbackedSecurities,
        [System.Xml.Serialization.XmlEnumAttribute("AMENDED")]
        AmendedRestated,
        [System.Xml.Serialization.XmlEnumAttribute("AN")]
        OtherAnticipationNotesBANGANEtc,
        [System.Xml.Serialization.XmlEnumAttribute("BA")]
        BankersAcceptance,
        [System.Xml.Serialization.XmlEnumAttribute("BDN")]
        BankDepositoryNote,
        [System.Xml.Serialization.XmlEnumAttribute("BN")]
        BankNotes,
        [System.Xml.Serialization.XmlEnumAttribute("BOX")]
        BillOfExchanges,
        [System.Xml.Serialization.XmlEnumAttribute("BRADY")]
        BradyBond,
        [System.Xml.Serialization.XmlEnumAttribute("BRIDGE")]
        BridgeLoan,
        [System.Xml.Serialization.XmlEnumAttribute("BUYSELL")]
        BuySellback,
        [System.Xml.Serialization.XmlEnumAttribute("CAMM")]
        CanadianMoneyMarkets,
        [System.Xml.Serialization.XmlEnumAttribute("CAN")]
        CanadianTreasuryNotes,
        [System.Xml.Serialization.XmlEnumAttribute("CASH")]
        Cash,
        [System.Xml.Serialization.XmlEnumAttribute("CB")]
        ConvertibleBond,
        [System.Xml.Serialization.XmlEnumAttribute("CD")]
        CertificateOfDeposit,
        [System.Xml.Serialization.XmlEnumAttribute("CDS")]
        CreditDefaultSwap,
        [System.Xml.Serialization.XmlEnumAttribute("CL")]
        CallLoans,
        [System.Xml.Serialization.XmlEnumAttribute("CMB")]
        CanadianMortgageBonds,
        [System.Xml.Serialization.XmlEnumAttribute("CMBS")]
        CorpMortgagebackedSecurities,
        [System.Xml.Serialization.XmlEnumAttribute("CMO")]
        CollateralizedMortgageObligation,
        [System.Xml.Serialization.XmlEnumAttribute("COFO")]
        CertificateOfObligation,
        [System.Xml.Serialization.XmlEnumAttribute("COFP")]
        CertificateOfParticipation,
        [System.Xml.Serialization.XmlEnumAttribute("CORP")]
        CorporateBond,
        [System.Xml.Serialization.XmlEnumAttribute("CP")]
        CommercialPaper,
        [System.Xml.Serialization.XmlEnumAttribute("CPP")]
        CorporatePrivatePlacement,
        [System.Xml.Serialization.XmlEnumAttribute("CS")]
        CommonStock,
        [System.Xml.Serialization.XmlEnumAttribute("CTB")]
        CanadianTreasuryBills,
        [System.Xml.Serialization.XmlEnumAttribute("DEFLTED")]
        Defaulted,
        [System.Xml.Serialization.XmlEnumAttribute("DINP")]
        DebtorInPossession,
        [System.Xml.Serialization.XmlEnumAttribute("DN")]
        DepositNotes,
        [System.Xml.Serialization.XmlEnumAttribute("DUAL")]
        DualCurrency,
        [System.Xml.Serialization.XmlEnumAttribute("EUCD")]
        EuroCertificateOfDeposit,
        [System.Xml.Serialization.XmlEnumAttribute("EUCORP")]
        EuroCorporateBond,
        [System.Xml.Serialization.XmlEnumAttribute("EUCP")]
        EuroCommercialPaper,
        [System.Xml.Serialization.XmlEnumAttribute("EUFRN")]
        EuroCorporateFloatingRateNotes,
        [System.Xml.Serialization.XmlEnumAttribute("EUSOV")]
        EuroSovereigns,
        [System.Xml.Serialization.XmlEnumAttribute("EUSUPRA")]
        EuroSupranationalCoupons,
        [System.Xml.Serialization.XmlEnumAttribute("FAC")]
        FederalAgencyCoupon,
        [System.Xml.Serialization.XmlEnumAttribute("FADN")]
        FederalAgencyDiscountNote,
        [System.Xml.Serialization.XmlEnumAttribute("FOR")]
        ForeignExchangeContract,
        [System.Xml.Serialization.XmlEnumAttribute("FORWARD")]
        Forward,
        [System.Xml.Serialization.XmlEnumAttribute("FRN")]
        USCorporateFloatingRateNotes,
        [System.Xml.Serialization.XmlEnumAttribute("FUT")]
        Future,
        [System.Xml.Serialization.XmlEnumAttribute("GO")]
        GeneralObligationBonds,
        [System.Xml.Serialization.XmlEnumAttribute("IET")]
        IOETTEMortgage,
        [System.Xml.Serialization.XmlEnumAttribute("IRS")]
        InterestRateSwap,
        [System.Xml.Serialization.XmlEnumAttribute("LOFC")]
        LetterOfCredit,
        [System.Xml.Serialization.XmlEnumAttribute("LQN")]
        LiquidityNote,
        [System.Xml.Serialization.XmlEnumAttribute("MATURED")]
        Matured,
        [System.Xml.Serialization.XmlEnumAttribute("MBS")]
        MortgagebackedSecurities,
        [System.Xml.Serialization.XmlEnumAttribute("MF")]
        MutualFund,
        [System.Xml.Serialization.XmlEnumAttribute("MIO")]
        MortgageInterestOnly,
        [System.Xml.Serialization.XmlEnumAttribute("MLEG")]
        MultilegInstrument,
        [System.Xml.Serialization.XmlEnumAttribute("MPO")]
        MortgagePrincipalOnly,
        [System.Xml.Serialization.XmlEnumAttribute("MPP")]
        MortgagePrivatePlacement,
        [System.Xml.Serialization.XmlEnumAttribute("MPT")]
        MiscellaneousPassthrough,
        [System.Xml.Serialization.XmlEnumAttribute("MT")]
        MandatoryTender,
        [System.Xml.Serialization.XmlEnumAttribute("MTN")]
        MediumTermNotes,
        [System.Xml.Serialization.XmlEnumAttribute("NONE")]
        NoSecurityType,
        [System.Xml.Serialization.XmlEnumAttribute("ONITE")]
        Overnight,
        [System.Xml.Serialization.XmlEnumAttribute("OOC")]
        OptionsOnCombo,
        [System.Xml.Serialization.XmlEnumAttribute("OOF")]
        OptionsOnFutures,
        [System.Xml.Serialization.XmlEnumAttribute("OOP")]
        OptionsOnPhysical,
        [System.Xml.Serialization.XmlEnumAttribute("OPT")]
        Option,
        [System.Xml.Serialization.XmlEnumAttribute("PEF")]
        PrivateExportFunding,
        [System.Xml.Serialization.XmlEnumAttribute("PFAND")]
        Pfandbriefe,
        [System.Xml.Serialization.XmlEnumAttribute("PN")]
        PromissoryNote,
        [System.Xml.Serialization.XmlEnumAttribute("PROV")]
        CanadianProvincialBonds,
        [System.Xml.Serialization.XmlEnumAttribute("PS")]
        PreferredStock,
        [System.Xml.Serialization.XmlEnumAttribute("PZFJ")]
        PlazosFijos,
        [System.Xml.Serialization.XmlEnumAttribute("RAN")]
        RevenueAnticipationNote,
        [System.Xml.Serialization.XmlEnumAttribute("REPLACD")]
        Replaced,
        [System.Xml.Serialization.XmlEnumAttribute("REPO")]
        Repurchase,
        [System.Xml.Serialization.XmlEnumAttribute("RETIRED")]
        Retired,
        [System.Xml.Serialization.XmlEnumAttribute("REV")]
        RevenueBonds,
        [System.Xml.Serialization.XmlEnumAttribute("RVLV")]
        RevolverLoan,
        [System.Xml.Serialization.XmlEnumAttribute("RVLVTRM")]
        RevolverTermLoan,
        [System.Xml.Serialization.XmlEnumAttribute("SECLOAN")]
        SecuritiesLoan,
        [System.Xml.Serialization.XmlEnumAttribute("SECPLEDGE")]
        SecuritiesPledge,
        [System.Xml.Serialization.XmlEnumAttribute("SLQN")]
        SecuredLiquidityNote,
        [System.Xml.Serialization.XmlEnumAttribute("SPCLA")]
        SpecialAssessment,
        [System.Xml.Serialization.XmlEnumAttribute("SPCLO")]
        SpecialObligation,
        [System.Xml.Serialization.XmlEnumAttribute("SPCLT")]
        SpecialTax,
        [System.Xml.Serialization.XmlEnumAttribute("STN")]
        ShortTermLoanNote,
        [System.Xml.Serialization.XmlEnumAttribute("STRUCT")]
        StructuredNotes,
        [System.Xml.Serialization.XmlEnumAttribute("SUPRA")]
        USDSupranationalCoupons,
        [System.Xml.Serialization.XmlEnumAttribute("SWING")]
        SwingLineFacility,
        [System.Xml.Serialization.XmlEnumAttribute("TAN")]
        TaxAnticipationNote,
        [System.Xml.Serialization.XmlEnumAttribute("TAXA")]
        TaxAllocation,
        [System.Xml.Serialization.XmlEnumAttribute("TB")]
        TreasuryBillNonUS,
        [System.Xml.Serialization.XmlEnumAttribute("TBA")]
        ToBeAnnounced,
        [System.Xml.Serialization.XmlEnumAttribute("TBILL")]
        USTreasuryBill,
        [System.Xml.Serialization.XmlEnumAttribute("TBOND")]
        USTreasuryBond,
        [System.Xml.Serialization.XmlEnumAttribute("TCAL")]
        PrincipalStripOfACallableBondOrNote,
        [System.Xml.Serialization.XmlEnumAttribute("TD")]
        TimeDeposit,
        [System.Xml.Serialization.XmlEnumAttribute("TECP")]
        TaxExemptCommercialPaper,
        [System.Xml.Serialization.XmlEnumAttribute("TERM")]
        TermLoan,
        [System.Xml.Serialization.XmlEnumAttribute("TINT")]
        InterestStripFromAnyBondOrNote,
        [System.Xml.Serialization.XmlEnumAttribute("TIPS")]
        TreasuryInflationProtectedSecurities,
        [System.Xml.Serialization.XmlEnumAttribute("TLQN")]
        TermLiquidityNote,
        [System.Xml.Serialization.XmlEnumAttribute("TMCP")]
        TaxableMunicipalCP,
        [System.Xml.Serialization.XmlEnumAttribute("TNOTE")]
        USTreasuryNote,
        [System.Xml.Serialization.XmlEnumAttribute("TPRN")]
        PrincipalStripFromANoncallableBondOrNote,
        [System.Xml.Serialization.XmlEnumAttribute("TRAN")]
        TaxRevenueAnticipationNote,
        [System.Xml.Serialization.XmlEnumAttribute("UST")]
        USTreasuryNoteDeprecatedValueUseTNOTE,
        [System.Xml.Serialization.XmlEnumAttribute("USTB")]
        USTreasuryBillDeprecatedValueUseTBILL,
        [System.Xml.Serialization.XmlEnumAttribute("VRDN")]
        VariableRateDemandNote,
        [System.Xml.Serialization.XmlEnumAttribute("WAR")]
        Warrant,
        [System.Xml.Serialization.XmlEnumAttribute("WITHDRN")]
        Withdrawn,
        [System.Xml.Serialization.XmlEnumAttribute("?")]
        WildcardEntryForUseOnSecurityDefinitionRequest,
        [System.Xml.Serialization.XmlEnumAttribute("XCN")]
        ExtendedCommNote,
        [System.Xml.Serialization.XmlEnumAttribute("XLINKD")]
        IndexedLinked,
        [System.Xml.Serialization.XmlEnumAttribute("YANK")]
        YankeeCorporateBond,
        [System.Xml.Serialization.XmlEnumAttribute("YCD")]
        YankeeCertificateOfDeposit,
    }
    #endregion SecurityTypeEnum
    #region SettlObligSourceEnum
    /* SettlObligSource(1164) */
    [System.SerializableAttribute()]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fixprotocol.org/FIXML-5-0-SP1")]
    public enum SettlObligSourceEnum
    {
        [System.Xml.Serialization.XmlEnumAttribute("1")]
        InstructionsOfBroker,
        [System.Xml.Serialization.XmlEnumAttribute("2")]
        InstructionsForInstitution,
        [System.Xml.Serialization.XmlEnumAttribute("3")]
        Investor,
    }
    #endregion SettlObligSourceEnum
    #region SettlTypeEnum
    /* SettlType(63) */
    [System.SerializableAttribute()]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fixprotocol.org/FIXML-5-0-SP1")]
    public enum SettlTypeEnum
    {
        [System.Xml.Serialization.XmlEnumAttribute("0")]
        Regular,
        [System.Xml.Serialization.XmlEnumAttribute("1")]
        Cash,
        [System.Xml.Serialization.XmlEnumAttribute("2")]
        NextDay,
        [System.Xml.Serialization.XmlEnumAttribute("3")]
        TPlus2,
        [System.Xml.Serialization.XmlEnumAttribute("4")]
        TPlus3,
        [System.Xml.Serialization.XmlEnumAttribute("5")]
        TPlus4,
        [System.Xml.Serialization.XmlEnumAttribute("6")]
        Future,
        [System.Xml.Serialization.XmlEnumAttribute("7")]
        WhenAndIfIssued,
        [System.Xml.Serialization.XmlEnumAttribute("8")]
        SellersOption,
        [System.Xml.Serialization.XmlEnumAttribute("9")]
        TPlus5,
        [System.Xml.Serialization.XmlEnumAttribute("B")]
        BrokenDate,
        [System.Xml.Serialization.XmlEnumAttribute("C")]
        FXSpotNextSettlement,
    }
    #endregion SettlTypeEnum
    #region ShortSaleReasonEnum
    /* ShortSaleReason(853) */
    [System.SerializableAttribute()]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fixprotocol.org/FIXML-5-0-SP1")]
    public enum ShortSaleReasonEnum
    {
        [System.Xml.Serialization.XmlEnumAttribute("0")]
        Dealer,
        [System.Xml.Serialization.XmlEnumAttribute("1")]
        DealerExempt,
        [System.Xml.Serialization.XmlEnumAttribute("2")]
        SellingCustomer,
        [System.Xml.Serialization.XmlEnumAttribute("3")]
        SellingCustomerExempt,
        [System.Xml.Serialization.XmlEnumAttribute("4")]
        QSROrAGUContraSide,
        [System.Xml.Serialization.XmlEnumAttribute("5")]
        QSROrAGUContraSideExempt,
    }
    #endregion ShortSaleReasonEnum
    #region SideMultiLegReportingTypeEnum
    /* SideMultiLegReportingType(752) */
    [System.SerializableAttribute()]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fixprotocol.org/FIXML-5-0-SP1")]
    public enum SideMultiLegReportingTypeEnum
    {
        [System.Xml.Serialization.XmlEnumAttribute("1")]
        SingleSecurity,
        [System.Xml.Serialization.XmlEnumAttribute("2")]
        IndividualLegOfMultilegSecurity,
        [System.Xml.Serialization.XmlEnumAttribute("3")]
        MultilegSecurity,
    }
    #endregion SideMultiLegReportingTypeEnum
    #region SideTrdSubTypEnum
    /* SideTrdSubTyp(1008) */
    [System.SerializableAttribute()]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fixprotocol.org/FIXML-5-0-SP1")]
    public enum SideTrdSubTypEnum
    {
        [System.Xml.Serialization.XmlEnumAttribute("0")]
        CMTA,
        [System.Xml.Serialization.XmlEnumAttribute("1")]
        InternalTransfer,
        [System.Xml.Serialization.XmlEnumAttribute("2")]
        ExternalTransfer,
        [System.Xml.Serialization.XmlEnumAttribute("3")]
        RejectForSubmittingTrade,
        [System.Xml.Serialization.XmlEnumAttribute("4")]
        AdvisoryForContraSide,
        [System.Xml.Serialization.XmlEnumAttribute("5")]
        OffsetDueToAnAllocation,
        [System.Xml.Serialization.XmlEnumAttribute("6")]
        OnsetDueToAnAllocation,
        [System.Xml.Serialization.XmlEnumAttribute("7")]
        DifferentialSpread,
        [System.Xml.Serialization.XmlEnumAttribute("8")]
        ImpliedSpread,
        [System.Xml.Serialization.XmlEnumAttribute("9")]
        TransactionFromExercise,
        [System.Xml.Serialization.XmlEnumAttribute("10")]
        TransactionFromAssignment,
    }
    #endregion SideTrdSubTypEnum
    #region SolicitedFlagEnum
    /* SolicitedFlag(377) */
    [System.SerializableAttribute()]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fixprotocol.org/FIXML-5-0-SP1")]
    public enum SolicitedFlagEnum
    {
        [System.Xml.Serialization.XmlEnumAttribute("N")]
        NotSolicited,
        [System.Xml.Serialization.XmlEnumAttribute("Y")]
        Solicited,
    }
    #endregion SolicitedFlagEnum
    #region StipulationTypeEnum
    /* StipulationType(233) */
    [System.SerializableAttribute()]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fixprotocol.org/FIXML-5-0-SP1")]
    public enum StipulationTypeEnum
    {
        [System.Xml.Serialization.XmlEnumAttribute("ABS")]
        AbsolutePrepaymentSpeed,
        [System.Xml.Serialization.XmlEnumAttribute("AMT")]
        AlternativeMinimumTax,
        [System.Xml.Serialization.XmlEnumAttribute("AUTOREINV")]
        AutoReinvestmentAtRateOrBetter,
        [System.Xml.Serialization.XmlEnumAttribute("BANKQUAL")]
        BankQualified,
        [System.Xml.Serialization.XmlEnumAttribute("BGNCON")]
        BargainConditions,
        [System.Xml.Serialization.XmlEnumAttribute("COUPON")]
        CouponRange,
        [System.Xml.Serialization.XmlEnumAttribute("CPP")]
        ConstantPrepaymentPenalty,
        [System.Xml.Serialization.XmlEnumAttribute("CPR")]
        ConstantPrepaymentRate,
        [System.Xml.Serialization.XmlEnumAttribute("CPY")]
        ConstantPrepaymentYield,
        [System.Xml.Serialization.XmlEnumAttribute("CURRENCY")]
        ISOCurrencyCode,
        [System.Xml.Serialization.XmlEnumAttribute("CUSTOMDATE")]
        CustomStartendDate,
        [System.Xml.Serialization.XmlEnumAttribute("GEOG")]
        GeographicsAndRange,
        [System.Xml.Serialization.XmlEnumAttribute("HAIRCUT")]
        ValuationDiscount,
        [System.Xml.Serialization.XmlEnumAttribute("HEP")]
        FinalCPROfHomeEquityPrepaymentCurve,
        [System.Xml.Serialization.XmlEnumAttribute("INSURED")]
        Insured,
        [System.Xml.Serialization.XmlEnumAttribute("ISSUE")]
        YearOrYearMonthOfIssue,
        [System.Xml.Serialization.XmlEnumAttribute("ISSUER")]
        IssuersTicker,
        [System.Xml.Serialization.XmlEnumAttribute("ISSUESIZE")]
        IssueSizeRange,
        [System.Xml.Serialization.XmlEnumAttribute("LOOKBACK")]
        LookbackDays,
        [System.Xml.Serialization.XmlEnumAttribute("LOT")]
        ExplicitLotIdentifier,
        [System.Xml.Serialization.XmlEnumAttribute("LOTVAR")]
        LotVarianceValueInPercentMaximumOverOrUnderallocationAllowed,
        [System.Xml.Serialization.XmlEnumAttribute("MAT")]
        MaturityYearAndMonth,
        [System.Xml.Serialization.XmlEnumAttribute("MATURITY")]
        MaturityRange,
        [System.Xml.Serialization.XmlEnumAttribute("MAXDNOM")]
        MaximumDenomination,
        [System.Xml.Serialization.XmlEnumAttribute("MAXSUBS")]
        MaximumSubstitutionsRepo,
        [System.Xml.Serialization.XmlEnumAttribute("MHP")]
        PercentOfManufacturedHousingPrepaymentCurve,
        [System.Xml.Serialization.XmlEnumAttribute("MINDNOM")]
        MinimumDenomination,
        [System.Xml.Serialization.XmlEnumAttribute("MININCR")]
        MinimumIncrement,
        [System.Xml.Serialization.XmlEnumAttribute("MINQTY")]
        MinimumQuantity,
        [System.Xml.Serialization.XmlEnumAttribute("MPR")]
        MonthlyPrepaymentRate,
        [System.Xml.Serialization.XmlEnumAttribute("PAYFREQ")]
        PaymentFrequencyCalendar,
        [System.Xml.Serialization.XmlEnumAttribute("PIECES")]
        NumberOfPieces,
        [System.Xml.Serialization.XmlEnumAttribute("PMAX")]
        PoolsMaximum,
        [System.Xml.Serialization.XmlEnumAttribute("PMIN")]
        PoolsMinimum,
        [System.Xml.Serialization.XmlEnumAttribute("PPC")]
        PercentOfProspectusPrepaymentCurve,
        [System.Xml.Serialization.XmlEnumAttribute("PPL")]
        PoolsPerLot,
        [System.Xml.Serialization.XmlEnumAttribute("PPM")]
        PoolsPerMillion,
        [System.Xml.Serialization.XmlEnumAttribute("PPT")]
        PoolsPerTrade,
        [System.Xml.Serialization.XmlEnumAttribute("PRICE")]
        PriceRange,
        [System.Xml.Serialization.XmlEnumAttribute("PRICEFREQ")]
        PricingFrequency,
        [System.Xml.Serialization.XmlEnumAttribute("PROD")]
        ProductionYear,
        [System.Xml.Serialization.XmlEnumAttribute("PROTECT")]
        CallProtection,
        [System.Xml.Serialization.XmlEnumAttribute("PSA")]
        PercentOfBMAPrepaymentCurve,
        [System.Xml.Serialization.XmlEnumAttribute("PURPOSE")]
        Purpose,
        [System.Xml.Serialization.XmlEnumAttribute("PXSOURCE")]
        BenchmarkPriceSource,
        [System.Xml.Serialization.XmlEnumAttribute("RATING")]
        RatingSourceAndRange,
        [System.Xml.Serialization.XmlEnumAttribute("REDEMPTION")]
        TypeOfRedemptionValuesAre,
        [System.Xml.Serialization.XmlEnumAttribute("RESTRICTED")]
        Restricted,
        [System.Xml.Serialization.XmlEnumAttribute("SECTOR")]
        MarketSector,
        [System.Xml.Serialization.XmlEnumAttribute("SECTYPE")]
        SecurityTypeIncludedOrExcluded,
        [System.Xml.Serialization.XmlEnumAttribute("SMM")]
        SingleMonthlyMortality,
        [System.Xml.Serialization.XmlEnumAttribute("STRUCT")]
        Structure,
        [System.Xml.Serialization.XmlEnumAttribute("SUBSFREQ")]
        SubstitutionsFrequencyRepo,
        [System.Xml.Serialization.XmlEnumAttribute("SUBSLEFT")]
        SubstitutionsLeftRepo,
        [System.Xml.Serialization.XmlEnumAttribute("TEXT")]
        FreeformText,
        [System.Xml.Serialization.XmlEnumAttribute("TRDVAR")]
        TradeVarianceValueInPercentMaximumOverOrUnderallocationAllowed,
        [System.Xml.Serialization.XmlEnumAttribute("WAC")]
        WeightedAverageCoupon,
        [System.Xml.Serialization.XmlEnumAttribute("WAL")]
        WeightedAverageLifeCoupon,
        [System.Xml.Serialization.XmlEnumAttribute("WALA")]
        WeightedAverageLoanAge,
        [System.Xml.Serialization.XmlEnumAttribute("WAM")]
        WeightedAverageMaturity,
        [System.Xml.Serialization.XmlEnumAttribute("WHOLE")]
        WholePool,
        [System.Xml.Serialization.XmlEnumAttribute("YIELD")]
        YieldRange,
        [System.Xml.Serialization.XmlEnumAttribute("AVFICO")]
        AverageFICOScore,
        [System.Xml.Serialization.XmlEnumAttribute("AVSIZE")]
        AverageLoanSize,
        [System.Xml.Serialization.XmlEnumAttribute("MAXBAL")]
        MaximumLoanBalance,
        [System.Xml.Serialization.XmlEnumAttribute("POOL")]
        PoolIdentifier,
        [System.Xml.Serialization.XmlEnumAttribute("ROLLTYPE")]
        TypeOfRollTrade,
        [System.Xml.Serialization.XmlEnumAttribute("REFTRADE")]
        ReferenceToRollingOrClosingTrade,
        [System.Xml.Serialization.XmlEnumAttribute("REFPRIN")]
        PrincipalOfRollingOrClosingTrade,
        [System.Xml.Serialization.XmlEnumAttribute("REFINT")]
        InterestOfRollingOrClosingTrade,
        [System.Xml.Serialization.XmlEnumAttribute("AVAILQTY")]
        AvailableOfferQuantityToBeShownToTheStreet,
        [System.Xml.Serialization.XmlEnumAttribute("BROKERCREDIT")]
        BrokersSalesCredit,
        [System.Xml.Serialization.XmlEnumAttribute("INTERNALPX")]
        OfferPriceToBeShownToInternalBrokers,
        [System.Xml.Serialization.XmlEnumAttribute("INTERNALQTY")]
        OfferQuantityToBeShownToInternalBrokers,
        [System.Xml.Serialization.XmlEnumAttribute("LEAVEQTY")]
        MinimumResidualOfferQuantity,
        [System.Xml.Serialization.XmlEnumAttribute("MAXORDQTY")]
        MaximumOrderSize,
        [System.Xml.Serialization.XmlEnumAttribute("ORDRINCR")]
        OrderQuantityIncrement,
        [System.Xml.Serialization.XmlEnumAttribute("PRIMARY")]
        PrimaryOrSecondaryMarketIndicator,
        [System.Xml.Serialization.XmlEnumAttribute("SALESCREDITOVR")]
        BrokerSalesCreditOverride,
        [System.Xml.Serialization.XmlEnumAttribute("TRADERCREDIT")]
        TradersCredit,
        [System.Xml.Serialization.XmlEnumAttribute("DISCOUNT")]
        DiscountRate,
        [System.Xml.Serialization.XmlEnumAttribute("YTM")]
        YieldToMaturity,
    }
    #endregion StipulationTypeEnum
    #region SubscriptionRequestTypeEnum
    /* SubscriptionRequestType(263) */
    [System.SerializableAttribute()]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fixprotocol.org/FIXML-5-0-SP1")]
    public enum SubscriptionRequestTypeEnum
    {
        [System.Xml.Serialization.XmlEnumAttribute("0")]
        Snapshot,
        [System.Xml.Serialization.XmlEnumAttribute("1")]
        Subscribe,
        [System.Xml.Serialization.XmlEnumAttribute("2")]
        Unsubscribe,
    }
    #endregion SubscriptionRequestTypeEnum
    #region TimeUnitEnum
    /* TimeUnit(997) */
    [System.SerializableAttribute()]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fixprotocol.org/FIXML-5-0-SP1")]
    public enum TimeUnitEnum
    {
        [System.Xml.Serialization.XmlEnumAttribute("S")]
        Second,
        [System.Xml.Serialization.XmlEnumAttribute("Min")]
        Minute,
        [System.Xml.Serialization.XmlEnumAttribute("H")]
        Hour,
        [System.Xml.Serialization.XmlEnumAttribute("D")]
        Day,
        [System.Xml.Serialization.XmlEnumAttribute("Wk")]
        Week,
        [System.Xml.Serialization.XmlEnumAttribute("Mo")]
        Month,
        [System.Xml.Serialization.XmlEnumAttribute("Yr")]
        Year,
    }
    #endregion TimeUnitEnum
    #region TradeAllocIndicatorEnum
    /* TradeAllocIndicator(826) */
    [System.SerializableAttribute()]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fixprotocol.org/FIXML-5-0-SP1")]
    public enum TradeAllocIndicatorEnum
    {
        [System.Xml.Serialization.XmlEnumAttribute("0")]
        AllocationNotRequired,
        [System.Xml.Serialization.XmlEnumAttribute("1")]
        AllocationRequired,
        [System.Xml.Serialization.XmlEnumAttribute("2")]
        UseAllocationProvidedWithTheTrade,
        [System.Xml.Serialization.XmlEnumAttribute("3")]
        AllocationGiveUpExecutor,
        [System.Xml.Serialization.XmlEnumAttribute("4")]
        AllocationFromExecutor,
        [System.Xml.Serialization.XmlEnumAttribute("5")]
        AllocationToClaimAccount,
    }
    #endregion TradeAllocIndicatorEnum
    #region TradeHandlingInstrEnum
    /* TradeHandlingInstr(1123) */
    [System.SerializableAttribute()]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fixprotocol.org/FIXML-5-0-SP1")]
    public enum TradeHandlingInstrEnum
    {
        [System.Xml.Serialization.XmlEnumAttribute("0")]
        TradeConfirmation,
        [System.Xml.Serialization.XmlEnumAttribute("1")]
        TwoPartyReport,
        [System.Xml.Serialization.XmlEnumAttribute("2")]
        OnePartyReportForMatching,
        [System.Xml.Serialization.XmlEnumAttribute("3")]
        OnePartyReportForPassThrough,
        [System.Xml.Serialization.XmlEnumAttribute("4")]
        AutomatedFloorOrderRouting,
        [System.Xml.Serialization.XmlEnumAttribute("5")]
        TwoPartyReportForClaim,
    }
    #endregion TradeHandlingInstrEnum
    #region TradePublishIndicatorEnum
    /* TradePublishIndicator(1390) */
    [System.SerializableAttribute()]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fixprotocol.org/FIXML-5-0-SP1")]
    public enum TradePublishIndicatorEnum
    {
        [System.Xml.Serialization.XmlEnumAttribute("0")]
        DoNotPublishTrade,
        [System.Xml.Serialization.XmlEnumAttribute("1")]
        PublishTrade,
        [System.Xml.Serialization.XmlEnumAttribute("2")]
        DeferredPublication,
    }
    #endregion TradePublishIndicatorEnum
    #region TradeReportTransTypeEnum
    /* TradeReportTransType(487) */
    [System.SerializableAttribute()]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fixprotocol.org/FIXML-5-0-SP1")]
    public enum TradeReportTransTypeEnum
    {
        [System.Xml.Serialization.XmlEnumAttribute("0")]
        New,
        [System.Xml.Serialization.XmlEnumAttribute("1")]
        Cancel,
        [System.Xml.Serialization.XmlEnumAttribute("2")]
        Replace,
        [System.Xml.Serialization.XmlEnumAttribute("3")]
        Release,
        [System.Xml.Serialization.XmlEnumAttribute("4")]
        Reverse,
        [System.Xml.Serialization.XmlEnumAttribute("5")]
        CancelDueToBackOutOfTrade,
    }
    #endregion TradeReportTransTypeEnum
    #region TradeReportTypeEnum
    /* TradeReportType(856) */
    [System.SerializableAttribute()]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fixprotocol.org/FIXML-5-0-SP1")]
    public enum TradeReportTypeEnum
    {
        [System.Xml.Serialization.XmlEnumAttribute("0")]
        Submit,
        [System.Xml.Serialization.XmlEnumAttribute("1")]
        Alleged,
        [System.Xml.Serialization.XmlEnumAttribute("2")]
        Accept,
        [System.Xml.Serialization.XmlEnumAttribute("3")]
        Decline,
        [System.Xml.Serialization.XmlEnumAttribute("4")]
        Addendum,
        [System.Xml.Serialization.XmlEnumAttribute("5")]
        NoWas,
        [System.Xml.Serialization.XmlEnumAttribute("6")]
        TradeReportCancel,
        [System.Xml.Serialization.XmlEnumAttribute("7")]
        TradeBreak,
        [System.Xml.Serialization.XmlEnumAttribute("8")]
        Defaulted,
        [System.Xml.Serialization.XmlEnumAttribute("9")]
        InvalidCMTA,
        [System.Xml.Serialization.XmlEnumAttribute("10")]
        Pended,
        [System.Xml.Serialization.XmlEnumAttribute("11")]
        AllegedNew,
        [System.Xml.Serialization.XmlEnumAttribute("12")]
        AllegedAddendum,
        [System.Xml.Serialization.XmlEnumAttribute("13")]
        AllegedNoWas,
        [System.Xml.Serialization.XmlEnumAttribute("14")]
        AllegedTradeReportCancel,
        [System.Xml.Serialization.XmlEnumAttribute("15")]
        AllegedTradeBreak,
    }
    #endregion TradeReportTypeEnum
    #region TradeRequestStatusEnum
    /* TradeRequestStatus(750) */
    [System.SerializableAttribute()]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fixprotocol.org/FIXML-5-0-SP1")]
    public enum TradeRequestStatusEnum
    {
        [System.Xml.Serialization.XmlEnumAttribute("0")]
        Accepted,
        [System.Xml.Serialization.XmlEnumAttribute("1")]
        Completed,
        [System.Xml.Serialization.XmlEnumAttribute("2")]
        Rejected,
    }
    #endregion TradeRequestStatusEnum
    #region TradeRequestTypeEnum
    /* TradeRequestType(569) */
    [System.SerializableAttribute()]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fixprotocol.org/FIXML-5-0-SP1")]
    public enum TradeRequestTypeEnum
    {
        [System.Xml.Serialization.XmlEnumAttribute("0")]
        AllTrades,
        [System.Xml.Serialization.XmlEnumAttribute("1")]
        MatchedTradesMatchingCriteriaProvidedOnRequest,
        [System.Xml.Serialization.XmlEnumAttribute("2")]
        UnmatchedTradesThatMatchCriteria,
        [System.Xml.Serialization.XmlEnumAttribute("3")]
        UnreportedTradesThatMatchCriteria,
        [System.Xml.Serialization.XmlEnumAttribute("4")]
        AdvisoriesThatMatchCriteria,
    }
    #endregion TradeRequestTypeEnum
    #region TrdRegTimestampTypeEnum
    /* TrdRegTimestampType(770) */
    [System.SerializableAttribute()]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fixprotocol.org/FIXML-5-0-SP1")]
    public enum TrdRegTimestampTypeEnum
    {
        [System.Xml.Serialization.XmlEnumAttribute("1")]
        ExecutionTime,
        [System.Xml.Serialization.XmlEnumAttribute("2")]
        TimeIn,
        [System.Xml.Serialization.XmlEnumAttribute("3")]
        TimeOut,
        [System.Xml.Serialization.XmlEnumAttribute("4")]
        BrokerReceipt,
        [System.Xml.Serialization.XmlEnumAttribute("5")]
        BrokerExecution,
        [System.Xml.Serialization.XmlEnumAttribute("6")]
        DeskReceipt,
        [System.Xml.Serialization.XmlEnumAttribute("7")]
        SubmissionToClearing,
        [System.Xml.Serialization.XmlEnumAttribute("8")]
        TimePriority,
        [System.Xml.Serialization.XmlEnumAttribute("9")]        	
        OrderbookEntryTime,
        [System.Xml.Serialization.XmlEnumAttribute("10")]
        OrderSubmissionTime,
        [System.Xml.Serialization.XmlEnumAttribute("11")]
        PubliclyReported,
        [System.Xml.Serialization.XmlEnumAttribute("12")]
        PublicReportUpdated,
        [System.Xml.Serialization.XmlEnumAttribute("13")]
        NonPubliclyReported,
        [System.Xml.Serialization.XmlEnumAttribute("14")]
        NonPublicReportUpdated,
        [System.Xml.Serialization.XmlEnumAttribute("15")]        	
        SubmittedForConfirmation,
        [System.Xml.Serialization.XmlEnumAttribute("16")]
        UpdatedForConfirmation,
        [System.Xml.Serialization.XmlEnumAttribute("17")]
        Confirmed,
        [System.Xml.Serialization.XmlEnumAttribute("18")]        	
        UpdatedForClearing,
        [System.Xml.Serialization.XmlEnumAttribute("19")]
        Cleared,
        [System.Xml.Serialization.XmlEnumAttribute("20")]
        AllocationsSubmitted,
        [System.Xml.Serialization.XmlEnumAttribute("21")]
        AllocationsUpdated,
        [System.Xml.Serialization.XmlEnumAttribute("22")]
        ApplicationCompleted,
        [System.Xml.Serialization.XmlEnumAttribute("23")]
        SubmittedToRepository,
        [System.Xml.Serialization.XmlEnumAttribute("24")]
        PostTradeContinuationEvent,
        [System.Xml.Serialization.XmlEnumAttribute("25")]
        PostTradeValuation,
        [System.Xml.Serialization.XmlEnumAttribute("26")]
        PreviousTimePriority,
    }
    #endregion TrdRegTimestampTypeEnum
    #region TrdRptStatusEnum
    /* TrdRptStatus(939) */
    [System.SerializableAttribute()]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fixprotocol.org/FIXML-5-0-SP1")]
    public enum TrdRptStatusEnum
    {
        [System.Xml.Serialization.XmlEnumAttribute("0")]
        Accepted,
        [System.Xml.Serialization.XmlEnumAttribute("1")]
        Rejected,
        [System.Xml.Serialization.XmlEnumAttribute("3")]
        AcceptedWithErrors,
    }
    #endregion TrdRptStatusEnum
    #region UnderlyingCashTypeEnum
    /* UnderlyingCashType(974) */
    [System.SerializableAttribute()]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fixprotocol.org/FIXML-5-0-SP1")]
    public enum UnderlyingCashTypeEnum
    {
        FIXED,
        DIFF,
    }
    #endregion UnderlyingCashTypeEnum
    #region UnderlyingFXRateCalcEnum
    /* UnderlyingFXRateCalc(1046) */
    [System.SerializableAttribute()]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fixprotocol.org/FIXML-5-0-SP1")]
    public enum UnderlyingFXRateCalcEnum
    {
        [System.Xml.Serialization.XmlEnumAttribute("M")]
        Multiply,
        [System.Xml.Serialization.XmlEnumAttribute("D")]
        Divide,
    }
    #endregion UnderlyingFXRateCalcEnum
    #region UnderlyingSettlementTypeEnum
    /* UnderlyingSettlementType(975) */
    [System.SerializableAttribute()]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fixprotocol.org/FIXML-5-0-SP1")]
    public enum UnderlyingSettlementTypeEnum
    {
        [System.Xml.Serialization.XmlEnumAttribute("2")]
        TPlus1,
        [System.Xml.Serialization.XmlEnumAttribute("4")]
        TPlus3,
        [System.Xml.Serialization.XmlEnumAttribute("5")]
        TPlus4,
    }
    #endregion UnderlyingSettlementTypeEnum
    #region UnitOfMeasureEnum
    /* UnitOfMeasure(996) */
    [System.SerializableAttribute()]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fixprotocol.org/FIXML-5-0-SP1")]
    public enum UnitOfMeasureEnum
    {
        [System.Xml.Serialization.XmlEnumAttribute("MWh")]
        MegawattHours,
        [System.Xml.Serialization.XmlEnumAttribute("MMBtu")]
        OneMillionBTU,
        [System.Xml.Serialization.XmlEnumAttribute("Bbl")]
        Barrels,
        [System.Xml.Serialization.XmlEnumAttribute("Gal")]
        Gallons,
        [System.Xml.Serialization.XmlEnumAttribute("t")]
        MetricTons,
        [System.Xml.Serialization.XmlEnumAttribute("tn")]
        TonsUS,
        [System.Xml.Serialization.XmlEnumAttribute("MMbbl")]
        MillionBarrels,
        [System.Xml.Serialization.XmlEnumAttribute("lbs")]
        Pounds,
        [System.Xml.Serialization.XmlEnumAttribute("oz_tr")]
        TroyOunces,
        [System.Xml.Serialization.XmlEnumAttribute("USD")]
        USDollars,
        [System.Xml.Serialization.XmlEnumAttribute("Bcf")]
        BillionCubicFeet,
        [System.Xml.Serialization.XmlEnumAttribute("Bu")]
        Bushels,
    }
    #endregion UnitOfMeasureEnum
    #region UnsolicitedIndicatorEnum
    /* UnsolicitedIndicator(325) */
    [System.SerializableAttribute()]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fixprotocol.org/FIXML-5-0-SP1")]
    public enum UnsolicitedIndicatorEnum
    {
        [System.Xml.Serialization.XmlEnumAttribute("N")]
        ResultOfAPriorRequest,
        [System.Xml.Serialization.XmlEnumAttribute("Y")]
        Unsolicited,
    }
    #endregion UnsolicitedIndicatorEnum
    #endregion Enums Version 5.0 SP1
}


namespace FixML.v50SP2.Enum
{

    /// <summary>
    /// 
    /// </summary>
    /// FI 20170928 [23452] Add 
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fixprotocol.org/FIXML-5-0-SP2")]
    public enum PartyDetailRoleQualifier
    {
        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlEnumAttribute("24")]
        Algorithm,
        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlEnumAttribute("22")]
        NaturalPerson
    }

}