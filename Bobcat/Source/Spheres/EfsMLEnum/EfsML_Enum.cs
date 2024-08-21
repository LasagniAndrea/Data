#region using directives
using System;
using System.Xml.Serialization;
#endregion using directives

namespace EfsML.Enum
{
    /// <summary>
    /// 
    /// </summary>
    public enum AccruedInterestCalculationMethodEnum
    {
        Native,
        Prorata,
    }

    /// <summary>
    /// 
    /// </summary>
    public enum ActorInfoStyleEnum
    {
        id,
        identifier,
        description,
        bic,
        cssIdentifier,
        LTAdress,
    }
    
    /// <summary>
    /// 
    /// </summary>
    public enum ActorTypeEnum
    {
        NA,
        actor,
        bookOwner,
        party,
        trader
    }

    /// <summary>
    /// Enabled,Disabled
    /// </summary>
    public enum AvailabilityEnum
    {
        NA, Enabled, Disabled
    }
    
    #region AveragingTypeEnum
    public enum AveragingTypeEnum
    {
        ArithmeticAverage,
        GeometricAverage,
        WeightedArithmeticAverage,
        Highest,
        Lowest,
        HighestLowest,
        FirstLast,
    }
    #endregion AveragingTypeEnum

    #region BaseRateMethodEnum
    public enum BaseRateMethodEnum
    {
        ActuarialEquivalent,
        Extrapolated,
        FRAEquivalent,
        InFineEquivalent,
        Interpolated,
        InterpolatedExtrapolated,
        MoneyMarketEquivalent,
        NA,
        SourceRate,
    }
    #endregion BaseRateMethodEnum
    #region BaseRateTypeEnum
    /// <summary>
    /// 
    /// </summary>
    /// FI 20100526 [17014] Ajout du point 6 mois 
    /// Ce point doit exister lorsque le bootstrapping pour le calcul du discountFactor des points > 1  commence à 6 mois)
    public enum BaseRateTypeEnum
    {
        Bond,
        Deposit,
        FRA,
        Future,
        Interpolated,
        NA,
        /// <summary>
        /// Indique le point à 1 an
        /// /// </summary>
        OneYear,
        Overnight,
        /// <summary>
        /// Indique le point à 6 mois
        /// </summary>
        SixtMonth,
        Spotnext,
        StubFRAs,
        StubFutures,
        Swap,
        Tom,
        Tomnext,
    }
    #endregion BaseRateType
    #region BasketTypeEnum
    public enum BasketTypeEnum
    {
        Average,
        BetterOf,
        Max,
        Min,
        OutPerformance,
        Spread,
        WorseOf,
    }
    #endregion BasketTypeEnum

    #region BasketUnitTypeWeightEnum
    /// <summary>
    /// Type d'unité de poids pour les paniers
    /// </summary>
    // PM 20201028 [25570][25542] Ajout BasketUnitTypeWeightEnum
    public enum BasketUnitTypeWeightEnum
    {
        basketPercentage,
        openUnits,
    }
    #endregion BasketUnitTypeWeightEnum

    #region BracketApplicationEnum
    public enum BracketApplicationEnum
    {
        Cumulative,
        Unit
    };
    #endregion BracketApplicationEnum

    /// <summary>
    /// 
    /// </summary>
    public enum BuyerSellerEnum
    {
        BUYER,
        SELLER,
    }
    
    /// <summary>
    /// 
    /// </summary>
    public enum BuySellEnum
    {
        BUY,
        SELL,
        NONE
    }

    /// <summary>
    /// Localization of the cover elements deposited to consider
    /// </summary>
    public enum CashAndCollateralLocalizationEnum
    {
        /// <summary>
        /// This Cash Balance Office
        /// </summary>
        CBO,
        /// <summary>
        /// Child Cash Balance Offices
        /// </summary>
        CBOChild,
        /// <summary>
        /// Child Margin Requirement Offices
        /// </summary>
        MROChild,
    }

    #region CashBalanceCalculationMethodEnum
    /// <summary>
    /// Methode de calcul du Cash Balance
    /// </summary>
    public enum CashBalanceCalculationMethodEnum
    {
        /// <summary>
        /// Méthode par défaut (Française)
        /// </summary>
        CSBDEFAULT,
        /// <summary>
        /// Méthode Anglaise
        /// </summary>
        CSBUK,
    }
    #endregion CashBalanceCalculationMethodEnum

    #region CashFlowCalculationMethodEnum
    /// <summary>
    /// Methode de calcul des Cash Flows (ETD)
    /// </summary>
    //PM 20141120 [20508] Ajout de l'enum CashFlowCalculationMethodEnum
    public enum CashFlowCalculationMethodEnum
    {
        /// <summary>
        /// Calcul sans aucun arrondi intermédiaire (valeur par défaut)
        /// </summary>
        OVERALL,
        /// <summary>
        /// Calcul unitaire arrondi avant d'appliquer la quantité
        /// </summary>
        UNITARY,
    }
    #endregion CashFlowCalculationMethodEnum

    /// <summary>
    /// Méthode de calcul de l'appel/restitution de déposit
    /// </summary>
    public enum MarginCallCalculationMethodEnum
    {
        /// <summary>
        /// Déposit et couverture en devise
        /// </summary>
        MGCCOLLATCUR,
        /// <summary>
        /// Déposit et couverture en contrevaleur
        /// </summary>
        MGCCTRVAL,
        /// <summary>
        /// Déposit en devise et couverture en contrevaleur
        /// </summary>
        MGCCOLLATCTRVAL,
    }

    /// <summary>
    /// Category of the collateral
    /// </summary>
    public enum CollateralCategoryEnum
    {
        /// <summary>
        /// Assets (issus du référentiel POSCOLLATERAL et POSCOLLATERALVAL)
        /// </summary>
        Assets,
        /// <summary>
        /// Available cash (Cash Disponible du jour)
        /// </summary>
        AvailableCash,
    }

    #region CashFlowTypeEnum
    public enum CashFlowTypeEnum
    {
        CouponPayment,
        BrokerageFee,
        NA,
        Other,
        PremiumFee,
        SettlementFee,
    }
    #endregion CashFlowTypeEnum
    #region CashSecuritiesEnum
    public enum CashSecuritiesEnum
    {
        CASH,
        SECURITIES,
    }
    #endregion EfsMLDocumentVersion
    #region CompoundingFrequencyEnum
    public enum CompoundingFrequencyEnum
    {
        Daily,
        Weekly,
        Monthly,
        Quaterly,
        SemiAnnually,
        Annual,
        InFine,
        Continuous,
    }
    #endregion CompoundingFrequencyEnum
    #region CfiCodeCategoryEnum
    /// <summary>
    /// Future ou Option
    /// </summary>
    [SerializableAttribute()]
    public enum CfiCodeCategoryEnum
    {
        [System.Xml.Serialization.XmlEnumAttribute("F")]
        Future,
        [System.Xml.Serialization.XmlEnumAttribute("O")]
        Option,
    }
    #endregion CfiCodeCategoryEnum
    #region CorridorTypeEnum
    public enum CorridorTypeEnum
    {
        ValuesExcluded,
        ValuesCapFloored,
    }
    #endregion CorridorTypeEnum
    #region CPProgramEnum
    public enum CPProgramEnum
    {
        [System.Xml.Serialization.XmlEnumAttribute("0")]
        NA,
        [System.Xml.Serialization.XmlEnumAttribute("1")]
        Program3a3,
        [System.Xml.Serialization.XmlEnumAttribute("2")]
        Program42,
        [System.Xml.Serialization.XmlEnumAttribute("3")]
        Program3a4,
        [System.Xml.Serialization.XmlEnumAttribute("4")]
        Program3c7,
        [System.Xml.Serialization.XmlEnumAttribute("5")]
        Program144A,
        [System.Xml.Serialization.XmlEnumAttribute("6")]
        Program3a2,
        [System.Xml.Serialization.XmlEnumAttribute("99")]
        Other,
    }
    #endregion CPProgramEnum

    /// <summary>
    /// Latin terms for Debit-Credit
    /// </summary>
    public enum CrDrEnum
    {
        /// <summary>
        /// Crédit
        /// </summary>
        CR,
        /// <summary>
        /// Débit
        /// </summary>
        DR
    
    }

    #region DebtSecurityTypeEnum
    // EG 20190823 [FIXEDINCOME] New 
    public enum DebtSecurityTypeEnum
    {
        Ordinary,
        Perpetual,
    }
    #endregion DebtSecurityTypeEnum

    #region DeliveryMethodEnum
    public enum DeliveryMethodEnum
    {
        DeliveryVersusPayment,
        FreeOfPayment,
    }
    #endregion DeliveryMethodEnum
    #region DerivativeContractValueEnum
    public enum DerivativeContractValueEnum
    {
        IDENTICAL
    }
    #endregion DerivativeContractValueEnum

    /// <summary>
    /// Type de DC (FLEX,LEAPS,STD) 
    /// </summary>
    /// FI 20140206 [19564] add enum 
    public enum DerivativeContractTypeEnum
    {
        /// <summary>
        /// FLexible EXchange
        /// </summary>
        FLEX,
        /// <summary>
        /// Long-term Equity AnticiPation Securities
        /// </summary>
        LEAPS,
        /// <summary>
        /// Standard
        /// </summary>
        STD
    }

    #region EfsMLDocumentVersionEnum
    /// <summary>
    /// 
    /// </summary>
    public enum EfsMLDocumentVersionEnum
    {
        /// <summary>
        /// version 1-0
        /// </summary>
        /// FI 20140618 [19923] add value 
        [System.Xml.Serialization.XmlEnumAttribute("1-0")]
        Version10,
        [System.Xml.Serialization.XmlEnumAttribute("2-0")]
        Version20,
        [System.Xml.Serialization.XmlEnumAttribute("3-0")]
        Version30,
        /// <summary>
        /// version 3-1
        /// </summary>
        /// FI 20130625 [18745]
        [System.Xml.Serialization.XmlEnumAttribute("3-1")]
        Version31,
        /// <summary>
        /// version 3-5
        /// </summary>
        [System.Xml.Serialization.XmlEnumAttribute("3-5")]
        Version35,
    }
    #endregion EfsMLDocumentVersionEnum
    #region EFS_FieldMemberEnum
    public enum EFS_FieldMemberEnum
    {
        Boolean,
        Method,
        Property,
        Constant,
        StaticMethod,
        StaticProperty,
        Unknown,
    }
    #endregion EFS_FieldMemberEnum
    #region EquiRateMethodEnum
    public enum EquiRateMethodEnum
    {
        /// <summary>
        /// Convertion d'un taux monétaire en taux equivalent actuariel
        /// </summary>
        CompoundToSimple,
        /// <summary>
        /// Convertion d'un taux actuariel (ou taux zero coupon) en taux equivalent Monétaire
        /// </summary>
        SimpleToCompound,
        /// <summary>
        /// Convertion d'un taux actuariel (ou taux zero coupon) en taux equivalent overnight décapitalise
        /// </summary>
        SimpleToOvernightDecapitalized,
    }
    #endregion EquiRateMethodEnum
    #region EventClassEnum
    public enum EventClassEnum
    {
        #region Administration Level Trade Events
        CLA,			/* Daily closing accounting */
        //
        LIN,			/* Linear     (Acrrued Interests) */
        /// <summary>
        /// Prorata    (Acrrued Interests / Linear depreciation / ...)
        /// </summary>
        PRO,			/*  */
        //
        /// <summary>
        /// Cox, Ross et Rubinstein (MTM)
        /// </summary>
        CRR,			
        /// <summary>
        /// Black-Scholes  (MTM)
        /// </summary>
        B_S,			
        /// <summary>
        /// Garman and Kolhagen  (MTM)
        /// </summary>
        G_K,			
        /// <summary>
        /// Forward Rate Projection (MTM)
        /// </summary>
        FRP,            
        /// <summary>
        /// Forward Rate (MTM / FX)
        /// </summary>
        FWR,			
        /// <summary>
        /// Spot Rate (MTM / FX and ETD)
        /// </summary>
        SPR,			
        /// <summary>
        /// Linear depreciation (prorata amount by accrualDays/TotalDays) 
        /// </summary>
        LDP,			
        /// <summary>
        /// Linear depreciation remaining (prorata amount by remainingAccrualDays/TotalDays)
        /// </summary>
        LDR,			
        /// <summary>
        /// Group level
        /// </summary>
        GRP,			
        /// <summary>
        /// Remove Event
        /// </summary>
        RMV,            
        #endregion Administration Level Trade Events
        #region Calculation Level Trade Events
        /// <summary>
        /// Average date
        /// </summary>
        AVG,     
        /// <summary>
        /// Compounded rate
        /// </summary>
        CMP, 
        /// <summary>
        /// Cash Settlement Payment date 
        /// </summary>
        CSP,			
        /// <summary>
        /// Cash Settlement Valuation date
        /// </summary>
        CSV,	
        /// <summary>
        /// Election Settlement date 
        /// </summary>
        ESD,	
        /// <summary>
        /// Fixing rate date
        /// </summary>
        FXG,
        /// <summary>
        /// Relevant Underlying date
        /// </summary>
        RUD,			
        #endregion Calculation Level Trade Events
        #region Description Level Trade Events
        CSH,            /* CashSettlement */
        DAT,			/* Dates */
        DLY,			/* DeliveryDelay */
        // EG 20150907 [21317]
        EXD,            /* Ex Date */
        EXC,            /* Exercise Cancelable */
        EXM,            /* Exercise MandatoryEarlyTermination*/
        ELE,            /* ElectionSettlement */
        EXO,            /* Exercise OptionalEarlyTermination */
        EXS,            /* Exercise Step-Up */
        EXX,            /* Exercise Extendible */
        KNK,			/* Barrier knock */
        PHY,            /* PhysicalSettlement */
        RAT,			/* Rate */
        // EG 20150907 [21317]
        RCD,            /* Record Date */
        TCH,			/* Trigger touch */
        #endregion Description Level Trade Events
        #region Group Level Trade Events
        DLM,			/* Delivery message */
        INV,			/* Invoiced */
        PRS,			/* Pre-settlement */
        REC,			/* Recognition */
        STL,			/* Settlement */
        STM,			/* Settlement message */
        VAL, 			/* Value date */

        #endregion Group Level Trade Events

        #region Others
        AMT,			/* Amount */
        CUT,			/* Rate cut-off date */
        #endregion Others
    }
    #endregion EventClassEnum
    #region EventCodeEnum
    // EG 20190613 [24683] New OCP
    // EG 20190926 [Maturity Redemption] Add MOD, RAM
    public enum EventCodeEnum
    {
        #region Administration Level Trade Events
        /// <summary>
        /// AdditionalPayment
        /// </summary>
        ADP,
        /// <summary>
        /// Daily closing
        /// </summary>
        CLO,
        /// <summary>
        /// Depreciable Amount
        /// </summary>
        DEA,
        /// <summary>
        /// Intermediary
        /// </summary>
        INT,
        /// <summary>
        /// Linked Future closing
        /// </summary>
        LFC,
        /// <summary>
        /// Linked Future intraday
        /// </summary>
        LFI,
        /// <summary>
        /// Linked Option closing
        /// </summary>
        LOC,
        /// <summary>
        /// Linked Option intraday
        /// </summary>
        LOI,
        /// <summary>
        /// Linked Product closing
        /// </summary>
        LPC,
        /// <summary>
        /// Linked Product intraday
        /// </summary>
        LPI,
        /// <summary>
        /// Linked Product payment
        /// </summary>
        LPP,
        /// <summary>
        /// Nominal step
        /// </summary>
        NOS,			/* Nominal step */
        /// <summary>
        /// Nominal Quantity step
        /// </summary>
        NQS,
        /// <summary>
        /// Other party payment
        /// </summary>
        OPP,
        /// <summary>
        /// Start
        /// </summary>
        STA,
        /// <summary>
        /// Start intermediary
        /// </summary>
        STI,
        /// <summary>
        /// Total abandon
        /// </summary>
        TAB,
        /// <summary>
        /// Termination
        /// </summary>
        TER,
        /// <summary>
        /// Termination intermediary
        /// </summary>
        TEI,
        #endregion Administration Level Trade Events
        #region Calculation Level Trade Events
        /// <summary>
        /// Calculation period
        /// </summary>
        PER,
        /// <summary>
        /// Reset event
        /// </summary>
        RES,
        /// <summary>
        /// Self average
        /// </summary>
        SAV,
        /// <summary>
        /// SafeKeepingPayment
        /// </summary>
        SKP,
        /// <summary>
        /// Self reset 
        /// </summary>
        SRT,
        #endregion Calculation Level Trade Events
        #region Description Level Trade Events
        /// <summary>
        /// Assignment Dates
        /// </summary>
        ASD,
        /// <summary>
        /// Barriers
        /// </summary>
        BAR,
        /// <summary>
        /// Exercise dates
        /// </summary>
        EXD,
        /// <summary>
        /// Automatic Exercise dates
        /// </summary>
        EAD,
        /// <summary>
        /// Effective Rate
        /// </summary>
        EFR,
        /// <summary>
        /// Trigger
        /// </summary>
        TRG,
        #endregion Description Level Trade Events
        #region Group Level Trade Events
        /// <summary>
        /// /* Fx American Average Rate option */
        /// </summary>
        AAO,
        /// <summary>
        /// /* Abandon */
        /// </summary>
        ABN,
        /// <summary>
        /// /* Fx American Barrier option */
        /// </summary>
        ABO,
        /// <summary>
        /// /* Fx American Digital option */
        /// </summary>
        ADO,
        /// <summary>
        /// /* American Exchange Traded derivative option */
        /// </summary>
        AED,
        /// <summary>
        /// /* American Equity option */
        /// </summary>
        AEO,
        /// <summary>
        /// /* American Bond option */
        /// </summary>
        BOA,
        /// <summary>
        /// /* AdditionalInvoiceDates */
        /// </summary>
        AID,
        /// <summary>
        /// /* AllocatedInvoiceDates */
        /// </summary>
        ALD,
        /// <summary>
        /// /* Asian Features */
        /// </summary>
        ASI,
        /// <summary>
        /// /* Fx American Simple option */
        /// </summary>
        ASO,
        /// <summary>
        /// /* Assignment */
        /// </summary>
        ASS,
        /// <summary>
        /// /* American Swaption */
        /// </summary>
        ASW,
        /// <summary>
        /// /* Fx Bermuda Average Rate option */
        /// </summary>
        BAO,
        /// <summary>
        /// /* Fx Bermuda Barrier option */
        /// </summary>
        BBO,
        /// <summary>
        /// /* Bermuda Equity option */
        /// </summary>
        BEO,
        /// <summary>
        /// /* BuyAndSellBack */
        /// </summary>
        BSB,
        /// <summary>
        /// /* Fx Bermuda Simple option */
        /// </summary>
        BSO,
        /// <summary>
        /// /* Bermuda Bond option */
        /// </summary>
        BOB,
        /// <summary>
        /// /* Bermuda Swaption */
        /// </summary>
        BSW,
        /// <summary>
        /// /* Bullet payment */
        /// </summary>
        BUL,
        /// <summary>
        /// /* Cascading */
        /// </summary>
        CAS,
        /// <summary>
        /// /* Cash Balance Stream */
        /// </summary>
        CBS,
        /// <summary>
        /// /* Cash Interest Stream */
        /// </summary>
        // PM 20120824 [18058] Add CIS (Cash Interest Stream)
        CIS,
        /// <summary>
        /// /* Cap Floor */
        /// </summary>
        CFL,
        /// <summary>
        /// /* Cash Flow Constituent */
        /// </summary>
        CFC,
        /// <summary>
        /// /* Credit Note Dates */
        /// </summary>
        CND,
        /// <summary>
        /// /* DebtSecurity stream */
        /// </summary>
        DSS,
        /// <summary>
        /// /* DebtSecurity transaction */
        /// </summary>
        DST,
        /// <summary>
        /// /* Fx European Average Rate option */
        /// </summary>
        EAO,
        /// <summary>
        /// /* Fx European Barrier option */
        /// </summary>
        EBO,
        /// <summary>
        /// /* Exchange Cash Balance Stream */
        /// </summary>
        ECS,
        /// <summary>
        /// /* Fx European Digital option */
        /// </summary>
        EDO,
        /// <summary>
        /// /* European Exchange Traded derivative option */
        /// </summary>
        EED,
        /// <summary>
        /// /* European Equity option */
        /// </summary>
        EEO,
        /// <summary>
        /// /* European Bond option */
        /// </summary>
        BOE,
        /// <summary>
        /// /* Delivery */
        /// </summary>
        DLV,
        /// <summary>
        /// /* Fx European Simple option */
        /// </summary>
        ESO,
        /// <summary>
        /// /* Equity Security Transaction*/
        /// </summary>
        EST,
        /// <summary>
        /// European Swaption
        /// </summary>
        ESW,
        /// <summary>
        /// Exercise
        /// </summary>
        EXE,
        /// <summary>
        /// Exercise Cancelable Provision 
        /// </summary>
        EXC,
        /// <summary>
        /// Exercise Extendible Provision
        /// </summary>
        EXX,
        /// <summary>
        /// Exercise Mandatory Early Termination Provision
        /// </summary>
        EXM,
        /// <summary>
        /// Exercise Step-Up Provision
        /// </summary>
        EXS,
        /// <summary>
        /// Exercise Optional Early Termination Provision 
        /// </summary>
        EXO,
        /// <summary>
        /// Future Exchange Traded derivative
        /// </summary>
        FED,
        /// <summary>
        /// /* Future rate agreement */
        /// </summary>
        FRA,
        /// <summary>
        /// Future Style Option
        /// </summary>
        FSO,
        /// <summary>
        /// /* Fx spot */
        /// </summary>
        FXS,
        /// <summary>
        /// /* Fx Forward */
        /// </summary>
        FXF,
        /// <summary>
        /// /* Invoice Amended */
        /// </summary>
        IAM,
        /// <summary>
        /// /* Invoice Master Base */
        /// </summary>
        IMB,
        /// <summary>
        /// /* Invoice Master */
        /// </summary>
        IMS,
        /// <summary>
        /// /* Initial Valuation */
        /// </summary>
        INI,
        /// <summary>
        /// /* Invoice */
        /// </summary>
        INV,
        /// <summary>
        /// /* Interest rate swap */
        /// </summary>
        IRS,
        /// <summary>
        /// /* InvoiceSettlement */
        /// </summary>
        IST,
        /// <summary>
        /// /* LoanDeposit */
        /// </summary>
        L_D,
        /// <summary>
        /// /* LookBack option */
        /// </summary>
        LBK,
        /// <summary>
        /// /* Leg */
        /// </summary>
        LEG,
        /// <summary>
        /// /* Margin Requirement */
        /// </summary>
        MGR,
        /// <summary>
        /// /* Maturity Off-Setting Future*/
        /// </summary>
        MOF,
        /// <summary>
        /// /* Maturity Redemption Off-Setting DebtSecurity */
        /// </summary>
        MOD,
        /// <summary>
        /// /* Off-Setting Corporate action */
        /// </summary>
        OCA,
        /// <summary>
        /// /* Off-Setting Closing/Reopening position */
        /// </summary>
        OCP,
        /// <summary>
        /// /* Off-Setting */
        /// </summary>
        OFS,
        /// <summary>
        /// /* Out */
        /// </summary>
        OUT,
        /// <summary>
        /// /* Position Cancelation */
        /// </summary>
        POC,
        /// <summary>
        /// /* Position Transfert */
        /// </summary>
        POT,
        /// <summary>
        /// /* Product dates */
        /// </summary>
        PRD,
        /// <summary>
        /// /* Optional Provision */
        /// </summary>
        PRO,
        /// <summary>
        /// Premium Style Option
        /// </summary>
        PSO,
        /// <summary>
        ///  /* Repo */
        /// </summary>
        REP,
        /// <summary>
        /// /* Repurchase Agreement Spot Leg */
        /// </summary>
        RFL,
        /// <summary>
        ///   /* Repurchase Agreement Forward Leg */
        /// </summary>
        RSL,
        /// <summary>
        /// /* Return Swap */
        /// </summary>
        BSK, /* Basket */
        SUL, /* Single Underlyer */
        UNL, /* Underlyer */
        UBC, /* Underlyer basket constituent */
        TRL, /* Total return leg */
        PRL, /* Price return leg */
        DRL, /* Dividend return leg */
        INL, /* Interest Leg */
        // EG 20231127 [WI754] Implementation Return Swap : New
        RLA, /* Return Leg Amount */

        /// <summary>
        /// /* Commodity Spot */
        /// </summary>
        FIL, /* Financial Leg */
        PHL, /* Physical Leg */

        /// <summary>
        /// /* Remove Trade */
        /// </summary>
        RMV,
        /// <summary>
        /// /* Shifting */
        /// </summary>
        SHI,
        /// <summary>
        /// /* Strategy product dates (virtual) */
        /// </summary>
        STG,
        /// <summary>
        ///  /* Swap underlyer */
        /// </summary>
        SWP,
        /// <summary>
        /// /* Term deposit */
        /// </summary>
        TED,
        /// <summary>
        /// /* Trade dates */
        /// </summary>
        TRD,
        /// <summary>
        /// /* UnclearingOffsetting */  
        /// </summary>
        UOF,
        /// <summary>
        /// /* UnderlyerValuationDate */  
        /// </summary>
        UVD,
        /// <summary>
        /// /* BasketValuationDate */  
        /// </summary>
        BVD,
        #endregion Group Level Trade Events

        #region Others
        /// <summary>
        /// Automatic abandon
        /// </summary>
        AAB,
        /// <summary>
        /// Automatic assignment
        /// </summary>
        AAS,
        /// <summary>
        /// Automatic exercise 
        /// </summary>
        AEX,
        /// <summary>
        /// Fallback exercise
        /// </summary>
        FEX,
        #endregion Others
    }
    #endregion EventCodeEnum
    #region EventOccurenceEnum
    public enum EventOccurenceEnum
    {
        All,
        AllExceptFirst,
        AllExceptLast,
        AllExceptFirstAndLast,
        First,
        FirstAndLast,
        Last,
        None,
        Unique,
        Item,
    }
    #endregion EventOccurenceEnum
    #region EventTypeEnum
    /// EG 20150302 Add BCU|BQU (CFD Forex)
    /// EG 20150319 (POC] New BWA
    /// PM 20150330 Add UST
    // EG 20190730 Upd (New HGA - Historical GAM for PositionOpening on DebtSecurityTransaction)
    // EG 20210812 [25173] New PCR et PRT (Prime)
    public enum EventTypeEnum
    {
        #region Administration Level Trade Events
        CAM,            /* Clean Amount */
        DAM,            /* Dirty Amount */
        BCU,            /* Base currency */
        QCU,            /* Quoted currency */
        BC1,            /* Base currency 1 */
        BC2,            /* Base currency 2 */
        BDP,            /* Bond Payment */
        BRO,			/* Brokerage */
        BWA,            /* BorrowingAmount */
        TBR,			/* Clearing Brokerage */
        CBR,			/* Trading Brokerage */
        CCU,            /* Call currency */
        CSH,            /* Cash settlement */
        CU1,            /* Currency 1 */
        CU2,            /* Currency 2 */
        DSA,            /* Debenture securities amounts */
        ELE,            /* Election settlement */
        ENT,            /* Option Entitlement */
        FAA,			/* FeeAccountingAmount */
        FWP,			/* ForwardPoints */
        FDA,            /* FundingAmount */
        FEE,			/* Fee */
        GAM,            /* Gross Amount */
        PAM,            /* Principal Amount FI 20151217 [21317] add PAM */
        RAM,            /* Redemption amount */
        HGA,            /* Historical gross amount */
        HPR,            /* Historical premium amount */
        HVM,            /* Historical variation margin amount */
        IMG,            /* Initial Margin */
        INT,			/* Interest */
        KNA,			/* Known amount */
        LOV,            /* Liquidation Option Value */
        MKV,			/* Market value */
        MKP,			/* Market value PAM */
        MKA,			/* Market value AIN */
        MGR,			/* Margin Requirement */
        TMG,			/* Total Margin */
        NOM,			/* Nominal */
        PAO,            /* Payout */
        PCU,            /* Put currency */
        PCR,            /* PremiumCashResidual */
        PRM,			/* Premium */
        PRT,            /* PremiumTheoretical */
        QTY,            /* Quantity */
        RSA,            /* Return Swap amount */
        REB,            /* Rebate */
        RMG,            /* Realized Margin */
        SCU,            /* Settlement currency */
        SEC,            /* Securities */
        UMG,            /* Unrealized Margin */
        VMG,            /* Variation Margin */
        // PM 20170911 [23408] New Equalisation Payment
        EQP,            /* Equalisation Payment */

        CSA,            /* Cash Available*/
        CSU,            /* Cash Used*/
        CLA,            /* Collateral Available*/
        CLU,            /* Collateral Used*/
        MGC,            /* Margin Call*/
        CSB,            /* Cash Balance*/
        UMR,            /* Uncovered Margin Requirement */
        PCB,            /* Previous Cash Balance */
        CBP,            /* Cash Balance Payment */

        CSD,            /* Cash Deposit */
        CSW,            /* Cash Withdrawal */
        E_B,            /* Equity Balance */
        E_D,            /* Excess/deficit */
        EBF,            /* Equity Balance With Forward Cash */
        EDF,            /* Excess/deficit With Forward Cash */
        FCP,            /* Forward Cash Payment */
        OVL,            /* Long Option Value */
        OVS,            /* Short Option Value */
        TAV,            /* Total Account Value */
        UST,            /* Unsettled transaction */

        // EG 20170206 [22787] new
        DVA,            /* Delivery Amount */
        // EG 20170424 [23064] new
        HDV,            /* Historical Delivery Amount */


        #region Arrêtés (CLO)
        AIN,            /* Accrual Interests */
        //
        ERA,            /* Effective rate amortization IFRS */
        FVL,            /* Fair value IFRS */
        MTM,            /* Mark to market */
        #endregion Arrêtés (CLO)

        #endregion Administration Level Trade Events
        #region Calculation Level Trade Events
        CAB,			/* Cap bought */
        CAS,			/* Cap sold */
        FIX,			/* Fixed rate */
        FLB,			/* Floor bought */
        FLO,			/* Floating rate */
        FLS,			/* Floor sold */
        FXC,			/* Fx Calculation Agent (anciennement: Fx customer/client) */
        FX1,			/* Fx rate 1 */
        FX2,			/* Fx rate 2 */
        FXR,			/* Fx rate */
        ZEC,            /* Zero coupon */
        #endregion Calculation Level Trade Events
        #region Description Level Trade Events
        #region CommodityClass 
        ELC,            /* Electricity physical */
        ENV,            /* Environmental Physical */
        GAS,            /* Gas physical */
        #endregion CommodityClass
        #region Exercise dates
        AME,            /* American Exercise */
        BRM,            /* Bermuda Exercise */
        EUR,            /* European Exercise */
        #endregion Exercise dates
        #region Triggers (TRG eventCode)
        ABO,			/* Above */
        BEL,			/* Below */
        DWN,			/* Down No touch */
        DWT,			/* Down Touch */
        UPN,			/* Up No touch */
        UPT,			/* Up Touch */
        #endregion Triggers (TRG eventCode)
        #region Effective rate (EFR eventCode)
        LOD,			/* Loan or deposit */
        SPD,			/* Securities premium-discount */
        #endregion  Effective rate (EFR eventCode)
        #endregion Description Level Trade Events
        #region Group Level Trade Events
        #region Call / Put / Future
        CAL,
        PUT,
        FUT,
        #endregion Call / Put
        #region Cap/Floor/Collar
        CAP,			/* Cap */
        //20090827 PL COL --> MCF
        //COL,			/* Collar */
        MCF,			/* Collar,Straddle,Strangle,Corridor,Exotic */
        FLR,			/* Floor */
        #endregion Cap/Floor/Collar
        #region Trade/Product (TRD/PRD eventCodes)
        DAT,			/* Dates */
        #endregion Trade/Product (TRD/PRD eventCodes)
        #region Exercise/Abandon (EXE/ABN eventCodes)
        MUL,            /* Multiple */
        PAR,            /* Partial */
        TOT,			/* Total */
        #endregion Exercise/Abandon (EXE/ABN eventCodes)
        #region Optional provisions (PRO eventCode)
        CAN,			/* Cancelable provision */
        EXT,			/* Extendible provision */
        MET,			/* Mandatory early termination provision */
        OET,			/* Optional early termination provision */
        SUP,			/* Step-Up provision */
        #endregion Optional provisions (PRO eventCode)
        #region Non detailed instruments
        BUL,			/* Bullet payment */
        FRA,			/* Future rate agreement */
        IRS,			/* Interest rate swap */
        TED,			/* Term deposit */
        #endregion Non detailed instruments
        #region ReturnSwapTransaction Type
        #endregion ReturnSwapTransaction Type
        #region ReturnSwap(Leg type)
        TRL,			/* TotalReturnLeg */
        PRL,			/* PriceReturnLeg */
        DRL,			/* DividendReturnLeg */
        OPN,            /* Open */
        TRM,            /* Term */
        INL,            /* InterestLeg */
        MGF,            /* MarginRequirementRatio (Factor)*/
        #endregion ReturnSwap(Leg type)

        #region FX Spot/Forward/Swap
        DLV,			/* Deliverable */
        NDV,			/* Non deliverable */
        #endregion FX Spot/Forward/Swap
        #region FX American digital option
        DLE,			/* Double Touch Limit Extinguishing */
        DLR,			/* Double Touch Limit Resurrecting */
        DNE,			/* Double No Touch Extinguishing */
        DNR,			/* Double No Touch Resurrecting */
        DNT,			/* Double No Touch */
        DTB,			/* Double Touch Boundary */
        DTC,			/* Double Touch */
        DTL,			/* Double Touch Limit */
        NTC,			/* No Touch */
        NKI,            /* No Touch Barrier KnockIn */
        NKO,            /* No Touch Barrier KnockOut */
        TCH,			/* Touch */
        TKI,             /* Touch Barrier KnockIn */
        TKO,             /* Touch Barrier KnockOut */
        #endregion FX American digital option
        #region FX European digital option
        ABA,			/* Above Asset or Nothing */
        ABE,			/* Above Extinguishing */
        ABG,			/* Above Gap */
        ABR,			/* Above Resurrecting */
        AKI,			/* Above Barrier KnockIn */
        AKO,			/* Above Barriers KnockOut */
        BEA,			/* Below Asset or Nothing */
        BEE,			/* Below Extinguishing */
        BEG,			/* Below Gap */
        BER,            /* Below resurrecting */
        BKI,			/* Below Barrier KnockIn */
        BKO,			/* Below Barrier KnockOut */
        RNG,			/* Range (Above and Below) */
        #endregion FX European digital option
        #region FX Average option
        ARA,            /* Rate */
        AST,            /* Strike */
        #endregion FX Average option
        #region FX Barrier option
        CAC,            /* Capped call */
        DWI,            /* Down In */
        DWO,            /* Down Out */
        FLP,            /* Floored put */
        KNI,            /* Knock in */
        KNO,            /* Knock out */
        RDI,            /* Rebate Down In  */
        RDO,            /* Rebate Down Out */
        RUI,            /* Rebate Up In */
        RUO,            /* Rebate Up Out */
        UPI,            /* Up In */
        UPO,            /* Up Out */
        #endregion FX Barrier option
        #region FX Simple option
        ASO,            /* Fx American Simple option */
        BSO,            /* Fx Bermuda Simple option */
        ESO,            /* Fx European Simple option */
        #endregion FX Simple option
        #region Equity
        AVI,            /* Average In */
        AVO,            /* Average Out */
        BND,            /* Bond */
        CBD,            /* ConvertibleBond */
        BSK,            /* Basket */
        FEA,            /* Features */
        IND,            /* Index */
        LBI,            /* LookBack In */
        LBO,            /* LookBack Out */
        SHR,            /* Share */
        SUL,            /* Single Underlyer */
        UNL,            /* Underlyer */
        VAN,            /* Vanilla */
        #endregion Equity
        #region Swaption
        SGL,            /* Strangle */
        SDL,            /* Straddle */
        #endregion Swaption
        #endregion Group Level Trade Events

        #region Invoice
        AMT,            /* Amounts */
        BRB,            /* Bracket rebate */
        CRB,            /* Cap rebate */
        CUM,            /* Cumulative */
        DRC,            /* Detail Rebate Cumulative */
        DRU,            /* Detail Rebate Unit */
        FXL,            /* Foreign Exchange Loss */
        FXP,            /* Foreign Exchange Profit */
        GRB,            /* Global rebate */
        GTO,            /* Gross TurnOver Amount */
        NAL,            /* Non allocated amount */
        NTA,            /* Net TurnOver Accounting Amount */
        NTI,            /* Net TurnOver Issue Amount */
        NTO,            /* Net TurnOver Amount */
        PER,            /* Period */
        VAT,            /* ValueAddedTax */
        TXO,            /* Tax amount */
        TXI,            /* Tax issue amount */
        TXA,            /* Tax accounting amount */
        #endregion Invoice

        #region Others
        PHY,            /* Physical settlement */
        FWD,            /* Forward */
        REG,            /* REGULAR */
        O_N,            /* Overnight */
        #endregion Others
    }
    #endregion EventTypeEnum
    #region ExchangeCurrencyPosEnum
    public enum EFS_ExchangeCurrencyPosEnum
    {
        ExchangeCurrency1,
        ExchangeCurrency2,
    }
    #endregion ExchangeCurrencyPosEnum
    #region ExchangeTypeEnum
    public enum ExchangeTypeEnum
    {
        /// <summary>
        /// Flow Currency
        /// </summary>
        FCU,
        /// <summary>
        /// Account Currency - Ear date fixing
        /// </summary>
        ACU_EARDATE,
        /// <summary>
        /// Account Currency - Event date fixing
        /// </summary>
        ACU_EVENTDATE,
        /// <summary>
        /// Account Currency - Transaction date fixing
        /// </summary>
        ACU_TRANSACTDATE,   /* Account Currency - */
        /// <summary>
        /// Account Currency - PRS EventDate fixing
        /// </summary>
        ACU_PRSDATE,
        /// <summary>
        /// Account Currency - Value date fixing
        /// </summary>
        ACU_VALUEDATE,

        /// <summary>
        ///  Currency 1 - Ear date fixing
        /// </summary>
        CU1_EARDATE,        /* Currency 1 - Ear date fixing*/
        /// <summary>
        ///  Currency 1 - Event date fixing
        /// </summary>
        CU1_EVENTDATE,
        /// <summary>
        ///  Currency 1 - Transaction date fixing
        /// </summary>
        CU1_TRANSACTDATE,
        /// <summary>
        ///  Currency 1 - PRS EventDate fixing
        /// </summary>
        CU1_PRSDATE,
        /// <summary>
        ///  Currency 1 - Value date fixing
        /// </summary>
        CU1_VALUEDATE,
        /// <summary>
        ///  Currency 2 - Ear date fixing
        /// </summary>
        CU2_EARDATE,
        /// <summary>
        ///  Currency 2 - Event date fixing
        /// </summary>
        CU2_EVENTDATE,
        /// <summary>
        ///  Currency 2 - Transaction date fixing
        /// </summary>
        CU2_TRANSACTDATE,
        /// <summary>
        ///  Currency 2 - PRS EventDate fixing
        /// </summary>
        CU2_PRSDATE,
        /// <summary>
        ///  Currency 2 - Value date fixing
        /// </summary>
        CU2_VALUEDATE,
    }
    #endregion ExchangeTypeEnum
    #region CBExchangeTypeEnum
    public enum CBExchangeTypeEnum
    {
        FCU,			    /* Flow Currency */
        ACU_BUSINESSDATE,   /* Account Currency - Business date fixing*/
    }
    #endregion CBExchangeTypeEnum
    #region ExerciseTypeEnum
    public enum ExerciseTypeEnum
    {
        TOT,
        PAR,
        MUL,
    }
    #endregion ExerciseTypeEnum

    #region FeatureFormulaEnum
    public enum FeatureFormulaEnum
    {
        Formula1,
        Formula2,
        Formula3,
        Formula4,
        Formula5,
    }
    #endregion FeatureFormulaEnum
    #region FeeExchangeTypeEnum
    public enum FeeExchangeTypeEnum
    {
        DayBeforeEventDate,
        DayBeforeTransactDate,
        EventDate,
        PrsDate,
        TransactDate,
        ValueDate,
        SpotRate,
        ExchangeRate
    };
    #endregion FeeExchangeTypeEnum
    #region FeeFormulaEnum
    public enum FeeFormulaEnum
    {
        //PL 20141017 
        /// <summary>
        /// Montant fixe        f()=@R1 [+@R2]
        /// </summary>
        CONST,
        /// <summary>
        /// Taux par Assiette par Fraction de jours        f()=@R1*@A1*@DCF [+@R2]
        /// </summary>
        F1,
        /// <summary>
        /// Courtage actualisé (Méthode standard)        f()=DiscountBrokerage()
        /// </summary>
        F2_V1,
        /// <summary>
        /// Courtage actualisé (Méthode du CIC-Paris)        f()=DiscountBrokerage(CIC-Paris)
        /// </summary>
        F2_V2,
        /// <summary>
        /// Courtage actualisé (Méthode de BNP-Paribas)        f()=DiscountBrokerage(BNP-Paribas)
        /// </summary>
        F2_V3,
        /// <summary>
        /// Montant par Millier d'assiette        f()=@R1*@A1/1000 [+@R2]
        /// </summary>
        F3KO,
        /// <summary>
        /// Montant par Millier d'assiette par Nombre de jours        f()=@R1*@A1/1000*@DC [+@R2]
        /// </summary>
        F3KOD,
        /// <summary>
        /// Montant par Million d'assiette        f()=@R1*@A1/1000000 [+@R2]
        /// </summary>
        F3MO,
        /// <summary>
        /// Montant par Million d'assiette par Nombre de jours        f()=@R1*@A1/1000000*@DC [+@R2]
        /// </summary>
        F3MOD,
        /// <summary>
        /// Montant par Unité d'assiette        f()=@R1*@A1 [+@R2]
        /// </summary>
        F4,
        /// <summary>
        /// Montant par Quantité        f()=@R1*@A1
        /// </summary>
        F4QTY,
        /// <summary>
        /// Pourcentage de la Prime        f()=@R1*@A1
        /// </summary>
        F4PRM,
        /// <summary>
        /// Pourcentage du Strike        f()=@R1*@A1
        /// </summary>
        F4STK,
        /// <summary>
        /// Centimes par Quantité        f()=@R1*@A1
        /// </summary>
        F4CPS,
        /// <summary>
        /// Points de base du Notionnel        f()=@R1*@A1
        /// </summary>
        F4BPS,
        /// <summary>
        /// Centimes par Quantité + Points de base du Notionnel        f()=@R1*@A1 + @R2*@A2
        /// </summary>
        F5CPSBPS,
        /// <summary>
        /// XML (Réservé à un usage ultérieur)
        /// </summary>
        XML,
        /// <summary>
        /// C#
        /// </summary>
        CSHARP,
    };
    #endregion FeeFormulaEnum
    #region FeePayRelativeToEnum
    public enum FeePayRelativeToEnum
    {
        TransactDate,
        EffectiveDate,
        BusinessDate,
        NextBusinessDate,
        DeliveryDate,
        ValueDate,
        TerminationDate
    };
    #endregion FeePayRelativeToEnum
    #region FinalSettlementPriceEnum
    /// <summary>
    /// Détermination de la date de lecture du Prix de référence pour les dénouement automatique à l'échéance
    /// </summary>
    /// EG 20130603 Ticket: 18721
    public enum FinalSettlementPriceEnum
    {
        ExpiryDate,
        LastTradingDay,
    }
    #endregion FinalSettlementPriceEnum
    #region FinancialInstrumentProductTypeCodeEnum
    public enum FinancialInstrumentProductTypeCodeEnum
    {
        ABBO,
        AMEN,
        ANNO,
        APOL,
        BAAP,
        BANT,
        BIOX,
        BRAD,
        BRID,
        CALN,
        CEOD,
        CMBS,
        CMOS,
        COFO,
        COFP,
        COMM,
        CONV,
        COPR,
        CPPE,
        DENT,
        DFLT,
        DINP,
        DUAL,
        EXCN,
        FEAC,
        FEAD,
        FXCO,
        GOBO,
        IETM,
        LINT,
        LOFC,
        MATU,
        METN,
        MFUN,
        MIPT,
        MLEG,
        MOBS,
        MOIO,
        MOPO,
        MPRP,
        MTEN,
        NONE,
        OVNT,
        PREF,
        PRIV,
        PRNT,
        RANO,
        REAM,
        REPL,
        RERA,
        RETR,
        REVB,
        RTLV,
        RVLV,
        SHTN,
        SPCA,
        SPCO,
        SPCT,
        STRU,
        SWIN,
        TANO,
        TAXA,
        TBON,
        TCAL,
        TECP,
        TERM,
        TIDE,
        TINT,
        TIPS,
        TOBA,
        TPRN,
        TRAN,
        USTB,
        USTN,
        VRDN,
        WARR,
        WITH,
        XLIN,
        YANK,
    }
    #endregion FinancialInstrumentProductTypeCodeEnum
    #region FlowTypeEnum
    public enum FlowTypeEnum
    {
        PAY,            /* Paid */
        REC,            /* Received */
    }
    #endregion FlowTypeEnum
    #region FxOptionTypeEnum
    public enum FxOptionTypeEnum
    {
        None,
        AverageRate_Geometric,
        AverageRate_Arithmetic,
        AverageStrike_Geometric,
        AverageStrike_Arithmetic,
        BarrierOption,
        CappedCallBarrierOption,
        FloorPutBarrierOption,
        SimpleOption,
    }
    #endregion FxOptionTypeEnum
    #region FxTypeEnum
    public enum FxTypeEnum
    {
        FxLinkedNotional,
        DeliverableFxLeg,
        DeliverableFxLeg_Fixing,
        DeliverableFxSwap,
        NDF,
        NDF_Bridge,
        NDF_ThirdSettlementCurrency,
        NDF_Bridge_ThirdSettlementCurrency,
        ContractForDifference,
        Unknown,
    }
    #endregion FxTypeEnum
    #region FullCouponCalculationMethodEnum
    public enum FullCouponCalculationMethodEnum
    {
        CapitalPosition,
        UnitCoupon,
    }
    #endregion FullCouponCalculationMethodEnum
    #region FungibilityModeEnum
    public enum FungibilityModeEnum
    {
        CLOSE,
        OPENCLOSE,
        NONE,
    }
    #endregion FungibilityModeEnum

    #region GlobalElementaryEnum
    /// <summary>
    /// 
    /// </summary>
    public enum GlobalElementaryEnum
    {
        Elementary,
        Global,
        Full,
    }
    #endregion GlobalElementaryEnum

    #region InitialMarginDeliveryStepEnum
    /// <summary>
    /// Type de calcul du déposit de livraison
    /// </summary>
    // PM 20190401 [24625][24387] Ajout Expiry
    public enum InitialMarginDeliveryStepEnum
    {
        [XmlEnum("NA")]
        NA,
        [XmlEnum("PreDelivery")]
        PreDelivery,
        [XmlEnum("Delivery")]
        Delivery,
        [XmlEnum("InvoicingInTwoSteps")]
        InvoicingInTwoSteps,
        [XmlEnum("Expiry")]
        Expiry,
    }
    #endregion InitialMarginDeliveryStepEnum
    #region IndexationFormulaEnum
    /// <summary>
    /// 
    /// </summary>
    public enum IndexationFormulaEnum
    {
        Barrier,
        BestOf,
        BestOfWithProtectedAverage,
        Binary,
        Capped,
        Corridor,
        FxBarrier,
        FxCorridor,
        FxSimple,
        InFine,
        Landing,
        LookBack,
        Pawls,
        ProtectedAverage,
        Reverse,
        SimpleAverage,
        SuperAverage,
    }
    #endregion IndexationFormulaEnum
    #region InitialMarginMethodEnum
    /// <summary>
    /// Représente les méthode de calcul de deposit
    /// </summary>
    public enum InitialMarginMethodEnum
    {
        CBOE_Margin,
        /// <summary>
        /// Custom (ex-Standard) method
        /// </summary>
        Custom,
        MEFFCOM2,
        /// <summary>
        /// Méthode Prisma, utilisée par EUREX Clearing AG
        /// </summary>
        EUREX_PRISMA,
        London_SPAN,
        /// <summary>
        /// Spot Initial Margin method (used by ECC on Commodity Spot contract)
        /// </summary>
        IMSM,
        /// <summary>
        /// Method used by NASDAQ OMX
        /// Calcul des déposits pour les contrats sur Bonds du NASDAQ OMX (CFM: Cash Flow Margin))
        /// </summary>
        /// PM 20190222 [24326] Ajout NOMX_CFM
        NOMX_CFM,
        /// <summary>
        /// Method used by OMX_NORDIC
        /// Calcul des déposits sur OMX (Riva Capital at Risk (RIVA pour RISK VALUATION))
        /// </summary>
        OMX_RCAR,
        SPAN_C21,
        SPAN_CME,
        // PM 20220111 [25617] Ajout méthode SPAN 2, nouvelle méthode de CME Clearing
        // (SPAN_2_CORE pour CME CORE Margin Service API)
        SPAN_2_CORE,
        // (SPAN_2_SOFTWARE pour CME Deployable Margin Software)
        SPAN_2_SOFTWARE,
        /// <summary>
        /// TIMS method, as used by the EUREX clearing AG (EUREX market)
        /// </summary>
        TIMS_EUREX,
        /// <summary>
        /// TIMS method, as used by the CCG italian clearing house (IDEM/IDEX markets)
        /// </summary>
        TIMS_IDEM,
        /// <summary>
        /// Pour le calcul du déposit de Livrairon
        /// </summary>
        Delivery,
        /// <summary>
        /// Valeur pour aucun calcul de déposit
        /// </summary>
        // PM 20221212 [XXXXX] Ajout NOMARGIN pour aucun calcul
        NOMARGIN,
        /// <summary>
        /// Euronext Var Based Method 
        /// </summary>
        EURONEXT_VAR_BASED,
        /// <summary>
        /// Euronext Legacy Var Based Method 
        /// </summary>
        // PM 20240423 [XXXXX] Ajout EURONEXT_LEGACY_VAR
        EURONEXT_LEGACY_VAR,
    }
    #endregion InitialMarginMethodEnum
    #region InterestAmountTypeEnum
    /// <summary>
    /// Type de montant sur lequel peut être calculé des intérêts
    /// </summary>
    public enum InterestAmountTypeEnum
    {
      CashBalance,
      CreditCashBalance,
      DebitCashBalance,
      CashCoveredInitialMargin,
    }
    #endregion InterestAmountTypeEnum
    #region InterfaceEnum
    public enum InterfaceEnum
    {
        IActualPrice,
        IAdjustableDate,
        IAdjustableDates,
        IAdjustableOrRelativeDate,
        IAdjustableOrRelativeDates,
        IAdjustableDateOrRelativeDateSequence,
        IAdjustableRelativeOrPeriodicDates,
        IAdjustableRelativeOrPeriodicDates2,
        IAdjustedDate,
        IAdjustedRelativeDateOffset,
        IAmericanExercise,
        IAmountSchedule,
        IAssetOrNothing,
        IAutomaticExercise,
        IAverageStrikeOption,

        IBasket,
        IBasketConstituent,
        IBermudaExercise,
        IBookId,
        IBulletPayment,
        IBusinessCenter,
        IBusinessCenters,
        IBusinessCenterTime,
        IBusinessDateRange,
        IBusinessDayAdjustments,

        ICalculation,
        ICalculationPeriodAmount,
        ICalculationPeriodDates,
        ICalculationPeriodFrequency,
        ICancelableProvision,
        ICapFloor,
        ICappedCallOrFlooredPut,
        ICashSettlement,
        ICashSettlementPaymentDate,
        ICommission,
        ICurrency,
        ICustomerSettlementPayment,

        IDataDocument,
        IDateList,
        IDateOffset,
        IDiscounting,
        IDocument,

        IEarlyTerminationProvision,
        IEFS_AdjustableDate,
        IEFS_DayCountFraction,
        IEFS_ExchangeRate,
        IEFS_IntervalPeriods,
        IEFS_Step,

        IEfsDocument,
        IEfsSettlementInstruction,
        IEquityAsset,
        IEuropeanExercise,
        IExchangeRate,
        IExchangeTradedBase, // FI 20180423 [23924]  Add
        IExchangeTradedDerivative,

        IExerciseProcedure,
        IExpiryDateTime,
        IExtendibleProvision,

        IFloatingRate,
        IFloatingRateCalculation,
        IFlowContext,
        IFra,
        IFxAmericanTrigger,
        IFxAverageRateObservationDate,
        IFxAverageRateObservationSchedule,
        IFxAverageRateOption,
        IFxBarrier,
        IFxBarrierOption,
        IFxCashSettlement,
        IFxClass,
        IFxDigitalOption,
        IFxEuropeanTrigger,
        IFxFixing,
        IFxLeg,
        IFxLinkedNotionalSchedule,
        IFxOptionBase,
        IFxOptionLeg,
        IFxOptionPayout,
        IFxOptionPremium,
        IFxSpotRateSource,
        IFxStrikePrice,
        IFxSwap,

        IHedgeClassDerv,
        IHedgeClassNDrv,
        IHedgeFactor,
        IHedgeFolder,
        IHourMinuteTime,

        IIASClassDerv,
        IIASClassNDrv,
        IIndex,
        IInflationRateCalculation,
        IInformationSource,
        IInterestLeg,
        IInterestLegCalculationPeriodDates,
        IInterestLegResetDates,
        IInterestRateStream,
        IInterval,

        IKnownAmountSchedule,

        ILocalClassDerv,
        ILocalClassNDrv,

        IManualExercise,
        IMandatoryEarlyTermination,
        IMandatoryEarlyTerminationAdjustedDates,
        IMarginDetailsDocument,
        IMoney,

        INotional,
        INotionalStepRule,

        IObservedRates,
        IOffset,
        IOptionalEarlyTermination,

        IParty,
        IPartyTradeIdentifier,
        IPartyTradeInformation,
        IPayment,
        IPaymentDates,
        IPayoutPeriod,
        /// EG 20161122 New Commodity Derivative
        IPhysicalQuantity,
        IPrincipalExchanges,
        IPrincipalExchangeAmount,
        IPrincipalExchangeDescriptions,
        IPrincipalExchangeFeatures,
        IProduct,
        IProductBase,
        IProvision,

        IQuotedCurrencyPair,

        IReference,
        IRelativeDateOffset,
        IRelativeDates,
        IRelativeDateSequence,
        IRequiredIdentifierDate,
        IResetDates,
        IResetFrequency,
        IReturnLeg,
        IReturnLegValuation,
        IReturnLegValuationPrice,
        IReturnSwapBase,
        IReturnSwap,
        IReturnSwapLeg,
        IReturnSwapNotional,
        IReturnSwapPaymentDates,
        IRounding,
        IRouting,
        IRoutingExplicitDetails,
        IRoutingId,
        IRoutingIds,
        IRoutingIdsAndExplicitDetails,

        IScheme,
        ISchedule,
        ISettlementMessageDocument,
        ISideRate,
        ISideRates,
        ISingleUnderlyer,
        ISpreadSchedule,
        IStepUpProvision,
        IStrategy,
        IStrikeSchedule,
        IStub,
        IStubCalculationPeriod,
        IStubCalculationPeriodAmount,
        ISwap,
        ISwaption,

        ITermDeposit,
        ITrade,
        ITradeDate,
        ITradeHeader,
        ITrader,

        IUnderlyer,
        IUnderlyingAsset,

        IVarianceLeg,
    }
    #endregion InterfaceEnum
    #region InterpolationMethodEnum
    public enum InterpolationMethodEnum
    {
        Linear,
        LinearCubic,
        LinearExponential,
        LinearQuadratic,
        LinearLogarithmic,
        Polynomial,
        CubicSpline,
        CubicBSpline,
    }
    #endregion InterpolationMethodEnum
    #region InterpolationTypeEnum
    public enum InterpolationTypeEnum
    {
        Extrapolated,
        Interpolated,
        InterpolatedExtrapolated,
    }
    #endregion InterpolationTypeEnum
    #region IntentionEnum
    public enum IntentionEnum
    {
        Initiator,
        Reactor,
    }
    #endregion IntentionEnum
    
    /// <summary>
    ///  In the money condition of "At-the-money" option
    /// </summary>
    /// FI 20170303 [22916] Add
    public enum ITMConditionEnum
    {
        /// <summary>
        /// At-the-money is out-of-the-money – Exactly at-the-money is treated as out-of-the-money for both calls and puts.  
        /// <para>
        /// This is the current behavior for all CME-cleared options. 
        /// </para>
        /// </summary>
        [System.Xml.Serialization.XmlEnumAttribute("0")]
        OTM,
        /// <summary>
        /// At-the-money is in-the-money – Exactly at-the-money is treated as in-the-money for both calls and puts. 
        /// </summary>
        [System.Xml.Serialization.XmlEnumAttribute("1")]
        ITM,
        /// <summary>
        /// At-the-money call is in-the-money 
        /// <para>
        /// An exactly at-the-money call is treated as in-the-money at expiration, while an exactly out-of-the-money put is treated as out-of-the-money.  
        /// </para>
        /// </summary>
        [System.Xml.Serialization.XmlEnumAttribute("2")]
        ITMCall,
        /// <summary>
        /// At-the-money put is in-the-money 
        /// <para>
        /// An exactly at-the-money put is treated as in-the-money at expiration, while an exactly at-the-money call is treated as out-of-the-money
        /// </para>
        /// </summary>
        [System.Xml.Serialization.XmlEnumAttribute("3")]
        ITMPut = 3
    }


    #region InvoiceApplicationPeriodEnum
    public enum InvoiceApplicationPeriodEnum
    {
        Invoice,
        CurrentMonth,
        CurrentQuarter,
        CurrentHalfYear,
        CurrentYear,
    }
    #endregion InvoiceApplicationPeriodEnum
    #region InvoicingGenMethodEnum
    public enum InvoicingGenMethodEnum
    {
        Simulation,
        Definitive,
        Replace,
    }
    #endregion InvoicingGenMethodEnum
    #region InvoiceSettlementDelayRelativeToEnum
    public enum InvoiceSettlementDelayRelativeToEnum
    {
        invoiceDate,
        tradeDate,
    }
    #endregion InvoiceSettlementDelayRelativeToEnum
    #region InvoicingSortEnum
    public enum InvoicingSortEnum
    {
        TRADE_INSTRUMENT,
        UNDERLYER_INSTRUMENT,
        TRADE_AMOUNTCURRENCY,
        CLIENT_TRADER,
        CLIENT_SALES,
        ISIN_CODE,
        MARKET,
        DERIVATIVECONTRACT,
        IDFEESCHEDULE,
    };
    #endregion InvoicingSortEnum
    #region InvoicingSortEnum
    public enum ReportSortEnum
    {
        TRADE_INSTRUMENT,
        UNDERLYER_INSTRUMENT,
        TRADE_AMOUNTCURRENCY,
        CLIENT_TRADER,
        CLIENT_SALES,
        ISIN_CODE,
        MARKET,
        DERIVATIVECONTRACT,
        IDFEESCHEDULE,
    };
    #endregion InvoicingSortEnum
    #region InvoicingTradeDetailEnum
    public enum InvoicingTradeDetailEnum
    {
        header,
        product,
        otherPartyPayment,
        extends,
        //20110622 FI add extlLink
        extlLink,
    };
    #endregion InvoicingTradeDetailEnum

    #region KnockConditionEnum
    public enum KnockConditionEnum
    {
        UpAndIn,
        UpAndOut,
        DownAndIn,
        DownAndOut,
    }
    #endregion KnockConditionEnum
    #region KnockEffectTypeEnum
    public enum KnockEffectTypeEnum
    {
        CalculationPeriod,
        InterestRateStream,
        PaymentPeriod,
        FullTrade,
    }
    #endregion KnockEffectTypeEnum
    #region KnockObservationTypeEnum
    public enum KnockObservationTypeEnum
    {
        CalculationPeriod,
        CalculationPeriodEndDate,
        PaymentPeriod,
        PaymentPeriodEndDate,
        PreviousCalculationPeriod,
        PreviousCalculationPeriodEndDate,
        PreviousPaymentPeriod,
        PreviousPaymentPeriodEndDate,
        SpecifiedPeriods,
        SpecifiedDates,
    }
    #endregion KnockObservationTypeEnum

    #region LoadDataSetSettlementMessage
    public enum LoadDataSetSettlementMessage
    {
        LoadESR,
        LoadMSO,
        LoadESRAndMSO,
    }
    #endregion LoadTypeEnum

    #region MandatoryOptionalEnum
    public enum MandatoryOptionalEnum
    {
        MANDATORY,
        OPTIONAL,
        NONE
    }
    #endregion
    #region MarginTypeEnum
    public enum MarginTypeEnum
    {
        Cash,
        Instrument,
    }
    #endregion MarginTypeEnum
    #region MarginWeightedRiskCalculationMethodEnum
    // PM 20161003 [22436] Add method ScanRange
    public enum MarginWeightedRiskCalculationMethodEnum
    {
        Capped,
        Normal,
        ScanRange,
    }
    #endregion MarginWeightedRiskCalculationMethodEnum

    #region MarketDisruptionEnum
    public enum MarketDisruptionEnum
    {
        FailureToOpen,
        TradingDisruption,
        ExchangeDisruption,
        EarlyClosing,
    }
    #endregion MarketDisruptionEnum
    #region MaturityValueEnum
    public enum MaturityValueEnum
    {
        IDENTICAL,
        PREVIOUS,
        NEXT,
        LOWER,
        HIGHER,
        DIFFERENT
    }
    #endregion
    #region MaturityDateTypeEnum
    // RD 20110518 
    //public enum MaturityDateTypeEnum
    //{
    //    MaturityDate,
    //    DeliveryDate,
    //    LastTradingDay,
    //    FirstNoticeDay,
    //    LastNoticeDay
    //}
    #endregion
    #region MatrixStructureTypeEnum
    public enum MatrixStructureTypeEnum
    {
        Expiration,
        Generic,
        Strike,
        Term,
    }
    #endregion MatrixStructureTypeEnum
    #region MatrixInterpolationMethodEnum
    public enum MatrixInterpolationMethodEnum
    {
        Linear,
        BiLinear,
        BiCubicPolynomial,
        BiCubicSpline,
        TriLinear,
        NearestNeighbor,
    }
    #endregion MatrixInterpolationMethodEnum

    #region MatchingEnum
    public enum MatchingEnum
    {
        #region Members
        HiMatch,
        LoMatch,
        UnMatch,
        Ignore,
        #endregion Members
    }
    #endregion MatchingEnum


    #region NbMaxValuesTypeEnum
    public enum NbMaxValuesTypeEnum
    {
        Crescent,
        CrescentAndSequential,
        DeCrescent,
        DeCrescentAndSequential,
        Sequential,
        NonSequential,
    }
    #endregion NbMaxValuesTypeEnum

    #region NettingMethodEnum
    /// <summary>
    ///  Netting 
    /// </summary>
    public enum NettingMethodEnum
    {
        /// <summary>
        /// 
        /// </summary>
        Standard,
        /// <summary>
        /// 
        /// </summary>
        Convention,
        /// <summary>
        /// 
        /// </summary>
        Designation,
        None
    }
    #endregion NettingMethodEnum
    #region NotificationSendToClass
    /// <summary>
    /// Type de destinataire 
    /// </summary>
    public enum NotificationSendToClass
    {
        /// <summary>
        /// Client
        /// </summary>
        Client,
        /// <summary>
        /// Donneur d'ordre Maison
        /// </summary>
        Entity,
        /// <summary>
        /// Contrepartie Externe
        /// </summary>
        External,
    }
    #endregion NotificationSendToClass
    #region NotificationClassEnum
    /// <summary>
    /// Classe (Catégorie) de notification
    /// <para>MONOTRADE,MULTITRADES,MULTIPARTIES</para>
    /// </summary>
    public enum NotificationClassEnum
    {
        /// <summary>
        /// Confirmation qui se rapporte à un seul trade 
        /// </summary>
        [System.Xml.Serialization.XmlEnumAttribute("MONO-TRADE")]
        MONOTRADE,
        /// <summary>
        ///  Edition qui se rapporte à un seul book (contient plusieurs trades)
        ///  <para>Edition simple</para>
        /// </summary>
        [System.Xml.Serialization.XmlEnumAttribute("MULTI-TRADES")]
        MULTITRADES,
        /// <summary>
        ///  Edition qui se rapporte à plusieurs books (contient plusieurs trades)
        ///  <para>Edition consolidée</para>
        /// </summary>
        [System.Xml.Serialization.XmlEnumAttribute("MULTI-PARTIES")]
        MULTIPARTIES
    }
    #endregion NotificationClassEnum 
    #region NotificationTypeEnum
    /// <summary>
    /// Type de notification
    /// <para>ALLOCATION,POSITION,FINANCIAL,SWIFT,ISDA,etc</para>
    /// </summary>
    public enum NotificationTypeEnum
    {
        /// <summary>
        /// Opérations du jour (Avis d'opéré)
        /// </summary>
        ALLOCATION,
        /// <summary>
        /// 
        /// </summary>
        EXEASSOPT,
        /// <summary>
        /// Financial Report (Situation financière) 
        /// </summary>
        FINANCIAL,
        /// <summary>
        /// Financial Report Periodique (Extrait de compte) 
        /// </summary>
        /// FI 20120731 [18048] add FINANCIALPERIODIC
        FINANCIALPERIODIC,
        /// <summary>
        /// Confirmation Isda
        /// </summary>
        ISDA,
        /// <summary>
        /// Position Action Report (Actions sur positions)
        /// </summary>
        POSACTION,
        /// <summary>
        /// Position Report (Positions ouvertes détaillées)
        /// </summary>
        POSITION,
        /// <summary>
        /// Synthetic Position Report (Positions ouvertes synthétiques)
        /// </summary>
        POSSYNTHETIC,
        /// <summary>
        /// Confirmation Swift
        /// </summary>
        SWIFT,
        /// <summary>
        /// Etat de synthèse 
        /// <para>Etat récapitulatif avec les avis d'opéré, les positions, les actions sur position, etc... </para>
        /// <para></para>
        /// </summary>
        /// FI 20130612 [18745]
        SYNTHESIS,
        /// <summary>
        /// Confirmation des trades alloc d'un ordre
        /// </summary>
        /// FI 20190515 [23912] Add
        ORDERALLOC,
    };
    #endregion NotificationTypeEnum
    #region NotificationMultiPartiesEnum
    /// <summary>
    /// type de message Multi parties
    /// </summary>
    public enum NotificationMultiPartiesEnum
    {
        /// <summary>
        /// Pour un acteur donné, le message regroupe toute l'activité des books dont il est propriétaire
        /// </summary>
        OWN,
        /// <summary>
        /// Pour un acteur donné, le message regroupe toute l'activité des books dont les proriétaires sont des acteurs enfants
        /// </summary>
        CHILD,
        /// <summary>
        /// Pour un acteur donné, le message regroupe toute l'activité des books dont il est propriétaire et toute l'activité des books dont les proriétaires sont des acteurs enfants
        /// </summary>
        ALL
    }
    #endregion NotificationMultiPartiesEnum
    #region NotificationAccessKeyEnum
    /// <summary>
    /// 
    /// </summary>
    public enum NotificationAccessKeyEnum
    {
        TRADE
    };
    #endregion NotificationAccessKeyEnum
    #region NotificationStepLifeEnum
    /// <summary>
    /// 
    /// </summary>
    public enum NotificationStepLifeEnum
    {
        /// <summary>
        ///  Notification de mise en place des trades
        /// </summary>
        INITIAL,
        /// <summary>
        ///  Notification prévue pour les périodes intermédiares d'un trade (Exeemple Tombée d'intérêt, exercice d'une option)
        /// </summary>
        INTERMEDIARY,
        /// <summary>
        /// Notification de fin de vie d'un trade
        /// </summary>
        FINAL,
        /// <summary>
        /// Notification quotidienne de fin de journée (ETD) 
        /// </summary>
        EOD,
    }
    #endregion NotificationStepLifeEnum

    

    #region OrderQuantityType3CodeEnum
    /// <summary>
    /// OrderQuantityType3CodeEnum ISO20022
    /// </summary>
    public enum OrderQuantityType3CodeEnum
    {
        /// <summary>
        /// Amount
        /// <para>Order is placed by amount of money.</para>
        /// </summary>
        CASH,
        /// <summary>
        /// UnitsOfMeasurePerTimeUnit
        /// <para>For futures - units of Measure per Time Unit (if used - must specify UnitofMeasure and TimeUnit).</para>
        /// </summary>
        UMPU,
        /// <summary>
        /// Unit
        /// <para>Order is placed by unit.</para>
        /// </summary>
        UNIT,
    }
    #endregion OrderQuantityType3CodeEnum

    // PL 20170411 [23064] Add column PHYSETTLTAMOUNT
    #region PhysicalSettlementAmountEnum
    public enum PhysicalSettlementAmountEnum
    {
        NA,
        None,
        Unsettled,
        Settled
    }
    #endregion PhysicalSettlementAmountEnum

    #region PosKeepingActionSourceEnum
    public enum PosKeepingActionSourceEnum
    {
        MAN,
        SYS,
    }
    #endregion PosKeepingActionSourceEnum
    #region PosKeepingActionTypeEnum
    // RD 20210906 [25803] PosRequestTypeEnum : Add NEX (OptionNotExercised) & NAS (OptionNotAssigned)
    public enum PosKeepingActionTypeEnum
    {
        AAS,
        ABN,
        NEX,
        NAS,
        AEX,
        ASS,
        EXE,
        CLO,
        MOF,
        POC,
        POI,
        POT,
        PGO,
        PSO,
    }
    #endregion PosKeepingActionTypeEnum
    #region PremiumAmountValuationTypeEnum
    public enum PremiumAmountValuationTypeEnum
    {
        Cash,
        PricePerOption,
        PercentageOfNotional,
    }
    #endregion PremiumAmountValuationTypeEnum

    #region AssetMeasureEnum
    /// <summary>
    /// 
    /// </summary>
    public enum AssetMeasureEnum
    {
        /// <summary>
        /// The price of an asset, expressed in par value, excluding accrued interest.
        /// </summary>
        CleanPrice,
        /// <summary>
        /// The price of an asset, expressed in par value, including accrued interest.
        /// </summary>
        DirtyPrice,
        /// <summary>
        /// The price of an instrument as quoted on an exchange or similar market.
        /// </summary>
        MarketQuote,
        /// <summary>
        /// The value of interest accrued from the previous payment to the valuation date
        /// </summary>
        AccruedInterest,
        /// <summary>
        /// Cash paid or received on the valuation date.
        /// </summary>
        Cash,
        /// <summary>
        /// Value at Risk is the amount of money that could be lost over a pre-defined period of time with a a given level of confidence.
        /// </summary>
        VAR,
    }
    #endregion AssetMeasureEnum
    #region PartyInfoEnum
    /// <summary>
    /// 
    /// </summary>
    public enum PartyInfoEnum
    {
        id,
        partyId,
        OTCmlId,
    }
    #endregion PartyInfoEnum

    #region PaymentRuleEnum
    public enum PaymentRuleEnum
    {
        FirstPeriodExcluded,
        FirstAndLastsPeriodsExcluded,
        LastPeriodExcluded,
        None
    };
    #endregion PaymentRuleEnum

    /// <summary>
    /// 
    /// </summary>
    /// FI 20161027 [22151] Add enum
    public enum PerformedSatusEnum
    {
        Performed,
        Unperformed
    }

    #region CalPeriodEnum
    // EG 20190823 [FIXEDINCOME] New Use to calculate interest periods (for Perpetual debSecurity)
    public enum CalPeriodEnum
    {
        All,
        First,
        FirstAndRegular,
        Regular,
        Last,
    }
    #endregion CalPeriodEnum


    #region PreSettlementDateMethodDeterminationEnum
    public enum PreSettlementDateMethodDeterminationEnum
    {
        BothCurrencies,
        BothCurrenciesAndUSD,
        OneByOneCurrency,
        OneByOneCurrencyAndUSD,
    }
    #endregion PreSettlementDateMethodDeterminationEnum
    #region PriceRateType3CodeEnum
    public enum PriceRateType3CodeEnum
    {
        /// <summary>
        /// Discount
        /// <para>Price expressed as a number of percentage points below par, eg, a discount price of 2.0% equals a price of 98 when par is 100.</para>
        /// </summary>
        DISC,
        /// <summary>
        /// Percentage
        /// <para>Price expressed as a percentage of par.</para>
        /// </summary>
        PRCT,
        /// <summary>
        /// Premium
        /// <para>Price expressed as a number of percentage points above par, eg, a premium price of 2.0% equals a price of 102 when par is 100.</para>
        /// </summary>
        PREM,
        /// <summary>
        /// Yield 
        /// <para>Price expressed as a yield.</para>
        /// </summary>
        YIEL,
    }
    #endregion PriceRateType3CodeEnum
    #region PriceValueEnum
    public enum PriceValueEnum
    {
        IDENTICAL,
        ZERO,
    }
    #endregion PriceValueEnum
    #region PrincipalExchangeEnum
    public enum PrincipalExchangeEnum
    {
        Intermediate,
        Final,
        IntermediateAndFinal,
    }
    #endregion PrincipalExchangeEnum
    #region PrincipalExchangeTypeEnum
    public enum PrincipalExchangeTypeEnum
    {
        Balloon,
        CleanIPS,
        DualCurrency,
        IAS,
        ReverseIAS,
    }
    #endregion PrincipalExchangeTypeEnum
    #region ProductTypeCodeEnum
    /// <summary>
    /// <para>Product Type Code (ISO 20022)</para>
    /// <para>Specifies the type of product or financial instrument</para>
    /// </summary>
    public enum ProductTypeCodeEnum
    {
        /// <summary>
        /// Agency
        /// </summary>
        AGEN,
        /// <summary>
        /// Commodity
        /// </summary>
        COMM,
        /// <summary>
        /// Corporate
        /// </summary>
        CORP,
        /// <summary>
        /// Currency
        /// </summary>
        CURR,
        /// <summary>
        /// Equity
        /// </summary>
        EQUI,
        /// <summary>
        /// Financing
        /// </summary>
        FINA,
        /// <summary>
        /// Government
        /// </summary>
        GOVE,
        /// <summary>
        /// Loan
        /// </summary>
        LOAN,
        /// <summary>
        /// MoneyMarket
        /// </summary>
        MOMA,
        /// <summary>
        /// Mortgage
        /// </summary>
        MORT,
        /// <summary>
        /// Municipal
        /// </summary>
        MUNI,
    }
    #endregion ProductTypeCodeEnum
    #region PutCallValueEnum
    public enum PutCallValueEnum
    {
        CALL,
        IDENTICAL,
        PUT,
        REVERSE
    }
    #endregion

    #region QuantityValueEnum
    public enum QuantityValueEnum
    {
        IDENTICAL,
        DOUBLE,
        HALF,
        MULTIPLE,
        DIFFERENT
    }
    #endregion
    #region QuoteEnum

    /// <summary>
    /// BOND,ETD,FXRATE,RATEINDEX,....
    /// </summary>
    /// EG 20091223 Add ETD (suppression de FUTURE dans un future proche)
    /// RD 20100428 Suppression de FUTURE, 
    /// dans le cadre de la suppression des tables ASSET_FUTURE et QUOTE_FUTURE_H    
    /// FI 20110630 add COMMODITY
    /// PL 20111024 add DEBTSECURITY, remove BOND, CONVERTIBLEBOND
    public enum QuoteEnum
    {
        //BOND,
        //CONVERTIBLEBOND,
        COMMODITY,
        DEBTSECURITY,
        DEPOSIT,
        EQUITY,
        EXCHANGETRADEDFUND,
        ETD,
        FXRATE,
        INDEX,
        MUTUALFUND,
        RATEINDEX,
        SIMPLECREDITDEFAULTSWAP,
        SIMPLEFRA,
        SIMPLEIRS,
    }
    #endregion QuoteEnum
    #region QuoteExchangeTypeEnum
    public enum QuoteExchangeTypeEnum
    {
        TODAY,
        TRADEDATE,
        EFFECTIVEDATE,
        EXCHANGERATE,
        AC_CLODATE,
        AC_CLOMINUS1DATE,
        MTM_CLODATE,
        MTM_CLOMINUS1DATE,
    }
    #endregion QuoteExchangeTypeEnum
    #region QuoteTimingEnum
    /// <summary>
    /// Notion Temporelle d'un prix (prix d'ouverture, prix Maximim, prix de clôture, etc...)
    /// </summary>
    public enum QuoteTimingEnum
    {
        /// <summary>
        /// Prix de clôture
        /// </summary>
        Close,
        /// <summary>
        /// Prix haut
        /// </summary>
        High,
        /// <summary>
        /// Prix dans la journée
        /// </summary>
        Intraday,
        /// <summary>
        /// Prix bas
        /// </summary>
        Low,
        /// <summary>
        /// Prix d'ouverture
        /// </summary>
        Open,
    }
    #endregion QuoteTimingEnum

    #region RateSourceTypeEnum
    public enum RateSourceTypeEnum
    {
        None,
        PrimaryRateSource,
        SecondaryRateSource,
    }
    #endregion RateSourceTypeEnum
    #region RateTypeEnum
    public enum RateTypeEnum
    {
        None,
        FixedRate,
        FloatingRate,
        FloatingRate2,
    }
    #endregion RateTypeEnum
    #region RepoDurationEnum
    public enum RepoDurationEnum
    {
        Overnight,
        Term,
        Open,
    }
    #endregion RepoDurationEnum

    #region RoleGActor
    /// <summary>
    /// Représente les rôles pour les regroupement d'actor
    /// </summary>
    // EG 20190308 Add CLOSINGREOPENING Role
    public enum RoleGActor
    {
        /// <summary>
        /// 
        /// </summary>
        TRADING,
        /// <summary>
        /// 
        /// </summary>
        FEE,
        /// <summary>
        /// 
        /// </summary>
        COLLATERAL,
        /// <summary>
        /// 
        /// </summary>
        TRADEMERGING,
        /// <summary>
        /// Messagerie
        /// </summary>
        /// FI 20141230 [20616] Add
        CNF,
        /// <summary>
        /// ClosingReopening position
        /// </summary>
        CLOSINGREOPENING,
    }
    #endregion RoleGActor
    #region RoleGBook
    /// <summary>
    /// Représente les rôles pour les regroupement de book
    /// </summary>
    public enum RoleGBook
    {
        /// <summary>
        /// 
        /// </summary>
        FEE,
        /// <summary>
        /// 
        /// </summary>
        TRADING,
        /// <summary>
        /// 
        /// </summary>
        TRADEMERGING,
        /// <summary>
        /// Messagerie
        /// </summary>
        /// FI 20141230 [20616] Add
        CNF,
        /// EG 20150520
        INVOICING,

    }
    #endregion RoleGBook
    #region RoleGInstr
    /// <summary>
    /// Représente les rôles pour les regroupement d'instrument
    /// </summary>
    /// EG 20130731 [18859]
    public enum RoleGInstr
    {
        ACCOUNTING,
        ADDRESS,
        /// <summary>
        /// Messagerie
        /// </summary>
        CNF,
        FEE,
        //FUNDING,
        INVOICING,
        NETTING,
        POSKEEPING,
        STL,
        TRADING,
        TRADEMERGING,
        TUNING,
    }
    #endregion RoleGInstr
    #region RoleGMarket
    /// <summary>
    /// Représente les rôles pour les regroupement de marché
    /// </summary>
    /// EG 20130731 [18859]
    public enum RoleGMarket
    {
        /// <summary>
        /// Messagerie
        /// </summary>
        CNF,
        INVOICING,
        FEE,
        POSKEEPING,
        STL,
        TRADING,
        TRADEMERGING,
    }
    #endregion RoleGMarket
    #region RoleGContract
    /// <summary>
    /// Représente les rôles pour les regroupement de contrat future
    /// </summary>
    /// EG 20130731 [18859]
    public enum RoleGContract
    {
        CNF, 
        FEE,
        POSKEEPING,
        TRADING,
        TRADEMERGING,
    }
    #endregion RoleGContract

    #region SendReceiveEnum
    /// <summary>
    /// Send,Receive,None
    /// </summary>
    public enum SendReceiveEnum
    {
        Send,
        Receive,
        None
    }
    #endregion SendReceiveEnum
    #region SendEnum
    public enum SendEnum
    {
        SendBy,
        SendTo,
    }
    #endregion
    #region SettlementOfHolidayDeliveryConventionEnum
    public enum SettlementOfHolidayDeliveryConventionEnum
    {
        NotApplicable,
        FOLLOWING,
        PRECEDING,
    }
    #endregion SettlementOfHolidayDeliveryConventionEnum
    #region SettlementStandingInstructionDatabase1CodeEnum
    public enum SettlementStandingInstructionDatabase1CodeEnum
    {
        BrokerDatabase,
        InternalDatabase,
        VendorDatabase
    }
    #endregion SettlementStandingInstructionDatabase1CodeEnum
    #region SiModeEnum
    /// <summary>
    ///<para>Osi:   OTCml Si (settlement instruction specified)</para>
    ///<para>Oissi: OTCml Internal Standing Si</para> 
    ///<para>Oessi:	OTCml External Standing Si</para> 
    ///
    ///<para>Fsi:	Fpml Si (settlement instruction specified)</para>
    ///<para>Fissi:	Fpml Internal Standing Si</para> 
    ///<para>Fessi:	Fpml External Standing Si</para>
    ///
    ///<para>Dbsi:	Default Book Entry SI</para>
    ///<para>Dessi:	Default External Standing Si</para>
    ///<para>Dissi:	Default Internal Standing Si</para>
    /// </summary>
    public enum SiModeEnum
    {
        /// <summary>
        /// OTCml Si (settlement instruction specified)
        /// </summary>
        Osi,
        /// <summary>
        /// OTCml Internal Standing Si 
        /// </summary>
        Oissi,
        /// <summary>
        /// OTCml External Standing Si
        /// </summary>
        Oessi,
        //
        /// <summary>
        /// Fpml Si (settlement instruction specified)
        /// </summary>
        Fsi,
        /// <summary>
        /// Fpml Internal Standing Si 
        /// </summary>
        Fissi,
        /// <summary>
        /// Fpml External Standing Si 
        /// </summary>
        Fessi,
        //
        /// <summary>
        /// Default Book Entry SI
        /// </summary>
        Dbsi,
        /// <summary>
        /// Default External Standing Si
        /// </summary>
        Dessi,
        /// <summary>
        /// Default Internal Standing Si
        /// </summary>
        Dissi,
        //
        Undefined
    }
    #endregion SiModeEnum
    #region SpanStrikeConversionEnum
    /// <summary>
    /// Importation des fichiers SPAN: Type de conversion de strike lorsque les strikes sont incomplet dans le fichier
    /// </summary>
    //PM 20140526 [19992] Ajout enum SpanStrikeConversionEnum
    public enum SpanStrikeConversionEnum
    {
        Quarters,
        Eighths,
    }
    #endregion SpanStrikeConversionEnum
    #region SpheresSourceStatusEnum
    public enum SpheresSourceStatusEnum
    {
        /// <summary>
        /// Statut when the fee from Fee Shedule 
        /// </summary>
        Default,
        /// <summary>
        /// Statut when the user change the fee manually
        /// </summary>
        Forced,
        /// <summary>
        /// Statut when the user substitute a default schedule
        /// </summary>
        /// FI 20180502 [23926] Add
        ScheduleForced,
        /// <summary>
        /// 
        /// </summary>
        Corrected,
    }
    #endregion SpheresSourceStatusEnum
    #region StandInstDbType
    public enum StandInstDbType
    {
        [System.Xml.Serialization.XmlEnumAttribute("0")]
        Other,
        [System.Xml.Serialization.XmlEnumAttribute("1")]
        DTC_SID,
        [System.Xml.Serialization.XmlEnumAttribute("2")]
        ThomsonAlert,
        [System.Xml.Serialization.XmlEnumAttribute("3")]
        GlobalCustodian,
    }
    #endregion StandInstDbType
    #region StatusCalculEnum
    public enum StatusCalculEnum
    {
        TOCALC,		/* To Calcul */
        CALC,		/* Calculated */
        CALCREV		/* Calculated  and revisable */
    }
    #endregion StatusCalculEnum

    #region SideValueEnum
    public enum SideValueEnum
    {
        REVERSESIDE,
        SIDE,
        DELTANEUTRALSIDE
    }
    #endregion
    #region StubEnum
    public enum StubEnum
    {
        None,
        Initial,
        Final,
    }
    #endregion StubEnum
    #region StrikeValueEnum
    public enum StrikeValueEnum
    {
        IDENTICAL,
        LOWER,
        HIGHER,
        DIFFERENT,
        /// <summary>
        /// Spread supérieur si option call ou Spread inférieur si option put
        /// </summary>
        SPREAD,
    }
    #endregion StubEnum

    #region TaxApplicationEnum
    public enum TaxApplicationEnum
    {
        Always,
        Condition,
        Never
    };
    #endregion TaxApplicationEnum
    #region TradeSideEnum
    public enum TradeSideEnum
    {
        All,
        Buyer,
        Seller
    };
    #endregion TradeSideEnum
    #region TransferModeEnum
    // EG 20190318 Upd (Union of TransferClosingModeEnum and TransferReopeningModeEnum) ClosingReopening Step3
    public enum TransferModeEnum
    {
        ReverseTrade,
        ReverseSyntheticPosition,
        ReverseLongShortPosition,
        Trade,
        SyntheticPosition,
        LongShortPosition,
    };
    #endregion TransferModeEnum
    #region TransferPriceEnum
    // EG 20190308 New (ClosingReopeningPostion)
    // EG 20230901 [WI700] ClosingReopeningPosition - Delisting action - NormMsgFactory (FairValue prices)
    public enum TransferPriceEnum
    {
        DayBeforePrice,
        DayPrice,
        FairValueDayBeforePrice,
        FairValueDayPrice,
        TradingPrice,
        Zero,
    };
    #endregion TransferPriceEnum
    #region TriggerHitOperatorEnum
    public enum TriggerHitOperatorEnum
    {
        TriggerIsReached,
        TriggerIsExceeded,
    }
    #endregion TriggerHitOperatorEnum
    #region TypeContractEnum
    /// <summary>
    /// Valeurs possibles dans les colonnes 
    /// </summary>
    /// FI 20170908 [23409] Modify
    // EG 20230901 [WI700] ClosingReopeningPosition - Delisting action - NormMsgFactory (Critère sur le Code ISIN du sous-jactent)
    public enum TypeContractEnum
    {
        GrpMarket,
        Market,
        GrpContract,
        //Contract,  // FI 20170908 [23409] Mise en commentaire
        DerivativeContract, // FI 20170908 [23409] Add
        CommodityContract,  // FI 20170908 [23409] Add
        IsinUnderlyerContract,
        None
    };
    #endregion TypeContractEnum
    #region TypeInstrEnum
    /// <summary>
    /// Product,GrpProduct,Instr,GrpInstr
    /// </summary>
    public enum TypeInstrEnum
    {
        Product,
        GrpInstr,
        Instr,
        // 20120618 MF Ticket ???? Introduction de la valeur GrpProduct dans enum C# 
        //                         pour être synchro avec l'énumeration Spheres avec CODE = 'TypeInstrEnum'
        /// <summary>
        /// Products group 
        /// </summary>
        /// <remarks>
        /// A products group is defined inside of the PRODUCT table, column GPRODUCT; any product sharing the same GPRODUCT value owns
        /// to the same group.
        /// </remarks>
        GrpProduct,
        None
    };
    #endregion TypeInstrEnum
    #region TypeMarketEnum
    public enum TypeMarketEnum
    {
        GrpMarket,
        Market,
        None
    };
    #endregion TypeMarketEnum
    #region TypePartyEnum
    /// <summary>
    /// 
    /// </summary>
    public enum TypePartyEnum
    {
        Actor,
        All,
        Book,
        GrpActor,
        GrpBook,
        None
    };
    #endregion TypePartyEnum
    #region TypeSidePartyEnum
    public enum TypeSidePartyEnum
    {
        ClearingHouse,
        OtherParty1,
        OtherParty2,
        PartyA,
        PartyB
    };
    #endregion TypeSidePartyEnum

    /// <summary>
    /// Dealer ou Clearer
    /// </summary>
    /// FI 20140206 [19564] add Enum 
    public enum TypeSideAllocation
    {
        /// <summary>
        /// Dealer
        /// </summary>
        Dealer,
        /// <summary>
        /// Clearer
        /// </summary>
        Clearer
    }

    #region UnderlyingAssetEnum
    /// <summary>
    /// Classification de sous-jacent issu du CFI Code ISO 10962
    /// </summary>
    // PM 20201028 [25570][25542] Ajout UnderlyingAssetEnum
    [System.Xml.Serialization.XmlTypeAttribute()]
    [SerializableAttribute()]
    public enum UnderlyingAssetEnum
    {
        [System.Xml.Serialization.XmlEnumAttribute("CA")]
        AgricultureForestryFishing,
        [System.Xml.Serialization.XmlEnumAttribute("CE")]
        ExtractionResources,
        [System.Xml.Serialization.XmlEnumAttribute("CI")]
        IndustrialProducts,
        [System.Xml.Serialization.XmlEnumAttribute("CS")]
        Services,
        [System.Xml.Serialization.XmlEnumAttribute("FB")]
        Basket,
        [System.Xml.Serialization.XmlEnumAttribute("FC")]
        Currencies,
        [System.Xml.Serialization.XmlEnumAttribute("FD")]
        InterestRateNotionalDebtSecurities,
        [System.Xml.Serialization.XmlEnumAttribute("FI")]
        Indices,
        [System.Xml.Serialization.XmlEnumAttribute("FO")]
        Options,
        [System.Xml.Serialization.XmlEnumAttribute("FS")]
        StockEquities,
        [System.Xml.Serialization.XmlEnumAttribute("FT")]
        Commodities,
        [System.Xml.Serialization.XmlEnumAttribute("FW")]
        Swaps,
        [System.Xml.Serialization.XmlEnumAttribute("M")]
        Others,
    }
    #endregion UnderlyingAssetEnum

    #region UnderlyingDeliveryStepEnum
    /// <summary>
    /// Etape de livraison du sous-jacent
    /// </summary>
    public enum UnderlyingDeliveryStepEnum
    {
        NA,
        PreDelivery,
        Delivery,
        DeliveredWithInvoicingInTwoSteps,
        Delivered,
    }
    #endregion UnderlyingDeliveryStepEnum

    #region UnitTypeEnum
    // EG 20150622  New price
    public enum UnitTypeEnum
    {
        Currency,
        Percentage,
        Price,
        Rate,
        Qty,
        None,
    }
    #endregion UnitTypeEnum

    #region YieldTypeEnum
    public enum YieldTypeEnum
    {
        COMPOUND,
        SIMPLE,
    }
    #endregion YieldTypeEnum
    #region YieldCurveTypeEnum
    public enum YieldCurveTypeEnum
    {
        CorporateBondCurve,
        GovernmentBondCurve,
        InterBankSwapCurve,
        NA,
    }
    #endregion YieldCurveTypeEnum


    #region SettlementTypeExtendedEnum
    // EG 20170918 [23342] New FpML extensions for MiFID II
    public enum SettlementTypeExtendedEnum
    {
        Cash,
        Election,
        Physical,
        CashOrPhysical,
    }
    #endregion SettlementTypeExtendedEnum

    #region TelephoneTypeEnum
    // EG 20170918 [23342] New FpML extensions for MiFID II
    public enum TelephoneTypeEnum
    {
        Work,
        Mobile,
        Fax,
        Personal,
    }
    #endregion TelephoneTypeEnum

}
