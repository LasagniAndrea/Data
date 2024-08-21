#region Using Directives
using EFS.ACommon;
using EFS.ApplicationBlocks.Data;
using EFS.Common;
using EFS.GUI.Interface;
using EfsML.Enum;
using EfsML.Enum.Tools;
using EfsML.Interface;
using FpML.Enum;
using FpML.Interface;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
#endregion Using Directives

namespace EfsML.Business
{
    #region EFS_DebtSecurityStream
    public class EFS_DebtSecurityStream
    {
        #region Members
        protected string m_Cs;
        protected Cst.ErrLevel m_ErrLevel = Cst.ErrLevel.UNDEFINED;
        private readonly EFS_Asset m_DebtSecurity;
        private readonly EFS_DebtSecurityStreamEvent m_Stream;
        public EFS_DebtSecurityStreamEvent[] nominalStep;
        public EFS_DebtSecurityStreamEvent[] nominal;
        public EFS_DebtSecurityStreamEvent[] interest;
        #endregion Members
        #region Accessors
        #region AdjustedEffectiveDate
        public EFS_Date AdjustedEffectiveDate
        {
            get { return m_Stream.AdjustedStartPeriod; }
        }
        #endregion AdjustedEffectiveDate
        #region AdjustedTerminationDate
        public EFS_Date AdjustedTerminationDate
        {
            get { return m_Stream.AdjustedEndPeriod; }
        }
        #endregion AdjustedTerminationDate
        #region DebtSecurity
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_Asset DebtSecurity
        {
            get { return m_DebtSecurity; }
        }
        #endregion DebtSecurity
        #region EffectiveDate
        public EFS_EventDate EffectiveDate
        {
            get { return m_Stream.StartPeriod; }
        }
        #endregion EffectiveDate
        #region ErrLevel
        public Cst.ErrLevel ErrLevel
        {
            get { return m_ErrLevel; }
        }
        #endregion ErrLevel
        #region EventCode
        public string EventCode
        {
            get { return m_Stream.EventCode; }
        }
        #endregion EventCode
        #region EventType
        public string EventType
        {
            get { return m_Stream.EventType; }
        }
        #endregion EventType
        #region IdE_Source
        public int IdE_Source
        {
            get { return m_Stream.IdE_Source; }
        }
        #endregion IdE_Source

        #region TerminationDate
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_EventDate TerminationDate
        {
            get { return m_Stream.EndPeriod; }
        }
        #endregion TerminationDate
        #endregion Accessors
        #region Constructors
        public EFS_DebtSecurityStream(string pConnectionString, DataRow pRowStream, EFS_Asset pDebtSecurity)
        {
            m_Cs = pConnectionString;
            m_DebtSecurity = pDebtSecurity;
            m_Stream = new EFS_DebtSecurityStreamEvent(pConnectionString, pRowStream);
        }
        // EG 20150907 [21317] add RecordDate test
        public EFS_DebtSecurityStream(string pConnectionString, DataRow pRowStream, EFS_Payment pGrossAmount, 
            IOrderQuantity pOrderQuantity, EFS_Asset pDebtSecurity)
        {
            m_Cs = pConnectionString;
            m_DebtSecurity = pDebtSecurity;
            m_Stream = new EFS_DebtSecurityStreamEvent(pConnectionString, pRowStream);
            DataRow[] rowChilds = pRowStream.GetChildRows(pRowStream.Table.TableName);
            ArrayList aNominal = new ArrayList();
            ArrayList aNominalStep = new ArrayList();
            ArrayList aInterest = new ArrayList();

            //DateTime effectiveDate = pGrossAmount.PaymentDate.unadjustedDate.DateValue;
            DateTime effectiveDate = pGrossAmount.PaymentDate.adjustedDate.DateValue;
            foreach (DataRow rowChild in rowChilds)
            {
                DateTime startDate = Convert.ToDateTime(rowChild["DTSTARTUNADJ"]);
                DateTime endDate   = Convert.ToDateTime(rowChild["DTENDUNADJ"]);

                string eventCode = rowChild["EVENTCODE"].ToString();
                string eventType = rowChild["EVENTTYPE"].ToString();

                EFS_DebtSecurityStreamEvent streamEvent;
                if (EventTypeFunc.IsNominal(eventType))
                {
                    streamEvent = new EFS_DebtSecurityStreamEvent(m_Cs, rowChild, pOrderQuantity);
                    if (EventCodeFunc.IsNominalStep(eventCode))
                        aNominalStep.Add(streamEvent);
                    else
                        aNominal.Add(streamEvent);
                    streamEvent.SetEffectiveDate(pGrossAmount);
                }
                else if (EventTypeFunc.IsInterest(eventType))
                {
                    // EG 20150907 [21317]
                    DataRow[] rowClass = rowChild.GetChildRows("EventClass");
                    foreach (DataRow row in rowClass)
                    {
                        if (EventClassFunc.IsRecordDate(row["EVENTCLASS"].ToString()))
                        {
                            endDate = Convert.ToDateTime(row["DTEVENT"]);
                            break;
                        }
                    }

                    if ((startDate <= effectiveDate) && (effectiveDate < endDate))
                    {
                        streamEvent = new EFS_DebtSecurityStreamEvent(m_Cs, rowChild, pOrderQuantity);
                        streamEvent.SetRecognitionDate(pGrossAmount);
                        aInterest.Add(streamEvent);
                    }
                }
            }
            if (0 < aNominal.Count)
                nominal = (EFS_DebtSecurityStreamEvent[])aNominal.ToArray(typeof(EFS_DebtSecurityStreamEvent));
            if (0 < aNominalStep.Count)
                nominalStep = (EFS_DebtSecurityStreamEvent[])aNominalStep.ToArray(typeof(EFS_DebtSecurityStreamEvent));
            if (0 < aInterest.Count)
                interest = (EFS_DebtSecurityStreamEvent[])aInterest.ToArray(typeof(EFS_DebtSecurityStreamEvent));

            m_ErrLevel = Cst.ErrLevel.SUCCESS;
        }
        #endregion Constructors
    }
    #endregion EFS_DebtSecurityStream
    #region EFS_DebtSecurityStreamEvent
    public class EFS_DebtSecurityStreamEvent
    {
        #region Members
        protected decimal quantity;
        protected string m_Cs;
        protected EFS_Event @event;
        #endregion Members
        #region Accessors
        #region AdjustedStartPeriod
        public EFS_Date AdjustedStartPeriod
        {
            get { return @event.startDate.adjustedDate; }
        }
        #endregion AdjustedStartPeriod
        #region AdjustedEndPeriod
        public EFS_Date AdjustedEndPeriod
        {
            get { return @event.endDate.adjustedDate; }
        }
        #endregion AdjustedEndPeriod
        #region Amount
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_Decimal Amount
        {
            get
            {
                if (@event.valorisationSpecified)
                    return @event.valorisation;
                return null;
            }
        }
        #endregion Amount
        #region Currency
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public string Currency
        {
            get
            {
                if (@event.unitSpecified)
                    return @event.unit;
                return null;
            }
        }
        #endregion Currency
        #region StartPeriod
        public EFS_EventDate StartPeriod
        {
            get { return @event.startDate; }
        }
        #endregion StartPeriod
        #region EventCode
        public string EventCode
        {
            get { return @event.eventKey.eventCode; }
        }
        #endregion EventCode
        #region EventType
        public string EventType
        {
            get { return @event.eventKey.eventType; }
        }
        #endregion EventType
        #region EndPeriod
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_EventDate EndPeriod
        {
            get { return @event.endDate; }
        }
        #endregion EndPeriod
        #region ExDate
        public EFS_Date ExDate
        {
            get { return GetEventClassDate(EventClassFunc.ExDate); }
            set { SetEventClassDate(EventClassFunc.ExDate, value); }
        }
        #endregion ExDate
        #region IdE_Source
        public int IdE_Source
        {
            get {return @event.eventKey.idE;}
        }
        #endregion IdE_Source
        #region PreSettlementDate
        public EFS_Date PreSettlementDate
        {
            get { return GetEventClassDate(EventClassFunc.PreSettlement); }
            set { SetEventClassDate(EventClassFunc.PreSettlement, value); }
        }
        #endregion PreSettlementDate
        #region Quantity
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_Decimal Quantity
        {
            get
            {
                if (@event.valorisationSpecified)
                    return new EFS_Decimal(quantity);
                return null;
            }
        }
        #endregion Quantity
        #region RecognitionDate
        public EFS_Date RecognitionDate
        {
            get { return GetEventClassDate(EventClassFunc.Recognition); }
            set { SetEventClassDate(EventClassFunc.Recognition, value); }
        }
        #endregion RecognitionDate
        #region RecordDate
        public EFS_Date RecordDate
        {
            get { return GetEventClassDate(EventClassFunc.RecordDate); }
            set { SetEventClassDate(EventClassFunc.RecordDate, value); }
        }
        #endregion RecordDate
        #region ValueDate
        // EG 20150616 [21124]
        public EFS_Date ValueDate
        {
            get { return GetEventClassDate(EventClassFunc.ValueDate); }
            set { SetEventClassDate(EventClassFunc.ValueDate, value); }
        }
        #endregion ValueDate
        #region SettlementDate
        public EFS_Date SettlementDate
        {
            get { return GetEventClassDate(EventClassFunc.Settlement); }
            set { SetEventClassDate(EventClassFunc.Settlement, value); }
        }
        #endregion SettlementDate
        #endregion Accessors
        #region Constructors
        public EFS_DebtSecurityStreamEvent(string pConnectionString, DataRow pRow, IOrderQuantity pOrderQuantity):
            this(pConnectionString,pRow)
        {
            if (@event.valorisationSpecified && @event.unitTypeSpecified && (UnitTypeEnum.Currency == @event.unitType))
            {
                if (pOrderQuantity.NumberOfUnitsSpecified)
                    quantity = pOrderQuantity.NumberOfUnits.DecValue;
                else
                    quantity = pOrderQuantity.NotionalAmount.Amount.DecValue / @event.valorisation.DecValue;

                @event.valorisation.DecValue *= quantity;
            }
        }
        public EFS_DebtSecurityStreamEvent(string pConnectionString, DataRow pRow)
        {
            m_Cs = pConnectionString;
            @event = new EFS_Event
            {
                startDate = new EFS_EventDate(Convert.ToDateTime(pRow["DTSTARTUNADJ"]), Convert.ToDateTime(pRow["DTSTARTADJ"])),
                endDate = new EFS_EventDate(Convert.ToDateTime(pRow["DTENDUNADJ"]), Convert.ToDateTime(pRow["DTENDADJ"])),
                valorisationSpecified = (false == Convert.IsDBNull(pRow["VALORISATION"])),
                unitTypeSpecified = (false == Convert.IsDBNull(pRow["UNITTYPE"])),
                unitSpecified = (false == Convert.IsDBNull(pRow["UNIT"])),

                eventKey = new EFS_EventKey
                {
                    eventCode = pRow["EVENTCODE"].ToString(),
                    eventType = pRow["EVENTTYPE"].ToString(),
                    idESpecified = true,
                    idE = Convert.ToInt32(pRow["IDE"])
                }
            };
            // EG 20140924 New
            @event.eventKey.idE_SourceSpecified = (EventTypeFunc.IsInterest(@event.eventKey.eventType) || EventCodeFunc.IsDebtSecurityStream(@event.eventKey.eventCode));

            if (@event.eventKey.idE_SourceSpecified)
                @event.eventKey.idE_Source = @event.eventKey.idE;
            if (@event.valorisationSpecified)
                @event.valorisation = new EFS_Decimal(Convert.ToDecimal(pRow["VALORISATION"]));
            if (@event.unitTypeSpecified)
                @event.unitType = (UnitTypeEnum)System.Enum.Parse(typeof(UnitTypeEnum), pRow["UNITTYPE"].ToString(), false);
            if (@event.unitSpecified)
                @event.unit = pRow["UNIT"].ToString();

            DataRow[] rowChilds = pRow.GetChildRows("EventClass");
            if (ArrFunc.IsFilled(rowChilds))
            {
                @event.eventClass = new EFS_EventClass[rowChilds.Length];
                for (int i=0;i<rowChilds.Length;i++)
                {
                    @event.eventClass[i] = new EFS_EventClass
                    {
                        eventClass = rowChilds[i]["EVENTCLASS"].ToString(),
                        eventDateSpecified = (false == Convert.IsDBNull(rowChilds[i]["DTEVENT"]))
                    };
                    if (@event.eventClass[i].eventDateSpecified)
                    {
                        @event.eventClass[i].eventDate = new EFS_Date
                        {
                            DateValue = Convert.ToDateTime(rowChilds[i]["DTEVENT"])
                        };
                        @event.eventClass[i].isPayment = Convert.ToBoolean(rowChilds[i]["ISPAYMENT"]);
                    }
                }
            }
        }
        #endregion Constructors
        #region Methods
        #region GetEventClassDate
        private EFS_Date GetEventClassDate(string pEventClass)
        {
            EFS_Date eventDate = null;
            foreach (EFS_EventClass eventClass in @event.eventClass)
            {
                if ((pEventClass == eventClass.eventClass) && eventClass.eventDateSpecified)
                {
                    eventDate = new EFS_Date
                    {
                        DateValue = eventClass.eventDate.DateValue
                    };
                    break;
                }
            }
            return eventDate; 
        }
        #endregion GetEventClassDate
        #region PayerNominalReference
        public string PayerNominalReference(string pEventType,string pBuyer,string pSeller,string pIssuer)
        {
            bool isStart = EventCodeFunc.IsStart(@event.eventKey.eventCode);
            bool isNominal = EventTypeFunc.IsNominal(pEventType);
            string payer;
            if (isNominal)
                payer = isStart ? pBuyer : pIssuer;
            else
                payer = isStart ? pSeller : pBuyer;

            return payer;
        }
        #endregion PayerNominalReference
        #region ReceiverNominalReference
        public string ReceiverNominalReference(string pEventType, string pBuyer, string pSeller, string pIssuer)
        {
            bool isStart = EventCodeFunc.IsStart(@event.eventKey.eventCode);
            bool isNominal = EventTypeFunc.IsNominal(pEventType);

            string receiver;
            if (isNominal)
                receiver = isStart ? pSeller : pBuyer;
            else
                receiver = isStart ? pBuyer : pIssuer;

            return receiver;
        }
        #endregion ReceiverNominalReference
        #region SetEventClassDate
        private void SetEventClassDate(string pEventClass,EFS_Date pDate)
        {
            foreach (EFS_EventClass eventClass in @event.eventClass)
            {
                if (pEventClass == eventClass.eventClass)
                {
                    eventClass.eventDateSpecified = (null != pDate);
                    eventClass.eventDate = pDate;
                    break;
                }
            }
        }
        #endregion SetEventClassDate
        #region SetEffectiveDate
        public void SetEffectiveDate(EFS_Payment pPayment)
        {
            DateTime effectiveDate     = pPayment.PaymentDate.unadjustedDate.DateValue;
            //DateTime startDate         = @event.startDate.unadjustedDate.DateValue;
            //DateTime endDate           = @event.endDate.unadjustedDate.DateValue;
            DateTime adjustedStartDate = @event.startDate.adjustedDate.DateValue;
            if ((null != RecognitionDate) && (RecognitionDate.DateValue < effectiveDate))
                RecognitionDate = pPayment.PaymentDate.unadjustedDate;
            if ((null != SettlementDate) && (adjustedStartDate <= pPayment.AdjustedPaymentDate.DateValue))
            {
                PreSettlementDate = pPayment.AdjustedPreSettlementDate;
                ValueDate = pPayment.AdjustedPaymentDate;
                SettlementDate = pPayment.AdjustedPaymentDate;
            }
            SetEventClassDate(EventClassFunc.Date, pPayment.PaymentDate.unadjustedDate);
        }
        #endregion SetEffectiveDate
        #region SetRecognitionDate
        public void SetRecognitionDate(EFS_Payment pPayment)
        {
            EFS_Date effectiveDate = pPayment.PaymentDate.unadjustedDate;
            if ((null != RecognitionDate) && (RecognitionDate.DateValue < effectiveDate.DateValue))
                RecognitionDate = effectiveDate;
        }
        #endregion SetRecognitionDate
        #endregion Methods
    }
    #endregion EFS_DebtSecurityStreamEvent
    #region EFS_DebtSecurityTransactionAmounts
    // EG 20150907 [21317] New
    public class EFS_Money
    {
        #region Members
        public IMoney money;
        public IReference payerPartyReference;
        public IReference receiverPartyReference;
        #endregion Members
        #region Accessors
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_Decimal Amount
        {
            get { return money.Amount; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public string Currency
        {
            get { return money.Currency; }
        }

        #region PayerPartyReference
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public string PayerPartyReference
        {
            get
            {
                return payerPartyReference.HRef;
            }
        }
        #endregion PayerPartyReference
        #region ReceiverPartyReference
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public string ReceiverPartyReference
        {
            get
            {
                return receiverPartyReference.HRef;
            }
        }
        #endregion ReceiverPartyReference

        #endregion Accessors
        #region Constructors
        public EFS_Money(IMoney pMoney)
        {
            money = pMoney;
        }
        public EFS_Money(IMoney pMoney, IReference pPayerPartyReference, IReference pReceiverPartyReference)
        {
            money = pMoney;
            payerPartyReference = pPayerPartyReference;
            receiverPartyReference = pReceiverPartyReference;
        }
        #endregion Constructors

    }
    // EG 20190730 Upd (Add isPositionOpening|isPositionKeeping members)
    public class EFS_DebtSecurityTransactionAmounts
    {
        #region Members
        protected string m_Cs;
        protected Cst.ErrLevel m_ErrLevel = Cst.ErrLevel.UNDEFINED;
        public DateTime clearingBusinessDate;
        public EFS_Payment grossAmount;
        public IOrderQuantity quantity;
        /// <summary>
        /// Représente le nominal du titre
        /// </summary>
        public IMoney securityNominal;
        public IOrderPrice price;
        public IMoney dirtyAmount;
        public bool accruedInterestAmountSpecified;
        // EG 20150907 [21317] New
        /// <summary>
        /// Montant des intérêts courus
        /// </summary>
        public EFS_Money accruedInterestAmount;

        public bool principalAmountSpecified;
        /// <summary>
        /// Montant CleanPrice * Qty 
        /// </summary>
        /// FI 20151228 [21660] Add principalAmount
        public EFS_Money principalAmount;

        private EFS_Asset m_DebtSecurity;
        private Cst.StatusBusiness m_StatusBusiness;
        public EFS_Boolean allocation; /* Virtual */
        public bool allocationSpecified; /* Virtual */
        public bool isPositionOpening;
        public bool isPositionKeeping;
        #endregion Members
        #region Accessors
        #region StatusBusiness
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public Cst.StatusBusiness StatusBusiness
        {
            set 
            {
                allocationSpecified = (value == Cst.StatusBusiness.ALLOC);
                if (allocationSpecified)
                    allocation = new EFS_Boolean(true);
                m_StatusBusiness = value;
            }
            get { return m_StatusBusiness; }
        }
        #endregion StatusBusiness

        #region AdjustedPaymentDate
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_Date AdjustedPaymentDate
        {
            get { return grossAmount.AdjustedPaymentDate; }
        }
        #endregion AdjustedPaymentDate
        #region AdjustedPreSettlementDate
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_Date AdjustedPreSettlementDate
        {
            get {return grossAmount.AdjustedPreSettlementDate;}
        }
        #endregion AdjustedPreSettlementDate
        #region AdjustedClearingBusinessDate
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_Date AdjustedClearingBusinessDate
        {
            get
            {
                EFS_Date adjustedDate = new EFS_Date
                {
                    DateValue = clearingBusinessDate.Date
                };
                return adjustedDate;
            }
        }
        #endregion AdjustedClearingBusinessDate

        #region DebtSecurity
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_Asset DebtSecurity
        {
            get { return m_DebtSecurity; }
        }
        #endregion DebtSecurity

        #region Quantity
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_Decimal Quantity
        {
            get 
            {
                EFS_Decimal ret = new EFS_Decimal();
                if (quantity.NumberOfUnitsSpecified)
                    ret.DecValue = quantity.NumberOfUnits.DecValue;
                else if ((null != quantity.NotionalAmount) && (null != securityNominal))
                    ret.DecValue = quantity.NotionalAmount.Amount.DecValue / securityNominal.Amount.DecValue;
                return ret; 
            }
        }
        #endregion Quantity
        #region NotionalAmount
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_Decimal NotionalAmount
        {
            get {return quantity.NotionalAmount.Amount; }
        }
        #endregion NotionalAmount
        #region NotionalCurrency
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public string NotionalCurrency
        {
            get { return quantity.NotionalAmount.Currency; }
        }
        #endregion NotionalCurrency
        #region ClearingBusinessDate
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_EventDate ClearingBusinessDate
        {
            get { return new EFS_EventDate(clearingBusinessDate.Date, clearingBusinessDate.Date); }
        }
        #endregion ClearingBusinessDate


        #region UnadjustedPaymentDate
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_Date UnadjustedPaymentDate
        {
            get { return grossAmount.UnadjustedPaymentDate; }
        }
        #endregion UnadjustedPaymentDate
        #region ErrLevel
        public Cst.ErrLevel ErrLevel
        {
            get { return m_ErrLevel; }
        }
        #endregion ErrLevel

        #region IsNotPositionOpeningAndIsPositionKeeping
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        // EG 20190730 New
        public bool IsNotPositionOpeningAndIsPositionKeeping
        {
            get
            {
                return (false == isPositionOpening) && isPositionKeeping;
            }
        }
        #endregion IsNotPositionOpeningAndIsPositionKeeping

        #region EventTypeGAM
        // EG 20190730 New (if PositionOpening GAM = HGA (Historical GAM)
        public string EventTypeGAM
        {
            get { return IsNotPositionOpeningAndIsPositionKeeping ? EventTypeFunc.GrossAmount : EventTypeFunc.HistoricalGrossAmount; }
        }
        #endregion EventTypeGAM
        #endregion Accessors
        #region Constructors
        public EFS_DebtSecurityTransactionAmounts(string pConnectionString, IDebtSecurityTransaction pDebtSecurityTransaction, DataDocumentContainer pDataDocument, Cst.StatusBusiness pStatusBusiness)
        {
            m_Cs = pConnectionString;
            m_ErrLevel = Calc(pDebtSecurityTransaction, pDataDocument, pStatusBusiness);
        }
        #endregion Constructors
        #region Methods
        #region Calc
        /// <summary>
        ///  Calculs
        /// </summary>
        /// <param name="pDebtSecurityTransaction"></param>
        /// <param name="pDataDocument"></param>
        /// <returns></returns>
        /// FI 20151228 [21660] Modify
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        // EG 20180307 [23769] Gestion dbTransaction
        // EG 20190730 Upd (Set isPositionKeeping and isPositionKeeping)
        protected Cst.ErrLevel Calc(IDebtSecurityTransaction pDebtSecurityTransaction, DataDocumentContainer pDataDocument, Cst.StatusBusiness pStatusBusiness)
        {
            DebtSecurityTransactionContainer debtSecurityTransactionContainer = new DebtSecurityTransactionContainer(pDebtSecurityTransaction, pDataDocument);
            debtSecurityTransactionContainer.InitRptSide(m_Cs, pStatusBusiness == Cst.StatusBusiness.ALLOC);
            isPositionOpening = debtSecurityTransactionContainer.IsPositionOpening;

            isPositionKeeping = false;
            if (pStatusBusiness == Cst.StatusBusiness.ALLOC)
                isPositionKeeping = debtSecurityTransactionContainer.IsPosKeepingOnBookDealer(m_Cs);

            DebtSecurityContainer debtSecurityContainer = new DebtSecurityContainer(pDebtSecurityTransaction.DebtSecurity);
            clearingBusinessDate = debtSecurityTransactionContainer.ClearingBusinessDate;

            grossAmount = new EFS_Payment(m_Cs, null, pDebtSecurityTransaction.GrossAmount, pDataDocument);

            quantity = pDebtSecurityTransaction.Quantity;
            securityNominal = debtSecurityContainer.GetNominal(debtSecurityTransactionContainer.Product.ProductBase);

            //price = pDebtSecurityTransaction.price;

            // FI 20151228 [21660] calc principalAmount 
            if (pDebtSecurityTransaction.Price.CleanPriceSpecified)
            {
                IMoney money = debtSecurityTransactionContainer.CalcPrincipalAmount();
                principalAmount = new EFS_Money(money, pDebtSecurityTransaction.BuyerPartyReference, pDebtSecurityTransaction.SellerPartyReference);
                principalAmountSpecified = true;
            }

            accruedInterestAmountSpecified = pDebtSecurityTransaction.Price.AccruedInterestAmountSpecified;
            if (accruedInterestAmountSpecified)
            {
                accruedInterestAmount = new EFS_Money(pDebtSecurityTransaction.Price.AccruedInterestAmount);
                if (0 <= accruedInterestAmount.money.Amount.DecValue)
                {
                    accruedInterestAmount.payerPartyReference = pDebtSecurityTransaction.BuyerPartyReference;
                    accruedInterestAmount.receiverPartyReference = pDebtSecurityTransaction.SellerPartyReference;
                }
                else
                {
                    accruedInterestAmount.payerPartyReference = pDebtSecurityTransaction.SellerPartyReference;
                    accruedInterestAmount.receiverPartyReference = pDebtSecurityTransaction.BuyerPartyReference;
                }
            }

            m_DebtSecurity = pDebtSecurityTransaction.Efs_Asset(m_Cs);
            
            return Cst.ErrLevel.SUCCESS;
        }
        #endregion Calc
        #endregion Methods
    }
    #endregion EFS_DebtSecurityTransactionAmounts
    #region EFS_DebtSecurityTransactionStream
    public class EFS_DebtSecurityTransactionStream
    {
        #region Members
        protected string m_Cs;
        protected DataDocumentContainer m_DataDocument;
        protected Cst.StatusBusiness m_StatusBusiness;
        protected Cst.ErrLevel m_ErrLevel = Cst.ErrLevel.UNDEFINED;
        public EFS_DebtSecurityStream[] debtSecurityStream;
        private DataSetEvent m_DsEvents;

        #endregion Members
        #region Accessors
        #region DtEvent
        public DataTable DtEvent
        {
            get { return m_DsEvents.DtEvent; }
        }
        #endregion DtEvent
        #region DtEventClass
        public DataTable DtEventClass
        {
            get { return m_DsEvents.DtEventClass; }
        }
        #endregion DtEventClass
        #region ErrLevel
        public Cst.ErrLevel ErrLevel
        {
            get { return m_ErrLevel; }
        }
        #endregion ErrLevel
        #region RowDebtSecurityStream
        public DataRow[] RowDebtSecurityStream
        {
            get
            {
                return DtEvent.Select("EVENTCODE=" + DataHelper.SQLString(EventCodeFunc.DebtSecurityStream));
            }
        }
        #endregion RowDebtSecurityStream
        #endregion Accessors
        #region Constructors
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        public EFS_DebtSecurityTransactionStream(string pConnectionString, IDebtSecurityTransaction pDebtSecurityTransaction, DataDocumentContainer pDataDocument, Cst.StatusBusiness status)
        {
            m_Cs = pConnectionString;
            m_DataDocument = pDataDocument;
            m_StatusBusiness = status;
            
            m_ErrLevel = Calc(pDebtSecurityTransaction);
        }
        #endregion Constructors
        #region Methods
        #region Calc
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        // EG 20180307 [23769] Gestion dbTransaction
        protected Cst.ErrLevel Calc(IDebtSecurityTransaction pDebtSecurityTransaction)
        {
            //Chargement des événements du titre associé à la transaction
            m_DsEvents = new DataSetEvent(m_Cs);
            DataSetEventLoadSettings settings = new DataSetEventLoadSettings(DataSetEventLoadEnum.Event);
            m_DsEvents.Load(null, pDebtSecurityTransaction.SecurityAssetOTCmlId, null, settings);

            EFS_Asset _asset = pDebtSecurityTransaction.Efs_Asset(m_Cs);

            List<EFS_DebtSecurityStream> aStreams = new List<EFS_DebtSecurityStream>();
            EFS_Payment grossAmount = new EFS_Payment(m_Cs, null, pDebtSecurityTransaction.GrossAmount, m_DataDocument);
            foreach (DataRow rowStream in RowDebtSecurityStream)
            {
                if (m_StatusBusiness == Cst.StatusBusiness.ALLOC)
                    aStreams.Add(new EFS_DebtSecurityStream(m_Cs, rowStream, _asset));
                else
                    aStreams.Add(new EFS_DebtSecurityStream(m_Cs, rowStream, grossAmount, pDebtSecurityTransaction.Quantity, _asset));
            }

            if (0 < aStreams.Count)
                debtSecurityStream = aStreams.ToArray();

            return Cst.ErrLevel.SUCCESS;
        }
        #endregion Calc
        #endregion Methods
    }
    #endregion EFS_DebtSecurityTransactionStream

    #region EFS_SaleAndRepurchaseAgreement
    public class EFS_SaleAndRepurchaseAgreement
    {
        #region Members
        protected string m_Cs;
        protected Cst.ErrLevel m_ErrLevel = Cst.ErrLevel.UNDEFINED;
        public RepoDurationEnum duration;
        public ICashStream[] cashStream;
        public EFS_SecurityLeg[] spotLeg;
        public EFS_SecurityLeg[] forwardLeg;
        public bool forwardLegSpecified;
        public IAdjustableOffset noticePeriod;
        public bool noticePeriodSpecified;
        #endregion Members
        #region Accessors
        #region ErrLevel
        public Cst.ErrLevel ErrLevel
        {
            get { return m_ErrLevel; }
        }
        #endregion ErrLevel
        #region EventType
        public string EventType
        {
            get 
            {
                string eventType = string.Empty;
                switch (duration)
                {
                    case RepoDurationEnum.Open:
                        eventType = EventTypeFunc.Open;
                        break;
                    case RepoDurationEnum.Overnight:
                        eventType = EventTypeFunc.Overnight;
                        break;
                    case RepoDurationEnum.Term:
                        eventType = EventTypeFunc.Term;
                        break;
                }
                return eventType; 
            }
        }
        #endregion EventType
        #endregion Accessors
        #region Constructors
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        public EFS_SaleAndRepurchaseAgreement(string pConnectionString, ISaleAndRepurchaseAgreement pSaleAndRepurchaseAgreement, DataDocumentContainer pDataDocument, Cst.StatusBusiness statusBusiness)
        {
            m_Cs = pConnectionString;
            m_ErrLevel = Calc(pSaleAndRepurchaseAgreement, pDataDocument, statusBusiness);
        }
        #endregion Constructors
        #region Methods
        #region Calc
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        protected Cst.ErrLevel Calc(ISaleAndRepurchaseAgreement pSaleAndRepurchaseAgreement, DataDocumentContainer pDataDocument, Cst.StatusBusiness statusBusiness)
        {
            duration = pSaleAndRepurchaseAgreement.Duration;
            noticePeriod = pSaleAndRepurchaseAgreement.NoticePeriod;

            cashStream = pSaleAndRepurchaseAgreement.CashStream;

            List<EFS_SecurityLeg> lst = new List<EFS_SecurityLeg>();
            foreach (ISecurityLeg item in pSaleAndRepurchaseAgreement.SpotLeg)
            
                lst.Add(new EFS_SecurityLeg(m_Cs, item, pDataDocument, statusBusiness));
            spotLeg = lst.ToArray();


            forwardLegSpecified = pSaleAndRepurchaseAgreement.ForwardLegSpecified;
            if (forwardLegSpecified)
            {
                lst = new List<EFS_SecurityLeg>();
                foreach (ISecurityLeg item in pSaleAndRepurchaseAgreement.ForwardLeg)
                    lst.Add(new EFS_SecurityLeg(m_Cs, item, pDataDocument, statusBusiness));
                forwardLeg = lst.ToArray();
            }

            return Cst.ErrLevel.SUCCESS;
        }
        #endregion Calc
        #endregion Methods
    }
    #endregion EFS_SaleAndRepurchaseAgreement
    #region EFS_SecurityLeg
    public class EFS_SecurityLeg
    {
        #region Members

        protected string m_Cs;
        protected DataDocumentContainer m_DataDocument;
        protected Cst.StatusBusiness m_statusBusiness;
        protected Cst.ErrLevel m_ErrLevel = Cst.ErrLevel.UNDEFINED;
        public IReference buyerPartyReference;
        public IReference sellerPartyReference;
        public IReference issuerPartyReference;
        public DateTime valueDate;
        public EFS_DebtSecurityTransactionAmounts efs_DebtSecurityTransactionAmounts;
        public EFS_DebtSecurityTransactionStream efs_DebtSecurityTransactionStream;
        #endregion Members
        #region Accessors
        #region BuyerPartyReference
        public string BuyerPartyReference
        {
            get { return buyerPartyReference.HRef; }
        }
        #endregion BuyerPartyReference
        #region IssuerPartyReference
        public string IssuerPartyReference
        {
            get { return issuerPartyReference.HRef; }
        }
        #endregion IssuerPartyReference
        #region SellerPartyReference
        public string SellerPartyReference
        {
            get { return sellerPartyReference.HRef; }
        }
        #endregion SellerPartyReference
        #region MaxTerminationDate
        public EFS_EventDate MaxTerminationDate
        {
            get
            {
                EFS_EventDate dtTermination = new EFS_EventDate
                {
                    unadjustedDate = new EFS_Date
                    {
                        DateValue = DateTime.MinValue
                    },
                    adjustedDate = new EFS_Date
                    {
                        DateValue = DateTime.MinValue
                    }
                };
                foreach (EFS_DebtSecurityStream stream in efs_DebtSecurityTransactionStream.debtSecurityStream)
                {
                    if (0 < stream.TerminationDate.unadjustedDate.DateValue.CompareTo(dtTermination.unadjustedDate.DateValue))
                    {
                        dtTermination.unadjustedDate.DateValue = stream.TerminationDate.unadjustedDate.DateValue;
                        dtTermination.adjustedDate.DateValue = stream.TerminationDate.adjustedDate.DateValue;
                    }
                }
                return dtTermination;
            }
        }
        #endregion MaxTerminationDate
        #region ValueDate
        public EFS_EventDate ValueDate
        {
            get { return efs_DebtSecurityTransactionAmounts.grossAmount.PaymentDate; }
        }
        #endregion ValueDate

        #region ErrLevel
        public Cst.ErrLevel ErrLevel
        {
            get { return m_ErrLevel; }
        }
        #endregion ErrLevel
        #endregion Accessors
        #region Constructors
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        public EFS_SecurityLeg(string pConnectionString, ISecurityLeg pSecurityLeg, DataDocumentContainer pDataDocument, Cst.StatusBusiness status)
        {
            m_Cs = pConnectionString;
            m_DataDocument = pDataDocument;
            m_statusBusiness = status;
            m_ErrLevel = Calc(pSecurityLeg);
        }
        #endregion Constructors
        #region Methods
        #region Calc
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        protected Cst.ErrLevel Calc(ISecurityLeg pSecurityLeg)
        {
            buyerPartyReference = pSecurityLeg.DebtSecurityTransaction.BuyerPartyReference;
            sellerPartyReference = pSecurityLeg.DebtSecurityTransaction.SellerPartyReference;
            issuerPartyReference = pSecurityLeg.DebtSecurityTransaction.IssuerPartyReference;
            efs_DebtSecurityTransactionAmounts = new EFS_DebtSecurityTransactionAmounts(m_Cs, pSecurityLeg.DebtSecurityTransaction, m_DataDocument, m_statusBusiness);
            Cst.ErrLevel ret = efs_DebtSecurityTransactionAmounts.ErrLevel;
            if (Cst.ErrLevel.SUCCESS == ret)
            {
                efs_DebtSecurityTransactionStream = new EFS_DebtSecurityTransactionStream(m_Cs, pSecurityLeg.DebtSecurityTransaction, m_DataDocument, m_statusBusiness);
                ret = efs_DebtSecurityTransactionStream.ErrLevel;
            }
            return ret;
        }
        #endregion Calc
        #endregion Methods
    }
    #endregion EFS_SecurityLeg

    #region EFS_EquitySecurityTransaction
    /// EG 20150306 [POC-BERKELEY] : Add MarginRatio
    // EG 2015124 [POC-MUREX] Add pDbTransaction
    public class EFS_EquitySecurityTransaction
    {
        #region Members
        private readonly string m_Cs;
        protected Cst.ErrLevel m_ErrLevel = Cst.ErrLevel.UNDEFINED;
        private EFS_Asset m_AssetEquity;
        private decimal price;
        private decimal quantity;
        public bool initialMarginSpecified;
        public IMoney initialMargin;

        // EG 20150403 [POC]
        public Pair<Pair<IReference, IReference>, Pair<IReference, IReference>> partyReference;
        public DateTime tradeDate;
        public DateTime clearingBusinessDate;
        public bool isPositionOpening;
        public bool isTradeCAAdjusted;
        public EFS_Payment grossAmount;
        public bool marginRatioSpecified;
        public IMarginRatio marginRatio;
        public bool isPositionKeeping;
        #endregion Members
        #region Accessors
        #region AssetEquity
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_Asset AssetEquity
        {
            get { return m_AssetEquity; }
        }
        #endregion AssetEquity

        #region AdjustedClearingBusinessDate
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_Date AdjustedClearingBusinessDate
        {
            get
            {
                EFS_Date adjustedDate = new EFS_Date
                {
                    DateValue = clearingBusinessDate.Date
                };
                return adjustedDate;
            }
        }
        #endregion AdjustedClearingBusinessDate
        #region ClearingBusinessDate
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_EventDate ClearingBusinessDate
        {
            get { return new EFS_EventDate(clearingBusinessDate.Date, clearingBusinessDate.Date); }
        }
        #endregion ClearingBusinessDate

        #region OpenTerminationDate
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        /// EG 20150306 [POC-BERKELEY] : New
        public EFS_EventDate OpenTerminationDate
        {
            get { return new EFS_EventDate(DateTime.MaxValue, DateTime.MaxValue); }
        }
        #endregion OpenTerminationDate


        #region AdjustedPaymentDate
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_Date AdjustedPaymentDate
        {
            get { return grossAmount.AdjustedPaymentDate; }
        }
        #endregion AdjustedPaymentDate
        #region AdjustedPreSettlementDate
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_Date AdjustedPreSettlementDate
        {
            get { return grossAmount.AdjustedPreSettlementDate; }
        }
        #endregion AdjustedPreSettlementDate
        #region UnadjustedPaymentDate
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_Date UnadjustedPaymentDate
        {
            get { return grossAmount.UnadjustedPaymentDate; }
        }
        #endregion UnadjustedPaymentDate

        #region ErrLevel
        public Cst.ErrLevel ErrLevel
        {
            get { return m_ErrLevel; }
        }
        #endregion ErrLevel

        #region Quantity
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_Decimal Quantity
        {
            get { return new EFS_Decimal(quantity); }
        }
        #endregion Quantity

        #region BuyerPartyReference
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public string BuyerPartyReference
        {
            get
            {
                return partyReference.First.First.HRef;
            }
        }
        #endregion BuyerPartyReference
        #region SellerPartyReference
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public string SellerPartyReference
        {
            get
            {
                return partyReference.First.Second.HRef;
            }
        }
        #endregion SellerPartyReference

        // EG 20150403 (POC]
        #region CustodianPartyReference
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public string CustodianPartyReference
        {
            get
            {
                string hRef = string.Empty;
                if ((null != partyReference) && (null != partyReference.Second) && (null != partyReference.Second.Second))
                    hRef = partyReference.Second.Second.HRef;
                return hRef;

            }
        }
        #endregion CustodianPartyReference
        #region DealerPartyReference
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public string DealerPartyReference
        {
            get
            {
                string hRef = string.Empty;
                if ((null != partyReference) && (null != partyReference.Second) && (null != partyReference.Second.First))
                    hRef = partyReference.Second.First.HRef;
                return hRef;
            }
        }
        #endregion DealerPartyReference
        #region InitialMarginPayerPartyReference
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public string InitialMarginPayerPartyReference
        {
            get
            {
                string hRef = DealerPartyReference;
                if (StrFunc.IsEmpty(hRef))
                    hRef = BuyerPartyReference;
                return hRef;
            }
        }
        #endregion InitialMarginPayerPartyReference
        #region InitialMarginReceiverPartyReference
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public string InitialMarginReceiverPartyReference
        {
            get
            {
                string hRef = CustodianPartyReference;
                if (StrFunc.IsEmpty(hRef))
                    hRef = SellerPartyReference;
                return hRef;
            }
        }
        #endregion InitialMarginReceiverPartyReference

        #region InitialMarginAmount
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        /// EG 20150306 [POC-BERKELEY] : New
        public EFS_Decimal InitialMarginAmount
        {
            get
            {
                EFS_Decimal _amount = new EFS_Decimal();
                if (initialMarginSpecified)
                    _amount.DecValue = initialMargin.Amount.DecValue;
                return _amount;
            }

        }
        #endregion InitialMarginAmount
        #region InitialMarginCurrency
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        /// EG 20150306 [POC-BERKELEY] : New
        public string InitialMarginCurrency
        {
            get
            {
                string _unit = string.Empty;
                if (initialMarginSpecified)
                    _unit = initialMargin.Currency;
                return _unit;
            }
        }
        #endregion InitialMarginCurrency

        #region MarginRatioAmount
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        /// EG 20150306 [POC-BERKELEY] : New
        public EFS_Decimal MarginRatioAmount
        {
            get
            {
                EFS_Decimal _amount = new EFS_Decimal();
                if (marginRatioSpecified)
                    _amount.DecValue = marginRatio.Amount.DecValue;
                return _amount;
            }

        }
        #endregion MarginRatioAmount
        #region MarginRatioUnitType
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        /// EG 20150306 [POC-BERKELEY] : New
        public string MarginRatioUnitType
        {
            get
            {
                UnitTypeEnum _unitType = UnitTypeEnum.None;
                if (marginRatioSpecified)
                {
                    switch (marginRatio.PriceExpression)
                    {
                        case PriceExpressionEnum.AbsoluteTerms:
                            _unitType = UnitTypeEnum.Currency;
                            break;
                        case PriceExpressionEnum.PercentageOfNotional:
                            _unitType = UnitTypeEnum.Percentage;
                            break;
                    }
                }
                return _unitType.ToString();
            }

        }
        #endregion MarginRatioUnitType
        #region MarginRatioUnit
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        /// EG 20150306 [POC-BERKELEY] : New
        public string MarginRatioUnit
        {
            get
            {
                string _unit = string.Empty;
                if (marginRatioSpecified &&
                    marginRatio.CurrencySpecified &&
                    marginRatio.PriceExpression == PriceExpressionEnum.AbsoluteTerms)
                    _unit = marginRatio.Currency.Value;
                return _unit.ToString();
            }
        }
        #endregion MarginRatioUnit

        #region IsNotPositionOpeningAndIsPositionKeeping
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        // EG 20190730 New
        public bool IsNotPositionOpeningAndIsPositionKeeping
        {
            get
            {
                return (false == isPositionOpening) && isPositionKeeping;
            }
        }
        #endregion IsNotPositionOpeningAndIsPositionKeeping

        #region EventTypeGAM
        // EG 20190730 New
        public string EventTypeGAM
        {
            get { return IsNotPositionOpeningAndIsPositionKeeping ? EventTypeFunc.GrossAmount : EventTypeFunc.HistoricalGrossAmount; }
        }
        #endregion EventTypeGAM

        #endregion Accessors
        #region Constructors
        // EG 20150306 [POC-BERKELEY] : Refactoring
        // EG 2015124 [POC-MUREX] Add pDbTransaction
        public EFS_EquitySecurityTransaction(string pConnectionString, IEquitySecurityTransaction pEquitySecurityTransaction, 
            DataDocumentContainer pDataDocument, Cst.StatusBusiness pStatusBusiness)
        {
            m_Cs = pConnectionString;
            Calc(null, pEquitySecurityTransaction, pDataDocument, pStatusBusiness);
        }
        // EG 2015124 [POC-MUREX] Add pDbTransaction
        public EFS_EquitySecurityTransaction(string pConnectionString, IDbTransaction pDbTransaction, IEquitySecurityTransaction pEquitySecurityTransaction,
            DataDocumentContainer pDataDocument, Cst.StatusBusiness pStatusBusiness)
        {
            m_Cs = pConnectionString;
            Calc(pDbTransaction, pEquitySecurityTransaction, pDataDocument, pStatusBusiness);
        }
        #endregion Constructors
        #region Methods
        #region Calc
        // EG 20140904 Add AssetCategory
        /// EG 20150306 [POC-BERKELEY] : Refactoring (MarginRatio)
        // EG 2015124 [POC-MUREX] Add pDbTransaction
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        // EG 20180307 [23769] Gestion dbTransaction
        // EG 20190613 [24683] Use dbTransaction
        protected void Calc(IDbTransaction pDbTransaction, IEquitySecurityTransaction pEquitySecurityTransaction, DataDocumentContainer pDataDocument, Cst.StatusBusiness pStatusBusiness)
        {
            // EG 2015124 [POC-MUREX] Add pDbTransaction
            EquitySecurityTransactionContainer equitySecurityTransactionContainer =
                new EquitySecurityTransactionContainer(m_Cs, pDbTransaction, pEquitySecurityTransaction, pDataDocument);
            partyReference = equitySecurityTransactionContainer.GetPartyReference();

            isPositionOpening = equitySecurityTransactionContainer.IsPositionOpening;
            isTradeCAAdjusted = equitySecurityTransactionContainer.IsTradeCAAdjusted;

            clearingBusinessDate = equitySecurityTransactionContainer.ClearingBusinessDate;

            grossAmount = new EFS_Payment(m_Cs, pDbTransaction, pEquitySecurityTransaction.GrossAmount, pDataDocument);
            quantity = pEquitySecurityTransaction.TradeCaptureReport.LastQty.DecValue;
            price = pEquitySecurityTransaction.TradeCaptureReport.LastPx.DecValue;

            isPositionKeeping = false;
            if (pStatusBusiness == Cst.StatusBusiness.ALLOC)
                isPositionKeeping = equitySecurityTransactionContainer.IsPosKeepingOnBookDealer(m_Cs, pDbTransaction);

            marginRatioSpecified = pEquitySecurityTransaction.MarginRatioSpecified;
            marginRatio = pEquitySecurityTransaction.MarginRatio;
            initialMarginSpecified = marginRatioSpecified;
            if (initialMarginSpecified)
            {
                IProductBase productBase = (IProductBase)equitySecurityTransactionContainer.DataDocument.CurrentProduct.ProductBase;
                switch (marginRatio.PriceExpression)
                {
                    case PriceExpressionEnum.PercentageOfNotional:
                        initialMargin = productBase.CreateMoney();
                        initialMargin.Amount = new EFS_Decimal(marginRatio.Amount.DecValue * grossAmount.Amount.DecValue);
                        initialMargin.Currency = grossAmount.Currency;
                        break;
                    case PriceExpressionEnum.AbsoluteTerms:
                        initialMargin = productBase.CreateMoney();
                        initialMargin.Amount = new EFS_Decimal(marginRatio.Amount.DecValue);
                        initialMargin.Currency = marginRatio.CurrencySpecified ? marginRatio.Currency.Value : grossAmount.Currency;
                        break;
                }
            }

            #region Asset
            m_AssetEquity = new EFS_Asset
            {
                idC = equitySecurityTransactionContainer.AssetEquity.IdC,
                IdMarket = equitySecurityTransactionContainer.IdMarket,
                idAsset = equitySecurityTransactionContainer.AssetEquity.Id,
                assetCategory = equitySecurityTransactionContainer.AssetEquity.AssetCategory,
                assetSymbol = equitySecurityTransactionContainer.AssetEquity.AssetSymbol,
                isinCode = equitySecurityTransactionContainer.AssetEquity.ISINCode
            };
            #endregion Asset

            m_ErrLevel = Cst.ErrLevel.SUCCESS;
        }
        #endregion Calc
        #endregion Methods
    }
    #endregion EFS_EquitySecurityTransaction

}
