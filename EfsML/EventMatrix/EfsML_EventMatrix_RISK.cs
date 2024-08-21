#region Using Directives
using System;
using System.Collections;
using System.Reflection;

using EFS.ACommon;
using EFS.Common;
using EFS.GUI.Interface;

using EfsML.Business;
using EfsML.Enum;
using EfsML.Enum.Tools;

using FpML.Interface;
#endregion Using Directives

namespace EfsML.EventMatrix
{
    /// <summary>
    /// 
    /// </summary>
    [System.Xml.Serialization.XmlTypeAttribute(IncludeInSchema = false)]
    public enum EFS_CashAvailableChoiceType
    {
        previousCashBalance,
        cashBalancePayment,
        cashFlowConstituent
    }

    [System.Xml.Serialization.XmlTypeAttribute(IncludeInSchema = false)]
    // PM 20150709 [21103] Add safekeeping
    // PM 20170911 [23408] Ajout Equalisation Payment
    public enum EFS_CashFlowConstituentChoiceType
    {
        variationMargin,
        premium,
        cashSettlement,
        fee,
        safekeeping,
        equalisationPayment
    }

    /// <summary>
    /// 
    /// </summary>
    // PM 20150324 [POC] Add borrowing
    // PM 20150330 Add unsettledTransaction
    // PM 20150616 [21124] Add marketValue, totalAccountValue
    [System.Xml.Serialization.XmlTypeAttribute(IncludeInSchema = false)]
    public enum EFS_CashBalanceStreamChoiceType
    {
        borrowing,
        cashAvailable,
        cashBalance,
        cashDeposit,
        cashUsed,
        cashWithdrawal,
        collateralAvailable,
        collateralUsed,
        equityBalance,
        equityBalanceWithForwardCash,
        excessDeficit,
        excessDeficitWithForwardCash,
        forwardCashPayment,
        funding,
        futureExchangeTradedDerivative,
        futuresStyleOption,
        longOptionValue,
        marginCall,
        marginRequirement,
        marketValue,
        premiumStyleOption,
        realizedMargin,
        shortOptionValue,
        totalAccountValue,
        uncoveredMarginRequirement,
        unrealizedMargin,
        unsettledTransaction,
    }

    /// <summary>
    /// 
    /// </summary>
    // PM 20150324 [POC] Add borrowing
    // PM 20150330 Add unsettledTransaction
    // PM 20150616 [21124] Add marketValue, totalAccountValue
    [System.Xml.Serialization.XmlTypeAttribute(IncludeInSchema = false)]
    public enum EFS_ExchangeCashBalanceStreamChoiceType
    {
        borrowing,
        cashAvailable,
        cashBalance,
        cashDeposit,
        cashUsed,
        cashWithdrawal,
        collateralAvailable,
        collateralUsed,
        equityBalance,
        equityBalanceWithForwardCash,
        excessDeficit,
        excessDeficitWithForwardCash,
        forwardCashPayment,
        funding,
        futureExchangeTradedDerivative,
        futuresStyleOption,
        longOptionValue,
        marginCall,
        marginRequirement,
        marketValue,
        premiumStyleOption,
        realizedMargin,
        shortOptionValue,
        totalAccountValue,
        uncoveredMarginRequirement,
        unrealizedMargin,
        unsettledTransaction,
    }

    /// <summary>
    /// 
    /// </summary>
    [System.Xml.Serialization.XmlTypeAttribute(IncludeInSchema = false)]
    public enum EFS_MarginConstituentChoiceType
    {
        realizedMargin,
        unrealizedMargin,
    }


    /// <summary>
    /// 
    /// </summary>
    public class EFS_EventMatrixMarginRequirement : EFS_EventMatrixProduct
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("groupLevel", typeof(EFS_EventMatrixGroupLevel))]
        public EFS_EventMatrixGroup[] group;
        #endregion Members
        #region Constructors
        public EFS_EventMatrixMarginRequirement() { productName = "MarginRequirement"; }
        #endregion Constructors
    }

    /// <summary>
    /// 
    /// </summary>
    public class EFS_EventMatrixCashBalance : EFS_EventMatrixProduct
    {
        [System.Xml.Serialization.XmlElementAttribute("cashBalanceStream", typeof(EFS_EventMatrixCashBalanceStream))]
        [System.Xml.Serialization.XmlElementAttribute("exchangeCashBalanceStream", typeof(EFS_EventMatrixExchangeCashBalanceStream))]
        public EFS_EventMatrixGroup[] group;

        #region Constructors
        public EFS_EventMatrixCashBalance() { productName = "CashBalance"; }
        #endregion Constructors
    
    }

    /// <summary>
    /// 
    /// </summary>
    public class EFS_EventMatrixCashBalanceStream : EFS_EventMatrixGroup
    {
        #region Members
        // PM 20150324 [POC] Add borrowing
        // PM 20150330 Add unsettledTransaction
        // PM 20150616 [21124] Add marketValue & totalAccountValue
        [System.Xml.Serialization.XmlElementAttribute("marginRequirement", typeof(EFS_EventMatrixGroup))]
        [System.Xml.Serialization.XmlElementAttribute("cashAvailable", typeof(EFS_EventMatrixCashAvailable))]
        [System.Xml.Serialization.XmlElementAttribute("cashUsed", typeof(EFS_EventMatrixGroup))]
        [System.Xml.Serialization.XmlElementAttribute("collateralAvailable", typeof(EFS_EventMatrixGroup))]
        [System.Xml.Serialization.XmlElementAttribute("collateralUsed", typeof(EFS_EventMatrixGroup))]
        [System.Xml.Serialization.XmlElementAttribute("uncoveredMarginRequirement", typeof(EFS_EventMatrixGroup))]
        [System.Xml.Serialization.XmlElementAttribute("marginCall", typeof(EFS_EventMatrixGroup))]
        [System.Xml.Serialization.XmlElementAttribute("cashBalance", typeof(EFS_EventMatrixGroup))]
        [System.Xml.Serialization.XmlElementAttribute("funding", typeof(EFS_EventMatrixGroup))]
        [System.Xml.Serialization.XmlElementAttribute("borrowing", typeof(EFS_EventMatrixGroup))]
        [System.Xml.Serialization.XmlElementAttribute("forwardCashPayment", typeof(EFS_EventMatrixGroup))]
        [System.Xml.Serialization.XmlElementAttribute("equityBalance", typeof(EFS_EventMatrixGroup))]
        [System.Xml.Serialization.XmlElementAttribute("equityBalanceWithForwardCash", typeof(EFS_EventMatrixGroup))]
        [System.Xml.Serialization.XmlElementAttribute("excessDeficit", typeof(EFS_EventMatrixGroup))]
        [System.Xml.Serialization.XmlElementAttribute("excessDeficitWithForwardCash", typeof(EFS_EventMatrixGroup))]
        [System.Xml.Serialization.XmlElementAttribute("longOptionValue", typeof(EFS_EventMatrixGroup))]
        [System.Xml.Serialization.XmlElementAttribute("shortOptionValue", typeof(EFS_EventMatrixGroup))]
        [System.Xml.Serialization.XmlElementAttribute("realizedMargin", typeof(EFS_EventMatrixGroup))]
        [System.Xml.Serialization.XmlElementAttribute("unrealizedMargin", typeof(EFS_EventMatrixGroup))]
        [System.Xml.Serialization.XmlElementAttribute("cashDeposit", typeof(EFS_EventMatrixGroup))]
        [System.Xml.Serialization.XmlElementAttribute("cashWithdrawal", typeof(EFS_EventMatrixGroup))]
        [System.Xml.Serialization.XmlElementAttribute("futureExchangeTradedDerivative", typeof(EFS_EventMatrixMarginConstituent))]
        [System.Xml.Serialization.XmlElementAttribute("futuresStyleOption", typeof(EFS_EventMatrixMarginConstituent))]
        [System.Xml.Serialization.XmlElementAttribute("premiumStyleOption", typeof(EFS_EventMatrixMarginConstituent))]
        [System.Xml.Serialization.XmlElementAttribute("unsettledTransaction", typeof(EFS_EventMatrixGroup))]
        [System.Xml.Serialization.XmlElementAttribute("marketValue", typeof(EFS_EventMatrixGroup))]
        [System.Xml.Serialization.XmlElementAttribute("totalAccountValue", typeof(EFS_EventMatrixGroup))]
        [System.Xml.Serialization.XmlChoiceIdentifierAttribute("itemsElementName")]
        public EFS_EventMatrixGroup[] group;
        [System.Xml.Serialization.XmlElementAttribute(IsNullable = false)]
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_CashBalanceStreamChoiceType[] itemsElementName;
        #endregion Members
    }

    /// <summary>
    /// 
    /// </summary>
    public class EFS_EventMatrixExchangeCashBalanceStream : EFS_EventMatrixGroup
    {
        #region Members
        // PM 20150324 [POC] Add borrowing
        // PM 20150330 Add unsettledTransaction
        // PM 20150616 [21124] Add marketValue & totalAccountValue
        [System.Xml.Serialization.XmlElementAttribute("marginRequirement", typeof(EFS_EventMatrixGroup))]
        [System.Xml.Serialization.XmlElementAttribute("cashAvailable", typeof(EFS_EventMatrixCashAvailable))]
        [System.Xml.Serialization.XmlElementAttribute("cashUsed", typeof(EFS_EventMatrixGroup))]
        [System.Xml.Serialization.XmlElementAttribute("collateralAvailable", typeof(EFS_EventMatrixGroup))]
        [System.Xml.Serialization.XmlElementAttribute("collateralUsed", typeof(EFS_EventMatrixGroup))]
        [System.Xml.Serialization.XmlElementAttribute("uncoveredMarginRequirement", typeof(EFS_EventMatrixGroup))]
        [System.Xml.Serialization.XmlElementAttribute("marginCall", typeof(EFS_EventMatrixGroup))]
        [System.Xml.Serialization.XmlElementAttribute("cashBalance", typeof(EFS_EventMatrixGroup))]
        [System.Xml.Serialization.XmlElementAttribute("funding", typeof(EFS_EventMatrixGroup))]
        [System.Xml.Serialization.XmlElementAttribute("borrowing", typeof(EFS_EventMatrixGroup))]
        [System.Xml.Serialization.XmlElementAttribute("forwardCashPayment", typeof(EFS_EventMatrixGroup))]
        [System.Xml.Serialization.XmlElementAttribute("equityBalance", typeof(EFS_EventMatrixGroup))]
        [System.Xml.Serialization.XmlElementAttribute("equityBalanceWithForwardCash", typeof(EFS_EventMatrixGroup))]
        [System.Xml.Serialization.XmlElementAttribute("excessDeficit", typeof(EFS_EventMatrixGroup))]
        [System.Xml.Serialization.XmlElementAttribute("excessDeficitWithForwardCash", typeof(EFS_EventMatrixGroup))]
        [System.Xml.Serialization.XmlElementAttribute("longOptionValue", typeof(EFS_EventMatrixGroup))]
        [System.Xml.Serialization.XmlElementAttribute("shortOptionValue", typeof(EFS_EventMatrixGroup))]
        [System.Xml.Serialization.XmlElementAttribute("realizedMargin", typeof(EFS_EventMatrixGroup))]
        [System.Xml.Serialization.XmlElementAttribute("unrealizedMargin", typeof(EFS_EventMatrixGroup))]
        [System.Xml.Serialization.XmlElementAttribute("cashDeposit", typeof(EFS_EventMatrixGroup))]
        [System.Xml.Serialization.XmlElementAttribute("cashWithdrawal", typeof(EFS_EventMatrixGroup))]
        [System.Xml.Serialization.XmlElementAttribute("futureExchangeTradedDerivative", typeof(EFS_EventMatrixMarginConstituent))]
        [System.Xml.Serialization.XmlElementAttribute("futuresStyleOption", typeof(EFS_EventMatrixMarginConstituent))]
        [System.Xml.Serialization.XmlElementAttribute("premiumStyleOption", typeof(EFS_EventMatrixMarginConstituent))]
        [System.Xml.Serialization.XmlElementAttribute("unsettledTransaction", typeof(EFS_EventMatrixGroup))]
        [System.Xml.Serialization.XmlElementAttribute("marketValue", typeof(EFS_EventMatrixGroup))]
        [System.Xml.Serialization.XmlElementAttribute("totalAccountValue", typeof(EFS_EventMatrixGroup))]
        [System.Xml.Serialization.XmlChoiceIdentifierAttribute("itemsElementName")]
        public EFS_EventMatrixGroup[] group;
        [System.Xml.Serialization.XmlElementAttribute(IsNullable = false)]
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_ExchangeCashBalanceStreamChoiceType[] itemsElementName;
        #endregion Members
    }

    /// <summary>
    /// 
    /// </summary>
    public class EFS_EventMatrixCashAvailable : EFS_EventMatrixGroup
    {
        [System.Xml.Serialization.XmlElementAttribute("previousCashBalance", typeof(EFS_EventMatrixGroup))]
        [System.Xml.Serialization.XmlElementAttribute("cashBalancePayment", typeof(EFS_EventMatrixGroup))]
        [System.Xml.Serialization.XmlElementAttribute("cashFlowConstituent", typeof(EFS_EventMatrixCashFlowConstituent))]
        [System.Xml.Serialization.XmlChoiceIdentifierAttribute("itemsElementName")]
        public EFS_EventMatrixGroup[] group;
        [System.Xml.Serialization.XmlElementAttribute(IsNullable = false)]
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_CashAvailableChoiceType[] itemsElementName;
    }

    /// <summary>
    /// 
    /// </summary>
    /// PM 20150709 [21103] Add safekeeping
    /// PM 20170911 [23408] Ajout Equalisation Payment
    public class EFS_EventMatrixCashFlowConstituent : EFS_EventMatrixGroup
    {
        [System.Xml.Serialization.XmlElementAttribute("variationMargin", typeof(EFS_EventMatrixGroup))]
        [System.Xml.Serialization.XmlElementAttribute("premium", typeof(EFS_EventMatrixGroup))]
        [System.Xml.Serialization.XmlElementAttribute("cashSettlement", typeof(EFS_EventMatrixGroup))]
        [System.Xml.Serialization.XmlElementAttribute("fee", typeof(EFS_EventMatrixPayment))]
        [System.Xml.Serialization.XmlElementAttribute("safekeeping", typeof(EFS_EventMatrixPayment))]
        [System.Xml.Serialization.XmlElementAttribute("equalisationPayment", typeof(EFS_EventMatrixPayment))]
        [System.Xml.Serialization.XmlChoiceIdentifierAttribute("itemsElementName")]
        public EFS_EventMatrixGroup[] group;
        [System.Xml.Serialization.XmlElementAttribute(IsNullable = false)]
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_CashFlowConstituentChoiceType[] itemsElementName;
    }

    /// <summary>
    /// 
    /// </summary>
    public class EFS_EventMatrixMarginConstituent : EFS_EventMatrixGroup
    {
        [System.Xml.Serialization.XmlElementAttribute("realizedMargin", typeof(EFS_EventMatrixGroup))]
        [System.Xml.Serialization.XmlElementAttribute("unrealizedMargin", typeof(EFS_EventMatrixGroup))]
        [System.Xml.Serialization.XmlChoiceIdentifierAttribute("itemsElementName")]
        public EFS_EventMatrixGroup[] group;
        [System.Xml.Serialization.XmlElementAttribute(IsNullable = false)]
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_MarginConstituentChoiceType[] itemsElementName;
    }

    #region EFS_EventMatrixCashBalanceInterest
    /// <summary>
    /// Interest on Cash Balance or Cash Deposit
    /// </summary>
    // PM 20120809 [18058] Added
    public class EFS_EventMatrixCashBalanceInterest : EFS_EventMatrixProduct
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("cashBalanceInterestStream", typeof(EFS_EventMatrixStream))]
        public EFS_EventMatrixGroup[] group;
        #endregion Members
        #region Constructors
        public EFS_EventMatrixCashBalanceInterest() { productName = "CashBalanceInterest"; }
        #endregion Constructors
    }
    #endregion EFS_EventMatrixCashBalanceInterest
     
}
