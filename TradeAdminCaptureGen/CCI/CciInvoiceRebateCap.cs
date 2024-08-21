#region Using Directives
using EFS.Common;
using EFS.Common.Web;
using EFS.GUI.CCI;
using EFS.GUI.Interface;
using EfsML.Enum.Tools;
using EfsML.Interface;
using FpML.Enum;
using System;
#endregion Using Directives

namespace EFS.TradeInformation
{
    /// <summary>
    /// Description résumée de CciInvoiceRebateCap. 
    /// </summary>
    public class CciInvoiceRebateCap : IContainerCciFactory, IContainerCci
    {
        #region Enums
        #region CciEnum
        public enum CciEnum
        {
            #region RebateCapParameters
            [System.Xml.Serialization.XmlEnumAttribute("parameters.applicationPeriod.periodMultiplier")]
            applicationPeriod_periodMultiplier,
            [System.Xml.Serialization.XmlEnumAttribute("parameters.applicationPeriod.period")]
            applicationPeriod_period,
            [System.Xml.Serialization.XmlEnumAttribute("parameters.maximumNetTurnOverAmount.amount")]
            maximumNetTurnOverAmount_amount,
            [System.Xml.Serialization.XmlEnumAttribute("parameters.maximumNetTurnOverAmount.currency")]
            maximumNetTurnOverAmount_currency,
            #endregion RebateCapParameters
            #region RebateCapResult
            [System.Xml.Serialization.XmlEnumAttribute("result.sumOfNetTurnOverPreviousPeriodAmount.amount")]
            sumOfNetTurnOverPreviousPeriodAmount_amount,
            [System.Xml.Serialization.XmlEnumAttribute("result.sumOfNetTurnOverPreviousPeriodAmount.currency")]
            sumOfNetTurnOverPreviousPeriodAmount_currency,
            [System.Xml.Serialization.XmlEnumAttribute("result.netTurnOverInExcessAmount.amount")]
            netTurnOverInExcessAmount_amount,
            [System.Xml.Serialization.XmlEnumAttribute("result.netTurnOverInExcessAmount.currency")]
            netTurnOverInExcessAmount_currency,
            #endregion RebateCapResult
            unknown,
        }
        #endregion CciEnum
        #endregion Enums
        #region Members
        private readonly CciInvoiceRebate m_CciInvoiceRebate;
        private IRebateCapConditions m_RebateCapConditions;
        private readonly string m_Prefix;
        private readonly TradeAdminCustomCaptureInfos m_Ccis;
        #endregion Members
        #region Accessors
        #region Ccis
        public TradeAdminCustomCaptureInfos Ccis
        {
            get { return m_Ccis; }
        }
        #endregion Ccis
        #region IsRebateBracketCapSpecified
        public bool IsRebateBracketCapSpecified
        {
            get
            {
                bool isSpecified = false;
                if (null != m_CciInvoiceRebate.RebateConditions)
                    isSpecified = m_CciInvoiceRebate.RebateConditions.CapConditionsSpecified;
                return isSpecified;
            }
        }
        #endregion RebateBracketConditions

        #region TurnOverCurrency
        private string TurnOverCurrency
        {
            get { return m_CciInvoiceRebate.Invoice.GrossTurnOverAmount.Currency; }
        }
        #endregion TurnOverCurrency
        #region RebateCapConditions
        public IRebateCapConditions RebateCapConditions
        {
            get { return m_RebateCapConditions; }
            set { m_RebateCapConditions = value; }
        }
        #endregion RebateCapConditions
        #region RebateCapParameters
        public IRebateCapParameters RebateCapParameters
        {
            get { return m_RebateCapConditions.Parameters; }
            set { m_RebateCapConditions.Parameters = value; }
        }
        #endregion RebateCapParameters
        #region RebateCapResult
        public IRebateCapResult RebateCapResult
        {
            get { return m_RebateCapConditions.Result; }
            set { m_RebateCapConditions.Result = value; }
        }
        #endregion RebateCapResult
        #endregion Accessors
        #region Constructors
        public CciInvoiceRebateCap(CciInvoiceRebate pCciInvoiceRebate, string pPrefix, IRebateCapConditions pRebateCapConditions)
        {
            m_CciInvoiceRebate = pCciInvoiceRebate;
            m_Ccis = pCciInvoiceRebate.Ccis;
            m_Prefix = pPrefix + CustomObject.KEY_SEPARATOR;
            m_RebateCapConditions = pRebateCapConditions;
        }
        #endregion Constructors
        #region Interface
        #region IContainerCciFactory Members
        #region AddCciSystem
        public void AddCciSystem()
        {
        }
        #endregion AddCciSystem
        #region CleanUp
        public void CleanUp()
        {
        }
        #endregion CleanUp
        #region Dump_ToDocument
        public void Dump_ToDocument()
        {
            bool isSetting;
            string data;
            CustomCaptureInfosBase.ProcessQueueEnum processQueue;
            Type tCciEnum = typeof(CciEnum);
            foreach (string enumName in Enum.GetNames(tCciEnum))
            {
                CustomCaptureInfo cci = Ccis[m_Prefix + enumName];
                if ((cci != null) && (cci.HasChanged))
                {
                    #region Reset variables
                    data = cci.NewValue;
                    isSetting = true;
                    processQueue = CustomCaptureInfosBase.ProcessQueueEnum.None;
                    #endregion Reset variables
                    //
                    CciEnum keyEnum = (CciEnum)System.Enum.Parse(typeof(CciEnum), enumName);
                    switch (keyEnum)
                    {
                        case CciEnum.applicationPeriod_period:
                            #region ApplicationPeriod (Period)
                            m_CciInvoiceRebate.RebateConditions.CapConditionsSpecified = cci.IsFilled;
                            if (m_CciInvoiceRebate.RebateConditions.CapConditionsSpecified)
                                RebateCapParameters.ApplicationPeriod.Period = StringToEnum.Period(data);
                            processQueue = CustomCaptureInfosBase.ProcessQueueEnum.Low;
                            #endregion ApplicationPeriod (Period)
                            break;
                        case CciEnum.applicationPeriod_periodMultiplier:
                            #region ApplicationPeriod (PeriodMultiplier)
                            m_CciInvoiceRebate.RebateConditions.CapConditionsSpecified = cci.IsFilled;
                            if (m_CciInvoiceRebate.RebateConditions.CapConditionsSpecified)
                                RebateCapParameters.ApplicationPeriod.PeriodMultiplier.Value = data;
                            processQueue = CustomCaptureInfosBase.ProcessQueueEnum.Low;
                            #endregion ApplicationPeriod (PeriodMultiplier)
                            break;
                        case CciEnum.maximumNetTurnOverAmount_amount:
                            #region MaximumNetTurnOverAmount (Amount)
                            RebateCapParameters.MaximumNetTurnOverAmountSpecified = cci.IsFilledValue;
                            RebateCapParameters.MaximumNetTurnOverAmount.Amount.Value = data;
                            processQueue = CustomCaptureInfosBase.ProcessQueueEnum.High;
                            #endregion MaximumNetTurnOverAmount (Amount)
                            break;
                        case CciEnum.maximumNetTurnOverAmount_currency:
                            #region MaximumNetTurnOverAmount (Currency)
                            RebateCapParameters.MaximumNetTurnOverAmount.Currency = data;
                            #endregion MaximumNetTurnOverAmount (Currency)
                            break;
                        case CciEnum.sumOfNetTurnOverPreviousPeriodAmount_amount:
                            #region SumOfNetTurnOverPreviousPeriodAmount (Amount)
                            RebateCapResult.SumOfNetTurnOverPreviousPeriodAmountSpecified = cci.IsFilledValue;
                            RebateCapResult.SumOfNetTurnOverPreviousPeriodAmount.Amount.Value = data;
                            processQueue = CustomCaptureInfosBase.ProcessQueueEnum.High;
                            #endregion SumOfNetTurnOverPreviousPeriodAmount (Amount)
                            break;
                        case CciEnum.sumOfNetTurnOverPreviousPeriodAmount_currency:
                            #region SumOfNetTurnOverPreviousPeriodAmount (Currency)
                            RebateCapResult.SumOfNetTurnOverPreviousPeriodAmount.Currency = data;
                            #endregion SumOfNetTurnOverPreviousPeriodAmount (Currency)
                            break;
                        case CciEnum.netTurnOverInExcessAmount_amount:
                            #region NetTurnOverInExcessAmount (Amount)
                            RebateCapResult.NetTurnOverInExcessAmountSpecified = cci.IsFilledValue;
                            RebateCapResult.NetTurnOverInExcessAmount.Amount.Value = data;
                            processQueue = CustomCaptureInfosBase.ProcessQueueEnum.High;
                            #endregion NetTurnOverInExcessAmount (Amount)
                            break;
                        case CciEnum.netTurnOverInExcessAmount_currency:
                            #region NetTurnOverInExcessAmount (Currency)
                            RebateCapResult.NetTurnOverInExcessAmount.Currency = data;
                            #endregion NetTurnOverInExcessAmount (Currency)
                            break;
                        default:
                            #region default
                            isSetting = false;
                            #endregion default
                            break;
                    }
                    if (isSetting)
                        Ccis.Finalize(cci.ClientId_WithoutPrefix, processQueue);
                }
            }
        }
        #endregion Dump_ToDocument
        #region Initialize_Document
        public void Initialize_Document()
        {
        }
        #endregion Initialize_Document
        #region Initialize_FromCci
        public void Initialize_FromCci()
        {
            CciTools.CreateInstance(this, m_RebateCapConditions);
        }
        #endregion Initialize_FromCci
        #region Initialize_FromDocument
        public void Initialize_FromDocument()
        {
            string data;
            bool isSetting;
            SQL_Table sql_Table;
            bool isToValidate;
            Type tCciEnum = typeof(CciEnum);
            foreach (string enumName in Enum.GetNames(tCciEnum))
            {
                CustomCaptureInfo cci = Ccis[m_Prefix + enumName];
                if (cci != null)
                {
                    #region Reset variables
                    data = string.Empty;
                    isSetting = true;
                    isToValidate = false;
                    sql_Table = null;
                    #endregion Reset variables

                    CciEnum keyEnum = (CciEnum)System.Enum.Parse(typeof(CciEnum), enumName);
                    switch (keyEnum)
                    {
                        case CciEnum.applicationPeriod_periodMultiplier:
                            #region ApplicationPeriod (Multiplier)
                            if (IsRebateBracketCapSpecified)
                                data = RebateCapParameters.ApplicationPeriod.PeriodMultiplier.Value;
                            #endregion ApplicationPeriod (Multiplier)
                            break;
                        case CciEnum.applicationPeriod_period:
                            #region ApplicationPeriod (Period)
                            if (IsRebateBracketCapSpecified)
                                data = RebateCapParameters.ApplicationPeriod.Period.ToString();
                            #endregion ApplicationPeriod (Period)
                            break;
                        case CciEnum.maximumNetTurnOverAmount_amount:
                            #region MaximumNetTurnOverAmount (Amount)
                            if (RebateCapParameters.MaximumNetTurnOverAmountSpecified)
                                data = RebateCapParameters.MaximumNetTurnOverAmount.Amount.Value;
                            #endregion MaximumNetTurnOverAmount (Amount)
                            break;
                        case CciEnum.maximumNetTurnOverAmount_currency:
                            #region MaximumNetTurnOverAmount (Currency)
                            if (RebateCapParameters.MaximumNetTurnOverAmountSpecified)
                                data = RebateCapParameters.MaximumNetTurnOverAmount.Currency;
                            #endregion MaximumNetTurnOverAmount (Currency)
                            break;
                        case CciEnum.sumOfNetTurnOverPreviousPeriodAmount_amount:
                            #region SumOfNetTurnOverPreviousPeriodAmount (Amount)
                            if (RebateCapResult.SumOfNetTurnOverPreviousPeriodAmountSpecified)
                                data = RebateCapResult.SumOfNetTurnOverPreviousPeriodAmount.Amount.Value;
                            #endregion SumOfNetTurnOverPreviousPeriodAmount (Amount)
                            break;
                        case CciEnum.sumOfNetTurnOverPreviousPeriodAmount_currency:
                            #region SumOfNetTurnOverPreviousPeriodAmount (Currency)
                            if (RebateCapResult.SumOfNetTurnOverPreviousPeriodAmountSpecified)
                                data = RebateCapResult.SumOfNetTurnOverPreviousPeriodAmount.Currency;
                            #endregion SumOfNetTurnOverPreviousPeriodAmount (Currency)
                            break;
                        case CciEnum.netTurnOverInExcessAmount_amount:
                            #region NetTurnOverInExcessAmount (Amount)
                            if (RebateCapResult.NetTurnOverInExcessAmountSpecified)
                                data = RebateCapResult.NetTurnOverInExcessAmount.Amount.Value;
                            #endregion NetTurnOverInExcessAmount (Amount)
                            break;
                        case CciEnum.netTurnOverInExcessAmount_currency:
                            #region NetTurnOverInExcessAmount (Currency)
                            if (RebateCapResult.NetTurnOverInExcessAmountSpecified)
                                data = RebateCapResult.NetTurnOverInExcessAmount.Currency;
                            #endregion NetTurnOverInExcessAmount (Currency)
                            break;
                        default:
                            #region Default
                            isSetting = false;
                            #endregion Default
                            break;
                    }
                    if (isSetting)
                    {
                        Ccis.InitializeCci(cci, sql_Table, data);
                        if (isToValidate)
                            cci.LastValue = ".";
                    }

                }
            }
        }
        #endregion Initialize_FromDocument
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
            if (IsCciOfContainer(pCci.ClientId_WithoutPrefix))
            {
                string clientId_Element = CciContainerKey(pCci.ClientId_WithoutPrefix);
                CciEnum elt = CciEnum.unknown;
                if (System.Enum.IsDefined(typeof(CciEnum), clientId_Element))
                    elt = (CciEnum)System.Enum.Parse(typeof(CciEnum), clientId_Element);
                //
                switch (elt)
                {
                    case CciEnum.applicationPeriod_period:
                    case CciEnum.applicationPeriod_periodMultiplier:
                        #region ApplicationPeriod
                        DisplayAndCalculCapDetails();
                        #endregion ApplicationPeriod
                        break;
                    case CciEnum.maximumNetTurnOverAmount_amount:
                        #region MaximumNetTurnOverAmount (Amount)
                        CalculNetTurnOverExcessAmount();
                        #endregion MaximumNetTurnOverAmount (Amount)
                        break;
                    case CciEnum.sumOfNetTurnOverPreviousPeriodAmount_amount:
                        #region SumOfNetTurnOverPreviousPeriodAmount (Amount)
                        CalculNetTurnOverExcessAmount();
                        #endregion SumOfNetTurnOverPreviousPeriodAmount (Amount)
                        break;
                    case CciEnum.netTurnOverInExcessAmount_amount:
                        #region NetTurnOverInExcessAmount (Amount)
                        m_CciInvoiceRebate.SetRebateAmounts();
                        #endregion NetTurnOverInExcessAmount (Amount)
                        break;
                        
                    default:
                        #region Default
                        #endregion Default
                        break;
                }
            }
        }
        #endregion ProcessInitialize
        #region RefreshCciEnabled
        public void RefreshCciEnabled()
        {
            bool capConditionsSpecified = m_CciInvoiceRebate.RebateConditions.CapConditionsSpecified;
            bool maximumNetTurnOverAmountSpecified = capConditionsSpecified && RebateCapConditions.Parameters.MaximumNetTurnOverAmountSpecified;
            bool sumOfNetTurnOverPreviousPeriodAmountSpecified = capConditionsSpecified && (null != RebateCapResult) && RebateCapResult.SumOfNetTurnOverPreviousPeriodAmountSpecified;
            Ccis.Set(CciClientId(CciEnum.maximumNetTurnOverAmount_amount), "IsEnabled", capConditionsSpecified);
            Ccis.Set(CciClientId(CciEnum.maximumNetTurnOverAmount_currency), "IsEnabled", capConditionsSpecified);
            Ccis.Set(CciClientId(CciEnum.sumOfNetTurnOverPreviousPeriodAmount_amount), "IsEnabled", sumOfNetTurnOverPreviousPeriodAmountSpecified && capConditionsSpecified);
            Ccis.Set(CciClientId(CciEnum.sumOfNetTurnOverPreviousPeriodAmount_currency), "IsEnabled", sumOfNetTurnOverPreviousPeriodAmountSpecified && capConditionsSpecified);
            Ccis.Set(CciClientId(CciEnum.netTurnOverInExcessAmount_amount), "IsEnabled", maximumNetTurnOverAmountSpecified && capConditionsSpecified);
            Ccis.Set(CciClientId(CciEnum.netTurnOverInExcessAmount_currency), "IsEnabled", maximumNetTurnOverAmountSpecified && capConditionsSpecified);
        }
        #endregion RefreshCciEnabled
        #region RemoveLastItemInArray
        public void RemoveLastItemInArray(string _)
        {
        }
        #endregion RemoveLastItemInArray
        #region SetDisplay
        public void SetDisplay(CustomCaptureInfo pCci)
        {
        }
        #endregion SetDisplay
        #endregion IContainerCciFactory Members
        #region IContainerCci Members
        #region Cci
        public CustomCaptureInfo Cci(CciEnum pEnumValue)
        {
            return Ccis[CciClientId(pEnumValue.ToString())];
        }
        public CustomCaptureInfo Cci(string pClientId_Key)
        {
            return Ccis[CciClientId(pClientId_Key)];
        }
        #endregion Cci
        #region CciClientId
        public string CciClientId(CciEnum pEnumValue)
        {
            return CciClientId(pEnumValue.ToString());
        }
        public string CciClientId(string pClientId_Key)
        {
            return m_Prefix + pClientId_Key;
        }
        #endregion CciClientId
        #region CciContainerKey
        public string CciContainerKey(string pClientId_WithoutPrefix)
        {
            return pClientId_WithoutPrefix.Substring(m_Prefix.Length);
        }
        #endregion CciContainerKey
        #region IsCci
        public bool IsCci(CciEnum pEnumValue, CustomCaptureInfo pCci)
        {
            return (this.CciClientId(pEnumValue) == pCci.ClientId_WithoutPrefix);
        }
        #endregion IsCci
        #region IsCciOfContainer
        public bool IsCciOfContainer(string pClientId_WithoutPrefix)
        {
            return pClientId_WithoutPrefix.StartsWith(m_Prefix);
        }
        #endregion IsCciOfContainer
        #endregion IContainerCci Members
        #endregion Interface

        #region Methods
        #region CalculNetTurnOverExcessAmount
        public void CalculNetTurnOverExcessAmount()
        {
            if (m_RebateCapConditions.Parameters.MaximumNetTurnOverAmountSpecified)
            {
                EFS_Decimal netTurnOverExcessAmount = new EFS_Decimal(0)
                {
                    DecValue = m_RebateCapConditions.Parameters.MaximumNetTurnOverAmount.Amount.DecValue
                };
                if (m_RebateCapConditions.Result.SumOfNetTurnOverPreviousPeriodAmountSpecified)
                    netTurnOverExcessAmount.DecValue -= m_RebateCapConditions.Result.SumOfNetTurnOverPreviousPeriodAmount.Amount.DecValue;
                netTurnOverExcessAmount.DecValue -= m_CciInvoiceRebate.Invoice.GrossTurnOverAmount.Amount.DecValue;
                netTurnOverExcessAmount.DecValue = Math.Abs(netTurnOverExcessAmount.DecValue);
                Ccis.SetNewValue(CciClientId(CciEnum.netTurnOverInExcessAmount_amount), netTurnOverExcessAmount.Value);
                Ccis.SetNewValue(CciClientId(CciEnum.netTurnOverInExcessAmount_currency), TurnOverCurrency);
            }
            else
            {
                Ccis.SetNewValue(CciClientId(CciEnum.netTurnOverInExcessAmount_amount), string.Empty);
                Ccis.SetNewValue(CciClientId(CciEnum.netTurnOverInExcessAmount_currency), string.Empty);
            }
        }
        #endregion CalculNetTurnOverExcessAmount
        #region DisplayAndCalculCapDetails
        private void DisplayAndCalculCapDetails()
        {
            if (m_CciInvoiceRebate.RebateConditions.CapConditionsSpecified)
            {
                if ((PeriodEnum.T != RebateCapParameters.ApplicationPeriod.Period) &&
                    (1 != RebateCapParameters.ApplicationPeriod.PeriodMultiplier.DecValue))
                {
                    Ccis.SetNewValue(CciClientId(CciEnum.sumOfNetTurnOverPreviousPeriodAmount_amount), "0");
                    Ccis.SetNewValue(CciClientId(CciEnum.sumOfNetTurnOverPreviousPeriodAmount_currency), TurnOverCurrency);
                }
                else
                {
                    Ccis.SetNewValue(CciClientId(CciEnum.sumOfNetTurnOverPreviousPeriodAmount_amount), string.Empty);
                    Ccis.SetNewValue(CciClientId(CciEnum.sumOfNetTurnOverPreviousPeriodAmount_currency), string.Empty);
                }
                Ccis.SetNewValue(CciClientId(CciEnum.maximumNetTurnOverAmount_currency), TurnOverCurrency);
            }
            else
            {
                Ccis.SetNewValue(CciClientId(CciEnum.maximumNetTurnOverAmount_amount), string.Empty);
                Ccis.SetNewValue(CciClientId(CciEnum.maximumNetTurnOverAmount_currency), string.Empty);
                Ccis.SetNewValue(CciClientId(CciEnum.sumOfNetTurnOverPreviousPeriodAmount_amount), string.Empty);
                Ccis.SetNewValue(CciClientId(CciEnum.sumOfNetTurnOverPreviousPeriodAmount_currency), string.Empty);
            }
        }
        #endregion DisplayAndCalculCapDetails
        #endregion Methods
    }
}