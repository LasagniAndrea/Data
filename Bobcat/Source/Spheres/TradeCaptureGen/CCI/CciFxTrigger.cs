using EFS.ACommon;
using EFS.Common;
using EFS.Common.Web;
using EFS.GUI.CCI;
using EfsML.Business;
using EfsML.Enum;
using FpML.Enum;
using FpML.Interface;
using System;
using System.Reflection;

namespace EFS.TradeInformation
{
    /// <summary>
    /// Description résumée de CciFxTrigger
    /// </summary>
    public class CciFxTrigger : IContainerCci, IContainerCciFactory, IContainerCciSpecified, ICciPresentation 
    {
        #region Members
        private readonly string prefix;
        private object fxtrigger; //FxAmericanTrigger Or FxEuropeanTrigger
        private readonly int _number;
        private readonly TradeCustomCaptureInfos _ccis;
        private readonly CciTradeBase _trade;
        private string _clientIdCu1;
        private string _clientIdCu2;
        #endregion Members

        #region Enums
        public enum CciEnum
        {
            [System.Xml.Serialization.XmlEnumAttribute("touchCondition")]
            touchCondition,
            [System.Xml.Serialization.XmlEnumAttribute("triggerCondition")]
            triggerCondition,
            [System.Xml.Serialization.XmlEnumAttribute("triggerRate")]
            triggerRate,
            [System.Xml.Serialization.XmlEnumAttribute("informationSource.assetFxRateId")]
            assetFxRate,
            [System.Xml.Serialization.XmlEnumAttribute("quotedCurrencyPair.quoteBasis")]
            quotedCurrencyPair_quoteBasis,
            [System.Xml.Serialization.XmlEnumAttribute("quotedCurrencyPair.currency1")]
            quotedCurrencyPair_currencyA,
            [System.Xml.Serialization.XmlEnumAttribute("quotedCurrencyPair.currency2")]
            quotedCurrencyPair_currencyB,
            [System.Xml.Serialization.XmlEnumAttribute("informationSource.rateSource")]
            informationSource_rateSource,
            [System.Xml.Serialization.XmlEnumAttribute("informationSource.rateSourcePage")]
            informationSource_rateSourcePage,
            [System.Xml.Serialization.XmlEnumAttribute("informationSource.rateSourcePageHeading")]
            informationSource_rateSourcePageHeading,
            [System.Xml.Serialization.XmlEnumAttribute("observationStartDate")]
            observationStartDate,
            [System.Xml.Serialization.XmlEnumAttribute("observationEndDate")]
            observationEndDate,
            unknown
        }
        #endregion Enums

        #region property
        private bool ExistNumber { get { return (0 < _number); } }
        private string NumberPrefix
        {
            get
            {
                string ret = string.Empty;
                if (ExistNumber)
                    ret = _number.ToString();
                return ret;
            }
        }

        public object FxTrigger
        {
            set { fxtrigger = value; }
            get { return fxtrigger; }
        }

        public bool IsEuropean
        {
            get { return Tools.IsTypeOrInterfaceOf(fxtrigger, InterfaceEnum.IFxEuropeanTrigger); }
        }
        public bool IsAmerican
        {
            get { return Tools.IsTypeOrInterfaceOf(fxtrigger, InterfaceEnum.IFxAmericanTrigger); }
        }

        public bool ExistCciAssetFxRate
        {
            //Existe-t-il un zone Asset Fx Rate
            get { return Ccis.Contains(CciClientId(CciEnum.assetFxRate)); }
        }

        public TradeCustomCaptureInfos Ccis
        {
            get { return _ccis; }
        }

        #endregion

        #region constructor
        public CciFxTrigger(CciTradeBase pTrade, int pNumber, object pfxTrigger, string pPrefix)
        {
            _number = pNumber;
            prefix = pPrefix + NumberPrefix + CustomObject.KEY_SEPARATOR;
            _ccis = pTrade.Ccis;
            _trade = pTrade;
            fxtrigger = pfxTrigger;
        }
        #endregion constructor

        #region Membres de IContainerCci
        public string CciClientId(CciEnum pEnumValue)
        {
            return CciClientId(pEnumValue.ToString());
        }

        public string CciClientId(string pClientId_Key)
        {
            return prefix + pClientId_Key;
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
            return pClientId_WithoutPrefix.StartsWith(prefix);
        }
        public string CciContainerKey(string pClientId_WithoutPrefix)
        {
            return pClientId_WithoutPrefix.Substring(prefix.Length);
        }
        #endregion

        #region Membres de IContainerCciFactory
        #region Initialize_FromCci
        public void Initialize_FromCci()
        {
            try
            {
                if (Ccis.Contains(CciClientId(CciEnum.informationSource_rateSource)) ||
                    Ccis.Contains(CciClientId(CciEnum.assetFxRate)))
                {
                    FieldInfo fld;
                    IInformationSource[] infos = null;
                    fld = fxtrigger.GetType().GetField("informationSource");
                    infos = (IInformationSource[])fld.GetValue(fxtrigger);
                    if (null == infos)
                    {
                        infos = ((IFxTrigger)fxtrigger).CreateInformationSources;
                        fld.SetValue(fxtrigger, infos);
                    }
                }
                //
                CciTools.CreateInstance(this, fxtrigger);
            }
            catch (Exception ex) { throw new SpheresException2(MethodInfo.GetCurrentMethod().Name, ex); }
        }
        #endregion Initialize_FromCci
        #region AddCciSystem
        public void AddCciSystem()
        {
            // TODO : ajoutez l'implémentation de CciFxTrigger.AddCciSystem
        }
        #endregion AddCciSystem
        #region Initialize_FromDocument
        /// <summary>
        /// Initialisation des CCI à partir des données "PRODUCT" présentes dans les classes du Document XML
        /// </summary>
        public void Initialize_FromDocument()
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
                    FieldInfo fld = null;
                    IInformationSource[] infos = null;
                    #endregion
                    //
                    CciEnum key = CciEnum.unknown;
                    if (System.Enum.IsDefined(typeof(CciEnum), clientId_Key))
                        key = (CciEnum)System.Enum.Parse(typeof(CciEnum), clientId_Key);
                    //
                    switch (key)
                    {
                        #region triggercondition/TouchCondition
                        case CciEnum.triggerCondition:
                            fld = fxtrigger.GetType().GetField("triggerCondition");
                            data = fld.GetValue(fxtrigger).ToString();
                            break;
                        case CciEnum.touchCondition:
                            fld = fxtrigger.GetType().GetField("touchCondition");
                            data = fld.GetValue(fxtrigger).ToString();
                            break;
                        #endregion triggercondition/TouchCondition
                        #region triggerRate
                        case CciEnum.triggerRate:
                            Cci(CciEnum.triggerRate).LastValue = "-99"; //=> Volontaire pour rentrer ds process
                            data = ((IFxTrigger)fxtrigger).TriggerRate.Value;
                            break;
                        #endregion triggerRate
                        #region Observation StartDate/ EndDate
                        case CciEnum.observationStartDate:
                            if (((IFxAmericanTrigger)fxtrigger).ObservationStartDateSpecified)
                                data = ((IFxAmericanTrigger)fxtrigger).ObservationStartDate.Value;
                            break;
                        case CciEnum.observationEndDate:
                            if (((IFxAmericanTrigger)fxtrigger).ObservationEndDateSpecified)
                                data = ((IFxAmericanTrigger)fxtrigger).ObservationEndDate.Value;
                            break;
                        #endregion Observation StartDate/ EndDate
                        #region Asset_FxRate
                        case CciEnum.assetFxRate:
                            try
                            {
                                fld = fxtrigger.GetType().GetField("informationSource");
                                infos = (IInformationSource[])fld.GetValue(fxtrigger);
                                if (null != infos)
                                {
                                    int idAsset = infos[0].OTCmlId;
                                    if (0 < idAsset)
                                    {
                                        SQL_AssetFxRate sql_AssetFxRate = new SQL_AssetFxRate(_trade.CS, idAsset);
                                        if (sql_AssetFxRate.IsLoaded)
                                        {
                                            sql_Table = sql_AssetFxRate;
                                            data = sql_AssetFxRate.Identifier;
                                            infos[0].SetAssetFxRateId(idAsset, data);
                                        }
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
                        #region quotedCurrencyPair
                        case CciEnum.quotedCurrencyPair_quoteBasis:
                            data = ((IFxTrigger)fxtrigger).QuotedCurrencyPair.QuoteBasis.ToString();
                            break;
                        case CciEnum.quotedCurrencyPair_currencyA:
                            data = ((IFxTrigger)fxtrigger).QuotedCurrencyPair.Currency1;
                            break;
                        case CciEnum.quotedCurrencyPair_currencyB:
                            data = ((IFxTrigger)fxtrigger).QuotedCurrencyPair.Currency2;
                            break;
                        #endregion quotedCurrencyPair
                        #region informationSource_rateSource
                        case CciEnum.informationSource_rateSource:
                            infos = ((IFxTrigger)fxtrigger).InformationSource;
                            if (null != infos)
                                data = infos[0].RateSource.Value;
                            break;

                        case CciEnum.informationSource_rateSourcePage:
                            infos = ((IFxTrigger)fxtrigger).InformationSource;
                            if ((null != infos) && infos[0].RateSourcePageSpecified)
                                data = infos[0].RateSourcePage.Value;
                            break;
                        case CciEnum.informationSource_rateSourcePageHeading:
                            infos = ((IFxTrigger)fxtrigger).InformationSource;
                            if ((null != infos) && infos[0].RateSourcePageHeadingSpecified)
                                data = infos[0].RateSourcePageHeading;
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
        #endregion Initialize_FromDocument
        #region Dump_ToDocument
        public void Dump_ToDocument()
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
                    #region Declaration
                    clientId_WithoutPrefix = cci.ClientId_WithoutPrefix;
                    processQueue = CustomCaptureInfosBase.ProcessQueueEnum.None;
                    clientId = cci.ClientId;
                    data = cci.NewValue;
                    isSetting = true;
                    clientId_Element = CciContainerKey(cci.ClientId_WithoutPrefix);
                    FieldInfo fld = null;
                    IInformationSource[] infos = null;
                    #endregion
                    //
                    CciEnum elt = CciEnum.unknown;
                    if (System.Enum.IsDefined(typeof(CciEnum), clientId_Element))
                        elt = (CciEnum)System.Enum.Parse(typeof(CciEnum), clientId_Element);

                    switch (elt)
                    {
                        #region triggerCondition/touchCondition
                        case CciEnum.triggerCondition:
                            fld = fxtrigger.GetType().GetField("triggerCondition");
                            if (StrFunc.IsFilled(data))
                            {
                                TriggerConditionEnum trgEnum = (TriggerConditionEnum)System.Enum.Parse(typeof(TriggerConditionEnum), data, true);
                                fld.SetValue(fxtrigger, trgEnum);
                            }
                            processQueue = CustomCaptureInfosBase.ProcessQueueEnum.Low;//Afin de préproposer les BCs
                            break;
                        case CciEnum.touchCondition:
                            if (StrFunc.IsFilled(data))
                            {
                                fld = fxtrigger.GetType().GetField("touchCondition");
                                TouchConditionEnum tCondEnum = (TouchConditionEnum)System.Enum.Parse(typeof(TouchConditionEnum), data, true);
                                fld.SetValue(fxtrigger, tCondEnum);
                            }
                            processQueue = CustomCaptureInfosBase.ProcessQueueEnum.Low;//Afin de préproposer les BCs
                            break;
                        #endregion triggerCondition/touchCondition
                        #region triggerRate
                        case CciEnum.triggerRate:
                            ((IFxTrigger)fxtrigger).TriggerRate.Value = data;
                            processQueue = CustomCaptureInfosBase.ProcessQueueEnum.Low;//Afin de préproposer les BCs
                            break;
                        #endregion triggerRate
                        #region observation StartDate/ EndDate
                        case CciEnum.observationStartDate:
                            if (IsAmerican)
                            {
                                ((IFxAmericanTrigger)fxtrigger).ObservationStartDateSpecified = cci.IsFilledValue;
                                ((IFxAmericanTrigger)fxtrigger).ObservationStartDate.Value = data;
                            }
                            break;
                        case CciEnum.observationEndDate:
                            if (IsAmerican)
                            {
                                ((IFxAmericanTrigger)fxtrigger).ObservationEndDateSpecified = cci.IsFilledValue;
                                ((IFxAmericanTrigger)fxtrigger).ObservationEndDate.Value = data;
                            }
                            break;
                        #endregion Observation StartDate/ EndDate
                        #region assetFxRate
                        case CciEnum.assetFxRate:
                            SQL_AssetFxRate sql_asset = null;

                            bool isLoaded = false;
                            cci.ErrorMsg = string.Empty;
                            //
                            if (StrFunc.IsFilled(data))
                            {
                                for (int i = 0; i < 2; i++)
                                {
                                    string dataToFind = data;
                                    if (i == 1)
                                        dataToFind = data.Replace(" ", "%") + "%";
                                    sql_asset = new SQL_AssetFxRate(Ccis.CS, SQL_TableWithID.IDType.Identifier, dataToFind, SQL_Table.ScanDataDtEnabledEnum.Yes);
                                    isLoaded = sql_asset.IsLoaded && (sql_asset.RowsCount == 1);
                                    if (isLoaded)
                                        break;
                                }
                                if (isLoaded)
                                {
                                    cci.NewValue = sql_asset.Identifier;
                                    cci.Sql_Table = sql_asset;
                                    Ccis.DumpTriggerOrBarrierFromAssetFxRate(fxtrigger, sql_asset);
                                }
                                cci.ErrorMsg = ((false == isLoaded) ? Ressource.GetString("Msg_AssetNotFound") : string.Empty);
                            }
                            break;
                        #endregion assetFxRate
                        #region quotedCurrencyPair
                        case CciEnum.quotedCurrencyPair_quoteBasis:
                            QuoteBasisEnum QbEnum = (QuoteBasisEnum)System.Enum.Parse(typeof(QuoteBasisEnum), data, true);
                            ((IFxTrigger)fxtrigger).QuotedCurrencyPair.QuoteBasis = QbEnum;
                            break;
                        case CciEnum.quotedCurrencyPair_currencyA:
                            ((IFxTrigger)fxtrigger).QuotedCurrencyPair.Currency1 = data;
                            break;
                        case CciEnum.quotedCurrencyPair_currencyB:
                            ((IFxTrigger)fxtrigger).QuotedCurrencyPair.Currency2 = data;
                            break;
                        #endregion quotedCurrencyPair
                        #region informationSource_rateSource
                        case CciEnum.informationSource_rateSource:
                            infos = ((IFxTrigger)fxtrigger).InformationSource;
                            infos[0].RateSource.Value = data;
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
        #endregion
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
                        #region TriggerRate
                        case CciEnum.triggerRate:
                            if (!Cci(CciEnum.triggerRate).IsFilledValue)
                            {
                                Ccis.SetNewValue(CciClientId(CciEnum.touchCondition), string.Empty);
                                Ccis.SetNewValue(CciClientId(CciEnum.triggerCondition), string.Empty);
                                Ccis.SetNewValue(CciClientId(CciEnum.informationSource_rateSource), string.Empty);
                            }
                            break;
                        #endregion

                        #region triggerCondition/touchCondition
                        case CciEnum.triggerCondition:
                        case CciEnum.touchCondition:
                            if (pCci.IsEmptyValue)
                                Clear();
                            else if (Ccis.Contains(CciClientId(CciEnum.assetFxRate)) && Cci(CciEnum.assetFxRate).IsEmptyValue)
                                Ccis.InitializeCciAssetFxRate(CciClientId(CciEnum.assetFxRate), _clientIdCu1, _clientIdCu2);
                            //
                            SetEnabled(pCci.IsFilledValue);
                            break;
                        #endregion

                        #region AssetFxRate
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
                        #endregion AssetFxRate

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
            return false;
        }
        #endregion
        #region CleanUp
        public void CleanUp()
        {

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

        #region Membres de IContainerCciSpecified
        public bool IsSpecified
        {
            get
            {
                bool ret;
                if (IsAmerican)
                    ret = Cci(CciEnum.touchCondition).IsFilled;
                else
                    ret = Cci(CciEnum.triggerCondition).IsFilled;

                return ret;
            }
        }
        #endregion IContainerCciSpecified

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
            if (IsAmerican)
                Cci(CciEnum.touchCondition).IsEnabled = true;
            else
                Cci(CciEnum.triggerCondition).IsEnabled = true;
        }
        #endregion

        #region public SetDefaultClientIdCurrency
        public void SetDefaultClientIdCurrency(string pClientIdCu1, string pClientIdCu2)
        {
            _clientIdCu1 = pClientIdCu1;
            _clientIdCu2 = pClientIdCu2;
        }
        #endregion


    }
}
