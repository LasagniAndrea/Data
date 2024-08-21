#region Using Directives
using EFS.ACommon;
using EFS.ApplicationBlocks.Data;
using EFS.Common;
using EFS.GUI.CCI;
using EFS.GUI.Interface;
using EfsML.Business;
using EfsML.Enum.Tools;
using FpML.Enum;
using FpML.Interface;
using System;
using System.Linq;
#endregion Using Directives

namespace EFS.TradeInformation
{
    /// <summary>
    /// Description résumée de TradeEquityValuation.
    /// </summary>
    public class CciReturnLegValuationPrice : ContainerCciBase, IContainerCciFactory
    {
        #region Members
        private IReturnLegValuationPrice _returnLegValuationPrice;
        /// <summary>
        /// Représente le returnLeg
        /// </summary>
        private readonly CciReturnLeg _cciReturnLeg;
        private readonly CciTradeBase _cciTrade;
        
        #endregion Members

        #region Enum
        private enum ActualPriceTypeEnum
        {
            initialPrice,
            priceInterim,
            priceFinal,
        }

        public enum CciEnum
        {
            //type of Price
            [System.Xml.Serialization.XmlEnumAttribute("priceDeterminationMethod.rateSource")]
            determinationMethod,
            [System.Xml.Serialization.XmlEnumAttribute("priceAmountRelativeTo.rateSource")]
            amountRelativeTo,
            [System.Xml.Serialization.XmlEnumAttribute("priceNetPrice.currency")]
            netPrice_currency,
            [System.Xml.Serialization.XmlEnumAttribute("priceNetPrice.amount")]
            netPrice_amount,
            [System.Xml.Serialization.XmlEnumAttribute("priceNetPrice.priceExpression")]
            netPrice_priceExpression,
            
            //commission Of Price
            [System.Xml.Serialization.XmlEnumAttribute("commission.commissionDenomination")]
            commission_commissionDenomination,
            [System.Xml.Serialization.XmlEnumAttribute("commission.commissionAmount")]
            commission_commissionAmount,
            [System.Xml.Serialization.XmlEnumAttribute("commission.currency")]
            commission_currency,

            //ValuationRules (AdjustableDate)
            [System.Xml.Serialization.XmlEnumAttribute("valuationRules.valuationDate.adjustableDateOrRelativeDateSequenceAdjustableDate.unadjustedDate")]
            valuationDate_adjustableDate_unadjustedDate,
            [System.Xml.Serialization.XmlEnumAttribute("valuationRules.valuationDate.adjustableDateOrRelativeDateSequenceAdjustableDate.dateAdjustments.businessDayConvention")]
            valuationDate_adjustableDate_dateAdjustments_bDC,

            //ValuationRules (RelativeDateSequence)			
            [System.Xml.Serialization.XmlEnumAttribute("valuationRules.valuationDate.adjustableOrRelativeDateSequenceRelativeDateSequence.dateRelativeTo")]
            valuationDate_relativeDateSequence_dateRelativeTo,
            [System.Xml.Serialization.XmlEnumAttribute("valuationRules.valuationDate.adjustableOrRelativeDateSequenceRelativeDateSequence.dateOffset.periodMultiplier")]
            valuationDate_relativeDateSequence_periodMultiplier,
            [System.Xml.Serialization.XmlEnumAttribute("valuationRules.valuationDate.adjustableOrRelativeDateSequenceRelativeDateSequence.dateOffset.period")]
            valuationDate_relativeDateSequence_period,
            [System.Xml.Serialization.XmlEnumAttribute("valuationRules.valuationDate.adjustableOrRelativeDateSequenceRelativeDateSequence.dateOffset.dayType")]
            valuationDate_relativeDateSequence_dayType,
            [System.Xml.Serialization.XmlEnumAttribute("valuationRules.valuationDate.adjustableOrRelativeDateSequenceRelativeDateSequence.dateOffset.businessDayConvention")]
            valuationDate_relativeDateSequence_bDC,

            //ValuationRules (PeriodicDates)			
            [System.Xml.Serialization.XmlEnumAttribute("valuationRules.valuationDates.adjustableRelativeOrPeriodicPeriodicDates.calculationPeriodFrequency.periodMultiplier")]
            valuationDates_periodicDates_calculationPeriodFrequency_periodMultiplier,
            [System.Xml.Serialization.XmlEnumAttribute("valuationRules.valuationDates.adjustableRelativeOrPeriodicPeriodicDates.calculationPeriodFrequency.period")]
            valuationDates_periodicDates_calculationPeriodFrequency_period,
            [System.Xml.Serialization.XmlEnumAttribute("valuationRules.valuationDates.adjustableRelativeOrPeriodicPeriodicDates.calculationPeriodFrequency.rollConvention")]
            valuationDates_periodicDates_calculationPeriodFrequency_rollConvention,
            [System.Xml.Serialization.XmlEnumAttribute("valuationRules.valuationDates.adjustableRelativeOrPeriodicPeriodicDates.calculationPeriodDatesAdjustments.businessDayConvention")]
            valuationDates_periodicDates_calculationPeriodDatesAdjustments_bDC,

            unknown
        }
        #endregion Enum

        #region Accessors
        private ActualPriceTypeEnum ActualPriceType
        {
            get 
            {
                ActualPriceTypeEnum _actualPriceTypeEnum = ActualPriceTypeEnum.initialPrice;
                if (Prefix.Contains(ActualPriceTypeEnum.priceInterim.ToString()))
                    _actualPriceTypeEnum = ActualPriceTypeEnum.priceInterim;
                else if (Prefix.Contains(ActualPriceTypeEnum.priceFinal.ToString()))
                    _actualPriceTypeEnum = ActualPriceTypeEnum.priceFinal;
                return _actualPriceTypeEnum; 
            }
        }
        #endregion Accessors
        #region Constructor
        public CciReturnLegValuationPrice(CciTradeBase pCciTrade, CciReturnLeg pCciReturnLeg, IReturnLegValuationPrice pReturnLegValuationPrice, string pPrefix) 
            : base(pPrefix, pCciTrade.Ccis)
        {
            _cciTrade = pCciTrade;
            _cciReturnLeg = pCciReturnLeg;
            _returnLegValuationPrice = pReturnLegValuationPrice;
        }
        #endregion Constructor

        #region public property
        public TradeCustomCaptureInfos Ccis
        {
            get { return base.CcisBase as TradeCustomCaptureInfos; }
        }
        public bool IsCommissionFixedAmount
        {
            get
            {
                return (Cci(CciEnum.commission_commissionDenomination).NewValue == CommissionDenominationEnum.FixedAmount.ToString());
            }
        }
        public bool IsCommissionPercentage
        {
            get
            {
                return (Cci(CciEnum.commission_commissionDenomination).NewValue == CommissionDenominationEnum.Percentage.ToString());
            }
        }
        public bool IsPriceExpressionAbsoluteTerms
        {
            get
            {
                return (Cci(CciEnum.netPrice_priceExpression).NewValue == PriceExpressionEnum.AbsoluteTerms.ToString());
            }
        }
        public bool IsSpecified
        {
            get
            {
                return Cci(CciEnum.determinationMethod).IsFilled ||
                       Cci(CciEnum.netPrice_priceExpression).IsFilled ||
                       Cci(CciEnum.amountRelativeTo).IsFilled;
            }
        }

        public IReturnLegValuationPrice ReturnLegValuationPrice
        {
            set { _returnLegValuationPrice = value; }
            get { return _returnLegValuationPrice; }
        }
        #endregion public property

        #region Membres de IContainerCciFactory
        #region Initialize_FromCci
        public void Initialize_FromCci()
        {
            CciTools.CreateInstance(this, _returnLegValuationPrice);
            // L'interface gère potentiellement les periodicDates => création des instances si Nulls
            if (CcisBase.Contains(CciClientId(CciEnum.valuationDates_periodicDates_calculationPeriodFrequency_periodMultiplier)))
            {
                if (null == _returnLegValuationPrice.ValuationRules)
                    _returnLegValuationPrice.ValuationRules = _returnLegValuationPrice.CreateValuationRules();
                if (null == _returnLegValuationPrice.ValuationRules.ValuationDates)
                    _returnLegValuationPrice.ValuationRules.ValuationDates = _returnLegValuationPrice.CreateAdjustableRelativeOrPeriodicDates();
                if (null == _returnLegValuationPrice.ValuationRules.ValuationDates.PeriodicDates)
                    _returnLegValuationPrice.ValuationRules.ValuationDates.PeriodicDates = _returnLegValuationPrice.CreatePeriodicDates();
            }
            if (CcisBase.Contains(CciClientId(CciEnum.netPrice_currency)))
            {
                _returnLegValuationPrice.NetPrice.Currency = _returnLegValuationPrice.CreateCurrency();
            }
        }
        #endregion Initialize_FromCci
        #region AddCciSystem
        /// <summary>
        /// 
        /// </summary>
        /// FI 20170116 [21916] Modify (use AddCciSystem Method)
        /// FI 20170518 [XXXXX] Modify  
        public void AddCciSystem()
        {

            CciTools.AddCciSystem(CcisBase, Cst.DDL + CciClientId(CciEnum.commission_commissionDenomination), false, TypeData.TypeDataEnum.@string);
            CciTools.AddCciSystem(CcisBase, Cst.DDL + CciClientId(CciEnum.netPrice_priceExpression), false, TypeData.TypeDataEnum.@string);

            CciTools.AddCciSystem(CcisBase, Cst.TXT + CciClientId(CciEnum.netPrice_amount), false, TypeData.TypeDataEnum.@decimal);
            Cci(CciEnum.netPrice_amount).DataType = TypeData.TypeDataEnum.@decimal;

            CciTools.AddCciSystem(CcisBase, Cst.DDL + CciClientId(CciEnum.netPrice_currency), false, TypeData.TypeDataEnum.@string);
            CciTools.AddCciSystem(CcisBase, Cst.DDL + CciClientId(CciEnum.determinationMethod), false, TypeData.TypeDataEnum.@string);
            CciTools.AddCciSystem(CcisBase, Cst.DDL + CciClientId(CciEnum.amountRelativeTo), false, TypeData.TypeDataEnum.@string);
            // S'il existe calculationPeriodFrequency_periodMultiplier => On est sur periodicDates (Ex valuation Interim) => Il faut une Business Convention
            // FI 20170518 [XXXXX] add du if (avait été supprimé par erreur lors d'un refactoring)
            // pas de ticket => Pb rencontré lors de la recette  
            if (CcisBase.Contains(CciClientId(CciEnum.valuationDates_periodicDates_calculationPeriodFrequency_periodMultiplier)))
                CciTools.AddCciSystem(CcisBase, Cst.DDL + CciClientId(CciEnum.valuationDates_periodicDates_calculationPeriodFrequency_periodMultiplier), true, TypeData.TypeDataEnum.@string);
            CciTools.AddCciSystem(CcisBase, Cst.DDL + CciClientId(CciEnum.valuationDates_periodicDates_calculationPeriodDatesAdjustments_bDC), false, TypeData.TypeDataEnum.@string);
        }
        #endregion AddCciSystem
        #region Initialize_FromDocument
        public void Initialize_FromDocument()
        {
            foreach (CciEnum cciEnum in Enum.GetValues(typeof(CciEnum)))
            {
                CustomCaptureInfo cci = Cci(cciEnum);
                if (cci != null)
                {
                    #region Reset variables
                    string data = string.Empty;
                    bool isSetting = true;
                    SQL_Table sql_Table = null;
                    #endregion
                    
                    switch (cciEnum)
                    {
                        #region commission
                        case CciEnum.commission_commissionDenomination:
                            if (_returnLegValuationPrice.CommissionSpecified)
                                data = _returnLegValuationPrice.Commission.CommissionDenomination.ToString();
                            break;
                        case CciEnum.commission_commissionAmount:
                            if (_returnLegValuationPrice.CommissionSpecified)
                                data = _returnLegValuationPrice.Commission.Amount.Value;
                            break;
                        case CciEnum.commission_currency:
                            if (_returnLegValuationPrice.CommissionSpecified && _returnLegValuationPrice.Commission.CurrencySpecified)
                                data = _returnLegValuationPrice.Commission.Currency;
                            break;
                        #endregion commission

                        #region DeterminationMethod
                        case CciEnum.determinationMethod:
                            if (_returnLegValuationPrice.DeterminationMethodSpecified)
                                data = _returnLegValuationPrice.DeterminationMethod;
                            break;
                        #endregion DeterminationMethod

                        #region amountRelativeTo
                        case CciEnum.amountRelativeTo:
                            if (_returnLegValuationPrice.AmountRelativeToSpecified)
                                data = _returnLegValuationPrice.AmountRelativeTo.HRef;
                            break;
                        #endregion amountRelativeTo

                        #region netprice
                        case CciEnum.netPrice_priceExpression:
                            if (_returnLegValuationPrice.NetPriceSpecified)
                                data = _returnLegValuationPrice.NetPrice.PriceExpression.ToString();
                            break;

                        case CciEnum.netPrice_currency:
                            if (_returnLegValuationPrice.NetPriceSpecified && _returnLegValuationPrice.NetPrice.CurrencySpecified)
                                data = _returnLegValuationPrice.NetPrice.Currency.Value;
                            break;
                        case CciEnum.netPrice_amount:
                            if (_returnLegValuationPrice.NetPriceSpecified)
                                data = _returnLegValuationPrice.NetPrice.Amount.Value;
                            break;
                        #endregion netprice

                        #region valuationDate_adjustableDate
                        case CciEnum.valuationDate_adjustableDate_unadjustedDate:
                            if (_returnLegValuationPrice.ValuationRulesSpecified &&
                                _returnLegValuationPrice.ValuationRules.ValuationDateSpecified && 
                                _returnLegValuationPrice.ValuationRules.ValuationDate.AdjustableDateSpecified)
                                    data = _returnLegValuationPrice.ValuationRules.ValuationDate.AdjustableDate.UnadjustedDate.Value;
                            break;

                        case CciEnum.valuationDate_adjustableDate_dateAdjustments_bDC:
                            if (_returnLegValuationPrice.ValuationRulesSpecified &&
                                _returnLegValuationPrice.ValuationRules.ValuationDateSpecified &&
                                _returnLegValuationPrice.ValuationRules.ValuationDate.AdjustableDateSpecified)
                                    data = _returnLegValuationPrice.ValuationRules.ValuationDate.AdjustableDate.DateAdjustments.BusinessDayConvention.ToString();
                            break;
                        #endregion valuationDate_adjustableDate

                        #region valuationDate_relativeDateSequence
                        case CciEnum.valuationDate_relativeDateSequence_dateRelativeTo:
                            if (_returnLegValuationPrice.ValuationRulesSpecified && 
                                _returnLegValuationPrice.ValuationRules.ValuationDateSpecified &&
                                _returnLegValuationPrice.ValuationRules.ValuationDate.RelativeDateSequenceSpecified)
                                    data = _returnLegValuationPrice.ValuationRules.ValuationDate.RelativeDateSequence.DateRelativeToValue;
                            break;
                        case CciEnum.valuationDate_relativeDateSequence_bDC:
                        case CciEnum.valuationDate_relativeDateSequence_dayType:
                        case CciEnum.valuationDate_relativeDateSequence_period:
                        case CciEnum.valuationDate_relativeDateSequence_periodMultiplier:
                            if ((_returnLegValuationPrice.ValuationRulesSpecified) && (_returnLegValuationPrice.ValuationRules.ValuationDateSpecified))
                                if (ArrFunc.IsFilled(_returnLegValuationPrice.ValuationRules.ValuationDate.RelativeDateSequence.DateOffset))
                                {
                                    switch (cciEnum)
                                    {
                                        case CciEnum.valuationDate_relativeDateSequence_bDC:
                                            data = _returnLegValuationPrice.ValuationRules.ValuationDate.RelativeDateSequence.DateOffset[0].BusinessDayConvention.ToString();
                                            break;
                                        case CciEnum.valuationDate_relativeDateSequence_dayType:
                                            data = _returnLegValuationPrice.ValuationRules.ValuationDate.RelativeDateSequence.DateOffset[0].DayType.ToString();
                                            break;
                                        case CciEnum.valuationDate_relativeDateSequence_period:
                                            data = _returnLegValuationPrice.ValuationRules.ValuationDate.RelativeDateSequence.DateOffset[0].Period.ToString();
                                            break;
                                        case CciEnum.valuationDate_relativeDateSequence_periodMultiplier:
                                            data = _returnLegValuationPrice.ValuationRules.ValuationDate.RelativeDateSequence.DateOffset[0].PeriodMultiplier.Value;
                                            break;
                                    }
                                }
                            break;
                        #endregion valuationDate_relativeDateSequence

                        #region valuationDates_periodicDates
                        case CciEnum.valuationDates_periodicDates_calculationPeriodFrequency_periodMultiplier:
                            if (_returnLegValuationPrice.ValuationRulesSpecified &&
                                _returnLegValuationPrice.ValuationRules.ValuationDatesSpecified &&
                                _returnLegValuationPrice.ValuationRules.ValuationDates.PeriodicDatesSpecified)
                                data = _returnLegValuationPrice.ValuationRules.ValuationDates.PeriodicDates.CalculationPeriodFrequency.Interval.PeriodMultiplier.Value;
                            break;
                        case CciEnum.valuationDates_periodicDates_calculationPeriodFrequency_period:
                            if (_returnLegValuationPrice.ValuationRulesSpecified &&
                                _returnLegValuationPrice.ValuationRules.ValuationDatesSpecified &&
                                _returnLegValuationPrice.ValuationRules.ValuationDates.PeriodicDatesSpecified)
                                data = _returnLegValuationPrice.ValuationRules.ValuationDates.PeriodicDates.CalculationPeriodFrequency.Interval.Period.ToString();
                            break;
                        case CciEnum.valuationDates_periodicDates_calculationPeriodFrequency_rollConvention:
                            if (_returnLegValuationPrice.ValuationRulesSpecified &&
                                _returnLegValuationPrice.ValuationRules.ValuationDatesSpecified &&
                                _returnLegValuationPrice.ValuationRules.ValuationDates.PeriodicDatesSpecified)
                                data = _returnLegValuationPrice.ValuationRules.ValuationDates.PeriodicDates.CalculationPeriodFrequency.RollConvention.ToString();
                            break;
                        case CciEnum.valuationDates_periodicDates_calculationPeriodDatesAdjustments_bDC:
                            if (_returnLegValuationPrice.ValuationRulesSpecified &&
                                _returnLegValuationPrice.ValuationRules.ValuationDatesSpecified &&
                                _returnLegValuationPrice.ValuationRules.ValuationDates.PeriodicDatesSpecified)
                                data = _returnLegValuationPrice.ValuationRules.ValuationDates.PeriodicDates.CalculationPeriodDatesAdjustments.BusinessDayConvention.ToString();
                            break;
                        #endregion valuationDates_periodicDates

                        #region default
                        default:
                            isSetting = false;
                            break;
                        #endregion
                    }

                    if (isSetting)
                        CcisBase.InitializeCci(cci, sql_Table, data);
                }
            }
        }
        #endregion Initialize_FromDocument
        #region Dump_ToDocument
        /// <summary>
        /// 
        /// </summary>
        /// FI 20150129 [20748] Modify
        public void Dump_ToDocument()
        {
            foreach (string clientId in CcisBase.ClientId_DumpToDocument.Where(x => IsCciOfContainer(x)))
            {
                string cliendId_Key = CciContainerKey(clientId);
                if (Enum.IsDefined(typeof(CciEnum), cliendId_Key))
                {
                    CustomCaptureInfo cci = CcisBase[clientId];
                    CciEnum cciEnum = (CciEnum)System.Enum.Parse(typeof(CciEnum), cliendId_Key);

                    CustomCaptureInfosBase.ProcessQueueEnum processQueue = CustomCaptureInfosBase.ProcessQueueEnum.None;
                    string clientId_Key = this.CciContainerKey(cci.ClientId_WithoutPrefix);
                    string data = cci.NewValue;
                    bool isSetting = true;

                    switch (cciEnum)
                    {
                        #region commission
                        case CciEnum.commission_commissionDenomination:
                            _returnLegValuationPrice.CommissionSpecified = cci.IsFilledValue;
                            if (_returnLegValuationPrice.CommissionSpecified)
                            {
                                if (cci.IsFilledValue)
                                {
                                    CommissionDenominationEnum cEnum = (CommissionDenominationEnum)System.Enum.Parse(typeof(CommissionDenominationEnum), data, true);
                                    _returnLegValuationPrice.Commission.CommissionDenomination = cEnum;
                                }
                            }
                            processQueue = CustomCaptureInfosBase.ProcessQueueEnum.Low;
                            break;
                        case CciEnum.commission_commissionAmount:
                            _returnLegValuationPrice.CommissionSpecified = cci.IsFilledValue;
                            _returnLegValuationPrice.Commission.Amount.Value = data;
                            processQueue = CustomCaptureInfosBase.ProcessQueueEnum.Low;  // Arrondi
                            break;
                        case CciEnum.commission_currency:
                            _returnLegValuationPrice.Commission.Currency = data;
                            processQueue = CustomCaptureInfosBase.ProcessQueueEnum.Low;
                            break;
                        #endregion commission
                        #region determination Method
                        case CciEnum.determinationMethod:
                            _returnLegValuationPrice.DeterminationMethodSpecified = cci.IsFilledValue;
                            if (_returnLegValuationPrice.DeterminationMethodSpecified)
                            {
                                _returnLegValuationPrice.DeterminationMethod = data;
                                _returnLegValuationPrice.AmountRelativeToSpecified = false;
                                _returnLegValuationPrice.NetPriceSpecified = false;
                            }
                            if (ActualPriceType == ActualPriceTypeEnum.priceInterim)
                            {
                                _cciReturnLeg.ReturnLeg.RateOfReturn.ValuationPriceInterimSpecified =
                                    _returnLegValuationPrice.NetPriceSpecified || _returnLegValuationPrice.DeterminationMethodSpecified || _returnLegValuationPrice.AmountRelativeToSpecified;
                            }
                            processQueue = CustomCaptureInfosBase.ProcessQueueEnum.Low;
                            break;
                        #endregion determination Method
                        #region amountRelativeTo
                        case CciEnum.amountRelativeTo:
                            _returnLegValuationPrice.AmountRelativeToSpecified = cci.IsFilledValue;
                            if (_returnLegValuationPrice.AmountRelativeToSpecified)
                            {
                                _returnLegValuationPrice.DeterminationMethodSpecified = false;
                                _returnLegValuationPrice.NetPriceSpecified = false;
                            }
                            _returnLegValuationPrice.AmountRelativeTo.HRef = data;
                            break;
                        #endregion determination Method
                        #region NetPrice
                        case CciEnum.netPrice_priceExpression:
                            _returnLegValuationPrice.NetPriceSpecified = cci.IsFilledValue;
                            if (_returnLegValuationPrice.NetPriceSpecified)
                            {
                                _returnLegValuationPrice.AmountRelativeToSpecified = false;
                                _returnLegValuationPrice.DeterminationMethodSpecified = false;
                            }
                            if (cci.IsFilledValue)
                            {
                                PriceExpressionEnum expEnum = (PriceExpressionEnum)System.Enum.Parse(typeof(PriceExpressionEnum), data, true);
                                _returnLegValuationPrice.NetPrice.PriceExpression = expEnum;
                            }
                            processQueue = CustomCaptureInfosBase.ProcessQueueEnum.Low;
                            break;
                        case CciEnum.netPrice_amount:
                            _returnLegValuationPrice.NetPriceSpecified = cci.IsFilledValue;
                            _returnLegValuationPrice.NetPrice.Amount = new EFS_Decimal(data);
                            processQueue = CustomCaptureInfosBase.ProcessQueueEnum.Low;
                            break;
                        case CciEnum.netPrice_currency:
                            _returnLegValuationPrice.NetPrice.CurrencySpecified = cci.IsFilledValue;
                            _returnLegValuationPrice.NetPrice.Currency.Value = data;
                            processQueue = CustomCaptureInfosBase.ProcessQueueEnum.Low;
                            break;

                        #endregion NetPrice
                        #region  valuationDate (AdjustableDate)
                        case CciEnum.valuationDate_adjustableDate_unadjustedDate:
                            _returnLegValuationPrice.ValuationRules.ValuationDate.AdjustableDateSpecified = cci.IsFilledValue;
                            if (cci.IsFilledValue)
                                _returnLegValuationPrice.ValuationRules.ValuationDate.AdjustableDate.UnadjustedDate.Value = data;
                            break;
                        #endregion
                        #region  valuationDate (RelativeDateSequence)
                        case CciEnum.valuationDate_relativeDateSequence_dateRelativeTo:
                            _returnLegValuationPrice.ValuationRules.ValuationDate.RelativeDateSequenceSpecified = cci.IsFilledValue;
                            if (cci.IsFilledValue)
                                _returnLegValuationPrice.ValuationRules.ValuationDate.RelativeDateSequence.DateRelativeToValue = data;
                            break;
                        case CciEnum.valuationDate_relativeDateSequence_bDC:
                            if (StrFunc.IsFilled(data)) //=>  Cette données est non obligatoire 
                            {
                                BusinessDayConventionEnum bdc = (BusinessDayConventionEnum)System.Enum.Parse(typeof(BusinessDayConventionEnum), data, true);
                                _returnLegValuationPrice.ValuationRules.ValuationDate.RelativeDateSequence.DateOffset[0].BusinessDayConvention = bdc;
                            }
                            break;
                        case CciEnum.valuationDate_relativeDateSequence_dayType:
                            _returnLegValuationPrice.ValuationRules.ValuationDate.RelativeDateSequence.DateOffset[0].DayTypeSpecified = cci.IsFilledValue;
                            if (cci.IsFilledValue)
                            {
                                DayTypeEnum daytype = (DayTypeEnum)System.Enum.Parse(typeof(DayTypeEnum), data, true);
                                _returnLegValuationPrice.ValuationRules.ValuationDate.RelativeDateSequence.DateOffset[0].DayType = daytype;
                            }
                            break;
                        case CciEnum.valuationDate_relativeDateSequence_period:
                            if (StrFunc.IsFilled(data))
                            {
                                PeriodEnum period = StringToEnum.Period(data);
                                _returnLegValuationPrice.ValuationRules.ValuationDate.RelativeDateSequence.DateOffset[0].Period = period;
                            }
                            break;
                        case CciEnum.valuationDate_relativeDateSequence_periodMultiplier:
                            _returnLegValuationPrice.ValuationRules.ValuationDateSpecified = cci.IsFilledValue;
                            _returnLegValuationPrice.ValuationRules.ValuationDate.RelativeDateSequenceSpecified = cci.IsFilledValue;
                            if (cci.IsFilledValue)
                                _returnLegValuationPrice.ValuationRules.ValuationDate.RelativeDateSequence.DateOffset[0].PeriodMultiplier.Value = data;
                            break;
                        #endregion
                        #region valuationdates periodicDates
                        case CciEnum.valuationDates_periodicDates_calculationPeriodFrequency_periodMultiplier:
                            _returnLegValuationPrice.ValuationRules.ValuationDatesSpecified = cci.IsFilledValue;
                            _returnLegValuationPrice.ValuationRules.ValuationDates.PeriodicDatesSpecified = cci.IsFilledValue;
                            if (cci.IsFilledValue)
                                _returnLegValuationPrice.ValuationRules.ValuationDates.PeriodicDates.CalculationPeriodFrequency.Interval.PeriodMultiplier.Value = data;
                            break;
                        case CciEnum.valuationDates_periodicDates_calculationPeriodFrequency_period:
                            if (StrFunc.IsFilled(data)) //=>  Cette données est non obligatoire 
                            {
                                PeriodEnum period = StringToEnum.Period(data);
                                _returnLegValuationPrice.ValuationRules.ValuationDates.PeriodicDates.CalculationPeriodFrequency.Interval.Period = period;
                            }

                            break;
                        case CciEnum.valuationDates_periodicDates_calculationPeriodFrequency_rollConvention:
                            if (StrFunc.IsFilled(data)) //=>  Cette données est non obligatoire 
                            {
                                RollConventionEnum rollConv = (RollConventionEnum)System.Enum.Parse(typeof(RollConventionEnum), data, true);
                                _returnLegValuationPrice.ValuationRules.ValuationDates.PeriodicDates.CalculationPeriodFrequency.RollConvention = rollConv;
                            }
                            break;
                        case CciEnum.valuationDates_periodicDates_calculationPeriodDatesAdjustments_bDC:
                            Dump_PeriodicDatesBDA();
                            break;
                        #endregion valuation dates periodicDates
                        #region default
                        default:
                            isSetting = false;
                            break;
                            #endregion default
                    }

                    if (isSetting)
                        CcisBase.Finalize(cci.ClientId_WithoutPrefix, processQueue);
                }
            }
        }

        #endregion Dump_ToDocument
        #region ProcessInitialize
        public void ProcessInitialize(CustomCaptureInfo pCci)
        {
            if (this.IsCciOfContainer(pCci.ClientId_WithoutPrefix))
            {
                string clientId_Key = CciContainerKey(pCci.ClientId_WithoutPrefix);
                //		
                CciEnum key = CciEnum.unknown;
                if (System.Enum.IsDefined(typeof(CciEnum), clientId_Key))
                    key = (CciEnum)System.Enum.Parse(typeof(CciEnum), clientId_Key);
                //
                switch (key)
                {
                    #region commission
                    case CciEnum.determinationMethod:
                        if (ActualPriceType == ActualPriceTypeEnum.priceInterim)
                        {
                            if (StrFunc.IsEmpty(pCci.NewValue) && (false == Cci(CciEnum.determinationMethod).IsMandatory))
                                Clear();
                            if (false == Cci(CciEnum.determinationMethod).IsMandatory)
                                SetEnabled(pCci.IsFilledValue);
                        }
                        break;
                    case CciEnum.commission_commissionDenomination:
                        if (_returnLegValuationPrice.CommissionSpecified)
                        {
                            //Commission
                            if (CcisBase.Contains(CciClientId(CciEnum.netPrice_amount)))
                            {
                                if (this.IsCommissionFixedAmount)
                                    Cci(CciEnum.commission_commissionAmount).Regex = EFSRegex.TypeRegex.RegexAmountExtend;
                                else if (this.IsCommissionPercentage)
                                    Cci(CciEnum.commission_commissionAmount).Regex = EFSRegex.TypeRegex.RegexFixedRatePercent;
                                else
                                    Cci(CciEnum.commission_commissionAmount).Regex = EFSRegex.TypeRegex.RegexDecimalExtend;
                            }
                            // Currency
                            if (this.IsCommissionFixedAmount)
                                CcisBase.SetNewValue(CciClientId(CciEnum.commission_currency), CcisBase[_cciTrade.CciClientIdMainCurrency].NewValue);
                            else
                                CcisBase.SetNewValue(CciClientId(CciEnum.commission_currency), string.Empty);
                        }
                        else
                        {
                            CcisBase.SetNewValue(CciClientId(CciEnum.commission_commissionAmount), string.Empty);
                            CcisBase.SetNewValue(CciClientId(CciEnum.commission_currency), string.Empty);
                        }
                        break;
                    case CciEnum.commission_commissionAmount:
                        if (_returnLegValuationPrice.CommissionSpecified)
                            Ccis.ProcessInitialize_AroundAmount(CciClientId(CciEnum.commission_commissionAmount),
                                _returnLegValuationPrice.Commission.Amount, _returnLegValuationPrice.Commission.Currency, false);

                        break;
                    case CciEnum.commission_currency:
                        if (_returnLegValuationPrice.CommissionSpecified)
                            Ccis.ProcessInitialize_AroundAmount(CciClientId(CciEnum.commission_commissionAmount),
                                _returnLegValuationPrice.Commission.Amount, _returnLegValuationPrice.Commission.Currency, false);
                        break;
                    #endregion commission
                    #region netPrice
                    case CciEnum.netPrice_priceExpression:
                        if (_returnLegValuationPrice.NetPriceSpecified)
                        {
                            if (CcisBase.Contains(CciClientId(CciEnum.netPrice_amount)))
                            {
                                if (this.IsPriceExpressionAbsoluteTerms)
                                    Cci(CciEnum.netPrice_amount).Regex = EFSRegex.TypeRegex.RegexAmountExtend;
                                else
                                    Cci(CciEnum.netPrice_amount).Regex = EFSRegex.TypeRegex.RegexFixedRatePercent;
                            }
                            // Currency
                            if (this.IsPriceExpressionAbsoluteTerms)
                            {
                                if (_cciReturnLeg.ReturnLeg.Notional.NotionalAmountSpecified)
                                {
                                    CcisBase.SetNewValue(CciClientId(CciEnum.netPrice_amount), _cciReturnLeg.ReturnLeg.Notional.NotionalAmount.Amount.Value);
                                    CcisBase.SetNewValue(CciClientId(CciEnum.netPrice_currency), _cciReturnLeg.ReturnLeg.Notional.NotionalAmount.Currency);
                                }
                            }
                            else
                                CcisBase.SetNewValue(CciClientId(CciEnum.netPrice_currency), string.Empty);
                        }
                        else
                        {
                            CcisBase.SetNewValue(CciClientId(CciEnum.netPrice_amount), string.Empty);
                            CcisBase.SetNewValue(CciClientId(CciEnum.netPrice_currency), string.Empty);
                        }
                        break;
                    case CciEnum.netPrice_amount:
                    case CciEnum.netPrice_currency:
                        if ((ActualPriceType == ActualPriceTypeEnum.initialPrice) &&  this.IsPriceExpressionAbsoluteTerms)
                            SetNotionalAmount();
                        break;
                    #endregion netPrice
                    #region default
                    default:

                        break;
                    #endregion default
                }
            }
        }
        #endregion ProcessInitialize
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
        #region IsClientId_PayerOrReceiver
        public bool IsClientId_PayerOrReceiver(CustomCaptureInfo pCci)
        {
            // Il n'y a pas de Payer/receiver ds EquityValuation
            return false;
        }
        #endregion IsClientId_PayerOrReceiver
        #region CleanUp
        public void CleanUp()
        {
            _cciReturnLeg.CleanUp();
            //
            _returnLegValuationPrice.CommissionSpecified = Cci(CciEnum.commission_commissionDenomination).IsFilledValue;
            _returnLegValuationPrice.NetPriceSpecified = Cci(CciEnum.netPrice_priceExpression).IsFilledValue;
            _returnLegValuationPrice.AmountRelativeToSpecified = Cci(CciEnum.amountRelativeTo).IsFilledValue;
        }
        #endregion CleanUp
        #region SetDisplay
        public void SetDisplay(CustomCaptureInfo pCci)
        {
            if (IsCci(CciEnum.valuationDate_adjustableDate_dateAdjustments_bDC, pCci))
                if (_returnLegValuationPrice.ValuationRulesSpecified && 
                    _returnLegValuationPrice.ValuationRules.ValuationDateSpecified &&
                    _returnLegValuationPrice.ValuationRules.ValuationDate.AdjustableDateSpecified)
                    Ccis.SetDisplayBusinessDayAdjustments(pCci, _returnLegValuationPrice.ValuationRules.ValuationDate.AdjustableDate.DateAdjustments);

            if (IsCci(CciEnum.valuationDates_periodicDates_calculationPeriodDatesAdjustments_bDC, pCci))
                if (_returnLegValuationPrice.ValuationRulesSpecified &&
                    _returnLegValuationPrice.ValuationRules.ValuationDatesSpecified &&
                    _returnLegValuationPrice.ValuationRules.ValuationDates.PeriodicDatesSpecified)
                    Ccis.SetDisplayBusinessDayAdjustments(pCci, _returnLegValuationPrice.ValuationRules.ValuationDates.PeriodicDates.CalculationPeriodDatesAdjustments);
        }
        #endregion

        
        /// <summary>
        /// 
        /// </summary>
        /// FI 20141104 [20466] Modify
        // EG 20150306 [POC-BERKELEY] : Add notionalBaseAmount valuation (CFD Forex)
        public void SetNotionalAmount()
        {
            int contractMultiplier = 1;
            decimal qty = CcisBase.GetDecimalNewValue(_cciReturnLeg.CciClientId(CciReturnLeg.CciEnumUnderlyer.underlyer_openUnits));

            IUnderlyingAsset underlyingAsset = _cciReturnLeg.UnderlyingAsset;
            
            // FxRate
            if (Cst.UnderlyingAsset.FxRateAsset == underlyingAsset.UnderlyerAssetCategory)
            {
                CustomCaptureInfo cci = _cciReturnLeg.Cci(CciReturnLeg.CciEnumUnderlyer.underlyer_underlyingAsset);
                if ((null != cci) && (null != cci.Sql_Table))
                {
                    string idC = string.Empty;
                    SQL_AssetFxRate sql_AssetFxRate = cci.Sql_Table as SQL_AssetFxRate;
                    contractMultiplier = sql_AssetFxRate.ContractMultiplier;
                    switch (sql_AssetFxRate.QCP_QuoteBasisEnum)
                    {
                        case QuoteBasisEnum.Currency1PerCurrency2:
                            idC = sql_AssetFxRate.QCP_Cur2;
                            break;
                        case QuoteBasisEnum.Currency2PerCurrency1:
                            idC = sql_AssetFxRate.QCP_Cur1;
                            break;
                    }
                    Pair<Nullable<decimal>, string> notionalBaseAmount =
                        Tools.ConvertToQuotedCurrency(CSTools.SetCacheOn(CcisBase.CS), null, new Pair<Nullable<decimal>, string>(qty * contractMultiplier, idC));

                    CcisBase.SetNewValue(_cciReturnLeg.CciClientId(CciReturnLeg.CciEnumUnderlyer.underlyer_notionalBase_amount),
                        StrFunc.FmtDecimalToInvariantCulture(notionalBaseAmount.First.Value));
                    CcisBase.SetNewValue(_cciReturnLeg.CciClientId(CciReturnLeg.CciEnumUnderlyer.underlyer_notionalBase_currency),
                        notionalBaseAmount.Second);
                }
            }
            string currency = Cci(CciEnum.netPrice_currency).NewValue;
            if (StrFunc.IsEmpty(currency))
                currency = _cciReturnLeg.Cci(CciReturnLeg.CciEnum.notional_notionalAmount_currency).NewValue;
            decimal price = Ccis.GetDecimalNewValue(CciClientId(CciEnum.netPrice_amount));

            Pair<Nullable<decimal>, string> notionalAmount =
                Tools.ConvertToQuotedCurrency(CSTools.SetCacheOn(CcisBase.CS), null, new Pair<Nullable<decimal>, string>(qty * contractMultiplier * price, currency));

            Ccis.SetNewValue(_cciReturnLeg.CciClientId(CciReturnLeg.CciEnum.notional_notionalAmount_amount), 
                StrFunc.FmtDecimalToInvariantCulture(notionalAmount.First.Value));

            Ccis.SetNewValue(_cciReturnLeg.CciClientId(CciReturnLeg.CciEnum.notional_notionalAmount_currency), notionalAmount.Second);


        }

        #region RemoveLastItemInArray
        public void RemoveLastItemInArray(string pPrefix)
        {
            _cciReturnLeg.RemoveLastItemInArray(pPrefix);
        }
        #endregion RemoveLastItemInArray
        #region RefreshCciEnabled
        public void RefreshCciEnabled()
        {
            _cciReturnLeg.RefreshCciEnabled();
        }
        #endregion
        #region Initialize_Document
        public void Initialize_Document()
        {
        }
        #endregion Initialize_Document
        #endregion Membres de IContainerCciFactory

        

        #region public Function
        public void Clear()
        {
            CciTools.SetCciContainer(this, "CciEnum", "NewValue", string.Empty);
        }

        public void SetEnabled(Boolean pIsEnabled)
        {
            CciTools.SetCciContainer(this, "CciEnum", "IsEnabled", pIsEnabled);
            Cci(CciEnum.determinationMethod).IsEnabled = true;
        }

        /// EG 20140426 TypeReferentialInfo.Asset remplace TypeReferentialInfo.Equity
        // EG 20150306 [POC-BERKELEY] : Add BC of vBCU and QCU (CFD Forex)
        public void Dump_PeriodicDatesBDA()
        {
            //string clientIdBdc = CciClientId(CciEnum.valuationDates_periodicDates_calculationPeriodDatesAdjustments_bDC);
            string clientIdAsset = _cciReturnLeg.CciClientId(CciReturnLeg.CciEnumUnderlyer.underlyer_underlyingAsset);    // A reprendre si basket (non gere pur l'instant)
            CciBC cciBC = new CciBC(_cciTrade);

            // EG 20150304 BCs = ceux de chaque devise sur CFD (Forex), celui du marché sur les autres
            IUnderlyingAsset underlyingAsset = _cciReturnLeg.UnderlyingAsset;
            if (null != underlyingAsset)
            {
                if (_cciReturnLeg.UnderlyingAsset.UnderlyerAssetCategory == Cst.UnderlyingAsset.FxRateAsset)
                {
                    string clientIdBaseCurrency = _cciReturnLeg.CciClientId(CciReturnLeg.CciEnumUnderlyer.underlyer_notionalBase_currency);
                    cciBC.Add(clientIdBaseCurrency, CciBC.TypeReferentialInfo.Currency);
                    string clientIdQuotedCurrency = _cciReturnLeg.CciClientId(CciReturnLeg.CciEnumUnderlyer.underlyer_currency);
                    cciBC.Add(clientIdQuotedCurrency, CciBC.TypeReferentialInfo.Currency);
                }
                else
                {
                        cciBC.Add(clientIdAsset, CciBC.TypeReferentialInfo.Asset);
                }
            }

            if (_returnLegValuationPrice.ValuationRulesSpecified)
            {
                IBusinessDayAdjustments bDA = null;
                string clientIdBdc = string.Empty;
                if (_returnLegValuationPrice.ValuationRules.ValuationDateSpecified)
                {
                    if (_returnLegValuationPrice.ValuationRules.ValuationDate.AdjustableDateSpecified)
                    {
                        bDA = _returnLegValuationPrice.ValuationRules.ValuationDate.AdjustableDate.DateAdjustments;
                        clientIdBdc = CciClientId(CciEnum.valuationDate_adjustableDate_dateAdjustments_bDC);
                    }
                    else if (_returnLegValuationPrice.ValuationRules.ValuationDate.RelativeDateSequenceSpecified)
                    {
                    }
                }
                else if (_returnLegValuationPrice.ValuationRules.ValuationDatesSpecified)
                {
                    if (_returnLegValuationPrice.ValuationRules.ValuationDates.AdjustableDatesSpecified)
                    {
                        bDA = _returnLegValuationPrice.ValuationRules.ValuationDates.AdjustableDates.DateAdjustments;
                    }
                    else if (_returnLegValuationPrice.ValuationRules.ValuationDates.RelativeDateSequenceSpecified)
                    {
                    }
                    else if (_returnLegValuationPrice.ValuationRules.ValuationDates.PeriodicDatesSpecified)
                    {
                        bDA = _returnLegValuationPrice.ValuationRules.ValuationDates.PeriodicDates.CalculationPeriodDatesAdjustments;
                        clientIdBdc = CciClientId(CciEnum.valuationDates_periodicDates_calculationPeriodDatesAdjustments_bDC);
                    }
                }
                if ((null != bDA) && StrFunc.IsFilled(clientIdBdc))
                    Ccis.DumpBDC_ToDocument(bDA, clientIdBdc, TradeCustomCaptureInfos.CCst.PERIOD_BUSINESS_CENTERS_REFERENCE, cciBC);
            }
            //IBusinessDayAdjustments bDA = _returnLegValuationPrice.valuationRules.valuationDates.periodicDates.calculationPeriodDatesAdjustments;
            //ccis.DumpBDC_ToDocument(bDA, clientIdBdc, TradeCustomCaptureInfos.CCst.PERIOD_BUSINESS_CENTERS_REFERENCE, cciBC);
        }
        #endregion

    }
}
