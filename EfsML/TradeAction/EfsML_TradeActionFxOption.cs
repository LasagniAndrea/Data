#region Debug Directives
//      -------------------------------------------------------------------------------------------------------------------------
//      EfsML_TradeActionFxOption : Contient l'ensemble des classe utilisées par les actions diverses liées aux options de change
//      -------------------------------------------------------------------------------------------------------------------------
//      * CurrentTradeAction           : Cette classe est partialisée, stocke l'événement de base en cours de traitement
//                                       vous y trouvez ici les Accesseurs/méthodes propres aux options de change
//      * TradeActionEvent             : Cette classe est partialisée, vous y trouvez ici les Accesseurs/méthodes propres 
//                                       aux options de change
//      Classes Action
//      * AbandonEvents                : Abandon option de change
//      * AbandonAndExerciseEvents     : classe commune à AbandonEvents et ExerciseEvents
//      * BarrierAndTriggerEvents      : Classe commune à BarrierEvents et TriggerEvents
//      * BarrierEvents                : Déclenchement de barrières
//      * CalculationAgentSettlementRateEvents : Fixing déterminé par un Agent de Calcul (anciennement: Fixing clientèle à l'échéance)
//      * ExerciseEvents               : Exercice
//      * PayoutEvents                 : Déclenchement de payout
//      * RebateEvents                 : Déclenchement de rebate
//      * TriggerEvents                : Déclenchement de barrières
using EFS.ACommon;
using EFS.Actor;
using EFS.ApplicationBlocks.Data;
using EFS.Common;
using EFS.Common.MQueue;
using EFS.Common.Web;
using EFS.GUI.Attributes;
using EFS.GUI.Interface;
using EFS.GUI.SimpleControls;
using EfsML.Business;
using EfsML.Enum;
using EfsML.Enum.Tools;
using EfsML.Interface;
using FpML.Enum;
using FpML.Interface;
using System;
using System.Collections;
using System.Globalization;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Web.UI;
using System.Web.UI.WebControls;
#endregion Using Directives

namespace EfsML
{
    #region FX_TradeAction
    /// <summary>Used by the trade action process </summary>
    public class FX_TradeAction : TradeAction
    {
        #region Members
        public IFxDigitalOption fxDigitalOption;
        public IFxAverageRateOption fxAverageRateOption;
        public IFxBarrierOption fxBarrierOption;
        public IFxOptionLeg fxOptionLeg;
        #endregion Members
        #region Constructors
        public FX_TradeAction() : base() { }
        public FX_TradeAction(int pCurrentIdE, TradeActionType.TradeActionTypeEnum pTradeActionType, TradeActionMode.TradeActionModeEnum pTradeActionMode,
            DataDocumentContainer pDataDocumentContainer, TradeActionEvent pEvents)
            : base(pCurrentIdE, pTradeActionType, pTradeActionMode, pDataDocumentContainer, pEvents) 
        { 
            if (dataDocumentContainer.CurrentProduct.Product.ProductBase.IsFxAverageRateOption)
                fxAverageRateOption = dataDocumentContainer.CurrentProduct.Product as IFxAverageRateOption;
            else if (dataDocumentContainer.CurrentProduct.Product.ProductBase.IsFxBarrierOption)
                fxBarrierOption = dataDocumentContainer.CurrentProduct.Product as IFxBarrierOption;
            else if (dataDocumentContainer.CurrentProduct.Product.ProductBase.IsFxOptionLeg)
                fxOptionLeg = dataDocumentContainer.CurrentProduct.Product as IFxOptionLeg;
            else if (dataDocumentContainer.CurrentProduct.Product.ProductBase.IsFxDigitalOption)
                fxDigitalOption = dataDocumentContainer.CurrentProduct.Product as IFxDigitalOption;
        }
        #endregion Constructors
        #region Override Accessors
        // EG 20150428 [20513] New
        public override bool IsAutomaticExercise
        {
            get
            {
                bool isAutomaticExercise = false;
                if (dataDocumentContainer.CurrentProduct.Product.ProductBase.IsFxAverageRateOption)
                    isAutomaticExercise = fxAverageRateOption.ProcedureSpecified && fxAverageRateOption.Procedure.ExerciseProcedureAutomaticSpecified;
                else if (dataDocumentContainer.CurrentProduct.Product.ProductBase.IsFxBarrierOption)
                    isAutomaticExercise = fxBarrierOption.ProcedureSpecified && fxBarrierOption.Procedure.ExerciseProcedureAutomaticSpecified;
                else if (dataDocumentContainer.CurrentProduct.Product.ProductBase.IsFxOptionLeg)
                    isAutomaticExercise = fxOptionLeg.ProcedureSpecified && fxOptionLeg.Procedure.ExerciseProcedureAutomaticSpecified;
                return isAutomaticExercise;
            }
        }
        // EG 20150428 [20513] New
        public override string SettlementCurrency
        {
            get
            {
                string settlementCurrency = string.Empty;
                if (dataDocumentContainer.CurrentProduct.Product.ProductBase.IsFxAverageRateOption)
                    settlementCurrency = fxAverageRateOption.PayoutCurrency.Value;
                else
                {
                    IFxCashSettlement fxCashSettlement = FxCashSettlement();
                    if (null != fxCashSettlement)
                        settlementCurrency = fxCashSettlement.SettlementCurrency.Value;
                }
                return settlementCurrency;
            }
        }
        // EG 20150428 [20513] New
        public override SettlementTypeEnum SettlementType
        {
            get
            {
                SettlementTypeEnum settlementType = SettlementTypeEnum.Physical;
                if (dataDocumentContainer.CurrentProduct.Product.ProductBase.IsFxAverageRateOption)
                {
                    if (fxAverageRateOption.AverageStrikeOptionSpecified)
                        settlementType = fxAverageRateOption.AverageStrikeOption.SettlementType;
                    else
                        settlementType = SettlementTypeEnum.Cash;

                }
                else
                {
                    IFxCashSettlement cashSettlement = FxCashSettlement();
                    if (null != cashSettlement)
                        settlementType = SettlementTypeEnum.Cash;
                }
                return settlementType;
            }
        }
        #endregion Override Accessors

        #region Override Methods
        // EG 20150428 [20513] New
        public override DateTime ExpiryDate()
        {
            DateTime expiryDate = DateTime.MinValue;
            if (dataDocumentContainer.CurrentProduct.Product.ProductBase.IsFxAverageRateOption)
                expiryDate = fxAverageRateOption.ExpiryDateTime.ExpiryDateTimeValue;
            else if (dataDocumentContainer.CurrentProduct.Product.ProductBase.IsFxBarrierOption)
                expiryDate = fxBarrierOption.ExpiryDateTime.ExpiryDateTimeValue;
            else if (dataDocumentContainer.CurrentProduct.Product.ProductBase.IsFxOptionLeg)
                expiryDate = fxOptionLeg.ExpiryDateTime.ExpiryDateTimeValue;
            else if (dataDocumentContainer.CurrentProduct.Product.ProductBase.IsFxDigitalOption)
                expiryDate = fxDigitalOption.ExpiryDateTime.ExpiryDateTimeValue;
            return expiryDate;
        }
        // EG 20150428 [20513] New
        public override IExpiryDateTime ExpiryDateTime()
        {
            IExpiryDateTime expiryDateTime = null;
            if (dataDocumentContainer.CurrentProduct.Product.ProductBase.IsFxAverageRateOption)
                expiryDateTime = fxAverageRateOption.ExpiryDateTime;
            else if (dataDocumentContainer.CurrentProduct.Product.ProductBase.IsFxBarrierOption)
                expiryDateTime = fxBarrierOption.ExpiryDateTime;
            else if (dataDocumentContainer.CurrentProduct.Product.ProductBase.IsFxOptionLeg)
                expiryDateTime = fxOptionLeg.ExpiryDateTime;
            else if (dataDocumentContainer.CurrentProduct.Product.ProductBase.IsFxDigitalOption)
                expiryDateTime = fxDigitalOption.ExpiryDateTime;
            return expiryDateTime;
        }
        public override IFxCashSettlement FxCashSettlement()
        {
            IFxCashSettlement fxCashSettlement = null;
            if (dataDocumentContainer.CurrentProduct.Product.ProductBase.IsFxOptionLeg && fxOptionLeg.CashSettlementTermsSpecified)
                fxCashSettlement = fxOptionLeg.CashSettlementTerms;
            else if (dataDocumentContainer.CurrentProduct.Product.ProductBase.IsFxBarrierOption && fxBarrierOption.CashSettlementTermsSpecified)
                    fxCashSettlement = fxBarrierOption.CashSettlementTerms;
            return fxCashSettlement;
        }
        // EG 20150428 [20513] New
        public override DateTime ValueDate()
        {
            DateTime valueDate = DateTime.MinValue;
            if (dataDocumentContainer.CurrentProduct.Product.ProductBase.IsFxAverageRateOption)
                valueDate = fxAverageRateOption.ValueDate.DateValue;
            else if (dataDocumentContainer.CurrentProduct.Product.ProductBase.IsFxBarrierOption)
                valueDate = fxBarrierOption.ValueDate.DateValue;
            else if (dataDocumentContainer.CurrentProduct.Product.ProductBase.IsFxOptionLeg)
                valueDate = fxOptionLeg.ValueDate.DateValue;
            else if (dataDocumentContainer.CurrentProduct.Product.ProductBase.IsFxDigitalOption)
                valueDate = fxDigitalOption.ValueDate.DateValue;
            return valueDate;
        }
        // EG 20150428 [20513] New
        public override IFxStrikePrice StrikePrice()
        {
            IFxStrikePrice fxStrikePrice = null;
            if (dataDocumentContainer.CurrentProduct.Product.ProductBase.IsFxAverageRateOption)
                fxStrikePrice = fxAverageRateOption.FxStrikePrice;
            else if (dataDocumentContainer.CurrentProduct.Product.ProductBase.IsFxBarrierOption)
                fxStrikePrice = fxBarrierOption.FxStrikePrice;
            else if (dataDocumentContainer.CurrentProduct.Product.ProductBase.IsFxOptionLeg)
                fxStrikePrice = fxOptionLeg.FxStrikePrice;
            return fxStrikePrice;
        }
        // EG 20150428 [20513] New
        public override decimal SpotRate()
        {
            decimal spotRate = 0;
            if (dataDocumentContainer.CurrentProduct.Product.ProductBase.IsFxAverageRateOption && fxAverageRateOption.SpotRateSpecified)
                spotRate = fxAverageRateOption.SpotRate.DecValue;
            else if (dataDocumentContainer.CurrentProduct.Product.ProductBase.IsFxBarrierOption && fxBarrierOption.SpotRateSpecified)
                spotRate = fxBarrierOption.SpotRate.DecValue;
            else if (dataDocumentContainer.CurrentProduct.Product.ProductBase.IsFxDigitalOption && fxDigitalOption.SpotRateSpecified)
                spotRate = fxDigitalOption.SpotRate.DecValue;
            return spotRate;
        }
        // EG 20150428 [20513] New
        public override PayoutEnum CappedCalledOrFlooredPutPayoutStyle(string pInstrumentNo)
        {
            PayoutEnum payoutEnum = PayoutEnum.Deferred;
            if (dataDocumentContainer.CurrentProduct.Product.ProductBase.IsFxBarrierOption && fxBarrierOption.CappedCallOrFlooredPutSpecified)
                payoutEnum = fxBarrierOption.CappedCallOrFlooredPut.PayoutStyle;
            return payoutEnum;
        }

        // EG 20150428 [20513] New
        // EG 20180514 [23812] Report
        public override void InitializeProduct(ActionEventsBase pAction)
        {
            IProductBase product = dataDocumentContainer.CurrentProduct.Product.ProductBase;

            ActionEvents action = pAction as ActionEvents;
            //action.SettlementRate = new EFS_Decimal();
            action.SettlementRate = new Pair<Cst.PriceQuoteUnits, EFS_Decimal>();
            action.StrikePrice = new EFS_Decimal();
            action.SettlementAmount = new EFS_Decimal();

            //action.CallAmount = new EFS_Decimal();
            //action.PutAmount = new EFS_Decimal();

            string eventCode = action.CurrentEventParent.eventCode;
            action.IsAmerican = EventCodeFunc.IsAmericanOption(eventCode);
            action.IsEuropean = EventCodeFunc.IsEuropeanOption(eventCode);
            action.IsBermuda = EventCodeFunc.IsBermudaOption(eventCode);

            action.IsFxAverageOption = EventCodeFunc.IsAverageRateOption(eventCode);
            action.IsFxSimpleOption = EventCodeFunc.IsSimpleOption(eventCode);
            action.IsFxBarrierOption = EventCodeFunc.IsBarrierOption(eventCode);
            action.IsFxDigitalOption = EventCodeFunc.IsDigitalOption(eventCode);

            action.ExpiryDate = ExpiryDate();
            action.ValueDate = new EFS_Date
            {
                DateValue = ValueDate()
            };
            action.FxStrikePrice = StrikePrice();

            action.Buyer = SetBuyerSeller(action, BuyerSellerEnum.BUYER);
            action.Seller = SetBuyerSeller(action, BuyerSellerEnum.SELLER);

            if (null != action.FxStrikePrice)
                action.StrikePrice.DecValue = action.FxStrikePrice.Rate.DecValue;

            if (false == action.IsFxDigitalOption)
            {
                action.StartCallAmount = action.CurrentEventParent.GetEventStart(EventTypeFunc.CallCurrency);
                // EG 20150706 [21021]
                //action.CallAmount = new AmountPayerReceiverInfo(null, action.StartCallAmount.valorisation.DecValue, action.StartCallAmount.unit,
                //        action.StartCallAmount.idPayer, action.StartCallAmount.payer, action.StartCallAmount.idPayerBook, action.StartCallAmount.payerBook,
                //        action.StartCallAmount.idReceiver, action.StartCallAmount.receiver, action.StartCallAmount.idReceiverBook, action.StartCallAmount.receiverBook);
                // RD 20150806 Add test
                if (action.StartCallAmount != null)
                {
                    action.CallAmount = new AmountPayerReceiverInfo(null, action.StartCallAmount.valorisation.IntValue, action.StartCallAmount.unit,
                        action.StartCallAmount.PayerInfo, action.StartCallAmount.ReceiverInfo);
                }

                action.StartPutAmount = action.CurrentEventParent.GetEventStart(EventTypeFunc.PutCurrency);
                // EG 20150706 [21021]
                //action.PutAmount = new AmountPayerReceiverInfo(null, action.StartPutAmount.valorisation.DecValue, action.StartPutAmount.unit,
                //        action.StartPutAmount.idPayer, action.StartPutAmount.payer, action.StartPutAmount.idPayerBook, action.StartPutAmount.payerBook,
                //        action.StartPutAmount.idReceiver, action.StartPutAmount.receiver, action.StartPutAmount.idReceiverBook, action.StartPutAmount.receiverBook);
                // RD 20150806 Add test
                if (action.StartPutAmount != null)
                {
                    action.PutAmount = new AmountPayerReceiverInfo(null, action.StartPutAmount.valorisation.IntValue, action.StartPutAmount.unit,
                        action.StartPutAmount.PayerInfo, action.StartPutAmount.ReceiverInfo);
                }

                action.SettlementType = SettlementType.ToString();
                action.SettlementCurrency = SettlementCurrency;
            }
            if (action.IsFxAverageOption)
            {
                action.SettlementRateQuoteBasis = fxAverageRateOption.AverageRateQuoteBasis;
                #region Virtual fxFixing
                action.FxFixing = product.CreateFxFixing();
                action.FxFixing.FixingTime = action.FxFixing.CreateBusinessCenterTime(fxAverageRateOption.FixingTime);
                if (StrikeQuoteBasisEnum.CallCurrencyPerPutCurrency == fxAverageRateOption.AverageRateQuoteBasis)
                    action.FxFixing.QuotedCurrencyPair = action.FxFixing.CreateQuotedCurrencyPair(action.CallAmount.Currency, action.PutAmount.Currency, QuoteBasisEnum.Currency1PerCurrency2);
                else
                    action.FxFixing.QuotedCurrencyPair = action.FxFixing.CreateQuotedCurrencyPair(action.PutAmount.Currency, action.CallAmount.Currency, QuoteBasisEnum.Currency1PerCurrency2);
                action.FxFixing.PrimaryRateSource = fxAverageRateOption.PrimaryRateSource;
                action.FxFixing.SecondaryRateSourceSpecified = fxAverageRateOption.SecondaryRateSourceSpecified;
                if (action.FxFixing.SecondaryRateSourceSpecified)
                    action.FxFixing.SecondaryRateSource = fxAverageRateOption.SecondaryRateSource;
                #endregion Virtual fxFixing
                action.IsFxAverageStrikeOption = fxAverageRateOption.AverageStrikeOptionSpecified;
                action.IsGeometricAverage = fxAverageRateOption.GeometricAverageSpecified;
            }
            else
            {
                IFxCashSettlement fxCashSettlement = FxCashSettlement();
                if (null != fxCashSettlement)
                {
                    action.FxFixing = fxCashSettlement.Fixing[0];
                }
                else
                {
                    IExpiryDateTime expiryDateTime = ExpiryDateTime();
                    // RD 20150806 Add test
                    if (null != expiryDateTime)
                    {
                        #region Virtual fxFixing
                        action.FxFixing = product.CreateFxFixing();
                        action.FxFixing.FixingTime = action.FxFixing.CreateBusinessCenterTime(expiryDateTime.BusinessCenterTime);
                        if ((null != action.FxStrikePrice) && ((null != action.CallAmount) || (null != action.PutAmount)))
                        {
                            if (StrikeQuoteBasisEnum.CallCurrencyPerPutCurrency == action.FxStrikePrice.StrikeQuoteBasis)
                                action.FxFixing.QuotedCurrencyPair = action.FxFixing.CreateQuotedCurrencyPair(action.CallAmount.Currency, action.PutAmount.Currency, QuoteBasisEnum.Currency1PerCurrency2);
                            else
                                action.FxFixing.QuotedCurrencyPair = action.FxFixing.CreateQuotedCurrencyPair(action.PutAmount.Currency, action.CallAmount.Currency, QuoteBasisEnum.Currency1PerCurrency2);
                        }
                        #endregion Virtual fxFixing
                    }
                }
            }

            #region BarrierOption
            if (action.IsFxBarrierOption)
            {
                action.IsCappedCallOrFlooredPutOption = fxBarrierOption.CappedCallOrFlooredPutSpecified;
                if (action.IsCappedCallOrFlooredPutOption)
                {
                    action.IsFxCapBarrierOption = fxBarrierOption.CappedCallOrFlooredPut.TypeFxCapBarrierSpecified;
                    action.IsFxFloorBarrierOption = fxBarrierOption.CappedCallOrFlooredPut.TypeFxFloorBarrierSpecified;
                    action.PayoutStyle = fxBarrierOption.CappedCallOrFlooredPut.PayoutStyle;
                }
            }
            #endregion BarrierOption

            #region QuotationBasis
            if (action.IsCashSettlement && ((null != action.CallAmount) || (null != action.PutAmount)))
            {
                if (SettlementCurrency == action.CallAmount.Currency)
                    action.QuoteBasis = StrikeQuoteBasisEnum.PutCurrencyPerCallCurrency;
                else if (SettlementCurrency == action.PutAmount.Currency)
                    action.QuoteBasis = StrikeQuoteBasisEnum.CallCurrencyPerPutCurrency;
            }
            else if (null != action.FxStrikePrice)
                action.QuoteBasis = action.FxStrikePrice.StrikeQuoteBasis;
            #endregion QuotationBasis
        }

        #endregion Override Methods

        #region Methods
        /// <summary>
        /// Alimentation des ID|IDENTIFIER des BUYER|SELLER de l'option
        /// Acteur = First, Book = Second
        /// </summary>
        /// <param name="pBuyerSeller"></param>
        /// <returns></returns>
        // EG 20150706 [21021] Nullable<int> for actor.Second (book)
        private Pair<Pair<Nullable<int>, string>, Pair<Nullable<int>, string>> SetBuyerSeller(ActionEvents pAction, BuyerSellerEnum pBuyerSeller)
        {
            // EG 20150706 [21021] 
            Pair<Pair<Nullable<int>, string>, Pair<Nullable<int>, string>> actor = new Pair<Pair<Nullable<int>, string>, Pair<Nullable<int>, string>>();
            string hRef = string.Empty;
            switch (pBuyerSeller)
            {
                case BuyerSellerEnum.BUYER:
                    if (pAction.IsFxAverageOption)
                        hRef = fxAverageRateOption.BuyerPartyReference.HRef;
                    else if (pAction.IsFxBarrierOption)
                        hRef = fxBarrierOption.BuyerPartyReference.HRef;
                    else if (pAction.IsFxSimpleOption)
                        hRef = fxOptionLeg.BuyerPartyReference.HRef;
                    else if (pAction.IsFxDigitalOption)
                        hRef = fxDigitalOption.BuyerPartyReference.HRef;
                    break;
                case BuyerSellerEnum.SELLER:
                    if (pAction.IsFxAverageOption)
                        hRef = fxAverageRateOption.SellerPartyReference.HRef;
                    else if (pAction.IsFxBarrierOption)
                        hRef = fxBarrierOption.SellerPartyReference.HRef;
                    else if (pAction.IsFxSimpleOption)
                        hRef = fxOptionLeg.SellerPartyReference.HRef;
                    else if (pAction.IsFxDigitalOption)
                        hRef = fxDigitalOption.SellerPartyReference.HRef;
                    break;
            }
            // EG 20150706 [21021]
            actor.First = new Pair<Nullable<int>, string>();
            actor.Second = new Pair<Nullable<int>, string>();

            IParty party = dataDocumentContainer.GetParty(hRef);
            if (null != party)
            {
                if (0 < party.OTCmlId)
                    actor.First.First = party.OTCmlId;
                actor.First.Second = party.PartyId;
            }
            IBookId book = dataDocumentContainer.GetBookId(hRef);
            if (null != book)
            {
                if (0 < book.OTCmlId)
                    actor.Second.First = book.OTCmlId;
                actor.Second.Second = book.Value;
            }
            return actor;
        }
        #endregion Methods
    }
    #endregion FX_TradeAction

    #region FX_AbandonEvents
    public class FX_AbandonEvents : FX_AbandonAndExerciseEvents
    {
        #region Members
        public string abandonType;
        #endregion Members
        #region Accessors
        #region AddCells_Capture
        public override TableCell[] AddCells_Capture
        {
            get
            {
                ArrayList aCell = new ArrayList();
                aCell.AddRange(base.AddCells_Capture);
                aCell.Add(TableTools.AddCell(note, HorizontalAlign.NotSet, 100, UnitEnum.Percentage, true, false, false));
                aCell.Add(TableTools.AddCell(Cst.HTMLSpace, HorizontalAlign.Center, 0, UnitEnum.Pixel));
                return (TableCell[])aCell.ToArray(typeof(TableCell));
            }
        }
        #endregion AddCells_Capture
        #region CreateHeaderCells_Capture
        public override TableHeaderCell[] CreateHeaderCells_Capture
        {
            get
            {
                ArrayList aHeaderCell = new ArrayList();
                aHeaderCell.AddRange(base.CreateHeaderCells_Capture);
                aHeaderCell.AddRange(base.CreateHeaderCells_CaptureAbandon);
                return (TableHeaderCell[])aHeaderCell.ToArray(typeof(TableHeaderCell));
            }
        }
        #endregion CreateHeaderCells_Capture

        // TO CONTROL
        #region CreateControlCurrentAction
        public TableRow[] CreateControlCurrentAction
        {
            get
            {
                ArrayList aTableRow = new ArrayList();
                #region ActionDate
                if (DtFunc.IsDateTimeEmpty(actionDate.DateValue))
                {
                    actionDate.DateTimeValue = ExpiryDate;
                    actionTime.TimeValue = actionDate.DateTimeValue;
                }
                aTableRow.Add(base.CreateControlActionDate());
                #endregion ActionDate
                #region ActionTime
                aTableRow.Add(base.CreateControlActionTime(IsEuropean));
                #endregion ActionTime
                #region ValueDate
                aTableRow.Add(base.CreateControlValueDate);
                #endregion ValueDate
                #region CallCurrencyAmount
                aTableRow.Add(base.CreateControlCurrencyAmount(OptionTypeEnum.Call, true));
                #endregion CallCurrencyAmount
                #region PutCurrencyAmount
                aTableRow.Add(base.CreateControlCurrencyAmount(OptionTypeEnum.Put, true));
                #endregion PutCurrencyAmount
                #region Description
                aTableRow.AddRange(base.CreateControlDescription);
                #endregion Description
                return (TableRow[])aTableRow.ToArray(typeof(TableRow));
            }
        }
        #endregion CreateControlCurrentAction
        #region ResFormTitleAbandonExerciseDate
        protected override string ResFormTitleAbandonExerciseDate { get { return Ressource.GetString("AbandonDate"); } }
        #endregion ResFormTitleAbandonExerciseDate
        #endregion Accessors
        #region Constructors
        public FX_AbandonEvents() { }
        public FX_AbandonEvents(TradeActionEventBase pEvent, TradeActionEventBase pEventParent)
            : base(pEvent, pEventParent)
        {
            #region AbandonType
            abandonType = m_Event.eventType;
            #endregion AbandonType
            if (null != m_Event.EventClassGroupLevel)
            {
                #region Abandon Date & Time
                actionDate.DateValue = m_Event.dtEndPeriod.DateValue;
                actionTime.TimeValue = m_Event.dtEndPeriod.DateValue;
                #endregion Abandon Date & Time
                #region ValueDate
                ValueDate.DateValue = m_Event.EventClassGroupLevel.dtEvent.DateValue;
                #endregion ValueDate
                #region Call & Put CurrencyAmount
                CallAmount = SetAmountAndQuantityExercise(EventTypeFunc.CallCurrency, CurrentEvent.GetEventTermination(EventTypeFunc.CallCurrency));
                PutAmount = SetAmountAndQuantityExercise(EventTypeFunc.PutCurrency, CurrentEvent.GetEventTermination(EventTypeFunc.PutCurrency));
                #endregion Call & Put CurrencyAmount
            }
            else
            {
                #region Abandon Date & Time
                if (IsEuropean)
                {
                    actionDate.DateValue = ExpiryDate;
                    actionTime.TimeValue = ExpiryDate;
                }
                else
                {
                    // FI 20200904 [XXXXX] call OTCmlHelper.GetDateSys  
                    //actionDate.DateValue = OTCmlHelper.GetDateBusiness(SessionTools.CS);
                    actionDate.DateValue = OTCmlHelper.GetDateSys(SessionTools.CS);
                    if (0 < actionDate.DateValue.CompareTo(ExpiryDate))
                        actionDate.DateValue = ExpiryDate;
                    actionTime.TimeValue = actionDate.DateTimeValue;
                }
                //actionDate.DateValue = ExpiryDate;
                //actionTime.TimeValue = ExpiryDate;
                #endregion Abandon Date & Time
                #region ValueDate
                ValueDate.DateValue = (IsEuropean ? ValueDate.DateValue : ExpiryDate.AddDays(2)); // 2jo à gérer
                #endregion ValueDate

                #region Call & Put CurrencyAmount
                if (EventTypeFunc.IsTotal(m_Event.eventType))
                {
                    #region Total abandon
                    CallAmount = SetAmountAndQuantityExercise(EventTypeFunc.CallCurrency,StartCallAmount);
                    PutAmount = SetAmountAndQuantityExercise(EventTypeFunc.PutCurrency, StartPutAmount);
                    #endregion Total abandon
                }
                else if (EventTypeFunc.IsPartiel(m_Event.eventType))
                {
                    #region Partial / Multiple abandon
                    CallAmount = SetAmountAndQuantityExercise(EventTypeFunc.CallCurrency, StartCallAmount, m_CallAmount_INT, m_CallAmount_TER);
                    PutAmount = SetAmountAndQuantityExercise(EventTypeFunc.PutCurrency, StartPutAmount, m_PutAmount_INT, m_PutAmount_TER);
                    #endregion Partial abandon
                }
                #endregion Call & Put CurrencyAmount
            }
        }
        #endregion Constructors
        #region Methods
        #region Save
        public override bool Save(Page pPage)
        {
            m_Event.validationRulesMessages = new ArrayList();
            bool isOk = base.Save(pPage);
            isOk = isOk && ValidationRules(pPage);
            m_Event.isValidated = isOk;
            return isOk;
        }
        #endregion Save

        // TO CONTROL
        #region PostedAction
        public object PostedAction(string pKeyAction)
        {
            string keyAction = pKeyAction + Convert.ToInt32(TradeActionCode.TradeActionCodeEnum.Abandon) + "_" + m_Event.eventCode;
            return new FX_AbandonMsg(idE, ActionDateTime, ValueDate.DateValue, abandonType, CallAmount, PutAmount, note, keyAction);
        }
        #endregion PostedAction
        #endregion Methods
    }
    #endregion FX_AbandonEvents
    #region FX_AbandonAndExerciseEvents
    public abstract class FX_AbandonAndExerciseEvents : AbandonAndExerciseEventsBase
    {
        #region Members
        protected TradeActionEvent[] m_CallAmount_INT;
        protected TradeActionEvent m_CallAmount_TER;
        protected TradeActionEvent[] m_PutAmount_INT;
        protected TradeActionEvent m_PutAmount_TER;
        protected TradeActionEvent[] m_SettlementAmount_INT;
        protected TradeActionEvent m_SettlementAmount_TER;
        #endregion Members
        #region Accessors
        #region AddCells_Capture
        public override TableCell[] AddCells_Capture
        {
            get
            {
                ArrayList aCell = new ArrayList();
                aCell.AddRange(base.AddCells_Capture);

                ArrayList aCell2 = new ArrayList();
                Table table = new Table
                {
                    CssClass = "subActionDataGrid",
                    CellPadding = 0,
                    CellSpacing = 0,
                    Height = Unit.Percentage(100)
                };
                TableRow tr = new TableRow
                {
                    CssClass = m_Event.GetRowClass(table.CssClass)
                };
                aCell2.Add(TableTools.AddCell("Call", true));
                aCell2.Add(TableTools.AddCell(StrFunc.FmtDecimalToCurrentCulture(CallAmount.Amount), HorizontalAlign.Right, 80, UnitEnum.Percentage));
                aCell2.Add(TableTools.AddCell(CallAmount.Currency, HorizontalAlign.Center));
                aCell2.Add(TableTools.AddCell(ResFormPaidBy, true));
                aCell2.Add(TableTools.AddCell(CallAmount.payer.actor.Second, HorizontalAlign.NotSet));
                aCell2.Add(TableTools.AddCell(CallAmount.payer.book.Second, HorizontalAlign.NotSet));
                tr.Cells.AddRange((TableCell[])aCell2.ToArray(typeof(TableCell)));
                table.Rows.Add(tr);

                aCell2 = new ArrayList();
                tr = new TableRow
                {
                    CssClass = m_Event.GetRowClass(table.CssClass)
                };
                aCell2.Add(TableTools.AddCell("Put", true));
                aCell2.Add(TableTools.AddCell(StrFunc.FmtDecimalToCurrentCulture(PutAmount.Amount), HorizontalAlign.Right, 80, UnitEnum.Percentage));
                aCell2.Add(TableTools.AddCell(PutAmount.Currency, HorizontalAlign.Center));
                aCell2.Add(TableTools.AddCell(ResFormPaidBy, true));
                aCell2.Add(TableTools.AddCell(PutAmount.payer.actor.Second, HorizontalAlign.NotSet));
                aCell2.Add(TableTools.AddCell(PutAmount.payer.book.Second, HorizontalAlign.NotSet));
                tr.Cells.AddRange((TableCell[])aCell2.ToArray(typeof(TableCell)));
                table.Rows.Add(tr);

                TableCell td = new TableCell();
                td.Controls.Add(table);
                aCell.Add(td);
                return (TableCell[])aCell.ToArray(typeof(TableCell));
            }
        }
        #endregion AddCells_Capture

        #region CreateControlValueDate
        protected TableRow CreateControlValueDate
        {
            get
            {
                TableRow tr = new TableRow
                {
                    CssClass = "DataGrid_ItemStyle"
                };
                TableCell td = new TableCell();
                ControlGUI controlGUI = new ControlGUI(true, ResFormValueDate)
                {
                    Regex = EFSRegex.TypeRegex.RegexDate,
                    LblWidth = 105
                };
                if (DtFunc.IsDateTimeEmpty(ValueDate.DateValue))
                    ValueDate.DateTimeValue = ExpiryDate.AddDays(2);

                FpMLCalendarBox txtDate = new FpMLCalendarBox(null, ValueDate.DateValue, "ValueDate", controlGUI, null, "TXTVALUEDATE_" + idE,
                    new Validator("ValueDate", true),
                    new Validator(EFSRegex.TypeRegex.RegexDate, "ValueDate", true, false));
                GetFormatControlAttribute(txtDate);
                td.Controls.Add(txtDate);
                tr.Cells.Add(td);
                return tr;
            }
        }
        #endregion CreateControlValueDate
        #region Resource in Form
        #region ResFormValueDate
        protected override string ResFormValueDate { get { return Ressource.GetString("ProvisionRelevantUnderlyingDate"); } }
        #endregion ResFormValueDate
        #endregion Resource in Form
        #endregion Accessors
        #region Constructors
        public FX_AbandonAndExerciseEvents() { }
        public FX_AbandonAndExerciseEvents(TradeActionEventBase pEvent, TradeActionEventBase pEventParent)
            : base(pEvent, pEventParent)
        {
            m_CallAmount_INT = EventExercise(EventCodeFunc.Intermediary, EventTypeFunc.CallCurrency);
            m_CallAmount_TER = EventExercise(EventTypeFunc.CallCurrency);
            m_PutAmount_INT = EventExercise(EventCodeFunc.Intermediary, EventTypeFunc.PutCurrency);
            m_PutAmount_TER = EventExercise(EventTypeFunc.PutCurrency);

            m_SettlementAmount_INT = EventExercise(EventCodeFunc.Intermediary, EventTypeFunc.SettlementCurrency);
            m_SettlementAmount_TER = EventExercise(EventTypeFunc.SettlementCurrency);

            SetExerciseTypeList(null == m_CallAmount_INT);
        }
        #endregion Constructors
        #region Methods
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
        #region Save
        public override bool Save(Page pPage)
        {
            bool isOk = base.Save(pPage);
            if (isOk)
            {
                if (null != pPage.Request.Form["TXTVALUEDATE_" + idE])
                    ValueDate.DateValue = new DtFunc().StringToDateTime(pPage.Request.Form["TXTVALUEDATE_" + idE]);
                if (null != pPage.Request.Form["TXTCALLAMOUNT_" + idE])
                    CallAmount.Amount = DecFunc.DecValue(pPage.Request.Form["TXTCALLAMOUNT_" + idE], Thread.CurrentThread.CurrentCulture);
                if (null != pPage.Request.Form["TXTPUTAMOUNT_" + idE])
                    PutAmount.Amount = DecFunc.DecValue(pPage.Request.Form["TXTPUTAMOUNT_" + idE], Thread.CurrentThread.CurrentCulture);
            }
            return isOk;
        }
        #endregion Save

        // TO CONTROL
        #region CalculCurrencyAmountExercise
        public virtual void CalculCurrencyAmountExercise(PageBase pPage, string pControlId)
        {
            TextBox ctrlId = (TextBox)pPage.FindControl(pControlId);

            if ((("TXTCALLAMOUNT_" + idE) == pControlId) || (("TXTPUTAMOUNT_" + idE) == pControlId))
            {
                EFS_Cash cash = null;
                if (null != pPage.Request.Form[pControlId])
                {
                    decimal amount = DecFunc.DecValue(ctrlId.Text, Thread.CurrentThread.CurrentCulture);
                    Control ctrl;
                    if (("TXTCALLAMOUNT_" + idE) == pControlId)
                    {
                        ctrl = pPage.FindControl("TXTPUTAMOUNT_" + idE);
                        if (null != ctrl)
                        {
                            if (StrikeQuoteBasisEnum.CallCurrencyPerPutCurrency == FxStrikePrice.StrikeQuoteBasis)
                                amount /= FxStrikePrice.Rate.DecValue;
                            else
                                amount *= FxStrikePrice.Rate.DecValue;
                            cash = new EFS_Cash(SessionTools.CS, amount, PutAmount.Currency);
                        }
                    }
                    else
                    {
                        ctrl = pPage.FindControl("TXTCALLAMOUNT_" + idE);
                        if (null != ctrl)
                        {
                            if (StrikeQuoteBasisEnum.CallCurrencyPerPutCurrency == FxStrikePrice.StrikeQuoteBasis)
                                amount *= FxStrikePrice.Rate.DecValue;
                            else
                                amount /= FxStrikePrice.Rate.DecValue;
                            cash = new EFS_Cash(SessionTools.CS, amount, CallAmount.Currency);
                        }
                    }
                    ((TextBox)ctrl).Text = StrFunc.FmtDecimalToCurrentCulture(cash.AmountRounded.ToString(NumberFormatInfo.InvariantInfo));
                }
            }
        }
        #endregion CalculCurrencyAmountExercise
        #region CreateControlCurrencyAmount
        protected TableRow CreateControlCurrencyAmount(OptionTypeEnum pOptionType)
        {
            return CreateControlCurrencyAmount(pOptionType, false);
        }
        protected TableRow CreateControlCurrencyAmount(OptionTypeEnum pOptionType, bool pIsReadOnly)
        {
            string key = pOptionType.ToString();
            AmountPayerReceiverInfo amount = null;
            if (OptionTypeEnum.Call == pOptionType)
                amount = (AmountPayerReceiverInfo) CallAmount;
            else if (OptionTypeEnum.Put == pOptionType)
                amount = (AmountPayerReceiverInfo)PutAmount ;

            TableRow tr = new TableRow
            {
                CssClass = "DataGrid_ItemStyle"
            };
            TableCell td = new TableCell();
            ControlGUI controlGUI = new ControlGUI(true, key)
            {
                Regex = EFSRegex.TypeRegex.RegexAmountExtend,
                LblWidth = 100
            };
            FpMLTextBox txtAmount = new FpMLTextBox(null, new EFS_Decimal(amount.Amount).CultureValue, 150, key + "Amount", controlGUI, null, pIsReadOnly,
                "TXT" + pOptionType.ToString().ToUpper() + "AMOUNT_" + idE, null,
                new Validator(key, true),
                new Validator(EFSRegex.TypeRegex.RegexAmountExtend, key, true, false));
            StringBuilder sb = new StringBuilder();
            sb.Append(@"if (Page_ClientValidate())");
            sb.AppendFormat("javascript:__doPostBack('{0}','{1}');", txtAmount.ID,
                TradeActionMode.TradeActionModeEnum.CalculCurrencyAmountExercise);
            txtAmount.Attributes.Add("onchange", sb.ToString());
            td.Controls.Add(txtAmount);
            td.Controls.Add(new LiteralControl(Cst.HTMLSpace + amount.Currency));
            td.Controls.Add(new LiteralControl(Cst.HTMLSpace + ResFormPaidBy + Cst.HTMLSpace + amount.payer.actor.Second));
            tr.Cells.Add(td);
            return tr;
        }
        #endregion CreateControlCurrencyAmount
        #region CurrentCallAmount
        protected decimal CurrentCallAmount(string pExerciseType)
        {
            decimal amount = StartCallAmount.valorisation.DecValue;
            if (EventTypeFunc.IsMultiple(pExerciseType))
            {
                decimal callAmount_INT = 0;
                if (null != m_CallAmount_INT)
                {
                    foreach (TradeActionEvent item in m_CallAmount_INT)
                        callAmount_INT += item.valorisation.DecValue;
                    amount -= callAmount_INT;
                }
                if (null != m_CallAmount_TER)
                    amount -= m_CallAmount_TER.valorisation.DecValue;
            }
            return amount;
        }
        #endregion CurrentCallAmount
        #region CurrentPutAmount
        protected decimal CurrentPutAmount(string pExerciseType)
        {
            decimal amount = StartPutAmount.valorisation.DecValue;
            if (EventTypeFunc.IsMultiple(pExerciseType))
            {
                decimal putAmount_INT = 0;
                if (null != m_PutAmount_INT)
                {
                    foreach (TradeActionEvent item in m_PutAmount_INT)
                        putAmount_INT += item.valorisation.DecValue;
                    amount -= putAmount_INT;
                }
                if (null != m_PutAmount_TER)
                    amount -= m_PutAmount_TER.valorisation.DecValue;
            }
            return amount;
        }
        #endregion CurrentPutAmount
        #endregion Methods
    }
    #endregion FX_AbandonAndExerciseEvents

    #region FxBarrierAndTriggerEvents
    public abstract class FxBarrierAndTriggerEvents : ActionEvents 
    {
        #region Members
        public string idStTrigger;
        public EFS_Decimal touchRate;
        #endregion Members
        #region Accessors
        #region AddCells_Capture
        private TableCell[] AddCells_Capture
        {
            get
            {
                ArrayList aCell = new ArrayList
                {
                    TableTools.AddCell(idStTrigger, HorizontalAlign.Center),
                    TableTools.AddCell(touchRate.CultureValue, HorizontalAlign.Right, 80, UnitEnum.Pixel),
                    TableTools.AddCell(ActionDate, HorizontalAlign.Center, 80, UnitEnum.Pixel),
                    TableTools.AddCell(actionTime.Value, HorizontalAlign.Center, 60, UnitEnum.Pixel),
                    TableTools.AddCell(note, HorizontalAlign.NotSet, 100, UnitEnum.Percentage, true, false, false),
                    TableTools.AddCell(Cst.HTMLSpace, HorizontalAlign.Center, 0, UnitEnum.Pixel)
                };
                return (TableCell[])aCell.ToArray(typeof(TableCell));
            }
        }
        #endregion AddCells_Capture
        #region AddCells_Static
        public TableCell[] AddCells_Static(TradeActionEvent pEvent, TradeActionEvent pEventParent)
        {
            ArrayList aCell = new ArrayList();
            aCell.AddRange(pEvent.NewCells_Static());
            if (null != pEvent.details)
            {
                TableCell[] aCellDetails = pEvent.details.AddCells_Static;
                if (ArrFunc.IsFilled(aCellDetails))
                    aCell.AddRange(pEvent.details.AddCells_Static);
            }
            aCell.AddRange(AddCells_Capture);
            return (TableCell[])aCell.ToArray(typeof(TableCell));
        }
        #endregion AddCells_Static
        #region CreateHeaderCells_Capture
        private TableHeaderCell[] CreateHeaderCells_Capture
        {
            get
            {
                ArrayList aHeaderCell = new ArrayList
                {
                    TableTools.AddHeaderCell(ResFormStatutTrigger, false, 0, UnitEnum.Pixel, 0, false),
                    TableTools.AddHeaderCell(ResFormExchangeRate, false, 0, UnitEnum.Pixel, 0, false),
                    TableTools.AddHeaderCell(ResFormActionDate, false, 0, UnitEnum.Pixel, 2, false),
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
                    TableTools.AddHeaderCell(ResFormTitlePeriod, false, 50, UnitEnum.Pixel, 2, false)
                };
                if (false == IsBondOption)
                {
                    aHeaderCell.Add(TableTools.AddHeaderCell(ResFormTitleCurrencies, false, 0, UnitEnum.Pixel, 0, false));
                    aHeaderCell.Add(TableTools.AddHeaderCell(ResFormSpotRate, false, 100, UnitEnum.Pixel, 0, false));
                }
                aHeaderCell.Add(TableTools.AddHeaderCell(ResFormTriggerRate, false, 100, UnitEnum.Pixel, 0, false));
                return (TableHeaderCell[])aHeaderCell.ToArray(typeof(TableHeaderCell));
            }
        }
        #endregion CreateHeaderCells_Static
        #region IsActionChanged
        public override bool IsActionChanged
        {
            get
            {
                FxBarrierAndTriggerEvents item = (FxBarrierAndTriggerEvents)m_Event.m_OriginalAction;
                return (null == item || false == item.idStTrigger.Equals(idStTrigger) ||
                        false == item.ActionDateTime.Equals(ActionDateTime) ||
                        false == item.touchRate.DecValue.Equals(touchRate.DecValue));
            }
        }
        #endregion IsActionChanged
        #region Resource in Form
        #region ResFormSpotRate
        protected virtual string ResFormSpotRate { get { return Ressource.GetString("SPOTRATE"); } }
        #endregion ResFormSpotRate
        #region ResFormTriggerRate
        protected virtual string ResFormTriggerRate { get { return Ressource.GetString("TriggerRate"); } }
        #endregion ResFormTriggerRate
        #region ResFormStatutTrigger
        protected virtual string ResFormStatutTrigger { get { return Ressource.GetString("StatutTrigger"); } }
        #endregion ResFormStatutTrigger
        #endregion Resource in Form
        #endregion Accessors
        #region Constructors
        public FxBarrierAndTriggerEvents() { }
        public FxBarrierAndTriggerEvents(TradeActionEventBase pEvent, TradeActionEventBase pEventParent)
            : base(pEvent, pEventParent)
        {
            idStTrigger = m_Event.idStTrigger;
            touchRate = new EFS_Decimal(m_Event.valorisation.Value);
        }
        #endregion Constructors
        #region Methods
        #region CreateControlCurrentAction
        public TableRow[] CreateControlCurrentAction
        {
            get
            {
                ArrayList aTableRow = new ArrayList();
                #region Status
                aTableRow.Add(CreateControlIdStTrigger);
                #endregion Status
                #region ActionDate
                #region DateMin / DateMax
                string startPeriod = DtFunc.DateTimeToString(m_Event.dtStartPeriod.DateValue, DtFunc.FmtShortDate);
                string endPeriod = DtFunc.DateTimeToString(m_Event.dtEndPeriod.DateValue, DtFunc.FmtShortDate);
                if (EventTypeFunc.IsEuropeanTrigger(m_Event.eventType))
                    startPeriod = endPeriod;
                if (DtFunc.IsDateTimeEmpty(actionDate.DateValue))
                {
                    if (EventTypeFunc.IsEuropeanTrigger(m_Event.eventType))
                        actionDate.DateTimeValue = ExpiryDate;
                    else
                    {
                        DateTime dtSys = OTCmlHelper.GetDateBusiness(SessionTools.CS);
                        if (-1 == ExpiryDate.CompareTo(dtSys))
                            actionDate.DateTimeValue = ExpiryDate;
                        else
                            actionDate.DateTimeValue = dtSys;
                    }
                    actionTime.TimeValue = actionDate.DateTimeValue;
                }
                string errorMess = Ressource.GetString2("Failure_RangeDate", startPeriod, endPeriod);
                Validator validator = new Validator(errorMess, true, false, ValidationDataType.Date, startPeriod, endPeriod);
                #endregion DateMin / DateMax
                aTableRow.Add(base.CreateControlActionDate(validator));
                #endregion ActionDate
                #region ActionTime
                aTableRow.Add(base.CreateControlActionTime());
                #endregion ActionTime
                #region TouchRate
                aTableRow.Add(CreateControlTouchRate);
                #endregion TouchRate
                #region Description
                aTableRow.AddRange(base.CreateControlDescription);
                #endregion Description
                return (TableRow[])aTableRow.ToArray(typeof(TableRow));
            }
        }
        #endregion CreateControlCurrentAction
        #region CreateControlIdStTrigger
        public TableRow CreateControlIdStTrigger
        {
            get
            {
                TableRow tr = new TableRow();
                TableCell td = new TableCell();
                //StringBuilder sb = new StringBuilder();
                tr.CssClass = "DataGrid_ItemStyle";
                ControlGUI controlGUI = new ControlGUI(true, ResFormStatutTrigger)
                {
                    LblWidth = 105
                };
                FpMLDropDownList ddlIdStTrigger = new FpMLDropDownList(null, idStTrigger, 150, "TriggerStatus", controlGUI, null)
                {
                    ID = "DDLIDSTTRIGGER_" + idE.ToString()
                };
                td.Controls.Add(ddlIdStTrigger);
                tr.Cells.Add(td);
                return tr;
            }
        }
        #endregion CreateControlIdStTrigger
        #region CreateControlTouchRate
        public TableRow CreateControlTouchRate
        {
            get
            {
                TableRow tr = new TableRow();
                TableCell td = new TableCell();
                StringBuilder sb = new StringBuilder();
                tr.CssClass = "DataGrid_ItemStyle";
                ControlGUI controlGUI = new ControlGUI(true, ResFormTouchRate)
                {
                    Regex = EFSRegex.TypeRegex.RegexFxRateExtend,
                    LblWidth = 100
                };
                FpMLTextBox txtRate = new FpMLTextBox(null, touchRate.CultureValue, 150, "TouchRate", controlGUI, null, false, "TXTTOUCHRATE_" + idE, null,
                    new Validator("TouchRate", true),
                    new Validator(EFSRegex.TypeRegex.RegexFxRateExtend, "TouchRate", true, false));
                sb.Append(@"if (Page_ClientValidate())");
                sb.AppendFormat("javascript:__doPostBack('{0}','{1}');", txtRate.ID, TradeActionMode.TradeActionModeEnum.ActiveDeactiveBarrierTrigger);
                txtRate.Attributes.Add("onchange", sb.ToString());
                td.Controls.Add(txtRate);
                tr.Cells.Add(td);
                return tr;
            }
        }
        #endregion CreateControlTouchRate
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
        #region IsEventChanged
        public override bool IsEventChanged(TradeActionEventBase pEvent)
        {
            bool isEventChanged = base.IsEventChanged(pEvent);
            if (false == isEventChanged)
            {
                decimal prevTouchRate = 0;
                string prevIdStTrigger = string.Empty;
                if (pEvent.valorisationSpecified)
                    prevTouchRate = pEvent.valorisation.DecValue;
                if (pEvent.idStTriggerSpecified)
                    prevIdStTrigger = pEvent.idStTrigger;
                isEventChanged = (idStTrigger != prevIdStTrigger) || (touchRate.DecValue != prevTouchRate);
            }
            return isEventChanged;
        }
        #endregion IsEventChanged
        #region Save
        public override bool Save(Page pPage)
        {
            bool isOk = base.Save(pPage);
            if (isOk)
            {
                if (null != pPage.Request.Form["DDLIDSTTRIGGER_" + idE])
                    idStTrigger = pPage.Request.Form["DDLIDSTTRIGGER_" + idE];
                if (null != pPage.Request.Form["TXTTOUCHRATE_" + idE])
                    touchRate.DecValue = DecFunc.DecValue(pPage.Request.Form["TXTTOUCHRATE_" + idE], Thread.CurrentThread.CurrentCulture);
            }
            return isOk;
        }
        #endregion Save
        #region ValidationRules
        public override bool ValidationRules(Page pPage)
        {
            bool isOk = base.ValidationRules(pPage);
            return isOk;
        }
        #endregion ValidationRules
        #endregion Methods
        #region Clone
        // EG 20160404 Migration vs2013
        //public virtual object Clone()
        //{
        //    return null;
        //}
        #endregion Clone
        #region Virtual Methods
        #region ActiveDeactiveBarrierTrigger
        public virtual void ActiveDeactiveBarrierTrigger(PageBase pPage, string pControlId)
        {
            FormatControl(pPage, pControlId);

            string barrierTriggerType = m_Event.eventType;
            decimal triggerRate = m_Event.details.rate.DecValue;
            Cst.StatusTrigger.StatusTriggerEnum status = Cst.StatusTrigger.StatusTriggerEnum.NA;
            decimal touchRate = 0;

            Control ctrl = pPage.FindControl("TXTTOUCHRATE_" + idE);
            if (null != ctrl)
                touchRate = DecFunc.DecValue(((TextBox)ctrl).Text, Thread.CurrentThread.CurrentCulture);


            if (EventTypeFunc.IsUpTouch(barrierTriggerType) || EventTypeFunc.IsUpNoTouch(barrierTriggerType))
            {
                #region Trigger Up
                if (triggerRate <= touchRate)
                    status = Cst.StatusTrigger.StatusTriggerEnum.ACTIV;
                else
                    status = Cst.StatusTrigger.StatusTriggerEnum.DEACTIV;
                #endregion Trigger Up
            }
            else if (EventTypeFunc.IsDownTouch(barrierTriggerType) || EventTypeFunc.IsDownNoTouch(barrierTriggerType))
            {
                #region Trigger Down
                if (triggerRate < touchRate)
                    status = Cst.StatusTrigger.StatusTriggerEnum.DEACTIV;
                else
                    status = Cst.StatusTrigger.StatusTriggerEnum.ACTIV;
                #endregion Trigger Down
            }
            else if (EventTypeFunc.IsAbove(barrierTriggerType))
            {
                #region Trigger Above
                if (triggerRate <= touchRate)
                    status = Cst.StatusTrigger.StatusTriggerEnum.ACTIV;
                else
                    status = Cst.StatusTrigger.StatusTriggerEnum.DEACTIV;
                #endregion Trigger Above
            }
            else if (EventTypeFunc.IsBelow(barrierTriggerType))
            {
                #region Trigger Below
                if (triggerRate < touchRate)
                    status = Cst.StatusTrigger.StatusTriggerEnum.DEACTIV;
                else
                    status = Cst.StatusTrigger.StatusTriggerEnum.ACTIV;
                #endregion Trigger Below
            }
            if (EventTypeFunc.IsUpIn(barrierTriggerType) || EventTypeFunc.IsRebateUpIn(barrierTriggerType) ||
                EventTypeFunc.IsUpOut(barrierTriggerType) || EventTypeFunc.IsRebateUpOut(barrierTriggerType))
            {
                #region Barrier Up
                if (triggerRate <= touchRate)
                    status = Cst.StatusTrigger.StatusTriggerEnum.ACTIV;
                else
                    status = Cst.StatusTrigger.StatusTriggerEnum.DEACTIV;
                #endregion Barrier Up
            }
            else if (EventTypeFunc.IsDownIn(barrierTriggerType) || EventTypeFunc.IsRebateDownIn(barrierTriggerType) ||
                     EventTypeFunc.IsDownOut(barrierTriggerType) || EventTypeFunc.IsRebateDownOut(barrierTriggerType))
            {
                #region Barrier Down
                if (triggerRate < touchRate)
                    status = Cst.StatusTrigger.StatusTriggerEnum.DEACTIV;
                else
                    status = Cst.StatusTrigger.StatusTriggerEnum.ACTIV;
                #endregion Barrier Down

            }
            ctrl = pPage.FindControl("DDLIDSTTRIGGER_" + idE);
            if (null != ctrl)
                ControlsTools.DDLSelectByValue((DropDownList)ctrl, status.ToString());
        }
        #endregion ActiveDeactiveBarrierTrigger
        #endregion Virtual Methods
    }
    #endregion FxBarrierAndTriggerEvents
    #region FxBarrierEvents
    public class FxBarrierEvents : FxBarrierAndTriggerEvents 
    {
        #region Constructors
        public FxBarrierEvents() { }
        public FxBarrierEvents(TradeActionEventBase pEvent, TradeActionEventBase pEventParent) : base(pEvent, pEventParent) { }
        #endregion Constructors
        #region Methods
        #region IsEventChanged
        public override bool IsEventChanged(TradeActionEventBase pEvent)
        {
            return base.IsEventChanged(pEvent);
        }
        #endregion IsEventChanged
        #region PostedAction
        public object PostedAction(string pKeyAction)
        {
            string keyAction = pKeyAction + Convert.ToInt32(TradeActionCode.TradeActionCodeEnum.Barrier) + "_" + m_Event.eventCode;
            return new BarrierMsg(idE, idStTrigger, ActionDateTime, touchRate.DecValue, note, keyAction);
        }
        #endregion PostedAction
        #region Save
        public override bool Save(Page pPage)
        {
            bool isOk = ValidationRules(pPage);
            if (isOk)
                isOk = base.Save(pPage);
            if (isOk)
                m_Event.m_OriginalAction = (FxBarrierEvents) m_Event.m_Action.Clone();
            m_Event.isValidated = isOk;
            return isOk;
        }
        #endregion Save
        #region ValidationRules
        public override bool ValidationRules(Page pPage)
        {
            bool isOk = true;
            m_Event.validationRulesMessages = new ArrayList();
            DateTime triggerMinDate = m_Event.dtStartPeriod.DateValue;
            DateTime triggerMaxDate = m_Event.dtEndPeriod.DateValue;

            // 20070813 EG  Correction du bug Ticket 15621. Lecture de la date d'action saisie par l'utilisateur
            DateTime dtAction = actionDate.DateValue;
            if (null != pPage.Request.Form["TXTACTIONDATE_" + idE])
                dtAction = new DtFunc().StringToDateTime(pPage.Request.Form["TXTACTIONDATE_" + idE]);
            if ((dtAction.CompareTo(triggerMinDate) < 0) || (dtAction.CompareTo(triggerMaxDate) > 0))
            {
                isOk = false;
                m_Event.validationRulesMessages.Add("Msg_IncorrectBarrierKnockDate");
            }
            if (isOk)
                isOk = base.ValidationRules(pPage);
            return isOk;
        }
        #endregion ValidationRules
        #endregion Methods

        #region Clone
        public override object Clone()
        {
            FxBarrierEvents clone = new FxBarrierEvents
            {
                idE = this.idE,
                actionDate = new EFS_Date(this.actionDate.Value),
                actionTime = new EFS_Time(this.actionTime.Value),
                touchRate = new EFS_Decimal(this.touchRate.DecValue),
                idStTrigger = this.idStTrigger,
                note = this.note
            };
            return clone;
        }
        #endregion Clone

    }
    #endregion FxBarrierEvents

    #region CalculationAgentSettlementRateEvents
    //PL 20100628 
    public class CalculationAgentSettlementRateEvents : ActionEvents
    {
        #region Members
        public EFS_Decimal fixingRate;
        private readonly string fixingEventType;
        #endregion Members
        #region Accessors
        #region CreateHeaderCells_Capture
        private TableHeaderCell[] CreateHeaderCells_Capture
        {
            get
            {
                ArrayList aHeaderCell = new ArrayList
                {
                    TableTools.AddHeaderCell(ResFormActionDate, false, 0, UnitEnum.Pixel, 0, false),
                    TableTools.AddHeaderCell(ResFormValueDate, false, 0, UnitEnum.Pixel, 0, false),
                    TableTools.AddHeaderCell(ResFormExchangeRate, false, 0, UnitEnum.Pixel, 0, true),
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
                    TableTools.AddHeaderCell(ResFormTitlePeriod, false, 150, UnitEnum.Pixel, 2, false),
                    TableTools.AddHeaderCell(ResFormCurrency, false, 0, UnitEnum.Pixel, 0, false),
                    TableTools.AddHeaderCell(ResFormAmount, false, 100, UnitEnum.Pixel, 0, false),
                    TableTools.AddHeaderCell(ResFormPayer, false, 100, UnitEnum.Pixel, 2, false),
                    TableTools.AddHeaderCell(ResFormReceiver, false, 100, UnitEnum.Pixel, 2, false)
                };
                return (TableHeaderCell[])aHeaderCell.ToArray(typeof(TableHeaderCell));
            }
        }
        #endregion CreateHeaderCells_Static
        #region ResFormValueDate
        //PL 20100629 Add QuotationDate
        protected override string ResFormValueDate { get { return Ressource.GetString("QuotationDate"); } }
        #endregion ResFormValueDate
        #endregion Accessors
        #region Constructors
        public CalculationAgentSettlementRateEvents() { }
        public CalculationAgentSettlementRateEvents(TradeActionEventBase pEvent, TradeActionEventBase pEventParent)
            : base(pEvent, pEventParent)
        {
            #region ActionDate
            actionDate.DateValue = OTCmlHelper.GetDateSys(SessionTools.CS);
            #endregion ActionDate
            #region Fixing Display (FXC or FXR)
            ValueDate = new EFS_Date();
            fixingRate = new EFS_Decimal();
            TradeActionEvent[] eventResetFx = CurrentEvent.EventResetFxCustomer;
            //EventClass eventClassFixing = null;
            if (null == eventResetFx)
            {
                eventResetFx = CurrentEvent.EventResetFxRate;
            }
            if (null != eventResetFx && (0 < eventResetFx.Length))
            {
                ValueDate.DateValue = CurrentEvent.EventClassSettlement.dtEvent.DateValue;
                fixingRate.DecValue = eventResetFx[0].valorisation.DecValue;
                fixingEventType = eventResetFx[0].eventType;
                if (eventResetFx[0].details.noteSpecified)
                    note = eventResetFx[0].details.note;
            }
            #endregion Fixing Display (FXC or FXR)
        }
        #endregion Constructors
        #region Methods
        #region AddCells_Capture
        private TableCell[] AddCells_Capture(TradeActionEvent pEventParent)
        {
            ArrayList aCell = new ArrayList();
            m_EventParent = pEventParent;

            aCell.Add(TableTools.AddCell(ActionDate, HorizontalAlign.Center, 80, UnitEnum.Pixel));
            aCell.Add(TableTools.AddCell(FormatedValueDate, HorizontalAlign.Center, 80, UnitEnum.Pixel));
            TableCell td = TableTools.AddCell(fixingRate.CultureValue + " [" + fixingEventType + "]", HorizontalAlign.Right, 80, UnitEnum.Pixel);
            if (null != eventTypeEnum.GetExtendEnumValueByValue(fixingEventType))
            {
                td.ToolTip = eventTypeEnum.GetExtendEnumValueByValue(fixingEventType).ExtValue;
                td.Style.Add(HtmlTextWriterStyle.Cursor, "pointer");
            }
            aCell.Add(td);
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
            aCell.Add(TableTools.AddCell(pEvent.unit, HorizontalAlign.Center));
            aCell.Add(TableTools.AddCell(StrFunc.FmtDecimalToCurrentCulture(pEvent.valorisation.DecValue), HorizontalAlign.Right));
            aCell.Add(TableTools.AddCell(pEvent.payer, HorizontalAlign.NotSet));
            aCell.Add(TableTools.AddCell(pEvent.payerBook, HorizontalAlign.NotSet));
            aCell.Add(TableTools.AddCell(pEvent.receiver, HorizontalAlign.NotSet));
            aCell.Add(TableTools.AddCell(pEvent.receiverBook, HorizontalAlign.NotSet));
            aCell.AddRange(AddCells_Capture(pEventParent));
            return (TableCell[])aCell.ToArray(typeof(TableCell));
        }
        #endregion AddCells_Static
        #region Clone
        // EG 20160404 Migration vs2013
        public override object Clone()
        {
            CalculationAgentSettlementRateEvents clone = new CalculationAgentSettlementRateEvents
            {
                idE = this.idE,
                actionDate = new EFS_Date(this.actionDate.Value),
                actionTime = new EFS_Time(this.actionTime.Value),
                fixingRate = new EFS_Decimal(this.fixingRate.DecValue),
                note = this.note
            };
            return clone;
        }
        #endregion Clone
        #region CreateControlCurrentAction
        public TableRow[] CreateControlCurrentAction
        {
            get
            {
                ArrayList aTableRow = new ArrayList();
                #region ActionDate
                #region DateMin / DateMax
                string startPeriod = DtFunc.DateTimeToString(m_Event.dtStartPeriod.DateValue, DtFunc.FmtShortDate);
                string endPeriod = DtFunc.DateTimeToString(OTCmlHelper.GetDateBusiness(SessionTools.CS), DtFunc.FmtShortDate);
                string errorMess = Ressource.GetString("Msg_IncorrectCustomerActionDate");
                Validator validator = new Validator(errorMess, true, false, ValidationDataType.Date, startPeriod, endPeriod);
                #endregion DateMin / DateMax
                aTableRow.Add(base.CreateControlActionDate(validator));
                #endregion ActionDate
                #region ValueDate
                aTableRow.Add(CreateControlValueDate);
                #endregion ValueDate
                #region FixingRate
                aTableRow.Add(CreateControlFixingRate);
                #endregion FixingRate
                #region Description
                aTableRow.AddRange(base.CreateControlDescription);
                #endregion Description
                return (TableRow[])aTableRow.ToArray(typeof(TableRow));
            }
        }
        #endregion CreateControlCurrentAction
        #region CreateControlFixingRate
        public TableRow CreateControlFixingRate
        {
            get
            {
                TableRow tr = new TableRow
                {
                    CssClass = "DataGrid_ItemStyle"
                };
                TableCell td = new TableCell();
                ControlGUI controlGUI = new ControlGUI(true, ResFormExchangeRate)
                {
                    Regex = EFSRegex.TypeRegex.RegexFxRateExtend,
                    LblWidth = 100
                };
                string errorMess = Ressource.GetString2("Failure_RangeRate");
                Validator validator = new Validator(errorMess, true, false, ValidationDataType.Double, "0", "9999999999"); ;
                FpMLTextBox txtRate = new FpMLTextBox(null, fixingRate.CultureValue, 150, "Rate", controlGUI, null, false, "TXTFIXINGRATE_" + idE, null,
                    new Validator(controlGUI.Name, true),
                    new Validator(EFSRegex.TypeRegex.RegexFxRateExtend, controlGUI.Name, true, false),
                    validator);
                GetFormatControlAttribute(txtRate);
                td.Controls.Add(txtRate);
                tr.Cells.Add(td);
                return tr;
            }
        }
        #endregion CreateControlFixingRate
        #region CreateControlValueDate
        public TableRow CreateControlValueDate
        {
            get
            {
                TableRow tr = new TableRow
                {
                    CssClass = "DataGrid_ItemStyle"
                };
                TableCell td = new TableCell();
                ControlGUI controlGUI = new ControlGUI(true, ResFormValueDate)
                {
                    Regex = EFSRegex.TypeRegex.RegexDate,
                    LblWidth = 105
                };
                #region DateMin / DateMax
                string startPeriod = DtFunc.DateTimeToString(m_Event.dtStartPeriod.DateValue, DtFunc.FmtShortDate);
                string endPeriod = DtFunc.DateTimeToString(m_Event.dtEndPeriod.DateValue, DtFunc.FmtShortDate);
                string errorMess = Ressource.GetString2("Failure_RangeDate", startPeriod, endPeriod);
                Validator validator = new Validator(errorMess, true, false, ValidationDataType.Date, startPeriod, endPeriod);
                #endregion DateMin / DateMax
                FpMLCalendarBox txtDate = new FpMLCalendarBox(null, ValueDate.DateValue, "ValueDate", controlGUI, null, "TXTVALUEDATE_" + idE,
                    new Validator("ValueDate", true),
                    new Validator(EFSRegex.TypeRegex.RegexDate, "ValueDate", true, false),
                    validator);
                GetFormatControlAttribute(txtDate);
                td.Controls.Add(txtDate);
                tr.Cells.Add(td);
                return tr;
            }
        }

        #endregion CreateControlValueDate
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
        #region IsEventChanged
        //20090129 PL/EG Il faudra voir si on peut affiner pour voir une donnée a réellement été modifié par le user...
        public override bool IsEventChanged(TradeActionEventBase pEvent)
        {
            bool isEventChanged = base.IsEventChanged(pEvent);
            if (false == isEventChanged)
            {
                isEventChanged = true;
            }
            return isEventChanged;
        }
        #endregion IsEventChanged
        #region PostedAction
        public object PostedAction(string pKeyAction)
        {
            string keyAction = pKeyAction + Convert.ToInt32(TradeActionCode.TradeActionCodeEnum.CustomerSettlementRate) + "_" + m_Event.eventType;
            return new CustomerSettlementRateMsg(System.Math.Abs(idE), ActionDateTime, ValueDate.DateValue, fixingRate.DecValue, note, keyAction);
        }
        #endregion PostedAction
        #region Save
        public override bool Save(Page pPage)
        {
            m_Event.m_OriginalAction = (CalculationAgentSettlementRateEvents)m_Event.m_Action.Clone();
            bool isOk = ValidationRules(pPage);
            if (isOk)
            {
                isOk = base.Save(pPage);
                if (isOk)
                {
                    if (null != pPage.Request.Form["TXTVALUEDATE_" + idE])
                        ValueDate.DateValue = new DtFunc().StringToDateTime(pPage.Request.Form["TXTVALUEDATE_" + idE]);

                    if (null != pPage.Request.Form["TXTFIXINGRATE_" + idE])
                        fixingRate.DecValue = DecFunc.DecValue(pPage.Request.Form["TXTFIXINGRATE_" + idE], Thread.CurrentThread.CurrentCulture);
                }
            }
            m_Event.isValidated = isOk;
            return isOk;
        }
        #endregion Save
        #region ValidationRules
        public override bool ValidationRules(Page pPage)
        {
            bool isOk = true;
            m_Event.validationRulesMessages = new ArrayList();
            DateTime valueMinDate = m_Event.dtStartPeriod.DateValue;
            DateTime valueMaxDate = m_Event.dtEndPeriod.DateValue;

            //PL 20100626 Debug
            DateTime dtAction = actionDate.DateValue;
            if (null != pPage.Request.Form["TXTACTIONDATE_" + idE])
                dtAction = new DtFunc().StringToDateTime(pPage.Request.Form["TXTACTIONDATE_" + idE]);
            //if (0 <= DateTime.Compare(actionDate.DateValue, valueMinDate))
            if (DateTime.Compare(valueMinDate, dtAction) > 0)
            {
                isOk = false;
                m_Event.validationRulesMessages.Add("Msg_IncorrectCustomerActionDate");
            }
            //if (0 <= DateTime.Compare(actionDate.DateValue, valueMinDate))
            if (DateTime.Compare(dtAction, valueMaxDate) > 0)
            {
                isOk = false;
                m_Event.validationRulesMessages.Add("Msg_IncorrectCustomerActionDate");
            }

            DateTime dtValue = ValueDate.DateValue;
            if (null != pPage.Request.Form["TXTVALUEDATE_" + idE])
                dtValue = new DtFunc().StringToDateTime(pPage.Request.Form["TXTVALUEDATE_" + idE]);
            //if ((0 <= DateTime.Compare(valueDate.DateValue, valueMinDate)) && (0 <= DateTime.Compare(valueMaxDate, valueDate.DateValue)))
            if ((DateTime.Compare(valueMinDate, dtValue)) > 0 && (DateTime.Compare(dtValue, valueMaxDate) > 0))
            {
                isOk = false;
                m_Event.validationRulesMessages.Add("Msg_IncorrectCustomerValueDate");
            }

            decimal decFixingRate = fixingRate.DecValue;
            if (null != pPage.Request.Form["TXTFIXINGRATE_" + idE])
                decFixingRate = DecFunc.DecValue(pPage.Request.Form["TXTFIXINGRATE_" + idE], Thread.CurrentThread.CurrentCulture);
            //if (0 < fixingRate.DecValue)
            if (decFixingRate <= 0)
            {
                isOk = false;
                m_Event.validationRulesMessages.Add("Msg_IncorrectCustomerSettlementRate");
            }

            if (isOk)
                isOk = base.ValidationRules(pPage);
            return isOk;
        }
        #endregion ValidationRules
        #endregion Methods
    }
    #endregion CalculationAgentSettlementRateEvents
    #region FX_ExerciseEvents
    public class FX_ExerciseEvents : FX_AbandonAndExerciseEvents
    {
        #region Members
        protected string m_QuoteSide;
        protected string m_QuoteTiming;
        protected KeyAssetFxRate m_PrimaryKeyAsset;
        protected bool m_PrimaryIdAssetSpecified;
        protected int m_PrimaryIdAsset;
        protected bool m_SecondaryKeyAssetSpecified;
        protected KeyAssetFxRate m_SecondaryKeyAsset;
        protected bool m_SecondaryIdAssetSpecified;
        protected int m_SecondaryIdAsset;
        protected EFS_FxAverageRateObservationDates m_rateObservationDates;
        protected FxOptionTypeEnum m_FxOptionType;
        protected AmountPayerReceiverInfo m_Settlement;
        #endregion Members
        #region Accessors
        #region AddCells_Capture
        public override TableCell[] AddCells_Capture
        {
            get
            {
                ArrayList aCell = new ArrayList();
                aCell.AddRange(base.AddCells_Capture);
                aCell.Add(TableTools.AddCell(SettlementType.ToString(), HorizontalAlign.Center));

                if (IsCashSettlement)
                {
                    aCell.Add(TableTools.AddCell(StrFunc.FmtDecimalToCurrentCulture(SettlementAmount.DecValue), HorizontalAlign.Right, 80, UnitEnum.Pixel));
                    aCell.Add(TableTools.AddCell(SettlementCurrency, HorizontalAlign.Center));
                }
                TableCell td = new TableCell();
                ArrayList aCell2 = new ArrayList();
                Table table = new Table();
                TableRow tr = new TableRow();
                table.CssClass = "subActionDataGrid";
                table.CellPadding = 0;
                table.CellSpacing = 0;
                table.Height = Unit.Percentage(100);
                tr.CssClass = m_Event.GetRowClass(table.CssClass);


                string rateSource = FxFixing.PrimaryRateSource.RateSource.Value;
                if (StrFunc.IsEmpty(rateSource) && (null != m_PrimaryKeyAsset))
                    rateSource = m_PrimaryKeyAsset.PrimaryRateSrc;
                if (StrFunc.IsEmpty(rateSource) && (null != m_SecondaryKeyAsset))
                    rateSource = m_SecondaryKeyAsset.PrimaryRateSrc;

                aCell2.Add(TableTools.AddCell(ResFormRateSource, true));
                aCell2.Add(TableTools.AddCell(rateSource, HorizontalAlign.NotSet));


                if ((IsFxAverageOption && (false == IsFxAverageStrikeOption)) || IsCappedCallOrFlooredPutOption)
                {
                    tr.Cells.AddRange((TableCell[])aCell2.ToArray(typeof(TableCell)));
                    table.Rows.Add(tr);
                    aCell2 = new ArrayList();
                    tr = new TableRow
                    {
                        CssClass = m_Event.GetRowClass(table.CssClass)
                    };
                    if (IsCappedCallOrFlooredPutOption)
                        aCell2.Add(TableTools.AddCell((IsFxCapBarrierOption ? ResFormCapRate : ResFormFloorRate), true));
                    else
                        aCell2.Add(TableTools.AddCell(ResFormTouchRate, true));

                    string label = SettlementRate.Second.CultureValue + Cst.HTMLSpace;
                    if (IsFxAverageOption && (false == IsFxAverageStrikeOption))
                        label += StrikeQuoteBasis(SettlementRateQuoteBasis, true);
                    else
                        label += QuoteBasisValue();
                    label += Cst.HTMLSpace;
                    aCell2.Add(TableTools.AddCell(label + AverageRateFormulaName, HorizontalAlign.Right));
                }
                else
                {
                    aCell2.Add(TableTools.AddCell(ResFormTouchRate, true));
                    aCell2.Add(TableTools.AddCell(SettlementRate.Second.CultureValue, HorizontalAlign.Right));
                    tr.Cells.AddRange((TableCell[])aCell2.ToArray(typeof(TableCell)));
                    table.Rows.Add(tr);
                    aCell2 = new ArrayList();
                    tr = new TableRow
                    {
                        CssClass = m_Event.GetRowClass(table.CssClass)
                    };
                    aCell2.Add(TableTools.AddCell(ResFormFixingDate, true));
                    aCell2.Add(TableTools.AddCell(FixingDate, HorizontalAlign.Center));
                    aCell2.Add(TableTools.AddCell(ResFormActionTime, true));
                    aCell2.Add(TableTools.AddCell(FxFixing.FixingTime.HourMinuteTime.Value, HorizontalAlign.Right));
                }
                tr.Cells.AddRange((TableCell[])aCell2.ToArray(typeof(TableCell)));
                table.Rows.Add(tr);
                td.Controls.Add(table);
                aCell.Add(td);
                aCell.Add(TableTools.AddCell(StrikePrice.CultureValue + AverageStrikeFormulaName, HorizontalAlign.Right));
                aCell.Add(TableTools.AddCell(note, HorizontalAlign.NotSet, 100, UnitEnum.Percentage, true, false, false));
                aCell.Add(TableTools.AddCell(Cst.HTMLSpace, HorizontalAlign.Center, 0, UnitEnum.Pixel));


                return (TableCell[])aCell.ToArray(typeof(TableCell));
            }
        }
        #endregion AddCells_Capture
        #region CreateHeaderCells_Capture
        public override TableHeaderCell[] CreateHeaderCells_Capture
        {
            get
            {
                ArrayList aHeaderCell = new ArrayList();
                aHeaderCell.AddRange(base.CreateHeaderCells_Capture);
                if (IsCashSettlement)
                    aHeaderCell.Add(TableTools.AddHeaderCell(ResFormAmount, false, 0, UnitEnum.Pixel, 4, false));
                else
                {
                    aHeaderCell.Add(TableTools.AddHeaderCell(ResFormAmount, false, 0, UnitEnum.Pixel, 0, false));
                    aHeaderCell.Add(TableTools.AddHeaderCell(ResFormSettlementMethod, false, 0, UnitEnum.Pixel, 0, true));
                }
                aHeaderCell.Add(TableTools.AddHeaderCell(ResFormSettlementRate, false, 0, UnitEnum.Pixel, 0, true));
                aHeaderCell.Add(TableTools.AddHeaderCell(ResFormStrike, false, 0, UnitEnum.Pixel, 0, false));
                aHeaderCell.Add(TableTools.AddHeaderCell(ResFormTitleNoteEvents, false, 0, UnitEnum.Pixel, 2, false));
                return (TableHeaderCell[])aHeaderCell.ToArray(typeof(TableHeaderCell));
            }
        }
        #endregion CreateHeaderCells_Capture

        // TO CONTROL
        #region AverageFormulaName
        /*
        private string AverageFormulaName
        {
            get
            {
                string formula = AverageRateFormulaName;
                if (StrFunc.IsEmpty(formula))
                    formula = AverageStrikeFormulaName;
                return formula;
            }
        }
        */
        #endregion AverageFormulaName
        #region AverageRateFormulaName
        private string AverageRateFormulaName
        {
            get
            {
                string formula = string.Empty;
                if (FxOptionTypeEnum.AverageRate_Geometric == m_FxOptionType)
                    formula = " (" + ResFormGeometricAverage + ")";
                else if (FxOptionTypeEnum.AverageRate_Arithmetic == m_FxOptionType)
                    formula = " (" + ResFormArithmeticAverage + ")";
                return formula;
            }
        }
        #endregion AverageRateFormulaName
        #region AverageStrikeFormulaName
        private string AverageStrikeFormulaName
        {
            get
            {
                string formula = string.Empty;
                if (FxOptionTypeEnum.AverageStrike_Geometric == m_FxOptionType)
                    formula = " (" + ResFormGeometricAverage + ")";
                else if (FxOptionTypeEnum.AverageStrike_Arithmetic == m_FxOptionType)
                    formula = " (" + ResFormArithmeticAverage + ")";
                return formula;
            }
        }
        #endregion AverageStrikeFormulaName
        #region Resource in Form
        #region ResFormTitleAbandonExerciseDate
        protected override string ResFormTitleAbandonExerciseDate { get { return Ressource.GetString("ExerciseDate"); } }
        #endregion ResFormTitleAbandonExerciseDate

        #region ResFormArithmeticAverage
        protected virtual string ResFormArithmeticAverage { get { return Ressource.GetString("ArithmeticAverage"); } }
        #endregion ResFormArithmeticAverage
        #region ResFormAutomaticExercise
        protected virtual string ResFormAutomaticExercise { get { return Ressource.GetString("AutomaticExercise"); } }
        #endregion ResFormAutomaticExercise
        #region ResFormCapRate
        protected virtual string ResFormCapRate { get { return Ressource.GetString("CapRate"); } }
        #endregion ResFormCapRate
        #region ResFormFallbackExercise
        protected virtual string ResFormFallbackExercise { get { return Ressource.GetString("FallbackExercise"); } }
        #endregion ResFormFallbackExercise
        #region ResFormFloorRate
        protected virtual string ResFormFloorRate { get { return Ressource.GetString("FloorRate"); } }
        #endregion ResFormFloorRate
        #region ResFormGeometricAverage
        protected virtual string ResFormGeometricAverage { get { return Ressource.GetString("GeometricAverage"); } }
        #endregion ResFormGeometricAverage
        #region ResFormWeight
        protected virtual string ResFormWeight { get { return Ressource.GetString("WEIGHT"); } }
        #endregion ResFormWeight
        #endregion Resource in Form
        #endregion Accessors
        #region Constructors
        public FX_ExerciseEvents() { }
        public FX_ExerciseEvents(TradeActionEventBase pEvent, TradeActionEventBase pEventParent)
            : base(pEvent, pEventParent)
        {
            SettlementRate = new Pair<Cst.PriceQuoteUnits, EFS_Decimal>(Cst.PriceQuoteUnits.Price,new EFS_Decimal());
            SettlementAmount = new EFS_Decimal();

            #region FxOptionType
            m_FxOptionType = FxOptionTypeEnum.None;
            if (IsFxAverageOption && IsFxAverageStrikeOption)
                m_FxOptionType = (IsGeometricAverage ? FxOptionTypeEnum.AverageStrike_Geometric : FxOptionTypeEnum.AverageStrike_Arithmetic);
            else if (IsFxAverageOption)
                m_FxOptionType = (IsGeometricAverage ? FxOptionTypeEnum.AverageRate_Geometric : FxOptionTypeEnum.AverageRate_Arithmetic);
            else if (IsFxSimpleOption)
                m_FxOptionType = FxOptionTypeEnum.SimpleOption;
            else if (IsFxBarrierOption)
            {
                if (IsCappedCallOrFlooredPutOption)
                {
                    if (IsFxCapBarrierOption)
                        m_FxOptionType = FxOptionTypeEnum.CappedCallBarrierOption;
                    else if (IsFxFloorBarrierOption)
                        m_FxOptionType = FxOptionTypeEnum.FloorPutBarrierOption;
                }
                else
                    m_FxOptionType = FxOptionTypeEnum.BarrierOption;
            }
            #endregion FxOptionType

            if (FxOptionTypeEnum.None != m_FxOptionType)
            {
                if (null != m_Event.EventClassGroupLevel)
                {
                    #region Exercise Date & Time
                    actionDate.DateValue = m_Event.dtEndPeriod.DateValue;
                    actionTime.TimeValue = m_Event.dtEndPeriod.DateValue;
                    #endregion Exercise Date & Time
                    #region ValueDate
                    ValueDate.DateValue = m_Event.EventClassGroupLevel.dtEvent.DateValue;
                    TradeActionEventDetailsBase details;
                    #endregion ValueDate
                    #region Call & Put CurrencyAmount
                    if (EventTypeFunc.IsTotal(m_Event.eventType) || EventTypeFunc.IsPartiel(m_Event.eventType))
                    {
                        #region Total/Partial Exercise
                        CallAmount = SetAmountAndQuantityExercise(EventTypeFunc.CallCurrency, m_CallAmount_TER);
                        PutAmount = SetAmountAndQuantityExercise(EventTypeFunc.PutCurrency, m_PutAmount_TER);

                        if (null != m_SettlementAmount_TER)
                        {
                            SettlementAmount.DecValue = m_SettlementAmount_TER.valorisation.DecValue;
                            SettlementCurrency = m_SettlementAmount_TER.unit;
                            if (null != m_SettlementAmount_TER.details)
                            {
                                details = m_SettlementAmount_TER.details;
                                SettlementRate.Second.DecValue = details.settlementRate.DecValue;
                                if (null != details.dtFixing)
                                {
                                    FxFixing.FixingDate.DateValue = details.dtFixing.DateValue;
                                    FxFixing.FixingTime.HourMinuteTime.TimeValue = details.dtFixing.DateTimeValue;
                                }
                            }
                        }
                        else
                        {
                            if (null != m_Event.details)
                            {
                                details = m_Event.details;
                                SettlementRate.Second.DecValue = details.settlementRate.DecValue;
                                if (null != details.dtFixing)
                                {
                                    FxFixing.FixingDate.DateValue = details.dtFixing.DateValue;
                                    FxFixing.FixingTime.HourMinuteTime.TimeValue = details.dtFixing.DateTimeValue;
                                }
                            }
                            if (null != m_Event.asset)
                                FxFixing.PrimaryRateSource.RateSource.Value = m_Event.asset.primaryRateSrc;
                        }
                        #endregion Total/Partial Exercise
                    }
                    else
                    {
                        #region Multiple Exercise
                        TradeActionEvent eventCallAmount = CurrentEvent.GetEvent(EventCodeFunc.Intermediary, EventTypeFunc.CallCurrency);
                        if (null == eventCallAmount)
                            eventCallAmount = CurrentEvent.GetEvent(EventCodeFunc.Termination, EventTypeFunc.CallCurrency);
                        CallAmount = SetAmountAndQuantityExercise(EventTypeFunc.CallCurrency, eventCallAmount);

                        TradeActionEvent eventPutAmount = CurrentEvent.GetEvent(EventCodeFunc.Intermediary, EventTypeFunc.PutCurrency);
                        if (null == eventPutAmount)
                            eventPutAmount = CurrentEvent.GetEvent(EventCodeFunc.Termination, EventTypeFunc.PutCurrency);
                        PutAmount = SetAmountAndQuantityExercise(EventTypeFunc.PutCurrency, eventPutAmount);

                        TradeActionEvent eventSettlementAmount = CurrentEvent.EventIntermediarySettlementCurrencyAmount;
                        if (null == eventSettlementAmount)
                            eventSettlementAmount = CurrentEvent.EventTerminationSettlementCurrencyAmount;

                        if (null != eventSettlementAmount)
                        {
                            SettlementAmount.DecValue = eventSettlementAmount.valorisation.DecValue;
                            SettlementCurrency = eventSettlementAmount.unit;
                            if (null != eventSettlementAmount.details)
                            {
                                SettlementRate.Second.DecValue = eventSettlementAmount.details.settlementRate.DecValue;
                                FxFixing.FixingDate.DateValue = eventSettlementAmount.details.dtFixing.DateValue;
                                FxFixing.FixingTime.HourMinuteTime.TimeValue = eventSettlementAmount.details.dtFixing.DateValue;
                            }
                        }
                        else
                        {
                            if (null != m_Event.details)
                            {
                                details = m_Event.details;
                                SettlementRate.Second.DecValue = details.settlementRate.DecValue;
                                if (null != details.dtFixing)
                                {
                                    FxFixing.FixingDate.DateValue = details.dtFixing.DateValue;
                                    FxFixing.FixingTime.HourMinuteTime.TimeValue = details.dtFixing.DateTimeValue;
                                }
                            }
                            if (null != m_Event.asset)
                                FxFixing.PrimaryRateSource.RateSource.Value = m_Event.asset.primaryRateSrc;
                        }
                        #endregion Multiple Exercise
                    }
                    #endregion Call & Put CurrencyAmount
                }
                else
                {
                    #region Exercise Date & Time
                    if (IsEuropean)
                    {
                        actionDate.DateValue = ExpiryDate;
                        actionTime.TimeValue = ExpiryDate;
                    }
                    else
                    {
                        // FI 20200904 [XXXXX] call OTCmlHelper.GetDateSys  
                        //actionDate.DateValue = OTCmlHelper.GetDateBusiness(SessionTools.CS);
                        actionDate.DateValue = OTCmlHelper.GetDateSys(SessionTools.CS);
                        if (0 < actionDate.DateValue.CompareTo(ExpiryDate))
                            actionDate.DateValue = ExpiryDate;
                        actionTime.TimeValue = actionDate.DateTimeValue;
                    }
                    #endregion Exercise Date & Time
                    #region ValueDate
                    if (IsCappedCallOrFlooredPutOption)
                        ValueDate.DateValue = (PayoutEnum.Deferred == PayoutStyle ? ExpiryDate : actionDate.DateValue);
                    else
                    {
                        IProduct product = (IProduct)CurrentProduct;
                        IOffset offset = product.ProductBase.CreateOffset(PeriodEnum.D, 2, DayTypeEnum.Business);
                        IBusinessCenters businessCenters = ((IOffset)offset).GetBusinessCentersCurrency(SessionTools.CS, null, CallAmount.Currency, PutAmount.Currency);
                        ValueDate.DateValue = Tools.ApplyOffset(SessionTools.CS, ExpiryDate, offset, businessCenters);
                    }
                    #endregion ValueDate
                    #region Call & Put CurrencyAmount
                    if (EventTypeFunc.IsMultiple(m_Event.eventType))
                    {
                        #region Multiple exercise
                        CallAmount = SetAmountAndQuantityExercise(EventTypeFunc.CallCurrency, StartCallAmount, m_CallAmount_INT, m_CallAmount_TER);
                        PutAmount = SetAmountAndQuantityExercise(EventTypeFunc.PutCurrency, StartPutAmount, m_PutAmount_INT, m_PutAmount_TER);
                        #endregion Multiple exercise
                    }
                    else
                    {
                        #region Total/Partial exercise
                        CallAmount = SetAmountAndQuantityExercise(EventTypeFunc.CallCurrency, StartCallAmount);
                        PutAmount = SetAmountAndQuantityExercise(EventTypeFunc.PutCurrency, StartPutAmount);
                        #endregion Total/Partial exercise
                    }
                    #endregion Call & Put CurrencyAmount
                    #region CashSettlement
                    if (IsFxAverageStrikeOption || false == IsCashSettlement)
                        FxFixing.FixingDate = new EFS_Date(actionDate.Value);

                    if (IsFxAverageOption)
                        PreCalculAverageOptionInTheMoney();
                    else if (EventCodeFunc.IsSimpleOption(m_EventParent.eventCode) || EventCodeFunc.IsBarrierOption(m_EventParent.eventCode))
                        PreCalculRegularOptionInTheMoney();

                    IsInTheMoney = m_EventParent.IsInTheMoney(SettlementFxRateValue(SettlementRate.Second.DecValue), StrikePriceValue, QuoteBasis);
                    SettlementAmount.DecValue = base.CalculFormulaOptionInTheMoney(SessionTools.CS, CallAmount.Amount, PutAmount.Amount,
                            SettlementFxRateValue(SettlementRate.Second.DecValue), StrikePriceValue);
                    decimal amount = base.CalculFormulaOptionInTheMoney(SessionTools.CS, CallAmount.Amount, PutAmount.Amount,
                            SettlementFxRateValue(SettlementRate.Second.DecValue), StrikePriceValue);

                    PayerReceiverInfoDet payer = new PayerReceiverInfoDet(PayerReceiverEnum.Payer, Seller);
                    PayerReceiverInfoDet receiver = new PayerReceiverInfoDet(PayerReceiverEnum.Receiver, Buyer);
                    // EG 20150706 [21021]
                    m_Settlement = new AmountPayerReceiverInfo(null, amount, SettlementCurrency, payer, receiver);
                    #endregion CashSettlement
                }
            }
        }
        #endregion Constructors
        #region Methods
        #region Save
        /// <revision>
        ///     <version>1.1.0 build 46</version><date>20060628</date><author>EG</author>
        ///     <EurosysSupport>N° 10270</EurosysSupport>
        ///     <comment>
        ///     La méthode retourne désormais un boolean indiquant la validité
        ///     de la saisie de l'exercice
        ///     </comment>
        /// </revision>
        public override bool Save(Page pPage)
        {
            m_Event.validationRulesMessages = new ArrayList();
            bool isOk = ValidationRules(pPage);
            if (isOk)
            {
                isOk = base.Save(pPage);
                if (isOk)
                {
                    if (null != pPage.Request.Form["DDLEXERCISETYPE_" + idE])
                    {
                        abandonExerciseType = pPage.Request.Form["DDLEXERCISETYPE_" + idE];
                        m_Event.eventType = abandonExerciseType;
                    }
                    if (null != pPage.Request.Form["TXTRATESOURCE_" + idE])
                        m_PrimaryKeyAsset.PrimaryRateSrc = pPage.Request.Form["TXTRATESOURCE_" + idE];
                    if (null != pPage.Request.Form["TXTSECONDARYRATESOURCE_" + idE] && m_SecondaryKeyAssetSpecified)
                        m_SecondaryKeyAsset.PrimaryRateSrc = pPage.Request.Form["TXTSECONDARYRATESOURCE_" + idE];
                    if (null != pPage.Request.Form["TXTSETTLEMENTAMOUNT_" + idE])
                        SettlementAmount.DecValue = DecFunc.DecValue(pPage.Request.Form["TXTSETTLEMENTAMOUNT_" + idE],
                            Thread.CurrentThread.CurrentCulture);
                    if (null != pPage.Request.Form["TXTFIXINGDATE_" + idE])
                        FxFixing.FixingDate.DateValue = new DtFunc().StringToDateTime(pPage.Request.Form["TXTFIXINGDATE_" + idE]);
                    if (null != pPage.Request.Form["TXTFIXINGTIME_" + idE])
                        FxFixing.FixingTime.HourMinuteTime.Value = pPage.Request.Form["TXTFIXINGTIME_" + idE];
                    if (null != pPage.Request.Form["TXTSETTLEMENTRATE_" + idE])
                        SettlementRate.Second.DecValue = DecFunc.DecValue(pPage.Request.Form["TXTSETTLEMENTRATE_" + idE],
                            Thread.CurrentThread.CurrentCulture);
                    if (null != pPage.Request.Form["TXTSETTLEMENTAMOUNT_" + idE])
                        SettlementAmount.DecValue = DecFunc.DecValue(pPage.Request.Form["TXTSETTLEMENTAMOUNT_" + idE],
                            Thread.CurrentThread.CurrentCulture);
                }
            }
            m_Event.isValidated = isOk;
            return isOk;
        }
        #endregion Save
        #region ValidationRules
        public override bool ValidationRules(Page pPage)
        {
            bool isOk = true;
            decimal callAmount = 0;
            decimal putAmount = 0;
            if (null != pPage.Request.Form["DDLEXERCISETYPE_" + idE])
                abandonExerciseType = pPage.Request.Form["DDLEXERCISETYPE_" + idE];

            if (null != pPage.Request.Form["TXTCALLAMOUNT_" + idE])
                callAmount = DecFunc.DecValue(pPage.Request.Form["TXTCALLAMOUNT_" + idE], Thread.CurrentThread.CurrentCulture);
            if (null != pPage.Request.Form["TXTPUTAMOUNT_" + idE])
                putAmount = DecFunc.DecValue(pPage.Request.Form["TXTPUTAMOUNT_" + idE], Thread.CurrentThread.CurrentCulture);

            decimal callAmount_STA = StartCallAmount.valorisation.DecValue;
            decimal putAmount_STA = StartPutAmount.valorisation.DecValue;
            decimal callAmount_INT = 0;
            decimal putAmount_INT = 0;
            if (EventTypeFunc.IsMultiple(abandonExerciseType))
            {
                decimal remainingAmount;
                #region CallCurrencyAmount control
                if (null != m_CallAmount_INT)
                {
                    foreach (TradeActionEvent item in m_CallAmount_INT)
                        callAmount_INT += item.valorisation.DecValue;

                    remainingAmount = callAmount_STA - callAmount_INT;
                    isOk = (0 < callAmount) && (callAmount <= remainingAmount);
                }
                else
                    isOk = (callAmount < callAmount_STA);
                #endregion CallCurrencyAmount control
                if (isOk)
                {
                    #region PutCurrencyAmount control
                    if (null != m_PutAmount_INT)
                    {
                        foreach (TradeActionEvent item in m_PutAmount_INT)
                            putAmount_INT += item.valorisation.DecValue;

                        remainingAmount = putAmount_STA - putAmount_INT;
                        isOk = (0 < putAmount) && (putAmount <= remainingAmount);
                    }
                    else
                        isOk = (putAmount < putAmount_STA);
                    #endregion PutCurrencyAmount control
                }
                if (false == isOk)
                    m_Event.validationRulesMessages.Add("Msg_IncorrectMultipleExerciseCallPutAmount");

            }
            else if (EventTypeFunc.IsPartiel(abandonExerciseType))
            {
                isOk = (0 < callAmount) && (callAmount < callAmount_STA) && (0 < putAmount) && (putAmount < putAmount_STA);
                if (false == isOk)
                    m_Event.validationRulesMessages.Add("Msg_IncorrectPartialExerciseCallPutAmount");
            }
            if (isOk)
                isOk = base.ValidationRules(pPage);
            return isOk;
        }
        #endregion ValidationRules

        #region CalculCashSettlementMoneyAmount
        public new void CalculCashSettlementMoneyAmount(PageBase pPage, string pControlId)
        {
            base.CalculCashSettlementMoneyAmount(pPage, pControlId);
            EnableAmountCurrency(pPage);
        }
        #endregion CalculCashSettlementMoneyAmount
        #region CreateControlActionDate
        public new TableRow CreateControlActionDate
        {
            get
            {
                ControlGUI controlGUI = new ControlGUI();

                if (DtFunc.IsDateTimeEmpty(actionDate.DateValue))
                {
                    actionDate.DateTimeValue = ExpiryDate;
                    actionTime.TimeValue = actionDate.DateTimeValue;
                }

                TableRow tr;
                if (IsAmerican)
                {
                    string startPeriod = DtFunc.DateTimeToString(exerciseDates[0].dtStartPeriod.DateValue, DtFunc.FmtShortDate);
                    string endPeriod = DtFunc.DateTimeToString(exerciseDates[0].dtEndPeriod.DateValue, DtFunc.FmtShortDate);
                    string errorMess = Ressource.GetString2("Failure_RangeDate", startPeriod, endPeriod);
                    Validator validator = new Validator(errorMess, true, false, ValidationDataType.Date, startPeriod, endPeriod);
                    tr = base.CreateControlActionDate(validator);
                }
                else
                {
                    tr = new TableRow
                    {
                        CssClass = "DataGrid_ItemStyle"
                    };
                    TableCell td = new TableCell();
                    controlGUI.Name = ResFormActionDate;
                    controlGUI.LblWidth = 100;
                    FpMLDropDownList ddlActionDate = new FpMLDropDownList(null, actionDate.Value, "DDLACTIONDATE_" + idE, 105, controlGUI, lstExerciseDate);
                    td.Controls.Add(ddlActionDate);
                    tr.Cells.Add(td);
                }
                return tr;
            }
        }
        #endregion CreateControlActionDate
        #region CreateControlCurrentAction
        public TableRow[] CreateControlCurrentAction
        {
            get
            {
                ArrayList aTableRow = new ArrayList();
                aTableRow.Add(CreateControlTitleSeparator(ResFormTitleExerciseEvents, false));
                #region ActionDate
                aTableRow.Add(CreateControlActionDate);
                #endregion ActionDate
                #region ActionTime
                aTableRow.Add(base.CreateControlActionTime(IsEuropean));
                #endregion ActionTime
                #region ValueDate
                aTableRow.Add(base.CreateControlValueDate);
                #endregion ValueDate
                #region ExerciseType
                aTableRow.Add(CreateControlExerciseType);
                #endregion ExerciseType
                #region Amounts
                aTableRow.Add(CreateControlTitleSeparator(ResFormAmount, false));
                #region CallCurrencyAmount
                aTableRow.Add(base.CreateControlCurrencyAmount(OptionTypeEnum.Call, EventTypeFunc.IsTotal(abandonExerciseType)));
                #endregion CallCurrencyAmount
                #region PutCurrencyAmount
                aTableRow.Add(base.CreateControlCurrencyAmount(OptionTypeEnum.Put, EventTypeFunc.IsTotal(abandonExerciseType)));
                #endregion PutCurrencyAmount
                #endregion Amounts
                #region Settlement Complements
                aTableRow.AddRange(CreateControlSettlementComplements);
                #endregion Settlement Complements
                #region Description
                aTableRow.AddRange(base.CreateControlDescription);
                #endregion Description
                return (TableRow[])aTableRow.ToArray(typeof(TableRow));
            }
        }
        #endregion CreateControlCurrentAction
        #region CreateControlExerciseType
        public TableRow CreateControlExerciseType
        {
            get
            {
                TableRow tr = new TableRow();
                TableCell td = new TableCell();
                ControlGUI controlGUI = new ControlGUI();
                tr.CssClass = "DataGrid_ItemStyle";
                controlGUI.Name = ResFormExerciseType;
                controlGUI.LblWidth = 105;
                FpMLDropDownList ddlExerciseType = new FpMLDropDownList(null, abandonExerciseType, "DDLEXERCISETYPE_" + idE, 150, controlGUI, lstExerciseType);
                StringBuilder sb = new StringBuilder();
                sb.AppendFormat("javascript:__doPostBack('{0}','{1}');", ddlExerciseType.ID,
                    TradeActionMode.TradeActionModeEnum.CalculCurrencyAmountExercise);
                ddlExerciseType.Attributes.Add("onchange", sb.ToString());

                td.Controls.Add(ddlExerciseType);
                td.Controls.Add(new LiteralControl(Cst.HTMLSpace + SettlementType));
                if (isAutomaticExercise)
                    td.Controls.Add(new LiteralControl(Cst.HTMLSpace + ResFormAutomaticExercise));
                if (isFallbackExercise)
                    td.Controls.Add(new LiteralControl(Cst.HTMLSpace + ResFormFallbackExercise));
                tr.Cells.Add(td);
                return tr;
            }
        }
        #endregion CreateControlActionDate
        #region CreateControlSettlementComplements
        // EG 20230711 [XXXXX] Del IsInTheMoney = false; [Demo BFF]
        public TableRow[] CreateControlSettlementComplements
        {
            get
            {
                ArrayList aTableRow = new ArrayList();
                StringBuilder sb = new StringBuilder();
                string title = IsCashSettlement ? ResFormTitleCashSettlement : ResFormTitlePhysicalSettlement;
                bool isReadOnly = (IsCashSettlement || IsFxAverageOption);
                #region Title
                if (false == IsInTheMoney)
                    title += " [Out-of-the money]";
                aTableRow.Add(CreateControlTitleSeparator(title, "LBLTITLE_" + idE, false));
                #endregion Title

                TableRow tr = new TableRow();
                TableCell td = new TableCell();
                ControlGUI controlGUI = new ControlGUI();
                #region Primary Rate source
                tr.CssClass = "DataGrid_ItemStyle";
                controlGUI.Name = ResFormRateSource;
                controlGUI.Regex = EFSRegex.TypeRegex.None;
                controlGUI.LblWidth = 100;
                string rateSource = FxFixing.PrimaryRateSource.RateSource.Value;
                if (StrFunc.IsEmpty(rateSource) && (null != m_PrimaryKeyAsset))
                    rateSource = m_PrimaryKeyAsset.PrimaryRateSrc;
                if (StrFunc.IsEmpty(rateSource) && (null != m_SecondaryKeyAsset))
                    rateSource = m_SecondaryKeyAsset.PrimaryRateSrc;

                FpMLTextBox txtRateSource = new FpMLTextBox(null, rateSource, 350, "RateSource", controlGUI, null, isReadOnly, "TXTRATESOURCE_" + idE, null)
                {
                    CssClass = EFSCssClass.CaptureOptional
                };
                sb.AppendFormat("javascript:__doPostBack('{0}','{1}');", txtRateSource.ID, TradeActionMode.TradeActionModeEnum.SearchQuote_FxRate);
                txtRateSource.Attributes.Add("onchange", sb.ToString());
                td.Controls.Add(txtRateSource);
                tr.Cells.Add(td);
                aTableRow.Add(tr);
                #endregion Primary Rate source
                #region Secondary Rate source
                if (FxFixing.SecondaryRateSourceSpecified)
                {
                    tr = new TableRow
                    {
                        CssClass = "DataGrid_ItemStyle"
                    };
                    td = new TableCell();
                    controlGUI = new ControlGUI
                    {
                        Name = ResFormSecondaryRateSource,
                        Regex = EFSRegex.TypeRegex.None,
                        LblWidth = 100
                    };
                    FpMLTextBox txtRateSource2 = new FpMLTextBox(null, FxFixing.SecondaryRateSource.RateSource.Value, 350,
                        "SecondaryRateSource", controlGUI, null, isReadOnly, "TXTSECONDARYRATESOURCE_" + idE, null)
                    {
                        CssClass = "txtCaptureOptional"
                    };
                    sb.AppendFormat("javascript:__doPostBack('{0}','{1}');", txtRateSource2.ID, TradeActionMode.TradeActionModeEnum.SearchQuote_FxRate);
                    txtRateSource2.Attributes.Add("onchange", sb.ToString());
                    td.Controls.Add(txtRateSource2);
                    tr.Cells.Add(td);
                    aTableRow.Add(tr);
                }
                #endregion Secondary Rate source

                if (IsFxAverageOption)
                {
                    if (IsFxAverageStrikeOption)
                    {
                        #region Fixing Date/Time
                        aTableRow.Add(CreateFixingDateTime(actionDate.DateValue, actionTime.TimeValue));
                        #endregion Fixing Date/Time
                        #region SettlementRate
                        aTableRow.Add(CreateSettlementRate);
                        #endregion SettlementRate
                        #region ObservedDates
                        aTableRow.Add(CreateObservedDates);
                        #endregion ObservedDates
                    }
                    else
                    {
                        #region ObservedDates
                        aTableRow.Add(CreateObservedDates);
                        #endregion ObservedDates
                        #region SettlementRate
                        aTableRow.Add(CreateSettlementRate);
                        #endregion SettlementRate
                    }
                }
                else
                {
                    #region Fixing Date/Time
                    aTableRow.Add(CreateFixingDateTime(FxFixing.FixingDate.DateValue, FxFixing.FixingTime.HourMinuteTime.TimeValue));
                    #endregion Fixing Date/Time
                    #region SettlementRate
                    aTableRow.Add(CreateSettlementRate);
                    #endregion SettlementRate
                }
                #region StrikePrice
                tr = new TableRow
                {
                    CssClass = "DataGrid_ItemStyle"
                };
                td = new TableCell();
                controlGUI = new ControlGUI
                {
                    Name = ResFormStrike,
                    Regex = EFSRegex.TypeRegex.RegexFxRateExtend,
                    LblWidth = 100
                };
                FpMLTextBox txtStrikePrice = new FpMLTextBox(null, StrikePrice.CultureValue, 150, "StrikePrice", controlGUI,
                    null, true, "TXTSTRIKEPRICE_" + idE, null,
                    new Validator("StrikePrice", true),
                    new Validator(EFSRegex.TypeRegex.RegexFxRateExtend, "StrikePrice", true, false));
                GetFormatControlAttribute(txtStrikePrice);
                td.Controls.Add(txtStrikePrice);
                td.Controls.Add(new LiteralControl(Cst.HTMLSpace + StrikeQuoteBasis(FxStrikePrice.StrikeQuoteBasis)));
                td.Controls.Add(new LiteralControl(Cst.HTMLSpace + AverageStrikeFormulaName));
                tr.Cells.Add(td);
                aTableRow.Add(tr);
                #endregion StrikePrice
                if (IsCashSettlement)
                {
                    #region Settlement amount
                    tr = new TableRow
                    {
                        CssClass = "DataGrid_ItemStyle"
                    };
                    td = new TableCell();
                    controlGUI = new ControlGUI
                    {
                        Name = ResFormAmount,
                        Regex = EFSRegex.TypeRegex.RegexAmountExtend,
                        LblWidth = 100
                    };
                    FpMLTextBox txtSettlementAmount = new FpMLTextBox(null, SettlementAmount.CultureValue, 150, "SettlementAmount", controlGUI,
                        null, true, "TXTSETTLEMENTAMOUNT_" + idE, null,
                        new Validator("SettlementRate", true),
                        new Validator(EFSRegex.TypeRegex.RegexAmountExtend, "SettlementAmount", true, false));
                    GetFormatControlAttribute(txtSettlementAmount);
                    td.Controls.Add(txtSettlementAmount);
                    td.Controls.Add(new LiteralControl(Cst.HTMLSpace + SettlementCurrency));
                    tr.Cells.Add(td);
                    aTableRow.Add(tr);
                    #endregion Settlement amount
                }
                return (TableRow[])aTableRow.ToArray(typeof(TableRow));
            }
        }
        #endregion CreateControlSettlementComplements
        #region CalculCurrencyAmountExercise
        public override void CalculCurrencyAmountExercise(PageBase pPage, string pControlId)
        {
            FormatControl(pPage, pControlId);
            string exerciseType = string.Empty;
            if (null != pPage.Request.Form["DDLEXERCISETYPE_" + idE])
                exerciseType = pPage.Request.Form["DDLEXERCISETYPE_" + idE];

            if (("DDLEXERCISETYPE_" + idE) == pControlId)
            {

                #region Call&PutCurrencyAmount are disabled if exercise size is Total
                Control ctrl = pPage.FindControl("TXTCALLAMOUNT_" + idE);
                decimal amount;
                if (null != ctrl)
                {
                    amount = CurrentCallAmount(exerciseType);
                    ((TextBox)ctrl).Text = StrFunc.FmtDecimalToCurrentCulture(amount.ToString(NumberFormatInfo.InvariantInfo));
                }
                ctrl = pPage.FindControl("TXTPUTAMOUNT_" + idE);
                if (null != ctrl)
                {
                    amount = CurrentPutAmount(exerciseType);
                    ((TextBox)ctrl).Text = StrFunc.FmtDecimalToCurrentCulture(amount.ToString(NumberFormatInfo.InvariantInfo));
                }
                #endregion Call&PutCurrencyAmount are disabled if exercise size is Total
            }
            else
                base.CalculCurrencyAmountExercise(pPage, pControlId);
            #region settlementAmount (and InTheMoney/OutTheMoney) are recalculated
            CalculCashSettlementMoneyAmount(pPage, pControlId);
            #endregion settlementAmount (and InTheMoney/OutTheMoney) are recalculated
        }
        #endregion CalculCurrencyAmountExercise
        #region CreateFixingDateTime
        private TableRow CreateFixingDateTime(DateTime pFixingDate, DateTime pFixingTime)
        {
            #region Fixing Date
            StringBuilder sb = new StringBuilder();
            TableRow tr = new TableRow
            {
                CssClass = "DataGrid_ItemStyle"
            };
            TableCell td = new TableCell();
            ControlGUI controlGUI = new ControlGUI
            {
                Name = ResFormFixingDate,
                Regex = EFSRegex.TypeRegex.RegexDate,
                LblWidth = 105
            };
            FpMLCalendarBox txtFixingDate = new FpMLCalendarBox(null, pFixingDate, "FixingDate", controlGUI, null, "TXTFIXINGDATE_" + idE,
                new Validator("FixingDate", true),
                new Validator(EFSRegex.TypeRegex.RegexDate, "FixingDate", true, false));
            sb.AppendFormat("javascript:__doPostBack('{0}','{1}');", txtFixingDate.ID,
                TradeActionMode.TradeActionModeEnum.SearchQuote_FxRate);
            txtFixingDate.Attributes.Add("onchange", sb.ToString());
            td.Controls.Add(txtFixingDate);
            #endregion Fixing Date
            #region Fixing time
            controlGUI = new ControlGUI
            {
                Name = ResFormFixingTime,
                Regex = EFSRegex.TypeRegex.RegexLongTime,
                LblWidth = 100
            };
            FpMLTextBox txtFixingTime = new FpMLTextBox(null, DtFunc.DateTimeToString(pFixingTime, DtFunc.FmtISOTime), 65,
                                            "FixingTime", controlGUI, null, false, "TXTFIXINGTIME_" + idE, null,
                new Validator("FixingTime", true),
                new Validator(EFSRegex.TypeRegex.RegexLongTime, "FixingTime", true, false));
            sb = new StringBuilder();
            sb.AppendFormat("javascript:__doPostBack('{0}','{1}');", txtFixingTime.ID,
                TradeActionMode.TradeActionModeEnum.SearchQuote_FxRate);
            txtFixingTime.Attributes.Add("onchange", sb.ToString());
            td.Controls.Add(txtFixingTime);
            tr.Cells.Add(td);
            return tr;
            #endregion Fixing time
        }
        #endregion CreateFixingDateTime
        #region CreateObservedDates
        // EG 20200914 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc) Correction et compléments
        private TableRow CreateObservedDates
        {
            get
            {
                TableRow tr = new TableRow
                {
                    CssClass = "DataGrid_ItemStyle"
                };
                TableCell td = new TableCell();

                Panel pnlObservedRate = new Panel
                {
                    Height = Unit.Pixel(60),
                    Width = Unit.Pixel(350)
                };
                pnlObservedRate.Style[HtmlTextWriterStyle.Overflow] = "auto";
                pnlObservedRate.Style[HtmlTextWriterStyle.MarginLeft] = "105";

                Table tableObservedRate = new Table()
                {
                    CssClass = "DataGrid",
                    CellSpacing = 0,
                    CellPadding = 0,
                    GridLines = GridLines.Both,
                    Width = Unit.Percentage(100)
                };
                TableRow rowObservedRate = new TableRow
                {
                    CssClass = "DataGrid_AlternatingItemStyle"
                };
                rowObservedRate.Cells.Add(TableTools.AddHeaderCell(ResFormActionDate, false, 0, UnitEnum.Pixel, 2, true));
                rowObservedRate.Cells.Add(TableTools.AddHeaderCell(ResFormRate, false, 0, UnitEnum.Pixel, 0, true));
                rowObservedRate.Cells.Add(TableTools.AddHeaderCell(ResFormWeight, false, 0, UnitEnum.Pixel, 0, true));
                tableObservedRate.Rows.Add(rowObservedRate);
                foreach (EFS_RateObservationDate item in m_rateObservationDates.efs_RateObservationDates)
                {
                    rowObservedRate = new TableRow
                    {
                        CssClass = "DataGrid_ItemStyle"
                    };
                    string observedDate = DtFunc.DateTimeToString(item.observationDate.DateValue, DtFunc.FmtShortDate);
                    string observedTime = DtFunc.DateTimeToString(item.observationDate.DateTimeValue, DtFunc.FmtISOTime);
                    rowObservedRate.Cells.Add(TableTools.AddCell(observedDate, HorizontalAlign.Center, 80, UnitEnum.Pixel));
                    rowObservedRate.Cells.Add(TableTools.AddCell(observedTime, HorizontalAlign.Center, 60, UnitEnum.Pixel));
                    if (item.rateSpecified)
                        rowObservedRate.Cells.Add(TableTools.AddCell(item.rate.CultureValue, HorizontalAlign.Right));
                    else
                        rowObservedRate.Cells.Add(TableTools.AddCell(Ressource.GetString("NotFound"), HorizontalAlign.Center));
                    rowObservedRate.Cells.Add(TableTools.AddCell(item.weightingFactor.CultureValue, HorizontalAlign.Right));
                    tableObservedRate.Rows.Add(rowObservedRate);
                }
                pnlObservedRate.Controls.Add(tableObservedRate);
                td.Controls.Add(pnlObservedRate);
                tr.Cells.Add(td);
                return tr;
            }
        }
        #endregion CreateObservedDates
        #region CreateSettlementRate
        private TableRow CreateSettlementRate
        {
            get
            {
                StringBuilder sb = new StringBuilder();
                TableRow tr = new TableRow
                {
                    CssClass = "DataGrid_ItemStyle"
                };
                TableCell td = new TableCell();
                ControlGUI controlGUI = new ControlGUI
                {
                    Regex = EFSRegex.TypeRegex.RegexFxRateExtend,
                    LblWidth = 100
                };
                if (IsCappedCallOrFlooredPutOption)
                    controlGUI.Name = (IsFxCapBarrierOption ? ResFormCapRate : ResFormFloorRate);
                else
                    controlGUI.Name = ResFormExchangeRate;

                bool isReadOnly = IsFxAverageOption && (false == IsFxAverageStrikeOption);
                FpMLTextBox txtSettlementRate = new FpMLTextBox(null, SettlementRate.Second.CultureValue, 150, "SettlementRate", controlGUI,
                    null, isReadOnly, "TXTSETTLEMENTRATE_" + idE, null,
                    new Validator("SettlementRate", true),
                    new Validator(EFSRegex.TypeRegex.RegexFxRateExtend, "SettlementRate", true, false));
                sb.AppendFormat("javascript:__doPostBack('{0}','{1}');", txtSettlementRate.ID,
                    TradeActionMode.TradeActionModeEnum.CalculCashSettlementMoneyAmount);
                txtSettlementRate.Attributes.Add("onchange", sb.ToString());
                td.Controls.Add(txtSettlementRate);
                if (IsFxAverageOption && (false == IsFxAverageStrikeOption))
                    td.Controls.Add(new LiteralControl(Cst.HTMLSpace + StrikeQuoteBasis(SettlementRateQuoteBasis, true)));
                else
                {
                    td.Controls.Add(new LiteralControl(Cst.HTMLSpace + QuoteBasisValue()));
                }
                td.Controls.Add(new LiteralControl(Cst.HTMLSpace + AverageRateFormulaName));
                tr.Cells.Add(td);
                return tr;
            }
        }
        #endregion CreateSettlementRate
        #region EnableAmountCurrency
        private void EnableAmountCurrency(PageBase pPage)
        {
            bool isReadOnly = false;
            if (null != pPage.Request.Form["DDLEXERCISETYPE_" + idE])
            {
                string exerciseType = pPage.Request.Form["DDLEXERCISETYPE_" + idE];
                isReadOnly = EventTypeFunc.IsTotal(exerciseType);
            }
            Control ctrl = pPage.FindControl("TXTCALLAMOUNT_" + idE);
            if (null != ctrl)
                ((FpMLTextBox)ctrl).IsLocked = isReadOnly;
            ctrl = pPage.FindControl("TXTPUTAMOUNT_" + idE);
            if (null != ctrl)
                ((FpMLTextBox)ctrl).IsLocked = isReadOnly;
        }
        #endregion EnableAmountCurrency
        #region PreCalculAverageOptionInTheMoney
        // EG 20180423 Analyse du code Correction [CA2200]
        private void PreCalculAverageOptionInTheMoney()
        {
            try
            {
                string cs = SessionTools.CS;
                IFxAverageRateOption product = (IFxAverageRateOption)CurrentProduct;
                #region Asset characteristics
                #region PrimaryRateSource
                m_PrimaryIdAsset = product.PrimaryRateSource.OTCmlId;
                m_PrimaryIdAssetSpecified = (m_PrimaryIdAsset != 0);
                m_PrimaryKeyAsset = new KeyAssetFxRate();
                #region PrimaryKeyAsset
                if (StrikeQuoteBasisEnum.CallCurrencyPerPutCurrency == SettlementRateQuoteBasis)
                {
                    m_PrimaryKeyAsset.IdC1 = product.CallCurrencyAmount.Currency;
                    m_PrimaryKeyAsset.IdC2 = product.PutCurrencyAmount.Currency;
                }
                else
                {
                    m_PrimaryKeyAsset.IdC1 = product.PutCurrencyAmount.Currency;
                    m_PrimaryKeyAsset.IdC2 = product.CallCurrencyAmount.Currency;
                }
                m_PrimaryKeyAsset.IdBCRateSrc = product.FixingTime.BusinessCenter.Value;
                m_PrimaryKeyAsset.PrimaryRateSrc = product.PrimaryRateSource.RateSource.Value;
                if (product.PrimaryRateSource.RateSourcePageSpecified)
                    m_PrimaryKeyAsset.PrimaryRateSrcPage = product.PrimaryRateSource.RateSourcePage.Value;
                if (product.PrimaryRateSource.RateSourcePageHeadingSpecified)
                    m_PrimaryKeyAsset.PrimaryRateSrcHead = product.PrimaryRateSource.RateSourcePageHeading;
                #endregion PrimaryKeyAsset
                #endregion PrimaryRateSource
                #region SecondaryRateSource
                m_SecondaryKeyAssetSpecified = product.SecondaryRateSourceSpecified;
                if (m_SecondaryKeyAssetSpecified)
                {
                    m_SecondaryIdAsset = product.SecondaryRateSource.OTCmlId;
                    m_SecondaryIdAssetSpecified = (m_SecondaryIdAsset != 0);
                    #region SecondaryKeyAsset
                    m_SecondaryKeyAsset = new KeyAssetFxRate
                    {
                        IdC1 = m_PrimaryKeyAsset.IdC1,
                        IdC2 = m_PrimaryKeyAsset.IdC2,
                        IdBCRateSrc = m_PrimaryKeyAsset.IdBCRateSrc,
                        PrimaryRateSrc = product.SecondaryRateSource.RateSource.Value
                    };
                    if (product.SecondaryRateSource.RateSourcePageSpecified)
                        m_SecondaryKeyAsset.PrimaryRateSrcPage = product.SecondaryRateSource.RateSourcePage.Value;
                    if (product.SecondaryRateSource.RateSourcePageHeadingSpecified)
                        m_SecondaryKeyAsset.PrimaryRateSrcHead = product.SecondaryRateSource.RateSourcePageHeading;
                    #endregion SecondaryKeyAsset
                }
                #endregion SecondaryRateSource
                #endregion Asset characteristics

                m_rateObservationDates = new EFS_FxAverageRateObservationDates(product, actionDate.DateValue);

                #region Spot reading
                decimal spotRate = 0;
                DateTime fixingTime = Convert.ToDateTime(null);
                foreach (EFS_RateObservationDate item in m_rateObservationDates.efs_RateObservationDates)
                {
                    if (false == item.rateSpecified)
                    {
                        fixingTime = DtFunc.AddTimeToDate(item.observationDate.DateValue, product.FixingTime.HourMinuteTime.TimeValue);
                        spotRate = QuoteAssetReading(cs, fixingTime, out RateSourceTypeEnum rateSourceType);
                        item.rateSpecified = (RateSourceTypeEnum.None != rateSourceType);
                        item.observationDate.DateTimeValue = fixingTime;
                        if (item.rateSpecified)
                        {
                            item.rate = new EFS_Decimal(spotRate);
                            item.isSecondaryRateSource = (RateSourceTypeEnum.SecondaryRateSource == rateSourceType);
                        }
                    }
                }
                #endregion Spot reading
                #region Average formula
                bool isAllRateSpecified = true;
                decimal nbAverageDates = m_rateObservationDates.efs_RateObservationDates.Length;
                decimal mtAverage = (product.GeometricAverageSpecified ? 1 : 0);
                foreach (EFS_RateObservationDate item in m_rateObservationDates.efs_RateObservationDates)
                {
                    if (item.rateSpecified)
                    {
                        if (IsGeometricAverage)
                            mtAverage *= item.rate.DecValue;
                        else
                            mtAverage += (item.rate.DecValue * item.weightingFactor.DecValue);
                    }
                    else
                    {
                        isAllRateSpecified = false;
                        break;
                    }
                }

                if (isAllRateSpecified)
                {
                    if (IsGeometricAverage)
                        mtAverage = (decimal)System.Math.Pow(Convert.ToDouble(mtAverage), Convert.ToDouble(1 / nbAverageDates));
                    else
                        mtAverage /= nbAverageDates;

                    if (product.PrecisionSpecified)
                        mtAverage = System.Math.Round(mtAverage, product.Precision.IntValue, MidpointRounding.AwayFromZero);
                }
                if (IsFxAverageStrikeOption)
                {
                    #region AverageStrikeOption Type
                    SettlementRate.Second.DecValue = QuoteAssetReading(cs, FixingDateTime);
                    StrikePrice.DecValue = mtAverage;
                    #endregion AverageStrikeOption Type
                }
                else
                {
                    #region AverageRateOption Type
                    SettlementRate.Second.DecValue = isAllRateSpecified ? mtAverage : 0;
                    #endregion AverageRateOption Type
                }
                #endregion Average formula

                FxFixing.QuotedCurrencyPair.Currency1 = m_PrimaryKeyAsset.IdC1;
                FxFixing.QuotedCurrencyPair.Currency2 = m_PrimaryKeyAsset.IdC2;
                FxFixing.QuotedCurrencyPair.QuoteBasis = m_PrimaryKeyAsset.QuoteBasis;
            }
            catch (Exception) { throw; }
        }
        #endregion PreCalculAverageOptionInTheMoney
        #region PreCalculRegularOptionInTheMoney
        // EG 20180423 Analyse du code Correction [CA2200]
        private void PreCalculRegularOptionInTheMoney()
        {
            try
            {
                #region FxOptionType
                IQuotedCurrencyPair quotedCurrencyPair = FxFixing.QuotedCurrencyPair;
                if (EventCodeFunc.IsSimpleOption(m_EventParent.eventCode))
                    m_FxOptionType = FxOptionTypeEnum.SimpleOption;
                else if (EventCodeFunc.IsBarrierOption(m_EventParent.eventCode))
                {
                    if (IsCappedCallOrFlooredPutOption)
                    {
                        if (IsFxCapBarrierOption)
                            m_FxOptionType = FxOptionTypeEnum.CappedCallBarrierOption;
                        else if (IsFxFloorBarrierOption)
                            m_FxOptionType = FxOptionTypeEnum.FloorPutBarrierOption;
                    }
                    else
                        m_FxOptionType = FxOptionTypeEnum.BarrierOption;
                }
                else
                    m_FxOptionType = FxOptionTypeEnum.None;
                #endregion FxOptionType
                #region Asset characteristics
                #region PrimaryRateSource
                m_PrimaryIdAsset = FxFixing.PrimaryRateSource.OTCmlId;
                m_PrimaryIdAssetSpecified = (m_PrimaryIdAsset != 0);
                m_PrimaryKeyAsset = new KeyAssetFxRate();

                #region PrimaryKeyAsset
                m_PrimaryKeyAsset.IdC1 = quotedCurrencyPair.Currency1;
                m_PrimaryKeyAsset.IdC2 = quotedCurrencyPair.Currency2;
                m_PrimaryKeyAsset.IdBCRateSrc = FxFixing.FixingTime.BusinessCenter.Value;
                m_PrimaryKeyAsset.PrimaryRateSrc = FxFixing.PrimaryRateSource.RateSource.Value;
                if (FxFixing.PrimaryRateSource.RateSourcePageSpecified)
                    m_PrimaryKeyAsset.PrimaryRateSrcPage = FxFixing.PrimaryRateSource.RateSourcePage.Value;
                if (FxFixing.PrimaryRateSource.RateSourcePageHeadingSpecified)
                    m_PrimaryKeyAsset.PrimaryRateSrcHead = FxFixing.PrimaryRateSource.RateSourcePageHeading;
                #endregion PrimaryKeyAsset
                #endregion PrimaryRateSource

                #region SecondaryRateSource
                m_SecondaryKeyAssetSpecified = FxFixing.SecondaryRateSourceSpecified;
                if (m_SecondaryKeyAssetSpecified)
                {
                    m_SecondaryIdAsset = FxFixing.SecondaryRateSource.OTCmlId;
                    m_SecondaryIdAssetSpecified = (m_SecondaryIdAsset != 0);
                    #region SecondaryKeyAsset
                    KeyAssetFxRate m_SecondaryKeyAsset = new KeyAssetFxRate
                    {
                        IdC1 = m_PrimaryKeyAsset.IdC1,
                        IdC2 = m_PrimaryKeyAsset.IdC2,
                        IdBCRateSrc = m_PrimaryKeyAsset.IdBCRateSrc,
                        QuoteBasis = quotedCurrencyPair.QuoteBasis,
                        QuoteBasisSpecified = true,
                        PrimaryRateSrc = FxFixing.SecondaryRateSource.RateSource.Value
                    };
                    if (FxFixing.SecondaryRateSource.RateSourcePageSpecified)
                        m_SecondaryKeyAsset.PrimaryRateSrcPage = FxFixing.SecondaryRateSource.RateSourcePage.Value;
                    if (FxFixing.SecondaryRateSource.RateSourcePageHeadingSpecified)
                        m_SecondaryKeyAsset.PrimaryRateSrcHead = FxFixing.SecondaryRateSource.RateSourcePageHeading;
                    #endregion SecondaryKeyAsset
                }
                #endregion SecondaryRateSource
                #endregion Asset characteristics
                if (IsCappedCallOrFlooredPutOption)
                    SettlementRate.Second.DecValue = CurrentEventParent.GetSettlementRateCappedFlooredBarrierKnocked;
                else
                {
                    string rateSource = string.Empty;
                    string cs = SessionTools.CS;
                    SettlementRate.Second.DecValue = QuoteAssetReading(cs, FixingDateTime, out rateSource);
                    if (StrFunc.IsEmpty(FxFixing.PrimaryRateSource.RateSource.Value))
                        FxFixing.PrimaryRateSource.RateSource.Value = rateSource;
                }
            }
            catch (Exception) { throw; }
        }
        #endregion PreCalculRegularOptionInTheMoney
        #region PostedAction
        public object PostedAction(string pKeyAction)
        {
            string keyAction = pKeyAction + Convert.ToInt32(TradeActionCode.TradeActionCodeEnum.Exercise) + "_" + m_Event.eventCode;
            DateTime fixingDate = FixingDateTime;
            RateObservationDateMsg[] rateObservationDateEvents = null;

            if (IsFxAverageOption)
            {
                ArrayList aRateObservationDates = new ArrayList();
                if (null != m_rateObservationDates)
                {
                    foreach (EFS_RateObservationDate item in m_rateObservationDates.efs_RateObservationDates)
                    {
                        RateObservationDateMsg rateObservation = new RateObservationDateMsg(item.observationDate.DateTimeValue,
                            (item.rateSpecified ? item.rate.DecValue : 0), item.weightingFactor.DecValue, item.isSecondaryRateSource);
                        aRateObservationDates.Add(rateObservation);
                    }
                    if (0 < aRateObservationDates.Count)
                        rateObservationDateEvents = (RateObservationDateMsg[])
                            aRateObservationDates.ToArray(typeof(RateObservationDateMsg));
                }
            }

            return new FX_ExerciseMsg(idE, ActionDateTime, ValueDate.DateValue, abandonExerciseType, IsCashSettlement, IsInTheMoney,
                CallAmount, PutAmount,
                SettlementAmount.DecValue, SettlementCurrency, SettlementRate.Second.DecValue, StrikePrice.DecValue, fixingDate,
                m_PrimaryIdAsset, m_PrimaryKeyAsset, m_SecondaryIdAsset, m_SecondaryKeyAsset, rateObservationDateEvents,
                m_QuoteSide, m_QuoteTiming, m_FxOptionType.ToString(), note, keyAction);
        }
        #endregion PostedAction
        #region QuoteAssetReading
        private decimal QuoteAssetReading(string pCs, DateTime pFixingDateTime)
        {
            return QuoteAssetReading(pCs, pFixingDateTime, out _, out _);
        }
        private decimal QuoteAssetReading(string pCs, DateTime pFixingDateTime, out RateSourceTypeEnum pRateSourceType)
        {
            return QuoteAssetReading(pCs, pFixingDateTime, out pRateSourceType, out _);
        }
        private decimal QuoteAssetReading(string pCs, DateTime pFixingDateTime, out string pRateSource)
        {
            return QuoteAssetReading(pCs, pFixingDateTime, out _, out pRateSource);
        }
        private decimal QuoteAssetReading(string pCs, DateTime pFixingDateTime, out RateSourceTypeEnum pRateSourceType, out string pRateSource)
        {
            string cs = pCs;
            IProduct product = (IProduct)CurrentProduct;
            pRateSourceType = RateSourceTypeEnum.None;
            pRateSource = string.Empty;
            m_QuoteSide = string.Empty;
            m_QuoteTiming = string.Empty;
            
            KeyQuote keyQuote = new KeyQuote(cs, pFixingDateTime,
                CallAmount.payer.actor.First, CallAmount.payer.book.First, PutAmount.payer.actor.First, PutAmount.payer.book.First);
            m_PrimaryKeyAsset.SetQuoteBasis();
            //
            SQL_Quote quote;
            if (m_PrimaryIdAssetSpecified)
                quote = new SQL_Quote(cs, QuoteEnum.FXRATE, AvailabilityEnum.Enabled, product.ProductBase , keyQuote, m_PrimaryIdAsset);
            else
                quote = new SQL_Quote(cs, QuoteEnum.FXRATE, AvailabilityEnum.Enabled, product.ProductBase, keyQuote, m_PrimaryKeyAsset);
            //
            if (quote.IsLoaded)
            {
                pRateSourceType = RateSourceTypeEnum.PrimaryRateSource;
                pRateSource = quote.QuoteSource;
            }
            else if (m_SecondaryKeyAssetSpecified)
            {
                m_SecondaryKeyAsset.SetQuoteBasis();
                if (m_SecondaryIdAssetSpecified)
                    quote = new SQL_Quote(cs, QuoteEnum.FXRATE, AvailabilityEnum.Enabled, product.ProductBase, keyQuote, m_SecondaryIdAsset);
                else
                    quote = new SQL_Quote(cs, QuoteEnum.FXRATE, AvailabilityEnum.Enabled, product.ProductBase , keyQuote, m_SecondaryKeyAsset);
                pRateSourceType = RateSourceTypeEnum.SecondaryRateSource;
            }
            if (quote.IsLoaded)
            {
                m_QuoteSide = (keyQuote.QuoteSide != null) ? keyQuote.QuoteSide.ToString() : string.Empty;
                m_QuoteTiming = (keyQuote.QuoteTiming != null)?  keyQuote.QuoteTiming.ToString(): string.Empty;  
                return quote.QuoteValue;
            }
            else
                return 0;
        }
        #endregion QuoteAssetReading
        #region QuoteBasisValue
        private string QuoteBasisValue()
        {
            return QuoteBasisValue(false);
        }
        private string QuoteBasisValue(bool pIsInverse)
        {
            IQuotedCurrencyPair quotedCurrencyPair = FxFixing.QuotedCurrencyPair;
            string currencyPair;
            if ((QuoteBasisEnum.Currency1PerCurrency2 == quotedCurrencyPair.QuoteBasis) ||
                ((QuoteBasisEnum.Currency2PerCurrency1 == quotedCurrencyPair.QuoteBasis) && pIsInverse))
                currencyPair = quotedCurrencyPair.Currency1 + "/" + quotedCurrencyPair.Currency2;
            else
                currencyPair = quotedCurrencyPair.Currency2 + "/" + quotedCurrencyPair.Currency1;
            return currencyPair;
        }
        #endregion QuoteBasisValue
        #region SearchQuote_FxRate
        public void SearchQuote_FxRate(PageBase pPage, string pControlId)
        {
            FormatControl(pPage, pControlId);
            TextBox ctrlId = (TextBox)pPage.FindControl(pControlId);
            if ((null != pPage.Request.Form["TXTFIXINGDATE_" + idE]) && (null != pPage.Request.Form["TXTFIXINGTIME_" + idE]))
            {
                DateTime dtFixingDate;
                if ("TXTFIXINGDATE_" + idE == pControlId)
                    dtFixingDate = new DtFunc().StringToDateTime(ctrlId.Text);
                else
                    dtFixingDate = new DtFunc().StringToDateTime(pPage.Request.Form["TXTFIXINGDATE_" + idE]);
                DateTime dtFixingTime;
                if ("TXTFIXINGTIME_" + idE == pControlId)
                    dtFixingTime = new DtFunc().StringToDateTime(ctrlId.Text);
                else
                    dtFixingTime = new DtFunc().StringToDateTime(pPage.Request.Form["TXTFIXINGTIME_" + idE]);
                dtFixingDate = DtFunc.AddTimeToDate(dtFixingDate, dtFixingTime);

                decimal settlementRate = QuoteAssetReading(SessionTools.CS, dtFixingDate, out string rateSource);

                Control ctrl;
                if ((null != pPage.Request.Form["TXTRATESOURCE_" + idE]) && StrFunc.IsFilled(rateSource))
                {
                    ctrl = pPage.FindControl("TXTRATESOURCE_" + idE);
                    if (null != ctrl)
                        ((TextBox)ctrl).Text = rateSource;
                }
                ctrl = pPage.FindControl("TXTSETTLEMENTRATE_" + idE);
                if (null != ctrl)
                {
                    ((TextBox)ctrl).Text = StrFunc.FmtDecimalToCurrentCulture(settlementRate.ToString(NumberFormatInfo.InvariantInfo));
                    #region settlementAmount are recalculated
                    CalculCashSettlementMoneyAmount(pPage, pControlId);
                    #endregion settlementAmount are recalculated
                }
            }
        }
        #endregion SearchQuote_FxRate
        #region StrikeQuoteBasis
        private string StrikeQuoteBasis(StrikeQuoteBasisEnum pQuoteBasis)
        {
            return StrikeQuoteBasis(pQuoteBasis, false);
        }
        private string StrikeQuoteBasis(StrikeQuoteBasisEnum pQuoteBasis, bool pIsSettlementRate)
        {
            // inversion de l'affichage des paires de devises
            string currencyPair;
            if (pIsSettlementRate && (null != m_PrimaryKeyAsset))
            {
                if (QuoteBasisEnum.Currency1PerCurrency2 == m_PrimaryKeyAsset.QuoteBasis)
                    currencyPair = m_PrimaryKeyAsset.IdC1 + "/" + m_PrimaryKeyAsset.IdC2;
                else
                    currencyPair = m_PrimaryKeyAsset.IdC2 + "/" + m_PrimaryKeyAsset.IdC1;
            }
            else
            {
                if (StrikeQuoteBasisEnum.CallCurrencyPerPutCurrency == pQuoteBasis)
                    currencyPair = CallAmount.Currency + "/" + PutAmount.Currency;
                else
                    currencyPair = PutAmount.Currency + "/" + CallAmount.Currency;
            }
            return currencyPair;
        }
        #endregion StrikeQuoteBasis

        #endregion Methods
    }
    #endregion ExerciseEvents
    #region PayoutEvents
    public class PayoutEvents : PayoutRebateEvents
    {
        #region Members
        public EFS_NonNegativeInteger nbPeriod;
        public EFS_Decimal percentage;
        public EFS_Decimal payoutRate;
        public EFS_Decimal gapRate;
        private readonly bool m_IsAssetOrNothing;
        private readonly bool m_IsGap;
        private readonly bool m_IsExtinguishing;
        private readonly bool m_IsResurrecting;
        private readonly IAssetOrNothing m_AssetOrNothing;
        private readonly IPayoutPeriod m_Period;
        #endregion Members
        #region Accessors
        #region AssetOrNothing
        private IAssetOrNothing AssetOrNothing
        {
            get
            {
                object product = CurrentProduct;
                Type tProduct = product.GetType();
                FieldInfo fldAssetOrNothing = tProduct.GetField("assetOrNothing");
                if (null != fldAssetOrNothing)
                {
                    IAssetOrNothing assetOrNothing = (IAssetOrNothing)fldAssetOrNothing.GetValue(product);
                    return assetOrNothing;
                }
                return null;
            }
        }
        #endregion AssetOrNothing
        #region CreateHeaderCells_Capture
        private TableHeaderCell[] CreateHeaderCells_Capture
        {
            get
            {
                ArrayList aHeaderCell = new ArrayList
                {
                    TableTools.AddHeaderCell(ResFormTitleAbandonExerciseDate, false, 0, UnitEnum.Pixel, 2, false),
                    TableTools.AddHeaderCell(ResFormSettlementDate, false, 0, UnitEnum.Pixel, 0, true),
                    TableTools.AddHeaderCell(ResFormOriginalAmount, false, 0, UnitEnum.Pixel, 2, true)
                };
                if (m_CustomerSettlementPayoutRebateSpecified)
                    aHeaderCell.Add(TableTools.AddHeaderCell(ResFormOriginalBaseAmount, false, 0, UnitEnum.Pixel, 2, true));
                if (IsExtinguishingOrResurrecting)
                {
                    aHeaderCell.Add(TableTools.AddHeaderCell(ResFormNbPeriodPayout, false, 0, UnitEnum.Pixel, 0, true));
                    aHeaderCell.Add(TableTools.AddHeaderCell(ResFormPercentageOfPayout, false, 0, UnitEnum.Pixel, 0, true));
                }
                else if (m_IsAssetOrNothing)
                {
                    aHeaderCell.Add(TableTools.AddHeaderCell(ResFormSpotRatePayout, false, 0, UnitEnum.Pixel, 0, true));
                    aHeaderCell.Add(TableTools.AddHeaderCell(ResFormGapRate, false, 0, UnitEnum.Pixel, 0, false));
                }
                aHeaderCell.Add(TableTools.AddHeaderCell(ResFormRealAmount, false, 0, UnitEnum.Pixel, 2, true));
                if (m_CustomerSettlementPayoutRebateSpecified)
                    aHeaderCell.Add(TableTools.AddHeaderCell(ResFormRealBaseAmount, false, 0, UnitEnum.Pixel, 2, true));
                aHeaderCell.Add(TableTools.AddHeaderCell(ResFormTitleNoteEvents, false, 0, UnitEnum.Pixel, 2, false));
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
        #region IsExtinguishingOrResurrecting
        private bool IsExtinguishingOrResurrecting
        {
            get { return m_IsExtinguishing || m_IsResurrecting; }
        }
        #endregion IsExtinguishingOrResurrecting
        #region IsGapSpecified
        private bool IsGapSpecified
        {
            get
            {
                bool isGap = false;
                if (IsFieldSpecified("assetOrNothing"))
                {
                    IFxDigitalOption product = (IFxDigitalOption)CurrentProduct;
                    isGap = product.AssetOrNothing.GapSpecified;
                    gapRate.DecValue = (isGap ? product.AssetOrNothing.Gap : 0);
                }
                return isGap;
            }
        }
        #endregion IsGapSpecified
        #region PayoutTouchDate
        private DateTime PayoutTouchDate
        {
            get
            {
                DateTime payoutTouchDate = Convert.ToDateTime(null);
                string eventCode = m_EventParent.eventCode;
                string eventType = m_EventParent.eventType;
                if (EventCodeFunc.IsEuropeanDigitalOption(eventCode))
                {
                    if (PayoutEnum.Deferred == PayoutStyle)
                        payoutTouchDate = ValueDate.DateValue;
                    else
                        payoutTouchDate = ExpiryDate;
                }
                else if (EventCodeFunc.IsAmericanDigitalOption(eventCode))
                {
                    if (EventTypeFunc.IsTouchNoTouch(eventType))
                    {
                        #region Simple trigger Touch, NoTouch : TouchDate = fixing date of trigger
                        payoutTouchDate = ((FxBarrierAndTriggerEvents)CurrentEventParent.EventTrigger[0].m_Action).ActionDateTime;
                        #endregion Simple trigger Touch, NoTouch : Date = fixing date of trigger
                    }
                    else if (EventTypeFunc.IsDoubleNoTouchPlus(eventType) ||
                             EventTypeFunc.IsDoubleTouchLimitPlus(eventType) ||
                             EventTypeFunc.IsDoubleTouchBoundary(eventType))
                    {
                        #region Double NoTouch, Limit, Range : TouchDate = greater fixing date of triggers
                        foreach (TradeActionEvent trg in CurrentEventParent.EventTrigger)
                        {
                            FxBarrierAndTriggerEvents action = (FxBarrierAndTriggerEvents)trg.m_Action;
                            if (0 < action.actionDate.DateTimeValue.CompareTo(payoutTouchDate))
                                payoutTouchDate = action.ActionDateTime;
                        }
                        #endregion Double NoTouch, Limit, Range : TouchDate = greater fixing date of triggers
                    }
                    else if (EventTypeFunc.IsDoubleTouch(eventType))
                    {
                        #region Double Touch : TouchDate = fixing date of activated trigger
                        foreach (TradeActionEvent trg in CurrentEventParent.EventTrigger)
                        {
                            FxBarrierAndTriggerEvents action = (FxBarrierAndTriggerEvents)trg.m_Action;
                            if (Cst.StatusTrigger.IsStatusActivated(action.idStTrigger))
                            {
                                payoutTouchDate = action.ActionDateTime;
                                break;
                            }
                        }
                        #endregion Double Touch : TouchDate = fixing date of activated trigger
                    }
                    if (PayoutEnum.Deferred == PayoutStyle)
                        payoutTouchDate = ValueDate.DateValue;
                }
                return payoutTouchDate;
            }
        }
        #endregion PayoutTouchDate
        #region Period
        private IPayoutPeriod Period
        {
            get
            {

                object product = CurrentProduct;
                Type tProduct = product.GetType();
                string fieldName = string.Empty;
                if (m_IsExtinguishing)
                    fieldName = "extinguishing";
                else if (m_IsResurrecting)
                    fieldName = "resurrecting";
                if (StrFunc.IsFilled(fieldName))
                {
                    FieldInfo fldPeriod = tProduct.GetField(fieldName);
                    if (null != fldPeriod)
                    {
                        IPayoutPeriod period = (IPayoutPeriod)fldPeriod.GetValue(product);
                        return period;
                    }
                }
                return null;
            }
        }
        #endregion Period
        #region PeriodInfo
        private string PeriodInfo
        {
            get { return m_Period.PeriodMultiplier.ToString() + " " + Ressource.GetString(m_Period.Period.ToString()); }
        }
        #endregion Period
        #region Resource in Form
        #region ResFormGapRate
        protected virtual string ResFormGapRate { get { return Ressource.GetString("GapRate"); } }
        #endregion ResFormGapRate
        #region ResFormNbPeriodPayout
        protected virtual string ResFormNbPeriodPayout { get { return Ressource.GetString("NbPeriodPayout"); } }
        #endregion ResFormNbPeriodPayout
        #region ResFormOriginalAmount
        protected virtual string ResFormOriginalAmount { get { return Ressource.GetString("OriginalPayoutAmount"); } }
        #endregion ResFormOriginalAmount
        #region ResFormOriginalBaseAmount
        protected virtual string ResFormOriginalBaseAmount { get { return Ressource.GetString("OriginalBasePayoutAmount"); } }
        #endregion ResFormOriginalBaseAmount
        #region ResFormPercentageOfPayout
        protected virtual string ResFormPercentageOfPayout { get { return Ressource.GetString("PercentageOfPayout"); } }
        #endregion ResFormPercentageOfPayout
        #region ResFormRealAmount
        protected virtual string ResFormRealAmount { get { return Ressource.GetString("RealAmount"); } }
        #endregion ResFormRealAmount
        #region ResFormRealBaseAmount
        protected virtual string ResFormRealBaseAmount { get { return Ressource.GetString("RealBaseAmount"); } }
        #endregion ResFormRealBaseAmount
        #region ResFormSettlementDate
        protected virtual string ResFormSettlementDate { get { return Ressource.GetString("DateSettlement"); } }
        #endregion ResFormSettlementDate
        #region ResFormSpotRatePayout
        protected virtual string ResFormSpotRatePayout { get { return Ressource.GetString("SpotRatePayout"); } }
        #endregion ResFormSpotRatePayout
        #region ResFormTitleAbandonEvents
        protected virtual string ResFormTitleAbandonEvents { get { return Ressource.GetString("AbandonEvents"); } }
        #endregion ResFormTitleAbandonEvents
        #region ResFormTitlePayoutCharacteristics
        protected virtual string ResFormTitlePayoutCharacteristics { get { return Ressource.GetString("PayoutCharacteristics"); } }
        #endregion ResFormTitlePayoutCharacteristics
        #endregion Resource in Form
        #endregion Accessors
        #region Constructors
        public PayoutEvents(TradeActionEvent pEvent, TradeActionEventBase pEventParent)
            : base(EventTypeFunc.Payout, pEvent, pEventParent)
        {
            percentage = new EFS_Decimal();
            payoutRate = new EFS_Decimal();
            gapRate = new EFS_Decimal();
            nbPeriod = new EFS_NonNegativeInteger();

            m_IsResurrecting = IsFieldSpecified("resurrecting");
            m_IsExtinguishing = IsFieldSpecified("extinguishing");
            m_IsAssetOrNothing = IsFieldSpecified("assetOrNothing");
            m_IsGap = IsGapSpecified;

            if (TradeAction is FX_TradeAction action)
            {
                PayoutStyle = action.PayoutStyle(m_Event.instrumentNo);
            }

            if (IsExtinguishingOrResurrecting)
                m_Period = Period;

            if (m_IsAssetOrNothing)
                m_AssetOrNothing = AssetOrNothing;

            if (null != m_FinalPayoutRebate)
            {
                actionDate.DateValue = m_Event.dtEndPeriod.DateValue;
                actionTime.TimeValue = m_Event.dtEndPeriod.DateTimeValue;
                ValueDate.DateValue = m_Event.EventClassGroupLevel.dtEvent.DateValue;
                amount.DecValue = m_FinalPayoutRebate.valorisation.DecValue;
                if (null != m_FinalPayoutRebate.details)
                {
                    originalAmount.DecValue = m_FinalPayoutRebate.details.totalPayoutAmount.DecValue;
                    if (IsExtinguishingOrResurrecting)
                    {
                        nbPeriod.IntValue = m_FinalPayoutRebate.details.nbPeriod.IntValue;
                        percentage.DecValue = m_FinalPayoutRebate.details.percentage.DecValue;
                    }
                    if (m_IsAssetOrNothing)
                    {
                        payoutRate.DecValue = m_FinalPayoutRebate.details.spotRate.DecValue;
                        gapRate.DecValue = m_FinalPayoutRebate.details.gapRate.DecValue;
                    }
                    if (m_CustomerSettlementPayoutRebateSpecified)
                    {
                        m_FinalBasePayoutRebate = m_FinalPayoutRebate.EventCustomerSettlementPayout;
                        baseAmount.DecValue = m_FinalBasePayoutRebate.valorisation.DecValue;
                    }
                }
            }
            else
            {
                if (null != m_EventParent)
                {
                    IsInTheMoney = m_EventParent.IsInTheMoney();
                    #region ValueDate (Settlement Date)
                    ValueDate.DateValue = PayoutTouchDate;
                    #endregion ValueDate (Settlement Date)

                    #region Action Date & Time
                    if (IsEuropean)
                    {
                        actionDate.DateValue = ExpiryDate;
                        actionTime.TimeValue = ExpiryDate;
                    }
                    else
                    {
                        // FI 20200904 [XXXXX] call OTCmlHelper.GetDateSys  
                        //actionDate.DateValue = OTCmlHelper.GetDateBusiness(SessionTools.CS);
                        actionDate.DateValue = OTCmlHelper.GetDateSys(SessionTools.CS);
                        actionDate.DateValue = ValueDate.DateValue;
                        if (0 < actionDate.DateValue.CompareTo(ExpiryDate))
                            actionDate.DateValue = ExpiryDate;
                        actionTime.TimeValue = actionDate.DateTimeValue;
                    }
                    #endregion Action Date & Time
                }

                //originalAmount.DecValue = m_OriginalPayoutRebate.valorisation.DecValue;
                //if (m_CustomerSettlementPayoutRebateSpecified)
                //    originalBaseAmount.DecValue = m_OriginalBasePayoutRebate.valorisation.DecValue;
                //else
                //    originalBaseAmount.DecValue = originalAmount.DecValue;

                if (IsExtinguishingOrResurrecting)
                {
                    nbPeriod.IntValue = 0;
                    percentage.DecValue = m_Period.Percentage;
                    amount.DecValue = 0;
                    baseAmount.DecValue = 0;
                }
                else if (m_IsAssetOrNothing)
                {
                    payoutRate.DecValue = 0;
                    gapRate.DecValue = (m_IsGap ? m_AssetOrNothing.Gap : 0);
                    amount.DecValue = 0;
                    baseAmount.DecValue = 0;
                }
                else
                {
                    amount.DecValue = (IsInTheMoney ? originalAmount.DecValue : 0);
                    baseAmount.DecValue = originalBaseAmount.DecValue;
                }
            }
        }
        #endregion Constructors
        #region Methods
        #region AddCells_Capture
        private TableCell[] AddCells_Capture(TradeActionEvent pEvent, TradeActionEvent pEventParent)
        {
            ArrayList aCell = new ArrayList();
            m_EventParent = pEventParent;
            IsInTheMoney = m_EventParent.IsInTheMoney();

            aCell.Add(TableTools.AddCell(ActionDate, HorizontalAlign.Center, 80, UnitEnum.Pixel));
            aCell.Add(TableTools.AddCell(actionTime.Value, HorizontalAlign.Center, 60, UnitEnum.Pixel));
            aCell.Add(TableTools.AddCell(FormatedValueDate, HorizontalAlign.Center, 80, UnitEnum.Pixel));

            aCell.Add(TableTools.AddCell(StrFunc.FmtDecimalToCurrentCulture(originalAmount.DecValue), HorizontalAlign.Right, 80, UnitEnum.Pixel));
            aCell.Add(TableTools.AddCell(m_OriginalPayoutRebate.unit, HorizontalAlign.Center));

            if (m_CustomerSettlementPayoutRebateSpecified)
            {
                // Affichage caractéristiques CustomerSettlementPayout
                aCell.Add(TableTools.AddCell(StrFunc.FmtDecimalToCurrentCulture(originalBaseAmount.DecValue), HorizontalAlign.Right, 80, UnitEnum.Pixel));
                aCell.Add(TableTools.AddCell(m_OriginalBasePayoutRebate.unit, HorizontalAlign.Center));
            }

            if (IsExtinguishingOrResurrecting)
            {
                aCell.Add(TableTools.AddCell(nbPeriod.Value, HorizontalAlign.Right, 80, UnitEnum.Pixel));
                aCell.Add(TableTools.AddCell(percentage.CultureValue, HorizontalAlign.Right, 80, UnitEnum.Pixel));
            }
            else if (m_IsAssetOrNothing)
            {
                aCell.Add(TableTools.AddCell(payoutRate.CultureValue, HorizontalAlign.Right, 80, UnitEnum.Pixel));
                aCell.Add(TableTools.AddCell(gapRate.CultureValue, HorizontalAlign.Right, 80, UnitEnum.Pixel));
            }
            aCell.Add(TableTools.AddCell(StrFunc.FmtDecimalToCurrentCulture(amount.DecValue), HorizontalAlign.Right, 80, UnitEnum.Pixel));
            aCell.Add(TableTools.AddCell(m_OriginalPayoutRebate.unit, HorizontalAlign.Center));

            if (m_CustomerSettlementPayoutRebateSpecified)
            {
                // Affichage caractéristiques CustomerSettlementPayout
                aCell.Add(TableTools.AddCell(StrFunc.FmtDecimalToCurrentCulture(baseAmount.DecValue), HorizontalAlign.Right, 80, UnitEnum.Pixel));
                aCell.Add(TableTools.AddCell(m_OriginalBasePayoutRebate.unit, HorizontalAlign.Center));
            }
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
            aCell.AddRange(AddCells_Capture(pEvent, pEventParent));
            return (TableCell[])aCell.ToArray(typeof(TableCell));
        }
        #endregion AddCells_Static
        #region CalculPayout
        public void CalculPayout(Page pPage, string pControlId)
        {
            FormatControl(pPage, pControlId);

            decimal originalBaseAmount = this.originalBaseAmount.DecValue;
            int nbPeriod = this.nbPeriod.IntValue;
            decimal percentage = this.percentage.DecValue;
            decimal payoutRate = this.payoutRate.DecValue;
            decimal gapRate = this.gapRate.DecValue;
            decimal baseAmount = this.baseAmount.DecValue;
            TextBox ctrlId = (TextBox)pPage.FindControl(pControlId);

            if (null != pPage.Request.Form["TXTORIGINALAMOUNT_" + idE])
                originalBaseAmount = DecFunc.DecValue(pPage.Request.Form["TXTORIGINALAMOUNT_" + idE], Thread.CurrentThread.CurrentCulture);

            if (null != pPage.Request.Form["TXTAMOUNT_" + idE])
            {
                if (("TXTAMOUNT_" + idE) == pControlId)
                    baseAmount = DecFunc.DecValue(ctrlId.Text, Thread.CurrentThread.CurrentCulture);
                else
                    baseAmount = DecFunc.DecValue(pPage.Request.Form["TXTAMOUNT_" + idE], Thread.CurrentThread.CurrentCulture);
            }

            EFS_Cash cash;
            Control ctrl;
            if (IsExtinguishingOrResurrecting)
            {
                if (null != pPage.Request.Form["TXTPERIOD_" + idE])
                {
                    if (("TXTPERIOD_" + idE) == pControlId)
                        nbPeriod = Convert.ToInt32(ctrlId.Text);
                    else
                        nbPeriod = Convert.ToInt32(pPage.Request.Form["TXTPERIOD_" + idE]);
                }
                if (null != pPage.Request.Form["TXTPCT_" + idE])
                {
                    if (("TXTPCT_" + idE) == pControlId)
                        percentage = DecFunc.DecValue(ctrlId.Text, Thread.CurrentThread.CurrentCulture);
                    else
                        percentage = DecFunc.DecValue(pPage.Request.Form["TXTPCT_" + idE], Thread.CurrentThread.CurrentCulture);
                }

                if ((("TXTPERIOD_" + idE) == pControlId) || (("TXTPCT_" + idE) == pControlId))
                {
                    ctrl = pPage.FindControl("TXTAMOUNT_" + idE);
                    baseAmount = originalBaseAmount * nbPeriod * percentage;
                    cash = new EFS_Cash(SessionTools.CS, baseAmount, m_OriginalBasePayoutRebate.unit);
                    baseAmount = cash.AmountRounded;
                    if (null != ctrl)
                        ((TextBox)ctrl).Text = StrFunc.FmtDecimalToCurrentCulture(baseAmount.ToString(NumberFormatInfo.InvariantInfo));
                }
                else if (("TXTAMOUNT_" + idE) == pControlId)
                {
                    ctrl = pPage.FindControl("TXTPCT_" + idE);
                    percentage = baseAmount / (originalBaseAmount * nbPeriod);
                    if (null != ctrl)
                        ((TextBox)ctrl).Text = StrFunc.FmtDecimalToCurrentCulture(percentage.ToString(NumberFormatInfo.InvariantInfo));
                }
            }
            else if (m_IsAssetOrNothing)
            {
                if (null != pPage.Request.Form["TXTSPOTRATE_" + idE])
                {
                    if (("TXTSPOTRATE_" + idE) == pControlId)
                        payoutRate = DecFunc.DecValue(ctrlId.Text, Thread.CurrentThread.CurrentCulture);
                    else
                        payoutRate = DecFunc.DecValue(pPage.Request.Form["TXTSPOTRATE_" + idE], Thread.CurrentThread.CurrentCulture);
                }
                if (null != pPage.Request.Form["TXTGAPRATE_" + idE])
                {
                    if (("TXTGAPRATE_" + idE) == pControlId)
                        gapRate = DecFunc.DecValue(ctrlId.Text, Thread.CurrentThread.CurrentCulture);
                    else
                        gapRate = DecFunc.DecValue(pPage.Request.Form["TXTGAPRATE_" + idE], Thread.CurrentThread.CurrentCulture);
                }

                if ((("TXTSPOTRATE_" + idE) == pControlId) || (("TXTGAPRATE_" + idE) == pControlId))
                {
                    ctrl = pPage.FindControl("TXTAMOUNT_" + idE);
                    baseAmount = originalBaseAmount * (payoutRate - gapRate);
                    cash = new EFS_Cash(SessionTools.CS, baseAmount, m_OriginalBasePayoutRebate.unit);
                    baseAmount = cash.AmountRounded;
                    if (null != ctrl)
                        ((TextBox)ctrl).Text = StrFunc.FmtDecimalToCurrentCulture(baseAmount.ToString(NumberFormatInfo.InvariantInfo));
                }
                else if (("TXTAMOUNT_" + idE) == pControlId)
                {
                    ctrl = pPage.FindControl("TXTSPOTRATE_" + idE);
                    payoutRate = (baseAmount + (originalBaseAmount * gapRate)) / originalBaseAmount;
                    if (null != ctrl)
                        ((TextBox)ctrl).Text = StrFunc.FmtDecimalToCurrentCulture(payoutRate.ToString(NumberFormatInfo.InvariantInfo));
                }
            }
        }
        #endregion CalculPayout
        #region CreateControlAmount
        public TableRow CreateControlAmount
        {
            get
            {
                TableRow tr = new TableRow
                {
                    CssClass = "DataGrid_ItemStyle"
                };
                TableCell td = new TableCell();
                ControlGUI controlGUI = new ControlGUI(true, ResFormRealBaseAmount)
                {
                    Regex = EFSRegex.TypeRegex.RegexAmountExtend,
                    LblWidth = 100
                };


                FpMLTextBox txtAmount = new FpMLTextBox(null, baseAmount.CultureValue, 200, "RealBaseAmount", controlGUI, null, false, "TXTAMOUNT_" + idE, null,
                    new Validator("RealAmount", true),
                    new Validator(EFSRegex.TypeRegex.RegexAmountExtend, "RealBaseAmount", true, false))
                {
                    Enabled = IsInTheMoney
                };
                if (IsInTheMoney)
                {
                    StringBuilder sb = new StringBuilder();
                    sb.AppendFormat("javascript:__doPostBack('{0}','{1}');", txtAmount.ID, TradeActionMode.TradeActionModeEnum.CalculPayout);
                    txtAmount.Attributes.Add("onchange", sb.ToString());
                }
                else
                    GetFormatControlAttribute(txtAmount);
                td.Controls.Add(txtAmount);
                td.Controls.Add(new LiteralControl(Cst.HTMLSpace + m_OriginalBasePayoutRebate.unit));
                tr.Cells.Add(td);

                return tr;
            }
        }
        #endregion CreateControlAmount
        #region CreateControlCurrentAction
        public TableRow[] CreateControlCurrentAction
        {
            get
            {
                IsInTheMoney = m_EventParent.IsInTheMoney();
                ArrayList aTableRow = new ArrayList
                {
                    CreateControlTitleSeparator(IsInTheMoney ? ResFormTitleExerciseEvents : ResFormTitleAbandonEvents, false)
                };
                #region ActionDate
                if (DtFunc.IsDateTimeEmpty(actionDate.DateValue))
                    actionDate.DateTimeValue = PayoutTouchDate;
                if (EventTypeFunc.IsEuropeanTrigger(m_Event.eventType))
                    actionDate.DateTimeValue = m_Event.dtEndPeriod.DateValue;
                aTableRow.Add(base.CreateControlActionDate());
                #endregion ActionDate
                #region ActionTime
                actionTime.TimeValue = actionDate.DateTimeValue;
                aTableRow.Add(base.CreateControlActionTime());
                #endregion ActionTime
                #region SettlementDate
                aTableRow.Add(CreateControlSettlementDate);
                #endregion SettlementDate

                aTableRow.Add(CreateControlTitleSeparator(ResFormTitlePayoutCharacteristics, false));
                #region Original payout amount
                aTableRow.Add(CreateControlOriginalPayoutAmount);
                #endregion Original payout amount
                #region PayoutCharacteristics
                aTableRow.AddRange(CreateControlPayoutCharacteristics);
                #endregion PayoutCharacteristics
                #region Amount
                aTableRow.Add(CreateControlAmount);
                #endregion Amount
                #region Description
                aTableRow.AddRange(base.CreateControlDescription);
                #endregion Description
                return (TableRow[])aTableRow.ToArray(typeof(TableRow));
            }
        }
        #endregion CreateControlCurrentAction
        #region CreateControlOriginalPayoutAmount
        public TableRow CreateControlOriginalPayoutAmount
        {
            get
            {
                TableRow tr = new TableRow
                {
                    CssClass = "DataGrid_ItemStyle"
                };
                TableCell td = new TableCell();
                ControlGUI controlGUI = new ControlGUI(true, ResFormOriginalBaseAmount)
                {
                    Regex = EFSRegex.TypeRegex.RegexAmountExtend,
                    LblWidth = 100
                };
                FpMLTextBox txtAmount = new FpMLTextBox(null, originalBaseAmount.CultureValue, 200, "OriginalBasePayoutAmount", controlGUI, null, true, "TXTORIGINALAMOUNT_" + idE, null,
                    new Validator("OriginalAmount", true),
                    new Validator(EFSRegex.TypeRegex.RegexAmountExtend, "OriginalPayoutAmount", true, false));
                GetFormatControlAttribute(txtAmount);
                td.Controls.Add(txtAmount);
                td.Controls.Add(new LiteralControl(Cst.HTMLSpace + m_OriginalBasePayoutRebate.unit));
                tr.Cells.Add(td);
                return tr;
            }
        }
        #endregion CreateControlOriginalPayoutAmount
        #region CreateControlPayoutCharacteristics
        public TableRow[] CreateControlPayoutCharacteristics
        {
            get
            {
                ArrayList aTableRow = new ArrayList();
                ControlGUI controlGUI = new ControlGUI();
                StringBuilder sb = new StringBuilder();
                TableRow tr;
                TableCell td;
                if (IsExtinguishingOrResurrecting)
                {
                    #region Nb Period
                    tr = new TableRow
                    {
                        CssClass = "DataGrid_ItemStyle"
                    };
                    td = new TableCell();
                    controlGUI.Name = ResFormNbPeriodPayout;
                    controlGUI.Regex = EFSRegex.TypeRegex.RegexInteger;
                    controlGUI.LblWidth = 100;
                    FpMLTextBox txtPeriod = new FpMLTextBox(null, nbPeriod.Value, 65, "NbPeriod", controlGUI, null, false, "TXTPERIOD_" + idE, null,
                        new Validator("NbPeriodPayout", true),
                        new Validator(EFSRegex.TypeRegex.RegexInteger, "NbPeriod", true, false));
                    if (IsInTheMoney)
                    {
                        sb.AppendFormat("javascript:__doPostBack('{0}','{1}');", txtPeriod.ID, TradeActionMode.TradeActionModeEnum.CalculPayout);
                        txtPeriod.Attributes.Add("onchange", sb.ToString());
                    }
                    else
                        GetFormatControlAttribute(txtPeriod);
                    td.Controls.Add(txtPeriod);
                    controlGUI = new ControlGUI
                    {
                        Name = Ressource.GetString2("CommentNbPeriodPayout", PeriodInfo),
                        LblWidth = 300
                    };
                    td.Controls.Add(new FpMLLabelOnly(controlGUI, null));
                    tr.Cells.Add(td);
                    aTableRow.Add(tr);
                    #endregion Nb Period
                    #region Percentage of original payout
                    tr = new TableRow
                    {
                        CssClass = "DataGrid_ItemStyle"
                    };
                    td = new TableCell();
                    controlGUI = new ControlGUI
                    {
                        Name = ResFormPercentageOfPayout,
                        Regex = EFSRegex.TypeRegex.RegexFixedRate,
                        LblWidth = 100
                    };
                    FpMLTextBox txtPct = new FpMLTextBox(null, percentage.CultureValue, 65, "PctPayout", controlGUI, null, false, "TXTPCT_" + idE, null,
                        new Validator("PercentageOfPayout", true),
                        new Validator(EFSRegex.TypeRegex.RegexFixedRate, "PctPayout", true, false));
                    if (IsInTheMoney)
                    {
                        sb = new StringBuilder();
                        sb.AppendFormat("javascript:__doPostBack('{0}','{1}');", txtPct.ID, TradeActionMode.TradeActionModeEnum.CalculPayout);
                        txtPct.Attributes.Add("onchange", sb.ToString());
                    }
                    else
                        GetFormatControlAttribute(txtPct);
                    td.Controls.Add(txtPct);
                    controlGUI.Name = Ressource.GetString2("CommentPercentageOfPayout");
                    controlGUI.LblWidth = 300;
                    td.Controls.Add(new FpMLLabelOnly(controlGUI, null));
                    tr.Cells.Add(td);
                    aTableRow.Add(tr);
                    #endregion Percentage of original payout
                }
                else if (m_IsAssetOrNothing)
                {
                    #region Spot rate of payout
                    tr = new TableRow
                    {
                        CssClass = "DataGrid_ItemStyle"
                    };
                    td = new TableCell();
                    controlGUI = new ControlGUI
                    {
                        Name = ResFormSpotRatePayout,
                        Regex = EFSRegex.TypeRegex.RegexFxRateExtend,
                        LblWidth = 100
                    };
                    FpMLTextBox txtSpot = new FpMLTextBox(null, payoutRate.CultureValue, 50, "SpotRatePayout", controlGUI, null, false, "TXTSPOTRATE_" + idE, null,
                        new Validator("SpotRatePayout", true),
                        new Validator(EFSRegex.TypeRegex.RegexFxRateExtend, "SpotRatePayout", true, false));
                    if (IsInTheMoney)
                    {
                        sb = new StringBuilder();
                        sb.AppendFormat("javascript:__doPostBack('{0}','{1}');", txtSpot.ID, TradeActionMode.TradeActionModeEnum.CalculPayout);
                        txtSpot.Attributes.Add("onchange", sb.ToString());
                    }
                    else
                        GetFormatControlAttribute(txtSpot);
                    td.Controls.Add(txtSpot);
                    tr.Cells.Add(td);
                    aTableRow.Add(tr);
                    #endregion Spot rate of payout
                    if (m_IsGap)
                    {
                        #region Gap rate
                        tr = new TableRow
                        {
                            CssClass = "DataGrid_ItemStyle"
                        };
                        td = new TableCell();
                        controlGUI = new ControlGUI
                        {
                            Name = ResFormGapRate,
                            Regex = EFSRegex.TypeRegex.RegexFxRateExtend,
                            LblWidth = 100
                        };
                        FpMLTextBox txtGap = new FpMLTextBox(null, gapRate.CultureValue, 50, "GapRate", controlGUI, null, false, "TXTGAPRATE_" + idE, null,
                            new Validator("GapRate", true),
                            new Validator(EFSRegex.TypeRegex.RegexFxRateExtend, "GapRate", true, false));
                        if (IsInTheMoney)
                        {
                            sb = new StringBuilder();
                            sb.AppendFormat("javascript:__doPostBack('{0}','{1}');", txtGap.ID, TradeActionMode.TradeActionModeEnum.CalculPayout);
                            txtGap.Attributes.Add("onchange", sb.ToString());
                        }
                        else
                            GetFormatControlAttribute(txtGap);
                        td.Controls.Add(txtGap);
                        tr.Cells.Add(td);
                        aTableRow.Add(tr);
                        #endregion Gap rate
                    }
                }
                else
                {
                    amount.DecValue = (IsInTheMoney ? originalAmount.DecValue : 0);
                    baseAmount.DecValue = (IsInTheMoney ? originalBaseAmount.DecValue : 0);
                }

                return (TableRow[])aTableRow.ToArray(typeof(TableRow));
            }
        }
        #endregion CreateControlOriginalFixedAmount
        #region CreateControlSettlementDate
        public TableRow CreateControlSettlementDate
        {
            get
            {
                TableRow tr = new TableRow
                {
                    CssClass = "DataGrid_ItemStyle"
                };
                TableCell td = new TableCell();
                ControlGUI controlGUI = new ControlGUI(true, ResFormValueDate)
                {
                    Regex = EFSRegex.TypeRegex.RegexDate,
                    LblWidth = 105
                };
                FpMLCalendarBox txtDate = new FpMLCalendarBox(null, ValueDate.DateValue, "ValueDate", controlGUI, null, "TXTVALUEDATE_" + idE,
                    new Validator("ValueDate", true),
                    new Validator(EFSRegex.TypeRegex.RegexDate, "ValueDate", true, false));
                GetFormatControlAttribute(txtDate);
                td.Controls.Add(txtDate);
                tr.Cells.Add(td);
                return tr;
            }
        }
        #endregion CreateControlSettlementDate
        #region CreateTableHeader
        public ArrayList CreateTableHeader(TradeActionEvent pEvent)
        {
            ArrayList aTableHeader = new ArrayList
            {
                pEvent.CurrentTradeAction.GetEventTypeTitle(pEvent.eventType),
                CreateHeaderCells_Static,
                CreateHeaderCells_Capture,
                ResFormTitleComplementary
            };
            return aTableHeader;
        }
        #endregion CreateTableHeader
        #region IsEventChanged
        //20090129 PL/EG Il faudra voir si on peut affiner pour voir une donnée a réellement été modifié par le user...
        public override bool IsEventChanged(TradeActionEventBase pEvent)
        {
            bool isEventChanged = base.IsEventChanged(pEvent);
            if (false == isEventChanged)
            {
                isEventChanged = true;
            }
            return isEventChanged;
        }
        #endregion IsEventChanged
        #region PostedAction
        public object PostedAction(string pKeyAction)
        {
            string keyAction = pKeyAction + Convert.ToInt32(TradeActionCode.TradeActionCodeEnum.Payout) + "_" + m_Event.eventCode;
            return new PayoutMsg(idE, ActionDateTime, ValueDate.DateValue, originalAmount.DecValue, amount.DecValue,
                originalBaseAmount.DecValue, baseAmount.DecValue, m_OriginalBasePayoutRebate.unit,
                nbPeriod.IntValue,percentage.DecValue, payoutRate.DecValue, gapRate.DecValue,
                m_OriginalPayoutRebate.unit, 
                Currency1,Currency2, QuoteBasis, CustomerPaymentRate, SpotRate,ForwardPoints,
                m_OriginalPayoutRebate.payer, m_OriginalPayoutRebate.receiver, note, keyAction);
        }
        #endregion PostedAction
        #region Save
        public override bool Save(Page pPage)
        {
            bool isOk = ValidationRules(pPage);
            if (isOk)
            {
                isOk = base.Save(pPage);
                if (isOk)
                {
                    if (null != pPage.Request.Form["TXTVALUEDATE_" + idE])
                        ValueDate.DateValue = new DtFunc().StringToDateTime(pPage.Request.Form["TXTVALUEDATE_" + idE]);
                    if (null != pPage.Request.Form["TXTPERIOD_" + idE])
                        nbPeriod.IntValue = Convert.ToInt32(pPage.Request.Form["TXTPERIOD_" + idE]);
                    if (null != pPage.Request.Form["TXTPCT_" + idE])
                        percentage.DecValue = DecFunc.DecValue(pPage.Request.Form["TXTPCT_" + idE], Thread.CurrentThread.CurrentCulture);
                    if (null != pPage.Request.Form["TXTSPOTRATE_" + idE])
                        payoutRate.DecValue = DecFunc.DecValue(pPage.Request.Form["TXTSPOTRATE_" + idE], Thread.CurrentThread.CurrentCulture);
                    if (null != pPage.Request.Form["TXTGAPRATE_" + idE])
                        gapRate.DecValue = DecFunc.DecValue(pPage.Request.Form["TXTGAPRATE_" + idE], Thread.CurrentThread.CurrentCulture);
                    if (null != pPage.Request.Form["TXTAMOUNT_" + idE])
                    {
                        baseAmount.DecValue = DecFunc.DecValue(pPage.Request.Form["TXTAMOUNT_" + idE], Thread.CurrentThread.CurrentCulture);
                        SetPayoutAmount(baseAmount.DecValue);
                    }
                }
            }
            m_Event.isValidated = isOk;
            return isOk;
        }
        #endregion Save
        #region ValidationRules
        public override bool ValidationRules(Page pPage)
        {
            m_Event.validationRulesMessages = new ArrayList();
            return base.ValidationRules(pPage);
        }
        #endregion ValidationRules
        #endregion Methods
    }
    #endregion PayoutEvents
    #region PayoutRebateEvents
    public class PayoutRebateEvents : ActionEvents
    {
        #region Members
        private readonly string eventType;
        public EFS_Decimal originalAmount;
        public EFS_Decimal originalBaseAmount;
        public EFS_Decimal amount;
        public EFS_Decimal baseAmount;
        public string currency;
        public string baseCurrency;

        protected bool m_TriggerPayoutSpecified;
        protected TradeActionEvent m_OriginalPayoutRebate;
        protected TradeActionEvent m_OriginalBasePayoutRebate;
        protected TradeActionEvent m_FinalPayoutRebate;
        protected TradeActionEvent m_FinalBasePayoutRebate;
        protected bool m_CustomerSettlementPayoutRebateSpecified;
        protected ICustomerSettlementPayment m_CustomerSettlementPayoutRebate;
        #endregion Members
        #region Accessors
        #region Currency1
        protected string Currency1
        {
            get
            {
                string currency = baseCurrency;
                if (m_CustomerSettlementPayoutRebateSpecified)
                {
                    currency = m_CustomerSettlementPayoutRebate.Rate.QuotedCurrencyPair.Currency1;
                }
                return currency;
            }
        }
        #endregion Currency1
        #region Currency2
        protected string Currency2
        {
            get
            {
                string currency = this.currency;
                if (m_CustomerSettlementPayoutRebateSpecified)
                {
                    currency = m_CustomerSettlementPayoutRebate.Rate.QuotedCurrencyPair.Currency2;
                }
                return currency;
            }
        }
        #endregion Currency2
        #region CustomerPaymentRate
        protected decimal CustomerPaymentRate
        {
            get
            {
                decimal rate = 0;
                if (m_CustomerSettlementPayoutRebateSpecified)
                {
                    rate = m_CustomerSettlementPayoutRebate.Rate.Rate.DecValue;
                }
                return rate;
            }
        }
        #endregion CustomerPaymentRate
        #region CustomerSettlementPayoutRebate
        private ICustomerSettlementPayment CustomerSettlementPayoutRebate
        {
            get
            {
                if (m_TriggerPayoutSpecified)
                {
                    object product = CurrentProduct;
                    Type tProduct = product.GetType();
                    FieldInfo fldTriggerPayout = tProduct.GetField("triggerPayout");
                    if (null != fldTriggerPayout)
                    {
                        IFxOptionPayout fxOptionPayout = (IFxOptionPayout)fldTriggerPayout.GetValue(product);
                        Type tFxOptionPayout = fxOptionPayout.GetType();
                        FieldInfo fldCustomerSettlementPayoutRebate = tFxOptionPayout.GetField("customerSettlementPayout");
                        if (null != fldCustomerSettlementPayoutRebate)
                        {
                            ICustomerSettlementPayment customerSettlementPayoutRebate = 
                                (ICustomerSettlementPayment)fldCustomerSettlementPayoutRebate.GetValue(fxOptionPayout);
                            return customerSettlementPayoutRebate;
                        }
                    }
                }
                return null;
            }
        }
        #endregion CustomerSettlementPayoutRebate
        #region EventCustomerSettlementPayoutRebate
        public TradeActionEvent EventCustomerSettlementPayoutRebate
        {
            get
            {
                if (EventTypeFunc.IsPayout(eventType))
                    return m_OriginalBasePayoutRebate.EventCustomerSettlementPayout;
                else if (EventTypeFunc.IsRebate(eventType))
                    return m_OriginalBasePayoutRebate.EventCustomerSettlementRebate;
                else
                    return null;
            }
        }
        #endregion EventCustomerSettlementPayoutRebate
        #region EventPayoutRebateRecognition
        public TradeActionEvent EventPayoutRebateRecognition
        {
            get
            {
                if (EventTypeFunc.IsPayout(eventType))
                    return CurrentEventParent.EventPayoutRecognition;
                else if (EventTypeFunc.IsRebate(eventType))
                    return CurrentEventParent.EventRebateRecognition;
                else
                    return null;
            }
        }
        #endregion EventPayoutRebateRecognition
        #region EventPayoutRebateSettlement
        public TradeActionEvent EventPayoutRebateSettlement
        {
            get
            {
                if (EventTypeFunc.IsPayout(eventType))
                    return CurrentEvent.EventPayoutSettlement;
                else if (EventTypeFunc.IsRebate(eventType))
                    return CurrentEvent.EventRebateSettlement;
                else
                    return null;
            }
        }
        #endregion EventPayoutRebateSettlement
        #region ForwardPoints
        protected decimal ForwardPoints
        {
            get
            {
                decimal forwardPoints = 0;
                if (m_CustomerSettlementPayoutRebateSpecified &&
                    m_CustomerSettlementPayoutRebate.Rate.ForwardPointsSpecified)
                {
                    forwardPoints = m_CustomerSettlementPayoutRebate.Rate.ForwardPoints.DecValue;
                }
                return forwardPoints;
            }
        }
        #endregion ForwardPoints
        #region IsTriggerPayoutSpecified
        private bool IsTriggerPayoutSpecified
        {
            get
            {
                bool isTriggerPayout = IsFieldSpecified("triggerPayout");
                if (false == isTriggerPayout)
                {
                    object product = CurrentProduct;
                    Type tProduct = product.GetType();
                    FieldInfo fldTriggerPayout = tProduct.GetField("triggerPayout");
                    if (null != fldTriggerPayout)
                    {
                        IFxOptionPayout fxOptionPayout = (IFxOptionPayout)fldTriggerPayout.GetValue(product);
                        isTriggerPayout = (null != fxOptionPayout);
                    }
                }
                return isTriggerPayout;
            }
        }
        #endregion IsTriggerPayoutSpecified
        #region QuoteBasis
        // EG 20160404 Migration vs2013
        protected new QuoteBasisEnum QuoteBasis
        {
            get
            {
                QuoteBasisEnum quoteBasis = QuoteBasisEnum.Currency1PerCurrency2;
                if (m_CustomerSettlementPayoutRebateSpecified)
                {
                    quoteBasis = m_CustomerSettlementPayoutRebate.Rate.QuotedCurrencyPair.QuoteBasis;
                }
                return quoteBasis;
            }
        }
        #endregion QuoteBasis
        #region SpotRate
        protected decimal SpotRate
        {
            get
            {
                decimal spotRate = 0;
                if (m_CustomerSettlementPayoutRebateSpecified &&
                    m_CustomerSettlementPayoutRebate.Rate.SpotRateSpecified)
                {
                    spotRate = m_CustomerSettlementPayoutRebate.Rate.SpotRate.DecValue;
                }
                return spotRate;
            }
        }
        #endregion SpotRate
        #endregion Accessors
        #region Constructors
        public PayoutRebateEvents(string pEventType, TradeActionEventBase pEvent, TradeActionEventBase pEventParent)
            : base(pEvent, pEventParent)
        {
            eventType = pEventType;
            m_TriggerPayoutSpecified = IsTriggerPayoutSpecified;
            originalBaseAmount = new EFS_Decimal();
            baseAmount = new EFS_Decimal();
            originalAmount = new EFS_Decimal();
            amount = new EFS_Decimal();

            if (m_TriggerPayoutSpecified)
            {
                m_OriginalPayoutRebate = EventPayoutRebateRecognition;
                m_OriginalBasePayoutRebate = m_OriginalPayoutRebate;
                m_FinalPayoutRebate = EventPayoutRebateSettlement;
                m_FinalBasePayoutRebate = m_FinalPayoutRebate;

                #region CustomerSettlementPayoutRebate
                m_CustomerSettlementPayoutRebate = CustomerSettlementPayoutRebate;
                m_CustomerSettlementPayoutRebateSpecified = (null != m_CustomerSettlementPayoutRebate);
                originalAmount.DecValue = m_OriginalPayoutRebate.valorisation.DecValue;
                if (m_CustomerSettlementPayoutRebateSpecified)
                {
                    m_OriginalBasePayoutRebate = EventCustomerSettlementPayoutRebate;
                    originalBaseAmount.DecValue = m_OriginalBasePayoutRebate.valorisation.DecValue;
                }
                else
                    originalBaseAmount.DecValue = originalAmount.DecValue;
                #endregion CustomerSettlementPayoutRebate
            }
        }
        #endregion Constructors
        #region Methods
        #region IsFieldSpecified
        public bool IsFieldSpecified(string pFieldName)
        {
            object product = CurrentProduct;
            Type tProduct = product.GetType();
            FieldInfo fld = tProduct.GetField(pFieldName + Cst.FpML_SerializeKeySpecified);
            if (null != fld)
                return (bool)fld.GetValue(product);
            return false;
        }
        #endregion IsFieldSpecified
        #region SetPayoutAmount
        protected void SetPayoutAmount(decimal pBaseAmount)
        {
            if (m_CustomerSettlementPayoutRebateSpecified)
            {
                if ((false == m_CustomerSettlementPayoutRebate.AmountSpecified) || 
                    (0 == m_CustomerSettlementPayoutRebate.Amount.DecValue))
                {
                    IQuotedCurrencyPair quote = m_CustomerSettlementPayoutRebate.Rate.QuotedCurrencyPair;
                    decimal rate = m_CustomerSettlementPayoutRebate.Rate.Rate.DecValue;
                    IMoney baseAmount = ((IProduct)this.CurrentProduct).ProductBase.CreateMoney(pBaseAmount, m_OriginalBasePayoutRebate.unit);
                    EFS_Cash cash = new EFS_Cash(SessionTools.CS, baseAmount, rate, quote);
                    amount.DecValue = cash.ExchangeAmountRounded;
                }
                else
                {
                    amount.DecValue = (pBaseAmount / m_OriginalPayoutRebate.valorisation.DecValue) * m_OriginalBasePayoutRebate.valorisation.DecValue;
                }
            }
            else
                amount.DecValue = pBaseAmount;
        }
        #endregion SetPayoutAmount
        #endregion Methods
    }
    #endregion PayoutRebateEvents
    #region RebateEvents
    public class RebateEvents : PayoutRebateEvents
    {
        #region Members
        public string payer;
        public string payerBook;
        public string receiver;
        public string receiverBook;
        public EFS_Date settlementDate;
        #endregion Members
        #region Accessors
        #region CreateHeaderCells_Capture
        private TableHeaderCell[] CreateHeaderCells_Capture
        {
            get
            {
                ArrayList aHeaderCell = new ArrayList
                {
                    TableTools.AddHeaderCell(ResFormActionDate, false, 0, UnitEnum.Pixel, 2, false)
                };
                if (m_TriggerPayoutSpecified)
                {
                    aHeaderCell.Add(TableTools.AddHeaderCell(ResFormValueDate, false, 0, UnitEnum.Pixel, 0, false));
                    aHeaderCell.Add(TableTools.AddHeaderCell(ResFormOriginalAmount, false, 0, UnitEnum.Pixel, 2, true));
                    if (m_CustomerSettlementPayoutRebateSpecified)
                        aHeaderCell.Add(TableTools.AddHeaderCell(ResFormOriginalBaseAmount, false, 0, UnitEnum.Pixel, 2, true));
                    aHeaderCell.Add(TableTools.AddHeaderCell(ResFormRealAmount, false, 0, UnitEnum.Pixel, 2, true));
                    if (m_CustomerSettlementPayoutRebateSpecified)
                        aHeaderCell.Add(TableTools.AddHeaderCell(ResFormRealBaseAmount, false, 0, UnitEnum.Pixel, 2, true));
                }
                aHeaderCell.Add(TableTools.AddHeaderCell(ResFormTitleNoteEvents, false, 0, UnitEnum.Pixel, 2, false));
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
                if (m_TriggerPayoutSpecified)
                {
                    aHeaderCell.Add(TableTools.AddHeaderCell(ResFormPayer, false, 100, UnitEnum.Pixel, 2, false));
                    aHeaderCell.Add(TableTools.AddHeaderCell(ResFormReceiver, false, 100, UnitEnum.Pixel, 2, false));
                }
                return (TableHeaderCell[])aHeaderCell.ToArray(typeof(TableHeaderCell));
            }
        }
        #endregion CreateHeaderCells_Static
        #region Resource in Form
        #region ResFormValueDate
        protected override string ResFormValueDate { get { return Ressource.GetString("DateSettlement"); } }
        #endregion ResFormValueDate
        #region ResFormOriginalAmount
        protected virtual string ResFormOriginalAmount { get { return Ressource.GetString("OriginalRebateAmount"); } }
        #endregion ResFormOriginalAmount
        #region ResFormOriginalBaseAmount
        protected virtual string ResFormOriginalBaseAmount { get { return Ressource.GetString("OriginalBaseRebateAmount"); } }
        #endregion ResFormOriginalBaseAmount
        #region ResFormRealAmount
        protected virtual string ResFormRealAmount { get { return Ressource.GetString("RealAmount"); } }
        #endregion ResFormRealAmount
        #region ResFormRealBaseAmount
        protected virtual string ResFormRealBaseAmount { get { return Ressource.GetString("RealBaseAmount"); } }
        #endregion ResFormRealBaseAmount
        #endregion Resource in Form
        #region SettlementDate
        public string SettlementDate
        {
            get
            {
                string settlementDate = Cst.HTMLSpace;
                if (DtFunc.IsDateTimeFilled(this.settlementDate.DateValue))
                    settlementDate = DtFunc.DateTimeToString(this.settlementDate.DateValue, DtFunc.FmtShortDate);
                return settlementDate;
            }
        }
        #endregion SettlementDate
        #endregion Accessors
        #region Constructors
        public RebateEvents(TradeActionEventBase pEvent, TradeActionEventBase pEventParent)
            : base(EventTypeFunc.Rebate, pEvent, pEventParent)
        {
            if (m_TriggerPayoutSpecified)
            {
                settlementDate = new EFS_Date();
                if (TradeAction is FX_TradeAction action)
                {
                    PayoutStyle = action.PayoutStyle(m_Event.instrumentNo);
                }

                if (null != m_FinalPayoutRebate)
                {
                    actionDate.DateValue = m_FinalPayoutRebate.EventClassSettlement.dtEvent.DateValue;
                    actionTime.TimeValue = m_FinalPayoutRebate.EventClassSettlement.dtEvent.DateTimeValue;
                    settlementDate.DateValue = actionDate.DateTimeValue;
                    amount.DecValue = m_FinalPayoutRebate.valorisation.DecValue;
                    baseAmount.DecValue = amount.DecValue;

                    if (m_CustomerSettlementPayoutRebateSpecified)
                    {
                        m_FinalBasePayoutRebate = m_FinalPayoutRebate.EventCustomerSettlementRebate;
                        baseAmount.DecValue = m_FinalBasePayoutRebate.valorisation.DecValue;
                    }
                }
                else
                {
                    actionDate.DateValue = m_OriginalPayoutRebate.dtEndPeriod.DateValue;
                    actionTime.TimeValue = m_OriginalPayoutRebate.dtEndPeriod.DateTimeValue;
                    settlementDate.DateValue = (PayoutEnum.Deferred == PayoutStyle ? ValueDate.DateValue : actionDate.DateValue);
                    //originalAmount.DecValue = m_OriginalPayoutRebate.valorisation.DecValue;
                    amount.DecValue = originalAmount.DecValue;
                    //if (m_CustomerSettlementPayoutRebateSpecified)
                    //    originalBaseAmount.DecValue = m_OriginalBasePayoutRebate.valorisation.DecValue;
                    //else
                    //    originalBaseAmount.DecValue = originalAmount.DecValue;
                    baseAmount.DecValue = originalBaseAmount.DecValue;

                }
                payer = m_OriginalPayoutRebate.payer;
                payerBook = m_OriginalPayoutRebate.payerBook;
                receiver = m_OriginalPayoutRebate.receiver;
                receiverBook = m_OriginalPayoutRebate.receiverBook;
                currency = m_OriginalPayoutRebate.unit;
                baseCurrency = currency;
                if (m_CustomerSettlementPayoutRebateSpecified)
                    baseCurrency = m_OriginalBasePayoutRebate.unit;
            }
            else
            {
                actionDate.DateValue = m_Event.dtEndPeriod.DateValue;
                actionTime.TimeValue = m_Event.dtEndPeriod.DateTimeValue;
            }
        }
        #endregion Constructors
        #region Methods
        #region AddCells_Capture
        private TableCell[] AddCells_Capture(TradeActionEvent pEvent, TradeActionEvent pEventParent)
        {
            ArrayList aCell = new ArrayList();
            m_EventParent = pEventParent;
            aCell.Add(TableTools.AddCell(ActionDate, HorizontalAlign.Center, 80, UnitEnum.Pixel));
            aCell.Add(TableTools.AddCell(actionTime.Value, HorizontalAlign.Center, 60, UnitEnum.Pixel));
            if (m_TriggerPayoutSpecified)
            {
                aCell.Add(TableTools.AddCell(SettlementDate, HorizontalAlign.Center, 80, UnitEnum.Pixel));
                aCell.Add(TableTools.AddCell(StrFunc.FmtDecimalToCurrentCulture(originalAmount.DecValue), HorizontalAlign.Right, 80, UnitEnum.Pixel));
                aCell.Add(TableTools.AddCell(m_OriginalPayoutRebate.unit, HorizontalAlign.Center));

                if (m_CustomerSettlementPayoutRebateSpecified)
                {
                    // Affichage caractéristiques CustomerSettlementRebate
                    aCell.Add(TableTools.AddCell(StrFunc.FmtDecimalToCurrentCulture(originalBaseAmount.DecValue), HorizontalAlign.Right, 80, UnitEnum.Pixel));
                    aCell.Add(TableTools.AddCell(m_OriginalBasePayoutRebate.unit, HorizontalAlign.Center));
                }
                aCell.Add(TableTools.AddCell(StrFunc.FmtDecimalToCurrentCulture(amount.DecValue), HorizontalAlign.Right, 80, UnitEnum.Pixel));
                aCell.Add(TableTools.AddCell(m_OriginalPayoutRebate.unit, HorizontalAlign.Center));
                if (m_CustomerSettlementPayoutRebateSpecified)
                {
                    // Affichage caractéristiques CustomerSettlementRebate
                    aCell.Add(TableTools.AddCell(StrFunc.FmtDecimalToCurrentCulture(baseAmount.DecValue), HorizontalAlign.Right, 80, UnitEnum.Pixel));
                    aCell.Add(TableTools.AddCell(m_OriginalBasePayoutRebate.unit, HorizontalAlign.Center));
                }
            }
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
            if (m_TriggerPayoutSpecified)
            {
                aCell.Add(TableTools.AddCell(payer, HorizontalAlign.NotSet));
                aCell.Add(TableTools.AddCell(payerBook, HorizontalAlign.NotSet));
                aCell.Add(TableTools.AddCell(receiver, HorizontalAlign.NotSet));
                aCell.Add(TableTools.AddCell(receiverBook, HorizontalAlign.NotSet));
            }
            aCell.AddRange(AddCells_Capture(pEvent, pEventParent));
            return (TableCell[])aCell.ToArray(typeof(TableCell));
        }
        #endregion AddCells_Static
        #region CreateControlAmount
        // 20080416 EG  Ticket 16154
        public TableRow CreateControlAmount
        {
            get
            {
                TableRow tr = new TableRow
                {
                    CssClass = "DataGrid_ItemStyle"
                };
                TableCell td = new TableCell();
                ControlGUI controlGUI = new ControlGUI(true, ResFormAmount)
                {
                    Regex = EFSRegex.TypeRegex.RegexAmountExtend,
                    LblWidth = 100
                };
                FpMLTextBox txtAmount = new FpMLTextBox(null, baseAmount.CultureValue, 200, "RebateBaseAmount", controlGUI, null, false, "TXTAMOUNT_" + idE, null,
                    new Validator("RebateBaseAmount", true),
                    new Validator(EFSRegex.TypeRegex.RegexAmountExtend, "RebateBaseAmount", true, false));
                GetFormatControlAttribute(txtAmount);
                td.Controls.Add(txtAmount);
                td.Controls.Add(new LiteralControl(Cst.HTMLSpace + m_OriginalBasePayoutRebate.unit));
                td.Controls.Add(new LiteralControl(Cst.HTMLSpace + "[" + PayoutStyle + "]"));
                tr.Cells.Add(td);
                return tr;
            }
        }
        #endregion CreateControlAmount
        #region CreateControlCurrentAction
        public TableRow[] CreateControlCurrentAction
        {
            get
            {
                ArrayList aTableRow = new ArrayList();
                #region ActionDate
                aTableRow.Add(base.CreateControlActionDate());
                #endregion ActionDate
                #region ActionTime
                aTableRow.Add(base.CreateControlActionTime());
                #endregion ActionTime

                if (m_TriggerPayoutSpecified)
                {
                    // 20080416 EG  Ticket 16154
                    aTableRow.Add(CreateControlTitleSeparator(ResFormTitleRebate, false));
                    #region SettlementDate
                    aTableRow.Add(CreateControlSettlementDate);
                    #endregion SettlementDate
                    #region RebateAmount
                    aTableRow.Add(CreateControlAmount);
                    #endregion RebateAmount
                }
                #region Description
                aTableRow.AddRange(base.CreateControlDescription);
                #endregion Description
                return (TableRow[])aTableRow.ToArray(typeof(TableRow));
            }
        }

        #endregion CreateControlCurrentAction
        #region CreateControlSettlementDate
        // 20080416 EG  Ticket 16154
        public TableRow CreateControlSettlementDate
        {
            get
            {
                TableRow tr = new TableRow
                {
                    CssClass = "DataGrid_ItemStyle"
                };
                TableCell td = new TableCell();
                ControlGUI controlGUI = new ControlGUI(true, ResFormValueDate)
                {
                    Regex = EFSRegex.TypeRegex.RegexDate,
                    LblWidth = 105
                };
                FpMLCalendarBox txtDate = new FpMLCalendarBox(null, settlementDate.DateValue, "ValueDate", controlGUI, null, "TXTVALUEDATE_" + idE,
                    new Validator("ValueDate", true),
                    new Validator(EFSRegex.TypeRegex.RegexDate, "ValueDate", true, false));
                GetFormatControlAttribute(txtDate);
                td.Controls.Add(txtDate);
                tr.Cells.Add(td);
                return tr;
            }
        }
        #endregion CreateControlSettlementDate
        #region CreateTableHeader
        public ArrayList CreateTableHeader(TradeActionEvent pEvent)
        {
            ArrayList aTableHeader = new ArrayList
            {
                pEvent.CurrentTradeAction.GetEventCodeTitle(pEvent.eventCode),
                CreateHeaderCells_Static,
                CreateHeaderCells_Capture,
                ResFormTitleComplementary
            };
            return aTableHeader;
        }
        #endregion CreateTableHeader
        #region IsEventChanged
        //20090129 PL/EG Il faudra voir si on peut affiner pour voir une donnée a réellement été modifié par le user...
        public override bool IsEventChanged(TradeActionEventBase pEvent)
        {
            bool isEventChanged = base.IsEventChanged(pEvent);
            if (false == isEventChanged)
            {
                isEventChanged = true;
            }
            return isEventChanged;
        }
        #endregion IsEventChanged
        #region PostedAction
        public object PostedAction(string pKeyAction)
        {
            // 20080416 EG  Ticket 16154
            string keyAction = pKeyAction + Convert.ToInt32(TradeActionCode.TradeActionCodeEnum.Rebate) + "_" + m_Event.eventType;
            // 20090306 RD  Ticket 16527
            DateTime dtSettlementDate = DateTime.MinValue;
            if (m_TriggerPayoutSpecified && null != settlementDate)
                dtSettlementDate = settlementDate.DateValue;
            return new RebateMsg(idE, ActionDateTime,
                CallAmount, PutAmount, PayoutStyle, 
                m_TriggerPayoutSpecified, dtSettlementDate, amount.DecValue, currency, baseAmount.DecValue, baseCurrency, 
                Currency1, Currency2, QuoteBasis , CustomerPaymentRate, SpotRate, ForwardPoints, payer, receiver, note, keyAction);
        }
        #endregion PostedAction
        #region Save
        public override bool Save(Page pPage)
        {
            bool isOk = ValidationRules(pPage);
            if (isOk)
            {
                isOk = base.Save(pPage);
                if (isOk)
                {
                    // 20080416 EG  Ticket 16154
                    if (null != pPage.Request.Form["TXTVALUEDATE_" + idE])
                        settlementDate.DateValue = new DtFunc().StringToDateTime(pPage.Request.Form["TXTVALUEDATE_" + idE]);
                    if (null != pPage.Request.Form["TXTAMOUNT_" + idE])
                    {
                        baseAmount.DecValue = DecFunc.DecValue(pPage.Request.Form["TXTAMOUNT_" + idE], Thread.CurrentThread.CurrentCulture);
                        SetPayoutAmount(baseAmount.DecValue);
                    }
                }
            }
            m_Event.isValidated = isOk;
            return isOk;
        }
        #endregion Save
        #region ValidationRules
        public override bool ValidationRules(Page pPage)
        {
            m_Event.validationRulesMessages = new ArrayList();
            return base.ValidationRules(pPage);
        }
        #endregion ValidationRules
        #endregion Methods
    }
    #endregion RebateEvents

    #region FxTriggerEvents
    public class FxTriggerEvents : FxBarrierAndTriggerEvents 
    {
        #region Constructors
        public FxTriggerEvents() { }
        public FxTriggerEvents(TradeActionEventBase pEvent, TradeActionEventBase pEventParent) : base(pEvent, pEventParent) { }
        #endregion Constructors
        #region Methods
        #region IsEventChanged
        public override bool IsEventChanged(TradeActionEventBase pEvent)
        {
            return base.IsEventChanged(pEvent);
        }
        #endregion IsEventChanged
        #region PostedAction
        public object PostedAction(string pKeyAction)
        {
            string keyAction = pKeyAction + Convert.ToInt32(TradeActionCode.TradeActionCodeEnum.Trigger) + "_" + m_Event.eventCode;
            return new TriggerMsg(idE, idStTrigger, ActionDateTime, touchRate.DecValue, note, keyAction);
        }
        #endregion PostedAction
        #region Save
        public override bool Save(Page pPage)
        {
            bool isOk = ValidationRules(pPage);
            if (isOk)
                isOk = base.Save(pPage);
            if (isOk)
                m_Event.m_OriginalAction = (FxTriggerEvents)m_Event.m_Action.Clone();

            m_Event.isValidated = isOk;
            return isOk;
        }
        #endregion Save
        #region ValidationRules
        public override bool ValidationRules(Page pPage)
        {
            bool isOk = true;
            m_Event.validationRulesMessages = new ArrayList();
            DateTime triggerMinDate = m_Event.dtStartPeriod.DateValue;
            DateTime triggerMaxDate = m_Event.dtEndPeriod.DateValue;
            // 20070813 EG  Correction du bug Ticket 15621. Lecture de la date d'action saisie par l'utilisateur
            DateTime dtAction = actionDate.DateValue;
            if (null != pPage.Request.Form["TXTACTIONDATE_" + idE])
                dtAction = new DtFunc().StringToDateTime(pPage.Request.Form["TXTACTIONDATE_" + idE]);
            if ((dtAction.CompareTo(triggerMinDate) < 0) || (dtAction.CompareTo(triggerMaxDate) > 0))
            {
                isOk = false;
                m_Event.validationRulesMessages.Add("Msg_IncorrectTriggerTouchDate");
            }
            if (isOk)
                isOk = base.ValidationRules(pPage);
            return isOk;
        }
        #endregion ValidationRules
        #endregion Methods
        #region Clone
        public override object Clone()
        {
            FxTriggerEvents clone = new FxTriggerEvents
            {
                idE = this.idE,
                actionDate = new EFS_Date(this.actionDate.Value),
                actionTime = new EFS_Time(this.actionTime.Value),
                touchRate = new EFS_Decimal(this.touchRate.DecValue),
                idStTrigger = this.idStTrigger,
                note = this.note
            };
            return clone;
        }
        #endregion Clone
    }
    #endregion FxTriggerEvents
}
