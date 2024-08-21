#region Using Directives
using EFS.ACommon;
using EFS.Common;
using EFS.Common.Web;
using EFS.GUI.CCI;
using EFS.GUI.Interface;
using EfsML.Business;
using EfsML.Enum;
using EfsML.Interface;
using FpML.Interface;
using System;
#endregion Using Directives

namespace EFS.TradeInformation
{
    #region CciOrderPrice
    /// <summary>
    /// CciOrderPrice
    /// </summary>
    public class CciOrderPrice : IContainerCciFactory, IContainerCci
    {
        #region Enum cciEnum
        public enum CciEnum
        {
            [System.Xml.Serialization.XmlEnumAttribute("priceUnits")]
            priceUnits,
            [System.Xml.Serialization.XmlEnumAttribute("cleanPrice")]
            cleanPrice,
            [System.Xml.Serialization.XmlEnumAttribute("dirtyPrice")]
            dirtyPrice,
            [System.Xml.Serialization.XmlEnumAttribute("accruedInterestRate")]
            accruedInterestRate,
            [System.Xml.Serialization.XmlEnumAttribute("accruedInterestAmount.amount")]
            accruedInterestAmount_amount,
            [System.Xml.Serialization.XmlEnumAttribute("accruedInterestAmount.currency")]
            accruedInterestAmount_currency,
            //
            assetMesure,
            /// <summary>
            /// Cci qui contient cleanPrice or dirtyPrice en fonction assetMesure
            /// </summary>
            price, 

            unknown
        }
        #endregion
        
        #region Members
        private readonly TradeCustomCaptureInfos _ccis;
        private readonly CciTradeBase _cciTrade;
        private readonly DebtSecurityTransactionContainer _debtSecurityTransaction;
        private readonly string _prefix;
        
        private IOrderPrice _orderPrice;
        #endregion
        
        #region Accessors
        public TradeCustomCaptureInfos Ccis
        {
            get { return _ccis; }
        }
        public string Prefix
        {
            get { return _prefix; }
        }
        public IOrderPrice OrderPrice
        {
            get { return _orderPrice; }
            set { _orderPrice = value; }
        }
        #endregion
        
        #region constructor
        public CciOrderPrice(CciTradeBase pTrade, string pPrefix, IOrderPrice pOrderPrice, DebtSecurityTransactionContainer pDebtSecutityTransactionContainer)
        {
            _cciTrade = pTrade;
            _ccis = _cciTrade.Ccis;
            _orderPrice = pOrderPrice;
            _prefix = pPrefix + CustomObject.KEY_SEPARATOR;
            _debtSecurityTransaction = pDebtSecutityTransactionContainer;
        }
        #endregion constructor
        
        #region Membres de IContainerCciFactory
        /// <summary>
        /// 
        /// </summary>
        /// FI 20170116 [21916] Modify (use AddCciSystem Method)
        public void AddCciSystem()
        {
            CciTools.AddCciSystem(Ccis, Cst.TXT + CciClientId(CciEnum.cleanPrice.ToString()), false, TypeData.TypeDataEnum.@decimal);
            CciTools.AddCciSystem(Ccis, Cst.TXT + CciClientId(CciEnum.dirtyPrice.ToString()), false, TypeData.TypeDataEnum.@decimal);
            CciTools.AddCciSystem(Ccis, Cst.TXT + CciClientId(CciEnum.accruedInterestRate.ToString()), false, TypeData.TypeDataEnum.@decimal);
            CciTools.AddCciSystem(Ccis, Cst.TXT + CciClientId(CciEnum.accruedInterestAmount_amount.ToString()), false, TypeData.TypeDataEnum.@decimal);
            CciTools.AddCciSystem(Ccis, Cst.TXT + CciClientId(CciEnum.accruedInterestAmount_currency.ToString()), false, TypeData.TypeDataEnum.@string);
            //
            //20090921 FI CreateInstance est appelé après l'ajout des ccis systeme
            //afin de générer les objets cleanPrice et dirtyPrice
            //
            CciTools.CreateInstance(this, _orderPrice);
        }

        /// <summary>
        /// 
        /// </summary>
        public void Initialize_FromCci()
        {

        }
        
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
                    SQL_Table sql_Table = null;
                    #endregion
                    //
                    switch (cciEnum)
                    {
                        #region priceUnits
                        case CciEnum.priceUnits:
                            data = OrderPrice.PriceUnits.Value;
                            SetPriceRegEx();
                            break;
                        #endregion priceUnits
                        #region cleanPrice
                        case CciEnum.cleanPrice:
                            //le cas cleanPriceSpecified= true alors que OrderPrice.cleanPrice == null
                            //est possible puisque cleanPriceSpecified est valorisé par le CciEnum.assetMesure indépendamment du price
                            if ((OrderPrice.CleanPriceSpecified) && (null != OrderPrice.CleanPrice))
                                data = OrderPrice.CleanPrice.Value;
                            break;
                        #endregion cleanPrice
                        #region dirtyPrice
                        case CciEnum.dirtyPrice:
                            //le cas dirtyPriceSpecified= true alors que OrderPrice.dirtyPriceSpecified == null
                            //est possible puisque dirtyPriceSpecified est valorisé par le CciEnum.assetMesure indépendamment du price
                            if ((OrderPrice.DirtyPriceSpecified) && (null != OrderPrice.DirtyPrice))
                                data = OrderPrice.DirtyPrice.Value;
                            break;
                        #endregion dirtyPrice
                        #region accruedInterestAmount
                        case CciEnum.accruedInterestAmount_amount:
                            if (OrderPrice.AccruedInterestAmountSpecified)
                                data = OrderPrice.AccruedInterestAmount.Amount.Value;
                            break;
                        case CciEnum.accruedInterestAmount_currency:
                            if (OrderPrice.AccruedInterestAmountSpecified)
                                data = OrderPrice.AccruedInterestAmount.Currency;
                            break;
                        #endregion accruedInterestAmount
                        #region accruedInterestRate
                        case CciEnum.accruedInterestRate:
                            if (OrderPrice.AccruedInterestRateSpecified)
                                data = OrderPrice.AccruedInterestRate.Value;
                            break;
                        #endregion accruedInterestRate
                        #region assetMesure
                        case CciEnum.assetMesure:
                            if (OrderPrice.CleanPriceSpecified)
                                data = AssetMeasureEnum.CleanPrice.ToString();
                            else if (OrderPrice.DirtyPriceSpecified)
                                data = AssetMeasureEnum.DirtyPrice.ToString();
                            break;
                        #endregion assetMesure
                        #region price
                        case CciEnum.price:
                            //(voir les commentaires sur cleanPrice et dirtyPrice
                            if ((OrderPrice.CleanPriceSpecified) && (null != OrderPrice.CleanPrice))
                                data = OrderPrice.CleanPrice.Value;
                            else if ((OrderPrice.DirtyPriceSpecified) && (null != OrderPrice.DirtyPrice))
                                data = OrderPrice.DirtyPrice.Value;
                            break;
                        #endregion price
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCci"></param>
        public void ProcessInitialize(CustomCaptureInfo pCci)
        {
            if (IsCciOfContainer(pCci.ClientId_WithoutPrefix))
            {
                #region CciOrderPrice
                string clientId_Element = CciContainerKey(pCci.ClientId_WithoutPrefix);
                
                CciEnum elt = CciEnum.unknown;
                if (System.Enum.IsDefined(typeof(CciEnum), clientId_Element))
                    elt = (CciEnum)System.Enum.Parse(typeof(CciEnum), clientId_Element);
                
                switch (elt)
                {
                    case CciEnum.priceUnits:
                        SetPriceRegEx();
                        //Si expression en taux le prix est nécessairement dirty
                        if (PriceQuoteUnitsTools.IsPriceInRate(pCci.NewValue))
                            Ccis.SetNewValue(CciClientId(CciEnum.assetMesure), AssetMeasureEnum.DirtyPrice.ToString());
                        //FI 20111018 [17602] (saisie des titres en prix) si expression en prix, le prix est par défaut dirty
                        else if (PriceQuoteUnitsTools.IsPriceInPrice(pCci.NewValue))
                            Ccis.SetNewValue(CciClientId(CciEnum.assetMesure), AssetMeasureEnum.DirtyPrice.ToString());
                        break;
                    case CciEnum.assetMesure:
                        if (OrderPrice.CleanPriceSpecified)
                        {
                            // si on passe d'un dirtyPrice à un cleanPrice Spheres® récupère le prix existant sur le dirty
                            Cci(CciEnum.cleanPrice).NewValue = Cci(CciEnum.dirtyPrice).NewValue;
                            Cci(CciEnum.dirtyPrice).NewValue = string.Empty;
                        }
                        else if (OrderPrice.DirtyPriceSpecified)
                        {
                            // si on passe d'un cleanPrice à un dirtyPrice Spheres® récupère le prix existant sur le cleanPrice
                            Cci(CciEnum.dirtyPrice).NewValue = Cci(CciEnum.cleanPrice).NewValue;
                            Cci(CciEnum.cleanPrice).NewValue = string.Empty;
                        }
                        break;
                    case CciEnum.dirtyPrice:
                    case CciEnum.cleanPrice:
                        pCci.NewValue = StrFunc.FmtDecimalToInvariantCulture(GetRoudingPrice(new EFS_Decimal(pCci.NewValue).DecValue));
                        break;
                    case CciEnum.price:
                        pCci.NewValue = StrFunc.FmtDecimalToInvariantCulture(GetRoudingPrice(new EFS_Decimal(pCci.NewValue).DecValue));
                        //
                        if (OrderPrice.CleanPriceSpecified)
                        {
                            Cci(CciEnum.cleanPrice).NewValue = pCci.NewValue;
                            Cci(CciEnum.dirtyPrice).NewValue = string.Empty;
                        }
                        else if (OrderPrice.DirtyPriceSpecified)
                        {
                            Cci(CciEnum.cleanPrice).NewValue = string.Empty;
                            Cci(CciEnum.dirtyPrice).NewValue = pCci.NewValue;
                        }
                        break;

                    case CciEnum.accruedInterestRate:
                        // 20090606 RD Utiliser une méthode public, pour pouvoir la réutiliser dans CheckTradeValidationRule.CheckDebtSecurityTransactionValidation()
                        // EG 20150624 [21151]
                        IMoney interestAmount = _debtSecurityTransaction.CalcAccruedInterestAmount(_cciTrade.CSCacheOn);
                        Ccis.SetNewValue(CciClientId(CciEnum.accruedInterestAmount_amount), StrFunc.FmtDecimalToInvariantCulture(interestAmount.Amount.DecValue));
                        Ccis.SetNewValue(CciClientId(CciEnum.accruedInterestAmount_currency), interestAmount.Currency);
                        break;
                    case CciEnum.accruedInterestAmount_amount:
                        Ccis.SetNewValue(CciClientId(CciEnum.accruedInterestAmount_currency), _debtSecurityTransaction.DebtSecurityTransaction.Quantity.NotionalAmount.Currency);
                        Ccis.ProcessInitialize_AroundAmount(pCci.ClientId_WithoutPrefix, OrderPrice.AccruedInterestAmount, true);
                        break;
                    case CciEnum.accruedInterestAmount_currency:
                        Ccis.ProcessInitialize_AroundAmount(pCci.ClientId_WithoutPrefix, OrderPrice.AccruedInterestAmount, false);
                        break;
                    case CciEnum.unknown:
                        break;
                }
                #endregion

            }
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCci"></param>
        public void ProcessExecute(CustomCaptureInfo pCci)
        {
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCci"></param>
        public void ProcessExecuteAfterSynchronize(CustomCaptureInfo pCci)
        {

        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCci"></param>
        /// <returns></returns>
        public bool IsClientId_PayerOrReceiver(CustomCaptureInfo pCci)
        {
            return false;
        }
        
        /// <summary>
        /// Déversement des données "PRODUCT" issues des CCI, dans les classes du Document XML
        /// </summary>
        public void Dump_ToDocument()
        {
            foreach (CciEnum cciEnum in Enum.GetValues(typeof(CciEnum)))
            {
                CustomCaptureInfo cci = Cci(cciEnum);
                if ((cci != null) && (cci.HasChanged))
                {
                    #region Reset variables
                    CustomCaptureInfosBase.ProcessQueueEnum processQueue = CustomCaptureInfosBase.ProcessQueueEnum.None;
                    string data = cci.NewValue;
                    bool isSetting = true;
                    #endregion Reset variables
                    //

                    switch (cciEnum)
                    {
                        case CciEnum.priceUnits:
                            OrderPrice.PriceUnits.Value = data;
                            processQueue = CustomCaptureInfosBase.ProcessQueueEnum.Low;
                            break;
                        case CciEnum.cleanPrice:
                            OrderPrice.CleanPrice = new EFS_Decimal(data);
                            processQueue = CustomCaptureInfosBase.ProcessQueueEnum.High;
                            break;
                        case CciEnum.dirtyPrice:
                            OrderPrice.DirtyPrice = new EFS_Decimal(data);
                            processQueue = CustomCaptureInfosBase.ProcessQueueEnum.High;
                            break;
                        case CciEnum.accruedInterestAmount_amount:
                            OrderPrice.AccruedInterestAmountSpecified = StrFunc.IsFilled(data);
                            if (OrderPrice.AccruedInterestAmountSpecified)
                                OrderPrice.AccruedInterestAmount.Amount = new EFS_Decimal(data);
                            processQueue = CustomCaptureInfosBase.ProcessQueueEnum.Low;
                            break;
                        case CciEnum.accruedInterestAmount_currency:
                            OrderPrice.AccruedInterestAmount.Currency = data;
                            break;

                        case CciEnum.accruedInterestRate:
                            OrderPrice.AccruedInterestRate = new EFS_Decimal(data);
                            OrderPrice.AccruedInterestRateSpecified = StrFunc.IsFilled(data);
                            processQueue = CustomCaptureInfosBase.ProcessQueueEnum.Low;
                            break;

                        case CciEnum.assetMesure:
                            OrderPrice.CleanPriceSpecified = (data == AssetMeasureEnum.CleanPrice.ToString());
                            OrderPrice.DirtyPriceSpecified = (data == AssetMeasureEnum.DirtyPrice.ToString());
                            processQueue = CustomCaptureInfosBase.ProcessQueueEnum.Low;
                            break;

                        case CciEnum.price:
                            processQueue = CustomCaptureInfosBase.ProcessQueueEnum.Low;
                            break;

                        default:
                            isSetting = false;
                            break;

                    }
                    if (isSetting)
                        Ccis.Finalize(cci.ClientId_WithoutPrefix, processQueue);
                }
            }
            //
            ISecurityAsset securityAsset = _debtSecurityTransaction.GetSecurityAssetInDataDocument();
            if (null != securityAsset && null != securityAsset.DebtSecurity)
            {
                if ((securityAsset.DebtSecurity.Security.PriceRateTypeSpecified) && 
                    (securityAsset.DebtSecurity.Security.PriceRateType == PriceRateType3CodeEnum.DISC))
                {
                    Cci(CciEnum.accruedInterestRate).NewValue = string.Empty;
                    Cci(CciEnum.accruedInterestAmount_amount).NewValue = string.Empty;
                    Cci(CciEnum.accruedInterestAmount_currency).NewValue = string.Empty;
                }
            }
        }
        
        /// <summary>
        /// 
        /// </summary>
        public void CleanUp()
        {
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pPrefix"></param>
        public void RemoveLastItemInArray(string _)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        // EG 20200918 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc) Corrections et Compléments
        public void RefreshCciEnabled()
        {
            Cci(CciEnum.priceUnits).IsEnabled = true;

            ISecurityAsset securityAsset = _debtSecurityTransaction.GetSecurityAssetInDataDocument();
            IDebtSecurity debtSecurity = null;
            if (null != securityAsset)
                debtSecurity = securityAsset.DebtSecurity;

            if (null != debtSecurity)
            {
                //PriceUnits
                Cci(CciEnum.priceUnits).IsEnabled = true;
                if (debtSecurity.Security.OrderRulesSpecified && debtSecurity.Security.OrderRules.PriceUnitsSpecified)
                    Cci(CciEnum.priceUnits).IsEnabled = (false == debtSecurity.Security.OrderRules.PriceUnits.Forced);

                //CC
                bool isEnabled = true;
                if (debtSecurity.Security.PriceRateTypeSpecified)
                    isEnabled = !(debtSecurity.Security.PriceRateType == PriceRateType3CodeEnum.DISC);

                Cci(CciEnum.accruedInterestRate).IsEnabled = isEnabled;
                Cci(CciEnum.accruedInterestAmount_amount).IsEnabled = isEnabled;
                Cci(CciEnum.accruedInterestAmount_currency).IsEnabled = isEnabled;
            }

            //CciEnum.assetMesure 
            //Lorsque le prix est exprimé en taux ou en prix alors c'est du dirtyPrice nécessairement 
            //=> la combo assetMesure est disabled dans ce cas
            //FI 20111018 [17602] (saisie des titres en prix)
            Cci(CciEnum.assetMesure).IsEnabled = true;
            if (OrderPrice.PriceUnits.Value != null)
            {
                if (PriceQuoteUnitsTools.IsPriceInPrice(OrderPrice.PriceUnits.Value) || PriceQuoteUnitsTools.IsPriceInRate(OrderPrice.PriceUnits.Value))
                    Cci(CciEnum.assetMesure).IsEnabled = false;
                else if (PriceQuoteUnitsTools.IsPriceInParValueDecimal(OrderPrice.PriceUnits.Value) && (null!=debtSecurity) && debtSecurity.Security.OrderRulesSpecified)
                    //FI 20190625 [XXXXX] CleanPrice disabled si (false == accruedInterestIndicatorSpecified)
                    // Cci(CciEnum.assetMesure) sera renseignée avec CleanPrice si accruedInterestIndicator = false ou DirtyPrice dans l'autre cas
                    Cci(CciEnum.assetMesure).IsEnabled = (false == debtSecurity.Security.OrderRules.AccruedInterestIndicatorSpecified);
            }
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCci"></param>
        public void SetDisplay(CustomCaptureInfo pCci)
        {
            if (IsCci(CciEnum.accruedInterestAmount_amount, pCci))
            {
                if (OrderPrice.AccruedInterestAmountSpecified)
                    pCci.Display = OrderPrice.AccruedInterestAmount.Currency;
            }

        }
        
        /// <summary>
        /// 
        /// </summary>
        public void Initialize_Document()
        {
        }
        #endregion

        #region Membres de IContainerCci
        public string CciClientId(CciEnum pEnumValue)
        {
            return CciClientId(pEnumValue.ToString());
        }

        public string CciClientId(string pClientId_Key)
        {
            return Prefix + pClientId_Key;
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
            return pClientId_WithoutPrefix.StartsWith(Prefix);
        }
        
        public string CciContainerKey(string pClientId_WithoutPrefix)
        {
            return pClientId_WithoutPrefix.Substring(Prefix.Length);
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

        #region private Method
        /// <summary>
        /// Alimente le regEx de du cci CciEnum.price en fonction de OrderPrice.priceUnits
        /// </summary>
        /// FI 20151202 [21609] Modify
        private void SetPriceRegEx()
        {
            if (OrderPrice.PriceUnits.Value == Cst.PriceQuoteUnits.Rate.ToString())
                Cci(CciEnum.price).Regex = EFSRegex.TypeRegex.RegexFixedRateExtend;
            else if (OrderPrice.PriceUnits.Value == Cst.PriceQuoteUnits.ParValueDecimal.ToString())
                Cci(CciEnum.price).Regex = EFSRegex.TypeRegex.RegexDecimalExtend; //FI 20151202 [21609] (usage de decimal)
            else if (OrderPrice.PriceUnits.Value == Cst.PriceQuoteUnits.Price.ToString())
                Cci(CciEnum.price).Regex = EFSRegex.TypeRegex.RegexDecimalExtend;
            else
                Cci(CciEnum.price).Regex = EFSRegex.TypeRegex.RegexDecimalExtend;

            /// FI 20151202 [21609] add
            Cci(CciEnum.accruedInterestRate).Regex = Cci(CciEnum.price).Regex; //De toute façon cci accessible uniquement si ParValueDecimal
        }
        
        
        /// <summary>
        /// Application des arrondis sur le price lorsqu'il est exprimé en ParValueDecimal, ParValueFraction et Rate
        /// </summary>
        /// <param name="pData"></param>
        /// <returns></returns>
        public decimal GetRoudingPrice(decimal pPrice)
        {
            decimal ret = pPrice;
            IRounding rounding = null;
            //
            ISecurityAsset securityAsset = _debtSecurityTransaction.GetSecurityAssetInDataDocument();
            if (null != securityAsset)
            {
                if (securityAsset.DebtSecuritySpecified)
                {
                    if (securityAsset.DebtSecurity.Security.OrderRulesSpecified)
                    {
                        if (PriceQuoteUnitsTools.IsPriceInParValueDecimal(OrderPrice.PriceUnits.Value))
                        {
                            if (securityAsset.DebtSecurity.Security.OrderRules.PriceInPercentageRoundingSpecified)
                            {
                                rounding = securityAsset.DebtSecurity.Security.OrderRules.PriceInPercentageRounding;
                            }
                        }
                        else if (PriceQuoteUnitsTools.IsPriceInRate(OrderPrice.PriceUnits.Value))
                        {
                            if (securityAsset.DebtSecurity.Security.OrderRules.PriceInRateRoundingSpecified)
                            {
                                rounding = securityAsset.DebtSecurity.Security.OrderRules.PriceInRateRounding;
                            }
                        }
                        //FI 20111018 [17602] (saisie des titres en prix)
                        else if (PriceQuoteUnitsTools.IsPriceInPrice(OrderPrice.PriceUnits.Value))
                        {
                            rounding = null;
                        }
                    }
                }
            }
            //
            if (null != rounding)
            {
                EFS_Round round = new EFS_Round(rounding)
                {
                    Amount = pPrice
                };
                ret = round.AmountRounded;
            }
            //
            return ret;
        }
        #endregion private Method
    }
    #endregion
}
