#region Using Directives
using System;
using System.Collections;
using System.Data;
using System.Reflection;
using EFS.ACommon;
using EFS.ApplicationBlocks.Data;
using EFS.Common;

using EFS.GUI.Interface;
using EFS.Tuning;
using EFS.Status; 
using EfsML.Enum;
using EfsML.Enum.Tools;
using EfsML.EventMatrix;
using FpML.Interface;
#endregion Using Directives

namespace EfsML.Business
{
    #region EFS_Event
    /// EG 20240105 [WI756] Spheres Core : Refactoring Code Analysis - Correctifs après tests (property Id - Attribute name)
    public class EFS_Event
    {
        #region Members
        #region Event
        #region EventReference
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool eventReferenceSpecified;
        public string eventReference;
        #endregion EventReference
        #region EventKey
        public EFS_EventKey eventKey;
        #endregion EventKey
        #region Periods
        public EFS_EventDate startDate;
        public EFS_EventDate endDate;
        #endregion Periods
        #region Payer/Receiver
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool payerSpecified;
        public string payer;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool receiverSpecified;
        public string receiver;
        #endregion Payer/Receiver
        #region Valorisation/Unit
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool valorisationSpecified;
        public EFS_Decimal valorisation;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool unitTypeSpecified;
        public UnitTypeEnum unitType;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool unitSpecified;
        public string unit;
        #endregion Valorisation/Unit
        #region IdStCalcul
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public StatusCalculEnum idStCalcul;
        #endregion IdStCalcul
        #endregion Event
        #region EventAsset / EventDet
        [System.Xml.Serialization.XmlElementAttribute("eventDetail")]
        public EFS_EventDetail eventDetail;
        #endregion EventAsset / EventDet
        #region EventClass
        [System.Xml.Serialization.XmlElementAttribute("subEvent")]
        public EFS_EventClass[] eventClass;
        #endregion EventClass
        #region Others
        [System.Xml.Serialization.XmlElementAttribute("productId")]
        public int productId;
        [System.Xml.Serialization.XmlAttributeAttribute("id", Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "ID")]
        public string id;
        [System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "normalizedString")]
        public string name;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public int instrumentNo;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public int streamNo;

        #endregion Others
        #endregion Members
        #region Constructors
        public EFS_Event() { }
        public EFS_Event(EFS_EventKey pEventKey, EFS_EventMatrixGroupKey pEventGroupKeyParent, EFS_EventMatrixGroupKey pEventGroupKey)
        {
            if (null != pEventGroupKeyParent)
                this.eventReference = pEventGroupKeyParent.result;

            if (null != pEventGroupKey)
            {
                this.id = pEventGroupKey.result;
                this.name = pEventGroupKey.name;
            }
            eventReferenceSpecified = (null != pEventGroupKeyParent);
            eventKey = (EFS_EventKey)pEventKey.Clone();
        }

        #endregion Constructors
        #region Methods
        #region SetSequence
        public void SetSequence(int pInstrumentNo, int pStreamNo)
        {
            SetSequence(pInstrumentNo, pStreamNo, 1, 0);
        }
        public void SetSequence(int pInstrumentNo, int pStreamNo, int pMultiplierStreamNo, int pSpreadStreamNo)
        {
            instrumentNo = pInstrumentNo;
            streamNo = (pStreamNo * pMultiplierStreamNo) + pSpreadStreamNo;
        }
        #endregion SetSequence

        #endregion Methods
    }
    #endregion EFS_Event
    #region EFS_EventClass
    public class EFS_EventClass
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("eventClass")]
        public string eventClass;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool eventDateSpecified;
        public EFS_Date eventDate;
        public bool isPayment;
        #endregion Members
        #region Constructors
        public EFS_EventClass() { }
        // EG 20231127 [WI755] Implementation Return Swap : Refactoring Code Analysis
        public EFS_EventClass(string pEventClass, EFS_Date pEventDate, bool pIsPayment)
        {
            eventClass = pEventClass;
            if (null != pEventDate)
            {
                eventDate = new EFS_Date
                {
                    DateValue = pEventDate.DateValue
                };
                isPayment = pIsPayment;
            }
            eventDateSpecified = (null != eventDate);
        }
        #endregion Constructors
    }
    #endregion EFS_EventClass
    #region EFS_EventDate
    /// <summary>
    /// Représente une date (valeurs adjusted et unadjusted)
    /// </summary>
    public class EFS_EventDate
    {
        #region Members
        /// <summary>
        /// Date non ajustée
        /// </summary>
        public EFS_Date unadjustedDate;
        /// <summary>
        /// Date ajustée
        /// </summary>
        public EFS_Date adjustedDate;
        #endregion Members
        #region Constructors
        public EFS_EventDate()
        {
            unadjustedDate = new EFS_Date();
            adjustedDate = new EFS_Date();
        }
        // EG 20231127 [WI755] Implementation Return Swap : Refactoring Code Analysis
        public EFS_EventDate(DateTime pUnadjustedDate, DateTime pAdjustedDate)
        {
            unadjustedDate = new EFS_Date
            {
                DateValue = pUnadjustedDate
            };
            adjustedDate = new EFS_Date
            {
                DateValue = pAdjustedDate
            };
        }

        public EFS_EventDate(EFS_AdjustableDate pAdjustableDate)
        {
            unadjustedDate = pAdjustableDate.UnadjustedEventDate;
            if (pAdjustableDate.adjustableDateSpecified)
                adjustedDate = pAdjustableDate.AdjustedEventDate;
        }
        #endregion Constructors
    }
    #endregion EFS_EventDates
    #region EFS_EventDetail

    public class EFS_EventDetail
    {
        #region Members
        #region Assets
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool assetSpecified;
        public EFS_Asset asset;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool asset2Specified;
        public EFS_Asset asset2;
        #endregion Assets
        #region Details COMD
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool fixedPriceSpecified;
        public EFS_FixedPrice fixedPrice;
        public bool deliveryDateSpecified;
        public EFS_CommodityDeliveryDate deliveryDate;
        #endregion Details COMD
        #region Details FX
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool paymentQuoteSpecified;
        public EFS_PaymentQuote paymentQuote;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool exchangeRateSpecified;
        public EFS_ExchangeRate exchangeRate;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool sideRateSpecified;
        public ISideRate sideRate;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool fixingRateSpecified;
        public EFS_FxFixing fixingRate;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool settlementRateSpecified;
        public EFS_SettlementRate settlementRate;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool currencyPairSpecified;
        public IQuotedCurrencyPair currencyPair;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool triggerRateSpecified;
        public EFS_TriggerRate triggerRate;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool strikePriceSpecified;
        public IFxStrikePrice strikePrice;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool premiumQuoteSpecified;
        public EFS_FxPremiumQuote premiumQuote;
        #endregion Details FX
        #region Details EQD
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool triggerEventSpecified;
        public EFS_TriggerEvent triggerEvent;
        #endregion Details EQD
        #region Details IRD
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool fixedRateSpecified;
        public EFS_Decimal fixedRate;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool dayCountFractionSpecified;
        public IEFS_DayCountFraction dayCountFraction;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool strikeSpecified;
        public EFS_Decimal strike;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool spreadSpecified;
        public EFS_Decimal spread;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool multiplierSpecified;
        public EFS_Decimal multiplier;
        #endregion Details IRD
        #region Details ADM
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool invoicingAmountBaseSpecified;
        public EFS_InvoicingAmountBase invoicingAmountBase;
        #endregion Details ADM
        #region Details ETD
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool etdPremiumCalculationSpecified;
        public EFS_ETDPremiumCalculation etdPremiumCalculation;
        #endregion Details ETD
        #region EventFee
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool paymentSourceSpecified;
        public EFS_PaymentSource paymentSource;
        public bool taxSourceSpecified;
        public EFS_TaxSource taxSource;
        #endregion EventFee
        #endregion Members
        #region Accessors
        #region IsEventDetSpecified
        public bool IsEventDetSpecified
        {
            get
            {
                bool isSpecified = paymentQuoteSpecified || exchangeRateSpecified || sideRateSpecified;
                isSpecified |= fixingRateSpecified || settlementRateSpecified || currencyPairSpecified;
                isSpecified |= triggerRateSpecified || strikePriceSpecified || premiumQuoteSpecified;
                isSpecified |= fixedRateSpecified || dayCountFractionSpecified;
                isSpecified |= strikeSpecified || spreadSpecified || multiplierSpecified;
                isSpecified |= invoicingAmountBaseSpecified;
                isSpecified |= triggerEventSpecified;
                isSpecified |= etdPremiumCalculationSpecified;
                isSpecified |= fixedPriceSpecified;
                isSpecified |= deliveryDateSpecified;;
                return isSpecified;
            }
        }

        #endregion IsEventDetSpecified
        #endregion Accessors
    }
    #endregion EFS_EventDetail
    #region EFS_EventKey
    public class EFS_EventKey : ICloneable
    {
        #region Members
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool idESpecified;
        [System.Xml.Serialization.XmlElementAttribute("idE")]
        public int idE;
        [System.Xml.Serialization.XmlElementAttribute("eventCode")]
        public string eventCode;
        [System.Xml.Serialization.XmlElementAttribute("eventType")]
        public string eventType;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool idE_SourceSpecified;
        [System.Xml.Serialization.XmlElementAttribute("idE_Source")]
        public int idE_Source;
        #endregion Members
        #region Constructors
        public EFS_EventKey() { }
        #endregion Constructors
        #region ICloneable Members
        #region Clone
        // EG 20231127 [WI755] Implementation Return Swap : Refactoring Code Analysis
        public object Clone()
        {
            EFS_EventKey clone = new EFS_EventKey
            {
                eventCode = this.eventCode,
                eventType = this.eventType,
                idE_SourceSpecified = this.idE_SourceSpecified,
                idE_Source = this.idE_Source
            };
            return clone;
        }
        #endregion Clone
        #endregion ICloneable Members
    }
    #endregion EFS_EventKey
    #region EFS_EventParent
    public class EFS_EventParent
    {
        #region Members
        public string eventCode;
        public string eventType;
        public string key;
        public string keyReference;
        public bool keyReferenceSpecified;
        public int idE;
        public bool idE_SourceSpecified;
        public int idE_Source;
        public int instrumentNo;
        public int streamNo;
        #endregion Members
        #region Constructors
        public EFS_EventParent(EFS_Event pEvent, int pIdE)
        {
            eventCode = pEvent.eventKey.eventCode;
            eventType = pEvent.eventKey.eventType;
            key = pEvent.id;
            keyReferenceSpecified = pEvent.eventReferenceSpecified;
            keyReference = pEvent.eventReference;
            instrumentNo = pEvent.instrumentNo;
            streamNo = pEvent.streamNo;
            idE = pIdE;
            idE_SourceSpecified = pEvent.eventKey.idE_SourceSpecified;
            if (idE_SourceSpecified)
                idE_Source = pEvent.eventKey.idE_Source;
        }
        #endregion Constructors
        #region Methods
        #region ChangeSequence
        // 20081211 EG Gestion de la prime d'un Swaption
        public void ChangeSequence(EFS_Event pEvent)
        {
            string childEventCode = pEvent.eventKey.eventCode;
            string childEventType = pEvent.eventKey.eventType;
            //
            if ((EventCodeFunc.IsTrade(eventCode)) &&
                (false == EventCodeFunc.IsProduct(childEventCode)) &&
                (false == EventCodeFunc.IsProductUnderlyer(childEventCode)) &&
                (false == EventCodeFunc.IsStrategy(childEventCode)))
            {
                pEvent.SetSequence(0, 0);
            }
            else if (EventCodeFunc.IsProduct(eventCode) && EventTypeFunc.IsPremium(childEventType) && (instrumentNo != pEvent.instrumentNo))
            {
                // prime d'un Swaption
                pEvent.SetSequence(instrumentNo, streamNo);
            }
            else if ((EventCodeFunc.IsProduct(eventCode) || EventCodeFunc.IsProductUnderlyer(eventCode)) &&
                (false == EventCodeFunc.IsLegGroup(childEventCode)))
            {
                pEvent.SetSequence(pEvent.instrumentNo, 0);
            }
            else if (EventCodeFunc.IsLegGroup(eventCode) && (false == EventCodeFunc.IsProductUnderlyer(childEventCode)) &&
                (instrumentNo != pEvent.instrumentNo))
            {
                pEvent.SetSequence(instrumentNo, streamNo);
            }
            else if (EventCodeFunc.IsOtherPartyPayment(eventCode))
            {
                pEvent.SetSequence(0, 0);
            }
        }
        #endregion GetSequence
        #endregion Methods
    }
    #endregion EFS_EventParent
    #region EFS_Events
    [System.Xml.Serialization.XmlRootAttribute("Events", IsNullable = false)]
    // EG 20180205 [23769] Add tradeLibrary to EFS_Events  
    // EG 20231127 [WI755] Implementation Return Swap : Refactoring Code Analysis
    public class EFS_Events
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("event")]
        public EFS_Event[] events;

        private string productName;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public string ProductName
        {
            get { return productName; }
        }

        [System.Xml.Serialization.XmlElementAttribute("party")]
        public IParty[] party;

        private readonly EFS_TradeLibrary tradeLibrary;
        private ArrayList eventResults;

        #endregion Members
        #region Constructors
        // EG 20180205 [23769] Add tradeLibrary to EFS_Events  
        public EFS_Events(EFS_TradeLibrary pTradeLibrary)
        {
            tradeLibrary = pTradeLibrary;
            party = tradeLibrary.Party;
        }
        #endregion Constructors
        #region Methods
        #region Calc
        /// <summary>
        /// Calculate events for a trade
        /// </summary>
        /// <param name="pProduct">FpML Product element</param>
        // EG 20180205 [23769] use tradeLibrary
        public void Calc()
        {
            productName = ((IProductBase)tradeLibrary.CurrentTrade.Product).ProductName;
            SetParameters(tradeLibrary, tradeLibrary.EventMatrix);

            #region Read EventMatrix
            eventResults = new ArrayList();
            EFS_EventMatrixTrade trade = tradeLibrary.EventMatrix.trade;
            if (null != trade)
            {
                SetParameters(tradeLibrary.CurrentTrade, trade);

                #region Group treatment (alert : recursivity)
                ReadEventMatrixGroup((EFS_EventMatrixGroup)trade);
                #endregion Group treatment (alert : recursivity)

                #region Copy ArrayList to Events class
                if (0 < eventResults.Count)
                    this.events = (EFS_Event[])eventResults.ToArray(eventResults[0].GetType());
                #endregion Copy ArrayList to Events class
            }
            #endregion Read EventMatrix
        }
        #endregion Calc
        #region CreateEvent
        private void CreateEvent(object pObject, EFS_EventMatrixItem pEventMatrixItem, EFS_EventMatrixGroupKey pEventMatrixGroupKey)
        {
            CreateEvent(pObject, pEventMatrixItem, pEventMatrixGroupKey, null);
        }
        private void CreateEvent(object pObject, EFS_EventMatrixItem pEventMatrixItem,
            EFS_EventMatrixGroupKey pEventMatrixGroupKeyParent, EFS_EventMatrixGroupKey pEventMatrixGroupKey)
        {

            bool isConditional = true;
            if (null != pObject)
            {
                #region Event parameters
                SetParameters(pObject, pEventMatrixItem);
                #endregion Event parameters
                #region Event Conditional
                if (pEventMatrixItem.conditionalSpecified)
                {
                    object result = SetEvent(pObject, pEventMatrixItem, "conditional");
                    if (null != result)
                        isConditional = Convert.ToBoolean(result);
                }
                #endregion Event Conditional

                if ((null != pEventMatrixItem.key) && isConditional)
                {
                    #region Event
                    EFS_EventKey efs_eventKey = new EFS_EventKey
                    {
                        eventCode = (string)SetEvent(pObject, pEventMatrixItem.key, "eventCode"),
                        eventType = (string)SetEvent(pObject, pEventMatrixItem.key, "eventType")
                    };
                    if (pEventMatrixItem.key.idE_SourceSpecified)
                    {
                        efs_eventKey.idE_Source = (int)SetEvent(pObject, pEventMatrixItem.key, "idE_Source");
                        efs_eventKey.idE_SourceSpecified = (0 < efs_eventKey.idE_Source);
                    }
                    #endregion Event

                    EFS_Event efs_event = new EFS_Event(efs_eventKey, pEventMatrixGroupKeyParent, pEventMatrixGroupKey);

                    #region Start & End Periods dates
                    efs_event.startDate = (EFS_EventDate)SetEvent(pObject, pEventMatrixItem, "startDate");
                    efs_event.endDate = (EFS_EventDate)SetEvent(pObject, pEventMatrixItem, "endDate");
                    #endregion Start & End Periods dates
                    #region EventClass
                    if (null != pEventMatrixItem.subItem)
                        CreateEventClass(pObject, efs_event, pEventMatrixItem.subItem);
                    #endregion EventClass
                    #region Payer & Receiver parties
                    if (pEventMatrixItem.payerSpecified)
                        efs_event.payer = (string)SetEvent(pObject, pEventMatrixItem, "payer");
                    efs_event.payerSpecified = (null != efs_event.payer);
                    if (pEventMatrixItem.receiverSpecified)
                        efs_event.receiver = (string)SetEvent(pObject, pEventMatrixItem, "receiver");
                    efs_event.receiverSpecified = (null != efs_event.receiver);
                    #endregion Payer & Receiver parties
                    #region Valorisation
                    if (pEventMatrixItem.valorisationSpecified)
                        efs_event.valorisation = (EFS_Decimal)SetEvent(pObject, pEventMatrixItem, "valorisation");
                    efs_event.valorisationSpecified = (null != efs_event.valorisation);
                    #endregion Valorisation
                    #region UnitType
                    efs_event.unitType = UnitTypeEnum.None;
                    if (pEventMatrixItem.unitTypeSpecified)
                    {
                        object result = SetEvent(pObject, pEventMatrixItem, "unitType");
                        if ((null != result) && System.Enum.IsDefined(typeof(UnitTypeEnum), result.ToString()))
                            efs_event.unitType = (UnitTypeEnum)System.Enum.Parse(typeof(UnitTypeEnum), result.ToString(), true);
                    }
                    efs_event.unitTypeSpecified = (UnitTypeEnum.None != efs_event.unitType);
                    #endregion UnitType
                    #region Unit
                    if (pEventMatrixItem.unitSpecified)
                        efs_event.unit = (string)SetEvent(pObject, pEventMatrixItem, "unit");
                    efs_event.unitSpecified = (null != efs_event.unit);
                    #endregion Unit
                    #region IdStCalcul
                    if (pEventMatrixItem.idStCalculSpecified)
                    {
                        object result = SetEvent(pObject, pEventMatrixItem, "idStCalcul");
                        if (null == result)
                            result = StatusCalculFunc.ToCalculate;
                        efs_event.idStCalcul = (StatusCalculEnum)System.Enum.Parse(typeof(StatusCalculEnum), result.ToString(), true);
                    }
                    #endregion IdStCalcul
                    #region Asset & Details
                    if (pEventMatrixItem.itemDetailsSpecified)
                        CreateEventDetails(efs_event, pObject, pEventMatrixItem.itemDetails);
                    #endregion Asset & Details

                    if ((pObject is IProductBase productBase)
                        && Tools.IsTypeOrInterfaceOf(pObject, InterfaceEnum.IProductBase)
                        && (null != productBase.ProductType))
                        efs_event.productId = productBase.ProductType.OTCmlId;
                    eventResults.Add(efs_event);
                }
            }
        }
        #endregion CreateEvent
        #region CreateEventClass
        // EG 20231127 [WI755] Implementation Return Swap : Refactoring Code Analysis
        private Cst.ErrLevel CreateEventClass(object pObject, EFS_Event pEfs_Event, EFS_EventMatrixSubItem[] pEventMatrixSubItem)
        {
            Cst.ErrLevel ret = Cst.ErrLevel.ABORTED;
            ArrayList aEventClass = new ArrayList();
            EFS_Date eventDate;
            EFS_EventMatrixSubItem subItem;
            string eventClass;
            for (int i = 0; i < pEventMatrixSubItem.Length; i++)
            {
                subItem = pEventMatrixSubItem[i];
                #region SubItem Conditional
                bool isConditional = true;
                if (subItem.conditionalSpecified)
                    isConditional = Convert.ToBoolean(SetEvent(pObject, subItem, "conditional"));
                #endregion SubItem Conditional
                if (isConditional)
                {
                    #region EventDate
                    eventDate = (EFS_Date)SetEvent(pObject, subItem, "eventDate");
                    #endregion EventDate
                    #region EventClass
                    eventClass = (string)SetEvent(pObject, subItem, "eventClass");
                    #endregion EventClass
                    #region IsPayment
                    bool isPayment = subItem.isPaymentSpecified;
                    if (isPayment)
                        isPayment = Convert.ToBoolean(SetEvent(pObject, subItem, "isPayment"));
                    #endregion IsPayment
                    EFS_EventClass efs_EventClass = new EFS_EventClass(eventClass, eventDate, isPayment)
                    {
                        eventDateSpecified = (null != eventDate)
                    };
                    aEventClass.Add(efs_EventClass);
                }
            }
            if (0 < aEventClass.Count)
                pEfs_Event.eventClass = (EFS_EventClass[])aEventClass.ToArray(typeof(EFS_EventClass));

            return ret;
        }
        #endregion CreateEventClass
        #region CreateEventOccurrence
        private void CreateEventOccurrence(object pObject, EFS_EventMatrixItem pEventMatrixItem, EFS_EventMatrixGroupKey pEventMatrixGroupKey)
        {
            if (false == pEventMatrixItem.occurenceSpecified)
                throw new Exception("No EventOccurence");
            //
            object @Object = pObject;
            //
            if (null != @Object)
            {
                EFS_EventMatrixOccurence occurence = pEventMatrixItem.occurence;
                if (occurence.fieldSpecified)
                {
                    string[] fields = occurence.field.Split(new char[] { '/' });
                    for (int i = 0; i < fields.Length; i++)
                    {
                        FieldInfo fld = @Object.GetType().GetField(fields[i]);
                        if (null != fld)
                            @Object = fld.GetValue(@Object);
                        if ((null == fld) || (null == @Object))
                            break;
                    }
                }
            }
            // EG 20111026
            if (null != @Object)
            {
                if (@Object.GetType().IsArray)
                {
                    GetSequenceOccurence(@Object, pEventMatrixItem, out int start, out int end);
                    for (int i = start; i < end; i++)
                        CreateEvent(((Array)@Object).GetValue(i), pEventMatrixItem, pEventMatrixGroupKey);
                }
                else
                    CreateEvent(@Object, pEventMatrixItem, pEventMatrixGroupKey);
            }
        }
        #endregion CreateEventOccurrence
        #region CreateEventDetails
        private Cst.ErrLevel CreateEventDetails(EFS_Event pEfs_event, object pObject, EFS_EventMatrixItemDetails pEventMatrixItemDetails)
        {
            pEfs_event.eventDetail = new EFS_EventDetail();
            EFS_EventDetail eventDetail = pEfs_event.eventDetail;
            #region Assets
            #region IdAsset
            if (pEventMatrixItemDetails.assetSpecified)
                eventDetail.asset = (EFS_Asset)SetEvent(pObject, pEventMatrixItemDetails, "asset");
            eventDetail.assetSpecified = (null != eventDetail.asset);
            #endregion IdAsset
            #region IdAsset2
            if (pEventMatrixItemDetails.asset2Specified)
                eventDetail.asset2 = (EFS_Asset)SetEvent(pObject, pEventMatrixItemDetails, "asset2");
            eventDetail.asset2Specified = (null != eventDetail.asset2);
            #endregion IdAsset
            #endregion Assets

            #region COMD Details
            #region FixedPrice
            if (pEventMatrixItemDetails.fixedPriceSpecified)
                eventDetail.fixedPrice = (EFS_FixedPrice)SetEvent(pObject, pEventMatrixItemDetails, "fixedPrice");
            eventDetail.fixedPriceSpecified = (null != eventDetail.fixedPrice);
            #endregion FixedPrice
            #region DeliveryDate
            if (pEventMatrixItemDetails.deliveryDateSpecified)
                eventDetail.deliveryDate = (EFS_CommodityDeliveryDate)SetEvent(pObject, pEventMatrixItemDetails, "deliveryDate");
            eventDetail.deliveryDateSpecified = (null != eventDetail.deliveryDate);
            #endregion DeliveryDate
            #endregion COMD Details
            #region FX Details
            #region ExchangeRate
            if (pEventMatrixItemDetails.exchangeRateSpecified)
                eventDetail.exchangeRate = (EFS_ExchangeRate)SetEvent(pObject, pEventMatrixItemDetails, "exchangeRate");
            eventDetail.exchangeRateSpecified = (null != eventDetail.exchangeRate);
            #endregion ExchangeRate
            #region PaymentQuote
            if (pEventMatrixItemDetails.paymentQuoteSpecified)
                eventDetail.paymentQuote = (EFS_PaymentQuote)SetEvent(pObject, pEventMatrixItemDetails, "paymentQuote");
            eventDetail.paymentQuoteSpecified = (null != eventDetail.paymentQuote);
            #endregion ExchangeRate
            #region SideRate
            if (pEventMatrixItemDetails.sideRateSpecified)
                eventDetail.sideRate = (ISideRate)SetEvent(pObject, pEventMatrixItemDetails, "sideRate");
            eventDetail.sideRateSpecified = (null != eventDetail.sideRate);
            #endregion SideRate
            #region FixingRate
            if (pEventMatrixItemDetails.fixingRateSpecified)
                eventDetail.fixingRate = (EFS_FxFixing)SetEvent(pObject, pEventMatrixItemDetails, "fixingRate");
            eventDetail.fixingRateSpecified = (null != eventDetail.fixingRate);
            #endregion FixingRate
            #region SettlementRate
            if (pEventMatrixItemDetails.settlementRateSpecified)
                eventDetail.settlementRate = (EFS_SettlementRate)SetEvent(pObject, pEventMatrixItemDetails, "settlementRate");
            eventDetail.settlementRateSpecified = (null != eventDetail.settlementRate);
            #endregion SettlementRate
            #region CurrencyPair
            if (pEventMatrixItemDetails.currencyPairSpecified)
                eventDetail.currencyPair = (IQuotedCurrencyPair)SetEvent(pObject, pEventMatrixItemDetails, "currencyPair");
            eventDetail.currencyPairSpecified = (null != eventDetail.currencyPair);
            #endregion CurrencyPair
            #region TriggerRate
            if (pEventMatrixItemDetails.triggerRateSpecified)
                eventDetail.triggerRate = (EFS_TriggerRate)SetEvent(pObject, pEventMatrixItemDetails, "triggerRate");
            eventDetail.triggerRateSpecified = (null != eventDetail.triggerRate);
            #endregion TriggerRate
            #region StrikePrice
            if (pEventMatrixItemDetails.strikePriceSpecified)
                eventDetail.strikePrice = (IFxStrikePrice)SetEvent(pObject, pEventMatrixItemDetails, "strikePrice");
            eventDetail.strikePriceSpecified = (null != eventDetail.strikePrice);
            #endregion StrikePrice
            #region PremiumQuote
            if (pEventMatrixItemDetails.premiumQuoteSpecified)
                eventDetail.premiumQuote = (EFS_FxPremiumQuote)SetEvent(pObject, pEventMatrixItemDetails, "premiumQuote");
            eventDetail.premiumQuoteSpecified = (null != eventDetail.premiumQuote);
            #endregion PremiumQuote
            #endregion FX Details
            #region IRD Details
            #region FixedRate
            if (pEventMatrixItemDetails.fixedRateSpecified)
                eventDetail.fixedRate = (EFS_Decimal)SetEvent(pObject, pEventMatrixItemDetails, "fixedRate");
            eventDetail.fixedRateSpecified = (null != eventDetail.fixedRate);
            #endregion FixedRate
            #region DayCountFraction
            if (pEventMatrixItemDetails.dayCountFractionSpecified)
                eventDetail.dayCountFraction = (IEFS_DayCountFraction)SetEvent(pObject, pEventMatrixItemDetails, "dayCountFraction");
            eventDetail.dayCountFractionSpecified = (null != eventDetail.dayCountFraction);
            #endregion DayCountFraction
            #region Strike
            if (pEventMatrixItemDetails.strikeSpecified)
                eventDetail.strike = (EFS_Decimal)SetEvent(pObject, pEventMatrixItemDetails, "strike");
            eventDetail.strikeSpecified = (null != eventDetail.strike);
            #endregion Strike
            #region Spread
            if (pEventMatrixItemDetails.spreadSpecified)
                eventDetail.spread = (EFS_Decimal)SetEvent(pObject, pEventMatrixItemDetails, "spread");
            eventDetail.spreadSpecified = (null != eventDetail.spread);
            #endregion Spread
            #region Multiplier
            if (pEventMatrixItemDetails.multiplierSpecified)
                eventDetail.multiplier = (EFS_Decimal)SetEvent(pObject, pEventMatrixItemDetails, "multiplier");
            eventDetail.multiplierSpecified = (null != eventDetail.multiplier);
            #endregion Multiplier
            #endregion IRD Details
            #region ADM Details
            #region InvoicingAmountBase
            if (pEventMatrixItemDetails.invoicingAmountBaseSpecified)
                eventDetail.invoicingAmountBase = (EFS_InvoicingAmountBase)SetEvent(pObject, pEventMatrixItemDetails, "invoicingAmountBase");
            eventDetail.invoicingAmountBaseSpecified = (null != eventDetail.invoicingAmountBase);
            #endregion InvoicingAmountBase
            #endregion ADM Details
            #region ETD Details
            #region PremiumCalculation
            if (pEventMatrixItemDetails.etdPremiumSpecified)
                eventDetail.etdPremiumCalculation = (EFS_ETDPremiumCalculation)SetEvent(pObject, pEventMatrixItemDetails, "etdPremium");
            eventDetail.etdPremiumCalculationSpecified = (null != eventDetail.etdPremiumCalculation);
            #endregion PremiumCalculation
            #endregion ETD Details
            #region EQD Details
            #region TriggerEvent
            if (pEventMatrixItemDetails.triggerEventSpecified)
                eventDetail.triggerEvent = (EFS_TriggerEvent)SetEvent(pObject, pEventMatrixItemDetails, "triggerEvent");
            eventDetail.triggerEventSpecified = (null != eventDetail.triggerEvent);
            #endregion TriggerEvent
            #endregion EQD Details

            #region EventFee
            #region PaymentSource
            if (pEventMatrixItemDetails.paymentSourceSpecified)
                eventDetail.paymentSource = (EFS_PaymentSource)SetEvent(pObject, pEventMatrixItemDetails, "paymentSource");
            eventDetail.paymentSourceSpecified = (null != eventDetail.paymentSource);
            #endregion PaymentSource
            #region TaxSource
            if (pEventMatrixItemDetails.taxSourceSpecified)
                eventDetail.taxSource = (EFS_TaxSource)SetEvent(pObject, pEventMatrixItemDetails, "taxSource");
            eventDetail.taxSourceSpecified = (null != eventDetail.taxSource);
            #endregion TaxSource
            #endregion EventFee
            return Cst.ErrLevel.SUCCESS;
        }
        #endregion CreateEventDetails

        #region GetField
        private object GetField(object pObject, EFS_EventMatrixItem pItemGroup, bool pIsObjectSpecified)
        {

            #region FieldSpecified
            object _obj = pObject;
            if (pItemGroup.occurence.fieldSpecified)
            {
                string[] fields = pItemGroup.occurence.field.Split(new char[] { '/' });
                for (int j = 0; j < fields.Length; j++)
                {
                    FieldInfo fld = _obj.GetType().GetField(fields[j]);
                    if (null != fld)
                    {
                        if (pItemGroup.occurence.isFieldOptional)
                        {
                            string fieldSpecified = fields[j] + Cst.FpML_SerializeKeySpecified;
                            ArrayList aIsSpecified = ReflectionTools.GetObjectByName(pItemGroup, fieldSpecified, true);
                            _ = (0 == aIsSpecified.Count) || Boolean.Equals(pIsObjectSpecified, aIsSpecified[0]);
                        }
                        _obj = fld.GetValue(_obj);
                    }
                    else
                    {
                        throw new Exception(StrFunc.AppendFormat("[Field:{0}] doesn't exist in [Object:{1}]", fields[j], _obj.GetType().ToString()));
                    }
                }
            }
            return _obj;
            #endregion FieldSpecified
        }
        #endregion GetField
        #region GetMethod
        private MethodInfo GetMethod(Type pTObject, string pMethodName)
        {
            MethodInfo method = pTObject.GetMethod(pMethodName);
            if (null == method)
            {
                Type @interface = pTObject.GetInterface("I" + pTObject.Name);
                if (null != @interface)
                    method = @interface.GetMethod(pMethodName);

                if (null == method)
                {
                    Type[] @interfaces = pTObject.GetInterfaces();
                    if (ArrFunc.IsFilled(@interfaces))
                    {
                        foreach (Type item in @interfaces)
                        {
                            method = GetMethod(item, pMethodName);
                            if (null != method)
                                break;
                        }
                    }
                }
            }
            return method;

        }
        #endregion GetMethod
        #region GetProperty
        private PropertyInfo GetProperty(Type pTObject, string pPropertyName)
        {
            PropertyInfo pty = pTObject.GetProperty(pPropertyName);
            if (null == pty)
            {
                Type @interface = pTObject.GetInterface("I" + pTObject.Name);
                if (null != @interface)
                    pty = @interface.GetProperty(pPropertyName);
                if (null == pty)
                {
                    Type[] @interfaces = pTObject.GetInterfaces();
                    if (ArrFunc.IsFilled(@interfaces))
                    {
                        foreach (Type item in @interfaces)
                        {
                            pty = GetProperty(item, pPropertyName);
                            if (null != pty)
                                break;
                        }
                    }
                }
            }
            return pty;

        }
        #endregion GetProperty
        #region GetSequenceOccurence
        private static void GetSequenceOccurence(object pObject, EFS_EventMatrixItem pEventMatrixItem, out int opStartIndex, out int opEndIndex)
        {
            int start = 0;
            int end = 0;
            if (pObject.GetType().IsArray)
            {
                end = ((Array)pObject).Length;
                if (pEventMatrixItem.occurenceSpecified)
                {
                    EFS_EventMatrixOccurence occurence = pEventMatrixItem.occurence;
                    EventOccurenceEnum occurenceValue = (EventOccurenceEnum)System.Enum.Parse(typeof(EventOccurenceEnum), occurence.to, true);
                    switch (occurenceValue)
                    {
                        case EventOccurenceEnum.All:
                            break;
                        case EventOccurenceEnum.AllExceptFirst:
                            start++;
                            break;
                        case EventOccurenceEnum.AllExceptFirstAndLast:
                            start++;
                            end--;
                            break;
                        case EventOccurenceEnum.AllExceptLast:
                            end--;
                            break;
                        case EventOccurenceEnum.First:
                        case EventOccurenceEnum.Unique:
                            end = start + 1;
                            break;
                        case EventOccurenceEnum.Last:
                            start = end - 1;
                            break;
                        case EventOccurenceEnum.None:
                            end = start;
                            break;
                        case EventOccurenceEnum.Item:
                            if (occurence.positionSpecified)
                                start = Convert.ToInt32(occurence.position) - 1;
                            end = start + 1;
                            break;

                    }
                }
            }
            opStartIndex = start;
            opEndIndex = end;
        }
        #endregion GetSequenceOccurence

        #region MemberType
        private static EFS_FieldMemberEnum MemberType(EFS_EventMatrixField pEventMatrixField)
        {

            string fieldValue = pEventMatrixField.Value;
            if (null != fieldValue)
            {
                if (("0" == fieldValue) || ("1" == fieldValue))
                    return EFS_FieldMemberEnum.Boolean;
                else if (fieldValue.StartsWith("[") && fieldValue.EndsWith("]"))
                    return EFS_FieldMemberEnum.Constant;
                else if (-1 < fieldValue.IndexOf("|"))
                {
                    if (fieldValue.EndsWith(")"))
                        return EFS_FieldMemberEnum.StaticMethod;
                    else
                        return EFS_FieldMemberEnum.StaticProperty;
                }
                else if (fieldValue.EndsWith(")"))
                    return EFS_FieldMemberEnum.Method;
                else
                    return EFS_FieldMemberEnum.Property;
            }
            return EFS_FieldMemberEnum.Unknown;

        }
        #endregion MemberType

        #region ReadEventMatrixGroup
        /// <summary>Calculate events GROUP informative</summary>
        /// <param name="pEventMatrixGroup">EventMatrixGroup element</param>
        /// <returns>Integer ErrorLevel (-1 = Succes)</returns>
        // EG 20180205 [23769] use tradeLibrary
        private void ReadEventMatrixGroup(EFS_EventMatrixGroup pEventMatrixGroup)
        {
            ReadEventMatrixGroup(tradeLibrary.DataDocument.DataDocument, pEventMatrixGroup);
        }
        private void ReadEventMatrixGroup(object pObject, EFS_EventMatrixGroup pEventMatrixGroup)
        {
            ReadEventMatrixGroup(pObject, pEventMatrixGroup, null);
        }
        // EG 20231127 [WI755] Implementation Return Swap : Add test on to2 attribute
        private void ReadEventMatrixGroup(object pObject, EFS_EventMatrixGroup pEventMatrixGroup, EFS_EventMatrixGroupKey pKeyGroupParent)
        {

            EFS_EventMatrixItem itemGroup = pEventMatrixGroup.itemGroup;

            if (itemGroup.occurenceSpecified)
            {
                #region private variables
                bool IsUnique = (itemGroup.occurence.to == EventOccurenceEnum.Unique.ToString() || itemGroup.occurence.to2 == EventOccurenceEnum.Unique.ToString());
                bool isObjectSpecified = true;
                ArrayList aObjectGroup = new ArrayList();
                #endregion private variables
                #region Return if exist value of "occurence + Cst.FpML_SerializeKeySpecified"
                if (itemGroup.occurence.isOptional)
                {
                    string fieldSpecified = itemGroup.occurence.Value + Cst.FpML_SerializeKeySpecified;
                    ArrayList aIsSpecified = ReflectionTools.GetObjectByNameSorted(pObject, fieldSpecified, true);
                    isObjectSpecified = (0 == aIsSpecified.Count) || Boolean.Equals(isObjectSpecified, aIsSpecified[0]);
                }
                #endregion Return if exist value of "occurence + Cst.FpML_SerializeKeySpecified"
                #region Get object
                if (isObjectSpecified)
                    aObjectGroup = ReflectionTools.GetObjectByNameSorted(pObject, itemGroup.occurence.Value, IsUnique);
                #endregion Get object
                #region Loop in object occurence
                for (int i = 0; i < aObjectGroup.Count; i++)
                {
                    object objectGroup = aObjectGroup[i];

                    if (objectGroup.GetType().IsArray)
                    {
                        GetSequenceOccurence(objectGroup, pEventMatrixGroup.itemGroup, out int objStart, out int objEnd);
                        for (int j = objStart; j < objEnd; j++)
                        {
                            object obj = ((Array)objectGroup).GetValue(j);
                            obj = GetField(obj, itemGroup, isObjectSpecified);
                            if (null != obj)
                            {
                                int idGroup = 1;
                                if (obj.GetType().IsArray)
                                {
                                    GetSequenceOccurence(obj, pEventMatrixGroup.itemGroup, out int start, out int end);
                                    idGroup = start + 1;
                                    for (int k = start; k < end; k++)
                                    {
                                        object obj2 = ((Array)obj).GetValue(k);
                                        TrtEventMatrixGroup(obj2, pEventMatrixGroup, pKeyGroupParent, idGroup);
                                        idGroup++;
                                    }
                                }
                                else
                                {
                                    TrtEventMatrixGroup(obj, pEventMatrixGroup, pKeyGroupParent, idGroup);
                                }
                            }

                        }

                    }
                    else
                    {
                        objectGroup = GetField(objectGroup, itemGroup, isObjectSpecified);
                        if (null != objectGroup)
                        {
                            int idGroup = 1;
                            if (objectGroup.GetType().IsArray)
                            {
                                GetSequenceOccurence(objectGroup, pEventMatrixGroup.itemGroup, out int start, out int end);
                                idGroup = start + 1;
                                for (int j = start; j < end; j++)
                                {
                                    object obj = ((Array)objectGroup).GetValue(j);
                                    TrtEventMatrixGroup(obj, pEventMatrixGroup, pKeyGroupParent, idGroup);
                                    idGroup++;
                                }
                            }
                            else
                            {
                                TrtEventMatrixGroup(objectGroup, pEventMatrixGroup, pKeyGroupParent, idGroup);
                            }
                        }
                    }
                }
                #endregion Loop in object occurence
            }
            else
            {
                TrtEventMatrixGroup(pObject, pEventMatrixGroup, pKeyGroupParent, 1);
            }
        }
        #endregion ReadEventMatrixGroup
        #region ReadEventMatrixItems
        private void ReadEventMatrixItems(object pObject, EFS_EventMatrixGroup pEventMatrixGroup)
        {

            if (null != pObject)
            {
                EFS_EventMatrixItem[] items = pEventMatrixGroup.item;
                EFS_EventMatrixGroupKey keyGroup = pEventMatrixGroup.keyGroup;
                if (null != items)
                {
                    foreach (EFS_EventMatrixItem item in items)
                    {
                        if (item.occurenceSpecified)
                        {
                            #region private variables
                            bool isObjectSpecified = true;
                            ArrayList aObjectOccurence = new ArrayList();
                            bool IsUnique = (item.occurence.to == EventOccurenceEnum.Unique.ToString());
                            #endregion private variables

                            #region Return if exist value of "occurence + Cst.FpML_SerializeKeySpecified"
                            if (item.occurence.isOptional)
                            {
                                string fieldSpecified = item.occurence.Value + Cst.FpML_SerializeKeySpecified;
                                ArrayList aIsSpecified = ReflectionTools.GetObjectByNameSorted(pObject, fieldSpecified, true);
                                isObjectSpecified = (0 == aIsSpecified.Count) || Boolean.Equals(isObjectSpecified, aIsSpecified[0]);
                            }
                            #endregion Return if exist value of "occurence + Cst.FpML_SerializeKeySpecified"
                            if (isObjectSpecified)
                                aObjectOccurence = ReflectionTools.GetObjectByNameSorted(pObject, item.occurence.Value, IsUnique);

                            for (int i = 0; i < aObjectOccurence.Count; i++)
                                CreateEventOccurrence(aObjectOccurence[i], item, keyGroup);
                        }
                        else
                            CreateEvent(pObject, item, keyGroup);
                    }
                }
            }
        }
        #endregion ReadEventMatrixItems
        #region ReadEventMatrixSubGroup
        private void ReadEventMatrixSubGroup(object pObject, EFS_EventMatrixGroup pEventMatrixGroup, EFS_EventMatrixGroupKey pKeyGroupParent)
        {

            Type tEventMatrixGroup = pEventMatrixGroup.GetType();
            FieldInfo fld = tEventMatrixGroup.GetField("group");
            if (null != fld)
            {
                object obj = fld.GetValue(pEventMatrixGroup);
                if (null != obj)
                {
                    Type tObj = obj.GetType();
                    if (tObj.IsArray)

                        foreach (EFS_EventMatrixGroup subEventMatrixGroup in (EFS_EventMatrixGroup[])obj)
                            ReadEventMatrixGroup(pObject, subEventMatrixGroup, pKeyGroupParent);
                    else
                    {
                        if (obj.GetType().BaseType.Equals(typeof(EFS_EventMatrixProduct)))
                            if (productName == ((EFS_EventMatrixProduct)obj).productName)
                                ReadEventMatrixGroup(pObject, (EFS_EventMatrixGroup)obj, pKeyGroupParent);
                    }
                }
            }
        }
        #endregion ReadEventMatrixSubGroup

        #region SetEvent
        /// <summary>
        /// Retourne la valeur présente dans pObject déclarée via pEvent 
        /// </summary>
        /// <param name="pObject"></param>
        /// <param name="pEvent">Repésente un membres, une méthode, etc...</param>
        /// <param name="pEventField"></param>
        /// <returns></returns>
        private object SetEvent(object pObject, object pEvent, string pEventField)
        {

            object result = null;
            Type tEvent = pEvent.GetType();
            FieldInfo fld = tEvent.GetField(pEventField);
            if (null != fld)
            {
                object obj = fld.GetValue(pEvent);
                if (null != obj)
                {
                    if (obj.GetType().Equals(typeof(EFS_EventMatrixField)))
                    {
                        EFS_EventMatrixField eventMatrixField = (EFS_EventMatrixField)obj;
                        EFS_FieldMemberEnum fieldMemberEnum = MemberType(eventMatrixField);
                        switch (fieldMemberEnum)
                        {
                            case EFS_FieldMemberEnum.Boolean:
                                #region Boolean
                                result = BoolFunc.IsTrue(eventMatrixField.Value);
                                #endregion Boolean
                                break;
                            case EFS_FieldMemberEnum.Property:
                                #region Property
                                result = SetEventByProperty(pObject, eventMatrixField);
                                #endregion Property
                                break;
                            case EFS_FieldMemberEnum.Method:
                                #region Method
                                if (pEvent.GetType().Equals(typeof(EFS_EventMatrixItem)) ||
                                    pEvent.GetType().BaseType.Equals(typeof(EFS_EventMatrixItemDetails)))
                                    result = SetEventByMethod(pObject, eventMatrixField, pEvent);
                                else
                                    result = SetEventByMethod(pObject, eventMatrixField, null);
                                #endregion Method
                                break;
                            case EFS_FieldMemberEnum.Constant:
                                #region Constant
                                result = SetEventByConstant(eventMatrixField);
                                #endregion Constant
                                break;
                            case EFS_FieldMemberEnum.StaticMethod:
                                #region Static Method
                                if (pEvent.GetType().Equals(typeof(EFS_EventMatrixItem)) ||
                                    pEvent.GetType().BaseType.Equals(typeof(EFS_EventMatrixItemDetails)))
                                    result = SetEventByStaticMethod(pObject, eventMatrixField, pEvent);
                                else
                                    result = SetEventByStaticMethod(pObject, eventMatrixField, null);
                                #endregion Static Method
                                break;
                            case EFS_FieldMemberEnum.StaticProperty:
                                #region Static Property
                                result = SetEventByStaticProperty(eventMatrixField);
                                #endregion Static Property
                                break;
                            default:
                                break;
                        }
                    }
                    else if (obj.GetType().Equals(typeof(EFS_EventMatrixFieldReference)))
                    {
                        #region Parameter
                        result = SetEventByReference((EFS_EventMatrixFieldReference)obj);
                        #endregion Parameter
                    }
                }
            }
            return result;
        }
        #endregion SetEvent
        #region SetEventByConstant
        private static object SetEventByConstant(EFS_EventMatrixField pEventMatrixField)
        {
            return pEventMatrixField.Value.Substring(1, pEventMatrixField.Value.Length - 2);
        }
        #endregion SetEventByConstant
        #region SetEventByMethod
        /// <summary>
        /// Retourne le résultat d'une fonction présente dans {pObject}
        /// </summary>
        /// <param name="pObject"></param>
        /// <param name="pEventMatrixField">Représente la méthode à appelée</param>
        /// <param name="pEventMatrixItemProduct"></param>
        /// <returns></returns>
        // EG 20180205 [23769] Call ReflectionTools.GetObjectById (substitution to the static class EFS_CURRENT)  
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        // EG 20231127 [WI755] Implementation Return Swap : Refactoring Code Analysis
        private object SetEventByMethod(object pObject, EFS_EventMatrixField pEventMatrixField, object pEventMatrixItemProduct)
        {
            object result = null;
            //
            Type tObject = pObject.GetType();
            if (tObject.IsArray)
                tObject = tObject.GetElementType();

            string[] sTemp = pEventMatrixField.Value.Split("(".ToCharArray());
            string func = sTemp[0].Replace("(", string.Empty);

            MethodInfo method = GetMethod(tObject, func);
            //#if DEBUG
            //            if (null == method)
            //                throw new Exception(StrFunc.AppendFormat("Method[name:{0}] not found in object[name:{1}]", method, tObject.ToString()));
            //#endif

            if (null != method)
            {
                object[] argValues = null;
                #region détermination des arguments de la méthode
                sTemp[1] = sTemp[1].Replace(")", string.Empty);
                if (StrFunc.IsFilled(sTemp[1]))
                {
                    string[] parameters = sTemp[1].Split(",".ToCharArray());
                    argValues = new object[parameters.Length];
                    for (int i = 0; i < parameters.Length; i++)
                    {
                        if (-1 < parameters[i].IndexOf("'"))
                        {
                            argValues[i] = parameters[i].Replace("'", string.Empty);
                        }
                        else
                        {
                            object parameter = null;
                            if (null != pEventMatrixItemProduct)
                                parameter = ReflectionTools.GetObjectById(pEventMatrixItemProduct, parameters[i]);

                            if (null == parameter)
                                parameter = ReflectionTools.GetObjectById(tradeLibrary.EventMatrix, parameters[i]);

                            if ((null != parameter) && (parameter is EFS_EventMatrixParameter parameter1))
                                argValues[i] = parameter1.result;
                        }
                    }
                }
                #endregion
                result = method.DeclaringType.InvokeMember(method.Name, BindingFlags.InvokeMethod, null, pObject, argValues, null, null, null);
            }
            return result;
        }
        #endregion SetEventByMethod
        #region SetEventByProperty
        private object SetEventByProperty(object pObject, EFS_EventMatrixField pEventMatrixField)
        {
            object result = null;
            PropertyInfo pty = null;
            //
            Type tObject = pObject.GetType();
            if (tObject.IsArray)
                tObject = tObject.GetElementType();
            //
            if (pEventMatrixField.declaringTypeSpecified)
            {
                pty = GetProperty(tObject, pEventMatrixField.declaringType);
#if debug
                if (null == pty)
                    throw new Exception(StrFunc.AppendFormat("Property[name:{0}] not found in object[name:{1}]", pEventMatrixField.declaringType, tObject.ToString()));
#endif
                if (null != pty)
                {
                    object target = pty.DeclaringType.InvokeMember(pty.Name, BindingFlags.GetProperty, null, pObject, null);
                    if (null != target)
                    {
                        tObject = target.GetType();
                        if (tObject.IsArray)
                            tObject = tObject.GetElementType();
                        pty = GetProperty(tObject, pEventMatrixField.Value);
                        if (null != pty)
                            result = pty.DeclaringType.InvokeMember(pty.Name, BindingFlags.GetProperty, null, target, null);
                    }
                }

            }
            else
            {
                pty = GetProperty(tObject, pEventMatrixField.Value);
#if debug
                if (null == pty)
                    throw new Exception(StrFunc.AppendFormat("Property[name:{0}] not found in object[name:{1}]", pEventMatrixField.Value, tObject.ToString()));
#endif
                if (null != pty)
                    result = pty.DeclaringType.InvokeMember(pty.Name, BindingFlags.GetProperty, null, pObject, null);
            }
            return result;
        }
        #endregion SetEventByProperty
        #region SetEventByStaticMethod
        // EG 20180205 [23769] Call ReflectionTools.GetObjectById (substitution to the static class EFS_CURRENT)  
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        private object SetEventByStaticMethod(object pObject, EFS_EventMatrixField pEventMatrixField, object pEventMatrixItemProduct)
        {
            object result = null;
            string[] staticMethodInfo = pEventMatrixField.Value.Split('|');
            string className = string.Empty;
            string fullMethod = string.Empty;
            string assemblyName = string.Empty;

            if (1 < staticMethodInfo.Length)
            {
                className = staticMethodInfo[0];
                if (2 == staticMethodInfo.Length)
                {
                    assemblyName = staticMethodInfo[0].Substring(0, staticMethodInfo[0].LastIndexOf("."));
                    fullMethod = staticMethodInfo[1];
                }
                else
                {
                    assemblyName = staticMethodInfo[1];
                    fullMethod = staticMethodInfo[2];
                }

                string[] sTemp = fullMethod.Split("(".ToCharArray());
                string methodName = sTemp[0].Replace("(", string.Empty);

                Type tStaticClass = Type.GetType(className + "," + assemblyName, true, false);
#if debug
                if (null == tStaticClass)
                    throw new Exception(StrFunc.AppendFormat("Class[name:{0},assembly{1}] not found", className, assemblyName));
#endif
                MethodInfo method = tStaticClass.GetMethod(methodName);
#if debug
                if (null == method)
                    throw new Exception(StrFunc.AppendFormat("Method[name:{0}] not found in object[name:{1}]", methodName, tStaticClass.ToString()));
#endif
                //
                if (null != method)
                {
                    string[] parameters = sTemp[1].Replace(")", string.Empty).Split(",".ToCharArray());
                    object[] argValues = new object[parameters.Length];
                    for (int i = 0; i < parameters.Length; i++)
                    {
                        if (-1 < parameters[i].IndexOf("'"))
                            argValues[i] = parameters[i].Replace("'", string.Empty);
                        else
                        {
                            object parameter = null;
                            if (null != pEventMatrixItemProduct)
                                parameter = ReflectionTools.GetObjectById(pEventMatrixItemProduct, parameters[i]);
                            if (null == parameter)
                                parameter = ReflectionTools.GetObjectById(tradeLibrary.EventMatrix, parameters[i]);
                            if (null != parameter)
                                argValues[i] = ((EFS_EventMatrixParameter)parameter).result;
                        }
                    }
                    result = tStaticClass.InvokeMember(method.Name, BindingFlags.InvokeMethod, null, null, argValues, null, null, null);
                }
            }
            return result;
        }
        #endregion SetEventByStaticMethod
        #region SetEventByStaticProperty
        private static object SetEventByStaticProperty(EFS_EventMatrixField pEventMatrixField)
        {

            object result = null;
            string[] staticPropertyInfo = pEventMatrixField.Value.Split('|');
            string className = string.Empty;
            string ptyName = string.Empty;
            string assemblyName = string.Empty;

            if (1 < staticPropertyInfo.Length)
            {
                className = staticPropertyInfo[0];
                if (2 == staticPropertyInfo.Length)
                {
                    assemblyName = staticPropertyInfo[0].Substring(0, staticPropertyInfo[0].LastIndexOf("."));
                    ptyName = staticPropertyInfo[1];
                }
                else
                {
                    assemblyName = staticPropertyInfo[1];
                    ptyName = staticPropertyInfo[2];
                }
                Type tStaticClass = Type.GetType(className + "," + assemblyName, true, false);
#if debug
                if (null == tStaticClass)
                    throw new Exception(StrFunc.AppendFormat("Class[name:{0},assembly{1}] not found", className, assemblyName));
#endif
                //
                PropertyInfo pty = tStaticClass.GetProperty(ptyName);
#if debug
                if (null == pty)
                    throw new Exception(StrFunc.AppendFormat("Property[name:{0}] not found in object[name:{1}]", ptyName, tStaticClass.ToString()));
#endif
                //
                if (null != pty)
                    result = tStaticClass.InvokeMember(pty.Name, BindingFlags.GetProperty, null, null, null);

            }
            return result;

        }
        #endregion SetEventByStaticProperty
        #region SetEventByReference
        // EG 20180205 [23769] Call ReflectionTools.GetObjectById (substitution to the static class EFS_CURRENT)  
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        private object SetEventByReference(EFS_EventMatrixFieldReference pEventMatrixField)
        {
            object result = null;
            object parameter = ReflectionTools.GetObjectById(tradeLibrary.EventMatrix, pEventMatrixField.hRef);
            if (null != parameter)
                result = ((EFS_EventMatrixParameter)parameter).result;
            return result;
        }
        #endregion SetEventByReference
        #region SetParameter
        // EG 20180205 [23769] Call ReflectionTools.GetObjectById (substitution to the static class EFS_CURRENT)  
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        private object SetParameter(object pObject, EFS_EventMatrixParameter pEventMatrixParameter)
        {
            object result = null;
            if (StrFunc.IsEmpty(pEventMatrixParameter.hRef))
            {
                EFS_FieldMemberEnum fieldMemberEnum = MemberType(pEventMatrixParameter);
                switch (fieldMemberEnum)
                {
                    case EFS_FieldMemberEnum.Property:
                        #region Property
                        result = SetEventByProperty(pObject, (EFS_EventMatrixField)pEventMatrixParameter);
                        #endregion Property
                        break;
                    case EFS_FieldMemberEnum.Method:
                        #region Method
                        result = SetEventByMethod(pObject, (EFS_EventMatrixField)pEventMatrixParameter, null);
                        #endregion Method
                        break;
                    case EFS_FieldMemberEnum.Constant:
                        #region Constant
                        result = SetEventByConstant((EFS_EventMatrixField)pEventMatrixParameter);
                        #endregion Constant
                        break;
                    case EFS_FieldMemberEnum.StaticProperty:
                        #region Constant
                        result = SetEventByStaticProperty((EFS_EventMatrixField)pEventMatrixParameter);
                        #endregion Constant
                        break;
                    case EFS_FieldMemberEnum.StaticMethod:
                        #region Constant
                        result = SetEventByStaticMethod(pObject, (EFS_EventMatrixField)pEventMatrixParameter, null);
                        #endregion Constant
                        break;
                    default:
                        break;
                }
            }
            else
            {
                object parameter = ReflectionTools.GetObjectById(tradeLibrary.EventMatrix, pEventMatrixParameter.hRef);
                if (null != parameter)
                    result = ((EFS_EventMatrixParameter)parameter).result;
            }
            return result;
        }
        #endregion SetParameter
        #region SetParameters
        private void SetParameters(object pObject, object pDeclaringParameters)
        {

            Type tDeclaringParameters = pDeclaringParameters.GetType();
            FieldInfo fld = tDeclaringParameters.GetField("parameters");
            if (null != fld)
            {
                EFS_EventMatrixParameter[] eventMatrixParameters = (EFS_EventMatrixParameter[])fld.GetValue(pDeclaringParameters);
                if ((null != eventMatrixParameters) && (0 != eventMatrixParameters.Length))
                {
                    foreach (EFS_EventMatrixParameter eventMatrixParameter in eventMatrixParameters)
                    {
                        eventMatrixParameter.result = SetParameter(pObject, eventMatrixParameter);
                    }
                }
            }

        }
        #endregion SetParameters

        #region TrtEventMatrixGroup
        private void TrtEventMatrixGroup(object pObject, EFS_EventMatrixGroup pEventMatrixGroup,
            EFS_EventMatrixGroupKey pKeyGroupParent, int pIdGroup)
        {

            #region EventMatrixGroup parameters
            SetParameters(pObject, pEventMatrixGroup);
            #endregion EventMatrixGroup parameters

            #region EventMatrixGroup item
            EFS_EventMatrixGroupKey keyGroup = pEventMatrixGroup.keyGroup;
            keyGroup.result = keyGroup.key + pIdGroup.ToString().PadLeft(3, '0');
            CreateEvent(pObject, pEventMatrixGroup.itemGroup, pKeyGroupParent, keyGroup);
            #endregion EventMatrixGroup item

            #region Recursivity
            ReadEventMatrixSubGroup(pObject, pEventMatrixGroup, keyGroup);
            #endregion Recursivity

            #region EventMatrixGroup items
            ReadEventMatrixItems(pObject, pEventMatrixGroup);
            #endregion EventMatrixGroup items

        }
        #endregion TrtEventMatrixGroup
        #endregion Methods
    }
    #endregion EFS_Events

    #region DataSetEventTrade
    // EG 20150612 [20665] Refactoring : Chargement DataSetEventTrade
    // EG 20231127 [WI755] Implementation Return Swap : Refactoring Code Analysis
    public class DataSetEventTrade
    {
        #region Members
        // RD 20120809 [18070] Optimisation de la compta / rendre ces membres "protected" pour pouvoir les utiliser dans la classe qui hérite
        protected string m_cs;
        protected IDbTransaction m_DbTransaction;
        protected int m_IdT;
        protected DataSet m_dsEvents;

        
        private readonly int[] m_IdE;
        #endregion Members
        #region Accessors
        
        #region CS
        public string CS
        {
            get { return m_cs; }
        }
        #endregion CS
        #region IdT
        public int IdT
        {
            get { return m_IdT; }
        }
        #endregion
        #region DsEvent
        public DataSet DsEvent
        {
            get { return m_dsEvents; }
        }
        #endregion DsEvent
        #region DtEvent
        public DataTable DtEvent
        {
            get
            {
                DataTable ret = null;
                if (m_dsEvents.Tables.Contains("Event"))
                    ret = m_dsEvents.Tables["Event"];
                return ret;
            }
        }
        #endregion DtEvent
        #region DtEventAsset
        public DataTable DtEventAsset
        {
            get
            {
                DataTable ret = null;
                if (m_dsEvents.Tables.Contains("EventAsset"))
                    ret = m_dsEvents.Tables["EventAsset"];
                return ret;
            }
        }
        #endregion EventAsset
        #region DtEventClass
        public DataTable DtEventClass
        {
            get
            {
                DataTable ret = null;
                if (m_dsEvents.Tables.Contains("EventClass"))
                    ret = m_dsEvents.Tables["EventClass"];
                return ret;
            }
        }
        #endregion DtEventClass
        #region DtEventProcess
        public DataTable DtEventProcess
        {
            get
            {
                DataTable ret = null;
                if (m_dsEvents.Tables.Contains("EventProcess"))
                    ret = m_dsEvents.Tables["EventProcess"];
                return ret;
            }
        }
        #endregion DtEventProcess
        #region DtEventStCheck
        public DataTable DtEventStCheck
        {
            get
            {
                DataTable ret = null;
                if (m_dsEvents.Tables.Contains("EventStCheck"))
                    ret = m_dsEvents.Tables["EventStCheck"];
                return ret;
            }
        }
        #endregion DtEventStCheck
        #region DtEventStMatch
        public DataTable DtEventStMatch
        {
            get
            {
                DataTable ret = null;
                if (m_dsEvents.Tables.Contains("EventStMatch"))
                    ret = m_dsEvents.Tables["EventStMatch"];
                return ret;
            }
        }
        #endregion DtEventStMatch
        #region DtEventFee
        public DataTable DtEventFee
        {
            get
            {
                DataTable ret = null;
                if (m_dsEvents.Tables.Contains("EventFee"))
                    ret = m_dsEvents.Tables["EventFee"];
                return ret;
            }
        }
        #endregion DtEventFee
        #region DtEventDet
        public DataTable DtEventDet
        {
            get
            {
                DataTable ret = null;
                if (m_dsEvents.Tables.Contains("EventDet"))
                    ret = m_dsEvents.Tables["EventDet"];
                return ret;
            }
        }
        #endregion DtEventDet
        #region DtEventSi
        public DataTable DtEventSi
        {
            get
            {
                DataTable ret = null;
                if (m_dsEvents.Tables.Contains("EventSi"))
                    ret = m_dsEvents.Tables["EventSi"];
                return ret;
            }
        }
        #endregion DtEventSi
        #region DtEventSi_Simul
        public DataTable DtEventSi_Simul
        {
            get
            {
                DataTable ret = null;
                if (m_dsEvents.Tables.Contains("EventSi_T"))
                    ret = m_dsEvents.Tables["EventSi_T"];
                return ret;
            }
        }
        #endregion DtEventSi_Simul


        #region ChildEvent
        public DataRelation ChildEvent
        {
            get
            {
                DataRelation ret = null;
                if (m_dsEvents.Relations.Contains("ChildEvent"))
                    ret = m_dsEvents.Relations["ChildEvent"];
                return ret;
            }
        }
        #endregion ChildEvent
        #region ChildEventByEventCode
        public DataRelation ChildEventByEventCode
        {
            get
            {
                DataRelation ret = null;
                if (m_dsEvents.Relations.Contains("ChildEventByEventCode"))
                    ret = m_dsEvents.Relations["ChildEventByEventCode"];
                return ret;
            }
        }
        #endregion ChildEventByEventCode
        #region ChildEventAsset
        public DataRelation ChildEventAsset
        {
            get
            {
                DataRelation ret = null;
                if (m_dsEvents.Relations.Contains("Event_EventAsset"))
                    ret = m_dsEvents.Relations["Event_EventAsset"];
                return ret;
            }
        }
        #endregion ChildEventAsset
        #region ChildEventClass
        public DataRelation ChildEventClass
        {
            get
            {
                DataRelation ret = null;
                if (m_dsEvents.Relations.Contains("Event_EventClass"))
                    ret = m_dsEvents.Relations["Event_EventClass"];
                return ret;
            }
        }
        #endregion ChildEventClass
        #region ChildEventDet
        public DataRelation ChildEventDet
        {
            get
            {
                DataRelation ret = null;
                if (m_dsEvents.Relations.Contains("Event_EventDet"))
                    ret = m_dsEvents.Relations["Event_EventDet"];
                return ret;
            }
        }
        #endregion ChildEventDet
        #region ChildEventFee
        public DataRelation ChildEventFee
        {
            get
            {
                DataRelation ret = null;
                if (m_dsEvents.Relations.Contains("Event_EventFee"))
                    ret = m_dsEvents.Relations["Event_EventFee"];
                return ret;
            }
        }
        #endregion ChildEventFee
        #region ChildEventSi
        public DataRelation ChildEventSi
        {
            get
            {
                DataRelation ret = null;
                if (m_dsEvents.Relations.Contains("Event_EventSi"))
                    ret = m_dsEvents.Relations["Event_EventSi"];
                return ret;
            }
        }
        #endregion ChildEventSi
        #region ChildEventSi_Simul
        public DataRelation ChildEventSi_Simul
        {
            get
            {
                DataRelation ret = null;
                if (m_dsEvents.Relations.Contains("Event_EventSi_T"))
                    ret = m_dsEvents.Relations["Event_EventSi_T"];
                return ret;
            }
        }
        #endregion ChildEventSi_Simul

        #region ChildEventClassEnum
        public DataRelation ChildEventClassEnum
        {
            get { return m_dsEvents.Relations["EventClass_EventClassEnum"]; }
        }
        #endregion ChildEventClassEnum
        #region ChildEventCodeEnum
        public DataRelation ChildEventCodeEnum
        {
            get { return m_dsEvents.Relations["Event_EventCodeEnum"]; }
        }
        #endregion ChildEventCodeEnum
        #region ChildEventProcess
        public DataRelation ChildEventProcess
        {
            get
            {
                DataRelation ret = null;
                if (m_dsEvents.Relations.Contains("Event_EventProcess"))
                    ret = m_dsEvents.Relations["Event_EventProcess"];
                return ret;
            }
        }
        #endregion ChildEventProcess
        #region ChildEventStCheck
        public DataRelation ChildEventStCheck
        {
            get
            {
                DataRelation ret = null;
                if (m_dsEvents.Relations.Contains("Event_EventStCheck"))
                    ret = m_dsEvents.Relations["Event_EventStCheck"];
                return ret;

            }
        }
        #endregion ChildEventStCheck
        #region ChildEventStMatch
        public DataRelation ChildEventStMatch
        {
            get
            {
                DataRelation ret = null;
                if (m_dsEvents.Relations.Contains("Event_EventStMatch"))
                    ret = m_dsEvents.Relations["Event_EventStMatch"];
                return ret;
            }
        }
        #endregion ChildEventStMatch
        #endregion Accessors
        #region Constructors
        // EG 20150612 [20665] Refactoring : Chargement DataSetEventTrade
        public DataSetEventTrade()
        {
        }
        // EG 20150612 [20665] Refactoring : Chargement DataSetEventTrade
        /*
        public DataSetEventTrade(string pConnectionString, IProduct pProduct, int pIdT)
        {
            m_cs = pConnectionString;
            m_IdT = pIdT;
            product = pProduct;
            Load();
        }
        public DataSetEventTrade(string pConnectionString, int pIdT)
            : this(pConnectionString, pIdT, null)
        {
        }
        public DataSetEventTrade(string pConnectionString, IDbTransaction pDbTransaction, int[] pIdE)
            : this(pConnectionString, pDbTransaction, 0, pIdE)
        {
        }
        public DataSetEventTrade(string pConnectionString, int pIdT, int[] pIdE)
            : this(pConnectionString, null, pIdT, pIdE)
        {
        }
        public DataSetEventTrade(string pConnectionString, IDbTransaction pDbTransaction, int pIdT, int[] pIdE)
        {
            m_cs = pConnectionString;
            m_DbTransaction = pDbTransaction;
            m_IdT = pIdT;
            m_IdE = pIdE;
            Load();
        }
        public DataSetEventTrade(string pConnectionString, IDbTransaction pDbTransaction, IProduct pProduct, int pIdT)
        {
            m_cs = pConnectionString;
            m_DbTransaction = pDbTransaction;
            m_IdT = pIdT;
            product = pProduct;
            Load();
        }
        */
        // EG 20150612 [20665] Refactoring : Chargement DataSetEventTrade
        public DataSetEventTrade(string pConnectionString, int pIdT)
        {
            m_cs = pConnectionString;
            m_IdT = pIdT;
            Load();
        }
        // EG 20150612 [20665] Refactoring : Chargement DataSetEventTrade
        public DataSetEventTrade(string pConnectionString, IDbTransaction pDbTransaction, int[] pIdE)
        {
            m_cs = pConnectionString;
            m_DbTransaction = pDbTransaction;
            m_IdE = pIdE;
            Load();
        }
        // EG 20150612 [20665] Refactoring : Chargement DataSetEventTrade
        public DataSetEventTrade(string pConnectionString, int[] pIdE)
        {
            m_cs = pConnectionString;
            m_IdE = pIdE;
            Load();
        }
        // EG 20150612 [20665] Refactoring : Chargement DataSetEventTrade
        // EG 20200914 [XXXXX] Correction UnComment Load()
        public DataSetEventTrade(string pConnectionString, IDbTransaction pDbTransaction,  int pIdT)
        {
            m_cs = pConnectionString;
            m_DbTransaction = pDbTransaction;
            m_IdT = pIdT;
            
            Load();
        }
        #endregion Constructors
        #region Methods
        #region Load
        // RD 20120809 [18070] Optimisation de la compta / rendre cette méthode "virtual" pour pouvoir la rédéfinir
        // EG 20150612 [20665] Refactoring : Chargement DataSetEventTrade
        // EG 20150617 [20665] 
        public virtual void Load()
        {
            Load(0, null, null);
        }
        // EG 20150612 [20665] Refactoring : Chargement DataSetEventTrade
        // EG 20150612 [20665] New Method
        // EG 20150617 [20665] Add pDtBusinessPrev
        public void Load(EventTableEnum pEventTable, Nullable<DateTime> pDtBusiness, Nullable<DateTime> pDtBusinessPrev)
        {
            bool isAlltable = (pEventTable == 0); // Toutes les table sont chargées
            DataParameters parameters = new DataParameters();

            #region Construction du Where
            SQLWhere sqlWhere = new SQLWhere();

            // Restriction IDT
            if (m_IdT > 0)
            {
                parameters.Add(new DataParameter(CS, "IDT", DbType.Int32), m_IdT);
                sqlWhere.Append(@"(ev.IDT = @IDT)");
            }

            // Restriction IDEs
            if (ArrFunc.IsFilled(m_IdE))
            {
                if (ArrFunc.Count(m_IdE) == 1)
                {
                    parameters.Add(new DataParameter(CS, "IDE", System.Data.DbType.Int32), m_IdE[0]);
                    sqlWhere.Append(@"(ev.IDE = @IDE)");
                }
                else
                    sqlWhere.Append(@"(" + DataHelper.SQLColumnIn(CS, "ev.IDE", m_IdE, TypeData.TypeDataEnum.integer, false, true) + ")");
            }
            #endregion Construction du Where


            if (0 < sqlWhere.Length())
            {
                // EG 20150612 [20665] Restriction sur DTBUSINESS
                // EG 20150617 [20665] Restriction sur DTBUSINESS|DTBUSINESSPREV
                // EG 20170324 [22991] Change Columns between xxUNADJ replace xxADJ
                if (pDtBusiness.HasValue)
                {
                    string sqlAdditionalWhere = @"(@DTBUSINESS between DTSTARTUNADJ and DTENDUNADJ)";
                    parameters.Add(DataParameter.GetParameter(CS, DataParameter.ParameterEnum.DTBUSINESS), pDtBusiness.Value); // FI 20201006 [XXXXX] DbType.Date
                    if (pDtBusinessPrev.HasValue && (pDtBusinessPrev.Value.Date != pDtBusiness.Value.Date))
                    {
                        parameters.Add(new DataParameter(CS, "DTBUSINESSPREV", DbType.Date), pDtBusinessPrev.Value); // FI 20201006 [XXXXX] DbType.Date
                        sqlAdditionalWhere += @" or (@DTBUSINESSPREV between DTSTARTUNADJ and DTENDUNADJ)";
                    }
                    sqlWhere.Append(@"(" + sqlAdditionalWhere + ")");
                }

                StrBuilder sqlSelect = new StrBuilder();
                sqlSelect += QueryLibraryTools.GetQuerySelect(CS, Cst.OTCml_TBL.EVENT) + sqlWhere.ToString() + Cst.CrLf + SQLCst.ORDERBY + "ev.IDE" + SQLCst.SEPARATOR_MULTISELECT;

                if (isAlltable || ((EventTableEnum.Class & pEventTable) > 0))
                    sqlSelect += QueryLibraryTools.GetQuerySelect(CS, Cst.OTCml_TBL.EVENTCLASS) + sqlWhere.ToString() + SQLCst.SEPARATOR_MULTISELECT;

                if (isAlltable || ((EventTableEnum.Asset & pEventTable) > 0))
                    sqlSelect += QueryLibraryTools.GetQuerySelect(CS, Cst.OTCml_TBL.EVENTASSET) + sqlWhere.ToString() + SQLCst.SEPARATOR_MULTISELECT;

                if (isAlltable || ((EventTableEnum.StCheck & pEventTable) > 0))
                    sqlSelect += QueryLibraryTools.GetQuerySelect(CS, Cst.OTCml_TBL.EVENTSTCHECK) + sqlWhere.ToString() + SQLCst.SEPARATOR_MULTISELECT;

                if (isAlltable || ((EventTableEnum.StMatch & pEventTable) > 0))
                    sqlSelect += QueryLibraryTools.GetQuerySelect(CS, Cst.OTCml_TBL.EVENTSTMATCH) + sqlWhere.ToString() + SQLCst.SEPARATOR_MULTISELECT;

                if (isAlltable || ((EventTableEnum.Process & pEventTable) > 0))
                    sqlSelect += QueryLibraryTools.GetQuerySelect(CS, Cst.OTCml_TBL.EVENTPROCESS) + sqlWhere.ToString() + SQLCst.SEPARATOR_MULTISELECT;

                if (isAlltable || ((EventTableEnum.SettlSi & pEventTable) > 0))
                    sqlSelect += QueryLibraryTools.GetQuerySelect(CS, Cst.OTCml_TBL.EVENTSI) + sqlWhere.ToString() + SQLCst.SEPARATOR_MULTISELECT;

                if (isAlltable || ((EventTableEnum.Fee & pEventTable) > 0))
                    sqlSelect += QueryLibraryTools.GetQuerySelect(CS, Cst.OTCml_TBL.EVENTFEE) + sqlWhere.ToString() + SQLCst.SEPARATOR_MULTISELECT;

                if (isAlltable || ((EventTableEnum.Detail & pEventTable) > 0))
                    sqlSelect += QueryLibraryTools.GetQuerySelect(CS, Cst.OTCml_TBL.EVENTDET) + sqlWhere.ToString() + SQLCst.SEPARATOR_MULTISELECT;

                QueryParameters qryParameters = new QueryParameters(CS, sqlSelect.ToString(), parameters);
                if (null != m_DbTransaction)
                {
                    m_dsEvents = DataHelper.ExecuteDataset(m_DbTransaction, CommandType.Text, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter());
                }
                else
                {
                    // PL 20180312 WARNING: Use Read Commited !
                    //m_dsEvents = OTCmlHelper.GetDataSetWithIsolationLevel(m_cs, IsolationLevel.ReadUncommitted, qryParameters, null);
                    m_dsEvents = OTCmlHelper.GetDataSetWithIsolationLevel(m_cs, IsolationLevel.ReadCommitted, qryParameters, null);
                }

                InitializeDataSet("Events", pEventTable);
            }
            else
                throw new SpheresException2(MethodInfo.GetCurrentMethod().Name, "No SqlWhere For Query");

        }
        #endregion Load
        #region InitializeDataSet
        /// <summary>
        /// Initialisation du DataSet en fonction des tables sélectionnées
        /// </summary>
        /// <param name="pDataSetName"></param>
        /// <param name="pEventTable"></param>
        // EG 20150612 [20665] Refactoring : Chargement DataSetEventTrade
        public void InitializeDataSet(string pDataSetName, EventTableEnum pEventTable)
        {
            bool isAlltable = (pEventTable == 0);

            m_dsEvents.DataSetName = pDataSetName;
            m_dsEvents.Tables[0].TableName = "Event";

            m_dsEvents.Relations.Clear();

            if (null != DtEvent)
            {
                DataRelation relChildEvent = new DataRelation("ChildEvent", DtEvent.Columns["IDE"], DtEvent.Columns["IDE_EVENT"], false);
                m_dsEvents.Relations.Add(relChildEvent);
            }
            if (null != DtEvent)
            {
                DataRelation relChildEventByEventCode = new DataRelation("ChildEventByEventCode",
                    new DataColumn[2] { DtEvent.Columns["IDE"], DtEvent.Columns["EVENTCODE"] },
                    new DataColumn[2] { DtEvent.Columns["IDE_EVENT"], DtEvent.Columns["EVENTCODE"] }, false);
                m_dsEvents.Relations.Add(relChildEventByEventCode);
            }

            int _index = 1;
            if (isAlltable || ((EventTableEnum.Class & pEventTable) > 0))
            {
                m_dsEvents.Tables[_index].TableName = "EventClass";
                DataRelation relEventClass = new DataRelation("Event_EventClass", DtEvent.Columns["IDE"], DtEventClass.Columns["IDE"], false)
                {
                    Nested = true
                };
                m_dsEvents.Relations.Add(relEventClass);
                _index++;
            }

            if (isAlltable || ((EventTableEnum.Asset & pEventTable) > 0))
            {
                m_dsEvents.Tables[_index].TableName = "EventAsset";
                DataRelation relEventAsset = new DataRelation("Event_EventAsset", DtEvent.Columns["IDE"], DtEventAsset.Columns["IDE"], false)
                {
                    Nested = true
                };
                m_dsEvents.Relations.Add(relEventAsset);
                _index++;
            }

            if (isAlltable || ((EventTableEnum.StCheck & pEventTable) > 0))
            {
                m_dsEvents.Tables[_index].TableName = "EventStCheck";
                DataRelation relEventStCheck = new DataRelation("Event_EventStCheck", DtEvent.Columns["IDE"], DtEventStCheck.Columns["IDE"], false)
                {
                    Nested = true
                };
                m_dsEvents.Relations.Add(relEventStCheck);
                _index++;
            }

            if (isAlltable || ((EventTableEnum.StMatch & pEventTable) > 0))
            {
                m_dsEvents.Tables[_index].TableName = "EventStMatch";
                DataRelation relEventStMatch = new DataRelation("Event_EventStMatch", DtEvent.Columns["IDE"], DtEventStMatch.Columns["IDE"], false)
                {
                    Nested = true
                };
                m_dsEvents.Relations.Add(relEventStMatch);
                _index++;
            }

            if (isAlltable || ((EventTableEnum.Process & pEventTable) > 0))
            {
                m_dsEvents.Tables[_index].TableName = "EventProcess";
                DataRelation relEventProcess = new DataRelation("Event_EventProcess", DtEvent.Columns["IDE"], DtEventProcess.Columns["IDE"], false)
                {
                    Nested = true
                };
                m_dsEvents.Relations.Add(relEventProcess);
                _index++;
            }

            if (isAlltable || ((EventTableEnum.SettlSi & pEventTable) > 0))
            {
                m_dsEvents.Tables[_index].TableName = "EventSi";
                DataRelation relEventSi = new DataRelation("Event_EventSi", DtEvent.Columns["IDE"], DtEventSi.Columns["IDE"], false)
                {
                    Nested = true
                };
                m_dsEvents.Relations.Add(relEventSi);
                //RD 20150323 [20805] PrimaryKey manquant pour la DataTable DtEventSi
                DtEventSi.PrimaryKey = new DataColumn[] { DtEventSi.Columns["IDE"], DtEventSi.Columns["PAYER_RECEIVER"] };
                _index++;
            }

            if (isAlltable || ((EventTableEnum.Fee & pEventTable) > 0))
            {
                m_dsEvents.Tables[_index].TableName = "EventFee";
                DataRelation relEventFee = new DataRelation("Event_EventFee", DtEvent.Columns["IDE"], DtEventFee.Columns["IDE"], false)
                {
                    Nested = true
                };
                m_dsEvents.Relations.Add(relEventFee);
                _index++;
            }

            if (isAlltable || ((EventTableEnum.Detail & pEventTable) > 0))
            {
                m_dsEvents.Tables[_index].TableName = "EventDet";
                DataRelation relEventDet = new DataRelation("Event_EventDet", DtEvent.Columns["IDE"], DtEventDet.Columns["IDE"], false)
                {
                    Nested = true
                };
                m_dsEvents.Relations.Add(relEventDet);
            }
        }
        #endregion InitializeDataSet

        #region GetRowsCreditNoteDates
        public DataRow[] GetRowsCreditNoteDates()
        {
            return DtEvent.Select(StrFunc.AppendFormat(@"EVENTCODE = '{0}' and EVENTTYPE = '{1}'", EventCodeFunc.CreditNoteDates, EventTypeFunc.Period), "IDE");
        }
        #endregion GetRowsCreditNoteDates
        #region GetRowsEvent
        /// <summary>
        /// Retourne tous les évènements présents dans DtEvent
        /// </summary>
        /// <returns></returns>
        public DataRow[] GetRowsEvent()
        {
            return DtEvent.Select("IDT = " + m_IdT.ToString(), "IDE");
        }
        #endregion GetRowsEvent
        #region GetRowsEventCancellation
        /// <summary>
        /// Obtient l'évènement EVENTCODE='RMV', EVENTTYPE='DAT'
        /// </summary>
        /// <returns></returns>
        public DataRow[] GetRowsEventCancellation()
        {
            return DtEvent.Select(StrFunc.AppendFormat(@"EVENTCODE = '{0}' and EVENTTYPE = '{1}'", EventCodeFunc.RemoveTrade, EventTypeFunc.Date), "IDE");
        }
        #endregion GetRowsEventCancellation

        #region RowEvent
        public DataRow RowEvent(int pIdE)
        {
            DataRow rowEvent = null;
            if (null != DtEvent)
            {
                DataRow[] rows = DtEvent.Select("IDE = " + pIdE.ToString());
                if (null != rows && rows.Length == 1)
                    rowEvent = rows[0];
            }
            return rowEvent;
        }
        #endregion
        #region RowEventDet
        public DataRow RowEventDet(int pIdE)
        {
            DataRow rowEventDet = null;
            if (null != DtEventDet)
            {
                DataRow[] rows = DtEventDet.Select("IDE = " + pIdE.ToString());
                if (null != rows && rows.Length == 1)
                    rowEventDet = rows[0];
            }
            return rowEventDet;
        }
        #endregion

        #region SetEventStatus
        // EG 20231127 [WI755] Implementation Return Swap : Refactoring Code Analysis
        public void SetEventStatus(int pIdE, ProcessTuningOutput pTuning, int pIdA, DateTime pDtSysBusiness)
        {
            DataRow eventRow = RowEvent(pIdE);
            bool isOk = null != eventRow;
            if (isOk)
            {
                if (pTuning.IdStActivationSpecified)
                {
                    eventRow["IDSTACTIVATION"] = pTuning.IdStActivation.ToString();
                    eventRow["DTSTACTIVATION"] = pDtSysBusiness;
                    eventRow["IDASTACTIVATION"] = pIdA;
                }
                //
                if (pTuning.IdStSpecified(StatusEnum.StatusCheck) && (null != DtEventStCheck))
                {
                    string status = pTuning.IdSt(StatusEnum.StatusCheck);
                    DataRow[] eventStCheckRows = DtEventStCheck.Select(StrFunc.AppendFormat(@"IDE = {0} and IDSTCHECK = '{1}'", pIdE, status));
                    if (null != eventStCheckRows && eventStCheckRows.Length > 0)
                    {
                        DataRow row = eventStCheckRows[0];
                        row.BeginEdit();
                        // FI 20200820 [25468] Dates systemes en UTC
                        row["DTINS"] = OTCmlHelper.GetDateSysUTC(CS);
                        row["IDAINS"] = pIdA;
                        row.EndEdit();
                    }
                    else
                    {
                        DataRow newRow = DtEventStCheck.NewRow();
                        newRow.BeginEdit();
                        newRow["IDE"] = pIdE;
                        newRow["IDSTCHECK"] = status;
                        // FI 20200820 [25468] Dates systemes en UTC
                        newRow["DTINS"] = OTCmlHelper.GetDateSysUTC(CS);
                        newRow["IDAINS"] = pIdA;
                        newRow.EndEdit();
                        DtEventStCheck.Rows.Add(newRow);
                    }
                }

                if (pTuning.IdStSpecified(StatusEnum.StatusMatch) && (null != DtEventStMatch))
                {
                    string status = pTuning.IdSt(StatusEnum.StatusMatch);
                    DataRow[] eventStMatchRows = DtEventStMatch.Select(StrFunc.AppendFormat(@"IDE = {0} and IDSTMATCH = '{1}'", pIdE, status));
                    if (null != eventStMatchRows && eventStMatchRows.Length > 0)
                    {
                        DataRow row = eventStMatchRows[0];
                        row.BeginEdit();
                        // FI 20200820 [25468] Dates systemes en UTC
                        row["DTINS"] = OTCmlHelper.GetDateSysUTC(CS);
                        row["IDAINS"] = pIdA;
                        row.EndEdit();
                    }
                    else
                    {
                        DataRow newRow = DtEventStMatch.NewRow();
                        newRow.BeginEdit();
                        newRow["IDE"] = pIdE;
                        newRow["IDSTMATCH"] = status;
                        // FI 20200820 [25468] Dates systemes en UTC
                        newRow["DTINS"] = OTCmlHelper.GetDateSysUTC(CS);
                        newRow["IDAINS"] = pIdA;
                        newRow.EndEdit();
                        DtEventStMatch.Rows.Add(newRow);
                    }
                }
            }
        }
        #endregion

        #region Update
        public void Update(IDbTransaction pDbTransaction)
        {
            Update(pDbTransaction, false);
        }
        public void Update(IDbTransaction pDbTransaction, bool pIsEndOfDayProcess)
        {
            if (false == pIsEndOfDayProcess)
            {
                Update(pDbTransaction, Cst.OTCml_TBL.EVENTSI);
                Update(pDbTransaction, Cst.OTCml_TBL.EVENTSI_T);
            }
            Update(pDbTransaction, Cst.OTCml_TBL.EVENT);

            if (false == pIsEndOfDayProcess)
            {
                Update(pDbTransaction, Cst.OTCml_TBL.EVENTASSET);
                Update(pDbTransaction, Cst.OTCml_TBL.EVENTFEE);
            }
            Update(pDbTransaction, Cst.OTCml_TBL.EVENTCLASS);
            Update(pDbTransaction, Cst.OTCml_TBL.EVENTSTCHECK);
            Update(pDbTransaction, Cst.OTCml_TBL.EVENTSTMATCH);
        }
        public void Update(IDbTransaction pDbTransaction, Cst.OTCml_TBL pTable)
        {
            DataTable _dataTable;
            switch (pTable)
            {
                case Cst.OTCml_TBL.EVENT:
                    _dataTable = DtEvent;
                    break;

                case Cst.OTCml_TBL.EVENTASSET:
                    _dataTable = DtEventAsset;
                    break;

                case Cst.OTCml_TBL.EVENTCLASS:
                    _dataTable = DtEventClass;
                    break;

                case Cst.OTCml_TBL.EVENTFEE:
                    _dataTable = DtEventFee;
                    break;

                case Cst.OTCml_TBL.EVENTSI:
                    _dataTable = DtEventSi;
                    break;

                case Cst.OTCml_TBL.EVENTSI_T:
                    _dataTable = DtEventSi_Simul;
                    break;

                case Cst.OTCml_TBL.EVENTDET:
                    _dataTable = DtEventDet;
                    break;

                case Cst.OTCml_TBL.EVENTPROCESS:
                    _dataTable = DtEventProcess;
                    break;

                case Cst.OTCml_TBL.EVENTSTCHECK:
                    _dataTable = DtEventStCheck;
                    break;

                case Cst.OTCml_TBL.EVENTSTMATCH:
                    _dataTable = DtEventStMatch;
                    break;

                default:
                    throw new Exception("GetQuerySelect for " + pTable.ToString() + " is not managed, please contact EFS");
            }
            Update(pDbTransaction, _dataTable, pTable);
        }
        // EG 20180205 [23769] Upd DataHelper.ExecuteDataAdapter
        public void Update(IDbTransaction pDbTransaction, DataTable pDataTable, Cst.OTCml_TBL pTable)
        {
            if (null != pDataTable)
            {
                string sqlSelect = QueryLibraryTools.GetQuerySelect(CS, pTable, true);
                DataHelper.ExecuteDataAdapter(CS, pDbTransaction, sqlSelect, pDataTable);
            }
        }
        #endregion Update

        #region IsTradeRemoved
        public bool IsTradeRemoved(DateTime pDtProcess)
        {
            DateTime dtEventRemove = DateTime.MinValue;
            return IsTradeRemoved(pDtProcess, ref dtEventRemove);
        }
        public bool IsTradeRemoved(DateTime pDtProcess, ref DateTime pDtEventRemove)
        {
            pDtEventRemove = DateTime.MinValue;
            DataRow[] rowsCancellation = GetRowsEventCancellation();
            if (null != rowsCancellation)
            {
                foreach (DataRow rowCancellation in rowsCancellation)
                {
                    int compareValue = 1;
                    DataRow[] rowsEventClass = rowCancellation.GetChildRows(ChildEventClass);
                    foreach (DataRow rowEventClass in rowsEventClass)
                    {
                        DateTime dtEvent = Convert.ToDateTime(rowEventClass["DTEVENT"]);
                        compareValue = dtEvent.CompareTo(pDtProcess);
                        //
                        if (compareValue <= 0)
                        {
                            pDtEventRemove = dtEvent;
                            return true;
                        }
                    }
                }
            }
            return false;
        }
        #endregion IsTradeRemoved
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pIE"></param>
        public void DeactivEvent(int[] pIdEvent, int pIdA, DateTime pDtBusiness)
        {
            foreach (int idE in pIdEvent)
            {
                // EVENT (DEACTIV)
                DataRow rowEvent = RowEvent(idE);
                if (null == rowEvent)
                    throw new NullReferenceException("");

                rowEvent.BeginEdit();
                rowEvent["IDASTACTIVATION"] = pIdA;
                // FI 20200820 [25468] Dates systemes en UTC
                //rowEvent["DTSTACTIVATION"] = pDtBusiness;
                rowEvent["DTSTACTIVATION"] = OTCmlHelper.GetDateSysUTC(m_cs);
                rowEvent["IDSTACTIVATION"] = Cst.StatusActivation.DEACTIV;
                rowEvent.EndEdit();

                // EVENTCLASS (RMV)
                if (null == rowEvent)
                    throw new NullReferenceException("DeactivEvent : rowEvent is null on ");
                DataRow rowClass = DtEventClass.NewRow();
                rowClass.BeginEdit();
                rowClass["IDE"] = idE;
                rowClass["EVENTCLASS"] = EventClassFunc.RemoveEvent;
                rowClass["DTEVENT"] = pDtBusiness;
                rowClass["DTEVENTFORCED"] = OTCmlHelper.GetAnticipatedDate(CS, pDtBusiness);
                rowClass["ISPAYMENT"] = false;
                rowClass.EndEdit();

            }

        }

        #endregion Methods
    }
    #endregion DataSetEventTrade

}
