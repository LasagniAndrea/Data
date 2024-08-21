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
    #region EFS_EquitySecurityTransactionAmountChoiceType
    [System.Xml.Serialization.XmlTypeAttribute(IncludeInSchema = false)]
    /// EG 20150306 [POC-BERKELEY] : Add marginRatio
    public enum EFS_EquitySecurityTransactionAmountChoiceType
    {
        initialQty,
        grossAmount,
        marginRatio,
        linkedProductClosingAmounts,
    }
    #endregion EFS_EquitySecurityTransactionAmountChoiceType
    #endregion Choice Matrix Type

    #region Product
    #region EFS_EventMatrixEquitySecurityTransaction
    public class EFS_EventMatrixEquitySecurityTransaction : EFS_EventMatrixProduct
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("equitySecurityTransactionStream", typeof(EFS_EventMatrixEquitySecurityTransactionStream))]
        public EFS_EventMatrixGroup[] group;
        #endregion Members
        #region Constructors
        public EFS_EventMatrixEquitySecurityTransaction() { productName = "EquitySecurityTransaction"; }
        #endregion Constructors
    }
    #endregion EFS_EventMatrixDebtSecurityTransaction
    #endregion Product

    #region EFS_EventMatrixEquitySecurityTransactionAmounts
    /// EG 20150306 [POC-BERKELEY] : Add marginRatio
    public class EFS_EventMatrixEquitySecurityTransactionStream : EFS_EventMatrixGroup
    {
        [System.Xml.Serialization.XmlElementAttribute("initialQty", typeof(EFS_EventMatrixGroup))]
        [System.Xml.Serialization.XmlElementAttribute("grossAmount", typeof(EFS_EventMatrixGroup))]
        [System.Xml.Serialization.XmlElementAttribute("marginRatio", typeof(EFS_EventMatrixGroup))]
        [System.Xml.Serialization.XmlElementAttribute("linkedProductClosingAmounts", typeof(EFS_EventMatrixGroup))]
        [System.Xml.Serialization.XmlChoiceIdentifierAttribute("itemsElementName")]
        public EFS_EventMatrixGroup[] group;
        [System.Xml.Serialization.XmlElementAttribute(IsNullable = false)]
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_EquitySecurityTransactionAmountChoiceType[] itemsElementName;

        public EFS_EventMatrixEquitySecurityTransactionStream() { }
    }
    #endregion EFS_EventMatrixDebtSecurityTransactionAmounts
}
