#region Using Directives
using EFS.ACommon;
using EFS.Common;
using EFS.GUI.CCI;
using EFS.GUI.Interface;
using FpML.Interface;
using System;
using System.Linq;
#endregion Using Directives

namespace EFS.TradeInformation
{
    /// <summary>
    /// Description résumée de TradeStub.
    /// </summary>
    public class CciStub : ContainerCciBase, IContainerCciFactory, IContainerCciSpecified
    {
        #region Membres private
        private readonly IStub _stub;
        private readonly object _objectParent;
        private readonly CciTradeBase _cciTrade;
        #endregion Membres private
        //
        #region Enums
        public enum CciEnum
        {
            [System.Xml.Serialization.XmlEnumAttribute("stubTypeFloatingRate")]
            floatingRate1,
            [System.Xml.Serialization.XmlEnumAttribute("stubTypeFloatingRate")]
            floatingRate2,
            [System.Xml.Serialization.XmlEnumAttribute("stubTypeFixedRate")]
            rate,				//FloatingRateOrStubRate
            [System.Xml.Serialization.XmlEnumAttribute("stubTypeAmount.amount")]
            stubAmount_amount,
            [System.Xml.Serialization.XmlEnumAttribute("stubTypeAmount.currency")]
            stubAmount_currency,
            [System.Xml.Serialization.XmlEnumAttribute("stubStartDate.adjustableOrRelativeDateRelativeDate.dateRelativeTo")]
            stubStartDate_relativeDate_dateRelativeTo,
            [System.Xml.Serialization.XmlEnumAttribute("stubEndDate.adjustableOrRelativeDateRelativeDate.dateRelativeTo")]
            stubEndDate_relativeDate_dateRelativeTo,
            unknown
        }
        #endregion Enums
        //
        #region properties
        
        public TradeCustomCaptureInfos Ccis
        {
            get { return base.CcisBase as TradeCustomCaptureInfos; }
        }




        #endregion properties
        //
        #region constructor
        public CciStub(CciTradeBase pTrade, IStub pStub, string pPrefix, object pObjectParent) :
            base(pPrefix, pTrade.Ccis)
        {
            _cciTrade = pTrade;
            _stub = pStub;
            _objectParent = pObjectParent;
        }
        #endregion constructor

        #region Membres de IContainerCciSpecified
        public bool IsSpecified
        {
            get
            {
                bool ret = false;
                //
                if (IsStubTypeAmount)
                    ret = (Cci(CciEnum.stubAmount_amount).IsFilledValue);
                else if (IsStubTypeRate)
                {
                    if (CcisBase.Contains(CciClientId(CciEnum.floatingRate1)))
                        ret = (null != Cci(CciEnum.floatingRate1).Sql_Table);
                    //
                    if (CcisBase.Contains(CciClientId(CciEnum.rate)))
                        ret = ret || (Cci(CciEnum.rate).IsFilledValue);
                }
                return ret;
            }
        }

        public bool IsStubTypeAmount
        {
            get { return CcisBase.Contains(CciClientId(CciEnum.stubAmount_amount)); }
        }
        public bool IsStubTypeRate
        {
            get
            {
                return (
                        CcisBase.Contains(CciClientId(CciEnum.rate)) ||
                        CcisBase.Contains(CciClientId(CciEnum.floatingRate1))
                       );
            }
        }
        #endregion Membres de ItradeSpecified

        

        #region Membres de IContainerCciFactory
        #region Initialize_FromCci
        public void Initialize_FromCci()
        {
            CciTools.CreateInstance(this, _stub);
            //
            if (IsStubTypeRate)
            {
                if (null == _stub.StubTypeFloatingRate)
                    _stub.StubTypeFloatingRate = _stub.CreateFloatingRate;
                else if (_stub.StubTypeFloatingRate.Length == 1)
                    ReflectionTools.AddItemInArray(_stub, "stubTypeFloatingRate", 1);
                //
                if (null == _stub.StubTypeFixedRate)
                    _stub.StubTypeFixedRate = new EFS_Decimal();
            }
            //
            if (IsStubTypeAmount)
            {
                if (null == _stub.StubTypeAmount)
                    _stub.StubTypeAmount = _stub.CreateMoney;
            }
        }
        #endregion Initialize_FromCci
        #region AddCciSystem
        public void AddCciSystem()
        {
            if (CcisBase.Contains(CciClientId(CciEnum.rate)))
            {
                string clientId_WithoutPrefix = CciClientId(CciEnum.floatingRate1);
                CciTools.AddCciSystem(CcisBase, Cst.TXT + clientId_WithoutPrefix, false, TypeData.TypeDataEnum.@string);

                clientId_WithoutPrefix = CciClientId(CciEnum.floatingRate2);
                CciTools.AddCciSystem(CcisBase, Cst.TXT + clientId_WithoutPrefix, false, TypeData.TypeDataEnum.@string);
            }
        }
        #endregion AddCciSystem
        #region Initialize_FromDocument
        /// <summary>
        /// Initialisation des CCI à partir des données "PRODUCT" présentes dans les classes du Document XML
        /// </summary>
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
                    Boolean isToValidate = false;
                    SQL_Table sql_Table = null;
                    #endregion
                    
                    switch (cciEnum)
                    {
                        #region rate
                        case CciEnum.rate:
                            if (_stub.StubTypeFixedRateSpecified)
                                data = _stub.StubTypeFixedRate.Value;
                            else if (_stub.StubTypeFloatingRateSpecified)
                                data = FormatStubFloatingRate(_stub.StubTypeFloatingRate);
                            break;
                        case CciEnum.floatingRate1:
                        case CciEnum.floatingRate2:

                            try
                            {
                                if (_stub.StubTypeFloatingRateSpecified && ArrFunc.IsFilled(_stub.StubTypeFloatingRate))
                                {
                                    SQL_AssetRateIndex sql_RateIndex = null;
                                    IFloatingRate[] floatingRates = _stub.StubTypeFloatingRate;
                                    IFloatingRate floatingRatesItem = null;
                                    //
                                    int idAsset = 0;
                                    //
                                    if (CciEnum.floatingRate1 == cciEnum)
                                        floatingRatesItem = floatingRates[0];
                                    else if (floatingRates.Length >= 2)
                                        floatingRatesItem = floatingRates[1];
                                    //
                                    idAsset = floatingRatesItem.FloatingRateIndex.OTCmlId;
                                    //
                                    if (idAsset > 0)
                                    {
                                        sql_RateIndex = new SQL_AssetRateIndex(_cciTrade.CSCacheOn, SQL_AssetRateIndex.IDType.IDASSET, idAsset);
                                    }
                                    else
                                    {
                                        if (StrFunc.IsFilled(floatingRatesItem.FloatingRateIndex.Value))
                                        {
                                            //20090619 PL TODO MultiSearch
                                            sql_RateIndex = new SQL_AssetRateIndex(_cciTrade.CSCacheOn, SQL_AssetRateIndex.IDType.Asset_Identifier, floatingRatesItem.FloatingRateIndex.Value.Replace(" ", "%") + "%");
                                            //
                                            if (floatingRatesItem.IndexTenorSpecified)
                                            {
                                                sql_RateIndex.Asset_PeriodMltp_In = floatingRatesItem.IndexTenor.PeriodMultiplier.IntValue;
                                                sql_RateIndex.Asset_Period_In = floatingRatesItem.IndexTenor.Period.ToString();
                                            }
                                        }
                                    }
                                    //
                                    if ((null != sql_RateIndex) && sql_RateIndex.IsLoaded)
                                    {
                                        sql_Table = sql_RateIndex;
                                        data = sql_RateIndex.Identifier;
                                        isToValidate = (idAsset == 0);
                                    }
                                }
                            }
                            catch
                            {
                                cci.Sql_Table = null;
                                data = string.Empty;
                            }
                            break;
                        #endregion rate
                        #region stub_amount
                        case CciEnum.stubAmount_amount:
                            if (_stub.StubTypeAmountSpecified)
                                data = _stub.StubTypeAmount.Amount.Value;
                            break;
                        #endregion stub_Amount
                        #region stub_currency
                        case CciEnum.stubAmount_currency:
                            if (_stub.StubTypeAmountSpecified)
                                data = _stub.StubTypeAmount.Currency;
                            break;
                        #endregion stub_currency
                        #region stubStartDate_relativeDate_dateRelativeTo
                        case CciEnum.stubStartDate_relativeDate_dateRelativeTo:
                            if (_stub.StubStartDate.RelativeDateSpecified)
                                data = _stub.StubStartDate.RelativeDate.DateRelativeToValue;
                            break;
                        #endregion stubStartDate_relativeDate_dateRelativeTo
                        #region stubEndDate_relativeDate_dateRelativeTo
                        case CciEnum.stubEndDate_relativeDate_dateRelativeTo:
                            if (_stub.StubEndDate.RelativeDateSpecified)
                                data = _stub.StubEndDate.RelativeDate.DateRelativeToValue;
                            break;
                        #endregion stubEndDate_relativeDate_dateRelativeTo
                        #region default
                        default:
                            isSetting = false;
                            break;
                        #endregion
                    }
                    //
                    if (isSetting)
                    {
                        CcisBase.InitializeCci(cci, sql_Table, data);
                        if (isToValidate)
                            cci.LastValue = ".";
                    }
                }
            }
        }
        #endregion Initialize_FromDocument
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

                    #region Declaration
                    string clientId_WithoutPrefix = cci.ClientId_WithoutPrefix;
                    CustomCaptureInfosBase.ProcessQueueEnum processQueue = CustomCaptureInfosBase.ProcessQueueEnum.None;
                    string data = cci.NewValue;
                    Boolean isSetting = true;
                    #endregion
                    
                    switch (cciEnum)
                    {
                        #region rate
                        case CciEnum.rate:
                            processQueue = CustomCaptureInfosBase.ProcessQueueEnum.High;
                            break;
                        #endregion
                        #region floatingRate1/floatingRate2
                        case CciEnum.floatingRate1:
                        case CciEnum.floatingRate2:
                            //
                            Cci(cciEnum).Sql_Table = null;
                            Cci(cciEnum).ErrorMsg = string.Empty;
                            //
                            if (CcisBase.Contains(CciClientId(CciEnum.rate)))
                            {
                                Cci(CciEnum.rate).Sql_Table = null;
                                Cci(CciEnum.rate).ErrorMsg = string.Empty;
                            }
                            //
                            _stub.StubTypeFixedRateSpecified = RateTools.IsFixedRate(data);
                            _stub.StubTypeFloatingRateSpecified = RateTools.IsFloatingRate(data);
                            //
                            #region Fixe Rate
                            if (_stub.StubTypeFixedRateSpecified)
                            {
                                if (null == _stub.StubTypeFixedRate)
                                    _stub.StubTypeFixedRate = new EFS_Decimal();
                                _stub.StubTypeFixedRate.Value = data;
                            }
                            #endregion Fixe Rate
                            //
                            #region  Floating Rate
                            if (_stub.StubTypeFloatingRateSpecified)
                            {
                                IFloatingRateIndex floatingRateIndex = null;
                                IFloatingRate floatingRate = null;
                                if (CciEnum.floatingRate1 == cciEnum)
                                {
                                    floatingRate = _stub.StubTypeFloatingRate[0];
                                    floatingRateIndex = _stub.StubTypeFloatingRate[0].FloatingRateIndex;
                                }
                                else
                                {
                                    floatingRate = _stub.StubTypeFloatingRate[1];
                                    floatingRateIndex = _stub.StubTypeFloatingRate[1].FloatingRateIndex;
                                }
                                //								
                                IInterval tenorDefault = null;
                                if (_cciTrade.CurrentTrade.Product.ProductBase.IsSwap)
                                {
                                    try
                                    {
                                        IInterestRateStream irs = (IInterestRateStream)_objectParent;
                                        tenorDefault = (IInterval)irs.CalculationPeriodDates.CalculationPeriodFrequency;
                                    }
                                    catch { tenorDefault = null; }
                                }
                                Ccis.DumpFloatingRateIndex_ToDocument(CciClientId(cciEnum), tenorDefault, floatingRate, floatingRateIndex, floatingRate.IndexTenor);
                            }
                            // Afin d'avoir du text en rouge en cas de saisie incorrecte
                            if (CcisBase.Contains(CciClientId(CciEnum.rate)))
                            {
                                Cci(CciEnum.rate).ErrorMsg = Cci(cciEnum).ErrorMsg;
                            }
                            #endregion Floating Rate
                            //
                            processQueue = CustomCaptureInfosBase.ProcessQueueEnum.High;
                            break;
                        #endregion floatingRate1/floatingRate2
                        #region stubAmount_amount
                        case CciEnum.stubAmount_amount:
                            _stub.StubTypeAmountSpecified = cci.IsFilledValue;
                            if (_stub.StubTypeAmountSpecified)
                                _stub.StubTypeAmount.Amount.Value = cci.NewValue;
                            break;
                        #endregion stubAmount_amount
                        #region stubAmount_currency
                        case CciEnum.stubAmount_currency:
                            _stub.StubTypeAmountSpecified = cci.IsFilledValue;
                            if (_stub.StubTypeAmountSpecified)
                                _stub.StubTypeAmount.Currency = cci.NewValue;
                            break;
                        #endregion stubAmount_currency
                        #region default
                        default:
                            isSetting = false;
                            break;
                        #endregion
                    }
                    //
                    if (isSetting)
                        CcisBase.Finalize(clientId_WithoutPrefix, processQueue);
                }

                #region Choice stubType
                if (_stub.StubTypeAmountSpecified)
                {
                    _stub.StubTypeFixedRateSpecified = false;
                    _stub.StubTypeFloatingRateSpecified = false;
                }
                else if (_stub.StubTypeFixedRateSpecified)
                {
                    _stub.StubTypeAmountSpecified = false;
                    _stub.StubTypeFloatingRateSpecified = false;
                }
                else if (_stub.StubTypeFloatingRateSpecified)
                {
                    _stub.StubTypeFixedRateSpecified = false;
                    _stub.StubTypeAmountSpecified = false;
                }
                #endregion Choice stubType

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
            if (IsCciOfContainer(pCci.ClientId_WithoutPrefix))
            {
                string clientId_Element = CciContainerKey(pCci.ClientId_WithoutPrefix);
                CciEnum key = CciEnum.unknown;
                if (System.Enum.IsDefined(typeof(CciEnum), clientId_Element))
                    key = (CciEnum)System.Enum.Parse(typeof(CciEnum), clientId_Element);
                string[] AssetRateIndexes;
                switch (key)
                {
                    case CciEnum.rate:
                        AssetRateIndexes = pCci.NewValue.Split("&".ToCharArray());
                        CcisBase.SetNewValue(this.CciClientId(CciEnum.floatingRate1), AssetRateIndexes[0]);
                        if (AssetRateIndexes.Length > 1)
                            CcisBase.SetNewValue(this.CciClientId(CciEnum.floatingRate2), AssetRateIndexes[1]);
                        else if (StrFunc.IsFilled(CciClientId(CciEnum.floatingRate2)))
                            CcisBase.SetNewValue(CciClientId(CciEnum.floatingRate2), string.Empty);
                        break;
                    case CciEnum.floatingRate1:
                        if (this.IsCciOfContainer(CciClientId(CciEnum.rate)))
                        {
                            if (Cci(CciEnum.rate).NewValue.IndexOf("&") > 0)
                            {
                                AssetRateIndexes = Cci(CciEnum.rate).NewValue.Split("&".ToCharArray());
                                CcisBase.SetNewValue(CciClientId(CciEnum.rate), Cci(CciEnum.floatingRate1).NewValue + " & " + AssetRateIndexes[1]);
                            }
                            else
                                CcisBase.SetNewValue(CciClientId(CciEnum.rate), Cci(CciEnum.floatingRate1).NewValue);
                        }

                        break;
                    case CciEnum.floatingRate2:
                        if (this.IsCciOfContainer(CciClientId(CciEnum.rate)))
                        {
                            if (Cci(CciEnum.rate).NewValue.IndexOf("&") > 0)
                            {
                                AssetRateIndexes = Cci(CciEnum.rate).NewValue.Split("&".ToCharArray());
                                CcisBase.SetNewValue(CciClientId(CciEnum.rate), AssetRateIndexes[0] + " & " + Cci(CciEnum.floatingRate2).NewValue);
                            }
                        }
                        break;

                    default:
                        break;
                }
            }

        }
        #endregion ProcessInitialize
        #region IsClientId
        public bool IsClientId_PayerOrReceiver(CustomCaptureInfo pCci)
        {
            return false;
        }
        #endregion IsClientId
        #region CleanUp
        public void CleanUp()
        {
            // Suppression des streams issus du paramétrage screen et non alimenté
            if (ArrFunc.IsFilled(_stub.StubTypeFloatingRate))
            {
                for (int i = _stub.StubTypeFloatingRate.Length - 1; -1 < i; i--)
                {
                    if (StrFunc.IsEmpty(_stub.StubTypeFloatingRate[i].FloatingRateIndex.Value))
                        ReflectionTools.RemoveItemInArray(_stub, "stubTypeFloatingRate", i);
                }
            }
            _stub.StubTypeFloatingRateSpecified = (ArrFunc.IsFilled(_stub.StubTypeFloatingRate) && (0 < _stub.StubTypeFloatingRate.Length));
        }
        #endregion
        #region SetDisplay
        public void SetDisplay(CustomCaptureInfo pCci)
        {

        }
        #endregion
        #region RemoveLastItemInArray
        public void RemoveLastItemInArray(string _)
        {
            //
        }
        #endregion
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
        #endregion

        #region public methode
        /// <summary>
        /// 
        /// </summary>
        public bool IsCciMasterSpecified 
        { 
            get 
            {
                bool isOk = CcisBase.Contains(CciClientId(CciEnum.rate));
                isOk = isOk || CcisBase.Contains(CciClientId(CciEnum.floatingRate1));
                isOk = isOk || CcisBase.Contains(CciClientId(CciEnum.stubAmount_amount));
                //
                return isOk; 
            }
        }
        #endregion 

        #region private Method
        private string FormatStubFloatingRate(IFloatingRate[] pFloatingRates)
        {
            string ret = string.Empty;
            //
            for (int i = 0; i < pFloatingRates.Length; i++)
            {
                int idAsset = pFloatingRates[i].FloatingRateIndex.OTCmlId;
                //
                if (idAsset > 0)
                {
                    SQL_AssetRateIndex sql_RateIndex = new SQL_AssetRateIndex(_cciTrade.CSCacheOn, SQL_AssetRateIndex.IDType.IDASSET, idAsset);
                    if (sql_RateIndex.IsLoaded)
                    {
                        if (StrFunc.IsFilled(ret))
                            ret += Cst.Space + "&" + Cst.Space;
                        ret += sql_RateIndex.Identifier;
                    }
                }
            }
            return ret;
        }
        #endregion private Method
    }
}
