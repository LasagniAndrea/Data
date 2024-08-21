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
#endregion Debug Directives
#region Using Directives
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
using System.Text;
using System.Threading;
using System.Web.UI;
using System.Web.UI.WebControls;
#endregion Using Directives

namespace EfsML
{
    #region EQD_TradeAction
    /// <summary>Used by the trade action process (EQD family) </summary>
    public class EQD_TradeAction : TradeAction
    {
        #region Members
        private readonly IEquityOption equityOption;
        #endregion Members

        #region Accessors
        // EG 20150428 [20513] New
        /// <summary>
        /// Strike en Prix (Prix|Montant|Pourcentage)
        /// Courbe de taux non géré
        /// </summary>
        /// <returns></returns>
        public override IEquityStrike EquityStrike()
        {
            IEquityStrike equityStrike = null;
            if (equityOption.Strike.PriceSpecified)
                equityStrike = equityOption.Strike;
            return equityStrike;
        }

        #region Exercise
        // EG 20150428 [20513] New
        private IEquityExercise Exercise
        {
            get
            {
                IEquityExercise exercise = null;
                if (equityOption.EquityExercise.EquityExerciseAmericanSpecified)
                    exercise = equityOption.EquityExercise.EquityExerciseAmerican;
                else if (equityOption.EquityExercise.EquityExerciseBermudaSpecified)
                    exercise = equityOption.EquityExercise.EquityExerciseBermuda;
                else if (equityOption.EquityExercise.EquityExerciseEuropeanSpecified)
                    exercise = equityOption.EquityExercise.EquityExerciseEuropean;
                return exercise;
            }
        }
        #endregion Exercise

        #endregion Accessors
        #region Constructors
        public EQD_TradeAction() : base() { }
        public EQD_TradeAction(int pCurrentIdE, TradeActionType.TradeActionTypeEnum pTradeActionType,
            TradeActionMode.TradeActionModeEnum pTradeActionMode, DataDocumentContainer pDataDocumentContainer, TradeActionEvent pEvents)
            :base(pCurrentIdE, pTradeActionType, pTradeActionMode, pDataDocumentContainer, pEvents)
        {
            equityOption = dataDocumentContainer.CurrentProduct.Product as IEquityOption;
        }
        #endregion Constructors

        #region Override Accessors
        // EG 20150428 [20513] New
        public override bool IsAutomaticExercise
        {
            get { return equityOption.EquityExercise.AutomaticExerciseSpecified; }
        }
        // EG 20150428 [20513] New
        public override bool IsFeatures
        {
            get { return (null != equityOption) && equityOption.FeatureSpecified; }
        }
        // EG 20150428 [20513] New
        public override bool IsBarrierFeatures
        {
            get { return (null != equityOption) && equityOption.FeatureSpecified && equityOption.Feature.BarrierSpecified; }
        }
        // EG 20150428 [20513] New
        public override SettlementTypeEnum SettlementType
        {
            get {return equityOption.EquityExercise.SettlementType;}
        }
        // EG 20150428 [20513] New
        public override string SettlementCurrency
        {
            get {return equityOption.EquityExercise.SettlementCurrency.Value;}
        }
        #endregion Override Accessors

        #region Override Methods
        // EG 20150428 [20513] New
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        public override DateTime ExpiryDate()
        {
            IAdjustableOrRelativeDate expirationDate = Exercise.ExpirationDate;
            EFS_AdjustableDate adjustableExpiryDate = Tools.GetEFS_AdjustableDate(SessionTools.CS, expirationDate, dataDocumentContainer);
            DateTime expiryDate;
            if (adjustableExpiryDate.adjustableDateSpecified)
                expiryDate = adjustableExpiryDate.AdjustableDate.GetAdjustedDate().DateValue;
            else
                expiryDate = adjustableExpiryDate.adjustedDate.DateValue;
            return expiryDate;
        }
        // EG 20150428 [20513] New
        public override DateTime ValueDate()
        {
            DateTime valueDate = DateTime.MinValue;
            if (equityOption.EquityEffectiveDateSpecified)
                valueDate = equityOption.EquityEffectiveDate.DateValue;
            return valueDate;
        }
        // EG 20150428 [20513] New
        public override IExpiryDateTime ExpiryDateTime()
        {
            IExpiryDateTime expiryDateTime = null;
            IEquityExercise exercise = Exercise;
            if (null != exercise)
            {
                expiryDateTime = dataDocumentContainer.CurrentProduct.ProductBase.CreateExpiryDateTime();
                expiryDateTime.ExpiryDate.DateValue = ExpiryDate();
                if (exercise.EquityExpirationTimeSpecified)
                {
                    expiryDateTime.BusinessCenterTime.BusinessCenter.Value = exercise.EquityExpirationTime.BusinessCenter.Value;
                    expiryDateTime.BusinessCenterTime.HourMinuteTime.Value = exercise.EquityExpirationTime.HourMinuteTime.Value;
                }
            }
            return expiryDateTime;
        }

        // EG 20150428 [20513] New
        public override void InitializeProduct(ActionEventsBase pAction)
        {
            IProductBase product = dataDocumentContainer.CurrentProduct.Product.ProductBase;
            ActionEvents action = pAction as ActionEvents;
            action.ValueDate = new EFS_Date();
            action.SettlementRate = new Pair<Cst.PriceQuoteUnits, EFS_Decimal>();
            action.StrikePrice = new EFS_Decimal();
            action.SettlementAmount = new EFS_Decimal();

            string eventCode = action.CurrentEventParent.eventCode;
            action.IsEquityOption = true;
            action.IsAmerican = EventCodeFunc.IsAmericanEquityOption(eventCode);
            action.IsEuropean = EventCodeFunc.IsEuropeanEquityOption(eventCode);
            action.IsBermuda = EventCodeFunc.IsBermudaEquityOption(eventCode);

            action.ExpiryDate = ExpiryDate();
            IExpiryDateTime expiryDateTime = ExpiryDateTime();

            action.ValueDate.DateValue = ValueDate();

            action.Buyer = SetBuyerSeller(BuyerSellerEnum.BUYER);
            action.Seller = SetBuyerSeller(BuyerSellerEnum.SELLER);


            action.InitialEntitlement = action.CurrentEventParent.SetEventInitialUnderlyer(EventTypeFunc.SingleUnderlyer);
            action.Entitlement = new QuantityPayerReceiverInfo(action.InitialEntitlement.valorisation.DecValue,
                action.InitialEntitlement.PayerInfo, action.InitialEntitlement.ReceiverInfo);

            action.InitialNotional = action.CurrentEventParent.GetEventStart(EventTypeFunc.Nominal);
            action.Notional = new AmountPayerReceiverInfo(null, action.InitialNotional.valorisation.DecValue, action.InitialNotional.unit,
                action.InitialNotional.PayerInfo, action.InitialNotional.ReceiverInfo);

            action.FxStrikePrice = ((TradeAction)action.CurrentEvent.CurrentTradeAction).StrikePrice(action.CurrentEvent.instrumentNo);
            action.EquityStrike = EquityStrike();
            if (null != action.EquityStrike)
            {
                if (action.EquityStrike.PriceSpecified)
                    action.StrikePrice.DecValue = action.EquityStrike.Price.DecValue;
                else if (action.EquityStrike.PercentageSpecified)
                    action.StrikePrice.DecValue = action.EquityStrike.Percentage.DecValue;
            }

            action.SettlementType = SettlementType.ToString();
            action.SettlementCurrency = SettlementCurrency;

            action.FxFixing = product.CreateFxFixing();
            action.FxFixing.FixingTime = action.FxFixing.CreateBusinessCenterTime(expiryDateTime.BusinessCenterTime);
            if (null != action.FxStrikePrice)
            {
                // GLOP
                //if (StrikeQuoteBasisEnum.CallCurrencyPerPutCurrency == action.FxStrikePrice.strikeQuoteBasis)
                //    action.FxFixing.quotedCurrencyPair = action.FxFixing.CreateQuotedCurrencyPair(CallCurrency, PutCurrency, QuoteBasisEnum.Currency1PerCurrency2);
                //else
                //    action.FxFixing.quotedCurrencyPair = action.FxFixing.CreateQuotedCurrencyPair(PutCurrency, CallCurrency, QuoteBasisEnum.Currency1PerCurrency2);
            }

            if (null != action.FxStrikePrice)
                action.QuoteBasis = action.FxStrikePrice.StrikeQuoteBasis;

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
        private Pair<Pair<Nullable<int>, string>, Pair<Nullable<int>, string>> SetBuyerSeller(BuyerSellerEnum pBuyerSeller)
        {
            // EG 20150706 [21021] 
            Pair<Pair<Nullable<int>, string>, Pair<Nullable<int>, string>> actor = new Pair<Pair<Nullable<int>, string>, Pair<Nullable<int>, string>>();
            string hRef = string.Empty;
            switch (pBuyerSeller)
            {
                case BuyerSellerEnum.BUYER:
                    hRef = equityOption.BuyerPartyReference.HRef;
                    break;
                case BuyerSellerEnum.SELLER:
                    hRef = equityOption.SellerPartyReference.HRef;
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
    #endregion EQD_TradeAction

    #region EQD_AbandonEvents
    public class EQD_AbandonEvents : EQD_AbandonAndExerciseEvents
    {
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
                #region Qty
                aTableRow.Add(base.CreateControlQty(true));
                #endregion Qty
                #region Description
                aTableRow.AddRange(base.CreateControlDescription);
                #endregion Description
                return (TableRow[])aTableRow.ToArray(typeof(TableRow));
            }
        }
        #endregion CreateControlCurrentAction
        #endregion Accessors

        #region Constructors
        public EQD_AbandonEvents() { }
        public EQD_AbandonEvents(TradeActionEventBase pEvent, TradeActionEventBase pEventParent)
            : base(pEvent, pEventParent)
        {
            if (null != m_Event.EventClassGroupLevel)
            {
                #region Abandon Date & Time
                actionDate.DateValue = m_Event.dtEndPeriod.DateValue;
                actionTime.TimeValue = m_Event.dtEndPeriod.DateValue;
                #endregion Abandon Date & Time
                #region ValueDate
                ValueDate.DateValue = m_Event.EventClassGroupLevel.dtEvent.DateValue;
                #endregion ValueDate
                #region OptionEntitlement
                Entitlement = SetAmountAndQuantityExercise(EventTypeFunc.Underlyer, CurrentEvent.GetEventTermination(EventTypeFunc.SingleUnderlyer));
                #endregion OptionEntitlement
            }
            else
            {
                #region Abandon Date & Time
                actionDate.DateValue = ExpiryDate;
                actionTime.TimeValue = ExpiryDate;
                #endregion Abandon Date & Time
                #region ValueDate
                ValueDate.DateValue = (IsEuropean ? ValueDate.DateValue : ExpiryDate.AddDays(2)); // 2jo à gérer
                #endregion ValueDate
                #region OptionEntitlement
                if (EventTypeFunc.IsTotal(m_Event.eventType))
                {
                    #region Total abandon
                    Entitlement = SetAmountAndQuantityExercise(EventTypeFunc.SingleUnderlyer, InitialEntitlement);
                    #endregion Total abandon
                }
                else if (EventTypeFunc.IsPartiel(m_Event.eventType))
                {
                    #region Partial abandon
                    Entitlement = SetAmountAndQuantityExercise(EventTypeFunc.SingleUnderlyer, InitialEntitlement, m_EntitlementExercise_INT, m_EntitlementExercise_TER);
                    #endregion Partial abandon
                }
                #endregion OptionEntitlement
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
            return new EQD_AbandonMsg(idE, ActionDateTime, ValueDate.DateValue, abandonExerciseType, note, keyAction);
        }
        #endregion PostedAction
        #endregion Methods
    }
    #endregion EQD_AbandonEvents
    #region EQD_AbandonAndExerciseEvents
    public abstract class EQD_AbandonAndExerciseEvents : AbandonAndExerciseEventsBase
    {
        #region Members
        protected IEquityOption m_EquityOption;
        protected OptionTypeEnum m_OptionType;
        protected TradeActionEvent[] m_EntitlementExercise_INT;
        protected TradeActionEvent m_EntitlementExercise_TER;

        protected TradeActionEvent[] m_SettlementCcyAmount_INT;
        protected TradeActionEvent m_SettlementCcyAmount_TER;
        #endregion Members
        #region Accessors
        // TO CONTROL
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
        #region CreateControlQty
        protected TableRow CreateControlQty(bool pIsReadOnly)
        {
            TableRow tr = new TableRow
            {
                CssClass = "DataGrid_ItemStyle"
            };
            TableCell td = new TableCell();
            ControlGUI controlGUI = new ControlGUI(true, "optionEntitlement")
            {
                Regex = EFSRegex.TypeRegex.RegexInteger,
                LblWidth = 100
            };
            FpMLTextBox txtOptionEntitlement = new FpMLTextBox(null, Entitlement.Quantity.ToString(),
                150, "optionEntitlement", controlGUI, null, pIsReadOnly, "TXTOPTIONENTITLEMENT_" + idE, null,
                new Validator("optionEntitlement", true),
                new Validator(EFSRegex.TypeRegex.RegexAmountExtend, "optionEntitlement", true, false));
            StringBuilder sb = new StringBuilder();
            sb.Append(@"if (Page_ClientValidate())");
            sb.AppendFormat("javascript:__doPostBack('{0}','{1}');", txtOptionEntitlement.ID, TradeActionMode.TradeActionModeEnum.None);
            txtOptionEntitlement.Attributes.Add("onchange", sb.ToString());
            td.Controls.Add(txtOptionEntitlement);
            tr.Cells.Add(td);
            return tr;
        }
        #endregion CreateControlQty
        #region Resource in Form
        #region ResFormValueDate
        protected override string ResFormValueDate { get { return Ressource.GetString("ProvisionRelevantUnderlyingDate"); } }
        #endregion ResFormValueDate
        #endregion Resource in Form
        #endregion Accessors

        #region Constructors
        public EQD_AbandonAndExerciseEvents() { }
        public EQD_AbandonAndExerciseEvents(TradeActionEventBase pEvent, TradeActionEventBase pEventParent)
            : base(pEvent, pEventParent)
        {
            m_EquityOption = (IEquityOption)CurrentProduct;
            m_OptionType = m_EquityOption.OptionType;
            Notional = SetAmountAndQuantityExercise(EventTypeFunc.Nominal, InitialNotional);
            m_EntitlementExercise_INT = EventExercise(EventCodeFunc.Intermediary, EventTypeFunc.SingleUnderlyer);
            m_EntitlementExercise_TER = EventExercise(EventTypeFunc.SingleUnderlyer);

            m_SettlementCcyAmount_INT = EventExercise(EventCodeFunc.Intermediary, EventTypeFunc.SettlementCurrency);
            m_SettlementCcyAmount_TER = EventExercise(EventTypeFunc.SettlementCurrency);

            SetExerciseTypeList(null == m_EntitlementExercise_INT);
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
            }
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
        // TO CONTROL
        #endregion Methods
    }
    #endregion EQD_AbandonAndExerciseEvents
    #region EQD_ExerciseEvents
    public class EQD_ExerciseEvents : EQD_AbandonAndExerciseEvents
    {
        #region Members
        public string m_Currency;
        protected string m_QuoteSide;
        protected string m_QuoteTiming;
        protected KeyAssetFxRate m_PrimaryKeyAsset;
        protected bool m_PrimaryIdAssetSpecified;
        protected int m_PrimaryIdAsset;
        protected bool m_SecondaryKeyAssetSpecified;
        protected KeyAssetFxRate m_SecondaryKeyAsset;
        protected bool m_SecondaryIdAssetSpecified;
        protected int m_SecondaryIdAsset;

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

                    aCell.Add(TableTools.AddCell(StrFunc.FmtDecimalToCurrentCulture(m_Settlement.Amount), HorizontalAlign.Right, 80, UnitEnum.Pixel));
                    aCell.Add(TableTools.AddCell(m_Settlement.Currency, HorizontalAlign.Center));
                    aCell.Add(TableTools.AddCell(FormatSettlementRate, HorizontalAlign.Right));

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


                aCell2.Add(TableTools.AddCell(ResFormTouchRate, true));
                aCell2.Add(TableTools.AddCell(FormatSettlementRate, HorizontalAlign.Right));
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
                tr.Cells.AddRange((TableCell[])aCell2.ToArray(typeof(TableCell)));
                table.Rows.Add(tr);
                td.Controls.Add(table);
                aCell.Add(td);
                aCell.Add(TableTools.AddCell(FormatEquityStrikePrice, HorizontalAlign.Right));
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
                    aHeaderCell.Add(TableTools.AddHeaderCell(ResFormQty, false, 0, UnitEnum.Pixel, 0, false));
                    aHeaderCell.Add(TableTools.AddHeaderCell(ResFormSettlementMethod, false, 0, UnitEnum.Pixel, 0, true));
                }
                aHeaderCell.Add(TableTools.AddHeaderCell(ResFormSettlementRate, false, 0, UnitEnum.Pixel, 0, true));
                aHeaderCell.Add(TableTools.AddHeaderCell(ResFormStrike, false, 0, UnitEnum.Pixel, 0, false));
                aHeaderCell.Add(TableTools.AddHeaderCell(ResFormTitleNoteEvents, false, 0, UnitEnum.Pixel, 2, false));
                return (TableHeaderCell[])aHeaderCell.ToArray(typeof(TableHeaderCell));
            }
        }
        #endregion CreateHeaderCells_Capture

        #region StrikePriceValue
        protected override decimal StrikePriceValue
        {
            get
            {
                return StrikePrice.DecValue;
            }
        }
        #endregion StrikePriceValue

        // TO CONTROL
        #region Resource in Form
        #region ResFormAutomaticExercise
        protected virtual string ResFormAutomaticExercise { get { return Ressource.GetString("AutomaticExercise"); } }
        #endregion ResFormAutomaticExercise
        #region ResFormFallbackExercise
        protected virtual string ResFormFallbackExercise { get { return Ressource.GetString("FallbackExercise"); } }
        #endregion ResFormFallbackExercise
        #endregion Resource in Form
        #endregion Accessors
        #region Constructors
        public EQD_ExerciseEvents() { }
        public EQD_ExerciseEvents(TradeActionEventBase pEvent, TradeActionEventBase pEventParent)
            : base(pEvent, pEventParent)
        {
            SettlementRate = new Pair<Cst.PriceQuoteUnits, EFS_Decimal>();

            SettlementAmount = new EFS_Decimal();

            IEquityOption product = (IEquityOption)CurrentProduct;
            if (product.Underlyer.UnderlyerSingleSpecified && product.Underlyer.UnderlyerSingle.UnderlyingAsset.CurrencySpecified)
            {
                m_Currency = product.Underlyer.UnderlyerSingle.UnderlyingAsset.Currency.Value;
            }
            else if (product.Underlyer.UnderlyerBasketSpecified && product.Underlyer.UnderlyerBasket.BasketCurrencySpecified)
            {
                m_Currency = product.Underlyer.UnderlyerBasket.BasketCurrency.Value;
            }

            if (null != m_Event.EventClassGroupLevel)
            {
                #region Exercise Date & Time
                actionDate.DateValue = m_Event.dtEndPeriod.DateValue;
                actionTime.TimeValue = m_Event.dtEndPeriod.DateValue;
                #endregion Exercise Date & Time
                #region ValueDate
                ValueDate.DateValue = m_Event.EventClassGroupLevel.dtEvent.DateValue;
                #endregion ValueDate

                if (EventTypeFunc.IsTotal(m_Event.eventType) || EventTypeFunc.IsPartiel(m_Event.eventType))
                {
                    #region Total/Partial Exercise
                    #endregion Total/Partial Exercise
                }
                else
                {
                    #region Multiple Exercise
                    if (IsCashSettlement)
                    {
                    }
                    #endregion Multiple Exercise

                }
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
                    actionDate.DateValue = OTCmlHelper.GetDateSys(SessionTools.CS);
                    if (0 < actionDate.DateValue.CompareTo(ExpiryDate))
                        actionDate.DateValue = ExpiryDate;
                    actionTime.TimeValue = actionDate.DateTimeValue;
                }
                #endregion Exercise Date & Time
                #region ValueDate
                IOffset offset = ((IProduct)product).ProductBase.CreateOffset(PeriodEnum.D, 2, DayTypeEnum.Business);
                IBusinessCenters businessCenters = ((IOffset)offset).GetBusinessCentersCurrency(SessionTools.CS, null, m_Currency);
                ValueDate.DateValue = Tools.ApplyOffset(SessionTools.CS, ExpiryDate, offset, businessCenters);
                #endregion ValueDate
                #region Entitlement
                if (EventTypeFunc.IsMultiple(m_Event.eventType))
                {
                    #region Multiple exercise
                    Entitlement = SetAmountAndQuantityExercise(EventTypeFunc.SingleUnderlyer, InitialEntitlement, m_EntitlementExercise_INT, m_EntitlementExercise_TER);
                    #endregion Multiple exercise
                }
                else
                {
                    #region Total/Partial exercise
                    Entitlement = SetAmountAndQuantityExercise(EventTypeFunc.SingleUnderlyer, InitialEntitlement);
                    #endregion Total/Partial exercise
                }
                #endregion OptionEntitlement


                #region CashSettlement / BondPayment / AccruedInterest
                if (IsCashSettlement)
                {
                    SettlementRate = GetSettlementRate(SessionTools.CS, actionDate.DateValue);
                    decimal amount = CalculSettlementAmount(SessionTools.CS, Entitlement.Quantity, Notional.Amount, SettlementRate, StrikePrice.DecValue);
                    m_Settlement = new AmountPayerReceiverInfo(null, amount, m_Currency,
                        new PayerReceiverInfoDet(PayerReceiverEnum.Payer, Seller),
                        new PayerReceiverInfoDet(PayerReceiverEnum.Receiver, Buyer));

                }
                #endregion CashSettlement / BondPayment / AccruedInterest


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
        // EG 20170127 Qty Long To Decimal
        public override bool ValidationRules(Page pPage)
        {
            bool isOk = true;
            // EG 20150920 [21374] Int (int32) to Long (Int64) 
            // EG 20170127 Qty Long To Decimal
            decimal qty = 0;
            if (null != pPage.Request.Form["DDLEXERCISETYPE_" + idE])
                abandonExerciseType = pPage.Request.Form["DDLEXERCISETYPE_" + idE];

            if (null != pPage.Request.Form["TXTQTY_" + idE])
                qty = Convert.ToDecimal(pPage.Request.Form["TXTQTY_" + idE], Thread.CurrentThread.CurrentCulture);

            decimal initialEntitlement = InitialEntitlement.valorisation.DecValue;
            decimal entitlement_INT = 0;
            if (EventTypeFunc.IsMultiple(abandonExerciseType))
            {
                #region Qty control
                if (null != m_EntitlementExercise_INT)
                {
                    foreach (TradeActionEvent item in m_EntitlementExercise_INT)
                        entitlement_INT += item.valorisation.DecValue;

                    decimal remainingEntitlement = initialEntitlement - entitlement_INT;
                    isOk = (0 < qty) && (qty <= remainingEntitlement);
                }
                else
                    isOk = (qty < initialEntitlement);
                #endregion Qty control
                if (false == isOk)
                    m_Event.validationRulesMessages.Add("Msg_IncorrectMultipleExerciseQty");

            }
            else if (EventTypeFunc.IsPartiel(abandonExerciseType))
            {
                isOk = (0 < qty) && (qty < initialEntitlement);
                if (false == isOk)
                    m_Event.validationRulesMessages.Add("Msg_IncorrectPartialExerciseQty");
            }
            if (isOk)
                isOk = base.ValidationRules(pPage);
            return isOk;
        }
        #endregion ValidationRules

        // TO CONTROL
        #region CalculCashSettlementMoneyAmount
        public new void CalculCashSettlementMoneyAmount(PageBase pPage, string pControlId)
        {
            base.CalculCashSettlementMoneyAmount(pPage, pControlId);
            // GLOP EG EnableAmountCurrency(pPage);
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
                #region Qty
                aTableRow.Add(CreateControlTitleSeparator(ResFormQty, false));
                aTableRow.Add(base.CreateControlQty(EventTypeFunc.IsTotal(abandonExerciseType)));
                #endregion Qty
                #region Settlement Complements
                aTableRow.AddRange(CreateControlSettlement);
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
        #region CreateControlSettlement
        public TableRow[] CreateControlSettlement
        {
            get
            {
                ArrayList aTableRow = new ArrayList();
                StringBuilder sb = new StringBuilder();
                string title = IsCashSettlement ? ResFormTitleCashSettlement : ResFormTitlePhysicalSettlement;

                #region Title
                aTableRow.Add(CreateControlTitleSeparator(title, "LBLTITLE_" + idE, false));
                #endregion Title

                #region StrikePrice
                TableRow tr = new TableRow
                {
                    CssClass = "DataGrid_ItemStyle"
                };
                TableCell td = new TableCell();
                ControlGUI controlGUI = new ControlGUI
                {
                    Name = ResFormStrike,
                    Regex = EquityStrike.PercentageSpecified ? EFSRegex.TypeRegex.RegexPercent : EFSRegex.TypeRegex.RegexFixedRateExtend,
                    LblWidth = 100
                };
                FpMLTextBox txtStrikePrice = new FpMLTextBox(null, FormatEquityStrikePrice, 150, "StrikePrice", controlGUI,
                    null, true, "TXTSTRIKEPRICE_" + idE, null,
                    new Validator("StrikePrice", true),
                    new Validator(controlGUI.Regex, "StrikePrice", true, false));
                GetFormatControlAttribute(txtStrikePrice);
                td.Controls.Add(txtStrikePrice);
                tr.Cells.Add(td);
                aTableRow.Add(tr);
                #endregion StrikePrice
                if (IsCashSettlement)
                {
                    tr = new TableRow
                    {
                        CssClass = "DataGrid_ItemStyle"
                    };
                    td = new TableCell();
                    controlGUI = new ControlGUI
                    {
                        Name = ResFormExchangeRate,
                        Regex = SettlementRate.First == Cst.PriceQuoteUnits.Price ? EFSRegex.TypeRegex.RegexFixedRateExtend : EFSRegex.TypeRegex.RegexPercent,
                        LblWidth = 100
                    };
                    FpMLTextBox txtSettlementRate = new FpMLTextBox(null, FormatSettlementRate, 150, "SettlementRate", controlGUI,
                        null, false, "TXTSETTLEMENTRATE_" + idE, null,
                        new Validator("SettlementRate", true),
                        new Validator(controlGUI.Regex, "SettlementRate", true, false));
                    // EG 20150910 Branchement PostBack pour calcul CashSettlement amount
                    sb.Append(@"if (Page_ClientValidate())");
                    sb.AppendFormat("javascript:__doPostBack('{0}','{1}');", txtSettlementRate.ID,
                        TradeActionMode.TradeActionModeEnum.CalculCashSettlementOrBondPaymentAmount);
                    txtSettlementRate.Attributes.Add("onchange", sb.ToString());
                    //GetFormatControlAttribute(txtSettlementRate);
                    td.Controls.Add(txtSettlementRate);
                    td.Controls.Add(new LiteralControl(Cst.HTMLSpace));
                    tr.Cells.Add(td);
                    aTableRow.Add(tr);

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
                    // EG 20150910 Modification new Validator("SettlementAmount", true)
                    FpMLTextBox txtSettlementAmount = new FpMLTextBox(null, StrFunc.FmtDecimalToCurrentCulture(m_Settlement.Amount), 150, "SettlementAmount", controlGUI,
                        null, false, "TXTSETTLEMENTAMOUNT_" + idE, null,
                        new Validator("SettlementAmount", true),
                        new Validator(EFSRegex.TypeRegex.RegexAmountExtend, "SettlementAmount", true, false));
                    GetFormatControlAttribute(txtSettlementAmount);
                    td.Controls.Add(txtSettlementAmount);
                    td.Controls.Add(new LiteralControl(Cst.HTMLSpace + m_Settlement.Currency));
                    td.Controls.Add(new LiteralControl(Cst.HTMLSpace + ResFormPaidBy + Cst.HTMLSpace + m_Settlement.payer.actor.Second));
                    tr.Cells.Add(td);
                    aTableRow.Add(tr);
                    #endregion Settlement amount
                }
                return (TableRow[])aTableRow.ToArray(typeof(TableRow));
            }
        }
        #endregion CreateControlSettlement
        #region CalculCurrencyAmountExercise
        // EG 20160404 Migration vs2013
        public void CalculCurrencyAmountExercise(PageBase pPage, string pControlId)
        {
            FormatControl(pPage, pControlId);

            if (("DDLEXERCISETYPE_" + idE) == pControlId)
            {
                #region OptionEntitlement are disabled if exercise size is Total
                decimal qty = 0;
                Control ctrl = pPage.FindControl("TXTOPTIONENTITLEMENT_" + idE);
                if (null != ctrl)
                {
                    // GLOP qty = CurrentOptionEntitlement(exerciseType);
                    // EG 20170127 Qty Long To Decimal
                    ((TextBox)ctrl).Text = StrFunc.FmtDecimalToCurrentCulture(qty.ToString(NumberFormatInfo.InvariantInfo));
                }
                #endregion OptionEntitlement are disabled if exercise size is Total
            }
            else
            {
                // GLOP base.CalculCurrencyAmountExercise(pPage, pControlId);
            }
            #region settlementAmount (and InTheMoney/OutTheMoney) are recalculated
            CalculCashSettlementMoneyAmount(pPage, pControlId);
            #endregion settlementAmount (and InTheMoney/OutTheMoney) are recalculated
        }
        #endregion CalculCurrencyAmountExercise
        #region PostedAction
        public object PostedAction(string pKeyAction)
        {
            string keyAction = pKeyAction + Convert.ToInt32(TradeActionCode.TradeActionCodeEnum.Exercise) + "_" + m_Event.eventCode;
            return new EQD_ExerciseMsg(idE, ActionDateTime, ValueDate.DateValue, abandonExerciseType, note, keyAction);
        }
        #endregion PostedAction
        #region QuoteAssetReading
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

            SQL_Quote quote;
            if (m_PrimaryIdAssetSpecified)
                quote = new SQL_Quote(cs, QuoteEnum.FXRATE, AvailabilityEnum.Enabled, product.ProductBase, keyQuote, m_PrimaryIdAsset);
            else
                quote = new SQL_Quote(cs, QuoteEnum.FXRATE, AvailabilityEnum.Enabled, product.ProductBase, keyQuote, m_PrimaryKeyAsset);

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
                    quote = new SQL_Quote(cs, QuoteEnum.FXRATE, AvailabilityEnum.Enabled, product.ProductBase, keyQuote, m_SecondaryKeyAsset);
                pRateSourceType = RateSourceTypeEnum.SecondaryRateSource;
            }
            if (quote.IsLoaded)
            {
                m_QuoteSide = (keyQuote.QuoteSide != null) ? keyQuote.QuoteSide.ToString() : string.Empty;
                m_QuoteTiming = (keyQuote.QuoteTiming != null) ? keyQuote.QuoteTiming.ToString() : string.Empty;
                return quote.QuoteValue;
            }
            else
                return 0;
        }
        #endregion QuoteAssetReading
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

        #region GetSettlementRate
        private Pair<Cst.PriceQuoteUnits, EFS_Decimal> GetSettlementRate(string pCS, DateTime pFixingDate)
        {
            Pair<Cst.PriceQuoteUnits, EFS_Decimal> settlementRate = new Pair<Cst.PriceQuoteUnits, EFS_Decimal>
            {
                Second = new EFS_Decimal()
            };
            IProduct product = (IProduct)CurrentProduct;
            QuoteEnum quoteType = QuoteEnum.EQUITY;
            Nullable<int> idAsset = null;
            if (m_EquityOption.Underlyer.UnderlyerSingleSpecified)
            {
                Cst.UnderlyingAsset assetCategory = m_EquityOption.Underlyer.UnderlyerSingle.UnderlyingAsset.UnderlyerAssetCategory;
                idAsset = m_EquityOption.Underlyer.UnderlyerSingle.UnderlyingAsset.OTCmlId;
                switch (assetCategory)
                {
                    case Cst.UnderlyingAsset.EquityAsset:
                        quoteType = QuoteEnum.EQUITY;
                        break;
                    case Cst.UnderlyingAsset.Index:
                        quoteType = QuoteEnum.INDEX;
                        break;
                    case Cst.UnderlyingAsset.Bond:
                        quoteType = QuoteEnum.DEBTSECURITY;
                        break;
                }
            }
            else if (m_EquityOption.Underlyer.UnderlyerBasketSpecified)
            {
            }
            if (idAsset.HasValue)
            {
                m_QuoteSide = string.Empty;
                m_QuoteTiming = string.Empty;
                KeyQuote keyQuote = new KeyQuote(pCS, pFixingDate);
                SQL_Quote quote = new SQL_Quote(pCS, quoteType, AvailabilityEnum.Enabled, product.ProductBase, keyQuote, idAsset.Value);
                if (quote.IsLoaded)
                {
                    m_QuoteSide = (keyQuote.QuoteSide != null) ? keyQuote.QuoteSide.ToString() : string.Empty;
                    m_QuoteTiming = (keyQuote.QuoteTiming != null) ? keyQuote.QuoteTiming.ToString() : string.Empty;

                    settlementRate.First = (Cst.PriceQuoteUnits)ReflectionTools.EnumParse(new Cst.PriceQuoteUnits(), quote.QuoteUnit);
                    settlementRate.Second.DecValue = quote.QuoteValue;
                }
            }
            return settlementRate;
        }
        #endregion GetSettlementRate

        protected decimal CalculSettlementAmount(string pCs, decimal pNbOptions, decimal pNotionalAmount, Pair<Cst.PriceQuoteUnits, EFS_Decimal> pSettlementRate, decimal pStrikePrice)
        {
            decimal amount = 0;
            if ((null != pSettlementRate) && (null != pSettlementRate.Second) && (0 < pSettlementRate.Second.DecValue))
            {
                decimal settlementRate = pSettlementRate.Second.DecValue;

                // Si Strike exprimé en montant (Prix + Currency)
                if (OptionStrike.PriceSpecified && OptionStrike.CurrencySpecified)
                {
                    switch (m_OptionType)
                    {
                        case OptionTypeEnum.Call:
                            amount = pNbOptions * System.Math.Max(0, (settlementRate * pNotionalAmount) - pStrikePrice);
                            break;
                        case OptionTypeEnum.Put:
                            amount = pNbOptions * System.Math.Max(0, pStrikePrice - (settlementRate * pNotionalAmount));
                            break;
                    }
                }
                // Si Strike exprimé en pourcentage|prix
                else
                {
                    switch (m_OptionType)
                    {
                        case OptionTypeEnum.Call:
                            amount = pNbOptions * System.Math.Max(0, (settlementRate - pStrikePrice) * pNotionalAmount);
                            break;
                        case OptionTypeEnum.Put:
                            amount = pNbOptions * System.Math.Max(0, (pStrikePrice - settlementRate) * pNotionalAmount);
                            break;
                    }
                }
                EFS_Cash cash = new EFS_Cash(pCs, amount, SettlementCurrency);
                amount = cash.AmountRounded;
            }
            return amount;
        }

        #endregion Methods
    }
    #endregion EQD_ExerciseEvents
}
