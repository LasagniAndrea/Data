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
    /// Description résumée de TradeFxBarrier.
    /// </summary>
    internal class CciFxBarrier : IContainerCciFactory, IContainerCci, IContainerCciSpecified, IContainerCciQuoteBasis, ICciPresentation 
    {
        #region Members
        private readonly string prefix;
        private readonly CciTradeBase cciTrade;
        private IFxBarrier fxBar;
        private readonly int number;
        private readonly TradeCustomCaptureInfos _ccis;
        private string _clientIdCu1;
        private string _clientIdCu2;
        #endregion

        #region Enum
        public enum CciEnum
        {
            #region fxBarrierType
            [System.Xml.Serialization.XmlEnumAttribute("fxBarrierType")]
            fxBarrierType,
            #endregion fxBarrierType
            #region triggerRate
            [System.Xml.Serialization.XmlEnumAttribute("triggerRate")]
            triggerRate,
            #endregion triggerrate
            #region observationStartDate
            [System.Xml.Serialization.XmlEnumAttribute("observationStartDate")]
            observationStartDate,
            #endregion observationStartDate
            #region observationEndDate
            [System.Xml.Serialization.XmlEnumAttribute("observationEndDate")]
            observationEndDate,
            #endregion observationEndDate
            #region assetFxRate
            assetFxRate,
            #endregion
            #region quotedCurrencyPair
            [System.Xml.Serialization.XmlEnumAttribute("quotedCurrencyPair.quoteBasis")]
            quotedCurrencyPair_quoteBasis,
            [System.Xml.Serialization.XmlEnumAttribute("quotedCurrencyPair.currency1")]
            quotedCurrencyPair_currencyA,
            [System.Xml.Serialization.XmlEnumAttribute("quotedCurrencyPair.currency2")]
            quotedCurrencyPair_currencyB,
            #endregion quotedCurrencyPair
            #region informationSource_rateSource
            [System.Xml.Serialization.XmlEnumAttribute("informationSource.rateSource")]
            informationSource_rateSource,
            [System.Xml.Serialization.XmlEnumAttribute("informationSource.rateSourcePage")]
            informationSource_rateSourcePage,
            [System.Xml.Serialization.XmlEnumAttribute("informationSource.rateSourcePageHeading")]
            informationSource_rateSourcePageHeading,
            #endregion informationSource_rateSource
            unknown,
        }
        #endregion Enum

        #region Property
        public TradeCustomCaptureInfos Ccis
        {
            get { return _ccis; }
        }
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
        private bool ExistNumber { get { return (0 < number); } }
        public bool IsSpecified { get { return Cci(CciEnum.fxBarrierType).IsFilled; } }
        public bool ExistCciAssetFxRate
        {
            //Existe-t-il un zone Asset Fx Rate
            get { return Ccis.Contains(CciClientId(CciEnum.assetFxRate)); }
        }
        public IFxBarrier FxBarrier
        {
            set { fxBar = (IFxBarrier)value; }
            get { return fxBar; }
        }
        #endregion

        #region Constructor
        public CciFxBarrier(CciTradeBase pCciTrade, int pBarrierNumber, IFxBarrier pfxBarrier, string pPrefixBarrier)
        {
            cciTrade = pCciTrade;
            _ccis = pCciTrade.Ccis;
            fxBar = pfxBarrier;
            number = pBarrierNumber;
            prefix = pPrefixBarrier + this.Number + CustomObject.KEY_SEPARATOR;
        }
        #endregion Constructor

        #region Membres de IContainerCciFactory
        #region Initialize_FromCci
        public void Initialize_FromCci()
        {
            CciTools.CreateInstance(this, fxBar);
            if ((Ccis.Contains(CciClientId(CciEnum.informationSource_rateSource)) || this.ExistCciAssetFxRate) &&
                (null == fxBar.InformationSource))
                fxBar.InformationSource = fxBar.CreateInformationSources;
        }

        #endregion Initialize_FromCci
        #region AddCciSystem
        /// <summary>
        /// 
        /// </summary>
        /// FI 20170116 [21916] Modify (use AddCciSystem Method)
        public void AddCciSystem()
        {
            bool isMandatoty = Cci(CciEnum.fxBarrierType).IsMandatory;
            CciTools.AddCciSystem(Ccis, Cst.DDL + CciClientId(CciEnum.quotedCurrencyPair_currencyA.ToString()), isMandatoty, TypeData.TypeDataEnum.@string);
            CciTools.AddCciSystem(Ccis, Cst.DDL + CciClientId(CciEnum.quotedCurrencyPair_currencyB.ToString()), isMandatoty, TypeData.TypeDataEnum.@string);
        }
        #endregion AddCciSystem
        #region Initialize_FromDocument
        /// <summary>
        /// Initialisation des CCI à partir des données "PRODUCT" présentes dans les classes du Document XML
        /// </summary>
        public void Initialize_FromDocument()
        {
            try
            {
                string data;
                bool isSetting;
                SQL_Table sql_Table;
                string clientId_Key;
                foreach (CustomCaptureInfo cci in Ccis)
                {
                    if (IsCciOfContainer(cci.ClientId_WithoutPrefix))
                    {
                        #region Reset variables
                        clientId_Key = CciContainerKey(cci.ClientId_WithoutPrefix);
                        data = string.Empty;
                        isSetting = true;
                        sql_Table = null;
                        #endregion
                        //
                        CciEnum key = CciEnum.unknown;
                        if (System.Enum.IsDefined(typeof(CciEnum), clientId_Key))
                            key = (CciEnum)System.Enum.Parse(typeof(CciEnum), clientId_Key);
                        //
                        switch (key)
                        {
                            #region fxBarrierType
                            case CciEnum.fxBarrierType:
                                if (fxBar.FxBarrierTypeSpecified)
                                    data = fxBar.FxBarrierType.ToString();
                                break;
                            #endregion fxBarrierType
                            #region triggerRate
                            case CciEnum.triggerRate:
                                data = fxBar.TriggerRate.Value;
                                break;
                            #endregion triggerRate
                            #region observationStartDate
                            case CciEnum.observationStartDate:
                                if (fxBar.ObservationStartDateSpecified)
                                    data = fxBar.ObservationStartDate.Value;
                                break;
                            #endregion observationStartDate
                            #region observationEndDate
                            case CciEnum.observationEndDate:
                                if (fxBar.ObservationEndDateSpecified)
                                    data = fxBar.ObservationEndDate.Value;
                                break;
                            #endregion observationEndDate
                            #region Asset_FxRate
                            case CciEnum.assetFxRate:
                                int idAsset = fxBar.InformationSource[0].OTCmlId;
                                if (idAsset > 0)
                                {
                                    SQL_AssetFxRate sql_AssetFxRate = new SQL_AssetFxRate(cciTrade.CSCacheOn, idAsset);
                                    if (sql_AssetFxRate.IsLoaded)
                                    {
                                        cci.Sql_Table = sql_AssetFxRate;
                                        data = sql_AssetFxRate.Identifier;
                                    }
                                }
                                break;
                            #endregion Asset_FxRate
                            #region quotedCurrencyPair
                            case CciEnum.quotedCurrencyPair_quoteBasis:
                                if (fxBar.FxBarrierTypeSpecified)
                                    data = fxBar.QuotedCurrencyPair.QuoteBasis.ToString();
                                break;
                            case CciEnum.quotedCurrencyPair_currencyA:
                                data = fxBar.QuotedCurrencyPair.Currency1;
                                break;
                            case CciEnum.quotedCurrencyPair_currencyB:
                                data = fxBar.QuotedCurrencyPair.Currency2;
                                break;
                            #endregion quotedCurrencyPair
                            #region informationSource_rateSource
                            case CciEnum.informationSource_rateSource:
                                data = fxBar.InformationSource[0].RateSource.Value;
                                break;
                            case CciEnum.informationSource_rateSourcePage:
                                if (fxBar.InformationSource[0].RateSourcePageSpecified)
                                    data = fxBar.InformationSource[0].RateSourcePage.Value;
                                break;
                            case CciEnum.informationSource_rateSourcePageHeading:
                                if (fxBar.InformationSource[0].RateSourcePageHeadingSpecified)
                                    data = fxBar.InformationSource[0].RateSourcePageHeading;
                                break;
                            #endregion informationSource_rateSource
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
            catch (Exception ex) { throw new SpheresException2(MethodInfo.GetCurrentMethod().Name, ex); }
        }
        #endregion Initialize_FromDocument
        #region Dump_ToDocument
        public void Dump_ToDocument()
        {
            try
            {
                bool isSetting;
                string data;
                string clientId, clientId_WithoutPrefix;
                string clientId_Element;
                CustomCaptureInfosBase.ProcessQueueEnum processQueue;
                //
                foreach (CustomCaptureInfo cci in Ccis)
                {
                    //On ne traite que les contrôle dont le contenu à changé
                    if (cci.HasChanged && IsCciOfContainer(cci.ClientId_WithoutPrefix))
                    {
                        clientId_WithoutPrefix = cci.ClientId_WithoutPrefix;
                        processQueue = CustomCaptureInfosBase.ProcessQueueEnum.None;
                        clientId = cci.ClientId;
                        data = cci.NewValue;
                        isSetting = true;
                        clientId_Element = CciContainerKey(cci.ClientId_WithoutPrefix);
                        //
                        CciEnum elt = CciEnum.unknown;
                        if (System.Enum.IsDefined(typeof(CciEnum), clientId_Element))
                            elt = (CciEnum)System.Enum.Parse(typeof(CciEnum), clientId_Element);
                        //
                        switch (elt)
                        {
                            #region fxBarrierType
                            case CciEnum.fxBarrierType:
                                fxBar.FxBarrierTypeSpecified = cci.IsFilledValue;
                                if (fxBar.FxBarrierTypeSpecified)
                                {
                                    FxBarrierTypeEnum FxBarEnum = (FxBarrierTypeEnum)System.Enum.Parse(typeof(FxBarrierTypeEnum), data, true);
                                    fxBar.FxBarrierType = FxBarEnum;
                                }
                                processQueue = CustomCaptureInfosBase.ProcessQueueEnum.Low;//Afin de recalculer les BCs
                                break;
                            #endregion
                            #region triggerRate
                            case CciEnum.triggerRate:
                                fxBar.TriggerRate.Value = data;
                                break;
                            #endregion spotRate
                            #region observationStartDate
                            case CciEnum.observationStartDate:
                                fxBar.ObservationStartDateSpecified = cci.IsFilledValue;
                                fxBar.ObservationStartDate.Value = data;
                                break;
                            #endregion
                            #region observationEndDate
                            case CciEnum.observationEndDate:
                                fxBar.ObservationEndDateSpecified = cci.IsFilledValue;
                                fxBar.ObservationEndDate.Value = data;
                                break;
                            #endregion
                            #region assetFxRate
                            case CciEnum.assetFxRate:
                                SQL_AssetFxRate sql_asset = null;
                                bool isLoaded = false;
                                cci.ErrorMsg = string.Empty;
                                cci.Sql_Table = null ; 
                                if (StrFunc.IsFilled(data))
                                {
                                    for (int i = 0; i < 2; i++)
                                    {
                                        string dataToFind = data;
                                        if (i == 1)
                                            dataToFind = data.Replace(" ", "%") + "%";
                                        sql_asset = new SQL_AssetFxRate(cciTrade.CSCacheOn, SQL_TableWithID.IDType.Identifier, dataToFind, SQL_Table.ScanDataDtEnabledEnum.Yes);
                                        isLoaded = sql_asset.IsLoaded && (sql_asset.RowsCount == 1);
                                        if (isLoaded)
                                            break;
                                    }
                                    //
                                    if (isLoaded)
                                    {
                                        cci.NewValue = sql_asset.Identifier;
                                        cci.Sql_Table = sql_asset;
                                        Ccis.DumpTriggerOrBarrierFromAssetFxRate(fxBar, sql_asset);
                                    }
                                    cci.ErrorMsg = ((false == isLoaded) ? Ressource.GetString("Msg_AssetNotFound") : string.Empty);
                                }
                                processQueue = CustomCaptureInfosBase.ProcessQueueEnum.High;
                                break;
                            #endregion assetFxRate


                            #region quotedCurrencyPair
                            case CciEnum.quotedCurrencyPair_quoteBasis:
                                if (cci.IsFilledValue)
                                {
                                    QuoteBasisEnum QbEnum = (QuoteBasisEnum)System.Enum.Parse(typeof(QuoteBasisEnum), data, true);
                                    fxBar.QuotedCurrencyPair.QuoteBasis = QbEnum;
                                }
                                break;
                            case CciEnum.quotedCurrencyPair_currencyA:
                                fxBar.QuotedCurrencyPair.Currency1 = data;
                                break;
                            case CciEnum.quotedCurrencyPair_currencyB:
                                fxBar.QuotedCurrencyPair.Currency2 = data;
                                break;
                            #endregion quotedCurrencyPair
                            #region informationSource_rateSource
                            case CciEnum.informationSource_rateSource:
                                fxBar.InformationSource[0].RateSource.Value = data;
                                break;
                            case CciEnum.informationSource_rateSourcePage:
                                fxBar.InformationSource[0].RateSourcePageSpecified = cci.IsFilledValue;
                                fxBar.InformationSource[0].RateSourcePage.Value = data;
                                break;
                            case CciEnum.informationSource_rateSourcePageHeading:
                                fxBar.InformationSource[0].RateSourcePageHeadingSpecified = cci.IsFilledValue;
                                fxBar.InformationSource[0].RateSourcePageHeading = data;
                                break;
                            #endregion informationSource_rateSource

                            #region default
                            default:
                                isSetting = false;
                                break;
                            #endregion
                        }
                        if (isSetting)
                            Ccis.Finalize(clientId_WithoutPrefix, processQueue);
                    }
                }
            }
            catch (Exception ex) { throw new SpheresException2(MethodInfo.GetCurrentMethod().Name, ex); }
        }
        #endregion
        #region ProcessInitialize
        /// <summary>
        /// Initialization others data following modification
        /// </summary>
        /// <param name="pCci"></param>
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
                        #region fxBarrierType
                        case CciEnum.fxBarrierType:
                            if (pCci.IsEmptyValue)
                                Clear();
                            else if (Ccis.Contains(CciClientId(CciEnum.assetFxRate.ToString())) && Cci(CciEnum.assetFxRate.ToString()).IsEmptyValue)
                                Ccis.InitializeCciAssetFxRate(CciClientId(CciEnum.assetFxRate), _clientIdCu1, _clientIdCu2);
                            //
                            SetEnabled(pCci.IsFilledValue);
                            //
                            break;
                        #endregion fxBarrierType
                        case CciEnum.assetFxRate:
                            if (null != pCci.Sql_Table)
                            {
                                SQL_AssetFxRate sql_AssetFxRate = (SQL_AssetFxRate)pCci.Sql_Table;

                                Ccis.SetNewValue(CciClientId(CciEnum.quotedCurrencyPair_currencyA), sql_AssetFxRate.QCP_Cur1);
                                Ccis.SetNewValue(CciClientId(CciEnum.quotedCurrencyPair_currencyB), sql_AssetFxRate.QCP_Cur2);
                                Ccis.SetNewValue(CciClientId(CciEnum.quotedCurrencyPair_quoteBasis), sql_AssetFxRate.QCP_QuoteBasis);

                                Ccis.SetNewValue(CciClientId(CciEnum.informationSource_rateSource), sql_AssetFxRate.PrimaryRateSrc);
                                Ccis.SetNewValue(CciClientId(CciEnum.informationSource_rateSourcePage), sql_AssetFxRate.PrimaryRateSrcPage);
                                Ccis.SetNewValue(CciClientId(CciEnum.informationSource_rateSourcePageHeading), sql_AssetFxRate.PrimaryRateSrcHead);
                            }
                            else
                            {
                                Ccis.SetNewValue(CciClientId(CciEnum.quotedCurrencyPair_currencyA), string.Empty);
                                Ccis.SetNewValue(CciClientId(CciEnum.quotedCurrencyPair_currencyB), string.Empty);
                                Ccis.SetNewValue(CciClientId(CciEnum.quotedCurrencyPair_quoteBasis), string.Empty);

                                Ccis.SetNewValue(CciClientId(CciEnum.informationSource_rateSource), string.Empty);
                                Ccis.SetNewValue(CciClientId(CciEnum.informationSource_rateSourcePage), string.Empty);
                                Ccis.SetNewValue(CciClientId(CciEnum.informationSource_rateSourcePageHeading), string.Empty);
                            }
                            break;
                        #region default
                        default:
                            break;
                        #endregion default
                    }
                }
            }
            catch (Exception ex) { throw new SpheresException2(MethodInfo.GetCurrentMethod().Name, ex); }
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
        #region IsClientId_XXX
        public bool IsClientId_PayerOrReceiver(CustomCaptureInfo pCci)
        {
            return false;
        }
        #endregion IsClientId_XXX
        #region CleanUp
        public void CleanUp()
        {
            // TODO : ajoutez l'implémentation de TradeFxOptionBarrier.CleanUp
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
        #endregion

        #region Membres de IContainerCci
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
        public CustomCaptureInfo Cci(CciEnum pEnumValue)
        {
            return Ccis[CciClientId(pEnumValue.ToString())];
        }

        public CustomCaptureInfo Cci(string pClientId_Key)
        {
            return Ccis[CciClientId(pClientId_Key)];
        }

        #endregion
        #region IsCciOfContainer
        public bool IsCciOfContainer(string pClientId_WithoutPrefix)
        {
            return pClientId_WithoutPrefix.StartsWith(prefix);
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
        #endregion IContainerCci

        #region Membres de IContainerCciQuoteBasis
        public bool IsClientId_QuoteBasis(CustomCaptureInfo pCci)
        {
            return IsCci(CciEnum.quotedCurrencyPair_quoteBasis, pCci);
        }

        public string GetCurrency1(CustomCaptureInfo pCci)
        {
            string ret = string.Empty;
            //
            if (IsCci(CciEnum.quotedCurrencyPair_quoteBasis, pCci))
                ret = Cci(CciEnum.quotedCurrencyPair_currencyA).NewValue;
            //
            return ret;
        }

        public string GetCurrency2(CustomCaptureInfo pCci)
        {
            string ret = string.Empty;
            //
            if (IsCci(CciEnum.quotedCurrencyPair_quoteBasis, pCci))
                ret = Cci(CciEnum.quotedCurrencyPair_currencyB).NewValue;
            //
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

        #region public SetDefaultClientIdCurrency
        public void SetDefaultClientIdCurrency(string pClientIdCu1, string pClientIdCu2)
        {
            _clientIdCu1 = pClientIdCu1;
            _clientIdCu2 = pClientIdCu2;
        }
        #endregion
        #region public Clear
        public void Clear()
        {
            CciTools.SetCciContainer(this, "CciEnum", "NewValue", string.Empty);
        }
        #endregion
        #region public SetEnabled
        public void SetEnabled(Boolean pIsEnabled)
        {
            CciTools.SetCciContainer(this, "CciEnum", "IsEnabled", pIsEnabled);
            Cci(CciEnum.fxBarrierType).IsEnabled = true;
        }
        #endregion





        #region ICciPresentation Membres

        public void DumpSpecific_ToGUI(CciPageBase pPage)
        {
            CustomCaptureInfo cci = Cci(CciEnum.assetFxRate);
            if (null != cci)
                pPage.SetOpenFormReferential(cci, Cst.OTCml_TBL.ASSET_FXRATE);

        }

        #endregion
    }
}
