#region Using Directives
using EFS.ACommon;
using EFS.Common;
using EFS.Common.Web;
using EFS.GUI.CCI;
using EfsML.Business;
using EfsML.Enum;
using EfsML.Interface;
using FixML.Enum;
using FixML.Interface;
using FixML.v50SP1.Enum;
using FpML.Interface;
using System;
using System.Linq;
using System.Web.UI;


#endregion Using Directives

namespace EFS.TradeInformation
{
    /// <summary>
    /// 
    /// </summary>
    public class CciProductDebtSecurityTransaction : CciProductBase, IContainerCci
    {

        #region Members
        private IDebtSecurityTransaction _debtSecurityTransaction;
        
        #endregion

        #region Enum
        // EG 20171031 [23509] Upd
        // EG 20190730 Upd (Add TrdType, TrdSubTyp, TrdTyp2)
        public enum CciEnum
        {
            /// <summary>
            /// 
            /// </summary>
            [System.Xml.Serialization.XmlEnumAttribute("buyer")]
            buyer,
            /// <summary>
            /// 
            /// </summary>
            [System.Xml.Serialization.XmlEnumAttribute("seller")]
            seller,
            /// <summary>
            /// 
            /// </summary>
            RptSide_Side,
            TrdSubTyp,
            TrdTyp,
            TrdTyp2,

            [System.Xml.Serialization.XmlEnumAttribute("exchangeId")]
            exchangeId,
            unknown,
        }
        #endregion Enum
        
        #region Public property
        /// <summary>
        /// 
        /// </summary>
        public IReference BuyerPartyReference
        {
            get
            {
                return DebtSecurityTransaction.BuyerPartyReference;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public IReference SellerPartyReference
        {
            get
            {
                return DebtSecurityTransaction.SellerPartyReference;
            }
        }

        /// <summary>
        /// Retroune true si la saisie est complete
        /// </summary>
        public bool IsInputFilled
        {
            get
            {
                bool ret = false;
                bool isDiscount = DebtSecurityTransactionContainer.GetSecurityAssetInDataDocumentIsDiscount();
                //
                IOrderPrice price = DebtSecurityTransaction.Price;
                if (false == isDiscount)
                {
                    //Titre postcompte, si clean Price renseigné, la saisie est complete si le prix, le montant de coupon couru et le montant net sont alimentés 
                    if (price.CleanPriceSpecified)
                    {
                        ret = (null != price.CleanPrice) && (price.CleanPrice.DecValue > Decimal.Zero);
                        ret = ret && price.AccruedInterestAmountSpecified;
                        if (price.AccruedInterestAmountSpecified)
                        {
                            // FI 20191204 [XXXXX] Sur les GILTS le accruedInterestAmount peut être négatif
                            //ret = ret && (null != price.accruedInterestAmount) && price.accruedInterestAmount.amount.DecValue > decimal.Zero;
                            ret = ret && (null != price.AccruedInterestAmount) && price.AccruedInterestAmount.Amount.DecValue != decimal.Zero;
                        }
                    }
                    //Titre postcompte, si dirty Price renseigné, la saisie est complete si le prix et le montant net est alimenté
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

        /// <summary>
        /// 
        /// </summary>
        public TradeCustomCaptureInfos Ccis
        {
            get
            {
                return base.CcisBase as TradeCustomCaptureInfos;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        protected CciTrade CciTrade
        {
            get { return base.CciTradeCommon as CciTrade; }
        }
        /// <summary>
        /// 
        /// </summary>
        public IDebtSecurityTransaction DebtSecurityTransaction
        {
            get { return _debtSecurityTransaction; }
        }

        /// <summary>
        /// 
        /// </summary>
        public DebtSecurityTransactionContainer DebtSecurityTransactionContainer
        {
            get; private set;
        }

        #region IdA_Custodian
        // EG 20171031 [23509] New
        public override Nullable<int> IdA_Custodian
        {
            get
            {
                return DebtSecurityTransactionContainer.IdA_Custodian;
            }
        }
        #endregion IdA_Custodian


        /// <summary>
        /// 
        /// </summary>
        public CciSecurityAsset CciSecurityAsset { get; private set; }

        // EG 20150624 [21151] New
        public CciOrderQuantity CciQuantity { get; private set; }
        // EG 20150624 [21151] New
        public CciOrderPrice CciPrice { get; private set; }
        // EG 20150907 [21317] New
        public CciPayment CciGrossAmount { get; private set; }

        #endregion

        #region constructor
        public CciProductDebtSecurityTransaction(CciTrade pCciTrade, IDebtSecurityTransaction pDebtSecurityTransaction, string pPrefix)
            : this(pCciTrade, pDebtSecurityTransaction, pPrefix, -1) { }
        public CciProductDebtSecurityTransaction(CciTrade pCciTrade, IDebtSecurityTransaction pDebtSecurityTransaction, string pPrefix, int pNumber)
            : base((CciTradeCommonBase)pCciTrade, (IProduct)pDebtSecurityTransaction, pPrefix, pNumber)
        {
          
        }
        #endregion constructor

        #region Membres de ITradeCci
        /// <summary>
        /// 
        /// </summary>
        public override string RetSidePayer { get { return SideTools.RetBuySide(); } }
        /// <summary>
        /// 
        /// </summary>
        public override string RetSideReceiver { get { return SideTools.RetSellSide(); } }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pKey"></param>
        /// <param name="pCci"></param>
        /// <returns></returns>
        public override string GetData(string pKey, CustomCaptureInfo pCci)
        {
            //
            string ret = string.Empty;
            //
            return ret;

        }
        #endregion

        #region Membres de IContainerCciPayerReceiver
        /// <summary>
        /// 
        /// </summary>
        public override string CciClientIdPayer
        {
            get { return CciClientId(CciEnum.buyer); }
        }

        /// <summary>
        /// 
        /// </summary>
        public override string CciClientIdReceiver
        {
            get { return CciClientId(CciEnum.seller); }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pLastValue"></param>
        /// <param name="pNewValue"></param>
        public override void SynchronizePayerReceiver(string pLastValue, string pNewValue)
        {

            CcisBase.Synchronize(CciClientIdPayer, pLastValue, pNewValue);
            CcisBase.Synchronize(CciClientIdReceiver, pLastValue, pNewValue);
            //
            // 20090511 RD 
            // Attention : il ne faut pas modifier le payeur du stream du Titre( en l'occurence l'emetteur du Titre)
            // Seul le receiver est synchronize
            if (null != CciSecurityAsset)
            {
                CciDebtSecurity cciDebtSecurity = CciSecurityAsset.cciDebtSecurity;
                if (null != cciDebtSecurity)
                {
                    // 20090618 RD Dans le cas ou y'a pas de CciStream, il faudrait synchroniser le CciStreamGlobal qui est obligatoire
                    CcisBase.Synchronize(cciDebtSecurity.cciStreamGlobal.CciClientIdReceiver, pLastValue, pNewValue, true);
                    //
                    for (int i = 0; i < cciDebtSecurity.DebtSecurityStreamLenght; i++)
                        CcisBase.Synchronize(cciDebtSecurity.cciStream[i].CciClientIdReceiver, pLastValue, pNewValue, true);
                }
            }
            //
            CciGrossAmount.SynchronizePayerReceiver(pLastValue, pNewValue);
            //

        }
        #endregion Membres de IContainerCciPayerReceiver
                
        #region Membres de IContainerCciFactory
        /// <summary>
        /// 
        /// </summary>
        public override void Initialize_FromCci()
        {

            CciTools.CreateInstance(this, DebtSecurityTransaction);
            //
            CciSecurityAsset cciSecurityAssetCurrent = new CciSecurityAsset(CciTrade, Prefix + TradeCustomCaptureInfos.CCst.Prefix_securityAsset, DebtSecurityTransaction.SecurityAsset);
            //
            bool isOk = CcisBase.Contains(cciSecurityAssetCurrent.CciClientId(CciSecurityAsset.CciEnum.securityId));
            if (isOk)
            {
                CciSecurityAsset = cciSecurityAssetCurrent;
                if (null == DebtSecurityTransaction.SecurityAsset)
                {
                    DebtSecurityTransaction.SecurityAsset = CciTrade.CurrentTrade.Product.ProductBase.CreateSecurityAsset();
                    //
                    // Si un securityId est saisi sur un DebtSecurityTransaction:
                    //      cela revient à considerer que c'est le SecurityAsset qui sera présent sur le DebtSecurityTransaction et non pas le securityAssetReference                                
                    this.DebtSecurityTransaction.SecurityAssetSpecified = true;
                }
                //
                CciSecurityAsset.securityAsset = this.DebtSecurityTransaction.SecurityAsset;
            }
            //
            if (null != CciSecurityAsset)
                CciSecurityAsset.Initialize_FromCci();
            //
            CciQuantity.Initialize_FromCci();
            CciPrice.Initialize_FromCci();
            CciGrossAmount.Initialize_FromCci();

        }
        /// <summary>
        /// 
        /// </summary>
        /// FI 20170116 [21916] Modify (use AddCciSystem Method)
        public override void AddCciSystem()
        {
            if (DebtSecurityTransactionContainer.IsOneSide)
                CciTools.AddCciSystem(CcisBase, Cst.DDL + CciClientId(CciEnum.RptSide_Side), true, TypeData.TypeDataEnum.@string);

            CciTools.AddCciSystem(CcisBase, Cst.DDL + CciClientId(CciEnum.buyer), true, TypeData.TypeDataEnum.@string);
            CciTools.AddCciSystem(CcisBase, Cst.DDL + CciClientId(CciEnum.seller), true, TypeData.TypeDataEnum.@string);

            if (null != CciSecurityAsset)
                CciSecurityAsset.AddCciSystem();

            CciQuantity.AddCciSystem();
            CciPrice.AddCciSystem();
            CciGrossAmount.AddCciSystem();

        }
        /// <summary>
        /// 
        /// </summary>
        // EG 20171031 [23509] Upd
        // EG 20190730 Upd (Add TrdType, TrdSubTyp, TrdTyp2)
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
                        #region Side
                        case CciEnum.RptSide_Side:
                            IFixTrdCapRptSideGrp _rptSide = DebtSecurityTransactionContainer.RptSide[0];
                            if (_rptSide.SideSpecified)
                                data = ReflectionTools.ConvertEnumToString<SideEnum>(_rptSide.Side);
                            break;
                        #endregion Side

                        #region Trade Sub Type
                        case CciEnum.TrdSubTyp:
                            if (DebtSecurityTransaction.TrdSubTypeSpecified)
                                data = ReflectionTools.ConvertEnumToString<TrdSubTypeEnum>(DebtSecurityTransaction.TrdSubType);
                            break;
                        #endregion Trade Sub Type
                        #region Trade Type
                        case CciEnum.TrdTyp:
                            if (DebtSecurityTransaction.TrdTypeSpecified)
                                data = ReflectionTools.ConvertEnumToString<TrdTypeEnum>(DebtSecurityTransaction.TrdType);
                            break;
                        #endregion Trade Type
                        #region Secondary Trade Type
                        case CciEnum.TrdTyp2:
                            if (DebtSecurityTransaction.SecondaryTrdTypeSpecified)
                                data = ReflectionTools.ConvertEnumToString<SecondaryTrdTypeEnum>(DebtSecurityTransaction.SecondaryTrdType);
                            break;
                        #endregion Secondary Trade Type

                        case CciEnum.exchangeId:
                            if ((null != DebtSecurityTransaction.DebtSecurity) && DebtSecurityTransaction.DebtSecurity.Security.ExchangeIdSpecified)
                            {
                                data = DebtSecurityTransaction.DebtSecurity.Security.ExchangeId.Value;
                                SQL_Market sqlMarket = new SQL_Market(CciTradeCommon.CS, SQL_TableWithID.IDType.FIXML_SecurityExchange, data, SQL_Table.ScanDataDtEnabledEnum.No);
                                if (sqlMarket.LoadTable())
                                    sql_Table = sqlMarket;
                            }
                            break;


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
            //
            if (null != CciSecurityAsset)
                CciSecurityAsset.Initialize_FromDocument();
            //
            CciQuantity.Initialize_FromDocument();
            CciPrice.Initialize_FromDocument();

            // EG 20150624 [21151] call InitCciGrossAmountDefaultDateSettings
            InitCciGrossAmountDefaultDateSettings();

            CciGrossAmount.Initialize_FromDocument();
        }

        /// <summary>
        /// Initialisation d'un offset sur l'objet cciGrossAmount de manière à pré-proposé une date de rglt qui tient compte du délai de règlement livraison du Bond	
        /// </summary>
        /// EG 20150624 [21151] New
        private void InitCciGrossAmountDefaultDateSettings()
        {
            CciGrossAmount.DefaultDateSettings = null;

            IDebtSecurity debtSecurity = CciSecurityAsset.securityAsset.DebtSecurity;
            if (debtSecurity.Security.OrderRulesSpecified && debtSecurity.Security.OrderRules.SettlementDaysOffsetSpecified)
            {
                IOffset offset = debtSecurity.Security.OrderRules.SettlementDaysOffset;
                IBusinessCenters bcs = null;
                if (debtSecurity.Stream[0].CalculationPeriodDates.CalculationPeriodDatesAdjustments.BusinessCentersDefineSpecified)
                    bcs = debtSecurity.Stream[0].CalculationPeriodDates.CalculationPeriodDatesAdjustments.BusinessCentersDefine;
                else if (debtSecurity.Stream[0].PaymentDates.PaymentDatesAdjustments.BusinessCentersDefineSpecified)
                    bcs = debtSecurity.Stream[0].PaymentDates.PaymentDatesAdjustments.BusinessCentersDefine;

                CciGrossAmount.DefaultDateSettings = new Pair<IOffset, IBusinessCenters>(offset, bcs);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// FI 20160117 [21916] Modify
        // EG 20171031 [23509] Upd
        // EG 20190730 Upd (Add TrdType, TrdSubTyp, TrdTyp2)
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
                        #region Buyer
                        case CciEnum.buyer:
                            BuyerPartyReference.HRef = data;
                            // FI 20160117 [21916] Appel à RptSideSetBuyerSeller (harmonisation des produits contenant un RptSide)
                            RptSideSetBuyerSeller(BuyerSellerEnum.BUYER);
                            processQueue = CustomCaptureInfosBase.ProcessQueueEnum.Low;
                            break;
                        #endregion Buyer
                        #region Seller
                        case CciEnum.seller:
                            SellerPartyReference.HRef = data;
                            // FI 20160117 [21916] Appel à RptSideSetBuyerSeller (harmonisation des produits contenant un RptSide)
                            RptSideSetBuyerSeller(BuyerSellerEnum.SELLER);
                            processQueue = CustomCaptureInfosBase.ProcessQueueEnum.Low;
                            break;
                        #endregion
                        #region Side
                        case CciEnum.RptSide_Side:
                            if (DebtSecurityTransactionContainer.IsOneSide)
                            {
                                IFixTrdCapRptSideGrp _rptSide = DebtSecurityTransactionContainer.RptSide[0];
                                _rptSide.SideSpecified = StrFunc.IsFilled(data);
                                if (_rptSide.SideSpecified)
                                {
                                    SideEnum sideEnum = (SideEnum)ReflectionTools.EnumParse(_rptSide.Side, data);
                                    _rptSide.Side = sideEnum;
                                }
                            }
                            processQueue = CustomCaptureInfosBase.ProcessQueueEnum.High;
                            break;
                        #endregion Side

                        #region Trade Sub Type
                        case CciEnum.TrdSubTyp:
                            DebtSecurityTransaction.TrdSubTypeSpecified = cci.IsFilledValue;
                            if (DebtSecurityTransaction.TrdSubTypeSpecified)
                                DebtSecurityTransaction.TrdSubType = ReflectionTools.ConvertStringToEnum<TrdSubTypeEnum>(data); 
                            break;
                        #endregion Trade Sub Type
                        #region Trade Type
                        case CciEnum.TrdTyp:
                            DebtSecurityTransaction.TrdTypeSpecified = cci.IsFilledValue;
                            if (DebtSecurityTransaction.TrdTypeSpecified)
                                DebtSecurityTransaction.TrdType = ReflectionTools.ConvertStringToEnum<TrdTypeEnum>(data); ;
                            break;
                        #endregion Trade Type
                        #region Secondary Trade Type
                        // 20120711 MF Ticket 18006
                        case CciEnum.TrdTyp2:
                            DebtSecurityTransaction.SecondaryTrdTypeSpecified = cci.IsFilledValue;
                            if (DebtSecurityTransaction.SecondaryTrdTypeSpecified)
                                DebtSecurityTransaction.SecondaryTrdType = ReflectionTools.ConvertStringToEnum<SecondaryTrdTypeEnum>(data);
                            break;
                        #endregion Secondary Trade Type

                        case CciEnum.exchangeId:

                            cci.ErrorMsg = string.Empty;
                            cci.Sql_Table = null;

                            DebtSecurityTransaction.ExchangeIdSpecified = StrFunc.IsFilled(data);
                            if (DebtSecurityTransaction.ExchangeIdSpecified)
                            {

                                SQL_Market sqlMarket = new SQL_Market(CciTradeCommon.CSCacheOn, SQL_TableWithID.IDType.FIXML_SecurityExchange, data, SQL_Table.RestrictEnum.Yes, SQL_Table.ScanDataDtEnabledEnum.Yes, CcisBase.User, CcisBase.SessionId);
                                if (sqlMarket.IsLoaded)
                                {
                                    cci.Sql_Table = sqlMarket;
                                    DebtSecurityTransaction.ExchangeId.Value = sqlMarket.FIXML_SecurityExchange;
                                    Ccis.TradeCommonInput.SetDefault(CommonInput.DefaultEnum.market, data);
                                }
                                else
                                {
                                    cci.ErrorMsg = CciTools.BuildCciErrMsg(Ressource.GetString("Msg_MarketNotFound"), data);
                                }

                            }
                            processQueue = CustomCaptureInfosBase.ProcessQueueEnum.High;
                            break;


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

            // initialisation de TrdTyp si non saisi
            if (null != Cci(CciEnum.TrdTyp))
            {
                if (Cst.Capture.IsModeNew(CcisBase.CaptureMode) && (false == DebtSecurityTransaction.TrdTypeSpecified))
                {
                    if (false == Cci(CciEnum.TrdTyp).IsInputByUser)
                        CcisBase.SetNewValue(CciClientId(CciEnum.TrdTyp), ReflectionTools.ConvertEnumToString<TrdTypeEnum>(TrdTypeEnum.RegularTrade));
                }
            }

            //
            if (null != CciSecurityAsset)
                CciSecurityAsset.Dump_ToDocument();
            //
            CciQuantity.Dump_ToDocument();
            CciPrice.Dump_ToDocument();
            CciGrossAmount.Dump_ToDocument();
            //
            if (DebtSecurityTransaction.SecurityAssetSpecified && DebtSecurityTransaction.SecurityAsset.DebtSecuritySpecified)
            {
                for (int i = 0; i < ArrFunc.Count(DebtSecurityTransaction.SecurityAsset.DebtSecurity.Stream); i++)
                    DebtSecurityTransaction.SecurityAsset.DebtSecurity.Stream[i].ReceiverPartyReference.HRef = DebtSecurityTransaction.BuyerPartyReference.HRef;
            }


            if (Cst.Capture.IsModeNewCapture(CcisBase.CaptureMode) || Cst.Capture.IsModeUpdateGen(CcisBase.CaptureMode))
                Product.SynchronizeFromDataDocument();

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCci"></param>
        /// EG 20171016 [23509] Upd
        public override void ProcessInitialize(CustomCaptureInfo pCci)
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
                            if (DebtSecurityTransactionContainer.IsOneSide)
                            {
                                if (null != _debtSecurityTransaction.SellerPartyReference)
                                {
                                    string clientIdSide = CciClientId(CciEnum.RptSide_Side);
                                    if (CciTrade.cciParty[0].GetPartyId(true) == _debtSecurityTransaction.SellerPartyReference.HRef)
                                        CcisBase.SetNewValue(clientIdSide, ReflectionTools.ConvertEnumToString<SideEnum>(SideEnum.Sell));
                                    else
                                        CcisBase.SetNewValue(clientIdSide, ReflectionTools.ConvertEnumToString<SideEnum>(SideEnum.Buy));
                                }
                            }

                            CcisBase.Synchronize(CciClientIdReceiver, pCci.NewValue, pCci.LastValue);
                            break;
                        case CciEnum.seller:
                            if (DebtSecurityTransactionContainer.IsOneSide)
                            {
                                if (null != _debtSecurityTransaction.BuyerPartyReference)
                                {
                                    string clientIdSide = CciClientId(CciEnum.RptSide_Side);
                                    if (CciTrade.cciParty[0].GetPartyId(true) == _debtSecurityTransaction.BuyerPartyReference.HRef)
                                        CcisBase.SetNewValue(clientIdSide, ReflectionTools.ConvertEnumToString<SideEnum>(SideEnum.Buy));
                                    else
                                        CcisBase.SetNewValue(clientIdSide, ReflectionTools.ConvertEnumToString<SideEnum>(SideEnum.Sell));
                                }
                            }

                            CcisBase.Synchronize(CciClientIdPayer, pCci.NewValue, pCci.LastValue);
                            break;
                        #endregion
                        case CciEnum.RptSide_Side:
                            if (DebtSecurityTransactionContainer.IsOneSide)
                            {
                                IFixTrdCapRptSideGrp _rptSide = DebtSecurityTransactionContainer.RptSide[0];
                                if (_rptSide.SideSpecified)
                                {
                                    string clientId = string.Empty;
                                    if (_rptSide.Side == SideEnum.Buy)
                                        clientId = CciClientIdPayer;
                                    else if (_rptSide.Side == SideEnum.Sell)
                                        clientId = CciClientIdReceiver;
                                    //
                                    if (StrFunc.IsFilled(clientId))
                                        CcisBase.SetNewValue(clientId, CciTrade.cciParty[0].GetPartyId(true));
                                }
                            }
                            break;

                        #region default
                        default:
                            break;
                        #endregion default
                    }
                    #endregion
                }
            }
            
            if (null != CciSecurityAsset)
            {
                CciSecurityAsset.ProcessInitialize(pCci);
                // FI 20190625 [XXXXX] InitializeFromDebtSecurity post saisie d'un template
                if ((CciSecurityAsset.IsCci(CciSecurityAsset.CciEnum.securityId, pCci) && (null != pCci.Sql_Table)) ||
                     CciSecurityAsset.IsCci(CciSecurityAsset.CciEnum.template, pCci) && pCci.IsFilledValue)
                {
                    InitializeFromDebtSecurity();
                }
            }
            
            CciQuantity.ProcessInitialize(pCci);
            CciPrice.ProcessInitialize(pCci);
            CciGrossAmount.ProcessInitialize(pCci);

            // EG 20150624 [21151] call InitCciGrossAmountDefaultDateSettings
            if (CciSecurityAsset.IsCci(CciSecurityAsset.CciEnum.securityId, pCci) || 
                CciSecurityAsset.IsCci(CciSecurityAsset.CciEnum.template, pCci))
                InitCciGrossAmountDefaultDateSettings();

            // FI 20180301 [23814] Utilisation de clearedDate du cciMarket[0]
            // FI 20190520 [XXXXX] Utilisation de IsCCiReferenceForInitPaymentDate
            if (CciSecurityAsset.IsCci(CciSecurityAsset.CciEnum.securityId, pCci) ||
                CciSecurityAsset.IsCci(CciSecurityAsset.CciEnum.template, pCci) ||
                IsCCiReferenceForInitPaymentDate(pCci))
            {
                // FI 20191021 [XXXXX] Modification de l'initilisation de _cciGrossAmount. clientIdDefaultDate
                // puisqu'en duplication de trade la date de référence n'est potentiellement pas saisie et dans ce cas _cciGrossAmount.clientIdDefaultDate n'était pas valorisé
                //if (IsCCiReferenceForInitPaymentDate(pCci))
                if (StrFunc.IsEmpty(CciGrossAmount.ClientIdDefaultDate))
                {
                    CustomCaptureInfo cciDate = CciTrade.cciProduct.GetCCiReferenceForPaymentDate();
                    if (null != cciDate)
                        CciGrossAmount.ClientIdDefaultDate = cciDate.ClientId_WithoutPrefix;
                }

                CciGrossAmount.PaymentDateInitialize(true);
            }

            Calc(pCci);


            if ((false == pCci.HasError) && (pCci.IsFilledValue))
            {
                if (ArrFunc.Count(CciTrade.cciParty) >= 2) //C'est évident mais bon un test de plus
                {
                    if (DebtSecurityTransactionContainer.IsOneSide)
                    {
                        #region Préproposition de la contrepartie en fonction du ClearingTemplate
                        if (CciTrade.cciParty[1].IsInitFromClearingTemplate)
                        {
                            if (CciTrade.cciParty[0].IsCci(CciTradeParty.CciEnum.book, pCci))
                                CciTrade.SetCciClearerOrBrokerFromClearingTemplate(false);
                            //FI 20140815 [XXXXX] initialisation si l'aaset change
                            else if (CciSecurityAsset.IsCci(CciSecurityAsset.CciEnum.securityId, pCci))
                                CciTrade.SetCciClearerOrBrokerFromClearingTemplate(false);
                        }
                        #endregion
                    }
                }

                // EG 20150624 [21151] New
                CustomCaptureInfo cciToInitializeDates = pCci;
                if (IsCci(CciEnum.buyer, pCci) || IsCci(CciEnum.seller, pCci))
                    cciToInitializeDates = CciTrade.cciParty[0].Cci(CciTradeParty.CciEnum.book);
                if (CciTrade.cciParty[0].IsCci(CciTradeParty.CciEnum.book, cciToInitializeDates) ||
                    CciSecurityAsset.IsCci(CciSecurityAsset.CciEnum.securityId, cciToInitializeDates))
                {
                    InitializeDates(cciToInitializeDates);
                }
            }
        }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCci"></param>
        /// <returns></returns>
        public override bool IsClientId_PayerOrReceiver(CustomCaptureInfo pCci)
        {

            bool isOk = false;
            isOk = isOk || (CciClientIdPayer == pCci.ClientId_WithoutPrefix);
            isOk = isOk || (CciClientIdReceiver == pCci.ClientId_WithoutPrefix);
            //
            if ((!isOk) && (null != CciSecurityAsset))
                isOk = CciSecurityAsset.IsClientId_PayerOrReceiver(pCci);
            //
            if (!isOk)
                isOk = CciQuantity.IsClientId_PayerOrReceiver(pCci);
            //
            if (!isOk)
                isOk = CciPrice.IsClientId_PayerOrReceiver(pCci);
            //
            if (!isOk)
                isOk = CciGrossAmount.IsClientId_PayerOrReceiver(pCci);
            //
            return isOk;
        }
        /// <summary>
        /// 
        /// </summary>
        public override void CleanUp()
        {
            if (null != CciSecurityAsset)
                CciSecurityAsset.CleanUp();
            CciQuantity.CleanUp();
            CciPrice.CleanUp();
            CciGrossAmount.CleanUp();

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCci"></param>
        // EG 20171031 [23509] Upd
        public override void SetDisplay(CustomCaptureInfo pCci)
        {
            if (IsCci(CciEnum.exchangeId, pCci))
            {
                if (null != pCci.Sql_Table)
                    CciTools.SetMarKetDisplay(pCci);
            }

            if (null != CciSecurityAsset)
                CciSecurityAsset.SetDisplay(pCci);
            CciQuantity.SetDisplay(pCci);
            CciPrice.SetDisplay(pCci);
            CciGrossAmount.SetDisplay(pCci);

        }
        /// <summary>
        /// 
        /// </summary>
        /// FI 20170116 [21916] Modify
        public override void Initialize_Document()
        {
            //if (Cst.Capture.IsModeInput(ccis.CaptureMode))
            //    _debtSecurityTransactionContainer.InitRptSide(_cciTrade.CS, CciTradeCommon.TradeCommonInput.IsAllocation);

            // FI 20170116 [21916] call InitializeRptSideElement (harmonisation des produits contenant un RptSide)
            base.InitializeRptSideElement(); 


            if (Cst.Capture.IsModeNew(CcisBase.CaptureMode) && (false == CcisBase.IsPreserveData))
            {
                string id = string.Empty;
                //
                if (StrFunc.IsEmpty(BuyerPartyReference.HRef) && StrFunc.IsEmpty(SellerPartyReference.HRef))
                {
                    //HPC est broker ds les template et ne veut pas être 1 contrepartie
                    id = StrFunc.IsFilled(id) ? id : TradeCustomCaptureInfos.PartyUnknown;
                    BuyerPartyReference.HRef = id;
                }

                if (TradeCustomCaptureInfos.PartyUnknown == id)
                    CciTrade.AddPartyUnknown();

                if (null != CciSecurityAsset)
                    CciSecurityAsset.Initialize_Document();

                CciQuantity.Initialize_Document();
                CciPrice.Initialize_Document();
                CciGrossAmount.Initialize_Document();
            }
            
        }
        /// <summary>
        /// 
        /// </summary>
        public override void RefreshCciEnabled()
        {

            if (null != CciSecurityAsset)
                CciSecurityAsset.RefreshCciEnabled();
            //
            CciQuantity.RefreshCciEnabled();
            //
            CciPrice.RefreshCciEnabled();
            //
            CciGrossAmount.RefreshCciEnabled();

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCci"></param>
        public override void ProcessExecute(CustomCaptureInfo pCci)
        {
        }
        #endregion

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCci"></param>
        public override void ProcessExecuteAfterSynchronize(CustomCaptureInfo pCci)
        {

        }

        #region Membres de IContainerCciGetInfoButton
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCci"></param>
        /// <param name="pCo"></param>
        /// <param name="pIsSpecified"></param>
        /// <param name="pIsEnabled"></param>
        /// <returns></returns>
        public override bool SetButtonZoom(CustomCaptureInfo pCci, CustomObjectButtonFpmlObject pCo, ref bool pIsSpecified, ref bool pIsEnabled)
        {
            bool isOk = false;
            //            
            #region buttons of CciSecurityAsset
            if ((!isOk) && (null != CciSecurityAsset))
                isOk = CciSecurityAsset.SetButtonZoom(pCci, pCo, ref pIsSpecified, ref pIsEnabled);
            #endregion
            //         
            #region buttons of cciGrossAmount
            #region buttons settlementInfo
            if (!isOk)
            {
                isOk = CciGrossAmount.IsCci(CciPayment.CciEnumPayment.settlementInformation, pCci);
                if (isOk)
                {
                    pCo.Element = "settlementInformation";
                    pCo.Object = "grossAmount";
                    pCo.OccurenceValue = 1;
                    pIsSpecified = CciGrossAmount.IsSettlementInfoSpecified;
                    pIsEnabled = CciGrossAmount.IsSettlementInstructionSpecified;
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
        public override void SetButtonReferential(CustomCaptureInfo pCci, CustomObjectButtonReferential pCo)
        {


            if (null != CciSecurityAsset)
                CciSecurityAsset.SetButtonReferential(pCci, pCo);

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCci"></param>
        /// <param name="pCo"></param>
        /// <param name="pIsObjSpecified"></param>
        /// <param name="pIsEnabled"></param>
        /// <returns></returns>
        public override bool SetButtonScreenBox(CustomCaptureInfo pCci, CustomObjectButtonScreenBox pCo, ref bool pIsObjSpecified, ref bool pIsEnabled)
        {
            return false;
        }
        #endregion

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pPage"></param>
        // EG 20171031 [23509] Upd
        public override void DumpSpecific_ToGUI(CciPageBase pPage)
        {
            CustomCaptureInfo cci = Cci(CciEnum.exchangeId);
            if (null != cci)
            {
                pPage.SetMarketCountryImage(cci);
                pPage.SetOpenFormReferential(cci, Cst.OTCml_TBL.MARKET);
            }

            if (null != CciSecurityAsset)
            {
                //Si click sur isNewAsset => affichage du panel securityAsset
                //eventTarget => Control à l'origine du post
                bool isModeConsult = Cst.Capture.IsModeConsult(CcisBase.CaptureMode);
                string eventTarget = string.Empty + pPage.Request.Params["__EVENTTARGET"];
                //
                bool isNewAsset = CciSecurityAsset.IsNewAsset;
                CustomCaptureInfo cciNewAsset = CciSecurityAsset.Cci(CciSecurityAsset.CciEnum.isNewAsset);
                CustomCaptureInfo cciTemplate = CciSecurityAsset.Cci(CciSecurityAsset.CciEnum.template);
                if (StrFunc.IsFilled(eventTarget))
                {
                    bool isDisplay = (null != cciNewAsset) && (cciNewAsset.ClientId == eventTarget);
                    isDisplay = isDisplay || ((null != cciTemplate) && (cciTemplate.ClientId == eventTarget));
                    if (isDisplay)
                    {
                        string id = Cst.IMG + CciSecurityAsset.Prefix + "tblDebtSecurityAsset";
                        pPage.ShowLinkControl(id);
                    }
                }
                Control ctrl = pPage.FindControl(cciTemplate.ClientId);
                if (null != ctrl)
                    ctrl.Visible = isNewAsset;
                ctrl = pPage.FindControl("LBL" + cciTemplate.ClientId_WithoutPrefix);
                if (null != ctrl)
                    ctrl.Visible = isNewAsset;
                ctrl = pPage.FindControl(cciNewAsset.ClientId);
                if (null != ctrl)
                {
                    ctrl.Visible = (false == isModeConsult);
                    if (ctrl.Parent.Parent.GetType().Equals(typeof(System.Web.UI.WebControls.TableRow)))
                        ctrl.Parent.Parent.Visible = ctrl.Visible;
                }
                ctrl = pPage.FindControl("LBL" + cciNewAsset.ClientId_WithoutPrefix);
                if (null != ctrl)
                    ctrl.Visible = (false == isModeConsult);

                /// FI 20120625 all _cciSecurityAsset.DumpSpecific_ToGUI 
                CciSecurityAsset.DumpSpecific_ToGUI(pPage);
            }
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
        /// Retourne l'asset géré (soit il est spécifié, soit c'est une référence) 
        /// </summary>
        /// <returns></returns>
        public ISecurityAsset GetSecurityAssetInDataDocument()
        {
            return DebtSecurityTransactionContainer.GetSecurityAssetInDataDocument();
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

                if (debtSecurity.Security.ExchangeIdSpecified)
                    CcisBase.SetNewValue(this.CciClientId(CciEnum.exchangeId), debtSecurity.Security.ExchangeId.Value);

                //
                //QuantityType
                OrderQuantityType3CodeEnum quantityType = OrderQuantityType3CodeEnum.CASH;
                if (debtSecurity.Security.OrderRulesSpecified && debtSecurity.Security.OrderRules.QuantityTypeSpecified)
                    quantityType = debtSecurity.Security.OrderRules.QuantityType;
                CcisBase.SetNewValue(CciQuantity.CciClientId(CciOrderQuantity.CciEnum.quantityType), quantityType.ToString());
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
                        else
                            //FI 20190625 [XXXXX] CleanPrice si (false == accruedInterestIndicator)
                            assetMeasure = AssetMeasureEnum.CleanPrice;
                    }
                }
                CcisBase.SetNewValue(CciPrice.CciClientId(CciOrderPrice.CciEnum.assetMesure), assetMeasure.ToString());
                
                //
                //PriceQuoteUnits
                IPriceUnits priceUnits = CciTrade.CurrentTrade.Product.ProductBase.CreatePriceUnits();
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
                CcisBase.SetNewValue(CciPrice.CciClientId(CciOrderPrice.CciEnum.priceUnits), priceUnits.Value);
                if (Cst.PriceQuoteUnits.Rate.ToString() == priceUnits.Value)
                    CcisBase.SetNewValue(CciPrice.CciClientId(CciOrderPrice.CciEnum.assetMesure), AssetMeasureEnum.DirtyPrice.ToString());
                //
                //Coupon couru
                //if (debtSecurity.security.priceRateTypeSpecified)
                //{
                //    if (PriceRateType3CodeEnum.DISC == debtSecurity.security.priceRateType)
                //    {
                //        ccis.SetNewValue(CciPrice.CciClientId(CciOrderPrice.CciEnum.accruedInterestRate), string.Empty);
                //        ccis.SetNewValue(CciPrice.CciClientId(CciOrderPrice.CciEnum.accruedInterestAmount_amount), string.Empty);
                //        ccis.SetNewValue(CciPrice.CciClientId(CciOrderPrice.CciEnum.accruedInterestAmount_currency), string.Empty);
                //    }
                //}
                //
                //Currency
                IMoney nominal = new SecurityAssetContainer(securityAsset).GetNominal(CciTrade.DataDocument.CurrentProduct.ProductBase);
                if (StrFunc.IsFilled(nominal.Currency))
                    CcisBase.SetNewValue(CciGrossAmount.CciClientId(CciPayment.CciEnum.currency), nominal.Currency);




                // EG 20150624 (21151] Mise en commentaire : Le calcul de la date est fait sur ProcessInitialize (Appel à ccoGrossAmount.PaymentDateInitialize)
                //DateTime date = new SecurityAssetContainer(securityAsset).CalcPaymentDate(_cciTrade.CSCacheOn, _cciTrade.DataDocument.tradeDate);
                //if (DtFunc.IsDateTimeFilled(date))
                //    ccis.SetNewValue(_cciGrossAmount.CciClientId(CciPayment.CciEnum.date), DtFunc.DateTimeToStringDateISO(date));
                #endregion
            }

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pProduct"></param>
        /// EG 20171016 [23509] Upd
        public override void SetProduct(IProduct pProduct)
        {
            _debtSecurityTransaction = (IDebtSecurityTransaction)pProduct;
            DebtSecurityTransactionContainer = new DebtSecurityTransactionContainer(DebtSecurityTransaction, CciTrade.DataDocument);
            //
            IOrderQuantity quantity = null;
            if (null != _debtSecurityTransaction)
                quantity = _debtSecurityTransaction.Quantity;
            CciQuantity = new CciOrderQuantity(CciTrade, Prefix + TradeCustomCaptureInfos.CCst.Prefix_orderQuantity, quantity, DebtSecurityTransactionContainer);
            //
            IOrderPrice price = null;
            if (null != _debtSecurityTransaction)
                price = _debtSecurityTransaction.Price;
            CciPrice = new CciOrderPrice(CciTrade, Prefix + TradeCustomCaptureInfos.CCst.Prefix_orderPrice, price, DebtSecurityTransactionContainer);
            //
            IPayment payment = null;
            if (null != _debtSecurityTransaction)
                payment = _debtSecurityTransaction.GrossAmount;

            //FI 20180301 [23814] pClientIdDefaultDate => String.empty
            CciGrossAmount = new CciPayment(CciTrade, -1, payment, CciPayment.PaymentTypeEnum.Payment, Prefix + TradeCustomCaptureInfos.CCst.Prefix_grossAmount,
                string.Empty, string.Empty, string.Empty, string.Empty, string.Empty)
            {
                ProcessQueueCciDate = CustomCaptureInfosBase.ProcessQueueEnum.High
            };

            base.SetProduct(pProduct);

        }

        /// <summary>
        /// Calcul de cleanPrice, coupon couru, dirtyPrice, grossAmount suite à la saisie de la donnée {pCci}
        /// </summary>
        /// <param name="pCci"></param>
        private void Calc(CustomCaptureInfo pCci)
        {
            String key = GetCciToCalc(pCci);

            if (StrFunc.IsFilled(key))
            {
                switch (key)
                {
                    case "grossAmount":
                        IMoney grosAmount = DebtSecurityTransactionContainer.CalcGrossAmount(CciTrade.CSCacheOn);
                        if ((grosAmount.Amount.DecValue) > decimal.Zero)
                            CciGrossAmount.Cci(CciPayment.CciEnum.amount).NewValue = StrFunc.FmtDecimalToInvariantCulture(grosAmount.Amount.DecValue);
                        break;
                    case "accruedInterestAmount":
                        // FI 20190801 [24820] Appel à CalcAccruedInterestAmount si GrossAmount non renseigné
                        IMoney accruedInterest;
                        if (DebtSecurityTransactionContainer.GetGrossAmount().Amount.DecValue > 0)
                            accruedInterest = DebtSecurityTransactionContainer.CalcAccruedInterestByCleanPrice(CciTradeCommon.CSCacheOn);
                        else
                            accruedInterest = DebtSecurityTransactionContainer.CalcAccruedInterestAmount(CciTradeCommon.CSCacheOn);

                        // FI 20191204 [XXXXX]  Sur les GILTS accruedInterest peut être négatif
                        if ((accruedInterest.Amount.DecValue) != decimal.Zero)
                            CciPrice.Cci(CciOrderPrice.CciEnum.accruedInterestAmount_amount).NewValue = StrFunc.FmtDecimalToInvariantCulture(accruedInterest.Amount.DecValue);
                        break;
                    case "cleanPrice":
                        decimal cleanPrice = DebtSecurityTransactionContainer.CalcCleanPrice();
                        if ((cleanPrice) > decimal.Zero)
                            CciPrice.Cci(CciOrderPrice.CciEnum.price).NewValue = StrFunc.FmtDecimalToInvariantCulture(cleanPrice);
                        break;
                    case "dirtyPrice":
                        decimal dirtyPrice = DebtSecurityTransactionContainer.CalcDirtyPrice();
                        if (dirtyPrice > decimal.Zero)
                            CciPrice.Cci(CciOrderPrice.CciEnum.price).NewValue = StrFunc.FmtDecimalToInvariantCulture(dirtyPrice);
                        break;
                }
            }
        }

        /// <summary>
        /// Retourne la donnée qui doit être pré-proposée suite à la saisie de la donnée {pCci}
        /// <para>Retourne null si rien est à pré-proposer</para>
        /// <para>Valeurs possibles : grossAmount, accruedInterestAmount, cleanPrice, dirtyPrice</para> 
        /// </summary>
        /// <param name="pCci">Représente la donnée précédemment saisie</param>
        /// <returns></returns>
        private string GetCciToCalc(CustomCaptureInfo pCci)
        {
            string ret = null;
            CciCompare[] ccic = null;
            
            bool isDiscount = DebtSecurityTransactionContainer.GetSecurityAssetInDataDocumentIsDiscount();
            IOrderPrice price = DebtSecurityTransaction.Price;

            if (price.CleanPriceSpecified)
            {
                #region cleanPriceSpecified
                //FI 20111018 [17602] (saisie des titres en prix) Add numberOfUnits
                if (CciPrice.IsCci(CciOrderPrice.CciEnum.cleanPrice, pCci) ||
                    CciPrice.IsCci(CciOrderPrice.CciEnum.priceUnits, pCci) ||
                    CciQuantity.IsCci(CciOrderQuantity.CciEnum.notional_amount, pCci) ||
                    CciQuantity.IsCci(CciOrderQuantity.CciEnum.numberOfUnits, pCci))
                {
                    if (isDiscount)
                    {
                        // Si titre precompté
                        // A chaque modif du clean price on recalcule le grossAmount
                        ccic = new CciCompare[] { new CciCompare("grossAmount", CciGrossAmount.Cci(CciPayment.CciEnum.amount), 1) };
                    }
                    else
                    {
                        // Si titre postcompté
                        // A chaque modif du clean price, si tout est déjà renseigné on calcule le grossAmount
                        // sinon la donnée non renseignée entre le grossAmount et le couponCouru
                        if (IsInputFilled)
                        {
                            // FI 20190801 [24820] si modification de la qté ou du nominal, lorsque tout est déja renseigné 
                            //=> calcul des intérêts courus si le taux est renseigné => cela impliquera un recalcul du grossAmount
                            if ((CciQuantity.IsCci(CciOrderQuantity.CciEnum.notional_amount, pCci) ||
                                CciQuantity.IsCci(CciOrderQuantity.CciEnum.numberOfUnits, pCci)) &&
                                price.AccruedInterestAmountSpecified && price.AccruedInterestRateSpecified)
                            {
                                DebtSecurityTransaction.GrossAmount.PaymentAmount.Amount.DecValue = 0; // Mise à zéro pour recalcul
                                ccic = new CciCompare[] { new CciCompare("accruedInterestAmount", CciPrice.Cci(CciOrderPrice.CciEnum.accruedInterestAmount_amount), 1) };
                            }
                            else
                            {
                                ccic = new CciCompare[] { new CciCompare("grossAmount", CciGrossAmount.Cci(CciPayment.CciEnum.amount), 1) };
                            }
                        }
                        else
                        {
                            ccic = new CciCompare[] {new CciCompare("grossAmount",CciGrossAmount.Cci(CciPayment.CciEnum.amount),1), 
                                                 new CciCompare("accruedInterestAmount" ,CciPrice.Cci(CciOrderPrice.CciEnum.accruedInterestAmount_amount),2)};
                        }
                    }
                }
                else if (CciPrice.IsCci(CciOrderPrice.CciEnum.accruedInterestAmount_amount, pCci) ||
                            CciPrice.IsCci(CciOrderPrice.CciEnum.accruedInterestRate, pCci))
                {
                    // A chaque modif des intérêts courus, si tout est déjà renseigné on calcule le grossAmount
                    // sinon la donnée non renseignée entre le grossAmount et le cleanPrice

                    if (IsInputFilled)
                    {
                        ccic = new CciCompare[] { new CciCompare("grossAmount", CciGrossAmount.Cci(CciPayment.CciEnum.amount), 1) };
                    }
                    else
                    {
                        ccic = new CciCompare[] {new CciCompare("grossAmount",CciGrossAmount.Cci(CciPayment.CciEnum.amount),1), 
                                                 new CciCompare("cleanPrice" ,CciPrice.Cci(CciOrderPrice.CciEnum.cleanPrice),2)};
                    }
                }
                else if (CciGrossAmount.IsCci(CciPayment.CciEnum.amount, pCci))
                {
                    //A chaque modif du grosAmount, si tout est déjà renseigné on ne calcule rien
                    if (false == IsInputFilled)
                    {
                        //sinon si titre precompté on calcule le cleanPrice (attention la méthode CalcCleanPrice ne sait pas faire le calcul)
                        if (isDiscount)
                        {
                            ccic = new CciCompare[] { new CciCompare("cleanPrice", CciPrice.Cci(CciOrderPrice.CciEnum.cleanPrice), 1) };
                        }
                        //
                        //sinon  si titre postCompté 
                        else
                        {
                            //on calcule la donnée non renseignée entre le couponCouru et le cleanPrice
                            ccic = new CciCompare[] {new CciCompare("accruedInterestAmount",CciPrice.Cci(CciOrderPrice.CciEnum.accruedInterestAmount_amount),1), 
                                                 new CciCompare("cleanPrice" ,CciPrice.Cci(CciOrderPrice.CciEnum.cleanPrice),2)};

                        }
                    }
                }
                #endregion
            }
            else if (price.DirtyPriceSpecified)
            {
                #region dirtyPriceSpecified
                //FI 20111018 [17602] (saisie des titres en prix) Add numberOfUnits
                if (CciPrice.IsCci(CciOrderPrice.CciEnum.dirtyPrice, pCci) ||
                    CciPrice.IsCci(CciOrderPrice.CciEnum.priceUnits, pCci) ||
                    CciQuantity.IsCci(CciOrderQuantity.CciEnum.notional_amount, pCci) ||
                    CciQuantity.IsCci(CciOrderQuantity.CciEnum.numberOfUnits, pCci)
                )
                {
                    //A chaque modif du dirtyPrice on recalcule le grossAmount
                    ccic = new CciCompare[] { new CciCompare("grossAmount", CciGrossAmount.Cci(CciPayment.CciEnum.amount), 1) };
                }
                else if (CciGrossAmount.IsCci(CciPayment.CciEnum.amount, pCci))
                {
                    //A chaque modif du GrossAmount, si tout est déjà renseigné on ne calcule rien
                    //sinon on calcule le dirtyPrice
                    if (false == IsInputFilled)
                    {
                        ccic = new CciCompare[] { new CciCompare("dirtyPrice", CciPrice.Cci(CciOrderPrice.CciEnum.dirtyPrice), 1) };
                    }
                }
                else if (CciGrossAmount.IsCci(CciPayment.CciEnum.date, pCci))
                {
                    //si titre infine à taux Fixe => Recalcule du grossAmount
                    if (null != GetSecurityAssetInDataDocument())
                    {
                        SecurityAssetContainer secAsset = new SecurityAssetContainer(GetSecurityAssetInDataDocument());
                        //if (secAsset.isTerm && (false == secAsset.isZeroCoupon))
                        if (secAsset.IsTerm)
                            //A chaque modif de la date on recalcule le grossAmount car la date rentre dans la formule (voir actualisation)
                            ccic = new CciCompare[] { new CciCompare("grossAmount", CciGrossAmount.Cci(CciPayment.CciEnum.amount), 1) };
                    }
                }
                #endregion
            }

            if (ArrFunc.IsFilled(ccic))
            {
                Array.Sort(ccic);
                ret = ccic[0].key;
            }

            return ret;
        }
    }
}
