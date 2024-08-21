#region Using Directives
using EFS.ACommon;
using EFS.Common.Web;
using EFS.GUI.CCI;
using EfsML.Business;
using FpML.Interface;
using System.Collections;
#endregion Using Directives

namespace EFS.TradeInformation
{

    /// <summary>
    /// Description résumée de CciProductCapFloor.
    /// </summary>
    public class CciProductCapFloor : CciProductBase
    {
        #region Enums
        #region CciEnum
        public enum CciEnum
        {
            //
            [System.Xml.Serialization.XmlEnumAttribute("earlyTerminationProvision")]
            earlyTerminationProvision,
            unknown,
        }
        #endregion CciEnum
        #endregion Enums

        #region Members
        
        private CapFloorContainer _capFloor;

        private CciStream _cciStream;
        private CciPayment[] _cciPremium;
        private CciEarlyTerminationProvision _cciEarlyTerminationProvision;
        #endregion

        #region Accessors
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
        public int AdditionalPaymentsLength
        {
            get { return ArrFunc.IsFilled(CciAdditionalPayment) ? CciAdditionalPayment.Length : 0; }
        }
        /// <summary>
        /// 
        /// </summary>
        public int PremiumsLength
        {
            get { return ArrFunc.IsFilled(_cciPremium) ? _cciPremium.Length : 0; }
        }
        
        /// <summary>
        /// 
        /// </summary>
        public CciPayment[] CciAdditionalPayment
        {
            get;
            private set;
        }
        #endregion Accessors

        #region Constructors
        public CciProductCapFloor(CciTrade pCciTrade, ICapFloor pCapFloor, string pPrefix)
            : this(pCciTrade, pCapFloor, pPrefix, -1)
        { }
        public CciProductCapFloor(CciTrade pCciTrade, ICapFloor pCapFloor, string pPrefix, int pNumber)
            : base((CciTradeCommonBase)pCciTrade, (IProduct)pCapFloor, pPrefix, pNumber)
        {
        }
        #endregion Constructors

        #region Interfaces
        #region IContainerCciPayerReceiver Members
        #region CciClientIdPayer
        public override string CciClientIdPayer
        {
            get { return _cciStream.CciClientIdPayer; }
        }
        #endregion CciClientIdPayer
        #region CciClientIdReceiver
        public override string CciClientIdReceiver
        {
            get { return _cciStream.CciClientIdReceiver; }
        }
        #endregion CciClientIdReceiver
        #region SynchronizePayerReceiver
        public override void SynchronizePayerReceiver(string pLastValue, string pNewValue)
        {
            _cciStream.SynchronizePayerReceiver(pLastValue, pNewValue);
            //
            for (int i = 0; i < PremiumsLength; i++)
                _cciPremium[i].SynchronizePayerReceiver(pLastValue, pNewValue);
            //
            for (int i = 0; i < AdditionalPaymentsLength; i++)
                CciAdditionalPayment[i].SynchronizePayerReceiver(pLastValue, pNewValue);
            //

        }
        #endregion SynchronizePayerReceiver
        #endregion IContainerCciPayerReceiver Members
        
        #region ITradeCci Members
        #region CciClientIdMainCurrency
        public override string CciClientIdMainCurrency
        {
            get { return _cciStream.CciClientId(CciStream.CciEnum.calculationPeriodAmount_calculation_notionalSchedule_notionalStepSchedule_currency); }
        }
        #endregion CciClientIdMainCurrency
        #region GetMainCurrency
        public override string GetMainCurrency
        {
            get
            {
                string ret = string.Empty;

                if (_capFloor.Stream.CalculationPeriodAmount.CalculationSpecified)
                    ret = _capFloor.Stream.CalculationPeriodAmount.Calculation.Notional.StepSchedule.Currency.Value;
                else if (_capFloor.Stream.CalculationPeriodAmount.KnownAmountScheduleSpecified)
                    ret = _capFloor.Stream.CalculationPeriodAmount.KnownAmountSchedule.Currency.Value;

                return ret;

            }
        }
        #endregion GetMainCurrency
        #region RetSidePayer
        public override string RetSidePayer
        {
            get
            {
                string ret = SideTools.RetBuySide();
                if (_capFloor.Stream.CalculationPeriodAmount.CalculationSpecified)
                {
                    ICalculation calculation = _capFloor.Stream.CalculationPeriodAmount.Calculation;
                    if (calculation.RateFloatingRateSpecified)
                    {
                        //=========================================
                        //20090825 PL TRIM 16494 NewModelCapFloor
                        //=========================================
                        //Cap
                        //if (isCap)
                        //    ret = SideTools.RetBuySide();
                        ////Floor
                        //else if (isFloor)
                        //    ret = SideTools.RetSellSide();
                        ////Collar
                        //else
                        //    ret = SideTools.RetBuySide();
                        ret = SideTools.RetSellSide();
                        //=========================================
                    }
                }
                return ret;

            }
        }
        #endregion RetSidePayer
        #region RetSideReceiver
        public override string RetSideReceiver
        {
            get { return SideTools.RetReverseSide(this.RetSidePayer); }
        }
        #endregion RetSideReceiver
        #endregion
        
        #region IContainerCciFactory members
        #region AddCciSystem
        public override void AddCciSystem()
        {
            _cciStream.AddCciSystem();

            for (int i = 0; i < PremiumsLength; i++)
                _cciPremium[i].AddCciSystem();

            // RD 20200511 [25326] Add
            _cciEarlyTerminationProvision.AddCciSystem();

            for (int i = 0; i < AdditionalPaymentsLength; i++)
                CciAdditionalPayment[i].AddCciSystem();
        }
        #endregion AddCciSystem
        #region CleanUp
        public override void CleanUp()
        {
            //
            _cciStream.CleanUp();
            //
            #region premium
            if (ArrFunc.IsFilled(_cciPremium))
            {
                for (int i = 0; i < _cciPremium.Length; i++)
                    _cciPremium[i].CleanUp();
            }
            //
            if (ArrFunc.IsFilled(_capFloor.Premium))
            {
                for (int i = _capFloor.Premium.Length - 1; -1 < i; i--)
                {
                    if (false == CaptureTools.IsDocumentElementValid(_capFloor.Premium[i].PayerPartyReference.HRef))
                        ReflectionTools.RemoveItemInArray(_capFloor.CapFloor, "premium", i);
                }
            }
            _capFloor.PremiumSpecified = (ArrFunc.IsFilled(_capFloor.Premium));
            #endregion premium
            //
            #region additionalPayment
            if (ArrFunc.IsFilled(CciAdditionalPayment))
            {
                for (int i = 0; i < CciAdditionalPayment.Length; i++)
                    CciAdditionalPayment[i].CleanUp();
            }
            //
            if (ArrFunc.IsFilled(_capFloor.AdditionalPayment))
            {
                for (int i = _capFloor.AdditionalPayment.Length - 1; -1 < i; i--)
                {
                    if (false == CaptureTools.IsDocumentElementValid(_capFloor.AdditionalPayment[i].PayerPartyReference.HRef))
                        ReflectionTools.RemoveItemInArray(_capFloor.CapFloor, "additionalPayment", i);
                }
            }
            _capFloor.AdditionalPaymentSpecified = ArrFunc.IsFilled(_capFloor.AdditionalPayment);
            #endregion additionalPayment
            //

        }
        #endregion CleanUp
        #region Dump_ToDocument
        public override void Dump_ToDocument()
        {
            _cciStream.Dump_ToDocument();
            //
            if (Cst.Capture.IsModeInput(CcisBase.CaptureMode) && (false == Cst.Capture.IsModeAction(CcisBase.CaptureMode)))
                Ccis.InitializePaymentPaymentQuoteRelativeTo(_cciStream, _cciPremium, CciAdditionalPayment, CciTrade.cciOtherPartyPayment);
            //
            //Premium
            for (int i = 0; i < PremiumsLength; i++)
                _cciPremium[i].Dump_ToDocument();
            _capFloor.PremiumSpecified = CciTools.Dump_IsCciContainerArraySpecified(_capFloor.PremiumSpecified, _cciPremium);
            //
            //additionalPayment
            for (int i = 0; i < AdditionalPaymentsLength; i++)
                CciAdditionalPayment[i].Dump_ToDocument();
            _capFloor.AdditionalPaymentSpecified = CciTools.Dump_IsCciContainerArraySpecified(_capFloor.AdditionalPaymentSpecified, CciAdditionalPayment);

            //int toto = CciTools.GetArrayElementCount(_cciAdditionalPayment[AdditionalPaymentsLength-1].CciClientId(CciPayment.CciEnum.payer),_cciTrade.DataDocument.dataDocument   );    
            //			
            if (null != _cciEarlyTerminationProvision)
                _cciEarlyTerminationProvision.Dump_ToDocument();
        }
        #endregion Dump_ToDocument
        #region Initialize_Document
        public override void Initialize_Document()
        {

            if (Cst.Capture.IsModeNew(CcisBase.CaptureMode) && (false == CcisBase.IsPreserveData))
            {
                string id = string.Empty;
                //
                if (StrFunc.IsEmpty(_capFloor.Stream.PayerPartyReference.HRef) &&
                     StrFunc.IsEmpty(_capFloor.Stream.ReceiverPartyReference.HRef))
                {
                    //20080523 FI Mise en commentaire, s'il n'y a pas partie il mettre unknown 
                    //HPC est broker ds les template et ne veut pas être 1 contrepartie
                    //id = GetIdFirstPartyCounterparty();
                    id = StrFunc.IsFilled(id) ? id : TradeCustomCaptureInfos.PartyUnknown;
                    //
                    if (_capFloor.IsFloor())
                        _capFloor.Stream.ReceiverPartyReference.HRef = id;
                    else
                        _capFloor.Stream.PayerPartyReference.HRef = id;
                }
                //
                for (int i = 0; i < PremiumsLength; i++)
                {
                    if ((_cciPremium[i].Cci(CciPayment.CciEnum.payer).IsMandatory) && (_cciPremium[i].Cci(CciPayment.CciEnum.receiver).IsMandatory))
                    {
                        if (StrFunc.IsEmpty(_capFloor.Premium[i].PayerPartyReference.HRef) &&
                             StrFunc.IsEmpty(_capFloor.Premium[i].ReceiverPartyReference.HRef))
                        {
                            if (_capFloor.IsFloor())
                                _capFloor.Premium[i].PayerPartyReference.HRef = _capFloor.Stream.ReceiverPartyReference.HRef;
                            else
                                _capFloor.Premium[i].PayerPartyReference.HRef = _capFloor.Stream.PayerPartyReference.HRef;
                        }
                    }
                }
                //
                for (int i = 0; i < AdditionalPaymentsLength; i++)
                {
                    if ((CciAdditionalPayment[i].Cci(CciPayment.CciEnum.payer).IsMandatory) && (CciAdditionalPayment[i].Cci(CciPayment.CciEnum.receiver).IsMandatory))
                    {
                        if (StrFunc.IsEmpty(_capFloor.AdditionalPayment[i].PayerPartyReference.HRef) &&
                             StrFunc.IsEmpty(_capFloor.AdditionalPayment[i].ReceiverPartyReference.HRef))
                        {
                            if (_capFloor.IsFloor())
                                _capFloor.AdditionalPayment[i].PayerPartyReference.HRef = _capFloor.Stream.ReceiverPartyReference.HRef;
                            else
                                _capFloor.AdditionalPayment[i].PayerPartyReference.HRef = _capFloor.Stream.PayerPartyReference.HRef;
                        }
                    }
                }
                //
                if (TradeCustomCaptureInfos.PartyUnknown == id)
                    CciTrade.AddPartyUnknown();
            }

        }
        #endregion Initialize_Document
        #region Initialize_FromCci
        public override void Initialize_FromCci()
        {
            InitializePayment_FromCci(true);
            InitializePayment_FromCci(false);
            _cciStream.Initialize_FromCci();
            InitializeProvision_FromCci();

        }
        #endregion Initialize_FromCci
        #region Initialize_FromDocument
        public override void Initialize_FromDocument()
        {

            _cciStream.Initialize_FromDocument();
            //
            for (int i = 0; i < PremiumsLength; i++)
                _cciPremium[i].Initialize_FromDocument();
            //
            for (int i = 0; i < AdditionalPaymentsLength; i++)
                CciAdditionalPayment[i].Initialize_FromDocument();
            //
            if (null != _cciEarlyTerminationProvision)
                _cciEarlyTerminationProvision.Initialize_FromDocument();

        }
        #endregion Initialize_FromDocument
        #region IsClientId_PayerOrReceiver
        public override bool IsClientId_PayerOrReceiver(CustomCaptureInfo pCci)
        {

            bool isOk = false;
            //
            if (false == isOk)
                isOk = _cciStream.IsClientId_PayerOrReceiver(pCci);
            //
            if (false == isOk)
            {
                for (int i = 0; i < PremiumsLength; i++)
                {
                    isOk = _cciPremium[i].IsClientId_PayerOrReceiver(pCci);
                    if (isOk)
                        break;
                }
            }
            //
            if (false == isOk)
            {
                for (int i = 0; i < AdditionalPaymentsLength; i++)
                {
                    isOk = CciAdditionalPayment[i].IsClientId_PayerOrReceiver(pCci);
                    if (isOk)
                        break;
                }
            }
            //
            if (false == isOk)
            {
                if (null != _cciEarlyTerminationProvision)
                    isOk = _cciEarlyTerminationProvision.IsClientId_PayerOrReceiver(pCci);
            }

            return isOk;

        }
        #endregion
        #region ProcessInitialize
        public override void ProcessInitialize(CustomCaptureInfo pCci)
        {
            _cciStream.ProcessInitialize(pCci);
            //
            // Premium
            for (int i = 0; i < PremiumsLength; i++)
                _cciPremium[i].ProcessInitialize(pCci);
            //
            // Additional 
            for (int i = 0; i < AdditionalPaymentsLength; i++)
                CciAdditionalPayment[i].ProcessInitialize(pCci);
            //
            // EarlyTerminationProvision
            if (null != _cciEarlyTerminationProvision)
                _cciEarlyTerminationProvision.ProcessInitialize(pCci);


        }
        #endregion ProcessInitialize
        #region SetDisplay
        public override void SetDisplay(CustomCaptureInfo pCci)
        {

            _cciStream.SetDisplay(pCci);
            //
            #region premium

            for (int i = 0; i < PremiumsLength; i++)
                _cciPremium[i].SetDisplay(pCci);

            #endregion
            //
            #region additionalPayment

            for (int i = 0; i < AdditionalPaymentsLength; i++)
                CciAdditionalPayment[i].SetDisplay(pCci);

            #endregion
            //
            if (null != _cciEarlyTerminationProvision)
                _cciEarlyTerminationProvision.SetDisplay(pCci);
            //

        }
        #endregion SetDisplay
        #endregion IContainerCciFactory members
        
        #region ITradeGetInfoButton Members
        #region SetButtonZoom
        public override bool SetButtonZoom(CustomCaptureInfo pCci, CustomObjectButtonFpmlObject pCo, ref bool pIsSpecified, ref bool pIsEnabled)
        {

            bool isOk = false;
            #region Stream
            if (_cciStream.IsCciOfContainer(pCci.ClientId_WithoutPrefix))
            {
                string key = _cciStream.CciContainerKey(pCci.ClientId_WithoutPrefix);
                CciStream.CciEnum elt = CciStream.CciEnum.unknown;
                //
                if (System.Enum.IsDefined(typeof(CciStream.CciEnum), _cciStream.CciContainerKey(pCci.ClientId_WithoutPrefix)))
                    elt = (CciStream.CciEnum)System.Enum.Parse(typeof(CciStream.CciEnum), key);
                //
                isOk = true;
                switch (elt)
                {
                    case CciStream.CciEnum.calculationPeriodAmount_calculation_notionalSchedule_notionalStepSchedule_initialValue:
                        pCo.Element = "notionalStepSchedule";
                        pCo.OccurenceValue = 1;
                        pIsSpecified = _cciStream.IsNotionalStepScheduleStepSpecified;
                        pIsEnabled = true;
                        break;
                    case CciStream.CciEnum.calculationPeriodAmount_calculation_notionalSchedule_notionalStepParameters:
                        pCo.Object = "calculationNotional";
                        pCo.Element = "notionalStepParameters";
                        pCo.OccurenceValue = 1;
                        pIsSpecified = _cciStream.IsNotionalStepParametersSpecified;
                        pIsEnabled = true;
                        break;
                    case CciStream.CciEnum.calculationPeriodAmount_knownAmountSchedule_initialValue:
                        pCo.Element = "calculationPeriodAmountKnownAmountSchedule";
                        pCo.OccurenceValue = 1;
                        pIsSpecified = _cciStream.IsKnownAmountScheduleStepSpecified;
                        pIsEnabled = true;
                        break;
                    case CciStream.CciEnum.calculationPeriodAmount_calculation_floatingRateCalculation_capRateSchedule_initialValue:
                        pCo.Object = "rateFloatingRate";
                        pCo.Element = "capRateSchedule";
                        pCo.OccurenceValue = 1;
                        pIsSpecified = _cciStream.IsCapRateScheduleSpecified;
                        pIsEnabled = true;
                        break;
                    case CciStream.CciEnum.calculationPeriodAmount_calculation_floatingRateCalculation_floorRateSchedule_initialValue:
                        pCo.Object = "rateFloatingRate";
                        pCo.Element = "floorRateSchedule";
                        pCo.OccurenceValue = 1;
                        pIsSpecified = _cciStream.IsFloorRateScheduleSpecified;
                        pIsEnabled = true;
                        break;
                    case CciStream.CciEnum.calculationPeriodAmount_calculation_floatingRateCalculation_spreadSchedule_initialValue:
                        pCo.Object = "rateFloatingRate";
                        pCo.Element = "spreadSchedule";
                        pCo.OccurenceValue = 1;
                        pIsSpecified = _cciStream.IsSpreadScheduleSpecified;
                        pIsEnabled = true;
                        break;
                    case CciStream.CciEnum.calculationPeriodAmount_calculation_floatingRateCalculation_floatingRateMultiplierSchedule_initialValue:
                        pCo.Object = "rateFloatingRate";
                        pCo.Element = "floatingRateMultiplierSchedule";
                        pCo.OccurenceValue = 1;
                        pIsSpecified = _cciStream.IsFloatingRateMultiplierScheduleSpecified;
                        pIsEnabled = true;
                        break;
                    default:
                        isOk = false;
                        break;
                }
            }
            #endregion Stream
            #region Premium
            if (false == isOk)
            {
                for (int i = 0; i < PremiumsLength; i++)
                {
                    if (_cciPremium[i].IsCciOfContainer(pCci.ClientId_WithoutPrefix))
                    {
                        isOk = _cciPremium[i].IsCci(CciPayment.CciEnumPayment.settlementInformation, pCci);
                        if (isOk)
                        {
                            pCo.Object = "premium";
                            pCo.Element = "settlementInformation";
                            pCo.OccurenceValue = i + 1;
                            pIsSpecified = _cciPremium[i].IsSettlementInfoSpecified;
                            pIsEnabled = _cciPremium[i].IsSettlementInstructionSpecified;
                            break;
                        }
                    }
                }
            }
            #endregion Premium
            #region additionalPayment
            if (false == isOk)
            {
                for (int i = 0; i < this.AdditionalPaymentsLength; i++)
                {
                    if (CciAdditionalPayment[i].IsCciOfContainer(pCci.ClientId_WithoutPrefix))
                    {
                        isOk = CciAdditionalPayment[i].IsCci(CciPayment.CciEnumPayment.settlementInformation, pCci);
                        if (isOk)
                        {
                            pCo.Object = "additionalPayment";
                            pCo.Element = "settlementInformation";
                            pCo.OccurenceValue = i + 1;
                            pIsSpecified = CciAdditionalPayment[i].IsSettlementInfoSpecified;
                            pIsEnabled = CciAdditionalPayment[i].IsSettlementInstructionSpecified;
                            break;
                        }
                    }
                }
            }
            #endregion additionalPayment
            #region earlyTerminationProvision
            if (false == isOk)
                isOk = _cciEarlyTerminationProvision.SetButtonZoom(pCci, pCo, ref pIsSpecified, ref pIsEnabled);
            #endregion earlyTerminationProvision

            return isOk;

        }
        #endregion SetButtonZoom
        #endregion ITradeGetInfoButton Members
        
        #region IContainerCciQuoteBasis Members
        #region GetCurrency1
        public override string GetCurrency1(CustomCaptureInfo pCci)
        {

            string ret = string.Empty;
            if (StrFunc.IsEmpty(ret))
            {
                for (int i = 0; i < PremiumsLength; i++)
                {
                    ret = _cciPremium[i].GetCurrency1(pCci);
                    if (StrFunc.IsFilled(ret))
                        break;
                }
            }
            //
            if (StrFunc.IsEmpty(ret))
            {
                for (int i = 0; i < AdditionalPaymentsLength; i++)
                {
                    ret = CciAdditionalPayment[i].GetCurrency1(pCci);
                    if (StrFunc.IsFilled(ret))
                        break;
                }
            }
            //
            return ret;

        }
        #endregion GetCurrency1
        #region GetCurrency2
        public override string GetCurrency2(CustomCaptureInfo pCci)
        {
            string ret = string.Empty;
            //
            if (StrFunc.IsEmpty(ret))
            {
                for (int i = 0; i < PremiumsLength; i++)
                {
                    ret = _cciPremium[i].GetCurrency2(pCci);
                    if (StrFunc.IsFilled(ret))
                        break;
                }
            }
            //
            if (StrFunc.IsEmpty(ret))
            {
                for (int i = 0; i < AdditionalPaymentsLength; i++)
                {
                    ret = CciAdditionalPayment[i].GetCurrency2(pCci);
                    if (StrFunc.IsFilled(ret))
                        break;
                }
            }
            //
            return ret;

        }
        #endregion GetCurrency2
        #region IsClientId_QuoteBasis
        public override bool IsClientId_QuoteBasis(CustomCaptureInfo pCci)
        {

            bool isOk = false;
            for (int i = 0; i < PremiumsLength; i++)
            {
                isOk = _cciPremium[i].IsClientId_QuoteBasis(pCci);
                if (isOk)
                    break;
            }
            //
            if (false == isOk)
            {
                for (int i = 0; i < AdditionalPaymentsLength; i++)
                {
                    isOk = CciAdditionalPayment[i].IsClientId_QuoteBasis(pCci);
                    if (isOk)
                        break;
                }
            }
            //
            if (false == isOk)
                isOk = base.IsClientId_QuoteBasis(pCci);
            //
            return isOk;

        }
        #endregion IsClientId_QuoteBasis
        #endregion IContainerCciQuoteBasis Members
        #endregion Interfaces

        #region Methods
        public override void SetProduct(IProduct pProduct)
        {

            if (null != pProduct)
                _capFloor = new CapFloorContainer((ICapFloor)pProduct);

            IInterestRateStream stream = null;
            if (null != _capFloor)
                stream = _capFloor.Stream;
            _cciStream = new CciStream(CciTrade, Prefix + TradeCustomCaptureInfos.CCst.Prefix_capFloorStream, -1, stream);

            _cciEarlyTerminationProvision = new CciEarlyTerminationProvision(CciTrade, Prefix);

        }
        #region public override GetData
        public override string GetData(string pKey, CustomCaptureInfo pCci)
        {

            string ret = string.Empty;
            //
            if (StrFunc.IsEmpty(pKey))
            {
                if (_cciStream.IsCciOfContainer(pCci.ClientId_WithoutPrefix))
                {
                    string cliendId_Key_Stream = _cciStream.CciContainerKey(pCci.ClientId_WithoutPrefix);
                    CciStream.CciEnum enumStreamKey = CciStream.CciEnum.unknown;
                    //
                    if (System.Enum.IsDefined(typeof(CciStream.CciEnum), cliendId_Key_Stream))
                        enumStreamKey = (CciStream.CciEnum)System.Enum.Parse(typeof(CciStream.CciEnum), cliendId_Key_Stream);
                    //  
                    switch (enumStreamKey)
                    {
                        case CciStream.CciEnum.calculationPeriodDates_effectiveDate:
                            pKey = "T";
                            break;
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
                    case "E":
                        ret = _cciStream.Cci(CciStream.CciEnum.calculationPeriodDates_effectiveDate).NewValue;
                        break;
                }
            }
            //
            return ret;

        }
        #endregion GetData
        //
        #region private InitializePayment_FromCci
        private void InitializePayment_FromCci(bool pIsPremium)
        {

            bool isOk = true;
            int index = -1;
            bool saveSpecified;
            ArrayList lst = new ArrayList();
            IPayment[] payments;
            CciPayment[] cciPayments;
            string prefixPayment;
            string element;
            //
            if (pIsPremium)
            {
                payments = _capFloor.Premium;
                element = "premium";
                saveSpecified = _capFloor.PremiumSpecified;
                prefixPayment = TradeCustomCaptureInfos.CCst.Prefix_premium;
                //			
                //				if (PayerReceiverEnum.Payer  == rateBuyer)
                //					clientIdReceiver = CciClientIdReceiver; 	
                //				else
                //					clientIdReceiver = CciClientIdPayer; 	
                //
            }
            else
            {
                payments = _capFloor.AdditionalPayment;
                element = "additionalPayment";
                saveSpecified = _capFloor.AdditionalPaymentSpecified;
                prefixPayment = TradeCustomCaptureInfos.CCst.Prefix_additionalPayment;
            }

            while (isOk)
            {
                index += 1;
                CciPayment cciPayment = new CciPayment(CciTrade, index + 1, null, CciPayment.PaymentTypeEnum.Payment, prefixPayment, string.Empty, string.Empty, string.Empty, CciClientIdMainCurrency, string.Empty);

                //
                isOk = CcisBase.Contains(cciPayment.CciClientId(CciPayment.CciEnumPayment.payer));
                if (isOk)
                {
                    if (ArrFunc.IsEmpty(payments) || (index == payments.Length))
                    {
                        ReflectionTools.AddItemInArray(_capFloor.CapFloor, element, index);
                        //
                        //20110407 FI/GM [17370] valorisation de payments parce que l'ajout d'un item dans l'élément ne se répercute pas dans la variable payments
                        if (pIsPremium)
                            payments = _capFloor.Premium;
                        else
                            payments = _capFloor.AdditionalPayment;
                    }
                    //
                    if (pIsPremium)
                        cciPayment.Payment = _capFloor.Premium[index];
                    else
                        cciPayment.Payment = _capFloor.AdditionalPayment[index];
                    //
                    lst.Add(cciPayment);
                }
            }
            //
            cciPayments = new CciPayment[lst.Count];
            for (int i = 0; i < lst.Count; i++)
            {
                cciPayments[i] = (CciPayment)lst[i];
                cciPayments[i].Initialize_FromCci();
            }
            //			
            if (pIsPremium)
            {
                _cciPremium = cciPayments;
                _capFloor.PremiumSpecified = saveSpecified;
            }
            else
            {
                CciAdditionalPayment = cciPayments;
                _capFloor.AdditionalPaymentSpecified = saveSpecified;
            }
            //

        }
        #endregion InitializePayment_FromCci
        #region private InitializeProvision_FromCci
        private void InitializeProvision_FromCci()
        {

            CciTools.CreateInstance(this, _capFloor);
            //
            if (null != _cciEarlyTerminationProvision)
                _cciEarlyTerminationProvision.Initialize_FromCci();

        }
        #endregion InitializeProvision_FromCci
        #endregion Methods

        #region ICciPresentation Membres
        public override void DumpSpecific_ToGUI(CciPageBase pPage)
        {
            _cciStream.DumpSpecific_ToGUI(pPage);
            base.DumpSpecific_ToGUI(pPage);
        }
        #endregion

    }

}
