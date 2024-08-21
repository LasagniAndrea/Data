#region Using Directives
using EFS.ACommon;
using EFS.Common;
using EFS.Common.Web;
using EFS.GUI.CCI;
using EfsML.Enum.Tools;
using FpML.Enum;
using FpML.Interface;
using System;
using System.Linq;
#endregion Using Directives

namespace EFS.TradeInformation
{
    /// <summary>
    /// Description résumée de TradeFxAverageRateOption.
    /// </summary>
    public  class CciProductFxAverageRateOption : CciProductFXOptionLeg
    {
        #region Members
        private IFxAverageRateOption _fxAverageRateOption;
        #endregion Members
        //
        #region Enum
        public new enum CciEnum
        {
            #region buyer/seller
            [System.Xml.Serialization.XmlEnumAttribute("buyerPartyReference")]
            buyer,
            [System.Xml.Serialization.XmlEnumAttribute("sellerPartyReference")]
            seller,
            #endregion buyer/seller
            #region optionType
            [System.Xml.Serialization.XmlEnumAttribute("optionType")]
            optionType,
            #endregion
            #region expiryDateTime
            [System.Xml.Serialization.XmlEnumAttribute("expiryDateTime.expiryDate")]
            expiryDateTime_expiryDate,
            [System.Xml.Serialization.XmlEnumAttribute("expiryDateTime.expiryTime.hourMinuteTime")]
            expiryDateTime_expiryTime_hourMinuteTime,
            [System.Xml.Serialization.XmlEnumAttribute("expiryDateTime.expiryTime.businessCenter")]
            expiryDateTime_expiryTime_businessCenter,
            [System.Xml.Serialization.XmlEnumAttribute("expiryDateTime.cutName")]
            expiryDateTime_cutName,
            #endregion expiryDateTime
            #region exerciseStyle
            [System.Xml.Serialization.XmlEnumAttribute("exerciseStyle")]
            exerciseStyle,
            #endregion
            #region valueDate
            [System.Xml.Serialization.XmlEnumAttribute("valueDate")]
            valueDate,
            #endregion
            #region callCurrencyAmount
            [System.Xml.Serialization.XmlEnumAttribute("callCurrencyAmount.amount")]
            callCurrencyAmount_amount,
            [System.Xml.Serialization.XmlEnumAttribute("callCurrencyAmount.currency")]
            callCurrencyAmount_currency,
            #endregion callCurrencyAmount
            #region putCurrencyAmount
            [System.Xml.Serialization.XmlEnumAttribute("putCurrencyAmount.amount")]
            putCurrencyAmount_amount,
            [System.Xml.Serialization.XmlEnumAttribute("putCurrencyAmount.currency")]
            putCurrencyAmount_currency,
            #endregion putCurrencyAmount
            #region fxStrikePrice
            [System.Xml.Serialization.XmlEnumAttribute("fxStrikePrice.rate")]
            fxStrikePrice_rate,
            [System.Xml.Serialization.XmlEnumAttribute("fxStrikePrice.strikeQuoteBasis")]
            fxStrikePrice_strikeQuoteBasis,
            #endregion fxStrikePrice
            #region procedureSpecified
            [System.Xml.Serialization.XmlEnumAttribute("procedure")]
            procedureSpecified,
            #endregion procedureSpecified
            #region spotRate
            [System.Xml.Serialization.XmlEnumAttribute("spotRate")]
            spotRate,
            #endregion spotRate
            #region payout
            [System.Xml.Serialization.XmlEnumAttribute("payoutCurrency")]
            payoutCurrency,
            [System.Xml.Serialization.XmlEnumAttribute("payoutFormula")]
            payoutFomula,
            #endregion payout
            #region geometricAverageSpecified
            geometricAverageSpecified,
            #endregion geometricAverageSpecified
            #region AssetFxRate
            // EG 20160404 Migration vs2013
            // #warning TO BE DEFINE and REPLACE XmlEnumAttribute in All CciEnum
            //[CciInstance("primaryRateSource.rateSource")]
            //[CciInstance("primaryRateSource.rateSourcePage")]
            //[CciInstance("primaryRateSource.rateSourcePageHeading")]
            assetFxRate,
            #endregion AssetFxRate
            #region averageRateQuoteBasis
            [System.Xml.Serialization.XmlEnumAttribute("averageRateQuoteBasis")]
            averageRateQuoteBasis,
            #endregion averageRateQuoteBasis
            #region primaryRateSource
            [System.Xml.Serialization.XmlEnumAttribute("primaryRateSource.rateSource")]
            primaryRateSource_rateSource,
            [System.Xml.Serialization.XmlEnumAttribute("primaryRateSource.rateSourcePage")]
            primaryRateSource_rateSourcePage,
            [System.Xml.Serialization.XmlEnumAttribute("primaryRateSource.rateSourcePageHeading")]
            primaryRateSource_rateSourcePageHeading,
            #endregion primaryRateSource
            #region secondaryRateSource
            [System.Xml.Serialization.XmlEnumAttribute("secondaryRateSource.rateSource")]
            secondaryRateSource_rateSource,
            [System.Xml.Serialization.XmlEnumAttribute("secondaryRateSource.rateSourcePage")]
            secondaryRateSource_rateSourcePage,
            [System.Xml.Serialization.XmlEnumAttribute("secondaryRateSource.rateSourcePageHeading")]
            secondaryRateSource_rateSourcePageHeading,
            #endregion secondaryRateSource
            #region fixingTime
            [System.Xml.Serialization.XmlEnumAttribute("fixingTime.hourMinuteTime")]
            fixingTime_hourMinuteTime,
            [System.Xml.Serialization.XmlEnumAttribute("fixingTime.businessCenter")]
            fixingTime_businessCenter,
            #endregion fixingTime

            #region FxAverageRateObservationSchedule
            averageRateObservationScheduleSpecified,
            [System.Xml.Serialization.XmlEnumAttribute("rateObservationSchedule.observationStartDate")]
            averageRateObservationSchedule_observationStartDate,
            [System.Xml.Serialization.XmlEnumAttribute("rateObservationSchedule.observationEndDate")]
            averageRateObservationSchedule_observationEndDate,
            [System.Xml.Serialization.XmlEnumAttribute("rateObservationSchedule.calculationPeriodFrequency.periodMultiplier")]
            averageRateObservationSchedule_calculationPeriodFrequency_periodMultiplier,
            [System.Xml.Serialization.XmlEnumAttribute("rateObservationSchedule.calculationPeriodFrequency.period")]
            averageRateObservationSchedule_calculationPeriodFrequency_period,
            [System.Xml.Serialization.XmlEnumAttribute("rateObservationSchedule.calculationPeriodFrequency.rollConvention")]
            averageRateObservationSchedule_calculationPeriodFrequency_rollConvention,
            #endregion FxAverageRateObservationSchedule
            #region FxaverageRateObservationDate
            [System.Xml.Serialization.XmlEnumAttribute("rateObservationDate")]
            averageRateObservationDate,
            averageRateObservationDateSpecified,
            #endregion FxaverageRateObservationDate
            #region averageStrikeOption
            [System.Xml.Serialization.XmlEnumAttribute("averageStrikeOption.settlementType")]
            averageStrikeOption_settlementType,
            #endregion averageStrikeOption
            unknown,
        }
        #endregion Enum
        //
        #region public property
        public bool ExistCciAssetFxRate
        {
            //Existe-t-il un zone Asset Fx Rate
            get { return CcisBase.Contains(CciClientId(CciEnum.assetFxRate.ToString())); }
        }
        #endregion public property
        //
        #region Constructors
        public CciProductFxAverageRateOption(CciTrade pCciTrade, IFxAverageRateOption pFxAverageRateOption, string pPrefix)
            : this(pCciTrade, pFxAverageRateOption, pPrefix, -1) { }
        public CciProductFxAverageRateOption(CciTrade pCciTrade, IFxAverageRateOption pFxAverageRateOption, string pPrefix, int pNumber)
            : base(pCciTrade, pFxAverageRateOption, CciProductFXOptionLeg.FxProduct.FxAverageRateOption, pPrefix, pNumber)
        {
            _fxAverageRateOption = pFxAverageRateOption;
        }
        #endregion Constructors
        
        #region Membres de IContainerCciFactory
        #region public override Initialize_FromCci
        public override void Initialize_FromCci()
        {
            CciTools.CreateInstance(this, _fxAverageRateOption);
            if (CcisBase.Contains(CciClientId(CciEnum.averageStrikeOption_settlementType.ToString())) &&
                (null == _fxAverageRateOption.AverageStrikeOption))
                _fxAverageRateOption = CciTrade.CurrentTrade.Product.ProductBase.CreateFxAverageRateOption();
            base.Initialize_FromCci();
        }
        #endregion Initialize_FromCci
        #region public override AddCciSystem
        /// <summary>
        /// 
        /// </summary>
        /// FI 20170116 [21916] Modify (use AddCciSystem Method)
        public override void AddCciSystem()
        {
            CciTools.AddCciSystem(CcisBase, Cst.HCK + CciClientId(CciEnum.averageRateObservationScheduleSpecified.ToString()), true, TypeData.TypeDataEnum.@bool);
            CciTools.AddCciSystem(CcisBase, Cst.BUT + CciClientId(CciEnum.averageRateObservationDate.ToString()), false, TypeData.TypeDataEnum.@string);
            CciTools.AddCciSystem(CcisBase, Cst.HCK + CciClientId(CciEnum.averageRateObservationDateSpecified.ToString()), true, TypeData.TypeDataEnum.@bool);
            base.AddCciSystem();
        }
        #endregion AddCciSystem
        #region public override Initialize_FromDocument
        /// <summary>
        /// Initialisation des CCI à partir des données "PRODUCT" présentes dans les classes du Document XML
        /// </summary>
        public override void Initialize_FromDocument()
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
                    //
                    switch (cciEnum)
                    {
                        #region expiryDateTime
                        case CciEnum.expiryDateTime_expiryDate:
                            data = _fxAverageRateOption.ExpiryDateTime.ExpiryDate.Value;
                            break;
                        case CciEnum.expiryDateTime_expiryTime_hourMinuteTime:
                            data = _fxAverageRateOption.ExpiryDateTime.ExpiryTime.Value;
                            break;
                        case CciEnum.expiryDateTime_expiryTime_businessCenter:
                            data = _fxAverageRateOption.ExpiryDateTime.BusinessCenter;
                            break;
                        case CciEnum.expiryDateTime_cutName:
                            if (_fxAverageRateOption.ExpiryDateTime.CutNameSpecified)
                                data = _fxAverageRateOption.ExpiryDateTime.CutName;
                            break;
                        #endregion expiryDateTime
                        #region spotRate
                        case CciEnum.spotRate:
                            if (_fxAverageRateOption.SpotRateSpecified)
                                data = _fxAverageRateOption.SpotRate.Value;
                            break;
                        #endregion spotRate
                        #region payout
                        case CciEnum.payoutCurrency:
                            data = _fxAverageRateOption.PayoutCurrency.Value;
                            break;
                        case CciEnum.payoutFomula:
                            if (_fxAverageRateOption.PayoutFormulaSpecified)
                                data = _fxAverageRateOption.PayoutFormula;
                            break;
                        #endregion payout
                        #region Asset_FxRate
                        case CciEnum.assetFxRate:
                            try
                            {
                                int idAsset = _fxAverageRateOption.PrimaryRateSource.OTCmlId;
                                if (idAsset > 0)
                                {
                                    SQL_AssetFxRate sql_AssetFxRate = new SQL_AssetFxRate(CciTrade.CSCacheOn, idAsset);
                                    if (sql_AssetFxRate.IsLoaded)
                                    {
                                        cci.Sql_Table = sql_AssetFxRate;
                                        data = sql_AssetFxRate.Identifier;
                                        _fxAverageRateOption.PrimaryRateSource.SetAssetFxRateId(idAsset, data);
                                    }
                                }
                            }
                            catch
                            {
                                cci.Sql_Table = null;
                                data = string.Empty;
                            }
                            break;
                        #endregion Asset_FxRate
                        #region averageRateQuoteBasis
                        case CciEnum.averageRateQuoteBasis:
                            data = _fxAverageRateOption.AverageRateQuoteBasis.ToString();
                            break;
                        #endregion averageRateQuoteBasis
                        #region primaryRateSource
                        case CciEnum.primaryRateSource_rateSource:
                            data = _fxAverageRateOption.PrimaryRateSource.RateSource.Value;
                            break;
                        case CciEnum.primaryRateSource_rateSourcePage:
                            if (_fxAverageRateOption.PrimaryRateSource.RateSourcePageSpecified)
                                data = _fxAverageRateOption.PrimaryRateSource.RateSourcePage.Value;
                            break;
                        case CciEnum.primaryRateSource_rateSourcePageHeading:
                            if (_fxAverageRateOption.PrimaryRateSource.RateSourcePageHeadingSpecified)
                                data = _fxAverageRateOption.PrimaryRateSource.RateSourcePageHeading;
                            break;
                        #endregion primaryRateSource
                        #region secondaryRateSource
                        case CciEnum.secondaryRateSource_rateSource:
                            if (_fxAverageRateOption.SecondaryRateSourceSpecified)
                                data = _fxAverageRateOption.SecondaryRateSource.RateSource.Value;
                            break;
                        case CciEnum.secondaryRateSource_rateSourcePage:
                            if (_fxAverageRateOption.SecondaryRateSourceSpecified && _fxAverageRateOption.SecondaryRateSource.RateSourcePageSpecified)
                                data = _fxAverageRateOption.SecondaryRateSource.RateSourcePage.Value;
                            break;
                        case CciEnum.secondaryRateSource_rateSourcePageHeading:
                            if (_fxAverageRateOption.SecondaryRateSourceSpecified && _fxAverageRateOption.SecondaryRateSource.RateSourcePageHeadingSpecified)
                                data = _fxAverageRateOption.SecondaryRateSource.RateSourcePageHeading;
                            break;
                        #endregion secondaryRateSource
                        #region fixingTime
                        case CciEnum.fixingTime_hourMinuteTime:
                            data = _fxAverageRateOption.FixingTime.HourMinuteTime.Value;
                            break;
                        case CciEnum.fixingTime_businessCenter:
                            data = _fxAverageRateOption.FixingTime.BusinessCenter.Value;
                            break;
                        #endregion
                        #region averageRateObservationSchedule
                        case CciEnum.averageRateObservationScheduleSpecified:
                            data = _fxAverageRateOption.RateObservationScheduleSpecified.ToString().ToLower();
                            break;
                        case CciEnum.averageRateObservationSchedule_observationStartDate:
                            if (_fxAverageRateOption.RateObservationScheduleSpecified)
                                data = _fxAverageRateOption.RateObservationSchedule.ObservationStartDate.Value;
                            break;
                        case CciEnum.averageRateObservationSchedule_observationEndDate:
                            if (_fxAverageRateOption.RateObservationScheduleSpecified)
                                data = _fxAverageRateOption.RateObservationSchedule.ObservationEndDate.Value;
                            break;
                        case CciEnum.averageRateObservationSchedule_calculationPeriodFrequency_period:
                            if (_fxAverageRateOption.RateObservationScheduleSpecified)
                                data = _fxAverageRateOption.RateObservationSchedule.CalculationPeriodFrequency.Interval.Period.ToString();
                            break;
                        case CciEnum.averageRateObservationSchedule_calculationPeriodFrequency_periodMultiplier:
                            if (_fxAverageRateOption.RateObservationScheduleSpecified)
                                data = _fxAverageRateOption.RateObservationSchedule.CalculationPeriodFrequency.Interval.PeriodMultiplier.Value;
                            break;

                        case CciEnum.averageRateObservationSchedule_calculationPeriodFrequency_rollConvention:
                            if (_fxAverageRateOption.RateObservationScheduleSpecified)
                                data = _fxAverageRateOption.RateObservationSchedule.CalculationPeriodFrequency.RollConvention.ToString();
                            break;
                        #endregion averageRateObservationSchedule
                        #region averageRateObservationDateSpecified
                        case CciEnum.averageRateObservationDateSpecified:
                            data = _fxAverageRateOption.RateObservationDateSpecified.ToString().ToLower();
                            break;
                        #endregion averageRateObservedRate
                        #region geometricAverageSpecified
                        case CciEnum.geometricAverageSpecified:
                            data = _fxAverageRateOption.GeometricAverageSpecified.ToString().ToLower();
                            break;
                        #endregion geometricAverageSpecified
                        #region averageStrikeOption_settlementType
                        case CciEnum.averageStrikeOption_settlementType:
                            if (_fxAverageRateOption.AverageStrikeOptionSpecified)
                                data = _fxAverageRateOption.AverageStrikeOption.SettlementType.ToString();
                            break;
                        #endregion averageStrikeOption_settlementType
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
            base.Initialize_FromDocument();
        }
        #endregion Initialize_FromDocument
        #region public override Dump_ToDocument
        public override void Dump_ToDocument()
        {
            foreach (string clientId in CcisBase.ClientId_DumpToDocument.Where(x => IsCciOfContainer(x)))
            {
                string cliendId_Key = CciContainerKey(clientId);
                if (Enum.IsDefined(typeof(CciEnum), cliendId_Key))
                {
                    CustomCaptureInfo cci = CcisBase[clientId];
                    CciEnum cciEnum = (CciEnum)System.Enum.Parse(typeof(CciEnum), cliendId_Key);
                    #region Reset variables
                    string data = cci.NewValue;
                    bool isSetting = true;
                    bool isFilled = StrFunc.IsFilled(data);
                    CustomCaptureInfosBase.ProcessQueueEnum processQueue = CustomCaptureInfosBase.ProcessQueueEnum.None;
                    #endregion Reset variables

                    switch (cciEnum)
                    {
                        #region expiryDateTime
                        case CciEnum.expiryDateTime_expiryDate:
                            _fxAverageRateOption.ExpiryDateTime.ExpiryDate.Value = data;
                            break;
                        case CciEnum.expiryDateTime_expiryTime_hourMinuteTime:
                            _fxAverageRateOption.ExpiryDateTime.ExpiryTime.Value = data;
                            break;
                        case CciEnum.expiryDateTime_expiryTime_businessCenter:
                            _fxAverageRateOption.ExpiryDateTime.BusinessCenter = data;
                            break;
                        case CciEnum.expiryDateTime_cutName:
                            _fxAverageRateOption.ExpiryDateTime.CutNameSpecified = cci.IsFilledValue;
                            if (_fxAverageRateOption.ExpiryDateTime.CutNameSpecified)
                                _fxAverageRateOption.ExpiryDateTime.CutName = data;
                            break;
                        #endregion expiryDateTime
                        #region spotRate
                        case CciEnum.spotRate:
                            _fxAverageRateOption.SpotRateSpecified = cci.IsFilledValue;
                            _fxAverageRateOption.SpotRate.Value = data;
                            break;
                        #endregion spotRate
                        #region payout
                        case CciEnum.payoutCurrency:
                            _fxAverageRateOption.PayoutCurrency.Value = data;
                            break;
                        case CciEnum.payoutFomula:
                            _fxAverageRateOption.PayoutFormulaSpecified = cci.IsFilledValue;
                            _fxAverageRateOption.PayoutFormula = data;
                            break;
                        #endregion payout
                        #region 	Asset_FxRate
                        case CciEnum.assetFxRate:
                            SQL_AssetFxRate sql_asset = null;
                            bool isLoaded = false;
                            cci.ErrorMsg = string.Empty;
                            //
                            if (StrFunc.IsFilled(data))
                            {
                                //Check if actor is a valid css
                                for (int i = 0; i < 2; i++)
                                {
                                    string dataToFind = data;
                                    if (i == 1)
                                        dataToFind = data.Replace(" ", "%") + "%";
                                    sql_asset = new SQL_AssetFxRate(CciTrade.CSCacheOn, SQL_TableWithID.IDType.Identifier, dataToFind, SQL_Table.ScanDataDtEnabledEnum.Yes);
                                    isLoaded = sql_asset.IsLoaded && (sql_asset.RowsCount == 1);
                                    //
                                    if (isLoaded)
                                        break;
                                }
                                //
                                if (isLoaded)
                                {
                                    cci.NewValue = sql_asset.Identifier;
                                    cci.Sql_Table = sql_asset;
                                    //
                                    #region fixingTime
                                    _fxAverageRateOption.FixingTime.HourMinuteTime.TimeValue = sql_asset.TimeRateSrc;
                                    _fxAverageRateOption.FixingTime.BusinessCenter.Value = sql_asset.IdBC_RateSrc;
                                    #endregion fixingTime
                                    //
                                    #region averageRateQuoteBasis
                                    if ((_fxAverageRateOption.CallCurrencyAmount.Currency == sql_asset.QCP_Cur1) &&
                                        (_fxAverageRateOption.PutCurrencyAmount.Currency == sql_asset.QCP_Cur2))
                                    {
                                        if (sql_asset.QCP_QuoteBasisEnum == QuoteBasisEnum.Currency1PerCurrency2)
                                            _fxAverageRateOption.AverageRateQuoteBasis = StrikeQuoteBasisEnum.CallCurrencyPerPutCurrency;
                                        else
                                            _fxAverageRateOption.AverageRateQuoteBasis = StrikeQuoteBasisEnum.PutCurrencyPerCallCurrency;
                                    }
                                    else if ((_fxAverageRateOption.PutCurrencyAmount.Currency == sql_asset.QCP_Cur1) &&
                                        (_fxAverageRateOption.CallCurrencyAmount.Currency == sql_asset.QCP_Cur2))
                                    {
                                        if (sql_asset.QCP_QuoteBasisEnum == QuoteBasisEnum.Currency1PerCurrency2)
                                            _fxAverageRateOption.AverageRateQuoteBasis = StrikeQuoteBasisEnum.PutCurrencyPerCallCurrency;
                                        else
                                            _fxAverageRateOption.AverageRateQuoteBasis = StrikeQuoteBasisEnum.CallCurrencyPerPutCurrency;
                                    }
                                    #endregion averageRateQuoteBasis
                                    //
                                    #region primaryRateSource
                                    if (null == _fxAverageRateOption.PrimaryRateSource)
                                    {
                                        _fxAverageRateOption.PrimaryRateSource = _fxAverageRateOption.CreateInformationSource;
                                    }
                                    _fxAverageRateOption.PrimaryRateSource.OTCmlId = sql_asset.Id;
                                    _fxAverageRateOption.PrimaryRateSource.RateSource.Value = sql_asset.PrimaryRateSrc;
                                    _fxAverageRateOption.PrimaryRateSource.AssetFxRateId.Value = sql_asset.Identifier;
                                    //
                                    _fxAverageRateOption.PrimaryRateSource.RateSourcePageSpecified = StrFunc.IsFilled(sql_asset.PrimaryRateSrcPage);
                                    if (_fxAverageRateOption.PrimaryRateSource.RateSourcePageSpecified)
                                        _fxAverageRateOption.PrimaryRateSource.CreateRateSourcePage(sql_asset.PrimaryRateSrcPage);
                                    //
                                    _fxAverageRateOption.PrimaryRateSource.RateSourcePageHeadingSpecified = StrFunc.IsFilled(sql_asset.PrimaryRateSrcHead);
                                    if (_fxAverageRateOption.PrimaryRateSource.RateSourcePageHeadingSpecified)
                                        _fxAverageRateOption.PrimaryRateSource.RateSourcePageHeading = sql_asset.PrimaryRateSrcHead;
                                    #endregion primaryRateSource
                                    //
                                    #region secondaryRateSource
                                    _fxAverageRateOption.SecondaryRateSourceSpecified = StrFunc.IsFilled(sql_asset.SecondaryRateSrc);
                                    if (_fxAverageRateOption.SecondaryRateSourceSpecified)
                                    {
                                        if (null == _fxAverageRateOption.SecondaryRateSource)
                                            _fxAverageRateOption.SecondaryRateSource = _fxAverageRateOption.CreateInformationSource;

                                        _fxAverageRateOption.SecondaryRateSource.RateSource.Value = sql_asset.SecondaryRateSrc;
                                        //
                                        _fxAverageRateOption.SecondaryRateSource.RateSourcePageSpecified = StrFunc.IsFilled(sql_asset.SecondaryRateSrcPage);
                                        if (_fxAverageRateOption.SecondaryRateSource.RateSourcePageSpecified)
                                            _fxAverageRateOption.SecondaryRateSource.CreateRateSourcePage(sql_asset.SecondaryRateSrcPage);
                                        //
                                        _fxAverageRateOption.SecondaryRateSource.RateSourcePageHeadingSpecified = StrFunc.IsFilled(sql_asset.SecondaryRateSrcHead);
                                        if (_fxAverageRateOption.SecondaryRateSource.RateSourcePageHeadingSpecified)
                                            _fxAverageRateOption.SecondaryRateSource.RateSourcePageHeading = sql_asset.SecondaryRateSrcHead;
                                    }
                                    #endregion secondaryRateSource

                                }
                                //
                                cci.ErrorMsg = ((false == isLoaded) ? Ressource.GetString("Msg_AssetNotFound") : string.Empty);
                            }
                            processQueue = CustomCaptureInfosBase.ProcessQueueEnum.High;//Afin de préproposer les BCs
                            break;
                        #endregion 	Asset_FxRate
                        #region averageRateQuoteBasis
                        case CciEnum.averageRateQuoteBasis:
                            if (StrFunc.IsFilled(data))
                            {
                                StrikeQuoteBasisEnum sQBEnum = (StrikeQuoteBasisEnum)System.Enum.Parse(typeof(StrikeQuoteBasisEnum), data, true);
                                _fxAverageRateOption.AverageRateQuoteBasis = sQBEnum;
                            }
                            break;
                        #endregion averageRateQuoteBasis
                        #region primaryRateSource
                        case CciEnum.primaryRateSource_rateSource:
                            _fxAverageRateOption.PrimaryRateSource.RateSource.Value = data;
                            break;
                        case CciEnum.primaryRateSource_rateSourcePage:
                            _fxAverageRateOption.PrimaryRateSource.RateSourcePageSpecified = cci.IsFilledValue;
                            _fxAverageRateOption.PrimaryRateSource.RateSourcePage.Value = data;
                            break;
                        case CciEnum.primaryRateSource_rateSourcePageHeading:
                            _fxAverageRateOption.PrimaryRateSource.RateSourcePageHeadingSpecified = cci.IsFilledValue;
                            _fxAverageRateOption.PrimaryRateSource.RateSourcePageHeading = data;
                            break;
                        #endregion primaryRateSource
                        #region secondaryRateSource
                        case CciEnum.secondaryRateSource_rateSource:
                            _fxAverageRateOption.SecondaryRateSourceSpecified = cci.IsFilledValue;
                            if (_fxAverageRateOption.SecondaryRateSourceSpecified)
                                _fxAverageRateOption.SecondaryRateSource.RateSource.Value = data;
                            break;
                        case CciEnum.secondaryRateSource_rateSourcePage:
                            _fxAverageRateOption.SecondaryRateSource.RateSourcePageSpecified = cci.IsFilledValue;
                            _fxAverageRateOption.SecondaryRateSource.RateSourcePage.Value = data;
                            break;
                        case CciEnum.secondaryRateSource_rateSourcePageHeading:
                            _fxAverageRateOption.SecondaryRateSource.RateSourcePageHeadingSpecified = cci.IsFilledValue;
                            _fxAverageRateOption.SecondaryRateSource.RateSourcePageHeading = data;
                            break;
                        #endregion secondaryRateSource
                        #region fixingTime
                        case CciEnum.fixingTime_hourMinuteTime:
                            _fxAverageRateOption.FixingTime.HourMinuteTime.Value = data;
                            break;
                        case CciEnum.fixingTime_businessCenter:
                            _fxAverageRateOption.FixingTime.BusinessCenter.Value = data;
                            break;
                        #endregion fixingTime
                        #region averageRateObservationSchedule
                        case CciEnum.averageRateObservationScheduleSpecified:
                            _fxAverageRateOption.RateObservationScheduleSpecified = cci.IsFilledValue;
                            processQueue = CustomCaptureInfosBase.ProcessQueueEnum.Low;
                            break;
                        case CciEnum.averageRateObservationSchedule_observationStartDate:
                            _fxAverageRateOption.RateObservationScheduleSpecified = cci.IsFilledValue;
                            _fxAverageRateOption.RateObservationSchedule.ObservationStartDate.Value = data;
                            processQueue = CustomCaptureInfosBase.ProcessQueueEnum.Low;
                            break;
                        case CciEnum.averageRateObservationSchedule_observationEndDate:
                            _fxAverageRateOption.RateObservationSchedule.ObservationEndDate.Value = data;
                            break;
                        case CciEnum.averageRateObservationSchedule_calculationPeriodFrequency_period:
                            if (cci.IsFilledValue)
                            {
                                PeriodEnum periodEnum = StringToEnum.Period(data);
                                _fxAverageRateOption.RateObservationSchedule.CalculationPeriodFrequency.Interval.Period = periodEnum;
                            }
                            break;
                        case CciEnum.averageRateObservationSchedule_calculationPeriodFrequency_periodMultiplier:
                            _fxAverageRateOption.RateObservationSchedule.CalculationPeriodFrequency.Interval.PeriodMultiplier.Value = data;
                            break;
                        case CciEnum.averageRateObservationSchedule_calculationPeriodFrequency_rollConvention:
                            if (cci.IsFilledValue)
                            {
                                RollConventionEnum rollConventionEnum = (RollConventionEnum)System.Enum.Parse(typeof(RollConventionEnum), data, true);
                                _fxAverageRateOption.RateObservationSchedule.CalculationPeriodFrequency.RollConvention = rollConventionEnum;
                            }
                            break;
                        #endregion averageRateObservationSchedule
                        #region averageRateObservationDate
                        case CciEnum.averageRateObservationDateSpecified:
                            _fxAverageRateOption.RateObservationDateSpecified = cci.IsFilledValue;
                            processQueue = CustomCaptureInfosBase.ProcessQueueEnum.Low;
                            break;
                        #endregion averageRateObservationDate
                        #region geometricAverageSpecified
                        case CciEnum.geometricAverageSpecified:
                            _fxAverageRateOption.GeometricAverageSpecified = cci.IsFilledValue;
                            break;
                        #endregion geometricAverageSpecified
                        #region averageStrikeOption_settlementType
                        case CciEnum.averageStrikeOption_settlementType:
                            _fxAverageRateOption.AverageStrikeOptionSpecified = cci.IsFilledValue;
                            if (_fxAverageRateOption.AverageStrikeOptionSpecified)
                            {
                                SettlementTypeEnum stTypeEnum = (SettlementTypeEnum)System.Enum.Parse(typeof(SettlementTypeEnum), data, true);
                                _fxAverageRateOption.AverageStrikeOption.SettlementType = stTypeEnum;
                            }
                            break;
                        #endregion averageStrikeOption_settlementType
                        #region default
                        default:
                            isSetting = false;
                            break;
                        #endregion
                    }
                    if (isSetting)
                        CcisBase.Finalize(cci.ClientId_WithoutPrefix, processQueue);
                }
            }
            //
            if ((_fxAverageRateOption.RateObservationScheduleSpecified) && (null == _fxAverageRateOption.RateObservationSchedule))
                _fxAverageRateOption.RateObservationSchedule = _fxAverageRateOption.CreateFxAverageRateObservationSchedule;

            if ((_fxAverageRateOption.RateObservationDateSpecified) && (null == _fxAverageRateOption.RateObservationDate))
                _fxAverageRateOption.RateObservationDate = _fxAverageRateOption.CreateFxAverageRateObservationDates;

            //
            base.Dump_ToDocument();

        }
        #endregion Dump_ToDocument
        #region public override ProcessInitialize
        public override void ProcessInitialize(CustomCaptureInfo pCci)
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
                    #region assetFxRate
                    case CciEnum.assetFxRate:
                        if (null != pCci.Sql_Table)
                        {
                            SQL_AssetFxRate sql_asset = (SQL_AssetFxRate)pCci.Sql_Table;

                            if ((_fxAverageRateOption.CallCurrencyAmount.Currency == sql_asset.QCP_Cur1) &&
                                (_fxAverageRateOption.PutCurrencyAmount.Currency == sql_asset.QCP_Cur2))
                            {
                                if (sql_asset.QCP_QuoteBasisEnum == QuoteBasisEnum.Currency1PerCurrency2)
                                    CcisBase.SetNewValue(CciClientId(CciEnum.averageRateQuoteBasis.ToString()), StrikeQuoteBasisEnum.CallCurrencyPerPutCurrency.ToString());
                                else
                                    CcisBase.SetNewValue(CciClientId(CciEnum.averageRateQuoteBasis.ToString()), StrikeQuoteBasisEnum.PutCurrencyPerCallCurrency.ToString());
                            }
                            else if ((_fxAverageRateOption.PutCurrencyAmount.Currency == sql_asset.QCP_Cur1) &&
                                     (_fxAverageRateOption.CallCurrencyAmount.Currency == sql_asset.QCP_Cur2))
                            {
                                if (sql_asset.QCP_QuoteBasisEnum == QuoteBasisEnum.Currency1PerCurrency2)
                                    CcisBase.SetNewValue(CciClientId(CciEnum.averageRateQuoteBasis.ToString()), StrikeQuoteBasisEnum.PutCurrencyPerCallCurrency.ToString());
                                else
                                    CcisBase.SetNewValue(CciClientId(CciEnum.averageRateQuoteBasis.ToString()), StrikeQuoteBasisEnum.CallCurrencyPerPutCurrency.ToString());
                            }
                            //
                            CcisBase.SetNewValue(CciClientId(CciEnum.primaryRateSource_rateSource.ToString()), sql_asset.PrimaryRateSrc);
                            CcisBase.SetNewValue(CciClientId(CciEnum.primaryRateSource_rateSourcePage.ToString()), sql_asset.PrimaryRateSrcPage);
                            CcisBase.SetNewValue(CciClientId(CciEnum.primaryRateSource_rateSourcePageHeading.ToString()), sql_asset.PrimaryRateSrcHead);
                            //
                            CcisBase.SetNewValue(CciClientId(CciEnum.secondaryRateSource_rateSource.ToString()), sql_asset.SecondaryRateSrc);
                            CcisBase.SetNewValue(CciClientId(CciEnum.secondaryRateSource_rateSourcePage.ToString()), sql_asset.SecondaryRateSrcPage);
                            CcisBase.SetNewValue(CciClientId(CciEnum.secondaryRateSource_rateSourcePageHeading.ToString()), sql_asset.SecondaryRateSrcHead);
                            //
                            CcisBase.SetNewValue(CciClientId(CciEnum.fixingTime_hourMinuteTime.ToString()), DtFunc.DateTimeToString(sql_asset.TimeRateSrc, DtFunc.FmtISOTime));
                            CcisBase.SetNewValue(CciClientId(CciEnum.fixingTime_businessCenter.ToString()), sql_asset.IdBC_RateSrc);
                        }
                        else
                        {
                            CcisBase.SetNewValue(CciClientId(CciEnum.averageRateQuoteBasis.ToString()), String.Empty);
                            CcisBase.SetNewValue(CciClientId(CciEnum.primaryRateSource_rateSource.ToString()), String.Empty);
                            CcisBase.SetNewValue(CciClientId(CciEnum.primaryRateSource_rateSourcePage.ToString()), String.Empty);
                            CcisBase.SetNewValue(CciClientId(CciEnum.primaryRateSource_rateSourcePageHeading.ToString()), String.Empty);
                            //
                            CcisBase.SetNewValue(CciClientId(CciEnum.secondaryRateSource_rateSource.ToString()), String.Empty);
                            CcisBase.SetNewValue(CciClientId(CciEnum.secondaryRateSource_rateSourcePage.ToString()), String.Empty);
                            CcisBase.SetNewValue(CciClientId(CciEnum.secondaryRateSource_rateSourcePageHeading.ToString()), String.Empty);
                            //
                            CcisBase.SetNewValue(CciClientId(CciEnum.fixingTime_hourMinuteTime.ToString()), String.Empty);
                            CcisBase.SetNewValue(CciClientId(CciEnum.fixingTime_businessCenter.ToString()), String.Empty);

                        }
                        break;
                    #endregion assetFxRate
                    #region averageRateObservationDateSpecified
                    case CciEnum.averageRateObservationDateSpecified:
                        if (Cci(CciEnum.averageRateObservationDateSpecified.ToString()).IsFilledValue)
                            CcisBase.SetNewValue(CciClientId(CciEnum.averageRateObservationScheduleSpecified.ToString()), "false");
                        else
                            CcisBase.SetNewValue(CciClientId(CciEnum.averageRateObservationScheduleSpecified.ToString()), "true");
                        break;
                    #endregion averageRateObservationDateSpecified
                    #region averageRateObservationScheduleSpecified
                    case CciEnum.averageRateObservationScheduleSpecified:
                        if (Cci(CciEnum.averageRateObservationScheduleSpecified.ToString()).IsFilledValue)
                        {
                            CcisBase.SetNewValue(CciClientId(CciEnum.averageRateObservationDateSpecified.ToString()), "false");
                            EnabledAverageRateObservationSchedule(true);
                        }
                        else
                        {
                            CcisBase.SetNewValue(CciClientId(CciEnum.averageRateObservationDateSpecified.ToString()), "true");
                            ClearAverageRateObservationSchedule();
                            EnabledAverageRateObservationSchedule(false);
                        }
                        break;
                    #endregion averageRateObservationScheduleSpecified
                    #region averageRateObservationSchedule_observationStartDate
                    case CciEnum.averageRateObservationSchedule_observationStartDate:
                        if (pCci.IsFilledValue)
                            CcisBase.SetNewValue(CciClientId(CciEnum.averageRateObservationScheduleSpecified.ToString()), "true");
                        else
                            CcisBase.SetNewValue(CciClientId(CciEnum.averageRateObservationScheduleSpecified.ToString()), "false");
                        break;
                    #endregion averageRateObservationSchedule_observationStartDate
                    #region Default
                    default:

                        break;
                    #endregion Default
                }
            }
            //
            base.ProcessInitialize(pCci);
            //

        }
        #endregion ProcessInitialize
        #region public override RefreshCciEnabled
        public override void RefreshCciEnabled()
        {
            EnabledAverageRateObservationSchedule(_fxAverageRateOption.RateObservationScheduleSpecified);
            base.RefreshCciEnabled();
        }
        #endregion
        #endregion Membres de IContainerCciFactory
        
        #region Membres de IContainerCciQuoteBasis
        #region public override IsClientId_QuoteBasis
        public override bool IsClientId_QuoteBasis(CustomCaptureInfo pCci)
        {
            bool isOk = false;
            if (!isOk)
                isOk = IsCci(CciEnum.averageRateQuoteBasis, pCci);
            if (!isOk)
                isOk = base.IsClientId_QuoteBasis(pCci);
            return isOk;
        }
        #endregion
        #region public override GetCurrency1
        public override string GetCurrency1(CustomCaptureInfo pCci)
        {
            string ret = string.Empty;
            if (StrFunc.IsEmpty(ret))
            {
                if (IsCci(CciEnum.averageRateQuoteBasis, pCci))
                    ret = Cci(CciEnum.callCurrencyAmount_currency.ToString()).NewValue;
            }
            if (StrFunc.IsEmpty(ret))
                ret = base.GetCurrency1(pCci);
            return ret;
        }
        #endregion
        #region public override GetCurrency1
        public override string GetCurrency2(CustomCaptureInfo pCci)
        {
            string ret = string.Empty;
            if (StrFunc.IsEmpty(ret))
            {
                if (IsCci(CciEnum.averageRateQuoteBasis, pCci))
                    ret = Cci(CciEnum.putCurrencyAmount_currency.ToString()).NewValue;
            }
            if (StrFunc.IsEmpty(ret))
                ret = base.GetCurrency2(pCci);
            return ret;
        }
        #endregion
        #endregion Membres de IContainerCciQuoteBasis
        
        #region Membres de IContainerCciGetInfoButton
        #region SetButtonZoom
        public override bool SetButtonZoom(CustomCaptureInfo pCci, CustomObjectButtonFpmlObject pCo, ref bool pIsSpecified, ref bool pIsEnabled)
        {
            bool isOk = false;
            #region buttons settlementInformation triggerPayout
            if (!isOk)
            {
                isOk = this.IsCci(CciEnum.averageRateObservationDate, pCci);
                if (isOk)
                {
                    pCo.Element = "rateObservationDate";
                    pCo.Object = "product";
                    pIsSpecified = _fxAverageRateOption.RateObservationDateSpecified;
                    pIsEnabled = _fxAverageRateOption.RateObservationDateSpecified;
                }
            }
            #endregion buttons bermuda dates
            // Autres		
            if (!isOk)
                isOk = base.SetButtonZoom(pCci, pCo, ref pIsSpecified, ref pIsEnabled);
            return isOk;
        }
        #endregion
        #endregion Membres de IContainerCciGetInfoButton
        

        public override void DumpSpecific_ToGUI(CciPageBase pPage)
        {
            CustomCaptureInfo cci = Cci(CciEnum.assetFxRate);
            if (null != cci)
                pPage.SetOpenFormReferential(cci, Cst.OTCml_TBL.ASSET_FXRATE);

            base.DumpSpecific_ToGUI(pPage);
        }

        #region private Method
        #region ClearAverageRateObservationSchedule
        private void ClearAverageRateObservationSchedule()
        {
            CcisBase.SetNewValue(CciClientId(CciEnum.averageRateObservationSchedule_calculationPeriodFrequency_period.ToString()), string.Empty);
            CcisBase.SetNewValue(CciClientId(CciEnum.averageRateObservationSchedule_calculationPeriodFrequency_periodMultiplier.ToString()), string.Empty);
            CcisBase.SetNewValue(CciClientId(CciEnum.averageRateObservationSchedule_calculationPeriodFrequency_rollConvention.ToString()), string.Empty);
            CcisBase.SetNewValue(CciClientId(CciEnum.averageRateObservationSchedule_observationStartDate.ToString()), string.Empty);
            CcisBase.SetNewValue(CciClientId(CciEnum.averageRateObservationSchedule_observationEndDate.ToString()), string.Empty);
        }
        #endregion
        #region EnabledAverageRateObservationSchedule
        private void EnabledAverageRateObservationSchedule(bool isEnabled)
        {
            CcisBase.Set(CciClientId(CciEnum.averageRateObservationSchedule_calculationPeriodFrequency_period.ToString()), "IsEnabled", isEnabled);
            CcisBase.Set(CciClientId(CciEnum.averageRateObservationSchedule_calculationPeriodFrequency_periodMultiplier.ToString()), "IsEnabled", isEnabled);
            CcisBase.Set(CciClientId(CciEnum.averageRateObservationSchedule_calculationPeriodFrequency_rollConvention.ToString()), "IsEnabled", isEnabled);
            CcisBase.Set(CciClientId(CciEnum.averageRateObservationSchedule_observationStartDate.ToString()), "IsEnabled", isEnabled);
            CcisBase.Set(CciClientId(CciEnum.averageRateObservationSchedule_observationEndDate.ToString()), "IsEnabled", isEnabled);
        }
        #endregion EnabledAverageRateObservationSchedule
        #endregion
    }

}
