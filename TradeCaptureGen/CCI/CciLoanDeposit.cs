#region Using Directives
using EFS.ACommon;
using EFS.Common;
using EFS.Common.Web;
using EFS.GUI.CCI;
using EfsML.Interface;
using FpML.Interface;
using System;
using System.Collections;
using System.Linq;
using System.Reflection;
#endregion Using Directives

namespace EFS.TradeInformation
{
    #region CciLoanDeposit
    /// <summary>
    /// Description résumée de cciLoanDeposit.
    /// </summary>
    public class CciLoanDeposit : IContainerCci, IContainerCciFactory, IContainerCciGetInfoButton, IContainerCciQuoteBasis, ICciPresentation 
    {
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
        #region Members
        private readonly TradeCustomCaptureInfos ccis;
        private readonly CciTrade cciTrade;
        private readonly ILoanDeposit loanDeposit;
        private readonly string prefix;
        private readonly CciStream cciStreamGlobal; //Represente les ccis dits globaux
        public CciStream[] cciStream;
        public CciPayment[] cciAdditionalPayment;
        private readonly CciCancelableProvision cciCancelableProvision;
        private readonly CciExtendibleProvision cciExtendibleProvision;
        private readonly CciEarlyTerminationProvision cciEarlyTerminationProvision;
        #endregion Members
        #region Accessors
        #region AdditionalPaymentLength
        public int AdditionalPaymentLength
        {
            get { return ArrFunc.IsFilled(cciAdditionalPayment) ? cciAdditionalPayment.Length : 0; }
        }
        #endregion AdditionalPaymentLength
        #region LoanDepositStreamLength
        public int LoanDepositStreamLength
        {
            get { return ArrFunc.IsFilled(cciStream) ? cciStream.Length : 0; }
        }
        #endregion LoanDepositStreamLength
        #endregion Accessors
        #region Constructors
        public CciLoanDeposit(CciTrade pTrade, ILoanDeposit pLoanDeposit) : this(pTrade, pLoanDeposit, string.Empty) { }
        public CciLoanDeposit(CciTrade pTrade, ILoanDeposit pLoanDeposit, string pPrefix)
        {
            cciTrade = pTrade;
            ccis = cciTrade.Ccis;
            loanDeposit = pLoanDeposit;
            prefix = pPrefix;
            if (StrFunc.IsFilled(prefix))
                prefix += CustomObject.KEY_SEPARATOR;
            //
            cciStreamGlobal = new CciStream(pTrade, prefix + TradeCustomCaptureInfos.CCst.Prefix_loanDepositStream, -1, loanDeposit.Stream[0]);
            cciStream = null;
            cciCancelableProvision = new CciCancelableProvision(cciTrade, prefix);
            cciExtendibleProvision = new CciExtendibleProvision(cciTrade, prefix);
            cciEarlyTerminationProvision = new CciEarlyTerminationProvision(cciTrade, prefix);
        }
        #endregion Constructors
        #region Interfaces
        #region IContainerCciPayerReceiver Members
        #region CciClientIdPayer
        public string CciClientIdPayer
        {
            get
            {
                int j = 0;
                for (int i = 0; i < LoanDepositStreamLength; i++)
                {
                    if (false == cciStream[i].IsFloatingRateSpecified)
                    {
                        j = i;
                        break;
                    }
                }
                return cciStream[j].CciClientIdPayer;
            }
        }
        #endregion CciClientIdPayer
        #region CciClientIdReceiver
        public string CciClientIdReceiver
        {
            get
            {
                int j = 0;
                for (int i = 0; i < LoanDepositStreamLength; i++)
                {
                    if (false == cciStream[i].IsFloatingRateSpecified)
                    {
                        j = i;
                        break;
                    }
                }
                return cciStream[j].CciClientIdReceiver;
            }
        }
        #endregion CciClientIdReceiver
        #region SynchronizePayerReceiver
        public void SynchronizePayerReceiver(string pLastValue, string pNewValue)
        {
            for (int i = 0; i < cciStream.Length; i++)
                cciStream[i].SynchronizePayerReceiver(pLastValue, pNewValue);

            for (int i = 0; i < AdditionalPaymentLength; i++)
                cciStream[i].SynchronizePayerReceiver(pLastValue, pNewValue);
        }
        #endregion
        #endregion IContainerCciPayerReceiver Members
        #region IContainerCciFactory Members
        #region AddCciSystem
        /// <summary>
        /// 
        /// </summary>
        /// FI 20170116 [21916] Modify (use AddCciSystem Method)
        public void AddCciSystem()
        {

            CciTools.AddCciSystem(ccis, Cst.BUT + cciStreamGlobal.CciClientId(CciStream.CciEnum.calculationPeriodAmount_calculation_notionalSchedule_notionalStepParameters.ToString()), false, TypeData.TypeDataEnum.@string);
            
            for (int i = 0; i < LoanDepositStreamLength; i++)
                cciStream[i].AddCciSystem();
            
            for (int i = 0; i < AdditionalPaymentLength; i++)
                cciAdditionalPayment[i].AddCciSystem();
        }
        #endregion AddCciSystem
        #region CleanUp
        public void CleanUp()
        {
            try
            {
                for (int i = 0; i < LoanDepositStreamLength; i++)
                    cciStream[i].CleanUp();
                //
                for (int i = 0; i < AdditionalPaymentLength; i++)
                    cciAdditionalPayment[i].CleanUp();
                //
                // Suppression des streams issus du paramétrage screen et non alimenté
                if (ArrFunc.IsFilled(loanDeposit.Stream))
                {
                    for (int i = loanDeposit.Stream.Length - 1; -1 < i; i--)
                    {
                        if (false == CaptureTools.IsDocumentElementValid(loanDeposit.Stream[i].PayerPartyReference.HRef))
                            ReflectionTools.RemoveItemInArray(loanDeposit, "loanDepositStream", i);
                    }
                }
                //
                // Suppression des additionalPayment issus du paramétrage screen et non alimenté
                if (ArrFunc.IsFilled(loanDeposit.AdditionalPayment))
                {
                    for (int i = loanDeposit.AdditionalPayment.Length - 1; -1 < i; i--)
                    {

                        if (false == CaptureTools.IsDocumentElementValid(loanDeposit.AdditionalPayment[i].PayerPartyReference.HRef))
                            ReflectionTools.RemoveItemInArray(loanDeposit, "additionalPayment", i);
                    }
                }
                loanDeposit.AdditionalPaymentSpecified = (ArrFunc.IsFilled(loanDeposit.AdditionalPayment) && (0 < loanDeposit.AdditionalPayment.Length));
                //
            }
            catch (Exception ex) { throw new SpheresException2(MethodInfo.GetCurrentMethod().Name, ex); }
        }
        #endregion CleanUp
        #region Dump_ToDocument
        public void Dump_ToDocument()
        {
            foreach (string clientId in ccis.ClientId_DumpToDocument.Where(x => IsCciOfContainer(x)))
            {
                string cliendId_Key = CciContainerKey(clientId);
                if (Enum.IsDefined(typeof(CciEnum), cliendId_Key))
                {
                    CustomCaptureInfo cci = ccis[clientId];
                    CciEnum cciEnum = (CciEnum)System.Enum.Parse(typeof(CciEnum), cliendId_Key);
                
                    #region Reset Variables
                    CustomCaptureInfosBase.ProcessQueueEnum processQueue = CustomCaptureInfosBase.ProcessQueueEnum.None;
                    string data = cci.NewValue;
                    bool isSetting = true;
                    #endregion Reset Variables


                    switch (cciEnum)
                    {
                        case CciEnum.calculationPeriodDates_effectiveMinDate:
                            processQueue = CustomCaptureInfosBase.ProcessQueueEnum.High;
                            break;
                        case CciEnum.calculationPeriodDates_terminationMaxDate:
                            processQueue = CustomCaptureInfosBase.ProcessQueueEnum.High;
                            break;
                        default:
                            isSetting = false;

                            break;
                    }
                    if (isSetting)
                        ccis.Finalize(cci.ClientId_WithoutPrefix, processQueue);

                }
            }
            #region Synchronize tradeStreamGlobal from cciLoanDeposit[0]
            // Permet de conserver en phase le stream global et tradeLoanDeposit[0] pour les ccis existants dans les 2 objects
            // Ex il existe loanDeposit_payer et loanDeposit1.payer
            foreach (CustomCaptureInfo cci in ccis)
            {
                if (cci.HasChanged && cciStream[0].IsCciOfContainer(cci.ClientId_WithoutPrefix))
                {
                    string clientId_Key = cciStream[0].CciContainerKey(cci.ClientId_WithoutPrefix);
                    CciStream.CciEnum cciEnum = CciStream.CciEnum.unknown;
                    if (System.Enum.IsDefined(typeof(CciStream.CciEnum), clientId_Key))
                    {
                        cciEnum = (CciStream.CciEnum)System.Enum.Parse(typeof(CciStream.CciEnum), clientId_Key);
                        if (ccis.Contains(cciStreamGlobal.CciClientId(cciEnum)))
                            cciStreamGlobal.Cci(cciEnum).NewValue = cci.NewValue;
                    }
                }
            }
            #endregion Synchronize tradeStreamGlobal from cciLoanDeposit[0]
            #region synchronize cciLoanDeposit[i] from  tradeStreamGlobal (exclude Payer/receiver)
            foreach (CustomCaptureInfo cci in ccis)
            {
                if (cci.HasChanged && cciStreamGlobal.IsCciOfContainer(cci.ClientId_WithoutPrefix))
                {
                    string clientId_Key = cciStreamGlobal.CciContainerKey(cci.ClientId_WithoutPrefix);
                    CciStream.CciEnum cciEnum = CciStream.CciEnum.unknown;
                    if (System.Enum.IsDefined(typeof(CciStream.CciEnum), clientId_Key))
                    {
                        cciEnum = (CciStream.CciEnum)System.Enum.Parse(typeof(CciStream.CciEnum), clientId_Key);
                        if ((CciStream.CciEnum.receiver != cciEnum) && (CciStream.CciEnum.payer != cciEnum))
                        {
                            for (int i = 0; i < LoanDepositStreamLength; i++)
                                cciStream[i].Cci(cciEnum).NewValue = cci.NewValue;
                        }
                    }
                }
            }
            #endregion synchronize cciLoanDeposit[i] from  tradeStreamGlobal (exclude Payer/receiver)
            
            cciStreamGlobal.Dump_ToDocument();
            
            for (int i = 0; i < LoanDepositStreamLength; i++)
                cciStream[i].Dump_ToDocument();
            
            if (Cst.Capture.IsModeInput(ccis.CaptureMode) && (false == Cst.Capture.IsModeAction(ccis.CaptureMode)))
                ccis.InitializePaymentPaymentQuoteRelativeTo(cciStream[0], null, cciAdditionalPayment, cciTrade.cciOtherPartyPayment);
            
            //AdditionalPayment
            for (int i = 0; i < AdditionalPaymentLength; i++)
                cciAdditionalPayment[i].Dump_ToDocument();
            loanDeposit.AdditionalPaymentSpecified = CciTools.Dump_IsCciContainerArraySpecified(loanDeposit.AdditionalPaymentSpecified, cciAdditionalPayment);
         
            if (null != cciCancelableProvision)
                cciCancelableProvision.Dump_ToDocument();
            if (null != cciExtendibleProvision)
                cciExtendibleProvision.Dump_ToDocument();
            if (null != cciEarlyTerminationProvision)
                cciEarlyTerminationProvision.Dump_ToDocument();

        }
        #endregion Dump_ToDocument
        #region GetArrayElementDocumentCount
        public int GetArrayElementDocumentCount(string pPrefix)
        {
            int ret = -1;
            if (-1 == ret)
            {
                if (TradeCustomCaptureInfos.CCst.Prefix_loanDepositStream == pPrefix)
                    ret = ArrFunc.Count(loanDeposit.Stream);
            }
            if (-1 == ret)
            {
                if (TradeCustomCaptureInfos.CCst.Prefix_additionalPayment == pPrefix)
                    ret = ArrFunc.Count(loanDeposit.AdditionalPayment);
            }
            return ret;
        }
        #endregion GetArrayElementDocumentCount
        #region Initialize_Document
        public void Initialize_Document()
        {
            try
            {
                if (Cst.Capture.IsModeNew(ccis.CaptureMode) && (false == ccis.IsPreserveData))
                {
                    string id = string.Empty;
                    //
                    for (int i = 0; i < LoanDepositStreamLength; i++)
                    {
                        if (cciStream[i].Cci(CciStream.CciEnum.payer).IsMandatory &&
                            cciStream[i].Cci(CciStream.CciEnum.receiver).IsMandatory)
                        {
                            if (StrFunc.IsEmpty(loanDeposit.Stream[i].PayerPartyReference.HRef) &&
                                StrFunc.IsEmpty(loanDeposit.Stream[i].ReceiverPartyReference.HRef))
                            {
                                if (1 == i)
                                {
                                    loanDeposit.Stream[i].PayerPartyReference.HRef = loanDeposit.Stream[i - 1].PayerPartyReference.HRef;
                                    loanDeposit.Stream[i].ReceiverPartyReference.HRef = loanDeposit.Stream[i - 1].ReceiverPartyReference.HRef;
                                }
                                else
                                {
                                    if (StrFunc.IsEmpty(id))
                                        id = StrFunc.IsFilled(id) ? id : TradeCustomCaptureInfos.PartyUnknown;
                                    loanDeposit.Stream[i].PayerPartyReference.HRef = id;
                                }
                            }
                        }
                    }
                    //
                    for (int i = 0; i < AdditionalPaymentLength; i++)
                    {
                        if ((cciAdditionalPayment[i].Cci(CciPayment.CciEnum.payer).IsMandatory) &&
                            (cciAdditionalPayment[i].Cci(CciPayment.CciEnum.receiver).IsMandatory))
                        {
                            if (StrFunc.IsEmpty(loanDeposit.AdditionalPayment[i].PayerPartyReference.HRef) &&
                                StrFunc.IsEmpty(loanDeposit.AdditionalPayment[i].ReceiverPartyReference.HRef))
                            {
                                loanDeposit.AdditionalPayment[i].PayerPartyReference.HRef = loanDeposit.Stream[0].PayerPartyReference.HRef;
                                loanDeposit.AdditionalPayment[i].ReceiverPartyReference.HRef = loanDeposit.Stream[0].ReceiverPartyReference.HRef;
                            }
                        }
                    }
                    //
                    if (TradeCustomCaptureInfos.PartyUnknown == id)
                        cciTrade.AddPartyUnknown();
                }
            }
            catch (Exception ex) { throw new SpheresException2(MethodInfo.GetCurrentMethod().Name, ex); }
        }
        #endregion Initialize_Document
        #region Initialize_FromCci
        public void Initialize_FromCci()
        {
            InitializeLoanDepositStream_FromCci();
            InitializeAdditionalPayment_FromCci();
            InitializeProvision_FromCci();
        }
        #endregion Initialize_FromCci
        #region Initialize_FromDocument
        public void Initialize_FromDocument()
        {
            foreach (CciEnum cciEnum in Enum.GetValues(typeof(CciEnum)))
            {
                CustomCaptureInfo cci = Cci(cciEnum);
                if (cci != null)
                {
                    string data = string.Empty;
                    Boolean isSetting = true;
                    SQL_Table sql_Table = null;

                    switch (cciEnum)
                    {
                        #region calculationPeriodDates_effectiveMinDate & calculationPeriodDates_terminationMaxDate
                        case CciEnum.calculationPeriodDates_effectiveMinDate:
                            data = loanDeposit.Stream[0].CalculationPeriodDates.EffectiveDateAdjustable.UnadjustedDate.Value;
                            break;
                        case CciEnum.calculationPeriodDates_terminationMaxDate:
                            for (int i = loanDeposit.Stream.Length - 1; i > -1; i--)
                            {
                                //On ne récupère pas la termination date de la dernière jambe
                                //En création lorsqu'il y a ajout d'un stream, la date terminationMaxDate est initialisée à blanc (On pert la saisie déjà effectuée)
                                if (DtFunc.IsDateTimeFilled(loanDeposit.Stream[i].CalculationPeriodDates.TerminationDateAdjustable.UnadjustedDate.DateValue))
                                {
                                    data = loanDeposit.Stream[i].CalculationPeriodDates.TerminationDateAdjustable.UnadjustedDate.Value;
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
                        ccis.InitializeCci(cci, sql_Table, data);
                }
            }

            cciStreamGlobal.Initialize_FromDocument();

            for (int i = 0; i < LoanDepositStreamLength; i++)
                cciStream[i].Initialize_FromDocument();

            for (int i = 0; i < AdditionalPaymentLength; i++)
                cciAdditionalPayment[i].Initialize_FromDocument();

            if (null != cciCancelableProvision)
                cciCancelableProvision.Initialize_FromDocument();
            if (null != cciExtendibleProvision)
                cciExtendibleProvision.Initialize_FromDocument();
            if (null != cciEarlyTerminationProvision)
                cciEarlyTerminationProvision.Initialize_FromDocument();

        }
        #endregion Initialize_FromDocument
        #region IsClientId_PayerOrReceiver
        public bool IsClientId_PayerOrReceiver(CustomCaptureInfo pCci)
        {
            bool isOk = cciStreamGlobal.IsClientId_PayerOrReceiver(pCci);
            if (false == isOk)
            {
                for (int i = 0; i < LoanDepositStreamLength; i++)
                {
                    isOk = isOk || cciStream[i].IsClientId_PayerOrReceiver(pCci);
                    if (isOk) break;
                }
            }
            //
            if (false == isOk)
            {
                for (int i = 0; i < AdditionalPaymentLength; i++)
                {
                    isOk = cciAdditionalPayment[i].IsClientId_PayerOrReceiver(pCci);
                    if (isOk)
                        break;
                }
            }
            //
            if (false == isOk)
            {
                if (null != cciCancelableProvision)
                    isOk = cciCancelableProvision.IsClientId_PayerOrReceiver(pCci);
            }
            if (false == isOk)
            {
                if (null != cciExtendibleProvision)
                    isOk = cciExtendibleProvision.IsClientId_PayerOrReceiver(pCci);
            }
            if (false == isOk)
            {
                if (null != cciEarlyTerminationProvision)
                    cciEarlyTerminationProvision.IsClientId_PayerOrReceiver(pCci);
            }
            return isOk;
        }
        #endregion IsClientId_PayerOrReceiver
        #region ProcessInitialize
        public void ProcessInitialize(CustomCaptureInfo pCci)
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
                        //ccis.SetNewValue(cciStream[0].CciClientId(CciStream.CciEnum.calculationPeriodDates_effectiveDate), pCci.NewValue, true);
                        //ccis.SetNewValue(cciStream[1].CciClientId(CciStream.CciEnum.calculationPeriodDates_effectiveDate), pCci.NewValue, true);
                        ccis.SetNewValue(cciStream[0].CciClientId(CciStream.CciEnum.calculationPeriodDates_effectiveDate), pCci.NewValue, false);
                        ccis.SetNewValue(cciStream[1].CciClientId(CciStream.CciEnum.calculationPeriodDates_effectiveDate), pCci.NewValue, false);
                        break;
                    case CciEnum.calculationPeriodDates_terminationMaxDate:
                        Math.DivRem(LoanDepositStreamLength, 2, out int k);
                        if (0 == k)
                        {
                            // FI 20161214 [21916] Modification afin d'éviter toute dégradation puisqu'avant correction de la méthode SetNewValue la valeur True sur le paramètre était equivalent à false
                            //ccis.SetNewValue(cciStream[LoanDepositStreamLength - 2].CciClientId(CciStream.CciEnum.calculationPeriodDates_terminationDate), pCci.NewValue, true);
                            ccis.SetNewValue(cciStream[LoanDepositStreamLength - 2].CciClientId(CciStream.CciEnum.calculationPeriodDates_terminationDate), pCci.NewValue, false);
                        }
                        // FI 20161214 [21916] Modification afin d'éviter toute dégradation puisqu'avant correction de la méthode SetNewValue la valeur True sur le paramètre était equivalent à false    
                        //ccis.SetNewValue(cciStream[LoanDepositStreamLength - 1].CciClientId(CciStream.CciEnum.calculationPeriodDates_terminationDate), pCci.NewValue, true);
                        ccis.SetNewValue(cciStream[LoanDepositStreamLength - 1].CciClientId(CciStream.CciEnum.calculationPeriodDates_terminationDate), pCci.NewValue, false);
                        break;
                }
            }
            
            cciStreamGlobal.ProcessInitialize(pCci);
            
            for (int i = 0; i < LoanDepositStreamLength; i++)
                cciStream[i].ProcessInitialize(pCci);
            
            SynchronizeBDAFixedRate(pCci);
            
            for (int i = 0; i < AdditionalPaymentLength; i++)
                cciAdditionalPayment[i].ProcessInitialize(pCci);
            
            if (null != cciCancelableProvision)
                cciCancelableProvision.ProcessInitialize(pCci);
            if (null != cciExtendibleProvision)
                cciExtendibleProvision.ProcessInitialize(pCci);
            if (null != cciEarlyTerminationProvision)
                cciEarlyTerminationProvision.ProcessInitialize(pCci);
        }
        #endregion ProcessInitialize
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
        #region RefreshCciEnabled
        public void RefreshCciEnabled()
        {
            cciStreamGlobal.RefreshCciEnabled();
            for (int i = 0; i < LoanDepositStreamLength; i++)
                cciStream[i].RefreshCciEnabled();
            for (int i = 0; i < AdditionalPaymentLength; i++)
                cciAdditionalPayment[i].RefreshCciEnabled();
        }
        #endregion RefreshCciEnabled
        #region RemoveLastItemInArray
        public void RemoveLastItemInArray(string pPrefix)
        {
            RemoveLastItemInAdditionalPaymentArray(pPrefix);
        }
        #endregion RemoveLastItemInArray
        #region SetDisplay
        public void SetDisplay(CustomCaptureInfo pCci)
        {
            cciStreamGlobal.SetDisplay(pCci);
            for (int i = 0; i < LoanDepositStreamLength; i++)
                cciStream[i].SetDisplay(pCci);

            for (int i = 0; i < AdditionalPaymentLength; i++)
                cciAdditionalPayment[i].SetDisplay(pCci);

            if (null != cciCancelableProvision)
                cciCancelableProvision.SetDisplay(pCci);
            if (null != cciExtendibleProvision)
                cciCancelableProvision.SetDisplay(pCci);
            if (null != cciEarlyTerminationProvision)
                cciEarlyTerminationProvision.SetDisplay(pCci);
        }
        #endregion SetDisplay
        #endregion IContainerCciFactory Members
        #region IContainerCci Members
        #region Cci
        public CustomCaptureInfo Cci(CciEnum pEnumValue)
        {
            return ccis[CciClientId(pEnumValue.ToString())];
        }
        public CustomCaptureInfo Cci(string pClientId_Key)
        {
            return ccis[CciClientId(pClientId_Key)];
        }
        #endregion Cci
        #region CciClientId
        public string CciClientId(CciEnum pEnumValue)
        {
            return CciClientId(pEnumValue.ToString());
        }
        public string CciClientId(string pClientId_Key)
        {
            return prefix + pClientId_Key;
        }
        #endregion CciClientId
        #region CciContainerKey
        public string CciContainerKey(string pClientId_WithoutPrefix)
        {
            return pClientId_WithoutPrefix.Substring(prefix.Length);
        }
        #endregion CciContainerKey
        #region IsCci
        public bool IsCci(CciEnum pEnumValue, CustomCaptureInfo pCci)
        {
            return (this.CciClientId(pEnumValue) == pCci.ClientId_WithoutPrefix);
        }
        #endregion IsCci
        #region IsCciOfContainer
        public bool IsCciOfContainer(string pClientId_WithoutPrefix)
        {
            return pClientId_WithoutPrefix.StartsWith(prefix);
        }
        #endregion IsCciOfContainer
        #endregion IContainerCci Members
        #region IContainerCciQuoteBasis Members
        #region GetBaseCurrency
        public string GetBaseCurrency(CustomCaptureInfo pCci)
        {
            return string.Empty;
        }
        #endregion GetBaseCurrency
        #region GetCurrency1
        public string GetCurrency1(CustomCaptureInfo pCci)
        {
            string currency = string.Empty;
            for (int i = 0; i < AdditionalPaymentLength; i++)
            {
                currency = cciAdditionalPayment[i].GetCurrency1(pCci);
                if (StrFunc.IsFilled(currency))
                    break;
            }
            return currency;
        }
        #endregion GetCurrency1
        #region GetCurrency2
        public string GetCurrency2(CustomCaptureInfo pCci)
        {
            string currency = string.Empty;
            for (int i = 0; i < AdditionalPaymentLength; i++)
            {
                currency = cciAdditionalPayment[i].GetCurrency2(pCci);
                if (StrFunc.IsFilled(currency))
                    break;
            }
            return currency;
        }
        #endregion GetCurrency2
        #region IsClientId_QuoteBasis
        public bool IsClientId_QuoteBasis(CustomCaptureInfo pCci)
        {
            bool isClientId_QuoteBasis = false;
            for (int i = 0; i < AdditionalPaymentLength; i++)
            {
                isClientId_QuoteBasis = cciAdditionalPayment[i].IsClientId_QuoteBasis(pCci);
                if (isClientId_QuoteBasis)
                    break;
            }
            return isClientId_QuoteBasis;
        }
        #endregion IsClientId_QuoteBasis
        #endregion IContainerCciQuoteBasis Members
        #region ICciGetInfoButton Members
        #region SetButtonReferential
        public void SetButtonReferential(CustomCaptureInfo pCci, CustomObjectButtonReferential pCo)
        {
        }
        #endregion SetButtonReferential
        #region SetButtonScreenBox
        public bool SetButtonScreenBox(CustomCaptureInfo pCci, CustomObjectButtonScreenBox pCo, ref bool pIsSpecified, ref bool pIsEnabled)
        {
            return false;
        }
        #endregion SetButtonScreenBox
        #region SetButtonZoom
        public bool SetButtonZoom(CustomCaptureInfo pCci, CustomObjectButtonFpmlObject pCo, ref bool pIsSpecified, ref bool pIsEnabled)
        {
            try
            {
                bool isOk = false;
                //
                #region Button on StreamGlobal
                if (cciStreamGlobal.IsCciOfContainer(pCci.ClientId_WithoutPrefix))
                {
                    isOk = cciStreamGlobal.IsCci(CciStream.CciEnum.calculationPeriodAmount_calculation_notionalSchedule_notionalStepSchedule_initialValue, pCci);
                    if (isOk)
                    {
                        pCo.Object = "calculationNotional";
                        pCo.Element = "notionalStepSchedule";
                        pCo.OccurenceValue = 1;
                        pCo.CopyTo = "All";
                        pIsSpecified = cciStreamGlobal.IsNotionalStepScheduleStepSpecified;
                        pIsEnabled = true;
                    }
                    //
                    if (false == isOk)
                    {
                        isOk = cciStreamGlobal.IsCci(CciStream.CciEnum.calculationPeriodAmount_calculation_notionalSchedule_notionalStepParameters, pCci);
                        if (isOk)
                        {
                            pCo.Object = "calculationNotional";
                            pCo.Element = "notionalStepParameters";
                            pCo.OccurenceValue = 1;
                            pCo.CopyTo = "All";
                            pIsSpecified = cciStreamGlobal.IsNotionalStepParametersSpecified;
                            pIsEnabled = true;
                        }
                    }

                    if (false == isOk)
                    {
                        isOk = cciStreamGlobal.IsCci(CciStream.CciEnum.calculationPeriodAmount_knownAmountSchedule_initialValue, pCci);
                        if (isOk)
                        {
                            isOk = true;
                            pCo.Object = "";
                            pCo.Element = "calculationPeriodAmountKnownAmountSchedule";
                            pCo.OccurenceValue = 1;
                            pCo.CopyTo = "All";
                            pIsSpecified = cciStreamGlobal.IsKnownAmountScheduleStepSpecified;
                            pIsEnabled = true;
                        }
                    }
                    //
                    if (false == isOk)
                    {
                        isOk = cciStreamGlobal.IsCci(CciStream.CciEnum.calculationPeriodAmount_calculation_notionalSchedule_notionalStepParameters, pCci);
                        if (isOk)
                        {
                            isOk = true;
                            pCo.Object = "calculationNotional";
                            pCo.Element = "notionalStepParameters";
                            pCo.OccurenceValue = 1;
                            pCo.CopyTo = "All";
                            pIsSpecified = cciStreamGlobal.IsNotionalStepParametersSpecified;
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
                        if (cciStream[i].IsCciOfContainer(pCci.ClientId_WithoutPrefix))
                        {
                            string key = cciStream[i].CciContainerKey(pCci.ClientId_WithoutPrefix);
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
                                    pIsSpecified = cciStream[i].IsNotionalStepScheduleStepSpecified;
                                    pIsEnabled = true;
                                    break;

                                case CciStream.CciEnum.calculationPeriodAmount_calculation_notionalSchedule_notionalStepParameters:
                                    pCo.Object = "calculationNotional";
                                    pCo.Element = "notionalStepParameters";
                                    pCo.ObjectIndexValue = i;
                                    pCo.OccurenceValue = i + 1;
                                    pIsSpecified = cciStream[i].IsNotionalStepParametersSpecified;
                                    pIsEnabled = true;
                                    break;

                                case CciStream.CciEnum.calculationPeriodAmount_knownAmountSchedule_initialValue:
                                    pCo.Object = string.Empty;
                                    pCo.Element = "calculationPeriodAmountKnownAmountSchedule";
                                    pCo.ObjectIndexValue = i;
                                    pCo.OccurenceValue = i + 1;
                                    pIsSpecified = cciStream[i].IsKnownAmountScheduleStepSpecified;
                                    pIsEnabled = true;
                                    break;

                                case CciStream.CciEnum.calculationPeriodAmount_calculation_floatingRateCalculation_capRateSchedule_initialValue:
                                    pCo.Object = "rateFloatingRate";
                                    pCo.Element = "capRateSchedule";
                                    pCo.ObjectIndexValue = i;
                                    pCo.OccurenceValue = i + 1;
                                    pIsSpecified = cciStream[i].IsCapRateScheduleSpecified;
                                    pIsEnabled = cciStream[i].IsFloatingRateSpecified;
                                    break;

                                case CciStream.CciEnum.calculationPeriodAmount_calculation_floatingRateCalculation_floorRateSchedule_initialValue:
                                    pCo.Object = "rateFloatingRate";
                                    pCo.Element = "floorRateSchedule";
                                    pCo.ObjectIndexValue = i;
                                    pCo.OccurenceValue = i + 1;
                                    pIsSpecified = cciStream[i].IsFloorRateScheduleSpecified;
                                    pIsEnabled = cciStream[i].IsFloatingRateSpecified;
                                    break;

                                case CciStream.CciEnum.calculationPeriodAmount_calculation_floatingRateCalculation_spreadSchedule_initialValue:
                                    pCo.Object = "rateFloatingRate";
                                    pCo.Element = "spreadSchedule";
                                    pCo.ObjectIndexValue = i;
                                    pCo.OccurenceValue = i + 1;
                                    pIsSpecified = cciStream[i].IsSpreadScheduleSpecified;
                                    pIsEnabled = cciStream[i].IsFloatingRateSpecified;
                                    break;

                                case CciStream.CciEnum.calculationPeriodAmount_calculation_floatingRateCalculation_floatingRateMultiplierSchedule_initialValue:
                                    pCo.Object = "rateFloatingRate";
                                    pCo.Element = "floatingRateMultiplierSchedule";
                                    pCo.ObjectIndexValue = i;
                                    pCo.OccurenceValue = i + 1;
                                    pIsSpecified = cciStream[i].IsFloatingRateMultiplierScheduleSpecified;
                                    pIsEnabled = cciStream[i].IsFloatingRateSpecified;
                                    break;

                                case CciStream.CciEnum.calculationPeriodAmount_calculation_fixedRateSchedule_initialValue:
                                    pCo.Object = string.Empty;
                                    pCo.Element = "rateFixedRate";
                                    // 20090911 EG Add pCo.ObjectIndexValue = i;
                                    pCo.ObjectIndexValue = i;
                                    pCo.OccurenceValue = i + 1;
                                    pIsSpecified = cciStream[i].IsFixedRateScheduleSpecified;
                                    pIsEnabled = cciStream[i].IsFixedRateSpecified;
                                    break;

                                case CciStream.CciEnum.paymentDates_offset:
                                    pCo.Object = "paymentDates";
                                    pCo.Element = "paymentDaysOffset";
                                    // 20090911 EG Add pCo.ObjectIndexValue = i;
                                    pCo.ObjectIndexValue = i;
                                    pCo.OccurenceValue = i + 1;
                                    pIsSpecified = cciStream[i].IsOffsetSpecified;
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
                        if (cciAdditionalPayment[i].IsCciOfContainer(pCci.ClientId_WithoutPrefix))
                        {
                            isOk = cciAdditionalPayment[i].IsCci(CciPayment.CciEnumPayment.settlementInformation, pCci);
                            if (isOk)
                            {
                                pCo.Object = "additionalPayment";
                                pCo.Element = "settlementInformation";
                                pCo.OccurenceValue = i + 1;
                                pIsSpecified = cciAdditionalPayment[i].IsSettlementInfoSpecified;
                                pIsEnabled = cciAdditionalPayment[i].IsSettlementInstructionSpecified;
                                break;
                            }
                        }
                    }
                }
                #endregion  AdditionalPaymentLength
                //
                if (false == isOk)
                    isOk = cciCancelableProvision.SetButtonZoom(pCci, pCo, ref pIsSpecified, ref pIsEnabled);
                if (false == isOk)
                    isOk = cciExtendibleProvision.SetButtonZoom(pCci, pCo, ref pIsSpecified, ref pIsEnabled);
                if (false == isOk)
                    isOk = cciEarlyTerminationProvision.SetButtonZoom(pCci, pCo, ref pIsSpecified, ref pIsEnabled);
                return isOk;

            }
            catch (Exception ex) { throw new SpheresException2(MethodInfo.GetCurrentMethod().Name, ex); }
        }
        #endregion SetButtonZoom
        #endregion ICciGetInfoButton Members
        #endregion Interfaces
        #region Methods
        #region GetMainCurrency
        public string GetMainCurrency()
        {
            string ret = string.Empty;
            if (loanDeposit.Stream[0].CalculationPeriodAmount.CalculationSpecified)
            {
                ICalculation calc = loanDeposit.Stream[0].CalculationPeriodAmount.Calculation;
                if (calc.NotionalSpecified)
                    ret = calc.Notional.StepSchedule.Currency.Value;
            }
            else if (loanDeposit.Stream[0].CalculationPeriodAmount.KnownAmountScheduleSpecified)
                ret = loanDeposit.Stream[0].CalculationPeriodAmount.KnownAmountSchedule.Currency.Value;

            return ret;
        }
        #endregion GetMainCurrency
        #region InitializeAdditionalPayment_FromCci
        private void InitializeAdditionalPayment_FromCci()
        {
            bool isOk = true;
            int index = -1;
            //            
            bool saveSpecified = loanDeposit.AdditionalPaymentSpecified;
            ArrayList lst = new ArrayList();
            while (isOk)
            {
                index += 1;
                CciPayment cciPayment = new CciPayment(cciTrade, index + 1, null, CciPayment.PaymentTypeEnum.Payment, prefix + TradeCustomCaptureInfos.CCst.Prefix_additionalPayment, string.Empty, string.Empty, string.Empty, cciTrade.CciClientIdMainCurrency,string.Empty);
                isOk = ccis.Contains(cciPayment.CciClientId(CciPayment.CciEnumPayment.payer));
                if (isOk)
                {
                    if (ArrFunc.IsEmpty(loanDeposit.AdditionalPayment) || (index == loanDeposit.AdditionalPayment.Length))
                        ReflectionTools.AddItemInArray(loanDeposit, "additionalPayment", index);
                    cciPayment.Payment = loanDeposit.AdditionalPayment[index];
                    //
                    lst.Add(cciPayment);
                }
            }
            //
            cciAdditionalPayment = (CciPayment[])lst.ToArray(typeof(CciPayment));
            for (int i = 0; i < this.AdditionalPaymentLength; i++)
                cciAdditionalPayment[i].Initialize_FromCci();
            //			
            loanDeposit.AdditionalPaymentSpecified = saveSpecified;
        }
        #endregion InitializeAdditionalPayment_FromCci
        #region InitializeLoanDepositStream_FromCci
        private void InitializeLoanDepositStream_FromCci()
        {
            try
            {
                bool isOk = true;
                int index = -1;
                ArrayList lst = new ArrayList();
                while (isOk)
                {
                    index += 1;
                    //
                    CciStream cciStreamCurrent = new CciStream(cciTrade, prefix + TradeCustomCaptureInfos.CCst.Prefix_loanDepositStream, index + 1, null);
                    //
                    isOk = ccis.Contains(cciStreamCurrent.CciClientId(CciStream.CciEnum.payer));
                    if (isOk)
                    {
                        if (ArrFunc.IsEmpty(loanDeposit.Stream) || (index == loanDeposit.Stream.Length))
                        {
                            ReflectionTools.AddItemInArray(loanDeposit, "loanDepositStream", index);
                            if (ArrFunc.IsFilled(ccis.TradeCommonInput.FpMLDataDocReader.Party))
                            {
                                loanDeposit.Stream[index].PayerPartyReference.HRef = ccis.TradeCommonInput.FpMLDataDocReader.Party[0].Id;
                                loanDeposit.Stream[index].ReceiverPartyReference.HRef = string.Empty;
                                if (2 == ArrFunc.Count(ccis.TradeCommonInput.FpMLDataDocReader.Party))
                                    loanDeposit.Stream[index].ReceiverPartyReference.HRef = ccis.TradeCommonInput.FpMLDataDocReader.Party[1].Id;
                            }
                        }
                        //
                        if (StrFunc.IsEmpty(loanDeposit.Stream[index].Id))
                            loanDeposit.Stream[index].Id = prefix + TradeCustomCaptureInfos.CCst.Prefix_loanDepositStream + Convert.ToString(index + 1);
                        //					
                        cciStreamCurrent.Irs = loanDeposit.Stream[index];
                        // 
                        lst.Add(cciStreamCurrent);
                    }
                }
                cciStream = (CciStream[])lst.ToArray(typeof(CciStream));
                //
                #region génération ds chaque Tradestream des ccis du  CciStreamGlobal
                for (int i = 0; i < ccis.Count; i++)
                {
                    CustomCaptureInfo cci = ccis[i];
                    if (cciStreamGlobal.IsCciOfContainer(cci.ClientId_WithoutPrefix))
                    {
                        string clientId_Key = cciStreamGlobal.CciContainerKey(cci.ClientId_WithoutPrefix);
                        if (System.Enum.IsDefined(typeof(CciStream.CciEnum), clientId_Key))
                        {
                            for (int j = 0; j < LoanDepositStreamLength; j++)
                                ccis.CloneGlobalCci(clientId_Key, cci, cciStream[j]);
                        }
                    }
                }
                #endregion génération ds chaque Tradestream des ccis du  CciStreamGlobal
                //
                cciStreamGlobal.Initialize_FromCci();
                for (int i = 0; i < LoanDepositStreamLength; i++)
                    cciStream[i].Initialize_FromCci();
                //			
            }
            catch (Exception ex) { throw new SpheresException2(MethodInfo.GetCurrentMethod().Name, ex); }
        }
        #endregion InitializeLoanDepositStream_FromCci
        #region InitializeProvision_FromCci
        private void InitializeProvision_FromCci()
        {
            CciTools.CreateInstance(this, loanDeposit);
            //
            if (null != cciCancelableProvision)
                cciCancelableProvision.Initialize_FromCci();
            if (null != cciExtendibleProvision)
                cciExtendibleProvision.Initialize_FromCci();
            if (null != cciEarlyTerminationProvision)
                cciEarlyTerminationProvision.Initialize_FromCci();
        }

        #endregion InitializeProvision_FromCci
        #region RemoveLastItemInAdditionalPaymentArray
        public bool RemoveLastItemInAdditionalPaymentArray(string pPrefix)
        {
            return RemoveLastItemInAdditionalPaymentArray(pPrefix, false);
        }
        public bool RemoveLastItemInAdditionalPaymentArray(string pPrefix, bool pIsEmpty)
        {
            bool isOk = true;
            //
            if (pPrefix == TradeCustomCaptureInfos.CCst.Prefix_additionalPayment)
            {
                int posArray = AdditionalPaymentLength - 1;
                bool isToRemove = true;
                if (pIsEmpty)
                    isToRemove = StrFunc.IsEmpty(loanDeposit.AdditionalPayment[posArray].PayerPartyReference.HRef);
                //
                if (isToRemove)
                {
                    ccis.RemoveCciOf(cciAdditionalPayment[posArray]);
                    ReflectionTools.RemoveItemInArray(this, "tradeAdditionalPayment", posArray);
                    ReflectionTools.RemoveItemInArray(loanDeposit, "additionalPayment", posArray);
                }
                else
                    isOk = false;
            }
            //
            return isOk;
        }
        #endregion RemoveLastItemInAdditionalPaymentArray
        #region SynchronizeBDAFixedRate
        private void SynchronizeBDAFixedRate(CustomCaptureInfo pCci)
        {
            try
            {
                ArrayList alFloatingRateStream = new ArrayList();
                //
                for (int i = 0; i < LoanDepositStreamLength; i++)
                {
                    if (cciStream[i].IsFloatingRateSpecified)
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
                            if (false == cciStream[i].IsFloatingRateSpecified)
                            {
                                string clientId_WithoutPrefix = string.Empty;
                                //
                                if (cciStream[streamFloatIndex].IsCci(CciStream.CciEnum.calculationPeriodDates_calculationPeriodDatesAdjustments_bDC, pCci))
                                {
                                    if (0 == ((IInterval)cciStream[i].Irs.CalculationPeriodDates.CalculationPeriodFrequency).CompareTo(cciStream[streamFloatIndex].Irs.CalculationPeriodDates.CalculationPeriodFrequency))
                                    {
                                        //CalculationPeriodDates
                                        clientId_WithoutPrefix = cciStream[i].CciClientId(CciStream.CciEnum.calculationPeriodDates_calculationPeriodDatesAdjustments_bDC);
                                        ccis[clientId_WithoutPrefix].NewValue = cciStream[streamFloatIndex].Irs.CalculationPeriodDates.CalculationPeriodDatesAdjustments.BusinessDayConvention.ToString();
                                    }
                                }
                                //
                                if (cciStream[streamFloatIndex].IsCci(CciStream.CciEnum.paymentDates_paymentDatesAdjustments_bDC, pCci))
                                {
                                    if (0 == cciStream[i].Irs.PaymentDates.PaymentFrequency.CompareTo(cciStream[streamFloatIndex].Irs.PaymentDates.PaymentFrequency))
                                    {
                                        //paymentDates
                                        clientId_WithoutPrefix = cciStream[i].CciClientId(CciStream.CciEnum.paymentDates_paymentDatesAdjustments_bDC);
                                        ccis[clientId_WithoutPrefix].NewValue = cciStream[streamFloatIndex].Irs.PaymentDates.PaymentDatesAdjustments.BusinessDayConvention.ToString();
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex) { throw new SpheresException2(MethodInfo.GetCurrentMethod().Name, ex); }
        }
        #endregion SynchronizeBDAFixedRate
        #endregion Methods

        #region ICciPresentation Membres
        public void DumpSpecific_ToGUI(CciPageBase pPage)
        {
            for (int i = 0; i < ArrFunc.Count(cciStream); i++)
            {
                cciStream[i].DumpSpecific_ToGUI(pPage);
            }
        }
        #endregion
    }
    #endregion

}
