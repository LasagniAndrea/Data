#region Using Directives
using EFS.ACommon;
using EFS.Actor;
using EFS.ApplicationBlocks.Data;
using EFS.Common;
using EFS.Common.MQueue;
using EFS.Common.Web;
using EFS.GUI;
using EFS.GUI.Attributes;
using EFS.GUI.Interface;
using EfsML.Business;
using EfsML.Enum.Tools;
using EfsML.Interface;
using FpML.Enum;
using FpML.Interface;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Threading;
using System.Web.UI;
using System.Web.UI.WebControls;
#endregion Using Directives

namespace EfsML
{
    #region TradeAction
    /// <summary>Used by the trade action process </summary>
    public abstract class TradeAction : TradeActionBase
    {
        #region Members
        public TradeActionEvent events;
        #endregion Members
        #region Virtual accessors
        public virtual IFxStrikePrice StrikePrice(string pInstrumentNo)
        {
            return null;
        }
        #endregion Virtual accessors

        #region override accessors
        // EG 20150428 [20513] New
        public virtual string SettlementCurrency { get { return string.Empty; } }
        // EG 20150428 [20513] New
        public virtual SettlementTypeEnum SettlementType { get { return SettlementTypeEnum.Physical; } }
        #endregion override accessors

        #region Constructors
        public TradeAction():base(){}
        public TradeAction(int pCurrentIdE, TradeActionType.TradeActionTypeEnum pTradeActionType, TradeActionMode.TradeActionModeEnum pTradeActionMode, 
            DataDocumentContainer pDataDocumentContainer,TradeActionEvent pEvents)
            :base(pCurrentIdE, pTradeActionType, pTradeActionMode, pDataDocumentContainer)
        {
            events = pEvents;
        }
        #endregion Constructors

        #region Virtual Methods
        // EG 20150428 [20513] New
        public virtual DateTime ExpiryDate(string pInstrumentNo)
        {
            return Convert.ToDateTime(null);
        }
        public virtual DateTime ExpiryDate()
        {
            return Convert.ToDateTime(null);
        }
        // EG 20150428 [20513] New
        public virtual IExpiryDateTime ExpiryDateTime()
        {
            return null;
        }
        public virtual IExpiryDateTime ExpiryDateTime(string pInstrumentNo)
        {
            return null;
        }
        // EG 20150428 [20513] New
        public virtual IFxCashSettlement FxCashSettlement(){return null;}
        // EG 20150428 [20513] New
        public virtual DateTime ValueDate(){return Convert.ToDateTime(null);}
        // EG 20150428 [20513] New
        public virtual decimal SpotRate() { return 0; }
        // EG 20150428 [20513] New
        public virtual IFxStrikePrice StrikePrice(){return null;}
        // EG 20150428 [20513] New
        public virtual IOptionStrike OptionStrike() { return null; }
        // EG 20150428 [20513] New
        public virtual IEquityStrike EquityStrike() { return null; }
        public virtual PayoutEnum CappedCalledOrFlooredPutPayoutStyle(string pInstrumentNo)
        {
            return PayoutEnum.Deferred;
        }
        public virtual string PayoutCurrency(string pInstrumentNo)
        {
            return null;
        }
        public virtual PayoutEnum PayoutStyle(string pInstrumentNo)
        {
            return PayoutEnum.Deferred;
        }
        public virtual decimal SpotRate(string pInstrumentNo)
        {
            return 0;
        }
        #endregion Virtual Methods
    }
    #endregion TradeAction

    #region TradeActionEvent
    // EG 20200914 [XXXXX] Couleurs sur OpenDivGUI
    public class TradeActionEvent : TradeActionEventBase
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("Event")]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "", IsVisible = true,Color=MethodsGUI.ColorEnum.None)]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "")]
        public TradeActionEvent[] events;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public TradeActionBase currentTradeAction;
        #endregion Members
        #region Accessors
        #region ActionTitle
        public override string ActionTitle
        {
            get
            {
                string param = string.Empty;
                if (EventCodeFunc.IsBarrier(eventCode) || EventCodeFunc.IsTrigger(eventCode) ||
                    EventCodeFunc.IsAbandon(eventCode) || EventCodeFunc.IsExercise(eventCode) ||
                    EventCodeFunc.IsOut(eventCode) || EventCodeFunc.IsExerciseProvision(eventCode))
                    param = CurrentTradeAction.GetEventCodeTitle(eventCode);
                else if (EventTypeFunc.IsPayout(eventType) || EventTypeFunc.IsRebate(eventType))
                    param = CurrentTradeAction.GetEventTypeTitle(eventType);
                //
                return Ressource.GetString2("TradeActionCharacteristics", param);
            }
        }
        #endregion ActionTitle
        #region CurrentTradeAction
        public override TradeActionBase CurrentTradeAction
        {
            get { return currentTradeAction; }
        }
        #endregion CurrentTradeAction
        #region DisplayCancellationTrade
        /// <summary>Cancellation Trade</summary>
        private PlaceHolder DisplayCancellationTrade
        {
            get
            {
                PlaceHolder plhCancellationTrade = new PlaceHolder();
                bool isExistCancellationTradeGroup = (null != EventCancellationTradeGroup);
                TradeActionEvent[] eventCancellation = new TradeActionEvent[1] { new TradeActionEvent() };
                if (isExistCancellationTradeGroup)
                {
                    #region Event already generated (present into the EVENT table)
                    eventCancellation[0] = EventCancellationTradeGroup;
                    plhCancellationTrade.Controls.Add(DisplayEvents(eventCancellation, false));
                    #endregion Event already generated (present into the EVENT table)
                }
                else
                {
                    bool isExistCancellationTrade = (null != EventCancellationTrade);
                    if (isExistCancellationTrade)
                    {
                        eventCancellation[0] = EventCancellationTrade[0];
                        plhCancellationTrade.Controls.Add(DisplayEvents(eventCancellation, true));
                        eventCancellation[0].isModified = true;
                    }
                    else
                    {
                        #region Event doesn't generated
                        eventCancellation[0].instrumentNo = instrumentNo;
                        eventCancellation[0].streamNo = streamNo;
                        eventCancellation[0].eventCode = EventCodeFunc.RemoveTrade;
                        eventCancellation[0].eventType = EventTypeFunc.Date;
                        eventCancellation[0].dtStartPeriod = dtStartPeriod;
                        eventCancellation[0].dtEndPeriod = dtEndPeriod;
                        eventCancellation[0].idE = -idE;
                        eventCancellation[0].idEParent = idE;
                        this.Add(eventCancellation[0]);
                        plhCancellationTrade.Controls.Add(DisplayEvents(eventCancellation, true));
                        eventCancellation[0].isModified = true;
                        #endregion Event doesn't generated
                    }
                }
                return plhCancellationTrade;
            }
        }
        #endregion DisplayCancellationTrade
        #region DisplayNominalStep
        private PlaceHolder DisplayNominalStep
        {
            get
            {
                return DisplayEvents(EventNominalStep, false);
            }
        }
        #endregion DisplayNominalStep
        #region DisplayProvision
        private PlaceHolder DisplayProvision(TradeActionEvent pEvent, string pEventCode)
        {
            PlaceHolder plhProvision = new PlaceHolder();
            bool isConditionRespected = true;
            bool isExistExerciseTotalEffect = (null != EventExerciseProvisionTotalEffect(pEventCode));
            bool isExistExercisePartielEffect = (null != EventExerciseProvisionPartielEffect(pEventCode));
            bool isExistExerciseEffect = (null != EventExerciseProvisionEffect(pEventCode));
            bool isExistExerciseProvisionMultipleTotalEffect = (null != EventTerminationNominalProvisionEffect(pEvent, pEventCode));

            if (isExistExerciseTotalEffect || isExistExercisePartielEffect || isExistExerciseProvisionMultipleTotalEffect)
            {
                if (isExistExerciseEffect)
                    // Provision Total/Partial/multiple already present nothing to do
                    plhProvision.Controls.Add(DisplayEvents(GetEvent_EventCode(pEventCode), false));
            }
            else
            {
                bool isExistExerciseVirtual = (null != EventExerciseProvisionVirtual(pEventCode));
                TradeActionEvent[] eventExercise = new TradeActionEvent[1] { new TradeActionEvent() };

                if (isExistExerciseEffect)
                    // Provision multiple already present : Provision must be multiple
                    plhProvision.Controls.Add(DisplayEvents(EventExerciseProvisionEffect(pEventCode), false));

                if (isExistExerciseVirtual)
                {
                    // Event already present in this process
                    eventExercise[0] = EventExerciseProvisionVirtual(pEventCode);
                    if (isConditionRespected)
                    {
                        // Display Event already present during this process
                        plhProvision.Controls.Add(DisplayEvents(eventExercise, true));
                        eventExercise[0].isModified = true;
                    }
                    else
                    {
                        eventExercise[0].m_Action = null;
                        eventExercise[0].m_OriginalAction = null;
                    }
                }
                else
                {
                    // Event not exist 
                    if (isConditionRespected)
                    {
                        // Conditions of exercise are observed, creation of Fee / Notional Extendible during this process
                        eventExercise[0].instrumentNo = instrumentNo;
                        eventExercise[0].streamNo = streamNo;
                        eventExercise[0].eventCode = pEventCode;
                        eventExercise[0].eventType = (isExistExerciseEffect ? EventTypeFunc.Multiple : EventTypeFunc.Total);
                        eventExercise[0].dtStartPeriod = dtStartPeriod;
                        eventExercise[0].dtEndPeriod = dtEndPeriod;
                        eventExercise[0].idE = -idE;
                        eventExercise[0].idEParent = idE;
                        this.Add(eventExercise[0]);
                        // Display Event 
                        plhProvision.Controls.Add(DisplayEvents(eventExercise, true));
                        eventExercise[0].isModified = true;
                    }
                }
            }
            return plhProvision;
        }
        #endregion DisplayProvision


        #region EventAbandon
        private TradeActionEvent[] EventAbandon
        {
            get { return GetEvent_EventCode(EventCodeFunc.Abandon); }
        }
        #endregion EventAbandon
        #region EventAbandonEffect
        private TradeActionEvent EventAbandonEffect
        {
            get
            {
                TradeActionEvent[] eventAbandon = EventAbandon;
                if (null != eventAbandon)
                {
                    foreach (TradeActionEvent item in eventAbandon)
                    {
                        if (null != item.EventClassGroupLevel)
                            return item;
                    }
                }
                return null;
            }
        }
        #endregion EventAbandonEffect
        #region EventAbandonVirtual
        private TradeActionEvent EventAbandonVirtual
        {
            get
            {
                TradeActionEvent[] eventAbandon = EventAbandon;
                if (null != eventAbandon)
                {
                    foreach (TradeActionEvent item in eventAbandon)
                    {
                        if (null == item.EventClassGroupLevel)
                            return item;
                    }
                }
                return null;
            }
        }
        #endregion EventAbandonVirtual

        #region EventAllIntermediaryCallCurrencyAmount
        public TradeActionEvent[] EventAllIntermediaryCallCurrencyAmount
        {
            get
            {
                ArrayList aAllIntermediaryCallCurrencyAmount = new ArrayList();
                TradeActionEvent[] eventExercise = EventExercise;
                if (null != eventExercise)
                {
                    foreach (TradeActionEvent item in eventExercise)
                    {
                        TradeActionEvent[] eventCallCurrencyAmount = item.EventCallCurrencyAmount;
                        if (null != eventCallCurrencyAmount)
                        {
                            foreach (TradeActionEvent subItem in eventCallCurrencyAmount)
                            {
                                if (EventCodeFunc.IsIntermediary(subItem.eventCode))
                                    aAllIntermediaryCallCurrencyAmount.Add(subItem);
                            }
                        }
                    }
                    if (0 < aAllIntermediaryCallCurrencyAmount.Count)
                        return (TradeActionEvent[])aAllIntermediaryCallCurrencyAmount.ToArray(typeof(TradeActionEvent));
                }
                return null;
            }
        }
        #endregion EventAllIntermediaryCallCurrencyAmount
        #region EventAllIntermediaryOptionEntitlement
        public TradeActionEvent[] EventAllIntermediaryOptionEntitlement
        {
            get
            {
                ArrayList aAllIntermediaryQty = new ArrayList();
                TradeActionEvent[] eventExercise = EventExercise;
                if (null != eventExercise)
                {
                    foreach (TradeActionEvent item in eventExercise)
                    {
                        TradeActionEvent[] eventSingleUnderlyer = item.EventSingleUnderlyer;
                        if (null != eventSingleUnderlyer)
                        {
                            foreach (TradeActionEvent subItem in eventSingleUnderlyer)
                            {
                                if (EventCodeFunc.IsIntermediary(subItem.eventCode))
                                    aAllIntermediaryQty.Add(subItem);
                            }
                        }
                    }
                    if (0 < aAllIntermediaryQty.Count)
                        return (TradeActionEvent[])aAllIntermediaryQty.ToArray(typeof(TradeActionEvent));
                }
                return null;
            }
        }
        #endregion EventAllIntermediaryOptionEntitlement
        #region EventAllIntermediaryPutCurrencyAmount
        public TradeActionEvent[] EventAllIntermediaryPutCurrencyAmount
        {
            get
            {
                ArrayList aAllIntermediaryPutCurrencyAmount = new ArrayList();
                TradeActionEvent[] eventExercise = EventExercise;
                if (null != eventExercise)
                {
                    foreach (TradeActionEvent item in eventExercise)
                    {
                        TradeActionEvent[] eventPutCurrencyAmount = item.EventPutCurrencyAmount;
                        if (null != eventPutCurrencyAmount)
                        {

                            foreach (TradeActionEvent subItem in eventPutCurrencyAmount)
                            {
                                if (EventCodeFunc.IsIntermediary(subItem.eventCode))
                                    aAllIntermediaryPutCurrencyAmount.Add(subItem);
                            }
                        }
                    }
                    if (0 < aAllIntermediaryPutCurrencyAmount.Count)
                        return (TradeActionEvent[])aAllIntermediaryPutCurrencyAmount.ToArray(typeof(TradeActionEvent));
                }
                return null;
            }
        }
        #endregion EventAllIntermediaryPutCurrencyAmount
        #region EventAllIntermediarySettlementCurrencyAmount
        public TradeActionEvent[] EventAllIntermediarySettlementCurrencyAmount
        {
            get
            {
                ArrayList aAllIntermediarySettlementCurrencyAmount = new ArrayList();
                TradeActionEvent[] eventExercise = EventExercise;
                if (null != eventExercise)
                {
                    foreach (TradeActionEvent item in eventExercise)
                    {
                        TradeActionEvent[] eventSettlementCurrencyAmount = item.EventSettlementCurrencyAmount;
                        if (null != eventSettlementCurrencyAmount)
                        {

                            foreach (TradeActionEvent subItem in eventSettlementCurrencyAmount)
                            {
                                if (EventCodeFunc.IsIntermediary(subItem.eventCode))
                                    aAllIntermediarySettlementCurrencyAmount.Add(subItem);
                            }
                        }
                    }
                    if (0 < aAllIntermediarySettlementCurrencyAmount.Count)
                        return (TradeActionEvent[])aAllIntermediarySettlementCurrencyAmount.ToArray(typeof(TradeActionEvent));
                }
                return null;
            }
        }
        #endregion EventAllIntermediarySettlementCurrencyAmount

        #region EventAllTerminationCallCurrencyAmount
        public TradeActionEvent EventAllTerminationCallCurrencyAmount
        {
            get
            {
                TradeActionEvent[] eventExercise = EventExercise;
                if (null != eventExercise)
                {
                    foreach (TradeActionEvent item in eventExercise)
                    {
                        TradeActionEvent[] eventCallCurrencyAmount = item.EventCallCurrencyAmount;
                        if (null != eventCallCurrencyAmount)
                        {

                            foreach (TradeActionEvent subItem in eventCallCurrencyAmount)
                            {
                                if (EventCodeFunc.IsTermination(subItem.eventCode))
                                    return subItem;
                            }
                        }
                    }
                }
                return null;
            }
        }
        #endregion EventAllTerminationCallCurrencyAmount
        #region EventAllTerminationOptionEntitlement
        public TradeActionEvent EventAllTerminationOptionEntitlement
        {
            get
            {
                TradeActionEvent[] eventExercise = EventExercise;
                if (null != eventExercise)
                {
                    foreach (TradeActionEvent item in eventExercise)
                    {
                        TradeActionEvent[] eventSingleUnderlyer = item.EventSingleUnderlyer;
                        if (null != eventSingleUnderlyer)
                        {

                            foreach (TradeActionEvent subItem in eventSingleUnderlyer)
                            {
                                if (EventCodeFunc.IsTermination(subItem.eventCode))
                                    return subItem;
                            }
                        }
                    }
                }
                return null;
            }
        }
        #endregion EventAllTerminationOptionEntitlement
        #region EventAllTerminationPutCurrencyAmount
        public TradeActionEvent EventAllTerminationPutCurrencyAmount
        {
            get
            {
                TradeActionEvent[] eventExercise = EventExercise;
                if (null != eventExercise)
                {
                    foreach (TradeActionEvent item in eventExercise)
                    {
                        TradeActionEvent[] eventPutCurrencyAmount = item.EventPutCurrencyAmount;
                        if (null != eventPutCurrencyAmount)
                        {
                            foreach (TradeActionEvent subItem in eventPutCurrencyAmount)
                            {
                                if (EventCodeFunc.IsTermination(subItem.eventCode))
                                    return subItem;
                            }
                        }
                    }
                }
                return null;
            }
        }
        #endregion EventAllTerminationCallCurrencyAmount
        #region EventAllTerminationSettlementCurrencyAmount
        public TradeActionEvent EventAllTerminationSettlementCurrencyAmount
        {
            get
            {
                TradeActionEvent[] eventExercise = EventExercise;
                if (null != eventExercise)
                {
                    foreach (TradeActionEvent item in eventExercise)
                    {
                        TradeActionEvent[] eventSettlementCurrencyAmount = item.EventSettlementCurrencyAmount;
                        if (null != eventSettlementCurrencyAmount)
                        {
                            foreach (TradeActionEvent subItem in eventSettlementCurrencyAmount)
                            {
                                if (EventCodeFunc.IsTermination(subItem.eventCode))
                                    return subItem;
                            }
                        }
                    }
                }
                return null;
            }
        }
        #endregion EventAllTerminationCallCurrencyAmount
        #region EventBarrier
        private TradeActionEvent[] EventBarrier
        {
            get { return GetEvent_EventCode(EventCodeFunc.Barrier); }
        }
        #endregion EventBarrier
        #region EventCallCurrencyAmount
        private TradeActionEvent[] EventCallCurrencyAmount
        {
            get { return GetEvent_EventType(EventTypeFunc.CallCurrency); }
        }
        #endregion EventCallCurrencyAmount
        #region EventCustomerSettlementRebate
        public TradeActionEvent EventCustomerSettlementRebate
        {
            get
            {
                TradeActionEvent[] eventCustomerSettlementRebate = GetEvent_EventType(EventTypeFunc.Rebate);
                if (null != eventCustomerSettlementRebate)
                {
                    if (1 == eventCustomerSettlementRebate.Length)
                        return eventCustomerSettlementRebate[0];
                }
                return null;
            }
        }
        #endregion EventCustomerSettlementRebate
        #region EventCustomerSettlementPayout
        public TradeActionEvent EventCustomerSettlementPayout
        {
            get
            {
                TradeActionEvent[] eventCustomerSettlementPayout = GetEvent_EventType(EventTypeFunc.Payout);
                if (null != eventCustomerSettlementPayout)
                {
                    if (1 == eventCustomerSettlementPayout.Length)
                        return eventCustomerSettlementPayout[0];
                }
                return null;
            }
        }
        #endregion EventCustomerSettlementPayout
        #region EventExerciseCashSettlement
        public TradeActionEvent EventExerciseCashSettlement
        {
            get
            {
                TradeActionEvent[] eventExerciseCashSettlement = GetEvent_EventType(EventTypeFunc.CashSettlement);
                if ((null != eventExerciseCashSettlement) && (1 == eventExerciseCashSettlement.Length))
                    return eventExerciseCashSettlement[0];
                return null;
            }
        }
        #endregion EventExerciseCashSettlement
        #region EventExercise
        public TradeActionEvent[] EventExercise
        {
            get { return GetEvent_EventCode(EventCodeFunc.Exercise); }
        }
        #endregion EventExercise
        #region EventExerciseEffect
        private TradeActionEvent[] EventExerciseEffect
        {
            get
            {
                ArrayList aEventExercise = new ArrayList();
                TradeActionEvent[] eventExercise = EventExercise;
                if (null != eventExercise)
                {
                    foreach (TradeActionEvent item in eventExercise)
                    {
                        if (null != item.EventClassGroupLevel)
                            aEventExercise.Add(item);
                    }
                    if (0 < aEventExercise.Count)
                        return (TradeActionEvent[])aEventExercise.ToArray(typeof(TradeActionEvent));
                }
                return null;
            }
        }
        #endregion EventExerciseEffect
        #region EventExercisePartiel
        private TradeActionEvent EventExercisePartiel
        {
            get
            {
                TradeActionEvent[] eventExercise = EventExercise;
                if (null != eventExercise)
                {
                    foreach (TradeActionEvent item in eventExercise)
                    {
                        if (EventCodeAndEventTypeFunc.IsExercisePartiel(item.eventCode, item.eventType))
                            return item;
                    }
                }
                return null;
            }
        }
        #endregion EventExercisePartiel
        #region EventExercisePartielEffect
        private TradeActionEvent EventExercisePartielEffect
        {
            get
            {
                TradeActionEvent eventExercisePartiel = EventExercisePartiel;
                if ((null != eventExercisePartiel) && (null != eventExercisePartiel.EventClassGroupLevel))
                    return eventExercisePartiel;
                return null;
            }
        }
        #endregion EventExercisePartielEffect
        #region EventExerciseTotal
        private TradeActionEvent EventExerciseTotal
        {
            get
            {
                TradeActionEvent[] eventExercise = EventExercise;
                if (null != eventExercise)
                {
                    foreach (TradeActionEvent item in eventExercise)
                    {
                        if (EventCodeAndEventTypeFunc.IsExerciseTotal(item.eventCode, item.eventType))
                            return item;
                    }
                }
                return null;
            }
        }
        #endregion EventExerciseTotal
        #region EventExerciseTotalEffect
        public TradeActionEvent EventExerciseTotalEffect
        {
            get
            {
                TradeActionEvent eventExerciseTotal = EventExerciseTotal;
                if ((null != eventExerciseTotal) && (null != eventExerciseTotal.EventClassGroupLevel))
                    return eventExerciseTotal;
                return null;
            }
        }
        #endregion EventExerciseTotalEffect
        #region EventExerciseVirtual
        private TradeActionEvent EventExerciseVirtual
        {
            get
            {
                TradeActionEvent[] eventExercise = EventExercise;
                if (null != eventExercise)
                {
                    foreach (TradeActionEvent item in eventExercise)
                    {
                        if (null == item.EventClassGroupLevel)
                            return item;
                    }
                }
                return null;
            }
        }
        #endregion EventExerciseVirtual
        #region EventFxForward
        private TradeActionEvent EventFxForward
        {
            get
            {
                foreach (TradeActionEvent item in events)
                {
                    if (EventCodeFunc.IsFxForward(item.eventCode))
                        return item;
                }
                return null;
            }
        }
        #endregion EventFxForward
        #region EventIntermediaryCallCurrencyAmount
        public TradeActionEvent EventIntermediaryCallCurrencyAmount
        {
            get
            {
                TradeActionEvent[] eventCallCurrencyAmount = EventCallCurrencyAmount;
                if (null != eventCallCurrencyAmount)
                {
                    foreach (TradeActionEvent item in eventCallCurrencyAmount)
                    {
                        if (EventCodeFunc.IsIntermediary(item.eventCode))
                            return item;
                    }
                }
                return null;
            }
        }
        #endregion EventIntermediaryCallCurrencyAmount
        #region EventIntermediaryPutCurrencyAmount
        public TradeActionEvent EventIntermediaryPutCurrencyAmount
        {
            get
            {
                TradeActionEvent[] eventPutCurrencyAmount = EventPutCurrencyAmount;
                if (null != eventPutCurrencyAmount)
                {
                    foreach (TradeActionEvent item in eventPutCurrencyAmount)
                    {
                        if (EventCodeFunc.IsIntermediary(item.eventCode))
                            return item;
                    }
                }
                return null;
            }
        }
        #endregion EventIntermediaryPutCurrencyAmount
        #region EventIntermediarySettlementCurrencyAmount
        public TradeActionEvent EventIntermediarySettlementCurrencyAmount
        {
            get
            {
                TradeActionEvent[] eventSettlementCurrencyAmount = EventSettlementCurrencyAmount;
                if (null != eventSettlementCurrencyAmount)
                {
                    foreach (TradeActionEvent item in eventSettlementCurrencyAmount)
                    {
                        if (EventCodeFunc.IsIntermediary(item.eventCode))
                            return item;
                    }
                }
                return null;
            }
        }
        #endregion EventIntermediarySettlementCurrencyAmount
        #region EventOption
        // EG 20180514 [23812] Report
        public TradeActionEvent EventOption
        {
            get
            {
                foreach (TradeActionEvent item in events)
                {
                    if (EventCodeFunc.IsFxOption(item.eventCode) || EventCodeFunc.IsEquityOption(item.eventCode) || EventCodeFunc.IsBondOption(item.eventCode))
                        return item;
                }
                return null;
            }
        }
        #endregion EventOption
        #region EventUnderlyer
        public TradeActionEvent[] EventUnderlyer(string pEventType)
        {
            return GetEvent_EventType(pEventType);
        }
        #endregion EventSingleUnderlyer
        #region EventSingleUnderlyer
        private TradeActionEvent[] EventSingleUnderlyer
        {
            get { return GetEvent_EventType(EventTypeFunc.SingleUnderlyer); }
        }
        #endregion EventSingleUnderlyer
        #region EventInitialSingleUnderlyer
        public TradeActionEvent EventInitialSingleUnderlyer
        {
            get
            {
                TradeActionEvent[] eventSingleUnderlyer = EventSingleUnderlyer;
                if (null != eventSingleUnderlyer)
                {
                    foreach (TradeActionEvent item in eventSingleUnderlyer)
                    {
                        if (EventCodeFunc.IsInitialValuation(item.eventCode) && (null != item.EventClassGroupLevel))
                            return item;
                    }
                }
                return null;
            }
        }
        #endregion EventInitialSingleUnderlyer

        #region SetEventInitialUnderlyer
        public TradeActionEvent SetEventInitialUnderlyer(string pEventType)
        {
            TradeActionEvent[] eventUnderlyer = GetEvent_EventType(pEventType);
            if (null != eventUnderlyer)
            {
                foreach (TradeActionEvent item in eventUnderlyer)
                {
                    if (EventCodeFunc.IsInitialValuation(item.eventCode) && (null != item.EventClassGroupLevel))
                        return item;
                }
            }
            return null;
        }
        #endregion SetEventInitialUnderlyer

        #region EventOut
        private TradeActionEvent[] EventOut
        {
            get { return GetEvent_EventCode(EventCodeFunc.Out); }
        }
        #endregion EventOut
        #region EventOutEffect
        private TradeActionEvent EventOutEffect
        {
            get
            {
                TradeActionEvent[] eventOut = EventOut;
                if (null != eventOut)
                {
                    foreach (TradeActionEvent item in eventOut)
                    {
                        if (null != item.EventClassGroupLevel)
                            return item;
                    }
                }
                return null;
            }
        }
        #endregion EventOutEffect
        #region EventOutVirtual
        private TradeActionEvent EventOutVirtual
        {
            get
            {
                TradeActionEvent[] eventOut = EventOut;
                if (null != eventOut)
                {
                    foreach (TradeActionEvent item in eventOut)
                    {
                        if (null == item.EventClassGroupLevel)
                            return item;
                    }
                }
                return null;
            }
        }
        #endregion EventOutVirtual
        #region EventPayout
        private TradeActionEvent[] EventPayout
        {
            get { return GetEvent_EventType(EventTypeFunc.Payout); }
        }
        #endregion EventPayout
        #region EventPayoutRecognition
        public TradeActionEvent EventPayoutRecognition
        {
            get
            {
                TradeActionEvent[] eventPayout = EventPayout;
                if (null != eventPayout)
                {
                    foreach (TradeActionEvent item in eventPayout)
                    {
                        if (null != item.EventClassRecognition)
                            return item;
                    }
                }
                return null;
            }
        }
        #endregion EventPayoutRecognition
        #region EventPayoutSettlement
        public TradeActionEvent EventPayoutSettlement
        {
            get
            {
                TradeActionEvent[] eventPayout = EventPayout;
                if (null != eventPayout)
                {
                    foreach (TradeActionEvent item in eventPayout)
                    {
                        if (null != item.EventClassSettlement)
                            return item;
                    }
                }
                return null;
            }
        }
        #endregion EventPayoutSettlement
        #region EventPutCurrencyAmount
        private TradeActionEvent[] EventPutCurrencyAmount
        {
            get { return GetEvent_EventType(EventTypeFunc.PutCurrency); }
        }
        #endregion EventPutCurrencyAmount
        #region EventRebate
        public TradeActionEvent[] EventRebate
        {
            get { return GetEvent_EventType(EventTypeFunc.Rebate); }
        }
        #endregion EventRebate
        #region EventRebateRecognition
        public TradeActionEvent EventRebateRecognition
        {
            get
            {
                TradeActionEvent[] eventRebate = EventRebate;
                if (null != eventRebate)
                {
                    foreach (TradeActionEvent item in eventRebate)
                    {
                        if (null != item.EventClassRecognition)
                            return item;
                    }
                }
                return null;
            }
        }
        #endregion EventRebateRecognition
        #region EventRebateSettlement
        public TradeActionEvent EventRebateSettlement
        {
            get
            {
                TradeActionEvent[] eventRebate = EventRebate;
                if (null != eventRebate)
                {
                    foreach (TradeActionEvent item in eventRebate)
                    {
                        if (null != item.EventClassSettlement)
                            return item;
                    }
                }
                return null;
            }
        }
        #endregion EventRebateSettlement
        #region EventResetFxCustomer
        public TradeActionEvent[] EventResetFxCustomer
        {
            get { return GetEvent_EventType(EventTypeFunc.FxCalculationAgent); }
        }
        #endregion EventResetFxCustomer
        #region EventResetFxRate
        public TradeActionEvent[] EventResetFxRate
        {
            get { return GetEvent_EventType(EventTypeFunc.FxRate); }
        }
        #endregion EventResetFxRate
        #region EventStartCallCurrencyAmountRecognition
        public TradeActionEvent EventStartCallCurrencyAmountRecognition
        {
            get
            {
                TradeActionEvent[] eventCallCurrencyAmount = EventCallCurrencyAmount;
                if (null != eventCallCurrencyAmount)
                {
                    foreach (TradeActionEvent item in eventCallCurrencyAmount)
                    {
                        if (EventCodeFunc.IsStart(item.eventCode) && (null != item.EventClassRecognition))
                            return item;
                    }
                }
                return null;
            }
        }
        #endregion EventStartCallCurrencyAmountRecognition
        #region EventStartPutCurrencyAmountRecognition
        public TradeActionEvent EventStartPutCurrencyAmountRecognition
        {
            get
            {
                TradeActionEvent[] eventPutCurrencyAmount = EventPutCurrencyAmount;
                if (null != eventPutCurrencyAmount)
                {
                    foreach (TradeActionEvent item in eventPutCurrencyAmount)
                    {
                        if (EventCodeFunc.IsStart(item.eventCode) && (null != item.EventClassRecognition))
                            return item;
                    }
                }
                return null;
            }
        }
        #endregion EventStartPutCurrencyAmountRecognition
        #region EventSettlementCurrencyAmount
        private TradeActionEvent[] EventSettlementCurrencyAmount
        {
            get { return GetEvent_EventType(EventTypeFunc.SettlementCurrency); }
        }
        #endregion EventSettlementCurrencyAmount
        #region EventTerminationCallCurrencyAmount
        public TradeActionEvent EventTerminationCallCurrencyAmount
        {
            get
            {
                TradeActionEvent[] eventCallCurrencyAmount = EventCallCurrencyAmount;
                if (null != eventCallCurrencyAmount)
                {
                    foreach (TradeActionEvent item in eventCallCurrencyAmount)
                    {
                        if (EventCodeFunc.IsTermination(item.eventCode))
                            return item;
                    }
                }
                return null;
            }
        }
        #endregion EventTerminationCallCurrencyAmount
        #region EventTerminationPutCurrencyAmount
        public TradeActionEvent EventTerminationPutCurrencyAmount
        {
            get
            {
                TradeActionEvent[] eventPutCurrencyAmount = EventPutCurrencyAmount;
                if (null != eventPutCurrencyAmount)
                {
                    foreach (TradeActionEvent item in eventPutCurrencyAmount)
                    {
                        if (EventCodeFunc.IsTermination(item.eventCode))
                            return item;
                    }
                }
                return null;
            }
        }
        #endregion EventTerminationPutCurrencyAmount
        #region EventTerminationSingleUnderlyer
        public TradeActionEvent EventTerminationSingleUnderlyer
        {
            get
            {
                TradeActionEvent[] eventSingleUnderlyer = EventSingleUnderlyer;
                if (null != eventSingleUnderlyer)
                {
                    foreach (TradeActionEvent item in eventSingleUnderlyer)
                    {
                        if (EventCodeFunc.IsTermination(item.eventCode))
                            return item;
                    }
                }
                return null;
            }
        }
        #endregion EventTerminationSingleUnderlyer
        #region EventTerminationSettlementCurrencyAmount
        public TradeActionEvent EventTerminationSettlementCurrencyAmount
        {
            get
            {
                TradeActionEvent[] eventSettlementCurrencyAmount = EventSettlementCurrencyAmount;
                if (null != eventSettlementCurrencyAmount)
                {
                    foreach (TradeActionEvent item in eventSettlementCurrencyAmount)
                    {
                        if (EventCodeFunc.IsTermination(item.eventCode))
                            return item;
                    }
                }
                return null;
            }
        }
        #endregion EventTerminationPutCurrencyAmount
        #region EventTrigger
        public TradeActionEvent[] EventTrigger
        {
            get { return GetEvent_EventCode(EventCodeFunc.Trigger); }
        }
        #endregion EventTrigger

        #region SetEventTerminationUnderlyer
        public TradeActionEvent SetEventTerminationUnderlyer(string pEventType)
        {
            TradeActionEvent[] eventUnderlyer = GetEvent_EventType(pEventType);
            if (null != eventUnderlyer)
            {
                foreach (TradeActionEvent item in eventUnderlyer)
                {
                    if (EventCodeFunc.IsTermination(item.eventCode))
                        return item;
                }
            }
            return null;
        }
        #endregion SetEventTerminationUnderlyer



        #region DisplayAbandon
        private PlaceHolder DisplayAbandon
        {
            get
            {
                PlaceHolder plhAbandon = new PlaceHolder();
                bool isConditionRespected = (IsAllBarriersDeclared && (false == IsOptionDead));
                bool isExistAbandonEffect = (null != EventAbandonEffect);
                bool isExistExerciseTotalEffect = (null != EventExerciseTotalEffect);
                //bool isExistExercisePartielEffect = (null != EventExercisePartielEffect);
                bool isExistExercise = (null != EventExercise);
                if (isExistExerciseTotalEffect || isExistAbandonEffect)
                {
                    if (isExistExercise)
                        // Exercise Total already present nothing to do
                        plhAbandon.Controls.Add(DisplayEvents(EventExercise, false));
                    if (isExistAbandonEffect)
                        // Abandon already present nothing to do
                        plhAbandon.Controls.Add(DisplayEvents(EventAbandon, false));
                }
                else
                {
                    bool isExistAbandon = (null != EventAbandon);
                    TradeActionEvent[] eventAbandon = new TradeActionEvent[1] { new TradeActionEvent() };

                    if (isExistExercise)
                        // Exercise partial or multiple already present : abandon must be partial
                        plhAbandon.Controls.Add(DisplayEvents(EventExercise, false));

                    if (isExistAbandon)
                    {
                        // Event already present in this process
                        eventAbandon[0] = EventAbandon[0];
                        if (isConditionRespected)
                        {
                            // Display Event already present during this process
                            plhAbandon.Controls.Add(DisplayEvents(eventAbandon, true));
                            eventAbandon[0].isModified = true;
                        }
                        else
                        {
                            eventAbandon[0].m_Action = null;
                            eventAbandon[0].m_OriginalAction = null;
                        }
                    }
                    else
                    {
                        // Event not exist 
                        if (isConditionRespected)
                        {
                            // Conditions of abandon are observed, creation of Event Payout during this process
                            eventAbandon[0].instrumentNo = instrumentNo;
                            eventAbandon[0].streamNo = streamNo;
                            eventAbandon[0].eventCode = EventCodeFunc.Abandon;
                            eventAbandon[0].eventType = (isExistExercise ? EventTypeFunc.Partiel : EventTypeFunc.Total);
                            eventAbandon[0].dtStartPeriod = dtStartPeriod;
                            eventAbandon[0].dtEndPeriod = dtEndPeriod;
                            eventAbandon[0].idE = -idE;
                            eventAbandon[0].idEParent = idE;
                            this.Add(eventAbandon[0]);
                            // Display Event 
                            plhAbandon.Controls.Add(DisplayEvents(eventAbandon, true));
                            eventAbandon[0].isModified = true;
                        }
                    }
                }
                return plhAbandon;
            }
        }
        #endregion DisplayAbandon
        #region DisplayBarriers
        private PlaceHolder DisplayBarriers
        {
            get
            {
                bool isActionAuthorized = false;
                if (IsBarrierOption)
                    isActionAuthorized = (null == this.EventOutEffect);
                else if (IsDigitalOption)
                    isActionAuthorized = (null == this.EventPayoutSettlement);
                else if (IsFeatures)
                    isActionAuthorized = true;

                if (isActionAuthorized)
                    isActionAuthorized = (null == this.EventAbandonEffect && null == this.EventExerciseEffect);

                return DisplayEvents(EventBarrier, isActionAuthorized);
            }
        }
        #endregion DisplayBarriers
        #region DisplayExercise
        /// <revision>
        ///     <version>1.1.0 build 46</version><date>20060628</date><author>EG</author>
        ///     <EurosysSupport>N° 10270</EurosysSupport>
        ///     <comment>
        ///     Ajout du test IsExistExerciseMultipleTotalEffect pour ne pas afficher une nouvelle ligne
        ///     d'exercice dans la cas d'exercices multiples opérés pour la totalité du montant d'origine
        ///     </comment>
        /// </revision>
        private PlaceHolder DisplayExercise
        {
            get
            {
                PlaceHolder plhExercise = new PlaceHolder();
                bool isConditionRespected = (IsAllBarriersDeclared && (false == IsOptionDead));
                bool isExistAbandonEffect = (null != EventAbandonEffect);
                bool isExistExerciseTotalEffect = (null != EventExerciseTotalEffect);
                bool isExistExercisePartielEffect = (null != EventExercisePartielEffect);
                bool isExistExerciseEffect = (null != EventExerciseEffect);

                if (isExistExerciseTotalEffect || isExistExercisePartielEffect || isExistAbandonEffect || IsExistExerciseMultipleTotalEffect)
                {
                    if (isExistExerciseEffect)
                        // Exercise Total/Partial/multiple already present nothing to do
                        plhExercise.Controls.Add(DisplayEvents(EventExercise, false));
                    if (isExistAbandonEffect)
                        // Abandon already present nothing to do
                        plhExercise.Controls.Add(DisplayEvents(EventAbandon, false));
                }
                else
                {
                    bool isExistExerciseVirtual = (null != EventExerciseVirtual);
                    TradeActionEvent[] eventExercise = new TradeActionEvent[1] { new TradeActionEvent() };

                    if (isExistExerciseEffect)
                        // Exercise multiple already present : exercise must be multiple
                        plhExercise.Controls.Add(DisplayEvents(EventExerciseEffect, false));

                    if (isExistExerciseVirtual)
                    {
                        // Event already present in this process
                        eventExercise[0] = EventExerciseVirtual;
                        if (isConditionRespected)
                        {
                            // Display Event already present during this process
                            plhExercise.Controls.Add(DisplayEvents(eventExercise, true));
                            eventExercise[0].isModified = true;
                        }
                        else
                        {
                            /*
                            if (IsBarrierOption)
                            {
                                this.Remove(eventExercise[0]);
                                plhExercise.Controls.Add(DisplayRebate);
                            }
                            else
                            {
                            }
                            */
                            eventExercise[0].m_Action = null;
                            eventExercise[0].m_OriginalAction = null;
                        }
                    }
                    else
                    {
                        // Event not exist 
                        if (isConditionRespected)
                        {
                            // Conditions of exercise are observed, creation of Event Payout during this process
                            eventExercise[0].instrumentNo = instrumentNo;
                            eventExercise[0].streamNo = streamNo;
                            eventExercise[0].eventCode = EventCodeFunc.Exercise;
                            eventExercise[0].eventType = (isExistExerciseEffect ? EventTypeFunc.Multiple : EventTypeFunc.Total);
                            eventExercise[0].dtStartPeriod = dtStartPeriod;
                            eventExercise[0].dtEndPeriod = dtEndPeriod;
                            eventExercise[0].idE = -idE;
                            eventExercise[0].idEParent = idE;
                            this.Add(eventExercise[0]);
                            // Display Event 
                            plhExercise.Controls.Add(DisplayEvents(eventExercise, true));
                            eventExercise[0].isModified = true;
                        }
                        /*
                        else
                        {
                            if (IsBarrierOption)
                                plhExercise.Controls.Add(DisplayRebate);
                        }
                        */
                    }
                }
                return plhExercise;
            }
        }
        #endregion DisplayExercise


        #region DisplayBondOptionOut
        /// <summary>
        /// FeaturePayment for a BondOption KnockOut
        /// <para/>
        /// <para><b>Conditions of event payment generation</b></para>
        /// <para>. Option is desactivated (KnockIn is deactivated or KnockOut is activated)</para>
        /// <para>. Feature Payment specified</para>  
        /// <para/>
        /// </summary>
        private PlaceHolder DisplayBondOptionOut
        {
            get
            {
                PlaceHolder plhBondOptionOut = new PlaceHolder();
                bool isConditionRespected = IsOptionDead;
                bool isExistOutEffect = (null != EventOutEffect);

                IDebtSecurityOption debtSecurityOption = CurrentProduct(instrumentNo) as IDebtSecurityOption;
                TradeActionEvent[] eventBondOptionOut = new TradeActionEvent[1] { new TradeActionEvent() };

                if (isExistOutEffect)
                {
                    // Out already present nothing to do
                    plhBondOptionOut.Controls.Add(DisplayEvents(EventOut, false));
                }
                else
                {
                    #region Event doesn't generated
                    bool isExistBondOptionOut = (null != EventOut);
                    if (isExistBondOptionOut)
                    {
                        // Event already present in this process

                        eventBondOptionOut[0] = EventOut[0];
                        bool isBarriersChanged = IsBarriersChanged;
                        ApplyBarriersOrTriggersChanged(EventBarrier);
                        if (isConditionRespected)
                            // Display Event already present during this process
                            plhBondOptionOut.Controls.Add(DisplayEvents(eventBondOptionOut, true, isBarriersChanged));
                        else
                        {
                            // Delete action event because conditions of payout not respected
                            eventBondOptionOut[0].m_Action = null;
                            eventBondOptionOut[0].m_OriginalAction = null;
                        }
                    }
                    else
                    {
                        // Event not exist 
                        if (isConditionRespected)
                        {
                            // Conditions of rebate are observed, creation of Event Payout during this process
                            eventBondOptionOut[0].instrumentNo = instrumentNo;
                            eventBondOptionOut[0].streamNo = streamNo;
                            eventBondOptionOut[0].eventCode = EventCodeFunc.Out;
                            eventBondOptionOut[0].eventType = EventTypeFunc.Total;
                            if (debtSecurityOption.Feature.KnockSpecified && debtSecurityOption.Feature.Knock.KnockOutSpecified)
                            {

                                if (debtSecurityOption.Feature.Knock.KnockOut.FeaturePaymentSpecified)
                                {
                                    IFeaturePayment featurePayment = debtSecurityOption.Feature.Knock.KnockOut.FeaturePayment;
                                    IMoney paymentAmount = ((IProductBase)debtSecurityOption).CreateMoney();
                                    if (featurePayment.AmountSpecified)
                                    {
                                        paymentAmount.Amount.DecValue = featurePayment.Amount.DecValue;
                                        paymentAmount.Currency = featurePayment.Currency.Value;
                                    }
                                    else if (featurePayment.LevelPercentageSpecified)
                                    {
                                        // TBD
                                    }

                                    IParty payer = CurrentTradeAction.dataDocumentContainer.GetParty(featurePayment.PayerPartyReference.HRef);
                                    IBookId bookPayer = CurrentTradeAction.dataDocumentContainer.GetBookId(featurePayment.PayerPartyReference.HRef);
                                    IParty receiver = CurrentTradeAction.dataDocumentContainer.GetParty(featurePayment.ReceiverPartyReference.HRef);
                                    IBookId bookReceiver = CurrentTradeAction.dataDocumentContainer.GetBookId(featurePayment.PayerPartyReference.HRef);

                                    eventBondOptionOut[0].unit = paymentAmount.Currency;
                                    eventBondOptionOut[0].unitSpecified = true; ;
                                    eventBondOptionOut[0].valorisation = paymentAmount.Amount;
                                    eventBondOptionOut[0].payer = payer.PartyId;
                                    eventBondOptionOut[0].payerBook = bookPayer.Value;
                                    eventBondOptionOut[0].receiver = receiver.PartyId;
                                    eventBondOptionOut[0].receiverBook = bookReceiver.Value;
                                }

                            }
                            DateTime dtKnocked = GetDateBarrierKnocked;
                            if (false == eventBondOptionOut[0].dtStartPeriodSpecified)
                                eventBondOptionOut[0].dtStartPeriod = new EFS_Date();
                            if (false == eventBondOptionOut[0].dtStartPeriodSpecified)
                                eventBondOptionOut[0].dtEndPeriod = new EFS_Date();

                            eventBondOptionOut[0].dtStartPeriod.DateValue = dtKnocked;
                            eventBondOptionOut[0].dtEndPeriod.DateValue = dtKnocked;
                            eventBondOptionOut[0].idE = -idE;
                            eventBondOptionOut[0].idEParent = idE;
                            this.Add(eventBondOptionOut[0]);
                            // Display Event 
                            plhBondOptionOut.Controls.Add(DisplayEvents(eventBondOptionOut, true));
                        }
                    }
                    #endregion Event doesn't generated
                }
                return plhBondOptionOut;
            }
        }
        #endregion DisplayFeature



        #region DisplayPayout
        private PlaceHolder DisplayPayout
        {
            get
            {
                PlaceHolder plhPayout = new PlaceHolder();
                bool isAllTriggersDeclared = IsAllTriggersDeclared;
                bool isTriggersChanged = IsTriggersChanged;

                bool isExistAbandonEffect = (null != EventAbandonEffect);
                bool isExistExerciseTotalEffect = (null != EventExerciseTotalEffect);
                bool isExistOutEffect = (null != EventOutEffect);

                if (isExistExerciseTotalEffect)
                    // Exercise Total already present nothing to do
                    plhPayout.Controls.Add(DisplayEvents(EventExercise, false));
                else if (isExistAbandonEffect)
                    // Abandon already present nothing to do
                    plhPayout.Controls.Add(DisplayEvents(EventAbandon, false));
                else if (isExistOutEffect)
                    // Out already present nothing to do
                    plhPayout.Controls.Add(DisplayEvents(EventOut, false));
                else
                {
                    bool isExercise = IsInTheMoney();
                    bool isDead = IsOptionDead;
                    bool isExistEventExerciseVirtual = (null != EventExerciseVirtual);
                    bool isExistEventAbandonVirtual = (null != EventAbandonVirtual);
                    bool isExistEventOutVirtual = (null != EventOutVirtual);
                    bool isExistEventVirtual = isExistEventExerciseVirtual || isExistEventAbandonVirtual || isExistEventOutVirtual;
                    TradeActionEvent[] eventExerciseAbandon = new TradeActionEvent[1] { new TradeActionEvent() };
                    ApplyBarriersOrTriggersChanged(EventTrigger);


                    if (isExistEventVirtual)
                    {
                        if (isExercise)
                        {
                            if (isExistEventExerciseVirtual)
                                eventExerciseAbandon[0] = EventExerciseVirtual;
                            else
                            {
                                eventExerciseAbandon[0] = isExistEventAbandonVirtual ? EventAbandonVirtual : EventOutVirtual;
                                eventExerciseAbandon[0].eventCode = EventCodeFunc.Exercise;
                                isTriggersChanged = true;
                            }
                        }
                        else if (isDead)
                        {
                            if (isExistEventOutVirtual)
                                eventExerciseAbandon[0] = EventOutVirtual;
                            else
                            {
                                eventExerciseAbandon[0] = isExistEventAbandonVirtual ? EventAbandonVirtual : EventExerciseVirtual;
                                eventExerciseAbandon[0].eventCode = EventCodeFunc.Out;
                                isTriggersChanged = true;
                            }
                        }
                        else
                        {
                            if (isExistEventAbandonVirtual)
                                eventExerciseAbandon[0] = EventAbandonVirtual;
                            else
                            {
                                eventExerciseAbandon[0] = isExistEventExerciseVirtual ? EventExerciseVirtual : EventOutVirtual;
                                eventExerciseAbandon[0].eventCode = EventCodeFunc.Abandon;
                                isTriggersChanged = true;
                            }
                        }

                        // Event already present in this process
                        if (isAllTriggersDeclared)
                        {
                            // Display Event already present during this process
                            plhPayout.Controls.Add(DisplayEvents(eventExerciseAbandon, isAllTriggersDeclared, isTriggersChanged));
                            eventExerciseAbandon[0].isModified = true;
                        }
                        else
                        {
                            eventExerciseAbandon[0].m_Action = null;
                            eventExerciseAbandon[0].m_OriginalAction = null;
                        }
                    }
                    else
                    {
                        // Event not exist 
                        if (isAllTriggersDeclared)
                        {
                            // Conditions of exercise are observed, creation of Event Payout during this process
                            eventExerciseAbandon[0].instrumentNo = instrumentNo;
                            eventExerciseAbandon[0].streamNo = streamNo;
                            eventExerciseAbandon[0].eventCode = isExercise ? EventCodeFunc.Exercise : (isDead ? EventCodeFunc.Out : EventCodeFunc.Abandon);
                            eventExerciseAbandon[0].eventType = EventTypeFunc.Total;
                            eventExerciseAbandon[0].dtStartPeriod = dtStartPeriod;
                            eventExerciseAbandon[0].dtEndPeriod = dtEndPeriod;
                            eventExerciseAbandon[0].idE = -idE;
                            eventExerciseAbandon[0].idEParent = idE;
                            this.Add(eventExerciseAbandon[0]);
                            // Display Event 
                            plhPayout.Controls.Add(DisplayEvents(eventExerciseAbandon, isAllTriggersDeclared, isTriggersChanged));
                            eventExerciseAbandon[0].isModified = true;
                        }
                    }
                }
                return plhPayout;
            }
        }
        #endregion DisplayPayout
        #region DisplayRebate
        /// <summary>
        /// Rebate payout for a FxBarrierOption
        /// <para/>
        /// <para><b>Conditions of event payout generation</b></para>
        /// <para>. No event payout</para>
        /// <para>. Option is desactivated (KnockIn is deactivated or KnockOut is activated)</para>
        /// <para>. Trigger payout specified</para>  
        /// <para/>
        /// <para>and if fxRebateBarrierTrigger is specified</para>
        /// <para>. fxRebateBarrier is activated (KnockIn is deactivated or KnockOut is activated</para>
        /// </summary>
        private PlaceHolder DisplayRebate
        {
            get
            {
                PlaceHolder plhRebate = new PlaceHolder();
                bool isConditionRespected = IsOptionDead;
                bool isExistOutEffect = (null != EventOutEffect);

                object product = CurrentProduct(instrumentNo);
                if (product is IFxBarrierOption)
                {
                    IFxBarrierOption fxBarrierOption = product as IFxBarrierOption;
                    if (isConditionRespected && fxBarrierOption.FxRebateBarrierSpecified)
                        isConditionRespected = IsRebateBarrierActivated;
                }
                TradeActionEvent[] eventRebate = new TradeActionEvent[1] { new TradeActionEvent() };

                if (isExistOutEffect)
                {
                    // Out already present nothing to do
                    plhRebate.Controls.Add(DisplayEvents(EventOut, false));
                }
                else
                {
                    #region Event doesn't generated
                    bool isExistRebate = (null != EventOut);
                    if (isExistRebate)
                    {
                        // Event already present in this process

                        eventRebate[0] = EventOut[0];
                        bool isBarriersChanged = IsBarriersChanged;
                        ApplyBarriersOrTriggersChanged(EventBarrier);
                        if (isConditionRespected)
                            // Display Event already present during this process
                            plhRebate.Controls.Add(DisplayEvents(eventRebate, true, isBarriersChanged));
                        else
                        {
                            // Delete action event because conditions of payout not respected
                            eventRebate[0].m_Action = null;
                            eventRebate[0].m_OriginalAction = null;
                        }
                    }
                    else
                    {
                        // Event not exist 
                        if (isConditionRespected)
                        {
                            // Conditions of rebate are observed, creation of Event Payout during this process
                            eventRebate[0].instrumentNo = instrumentNo;
                            eventRebate[0].streamNo = streamNo;
                            eventRebate[0].eventCode = EventCodeFunc.Out;
                            eventRebate[0].eventType = EventTypeFunc.Total;
                            if (product is IFxBarrierOption)
                            {
                                IFxBarrierOption fxBarrierOption = product as IFxBarrierOption;
                                if (fxBarrierOption.TriggerPayoutSpecified)
                                {
                                    //DataDocumentContainer dataDocument = new DataDocumentContainer((IDataDocument)CurrentTradeCommonAction.EFSmLDocument);

                                    IParty payer = CurrentTradeAction.dataDocumentContainer.GetParty(fxBarrierOption.SellerPartyReference.HRef);
                                    IPartyTradeIdentifier payerPartyTradeIdentifier =
                                        CurrentTradeAction.dataDocumentContainer.GetPartyTradeIdentifier(fxBarrierOption.SellerPartyReference.HRef);
                                    IParty receiver = CurrentTradeAction.dataDocumentContainer.GetParty(fxBarrierOption.BuyerPartyReference.HRef);
                                    IPartyTradeIdentifier receiverPartyTradeIdentifier =
                                        CurrentTradeAction.dataDocumentContainer.GetPartyTradeIdentifier(fxBarrierOption.BuyerPartyReference.HRef);
                                    eventRebate[0].unit = fxBarrierOption.TriggerPayout.Currency;
                                    eventRebate[0].valorisation = fxBarrierOption.TriggerPayout.Amount;
                                    eventRebate[0].payer = payer.PartyId;
                                    if ((null != payerPartyTradeIdentifier) && (payerPartyTradeIdentifier.BookIdSpecified))
                                        eventRebate[0].payerBook = payerPartyTradeIdentifier.BookId.Value;
                                    eventRebate[0].receiver = receiver.PartyId;
                                    if ((null != receiverPartyTradeIdentifier) && (receiverPartyTradeIdentifier.BookIdSpecified))
                                        eventRebate[0].receiverBook = receiverPartyTradeIdentifier.BookId.Value;
                                }
                            }
                            else if (product is IDebtSecurityOption)
                            {
                                IDebtSecurityOption debtSecurityOption = product as IDebtSecurityOption;
                                if (debtSecurityOption.Feature.KnockSpecified && 
                                    debtSecurityOption.Feature.Knock.KnockOutSpecified &&
                                    debtSecurityOption.Feature.Knock.KnockOut.FeaturePaymentSpecified)
                                {
                                    IFeaturePayment featurePayment = debtSecurityOption.Feature.Knock.KnockOut.FeaturePayment;
                                    IMoney paymentAmount = ((IProductBase)product).CreateMoney();
                                    if (featurePayment.AmountSpecified)
                                    {
                                        paymentAmount.Amount.DecValue = featurePayment.Amount.DecValue;
                                        paymentAmount.Currency = featurePayment.Currency.Value;
                                    }
                                    else if (featurePayment.LevelPercentageSpecified)
                                    {
                                        // TBD
                                    }

                                    IParty payer = CurrentTradeAction.dataDocumentContainer.GetParty(featurePayment.PayerPartyReference.HRef);
                                    IBookId bookPayer = CurrentTradeAction.dataDocumentContainer.GetBookId(featurePayment.PayerPartyReference.HRef);
                                    IParty receiver = CurrentTradeAction.dataDocumentContainer.GetParty(featurePayment.ReceiverPartyReference.HRef);
                                    IBookId bookReceiver = CurrentTradeAction.dataDocumentContainer.GetBookId(featurePayment.PayerPartyReference.HRef);

                                    eventRebate[0].unit = paymentAmount.Currency;
                                    eventRebate[0].unitSpecified = true;;
                                    eventRebate[0].valorisation = paymentAmount.Amount;
                                    eventRebate[0].payer = payer.PartyId;
                                    eventRebate[0].payerBook = bookPayer.Value;
                                    eventRebate[0].receiver = receiver.PartyId;
                                    eventRebate[0].receiverBook = bookReceiver.Value;
                                }
                            }
                            DateTime dtKnocked = GetDateBarrierKnocked;
                            if (false == eventRebate[0].dtStartPeriodSpecified)
                                eventRebate[0].dtStartPeriod = new EFS_Date();
                            if (false == eventRebate[0].dtStartPeriodSpecified)
                                eventRebate[0].dtEndPeriod = new EFS_Date();

                            eventRebate[0].dtStartPeriod.DateValue = dtKnocked;
                            eventRebate[0].dtEndPeriod.DateValue = dtKnocked;
                            eventRebate[0].idE = -idE;
                            eventRebate[0].idEParent = idE;
                            this.Add(eventRebate[0]);
                            // Display Event 
                            plhRebate.Controls.Add(DisplayEvents(eventRebate, true));
                        }
                    }
                    #endregion Event doesn't generated
                }
                return plhRebate;
            }
        }
        #endregion DisplayRebate
        #region DisplayTriggers
        private PlaceHolder DisplayTriggers
        {
            get
            {
                bool isActionAuthorized = IsAllBarriersDeclared;
                if (isActionAuthorized)
                    isActionAuthorized = (null == this.EventPayoutSettlement);
                if (isActionAuthorized)
                    isActionAuthorized = (null == this.EventAbandonEffect && null == this.EventExerciseEffect);

                return DisplayEvents(EventTrigger, isActionAuthorized);
            }
        }
        #endregion DisplayTriggers

        #region GetDateBarrierKnocked
        protected DateTime GetDateBarrierKnocked
        {
            get
            {
                TradeActionEvent[] eventBarrier = EventBarrier;
                if (null != eventBarrier)
                {
                    foreach (TradeActionEvent bar in eventBarrier)
                    {
                        string idStTrigger = ((FxBarrierEvents)bar.m_Action).idStTrigger;
                        if ((EventTypeFunc.IsKnockOutPlus(bar.eventType) && Cst.StatusTrigger.IsStatusActivated(idStTrigger)) ||
                            (EventTypeFunc.IsKnockInPlus(bar.eventType) && Cst.StatusTrigger.IsStatusDeActivated(idStTrigger)))
                            return ((FxBarrierEvents)bar.m_Action).ActionDateTime;
                    }
                }
                return Convert.ToDateTime(null);
            }
        }
        #endregion GetDateBarrierKnocked
        #region GetDateCappedFlooredBarrierKnocked
        public DateTime GetDateCappedFlooredBarrierKnocked
        {
            get
            {
                TradeActionEvent[] eventBarrier = EventBarrier;
                if (null != eventBarrier)
                {
                    foreach (TradeActionEvent bar in eventBarrier)
                    {
                        string idStTrigger = ((FxBarrierEvents)bar.m_Action).idStTrigger;
                        if ((EventTypeFunc.IsCappedCallOrFlooredPut(bar.eventType) && Cst.StatusTrigger.IsStatusActivated(idStTrigger)))
                            return ((FxBarrierEvents)bar.m_Action).ActionDateTime;
                    }
                }
                return Convert.ToDateTime(null);
            }
        }
        #endregion GetDateCappedFlooredBarrierKnocked
        #region GetSettlementRateCappedFlooredBarrierKnocked
        public Decimal GetSettlementRateCappedFlooredBarrierKnocked
        {
            get
            {
                TradeActionEvent[] eventBarrier = EventBarrier;
                if (null != eventBarrier)
                {
                    foreach (TradeActionEvent bar in eventBarrier)
                    {
                        string idStTrigger = ((FxBarrierEvents)bar.m_Action).idStTrigger;
                        if ((EventTypeFunc.IsCappedCallOrFlooredPut(bar.eventType) && Cst.StatusTrigger.IsStatusActivated(idStTrigger)))
                            return bar.details.rate.DecValue;
                    }
                }
                return Decimal.Zero;
            }
        }
        #endregion GetSettlementRateCappedFlooredBarrierKnocked

        #region IsAllBarriersDeclared
        private bool IsAllBarriersDeclared
        {
            get { return IsAllBarrierTriggerDeclared(EventBarrier); }
        }
        #endregion IsAllBarriersDeclared
        #region IsAllTriggersDeclared
        private bool IsAllTriggersDeclared
        {
            get { return IsAllBarrierTriggerDeclared(EventTrigger); }
        }
        #endregion IsAllTriggersDeclared
        #region IsAllTriggerActivated
        protected bool IsAllTriggerActivated
        {
            get
            {
                TradeActionEvent[] eventTrigger = EventTrigger;
                if (null != eventTrigger)
                {
                    foreach (TradeActionEvent trg in eventTrigger)
                    {
                        if (Cst.StatusTrigger.IsStatusDeActivated(((FxTriggerEvents)trg.m_Action).idStTrigger))
                            return false;
                    }
                }
                return true;
            }
        }
        #endregion IsAllTriggerActivated
        #region IsAverageRateOption
        protected bool IsAverageRateOption
        {
            get { return EventCodeFunc.IsAverageRateOption(eventCode); }
        }
        #endregion IsAverageRateOption
        #region IsBarriersChanged
        public bool IsBarriersChanged
        {
            get
            {
                TradeActionEvent[] eventBarrier = EventBarrier;
                if (null != eventBarrier)
                {
                    foreach (TradeActionEvent item in eventBarrier)
                    {
                        if (true == ((FxBarrierAndTriggerEvents)item.m_Action).IsActionChanged)
                            return true;
                    }
                }
                return false;
            }
        }
        #endregion IsBarriersChanged
        #region IsBarrierOption
        protected bool IsBarrierOption
        {
            get { return EventCodeFunc.IsBarrierOption(eventCode); }
        }
        #endregion IsBarrierOption
        #region IsExistExerciseMultipleTotalEffect
        private bool IsExistExerciseMultipleTotalEffect
        {
            get
            {
                decimal totalCallCcyAmountExercised = 0;
                TradeActionEvent eventInitialCallCcyAmount = EventStartCallCurrencyAmountRecognition;
                if (null != eventInitialCallCcyAmount)
                {
                    decimal initialCallCcyAmount = eventInitialCallCcyAmount.valorisation.DecValue;
                    TradeActionEvent[] eventMultipleExercise = EventAllIntermediaryCallCurrencyAmount;
                    if (null != eventMultipleExercise)
                    {
                        foreach (TradeActionEvent item in eventMultipleExercise)
                        {
                            totalCallCcyAmountExercised += item.valorisation.DecValue;
                        }
                        if (null != EventAllTerminationCallCurrencyAmount)
                            totalCallCcyAmountExercised += EventAllTerminationCallCurrencyAmount.valorisation.DecValue;
                        return (initialCallCcyAmount <= totalCallCcyAmountExercised);
                    }
                }
                return false;
            }
        }
        #endregion IsExistExerciseMultipleTotalEffect
        #region IsFxDigitalOption
        protected bool IsDigitalOption
        {
            get { return EventCodeFunc.IsDigitalOption(eventCode); }
        }
        #endregion IsDigitalOption
        #region IsFeatures
        // EG 20150423 [20513]BANCAPERTA
        protected bool IsFeatures
        {
            get { return EventTypeFunc.IsFeatures(eventType) || CurrentTradeAction.IsFeatures; }
        }
        #endregion IsFeatures

        #region IsInTheMoney
        /// <revision>
        ///     <version>1.1.5</version><date>20070404</date><author>EG</author>
        ///     <EurosysSupport>N° 15409</EurosysSupport>
        ///     <comment>
        ///     Ajout IsAboveOrBelowIn et IsAboveOrBelowOut pour déterminer IsInTheMoney     (Cas European DigitalBarrier option)
        ///     Ajout IsTouchOrNoTouchIn et IsTouchOrNoTouchOut pour déterminer IsInTheMoney (Cas American DigitalBarrier option)
        ///		</comment>
        /// </revision>
        public override bool IsInTheMoney()
        {
            bool isInTheMoney = false;
            if (EventCodeFunc.IsEuropeanDigitalOption(eventCode))
            {
                if (EventTypeFunc.IsAboveOrBelowPlus(eventType))
                    isInTheMoney = IsAllTriggerActivated;
                else if (EventTypeFunc.IsRange(eventType))
                    isInTheMoney = IsAllTriggerActivated;
                else if (EventTypeFunc.IsKnockInPlus(eventType) || EventTypeFunc.IsAboveOrBelowIn(eventType))
                    isInTheMoney = IsAllTriggerActivated && IsOneBarrierActivated;
                else if (EventTypeFunc.IsKnockOutPlus(eventType) || EventTypeFunc.IsAboveOrBelowOut(eventType))
                    isInTheMoney = IsAllTriggerActivated && IsNoBarrierActivated;
            }
            else if (EventCodeFunc.IsAmericanDigitalOption(eventCode))
            {
                if (EventTypeFunc.IsTouchNoTouch(eventType))
                    isInTheMoney = IsAllTriggerActivated;
                else if (EventTypeFunc.IsDoubleNoTouchPlus(eventType))
                    isInTheMoney = IsNoTriggerActivated;
                else if (EventTypeFunc.IsDoubleTouchBoundary(eventType))
                    isInTheMoney = IsNoTriggerActivated || IsAllTriggerActivated;
                else if (EventTypeFunc.IsDoubleTouch(eventType))
                    isInTheMoney = IsOneTriggerActivated;
                else if (EventTypeFunc.IsDoubleTouchLimitPlus(eventType))
                    isInTheMoney = IsAllTriggerActivated;
                else if (EventTypeFunc.IsTouchOrNoTouchIn(eventType))
                    isInTheMoney = IsAllTriggerActivated && IsOneBarrierActivated;
                else if (EventTypeFunc.IsTouchOrNoTouchOut(eventType))
                    isInTheMoney = IsAllTriggerActivated && IsNoBarrierActivated;
            }
            return isInTheMoney;
        }
        public override bool IsInTheMoney(decimal pSpot, decimal pStrike, StrikeQuoteBasisEnum pQuoteBasis)
        {
            bool isInTheMoney = false;
            decimal strike = pStrike;
            decimal spot = pSpot;
            if (EventCodeFunc.IsSimpleOption(eventCode) ||
                EventCodeFunc.IsAverageRateOption(eventCode) ||
                EventCodeFunc.IsBarrierOption(eventCode))
            {
                if (StrikeQuoteBasisEnum.CallCurrencyPerPutCurrency != pQuoteBasis)
                {
                    if (0 != spot)
                    {
                        spot = (1 / spot);
                    }
                    if (0 != strike)
                    {
                        strike = (1 / strike);
                    }
                }
                isInTheMoney = (strike > spot);
            }
            return isInTheMoney;
        }
        // GetMoneyPositionEnum
        #endregion IsInTheMoney
        #region IsNoBarrierActivated
        protected bool IsNoBarrierActivated
        {
            get { return false == IsOneBarrierTriggerActivated(EventBarrier); }
        }
        #endregion IsNoBarrierActivated
        #region IsNoTriggerActivated
        protected bool IsNoTriggerActivated
        {
            get { return false == IsOneBarrierTriggerActivated(EventTrigger); }
        }
        #endregion IsNoTriggerActivated
        #region IsOneBarrierActivated
        protected bool IsOneBarrierActivated
        {
            get { return IsOneBarrierTriggerActivated(EventBarrier); }
        }
        #endregion IsOneBarrierActivated
        #region IsOneTriggerActivated
        protected bool IsOneTriggerActivated
        {
            get { return IsOneBarrierTriggerActivated(EventTrigger); }
        }
        #endregion IsOneTriggerActivated
        #region IsOneBarrierKnockOutActivated
        protected bool IsOneBarrierKnockOutActivated
        {
            get
            {
                TradeActionEvent[] eventBarrier = EventBarrier;
                if (null != eventBarrier)
                {
                    foreach (TradeActionEvent bar in eventBarrier)
                    {
                        if ((EventTypeFunc.IsKnockOutPlus(bar.eventType) || EventTypeFunc.IsRebateOut(bar.eventType))
                            && (Cst.StatusTrigger.IsStatusActivated(((FxBarrierEvents)bar.m_Action).idStTrigger)))
                            return true;
                    }
                }
                return false;
            }
        }
        #endregion IsOneBarrierKnockOutActivated
        #region IsOptionIsCall
        public static bool IsOptionIsCall(string pSettlementCurrency, string pPutCurrency)
        {
            return (pSettlementCurrency == pPutCurrency);
        }
        #endregion IsOptionIsCall
        #region IsOptionDead
        protected bool IsOptionDead
        {
            get
            {
                TradeActionEvent[] eventBarrier = EventBarrier;
                if (null != eventBarrier)
                {
                    foreach (TradeActionEvent bar in eventBarrier)
                    {
                        if (EventTypeFunc.IsKnockOutPlus(bar.eventType) &&
                            (Cst.StatusTrigger.IsStatusActivated(((FxBarrierEvents)bar.m_Action).idStTrigger)))
                            return true;
                        if (EventTypeFunc.IsKnockInPlus(bar.eventType) &&
                            (Cst.StatusTrigger.IsStatusDeActivated(((FxBarrierEvents)bar.m_Action).idStTrigger)))
                            return true;
                    }
                }
                return false;
            }
        }
        #endregion IsOptionDead
        #region IsPayout
        protected bool IsPayout
        {
            get { return EventTypeFunc.IsPayout(eventType); }
        }
        #endregion IsPayout
        #region IsRebate
        protected bool IsRebate
        {
            get { return EventTypeFunc.IsRebate(eventType); }
        }
        #endregion IsRebate
        #region IsRebateBarrierActivated
        protected bool IsRebateBarrierActivated
        {
            get
            {
                TradeActionEvent[] eventBarrier = EventBarrier;
                if (null != eventBarrier)
                {
                    foreach (TradeActionEvent bar in eventBarrier)
                    {
                        if (EventTypeFunc.IsRebateIn(bar.eventType) &&
                            (Cst.StatusTrigger.IsStatusActivated(((FxBarrierEvents)bar.m_Action).idStTrigger)))
                            return true;
                        else if (EventTypeFunc.IsRebateOut(bar.eventType) &&
                            (Cst.StatusTrigger.IsStatusDeActivated(((FxBarrierEvents)bar.m_Action).idStTrigger)))
                            return true;
                    }
                }
                return false;
            }
        }
        #endregion IsRebateBarrierActivated
        #region IsSimpleOption
        protected bool IsSimpleOption
        {
            get { return EventCodeFunc.IsSimpleOption(eventCode); }
        }
        #endregion IsSimpleOption
        #region IsTrigger
        protected bool IsTrigger
        {
            get { return EventCodeFunc.IsTrigger(eventCode); }
        }
        #endregion IsTrigger
        #region IsTriggersChanged
        public bool IsTriggersChanged
        {
            get
            {
                TradeActionEvent[] eventTrigger = EventTrigger;
                if (null != eventTrigger)
                {
                    foreach (TradeActionEvent item in eventTrigger)
                    {
                        if (true == ((FxBarrierAndTriggerEvents)item.m_Action).IsActionChanged)
                            return true;
                    }
                }
                return false;
            }
        }
        #endregion IsTriggersChanged
        #region TouchDate
        public DateTime TouchDate
        {
            get
            {
                DateTime dtTouch = Convert.ToDateTime(null);
                EventClass eventClassFixing = EventClassFixing;
                if (null != eventClassFixing)
                    dtTouch = eventClassFixing.dtEvent.DateValue;
                return dtTouch;
            }
        }
        #endregion TouchDate


        #region Accessors
        public PayerReceiverInfoDet PayerInfo
        {
            get
            {
                Nullable<int> idB = null;
                if (idPayerBookSpecified)
                    idB = idPayerBook;
                return new PayerReceiverInfoDet(PayerReceiverEnum.Payer, idPayer, payer, idB, payerBook);
            }
        }
        public PayerReceiverInfoDet ReceiverInfo
        {
            get
            {
                Nullable<int> idB = null;
                if (idReceiverBookSpecified)
                    idB = idReceiverBook;
                return new PayerReceiverInfoDet(PayerReceiverEnum.Receiver, idReceiver, receiver, idB, receiverBook);
            }
        }
        #endregion Accessors
        #endregion Accessors
        #region Methods
        #region ApplyBarriersOrTriggersChanged
        public static void ApplyBarriersOrTriggersChanged(TradeActionEvent[] pEvent)
        {
            if (null != pEvent)
            {
                foreach (TradeActionEvent item in pEvent)
                {
                    if (true == item.m_Action.IsActionChanged)
                        item.m_OriginalAction = (ActionEventsBase)item.m_Action.Clone();
                }
            }
        }
        #endregion ApplyBarriersOrTriggersChanged
        #region CreateAbandonEvents
        public static PlaceHolder CreateAbandonEvents(TradeActionEvent pEvent, FullConstructor pFullCtor)
        {
            PlaceHolder phAbandonEvents = new PlaceHolder();
            TradeActionEvent eventOption = pEvent.EventOption;
            if (null != eventOption)
            {
                eventOption.currentTradeAction = pEvent.currentTradeAction;
                phAbandonEvents.Controls.Add(eventOption.OpenTitle(pFullCtor));
                // EG 20150423 [20513] BANCAPERTA
                if (eventOption.IsBarrierOption || eventOption.currentTradeAction.IsBarrierFeatures)
                {
                    phAbandonEvents.Controls.Add(eventOption.DisplayBarriers);
                    phAbandonEvents.Controls.Add(new LiteralControl("<br/>"));
                }
                if (false == EventCodeFunc.IsDigitalOption(eventOption.eventCode))
                {
                    phAbandonEvents.Controls.Add(eventOption.DisplayAbandon);
                    phAbandonEvents.Controls.Add(new LiteralControl("<br/>"));
                }
                phAbandonEvents.Controls.Add(eventOption.CloseTitle(pFullCtor));
            }
            return phAbandonEvents;
        }
        #endregion CreateAbandonEvents
        #region CreateBarriersAndTriggersEvents
        // EG 20140708 Add Test on EquityOption/Features
        public static PlaceHolder CreateBarriersAndTriggersEvents(TradeActionEvent pEvent, FullConstructor pFullCtor)
        {
            PlaceHolder phBarrierAndTriggerEvents = new PlaceHolder();
            TradeActionEvent eventOption = pEvent.EventOption;
            if (null != eventOption)
            {
                eventOption.currentTradeAction = pEvent.currentTradeAction;
                phBarrierAndTriggerEvents.Controls.Add(eventOption.OpenTitle(pFullCtor));
                if (eventOption.IsBarrierOption || eventOption.IsDigitalOption)
                {
                    phBarrierAndTriggerEvents.Controls.Add(eventOption.DisplayBarriers);
                    if (eventOption.IsBarrierOption)
                    {
                        phBarrierAndTriggerEvents.Controls.Add(new LiteralControl("<br/>"));
                        phBarrierAndTriggerEvents.Controls.Add(eventOption.DisplayRebate);
                    }
                    else if (eventOption.IsDigitalOption)
                    {
                        phBarrierAndTriggerEvents.Controls.Add(eventOption.DisplayTriggers);
                        phBarrierAndTriggerEvents.Controls.Add(new LiteralControl("<br/>"));
                        phBarrierAndTriggerEvents.Controls.Add(eventOption.DisplayPayout);
                    }
                    phBarrierAndTriggerEvents.Controls.Add(new LiteralControl("<br/>"));
                }
                // EG 20150423 [20513] BANCAPERTA
                else if (eventOption.IsFeatures || eventOption.currentTradeAction.IsBarrierFeatures)
                {
                    phBarrierAndTriggerEvents.Controls.Add(eventOption.DisplayBarriers);
                    phBarrierAndTriggerEvents.Controls.Add(new LiteralControl("<br/>"));
                    phBarrierAndTriggerEvents.Controls.Add(eventOption.DisplayBondOptionOut);
                }
                phBarrierAndTriggerEvents.Controls.Add(eventOption.CloseTitle(pFullCtor));
            }
            return phBarrierAndTriggerEvents;
        }
        #endregion CreateBarriersAndTriggersEvents
        #region CreateCalculationAgentSettlementRateEvents
        //PL 20100628 
        public PlaceHolder CreateCalculationAgentSettlementRateEvents(TradeActionEvent pEvent, FullConstructor pFullCtor)
        {
            PlaceHolder phCalculationAgentSettlementRateEvents = new PlaceHolder();
            TradeActionEvent eventFxForward = pEvent.EventFxForward;
            if (null != eventFxForward)
            {
                eventFxForward.currentTradeAction = pEvent.currentTradeAction;
                phCalculationAgentSettlementRateEvents.Controls.Add(eventFxForward.OpenTitle(pFullCtor));
                phCalculationAgentSettlementRateEvents.Controls.Add(DisplayEvents(eventFxForward.EventSettlementCurrencyAmount, true));
                phCalculationAgentSettlementRateEvents.Controls.Add(eventFxForward.CloseTitle(pFullCtor));
            }
            return phCalculationAgentSettlementRateEvents;
        }
        #endregion CreateCalculationAgentSettlementRateEvents
        #region CreateExerciseEvents
        public static PlaceHolder CreateExerciseEvents(TradeActionEvent pEvent, FullConstructor pFullCtor)
        {
            PlaceHolder phExerciseEvents = new PlaceHolder();
            TradeActionEvent eventOption = pEvent.EventOption;
            if (null != eventOption)
            {
                eventOption.currentTradeAction = pEvent.currentTradeAction;
                phExerciseEvents.Controls.Add(eventOption.OpenTitle(pFullCtor));
                // EG 20150423 [20513] BANCAPERTA
                if (eventOption.IsBarrierOption || eventOption.currentTradeAction.IsBarrierFeatures)
                {
                    phExerciseEvents.Controls.Add(eventOption.DisplayBarriers);
                    phExerciseEvents.Controls.Add(new LiteralControl("<br/>"));
                }
                if (false == EventCodeFunc.IsDigitalOption(eventOption.eventCode))
                {
                    phExerciseEvents.Controls.Add(eventOption.DisplayExercise);
                    phExerciseEvents.Controls.Add(new LiteralControl("<br/>"));
                }
                phExerciseEvents.Controls.Add(eventOption.CloseTitle(pFullCtor));
            }
            return phExerciseEvents;
        }
        #endregion CreateExerciseEvents
        #region IsAllBarrierTriggerDeclared
        private static bool IsAllBarrierTriggerDeclared(TradeActionEvent[] pEvent)
        {
            if (null != pEvent)
            {
                foreach (TradeActionEvent item in pEvent)
                {
                    if (false == Cst.StatusTrigger.IsStatusDeclared(((FxBarrierAndTriggerEvents)item.m_Action).idStTrigger))
                        return false;
                }
            }
            return true;
        }
        #endregion IsAllBarrierTriggerDeclared
        #region IsNoBarrierTriggerActivated
        protected static bool IsNoBarrierTriggerActivated(TradeActionEvent[] pEvent)
        {
            if (null != pEvent)
            {
                foreach (TradeActionEvent item in pEvent)
                {
                    if (Cst.StatusTrigger.IsStatusActivated(((FxBarrierAndTriggerEvents)item.m_Action).idStTrigger))
                        return false;
                }
            }
            return true;
        }
        #endregion IsNoBarrierTriggerActivated
        #region IsOneBarrierTriggerActivated
        protected static bool IsOneBarrierTriggerActivated(TradeActionEvent[] pEvent)
        {
            if (null != pEvent)
            {
                foreach (TradeActionEvent item in pEvent)
                {
                    if (Cst.StatusTrigger.IsStatusActivated(((FxBarrierAndTriggerEvents)item.m_Action).idStTrigger))
                        return true;
                }
            }
            return false;
        }
        #endregion IsOneBarrierTriggerActivated

        #region EventExerciseFee
        public TradeActionEvent EventExerciseFee
        {
            get
            {
                TradeActionEvent[] eventExerciseFee = GetEvent_EventType(EventTypeFunc.Fee);
                if ((null != eventExerciseFee) && (1 == eventExerciseFee.Length))
                    return eventExerciseFee[0];
                return null;
            }
        }
        #endregion EventExerciseFee
        #region EventCancellationTrade
        private TradeActionEvent[] EventCancellationTrade
        {
            get { return GetEvent_EventCode(EventCodeFunc.RemoveTrade); }
        }
        #endregion EventCancellationTrade
        #region EventCancellationTradeGroup
        private TradeActionEvent EventCancellationTradeGroup
        {
            get
            {
                TradeActionEvent[] eventCancellationTrade = EventCancellationTrade;
                if (null != eventCancellationTrade)
                {
                    foreach (TradeActionEvent item in eventCancellationTrade)
                    {
                        if (EventCodeFunc.IsRemoveTrade(item.eventCode) && (null != item.EventClassGroupLevel))
                            return item;
                    }
                }
                return null;
            }
        }
        #endregion EventCancellationTradeGroup
        #region EventCapFloor
        public TradeActionEvent[] EventCapFloor
        {
            get { return GetEvent_EventCode(EventCodeFunc.CapFloor); }
        }
        #endregion EventCapFloor
        #region EventExerciseDates
        public TradeActionEvent[] EventExerciseDates
        {
            get { return GetEvent_EventCode(EventCodeFunc.ExerciseDates); }
        }
        #endregion EventExerciseDates
        #region EventInterestRateSwap
        public TradeActionEvent[] EventInterestRateSwap
        {
            get { return GetEvent_EventCode(EventCodeFunc.InterestRateSwap); }
        }
        #endregion EventInterestRateSwap
        #region EventNominalStep
        public TradeActionEvent[] EventNominalStep
        {
            get { return GetEvent_EventCode(EventCodeFunc.NominalStep); }
        }
        #endregion EventNominalStep
        #region EventProduct
        private TradeActionEvent[] EventProduct
        {
            get { return GetEvent_EventCode(EventCodeFunc.Product); }
        }
        #endregion EventProduct

        #region EventStreamLeg
        // EG 20180514 [23812] Report
        public TradeActionEvent[] EventStreamLeg
        {
            get
            {
                IProductBase productBase = CurrentProduct(instrumentNo) as IProductBase;
                return GetEventStreamLeg(productBase);
            }
        }
        public TradeActionEvent[] GetEventStreamLeg(IProductBase pProductBase)
        {
            TradeActionEvent[] tradeActionEvents = null;
            if (null != pProductBase)
            {
                if (pProductBase.IsSwap)
                    tradeActionEvents = EventInterestRateSwap;
                else if (pProductBase.IsCapFloor)
                    tradeActionEvents = EventCapFloor;
                else if (pProductBase.IsFxOption)
                    tradeActionEvents = new TradeActionEvent[1] { EventOption };
            }
            return tradeActionEvents;
        }

        #endregion EventStreamLeg

        #region Events
        public override TradeActionEventBase[] Events
        {
            get { return events; }
        }
        #endregion Events
        #region FieldEvents
        public override FieldInfo FieldEvents
        {
            get { return this.GetType().GetField("events"); }
        }
        #endregion FieldEvents

        #region Add
        private void Add(TradeActionEvent pEvent)
        {
            ArrayList aEvent = new ArrayList();
            if (null != events)
                aEvent.AddRange(events);
            aEvent.Add(pEvent);
            events = (TradeActionEvent[])aEvent.ToArray(typeof(TradeActionEvent));
        }
        #endregion Add
        #region CreateControls
        /// <summary>
        /// Procédure principale de création du tableau des évènements.
        /// </summary>
        /// EG 20140708 Add IsEQD
        // EG 20180514 [23812] Report 
        public PlaceHolder CreateControls(TradeActionType.TradeActionTypeEnum pTradeActionType, FullConstructor pFullCtor, TradeActionBase pCurrentTradeAction)
        {
            currentTradeAction = pCurrentTradeAction;
            PlaceHolder phEvents = new PlaceHolder();
            if (TradeActionType.IsRemoveTradeEvents(pTradeActionType))
            {
                phEvents.Controls.Add(this.OpenTitle(pFullCtor));
                phEvents.Controls.Add(this.DisplayCancellationTrade);
                phEvents.Controls.Add(new LiteralControl("<br/>"));
                phEvents.Controls.Add(this.CloseTitle(pFullCtor));
            }
            else
            {
                TradeActionEvent[] eventProduct = this.EventProduct;
                if (null != eventProduct)
                {
                    foreach (TradeActionEvent prd in eventProduct)
                    {
                        prd.currentTradeAction = currentTradeAction;
                        phEvents.Controls.Add(prd.OpenTitle(pFullCtor));

                        if (prd.IsStrategy)
                            phEvents.Controls.Add(prd.CreateControls(pTradeActionType, pFullCtor, CurrentTradeAction));
                        else
                        {
                            object product = CurrentProduct(prd.instrumentNo);
                            IProductBase productBase = (IProductBase)product;
                            if (productBase.IsFxOption || productBase.IsEQD || productBase.IsBondOption)
                            {
                                if (TradeActionType.IsBarrierAndTriggerEvents(pTradeActionType))
                                    phEvents.Controls.Add(CreateBarriersAndTriggersEvents(prd, pFullCtor));
                                else if (TradeActionType.IsAbandonEvents(pTradeActionType))
                                    phEvents.Controls.Add(CreateAbandonEvents(prd, pFullCtor));
                                else if (TradeActionType.IsExerciseEvents(pTradeActionType))
                                    phEvents.Controls.Add(CreateExerciseEvents(prd, pFullCtor));

                                if (productBase.IsFxOption)
                                {
                                    if (TradeActionType.IsOptionalEarlyTerminationProvisionEvents(pTradeActionType))
                                        phEvents.Controls.Add(CreateProvisionEvents(prd,
                                            EventCodeFunc.ExerciseOptionalEarlyTermination, EventTypeFunc.OptionalEarlyTerminationProvision, pFullCtor));

                                }

                            }
                            else if (productBase.IsFxLeg)
                            {
                                //PL 20100628
                                if (TradeActionType.IsCalculationAgentSettlementRateEvents(pTradeActionType))
                                {
                                    IFxLeg fxLeg = (IFxLeg)product;
                                    //PL 20100628 customerSettlementRateSpecified à supprimer plus tard...
                                    if (fxLeg.NonDeliverableForwardSpecified &&
                                        (fxLeg.NonDeliverableForward.CustomerSettlementRateSpecified
                                        ||
                                        fxLeg.NonDeliverableForward.CalculationAgentSettlementRateSpecified)
                                        )
                                    {
                                        //PL 20100628
                                        //phEvents.Controls.Add(CreateCustomerSettlementRateEvents(prd, pFullCtor));
                                        phEvents.Controls.Add(CreateCalculationAgentSettlementRateEvents(prd, pFullCtor));
                                    }
                                }
                            }
                            else if (productBase.IsSwap)
                            {

                                if (TradeActionType.IsCancelableProvisionEvents(pTradeActionType))
                                    phEvents.Controls.Add(CreateProvisionEvents(prd,
                                    EventCodeFunc.ExerciseCancelable, EventTypeFunc.CancelableProvision, pFullCtor));
                                else if (TradeActionType.IsExtendibleProvisionEvents(pTradeActionType))
                                    phEvents.Controls.Add(CreateProvisionEvents(prd,
                                        EventCodeFunc.ExerciseExtendible, EventTypeFunc.ExtendibleProvision, pFullCtor));
                                else if (TradeActionType.IsMandatoryEarlyTerminationProvisionEvents(pTradeActionType))
                                    phEvents.Controls.Add(CreateProvisionEvents(prd,
                                        EventCodeFunc.ExerciseMandatoryEarlyTermination, EventTypeFunc.MandatoryEarlyTerminationProvision, pFullCtor));
                                else if (TradeActionType.IsOptionalEarlyTerminationProvisionEvents(pTradeActionType))
                                    phEvents.Controls.Add(CreateProvisionEvents(prd,
                                        EventCodeFunc.ExerciseOptionalEarlyTermination, EventTypeFunc.OptionalEarlyTerminationProvision, pFullCtor));
                                else if (TradeActionType.IsStepUpProvisionEvents(pTradeActionType))
                                    phEvents.Controls.Add(CreateProvisionEvents(prd,
                                        EventCodeFunc.ExerciseStepUp, EventTypeFunc.StepUpProvision, pFullCtor));
                            }
                            else if (productBase.IsCapFloor)
                            {

                                if (TradeActionType.IsMandatoryEarlyTerminationProvisionEvents(pTradeActionType))
                                    phEvents.Controls.Add(CreateProvisionEvents(prd,
                                        EventCodeFunc.ExerciseMandatoryEarlyTermination, EventTypeFunc.MandatoryEarlyTerminationProvision, pFullCtor));
                                else if (TradeActionType.IsOptionalEarlyTerminationProvisionEvents(pTradeActionType))
                                    phEvents.Controls.Add(CreateProvisionEvents(prd,
                                        EventCodeFunc.ExerciseOptionalEarlyTermination, EventTypeFunc.OptionalEarlyTerminationProvision, pFullCtor));
                            }
                            else
                                phEvents.Controls.Add(new LiteralControl(Ressource.GetString("NothingToDo")));
                        }
                        phEvents.Controls.Add(prd.CloseTitle(pFullCtor));
                    }
                }
            }
            return phEvents;
        }
        #endregion CreateControls
        #region DisplayEvents
        private PlaceHolder DisplayEvents(TradeActionEvent[] pEvent, bool pIsActionAuthorized)
        {
            return DisplayEvents(pEvent, pIsActionAuthorized, false);
        }
        private PlaceHolder DisplayEvents(TradeActionEvent[] pEvent, bool pIsActionAuthorized, bool pIsReinit)
        {
            PlaceHolder ph = new PlaceHolder();
            if (null != pEvent)
            {
                Table table = null;
                foreach (TradeActionEvent item in pEvent)
                {
                    item.currentTradeAction = currentTradeAction;
                    if ((null == item.m_Action) || (false == pIsActionAuthorized) || pIsReinit)
                    {
                        item.m_Action = item.InitializeAction(this);
                        item.isModified = false;
                    }
                    if (null == table)
                        table = item.CreateTable(pIsActionAuthorized);
                    table.Rows.Add(item.CreateRow(pIsActionAuthorized, this));
                }
                ph.Controls.Add(table);
            }
            return ph;
        }
        #endregion DisplayEvents
        #region EventVariationNominal
        public TradeActionEvent EventVariationNominal(TradeActionEventBase pEventNominalStep)
        {
            //ArrayList aEventVariationNominal = new ArrayList();
            TradeActionEvent[] items = GetEvent_EventType(EventTypeFunc.Nominal);
            foreach (TradeActionEvent item in items)
            {
                if ((EventCodeFunc.IsIntermediary(item.eventCode)) &&
                    (pEventNominalStep.dtEndPeriod.DateValue == item.dtStartPeriod.DateValue) &&
                    (pEventNominalStep.instrumentNo == item.instrumentNo) && (pEventNominalStep.streamNo == item.streamNo))
                    return item;
            }
            return null;
        }
        #endregion EventVariationNominal
        #region GetEvent
        public TradeActionEvent GetEvent(int pIdE)
        {
            if (pIdE == this.idE)
                return this;
            else
            {
                TradeActionEvent _event = this[pIdE];
                if ((null == _event) && (null != this.events))
                {
                    foreach (TradeActionEvent item in this.events)
                    {
                        _event = item.GetEvent(pIdE);
                        if (null != _event)
                            return _event;
                    }
                }
                return _event;
            }
        }
        public TradeActionEvent GetEvent(string pEventCode, string pEventType)
        {
            TradeActionEvent[] @events = GetEvent_EventType(pEventType);
            if (ArrFunc.IsFilled(@events))
            {
                foreach (TradeActionEvent @event in @events)
                {
                    if (pEventCode == @event.eventCode)
                        return @event;
                }
            }
            return null;
        }
        #endregion GetEvent
        #region GetEvent_EventCode
        private TradeActionEvent[] GetEvent_EventCode(string pEventCode)
        {
            if (null != events)
            {
                List<TradeActionEvent> lstEvent = new List<TradeActionEvent>();
                foreach (TradeActionEvent @event in events)
                {
                    if (pEventCode == @event.eventCode)
                        lstEvent.Add(@event);
                }
                if (0 < lstEvent.Count)
                    return (TradeActionEvent[])lstEvent.ToArray();
            }
            return null;
        }
        #endregion GetEvent_EventCode
        #region GetEvent_EventType
        public TradeActionEvent[] GetEvent_EventType(string pEventType)
        {
            if (null != events)
            {
                List<TradeActionEvent> lstEvent = new List<TradeActionEvent>();
                foreach (TradeActionEvent @event in events)
                {
                    if (pEventType == @event.eventType)
                        lstEvent.Add(@event);
                }
                if (0 < lstEvent.Count)
                    return (TradeActionEvent[])lstEvent.ToArray();
            }
            return null;
        }
        #endregion GetEvent_EventType
        #region GetEvent_EventTypeRecursive
        public TradeActionEvent[] GetEvent_EventTypeRecursive(string pEventType)
        {
            if (null != events)
            {
                List<TradeActionEvent> lstEvent = new List<TradeActionEvent>();
                foreach (TradeActionEvent @event in events)
                {
                    if (pEventType == @event.eventType)
                        lstEvent.Add(@event);
                    else if (ArrFunc.IsFilled(@event.events))
                    {
                        TradeActionEvent[] subEvent = @event.GetEvent_EventTypeRecursive(pEventType);
                        if (ArrFunc.IsFilled(subEvent))
                            lstEvent.AddRange(subEvent);
                    }
                }
                if (0 < lstEvent.Count)
                    return (TradeActionEvent[])lstEvent.ToArray();
            }
            return null;
        }
        #endregion GetEvent_EventTypeRecursive
        #region GetEventInitial
        public TradeActionEvent GetEventInitial(string pEventType)
        {
            TradeActionEvent[] @events = GetEvent_EventType(pEventType);
            if (ArrFunc.IsFilled(@events))
            {
                foreach (TradeActionEvent @event in @events)
                {
                    if (EventCodeFunc.IsInitialValuation(@event.eventCode))
                        return @event;
                }
            }
            return null;
        }
        #endregion GetEventInitial
        #region GetEventStart
        public TradeActionEvent GetEventStart(string pEventType)
        {
            TradeActionEvent[] @events = GetEvent_EventTypeRecursive(pEventType);
            if (ArrFunc.IsFilled(@events))
            {
                foreach (TradeActionEvent @event in @events)
                {
                    if (EventCodeFunc.IsStart(@event.eventCode))
                        return @event;
                }
            }
            return null;
        }
        #endregion GetEventStart
        #region GetEventTermination
        public TradeActionEvent GetEventTermination(string pEventType)
        {
            TradeActionEvent[] @events = GetEvent_EventTypeRecursive(pEventType);
            if (ArrFunc.IsFilled(@events))
            {
                foreach (TradeActionEvent @event in @events)
                {
                    if (EventCodeFunc.IsTermination(@event.eventCode))
                        return @event;
                }
            }
            return null;
        }
        #endregion GetEventTermination

        #region InitializeAction
        public override ActionEventsBase InitializeAction(TradeActionEventBase pEventParent)
        {
            ActionEventsBase action;
            if (EventCodeFunc.IsTrigger(eventCode))
                action = new FxTriggerEvents(this, pEventParent);
            else if (EventCodeFunc.IsBarrier(eventCode))
                action = new FxBarrierEvents(this, pEventParent);
            else if (EventTypeFunc.IsRebate(eventType))
                action = new RebateEvents(this, pEventParent);
            else if (EventCodeFunc.IsAbandon(eventCode))
                action = InitializeActionAbandon(pEventParent);
            else if (EventCodeFunc.IsExercise(eventCode))
                action = InitializeActionExercise(pEventParent);
            else if (EventCodeFunc.IsOut(eventCode))
                action = InitializeActionOut(pEventParent);
            else if (EventCodeFunc.IsRemoveTrade(eventCode))
                action = new RemoveTradeEvents(SessionTools.CS, this, pEventParent);
            else if (EventTypeFunc.IsSettlementCurrency(eventType))
                action = new CalculationAgentSettlementRateEvents(this, pEventParent);
            else if (EventCodeFunc.IsNominalStep(eventCode))
                action = new NominalStepEvents(this, pEventParent);
            else if (EventCodeFunc.IsProvisionEvent(eventCode))
                action = InitializeActionProvision(pEventParent);
            else
                action = new InformationEvents(this, pEventParent);
            return action;
        }
        #endregion InitializeAction
        #region InitializeActionAbandon
        private ActionEventsBase InitializeActionAbandon(TradeActionEventBase pEventParent)
        {
            ActionEventsBase action = null;
            if (EventCodeFunc.IsDigitalOption(pEventParent.eventCode))
                action = new PayoutEvents(this, pEventParent);
            else if (EventCodeFunc.IsFxOption(pEventParent.eventCode))
                action = new FX_AbandonEvents(this, pEventParent);
            else if (EventCodeFunc.IsEquityOption(pEventParent.eventCode))
                action = new EQD_AbandonEvents(this, pEventParent);
            else if (EventCodeFunc.IsBondOption(pEventParent.eventCode))
                action = new BO_AbandonEvents(this, pEventParent);
            return action;
        }
        #endregion InitializeActionAbandon
        #region InitializeActionExercise
        private ActionEventsBase InitializeActionExercise(TradeActionEventBase pEventParent)
        {
            ActionEventsBase action = null;
            if (EventCodeFunc.IsDigitalOption(pEventParent.eventCode))
                action = new PayoutEvents(this, pEventParent);
            else if (EventCodeFunc.IsFxOption(pEventParent.eventCode))
                action = new FX_ExerciseEvents(this, pEventParent);
            else if (EventCodeFunc.IsEquityOption(pEventParent.eventCode))
                action = new EQD_ExerciseEvents(this, pEventParent);
            else if (EventCodeFunc.IsBondOption(pEventParent.eventCode))
                action = new BO_ExerciseEvents(this, pEventParent);
            return action;
        }
        #endregion InitializeActionExercise
        #region InitializeActionOut
        private ActionEventsBase InitializeActionOut(TradeActionEventBase pEventParent)
        {
            ActionEventsBase action = null;
            if (EventCodeFunc.IsDigitalOption(pEventParent.eventCode))
                action = new PayoutEvents(this, pEventParent);
            else if (EventCodeFunc.IsBarrierOption(pEventParent.eventCode))
                action = new RebateEvents(this, pEventParent);
            else if (EventCodeFunc.IsBondOption(pEventParent.eventCode))
                action = new BO_FeatureEvents(this, pEventParent);
            return action;
        }
        #endregion InitializeActionOut
        #region InitializeActionProvision
        private ActionEventsBase InitializeActionProvision(TradeActionEventBase pEventParent)
        {
            ActionEventsBase action = null;
            if (EventCodeFunc.IsExerciseCancelable(eventCode))
                action = new CancelableProvisionEvents(this, pEventParent);
            else if (EventCodeFunc.IsExerciseExtendible(eventCode))
                action = new ExtendibleProvisionEvents(this, pEventParent);
            else if (EventCodeFunc.IsExerciseMandatoryEarlyTermination(eventCode))
                action = new MandatoryEarlyTerminationProvisionEvents(this, pEventParent);
            else if (EventCodeFunc.IsExerciseOptionalEarlyTermination(eventCode))
                action = new OptionalEarlyTerminationProvisionEvents(this, pEventParent);
            else if (EventCodeFunc.IsExerciseStepUp(eventCode))
                action = new StepUpProvisionEvents(this, pEventParent);
            return action;
        }
        #endregion InitializeActionProvision

        #region CreateProvisionEvents
        // EG 20180514 [23812] Report
        public static PlaceHolder CreateProvisionEvents(TradeActionEvent pEvent, string pEventCode, string pEventType, FullConstructor pFullCtor)
        {
            PlaceHolder phProvisionEvents = new PlaceHolder();
            TradeActionEvent[] tradeActionEvents = pEvent.EventStreamLeg;

            if (null != tradeActionEvents)
            {
                foreach (TradeActionEvent tradeActionEvent in tradeActionEvents)
                {
                    tradeActionEvent.currentTradeAction = pEvent.currentTradeAction;
                    phProvisionEvents.Controls.Add(tradeActionEvent.OpenTitle(pFullCtor));
                    phProvisionEvents.Controls.Add(tradeActionEvent.DisplayNominalStep);
                    phProvisionEvents.Controls.Add(tradeActionEvent.CloseTitle(pFullCtor));
                }
                phProvisionEvents.Controls.Add(new LiteralControl("<br/>"));
                TradeActionEvent[] eventsProvision = pEvent.GetEvent_EventType(pEventType);
                if ((null != eventsProvision) && (0 < eventsProvision.Length))
                {
                    eventsProvision[0].currentTradeAction = pEvent.currentTradeAction;
                    phProvisionEvents.Controls.Add(eventsProvision[0].DisplayProvision(pEvent, pEventCode));
                }
            }
            return phProvisionEvents;
        }
        #endregion CreateProvisionEvents
        #region EventExerciseProvisionEffect
        public TradeActionEvent[] EventExerciseProvisionEffect(string pEventCode)
        {
            ArrayList aEventExercise = new ArrayList();
            TradeActionEvent[] eventExercise = GetEvent_EventCode(pEventCode);
            if (null != eventExercise)
            {
                foreach (TradeActionEvent item in eventExercise)
                {
                    if (null != item.EventClassGroupLevel)
                        aEventExercise.Add(item);
                }
                if (0 < aEventExercise.Count)
                    return (TradeActionEvent[])aEventExercise.ToArray(typeof(TradeActionEvent));
            }
            return null;
        }
        #endregion EventExerciseProvisionEffect
        #region EventExerciseProvisionPartiel
        private TradeActionEvent EventExerciseProvisionPartiel(string pEventCode)
        {
            TradeActionEvent[] eventExercise = GetEvent_EventCode(pEventCode);
            if (null != eventExercise)
            {
                foreach (TradeActionEvent item in eventExercise)
                {
                    if (EventCodeAndEventTypeFunc.IsExercisePartiel(item.eventCode, item.eventType))
                        return item;
                }
            }
            return null;
        }
        #endregion EventExerciseProvisionPartiel
        #region EventExerciseProvisionPartielEffect
        private TradeActionEvent EventExerciseProvisionPartielEffect(string pEventCode)
        {
            TradeActionEvent eventExercise = EventExerciseProvisionPartiel(pEventCode);
            if ((null != eventExercise) && (null != eventExercise.EventClassGroupLevel))
                return eventExercise;
            return null;
        }
        #endregion EventExerciseProvisionPartielEffect
        #region EventExerciseProvisionTotal
        private TradeActionEvent EventExerciseProvisionTotal(string pEventCode)
        {
            TradeActionEvent[] eventExercise = GetEvent_EventCode(pEventCode);
            if (null != eventExercise)
            {
                foreach (TradeActionEvent item in eventExercise)
                {
                    if (EventCodeAndEventTypeFunc.IsExerciseTotal(item.eventCode, item.eventType))
                        return item;
                }
            }
            return null;
        }
        #endregion EventExerciseProvisionTotal
        #region EventExerciseProvisionTotalEffect
        public TradeActionEvent EventExerciseProvisionTotalEffect(string pEventCode)
        {
            TradeActionEvent eventExercise = EventExerciseProvisionTotal(pEventCode);
            if ((null != eventExercise) && (null != eventExercise.EventClassGroupLevel))
                return eventExercise;
            return null;
        }
        #endregion EventExerciseProvisionTotalEffect
        #region EventExerciseProvisionVirtual
        private TradeActionEvent EventExerciseProvisionVirtual(string pEventCode)
        {
            TradeActionEvent[] eventExercise = GetEvent_EventCode(pEventCode);
            if (null != eventExercise)
            {
                foreach (TradeActionEvent item in eventExercise)
                {
                    if (null == item.EventClassGroupLevel)
                        return item;
                }
            }
            return null;
        }
        #endregion EventExerciseProvisionVirtual
        #region EventTerminationNominalProvisionEffect
        // EG 20180514 [23812] Report
        private static EventClass EventTerminationNominalProvisionEffect(TradeActionEvent pEvent, string pEventClass)
        {
            EventClass eventClassProvision = null;
            TradeActionEvent[] tradeActionEvents = pEvent.EventStreamLeg;

            if (null == tradeActionEvents)
            {
                foreach (TradeActionEvent eventStream in tradeActionEvents)
                {
                    TradeActionEvent[] eventNominal = eventStream.GetEvent_EventType(EventTypeFunc.Nominal);
                    if (null != eventNominal)
                    {
                        foreach (TradeActionEvent item in eventNominal)
                        {
                            if (EventCodeFunc.IsTermination(item.eventCode))
                            {
                                if (EventClassFunc.IsExerciseCancelable(pEventClass))
                                    eventClassProvision = item.EventClassExerciseCancelableProvision;
                                else if (EventClassFunc.IsExerciseExtendible(pEventClass))
                                    eventClassProvision = item.EventClassExerciseExtendibleProvision;
                                else if (EventClassFunc.IsExerciseMandatoryEarlyTermination(pEventClass))
                                    eventClassProvision = item.EventClassExerciseMandatoryEarlyTerminationProvision;
                                else if (EventClassFunc.IsExerciseOptionalEarlyTermination(pEventClass))
                                    eventClassProvision = item.EventClassExerciseOptionalEarlyTerminationProvision;
                                else if (EventClassFunc.IsExerciseStepUp(pEventClass))
                                    eventClassProvision = item.EventClassExerciseStepUpProvision;

                            }
                        }
                    }
                    if (null == eventClassProvision)
                        break;
                }
            }
            return eventClassProvision;
        }
        #endregion EventTerminationNominalProvisionEffect

        #endregion Methods

        #region Indexors
        public TradeActionEvent this[int pIdE]
        {
            get
            {
                if (null != events)
                {
                    for (int i = 0; i < events.Length; i++)
                    {
                        if (pIdE == events[i].idE)
                            return events[i];
                    }
                }
                return null;
            }
        }
        #endregion Indexors

    }
    #endregion TradeActionEvent
    #region TradeActionEvents
    [System.Xml.Serialization.XmlRootAttribute("TradeAction", Namespace = "", IsNullable = false)]
    public class TradeActionEvents : TradeActionEventsBase
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("Event")]
        public TradeActionEvent events;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public TradeAction currentTradeAction;
        #endregion Members
        #region Accessors
        #region Events
        public override TradeActionEventBase Events
        {
            get { return events; }
        }
        #endregion Events
        #region SubEvents
        public override TradeActionEventBase[] SubEvents
        {
            get { return events.events; }
        }
        #endregion SubEvents
        #endregion Accessors
        #region Methods
        #region CreateControls
        public PlaceHolder CreateControls(string pFamily, TradeActionType.TradeActionTypeEnum pTradeActionType,
            TradeActionMode.TradeActionModeEnum pTradeActionMode, int pCurrentIdE, DataDocumentContainer pDataDocumentContainer, FullConstructor pFullCtor)
        {
            PlaceHolder plh = new PlaceHolder();
            family = pFamily;
            tradeActionType = pTradeActionType;
            switch (family)
            {
                case Cst.ProductFamily_FX:
                    currentTradeAction = new FX_TradeAction(pCurrentIdE, pTradeActionType, pTradeActionMode, pDataDocumentContainer, events);
                    break;
                case Cst.ProductFamily_IRD:
                    currentTradeAction = new IRD_TradeAction(pCurrentIdE, pTradeActionType, pTradeActionMode, pDataDocumentContainer, events);
                    break;
                case Cst.ProductFamily_EQD:
                    currentTradeAction = new EQD_TradeAction(pCurrentIdE, pTradeActionType, pTradeActionMode, pDataDocumentContainer, events);
                    break;
                case Cst.ProductFamily_BO:
                    currentTradeAction = new BO_TradeAction(pCurrentIdE, pTradeActionType, pTradeActionMode, pDataDocumentContainer, events);
                    break;
            }

            if (null != currentTradeAction)
                plh.Controls.Add(events.CreateControls(pTradeActionType, pFullCtor, currentTradeAction));

            //currentTradeAction = new CurrentTradeAction(pCurrentIdE, pTradeActionType, pTradeActionMode, pEFSmLDocument, events);
            //plh.Controls.Add(events.CreateControls(pTradeActionType, pFullCtor, currentTradeAction));
            return plh;
        }
        #endregion CreateControls
        #region GetEvent
        public TradeActionEvent GetEvent(int pIdE)
        {
            return events.GetEvent(pIdE);
        }
        #endregion GetEvent
        #endregion Methods
    }
    #endregion TradeActionEvents
    #region TradeActionEventDetails
    /// <summary>
	/// Class where are stored by serialisation the lines of the table <b>EventDet</b> for a trade.
	/// </summary>
    public class TradeActionEventDetails : TradeActionEventDetailsBase
	{
		#region Methods
		#region AddCells_Static
		public override TableCell[] AddCells_Static
		{
			get
			{
				ArrayList aCell = new ArrayList();
				string currencies = string.Empty;
                if (quoteBasisSpecified)
                    currencies = (QuoteBasisEnum.Currency1PerCurrency2 == quoteBasis) ? idC1 + "/" + idC2 : idC2 + "/" + idC1;
                else if (sideRateBasisSpecified)
                {
                    if (SideRateBasisEnum.Currency1PerBaseCurrency == sideRateBasis)
                        currencies = idC1 + "/" + idCBase;
                    else if (SideRateBasisEnum.Currency2PerBaseCurrency == sideRateBasis)
                        currencies = idC2 + "/" + idCBase;
                    else if (SideRateBasisEnum.BaseCurrencyPerCurrency1 == sideRateBasis)
                        currencies = idCBase + "/" + idC1;
                    else if (SideRateBasisEnum.BaseCurrencyPerCurrency2 == sideRateBasis)
                        currencies = idCBase + "/" + idC2;
                }
                if (StrFunc.IsFilled(currencies))
                {
                    aCell.Add(TableTools.AddCell(currencies, HorizontalAlign.Center));
                    aCell.Add(TableTools.AddCell(spotRateSpecified ? spotRate.CultureValue : "0", HorizontalAlign.Right));
                }
				aCell.Add(TableTools.AddCell(rate.CultureValue, HorizontalAlign.Right));
				return (TableCell[])aCell.ToArray(typeof(TableCell));
			}
		}
		#endregion AddCells_Static
		#endregion Methods
    }
    #endregion TradeActionEventDetails

	#region ActionEvents
    public abstract class ActionEvents : ActionEventsBase 
	{
		#region Members
        protected TradeActionEventBase m_Event;
        protected TradeActionEventBase m_EventParent;
		protected IFxCashSettlement m_FxCashSettlement;

        public virtual DateTime ExpiryDate {set;get;}
        public virtual bool IsAmerican { set; get; }
        public virtual bool IsEuropean { set; get; }
        public virtual bool IsBermuda { set; get; }
        public virtual bool IsBondOption { set; get; }
        public virtual bool IsEquityOption { set; get; }
        public virtual bool IsFxAverageOption { set; get; }
        public virtual bool IsFxSimpleOption { set; get; }
        public virtual bool IsFxBarrierOption { set; get; }
        public virtual bool IsFxDigitalOption { set; get; }
        public virtual bool IsFxAverageStrikeOption { set; get; }
        public virtual bool IsGeometricAverage { set; get; }
        public virtual bool IsCappedCallOrFlooredPutOption { set; get; }
        public virtual bool IsFxCapBarrierOption { set; get; }
        public virtual bool IsFxFloorBarrierOption { set; get; }
        public virtual bool IsInTheMoney { set; get; }

        public virtual TradeActionEvent StartCallAmount { set; get; }
        public virtual PayerReceiverInfo CallAmount { set; get; }
        public virtual TradeActionEvent StartPutAmount { set; get; }
        public virtual PayerReceiverInfo PutAmount { set; get; }

        public virtual TradeActionEvent InitialEntitlement {set;get;}
        public virtual PayerReceiverInfo Entitlement { set; get; }
        public virtual TradeActionEvent InitialNbOptions { set; get; }
        public virtual PayerReceiverInfo NbOptions { set; get; }
        public virtual TradeActionEvent InitialNotional { set; get; }
        public virtual PayerReceiverInfo Notional { set; get; }

        public virtual string SettlementType { set; get; }
        public virtual IFxStrikePrice FxStrikePrice { set; get; }
        public virtual IOptionStrike OptionStrike { set; get; }
        public virtual IEquityStrike EquityStrike { set; get; }
        public virtual StrikeQuoteBasisEnum SettlementRateQuoteBasis { set; get; }
        public virtual EFS_Decimal StrikePrice { set; get; }
        public virtual IFxFixing FxFixing { set; get; }
        public virtual EFS_Decimal SettlementAmount { set; get; }
        public virtual string SettlementCurrency { set; get; }
        //public virtual EFS_Decimal SettlementRate { set; get; }
        public virtual Pair<Cst.PriceQuoteUnits,EFS_Decimal> SettlementRate { set; get; }
        public virtual StrikeQuoteBasisEnum QuoteBasis { set; get; }
        public virtual PayoutEnum PayoutStyle { set; get; }
        // EG 20150706 [21021] Nullable<int>
        //public virtual Pair<Pair<int, string>, Pair<int, string>> Buyer { set; get; }
        //public virtual Pair<Pair<int, string>, Pair<int, string>> Seller { set; get; }
        public virtual Pair<Pair<Nullable<int>, string>, Pair<Nullable<int>, string>> Buyer { set; get; }
        public virtual Pair<Pair<Nullable<int>, string>, Pair<Nullable<int>, string>> Seller { set; get; }

		#endregion Members
		#region Accessors
        public TradeActionEvent CurrentEvent
        {
            get { return (TradeActionEvent)m_Event; }
        }
        public TradeActionEvent CurrentEventParent
        {
            get { return (TradeActionEvent)m_EventParent; }
        }

        #region ActionEvent
        public override TradeActionEventBase ActionEvent
        {
            get { return m_Event; }
        }
        #endregion ActionEvent        
        #region BaseAmount
		public decimal BaseAmount
		{
            get { return (SettlementCurrency == CallAmount.Currency ? CallAmount.Amount : PutAmount.Amount); }
		}
		#endregion BaseAmount
        #region TradeAction
        protected override TradeActionBase TradeAction
        {
            get { return m_Event.CurrentTradeAction; }
        }
        #endregion TradeAction

        #region CurrentTradeAction2
        protected TradeAction CurrentTradeAction2
        {
            get { return (TradeAction)m_Event.CurrentTradeAction; }
        }
        #endregion CurrentTradeAction2

        #region FixingDate
        public string FixingDate
		{
			get
			{
				string fixingDate = Cst.HTMLSpace;
				if (DtFunc.IsDateTimeFilled(FxFixing.FixingDate.DateValue))
					fixingDate = DtFunc.DateTimeToString(FxFixing.FixingDate.DateValue, DtFunc.FmtShortDate);
				return fixingDate;
			}
		}
		#endregion FixingDate
		#region FixingDateTime
		public DateTime FixingDateTime
		{
			get
			{
				if ((null != FxFixing) && (null != FxFixing.FixingDate) && (null != FxFixing.FixingTime))
					return DtFunc.AddTimeToDate(FxFixing.FixingDate.DateValue, FxFixing.FixingTime.HourMinuteTime.TimeValue);
				else
					return Convert.ToDateTime(null);
			}
		}
        #endregion FixingDateTime

        #region FormatSettlementRate

        /// <summary>
        /// 
        /// </summary>
        /// EG 20150605 Initialisation data à 0 et Test existence SettlementRate
        /// FI 20151202 [21609] Modify
        public string FormatSettlementRate
        {
            get
            {
                string data = new EFS_Decimal(0).CultureValue;
                if ((null != SettlementRate) && (null != SettlementRate.Second))
                {
                    data = SettlementRate.Second.CultureValue;
                    // FI 20151202 [21609] Mise en commentaire (pas de % lorsque le prix est exprimé en ParValueDecimal même si c'est un pourcentage)
                    //if (PriceQuoteUnitsTools.IsPriceInParValueDecimal(SettlementRate.First))
                    //    data = FormatCell(new EFS_Decimal(SettlementRate.Second.DecValue * 100).CultureValue, EFSRegex.TypeRegex.RegexPercent);
                    
                }
                return data;
            }
        }
        #endregion FormatSettlementRate
        #region FormatStrikePrice
        public virtual string FormatStrikePrice
        {
            get
            {
                string data = StrikePrice.CultureValue;
                if ((null != OptionStrike) && OptionStrike.PercentageSpecified)
                    data = FormatCell(new EFS_Decimal(StrikePrice.DecValue * 100).CultureValue, EFSRegex.TypeRegex.RegexPercent);
                return data;
            }
        }
        #endregion FormatStrikePrice
        #region FormatEquityStrikePrice
        public virtual string FormatEquityStrikePrice
        {
            get
            {
                string data = StrikePrice.CultureValue;
                if ((null != EquityStrike) && EquityStrike.PercentageSpecified)
                    data = FormatCell(new EFS_Decimal(StrikePrice.DecValue * 100).CultureValue, EFSRegex.TypeRegex.RegexPercent);
                return data;
            }
        }
        #endregion FormatEquityStrikePrice

        #region IsCashSettlement
        public bool IsCashSettlement
		{
			get { return SettlementTypeEnum.Cash.ToString() == SettlementType; }
		}
		#endregion IsCashSettlement
		#region IsFxCashSettlement
		public bool IsFxCashSettlement
		{
			get { return null != m_FxCashSettlement; }
		}
		#endregion IsFxCashSettlement
        #region Resource in Form
        #region Label
        #region ResFormEntitlement
        protected virtual string ResFormEntitlement { get { return Ressource.GetString("Entitlement"); } }
        #endregion ResFormEntitlement
        #region ResFormExchangeRate
        protected virtual string ResFormExchangeRate { get { return Ressource.GetString("EXCHANGERATE"); } }
        #endregion ResFormExchangeRate
        #region ResFormExerciseType
        protected virtual string ResFormExerciseType { get { return Ressource.GetString("ExerciseType"); } }
        #endregion ResFormExerciseType
        #region ResFormFixingDate
        protected virtual string ResFormFixingDate { get { return Ressource.GetString("FixingDate"); } }
        #endregion ResFormFixingDate
        #region ResFormFixingTime
        protected virtual string ResFormFixingTime { get { return Ressource.GetString("FixingTime"); } }
        #endregion ResFormFixingTime
        #region ResFormNotional
        protected virtual string ResFormNotional { get { return Ressource.GetString("NOTIONALAMOUNT"); } }
        #endregion ResFormNotional
        #region ResFormNotionalReference
        protected virtual string ResFormNotionalReference { get { return Ressource.GetString("NotionalReference"); } }
        #endregion ResFormNotionalReference
        #region ResFormPaidBy
        protected virtual string ResFormPaidBy { get { return Ressource.GetString("PaidBy"); } }
        #endregion ResFormPaidBy
        #region ResFormPaymentDate
        protected virtual string ResFormPaymentDate { get { return Ressource.GetString("PaymentDate"); } }
        #endregion ResFormPaymentDate
        #region ResFormRate
        protected virtual string ResFormRate { get { return Ressource.GetString("Rate"); } }
        #endregion ResFormRate
        #region ResFormRateSource
        protected virtual string ResFormRateSource { get { return Ressource.GetString("RateSource"); } }
        #endregion ResFormRateSource
        #region ResFormSecondaryRateSource
        protected virtual string ResFormSecondaryRateSource { get { return Ressource.GetString("SecondaryRateSource"); } }
        #endregion ResFormSecondaryRateSource
        #region ResFormSettlementMethod
        protected virtual string ResFormSettlementMethod { get { return Ressource.GetString("SettlementMethod"); } }
        #endregion ResFormSettlementMethod
        #region ResFormSettlementRate
        protected virtual string ResFormSettlementRate { get { return Ressource.GetString("SETTLEMENTRATE"); } }
        #endregion ResFormSettlementRate
        #region ResFormStrike
        protected virtual string ResFormStrike { get { return Ressource.GetString("STRIKEPRICE"); } }
        #endregion ResFormStrike
        #region ResFormTouchRate
        protected virtual string ResFormTouchRate { get { return Ressource.GetString("TouchRate"); } }
        #endregion ResFormTouchRate
        #endregion Label        
        #region Title
        #region ResFormTitleCashSettlement
        protected virtual string ResFormTitleCashSettlement { get { return Ressource.GetString("CashSettlementInformation"); } }
        #endregion ResFormTitleCashSettlement
        #region ResFormTitleCurrencies
        protected virtual string ResFormTitleCurrencies { get { return Ressource.GetString("Currencies"); } }
        #endregion ResFormTitleCurrencies
        #region ResFormTitleCurrentNotionalAmounts
        protected virtual string ResFormTitleCurrentNotionalAmounts { get { return Ressource.GetString("CurrentNotionalAmounts"); } }
        #endregion ResFormTitleCurrentNotionalAmounts
        #region ResFormTitleExerciseDate
        protected virtual string ResFormTitleAbandonExerciseDate { get { return Ressource.GetString("ExerciseDate"); } }
        #endregion ResFormTitleExerciseDate
        #region ResFormTitleExerciseEvents
        protected virtual string ResFormTitleExerciseEvents { get { return Ressource.GetString("ExerciseEvents"); } }
        #endregion ResFormTitleExerciseEvents
        #region ResFormTitleFee
        protected virtual string ResFormTitleFee { get { return Ressource.GetString("Fee"); } }
        #endregion ResFormTitleFee
        #region ResFormTitlePhysicalSettlement
        protected virtual string ResFormTitlePhysicalSettlement { get { return Ressource.GetString("PhysicalSettlementInformation"); } }
        #endregion ResFormTitlePhysicalSettlement
        #region ResFormTitleRebate
        protected virtual string ResFormTitleRebate { get { return Ressource.GetString("Rebate"); } }
        #endregion ResFormTitleRebate
        #region ResFormTitleFeaturePayment
        protected virtual string ResFormTitleFeaturePayment { get { return Ressource.GetString("Payment"); } }
        #endregion ResFormTitleFeaturePayment
        #endregion Title
        #endregion Resource in Form
        #region StrikePriceValue
        protected virtual decimal StrikePriceValue
        {
            get
            {
                if ((null != FxStrikePrice) && (FxStrikePrice.StrikeQuoteBasis == QuoteBasis))
                    return StrikePrice.DecValue;
                else if (0 < StrikePrice.DecValue)
                {
                    return (1 / StrikePrice.DecValue);
                }
                else
                    return 0;
            }
        }
        #endregion StrikePriceValue

        #endregion Accessors
        #region Constructors
        public ActionEvents() { }
        public ActionEvents(TradeActionEventBase pEvent, TradeActionEventBase pEventParent)
            : base(pEvent)
        {
            m_Event = pEvent;
            m_EventParent = pEventParent;
            TradeAction.InitializeProduct(this);
        }
        #endregion Constructors
        #region Methods
        #region CalculAverageOptionInTheMoney
        private decimal CalculAverageOptionInTheMoney(PageBase pPage)
		{
			decimal callAmount = this.CallAmount.Amount;
			decimal putAmount = this.PutAmount.Amount;
            decimal settlementRate = this.SettlementRate.Second.DecValue;

			Control ctrl = pPage.FindControl("TXTCALLAMOUNT_" + idE);
			if (null != ctrl)
				callAmount = DecFunc.DecValue(((TextBox)ctrl).Text, Thread.CurrentThread.CurrentCulture);
			ctrl = pPage.FindControl("TXTPUTAMOUNT_" + idE);
			if (null != ctrl)
				putAmount = DecFunc.DecValue(((TextBox)ctrl).Text, Thread.CurrentThread.CurrentCulture);
			ctrl = pPage.FindControl("TXTSETTLEMENTRATE_" + idE);
			if (null != ctrl)
				settlementRate = DecFunc.DecValue(((TextBox)ctrl).Text, Thread.CurrentThread.CurrentCulture);
			ctrl = pPage.FindControl("TXTSTRIKEPRICE_" + idE);
			if (null != ctrl)
				StrikePrice.DecValue = DecFunc.DecValue(((TextBox)ctrl).Text, Thread.CurrentThread.CurrentCulture);
			IsInTheMoney = m_EventParent.IsInTheMoney(SettlementFxRateValue(settlementRate), StrikePriceValue, QuoteBasis);
			//
			return CalculFormulaOptionInTheMoney(SessionTools.CS, callAmount, putAmount, SettlementFxRateValue(settlementRate), StrikePriceValue);
		}
		#endregion CalculAverageOptionInTheMoney
		#region CalculCashSettlementMoneyAmount
		protected void CalculCashSettlementMoneyAmount(PageBase pPage, string pControlId)
		{
			FormatControl(pPage, pControlId);

			decimal settlementAmount = 0;
			if (EventCodeFunc.IsSimpleOption(m_EventParent.eventCode) || EventCodeFunc.IsBarrierOption(m_EventParent.eventCode))
				settlementAmount = CalculRegularOptionInTheMoney(pPage);
			else if (EventCodeFunc.IsAverageRateOption(m_EventParent.eventCode))
				settlementAmount = CalculAverageOptionInTheMoney(pPage);

			Control ctrl = pPage.FindControl("TXTSETTLEMENTAMOUNT_" + idE);
			if (null != ctrl)
				((TextBox)ctrl).Text = StrFunc.FmtDecimalToCurrentCulture(settlementAmount.ToString(NumberFormatInfo.InvariantInfo));

			ctrl = pPage.FindControl("LBLTITLE_" + idE);
			if (null != ctrl)
			{
				string title = (IsCashSettlement ? ResFormTitleCashSettlement : ResFormTitlePhysicalSettlement);
				if (IsInTheMoney)
					title += @" <span style=""color:green"">[In-the money]</span>";
				else
					title += @" <b><span style=""color:white"">[Out-of-the money]</span></b>";
				if (ctrl.GetType().Equals(typeof(Label)))
					((Label)ctrl).Text = title;
				else if (ctrl.GetType().Equals(typeof(TableCell)))
					((TableCell)ctrl).Text = title;
			}
		}
		#endregion CalculCashSettlementMoneyAmount
        #region CalculRegularOptionInTheMoney
        private decimal CalculRegularOptionInTheMoney(PageBase pPage)
        {
            decimal callAmount = this.CallAmount.Amount;
            decimal putAmount = this.PutAmount.Amount;
            decimal settlementRate = this.SettlementRate.Second.DecValue;

            Control ctrl = pPage.FindControl("TXTCALLAMOUNT_" + idE);
            if (null != ctrl)
                callAmount = DecFunc.DecValue(((TextBox)ctrl).Text, Thread.CurrentThread.CurrentCulture);
            ctrl = pPage.FindControl("TXTPUTAMOUNT_" + idE);
            if (null != ctrl)
                putAmount = DecFunc.DecValue(((TextBox)ctrl).Text, Thread.CurrentThread.CurrentCulture);
            ctrl = pPage.FindControl("TXTSETTLEMENTRATE_" + idE);
            if (null != ctrl)
                settlementRate = DecFunc.DecValue(((TextBox)ctrl).Text, Thread.CurrentThread.CurrentCulture);
            IsInTheMoney = m_EventParent.IsInTheMoney(SettlementFxRateValue(settlementRate), StrikePriceValue, QuoteBasis);
            return CalculFormulaOptionInTheMoney(SessionTools.CS, callAmount, putAmount, SettlementFxRateValue(settlementRate), StrikePriceValue);
        }
        #endregion CalculRegularOptionInTheMoney
        #region CalculFormulaOptionInTheMoney
        protected decimal CalculFormulaOptionInTheMoney(string pCs, decimal pCallAmount, decimal pPutAmount, decimal pSettlementRate, decimal pStrikePrice)
        {
            decimal amount = 0;
            if (0 < pSettlementRate)
            {
                if (SettlementCurrency == CallAmount.Currency)
                    amount = System.Math.Max(0, pCallAmount * ((pSettlementRate - pStrikePrice) / pSettlementRate));
                else
                    amount = System.Math.Max(0, pPutAmount * ((pStrikePrice - pSettlementRate) / pSettlementRate));

                EFS_Cash cash = new EFS_Cash(pCs, amount, SettlementCurrency);
                amount = cash.AmountRounded;
            }
            return amount;
        }
        #endregion CalculFormulaOptionInTheMoney
		#region SettlementRate
		/// <summary>
		/// 
		/// </summary>
		/// <param name="pSettlementRate"></param>
		/// <returns></returns>
		protected virtual decimal SettlementFxRateValue(decimal pSettlementRate)
		{
			decimal settlementRate = 0;
			string currency1 = FxFixing.QuotedCurrencyPair.Currency1;
			string currency2 = FxFixing.QuotedCurrencyPair.Currency2;

			if (CallAmount.Currency == currency1 && PutAmount.Currency == currency2)
			{
				if ((QuoteBasisEnum.Currency1PerCurrency2 == FxFixing.QuotedCurrencyPair.QuoteBasis) &&
					(StrikeQuoteBasisEnum.CallCurrencyPerPutCurrency == QuoteBasis))
					settlementRate = pSettlementRate;
				else if ((QuoteBasisEnum.Currency2PerCurrency1 == FxFixing.QuotedCurrencyPair.QuoteBasis) &&
					(StrikeQuoteBasisEnum.PutCurrencyPerCallCurrency == QuoteBasis))
					settlementRate = pSettlementRate;
				else if (0 < pSettlementRate)
					settlementRate = (1 / pSettlementRate);
			}
			else if (CallAmount.Currency == currency2 && PutAmount.Currency == currency1)
			{
				if ((QuoteBasisEnum.Currency1PerCurrency2 == FxFixing.QuotedCurrencyPair.QuoteBasis) &&
					(StrikeQuoteBasisEnum.PutCurrencyPerCallCurrency == QuoteBasis))
					settlementRate = pSettlementRate;
				else if ((QuoteBasisEnum.Currency2PerCurrency1 == FxFixing.QuotedCurrencyPair.QuoteBasis) &&
					(StrikeQuoteBasisEnum.CallCurrencyPerPutCurrency == QuoteBasis))
					settlementRate = pSettlementRate;
				else if (0 < pSettlementRate)
					settlementRate = (1 / pSettlementRate);
			}
			else // Devise de cash settlement differente
			{


			}
            return settlementRate;
		}
		#endregion SettlementRate
		#endregion Methods

        #region Clone
        // EG 20160308 Migration vs2013
        //public virtual object Clone()
        //{
        //    return null;
        //}
        #endregion Clone

    }
    #endregion ActionEvents

    #region AbandonAndExerciseEventsBase
    /// <summary>
    /// Classe regroupant les données communes Abandon|Exercise sur FX|BO|EQD option
    /// </summary>
    public abstract class AbandonAndExerciseEventsBase : ActionEvents
    {
        #region Members
        public string eventCodeParent;
        public string abandonExerciseType;
        public bool isAutomaticExercise;
        public bool isFallbackExercise;

        public Dictionary<string,string> lstExerciseType;
        public Dictionary<string, string> lstExerciseDate;

        public TradeActionEvent[] exerciseDates;
        #endregion Members
        #region Accessors
        #region AddCells_Capture
        public virtual TableCell[] AddCells_Capture
        {
            get
            {
                ArrayList aCell = new ArrayList
                {
                    TableTools.AddCell(ActionDate, HorizontalAlign.Center, 80, UnitEnum.Pixel),
                    TableTools.AddCell(actionTime.Value, HorizontalAlign.Center, 60, UnitEnum.Pixel),
                    TableTools.AddCell(FormatedValueDate, HorizontalAlign.Center, 80, UnitEnum.Pixel)
                };
                return (TableCell[])aCell.ToArray(typeof(TableCell));
            }
        }
        #endregion AddCells_Capture
        #region CreateHeaderCells_Capture
        public virtual TableHeaderCell[] CreateHeaderCells_Capture
        {
            get
            {
                ArrayList aHeaderCell = new ArrayList
                {
                    TableTools.AddHeaderCell(ResFormTitleAbandonExerciseDate, false, 0, UnitEnum.Pixel, 2, false),
                    TableTools.AddHeaderCell(ResFormValueDate, false, 0, UnitEnum.Pixel, 0, false)
                };
                return (TableHeaderCell[])aHeaderCell.ToArray(typeof(TableHeaderCell));
            }
        }
        #endregion CreateHeaderCells_Capture
        #region CreateHeaderCells_CaptureAbandon
        protected virtual TableHeaderCell[] CreateHeaderCells_CaptureAbandon
        {
            get
            {
                ArrayList aHeaderCell = new ArrayList
                {
                    TableTools.AddHeaderCell(ResFormAmount, false, 0, UnitEnum.Pixel, 0, false),
                    TableTools.AddHeaderCell(ResFormTitleNoteEvents, false, 0, UnitEnum.Pixel, 2, false)
                };
                return (TableHeaderCell[])aHeaderCell.ToArray(typeof(TableHeaderCell));
            }
        }
        #endregion CreateHeaderCells_CaptureAbandon
        #region CreateHeaderCells_Static
        protected virtual TableHeaderCell[] CreateHeaderCells_Static
        {
            get
            {
                ArrayList aHeaderCell = new ArrayList
                {
                    TableTools.AddHeaderCell(ResFormTitleEventCode, false, 0, UnitEnum.Pixel, 2, false)
                };
                return (TableHeaderCell[])aHeaderCell.ToArray(typeof(TableHeaderCell));
            }
        }
        #endregion CreateHeaderCells_Static

        #endregion Accessors
        #region Constructors
        public AbandonAndExerciseEventsBase() { }
        public AbandonAndExerciseEventsBase(TradeActionEventBase pEvent, TradeActionEventBase pEventParent)
            : base(pEvent, pEventParent)
        {
            eventCodeParent = pEventParent.eventCode;
            abandonExerciseType = pEvent.eventType;
            isAutomaticExercise = TradeAction.IsAutomaticExercise;
            isFallbackExercise = TradeAction.IsFallBackExercise;
            exerciseDates = CurrentEventParent.EventExerciseDates;

            lstExerciseDate = new Dictionary<string, string>();
            if (null != exerciseDates)
            {
                foreach (TradeActionEvent events in exerciseDates)
                {
                    if (null != events.eventClass)
                    {
                        foreach (EventClass item in events.eventClass)
                        {
                            EFS_Date dateExercise = new EFS_Date
                            {
                                DateValue = item.dtEvent.DateValue
                            };
                            lstExerciseDate.Add(DtFunc.DateTimeToStringDateISO(dateExercise.DateValue),DtFunc.DateTimeToString(dateExercise.DateValue, DtFunc.FmtShortDate));
                        }
                    }
                }
            }
        }
        #endregion Constructors
        #region Members
        #region AddCells_Static
        public virtual TableCell[] AddCells_Static(TradeActionEvent pEvent, TradeActionEvent pEventParent)
        {
            ArrayList aCell = new ArrayList
            {
                TableTools.AddCell(pEvent.eventCode, HorizontalAlign.Center, true),
                TableTools.AddCell(pEvent.eventType, HorizontalAlign.Center, true)
            };
            aCell.AddRange(AddCells_Capture);
            return (TableCell[])aCell.ToArray(typeof(TableCell));
        }
        #endregion AddCells_Static
        #region CreateTableHeader
        public virtual ArrayList CreateTableHeader(TradeActionEvent pEvent)
        {
            ArrayList aTableHeader = new ArrayList
            {
                pEvent.currentTradeAction.GetEventCodeTitle(pEvent.eventCode),
                CreateHeaderCells_Static,
                CreateHeaderCells_Capture,
                ResFormTitleComplementary
            };
            return aTableHeader;
        }
        #endregion CreateTableHeader


        #region IsAutomaticExercise
        public virtual bool IsAutomaticExercise()
        {
            return false;
        }
        #endregion IsAutomaticExercise
        #region SetExerciseTypeList
        protected void SetExerciseTypeList(bool pIsTotalExerciseAuthorized)//TradeActionEvent pEvent, TradeActionEvent pEventParent)
        {
            // FI 20240731 [XXXXX] Mise en commentaire => use DataEnabledEnum/DataEnabledEnumHelper
            //ExtendEnum extendEnums = ExtendEnumsTools.ListEnumsSchemes["ExerciseTypeEnum"];
            ExtendEnum extendEnums = DataEnabledEnumHelper.GetDataEnum(SessionTools.CS, "ExerciseTypeEnum");
            if ((null != extendEnums) && (null != extendEnums.item))
            {
                lstExerciseType = new Dictionary<string, string>();
                foreach (ExtendEnumValue extendEnumValue in extendEnums.Sort("ExtValue"))
                {
                    if (null != extendEnumValue)
                    {
                        if (EventTypeFunc.IsPartiel(extendEnumValue.Value) && EventCodeFunc.IsEuropeanOption(eventCodeParent))
                            lstExerciseType.Add(extendEnumValue.Value, extendEnumValue.ExtValue);
                        else if (EventTypeFunc.IsMultiple(extendEnumValue.Value) && (false == EventCodeFunc.IsEuropeanOption(eventCodeParent)))
                            lstExerciseType.Add(extendEnumValue.Value, extendEnumValue.ExtValue);
                        else if (EventTypeFunc.IsTotal(extendEnumValue.Value) && pIsTotalExerciseAuthorized)
                            lstExerciseType.Add(extendEnumValue.Value, extendEnumValue.ExtValue);
                    }
                }
            }
        }
        #endregion SetExerciseTypeList

        #region IsEventChanged
        public override bool IsEventChanged(TradeActionEventBase pEvent)
        {
            bool isEventChanged = base.IsEventChanged(pEvent);
            if (false == isEventChanged)
            {
                // TODO
                isEventChanged = true;
            }
            return isEventChanged;
        }
        #endregion IsEventChanged

        #region SetAmountAndQuantityExercise
        // EG 20150429 [20513]
        protected PayerReceiverInfo SetAmountAndQuantityExercise(string pEventType, TradeActionEvent pEvent_INI)
        {
            return SetAmountAndQuantityExercise(pEventType, pEvent_INI, null, null);
        }
        // EG 20150429 [20513]
        protected PayerReceiverInfo SetAmountAndQuantityExercise(string pEventType, TradeActionEvent pEvent_INI, TradeActionEvent[] pEvents_INT, TradeActionEvent pEvent_TER)
        {
            PayerReceiverInfo exerciseInfo = null;
            decimal value_INT = 0;
            decimal value_TER = 0;
            if (ArrFunc.IsFilled(pEvents_INT))
            {
                foreach (TradeActionEvent item in pEvents_INT)
                    value_INT += item.valorisation.DecValue;
            }
            if (null != pEvent_TER)
                value_TER += pEvent_TER.valorisation.DecValue;

            if (EventTypeFunc.IsQuantity(pEventType) || EventTypeFunc.IsUnderlyer(pEventType) || EventTypeFunc.IsSingleUnderlyer(pEventType))
            {
                // EG 20150706 [21021]
                //exerciseInfo = new QuantityPayerReceiverInfo(pEvent_INI.valorisation.IntValue - Convert.ToInt32(value_INT + value_TER),
                //    pEvent_INI.idPayer, pEvent_INI.payer, pEvent_INI.idPayerBook, pEvent_INI.payerBook,
                //    pEvent_INI.idReceiver, pEvent_INI.receiver, pEvent_INI.idReceiverBook, pEvent_INI.receiverBook);
                exerciseInfo = new QuantityPayerReceiverInfo(pEvent_INI.valorisation.IntValue - Convert.ToInt32(value_INT + value_TER),
                    pEvent_INI.PayerInfo, pEvent_INI.ReceiverInfo);
            }
            else if (EventTypeFunc.IsNominal(pEventType) || EventTypeFunc.IsBondPayment(pEventType) ||
                EventTypeFunc.IsSettlementCurrency(pEventType) || EventTypeFunc.IsCallPutCurrency(pEventType))
            {
                // EG 20150706 [21021]
                //exerciseInfo = new AmountPayerReceiverInfo(null, pEvent_INI.valorisation.DecValue - value_INT - value_TER, pEvent_INI.unit,
                //    pEvent_INI.idPayer, pEvent_INI.payer, pEvent_INI.idPayerBook, pEvent_INI.payerBook,
                //    pEvent_INI.idReceiver, pEvent_INI.receiver, pEvent_INI.idReceiverBook, pEvent_INI.receiverBook);
                exerciseInfo = new AmountPayerReceiverInfo(null, pEvent_INI.valorisation.DecValue - value_INT - value_TER, pEvent_INI.unit,
                    pEvent_INI.PayerInfo, pEvent_INI.ReceiverInfo);
            }
            return exerciseInfo;

        }
        #endregion SetAmountAndQuantityExercise
        #region Event_Exercise
        protected TradeActionEvent EventExercise(string pEventType)
        {
            TradeActionEvent @event = null;
            TradeActionEvent[] events = EventExercise(EventCodeFunc.Termination, pEventType);
            if (ArrFunc.IsFilled(events))
                @event = events[0];
            return @event;
        }
        protected TradeActionEvent[] EventExercise(string pEventCode, string pEventType)
        {
            TradeActionEvent[] eventExercises = CurrentEventParent.EventExercise;
            if (null != eventExercises)
            {
                List<TradeActionEvent> lstEvent = new List<TradeActionEvent>();
                foreach (TradeActionEvent eventExercise in eventExercises)
                {
                    TradeActionEvent[] events = eventExercise.GetEvent_EventType(pEventType);
                    if (null != events)
                    {
                        foreach (TradeActionEvent @event in events)
                        {
                            if (pEventCode == @event.eventCode)
                                lstEvent.Add(@event);
                        }
                    }
                }
                if (0 < lstEvent.Count)
                    return (TradeActionEvent[])lstEvent.ToArray();
            }
            return null;
        }
        #endregion Event_Exercise

        #endregion Members
    }
    #endregion AbandonAndExerciseEventsBase


    #region InformationEvents
    public class InformationEvents : ActionEvents
	{
		#region Accessors
		#region CreateHeaderCells_Static
		private TableHeaderCell[] CreateHeaderCells_Static
		{
			get
			{
                ArrayList aHeaderCell = new ArrayList
                {
                    TableTools.AddHeaderCell(ResFormTitleEventCode, false, 0, UnitEnum.Pixel, 2, false),
                    TableTools.AddHeaderCell(ResFormTitlePeriod, false, 150, UnitEnum.Pixel, 2, false)
                };
                return (TableHeaderCell[])aHeaderCell.ToArray(typeof(TableHeaderCell));
			}
		}
		#endregion CreateHeaderCells_Static
		#endregion Accessors
		#region Constructors
        public InformationEvents(TradeActionEventBase pEvent, TradeActionEventBase pEventParent) : base(pEvent, pEventParent) { }
		#endregion Constructors
		#region Methods
		#region AddCells_Static
		public static TableCell[] AddCells_Static(TradeActionEvent pEvent, TradeActionEvent pEventParent)
		{
			ArrayList aCell = new ArrayList();
			aCell.AddRange(pEvent.NewCells_Static());
			return (TableCell[])aCell.ToArray(typeof(TableCell));
		}
		#endregion AddCells_Static
		#region CreateTableHeader
		public ArrayList CreateTableHeader(TradeActionEvent pEvent)
		{
            ArrayList aTableHeader = new ArrayList
            {
                pEvent.CurrentTradeAction.GetEventCodeTitle(pEvent.eventCode),
                CreateHeaderCells_Static
            };
            return aTableHeader;
		}
		#endregion CreateTableHeader
		#endregion Methods
	}
	#endregion InformationEvents
	#region NominalStepEvents
	public class NominalStepEvents : ActionEvents
	{
		#region Members
		public TradeActionEvent nominalVariationEvent;
		public bool isNominalVariationEventProvision;
		#endregion Members
		#region Accessors
		#region CreateHeaderCells_Static
		private TableHeaderCell[] CreateHeaderCells_Static
		{
			get
			{
                ArrayList aHeaderCell = new ArrayList
                {
                    TableTools.AddHeaderCell(ResFormTitleEventCode, false, 0, UnitEnum.Pixel, 2, false),
                    TableTools.AddHeaderCell(ResFormTitlePeriod, false, 150, UnitEnum.Pixel, 2, false),
                    TableTools.AddHeaderCell(ResFormCurrentNotionalAmounts, false, 200, UnitEnum.Pixel, 2, false),
                    TableTools.AddHeaderCell(ResFormVaryingNotional, false, 200, UnitEnum.Pixel, 2, false),
                    TableTools.AddHeaderCell(ResFormTitleNoteEvents, false, 100, UnitEnum.Percentage, 2, false)
                };
                return (TableHeaderCell[])aHeaderCell.ToArray(typeof(TableHeaderCell));
			}
		}
		#endregion CreateHeaderCells_Static

		#region Resource in Form
		#region ResFormCurrentNotionalAmounts
		protected virtual string ResFormCurrentNotionalAmounts { get { return Ressource.GetString("CurrentNotionalAmounts"); } }
		#endregion ResFormCurrentNotionalAmounts
		#region ResFormVaryingNotional
		protected virtual string ResFormVaryingNotional { get { return Ressource.GetString("VaryingNotional"); } }
		#endregion ResFormVaryingNotional

		#endregion Resource in Form

		#endregion Accessors
		#region Constructors
        public NominalStepEvents(TradeActionEventBase pEvent, TradeActionEventBase pEventParent)
			: base(pEvent, pEventParent)
		{
            nominalVariationEvent = CurrentEventParent.EventVariationNominal(pEvent);
			if (null != nominalVariationEvent)
				isNominalVariationEventProvision = (null != nominalVariationEvent.EventClassExerciseProvision);
		}
		#endregion Constructors
		#region Methods
		#region AddCells_Static
		public TableCell[] AddCells_Static(TradeActionEvent pEvent, TradeActionEvent pEventParent)
		{
			string note = string.Empty;
			ArrayList aCell = new ArrayList();
			aCell.AddRange(pEvent.NewCells_Static());
			aCell.Add(TableTools.AddCell(pEvent.unit, HorizontalAlign.Center));
			aCell.Add(TableTools.AddCell(StrFunc.FmtDecimalToCurrentCulture(pEvent.valorisation.DecValue), HorizontalAlign.Right));
			if (null != nominalVariationEvent)
			{
				aCell.Add(TableTools.AddCell(nominalVariationEvent.unit, HorizontalAlign.Center));
				aCell.Add(TableTools.AddCell(StrFunc.FmtDecimalToCurrentCulture(nominalVariationEvent.valorisation.DecValue), HorizontalAlign.Right));
			}
			else
			{
				aCell.Add(TableTools.AddCell(Cst.HTMLSpace, HorizontalAlign.Center, 0, UnitEnum.Pixel));
				aCell.Add(TableTools.AddCell(Cst.HTMLSpace, HorizontalAlign.Right, 0, UnitEnum.Pixel));
			}
			if (pEvent.detailsSpecified && pEvent.details.noteSpecified)
				note = pEvent.details.note;
			aCell.Add(TableTools.AddCell(note, HorizontalAlign.NotSet, 100, UnitEnum.Percentage, true, false, false));
			aCell.Add(TableTools.AddCell(Cst.HTMLSpace, HorizontalAlign.Center, 0, UnitEnum.Pixel));

			return (TableCell[])aCell.ToArray(typeof(TableCell));
		}
		#endregion AddCells_Static
		#region CreateTableHeader
		public ArrayList CreateTableHeader(TradeActionEvent pEvent)
		{
            ArrayList aTableHeader = new ArrayList
            {
                pEvent.CurrentTradeAction.GetEventCodeTitle(pEvent.eventCode),
                CreateHeaderCells_Static
            };
            return aTableHeader;
		}
		#endregion CreateTableHeader
		#endregion Methods
	}
	#endregion NominalStepEvents
    #region RemoveTradeEvents
    public class RemoveTradeEvents : ActionEvents
    {
        #region Accessors
        #region CreateHeaderCells_Capture
        private TableHeaderCell[] CreateHeaderCells_Capture
        {
            get
            {
                ArrayList aHeaderCell = new ArrayList
                {
                    TableTools.AddHeaderCell(ResFormActionDate, false, 0, UnitEnum.Pixel, 0, false),
                    TableTools.AddHeaderCell(ResFormTitleNoteEvents, false, 0, UnitEnum.Pixel, 2, false)
                };
                return (TableHeaderCell[])aHeaderCell.ToArray(typeof(TableHeaderCell));
            }
        }
        #endregion CreateHeaderCells_Capture
        #region CreateHeaderCells_Static
        private TableHeaderCell[] CreateHeaderCells_Static
        {
            get
            {
                ArrayList aHeaderCell = new ArrayList
                {
                    TableTools.AddHeaderCell(ResFormTitleEventCode, false, 0, UnitEnum.Pixel, 2, false),
                    TableTools.AddHeaderCell(ResFormTitlePeriod, false, 150, UnitEnum.Pixel, 2, false)
                };
                return (TableHeaderCell[])aHeaderCell.ToArray(typeof(TableHeaderCell));
            }
        }
        #endregion CreateHeaderCells_Static
        #endregion Accessors
        #region Constructors
        public RemoveTradeEvents(string pCS, TradeActionEventBase pEvent, TradeActionEventBase pEventParent)
            : base(pEvent, pEventParent)
        {
            if (null != m_Event.EventClassGroupLevel)
                actionDate.DateValue = m_Event.EventClassGroupLevel.dtEvent.DateValue;
            else
            {
                DateTime tradeDate = TradeAction.TradeDate;
                DateTime removeMinDate = OTCmlHelper.GetDateBusiness(pCS);
                DateTime removeMaxDate = m_Event.dtEndPeriod.DateValue;

                if (0 < DateTime.Compare(tradeDate, removeMinDate))
                    removeMinDate = tradeDate;

                if (0 < DateTime.Compare(removeMinDate, removeMaxDate))
                    actionDate.DateValue = removeMaxDate;
                else
                    actionDate.DateValue = removeMinDate;
            }
        }
        #endregion Constructors
        #region Methods
        #region AddCells_Capture
        private TableCell[] AddCells_Capture(TradeActionEvent pEventParent)
        {
            ArrayList aCell = new ArrayList();
            m_EventParent = pEventParent;
            aCell.Add(TableTools.AddCell(ActionDate, HorizontalAlign.Center, 80, UnitEnum.Pixel));
            aCell.Add(TableTools.AddCell(note, HorizontalAlign.NotSet, 100, UnitEnum.Percentage, true, false, false));
            aCell.Add(TableTools.AddCell(Cst.HTMLSpace, HorizontalAlign.Center, 0, UnitEnum.Pixel));
            return (TableCell[])aCell.ToArray(typeof(TableCell));
        }
        #endregion AddCells_Capture
        #region AddCells_Static
        public TableCell[] AddCells_Static(TradeActionEvent pEvent, TradeActionEvent pEventParent)
        {
            ArrayList aCell = new ArrayList();
            aCell.AddRange(pEvent.NewCells_Static());
            aCell.AddRange(AddCells_Capture(pEventParent));
            return (TableCell[])aCell.ToArray(typeof(TableCell));
        }
        #endregion AddCells_Static
        #region CreateControlCurrentAction
        public TableRow[] CreateControlCurrentAction
        {
            get
            {
                ArrayList aTableRow = new ArrayList();
                #region ActionDate
                aTableRow.Add(base.CreateControlActionDate());
                #endregion ActionDate
                #region Description
                aTableRow.AddRange(base.CreateControlDescription);
                #endregion Description
                return (TableRow[])aTableRow.ToArray(typeof(TableRow));
            }
        }
        #endregion CreateControlCurrentAction
        #region CreateTableHeader
        public ArrayList CreateTableHeader(TradeActionEvent pEvent)
        {
            ArrayList aTableHeader = new ArrayList
            {
                TradeAction.GetEventCodeTitle(pEvent.eventCode),
                CreateHeaderCells_Static,
                CreateHeaderCells_Capture,
                ResFormTitleComplementary
            };
            return aTableHeader;
        }
        #endregion CreateTableHeader
        #region PostedAction
        public object PostedAction(string pKeyAction)
        {
            string keyAction = pKeyAction + Convert.ToInt32(TradeActionCode.TradeActionCodeEnum.RemoveTrade) + "_" + m_Event.eventCode;
            return new RemoveTradeEventMsg(System.Math.Abs(idE), ActionDateTime, note, keyAction);
        }
        #endregion PostedAction
        #region Save
        public override bool Save(Page pPage)
        {
            bool isOk = ValidationRules(pPage);
            if (isOk)
                isOk = base.Save(pPage);
            m_Event.isValidated = isOk;
            return isOk;
        }
        #endregion Save
        #region ValidationRules
        public override bool ValidationRules(Page pPage)
        {
            bool isOk = true;
            m_Event.validationRulesMessages = new ArrayList();
            DateTime removeDate = actionDate.DateValue;
            DateTime tradeDate = TradeAction.TradeDate;
            DateTime removeMinDate = OTCmlHelper.GetDateBusiness(SessionTools.CS);
            DateTime removeMaxDate = m_Event.dtEndPeriod.DateValue;
            if (0 < DateTime.Compare(tradeDate, removeMinDate))
                removeMinDate = tradeDate;

            if ((0 <= DateTime.Compare(removeDate, removeMinDate)) &&
                (0 <= DateTime.Compare(removeMaxDate, removeDate)))
            {
                isOk = false;
                m_Event.validationRulesMessages.Add("Msg_IncorrectRemoveDate");
            }
            if (isOk)
                isOk = base.ValidationRules(pPage);
            return isOk;
        }
        #endregion ValidationRules
        #endregion Methods
    }
    #endregion RemoveTradeEvents
}