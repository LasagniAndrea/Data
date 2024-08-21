namespace EfsML.Enum.MiFIDII_Extended
{
    /// <summary>
    /// Indicates the role of a person in a transaction.
    /// <para>http://www.fpml.org/coding-scheme/person-role-1-2.xml</para>
    /// </summary>
    /// FI 20170928 [23452] add
    public enum PersonRoleEnum
    {
        /// <summary>
        /// The person who arranged with a client to execute the trade.
        /// </summary>
        Broker,
        /// <summary>
        /// Acquirer of the legal title to the financial instrument.
        /// </summary>
        Buyer,
        /// <summary>
        /// The party or person with legal responsibility for authorization of the execution of the transaction.
        /// </summary>
        DecisionMaker,
        /// <summary>
        /// Person within the firm who is responsible for execution of the transaction.
        /// </summary>
        ExecutionWithinFirm,
        /// <summary>
        /// Person who is responsible for making the investment decision.
        /// </summary>
        InvestmentDecisionMaker,
        /// <summary>
        /// Seller of the legal title to the financial instrument.
        /// </summary>
        Seller,
        /// <summary>
        /// The person who executed the trade.
        /// </summary>
        Trader
    }

    /// <summary>
    /// Defines the role of the algorithm. 
    /// <para>http://www.fpml.org/coding-scheme/algorithm-role-1-0.xml</para>
    /// </summary>
    /// FI 20170928 [23452] add
    public enum AlgorithmRoleEnum
    {
        /// <summary>
        /// Algorithm responsible for the execution of the transaction
        /// </summary>
        Execution,
        /// <summary>
        /// Specifies a role of investment decision for the algorithm.
        /// </summary>
        InvestmentDecision
    }

    /// <summary>
    /// Contains a code representing a related party role. This can be extended to provide custom roles.
    /// <para>http://www.fpml.org/coding-scheme/party-role</para>
    /// </summary>
    /// FI 20170928 [23452] add
    public enum PartyRole
    {
        /// <summary>
        /// Organization responsible for preparing the accounting for the trade.
        /// </summary>
        Accountant,
        /// <summary>
        /// The organization responsible for supplying the allocations for a trade to be allocated to multiple accounts/organizations.
        /// </summary>
        AllocationAgent,
        /// <summary>
        /// The organization that arranged the trade,i.e. brought together the counterparties. Synonyms/Alternatives: Inter-dealer broker,agent.
        /// </summary>
        ArrangingBroker,
        /// <summary>
        /// Organization that suffers the economic benefit of the trade. The beneficiary may be distinct from the principal/counterparty - an example occurs when a hedge fund trades via a prime broker;
        /// in this case the principal is the prime broker,but the beneficiary is the hedge fund.
        /// This can be represented as a payer/receiver account in the name of the hedge fund,but it is also possible to add the party role of "Beneficiary" at the partyTradeInformation level.
        /// </summary>
        Beneficiary,
        /// <summary>
        /// The entity for which the organization supporting the trade's processing has booked/recorded the trade. This is used in non-reporting workflows situations in which the trade doesn't need to be reported but a firm still wants to specify their own side.
        /// </summary>
        BookingParty,
        /// <summary>
        /// A party to the trade that claims a clearing exception,such as an end-user exception under Dodd-Frank Act provisions.
        /// </summary>
        ClearingExceptionParty,
        /// <summary>
        /// Organization that submits the trade to a clearing house on behalf of the principal. Synonyms/alternates: Futures Commission Merchant (FCM),Clearing Broker,Clearing Member Firm.
        /// Some implementations use "Clearing Broker" as synonym.
        /// </summary>
        ClearingFirm,
        /// <summary>
        /// Client as defined under ESMA MIFIR. This is generally the investor or other client of an investment firm,and is synonymous with the Beneficiary in many circumstances.
        /// </summary>
        Client,
        /// <summary>
        /// The party or person who,having legal authority to act on behalf of a trade counterparty,made the decision to acquire or sell the financial instrument.
        /// </summary>
        ClientDecisionMaker,
        /// <summary>
        /// Organization serving as a financial intermediary for the purposes of electronic confirmation or providing services for post-processing of transactional data.
        /// </summary>
        ConfirmationPlatform,
        /// <summary>
        /// A party to a contractual document. If the intended usage relates to the context of the trade lifecycle,more specific annotations have been defined which might be more appropriate.
        /// </summary>
        ContractualParty,
        /// <summary>
        /// An economic counterparty to the trade. Synonym: principal.
        /// </summary>
        Counterparty,
        /// <summary>
        /// Organization offiially attached to the counterparty. e.g. partner,branch,subsidiary.
        /// </summary>
        CounterPartyAffiliate,
        /// <summary>
        /// The topmost entity or organization,within the corporate hierarchy,responsible for the reporting party.
        /// </summary>
        CounterPartyUltimateParent,
        /// <summary>
        /// Organization that enhances the credit of another organization (similar to guarantor,but may not fully guarantee the obligation).
        /// </summary>
        CreditSupportProvider,
        /// <summary>
        /// Acquirer of the legal title to the financial instrument. In the case of an option,the buyer is the holder of the option. In the case of a swap or forward,the buyer will be determined by industry best practice.
        /// This does not refer to an investor or investment manager or other organization on what is typically called the "Buy side"; for that,see the "Client" role. Corresponds to "Buyer" as defined in certain regulations such as ESMA MiFID II/MIFIR RTS 22 field 9.
        /// </summary>
        Buyer,
        /// <summary>
        /// The party or person who,having legal authority to act on behalf of the trade counterparty acting as Buyer as defined in this coding scheme,made the decision to acquire the financial instrument.
        /// Corresponds to "buyer decision maker" as defined in ESMA's MIFIR RTS 23 report.
        /// This does not refer to the decision maker for what is traditionally called the "Buy side"; for that,see the "Client Decision Maker" role.
        /// </summary>
        BuyerDecisionMaker,
        /// <summary>
        /// An organization that clears trades through a clearing house,via a clearing broker (member of the clearing house) who acts as an agent on its behalf.
        /// The term "client" refers to the organization's role in the clearing process in relation to its clearing broker,and not whether it is a price maker or taker in the execution process.
        /// </summary>
        ClearingClient,
        /// <summary>
        /// The organization that acts as a central counterparty to clear a derivatives contract.
        /// This is used to represent the role of Central Counterparties (CCPs) or Derivative Clearing Organizations (DCOs).
        /// Sometimes called "ClearingService".
        /// Some implementations also use the term "Clearer".
        /// </summary>
        ClearingOrganization,
        /// <summary>
        /// Organization that maintains custody of the asset represented by the trade on behalf of the owner/principal.
        /// </summary>
        Custodian,
        /// <summary>
        /// Entity submitting the transaction report to the competent authority.
        /// </summary>
        DataSubmitter,
        /// <summary>
        /// Organization that is disputing the trade or transaction.
        /// </summary>
        DisputingParty,
        /// <summary>
        /// A marketplace organization which purpose is to maintain document records. If the intended usage relates to the context of the trade lifecycle,more specific annotations have been defined which might be more appropriate.
        /// </summary>
        DocumentRepository,
        /// <summary>
        /// The (generally sell-side) organization that executed the trade; the price-making party.
        /// </summary>
        ExecutingBroker,
        /// <summary>
        /// Entity executing the transaction. If the transaction is executed directly by the reporting party,it will be the reporting party.
        /// If it is executed by an execution agent or an affiliated party on behalf of the reporting party,it will be that affiliate or agent.
        /// </summary>
        ExecutingEntity,
        /// <summary>
        /// The (generally buy-side) organization that acts to execute trades on behalf of an investor.
        ///  Typically this is an investment manager or asset manager,and also makes the investment decisions for the investor.
        ///  If required,a separate InvestmentDecision role can be specified to distinguish that the party making the investment decision is different.
        /// </summary>
        ExecutionAgent,
        /// <summary>
        /// The facility,exchange,or market where the trade was executed. Synonym: Swap Execution Facility,Designated Contract Market,Execution Venue.
        /// </summary>
        ExecutionFacility,
        /// <summary>
        /// Organization that backs (guarantees) the credit risk of the trade.
        /// </summary>
        Guarantor,
        /// <summary>
        /// The entity transmitting the order to the reporting firm. Synonym: Transmitting Firm.
        /// </summary>
        OrderTransmitter,
        /// <summary>
        /// The organization that takes on or took on the credit risk for this trade by stepping in between the two economic parties (without a central counterparty clearing mechanism).
        /// </summary>
        PrimeBroker,
        /// <summary>
        /// The trade repository at which the trade was reported previous to the current trade repository.
        /// </summary>
        PriorTradeRepository,
        /// <summary>
        /// The reporting service (whether trade repository,market data service,or exchange/facility/venue data distribution service) that published the report of this trade.
        /// </summary>
        PublicationVenue,
        /// <summary>
        /// The party with the regulatory responsibility to report this trade.
        /// </summary>
        ReportingParty,
        /// <summary>
        /// Organization offiially attached to the reporting party e.g. partner,branch,subsidiary.
        /// </summary>
        ReportingPartyAffiliate,
        /// <summary>
        /// The topmost entity or organization,within the corporate hierarchy,responsible for the reporting party.
        /// </summary>
        ReportingPartyUltimateParent,
        /// <summary>
        /// A counterparty in a trade,which performs in one of the following capacities:
        /// 1) it transfers or agrees to transfer in the future an instrument or title to that instrument in exchange for payment,
        /// 2) it writes a derivatives instrument such as an option or a swap in which it provides risk protection to the buyer.
        ///  This does not refer to the broker/dealer or other organization on what is typically called the "Sell side"; for that,see the "Executing Broker" role. Corresponds to "Seller" as defined in certain regulations such as ESMA MiFID II/MIFIR RTS 22 field 16.
        /// </summary>
        Seller,
        /// <summary>
        /// The party or person who,having legal authority to act on behalf of the trade counterparty acting as Seller as defined in this coding scheme,made the decision to sell the financial instrument.
        /// Corresponds to "seller decision maker" as defined in ESMA's MIFIR RTS 23 report.
        /// This does not refer to the decision maker for what is traditionally called the "Sell side"; for that,see the "Trader" person role.
        /// </summary>
        SellerDecisionMaker,
        /// <summary>
        /// The organization that makes or receives payments on behalf of the given principal party.
        /// </summary>
        SettlementAgent,
        /// <summary>
        /// An organization that maintains records of the trade for regulatory reporting purposes.
        /// </summary>
        TradeRepository,
        /// <summary>
        /// The organization that originally supplied the record of the trade. In the context of regulatory reporting,it is the submitter of the trade record to a regulator or TR.
        /// </summary>
        TradeSource,
        /// <summary>
        /// The entity responsible for managing the assets/investments of this party. Synonnym: Asset Manager,Investment Manager,Trading Advisory.
        /// </summary>
        TradingManager,
        /// <summary>
        /// An entity with which this party trades from time to time,ie. with which it acts as a counterparty on some transactions.This role is used for static reference data,not individual transactions.
        /// </summary>
        TradingPartner
    }

    /// <summary >
    /// Specifies the type of trading capacity, as defined under ESMA MiFID II / MIFIR.
    /// </summary>
    /// FI 20170928 [23452] add
    public enum TradingCapacityScheme
    {
        /// <summary >
        /// Trading in an "any other capacity".
        /// </summary>
        AOTC,
        /// <summary >
        /// Dealing on own account.
        /// </summary>
        DEAL,
        /// <summary >
        /// Trading in a matched principal trading capacity.
        /// </summary>
        MTCH,
    }


    /// <summary >
    /// FpML coding scheme supporting the ESMA Asset Class and Sub Asset Class product classification. Each value contains an FpML defined taxonomy style code for the corresponding ESMA codes. The description of the code contains the full string ESMA value. This coding scheme should be used in the productType element.
    /// <para>http://www.fpml.org/coding-scheme/esma-product-classification</para>
    /// </summary>
    /// FI 20170928 [23452] add
    public enum EsmaProductClassificationScheme
    {
        /// <summary >
        /// Bespoke basket credit default swap (CDS)
        /// </summary>
        [System.Xml.Serialization.XmlEnumAttribute("CDS:BespokeCreditDefaultSwap")]
        CDS_BespokeCreditDefaultSwap,
        /// <summary >
        /// Freight derivatives
        /// </summary>
        [System.Xml.Serialization.XmlEnumAttribute("CDS:FreightDerivatives")]
        CDS_FreightDerivatives,
        /// <summary >
        /// Index credit default swap (CDS)
        /// </summary>
        [System.Xml.Serialization.XmlEnumAttribute("CDS:IndexCreditDefaultSwap")]
        CDS_IndexCreditDefaultSwap,
        /// <summary >
        /// CDS index options
        /// </summary>
        [System.Xml.Serialization.XmlEnumAttribute("CDS:IndexOption")]
        CDS_IndexOption,
        /// <summary >
        /// Other credit derivatives
        /// </summary>
        [System.Xml.Serialization.XmlEnumAttribute("CDS:Other")]
        CDS_Other,
        /// <summary >
        /// Single name credit default swap (CDS)
        /// </summary>
        [System.Xml.Serialization.XmlEnumAttribute("CDS:SingleNameCreditDefaultSwap")]
        CDS_SingleNameCreditDefaultSwap,
        /// <summary >
        /// Single name CDS options
        /// </summary>
        [System.Xml.Serialization.XmlEnumAttribute("CDS:SingleNameOption")]
        CDS_SingleNameOption,
        /// <summary >
        /// Agricultural commodity futures/forwards
        /// </summary>
        [System.Xml.Serialization.XmlEnumAttribute("Commodity:AgriculturalCommodityFutureForward")]
        Commodity_AgriculturalCommodityFutureForward,
        /// <summary >
        /// Agricultural commodity options
        /// </summary>
        [System.Xml.Serialization.XmlEnumAttribute("Commodity:AgriculturalCommodityOption")]
        Commodity_AgriculturalCommodityOption,
        /// <summary >
        /// Agricultural commodity swaps
        /// </summary>
        [System.Xml.Serialization.XmlEnumAttribute("Commodity:AgriculturalCommoditySwap")]
        Commodity_AgriculturalCommoditySwap,
        /// <summary >
        /// Energy commodity swaps
        /// </summary>
        [System.Xml.Serialization.XmlEnumAttribute("Commodity:EnergyCommoditySwap")]
        Commodity_EnergyCommoditySwap,
        /// <summary >
        /// Other commodity derivatives
        /// </summary>
        [System.Xml.Serialization.XmlEnumAttribute("Commodity:Other")]
        Commodity_Other,
        /// <summary >
        /// Certified Emission Reductions (CER)
        /// </summary>
        [System.Xml.Serialization.XmlEnumAttribute("EmissionAllowances:CertifiedEmissionReductions")]
        EmissionAllowances_CertifiedEmissionReductions,
        /// <summary >
        /// Emission Reduction Units (ERU)
        /// </summary>
        [System.Xml.Serialization.XmlEnumAttribute("EmissionAllowances:EmissionReductionUnits")]
        EmissionAllowances_EmissionReductionUnits,
        /// <summary >
        /// European Union Allowances (EUA)
        /// </summary>
        [System.Xml.Serialization.XmlEnumAttribute("EmissionAllowances:EuropeanUnionAllowances")]
        EmissionAllowances_EuropeanUnionAllowances,
        /// <summary >
        /// European Union Aviation Allowances (EUAA)
        /// </summary>
        [System.Xml.Serialization.XmlEnumAttribute("EmissionAllowances:EuropeanUnionAviationAllowances")]
        EmissionAllowances_EuropeanUnionAviationAllowances,
        /// <summary >
        /// Other Emission allowance derivatives
        /// </summary>
        [System.Xml.Serialization.XmlEnumAttribute("EmissionAllowances:Other")]
        EmissionAllowances_Other,
        /// <summary >
        /// Dividend index futures/ forwards
        /// </summary>
        [System.Xml.Serialization.XmlEnumAttribute("Equity:DividendIndexFutureForward")]
        Equity_DividendIndexFutureForward,
        /// <summary >
        /// Dividend index options
        /// </summary>
        [System.Xml.Serialization.XmlEnumAttribute("Equity:DividendIndexOption")]
        Equity_DividendIndexOption,
        /// <summary >
        /// ETF futures/ forwards
        /// </summary>
        [System.Xml.Serialization.XmlEnumAttribute("Equity:ETFFutureForward")]
        Equity_ETFFutureForward,
        /// <summary >
        /// ETF options
        /// </summary>
        [System.Xml.Serialization.XmlEnumAttribute("Equity:ETFOption")]
        Equity_ETFOption,
        /// <summary >
        /// Other equity derivatives
        /// </summary>
        [System.Xml.Serialization.XmlEnumAttribute("Equity:Other")]
        Equity_Other,
        /// <summary >
        /// Portfolio Swaps
        /// </summary>
        [System.Xml.Serialization.XmlEnumAttribute("Equity:PortfolioSwap")]
        Equity_PortfolioSwap,
        /// <summary >
        /// Stock dividend futures/ forwards
        /// </summary>
        [System.Xml.Serialization.XmlEnumAttribute("Equity:StockDividendFutureForward")]
        Equity_StockDividendFutureForward,
        /// <summary >
        /// Stock dividend options
        /// </summary>
        [System.Xml.Serialization.XmlEnumAttribute("Equity:StockDividendOption")]
        Equity_StockDividendOption,
        /// <summary >
        /// Stock futures/ forwards
        /// </summary>
        [System.Xml.Serialization.XmlEnumAttribute("Equity:StockFutureForward")]
        Equity_StockFutureForward,
        /// <summary >
        /// Stock index futures/ forwards
        /// </summary>
        [System.Xml.Serialization.XmlEnumAttribute("Equity:StockIndexFutureForward")]
        Equity_StockIndexFutureForward,
        /// <summary >
        /// Stock index options
        /// </summary>
        [System.Xml.Serialization.XmlEnumAttribute("Equity:StockIndexOption")]
        Equity_StockIndexOption,
        /// <summary >
        /// Stock options
        /// </summary>
        [System.Xml.Serialization.XmlEnumAttribute("Equity:StockOption")]
        Equity_StockOption,
        /// <summary >
        /// Swaps
        /// </summary>
        [System.Xml.Serialization.XmlEnumAttribute("Equity:Swap")]
        Equity_Swap,
        /// <summary >
        /// Volatility index futures/ forwards
        /// </summary>
        [System.Xml.Serialization.XmlEnumAttribute("Equity:VolatilityIndexFutureForward")]
        Equity_VolatilityIndexFutureForward,
        /// <summary >
        /// Volatility index options
        /// </summary>
        [System.Xml.Serialization.XmlEnumAttribute("Equity:VolatilityIndexOption")]
        Equity_VolatilityIndexOption,
        /// <summary >
        /// Deliverable forward (DF)
        /// </summary>
        [System.Xml.Serialization.XmlEnumAttribute("FX:DeliverableForward")]
        FX_DeliverableForward,
        /// <summary >
        /// Deliverable FX options (DO)
        /// </summary>
        [System.Xml.Serialization.XmlEnumAttribute("FX:DeliverableOption")]
        FX_DeliverableOption,
        /// <summary >
        /// Deliverable FX swaps (DS)
        /// </summary>
        [System.Xml.Serialization.XmlEnumAttribute("FX:DeliverableSwap")]
        FX_DeliverableSwap,
        /// <summary >
        /// FX futures
        /// </summary>
        [System.Xml.Serialization.XmlEnumAttribute("FX:Future")]
        FX_Future,
        /// <summary >
        /// Non-deliverable forward (NDF)
        /// </summary>
        [System.Xml.Serialization.XmlEnumAttribute("FX:NonDeliverableForward")]
        FX_NonDeliverableForward,
        /// <summary >
        /// Non-Deliverable FX options (NDO)
        /// </summary>
        [System.Xml.Serialization.XmlEnumAttribute("FX:NonDeliverableOption")]
        FX_NonDeliverableOption,
        /// <summary >
        /// Non-Deliverable FX swaps (NDS)
        /// </summary>
        [System.Xml.Serialization.XmlEnumAttribute("FX:NonDeliverableSwap")]
        FX_NonDeliverableSwap,
        /// <summary >
        /// Other Foreign Exchange Derivatives
        /// </summary>
        [System.Xml.Serialization.XmlEnumAttribute("FX:Other")]
        FX_Other,
        /// <summary >
        /// Bond futures/forwards
        /// </summary>
        [System.Xml.Serialization.XmlEnumAttribute("InterestRate:BondFutureForward")]
        InterestRate_BondFutureForward,
        /// <summary >
        /// Bond options
        /// </summary>
        [System.Xml.Serialization.XmlEnumAttribute("InterestRate:BondOption")]
        InterestRate_BondOption,
        /// <summary >
        /// Fixed-to-Fixed 'multi-currency swaps' or ‘cross-currency swaps’ and futures/forwards on Fixed-to-Fixed 'multi-currency swaps' or ‘cross-currency swaps’
        /// </summary>
        [System.Xml.Serialization.XmlEnumAttribute("InterestRate:FixedFixed:CrossCurrency")]
        InterestRate_FixedFixed_CrossCurrency,
        /// <summary >
        /// Fixed-to-Fixed 'single currency swaps' and futures/forwards on Fixed-to-Fixed 'single currency swaps'
        /// </summary>
        [System.Xml.Serialization.XmlEnumAttribute("InterestRate:FixedFixed:SingleCurrency")]
        InterestRate_FixedFixed_SingleCurrency,
        /// <summary >
        /// Fixed-to-Float 'multi-currency swaps' or ‘cross-currency swaps’ and futures/forwards on Fixed-to-Float 'multi-currency swaps' or ‘cross-currency swaps’
        /// </summary>
        [System.Xml.Serialization.XmlEnumAttribute("InterestRate:FixedFloat:CrossCurrency")]
        InterestRate_FixedFloat_CrossCurrency,
        /// <summary >
        /// Fixed-to-Float 'single currency swaps' and futures/forwards on Fixed-to-Float 'single currency swaps'
        /// </summary>
        [System.Xml.Serialization.XmlEnumAttribute("InterestRate:FixedFloat:SingleCurrency")]
        InterestRate_FixedFloat_SingleCurrency,
        /// <summary >
        /// Float-to-Float 'multi-currency swaps' or ‘cross-currency swaps’ and futures/forwards on Float-to-Float 'multi-currency swaps' or ‘cross-currency swaps’
        /// </summary>
        [System.Xml.Serialization.XmlEnumAttribute("InterestRate:FloatFloat:CrossCurrency")]
        InterestRate_FloatFloat_CrossCurrency,
        /// <summary >
        /// Float-to-Float 'single currency swaps' and futures/forwards on Float-to-Float 'single currency swaps'
        /// </summary>
        [System.Xml.Serialization.XmlEnumAttribute("InterestRate:FloatFloat:SingleCurrency")]
        InterestRate_FloatFloat_SingleCurrency,
        /// <summary >
        /// IR futures and FRA
        /// </summary>
        [System.Xml.Serialization.XmlEnumAttribute("InterestRate:FutureFra")]
        InterestRate_FutureFra,
        /// <summary >
        /// Inflation 'multi-currency swaps' or ‘cross-currency swaps’ and futures/forwards on Inflation 'multi-currency swaps' or ‘cross-currency swaps’
        /// </summary>
        [System.Xml.Serialization.XmlEnumAttribute("InterestRate:Inflation:CrossCurrency")]
        InterestRate_Inflation_CrossCurrency,
        /// <summary >
        /// Inflation 'single currency swaps' and futures/forwards on Inflation 'single currency swaps'
        /// </summary>
        [System.Xml.Serialization.XmlEnumAttribute("InterestRate:Inflation:SingleCurrency")]
        InterestRate_Inflation_SingleCurrency,
        /// <summary >
        /// Overnight Index Swap (OIS) 'multi-currency swaps' or ‘cross-currency swaps’ and futures/forwards on Overnight Index Swap (OIS) 'multi-currency swaps' or ‘cross-currency swaps’
        /// </summary>
        [System.Xml.Serialization.XmlEnumAttribute("InterestRate:OIS:CrossCurrency")]
        InterestRate_OIS_CrossCurrency,
        /// <summary >
        /// Overnight Index Swap (OIS) 'single currency swaps' and futures/forwards on Overnight Index Swap (OIS) 'single currency swaps'
        /// </summary>
        [System.Xml.Serialization.XmlEnumAttribute("InterestRate:OIS:SingleCurrency")]
        InterestRate_OIS_SingleCurrency,
        /// <summary >
        /// IR options
        /// </summary>
        [System.Xml.Serialization.XmlEnumAttribute("InterestRate:Option")]
        InterestRate_Option,
        /// <summary >
        /// Other Interest Rate Derivatives
        /// </summary>
        [System.Xml.Serialization.XmlEnumAttribute("InterestRate:Other")]
        InterestRate_Other,
        /// <summary >
        /// Swaptions
        /// </summary>
        [System.Xml.Serialization.XmlEnumAttribute("InterestRate:Swaption")]
        InterestRate_Swaption,
    }

    /// <summary >
    /// Specifies the type of short selling indicator, as defined under ESMA MiFID II.
    /// </summary>
    /// FI 20170928 [23452] add
    public enum ShortSaleScheme
    {
        /// <summary >
        /// Information not available.
        /// </summary>
        NTAV,
        /// <summary >
        /// No short sale.
        /// </summary>
        SELL,
        /// <summary >
        /// Short sale with no exemption.
        /// </summary>
        SESH,
        /// <summary >
        /// Short sale with exemption.
        /// </summary>
        SSEX,
        /// <summary>
        ///  Information not available
        /// </summary>
        UNDI
    }

}