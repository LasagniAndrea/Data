#region Using Directives
using System;
using System.Collections;
using System.Data;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;

using EFS.ACommon;
using EFS.Common.Web;
using EFS.Actor;
using EFS.ApplicationBlocks.Data;
using EFS.Common;
using EFS.Common.MQueue;

using EFS.GUI;
using EFS.GUI.Attributes;
using EFS.GUI.CCI;
using EFS.GUI.Interface;



using EFS.Status;
using EFS.Tuning;
using EFS.Permission;

using EfsML;
using EfsML.Business;
using EfsML.Enum;
using EfsML.Enum.Tools;
using EfsML.Interface;

using FpML.Enum;
using FpML.Interface;
#endregion Using Directives

namespace EFS.TradeInformation
{
    /// <summary>
    /// EventCustomCaptureInfos
    /// </summary>
    public sealed class EventCustomCaptureInfos : CustomCaptureInfosBase, ICloneable
    {
        #region CCst
        /// <summary>
        /// CCst: Contient les constantes nécessaires à la saisie. 
        /// </summary>
        public sealed class CCst
        {
            public CCst() { }
            #region Constants
            public const string Prefix_event = "event";
            public const string Prefix_eventParent = "eventParent";
            public const string Prefix_eventClass = "eventClass";
            public const string Prefix_eventDet = "eventDet";
            public const string Prefix_eventProcess = "eventProcess";
            public const string Prefix_eventAsset = "eventAsset";
            public const string Prefix_eventChild = "eventChild";

            public const string Prefix_eventDet_currencyPair = "currencyPair";
            public const string Prefix_eventDet_dayCountFraction = "dayCountFraction";
            public const string Prefix_eventDet_capfloorSchedule = "capFloorSchedule";
            public const string Prefix_eventDet_exchangeRate = "exchangeRate";
            public const string Prefix_eventDet_exchangeRatePremium = "exchangeRatePremium";
            public const string Prefix_eventDet_fixedRate = "fixedRate";
            public const string Prefix_eventDet_fixingRate = "fixingRate";
            public const string Prefix_eventDet_paymentQuote = "paymentQuote";
            public const string Prefix_eventDet_premiumQuote = "premiumQuote";
            public const string Prefix_eventDet_settlementRate = "settlementRate";
            public const string Prefix_eventDet_sideRate = "sideRate";
            public const string Prefix_eventDet_strikePrice = "strikePrice";
            public const string Prefix_eventDet_triggerRate = "triggerRate";
            public const string Prefix_eventDet_notes = "notes";
            public const string Prefix_eventDet_pricingFx = "pricingFx";
            public const string Prefix_eventDet_pricingFxOption = "pricingFxOption";
            public const string Prefix_eventDet_pricingIRD = "pricingIRD";
            public const string Prefix_eventDet_Closing = "closing";

            #endregion Constants
        }
        #endregion CCst
        #region Members
        #endregion Members
        #region Accessors
        #region CciEvent
        public CciEvent CciEvent
        {
            get { return (CciEvent)CciContainer; }
            set { CciContainer = value; }
        }
        #endregion CciEvent
        #region EventCaptureGen
        public EventInput EventInput
        {
            get { return (EventInput)Obj; }
        }
        #endregion EventCaptureGen
        #region IsPreserveData
        public override bool IsPreserveData
        {
            get { return false; }
        }
        #endregion IsPreserveData
        #endregion Accessors
        #region Constructor
        public EventCustomCaptureInfos(string pCS, EventInput pEventInput) :
            this(pCS, pEventInput, null, string.Empty, true) { }
        public EventCustomCaptureInfos(string pCS, EventInput pEventInput, User pUser, string pSessionId, bool pIsGetDefaultOnInitializeCci)
            : base(pCS, pEventInput, pUser, pSessionId, pIsGetDefaultOnInitializeCci) { }
        #endregion Constructors
        #region Methods
        #region AddDDLParty
        private void AddDDLParty(DropDownList pDDL, CciEvent.CciEnum pCciEnum)
        {
            CustomCaptureInfo cciTemp = CciEvent.Cci(pCciEnum);
            if (cciTemp.IsFilled)
            {
                //int id = ((SQL_Actor)cciTemp.Sql_Table).Id;
                string identifier = cciTemp.NewValue;
                pDDL.Items.Add(new ListItem(identifier, identifier));
            }
        }
        #endregion AddDDLParty
        #region AddDDLBook
        private void AddDDLBook(DropDownList pDDL, CciEvent.CciEnum pCciEnum)
        {
            CustomCaptureInfo cciTemp = CciEvent.Cci(pCciEnum);
            if (cciTemp.IsFilled)
            {
                //int id = ((SQL_Book)cciTemp.Sql_Table).Id;
                string identifier = cciTemp.NewValue;
                pDDL.Items.Add(new ListItem(identifier, identifier));
            }
        }
        #endregion AddDDLBook
        #region Dump_ToGUI
        // EG 20200828 [XXXXX] Nouvelle interface GUI v10(Mode Noir ou blanc)
        // EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc) Correction et compléments
        public override void Dump_ToGUI(CciPageBase pPage)
        {
            string warningMsg = string.Empty;
            //
            foreach (CustomCaptureInfo cci in this)
            {
                string data = cci.NewValue;
                bool isControlEnabled = cci.IsEnabled;
                Control control = (Control)pPage.FindControl(cci.ClientId);
                //
                if (null != control)
                {
                    switch (cci.ClientId_Prefix)
                    {
                        case Cst.TXT:
                            #region TextBox
                            TextBox txt = (TextBox)control;
                            //
                            if (IsPreserveData)
                                data = cci.NewValue;
                            else
                            {
                                if (StrFunc.IsFilled(data))
                                    data = cci.NewValueFmtToCurrentCulture;
                            }
                            //
                            txt.Enabled = isControlEnabled;
                            txt.Text = data;
                            break;
                            #endregion TextBox
                        case Cst.HSL:
                            #region HTMLDropDownList
                            ControlsTools.DDLSelectByValue((HtmlSelect)control, data);
                            break;
                            #endregion HTMLDropDownList
                        case Cst.DDL:
                            #region DropDownList
                            DropDownList ddl = (DropDownList)control;
                            CustomObjectDropDown coDDL = pPage.GetCciCustomObject(cci.ClientId) as CustomObjectDropDown;
                            //
                            if (!(cci.IsMandatory))
                                ControlsTools.DDLLoad_AddListItemEmptyEmpty(ddl);
                            //
                            ddl.Enabled = isControlEnabled;
                            bool isFound = ControlsTools.DDLSelectByValue(ddl, data);
                            bool isQuoteBasis = CciEvent.IsClientId_QuoteBasis(cci);

                            if ((false == isFound) && (StrFunc.IsFilled(data)))
                            {
                                if (CciEvent.IsCci(CciEvent.CciEnum.payerReceiver_payer_party, cci) ||
                                    CciEvent.IsCci(CciEvent.CciEnum.payerReceiver_receiver_party, cci))
                                {
                                    AddDDLParty(ddl, CciEvent.CciEnum.payerReceiver_payer_party);
                                    AddDDLParty(ddl, CciEvent.CciEnum.payerReceiver_receiver_party);
                                    //isFound = ControlsTools.DDLSelectByValue(ddl, data);
                                }
                                else if (CciEvent.IsCci(CciEvent.CciEnum.payerReceiver_payer_book, cci) ||
                                    CciEvent.IsCci(CciEvent.CciEnum.payerReceiver_receiver_book, cci))
                                {
                                    AddDDLBook(ddl, CciEvent.CciEnum.payerReceiver_payer_book);
                                    AddDDLBook(ddl, CciEvent.CciEnum.payerReceiver_receiver_book);
                                    //isFound = ControlsTools.DDLSelectByValue(ddl, ((SQL_Book)cci.Sql_Table).Id.ToString());
                                }
                                else if (isQuoteBasis)
                                {
                                    #region isQuoteBasis
                                    if (StrFunc.IsFilled(CciEvent.GetBaseCurrency(cci)))
                                        CciTools.DDL_LoadSideRateBasis(coDDL, ddl, CciEvent.GetCurrency1(cci), CciEvent.GetCurrency2(cci));
                                    else
                                        CciTools.DDL_LoadQuoteBasis(coDDL, ddl, CciEvent.GetCurrency1(cci), CciEvent.GetCurrency2(cci));
                                    #endregion isQuoteBasis
                                }
                                else
                                {
                                    #region Others DDL
                                    ddl.Items.Add(new ListItem(data, data));
                                    //isFound = ControlsTools.DDLSelectByValue(ddl, data);
                                    #endregion Others DDL
                                }
                                _ = ControlsTools.DDLSelectByValue(ddl, data);
                            }
                            break;
                            #endregion DropDownList
                        case Cst.CHK:
                        case Cst.HCK:
                            #region CheckBox
                            PropertyInfo pty;
                            //
                            pty = control.GetType().GetProperty("Enabled");
                            pty.SetValue(control, isControlEnabled, null);
                            //
                            pty = control.GetType().GetProperty("Checked");
                            pty.SetValue(control, cci.IsFilledValue, null);
                            break;
                            #endregion CheckBox
                        case Cst.LNK:
                            #region Gestion des hyperlink
                            HyperLink lnk = (HyperLink)control;
                            if (CciEvent.IsCci(CciEvent.CciEnum.idEparent, cci))
                            {
                                string navigateUrl = lnk.NavigateUrl.Replace("{0}", IdMenu.GetIdMenu(IdMenu.Menu.InputEvent));
                                navigateUrl = navigateUrl.Replace("{1}", EventInput.CurrentEvent.idT.ToString());
                                navigateUrl = navigateUrl.Replace("{2}", EventInput.CurrentEvent.idEParent.ToString());
                                lnk.NavigateUrl = navigateUrl;
                            }
                            break;
                            #endregion Gestion des hyperlink
                    }
                    #region Gestion des buttons
                    bool isOk = false;
                    bool isSpecified = false;
                    bool isEnabled = false;
                    WebControl but;
                    if (false == isOk)
                    {
                        #region Is Button ScreenBox
                        CustomObjectButtonScreenBox co = new CustomObjectButtonScreenBox();
                        isOk = CciEvent.SetButtonScreenBox(cci, co, ref isSpecified, ref isEnabled);
                        if (isOk)
                        {
                            but = pPage.FindControl(Cst.BUT + cci.ClientId_WithoutPrefix) as WebControl;
                            if (null != but)
                            {
                                ControlsTools.SetAttributeOnClickOnButtonScreenBox(pPage, co, Cst.BUT + cci.ClientId_WithoutPrefix);
                                but.CssClass = "fa-icon eventcaption";
                                if (isSpecified)
                                    but.CssClass += but.CssClass + " green";
                                but.Enabled = isEnabled;
                                but.Visible = isEnabled | isSpecified;
                            }
                        }
                        #endregion  Is Button ScreenBox
                    }
                    if (false == isOk)
                    {
                        #region IsButtonReferential
                        CustomObjectButtonReferential co = pPage.GetCustomObjectButtonReferential(cci.ClientId_WithoutPrefix);
                        isOk = (null != co);
                        if (isOk)
                        {
                            but = pPage.FindControl(Cst.BUT + cci.ClientId_WithoutPrefix) as WebControl;
                            if (null != but)
                            {
                                CciEvent.SetButtonReferential(cci, co);
                                but.Enabled = isControlEnabled;
                                ControlsTools.SetAttributeOnCLickButtonReferential(co, but);
                            }
                        }
                        #endregion SetButtonReferential
                    }
                    #endregion Gestion des buttons

                    #region Display management
                    control = (Control)pPage.FindControl(Cst.DSP + cci.ClientId_WithoutPrefix);
                    if (null != control)
                    {
                        string msg = string.Empty;

                        System.Drawing.Color color = System.Drawing.Color.Empty;
                        if (cci.HasError)
                        {
                            msg = cci.ErrorMsg;
                            color = System.Drawing.Color.Red;
                        }
                        else if (StrFunc.IsFilled(cci.Display))
                            msg = cci.Display;
                        else if (null != cci.Sql_Table)
                            msg = cci.Sql_Table.FirstRow["DISPLAYNAME"].ToString();

                        if (control is Label lbl)
                        {
                            lbl.Text = msg;
                            lbl.ForeColor = color;
                        }
                    }
                    //}
                    #endregion Display management
                }
            }
            if ((false == Cst.Capture.IsModeMatch(CaptureMode)) && (pPage.FocusMode == CciPageBase.FocusModeEnum.Forced))
                SetFocus(pPage);
            //
            if (StrFunc.IsFilled(warningMsg))
                JavaScript.DialogStartUpImmediate(pPage, warningMsg);
            //
            CciEvent.DumpSpecific_ToGUI(pPage);
        }
        #endregion Dump_ToGUI
        #region InitializeCciContainer
        /// <summary>
        /// Synchronize les différents pointeurs du dataDocument existants dans les cciContainers  
        /// </summary>
        public override void InitializeCciContainer()
        {
            CciEvent = new CciEvent(this, this.EventInput.CurrentEvent, CCst.Prefix_event);
        }

        #endregion InitializeCciContainer

        #endregion Methods

        #region ICloneable Members
        public object Clone()
        {
            return Clone(CustomCaptureInfo.CloneMode.CciAll);
        }
        // EG 20180425 Analyse du code Correction [CA2235]
        public object Clone(CustomCaptureInfo.CloneMode pCloneMode)
        {
            EventCustomCaptureInfos clone = new EventCustomCaptureInfos(this.CS, EventInput, this.User, this.SessionId, IsGetDefaultOnInitializeCci);
            clone.InitializeCciContainer();
            foreach (CustomCaptureInfo cci in this)
            {
                clone.Add((CustomCaptureInfo)cci.Clone(pCloneMode));
            }
            return clone;
        }
        #endregion ICloneable Members
    }
    
}
