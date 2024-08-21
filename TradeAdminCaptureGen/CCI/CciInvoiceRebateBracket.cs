#region Using Directives
using EFS.ACommon;
using EFS.Common;
using EFS.Common.Web;
using EFS.GUI.CCI;
using EFS.GUI.Interface;
using EfsML.Enum;
using EfsML.Enum.Tools;
using EfsML.Interface;
using FpML.Enum;
using System;
using System.Collections;
#endregion Using Directives

namespace EFS.TradeInformation
{
    /// <summary>
    /// Description résumée de CciInvoiceRebateBracket. 
    /// </summary>
    public class CciInvoiceRebateBracket : IContainerCciFactory, IContainerCci
    {
        #region Enums
        #region CciEnum
        public enum CciEnum
        {
            #region RebateBracketParameters
            [System.Xml.Serialization.XmlEnumAttribute("parameters.applicationPeriod.periodMultiplier")]
            applicationPeriod_periodMultiplier,
            [System.Xml.Serialization.XmlEnumAttribute("parameters.applicationPeriod.period")]
            applicationPeriod_period,
            [System.Xml.Serialization.XmlEnumAttribute("parameters.managementType")]
            managementType,
            #endregion RebateBracketParameters
            #region RebateBracketResult
            [System.Xml.Serialization.XmlEnumAttribute("result.sumOfGrossTurnOverPreviousPeriodAmount.amount")]
            sumOfGrossTurnOverPreviousPeriodAmount_amount,
            [System.Xml.Serialization.XmlEnumAttribute("result.sumOfGrossTurnOverPreviousPeriodAmount.currency")]
            sumOfGrossTurnOverPreviousPeriodAmount_currency,
            [System.Xml.Serialization.XmlEnumAttribute("result.sumOfNetTurnOverPreviousPeriodAmount.amount")]
            sumOfNetTurnOverPreviousPeriodAmount_amount,
            [System.Xml.Serialization.XmlEnumAttribute("result.sumOfNetTurnOverPreviousPeriodAmount.currency")]
            sumOfNetTurnOverPreviousPeriodAmount_currency,
            [System.Xml.Serialization.XmlEnumAttribute("result.totalRebateBracketAmount.amount")]
            totalRebateBracketAmount_amount,
            [System.Xml.Serialization.XmlEnumAttribute("result.totalRebateBracketAmount.currency")]
            totalRebateBracketAmount_currency,
            #endregion RebateBracketResult
            unknown,
        }
        #endregion CciEnum
        #endregion Enums
        #region Members
        private readonly CciInvoiceRebate m_CciInvoiceRebate;
        private IRebateBracketConditions m_RebateBracketConditions;
        private readonly string m_Prefix;
        private readonly TradeAdminCustomCaptureInfos m_Ccis;
        private CciInvoiceRebateBracketCalculation[] m_CciInvoiceRebateBracketCalculation;
        #endregion Members
        #region Accessors
        #region Ccis
        public TradeAdminCustomCaptureInfos Ccis
        {
            get { return m_Ccis; }
        }
        #endregion Ccis
        #region IsRebateBracketConditionsSpecified
        public bool IsRebateBracketConditionsSpecified
        {
            get 
            {
                bool isSpecified = false;
                if (null != m_CciInvoiceRebate.RebateConditions)
                    isSpecified = m_CciInvoiceRebate.RebateConditions.BracketConditionsSpecified;
                return isSpecified; 
            }
        }
        #endregion RebateBracketConditions
        #region RebateBracketConditions
        public IRebateBracketConditions RebateBracketConditions
        {
            get { return m_RebateBracketConditions; }
            set { m_RebateBracketConditions = value; }
        }
        #endregion RebateBracketConditions
        #region RebateBracketCalculation
        /*
        public IRebateBracketCalculation[] RebateBracketCalculation
        {
            get 
            {
                return RebateBracketCalculations.rebateBracketCalculation;
            }
        }
        */
        #endregion RebateBracketCalculation
        #region RebateBracketCalculations
        public IRebateBracketCalculations RebateBracketCalculations
        {
            get { return m_RebateBracketConditions.Result.Calculations;}
        }
        #endregion RebateBracketCalculations
        #region RebateBracketCalculationLength
        public int RebateBracketCalculationLength
        {
            get { return ArrFunc.IsFilled(m_CciInvoiceRebateBracketCalculation) ? m_CciInvoiceRebateBracketCalculation.Length : 0; }
        }
        #endregion RebateBracketCalculationLength
        #region RebateBracketParameters
        public IRebateBracketParameters RebateBracketParameters
        {
            get { return m_RebateBracketConditions.Parameters; }
            set { m_RebateBracketConditions.Parameters = value; }
        }
        #endregion RebateBracketParameters
        #region RebateBracketResult
        public IRebateBracketResult RebateBracketResult
        {
            get { return m_RebateBracketConditions.Result; }
            set { m_RebateBracketConditions.Result = value; }
        }
        #endregion RebateBracketResult
        #region TurnOverCurrency
        private string TurnOverCurrency
        {
            get { return m_CciInvoiceRebate.Invoice.GrossTurnOverAmount.Currency; }
        }
        #endregion TurnOverCurrency
        #endregion Accessors
        #region Constructors
        public CciInvoiceRebateBracket(CciInvoiceRebate pCciInvoiceRebate, string pPrefix, IRebateBracketConditions pRebateBracketConditions)
        {
            m_CciInvoiceRebate = pCciInvoiceRebate;
            m_Ccis = pCciInvoiceRebate.Ccis;
            m_Prefix = pPrefix + CustomObject.KEY_SEPARATOR;
            m_RebateBracketConditions = pRebateBracketConditions;
        }
        #endregion Constructors
        #region Interface
        #region IContainerCciFactory Members
        #region AddCciSystem
        public void AddCciSystem()
        {
            for (int i = 0; i < RebateBracketCalculationLength; i++)
                m_CciInvoiceRebateBracketCalculation[i].AddCciSystem();
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
                            m_CciInvoiceRebate.RebateConditions.BracketConditionsSpecified = cci.IsFilled;
                            if (m_CciInvoiceRebate.RebateConditions.BracketConditionsSpecified)
                                RebateBracketParameters.ApplicationPeriod.Period = StringToEnum.Period(data);
                            processQueue = CustomCaptureInfosBase.ProcessQueueEnum.Low;
                            #endregion ApplicationPeriod (Period)
                            break;
                        case CciEnum.applicationPeriod_periodMultiplier:
                            #region ApplicationPeriod (PeriodMultiplier)
                            m_CciInvoiceRebate.RebateConditions.BracketConditionsSpecified = cci.IsFilled;
                            if (m_CciInvoiceRebate.RebateConditions.BracketConditionsSpecified)
                                RebateBracketParameters.ApplicationPeriod.PeriodMultiplier.Value = data;
                            processQueue = CustomCaptureInfosBase.ProcessQueueEnum.Low;
                            #endregion ApplicationPeriod (PeriodMultiplier)
                            break;
                        case CciEnum.managementType:
                            #region ManagementType
                            if (m_CciInvoiceRebate.RebateConditions.BracketConditionsSpecified)
                                RebateBracketParameters.ManagementType = (BracketApplicationEnum)StringToEnum.Parse(data, BracketApplicationEnum.Unit);
                            processQueue = CustomCaptureInfosBase.ProcessQueueEnum.Low;
                            #endregion ManagementType
                            break;
                        case CciEnum.sumOfGrossTurnOverPreviousPeriodAmount_amount:
                            #region SumOfGrossTurnOverPreviousPeriodAmount (Amount)
                            RebateBracketResult.SumOfGrossTurnOverPreviousPeriodAmountSpecified = cci.IsFilledValue;
                            RebateBracketResult.SumOfGrossTurnOverPreviousPeriodAmount.Amount.Value = data;
                            #endregion SumOfGrossTurnOverPreviousPeriodAmount (Amount)
                            break;
                        case CciEnum.sumOfGrossTurnOverPreviousPeriodAmount_currency:
                            #region SumOfGrossTurnOverPreviousPeriodAmount (Currency)
                            RebateBracketResult.SumOfGrossTurnOverPreviousPeriodAmount.Currency = data;
                            #endregion SumOfGrossTurnOverPreviousPeriodAmount (Currency)
                            break;
                        case CciEnum.sumOfNetTurnOverPreviousPeriodAmount_amount:
                            #region SumOfNetTurnOverPreviousPeriodAmount (Amount)
                            RebateBracketResult.SumOfNetTurnOverPreviousPeriodAmountSpecified = cci.IsFilledValue;
                            RebateBracketResult.SumOfNetTurnOverPreviousPeriodAmount.Amount.Value = data;
                            #endregion SumOfNetTurnOverPreviousPeriodAmount (Amount)
                            break;
                        case CciEnum.sumOfNetTurnOverPreviousPeriodAmount_currency:
                            #region SumOfNetTurnOverPreviousPeriodAmount (Currency)
                            RebateBracketResult.SumOfNetTurnOverPreviousPeriodAmount.Currency = data;
                            #endregion SumOfNetTurnOverPreviousPeriodAmount (Currency)
                            break;
                        case CciEnum.totalRebateBracketAmount_amount:
                            #region FinalAmount (Amount)
                            RebateBracketResult.TotalRebateBracketAmountSpecified = cci.IsFilledValue;
                            RebateBracketResult.TotalRebateBracketAmount.Amount.Value = data;
                            #endregion FinalAmount (Amount)
                            break;
                        case CciEnum.totalRebateBracketAmount_currency:
                            #region FinalAmount (Currency)
                            RebateBracketResult.TotalRebateBracketAmount.Currency = data;
                            #endregion FinalAmount (Currency)
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
            for (int i = 0; i < RebateBracketCalculationLength; i++)
                m_CciInvoiceRebateBracketCalculation[i].Dump_ToDocument();
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
            CciTools.CreateInstance(this, m_RebateBracketConditions);
            InitializeRebateBracketCalculation_FromCci();
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
                            if (IsRebateBracketConditionsSpecified)
                                data = RebateBracketParameters.ApplicationPeriod.PeriodMultiplier.Value;
                            #endregion ApplicationPeriod (Multiplier)
                            break;
                        case CciEnum.applicationPeriod_period:
                            #region ApplicationPeriod (Period)
                            if (IsRebateBracketConditionsSpecified)
                                data = RebateBracketParameters.ApplicationPeriod.Period.ToString();
                            #endregion ApplicationPeriod (Period)
                            break;
                        case CciEnum.managementType:
                            #region ManagementType
                            if (IsRebateBracketConditionsSpecified) 
                                data = RebateBracketParameters.ManagementType.ToString();
                            #endregion ManagementType
                            break;
                        case CciEnum.sumOfGrossTurnOverPreviousPeriodAmount_amount:
                            #region SumOfGrossTurnOverPreviousPeriodAmount (Amount)
                            if (RebateBracketResult.SumOfGrossTurnOverPreviousPeriodAmountSpecified)
                                data = RebateBracketResult.SumOfGrossTurnOverPreviousPeriodAmount.Amount.Value;
                            #endregion SumOfGrossTurnOverPreviousPeriodAmount (Amount)
                            break;
                        case CciEnum.sumOfGrossTurnOverPreviousPeriodAmount_currency:
                            #region SumOfGrossTurnOverPreviousPeriodAmount (Currency)
                            if (RebateBracketResult.SumOfGrossTurnOverPreviousPeriodAmountSpecified)
                                data = RebateBracketResult.SumOfGrossTurnOverPreviousPeriodAmount.Currency;
                            #endregion SumOfGrossTurnOverPreviousPeriodAmount (Currency)
                            break;
                        case CciEnum.sumOfNetTurnOverPreviousPeriodAmount_amount:
                            #region SumOfNetTurnOverPreviousPeriodAmount (Amount)
                            if (RebateBracketResult.SumOfNetTurnOverPreviousPeriodAmountSpecified)
                                data = RebateBracketResult.SumOfNetTurnOverPreviousPeriodAmount.Amount.Value;
                            #endregion SumOfNetTurnOverPreviousPeriodAmount (Amount)
                            break;
                        case CciEnum.sumOfNetTurnOverPreviousPeriodAmount_currency:
                            #region SumOfNetTurnOverPreviousPeriodAmount (Currency)
                            if (RebateBracketResult.SumOfNetTurnOverPreviousPeriodAmountSpecified)
                                data = RebateBracketResult.SumOfNetTurnOverPreviousPeriodAmount.Currency;
                            #endregion SumOfNetTurnOverPreviousPeriodAmount (Currency)
                            break;
                        case CciEnum.totalRebateBracketAmount_amount:
                            #region FinalAmount (Amount)
                            if (RebateBracketResult.TotalRebateBracketAmountSpecified)
                                data = RebateBracketResult.TotalRebateBracketAmount.Amount.Value;
                            #endregion FinalAmount (Amount)
                            break;
                        case CciEnum.totalRebateBracketAmount_currency:
                            #region FinalAmount (Currency)
                            if (RebateBracketResult.TotalRebateBracketAmountSpecified) 
                                data = RebateBracketResult.TotalRebateBracketAmount.Currency;
                            #endregion FinalAmount (Currency)
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
            for (int i = 0; i < RebateBracketCalculationLength; i++)
                m_CciInvoiceRebateBracketCalculation[i].Initialize_FromDocument();
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
                        DisplayAndCalculBracketDetails();
                        #endregion ApplicationPeriod
                        break;
                    default:
                        #region Default
                        #endregion Default
                        break;
                }
            }
            for (int i = 0; i < RebateBracketCalculationLength; i++)
                m_CciInvoiceRebateBracketCalculation[i].ProcessInitialize(pCci);

        }
        #endregion ProcessInitialize
        #region RefreshCciEnabled
        public void RefreshCciEnabled()
        {
            bool bracketConditionSpecified = m_CciInvoiceRebate.RebateConditions.BracketConditionsSpecified;
            bool sumOfGrossTurnOverPreviousPeriodAmountSpecified = bracketConditionSpecified && RebateBracketResult.SumOfGrossTurnOverPreviousPeriodAmountSpecified;
            bool sumOfNetTurnOverPreviousPeriodAmountSpecified = bracketConditionSpecified && RebateBracketResult.SumOfNetTurnOverPreviousPeriodAmountSpecified;

            Ccis.Set(CciClientId(CciEnum.managementType), "IsEnabled", bracketConditionSpecified);
            Ccis.Set(CciClientId(CciEnum.sumOfGrossTurnOverPreviousPeriodAmount_amount), "IsEnabled", sumOfGrossTurnOverPreviousPeriodAmountSpecified && bracketConditionSpecified);
            Ccis.Set(CciClientId(CciEnum.sumOfGrossTurnOverPreviousPeriodAmount_currency), "IsEnabled", sumOfGrossTurnOverPreviousPeriodAmountSpecified && bracketConditionSpecified);
            Ccis.Set(CciClientId(CciEnum.sumOfNetTurnOverPreviousPeriodAmount_amount), "IsEnabled", sumOfNetTurnOverPreviousPeriodAmountSpecified && bracketConditionSpecified);
            Ccis.Set(CciClientId(CciEnum.sumOfNetTurnOverPreviousPeriodAmount_currency), "IsEnabled", sumOfNetTurnOverPreviousPeriodAmountSpecified && bracketConditionSpecified);
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
        #region CalculRebateBracketAmount
        public void CalculRebateBracketAmount()
        {
            EFS_Decimal totalRebateBracketAmount = new EFS_Decimal(0);
            Ccis.SetNewValue(CciClientId(CciEnum.totalRebateBracketAmount_amount), totalRebateBracketAmount.Value);
        }
        #endregion CalculRebateBracketAmount
        #region DisplayAndCalculBracketDetails
        private void DisplayAndCalculBracketDetails()
        {
            if (m_CciInvoiceRebate.RebateConditions.BracketConditionsSpecified)
            {
                if ((PeriodEnum.T != RebateBracketParameters.ApplicationPeriod.Period) &&
                    (1 != RebateBracketParameters.ApplicationPeriod.PeriodMultiplier.DecValue))
                {
                    Ccis.SetNewValue(CciClientId(CciEnum.sumOfGrossTurnOverPreviousPeriodAmount_amount), "0");
                    Ccis.SetNewValue(CciClientId(CciEnum.sumOfGrossTurnOverPreviousPeriodAmount_currency), TurnOverCurrency);
                    Ccis.SetNewValue(CciClientId(CciEnum.sumOfNetTurnOverPreviousPeriodAmount_amount), "0");
                    Ccis.SetNewValue(CciClientId(CciEnum.sumOfNetTurnOverPreviousPeriodAmount_currency), TurnOverCurrency);
                }
                else 
                {
                    Ccis.SetNewValue(CciClientId(CciEnum.sumOfGrossTurnOverPreviousPeriodAmount_amount), string.Empty);
                    Ccis.SetNewValue(CciClientId(CciEnum.sumOfGrossTurnOverPreviousPeriodAmount_currency), string.Empty);
                    Ccis.SetNewValue(CciClientId(CciEnum.sumOfNetTurnOverPreviousPeriodAmount_amount), string.Empty);
                    Ccis.SetNewValue(CciClientId(CciEnum.sumOfNetTurnOverPreviousPeriodAmount_currency), string.Empty);
                }
                Ccis.SetNewValue(CciClientId(CciEnum.totalRebateBracketAmount_currency), TurnOverCurrency);
            }
            else
            {
                Ccis.SetNewValue(CciClientId(CciEnum.sumOfGrossTurnOverPreviousPeriodAmount_amount), string.Empty);
                Ccis.SetNewValue(CciClientId(CciEnum.sumOfGrossTurnOverPreviousPeriodAmount_currency), string.Empty);
                Ccis.SetNewValue(CciClientId(CciEnum.sumOfNetTurnOverPreviousPeriodAmount_amount), string.Empty);
                Ccis.SetNewValue(CciClientId(CciEnum.sumOfNetTurnOverPreviousPeriodAmount_currency), string.Empty);
            }
        }
        #endregion DisplayAndCalculBracketDetails
        #region GetRebateBracketCalculation
        public IRebateBracketCalculation[] GetRebateBracketCalculation()
        {
            return RebateBracketCalculations.RebateBracketCalculation;
        }
        #endregion RebateBracketCalculation

        #region InitializeRebateBracketCalculation_FromCci
        private void InitializeRebateBracketCalculation_FromCci()
        {
            bool isOk = true;
            int index = -1;
            ArrayList lst = new ArrayList();
            while (isOk)
            {
                index += 1;
                CciInvoiceRebateBracketCalculation cciInvoiceRebateBracketCalculation = new CciInvoiceRebateBracketCalculation(this, m_Prefix + TradeAdminCustomCaptureInfos.CCst.Prefix_InvoiceRebateBracketCalculation, index + 1, null);
                //
                isOk = Ccis.Contains(cciInvoiceRebateBracketCalculation.CciClientId(CciInvoiceRebateBracketCalculation.CciEnum.amount));
                if (isOk)
                {
                    if (ArrFunc.IsEmpty(GetRebateBracketCalculation()) || (index == GetRebateBracketCalculation().Length))
                    {
                        ReflectionTools.AddItemInArray(RebateBracketCalculations, "rebateBracketCalculation", index);
                    }
                    cciInvoiceRebateBracketCalculation.RebateBracketCalculation = GetRebateBracketCalculation()[index];
                    lst.Add(cciInvoiceRebateBracketCalculation);
                }
            }
            m_CciInvoiceRebateBracketCalculation = (CciInvoiceRebateBracketCalculation[])lst.ToArray(typeof(CciInvoiceRebateBracketCalculation));
            //
            for (int i = 0; i < RebateBracketCalculationLength; i++)
                m_CciInvoiceRebateBracketCalculation[i].Initialize_FromCci();
        }
        #endregion InitializeRebateBracketCalculation_FromCci
        #endregion Methods
    }
}