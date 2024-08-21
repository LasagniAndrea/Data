#region Using Directives
using System;
using System.Collections;
using System.Data;
using System.Reflection;
using System.Text;
using System.Xml;

using EFS.ACommon;
using EFS.Common.Web;
using EFS.Actor;
using EFS.ApplicationBlocks.Data;
using EFS.Common;
using EFS.Common.MQueue;

using EFS.GUI;
using EFS.GUI.CCI;
using EFS.GUI.Interface;


using EFS.EFSTools;
using EFS.OTCmlStatus;
using EFS.Tuning;
using EFS.Permission;

using EFS.Book; 

using EfsML;
using EfsML.Business;
using EfsML.Enum.Tools;
using EfsML.Interface;

using FpML.Enum;
using FpML.Interface;

using FixML.Enum;
#endregion Using Directives

namespace EFS.TradeInformation
{

    /// <summary>
    /// Validation de la saisie des trades
    /// </summary>
    ///<remarks>
    ///  
    ///</remarks> 
    public class CheckTradeValidationRule : CheckTradeInputValidationRuleBase
    {
        #region Members
        private TradeInput m_Input;
        #endregion Members

        #region Accessors

        #endregion Accessors

        #region Constructors
        // RD 20110222 Pour charger les Assets créés en Mode Transactionnel.
        public CheckTradeValidationRule(string pCs, TradeInput pTradeInput, Cst.Capture.ModeEnum pCaptureModeEnum)
            : this(pCs, null, pTradeInput, pCaptureModeEnum) { }
        public CheckTradeValidationRule(string pCs, IDbTransaction pTransaction, TradeInput pTradeInput, Cst.Capture.ModeEnum pCaptureModeEnum)
            : base(pCs, pTransaction, pTradeInput, pCaptureModeEnum)
        {
            m_Input = pTradeInput;
        }
        #endregion constructor

        #region Methods

        /// <summary>
        /// Retourne true si toutes les validations rules sont respectées
        /// </summary>
        /// <param name="pCheckMode"></param>
        /// <returns></returns>
        public override bool ValidationRules(CheckModeEnum pCheckMode)
        {
            m_CheckMode = pCheckMode;
            m_CheckConformity = new Hashtable();
            //
            if (Cst.Capture.IsModeNewCapture(captureMode) ||
                Cst.Capture.IsModeUpdate(captureMode))
            {
                if (false == m_Input.DataDocument.currentProduct.isStrategy)
                    CheckActorBuyerSeller();
                CheckValidationRule_Book();
                CheckValidationRule_TraderAndSales();
                CheckValidationRule_Hedging();
                CheckValidationRule_DtTransac();
                CheckValidationRule_Product();
                CheckValidationRule_OPP();
                CheckValidationRule_Element();
            }
            //
            if (Cst.Capture.IsModeAction(captureMode))
            {
                if (captureMode == Cst.Capture.ModeEnum.PositionCancelation)
                {
                    CheckCorrectionOfQuantityValidationRule chk = new CheckCorrectionOfQuantityValidationRule(CS, DbTransaction, this.m_Input.positionCancel);
                    string msg = chk.GetConformityMsg();
                    if (StrFunc.IsFilled(msg))
                        SetValidationRuleError("ddd", msg);

                    //CheckValidationRule_CorrectionOfQuantity();
                }
                else if (
                    captureMode == Cst.Capture.ModeEnum.OptionExercise ||
                    captureMode == Cst.Capture.ModeEnum.OptionAssignment ||
                    captureMode == Cst.Capture.ModeEnum.OptionAbandon)
                {
                    CheckValidationRule_ExeAssAbn();
                }
            }
            //			
            return ArrFunc.IsEmpty(m_CheckConformity);
        }

        /// <summary>
        /// Validation Rule: Date de transaction
        /// <para>- Nbre de jour(s) avant/après la date système</para>
        /// </summary>
        private void CheckValidationRule_DtTransac()
        {
            if (IsToCheck("VRTRANSBEFSYSMAX") || IsToCheck("VRTRANSAFTSYSMAX"))
            {
                DateTime dtTransac = m_Input.CurrentTrade.tradeHeader.tradeDate.DateValue;
                DateTime dtSysBusiness = new DtFunc().StringDateISOToDateTime(DtFunc.DateTimeToStringDateISO(OTCmlHelper.GetDateBusiness(CS)));

                //VRTRANSBEFSYSMAX
                if (IsToCheck("VRTRANSBEFSYSMAX"))
                {
                    int offset = Convert.ToInt32(sqlInstrument.GetFirstRowColumnValue("TRANSBEFSYSMAX"));
                    DateTime dtSysOffset = dtSysBusiness.AddDays(-offset);

                    if (0 > dtTransac.CompareTo(dtSysOffset))
                        SetValidationRuleError("Msg_ValidationRule_DtTransacBeforeDtSysMax",
                            new string[] { DtFunc.DateTimeToString(dtTransac, DtFunc.FmtShortDate), offset.ToString() });
                }

                //VRTRANSAFTSYSMAX
                if (IsToCheck("VRTRANSAFTSYSMAX"))
                {
                    int offset = Convert.ToInt32(sqlInstrument.GetFirstRowColumnValue("TRANSAFTSYSMAX"));
                    DateTime dtSysOffset = dtSysBusiness.AddDays(offset);

                    if (0 < dtTransac.CompareTo(dtSysOffset))
                        SetValidationRuleError("Msg_ValidationRule_DtTransacAfterDtSysMax",
                            new string[] { DtFunc.DateTimeToString(dtTransac, DtFunc.FmtShortDate), offset.ToString() });
                }
            }
        }

        /// <summary>
        /// Validation Rule: OPPs
        /// <para>- Contrôle d'existance, sur chaque payment, d'un book géré sur le payer ou à minima sur le receiver</para>
        /// </summary>
        private void CheckValidationRule_OPP()
        {
            if (m_Input.DataDocument.otherPartyPaymentSpecified)
            {
                //FI 20100423 [16967] add contrôle de l'existence sur OPP
                #region Contrôle d'existence d'au moins un book géré par paiement
                for (int i = 0; i < ArrFunc.Count(m_Input.DataDocument.otherPartyPayment); i++)
                {
                    IPayment payment = m_Input.DataDocument.otherPartyPayment[i];
                    Boolean isOk = false;
                    //Check PAYER
                    IPartyTradeIdentifier partyTradeIdentifier = m_Input.DataDocument.GetPartyTradeIdentifier(payment.payerPartyReference.hRef);
                    if ((null != partyTradeIdentifier) && partyTradeIdentifier.bookIdSpecified)
                    {
                        IBookId bookId = partyTradeIdentifier.bookId;
                        isOk = BookTools.isBookManaged(CSTools.SetCacheOn(CS), bookId.OTCmlId);
                    }

                    if (!isOk)
                    {
                        //Check RECEIVER
                        partyTradeIdentifier = m_Input.DataDocument.GetPartyTradeIdentifier(payment.receiverPartyReference.hRef);
                        if ((null != partyTradeIdentifier) && partyTradeIdentifier.bookIdSpecified)
                        {
                            IBookId bookId = partyTradeIdentifier.bookId;
                            isOk = BookTools.isBookManaged(CSTools.SetCacheOn(CS), bookId.OTCmlId);
                        }
                    }

                    //Dès qu'un payment n'est pas ok, on arrête
                    if (!isOk)
                    {
                        SetValidationRuleError("Msg_ValidationRule_OPP_Book_IsAvailable");
                        break;
                    }
                }
                #endregion
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private void CheckValidationRule_TraderAndSales()
        {
            ITradeHeader tradeHeader = m_Input.CurrentTrade.tradeHeader;
            if (ArrFunc.IsFilled(tradeHeader.partyTradeInformation))
            {
                for (int i = 0; i < tradeHeader.partyTradeInformation.Length; i++)
                {
                    IPartyTradeInformation partyTradeInformation = (IPartyTradeInformation)tradeHeader.partyTradeInformation[i];
                    string[] partyReference = new string[] { partyTradeInformation.partyReference };
                    // 
                    #region Get Party Type
                    string partyType = CciTradeParty.PartyType.party.ToString();
                    //
                    if (m_Input.DataDocument.currentTrade.brokerPartyReferenceSpecified)
                    {
                        foreach (IReference broker in m_Input.DataDocument.currentTrade.brokerPartyReference)
                        {
                            if (broker.hRef == partyTradeInformation.partyReference)
                            {
                                partyType = CciTradeParty.PartyType.broker.ToString();
                                break;
                            }
                        }
                    }
                    #endregion Get Party Type
                    //
                    CheckValidationRule_TraderAndSales(partyTradeInformation.trader, partyType, partyReference);
                    CheckValidationRule_TraderAndSales(partyTradeInformation.sales, partyType, partyReference);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pTrader"></param>
        /// <param name="pPartyType"></param>
        /// <param name="pPartyReference"></param>
        private void CheckValidationRule_TraderAndSales(ITrader[] pTrader, string pPartyType, string[] pPartyReference)
        {

            string errMessage = "Msg_ValidationRule_" + pPartyType;
            //
            if (ArrFunc.IsFilled(pTrader))
            {
                decimal sumFactor = 0;
                bool isUniqueTraderOk = true;
                //                                
                for (int j = 0; j < pTrader.Length; j++)
                {
                    if (StrFunc.IsFilled(pTrader[j].factor))
                        sumFactor += pTrader[j].Factor;
                    // La saisie du même Trader plusieurs fois
                    for (int jj = j + 1; jj < pTrader.Length; jj++)
                    {
                        // 20091012 RD Pour éviter de comparer deux Trader Vides.
                        if (StrFunc.IsFilled(pTrader[j].Value) && (pTrader[j].Value == pTrader[jj].Value))
                            isUniqueTraderOk = false;
                    }
                }
                //
                if (IsCheckError)
                {
                    if (1 < sumFactor)
                        SetValidationRuleError(errMessage + "_TraderFactorSumUp100", pPartyReference);
                    //
                    if (false == isUniqueTraderOk)
                        SetValidationRuleError(errMessage + "_TraderNotUnique", pPartyReference);
                }
                else if (IsCheckWarning && (0 < sumFactor && sumFactor < 1))
                    SetValidationRuleError(errMessage + "_TraderFactorSumLess100", pPartyReference);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private void CheckValidationRule_Hedging()
        {
            if (IsCheckError)
            {
                if (ArrFunc.IsFilled(m_Input.DataDocument.currentTrade.tradeHeader.partyTradeIdentifier))
                {
                    for (int i = 0; i < m_Input.DataDocument.currentTrade.tradeHeader.partyTradeIdentifier.Length; i++)
                    {
                        IPartyTradeIdentifier partyTradeIdentifier = (IPartyTradeIdentifier)m_Input.DataDocument.currentTrade.tradeHeader.partyTradeIdentifier[i];
                        ILinkId linkId = partyTradeIdentifier.GetLinkIdFromScheme(Cst.OTCmL_hedgingFolderid);
                        if ((null != linkId) && (StrFunc.IsFilled(linkId.factor)) && (linkId.Factor > 1))
                            SetValidationRuleError("Msg_ValidationRule_Hedging_FactorUp100", new string[] { partyTradeIdentifier.partyReference.hRef });
                    }
                }
            }

        }

        /// <summary>
        /// 
        /// </summary>
        private void CheckValidationRule_Product()
        {

            DataDocumentContainer dataDocument = m_Input.DataDocument;
            ProductContainer product = dataDocument.currentProduct;
            if (product.isFxLeg)
            {
                #region IsFxLeg
                IFxLeg fxleg = (IFxLeg)product.product;
                CheckVR_FXLeg(fxleg, null);
                #endregion IsFxLeg
            }
            else if (product.isFxSwap)
            {
                #region IsFxSwap
                IFxSwap fxSwap = (IFxSwap)product.product;
                for (int i = 0; i < ArrFunc.Count(fxSwap.fxSingleLeg); i++)
                {
                    if (i == 0)
                        CheckVR_FXLeg(fxSwap.fxSingleLeg[i], null);
                    else
                        CheckVR_FXLeg(fxSwap.fxSingleLeg[i], fxSwap.fxSingleLeg[i - 1]);
                }
                #endregion IsFxSwap
            }
            else if (product.isFxTermDeposit)
            {
                #region IsFxTermDeposit
                ITermDeposit termDeposit = (ITermDeposit)product.product;
                #endregion
            }
            else if (product.isFxSimpleOption)
            {
                #region IsFxSimpleOption
                IFxOptionLeg fxOptLeg = (IFxOptionLeg)product.product;
                CheckVR_FXOption(fxOptLeg);
                #endregion IsFxSimpleOption
            }
            else if (product.isFxDigitalOption)
            {
                #region IsFxDigitalOption
                IFxDigitalOption fxDigOpt = (IFxDigitalOption)product.product;
                CheckVR_FXOption(fxDigOpt);
                #endregion IsFxDigitalOption
            }
            else if (product.isFxBarrierOption)
            {
                #region IsFxBarrierOption
                IFxBarrierOption fxBarOpt = (IFxBarrierOption)product.product;
                CheckVR_FXOption(fxBarOpt);
                if (IsCheckError) // systematique en mode Erreur
                {
                    if (ArrFunc.IsEmpty(fxBarOpt.fxBarrier))
                        SetValidationRuleError("Msg_ValidationRule_FxBarrierOptionNoBarrier");
                }
                #endregion IsFxBarrierOption
            }
            else if (product.isFxAverageRateOption)
            {
                #region IsFxAverageRateOption
                IFxAverageRateOption fxaveRateOpt = (IFxAverageRateOption)product.product;
                CheckVR_FXOption(fxaveRateOpt);
                #endregion IsFxAverageRateOption
            }
            else if (product.isFra)
            {
                #region IsFra
                IFra fra = (IFra)product.product;
                #endregion
            }
            else if (product.isSwap)
            {
                #region IsSwap
                ISwap swap = (ISwap)product.product;
                //
                if (IsCheckError) // systematique en mode Erreur
                {
                    if (ArrFunc.Count(swap.stream) == 2)
                    {
                        if ((0 == swap.stream[0].payerPartyReference.hRef.CompareTo(swap.stream[1].payerPartyReference.hRef)) ||
                            (0 == swap.stream[0].receiverPartyReference.hRef.CompareTo(swap.stream[1].receiverPartyReference.hRef)))
                        {
                            SetValidationRuleError("Msg_ValidationRule_IdenticPayerReceiver2");
                        }
                    }
                }
                // 20090615 RD Pour partager le même code avec les SaleAndRepurchaseAgreement
                CheckVR_IRD(swap.stream);
                #endregion IsSwap
            }
            else if (product.isLoanDeposit)
            {
                #region IsLoanDeposit
                ILoanDeposit loanDeposit = (ILoanDeposit)product.product;
                //
                if (IsCheckError) // systematique en mode Erreur
                {
                    if (ArrFunc.Count(loanDeposit.stream) > 1)
                    {
                        string hrefPayer = loanDeposit.stream[0].payerPartyReference.hRef;
                        string hrefReceiver = loanDeposit.stream[0].receiverPartyReference.hRef;
                        for (int i = 1; i < ArrFunc.Count(loanDeposit.stream); i++)
                        {
                            if ((0 == hrefPayer.CompareTo(loanDeposit.stream[i].payerPartyReference.hRef))
                                ||
                                (0 == hrefReceiver.CompareTo(loanDeposit.stream[i].receiverPartyReference.hRef)))
                            {
                                SetValidationRuleError("Msg_ValidationRule_DifferentPayerReceiver");
                            }
                        }
                    }
                }
                //
                //20090629 PL Refactoring: Call CheckVR_IRD()
                CheckVR_IRD(loanDeposit.stream);
                #endregion IsLoanDeposit
            }
            else if (product.isSwaption)
            {
                #region IsSwaption
                ISwaption swaption = (ISwaption)product.product;
                ISwap swap = swaption.swap;
                //
                if (IsCheckError) // systematique en mode Erreur
                {
                    if (ArrFunc.Count(swap.stream) == 2)
                    {
                        if ((0 == swap.stream[0].payerPartyReference.hRef.CompareTo(swap.stream[1].payerPartyReference.hRef)) ||
                            (0 == swap.stream[0].receiverPartyReference.hRef.CompareTo(swap.stream[1].receiverPartyReference.hRef)))
                        {
                            SetValidationRuleError("Msg_ValidationRule_IdenticPayerReceiver2");
                        }
                    }
                }
                // 
                CheckVR_IRD(swap.stream);
                #endregion IsSwaption
            }
            else if (product.isCapFloor)
            {
                #region IsCapFloor
                ICapFloor capFloor = (ICapFloor)product.product;
                //20090629 PL Refactoring: Call CheckVR_IRD()
                CheckVR_IRD(capFloor.stream);
                #endregion IsCapFloor
            }
            else if (product.isBulletPayment)
            {
                #region IsBulletPayment
                IBulletPayment bulletPayment = (IBulletPayment)product.product;
                #endregion IsBulletPayment
            }
            else if (product.isEquitySwap)
            {
                #region IsEquitySwap
                IReturnSwap equiSwap = (IReturnSwap)product.product;
                #endregion IsEquitySwap
            }
            else if (product.isEquityOption)
            {
                #region IsEquityOption
                IEquityOption equiOption = (IEquityOption)product.product;
                #endregion IsEquityOption
            }
            else if (product.isDebtSecurityTransaction)
            {
                #region IsDebtSecurityTransaction
                IDebtSecurityTransaction debtSecurityTransaction = (IDebtSecurityTransaction)product.product;
                CheckVR_DebtSecurityTransaction(debtSecurityTransaction, true);
                #endregion
            }
            else if (product.isRepo || product.isBuyAndSellBack)
            {
                #region IsSalesAndRepurchaseAgreement
                ISaleAndRepurchaseAgreement saleAndRepurchaseAgreement = (ISaleAndRepurchaseAgreement)product.product;
                //
                for (int i = 0; i < ArrFunc.Count(saleAndRepurchaseAgreement.spotLeg); i++)
                    CheckVR_DebtSecurityTransaction(saleAndRepurchaseAgreement.spotLeg[i].debtSecurityTransaction, !product.isRepo);
                //
                if (saleAndRepurchaseAgreement.forwardLegSpecified)
                {
                    for (int i = 0; i < ArrFunc.Count(saleAndRepurchaseAgreement.forwardLeg); i++)
                        CheckVR_DebtSecurityTransaction(saleAndRepurchaseAgreement.forwardLeg[i].debtSecurityTransaction, false);
                }
                //
                CheckVR_IRD(saleAndRepurchaseAgreement.cashStream);
                CheckVR_SalesAndRepurchaseAgreement(saleAndRepurchaseAgreement);
                #endregion
            }
            else if (product.isExchangeTradedDerivative)
            {
                IExchangeTradedDerivative exchangeTradedDerivative = (IExchangeTradedDerivative)product.product;
                CheckVR_ExchangeTradedDerivative(exchangeTradedDerivative);
            }
            else if (product.isStrategy)
            {
#warning les strategies ne sont pas traitées, présence d'un seul cas particulier sur les strategies sur ETD afin de ne pas reproduire le problème rencontré sur le ticket [17209]
                //FI 20111009 [17209]
                StrategyContainer strategy = (StrategyContainer)dataDocument.currentProduct;
                for (int i = 0; i < ArrFunc.Count(strategy.subProduct); i++)
                {
                    if (Tools.IsTypeOrInterfaceOf(strategy.subProduct[i], EfsML.Enum.InterfaceEnum.IExchangeTradedDerivative))
                    {
                        IExchangeTradedDerivative exchangeTradedDerivative = (IExchangeTradedDerivative)strategy.subProduct[i];
                        CheckVR_ExchangeTradedDerivative(exchangeTradedDerivative);
                    }
                }
            }
        }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="pIrs"></param>
        private void CheckVR_IRD(IInterestRateStream pIrs)
        {
            //FI 20101110 Bizaremment on ne controlait pas la dates => Ajout appel à CheckVR_IRDDate
            CheckVR_IRDDate(pIrs, null);
            CheckVR_IRDStream(pIrs, null);

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pIrs"></param>
        private void CheckVR_IRD(IInterestRateStream[] pIrs)
        {

            for (int i = 0; i < ArrFunc.Count(pIrs); i++)
            {
                if (0 == i)
                {
                    CheckVR_IRDDate(pIrs[i], null);
                    CheckVR_IRDStream(pIrs[i], null);
                }
                else
                {
                    CheckVR_IRDDate(pIrs[i], pIrs[i - 1]);
                    CheckVR_IRDStream(pIrs[i], pIrs[i - 1]);
                }
            }

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pIrsCurrent"></param>
        /// <param name="pIrsPrevious"></param>
        private void CheckVR_IRDDate(IInterestRateStream pIrsCurrent, IInterestRateStream pIrsPrevious)
        {

            //20061201 PL Rajouter ctrl sur stubdate, firstdate, ...
            DateTime dtTransac = m_Input.DataDocument.currentTrade.tradeHeader.tradeDate.DateValue;
            DateTime dtEffectiveDate = pIrsCurrent.calculationPeriodDates.effectiveDateAdjustable.unadjustedDate.DateValue;
            //FI 20091223 [16471] add test sur terminationDateAdjustableSpecified
            DateTime dtTerminationDate = DateTime.MinValue;
            if (pIrsCurrent.calculationPeriodDates.terminationDateAdjustableSpecified)
                dtTerminationDate = pIrsCurrent.calculationPeriodDates.terminationDateAdjustable.unadjustedDate.DateValue;
            //
            string[] idA = GetParties(pIrsCurrent, true);
            string[] idC = null;
            if (StrFunc.IsFilled(pIrsCurrent.GetCurrency))
                idC = new string[] { pIrsCurrent.GetCurrency };
            //
            IBusinessCenters bcs = m_Input.DataDocument.currentProduct.productBase.LoadBusinessCenters(CSTools.SetCacheOn(CS) , idA, idC, null);
            IBusinessDayAdjustments bda = bcs.GetBusinessDayAdjustments(BusinessDayConventionEnum.FOLLOWING);
            IBusinessDayAdjustments bdaWithoutBC = m_Input.DataDocument.currentProduct.productBase.CreateBusinessDayAdjustments(BusinessDayConventionEnum.FOLLOWING);

            EFS_BusinessCenters efs_bc = new EFS_BusinessCenters(CS, bcs);
            if (efs_bc.businessCentersSpecified)
            {
                if (IsToCheck("VRTRANSHOLIDAY"))
                {
                    if (efs_bc.IsHoliday(dtTransac))
                        SetValidationRuleError("Msg_ValidationRule_DtTransacIsHoliday", new string[] { DtFunc.DateTimeToString(dtTransac, DtFunc.FmtShortDate) });
                }
                if (IsToCheck("VRSTAHOLIDAY"))
                {
                    if (efs_bc.IsHoliday(dtEffectiveDate))
                        SetValidationRuleError("Msg_ValidationRule_DtEffectiveIsHoliday", new string[] { DtFunc.DateTimeToString(dtEffectiveDate, DtFunc.FmtShortDate) });
                }
                if (IsToCheck("VRENDHOLIDAY"))
                {
                    // 20090617 RD ( Pour un Repo la date d'échéance n'est pas obligatoire)
                    if (DtFunc.IsDateTimeFilled(dtTerminationDate) && efs_bc.IsHoliday(dtTerminationDate))
                        SetValidationRuleError("Msg_ValidationRule_DtTerminationIsHoliday", new string[] { DtFunc.DateTimeToString(dtTerminationDate, DtFunc.FmtShortDate) });
                }
            }
            if (IsToCheck("VRSTAAFTTRANSMIN"))
            {
                int offset = Convert.ToInt32(sqlInstrument.GetFirstRowColumnValue("STAAFTTRANSMIN"));
                bool isBusinness = BoolFunc.IsTrue(sqlInstrument.GetFirstRowColumnValue("STAAFTTRANSMINB").ToString());
                IInterval interval = m_Input.DataDocument.currentProduct.productBase.CreateInterval(PeriodEnum.D, offset);
                DateTime dtMinEffective = Tools.ApplyAdjustedInterval(CS, dtTransac, interval, isBusinness ? bda : bdaWithoutBC);
                if (0 > dtEffectiveDate.CompareTo(dtMinEffective))
                    SetValidationRuleError("Msg_ValidationRule_DtEffective2", new string[] { 
                            DtFunc.DateTimeToString(dtTransac, DtFunc.FmtShortDate), 
                            DtFunc.DateTimeToString(dtEffectiveDate, DtFunc.FmtShortDate), 
                            offset.ToString() });
            }
            //
            if (IsToCheck("VRSTAAFTTRANSMAX"))
            {
                int offset = Convert.ToInt32(sqlInstrument.GetFirstRowColumnValue("STAAFTTRANSMAX"));
                bool isBusinness = BoolFunc.IsTrue(sqlInstrument.GetFirstRowColumnValue("STAAFTTRANSMAXB").ToString());
                IInterval interval = m_Input.DataDocument.currentProduct.productBase.CreateInterval(PeriodEnum.D, offset);
                DateTime dtMaxEffective = Tools.ApplyAdjustedInterval(CSTools.SetCacheOn(CS), dtTransac, interval, isBusinness ? bda : bdaWithoutBC);
                if (0 < dtEffectiveDate.CompareTo(dtMaxEffective))
                    SetValidationRuleError("Msg_ValidationRule_DtEffective3", new string[] { 
                            DtFunc.DateTimeToString(dtTransac, DtFunc.FmtShortDate), 
                            DtFunc.DateTimeToString(dtEffectiveDate, DtFunc.FmtShortDate), 
                            offset.ToString() });
            }
            //
            // 20090617 RD ( Pour un Repo la date d'échéance n'est pas obligatoire)
            if (IsToCheck("VRENDAFTSTAMIN") && DtFunc.IsDateTimeFilled(dtTerminationDate))
            {
                int offset = Convert.ToInt32(sqlInstrument.GetFirstRowColumnValue("ENDAFTSTAMIN"));
                bool isBusinness = BoolFunc.IsTrue(sqlInstrument.GetFirstRowColumnValue("ENDAFTSTAMINB").ToString());
                IInterval interval = m_Input.DataDocument.currentProduct.productBase.CreateInterval(PeriodEnum.D, offset);
                DateTime dtMinTermination = Tools.ApplyAdjustedInterval(CSTools.SetCacheOn(CS), dtEffectiveDate, interval, isBusinness ? bda : bdaWithoutBC);
                if (0 > dtTerminationDate.CompareTo(dtMinTermination))
                    SetValidationRuleError("Msg_ValidationRule_DtTermination2", new string[] { 
                            DtFunc.DateTimeToString(dtEffectiveDate, DtFunc.FmtShortDate), 
                            DtFunc.DateTimeToString(dtTerminationDate, DtFunc.FmtShortDate), 
                            offset.ToString() });
            }
            //
            // 20090617 RD ( Pour un Repo la date d'échéance n'est pas obligatoire)
            if (IsToCheck("VRENDAFTSTAMAX") && DtFunc.IsDateTimeFilled(dtTerminationDate))
            {
                int offset = Convert.ToInt32(sqlInstrument.GetFirstRowColumnValue("ENDAFTSTAMAX"));
                bool isBusinness = BoolFunc.IsTrue(sqlInstrument.GetFirstRowColumnValue("ENDAFTSTAMAXB").ToString());
                IInterval interval = m_Input.DataDocument.currentProduct.productBase.CreateInterval(PeriodEnum.D, offset);
                DateTime dtMaxTermination = Tools.ApplyAdjustedInterval(CSTools.SetCacheOn(CS), dtEffectiveDate, interval, isBusinness ? bda : bdaWithoutBC);
                if (0 < dtTerminationDate.CompareTo(dtMaxTermination))
                    SetValidationRuleError("Msg_ValidationRule_DtTermination3", new string[] { 
                            DtFunc.DateTimeToString(dtEffectiveDate, DtFunc.FmtShortDate), 
                            DtFunc.DateTimeToString(dtTerminationDate, DtFunc.FmtShortDate), 
                            offset.ToString() });
            }
            //
            // 20090617 RD ( Pour un Repo la date d'échéance n'est pas obligatoire)
            if (DtFunc.IsDateTimeFilled(dtTerminationDate) && IsCheckError) // systematique en mode Erreur
            {
                if (0 > dtTerminationDate.CompareTo(dtTransac))
                    SetValidationRuleError("Msg_ValidationRule_DtTermination1", new string[] { 
                            DtFunc.DateTimeToString(dtTerminationDate, DtFunc.FmtShortDate),
                            DtFunc.DateTimeToString(dtTransac, DtFunc.FmtShortDate)});
            }
            //
            if (IsCheckError) // systematique en mode Erreur
            {
                if (pIrsCurrent.calculationPeriodDates.firstPeriodStartDateSpecified)
                {
                    if (Tools.IsBdcNoneOrNA(pIrsCurrent.calculationPeriodDates.effectiveDateAdjustable.dateAdjustments.businessDayConvention) &&
                        Tools.IsBdcNoneOrNA(pIrsCurrent.calculationPeriodDates.firstPeriodStartDate.dateAdjustments.businessDayConvention))
                    {
                        DateTime dtFirstPeriodStartDate = pIrsCurrent.calculationPeriodDates.firstPeriodStartDate.unadjustedDate.DateValue;
                        if (0 >= dtEffectiveDate.CompareTo(dtFirstPeriodStartDate))
                            SetValidationRuleError("Msg_ValidationRule_FirstPeriodStartDate", new string[] {  
                                    DtFunc.DateTimeToString(dtFirstPeriodStartDate, DtFunc.FmtShortDate),
                                    DtFunc.DateTimeToString(dtEffectiveDate, DtFunc.FmtShortDate)});
                    }
                }
            }
            //
            if (IsCheckError) // systematique en mode Erreur
            {
                if (pIrsCurrent.calculationPeriodDates.firstRegularPeriodStartDateSpecified)
                {
                    DateTime dtFirstRegularPeriodStartDate = pIrsCurrent.calculationPeriodDates.firstRegularPeriodStartDate.DateValue;
                    //
                    if (pIrsCurrent.calculationPeriodDates.firstPeriodStartDateSpecified &&
                        Tools.IsBdcNoneOrNA(pIrsCurrent.calculationPeriodDates.firstPeriodStartDate.dateAdjustments.businessDayConvention))
                    {
                        DateTime dtFirstPeriodStartDate = pIrsCurrent.calculationPeriodDates.firstPeriodStartDate.unadjustedDate.DateValue;
                        if (0 >= dtFirstRegularPeriodStartDate.CompareTo(dtFirstPeriodStartDate))
                            SetValidationRuleError("Msg_ValidationRule_FirstRegularPeriodStartDate2", new string[] {  
                                    DtFunc.DateTimeToString(dtFirstRegularPeriodStartDate, DtFunc.FmtShortDate),
                                    DtFunc.DateTimeToString(dtFirstPeriodStartDate, DtFunc.FmtShortDate)});
                    }
                    else
                    {
                        if (0 >= dtFirstRegularPeriodStartDate.CompareTo(dtEffectiveDate))
                            SetValidationRuleError("Msg_ValidationRule_FirstRegularPeriodStartDate", new string[] {  
                                    DtFunc.DateTimeToString(dtFirstRegularPeriodStartDate, DtFunc.FmtShortDate),
                                    DtFunc.DateTimeToString(dtEffectiveDate, DtFunc.FmtShortDate)});
                    }
                    //
                    if (pIrsCurrent.calculationPeriodDates.lastRegularPeriodEndDateSpecified)
                    {
                        if (0 > pIrsCurrent.calculationPeriodDates.lastRegularPeriodEndDate.DateValue.CompareTo(dtFirstRegularPeriodStartDate))
                            SetValidationRuleError("Msg_ValidationRule_FirstRegularPeriodStartDate3", new string[] {  
                                    DtFunc.DateTimeToString(pIrsCurrent.calculationPeriodDates.lastRegularPeriodEndDate.DateValue, DtFunc.FmtShortDate),
                                    DtFunc.DateTimeToString(dtFirstRegularPeriodStartDate, DtFunc.FmtShortDate)});
                    }
                }
            }
            //
            // 20090617 RD ( Pour un Repo la date d'échéance n'est pas obligatoire)
            if (DtFunc.IsDateTimeFilled(dtTerminationDate) && IsCheckError) // systematique en mode Erreur
            {
                if (pIrsCurrent.calculationPeriodDates.lastRegularPeriodEndDateSpecified)
                {
                    DateTime dtLastRegularPeriodEndDate = pIrsCurrent.calculationPeriodDates.lastRegularPeriodEndDate.DateValue;
                    if (0 >= dtTerminationDate.CompareTo(dtLastRegularPeriodEndDate))
                        SetValidationRuleError("Msg_ValidationRule_LastRegularPeriodStartDate", new string[] {  
                                    DtFunc.DateTimeToString(dtLastRegularPeriodEndDate, DtFunc.FmtShortDate),
                                    DtFunc.DateTimeToString(dtTerminationDate, DtFunc.FmtShortDate) });
                }
            }
            //
            if (IsCheckError) // systematique en mode Erreur
            {
                if (pIrsCurrent.calculationPeriodAmount.calculationSpecified)
                {
                    DateTime dtLastItem = dtEffectiveDate;
                    ICalculation cal = pIrsCurrent.calculationPeriodAmount.calculation;
                    if ((cal.notionalSpecified))
                    {
                        if (cal.notional.stepSchedule.stepSpecified)
                        {
                            IStep[] step = cal.notional.stepSchedule.step;
                            if (ArrFunc.IsFilled(step))
                            {
                                if (0 >= step[0].stepDate.DateValue.CompareTo(dtEffectiveDate))
                                    SetValidationRuleError("Msg_ValidationRule_StepInvalid", GetIdAdditionalInfo(cal.notional.stepSchedule.id));
                                //
                                // 20090617 RD ( Pour un Repo la date d'échéance n'est pas obligatoire)
                                if (DtFunc.IsDateTimeFilled(dtTerminationDate) && 0 >= dtTerminationDate.CompareTo(step[step.Length - 1].stepDate.DateValue))
                                    SetValidationRuleError("Msg_ValidationRule_StepInvalid", GetIdAdditionalInfo(cal.notional.stepSchedule.id));
                            }
                            //
                            if (cal.notional.stepParametersSpecified)
                            {
                                INotionalStepRule notionalStepParameters = cal.notional.stepParameters;
                                // Todo
                            }
                        }
                    }
                }
            }
            //
            if (null != pIrsPrevious)
            {
                if (IsToCheck("VRSTREAMSTART"))
                {
                    if (false == (dtEffectiveDate.CompareTo(pIrsPrevious.calculationPeriodDates.effectiveDateAdjustable.unadjustedDate.DateValue) == 0))
                        SetValidationRuleError("Msg_ValidationRule_DifferentDtEffective");
                }
                // 20090617 RD ( Pour un Repo la date d'échéance n'est pas obligatoire)
                if (IsToCheck("VRSTREAMEND") && DtFunc.IsDateTimeFilled(dtTerminationDate))
                {
                    if (false == (dtTerminationDate.CompareTo(pIrsPrevious.calculationPeriodDates.terminationDateAdjustable.unadjustedDate.DateValue) == 0))
                        SetValidationRuleError("Msg_ValidationRule_DifferentDtTermination");
                }
            }
            //

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pIrsCurrent"></param>
        /// <param name="pIrsPrevious"></param>
        private void CheckVR_IRDStream(IInterestRateStream pIrsCurrent, IInterestRateStream pIrsPrevious)
        {

            if (StrFunc.IsFilled(pIrsCurrent.payerPartyReference.hRef) &&
                StrFunc.IsFilled(pIrsCurrent.receiverPartyReference.hRef))
            {
                if ((0 == pIrsCurrent.payerPartyReference.hRef.CompareTo(pIrsCurrent.receiverPartyReference.hRef)))
                    SetValidationRuleError("Msg_ValidationRule_IdenticPayerReceiver1");
            }
            //Check Reset object
            if (pIrsCurrent.calculationPeriodAmount.calculationSpecified)
            {
                ICalculation calculation = pIrsCurrent.calculationPeriodAmount.calculation;
                if (calculation.rateFixedRateSpecified && (pIrsCurrent.resetDatesSpecified))
                    SetValidationRuleError("Msg_ValidationRule_Instr_ResetOnFixedRate");
                if ((calculation.rateFloatingRateSpecified || calculation.rateInflationRateSpecified) &&
                    (false == pIrsCurrent.resetDatesSpecified))
                    SetValidationRuleError("Msg_ValidationRule_Instr_NoResetOnFloatingRate");
            }
            //
            if (false == pIrsCurrent.principalExchangesSpecified)
            {
                //TODO
            }
            //
            if ((PeriodEnum.T == pIrsCurrent.calculationPeriodDates.calculationPeriodFrequency.interval.period) &&
                (1 != pIrsCurrent.calculationPeriodDates.calculationPeriodFrequency.interval.periodMultiplier.IntValue))
                SetValidationRuleError("Msg_ValidationRule_PeriodMultiplierTerme");

        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="pFxLegCurrent"></param>
        /// <param name="pFxLegPrevious"></param>
        private void CheckVR_FXLeg(IFxLeg pFxLegCurrent, IFxLeg pFxLegPrevious)
        {

            bool isOk = true;
            //
            if (IsCheckError) // systematique en mode Erreur
            {
                if (StrFunc.IsFilled(pFxLegCurrent.exchangedCurrency1.payerPartyReference.hRef) &&
                    StrFunc.IsFilled(pFxLegCurrent.exchangedCurrency1.receiverPartyReference.hRef))
                    isOk = (0 != pFxLegCurrent.exchangedCurrency1.payerPartyReference.hRef.CompareTo(pFxLegCurrent.exchangedCurrency1.receiverPartyReference.hRef));
                //		
                if (isOk)
                {
                    if (StrFunc.IsFilled(pFxLegCurrent.exchangedCurrency2.payerPartyReference.hRef) &&
                        StrFunc.IsFilled(pFxLegCurrent.exchangedCurrency2.receiverPartyReference.hRef))
                        isOk = (0 != pFxLegCurrent.exchangedCurrency2.payerPartyReference.hRef.CompareTo(pFxLegCurrent.exchangedCurrency2.receiverPartyReference.hRef));
                }
                //
                if (false == isOk)
                    SetValidationRuleError("Msg_ValidationRule_IdenticPayerReceiver1");
            }
            //	
            if (IsToCheck("VRFXCTRV"))
            {
                decimal exchangeAmountRounded = 0;
                isOk = CompareAmount(out exchangeAmountRounded, BoolFunc.IsTrue(Convert.ToInt32(sqlInstrument.GetFirstRowColumnValue("ISFXCTRVEXCEPTDEC"))),
                    pFxLegCurrent.exchangeRate.quotedCurrencyPair.quoteBasis,
                    pFxLegCurrent.exchangeRate.rate.DecValue,
                    pFxLegCurrent.exchangedCurrency1.paymentAmount.amount.DecValue, pFxLegCurrent.exchangedCurrency1.paymentAmount.currency,
                    pFxLegCurrent.exchangedCurrency2.paymentAmount.amount.DecValue, pFxLegCurrent.exchangedCurrency2.paymentAmount.currency);
                if (false == isOk)
                    SetValidationRuleError("Msg_ValidationRule_MtCtrInvalid", new string[] {  
                            StrFunc.FmtAmountToGUI(StrFunc.FmtDecimalToInvariantCulture(pFxLegCurrent.exchangedCurrency1.paymentAmount.amount.DecValue)),
                            StrFunc.FmtAmountToGUI(StrFunc.FmtDecimalToInvariantCulture(exchangeAmountRounded)) });
            }
            //				
            if (IsToCheck("VRFORWARDPOINTS"))
            {
                if (pFxLegCurrent.exchangeRate.spotRateSpecified && (false == pFxLegCurrent.exchangeRate.forwardPointsSpecified))
                    SetValidationRuleError("Msg_ValidationRule_NoForwardPoints");
            }
            //
            if (IsToCheck("VRSPOTRATE"))
            {
                if (false == pFxLegCurrent.exchangeRate.spotRateSpecified && (pFxLegCurrent.exchangeRate.forwardPointsSpecified))
                    SetValidationRuleError("Msg_ValidationRule_NoSpot");
            }
            //
            if (IsToCheck("VRFXRATE"))
            {
                if (pFxLegCurrent.exchangeRate.spotRateSpecified && pFxLegCurrent.exchangeRate.forwardPointsSpecified)
                {
                    if (false == (pFxLegCurrent.exchangeRate.rate.DecValue == (pFxLegCurrent.exchangeRate.spotRate.DecValue + pFxLegCurrent.exchangeRate.forwardPoints.DecValue)))
                        SetValidationRuleError("Msg_ValidationRule_ExchangeRateInvalid");
                }
            }
            //
            if (pFxLegPrevious != null)
            {
                if (IsToCheck("VRFXSWAPAMOUNTIDC1"))
                {
                    if (pFxLegPrevious.exchangedCurrency1.paymentAmount.amount.DecValue != pFxLegCurrent.exchangedCurrency1.paymentAmount.amount.DecValue)
                        SetValidationRuleError("Msg_ValidationRule_DifferentExchangedCurrency1");
                }
            }
            //
            CheckVR_FXLegDate(pFxLegCurrent, pFxLegPrevious);
            CheckVR_FXLegPayment(pFxLegCurrent, pFxLegPrevious);

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pFxLegCurrent"></param>
        /// <param name="pFxLegPrevious"></param>
        private void CheckVR_FXLegDate(IFxLeg pFxLegCurrent, IFxLeg pFxLegPrevious)
        {

            string[] idA = GetParties(pFxLegCurrent.exchangedCurrency1, true);
            string[] idC = null;
            ArrayList al = new ArrayList();
            if (StrFunc.IsFilled(pFxLegCurrent.exchangedCurrency1.paymentAmount.currency))
                al.Add(pFxLegCurrent.exchangedCurrency1.paymentAmount.currency);
            if (StrFunc.IsFilled(pFxLegCurrent.exchangedCurrency2.paymentAmount.currency))
                al.Add(pFxLegCurrent.exchangedCurrency2.paymentAmount.currency);
            if (ArrFunc.IsFilled(al))
                idC = (string[])al.ToArray(typeof(string));
            //
            IBusinessCenters bcs = m_Input.DataDocument.currentProduct.productBase.LoadBusinessCenters(CSTools.SetCacheOn(CS), idA, idC, null);
            IBusinessDayAdjustments bda = bcs.GetBusinessDayAdjustments(BusinessDayConventionEnum.FOLLOWING);
            IBusinessDayAdjustments bdaWithoutBC = m_Input.DataDocument.currentProduct.productBase.CreateBusinessDayAdjustments(BusinessDayConventionEnum.FOLLOWING);
            //20061201 PL Rajouter ctrl sur fixingdate, ...
            DateTime dtTransac = m_Input.DataDocument.currentTrade.tradeHeader.tradeDate.DateValue;
            DateTime fxDateValue = DateTime.MinValue;
            //
            if (pFxLegCurrent.fxDateValueDateSpecified)
                fxDateValue = pFxLegCurrent.fxDateValueDate.DateValue;
            //
            if (DtFunc.IsDateTimeFilled(fxDateValue))
            {
                EFS_BusinessCenters efs_bc = new EFS_BusinessCenters(CS, bcs);
                if (efs_bc.businessCentersSpecified)
                {
                    if (IsToCheck("VRTRANSHOLIDAY"))
                    {
                        if (efs_bc.IsHoliday(dtTransac))
                            SetValidationRuleError("Msg_ValidationRule_DtTransacIsHoliday", new string[] { DtFunc.DateTimeToString(dtTransac, DtFunc.FmtShortDate) });
                    }
                    if (IsToCheck("VRSTAHOLIDAY"))
                    {
                        if (efs_bc.IsHoliday(fxDateValue))
                            SetValidationRuleError("Msg_ValidationRule_DtEffectiveIsHoliday", new string[] { DtFunc.DateTimeToString(fxDateValue, DtFunc.FmtShortDate) });
                    }
                    //20090629 PL Ctrl mis en commentaire car redondant avec VRSTAHOLIDAY (de plus cette donnée n'est plus visible sur l'IHM pour un FX)
                    //if (IsToCheck("VRENDHOLIDAY") && m_Input.DataDocument.isFxLeg)
                    //{
                    //    if (efs_bc.IsHoliday(fxDateValue))
                    //        SetValidationRuleError("Msg_ValidationRule_DtTerminationIsHoliday", new string[] { DtFunc.DateTimeToString(fxDateValue, DtFunc.FmtShortDate) });
                    //}
                    //
                    if (IsToCheck("VRENDHOLIDAY") && m_Input.DataDocument.currentProduct.isFxSwap && (pFxLegPrevious != null))
                    {
                        if (efs_bc.IsHoliday(fxDateValue))
                            SetValidationRuleError("Msg_ValidationRule_DtTerminationIsHoliday", new string[] { DtFunc.DateTimeToString(fxDateValue, DtFunc.FmtShortDate) });
                    }
                }
                //	
                if (null == pFxLegPrevious) //1er leg
                {
                    if (IsToCheck("VRSTAAFTTRANSMIN"))
                    {
                        int offset = Convert.ToInt32(sqlInstrument.GetFirstRowColumnValue("STAAFTTRANSMIN"));
                        bool isBusinness = BoolFunc.IsTrue(sqlInstrument.GetFirstRowColumnValue("STAAFTTRANSMINB").ToString());
                        IInterval interval = m_Input.DataDocument.currentTrade.product.productBase.CreateInterval(PeriodEnum.D, offset);
                        DateTime dtMinEffective = Tools.ApplyAdjustedInterval(CSTools.SetCacheOn(CS), dtTransac, interval, isBusinness ? bda : bdaWithoutBC);
                        if (0 > fxDateValue.CompareTo(dtMinEffective))
                            SetValidationRuleError("Msg_ValidationRule_DtEffective2", new string[] { 
                                    DtFunc.DateTimeToString(dtTransac, DtFunc.FmtShortDate), 
                                    DtFunc.DateTimeToString(fxDateValue, DtFunc.FmtShortDate), 
                                    offset.ToString() });
                    }
                    if (IsToCheck("VRSTAAFTTRANSMAX"))
                    {
                        int offset = Convert.ToInt32(sqlInstrument.GetFirstRowColumnValue("STAAFTTRANSMAX"));
                        bool isBusinness = BoolFunc.IsTrue(sqlInstrument.GetFirstRowColumnValue("STAAFTTRANSMAXB").ToString());
                        IInterval interval = m_Input.DataDocument.currentTrade.product.productBase.CreateInterval(PeriodEnum.D, offset);
                        DateTime dtMaxEffective = Tools.ApplyAdjustedInterval(CSTools.SetCacheOn(CS), dtTransac, interval, isBusinness ? bda : bdaWithoutBC);
                        if (0 < fxDateValue.CompareTo(dtMaxEffective))
                            SetValidationRuleError("Msg_ValidationRule_DtEffective3", new string[] { 
                                    DtFunc.DateTimeToString(dtTransac, DtFunc.FmtShortDate), 
                                    DtFunc.DateTimeToString(fxDateValue, DtFunc.FmtShortDate), 
                                    offset.ToString() });
                    }
                }
                else //2ème leg
                {
                    if (IsToCheck("VRENDAFTSTAMIN"))
                    {
                        int offset = Convert.ToInt32(sqlInstrument.GetFirstRowColumnValue("ENDAFTSTAMIN"));
                        bool isBusinness = BoolFunc.IsTrue(sqlInstrument.GetFirstRowColumnValue("ENDAFTSTAMINB").ToString());
                        IInterval interval = m_Input.DataDocument.currentTrade.product.productBase.CreateInterval(PeriodEnum.D, offset);
                        DateTime dateValue = DateTime.MinValue;
                        if (pFxLegPrevious.fxDateValueDateSpecified)
                            dateValue = pFxLegCurrent.fxDateValueDate.DateValue;
                        //
                        if (DtFunc.IsDateTimeFilled(dateValue))
                        {
                            DateTime dtMinEndDate = Tools.ApplyAdjustedInterval(CSTools.SetCacheOn(CS), dateValue, interval, isBusinness ? bda : bdaWithoutBC);
                            if (0 > fxDateValue.CompareTo(dtMinEndDate))
                                SetValidationRuleError("Msg_ValidationRule_DtTermination2", new string[] { 
                                        DtFunc.DateTimeToString(dateValue, DtFunc.FmtShortDate), 
                                        DtFunc.DateTimeToString(fxDateValue, DtFunc.FmtShortDate), 
                                        offset.ToString() });
                        }
                    }
                    if (IsToCheck("VRENDAFTSTAMAX"))
                    {
                        int offset = Convert.ToInt32(sqlInstrument.GetFirstRowColumnValue("ENDAFTSTAMAX"));
                        bool isBusinness = BoolFunc.IsTrue(sqlInstrument.GetFirstRowColumnValue("ENDAFTSTAMAXB").ToString());
                        IInterval interval = m_Input.DataDocument.currentTrade.product.productBase.CreateInterval(PeriodEnum.D, offset);
                        DateTime dateValue = DateTime.MinValue;
                        if (pFxLegPrevious.fxDateValueDateSpecified)
                            dateValue = pFxLegCurrent.fxDateValueDate.DateValue;
                        //
                        if (DtFunc.IsDateTimeFilled(dateValue))
                        {
                            DateTime dtMaxEndDate = Tools.ApplyAdjustedInterval(CSTools.SetCacheOn(CS), dateValue, interval, isBusinness ? bda : bdaWithoutBC);
                            if (0 < fxDateValue.CompareTo(dtMaxEndDate))
                                SetValidationRuleError("Msg_ValidationRule_DtTermination3", new string[] { 
                                        DtFunc.DateTimeToString(dateValue, DtFunc.FmtShortDate), 
                                        DtFunc.DateTimeToString(fxDateValue, DtFunc.FmtShortDate), 
                                        offset.ToString() });
                        }
                    }
                }
            }
            //
            if (IsCheckError) // systematique en mode Erreur
            {
                if (pFxLegPrevious != null)
                {
                    if (pFxLegPrevious.fxDateValueDateSpecified)
                    {
                        DateTime fxDateValuePrevious = pFxLegPrevious.fxDateValueDate.DateValue;
                        //
                        if (0 > fxDateValue.CompareTo(fxDateValuePrevious))
                            SetValidationRuleError("Msg_ValidationRule_FxValueDate1");
                    }
                }
            }

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pFxLegCurrent"></param>
        /// <param name="pFxLegPrevious"></param>
        private void CheckVR_FXLegPayment(IFxLeg pFxLegCurrent, IFxLeg pFxLegPrevious)
        {

            if (IsCheckError) // systematique en mode Erreur
            {
                if (pFxLegCurrent.exchangedCurrency1.paymentAmount.currency == pFxLegCurrent.exchangedCurrency2.paymentAmount.currency)
                    SetValidationRuleError("Msg_ValidationRule_IdenticCurrency");
            }
            //
            if (pFxLegPrevious != null)
            {
                // si les payeurs sont différents => les devises doivent être identiques
                // si les payeurs sont différents => MT1 doit être identique sur chaque Leg
                if (IsCheckError) // systematique en mode Erreur
                {
                    if (pFxLegPrevious.exchangedCurrency1.payerPartyReference != pFxLegCurrent.exchangedCurrency1.payerPartyReference)
                    {
                        if (pFxLegPrevious.exchangedCurrency1.paymentAmount.currency != pFxLegCurrent.exchangedCurrency1.paymentAmount.currency)
                            SetValidationRuleError("Msg_ValidationRule_DifferentCurrency");
                        //if (pFxLegPrevious.exchangedCurrency1.paymentAmount.amount.DecValue != pFxLegCurrent.exchangedCurrency1.paymentAmount.amount.DecValue)
                        //    SetValidationRuleError("Msg_ValidationRule_DifferentExchangedCurrency1");
                    }
                }
                //
                if (IsCheckError) // systematique en mode Erreur
                {
                    if (pFxLegPrevious.exchangedCurrency2.payerPartyReference != pFxLegCurrent.exchangedCurrency2.payerPartyReference)
                    {
                        if (pFxLegPrevious.exchangedCurrency2.paymentAmount.currency != pFxLegCurrent.exchangedCurrency2.paymentAmount.currency)
                            SetValidationRuleError("Msg_ValidationRule_DifferentCurrency");
                    }
                }
                //
                if (IsCheckError) // systematique en mode Erreur
                {
                    if (pFxLegCurrent.exchangeRate.spotRateSpecified)
                    {
                        if (pFxLegCurrent.exchangeRate.spotRate.DecValue != pFxLegPrevious.exchangeRate.rate.DecValue)
                            SetValidationRuleError("Msg_ValidationRule_DifferentSpotAndRate");
                    }
                }
            }

        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="pFxOptionLeg"></param>
        private void CheckVR_FXOption(object pFxOptionLeg)
        {

            bool isOk = true;
            IReference buyer = ((IFxOptionBase)pFxOptionLeg).buyerPartyReference;
            IReference seller = ((IFxOptionBase)pFxOptionLeg).sellerPartyReference;
            //
            if (IsCheckError) // systematique en mode Erreur
            {
                if (StrFunc.IsFilled(seller.hRef) && StrFunc.IsFilled(buyer.hRef))
                {
                    if (false == (0 != seller.hRef.CompareTo(buyer.hRef)))
                        SetValidationRuleError("Msg_ValidationRule_IdenticBuyerSeller");
                }
            }
            //
            if ((false == m_Input.DataDocument.currentProduct.isFxDigitalOption))
            {
                IMoney callCurrencyAmount = ((IFxOptionBase)pFxOptionLeg).callCurrencyAmount;
                IMoney putCurrencyAmount = ((IFxOptionBase)pFxOptionLeg).putCurrencyAmount;
                IFxStrikePrice fxStrikePrice = ((IFxOptionBaseNotDigital)pFxOptionLeg).fxStrikePrice;
                //
                if (IsCheckError) // systematique en mode Erreur
                {
                    if (false == (0 != callCurrencyAmount.currency.CompareTo(putCurrencyAmount.currency)))
                        SetValidationRuleError("Msg_ValidationRule_IdenticCurrency");
                }
                //
                if (IsToCheck("VRFXCTRV"))
                {
                    decimal exchangeAmountRounded = 0;
                    isOk = CompareAmount(out exchangeAmountRounded, BoolFunc.IsTrue(Convert.ToInt32(sqlInstrument.GetFirstRowColumnValue("ISFXCTRVEXCEPTDEC"))),
                        (fxStrikePrice.strikeQuoteBasis == StrikeQuoteBasisEnum.CallCurrencyPerPutCurrency) ? QuoteBasisEnum.Currency1PerCurrency2 : QuoteBasisEnum.Currency2PerCurrency1,
                        fxStrikePrice.rate.DecValue, callCurrencyAmount.amount.DecValue, callCurrencyAmount.currency, putCurrencyAmount.amount.DecValue, putCurrencyAmount.currency);
                    if (false == isOk)
                        SetValidationRuleError("Msg_ValidationRule_MtCtrInvalid", new string[] {   
                                StrFunc.FmtAmountToGUI(StrFunc.FmtDecimalToInvariantCulture(callCurrencyAmount.amount.DecValue)),
                                StrFunc.FmtAmountToGUI(StrFunc.FmtDecimalToInvariantCulture(exchangeAmountRounded)) });
                }
            }
            //
            IFxOptionPremium[] fxOptionPremium = ((IFxOptionBase)pFxOptionLeg).fxOptionPremium;
            if (ArrFunc.IsFilled(fxOptionPremium))
            {
                for (int i = 0; i < ArrFunc.Count(fxOptionPremium); i++)
                {
                    if (IsCheckError) // systematique en mode Erreur
                    {
                        if (StrFunc.IsFilled(fxOptionPremium[i].payerPartyReference.hRef) &&
                            StrFunc.IsFilled(fxOptionPremium[i].receiverPartyReference.hRef))
                        {
                            if (false == (0 != fxOptionPremium[i].payerPartyReference.hRef.CompareTo(fxOptionPremium[i].receiverPartyReference.hRef)))
                                SetValidationRuleError("Msg_ValidationRule_FxOptionPremiumIdenticPayerReceiver");
                        }
                    }
                    //						
                    if (IsToCheck("VRPREMIUMPAYER"))
                    {
                        if (StrFunc.IsFilled(fxOptionPremium[i].payerPartyReference.hRef) &&
                            StrFunc.IsFilled(buyer.hRef))
                        {
                            if (false == (0 == fxOptionPremium[i].payerPartyReference.hRef.CompareTo(buyer.hRef)))
                                SetValidationRuleError("Msg_ValidationRule_FxOptionPremiumPayerInvalid");
                        }
                    }
                    //
                    if (IsToCheck("VRFXPREMIUM"))
                    {
                        if ((false == m_Input.DataDocument.currentProduct.isFxDigitalOption)
                            && fxOptionPremium[i].premiumQuoteSpecified
                            && (fxOptionPremium[i].premiumQuote.premiumQuoteBasis != PremiumQuoteBasisEnum.Explicit))
                        {
                            IMoney callCurrencyAmount = ((IFxOptionBase)pFxOptionLeg).callCurrencyAmount;
                            IMoney putCurrencyAmount = ((IFxOptionBase)pFxOptionLeg).putCurrencyAmount;
                            decimal amount = 0;
                            string cur = string.Empty;
                            switch (fxOptionPremium[i].premiumQuote.premiumQuoteBasis)
                            {
                                case PremiumQuoteBasisEnum.PercentageOfCallCurrencyAmount:
                                    amount = fxOptionPremium[i].premiumQuote.premiumValue.DecValue * callCurrencyAmount.amount.DecValue;
                                    cur = callCurrencyAmount.currency;
                                    break;
                                case PremiumQuoteBasisEnum.PercentageOfPutCurrencyAmount:
                                    amount = fxOptionPremium[i].premiumQuote.premiumValue.DecValue * putCurrencyAmount.amount.DecValue;
                                    cur = putCurrencyAmount.currency;
                                    break;
                                case PremiumQuoteBasisEnum.CallCurrencyPerPutCurrency:
                                    amount = fxOptionPremium[i].premiumQuote.premiumValue.DecValue * putCurrencyAmount.amount.DecValue;
                                    cur = callCurrencyAmount.currency;
                                    break;
                                case PremiumQuoteBasisEnum.PutCurrencyPerCallCurrency:
                                    amount = fxOptionPremium[i].premiumQuote.premiumValue.DecValue * callCurrencyAmount.amount.DecValue;
                                    cur = putCurrencyAmount.currency;
                                    break;
                            }
                            EFS_Cash cash = new EFS_Cash(CSTools.SetCacheOn(CS), amount, cur);
                            //
                            if (false == ((0 == fxOptionPremium[i].premiumAmount.currency.CompareTo(cur)) &&
                                (0 == fxOptionPremium[i].premiumAmount.amount.DecValue.CompareTo(cash.AmountRounded))))
                            {
                                string valueTheo = StrFunc.FmtAmountToGUI(StrFunc.FmtDecimalToInvariantCulture(cash.AmountRounded)) + Cst.Space + cur;
                                string mtPrime = StrFunc.FmtAmountToGUI(StrFunc.FmtDecimalToInvariantCulture(fxOptionPremium[i].premiumAmount.amount.DecValue)) + Cst.Space + fxOptionPremium[i].premiumAmount.currency;
                                //
                                SetValidationRuleError("Msg_ValidationRule_FxOptionPremiumAmountInvalid", new string[] { mtPrime, valueTheo });
                            }
                        }
                    }
                }
            }
            CheckVR_FXOptionDate(pFxOptionLeg);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pFxOptionLeg"></param>
        private void CheckVR_FXOptionDate(object pFxOptionLeg)
        {

            DateTime dtTransac = m_Input.DataDocument.currentTrade.tradeHeader.tradeDate.DateValue;
            DateTime dtExpiryDate = ((IFxOptionBase)pFxOptionLeg).expiryDateTime.expiryDate.DateValue;
            DateTime valueDate = ((IFxOptionBase)pFxOptionLeg).valueDate.DateValue;
            //
            string[] idA = GetParties(pFxOptionLeg, false);
            string[] idC = null;
            ArrayList al = new ArrayList();
            if (false == m_Input.DataDocument.currentProduct.isFxDigitalOption)
            {
                al.Add(((IFxOptionBase)pFxOptionLeg).callCurrencyAmount.currency);
                al.Add(((IFxOptionBase)pFxOptionLeg).putCurrencyAmount.currency);
            }
            else
            {
                IQuotedCurrencyPair qcp = ((IFxDigitalOption)pFxOptionLeg).quotedCurrencyPair;
                al.Add(qcp.currency1);
                al.Add(qcp.currency2);
            }
            //
            if (ArrFunc.IsFilled(al))
                idC = (string[])al.ToArray(typeof(string));
            //
            IBusinessCenters bcs = ((IProductBase)pFxOptionLeg).LoadBusinessCenters( CSTools.SetCacheOn(CS), idA, idC, null);
            IBusinessDayAdjustments bda = bcs.GetBusinessDayAdjustments(BusinessDayConventionEnum.FOLLOWING);
            IBusinessDayAdjustments bdaWithoutBC = m_Input.DataDocument.currentTrade.product.productBase.CreateBusinessDayAdjustments(BusinessDayConventionEnum.FOLLOWING);
            //
            if (IsCheckError) // systematique en mode Erreur
            {
                if (0 > dtExpiryDate.CompareTo(dtTransac))
                    SetValidationRuleError("Msg_ValidationRule_DtTermination1", new string[] { 
                            DtFunc.DateTimeToString(dtExpiryDate, DtFunc.FmtShortDate),
                            DtFunc.DateTimeToString(dtTransac, DtFunc.FmtShortDate)});
            }
            //
            if (IsCheckError) // systematique en mode Erreur
            {
                if (0 > valueDate.CompareTo(dtExpiryDate))
                    SetValidationRuleError("Msg_ValidationRule_FxOptionValueDate1", new string[] { 
                            DtFunc.DateTimeToString(valueDate, DtFunc.FmtShortDate),
                            DtFunc.DateTimeToString(dtExpiryDate, DtFunc.FmtShortDate)});
            }
            //20090630 PL Add VRTRANSHOLIDAY
            EFS_BusinessCenters efs_bc = new EFS_BusinessCenters(CSTools.SetCacheOn(CS), bcs);
            if (efs_bc.businessCentersSpecified)
            {
                if (IsToCheck("VRTRANSHOLIDAY"))
                {
                    if (efs_bc.IsHoliday(dtTransac))
                        SetValidationRuleError("Msg_ValidationRule_DtTransacIsHoliday", new string[] { DtFunc.DateTimeToString(dtTransac, DtFunc.FmtShortDate) });
                }
            }
            //
            if (IsToCheck("VRENDAFTSTAMIN"))
            {
                int offset = Convert.ToInt32(sqlInstrument.GetFirstRowColumnValue("ENDAFTSTAMIN"));
                bool isBusinness = BoolFunc.IsTrue(sqlInstrument.GetFirstRowColumnValue("ENDAFTSTAMINB").ToString());
                IInterval interval = m_Input.DataDocument.currentTrade.product.productBase.CreateInterval(PeriodEnum.D, offset);
                DateTime dtMinEnd = Tools.ApplyAdjustedInterval(CSTools.SetCacheOn(CS), dtTransac, interval, isBusinness ? bda : bdaWithoutBC);
                if (0 > dtExpiryDate.CompareTo(dtMinEnd))
                    SetValidationRuleError("Msg_ValidationRule_DtTermination2", new string[] { 
                                        DtFunc.DateTimeToString(dtTransac, DtFunc.FmtShortDate), 
                                        DtFunc.DateTimeToString(dtExpiryDate, DtFunc.FmtShortDate), 
                                        offset.ToString() });
            }
            //
            if (IsToCheck("VRENDAFTSTAMAX"))
            {
                int offset = Convert.ToInt32(sqlInstrument.GetFirstRowColumnValue("ENDAFTSTAMAX"));
                bool isBusinness = BoolFunc.IsTrue(sqlInstrument.GetFirstRowColumnValue("ENDAFTSTAMAXB").ToString());
                IInterval interval = m_Input.DataDocument.currentTrade.product.productBase.CreateInterval(PeriodEnum.D, offset);
                DateTime dtMaxEnd = Tools.ApplyAdjustedInterval(CSTools.SetCacheOn(CS), dtTransac, interval, isBusinness ? bda : bdaWithoutBC);
                if (0 < dtExpiryDate.CompareTo(dtMaxEnd))
                    SetValidationRuleError("Msg_ValidationRule_DtTermination3", new string[] { 
                                        DtFunc.DateTimeToString(dtTransac, DtFunc.FmtShortDate), 
                                        DtFunc.DateTimeToString(dtExpiryDate, DtFunc.FmtShortDate), 
                                        offset.ToString() });
            }
            //
            if (IsCheckError) // systematique en mode Erreur
            {
                IFxOptionPremium[] fxOptionPremium = ((IFxOptionBase)pFxOptionLeg).fxOptionPremium;
                if (ArrFunc.IsFilled(fxOptionPremium))
                {
                    for (int i = 0; i < ArrFunc.Count(fxOptionPremium); i++)
                    {
                        if (0 > fxOptionPremium[i].premiumSettlementDate.DateValue.CompareTo(dtTransac))
                            SetValidationRuleError("Msg_ValidationRule_FxOptionPremiumValueDate1", new string[] { 
                                    DtFunc.DateTimeToString(fxOptionPremium[i].premiumSettlementDate.DateValue, DtFunc.FmtShortDate),
                                    DtFunc.DateTimeToString(dtTransac, DtFunc.FmtShortDate)});
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pSaleAndRepurchaseAgreement"></param>
        private void CheckVR_SalesAndRepurchaseAgreement(ISaleAndRepurchaseAgreement pSaleAndRepurchaseAgreement)
        {

            if (IsToCheck("VRREPOQTYSPOTFWD") && pSaleAndRepurchaseAgreement.forwardLegSpecified)
            {
                bool isRepoQtyExceptDec = BoolFunc.IsTrue(Convert.ToInt32(sqlInstrument.GetFirstRowColumnValue("ISREPOQTYEXCEPTDEC")));
                bool isOk = true;
                //
                for (int i = 0; i < ArrFunc.Count(pSaleAndRepurchaseAgreement.spotLeg); i++)
                {
                    if (i < ArrFunc.Count(pSaleAndRepurchaseAgreement.forwardLeg))
                    {
                        decimal spotAmount = pSaleAndRepurchaseAgreement.spotLeg[i].debtSecurityTransaction.quantity.notionalAmount.amount.DecValue;
                        decimal forwardAmount = pSaleAndRepurchaseAgreement.forwardLeg[i].debtSecurityTransaction.quantity.notionalAmount.amount.DecValue;
                        //
                        isOk = CompareAmount(isRepoQtyExceptDec, spotAmount, forwardAmount);
                        //
                        if (isOk && pSaleAndRepurchaseAgreement.spotLeg[i].debtSecurityTransaction.quantity.numberOfUnitsSpecified &&
                            pSaleAndRepurchaseAgreement.forwardLeg[i].debtSecurityTransaction.quantity.numberOfUnitsSpecified)
                        {
                            decimal spotQty = pSaleAndRepurchaseAgreement.spotLeg[i].debtSecurityTransaction.quantity.numberOfUnits.DecValue;
                            decimal forwardQty = pSaleAndRepurchaseAgreement.forwardLeg[i].debtSecurityTransaction.quantity.numberOfUnits.DecValue;
                            //
                            isOk = CompareAmount(isRepoQtyExceptDec, spotQty, forwardQty);
                        }
                    }
                    //
                    if (false == isOk)
                    {
                        SetValidationRuleError("Msg_ValidationRule_VRREPOQTYSPOTFWD");
                        break;
                    }
                }
            }
            //
            IAdjustableDate adjustableDate;
            DateTime dtCashStreamMaxTerminationDate = DateTime.MinValue;
            //
            for (int i = 0; i < ArrFunc.Count(pSaleAndRepurchaseAgreement.cashStream); i++)
            {
                if (pSaleAndRepurchaseAgreement.cashStream[i].calculationPeriodDates.terminationDateAdjustableSpecified)
                {
                    adjustableDate = pSaleAndRepurchaseAgreement.cashStream[i].calculationPeriodDates.terminationDateAdjustable;
                    if ((0 > adjustableDate.unadjustedDate.DateValue.CompareTo(dtCashStreamMaxTerminationDate)) || DtFunc.IsDateTimeEmpty(dtCashStreamMaxTerminationDate))
                        dtCashStreamMaxTerminationDate = adjustableDate.unadjustedDate.DateValue;
                }
            }
            //                                        
            if (IsToCheck("VRREPOSTAEND"))
            {
                bool isOk = true;
                //
                DateTime dtCashStreamMinStartDate = DateTime.MinValue;
                DateTime dtSpotMinPaymentDate = DateTime.MinValue;
                DateTime dtForwardMaxPaymentDate = DateTime.MinValue;
                //
                for (int i = 0; i < ArrFunc.Count(pSaleAndRepurchaseAgreement.cashStream); i++)
                {
                    adjustableDate = pSaleAndRepurchaseAgreement.cashStream[i].calculationPeriodDates.effectiveDateAdjustable;
                    if ((0 > adjustableDate.unadjustedDate.DateValue.CompareTo(dtCashStreamMinStartDate)) || DtFunc.IsDateTimeEmpty(dtCashStreamMinStartDate))
                        dtCashStreamMinStartDate = adjustableDate.unadjustedDate.DateValue;
                }
                //
                for (int i = 0; i < ArrFunc.Count(pSaleAndRepurchaseAgreement.spotLeg); i++)
                {
                    if (pSaleAndRepurchaseAgreement.spotLeg[i].debtSecurityTransaction.grossAmount.paymentDateSpecified)
                    {
                        adjustableDate = pSaleAndRepurchaseAgreement.spotLeg[i].debtSecurityTransaction.grossAmount.paymentDate;
                        if ((0 < adjustableDate.unadjustedDate.DateValue.CompareTo(dtSpotMinPaymentDate)) || DtFunc.IsDateTimeEmpty(dtSpotMinPaymentDate))
                            dtSpotMinPaymentDate = adjustableDate.unadjustedDate.DateValue;
                    }
                }
                //
                if (0 != dtCashStreamMinStartDate.CompareTo(dtSpotMinPaymentDate))
                    isOk = false;
                //
                // Pour un Repo la date d'échéance n'est pas obligatoire
                if (isOk && DtFunc.IsDateTimeFilled(dtCashStreamMaxTerminationDate) && pSaleAndRepurchaseAgreement.forwardLegSpecified)
                {
                    for (int i = 0; i < ArrFunc.Count(pSaleAndRepurchaseAgreement.forwardLeg); i++)
                    {
                        if (pSaleAndRepurchaseAgreement.forwardLeg[i].debtSecurityTransaction.grossAmount.paymentDateSpecified)
                        {
                            adjustableDate = pSaleAndRepurchaseAgreement.forwardLeg[i].debtSecurityTransaction.grossAmount.paymentDate;
                            if ((0 > adjustableDate.unadjustedDate.DateValue.CompareTo(dtForwardMaxPaymentDate)) || DtFunc.IsDateTimeEmpty(dtForwardMaxPaymentDate))
                                dtForwardMaxPaymentDate = adjustableDate.unadjustedDate.DateValue;
                        }
                    }
                    //
                    if (0 != dtCashStreamMaxTerminationDate.CompareTo(dtForwardMaxPaymentDate))
                        isOk = false;
                }
                //
                if (false == isOk)
                    SetValidationRuleError("Msg_ValidationRule_VRREPOSTAEND");
            }
            //
            if (IsToCheck("VRREPOAMOUNTSRATE"))
            {
                //
                // La vérification est faite uniquement dans le cas ou on a:
                // 1 seul CashStream
                // 1 seul SpotLeg
                // 1 seul ForwardLeg
                //
                if (1 == ArrFunc.Count(pSaleAndRepurchaseAgreement.cashStream) &&
                    (1 == ArrFunc.Count(pSaleAndRepurchaseAgreement.spotLeg)))
                {

                    bool isOk = true;
                    bool isRepoAmountExceptDec = BoolFunc.IsTrue(Convert.ToInt32(sqlInstrument.GetFirstRowColumnValue("ISREPOAMTEXCEPTDEC")));
                    decimal spotGrossAmount = pSaleAndRepurchaseAgreement.spotLeg[0].debtSecurityTransaction.grossAmount.paymentAmount.amount.DecValue;
                    //
                    if (m_Input.DataDocument.currentProduct.isRepo)
                    {
                        decimal cashStreamNotional = pSaleAndRepurchaseAgreement.cashStream[0].calculationPeriodAmount.calculation.notional.stepSchedule.initialValue.DecValue;
                        isOk = CompareAmount(isRepoAmountExceptDec, spotGrossAmount, cashStreamNotional);
                        //
                        if (isOk && pSaleAndRepurchaseAgreement.forwardLegSpecified &&
                            (1 == ArrFunc.Count(pSaleAndRepurchaseAgreement.forwardLeg)))
                        {
                            decimal forwardGrossAmount = pSaleAndRepurchaseAgreement.forwardLeg[0].debtSecurityTransaction.grossAmount.paymentAmount.amount.DecValue;
                            isOk = CompareAmount(isRepoAmountExceptDec, spotGrossAmount, forwardGrossAmount);
                        }
                        //
                        if (false == isOk)
                            SetValidationRuleError("Msg_ValidationRule_VRREPOAMOUNTSRATE");
                    }
                    else
                    {
                        SaleAndRepurchaseAgreementContainer repurchaseAgreementContainer = new SaleAndRepurchaseAgreementContainer(pSaleAndRepurchaseAgreement, m_Input.DataDocument.dataDocument);
                        //
                        decimal calculatedSpotGrossAmount = repurchaseAgreementContainer.CalcSpotGrossAmount();
                        isOk = CompareAmount(isRepoAmountExceptDec, spotGrossAmount, calculatedSpotGrossAmount);
                        //
                        if (false == isOk)
                            SetValidationRuleError("Msg_ValidationRule_VRBSBAMOUNTSRATE");
                    }
                }
            }
            //
            // Pour un Repo la date d'échéance n'est pas obligatoire
            if (IsToCheck("VRREPOENDSECURITIES") && DtFunc.IsDateTimeFilled(dtCashStreamMaxTerminationDate))
            {
                bool isOk = true;
                //
                for (int i = 0; i < ArrFunc.Count(pSaleAndRepurchaseAgreement.spotLeg); i++)
                {
                    DebtSecurityTransactionContainer debtSecurityTransactionContainer =
                        new DebtSecurityTransactionContainer(pSaleAndRepurchaseAgreement.spotLeg[i].debtSecurityTransaction, m_Input.DataDocument.dataDocument);
                    //
                    ISecurityAsset securityAsset = debtSecurityTransactionContainer.GetSecurityAssetInDataDocument();
                    // EG 20101014 Add Test (null != securityAsset)
                    if ((null != securityAsset) && securityAsset.debtSecuritySpecified)
                    {
                        for (int j = 0; j < ArrFunc.Count(securityAsset.debtSecurity.stream); j++)
                        {
                            if (securityAsset.debtSecurity.stream[j].calculationPeriodDates.terminationDateAdjustableSpecified)
                            {
                                adjustableDate = securityAsset.debtSecurity.stream[j].calculationPeriodDates.terminationDateAdjustable;
                                if (0 > adjustableDate.unadjustedDate.DateValue.CompareTo(dtCashStreamMaxTerminationDate))
                                {
                                    isOk = false;
                                    break;
                                }
                            }
                        }
                    }
                }
                //
                if (isOk && pSaleAndRepurchaseAgreement.forwardLegSpecified)
                {
                    for (int i = 0; i < ArrFunc.Count(pSaleAndRepurchaseAgreement.forwardLeg); i++)
                    {
                        DebtSecurityTransactionContainer debtSecurityTransactionContainer =
                            new DebtSecurityTransactionContainer(pSaleAndRepurchaseAgreement.forwardLeg[i].debtSecurityTransaction, m_Input.DataDocument.dataDocument);
                        //
                        ISecurityAsset securityAsset = debtSecurityTransactionContainer.GetSecurityAssetInDataDocument();
                        // EG 20101014 Add Test (null != securityAsset)
                        if ((null != securityAsset) && securityAsset.debtSecuritySpecified)
                        {
                            for (int j = 0; j < ArrFunc.Count(securityAsset.debtSecurity.stream); j++)
                            {
                                if (securityAsset.debtSecurity.stream[j].calculationPeriodDates.terminationDateAdjustableSpecified)
                                {
                                    adjustableDate = securityAsset.debtSecurity.stream[j].calculationPeriodDates.terminationDateAdjustable;
                                    if (0 > adjustableDate.unadjustedDate.DateValue.CompareTo(dtCashStreamMaxTerminationDate))
                                    {
                                        isOk = false;
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
                //
                if (false == isOk)
                    SetValidationRuleError("Msg_ValidationRule_VRREPOENDSECURITIES");
            }

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pDebtSecurityTransaction"></param>
        /// <param name="pIsToCheck_SettlementDate"></param>
        private void CheckVR_DebtSecurityTransaction(IDebtSecurityTransaction pDebtSecurityTransaction, bool pIsToCheck_SettlementDate)
        {

            DebtSecurityTransactionContainer debtSecurityTransactionContainer = new DebtSecurityTransactionContainer(pDebtSecurityTransaction, m_Input.DataDocument.dataDocument);
            //
            if (IsToCheck("VRDSEAINAMOUNT"))
            {
                //20090921 FI ctrl sur coupon couru uniquement si cleanPriceSpecified 
                if (pDebtSecurityTransaction.price.cleanPriceSpecified)
                {
                    if ((false == pDebtSecurityTransaction.price.accruedInterestAmountSpecified) ||
                        StrFunc.IsEmpty(pDebtSecurityTransaction.price.accruedInterestAmount.amount.Value))
                    {
                        SetValidationRuleError("Msg_ValidationRule_VRDSEAINAMOUNT");
                    }
                }
            }
            //
            bool isDseGAMExceptDec = BoolFunc.IsTrue(Convert.ToInt32(sqlInstrument.GetFirstRowColumnValue("ISDSEGAMEXCEPTDEC")));
            //
            if (IsToCheck("VRDSEAINRATE"))
            {
                //
                // Si le Montant et le Taux du coupon sont spécifiés, on recalcul le nouveau Montant et on compare avec le montant existant
                //
                if (pDebtSecurityTransaction.price.accruedInterestAmountSpecified &&
                    StrFunc.IsFilled(pDebtSecurityTransaction.price.accruedInterestAmount.amount.Value) &&
                    pDebtSecurityTransaction.price.accruedInterestRateSpecified &&
                    StrFunc.IsFilled(pDebtSecurityTransaction.price.accruedInterestRate.Value))
                {
                    IMoney interestAmount = debtSecurityTransactionContainer.CalcMtCC(m_Input.CS);
                    //
                    if (false == CompareAmount(isDseGAMExceptDec, interestAmount.amount.DecValue, pDebtSecurityTransaction.price.accruedInterestAmount.amount.DecValue))
                        SetValidationRuleError("Msg_ValidationRule_VRDSEAINRATE", new string[] {  
                                StrFunc.FmtDecimalToInvariantCulture(pDebtSecurityTransaction.price.accruedInterestRate.DecValue),
                                StrFunc.FmtAmountToGUI(StrFunc.FmtDecimalToInvariantCulture(pDebtSecurityTransaction.price.accruedInterestAmount.amount.DecValue)),
                                StrFunc.FmtAmountToGUI(StrFunc.FmtDecimalToInvariantCulture(interestAmount.amount.DecValue))});
                }
            }
            //
            if (IsToCheck("VRDSESETTLEMENTDT") && pIsToCheck_SettlementDate)
            {
                // Date de règlement différente de la date théorique
                //
                // 20090626 RD En cas ou orderRules n'est pas renseigné sur le Titre.
                ISecurityAsset securityAsset = debtSecurityTransactionContainer.GetSecurityAssetInDataDocument();
                DateTime dtGAMPaymentDate = new SecurityAssetContainer(securityAsset).CalcPaymentDate(CSTools.SetCacheOn(CS), m_Input.DataDocument.tradeDate);

                //DateTime dtGAMPaymentDate = debtSecurityTransactionContainer.CalcGrossAmountPaymentDate(m_Input.CS);
                if (DtFunc.IsDateTimeFilled(dtGAMPaymentDate) && (pDebtSecurityTransaction.grossAmount.paymentDateSpecified))
                {
                    IAdjustableDate adjustableDate = pDebtSecurityTransaction.grossAmount.paymentDate;
                    if ((0 != adjustableDate.unadjustedDate.DateValue.CompareTo(dtGAMPaymentDate)))
                        SetValidationRuleError("Msg_ValidationRule_VRDSESETTLEMENTDT", new string[] { 
                                DtFunc.DateTimeToString(adjustableDate.unadjustedDate.DateValue, DtFunc.FmtShortDate),
                                DtFunc.DateTimeToString(dtGAMPaymentDate, DtFunc.FmtShortDate)});
                }
            }
            if (IsToCheck("VRDSEGAM"))
            {
                decimal grossAmount = debtSecurityTransactionContainer.CalcGrossAmount(m_Input.CS).amount.DecValue;
                if (false == CompareAmount(isDseGAMExceptDec, grossAmount, pDebtSecurityTransaction.grossAmount.paymentAmount.amount.DecValue))
                    SetValidationRuleError("Msg_ValidationRule_VRDSEGAM", new string[] { 
                            StrFunc.FmtAmountToGUI(StrFunc.FmtDecimalToInvariantCulture(pDebtSecurityTransaction.grossAmount.paymentAmount.amount.DecValue)), 
                            StrFunc.FmtAmountToGUI(StrFunc.FmtDecimalToInvariantCulture(grossAmount)) });
            }
            //

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pExchangeTradedDerivative"></param>
        private void CheckVR_ExchangeTradedDerivative(IExchangeTradedDerivative pExchangeTradedDerivative)
        {

            if (IsCheckError) // systematique en mode Erreur
            {
                DateTime dtTransac = m_Input.DataDocument.currentTrade.tradeHeader.tradeDate.DateValue;
                string strDtTransac = DtFunc.DateTimeToString(dtTransac, DtFunc.FmtShortDate);
                //
                DateTime dtClearingBusiness = DateTime.MinValue;
                if (pExchangeTradedDerivative.tradeCaptureReport.ClearingBusinessDateSpecified)
                    dtClearingBusiness = pExchangeTradedDerivative.tradeCaptureReport.ClearingBusinessDate.DateValue;
                string strDtClearingBusiness = DtFunc.DateTimeToString(dtClearingBusiness, DtFunc.FmtShortDate);
                //
                DateTime dtSysBusiness = Tools.GetDateBusiness(CSTools.SetCacheOn(CS), m_Input.DataDocument);
                string strDtSysBusiness = DtFunc.DateTimeToString(dtSysBusiness, DtFunc.FmtShortDate);
                //
                ExchangeTradedDerivativeContainer exchangeTradedDerivativeContainer = new ExchangeTradedDerivativeContainer(
                    CSTools.SetCacheOn(CS), DbTransaction, pExchangeTradedDerivative);
                // Dans le cas où l'asset n'existe pas, exchangeTradedDerivativeContainer.AssetETD est valorisé tout de même
                bool isAssetExist = (exchangeTradedDerivativeContainer.SecurityId > 0 &&
                        (null != exchangeTradedDerivativeContainer.AssetETD));
                //
                if (isAssetExist)
                {
                    int idMarket = exchangeTradedDerivativeContainer.derivativeContract.IdMarket;

                    IBusinessCenters bcs = m_Input.DataDocument.currentProduct.productBase.LoadBusinessCenters(CSTools.SetCacheOn(CS), null, null, new string[] { idMarket.ToString() });
                    EFS_BusinessCenters efs_bc = new EFS_BusinessCenters(CS, bcs);
                    if (efs_bc.businessCentersSpecified)
                    {
                        // RD 20120105
                        // Utiliser DayTypeEnum.ExchangeBusiness car on est en présence d'un BC du marché
                        if (efs_bc.IsHoliday(dtClearingBusiness, DayTypeEnum.ExchangeBusiness))
                        {
                            string marketIdentifier = string.Empty;
                            //
                            SQL_Market sqlMarket = new SQL_Market(CSTools.SetCacheOn(CS), idMarket);
                            sqlMarket.LoadTable(new string[] { "IDENTIFIER" });
                            if (sqlMarket.IsLoaded)
                                marketIdentifier = sqlMarket.Identifier;
                            //
                            SetValidationRuleError("Msg_ValidationRule_ETDBizDtIsHoliday",
                                new string[] { strDtClearingBusiness,marketIdentifier, 
                                        exchangeTradedDerivativeContainer.derivativeContract.Identifier });
                        }
                    }
                }
                //
                #region Date de compensation doit être >= à la date Business (présente dans la table ENTITYMARKET)
                //FI 20121009 [18176] Spheres® impose que la date de compensation soit >= à la date Business en vigeur dans ENTITYMARKET uniquement 
                //si le trade est une allocation et si le trade que le book est avec tenue de position
                if (m_Input.IsAllocation && exchangeTradedDerivativeContainer.IsPosKeepingOnBookDealer(CSTools.SetCacheOn(CS), m_Input.DataDocument))
                {
                    if (DtFunc.IsDateTimeFilled(dtSysBusiness) && (dtClearingBusiness < dtSysBusiness))
                        SetValidationRuleError("Msg_ValidationRule_ETDBizDtLessSysBizDt", new string[] { strDtClearingBusiness, strDtSysBusiness });
                }
                #endregion
                //
                #region Date de transaction par rapport à la date de compensation
                bool isLateTrade = false;
                if (pExchangeTradedDerivative.tradeCaptureReport.TrdTypeSpecified)
                    isLateTrade = (pExchangeTradedDerivative.tradeCaptureReport.TrdType == TrdTypeEnum.LateTrade);
                //
                if (isLateTrade)
                {
                    // TrdType = LateTrade: Date de transaction doit être < à la date de compensation
                    if (dtTransac >= dtClearingBusiness)
                        SetValidationRuleError("Msg_ValidationRule_ETDLateTrdTransacDtNotLessBizDt", new string[] { strDtTransac, strDtClearingBusiness });
                }
                else
                {
                    // Date de transaction doit être <= à la date de compensation
                    if (dtTransac > dtClearingBusiness)
                        SetValidationRuleError("Msg_ValidationRule_ETDTransacDtUpperBizDt", new string[] { strDtTransac, strDtClearingBusiness });
                }
                #endregion
                //
                if (isAssetExist)
                {
                    #region Dernier Jour de négociation du ou des actifs doit être >= à la date de transaction
                    DateTime dtLastTrdDay = exchangeTradedDerivativeContainer.AssetETD.Maturity_LastTradingDay;
                    if (DtFunc.IsDateTimeFilled(dtLastTrdDay) && (dtTransac > dtLastTrdDay))
                        SetValidationRuleError("Msg_ValidationRule_ETDTransacDtUpperLastTrdDay",
                            new string[] { strDtTransac, 
                                    DtFunc.DateTimeToString(dtLastTrdDay, DtFunc.FmtShortDate), 
                                    exchangeTradedDerivativeContainer.AssetETD.Identifier });
                    #endregion
                    //
                    #region Date d’expiration du ou des actifs doit être >= à la date de compensation
                    DateTime dtMaturity = exchangeTradedDerivativeContainer.AssetETD.Maturity_MaturityDate;
                    if (DtFunc.IsDateTimeFilled(dtMaturity) && (dtClearingBusiness > dtMaturity))
                        SetValidationRuleError("Msg_ValidationRule_ETDBizDtUpperMaturityDt",
                            new string[] { strDtClearingBusiness, 
                                    DtFunc.DateTimeToString(dtMaturity, DtFunc.FmtShortDate), 
                                    exchangeTradedDerivativeContainer.AssetETD.Identifier });
                    #endregion
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pObjectPayerReceiverRef"></param>
        /// <param name="pIsPayerReceiver"></param>
        /// <returns></returns>
        private string[] GetParties(object pObjectPayerReceiverRef, bool pIsPayerReceiver)
        {

            string[] ret = null;
            FieldInfo fld = null;
            IReference partyReference = null;
            string prefix = string.Empty;
            //
            ArrayList al = new ArrayList();
            if (null != pObjectPayerReceiverRef)
            {
                prefix = pIsPayerReceiver ? "receiver" : "seller";
                fld = pObjectPayerReceiverRef.GetType().GetField(prefix + "PartyReference");
                partyReference = (IReference)fld.GetValue(pObjectPayerReceiverRef);
                //				
                if ((null != partyReference && StrFunc.IsFilled(partyReference.hRef)))
                {
                    IParty party = m_Input.DataDocument.GetParty(partyReference.hRef);
                    if (null != party)
                        al.Add((party.otcmlId));
                }
                //
                prefix = pIsPayerReceiver ? "payer" : "buyer";
                //
                fld = pObjectPayerReceiverRef.GetType().GetField(prefix + "PartyReference");
                partyReference = (IReference)fld.GetValue(pObjectPayerReceiverRef);
                //				
                if ((null != partyReference && StrFunc.IsFilled(partyReference.hRef)))
                {
                    IParty party = m_Input.DataDocument.GetParty(partyReference.hRef);
                    if (null != party)
                        al.Add((party.otcmlId));
                }
            }
            //
            if (ArrFunc.IsFilled(al))
                ret = (string[])al.ToArray(typeof(string));
            return ret;
        }

        //#region CheckValidationRule_CorrectionOfQuantity
        ///// <summary>
        ///// Contrôle la correction de quantité
        ///// </summary>
        //private void CheckValidationRule_CorrectionOfQuantity()
        //{
        //    try
        //    {
        //        if (IsCheckError)
        //        {
        //            //bool isOk = false;
        //            //if (ArrFunc.IsFilled(m_Input.DataDocument.currentTrade.tradeHeader.partyTradeIdentifier))
        //            //{
        //            //    for (int i = 0; i < m_Input.DataDocument.currentTrade.tradeHeader.partyTradeIdentifier.Length; i++)
        //            //    {
        //            //        IPartyTradeIdentifier partyTradeIdentifier = (IPartyTradeIdentifier)m_Input.DataDocument.currentTrade.tradeHeader.partyTradeIdentifier[i];
        //            //        if (partyTradeIdentifier.bookIdSpecified)
        //            //        {
        //            //            SQL_Book sqlbook = new SQL_Book(CS, partyTradeIdentifier.bookId.OTCmlId, SQL_Table.ScanDataDtEnabledEnum.Yes);
        //            //            isOk = (sqlbook.IsLoaded && sqlbook.IdA_Entity > 0);
        //            //        }
        //            //        if (isOk)
        //            //            break;
        //            //    }
        //            //}
        //            //if (false == isOk)
        //            //SetValidationRuleError("Msg_ValidationRule_Book_IsAvailable");
        //        }
        //    }
        //    catch (Exception ex) { throw (new SpheresException("CheckTradeValidationRule.CheckValidationRule_CorrectionOfQuantity", ex)); }
        //}
        //#endregion CheckValidationRule_CorrectionOfQuantity

        #region CheckValidationRule_ExeAssAbn
        /// <summary>
        /// 
        /// </summary>
        private void CheckValidationRule_ExeAssAbn()
        {

            if (IsCheckError)
            {

            }

        }
        #endregion CheckValidationRule_ExeAssAbn



        #endregion Methods
    }

    /// <summary>
    /// Validation de la saisie des titres
    /// </summary>
    public class CheckDebtSecValidationRule : CheckTradeInputValidationRuleBase
    {
        /// <summary>
        /// 
        /// </summary>
        private DebtSecInput _Input;


        #region Constructors
        public CheckDebtSecValidationRule(string pCs, IDbTransaction pDbTransaction, DebtSecInput pDebtSecInput, Cst.Capture.ModeEnum pCaptureModeEnum)
            : base(pCs, pDbTransaction, pDebtSecInput, pCaptureModeEnum)
        {
            _Input = pDebtSecInput;
        }

        public CheckDebtSecValidationRule(string pCs, DebtSecInput pDebtSecInput, Cst.Capture.ModeEnum pCaptureModeEnum)
            : base(pCs, pDebtSecInput, pCaptureModeEnum)
        {
            _Input = pDebtSecInput;
        }
        #endregion constructor

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCheckMode"></param>
        /// <returns></returns>
        public override bool ValidationRules(CheckModeEnum pCheckMode)
        {
            m_CheckMode = pCheckMode;
            m_CheckConformity = new Hashtable();
            //
            CheckValidationRule_CodeIsin();
            CheckValidationRule_CodeIsinUnique();
            //
            return ArrFunc.IsEmpty(m_CheckConformity);
        }


        /// <summary>
        /// 
        /// </summary>
        private void CheckValidationRule_CodeIsin()
        {

            bool isOk = true;
            string codeIsin = GetCodeIsin();
            //
            bool isToCheck = IsToCheck("VRDSEISIN");
            if (isToCheck && StrFunc.IsFilled((String)sqlInstrument.GetFirstRowColumnValue("VRDSEISINLST")))
            {
                string[] codeIsinIgnore = StrFunc.QueryStringData.StringListToStringArray((String)sqlInstrument.GetFirstRowColumnValue("VRDSEISINLST"));
                isToCheck = (false == ArrFunc.ExistInArray(codeIsinIgnore, codeIsin));
            }
            //
            if (isToCheck)
            {
                if (StrFunc.IsEmpty(codeIsin))
                {
                    SetValidationRuleError("Msg_ValidationRule_VRDSEISIN");
                }
                else
                {
                    // Verification de la taile du CODEISIN
                    if (BoolFunc.IsTrue(sqlInstrument.GetFirstRowColumnValue("VRDSEISINLEN")))
                    {
                        isOk = StrFunc.IsFilled(codeIsin) && (codeIsin.Length == 12);
                        if (false == isOk)
                            SetValidationRuleError("Msg_ValidationRule_VRDSEISINLEN", new string[] { codeIsin });
                    }
                    // Verification du pays du CODEISIN
                    if (BoolFunc.IsTrue(sqlInstrument.GetFirstRowColumnValue("VRDSEISINCOUNTRY")))
                    {
                        isToCheck = (false == (StrFunc.IsFilled(codeIsin) && codeIsin.StartsWith("X")));
                        if (isToCheck)
                        {
                            string country = string.Empty;
                            isOk = (2 <= codeIsin.Length);
                            if (isOk)
                            {
                                country = codeIsin.Substring(0, 2);
                                SQL_Country sqlCountry = new SQL_Country(CSTools.SetCacheOn(CS), SQL_Country.IDType.Iso3166Alpha2, country, SQL_Table.ScanDataDtEnabledEnum.Yes);
                                isOk = sqlCountry.IsFound;
                            }
                            //
                            if (false == isOk)
                                SetValidationRuleError("Msg_ValidationRule_VRDSEISINCOUNTRY", new string[] { country, codeIsin });
                        }
                    }
                    // Verification avec la clef 
                    if (BoolFunc.IsTrue(sqlInstrument.GetFirstRowColumnValue("VRDSEISINKEY")))
                    {
                        isOk = true;
                        if (false == isOk)
                            SetValidationRuleError("Msg_ValidationRule_VRDSEISINKEY", new string[] { codeIsin });
                    }
                }
            }

        }

        /// <summary>
        /// 
        /// </summary>
        /// FI 20120614 [17904] Utilisation de la table TRADE_ASSET
        private void CheckValidationRule_CodeIsinUnique()
        {
            string codeIsin = GetCodeIsin();
            bool isOk = true;
            //
            bool isToCheck = IsToCheck("VRDSEISINUNIQUE");
            if (isToCheck && StrFunc.IsFilled((String)sqlInstrument.GetFirstRowColumnValue("VRDSEISINUNIQUELST")))
            {
                string[] codeIsinIgnore = StrFunc.QueryStringData.StringListToStringArray((String)sqlInstrument.GetFirstRowColumnValue("VRDSEISINUNIQUELST"));
                isToCheck = (false == ArrFunc.ExistInArray(codeIsinIgnore, codeIsin));
                //droits non "isinés" à regrouper sous FR0000880007
                //fonds communs de placement résidents non "isinés" à regrouper sous FR0007999990
                if (isToCheck)
                    isToCheck = ((codeIsin != "FR0000880007") && (codeIsin != "FR00007999990"));
            }
            //
            bool isNew = Cst.Capture.IsModeNewCapture(captureMode);
            bool isUpd = Cst.Capture.IsModeUpdateOrUpdatePostEvts(captureMode);
            //
            if (isToCheck)
                isToCheck = (isNew || isUpd);
            //                
            if (isToCheck)
            {
                //StrBuilder sql = new StrBuilder("");
                //DataParameters parameters = null;
                //sql += SQLCst.SELECT + "t.IDENTIFIER" + Cst.CrLf;
                //sql += SQLCst.FROM_DBO + Cst.OTCml_TBL.TRADE + " t" + Cst.CrLf;
                //sql += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.TRADESTSYS + " ts on ts.IDT=t.IDT" + Cst.CrLf;
                //sql += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.INSTRUMENT + " i on i.IDI=t.IDI" + Cst.CrLf;
                //sql += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.PRODUCT + " p on p.IDP=i.IDP" + Cst.CrLf;
                //sql += SQLCst.WHERE + "p.FAMILY=@FAMILY" + SQLCst.AND + "p.GPRODUCT=@GPRODUCT" + Cst.CrLf;
                //sql += SQLCst.AND + "ts.IDSTENVIRONMENT!=@DEACTIV" + Cst.CrLf;
                //if (isUpd)
                //    sql += SQLCst.AND + @"t.IDT!=@IDT" + Cst.CrLf;
                //sql += SQLCst.AND + DataHelper.GetSQLXQuery_ExistsNode(CS, "TRADEXML", "t", @"efs:EfsML/trade/efs:debtSecurity/efs:security/instrumentId[contains(@instrumentIdScheme,""ISIN"")][text()=sql:variable(""@ISIN"")]", OTCmlHelper.GetXMLNamespace_3_0(CS));
                ////
                //parameters = new DataParameters();
                //parameters.Add(new DataParameter(CS, "DEACTIV", DbType.AnsiString, 255), Cst.StatusActivation.DEACTIV.ToString());
                //parameters.Add(new DataParameter(CS, "FAMILY", DbType.AnsiString, 255), "DSE");
                //parameters.Add(new DataParameter(CS, "GPRODUCT", DbType.AnsiString, 255), "ASSET");
                //parameters.Add(new DataParameter(CS, "ISIN", DbType.AnsiString, 255), codeIsin);
                //if (isUpd)
                //    parameters.Add(new DataParameter(CS, "@IDT", DbType.Int32), _Input.IdT);
                //
                //FI 20120614 [17904] 
                StrBuilder sql = new StrBuilder(SQLCst.SELECT);
                sql += "t.IDENTIFIER" + Cst.CrLf;
                sql += SQLCst.FROM_DBO + Cst.OTCml_TBL.TRADEASSET + " tasset";
                sql += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.TRADE + " t on t.IDT=tasset.IDT" + Cst.CrLf;
                sql += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.TRADESTSYS + " ts on ts.IDT=t.IDT" + Cst.CrLf;
                sql += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.INSTRUMENT + " i on i.IDI=t.IDI" + Cst.CrLf;
                sql += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.PRODUCT + " p on p.IDP=i.IDP and p.FAMILY=@FAMILY" + Cst.CrLf;
                sql += SQLCst.WHERE + "tasset.ISIN=@ISIN" + Cst.CrLf;
                sql += SQLCst.AND + "ts.IDSTACTIVATION!=@DEACTIV" + Cst.CrLf;
                if (isUpd)
                   sql += SQLCst.AND + @"t.IDT!=@IDT" + Cst.CrLf;

                DataParameters parameters = new DataParameters();
                parameters.Add(new DataParameter(CS, "ISIN", DbType.AnsiString, 255), codeIsin);
                parameters.Add(new DataParameter(CS, "FAMILY", DbType.AnsiString, 255), "DSE");
                parameters.Add(new DataParameter(CS, "DEACTIV", DbType.AnsiString, 255), Cst.StatusActivation.DEACTIV.ToString());
                if (isUpd)
                    parameters.Add(new DataParameter(CS, "@IDT", DbType.Int32), _Input.IdT);
                
                QueryParameters qry = new QueryParameters(_Input.CS, sql.ToString(), parameters);
                //
                object obj = DataHelper.ExecuteScalar(CSTools.SetCacheOn(qry.cs,1,null), CommandType.Text, qry.query, qry.parameters.GetArrayDbParameter());
                isOk = (null == obj);
                if (false == isOk)
                {
                    string identifier = Convert.ToString(obj);
                    SetValidationRuleError("Msg_ValidationRule_VRDSEISINUNIQUE", new string[] { codeIsin, identifier });
                }
            }

        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private string GetCodeIsin()
        {
            IDebtSecurity debtSecurity = (IDebtSecurity)_Input.DataDocument.currentProduct.product;
            DebtSecurityContainer debtSec = new DebtSecurityContainer(debtSecurity);
            return debtSec.GetCodeIsin(CSTools.SetCacheOn(CS));
        }
    }

    /// <summary>
    /// Validation de la saisie des trades tels que GPRODUCT = 'RISK'
    /// </summary>
    ///<remarks>
    /// Pour l'instant aucune validation rule n'est contrôlée
    ///</remarks> 
    public class CheckTradeRiskValidationRule : CheckTradeInputValidationRuleBase
    {
        #region Members
        private TradeRiskInput m_Input;
        #endregion Members

        #region Constructors
        public CheckTradeRiskValidationRule(string pCs, IDbTransaction pDbTransaction, TradeRiskInput pRiskInput, Cst.Capture.ModeEnum pCaptureModeEnum)
            : base(pCs, pDbTransaction, pRiskInput, pCaptureModeEnum)
        {
            m_Input = pRiskInput;
        }

        public CheckTradeRiskValidationRule(string pCs, TradeRiskInput pRiskInput, Cst.Capture.ModeEnum pCaptureModeEnum)
            : base(pCs, pRiskInput, pCaptureModeEnum)
        {
            m_Input = pRiskInput;
        }
        #endregion constructor

        /// <summary>
        /// Retourne true si toutes les validations rules sont respectées
        /// </summary>
        /// <param name="pCheckMode"></param>
        /// <returns></returns>
        public override bool ValidationRules(CheckModeEnum pCheckMode)
        {
            m_CheckMode = pCheckMode;
            m_CheckConformity = new Hashtable();

            CheckValidationRule_Book();

            return ArrFunc.IsEmpty(m_CheckConformity);
        }
    }

    /// <summary>
    /// Validation d'une saisie de correctionOfQuantity
    /// </summary>
    public class CheckCorrectionOfQuantityValidationRule : CheckValidationRuleBase
    {
        #region Members
        private TradePositionCancelation m_correctionOfQty;
        #endregion Members

        #region Constructors
        public CheckCorrectionOfQuantityValidationRule(string pCs, IDbTransaction pDbTransaction, TradePositionCancelation pCorrectionOfQty)
            : base(pCs, pDbTransaction)
        {
            m_correctionOfQty = pCorrectionOfQty;
        }
        #endregion constructor

        #region  ValidationRules
        public override bool ValidationRules(CheckModeEnum pCheckMode)
        {
            m_CheckMode = pCheckMode;
            m_CheckConformity = new Hashtable();
            //
            CheckValidationRule_CorrectionOfQuantity();
            //
            return ArrFunc.IsEmpty(m_CheckConformity);
        }
        #endregion

        #region CheckValidationRule_CorrectionOfQuantity
        /// <summary>
        /// Contrôle la correction de quantité
        /// </summary>
        private void CheckValidationRule_CorrectionOfQuantity()
        {
            try
            {
                if (IsCheckError)
                {



                    //bool isOk = false;
                    //if (ArrFunc.IsFilled(m_Input.DataDocument.currentTrade.tradeHeader.partyTradeIdentifier))
                    //{
                    //    for (int i = 0; i < m_Input.DataDocument.currentTrade.tradeHeader.partyTradeIdentifier.Length; i++)
                    //    {
                    //        IPartyTradeIdentifier partyTradeIdentifier = (IPartyTradeIdentifier)m_Input.DataDocument.currentTrade.tradeHeader.partyTradeIdentifier[i];
                    //        if (partyTradeIdentifier.bookIdSpecified)
                    //        {
                    //            SQL_Book sqlbook = new SQL_Book(CS, partyTradeIdentifier.bookId.OTCmlId, SQL_Table.ScanDataDtEnabledEnum.Yes);
                    //            isOk = (sqlbook.IsLoaded && sqlbook.IdA_Entity > 0);
                    //        }
                    //        if (isOk)
                    //            break;
                    //    }
                    //}
                    //if (false == isOk)
                    //SetValidationRuleError("Msg_ValidationRule_Book_IsAvailable");
                }
            }
            catch (Exception ex) { throw (new SpheresException(MethodInfo.GetCurrentMethod().Name, ex)); }
        }
        #endregion CheckValidationRule_CorrectionOfQuantity
    }

}
