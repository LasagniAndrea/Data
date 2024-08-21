#region Using Directives
using System;
using System.Collections;
using System.Linq;

using EFS.ACommon;
using EFS.Common;
using EFS.Common.Web;
using EFS.GUI.CCI;
using EFS.GUI.Interface; 

using EfsML.Business;

using FpML.Interface;
#endregion Using Directives

namespace EFS.TradeInformation
{
  
    /// <summary>
    /// Description résumée de TradeSwap.
    /// </summary>
    public class CciProductSwap : CciProductBase
    {
        #region membres
        
        private ISwap _swap;
        private CciCancelableProvision _cciCancelableProvision;
        private CciExtendibleProvision _cciExtendibleProvision;
        private CciEarlyTerminationProvision _cciEarlyTerminationProvision;
        #endregion membres

        #region Enum
        public enum CciEnum
        {
            calculationPeriodDates_effectiveMinDate,
            calculationPeriodDates_terminationMaxDate,
            isSynchronous,
            unknown,
        }
        #endregion Enum

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
            get { return ArrFunc.IsFilled(CciAdditionalPayment) ? CciAdditionalPayment.Length : 0; }
        }
        #endregion AdditionalPaymentLength
        #region SwapStreamLength
        /// <summary>
        /// Obtient le nbr de CciStream
        /// </summary>
        public int SwapStreamLength
        {
            get { return ArrFunc.IsFilled(CciSwapStream) ? CciSwapStream.Length : 0; }
        }
        #endregion SwapStreamLength
        #region cciStreamGlobal
        /// <summary>
        /// Obtient le cciStreamGlobal
        /// </summary>
        public CciStream CciStreamGlobal { get; private set; }
        #endregion
        #region CciStream
        /// <summary>
        /// Obtient l'array CciStream
        /// </summary>
        public CciStream[] CciSwapStream { get; private set; }
        #endregion
        #region CciAdditionalPayment
        /// <summary>
        /// Obtient l'array CciAdditionalPayment
        /// </summary>
        public CciPayment[] CciAdditionalPayment { get; private set; }
        #endregion
        #endregion Accessors

        #region Constructor
        public CciProductSwap(CciTrade pCciTrade, ISwap pSwap, string pPrefix)
            : this(pCciTrade, pSwap, pPrefix, -1)
        { }
        public CciProductSwap(CciTrade pCciTrade, ISwap pSwap, string pPrefix, int pNumber)
            : base((CciTradeCommonBase)pCciTrade, (IProduct)pSwap, pPrefix, pNumber)
        {
            
        }
        #endregion Constructor

        #region Membres de ITradeCci
        #region public override GetMainCurrency
        public override string GetMainCurrency
        {
            get
            {
                string ret = string.Empty;
                //
                if (_swap.Stream[0].CalculationPeriodAmount.CalculationSpecified)
                {
                    ICalculation calc = _swap.Stream[0].CalculationPeriodAmount.Calculation;
                    if (calc.NotionalSpecified)
                        ret = calc.Notional.StepSchedule.Currency.Value;
                    else if (calc.FxLinkedNotionalSpecified)
                        ret = calc.FxLinkedNotional.Currency;
                }
                else if (_swap.Stream[0].CalculationPeriodAmount.KnownAmountScheduleSpecified)
                    ret = _swap.Stream[0].CalculationPeriodAmount.KnownAmountSchedule.Currency.Value;

                return ret;
            }
        }
        #endregion
        #region public override RetSidePayer
        public override string RetSidePayer { get { return SideTools.RetBuySide(); } }
        #endregion RetSidePayer
        #region public override RetSideReceiver
        public override string RetSideReceiver { get { return SideTools.RetSellSide(); } }
        #endregion RetSideReceiver
        #region public override GetData
        public override string GetData(string pKey, CustomCaptureInfo pCci)
        {

            string ret = string.Empty;
            //
            if (StrFunc.IsEmpty(pKey))
            {
                if (CciStreamGlobal.IsCciOfContainer(pCci.ClientId_WithoutPrefix))
                {
                    string cliendId_Key = CciStreamGlobal.CciContainerKey(pCci.ClientId_WithoutPrefix);
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
            }
            //
            if (StrFunc.IsFilled(pKey))
            {
                switch (pKey.ToUpper())
                {
                    case "E":
                        ret = CciSwapStream[0].Cci(CciStream.CciEnum.calculationPeriodDates_effectiveDate).NewValue;
                        break;
                }
            }
            //
            return ret;

        }
        #endregion
        #region public override CciClientIdMainCurrency
        //20091014 FI CciClientIdMainCurrency est utiliser pour les ajustements associés aux dates d'exercice
        //(voir CciExercise)
        public override string CciClientIdMainCurrency
        {
            get { return CciSwapStream[0].CciClientId(CciStream.CciEnum.calculationPeriodAmount_calculation_notionalSchedule_notionalStepSchedule_currency); }
        }
        #endregion CciClientIdMainCurrency
        #endregion Membres de ITrade

        #region Membres de IContainerCciPayerReceiver
        /// <summary>
        /// 
        /// </summary>
        public override string CciClientIdPayer
        {
            get
            {
                int j = 0;
                for (int i = 0; i < SwapStreamLength; i++)
                {
                    if (false == CciSwapStream[i].IsFloatingRateSpecified)
                    {
                        j = i;
                        break;
                    }
                }
                return CciSwapStream[j].CciClientIdPayer;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public override string CciClientIdReceiver
        {
            get
            {
                int j = 0;
                for (int i = 0; i < SwapStreamLength; i++)
                {
                    if (false == CciSwapStream[i].IsFloatingRateSpecified)
                    {
                        j = i;
                        break;
                    }
                }
                return CciSwapStream[j].CciClientIdReceiver;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pLastValue"></param>
        /// <param name="pNewValue"></param>
        public override void SynchronizePayerReceiver(string pLastValue, string pNewValue)
        {
            for (int i = 0; i < SwapStreamLength; i++)
                CciSwapStream[i].SynchronizePayerReceiver(pLastValue, pNewValue);
            //
            for (int i = 0; i < AdditionalPaymentLength; i++)
                CciAdditionalPayment[i].SynchronizePayerReceiver(pLastValue, pNewValue);

        }
        #endregion

        #region Membres de IContainerCciFactory
        /// <summary>
        /// 
        /// </summary>
        public override void Initialize_FromCci()
        {
            InitializeSwapStream_FromCci();
            InitializeAdditionalPayment_FromCci();
            InitializeProvision_FromCci();
        }
        /// <summary>
        /// 
        /// </summary>
        /// FI 20170116 [21916] Modify (use AddCciSystem Method)
        /// FI 20170907 [XXXXX] Modify
        public override void AddCciSystem()
        {
            //
            //20070906 FI Mise en commentaire => pour eviter des mise à jour en cascade lorsque l'on change une donnée sur le stream 1
            // Rappel la donnée du stream 1 est copier sur le stream 0 et toute donnée du stream 0 (global) est recopié sur l'ensemble des streams 
            //CciStreamGlobal.AddCciSystem();  
            //
            //20070924 FI Ticket 15763
            CciTools.AddCciSystem(CcisBase, Cst.BUT + CciStreamGlobal.CciClientId(CciStream.CciEnum.calculationPeriodAmount_calculation_notionalSchedule_notionalStepParameters.ToString()), false, TypeData.TypeDataEnum.@string);

            for (int i = 0; i < SwapStreamLength; i++)
                CciSwapStream[i].AddCciSystem();

            for (int i = 0; i < AdditionalPaymentLength; i++)
                CciAdditionalPayment[i].AddCciSystem();

            // FI 20170907 [XXXXX] Appel AddCciSystem
            if (null != _cciCancelableProvision)
                _cciCancelableProvision.AddCciSystem();
            
            // FI 20170907 [XXXXX] Appel AddCciSystem
            if (null != _cciExtendibleProvision)
                _cciExtendibleProvision.AddCciSystem();

            // FI 20170907 [XXXXX] Appel AddCciSystem
            if (null != _cciEarlyTerminationProvision)
                _cciEarlyTerminationProvision.AddCciSystem();
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
                    #endregion Reset variables

                    switch (cciEnum)
                    {

                        case CciEnum.calculationPeriodDates_effectiveMinDate:
                            data = _swap.Stream[0].CalculationPeriodDates.EffectiveDateAdjustable.UnadjustedDate.Value;
                            break;
                        case CciEnum.calculationPeriodDates_terminationMaxDate:
                            for (int i = _swap.Stream.Length - 1; i > -1; i--)
                            {
                                //On ne recupère pas la termination date de la dernière jambe
                                //En création lorsqu'il y a ajout d'un stream, la date terminationMaxDate est initialisée à blanc (On pert la saisie déjà effectuée)
                                if (DtFunc.IsDateTimeFilled(_swap.Stream[i].CalculationPeriodDates.TerminationDateAdjustable.UnadjustedDate.DateValue))
                                {
                                    data = _swap.Stream[i].CalculationPeriodDates.TerminationDateAdjustable.UnadjustedDate.Value;
                                    break;
                                }
                            }
                            break;
                        case CciEnum.isSynchronous:
                            data = new EFS_Boolean(_swap.IsPaymentDatesSynchronous).Value;
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
            CciStreamGlobal.Initialize_FromDocument();
            //
            for (int i = 0; i < SwapStreamLength; i++)
                CciSwapStream[i].Initialize_FromDocument();
            //
            for (int i = 0; i < AdditionalPaymentLength; i++)
                CciAdditionalPayment[i].Initialize_FromDocument();
            //
            if (null != _cciCancelableProvision)
                _cciCancelableProvision.Initialize_FromDocument();
            if (null != _cciExtendibleProvision)
                _cciExtendibleProvision.Initialize_FromDocument();
            if (null != _cciEarlyTerminationProvision)
                _cciEarlyTerminationProvision.Initialize_FromDocument();

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
                        #region specific
                        case CciEnum.calculationPeriodDates_effectiveMinDate:
                            processQueue = CustomCaptureInfosBase.ProcessQueueEnum.High;
                            break;

                        case CciEnum.calculationPeriodDates_terminationMaxDate:
                            processQueue = CustomCaptureInfosBase.ProcessQueueEnum.High;
                            break;

                        case CciEnum.isSynchronous:
                            _swap.IsPaymentDatesSynchronous = BoolFunc.IsTrue(data);
                            processQueue = CustomCaptureInfosBase.ProcessQueueEnum.High;
                            break;

                        #endregion specific
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
            //
            #region synchrozine tradeStreamGlobal from cciSwap[0]
            // Permet de conserver en phase le stream global et tradeswap[0] pour les ccis existants dans les 2 objects
            // Ex il existe irs_payer et irs1.payer
            foreach (CustomCaptureInfo cci in CcisBase)
            {
                if (cci.HasChanged && CciSwapStream[0].IsCciOfContainer(cci.ClientId_WithoutPrefix))
                {
                    string clientId_Key = CciSwapStream[0].CciContainerKey(cci.ClientId_WithoutPrefix);
                    CciStream.CciEnum cciEnum = CciStream.CciEnum.unknown;
                    if (System.Enum.IsDefined(typeof(CciStream.CciEnum), clientId_Key))
                    {
                        cciEnum = (CciStream.CciEnum)System.Enum.Parse(typeof(CciStream.CciEnum), clientId_Key);
                        if (CcisBase.Contains(CciStreamGlobal.CciClientId(cciEnum)))
                            CciStreamGlobal.Cci(cciEnum).NewValue = cci.NewValue;
                    }
                }
            }
            #endregion

            #region synchronize cciSwap[i] from  tradeStreamGlobal (exclude Payer/receiver)
            foreach (CustomCaptureInfo cci in CcisBase)
            {
                if (cci.HasChanged && CciStreamGlobal.IsCciOfContainer(cci.ClientId_WithoutPrefix))
                {
                    string clientId_Key = CciStreamGlobal.CciContainerKey(cci.ClientId_WithoutPrefix);
                    CciStream.CciEnum cciEnum = CciStream.CciEnum.unknown;
                    if (System.Enum.IsDefined(typeof(CciStream.CciEnum), clientId_Key))
                    {
                        cciEnum = (CciStream.CciEnum)System.Enum.Parse(typeof(CciStream.CciEnum), clientId_Key);
                        if ((CciStream.CciEnum.receiver != cciEnum) && (CciStream.CciEnum.payer != cciEnum))
                        {
                            for (int i = 0; i < SwapStreamLength; i++)
                                CciSwapStream[i].Cci(cciEnum).NewValue = cci.NewValue;
                        }
                    }
                }
            }
            #endregion
            //			
            CciStreamGlobal.Dump_ToDocument();
            //
            for (int i = 0; i < SwapStreamLength; i++)
                CciSwapStream[i].Dump_ToDocument();
            //
            if (Cst.Capture.IsModeInput(CcisBase.CaptureMode) && (false == Cst.Capture.IsModeAction(CcisBase.CaptureMode)))
                Ccis.InitializePaymentPaymentQuoteRelativeTo(CciSwapStream[0], null, CciAdditionalPayment, CciTrade.cciOtherPartyPayment);
            //
            //AdditionalPayment
            for (int i = 0; i < AdditionalPaymentLength; i++)
                CciAdditionalPayment[i].Dump_ToDocument();
            _swap.AdditionalPaymentSpecified = CciTools.Dump_IsCciContainerArraySpecified(_swap.AdditionalPaymentSpecified, CciAdditionalPayment);
            //
            if (null != _cciCancelableProvision)
                _cciCancelableProvision.Dump_ToDocument();
            //
            if (null != _cciExtendibleProvision)
                _cciExtendibleProvision.Dump_ToDocument();
            //
            if (null != _cciEarlyTerminationProvision)
                _cciEarlyTerminationProvision.Dump_ToDocument();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCci"></param>
        /// FI 20161129 [RATP] Modify
        public override void ProcessInitialize(CustomCaptureInfo pCci)
        {
            if (IsCciOfContainer(pCci.ClientId_WithoutPrefix))
            {
                string clientId_Element = CciContainerKey(pCci.ClientId_WithoutPrefix);

                CciEnum elt = CciEnum.unknown;
                if (System.Enum.IsDefined(typeof(CciEnum), clientId_Element))
                    elt = (CciEnum)System.Enum.Parse(typeof(CciEnum), clientId_Element);

                switch (elt)
                {
                    case CciEnum.calculationPeriodDates_effectiveMinDate:
                        // FI 20161214 [21916] Modification afin d'éviter toute dégradation puisqu'avant correction de la méthode SetNewValue la valeur True sur le paramètre était equivalent à false

                        CcisBase.SetNewValue(CciSwapStream[0].CciClientId(CciStream.CciEnum.calculationPeriodDates_effectiveDate), pCci.NewValue, /*true*/ false);
                        if (ArrFunc.Count(CciSwapStream) > 1) // FI 20161129 [RATP] Add Test (il peut exister que seule jambe)
                            CcisBase.SetNewValue(CciSwapStream[1].CciClientId(CciStream.CciEnum.calculationPeriodDates_effectiveDate), pCci.NewValue, /*true*/ false);
                        break;
                    case CciEnum.calculationPeriodDates_terminationMaxDate:
                        int k;
                        Math.DivRem(SwapStreamLength, 2, out k);
                        if (0 == k)
                            CcisBase.SetNewValue(CciSwapStream[SwapStreamLength - 2].CciClientId(CciStream.CciEnum.calculationPeriodDates_terminationDate), pCci.NewValue, /*true*/ false);
                        CcisBase.SetNewValue(CciSwapStream[SwapStreamLength - 1].CciClientId(CciStream.CciEnum.calculationPeriodDates_terminationDate), pCci.NewValue, /*true*/ false);
                        break;
                    case CciEnum.isSynchronous:
                        break;

                }
            }
            //
            CciStreamGlobal.ProcessInitialize(pCci);
            //
            for (int i = 0; i < SwapStreamLength; i++)
                CciSwapStream[i].ProcessInitialize(pCci);
            //
            //FI 20101108 
            //Lorsque l'utilisateur renseigne le payer, Spheres® pré-alimente les ccis du _cciStream avec les valeurs des ccis dits "globales"
            for (int i = 0; i < SwapStreamLength; i++)
            {
                if (CciSwapStream[i].CciClientIdPayer == pCci.ClientId_WithoutPrefix && pCci.IsFilledValue && pCci.IsLastEmpty)
                    SynchronizeStream(i);
            }

            if (_swap.IsPaymentDatesSynchronous)
            {
                SynchronizeFixedRateStream(pCci);
            }
            else
                SynchronizeBDAFixedRate(pCci);

            for (int i = 0; i < AdditionalPaymentLength; i++)
                CciAdditionalPayment[i].ProcessInitialize(pCci);

            if (null != _cciCancelableProvision)
                _cciCancelableProvision.ProcessInitialize(pCci);

            if (null != _cciExtendibleProvision)
                _cciExtendibleProvision.ProcessInitialize(pCci);

            if (null != _cciEarlyTerminationProvision)
                _cciEarlyTerminationProvision.ProcessInitialize(pCci);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCci"></param>
        public override void ProcessExecute(CustomCaptureInfo pCci)
        {
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCci"></param>
        public override void ProcessExecuteAfterSynchronize(CustomCaptureInfo pCci)
        {

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCci"></param>
        /// <returns></returns>
        public override bool IsClientId_PayerOrReceiver(CustomCaptureInfo pCci)
        {
            bool isOk = CciStreamGlobal.IsClientId_PayerOrReceiver(pCci);
            if (!isOk)
            {
                for (int i = 0; i < SwapStreamLength; i++)
                {
                    isOk = isOk || CciSwapStream[i].IsClientId_PayerOrReceiver(pCci);
                    if (isOk)
                        break;
                }
            }

            if (!isOk)
            {
                for (int i = 0; i < AdditionalPaymentLength; i++)
                {
                    isOk = CciAdditionalPayment[i].IsClientId_PayerOrReceiver(pCci);
                    if (isOk)
                        break;
                }
            }

            if (!isOk)
            {
                if (null != _cciCancelableProvision)
                    isOk = _cciCancelableProvision.IsClientId_PayerOrReceiver(pCci);
            }
            if (!isOk)
            {
                if (null != _cciExtendibleProvision)
                    isOk = _cciExtendibleProvision.IsClientId_PayerOrReceiver(pCci);
            }
            if (!isOk)
            {
                if (null != _cciEarlyTerminationProvision)
                    _cciEarlyTerminationProvision.IsClientId_PayerOrReceiver(pCci);
            }
            return isOk;
        }
        /// <summary>
        /// 
        /// </summary>
        public override void CleanUp()
        {

            for (int i = 0; i < SwapStreamLength; i++)
                CciSwapStream[i].CleanUp();
            //
            for (int i = 0; i < AdditionalPaymentLength; i++)
                CciAdditionalPayment[i].CleanUp();
            //
            // Suppression des streams issus du paramétrage screen et non alimenté
            if (ArrFunc.IsFilled(_swap.Stream))
            {
                for (int i = _swap.Stream.Length - 1; -1 < i; i--)
                {
                    if (StrFunc.IsEmpty(_swap.Stream[i].PayerPartyReference.HRef))
                        ReflectionTools.RemoveItemInArray(_swap, "swapStream", i);
                }
            }
            //
            // Suppression des additionalPayment issus du paramétrage screen et non alimenté
            if (ArrFunc.IsFilled(_swap.AdditionalPayment))
            {
                for (int i = _swap.AdditionalPayment.Length - 1; -1 < i; i--)
                {

                    if (StrFunc.IsEmpty(_swap.AdditionalPayment[i].PayerPartyReference.HRef))
                        ReflectionTools.RemoveItemInArray(_swap, "additionalPayment", i);
                }
            }
            _swap.AdditionalPaymentSpecified = (ArrFunc.IsFilled(_swap.AdditionalPayment) && (0 < _swap.AdditionalPayment.Length));
            //

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCci"></param>
        public override void SetDisplay(CustomCaptureInfo pCci)
        {

            CciStreamGlobal.SetDisplay(pCci);
            //
            for (int i = 0; i < SwapStreamLength; i++)
                CciSwapStream[i].SetDisplay(pCci);
            //
            for (int i = 0; i < AdditionalPaymentLength; i++)
                CciAdditionalPayment[i].SetDisplay(pCci);
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
        /// <summary>
        /// 
        /// </summary>
        public override void RefreshCciEnabled()
        {
            CciStreamGlobal.RefreshCciEnabled();
            //
            for (int i = 0; i < SwapStreamLength; i++)
                CciSwapStream[i].RefreshCciEnabled();
            //
            for (int i = 0; i < AdditionalPaymentLength; i++)
                CciAdditionalPayment[i].RefreshCciEnabled();
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
                for (int i = 0; i < SwapStreamLength; i++)
                {
                    if (CciSwapStream[i].Cci(CciStream.CciEnum.payer).IsMandatory &&
                        CciSwapStream[i].Cci(CciStream.CciEnum.receiver).IsMandatory)
                    {
                        if (StrFunc.IsEmpty(_swap.Stream[i].PayerPartyReference.HRef) &&
                            StrFunc.IsEmpty(_swap.Stream[i].ReceiverPartyReference.HRef))
                        {
                            if (1 == i)
                            {
                                _swap.Stream[i].PayerPartyReference.HRef = _swap.Stream[i - 1].ReceiverPartyReference.HRef;
                                _swap.Stream[i].ReceiverPartyReference.HRef = _swap.Stream[i - 1].PayerPartyReference.HRef;
                            }
                            else
                            {
                                if (StrFunc.IsEmpty(id))
                                {
                                    //id = cciTrade.GetIdFirstPartyCounterparty();
                                    id = StrFunc.IsFilled(id) ? id : TradeCustomCaptureInfos.PartyUnknown;
                                }
                                _swap.Stream[i].PayerPartyReference.HRef = id;
                            }
                        }
                    }
                }
                //
                for (int i = 0; i < AdditionalPaymentLength; i++)
                {
                    if ((CciAdditionalPayment[i].Cci(CciPayment.CciEnum.payer).IsMandatory) && (CciAdditionalPayment[i].Cci(CciPayment.CciEnum.receiver).IsMandatory))
                    {
                        if (StrFunc.IsEmpty(_swap.AdditionalPayment[i].PayerPartyReference.HRef) &&
                            StrFunc.IsEmpty(_swap.AdditionalPayment[i].PayerPartyReference.HRef))
                        {
                            _swap.AdditionalPayment[i].PayerPartyReference.HRef = _swap.Stream[0].PayerPartyReference.HRef;
                        }
                    }
                }
                //
                if (TradeCustomCaptureInfos.PartyUnknown == id)
                    CciTrade.AddPartyUnknown();
            }
        }
        #endregion

        #region Membres de IContainerCciQuoteBasis
        #region public override IsClientId_QuoteBasis
        public override bool IsClientId_QuoteBasis(CustomCaptureInfo pCci)
        {

            bool ret = false;
            //
            if (false == ret)
            {
                for (int i = 0; i < AdditionalPaymentLength; i++)
                {
                    ret = CciAdditionalPayment[i].IsClientId_QuoteBasis(pCci);
                    if (ret)
                        break;
                }
            }
            //
            return ret;

        }
        #endregion
        #region public override GetCurrency1
        public override string GetCurrency1(CustomCaptureInfo pCci)
        {

            string ret = string.Empty;
            //
            if (StrFunc.IsEmpty(ret))
            {
                for (int i = 0; i < AdditionalPaymentLength; i++)
                {
                    ret = CciAdditionalPayment[i].GetCurrency1(pCci);
                    if (StrFunc.IsFilled(ret))
                        break;
                }
            }
            //
            return ret;

        }
        #endregion
        #region public override GetCurrency2
        public override string GetCurrency2(CustomCaptureInfo pCci)
        {
            string ret = string.Empty;
            //
            if (StrFunc.IsEmpty(ret))
            {
                for (int i = 0; i < AdditionalPaymentLength; i++)
                {
                    ret = CciAdditionalPayment[i].GetCurrency2(pCci);
                    if (StrFunc.IsFilled(ret))
                        break;
                }
            }
            //
            return ret;
        }
        #endregion
        #region public override GetBaseCurrency
        public override string GetBaseCurrency(CustomCaptureInfo pCci)
        {
            return string.Empty;

        }
        #endregion
        #endregion

        #region Membres de ICciGetInfoButton
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
            #region Button on StreamGlobal
            if (CciStreamGlobal.IsCciOfContainer(pCci.ClientId_WithoutPrefix))
            {
                isOk = CciStreamGlobal.IsCci(CciStream.CciEnum.calculationPeriodAmount_calculation_notionalSchedule_notionalStepSchedule_initialValue, pCci);
                if (isOk)
                {
                    pCo.Object = "calculationNotional";
                    pCo.Element = "notionalStepSchedule";
                    pCo.OccurenceValue = 1;
                    pCo.CopyTo = "All";
                    pIsSpecified = CciStreamGlobal.IsNotionalStepScheduleStepSpecified;
                    pIsEnabled = true;
                }
                //
                if (false == isOk)
                {
                    isOk = CciStreamGlobal.IsCci(CciStream.CciEnum.calculationPeriodAmount_calculation_notionalSchedule_notionalStepParameters, pCci);
                    if (isOk)
                    {
                        pCo.Object = "calculationNotional";
                        pCo.Element = "notionalStepParameters";
                        pCo.OccurenceValue = 1;
                        pCo.CopyTo = "All";
                        pIsSpecified = CciStreamGlobal.IsNotionalStepParametersSpecified;
                        pIsEnabled = true;
                    }
                }

                if (false == isOk)
                {
                    isOk = CciStreamGlobal.IsCci(CciStream.CciEnum.calculationPeriodAmount_knownAmountSchedule_initialValue, pCci);
                    if (isOk)
                    {
                        isOk = true;
                        pCo.Object = "calculationPeriodAmount";
                        pCo.Element = "calculationPeriodAmountKnownAmountSchedule";
                        pCo.OccurenceValue = 1;
                        pCo.CopyTo = "All";
                        pIsSpecified = CciStreamGlobal.IsKnownAmountScheduleStepSpecified;
                        pIsEnabled = true;
                    }
                }
                //
                if (false == isOk)
                {
                    isOk = CciStreamGlobal.IsCci(CciStream.CciEnum.calculationPeriodAmount_calculation_notionalSchedule_notionalStepParameters, pCci);
                    if (isOk)
                    {
                        isOk = true;
                        pCo.Object = "calculationNotional";
                        pCo.Element = "notionalStepParameters";
                        pCo.OccurenceValue = 1;
                        pCo.CopyTo = "All";
                        pIsSpecified = CciStreamGlobal.IsNotionalStepParametersSpecified;
                        pIsEnabled = true;
                    }
                }
            }
            #endregion Button on StreamGlobal
            //
            #region  CciSwapStream[i]
            if (!isOk)
            {
                for (int i = 0; i < this.SwapStreamLength; i++)
                {
                    if (CciSwapStream[i].IsCciOfContainer(pCci.ClientId_WithoutPrefix))
                    {
                        string key = CciSwapStream[i].CciContainerKey(pCci.ClientId_WithoutPrefix);
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
                                pIsSpecified = CciSwapStream[i].IsNotionalStepScheduleStepSpecified;
                                pIsEnabled = true;
                                break;

                            case CciStream.CciEnum.calculationPeriodAmount_calculation_notionalSchedule_notionalStepParameters:
                                pCo.Object = "calculationNotional";
                                pCo.Element = "notionalStepParameters";
                                pCo.ObjectIndexValue = i;
                                pCo.OccurenceValue = i + 1;
                                pIsSpecified = CciSwapStream[i].IsNotionalStepParametersSpecified;
                                pIsEnabled = true;
                                break;

                            case CciStream.CciEnum.calculationPeriodAmount_knownAmountSchedule_initialValue:
                                pCo.Object = "calculationPeriodAmount";
                                pCo.Element = "calculationPeriodAmountKnownAmountSchedule";
                                pCo.ObjectIndexValue = i;
                                pCo.OccurenceValue = i + 1;
                                pIsSpecified = CciSwapStream[i].IsKnownAmountScheduleStepSpecified;
                                pIsEnabled = true;
                                break;

                            case CciStream.CciEnum.calculationPeriodAmount_calculation_floatingRateCalculation_capRateSchedule_initialValue:
                                pCo.Object = "rateFloatingRate";
                                pCo.Element = "capRateSchedule";
                                pCo.ObjectIndexValue = i;
                                pCo.OccurenceValue = i + 1;
                                pIsSpecified = CciSwapStream[i].IsCapRateScheduleSpecified;
                                pIsEnabled = CciSwapStream[i].IsFloatingRateSpecified;
                                break;

                            case CciStream.CciEnum.calculationPeriodAmount_calculation_floatingRateCalculation_floorRateSchedule_initialValue:
                                pCo.Object = "rateFloatingRate";
                                pCo.Element = "floorRateSchedule";
                                pCo.ObjectIndexValue = i;
                                pCo.OccurenceValue = i + 1;
                                pIsSpecified = CciSwapStream[i].IsFloorRateScheduleSpecified;
                                pIsEnabled = CciSwapStream[i].IsFloatingRateSpecified;
                                break;

                            case CciStream.CciEnum.calculationPeriodAmount_calculation_floatingRateCalculation_spreadSchedule_initialValue:
                                pCo.Object = "rateFloatingRate";
                                pCo.Element = "spreadSchedule";
                                pCo.ObjectIndexValue = i;
                                pCo.OccurenceValue = i + 1;
                                pIsSpecified = CciSwapStream[i].IsSpreadScheduleSpecified;
                                pIsEnabled = CciSwapStream[i].IsFloatingRateSpecified;
                                break;

                            case CciStream.CciEnum.calculationPeriodAmount_calculation_floatingRateCalculation_floatingRateMultiplierSchedule_initialValue:
                                pCo.Object = "rateFloatingRate";
                                pCo.Element = "floatingRateMultiplierSchedule";
                                pCo.ObjectIndexValue = i;
                                pCo.OccurenceValue = i + 1;
                                pIsSpecified = CciSwapStream[i].IsFloatingRateMultiplierScheduleSpecified;
                                pIsEnabled = CciSwapStream[i].IsFloatingRateSpecified;
                                break;

                            case CciStream.CciEnum.calculationPeriodAmount_calculation_fixedRateSchedule_initialValue:
                                //pCo.Object = string.Empty;
                                pCo.Object = "calculationPeriodAmountCalculation";
                                pCo.Element = "rateFixedRate";
                                pCo.ObjectIndexValue = i;
                                pCo.OccurenceValue = i + 1;
                                pIsSpecified = CciSwapStream[i].IsFixedRateScheduleSpecified;
                                pIsEnabled = CciSwapStream[i].IsFixedRateSpecified;
                                break;
                            case CciStream.CciEnum.paymentDates_offset:
                                pCo.Object = "paymentDates";
                                // 20090911 EG Add pCo.ObjectIndexValue = i;
                                pCo.ObjectIndexValue = i;
                                pCo.Element = "paymentDaysOffset";
                                pCo.OccurenceValue = i + 1;
                                pIsSpecified = CciSwapStream[i].IsOffsetSpecified;
                                pIsEnabled = true;
                                break;
                            default:
                                isOk = false;
                                break;
                        }
                    }
                }
            }
            #endregion  CciSwapStream[i]
            //
            #region  AdditionalPaymentLength
            if (!isOk)
            {
                for (int i = 0; i < this.AdditionalPaymentLength; i++)
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
            #endregion  AdditionalPaymentLength
            //
            if (!isOk)
                isOk = _cciCancelableProvision.SetButtonZoom(pCci, pCo, ref pIsSpecified, ref pIsEnabled);
            if (!isOk)
                isOk = _cciExtendibleProvision.SetButtonZoom(pCci, pCo, ref pIsSpecified, ref pIsEnabled);
            if (!isOk)
                isOk = _cciEarlyTerminationProvision.SetButtonZoom(pCci, pCo, ref pIsSpecified, ref pIsEnabled);
            return isOk;
            //

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCci"></param>
        /// <param name="pCo"></param>
        /// <param name="pIsSpecified"></param>
        /// <param name="pIsEnabled"></param>
        /// <returns></returns>
        public override bool SetButtonScreenBox(CustomCaptureInfo pCci, CustomObjectButtonScreenBox pCo, ref bool pIsSpecified, ref bool pIsEnabled)
        {
            return false;

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCci"></param>
        /// <param name="pCo"></param>
        public override void SetButtonReferential(CustomCaptureInfo pCci, CustomObjectButtonReferential pCo)
        {

        }
        #endregion Membres de ITradeGetInfoButton

        #region ICciPresentation Membres
        public override void DumpSpecific_ToGUI(CciPageBase pPage)
        {
            for (int i = 0; i < ArrFunc.Count(CciSwapStream); i++)
                CciSwapStream[i].DumpSpecific_ToGUI(pPage);
            //
            base.DumpSpecific_ToGUI(pPage);
        }
        #endregion

        #region Methods
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pProduct"></param>
        public override void SetProduct(IProduct pProduct)
        {

            _swap = (ISwap)pProduct;
            
            IInterestRateStream streamGlobal = null;
            if ((null != _swap) && ArrFunc.IsFilled(_swap.Stream))
                streamGlobal = _swap.Stream[0];
            CciStreamGlobal = new CciStream(CciTrade, Prefix + TradeCustomCaptureInfos.CCst.Prefix_swapStream, -1, streamGlobal);
            
            CciSwapStream = null;
            
            CciAdditionalPayment = null;
            _cciCancelableProvision = new CciCancelableProvision(CciTrade, Prefix);
            _cciExtendibleProvision = new CciExtendibleProvision(CciTrade, Prefix);
            _cciEarlyTerminationProvision = new CciEarlyTerminationProvision(CciTrade, Prefix);
            
            base.SetProduct(pProduct);

        }

        /// <summary>
        /// 
        /// </summary>
        private void InitializeAdditionalPayment_FromCci()
        {

            bool isOk = true;
            int index = -1;
            bool saveSpecified;

            ArrayList lst = new ArrayList();

            saveSpecified = _swap.AdditionalPaymentSpecified;
            lst.Clear();

            while (isOk)
            {
                index += 1;
                CciPayment cciPayment = new CciPayment(CciTrade, index + 1, null, CciPayment.PaymentTypeEnum.Payment, Prefix + TradeCustomCaptureInfos.CCst.Prefix_additionalPayment, string.Empty, string.Empty, string.Empty, CciTrade.CciClientIdMainCurrency, string.Empty);
                isOk = CcisBase.Contains(cciPayment.CciClientId(CciPayment.CciEnumPayment.payer));
                if (isOk)
                {
                    if (ArrFunc.IsEmpty(_swap.AdditionalPayment) || (index == _swap.AdditionalPayment.Length))
                        ReflectionTools.AddItemInArray(_swap, "additionalPayment", index);
                    cciPayment.Payment = _swap.AdditionalPayment[index];
                    lst.Add(cciPayment);
                }
            }

            CciAdditionalPayment = (CciPayment[])lst.ToArray(typeof(CciPayment));
            for (int i = 0; i < this.AdditionalPaymentLength; i++)
                CciAdditionalPayment[i].Initialize_FromCci();

            _swap.AdditionalPaymentSpecified = saveSpecified;
        }

        /// <summary>
        /// 
        /// </summary>
        private void InitializeProvision_FromCci()
        {
            CciTools.CreateInstance((IContainerCci)this, _swap);
            //
            if (null != _cciCancelableProvision)
                _cciCancelableProvision.Initialize_FromCci();
            //
            if (null != _cciExtendibleProvision)
                _cciExtendibleProvision.Initialize_FromCci();
            //
            if (null != _cciEarlyTerminationProvision)
                _cciEarlyTerminationProvision.Initialize_FromCci();

        }

        /// <summary>
        /// 
        /// </summary>
        private void InitializeSwapStream_FromCci()
        {
            bool isOk = true;
            int index = -1;
            //
            ArrayList lst = new ArrayList();
            //
            lst.Clear();
            while (isOk)
            {
                index += 1;
                //
                CciStream cciStreamCurrent = new CciStream(CciTrade, Prefix + TradeCustomCaptureInfos.CCst.Prefix_swapStream, index + 1, null);
                //
                isOk = CcisBase.Contains(cciStreamCurrent.CciClientId(CciStream.CciEnum.payer));
                if (isOk)
                {
                    if (ArrFunc.IsEmpty(_swap.Stream) || (index == _swap.Stream.Length))
                    {
                        ReflectionTools.AddItemInArray(_swap, "swapStream", index);
                        if (ArrFunc.IsFilled(Ccis.TradeCommonInput.FpMLDataDocReader.Party))
                        {
                            _swap.Stream[index].PayerPartyReference.HRef = Ccis.TradeCommonInput.FpMLDataDocReader.Party[0].Id;
                            _swap.Stream[index].ReceiverPartyReference.HRef = string.Empty;
                            if (2 == ArrFunc.Count(Ccis.TradeCommonInput.FpMLDataDocReader.Party))
                                _swap.Stream[index].ReceiverPartyReference.HRef = Ccis.TradeCommonInput.FpMLDataDocReader.Party[1].Id;
                        }
                    }
                    //
                    if (StrFunc.IsEmpty(_swap.Stream[index].Id))
                        _swap.Stream[index].Id = Prefix + TradeCustomCaptureInfos.CCst.Prefix_swapStream + Convert.ToString(index + 1);
                    //					
                    cciStreamCurrent.Irs = _swap.Stream[index];
                    // 
                    lst.Add(cciStreamCurrent);
                }
            }
            CciSwapStream = (CciStream[])lst.ToArray(typeof(CciStream));
            //
            #region génération ds chaque Tradestream des ccis du  CciStreamGlobal
            for (int i = 0; i < CcisBase.Count; i++)
            {
                CustomCaptureInfo cci = CcisBase[i];
                if (CciStreamGlobal.IsCciOfContainer(cci.ClientId_WithoutPrefix))
                {
                    string clientId_Key = CciStreamGlobal.CciContainerKey(cci.ClientId_WithoutPrefix);
                    if (System.Enum.IsDefined(typeof(CciStream.CciEnum), clientId_Key))
                    {
                        for (int j = 0; j < SwapStreamLength; j++)
                            Ccis.CloneGlobalCci(clientId_Key, cci, CciSwapStream[j]);
                    }
                }
            }
            #endregion
            //
            CciStreamGlobal.Initialize_FromCci();
            for (int i = 0; i < SwapStreamLength; i++)
                CciSwapStream[i].Initialize_FromCci();
            //			

        }

        /// <summary>
        /// Synchronisation des BDA des stream à taux fixe (ou non renseigné) en fonction des BDA du 1er stream à taux flottant
        /// <para>Ces derniers sont en général alimentés avec les infos rattachées au rate_Index</para>
        /// <para>Ces Synchros sont réalisées uniquement si les périodicité des streams sont identiques</para>
        /// <para>Cette synchro se fait dès que Spheres® alimente la BDC d'un taux Flottant</para>
        /// </summary>
        /// <param name="pCci">Cci qui est alimenté</param>
        private void SynchronizeBDAFixedRate(CustomCaptureInfo pCci)
        {
            ArrayList alFloatingRateStream = new ArrayList();
            //
            for (int i = 0; i < SwapStreamLength; i++)
            {
                if (CciSwapStream[i].IsFloatingRateSpecified)
                    alFloatingRateStream.Add(i);
            }
            //
            foreach (int streamFloatIndex in alFloatingRateStream)
            {
                for (int i = 0; i < SwapStreamLength; i++)
                {
                    if (i != streamFloatIndex)
                    {
                        if (false == CciSwapStream[i].IsFloatingRateSpecified)
                        {
                            string clientId_WithoutPrefix;
                            if (CciSwapStream[streamFloatIndex].IsCci(CciStream.CciEnum.calculationPeriodDates_calculationPeriodDatesAdjustments_bDC, pCci))
                            {
                                if (0 == ((IInterval)CciSwapStream[i].Irs.CalculationPeriodDates.CalculationPeriodFrequency).CompareTo(CciSwapStream[streamFloatIndex].Irs.CalculationPeriodDates.CalculationPeriodFrequency))
                                {
                                    //CalculationPeriodDates
                                    clientId_WithoutPrefix = CciSwapStream[i].CciClientId(CciStream.CciEnum.calculationPeriodDates_calculationPeriodDatesAdjustments_bDC);
                                    CcisBase[clientId_WithoutPrefix].NewValue = CciSwapStream[streamFloatIndex].Irs.CalculationPeriodDates.CalculationPeriodDatesAdjustments.BusinessDayConvention.ToString();
                                }
                            }

                            if (CciSwapStream[streamFloatIndex].IsCci(CciStream.CciEnum.paymentDates_paymentDatesAdjustments_bDC, pCci))
                            {
                                if (0 == CciSwapStream[i].Irs.PaymentDates.PaymentFrequency.CompareTo(CciSwapStream[streamFloatIndex].Irs.PaymentDates.PaymentFrequency))
                                {
                                    //paymentDates
                                    clientId_WithoutPrefix = CciSwapStream[i].CciClientId(CciStream.CciEnum.paymentDates_paymentDatesAdjustments_bDC);
                                    CcisBase[clientId_WithoutPrefix].NewValue = CciSwapStream[streamFloatIndex].Irs.PaymentDates.PaymentDatesAdjustments.BusinessDayConvention.ToString();
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Alimente le _cciStream avec les valeurs présentes le _cciProductGlogal
        /// </summary>
        /// <param name="pIndex"></param>
        private void SynchronizeStream(int pIndex)
        {
            if (null != CciStreamGlobal)
            {
                for (int i = 0; i < CcisBase.Count; i++)
                {
                    CustomCaptureInfo cci = CcisBase[i];
                    if (CciStreamGlobal.IsCciOfContainer(cci.ClientId_WithoutPrefix))
                    {
                        string clientId_Key = CciStreamGlobal.CciContainerKey(cci.ClientId_WithoutPrefix);
                        string clientId = CciSwapStream[pIndex].CciClientId(clientId_Key);
                        if (CcisBase.Contains(clientId))
                            CciSwapStream[pIndex].Cci(clientId_Key).NewValue = cci.NewValue;
                    }
                }
            }
        }

        /// <summary>
        ///  Applique une synchronisation des payments sur tous les strams à taux fixe vis à vis du 1er taux flottant trouvé
        /// </summary>
        private void SynchronizeFixedRateStream(CustomCaptureInfo pCci)
        {
            int streamFloatIndex = GetIndexFirstStreamFloatingRate();
            if (streamFloatIndex > -1)
            {
                if (IsCci(CciEnum.isSynchronous, pCci) ||
                    CciSwapStream[streamFloatIndex].IsCciOfContainer(pCci.ClientId_WithoutPrefix))
                {
                    for (int i = 0; i < SwapStreamLength; i++)
                    {
                        if (false == CciSwapStream[i].IsFloatingRateSpecified)
                        {
                            SetCalculationPeriodDatesSynchroneous(CciSwapStream[streamFloatIndex], CciSwapStream[i]);
                            SetPaymentDatesSynchroneous(CciSwapStream[streamFloatIndex], CciSwapStream[i]);
                        }
                    }
                }
            }
        }
        /// <summary>
        /// Retourne l'index du 1er stream à taux flottant
        /// </summary>
        /// <returns></returns>
        private int GetIndexFirstStreamFloatingRate()
        {
            int ret = -1;
            for (int i = 0; i < SwapStreamLength; i++)
            {
                if (CciSwapStream[i].IsFloatingRateSpecified)
                {
                    ret = i;
                    break;
                }
            }
            //
            return ret;
        }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="pFloatingRateStream"></param>
        /// <param name="pFixedRateStream"></param>
        private void SetCalculationPeriodDatesSynchroneous(CciStream pFloatingRateStream, CciStream pFixedRateStream)
        {
            IInterestRateStream floatingRateStream = pFloatingRateStream.Irs;
            IInterestRateStream fixedRateStream = pFixedRateStream.Irs;
            //
            //EffectiveDate
            try
            {
                string clientId = pFixedRateStream.CciClientId(CciStream.CciEnum.calculationPeriodDates_effectiveDate);
                if (CcisBase.Contains(clientId))
                {
                    CcisBase.SetNewValue(clientId, string.Empty);
                    if (floatingRateStream.CalculationPeriodDates.EffectiveDateAdjustableSpecified)
                        CcisBase.SetNewValue(clientId, floatingRateStream.CalculationPeriodDates.EffectiveDateAdjustable.UnadjustedDate.Value);
                }
                else
                {
                    fixedRateStream.CalculationPeriodDates.EffectiveDateAdjustableSpecified = false;
                    if (floatingRateStream.CalculationPeriodDates.EffectiveDateAdjustableSpecified)
                    {
                        fixedRateStream.CalculationPeriodDates.EffectiveDateAdjustableSpecified = true;
                        fixedRateStream.CalculationPeriodDates.EffectiveDateAdjustable.UnadjustedDate =
                            Product.ProductBase.CreateAdjustedDate(floatingRateStream.CalculationPeriodDates.EffectiveDateAdjustable.UnadjustedDate.DateValue);
                    }
                }
                //
                string clientIdBDC = pFixedRateStream.CciClientId(CciStream.CciEnum.calculationPeriodDates_effectiveDate_dateAdjustedDate_bDC);
                if (CcisBase.Contains(clientIdBDC))
                {
                    CcisBase.SetNewValue(clientIdBDC, string.Empty);
                    if (floatingRateStream.CalculationPeriodDates.EffectiveDateAdjustableSpecified)
                        CcisBase.SetNewValue(clientIdBDC, floatingRateStream.CalculationPeriodDates.EffectiveDateAdjustable.DateAdjustments.BusinessDayConvention.ToString());
                }
                else
                {
                    if (floatingRateStream.CalculationPeriodDates.EffectiveDateAdjustableSpecified)
                    {
                        fixedRateStream.CalculationPeriodDates.EffectiveDateAdjustable.DateAdjustments =
                            Tools.CreateBusinessDayAdjustmentsFromBusinessDayAdjustments(Product.ProductBase, floatingRateStream.CalculationPeriodDates.EffectiveDateAdjustable.DateAdjustments);
                    }
                }
            }
            finally { }
            //
            //TerminationDate
            try
            {
                string clientId = pFixedRateStream.CciClientId(CciStream.CciEnum.calculationPeriodDates_terminationDate);
                if (CcisBase.Contains(clientId))
                {
                    CcisBase.SetNewValue(clientId, string.Empty);
                    if (floatingRateStream.CalculationPeriodDates.TerminationDateAdjustableSpecified)
                        CcisBase.SetNewValue(clientId, floatingRateStream.CalculationPeriodDates.TerminationDateAdjustable.UnadjustedDate.Value);
                }
                else
                {
                    fixedRateStream.CalculationPeriodDates.TerminationDateAdjustableSpecified = false;
                    if (floatingRateStream.CalculationPeriodDates.TerminationDateAdjustableSpecified)
                    {
                        fixedRateStream.CalculationPeriodDates.TerminationDateAdjustableSpecified = true;
                        fixedRateStream.CalculationPeriodDates.TerminationDateAdjustable.UnadjustedDate =
                            Product.ProductBase.CreateAdjustedDate(floatingRateStream.CalculationPeriodDates.TerminationDateAdjustable.UnadjustedDate.DateValue);
                    }
                }
                //
                string clientIdBDC = pFixedRateStream.CciClientId(CciStream.CciEnum.calculationPeriodDates_terminationDate_dateAdjustedDate_bDC);
                if (CcisBase.Contains(clientIdBDC))
                {
                    CcisBase.SetNewValue(clientIdBDC, string.Empty);
                    if (floatingRateStream.CalculationPeriodDates.TerminationDateAdjustableSpecified)
                        CcisBase.SetNewValue(clientIdBDC, floatingRateStream.CalculationPeriodDates.TerminationDateAdjustable.DateAdjustments.BusinessDayConvention.ToString());
                }
                else
                {
                    if (floatingRateStream.CalculationPeriodDates.TerminationDateAdjustableSpecified)
                    {
                        fixedRateStream.CalculationPeriodDates.TerminationDateAdjustable.DateAdjustments =
                            Tools.CreateBusinessDayAdjustmentsFromBusinessDayAdjustments(Product.ProductBase, floatingRateStream.CalculationPeriodDates.TerminationDateAdjustable.DateAdjustments);
                    }
                }
            }
            finally { }
            //
            //FirstRegularPeriodStartDate
            try
            {
                string clientId = pFixedRateStream.CciClientId(CciStream.CciEnum.calculationPeriodDates_firstRegularPeriodStartDate);
                if (CcisBase.Contains(clientId))
                {
                    CcisBase.SetNewValue(clientId, string.Empty);
                    if (floatingRateStream.CalculationPeriodDates.FirstRegularPeriodStartDateSpecified)
                        CcisBase.SetNewValue(clientId, floatingRateStream.CalculationPeriodDates.FirstRegularPeriodStartDate.Value);
                }
                else
                {
                    fixedRateStream.CalculationPeriodDates.FirstRegularPeriodStartDateSpecified = floatingRateStream.CalculationPeriodDates.FirstRegularPeriodStartDateSpecified;
                    if (fixedRateStream.CalculationPeriodDates.FirstRegularPeriodStartDateSpecified)
                    {
                        fixedRateStream.CalculationPeriodDates.FirstRegularPeriodStartDate =
                            new EFS_Date(floatingRateStream.CalculationPeriodDates.FirstRegularPeriodStartDate.Value);
                    }
                }
            }
            finally { }
            //
            //LastRegularPeriodStartDate
            try
            {
                string clientId = pFixedRateStream.CciClientId(CciStream.CciEnum.calculationPeriodDates_lastRegularPeriodEndDate);
                if (CcisBase.Contains(clientId))
                {
                    CcisBase.SetNewValue(clientId, string.Empty);
                    if (floatingRateStream.CalculationPeriodDates.LastRegularPeriodEndDateSpecified)
                        CcisBase.SetNewValue(clientId, floatingRateStream.CalculationPeriodDates.LastRegularPeriodEndDate.Value);
                }
                else
                {
                    fixedRateStream.CalculationPeriodDates.LastRegularPeriodEndDateSpecified =
                            floatingRateStream.CalculationPeriodDates.LastRegularPeriodEndDateSpecified;
                    if (fixedRateStream.CalculationPeriodDates.LastRegularPeriodEndDateSpecified)
                    {
                        fixedRateStream.CalculationPeriodDates.LastRegularPeriodEndDate =
                            new EFS_Date(floatingRateStream.CalculationPeriodDates.LastRegularPeriodEndDate.Value);
                    }
                }
            }
            finally { }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pFloatingRateStream"></param>
        /// <param name="pFixedRateStream"></param>
        private void SetPaymentDatesSynchroneous(CciStream pFloatingRateStream, CciStream pFixedRateStream)
        {
            IInterestRateStream floatingRateStream = pFloatingRateStream.Irs;
            IInterestRateStream fixedRateStream = pFixedRateStream.Irs;
            //
            //paymentDatesAdjustments
            try
            {
                string clientId = pFixedRateStream.CciClientId(CciStream.CciEnum.paymentDates_paymentDatesAdjustments_bDC);
                if (CcisBase.Contains(clientId))
                    CcisBase.SetNewValue(clientId, floatingRateStream.PaymentDates.PaymentDatesAdjustments.BusinessDayConvention.ToString());
                else
                {
                    fixedRateStream.PaymentDates.PaymentDatesAdjustments =
                        Tools.CreateBusinessDayAdjustmentsFromBusinessDayAdjustments(Product.ProductBase, floatingRateStream.PaymentDates.PaymentDatesAdjustments);
                }
            }
            finally { }
            //
            //paymentDatesOffset
            try
            {
                fixedRateStream.PaymentDates.PaymentDaysOffsetSpecified = false;
                if (floatingRateStream.PaymentDates.PaymentDaysOffsetSpecified)
                {
                    fixedRateStream.PaymentDates.PaymentDaysOffsetSpecified = true;
                    //
                    IOffset offset = floatingRateStream.PaymentDates.PaymentDaysOffset;
                    fixedRateStream.PaymentDates.PaymentDaysOffset =
                        Product.ProductBase.CreateOffset(offset.Period, offset.PeriodMultiplier.IntValue, offset.DayType);
                }
            }
            finally { }

            //payRelativeTo
            try
            {
                string clientId = pFixedRateStream.CciClientId(CciStream.CciEnum.paymentDates_payRelativeToReverse);
                if (CcisBase.Contains(clientId))
                {
                    string data;
                    if (floatingRateStream.PaymentDates.PayRelativeTo == FpML.Enum.PayRelativeToEnum.CalculationPeriodStartDate)
                        data = Cst.FpML_Boolean_True;
                    else
                        data = Cst.FpML_Boolean_False;
                    CcisBase.SetNewValue(clientId, data);
                }
                else
                {
                    fixedRateStream.PaymentDates.PayRelativeTo = floatingRateStream.PaymentDates.PayRelativeTo;
                }
            }
            finally { }

            //firstPaymentDate
            try
            {
                string clientId = pFixedRateStream.CciClientId(CciStream.CciEnum.paymentDates_firstPaymentDate);
                if (CcisBase.Contains(clientId))
                {
                    CcisBase.SetNewValue(clientId, string.Empty);
                    if (floatingRateStream.PaymentDates.FirstPaymentDateSpecified)
                        CcisBase.SetNewValue(clientId, floatingRateStream.PaymentDates.FirstPaymentDate.Value);
                }
                else
                {
                    fixedRateStream.PaymentDates.FirstPaymentDateSpecified = false;
                    if (floatingRateStream.PaymentDates.FirstPaymentDateSpecified)
                    {
                        fixedRateStream.PaymentDates.FirstPaymentDateSpecified = true;
                        fixedRateStream.PaymentDates.FirstPaymentDate = 
                            new EFS_Date(fixedRateStream.PaymentDates.FirstPaymentDate.Value);  
                    }
                }
            }
            finally { }

            //lastRegularPaymentDate
            try
            {
                fixedRateStream.PaymentDates.LastRegularPaymentDateSpecified = false;
                if (floatingRateStream.PaymentDates.LastRegularPaymentDateSpecified)
                {
                    fixedRateStream.PaymentDates.LastRegularPaymentDateSpecified = true;
                    fixedRateStream.PaymentDates.LastRegularPaymentDate =
                        new EFS_Date(fixedRateStream.PaymentDates.LastRegularPaymentDate.Value);
                }
            }
            finally { }
        }
        #endregion
    }
  
}
