#region Using Directives
using EFS.ACommon;
using EFS.ApplicationBlocks.Data;
using EFS.Common.MQueue;
using EFS.Common.Web;
using EFS.GUI;
using EFS.GUI.Attributes;
using EFS.GUI.Interface;
using EFS.GUI.SimpleControls;
using EfsML.Business;
using EfsML.Enum.Tools;
using FpML.Interface;
using System;
using System.Collections;
using System.Reflection;
using System.Threading;
using System.Web.UI;
using System.Web.UI.WebControls;
#endregion Using Directives

namespace EfsML
{
    #region TradeAdminAction
    /// <summary>Used by the trade action process </summary>
    public class TradeAdminAction : TradeActionBase
    {
        #region Members
        public TradeAdminActionEvent events;
        #endregion Members
        #region Constructors
        public TradeAdminAction():base(){}
        public TradeAdminAction(int pCurrentIdE, TradeActionType.TradeActionTypeEnum pTradeActionType, TradeActionMode.TradeActionModeEnum pTradeActionMode,
            DataDocumentContainer pDataDocumentContainer, TradeAdminActionEvent pEvents)
            : base(pCurrentIdE, pTradeActionType, pTradeActionMode, pDataDocumentContainer)
        {
            events = pEvents;
        }
        #endregion Constructors
        #region Methods
        #endregion Methods
    }
    #endregion TradeAdminAction

    #region TradeAdminActionEvent
    public class TradeAdminActionEvent : TradeActionEventBase
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("Event")]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "", IsVisible = true)]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "")]
        public TradeAdminActionEvent[] events;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public TradeActionBase currentTradeAdminAction;
        #endregion Members
        #region Accessors
        #region ActionTitle
        public override string ActionTitle
        {
            get
            {
                string param = CurrentTradeAction.GetEventTypeTitle(eventType);
                return Ressource.GetString2("TradeActionCharacteristics", param);
            }
        }
        #endregion ActionTitle
        #region CurrentTradeAction
        public override TradeActionBase CurrentTradeAction
        {
            get { return currentTradeAdminAction; }
        }
        #endregion CurrentTradeAction
        #region EventDetailGrossTurnOverAmount
        private TradeAdminActionEvent[] EventDetailGrossTurnOverAmount
        {
            get {return GetEvent_EventCode(EventCodeFunc.LinkedProductPayment);}
        }
        #endregion EventDetailGrossTurnOverAmount
        #region EventGrossTurnOverAmount
        private TradeAdminActionEvent[] EventGrossTurnOverAmount
        {
            get {return GetEvent_EventType(EventTypeFunc.GrossTurnOverAmount);}
        }
        #endregion EventGrossTurnOverAmount
        #region EventClassInvoiced
        public EventClass EventClassInvoiced
        {
            get
            {
                if (null != eventClass)
                {
                    foreach (EventClass item in eventClass)
                    {
                        if (EventClassFunc.IsInvoiced(item.code))
                            return item;
                    }
                }
                return null;
            }
        }
        #endregion EventClassInvoiced

        #region EventProduct
        private TradeAdminActionEvent[] EventProduct
        {
            get { return GetEvent_EventCode(EventCodeFunc.Product); }
        }
        #endregion EventProduct
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
        #endregion Accessors
        #region Indexors
        public TradeAdminActionEvent this[int pIdE]
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
        #region Methods
        #region CreateControls
        /// <summary>
        /// Procédure principale de création du tableau des évènements.
        /// </summary>
        public PlaceHolder CreateControls(TradeActionType.TradeActionTypeEnum pTradeActionType, FullConstructor pFullCtor, TradeActionBase pCurrentTradeAdminAction)
        {
            currentTradeAdminAction = pCurrentTradeAdminAction;
            PlaceHolder phEvents = new PlaceHolder();
            TradeAdminActionEvent[] eventProduct = this.EventProduct;
            if (null != eventProduct)
            {
                foreach (TradeAdminActionEvent prd in eventProduct)
                {
                    prd.currentTradeAdminAction = currentTradeAdminAction;
                    phEvents.Controls.Add(prd.OpenTitle(pFullCtor));

                    if (prd.IsStrategy)
                        phEvents.Controls.Add(prd.CreateControls(pTradeActionType, pFullCtor, currentTradeAdminAction));
                    else
                    {
                        object product = CurrentProduct(prd.instrumentNo);
                        IProductBase productBase = (IProductBase)product;
                        if (productBase.IsInvoice)
                        {
                            if (TradeActionType.IsInvoicingCorrectionEvents(pTradeActionType))
                                phEvents.Controls.Add(CreateInvoicingCorrectionEvents(prd, pFullCtor));
                        }
                        else
                            phEvents.Controls.Add(new LiteralControl(Ressource.GetString("NothingToDo")));
                    }
                    phEvents.Controls.Add(prd.CloseTitle(pFullCtor));
                }
            }
            return phEvents;
        }
        #endregion CreateControls
        #region CreateInvoicingCorrectionEvents
        public static PlaceHolder CreateInvoicingCorrectionEvents(TradeAdminActionEvent pEvent, FullConstructor pFullCtor)
        {
            PlaceHolder phInvoicingCorrectionEvents = new PlaceHolder();
            TradeAdminActionEvent[] eventInvoicePeriod = pEvent.GetEvent_EventType(EventTypeFunc.Period);
            if (null != eventInvoicePeriod)
            {
                eventInvoicePeriod[0].currentTradeAdminAction = pEvent.currentTradeAdminAction;
                phInvoicingCorrectionEvents.Controls.Add(eventInvoicePeriod[0].OpenTitle(pFullCtor));
                phInvoicingCorrectionEvents.Controls.Add(eventInvoicePeriod[0].DisplayGrossTurnOverAmount);
                phInvoicingCorrectionEvents.Controls.Add(new LiteralControl("<br/>"));
                phInvoicingCorrectionEvents.Controls.Add(eventInvoicePeriod[0].DisplayDetailGrossTurnOverAmount);
                phInvoicingCorrectionEvents.Controls.Add(new LiteralControl("<br/>"));
            }
            return phInvoicingCorrectionEvents;
        }
        #endregion CreateInvoicingCorrectionEvents
        #region DisplayDetailGrossTurnOverAmount
        private PlaceHolder DisplayDetailGrossTurnOverAmount
        {
            get
            {
                bool isActionAuthorized = true;
                TradeAdminActionEvent[] eventGTOAmount = EventGrossTurnOverAmount;
                if (null != eventGTOAmount)
                {
                    return DisplayEvents(eventGTOAmount[0].EventDetailGrossTurnOverAmount, isActionAuthorized);
                }
                return null;
            }
        }
        #endregion DisplayDetailGrossTurnOverAmount

        #region DisplayEvents
        private PlaceHolder DisplayEvents(TradeAdminActionEvent[] pEvent, bool pIsActionAuthorized)
        {
            return DisplayEvents(pEvent, pIsActionAuthorized, false);
        }
        private PlaceHolder DisplayEvents(TradeAdminActionEvent[] pEvent, bool pIsActionAuthorized, bool pIsReinit)
        {
            PlaceHolder ph = new PlaceHolder();
            if (null != pEvent)
            {
                Table table = null;
                foreach (TradeAdminActionEvent item in pEvent)
                {
                    item.currentTradeAdminAction = currentTradeAdminAction;
                    if ((null == item.m_Action) || (false == pIsActionAuthorized) || pIsReinit)
                    {
                        item.m_Action = item.InitializeAction(this);
                        item.isModified = false;
                    }
                    if (null == table)
                        table = item.CreateTable(pIsActionAuthorized);
                    table.Rows.Add(item.CreateRow(pIsActionAuthorized,this));
                }
                // EG 20091110 Overflow
                if (20 < pEvent.Length)
                {
                    Panel pnlOverFlow = new Panel();
                    pnlOverFlow.Controls.Add(table);
                    pnlOverFlow.Height = Unit.Pixel(200);
                    pnlOverFlow.Style.Add(HtmlTextWriterStyle.Overflow, "auto");
                    ph.Controls.Add(pnlOverFlow);
                }
                else
                    ph.Controls.Add(table);
            }
            return ph;
        }
        #endregion DisplayEvents
        #region DisplayGrossTurnOverAmount
        private PlaceHolder DisplayGrossTurnOverAmount
        {
            get
            {
                bool isActionAuthorized = false;
                return DisplayEvents(EventGrossTurnOverAmount, isActionAuthorized);
            }
        }
        #endregion DisplayGrossTurnOverAmount

        #region GetEvent
        public TradeAdminActionEvent GetEvent(int pIdE)
        {
            if (pIdE == this.idE)
                return this;
            else
            {
                TradeAdminActionEvent _event = this[pIdE];
                if ((null == _event) && (null != this.events))
                {
                    foreach (TradeAdminActionEvent item in this.events)
                    {
                        _event = item.GetEvent(pIdE);
                        if (null != _event)
                            return _event;
                    }
                }
                return _event;
            }
        }
        #endregion GetEvent
        #region GetEvent_EventCode
        private TradeAdminActionEvent[] GetEvent_EventCode(string pEventCode)
        {
            ArrayList aEvent = new ArrayList();
            if (null != events)
            {
                foreach (TradeAdminActionEvent item in events)
                {
                    if (pEventCode == item.eventCode)
                        aEvent.Add(item);
                }
                if (0 < aEvent.Count)
                    return (TradeAdminActionEvent[])aEvent.ToArray(typeof(TradeAdminActionEvent));
            }
            return null;
        }
        #endregion GetEvent_EventCode
        #region GetEvent_EventType
        private TradeAdminActionEvent[] GetEvent_EventType(string pEventType)
        {
            ArrayList aEvent = new ArrayList();
            if (null != events)
            {
                foreach (TradeAdminActionEvent item in events)
                {
                    if (pEventType == item.eventType)
                        aEvent.Add(item);
                }
                if (0 < aEvent.Count)
                    return (TradeAdminActionEvent[])aEvent.ToArray(typeof(TradeAdminActionEvent));
            }
            return null;
        }
        #endregion GetEvent_EventType
        #region InitializeAction
        public override ActionEventsBase InitializeAction(TradeActionEventBase pEventParent)
        {
            ActionEventsBase action = null;
            if (EventTypeFunc.IsGrossTurnOverAmount(eventType))
                action = new InvoicingGTOEvents(this, pEventParent);
            else if (EventCodeFunc.IsLinkedProductPayment(eventCode))
                action = new InvoicingDetailGTOEvents(SessionTools.CS, this, pEventParent);
            return action;
        }
        #endregion InitializeAction
        #endregion Methods
    }
    #endregion TradeAdminActionEvent
    #region TradeAdminActionEvents
    [System.Xml.Serialization.XmlRootAttribute("TradeAdminAction", Namespace = "", IsNullable = false)]
    public class TradeAdminActionEvents : TradeActionEventsBase
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("Event")]
        public TradeAdminActionEvent events;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public TradeAdminAction currentTradeAdminAction;
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
        public PlaceHolder CreateControls(TradeActionType.TradeActionTypeEnum pTradeActionType,
            TradeActionMode.TradeActionModeEnum pTradeActionMode, int pCurrentIdE, DataDocumentContainer pDataDocumentContainer, FullConstructor pFullCtor)
        {
            PlaceHolder plh = new PlaceHolder();
            tradeActionType = pTradeActionType;
            currentTradeAdminAction = new TradeAdminAction(pCurrentIdE, pTradeActionType, pTradeActionMode, pDataDocumentContainer, events);
            plh.Controls.Add(events.CreateControls(pTradeActionType, pFullCtor, currentTradeAdminAction));
            return plh;
        }
        #endregion CreateControls
        #region GetEvent
        public TradeAdminActionEvent GetEvent(int pIdE)
        {
            return events.GetEvent(pIdE);
        }
        #endregion GetEvent
        #endregion Methods
    }
    #endregion TradeAdminActionEvents
    #region TradeAdminActionEventDetails
    /// <summary>
    /// Class where are stored by serialisation the lines of the table <b>EventDet</b> for a trade.
    /// </summary>
    public class TradeAdminActionEventDetails : TradeActionEventDetailsBase
    {
        #region Methods
        #endregion Methods
    }
    #endregion TradeAdminActionEventDetails

    #region AdminActionEvents
    public abstract class AdminActionEvents : ActionEventsBase
    {
        #region Members
        protected TradeActionEventBase m_Event;
        protected TradeActionEventBase m_EventParent;
        #endregion Members
        #region Accessors
        #region ActionEvent
        public override TradeActionEventBase ActionEvent
        {
            get { return m_Event; }
        }
        #endregion ActionEvent
        #region TradeAdminAction
        protected override TradeActionBase TradeAdminAction
        {
            get { return m_Event.CurrentTradeAction; }
        }
        #endregion TradeAdminAction
        #region Resource in Form
        #region Label
        #region ResFormCorrectedAmount
        protected virtual string ResFormCorrectedAmount { get { return Ressource.GetString("CorrectedAmount"); } }
        #endregion ResFormCorrectedAmount
        #endregion Label
        #region Title
        #region ResFormTitleAmount
        protected virtual string ResFormTitleAmount { get { return Ressource.GetString("Amount"); } }
        #endregion ResFormTitleAmount
        #region ResFormTitleBeneficiary
        protected virtual string ResFormTitleBeneficiary { get { return Ressource.GetString("Beneficiary"); } }
        #endregion ResFormTitleBeneficiary
        #region ResFormTitleDetailGrossTurnOverAmount
        protected virtual string ResFormTitleDetailGrossTurnOverAmount { get { return Ressource.GetString("DetailGrossTurnOverAmount"); } }
        #endregion ResFormTitleDetailGrossTurnOverAmount
        #region ResFormTitleInvoiced
        protected virtual string ResFormTitleInvoiced { get { return Ressource.GetString("Invoiced"); } }
        #endregion ResFormTitleInvoiced                 
        #region ResFormTitlePaymentDate
        protected virtual string ResFormTitlePaymentDate { get { return Ressource.GetString("DateSettlement"); } }
        #endregion ResFormTitlePaymentDate              
        #endregion Title
        #endregion Resource in Form
        #endregion Accessors
        #region Constructors
        public AdminActionEvents() { }
        public AdminActionEvents(TradeActionEventBase pEvent, TradeActionEventBase pEventParent)
            : base(pEvent)
        {
            m_Event = pEvent;
            m_EventParent = pEventParent;
        }
        #endregion Constructors
        #region Methods
        #endregion Methods
    }
    #endregion AdminActionEvents

    #region InvoicingDetailGTOEvents
    public class InvoicingDetailGTOEvents : AdminActionEvents
    {
        #region Members
        public int idE_Source;
        public EFS_Decimal originalAmount;
        public EFS_Decimal amount;
        #endregion Members
        //
        #region Accessors
        #region CreateHeaderCells_Capture
        private TableHeaderCell[] CreateHeaderCells_Capture
        {
            get
            {
                ArrayList aHeaderCell = new ArrayList
                {
                    TableTools.AddHeaderCell(ResFormActionDate, false, 0, UnitEnum.Pixel, 2, false),
                    TableTools.AddHeaderCell(ResFormCorrectedAmount, false, 100, UnitEnum.Pixel, 0, false),
                    TableTools.AddHeaderCell(ResFormTitleNoteEvents, false, 0, UnitEnum.Pixel, 2, false)
                };
                return (TableHeaderCell[])aHeaderCell.ToArray(typeof(TableHeaderCell));
            }
        }
        #endregion CreateHeaderCells_Capture
        #region CreateHeaderCells_Static
        // EG 20100127 Add ResFormTitleTrade
        // EG 20100208 Add ResFormTitleEventId
        private TableHeaderCell[] CreateHeaderCells_Static
        {
            get
            {
                ArrayList aHeaderCell = new ArrayList
                {
                    TableTools.AddHeaderCell(ResFormTitleEventType, false, 0, UnitEnum.Pixel, 2, false),
                    TableTools.AddHeaderCell(ResFormTitlePeriod, false, 150, UnitEnum.Pixel, 2, false),
                    // EG 20100127 Add Identifier Trade
                    TableTools.AddHeaderCell(ResFormTitleTrade, false, 100, UnitEnum.Pixel, 0, false),
                    // EG 20100208 Add ResFormTitleEventId
                    TableTools.AddHeaderCell(ResFormTitleEventId, false, 0, UnitEnum.Pixel, 1, false),
                    TableTools.AddHeaderCell(ResFormTitlePaymentDate, false, 75, UnitEnum.Pixel, 0, false),
                    TableTools.AddHeaderCell(ResFormTitleAmount, false, 120, UnitEnum.Pixel, 2, false),
                    TableTools.AddHeaderCell(ResFormPayer, false, 150, UnitEnum.Pixel, 2, false)
                };
                return (TableHeaderCell[])aHeaderCell.ToArray(typeof(TableHeaderCell));
            }
        }
        #endregion CreateHeaderCells_Static
        #region Resource in Form
        #endregion Resource in Form
        #endregion Accessors
        //
        #region Constructors
        public InvoicingDetailGTOEvents(string pCS, TradeActionEventBase pEvent, TradeActionEventBase pEventParent)
            : base(pEvent, pEventParent)
        {
            // FI 20200904 [XXXXX] call OTCmlHelper.GetDateSys
            //DateTime dtSys = OTCmlHelper.GetDateBusiness(pCS);
            DateTime dtSys = OTCmlHelper.GetDateSys(pCS);

            actionDate.DateValue = dtSys.Date;
            actionTime.TimeValue = dtSys;
            idE_Source = pEvent.idE_Source;

            originalAmount = new EFS_Decimal(pEvent.valorisation.DecValue);
            amount = new EFS_Decimal(pEvent.valorisation.DecValue);
        }
        #endregion Constructors
        #region Methods
        #region AddCells_Capture
        private TableCell[] AddCells_Capture(TradeAdminActionEvent pEventParent)
        {
            ArrayList aCell = new ArrayList();
            m_EventParent = pEventParent;
            aCell.Add(TableTools.AddCell(ActionDate, HorizontalAlign.Center, 80, UnitEnum.Pixel));
            aCell.Add(TableTools.AddCell(actionTime.Value, HorizontalAlign.Center, 60, UnitEnum.Pixel));
            aCell.Add(TableTools.AddCell(StrFunc.FmtDecimalToCurrentCulture(amount.DecValue), HorizontalAlign.Right));
            aCell.Add(TableTools.AddCell(note, HorizontalAlign.NotSet, 100, UnitEnum.Percentage, true, false, false));
            aCell.Add(TableTools.AddCell(Cst.HTMLSpace, HorizontalAlign.Center, 0, UnitEnum.Pixel));
            return (TableCell[])aCell.ToArray(typeof(TableCell));
        }
        #endregion AddCells_Capture
        #region AddCells_Static
        // EG 20100127 Add Identifier Trade
        // EG 20100208 Add pEvent.idE_Source
        public TableCell[] AddCells_Static(TradeAdminActionEvent pEvent, TradeAdminActionEvent pEventParent)
        {
            ArrayList aCell = new ArrayList();
            aCell.AddRange(pEvent.NewCells_Static());
            // EG 20100127 Add Identifier Trade
            aCell.Add(TableTools.AddCell(pEvent.identifier_TradeSource, HorizontalAlign.NotSet));
            // EG 20100208 Add pEvent.idE_Source
            aCell.Add(TableTools.AddCell(pEvent.idE_Source.ToString(), HorizontalAlign.NotSet));
            aCell.Add(TableTools.AddCell(DtFunc.DateTimeToString(pEvent.EventClassRecognition.dtEvent.DateValue, DtFunc.FmtShortDate), HorizontalAlign.Center));
            aCell.Add(TableTools.AddCell(pEvent.unit, HorizontalAlign.Center));
            aCell.Add(TableTools.AddCell(StrFunc.FmtDecimalToCurrentCulture(pEvent.valorisation.DecValue), HorizontalAlign.Right));
            aCell.Add(TableTools.AddCell(pEvent.payer, HorizontalAlign.NotSet));
            aCell.Add(TableTools.AddCell(pEvent.payerBook, HorizontalAlign.NotSet));
            aCell.AddRange(AddCells_Capture(pEventParent));
            return (TableCell[])aCell.ToArray(typeof(TableCell));
        }
        #endregion AddCells_Static
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
                ControlGUI controlGUI = new ControlGUI(true, ResFormCorrectedAmount)
                {
                    Regex = EFSRegex.TypeRegex.RegexAmountExtend,
                    LblWidth = 100
                };
                FpMLTextBox txtAmount = new FpMLTextBox(null, amount.CultureValue, 200, "CorrectedAmount", controlGUI, null, false, "TXTAMOUNT_" + idE, null,
                    new Validator("CorrectedAmount", true),
                    new Validator(EFSRegex.TypeRegex.RegexAmountExtend, "CorrectedAmount", true, false))
                {
                    Enabled = true
                };
                GetFormatControlAttribute(txtAmount);
                td.Controls.Add(txtAmount);
                td.Controls.Add(new LiteralControl(Cst.HTMLSpace + m_Event.unit));
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
        #region CreateTableHeader
        public ArrayList CreateTableHeader(TradeAdminActionEvent pEvent)
        {
            ArrayList aTableHeader = new ArrayList
            {
                ResFormTitleDetailGrossTurnOverAmount,
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
                isEventChanged = (amount.DecValue != originalAmount.DecValue);
            return isEventChanged;
        }
        #endregion IsEventChanged

        #region PostedAction
        public object PostedAction(string pKeyAction)
        {
            if (originalAmount.DecValue != amount.DecValue)
            {
                string keyAction = pKeyAction + Convert.ToInt32(TradeActionCode.TradeActionCodeEnum.InvoiceCorrection) + "_" + m_Event.eventType;
                return new InvoicingDetailMsg(idE, ActionDateTime, idE_Source,originalAmount.DecValue, amount.DecValue, note, keyAction);
            }
            return null;
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
                    if (null != pPage.Request.Form["TXTAMOUNT_" + idE])
                        amount.DecValue = DecFunc.DecValue(pPage.Request.Form["TXTAMOUNT_" + idE], Thread.CurrentThread.CurrentCulture);
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
    #endregion InvoicingDetailGTOEvents
    #region InvoicingGTOEvents
    public class InvoicingGTOEvents : AdminActionEvents
    {
        #region Accessors
        #region CreateHeaderCells_Static
        private TableHeaderCell[] CreateHeaderCells_Static
        {
            get
            {
                ArrayList aHeaderCell = new ArrayList
                {
                    TableTools.AddHeaderCell(ResFormTitleEventType, false, 0, UnitEnum.Pixel, 2, false),
                    TableTools.AddHeaderCell(ResFormTitlePeriod, false, 150, UnitEnum.Pixel, 2, false),
                    TableTools.AddHeaderCell(ResFormTitlePaymentDate, false, 120, UnitEnum.Pixel, 0, false),
                    TableTools.AddHeaderCell(ResFormTitleAmount, false, 175, UnitEnum.Pixel, 2, false),
                    TableTools.AddHeaderCell(ResFormTitleInvoiced, false, 175, UnitEnum.Pixel, 0, false),
                    TableTools.AddHeaderCell(ResFormTitleBeneficiary, false, 200, UnitEnum.Pixel, 2, false)
                };
                return (TableHeaderCell[])aHeaderCell.ToArray(typeof(TableHeaderCell));
            }
        }
        #endregion CreateHeaderCells_Static
        #endregion Accessors
        #region Constructors
        public InvoicingGTOEvents(TradeActionEventBase pEvent, TradeActionEventBase pEventParent) : base(pEvent, pEventParent) { }
        #endregion Constructors
        #region Methods
        #region AddCells_Static
        public static TableCell[] AddCells_Static(TradeAdminActionEvent pEvent, TradeAdminActionEvent pEventParent)
        {
            ArrayList aCell = new ArrayList();
            aCell.AddRange(pEvent.NewCells_Static());
            aCell.Add(TableTools.AddCell(DtFunc.DateTimeToString(pEvent.EventClassRecognition.dtEvent.DateValue, DtFunc.FmtShortDate), HorizontalAlign.Center));
            aCell.Add(TableTools.AddCell(pEvent.unit, HorizontalAlign.Center));
            aCell.Add(TableTools.AddCell(StrFunc.FmtDecimalToCurrentCulture(pEvent.valorisation.DecValue), HorizontalAlign.Right));
            aCell.Add(TableTools.AddCell(pEvent.payer, HorizontalAlign.NotSet));
            aCell.Add(TableTools.AddCell(pEvent.receiver, HorizontalAlign.NotSet));
            aCell.Add(TableTools.AddCell(pEvent.receiverBook, HorizontalAlign.NotSet));

            return (TableCell[])aCell.ToArray(typeof(TableCell));
        }
        #endregion AddCells_Static
        #region CreateTableHeader
        public ArrayList CreateTableHeader(TradeAdminActionEvent pEvent)
        {
            ArrayList aTableHeader = new ArrayList
            {
                pEvent.CurrentTradeAction.GetEventTypeTitle(pEvent.eventType),
                CreateHeaderCells_Static
            };
            return aTableHeader;
        }
        #endregion CreateTableHeader
        #endregion Methods
    }
    #endregion InvoicingGTOEvents

}
