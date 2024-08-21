#region Using Directives
using System;
using System.Collections;
using System.Data;
using System.Globalization;
using System.Configuration;
using System.Reflection;
using System.Web.UI;
using System.Web.UI.WebControls;


using EFS.ACommon;
using EFS.ApplicationBlocks.Data;
using EFS.Common;
using EFS.Common.Web; 

using EFS.GUI;
using EFS.GUI.CCI; 
using EFS.GUI.Interface;



using EfsML;
using EfsML.Business;

using FpML.Enum;
#endregion Using Directives

namespace EFS.TradeInformation
{
    #region CciEvent
    public class CciEvent : ITradeCci, IContainerCciFactory, IContainerCciPayerReceiver, IContainerCci, IContainerCciQuoteBasis, IContainerCciGetInfoButton
    {
        #region Enums
        #region CciEnum
        public enum CciEnum
        {
            trade_identifier,
            trade_displayname,
            trade_description,

            

            idEparent,
            instrument_displayName,
            instrument_family,
            instrument_instrumentNo,
            instrument_streamNo,

            codes_eventCode,
            codes_eventType,
            dates_startDate,
            dates_endDate,

            valorisationAmounts_valorisationAmount_unit,
            valorisationAmounts_valorisationAmountSys_unitSys,
            
            
            valorisationAmounts_valorisationAmount_amount,
            valorisationAmounts_valorisationAmountSys_amountSys,

            valorisationRates_valorisationRate_rate,
            valorisationRates_valorisationRateSys_rateSys,

            valorisationFxRates_valorisationFxRate_rate,
            valorisationFxRates_valorisationFxRateSys_rateSys,

            trigger_rate,
            trigger_status,
            trigger_rateSys,

            payerReceiver_payer_party,
            payerReceiver_payer_book,
            payerReceiver_receiver_party,
            payerReceiver_receiver_book,

            divers_idStCalcul,
            
            divers_source,
            divers_extlLink,

            #region screen DialogBox
            eventDet_dayCountFraction_screen,
            eventDet_exchangeRate_screen,
            eventDet_exchangeRatePremium_screen,
            eventDet_sideRate_screen,
            eventDet_strikePrice_screen,
            eventDet_fixingRate_screen,
            eventDet_settlementRate_screen,
            eventDet_currencyPair_screen,
            eventDet_triggerRate_screen,
            eventDet_fixedRate_screen,
            eventDet_capFloorSchedule_screen,
            eventDet_premiumQuote_screen,
            eventDet_paymentQuote_screen,

            eventDet_pricingFx_screen,
            eventDet_pricingFxOption_screen,

            eventDet_assetFxRate_screen,
            eventDet_assetRateIndex_screen,

            eventDet_asset_screen,
            eventDet_process_screen,
            eventDet_pricing2_screen,

            eventDet_eventChild_screen,
            eventDet_notes_screen,
            #endregion screen DialogBox
            unknown,
        }
        #endregion CciEnum
        #endregion Enum
        #region Members
        public EventCustomCaptureInfos ccis;
        private readonly Event _event;
        private CciEvent cciEventParent;
        public CciEventDet cciEventDetail;
        public CciEventProcess[] cciEventProcess;
        public CciEventAsset[] cciEventAsset;
        public CciEventClass[] cciEventClass;
        public CciEventItem[] cciEventItem;
        public CciEventPricing2[] cciEventPricing2;
        private readonly Hashtable _currencyInfos;
        private readonly string prefix;
        #endregion Members
        #region Accessors
        #region CS
        public string CS
        {
            get { return ccis.CS; }
        }
        #endregion CS
        #region CurrentEventDocReader
        public Events CurrentEventDocReader
        {
            get { return ccis.EventInput.EventDocReader; }
        }
        #endregion CurrentEventDocReader
        #region EventAssetLenght
        public int EventAssetLenght
        {
            get { return ArrFunc.IsFilled(cciEventAsset) ? cciEventAsset.Length : 0; }
        }
        #endregion EventAssetLenght
        #region EventClassLenght
        public int EventClassLenght
        {
            get { return ArrFunc.IsFilled(cciEventClass) ? cciEventClass.Length : 0; }
        }
        #endregion EventClassLenght
        #region EventProcessLenght
        public int EventProcessLenght
        {
            get { return ArrFunc.IsFilled(cciEventProcess) ? cciEventProcess.Length : 0; }
        }
        #endregion EventProcessLenght
        #region EventPricingLenght
        public int EventPricingLenght
        {
            get { return ArrFunc.IsFilled(cciEventPricing2) ? cciEventPricing2.Length : 0; }
        }
        #endregion EventPricingLenght

        #endregion Accessors
        #region Constructor
        public CciEvent(EventCustomCaptureInfos pCcis, Event pEvent, string pPrefix)
        {
            ccis = pCcis;
            _currencyInfos = new Hashtable();
            prefix = pPrefix + CustomObject.KEY_SEPARATOR;
            _event = pEvent;
        }
        #endregion Constructors
        #region Methods
        #region InitializeEventAsset_FromCci
        private void InitializeEventAsset_FromCci()
        {
            bool isOk = true;
            int index = -1;
            ArrayList aEventAsset = new ArrayList();
            //int maxEventAsset     = 0;
            //if (ArrFunc.IsFilled(_event.asset))
            //	maxEventAsset     = _event.asset.Length;

            while (isOk)
            {
                index += 1;
                CciEventAsset cciEventAsset = new CciEventAsset(this, index + 1, null, prefix + EventCustomCaptureInfos.CCst.Prefix_eventAsset);
                isOk = ccis.Contains(cciEventAsset.CciClientId(CciEventAsset.CciEnum.idAsset));
                if (isOk)
                {
                    if (ArrFunc.IsEmpty(_event.asset) || (index == _event.asset.Length))
                        ReflectionTools.AddItemInArray(_event, "asset", index);
                    cciEventAsset.EventAsset = _event.asset[index];
                    aEventAsset.Add(cciEventAsset);
                }
            }
            this.cciEventAsset = (CciEventAsset[])aEventAsset.ToArray(typeof(CciEventAsset));
            for (int i = 0; i < this.cciEventAsset.Length; i++)
                this.cciEventAsset[i].Initialize_FromCci();
        }
        #endregion InitializeEventAsset_FromCci
        #region InitializeEventClass_FromCci
        private void InitializeEventClass_FromCci()
        {
            bool isOk = true;
            int index = -1;
            ArrayList aEventClass = new ArrayList();
            //int maxEventClass     = 0;
            //if (ArrFunc.IsFilled(_event.eventClass))
            //	maxEventClass     = _event.eventClass.Length;

            while (isOk)
            {
                index += 1;
                CciEventClass cciEventClass = new CciEventClass(this, index + 1, null, prefix + EventCustomCaptureInfos.CCst.Prefix_eventClass);
                isOk = ccis.Contains(cciEventClass.CciClientId(CciEventClass.CciEnum.eventClass));
                if (isOk)
                {
                    if (ArrFunc.IsEmpty(_event.eventClass) || (index == _event.eventClass.Length))
                        ReflectionTools.AddItemInArray(_event, "eventClass", index);
                    cciEventClass.EventClass = _event.eventClass[index];
                    aEventClass.Add(cciEventClass);
                }
            }
            this.cciEventClass = (CciEventClass[])aEventClass.ToArray(typeof(CciEventClass));
            for (int i = 0; i < this.cciEventClass.Length; i++)
                this.cciEventClass[i].Initialize_FromCci();
        }
        #endregion InitializeEventClass_FromCci
        #region InitializeEventChild_FromCci
        private void InitializeEventChild_FromCci()
        {
            bool isOk = true;
            int index = -1;
            ArrayList aEventChild = new ArrayList();
            //int maxEventChild     = 0;
            Event[] eventChild = ccis.EventInput.CurrentEventChilds;
            //if (ArrFunc.IsFilled(eventChild))
            //	maxEventChild     = eventChild.Length;

            if (null != eventChild)
            {
                while (isOk)
                {
                    index += 1;
                    CciEventItem cciEventItem = new CciEventItem(this, index + 1, null, prefix + EventCustomCaptureInfos.CCst.Prefix_eventChild);
                    isOk = ccis.Contains(cciEventItem.CciClientId(CciEventItem.CciEnum.eventCode));
                    if (isOk)
                    {
                        cciEventItem.EventItem = eventChild[index];
                        aEventChild.Add(cciEventItem);
                    }
                }
                this.cciEventItem = (CciEventItem[])aEventChild.ToArray(typeof(CciEventItem));
                for (int i = 0; i < this.cciEventItem.Length; i++)
                    this.cciEventItem[i].Initialize_FromCci();
            }
        }
        #endregion InitializeEventChild_FromCci
        #region InitializeEventProcess_FromCci
        private void InitializeEventProcess_FromCci()
        {
            bool isOk = true;
            int index = -1;
            ArrayList aEventProcess = new ArrayList();
            //int maxEventProcess     = 0;
            //if (ArrFunc.IsFilled(_event.process))
            //	maxEventProcess     = _event.process.Length;

            while (isOk)
            {
                index += 1;
                CciEventProcess cciEventProcess = new CciEventProcess(this, index + 1, null, prefix + EventCustomCaptureInfos.CCst.Prefix_eventProcess);
                isOk = ccis.Contains(cciEventProcess.CciClientId(CciEventProcess.CciEnum.process));
                if (isOk)
                {
                    if (ArrFunc.IsEmpty(_event.process) || (index == _event.process.Length))
                        ReflectionTools.AddItemInArray(_event, "process", index);
                    cciEventProcess.EventProcess = _event.process[index];
                    aEventProcess.Add(cciEventProcess);
                }
            }
            this.cciEventProcess = (CciEventProcess[])aEventProcess.ToArray(typeof(CciEventProcess));
            for (int i = 0; i < this.cciEventProcess.Length; i++)
                this.cciEventProcess[i].Initialize_FromCci();
        }
        #endregion InitializeEventProcess_FromCci
        #region InitializeEventDet_FromCci
        private void InitializeEventDet_FromCci()
        {
            if (false == ccis.EventInput.CurrentEvent.detailsSpecified)
                ccis.EventInput.CurrentEvent.details = new EventDetails();
            if (false == ccis.EventInput.CurrentEvent.pricingSpecified)
                ccis.EventInput.CurrentEvent.pricing = new EventPricing();
            this.cciEventDetail = new CciEventDet(this, ccis.EventInput.CurrentEvent.details, ccis.EventInput.CurrentEvent.pricing,
                prefix + EventCustomCaptureInfos.CCst.Prefix_eventDet);
            this.cciEventDetail.Initialize_FromCci();
        }
        #endregion InitializeEventDet_FromCci
        #region InitializeEventParent_FromCci
        private void InitializeEventParent_FromCci()
        {
            Event eventParent = ccis.EventInput.CurrentEventParent;
            if (null != eventParent)
            {
                this.cciEventParent = new CciEvent(ccis, eventParent, prefix + EventCustomCaptureInfos.CCst.Prefix_eventParent);
                this.cciEventParent.Initialize_FromCci();
            }
        }
        #endregion InitializeEventParent_FromCci
        #region InitializeEventPricing2_FromCci
        private void InitializeEventPricing2_FromCci()
        {
            bool isOk = true;
            int index = -1;
            ArrayList aEventPricing2 = new ArrayList();
            while (isOk)
            {
                index += 1;
                CciEventPricing2 cciEventPricing2 = new CciEventPricing2(this, index + 1, null, prefix + EventCustomCaptureInfos.CCst.Prefix_eventDet_pricingIRD);
                isOk = ccis.Contains(cciEventPricing2.CciClientId(CciEventPricing2.CciEnum.idYieldCurveDef));
                if (isOk)
                {
                    if (ArrFunc.IsEmpty(_event.pricing2) || (index == _event.pricing2.Length))
                        ReflectionTools.AddItemInArray(_event, "pricing2", index);
                    cciEventPricing2.EventPricing2 = _event.pricing2[index];
                    aEventPricing2.Add(cciEventPricing2);
                }
            }
            this.cciEventPricing2 = (CciEventPricing2[])aEventPricing2.ToArray(typeof(CciEventPricing2));
            for (int i = 0; i < this.cciEventPricing2.Length; i++)
                this.cciEventPricing2[i].Initialize_FromCci();
        }
        #endregion InitializeEventPricing2_FromCci
        #region FormatAmountByCurrency
        private string FormatAmountByCurrency(decimal pAmount, string pCurrency)
        {
            string amount = StrFunc.FmtDecimalToInvariantCulture(pAmount);
            CurrencyCashInfo currencyInfo;
            if (StrFunc.IsFilled(pCurrency) && false == _currencyInfos.ContainsKey(pCurrency))
            {
                currencyInfo = new CurrencyCashInfo(SessionTools.CS, pCurrency);
                if (null != currencyInfo)
                    _currencyInfos.Add(pCurrency, currencyInfo);
            }
            if (StrFunc.IsFilled(pCurrency) && _currencyInfos.ContainsKey(pCurrency))
            {
                currencyInfo = (CurrencyCashInfo)_currencyInfos[pCurrency];
                amount = StrFunc.FmtAmountToInvariantCulture(StrFunc.FmtDecimalToCurrentCulture(pAmount, currencyInfo.RoundPrec));
            }
            return amount;
        }
        #endregion FormatAmountByCurrency
        #region DumpSpecific_ToGUI
        public virtual void DumpSpecific_ToGUI(CciPageBase pPage)
        {
            if (ArrFunc.IsFilled(cciEventPricing2))
            {
                foreach (CciEventPricing2 item in cciEventPricing2)
                {
                    item.DumpSpecific_ToGUI(pPage);
                }
            }
            // FI 20200820 [25468] call DumpSpecific_ToGUI
            if (ArrFunc.IsFilled(cciEventProcess))
            {
                foreach (CciEventProcess item in cciEventProcess)
                {
                    item.DumpSpecific_ToGUI(pPage);
                }
            }
        }
        #endregion DumpSpecific_ToGUI

        #endregion Methods
        #region Interface Methods
        #region IContainerCciFactory members
        #region AddCciSystem
        /// <summary>
        /// 
        /// </summary>
        /// FI 20170116  [21916] Modify (use AddCciSystem Method)
        public void AddCciSystem()
        {
            #region EventAsset
            if (null != cciEventAsset)
            {
                CciTools.AddCciSystem(ccis, Cst.BUT + CciClientId(CciEnum.eventDet_asset_screen), false, TypeData.TypeDataEnum.@string);

                if (Cst.ProductFamily_FX == _event.family)
                    CciTools.AddCciSystem(ccis, Cst.BUT + CciClientId(CciEnum.eventDet_assetFxRate_screen), false, TypeData.TypeDataEnum.@string);

                else if (false == ccis.Contains(CciClientId(CciEnum.eventDet_assetRateIndex_screen.ToString())))
                    CciTools.AddCciSystem(ccis, Cst.BUT + CciClientId(CciEnum.eventDet_assetRateIndex_screen), false, TypeData.TypeDataEnum.@string);
            }
            #endregion EventAsset
            #region EventDetail
            if (null != cciEventDetail)
            {
                CciTools.AddCciSystem(ccis, Cst.BUT + CciClientId(CciEnum.eventDet_dayCountFraction_screen), false, TypeData.TypeDataEnum.@string);
                CciTools.AddCciSystem(ccis, Cst.BUT + CciClientId(CciEnum.eventDet_exchangeRate_screen), false, TypeData.TypeDataEnum.@string);
                CciTools.AddCciSystem(ccis, Cst.BUT + CciClientId(CciEnum.eventDet_exchangeRatePremium_screen), false, TypeData.TypeDataEnum.@string);
                CciTools.AddCciSystem(ccis, Cst.BUT + CciClientId(CciEnum.eventDet_sideRate_screen), false, TypeData.TypeDataEnum.@string);
                CciTools.AddCciSystem(ccis, Cst.BUT + CciClientId(CciEnum.eventDet_strikePrice_screen), false, TypeData.TypeDataEnum.@string);
                CciTools.AddCciSystem(ccis, Cst.BUT + CciClientId(CciEnum.eventDet_fixingRate_screen), false, TypeData.TypeDataEnum.@string);
                CciTools.AddCciSystem(ccis, Cst.BUT + CciClientId(CciEnum.eventDet_settlementRate_screen), false, TypeData.TypeDataEnum.@string);
                CciTools.AddCciSystem(ccis, Cst.BUT + CciClientId(CciEnum.eventDet_currencyPair_screen), false, TypeData.TypeDataEnum.@string);
                CciTools.AddCciSystem(ccis, Cst.BUT + CciClientId(CciEnum.eventDet_triggerRate_screen), false, TypeData.TypeDataEnum.@string);
                CciTools.AddCciSystem(ccis, Cst.BUT + CciClientId(CciEnum.eventDet_fixedRate_screen), false, TypeData.TypeDataEnum.@string);
                CciTools.AddCciSystem(ccis, Cst.BUT + CciClientId(CciEnum.eventDet_capFloorSchedule_screen), false, TypeData.TypeDataEnum.@string);
                CciTools.AddCciSystem(ccis, Cst.BUT + CciClientId(CciEnum.eventDet_paymentQuote_screen), false, TypeData.TypeDataEnum.@string);
                CciTools.AddCciSystem(ccis, Cst.BUT + CciClientId(CciEnum.eventDet_premiumQuote_screen), false, TypeData.TypeDataEnum.@string);
                CciTools.AddCciSystem(ccis, Cst.BUT + CciClientId(CciEnum.eventDet_pricingFxOption_screen), false, TypeData.TypeDataEnum.@string);
                CciTools.AddCciSystem(ccis, Cst.BUT + CciClientId(CciEnum.eventDet_pricingFx_screen), false, TypeData.TypeDataEnum.@string);
                CciTools.AddCciSystem(ccis, Cst.BUT + CciClientId(CciEnum.eventDet_notes_screen), false, TypeData.TypeDataEnum.@string);
            }
            #endregion EventDetail
            #region EventProcess
            if (null != cciEventProcess)
                CciTools.AddCciSystem(ccis, Cst.BUT + CciClientId(CciEnum.eventDet_process_screen), false, TypeData.TypeDataEnum.@string);
            #endregion EventProcess
            #region EventItem
            if (null != cciEventItem)
                CciTools.AddCciSystem(ccis, Cst.BUT + CciClientId(CciEnum.eventDet_eventChild_screen), false, TypeData.TypeDataEnum.@string);
            #endregion EventItem
        }
        #endregion AddCciSystem
        #region Initialize_FromCci
        public void Initialize_FromCci()
        {
            if (2 == ArrFunc.Count(prefix.Split(CustomObject.KEY_SEPARATOR)))
            {
                InitializeEventParent_FromCci();
                InitializeEventAsset_FromCci();
                InitializeEventClass_FromCci();
                InitializeEventProcess_FromCci();
                InitializeEventPricing2_FromCci();
                InitializeEventDet_FromCci();
                InitializeEventChild_FromCci();
            }
        }
        #endregion Initialize_FromCci
        #region Initialize_FromDocument
        public void Initialize_FromDocument()
        {
            Type tCciEnum = typeof(CciEnum);
            foreach (string enumName in Enum.GetNames(tCciEnum))
            {
                CustomCaptureInfo cci = ccis[prefix + enumName];
                if (cci != null)
                {
                    #region Reset variables
                    string data = string.Empty;
                    string display = string.Empty;
                    bool isSetting = true;
                    SQL_Table sql_Table = null;
                    #endregion Reset variables
                    //
                    CciEnum keyEnum = (CciEnum)System.Enum.Parse(typeof(CciEnum), enumName);
                    switch (keyEnum)
                    {
                        #region Trade
                        case CciEnum.trade_identifier:
                            #region Identifier
                            data = ccis.EventInput.TradeIdentifier;
                            break;
                            #endregion Identifier
                        case CciEnum.trade_displayname:
                            #region DisplayName
                            data = ccis.EventInput.SQLTrade.DisplayName;
                            break;
                            #endregion DisplayName
                        case CciEnum.trade_description:
                            #region Description
                            data = ccis.EventInput.SQLTrade.Description;
                            break;
                            #endregion Description
                        #endregion Trade

                        case CciEnum.instrument_instrumentNo:
                            #region Instrument N°
                            data = _event.instrumentNo;
                            break;
                            #endregion Instrument N°
                        case CciEnum.instrument_streamNo:
                            #region Stream N°
                            data = _event.streamNo;
                            break;
                            #endregion Stream N°
                        case CciEnum.codes_eventCode:
                            #region EventCode
                            data = _event.eventCode;
                            break;
                            #endregion EventCodes
                        case CciEnum.codes_eventType:
                            #region EventType
                            data = _event.eventType;
                            break;
                            #endregion EventType
                        case CciEnum.dates_startDate:
                            #region StartDate
                            data = _event.dtStartPeriod.Value;
                            break;
                            #endregion StartDate
                        case CciEnum.dates_endDate:
                            #region EndDate
                            data = _event.dtEndPeriod.Value;
                            break;
                            #endregion EndDate
                        case CciEnum.valorisationFxRates_valorisationFxRate_rate:
                        case CciEnum.valorisationRates_valorisationRate_rate:
                        case CciEnum.trigger_rate:
                            #region Valorisation Rate
                            if (_event.valorisationSpecified)
                                data = _event.valorisation.Value;
                            break;
                            #endregion Valorisation Rate
                        case CciEnum.valorisationAmounts_valorisationAmount_amount:
                            #region Valorisation Amount
                            if (_event.valorisationSpecified)
                                data = FormatAmountByCurrency(_event.valorisation.DecValue, _event.unit);
                            break;
                            #endregion Valorisation Amount
                        case CciEnum.valorisationAmounts_valorisationAmount_unit:
                            #region Unit
                            if (_event.unitSpecified)
                                data = _event.unit;
                            break;
                            #endregion Unit
                        case CciEnum.valorisationFxRates_valorisationFxRateSys_rateSys:
                        case CciEnum.valorisationRates_valorisationRateSys_rateSys:
                        case CciEnum.trigger_rateSys:
                            #region Valorisation Rate System
                            if (_event.valorisationSysSpecified)
                                data = _event.valorisationSys.Value;
                            break;
                            #endregion Valorisation Rate System
                        case CciEnum.valorisationAmounts_valorisationAmountSys_amountSys:
                            #region Valorisation system
                            if (_event.valorisationSysSpecified)
                                data = FormatAmountByCurrency(_event.valorisationSys.DecValue, _event.unitSys);
                            break;
                            #endregion Valorisation system
                        case CciEnum.valorisationAmounts_valorisationAmountSys_unitSys:
                            #region Unit system
                            if (_event.unitSysSpecified)
                                data = _event.unitSys;
                            break;
                            #endregion Unit system
                        case CciEnum.payerReceiver_payer_party:
                            #region Payer
                            if (_event.idPayerSpecified)
                            {
                                sql_Table = new SQL_Actor(CS, _event.idPayer);
                                if (sql_Table.IsLoaded)
                                    data = ((SQL_Actor)sql_Table).Identifier;
                            }
                            break;
                            #endregion Payer
                        case CciEnum.payerReceiver_receiver_party:
                            #region Receiver
                            if (_event.idReceiverSpecified)
                            {
                                sql_Table = new SQL_Actor(CS, _event.idReceiver);
                                if (sql_Table.IsLoaded)
                                    data = ((SQL_Actor)sql_Table).Identifier;
                            }
                            break;
                            #endregion Receiver
                        case CciEnum.payerReceiver_payer_book:
                            #region BookPayer
                            if (_event.idPayerBookSpecified)
                            {
                                sql_Table = new SQL_Book(CS, _event.idPayerBook);
                                if (sql_Table.IsLoaded)
                                    data = ((SQL_Book)sql_Table).Identifier;
                            }
                            break;
                            #endregion BookPayer
                        case CciEnum.payerReceiver_receiver_book:
                            #region BookReceiver
                            if (_event.idReceiverBookSpecified)
                            {
                                sql_Table = new SQL_Book(CS, _event.idReceiverBook);
                                if (sql_Table.IsLoaded)
                                    data = ((SQL_Book)sql_Table).Identifier;
                            }
                            break;
                            #endregion BookReceiver
                        case CciEnum.instrument_displayName:
                            #region Instrument DisplayName
                            data = _event.displayNameInstrSpecified ? _event.displayNameInstr : Cst.NotAvailable;
                            break;
                            #endregion Instrument Identifier
                        case CciEnum.instrument_family:
                            #region Product Family
                            data = _event.displayNameInstrSpecified ? _event.family : Cst.NotAvailable;
                            break;
                            #endregion Product Family
                        case CciEnum.trigger_status:
                            #region IdStTrigger
                            if (_event.idStTriggerSpecified)
                                data = _event.idStTrigger;
                            break;
                            #endregion IdStTrigger
                        case CciEnum.divers_idStCalcul:
                            #region IdStCalcul
                            data = _event.idStCalcul;
                            break;
                            #endregion IdStCalcul
                        
                        case CciEnum.divers_extlLink:
                            #region ExtlLink
                            if (_event.extlLinkSpecified)
                                data = _event.extlLink;
                            break;
                            #endregion ExtlLink
                        case CciEnum.divers_source:
                            #region Source
                            if (_event.sourceSpecified)
                                data = _event.source;
                            break;
                            #endregion Source
                        default:
                            isSetting = false;
                            break;
                    }
                    if (isSetting)
                        ccis.InitializeCci(cci, sql_Table, data, display);
                }
            }
            #region EventParent
            if (null != cciEventParent)
                cciEventParent.Initialize_FromDocument();
            #endregion EventParent
            #region EventAsset
            if (ArrFunc.IsFilled(cciEventAsset))
            {
                foreach (CciEventAsset cci in cciEventAsset)
                    cci.Initialize_FromDocument();
            }
            #endregion EventAsset
            #region EventProcess
            if (ArrFunc.IsFilled(cciEventProcess))
            {
                foreach (CciEventProcess cci in cciEventProcess)
                    cci.Initialize_FromDocument();
            }
            #endregion EventProcess
            #region EventPricing2
            if (ArrFunc.IsFilled(cciEventPricing2))
            {
                foreach (CciEventPricing2 cci in cciEventPricing2)
                    cci.Initialize_FromDocument();
            }
            #endregion EventPricing2
            #region EventClass
            if (ArrFunc.IsFilled(cciEventClass))
            {
                foreach (CciEventClass cci in cciEventClass)
                    cci.Initialize_FromDocument();
            }
            #endregion EventClass
            #region EventDetail
            if (null != cciEventDetail)
                cciEventDetail.Initialize_FromDocument();
            #endregion EventDetail
            #region EventChildren
            if (ArrFunc.IsFilled(cciEventItem))
            {
                foreach (CciEventItem cci in cciEventItem)
                    cci.Initialize_FromDocument();
            }
            #endregion EventChildren

        }
        #endregion Initialize_FromDocument
        #region Dump_ToDocument
        /// <summary>
        /// 
        /// </summary>
        /// FI 20161123 [22629] Modify
        public void Dump_ToDocument()
        {
            Type tCciEnum = typeof(CciEnum);
            foreach (string enumName in Enum.GetNames(tCciEnum))
            {
                CustomCaptureInfo cci = ccis[prefix + enumName];
                if ((cci != null) && (cci.HasChanged))
                {
                    #region Reset variables
                    string  data = cci.NewValue;
                    Boolean  isSetting = true;
                    CustomCaptureInfosBase.ProcessQueueEnum processQueue = CustomCaptureInfosBase.ProcessQueueEnum.None;
                    #endregion
                    //
                    CciEnum keyEnum = (CciEnum)System.Enum.Parse(typeof(CciEnum), enumName);
                    switch (keyEnum)
                    {
                        #region Codes/Periods
                        case  CciEnum.codes_eventCode:
                            _event.eventCode = data;
                            break;
                        case CciEnum.codes_eventType:
                            _event.eventType = data;
                            break;
                        case CciEnum.dates_startDate:
                            _event.dtStartPeriod.Value = data;
                            break;
                        case CciEnum.dates_endDate:
                            _event.dtEndPeriod.Value = data;
                            break;
                        #endregion Codes/Periods
                        #region Valorisation
                        case CciEnum.valorisationRates_valorisationRate_rate:
                        case CciEnum.valorisationFxRates_valorisationFxRate_rate:
                        case CciEnum.valorisationAmounts_valorisationAmount_amount:
                            _event.valorisationSpecified = StrFunc.IsFilled(data);
                            if (null == _event.valorisation)
                                _event.valorisation = new EFS_Decimal(data);
                            else
                                _event.valorisation.Value = data;
                            break;
                        case CciEnum.valorisationAmounts_valorisationAmount_unit:
                            _event.unitSpecified = StrFunc.IsFilled(data);
                            _event.unit = data;


                            break;
                        #endregion Valorisation
                        #region Payer/Receiver
                        case CciEnum.payerReceiver_payer_party:
                            #region idPayer
                            _event.idPayer = 0;
                            if (StrFunc.IsFilled(data))
                            {
                                cci.Sql_Table = new SQL_Actor(CS, data);
                                if (cci.Sql_Table.IsLoaded)
                                    _event.idPayer = ((SQL_Actor)cci.Sql_Table).Id;
                            }
                            _event.idPayerSpecified = (0 != _event.idPayer);
                            processQueue = CustomCaptureInfosBase.ProcessQueueEnum.Low;
                            #endregion idPayer
                            break;
                        case CciEnum.payerReceiver_payer_book:
                            #region idPayerBook
                            _event.idPayerBook = 0;
                            if (StrFunc.IsFilled(data))
                            {
                                cci.Sql_Table = new SQL_Book(CS, SQL_TableWithID.IDType.Identifier, data);
                                if (cci.Sql_Table.IsLoaded)
                                    _event.idPayerBook = ((SQL_Book)cci.Sql_Table).Id;
                            }
                            _event.idPayerBookSpecified = (0 != _event.idPayerBook);
                            #endregion idPayerBook
                            break;
                        case CciEnum.payerReceiver_receiver_party:
                            #region idReceiver
                            _event.idReceiver = 0;
                            if (StrFunc.IsFilled(data))
                            {
                                cci.Sql_Table = new SQL_Actor(CS, data);
                                if (cci.Sql_Table.IsLoaded)
                                    _event.idReceiver = ((SQL_Actor)cci.Sql_Table).Id;
                            }
                            _event.idReceiverSpecified = (0 != _event.idPayer);
                            processQueue = CustomCaptureInfosBase.ProcessQueueEnum.Low;
                            #endregion idReceiver
                            break;
                        case CciEnum.payerReceiver_receiver_book:
                            #region idReceiverBook
                            _event.idReceiverBook = 0;
                            if (StrFunc.IsFilled(data))
                            {
                                cci.Sql_Table = new SQL_Book(CS, SQL_TableWithID.IDType.Identifier, data);
                                if (cci.Sql_Table.IsLoaded)
                                    _event.idReceiverBook = ((SQL_Book)cci.Sql_Table).Id;
                            }
                            _event.idReceiverBookSpecified = (0 != _event.idPayerBook);
                            #endregion idReceiverBook
                            break;
                        #endregion Payer/Receiver
                        #region Divers
                        case CciEnum.divers_idStCalcul:
                            _event.idStCalculSpecified = StrFunc.IsFilled(data);
                            _event.idStCalcul = data;
                            break;
                        case CciEnum.divers_source:
                            _event.sourceSpecified = StrFunc.IsFilled(data);
                            _event.source = data;
                            break;
                        #endregion Divers

                        default:
                            isSetting = false;
                            break;
                    }
                    if (isSetting)
                        ccis.Finalize(cci.ClientId_WithoutPrefix, processQueue);
                }
            }

            for (int i = 0; i < EventProcessLenght; i++)
                cciEventProcess[i].Dump_ToDocument();

            for (int i = 0; i < EventPricingLenght; i++)
                cciEventPricing2[i].Dump_ToDocument();

            for (int i = 0; i < EventAssetLenght; i++)
                cciEventAsset[i].Dump_ToDocument();

            for (int i = 0; i < EventClassLenght; i++)
                cciEventClass[i].Dump_ToDocument();

            if (null != cciEventDetail)
            {
                cciEventDetail.Dump_ToDocument();
                _event.detailsSpecified = cciEventDetail.IsSpecified;
            }


        }
        #endregion Dump_ToDocument
        #region ProcessExecute
        public void ProcessExecute(CustomCaptureInfo pCci)
        {

        }
        #endregion ProcessExecute
        #region ProcessExecuteAfterSynchronize
        // EG 20091207 New
        public void ProcessExecuteAfterSynchronize(CustomCaptureInfo pCci)
        {

        }
        #endregion ProcessExecuteAfterSynchronize
        
        #region ProcessInitialize
        public void ProcessInitialize(CustomCaptureInfo pCci)
        {
            for (int i = 0; i < EventAssetLenght; i++)
                cciEventAsset[i].ProcessInitialize(pCci);

            if (IsCciOfContainer(pCci.ClientId_WithoutPrefix))
            {
                string clientId_Element = CciContainerKey(pCci.ClientId_WithoutPrefix);
                CciEnum key = CciEnum.unknown;
                if (System.Enum.IsDefined(typeof(CciEnum), clientId_Element))
                    key = (CciEnum)System.Enum.Parse(typeof(CciEnum), clientId_Element);
                switch (key)
                {
                    case CciEnum.payerReceiver_payer_party:
                        #region Payer
                        ccis.Synchronize(CciClientIdReceiver, pCci.NewValue, pCci.LastValue);
                        SwapBookPayerReceiver();
                        break;
                        #endregion Payer
                    case CciEnum.payerReceiver_receiver_party:
                        #region Receiver
                        ccis.Synchronize(CciClientIdPayer, pCci.NewValue, pCci.LastValue);
                        break;
                        #endregion Receiver
                    default:
                        break;
                }
            }
            ccis.Finalize(pCci.ClientId_WithoutPrefix, CustomCaptureInfosBase.ProcessQueueEnum.None);
        }
        #endregion ProcessInitialize
        #region IsClientId_PayerOrReceiver
        public bool IsClientId_PayerOrReceiver(CustomCaptureInfo pCci)
        {
            bool isOk = false;
            return isOk;
        }
        #endregion IsClientId_PayerOrReceiver
        #region CleanUp
        public void CleanUp()
        {
            #region EventAsset
            if (_event.assetSpecified)
            {
                for (int i = _event.asset.Length - 1; -1 < i; i--)
                {
                    bool isRemove = (false == CaptureToolsBase.IsDocumentElementValid(_event.asset[i].idAsset.ToString()));
                    if (isRemove)
                        ReflectionTools.RemoveItemInArray(_event, "asset", i);
                }
            }
            _event.assetSpecified = ArrFunc.IsFilled(_event.asset);
            #endregion EventAsset
            #region EventClass
            if (_event.eventClassSpecified)
            {
                for (int i = _event.eventClass.Length - 1; -1 < i; i--)
                {
                    bool isRemove = (false == CaptureToolsBase.IsDocumentElementValid(_event.eventClass[i].code));
                    if (false == isRemove)
                        isRemove = (false == CaptureToolsBase.IsDocumentElementValid(_event.eventClass[i].dtEvent));
                    if (isRemove)
                        ReflectionTools.RemoveItemInArray(_event, "eventClass", i);
                }
            }
            _event.eventClassSpecified = ArrFunc.IsFilled(_event.eventClass);
            #endregion EventClass
            #region EventProcess
            if (_event.processSpecified)
            {
                for (int i = _event.process.Length - 1; -1 < i; i--)
                {
                    bool isRemove = (false == CaptureToolsBase.IsDocumentElementValid(_event.process[i].process));
                    if (false == isRemove)
                        isRemove = (false == CaptureToolsBase.IsDocumentElementValid(_event.process[i].idStProcess));
                    if (isRemove)
                        ReflectionTools.RemoveItemInArray(_event, "process", i);
                }
            }
            _event.processSpecified = ArrFunc.IsFilled(_event.process);
            #endregion EventProcess
            #region EventPricing2
            if (_event.pricing2Specified)
            {
                for (int i = _event.pricing2.Length - 1; -1 < i; i--)
                {
                    bool isRemove = (false == CaptureToolsBase.IsDocumentElementValid(_event.pricing2[i].idYieldCurveDef));
                    if (isRemove)
                        ReflectionTools.RemoveItemInArray(_event, "pricing2", i);
                }
            }
            _event.processSpecified = ArrFunc.IsFilled(_event.process);
            #endregion EventPricing2

        }
        #endregion CleanUp
        #region SetDisplay
        public void SetDisplay(CustomCaptureInfo pCci)
        {

        }
        #endregion
        #region RefreshCciEnabled
        public void RefreshCciEnabled()
        {
            
        }
        #endregion
        #region RemoveLastItemInArray
        public void RemoveLastItemInArray(string _)
        {
        }
        #endregion RemoveLastItemInArray
        #region Initialize_Document
        public void Initialize_Document()
        {
        }
        #endregion Initialize_Document
        #endregion IContainerCciFactory members
        #region IContainerCciPayerReceiver Members
        virtual public string CciClientIdPayer
        {
            get { return CciClientId(CciEnum.payerReceiver_payer_party); }
        }
        virtual public string CciClientIdReceiver
        {
            get { return CciClientId(CciEnum.payerReceiver_receiver_party); }
        }
        public void SynchronizePayerReceiver(string pLastValue, string pNewValue)
        {
            ccis.Synchronize(CciClientIdPayer, pLastValue, pNewValue);
            ccis.Synchronize(CciClientIdReceiver, pLastValue, pNewValue);
        }
        #endregion IContainerCciPayerReceiver Members
        #region ITradeCci Members
        #region RetSidePayer
        public string RetSidePayer { get { return SideTools.RetBuySide(); } }
        #endregion RetSidePayer
        #region RetSideReceiver
        public string RetSideReceiver { get { return SideTools.RetSellSide(); } }
        #endregion RetSideReceiver
        #region GetMainCurrency
        public string GetMainCurrency
        {
            get
            {
                return GetDefaultCurrency();
            }
        }
        private static string GetDefaultCurrency()
        {
            return ConfigurationManager.AppSettings["Spheres_ReferentialDefault_currency"];
        }
        #endregion GetMainCurrency
        #region CciClientIdMainCurrency
        public string CciClientIdMainCurrency { get { return string.Empty; } }
        #endregion CciClientIdMainCurrency
        #endregion ITradeCci Members
        #region IContainerCci members
        #region CciClientId
        public string CciClientId(CciEnum pEnumValue)
        {
            return CciClientId(pEnumValue.ToString());
        }
        public string CciClientId(string pClientId_Key)
        {
            return prefix + pClientId_Key;
        }
        #endregion
        #region Cci
        public CustomCaptureInfo Cci(CciEnum pEnum)
        {
            return Cci(pEnum.ToString());
        }
        public CustomCaptureInfo Cci(string pClientId_Key)
        {
            return ccis[CciClientId(pClientId_Key)];
        }

        #endregion
        #region IsCciOfContainer
        public bool IsCciOfContainer(string pClientId_WithoutPrefix)
        {
            return (pClientId_WithoutPrefix.StartsWith(prefix));
        }
        #endregion
        #region CciContainerKey
        public string CciContainerKey(string pClientId_WithoutPrefix)
        {
            return pClientId_WithoutPrefix.Substring(prefix.Length);
        }
        #endregion
        #region IsCci
        public bool IsCci(CciEnum pEnumValue, CustomCaptureInfo pCci)
        {
            return (this.CciClientId(pEnumValue) == pCci.ClientId_WithoutPrefix);
        }
        #endregion
        #endregion IContainerCci members
        #region IContainerCciQuoteBasis members
        public bool IsClientId_QuoteBasis(CustomCaptureInfo pCci)
        {
            return (null != cciEventDetail) && (cciEventDetail.IsClientId_QuoteBasis(pCci));
        }
        public string GetCurrency1(CustomCaptureInfo pCci)
        {
            string ret = string.Empty;
            if (null != cciEventDetail)
                ret = cciEventDetail.GetCurrency1(pCci);
            return ret;
        }

        public string GetCurrency2(CustomCaptureInfo pCci)
        {
            string ret = string.Empty;
            if (null != cciEventDetail)
                ret = cciEventDetail.GetCurrency2(pCci);
            return ret;
        }
        public string GetBaseCurrency(CustomCaptureInfo pCci)
        {
            string ret = string.Empty;
            if (null != cciEventDetail)
                ret = cciEventDetail.GetBaseCurrency(pCci);
            return ret;

        }
        #endregion IContainerCciQuoteBasis members
        #region Membres de ITradeGetInfoButton
        #region SetButtonZoom
        public virtual bool SetButtonZoom(CustomCaptureInfo pCci, CustomObjectButtonFpmlObject pCo, ref bool pIsSpecified, ref bool pIsEnabled)
        {
            bool isOk = false;
            return isOk;
        }
        #endregion SetButtonZoom
        #region SetButtonScreenBox
        public virtual bool SetButtonScreenBox(CustomCaptureInfo pCci, CustomObjectButtonScreenBox pCo, ref bool pIsSpecified, ref bool pIsEnabled)
        {
           bool isOk = false;
                if (IsCciOfContainer(pCci.ClientId_WithoutPrefix))
                {
                    if (IsCci(CciEnum.eventDet_dayCountFraction_screen, pCci))
                    {
                        #region DayCountFraction
                        isOk = true;
                        pIsSpecified = _event.detailsSpecified && _event.details.dcfSpecified;
                        pIsEnabled = true;
                        #endregion DayCountFraction
                    }
                    else if (IsCci(CciEnum.eventDet_premiumQuote_screen, pCci))
                    {
                        #region PremiumQuote
                        isOk = true;
                        pIsSpecified = _event.detailsSpecified && _event.details.premiumQuoteBasisSpecified;
                        pIsEnabled = true;
                        #endregion PremiumQuote
                    }
                    else if (IsCci(CciEnum.eventDet_paymentQuote_screen, pCci))
                    {
                        #region PaymentQuote
                        isOk = true;
                        pIsSpecified = _event.detailsSpecified && _event.details.pctRateSpecified;
                        pIsEnabled = true;
                        #endregion PaymentQuote
                    }
                    else if (IsCci(CciEnum.eventDet_exchangeRate_screen, pCci))
                    {
                        #region ExchangeRate
                        isOk = true;
                        pIsSpecified = _event.detailsSpecified && _event.details.quoteBasisSpecified;
                        pIsEnabled = true;
                        #endregion ExchangeRate
                    }
                    else if (IsCci(CciEnum.eventDet_exchangeRatePremium_screen, pCci))
                    {
                        #region ExchangeRatePremium
                        isOk = true;
                        pIsSpecified = _event.detailsSpecified && _event.details.quoteBasisSpecified;
                        pIsEnabled = true;
                        #endregion ExchangeRatePremium
                    }
                    else if (IsCci(CciEnum.eventDet_sideRate_screen, pCci))
                    {
                        #region SideRate
                        isOk = true;
                        pIsSpecified = _event.detailsSpecified && _event.details.sideRateBasisSpecified;
                        pIsEnabled = true;
                        #endregion SideRate
                    }
                    else if (IsCci(CciEnum.eventDet_strikePrice_screen, pCci))
                    {
                        #region Strike
                        isOk = true;
                        pIsSpecified = _event.detailsSpecified && _event.details.strikePriceSpecified;
                        pIsEnabled = true;
                        #endregion Strike
                    }
                    else if (IsCci(CciEnum.eventDet_fixingRate_screen, pCci))
                    {
                        #region FixingRate
                        isOk = true;
                        pIsSpecified = _event.detailsSpecified && _event.details.idBCSpecified;
                        pIsEnabled = true;
                        #endregion FixingRate
                    }
                    else if (IsCci(CciEnum.eventDet_currencyPair_screen, pCci))
                    {
                        #region CurrencyPair
                        isOk = true;
                        pIsSpecified = _event.detailsSpecified && _event.details.idC1Specified;
                        pIsEnabled = true;
                        #endregion CurrencyPair
                    }
                    else if (IsCci(CciEnum.eventDet_triggerRate_screen, pCci) ||
                             IsCci(CciEnum.eventDet_fixedRate_screen, pCci))
                    {
                        #region TriggerRate / FixedRate
                        isOk = true;
                        pIsSpecified = _event.detailsSpecified && _event.details.rateSpecified;
                        pIsEnabled = true;
                        #endregion TriggerRate / FixedRate
                    }
                    else if (IsCci(CciEnum.eventDet_capFloorSchedule_screen, pCci))
                    {
                        #region CapFloorSchedule
                        isOk = true;
                        pIsSpecified = _event.detailsSpecified && _event.details.strikePriceSpecified;
                        pIsEnabled = true;
                        #endregion CapFloorSchedule
                    }
                    else if (IsCci(CciEnum.eventDet_settlementRate_screen, pCci))
                    {
                        #region SettlementRate
                        isOk = true;
                        pIsSpecified = _event.detailsSpecified && _event.details.settlementRateSpecified;
                        pIsEnabled = true;
                        #endregion SettlementRate
                    }
                    else if (IsCci(CciEnum.eventDet_settlementRate_screen, pCci))
                    {
                        #region SettlementRate
                        isOk = true;
                        pIsSpecified = _event.detailsSpecified && _event.details.settlementRateSpecified;
                        pIsEnabled = true;
                        #endregion SettlementRate
                    }
                    else if (IsCci(CciEnum.eventDet_sideRate_screen, pCci))
                    {
                        #region SideRate
                        isOk = true;
                        pIsSpecified = _event.detailsSpecified && _event.details.sideRateBasisSpecified;
                        pIsEnabled = true;
                        #endregion SideRate
                    }
                    else if (IsCci(CciEnum.eventDet_pricingFxOption_screen, pCci) || IsCci(CciEnum.eventDet_pricingFx_screen, pCci))
                    {
                        #region Pricing
                        isOk = true;
                        pIsSpecified = _event.pricingSpecified;
                        pIsEnabled = true;
                        #endregion Pricing
                    }
                    else if (IsCci(CciEnum.eventDet_pricing2_screen, pCci))
                    {
                        #region Pricing
                        isOk = true;
                        pIsSpecified = _event.pricing2Specified;
                        pIsEnabled = true;
                        #endregion Pricing
                    }
                    else if (IsCci(CciEnum.eventDet_assetFxRate_screen, pCci) || IsCci(CciEnum.eventDet_assetRateIndex_screen, pCci))
                    {
                        #region Asset
                        isOk = true;
                        pIsSpecified = _event.assetSpecified;
                        pIsEnabled = _event.assetSpecified;
                        #endregion Asset
                    }
                    else if (IsCci(CciEnum.eventDet_asset_screen, pCci))
                    {
                        #region Asset
                        isOk = true;
                        pIsSpecified = _event.assetSpecified;
                        pIsEnabled = _event.assetSpecified;
                        #endregion Asset
                    }
                    else if (IsCci(CciEnum.eventDet_process_screen, pCci))
                    {
                        #region Process
                        isOk = true;
                        pIsSpecified = _event.processSpecified;
                        pIsEnabled = true;
                        #endregion Process
                    }
                    else if (IsCci(CciEnum.eventDet_eventChild_screen, pCci))
                    {
                        #region Childs
                        isOk = true;
                        pIsSpecified = ArrFunc.IsFilled(ccis.EventInput.CurrentEventChilds);
                        pIsEnabled = true;
                        #endregion Childs
                    }
                    else if (IsCci(CciEnum.eventDet_notes_screen, pCci))
                    {
                        #region Notes
                        isOk = true;
                        pIsSpecified = _event.detailsSpecified && (_event.details.noteSpecified || _event.details.dtActionSpecified || _event.details.extlLinkSpecified);
                        pIsEnabled = true;
                        #endregion Notes
                    }
                }
                return isOk;
            
        }
        #endregion SetButtonScreenBox
        #region SetButtonReferential
        public virtual void SetButtonReferential(CustomCaptureInfo pCci, CustomObjectButtonReferential pCo)
        {
            for (int i = 0; i < EventAssetLenght; i++)
            {
                cciEventAsset[i].SetButtonReferential(pCci, pCo);
            }
        }
        #endregion SetButtonReferential
        #endregion Membres de ITradeGetInfoButton
        #endregion Interface Methods

        #region Methods
        #region SwapBookPayerReceiver
        private void SwapBookPayerReceiver()
        {
            CustomCaptureInfo cciPayerBook = Cci(CciEnum.payerReceiver_payer_book);
            string payerBook = cciPayerBook.NewValue;
            
            CustomCaptureInfo cciReceiverBook = Cci(CciEnum.payerReceiver_receiver_book);
            string receiverBook = cciReceiverBook.NewValue;
            
            ccis.SetNewValue(CciClientId(CciEnum.payerReceiver_receiver_book), payerBook);
            ccis.SetNewValue(CciClientId(CciEnum.payerReceiver_payer_book), receiverBook);
        }
        #endregion SwapBookPayerReceiver
        #endregion Methods
    }
    #endregion CciEvent
}
