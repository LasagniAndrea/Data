#region using directives
using System;
using System.Data;
using System.IO;
using System.Text;
using System.Collections;
using System.Xml.Serialization;
using System.Reflection;

using EFS.ACommon;
using EFS.Common;
using EFS.Actor;
using EFS.ApplicationBlocks.Data;

using EFS.GUI.Interface;

using EfsML.Business;
using EfsML.Enum;
using EfsML.Interface;
using EfsML.Settlement;

using EfsML.v20;

using FpML.Enum;
using FpML.Interface;

using FpML.v42.Enum;
using FpML.v42.Shared;
using FpML.v42.Doc;

using EfsML.Settlement.Message;
#endregion using directives


namespace EfsML.v20.Settlement.Message
{
    #region SettlementMessageDocument
    ///  <revision>
    ///     <version>2.0.1.0</version>
    ///     <date>20080317</date><author>FI</author>
    ///     <comment>Ticket 16132 suppression du membre m_Document</comment>
    /// </revision>
    public partial class SettlementMessageDocument : ISettlementMessageDocument
    {

        #region Constructor
        public SettlementMessageDocument()
        {
            EfsMLversion = EfsMLDocumentVersionEnum.Version20;
        }
        #endregion Constructor

        #region ISettlementMessageDocument Membres
        EfsMLDocumentVersionEnum ISettlementMessageDocument.EfsMLversion
        {
            get { return this.EfsMLversion; }
            set { this.EfsMLversion = value; }
        }
        ISettlementMessageHeader ISettlementMessageDocument.header
        {
            get { return this.header; }
            set { this.header = (SettlementMessageHeader)value; }
        }
        ISettlementMessagePayment[] ISettlementMessageDocument.payment
        {
            get { return this.payment; }
            set { this.payment = (SettlementMessagePayment[])value; }
        }
        ISettlementMessageHeader ISettlementMessageDocument.CreateSettlementMessageHeader()
        {
            return new SettlementMessageHeader();
        }
        ISettlementMessagePayment[] ISettlementMessageDocument.CreateSettlementMessagePayment(int pNumber)
        {
            SettlementMessagePayment[] ret = new SettlementMessagePayment[pNumber];
            if (ArrFunc.IsFilled(ret))
            {
                for (int i = 0; i < ArrFunc.Count(ret); i++)
                {
                    ret[i] = new SettlementMessagePayment();
                }
            }
            return ret;
        }
        ISettlementMessagePartyPayment ISettlementMessageDocument.CreateSettlementMessagePartyPayment()
        {
            return ((IProductBase)new Product()).CreateSettlementMessagePartyPayment();
        }
        IMoney ISettlementMessageDocument.CreateMoney(decimal pAmount, string pCurrency)
        {
            return ((IProductBase)new Product()).CreateMoney(pAmount, pCurrency);
        }
        IRoutingCreateElement ISettlementMessageDocument.CreateRoutingCreateElement()
        {
            return new RoutingCreateElement(); 
        }
        #endregion

    }
    #endregion SettlementMessageDocument

    #region SettlementMessageHeader
    public partial class SettlementMessageHeader : ISettlementMessageHeader 
    {
        #region Accessors
        #region OTCmlId
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public int OTCmlId
        {
            get { return Convert.ToInt32(otcmlId); }
            set { otcmlId = value.ToString(); }
        }
        #endregion OTCmlId
        #endregion Accessors
        #region Constructor
        public SettlementMessageHeader() { }
        #endregion Constructor

        #region ISpheresId Membres
        int ISpheresId.OTCmlId
        {
            get { return this.OTCmlId; }
            set { this.OTCmlId = value; }
        }
        string ISpheresId.otcmlId
        {
            get { return otcmlId; }
            set { otcmlId = value; }
        }
        #endregion
        #region ISettlementMessageHeader Membres
        SettlementMessageId[] ISettlementMessageHeader.settlementMessageId
        {
            get { return this.settlementMessageId; }
            set { this.settlementMessageId = value; }
        }
        EFS_DateTime ISettlementMessageHeader.creationTimestamp
        {
            get { return this.creationTimestamp; }
            set { this.creationTimestamp = value; }
        }
        EFS_Date ISettlementMessageHeader.valueDate
        {
            get { return this.valueDate; }
            set { this.valueDate = value; }
        }
        bool ISettlementMessageHeader.sumOfPaymentAmountsSpecified
        {
            get { return this.sumOfPaymentAmountsSpecified; }
            set { this.sumOfPaymentAmountsSpecified = value; }
        }
        EFS_Decimal ISettlementMessageHeader.sumOfPaymentAmounts
        {
            get { return this.sumOfPaymentAmounts; }
            set { this.sumOfPaymentAmounts = value; }
        }
        IRouting ISettlementMessageHeader.sender
        {
            get { return this.sender; }
            set { this.sender = (Routing)value; }
        }
        IRouting ISettlementMessageHeader.receiver
        {
            get { return this.receiver; }
            set { this.receiver = (Routing)value; }
        }
        #endregion
    }
    #endregion SettlementMessageHeader

    #region SettlementMessagePayment
    /// <summary>
    /// 
    /// </summary>
    /// <revision>
    ///     <version>2.0.1.0</version><date>20080318</date>
    ///     <author>FI</author>
    ///     <comment>Ticket 16132 suppression du membre m_Document</comment>
    /// </revision>
    public partial class SettlementMessagePayment : ISettlementMessagePayment
    {
        #region Accessors

        #endregion Accessors
        #region Constructor
        public SettlementMessagePayment() { }
        #endregion
        //
        #region ISettlementMessagePayment Membres
        PaymentId ISettlementMessagePayment.paymentId
        {
            get { return this.paymentId; }
            set { paymentId = value; }
        }
        ISettlementMessagePartyPayment ISettlementMessagePayment.payer
        {
            get { return this.payer; }
            set { this.payer = (SettlementMessagePartyPayment)value; }
        }
        ISettlementMessagePartyPayment ISettlementMessagePayment.receiver
        {
            get { return this.receiver; }
            set { this.receiver = (SettlementMessagePartyPayment)value; }
        }
        EFS_Date ISettlementMessagePayment.valueDate
        {
            get { return this.valueDate; }
            set { valueDate = value; }
        }
        IMoney ISettlementMessagePayment.paymentAmount
        {
            get { return this.paymentAmount; }
            set { this.paymentAmount = (Money)value; }
        }
        NettingInformation ISettlementMessagePayment.nettingInformation
        {
            get { return this.nettingInformation; }
            set { this.nettingInformation = value; }
        }
        bool ISettlementMessagePayment.dataDocumentSpecified
        {
            get { return this.dataDocumentSpecified; }
            set { dataDocumentSpecified = value; }
        }
        IDataDocument ISettlementMessagePayment.dataDocument
        {
            get { return this.dataDocument; }
            set { this.dataDocument = (DataDocument)value; }
        }
        bool ISettlementMessagePayment.eventsSpecified
        {
            get { return this.eventsSpecified; }
            set { eventsSpecified = value; }
        }
        EventItems ISettlementMessagePayment.events
        {
            get { return this.events; }
            set { this.events = value; }
        }
        #endregion

    }
    #endregion SettlementMessagePayment

    #region SettlementMessagePartyPayment
    /// <summary>
    /// Represente les paries impliquées dans un payment
    /// </summary>
    public partial class SettlementMessagePartyPayment : ISettlementMessagePartyPayment
    {
        #region constructor
        public SettlementMessagePartyPayment() { }
        #endregion Constructor

        #region Methods
        #region LoadTradeId
        // EG 20180426 Analyse du code Correction [CA2202]
        public void LoadTradeId(string pCs, int pIdT)
        {
            int idA = new RoutingContainer((IRouting)this).GetRoutingIdA(pCs);
            if (0 < idA)
            {
                DataParameters dataParameters = new DataParameters();
                dataParameters.Add(new DataParameter(pCs, "IDA", DbType.Int32), idA);
                dataParameters.Add(new DataParameter(pCs, "IDT", DbType.Int32), pIdT);

                StrBuilder sql = new StrBuilder();
                sql += SQLCst.SELECT + "tid.TRADEID, tid.TRADEIDSCHEME" + Cst.CrLf;
                sql += SQLCst.FROM_DBO + Cst.OTCml_TBL.TRADEID + " tid " + Cst.CrLf;

                sql += SQLCst.WHERE + " tid.IDA= @IDA" + Cst.CrLf;
                sql += SQLCst.AND + " tid.IDT= @IDT" + Cst.CrLf;

                using (IDataReader dr = DataHelper.ExecuteReader(pCs, CommandType.Text, sql.ToString(), dataParameters.GetArrayDbParameter()))
                {
                    while (dr.Read())
                    {
                        ReflectionTools.AddItemInArray(this, "tradeId", 0);
                        tradeId[tradeId.Length - 1].Value = dr.GetString(0);
                        tradeId[tradeId.Length - 1].tradeIdScheme = dr.GetString(1);
                    }
                }
            }
            tradeIdSpecified = ArrFunc.IsFilled(tradeId);
        }
        #endregion LoadTradeId
        #endregion Methods

        #region ISettlementMessagePartyPayment Members
        bool ISettlementMessagePartyPayment.tradeIdSpecified
        {
            get { return this.tradeIdSpecified; }
            set { this.tradeIdSpecified = value; }
        }
        ISchemeId[] ISettlementMessagePartyPayment.tradeId
        {
            get { return (ISchemeId[])this.tradeId; }
            set { this.tradeId = (TradeId[])value; }
        }
        IEfsSettlementInstruction ISettlementMessagePartyPayment.settlementInstruction
        {
            get { return this.settlementInstruction; }
            set { this.settlementInstruction = (EfsSettlementInstruction)value; }
        }
        void ISettlementMessagePartyPayment.LoadTradeId(string pCs, int pIdT)
        {
            this.LoadTradeId(pCs, pIdT);
        }
        #endregion ISettlementMessagePartyPayment Members

    }
    #endregion SettlementMessagePartyPayment

}
