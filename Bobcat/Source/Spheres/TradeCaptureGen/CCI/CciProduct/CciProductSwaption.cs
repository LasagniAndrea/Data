#region Using Directives
using System;
using System.Linq;

using System.Collections;
using System.Reflection;

using EFS.GUI.CCI;
using EFS.GUI.Interface;
using EFS.ACommon;
using EFS.Common;
using EFS.Common.Web;

using EfsML.Business;

using FpML.Interface;

#endregion Using Directives

namespace EFS.TradeInformation
{
    /// <summary>
    /// 
    /// </summary>
    public class CciProductSwaption : CciProductBase, IContainerCci, ICciPresentation
    {
        #region Members
        
        private ISwaption _swaption;
        
        private CciPayment[] _cciPremium;
        private CciExercise _cciExercise;
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
            #region exerciseProcedure
            [System.Xml.Serialization.XmlEnumAttribute("exerciseProcedure")]
            exerciseProcedure,
            #endregion exerciseProcedure
            #region calculationAgent
            [System.Xml.Serialization.XmlEnumAttribute("calculationAgent")]
            calculationAgent,
            #endregion calculationAgent
            #region cashSettlement
            [System.Xml.Serialization.XmlEnumAttribute("cashSettlement")]
            cashSettlement,
            #endregion cashSettlement
            #region swaptionStraddle
            swaptionStraddle,
            #endregion swaptionStraddle
            #region underlyingSwap
            underlyingSwap,
            #endregion underlyingSwap
            unknown,
        }
        #endregion Enum

        #region Accessors
        public int PremiumLength
        {
            get { return ArrFunc.IsFilled(_cciPremium) ? _cciPremium.Length : 0; }
        }
        public IReference BuyerPartyReference
        {
            get
            {
                return _swaption.BuyerPartyReference;
            }
        }
        public IReference SellerPartyReference
        {
            get
            {
                return _swaption.SellerPartyReference;
            }
        }
        public IPayment[] Premium
        {
            get
            {
                return _swaption.Premium;
            }
        }
        public TradeCustomCaptureInfos Ccis
        {
            get { return base.CcisBase as TradeCustomCaptureInfos; }
        }
        /// <summary>
        /// 
        /// </summary>
        protected CciTrade CciTrade
        {
            get { return base.CciTradeCommon as CciTrade; }
        }
        #region cciSwap
        public CciProductSwap CciSwap { get; private set; }
        #endregion

        #endregion

        #region constructor
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCciTrade"></param>
        /// <param name="pSwaption"></param>
        /// <param name="pPrefix"></param>
        public CciProductSwaption(CciTrade pCciTrade, ISwaption pSwaption, string pPrefix)
            : this(pCciTrade, pSwaption, pPrefix, -1) { }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCciTrade"></param>
        /// <param name="pSwaption"></param>
        /// <param name="pPrefix"></param>
        /// <param name="pNumber"></param>
        public CciProductSwaption(CciTrade pCciTrade, ISwaption pSwaption, string pPrefix, int pNumber)
            : base((CciTradeCommonBase)pCciTrade, (IProduct)pSwaption, pPrefix, pNumber)
        {
        }
        #endregion

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
            string ret = string.Empty;
            //
            if (StrFunc.IsEmpty(pKey) && IsCciOfContainer(pCci.ClientId_WithoutPrefix))
            {
                if (_cciExercise.IsCciOfContainer(pCci.ClientId_WithoutPrefix))
                {
                    string cliendId_Key_AmericanExercise = _cciExercise.CciContainerKey(pCci.ClientId_WithoutPrefix);
                    CciExercise.CciEnumAmericanExercise enumAmericanExerciseKey = CciExercise.CciEnumAmericanExercise.unknown;
                    //
                    if (System.Enum.IsDefined(typeof(CciExercise.CciEnumAmericanExercise), cliendId_Key_AmericanExercise))
                        enumAmericanExerciseKey = (CciExercise.CciEnumAmericanExercise)System.Enum.Parse(typeof(CciExercise.CciEnumAmericanExercise), cliendId_Key_AmericanExercise);
                    //
                    switch (enumAmericanExerciseKey)
                    {
                        case CciExercise.CciEnumAmericanExercise.commencementDate_adjustableDate_unadjustedDate:
                            pKey = "T";
                            break;
                        case CciExercise.CciEnumAmericanExercise.expirationDate_adjustableDate_unadjustedDate:
                            pKey = "C";
                            break;
                    }
                }
                //
                if (CciSwap.IsCciOfContainer(pCci.ClientId_WithoutPrefix))
                {
                    if (CciSwap.CciStreamGlobal.IsCciOfContainer(pCci.ClientId_WithoutPrefix))
                    {
                        string cliendId_Key_Stream = CciSwap.CciStreamGlobal.CciContainerKey(pCci.ClientId_WithoutPrefix);
                        CciStream.CciEnum enumStreamKey = CciStream.CciEnum.unknown;

                        if (System.Enum.IsDefined(typeof(CciStream.CciEnum), cliendId_Key_Stream))
                            enumStreamKey = (CciStream.CciEnum)System.Enum.Parse(typeof(CciStream.CciEnum), cliendId_Key_Stream);

                        switch (enumStreamKey)
                        {
                            case CciStream.CciEnum.calculationPeriodDates_effectiveDate:
                                pKey = "T";
                                break;
                        }
                    }
                    //
                    if (StrFunc.IsEmpty(pKey))
                        pKey = "E";
                }
            }
            //
            if (StrFunc.IsFilled(pKey))
            {
                switch (pKey.ToUpper())
                {
                    case "C":
                        if (_cciExercise.ExerciseType == CciExercise.ExerciseTypeEnum.american)
                            ret = _cciExercise.Cci(CciExercise.CciEnumAmericanExercise.commencementDate_adjustableDate_unadjustedDate).NewValue;
                        break;
                    case "E":
                        ret = CciSwap.CciSwapStream[0].Cci(CciStream.CciEnum.calculationPeriodDates_effectiveDate).NewValue;
                        break;
                }
            }
            return ret;
        }
        /// <summary>
        /// 
        /// </summary>
        /// //FI 20100416 [16945] récupération du cci de la devise du swap comme devise principale du Swaption 
        public override string CciClientIdMainCurrency
        {
            get
            {
                return CciSwap.CciClientIdMainCurrency;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public override string GetMainCurrency
        {
            get
            {
                return CciSwap.GetMainCurrency;
            }
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

            for (int i = 0; i < PremiumLength; i++)
                _cciPremium[i].SynchronizePayerReceiver(pLastValue, pNewValue);

            CciSwap.SynchronizePayerReceiver(pLastValue, pNewValue);
        }
        #endregion Membres de IContainerCciPayerReceiver

        #region Membres de IContainerCciFactory
        /// <summary>
        /// 
        /// </summary>
        public override void Initialize_FromCci()
        {
            CciTools.CreateInstance(this, _swaption);

            InitializePremium_FromCci();
            InitializeExercise_FromCci();

            if ((null == _swaption.ExerciseProcedure))
                _swaption.ExerciseProcedure = CciTrade.CurrentTrade.Product.ProductBase.CreateExerciseProcedure();

            if (null == _swaption.CashSettlement)
                _swaption.CashSettlement = CciTrade.CurrentTrade.Product.ProductBase.CreateCashSettlement();

            if (null == _swaption.CalculationAgent)
                _swaption.CalculationAgent = CciTrade.CurrentTrade.Product.ProductBase.CreateCalculationAgent();

            if (null == _swaption.Swap)
                _swaption.Swap = CciTrade.CurrentTrade.Product.ProductBase.CreateSwap();

            CciSwap.Initialize_FromCci();
        }
        /// <summary>
        /// 
        /// </summary>
        /// FI 20170116 [21916] Modify (use AddCciSystem Method)
        public override void AddCciSystem()
        {

            CciTools.AddCciSystem(CcisBase, Cst.DDL + CciClientId(CciEnum.buyer), true, TypeData.TypeDataEnum.@string);
            CciTools.AddCciSystem(CcisBase, Cst.DDL + CciClientId(CciEnum.seller), true, TypeData.TypeDataEnum.@string);
            CciTools.AddCciSystem(CcisBase, Cst.DDL + CciClientId(CciEnum.underlyingSwap), true, TypeData.TypeDataEnum.@string);
            Cci(CciEnum.underlyingSwap).IsEnabled = true;

            for (int i = 0; i < PremiumLength; i++)
                _cciPremium[i].AddCciSystem();

            _cciExercise.AddCciSystem();

            CciSwap.AddCciSystem();

            CciTools.AddCciSystem(CcisBase, Cst.BUT + CciClientId(CciEnum.exerciseProcedure), false, TypeData.TypeDataEnum.@string);
            CciTools.AddCciSystem(CcisBase, Cst.BUT + CciClientId(CciEnum.calculationAgent), false, TypeData.TypeDataEnum.@string);
            CciTools.AddCciSystem(CcisBase, Cst.BUT + CciClientId(CciEnum.cashSettlement), false, TypeData.TypeDataEnum.@string);
        }
        /// <summary>
        /// 
        /// </summary>
        public override void Initialize_FromDocument()
        {
            //
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
                        #region swaptionStraddle
                        case CciEnum.swaptionStraddle:
                            data = BoolFunc.IsTrue(_swaption.SwaptionStraddle.Value).ToString().ToLower();
                            break;
                        #endregion swaptionStraddle
                        #region underlyingSwap
                        case CciEnum.underlyingSwap:
                            data = ((IProduct)_swaption.Swap).ProductBase.ProductType.OTCmlId.ToString();
                            break;
                        #endregion underlyingSwap
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

            for (int i = 0; i < PremiumLength; i++)
                _cciPremium[i].Initialize_FromDocument();

            _cciExercise.Initialize_FromDocument();

            CciSwap.Initialize_FromDocument();
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
                        #region swaptionStraddle
                        case CciEnum.swaptionStraddle:
                            _swaption.SwaptionStraddle = new EFS_Boolean(BoolFunc.IsTrue(data));
                            break;
                        #endregion
                        #region underlyingSwap
                        case CciEnum.underlyingSwap:
                            //FI 20120712 chargement de la colonne IDENTIFIER uniquement
                            SQL_Instrument sql_Instrument = new SQL_Instrument(CciTrade.CSCacheOn, Convert.ToInt32(data));
                            if (sql_Instrument.LoadTable(new string[] { "IDI", "IDENTIFIER" }))
                                ((IProduct)_swaption.Swap).ProductBase.SetProductType(sql_Instrument.IdI.ToString(), sql_Instrument.Identifier);
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
            }

            _cciExercise.Dump_ToDocument();

            CciSwap.Dump_ToDocument();

            // 20090608 RD Ticket 16249
            if (Cst.Capture.IsModeInput(CcisBase.CaptureMode) && (false == Cst.Capture.IsModeAction(CcisBase.CaptureMode)))
                Ccis.InitializePaymentPaymentQuoteRelativeTo(CciSwap.CciStreamGlobal, _cciPremium, null, null);

            for (int i = 0; i < PremiumLength; i++)
                _cciPremium[i].Dump_ToDocument();

            _swaption.PremiumSpecified = CciTools.Dump_IsCciContainerArraySpecified(_swaption.PremiumSpecified, _cciPremium);

            _swaption.ExerciseAmericanSpecified = (CciExercise.ExerciseTypeEnum.american == _cciExercise.ExerciseType);
            _swaption.ExerciseBermudaSpecified = (CciExercise.ExerciseTypeEnum.bermuda == _cciExercise.ExerciseType);
            _swaption.ExerciseEuropeanSpecified = (CciExercise.ExerciseTypeEnum.european == _cciExercise.ExerciseType);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCci"></param>
        public override void ProcessInitialize(CustomCaptureInfo pCci)
        {

            if (IsCciOfContainer(pCci.ClientId_WithoutPrefix))
            {
                string clientId_Key = CciContainerKey(pCci.ClientId_WithoutPrefix);

                CciEnum key = CciEnum.unknown;
                if (System.Enum.IsDefined(typeof(CciEnum), clientId_Key))
                    key = (CciEnum)System.Enum.Parse(typeof(CciEnum), clientId_Key);
                //
                switch (key)
                {
                    #region Buyer/Seller
                    case CciEnum.buyer:
                        CcisBase.Synchronize(CciClientIdReceiver, pCci.NewValue, pCci.LastValue);
                        break;
                    case CciEnum.seller:
                        CcisBase.Synchronize(CciClientIdPayer, pCci.NewValue, pCci.LastValue);
                        break;
                    #endregion
                    #region default
                    default:

                        break;
                    #endregion default
                }
            }

            for (int i = 0; i < PremiumLength; i++)
                _cciPremium[i].ProcessInitialize(pCci);

            _cciExercise.ProcessInitialize(pCci);

            CciSwap.ProcessInitialize(pCci);
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

            if (!isOk)
            {
                for (int i = 0; i < PremiumLength; i++)
                {
                    isOk = _cciPremium[i].IsClientId_PayerOrReceiver(pCci);
                    if (isOk)
                        break;
                }
            }

            if (!isOk)
                isOk = CciSwap.IsClientId_PayerOrReceiver(pCci);

            return isOk;

        }
        /// <summary>
        /// 
        /// </summary>
        public override void CleanUp()
        {
            #region Premium
            if (ArrFunc.IsFilled(_cciPremium))
            {
                for (int i = 0; i < _cciPremium.Length; i++)
                    _cciPremium[i].CleanUp();
            }
            //
            if (ArrFunc.IsFilled(_swaption.Premium))
            {
                for (int i = _swaption.Premium.Length - 1; -1 < i; i--)
                {
                    if (false == CaptureTools.IsDocumentElementValid(_swaption.Premium[i].PayerPartyReference))
                        ReflectionTools.RemoveItemInArray(_swaption, "premium", i);
                }
            }
            //
            FieldInfo fld = _swaption.GetType().GetField("premiumSpecified");
            fld.SetValue(_swaption, ArrFunc.IsFilled(_swaption.Premium));
            #endregion Premium

            #region Exercise
            if (_cciExercise != null)
            {
                _cciExercise.CleanUp();
                _swaption.EFS_Exercise = _cciExercise.Exercise;
            }
            //
            //swaption.exerciseAmericanSpecified = (swaption.exerciseAmerican !=null);
            //swaption.exerciseBermudaSpecified  = (swaption.exerciseBermuda != null);
            //swaption.exerciseEuropeanSpecified = (swaption.exerciseEuropean != null);
            //
            _swaption.ExerciseAmericanSpecified = (CciExercise.ExerciseTypeEnum.american == _cciExercise.ExerciseType);
            _swaption.ExerciseBermudaSpecified = (CciExercise.ExerciseTypeEnum.bermuda == _cciExercise.ExerciseType);
            _swaption.ExerciseEuropeanSpecified = (CciExercise.ExerciseTypeEnum.european == _cciExercise.ExerciseType);
            #endregion Exercise

            #region cciSwap
            CciSwap.CleanUp();
            #endregion
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCci"></param>
        public override void SetDisplay(CustomCaptureInfo pCci)
        {
            #region cciPremium
            for (int i = 0; i < PremiumLength; i++)
                _cciPremium[i].SetDisplay(pCci);
            #endregion
            #region cciExercise
            _cciExercise.SetDisplay(pCci);
            #endregion
            #region cciSwap
            CciSwap.SetDisplay(pCci);
            #endregion
        }
        /// <summary>
        /// 
        /// </summary>
        public override void Initialize_Document()
        {

            if (Cst.Capture.IsModeNewCapture(CcisBase.CaptureMode) && (false == CcisBase.IsPreserveData))
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

                for (int i = 0; i < PremiumLength; i++)
                {
                    if ((_cciPremium[i].Cci(CciPayment.CciEnum.payer).IsMandatory) && (_cciPremium[i].Cci(CciPayment.CciEnum.receiver).IsMandatory))
                    {
                        if (StrFunc.IsEmpty(Premium[i].PayerPartyReference.HRef) && StrFunc.IsEmpty(Premium[i].PayerPartyReference.HRef))
                            Premium[i].PayerPartyReference.HRef = BuyerPartyReference.HRef;
                    }
                }

                if (TradeCustomCaptureInfos.PartyUnknown == id)
                    CciTrade.AddPartyUnknown();
                CciSwap.Initialize_Document();
            }
        }

        #endregion Membres de IContainerCciFactory

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

            #region buttons settlementInfo Premium
            if (!isOk)
            {
                for (int i = 0; i < this.PremiumLength; i++)
                {
                    isOk = _cciPremium[i].IsCci(CciPayment.CciEnumPayment.settlementInformation, pCci);
                    if (isOk)
                    {
                        pCo.Element = "settlementInformation";
                        pCo.Object = "premium";
                        pCo.OccurenceValue = i + 1;
                        pIsSpecified = _cciPremium[i].IsSettlementInfoSpecified;
                        pIsEnabled = _cciPremium[i].IsSettlementInstructionSpecified;
                        break;
                    }
                }
            }
            #endregion buttons settlementInfo
            #region button bermuda dates
            if (!isOk)
            {
                isOk = _cciExercise.IsCci(CciExercise.CciEnumBermudaExercise.bermudaExerciseDates_adjustableDates_unadjustedDate, pCci);
                if (isOk)
                {
                    pCo.Object = "";
                    pCo.Element = "bermudaExerciseDates";
                    pIsSpecified = (_swaption.ExerciseBermudaSpecified);
                    pIsEnabled = true;
                }
            }
            //if (!isOk)
            //{
            //    isOk = cciExercise.IsCci(CciExercise.CciEnumExercise.exerciseDates, pCci);
            //    if (isOk)
            //    {
            //        pCo.Element = "bermudaExerciseDates";
            //        pCo.Object = "";
            //        pIsSpecified = (swaption.exerciseBermudaSpecified);
            //        pbEnabled = true;
            //    }
            //}
            #endregion button bermuda dates
            #region button procedure
            if (!isOk)
            {
                isOk = this.IsCci(CciEnum.exerciseProcedure, pCci);
                if (isOk)
                {
                    pCo.Object = "product";
                    pCo.Element = "procedure";
                    pIsSpecified = _swaption.ExerciseProcedureSpecified;
                    pIsEnabled = true;
                }
            }
            #endregion button procedure
            #region button calculationAgent
            if (!isOk)
            {
                isOk = this.IsCci(CciEnum.calculationAgent, pCci);
                if (isOk)
                {
                    pCo.Object = "product";
                    pCo.Element = "calculationAgent";
                    pIsSpecified = _swaption.CalculationAgentSpecified;
                    pIsEnabled = true;
                }
            }
            #endregion button calculationAgent
            #region button cashSettlement
            if (!isOk)
            {
                isOk = this.IsCci(CciEnum.cashSettlement, pCci);
                if (isOk)
                {
                    pCo.Object = "product";
                    pCo.Element = "cashSettlement";
                    pIsSpecified = _swaption.CashSettlementSpecified;
                    pIsEnabled = true;
                }
            }
            #endregion button cashSettlement

            #region buttons of Swap
            if (!isOk)
                isOk = CciSwap.SetButtonZoom(pCci, pCo, ref pIsSpecified, ref pIsEnabled);
            #endregion

            return isOk;
        }
        #endregion Membres de IContainerCciGetInfoButton

        #region methods
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pProduct"></param>
        // EG 20180619 Call SetProduct for swap
        public override void SetProduct(IProduct pProduct)
        {
            _swaption = (ISwaption)pProduct;
            
            ISwap swap = null;
            if (null != _swaption)
                swap = _swaption.Swap;
            CciSwap = new CciProductSwap(CciTrade, swap, Prefix + TradeCustomCaptureInfos.CCst.Prefix_swap);

            CciSwap.SetProduct((IProduct)swap);
            base.SetProduct(pProduct);
        }

        /// <summary>
        /// 
        /// </summary>
        private void InitializePremium_FromCci()
        {

            bool isOk = true;
            int index = -1;
            bool SaveSpecified;
            ArrayList lst = new ArrayList();
            FieldInfo fld = _swaption.GetType().GetField("premiumSpecified");
            SaveSpecified = (bool)fld.GetValue(_swaption);
            //
            lst.Clear();
            while (isOk)
            {
                index += 1;

                CciPayment ccipayment =
                    new CciPayment(CciTrade, index + 1, null, CciPayment.PaymentTypeEnum.Payment, Prefix + TradeCustomCaptureInfos.CCst.Prefix_swaptionPremium, string.Empty, CciClientIdReceiver, string.Empty, string.Empty, string.Empty);

                isOk = CcisBase.Contains(ccipayment.CciClientId(CciPayment.CciEnumPayment.payer));
                if (isOk)
                {
                    if (ArrFunc.IsEmpty(_swaption.Premium) || (index == _swaption.Premium.Length))
                        ReflectionTools.AddItemInArray(_swaption, "premium", index);

                    ccipayment.Payment = _swaption.Premium[index];

                    lst.Add(ccipayment);
                }
            }

            _cciPremium = null;
            _cciPremium = (CciPayment[])lst.ToArray(typeof(CciPayment));
            for (int i = 0; i < this.PremiumLength; i++)
                _cciPremium[i].Initialize_FromCci();

            fld = _swaption.GetType().GetField("premiumSpecified");
            fld.SetValue(_swaption, SaveSpecified);

        }

        /// <summary>
        /// 
        /// </summary>
        private void InitializeExercise_FromCci()
        {

            bool saveAmericanSpecified = _swaption.ExerciseAmericanSpecified;
            bool saveBermudaSpecified = _swaption.ExerciseBermudaSpecified;
            bool saveEuropeanSpecified = _swaption.ExerciseEuropeanSpecified;
            //
            foreach (string exerciseType in Enum.GetNames(typeof(CciExercise.ExerciseTypeEnum)))
            {
                _cciExercise = new CciExercise(CciTrade, Prefix + exerciseType + "Exercise");
                //
                if (((exerciseType == CciExercise.ExerciseTypeEnum.american.ToString()) &&
                    CcisBase.Contains(_cciExercise.CciClientId(CciExercise.CciEnumExercise.expirationTime_hourMinuteTime))) ||
                    ((exerciseType == CciExercise.ExerciseTypeEnum.bermuda.ToString()) &&
                    CcisBase.Contains(_cciExercise.CciClientId(CciExercise.CciEnumExercise.expirationTime_hourMinuteTime))) ||
                    ((exerciseType == CciExercise.ExerciseTypeEnum.european.ToString()) &&
                    CcisBase.Contains(_cciExercise.CciClientId(CciExercise.CciEnumExercise.expirationTime_hourMinuteTime))))
                {
                    _cciExercise.ExerciseType = (CciExercise.ExerciseTypeEnum)Enum.Parse(typeof(CciExercise.ExerciseTypeEnum), exerciseType);
                    _cciExercise.SetExercise(_swaption as IProduct);
                    break;
                }
            }
            _cciExercise.Initialize_FromCci();
            _swaption.ExerciseAmericanSpecified = saveAmericanSpecified;
            _swaption.ExerciseBermudaSpecified = saveBermudaSpecified;
            _swaption.ExerciseEuropeanSpecified = saveEuropeanSpecified;
        }
        #endregion

        #region ICciPresentation Membres
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pPage"></param>
        public override void DumpSpecific_ToGUI(CciPageBase pPage)
        {
            if (null != CciSwap)
                CciSwap.DumpSpecific_ToGUI(pPage);
        }
        #endregion
    }
}
