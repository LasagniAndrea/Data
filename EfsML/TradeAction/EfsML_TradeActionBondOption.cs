#region Using Directives
// -------------------------------------------------------------------------------------------------------------------------
// EfsML_TradeActionBondOption : Contient l'ensemble des classe utilisées par les actions diverses liées aux Bond options 
// -------------------------------------------------------------------------------------------------------------------------
using EFS.ACommon;
using EFS.Actor;
using EFS.ApplicationBlocks.Data;
using EFS.Common;
using EFS.Common.MQueue;
using EFS.Common.Web;
using EFS.GUI.Attributes;
using EFS.GUI.CCI;
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
using System.Data;
using System.Globalization;
using System.Text;
using System.Threading;
using System.Web.UI;
using System.Web.UI.WebControls;

#endregion Using Directives

namespace EfsML
{
    #region BO_TradeAction
    /// <summary>
    /// Used by the trade action process (BO family) 
    /// </summary>
    public class BO_TradeAction : TradeAction
    {
        #region Members
        private readonly IDebtSecurityOption debtSecurityOption;
        #endregion Members

        #region Override Accessors
        // EG 20150428 [20513] New
        public override bool IsAutomaticExercise
        {
            get { return debtSecurityOption.ExerciseProcedure.ExerciseProcedureAutomaticSpecified; }
        }
        // EG 20150428 [20513] New
        public override bool IsFallBackExercise
        {
            get 
            { 
                return debtSecurityOption.ExerciseProcedure.ExerciseProcedureManualSpecified && 
                    debtSecurityOption.ExerciseProcedure.ExerciseProcedureManual.FallbackExerciseSpecified; 
            }
        }
        // EG 20150428 [20513] New
        public override bool IsFeatures
        {
            get {return (null != debtSecurityOption) && debtSecurityOption.FeatureSpecified;}
        }
        // EG 20150428 [20513] New
        public override bool IsBarrierFeatures
        {
            get { return (null != debtSecurityOption) && debtSecurityOption.FeatureSpecified && debtSecurityOption.Feature.BarrierSpecified; }
        }
        // EG 20150428 [20513] New
        public override SettlementTypeEnum SettlementType
        {
            get
            {
                SettlementTypeEnum settlementType = SettlementTypeEnum.Cash;
                if (debtSecurityOption.SettlementTypeSpecified)
                    settlementType = debtSecurityOption.SettlementType;
                return settlementType;
            }
        }
        // EG 20150428 [20513] New
        public override string SettlementCurrency
        {
            get
            {
                string settlementCurrency = string.Empty;
                if (debtSecurityOption.SettlementCurrencySpecified)
                    settlementCurrency = debtSecurityOption.SettlementCurrency.Value;
                else if (debtSecurityOption.NotionalAmountSpecified)
                    settlementCurrency = debtSecurityOption.NotionalAmount.Currency;
                return settlementCurrency;
            }
        }
        #endregion Override Accessors

        #region Constructors
        public BO_TradeAction() : base() { }
        public BO_TradeAction(int pCurrentIdE, TradeActionType.TradeActionTypeEnum pTradeActionType,
            TradeActionMode.TradeActionModeEnum pTradeActionMode, DataDocumentContainer pDataDocumentContainer, TradeActionEvent pEvents)
            :base(pCurrentIdE, pTradeActionType, pTradeActionMode, pDataDocumentContainer, pEvents)
        {
            debtSecurityOption = dataDocumentContainer.CurrentProduct.Product as IDebtSecurityOption;
        }

        #endregion Constructors

        #region Override Methods
        // EG 20150428 [20513] New
        /// <summary>
        /// Date d'expiration de l'option
        /// </summary>
        /// <returns></returns>
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        public override DateTime ExpiryDate()
        {
            IAdjustableOrRelativeDate expirationDate = null;
            DateTime expiryDate = DateTime.MinValue;
            if (debtSecurityOption.AmericanExerciseSpecified)
            {
                IAmericanExercise exercise = debtSecurityOption.AmericanExercise;
                expirationDate = exercise.ExpirationDate;
            }
            else if (debtSecurityOption.BermudaExerciseSpecified)
            {
                IBermudaExercise exercise = debtSecurityOption.BermudaExercise;
                if (exercise.BermudaExerciseDates.AdjustableDatesSpecified)
                {
                    foreach (IAdjustedDate adjustedDate in exercise.BermudaExerciseDates.AdjustableDates.UnadjustedDate)
                    {
                        if (0 < adjustedDate.DateValue.CompareTo(expiryDate))
                            expiryDate = adjustedDate.DateValue;
                    }
                }
                else if (exercise.BermudaExerciseDates.RelativeDatesSpecified)
                {
                    // TBD
                }
            }
            else if (debtSecurityOption.EuropeanExerciseSpecified)
            {
                IEuropeanExercise exercise = debtSecurityOption.EuropeanExercise;
                expirationDate = exercise.ExpirationDate;
            }
            if (null != expirationDate)
            {
                EFS_AdjustableDate adjustableExpiryDate = Tools.GetEFS_AdjustableDate(SessionTools.CS, (IAdjustableOrRelativeDate)expirationDate, dataDocumentContainer);
                if (adjustableExpiryDate.adjustableDateSpecified)
                    expiryDate = adjustableExpiryDate.AdjustableDate.GetAdjustedDate().DateValue;
                else
                    expiryDate = adjustableExpiryDate.adjustedDate.DateValue;
            }
            return expiryDate;
        }
        // EG 20150428 [20513] New
        /// <summary>
        /// Heure et BC d'expiration de l'option
        /// </summary>
        /// <returns></returns>
        public override IExpiryDateTime ExpiryDateTime()
        {
            IExpiryDateTime expiryDateTime = null;
            IExerciseBase exercise = ExerciseBase;
            if (null != exercise)
            {
                expiryDateTime = dataDocumentContainer.CurrentProduct.ProductBase.CreateExpiryDateTime();
                expiryDateTime.ExpiryDate.DateValue = ExpiryDate();
                expiryDateTime.BusinessCenterTime.BusinessCenter.Value = exercise.ExpirationTime.BusinessCenter.Value;
                expiryDateTime.BusinessCenterTime.HourMinuteTime.Value = exercise.ExpirationTime.HourMinuteTime.Value;
            }
            return expiryDateTime;
        }
        // EG 20150428 [20513] New
        /// <summary>
        /// Strike en Prix (Prix|Montant|Pourcentage)
        /// Courbe de taux non géré
        /// </summary>
        /// <returns></returns>
        public override IOptionStrike OptionStrike()
        {
            IOptionStrike optionStrike = null;
            if (debtSecurityOption.Strike.PriceSpecified)
                optionStrike = debtSecurityOption.Strike.Price;
            return optionStrike;
        }

        // EG 20150428 [20513] New
        /// <summary>
        /// Initialisation des données de base pour une action sur Bond Options
        /// </summary>
        /// <param name="pAction"></param>
        public override void InitializeProduct(ActionEventsBase pAction)
        {
            ActionEvents action = pAction as ActionEvents;
            action.IsBondOption = true;
            action.ValueDate = new EFS_Date();
            action.SettlementRate = new Pair<Cst.PriceQuoteUnits, EFS_Decimal>();
            action.StrikePrice = new EFS_Decimal();
            action.SettlementAmount = new EFS_Decimal();

            action.Buyer = SetBuyerSeller(BuyerSellerEnum.BUYER);
            action.Seller = SetBuyerSeller(BuyerSellerEnum.SELLER);

            string eventCode = action.CurrentEventParent.eventCode;
            action.IsAmerican = EventCodeFunc.IsAmericanBondOption(eventCode);
            action.IsEuropean = EventCodeFunc.IsEuropeanBondOption(eventCode);
            action.IsBermuda = EventCodeFunc.IsBermudaBondOption(eventCode);

            action.InitialNbOptions = action.CurrentEventParent.GetEventStart(EventTypeFunc.Quantity);
            // EG 20150706 [21021]
            //action.NbOptions = new QuantityPayerReceiverInfo(action.InitialNbOptions.valorisation.IntValue,
            //        action.InitialNbOptions.idPayer, action.InitialNbOptions.payer, action.InitialNbOptions.idPayerBook, action.InitialNbOptions.payerBook,
            //        action.InitialNbOptions.idReceiver, action.InitialNbOptions.receiver, action.InitialNbOptions.idReceiverBook, action.InitialNbOptions.receiverBook);
            // EG 20170127 Qty Long To Decimal
            action.NbOptions = new QuantityPayerReceiverInfo(action.InitialNbOptions.valorisation.DecValue, action.InitialNbOptions.PayerInfo, action.InitialNbOptions.ReceiverInfo);

            action.InitialEntitlement = action.CurrentEventParent.GetEventStart(EventTypeFunc.Underlyer);
            // EG 20150706 [21021]
            //action.Entitlement = new QuantityPayerReceiverInfo(action.InitialEntitlement.valorisation.IntValue,
            //        action.InitialEntitlement.idPayer, action.InitialEntitlement.payer, action.InitialEntitlement.idPayerBook, action.InitialEntitlement.payerBook,
            //        action.InitialEntitlement.idReceiver, action.InitialEntitlement.receiver, action.InitialEntitlement.idReceiverBook, action.InitialEntitlement.receiverBook);
            // EG 20170127 Qty Long To Decimal
            action.Entitlement = new QuantityPayerReceiverInfo(action.InitialEntitlement.valorisation.DecValue, action.InitialEntitlement.PayerInfo, action.InitialEntitlement.ReceiverInfo);

            action.InitialNotional = action.CurrentEventParent.GetEventStart(EventTypeFunc.Nominal);
            // EG 20150706 [21021]
            //action.Notional = new AmountPayerReceiverInfo(null, action.InitialNotional.valorisation.DecValue, action.InitialNotional.unit,
            //        action.InitialNotional.idPayer, action.InitialNotional.payer, action.InitialNotional.idPayerBook, action.InitialNotional.payerBook,
            //        action.InitialNotional.idReceiver, action.InitialNotional.receiver, action.InitialNotional.idReceiverBook, action.InitialNotional.receiverBook);
            // EG 20170127 Qty Long To Decimal
            action.Notional = new AmountPayerReceiverInfo(null, action.InitialNotional.valorisation.DecValue, action.InitialNotional.unit,
                action.InitialNotional.PayerInfo, action.InitialNotional.ReceiverInfo);

            action.SettlementType = SettlementType.ToString();
            action.SettlementCurrency = SettlementCurrency;
            action.ExpiryDate = ExpiryDate();
            _ = ExpiryDateTime();

            action.ValueDate.DateValue = this.TradeDate;

            action.FxStrikePrice = ((TradeAction)action.CurrentEvent.CurrentTradeAction).StrikePrice(action.CurrentEvent.instrumentNo);
            action.OptionStrike = OptionStrike();
            if (null != action.OptionStrike)
            {
                if (action.OptionStrike.PriceSpecified)
                    action.StrikePrice.DecValue = action.OptionStrike.Price.DecValue;
                else if (action.OptionStrike.PercentageSpecified)
                    action.StrikePrice.DecValue = action.OptionStrike.Percentage.DecValue;
            }
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
                    hRef = debtSecurityOption.BuyerPartyReference.HRef;
                    break;
                case BuyerSellerEnum.SELLER:
                    hRef = debtSecurityOption.SellerPartyReference.HRef;
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
        // EG 20150428 [20513] New
        private IExerciseBase ExerciseBase
        {
            get
            {
                IExerciseBase exercise = null;
                if (debtSecurityOption.AmericanExerciseSpecified)
                    exercise = debtSecurityOption.AmericanExercise;
                else if (debtSecurityOption.BermudaExerciseSpecified)
                    exercise = debtSecurityOption.BermudaExercise;
                else if (debtSecurityOption.EuropeanExerciseSpecified)
                    exercise = debtSecurityOption.EuropeanExercise;
                return exercise;
            }
        }
        #endregion Methods

    }
    #endregion BO_TradeAction

    #region BO_AbandonEvents
    /// <summary>
    /// Abandon sur Bond Options
    /// </summary>
    public class BO_AbandonEvents : BO_AbandonAndExerciseEvents
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
                #region NbOption / entitlement / NotionalAmount
                aTableRow.Add(base.CreateControlNbOptions(true));
                aTableRow.Add(base.CreateControlEntitlement());
                aTableRow.Add(base.CreateControlNotional());
                #endregion NbOption / entitlement / NotionalAmount
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
        public BO_AbandonEvents() { }
        public BO_AbandonEvents(TradeActionEventBase pEvent, TradeActionEventBase pEventParent)
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
                #region NbOption
                NbOptions = SetAmountAndQuantityExercise(EventTypeFunc.Underlyer, CurrentEvent.GetEventTermination(EventTypeFunc.Quantity));
                #endregion NbOption
            }
            else
            {
                #region Abandon Date & Time
                //actionDate.DateValue = ExpiryDate;
                //actionTime.TimeValue = ExpiryDate;
                if (IsEuropean)
                {
                    actionDate.DateValue = ExpiryDate;
                    actionTime.TimeValue = ExpiryDate;
                }
                else
                {
                    // FI 20200904 [XXXXX] call OTCmlHelper.GetDateSys
                    // actionDate.DateValue = OTCmlHelper.GetDateBusiness(SessionTools.CS);
                    actionDate.DateValue = OTCmlHelper.GetDateSys(SessionTools.CS);
                    if (0 < actionDate.DateValue.CompareTo(ExpiryDate))
                        actionDate.DateValue = ExpiryDate;
                    actionTime.TimeValue = actionDate.DateTimeValue;
                }
                #endregion Abandon Date & Time
                #region ValueDate
                //ValueDate.DateValue = (IsEuropean ? ValueDate.DateValue : ExpiryDate.AddDays(2)); // 2jo à gérer
                IOffset offset = ((IProduct)m_DebtSecurityOption).ProductBase.CreateOffset(PeriodEnum.D, 2, DayTypeEnum.Business);
                IBusinessCenters businessCenters = ((IOffset)offset).GetBusinessCentersCurrency(SessionTools.CS, null, m_Currency);
                ValueDate.DateValue = Tools.ApplyOffset(SessionTools.CS, actionDate.DateValue, offset, businessCenters);
                #endregion ValueDate
                #region NbOption
                if (EventTypeFunc.IsTotal(m_Event.eventType))
                {
                    #region Total abandon
                    NbOptions = SetAmountAndQuantityExercise(EventTypeFunc.Underlyer, InitialNbOptions);
                    #endregion Total abandon
                }
                else if (EventTypeFunc.IsPartiel(m_Event.eventType))
                {
                    #region Partial / Multiple abandon
                    NbOptions = SetAmountAndQuantityExercise(EventTypeFunc.Underlyer, InitialNbOptions, m_NbOptionExercise_INT, m_NbOptionExercise_TER);
                    #endregion Partial / Multiple abandon
                }
                #endregion NbOption
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
        #region PostedAction
        public object PostedAction(string pKeyAction)
        {
            string keyAction = pKeyAction + Convert.ToInt32(TradeActionCode.TradeActionCodeEnum.Abandon) + "_" + m_Event.eventCode;
            return new BO_AbandonMsg(idE, ActionDateTime, ValueDate.DateValue, abandonExerciseType, IsCashSettlement, NbOptions, Notional, Entitlement, note, keyAction);
        }
        #endregion PostedAction
        #endregion Methods
    }
    #endregion BO_AbandonEvents
    #region BO_AbandonAndExerciseEvents
    /// <summary>
    /// Classe abstraite commune à Abandon|Exercice sur Bond Options
    /// </summary>
    public abstract class BO_AbandonAndExerciseEvents : AbandonAndExerciseEventsBase
    {
        #region Members
        protected OptionTypeEnum m_OptionType;
        protected DebtSecurityContainer m_DebtSecurityContainer;
        protected DataSetEventTrade m_DsEventsDebtSecurity;
        protected IDebtSecurityOption m_DebtSecurityOption;

        protected string m_Currency;
        protected decimal m_ParAmount;
        protected TradeActionEvent[] m_NbOptionExercise_INT;
        protected TradeActionEvent m_NbOptionExercise_TER;
        protected TradeActionEvent m_BondPaymentAmount_TER;
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
                aCell2.Add(TableTools.AddCell(ResFormQty, true));
                // EG 20170127 Qty Long To Decimal
                // FI 2190520 [XXXXX] usage de NbOptions.Quantity.ToString(NumberFormatInfo.InvariantInfo)
                aCell2.Add(TableTools.AddCell(StrFunc.FmtDecimalToCurrentCulture(NbOptions.Quantity.ToString(NumberFormatInfo.InvariantInfo)), HorizontalAlign.Right, 80, UnitEnum.Percentage));
                tr.Cells.AddRange((TableCell[])aCell2.ToArray(typeof(TableCell)));
                table.Rows.Add(tr);

                aCell2 = new ArrayList();
                tr = new TableRow
                {
                    CssClass = m_Event.GetRowClass(table.CssClass)
                };
                aCell2.Add(TableTools.AddCell(ResFormEntitlement, true));
                // FI 2190520 [XXXXX] usage de Entitlement.Quantity.ToString(NumberFormatInfo.InvariantInfo)
                aCell2.Add(TableTools.AddCell(StrFunc.FmtDecimalToCurrentCulture(Entitlement.Quantity.ToString()), HorizontalAlign.Right, 80, UnitEnum.Percentage));
                tr.Cells.AddRange((TableCell[])aCell2.ToArray(typeof(TableCell)));
                table.Rows.Add(tr);

                aCell2 = new ArrayList();
                tr = new TableRow
                {
                    CssClass = m_Event.GetRowClass(table.CssClass)
                };
                aCell2.Add(TableTools.AddCell(ResFormNotional, true));
                aCell2.Add(TableTools.AddCell(StrFunc.FmtDecimalToCurrentCulture(Notional.Amount), HorizontalAlign.Right, 80, UnitEnum.Percentage));
                aCell2.Add(TableTools.AddCell(Notional.Currency, HorizontalAlign.Center));
                if (this is BO_ExerciseEvents)
                {
                    aCell2.Add(TableTools.AddCell(ResFormPaidBy, true));
                    aCell2.Add(TableTools.AddCell(Notional.payer.actor.Second, HorizontalAlign.NotSet));
                }
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
                using (TableRow tr = new TableRow())
                {
                    tr.CssClass = "DataGrid_ItemStyle";
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
        }
        #endregion CreateControlValueDate
        #region CreateControlEntitlement
        protected TableRow CreateControlEntitlement()
        {
            TableRow tr = new TableRow
            {
                CssClass = "DataGrid_ItemStyle"
            };
            TableCell td = new TableCell();
            ControlGUI controlGUI = new ControlGUI(true, "entitlement")
            {
                Regex = EFSRegex.TypeRegex.RegexInteger,
                LblWidth = 100
            };
            FpMLTextBox txtEntitlement = new FpMLTextBox(null, Entitlement.Quantity.ToString(), 150, "entitlement", controlGUI, null, true, "TXTENTITLEMENT_" + idE, null,
                new Validator("entitlement", true),
                new Validator(EFSRegex.TypeRegex.RegexAmountExtend, "entitlement", true, false));
            StringBuilder sb = new StringBuilder();
            sb.Append(@"if (Page_ClientValidate())");
            sb.AppendFormat("javascript:__doPostBack('{0}','{1}');", txtEntitlement.ID, TradeActionMode.TradeActionModeEnum.None);
            txtEntitlement.Attributes.Add("onchange", sb.ToString());
            td.Controls.Add(txtEntitlement);
            tr.Cells.Add(td);
            return tr;
        }
        #endregion CreateControlEntitlement
        #region CreateControlNbOptions
        protected TableRow CreateControlNbOptions(bool pIsReadOnly)
        {
            TableRow tr = new TableRow
            {
                CssClass = "DataGrid_ItemStyle"
            };
            TableCell td = new TableCell();
            ControlGUI controlGUI = new ControlGUI(true, ResFormQty)
            {
                Regex = EFSRegex.TypeRegex.RegexInteger,
                LblWidth = 100
            };
            FpMLTextBox txtNbOptions = new FpMLTextBox(null, NbOptions.Quantity.ToString(), 150, "nbOptions", controlGUI, null, pIsReadOnly, "TXTNBOPTIONS_" + idE, null,
                new Validator("nbOptions", true),
                new Validator(EFSRegex.TypeRegex.RegexInteger, "nbOptions", true, false));
            StringBuilder sb = new StringBuilder();
            sb.Append(@"if (Page_ClientValidate())");
            sb.AppendFormat("javascript:__doPostBack('{0}','{1}');", txtNbOptions.ID, TradeActionMode.TradeActionModeEnum.CalculCashSettlementOrBondPaymentAmount);
            txtNbOptions.Attributes.Add("onchange", sb.ToString());
            td.Controls.Add(txtNbOptions);
            tr.Cells.Add(td);
            return tr;
        }
        #endregion CreateControlNbOptions
        #region CreateControlNotional
        protected TableRow CreateControlNotional()
        {
            TableRow tr = new TableRow
            {
                CssClass = "DataGrid_ItemStyle"
            };
            TableCell td = new TableCell();
            ControlGUI controlGUI = new ControlGUI(true, ResFormNotional)
            {
                Regex = EFSRegex.TypeRegex.RegexInteger,
                LblWidth = 100
            };
            FpMLTextBox txtNotional = new FpMLTextBox(null, new EFS_Decimal(Notional.Amount).CultureValue, 150, "notional", controlGUI, null, true, "TXTNOTIONAL_" + idE, null,
                new Validator("notional", true),
                new Validator(EFSRegex.TypeRegex.RegexAmountExtend, "notional", true, false));
            StringBuilder sb = new StringBuilder();
            sb.Append(@"if (Page_ClientValidate())");
            sb.AppendFormat("javascript:__doPostBack('{0}','{1}');", txtNotional.ID, TradeActionMode.TradeActionModeEnum.None);
            txtNotional.Attributes.Add("onchange", sb.ToString());
            td.Controls.Add(txtNotional);
            td.Controls.Add(new LiteralControl(Cst.HTMLSpace + Notional.Currency));
            if (this is BO_ExerciseEvents)
                td.Controls.Add(new LiteralControl(Cst.HTMLSpace + ResFormPaidBy + Cst.HTMLSpace + Notional.payer.actor.Second));
            tr.Cells.Add(td);
            return tr;
        }
        #endregion CreateControlNotional

        #region Resource in Form
        #region ResFormValueDate
        protected override string ResFormValueDate { get { return Ressource.GetString("ProvisionRelevantUnderlyingDate"); } }
        #endregion ResFormValueDate
        #endregion Resource in Form
        #endregion Accessors

        #region Constructors
        public BO_AbandonAndExerciseEvents() { }
        public BO_AbandonAndExerciseEvents(TradeActionEventBase pEvent, TradeActionEventBase pEventParent)
            : base(pEvent, pEventParent)
        {
            m_DebtSecurityOption = (IDebtSecurityOption)CurrentProduct;
            m_OptionType = m_DebtSecurityOption.OptionType;
            m_DebtSecurityContainer = new DebtSecurityContainer(m_DebtSecurityOption.DebtSecurity);
            m_DsEventsDebtSecurity = new DataSetEventTrade(SessionTools.CS, m_DebtSecurityOption.SecurityAssetOTCmlId);

            if (m_DebtSecurityOption.BondSpecified && m_DebtSecurityOption.Bond.CurrencySpecified)
            {
                m_Currency = m_DebtSecurityOption.Bond.Currency.Value;
                m_ParAmount = m_DebtSecurityOption.Bond.ParValue.DecValue;
            }
            else if (m_DebtSecurityOption.ConvertibleBondSpecified && m_DebtSecurityOption.ConvertibleBond.CurrencySpecified)
            {
                m_Currency = m_DebtSecurityOption.ConvertibleBond.Currency.Value;
                m_ParAmount = m_DebtSecurityOption.ConvertibleBond.ParValue.DecValue;
            }

            m_NbOptionExercise_INT = EventExercise(EventCodeFunc.Intermediary, EventTypeFunc.Quantity);
            m_NbOptionExercise_TER = EventExercise(EventTypeFunc.Quantity);
            m_SettlementAmount_TER = EventExercise(EventTypeFunc.SettlementCurrency);
            m_BondPaymentAmount_TER = EventExercise(EventTypeFunc.BondPayment);

            #region Entitlement / NotionalAmount
            Entitlement = SetAmountAndQuantityExercise(EventTypeFunc.Underlyer, InitialEntitlement);
            Notional = SetAmountAndQuantityExercise(EventTypeFunc.Nominal, InitialNotional);
            #endregion Entitlement / NotionalAmount

            SetExerciseTypeList(null == m_NbOptionExercise_INT);
        }
        #endregion Constructors

        #region Methods
        #region SetNbOptionsAndNotionalAmount
        protected void SetNbOptionsAndNotionalAmount(TextBox pTextBox, decimal pValue)
        {
            pTextBox.Text = StrFunc.FmtDecimalToCurrentCulture(pValue.ToString(NumberFormatInfo.InvariantInfo));
        }
        #endregion SetNbOptionsAndNotionalAmount

        #region CurrentNbOptions
        protected decimal CurrentNbOptions(string pExerciseType)
        {
            decimal nbOptions = InitialNbOptions.valorisation.DecValue;
            if (EventTypeFunc.IsMultiple(pExerciseType))
            {
                decimal nbOptionExercise_INT = 0;
                if (null != m_NbOptionExercise_INT)
                {
                    foreach (TradeActionEvent item in m_NbOptionExercise_INT)
                        nbOptionExercise_INT += item.valorisation.DecValue;
                    nbOptions -= nbOptionExercise_INT;
                }
                if (null != m_NbOptionExercise_TER)
                    nbOptions -= m_NbOptionExercise_TER.valorisation.DecValue;
            }
            return nbOptions;
        }
        #endregion CurrentNbOptions

        #region EnableNbOptions
        protected void EnableNbOptions(PageBase pPage)
        {
            if (null != pPage.Request.Form["DDLEXERCISETYPE_" + idE])
            {
                string exerciseType = pPage.Request.Form["DDLEXERCISETYPE_" + idE];
                bool isReadOnly = EventTypeFunc.IsTotal(exerciseType);

                Control ctrl = pPage.FindControl("TXTNBOPTIONS_" + idE);
                if (null != ctrl)
                    ((FpMLTextBox)ctrl).IsLocked = isReadOnly || (1 == CurrentNbOptions(exerciseType));
            }
        }
        #endregion EnableNbOptions
        #region IsEventChanged
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
        #endregion Methods
    }
    #endregion BO_AbandonAndExerciseEvents
    #region BO_ExerciseEvents
    public class BO_ExerciseEvents : BO_AbandonAndExerciseEvents
    {
        #region Members
        protected string m_QuoteSide;
        protected string m_QuoteTiming;
        protected KeyQuote keyQuote;
        protected AmountPayerReceiverInfo m_Settlement;
        protected AmountPayerReceiverInfo m_BondPayment;
        protected AmountPayerReceiverInfo m_AccruedInterest;
        #endregion Members
        #region Accessors
        #region AddCells_Capture
        // EG 20150605 Déplacement SettlementRate dans IsCashSettlement
        public override TableCell[] AddCells_Capture
        {
            get
            {
                ArrayList aCell = new ArrayList();
                aCell.AddRange(base.AddCells_Capture);
                aCell.Add(TableTools.AddCell(SettlementType.ToString(), HorizontalAlign.Center));
                if (IsCashSettlement)
                {
                    aCell.Add(TableTools.AddCell(StrFunc.FmtDecimalToCurrentCulture(m_Settlement.Amount), HorizontalAlign.Right, 80, UnitEnum.Pixel));
                    aCell.Add(TableTools.AddCell(m_Settlement.Currency, HorizontalAlign.Center));
                    aCell.Add(TableTools.AddCell(FormatSettlementRate, HorizontalAlign.Right));
                }
                else
                {
                    aCell.Add(AddCells_BondPaymentCapture);
                }
                aCell.Add(TableTools.AddCell(FormatStrikePrice, HorizontalAlign.Right));
                aCell.Add(TableTools.AddCell(note, HorizontalAlign.NotSet, 100, UnitEnum.Percentage, true, false, false));
                aCell.Add(TableTools.AddCell(Cst.HTMLSpace, HorizontalAlign.Center, 0, UnitEnum.Pixel));


                return (TableCell[])aCell.ToArray(typeof(TableCell));
            }
        }
        #endregion AddCells_Capture
        #region AddCells_BondPaymentCapture
        private TableCell AddCells_BondPaymentCapture
        {
            get
            {
                ArrayList aCell = new ArrayList();
                Table table = new Table();
                TableRow tr = new TableRow();
                table.CssClass = "subActionDataGrid";
                table.CellPadding = 0;
                table.CellSpacing = 0;
                table.Height = Unit.Percentage(100);
                tr.CssClass = m_Event.GetRowClass(table.CssClass);
                aCell.Add(TableTools.AddCell(ResFormAccruedInterestAmount, true));
                //aCell.Add(TableTools.AddCell(StrFunc.FmtDecimalToCurrentCulture(AccruedInterestAmount.DecValue), HorizontalAlign.Right, 80, UnitEnum.Percentage));
                aCell.Add(TableTools.AddCell(StrFunc.FmtDecimalToCurrentCulture(m_AccruedInterest.Amount), HorizontalAlign.Right, 80, UnitEnum.Percentage));
                aCell.Add(TableTools.AddCell(Notional.Currency, HorizontalAlign.Center));
                tr.Cells.AddRange((TableCell[])aCell.ToArray(typeof(TableCell)));
                table.Rows.Add(tr);

                aCell = new ArrayList();
                tr = new TableRow
                {
                    CssClass = m_Event.GetRowClass(table.CssClass)
                };
                aCell.Add(TableTools.AddCell(ResFormBondPaymentAmount, true));
                aCell.Add(TableTools.AddCell(StrFunc.FmtDecimalToCurrentCulture(m_BondPayment.Amount), HorizontalAlign.Right, 80, UnitEnum.Percentage));
                aCell.Add(TableTools.AddCell(Notional.Currency, HorizontalAlign.Center));
                aCell.Add(TableTools.AddCell(ResFormPaidBy, true));
                aCell.Add(TableTools.AddCell(Notional.payer.actor.Second, HorizontalAlign.NotSet));
                tr.Cells.AddRange((TableCell[])aCell.ToArray(typeof(TableCell)));
                table.Rows.Add(tr);

                TableCell td = new TableCell();
                td.Controls.Add(table);
                return td;
            }
        }
        #endregion AddCells_BondPaymentCapture
        #region CreateHeaderCells_Capture
        // EG 20150605 Déplacement SettlementRate dans IsCashSettlement
        public override TableHeaderCell[] CreateHeaderCells_Capture
        {
            get
            {
                ArrayList aHeaderCell = new ArrayList();
                aHeaderCell.AddRange(base.CreateHeaderCells_Capture);
                aHeaderCell.Add(TableTools.AddHeaderCell(ResFormQty, false, 0, UnitEnum.Pixel, 0, false));
                aHeaderCell.Add(TableTools.AddHeaderCell(ResFormSettlementMethod, false, 0, UnitEnum.Pixel, 0, true));
                aHeaderCell.Add(TableTools.AddHeaderCell(ResFormAmount, false, 0, UnitEnum.Pixel, IsCashSettlement?2:0, false));
                if (IsCashSettlement)
                    aHeaderCell.Add(TableTools.AddHeaderCell(ResFormSettlementRate, false, 0, UnitEnum.Pixel, 0, true));
                aHeaderCell.Add(TableTools.AddHeaderCell(ResFormStrike, false, 0, UnitEnum.Pixel, 0, false));
                aHeaderCell.Add(TableTools.AddHeaderCell(ResFormTitleNoteEvents, false, 0, UnitEnum.Pixel, 2, false));
                return (TableHeaderCell[])aHeaderCell.ToArray(typeof(TableHeaderCell));
            }
        }
        #endregion CreateHeaderCells_Capture

        #region Resource in Form
        #region ResFormTitleAbandonExerciseDate
        protected override string ResFormTitleAbandonExerciseDate { get { return Ressource.GetString("ExerciseDate"); } }
        #endregion ResFormTitleAbandonExerciseDate
        #region ResFormAccruedInterest
        protected virtual string ResFormAccruedInterestAmount { get { return Ressource.GetString("AccruedInterestAmount"); } }
        #endregion ResFormAccruedInterest
        #region ResFormBondPayment
        protected virtual string ResFormBondPaymentAmount { get { return Ressource.GetString("BondPaymentAmount"); } }
        #endregion ResFormBondPayment

        #region ResFormAutomaticExercise
        protected virtual string ResFormAutomaticExercise { get { return Ressource.GetString("AutomaticExercise"); } }
        #endregion ResFormAutomaticExercise
        #region ResFormFallbackExercise
        protected virtual string ResFormFallbackExercise { get { return Ressource.GetString("FallbackExercise"); } }
        #endregion ResFormFallbackExercise
        #endregion Resource in Form
        #endregion Accessors

        #region Constructors
        public BO_ExerciseEvents() { }
        public BO_ExerciseEvents(TradeActionEventBase pEvent, TradeActionEventBase pEventParent)
            : base(pEvent, pEventParent)
        {
            SettlementRate = new Pair<Cst.PriceQuoteUnits,EFS_Decimal>();

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
                #region NbOptions / SettlementAmount / BondPayment
                if (EventTypeFunc.IsTotal(m_Event.eventType) || EventTypeFunc.IsPartiel(m_Event.eventType))
                {
                    #region Total/Partial Exercise
                    // NbOptions / Entitlement / Notional
                    NbOptions = SetAmountAndQuantityExercise(EventTypeFunc.Quantity, m_NbOptionExercise_TER);
                    if (null != m_SettlementAmount_TER)
                    {
                        m_Settlement = GetBondAmountInfo(EventTypeFunc.SettlementCurrency, m_SettlementAmount_TER.valorisation.DecValue, m_SettlementAmount_TER.unit);
                        if (m_SettlementAmount_TER.detailsSpecified)
                        {
                            details = m_SettlementAmount_TER.details;
                            // EG 20150617 [21091] Alimentation SettlementRate
                            if (details.settlementRateSpecified)
                                SettlementRate = new Pair<Cst.PriceQuoteUnits, EFS_Decimal>(Cst.PriceQuoteUnits.ParValueDecimal, details.settlementRate);
                        }
                    }
                    else if (null != m_BondPaymentAmount_TER)
                    {
                        m_BondPayment = GetBondAmountInfo(EventTypeFunc.BondPayment, m_BondPaymentAmount_TER.valorisation.DecValue, m_BondPaymentAmount_TER.unit);
                        if (m_BondPaymentAmount_TER.detailsSpecified)
                        {
                            details = m_BondPaymentAmount_TER.details;
                            // EG 20150617 [21091] Mis en commentaire
                            //if (details.settlementRateSpecified)
                            //{
                            //    SettlementRate.Second.DecValue = details.settlementRate.DecValue;
                            //}
                            if (details.interestSpecified)
                                m_AccruedInterest = GetBondAmountInfo(EventTypeFunc.BondPayment, details.interest.DecValue, m_BondPaymentAmount_TER.unit);
                        }
                    }
                    #endregion Total/Partial Exercise
                }
                else
                {
                    #region Multiple Exercise
                    TradeActionEvent eventNbOptions = CurrentEvent.GetEvent(EventCodeFunc.Intermediary, EventTypeFunc.Quantity);
                    if (null == eventNbOptions)
                        eventNbOptions = CurrentEvent.GetEvent(EventCodeFunc.Termination, EventTypeFunc.Quantity);

                    NbOptions = SetAmountAndQuantityExercise(EventTypeFunc.Quantity, eventNbOptions);

                    if (IsCashSettlement)
                    {
                        TradeActionEvent eventSettlement = CurrentEvent.GetEvent(EventCodeFunc.Intermediary, EventTypeFunc.SettlementCurrency);
                        if (null == eventSettlement)
                            eventSettlement = CurrentEvent.GetEvent(EventCodeFunc.Termination, EventTypeFunc.SettlementCurrency);
                        m_Settlement = GetBondAmountInfo(EventTypeFunc.SettlementCurrency, eventSettlement.valorisation.DecValue, eventSettlement.unit);
                        if (eventSettlement.detailsSpecified)
                        {
                            details = eventSettlement.details;
                            // EG 20150617 [21091] Alimentation SettlementRate
                            if (details.settlementRateSpecified)
                                SettlementRate = new Pair<Cst.PriceQuoteUnits, EFS_Decimal>(Cst.PriceQuoteUnits.ParValueDecimal, details.settlementRate);
                        }
                    }
                    else
                    {
                        TradeActionEvent eventBondPayment = CurrentEvent.GetEvent(EventCodeFunc.Intermediary, EventTypeFunc.BondPayment);
                        if (null == eventBondPayment)
                            eventBondPayment = CurrentEvent.GetEvent(EventCodeFunc.Termination, EventTypeFunc.BondPayment);

                        m_BondPayment = GetBondAmountInfo(EventTypeFunc.BondPayment, eventBondPayment.valorisation.DecValue, eventBondPayment.unit);
                        if (eventBondPayment.detailsSpecified)
                        {
                            details = eventBondPayment.details;
                            // EG 20150617 [21091] Mis en commentaire
                            //if (details.settlementRateSpecified)
                            //    SettlementRate.Second.DecValue = details.settlementRate.DecValue;
                            if (details.interestSpecified)
                                m_AccruedInterest = GetBondAmountInfo(EventTypeFunc.BondPayment, details.interest.DecValue, eventBondPayment.unit);
                        }
                    }
                    #endregion Multiple Exercise
                }
                #endregion NbOptions / SettlementAmount / BondPayment

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
                IOffset offset = ((IProduct)m_DebtSecurityOption).ProductBase.CreateOffset(PeriodEnum.D, 2, DayTypeEnum.Business);
                IBusinessCenters businessCenters = ((IOffset)offset).GetBusinessCentersCurrency(SessionTools.CS, null, m_Currency);
                ValueDate.DateValue = Tools.ApplyOffset(SessionTools.CS, actionDate.DateValue, offset, businessCenters);
                #endregion ValueDate
                #region NbOption
                if (EventTypeFunc.IsMultiple(m_Event.eventType))
                {
                    #region Multiple exercise
                    NbOptions = SetAmountAndQuantityExercise(EventTypeFunc.Quantity, InitialNbOptions, m_NbOptionExercise_INT, m_NbOptionExercise_TER);
                    #endregion Multiple exercise
                }
                else
                {
                    #region Total/Partial exercise
                    NbOptions = SetAmountAndQuantityExercise(EventTypeFunc.Quantity, InitialNbOptions);
                    #endregion Total/Partial exercise
                }
                #endregion NbOption
                #region CashSettlement / BondPayment / AccruedInterest
                if (IsCashSettlement)
                {
                    // EG 20150605 Déplacement Alimentation de SettlementRate dans IsCashSettlement
                    SettlementRate = GetSettlementRate(SessionTools.CS, m_DebtSecurityOption.SecurityAssetOTCmlId, actionDate.DateValue);
                    decimal amount = CalculSettlementAmount(SessionTools.CS, NbOptions.Quantity, Notional.Amount, SettlementRate, StrikePrice.DecValue);
                    m_Settlement = GetBondAmountInfo(EventTypeFunc.SettlementCurrency, amount, m_Currency);
                }
                else
                {
                    decimal amount = CalculAccruedInterestAmount(SessionTools.CS, actionDate.DateValue, Entitlement.Quantity);
                    m_AccruedInterest = GetBondAmountInfo(EventTypeFunc.AccruedInterestAmount, amount, m_Currency);
                    amount = CalculBondPaymentAmount(SessionTools.CS, NbOptions.Quantity, Entitlement.Quantity, m_AccruedInterest.Amount, StrikePrice.DecValue);
                    m_BondPayment = GetBondAmountInfo(EventTypeFunc.BondPayment, amount, m_Currency);
                }
                #endregion CashSettlement / BondPayment / AccruedInterest
            }
        }
        #endregion Constructors

        #region Methods
        #region CalculCashSettlementOrBondPaymentAmount
        /// <summary>
        /// Calcul du CashSettlementAmount (si Cash)
        /// Calcul du BondPaymentAmount (si Physical)
        /// </summary>
        /// <param name="pPage"></param>
        /// <param name="pControlId"></param>
        public void CalculCashSettlementOrBondPaymentAmount(PageBase pPage, string pControlId)
        {
            FormatControl(pPage, pControlId);

            decimal nbOptions = NbOptions.Quantity;
            decimal notionalAmount = Notional.Amount;

            Control ctrl = pPage.FindControl("TXTNBOPTIONS_" + idE);
            if (null != ctrl)
                nbOptions = DecFunc.DecValue(((TextBox)ctrl).Text, Thread.CurrentThread.CurrentCulture);

            if (IsCashSettlement)
            {
                Pair<Cst.PriceQuoteUnits, EFS_Decimal> settlementRate = SettlementRate;

                ctrl = pPage.FindControl("TXTSETTLEMENTRATE_" + idE);
                if (null != ctrl)
                {
                    // EG 20150910 Modification formatage du SettelementRate
                    //settlementRate.Second.DecValue = DecFunc.DecValue(((TextBox)ctrl).Text, Thread.CurrentThread.CurrentCulture);
                    FpMLTextBox txtSettlementRate = ctrl as FpMLTextBox;
                    CustomCaptureInfo cci = new CustomCaptureInfo
                    {
                        Regex = txtSettlementRate.Regex,
                        NewValueFromLiteral = ((TextBox)ctrl).Text
                    };
                    settlementRate.Second.Value = cci.NewValue;
                }
                decimal settlementAmount = CalculSettlementAmount(SessionTools.CS, nbOptions, notionalAmount, settlementRate, StrikePrice.DecValue);


                ctrl = pPage.FindControl("TXTSETTLEMENTAMOUNT_" + idE);
                if (null != ctrl)
                    ((TextBox)ctrl).Text = StrFunc.FmtDecimalToCurrentCulture(settlementAmount.ToString(NumberFormatInfo.InvariantInfo));

                ctrl = pPage.FindControl("LBLTITLE_" + idE);
                if (null != ctrl)
                {
                    string title = (IsCashSettlement ? ResFormTitleCashSettlement : ResFormTitlePhysicalSettlement);
                    if (ctrl.GetType().Equals(typeof(Label)))
                        ((Label)ctrl).Text = title;
                    else if (ctrl.GetType().Equals(typeof(TableCell)))
                        ((TableCell)ctrl).Text = title;
                }
            }
            else
            {
                decimal bondPaymentAmount = CalculBondPaymentAmount(SessionTools.CS, nbOptions, Entitlement.Quantity, m_AccruedInterest.Amount, StrikePrice.DecValue);
                ctrl = pPage.FindControl("TXTBONDPAYMENTAMOUNT_" + idE);
                if (null != ctrl)
                    ((TextBox)ctrl).Text = StrFunc.FmtDecimalToCurrentCulture(bondPaymentAmount.ToString(NumberFormatInfo.InvariantInfo));
            }
            EnableNbOptions(pPage);
        }
        #endregion CalculCashSettlementOrBondPaymentAmount
        #region CalculUnitAccruedInterestAmount
        /// <summary>
        /// Calcul du coupon couru pour 1 titre
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pNotionalAmount"></param>
        /// <param name="pNbOptions"></param>
        /// <param name="pExerciseDate"></param>
        /// <returns></returns>
        /// EG 20150907 [21317] GLOP EG/PL Gestion ExDate
        protected decimal CalculAccruedInterestAmount(string pCS, DateTime pExerciseDate, decimal pEntitlement)
        {
            decimal amount = 0;
            DataRow[] _rowDebtSecurityStreams = m_DsEventsDebtSecurity.DtEvent.Select("EVENTCODE=" + DataHelper.SQLString(EventCodeFunc.DebtSecurityStream));
            foreach (DataRow _rowDebtSecurityStream in _rowDebtSecurityStreams)
            {
                DataRow[] _rowDebtSecurityChilds = _rowDebtSecurityStream.GetChildRows(m_DsEventsDebtSecurity.ChildEvent);
                foreach (DataRow _rowDebtSecurityChild in _rowDebtSecurityChilds)
                {
                    string eventType = _rowDebtSecurityChild["EVENTTYPE"].ToString();
                    if (EventTypeFunc.IsInterest(eventType))
                    {
                        DateTime startDate = Convert.ToDateTime(_rowDebtSecurityChild["DTSTARTUNADJ"]);
                        DateTime endDate = Convert.ToDateTime(_rowDebtSecurityChild["DTENDUNADJ"]);

                        // EG 20150907 [21317]
                        int idE = Convert.ToInt32(_rowDebtSecurityChild["IDE"]);
                        DataRow[] rowEventClassEXD = m_DsEventsDebtSecurity.DtEventClass.Select(StrFunc.AppendFormat(@"IDE = {0} and EVENTCLASS = '{1}'", idE, EventClassFunc.RecordDate), "IDE");
                        if (ArrFunc.IsFilled(rowEventClassEXD))
                            endDate = Convert.ToDateTime(rowEventClassEXD[0]["DTEVENT"]);

                        if ((startDate <= pExerciseDate) && ( pExerciseDate < endDate))
                        {
                            decimal nativeAmount = 0;
                            if (false == Convert.IsDBNull(_rowDebtSecurityChild["VALORISATION"]))
                                nativeAmount = Convert.ToDecimal(_rowDebtSecurityChild["VALORISATION"]);
                            string currency = _rowDebtSecurityChild["UNIT"].ToString();

                            string dayCountFraction = m_DebtSecurityContainer.DebtSecurity.Stream[0].DayCountFraction;
                            IInterval intervalFrequency = m_DebtSecurityContainer.DebtSecurity.Stream[0].PaymentDates.PaymentFrequency;
                            EFS_DayCountFraction dcfTotal = new EFS_DayCountFraction(startDate, endDate, dayCountFraction, intervalFrequency);
                            EFS_DayCountFraction dcfAccrued = new EFS_DayCountFraction(startDate, pExerciseDate, dayCountFraction, intervalFrequency, endDate);
                            decimal fullCouponFraction = 0;
                            if (null != dcfTotal)
                                fullCouponFraction = dcfTotal.Factor;
                            decimal accruedFraction = 0;
                            if (null != dcfAccrued)
                                accruedFraction = dcfAccrued.Factor;

                            EFS_Cash cash = new EFS_Cash(pCS, (nativeAmount * accruedFraction / fullCouponFraction) * pEntitlement, currency);
                            amount = cash.AmountRounded;
                            break;
                        }
                    }
                }
            }
            return amount;
        }
        #endregion CalculUnitAccruedInterestAmount
        #region CalculBondPaymentAmount
        protected decimal CalculBondPaymentAmount(string pCs, decimal pNbOptions, decimal pEntitlement, decimal pAccruedInterestAmount, decimal pStrikePrice)
        {
            decimal amount;
            // Si Strike exprimé en montant (prix et currencySpecified
            if (OptionStrike.PriceSpecified && OptionStrike.CurrencySpecified)
                amount = (pStrikePrice + pAccruedInterestAmount) * pNbOptions;
            // Si Strike exprimé en pourcentage|prix
            else
                amount = ((pStrikePrice * pEntitlement) + pAccruedInterestAmount) * pNbOptions;

            EFS_Cash cash = new EFS_Cash(pCs, amount, Notional.Currency);
            amount = cash.AmountRounded;
            return amount;
        }
        #endregion CalculBondPaymentAmount
        #region CalculSettlementAmount
        // EG 20150605 Test SettlementRate 
        protected decimal CalculSettlementAmount(string pCs, decimal pNbOptions, decimal pNotionalAmount, Pair<Cst.PriceQuoteUnits,EFS_Decimal> pSettlementRate, decimal pStrikePrice)
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
        #endregion CalculSettlementAmount
        #region GetSettlementRate
        private Pair<Cst.PriceQuoteUnits,EFS_Decimal> GetSettlementRate(string pCS, int pIdAsset, DateTime pFixingDate)
        {
            Pair<Cst.PriceQuoteUnits, EFS_Decimal> settlementRate = new Pair<Cst.PriceQuoteUnits, EFS_Decimal>
            {
                // EG 20150910 Initialisation
                Second = new EFS_Decimal()
            };
            IProduct product = (IProduct)CurrentProduct;
            m_QuoteSide = string.Empty;
            m_QuoteTiming = string.Empty;
            KeyQuote keyQuote = new KeyQuote(pCS, pFixingDate);
            SQL_Quote quote = new SQL_Quote(pCS, QuoteEnum.DEBTSECURITY, AvailabilityEnum.Enabled, product.ProductBase, keyQuote, pIdAsset);
            if (quote.IsLoaded)
            {
                m_QuoteSide = (keyQuote.QuoteSide != null) ? keyQuote.QuoteSide.ToString() : string.Empty;
                m_QuoteTiming = (keyQuote.QuoteTiming != null) ? keyQuote.QuoteTiming.ToString() : string.Empty;

                settlementRate.First = (Cst.PriceQuoteUnits)ReflectionTools.EnumParse(new Cst.PriceQuoteUnits(), quote.QuoteUnit);
                // EG 20150910 Upd
                settlementRate.Second.DecValue = quote.QuoteValue;
            }
            return settlementRate;
        }
        #endregion GetSettlementRate


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
                    StringBuilder sb = new StringBuilder();
                    sb.AppendFormat("javascript:__doPostBack('{0}','{1}');", ddlActionDate.ID,
                        TradeActionMode.TradeActionModeEnum.CalculCashSettlementOrBondPaymentAmount);
                    ddlActionDate.Attributes.Add("onchange", sb.ToString());
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
                #region NbOption / entitlement / NotionalAmount
                aTableRow.Add(CreateControlTitleSeparator(ResFormQty, false));
                aTableRow.Add(base.CreateControlNbOptions(EventTypeFunc.IsTotal(abandonExerciseType)));
                aTableRow.Add(base.CreateControlEntitlement());
                aTableRow.Add(base.CreateControlNotional());
                #endregion NbOption / entitlement / NotionalAmount
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
                    TradeActionMode.TradeActionModeEnum.CalculCashSettlementOrBondPaymentAmount);
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
        #endregion CreateControlExerciseType
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
                    Regex = OptionStrike.PercentageSpecified ? EFSRegex.TypeRegex.RegexPercent : EFSRegex.TypeRegex.RegexFixedRateExtend,
                    LblWidth = 100
                };
                FpMLTextBox txtStrikePrice = new FpMLTextBox(null, FormatStrikePrice, 150, "StrikePrice", controlGUI,
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
                else
                {
                    #region Bond Paiement
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
                    FpMLTextBox txtBondPaiementAmount = new FpMLTextBox(null, StrFunc.FmtDecimalToCurrentCulture(m_BondPayment.Amount), 150, "BondPaymentAmount", controlGUI,
                        null, false, "TXTBONDPAYMENTAMOUNT_" + idE, null,
                        new Validator("BondPaymentAmount", true),
                        new Validator(EFSRegex.TypeRegex.RegexAmountExtend, "BondPaymentAmount", true, false));

                    GetFormatControlAttribute(txtBondPaiementAmount);
                    td.Controls.Add(txtBondPaiementAmount);
                    td.Controls.Add(new LiteralControl(Cst.HTMLSpace + m_BondPayment.Currency));
                    td.Controls.Add(new LiteralControl(Cst.HTMLSpace + ResFormPaidBy + Cst.HTMLSpace + m_BondPayment.payer.actor.Second));
                    tr.Cells.Add(td);
                    aTableRow.Add(tr);
                    #endregion Bond Paiement
                }
                return (TableRow[])aTableRow.ToArray(typeof(TableRow));
            }
        }
        #endregion CreateControlSettlement

        #region PostedAction
        // EG 20150616 [21091] Test IsCashSettlement pour SettlementRate
        public object PostedAction(string pKeyAction)
        {
            string keyAction = pKeyAction + Convert.ToInt32(TradeActionCode.TradeActionCodeEnum.Exercise) + "_" + m_Event.eventCode;
            return new BO_ExerciseMsg(idE, ActionDateTime, ValueDate.DateValue, abandonExerciseType, IsCashSettlement,
                NbOptions, Notional, Entitlement, m_BondPayment, m_AccruedInterest, m_Settlement, ((IDebtSecurityOption)CurrentProduct).SecurityAssetOTCmlId,
                IsCashSettlement?SettlementRate.Second.DecValue:0, StrikePrice.DecValue, m_QuoteSide, m_QuoteTiming, note, keyAction);
        }
        #endregion PostedAction
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
                    // EG 20170127 Qty Long To Decimal
                    if (null != pPage.Request.Form["TXTNBOPTIONS_" + idE])
                        NbOptions.Quantity = Convert.ToDecimal(pPage.Request.Form["TXTNBOPTIONS_" + idE]);
                    if (null != pPage.Request.Form["TXTSETTLEMENTRATE_" + idE])
                    {
                        int coeff = 1;
                        string rate = pPage.Request.Form["TXTSETTLEMENTRATE_" + idE];
                        if (rate.EndsWith("%"))
                        {
                            rate = rate.Replace("%", string.Empty);
                            coeff = 100;
                        }
                        SettlementRate.Second.DecValue = DecFunc.DecValue(rate, Thread.CurrentThread.CurrentCulture) / coeff;
                    }
                    if (null != pPage.Request.Form["TXTSETTLEMENTAMOUNT_" + idE])
                        m_Settlement.Amount = DecFunc.DecValue(pPage.Request.Form["TXTSETTLEMENTAMOUNT_" + idE],
                            Thread.CurrentThread.CurrentCulture);
                    if (null != pPage.Request.Form["TXTBONDPAYMENTAMOUNT_" + idE])
                        m_BondPayment.Amount = DecFunc.DecValue(pPage.Request.Form["TXTBONDPAYMENTAMOUNT_" + idE],
                            Thread.CurrentThread.CurrentCulture);
                    if (null != pPage.Request.Form["TXTACCRUEDINTEREST_" + idE])
                        m_AccruedInterest.Amount = DecFunc.DecValue(pPage.Request.Form["TXTACCRUEDINTEREST_" + idE],
                            Thread.CurrentThread.CurrentCulture);
                }
            }
            m_Event.isValidated = isOk;
            return isOk;
        }
        #endregion Save

        #region ValidationRules
        // EG 20150920 [21374] Int (int32) to Long (Int64) 
        // EG 20170127 Qty Long To Decimal
        public override bool ValidationRules(Page pPage)
        {
            bool isOk = true;
            m_Event.validationRulesMessages = new ArrayList();
            decimal qty = 0;
            if (null != pPage.Request.Form["DDLEXERCISETYPE_" + idE])
                abandonExerciseType = pPage.Request.Form["DDLEXERCISETYPE_" + idE];

            // EG 20170127 Qty Long To Decimal
            if (null != pPage.Request.Form["TXTNBOPTIONS_" + idE])
                qty = Convert.ToDecimal(pPage.Request.Form["TXTNBOPTIONS_" + idE], Thread.CurrentThread.CurrentCulture);

            decimal initialNbOptions = InitialNbOptions.valorisation.DecValue;
            decimal nbOptions_INT = 0;
            if (EventTypeFunc.IsMultiple(abandonExerciseType))
            {
                #region Qty control
                if (null != m_NbOptionExercise_INT)
                {
                    foreach (TradeActionEvent item in m_NbOptionExercise_INT)
                        nbOptions_INT += item.valorisation.DecValue;

                    decimal remainingNbOptions = initialNbOptions - nbOptions_INT;
                    isOk = (0 < qty) && (qty <= remainingNbOptions);
                }
                else
                    isOk = (qty < initialNbOptions);
                #endregion Qty control
                if (false == isOk)
                    m_Event.validationRulesMessages.Add("Msg_IncorrectMultipleExerciseQty");

            }
            else if (EventTypeFunc.IsPartiel(abandonExerciseType))
            {
                isOk = (0 < qty) && (qty < initialNbOptions);
                if (false == isOk)
                    m_Event.validationRulesMessages.Add("Msg_IncorrectPartialExerciseQty");
            }
            if (isOk)
                isOk = base.ValidationRules(pPage);
            return isOk;
        }
        #endregion ValidationRules

        #region GetBondAmountInfo
        /// <summary>
        /// Alimentation du Payer/Receiver associé  au montant spcécifié
        /// Call : BondPayment|AccruedInterest est payé par l'acheteur du Bond Option
        /// Put  : BondPayment|AccruedInterest est payé par le vendeue du Bond Option
        /// </summary>
        /// <param name="pAmount"></param>
        /// <param name="pCurrency"></param>
        /// <returns></returns>
        private AmountPayerReceiverInfo GetBondAmountInfo(string pEventType, decimal pAmount, string pCurrency)
        {
            PayerReceiverInfoDet payer = null;
            PayerReceiverInfoDet receiver = null;
            if (EventTypeFunc.IsBondPayment(pEventType) || EventTypeFunc.IsAccruedInterestAmount(pEventType))
            {
                switch (m_OptionType)
                {
                    case OptionTypeEnum.Call:
                        payer = new PayerReceiverInfoDet(PayerReceiverEnum.Payer, Buyer);
                        receiver = new PayerReceiverInfoDet(PayerReceiverEnum.Receiver, Seller);
                        break;
                    case OptionTypeEnum.Put:
                        payer = new PayerReceiverInfoDet(PayerReceiverEnum.Payer, Seller);
                        receiver = new PayerReceiverInfoDet(PayerReceiverEnum.Receiver, Buyer);
                        break;
                }
            }
            else if (EventTypeFunc.IsSettlementCurrency(pEventType))
            {
                payer = new PayerReceiverInfoDet(PayerReceiverEnum.Payer, Seller);
                receiver = new PayerReceiverInfoDet(PayerReceiverEnum.Receiver, Buyer);
            }
            // EG 20150706 [21021]
            AmountPayerReceiverInfo amountInfo = new AmountPayerReceiverInfo(null, pAmount, pCurrency, payer, receiver);
            return amountInfo;
        }
        #endregion GetBondAmountInfo

        #endregion Methods
    }
    #endregion BO_ExerciseEvents


    #region BO_FeatureEvents
    // TBD
    public class BO_FeatureEvents : BO_FeaturePaymentEvents
    {
        #region Constructors
        public BO_FeatureEvents(TradeActionEventBase pEvent, TradeActionEventBase pEventParent)
            : base(pEvent, pEventParent) 
        {
        }
        #endregion Constructors
    }
    #endregion BO_FeatureEvents

    #region BO_FeaturePaymentEvents 
    // TBD
    public class BO_FeaturePaymentEvents  : ActionEvents
    {
        #region Members
        protected AmountPayerReceiverInfo payment;
        #endregion Members

        public BO_FeaturePaymentEvents(TradeActionEventBase pEvent, TradeActionEventBase pEventParent)
            : base(pEvent, pEventParent) { }

    }
    #endregion BO_FeaturePaymentEvents
}
