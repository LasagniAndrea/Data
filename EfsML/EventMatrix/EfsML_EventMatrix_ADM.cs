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
    #region Choice Matrix
    #region EFS_CorrectionInvoiceChoiceType
    [System.Xml.Serialization.XmlTypeAttribute(IncludeInSchema = false)]
    public enum EFS_CorrectionInvoiceChoiceType
    {
        turnOverAmount,
        invoiceRebateAmount,
        rebateAmount,
        initialInvoice,
        baseInvoice,
        theoricInvoice,
    }
    #endregion EFS_CorrectionInvoiceChoiceType
    #region EFS_InvoiceSettlementLevelChoiceType
    [System.Xml.Serialization.XmlTypeAttribute(IncludeInSchema = false)]
    public enum EFS_InvoiceSettlementLevelChoiceType
    {
        allocatedAmountDates,
        invoiceSettlementAmount,
    }
    #endregion EFS_InvoiceSettlementLevelChoiceType
    #region EFS_TurnOverAmountChoiceType
    [System.Xml.Serialization.XmlTypeAttribute(IncludeInSchema = false)]
    public enum EFS_TurnOverAmountChoiceType
    {
        detailTurnOverAmount,
        turnOverAmountFixing,
        taxAmount,
    }
    #endregion EFS_TurnOverAmountChoiceType
    #endregion Choice Matrix

    #region Product
    #region EFS_EventMatrixAdditionalInvoice
    public class EFS_EventMatrixAdditionalInvoice : EFS_EventMatrixProduct
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("finalInvoice", typeof(EFS_EventMatrixCorrectionInvoiceLevel))]
        public EFS_EventMatrixGroup[] group;
        #endregion Members
        #region Constructors
        public EFS_EventMatrixAdditionalInvoice() { productName = "AdditionalInvoice"; }
        #endregion Constructors
    }
    #endregion EFS_EventMatrixAdditionalInvoice
    #region EFS_EventMatrixCreditNote
    public class EFS_EventMatrixCreditNote : EFS_EventMatrixProduct
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("finalInvoice", typeof(EFS_EventMatrixCorrectionInvoiceLevel))]
        public EFS_EventMatrixGroup[] group;
        #endregion Members
        #region Constructors
        public EFS_EventMatrixCreditNote() { productName = "CreditNote"; }
        #endregion Constructors
    }
    #endregion EFS_EventMatrixCreditNote
    #region EFS_EventMatrixInvoice
    public class EFS_EventMatrixInvoice : EFS_EventMatrixProduct
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("groupLevel", typeof(EFS_EventMatrixInvoiceLevel))]
        public EFS_EventMatrixGroup[] group;
        #endregion Members
        #region Constructors
        public EFS_EventMatrixInvoice() { productName = "Invoice"; }
        #endregion Constructors
    }
    #endregion EFS_EventMatrixInvoice
    #region EFS_EventMatrixInvoiceSettlement
    public class EFS_EventMatrixInvoiceSettlement : EFS_EventMatrixProduct
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("groupLevel", typeof(EFS_EventMatrixInvoiceSettlementLevel))]
        public EFS_EventMatrixGroup[] group;
        #endregion Members
        #region Constructors
        public EFS_EventMatrixInvoiceSettlement() { productName = "InvoiceSettlement"; }
        #endregion Constructors
    }
    #endregion EFS_EventMatrixInvoice
    #endregion Product

    #region EFS_EventMatrixCorrectionInvoiceLevel
    public class EFS_EventMatrixCorrectionInvoiceLevel : EFS_EventMatrixGroup
    {
        [System.Xml.Serialization.XmlElementAttribute("turnOverAmount", typeof(EFS_EventMatrixTurnOverAmount))]
        [System.Xml.Serialization.XmlElementAttribute("invoiceRebateAmount", typeof(EFS_EventMatrixInvoiceRebateAmount))]
        [System.Xml.Serialization.XmlElementAttribute("rebateAmount", typeof(EFS_EventMatrixGroup))]
        [System.Xml.Serialization.XmlElementAttribute("initialInvoice", typeof(EFS_EventMatrixInvoiceLevel))]
        [System.Xml.Serialization.XmlElementAttribute("baseInvoice", typeof(EFS_EventMatrixInvoiceLevel))]
        [System.Xml.Serialization.XmlElementAttribute("theoricInvoice", typeof(EFS_EventMatrixInvoiceLevel))]
        [System.Xml.Serialization.XmlChoiceIdentifierAttribute("itemsElementName")]
        public EFS_EventMatrixGroup[] group;
        [System.Xml.Serialization.XmlElementAttribute(IsNullable = false)]
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_CorrectionInvoiceChoiceType[] itemsElementName;

        public EFS_EventMatrixCorrectionInvoiceLevel() { }
    }
    #endregion EFS_EventMatrixInvoiceLevel
    #region EFS_EventMatrixDetailTax
    public class EFS_EventMatrixDetailTax : EFS_EventMatrixGroup
    {
        [System.Xml.Serialization.XmlElementAttribute("taxDetails", typeof(EFS_EventMatrixGroup))]
        public EFS_EventMatrixGroup[] group;

        public EFS_EventMatrixDetailTax() { }
    }
    #endregion EFS_EventMatrixDetailTax
    #region EFS_EventMatrixDetailTurnOverAmount
    public class EFS_EventMatrixDetailTurnOverAmount : EFS_EventMatrixGroup
    {
        [System.Xml.Serialization.XmlElementAttribute("detailTurnOverAccountingAmount", typeof(EFS_EventMatrixTurnOverAmount))]
        public EFS_EventMatrixGroup[] group;

        public EFS_EventMatrixDetailTurnOverAmount() { }
    }
    #endregion EFS_EventMatrixDetailTurnOverAmount
    #region EFS_EventMatrixInvoiceLevel
    public class EFS_EventMatrixInvoiceLevel : EFS_EventMatrixGroup
    {
        [System.Xml.Serialization.XmlElementAttribute("turnOverAmount", typeof(EFS_EventMatrixTurnOverAmount))]
        [System.Xml.Serialization.XmlElementAttribute("invoiceRebateAmount", typeof(EFS_EventMatrixInvoiceRebateAmount))]
        [System.Xml.Serialization.XmlElementAttribute("rebateAmount", typeof(EFS_EventMatrixGroup))]
        public EFS_EventMatrixGroup[] group;

        public EFS_EventMatrixInvoiceLevel() { }
    }
    #endregion EFS_EventMatrixInvoiceLevel
    #region EFS_EventMatrixInvoiceRebateAmount
    public class EFS_EventMatrixInvoiceRebateAmount : EFS_EventMatrixGroup
    {
        [System.Xml.Serialization.XmlElementAttribute("rebateCapAmount", typeof(EFS_EventMatrixGroup))]
        [System.Xml.Serialization.XmlElementAttribute("rebateBracketAmount", typeof(EFS_EventMatrixInvoiceRebateBracket))]
        public EFS_EventMatrixGroup[] group;

        public EFS_EventMatrixInvoiceRebateAmount() { }
    }
    #endregion EFS_EventMatrixInvoiceRebateAmount
    #region EFS_EventMatrixInvoiceRebateBracket
    public class EFS_EventMatrixInvoiceRebateBracket : EFS_EventMatrixGroup
    {
        [System.Xml.Serialization.XmlElementAttribute("rebateBracketAmountDetail", typeof(EFS_EventMatrixGroup))]
        public EFS_EventMatrixGroup[] group;

        public EFS_EventMatrixInvoiceRebateBracket() { }
    }
    #endregion EFS_EventMatrixInvoiceRebateBracket
    #region EFS_EventMatrixInvoiceSettlementLevel
    public class EFS_EventMatrixInvoiceSettlementLevel : EFS_EventMatrixGroup
    {
        [System.Xml.Serialization.XmlElementAttribute("invoiceSettlementAmount", typeof(EFS_EventMatrixGroup))]
        [System.Xml.Serialization.XmlElementAttribute("allocatedAmountDates", typeof(EFS_EventMatrixGroup))]
        [System.Xml.Serialization.XmlChoiceIdentifierAttribute("itemsElementName")]
        public EFS_EventMatrixGroup[] group;
        [System.Xml.Serialization.XmlElementAttribute(IsNullable = false)]
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_InvoiceSettlementLevelChoiceType[] itemsElementName;

        public EFS_EventMatrixInvoiceSettlementLevel() { }
    }
    #endregion EFS_EventMatrixInvoiceSettlementLevel
    #region EFS_EventMatrixTurnOverAmount
    public class EFS_EventMatrixTurnOverAmount : EFS_EventMatrixGroup
    {
        [System.Xml.Serialization.XmlElementAttribute("detailTurnOverAmount", typeof(EFS_EventMatrixDetailTurnOverAmount))]
        [System.Xml.Serialization.XmlElementAttribute("turnOverAmountFixing", typeof(EFS_EventMatrixGroup))]
        [System.Xml.Serialization.XmlElementAttribute("taxAmount", typeof(EFS_EventMatrixDetailTax))]
        [System.Xml.Serialization.XmlChoiceIdentifierAttribute("itemsElementName")]
        public EFS_EventMatrixGroup[] group;
        [System.Xml.Serialization.XmlElementAttribute(IsNullable = false)]
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_TurnOverAmountChoiceType[] itemsElementName;


        public EFS_EventMatrixTurnOverAmount() { }
    }
    #endregion EFS_EventMatrixTurnOverAmount
}
