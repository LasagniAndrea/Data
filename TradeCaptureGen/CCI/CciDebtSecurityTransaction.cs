#region Using Directives
using EFS.ACommon;
using EFS.Common;
using EFS.Common.Web;
using EFS.GUI.CCI;
using EfsML.Business;
using EfsML.Enum;
using EfsML.Interface;
using FpML.Interface;
using System;

#endregion Using Directives

namespace EFS.TradeInformation
{
    /// <summary>
    /// Description résumée de CciDebtSecurityTransaction.
    /// </summary>
    public class CciDebtSecurityTransaction : IContainerCciFactory, IContainerCciPayerReceiver, IContainerCci, IContainerCciGetInfoButton, ICciPresentation
    {

        #region Members
        private IDebtSecurityTransaction _debtSecurityTransaction;
        private readonly string _prefix;
        private readonly CciTradeBase _cciTrade;
        private readonly TradeCustomCaptureInfos _ccis;
        private readonly DebtSecurityTransactionContainer _debtSecurityTransactionContainer;
        //
        public CciSecurityAsset cciSecurityAsset;
        public CciOrderQuantity cciQuantity;
        public CciOrderPrice cciPrice;
        public CciPayment cciGrossAmount;
        #endregion

        #region Enum
        public enum CciEnum
        {
            #region buyer/seller
            [System.Xml.Serialization.XmlEnumAttribute("buyer")]
            buyer,
            [System.Xml.Serialization.XmlEnumAttribute("seller")]
            seller,
            #endregion buyer/seller
            unknown,
        }
        #endregion Enum

        #region Public property
        #region buyerPartyReference
        public IReference BuyerPartyReference
        {
            get
            {
                return DebtSecurityTransaction.BuyerPartyReference;
            }
        }
        #endregion
        #region sellerPartyReference
        public IReference SellerPartyReference
        {
            get
            {
                return DebtSecurityTransaction.SellerPartyReference;
            }
        }
        #endregion
        #region IsInputFilled
        /// <summary>
        /// Retroune true si la saisie est complete
        /// </summary>
        public bool IsInputFilled
        {
            get
            {
                bool ret = false;
                bool isDiscount = _debtSecurityTransactionContainer.GetSecurityAssetInDataDocumentIsDiscount();
                //
                IOrderPrice price = DebtSecurityTransaction.Price;
                if (false == isDiscount)
                {
                    //Titre postcompte, si clean Price renseigné, la saisie est complete si le prix et le montant net sont alimentés 
                    if (price.CleanPriceSpecified)
                    {
                        ret = (null != price.CleanPrice) && (price.CleanPrice.DecValue > Decimal.Zero);
                        ret = ret && price.AccruedInterestAmountSpecified;
                        if (price.AccruedInterestAmountSpecified)
                            ret = ret && (null != price.AccruedInterestAmount) && price.AccruedInterestAmount.Amount.DecValue > decimal.Zero;
                    }
                    //Titre postcompte, si dirty Price renseigné, la saisie est complete montant net est alimenté
                    if (price.DirtyPriceSpecified)
                    {
                        ret = (null != price.DirtyPrice) && (price.DirtyPrice.DecValue > Decimal.Zero);
                    }
                    ret = ret && (DebtSecurityTransaction.GrossAmount.PaymentAmount.Amount.DecValue > decimal.Zero);
                }
                else
                {
                    ret = (null != price.CleanPrice) && (price.CleanPrice.DecValue > Decimal.Zero);
                    ret = ret && (DebtSecurityTransaction.GrossAmount.PaymentAmount.Amount.DecValue > decimal.Zero);
                }
                return ret;
            }
        }
        #endregion
        #region ccis
        public TradeCustomCaptureInfos Ccis
        {
            get { return _ccis; }
        }
        #endregion
        #region prefix
        public string Prefix
        {
            get { return _prefix; }
        }
        #endregion
        #region debtSecurityTransaction
        public IDebtSecurityTransaction DebtSecurityTransaction
        {
            get { return _debtSecurityTransaction; }
            set { _debtSecurityTransaction = value; }
        }
        #endregion
        #region DebtSecurityTransactionContainer
        public DebtSecurityTransactionContainer DebtSecurityTransactionContainer
        {
            get { return _debtSecurityTransactionContainer; }
        }
        #endregion
        #endregion

        #region constructor
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pTrade"></param>
        /// <param name="pPrefix"></param>
        public CciDebtSecurityTransaction(CciTrade pTrade, string pPrefix)
            : this(pTrade, pPrefix, null)
        {
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pTrade"></param>
        /// <param name="pPrefix"></param>
        /// <param name="pDebtSecurityTransaction"></param>
        public CciDebtSecurityTransaction(CciTrade pTrade, string pPrefix, IDebtSecurityTransaction pDebtSecurityTransaction)
        {
            _cciTrade = pTrade;
            _ccis = _cciTrade.Ccis;
            _debtSecurityTransaction = pDebtSecurityTransaction;
            if (null == pDebtSecurityTransaction)
                _debtSecurityTransaction = _cciTrade.CurrentTrade.Product.ProductBase.CreateDebtSecurityTransaction();
            _prefix = pPrefix + CustomObject.KEY_SEPARATOR;

            //_debtSecurityTransactionContainer = new DebtSecurityTransactionContainer(debtSecurityTransaction, _cciTrade.DataDocument.dataDocument);
            _debtSecurityTransactionContainer = new DebtSecurityTransactionContainer(DebtSecurityTransaction, _cciTrade.DataDocument);

            cciQuantity = new CciOrderQuantity(pTrade, Prefix + TradeCustomCaptureInfos.CCst.Prefix_orderQuantity, DebtSecurityTransaction.Quantity, _debtSecurityTransactionContainer);
            cciPrice = new CciOrderPrice(pTrade, Prefix + TradeCustomCaptureInfos.CCst.Prefix_orderPrice, DebtSecurityTransaction.Price, _debtSecurityTransactionContainer);
            cciGrossAmount = new CciPayment(pTrade, -1, DebtSecurityTransaction.GrossAmount, CciPayment.PaymentTypeEnum.Payment, Prefix + TradeCustomCaptureInfos.CCst.Prefix_grossAmount)
            {
                ProcessQueueCciDate = CustomCaptureInfosBase.ProcessQueueEnum.High
            };
            //

        }
        #endregion constructor

        #region Membres de IContainerCciPayerReceiver
        /// <summary>
        /// 
        /// </summary>
        public string CciClientIdPayer
        {
            get { return CciClientId(CciEnum.buyer); }
        }

        /// <summary>
        /// 
        /// </summary>
        public string CciClientIdReceiver
        {
            get { return CciClientId(CciEnum.seller); }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pLastValue"></param>
        /// <param name="pNewValue"></param>
        public void SynchronizePayerReceiver(string pLastValue, string pNewValue)
        {

            Ccis.Synchronize(CciClientIdPayer, pLastValue, pNewValue);
            Ccis.Synchronize(CciClientIdReceiver, pLastValue, pNewValue);
            //
            // 20090511 RD 
            // Attention : il ne faut pas modifier le payeur du stream du Titre( en l'occurence l'emetteur du Titre)
            // Seul le receiver est synchronize
            if (null != cciSecurityAsset)
            {
                CciDebtSecurity cciDebtSecurity = cciSecurityAsset.cciDebtSecurity;
                if (null != cciDebtSecurity)
                {
                    // 20090618 RD Dans le cas ou y'a pas de CciStream, il faudrait synchroniser le CciStreamGlobal qui est obligatoire
                    Ccis.Synchronize(cciDebtSecurity.cciStreamGlobal.CciClientIdReceiver, pLastValue, pNewValue, true);
                    //
                    for (int i = 0; i < cciDebtSecurity.DebtSecurityStreamLenght; i++)
                        Ccis.Synchronize(cciDebtSecurity.cciStream[i].CciClientIdReceiver, pLastValue, pNewValue, true);
                }
            }
            //
            cciGrossAmount.SynchronizePayerReceiver(pLastValue, pNewValue);
            //

        }

        #endregion Membres de IContainerCciPayerReceiver

        #region Membres de IContainerCci
        #region CciClientId
        public string CciClientId(CciEnum pEnumValue)
        {
            return CciClientId(pEnumValue.ToString());
        }

        public string CciClientId(string pClientId_Key)
        {
            return Prefix + pClientId_Key;
        }
        #endregion
        #region Cci
        public CustomCaptureInfo Cci(CciEnum pEnumValue)
        {
            return Cci(pEnumValue.ToString());
        }

        public CustomCaptureInfo Cci(string pClientId_Key)
        {
            return Ccis[CciClientId(pClientId_Key)];
        }
        #endregion
        #region IsCciOfContainer
        public bool IsCciOfContainer(string pClientId_WithoutPrefix)
        {
            return pClientId_WithoutPrefix.StartsWith(Prefix);
        }
        #endregion
        #region CciContainerKey
        public string CciContainerKey(string pClientId_WithoutPrefix)
        {
            return pClientId_WithoutPrefix.Substring(Prefix.Length);
        }
        #endregion
        #region IsCci
        public bool IsCci(CciEnum pEnumValue, CustomCaptureInfo pCci)
        {
            return (this.CciClientId(pEnumValue) == pCci.ClientId_WithoutPrefix);
        }
        public bool IsCci(string pEnumValue, CustomCaptureInfo pCci)
        {
            return (this.CciClientId(pEnumValue) == pCci.ClientId_WithoutPrefix);
        }
        #endregion
        #endregion IContainerCci

        #region Membres de IContainerCciFactory
        /// <summary>
        /// 
        /// </summary>
        public void Initialize_FromCci()
        {

            CciTools.CreateInstance(this, DebtSecurityTransaction);
            //
            CciSecurityAsset cciSecurityAssetCurrent = new CciSecurityAsset(_cciTrade, Prefix + TradeCustomCaptureInfos.CCst.Prefix_securityAsset, DebtSecurityTransaction.SecurityAsset);
            //
            bool isOk = Ccis.Contains(cciSecurityAssetCurrent.CciClientId(CciSecurityAsset.CciEnum.securityId));
            if (isOk)
            {
                cciSecurityAsset = cciSecurityAssetCurrent;
                if (null == DebtSecurityTransaction.SecurityAsset)
                {
                    DebtSecurityTransaction.SecurityAsset = _cciTrade.CurrentTrade.Product.ProductBase.CreateSecurityAsset();
                    //
                    // Si un securityId est saisi sur un DebtSecurityTransaction:
                    //      cela revient à considerer que c'est le SecurityAsset qui sera présent sur le DebtSecurityTransaction et non pas le securityAssetReference                                
                    this.DebtSecurityTransaction.SecurityAssetSpecified = true;
                }
                //
                cciSecurityAsset.securityAsset = this.DebtSecurityTransaction.SecurityAsset;
            }
            //
            if (null != cciSecurityAsset)
                cciSecurityAsset.Initialize_FromCci();
            //
            cciQuantity.Initialize_FromCci();
            cciPrice.Initialize_FromCci();
            cciGrossAmount.Initialize_FromCci();

        }
        /// <summary>
        /// 
        /// </summary>
         /// FI 20170116 [21916] Modify (use AddCciSystem Method)
        public void AddCciSystem()
        {

            CciTools.AddCciSystem(Ccis, Cst.DDL + CciClientId(CciEnum.buyer), true, TypeData.TypeDataEnum.@string);
            CciTools.AddCciSystem(Ccis, Cst.DDL + CciClientId(CciEnum.seller), true, TypeData.TypeDataEnum.@string);

            if (null != cciSecurityAsset)
                cciSecurityAsset.AddCciSystem();

            cciQuantity.AddCciSystem();
            cciPrice.AddCciSystem();
            cciGrossAmount.AddCciSystem();

        }
        /// <summary>
        /// 
        /// </summary>
        /// FI 20121106 [18224] tuning Spheres ne balaye plus la collection cci mais la liste des enums de CciEnum
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
                        #region Buyer
                        case CciEnum.buyer:
                            data = BuyerPartyReference.HRef;
                            break;
                        #endregion Buyer
                        #region Seller
                        case CciEnum.seller:
                            data = SellerPartyReference.HRef;
                            break;
                        #endregion Seller
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
            
            if (null != cciSecurityAsset)
                cciSecurityAsset.Initialize_FromDocument();
            
            cciQuantity.Initialize_FromDocument();
            cciPrice.Initialize_FromDocument();
            cciGrossAmount.Initialize_FromDocument();
        }
        /// <summary>
        /// 
        /// </summary>
        /// FI 20121106 [18224] tuning Spheres ne balaye plus la collection cci mais la liste des enums de CciEnum
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

                    switch (cciEnum)
                    {
                        #region Buyer
                        case CciEnum.buyer:
                            BuyerPartyReference.HRef = data;
                            processQueue = CustomCaptureInfosBase.ProcessQueueEnum.Low;
                            break;
                        #endregion Buyer
                        #region Seller
                        case CciEnum.seller:
                            SellerPartyReference.HRef = data;
                            processQueue = CustomCaptureInfosBase.ProcessQueueEnum.Low;
                            break;
                        #endregion
                        #region default
                        default:
                            isSetting = false;
                            break;
                        #endregion
                    }
                    if (isSetting)
                        Ccis.Finalize(cci.ClientId_WithoutPrefix, processQueue);
                }
            }
            //
            if (null != cciSecurityAsset)
                cciSecurityAsset.Dump_ToDocument();
            //
            cciQuantity.Dump_ToDocument();
            cciPrice.Dump_ToDocument();
            cciGrossAmount.Dump_ToDocument();
            //
            if (DebtSecurityTransaction.SecurityAssetSpecified && DebtSecurityTransaction.SecurityAsset.DebtSecuritySpecified)
            {
                for (int i = 0; i < ArrFunc.Count(DebtSecurityTransaction.SecurityAsset.DebtSecurity.Stream); i++)
                    DebtSecurityTransaction.SecurityAsset.DebtSecurity.Stream[i].ReceiverPartyReference.HRef = DebtSecurityTransaction.BuyerPartyReference.HRef;
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
                bool isOk = true;
                //
                if (isOk)
                {
                    #region CciTradeDebtSecurityTransaction
                    CciEnum key = CciEnum.unknown;
                    if (System.Enum.IsDefined(typeof(CciEnum), clientId_Key))
                        key = (CciEnum)System.Enum.Parse(typeof(CciEnum), clientId_Key);
                    //
                    switch (key)
                    {
                        #region Buyer/Seller
                        case CciEnum.buyer:
                            Ccis.Synchronize(CciClientIdReceiver, pCci.NewValue, pCci.LastValue);
                            break;
                        case CciEnum.seller:
                            Ccis.Synchronize(CciClientIdPayer, pCci.NewValue, pCci.LastValue);
                            break;
                        #endregion
                        #region default
                        default:
                            break;
                        #endregion default
                    }
                    #endregion
                }
            }
            //
            if (null != cciSecurityAsset)
            {
                cciSecurityAsset.ProcessInitialize(pCci);
                if ((cciSecurityAsset.IsCci(CciSecurityAsset.CciEnum.securityId, pCci)) && (null != pCci.Sql_Table))
                    InitializeFromDebtSecurity();
            }
            //
            cciQuantity.ProcessInitialize(pCci);
            cciPrice.ProcessInitialize(pCci);
            cciGrossAmount.ProcessInitialize(pCci);
            //
            Calc(pCci);
            //
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCci"></param>
        /// <returns></returns>
        public bool IsClientId_PayerOrReceiver(CustomCaptureInfo pCci)
        {
            bool isOk = false;
            isOk = isOk || (CciClientIdPayer == pCci.ClientId_WithoutPrefix);
            isOk = isOk || (CciClientIdReceiver == pCci.ClientId_WithoutPrefix);
            //
            if ((!isOk) && (null != cciSecurityAsset))
                isOk = cciSecurityAsset.IsClientId_PayerOrReceiver(pCci);
            //
            if (!isOk)
                isOk = cciQuantity.IsClientId_PayerOrReceiver(pCci);
            //
            if (!isOk)
                isOk = cciPrice.IsClientId_PayerOrReceiver(pCci);
            //
            if (!isOk)
                isOk = cciGrossAmount.IsClientId_PayerOrReceiver(pCci);
            //
            return isOk;
        }
        /// <summary>
        /// 
        /// </summary>
        public void CleanUp()
        {
            if (null != cciSecurityAsset)
                cciSecurityAsset.CleanUp();
            cciQuantity.CleanUp();
            cciPrice.CleanUp();
            cciGrossAmount.CleanUp();

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCci"></param>
        public void SetDisplay(CustomCaptureInfo pCci)
        {
            if (null != cciSecurityAsset)
                cciSecurityAsset.SetDisplay(pCci);
            cciQuantity.SetDisplay(pCci);
            cciPrice.SetDisplay(pCci);
            cciGrossAmount.SetDisplay(pCci);

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pIsEnabled"></param>
        public void SetEnabled(bool pIsEnabled)
        {
            CciTools.SetCciContainer(this, "IsEnabled", pIsEnabled);

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pPrefix"></param>
        /// <param name="pParentClientId"></param>
        /// <param name="pParentOccurs"></param>
        /// <returns></returns>
        public int GetArrayElementDocumentCount(string pPrefix, string _1, int _2)
        {
            int ret = -1;
            if (null != cciSecurityAsset)
            {
                if (-1 == ret && null != cciSecurityAsset.cciDebtSecurity)
                    ret = cciSecurityAsset.cciDebtSecurity.GetArrayElementDocumentCount(pPrefix);
            }
            return ret;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pPrefix"></param>
        /// <param name="pOccurs"></param>
        /// <param name="pParentClientId"></param>
        /// <param name="pParentOccurs"></param>
        /// <returns></returns>
        public bool RemoveLastEmptyItemInDocumentArray(string _1, int _2, string _3, int _4)
        {
            return false;
        }
        /// <summary>
        /// 
        /// </summary>
        public void Initialize_Document()
        {

            if (Cst.Capture.IsModeNew(Ccis.CaptureMode) && (false == Ccis.IsPreserveData))
            {
                string id = string.Empty;
                //
                if (StrFunc.IsEmpty(BuyerPartyReference.HRef) && StrFunc.IsEmpty(SellerPartyReference.HRef))
                {
                    //20080523 FI Mise en commentaire, s'il n'y a pas partie il mettre unknown 
                    //HPC est broker ds les template et ne veut pas être 1 contrepartie
                    //id = GetIdFirstPartyCounterparty();
                    id = StrFunc.IsFilled(id) ? id : TradeCustomCaptureInfos.PartyUnknown;
                    //
                    BuyerPartyReference.HRef = id;
                }
                //
                if (TradeCustomCaptureInfos.PartyUnknown == id)
                    _cciTrade.AddPartyUnknown();
                //
                if (null != cciSecurityAsset)
                    cciSecurityAsset.Initialize_Document();
                //
                cciQuantity.Initialize_Document();
                cciPrice.Initialize_Document();
                cciGrossAmount.Initialize_Document();
            }

        }
        /// <summary>
        /// 
        /// </summary>
        public void RefreshCciEnabled()
        {
            if (null != cciSecurityAsset)
                cciSecurityAsset.RefreshCciEnabled();
            //
            cciQuantity.RefreshCciEnabled();
            //
            cciPrice.RefreshCciEnabled();
            //
            cciGrossAmount.RefreshCciEnabled();

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
        /// <param name="pPrefix"></param>
        public void RemoveLastItemInArray(string _)
        { }
        #endregion

        #region Membres de IContainerCciGetInfoButton
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCci"></param>
        /// <param name="pCo"></param>
        /// <param name="pIsSpecified"></param>
        /// <param name="pIsEnabled"></param>
        /// <returns></returns>
        public bool SetButtonZoom(CustomCaptureInfo pCci, CustomObjectButtonFpmlObject pCo, ref bool pIsSpecified, ref bool pIsEnabled)
        {

            bool isOk = false;
            //            
            #region buttons of cciSecurityAsset
            if ((!isOk) && (null != cciSecurityAsset))
                isOk = cciSecurityAsset.SetButtonZoom(pCci, pCo, ref pIsSpecified, ref pIsEnabled);
            #endregion
            //         
            #region buttons of cciGrossAmount
            #region buttons settlementInfo
            if (!isOk)
            {
                isOk = cciGrossAmount.IsCci(CciPayment.CciEnumPayment.settlementInformation, pCci);
                if (isOk)
                {
                    pCo.Element = "settlementInformation";
                    pCo.Object = "grossAmount";
                    pCo.OccurenceValue = 1;
                    pIsSpecified = cciGrossAmount.IsSettlementInfoSpecified;
                    pIsEnabled = cciGrossAmount.IsSettlementInstructionSpecified;
                }
            }
            #endregion buttons settlementInfo
            #endregion
            //
            return isOk;

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCci"></param>
        /// <param name="pCo"></param>
        public void SetButtonReferential(CustomCaptureInfo pCci, CustomObjectButtonReferential pCo)
        {
            #region buttons of cciSecurityAsset
            if (null != cciSecurityAsset)
                cciSecurityAsset.SetButtonReferential(pCci, pCo);
            #endregion

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCci"></param>
        /// <param name="pCo"></param>
        /// <param name="pIsObjSpecified"></param>
        /// <param name="pIsEnabled"></param>
        /// <returns></returns>
        public bool SetButtonScreenBox(CustomCaptureInfo pCci, CustomObjectButtonScreenBox pCo, ref bool pIsObjSpecified, ref bool pIsEnabled)
        {
            return false;
        }
        #endregion Membres de IContainerCciGetInfoButton

        #region public method


        /// <summary>
        /// Retourne l'asset géré (soit il est spécifié, soit c'est une référence) 
        /// </summary>
        /// <returns></returns>
        public ISecurityAsset GetSecurityAssetInDataDocument()
        {
            return _debtSecurityTransactionContainer.GetSecurityAssetInDataDocument();
        }

        /// <summary>
        /// 
        /// </summary>
        public void InitializeFromDebtSecurity()
        {

            ISecurityAsset securityAsset = GetSecurityAssetInDataDocument();
            if (securityAsset.DebtSecuritySpecified)
            {
                #region preproposition en fonction du titre
                IDebtSecurity debtSecurity = securityAsset.DebtSecurity;
                //
                //QuantityType
                OrderQuantityType3CodeEnum quantityType = OrderQuantityType3CodeEnum.CASH;
                if (debtSecurity.Security.OrderRulesSpecified && debtSecurity.Security.OrderRules.QuantityTypeSpecified)
                    quantityType = debtSecurity.Security.OrderRules.QuantityType;
                Ccis.SetNewValue(cciQuantity.CciClientId(CciOrderQuantity.CciEnum.quantityType), quantityType.ToString());
                //
                //CleanPrice/DirtyPrice
                AssetMeasureEnum assetMeasure = AssetMeasureEnum.CleanPrice;
                // 20090921 RD pas besoin de quantityTypeSpecified
                if (debtSecurity.Security.OrderRulesSpecified)
                {
                    if (debtSecurity.Security.OrderRules.AccruedInterestIndicatorSpecified)
                    {
                        if (debtSecurity.Security.OrderRules.AccruedInterestIndicator.BoolValue)
                            assetMeasure = AssetMeasureEnum.DirtyPrice;
                    }
                }
                Ccis.SetNewValue(cciPrice.CciClientId(CciOrderPrice.CciEnum.assetMesure), assetMeasure.ToString());
                //
                //PriceQuoteUnits
                IPriceUnits priceUnits = _cciTrade.CurrentTrade.Product.ProductBase.CreatePriceUnits();
                priceUnits.Value = Cst.PriceQuoteUnits.ParValueDecimal.ToString();
                if (debtSecurity.Security.OrderRulesSpecified && debtSecurity.Security.OrderRules.PriceUnitsSpecified)
                {
                    priceUnits.Value = debtSecurity.Security.OrderRules.PriceUnits.Value;
                }
                else if (debtSecurity.Security.PriceRateTypeSpecified)
                {
                    if (PriceRateType3CodeEnum.DISC == debtSecurity.Security.PriceRateType)
                        priceUnits.Value = Cst.PriceQuoteUnits.Rate.ToString();
                }
                Ccis.SetNewValue(cciPrice.CciClientId(CciOrderPrice.CciEnum.priceUnits), priceUnits.Value);
                //FI 20111018[17602] (Saisie des titres en prix)
                if (PriceQuoteUnitsTools.IsPriceInRate(priceUnits.Value) ||
                    PriceQuoteUnitsTools.IsPriceInPrice(priceUnits.Value))
                    Ccis.SetNewValue(cciPrice.CciClientId(CciOrderPrice.CciEnum.assetMesure), AssetMeasureEnum.DirtyPrice.ToString());
                //
                //Coupon couru
                //if (debtSecurity.security.priceRateTypeSpecified)
                //{
                //    if (PriceRateType3CodeEnum.DISC == debtSecurity.security.priceRateType)
                //    {
                //        ccis.SetNewValue(cciPrice.CciClientId(CciOrderPrice.CciEnum.accruedInterestRate), string.Empty);
                //        ccis.SetNewValue(cciPrice.CciClientId(CciOrderPrice.CciEnum.accruedInterestAmount_amount), string.Empty);
                //        ccis.SetNewValue(cciPrice.CciClientId(CciOrderPrice.CciEnum.accruedInterestAmount_currency), string.Empty);
                //    }
                //}
                //
                //Currency
                IMoney nominal = new SecurityAssetContainer(securityAsset).GetNominal(_cciTrade.DataDocument.CurrentProduct.ProductBase);
                if (StrFunc.IsFilled(nominal.Currency))
                    Ccis.SetNewValue(cciGrossAmount.CciClientId(CciPayment.CciEnum.currency), nominal.Currency);
                //
                #endregion
            }
        }


        #endregion

        #region Private Methods

        /// <summary>
        /// Calcul de cleanPrice, coupon couru, dirtyPrice, grossAmount en fonction du cci saisi
        /// </summary>
        /// <param name="pCci"></param>
        // EG 20150624 [21151] Upd
        private void Calc(CustomCaptureInfo pCci)
        {

            CciCompare ccic = GetCciToCalc(pCci);
            //
            if (null != ccic)
            {
                switch (ccic.key)
                {
                    case "grossAmount":
                        IMoney grosAmount = _debtSecurityTransactionContainer.CalcGrossAmount(_cciTrade.CSCacheOn);
                        if ((grosAmount.Amount.DecValue) > decimal.Zero)
                            cciGrossAmount.Cci(CciPayment.CciEnum.amount).NewValue = StrFunc.FmtDecimalToInvariantCulture(grosAmount.Amount.DecValue);
                        break;
                    case "accruedInterest":
                        IMoney accruedInterest = _debtSecurityTransactionContainer.CalcAccruedInterestByCleanPrice(_cciTrade.CSCacheOn);
                        if ((accruedInterest.Amount.DecValue) > decimal.Zero)
                            cciPrice.Cci(CciOrderPrice.CciEnum.accruedInterestAmount_amount).NewValue = StrFunc.FmtDecimalToInvariantCulture(accruedInterest.Amount.DecValue);
                        break;
                    case "cleanPrice":
                        decimal cleanPrice = _debtSecurityTransactionContainer.CalcCleanPrice();
                        if ((cleanPrice) > decimal.Zero)
                            cciPrice.Cci(CciOrderPrice.CciEnum.price).NewValue = StrFunc.FmtDecimalToInvariantCulture(cleanPrice);
                        break;
                    case "dirtyPrice":
                        decimal dirtyPrice = _debtSecurityTransactionContainer.CalcDirtyPrice();
                        if (dirtyPrice > decimal.Zero)
                            cciPrice.Cci(CciOrderPrice.CciEnum.price).NewValue = StrFunc.FmtDecimalToInvariantCulture(dirtyPrice);
                        break;
                }
            }

        }

        /// <summary>
        /// Retourne le cci 
        /// </summary>
        /// <param name="pCci"></param>
        /// <returns></returns>
        // EG 20150624 [21151]
        private CciCompare GetCciToCalc(CustomCaptureInfo pCci)
        {
            CciCompare ret = null;
            CciCompare[] ccic = null;
            //
            if (DebtSecurityTransaction.Price.CleanPriceSpecified)
            {
                #region cleanPriceSpecified
                //FI 20111018[17602] (Saisie des titres en prix)
                if (cciPrice.IsCci(CciOrderPrice.CciEnum.cleanPrice, pCci) ||
                    cciPrice.IsCci(CciOrderPrice.CciEnum.priceUnits, pCci) ||
                    cciQuantity.IsCci(CciOrderQuantity.CciEnum.notional_amount, pCci) ||
                    cciQuantity.IsCci(CciOrderQuantity.CciEnum.numberOfUnits, pCci)
                    )
                {
                    if (_debtSecurityTransactionContainer.GetSecurityAssetInDataDocumentIsDiscount())
                    {
                        // Si titre precompté
                        // A chaque modif du clean price on recalcule le grossAmount
                        ccic = new CciCompare[] { new CciCompare("grossAmount", cciGrossAmount.Cci(CciPayment.CciEnum.amount), 1) };
                    }
                    else
                    {
                        // Si titre postcompté
                        // A chaque modif du clean price, si tout est déjà renseigné on calcule le grossAmount
                        // sinon la donnée non renseignée entre le grossAmount et le couponCouru
                        // 
                        if (IsInputFilled)
                        {
                            ccic = new CciCompare[] { new CciCompare("grossAmount", cciGrossAmount.Cci(CciPayment.CciEnum.amount), 1) };
                        }
                        else
                        {
                            ccic = new CciCompare[] {new CciCompare("grossAmount",cciGrossAmount.Cci(CciPayment.CciEnum.amount),1), 
											     new CciCompare("accruedInterest" ,cciPrice.Cci(CciOrderPrice.CciEnum.accruedInterestAmount_amount),2)};
                        }
                    }
                }
                else if (cciPrice.IsCci(CciOrderPrice.CciEnum.accruedInterestAmount_amount, pCci) ||
                            cciPrice.IsCci(CciOrderPrice.CciEnum.accruedInterestRate, pCci))
                {
                    // A chaque modif des intérêts courus, si tout est déjà renseigné on calcule le grossAmount
                    // sinon la donnée non renseignée entre le grossAmount et le cleanPrice

                    if (IsInputFilled)
                    {
                        ccic = new CciCompare[] { new CciCompare("grossAmount", cciGrossAmount.Cci(CciPayment.CciEnum.amount), 1) };
                    }
                    else
                    {
                        ccic = new CciCompare[] {new CciCompare("grossAmount",cciGrossAmount.Cci(CciPayment.CciEnum.amount),1), 
											     new CciCompare("cleanPrice" ,cciPrice.Cci(CciOrderPrice.CciEnum.cleanPrice),2)};
                    }
                }
                else if (cciGrossAmount.IsCci(CciPayment.CciEnum.amount, pCci))
                {
                    //A chaque modif du grosAmount, si tout est déjà renseigné on ne calcule rien
                    if (false == IsInputFilled)
                    {
                        //sinon si titre precompté on calcule le cleanPrice (attention la méthode CalcCleanPrice ne sait pas faire le calcul)
                        if (_debtSecurityTransactionContainer.GetSecurityAssetInDataDocumentIsDiscount())
                        {
                            ccic = new CciCompare[] { new CciCompare("cleanPrice", cciPrice.Cci(CciOrderPrice.CciEnum.cleanPrice), 1) };
                        }
                        //
                        //sinon  si titre postCompté 
                        else
                        {
                            //on calcule la donnée non renseignée entre le couponCouru et le cleanPrice
                            ccic = new CciCompare[] {new CciCompare("accruedInterest",cciPrice.Cci(CciOrderPrice.CciEnum.accruedInterestAmount_amount),1), 
											     new CciCompare("cleanPrice" ,cciPrice.Cci(CciOrderPrice.CciEnum.cleanPrice),2)};

                        }
                    }
                }
                #endregion
            }
            else if (DebtSecurityTransaction.Price.DirtyPriceSpecified)
            {
                #region dirtyPriceSpecified
                //FI 20111018[17602] (Saisie des titres en prix)
                if (cciPrice.IsCci(CciOrderPrice.CciEnum.dirtyPrice, pCci) ||
                    cciPrice.IsCci(CciOrderPrice.CciEnum.priceUnits, pCci) ||
                    cciQuantity.IsCci(CciOrderQuantity.CciEnum.notional_amount, pCci) ||
                    cciQuantity.IsCci(CciOrderQuantity.CciEnum.numberOfUnits, pCci))
                {
                    //A chaque modif du dirtyPrice on recalcule le grossAmount
                    ccic = new CciCompare[] { new CciCompare("grossAmount", cciGrossAmount.Cci(CciPayment.CciEnum.amount), 1) };
                }
                else if (cciGrossAmount.IsCci(CciPayment.CciEnum.amount, pCci))
                {
                    //A chaque modif du GrossAmount, si tout est déjà renseigné on ne calcule rien
                    //sinon on calcule le dirtyPrice
                    if (false == IsInputFilled)
                    {
                        ccic = new CciCompare[] { new CciCompare("dirtyPrice", cciPrice.Cci(CciOrderPrice.CciEnum.dirtyPrice), 1) };
                    }
                }
                else if (cciGrossAmount.IsCci(CciPayment.CciEnum.date, pCci))
                {
                    //si titre infine à taux Fixe => Recalcule du grossAmount
                    if (null != GetSecurityAssetInDataDocument())
                    {
                        SecurityAssetContainer secAsset = new SecurityAssetContainer(GetSecurityAssetInDataDocument());
                        //if (secAsset.isTerm && (false == secAsset.isZeroCoupon))
                        //FI 20100406 calcul du gross amount même si zero coupon puisque la date influence le résultat
                        if (secAsset.IsTerm)
                            //A chaque modif de la date on recalcule le grossAmount car la date rentre dans la formule (voir actualisation)
                            ccic = new CciCompare[] { new CciCompare("grossAmount", cciGrossAmount.Cci(CciPayment.CciEnum.amount), 1) };
                    }
                }
                #endregion
            }
            //
            if (ArrFunc.IsFilled(ccic))
            {
                Array.Sort(ccic);
                ret = ccic[0];
            }
            //
            return ret;
        }
        #endregion

        #region ICciPresentation Membres
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pPage"></param>
        ///FI 20120625 Add ICciPresentation implementation
        public void DumpSpecific_ToGUI(CciPageBase pPage)
        {
            if (null != cciSecurityAsset)
                cciSecurityAsset.DumpSpecific_ToGUI(pPage);
        }
        #endregion
    }
}
