#region Using Directives
using EFS.ACommon;
using EFS.Actor;
using EFS.ApplicationBlocks.Data;
using EFS.Book;
using EFS.Common;
using EfsML.Business;
using EfsML.Interface;
using FixML.Enum;
using FixML.Interface;
using FpML.Enum;
using FpML.Interface;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using Tz = EFS.TimeZone;
#endregion Using Directives

namespace EFS.TradeInformation
{

    /// <summary>
    /// Validation de la saisie des trades
    /// </summary>
    public class CheckTradeValidationRule : CheckTradeInputValidationRuleBase
    {
        #region Members
        private readonly TradeInput m_Input;
        #endregion Members

        #region Accessors

        #endregion Accessors

        #region Constructors

        // EG 20171115 Upd Add CaptureSessionInfo
        public CheckTradeValidationRule(TradeInput pTradeInput, Cst.Capture.ModeEnum pCaptureModeEnum, User user)
            : base(pTradeInput, pCaptureModeEnum, user)
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
        // EG 20171115 Upd CheckValidationRule_DtExecutionDtBusiness
        // EG 20180307 [23769] Gestion dbTransaction
        public override bool ValidationRules(string pCS, IDbTransaction pDbTransaction, CheckModeEnum pCheckMode)
        {
            m_CheckMode = pCheckMode;
            m_CheckConformity = new Hashtable();

            if (Cst.Capture.IsModeNewCapture(CaptureMode) ||
                Cst.Capture.IsModeUpdate(CaptureMode))
            {
                CheckValidationRule_Party();

                if (false == m_Input.DataDocument.CurrentProduct.IsStrategy)
                    CheckActorBuyerSeller();
                CheckValidationRule_Book(pCS, pDbTransaction);
                CheckValidationRule_TraderAndSales();
                CheckValidationRule_Hedging();
                CheckValidationRule_DtTransac(pCS, pDbTransaction);
                CheckValidationRule_DtExecutionDtBusiness(pCS, pDbTransaction, User);
                CheckValidationRule_Product(pCS, pDbTransaction);
                CheckValidationRule_OPP(pCS, pDbTransaction);
                CheckValidationRule_Element();

            }
            else if (Cst.Capture.IsModeAction(CaptureMode))
            {
                CheckValidationRule_Action(pCS, pDbTransaction);
            }

            return ArrFunc.IsEmpty(m_CheckConformity);
        }

        /// <summary>
        /// Validation Rule: Date de transaction
        /// <para>- Nbre de jour(s) avant/après la date système</para>
        /// </summary>
        /// FI 20161214 [21916] Modify
        // EG 20180307 [23769] Gestion dbTransaction
        private void CheckValidationRule_DtTransac(string pCS, IDbTransaction pDbTransaction)
        {
            if (IsToCheck("VRTRANSBEFSYSMAX") || IsToCheck("VRTRANSAFTSYSMAX"))
            {
                DateTime dtTransac = m_Input.CurrentTrade.TradeHeader.TradeDate.DateValue;
                // FI 20161214 [21916] Utilisation de la DateTime.Date
                //DateTime dtSysBusiness = new DtFunc().StringDateISOToDateTime(DtFunc.DateTimeToStringDateISO(OTCmlHelper.GetDateBusiness(CSTools.SetCacheOff(pCS))));
                DateTime dtSysBusiness = OTCmlHelper.GetDateBusiness(CSTools.SetCacheOff(pCS), pDbTransaction).Date;

                //VRTRANSBEFSYSMAX
                if (IsToCheck("VRTRANSBEFSYSMAX"))
                {
                    int offset = Convert.ToInt32(SqlInstrument.GetFirstRowColumnValue("TRANSBEFSYSMAX"));
                    DateTime dtSysOffset = dtSysBusiness.AddDays(-offset);

                    if (0 > dtTransac.CompareTo(dtSysOffset))
                        SetValidationRuleError("Msg_ValidationRule_DtTransacBeforeDtSysMax",
                            new string[] { DtFunc.DateTimeToString(dtTransac, DtFunc.FmtShortDate), offset.ToString() });
                }

                //VRTRANSAFTSYSMAX
                if (IsToCheck("VRTRANSAFTSYSMAX"))
                {
                    int offset = Convert.ToInt32(SqlInstrument.GetFirstRowColumnValue("TRANSAFTSYSMAX"));
                    DateTime dtSysOffset = dtSysBusiness.AddDays(offset);

                    if (0 < dtTransac.CompareTo(dtSysOffset))
                        SetValidationRuleError("Msg_ValidationRule_DtTransacAfterDtSysMax",
                            new string[] { DtFunc.DateTimeToString(dtTransac, DtFunc.FmtShortDate), offset.ToString() });
                }
            }
        }


        /// <summary>
        /// Validation Rule: Date d'exécution et Date Business
        /// </summary>
        // EG 20171115 New
        // EG 20180307 [23769] Gestion dbTransaction
        private void CheckValidationRule_DtExecutionDtBusiness(string pCS, IDbTransaction pDbTransaction, User user)
        {
            Nullable<DateTimeOffset> dtExecution = m_Input.DataDocument.GetExecutionDateTimeOffset();
            Nullable<DateTime> dtCleared = null;

            if (m_Input.DataDocument.TradeHeader.ClearedDateSpecified)
                dtCleared = m_Input.DataDocument.TradeHeader.ClearedDate.DateValue;

            bool isApplyCutOff = dtExecution.HasValue && dtCleared.HasValue;

            // Contrôle du CutOff
            if (isApplyCutOff)
            {
                string timezone = m_Input.DataDocument.GetTradeTimeZone(pCS, pDbTransaction, user.Entity_IdA);

                Nullable<DateTimeOffset> dtExecutionInTimeZone = Tz.Tools.FromTimeZone(dtExecution, timezone);
                if (dtExecutionInTimeZone.HasValue && (dtCleared.Value.Date <= dtExecutionInTimeZone.Value.Date))
                {
                    dtCleared = m_Input.DataDocument.ApplyCutOff(pCS, pDbTransaction, dtExecutionInTimeZone, m_Input.SQLProduct.Family);
                    if (DtFunc.IsDateTimeFilled(dtCleared.Value))
                        m_Input.DataDocument.TradeHeader.ClearedDate.Value = DtFunc.DateTimeToStringDateISO(dtCleared.Value);
                }

            }
        }


        /// <summary>
        /// Validation Rule: OPPs
        /// <para>- Contrôle d'existance, sur chaque payment, d'un book géré sur le payer ou à minima sur le receiver</para>
        /// </summary>
        // EG 20180307 [23769] Gestion dbTransaction
        private void CheckValidationRule_OPP(string pCS, IDbTransaction pDbTransaction)
        {
            if (m_Input.DataDocument.OtherPartyPaymentSpecified)
            {
                //FI 20100423 [16967] add contrôle de l'existence sur OPP
                #region Contrôle d'existence d'au moins un book géré par paiement
                for (int i = 0; i < ArrFunc.Count(m_Input.DataDocument.OtherPartyPayment); i++)
                {
                    IPayment payment = m_Input.DataDocument.OtherPartyPayment[i];
                    Boolean isOk = false;
                    //Check PAYER
                    IPartyTradeIdentifier partyTradeIdentifier = m_Input.DataDocument.GetPartyTradeIdentifier(payment.PayerPartyReference.HRef);
                    if ((null != partyTradeIdentifier) && partyTradeIdentifier.BookIdSpecified)
                    {
                        IBookId bookId = partyTradeIdentifier.BookId;
                        isOk = BookTools.IsBookManaged(pCS, pDbTransaction, bookId.OTCmlId);
                    }

                    if (!isOk)
                    {
                        //Check RECEIVER
                        partyTradeIdentifier = m_Input.DataDocument.GetPartyTradeIdentifier(payment.ReceiverPartyReference.HRef);
                        if ((null != partyTradeIdentifier) && partyTradeIdentifier.BookIdSpecified)
                        {
                            IBookId bookId = partyTradeIdentifier.BookId;
                            isOk = BookTools.IsBookManaged(pCS, pDbTransaction, bookId.OTCmlId);
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
        /// FI 20170928 [23452] Modify
        private void CheckValidationRule_TraderAndSales()
        {
            ITradeHeader tradeHeader = m_Input.CurrentTrade.TradeHeader;
            if (ArrFunc.IsFilled(tradeHeader.PartyTradeInformation))
            {
                for (int i = 0; i < tradeHeader.PartyTradeInformation.Length; i++)
                {
                    IPartyTradeInformation partyTradeInformation = (IPartyTradeInformation)tradeHeader.PartyTradeInformation[i];
                    string partyReference = partyTradeInformation.PartyReference;

                    Nullable<CciTradeParty.PartyType> partyType = null;
                    if (m_Input.DataDocument.IsPartyBroker(partyReference))
                        partyType = CciTradeParty.PartyType.broker;
                    else if (m_Input.DataDocument.IsPartyCounterParty(partyReference))
                        partyType = CciTradeParty.PartyType.party;

                    if (partyType.HasValue)
                    {
                        if (partyTradeInformation.TraderSpecified)
                            CheckValidationRule_TraderAndSales(partyTradeInformation.Trader, partyType.Value.ToString(), partyReference);
                        if (partyTradeInformation.SalesSpecified)
                            CheckValidationRule_TraderAndSales(partyTradeInformation.Sales, partyType.Value.ToString(), partyReference);
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pTrader"></param>
        /// <param name="pPartyType"></param>
        /// <param name="pPartyReference"></param>
        /// FI 20170404 [23039] Modify
        private void CheckValidationRule_TraderAndSales(ITrader[] pTrader, string pPartyType, string pPartyReference)
        {

            string errMessage = "Msg_ValidationRule_" + pPartyType;
            //
            if (ArrFunc.IsFilled(pTrader))
            {
                decimal sumFactor = 0;
                bool isUniqueTraderOk = true;

                for (int j = 0; j < pTrader.Length; j++)
                {
                    if (StrFunc.IsFilled(pTrader[j].StrFactor))
                        sumFactor += pTrader[j].Factor;
                    // La saisie du même Trader plusieurs fois
                    for (int jj = j + 1; jj < pTrader.Length; jj++)
                    {
                        // 20091012 RD Pour éviter de comparer deux Trader Vides.
                        if (StrFunc.IsFilled(pTrader[j].Identifier) && (pTrader[j].Identifier == pTrader[jj].Identifier))
                            isUniqueTraderOk = false;
                    }
                }

                if (IsCheckError)
                {
                    // FI 20170404 [23039] Error si trader inconnu
                    IEnumerable<string> traderValue = from item in pTrader.Where(x => x.OTCmlId == 0)
                                                      select item.Identifier;
                    foreach (string item in traderValue)
                        SetValidationRuleError(errMessage + "_Trader_Unknown", null, new string[2] { item, pPartyReference });

                    if (1 < sumFactor)
                        SetValidationRuleError(errMessage + "_TraderFactorSumUp100", null, new string[] { pPartyReference });

                    if (false == isUniqueTraderOk)
                        SetValidationRuleError(errMessage + "_TraderNotUnique", null, new string[] { pPartyReference });
                }
                else if (IsCheckWarning && (0 < sumFactor && sumFactor < 1))
                    SetValidationRuleError(errMessage + "_TraderFactorSumLess100", null, new string[] { pPartyReference });
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private void CheckValidationRule_Hedging()
        {
            if (IsCheckError)
            {
                if (ArrFunc.IsFilled(m_Input.DataDocument.CurrentTrade.TradeHeader.PartyTradeIdentifier))
                {
                    for (int i = 0; i < m_Input.DataDocument.CurrentTrade.TradeHeader.PartyTradeIdentifier.Length; i++)
                    {
                        IPartyTradeIdentifier partyTradeIdentifier = (IPartyTradeIdentifier)m_Input.DataDocument.CurrentTrade.TradeHeader.PartyTradeIdentifier[i];
                        ILinkId linkId = partyTradeIdentifier.GetLinkIdFromScheme(Cst.OTCmL_hedgingFolderid);
                        if ((null != linkId) && (StrFunc.IsFilled(linkId.StrFactor)) && (linkId.Factor > 1))
                            SetValidationRuleError("Msg_ValidationRule_Hedging_FactorUp100", new string[] { partyTradeIdentifier.PartyReference.HRef });
                    }
                }
            }

        }

        /// <summary>
        /// Véfification que le position effect est conforme au paramétrage FungibilityMode de l'instruement
        /// </summary>
        /// FI 20140902 [XXXXX] add Method
        private void CheckValidationRule_PositionEffect(RptSideProductContainer pRptSide)
        {
            if ((IsCheckError) && (this.m_Input.IsAllocation) && (this.m_Input.SQLInstrument.FungibilityMode != EfsML.Enum.FungibilityModeEnum.NONE))
            {
                if (false == pRptSide.RptSide[0].PositionEffectSpecified)
                {
                    SetValidationRuleError("Msg_ValidationRule_PosEffectNotSpecified");
                }
                else
                {
                    switch (this.m_Input.SQLInstrument.FungibilityMode)
                    {
                        case EfsML.Enum.FungibilityModeEnum.CLOSE:
                            if (pRptSide.RptSide[0].PositionEffect != FixML.v50SP1.Enum.PositionEffectEnum.Close)
                                SetValidationRuleError("Msg_ValidationRule_PosEffectInvalid", new string[] { pRptSide.RptSide[0].PositionEffect.ToString() });
                            break;
                        case EfsML.Enum.FungibilityModeEnum.OPENCLOSE:
                            // OK => pas de contrôle
                            break;
                        default:
                            throw new NotImplementedException(StrFunc.AppendFormat("{0} is not implemented", this.m_Input.SQLInstrument.FungibilityMode.ToString()));
                    }
                }
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// FI 20140902 [XXXXX] Modify 
        /// FI 20150129 [20751] Modify
        /// FI 20161214 [21916] Modify
        /// FI 20170928 [23452] Modify
        // EG 20180307 [23769] Gestion dbTransaction
        private void CheckValidationRule_Product(string pCS, IDbTransaction pDbTransaction)
        {

            DataDocumentContainer dataDocument = m_Input.DataDocument;
            ProductContainer product = dataDocument.CurrentProduct;
            if (product.IsFxLeg)
            {
                IFxLeg fxleg = (IFxLeg)product.Product;
                CheckVR_FXLeg(pCS, fxleg, null);
            }
            else if (product.IsFxSwap)
            {
                #region IsFxSwap
                IFxSwap fxSwap = (IFxSwap)product.Product;
                for (int i = 0; i < ArrFunc.Count(fxSwap.FxSingleLeg); i++)
                {
                    if (i == 0)
                        CheckVR_FXLeg(pCS, fxSwap.FxSingleLeg[i], null);
                    else
                        CheckVR_FXLeg(pCS, fxSwap.FxSingleLeg[i], fxSwap.FxSingleLeg[i - 1]);
                }
                #endregion IsFxSwap
            }
            else if (product.IsFxTermDeposit)
            {
                #region IsFxTermDeposit
                _ = (ITermDeposit)product.Product;
                #endregion
            }
            else if (product.IsFxSimpleOption)
            {
                #region IsFxSimpleOption
                IFxOptionLeg fxOptLeg = (IFxOptionLeg)product.Product;
                CheckVR_FXOption(pCS, fxOptLeg);
                #endregion IsFxSimpleOption
            }
            else if (product.IsFxDigitalOption)
            {
                #region IsFxDigitalOption
                IFxDigitalOption fxDigOpt = (IFxDigitalOption)product.Product;
                CheckVR_FXOption(pCS, fxDigOpt);
                #endregion IsFxDigitalOption
            }
            else if (product.IsFxBarrierOption)
            {
                #region IsFxBarrierOption
                IFxBarrierOption fxBarOpt = (IFxBarrierOption)product.Product;
                CheckVR_FXOption(pCS, fxBarOpt);
                if (IsCheckError) // systematique en mode Erreur
                {
                    if (ArrFunc.IsEmpty(fxBarOpt.FxBarrier))
                        SetValidationRuleError("Msg_ValidationRule_FxBarrierOptionNoBarrier");
                }
                #endregion IsFxBarrierOption
            }
            else if (product.IsFxAverageRateOption)
            {
                #region IsFxAverageRateOption
                IFxAverageRateOption fxaveRateOpt = (IFxAverageRateOption)product.Product;
                CheckVR_FXOption(pCS, fxaveRateOpt);
                #endregion IsFxAverageRateOption
            }
            else if (product.IsFra)
            {
                #region IsFra
                _ = (IFra)product.Product;
                #endregion
            }
            else if (product.IsSwap)
            {
                #region IsSwap
                ISwap swap = (ISwap)product.Product;
                //
                if (IsCheckError) // systematique en mode Erreur
                {
                    if (ArrFunc.Count(swap.Stream) == 2)
                    {
                        if ((0 == swap.Stream[0].PayerPartyReference.HRef.CompareTo(swap.Stream[1].PayerPartyReference.HRef)) ||
                            (0 == swap.Stream[0].ReceiverPartyReference.HRef.CompareTo(swap.Stream[1].ReceiverPartyReference.HRef)))
                        {
                            SetValidationRuleError("Msg_ValidationRule_IdenticPayerReceiver2");
                        }
                    }
                }
                // 20090615 RD Pour partager le même code avec les SaleAndRepurchaseAgreement
                CheckVR_IRD(pCS, swap.Stream);
                #endregion IsSwap
            }
            else if (product.IsLoanDeposit)
            {
                #region IsLoanDeposit
                ILoanDeposit loanDeposit = (ILoanDeposit)product.Product;
                //
                if (IsCheckError) // systematique en mode Erreur
                {
                    if (ArrFunc.Count(loanDeposit.Stream) > 1)
                    {
                        string hrefPayer = loanDeposit.Stream[0].PayerPartyReference.HRef;
                        string hrefReceiver = loanDeposit.Stream[0].ReceiverPartyReference.HRef;
                        for (int i = 1; i < ArrFunc.Count(loanDeposit.Stream); i++)
                        {
                            if ((0 == hrefPayer.CompareTo(loanDeposit.Stream[i].PayerPartyReference.HRef))
                                ||
                                (0 == hrefReceiver.CompareTo(loanDeposit.Stream[i].ReceiverPartyReference.HRef)))
                            {
                                SetValidationRuleError("Msg_ValidationRule_DifferentPayerReceiver");
                            }
                        }
                    }
                }
                //
                //20090629 PL Refactoring: Call CheckVR_IRD()
                CheckVR_IRD(pCS, loanDeposit.Stream);
                #endregion IsLoanDeposit
            }
            else if (product.IsSwaption)
            {
                #region IsSwaption
                ISwaption swaption = (ISwaption)product.Product;
                ISwap swap = swaption.Swap;
                //
                if (IsCheckError) // systematique en mode Erreur
                {
                    if (ArrFunc.Count(swap.Stream) == 2)
                    {
                        if ((0 == swap.Stream[0].PayerPartyReference.HRef.CompareTo(swap.Stream[1].PayerPartyReference.HRef)) ||
                            (0 == swap.Stream[0].ReceiverPartyReference.HRef.CompareTo(swap.Stream[1].ReceiverPartyReference.HRef)))
                        {
                            SetValidationRuleError("Msg_ValidationRule_IdenticPayerReceiver2");
                        }
                    }
                }
                // 
                CheckVR_IRD(pCS, swap.Stream);
                #endregion IsSwaption
            }
            else if (product.IsCapFloor)
            {
                ICapFloor capFloor = (ICapFloor)product.Product;
                //20090629 PL Refactoring: Call CheckVR_IRD()
                CheckVR_IRD(pCS, capFloor.Stream);

            }
            else if (product.IsBulletPayment)
            {
                _ = (IBulletPayment)product.Product;
            }
            else if (product.IsReturnSwap)
            {
                #region IsReturnSwap
                // EG 20140526 New
                IReturnSwap returnSwap = (IReturnSwap)product.Product;
                CheckVR_ReturnSwap(pCS, pDbTransaction, returnSwap);
                #endregion IsReturnSwap
            }
            else if (product.IsEquityOption)
            {
                #region IsEquityOption
                _ = (IEquityOption)product.Product;
                #endregion IsEquityOption
            }
            else if (product.IsDebtSecurityTransaction)
            {
                #region IsDebtSecurityTransaction
                IDebtSecurityTransaction debtSecurityTransaction = (IDebtSecurityTransaction)product.Product;
                CheckVR_DebtSecurityTransaction(pCS, pDbTransaction, debtSecurityTransaction, true);
                #endregion
            }
            else if (product.IsRepo || product.IsBuyAndSellBack)
            {
                #region IsSalesAndRepurchaseAgreement
                ISaleAndRepurchaseAgreement saleAndRepurchaseAgreement = (ISaleAndRepurchaseAgreement)product.Product;
                //
                for (int i = 0; i < ArrFunc.Count(saleAndRepurchaseAgreement.SpotLeg); i++)
                    CheckVR_DebtSecurityTransaction(pCS, pDbTransaction, saleAndRepurchaseAgreement.SpotLeg[i].DebtSecurityTransaction, !product.IsRepo);
                //
                if (saleAndRepurchaseAgreement.ForwardLegSpecified)
                {
                    for (int i = 0; i < ArrFunc.Count(saleAndRepurchaseAgreement.ForwardLeg); i++)
                        CheckVR_DebtSecurityTransaction(pCS, pDbTransaction, saleAndRepurchaseAgreement.ForwardLeg[i].DebtSecurityTransaction, false);
                }
                //
                CheckVR_IRD(pCS, saleAndRepurchaseAgreement.CashStream);
                CheckVR_SalesAndRepurchaseAgreement(saleAndRepurchaseAgreement);
                #endregion
            }
            else if (product.IsExchangeTradedDerivative)
            {
                IExchangeTradedDerivative exchangeTradedDerivative = (IExchangeTradedDerivative)product.Product;
                CheckVR_ExchangeTradedDerivative(pCS, pDbTransaction, exchangeTradedDerivative);
            }
            else if (product.IsEquitySecurityTransaction) //FI 20140902 [XXXXX] Gestion des EquitySecurityTransaction à l'image des exchangeTradedDerivatives
            {
                IEquitySecurityTransaction equitySecuritytransaction = (IEquitySecurityTransaction)product.Product;
                CheckVR_EquitySecurityTransaction(pCS, pDbTransaction, equitySecuritytransaction);
            }
            else if (product.IsCommoditySpot) // FI 20161214 [21916] add if 
            {
                ICommoditySpot commoditySpot = (ICommoditySpot)product.Product;
                CheckVR_CommoditySpot(pCS, pDbTransaction, commoditySpot);
            }
            else if (product.IsStrategy)
            {
                // EG 20160404 Migration vs2013
                // #warning les strategies ne sont pas traitées, présence d'un seul cas particulier sur les strategies sur ETD afin de ne pas reproduire le problème rencontré sur le ticket [17209]
                //FI 20111009 [17209]
                StrategyContainer strategy = (StrategyContainer)dataDocument.CurrentProduct;
                for (int i = 0; i < ArrFunc.Count(strategy.SubProduct); i++)
                {
                    if (Tools.IsTypeOrInterfaceOf(strategy.SubProduct[i], EfsML.Enum.InterfaceEnum.IExchangeTradedDerivative))
                    {
                        IExchangeTradedDerivative exchangeTradedDerivative = (IExchangeTradedDerivative)strategy.SubProduct[i];
                        CheckVR_ExchangeTradedDerivative(pCS, pDbTransaction, exchangeTradedDerivative);
                    }
                }
            }
            // FI 20170928 [23452] Call ChekVR_MiFIR
            ChekVR_MiFIR(pCS);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pIrs"></param>
        private void CheckVR_IRD(string pCS, IInterestRateStream pIrs)
        {
            //FI 20101110 Bizaremment on ne controlait pas la dates => Ajout appel à CheckVR_IRDDate
            CheckVR_IRDDate(pCS, pIrs, null);
            CheckVR_IRDStream(pIrs);

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pIrs"></param>
        private void CheckVR_IRD(string pCS, IInterestRateStream[] pIrs)
        {

            for (int i = 0; i < ArrFunc.Count(pIrs); i++)
            {
                if (0 == i)
                {
                    CheckVR_IRDDate(pCS, pIrs[i], null);
                    CheckVR_IRDStream(pIrs[i]);
                }
                else
                {
                    CheckVR_IRDDate(pCS, pIrs[i], pIrs[i - 1]);
                    CheckVR_IRDStream(pIrs[i]);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pIrsCurrent"></param>
        /// <param name="pIrsPrevious"></param>
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        private void CheckVR_IRDDate(string pCS, IInterestRateStream pIrsCurrent, IInterestRateStream pIrsPrevious)
        {

            //20061201 PL Rajouter ctrl sur stubdate, firstdate, ...
            DateTime dtTransac = m_Input.DataDocument.CurrentTrade.TradeHeader.TradeDate.DateValue;
            DateTime dtEffectiveDate = pIrsCurrent.CalculationPeriodDates.EffectiveDateAdjustable.UnadjustedDate.DateValue;
            //FI 20091223 [16471] add test sur terminationDateAdjustableSpecified
            DateTime dtTerminationDate = DateTime.MinValue;
            if (pIrsCurrent.CalculationPeriodDates.TerminationDateAdjustableSpecified)
                dtTerminationDate = pIrsCurrent.CalculationPeriodDates.TerminationDateAdjustable.UnadjustedDate.DateValue;
            //
            string[] idA = GetParties(pIrsCurrent, true);
            string[] idC = null;
            if (StrFunc.IsFilled(pIrsCurrent.GetCurrency))
                idC = new string[] { pIrsCurrent.GetCurrency };
            //
            IBusinessCenters bcs = m_Input.DataDocument.CurrentProduct.ProductBase.LoadBusinessCenters(pCS, null, idA, idC, null);
            IBusinessDayAdjustments bda = bcs.GetBusinessDayAdjustments(BusinessDayConventionEnum.FOLLOWING);
            IBusinessDayAdjustments bdaWithoutBC = m_Input.DataDocument.CurrentProduct.ProductBase.CreateBusinessDayAdjustments(BusinessDayConventionEnum.FOLLOWING);

            EFS_BusinessCenters efs_bc = new EFS_BusinessCenters(pCS, bcs, m_Input.DataDocument);
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
                int offset = Convert.ToInt32(SqlInstrument.GetFirstRowColumnValue("STAAFTTRANSMIN"));
                bool isBusinness = BoolFunc.IsTrue(SqlInstrument.GetFirstRowColumnValue("STAAFTTRANSMINB").ToString());
                IInterval interval = m_Input.DataDocument.CurrentProduct.ProductBase.CreateInterval(PeriodEnum.D, offset);
                DateTime dtMinEffective = Tools.ApplyAdjustedInterval(pCS, dtTransac, interval, isBusinness ? bda : bdaWithoutBC, m_Input.DataDocument);
                if (0 > dtEffectiveDate.CompareTo(dtMinEffective))
                    SetValidationRuleError("Msg_ValidationRule_DtEffective2", new string[] { 
                            DtFunc.DateTimeToString(dtTransac, DtFunc.FmtShortDate), 
                            DtFunc.DateTimeToString(dtEffectiveDate, DtFunc.FmtShortDate), 
                            offset.ToString() });
            }
            //
            if (IsToCheck("VRSTAAFTTRANSMAX"))
            {
                int offset = Convert.ToInt32(SqlInstrument.GetFirstRowColumnValue("STAAFTTRANSMAX"));
                bool isBusinness = BoolFunc.IsTrue(SqlInstrument.GetFirstRowColumnValue("STAAFTTRANSMAXB").ToString());
                IInterval interval = m_Input.DataDocument.CurrentProduct.ProductBase.CreateInterval(PeriodEnum.D, offset);
                DateTime dtMaxEffective = Tools.ApplyAdjustedInterval(pCS, dtTransac, interval, isBusinness ? bda : bdaWithoutBC, m_Input.DataDocument);
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
                int offset = Convert.ToInt32(SqlInstrument.GetFirstRowColumnValue("ENDAFTSTAMIN"));
                bool isBusinness = BoolFunc.IsTrue(SqlInstrument.GetFirstRowColumnValue("ENDAFTSTAMINB").ToString());
                IInterval interval = m_Input.DataDocument.CurrentProduct.ProductBase.CreateInterval(PeriodEnum.D, offset);
                DateTime dtMinTermination = Tools.ApplyAdjustedInterval(pCS, dtEffectiveDate, interval, isBusinness ? bda : bdaWithoutBC, m_Input.DataDocument);
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
                int offset = Convert.ToInt32(SqlInstrument.GetFirstRowColumnValue("ENDAFTSTAMAX"));
                bool isBusinness = BoolFunc.IsTrue(SqlInstrument.GetFirstRowColumnValue("ENDAFTSTAMAXB").ToString());
                IInterval interval = m_Input.DataDocument.CurrentProduct.ProductBase.CreateInterval(PeriodEnum.D, offset);
                DateTime dtMaxTermination = Tools.ApplyAdjustedInterval(pCS, dtEffectiveDate, interval, isBusinness ? bda : bdaWithoutBC, m_Input.DataDocument);
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
                if (pIrsCurrent.CalculationPeriodDates.FirstPeriodStartDateSpecified)
                {
                    if (Tools.IsBdcNoneOrNA(pIrsCurrent.CalculationPeriodDates.EffectiveDateAdjustable.DateAdjustments.BusinessDayConvention) &&
                        Tools.IsBdcNoneOrNA(pIrsCurrent.CalculationPeriodDates.FirstPeriodStartDate.DateAdjustments.BusinessDayConvention))
                    {
                        DateTime dtFirstPeriodStartDate = pIrsCurrent.CalculationPeriodDates.FirstPeriodStartDate.UnadjustedDate.DateValue;
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
                if (pIrsCurrent.CalculationPeriodDates.FirstRegularPeriodStartDateSpecified)
                {
                    DateTime dtFirstRegularPeriodStartDate = pIrsCurrent.CalculationPeriodDates.FirstRegularPeriodStartDate.DateValue;
                    //
                    if (pIrsCurrent.CalculationPeriodDates.FirstPeriodStartDateSpecified &&
                        Tools.IsBdcNoneOrNA(pIrsCurrent.CalculationPeriodDates.FirstPeriodStartDate.DateAdjustments.BusinessDayConvention))
                    {
                        DateTime dtFirstPeriodStartDate = pIrsCurrent.CalculationPeriodDates.FirstPeriodStartDate.UnadjustedDate.DateValue;
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
                    if (pIrsCurrent.CalculationPeriodDates.LastRegularPeriodEndDateSpecified)
                    {
                        if (0 > pIrsCurrent.CalculationPeriodDates.LastRegularPeriodEndDate.DateValue.CompareTo(dtFirstRegularPeriodStartDate))
                            SetValidationRuleError("Msg_ValidationRule_FirstRegularPeriodStartDate3", new string[] {  
                                    DtFunc.DateTimeToString(pIrsCurrent.CalculationPeriodDates.LastRegularPeriodEndDate.DateValue, DtFunc.FmtShortDate),
                                    DtFunc.DateTimeToString(dtFirstRegularPeriodStartDate, DtFunc.FmtShortDate)});
                    }
                }
            }
            //
            // 20090617 RD ( Pour un Repo la date d'échéance n'est pas obligatoire)
            if (DtFunc.IsDateTimeFilled(dtTerminationDate) && IsCheckError) // systematique en mode Erreur
            {
                if (pIrsCurrent.CalculationPeriodDates.LastRegularPeriodEndDateSpecified)
                {
                    DateTime dtLastRegularPeriodEndDate = pIrsCurrent.CalculationPeriodDates.LastRegularPeriodEndDate.DateValue;
                    if (0 >= dtTerminationDate.CompareTo(dtLastRegularPeriodEndDate))
                        SetValidationRuleError("Msg_ValidationRule_LastRegularPeriodStartDate", new string[] {  
                                    DtFunc.DateTimeToString(dtLastRegularPeriodEndDate, DtFunc.FmtShortDate),
                                    DtFunc.DateTimeToString(dtTerminationDate, DtFunc.FmtShortDate) });
                }
            }
            //
            if (IsCheckError) // systematique en mode Erreur
            {
                if (pIrsCurrent.CalculationPeriodAmount.CalculationSpecified)
                {
                    ICalculation cal = pIrsCurrent.CalculationPeriodAmount.Calculation;
                    if ((cal.NotionalSpecified))
                    {
                        if (cal.Notional.StepSchedule.StepSpecified)
                        {
                            IStep[] step = cal.Notional.StepSchedule.Step;
                            if (ArrFunc.IsFilled(step))
                            {
                                if (0 >= step[0].StepDate.DateValue.CompareTo(dtEffectiveDate))
                                    SetValidationRuleError("Msg_ValidationRule_StepInvalid", GetIdAdditionalInfo(cal.Notional.StepSchedule.Id));
                                //
                                // 20090617 RD ( Pour un Repo la date d'échéance n'est pas obligatoire)
                                if (DtFunc.IsDateTimeFilled(dtTerminationDate) && 0 >= dtTerminationDate.CompareTo(step[step.Length - 1].StepDate.DateValue))
                                    SetValidationRuleError("Msg_ValidationRule_StepInvalid", GetIdAdditionalInfo(cal.Notional.StepSchedule.Id));
                            }
                            //
                            if (cal.Notional.StepParametersSpecified)
                            {
                                _ = cal.Notional.StepParameters;
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
                    if (false == (dtEffectiveDate.CompareTo(pIrsPrevious.CalculationPeriodDates.EffectiveDateAdjustable.UnadjustedDate.DateValue) == 0))
                        SetValidationRuleError("Msg_ValidationRule_DifferentDtEffective");
                }
                // 20090617 RD ( Pour un Repo la date d'échéance n'est pas obligatoire)
                if (IsToCheck("VRSTREAMEND") && DtFunc.IsDateTimeFilled(dtTerminationDate))
                {
                    if (false == (dtTerminationDate.CompareTo(pIrsPrevious.CalculationPeriodDates.TerminationDateAdjustable.UnadjustedDate.DateValue) == 0))
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
        private void CheckVR_IRDStream(IInterestRateStream pIrsCurrent)
        {

            if (StrFunc.IsFilled(pIrsCurrent.PayerPartyReference.HRef) &&
                StrFunc.IsFilled(pIrsCurrent.ReceiverPartyReference.HRef))
            {
                if ((0 == pIrsCurrent.PayerPartyReference.HRef.CompareTo(pIrsCurrent.ReceiverPartyReference.HRef)))
                    SetValidationRuleError("Msg_ValidationRule_IdenticPayerReceiver1");
            }
            //Check Reset object
            if (pIrsCurrent.CalculationPeriodAmount.CalculationSpecified)
            {
                ICalculation calculation = pIrsCurrent.CalculationPeriodAmount.Calculation;
                if (calculation.RateFixedRateSpecified && (pIrsCurrent.ResetDatesSpecified))
                    SetValidationRuleError("Msg_ValidationRule_Instr_ResetOnFixedRate");
                if ((calculation.RateFloatingRateSpecified || calculation.RateInflationRateSpecified) &&
                    (false == pIrsCurrent.ResetDatesSpecified))
                    SetValidationRuleError("Msg_ValidationRule_Instr_NoResetOnFloatingRate");
            }
            //
            if (false == pIrsCurrent.PrincipalExchangesSpecified)
            {
                //TODO
            }
            //
            if ((PeriodEnum.T == pIrsCurrent.CalculationPeriodDates.CalculationPeriodFrequency.Interval.Period) &&
                (1 != pIrsCurrent.CalculationPeriodDates.CalculationPeriodFrequency.Interval.PeriodMultiplier.IntValue))
                SetValidationRuleError("Msg_ValidationRule_PeriodMultiplierTerme");

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pFxLegCurrent"></param>
        /// <param name="pFxLegPrevious"></param>
        /// EG 20150331 [POC] FxLeg
        private void CheckVR_FXLeg(string pCS, IFxLeg pFxLegCurrent, IFxLeg pFxLegPrevious)
        {

            bool isOk = true;
            //
            if (IsCheckError) // systematique en mode Erreur
            {
                if (null == pFxLegPrevious)
                {
                    FxLegContainer _fxLegContainer = new FxLegContainer(pFxLegCurrent, m_Input.DataDocument);
                    _fxLegContainer.InitRptSide(pCS, m_Input.IsAllocation);
                }

                if (StrFunc.IsFilled(pFxLegCurrent.ExchangedCurrency1.PayerPartyReference.HRef) &&
                    StrFunc.IsFilled(pFxLegCurrent.ExchangedCurrency1.ReceiverPartyReference.HRef))
                    isOk = (0 != pFxLegCurrent.ExchangedCurrency1.PayerPartyReference.HRef.CompareTo(pFxLegCurrent.ExchangedCurrency1.ReceiverPartyReference.HRef));
                //		
                if (isOk)
                {
                    if (StrFunc.IsFilled(pFxLegCurrent.ExchangedCurrency2.PayerPartyReference.HRef) &&
                        StrFunc.IsFilled(pFxLegCurrent.ExchangedCurrency2.ReceiverPartyReference.HRef))
                        isOk = (0 != pFxLegCurrent.ExchangedCurrency2.PayerPartyReference.HRef.CompareTo(pFxLegCurrent.ExchangedCurrency2.ReceiverPartyReference.HRef));
                }
                //
                if (false == isOk)
                    SetValidationRuleError("Msg_ValidationRule_IdenticPayerReceiver1");
            }
            //	
            if (IsToCheck("VRFXCTRV"))
            {
                isOk = CompareAmount(pCS, out decimal exchangeAmountRounded, BoolFunc.IsTrue(Convert.ToInt32(SqlInstrument.GetFirstRowColumnValue("ISFXCTRVEXCEPTDEC"))),
                    pFxLegCurrent.ExchangeRate.QuotedCurrencyPair.QuoteBasis,
                    pFxLegCurrent.ExchangeRate.Rate.DecValue,
                    pFxLegCurrent.ExchangedCurrency1.PaymentAmount.Amount.DecValue, pFxLegCurrent.ExchangedCurrency1.PaymentAmount.Currency,
                    pFxLegCurrent.ExchangedCurrency2.PaymentAmount.Amount.DecValue, pFxLegCurrent.ExchangedCurrency2.PaymentAmount.Currency);
                if (false == isOk)
                    SetValidationRuleError("Msg_ValidationRule_MtCtrInvalid", new string[] {  
                            StrFunc.FmtAmountToGUI(StrFunc.FmtDecimalToInvariantCulture(pFxLegCurrent.ExchangedCurrency1.PaymentAmount.Amount.DecValue)),
                            StrFunc.FmtAmountToGUI(StrFunc.FmtDecimalToInvariantCulture(exchangeAmountRounded)) });
            }
            //				
            if (IsToCheck("VRFORWARDPOINTS"))
            {
                if (pFxLegCurrent.ExchangeRate.SpotRateSpecified && (false == pFxLegCurrent.ExchangeRate.ForwardPointsSpecified))
                    SetValidationRuleError("Msg_ValidationRule_NoForwardPoints");
            }
            //
            if (IsToCheck("VRSPOTRATE"))
            {
                if (false == pFxLegCurrent.ExchangeRate.SpotRateSpecified && (pFxLegCurrent.ExchangeRate.ForwardPointsSpecified))
                    SetValidationRuleError("Msg_ValidationRule_NoSpot");
            }
            //
            if (IsToCheck("VRFXRATE"))
            {
                if (pFxLegCurrent.ExchangeRate.SpotRateSpecified && pFxLegCurrent.ExchangeRate.ForwardPointsSpecified)
                {
                    if (false == (pFxLegCurrent.ExchangeRate.Rate.DecValue == (pFxLegCurrent.ExchangeRate.SpotRate.DecValue + pFxLegCurrent.ExchangeRate.ForwardPoints.DecValue)))
                        SetValidationRuleError("Msg_ValidationRule_ExchangeRateInvalid");
                }
            }
            //
            if (pFxLegPrevious != null)
            {
                if (IsToCheck("VRFXSWAPAMOUNTIDC1"))
                {
                    if (pFxLegPrevious.ExchangedCurrency1.PaymentAmount.Amount.DecValue != pFxLegCurrent.ExchangedCurrency1.PaymentAmount.Amount.DecValue)
                        SetValidationRuleError("Msg_ValidationRule_DifferentExchangedCurrency1");
                }
            }
            //
            CheckVR_FXLegDate(pCS, pFxLegCurrent, pFxLegPrevious);
            CheckVR_FXLegPayment(pFxLegCurrent, pFxLegPrevious);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pFxLegCurrent"></param>
        /// <param name="pFxLegPrevious"></param>
        private void CheckVR_FXLegDate(string pCS, IFxLeg pFxLegCurrent, IFxLeg pFxLegPrevious)
        {

            string[] idA = GetParties(pFxLegCurrent.ExchangedCurrency1, true);
            string[] idC = null;
            ArrayList al = new ArrayList();
            if (StrFunc.IsFilled(pFxLegCurrent.ExchangedCurrency1.PaymentAmount.Currency))
                al.Add(pFxLegCurrent.ExchangedCurrency1.PaymentAmount.Currency);
            if (StrFunc.IsFilled(pFxLegCurrent.ExchangedCurrency2.PaymentAmount.Currency))
                al.Add(pFxLegCurrent.ExchangedCurrency2.PaymentAmount.Currency);
            if (ArrFunc.IsFilled(al))
                idC = (string[])al.ToArray(typeof(string));
            //
            IBusinessCenters bcs = m_Input.DataDocument.CurrentProduct.ProductBase.LoadBusinessCenters(pCS, null, idA, idC, null);
            IBusinessDayAdjustments bda = bcs.GetBusinessDayAdjustments(BusinessDayConventionEnum.FOLLOWING);
            IBusinessDayAdjustments bdaWithoutBC = m_Input.DataDocument.CurrentProduct.ProductBase.CreateBusinessDayAdjustments(BusinessDayConventionEnum.FOLLOWING);
            //20061201 PL Rajouter ctrl sur fixingdate, ...
            DateTime dtTransac = m_Input.DataDocument.CurrentTrade.TradeHeader.TradeDate.DateValue;
            DateTime fxDateValue = DateTime.MinValue;
            //
            if (pFxLegCurrent.FxDateValueDateSpecified)
                fxDateValue = pFxLegCurrent.FxDateValueDate.DateValue;
            //
            if (DtFunc.IsDateTimeFilled(fxDateValue))
            {
                EFS_BusinessCenters efs_bc = new EFS_BusinessCenters(pCS, bcs, m_Input.DataDocument);
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
                    if (IsToCheck("VRENDHOLIDAY") && m_Input.DataDocument.CurrentProduct.IsFxSwap && (pFxLegPrevious != null))
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
                        int offset = Convert.ToInt32(SqlInstrument.GetFirstRowColumnValue("STAAFTTRANSMIN"));
                        bool isBusinness = BoolFunc.IsTrue(SqlInstrument.GetFirstRowColumnValue("STAAFTTRANSMINB").ToString());
                        IInterval interval = m_Input.DataDocument.CurrentTrade.Product.ProductBase.CreateInterval(PeriodEnum.D, offset);
                        DateTime dtMinEffective = Tools.ApplyAdjustedInterval(pCS, dtTransac, interval, isBusinness ? bda : bdaWithoutBC, m_Input.DataDocument);
                        if (0 > fxDateValue.CompareTo(dtMinEffective))
                            SetValidationRuleError("Msg_ValidationRule_DtEffective2", new string[] { 
                                    DtFunc.DateTimeToString(dtTransac, DtFunc.FmtShortDate), 
                                    DtFunc.DateTimeToString(fxDateValue, DtFunc.FmtShortDate), 
                                    offset.ToString() });
                    }
                    if (IsToCheck("VRSTAAFTTRANSMAX"))
                    {
                        int offset = Convert.ToInt32(SqlInstrument.GetFirstRowColumnValue("STAAFTTRANSMAX"));
                        bool isBusinness = BoolFunc.IsTrue(SqlInstrument.GetFirstRowColumnValue("STAAFTTRANSMAXB").ToString());
                        IInterval interval = m_Input.DataDocument.CurrentTrade.Product.ProductBase.CreateInterval(PeriodEnum.D, offset);
                        DateTime dtMaxEffective = Tools.ApplyAdjustedInterval(pCS, dtTransac, interval, isBusinness ? bda : bdaWithoutBC, m_Input.DataDocument);
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
                        int offset = Convert.ToInt32(SqlInstrument.GetFirstRowColumnValue("ENDAFTSTAMIN"));
                        bool isBusinness = BoolFunc.IsTrue(SqlInstrument.GetFirstRowColumnValue("ENDAFTSTAMINB").ToString());
                        IInterval interval = m_Input.DataDocument.CurrentTrade.Product.ProductBase.CreateInterval(PeriodEnum.D, offset);
                        DateTime dateValue = DateTime.MinValue;
                        if (pFxLegPrevious.FxDateValueDateSpecified)
                            dateValue = pFxLegCurrent.FxDateValueDate.DateValue;
                        //
                        if (DtFunc.IsDateTimeFilled(dateValue))
                        {
                            DateTime dtMinEndDate = Tools.ApplyAdjustedInterval(pCS, dateValue, interval, isBusinness ? bda : bdaWithoutBC, m_Input.DataDocument);
                            if (0 > fxDateValue.CompareTo(dtMinEndDate))
                                SetValidationRuleError("Msg_ValidationRule_DtTermination2", new string[] { 
                                        DtFunc.DateTimeToString(dateValue, DtFunc.FmtShortDate), 
                                        DtFunc.DateTimeToString(fxDateValue, DtFunc.FmtShortDate), 
                                        offset.ToString() });
                        }
                    }
                    if (IsToCheck("VRENDAFTSTAMAX"))
                    {
                        int offset = Convert.ToInt32(SqlInstrument.GetFirstRowColumnValue("ENDAFTSTAMAX"));
                        bool isBusinness = BoolFunc.IsTrue(SqlInstrument.GetFirstRowColumnValue("ENDAFTSTAMAXB").ToString());
                        IInterval interval = m_Input.DataDocument.CurrentTrade.Product.ProductBase.CreateInterval(PeriodEnum.D, offset);
                        DateTime dateValue = DateTime.MinValue;
                        if (pFxLegPrevious.FxDateValueDateSpecified)
                            dateValue = pFxLegCurrent.FxDateValueDate.DateValue;
                        //
                        if (DtFunc.IsDateTimeFilled(dateValue))
                        {
                            DateTime dtMaxEndDate = Tools.ApplyAdjustedInterval(pCS, dateValue, interval, isBusinness ? bda : bdaWithoutBC, m_Input.DataDocument);
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
                    if (pFxLegPrevious.FxDateValueDateSpecified)
                    {
                        DateTime fxDateValuePrevious = pFxLegPrevious.FxDateValueDate.DateValue;
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
                if (pFxLegCurrent.ExchangedCurrency1.PaymentAmount.Currency == pFxLegCurrent.ExchangedCurrency2.PaymentAmount.Currency)
                    SetValidationRuleError("Msg_ValidationRule_IdenticCurrency");
            }
            //
            if (pFxLegPrevious != null)
            {
                // si les payeurs sont différents => les devises doivent être identiques
                // si les payeurs sont différents => MT1 doit être identique sur chaque Leg
                if (IsCheckError) // systematique en mode Erreur
                {
                    if (pFxLegPrevious.ExchangedCurrency1.PayerPartyReference != pFxLegCurrent.ExchangedCurrency1.PayerPartyReference)
                    {
                        if (pFxLegPrevious.ExchangedCurrency1.PaymentAmount.Currency != pFxLegCurrent.ExchangedCurrency1.PaymentAmount.Currency)
                            SetValidationRuleError("Msg_ValidationRule_DifferentCurrency");
                        //if (pFxLegPrevious.exchangedCurrency1.paymentAmount.amount.DecValue != pFxLegCurrent.exchangedCurrency1.paymentAmount.amount.DecValue)
                        //    SetValidationRuleError("Msg_ValidationRule_DifferentExchangedCurrency1");
                    }
                }
                //
                if (IsCheckError) // systematique en mode Erreur
                {
                    if (pFxLegPrevious.ExchangedCurrency2.PayerPartyReference != pFxLegCurrent.ExchangedCurrency2.PayerPartyReference)
                    {
                        if (pFxLegPrevious.ExchangedCurrency2.PaymentAmount.Currency != pFxLegCurrent.ExchangedCurrency2.PaymentAmount.Currency)
                            SetValidationRuleError("Msg_ValidationRule_DifferentCurrency");
                    }
                }
                //
                if (IsCheckError) // systematique en mode Erreur
                {
                    if (pFxLegCurrent.ExchangeRate.SpotRateSpecified)
                    {
                        if (pFxLegCurrent.ExchangeRate.SpotRate.DecValue != pFxLegPrevious.ExchangeRate.Rate.DecValue)
                            SetValidationRuleError("Msg_ValidationRule_DifferentSpotAndRate");
                    }
                }
            }

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pFxOptionLeg"></param>
        /// EG 20150331 [POC] FxLeg|FxOptionLeg 
        private void CheckVR_FXOption(string pCS, object pFxOptionLeg)
        {
            IReference buyer = ((IFxOptionBase)pFxOptionLeg).BuyerPartyReference;
            IReference seller = ((IFxOptionBase)pFxOptionLeg).SellerPartyReference;
            //
            if (IsCheckError) // systematique en mode Erreur
            {
                if (m_Input.DataDocument.CurrentProduct.IsFxSimpleOption)
                {
                    FxOptionLegContainer _fxOptionLegContainer = new FxOptionLegContainer((IFxOptionLeg)pFxOptionLeg, m_Input.DataDocument);
                    _fxOptionLegContainer.InitRptSide(pCS, m_Input.IsAllocation);
                }

                if (StrFunc.IsFilled(seller.HRef) && StrFunc.IsFilled(buyer.HRef))
                {
                    if (false == (0 != seller.HRef.CompareTo(buyer.HRef)))
                        SetValidationRuleError("Msg_ValidationRule_IdenticBuyerSeller");
                }
            }
            //
            if ((false == m_Input.DataDocument.CurrentProduct.IsFxDigitalOption))
            {
                IMoney callCurrencyAmount = ((IFxOptionBase)pFxOptionLeg).CallCurrencyAmount;
                IMoney putCurrencyAmount = ((IFxOptionBase)pFxOptionLeg).PutCurrencyAmount;
                IFxStrikePrice fxStrikePrice = ((IFxOptionBaseNotDigital)pFxOptionLeg).FxStrikePrice;
                //
                if (IsCheckError) // systematique en mode Erreur
                {
                    if (false == (0 != callCurrencyAmount.Currency.CompareTo(putCurrencyAmount.Currency)))
                        SetValidationRuleError("Msg_ValidationRule_IdenticCurrency");
                }
                //
                if (IsToCheck("VRFXCTRV"))
                {
                    bool isOk = CompareAmount(pCS, out decimal exchangeAmountRounded, BoolFunc.IsTrue(Convert.ToInt32(SqlInstrument.GetFirstRowColumnValue("ISFXCTRVEXCEPTDEC"))),
            (fxStrikePrice.StrikeQuoteBasis == StrikeQuoteBasisEnum.CallCurrencyPerPutCurrency) ? QuoteBasisEnum.Currency1PerCurrency2 : QuoteBasisEnum.Currency2PerCurrency1,
            fxStrikePrice.Rate.DecValue, callCurrencyAmount.Amount.DecValue, callCurrencyAmount.Currency, putCurrencyAmount.Amount.DecValue, putCurrencyAmount.Currency);
                    if (false == isOk)
                        SetValidationRuleError("Msg_ValidationRule_MtCtrInvalid", new string[] {   
                                StrFunc.FmtAmountToGUI(StrFunc.FmtDecimalToInvariantCulture(callCurrencyAmount.Amount.DecValue)),
                                StrFunc.FmtAmountToGUI(StrFunc.FmtDecimalToInvariantCulture(exchangeAmountRounded)) });
                }
            }
            //
            IFxOptionPremium[] fxOptionPremium = ((IFxOptionBase)pFxOptionLeg).FxOptionPremium;
            if (ArrFunc.IsFilled(fxOptionPremium))
            {
                for (int i = 0; i < ArrFunc.Count(fxOptionPremium); i++)
                {
                    if (IsCheckError) // systematique en mode Erreur
                    {
                        if (StrFunc.IsFilled(fxOptionPremium[i].PayerPartyReference.HRef) &&
                            StrFunc.IsFilled(fxOptionPremium[i].ReceiverPartyReference.HRef))
                        {
                            if (false == (0 != fxOptionPremium[i].PayerPartyReference.HRef.CompareTo(fxOptionPremium[i].ReceiverPartyReference.HRef)))
                                SetValidationRuleError("Msg_ValidationRule_FxOptionPremiumIdenticPayerReceiver");
                        }
                    }
                    //						
                    if (IsToCheck("VRPREMIUMPAYER"))
                    {
                        if (StrFunc.IsFilled(fxOptionPremium[i].PayerPartyReference.HRef) &&
                            StrFunc.IsFilled(buyer.HRef))
                        {
                            if (false == (0 == fxOptionPremium[i].PayerPartyReference.HRef.CompareTo(buyer.HRef)))
                                SetValidationRuleError("Msg_ValidationRule_FxOptionPremiumPayerInvalid");
                        }
                    }
                    //
                    if (IsToCheck("VRFXPREMIUM"))
                    {
                        if ((false == m_Input.DataDocument.CurrentProduct.IsFxDigitalOption)
                            && fxOptionPremium[i].PremiumQuoteSpecified
                            && (fxOptionPremium[i].PremiumQuote.PremiumQuoteBasis != PremiumQuoteBasisEnum.Explicit))
                        {
                            IMoney callCurrencyAmount = ((IFxOptionBase)pFxOptionLeg).CallCurrencyAmount;
                            IMoney putCurrencyAmount = ((IFxOptionBase)pFxOptionLeg).PutCurrencyAmount;
                            decimal amount = 0;
                            string cur = string.Empty;
                            switch (fxOptionPremium[i].PremiumQuote.PremiumQuoteBasis)
                            {
                                case PremiumQuoteBasisEnum.PercentageOfCallCurrencyAmount:
                                    amount = fxOptionPremium[i].PremiumQuote.PremiumValue.DecValue * callCurrencyAmount.Amount.DecValue;
                                    cur = callCurrencyAmount.Currency;
                                    break;
                                case PremiumQuoteBasisEnum.PercentageOfPutCurrencyAmount:
                                    amount = fxOptionPremium[i].PremiumQuote.PremiumValue.DecValue * putCurrencyAmount.Amount.DecValue;
                                    cur = putCurrencyAmount.Currency;
                                    break;
                                case PremiumQuoteBasisEnum.CallCurrencyPerPutCurrency:
                                    amount = fxOptionPremium[i].PremiumQuote.PremiumValue.DecValue * putCurrencyAmount.Amount.DecValue;
                                    cur = callCurrencyAmount.Currency;
                                    break;
                                case PremiumQuoteBasisEnum.PutCurrencyPerCallCurrency:
                                    amount = fxOptionPremium[i].PremiumQuote.PremiumValue.DecValue * callCurrencyAmount.Amount.DecValue;
                                    cur = putCurrencyAmount.Currency;
                                    break;
                            }
                            EFS_Cash cash = new EFS_Cash(pCS, amount, cur);
                            //
                            if (false == ((0 == fxOptionPremium[i].PremiumAmount.Currency.CompareTo(cur)) &&
                                (0 == fxOptionPremium[i].PremiumAmount.Amount.DecValue.CompareTo(cash.AmountRounded))))
                            {
                                string valueTheo = StrFunc.FmtAmountToGUI(StrFunc.FmtDecimalToInvariantCulture(cash.AmountRounded)) + Cst.Space + cur;
                                string mtPrime = StrFunc.FmtAmountToGUI(StrFunc.FmtDecimalToInvariantCulture(fxOptionPremium[i].PremiumAmount.Amount.DecValue)) + Cst.Space + fxOptionPremium[i].PremiumAmount.Currency;
                                //
                                SetValidationRuleError("Msg_ValidationRule_FxOptionPremiumAmountInvalid", new string[] { mtPrime, valueTheo });
                            }
                        }
                    }
                }
            }
            CheckVR_FXOptionDate(pCS, pFxOptionLeg);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pFxOptionLeg"></param>
        private void CheckVR_FXOptionDate(string pCS, object pFxOptionLeg)
        {

            DateTime dtTransac = m_Input.DataDocument.CurrentTrade.TradeHeader.TradeDate.DateValue;
            DateTime dtExpiryDate = ((IFxOptionBase)pFxOptionLeg).ExpiryDateTime.ExpiryDate.DateValue;
            DateTime valueDate = ((IFxOptionBase)pFxOptionLeg).ValueDate.DateValue;
            //
            string[] idA = GetParties(pFxOptionLeg, false);
            string[] idC = null;
            ArrayList al = new ArrayList();
            if (false == m_Input.DataDocument.CurrentProduct.IsFxDigitalOption)
            {
                al.Add(((IFxOptionBase)pFxOptionLeg).CallCurrencyAmount.Currency);
                al.Add(((IFxOptionBase)pFxOptionLeg).PutCurrencyAmount.Currency);
            }
            else
            {
                IQuotedCurrencyPair qcp = ((IFxDigitalOption)pFxOptionLeg).QuotedCurrencyPair;
                al.Add(qcp.Currency1);
                al.Add(qcp.Currency2);
            }
            //
            if (ArrFunc.IsFilled(al))
                idC = (string[])al.ToArray(typeof(string));
            //
            IBusinessCenters bcs = ((IProductBase)pFxOptionLeg).LoadBusinessCenters(pCS, null, idA, idC, null);
            IBusinessDayAdjustments bda = bcs.GetBusinessDayAdjustments(BusinessDayConventionEnum.FOLLOWING);
            IBusinessDayAdjustments bdaWithoutBC = m_Input.DataDocument.CurrentTrade.Product.ProductBase.CreateBusinessDayAdjustments(BusinessDayConventionEnum.FOLLOWING);
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
            EFS_BusinessCenters efs_bc = new EFS_BusinessCenters(pCS, bcs, m_Input.DataDocument);
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
                int offset = Convert.ToInt32(SqlInstrument.GetFirstRowColumnValue("ENDAFTSTAMIN"));
                bool isBusinness = BoolFunc.IsTrue(SqlInstrument.GetFirstRowColumnValue("ENDAFTSTAMINB").ToString());
                IInterval interval = m_Input.DataDocument.CurrentTrade.Product.ProductBase.CreateInterval(PeriodEnum.D, offset);
                DateTime dtMinEnd = Tools.ApplyAdjustedInterval(pCS, dtTransac, interval, isBusinness ? bda : bdaWithoutBC, m_Input.DataDocument);
                if (0 > dtExpiryDate.CompareTo(dtMinEnd))
                    SetValidationRuleError("Msg_ValidationRule_DtTermination2", new string[] { 
                                        DtFunc.DateTimeToString(dtTransac, DtFunc.FmtShortDate), 
                                        DtFunc.DateTimeToString(dtExpiryDate, DtFunc.FmtShortDate), 
                                        offset.ToString() });
            }
            //
            if (IsToCheck("VRENDAFTSTAMAX"))
            {
                int offset = Convert.ToInt32(SqlInstrument.GetFirstRowColumnValue("ENDAFTSTAMAX"));
                bool isBusinness = BoolFunc.IsTrue(SqlInstrument.GetFirstRowColumnValue("ENDAFTSTAMAXB").ToString());
                IInterval interval = m_Input.DataDocument.CurrentTrade.Product.ProductBase.CreateInterval(PeriodEnum.D, offset);
                DateTime dtMaxEnd = Tools.ApplyAdjustedInterval(pCS, dtTransac, interval, isBusinness ? bda : bdaWithoutBC, m_Input.DataDocument);
                if (0 < dtExpiryDate.CompareTo(dtMaxEnd))
                    SetValidationRuleError("Msg_ValidationRule_DtTermination3", new string[] { 
                                        DtFunc.DateTimeToString(dtTransac, DtFunc.FmtShortDate), 
                                        DtFunc.DateTimeToString(dtExpiryDate, DtFunc.FmtShortDate), 
                                        offset.ToString() });
            }
            //
            if (IsCheckError) // systematique en mode Erreur
            {
                IFxOptionPremium[] fxOptionPremium = ((IFxOptionBase)pFxOptionLeg).FxOptionPremium;
                if (ArrFunc.IsFilled(fxOptionPremium))
                {
                    for (int i = 0; i < ArrFunc.Count(fxOptionPremium); i++)
                    {
                        if (0 > fxOptionPremium[i].PremiumSettlementDate.DateValue.CompareTo(dtTransac))
                            SetValidationRuleError("Msg_ValidationRule_FxOptionPremiumValueDate1", new string[] { 
                                    DtFunc.DateTimeToString(fxOptionPremium[i].PremiumSettlementDate.DateValue, DtFunc.FmtShortDate),
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

            if (IsToCheck("VRREPOQTYSPOTFWD") && pSaleAndRepurchaseAgreement.ForwardLegSpecified)
            {
                bool isRepoQtyExceptDec = BoolFunc.IsTrue(Convert.ToInt32(SqlInstrument.GetFirstRowColumnValue("ISREPOQTYEXCEPTDEC")));
                bool isOk = true;
                //
                for (int i = 0; i < ArrFunc.Count(pSaleAndRepurchaseAgreement.SpotLeg); i++)
                {
                    if (i < ArrFunc.Count(pSaleAndRepurchaseAgreement.ForwardLeg))
                    {
                        decimal spotAmount = pSaleAndRepurchaseAgreement.SpotLeg[i].DebtSecurityTransaction.Quantity.NotionalAmount.Amount.DecValue;
                        decimal forwardAmount = pSaleAndRepurchaseAgreement.ForwardLeg[i].DebtSecurityTransaction.Quantity.NotionalAmount.Amount.DecValue;
                        //
                        isOk = CompareAmount(isRepoQtyExceptDec, spotAmount, forwardAmount);
                        //
                        if (isOk && pSaleAndRepurchaseAgreement.SpotLeg[i].DebtSecurityTransaction.Quantity.NumberOfUnitsSpecified &&
                            pSaleAndRepurchaseAgreement.ForwardLeg[i].DebtSecurityTransaction.Quantity.NumberOfUnitsSpecified)
                        {
                            decimal spotQty = pSaleAndRepurchaseAgreement.SpotLeg[i].DebtSecurityTransaction.Quantity.NumberOfUnits.DecValue;
                            decimal forwardQty = pSaleAndRepurchaseAgreement.ForwardLeg[i].DebtSecurityTransaction.Quantity.NumberOfUnits.DecValue;
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
            for (int i = 0; i < ArrFunc.Count(pSaleAndRepurchaseAgreement.CashStream); i++)
            {
                if (pSaleAndRepurchaseAgreement.CashStream[i].CalculationPeriodDates.TerminationDateAdjustableSpecified)
                {
                    adjustableDate = pSaleAndRepurchaseAgreement.CashStream[i].CalculationPeriodDates.TerminationDateAdjustable;
                    if ((0 > adjustableDate.UnadjustedDate.DateValue.CompareTo(dtCashStreamMaxTerminationDate)) || DtFunc.IsDateTimeEmpty(dtCashStreamMaxTerminationDate))
                        dtCashStreamMaxTerminationDate = adjustableDate.UnadjustedDate.DateValue;
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
                for (int i = 0; i < ArrFunc.Count(pSaleAndRepurchaseAgreement.CashStream); i++)
                {
                    adjustableDate = pSaleAndRepurchaseAgreement.CashStream[i].CalculationPeriodDates.EffectiveDateAdjustable;
                    if ((0 > adjustableDate.UnadjustedDate.DateValue.CompareTo(dtCashStreamMinStartDate)) || DtFunc.IsDateTimeEmpty(dtCashStreamMinStartDate))
                        dtCashStreamMinStartDate = adjustableDate.UnadjustedDate.DateValue;
                }
                //
                for (int i = 0; i < ArrFunc.Count(pSaleAndRepurchaseAgreement.SpotLeg); i++)
                {
                    if (pSaleAndRepurchaseAgreement.SpotLeg[i].DebtSecurityTransaction.GrossAmount.PaymentDateSpecified)
                    {
                        adjustableDate = pSaleAndRepurchaseAgreement.SpotLeg[i].DebtSecurityTransaction.GrossAmount.PaymentDate;
                        if ((0 < adjustableDate.UnadjustedDate.DateValue.CompareTo(dtSpotMinPaymentDate)) || DtFunc.IsDateTimeEmpty(dtSpotMinPaymentDate))
                            dtSpotMinPaymentDate = adjustableDate.UnadjustedDate.DateValue;
                    }
                }
                //
                if (0 != dtCashStreamMinStartDate.CompareTo(dtSpotMinPaymentDate))
                    isOk = false;
                //
                // Pour un Repo la date d'échéance n'est pas obligatoire
                if (isOk && DtFunc.IsDateTimeFilled(dtCashStreamMaxTerminationDate) && pSaleAndRepurchaseAgreement.ForwardLegSpecified)
                {
                    for (int i = 0; i < ArrFunc.Count(pSaleAndRepurchaseAgreement.ForwardLeg); i++)
                    {
                        if (pSaleAndRepurchaseAgreement.ForwardLeg[i].DebtSecurityTransaction.GrossAmount.PaymentDateSpecified)
                        {
                            adjustableDate = pSaleAndRepurchaseAgreement.ForwardLeg[i].DebtSecurityTransaction.GrossAmount.PaymentDate;
                            if ((0 > adjustableDate.UnadjustedDate.DateValue.CompareTo(dtForwardMaxPaymentDate)) || DtFunc.IsDateTimeEmpty(dtForwardMaxPaymentDate))
                                dtForwardMaxPaymentDate = adjustableDate.UnadjustedDate.DateValue;
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
                if (1 == ArrFunc.Count(pSaleAndRepurchaseAgreement.CashStream) &&
                    (1 == ArrFunc.Count(pSaleAndRepurchaseAgreement.SpotLeg)))
                {
                    bool isRepoAmountExceptDec = BoolFunc.IsTrue(Convert.ToInt32(SqlInstrument.GetFirstRowColumnValue("ISREPOAMTEXCEPTDEC")));
                    decimal spotGrossAmount = pSaleAndRepurchaseAgreement.SpotLeg[0].DebtSecurityTransaction.GrossAmount.PaymentAmount.Amount.DecValue;

                    bool isOk;
                    if (m_Input.DataDocument.CurrentProduct.IsRepo)
                    {
                        decimal cashStreamNotional = pSaleAndRepurchaseAgreement.CashStream[0].CalculationPeriodAmount.Calculation.Notional.StepSchedule.InitialValue.DecValue;
                        isOk = CompareAmount(isRepoAmountExceptDec, spotGrossAmount, cashStreamNotional);
                        //
                        if (isOk && pSaleAndRepurchaseAgreement.ForwardLegSpecified &&
                            (1 == ArrFunc.Count(pSaleAndRepurchaseAgreement.ForwardLeg)))
                        {
                            decimal forwardGrossAmount = pSaleAndRepurchaseAgreement.ForwardLeg[0].DebtSecurityTransaction.GrossAmount.PaymentAmount.Amount.DecValue;
                            isOk = CompareAmount(isRepoAmountExceptDec, spotGrossAmount, forwardGrossAmount);
                        }
                        //
                        if (false == isOk)
                            SetValidationRuleError("Msg_ValidationRule_VRREPOAMOUNTSRATE");
                    }
                    else
                    {
                        //SaleAndRepurchaseAgreementContainer repurchaseAgreementContainer = 
                        //new SaleAndRepurchaseAgreementContainer(pSaleAndRepurchaseAgreement, m_Input.DataDocument.dataDocument);
                        SaleAndRepurchaseAgreementContainer repurchaseAgreementContainer = new SaleAndRepurchaseAgreementContainer(pSaleAndRepurchaseAgreement, m_Input.DataDocument);
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
                for (int i = 0; i < ArrFunc.Count(pSaleAndRepurchaseAgreement.SpotLeg); i++)
                {
                    //DebtSecurityTransactionContainer debtSecurityTransactionContainer =
                    //    new DebtSecurityTransactionContainer(pSaleAndRepurchaseAgreement.spotLeg[i].debtSecurityTransaction, m_Input.DataDocument.dataDocument);

                    DebtSecurityTransactionContainer debtSecurityTransactionContainer =
                        new DebtSecurityTransactionContainer(pSaleAndRepurchaseAgreement.SpotLeg[i].DebtSecurityTransaction, m_Input.DataDocument);

                    ISecurityAsset securityAsset = debtSecurityTransactionContainer.GetSecurityAssetInDataDocument();
                    // EG 20101014 Add Test (null != securityAsset)
                    if ((null != securityAsset) && securityAsset.DebtSecuritySpecified)
                    {
                        for (int j = 0; j < ArrFunc.Count(securityAsset.DebtSecurity.Stream); j++)
                        {
                            if (securityAsset.DebtSecurity.Stream[j].CalculationPeriodDates.TerminationDateAdjustableSpecified)
                            {
                                adjustableDate = securityAsset.DebtSecurity.Stream[j].CalculationPeriodDates.TerminationDateAdjustable;
                                if (0 > adjustableDate.UnadjustedDate.DateValue.CompareTo(dtCashStreamMaxTerminationDate))
                                {
                                    isOk = false;
                                    break;
                                }
                            }
                        }
                    }
                }
                //
                if (isOk && pSaleAndRepurchaseAgreement.ForwardLegSpecified)
                {
                    for (int i = 0; i < ArrFunc.Count(pSaleAndRepurchaseAgreement.ForwardLeg); i++)
                    {
                        //DebtSecurityTransactionContainer debtSecurityTransactionContainer =
                        //    new DebtSecurityTransactionContainer(pSaleAndRepurchaseAgreement.forwardLeg[i].debtSecurityTransaction, m_Input.DataDocument.dataDocument);
                        DebtSecurityTransactionContainer debtSecurityTransactionContainer =
                            new DebtSecurityTransactionContainer(pSaleAndRepurchaseAgreement.ForwardLeg[i].DebtSecurityTransaction, m_Input.DataDocument);
                        ISecurityAsset securityAsset = debtSecurityTransactionContainer.GetSecurityAssetInDataDocument();
                        // EG 20101014 Add Test (null != securityAsset)
                        if ((null != securityAsset) && securityAsset.DebtSecuritySpecified)
                        {
                            for (int j = 0; j < ArrFunc.Count(securityAsset.DebtSecurity.Stream); j++)
                            {
                                if (securityAsset.DebtSecurity.Stream[j].CalculationPeriodDates.TerminationDateAdjustableSpecified)
                                {
                                    adjustableDate = securityAsset.DebtSecurity.Stream[j].CalculationPeriodDates.TerminationDateAdjustable;
                                    if (0 > adjustableDate.UnadjustedDate.DateValue.CompareTo(dtCashStreamMaxTerminationDate))
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
        // EG 20150624 [21151]
        // EG 20150907 [31317] New Calling CheckVR_BizDtHoliday (BusinessDate, Common to ETD|EST|DST|RTS)
        // EG 20180307 [23769] Gestion dbTransaction
        // EG 20190730 Upd (Control Dates for LateTrade)
        private void CheckVR_DebtSecurityTransaction(string pCS, IDbTransaction pDbTransaction, IDebtSecurityTransaction pDebtSecurityTransaction, bool pIsToCheck_SettlementDate)
        {

            DebtSecurityTransactionContainer debtSecurityTransactionContainer = new DebtSecurityTransactionContainer(pDebtSecurityTransaction, m_Input.DataDocument);
            debtSecurityTransactionContainer.InitRptSide(pCS, m_Input.IsAllocation);

            if (IsCheckError) // systematique en mode Erreur
            {
                Nullable<int> idM = null;
                if (debtSecurityTransactionContainer.DebtSecurityTransaction.DebtSecurity.Security.ExchangeIdSpecified)
                    idM = debtSecurityTransactionContainer.DebtSecurityTransaction.DebtSecurity.Security.ExchangeId.OTCmlId;

                CheckVR_BizDtHoliday(pCS, pDbTransaction, (RptSideProductContainer)debtSecurityTransactionContainer, idM,
                    debtSecurityTransactionContainer.DebtSecurityTransaction.TrdTypeSpecified &&
                    (debtSecurityTransactionContainer.DebtSecurityTransaction.TrdType == TrdTypeEnum.LateTrade));
            }

            if (IsToCheck("VRDSEAINAMOUNT"))
            {
                //20090921 FI ctrl sur coupon couru uniquement si cleanPriceSpecified 
                if (pDebtSecurityTransaction.Price.CleanPriceSpecified)
                {
                    if ((false == pDebtSecurityTransaction.Price.AccruedInterestAmountSpecified) ||
                        StrFunc.IsEmpty(pDebtSecurityTransaction.Price.AccruedInterestAmount.Amount.Value))
                    {
                        SetValidationRuleError("Msg_ValidationRule_VRDSEAINAMOUNT");
                    }
                }
            }
            //
            bool isDseGAMExceptDec = BoolFunc.IsTrue(Convert.ToInt32(SqlInstrument.GetFirstRowColumnValue("ISDSEGAMEXCEPTDEC")));

            if (IsToCheck("VRDSEAINRATE"))
            {
                //
                // Si le Montant et le Taux du coupon sont spécifiés, on recalcul le nouveau Montant et on compare avec le montant existant
                //
                if (pDebtSecurityTransaction.Price.AccruedInterestAmountSpecified &&
                    StrFunc.IsFilled(pDebtSecurityTransaction.Price.AccruedInterestAmount.Amount.Value) &&
                    pDebtSecurityTransaction.Price.AccruedInterestRateSpecified &&
                    StrFunc.IsFilled(pDebtSecurityTransaction.Price.AccruedInterestRate.Value))
                {
                    // EG 20150624 [21151]
                    IMoney interestAmount = debtSecurityTransactionContainer.CalcAccruedInterestAmount(pCS);
                    //
                    if (false == CompareAmount(isDseGAMExceptDec, interestAmount.Amount.DecValue, pDebtSecurityTransaction.Price.AccruedInterestAmount.Amount.DecValue))
                        SetValidationRuleError("Msg_ValidationRule_VRDSEAINRATE", new string[] {  
                                StrFunc.FmtDecimalToInvariantCulture(pDebtSecurityTransaction.Price.AccruedInterestRate.DecValue),
                                StrFunc.FmtAmountToGUI(StrFunc.FmtDecimalToInvariantCulture(pDebtSecurityTransaction.Price.AccruedInterestAmount.Amount.DecValue)),
                                StrFunc.FmtAmountToGUI(StrFunc.FmtDecimalToInvariantCulture(interestAmount.Amount.DecValue))});
                }
            }

            if (IsToCheck("VRDSESETTLEMENTDT") && pIsToCheck_SettlementDate)
            {
                // Date de règlement différente de la date théorique
                //
                // 20090626 RD En cas ou orderRules n'est pas renseigné sur le Titre.
                ISecurityAsset securityAsset = debtSecurityTransactionContainer.GetSecurityAssetInDataDocument();
                DateTime dtGAMPaymentDate = new SecurityAssetContainer(securityAsset).CalcPaymentDate(pCS, m_Input.DataDocument.TradeDate);

                //DateTime dtGAMPaymentDate = debtSecurityTransactionContainer.CalcGrossAmountPaymentDate(m_Input.CS);
                if (DtFunc.IsDateTimeFilled(dtGAMPaymentDate) && (pDebtSecurityTransaction.GrossAmount.PaymentDateSpecified))
                {
                    IAdjustableDate adjustableDate = pDebtSecurityTransaction.GrossAmount.PaymentDate;
                    if ((0 != adjustableDate.UnadjustedDate.DateValue.CompareTo(dtGAMPaymentDate)))
                        SetValidationRuleError("Msg_ValidationRule_VRDSESETTLEMENTDT", new string[] { 
                                DtFunc.DateTimeToString(adjustableDate.UnadjustedDate.DateValue, DtFunc.FmtShortDate),
                                DtFunc.DateTimeToString(dtGAMPaymentDate, DtFunc.FmtShortDate)});
                }
            }
            if (IsToCheck("VRDSEGAM"))
            {
                decimal grossAmount = debtSecurityTransactionContainer.CalcGrossAmount(pCS).Amount.DecValue;
                if (false == CompareAmount(isDseGAMExceptDec, grossAmount, pDebtSecurityTransaction.GrossAmount.PaymentAmount.Amount.DecValue))
                    SetValidationRuleError("Msg_ValidationRule_VRDSEGAM", new string[] { 
                            StrFunc.FmtAmountToGUI(StrFunc.FmtDecimalToInvariantCulture(pDebtSecurityTransaction.GrossAmount.PaymentAmount.Amount.DecValue)), 
                            StrFunc.FmtAmountToGUI(StrFunc.FmtDecimalToInvariantCulture(grossAmount)) });
            }
            //

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pExchangeTradedDerivative"></param>
        /// <param name="pCS"></param>
        /// <param name="pDbTransaction"></param>
        /// FI 20140902 [XXXXX] Modify
        // EG 20150907 [31317] New Calling CheckVR_BizDtHoliday (BusinessDate, Common to ETD|EST|DST|RTS)
        // EG 20180307 [23769] Gestion dbTransaction
        private void CheckVR_ExchangeTradedDerivative(string pCS, IDbTransaction pDbTransaction, IExchangeTradedDerivative pExchangeTradedDerivative)
        {

            if (IsCheckError) // systematique en mode Erreur
            {
                ExchangeTradedDerivativeContainer exchangeTradedDerivativeContainer =
                    new ExchangeTradedDerivativeContainer(pCS, pDbTransaction, pExchangeTradedDerivative, m_Input.DataDocument);

                // Dans le cas où l'asset n'existe pas, exchangeTradedDerivativeContainer.AssetETD est valorisé tout de même
                Nullable<int> idM = null;
                bool isAssetExist = (exchangeTradedDerivativeContainer.SecurityId > 0 && (null != exchangeTradedDerivativeContainer.AssetETD));
                if (isAssetExist)
                    idM = exchangeTradedDerivativeContainer.DerivativeContract.IdMarket;

                CheckVR_BizDtHoliday(pCS, pDbTransaction, (RptSideProductContainer)exchangeTradedDerivativeContainer, idM,
                    pExchangeTradedDerivative.TradeCaptureReport.TrdTypeSpecified && (pExchangeTradedDerivative.TradeCaptureReport.TrdType == TrdTypeEnum.LateTrade));

                //FI 20140902 [XXXXX] Appel de la méthode CheckValidationRule_PositionEffect
                CheckValidationRule_PositionEffect(exchangeTradedDerivativeContainer);

            }
        }

        /// <summary>
        /// Règle de validation du return swap
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pDbTransaction"></param>
        /// <param name="pReturnSwap"></param>
        ///FI 20140902 [XXXXX] Modify
        ///FI 20150129 [20751] Modify
        // EG 20180307 [23769] Gestion dbTransaction
        private void CheckVR_ReturnSwap(string pCS, IDbTransaction pDbTransaction, IReturnSwap pReturnSwap)
        {
            if (IsCheckError) // systematique en mode Erreur
            {
                ReturnSwapContainer _returnSwapContainer = new ReturnSwapContainer(pCS, pReturnSwap, m_Input.DataDocument);
                _returnSwapContainer.InitRptSide(pCS, m_Input.IsAllocation);

                Nullable<int> idM = null;
                if (_returnSwapContainer.MainReturnLeg.Second.ExchangeIdSpecified)
                    idM = _returnSwapContainer.MainReturnLeg.Second.ExchangeId.OTCmlId;

                CheckVR_BizDtHoliday(pCS, pDbTransaction, (RptSideProductContainer)_returnSwapContainer, idM, false);

                //FI 20140902 [XXXXX] Appel de la méthode CheckValidationRule_PositionEffect
                //FI 20140904 [XXXXX] Mise en commentaire puisque RptSide n'est alimenté que partiellement on verra plus tard
                //CheckValidationRule_PositionEffect(_returnSwapContainer);

                //FI 20150129 [20751] Ajout des 3 règles de validation 
                IInterestLeg interestLeg = _returnSwapContainer.MainInterestLeg.First;
                if (null != interestLeg)
                {
                    if ((0 == interestLeg.PayerPartyReference.HRef.CompareTo(interestLeg.ReceiverPartyReference.HRef)))
                        SetValidationRuleError("Msg_ValidationRule_IdenticPayerReceiver1");

                    if ((false == interestLeg.InterestCalculation.FixedRateSpecified) && (false == interestLeg.InterestCalculation.FloatingRateSpecified))
                        SetValidationRuleError("Msg_ValidationRule_NoInterestRate");

                    if (interestLeg.InterestCalculation.FloatingRateSpecified)
                    {
                        IFloatingRateIndex floatingRateIndex = interestLeg.InterestCalculation.FloatingRate.FloatingRateIndex;
                        if ((floatingRateIndex.OTCmlId == 0) && (false == floatingRateIndex.HrefSpecified))
                        {
                            if (StrFunc.IsEmpty(floatingRateIndex.Value))
                                SetValidationRuleError("Msg_ValidationRule_NoInterestRate");
                            else
                                SetValidationRuleError("Msg_ValidationRule_FloatingRateIndex", floatingRateIndex.Value);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Règle de validation du Equity Security Transaction
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pDbTransaction"></param>
        /// <param name="pEquitySecurityTransaction"></param>
        ///FI 20140902 [XXXXX] add Method (Gestion des EquitySecurityTransaction à l'image des ETD)
        /// EG 20150907 [31317] New Calling CheckVR_BizDtHoliday (BusinessDate, Common to ETD|EST|DST|RTS)
        // EG 20180307 [23769] Gestion dbTransaction
        private void CheckVR_EquitySecurityTransaction(string pCS, IDbTransaction pDbTransaction, IEquitySecurityTransaction pEquitySecurityTransaction)
        {

            if (IsCheckError) // systematique en mode Erreur
            {
                EquitySecurityTransactionContainer equitySecurityTransactionContainer =
                    new EquitySecurityTransactionContainer(pCS, pDbTransaction, pEquitySecurityTransaction, m_Input.DataDocument);

                // Dans le cas où l'asset n'existe pas, exchangeTradedDerivativeContainer.AssetETD est valorisé tout de même
                Nullable<int> idM = null;
                bool isAssetExist = (equitySecurityTransactionContainer.SecurityId > 0 && (null != equitySecurityTransactionContainer.AssetEquity));
                if (isAssetExist)
                    idM = equitySecurityTransactionContainer.IdMarket;

                CheckVR_BizDtHoliday(pCS, pDbTransaction, (RptSideProductContainer)equitySecurityTransactionContainer, idM,
                    equitySecurityTransactionContainer.TradeCaptureReport.TrdTypeSpecified && (equitySecurityTransactionContainer.TradeCaptureReport.TrdType == TrdTypeEnum.LateTrade));

                CheckValidationRule_PositionEffect(equitySecurityTransactionContainer);

            }
        }
        /// <summary>
        /// Validation rules sur les produits (ETD|EST|DST|RTS|COMD)
        /// Contrôles dates DtTransac|DtBusiness (Jours fériés, LateTrade, ...)
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pRptSideProductContainer"></param>
        /// <param name="pIdM"></param>
        /// <param name="pIsLateTrade"></param>
        // EG 20180307 [23769] Gestion dbTransaction
        // EG 20190613 [24683] Use DbTransaction
        private void CheckVR_BizDtHoliday(string pCS, IDbTransaction pDbTransaction, RptSideProductContainer pRptSideProductContainer, Nullable<int> pIdM, bool pIsLateTrade)
        {
            //Date de transaction
            DateTime dtTransac = m_Input.DataDocument.CurrentTrade.TradeHeader.TradeDate.DateValue;
            string strDtTransac = DtFunc.DateTimeToString(dtTransac, DtFunc.FmtShortDate);

            //Date de Clearing
            DateTime dtClearingBusiness = DateTime.MinValue;
            if (DtFunc.IsDateTimeFilled(pRptSideProductContainer.ClearingBusinessDate))
                dtClearingBusiness = pRptSideProductContainer.ClearingBusinessDate;
            string strDtClearingBusiness = DtFunc.DateTimeToString(dtClearingBusiness, DtFunc.FmtShortDate);

            //Date Business courante
            DateTime dtSysBusiness = Tools.GetDateBusiness(pCS, pDbTransaction, m_Input.DataDocument);
            string strDtSysBusiness = DtFunc.DateTimeToString(dtSysBusiness, DtFunc.FmtShortDate);

            #region Holdidays sur marché
            if (pIdM.HasValue)
            {
                IBusinessCenters bcs = m_Input.DataDocument.CurrentProduct.ProductBase.LoadBusinessCenters(pCS, pDbTransaction, null, null, new string[] { pIdM.ToString() });
                EFS_BusinessCenters efs_bc = new EFS_BusinessCenters(pCS, pDbTransaction, bcs, m_Input.DataDocument);
                if (efs_bc.businessCentersSpecified)
                {
                    if (efs_bc.IsHoliday(dtClearingBusiness, DayTypeEnum.ExchangeBusiness))
                    {
                        string marketIdentifier = string.Empty;

                        SQL_Market sqlMarket = new SQL_Market(pCS, pIdM.Value)
                        {
                            DbTransaction = pDbTransaction
                        };
                        sqlMarket.LoadTable(new string[] { "IDENTIFIER" });
                        if (sqlMarket.IsLoaded)
                            marketIdentifier = sqlMarket.Identifier;

                        SetValidationRuleError("Msg_ValidationRule_ETDBizDtIsHoliday", new string[] { strDtClearingBusiness, marketIdentifier });
                    }
                }
            }
            #endregion Holdidays sur marché

            #region Date de compensation doit être >= à la date Business (présente dans la table ENTITYMARKET)
            if (m_Input.IsAllocation && pRptSideProductContainer.IsPosKeepingOnBookDealer(pCS, pDbTransaction))
            {
                if (DtFunc.IsDateTimeFilled(dtSysBusiness) && (dtClearingBusiness < dtSysBusiness))
                    SetValidationRuleError("Msg_ValidationRule_BizDtLessSysBizDt", new string[] { strDtClearingBusiness, strDtSysBusiness });
            }
            #endregion

            #region Date de transaction par rapport à la date de compensation
            if (pIsLateTrade)
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

            if (pIdM.HasValue && (pRptSideProductContainer is ExchangeTradedDerivativeContainer))
            {
                ExchangeTradedDerivativeContainer exchangeTradedDerivativeContainer = pRptSideProductContainer as ExchangeTradedDerivativeContainer;
                #region Dernier Jour de négociation du ou des actifs doit être >= à la date de transaction
                DateTime dtLastTrdDay = exchangeTradedDerivativeContainer.AssetETD.Maturity_LastTradingDay;
                if (DtFunc.IsDateTimeFilled(dtLastTrdDay) && (dtTransac > dtLastTrdDay))
                    SetValidationRuleError("Msg_ValidationRule_ETDTransacDtUpperLastTrdDay",
                        new string[] { strDtTransac, DtFunc.DateTimeToString(dtLastTrdDay, DtFunc.FmtShortDate), exchangeTradedDerivativeContainer.AssetETD.Identifier });
                #endregion

                #region Date d’expiration du ou des actifs doit être >= à la date de compensation
                DateTime dtMaturity = exchangeTradedDerivativeContainer.AssetETD.Maturity_MaturityDate;
                if (DtFunc.IsDateTimeFilled(dtMaturity) && (dtClearingBusiness > dtMaturity))
                    SetValidationRuleError("Msg_ValidationRule_ETDBizDtUpperMaturityDt",
                        new string[] { strDtClearingBusiness, DtFunc.DateTimeToString(dtMaturity, DtFunc.FmtShortDate), exchangeTradedDerivativeContainer.AssetETD.Identifier });
                #endregion
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
            ArrayList al = new ArrayList();
            if (null != pObjectPayerReceiverRef)
            {
                string prefix = pIsPayerReceiver ? "receiver" : "seller";
                FieldInfo fld = pObjectPayerReceiverRef.GetType().GetField(prefix + "PartyReference");
                IReference partyReference = (IReference)fld.GetValue(pObjectPayerReceiverRef);

                if ((null != partyReference && StrFunc.IsFilled(partyReference.HRef)))
                {
                    IParty party = m_Input.DataDocument.GetParty(partyReference.HRef);
                    if (null != party)
                        al.Add((party.OtcmlId));
                }

                prefix = pIsPayerReceiver ? "payer" : "buyer";

                fld = pObjectPayerReceiverRef.GetType().GetField(prefix + "PartyReference");
                partyReference = (IReference)fld.GetValue(pObjectPayerReceiverRef);

                if ((null != partyReference && StrFunc.IsFilled(partyReference.HRef)))
                {
                    IParty party = m_Input.DataDocument.GetParty(partyReference.HRef);
                    if (null != party)
                        al.Add((party.OtcmlId));
                }
            }
            //
            if (ArrFunc.IsFilled(al))
                ret = (string[])al.ToArray(typeof(string));
            return ret;
        }

        /// <summary>
        /// 
        /// </summary>
        private void CheckValidationRule_ExeAssAbn()
        {

            if (IsCheckError)
            {

            }

        }

        /// <summary>
        /// Validation de l'annulation d'un trade 
        /// </summary>
        /// FI 20160517 [22148] Add
        private void CheckValidationRule_RemoveAlloc(string pCS , IDbTransaction pDbTransaction )
        {
            if (IsCheckError)
            {
                // Verification que l'annulation ne porte pas sur un trade antérieur à la date business courante
                if (m_Input.IsAllocation && m_Input.IsPosKeepingOnBookDealer(pCS, pDbTransaction))
                {
                    //Date de Clearing
                    DateTime dtClearingBusiness = m_Input.ClearingBusinessDate;
                    string strDtClearingBusiness = DtFunc.DateTimeToString(dtClearingBusiness, DtFunc.FmtShortDate);
                    //
                    DateTime dtSysBusiness = this.m_Input.CurrentBusinessDate;
                    string strDtSysBusiness = DtFunc.DateTimeToString(dtSysBusiness, DtFunc.FmtShortDate);

                    if (DtFunc.IsDateTimeFilled(dtSysBusiness) && (dtClearingBusiness < dtSysBusiness))
                        SetValidationRuleError("Msg_ValidationRule_BizDtLessSysBizDt", new string[] { strDtClearingBusiness, strDtSysBusiness });
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// FI 20160517 [22148] Modify
        private void CheckValidationRule_Action(string pCS, IDbTransaction pDbTransaction)
        {

            switch (CaptureMode)
            {
                case Cst.Capture.ModeEnum.PositionCancelation:
                    //CheckCorrectionOfQuantityValidationRule ne contrôle rien pour l'instant
                    CheckCorrectionOfQuantityValidationRule chk = new CheckCorrectionOfQuantityValidationRule(this.m_Input.positionCancel);
                    string msg = chk.GetConformityMsg();
                    if (StrFunc.IsFilled(msg))
                        SetValidationRuleError("Msg_ValidationRule_CorrectionOfQuantity", msg); //Cette ressource n'existe pas 
                    break;
                case Cst.Capture.ModeEnum.OptionExercise:
                case Cst.Capture.ModeEnum.OptionAssignment:
                case Cst.Capture.ModeEnum.OptionAbandon:
                    CheckValidationRule_ExeAssAbn();
                    break;
                case Cst.Capture.ModeEnum.RemoveAllocation: // FI 20160517 [22148] Add
                    CheckValidationRule_RemoveAlloc(pCS, pDbTransaction);
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Contrôle des books
        /// </summary>
        /// <param name="pCS"></param>
        /// FI 20150730 [21156] Add method
        // EG 20180307 [23769] Gestion dbTransaction
        private void CheckValidationRule_Book(string pCS, IDbTransaction pDbTransaction)
        {

            CheckValidationRule_BookManaged(pCS, pDbTransaction);

            CheckValidationRule_BookOnDealer(pCS, pDbTransaction);

            CheckValidationRule_Book_MandatoryFees(pCS, pDbTransaction);

        }

        /// <summary>
        /// Vérification de la présence d'un book géré sur le dealer lorsque le trade est une allocation
        /// </summary>
        /// FI 20150730 [21156] Add method
        /// FI 20150930 [21403] Modify
        /// FI 20170116 [21916] Modify
        // EG 20180307 [23769] Gestion dbTransaction
        private void CheckValidationRule_BookOnDealer(string pCS, IDbTransaction pDbTransaction)
        {
            if (this.IsCheckError && m_Input.IsAllocation)
            {
                // FI 20170116 [21916]
                //RptSideProductContainer rptSide = m_Input.Product.RptSide(pCS, true);
                RptSideProductContainer rptSide = m_Input.Product.RptSide();
                if (null == rptSide)
                    throw new NotImplementedException(StrFunc.AppendFormat("product:{0} is not implemented ", m_Input.Product.ProductBase.ToString()));

                Boolean isOk = false;
                IFixParty partyDealer = rptSide.GetDealer();
                if (null != partyDealer)
                {
                    IParty dealerParty = m_Input.DataDocument.GetParty(partyDealer.PartyId.href);

                    IPartyTradeIdentifier partyTradeIdentifier = m_Input.DataDocument.GetPartyTradeIdentifier(dealerParty.Id);
                    if (null != partyTradeIdentifier)
                    {
                        if (partyTradeIdentifier.BookIdSpecified && partyTradeIdentifier.BookId.OTCmlId > 0)
                        {
                            isOk = BookTools.IsBookManaged(pCS, pDbTransaction, partyTradeIdentifier.BookId.OTCmlId);
                        }
                    }
                }

                if (false == isOk)
                    SetValidationRuleError("Msg_ValidationRule_BookDealer_IsAvailable");
            }
        }

        /// <summary>
        ///  Contrôle l'existence de frais sur les books où il est nécessaire de contrôler l'existence de frais 
        /// </summary>
        // FI 20150730 [21156] Add method
        // RD 20151012 [21442] Modify
        // EG 20180307 [23769] Gestion dbTransaction
        private void CheckValidationRule_Book_MandatoryFees(string pCS, IDbTransaction pDbTransaction)
        {
            string booksWithouFees = string.Empty;

            var partyTradeIdentifier =
                from item in m_Input.DataDocument.PartyTradeIdentifier.Where(x => x.BookIdSpecified && x.BookId.OTCmlId > 0)
                select item;

            foreach (IPartyTradeIdentifier item in partyTradeIdentifier)
            {
                SQL_Book sqlbook = new SQL_Book(pCS, item.BookId.OTCmlId, SQL_Table.ScanDataDtEnabledEnum.Yes)
                {
                    DbTransaction = pDbTransaction,
                    IsUseTable = true
                };
                // RD 20151012 [21442] Add "IDENTIFIER" column
                if (sqlbook.LoadTable(new string[] { "VRFEE,IDENTIFIER" }))
                {
                    Boolean isToControl = true;
                    if (m_Input.Product.IsExchangeTradedDerivative)
                    {
                        ExchangeTradedDerivativeContainer etd = new ExchangeTradedDerivativeContainer((IExchangeTradedDerivative)m_Input.Product.Product);
                        // PM 20130215 [18414] : Egalement pas de contôle sur Cascading
                        // EG 20130607 : Pas contrôle sur les trades ajustés suite à CA
                        isToControl = ((false == etd.IsPositionOpening) && (false == etd.IsCascading) || (false == etd.IsTradeCAAdjusted));
                    }

                    if (isToControl)
                    {
                        //Contrôle des books sans frais
                        Cst.CheckModeEnum vrFee = sqlbook.VRFee;
                        if ((vrFee == Cst.CheckModeEnum.Warning && IsCheckWarning) || (vrFee == Cst.CheckModeEnum.Error && IsCheckError))
                        {
                            // RD 20151022 [xxxxx] Add test "if (m_Input.DataDocument.otherPartyPaymentSpecified)"
                            //IPayment defaultPayment = Tools.GetNewProductBase().CreatePayment();

                            //var payment = from itemPayment in m_Input.DataDocument.otherPartyPayment.DefaultIfEmpty(defaultPayment).Where
                            //               (x => (x.payerPartyReference.hRef == item.partyReference.hRef) || (x.receiverPartyReference.hRef == item.partyReference.hRef))
                            //              select itemPayment;

                            //bool isFound = (payment.Count() > 0);

                            bool isFound = false;

                            if (m_Input.DataDocument.OtherPartyPaymentSpecified)
                            {
                                IPayment defaultPayment = Tools.GetNewProductBase().CreatePayment();

                                var payment = from itemPayment in m_Input.DataDocument.OtherPartyPayment.DefaultIfEmpty(defaultPayment).Where
                                                  (x => (x.PayerPartyReference.HRef == item.PartyReference.HRef) || (x.ReceiverPartyReference.HRef == item.PartyReference.HRef))
                                              select itemPayment;

                                isFound = (payment.Count() > 0);
                            }
                            if (!isFound)
                                booksWithouFees += sqlbook.Identifier + ";";

                        }
                    }
                }
            }

            if (StrFunc.IsFilled(booksWithouFees))
                SetValidationRuleError("Msg_ValidationRule_OPP_Book_MandatoryFees", StrFunc.StringArrayList.StringListToStringArray(booksWithouFees));

        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pDbTransaction"></param>
        /// <param name="ICommoditySpot"></param>
        /// FI 20161214 [21916] Add
        /// FI 20161214 [21916] GLOP A ENRICHIR
        /// FI 20170419 [XXXXX] Modify
        // EG 20180307 [23769] Gestion dbTransaction
        // EG 20221201 [25639] [WI484] Add Test Environmental
        // EG 20230505 [XXXXX] [WI617] DeliveryStartDateTime & DeliveryEndDateTime optional => controls for Trade template
        private void CheckVR_CommoditySpot(string pCS, IDbTransaction pDbTransaction, ICommoditySpot pCommoditySpot)
        {
            if (IsCheckError) // systematique en mode Erreur
            {
                CommoditySpotContainer commoditySpotContainer =
                    new CommoditySpotContainer(pCS, pDbTransaction, pCommoditySpot, m_Input.DataDocument);

                int idM = commoditySpotContainer.AssetCommodity.IdM;
                CheckVR_BizDtHoliday(pCS, pDbTransaction, (RptSideProductContainer)commoditySpotContainer, idM, false);

                // FI 20170419 [XXXXX] contrôle payer/receiver identique
                IPayment grossAmount = pCommoditySpot.FixedLeg.GrossAmount;
                if (StrFunc.IsFilled(grossAmount.PayerPartyReference.HRef) &&
                            StrFunc.IsFilled(grossAmount.ReceiverPartyReference.HRef))
                {
                    if ((0 == grossAmount.PayerPartyReference.HRef.CompareTo(grossAmount.ReceiverPartyReference.HRef)))
                        SetValidationRuleError("Msg_ValidationRule_IdenticPayerReceiver1", StrFunc.AppendFormat(" ({0})", Ressource.GetString("Settlement_Title")));
                }

                if (false == pCommoditySpot.IsEnvironmental)
                {
                    Nullable<DateTimeOffset> dateTimeStart = commoditySpotContainer.DeliveryStartDateTime;
                    Nullable<DateTimeOffset> dateTimeEnd = commoditySpotContainer.DeliveryEndDateTime;

                    if (dateTimeStart.HasValue && dateTimeEnd.HasValue && (dateTimeEnd.Value.CompareTo(dateTimeStart.Value) <= 0))
                        SetValidationRuleError("Msg_ValidationRule_COMD_DeliveryEndCompareToDeliveryStart");
                }

                if (pCommoditySpot.IsGas) //Prix négatif sur le gaz est impossible
                {
                    if (Math.Sign(pCommoditySpot.FixedLeg.FixedPrice.Price.DecValue) < 0)
                    {
                        SetValidationRuleError("Msg_ValidationRule_COMD_NegativePrice");
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCS"></param>
        // FI 20170928 [23452] Add
        private void ChekVR_MiFIR(string pCS)
        {
            if (IsCheckWarning) 
            {
                Boolean isApplyCheck = BoolFunc.IsFalse(SqlInstrument.GetFirstRowColumnValue("ESMAMIFIREXEMPT"));
                if (isApplyCheck)
                {
                    //Trading Capacity 
                    CheckTradingCapacity(pCS);
                    
                    //Short selling indicator
                    CheckShortSaleIndicator(pCS);
                }
            }
        }


        /// <summary>
        /// Vérification de la présence du shortSale si vente sur un instrument avec déclaration des ventes à découvert 
        /// </summary>
        /// FI 20170928 [23452] Add
        private void CheckShortSaleIndicator(string pCS)
        {
            DataDocumentContainer doc = m_Input.DataDocument;

            if (null != this.SqlInstrument.GetFirstRowColumnValue("ESMACTRLSHORTSALE") &&
                BoolFunc.IsTrue(this.SqlInstrument.GetFirstRowColumnValue("ESMACTRLSHORTSALE")))
            {
                IParty partySeller = m_Input.GetPartyMiFIR(pCS).Where(x => doc.IsPartySeller(x)).FirstOrDefault();

                if (null != partySeller)
                {
                    IBookId bookId = doc.GetBookId(partySeller.Id);
                    if (null == bookId)
                        throw new NullReferenceException(StrFunc.AppendFormat("Book is missing for party {0}", partySeller.PartyId));

                    SQL_Book sqlBook = new SQL_Book(pCS, bookId.OTCmlId);
                    if (false == sqlBook.LoadTable(new string[] { "ISPOSKEEPING" }))
                        throw new NullReferenceException(StrFunc.AppendFormat("Book (Id:{0}) does not exist", bookId.OTCmlId));

                    if (sqlBook.IsPosKeeping)
                    {
                        IPartyTradeInformation tradeInformation = doc.GetPartyTradeInformation(partySeller.Id);
                        if (false == tradeInformation.ShortSaleSpecified)
                        {
                            SetValidationRuleError("Msg_ValidationRule_MiFIR_ShortSaleIndicator", null, new string[] { partySeller.PartyId });
                        }
                    }
                }
            }
        }

        /// <summary>
        ///  Vérification que Trading capacity est renseigné
        /// </summary>
        /// FI 20170928 [23452] Add
        // EG 20171031 [23509] Upd
        private void CheckTradingCapacity(string pCS)
        {
            DataDocumentContainer doc = m_Input.DataDocument;

            IEnumerable<IParty> party = m_Input.GetPartyMiFIR(pCS);
            if (null != party)
            {
                foreach (IParty item in party)
                {
                    Boolean isFound = false;
                    IPartyTradeInformation partyTradeInformation = doc.GetPartyTradeInformation(item.Id);
                    if ((null != partyTradeInformation ) && partyTradeInformation.CategorySpecified)
                        isFound = (partyTradeInformation.Category.Where(x => x.Scheme == "http://www.fpml.org/coding-scheme/esma-mifir-trading-capacity").Count() > 0);

                    if (false == isFound)
                        SetValidationRuleError("Msg_ValidationRule_MiFIR_TradingCapacity", null, new string[] { item.PartyId });
                }
            }
        }

        #endregion Methods
    }
}