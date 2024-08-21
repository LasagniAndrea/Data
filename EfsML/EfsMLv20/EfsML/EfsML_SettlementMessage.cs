using System;
using System.Text;
using System.Reflection;
using System.Data;

using EFS.ACommon;
using EFS.ApplicationBlocks.Data;
using EFS.Common;


using EFS.GUI;
using EFS.GUI.Attributes;
using EFS.GUI.Interface;
using EFS.GUI.ComplexControls;

using EfsML;
using EfsML.DynamicData;
using EfsML.Enum;
using EfsML.Interface;
using EfsML.Settlement;

using FpML.Enum;
using FpML.Interface;

using FpML.v42.Doc;
using FpML.v42.Shared;

using EfsML.Settlement.Message;   



namespace EfsML.v20.Settlement.Message
{
    #region SettlementMessageDocument
    /// <summary>
    /// Modelisation d'un message de règlement
    /// Contient un header et n payments issus de trade potentiellement différent
    /// </summary>
    /// <revision>
    ///     <version>2.0.1.0</version>
    ///     <date>20080317</date><author>FI</author>
    ///     <comment>Ticket 16132 suppression du membre m_Document</comment>
    /// </revision>
    [System.Xml.Serialization.XmlRootAttribute("settlementMessage",  Namespace = "http://www.efs.org/2005/EFSmL-2-0", IsNullable = false)]
    public partial class SettlementMessageDocument
    {
        #region Members
        //Version du document
        [System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public EfsMLDocumentVersionEnum EfsMLversion;
        //
        [System.Xml.Serialization.XmlElementAttribute("header",Order=1)]
        public SettlementMessageHeader header;
        //
        //Les paiements contenus dans le message
        [System.Xml.Serialization.XmlElementAttribute("payment", Order = 2)]
        public SettlementMessagePayment[] payment;
        #endregion Members
    }
    #endregion SettlementMessageDocument

    #region SettlementMessageHeader
    public partial class SettlementMessageHeader
    {
        #region Members
        //IdMSO => Id unique du message 
        [System.Xml.Serialization.XmlAttributeAttribute("OTCmlId", DataType = "normalizedString")]
        public string otcmlId;
        [System.Xml.Serialization.XmlElementAttribute("settlementMessageId", Order = 1)]
        public SettlementMessageId[] settlementMessageId;
        [System.Xml.Serialization.XmlElementAttribute("creationTimestamp", Order = 2)]
        public EFS_DateTime creationTimestamp;
        [System.Xml.Serialization.XmlElementAttribute("valueDate", Order = 3)]
        public EFS_Date valueDate;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool sumOfPaymentAmountsSpecified;
        [System.Xml.Serialization.XmlElementAttribute("sumOfPaymentAmounts", Order = 4)]
        public EFS_Decimal sumOfPaymentAmounts;
        [System.Xml.Serialization.XmlElementAttribute("sender", Order = 5)]
        public Routing sender;
        [System.Xml.Serialization.XmlElementAttribute("receiver", Order = 6)]
        public Routing receiver;
        #endregion Members
    }
    #endregion SettlementMessageHeader

    #region SettlementMessagePayment
    public partial class SettlementMessagePayment
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("paymentId", Order = 1)]
        public PaymentId paymentId;
        //Payer du paiement
        [System.Xml.Serialization.XmlElementAttribute("payer", Order = 2)]
        public SettlementMessagePartyPayment payer;
        //Receiver du paiement
        [System.Xml.Serialization.XmlElementAttribute("receiver", Order = 3)]
        public SettlementMessagePartyPayment receiver;
        //Date du paiement
        [System.Xml.Serialization.XmlElementAttribute("valueDate", Order = 4)]
        public EFS_Date valueDate;
        //Montant du paiement
        [System.Xml.Serialization.XmlElementAttribute("paymentAmount", Order = 5)]
        public Money paymentAmount;
        [System.Xml.Serialization.XmlElementAttribute("nettingInformation", Order = 6)]
        public NettingInformation nettingInformation;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool dataDocumentSpecified;
        [System.Xml.Serialization.XmlElementAttribute("dataDocument", Order = 7)]
        public DataDocument dataDocument;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool eventsSpecified;
        [System.Xml.Serialization.XmlElementAttribute("Events", Order = 8)]
        public EventItems events;
        #endregion Members
    }
    #endregion SettlementMessagePayment

    #region SettlementMessagePartyPayment
    /// <summary>
    /// Represente les paries impliquées dans un payment
    /// </summary>
    public partial class SettlementMessagePartyPayment : Routing
    {
        #region Members
        //Identification du trade pour la Party 
        //si le Payment a pour origine un unique Trade (n'est pas issu d'un netting multi-trade)
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool tradeIdSpecified;
        [System.Xml.Serialization.XmlElementAttribute("tradeId", Order = 4)]
        public TradeId[] tradeId;
        //
        //Instructions de règlement associés à la party
        [System.Xml.Serialization.XmlElementAttribute("settlementInstruction", Order = 5)]
        public EfsSettlementInstruction settlementInstruction;
        #endregion Members
    }
    #endregion SettlementMessagePartyPayment

}