#region Using Directives
using EFS.ACommon;
using EFS.Common;
using EFS.Common.Web;
using EFS.GUI;
using EFS.GUI.Attributes;
using EFS.GUI.CCI;
using EFS.GUI.Interface;
using EFS.GUI.SimpleControls;
using EfsML.Business;
using EfsML.Enum;
using EfsML.Enum.Tools;
using FpML.Enum;
using FpML.Interface;
using System;
using System.Collections;
using System.Reflection;
using System.Text;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
#endregion Using Directives
namespace EfsML
{
    #region TradeActionBase
    /// <summary>Used by the trade action process </summary>
    /// EG 20150423 [20513] BANCAPERTA Refactoring OK
    public abstract class TradeActionBase
    {
        #region Members
        public int currentIdE;
        public Pair<TradeActionType.TradeActionTypeEnum, TradeActionMode.TradeActionModeEnum> action;
        public DataDocumentContainer dataDocumentContainer;
        private readonly ExtendEnum m_EventCodeEnum;
        private readonly ExtendEnum m_EventTypeEnum;
        private readonly ExtendEnum m_EventClassEnum;
        #endregion Members
        #region Accessors
        // EG 20150428 [20513] New
        public virtual bool IsFeatures { get { return false; } }
        // EG 20150428 [20513] New
        public virtual bool IsBarrierFeatures { get { return false; } }
        // EG 20150428 [20513] New
        public virtual bool IsAutomaticExercise { get { return false; } }
        // EG 20150428 [20513] New
        public virtual bool IsFallBackExercise { get { return false; } }

        #region ActionMode
        public TradeActionMode.TradeActionModeEnum ActionMode {get { return action.Second; }}
        #endregion ActionMode
        #region ActionType
        public TradeActionType.TradeActionTypeEnum ActionType {get { return action.First; }}
        #endregion ActionType

        #region CurrentProduct
        public IProduct CurrentProduct { get { return dataDocumentContainer.CurrentProduct.Product; } }
        #endregion CurrentProduct

        #region CurrentTrade
        public ITrade CurrentTrade {get {return dataDocumentContainer.CurrentTrade;}}
        #endregion CurrentTrade

        #region DataDocument
        public IDataDocument DataDocument {get { return dataDocumentContainer.DataDocument; }}
        #endregion DataDocument
        #region Parties
        public IParty[] Parties {get { return dataDocumentContainer.Party; }}
        #endregion Parties
        #region TradeDate
        public DateTime TradeDate {get { return CurrentTrade.AdjustedTradeDate; }}
        #endregion TradeDate

        #endregion Accessors

        #region Constructors
        public TradeActionBase(){}
        public TradeActionBase(int pCurrentIdE, TradeActionType.TradeActionTypeEnum pTradeActionType, TradeActionMode.TradeActionModeEnum pTradeActionMode,
            DataDocumentContainer pDataDocumentContainer)
        {
            currentIdE = pCurrentIdE;
            action = new Pair<TradeActionType.TradeActionTypeEnum, TradeActionMode.TradeActionModeEnum>(pTradeActionType, pTradeActionMode);
            dataDocumentContainer = pDataDocumentContainer;
            
            /* FI 20240731 [XXXXX] Mise en commentaire => use DataEnabledEnum/DataEnabledEnumHelper
            ExtendEnums extendEnums = ExtendEnumsTools.ListEnumsSchemes;
            m_EventCodeEnum = extendEnums["EventCode"];
            m_EventTypeEnum = extendEnums["EventType"];
            m_EventClassEnum = extendEnums["EventClass"];
            */

            m_EventCodeEnum = DataEnabledEnumHelper.GetDataEnum(SessionTools.CS, "EventCode");
            m_EventTypeEnum = DataEnabledEnumHelper.GetDataEnum(SessionTools.CS, "EventType");
            m_EventClassEnum = DataEnabledEnumHelper.GetDataEnum(SessionTools.CS, "EventClass");
        }
        #endregion Constructors

        #region Methods
        #region GetEventClassTitle
        public string GetEventClassTitle(string pEventClass)
        {
            string title = pEventClass;
            ExtendEnumValue extendEnumValue = m_EventClassEnum.GetExtendEnumValueByValue(pEventClass);
            if (null != extendEnumValue)
                title = extendEnumValue.Documentation;
            return title;
        }
        #endregion GetEventClassTitle
        #region GetEventCodeTitle
        public string GetEventCodeTitle(string pEventCode)
        {
            string title = pEventCode;
            ExtendEnumValue extendEnumValue = m_EventCodeEnum.GetExtendEnumValueByValue(pEventCode);
            if (null != extendEnumValue)
                title = extendEnumValue.Documentation;
            return title;
        }
        #endregion GetEventCodeTitle
        #region GetEventTypeTitle
        public string GetEventTypeTitle(string pEventType)
        {
            string title = pEventType;
            ExtendEnumValue extendEnumValue = m_EventTypeEnum.GetExtendEnumValueByValue(pEventType);
            if (null != extendEnumValue)
                title = extendEnumValue.Documentation;
            return title;
        }
        #endregion GetEventTypeTitle

        #region InitializeProduct
        public virtual void InitializeProduct(ActionEventsBase pAction)
        {
        }
        #endregion InitializeProduct

        #endregion Methods
    }
    #endregion TradeActionBase

    #region TradeActionEventBase
    public abstract class TradeActionEventBase : TradeActionEventItemBase
    {
        #region Members
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool isModified;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool isValidated;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.Fixed, Name = "", IsVisible = true,Color=MethodsGUI.ColorEnum.Orange)]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.Fixed, Name = "")]
        public ActionEventsBase m_Action;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public ActionEventsBase m_OriginalAction;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public ArrayList validationRulesMessages;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        protected bool m_IsEditable;
        #endregion Members
        #region Accessors
        #region ActionTitle
        public virtual string ActionTitle
        {
            get { return Ressource.GetString2("TradeActionCharacteristics", string.Empty); }
        }
        #endregion ActionTitle

        #region CloseDivEvents
        public object[] CloseDivEvents
        {
            get { return FieldEvents.GetCustomAttributes(typeof(CloseDivGUI), true); }
        }
        #endregion CloseDivEvents
        #region CloseTitle
        protected PlaceHolder CloseTitle(FullConstructor pFullCtor)
        {
            PlaceHolder plh = new PlaceHolder();
            foreach (CloseDivGUI item in CloseDivEvents)
            {
                item.Name = EventTitle;
                plh.Controls.Add(MethodsGUI.MakeDiv(pFullCtor, item, false));
            }
            return plh;
        }
        #endregion CloseTitle
        #region CurrentTradeAction
        public virtual TradeActionBase CurrentTradeAction
        {
            get { return null; }
        }
        #endregion CurrentTradeAction

        #region Events
        public virtual TradeActionEventBase[] Events
        {
            get { return null; }
        }
        #endregion Events
        #region EventTitle
        protected string EventTitle
        {
            get
            {
                string title = string.Empty;
                if (EventCodeFunc.IsProduct(eventCode))
                {
                    title = instrumentNo + ") ";
                    if (displayNameSpecified)
                        return title + displayName;
                }
                title += CurrentTradeAction.GetEventCodeTitle(eventCode);
                if (false == EventTypeFunc.IsDate(eventType) && (eventCode != eventType))
                    title += " - " + CurrentTradeAction.GetEventTypeTitle(eventType);
                return title;
            }
        }
        #endregion EventTitle

        #region FieldEvents
        public virtual FieldInfo FieldEvents
        {
            get { return null; }
        }
        #endregion Events

        #region GetDefaultRowClass
        // EG 20200914 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc) Correction et compléments
        public string GetDefaultRowClass
        {
            get { return GetRowClass("DataGrid_ItemStyle"); }
        }
        #endregion GetDefaultRowClass

        #region Instrument
        protected string Instrument
        {
            get { return instrumentNo + ") " + (displayNameSpecified ? displayName : EventTitle); }
        }
        #endregion Instrument
        #region IsEditable
        public bool IsEditable
        {
            get
            {
                bool isEditable = m_IsEditable;
                if ((false == isEditable) && (null != Events))
                {
                    foreach (TradeActionEventBase item in Events)
                    {
                        isEditable = item.IsEditable;
                        if (isEditable)
                            break;
                    }
                }
                return isEditable;
            }
        }
        #endregion IsEditable
        #region IsEventChanged
        public bool IsEventChanged
        {
            get
            {
                bool isEventChanged = false;
                Type tAction = m_Action.GetType();
                MethodInfo method = tAction.GetMethod("IsEventChanged");
                if (null != method)
                {
                    object[] _params = null;
                    string[] _names = null;
                    ParameterInfo[] pi = method.GetParameters();
                    if (0 < pi.Length)
                    {
                        _params = new object[] { this };
                        _names = new String[] { pi[0].Name };
                    }
                    isEventChanged = (bool)tAction.InvokeMember(method.Name, BindingFlags.InvokeMethod, null, m_Action, _params, null, null, _names);
                }
                return isEventChanged;
            }
        }
        #endregion IsEventChanged        
        #region IsStrategy
        protected bool IsStrategy
        {
            get
            {
                foreach (TradeActionEventBase item in Events)
                {
                    if (EventCodeFunc.IsProduct(item.eventCode))
                        return true;
                }
                return false;
            }
        }
        #endregion IsStrategy

        #region OpenDivEvents
        public object[] OpenDivEvents
        {
            get { return FieldEvents.GetCustomAttributes(typeof(OpenDivGUI), true); }
        }
        #endregion OpenDivEvents
        #region OpenTitle
        protected PlaceHolder OpenTitle(FullConstructor pFullCtor)
        {
            PlaceHolder plh = new PlaceHolder();
            foreach (OpenDivGUI item in OpenDivEvents)
            {
                item.Name = EventTitle;
                plh.Controls.Add(MethodsGUI.MakeDiv(pFullCtor, item, false));
            }
            return plh;
        }
        #endregion OpenTitle

        public virtual bool IsInTheMoney()
        {
            return false;
        }
        public virtual bool IsInTheMoney(decimal pSpot, decimal pStrike, StrikeQuoteBasisEnum pQuoteBasis)
        {
            return false;
        }
        public virtual ActionEventsBase InitializeAction(TradeActionEventBase pEventParent)
        {
            return null;
        }
        #endregion Accessors
        #region Methods
        #region GetRowClass
        // EG 20200914 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc) Correction et compléments
        public string GetRowClass(string pDefaultStyle)
        {
            string cssClass = pDefaultStyle;
            if (TradeActionMode.IsEditPlus(CurrentTradeAction.ActionMode) && (idE == CurrentTradeAction.currentIdE))
                cssClass = "DataGrid_AlternatingItemStyle";
            return cssClass;
        }
        #endregion GetRowClass

        #region CreateCellsAction
        protected TableCell[] CreateCellsAction(TradeActionEvent pEventParent, bool pIsActionAuthorized)
        {
            if (null != m_Action)
            {
                Type tAction = m_Action.GetType();
                MethodInfo method = tAction.GetMethod("CreateCells");
                if (null != method)
                {
                    object[] _params = null;
                    String[] _names = null;
                    ParameterInfo[] pi = method.GetParameters();
                    if (0 < pi.Length)
                    {
                        _params = new object[] { pEventParent, pIsActionAuthorized };
                        _names = new String[] { pi[0].Name, pi[1].Name };
                    }
                    return (TableCell[])tAction.InvokeMember(method.Name, BindingFlags.InvokeMethod, null, m_Action, _params, null, null, _names);
                }
            }
            return null;
        }
        #endregion CreateCellsAction
        #region CreateTable
        // EG 20200914 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc) Correction et compléments
        protected Table CreateTable(bool pIsActionAuthorized)
        {
            Table table = new Table()
            {
                CssClass = "DataGrid",
                CellSpacing = 0,
                CellPadding = 2,
                GridLines = GridLines.Both
            };

            TableRow tr = new TableRow
            {
                CssClass = "DataGrid_HeaderStyle"
            };
            TableHeaderCell[] th_2 = null;

            if (null != m_Action)
            {
                Type tAction = m_Action.GetType();
                MethodInfo method = tAction.GetMethod("CreateTableHeader");
                if (null != method)
                {
                    ParameterInfo[] pi = method.GetParameters();
                    if (0 < pi.Length)
                    {
                        object[] _params = new object[] { this };
                        string[] _names = new string[] { pi[0].Name };
                        ArrayList aTableHeader = (ArrayList)tAction.InvokeMember(method.Name, BindingFlags.InvokeMethod, null,
                            m_Action, _params, null, null, _names);

                        if (0 < aTableHeader.Count)
                        {
                            TableCell[] td = new TableCell[aTableHeader.Count / 2];
                            #region Table SubTitle
                            TableHeaderCell[] th_1 = (TableHeaderCell[])aTableHeader[1];
                            tr.Cells.AddRange(th_1);
                            if (3 < aTableHeader.Count)
                            {
                                th_2 = (TableHeaderCell[])aTableHeader[2];
                                tr.Cells.AddRange(th_2);
                            }
                            if (pIsActionAuthorized)
                                tr.Cells.AddAt(0, TableTools.AddHeaderCell("Action", true, 60, UnitEnum.Pixel, 0, false));
                            table.Rows.AddAt(0, tr);
                            #endregion Table SubTitle
                            #region Table Title
                            tr = new TableRow
                            {
                                TableSection = TableRowSection.TableHeader,
                                CssClass = "DataGrid_HeaderStyle"
                            };
                            td[0] = TableTools.AddCell(aTableHeader[0].ToString(), HorizontalAlign.NotSet, 0, UnitEnum.Pixel, false);
                            td[0].ColumnSpan = NbCells(th_1) + (pIsActionAuthorized ? 1 : 0);
                            if (3 < aTableHeader.Count)
                            {
                                td[1] = TableTools.AddCell(aTableHeader[3].ToString(), HorizontalAlign.NotSet, 0, UnitEnum.Pixel, false);
                                td[1].ColumnSpan = NbCells(th_2);
                            }
                            tr.Cells.AddRange(td);
                            table.Rows.AddAt(0, tr);
                            #endregion Table Title
                        }
                    }
                }
            }
            return table;
        }
        #endregion CreateTable
        #region CreateRow
        protected TableRow CreateRow(bool pIsActionAuthorized, object pEventParent)
        {
            TableRow tr = new TableRow
            {
                CssClass = GetDefaultRowClass,
                ID = idE.ToString()
            };
            if (null != m_Action)
            {
                Type tAction = m_Action.GetType();
                MethodInfo method = tAction.GetMethod("AddCells_Static");
                if (null != method)
                {
                    object[] _params = null;
                    String[] _names = null;
                    ParameterInfo[] pi = method.GetParameters();
                    if (0 < pi.Length)
                    {
                        _params = new object[] { this, pEventParent };
                        _names = new String[] { pi[0].Name, pi[1].Name };
                    }
                    tr.Cells.AddRange((TableCell[])tAction.InvokeMember(method.Name, BindingFlags.InvokeMethod, null,
                        m_Action, _params, null, null, _names));
                }
            }
            if (pIsActionAuthorized)
            {
                m_IsEditable = true;
                tr.Cells.AddAt(0, NewHyperLinkCell());
            }
            return tr;
        }
        #endregion CreateRow
        #region CreateControlCurrentAction
        // EG 20200914 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc) Correction et compléments
        public Table CreateControlCurrentAction
        {
            get
            {
                Table table = new Table()
                {
                    CssClass = "DataGrid",
                    CellSpacing = 0,
                    CellPadding = 2,
                    HorizontalAlign = HorizontalAlign.Center,
                    GridLines = GridLines.Vertical,
                    Width = Unit.Percentage(50)
                };
                table.Style.Add(HtmlTextWriterStyle.Margin, "0% 25%");
                TableRow tr = new TableRow
                {
                    CssClass = "DataGrid_HeaderStyle"
                };
                tr.Cells.Add(TableTools.AddHeaderCell(ActionTitle, false, 500, UnitEnum.Pixel, 0, true));
                table.Rows.Add(tr);

                if (null != m_Action)
                {
                    Type tAction = m_Action.GetType();
                    PropertyInfo pty = tAction.GetProperty("CreateControlCurrentAction");
                    if (null != pty)
                        table.Rows.AddRange((TableRow[])tAction.InvokeMember(pty.Name, BindingFlags.GetProperty, null, m_Action, null, null, null, null));
                }
                return table;
            }
        }
        #endregion CreateControlCurrentAction
        #region CurrentProduct
        public object CurrentProduct(string pInstrumentNo)
        {
            return CurrentTradeAction.CurrentProduct;
        }
        #endregion CurrentProduct
        #region DisplayTitle
        public PlaceHolder DisplayTitleAction(string pTitleSubstitute, FullConstructor pFullCtor)
        {
            PlaceHolder ph = new PlaceHolder();
            FieldInfo fldObject = this.GetType().GetField("m_Action");
            object[] attributes = fldObject.GetCustomAttributes(typeof(OpenDivGUI), true);
            foreach (OpenDivGUI openDiv in attributes)
            {
                if (StrFunc.IsFilled(pTitleSubstitute))
                    openDiv.Name = pTitleSubstitute;
                ph.Controls.Add(MethodsGUI.MakeDiv(pFullCtor, openDiv, false));
            }
            return ph;
        }
        #endregion DisplayTitle
        #region DisplayValidationRulesMessage
        // EG 20200914 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc) Correction et compléments
        public Table DisplayValidationRulesMessage
        {
            get
            {
                Table table = new Table()
                {
                    CssClass = "DataGrid",
                    CellSpacing = 0,
                    CellPadding = 2,
                    HorizontalAlign = HorizontalAlign.Center,
                    GridLines = GridLines.Both,
                    Width = Unit.Percentage(50),

                };
                table.Style.Add(HtmlTextWriterStyle.Margin, "0% 25%");

                TableRow tr = new TableRow
                {
                    CssClass = "DataGrid_HeaderErrorStyle"
                };
                tr.Cells.Add(TableTools.AddHeaderCell("ERROR", true, 500, UnitEnum.Pixel, 0, true));
                table.Rows.Add(tr);

                if ((null != m_Action) && (null != validationRulesMessages))
                {
                    tr = new TableRow
                    {
                        CssClass = "DataGrid_ErrorStyle"
                    };
                    TableCell td = new TableCell();
                    foreach (string validationRulesMessage in validationRulesMessages)
                    {
                        string message = Ressource.GetString(validationRulesMessage, true);
                        LiteralControl lit = new LiteralControl(message);
                        td.Controls.Add(lit);
                    }
                    tr.Cells.Add(td);
                    table.Rows.Add(tr);
                }
                return table;
            }
        }
        #endregion DisplayValidationRulesMessage
        #region GetChanges
        // EG 20180423 Analyse du code Correction [CA2200]
        public Cst.ErrLevel GetChanges(ArrayList pLstIdE)
        {
            Cst.ErrLevel ret = Cst.ErrLevel.SUCCESS;
            try
            {
                if (null != Events)
                {
                    foreach (TradeActionEventBase item in Events)
                    {
                        ret = item.GetChanges(pLstIdE);
                        if (Cst.ErrLevel.SUCCESS != ret)
                            break;
                    }
                }
                if (isModified)
                    pLstIdE.Add(idE);
            }
            catch (Exception) { throw; } 
            return ret;
        }
        #endregion GetChanges
        #region GetEventEditable
        public TradeActionEventBase GetEventEditable()
        {

            if (m_IsEditable && (null != m_Action) && (false == isValidated))
                return this;
            else
            {
                TradeActionEventBase _event = null;
                if (null != Events)
                {
                    foreach (TradeActionEventBase item in Events)
                    {
                        _event = item.GetEventEditable();
                        if (null != _event)
                            return _event;
                    }
                }
                return _event;
            }
        }
        #endregion GetEventEditable
        #region NbCells
        private static int NbCells(TableHeaderCell[] pCells)
        {
            int nbCells = 0;
            foreach (TableHeaderCell th in pCells)
            {
                if (0 < th.ColumnSpan)
                    nbCells += th.ColumnSpan;
                else
                    nbCells++;
            }
            return nbCells;
        }
        #endregion NbCells
        #region NewHyperLinkCell
        // EG 20200914 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc) Correction et compléments
        private TableCell NewHyperLinkCell()
        {

            TableCell td = TableTools.AddCell(string.Empty, HorizontalAlign.Center);
            HyperLink lnk = new HyperLink
            {
                ID = "LNK" + idE.ToString(),
                NavigateUrl = "javascript:__doPostBack('" + idE.ToString() + "','" + TradeActionMode.TradeActionModeEnum.Edit + "')",
                CssClass = "fa-icon"
            };
            bool isModeEdit = TradeActionMode.IsEditPlus(CurrentTradeAction.ActionMode);
            if (isModeEdit && (idE == CurrentTradeAction.currentIdE))
            {
                lnk.Text = "<i class='fas fa-edit'></i>";
                lnk.NavigateUrl = string.Empty;
            }
            else
            {
                lnk.Text = "<i class='far fa-edit'></i>";
                lnk.Font.Name = "verdana";
                lnk.Font.Bold = false;
                lnk.Enabled = true;
            }
            td.Controls.Add(lnk);
            return td;

        }
        #endregion NewHyperLinkCell
        #region NewCells_Static
        public TableCell[] NewCells_Static()
        {
            ArrayList aCell = new ArrayList
            {
                ((ActionEventsBase)m_Action).AddCellEventCode(eventCode),
                ((ActionEventsBase)m_Action).AddCellEventType(eventType),
                TableTools.AddCell(DtFunc.DateTimeToString(dtStartPeriod.DateValue, DtFunc.FmtShortDate), HorizontalAlign.Center),
                TableTools.AddCell(DtFunc.DateTimeToString(dtEndPeriod.DateValue, DtFunc.FmtShortDate), HorizontalAlign.Center)
            };
            return (TableCell[])aCell.ToArray(typeof(TableCell));
        }
        #endregion NewCells_Static

        #endregion Methods
    }
    #endregion TradeActionEventBase
    #region TradeActionEventDetailsBase
    /// <summary>
	/// Class where are stored by serialisation the lines of the table <b>EventDet</b> for a trade.
	/// </summary>
	public abstract class TradeActionEventDetailsBase : EventDetails
	{
		#region Methods
		#region AddCells_Static
		public virtual TableCell[] AddCells_Static
		{
			get {return null;}
		}
		#endregion AddCells_Static
		#endregion Methods
    }
    #endregion TradeActionEventDetailsBase
    #region TradeActionEventItemBase
    // EG 20151102 [21514] 
    public abstract class TradeActionEventItemBase : EventItem
    {
        #region Members
        // EG 20100127 Add identifier_TradeSource
        // EG 20151102 [21514] Rename XmlElementAttribute for identifier_TradeSource [ES_IDENTIFIER_TRADE] instead of [ES_IDENTIFIER_TRADESOURCE]
        [System.Xml.Serialization.XmlElementAttribute("ES_IDENTIFIER_TRADE")]
        public string identifier_TradeSource;
        [System.Xml.Serialization.XmlElementAttribute("AC1_IDENTIFIER")]
		public string payer;
		[System.Xml.Serialization.XmlElementAttribute("BK1_IDENTIFIER")]
		public string payerBook;
		[System.Xml.Serialization.XmlElementAttribute("AC2_IDENTIFIER")]
		public string receiver;
		[System.Xml.Serialization.XmlElementAttribute("BK2_IDENTIFIER")]
		public string receiverBook;
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		public bool displayNameSpecified;
		[System.Xml.Serialization.XmlElementAttribute("DISPLAYNAME")]
		public string displayName;
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		public bool assetSpecified;
		[System.Xml.Serialization.XmlElementAttribute("Asset")]
		public EventAsset asset;
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		public bool detailsSpecified;
		[System.Xml.Serialization.XmlElementAttribute("Details")]
        public TradeActionEventDetails details;
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		public bool eventClassSpecified;
		[System.Xml.Serialization.XmlElementAttribute("EventClass")]
		public EventClass[] eventClass;
        #endregion Members
        #region Accessors
        #region EventClassFixing
        public EventClass EventClassFixing
        {
            get {return GetEventClass(EventClassFunc.Fixing);}
        }
        #endregion EventClassFixing
        #region EventClassGroupLevel
        public EventClass EventClassGroupLevel
        {
            get { return GetEventClass(EventClassEnum.GRP.ToString());}
        }
        #endregion EventClassGroupLevel
        #region EventClassRecognition
        public EventClass EventClassRecognition
        {
            get {return GetEventClass(EventClassFunc.Recognition);}
        }
        #endregion EventClassRecognition
        #region EventClassSettlement
        public EventClass EventClassSettlement
        {
            get {return GetEventClass(EventClassFunc.Settlement);}
        }
        #endregion EventClassSettlement


        #region EventClassExerciseProvision
        public EventClass EventClassExerciseProvision
        {
            get
            {
                if (null != eventClass)
                {
                    foreach (EventClass item in eventClass)
                    {
                        if (EventClassFunc.IsExerciseProvision(item.code))
                            return item;
                    }
                }
                return null;
            }
        }
        #endregion EventClassExerciseProvision
        #region EventClassExerciseCancelableProvision
        public EventClass EventClassExerciseCancelableProvision
        {
            get {return GetEventClass(EventClassFunc.ExerciseCancelable);}
        }
        #endregion EventClassExerciseCancelableProvision
        #region EventClassExerciseExtendibleProvision
        public EventClass EventClassExerciseExtendibleProvision
        {
            get {return GetEventClass(EventClassFunc.ExerciseExtendible);}
        }
        #endregion EventClassExerciseExtendibleProvision
        #region EventClassExerciseMandatoryEarlyTerminationProvision
        public EventClass EventClassExerciseMandatoryEarlyTerminationProvision
        {
            get {return GetEventClass(EventClassFunc.ExerciseMandatoryEarlyTermination);}
        }
        #endregion EventClassExerciseMandatoryEarlyTerminationProvision
        #region EventClassExerciseOptionalEarlyTerminationProvision
        public EventClass EventClassExerciseOptionalEarlyTerminationProvision
        {
            get {return GetEventClass(EventClassFunc.ExerciseOptionalEarlyTermination);}
        }
        #endregion EventClassExerciseOptionalEarlyTerminationProvision
        #region EventClassExerciseStepUpProvision
        public EventClass EventClassExerciseStepUpProvision
        {
            get {return GetEventClass(EventClassFunc.ExerciseStepUp);}
        }
        #endregion EventClassExerciseStepUpProvision


        #region Family
        public string Family
        {
            get
            {
                string _family = string.Empty;
                if (familySpecified)
                    _family = family;
                return _family;
            }
        }
        #endregion Family
        #endregion Accessors
        #region Constructors
        public TradeActionEventItemBase()
		{
			valorisation = new EFS_Decimal();
		}
		#endregion Constructors
        #region Methods
        #region GetEventClass
        protected EventClass GetEventClass(string pEventClass)
        {
            if (null != eventClass)
            {
                foreach (EventClass item in eventClass)
                {
                    if (item.codeSpecified && (item.code == pEventClass))
                        return item;
                }
            }
            return null;
        }
        #endregion GetEventClass
        #endregion Methods
    }
    #endregion TradeActionEventItemBase
    #region TradeActionEventsBase
    public class TradeActionEventsBase
    {
        #region Members
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool idTSpecified;
        [System.Xml.Serialization.XmlElementAttribute("IDT")]
        public int idT;
        [System.Xml.Serialization.XmlElementAttribute("family")]
        public string family;
        [System.Xml.Serialization.XmlElementAttribute("actionType")]
        public TradeActionType.TradeActionTypeEnum tradeActionType;
        #endregion Members
        #region Accessors
        #region Events
        public virtual TradeActionEventBase Events
        {
            get { return null; }
        }
        #endregion Events
        #region IsEditable
        public bool IsEditable
        {
            get
            {
                foreach (TradeActionEventBase item in SubEvents)
                {
                    if (item.IsEditable)
                        return true;
                }
                return false;
            }
        }
        #endregion IsEditable
        #region SubEvents
        public virtual TradeActionEventBase[] SubEvents
        {
            get {return null;}
        }
        #endregion SubEvents
        #endregion Accessors
        #region Methods
        #region GetEventEditable
        public TradeActionEventBase GetEventEditable()
        {
            return Events.GetEventEditable();
        }
        #endregion GetEventEditable
        #region GetChanges
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public ArrayList GetChanges
        {
            get
            {
                ArrayList lstIdE = new ArrayList();
                if (Cst.ErrLevel.SUCCESS == Events.GetChanges(lstIdE))
                    return lstIdE;
                return null;
            }
        }
        #endregion GetChanges
        #endregion Methods
    }
    #endregion TradeActionEventsBase

    #region ActionEventsBase
    // EG 20100127 New ResFormTitleTrade
    // EG 20100208 ResFormTitleEventId
    public abstract class ActionEventsBase : ICloneable
    {
        #region Members
        public ExtendEnum eventCodeEnum;
        public ExtendEnum eventTypeEnum;
        public int idE;
        public EFS_Date actionDate;
        public EFS_Time actionTime;
        public string note;
        #endregion Members

        #region Virtual accessors
        public virtual EFS_Date ValueDate { set; get; }

        // EG 20150428 [20513] New
        protected virtual TradeActionBase TradeAction {get { return null; }}
        // EG 20150428 [20513] New
        protected virtual TradeActionBase TradeAdminAction{get { return null; }}
        #endregion Virtual accessors
        #region Accessors
        #region ActionDate
        public string ActionDate
        {
            get
            {
                string dt = Cst.HTMLSpace;
                if (DtFunc.IsDateTimeFilled(this.actionDate.DateValue))
                    dt = DtFunc.DateTimeToString(this.actionDate.DateValue, DtFunc.FmtShortDate);
                return dt;
            }
        }
        #endregion ActionDate
        #region ActionDateTime
        public DateTime ActionDateTime
        {
            get { return DtFunc.AddTimeToDate(actionDate.DateValue, actionTime.TimeValue); }
        }
        #endregion ActionDateTime
        #region ActionEvent
        public virtual TradeActionEventBase ActionEvent
        {
            get { return null; }
        }
        #endregion ActionEvent
        #region CreateControlDescription
        // EG 20200914 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc) Correction et compléments
        protected TableRow[] CreateControlDescription
        {
            get
            {
                ArrayList aTableRow = new ArrayList
                {
                    CreateControlBreakLine(),
                    CreateControlTitleSeparator(ResFormTitleNoteEvents, false)
                };
                TableRow tr = new TableRow
                {
                    CssClass = "DataGrid_ItemStyle"
                };
                TableCell td = new TableCell();
                ControlGUI controlGUI = new ControlGUI(false, ResFormTitleNoteEvents);
                FpMLTextBox txtComment = new FpMLTextBox(null, note, 0, "Comment", controlGUI, null, false, "TXTCOMMENT_" + idE, null)
                {
                    Width = Unit.Percentage(99.2),
                    TextMode = TextBoxMode.MultiLine,
                    CssClass = EFSCssClass.CaptureMultiline,
                    Height = Unit.Pixel(50)
                };
                txtComment.Style.Add(HtmlTextWriterStyle.Overflow,"auto");
                txtComment.IsMultilineMode = true;
                td.Controls.Add(txtComment);
                tr.Cells.Add(td);
                aTableRow.Add(tr);
                return (TableRow[])aTableRow.ToArray(typeof(TableRow));
            }
        }
        #endregion CreateControlDescription
        #region CurrentProduct
        protected object CurrentProduct
        {
            get { return ActionEvent.CurrentProduct(ActionEvent.instrumentNo); }
        }
        #endregion CurrentProduct
        #region Resource in Form
        #region Label
        #region ResFormActionDate
        protected virtual string ResFormActionDate { get { return Ressource.GetString("DateEvents"); } }
        #endregion ResFormActionDate
        #region ResFormActionTime
        protected virtual string ResFormActionTime { get { return Ressource.GetString("TimeEvents"); } }
        #endregion ResFormActionTime
        #region ResFormAmount
        protected virtual string ResFormAmount { get { return Ressource.GetString("Amount"); } }
        #endregion ResFormAmount

        #region ResFormCurrency
        protected virtual string ResFormCurrency { get { return Ressource.GetString("Currency"); } }
        #endregion ResFormCurrency
        #region ResFormQty
        protected virtual string ResFormQty { get { return Ressource.GetString("nbOfOptions"); } }
        #endregion ResFormQty

        #region ResFormPayer
        protected virtual string ResFormPayer { get { return Ressource.GetString("Payer"); } }
        #endregion ResFormPayer
        #region ResFormReceiver
        protected virtual string ResFormReceiver { get { return Ressource.GetString("Receiver"); } }
        #endregion ResFormReceiver
        #region ResFormValueDate
        protected virtual string ResFormValueDate { get { return Ressource.GetString("ValueDate"); } }
        #endregion ResFormValueDate
        #endregion Label
        #region Title
        #region ResFormTitleEventId
        protected virtual string ResFormTitleEventId { get { return "IdE"; } }
        #endregion ResFormTitleEventId
        #region ResFormTitleComplementary
        protected virtual string ResFormTitleComplementary { get { return Ressource.GetString("ComplementaryCapture"); } }
        #endregion ResFormTitleComplementary
        #region ResFormTitleDetail
        protected virtual string ResFormTitleDetail { get { return Ressource.GetString("Detail"); } }
        #endregion ResFormTitleDetail
        #region ResFormTitleEventCode
        protected virtual string ResFormTitleEventCode { get { return Ressource.GetString("EventCode"); } }
        #endregion ResFormTitleEventCode
        #region ResFormTitleTrade
        protected virtual string ResFormTitleTrade 
        { 
            //get { return Ressource.GetString("Trade"); }
            get { return Ressource.GetString("Trade_Title"); }
        }
        #endregion ResFormTitleTrade
        #region ResFormTitleEventType
        protected virtual string ResFormTitleEventType { get { return Ressource.GetString("EventType"); } }
        #endregion ResFormTitleEventType
        #region ResFormTitlePeriod
        protected virtual string ResFormTitlePeriod { get { return Ressource.GetString("Periods"); } }
        #endregion ResFormTitlePeriod
        #region ResFormTitleNoteEvents
        protected virtual string ResFormTitleNoteEvents { get { return Ressource.GetString("NoteEvents"); } }
        #endregion ResFormTitleNoteEvents
        #endregion Title
        #endregion Resource in Form
        #region ValueDate
        protected virtual string FormatedValueDate
        {

            get
            {
                string valueDate = Cst.HTMLSpace;
                if ((null != this.ValueDate) && DtFunc.IsDateTimeFilled(this.ValueDate.DateValue))
                    valueDate = DtFunc.DateTimeToString(this.ValueDate.DateValue, DtFunc.FmtShortDate);
                return valueDate;
            }
        }
        #endregion ValueDate

        #region IsActionChanged
        public virtual bool IsActionChanged
        {
            get { return false; }
        }
        #endregion IsActionChanged

        #endregion Accessors
        #region Constructors
        public ActionEventsBase() { }
        public ActionEventsBase(TradeActionEventBase pEvent)
        {
            /* FI 20240731 [XXXXX] Mise en commentaire => use DataEnabledEnum/DataEnabledEnumHelper
            eventCodeEnum = ExtendEnumsTools.ListEnumsSchemes["EventCode"];
            eventTypeEnum = ExtendEnumsTools.ListEnumsSchemes["EventType"];
            */

            eventCodeEnum = DataEnabledEnumHelper.GetDataEnum(SessionTools.CS, "EventCode");
            eventTypeEnum = DataEnabledEnumHelper.GetDataEnum(SessionTools.CS, "EventType");
            idE = pEvent.idE;
            actionDate = new EFS_Date();
            actionTime = new EFS_Time();
            if (null != pEvent.details)
            {
                note = pEvent.details.note;
                if (pEvent.details.dtActionSpecified)
                {
                    actionDate.DateValue = pEvent.details.dtAction.DateValue;
                    actionTime.TimeValue = pEvent.details.dtAction.DateTimeValue;
                }
            }
        }
        #endregion Constructors
        #region Methods
        #region AddCellEventCode
        public TableCell AddCellEventCode(string pEventCode)
        {
            TableCell td = TableTools.AddCell(pEventCode, HorizontalAlign.Center, false);
            ExtendEnumValue extValue = eventCodeEnum.GetExtendEnumValueByValue(pEventCode);
            if (null != extValue)
            {
                td.ToolTip = extValue.ExtValue;
                td.Style.Add(HtmlTextWriterStyle.Cursor, "pointer");
            }
            return td;
        }
        #endregion AddCellEventCode
        #region AddCellEventType
        public TableCell AddCellEventType(string pEventType)
        {
            TableCell td = TableTools.AddCell(pEventType, HorizontalAlign.Center, false);
            ExtendEnumValue extValue = eventTypeEnum.GetExtendEnumValueByValue(pEventType);
            if (null != extValue)
            {
                td.ToolTip = extValue.ExtValue;
                td.Style.Add(HtmlTextWriterStyle.Cursor, "pointer");
            }
            return td;
        }
        #endregion AddCellEventType
        #region CreateControlActionDate
        protected TableRow CreateControlActionDate()
        {
            return CreateControlActionDate(false);
        }

        protected TableRow CreateControlActionDate(bool pIsReadOnly)
        {
            return CreateControlActionDate(pIsReadOnly, null);
        }
        protected TableRow CreateControlActionDate(Validator pValidator)
        {
            return CreateControlActionDate(false, pValidator);
        }
        protected TableRow CreateControlActionDate(bool pIsReadOnly, Validator pValidator)
        {
            //StringBuilder sb = new StringBuilder();
            TableRow tr = new TableRow
            {
                CssClass = "DataGrid_ItemStyle"
            };
            TableCell td = new TableCell();
            ControlGUI controlGUI = new ControlGUI(true, ResFormActionDate)
            {
                Regex = EFSRegex.TypeRegex.RegexDate,
                LblWidth = 105
            };
            FpMLCalendarBox txtDate;
            if (null != pValidator)
                txtDate = new FpMLCalendarBox(null, actionDate.DateValue, "ActionDate", controlGUI, null, "TXTACTIONDATE_" + idE,
                    new Validator("ActionDate", true), new Validator(EFSRegex.TypeRegex.RegexDate, "ActionDate", true, false),
                    pValidator);
            else
                txtDate = new FpMLCalendarBox(null, actionDate.DateValue, "ActionDate", controlGUI, null, "TXTACTIONDATE_" + idE,
                    new Validator("ActionDate", true), new Validator(EFSRegex.TypeRegex.RegexDate, "ActionDate", true, false));
            txtDate.ID = "TXTACTIONDATE_" + idE;
            txtDate.ReadOnly = pIsReadOnly;
            if (false == pIsReadOnly)
                GetFormatControlAttribute(txtDate);
            td.Controls.Add(txtDate);
            tr.Cells.Add(td);
            return tr;
        }
        #endregion CreateControlActionDate
        #region CreateControlActionTime
        protected TableRow CreateControlActionTime()
        {
            return CreateControlActionTime(false);
        }
        protected TableRow CreateControlActionTime(bool pIsReadOnly)
        {
            TableRow tr = new TableRow
            {
                CssClass = "DataGrid_ItemStyle"
            };
            TableCell td = new TableCell();
            ControlGUI controlGUI = new ControlGUI(true, ResFormActionTime)
            {
                Regex = EFSRegex.TypeRegex.RegexLongTime,
                LblWidth = 100
            };
            FpMLTextBox txtTime = new FpMLTextBox(null, actionTime.Value, 65, "ActionTime", controlGUI, null, false, "TXTACTIONTIME_" + idE, null,
                new Validator("ActionTime", true),
                new Validator(EFSRegex.TypeRegex.RegexLongTime, "ActionTime", true, false))
            {
                IsLocked = pIsReadOnly
            };
            if (false == pIsReadOnly)
                GetFormatControlAttribute(txtTime);
            td.Controls.Add(txtTime);
            tr.Cells.Add(td);
            return tr;
        }
        #endregion CreateControlActionTime
        #region CreateControlBreakLine
        // EG 20200908 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc) Correction et compléments
        protected static TableRow CreateControlBreakLine()
        {
            TableRow tr = new TableRow();
            TableCell td = new TableCell();
            td.Controls.Add(new LiteralControl("<br/>"));
            tr.Cells.Add(td);
            return tr;
        }
        #endregion CreateControlSeparator
        #region CreateControlSeparator
        protected static TableRow CreateControlSeparator(params string[] pStyles)
        {
            TableRow tr = new TableRow
            {
                CssClass = "DataGrid_ItemStyle"
            };
            TableCell td = new TableCell();
            HtmlGenericControl div = new HtmlGenericControl("div");
            div.Attributes.Add("class", "hr");
            LiteralControl hr = new LiteralControl("<hr/>");
            if (null != pStyles)
            {
                for (int i = 0; i < pStyles.Length; i++)
                {
                    string[] style = pStyles[i].Split(':');
                    if (2 == style.Length)
                    {
                        div.Style.Add(style[0], style[1]);
                    }
                }
            }
            div.Controls.Add(hr);
            td.Controls.Add(div);
            tr.Cells.Add(td);
            return tr;
        }
        #endregion CreateControlSeparator
        #region CreateControlTitleSeparator
        protected static TableRow CreateControlTitleSeparator(string pTitle)
        {
            return CreateControlTitleSeparator(pTitle, string.Empty, true);
        }
        protected static TableRow CreateControlTitleSeparator(string pTitle, bool pIsRessource)
        {
            return CreateControlTitleSeparator(pTitle, string.Empty, pIsRessource);
        }
        protected static TableRow CreateControlTitleSeparator(string pTitle, string pId)
        {
            return CreateControlTitleSeparator(pTitle, pId, true);
        }
        // EG 20200914 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc) Correction et compléments
        protected static TableRow CreateControlTitleSeparator(string pTitle, string pId, bool pIsRessource)
        {
            TableRow tr = new TableRow
            {
                CssClass = "DataGrid_HeaderStyle"
            };
            TableCell td = new TableCell
            {
                Text = (StrFunc.IsFilled(pTitle) && pIsRessource) ? Ressource.GetString(pTitle) : pTitle,
                Wrap = false
            };
            if (StrFunc.IsFilled(pId))
                td.ID = pId;
            tr.Cells.Add(td);
            return tr;
        }
        #endregion CreateControlTitleSeparator
        #region FormatControl
        // 200900403 Eg Ticket 16540 : passage en virtual
        public virtual void FormatControl(Page pPage, string pControlId)
        {
            CustomCaptureInfo cci = new CustomCaptureInfo();
            Control ctrl = pPage.FindControl(pControlId);
            TextBox txtCtrl = null;
            try
            {
                if (null != ctrl)
                {
                    Type tCtrl = ctrl.GetType();
                    if (tCtrl.Equals(typeof(FpMLCalendarBox)))
                        cci.Regex = ((FpMLCalendarBox)ctrl).Regex;
                    else if (tCtrl.Equals(typeof(FpMLTextBox)))
                        cci.Regex = ((FpMLTextBox)ctrl).Regex;
                    else
                        return;

                    if ((EFSRegex.TypeRegex.RegexDate == cci.Regex) || (EFSRegex.TypeRegex.RegexDateTime == cci.Regex))
                        cci.DataType = TypeData.TypeDataEnum.date;
                    else if (EFSRegex.IsInteger(cci.Regex))
                        cci.DataType = TypeData.TypeDataEnum.integer;
                    else if (EFSRegex.TypeRegex.RegexLongTime == cci.Regex)
                        cci.DataType = TypeData.TypeDataEnum.time;
                    else if (EFSRegex.IsNumber(cci.Regex))
                        cci.DataType = TypeData.TypeDataEnum.@decimal;

                    txtCtrl = (TextBox)ctrl;
                    cci.NewValueFromLiteral = txtCtrl.Text;
                    txtCtrl.Text = cci.NewValueFmtToCurrentCulture;
                }
            }
            catch (Exception)
            {
                if (null != txtCtrl)
                    txtCtrl.Text = string.Empty;
            }
        }
        #endregion FormatControl
        #region FormatCell
        public virtual string FormatCell(string pValue, EFSRegex.TypeRegex pRegex)
        {
            CustomCaptureInfo cci = new CustomCaptureInfo
            {
                Regex = pRegex,
                NewValueFromLiteral = pValue
            };
            if ((EFSRegex.TypeRegex.RegexDate == cci.Regex) || (EFSRegex.TypeRegex.RegexDateTime == cci.Regex))
                cci.DataType = TypeData.TypeDataEnum.date;
            else if (EFSRegex.IsInteger(cci.Regex))
                cci.DataType = TypeData.TypeDataEnum.integer;
            else if (EFSRegex.TypeRegex.RegexLongTime == cci.Regex)
                cci.DataType = TypeData.TypeDataEnum.time;
            else if (EFSRegex.IsNumber(cci.Regex))
                cci.DataType = TypeData.TypeDataEnum.@decimal;

            return cci.NewValueFmtToCurrentCulture;
        }
        #endregion FormatCell
        #region GetFormatControlAttribute
        protected static void GetFormatControlAttribute(TextBox pControl)
        {
            StringBuilder sb = new StringBuilder("DisableValidators();");
            sb.AppendFormat("javascript:__doPostBack('{0}','{1}');", pControl.ID, TradeActionMode.TradeActionModeEnum.FormatControl);
            pControl.Attributes.Add("onchange", sb.ToString());
        }
        #endregion GetFormatControlAttribute
        #region IsEventChanged
        public virtual bool IsEventChanged(TradeActionEventBase pEvent)
        {
            string prevNote = string.Empty;
            DateTime prevDtAction = DateTime.MinValue;
            if (pEvent.detailsSpecified)
            {
                if (pEvent.details.dtActionSpecified)
                    prevDtAction = pEvent.details.dtAction.DateTimeValue;
                if (pEvent.details.noteSpecified)
                    prevNote = pEvent.details.note;
            }
            //20090202 EG
            return (ActionDateTime != prevDtAction) || (note != prevNote);
        }
        #endregion IsEventChanged
        #region Save
        /// <revision>
        ///     <version>1.1.0 build 46</version><date>20060628</date><author>EG</author>
        ///     <EurosysSupport>N° 10270</EurosysSupport>
        ///     <comment>
        ///     La méthode retourne désormais un boolean indiquant la validité
        ///     de la saisie de l'exercice
        ///     </comment>
        /// </revision>
        public virtual bool Save(Page pPage)
        {
            bool isOk = true;
            if (null != pPage.Request.Form["TXTACTIONDATE_" + idE])
                actionDate.DateValue = new DtFunc().StringToDateTime(pPage.Request.Form["TXTACTIONDATE_" + idE]);
            else if (null != pPage.Request.Form["DDLACTIONDATE_" + idE])
                actionDate.DateValue = new DtFunc().StringToDateTime(pPage.Request.Form["DDLACTIONDATE_" + idE]);

            if (null != pPage.Request.Form["TXTACTIONTIME_" + idE])
                actionTime.Value = pPage.Request.Form["TXTACTIONTIME_" + idE];
            if (null != pPage.Request.Form["TXTCOMMENT_" + idE])
                note = pPage.Request.Form["TXTCOMMENT_" + idE];
            return isOk;
        }
        #endregion Save
        #region ValidationRules
        public virtual bool ValidationRules(Page pPage)
        {
            bool isOk = true;
            #region ActionDate
            string id = "DDLACTIONDATE_" + idE;
            if (null != pPage.Request.Form[id])
                isOk = (StrFunc.IsFilled(pPage.Request.Form[id].ToString()));
            else
            {
                id = "TXTACTIONDATE_" + idE;
                if (null != pPage.Request.Form[id])
                    isOk = (StrFunc.IsFilled(pPage.Request.Form[id].ToString()));
            }
            if (false == isOk)
                ActionEvent.validationRulesMessages.Add(Ressource.GetString2("Msg_ProvisionActionDateRequired"));
            #endregion ActionDate
            return isOk;
        }
        #endregion ValidationRules
        #endregion Methods

        #region Clone
        public virtual object Clone()
        {
            return null;
        }
        #endregion Clone

    }
    #endregion ActionEventsBase

}
