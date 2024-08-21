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
    #region Choice Matrix Type
    #region EFS_DebtSecurityTransactionAmountChoiceType
    [System.Xml.Serialization.XmlTypeAttribute(IncludeInSchema = false)]
    public enum EFS_DebtSecurityTransactionAmountChoiceType
    {
        amount,
        grossAmount,
        cleanAmount,
        accruedInterestAmount,
        linkedProductClosingAmounts,
    }
    #endregion EFS_DebtSecurityTransactionAmountChoiceType
    #endregion Choice Matrix Type

    #region Product
    #region EFS_EventMatrixBuyAndSellBack
    public class EFS_EventMatrixBuyAndSellBack : EFS_EventMatrixProduct
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("repoDuration", typeof(EFS_EventMatrixRepurchaseAgreementDuration))]
        public EFS_EventMatrixGroup[] group;
        #endregion Members
        #region Constructors
        public EFS_EventMatrixBuyAndSellBack() { productName = "BuyAndSellBack"; }
        #endregion Constructors
    }
    #endregion EFS_EventMatrixBuyAndSellBack
    #region EFS_EventMatrixRepo
    public class EFS_EventMatrixRepo : EFS_EventMatrixProduct
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("repoDuration", typeof(EFS_EventMatrixRepurchaseAgreementDuration))]
        public EFS_EventMatrixGroup[] group;
        #endregion Members
        #region Constructors
        public EFS_EventMatrixRepo() { productName = "Repo"; }
        #endregion Constructors
    }
    #endregion EFS_EventMatrixRepo
    #region EFS_EventMatrixDebtSecurity
    public class EFS_EventMatrixDebtSecurity : EFS_EventMatrixProduct
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("debtSecurityStream", typeof(EFS_EventMatrixStream))]
        public EFS_EventMatrixGroup[] group;
        #endregion Members
        #region Constructors
        public EFS_EventMatrixDebtSecurity() { productName = "DebtSecurity"; }
        #endregion Constructors
    }
    #endregion EFS_EventMatrixDebtSecurity
    #region EFS_EventMatrixDebtSecurityTransaction
    public class EFS_EventMatrixDebtSecurityTransaction : EFS_EventMatrixProduct
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("debtSecurityTransactionAmounts", typeof(EFS_EventMatrixDebtSecurityTransactionAmounts))]
        [System.Xml.Serialization.XmlElementAttribute("debtSecurityTransactionStream", typeof(EFS_EventMatrixStream))]
        public EFS_EventMatrixGroup[] group;
        #endregion Members
        #region Constructors
        public EFS_EventMatrixDebtSecurityTransaction() { productName = "DebtSecurityTransaction"; }
        #endregion Constructors
    }
    #endregion EFS_EventMatrixDebtSecurityTransaction
    #endregion Product

    #region EFS_EventMatrixDebtSecurityTransactionAmounts
    public class EFS_EventMatrixDebtSecurityTransactionAmounts : EFS_EventMatrixGroup
    {
        [System.Xml.Serialization.XmlElementAttribute("amount", typeof(EFS_EventMatrixGroup))]
        [System.Xml.Serialization.XmlElementAttribute("grossAmount", typeof(EFS_EventMatrixGroup))]
        [System.Xml.Serialization.XmlElementAttribute("cleanAmount", typeof(EFS_EventMatrixGroup))]
        [System.Xml.Serialization.XmlElementAttribute("accruedInterestAmount", typeof(EFS_EventMatrixGroup))]
        [System.Xml.Serialization.XmlElementAttribute("linkedProductClosingAmounts", typeof(EFS_EventMatrixGroup))]
        [System.Xml.Serialization.XmlChoiceIdentifierAttribute("itemsElementName")]
        public EFS_EventMatrixGroup[] group;
        [System.Xml.Serialization.XmlElementAttribute(IsNullable = false)]
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_DebtSecurityTransactionAmountChoiceType[] itemsElementName;

        public EFS_EventMatrixDebtSecurityTransactionAmounts() { }
    }
    #endregion EFS_EventMatrixDebtSecurityTransactionAmounts
    #region EFS_EventMatrixRepurchaseAgreementDuration
    public class EFS_EventMatrixRepurchaseAgreementDuration : EFS_EventMatrixGroup
    {
        [System.Xml.Serialization.XmlElementAttribute("cashStream", typeof(EFS_EventMatrixStream))]
        [System.Xml.Serialization.XmlElementAttribute("securityLeg", typeof(EFS_EventMatrixDebtSecurityTransaction))]
        public EFS_EventMatrixGroup[] group;

        public EFS_EventMatrixRepurchaseAgreementDuration() { }
    }
    #endregion EFS_EventMatrixRepurchaseAgreementDuration
}
