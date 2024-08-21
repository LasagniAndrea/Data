#region Using Directives
using EFS.ACommon;
using EFS.Common;
using EFS.GUI.CCI;
using FpML.Enum;
using FpML.Interface;
using System;
using System.Linq;
#endregion Using Directives

namespace EFS.TradeInformation
{
    /// <summary>
    /// Description résumée de CciExchangeRate.
    /// </summary>
    public class CciExchangeRate : ContainerCciBase, IContainerCciFactory, IContainerCciQuoteBasis, ICciPresentation
    {

        #region Members
        private readonly CciTradeBase trade;
        private readonly IExchangeRate exchangeRate;
        private CciFxFixing _cciFixing;
        #endregion

        #region region Enum
        public enum CciEnum
        {
            [System.Xml.Serialization.XmlEnumAttribute("quotedCurrencyPair.quoteBasis")]
            quotedCurrencyPair_quoteBasis,
            [System.Xml.Serialization.XmlEnumAttribute("quotedCurrencyPair.currency1")]
            quotedCurrencyPair_currency1,
            [System.Xml.Serialization.XmlEnumAttribute("quotedCurrencyPair.currency2")]
            quotedCurrencyPair_currency2,
            [System.Xml.Serialization.XmlEnumAttribute("rate")]
            rate,
            [System.Xml.Serialization.XmlEnumAttribute("spotRate")]
            spotRate,
            [System.Xml.Serialization.XmlEnumAttribute("forwardPoints")]
            forwardPoints,
            [System.Xml.Serialization.XmlEnumAttribute("sideRates")]
            sideRatesSpecified,
            unknown
        }
        #endregion

        #region private Enum TypeFxEnum
        public enum TypeFxEnum
        {
            SPOT = 1,
            FORWARD = 2
        };
        #endregion

        #region accessors
        #region public IsSpotFixing
        public bool IsSpotFixing
        {
            get { return (null != _cciFixing); }
        }
        #endregion
        
        public TradeCustomCaptureInfos Ccis
        {
            get { return base.CcisBase as TradeCustomCaptureInfos; }
        }
        
        #region public CciFxFixing
        public CciFxFixing CciFixing
        {
            get { return _cciFixing; }
        }
        #endregion
        #region public TypeFx
        public TypeFxEnum TypeFx
        {
            get
            {
                return GetTypeFx();
            }
        }
        #endregion public TypeFx
        #region public IsExchangeRateFilled
        /// <summary>
        /// Retourne true les ccis représentatifs  du exchangeRate sont totalement rensignés
        /// <para>Remarque Si Exchange de type FORWARD retourne true si le spot = rate et le forwardpoints=0 </para>
        /// </summary>
        public bool IsExchangeRateFilled
        {
            get
            {
                bool ret = false;
                //
                switch (TypeFx)
                {
                    case TypeFxEnum.SPOT:
                        if (false == IsSpotFixing)
                            ret = Cci(CciEnum.rate).IsFilledValue;
                        break;
                    case TypeFxEnum.FORWARD:
                        ret = Cci(CciEnum.rate).IsFilledValue &&
                              Cci(CciEnum.forwardPoints).IsFilledValue &&
                              Cci(CciEnum.spotRate).IsFilledValue;
                        //20090917 FI On considère que l'ExchangeRate est totalement renseignés si spotRate=rate et forwardPoints=0;
                        if ((false == ret) && (Cci(CciEnum.rate).IsFilledValue && Cci(CciEnum.spotRate).IsFilledValue) && Cci(CciEnum.forwardPoints).IsEmptyValue)
                            ret = (Cci(CciEnum.rate).NewValue == Cci(CciEnum.spotRate).NewValue); 
                        //
                        break;
                }
                return (ret);
            }
        }

        #endregion
        #endregion

        #region constructor
        public CciExchangeRate(CciTradeBase pTrade, IExchangeRate pExchangeRate, string pPrefix) :
            base(pPrefix, pTrade.Ccis)
        {
            trade = pTrade;
            exchangeRate = pExchangeRate;
        }
        #endregion constructor

        

        #region Membres de IContainerCciFactory
        #region Initialize_FromCci
        public void Initialize_FromCci()
        {
            CciTools.CreateInstance(this, exchangeRate);

            if (TypeFxEnum.SPOT == TypeFx)
            {
                // Spot a fixing
                _cciFixing = new CciFxFixing(trade, -1, exchangeRate.FxFixing, Prefix + TradeCustomCaptureInfos.CCst.Prefix_fixing);
                if (CcisBase.Contains(_cciFixing.CciClientId(CciFxFixing.CciEnum.fixingDate)))
                {
                    if (null == exchangeRate.FxFixing)
                        exchangeRate.FxFixing = exchangeRate.CreateFxFixing;
                    _cciFixing.Fixing = exchangeRate.FxFixing;
                    _cciFixing.Initialize_FromCci();
                }
                else
                    _cciFixing = null;
            }
        }
        #endregion Initialize_FromCci
        #region AddCciSystem
        /// <summary>
        /// 
        /// </summary>
        /// FI 20170116 [21916] Modify (use AddCciSystem Method)
        public void AddCciSystem()
        {
            string clientId_WithoutPrefix = CciClientId(CciEnum.quotedCurrencyPair_currency1.ToString());
            CciTools.AddCciSystem(CcisBase, Cst.DDL + clientId_WithoutPrefix, true, TypeData.TypeDataEnum.@string);

            clientId_WithoutPrefix = CciClientId(CciEnum.quotedCurrencyPair_currency2.ToString());
            CciTools.AddCciSystem(CcisBase, Cst.DDL + clientId_WithoutPrefix, true, TypeData.TypeDataEnum.@string);

            clientId_WithoutPrefix = CciClientId(CciEnum.quotedCurrencyPair_quoteBasis.ToString());
            CciTools.AddCciSystem(CcisBase, Cst.DDL + clientId_WithoutPrefix, true, TypeData.TypeDataEnum.@string);

            if (TypeFxEnum.FORWARD == TypeFx)
            {
                clientId_WithoutPrefix = CciClientId(CciEnum.spotRate);
                CciTools.AddCciSystem(CcisBase, Cst.DDL + clientId_WithoutPrefix, false, TypeData.TypeDataEnum.@string);

                clientId_WithoutPrefix = CciClientId(CciEnum.forwardPoints);
                CciTools.AddCciSystem(CcisBase, Cst.DDL + clientId_WithoutPrefix, false, TypeData.TypeDataEnum.@string);
            }

            if (null != CciFixing)
                CciFixing.AddCciSystem();
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
            //
            foreach (CustomCaptureInfo cci in CcisBase)
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
                        #region quotedCurrencyPair
                        case CciEnum.quotedCurrencyPair_quoteBasis:
                            data = exchangeRate.QuotedCurrencyPair.QuoteBasis.ToString();
                            break;
                        case CciEnum.quotedCurrencyPair_currency1:
                            data = exchangeRate.QuotedCurrencyPair.Currency1;
                            break;
                        case CciEnum.quotedCurrencyPair_currency2:
                            data = exchangeRate.QuotedCurrencyPair.Currency2;
                            break;
                        #endregion quotedCurrencyPair
                        case CciEnum.rate:
                            data = exchangeRate.Rate.Value;
                            break;
                        case CciEnum.spotRate:
                            if (exchangeRate.SpotRateSpecified)
                                data = exchangeRate.SpotRate.Value;
                            break;
                        case CciEnum.forwardPoints:
                            if (exchangeRate.ForwardPointsSpecified)
                                data = exchangeRate.ForwardPoints.Value;
                            break;
                        case CciEnum.sideRatesSpecified:
                            data = exchangeRate.SideRatesSpecified.ToString().ToLower();
                            break;
                        default:
                            isSetting = false;
                            break;

                    }
                    if (isSetting)
                        CcisBase.InitializeCci(cci, sql_Table, data);
                }
            }
            //
            if (IsSpotFixing)
                CciFixing.Initialize_FromDocument();

        }
        #endregion
        #region Dump_ToDocument
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
                    
                    string data = cci.NewValue;
                    Boolean isSetting = true;
                    string clientId_Element = CciContainerKey(cci.ClientId_WithoutPrefix);
                    
                    switch (cciEnum)
                    {
                        #region quotedCurrencyPair
                        case CciEnum.quotedCurrencyPair_quoteBasis:
                            if (StrFunc.IsFilled(data))
                            {
                                QuoteBasisEnum QbEnum = (QuoteBasisEnum)System.Enum.Parse(typeof(QuoteBasisEnum), data, true);
                                exchangeRate.QuotedCurrencyPair.QuoteBasis = QbEnum;
                                processQueue = CustomCaptureInfosBase.ProcessQueueEnum.Low;//Afin de recalculer ...
                            }
                            break;
                        case CciEnum.quotedCurrencyPair_currency1:
                            exchangeRate.QuotedCurrencyPair.Currency1 = data;
                            break;
                        case CciEnum.quotedCurrencyPair_currency2:
                            exchangeRate.QuotedCurrencyPair.Currency2 = data;
                            break;
                        #endregion

                        #region Rate
                        case CciEnum.rate:
                            exchangeRate.Rate.Value = data;
                            processQueue = CustomCaptureInfosBase.ProcessQueueEnum.High;//Afin de recalculer ...
                            break;
                        case CciEnum.spotRate:
                            exchangeRate.SpotRate.Value = data;
                            exchangeRate.SpotRateSpecified = StrFunc.IsFilled(data);
                            processQueue = CustomCaptureInfosBase.ProcessQueueEnum.Low;//Afin de recalculer ...
                            break;
                        case CciEnum.forwardPoints:
                            exchangeRate.ForwardPoints.Value = data;
                            exchangeRate.ForwardPointsSpecified = StrFunc.IsFilled(data);
                            processQueue = CustomCaptureInfosBase.ProcessQueueEnum.Low;//Afin de recalculer ...
                            break;

                        case CciEnum.sideRatesSpecified:
                            exchangeRate.SideRatesSpecified = cci.IsFilledValue;
                            break;
                        #endregion

                        #region default
                        default:
                            isSetting = false;
                            break;
                        #endregion
                    }
                    if (isSetting)
                        CcisBase.Finalize(cci.ClientId_WithoutPrefix, processQueue);
                }
                //
            }
            //
            if (IsSpotFixing)
            {
                _cciFixing.Dump_ToDocument();
                exchangeRate.FxFixingSpecified = _cciFixing.IsSpecified;
                if (exchangeRate.FxFixingSpecified)
                {
                    CcisBase.SetNewValue(CciClientId(CciEnum.quotedCurrencyPair_currency1), exchangeRate.FxFixing.QuotedCurrencyPair.Currency1);
                    CcisBase.SetNewValue(CciClientId(CciEnum.quotedCurrencyPair_currency2), exchangeRate.FxFixing.QuotedCurrencyPair.Currency2);
                    CcisBase.SetNewValue(CciClientId(CciEnum.quotedCurrencyPair_quoteBasis), exchangeRate.FxFixing.QuotedCurrencyPair.QuoteBasis.ToString());
                    //
                    exchangeRate.QuotedCurrencyPair.Currency1 = exchangeRate.FxFixing.QuotedCurrencyPair.Currency1;
                    exchangeRate.QuotedCurrencyPair.Currency2 = exchangeRate.FxFixing.QuotedCurrencyPair.Currency2;
                    exchangeRate.Rate.DecValue = 0;
                    exchangeRate.SpotRateSpecified = false;
                    exchangeRate.ForwardPointsSpecified = false;
                    exchangeRate.SideRatesSpecified = false;
                }
            }

        }
        #endregion
        #region ProcessInitialize
        public void ProcessInitialize(CustomCaptureInfo pCci)
        {

            if (IsCciOfContainer(pCci.ClientId_WithoutPrefix))
            {
                string clientId_Element = CciContainerKey(pCci.ClientId_WithoutPrefix);
                CciEnum elt = CciEnum.unknown;
                if (System.Enum.IsDefined(typeof(CciEnum), clientId_Element))
                    elt = (CciEnum)System.Enum.Parse(typeof(CciEnum), clientId_Element);

                CciCompare[] ccic = null;
                bool isCalculate;
                switch (elt)
                {
                    #region QuoteBasis
                    case CciEnum.quotedCurrencyPair_quoteBasis:
                        //CalculRateFromAmount(); 
                        break;
                    #endregion QuoteBasis
                    #region Spot
                    case CciEnum.spotRate:
                        isCalculate = (!IsExchangeRateFilled);
                        if (isCalculate)
                        {
                            // Mise en comment pour revenir en arrière potentiellement
                            //							ccic = new CciCompare[] { 
                            //														 new CciCompare("Rate",Cci(CciEnum.exchangeRate_rate),1), 
                            //														 new CciCompare("ForwardPoints" ,Cci(CciEnum.exchangeRate_forwardPoints),2),
                            //														 new CciCompare("SpotRate" ,Cci(CciEnum.exchangeRate_spotRate),3)
                            //													 };

                            ccic = new CciCompare[] {   new CciCompare("Rate",Cci(CciEnum.rate),1),
                                                        new CciCompare("ForwardPoints" ,Cci(CciEnum.forwardPoints),2)   };
                        }
                        //
                        if (ArrFunc.IsFilled(ccic))
                        {
                            Array.Sort(ccic);
                            //
                            switch (ccic[0].key)
                            {
                                case "Rate":
                                    if (Cci(CciEnum.forwardPoints).IsFilledValue && Cci(CciEnum.spotRate).IsFilledValue)
                                        CalculateExchangedRate(Cst.ExchangeRateType.EXCHANGE);
                                    break;
                                case "ForwardPoints":
                                    if (Cci(CciEnum.spotRate).IsFilledValue && Cci(CciEnum.rate).IsFilledValue)
                                        CalculateExchangedRate(Cst.ExchangeRateType.FORWARDPTS);
                                    break;
                                case "SpotRate":
                                    if (Cci(CciEnum.forwardPoints).IsFilledValue && Cci(CciEnum.rate).IsFilledValue)
                                    {
                                        CalculateExchangedRate(Cst.ExchangeRateType.SPOT);
                                        exchangeRate.SpotRate.Value = Cci(CciEnum.spotRate).NewValue;
                                    }
                                    break;
                            }
                        }
                        break;
                    #endregion Spot
                    #region ForwardPoints
                    case CciEnum.forwardPoints:
                        isCalculate = (!IsExchangeRateFilled);
                        if (isCalculate)
                        {
                            // Mise en comment pour revenir en arrière potentiellement
                            //							ccic = new CciCompare[] { 
                            //														 new CciCompare("Rate",Cci(CciEnum.exchangeRate_rate),1), 
                            //														 new CciCompare("SpotRate" ,Cci(CciEnum.exchangeRate_spotRate),2),
                            //														 new CciCompare("forwardPoints" ,Cci(CciEnum.exchangeRate_forwardPoints),3)
                            //													 };
                            ccic = new CciCompare[] {   new CciCompare("Rate",Cci(CciEnum.rate),1),
                                                        new CciCompare("SpotRate" ,Cci(CciEnum.spotRate),2)};
                        }
                        //
                        if (ArrFunc.IsFilled(ccic))
                        {
                            Array.Sort(ccic);
                            //
                            switch (ccic[0].key)
                            {
                                case "Rate":
                                    if (Cci(CciEnum.forwardPoints).IsFilledValue && Cci(CciEnum.spotRate).IsFilledValue)
                                        CalculateExchangedRate(Cst.ExchangeRateType.EXCHANGE);
                                    break;
                                case "SpotRate":
                                    if (Cci(CciEnum.forwardPoints).IsFilledValue && Cci(CciEnum.rate).IsFilledValue)
                                        CalculateExchangedRate(Cst.ExchangeRateType.SPOT);
                                    break;
                                case "forwardPoints":
                                    if (Cci(CciEnum.spotRate).IsFilledValue && Cci(CciEnum.rate).IsFilledValue)
                                    {
                                        CalculateExchangedRate(Cst.ExchangeRateType.FORWARDPTS);
                                        exchangeRate.ForwardPoints.Value = Cci(CciEnum.forwardPoints).NewValue;
                                    }
                                    break;
                            }
                        }
                        break;
                    #endregion ForwardPoints
                    #region exchangeRate_rate
                    case CciEnum.rate:
                        //
                        //
                        bool existSpotRateOrForwardPoints = ((exchangeRate.SpotRateSpecified) || (exchangeRate.ForwardPointsSpecified));
                        ccic = null;
                        if ((TypeFxEnum.FORWARD == TypeFx) && (existSpotRateOrForwardPoints))
                        {
                            isCalculate = (!IsExchangeRateFilled);
                            if (isCalculate)
                            {
                                ccic = new CciCompare[2] {
                                                             new CciCompare("ForwardPoints",Cci(CciEnum.forwardPoints ),1),
                                                             new CciCompare("SpotRate" ,Cci(CciEnum.spotRate),2) };
                            }
                            //
                            if (ArrFunc.IsFilled(ccic))
                            {
                                Array.Sort(ccic);
                                //
                                switch (ccic[0].key)
                                {
                                    case "ForwardPoints":
                                        if (Cci(CciEnum.spotRate).IsFilledValue && Cci(CciEnum.rate).IsFilledValue)
                                            CalculateExchangedRate(Cst.ExchangeRateType.FORWARDPTS);
                                        break;
                                    case "SpotRate":
                                        if (Cci(CciEnum.forwardPoints).IsFilledValue && Cci(CciEnum.rate).IsFilledValue)
                                            CalculateExchangedRate(Cst.ExchangeRateType.SPOT);
                                        break;
                                }
                            }
                        }
                        break;
                    #endregion exchangeRate_rate


                    #region default
                    default:

                        break;
                        #endregion default
                }
                //			
                if (IsSpotFixing)
                    _cciFixing.ProcessInitialize(pCci);
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
            return false;
        }
        #endregion IsClientId_PayerOrReceiver
        #region CleanUP
        public void CleanUp()
        {
            if (null != CciFixing)
                CciFixing.CleanUp();

            if ((null != exchangeRate.ForwardPoints) && (false == CaptureTools.IsDocumentElementValid(exchangeRate.ForwardPoints)))
                exchangeRate.ForwardPointsSpecified = false;
            if ((null != exchangeRate.SpotRate) && (false == CaptureTools.IsDocumentElementValid(exchangeRate.SpotRate)))
                exchangeRate.SpotRateSpecified = false;
        }
        #endregion
        #region SetDisplay
        public void SetDisplay(CustomCaptureInfo pCci)
        {
            if (IsSpotFixing)
                _cciFixing.SetDisplay(pCci);
        }
        #endregion
        #region RefreshCciEnabled
        public void RefreshCciEnabled()
        {
            if (IsSpotFixing)
                _cciFixing.RefreshCciEnabled();
        }
        #endregion
        #region RemoveLastItemInArray
        public void RemoveLastItemInArray(string pPrefix)
        {
            if (IsSpotFixing)
                _cciFixing.RemoveLastItemInArray(pPrefix);
        }
        #endregion RemoveLastItemInArray
        #region Initialize_Document
        public void Initialize_Document()
        {
        }
        #endregion Initialize_Document
        #endregion Interface  IContainerCciFactory

        #region Membres de IContainerCciQuoteBasis
        #region public IsClientId_QuoteBasis
        public bool IsClientId_QuoteBasis(CustomCaptureInfo pCci)
        {
            bool isOk = false;
            //
            if (IsSpotFixing)
                isOk = CciFixing.IsClientId_QuoteBasis(pCci);
            //
            if (false == isOk)
                isOk = IsCci(CciEnum.quotedCurrencyPair_quoteBasis, pCci);
            //
            return isOk;
        }
        #endregion
        #region public GetCurrency1
        public string GetCurrency1(CustomCaptureInfo pCci)
        {
            string ret = string.Empty;
            //
            if (IsSpotFixing)
                ret = CciFixing.GetCurrency1(pCci);
            //
            if (StrFunc.IsEmpty(ret))
            {
                if (IsCci(CciEnum.quotedCurrencyPair_quoteBasis, pCci))
                    ret = Cci(CciEnum.quotedCurrencyPair_currency1).NewValue;
            }
            //
            return ret;
        }

        #endregion
        #region public GetCurrency2
        public string GetCurrency2(CustomCaptureInfo pCci)
        {
            string ret = string.Empty;
            //
            if (IsSpotFixing)
                ret = CciFixing.GetCurrency2(pCci);
            //
            if (StrFunc.IsEmpty(ret))
            {
                if (IsCci(CciEnum.quotedCurrencyPair_quoteBasis, pCci))
                    ret = Cci(CciEnum.quotedCurrencyPair_currency2).NewValue;
            }
            //
            return ret;
        }
        #endregion
        #region public GetBaseCurrency
        public string GetBaseCurrency(CustomCaptureInfo pCci)
        {
            string ret = string.Empty;
            return ret;
        }
        #endregion
        #endregion Membres de IContainerCciQuoteBasis

        #region public Clear
        public void Clear()
        {
            CciTools.SetCciContainer(this, "NewValue", string.Empty);
            if (null != CciFixing)
                CciFixing.Clear();
        }
        #endregion public Clear
        #region public SetEnabled
        public void SetEnabled(bool pIsEnabled)
        {
            CciTools.SetCciContainer(this, "IsEnabled", pIsEnabled);
            if (null != CciFixing)
                CciFixing.SetEnabled(pIsEnabled);
        }
        #endregion

        #region private GetTypeFx
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private TypeFxEnum GetTypeFx()
        {
            TypeFxEnum ret;
            if (null != Cci(CciEnum.forwardPoints) || null != Cci(CciEnum.spotRate))
                ret = TypeFxEnum.FORWARD;
            else
                ret = TypeFxEnum.SPOT;
            return ret;
        }
        #endregion
        #region private CalculateExchangedRate
        private void CalculateExchangedRate(Cst.ExchangeRateType pFxType)
        {
            decimal spotRate = decimal.Zero;
            decimal forwardPoints = decimal.Zero;

            if (TypeFxEnum.FORWARD == TypeFx)
            {
                if (exchangeRate.SpotRateSpecified)
                    spotRate = exchangeRate.SpotRate.DecValue;
                if (exchangeRate.ForwardPointsSpecified)
                    forwardPoints = exchangeRate.ForwardPoints.DecValue;

                decimal rate = exchangeRate.Rate.DecValue;

                switch (pFxType)
                {
                    case Cst.ExchangeRateType.SPOT:
                        spotRate = rate - forwardPoints;
                        Cci(CciEnum.spotRate).NewValue = StrFunc.FmtDecimalToInvariantCulture(spotRate);
                        break;
                    case Cst.ExchangeRateType.FORWARDPTS:
                        forwardPoints = rate - spotRate;
                        Cci(CciEnum.forwardPoints).NewValue = StrFunc.FmtDecimalToInvariantCulture(forwardPoints);
                        break;
                    case Cst.ExchangeRateType.EXCHANGE:
                        rate = spotRate + forwardPoints;
                        Cci(CciEnum.rate).NewValue = StrFunc.FmtDecimalToInvariantCulture(rate);
                        break;
                }
            }

        }
        #endregion private CalculateExchangedRate

        #region ICciPresentation Membres
        public void DumpSpecific_ToGUI(CciPageBase pPage)
        {
            if (null != _cciFixing)
                _cciFixing.DumpSpecific_ToGUI(pPage);
        }
        #endregion

        
    }
}
