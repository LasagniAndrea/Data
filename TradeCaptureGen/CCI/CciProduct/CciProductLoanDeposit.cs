#region Using Directives
using EFS.ACommon;
using EFS.Common;
using EFS.Common.Web;
using EFS.GUI.CCI;
using EfsML.Business;
using EfsML.Interface;
using FpML.Interface;
using System;
using System.Collections;
using System.Linq;
#endregion Using Directives

namespace EFS.TradeInformation
{

    /// <summary>
    /// Description résumée de cciLoanDeposit.
    /// </summary>
    internal class CciProductLoanDeposit : CciProductBase
    {

        #region Members
        
        private ILoanDeposit _loanDeposit;
        private CciStream _cciStreamGlobal; //Represente les ccis dits globaux
        private CciStream[] _cciStream;
        private CciPayment[] _cciAdditionalPayment;
        private CciCancelableProvision _cciCancelableProvision;
        private CciExtendibleProvision _cciExtendibleProvision;
        private CciEarlyTerminationProvision _cciEarlyTerminationProvision;
        #endregion Members

        #region Enums
        #region CciEnum
        public enum CciEnum
        {
            calculationPeriodDates_effectiveMinDate,
            calculationPeriodDates_terminationMaxDate,
            unknown,
        }
        #endregion CciEnum
        #endregion Enums

        #region Accessors
        /// <summary>
        /// 
        /// </summary>
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

        
        #region AdditionalPaymentLength
        public int AdditionalPaymentLength
        {
            get { return ArrFunc.IsFilled(_cciAdditionalPayment) ? _cciAdditionalPayment.Length : 0; }
        }
        #endregion AdditionalPaymentLength
        #region LoanDepositStreamLength
        public int LoanDepositStreamLength
        {
            get { return ArrFunc.IsFilled(_cciStream) ? _cciStream.Length : 0; }
        }
        #endregion LoanDepositStreamLength
        #region cciAdditionalPayment
        public CciPayment[] CciAdditionalPayment
        {
            get { return _cciAdditionalPayment; }
        }
        #endregion
        #endregion Accessors
        //
        #region Constructors
        public CciProductLoanDeposit(CciTrade pCciTrade, ILoanDeposit pLoanDeposit, string pPrefix)
            : this(pCciTrade, pLoanDeposit, pPrefix, -1)
        { }
        public CciProductLoanDeposit(CciTrade pCciTrade, ILoanDeposit pLoanDeposit, string pPrefix, int pNumber)
            : base((CciTradeCommonBase)pCciTrade, (IProduct)pLoanDeposit, pPrefix, pNumber)
        {
            _loanDeposit = pLoanDeposit;
        }
        #endregion Constructors

        #region Interfaces
        #region IContainerCciPayerReceiver Members
        #region public override CciClientIdPayer
        public override string CciClientIdPayer
        {
            get
            {
                int j = 0;
                for (int i = 0; i < LoanDepositStreamLength; i++)
                {
                    if (false == _cciStream[i].IsFloatingRateSpecified)
                    {
                        j = i;
                        break;
                    }
                }
                return _cciStream[j].CciClientIdPayer;
            }
        }
        #endregion
        #region public override CciClientIdReceiver
        public override string CciClientIdReceiver
        {
            get
            {
                int j = 0;
                for (int i = 0; i < LoanDepositStreamLength; i++)
                {
                    if (false == _cciStream[i].IsFloatingRateSpecified)
                    {
                        j = i;
                        break;
                    }
                }
                return _cciStream[j].CciClientIdReceiver;
            }
        }
        #endregion
        #region public override SynchronizePayerReceiver
        public override void SynchronizePayerReceiver(string pLastValue, string pNewValue)
        {
            for (int i = 0; i < _cciStream.Length; i++)
                _cciStream[i].SynchronizePayerReceiver(pLastValue, pNewValue);

            for (int i = 0; i < AdditionalPaymentLength; i++)
                _cciStream[i].SynchronizePayerReceiver(pLastValue, pNewValue);

        }
        #endregion
        #endregion IContainerCciPayerReceiver Members

        #region IContainerCciFactory Members
        #region public override AddCciSystem
        /// <summary>
        /// 
        /// </summary>
        /// FI 20170116 [21916] Modify (use AddCciSystem Method)
        public override void AddCciSystem()
        {

            CciTools.AddCciSystem(CcisBase, Cst.BUT + _cciStreamGlobal.CciClientId(CciStream.CciEnum.calculationPeriodAmount_calculation_notionalSchedule_notionalStepParameters.ToString()), false, TypeData.TypeDataEnum.@string);

            for (int i = 0; i < LoanDepositStreamLength; i++)
                _cciStream[i].AddCciSystem();

            for (int i = 0; i < AdditionalPaymentLength; i++)
                _cciAdditionalPayment[i].AddCciSystem();
        }
        #endregion AddCciSystem
        #region public override CleanUp
        public override void CleanUp()
        {

            for (int i = 0; i < LoanDepositStreamLength; i++)
                _cciStream[i].CleanUp();
            //
            for (int i = 0; i < AdditionalPaymentLength; i++)
                _cciAdditionalPayment[i].CleanUp();
            //
            // Suppression des streams issus du paramétrage screen et non alimenté
            if (ArrFunc.IsFilled(_loanDeposit.Stream))
            {
                for (int i = _loanDeposit.Stream.Length - 1; -1 < i; i--)
                {
                    if (false == CaptureTools.IsDocumentElementValid(_loanDeposit.Stream[i].PayerPartyReference.HRef))
                        ReflectionTools.RemoveItemInArray(_loanDeposit, "loanDepositStream", i);
                }
            }
            //
            // Suppression des additionalPayment issus du paramétrage screen et non alimenté
            if (ArrFunc.IsFilled(_loanDeposit.AdditionalPayment))
            {
                for (int i = _loanDeposit.AdditionalPayment.Length - 1; -1 < i; i--)
                {

                    if (false == CaptureTools.IsDocumentElementValid(_loanDeposit.AdditionalPayment[i].PayerPartyReference.HRef))
                        ReflectionTools.RemoveItemInArray(_loanDeposit, "additionalPayment", i);
                }
            }
            _loanDeposit.AdditionalPaymentSpecified = (ArrFunc.IsFilled(_loanDeposit.AdditionalPayment) && (0 < _loanDeposit.AdditionalPayment.Length));
            //

        }
        #endregion CleanUp
        #region public override Dump_ToDocument
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
                        case CciEnum.calculationPeriodDates_effectiveMinDate:
                            #region Min EffectiveDate
                            processQueue = CustomCaptureInfosBase.ProcessQueueEnum.High;
                            #endregion Min EffectiveDate
                            break;
                        case CciEnum.calculationPeriodDates_terminationMaxDate:
                            #region Max TerminationDate
                            processQueue = CustomCaptureInfosBase.ProcessQueueEnum.High;
                            #endregion Max TerminationDate
                            break;
                        default:
                            #region default
                            isSetting = false;
                            #endregion default
                            break;
                    }
                    if (isSetting)
                        CcisBase.Finalize(cci.ClientId_WithoutPrefix, processQueue);

                }
            }
            #region Synchronize tradeStreamGlobal from cciLoanDeposit[0]
            // Permet de conserver en phase le stream global et tradeLoanDeposit[0] pour les ccis existants dans les 2 objects
            // Ex il existe loanDeposit_payer et loanDeposit1.payer
            foreach (CustomCaptureInfo cci in CcisBase)
            {
                if (cci.HasChanged && _cciStream[0].IsCciOfContainer(cci.ClientId_WithoutPrefix))
                {
                    string clientId_Key = _cciStream[0].CciContainerKey(cci.ClientId_WithoutPrefix);
                    CciStream.CciEnum cciEnum = CciStream.CciEnum.unknown;
                    if (System.Enum.IsDefined(typeof(CciStream.CciEnum), clientId_Key))
                    {
                        cciEnum = (CciStream.CciEnum)System.Enum.Parse(typeof(CciStream.CciEnum), clientId_Key);
                        if (CcisBase.Contains(_cciStreamGlobal.CciClientId(cciEnum)))
                            _cciStreamGlobal.Cci(cciEnum).NewValue = cci.NewValue;
                    }
                }
            }
            #endregion Synchronize tradeStreamGlobal from cciLoanDeposit[0]
            #region synchronize cciLoanDeposit[i] from  tradeStreamGlobal (exclude Payer/receiver)
            foreach (CustomCaptureInfo cci in CcisBase)
            {
                if (cci.HasChanged && _cciStreamGlobal.IsCciOfContainer(cci.ClientId_WithoutPrefix))
                {
                    string clientId_Key = _cciStreamGlobal.CciContainerKey(cci.ClientId_WithoutPrefix);
                    CciStream.CciEnum cciEnum = CciStream.CciEnum.unknown;
                    if (System.Enum.IsDefined(typeof(CciStream.CciEnum), clientId_Key))
                    {
                        cciEnum = (CciStream.CciEnum)System.Enum.Parse(typeof(CciStream.CciEnum), clientId_Key);
                        if ((CciStream.CciEnum.receiver != cciEnum) && (CciStream.CciEnum.payer != cciEnum))
                        {
                            for (int i = 0; i < LoanDepositStreamLength; i++)
                                _cciStream[i].Cci(cciEnum).NewValue = cci.NewValue;
                        }
                    }
                }
            }
            #endregion synchronize cciLoanDeposit[i] from  tradeStreamGlobal (exclude Payer/receiver)
            //			
            _cciStreamGlobal.Dump_ToDocument();
            //
            for (int i = 0; i < LoanDepositStreamLength; i++)
                _cciStream[i].Dump_ToDocument();
            //
            if (Cst.Capture.IsModeInput(CcisBase.CaptureMode) && (false == Cst.Capture.IsModeAction(CcisBase.CaptureMode)))
                Ccis.InitializePaymentPaymentQuoteRelativeTo(_cciStream[0], null, _cciAdditionalPayment, CciTrade.cciOtherPartyPayment);
            //
            //AdditionalPayment
            for (int i = 0; i < AdditionalPaymentLength; i++)
                _cciAdditionalPayment[i].Dump_ToDocument();
            _loanDeposit.AdditionalPaymentSpecified = CciTools.Dump_IsCciContainerArraySpecified(_loanDeposit.AdditionalPaymentSpecified, _cciAdditionalPayment);
            //
            if (null != _cciCancelableProvision)
                _cciCancelableProvision.Dump_ToDocument();
            if (null != _cciExtendibleProvision)
                _cciExtendibleProvision.Dump_ToDocument();
            if (null != _cciEarlyTerminationProvision)
                _cciEarlyTerminationProvision.Dump_ToDocument();
            //

        }
        #endregion Dump_ToDocument
        #region public override Initialize_Document
        public override void Initialize_Document()
        {

            if (Cst.Capture.IsModeNew(CcisBase.CaptureMode) && (false == CcisBase.IsPreserveData))
            {
                string id = string.Empty;
                //
                for (int i = 0; i < LoanDepositStreamLength; i++)
                {
                    if (_cciStream[i].Cci(CciStream.CciEnum.payer).IsMandatory &&
                        _cciStream[i].Cci(CciStream.CciEnum.receiver).IsMandatory)
                    {
                        if (StrFunc.IsEmpty(_loanDeposit.Stream[i].PayerPartyReference.HRef) &&
                            StrFunc.IsEmpty(_loanDeposit.Stream[i].ReceiverPartyReference.HRef))
                        {
                            if (1 == i)
                            {
                                _loanDeposit.Stream[i].PayerPartyReference.HRef = _loanDeposit.Stream[i - 1].PayerPartyReference.HRef;
                                _loanDeposit.Stream[i].ReceiverPartyReference.HRef = _loanDeposit.Stream[i - 1].ReceiverPartyReference.HRef;
                            }
                            else
                            {
                                if (StrFunc.IsEmpty(id))
                                    id = StrFunc.IsFilled(id) ? id : TradeCustomCaptureInfos.PartyUnknown;
                                _loanDeposit.Stream[i].PayerPartyReference.HRef = id;
                            }
                        }
                    }
                }
                //
                for (int i = 0; i < AdditionalPaymentLength; i++)
                {
                    if ((_cciAdditionalPayment[i].Cci(CciPayment.CciEnum.payer).IsMandatory) &&
                        (_cciAdditionalPayment[i].Cci(CciPayment.CciEnum.receiver).IsMandatory))
                    {
                        if (StrFunc.IsEmpty(_loanDeposit.AdditionalPayment[i].PayerPartyReference.HRef) &&
                            StrFunc.IsEmpty(_loanDeposit.AdditionalPayment[i].ReceiverPartyReference.HRef))
                        {
                            _loanDeposit.AdditionalPayment[i].PayerPartyReference.HRef = _loanDeposit.Stream[0].PayerPartyReference.HRef;
                            _loanDeposit.AdditionalPayment[i].ReceiverPartyReference.HRef = _loanDeposit.Stream[0].ReceiverPartyReference.HRef;
                        }
                    }
                }
                //
                if (TradeCustomCaptureInfos.PartyUnknown == id)
                    CciTrade.AddPartyUnknown();
            }

        }
        #endregion Initialize_Document
        #region public override Initialize_FromCci
        public override void Initialize_FromCci()
        {
            InitializeLoanDepositStream_FromCci();
            InitializeAdditionalPayment_FromCci();
            InitializeProvision_FromCci();
        }
        #endregion Initialize_FromCci
        #region public override Initialize_FromDocument
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
                        #region calculationPeriodDates_effectiveMinDate & calculationPeriodDates_terminationMaxDate
                        case CciEnum.calculationPeriodDates_effectiveMinDate:
                            data = _loanDeposit.Stream[0].CalculationPeriodDates.EffectiveDateAdjustable.UnadjustedDate.Value;
                            break;
                        case CciEnum.calculationPeriodDates_terminationMaxDate:
                            for (int i = _loanDeposit.Stream.Length - 1; i > -1; i--)
                            {
                                //On ne récupère pas la termination date de la dernière jambe
                                //En création lorsqu'il y a ajout d'un stream, la date terminationMaxDate est initialisée à blanc (On pert la saisie déjà effectuée)
                                if (DtFunc.IsDateTimeFilled(_loanDeposit.Stream[i].CalculationPeriodDates.TerminationDateAdjustable.UnadjustedDate.DateValue))
                                {
                                    data = _loanDeposit.Stream[i].CalculationPeriodDates.TerminationDateAdjustable.UnadjustedDate.Value;
                                    break;
                                }
                            }
                            break;
                        #endregion calculationPeriodDates_effectiveMinDate & calculationPeriodDates_terminationMaxDate
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
            _cciStreamGlobal.Initialize_FromDocument();
            //
            for (int i = 0; i < LoanDepositStreamLength; i++)
                _cciStream[i].Initialize_FromDocument();
            //
            for (int i = 0; i < AdditionalPaymentLength; i++)
                _cciAdditionalPayment[i].Initialize_FromDocument();
            //
            if (null != _cciCancelableProvision)
                _cciCancelableProvision.Initialize_FromDocument();
            if (null != _cciExtendibleProvision)
                _cciExtendibleProvision.Initialize_FromDocument();
            if (null != _cciEarlyTerminationProvision)
                _cciEarlyTerminationProvision.Initialize_FromDocument();
            //

        }
        #endregion Initialize_FromDocument
        #region public override IsClientId_PayerOrReceiver
        public override bool IsClientId_PayerOrReceiver(CustomCaptureInfo pCci)
        {

            bool isOk = _cciStreamGlobal.IsClientId_PayerOrReceiver(pCci);
            if (false == isOk)
            {
                for (int i = 0; i < LoanDepositStreamLength; i++)
                {
                    isOk = isOk || _cciStream[i].IsClientId_PayerOrReceiver(pCci);
                    if (isOk) break;
                }
            }
            //
            if (false == isOk)
            {
                for (int i = 0; i < AdditionalPaymentLength; i++)
                {
                    isOk = _cciAdditionalPayment[i].IsClientId_PayerOrReceiver(pCci);
                    if (isOk)
                        break;
                }
            }
            //
            if (false == isOk)
            {
                if (null != _cciCancelableProvision)
                    isOk = _cciCancelableProvision.IsClientId_PayerOrReceiver(pCci);
            }
            if (false == isOk)
            {
                if (null != _cciExtendibleProvision)
                    isOk = _cciExtendibleProvision.IsClientId_PayerOrReceiver(pCci);
            }
            if (false == isOk)
            {
                if (null != _cciEarlyTerminationProvision)
                    _cciEarlyTerminationProvision.IsClientId_PayerOrReceiver(pCci);
            }
            return isOk;

        }
        #endregion IsClientId_PayerOrReceiver
        #region public override ProcessInitialize
        public override void ProcessInitialize(CustomCaptureInfo pCci)
        {
            if (IsCciOfContainer(pCci.ClientId_WithoutPrefix))
            {
                string clientId_Element = CciContainerKey(pCci.ClientId_WithoutPrefix);
                //
                CciEnum elt = CciEnum.unknown;
                if (System.Enum.IsDefined(typeof(CciEnum), clientId_Element))
                    elt = (CciEnum)System.Enum.Parse(typeof(CciEnum), clientId_Element);
                //
                switch (elt)
                {
                    case CciEnum.calculationPeriodDates_effectiveMinDate:
                        // FI 20161214 [21916] Modification afin d'éviter toute dégradation puisqu'avant correction de la méthode SetNewValue la valeur True sur le paramètre était equivalent à false
                        //ccis.SetNewValue(_cciStream[0].CciClientId(CciStream.CciEnum.calculationPeriodDates_effectiveDate), pCci.NewValue, true);
                        //ccis.SetNewValue(_cciStream[1].CciClientId(CciStream.CciEnum.calculationPeriodDates_effectiveDate), pCci.NewValue, true);
                        CcisBase.SetNewValue(_cciStream[0].CciClientId(CciStream.CciEnum.calculationPeriodDates_effectiveDate), pCci.NewValue, false);
                        CcisBase.SetNewValue(_cciStream[1].CciClientId(CciStream.CciEnum.calculationPeriodDates_effectiveDate), pCci.NewValue, false);

                        break;
                    case CciEnum.calculationPeriodDates_terminationMaxDate:
                        Math.DivRem(LoanDepositStreamLength, 2, out int k);
                        if (0 == k)
                        {
                            // FI 20161214 [21916] Modification afin d'éviter toute dégradation puisqu'avant correction de la méthode SetNewValue la valeur True sur le paramètre était equivalent à false
                            //ccis.SetNewValue(_cciStream[LoanDepositStreamLength - 2].CciClientId(CciStream.CciEnum.calculationPeriodDates_terminationDate), pCci.NewValue, true);
                            CcisBase.SetNewValue(_cciStream[LoanDepositStreamLength - 2].CciClientId(CciStream.CciEnum.calculationPeriodDates_terminationDate), pCci.NewValue, false);
                        }
                        // FI 20161214 [21916] Modification afin d'éviter toute dégradation puisqu'avant correction de la méthode SetNewValue la valeur True sur le paramètre était equivalent à false    
                        //ccis.SetNewValue(_cciStream[LoanDepositStreamLength - 1].CciClientId(CciStream.CciEnum.calculationPeriodDates_terminationDate), pCci.NewValue, true);
                        CcisBase.SetNewValue(_cciStream[LoanDepositStreamLength - 1].CciClientId(CciStream.CciEnum.calculationPeriodDates_terminationDate), pCci.NewValue, false);
                        break;
                }
            }
            //
            _cciStreamGlobal.ProcessInitialize(pCci);
            //
            for (int i = 0; i < LoanDepositStreamLength; i++)
                _cciStream[i].ProcessInitialize(pCci);
            //
            SynchronizeBDAFixedRate(pCci);
            //
            for (int i = 0; i < AdditionalPaymentLength; i++)
                _cciAdditionalPayment[i].ProcessInitialize(pCci);
            //
            if (null != _cciCancelableProvision)
                _cciCancelableProvision.ProcessInitialize(pCci);
            if (null != _cciExtendibleProvision)
                _cciExtendibleProvision.ProcessInitialize(pCci);
            if (null != _cciEarlyTerminationProvision)
                _cciEarlyTerminationProvision.ProcessInitialize(pCci);

        }
        #endregion ProcessInitialize
        #region public override RefreshCciEnabled
        public override void RefreshCciEnabled()
        {
            _cciStreamGlobal.RefreshCciEnabled();
            for (int i = 0; i < LoanDepositStreamLength; i++)
                _cciStream[i].RefreshCciEnabled();
            for (int i = 0; i < AdditionalPaymentLength; i++)
                _cciAdditionalPayment[i].RefreshCciEnabled();
        }
        #endregion RefreshCciEnabled
        #region public override SetDisplay
        public override void SetDisplay(CustomCaptureInfo pCci)
        {
            _cciStreamGlobal.SetDisplay(pCci);
            //
            for (int i = 0; i < LoanDepositStreamLength; i++)
                _cciStream[i].SetDisplay(pCci);
            //
            for (int i = 0; i < AdditionalPaymentLength; i++)
                _cciAdditionalPayment[i].SetDisplay(pCci);
            //
            if (null != _cciCancelableProvision)
                _cciCancelableProvision.SetDisplay(pCci);
            //
            if (null != _cciExtendibleProvision)
                _cciCancelableProvision.SetDisplay(pCci);
            //
            if (null != _cciEarlyTerminationProvision)
                _cciEarlyTerminationProvision.SetDisplay(pCci);
        }
        #endregion SetDisplay
        #endregion IContainerCciFactory Members

        #region IContainerCciQuoteBasis Members
        #region public override GetCurrency1
        public override string GetCurrency1(CustomCaptureInfo pCci)
        {
            string ret = string.Empty;
            for (int i = 0; i < AdditionalPaymentLength; i++)
            {
                ret = _cciAdditionalPayment[i].GetCurrency1(pCci);
                if (StrFunc.IsFilled(ret))
                    break;
            }
            return ret;
        }
        #endregion GetCurrency1
        #region public override GetCurrency2
        public override string GetCurrency2(CustomCaptureInfo pCci)
        {
            string ret = string.Empty;
            for (int i = 0; i < AdditionalPaymentLength; i++)
            {
                ret = _cciAdditionalPayment[i].GetCurrency2(pCci);
                if (StrFunc.IsFilled(ret))
                    break;
            }
            return ret;
        }
        #endregion GetCurrency2
        #region public override IsClientId_QuoteBasis
        public override bool IsClientId_QuoteBasis(CustomCaptureInfo pCci)
        {
            bool isClientId_QuoteBasis = false;
            for (int i = 0; i < AdditionalPaymentLength; i++)
            {
                isClientId_QuoteBasis = _cciAdditionalPayment[i].IsClientId_QuoteBasis(pCci);
                if (isClientId_QuoteBasis)
                    break;
            }
            return isClientId_QuoteBasis;
        }
        #endregion IsClientId_QuoteBasis
        #region public override GetBaseCurrency
        public override string GetBaseCurrency(CustomCaptureInfo pCci)
        {
            return string.Empty;
        }
        #endregion GetBaseCurrency
        #endregion IContainerCciQuoteBasis Members

        #region ICciGetInfoButton Members
        #region public override SetButtonZoom
        public override bool SetButtonZoom(CustomCaptureInfo pCci, CustomObjectButtonFpmlObject pCo, ref bool pIsSpecified, ref bool pIsEnabled)
        {

            bool isOk = false;
            //
            #region Button on StreamGlobal
            if (_cciStreamGlobal.IsCciOfContainer(pCci.ClientId_WithoutPrefix))
            {
                isOk = _cciStreamGlobal.IsCci(CciStream.CciEnum.calculationPeriodAmount_calculation_notionalSchedule_notionalStepSchedule_initialValue, pCci);
                if (isOk)
                {
                    pCo.Object = "calculationNotional";
                    pCo.Element = "notionalStepSchedule";
                    pCo.OccurenceValue = 1;
                    pCo.CopyTo = "All";
                    pIsSpecified = _cciStreamGlobal.IsNotionalStepScheduleStepSpecified;
                    pIsEnabled = true;
                }
                //
                if (false == isOk)
                {
                    isOk = _cciStreamGlobal.IsCci(CciStream.CciEnum.calculationPeriodAmount_calculation_notionalSchedule_notionalStepParameters, pCci);
                    if (isOk)
                    {
                        pCo.Object = "calculationNotional";
                        pCo.Element = "notionalStepParameters";
                        pCo.OccurenceValue = 1;
                        pCo.CopyTo = "All";
                        pIsSpecified = _cciStreamGlobal.IsNotionalStepParametersSpecified;
                        pIsEnabled = true;
                    }
                }

                if (false == isOk)
                {
                    isOk = _cciStreamGlobal.IsCci(CciStream.CciEnum.calculationPeriodAmount_knownAmountSchedule_initialValue, pCci);
                    if (isOk)
                    {
                        isOk = true;
                        pCo.Object = "";
                        pCo.Element = "calculationPeriodAmountKnownAmountSchedule";
                        pCo.OccurenceValue = 1;
                        pCo.CopyTo = "All";
                        pIsSpecified = _cciStreamGlobal.IsKnownAmountScheduleStepSpecified;
                        pIsEnabled = true;
                    }
                }
                //
                if (false == isOk)
                {
                    isOk = _cciStreamGlobal.IsCci(CciStream.CciEnum.calculationPeriodAmount_calculation_notionalSchedule_notionalStepParameters, pCci);
                    if (isOk)
                    {
                        isOk = true;
                        pCo.Object = "calculationNotional";
                        pCo.Element = "notionalStepParameters";
                        pCo.OccurenceValue = 1;
                        pCo.CopyTo = "All";
                        pIsSpecified = _cciStreamGlobal.IsNotionalStepParametersSpecified;
                        pIsEnabled = true;
                    }
                }
            }
            #endregion Button on StreamGlobal
            //
            #region  CciStream[i]
            if (false == isOk)
            {
                for (int i = 0; i < this.LoanDepositStreamLength; i++)
                {
                    if (_cciStream[i].IsCciOfContainer(pCci.ClientId_WithoutPrefix))
                    {
                        string key = _cciStream[i].CciContainerKey(pCci.ClientId_WithoutPrefix);
                        CciStream.CciEnum elt = CciStream.CciEnum.unknown;
                        //
                        if (System.Enum.IsDefined(typeof(CciStream.CciEnum), key))
                            elt = (CciStream.CciEnum)System.Enum.Parse(typeof(CciStream.CciEnum), key);
                        //
                        isOk = true;
                        switch (elt)
                        {
                            case CciStream.CciEnum.calculationPeriodAmount_calculation_notionalSchedule_notionalStepSchedule_initialValue:
                                pCo.Object = "calculationNotional";
                                pCo.Element = "notionalStepSchedule";
                                pCo.ObjectIndexValue = i;
                                pCo.OccurenceValue = i + 1;
                                pIsSpecified = _cciStream[i].IsNotionalStepScheduleStepSpecified;
                                pIsEnabled = true;
                                break;

                            case CciStream.CciEnum.calculationPeriodAmount_calculation_notionalSchedule_notionalStepParameters:
                                pCo.Object = "calculationNotional";
                                pCo.Element = "notionalStepParameters";
                                pCo.ObjectIndexValue = i;
                                pCo.OccurenceValue = i + 1;
                                pIsSpecified = _cciStream[i].IsNotionalStepParametersSpecified;
                                pIsEnabled = true;
                                break;

                            case CciStream.CciEnum.calculationPeriodAmount_knownAmountSchedule_initialValue:
                                pCo.Object = string.Empty;
                                pCo.Element = "calculationPeriodAmountKnownAmountSchedule";
                                pCo.ObjectIndexValue = i;
                                pCo.OccurenceValue = i + 1;
                                pIsSpecified = _cciStream[i].IsKnownAmountScheduleStepSpecified;
                                pIsEnabled = true;
                                break;

                            case CciStream.CciEnum.calculationPeriodAmount_calculation_floatingRateCalculation_capRateSchedule_initialValue:
                                pCo.Object = "rateFloatingRate";
                                pCo.Element = "capRateSchedule";
                                pCo.ObjectIndexValue = i;
                                pCo.OccurenceValue = i + 1;
                                pIsSpecified = _cciStream[i].IsCapRateScheduleSpecified;
                                pIsEnabled = _cciStream[i].IsFloatingRateSpecified;
                                break;

                            case CciStream.CciEnum.calculationPeriodAmount_calculation_floatingRateCalculation_floorRateSchedule_initialValue:
                                pCo.Object = "rateFloatingRate";
                                pCo.Element = "floorRateSchedule";
                                pCo.ObjectIndexValue = i;
                                pCo.OccurenceValue = i + 1;
                                pIsSpecified = _cciStream[i].IsFloorRateScheduleSpecified;
                                pIsEnabled = _cciStream[i].IsFloatingRateSpecified;
                                break;

                            case CciStream.CciEnum.calculationPeriodAmount_calculation_floatingRateCalculation_spreadSchedule_initialValue:
                                pCo.Object = "rateFloatingRate";
                                pCo.Element = "spreadSchedule";
                                pCo.ObjectIndexValue = i;
                                pCo.OccurenceValue = i + 1;
                                pIsSpecified = _cciStream[i].IsSpreadScheduleSpecified;
                                pIsEnabled = _cciStream[i].IsFloatingRateSpecified;
                                break;

                            case CciStream.CciEnum.calculationPeriodAmount_calculation_floatingRateCalculation_floatingRateMultiplierSchedule_initialValue:
                                pCo.Object = "rateFloatingRate";
                                pCo.Element = "floatingRateMultiplierSchedule";
                                pCo.ObjectIndexValue = i;
                                pCo.OccurenceValue = i + 1;
                                pIsSpecified = _cciStream[i].IsFloatingRateMultiplierScheduleSpecified;
                                pIsEnabled = _cciStream[i].IsFloatingRateSpecified;
                                break;

                            case CciStream.CciEnum.calculationPeriodAmount_calculation_fixedRateSchedule_initialValue:
                                pCo.Object = string.Empty;
                                pCo.Element = "rateFixedRate";
                                // 20090911 EG Add pCo.ObjectIndexValue = i;
                                pCo.ObjectIndexValue = i;
                                pCo.OccurenceValue = i + 1;
                                pIsSpecified = _cciStream[i].IsFixedRateScheduleSpecified;
                                pIsEnabled = _cciStream[i].IsFixedRateSpecified;
                                break;

                            case CciStream.CciEnum.paymentDates_offset:
                                pCo.Object = "paymentDates";
                                pCo.Element = "paymentDaysOffset";
                                // 20090911 EG Add pCo.ObjectIndexValue = i;
                                pCo.ObjectIndexValue = i;
                                pCo.OccurenceValue = i + 1;
                                pIsSpecified = _cciStream[i].IsOffsetSpecified;
                                pIsEnabled = true;
                                break;

                            default:
                                isOk = false;
                                break;
                        }
                    }
                }
            }
            #endregion  CciStream[i]
            //
            #region  AdditionalPaymentLength
            if (false == isOk)
            {
                for (int i = 0; i < this.AdditionalPaymentLength; i++)
                {
                    if (_cciAdditionalPayment[i].IsCciOfContainer(pCci.ClientId_WithoutPrefix))
                    {
                        isOk = _cciAdditionalPayment[i].IsCci(CciPayment.CciEnumPayment.settlementInformation, pCci);
                        if (isOk)
                        {
                            pCo.Object = "additionalPayment";
                            pCo.Element = "settlementInformation";
                            pCo.OccurenceValue = i + 1;
                            pIsSpecified = _cciAdditionalPayment[i].IsSettlementInfoSpecified;
                            pIsEnabled = _cciAdditionalPayment[i].IsSettlementInstructionSpecified;
                            break;
                        }
                    }
                }
            }
            #endregion  AdditionalPaymentLength
            //
            if (false == isOk)
                isOk = _cciCancelableProvision.SetButtonZoom(pCci, pCo, ref pIsSpecified, ref pIsEnabled);
            if (false == isOk)
                isOk = _cciExtendibleProvision.SetButtonZoom(pCci, pCo, ref pIsSpecified, ref pIsEnabled);
            if (false == isOk)
                isOk = _cciEarlyTerminationProvision.SetButtonZoom(pCci, pCo, ref pIsSpecified, ref pIsEnabled);
            return isOk;

        }
        #endregion SetButtonZoom
        #endregion ICciGetInfoButton Members

        #region ITradeCci Members
        #region public override GetMainCurrency
        public override string GetMainCurrency
        {
            get
            {
                string ret = string.Empty;
                if (_loanDeposit.Stream[0].CalculationPeriodAmount.CalculationSpecified)
                {
                    ICalculation calc = _loanDeposit.Stream[0].CalculationPeriodAmount.Calculation;
                    if (calc.NotionalSpecified)
                        ret = calc.Notional.StepSchedule.Currency.Value;
                }
                else if (_loanDeposit.Stream[0].CalculationPeriodAmount.KnownAmountScheduleSpecified)
                    ret = _loanDeposit.Stream[0].CalculationPeriodAmount.KnownAmountSchedule.Currency.Value;

                return ret;

            }
        }
        #endregion GetMainCurrency
        #region public override RetSidePayer
        //20080610 PL DEPRECATED: Sur un LoanDeposit on considère comme acheteur l'emprunteur du cash, donc le payer du taux 
        //20081216 PL ATTENTION, sur un LoanDeposit on considère comme acheteur l'emprunteur du cash, donc le payer du taux 
        public override string RetSidePayer { get { return SideTools.RetBuySide(); } }
        #endregion RetSidePayer
        #region public override RetSideReceiver
        //20080610 PL DEPRECATED: Sur un LoanDeposit on considère comme acheteur le prêteur du cash, donc le receiver du taux 
        //20081216 PL ATTENTION, sur un LoanDeposit on considère comme vendeur le payeur du cash, donc le receveur du taux 
        public override string RetSideReceiver { get { return SideTools.RetSellSide(); } }
        #endregion RetSideReceiver
        #endregion ITradeCci Members

        #region ICciPresentation Membres
        public override void DumpSpecific_ToGUI(CciPageBase pPage)
        {
            for (int i = 0; i < ArrFunc.Count(_cciStream); i++)
                _cciStream[i].DumpSpecific_ToGUI(pPage);
            base.DumpSpecific_ToGUI(pPage);
        }
        #endregion

        #region public override GetData
        public override string GetData(string pKey, CustomCaptureInfo pCci)
        {

            string ret = string.Empty;
            //
            for (int i = 0; i < LoanDepositStreamLength; i++)
            {
                if (StrFunc.IsEmpty(pKey))
                {
                    if (_cciStream[i].IsCciOfContainer(pCci.ClientId_WithoutPrefix))
                    {
                        string cliendId_Key = _cciStream[i].CciContainerKey(pCci.ClientId_WithoutPrefix);
                        CciStream.CciEnum enumStreamKey = CciStream.CciEnum.unknown;
                        //
                        if (System.Enum.IsDefined(typeof(CciStream.CciEnum), cliendId_Key))
                            enumStreamKey = (CciStream.CciEnum)System.Enum.Parse(typeof(CciStream.CciEnum), cliendId_Key);
                        //                
                        switch (enumStreamKey)
                        {
                            case CciStream.CciEnum.calculationPeriodDates_effectiveDate:
                                pKey = "T";
                                break;
                            case CciStream.CciEnum.calculationPeriodDates_terminationDate:
                                pKey = "E";
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
                                ret = _cciStream[i].Cci(CciStream.CciEnum.calculationPeriodDates_effectiveDate).NewValue;
                                break;
                        }
                    }
                }
            }
            //
            if (StrFunc.IsEmpty(pKey))
            {
                if (IsCciOfContainer(pCci.ClientId_WithoutPrefix))
                {
                    string cliendId_Key = CciContainerKey(pCci.ClientId_WithoutPrefix);
                    CciEnum enumStreamKey = CciEnum.unknown;
                    //
                    if (System.Enum.IsDefined(typeof(CciEnum), cliendId_Key))
                        enumStreamKey = (CciEnum)System.Enum.Parse(typeof(CciEnum), cliendId_Key);
                    //                
                    switch (enumStreamKey)
                    {
                        case CciEnum.calculationPeriodDates_effectiveMinDate:
                            pKey = "T";
                            break;
                        case CciEnum.calculationPeriodDates_terminationMaxDate:
                            pKey = "E";
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
                            ret = Cci(CciEnum.calculationPeriodDates_effectiveMinDate).NewValue;
                            break;
                        case "E1":
                            ret = _cciStream[0].Cci(CciStream.CciEnum.calculationPeriodDates_effectiveDate).NewValue;
                            break;
                    }
                }
            }
            //
            return ret;

        }
        #endregion
        #endregion Interfaces

        #region Methods
        public override void SetProduct(IProduct pProduct)
        {

            _loanDeposit = (ILoanDeposit)pProduct;
            //
            IInterestRateStream streamGlogal = null;
            if ((null != _loanDeposit) && ArrFunc.IsFilled(_loanDeposit.Stream))
                streamGlogal = _loanDeposit.Stream[0];
            _cciStreamGlobal = new CciStream(CciTrade, Prefix + TradeCustomCaptureInfos.CCst.Prefix_loanDepositStream, -1, streamGlogal);
            //
            _cciStream = null;
            //
            _cciCancelableProvision = new CciCancelableProvision(CciTrade, Prefix);
            _cciExtendibleProvision = new CciExtendibleProvision(CciTrade, Prefix);
            _cciEarlyTerminationProvision = new CciEarlyTerminationProvision(CciTrade, Prefix);
            //
            base.SetProduct(pProduct);

        }

        #region private InitializeAdditionalPayment_FromCci
        private void InitializeAdditionalPayment_FromCci()
        {
            bool isOk = true;
            int index = -1;
            //            
            bool saveSpecified = _loanDeposit.AdditionalPaymentSpecified;
            ArrayList lst = new ArrayList();
            while (isOk)
            {
                index += 1;
                CciPayment cciPayment = new CciPayment(CciTrade, index + 1, null, CciPayment.PaymentTypeEnum.Payment, Prefix + TradeCustomCaptureInfos.CCst.Prefix_additionalPayment, string.Empty, string.Empty, string.Empty, CciTrade.CciClientIdMainCurrency, string.Empty);
                isOk = CcisBase.Contains(cciPayment.CciClientId(CciPayment.CciEnumPayment.payer));
                if (isOk)
                {
                    if (ArrFunc.IsEmpty(_loanDeposit.AdditionalPayment) || (index == _loanDeposit.AdditionalPayment.Length))
                        ReflectionTools.AddItemInArray(_loanDeposit, "additionalPayment", index);
                    cciPayment.Payment = _loanDeposit.AdditionalPayment[index];
                    //
                    lst.Add(cciPayment);
                }
            }
            //
            _cciAdditionalPayment = (CciPayment[])lst.ToArray(typeof(CciPayment));
            for (int i = 0; i < this.AdditionalPaymentLength; i++)
                _cciAdditionalPayment[i].Initialize_FromCci();
            //			
            _loanDeposit.AdditionalPaymentSpecified = saveSpecified;
        }
        #endregion InitializeAdditionalPayment_FromCci
        #region private InitializeLoanDepositStream_FromCci
        private void InitializeLoanDepositStream_FromCci()
        {

            bool isOk = true;
            int index = -1;
            ArrayList lst = new ArrayList();
            while (isOk)
            {
                index += 1;
                //
                CciStream cciStreamCurrent = new CciStream(CciTrade, Prefix + TradeCustomCaptureInfos.CCst.Prefix_loanDepositStream, index + 1, null);
                //
                isOk = CcisBase.Contains(cciStreamCurrent.CciClientId(CciStream.CciEnum.payer));
                if (isOk)
                {
                    if (ArrFunc.IsEmpty(_loanDeposit.Stream) || (index == _loanDeposit.Stream.Length))
                    {
                        ReflectionTools.AddItemInArray(_loanDeposit, "loanDepositStream", index);
                        if (ArrFunc.IsFilled(Ccis.TradeCommonInput.FpMLDataDocReader.Party))
                        {
                            _loanDeposit.Stream[index].PayerPartyReference.HRef = Ccis.TradeCommonInput.FpMLDataDocReader.Party[0].Id;
                            _loanDeposit.Stream[index].ReceiverPartyReference.HRef = string.Empty;
                            if (2 == ArrFunc.Count(Ccis.TradeCommonInput.FpMLDataDocReader.Party))
                                _loanDeposit.Stream[index].ReceiverPartyReference.HRef = Ccis.TradeCommonInput.FpMLDataDocReader.Party[1].Id;
                        }
                    }
                    //
                    if (StrFunc.IsEmpty(_loanDeposit.Stream[index].Id))
                        _loanDeposit.Stream[index].Id = Prefix + TradeCustomCaptureInfos.CCst.Prefix_loanDepositStream + Convert.ToString(index + 1);
                    //					
                    cciStreamCurrent.Irs = _loanDeposit.Stream[index];
                    // 
                    lst.Add(cciStreamCurrent);
                }
            }
            _cciStream = (CciStream[])lst.ToArray(typeof(CciStream));
            //
            #region génération ds chaque Tradestream des ccis du  CciStreamGlobal
            for (int i = 0; i < CcisBase.Count; i++)
            {
                CustomCaptureInfo cci = CcisBase[i];
                if (_cciStreamGlobal.IsCciOfContainer(cci.ClientId_WithoutPrefix))
                {
                    string clientId_Key = _cciStreamGlobal.CciContainerKey(cci.ClientId_WithoutPrefix);
                    if (System.Enum.IsDefined(typeof(CciStream.CciEnum), clientId_Key))
                    {
                        for (int j = 0; j < LoanDepositStreamLength; j++)
                            Ccis.CloneGlobalCci(clientId_Key, cci, _cciStream[j]);
                    }
                }
            }
            #endregion génération ds chaque Tradestream des ccis du  CciStreamGlobal
            //
            _cciStreamGlobal.Initialize_FromCci();
            for (int i = 0; i < LoanDepositStreamLength; i++)
                _cciStream[i].Initialize_FromCci();
            //			

        }
        #endregion InitializeLoanDepositStream_FromCci
        #region private InitializeProvision_FromCci
        private void InitializeProvision_FromCci()
        {
            CciTools.CreateInstance(this, _loanDeposit);
            //
            if (null != _cciCancelableProvision)
                _cciCancelableProvision.Initialize_FromCci();
            if (null != _cciExtendibleProvision)
                _cciExtendibleProvision.Initialize_FromCci();
            if (null != _cciEarlyTerminationProvision)
                _cciEarlyTerminationProvision.Initialize_FromCci();
        }

        #endregion InitializeProvision_FromCci
        #region private SynchronizeBDAFixedRate
        private void SynchronizeBDAFixedRate(CustomCaptureInfo pCci)
        {

            ArrayList alFloatingRateStream = new ArrayList();
            //
            for (int i = 0; i < LoanDepositStreamLength; i++)
            {
                if (_cciStream[i].IsFloatingRateSpecified)
                    alFloatingRateStream.Add(i);
            }
            //
            foreach (int streamFloatIndex in alFloatingRateStream)
            {
                //Si Existence Taux Flottant
                //synchronisation des BDA des stream à taux fixe (ou non renseigné) en fonction des BDA du 1er stream à taux flottant
                //Ces derniers sont en général alimentés avec les infos rattachées au rate_Index 
                //Ces Synchro n'est réalisée que lorsque La Zone BDC n'existe pas ds l'écran 

                for (int i = 0; i < LoanDepositStreamLength; i++)
                {
                    if (i != streamFloatIndex)
                    {
                        if (false == _cciStream[i].IsFloatingRateSpecified)
                        {
                            string clientId_WithoutPrefix;
                            if (_cciStream[streamFloatIndex].IsCci(CciStream.CciEnum.calculationPeriodDates_calculationPeriodDatesAdjustments_bDC, pCci))
                            {
                                if (0 == ((IInterval)_cciStream[i].Irs.CalculationPeriodDates.CalculationPeriodFrequency).CompareTo(_cciStream[streamFloatIndex].Irs.CalculationPeriodDates.CalculationPeriodFrequency))
                                {
                                    //CalculationPeriodDates
                                    clientId_WithoutPrefix = _cciStream[i].CciClientId(CciStream.CciEnum.calculationPeriodDates_calculationPeriodDatesAdjustments_bDC);
                                    CcisBase[clientId_WithoutPrefix].NewValue = _cciStream[streamFloatIndex].Irs.CalculationPeriodDates.CalculationPeriodDatesAdjustments.BusinessDayConvention.ToString();
                                }
                            }

                            if (_cciStream[streamFloatIndex].IsCci(CciStream.CciEnum.paymentDates_paymentDatesAdjustments_bDC, pCci))
                            {
                                if (0 == _cciStream[i].Irs.PaymentDates.PaymentFrequency.CompareTo(_cciStream[streamFloatIndex].Irs.PaymentDates.PaymentFrequency))
                                {
                                    //paymentDates
                                    clientId_WithoutPrefix = _cciStream[i].CciClientId(CciStream.CciEnum.paymentDates_paymentDatesAdjustments_bDC);
                                    CcisBase[clientId_WithoutPrefix].NewValue = _cciStream[streamFloatIndex].Irs.PaymentDates.PaymentDatesAdjustments.BusinessDayConvention.ToString();
                                }
                            }
                        }
                    }
                }
            }
        }
        #endregion SynchronizeBDAFixedRate
        #endregion Methods
    }

}
