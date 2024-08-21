#region Using Directives
using EFS.ACommon;
using EFS.Common;
using EFS.Common.Web;
using EFS.GUI.CCI;
using EFS.GUI.Interface;
using EfsML;
using System;
using System.Web.UI;

#endregion Using Directives

namespace EFS.TradeInformation
{
    #region CciEventPricing2
    public class CciEventPricing2 : IContainerCciFactory, IContainerCci, IContainerCciGetInfoButton
    {
        #region Enums
        #region CciEnum
        public enum CciEnum
        {
            flowType,
            totalOfDay,
            cashFlow,
            discountFactor,
            rate,
            npv,
            dtClosing,
            method,
            methodDisplayName,
            idYieldCurveVal_H,
            idYieldCurveDef,
            zeroCouponOne,
            zeroCouponTwo,
            forwardRate,
            extlLink,

            screen,
            unknown,
        }
        #endregion CciEnum
        #endregion Enums
        #region Members
        public EventCustomCaptureInfos ccis;
        private EventPricing2 eventPricing2;
        private readonly string prefix;
        private readonly int number;
        #endregion Members
        #region Accessors
        #region CS
        public string CS
        {
            get { return ccis.CS; }
        }
        #endregion CS
        
        #region EventPricing2
        public EventPricing2 EventPricing2
        {
            set
            {
                eventPricing2 = (EventPricing2)value;
            }
            get
            {
                return eventPricing2;
            }
        }
        #endregion EventProcess
        #region ExistNumber
        private bool ExistNumber
        {
            get
            {
                return (0 < number);
            }
        }
        #endregion ExistNumber
        #region Number
        private string Number
        {
            get
            {
                string ret = string.Empty;
                if (ExistNumber)
                    ret = number.ToString();
                return ret;
            }
        }
        #endregion Number
        #endregion Accessors
        #region Constructor
        public CciEventPricing2(CciEvent pCciEvent, int pNumber, EventPricing2 pEventPricing2, string pPrefix)
        {
            number = pNumber;
            prefix = pPrefix + this.Number + CustomObject.KEY_SEPARATOR;
            ccis = pCciEvent.ccis;
            eventPricing2 = pEventPricing2;
        }
        #endregion Constructors

        #region Methods
        #region Clear
        public void Clear()
        {
            
                ccis.Set(CciClientId(CciEnum.cashFlow), "NewValue", string.Empty);
                ccis.Set(CciClientId(CciEnum.discountFactor), "NewValue", string.Empty);
                ccis.Set(CciClientId(CciEnum.dtClosing), "NewValue", string.Empty);
                ccis.Set(CciClientId(CciEnum.extlLink), "NewValue", string.Empty);
                ccis.Set(CciClientId(CciEnum.flowType), "NewValue", string.Empty);
                ccis.Set(CciClientId(CciEnum.forwardRate), "NewValue", string.Empty);
                ccis.Set(CciClientId(CciEnum.idYieldCurveVal_H), "NewValue", string.Empty);
                ccis.Set(CciClientId(CciEnum.idYieldCurveDef), "NewValue", string.Empty);
                ccis.Set(CciClientId(CciEnum.method), "NewValue", string.Empty);
                ccis.Set(CciClientId(CciEnum.methodDisplayName), "NewValue", string.Empty);
                ccis.Set(CciClientId(CciEnum.npv), "NewValue", string.Empty);
                ccis.Set(CciClientId(CciEnum.totalOfDay), "NewValue", string.Empty);
                ccis.Set(CciClientId(CciEnum.zeroCouponOne), "NewValue", string.Empty);
                ccis.Set(CciClientId(CciEnum.zeroCouponTwo), "NewValue", string.Empty);
            
        }
        #endregion Clear
        #region  SetEnabled
        public void SetEnabled(Boolean _)
        {
            
        }
        #endregion SetEnabled
        #endregion Methods

        #region Interface Methods
        #region IContainerCciFactory members
        #region AddCciSystem
        public void AddCciSystem()
        {
        }
        #endregion AddCciSystem
        #region Initialize_FromCci
        public void Initialize_FromCci() { }
        #endregion Initialize_FromCci
        #region Initialize_FromDocument
        public void Initialize_FromDocument()
        {
            
                string data;
                bool isSetting;
                SQL_Table sql_Table;

                Type tCciEnum = typeof(CciEnum);
                foreach (string enumName in Enum.GetNames(tCciEnum))
                {
                    CustomCaptureInfo cci = ccis[prefix + enumName];
                    if (cci != null)
                    {
                        #region Reset variables
                        data = string.Empty;
                        isSetting = true;
                        sql_Table = null;
                        #endregion Reset variables
                        //
                        CciEnum keyEnum = (CciEnum)System.Enum.Parse(typeof(CciEnum), enumName);
                        switch (keyEnum)
                        {
                            case CciEnum.cashFlow:
                                #region CashFlow
                                if (eventPricing2.cashFlowSpecified)
                                    data = eventPricing2.cashFlow.Value;
                                break;
                                #endregion CashFlow
                            case CciEnum.discountFactor:
                                #region discountFactor
                                if (eventPricing2.discountFactorSpecified)
                                    data = eventPricing2.discountFactor.Value;
                                break;
                                #endregion discountFactor
                            case CciEnum.dtClosing:
                                #region dtClosing
                                if (eventPricing2.dtClosingSpecified)
                                    data = eventPricing2.dtClosing.Value;
                                break;
                                #endregion dtClosing
                            case CciEnum.extlLink:
                                #region ExternalLink
                                if (eventPricing2.extlLinkSpecified)
                                    data = eventPricing2.extlLink;
                                break;
                                #endregion ExternalLink
                            case CciEnum.flowType:
                                #region flowType
                                if (eventPricing2.flowTypeSpecified)
                                    data = eventPricing2.flowType;
                                break;
                                #endregion flowType
                            case CciEnum.forwardRate:
                                #region forwardRate
                                if (eventPricing2.forwardRateSpecified)
                                    data = eventPricing2.forwardRate.Value;
                                break;
                                #endregion forwardRate
                            case CciEnum.idYieldCurveVal_H:
                                #region idYieldCurveVal_H
                                if (eventPricing2.idYieldCurveVal_HSpecified)
                                    data = eventPricing2.idYieldCurveVal_H.Value;
                                break;
                                #endregion idYieldCurveVal_H
                            case CciEnum.idYieldCurveDef:
                                #region idYieldCurveDef
                                if (eventPricing2.idYieldCurveDefSpecified)
                                    data = eventPricing2.idYieldCurveDef;
                                break;
                                #endregion idYieldCurveDef
                            case CciEnum.method:
                                #region method
                                if (eventPricing2.methodSpecified)
                                    data = eventPricing2.method;
                                break;
                                #endregion method
                            case CciEnum.methodDisplayName:
                                #region methodDisplayName
                                if (eventPricing2.methodDisplayNameSpecified)
                                    data = eventPricing2.methodDisplayName;
                                break;
                                #endregion methodDisplayName
                            case CciEnum.npv:
                                #region npv
                                if (eventPricing2.npvSpecified)
                                    data = eventPricing2.npv.Value;
                                break;
                                #endregion npv
                            case CciEnum.rate:
                                #region rate
                                if (eventPricing2.rateSpecified)
                                    data = eventPricing2.rate.Value;
                                break;
                                #endregion rate
                            case CciEnum.totalOfDay:
                                #region totalOfDay
                                if (eventPricing2.totalOfDaySpecified)
                                    data = eventPricing2.totalOfDay.Value;
                                break;
                                #endregion totalOfDay
                            case CciEnum.zeroCouponOne:
                                #region zeroCoupon1
                                if (eventPricing2.zeroCoupon1Specified)
                                    data = eventPricing2.zeroCoupon1.Value;
                                break;
                                #endregion zeroCoupon1
                            case CciEnum.zeroCouponTwo:
                                #region zeroCoupon2
                                if (eventPricing2.zeroCoupon2Specified)
                                    data = eventPricing2.zeroCoupon2.Value;
                                break;
                                #endregion zeroCoupon2
                            default:
                                isSetting = false;
                                break;
                        }
                        if (isSetting)
                            ccis.InitializeCci(cci, sql_Table, data);
                    }
                }
                //if (false == Cci(CciEnum.idYieldCurveVal_H).IsMandatory)
                //    SetEnabled(Cci(CciEnum.idYieldCurveVal_H).IsFilledValue);
            
        }
        #endregion Initialize_FromDocument
        #region Dump_ToDocument
        public void Dump_ToDocument()
        {
            bool isSetting;
            string data;
            CustomCaptureInfosBase.ProcessQueueEnum processQueue;

            Type tCciEnum = typeof(CciEnum);
            foreach (string enumName in Enum.GetNames(tCciEnum))
            {
                CustomCaptureInfo cci = ccis[prefix + enumName];
                if ((cci != null) && (cci.HasChanged))
                {
                    #region Reset variables
                    data = cci.NewValue;
                    isSetting = true;
                    processQueue = CustomCaptureInfosBase.ProcessQueueEnum.None;
                    #endregion
                    //
                    CciEnum keyEnum = (CciEnum)System.Enum.Parse(typeof(CciEnum), enumName);
                    switch (keyEnum)
                    {
                        case CciEnum.cashFlow:
                            #region CashFlow
                            eventPricing2.cashFlowSpecified = StrFunc.IsFilled(data);
                            if (eventPricing2.cashFlowSpecified)
                                eventPricing2.cashFlow = new EFS_Decimal(data);
                            break;
                            #endregion CashFlow
                        case CciEnum.discountFactor:
                            #region discountFactor
                            eventPricing2.discountFactorSpecified = StrFunc.IsFilled(data);
                            if (eventPricing2.discountFactorSpecified)
                                eventPricing2.discountFactor = new EFS_Decimal(data);
                            break;
                            #endregion discountFactor
                        case CciEnum.extlLink:
                            #region ExternalLink
                            eventPricing2.extlLinkSpecified = StrFunc.IsFilled(data);
                            eventPricing2.extlLink = data;
                            #endregion ExternalLink
                            break;
                        case CciEnum.flowType:
                            #region flowType
                            eventPricing2.flowTypeSpecified = StrFunc.IsFilled(data);
                            eventPricing2.flowType = data;
                            break;
                            #endregion flowType
                        case CciEnum.forwardRate:
                            #region forwardRate
                            eventPricing2.forwardRateSpecified = StrFunc.IsFilled(data);
                            if (eventPricing2.forwardRateSpecified)
                                eventPricing2.forwardRate = new EFS_Decimal(data);
                            break;
                            #endregion forwardRate
                        case CciEnum.method:
                            #region method
                            eventPricing2.methodSpecified = StrFunc.IsFilled(data);
                            eventPricing2.method = data;
                            break;
                            #endregion method
                        case CciEnum.methodDisplayName:
                            #region methodDisplayName
                            eventPricing2.methodDisplayNameSpecified = StrFunc.IsFilled(data);
                            eventPricing2.methodDisplayName = data;
                            break;
                            #endregion methodDisplayName
                        case CciEnum.npv:
                            #region npv
                            eventPricing2.npvSpecified = StrFunc.IsFilled(data);
                            if (eventPricing2.npvSpecified)
                                eventPricing2.npv = new EFS_Decimal(data);
                            break;
                            #endregion npv
                        case CciEnum.rate:
                            #region rate
                            eventPricing2.rateSpecified = StrFunc.IsFilled(data);
                            if (eventPricing2.rateSpecified)
                                eventPricing2.rate = new EFS_Decimal(data);
                            break;
                            #endregion rate
                        case CciEnum.totalOfDay:
                            #region totalOfDay
                            eventPricing2.totalOfDaySpecified = StrFunc.IsFilled(data);
                            if (eventPricing2.totalOfDaySpecified)
                                eventPricing2.totalOfDay = new EFS_Integer(data);
                            break;
                            #endregion totalOfDay
                        case CciEnum.zeroCouponOne:
                            #region zeroCoupon1
                            eventPricing2.zeroCoupon1Specified = StrFunc.IsFilled(data);
                            if (eventPricing2.zeroCoupon1Specified)
                                eventPricing2.zeroCoupon1 = new EFS_Decimal(data);
                            break;
                            #endregion zeroCoupon1
                        case CciEnum.zeroCouponTwo:
                            #region zeroCoupon2
                            eventPricing2.zeroCoupon2Specified = StrFunc.IsFilled(data);
                            if (eventPricing2.zeroCoupon2Specified)
                                eventPricing2.zeroCoupon2 = new EFS_Decimal(data);
                            break;
                            #endregion zeroCoupon2
                        default:
                            isSetting = false;
                            break;
                    }
                    if (isSetting)
                        ccis.Finalize(cci.ClientId_WithoutPrefix, processQueue);
                }
            }
            //if (false == Cci(CciEnum.idYieldCurveVal_H).IsMandatory)
            //    SetEnabled(Cci(CciEnum.idYieldCurveVal_H).IsFilledValue);
        }
        #endregion Dump_ToDocument
        #region DumpSpecific_ToGUI
        public void DumpSpecific_ToGUI(CciPageBase pPage)
        {
            // On Masque les champs non valorisés
            CustomCaptureInfo cciItem = Cci(CciEnum.totalOfDay);
            Control ctrl = pPage.FindControl(cciItem.ClientId);
            if (null != ctrl)
            {
                ctrl.Visible = eventPricing2.totalOfDaySpecified;
                ctrl = pPage.FindControl("LBL" + cciItem.ClientId_WithoutPrefix);
                if (null != ctrl)
                {
                    ctrl.Visible = eventPricing2.totalOfDaySpecified;
                    ctrl.Parent.Parent.Visible = eventPricing2.totalOfDaySpecified;
                }
            }
            //
            cciItem = Cci(CciEnum.discountFactor);
            ctrl = pPage.FindControl(cciItem.ClientId);
            if (null != ctrl)
            {
                ctrl.Visible = eventPricing2.discountFactorSpecified;
                ctrl = pPage.FindControl("LBL" + cciItem.ClientId_WithoutPrefix);
                if (null != ctrl)
                {
                    ctrl.Visible = eventPricing2.discountFactorSpecified;
                    ctrl.Parent.Parent.Visible = eventPricing2.discountFactorSpecified;
                }
            }
            //
            cciItem = Cci(CciEnum.forwardRate);
            ctrl = pPage.FindControl(cciItem.ClientId);
            if (null != ctrl)
            {
                ctrl.Visible = eventPricing2.forwardRateSpecified;
                ctrl = pPage.FindControl("LBL" + cciItem.ClientId_WithoutPrefix);
                if (null != ctrl)
                {
                    ctrl.Visible = eventPricing2.forwardRateSpecified;
                    ctrl.Parent.Parent.Visible = eventPricing2.forwardRateSpecified;
                }
            }
            //
            cciItem = Cci(CciEnum.methodDisplayName);
            ctrl = pPage.FindControl(cciItem.ClientId);
            if (null != ctrl)
            {
                ctrl.Visible = eventPricing2.methodDisplayNameSpecified;
                ctrl = pPage.FindControl("LBL" + cciItem.ClientId_WithoutPrefix);
                if (null != ctrl)
                {
                    ctrl.Visible = eventPricing2.methodDisplayNameSpecified;
                    ctrl.Parent.Parent.Visible = eventPricing2.methodDisplayNameSpecified;
                }
            }
            //
            cciItem = Cci(CciEnum.npv);
            ctrl = pPage.FindControl(cciItem.ClientId);
            if (null != ctrl)
            {
                ctrl.Visible = eventPricing2.npvSpecified;
                ctrl = pPage.FindControl("LBL" + cciItem.ClientId_WithoutPrefix);
                if (null != ctrl)
                {
                    ctrl.Visible = eventPricing2.npvSpecified;
                    ctrl.Parent.Parent.Visible = eventPricing2.npvSpecified;
                }
            }
            //
            cciItem = Cci(CciEnum.rate);
            ctrl = pPage.FindControl(cciItem.ClientId);
            if (null != ctrl)
            {
                ctrl.Visible = eventPricing2.rateSpecified;
                ctrl = pPage.FindControl("LBL" + cciItem.ClientId_WithoutPrefix);
                if (null != ctrl)
                {
                    ctrl.Visible = eventPricing2.rateSpecified;
                    ctrl.Parent.Parent.Visible = eventPricing2.rateSpecified;
                }
            }
            //
            cciItem = Cci(CciEnum.totalOfDay);
            ctrl = pPage.FindControl(cciItem.ClientId);
            if (null != ctrl)
            {
                ctrl.Visible = eventPricing2.totalOfDaySpecified;
                ctrl = pPage.FindControl("LBL" + cciItem.ClientId_WithoutPrefix);
                if (null != ctrl)
                {
                    ctrl.Visible = eventPricing2.totalOfDaySpecified;
                    ctrl.Parent.Parent.Visible = eventPricing2.totalOfDaySpecified;
                }
            }
            //
            cciItem = Cci(CciEnum.zeroCouponOne);
            ctrl = pPage.FindControl(cciItem.ClientId);
            if (null != ctrl)
            {
                ctrl.Visible = eventPricing2.zeroCoupon1Specified;
                ctrl = pPage.FindControl("LBL" + cciItem.ClientId_WithoutPrefix);
                if (null != ctrl)
                {
                    ctrl.Visible = eventPricing2.zeroCoupon1Specified;
                    ctrl.Parent.Parent.Visible = eventPricing2.zeroCoupon1Specified;
                }
            }
            //
            cciItem = Cci(CciEnum.zeroCouponTwo);
            ctrl = pPage.FindControl(cciItem.ClientId);
            if (null != ctrl)
            {
                ctrl.Visible = eventPricing2.zeroCoupon2Specified;
                ctrl = pPage.FindControl("LBL" + cciItem.ClientId_WithoutPrefix);
                if (null != ctrl)
                {
                    ctrl.Visible = eventPricing2.zeroCoupon2Specified;
                    ctrl.Parent.Parent.Visible = eventPricing2.zeroCoupon2Specified;
                }
            }
            //
            cciItem = Cci(CciEnum.cashFlow);
            ctrl = pPage.FindControl(cciItem.ClientId);
            if (null != ctrl)
            {
                ctrl.Visible = eventPricing2.cashFlowSpecified;
                ctrl = pPage.FindControl("LBL" + cciItem.ClientId_WithoutPrefix);
                if (null != ctrl)
                {
                    ctrl.Visible = eventPricing2.cashFlowSpecified;
                    ctrl.Parent.Parent.Visible = eventPricing2.cashFlowSpecified;
                }
            }
        }
        #endregion DumpSpecific_ToGUI
        #region IsClientId_PayerOrReceiver
        public bool IsClientId_PayerOrReceiver(CustomCaptureInfo pCci)
        {
            return false;
        }
        #endregion IsClientId_PayerOrReceiver
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

        }
        #endregion ProcessInitialize
        #region CleanUp
        public void CleanUp()
        {
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
        #endregion IContainerCciFactory Members
        #region IContainerCci Members
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
        #endregion IContainerCci Members
        #region IContainerCciGetInfoButton Members
        public bool SetButtonScreenBox(CustomCaptureInfo pCci, CustomObjectButtonScreenBox pCo, ref bool pIsObjSpecified, ref bool pIsEnabled)
        {
            return false;
        }
        public bool SetButtonZoom(CustomCaptureInfo pCci, CustomObjectButtonFpmlObject pCo, ref bool pIsObjSpecified, ref bool pIsEnabled)
        {
            return false;
        }
        public void SetButtonReferential(CustomCaptureInfo pCci, CustomObjectButtonReferential pCo)
        {
        }
        #endregion IContainerCciGetInfoButton Members
        #endregion Interface Methods
    }
    #endregion CciEventPricing2
}
