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
using System.Globalization;
#endregion Using Directives

namespace EFS.TradeInformation
{
    #region CciOrderQuantity
    /// <summary>
    /// CciOrderQuantity
    /// </summary>
    public class CciOrderQuantity : IContainerCciFactory, IContainerCci
    {
        #region enum
        public enum CciEnum
        {
            [System.Xml.Serialization.XmlEnumAttribute("quantityType")]
            quantityType,
            [System.Xml.Serialization.XmlEnumAttribute("numberOfUnits")]
            numberOfUnits,
            #region notional
            [System.Xml.Serialization.XmlEnumAttribute("notional.amount")]
            notional_amount,
            [System.Xml.Serialization.XmlEnumAttribute("notional.currency")]
            notional_currency,
            #endregion
            //
            quantityAmount,
            unknown
        }
        #endregion
       
        #region Members
        private readonly TradeCustomCaptureInfos _ccis;
        private readonly CciTradeBase _cciTrade;
        private readonly DebtSecurityTransactionContainer _debtSecurityTransaction;
        private readonly string _prefix;
        private IOrderQuantity _orderQuantity;
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
        public IOrderQuantity OrderQuantity
        {
            get { return _orderQuantity; }
            set { _orderQuantity = value; }
        }
        // EG 20150624 [21151] New
        public DebtSecurityTransactionContainer DebtSecurityTransaction
        {
            get { return _debtSecurityTransaction; }
        }

        #endregion
       
        #region constructor
        public CciOrderQuantity(CciTradeBase pTrade, string pPrefix, IOrderQuantity pOrderQuantity, DebtSecurityTransactionContainer pDebtSecurityTransaction)
        {
            _cciTrade = pTrade;
            _ccis = _cciTrade.Ccis;
            _debtSecurityTransaction = pDebtSecurityTransaction;
            _orderQuantity = pOrderQuantity;
            _prefix = pPrefix + CustomObject.KEY_SEPARATOR;
        }
        #endregion constructor
       
        #region Membres de IContainerCciFactory
        /// <summary>
        /// 
        /// </summary>
        /// FI 20170116 [21916] Modify (use AddCciSystem Method)
        public void AddCciSystem()
        {
            //Don't erase
            CreateInstance();

            CciTools.AddCciSystem(Ccis, Cst.TXT + CciClientId(CciEnum.numberOfUnits.ToString()), false, TypeData.TypeDataEnum.@string);
            CciTools.AddCciSystem(Ccis, Cst.TXT + CciClientId(CciEnum.notional_amount.ToString()), true, TypeData.TypeDataEnum.@decimal);
            CciTools.AddCciSystem(Ccis, Cst.TXT + CciClientId(CciEnum.notional_currency.ToString()), true, TypeData.TypeDataEnum.@string);
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
                        case CciEnum.quantityType:
                            SetCciQuantityTypeDataType();
                            data = OrderQuantity.QuantityType.ToString();
                            break;
                        case CciEnum.numberOfUnits:
                            if (OrderQuantity.NumberOfUnitsSpecified)
                                data = OrderQuantity.NumberOfUnits.Value;
                            break;
                        case CciEnum.notional_amount:
                            data = OrderQuantity.NotionalAmount.Amount.Value;
                            break;
                        case CciEnum.notional_currency:
                            data = OrderQuantity.NotionalAmount.Currency;
                            break;
                        case CciEnum.quantityAmount:
                            SetCciQuantityTypeDataType();
                            if (OrderQuantity.QuantityType == OrderQuantityType3CodeEnum.CASH)
                                data = OrderQuantity.NotionalAmount.Amount.Value;
                            else if ((OrderQuantity.QuantityType == OrderQuantityType3CodeEnum.UNIT) &&
                                        OrderQuantity.NumberOfUnitsSpecified)
                                data = OrderQuantity.NumberOfUnits.Value;
                            break;
                        default:
                            isSetting = false;
                            break;

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
                string clientId_Key = CciContainerKey(pCci.ClientId_WithoutPrefix);
                //
                CciEnum keyQte = CciEnum.unknown;
                if (System.Enum.IsDefined(typeof(CciEnum), clientId_Key))
                    keyQte = (CciEnum)System.Enum.Parse(typeof(CciEnum), clientId_Key);
                //
                switch (keyQte)
                {
                    #region quantityType
                    case CciEnum.quantityType:
                        SetCciQuantityTypeDataType();
                        //
                        Ccis.SetNewValue(CciClientId(CciEnum.numberOfUnits), string.Empty);
                        Ccis.SetNewValue(CciClientId(CciEnum.notional_amount), string.Empty);
                        Ccis.SetNewValue(CciClientId(CciEnum.notional_currency), string.Empty);
                        //
                        if (null != Cci(CciEnum.quantityAmount))
                        {
                            Cci(CciEnum.quantityAmount).NewValue = string.Empty;
                            Cci(CciEnum.quantityAmount).ErrorMsg = string.Empty;
                        }
                        break;
                    #endregion

                    #region quantityAmount
                    case CciEnum.quantityAmount:
                        CustomCaptureInfo cciNumberOfUnits = Cci(CciEnum.numberOfUnits);
                        CustomCaptureInfo cciNotionalAmount = Cci(CciEnum.notional_amount);
                        CustomCaptureInfo cciNotionalCurrency = Cci(CciEnum.notional_currency);
                        //
                        if (StrFunc.IsFilled(pCci.NewValue))
                        {
                            decimal mtCash = 0;
                            IMoney nominal = null;
                            ISecurityAsset securityAsset = _debtSecurityTransaction.GetSecurityAssetInDataDocument();
                            if (null != securityAsset)
                                nominal = new SecurityAssetContainer(securityAsset).GetNominal(_cciTrade.DataDocument.CurrentProduct.ProductBase);

                            pCci.ErrorMsg = string.Empty;

                            if (OrderQuantity.QuantityType == OrderQuantityType3CodeEnum.CASH)
                            {
                                if (DecFunc.IsDecimal(pCci.NewValue))
                                    mtCash = DecFunc.DecValue(pCci.NewValue);
                                //
                                if (mtCash > 0)
                                {
                                    if ((null != nominal) && nominal.Amount.DecValue > 0)
                                    {
                                        cciNotionalAmount.NewValue = StrFunc.FmtDecimalToInvariantCulture(mtCash);
                                        cciNotionalCurrency.NewValue = nominal.Currency;
                                        cciNumberOfUnits.NewValue = StrFunc.FmtDecimalToInvariantCulture(mtCash / nominal.Amount.DecValue);
                                    }
                                }
                                else
                                    pCci.ErrorMsg = Ressource.GetString("Msg_NotPositiveAmount");
                            }
                            else if (OrderQuantity.QuantityType == OrderQuantityType3CodeEnum.UNIT)
                            {
                                if (null != nominal && nominal.Amount.DecValue > 0)
                                {
                                    if (IntFunc.IsPositiveInteger(pCci.NewValue))
                                    {
                                        // EG 20150920 [21374] Int (int32) to Long (Int64) 
                                        // EG 20170127 Qty Long To Decimal
                                        decimal qteUnit = DecFunc.DecValue(pCci.NewValue);
                                        mtCash = qteUnit * nominal.Amount.DecValue;
                                        cciNumberOfUnits.NewValue = pCci.NewValue;
                                        //
                                        cciNotionalAmount.NewValue = StrFunc.FmtDecimalToInvariantCulture(mtCash);
                                        cciNotionalCurrency.NewValue = nominal.Currency;
                                    }
                                    else
                                        pCci.ErrorMsg = Ressource.GetString("Msg_QtyNotPositiveInteger");
                                }
                                else
                                {
                                    pCci.ErrorMsg = Ressource.GetString("Msg_NoNominal");
                                }
                            }
                        }
                        else
                        {
                            cciNotionalAmount.NewValue = string.Empty;
                            cciNotionalCurrency.NewValue = string.Empty;
                            cciNumberOfUnits.NewValue = string.Empty;
                        }
                        break;
                    #endregion

                    #region unknown
                    case CciOrderQuantity.CciEnum.unknown:
                        break;
                    #endregion
                }
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
                        case CciEnum.quantityType:
                            OrderQuantity.QuantityType = (OrderQuantityType3CodeEnum)Enum.Parse(typeof(OrderQuantityType3CodeEnum), data);
                            processQueue = CustomCaptureInfosBase.ProcessQueueEnum.High;
                            break;

                        case CciEnum.numberOfUnits:
                            OrderQuantity.NumberOfUnits = new EFS_Decimal(data);
                            OrderQuantity.NumberOfUnitsSpecified = StrFunc.IsFilled(data);
                            // FI 20191206 [XXXXX] S'il existe quantityAmount ce dernier met à jour les ccis notional_amount et numberOfUnits simultanément
                            // De manière à ne pas doubler l'appel aux calculs potentiels présents dans des méthodes ProcessInitialize seul le cci notional_amount alimente la queue
                            // Exemple de méthode de calcul CciProductDebtSecurityTransaction.Calc()
                            if (null == Cci(CciEnum.quantityAmount))
                                processQueue = CustomCaptureInfosBase.ProcessQueueEnum.Low;
                            break;
                        case CciEnum.notional_amount:
                            OrderQuantity.NotionalAmount.Amount = new EFS_Decimal(data);
                            processQueue = CustomCaptureInfosBase.ProcessQueueEnum.Low;
                            break;
                        case CciEnum.notional_currency:
                            OrderQuantity.NotionalAmount.Currency = data;
                            processQueue = CustomCaptureInfosBase.ProcessQueueEnum.Low;
                            break;
                        case CciEnum.quantityAmount:
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
        public void RefreshCciEnabled()
        {

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCci"></param>
        public void SetDisplay(CustomCaptureInfo pCci)
        {

            if (IsCci(CciEnum.quantityAmount, pCci))
            {
                pCci.Display = string.Empty;
                //
                if (StrFunc.IsFilled(pCci.NewValue))
                {
                    if (OrderQuantityType3CodeEnum.UNIT == OrderQuantity.QuantityType)
                    {
                        // FI 20190520 [XXXXX] usage de Value plutot que decValue pour ne pas perdre des decimales
                        pCci.Display = Cst.HTMLSpace + "Amount: " + StrFunc.FmtDecimalToCurrentCulture(OrderQuantity.NotionalAmount.Amount.Value) + Cst.Space + OrderQuantity.NotionalAmount.Currency;
                    }
                    else if (OrderQuantityType3CodeEnum.CASH == OrderQuantity.QuantityType)
                    {
                        pCci.Display = OrderQuantity.NotionalAmount.Currency;
                        if (OrderQuantity.NumberOfUnitsSpecified)
                        {
                            pCci.Display = pCci.Display + Cst.HTMLSpace + "Unit: ";
                            //
                            try
                            {
                                // EG 20150920 [21374] Int (int32) to Long (Int64) 
                                // EG 20170127 Qty Long To Decimal
                                decimal qty = OrderQuantity.NumberOfUnits.DecValue;
                                // FI 20190520 [XXXXX] qty.ToString(NumberFormatInfo.InvariantInfo) car la donnée string doit être en invariant Culture
                                pCci.Display += StrFunc.FmtDecimalToCurrentCulture(qty.ToString(NumberFormatInfo.InvariantInfo));
                            }
                            catch { pCci.Display += StrFunc.FmtDecimalToCurrentCulture(OrderQuantity.NumberOfUnits.DecValue); }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void Initialize_Document()
        {
        }
        #endregion Initialize_Document
        
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
        
        #region Methodes
        /// <summary>
        /// 
        /// </summary>
        private void CreateInstance()
        {
            CciTools.CreateInstance(this, OrderQuantity, "CciEnum");
        }
        
        /// <summary>
        /// 
        /// </summary>
        private void SetCciQuantityTypeDataType()
        {
            CustomCaptureInfo cciQuantityAmount = Cci(CciEnum.quantityAmount);
            if (null != cciQuantityAmount)
            {
                if (OrderQuantity.QuantityType == OrderQuantityType3CodeEnum.CASH)
                {
                    cciQuantityAmount.DataType = TypeData.TypeDataEnum.@decimal;
                    cciQuantityAmount.Regex = EFSRegex.TypeRegex.RegexAmountExtend;
                }
                else if (OrderQuantity.QuantityType == OrderQuantityType3CodeEnum.UNIT)
                {
                    cciQuantityAmount.DataType = TypeData.TypeDataEnum.integer;
                    cciQuantityAmount.Regex = EFSRegex.TypeRegex.RegexInteger;
                }
            }

        }
        #endregion
    }
    #endregion
}
