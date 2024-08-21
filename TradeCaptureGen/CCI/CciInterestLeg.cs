#region Using Directives
using EFS.ACommon;
using EFS.Common;
using EFS.Common.Web;
using EFS.GUI.CCI;
using EFS.GUI.Interface;
using FpML.Enum;
using FpML.Interface;
using System;
#endregion Using Directives

namespace EFS.TradeInformation
{
    /// <summary>
    /// Description résumée de CciInterestLeg.
    /// </summary>
    public class CciInterestLeg : IContainerCci, IContainerCciPayerReceiver, IContainerCciFactory, IContainerCciSpecified
    {
        #region Members
        private IInterestLeg _interestLeg;
        private readonly string _prefix;
        private string _prefixId;
        private readonly TradeCustomCaptureInfos _ccis;
        private readonly CciTradeBase _cciTrade;
        private readonly int _legNumber;
        #endregion Members

        #region region Enum
        public enum CciEnum
        {
            [System.Xml.Serialization.XmlEnumAttribute("payerPartyReference")]
            payer,
            [System.Xml.Serialization.XmlEnumAttribute("receiverPartyReference")]
            receiver,

            //Adjustable 
            [System.Xml.Serialization.XmlEnumAttribute("additionalPaymentDate.adjustableOrRelativeDateAdjustableDate.unadjustedDate")]
            additionalPaymentDate_adjustableDate_unadjustedDate,
            [System.Xml.Serialization.XmlEnumAttribute("additionalPaymentDate.adjustableOrRelativeDateAdjustableDate.dateAdjustments.businessDayConvention")]
            additionalPaymentDate_adjustableDate_dateAdjustments_bDC,
            //RelativeDate
            [System.Xml.Serialization.XmlEnumAttribute("additionalPaymentDate.adjustableOrRelativeDateRelativeDate.periodMultiplier")]
            additionalPaymentDate_relativeDate_periodMultiplier,
            [System.Xml.Serialization.XmlEnumAttribute("additionalPaymentDate.adjustableOrRelativeDateRelativeDate.period")]
            additionalPaymentDate_relativeDate_period,
            [System.Xml.Serialization.XmlEnumAttribute("additionalPaymentDate.adjustableOrRelativeDateRelativeDate.dateRelativeTo")]
            additionalPaymentDate_relativeDate_dateRelativeTo,
            [System.Xml.Serialization.XmlEnumAttribute("additionalPaymentDate.adjustableOrRelativeDateRelativeDate.businessDayConvention")]
            additionalPaymentDate_relativeDate_bDC,



            // Bouton interestLegCalculationPeriodDates.interestLegResetDates
            [System.Xml.Serialization.XmlEnumAttribute("interestLegCalculationPeriodDates.interestLegResetDates")]
            resetDates,

            // Bouton interestLegCalculationPeriodDates.interestLegPaymentDates
            [System.Xml.Serialization.XmlEnumAttribute("interestLegCalculationPeriodDates.interestLegPaymentDates")]
            paymentDates,
            unknown

        }
        #region CciEnumCalculation
        public enum CciEnumCalculation
        {
            [System.Xml.Serialization.XmlEnumAttribute("interestCalculation.interestAccrualsMethodFloatingRate.fixedRate")]
            calculation_rate,
            [System.Xml.Serialization.XmlEnumAttribute("interestCalculation.interestAccrualsMethodFloatingRate.spreadSchedule.initialValue")]
            calculation_floatingRate_spreadSchedule_initialValue,
            [System.Xml.Serialization.XmlEnumAttribute("interestCalculation.interestAccrualsMethodFloatingRate.floatingRateMultiplierSchedule.initialValue")]
            calculation_floatingRate_multiplierSchedule_initialValue,
            [System.Xml.Serialization.XmlEnumAttribute("interestCalculation.interestAccrualsMethodFloatingRate.")]
            calculation_floatingRate_capRateSchedule_initialValue,
            [System.Xml.Serialization.XmlEnumAttribute("interestCalculation.interestAccrualsMethodFloatingRate.")]
            calculation_floatingRate_capRateSchedule2_initialValue,
            [System.Xml.Serialization.XmlEnumAttribute("interestCalculation.interestAccrualsMethodFloatingRate.")]
            calculation_floatingRate_floorRateSchedule_initialValue,

            [System.Xml.Serialization.XmlEnumAttribute("interestCalculation.dayCountFraction")]
            calculation_dayCountFraction,
            unknown
        }
        #endregion CciEnumCalculation
        #region CciEnumPaymentDates
        public enum CciEnumPaymentDates
        {
            // PaymentDates.AdjustableDates
            [System.Xml.Serialization.XmlEnumAttribute("interestLegCalculationPeriodDates.interestLegPaymentDates.adjustableDates.unadjustedDate")]
            paymentDates_adjustableDates_unadjustedDate,
            [System.Xml.Serialization.XmlEnumAttribute("interestLegCalculationPeriodDates.interestLegPaymentDates.adjustableDates.dateAdjustments.businessDayConvention")]
            paymentDates_adjustableDates_dateAdjustments_bDC,
            [System.Xml.Serialization.XmlEnumAttribute("interestLegCalculationPeriodDates.interestLegPaymentDates.adjustableDates.dateAdjustments.businessCentersReference")]
            paymentDates_adjustableDates_dateAdjustments_bCR,
            [System.Xml.Serialization.XmlEnumAttribute("interestLegCalculationPeriodDates.interestLegPaymentDates.adjustableDates.dateAdjustments.businessCenters")]
            paymentDates_adjustableDates_dateAdjustments_bCS,

            // PaymentDates.RelativeDates
            [System.Xml.Serialization.XmlEnumAttribute("interestLegCalculationPeriodDates.interestLegPaymentDates.relativeDates")]
            paymentDates_relativeDates,

            // PaymentDates.PeriodicDates
            [System.Xml.Serialization.XmlEnumAttribute("interestLegCalculationPeriodDates.interestLegPaymentDates.periodicDates.calculationStartDate")]
            paymentDates_periodicDates,

            unknown
        }
        #endregion CciEnumPaymentDates
        #region CciEnumResetDates
        public enum CciEnumResetDates
        {
            [System.Xml.Serialization.XmlEnumAttribute("interestLegCalculationPeriodDates.interestLegResetDates.calculationPeriodDatesReference")]
            resetDates_calculationPeriodDatesReference,
            [System.Xml.Serialization.XmlEnumAttribute("interestLegCalculationPeriodDates.interestLegResetDates.resetRelativeTo")]
            resetDates_resetRelativeTo,
            [System.Xml.Serialization.XmlEnumAttribute("interestLegCalculationPeriodDates.interestLegResetDates.resetFrequency")]
            resetDates_resetFrequency,
            [System.Xml.Serialization.XmlEnumAttribute("interestLegCalculationPeriodDates.interestLegResetDates.weeklyRollConvention")]
            resetDates_weeklyRollConvention,
            unknown
        }
        #endregion CciEnumResetDates
        #endregion region

        #region constructor
        public CciInterestLeg(CciTradeBase pCciTrade, int pLegNumber, IInterestLeg pInterestLeg, string pPrefix)
        {
            _cciTrade = pCciTrade;
            _ccis = pCciTrade.Ccis;
            _interestLeg = pInterestLeg;
            _legNumber = pLegNumber;
            _prefix = pPrefix + pLegNumber.ToString() + CustomObject.KEY_SEPARATOR;
            _prefixId = string.Empty;
        }
        #endregion constructor

        #region public properties
        public IFloatingRate FloatingRate
        {
            get
            {
                IFloatingRate rate = null;
                if (IsFloatingRateSpecified)
                    rate = _interestLeg.InterestCalculation.FloatingRate;
                return rate;
            }
        }
        public bool IsFloatingRateSpecified
        {
            get {return _interestLeg.InterestCalculation.FloatingRateSpecified;}
        }
        public bool IsCapRateScheduleSpecified
        {
            get
            {
                IFloatingRate rate = FloatingRate;
                return (null != rate) && rate.CapRateScheduleSpecified && rate.CapRateSchedule[0].StepSpecified;
            }
        }
        public bool IsFloorRateScheduleSpecified
        {
            get
            {
                IFloatingRate rate = FloatingRate;
                return (null != rate) && rate.FloorRateScheduleSpecified && rate.FloorRateSchedule[0].StepSpecified;
            }
        }
        public bool IsFloatingRateMultiplierScheduleSpecified
        {
            get
            {
                IFloatingRate rate = FloatingRate;
                return (null != rate) && rate.FloatingRateMultiplierScheduleSpecified && rate.FloatingRateMultiplierSchedule.StepSpecified;
            }
        }
        public bool IsSpreadScheduleSpecified
        {
            get
            {
                IFloatingRate rate = FloatingRate;
                return (null != rate) && rate.SpreadScheduleSpecified && rate.SpreadSchedule.StepSpecified;
            }
        }
        public TradeCustomCaptureInfos Ccis
        {
            get { return _ccis; }
        }
        public IInterestLeg InterestLeg
        {
            set { _interestLeg = value; }
        }
        public string PrefixId
        {
            get
            {
                return _prefixId;
            }
            set
            {
                _prefixId = value;
            }
        }
        #endregion public properties

        #region Membres de IContainerCciSpecified
        public bool IsSpecified { get { return Cci(CciEnum.payer).IsFilled; } }
        #endregion Membres de IContainerCciSpecified

        #region Membres de IContainerCci
        /*
        public string CciClientId(CciEnum pEnumValue)
        {
            return CciClientId(pEnumValue.ToString());
        }
        */
        public string CciClientId(Enum pEnumValue)
        {
            return CciClientId(pEnumValue.ToString());
        }
        public string CciClientId(string pClientId_Key)
        {
            return _prefix + pClientId_Key;
        }
        public CustomCaptureInfo Cci(Enum pEnumValue)
        {
            return Ccis[CciClientId(pEnumValue)];
        }
        /*
        public CustomCaptureInfo Cci(CciEnum pEnumValue)
        {
            return ccis[CciClientId(pEnumValue)];
        }
        */
        public CustomCaptureInfo Cci(string pClientId_Key)
        {
            return Ccis[CciClientId(pClientId_Key)];
        }
        #region IsCci
        public bool IsCci(Enum pEnumValue, CustomCaptureInfo pCci)
        {
            return (CciClientId(pEnumValue) == pCci.ClientId_WithoutPrefix);
        }
        /*
        public bool IsCci(CciEnum pEnumValue, CustomCaptureInfo pCci)
        {
            return (CciClientId(pEnumValue) == pCci.ClientId_WithoutPrefix);
        }
        */
        public bool IsCci(string pEnumValue, CustomCaptureInfo pCci)
        {
            return (CciClientId(pEnumValue) == pCci.ClientId_WithoutPrefix);
        }
        #endregion IsCci
        public bool IsCciOfContainer(string pClientId_WithoutPrefix)
        {
            return (pClientId_WithoutPrefix.StartsWith(_prefix));
        }
        public string CciContainerKey(string pClientId_WithoutPrefix)
        {
            return pClientId_WithoutPrefix.Substring(_prefix.Length);
        }
        #endregion

        #region Membres de IContainerCciPayerReceiver
        #region  CciClientIdPayer/receiver
        public string CciClientIdPayer
        {
            get { return CciClientId(CciEnum.payer); }
        }
        public string CciClientIdReceiver
        {
            get { return CciClientId(CciEnum.receiver); }
        }
        #endregion
        #region SynchronizePayerReceiver
        public void SynchronizePayerReceiver(string pLastValue, string pNewValue)
        {
            Ccis.Synchronize(CciClientIdPayer, pLastValue, pNewValue);
            Ccis.Synchronize(CciClientIdReceiver, pLastValue, pNewValue);
        }
        #endregion
        #endregion

        #region Membres de IContainerCciFactory
        #region Initialize_FromCci
        // EG 20231024 [XXXXX] RTS / Corrections diverses : Mise en commentaire de FloatingRateSpecified
        public void Initialize_FromCci()
        {
            CciTools.CreateInstance(this, _interestLeg);
            //
            if (null == _interestLeg.InterestCalculation)
                _interestLeg.InterestCalculation = _interestLeg.CreateInterestCalculation;

            if (null == _interestLeg.CalculationPeriodDates)
                _interestLeg.CalculationPeriodDates = _interestLeg.CreateCalculationPeriodDates;

             
            if (Ccis.Contains(CciClientId(CciEnumCalculation.calculation_rate)))
            {
                //_interestLeg.interestCalculation.floatingRateSpecified = true;
                IFloatingRate floatingRate = _interestLeg.InterestCalculation.FloatingRate;
                if (null == floatingRate.SpreadSchedule)
                    floatingRate.SpreadSchedule = floatingRate.CreateSpreadSchedule();
                
            }

            // Initialisation par défaut, pour que lorsque que l'on saisie le taux que toutes les pré-propositions soient effectuées
            if (Ccis.Contains(CciClientId(CciEnumCalculation.calculation_rate)) ||
                Ccis.Contains(CciClientId(CciEnumCalculation.calculation_floatingRate_capRateSchedule_initialValue)) ||
                Ccis.Contains(CciClientId(CciEnumCalculation.calculation_floatingRate_floorRateSchedule_initialValue))
                )
            {
                //_interestLeg.interestCalculation.floatingRateSpecified = true;
                #region FloatingRate
                IFloatingRate floatingRate = _interestLeg.InterestCalculation.FloatingRate;
                if (null == floatingRate.SpreadSchedule)
                    floatingRate.SpreadSchedule = floatingRate.CreateSpreadSchedule();
                if (null == floatingRate.CapRateSchedule)
                {
                    if (Ccis.Contains(CciClientId(CciEnumCalculation.calculation_floatingRate_capRateSchedule2_initialValue)))
                        floatingRate.CapRateSchedule = floatingRate.CreateStrikeSchedule(2);
                    else
                        floatingRate.CapRateSchedule = floatingRate.CreateStrikeSchedule(1);
                }
                else if (ArrFunc.Count(floatingRate.CapRateSchedule) == 1 && (Ccis.Contains(CciClientId(CciEnumCalculation.calculation_floatingRate_capRateSchedule2_initialValue))))
                {
                    IStrikeSchedule[] capRateSav = (IStrikeSchedule[])floatingRate.CapRateSchedule.Clone();
                    floatingRate.CapRateSchedule = floatingRate.CreateStrikeSchedule(2);
                    floatingRate.CapRateSchedule[0] = capRateSav[0];
                }
                if (null == floatingRate.FloorRateSchedule)
                    floatingRate.FloorRateSchedule = floatingRate.CreateStrikeSchedule(1);
                #endregion FloatingRate
            }

            #region initial Stub
            #endregion initial Stub
            #region finalStub
            #endregion finalStub

            if (null == _interestLeg.InterestAmount)
                _interestLeg.InterestAmount = _interestLeg.CreateLegAmount;

            if (null == ((IReturnSwapLeg)_interestLeg).ReceiverPartyReference)
                ((IReturnSwapLeg)_interestLeg).ReceiverPartyReference = ((IReturnSwapLeg)_interestLeg).CreateReference;

            if (null == ((IReturnSwapLeg)_interestLeg).PayerPartyReference)
                ((IReturnSwapLeg)_interestLeg).PayerPartyReference = ((IReturnSwapLeg)_interestLeg).CreateReference;

            // ResetDates
            if (null == _interestLeg.CalculationPeriodDates.ResetDates)
                _interestLeg.CalculationPeriodDates.ResetDates = _interestLeg.CreateResetDates;
            IInterestLegResetDates _resetDates = _interestLeg.CalculationPeriodDates.ResetDates;

            if (null == _resetDates.ResetFrequency)
                _resetDates.ResetFrequency = _interestLeg.CreateResetFrequency;
            if (null == _resetDates.ResetFrequency.Interval.PeriodMultiplier)
                _resetDates.ResetFrequency.Interval.PeriodMultiplier = new EFS_Integer();
            if (null == _resetDates.InitialFixingDate)
                _resetDates.InitialFixingDate = _interestLeg.CreateRelativeDateOffset;

            // PaymentDates
            if (null == _interestLeg.CalculationPeriodDates.PaymentDates)
                _interestLeg.CalculationPeriodDates.PaymentDates = _interestLeg.CreatePaymentDates;
        }
        #endregion Initialize_FromCci
        /// <summary>
        /// 
        /// </summary>
        /// FI 20170116 [21916] Modify (use AddCciSystem Method)
        public void AddCciSystem()
        {
            CciTools.AddCciSystem(Ccis, Cst.DDL + CciClientId(CciEnum.payer), true, TypeData.TypeDataEnum.@string);
            CciTools.AddCciSystem(Ccis, Cst.DDL + CciClientId(CciEnum.receiver), true, TypeData.TypeDataEnum.@string);
            CciTools.AddCciSystem(Ccis, Cst.BUT + CciClientId(CciEnum.resetDates), false, TypeData.TypeDataEnum.@string);
            CciTools.AddCciSystem(Ccis, Cst.BUT + CciClientId(CciEnum.paymentDates), false, TypeData.TypeDataEnum.@string);
        }
        
        #region Initialize_FromDocument
        public void Initialize_FromDocument()
        {
            string cliendId_Key;
            foreach (CustomCaptureInfo cci in Ccis)
            {
                if (this.IsCciOfContainer(cci.ClientId_WithoutPrefix))
                {
                    cliendId_Key = this.CciContainerKey(cci.ClientId_WithoutPrefix);
                    if (System.Enum.IsDefined(typeof(CciEnum), cliendId_Key))
                    {
                        CciEnum _currEnum = (CciEnum)Enum.Parse(typeof(CciEnum), cliendId_Key);
                        Initialize_FromDocument(cci, _currEnum);
                    }
                    else if (System.Enum.IsDefined(typeof(CciEnumCalculation), cliendId_Key))
                    {
                        CciEnumCalculation _currEnum = (CciEnumCalculation)Enum.Parse(typeof(CciEnumCalculation), cliendId_Key);
                        Initialize_FromDocument(cci, _currEnum);
                    }
                    else if (System.Enum.IsDefined(typeof(CciEnumResetDates), cliendId_Key))
                    {
                        CciEnumResetDates _currEnum = (CciEnumResetDates)Enum.Parse(typeof(CciEnumResetDates), cliendId_Key);
                        Initialize_FromDocument(cci, _currEnum);
                    }
                    else if (System.Enum.IsDefined(typeof(CciEnumPaymentDates), cliendId_Key))
                    {
                        CciEnumPaymentDates _currEnum = (CciEnumPaymentDates)Enum.Parse(typeof(CciEnumPaymentDates), cliendId_Key);
                        Initialize_FromDocument(cci, _currEnum);
                    }
                }
            }
        }
        private void Initialize_FromDocument(CustomCaptureInfo pCci, CciEnum pEnum)
        {
            string data = string.Empty;
            bool isToValidate = false;
            bool isSetting = true;
            SQL_Table sql_Table = null;

            switch (pEnum)
            {
                #region Payer
                case CciEnum.payer:
                    data = ((IReturnSwapLeg)_interestLeg).PayerPartyReference.HRef;
                    break;
                #endregion Payer
                #region Receiver
                case CciEnum.receiver:
                    data = ((IReturnSwapLeg)_interestLeg).ReceiverPartyReference.HRef;
                    break;
                #endregion Receiver

                #region default
                default:
                    isSetting = false;
                    break;
                #endregion
            }
            if (isSetting)
            {
                Ccis.InitializeCci(pCci, sql_Table, data);
                if (isToValidate)
                    pCci.LastValue = ".";
            }
        }
        private void Initialize_FromDocument(CustomCaptureInfo pCci, CciEnumCalculation pEnum)
        {
            string data = string.Empty;
            bool isToValidate = false;
            bool isSetting = true;
            SQL_Table sql_Table = null;

            switch (pEnum)
            {
                #region FixedRate / FloatingRate
                case CciEnumCalculation.calculation_rate:
                    if (_interestLeg.InterestCalculation.FixedRateSpecified)
                    {
                        #region RateFixedRate
                        data = _interestLeg.InterestCalculation.FixedRate.Value;
                        #endregion RateFixedRate
                    }
                    else if (_interestLeg.InterestCalculation.FloatingRateSpecified)
                    {
                        #region FloatingRate
                        try
                        {
                            SQL_AssetRateIndex sql_RateIndex = null;
                            int idAsset = _interestLeg.InterestCalculation.FloatingRate.FloatingRateIndex.OTCmlId;
                            if (idAsset > 0)
                            {
                                sql_RateIndex = new SQL_AssetRateIndex(_cciTrade.CSCacheOn, SQL_AssetRateIndex.IDType.IDASSET, idAsset);
                            }
                            else
                            {
                                if (StrFunc.IsFilled(_interestLeg.InterestCalculation.FloatingRate.FloatingRateIndex.Value))
                                {
                                    sql_RateIndex = new SQL_AssetRateIndex(_cciTrade.CSCacheOn, SQL_AssetRateIndex.IDType.Asset_Identifier,
                                        _interestLeg.InterestCalculation.FloatingRate.FloatingRateIndex.Value.Replace(" ", "%") + "%");
                                    //
                                    if (_interestLeg.InterestCalculation.FloatingRate.IndexTenorSpecified)
                                    {
                                        sql_RateIndex.Asset_PeriodMltp_In = _interestLeg.InterestCalculation.FloatingRate.IndexTenor.PeriodMultiplier.IntValue;
                                        sql_RateIndex.Asset_Period_In = _interestLeg.InterestCalculation.FloatingRate.IndexTenor.Period.ToString();
                                    }
                                }
                            }
                            if ((null != sql_RateIndex) && sql_RateIndex.IsLoaded)
                            {
                                sql_Table = sql_RateIndex;
                                data = sql_RateIndex.Identifier;
                                isToValidate = (idAsset == 0);
                            }
                        }
                        catch
                        {
                            pCci.Sql_Table = null;
                            data = string.Empty;
                        }
                        #endregion FloatingRate
                    }
                    break;
                case CciEnumCalculation.calculation_floatingRate_spreadSchedule_initialValue:
                case CciEnumCalculation.calculation_floatingRate_multiplierSchedule_initialValue:
                case CciEnumCalculation.calculation_floatingRate_capRateSchedule_initialValue:
                case CciEnumCalculation.calculation_floatingRate_floorRateSchedule_initialValue:
                    if (_interestLeg.InterestCalculation.FloatingRateSpecified)
                    {
                        IFloatingRate floatingRate = _interestLeg.InterestCalculation.FloatingRate;
                        if (null != floatingRate)
                        {
                            switch (pEnum)
                            {
                                case CciEnumCalculation.calculation_floatingRate_multiplierSchedule_initialValue:
                                    #region MultiplierSchedule
                                    if ((null != floatingRate) && floatingRate.FloatingRateMultiplierScheduleSpecified)
                                        data = floatingRate.FloatingRateMultiplierSchedule.InitialValue.Value;
                                    #endregion MultiplierSchedule
                                    break;
                                case CciEnumCalculation.calculation_floatingRate_spreadSchedule_initialValue:
                                    #region SpreadSchedule
                                    if ((null != floatingRate) && floatingRate.SpreadScheduleSpecified)
                                        data = floatingRate.SpreadSchedule.InitialValue.Value;
                                    #endregion SpreadSchedule
                                    break;
                                case CciEnumCalculation.calculation_floatingRate_capRateSchedule_initialValue:
                                    #region CapRate
                                    if ((null != floatingRate) && floatingRate.CapRateScheduleSpecified)
                                        data = floatingRate.CapRateSchedule[0].InitialValue.Value;
                                    #endregion CapRate
                                    break;
                                case CciEnumCalculation.calculation_floatingRate_capRateSchedule2_initialValue:
                                    #region CapRate
                                    if ((null != floatingRate) && floatingRate.CapRateScheduleSpecified)
                                        data = floatingRate.CapRateSchedule[1].InitialValue.Value;
                                    #endregion CapRate
                                    break;

                                case CciEnumCalculation.calculation_floatingRate_floorRateSchedule_initialValue:
                                    #region FloorRate
                                    if ((null != floatingRate) && floatingRate.FloorRateScheduleSpecified)
                                        data = floatingRate.FloorRateSchedule[0].InitialValue.Value;
                                    #endregion FloorRate
                                    break;
                                default:
                                    isSetting = false;
                                    break;
                            }
                        }
                    }
                    break;
                #endregion FixedRate / FloatingRate
                #region DayCountFraction
                case CciEnumCalculation.calculation_dayCountFraction:
                    data = _interestLeg.InterestCalculation.DayCountFraction.ToString();
                    break;
                #endregion

                #region default
                default:
                    isSetting = false;
                    break;
                #endregion
            }
            if (isSetting)
            {
                Ccis.InitializeCci(pCci, sql_Table, data);
                if (isToValidate)
                    pCci.LastValue = ".";
            }
        }
        private void Initialize_FromDocument(CustomCaptureInfo pCci, CciEnumResetDates pEnum)
        {
            string data = string.Empty;
            bool isToValidate = false;
            bool isSetting = true;
            SQL_Table sql_Table = null;
            
            switch (pEnum)
            {
                #region ResetDates
                case CciEnumResetDates.resetDates_calculationPeriodDatesReference:
                    #region CalculationPeriodDatesReference
                    data = _interestLeg.CalculationPeriodDates.ResetDates.CalculationPeriodDatesReference.HRef;
                    #endregion CalculationPeriodDatesReference
                    break;
                case CciEnumResetDates.resetDates_resetRelativeTo:
                    #region ResetRelativeTo
                    if (_interestLeg.CalculationPeriodDates.ResetDates.ResetRelativeToSpecified)
                        data = _interestLeg.CalculationPeriodDates.ResetDates.ResetRelativeTo.ToString();
                    #endregion ResetRelativeTo
                    break;
                #endregion ResetDates

                #region default
                default:
                    isSetting = false;
                    break;
                #endregion
            }
            if (isSetting)
            {
                Ccis.InitializeCci(pCci, sql_Table, data);
                if (isToValidate)
                    pCci.LastValue = ".";
            }
        }
        private void Initialize_FromDocument(CustomCaptureInfo pCci, CciEnumPaymentDates pEnum)
        {
            string data = string.Empty;
            bool isToValidate = false;
            SQL_Table sql_Table = null;

            bool isSetting;
            switch (pEnum)
            {
                #region default
                default:
                    isSetting = false;
                    break;
                    #endregion
            }
            if (isSetting)
            {
                Ccis.InitializeCci(pCci, sql_Table, data);
                if (isToValidate)
                    pCci.LastValue = ".";
            }
        }
        #endregion Initialize_FromDocument

        #region Dump_ToDocument
        public void Dump_ToDocument()
        {
            foreach (CustomCaptureInfo cci in Ccis)
            {
                if ((cci.HasChanged) && IsCciOfContainer(cci.ClientId_WithoutPrefix))
                {
                    string cliendId_Key = this.CciContainerKey(cci.ClientId_WithoutPrefix);
                    if (Enum.IsDefined(typeof(CciEnum), cliendId_Key))
                    {
                        CciEnum _currEnum = (CciEnum)Enum.Parse(typeof(CciEnum), cliendId_Key);
                        Dump_ToDocument(cci, _currEnum);
                    }
                    else if (Enum.IsDefined(typeof(CciEnumCalculation), cliendId_Key))
                    {
                        CciEnumCalculation _currEnum = (CciEnumCalculation)Enum.Parse(typeof(CciEnumCalculation), cliendId_Key);
                        Dump_ToDocument(cci, _currEnum);
                    }
                    else if (Enum.IsDefined(typeof(CciEnumResetDates), cliendId_Key))
                    {
                        CciEnumResetDates _currEnum = (CciEnumResetDates)Enum.Parse(typeof(CciEnumResetDates), cliendId_Key);
                        Dump_ToDocument(cci, _currEnum);
                    }
                    else if (Enum.IsDefined(typeof(CciEnumPaymentDates), cliendId_Key))
                    {
                        CciEnumPaymentDates _currEnum = (CciEnumPaymentDates)Enum.Parse(typeof(CciEnumPaymentDates), cliendId_Key);
                        Dump_ToDocument(cci, _currEnum);
                    }
                }
            }
        }
        private void Dump_ToDocument(CustomCaptureInfo pCci, CciEnum pEnum)
        {
            bool isSetting = true;
            string data = pCci.NewValue;
            CustomCaptureInfosBase.ProcessQueueEnum processQueue = CustomCaptureInfosBase.ProcessQueueEnum.None;
            switch (pEnum)
            {
                #region payer
                case CciEnum.payer:
                    ((IReturnSwapLeg)_interestLeg).PayerPartyReference.HRef = data;
                    processQueue = CustomCaptureInfosBase.ProcessQueueEnum.Low;
                    break;
                #endregion payer
                #region receiver
                case CciEnum.receiver:
                    ((IReturnSwapLeg)_interestLeg).ReceiverPartyReference.HRef = data;
                    processQueue = CustomCaptureInfosBase.ProcessQueueEnum.Low;
                    break;
                #endregion receiver

                #region default
                default:
                    isSetting = false;
                    break;
                #endregion default
            }
            if (isSetting)
                Ccis.Finalize(pCci.ClientId_WithoutPrefix, processQueue);
        }
        private void Dump_ToDocument(CustomCaptureInfo pCci, CciEnumCalculation pEnum)
        {
            bool isSetting = true;
            string data = pCci.NewValue;
            CustomCaptureInfosBase.ProcessQueueEnum processQueue = CustomCaptureInfosBase.ProcessQueueEnum.None;

            switch (pEnum)
            {
                #region FixedRate / FloatingRate
                case CciEnumCalculation.calculation_rate:
                    Cci(CciEnumCalculation.calculation_rate).Sql_Table = null;
                    Cci(CciEnumCalculation.calculation_rate).ErrorMsg = string.Empty;
                    _interestLeg.InterestCalculation.FixedRateSpecified = RateTools.IsFixedRate(data);
                    _interestLeg.InterestCalculation.FloatingRateSpecified = RateTools.IsFloatingRate(data);

                    #region FixedRate
                    if (_interestLeg.InterestCalculation.FixedRateSpecified)
                        _interestLeg.InterestCalculation.FixedRate.Value = data;
                    #endregion FixedRate
                    //
                    #region  FloatingRate
                    if (_interestLeg.InterestCalculation.FloatingRateSpecified)
                    {
                        Ccis.DumpFloatingRateIndex_ToDocument(CciClientId(CciEnumCalculation.calculation_rate), null,
                            _interestLeg.InterestCalculation.FloatingRate, _interestLeg.InterestCalculation.FloatingRate.FloatingRateIndex, 
                            _interestLeg.InterestCalculation.FloatingRate.IndexTenor);
                    }
                    #endregion FloatingRate
                    processQueue = CustomCaptureInfosBase.ProcessQueueEnum.Low;//Afin de recalculer DCF
                    break;
                case CciEnumCalculation.calculation_floatingRate_multiplierSchedule_initialValue:
                    _interestLeg.InterestCalculation.FloatingRateSpecified = pCci.IsFilledValue;
                    _interestLeg.InterestCalculation.FloatingRate.FloatingRateMultiplierScheduleSpecified = pCci.IsFilledValue;
                    if (pCci.IsFilledValue)
                        _interestLeg.InterestCalculation.FloatingRate.FloatingRateMultiplierSchedule.InitialValue.Value = data;
                    break;
                case CciEnumCalculation.calculation_floatingRate_spreadSchedule_initialValue:
                    _interestLeg.InterestCalculation.FloatingRateSpecified = pCci.IsFilledValue;
                    _interestLeg.InterestCalculation.FloatingRate.SpreadScheduleSpecified = pCci.IsFilledValue;
                    if (pCci.IsFilledValue)
                        _interestLeg.InterestCalculation.FloatingRate.SpreadSchedule.InitialValue.Value = data;
                    break;
                #endregion FixedRate / FloatingRate
                #region dayCountFraction
                case CciEnumCalculation.calculation_dayCountFraction:
                    DayCountFractionEnum dcfEnum = (DayCountFractionEnum)System.Enum.Parse(typeof(DayCountFractionEnum), data, true);
                    _interestLeg.InterestCalculation.DayCountFraction = dcfEnum;
                    break;
                #endregion dayCountFraction
                #region default
                default:
                    isSetting = false;
                    break;
                #endregion default
            }
            if (isSetting)
                Ccis.Finalize(pCci.ClientId_WithoutPrefix, processQueue);
        }
        private void Dump_ToDocument(CustomCaptureInfo pCci, CciEnumResetDates pEnum)
        {
            bool isSetting = true;
            string data = pCci.NewValue;
            CustomCaptureInfosBase.ProcessQueueEnum processQueue = CustomCaptureInfosBase.ProcessQueueEnum.None;

            switch (pEnum)
            {
                #region ResetDates
                case CciEnumResetDates.resetDates_calculationPeriodDatesReference:
                    #region CalculationPeriodDatesReference
                    _interestLeg.CalculationPeriodDates.ResetDates.CalculationPeriodDatesReference.HRef = data;
                    #endregion CalculationPeriodDatesReference
                    break;
                case CciEnumResetDates.resetDates_resetRelativeTo:
                    #region ResetRelativeto
                    _interestLeg.CalculationPeriodDates.ResetDates.ResetRelativeToSpecified = pCci.IsFilledValue;
                    if (pCci.IsFilledValue)
                        _interestLeg.CalculationPeriodDates.ResetDates.ResetRelativeTo = (ResetRelativeToEnum)System.Enum.Parse(typeof(ResetRelativeToEnum), data, true);
                    #endregion ResetRelativeto
                    break;
                #endregion ResetDates
                #region default
                default:
                    isSetting = false;
                    break;
                #endregion default
            }
            if (isSetting)
                Ccis.Finalize(pCci.ClientId_WithoutPrefix, processQueue);
        }
        private void Dump_ToDocument(CustomCaptureInfo pCci, CciEnumPaymentDates pEnum)
        {
            CustomCaptureInfosBase.ProcessQueueEnum processQueue = CustomCaptureInfosBase.ProcessQueueEnum.None;

            bool isSetting;
            switch (pEnum)
            {
                #region default
                default:
                    isSetting = false;
                    break;
                    #endregion default
            }
            if (isSetting)
                Ccis.Finalize(pCci.ClientId_WithoutPrefix, processQueue);
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
            if (this.IsCciOfContainer(pCci.ClientId_WithoutPrefix))
            {
                string clientId_Key = CciContainerKey(pCci.ClientId_WithoutPrefix);
                if (System.Enum.IsDefined(typeof(CciEnum), clientId_Key))
                {
                    CciEnum key = (CciEnum)System.Enum.Parse(typeof(CciEnum), clientId_Key);

                    switch (key)
                    {
                        #region Buyer/Seller: Calcul des BCs
                        case CciEnum.payer:
                            Ccis.Synchronize(CciClientIdReceiver, pCci.NewValue, pCci.LastValue);
                            break;
                        case CciEnum.receiver:
                            Ccis.Synchronize(CciClientIdPayer, pCci.NewValue, pCci.LastValue);
                            break;
                        #endregion
                        default:
                            break;
                    }
                }
                else if (System.Enum.IsDefined(typeof(CciEnumCalculation), clientId_Key))
                {
                    CciEnumCalculation key = (CciEnumCalculation)System.Enum.Parse(typeof(CciEnumCalculation), clientId_Key);

                    switch (key)
                    {
                        #region FloatingRateIndex
                        case CciEnumCalculation.calculation_rate:
                            Ccis.ProcessInitialize_DCF(CciClientId(CciEnumCalculation.calculation_dayCountFraction), 
                            CciClientId(CciEnumCalculation.calculation_rate));
                            SetResetDates();
                        break;
                        #endregion

                        default:
                            break;
                    }

                }


            }
        }
        #endregion ProcessInitialize
        #region IsClientId_XXXXX
        public bool IsClientId_PayerOrReceiver(CustomCaptureInfo pCci)
        {
            bool isOk = false;
            isOk = isOk || (CciClientIdPayer == pCci.ClientId_WithoutPrefix);
            isOk = isOk || (CciClientIdReceiver == pCci.ClientId_WithoutPrefix);
            return isOk;
        }
        #endregion IsClientId_XXXXX
        #region CleanUp
        virtual public void CleanUp()
        {
        }
        #endregion CleanUp
        #region SetDisplay
        virtual public void SetDisplay(CustomCaptureInfo pCci)
        {
        }
        #endregion
        #region RemoveLastItemInArray
        public void RemoveLastItemInArray(string _)
        {
        }
        #endregion RemoveLastItemInArray
        #region RefreshCciEnabled
        public void RefreshCciEnabled()
        {
        }
        #endregion
        #region Initialize_Document
        public void Initialize_Document()
        {
        }
        #endregion Initialize_Document
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pPage"></param>
        /// FI 20140805 [XXXXX] add Method
        public void DumpSpecific_ToGUI(CciPageBase pPage)
        {
            CustomCaptureInfo cci = Cci(CciEnumCalculation.calculation_rate);
            if (null != cci)
                pPage.SetOpenFormReferential(cci, Cst.OTCml_TBL.ASSET_RATEINDEX);
        }

        #endregion Membres de IContainerCciFactory

        #region Membres de ITradeGetInfoButton
        #region public override SetButtonZoom
        public bool SetButtonZoom(CustomCaptureInfo pCci, CustomObjectButtonFpmlObject pCo, ref bool pIsSpecified, ref bool pIsEnabled)
        {
            bool isOk = false;
            //
            if (this.IsCciOfContainer(pCci.ClientId_WithoutPrefix))
            {
                isOk = this.IsCci(CciEnum.resetDates, pCci);
                if (isOk)
                {
                    pCo.Element = "interestLegResetDates";
                    //pCo.Object = "interestLegCalculationPeriodDates";
                    pCo.OccurenceValue = 1;
                    pIsSpecified = true;
                    pIsEnabled = true;
                }
                if (false == isOk)
                {
                    isOk = this.IsCci(CciEnum.paymentDates, pCci);
                    if (isOk)
                    {

                        pCo.Element = "interestLegPaymentDates";
                        //pCo.Object = "interestLegCalculationPeriodDates";
                        pCo.OccurenceValue = _legNumber;
                        pIsSpecified = _interestLeg.PaymentDates.AdjustableDatesSpecified || _interestLeg.PaymentDates.RelativeDatesSpecified;
                        pIsEnabled = true;
                    }
                }

                if (false == isOk)
                {
                    isOk = this.IsCci(CciEnumCalculation.calculation_floatingRate_capRateSchedule_initialValue, pCci);
                    if (isOk)
                    {

                        pCo.Element = "capRateSchedule";
                        pCo.Object = "rateFloatingRate";
                        pCo.OccurenceValue = _legNumber;
                        pIsSpecified = IsCapRateScheduleSpecified;
                        pIsEnabled = IsFloatingRateSpecified;
                    }
                }

                if (false == isOk)
                {
                    isOk = this.IsCci(CciEnumCalculation.calculation_floatingRate_floorRateSchedule_initialValue, pCci);
                    if (isOk)
                    {

                        pCo.Element = "floorRateSchedule";
                        pCo.Object = "rateFloatingRate";
                        pCo.OccurenceValue = _legNumber;
                        pIsSpecified = IsFloorRateScheduleSpecified;
                        pIsEnabled = IsFloatingRateSpecified;
                    }
                }

                if (false == isOk)
                {
                    isOk = this.IsCci(CciEnumCalculation.calculation_floatingRate_spreadSchedule_initialValue, pCci);
                    if (isOk)
                    {

                        pCo.Element = "spreadSchedule";
                        pCo.Object = "rateFloatingRate";
                        pCo.OccurenceValue = _legNumber;
                        pIsSpecified = IsSpreadScheduleSpecified;
                        pIsEnabled = IsFloatingRateSpecified;
                    }
                }

                if (false == isOk)
                {
                    isOk = this.IsCci(CciEnumCalculation.calculation_floatingRate_multiplierSchedule_initialValue, pCci);
                    if (isOk)
                    {

                        pCo.Element = "floatingRateMultiplierSchedule";
                        pCo.Object = "rateFloatingRate";
                        pCo.OccurenceValue = _legNumber;
                        pIsSpecified = IsSpreadScheduleSpecified;
                        pIsEnabled = IsFloatingRateSpecified;
                    }
                }
            }
            return isOk;
        }
        #endregion
        #endregion Membres de ITradeGetInfoButton

        #region Methods
        #region GenerateId
        private string GenerateId(string pKey)
        {
            if (StrFunc.IsFilled(_prefix))
                pKey = StrFunc.FirstUpperCase(pKey);
            return _cciTrade.DataDocument.GenerateId(PrefixId + pKey + _legNumber, false);
        }
        #endregion GenerateId
        #region GetCalculationPeriodDatesId
        private string GetCalculationPeriodDatesId()
        {
            if (StrFunc.IsEmpty(_interestLeg.CalculationPeriodDates.Id))
                _interestLeg.CalculationPeriodDates.Id = GenerateId(TradeCustomCaptureInfos.CCst.CALCULATION_PERIOD_DATES_REFERENCE);
            return _interestLeg.CalculationPeriodDates.Id; 
        }
        #endregion GetCalculationPeriodDatesId

        #region GetSqlAssetRateIndex
        private SQL_AssetRateIndex GetSqlAssetRateIndex()
        {
            SQL_AssetRateIndex ret = null;
            if (IsFloatingRateSpecified)
                ret = _interestLeg.InterestCalculation.FloatingRate.GetSqlAssetRateIndex(_cciTrade.CSCacheOn);
            return ret;
        }
        #endregion GetSqlAssetRateIndex

        #region SetResetDates
        /// <summary>
        /// Alimente irs.resetDates si le stream est à taux flottant
        /// </summary>
        private void SetResetDates()
        {
            IInterestCalculation calculation = _interestLeg.InterestCalculation;
            if (calculation.FloatingRateSpecified)
            {
                IInterestLegResetDates _resetDates = _interestLeg.CalculationPeriodDates.ResetDates;
                SQL_AssetRateIndex slqAssetRateIndex = GetSqlAssetRateIndex();
                _resetDates.CalculationPeriodDatesReference.HRef = GetCalculationPeriodDatesId();
                //ResetRelativeTo
                if (null != slqAssetRateIndex)
                {
                    string resetRelativeTo = slqAssetRateIndex.Idx_RelativeToResetDt;
                    _resetDates.ResetRelativeToSpecified = false;
                    if (System.Enum.IsDefined(typeof(ResetRelativeToEnum), resetRelativeTo))
                    {
                        _resetDates.ResetRelativeToSpecified = true;
                        _resetDates.ResetRelativeTo = (ResetRelativeToEnum)System.Enum.Parse(typeof(ResetRelativeToEnum), resetRelativeTo, true);
                        if (Ccis.Contains(CciClientId(CciEnumResetDates.resetDates_resetRelativeTo)))
                            Ccis.SetNewValue(CciClientId(CciEnumResetDates.resetDates_resetRelativeTo), resetRelativeTo, false);
                    }
                }

                //Fixing Dates
                _resetDates.InitialFixingDateSpecified = false;
                if (null != slqAssetRateIndex)
                {
                    Ccis.DumpRelativeDateOffset_ToDocument(_resetDates.FixingDates.RelativeDateOffset, slqAssetRateIndex, 
                    GetCalculationPeriodDatesId(), TradeCustomCaptureInfos.CCst.RESET_BUSINESS_CENTERS_REFERENCE + _legNumber.ToString());
                }

                //ResetFrequency
                if (null != slqAssetRateIndex)
                {
                    string weeklyRollConvention = slqAssetRateIndex.Asset_WeeklyRollConvResetDT;
                    if (Enum.IsDefined(typeof(WeeklyRollConventionEnum), weeklyRollConvention))
                        _resetDates.ResetFrequency.WeeklyRollConvention = (WeeklyRollConventionEnum) ReflectionTools.EnumParse(new WeeklyRollConventionEnum(), weeklyRollConvention);
                }
            }
        }
        #endregion SetResetDates
        #endregion Methods

    }
}