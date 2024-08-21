#region Using Directives
using EFS.ACommon;
using EFS.Common;
using EFS.GUI.CCI;
using EfsML.Business;
using FpML.Enum;
using FpML.Interface;
using System;
using System.Linq;
#endregion Using Directives

namespace EFS.TradeInformation
{
    /// <summary>
    /// 
    /// </summary>
    public class CciProductFra : CciProductBase
    {
        #region Members
        
        private IFra _fra;
        #endregion Members

        #region Enum
        public enum CciEnum
        {
            [System.Xml.Serialization.XmlEnumAttribute("buyerPartyReference")]
            buyer,
            [System.Xml.Serialization.XmlEnumAttribute("sellerPartyReference")]
            seller,
            [System.Xml.Serialization.XmlEnumAttribute("adjustedEffectiveDate")]
            adjustedEffectiveDate,
            [System.Xml.Serialization.XmlEnumAttribute("adjustedTerminationDate")]
            adjustedTerminationDate,
            [System.Xml.Serialization.XmlEnumAttribute("notional.amount")]
            notional_amount,
            [System.Xml.Serialization.XmlEnumAttribute("notional.currency")]
            notional_currency,
            [System.Xml.Serialization.XmlEnumAttribute("dayCountFraction")]
            dayCountFraction,
            [System.Xml.Serialization.XmlEnumAttribute("fixedRate")]
            fixedRate,
            [System.Xml.Serialization.XmlEnumAttribute("floatingRateIndex")]
            floatingRateIndex,
            [System.Xml.Serialization.XmlEnumAttribute("paymentDate.dateAdjustments.businessDayConvention")]
            paymentDate_dateAdjustments_bDC,
            unknown,
        }
        #endregion Enum

        #region properties
        #region ccis
        public TradeCustomCaptureInfos Ccis
        {
            get { return base.CcisBase as TradeCustomCaptureInfos; }
        }

        protected CciTrade CciTrade
        {
            get { return base.CciTradeCommon as CciTrade; }
        }

        #endregion
        #endregion

        #region Constructors
        public CciProductFra(CciTrade pCciTrade, IFra pFra, string pPrefix)
            : this(pCciTrade, pFra, pPrefix, -1)
        { }
        public CciProductFra(CciTrade pCciTrade, IFra pFra, string pPrefix, int pNumber)
            : base((CciTradeCommonBase)pCciTrade, (IProduct)pFra, pPrefix, pNumber)
        {
        }
        #endregion Constructors

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
        /// Return the main currency for a product
        /// </summary>
        /// <returns></returns>
        public override string GetMainCurrency
        {
            get
            {
                return _fra.Notional.Currency;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public override string CciClientIdMainCurrency
        {
            get { return CciClientId(CciEnum.notional_currency); }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pKey"></param>
        /// <param name="pCci"></param>
        /// <returns></returns>
        public override string GetData(string pKey, CustomCaptureInfo pCci)
        {

            string ret = string.Empty;
            //
            if (StrFunc.IsEmpty(pKey) && IsCciOfContainer(pCci.ClientId_WithoutPrefix))
            {
                string cliendId_Key = CciContainerKey(pCci.ClientId_WithoutPrefix);
                CciEnum enumKey = CciEnum.unknown;
                //
                if (System.Enum.IsDefined(typeof(CciEnum), cliendId_Key))
                    enumKey = (CciEnum)System.Enum.Parse(typeof(CciEnum), cliendId_Key);
                //  
                switch (enumKey)
                {
                    case CciEnum.adjustedEffectiveDate:
                        pKey = "T";
                        break;
                }
                //
                if (StrFunc.IsEmpty(pKey))
                    pKey = "E";
            }
            //
            if (StrFunc.IsFilled(pKey))
            {
                switch (pKey.ToUpper())
                {
                    case "E":
                        ret = Cci(CciEnum.adjustedEffectiveDate).NewValue;
                        break;
                }
            }
            //
            return ret;

        }

        #endregion  Membres de ITradeCci

        #region Membres de IContainerCciPayerReceiver
        /// <summary>
        /// 
        /// </summary>
        public override string CciClientIdPayer
        {
            get { return CciClientId(CciEnum.buyer.ToString()); }
        }

        /// <summary>
        /// 
        /// </summary>
        public override string CciClientIdReceiver
        {
            get { return CciClientId(CciEnum.seller.ToString()); }
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

        }

        #endregion Membres de IContainerCciPayerReceiver

        #region Membres de IContainerCciFactory
        /// <summary>
        /// 
        /// </summary>
        public override void Initialize_FromCci()
        {
            CciTools.CreateInstance((IContainerCci)this, _fra);
            //
            if (null == _fra.IndexTenor)
            {
                _fra.IndexTenor = CciTrade.CurrentTrade.Product.ProductBase.CreateIntervals();
                _fra.FirstIndexTenor = CciTrade.CurrentTrade.Product.ProductBase.CreateInterval(PeriodEnum.D, 0);
            }

        }
        /// <summary>
        /// 
        /// </summary>
        /// FI 20170116 [21916] Modify (use AddCciSystem Method)
        public override void AddCciSystem()
        {
            CciTools.AddCciSystem(CcisBase, Cst.DDL + CciClientId(CciEnum.buyer.ToString()), true, TypeData.TypeDataEnum.@string);
            CciTools.AddCciSystem(CcisBase, Cst.DDL + CciClientId(CciEnum.seller.ToString()), true, TypeData.TypeDataEnum.@string);
        }
        /// <summary>
        /// 
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
                        #region Buyer
                        case CciEnum.buyer:
                            data = _fra.BuyerPartyReference.HRef;
                            break;
                        #endregion Buyer
                        #region Seller
                        case CciEnum.seller:
                            data = _fra.SellerPartyReference.HRef;
                            break;
                        #endregion Seller
                        #region EffectiveDate
                        case CciEnum.adjustedEffectiveDate:
                            data = _fra.AdjustedEffectiveDate.Value;
                            break;
                        #endregion EffectiveDate
                        #region TerminationDate
                        case CciEnum.adjustedTerminationDate:
                            data = _fra.AdjustedTerminationDate.Value;
                            break;
                        #endregion TerminationDate
                        #region NotionalStepScheduleInitialValue
                        case CciEnum.notional_amount:
                            data = _fra.Notional.Amount.Value;
                            break;
                        #endregion NotionalStepScheduleInitialValue
                        #region Currency
                        case CciEnum.notional_currency:
                            data = _fra.Notional.Currency;
                            break;
                        #endregion Currency
                        #region DayCountFraction
                        case CciEnum.dayCountFraction:
                            data = _fra.DayCountFraction.ToString();
                            break;
                        #endregion
                        #region FixedRate
                        case CciEnum.fixedRate:
                            data = _fra.FixedRate.Value;
                            break;
                        #endregion FixedRate
                        #region FloatingRate
                        case CciEnum.floatingRateIndex:
                            try
                            {
                                int idAsset = _fra.FloatingRateIndex.OTCmlId;
                                if (idAsset > 0)
                                {
                                    SQL_AssetRateIndex sql_RateIndex = new SQL_AssetRateIndex(CciTrade.CSCacheOn, SQL_AssetRateIndex.IDType.IDASSET, idAsset);
                                    if (sql_RateIndex.IsLoaded)
                                    {
                                        cci.Sql_Table = sql_RateIndex;
                                        data = sql_RateIndex.Identifier;
                                    }
                                }
                            }
                            catch
                            {
                                cci.Sql_Table = null;
                                data = string.Empty;
                            }
                            break;
                        #endregion FloatingRate

                        #region PaymentDateBusinessDayConvention
                        case CciEnum.paymentDate_dateAdjustments_bDC:
                            data = _fra.PaymentDate.DateAdjustments.BusinessDayConvention.ToString();
                            //Glop lire les BCs pour les afficher en ToolTip du Label associé
                            break;
                        #endregion PaymentBusinessDayConvention

                        #region default
                        default:
                            isSetting = false;
                            break;
                        #endregion
                    }
                    //
                    if (isSetting)
                        CcisBase.InitializeCci(cci, sql_Table, data);
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
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
                            _fra.BuyerPartyReference.HRef = data;
                            processQueue = CustomCaptureInfosBase.ProcessQueueEnum.Low;//Afin de recalculer les BCs
                            break;
                        #endregion Buyer
                        #region Seller
                        case CciEnum.seller:
                            _fra.SellerPartyReference.HRef = data;
                            processQueue = CustomCaptureInfosBase.ProcessQueueEnum.Low;//Afin de recalculer les BCs
                            break;
                        #endregion Seller
                        #region EffectiveDate
                        case CciEnum.adjustedEffectiveDate:
                            _fra.AdjustedEffectiveDate.Value = data;
                            if (StrFunc.IsEmpty(_fra.AdjustedEffectiveDate.Id))
                                _fra.AdjustedEffectiveDate.Id = TradeCustomCaptureInfos.CCst.EFFECTIVEDATE_REFERENCE;
                            processQueue = CustomCaptureInfosBase.ProcessQueueEnum.Low;//Afin de recalculer le nbre de jour
                            break;
                        #endregion EffectiveDate
                        #region TerminationDate
                        case CciEnum.adjustedTerminationDate:
                            _fra.AdjustedTerminationDate.Value = data;
                            processQueue = CustomCaptureInfosBase.ProcessQueueEnum.Low;//Afin de recalculer le nbre de jour
                            break;
                        #endregion TerminationDate
                        #region Notional
                        case CciEnum.notional_amount:
                            _fra.Notional.Amount.Value = data;
                            if (StrFunc.IsEmpty(_fra.Notional.Id))
                                _fra.Notional.Id = TradeCustomCaptureInfos.CCst.NOTIONAL_REFERENCE;
                            processQueue = CustomCaptureInfosBase.ProcessQueueEnum.Low;//Afin d'arrondir NotionalStepScheduleInitialValue
                            break;
                        #endregion Notional
                        #region Currency
                        case CciEnum.notional_currency:
                            _fra.Notional.Currency = data;
                            processQueue = CustomCaptureInfosBase.ProcessQueueEnum.Low;//Afin d'arrondir NotionalStepScheduleInitialValue
                            break;
                        #endregion Currency
                        #region DayCountFraction
                        case CciEnum.dayCountFraction:
                            DayCountFractionEnum dcfEnum = (DayCountFractionEnum)System.Enum.Parse(typeof(DayCountFractionEnum), data, true);
                            _fra.DayCountFraction = dcfEnum;
                            processQueue = CustomCaptureInfosBase.ProcessQueueEnum.Low;//Afin de recalculer le nbre de jour
                            break;
                        #endregion DayCountFraction
                        #region FixedRate
                        case CciEnum.fixedRate:
                            _fra.FixedRate.Value = data;
                            break;
                        #endregion FixedRate
                        #region FloatingRateIndex
                        case CciEnum.floatingRateIndex:
                            Ccis.Dump_FloatingRateIndex_ToDocument(CciClientId(CciEnum.floatingRateIndex), _fra, _fra.FloatingRateIndex, _fra.FirstIndexTenor);
                            //							
                            if (null != cci.Sql_Table)  // if IsFound
                            {
                                SQL_AssetRateIndex sql_RateIndex = (SQL_AssetRateIndex)cci.Sql_Table;
                                Ccis.DumpRelativeDateOffset_ToDocument(_fra.FixingDateOffset, sql_RateIndex, _fra.AdjustedEffectiveDate.Id, null);
                            }
                            break;
                        #endregion FloatingRateIndex
                        #region PaymentDateBusinessDayConvention
                        case CciEnum.paymentDate_dateAdjustments_bDC:
                            DumpPaymentBDA();
                            break;
                        #endregion PaymentBusinessDayConvention
                        #region default
                        default:
                            isSetting = false;
                            break;
                        #endregion default
                    }
                    //
                    if (isSetting)
                        CcisBase.Finalize(cci.ClientId_WithoutPrefix, processQueue);
                }
            }

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCci"></param>
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
                    #region Buyer/Seller: Calcul des BCs
                    case CciEnum.buyer:
                        CcisBase.Synchronize(CciClientIdReceiver, pCci.NewValue, pCci.LastValue);
                        DumpPaymentBDA();
                        break;
                    case CciEnum.seller:
                        CcisBase.Synchronize(CciClientIdPayer, pCci.NewValue, pCci.LastValue);
                        DumpPaymentBDA();
                        break;
                    #endregion
                    #region EffectiveDate, TerminationDate, DayCountFraction: Calcul de CalculationPeriodNumberOfDays et de paymentDate
                    case CciEnum.adjustedEffectiveDate:
                    case CciEnum.adjustedTerminationDate:
                    case CciEnum.dayCountFraction:
                        try
                        {
                            IInterval interval = ((IProduct)_fra).ProductBase.CreateInterval(PeriodEnum.D, 0);
                            EFS_DayCountFraction dcf = new EFS_DayCountFraction(_fra.AdjustedEffectiveDate.DateValue, _fra.AdjustedTerminationDate.DateValue, _fra.DayCountFraction, interval);
                            _fra.CalculationPeriodNumberOfDays.Value = dcf.TotalNumberOfCalculatedDays.ToString();
                        }
                        catch (Exception) { _fra.CalculationPeriodNumberOfDays.Value = 0.ToString(); }

                        #region EffectiveDate: Calcul de paymentDate
                        if (CciEnum.adjustedEffectiveDate == key)
                            _fra.PaymentDate.UnadjustedDate.Value = _fra.AdjustedEffectiveDate.Value;
                        #endregion
                        break;
                    #endregion
                    #region FloatingRateIndex
                    case CciEnum.floatingRateIndex:
                        Ccis.ProcessInitialize_DCF(CciClientId(CciEnum.dayCountFraction), CciClientId(CciEnum.floatingRateIndex));
                    #endregion
                        break;
                    #region Currency: Arrondi du notional et Calcul des BCs
                    case CciEnum.notional_amount:
                    case CciEnum.notional_currency:
                        Ccis.ProcessInitialize_AroundAmount(CciClientId(CciEnum.notional_amount), _fra.Notional, (CciEnum.notional_amount == key));
                        if (CciEnum.notional_currency == key)
                            DumpPaymentBDA();
                        break;
                    #endregion
                    default:
                        //System.Diagnostics.Debug.WriteLine("PROCESSS NON GERE: " + pCci.ClientId_WithoutPrefix);
                        break;
                }
            }
            //

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
            return isOk;

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCci"></param>
        public override void SetDisplay(CustomCaptureInfo pCci)
        {

            if (IsCci(CciEnum.paymentDate_dateAdjustments_bDC, pCci))
                Ccis.SetDisplayBusinessDayAdjustments(pCci, _fra.PaymentDate.DateAdjustments);

        }
        /// <summary>
        /// 
        /// </summary>
        public override void Initialize_Document()
        {

            if (Cst.Capture.IsModeNew(CcisBase.CaptureMode) && (false == CcisBase.IsPreserveData))
            {
                string id = string.Empty;
                //
                if (StrFunc.IsEmpty(_fra.BuyerPartyReference.HRef) &&
                    StrFunc.IsEmpty(_fra.SellerPartyReference.HRef))
                {
                    //20080523 FI Mise en commentaire, s'il n'y a pas partie il mettre unknown 
                    //HPC est broker ds les template et ne veut pas être 1 contrepartie
                    //id = GetIdFirstPartyCounterparty();
                    id = StrFunc.IsFilled(id) ? id : TradeCustomCaptureInfos.PartyUnknown;
                    _fra.BuyerPartyReference.HRef = id;
                }
                //
                if (TradeCustomCaptureInfos.PartyUnknown == id)
                    CciTrade.AddPartyUnknown();
            }

        }
        #endregion Membres de IContainerCciFactory

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pProduct"></param>
        public override void SetProduct(IProduct pProduct)
        {

            _fra = (IFra)pProduct;
            base.SetProduct(pProduct);

        }

        /// <summary>
        /// 
        /// </summary>
        private void DumpPaymentBDA()
        {
            CciBC cciBC = new CciBC(CciTrade)
            {
                { CciClientIdPayer, CciBC.TypeReferentialInfo.Actor },
                { CciClientIdReceiver, CciBC.TypeReferentialInfo.Actor },
                { CciClientId(CciEnum.notional_currency), CciBC.TypeReferentialInfo.Currency }
            };
            Ccis.DumpBDC_ToDocument(_fra.PaymentDate.DateAdjustments, CciClientId(CciEnum.paymentDate_dateAdjustments_bDC),
                TradeCustomCaptureInfos.CCst.PERIOD_BUSINESS_CENTERS_REFERENCE, cciBC);
        }
    }
}
