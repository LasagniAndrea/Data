#region Using Directives
using System;
using System.Collections.Generic;
using System.Text;

using EFS.GUI.Interface;
using EfsML.Interface;

using FixML.v50SP1;

using FixML.Enum;
using FixML.v50SP1.Enum;
using EfsML.StrategyMarker;

#endregion Using Directives

namespace FixML.Interface
{
    #region IFixClrInstGrp
    public interface IFixClrInstGrp
    {
        #region Accessors
        ClearingInstructionEnum ClearingInstruction { set; get;}
        bool ClearingInstructionSpecified { set; get;}
        #endregion Accessors
    }
    #endregion IFixClrInstGrp
    #region IFixInstrument
    /// <summary>
    /// 
    /// </summary>
    /// FI 20131126 [19271] add MaturityDate
    /// EG 20140702 Add SrcSpecified|Src
    public interface IFixInstrument
    {
        #region Accessors
        /// <summary>
        /// <remarks>Contient l'identifier du dérivative contrat</remarks>
        /// </summary>
        string Symbol { set; get; }

        /// <summary>
        /// Security identifier value of SecurityIDSource(22) type(e.g.CUSIP, SEDOL, ISIN, etc).  Requires SecurityIDSource.
        /// </summary>
        string SecurityId { set; get; }

        /// <summary>
        /// Identifies class or source of the SecurityID (48) value.  Required if SecurityID is specified.
        /// 100+ are reserved for private security identifications
        /// </summary>
        Nullable<SecurityIDSourceEnum> Src { set; get; }

        IFixAlternateAssetId[] AlternateId { set; get; }
        string CFICode { set; get; }
        string MaturityMonthYear { set; get; }
        bool MaturityDateSpecified { set; get; }
        /// <summary>
        /// Obtient ou définie la date de amturité de l'asset
        /// </summary>
        EFS_Date MaturityDate { set; get; }
        EFS_Decimal StrikePrice { set; get; }
        bool StrikePriceSpecified { set; get; }
        string OptAttribute { set; get; }
        PutOrCallEnum PutOrCall { set; get; }
        bool PutOrCallSpecified { set; get; }
        string SecurityExchange { set; get; }
        string ISINCode { set; get; }
        /// <summary>
        /// 
        /// </summary>
        /// FI 20200216 [25699] add
        string MarketAssignedIdentifier { set; get; }
        // ESE
        bool FixProductSpecified { set; get; }
        ProductEnum FixProduct { set; get; }
        string RICCode { set; get; }
        string BBGCode { set; get; }
        string NSINCode { set; get; }
        string NSINTypeCode { set; get; }
        string NSINTypeCodeText { get; }
        bool IssuerSpecified { set; get; }
        string Issuer { set; get; }
        bool IssueDateSpecified { set; get; }
        EFS_Date IssueDate { set; get; }
        bool CountryOfIssueSpecified { set; get; }
        string CountryOfIssue { set; get; }
        bool StateOrProvinceOfIssueSpecified { set; get; }
        string StateOrProvinceOfIssue { set; get; }
        bool LocaleOfIssueSpecified { set; get; }
        string LocaleOfIssue { set; get; }
        //
        string ExchangeSymbol { set; get; }
        /// <summary>
        /// 
        /// </summary>
        /// FI 20220209 [25699] Add
        bool CntrctDtSpecified { set; get; }
        /// <summary>
        /// Eurex definition: Date used to identify the instrument
        /// </summary>
        /// FI 20220209 [25699] Add
        EFS_Date CntrctDt { set; get; }

        /// <summary>
        /// 
        /// </summary>
        /// FI 20220209 [25699] Add
        bool CntrctFreqSpecified { set; get; }
        /// <summary>
        /// Eurex definition: Indicates frequency of instrument creation.
        /// </summary>
        /// FI 20220209 [25699] Add
        ContractFrequencyEnum CntrctFreq { set; get; }
        #endregion Accessors
        
        #region Methods
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pIDSource"></param>
        /// <returns></returns>
        IFixAlternateAssetId GetAlternateId(string pIDSource);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pID"></param>
        /// <param name="pIDSource"></param>
        void SetAlternateId(string pID, string pIDSource);
        /// <summary>
        ///  Alimentation de SecurityID et sa source
        /// </summary>
        /// <param name="pID"></param>
        /// <param name="pIDSource"></param>
        void SetSecurityID(string pID, Nullable<SecurityIDSourceEnum> pIDSource);
        #endregion

    }
    #endregion IFixInstrument

    #region IFixParty
    public interface IFixParty
    {
        #region Accessors
        FixPartyReference PartyId { set; get;}
        bool PartyIdSpecified { set; get;}
        PartyIDSourceEnum PartyIdSource { set; get;}
        bool PartyIdSourceSpecified { set; get;}
        PartyRoleEnum PartyRole { set; get;}
        bool PartyRoleSpecified { set; get;}
        IFixPartySubGrp[] PtysSubGrp { set; get;}
        #endregion Accessors
    }
    #endregion IFixParty
    #region IFixPartySubGrp
    public interface IFixPartySubGrp
    {
        #region Accessors
        string PartySubId { set; get;}
        PartySubIDTypeEnum PartySubIdType { set; get;}
        #endregion Accessors
    }
    #endregion IFixPartySubGrp

    #region IFixRelatedPositionGrp
    // PM 20160428 [22107] Ajout à partir de l'Extension Pack: FIX.5.0SP2 EP142
    public interface IFixRelatedPositionGrp
    {
        #region Accessors
        string ID { set; get; }
        //
        RelatedPositionIDSourceEnum Src { set; get; }
        bool SrcSpecified { set; get; }
        //
        System.DateTime Dt { set; get; }
        bool DtSpecified { set; get; }
        #endregion Accessors
    }
    #endregion IFixRelatedPositionGrp

    // EG 20240227 [WI855] Trade input : New data TVTIC (Trading Venue Transaction Identification Code)
    public interface IFixRegulatoryTradeIDGrp
    {
        /// <summary>
        /// Tag : 1903 
        /// Trade identifier required by government regulators or other 
        /// regulatory organizations for regulatory reporting purposes. 
        /// For example, unique swap identifer (USI) as required by the U.S. 
        /// Commodity Futures Trading Commission.
        /// </summary>
        string ID { set; get; }
        /// <summary>
        /// Tag : 1905 
        /// Identifies the reporting entity that originated the value in RegulatoryTradeID (1903). 
        /// The reporting entitiy identifier may be assigned by a regulator.
        /// </summary>
        RegulatoryTradeIDSourceEnum Src { set; get; }
        bool SrcSpecified { set; get; }
        /// <summary>
        /// Tag : 1904
        /// Identifies the event which caused origination of the identifier in RegulatoryTradeID (1903)
        /// </summary>
        RegulatoryTradeIDEventEnum Evnt { set; get; }
        bool EvntSpecified { set; get; }
        /// <summary>
        /// Tag : 1906
        /// Specifies the type of trade identifier provided in RegulatoryTradeID (1903).
        /// Contextual hierarchy of events for the same trade or transaction maybe captured 
        /// through use of the different RegulatoryTradeIDType (1906) values using multiple instances 
        /// of the repeating group as needed for regulatory reporting.
        /// </summary>
        RegulatoryTradeIDTypeEnum Typ { set; get; }
        bool TypSpecified { set; get; }

        /// <summary>
        /// Tag : 2397
        /// Specifies the scope to which the RegulatoryTradeID (1903) applies. 
        /// Used when a trade must be assigned more than one identifier, 
        /// e.g. one for the clearing member and another for the client on a cleared trade as 
        /// with the principal model in Europe.
        /// </summary>
        RegulatoryTradeIDScopeEnum Scope { set; get; }
        bool ScopeSpecified { set; get; }
        /// <summary>
        /// Tag : 2411
        /// Identifies the leg of the trade the entry applies to by referencing the leg's LegID (1788).
        /// </summary>
        string LegRefID { set; get; }
        bool LegRefIDSpecified { set; get; }
    }
    #region IFixAlternateAssetId
    public interface IFixAlternateAssetId
    {
        #region Accessors
        string AlternateId { set; get;}
        SecurityIDSourceEnum AlternateIdSource { set; get;}
        bool AlternateIdSourceSpecified { set; get;}
        #endregion Accessors
    }
    #endregion SecAltIDGrp_Block

    #region IFixTrdCapRptSideGrp
    public interface IFixTrdCapRptSideGrp
    {
        #region Accessors
        bool SideSpecified { set; get;}
        SideEnum Side { set; get;}
        //
        bool OrderIdSpecified { set; get;}
        string OrderId { set; get;}
        //
        bool ExecRefIdSpecified { set; get;}
        string ExecRefId { set; get;}
        
        IFixParty[] Parties { set; get;}
        //
        bool AccountSpecified { set; get;}
        string Account { set; get;}
        //
        bool AcctIdSourceSpecified { set; get;}
        AcctIDSourceEnum AcctIdSource { set; get;}
        //
        bool AccountTypeSpecified { set; get;}
        AccountTypeEnum AccountType { set; get;}
        //
        IFixClrInstGrp[] ClrInstGrp { set; get;}
        //
        bool TradeInputSourceSpecified { set; get;}
        string TradeInputSource { set; get;}
        //
        bool TradeInputDeviceSpecified { set; get;}
        string TradeInputDevice { set; get;}
        //
        bool OrderInputDeviceSpecified { set; get;}
        string OrderInputDevice { set; get;}
        //
        bool OrdTypeSpecified { set; get;}
        OrdTypeEnum OrdType { set; get;}
        //
        bool TradingSessionIdSpecified { set; get;}
        string TradingSessionId { set; get;}
        //
        bool PositionEffectSpecified { set; get;}
        PositionEffectEnum PositionEffect { set; get;}
        //
        bool TextSpecified { set; get;}
        string Text { set; get;}
        //
        // PM 20160428 [22107] Ajout à partir de l'Extension Pack: FIX.5.0SP2 EP142
        IFixRelatedPositionGrp[] ReltdPos { set; get; }
        #endregion Accessors
    }
    #endregion IFixTrdCapRptSideGrp

    #region IFixTradeCaptureReport
    // EG 20170918 [23452] Upd TransactTime, Add LastUpdateTime
    // EG 20171025 [23509] Add RemoveTradeRegulatoryTimeStamp
    /// EG 20190730 New Interface ITradeTypeReport
    // EG 20240227 [WI855] Trade input : New data TVTIC (Trading Venue Transaction Identification Code)
    public interface IFixTradeCaptureReport : ITradeTypeReport
    {
        #region Accessors
        bool TradeIdSpecified { set; get; }
        string TradeId { set; get; }
        //
        bool FirmTradeIdSpecified { set; get; }
        string FirmTradeId { set; get; }
        //
        bool TradeReportTransTypeSpecified { set; get; }
        TradeReportTransTypeEnum TradeReportTransType { set; get; }
        //
        //bool TrdTypeSpecified { set; get; }
        //TrdTypeEnum TrdType { set; get; }
        //
        //bool SecondaryTrdTypeSpecified { set; get; }
        //SecondaryTrdTypeEnum SecondaryTrdType { set; get; }
        //

        //bool TrdSubTypeSpecified { set; get; }
        //TrdSubTypeEnum TrdSubType { set; get; }
        //
        bool ExecTypeSpecified { set; get; }
        ExecTypeEnum ExecType { set; get; }
        //
        bool TradeLinkIdSpecified { set; get; }
        string TradeLinkId { set; get; }
        //
        bool TrdMatchIdSpecified { set; get; }
        string TrdMatchId { set; get; }
        //
        bool ExecIdSpecified { set; get; }
        string ExecId { set; get; }
        //
        bool OrdStatusSpecified { set; get; }
        OrdStatusEnum OrdStatus { set; get; }
        //
        IFixInstrument Instrument { set; get; }
        //
        bool LastPxSpecified { set; get; }
        EFS_Decimal LastPx { set; get; }
        //
        bool LastQtySpecified { set; get; }
        EFS_Decimal LastQty { set; get; }
        //
        bool CcySpecified { set; get; }
        string Ccy { set; get; }
        //
        bool TradeDateSpecified { set; get; }
        EFS_Date TradeDate { set; get; }
        //
        bool ClearingBusinessDateSpecified { set; get; }
        EFS_Date ClearingBusinessDate { set; get; }
        //
        bool TransactTimeSpecified { set; get; }
        EFS_DateTimeOffset TransactTime { set; get; }

        bool LastUpdateTimeSpecified { set; get; }
        EFS_DateTimeOffset LastUpdateTime { set; get; }

        bool OrderCategorySpecified { set; get; }
        OrderCategoryEnum OrderCategory { set; get; }
        //
        IFixTrdCapRptSideGrp[] TrdCapRptSideGrp { set; get; }
        //
        bool TransferReasonSpecified { set; get; }
        EFS_String TransferReason { set; get; }

        /// <summary>
        /// Get the type of strategy, which the current trade is making part of
        /// </summary>
        /// MF XXXXXXXXX [17864]
        /// FI 20180214 [23774] Add Set
        string SecSubTyp { get; set; }

        // ticket 17864
        /// <summary>
        /// Get the type of strategy, which the current trade is making part of. 
        /// Expressed as value of the StrategyEnum Spheres enumeration
        /// </summary>
        StrategyEnumRepository.StrategyEnum StrategyTyp { get; }

        // ticket 17864
        /// <summary>
        /// Get the current leg number for the current trade
        /// </summary>
        /// MF XXXXXXXXX [17864]
        /// FI 20180214 [23774] Add Set
        int LegNo { get; set; }

        /// <summary>
        /// Traded Regulatory timestamps
        /// </summary>
        bool TradedRegulatoryTimestampsSpecified { set; get; }
        IFixTrdRegTimestamps[] TradedRegulatoryTimestamps { set; get; }

        // EG 20240227 [WI855] New
        bool RegulatoryTradeIDGrpSpecified { set; get; }
        // EG 20240227 [WI855] New
        IFixRegulatoryTradeIDGrp[] RegulatoryTradeIDGrp { set; get; }
        #endregion Accessors
        #region Method
        /// <summary>
        /// supprime les informations d'appartenance à une stratégie
        /// </summary>
        void ResetStrategyLeg();
        void SetTradeRegulatoryTimeStamp(TrdRegTimestampTypeEnum pTrdRegTimestampType, string pValue);
        void RemoveTradeRegulatoryTimeStamp(TrdRegTimestampTypeEnum pTrdRegTimestampType);
        /// <summary>
        /// Alimentation de la collection RegulatoryTradeIDGrp
        /// avec un ID pour un type donné (RegulatoryTradeIDTypeEnum)
        /// </summary>
        /// <param name="pRegulatoryTradeIDType">Type</param>
        /// <param name="pValue">Valeur</param>
        // EG 20240227 [WI855] New
        void SetRegulatoryTradeIDGrp(RegulatoryTradeIDTypeEnum pRegulatoryTradeIDType, string pValue);
        /// <summary>
        /// Suppression d'un élément de la collection RegulatoryTradeIDGrp
        /// pour un type donné  (RegulatoryTradeIDTypeEnum)
        /// </summary>
        /// <param name="pRegulatoryTradeIDType">Type</param>
        // EG 20240227 [WI855] New
        void RemoveRegulatoryTradeIDGrp(RegulatoryTradeIDTypeEnum pRegulatoryTradeIDType);
        // EG 20240227 [WI858] New
        void SetMarketTransactionId(string pValue);
        #endregion
    }
    #endregion IFixTradeCaptureReport

    #region IFixTrdRegTimestamps
    /// EG 20170918 [23452] New
    public interface IFixTrdRegTimestamps
    {
        #region Accessors
        bool TSSpecified { set; get; }
        EFS_DateTimeOffset TS { set; get; }

        bool TypSpecified { set; get; }
        TrdRegTimestampTypeEnum Typ { set; get; }
        #endregion Accessors
    }
    #endregion IFixTrdRegTimestamps

    #region IFixPositionReport and including interfaces

    /// <summary>
    /// Interface hsoting the properties for a position report quantity element
    /// </summary>
    public interface IFixQuantity
    {
        decimal Long { set; get;}

        decimal Short { set; get;}

        /// <summary>
        /// Quantity specific attribute : allocation, end-of-day, exercise, assignation, etc...
        /// </summary>
        /// <remarks>http://www.fixprotocol.org/FIXimate3.0/en/FIX.5.0SP2/tag703.html</remarks>
        PosType Typ { set; get; }

        /// <summary>
        /// Quantity Date
        /// </summary>
        /// <remarks>http://www.fixprotocol.org/FIXimate3.0/en/FIX.5.0SP2/tag976.html</remarks>
        DateTime QuantityDate { set; get; }
    }

    /// <summary>
    /// Interface hosting the property elements of a FIX position report element
    /// </summary>
    /// <remarks>
    /// fixml:PosRpt 
    /// </remarks>
    public interface IFixPositionReport
    {
        /// <summary>
        /// Position owner
        /// </summary>
        IFixParty Pty { set; get;}

        /// <summary>
        /// Instrument asset property relative to the position
        /// </summary>
        /// <remarks>
        /// fixml:Instrmt 
        /// </remarks>
        IFixInstrument Instrmt { set; get; }

        /// <summary>
        /// Quantité en position
        /// </summary>
        IFixQuantity Qty { set; get; }
    }
    #endregion IFixPositionReport and relative elements
}
