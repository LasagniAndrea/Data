#region Using Directives
using EFS.ACommon;
using EFS.Common;
using EFS.Common.Web;
using EFS.GUI.CCI;
using FpML.Enum;
using FpML.Interface;
using System;
using System.Reflection;
#endregion Using Directives

namespace EFS.TradeInformation
{
    /// <summary>
    /// Description résumée de CciFxFixing.
    /// </summary>
    public class CciFxFixing : IContainerCci, IContainerCciFactory, IContainerCciQuoteBasis, IContainerCciSpecified , ICciPresentation
    {
        #region Members
        private readonly string _prefix;
        private IFxFixing _fxFixing;
        private readonly int _number;
        private readonly TradeCustomCaptureInfos _ccis;
        private readonly CciTradeBase _cciTrade;
        #endregion

        #region Enums
        public enum CciEnum
        {
            #region FxRateAsset
            [System.Xml.Serialization.XmlEnumAttribute("primaryRateSource.assetFxRateId")]
            assetFxRate,
            #endregion FxRateAsset
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
            #region quotedCurrencyPair
            [System.Xml.Serialization.XmlEnumAttribute("quotedCurrencyPair.currency1")]
            quotedCurrencyPair_currencyA,
            [System.Xml.Serialization.XmlEnumAttribute("quotedCurrencyPair.currency2")]
            quotedCurrencyPair_currencyB,
            [System.Xml.Serialization.XmlEnumAttribute("quotedCurrencyPair.quoteBasis")]
            quotedCurrencyPair_quoteBasis,
            #endregion quotedCurrencyPair
            #region fixingDate
            [System.Xml.Serialization.XmlEnumAttribute("fixingDate")]
            fixingDate,
            #endregion fixingDate
            unknown
        }
        #endregion Enums

        #region constructor
        public CciFxFixing(CciTradeBase pCciTrade, int pNumber, IFxFixing pfxFixing, string pPrefix)
        {

            _number = pNumber;
            _prefix = pPrefix + this.Number + CustomObject.KEY_SEPARATOR;
            _ccis = pCciTrade.Ccis;
            _cciTrade = pCciTrade;
            _fxFixing = pfxFixing;
        }
        #endregion constructor

        #region private property
        private bool ExistNumber { get { return (0 < _number); } }
        private string Number
        {
            get
            {
                string ret = string.Empty;
                if (ExistNumber)
                    ret = _number.ToString();
                return ret;
            }
        }
        #endregion private property

        #region public property
        public TradeCustomCaptureInfos Ccis
        {
            get { return _ccis; }
        }
        public IFxFixing Fixing
        {
            set { _fxFixing = value; }
            get { return _fxFixing; }
        }
        #region ExistCciAssetFxRate
        /// <summary>
        /// Obtient true s'il existe le cci assetFxRate
        /// </summary>
        public bool ExistCciAssetFxRate
        {
            //
            get { return Ccis.Contains(CciClientId(CciEnum.assetFxRate)); }
        }
        #endregion
        
        #endregion

        #region Membres de IContainerCci
        public string CciClientId(CciEnum pEnumValue)
        {
            return CciClientId(pEnumValue.ToString());
        }

        public string CciClientId(string pClientId_Key)
        {
            return _prefix + pClientId_Key;
        }

        public CustomCaptureInfo Cci(CciEnum pEnum)
        {
            return Cci(pEnum.ToString());
        }

        public CustomCaptureInfo Cci(string pClientId_Key)
        {
            return Ccis[CciClientId(pClientId_Key)];
        }

        public bool IsCciOfContainer(string pClientId_WithoutPrefix)
        {
            return pClientId_WithoutPrefix.StartsWith(_prefix);
        }
        public string CciContainerKey(string pClientId_WithoutPrefix)
        {
            return pClientId_WithoutPrefix.Substring(_prefix.Length);
        }
        #region IsCci
        public bool IsCci(CciEnum pEnumValue, CustomCaptureInfo pCci)
        {
            return (CciClientId(pEnumValue) == pCci.ClientId_WithoutPrefix);
        }
        public bool IsCci(string pEnumValue, CustomCaptureInfo pCci)
        {
            return (CciClientId(pEnumValue) == pCci.ClientId_WithoutPrefix);
        }
        #endregion
        #endregion

        #region Membres de IContainerCciFactory
        #region Initialize_FromCci
        public void Initialize_FromCci()
        {
            CciTools.CreateInstance(this, _fxFixing);
        }

        #endregion Initialize_FromCci
        #region AddCciSystem
        /// <summary>
        /// 
        /// </summary>
        /// FI 20170116 [21916] Modify (use AddCciSystem Method)
        public void AddCciSystem()
        {
            if (false == ExistCciAssetFxRate)
            {
                // Il n'existe pas forcement currencyA car cette donnée est pré-proposé par une autre zone (un autre CCI) 
                string clientId_WithoutPrefix = CciClientId(CciEnum.quotedCurrencyPair_currencyA.ToString());
                CciTools.AddCciSystem(Ccis, Cst.DDL + clientId_WithoutPrefix, true, TypeData.TypeDataEnum.@string);

                // Il n'existe pas forcement currencyB car cette donnée est pré-proposé par une autre zone (un autre CCI)
                clientId_WithoutPrefix = CciClientId(CciEnum.quotedCurrencyPair_currencyB.ToString());
                CciTools.AddCciSystem(Ccis, Cst.DDL + clientId_WithoutPrefix, true, TypeData.TypeDataEnum.@string);
                //
                // Il n'existe pas forcement QuoteBasis car cette donnée est pré-proposé par une autre zone (un autre CCI)
                clientId_WithoutPrefix = CciClientId(CciEnum.quotedCurrencyPair_quoteBasis.ToString());
                CciTools.AddCciSystem(Ccis, Cst.DDL + clientId_WithoutPrefix, true, TypeData.TypeDataEnum.@string);
            }
        }
        #endregion AddCciSystem
        #region Initialize_FromDocument
        public void Initialize_FromDocument()
        {
            foreach (CustomCaptureInfo cci in Ccis)
            {
                if (IsCciOfContainer(cci.ClientId_WithoutPrefix))
                {
                    #region Reset variables
                    string clientId_Key = CciContainerKey(cci.ClientId_WithoutPrefix);
                    string data = string.Empty;
                    bool isSetting = true;
                    SQL_Table sql_Table = null;
                    #endregion
                    
                    CciEnum key = CciEnum.unknown;
                    if (System.Enum.IsDefined(typeof(CciEnum), clientId_Key))
                        key = (CciEnum)System.Enum.Parse(typeof(CciEnum), clientId_Key);
                    
                    switch (key)
                    {
                        #region Asset_FxRate
                        case CciEnum.assetFxRate:

                            int idAsset = _fxFixing.PrimaryRateSource.OTCmlId;
                            if (idAsset > 0)
                            {
                                SQL_AssetFxRate sql_AssetFxRate = new SQL_AssetFxRate(_cciTrade.CSCacheOn, idAsset);
                                if (sql_AssetFxRate.IsLoaded)
                                {
                                    sql_Table = sql_AssetFxRate;
                                    data = sql_AssetFxRate.Identifier;
                                    _fxFixing.PrimaryRateSource.SetAssetFxRateId(idAsset, data);
                                }
                            }
                            break;
                        #endregion Asset_FxRate
                        #region primaryRateSource
                        case CciEnum.primaryRateSource_rateSource:
                            data = _fxFixing.PrimaryRateSource.RateSource.Value;
                            break;
                        case CciEnum.primaryRateSource_rateSourcePage:
                            if (_fxFixing.PrimaryRateSource.RateSourcePageSpecified)
                                data = _fxFixing.PrimaryRateSource.RateSourcePage.Value;
                            break;
                        case CciEnum.primaryRateSource_rateSourcePageHeading:
                            if (_fxFixing.PrimaryRateSource.RateSourcePageHeadingSpecified)
                                data = _fxFixing.PrimaryRateSource.RateSourcePageHeading;
                            break;
                        #endregion primaryRateSource
                        #region secondaryRateSource
                        case CciEnum.secondaryRateSource_rateSource:
                            if (_fxFixing.SecondaryRateSourceSpecified)
                                data = _fxFixing.SecondaryRateSource.RateSource.Value;
                            break;
                        case CciEnum.secondaryRateSource_rateSourcePage:
                            if ((_fxFixing.SecondaryRateSourceSpecified) && (_fxFixing.SecondaryRateSource.RateSourcePageSpecified))
                                data = _fxFixing.SecondaryRateSource.RateSourcePage.Value;
                            break;
                        case CciEnum.secondaryRateSource_rateSourcePageHeading:
                            if ((_fxFixing.SecondaryRateSourceSpecified) && (_fxFixing.SecondaryRateSource.RateSourcePageHeadingSpecified))
                                data = _fxFixing.SecondaryRateSource.RateSourcePageHeading;
                            break;
                        #endregion secondaryRateSource
                        #region fixingTime
                        case CciEnum.fixingTime_hourMinuteTime:
                            data = _fxFixing.FixingTime.HourMinuteTime.Value;
                            break;
                        case CciEnum.fixingTime_businessCenter:
                            data = _fxFixing.FixingTime.BusinessCenter.Value;
                            break;
                        #endregion fixingTime
                        #region quotedCurrencyPair
                        case CciEnum.quotedCurrencyPair_currencyA:
                            data = _fxFixing.QuotedCurrencyPair.Currency1;
                            break;
                        case CciEnum.quotedCurrencyPair_currencyB:
                            data = _fxFixing.QuotedCurrencyPair.Currency2;
                            break;
                        case CciEnum.quotedCurrencyPair_quoteBasis:
                            if (StrFunc.IsFilled(_fxFixing.FixingDate.Value)) // Car un enum n'est jamais null
                                data = _fxFixing.QuotedCurrencyPair.QuoteBasis.ToString();
                            break;
                        #endregion quotedCurrencyPair
                        #region fixingDate
                        case CciEnum.fixingDate:
                            data = _fxFixing.FixingDate.Value;
                            break;
                        #endregion fixingDate
                        #region default
                        default:
                            isSetting = false;
                            break;
                        #endregion
                    }
                    if (isSetting)
                        Ccis.InitializeCci(cci, sql_Table, data);
                }
            }
        }

        #endregion Initialize_FromDocument
        #region Dump_ToDocument
        public void Dump_ToDocument()
        {
            string data;
            string clientId_Key;
            CustomCaptureInfosBase.ProcessQueueEnum processQueue;

            foreach (CustomCaptureInfo cci in Ccis)
            {
                //On ne traite que les contrôle dont le contenu à changé
                if ((cci.HasChanged) && IsCciOfContainer(cci.ClientId_WithoutPrefix))
                {
                    #region Reset variables
                    processQueue = CustomCaptureInfosBase.ProcessQueueEnum.None;
                    clientId_Key = this.CciContainerKey(cci.ClientId_WithoutPrefix);
                    data = cci.NewValue;
                    bool isSetting = true;
                    #endregion Reset variables
                    //
                    CciEnum key = CciEnum.unknown;
                    if (System.Enum.IsDefined(typeof(CciEnum), clientId_Key))
                        key = (CciEnum)System.Enum.Parse(typeof(CciEnum), clientId_Key);
                    //
                    switch (key)
                    {
                        #region Asset_FxRate
                        case CciEnum.assetFxRate:
                            SQL_AssetFxRate sql_asset = null;
                            bool isLoaded = false;
                            cci.ErrorMsg = string.Empty;
                            if (StrFunc.IsFilled(data))
                            {

                                for (int i = 0; i < 2; i++)
                                {
                                    string dataToFind = data;
                                    if (i == 1)
                                        dataToFind = data.Replace(" ", "%") + "%";
                                    sql_asset = new SQL_AssetFxRate(_cciTrade.CSCacheOn, SQL_TableWithID.IDType.Identifier, dataToFind, SQL_Table.ScanDataDtEnabledEnum.Yes);
                                    isLoaded = sql_asset.IsLoaded && (sql_asset.RowsCount == 1);
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
                                    _fxFixing.FixingTime.HourMinuteTime.TimeValue = sql_asset.TimeRateSrc;
                                    _fxFixing.FixingTime.BusinessCenter.Value = sql_asset.IdBC_RateSrc;
                                    #endregion fixingTime
                                    #region quotedCurrencyPair
                                    _fxFixing.QuotedCurrencyPair.Currency1 = sql_asset.QCP_Cur1;
                                    _fxFixing.QuotedCurrencyPair.Currency2 = sql_asset.QCP_Cur2;
                                    _fxFixing.QuotedCurrencyPair.QuoteBasis = sql_asset.QCP_QuoteBasisEnum;
                                    #endregion quotedCurrencyPair
                                    #region primaryRateSource
                                    _fxFixing.PrimaryRateSource.OTCmlId = sql_asset.Id;
                                    _fxFixing.PrimaryRateSource.RateSource.Value = sql_asset.PrimaryRateSrc;
                                    _fxFixing.PrimaryRateSource.AssetFxRateId.Value = sql_asset.Identifier;
                                    //
                                    _fxFixing.PrimaryRateSource.RateSourcePageSpecified = StrFunc.IsFilled(sql_asset.PrimaryRateSrcPage);
                                    if (_fxFixing.PrimaryRateSource.RateSourcePageSpecified)
                                        _fxFixing.PrimaryRateSource.CreateRateSourcePage(sql_asset.PrimaryRateSrcPage);
                                    //
                                    _fxFixing.PrimaryRateSource.RateSourcePageHeadingSpecified = StrFunc.IsFilled(sql_asset.PrimaryRateSrcHead);
                                    if (_fxFixing.PrimaryRateSource.RateSourcePageHeadingSpecified)
                                        _fxFixing.PrimaryRateSource.RateSourcePageHeading = sql_asset.PrimaryRateSrcHead;
                                    #endregion primaryRateSource
                                    #region secondaryRateSource
                                    _fxFixing.SecondaryRateSourceSpecified = StrFunc.IsFilled(sql_asset.SecondaryRateSrc);
                                    if (_fxFixing.SecondaryRateSourceSpecified)
                                    {
                                        if (null == _fxFixing.SecondaryRateSource)
                                            _fxFixing.SecondaryRateSource = _fxFixing.CreateInformationSource();
                                        _fxFixing.SecondaryRateSource.RateSource.Value = sql_asset.SecondaryRateSrc;
                                        //
                                        _fxFixing.SecondaryRateSource.RateSourcePageSpecified = StrFunc.IsFilled(sql_asset.SecondaryRateSrcPage);
                                        if (_fxFixing.SecondaryRateSource.RateSourcePageSpecified)
                                            _fxFixing.SecondaryRateSource.CreateRateSourcePage(sql_asset.SecondaryRateSrcPage);
                                        //
                                        _fxFixing.SecondaryRateSource.RateSourcePageHeadingSpecified = StrFunc.IsFilled(sql_asset.SecondaryRateSrcHead);
                                        if (_fxFixing.SecondaryRateSource.RateSourcePageHeadingSpecified)
                                            _fxFixing.SecondaryRateSource.RateSourcePageHeading = sql_asset.SecondaryRateSrcHead;
                                    }
                                    #endregion secondaryRateSource
                                }
                                //
                                cci.ErrorMsg = ((false == isLoaded) ? Ressource.GetString("Msg_AssetNotFound") : string.Empty);
                            }
                            processQueue = CustomCaptureInfosBase.ProcessQueueEnum.High;//Afin de préproposer les BCs
                            break;
                        #endregion 	Asset_FxRate
                        #region primaryRateSource
                        case CciEnum.primaryRateSource_rateSource:
                            _fxFixing.PrimaryRateSource.RateSource.Value = data;
                            break;
                        case CciEnum.primaryRateSource_rateSourcePage:
                            _fxFixing.PrimaryRateSource.RateSourcePageSpecified = cci.IsFilledValue;
                            _fxFixing.PrimaryRateSource.RateSourcePage.Value = data;
                            break;
                        case CciEnum.primaryRateSource_rateSourcePageHeading:
                            _fxFixing.PrimaryRateSource.RateSourcePageHeadingSpecified = cci.IsFilledValue;
                            _fxFixing.PrimaryRateSource.RateSourcePageHeading = data;
                            break;
                        #endregion primaryRateSource
                        #region secondaryRateSource
                        case CciEnum.secondaryRateSource_rateSource:
                            _fxFixing.SecondaryRateSourceSpecified = cci.IsFilledValue;
                            _fxFixing.SecondaryRateSource.RateSource.Value = data;
                            break;
                        case CciEnum.secondaryRateSource_rateSourcePage:
                            _fxFixing.SecondaryRateSource.RateSourcePageSpecified = cci.IsFilledValue;
                            _fxFixing.SecondaryRateSource.RateSourcePage.Value = data;
                            break;
                        case CciEnum.secondaryRateSource_rateSourcePageHeading:
                            _fxFixing.SecondaryRateSource.RateSourcePageHeadingSpecified = cci.IsFilledValue;
                            _fxFixing.SecondaryRateSource.RateSourcePageHeading = data;
                            break;
                        #endregion secondaryRateSource
                        #region fixingTime
                        case CciEnum.fixingTime_hourMinuteTime:
                            _fxFixing.FixingTime.HourMinuteTime.Value = data;
                            break;
                        case CciEnum.fixingTime_businessCenter:
                            _fxFixing.FixingTime.BusinessCenter.Value = data;
                            break;
                        #endregion fixingTime
                        #region quotedCurrencyPair
                        case CciEnum.quotedCurrencyPair_currencyA:
                            _fxFixing.QuotedCurrencyPair.Currency1 = data;
                            processQueue = CustomCaptureInfosBase.ProcessQueueEnum.Low;//Afin de recalculer ...
                            break;
                        case CciEnum.quotedCurrencyPair_currencyB:
                            _fxFixing.QuotedCurrencyPair.Currency2 = data;
                            processQueue = CustomCaptureInfosBase.ProcessQueueEnum.Low;//Afin de recalculer ...
                            break;
                        case CciEnum.quotedCurrencyPair_quoteBasis:
                            if (cci.IsFilledValue)
                            {
                                QuoteBasisEnum QbEnum = (QuoteBasisEnum)System.Enum.Parse(typeof(QuoteBasisEnum), data, true);
                                _fxFixing.QuotedCurrencyPair.QuoteBasis = QbEnum;
                            }
                            processQueue = CustomCaptureInfosBase.ProcessQueueEnum.Low;//Afin de recalculer ...
                            break;
                        #endregion quotedCurrencyPair
                        #region fixingDate
                        case CciEnum.fixingDate:
                            _fxFixing.FixingDate.Value = data;
                            break;
                        #endregion fixingDate
                        #region default
                        default:
                            isSetting = false;
                            break;
                        #endregion default
                    }
                    if (isSetting)
                        Ccis.Finalize(cci.ClientId_WithoutPrefix, processQueue);
                }
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
            try
            {
                if (IsCciOfContainer(pCci.ClientId_WithoutPrefix))
                {
                    string clientId_Element = CciContainerKey(pCci.ClientId_WithoutPrefix);
                    //
                    CciEnum key = CciEnum.unknown;
                    if (System.Enum.IsDefined(typeof(CciEnum), clientId_Element))
                        key = (CciEnum)System.Enum.Parse(typeof(CciEnum), clientId_Element);
                    //

                    switch (key)
                    {
                        case CciEnum.assetFxRate:
                            if (pCci.IsEmpty)
                                Clear();
                            else if (null != pCci.Sql_Table)
                            {
                                SQL_AssetFxRate sql_AssetFxRate = (SQL_AssetFxRate)pCci.Sql_Table;

                                Ccis.SetNewValue(CciClientId(CciEnum.quotedCurrencyPair_currencyA), sql_AssetFxRate.QCP_Cur1);
                                Ccis.SetNewValue(CciClientId(CciEnum.quotedCurrencyPair_currencyB), sql_AssetFxRate.QCP_Cur2);
                                Ccis.SetNewValue(CciClientId(CciEnum.quotedCurrencyPair_quoteBasis), sql_AssetFxRate.QCP_QuoteBasis);

                                Ccis.SetNewValue(CciClientId(CciEnum.primaryRateSource_rateSource), sql_AssetFxRate.PrimaryRateSrc);
                                Ccis.SetNewValue(CciClientId(CciEnum.primaryRateSource_rateSourcePage), sql_AssetFxRate.PrimaryRateSrcPage);
                                Ccis.SetNewValue(CciClientId(CciEnum.primaryRateSource_rateSourcePageHeading), sql_AssetFxRate.PrimaryRateSrcHead);

                                Ccis.SetNewValue(CciClientId(CciEnum.secondaryRateSource_rateSource), sql_AssetFxRate.SecondaryRateSrc);
                                Ccis.SetNewValue(CciClientId(CciEnum.secondaryRateSource_rateSourcePage), sql_AssetFxRate.SecondaryRateSrcPage);
                                Ccis.SetNewValue(CciClientId(CciEnum.secondaryRateSource_rateSourcePageHeading), sql_AssetFxRate.SecondaryRateSrcHead);

                                Ccis.SetNewValue(CciClientId(CciEnum.fixingTime_hourMinuteTime), DtFunc.DateTimeToString(sql_AssetFxRate.TimeRateSrc, DtFunc.FmtISOTime));
                                Ccis.SetNewValue(CciClientId(CciEnum.fixingTime_businessCenter), sql_AssetFxRate.IdBC_RateSrc);

                            }
                            else
                            {
                                Ccis.SetNewValue(CciClientId(CciEnum.quotedCurrencyPair_currencyA), string.Empty);
                                Ccis.SetNewValue(CciClientId(CciEnum.quotedCurrencyPair_currencyB), string.Empty);
                                Ccis.SetNewValue(CciClientId(CciEnum.quotedCurrencyPair_quoteBasis), string.Empty);

                                Ccis.SetNewValue(CciClientId(CciEnum.primaryRateSource_rateSource), string.Empty);
                                Ccis.SetNewValue(CciClientId(CciEnum.primaryRateSource_rateSourcePage), string.Empty);
                                Ccis.SetNewValue(CciClientId(CciEnum.primaryRateSource_rateSourcePageHeading), string.Empty);

                                Ccis.SetNewValue(CciClientId(CciEnum.secondaryRateSource_rateSource), string.Empty);
                                Ccis.SetNewValue(CciClientId(CciEnum.secondaryRateSource_rateSourcePage), string.Empty);
                                Ccis.SetNewValue(CciClientId(CciEnum.secondaryRateSource_rateSourcePageHeading), string.Empty);

                                Ccis.SetNewValue(CciClientId(CciEnum.fixingTime_hourMinuteTime), string.Empty);
                                Ccis.SetNewValue(CciClientId(CciEnum.fixingTime_businessCenter), string.Empty);
                            }
                            break;
                        case CciEnum.quotedCurrencyPair_currencyA:
                        case CciEnum.quotedCurrencyPair_currencyB:
                        case CciEnum.quotedCurrencyPair_quoteBasis:
                            if (pCci.IsEmpty)
                                Clear();
                            break;
                        default:

                            break;
                    }
                }
            }
            catch (Exception ex) { throw new SpheresException2(MethodInfo.GetCurrentMethod().Name, ex); }
        }

        #endregion ProcessInitialize
        #region IsClientId_PayerOrReceiver
        public bool IsClientId_PayerOrReceiver(CustomCaptureInfo pCci)
        {
            // TODO : OK
            return false;
        }
        #endregion IsClientId_PayerOrReceiver
        #region CleanUp
        public void CleanUp()
        {
            // TODO : OK
        }
        #endregion
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
        #endregion

        #region Membres de IContainerCciQuoteBasis
        public bool IsClientId_QuoteBasis(CustomCaptureInfo pCci)
        {
            return IsCci(CciEnum.quotedCurrencyPair_quoteBasis, pCci);
        }

        public string GetCurrency1(CustomCaptureInfo pCci)
        {
            string ret = string.Empty;
            if (IsCci(CciEnum.quotedCurrencyPair_quoteBasis, pCci))
                ret = Cci(CciEnum.quotedCurrencyPair_currencyA).NewValue;
            return ret;
        }

        public string GetCurrency2(CustomCaptureInfo pCci)
        {
            string ret = string.Empty;
            if (IsCci(CciEnum.quotedCurrencyPair_quoteBasis, pCci))
                ret = Cci(CciEnum.quotedCurrencyPair_currencyB).NewValue;
            return ret;
        }
        #region public GetBaseCurrency
        public string GetBaseCurrency(CustomCaptureInfo pCci)
        {
            string ret = string.Empty;
            return ret;
        }
        #endregion
        #endregion Membres de IContainerCciQuoteBasis

        #region Membres de IsSpecified
        public bool IsSpecified { get { return Cci(CciEnum.fixingDate).IsFilled; } }
        #endregion

        #region public Clear
        public void Clear()
        {
            CciTools.SetCciContainer(this, "NewValue", string.Empty);
        }
        #endregion
        #region public SetEnabled
        public void SetEnabled(bool pIsEnabled)
        {
            CciTools.SetCciContainer(this, "IsEnabled", pIsEnabled);
        }
        #endregion

        #region ICciPresentation Membres
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pPage"></param>
        public void DumpSpecific_ToGUI(CciPageBase pPage)
        {
            CustomCaptureInfo cci = Cci(CciEnum.assetFxRate);
            if (null != cci)
                pPage.SetOpenFormReferential(cci, Cst.OTCml_TBL.ASSET_FXRATE);
        }
        #endregion


    }
}
